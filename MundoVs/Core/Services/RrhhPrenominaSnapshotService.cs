using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Services;

public sealed class RrhhPrenominaSnapshotService : IRrhhPrenominaSnapshotService
{
    private readonly INominaLegalPolicyService _nominaLegalPolicy;
    private readonly IRrhhTiempoExtraResolutionService _tiempoExtraResolutionService;

    public RrhhPrenominaSnapshotService(INominaLegalPolicyService nominaLegalPolicy, IRrhhTiempoExtraResolutionService tiempoExtraResolutionService)
    {
        _nominaLegalPolicy = nominaLegalPolicy;
        _tiempoExtraResolutionService = tiempoExtraResolutionService;
    }

    public async Task<IReadOnlyList<RrhhPrenominaSnapshotItem>> ConstruirSnapshotPeriodoAsync(CrmDbContext db, DateTime inicio, DateTime fin, NominaConfiguracion configuracion, CancellationToken cancellationToken = default)
    {
        var diasPeriodo = Math.Max(1, (fin.Date - inicio.Date).Days + 1);
        var empleadosPeriodo = await db.Empleados
            .AsNoTracking()
            .Include(e => e.Posicion)
                .ThenInclude(p => p!.BonoEstructuraRrhh)
                    .ThenInclude(e => e!.Detalles)
                        .ThenInclude(d => d.BonoRubroRrhh)
            .Where(e => e.IsActive)
            .OrderBy(e => e.Nombre)
            .ThenBy(e => e.ApellidoPaterno)
            .ToListAsync(cancellationToken);

        var ids = empleadosPeriodo.Select(e => e.Id).ToList();
        var esquemasPorEmpleado = await ObtenerEsquemasPagoPeriodoAsync(db, inicio, fin, ids, cancellationToken);
        var montoDestajo = await db.ValesDestajo
            .Include(v => v.Detalles)
            .Where(v => v.IsActive && v.Estatus == EstatusValeDestajo.Aprobado && ids.Contains(v.EmpleadoId) && v.Fecha >= inicio && v.Fecha <= fin)
            .GroupBy(v => v.EmpleadoId)
            .ToDictionaryAsync(g => g.Key, g => g.Sum(v => v.Detalles.Sum(d => d.Importe)), cancellationToken);

        var vacacionesHistoricas = await db.RrhhAusencias
            .AsNoTracking()
            .Where(a => ids.Contains(a.EmpleadoId)
                && a.IsActive
                && a.Tipo == TipoAusenciaRrhh.Vacaciones
                && (a.Estatus == EstatusAusenciaRrhh.Aprobada || a.Estatus == EstatusAusenciaRrhh.Aplicada)
                && a.FechaFin < DateOnly.FromDateTime(inicio))
            .ToListAsync(cancellationToken);

        var vacacionesHistoricasPorEmpleado = vacacionesHistoricas
            .GroupBy(a => a.EmpleadoId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var ausenciasPeriodo = await db.RrhhAusencias
            .AsNoTracking()
            .Where(a => ids.Contains(a.EmpleadoId)
                && a.IsActive
                && (a.Estatus == EstatusAusenciaRrhh.Aprobada || a.Estatus == EstatusAusenciaRrhh.Aplicada)
                && a.FechaInicio <= DateOnly.FromDateTime(fin)
                && a.FechaFin >= DateOnly.FromDateTime(inicio))
            .ToListAsync(cancellationToken);

        var ausenciasPorEmpleado = ausenciasPeriodo
            .GroupBy(a => a.EmpleadoId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var resumenAsistencias = await ObtenerResumenAsistenciasPeriodoAsync(db, inicio, fin, ids, configuracion, cancellationToken);
        var saldosBancoActuales = new Dictionary<Guid, decimal>();
        var snapshots = new List<RrhhPrenominaSnapshotItem>(empleadosPeriodo.Count);

        foreach (var empleado in empleadosPeriodo)
        {
            var ausenciasEmpleado = ausenciasPorEmpleado.GetValueOrDefault(empleado.Id) ?? [];
            var vacacionesPeriodo = ausenciasEmpleado.Where(a => a.Tipo == TipoAusenciaRrhh.Vacaciones).Sum(a => a.Dias);
            var permisosConGoce = ausenciasEmpleado.Where(a => a.Tipo == TipoAusenciaRrhh.Permiso && a.ConGocePago).Sum(a => a.Dias);
            var permisosSinGoce = ausenciasEmpleado.Where(a => a.Tipo == TipoAusenciaRrhh.Permiso && !a.ConGocePago).Sum(a => a.Dias);
            var notaAusencias = ConstruirNotaAusencias(ausenciasEmpleado);
            var resumen = resumenAsistencias.GetValueOrDefault(empleado.Id) ?? new ResumenAsistenciaPrenominaSnapshot();
            var diasTrabajados = resumen.TieneAsistencias ? resumen.DiasTrabajados : Math.Max(0, diasPeriodo - vacacionesPeriodo - permisosConGoce - permisosSinGoce);
            var faltasInjustificadas = resumen.TieneAsistencias ? resumen.DiasFaltaInjustificada : 0;
            var diasPagados = Math.Max(0, diasPeriodo - permisosSinGoce - faltasInjustificadas);
            var cicloVacacional = _nominaLegalPolicy.ObtenerCicloVacacional(empleado, inicio, configuracion);
            var vacacionesUsadasCiclo = (vacacionesHistoricasPorEmpleado.GetValueOrDefault(empleado.Id) ?? [])
                .Sum(a => CalcularDiasAusenciaEnRango(a, DateOnly.FromDateTime(cicloVacacional.InicioCiclo), DateOnly.FromDateTime(cicloVacacional.FinCiclo)));
            var diasVacacionesDisponibles = _nominaLegalPolicy.CalcularDiasVacacionesDisponibles(empleado, inicio, vacacionesUsadasCiclo, configuracion);
            var montoDestajoEmpleado = montoDestajo.GetValueOrDefault(empleado.Id);
            var asignacion = esquemasPorEmpleado.GetValueOrDefault(empleado.Id);
            if (!saldosBancoActuales.ContainsKey(empleado.Id))
            {
                saldosBancoActuales[empleado.Id] = Math.Round((await _tiempoExtraResolutionService.ObtenerSaldoBancoHorasAsync(db, empleado.EmpresaId, empleado.Id, cancellationToken)) / 60m, 2);
            }

            snapshots.Add(new RrhhPrenominaSnapshotItem
            {
                Empleado = empleado,
                AsignacionEsquema = asignacion,
                DiasTrabajados = diasTrabajados,
                DiasPagados = diasPagados,
                DiasVacaciones = vacacionesPeriodo,
                DiasFaltaJustificada = permisosSinGoce,
                DiasFaltaInjustificada = faltasInjustificadas,
                DiasIncapacidad = 0,
                DiasDescansoTrabajado = resumen.DiasDescansoTrabajado,
                DiasConMarcacion = resumen.DiasConMarcacion,
                DiasDomingoTrabajado = resumen.DiasDomingoTrabajado,
                DiasFestivoTrabajado = resumen.DiasFestivoTrabajado,
                HorasTrabajadasNetas = resumen.HorasTrabajadasNetas,
                HorasExtraBase = resumen.HorasExtraBase,
                HorasExtra = resumen.HorasExtra,
                HorasBancoAcumuladas = resumen.HorasBancoAcumuladas,
                HorasBancoConsumidas = resumen.HorasBancoConsumidas,
                HorasBancoSaldoActual = saldosBancoActuales.GetValueOrDefault(empleado.Id),
                HorasDescansoTomado = resumen.HorasDescansoTomado,
                HorasDescansoPagado = resumen.HorasDescansoPagado,
                HorasDescansoNoPagado = resumen.HorasDescansoNoPagado,
                MinutosRetardo = resumen.MinutosRetardo,
                MinutosSalidaAnticipada = resumen.MinutosSalidaAnticipada,
                MinutosFaltanteDescontable = resumen.MinutosFaltanteDescontable,
                MinutosDescuentoManual = 0,
                FactorPagoTiempoExtra = configuracion.FactorHoraExtra,
                MontoDestajoInformativo = montoDestajoEmpleado,
                DiasVacacionesDisponibles = diasVacacionesDisponibles,
                DiasVacacionesRestantes = Math.Max(0, diasVacacionesDisponibles - vacacionesPeriodo),
                ComplementoSalarioMinimoSugerido = _nominaLegalPolicy.CalcularComplementoSalarioMinimo(empleado, asignacion, diasPagados, montoDestajoEmpleado, configuracion),
                AplicaImss = empleado.AplicaImss,
                Notas = CombinarNotas(notaAusencias, resumen.NotasRevision)
            });
        }

        return snapshots;
    }

    private static int CalcularDiasAusenciaEnRango(RrhhAusencia ausencia, DateOnly inicio, DateOnly fin)
    {
        var inicioReal = ausencia.FechaInicio > inicio ? ausencia.FechaInicio : inicio;
        var finReal = ausencia.FechaFin < fin ? ausencia.FechaFin : fin;
        if (finReal < inicioReal)
            return 0;

        return (finReal.DayNumber - inicioReal.DayNumber) + 1;
    }

    public async Task<Dictionary<Guid, EmpleadoEsquemaPago>> ObtenerEsquemasPagoPeriodoAsync(CrmDbContext db, DateTime inicio, DateTime fin, IReadOnlyCollection<Guid> empleadoIds, CancellationToken cancellationToken = default)
    {
        if (empleadoIds.Count == 0)
        {
            return [];
        }

        return await db.EmpleadosEsquemaPago
            .AsNoTracking()
            .Include(a => a.EsquemaPago)
            .Where(a => empleadoIds.Contains(a.EmpleadoId)
                && a.VigenteDesde <= fin
                && (a.VigenteHasta == null || a.VigenteHasta >= inicio))
            .GroupBy(a => a.EmpleadoId)
            .ToDictionaryAsync(g => g.Key, g => g.OrderByDescending(a => a.VigenteDesde).First(), cancellationToken);
    }

    private static async Task<Dictionary<Guid, ResumenAsistenciaPrenominaSnapshot>> ObtenerResumenAsistenciasPeriodoAsync(CrmDbContext db, DateTime inicio, DateTime fin, IReadOnlyCollection<Guid> empleadoIds, NominaConfiguracion configuracion, CancellationToken cancellationToken)
    {
        if (empleadoIds.Count == 0)
        {
            return [];
        }

        var festivosPeriodoSet = await db.FestivosRrhh
            .AsNoTracking()
            .Where(f => f.IsActive && f.Fecha >= inicio && f.Fecha <= fin)
            .Select(f => DateOnly.FromDateTime(f.Fecha.Date))
            .ToHashSetAsync(cancellationToken);

        var asistenciasPeriodo = await db.RrhhAsistencias
            .AsNoTracking()
            .Where(a => empleadoIds.Contains(a.EmpleadoId)
                && a.Fecha >= DateOnly.FromDateTime(inicio)
                && a.Fecha <= DateOnly.FromDateTime(fin))
            .ToListAsync(cancellationToken);

        var permisosPeriodo = await db.RrhhAusencias
            .AsNoTracking()
            .Where(a => empleadoIds.Contains(a.EmpleadoId)
                && a.IsActive
                && a.Tipo == TipoAusenciaRrhh.Permiso
                && (a.Estatus == EstatusAusenciaRrhh.Aprobada || a.Estatus == EstatusAusenciaRrhh.Aplicada)
                && a.FechaInicio <= DateOnly.FromDateTime(fin)
                && a.FechaFin >= DateOnly.FromDateTime(inicio))
            .ToListAsync(cancellationToken);

        var permisosPorDia = ConstruirPermisosPorDia(permisosPeriodo, DateOnly.FromDateTime(inicio), DateOnly.FromDateTime(fin));

        return asistenciasPeriodo
            .GroupBy(a => a.EmpleadoId)
            .ToDictionary(g => g.Key, g => ConstruirResumenAsistencia(g.ToList(), festivosPeriodoSet, configuracion, permisosPorDia));
    }

    private static ResumenAsistenciaPrenominaSnapshot ConstruirResumenAsistencia(IReadOnlyCollection<RrhhAsistencia> asistencias, IReadOnlySet<DateOnly> festivosPeriodo, NominaConfiguracion configuracion, IReadOnlyDictionary<string, int> permisosPorDia)
    {
        if (asistencias.Count == 0)
        {
            return new ResumenAsistenciaPrenominaSnapshot();
        }

        var trabajadas = asistencias.Where(a => a.Estatus is RrhhAsistenciaEstatus.AsistenciaNormal or RrhhAsistenciaEstatus.Retardo or RrhhAsistenciaEstatus.Incompleta or RrhhAsistenciaEstatus.DescansoTrabajado).ToList();
        var faltas = asistencias.Where(a => a.Estatus == RrhhAsistenciaEstatus.Falta).ToList();
        var notas = asistencias
            .Where(a => a.RequiereRevision || a.Estatus is RrhhAsistenciaEstatus.Incompleta or RrhhAsistenciaEstatus.TurnoNoAsignado or RrhhAsistenciaEstatus.MarcaNoReconocida)
            .OrderBy(a => a.Fecha)
            .Select(a => $"{a.Fecha:dd/MM}: {a.Estatus}{(string.IsNullOrWhiteSpace(a.Observaciones) ? string.Empty : $" - {a.Observaciones}")}")
            .ToList();
        var minutosExtraBase = asistencias.Sum(a => a.MinutosExtra);
        var minutosExtraPago = asistencias.Sum(a => a.MinutosExtraAutorizadosPago);
        var minutosBancoAcumulados = asistencias.Sum(a => a.MinutosExtraAutorizadosBanco);
        var minutosBancoConsumidos = asistencias.Sum(a => a.MinutosCubiertosBancoHoras);

        // Si el banco de horas está deshabilitado, los minutos marcados para banco se reconvierten a pago
        // y los consumos quedan en cero (no hay banco del cual descontar).
        if (!configuracion.BancoHorasHabilitado)
        {
            minutosExtraPago += minutosBancoAcumulados;
            minutosBancoAcumulados = 0;
            minutosBancoConsumidos = 0;
        }
        else
        {
            // Aplicar factor de acumulación al banco (p.ej. 1.5 para premiar acumulación).
            if (configuracion.BancoHorasFactorAcumulacion > 0m && configuracion.BancoHorasFactorAcumulacion != 1m)
            {
                minutosBancoAcumulados = (int)Math.Round(minutosBancoAcumulados * configuracion.BancoHorasFactorAcumulacion);
            }

            // Aplicar tope de banco: el excedente se paga como horas extra en lugar de acumularse.
            var topeMinutos = (int)Math.Round(configuracion.BancoHorasTopeHoras * 60m);
            if (topeMinutos > 0 && minutosBancoAcumulados > topeMinutos)
            {
                var excedente = minutosBancoAcumulados - topeMinutos;
                minutosBancoAcumulados = topeMinutos;
                minutosExtraPago += excedente;
            }
        }

        var minutosDescansoTomado = asistencias.Sum(a => a.MinutosDescansoTomado);
        var minutosDescansoPagado = asistencias.Sum(a => a.MinutosDescansoPagado);
        var minutosDescansoNoPagado = asistencias.Sum(a => a.MinutosDescansoNoPagado);
        var minutosRetardoOriginales = asistencias.Sum(a => Math.Max(0, a.MinutosRetardo));
        var minutosSalidaAnticipadaOriginales = asistencias.Sum(a => Math.Max(0, a.MinutosSalidaAnticipada));
        var minutosPerdonadosManual = asistencias.Sum(a => Math.Max(0, a.MinutosPerdonadosManual));
        var minutosRetardo = asistencias.Sum(a => RrhhTiempoExtraPolicy.ObtenerMinutosRetardoEfectivos(a, ObtenerMinutosPermisoAplicados(permisosPorDia, a)));
        var minutosSalidaAnticipada = asistencias.Sum(a => RrhhTiempoExtraPolicy.ObtenerMinutosSalidaAnticipadaEfectivos(a, ObtenerMinutosPermisoAplicados(permisosPorDia, a)));
        var minutosFaltanteDescontable = asistencias.Sum(a => RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteDescontable(a, ObtenerMinutosPermisoAplicados(permisosPorDia, a), 0));
        var diasCubiertosBanco = asistencias.Count(a => a.MinutosCubiertosBancoHoras > 0);
        if (diasCubiertosBanco > 0)
        {
            notas.Add($"Banco de horas cubrió faltantes en {diasCubiertosBanco} día(s) del período.");
        }

        var asistenciasSinResolucion = asistencias.Count(a => a.MinutosExtra > 0 && string.IsNullOrWhiteSpace(a.ResolucionTiempoExtra));
        if (asistenciasSinResolucion > 0)
        {
            notas.Add($"Hay {asistenciasSinResolucion} día(s) con tiempo extra pendiente de resolución operativa.");
        }

        if (!configuracion.BancoHorasHabilitado && asistencias.Any(a => a.MinutosExtraAutorizadosBanco > 0))
        {
            notas.Add("Banco de horas deshabilitado en configuración: el tiempo extra autorizado para banco se pagará.");
        }

        return new ResumenAsistenciaPrenominaSnapshot
        {
            TieneAsistencias = true,
            DiasTrabajados = trabajadas.Select(a => a.Fecha).Distinct().Count(),
            DiasFaltaInjustificada = faltas.Select(a => a.Fecha).Distinct().Count(),
            DiasDescansoTrabajado = asistencias.Count(a => a.Estatus == RrhhAsistenciaEstatus.DescansoTrabajado),
            DiasConMarcacion = asistencias.Count(a => a.TotalMarcaciones > 0),
            DiasDomingoTrabajado = trabajadas.Count(a => a.Fecha.ToDateTime(TimeOnly.MinValue).DayOfWeek == DayOfWeek.Sunday),
            DiasFestivoTrabajado = trabajadas.Count(a => festivosPeriodo.Contains(a.Fecha)),
            HorasTrabajadasNetas = Math.Round(asistencias.Sum(a => RrhhTiempoExtraPolicy.ObtenerMinutosTrabajadosNetosEfectivos(a)) / 60m, 2),
            HorasExtraBase = Math.Round(minutosExtraBase / 60m, 2),
            HorasExtra = Math.Round(minutosExtraPago / 60m, 2),
            HorasBancoAcumuladas = Math.Round(minutosBancoAcumulados / 60m, 2),
            HorasBancoConsumidas = Math.Round(minutosBancoConsumidos / 60m, 2),
            HorasDescansoTomado = Math.Round(minutosDescansoTomado / 60m, 2),
            HorasDescansoPagado = Math.Round(minutosDescansoPagado / 60m, 2),
            HorasDescansoNoPagado = Math.Round(minutosDescansoNoPagado / 60m, 2),
            MinutosRetardo = minutosRetardo,
            MinutosSalidaAnticipada = minutosSalidaAnticipada,
            MinutosRetardoOriginales = minutosRetardoOriginales,
            MinutosSalidaAnticipadaOriginales = minutosSalidaAnticipadaOriginales,
            MinutosPerdonadosManual = minutosPerdonadosManual,
            MinutosFaltanteDescontable = minutosFaltanteDescontable,
            NotasRevision = notas.Count == 0 ? null : string.Join(" | ", notas)
        };
    }

    private static Dictionary<string, int> ConstruirPermisosPorDia(IReadOnlyCollection<RrhhAusencia> permisos, DateOnly desde, DateOnly hasta)
    {
        var resultado = new Dictionary<string, int>();
        foreach (var permiso in permisos)
        {
            var inicio = permiso.FechaInicio < desde ? desde : permiso.FechaInicio;
            var fin = permiso.FechaFin > hasta ? hasta : permiso.FechaFin;
            var minutosPorDia = (int)Math.Round(Math.Max(0m, permiso.Horas) * 60m, MidpointRounding.AwayFromZero);
            for (var fecha = inicio; fecha <= fin; fecha = fecha.AddDays(1))
            {
                resultado[CrearClavePermiso(permiso.EmpleadoId, fecha)] = minutosPorDia;
            }
        }

        return resultado;
    }

    private static int ObtenerMinutosPermisoAplicados(IReadOnlyDictionary<string, int> permisosPorDia, RrhhAsistencia asistencia)
        => permisosPorDia.TryGetValue(CrearClavePermiso(asistencia.EmpleadoId, asistencia.Fecha), out var minutos)
            ? Math.Max(0, minutos)
            : Math.Max(0, asistencia.MinutosCubiertosBancoHoras);

    private static string CrearClavePermiso(Guid empleadoId, DateOnly fecha)
        => $"{empleadoId:N}:{fecha:yyyyMMdd}";

    private static string? ConstruirNotaAusencias(IEnumerable<RrhhAusencia> ausencias)
    {
        var resumen = ausencias
            .OrderBy(a => a.FechaInicio)
            .Select(a => a.Tipo == TipoAusenciaRrhh.Vacaciones
                ? $"Vacaciones {a.FechaInicio:dd/MM}-{a.FechaFin:dd/MM} ({a.Dias} d)"
                : $"Permiso {(a.ConGocePago ? "con goce" : "sin goce")} {a.FechaInicio:dd/MM}-{a.FechaFin:dd/MM} ({a.Dias} d{(a.Horas > 0 ? $", {a.Horas:0.##} h" : string.Empty)})")
            .ToList();

        return resumen.Count == 0 ? null : string.Join(" | ", resumen);
    }

    private static string? CombinarNotas(params string?[] notas)
    {
        var valores = notas.Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n!.Trim()).ToList();
        return valores.Count == 0 ? null : string.Join(" | ", valores);
    }

    private sealed class ResumenAsistenciaPrenominaSnapshot
    {
        public bool TieneAsistencias { get; set; }
        public int DiasTrabajados { get; set; }
        public int DiasFaltaInjustificada { get; set; }
        public int DiasDescansoTrabajado { get; set; }
        public int DiasConMarcacion { get; set; }
        public int DiasDomingoTrabajado { get; set; }
        public int DiasFestivoTrabajado { get; set; }
        public decimal HorasTrabajadasNetas { get; set; }
        public decimal HorasExtraBase { get; set; }
        public decimal HorasExtra { get; set; }
        public decimal HorasBancoAcumuladas { get; set; }
        public decimal HorasBancoConsumidas { get; set; }
        public decimal HorasDescansoTomado { get; set; }
        public decimal HorasDescansoPagado { get; set; }
        public decimal HorasDescansoNoPagado { get; set; }
        public int MinutosRetardo { get; set; }
        public int MinutosSalidaAnticipada { get; set; }
        public int MinutosRetardoOriginales { get; set; }
        public int MinutosSalidaAnticipadaOriginales { get; set; }
        public int MinutosPerdonadosManual { get; set; }
        public int MinutosFaltanteDescontable { get; set; }
        public string? NotasRevision { get; set; }
    }
}
