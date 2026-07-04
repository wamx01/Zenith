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

                // Calcular horas dobles/triples por semana (igual que NominaCalculator.ObtenerHorasExtraLegales).
                // Agrupar por semana ISO para aplicar el tope de 9h dobles por semana.
                var semanas = g.GroupBy(f => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                    f.Fecha.ToDateTime(TimeOnly.MinValue),
                    CalendarWeekRule.FirstDay,
                    DayOfWeek.Sunday));
                decimal totalHorasDobles = 0m;
                decimal totalHorasTriples = 0m;

                foreach (var semana in semanas)
                {
                    var minutosBase = semana.Sum(f => f.MinutosExtraAutorizadosPago);
                    if (minutosBase <= 0) continue;
                    var horasBase = minutosBase / 60m;
                    var horasDobles = Math.Min(9m, horasBase);
                    var horasTriples = Math.Max(0m, horasBase - horasDobles);
                    totalHorasDobles += horasDobles;
                    totalHorasTriples += horasTriples;
                }

                var dias = g
                    .OrderBy(f => f.Fecha)
                    .Select(f =>
                    {
                        var minutosBaseDia = f.MinutosExtraAutorizadosPago;
                        var horasBaseDia = minutosBaseDia / 60m;
                        var horasDoblesDia = Math.Min(9m, horasBaseDia);
                        var horasTriplesDia = Math.Max(0m, horasBaseDia - horasDoblesDia);
                        // Usar el override persistido si existe; si no, el factor de configuración
                        var factorDia = f.FactorTiempoExtraAplicado.HasValue && f.FactorTiempoExtraAplicado.Value > 0m
                            ? f.FactorTiempoExtraAplicado.Value
                            : factorTiempoExtra;
                        var factorTripleDia = factorDia; // el override aplica a ambos: dobles y triples
                        var montoDoblesDia = horasDoblesDia * sueldoHora * factorDia;
                        var montoTriplesDia = horasTriplesDia * sueldoHora * factorTripleDia;
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
                            HorasExtraDobles = Math.Round(horasDoblesDia, 2),
                            HorasExtraTriples = Math.Round(horasTriplesDia, 2),
                            SueldoHora = Math.Round(sueldoHora, 4),
                            MontoHorasExtraEstimado = Math.Round(montoDoblesDia + montoTriplesDia, 2),
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

                // El monto total se calcula sumando el monto de cada día (que ya respeta el override)
                var montoTotalEstimado = dias.Sum(d => d.MontoHorasExtraEstimado);

                var totales = new RrhhTiempoExtraReporteTotalesDto
                {
                    TotalDias = dias.Count,
                    DiasConExtra = dias.Count(d => d.MinutosExtra > 0
                        || d.MinutosExtraAutorizadosPago > 0
                        || d.MinutosExtraAutorizadosBanco > 0),
                    TotalMinutosExtra = dias.Sum(d => d.MinutosExtra),
                    TotalMinutosExtraAutorizadosPago = dias.Sum(d => d.MinutosExtraAutorizadosPago),
                    TotalMinutosExtraAutorizadosBanco = dias.Sum(d => d.MinutosExtraAutorizadosBanco),
                    TotalMinutosCubiertosBancoHoras = dias.Sum(d => d.MinutosCubiertosBancoHoras),
                    TotalMinutosPagoFactorado = dias.Sum(d => d.MinutosPagoFactorado),
                    TotalMinutosBancoFactorado = dias.Sum(d => d.MinutosBancoFactorado),
                    TotalHorasExtraDobles = Math.Round(totalHorasDobles, 2),
                    TotalHorasExtraTriples = Math.Round(totalHorasTriples, 2),
                    TotalMontoHorasExtraEstimado = montoTotalEstimado,
                    TotalMinutosRetardo = dias.Sum(d => d.MinutosRetardo),
                    TotalMinutosSalidaAnticipada = dias.Sum(d => d.MinutosSalidaAnticipada),
                    TotalMinutosTrabajadosNetos = dias.Sum(d => d.MinutosTrabajadosNetos)
                };

                // Si el cliente pidió agrupadoPor="empleado", colapsamos los días en un resumen
                // y exponemos sólo los totales (consistente con la UI semanal).
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
