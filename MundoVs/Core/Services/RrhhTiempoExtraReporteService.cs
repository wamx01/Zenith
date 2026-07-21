using System.Globalization;
using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Core.Models;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Services;

public sealed class RrhhTiempoExtraReporteService : IRrhhTiempoExtraReporteService
{
    public async Task<RrhhTiempoExtraReporteResponse> GenerarAsync(
        IDbContextFactory<CrmDbContext> dbFactory,
        RrhhTiempoExtraReporteRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.EmpresaId == Guid.Empty)
        {
            throw new ArgumentException("EmpresaId es requerido.", nameof(request));
        }

        if (request.FechaHasta < request.FechaDesde)
        {
            throw new ArgumentException("La fecha final no puede ser menor a la fecha inicial.", nameof(request));
        }

        var agrupadoPor = string.Equals(request.AgrupadoPor, "empleado", StringComparison.OrdinalIgnoreCase)
            ? "empleado"
            : "dia";

        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

        // Usar NominaConfiguracionLoader para obtener los factores exactos que usa la nómina,
        // garantizando que el reporte coincida con lo que NominaCalculator calculará.
        var configuracionNomina = await NominaConfiguracionLoader.LoadAsync(db, request.EmpresaId);
        var factorTiempoExtra = Math.Max(0m, configuracionNomina.FactorHoraExtra);
        var factorTiempoExtraTriple = Math.Max(0m, configuracionNomina.FactorHoraExtraTriple);
        var factorAcumulacionBanco = Math.Max(0m, configuracionNomina.BancoHorasFactorAcumulacion);
        var horasBaseSemanal = Math.Max(1m, configuracionNomina.ObtenerHorasBase(PeriodicidadPago.Semanal));
        var horasBaseQuincenal = Math.Max(1m, configuracionNomina.ObtenerHorasBase(PeriodicidadPago.Quincenal));
        var horasBaseMensual = Math.Max(1m, configuracionNomina.ObtenerHorasBase(PeriodicidadPago.Mensual));

        var query = db.RrhhAsistencias
            .AsNoTracking()
            .Where(a => a.EmpresaId == request.EmpresaId
                && a.Fecha >= request.FechaDesde
                && a.Fecha <= request.FechaHasta);

        if (request.EmpleadoId.HasValue && request.EmpleadoId.Value != Guid.Empty)
        {
            query = query.Where(a => a.EmpleadoId == request.EmpleadoId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Departamento))
        {
            var departamento = request.Departamento.Trim();
            query = query.Where(a => a.Empleado.Departamento != null
                && a.Empleado.Departamento.ToLower() == departamento.ToLower());
        }

        // Proyectamos a un tipo anónimo para no cargar toda la entidad ni el grafo de navegaciones.
        // El JOIN a TurnoBase es opcional: si el empleado no tiene turno, simplemente no traeremos su nombre.
        var filas = await query
            .Select(a => new
            {
                a.EmpleadoId,
                EmpleadoNumero = a.Empleado.NumeroEmpleado,
                EmpleadoCodigoChecador = a.Empleado.CodigoChecador,
                EmpleadoNombreCompleto = a.Empleado.NombreCompleto,
                EmpleadoDepartamento = a.Empleado.Departamento,
                EmpleadoPuesto = a.Empleado.Puesto,
                EmpleadoSueldoSemanal = a.Empleado.SueldoSemanal,
                EmpleadoPeriodicidadPago = a.Empleado.PeriodicidadPago,
                a.Fecha,
                a.Estatus,
                a.MinutosExtra,
                a.MinutosExtraAutorizadosPago,
                a.MinutosExtraAutorizadosBanco,
                a.MinutosCubiertosBancoHoras,
                a.MinutosRetardo,
                a.MinutosSalidaAnticipada,
                a.MinutosTrabajadosBrutos,
                a.MinutosTrabajadosNetos,
                a.RequiereRevision,
                a.ResolucionTiempoExtra,
                a.FactorTiempoExtraAplicado,
                a.TurnoBaseId,
                a.HoraEntradaProgramada,
                a.HoraSalidaProgramada,
                a.HoraEntradaReal,
                a.HoraSalidaReal,
                a.Observaciones,
                TurnoNombre = a.TurnoBase != null ? a.TurnoBase.Nombre : null
            })
            .ToListAsync(cancellationToken);

        // Filtrar en memoria por MinutosExtraMinimos (no se puede pre-filtrar aquí porque
        // el motor también puede aprobar tiempo extra sin que MinutosExtra > 0; aun así
        // el reporte se concentra en quienes sugieren extra).
        if (request.MinutosExtraMinimos.HasValue)
        {
            var minimo = Math.Max(0, request.MinutosExtraMinimos.Value);
            filas = filas.Where(f => f.MinutosExtra > minimo).ToList();
        }

        // Fase 8 — resoluciones de tiempo extra Autorizadas que tocan el rango, con sus líneas.
        // Lo APROBADO (pago/banco/factor/monto) viene de aquí; las asistencias above son solo
        // detección diaria (contexto). Un empleado puede tener varios periodos en el rango.
        var empleadoIdsRango = filas.Select(f => f.EmpleadoId).Distinct().ToHashSet();
        var resolucionesQuery = db.RrhhResolucionesTiempoExtraPeriodo
            .AsNoTracking()
            .Include(r => r.Lineas)
            .Where(r => r.EmpresaId == request.EmpresaId
                && r.Estatus == RrhhResolucionPeriodoEstatus.Autorizada
                && r.FechaInicio <= request.FechaHasta
                && r.FechaFin >= request.FechaDesde
                && r.IsActive);
        if (empleadoIdsRango.Count > 0)
        {
            resolucionesQuery = resolucionesQuery.Where(r => empleadoIdsRango.Contains(r.EmpleadoId));
        }
        var resoluciones = await resolucionesQuery.ToListAsync(cancellationToken);
        var resolucionesPorEmpleado = resoluciones
            .GroupBy(r => r.EmpleadoId)
            .ToDictionary(g => g.Key, g => g.OrderBy(r => r.FechaInicio).ToList());

        var empleados = filas
            .GroupBy(f => f.EmpleadoId)
            .OrderBy(g => g.First().EmpleadoNombreCompleto, StringComparer.CurrentCultureIgnoreCase)
            .Select(g =>
            {
                var primero = g.OrderBy(f => f.Fecha).First();
                var sueldoReferencia = primero.EmpleadoSueldoSemanal;
                var horasBaseEmpleado = primero.EmpleadoPeriodicidadPago switch
                {
                    PeriodicidadPago.Quincenal => horasBaseQuincenal,
                    PeriodicidadPago.Mensual => horasBaseMensual,
                    _ => horasBaseSemanal
                };
                var sueldoHora = horasBaseEmpleado > 0m ? sueldoReferencia / horasBaseEmpleado : 0m;

                // Detección diaria (contexto). Los campos "aprobados" del día vienen de las
                // columnas diarias legadas (0 en el flujo por periodo); lo aprobado real está
                // en los Periodos abajo. El día muestra lo detectado + retardos + marcaciones.
                var dias = g
                    .OrderBy(f => f.Fecha)
                    .Select(f =>
                    {
                        var factorDia = f.FactorTiempoExtraAplicado.HasValue && f.FactorTiempoExtraAplicado.Value > 0m
                            ? f.FactorTiempoExtraAplicado.Value
                            : factorTiempoExtra;
                        return new RrhhTiempoExtraReporteDiaDto
                        {
                            Fecha = f.Fecha,
                            Estatus = f.Estatus.ToString(),
                            MinutosExtra = f.MinutosExtra,
                            MinutosExtraAutorizadosPago = f.MinutosExtraAutorizadosPago,
                            MinutosExtraAutorizadosBanco = f.MinutosExtraAutorizadosBanco,
                            MinutosCubiertosBancoHoras = f.MinutosCubiertosBancoHoras,
                            FactorTiempoExtra = factorDia,
                            MinutosPagoFactorado = (int)Math.Round(f.MinutosExtraAutorizadosPago * factorDia, MidpointRounding.AwayFromZero),
                            MinutosBancoFactorado = (int)Math.Round(f.MinutosExtraAutorizadosBanco * factorAcumulacionBanco, MidpointRounding.AwayFromZero),
                            HorasExtraDobles = 0m,
                            HorasExtraTriples = 0m,
                            SueldoHora = Math.Round(sueldoHora, 4),
                            MontoHorasExtraEstimado = 0m,
                            MinutosRetardo = f.MinutosRetardo,
                            MinutosSalidaAnticipada = f.MinutosSalidaAnticipada,
                            MinutosTrabajadosBrutos = f.MinutosTrabajadosBrutos,
                            MinutosTrabajadosNetos = f.MinutosTrabajadosNetos,
                            RequiereRevision = f.RequiereRevision,
                            ResolucionTiempoExtra = f.ResolucionTiempoExtra,
                            Turno = f.TurnoNombre,
                            HoraEntradaProgramada = f.HoraEntradaProgramada,
                            HoraSalidaProgramada = f.HoraSalidaProgramada,
                            HoraEntradaReal = f.HoraEntradaReal,
                            HoraSalidaReal = f.HoraSalidaReal,
                            Observaciones = f.Observaciones
                        };
                    })
                    .ToList();

                // Fase 8 — periodos autorizados con sus líneas. Dobles/triples/simples se derivan
                // del factor de cada línea (==2 dobles, ==3 triples, resto simples), ya no del
                // techo semanal. El monto estimado de una línea de PAGO = Minutos/60 × Factor × sueldoHora.
                var periodos = (resolucionesPorEmpleado.TryGetValue(primero.EmpleadoId, out var resEmpleado)
                        ? resEmpleado
                        : new List<RrhhResolucionTiempoExtraPeriodo>())
                    .Select(r =>
                    {
                        var lineasDto = (r.Lineas is null
                            ? new List<RrhhResolucionTiempoExtraLinea>()
                            : r.Lineas.OrderBy(l => l.Orden).ToList())
                            .Select(l =>
                            {
                                var minutosFactorados = (int)Math.Round(Math.Max(0, l.Minutos) * Math.Max(0m, l.Factor), MidpointRounding.AwayFromZero);
                                var monto = l.Destino == RrhhDestinoTiempoExtraLinea.Pago
                                    ? Math.Round(Math.Max(0, l.Minutos) / 60m * Math.Max(0m, l.Factor) * sueldoHora, 2)
                                    : 0m;
                                return new RrhhTiempoExtraReporteLineaDto
                                {
                                    Destino = l.Destino == RrhhDestinoTiempoExtraLinea.Banco ? "Banco" : "Pago",
                                    Minutos = Math.Max(0, l.Minutos),
                                    Factor = l.Factor,
                                    MinutosFactorados = minutosFactorados,
                                    MontoEstimado = monto,
                                    Observaciones = l.Observaciones
                                };
                            })
                            .ToList();
                        return new RrhhTiempoExtraReportePeriodoDto
                        {
                            PeriodoEtiqueta = r.PeriodoEtiqueta,
                            PeriodoKey = r.PeriodoKey,
                            FechaInicio = r.FechaInicio,
                            FechaFin = r.FechaFin,
                            Estatus = r.Estatus.ToString(),
                            AutorizadoPor = r.AutorizadoPor,
                            FechaAutorizacion = r.FechaAutorizacion,
                            Lineas = lineasDto,
                            MinutosPago = lineasDto.Where(l => l.Destino == "Pago").Sum(l => l.Minutos),
                            MinutosBanco = lineasDto.Where(l => l.Destino == "Banco").Sum(l => l.Minutos),
                            MontoEstimado = Math.Round(lineasDto.Sum(l => l.MontoEstimado), 2)
                        };
                    })
                    .ToList();

                var todasLineasPago = periodos.SelectMany(p => p.Lineas).Where(l => l.Destino == "Pago").ToList();
                var todasLineasBanco = periodos.SelectMany(p => p.Lineas).Where(l => l.Destino == "Banco").ToList();
                // Para dobles/triples resumimos por factor de línea (consistente con la resolución por líneas).
                var totalHorasDobles = todasLineasPago
                    .Where(l => l.Factor == 2m)
                    .Sum(l => l.Minutos / 60m);
                var totalHorasTriples = todasLineasPago
                    .Where(l => l.Factor == 3m)
                    .Sum(l => l.Minutos / 60m);

                var totales = new RrhhTiempoExtraReporteTotalesDto
                {
                    TotalDias = dias.Count,
                    DiasConExtra = dias.Count(d => d.MinutosExtra > 0),
                    TotalMinutosExtra = dias.Sum(d => d.MinutosExtra),
                    TotalMinutosExtraAutorizadosPago = todasLineasPago.Sum(l => l.Minutos),
                    TotalMinutosExtraAutorizadosBanco = todasLineasBanco.Sum(l => l.Minutos),
                    TotalMinutosCubiertosBancoHoras = dias.Sum(d => d.MinutosCubiertosBancoHoras),
                    TotalMinutosPagoFactorado = todasLineasPago.Sum(l => l.MinutosFactorados),
                    TotalMinutosBancoFactorado = todasLineasBanco.Sum(l => l.MinutosFactorados),
                    TotalHorasExtraDobles = Math.Round(totalHorasDobles, 2),
                    TotalHorasExtraTriples = Math.Round(totalHorasTriples, 2),
                    TotalMontoHorasExtraEstimado = periodos.Sum(p => p.MontoEstimado),
                    TotalMinutosRetardo = dias.Sum(d => d.MinutosRetardo),
                    TotalMinutosSalidaAnticipada = dias.Sum(d => d.MinutosSalidaAnticipada),
                    TotalMinutosTrabajadosNetos = dias.Sum(d => d.MinutosTrabajadosNetos)
                };

                // Fase 8 — sin autorizar: hay detección diaria de extra pero ninguna resolución Autorizada.
                var tieneDeteccionExtra = dias.Any(d => d.MinutosExtra > 0);
                var sinAutorizar = periodos.Count == 0 && tieneDeteccionExtra;

                // Si el cliente pidió agrupadoPor="empleado", colapsamos los días en un resumen
                // y exponemos sólo los totales + periodos (consistente con la UI semanal).
                if (agrupadoPor == "empleado")
                {
                    dias = [];
                }

                return new RrhhTiempoExtraReporteEmpleadoDto
                {
                    EmpleadoId = primero.EmpleadoId,
                    NumeroEmpleado = primero.EmpleadoNumero ?? string.Empty,
                    CodigoChecador = primero.EmpleadoCodigoChecador,
                    NombreCompleto = primero.EmpleadoNombreCompleto ?? string.Empty,
                    Departamento = primero.EmpleadoDepartamento,
                    Puesto = primero.EmpleadoPuesto,
                    SueldoSemanal = primero.EmpleadoSueldoSemanal,
                    PeriodicidadPago = primero.EmpleadoPeriodicidadPago.ToString(),
                    Dias = dias,
                    Periodos = periodos,
                    SinAutorizar = sinAutorizar,
                    Totales = totales
                };
            })
            .ToList();

        var totalesGenerales = new RrhhTiempoExtraReporteTotalesDto
        {
            TotalDias = empleados.Sum(e => e.Totales.TotalDias),
            DiasConExtra = empleados.Sum(e => e.Totales.DiasConExtra),
            TotalMinutosExtra = empleados.Sum(e => e.Totales.TotalMinutosExtra),
            TotalMinutosExtraAutorizadosPago = empleados.Sum(e => e.Totales.TotalMinutosExtraAutorizadosPago),
            TotalMinutosExtraAutorizadosBanco = empleados.Sum(e => e.Totales.TotalMinutosExtraAutorizadosBanco),
            TotalMinutosCubiertosBancoHoras = empleados.Sum(e => e.Totales.TotalMinutosCubiertosBancoHoras),
            TotalMinutosPagoFactorado = empleados.Sum(e => e.Totales.TotalMinutosPagoFactorado),
            TotalMinutosBancoFactorado = empleados.Sum(e => e.Totales.TotalMinutosBancoFactorado),
            TotalHorasExtraDobles = empleados.Sum(e => e.Totales.TotalHorasExtraDobles),
            TotalHorasExtraTriples = empleados.Sum(e => e.Totales.TotalHorasExtraTriples),
            TotalMontoHorasExtraEstimado = empleados.Sum(e => e.Totales.TotalMontoHorasExtraEstimado),
            TotalMinutosRetardo = empleados.Sum(e => e.Totales.TotalMinutosRetardo),
            TotalMinutosSalidaAnticipada = empleados.Sum(e => e.Totales.TotalMinutosSalidaAnticipada),
            TotalMinutosTrabajadosNetos = empleados.Sum(e => e.Totales.TotalMinutosTrabajadosNetos)
        };

        return new RrhhTiempoExtraReporteResponse
        {
            EmpresaId = request.EmpresaId,
            FechaDesde = request.FechaDesde,
            FechaHasta = request.FechaHasta,
            AgrupadoPor = agrupadoPor,
            TotalEmpleados = empleados.Count,
            TotalDias = totalesGenerales.TotalDias,
            Totales = totalesGenerales,
            Empleados = empleados
        };
    }
}
