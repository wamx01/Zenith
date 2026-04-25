using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Core.Models;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Services;

public sealed class RrhhAsistenciaProcessor : IRrhhAsistenciaProcessor
{
    private const string DefaultMexicoTimeZone = "Central Standard Time (Mexico)";
    private const int MinutosToleranciaEntradaAnticipadaRevision = 30;
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

        foreach (var grupo in grupos)
        {
            await ReprocesarGrupoAsync(db, empresaId, grupo, configuracionNomina, cancellationToken);
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
        var analisisJornada = AnalizarJornada(detalleTurno, marcacionesClasificadas, configuracionDescansos, permisoParcial);

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

        var (estatus, requiereRevision, observaciones) = ClasificarAsistencia(detalleTurno, analisisJornada, minutosEntradaAnticipada, minutosRetardo, minutosSalidaAnticipada, minutosExtra);
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
        int minutosExtra)
    {
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
        var requiereRevisionEntradaAnticipada = minutosEntradaAnticipada > MinutosToleranciaEntradaAnticipadaRevision;
        var requiereRevisionJornadaIrregular = requiereRevisionEntradaAnticipada && minutosExtra > 0;

        if (minutosRetardo > 0)
        {
            return (RrhhAsistenciaEstatus.Retardo, minutosSalidaAnticipada > 0 || requiereRevisionDescansos || requiereRevisionEntradaAnticipada || requiereRevisionJornadaIrregular, ConstruirObservaciones(minutosEntradaAnticipada, minutosRetardo, minutosSalidaAnticipada, minutosExtra, "La asistencia presenta retardo.", observacionesDescansos));
        }

        if (minutosSalidaAnticipada > 0)
        {
            return (RrhhAsistenciaEstatus.AsistenciaNormal, true, ConstruirObservaciones(minutosEntradaAnticipada, minutosRetardo, minutosSalidaAnticipada, minutosExtra, "La salida real fue antes de la programada.", observacionesDescansos));
        }

        return (RrhhAsistenciaEstatus.AsistenciaNormal, requiereRevisionDescansos || requiereRevisionEntradaAnticipada || requiereRevisionJornadaIrregular, ConstruirObservaciones(minutosEntradaAnticipada, minutosRetardo, minutosSalidaAnticipada, minutosExtra, null, observacionesDescansos));
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

        if (minutosRetardo > 0 && minutosSalidaPosterior > 0)
        {
            minutosSalidaPosterior = Math.Max(0, minutosSalidaPosterior - minutosRetardo);
            if (minutosSalidaPosterior < minutosMinimosTiempoExtra)
            {
                minutosSalidaPosterior = 0;
            }
        }

        return minutosEntradaAnticipada + minutosSalidaPosterior;
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

        return partes.Count == 0 ? null : string.Join(" ", partes);
    }

    private static int ObtenerMinutosMinimosTiempoExtra(int minutosMinimosTiempoExtra)
        => minutosMinimosTiempoExtra > 0 ? minutosMinimosTiempoExtra : 30;

    private static int ObtenerMinutosRetardoAplicables(int minutosRetardo, RrhhAsistenciaDescansoSettings configuracionDescansos)
        => minutosRetardo <= Math.Max(0, configuracionDescansos.ToleranciaRetardoMinutos)
            ? 0
            : minutosRetardo;

    private static string? ConstruirObservaciones(int minutosEntradaAnticipada, int minutosRetardo, int minutosSalidaAnticipada, int minutosExtra, string? baseObservacion, IEnumerable<string>? observacionesAdicionales = null)
    {
        var partes = new List<string>();
        if (!string.IsNullOrWhiteSpace(baseObservacion))
        {
            partes.Add(baseObservacion);
        }

        if (minutosEntradaAnticipada > 0)
        {
            partes.Add($"Entrada anticipada de {minutosEntradaAnticipada} min.");
            if (minutosEntradaAnticipada > MinutosToleranciaEntradaAnticipadaRevision)
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

    private static AnalisisJornada AnalizarJornada(TurnoBaseDetalle? detalleTurno, IReadOnlyList<MarcacionProcesada> marcaciones, RrhhAsistenciaDescansoSettings configuracionDescansos, PermisoParcialDia? permisoParcial)
    {
        var descansosConfigurados = ObtenerDescansosConfigurados(detalleTurno);
        var minutosJornadaProgramada = detalleTurno?.HoraEntrada is TimeSpan entradaProgramada && detalleTurno.HoraSalida is TimeSpan salidaProgramada
            ? Math.Max(0, (int)Math.Round((salidaProgramada - entradaProgramada).TotalMinutes))
            : 0;
        var minutosDescansoProgramado = descansosConfigurados.Sum(d => d.Minutos);
        var minutosJornadaNetaProgramada = Math.Max(0, minutosJornadaProgramada - descansosConfigurados.Where(d => !d.EsPagado).Sum(d => d.Minutos));

        var entradaMarcacion = ObtenerMarcacionPorClasificacion(marcaciones, TipoClasificacionMarcacionRrhh.Entrada)
            ?? marcaciones.FirstOrDefault();
        var salidaMarcacion = ObtenerMarcacionPorClasificacion(marcaciones, TipoClasificacionMarcacionRrhh.Salida)
            ?? marcaciones.LastOrDefault();
        var entradaReal = entradaMarcacion?.FechaLocal.TimeOfDay;
        var salidaReal = salidaMarcacion != null && salidaMarcacion != entradaMarcacion
            ? salidaMarcacion.FechaLocal.TimeOfDay
            : (TimeSpan?)null;

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
                entradaReal,
                salidaReal,
                descansosConfigurados.Count > 0 ? "Descansos programados sin información suficiente." : null,
                descansosConfigurados.Count > 0 ? ["No hay suficientes marcaciones para evaluar descansos configurados."] : [],
                descansosConfigurados.Count > 0,
                false,
                false);
        }

        var fechaEntrada = entradaMarcacion?.FechaLocal;
        var fechaSalida = salidaMarcacion?.FechaLocal;
        var minutosTrabajadosBrutos = fechaEntrada.HasValue && fechaSalida.HasValue && fechaSalida > fechaEntrada
            ? Math.Max(0, (int)Math.Round((fechaSalida.Value - fechaEntrada.Value).TotalMinutes))
            : 0;
        var marcasIntermedias = marcaciones
            .Where(m => m != entradaMarcacion && m != salidaMarcacion)
            .ToList();
        var descansosTomados = new List<DescansoTomado>();
        var observaciones = new List<string>();

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

        if (descansosTomados.Count > descansosConfigurados.Count)
        {
            observaciones.Add($"Se detectaron {descansosTomados.Count} descanso(s) y el turno solo contempla {descansosConfigurados.Count}.");
        }

        var minutosSalidaAnticipada = detalleTurno?.HoraSalida is TimeSpan salidaProgramadaAplicada && salidaReal.HasValue
            ? Math.Max(0, (int)Math.Round((salidaProgramadaAplicada - salidaReal.Value).TotalMinutes))
            : 0;
        var descansosAplicados = CalcularDescansosAplicados(descansosConfigurados, descansosTomados, minutosSalidaAnticipada, permisoParcial, configuracionDescansos, observaciones);
        var minutosDescansoTomado = descansosAplicados.Sum(d => d.MinutosAplicados);
        var minutosDescansoPagado = descansosAplicados.Where(d => d.EsPagado).Sum(d => d.MinutosAplicados);
        var minutosDescansoNoPagado = Math.Max(0, minutosDescansoTomado - minutosDescansoPagado);
        var minutosTrabajadosNetos = Math.Max(0, minutosTrabajadosBrutos - minutosDescansoNoPagado);
        var resumenDescansos = ConstruirResumenDescansos(descansosConfigurados, descansosAplicados);

        var autoDescuentoDescansoNoMarcado = descansosAplicados.Any(d => !d.FueMarcado && d.MinutosAplicados > 0);
        var bloquearTiempoExtraAutomatico = false;
        var requiereRevisionDescansos = descansosAplicados.Any(d => d.RequiereConfirmacion)
            || descansosTomados.Count > descansosConfigurados.Count
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
            entradaReal,
            salidaReal,
            resumenDescansos,
            observaciones,
            requiereRevisionDescansos,
            autoDescuentoDescansoNoMarcado,
            bloquearTiempoExtraAutomatico);
    }

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

        var descansos = new List<DescansoConfigurado>();
        if (detalleTurno.CantidadDescansos >= 1 && detalleTurno.Descanso1Inicio.HasValue && detalleTurno.Descanso1Fin.HasValue)
        {
            descansos.Add(new DescansoConfigurado(1, detalleTurno.Descanso1Inicio.Value, detalleTurno.Descanso1Fin.Value, detalleTurno.Descanso1EsPagado));
        }

        if (detalleTurno.CantidadDescansos >= 2 && detalleTurno.Descanso2Inicio.HasValue && detalleTurno.Descanso2Fin.HasValue)
        {
            descansos.Add(new DescansoConfigurado(2, detalleTurno.Descanso2Inicio.Value, detalleTurno.Descanso2Fin.Value, detalleTurno.Descanso2EsPagado));
        }

        return descansos;
    }

    private static List<DescansoAplicado> CalcularDescansosAplicados(
        IReadOnlyList<DescansoConfigurado> descansosConfigurados,
        IReadOnlyList<DescansoTomado> descansosTomados,
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
        var tomadosPorNumero = descansosTomados
            .Where(d => d.Numero <= descansosConfigurados.Count)
            .ToDictionary(d => d.Numero);
        var faltantesCubiertosPorSalida = new HashSet<int>();
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

        foreach (var adicional in descansosTomados.Where(d => d.Numero > descansosConfigurados.Count))
        {
            observaciones.Add($"Se detectó un descanso adicional no programado de {adicional.Minutos} min.");
            aplicados.Add(new DescansoAplicado(
                adicional.Numero,
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
        }

        return aplicados;
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
        TimeSpan? EntradaReal,
        TimeSpan? SalidaReal,
        string? ResumenDescansos,
        IReadOnlyList<string> ObservacionesRevision,
        bool RequiereRevisionDescansos,
        bool AutoDescuentoDescansoNoMarcado,
        bool BloquearTiempoExtraAutomatico)
    {
        public int MinutosDescansoNoPagado => Math.Max(0, MinutosDescansoTomado - MinutosDescansoPagado);
        public bool RequiereRevision => RequiereRevisionDescansos;
    }

    private sealed record PoliticaDescansoNoMarcado(
        int MinutosDescansoTomado,
        int MinutosDescansoPagado,
        int MinutosTrabajadosNetos,
        string ResumenDescansos,
        IReadOnlyList<string> Observaciones,
        bool RequiereRevision,
        bool AutoDescuentoAplicado,
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
