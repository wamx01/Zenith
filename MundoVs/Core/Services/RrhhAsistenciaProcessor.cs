using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Core.Models;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Services;

public sealed class RrhhAsistenciaProcessor : IRrhhAsistenciaProcessor
{
    private const string DefaultMexicoTimeZone = "Central Standard Time (Mexico)";
    private const int MaxLongitudResultadoMarcacion = 250;
    private static readonly Dictionary<string, string> TimeZoneAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["America/Mexico_City"] = "Central Standard Time (Mexico)",
        ["Etc/UTC"] = "UTC"
    };

    public async Task ProcesarMarcacionesPendientesAsync(CrmDbContext db, Guid empresaId, Guid checadorId, CancellationToken cancellationToken = default)
    {
        var pendientes = await db.RrhhMarcaciones
            .Include(m => m.Checador)
            .Where(m => m.EmpresaId == empresaId && m.ChecadorId == checadorId && !m.Procesada)
            .OrderBy(m => m.FechaHoraMarcacionUtc)
            .ToListAsync(cancellationToken);

        if (pendientes.Count == 0)
        {
            return;
        }

        await ReintentarLigadoMarcacionesAsync(db, empresaId, pendientes, null, cancellationToken);
        var configuracionNomina = await NominaConfiguracionLoader.LoadAsync(db, empresaId);

        foreach (var sinEmpleado in pendientes.Where(m => m.EmpleadoId == null))
        {
            sinEmpleado.Procesada = true;
            sinEmpleado.ResultadoProcesamiento = "Empleado no identificado";
        }

        var grupos = pendientes
            .Where(m => m.EmpleadoId != null)
            .Select(m => new GrupoProceso(
                m.EmpleadoId!.Value,
                DateOnly.FromDateTime(ObtenerFechaHoraLocalMarcacion(m))))
            .Distinct()
            .ToList();

        foreach (var grupo in grupos)
        {
            await ReprocesarGrupoAsync(db, empresaId, grupo, configuracionNomina, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> ReprocesarRangoAsync(CrmDbContext db, Guid empresaId, DateOnly fechaDesde, DateOnly fechaHasta, Guid? empleadoId = null, CancellationToken cancellationToken = default)
        => await ReprocesarRangoAsync(db, empresaId, fechaDesde, fechaHasta, empleadoId, null, cancellationToken);

    public async Task<int> ReprocesarRangoAsync(CrmDbContext db, Guid empresaId, DateOnly fechaDesde, DateOnly fechaHasta, Guid? empleadoId, IProgress<RrhhAsistenciaReprocesoProgreso>? progress, CancellationToken cancellationToken = default)
    {
        if (fechaHasta < fechaDesde)
        {
            throw new ArgumentException("La fecha final no puede ser menor a la fecha inicial.", nameof(fechaHasta));
        }

        // Ventana UTC amplia para cubrir cualquier zona horaria de checador (-12..+14). Luego se filtra por fecha local real de cada marcación.
        var desdeUtc = DateTime.SpecifyKind(fechaDesde.ToDateTime(TimeOnly.MinValue).AddHours(-14), DateTimeKind.Utc);
        var hastaUtcExclusiva = DateTime.SpecifyKind(fechaHasta.AddDays(1).ToDateTime(TimeOnly.MinValue).AddHours(14), DateTimeKind.Utc);

        var marcacionesSinEmpleado = await db.RrhhMarcaciones
            .Where(m => m.EmpresaId == empresaId
                && m.EmpleadoId == null
                && m.FechaHoraMarcacionUtc >= desdeUtc
                && m.FechaHoraMarcacionUtc < hastaUtcExclusiva)
            .ToListAsync(cancellationToken);

        await ReintentarLigadoMarcacionesAsync(db, empresaId, marcacionesSinEmpleado, empleadoId, cancellationToken);
        var configuracionNomina = await NominaConfiguracionLoader.LoadAsync(db, empresaId);

        foreach (var marcacion in marcacionesSinEmpleado)
        {
            if (marcacion.EmpleadoId != null)
            {
                continue;
            }

            marcacion.Procesada = true;
            marcacion.ResultadoProcesamiento = "Empleado no identificado";
        }

        var marcacionesRango = await db.RrhhMarcaciones
            .Include(m => m.Checador)
            .Where(m => m.EmpresaId == empresaId
                && m.EmpleadoId != null
                && m.FechaHoraMarcacionUtc >= desdeUtc
                && m.FechaHoraMarcacionUtc < hastaUtcExclusiva
                && (!empleadoId.HasValue || m.EmpleadoId == empleadoId.Value))
            .ToListAsync(cancellationToken);

        var gruposMarcaciones = marcacionesRango
            .Select(m => new GrupoProceso(
                m.EmpleadoId!.Value,
                DateOnly.FromDateTime(ObtenerFechaHoraLocalMarcacion(m))))
            .Where(g => g.Fecha >= fechaDesde && g.Fecha <= fechaHasta)
            .Distinct()
            .ToList();

        var gruposAsistencias = await db.RrhhAsistencias
            .Where(a => a.EmpresaId == empresaId
                && a.Fecha >= fechaDesde
                && a.Fecha <= fechaHasta
                && (!empleadoId.HasValue || a.EmpleadoId == empleadoId.Value))
            .Select(a => new GrupoProceso(a.EmpleadoId, a.Fecha))
            .Distinct()
            .ToListAsync(cancellationToken);

        var grupos = gruposMarcaciones
            .Concat(gruposAsistencias)
            .Distinct()
            .OrderBy(g => g.EmpleadoId)
            .ThenBy(g => g.Fecha)
            .ToList();

        for (var indice = 0; indice < grupos.Count; indice++)
        {
            var grupo = grupos[indice];
            await ReprocesarGrupoAsync(db, empresaId, grupo, configuracionNomina, cancellationToken);
            progress?.Report(new RrhhAsistenciaReprocesoProgreso(indice + 1, grupos.Count, grupo.EmpleadoId, grupo.Fecha));
        }

        await db.SaveChangesAsync(cancellationToken);
        return grupos.Count;
    }

    private static async Task ReintentarLigadoMarcacionesAsync(CrmDbContext db, Guid empresaId, IEnumerable<RrhhMarcacion> marcaciones, Guid? empleadoId, CancellationToken cancellationToken)
    {
        var marcacionesSinEmpleado = marcaciones
            .Where(m => m.EmpleadoId == null && !string.IsNullOrWhiteSpace(m.CodigoChecador))
            .ToList();

        if (marcacionesSinEmpleado.Count == 0)
        {
            return;
        }

        var codigos = marcacionesSinEmpleado
            .Select(m => m.CodigoChecador.Trim())
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (codigos.Count == 0)
        {
            return;
        }

        var empleados = await db.Empleados
            .Where(e => e.EmpresaId == empresaId
                && e.IsActive
                && e.CodigoChecador != null
                && codigos.Contains(e.CodigoChecador)
                && (!empleadoId.HasValue || e.Id == empleadoId.Value))
            .Select(e => new { e.Id, e.CodigoChecador })
            .ToListAsync(cancellationToken);

        if (empleados.Count == 0)
        {
            return;
        }

        var empleadosPorCodigo = empleados
            .Where(e => !string.IsNullOrWhiteSpace(e.CodigoChecador))
            .GroupBy(e => e.CodigoChecador!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);

        foreach (var marcacion in marcacionesSinEmpleado)
        {
            var codigo = marcacion.CodigoChecador.Trim();
            if (!empleadosPorCodigo.TryGetValue(codigo, out var empleadoEncontradoId))
            {
                continue;
            }

            marcacion.EmpleadoId = empleadoEncontradoId;
            marcacion.Procesada = false;
            marcacion.ResultadoProcesamiento = null;
            marcacion.UpdatedAt = DateTime.UtcNow;
        }
    }

    private static async Task ReprocesarGrupoAsync(CrmDbContext db, Guid empresaId, GrupoProceso grupo, NominaConfiguracion configuracionNomina, CancellationToken cancellationToken)
    {
        var fecha = grupo.Fecha;
        // Ventana UTC amplia (±14h) para cubrir cualquier zona de checador; cada marcación se filtra después por su fecha local real.
        var inicioUtcCoarse = DateTime.SpecifyKind(fecha.ToDateTime(TimeOnly.MinValue).AddHours(-14), DateTimeKind.Utc);
        var finUtcCoarse = DateTime.SpecifyKind(fecha.AddDays(1).ToDateTime(TimeOnly.MinValue).AddHours(14), DateTimeKind.Utc);

        var empleado = await db.Empleados
            .Include(e => e.TurnoBase)!
                .ThenInclude(t => t!.Detalles)
                    .ThenInclude(d => d.Descansos)
            .FirstOrDefaultAsync(e => e.Id == grupo.EmpleadoId && e.EmpresaId == empresaId, cancellationToken);

        if (empleado == null)
        {
            return;
        }

        var candidatas = await db.RrhhMarcaciones
            .Include(m => m.Checador)
            .Where(m => m.EmpresaId == empresaId
                && m.EmpleadoId == grupo.EmpleadoId
                && !m.EsAnulada
                && m.FechaHoraMarcacionUtc >= inicioUtcCoarse
                && m.FechaHoraMarcacionUtc < finUtcCoarse)
            .OrderBy(m => m.FechaHoraMarcacionUtc)
            .ToListAsync(cancellationToken);

        // Cada marcación se convierte a local con SU propia zona de checador, y sólo se conservan las que caen en la fecha del grupo.
        var marcacionesClasificadas = candidatas
            .Select(m => new MarcacionProcesada(m, ObtenerFechaHoraLocalMarcacion(m)))
            .Where(mp => DateOnly.FromDateTime(mp.FechaLocal) == fecha)
            .OrderBy(mp => mp.FechaLocal)
            .ToList();

        var marcacionesDia = marcacionesClasificadas.Select(mp => mp.Marcacion).ToList();

        var turno = await ResolverTurnoVigenteAsync(db, empresaId, empleado, fecha, cancellationToken);
        var detalleTurno = turno?.Detalles.FirstOrDefault(d => d.DiaSemana == MapDiaSemana(fecha.DayOfWeek));
        var configuracionDescansos = await RrhhAsistenciaDescansoSettings.LoadAsync(db, empresaId, cancellationToken);
        var permisoParcial = await ObtenerPermisoParcialDiaAsync(db, empresaId, grupo.EmpleadoId, fecha, cancellationToken);
        var resolucionesSegmento = await ObtenerResolucionesSegmentoAsync(db, empresaId, grupo.EmpleadoId, fecha, cancellationToken);
        var analisisJornada = AnalizarJornada(detalleTurno, marcacionesClasificadas, resolucionesSegmento, configuracionDescansos, permisoParcial);
        await ConciliarResolucionesSegmentoAsync(db, empresaId, grupo.EmpleadoId, fecha, marcacionesClasificadas, resolucionesSegmento, cancellationToken);

        var entradaReal = analisisJornada.EntradaReal;
        var salidaReal = analisisJornada.SalidaReal;
        var minutosEntradaAnticipada = detalleTurno?.HoraEntrada is TimeSpan entradaProgramadaAnticipada && entradaReal.HasValue
            ? Math.Max(0, (int)Math.Round((entradaProgramadaAnticipada - entradaReal.Value).TotalMinutes))
            : 0;
        var minutosRetardo = detalleTurno?.HoraEntrada is TimeSpan entradaProgramada && entradaReal.HasValue
            ? ObtenerMinutosRetardoAplicables(Math.Max(0, (int)Math.Round((entradaReal.Value - entradaProgramada).TotalMinutes)), configuracionDescansos)
            : 0;
        var minutosSalidaAnticipada = detalleTurno?.HoraSalida is TimeSpan salidaProgramada && salidaReal.HasValue
            ? Math.Max(0, (int)Math.Round((salidaProgramada - salidaReal.Value).TotalMinutes))
            : 0;
        var minutosMargenNoComputables = CalcularMinutosMargenNoComputables(detalleTurno, analisisJornada, configuracionNomina.MinutosMinimosTiempoExtra);
        var minutosTrabajadosBrutos = Math.Max(0, analisisJornada.MinutosTrabajadosBrutos - minutosMargenNoComputables);
        var minutosTrabajadosNetos = Math.Max(0, analisisJornada.MinutosTrabajadosNetos - minutosMargenNoComputables);
        var minutosExtra = CalcularMinutosExtra(detalleTurno, analisisJornada, configuracionDescansos, configuracionNomina.MinutosMinimosTiempoExtra);

        var (estatus, requiereRevision, observaciones) = ClasificarAsistencia(
            detalleTurno,
            analisisJornada,
            minutosEntradaAnticipada,
            minutosRetardo,
            minutosSalidaAnticipada,
            minutosExtra,
            configuracionNomina.MinutosMinimosTiempoExtra);
        observaciones = AgregarObservacionMargenNoComputable(observaciones, detalleTurno, analisisJornada, configuracionNomina.MinutosMinimosTiempoExtra);

        var asistencia = await db.RrhhAsistencias
            .FirstOrDefaultAsync(a => a.EmpresaId == empresaId && a.EmpleadoId == grupo.EmpleadoId && a.Fecha == fecha, cancellationToken);

        if (asistencia == null)
        {
            asistencia = new RrhhAsistencia
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresaId,
                EmpleadoId = grupo.EmpleadoId,
                Fecha = fecha,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            db.RrhhAsistencias.Add(asistencia);
        }

        asistencia.TurnoBaseId = turno?.Id;
        asistencia.HoraEntradaProgramada = detalleTurno?.HoraEntrada;
        asistencia.HoraSalidaProgramada = detalleTurno?.HoraSalida;
        asistencia.HoraEntradaReal = entradaReal;
        asistencia.HoraSalidaReal = salidaReal;
        asistencia.TotalMarcaciones = marcacionesClasificadas.Count;
        asistencia.MinutosJornadaProgramada = analisisJornada.MinutosJornadaProgramada;
        asistencia.MinutosJornadaNetaProgramada = analisisJornada.MinutosJornadaNetaProgramada;
        asistencia.MinutosTrabajadosBrutos = minutosTrabajadosBrutos;
        asistencia.MinutosTrabajadosNetos = minutosTrabajadosNetos;
        asistencia.MinutosDescansoProgramado = analisisJornada.MinutosDescansoProgramado;
        asistencia.MinutosDescansoTomado = analisisJornada.MinutosDescansoTomado;
        asistencia.MinutosDescansoPagado = analisisJornada.MinutosDescansoPagado;
        asistencia.MinutosDescansoNoPagado = analisisJornada.MinutosDescansoNoPagado;
        asistencia.MinutosRetardo = minutosRetardo;
        asistencia.MinutosSalidaAnticipada = minutosSalidaAnticipada;
        asistencia.MinutosExtra = minutosExtra;
        asistencia.Estatus = estatus;
        asistencia.RequiereRevision = requiereRevision;
        asistencia.Observaciones = observaciones;
        asistencia.ResumenDescansos = analisisJornada.ResumenDescansos;
        asistencia.UpdatedAt = DateTime.UtcNow;

        var resultadoMarcacion = ConstruirResultadoMarcacion(estatus, observaciones);
        foreach (var marcacion in marcacionesDia)
        {
            marcacion.Procesada = true;
            marcacion.ResultadoProcesamiento = resultadoMarcacion;
            marcacion.UpdatedAt = DateTime.UtcNow;
        }
    }

    private static (RrhhAsistenciaEstatus Estatus, bool RequiereRevision, string? Observaciones) ClasificarAsistencia(
        TurnoBaseDetalle? detalleTurno,
        AnalisisJornada analisisJornada,
        int minutosEntradaAnticipada,
        int minutosRetardo,
        int minutosSalidaAnticipada,
        int minutosExtra,
        int minutosMinimosTiempoExtra)
    {
        minutosMinimosTiempoExtra = ObtenerMinutosMinimosTiempoExtra(minutosMinimosTiempoExtra);

        if (detalleTurno == null)
        {
            return (RrhhAsistenciaEstatus.TurnoNoAsignado, true, "El empleado no tiene turno configurado para este día.");
        }

        if (analisisJornada.TotalMarcaciones == 0)
        {
            return detalleTurno.Labora
                ? (RrhhAsistenciaEstatus.Falta, true, "No se detectaron marcaciones para un día laborable.")
                : (RrhhAsistenciaEstatus.Descanso, false, "No se detectaron marcaciones en un día no laborable.");
        }

        if (!detalleTurno.Labora)
        {
            return (RrhhAsistenciaEstatus.DescansoTrabajado, false, "Se registraron marcas en un día no laborable.");
        }

        if (analisisJornada.TotalMarcaciones <= 1)
        {
            return (RrhhAsistenciaEstatus.Incompleta, true, "Solo se detectó una marcación para el día.");
        }

        var requiereRevisionDescansos = analisisJornada.RequiereRevision;
        var observacionesDescansos = analisisJornada.ObservacionesRevision;
        var requiereRevisionEntradaAnticipada = minutosEntradaAnticipada >= minutosMinimosTiempoExtra;
        var requiereRevisionJornadaIrregular = requiereRevisionEntradaAnticipada && minutosExtra > 0;

        if (minutosRetardo > 0)
        {
            return (RrhhAsistenciaEstatus.Retardo, minutosSalidaAnticipada > 0 || requiereRevisionDescansos || requiereRevisionEntradaAnticipada || requiereRevisionJornadaIrregular, ConstruirObservaciones(minutosEntradaAnticipada, minutosRetardo, minutosSalidaAnticipada, minutosExtra, minutosMinimosTiempoExtra, "La asistencia presenta retardo.", observacionesDescansos));
        }

        if (minutosSalidaAnticipada > 0)
        {
            return (RrhhAsistenciaEstatus.AsistenciaNormal, true, ConstruirObservaciones(minutosEntradaAnticipada, minutosRetardo, minutosSalidaAnticipada, minutosExtra, minutosMinimosTiempoExtra, "La salida real fue antes de la programada.", observacionesDescansos));
        }

        return (RrhhAsistenciaEstatus.AsistenciaNormal, requiereRevisionDescansos || requiereRevisionEntradaAnticipada || requiereRevisionJornadaIrregular, ConstruirObservaciones(minutosEntradaAnticipada, minutosRetardo, minutosSalidaAnticipada, minutosExtra, minutosMinimosTiempoExtra, null, observacionesDescansos));
    }

    private static int CalcularMinutosExtra(TurnoBaseDetalle? detalleTurno, AnalisisJornada analisisJornada, RrhhAsistenciaDescansoSettings configuracionDescansos, int minutosMinimosTiempoExtra)
    {
        minutosMinimosTiempoExtra = ObtenerMinutosMinimosTiempoExtra(minutosMinimosTiempoExtra);

        if (analisisJornada.BloquearTiempoExtraAutomatico)
        {
            return 0;
        }

        if (analisisJornada.AutoDescuentoDescansoNoMarcado)
        {
            var margenNoComputable = CalcularMinutosMargenNoComputables(detalleTurno, analisisJornada, minutosMinimosTiempoExtra);
            var excedenteNeto = Math.Max(0, analisisJornada.MinutosTrabajadosNetos - analisisJornada.MinutosJornadaNetaProgramada - margenNoComputable);
            return excedenteNeto <= configuracionDescansos.ToleranciaCoincidenciaBrutaMinutos
                ? 0
                : excedenteNeto;
        }

        if (detalleTurno?.HoraEntrada is not TimeSpan entradaProgramada || detalleTurno.HoraSalida is not TimeSpan salidaProgramada)
        {
            return Math.Max(0, analisisJornada.MinutosTrabajadosNetos - analisisJornada.MinutosJornadaNetaProgramada);
        }

        var minutosEntradaAnticipada = analisisJornada.EntradaReal.HasValue
            ? Math.Max(0, (int)Math.Round((entradaProgramada - analisisJornada.EntradaReal.Value).TotalMinutes))
            : 0;
        var minutosRetardo = analisisJornada.EntradaReal.HasValue
            ? ObtenerMinutosRetardoAplicables(Math.Max(0, (int)Math.Round((analisisJornada.EntradaReal.Value - entradaProgramada).TotalMinutes)), configuracionDescansos)
            : 0;
        var minutosSalidaPosterior = analisisJornada.SalidaReal.HasValue
            ? Math.Max(0, (int)Math.Round((analisisJornada.SalidaReal.Value - salidaProgramada).TotalMinutes))
            : 0;

        if (minutosEntradaAnticipada < minutosMinimosTiempoExtra)
        {
            minutosEntradaAnticipada = 0;
        }

        if (minutosSalidaPosterior < minutosMinimosTiempoExtra)
        {
            minutosSalidaPosterior = 0;
        }

        var minutosTrabajoAdicionalAntes = analisisJornada.EsTrabajoAdicionalAntesValido
            ? analisisJornada.MinutosTrabajoAdicionalAntes
            : 0;
        if (minutosTrabajoAdicionalAntes > 0 && minutosTrabajoAdicionalAntes < minutosMinimosTiempoExtra)
        {
            minutosTrabajoAdicionalAntes = 0;
        }

        var minutosTrabajoAdicionalDespues = analisisJornada.EsTrabajoAdicionalDespuesValido
            ? analisisJornada.MinutosTrabajoAdicionalDespues
            : 0;
        if (minutosTrabajoAdicionalDespues > 0 && minutosTrabajoAdicionalDespues < minutosMinimosTiempoExtra)
        {
            minutosTrabajoAdicionalDespues = 0;
        }

        if (minutosRetardo > 0 && minutosSalidaPosterior > 0)
        {
            minutosSalidaPosterior = Math.Max(0, minutosSalidaPosterior - minutosRetardo);
            if (minutosSalidaPosterior < minutosMinimosTiempoExtra)
            {
                minutosSalidaPosterior = 0;
            }
        }

        return minutosEntradaAnticipada + minutosSalidaPosterior + minutosTrabajoAdicionalAntes + minutosTrabajoAdicionalDespues;
    }

    private static int CalcularMinutosMargenNoComputables(TurnoBaseDetalle? detalleTurno, AnalisisJornada analisisJornada, int minutosMinimosTiempoExtra)
    {
        minutosMinimosTiempoExtra = ObtenerMinutosMinimosTiempoExtra(minutosMinimosTiempoExtra);

        if (detalleTurno?.HoraEntrada is not TimeSpan entradaProgramada || detalleTurno.HoraSalida is not TimeSpan salidaProgramada)
        {
            return 0;
        }

        var minutosEntradaAnticipada = analisisJornada.EntradaReal.HasValue
            ? Math.Max(0, (int)Math.Round((entradaProgramada - analisisJornada.EntradaReal.Value).TotalMinutes))
            : 0;
        var minutosSalidaPosterior = analisisJornada.SalidaReal.HasValue
            ? Math.Max(0, (int)Math.Round((analisisJornada.SalidaReal.Value - salidaProgramada).TotalMinutes))
            : 0;

        var minutosNoComputables = 0;
        if (minutosEntradaAnticipada > 0 && minutosEntradaAnticipada < minutosMinimosTiempoExtra)
        {
            minutosNoComputables += minutosEntradaAnticipada;
        }

        if (minutosSalidaPosterior > 0 && minutosSalidaPosterior < minutosMinimosTiempoExtra)
        {
            minutosNoComputables += minutosSalidaPosterior;
        }

        if (analisisJornada.MinutosTrabajoAdicionalAntes > 0 && analisisJornada.MinutosTrabajoAdicionalAntes < minutosMinimosTiempoExtra)
        {
            minutosNoComputables += analisisJornada.MinutosTrabajoAdicionalAntes;
        }

        if (analisisJornada.MinutosTrabajoAdicionalDespues > 0 && analisisJornada.MinutosTrabajoAdicionalDespues < minutosMinimosTiempoExtra)
        {
            minutosNoComputables += analisisJornada.MinutosTrabajoAdicionalDespues;
        }

        return minutosNoComputables;
    }

    private static string? AgregarObservacionMargenNoComputable(string? observaciones, TurnoBaseDetalle? detalleTurno, AnalisisJornada analisisJornada, int minutosMinimosTiempoExtra)
    {
        minutosMinimosTiempoExtra = ObtenerMinutosMinimosTiempoExtra(minutosMinimosTiempoExtra);

        if (detalleTurno?.HoraEntrada is not TimeSpan entradaProgramada || detalleTurno.HoraSalida is not TimeSpan salidaProgramada)
        {
            return observaciones;
        }

        var partes = new List<string>();
        if (!string.IsNullOrWhiteSpace(observaciones))
        {
            partes.Add(observaciones);
        }

        var minutosEntradaAnticipada = analisisJornada.EntradaReal.HasValue
            ? Math.Max(0, (int)Math.Round((entradaProgramada - analisisJornada.EntradaReal.Value).TotalMinutes))
            : 0;
        var minutosSalidaPosterior = analisisJornada.SalidaReal.HasValue
            ? Math.Max(0, (int)Math.Round((analisisJornada.SalidaReal.Value - salidaProgramada).TotalMinutes))
            : 0;

        if (minutosEntradaAnticipada > 0 && minutosEntradaAnticipada < minutosMinimosTiempoExtra)
        {
            partes.Add($"Los {minutosEntradaAnticipada} min previos a la entrada programada no se contaron como tiempo laboral por no alcanzar {minutosMinimosTiempoExtra} min.");
        }

        if (minutosSalidaPosterior > 0 && minutosSalidaPosterior < minutosMinimosTiempoExtra)
        {
            partes.Add($"Los {minutosSalidaPosterior} min posteriores a la salida programada no se contaron como tiempo laboral por no alcanzar {minutosMinimosTiempoExtra} min.");
        }

        if (analisisJornada.MinutosTrabajoAdicionalAntes > 0 && analisisJornada.MinutosTrabajoAdicionalAntes < minutosMinimosTiempoExtra)
        {
            partes.Add($"El bloque previo al turno de {analisisJornada.MinutosTrabajoAdicionalAntes} min no se contó como tiempo laboral por no alcanzar {minutosMinimosTiempoExtra} min.");
        }

        if (analisisJornada.MinutosTrabajoAdicionalDespues > 0 && analisisJornada.MinutosTrabajoAdicionalDespues < minutosMinimosTiempoExtra)
        {
            partes.Add($"El bloque posterior al turno de {analisisJornada.MinutosTrabajoAdicionalDespues} min no se contó como tiempo laboral por no alcanzar {minutosMinimosTiempoExtra} min.");
        }

        return partes.Count == 0 ? null : string.Join(" ", partes);
    }

    private static int ObtenerMinutosMinimosTiempoExtra(int minutosMinimosTiempoExtra)
        => minutosMinimosTiempoExtra > 0 ? minutosMinimosTiempoExtra : 30;

    private static int ObtenerMinutosRetardoAplicables(int minutosRetardo, RrhhAsistenciaDescansoSettings configuracionDescansos)
        => minutosRetardo <= Math.Max(0, configuracionDescansos.ToleranciaRetardoMinutos)
            ? 0
            : minutosRetardo;

    private static string? ConstruirObservaciones(int minutosEntradaAnticipada, int minutosRetardo, int minutosSalidaAnticipada, int minutosExtra, int minutosMinimosTiempoExtra, string? baseObservacion, IEnumerable<string>? observacionesAdicionales = null)
    {
        minutosMinimosTiempoExtra = ObtenerMinutosMinimosTiempoExtra(minutosMinimosTiempoExtra);

        var partes = new List<string>();
        if (!string.IsNullOrWhiteSpace(baseObservacion))
        {
            partes.Add(baseObservacion);
        }

        if (minutosEntradaAnticipada > 0)
        {
            partes.Add($"Entrada anticipada de {minutosEntradaAnticipada} min.");
            if (minutosEntradaAnticipada >= minutosMinimosTiempoExtra)
            {
                partes.Add("Validar si corresponde cambio de turno o autorización de jornada extraordinaria.");
            }
        }

        if (minutosRetardo > 0)
        {
            partes.Add($"Retardo de {minutosRetardo} min.");
        }

        if (minutosSalidaAnticipada > 0)
        {
            partes.Add($"Salida anticipada de {minutosSalidaAnticipada} min.");
        }

        if (minutosExtra > 0)
        {
            partes.Add($"Tiempo extra de {minutosExtra} min.");
        }

        if (observacionesAdicionales != null)
        {
            partes.AddRange(observacionesAdicionales.Where(o => !string.IsNullOrWhiteSpace(o)));
        }

        return partes.Count == 0 ? null : string.Join(" ", partes);
    }

    private static AnalisisJornada AnalizarJornada(TurnoBaseDetalle? detalleTurno, IReadOnlyList<MarcacionProcesada> marcaciones, IReadOnlyList<RrhhSegmentoResolucion> resolucionesSegmento, RrhhAsistenciaDescansoSettings configuracionDescansos, PermisoParcialDia? permisoParcial)
    {
        var descansosConfigurados = ObtenerDescansosConfigurados(detalleTurno);
        var minutosJornadaProgramada = detalleTurno?.HoraEntrada is TimeSpan entradaProgramada && detalleTurno.HoraSalida is TimeSpan salidaProgramada
            ? Math.Max(0, (int)Math.Round((salidaProgramada - entradaProgramada).TotalMinutes))
            : 0;
        var minutosDescansoProgramado = descansosConfigurados.Sum(d => d.Minutos);
        var minutosJornadaNetaProgramada = Math.Max(0, minutosJornadaProgramada - descansosConfigurados.Where(d => !d.EsPagado).Sum(d => d.Minutos));

        var jornadaSeleccionada = SeleccionarJornadaPrincipal(detalleTurno, marcaciones);
        var entradaMarcacion = jornadaSeleccionada.Entrada;
        var salidaMarcacion = jornadaSeleccionada.Salida;
        var entradaReal = entradaMarcacion?.FechaLocal.TimeOfDay;
        var salidaReal = salidaMarcacion != null && salidaMarcacion != entradaMarcacion
            ? salidaMarcacion.FechaLocal.TimeOfDay
            : (TimeSpan?)null;
        var cantidadMarcasAntesEntradaProgramada = detalleTurno?.HoraEntrada is TimeSpan entradaProgramadaGlobal
            ? marcaciones.Count(m => m.FechaLocal.TimeOfDay < entradaProgramadaGlobal)
            : 0;
        var cantidadMarcasDespuesSalidaProgramada = detalleTurno?.HoraSalida is TimeSpan salidaProgramadaGlobal
            ? marcaciones.Count(m => m.FechaLocal.TimeOfDay > salidaProgramadaGlobal)
            : 0;

        if (marcaciones.Count <= 1)
        {
            return new AnalisisJornada(
                marcaciones.Count,
                minutosJornadaProgramada,
                minutosJornadaNetaProgramada,
                0,
                minutosDescansoProgramado,
                0,
                0,
                0,
                0,
                0,
                entradaReal,
                salidaReal,
                descansosConfigurados.Count > 0 ? "Descansos programados sin información suficiente." : null,
                descansosConfigurados.Count > 0 ? ["No hay suficientes marcaciones para evaluar descansos configurados."] : [],
                descansosConfigurados.Count > 0,
                false,
                false,
                false,
                false);
        }

        var fechaEntrada = entradaMarcacion?.FechaLocal;
        var fechaSalida = salidaMarcacion?.FechaLocal;
        var marcasPrevias = fechaEntrada.HasValue
            ? marcaciones.Where(m => m.FechaLocal < fechaEntrada.Value).ToList()
            : [];
        var marcasPosteriores = fechaSalida.HasValue
            ? marcaciones.Where(m => m.FechaLocal > fechaSalida.Value).ToList()
            : [];
        var minutosTrabajoAdicionalAntes = CalcularMinutosTrabajoAdicional(marcasPrevias, "previo al turno", observaciones: []);
        var minutosTrabajoAdicionalDespues = CalcularMinutosTrabajoAdicional(marcasPosteriores, "posterior al turno", observaciones: []);
        var esTrabajoAdicionalAntesValido = EsTrabajoAdicionalAutomaticoValido(marcasPrevias);
        var esTrabajoAdicionalDespuesValido = EsTrabajoAdicionalAutomaticoValido(marcasPosteriores);
        var minutosRetardoPrincipal = detalleTurno?.HoraEntrada is TimeSpan entradaProgramadaAplicada && entradaReal.HasValue
            ? ObtenerMinutosRetardoAplicables(Math.Max(0, (int)Math.Round((entradaReal.Value - entradaProgramadaAplicada).TotalMinutes)), configuracionDescansos)
            : 0;
        var minutosSalidaAnticipadaPrincipal = detalleTurno?.HoraSalida is TimeSpan salidaProgramadaAplicada && salidaReal.HasValue
            ? Math.Max(0, (int)Math.Round((salidaProgramadaAplicada - salidaReal.Value).TotalMinutes))
            : 0;
        var bloquearBloquePrevioComoExtra = minutosTrabajoAdicionalAntes > 0 && esTrabajoAdicionalAntesValido && (minutosRetardoPrincipal > 0 || minutosSalidaAnticipadaPrincipal > 0);
        var bloquearBloquePosteriorComoExtra = minutosTrabajoAdicionalDespues > 0 && esTrabajoAdicionalDespuesValido && (minutosRetardoPrincipal > 0 || minutosSalidaAnticipadaPrincipal > 0);
        if (bloquearBloquePrevioComoExtra)
        {
            esTrabajoAdicionalAntesValido = false;
        }

        if (bloquearBloquePosteriorComoExtra)
        {
            esTrabajoAdicionalDespuesValido = false;
        }

        var minutosTrabajadosBrutosJornada = fechaEntrada.HasValue && fechaSalida.HasValue && fechaSalida > fechaEntrada
            ? Math.Max(0, (int)Math.Round((fechaSalida.Value - fechaEntrada.Value).TotalMinutes))
            : 0;
        var minutosTrabajadosBrutos = minutosTrabajadosBrutosJornada + minutosTrabajoAdicionalAntes + minutosTrabajoAdicionalDespues;
        var marcasIntermedias = marcaciones
            .Where(m => fechaEntrada.HasValue
                && fechaSalida.HasValue
                && m.FechaLocal > fechaEntrada.Value
                && m.FechaLocal < fechaSalida.Value)
            .ToList();
        var tieneCorteAntesDeEntradaProgramada = detalleTurno?.HoraEntrada is TimeSpan entradaProgramadaSegmentos
            && marcasIntermedias.Any(m => m.FechaLocal.TimeOfDay < entradaProgramadaSegmentos);
        var tieneCorteDespuesDeSalidaProgramada = detalleTurno?.HoraSalida is TimeSpan salidaProgramadaSegmentos
            && marcasIntermedias.Any(m => m.FechaLocal.TimeOfDay > salidaProgramadaSegmentos);
        var descansosTomados = new List<DescansoTomado>();
        var observaciones = new List<string>();
        var minutosSalidaTemporal = 0;
        var minutosPermisoSegmento = 0;
        var minutosNoConsiderados = 0;

        if (tieneCorteAntesDeEntradaProgramada)
        {
            observaciones.Add("Se detectó un corte interno antes de la entrada programada; la entrada anticipada queda como referencia operativa y no como extra automática.");
        }

        if (tieneCorteDespuesDeSalidaProgramada)
        {
            observaciones.Add("Se detectó un corte interno después de la salida programada; la permanencia posterior queda como referencia operativa y no como extra automática.");
        }

        if (cantidadMarcasAntesEntradaProgramada > 2)
        {
            observaciones.Add("Se detectaron múltiples cortes antes de la entrada programada; los tramos previos quedan en revisión y no se toman como extra automática.");
        }

        if (cantidadMarcasDespuesSalidaProgramada > 2)
        {
            observaciones.Add("Se detectaron múltiples cortes después de la salida programada; los tramos posteriores quedan en revisión y no se toman como extra automática.");
        }

        if (minutosTrabajoAdicionalAntes > 0)
        {
            observaciones.Add(bloquearBloquePrevioComoExtra
                ? $"Se detectó un bloque previo al turno de {minutosTrabajoAdicionalAntes} min; quedó como referencia operativa y no como extra automática porque el día también presenta retardo o salida anticipada."
                : esTrabajoAdicionalAntesValido
                    ? $"Se detectó un bloque previo al turno de {minutosTrabajoAdicionalAntes} min; se tomó como tiempo adicional del día."
                    : $"Se detectó un bloque previo al turno de {minutosTrabajoAdicionalAntes} min; quedó como referencia operativa y no como extra automática.");
        }

        if (minutosTrabajoAdicionalDespues > 0)
        {
            observaciones.Add(bloquearBloquePosteriorComoExtra
                ? $"Se detectó un bloque posterior al turno de {minutosTrabajoAdicionalDespues} min; quedó como referencia operativa y no como extra automática porque el día también presenta retardo o salida anticipada."
                : esTrabajoAdicionalDespuesValido
                    ? $"Se detectó un bloque posterior al turno de {minutosTrabajoAdicionalDespues} min; se tomó como tiempo adicional del día."
                    : $"Se detectó un bloque posterior al turno de {minutosTrabajoAdicionalDespues} min; quedó como referencia operativa y no como extra automática.");
        }

        var segmentosEspeciales = ExtraerSegmentosEspeciales(marcasIntermedias, resolucionesSegmento, observaciones);
        minutosSalidaTemporal = segmentosEspeciales.MinutosSalidaTemporal;
        minutosPermisoSegmento = segmentosEspeciales.MinutosPermiso;
        minutosNoConsiderados = segmentosEspeciales.MinutosNoConsiderados;

        marcasIntermedias = segmentosEspeciales.MarcasRestantes;
        marcasIntermedias = ExcluirSegmentosTrabajoManual(marcasIntermedias, resolucionesSegmento, observaciones);

        var descansosClasificados = marcasIntermedias
            .Where(m => m.Marcacion.ClasificacionOperativa is TipoClasificacionMarcacionRrhh.InicioDescanso or TipoClasificacionMarcacionRrhh.FinDescanso)
            .ToList();
        var usarClasificacionManual = descansosClasificados.Count >= 2;

        if (usarClasificacionManual)
        {
            var inicios = descansosClasificados.Where(m => m.Marcacion.ClasificacionOperativa == TipoClasificacionMarcacionRrhh.InicioDescanso).ToList();
            var fines = descansosClasificados.Where(m => m.Marcacion.ClasificacionOperativa == TipoClasificacionMarcacionRrhh.FinDescanso).ToList();
            var pares = Math.Min(inicios.Count, fines.Count);
            for (var i = 0; i < pares; i++)
            {
                var salidaDescanso = inicios[i].FechaLocal;
                var regresoDescanso = fines[i].FechaLocal;
                var numero = descansosTomados.Count + 1;
                var configurado = numero <= descansosConfigurados.Count ? descansosConfigurados[numero - 1] : null;
                var minutos = Math.Max(0, (int)Math.Round((regresoDescanso - salidaDescanso).TotalMinutes));
                descansosTomados.Add(new DescansoTomado(numero, salidaDescanso, regresoDescanso, minutos, configurado?.EsPagado ?? false));
            }

            if (inicios.Count != fines.Count)
            {
                observaciones.Add("La clasificación manual de descansos no tiene pares completos.");
            }
        }
        else
        {
            var marcasIntermediasFechas = marcasIntermedias.Select(m => m.FechaLocal).ToList();
            for (var i = 0; i + 1 < marcasIntermediasFechas.Count; i += 2)
            {
                var salidaDescanso = marcasIntermediasFechas[i];
                var regresoDescanso = marcasIntermediasFechas[i + 1];
                var numero = descansosTomados.Count + 1;
                var configurado = numero <= descansosConfigurados.Count ? descansosConfigurados[numero - 1] : null;
                var minutos = Math.Max(0, (int)Math.Round((regresoDescanso - salidaDescanso).TotalMinutes));

                descansosTomados.Add(new DescansoTomado(numero, salidaDescanso, regresoDescanso, minutos, configurado?.EsPagado ?? false));

            }

            if (marcasIntermediasFechas.Count % 2 != 0)
            {
                observaciones.Add("Hay una marcación intermedia sin par para cierre o regreso de descanso.");
            }
        }

        descansosConfigurados = ObtenerDescansosConfiguradosOInferidos(detalleTurno, descansosTomados, resolucionesSegmento);

        var conflictosAlternancia = DetectarConflictosAlternancia(marcasIntermedias, resolucionesSegmento, descansosConfigurados);
        foreach (var conflicto in conflictosAlternancia.Observaciones)
        {
            observaciones.Add(conflicto);
        }

        if (descansosTomados.Count > descansosConfigurados.Count)
        {
            observaciones.Add($"Se detectaron {descansosTomados.Count} descanso(s) y el turno solo contempla {descansosConfigurados.Count}.");
        }

        var minutosSalidaAnticipada = detalleTurno?.HoraSalida is TimeSpan salidaProgramadaDescansos && salidaReal.HasValue
            ? Math.Max(0, (int)Math.Round((salidaProgramadaDescansos - salidaReal.Value).TotalMinutes))
            : 0;
        var descansosAplicados = CalcularDescansosAplicados(descansosConfigurados, descansosTomados, resolucionesSegmento, minutosSalidaAnticipada, permisoParcial, configuracionDescansos, observaciones);
        var minutosDescansoTomado = descansosAplicados.Sum(d => d.MinutosAplicados);
        var minutosDescansoPagado = descansosAplicados.Where(d => d.EsPagado).Sum(d => d.MinutosAplicados);
        var minutosDescansoNoPagado = Math.Max(0, minutosDescansoTomado - minutosDescansoPagado);
        var minutosTrabajadosNetos = Math.Max(0, minutosTrabajadosBrutos - minutosDescansoNoPagado - minutosSalidaTemporal - minutosPermisoSegmento - minutosNoConsiderados);
        var resumenDescansos = ConstruirResumenDescansos(descansosConfigurados, descansosAplicados);

        var autoDescuentoDescansoNoMarcado = descansosAplicados.Any(d => !d.FueMarcado && d.MinutosAplicados > 0);
        var bloquearTiempoExtraAutomatico = tieneCorteAntesDeEntradaProgramada
            || tieneCorteDespuesDeSalidaProgramada
            || cantidadMarcasAntesEntradaProgramada > 2
            || cantidadMarcasDespuesSalidaProgramada > 2
            || conflictosAlternancia.BloquearTiempoExtraAutomatico;
        var cantidadDescansosRealesAplicados = descansosAplicados.Count(d => d.FueMarcado && d.MinutosAplicados > 0);
        var requiereRevisionDescansos = descansosAplicados.Any(d => d.RequiereConfirmacion)
            || cantidadDescansosRealesAplicados > descansosConfigurados.Count
            || minutosTrabajoAdicionalAntes > 0
            || minutosTrabajoAdicionalDespues > 0
            || minutosSalidaTemporal > 0
            || minutosPermisoSegmento > 0
            || conflictosAlternancia.RequiereRevision
            || observaciones.Any(o => o.Contains("pares completos", StringComparison.OrdinalIgnoreCase)
                || o.Contains("sin par", StringComparison.OrdinalIgnoreCase)
                || o.Contains("excedió", StringComparison.OrdinalIgnoreCase)
                || o.Contains("adicional no programado", StringComparison.OrdinalIgnoreCase));

        return new AnalisisJornada(
            marcaciones.Count,
            minutosJornadaProgramada,
            minutosJornadaNetaProgramada,
            minutosTrabajadosBrutos,
            minutosDescansoProgramado,
            minutosDescansoTomado,
            minutosDescansoPagado,
            minutosTrabajadosNetos,
            minutosTrabajoAdicionalAntes,
            minutosTrabajoAdicionalDespues,
            entradaReal,
            salidaReal,
            resumenDescansos,
            observaciones,
            requiereRevisionDescansos,
            autoDescuentoDescansoNoMarcado,
            bloquearTiempoExtraAutomatico,
            esTrabajoAdicionalAntesValido,
            esTrabajoAdicionalDespuesValido);
    }

    private static bool EsTrabajoAdicionalAutomaticoValido(IReadOnlyList<MarcacionProcesada> marcaciones)
    {
        if (marcaciones.Count < 2 || marcaciones.Count % 2 != 0)
        {
            return false;
        }

        return marcaciones.All(m => m.Marcacion.ClasificacionOperativa is TipoClasificacionMarcacionRrhh.Entrada or TipoClasificacionMarcacionRrhh.Salida);
    }

    private static AlternanciaSegmentosResult DetectarConflictosAlternancia(IReadOnlyList<MarcacionProcesada> marcasIntermedias, IReadOnlyList<RrhhSegmentoResolucion> resolucionesSegmento, IReadOnlyList<DescansoConfigurado> descansosConfigurados)
    {
        if (marcasIntermedias.Count < 2)
        {
            return new AlternanciaSegmentosResult([], false, false);
        }

        var observaciones = new List<string>();
        var requiereRevision = false;
        var bloquearTiempoExtraAutomatico = false;

        for (var i = 0; i + 1 < marcasIntermedias.Count; i += 2)
        {
            var inicio = marcasIntermedias[i];
            var fin = marcasIntermedias[i + 1];
            var resolucion = resolucionesSegmento.FirstOrDefault(r => r.Estado == EstadoSegmentoResolucionRrhh.Vigente && r.MarcacionInicioId == inicio.Marcacion.Id && r.MarcacionFinId == fin.Marcacion.Id);
            if (resolucion != null && !resolucion.FueInferidoAutomaticamente)
            {
                continue;
            }

            var accion = ResolverAccionSegmento(inicio.Marcacion, fin.Marcacion, resolucionesSegmento);
            var indiceSegmento = (i / 2) + 1;
            var esperaPausa = indiceSegmento % 2 != 0;
            var minutos = Math.Max(0, (int)Math.Round((fin.FechaLocal - inicio.FechaLocal).TotalMinutes));
            var esPausa = accion is "descanso" or "permiso" or "temporal" || DebeInferirseComoDescansoEnMotor(descansosConfigurados, inicio.FechaLocal.TimeOfDay, fin.FechaLocal.TimeOfDay, minutos);

            if (esperaPausa && !esPausa)
            {
                observaciones.Add($"El tramo intermedio {inicio.FechaLocal:HH:mm}-{fin.FechaLocal:HH:mm} rompe la alternancia base trabajo-pausa; revisar si corresponde descanso, permiso o salida temporal.");
                requiereRevision = true;
                bloquearTiempoExtraAutomatico = true;
            }
        }

        return new AlternanciaSegmentosResult(observaciones, requiereRevision, bloquearTiempoExtraAutomatico);
    }

    private static bool DebeInferirseComoDescansoEnMotor(IReadOnlyList<DescansoConfigurado> descansosConfigurados, TimeSpan inicio, TimeSpan fin, int minutos)
    {
        if (descansosConfigurados.Count == 0 || minutos <= 0)
        {
            return false;
        }

        return ObtenerNumeroDescansoMasCercano(descansosConfigurados, inicio, fin).HasValue;
    }

    private static SegmentosEspecialesResult ExtraerSegmentosEspeciales(IReadOnlyList<MarcacionProcesada> marcasIntermedias, IReadOnlyList<RrhhSegmentoResolucion> resolucionesSegmento, List<string> observaciones)
    {
        if (marcasIntermedias.Count == 0)
        {
            return new SegmentosEspecialesResult([], 0, 0, 0);
        }

        var restantes = new List<MarcacionProcesada>();
        var minutosSalidaTemporal = 0;
        var minutosPermiso = 0;
        var minutosNoConsiderados = 0;

        for (var i = 0; i < marcasIntermedias.Count; i += 2)
        {
            if (i + 1 >= marcasIntermedias.Count)
            {
                restantes.Add(marcasIntermedias[i]);
                continue;
            }

            var inicio = marcasIntermedias[i];
            var fin = marcasIntermedias[i + 1];
            var accion = ResolverAccionSegmento(inicio.Marcacion, fin.Marcacion, resolucionesSegmento);
            var minutos = Math.Max(0, (int)Math.Round((fin.FechaLocal - inicio.FechaLocal).TotalMinutes));

            switch (accion)
            {
                case "trabajo":
                case "extra":
                    observaciones.Add($"Se respetó la resolución manual del tramo {inicio.FechaLocal:HH:mm}-{fin.FechaLocal:HH:mm} como {(accion == "extra" ? "bloque extra" : "trabajo principal")}; no se tomó como descanso.");
                    break;
                case "temporal":
                    minutosSalidaTemporal += minutos;
                    observaciones.Add($"Se detectó salida temporal de {minutos} min entre {inicio.FechaLocal:HH:mm} y {fin.FechaLocal:HH:mm}; no se tomó como descanso.");
                    break;
                case "permiso":
                    minutosPermiso += minutos;
                    observaciones.Add($"Se detectó tramo de permiso de {minutos} min entre {inicio.FechaLocal:HH:mm} y {fin.FechaLocal:HH:mm}; no se tomó como descanso.");
                    break;
                case "ignorar":
                    minutosNoConsiderados += minutos;
                    observaciones.Add($"Se ignoró un tramo de {minutos} min entre {inicio.FechaLocal:HH:mm} y {fin.FechaLocal:HH:mm} para el cálculo del día.");
                    break;
                default:
                    restantes.Add(inicio);
                    restantes.Add(fin);
                    break;
            }
        }

        return new SegmentosEspecialesResult(restantes, minutosSalidaTemporal, minutosPermiso, minutosNoConsiderados);
    }

    private static List<MarcacionProcesada> ExcluirSegmentosTrabajoManual(IReadOnlyList<MarcacionProcesada> marcasIntermedias, IReadOnlyList<RrhhSegmentoResolucion> resolucionesSegmento, List<string> observaciones)
    {
        if (marcasIntermedias.Count < 2)
        {
            return marcasIntermedias.ToList();
        }

        var restantes = new List<MarcacionProcesada>();

        for (var i = 0; i < marcasIntermedias.Count; i += 2)
        {
            if (i + 1 >= marcasIntermedias.Count)
            {
                restantes.Add(marcasIntermedias[i]);
                continue;
            }

            var inicio = marcasIntermedias[i];
            var fin = marcasIntermedias[i + 1];
            var accion = ResolverAccionSegmento(inicio.Marcacion, fin.Marcacion, resolucionesSegmento);

            if (accion is "trabajo" or "extra")
            {
                observaciones.Add($"Se respetó la resolución manual del tramo {inicio.FechaLocal:HH:mm}-{fin.FechaLocal:HH:mm} como {(accion == "extra" ? "bloque extra" : "trabajo principal")}; no se tomó como descanso.");
                continue;
            }

            restantes.Add(inicio);
            restantes.Add(fin);
        }

        return restantes;
    }

    private static string? ResolverAccionSegmento(RrhhMarcacion inicio, RrhhMarcacion fin, IReadOnlyList<RrhhSegmentoResolucion> resolucionesSegmento)
    {
        var resolucion = resolucionesSegmento.FirstOrDefault(r => r.Estado == EstadoSegmentoResolucionRrhh.Vigente && r.MarcacionInicioId == inicio.Id && r.MarcacionFinId == fin.Id);
        if (resolucion != null)
        {
            return resolucion.TipoSegmento switch
            {
                TipoSegmentoResolucionRrhh.SalidaTemporal => "temporal",
                TipoSegmentoResolucionRrhh.Permiso => "permiso",
                TipoSegmentoResolucionRrhh.NoConsiderar => "ignorar",
                TipoSegmentoResolucionRrhh.Descanso => "descanso",
                TipoSegmentoResolucionRrhh.Extra => "extra",
                _ => "trabajo"
            };
        }

        return RrhhMarcacionSegmentActionHelper.ResolveAction(inicio.PayloadRaw, fin.PayloadRaw);
    }

    private static async Task<List<RrhhSegmentoResolucion>> ObtenerResolucionesSegmentoAsync(CrmDbContext db, Guid empresaId, Guid empleadoId, DateOnly fecha, CancellationToken cancellationToken)
    {
        var resoluciones = await db.RrhhSegmentosResoluciones
            .Include(r => r.MarcacionInicio)
            .Include(r => r.MarcacionFin)
            .Where(r => r.EmpresaId == empresaId && r.EmpleadoId == empleadoId && r.Fecha == fecha && r.IsActive)
            .ToListAsync(cancellationToken);

        var marcacionIds = resoluciones
            .SelectMany(r => new[] { r.MarcacionInicioId, r.MarcacionFinId })
            .Distinct()
            .ToList();

        if (marcacionIds.Count == 0)
        {
            return resoluciones;
        }

        var marcacionesPorId = await db.RrhhMarcaciones
            .Include(m => m.Checador)
            .Where(m => marcacionIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, cancellationToken);

        foreach (var resolucion in resoluciones)
        {
            if (resolucion.MarcacionInicio == null && marcacionesPorId.TryGetValue(resolucion.MarcacionInicioId, out var inicio))
            {
                resolucion.MarcacionInicio = inicio;
            }

            if (resolucion.MarcacionFin == null && marcacionesPorId.TryGetValue(resolucion.MarcacionFinId, out var fin))
            {
                resolucion.MarcacionFin = fin;
            }
        }

        return resoluciones;
    }

    private static async Task ConciliarResolucionesSegmentoAsync(
        CrmDbContext db,
        Guid empresaId,
        Guid empleadoId,
        DateOnly fecha,
        IReadOnlyList<MarcacionProcesada> marcaciones,
        IReadOnlyList<RrhhSegmentoResolucion> resolucionesExistentes,
        CancellationToken cancellationToken)
    {
        var paresActivos = marcaciones
            .Zip(marcaciones.Skip(1), (inicio, fin) => (inicio.Marcacion.Id, fin.Marcacion.Id))
            .ToHashSet();

        var resolucionesRastreadas = db.ChangeTracker.Entries<RrhhSegmentoResolucion>()
            .Select(e => e.Entity)
            .Where(r => r.EmpresaId == empresaId && r.EmpleadoId == empleadoId && r.Fecha == fecha)
            .ToList();

        var resolucionesConocidas = resolucionesExistentes
            .Concat(resolucionesRastreadas)
            .GroupBy(r => new { r.MarcacionInicioId, r.MarcacionFinId })
            .Select(g => g.First())
            .ToList();

        foreach (var resolucion in resolucionesConocidas.Where(r => r.Estado == EstadoSegmentoResolucionRrhh.Vigente && !paresActivos.Contains((r.MarcacionInicioId, r.MarcacionFinId))))
        {
            resolucion.Estado = EstadoSegmentoResolucionRrhh.Obsoleta;
            resolucion.UpdatedAt = DateTime.UtcNow;
        }

        foreach (var (inicioId, finId) in paresActivos)
        {
            var resolucionExistente = resolucionesConocidas.FirstOrDefault(r => r.MarcacionInicioId == inicioId && r.MarcacionFinId == finId);
            if (resolucionExistente != null)
            {
                if (resolucionExistente.Estado != EstadoSegmentoResolucionRrhh.Vigente && resolucionExistente.FueInferidoAutomaticamente)
                {
                    resolucionExistente.Estado = EstadoSegmentoResolucionRrhh.RequiereRevision;
                    resolucionExistente.UpdatedAt = DateTime.UtcNow;
                }

                continue;
            }

            var nuevaResolucion = new RrhhSegmentoResolucion
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresaId,
                EmpleadoId = empleadoId,
                Fecha = fecha,
                MarcacionInicioId = inicioId,
                MarcacionFinId = finId,
                TipoSegmento = TipoSegmentoResolucionRrhh.Trabajo,
                Estado = EstadoSegmentoResolucionRrhh.RequiereRevision,
                FueInferidoAutomaticamente = true,
                Observaciones = "Bloque generado o reconstruido tras cambios en marcaciones; requiere revisión.",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await db.RrhhSegmentosResoluciones.AddAsync(nuevaResolucion, cancellationToken);
        }
    }

    private static JornadaSeleccionada SeleccionarJornadaPrincipal(TurnoBaseDetalle? detalleTurno, IReadOnlyList<MarcacionProcesada> marcaciones)
    {
        var entradaFallback = ObtenerMarcacionPorClasificacion(marcaciones, TipoClasificacionMarcacionRrhh.Entrada)
            ?? marcaciones.FirstOrDefault();
        var salidaFallback = ObtenerMarcacionPorClasificacion(marcaciones, TipoClasificacionMarcacionRrhh.Salida)
            ?? marcaciones.LastOrDefault();

        if (entradaFallback == null || salidaFallback == null || salidaFallback.FechaLocal <= entradaFallback.FechaLocal)
        {
            return new JornadaSeleccionada(entradaFallback, salidaFallback);
        }

        if (detalleTurno?.HoraEntrada is not TimeSpan entradaProgramada || detalleTurno.HoraSalida is not TimeSpan salidaProgramada)
        {
            return new JornadaSeleccionada(entradaFallback, salidaFallback);
        }

        var entradaCandidatas = marcaciones
            .Where(m => m.Marcacion.ClasificacionOperativa == TipoClasificacionMarcacionRrhh.Entrada)
            .Append(marcaciones.First())
            .Distinct()
            .OrderBy(m => m.FechaLocal)
            .ToList();
        var salidaCandidatas = marcaciones
            .Where(m => m.Marcacion.ClasificacionOperativa == TipoClasificacionMarcacionRrhh.Salida)
            .Append(marcaciones.Last())
            .Distinct()
            .OrderBy(m => m.FechaLocal)
            .ToList();

        if (entradaCandidatas.Count <= 1 || salidaCandidatas.Count <= 1)
        {
            return new JornadaSeleccionada(entradaFallback, salidaFallback);
        }

        var mejorEntrada = entradaFallback;
        var mejorSalida = salidaFallback;
        var mejorTraslape = CalcularMinutosTraslapeTurno(entradaFallback.FechaLocal.TimeOfDay, salidaFallback.FechaLocal.TimeOfDay, entradaProgramada, salidaProgramada);
        var mejorDesviacion = CalcularDesviacionTurno(entradaFallback.FechaLocal.TimeOfDay, salidaFallback.FechaLocal.TimeOfDay, entradaProgramada, salidaProgramada);
        var mejorDuracion = Math.Abs((int)Math.Round((salidaFallback.FechaLocal - entradaFallback.FechaLocal).TotalMinutes) - Math.Max(0, (int)Math.Round((salidaProgramada - entradaProgramada).TotalMinutes)));

        foreach (var entrada in entradaCandidatas)
        {
            foreach (var salida in salidaCandidatas.Where(s => s.FechaLocal > entrada.FechaLocal))
            {
                var traslape = CalcularMinutosTraslapeTurno(entrada.FechaLocal.TimeOfDay, salida.FechaLocal.TimeOfDay, entradaProgramada, salidaProgramada);
                var desviacion = CalcularDesviacionTurno(entrada.FechaLocal.TimeOfDay, salida.FechaLocal.TimeOfDay, entradaProgramada, salidaProgramada);
                var duracion = Math.Abs((int)Math.Round((salida.FechaLocal - entrada.FechaLocal).TotalMinutes) - Math.Max(0, (int)Math.Round((salidaProgramada - entradaProgramada).TotalMinutes)));

                if (traslape > mejorTraslape
                    || (traslape == mejorTraslape && desviacion < mejorDesviacion)
                    || (traslape == mejorTraslape && desviacion == mejorDesviacion && duracion < mejorDuracion))
                {
                    mejorEntrada = entrada;
                    mejorSalida = salida;
                    mejorTraslape = traslape;
                    mejorDesviacion = desviacion;
                    mejorDuracion = duracion;
                }
            }
        }

        return new JornadaSeleccionada(mejorEntrada, mejorSalida);
    }

    private static int CalcularMinutosTrabajoAdicional(IReadOnlyList<MarcacionProcesada> marcaciones, string etiqueta, List<string> observaciones)
    {
        if (marcaciones.Count == 0)
        {
            return 0;
        }

        var minutos = 0;
        for (var i = 0; i + 1 < marcaciones.Count; i += 2)
        {
            var inicio = marcaciones[i].FechaLocal;
            var fin = marcaciones[i + 1].FechaLocal;
            if (fin > inicio)
            {
                minutos += Math.Max(0, (int)Math.Round((fin - inicio).TotalMinutes));
            }
        }

        if (marcaciones.Count % 2 != 0)
        {
            observaciones.Add($"Hay una marcación {etiqueta} sin par; confirma si debe ignorarse o corregirse.");
        }

        return minutos;
    }

    private static int CalcularMinutosTraslapeTurno(TimeSpan entradaReal, TimeSpan salidaReal, TimeSpan entradaProgramada, TimeSpan salidaProgramada)
    {
        var inicio = entradaReal > entradaProgramada ? entradaReal : entradaProgramada;
        var fin = salidaReal < salidaProgramada ? salidaReal : salidaProgramada;
        return fin <= inicio
            ? 0
            : Math.Max(0, (int)Math.Round((fin - inicio).TotalMinutes));
    }

    private static int CalcularDesviacionTurno(TimeSpan entradaReal, TimeSpan salidaReal, TimeSpan entradaProgramada, TimeSpan salidaProgramada)
        => Math.Abs((int)Math.Round((entradaReal - entradaProgramada).TotalMinutes))
            + Math.Abs((int)Math.Round((salidaReal - salidaProgramada).TotalMinutes));

    private static PoliticaDescansoNoMarcado? ResolverDescansosNoMarcados(
        IReadOnlyList<DescansoConfigurado> descansosConfigurados,
        int minutosTrabajadosBrutos,
        int minutosJornadaProgramada,
        int minutosJornadaNetaProgramada,
        RrhhAsistenciaDescansoSettings configuracionDescansos)
    {
        if (descansosConfigurados.Count == 0)
        {
            return null;
        }

        var minutosDescansoPagadoProgramado = descansosConfigurados.Where(d => d.EsPagado).Sum(d => d.Minutos);
        var minutosDescansoNoPagadoProgramado = descansosConfigurados.Where(d => !d.EsPagado).Sum(d => d.Minutos);
        if (minutosDescansoNoPagadoProgramado <= 0)
        {
            return new PoliticaDescansoNoMarcado(
                0,
                0,
                minutosTrabajadosBrutos,
                "No se detectaron descansos tomados",
                ["Hay descansos configurados, pero todos son pagados y no requieren descuento automático."],
                false,
                false,
                false);
        }

        var diferenciaContraBruta = Math.Abs(minutosTrabajadosBrutos - minutosJornadaProgramada);
        var diferenciaContraNeta = Math.Abs(minutosTrabajadosBrutos - minutosJornadaNetaProgramada);
        var umbralAmbiguo = Math.Max(
            configuracionDescansos.ZonaAmbiguaHastaMinutos,
            Math.Max(configuracionDescansos.ToleranciaCoincidenciaBrutaMinutos, configuracionDescansos.ToleranciaCoincidenciaNetaMinutos));

        var coincideBruta = diferenciaContraBruta <= configuracionDescansos.ToleranciaCoincidenciaBrutaMinutos;
        var coincideNeta = diferenciaContraNeta <= configuracionDescansos.ToleranciaCoincidenciaNetaMinutos;

        if (coincideBruta && !coincideNeta)
        {
            return new PoliticaDescansoNoMarcado(
                minutosDescansoPagadoProgramado + minutosDescansoNoPagadoProgramado,
                minutosDescansoPagadoProgramado,
                Math.Max(0, minutosTrabajadosBrutos - minutosDescansoNoPagadoProgramado),
                $"Descansos inferidos automáticamente: {minutosDescansoNoPagadoProgramado} min no pagados",
                [$"No se detectaron marcaciones de descanso. Se descontaron {minutosDescansoNoPagadoProgramado} min no pagados por coincidencia con jornada bruta (±{configuracionDescansos.ToleranciaCoincidenciaBrutaMinutos} min)."],
                false,
                true,
                false);
        }

        if (coincideNeta && !coincideBruta)
        {
            return new PoliticaDescansoNoMarcado(
                0,
                0,
                minutosTrabajadosBrutos,
                "Sin descansos marcados; probable salida temprana compensada",
                [$"No se detectaron marcaciones de descanso. La jornada coincide con la neta programada (±{configuracionDescansos.ToleranciaCoincidenciaNetaMinutos} min); confirmar si el empleado salió temprano en lugar de tomar el descanso."],
                true,
                false,
                true);
        }

        if (coincideBruta && coincideNeta)
        {
            return new PoliticaDescansoNoMarcado(
                0,
                0,
                minutosTrabajadosBrutos,
                "Sin descansos marcados; caso ambiguo",
                [$"No se detectaron marcaciones de descanso. La jornada cae en la zona ambigua entre tiempo bruto y neto; revisar manualmente (hasta {umbralAmbiguo} min)."],
                true,
                false,
                true);
        }

        if (diferenciaContraBruta <= umbralAmbiguo || diferenciaContraNeta <= umbralAmbiguo)
        {
            return new PoliticaDescansoNoMarcado(
                0,
                0,
                minutosTrabajadosBrutos,
                "Sin descansos marcados; zona ambigua",
                [$"No se detectaron marcaciones de descanso. La jornada quedó dentro de la zona ambigua de revisión ({Math.Min(diferenciaContraBruta, diferenciaContraNeta)} min de diferencia; tope {umbralAmbiguo} min)."],
                true,
                false,
                true);
        }

        if (minutosTrabajadosBrutos > minutosJornadaProgramada)
        {
            return new PoliticaDescansoNoMarcado(
                0,
                0,
                minutosTrabajadosBrutos,
                "Sin descansos marcados; validar tiempo adicional",
                ["No se detectaron marcaciones de descanso y la jornada supera la bruta programada; confirmar antes de autorizar tiempo extra."],
                true,
                false,
                true);
        }

        return new PoliticaDescansoNoMarcado(
            0,
            0,
            minutosTrabajadosBrutos,
            "Sin descansos marcados; requiere confirmación",
            ["No se detectaron marcaciones de descanso y la jornada no coincide de forma clara con el turno; confirmar si hubo salida anticipada o descuento manual."],
            true,
            false,
            true);
    }

    private static MarcacionProcesada? ObtenerMarcacionPorClasificacion(IEnumerable<MarcacionProcesada> marcaciones, TipoClasificacionMarcacionRrhh clasificacion)
        => marcaciones.FirstOrDefault(m => m.Marcacion.ClasificacionOperativa == clasificacion);

    private static List<DescansoConfigurado> ObtenerDescansosConfigurados(TurnoBaseDetalle? detalleTurno)
    {
        if (detalleTurno == null || !detalleTurno.Labora)
        {
            return [];
        }

        return detalleTurno.Descansos
            .Where(d => d.HoraInicio.HasValue && d.HoraFin.HasValue)
            .OrderBy(d => d.Orden)
            .Select(d => new DescansoConfigurado(d.Orden, d.HoraInicio!.Value, d.HoraFin!.Value, d.EsPagado))
            .ToList();
    }

    private static List<DescansoConfigurado> ObtenerDescansosConfiguradosOInferidos(TurnoBaseDetalle? detalleTurno, IReadOnlyList<DescansoTomado> descansosTomados, IReadOnlyList<RrhhSegmentoResolucion> resolucionesSegmento)
    {
        var configurados = ObtenerDescansosConfigurados(detalleTurno);
        if (configurados.Count > 0 || detalleTurno == null || !detalleTurno.Labora)
        {
            return configurados;
        }

        var descansosBase = descansosTomados
            .Where(d => d.Minutos >= 25 || resolucionesSegmento.Any(r => r.TipoSegmento == TipoSegmentoResolucionRrhh.Descanso && SegmentoCoincideConResolucion(r, d.Salida, d.Regreso)))
            .OrderBy(d => d.Salida)
            .Select((d, index) => new DescansoConfigurado(
                index + 1,
                d.Salida.TimeOfDay,
                d.Regreso.TimeOfDay,
                d.EsPagado))
            .ToList();

        if (descansosBase.Count > 0)
        {
            return descansosBase;
        }

        return resolucionesSegmento
            .Where(r => r.Estado == EstadoSegmentoResolucionRrhh.Vigente && r.TipoSegmento == TipoSegmentoResolucionRrhh.Trabajo)
            .Select(r => CrearDescansoConfiguradoDesdeResolucion(r))
            .Where(d => d != null)
            .Select((d, index) => d! with { Numero = index + 1 })
            .ToList();
    }

    private static DescansoConfigurado? CrearDescansoConfiguradoDesdeResolucion(RrhhSegmentoResolucion resolucion)
    {
        if (resolucion.MarcacionInicio == null || resolucion.MarcacionFin == null)
        {
            return null;
        }

        var inicio = ObtenerFechaHoraLocalMarcacion(resolucion.MarcacionInicio);
        var fin = ObtenerFechaHoraLocalMarcacion(resolucion.MarcacionFin);
        if (fin <= inicio)
        {
            return null;
        }

        return new DescansoConfigurado(1, inicio.TimeOfDay, fin.TimeOfDay, false);
    }

    private static List<DescansoAplicado> CalcularDescansosAplicados(
        IReadOnlyList<DescansoConfigurado> descansosConfigurados,
        IReadOnlyList<DescansoTomado> descansosTomados,
        IReadOnlyList<RrhhSegmentoResolucion> resolucionesSegmento,
        int minutosSalidaAnticipada,
        PermisoParcialDia? permisoParcial,
        RrhhAsistenciaDescansoSettings configuracionDescansos,
        List<string> observaciones)
    {
        if (descansosConfigurados.Count == 0 && descansosTomados.Count == 0)
        {
            return [];
        }

        var aplicados = new List<DescansoAplicado>();
        var resolucionesTrabajo = resolucionesSegmento
            .Where(r => r.Estado == EstadoSegmentoResolucionRrhh.Vigente && r.TipoSegmento == TipoSegmentoResolucionRrhh.Trabajo)
            .ToList();
        var descansosTomadosFiltrados = descansosTomados
            .Where(tomado => !resolucionesTrabajo.Any(r => SegmentoCoincideConResolucion(r, tomado.Salida, tomado.Regreso)))
            .ToList();
        var (tomadosPorNumero, descansosAdicionales) = EmparejarDescansosTomados(descansosConfigurados, descansosTomadosFiltrados, resolucionesSegmento);
        var faltantesCubiertosPorSalida = new HashSet<int>();
        var descansosResueltosComoTrabajo = resolucionesTrabajo
            .Select(resolucion => ObtenerNumeroDescansoResueltoComoTrabajo(descansosConfigurados, resolucion))
            .Where(numero => numero.HasValue)
            .Select(numero => numero!.Value)
            .ToHashSet();
        var minutosSalidaDisponibles = Math.Max(0, minutosSalidaAnticipada);

        foreach (var configurado in descansosConfigurados
                     .Where(d => !d.EsPagado)
                     .OrderByDescending(d => d.Numero))
        {
            if (tomadosPorNumero.ContainsKey(configurado.Numero))
            {
                continue;
            }

            if (minutosSalidaDisponibles >= configurado.Minutos)
            {
                faltantesCubiertosPorSalida.Add(configurado.Numero);
                minutosSalidaDisponibles -= configurado.Minutos;
            }
        }

        foreach (var configurado in descansosConfigurados.OrderBy(d => d.Numero))
        {
            if (tomadosPorNumero.TryGetValue(configurado.Numero, out var tomado))
            {
                var excesoMinutos = Math.Max(0, tomado.Minutos - configurado.Minutos);
                var excedeTolerancia = excesoMinutos > configuracionDescansos.ToleranciaExcesoDescansoMinutos;
                var minutosAplicados = configurado.EsPagado
                    ? tomado.Minutos
                    : tomado.Minutos < configurado.Minutos
                        ? configurado.Minutos
                        : excedeTolerancia
                            ? tomado.Minutos
                            : configurado.Minutos;

                var overrideManual = ResolverMinutosAplicadosOverride(resolucionesSegmento, tomado.Salida, tomado.Regreso);
                if (overrideManual.HasValue)
                {
                    minutosAplicados = Math.Max(0, overrideManual.Value);
                    observaciones.Add($"El descanso {configurado.Numero} aplicó override manual de {minutosAplicados} min.");
                }

                if (!configurado.EsPagado && tomado.Minutos < configurado.Minutos)
                {
                    observaciones.Add($"El descanso {configurado.Numero} registró {tomado.Minutos} min; se aplicaron {configurado.Minutos} min programados.");
                }
                else if (excesoMinutos > 0 && excedeTolerancia)
                {
                    observaciones.Add($"El descanso {configurado.Numero} excedió su duración programada por {excesoMinutos} min.");
                }

                aplicados.Add(new DescansoAplicado(
                    configurado.Numero,
                    configurado.Inicio,
                    configurado.Fin,
                    configurado.EsPagado,
                    tomado.Salida,
                    tomado.Regreso,
                    tomado.Minutos,
                    minutosAplicados,
                    true,
                    false,
                    false,
                    false));

                continue;
            }

            if (!configurado.EsPagado && permisoParcial != null)
            {
                observaciones.Add($"No se descontó el descanso {configurado.Numero} porque existe un permiso parcial registrado para el día ({permisoParcial.Horas:0.##} h).");
                aplicados.Add(new DescansoAplicado(
                    configurado.Numero,
                    configurado.Inicio,
                    configurado.Fin,
                    configurado.EsPagado,
                    null,
                    null,
                    0,
                    0,
                    false,
                    false,
                    false,
                    true));

                continue;
            }

            if (descansosResueltosComoTrabajo.Contains(configurado.Numero))
            {
                aplicados.Add(new DescansoAplicado(
                    configurado.Numero,
                    configurado.Inicio,
                    configurado.Fin,
                    configurado.EsPagado,
                    null,
                    null,
                    0,
                    0,
                    false,
                    false,
                    false,
                    false));

                continue;
            }

            if (!configurado.EsPagado && faltantesCubiertosPorSalida.Contains(configurado.Numero))
            {
                observaciones.Add($"No se detectó el descanso {configurado.Numero}; la salida anticipada sugiere permiso o descanso no tomado. Confirmar si corresponde descontarlo.");
                aplicados.Add(new DescansoAplicado(
                    configurado.Numero,
                    configurado.Inicio,
                    configurado.Fin,
                    configurado.EsPagado,
                    null,
                    null,
                    0,
                    0,
                    false,
                    true,
                    false,
                    true));

                continue;
            }

            var minutosPlaneados = configurado.EsPagado ? 0 : configurado.Minutos;
            if (!configurado.EsPagado)
            {
                observaciones.Add($"No se detectó el descanso {configurado.Numero}; se aplicaron {configurado.Minutos} min programados.");
            }

            aplicados.Add(new DescansoAplicado(
                configurado.Numero,
                configurado.Inicio,
                configurado.Fin,
                configurado.EsPagado,
                null,
                null,
                0,
                minutosPlaneados,
                false,
                false,
                false,
                false));
        }

        var siguienteNumeroAdicional = Math.Max(
            descansosConfigurados.Count == 0 ? 0 : descansosConfigurados.Max(d => d.Numero),
            aplicados.Count == 0 ? 0 : aplicados.Max(d => d.Numero)) + 1;

        foreach (var adicional in descansosAdicionales)
        {
            observaciones.Add($"Se detectó un descanso adicional no programado de {adicional.Minutos} min.");
            aplicados.Add(new DescansoAplicado(
                siguienteNumeroAdicional,
                adicional.Salida.TimeOfDay,
                adicional.Regreso.TimeOfDay,
                adicional.EsPagado,
                adicional.Salida,
                adicional.Regreso,
                adicional.Minutos,
                adicional.Minutos,
                true,
                false,
                false,
                false));
            siguienteNumeroAdicional++;
        }

        return aplicados;
    }

    private static int? ResolverMinutosAplicadosOverride(IReadOnlyList<RrhhSegmentoResolucion> resolucionesSegmento, DateTime salida, DateTime regreso)
    {
        const int toleranciaMinutos = 2;

        var resolucion = resolucionesSegmento
            .Where(r => r.Estado == EstadoSegmentoResolucionRrhh.Vigente
                && r.TipoSegmento == TipoSegmentoResolucionRrhh.Descanso
                && r.MinutosAplicadosOverride.HasValue)
            .Select(r => new
            {
                Resolucion = r,
                Diferencia = ResolverDiferenciaSegmento(r, salida, regreso)
            })
            .Where(x => x.Diferencia.HasValue && x.Diferencia.Value <= toleranciaMinutos)
            .OrderBy(x => x.Diferencia)
            .Select(x => x.Resolucion)
            .FirstOrDefault();

        return resolucion?.MinutosAplicadosOverride;
    }

    private static int? ObtenerNumeroDescansoMasCercano(IReadOnlyList<DescansoConfigurado> descansosConfigurados, TimeSpan inicio, TimeSpan fin)
    {
        var mejor = descansosConfigurados
            .Select(d => new
            {
                d.Numero,
                Duracion = d.Minutos,
                Diferencia = Math.Abs((int)Math.Round((inicio - d.Inicio).TotalMinutes)) + Math.Abs((int)Math.Round((fin - d.Fin).TotalMinutes))
            })
            .Where(d => d.Duracion <= 0 || MinutosCoinciden(d.Duracion, inicio, fin))
            .OrderBy(d => d.Diferencia)
            .FirstOrDefault();

        return mejor?.Numero;

        static bool MinutosCoinciden(int duracionProgramada, TimeSpan inicioBloque, TimeSpan finBloque)
        {
            var duracionBloque = Math.Max(0, (int)Math.Round((finBloque - inicioBloque).TotalMinutes));
            return duracionBloque <= duracionProgramada + 30;
        }
    }

    private static int? ObtenerNumeroDescansoResueltoComoTrabajo(IReadOnlyList<DescansoConfigurado> descansosConfigurados, RrhhSegmentoResolucion resolucion)
    {
        if (resolucion.MarcacionInicio == null || resolucion.MarcacionFin == null)
        {
            return null;
        }

        var inicio = ObtenerFechaHoraLocalMarcacion(resolucion.MarcacionInicio);
        var fin = ObtenerFechaHoraLocalMarcacion(resolucion.MarcacionFin);
        return ObtenerNumeroDescansoMasCercano(descansosConfigurados, inicio.TimeOfDay, fin.TimeOfDay);
    }

    private static int? ResolverDiferenciaSegmento(RrhhSegmentoResolucion resolucion, DateTime salida, DateTime regreso)
    {
        if (resolucion.MarcacionInicio == null || resolucion.MarcacionFin == null)
        {
            return null;
        }

        var salidaResolucion = ObtenerFechaHoraLocalMarcacion(resolucion.MarcacionInicio);
        var regresoResolucion = ObtenerFechaHoraLocalMarcacion(resolucion.MarcacionFin);
        return Math.Abs((int)Math.Round((salidaResolucion - salida).TotalMinutes))
            + Math.Abs((int)Math.Round((regresoResolucion - regreso).TotalMinutes));
    }

    private static bool SegmentoCoincideConResolucion(RrhhSegmentoResolucion resolucion, DateTime salida, DateTime regreso)
    {
        if (ResolverDiferenciaSegmento(resolucion, salida, regreso) is int diferencia)
        {
            return diferencia <= 2;
        }

        var inicio = resolucion.MarcacionInicio?.FechaHoraMarcacionLocal;
        var fin = resolucion.MarcacionFin?.FechaHoraMarcacionLocal;
        if (inicio.HasValue && fin.HasValue)
        {
            return Math.Abs((int)Math.Round((inicio.Value - salida).TotalMinutes))
                + Math.Abs((int)Math.Round((fin.Value - regreso).TotalMinutes)) <= 2;
        }

        return false;
    }

    private static (Dictionary<int, DescansoTomado> TomadosPorNumero, List<DescansoTomado> DescansosAdicionales) EmparejarDescansosTomados(
        IReadOnlyList<DescansoConfigurado> descansosConfigurados,
        IReadOnlyList<DescansoTomado> descansosTomados,
        IReadOnlyList<RrhhSegmentoResolucion> resolucionesSegmento)
    {
        if (descansosConfigurados.Count == 0 || descansosTomados.Count == 0)
        {
            return ([], descansosTomados.ToList());
        }

        var tomadosPorNumeroManual = new Dictionary<int, DescansoTomado>();
        var descansosTomadosRestantes = descansosTomados.ToList();

        foreach (var resolucion in resolucionesSegmento.Where(r => r.Estado == EstadoSegmentoResolucionRrhh.Vigente && r.TipoSegmento == TipoSegmentoResolucionRrhh.Descanso && !string.IsNullOrWhiteSpace(r.Observaciones)))
        {
            var numeroManual = ExtraerNumeroDescansoManual(resolucion.Observaciones!);
            if (!numeroManual.HasValue || tomadosPorNumeroManual.ContainsKey(numeroManual.Value))
            {
                continue;
            }

            var tomado = descansosTomadosRestantes.FirstOrDefault(d => ResolverDiferenciaSegmento(resolucion, d.Salida, d.Regreso) is int diferencia && diferencia <= 2);
            if (tomado == null)
            {
                continue;
            }

            tomadosPorNumeroManual[numeroManual.Value] = tomado;
            descansosTomadosRestantes.Remove(tomado);
        }

        const int costoDescansoNoEmparejado = 240;
        var configuradosPendientes = descansosConfigurados.Where(d => !tomadosPorNumeroManual.ContainsKey(d.Numero)).ToList();
        var asignacionActual = new int?[configuradosPendientes.Count];
        var mejorAsignacion = new int?[configuradosPendientes.Count];
        var usados = new bool[descansosTomadosRestantes.Count];
        var mejorCosto = int.MaxValue;

        void Buscar(int indiceConfigurado, int costoAcumulado)
        {
            if (costoAcumulado >= mejorCosto)
            {
                return;
            }

            if (indiceConfigurado >= configuradosPendientes.Count)
            {
                mejorCosto = costoAcumulado;
                Array.Copy(asignacionActual, mejorAsignacion, asignacionActual.Length);
                return;
            }

            asignacionActual[indiceConfigurado] = null;
            Buscar(indiceConfigurado + 1, costoAcumulado + costoDescansoNoEmparejado);

            for (var i = 0; i < descansosTomadosRestantes.Count; i++)
            {
                if (usados[i])
                {
                    continue;
                }

                usados[i] = true;
                asignacionActual[indiceConfigurado] = i;
                Buscar(indiceConfigurado + 1, costoAcumulado + CalcularPuntajeEmparejamientoDescanso(configuradosPendientes[indiceConfigurado], descansosTomadosRestantes[i]));
                usados[i] = false;
                asignacionActual[indiceConfigurado] = null;
            }
        }

        Buscar(0, 0);

        var tomadosPorNumero = tomadosPorNumeroManual.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        var descansosAsignados = new HashSet<int>();
        for (var i = 0; i < mejorAsignacion.Length; i++)
        {
            if (mejorAsignacion[i] is not int indiceTomado)
            {
                continue;
            }

            var descansoTomado = descansosTomadosRestantes[indiceTomado];
            tomadosPorNumero[configuradosPendientes[i].Numero] = descansoTomado;
            descansosAsignados.Add(indiceTomado);
        }

        var descansosAdicionales = descansosTomadosRestantes
            .Where((_, indice) => !descansosAsignados.Contains(indice))
            .ToList();

        return (tomadosPorNumero, descansosAdicionales);
    }

    private static int? ExtraerNumeroDescansoManual(string observaciones)
    {
        var match = System.Text.RegularExpressions.Regex.Match(observaciones, @"\(D(?<numero>\d+)\)");
        return match.Success && int.TryParse(match.Groups["numero"].Value, out var numero)
            ? numero
            : null;
    }

    private static int CalcularPuntajeEmparejamientoDescanso(DescansoConfigurado configurado, DescansoTomado tomado)
    {
        var diferenciaInicio = Math.Abs((int)Math.Round((tomado.Salida.TimeOfDay - configurado.Inicio).TotalMinutes));
        var diferenciaFin = Math.Abs((int)Math.Round((tomado.Regreso.TimeOfDay - configurado.Fin).TotalMinutes));
        var diferenciaDuracion = Math.Abs(tomado.Minutos - configurado.Minutos);
        return diferenciaInicio + diferenciaFin + (diferenciaDuracion * 2);
    }

    private static string? ConstruirResumenDescansos(IReadOnlyList<DescansoConfigurado> descansosConfigurados, IReadOnlyList<DescansoAplicado> descansosAplicados)
    {
        if (descansosConfigurados.Count == 0 && descansosAplicados.Count == 0)
        {
            return "Sin descansos configurados";
        }

        if (descansosAplicados.Count == 0)
        {
            return "No se detectaron descansos tomados";
        }

        return string.Join(" | ", descansosAplicados
            .OrderBy(d => d.Numero)
            .Select(d => d.FueCubiertoPorPermiso
                ? $"D{d.Numero}: sin marcar; cubierto por permiso del día ({d.InicioProgramado:hh\\:mm}-{d.FinProgramado:hh\\:mm})"
                : d.RequiereConfirmacion
                ? $"D{d.Numero}: sin marcar; confirmar permiso o descuento ({d.InicioProgramado:hh\\:mm}-{d.FinProgramado:hh\\:mm})"
                : d.FueMarcado
                    ? $"D{d.Numero}: {d.SalidaReal:HH:mm}-{d.RegresoReal:HH:mm} (real {d.MinutosReales} min, aplicado {d.MinutosAplicados} min{(d.EsPagado ? ", pagado" : string.Empty)})"
                    : $"D{d.Numero}: sin marcar; aplicado {d.MinutosAplicados} min programados{(d.EsPagado ? " pagados" : string.Empty)}"));
    }

    private static async Task<PermisoParcialDia?> ObtenerPermisoParcialDiaAsync(CrmDbContext db, Guid empresaId, Guid empleadoId, DateOnly fecha, CancellationToken cancellationToken)
    {
        var permiso = await db.RrhhAusencias
            .AsNoTracking()
            .Where(a => a.EmpresaId == empresaId
                && a.EmpleadoId == empleadoId
                && a.IsActive
                && a.Tipo == TipoAusenciaRrhh.Permiso
                && (a.Estatus == EstatusAusenciaRrhh.Aprobada || a.Estatus == EstatusAusenciaRrhh.Aplicada)
                && a.Horas > 0
                && a.FechaInicio <= fecha
                && a.FechaFin >= fecha)
            .OrderByDescending(a => a.FechaAprobacion ?? a.UpdatedAt ?? a.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return permiso == null
            ? null
            : new PermisoParcialDia(permiso.Horas, permiso.ConGocePago, permiso.Motivo);
    }

    private static string ConstruirResultadoMarcacion(RrhhAsistenciaEstatus estatus, string? observaciones)
    {
        var resultado = string.IsNullOrWhiteSpace(observaciones)
            ? $"Clasificada como {ObtenerTextoEstatus(estatus)}"
            : $"Clasificada como {ObtenerTextoEstatus(estatus)}. {observaciones}";

        return resultado.Length <= MaxLongitudResultadoMarcacion
            ? resultado
            : resultado[..(MaxLongitudResultadoMarcacion - 1)].TrimEnd() + "…";
    }

    private static async Task<TurnoBase?> ResolverTurnoVigenteAsync(CrmDbContext db, Guid empresaId, Empleado empleado, DateOnly fecha, CancellationToken cancellationToken)
    {
        var vigencia = await db.RrhhEmpleadosTurno
            .AsNoTracking()
            .Include(v => v.TurnoBase)
                .ThenInclude(t => t.Detalles)
                    .ThenInclude(d => d.Descansos)
            .Where(v => v.EmpresaId == empresaId
                && v.EmpleadoId == empleado.Id
                && v.IsActive
                && v.VigenteDesde <= fecha
                && (v.VigenteHasta == null || v.VigenteHasta >= fecha))
            .OrderByDescending(v => v.VigenteDesde)
            .FirstOrDefaultAsync(cancellationToken);

        if (vigencia?.TurnoBase != null)
        {
            return vigencia.TurnoBase;
        }

        return empleado.TurnoBase;
    }

    private static string ObtenerTextoEstatus(RrhhAsistenciaEstatus estatus)
        => estatus switch
        {
            RrhhAsistenciaEstatus.AsistenciaNormal => "asistencia normal",
            RrhhAsistenciaEstatus.Retardo => "retardo",
            RrhhAsistenciaEstatus.Falta => "falta",
            RrhhAsistenciaEstatus.Descanso => "descanso",
            RrhhAsistenciaEstatus.DescansoTrabajado => "descanso trabajado",
            RrhhAsistenciaEstatus.Incompleta => "asistencia incompleta",
            RrhhAsistenciaEstatus.TurnoNoAsignado => "turno no asignado",
            RrhhAsistenciaEstatus.MarcaNoReconocida => "marca no reconocida",
            _ => "pendiente"
        };

    private static DiaSemanaTurno MapDiaSemana(DayOfWeek dayOfWeek)
        => dayOfWeek switch
        {
            DayOfWeek.Monday => DiaSemanaTurno.Lunes,
            DayOfWeek.Tuesday => DiaSemanaTurno.Martes,
            DayOfWeek.Wednesday => DiaSemanaTurno.Miercoles,
            DayOfWeek.Thursday => DiaSemanaTurno.Jueves,
            DayOfWeek.Friday => DiaSemanaTurno.Viernes,
            DayOfWeek.Saturday => DiaSemanaTurno.Sabado,
            _ => DiaSemanaTurno.Domingo
        };

    private static (DateTime InicioUtc, DateTime FinUtc) ObtenerRangoUtcDelDia(DateOnly fecha, TimeZoneInfo zona)
    {
        var inicioLocal = fecha.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        var finLocal = fecha.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return (TimeZoneInfo.ConvertTimeToUtc(inicioLocal, zona), TimeZoneInfo.ConvertTimeToUtc(finLocal, zona));
    }

    private static DateTime ConvertirUtcALocal(DateTime fechaUtc, TimeZoneInfo zona)
    {
        var utc = fechaUtc.Kind == DateTimeKind.Utc ? fechaUtc : DateTime.SpecifyKind(fechaUtc, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utc, zona);
    }

    private static DateTime ObtenerFechaHoraLocalMarcacion(RrhhMarcacion marcacion)
    {
        if (marcacion.FechaHoraMarcacionLocal.HasValue)
        {
            return DateTime.SpecifyKind(marcacion.FechaHoraMarcacionLocal.Value, DateTimeKind.Unspecified);
        }

        var zonaHoraria = string.IsNullOrWhiteSpace(marcacion.ZonaHorariaAplicada)
            ? marcacion.Checador?.ZonaHoraria
            : marcacion.ZonaHorariaAplicada;

        return DateTime.SpecifyKind(ConvertirUtcALocal(marcacion.FechaHoraMarcacionUtc, ResolverZonaHoraria(zonaHoraria)), DateTimeKind.Unspecified);
    }

    private sealed record DescansoConfigurado(int Numero, TimeSpan Inicio, TimeSpan Fin, bool EsPagado)
    {
        public int Minutos => Math.Max(0, (int)Math.Round((Fin - Inicio).TotalMinutes));
    }

    private sealed record DescansoTomado(int Numero, DateTime Salida, DateTime Regreso, int Minutos, bool EsPagado);

    private sealed record DescansoAplicado(
        int Numero,
        TimeSpan InicioProgramado,
        TimeSpan FinProgramado,
        bool EsPagado,
        DateTime? SalidaReal,
        DateTime? RegresoReal,
        int MinutosReales,
        int MinutosAplicados,
        bool FueMarcado,
        bool FueCubiertoPorSalidaAnticipada,
        bool RequiereConfirmacion,
        bool FueCubiertoPorPermiso);

    private sealed record AnalisisJornada(
        int TotalMarcaciones,
        int MinutosJornadaProgramada,
        int MinutosJornadaNetaProgramada,
        int MinutosTrabajadosBrutos,
        int MinutosDescansoProgramado,
        int MinutosDescansoTomado,
        int MinutosDescansoPagado,
        int MinutosTrabajadosNetos,
        int MinutosTrabajoAdicionalAntes,
        int MinutosTrabajoAdicionalDespues,
        TimeSpan? EntradaReal,
        TimeSpan? SalidaReal,
        string? ResumenDescansos,
        IReadOnlyList<string> ObservacionesRevision,
        bool RequiereRevisionDescansos,
        bool AutoDescuentoDescansoNoMarcado,
        bool BloquearTiempoExtraAutomatico,
        bool EsTrabajoAdicionalAntesValido,
        bool EsTrabajoAdicionalDespuesValido)
    {
        public int MinutosDescansoNoPagado => Math.Max(0, MinutosDescansoTomado - MinutosDescansoPagado);
        public bool RequiereRevision => RequiereRevisionDescansos;
    }

    private sealed record JornadaSeleccionada(MarcacionProcesada? Entrada, MarcacionProcesada? Salida);

    private sealed record PoliticaDescansoNoMarcado(
        int MinutosDescansoTomado,
        int MinutosDescansoPagado,
        int MinutosTrabajadosNetos,
        string ResumenDescansos,
        IReadOnlyList<string> Observaciones,
        bool RequiereRevision,
        bool AutoDescuentoAplicado,
        bool BloquearTiempoExtraAutomatico);

    private sealed record SegmentosEspecialesResult(
        List<MarcacionProcesada> MarcasRestantes,
        int MinutosSalidaTemporal,
        int MinutosPermiso,
        int MinutosNoConsiderados);

    private sealed record AlternanciaSegmentosResult(
        IReadOnlyList<string> Observaciones,
        bool RequiereRevision,
        bool BloquearTiempoExtraAutomatico);

    private sealed record PermisoParcialDia(decimal Horas, bool ConGocePago, string? Motivo);

    private sealed record MarcacionProcesada(RrhhMarcacion Marcacion, DateTime FechaLocal);

    private static TimeZoneInfo ResolverZonaHoraria(string? zonaHoraria)
    {
        if (string.IsNullOrWhiteSpace(zonaHoraria))
        {
            return ResolverZonaHoraria(DefaultMexicoTimeZone);
        }

        if (TryCreateUtcOffsetTimeZone(zonaHoraria, out var zonaHorariaOffset))
        {
            return zonaHorariaOffset;
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(zonaHoraria.Trim());
        }
        catch (TimeZoneNotFoundException)
        {
            if (TimeZoneAliases.TryGetValue(zonaHoraria.Trim(), out var alias))
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(alias);
                }
                catch (TimeZoneNotFoundException)
                {
                }
            }
        }
        catch (InvalidTimeZoneException)
        {
        }

        return TimeZoneInfo.Utc;
    }

    private static bool TryCreateUtcOffsetTimeZone(string zonaHoraria, out TimeZoneInfo timeZone)
    {
        var normalized = zonaHoraria.Trim().ToUpperInvariant();
        if (normalized == "UTC")
        {
            timeZone = TimeZoneInfo.Utc;
            return true;
        }

        if (!normalized.StartsWith("UTC", StringComparison.Ordinal) || normalized.Length <= 3)
        {
            timeZone = TimeZoneInfo.Utc;
            return false;
        }

        var offsetText = normalized[3..];
        if (offsetText.Length == 3)
        {
            offsetText += ":00";
        }

        if (offsetText.Length != 6
            || (offsetText[0] != '+' && offsetText[0] != '-')
            || offsetText[3] != ':'
            || !int.TryParse(offsetText[1..3], out var hours)
            || !int.TryParse(offsetText[4..6], out var minutes))
        {
            timeZone = TimeZoneInfo.Utc;
            return false;
        }

        var totalMinutes = (hours * 60) + minutes;
        var offset = offsetText[0] == '-'
            ? TimeSpan.FromMinutes(-totalMinutes)
            : TimeSpan.FromMinutes(totalMinutes);

        try
        {
            var id = $"UTC{(offset >= TimeSpan.Zero ? "+" : "-")}{Math.Abs(offset.Hours):00}:{Math.Abs(offset.Minutes):00}";
            timeZone = TimeZoneInfo.CreateCustomTimeZone(id, offset, id, id);
            return true;
        }
        catch
        {
            timeZone = TimeZoneInfo.Utc;
            return false;
        }
    }

    private sealed record GrupoProceso(Guid EmpleadoId, DateOnly Fecha);
}
