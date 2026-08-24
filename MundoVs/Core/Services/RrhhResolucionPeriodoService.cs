using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Services;

/// <summary>
/// Resolución de tiempo extra por periodo de nómina. La DETECCIÓN sigue siendo
/// diaria (RrhhAsistencia.MinutosExtra); la LIQUIDACIÓN se autoriza por periodo.
///
/// Fase 1: SIN netting. Faltante, retardo y extra se reportan independientes; el
/// operador solo reparte el extra detectado entre pago y banco. Los neteos
/// (extra absorbe faltante/retardo, restablece banco) llegan en fases posteriores.
/// </summary>
public sealed class RrhhResolucionPeriodoService : IRrhhResolucionPeriodoService
{
    private const string ReferenciaPeriodoExtraBancoPrefix = "Periodo";

    private readonly IRrhhTiempoExtraResolutionService _tiempoExtra;
    private readonly IRrhhPermisoPorDiferenciaService _permisoPorDiferencia;

    public RrhhResolucionPeriodoService(
        IRrhhTiempoExtraResolutionService tiempoExtra,
        IRrhhPermisoPorDiferenciaService permisoPorDiferencia)
    {
        _tiempoExtra = tiempoExtra;
        _permisoPorDiferencia = permisoPorDiferencia;
    }

    public async Task<RrhhResolucionTiempoExtraPeriodo> ObtenerOCrearPeriodoAsync(
        CrmDbContext db, Guid empresaId, Guid empleadoId, DateOnly fechaReferencia, CancellationToken cancellationToken = default)
    {
        var (empleado, corte) = await CargarEmpleadoYCorteAsync(db, empresaId, empleadoId, cancellationToken);
        GarantizarAplicable(empleado);

        var calendario = ResolverPeriodo(empleado, fechaReferencia, corte);
        return await ObtenerOCrearPeriodoPorCalendarioAsync(db, empresaId, empleadoId, calendario, cancellationToken);
    }

    // Resuelve el calendario del periodo coherente con el command: si trae rango explícito
    // (vista contenedor), usa ResolverPeriodoDesdeFechas — igual que el preview — para que
    // apply y vean el MISMO periodo. Si no, cae al legado por FechaReferencia.
    // English: Resolves the period calendario consistent with the command: if it carries an
    // explicit range (container view), uses ResolverPeriodoDesdeFechas — same as the preview —
    // so apply and preview see the SAME period. Otherwise falls back to the legacy by
    // FechaReferencia.
    private static NominaPeriodoCalendario ResolverPeriodoCommand(Empleado empleado, RrhhResolucionPeriodoCommand command, NominaCorteRrhh? corte)
        => command.FechaInicioPeriodo is { } fi && command.FechaFinPeriodo is { } ff
            ? ResolverPeriodoDesdeFechas(empleado, fi, ff, corte)
            : ResolverPeriodo(empleado, command.FechaReferencia, corte);

    private async Task<RrhhResolucionTiempoExtraPeriodo> ObtenerOCrearPeriodoPorCalendarioAsync(
        CrmDbContext db, Guid empresaId, Guid empleadoId, NominaPeriodoCalendario calendario, CancellationToken cancellationToken)
    {
        var existente = await db.RrhhResolucionesTiempoExtraPeriodo
            .FirstOrDefaultAsync(r => r.EmpresaId == empresaId
                && r.EmpleadoId == empleadoId
                && r.PeriodicidadPago == calendario.PeriodicidadPago
                && r.AnioPeriodo == calendario.AnioPeriodo
                && r.NumeroPeriodo == calendario.NumeroPeriodo, cancellationToken);

        if (existente is not null)
        {
            return existente;
        }

        var periodo = new RrhhResolucionTiempoExtraPeriodo
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            EmpleadoId = empleadoId,
            PeriodicidadPago = calendario.PeriodicidadPago,
            AnioPeriodo = calendario.AnioPeriodo,
            NumeroPeriodo = calendario.NumeroPeriodo,
            PeriodoKey = ConstruirPeriodoKey(calendario),
            PeriodoEtiqueta = calendario.Periodo,
            FechaInicio = DateOnly.FromDateTime(calendario.Inicio),
            FechaFin = DateOnly.FromDateTime(calendario.Fin),
            Estatus = RrhhResolucionPeriodoEstatus.Pendiente,
            CreatedAt = DateTime.UtcNow
        };
        db.RrhhResolucionesTiempoExtraPeriodo.Add(periodo);
        return periodo;
    }

    public async Task<RrhhResolucionPeriodoResumen> ObtenerResumenPeriodoAsync(
        CrmDbContext db, Guid empresaId, Guid empleadoId, DateOnly fechaReferencia, CancellationToken cancellationToken = default)
    {
        var (empleado, corte) = await CargarEmpleadoYCorteAsync(db, empresaId, empleadoId, cancellationToken);
        var calendario = ResolverPeriodo(empleado, fechaReferencia, corte);
        var fechaInicio = DateOnly.FromDateTime(calendario.Inicio);
        var fechaFin = DateOnly.FromDateTime(calendario.Fin);

        return await ObtenerResumenPeriodoPorRangoAsync(db, empresaId, empleadoId, empleado.TipoNomina != TipoNomina.Destajo, fechaInicio, fechaFin, calendario, cancellationToken);
    }

    public async Task<RrhhResolucionPeriodoResumen> ObtenerResumenPeriodoAsync(
        CrmDbContext db, Guid empresaId, Guid empleadoId, DateOnly fechaInicio, DateOnly fechaFin, CancellationToken cancellationToken = default)
    {
        var (empleado, corte) = await CargarEmpleadoYCorteAsync(db, empresaId, empleadoId, cancellationToken);
        var calendario = ResolverPeriodoDesdeFechas(empleado, fechaInicio, fechaFin, corte);

        return await ObtenerResumenPeriodoPorRangoAsync(db, empresaId, empleadoId, empleado.TipoNomina != TipoNomina.Destajo, fechaInicio, fechaFin, calendario, cancellationToken);
    }

    // Totales de detección del periodo. Para un Fija-sin-turno (EsMetaSemanal) se calculan
    // contra la meta semanal (HorasBase del periodo, default 48h): extra = trabajado sobre la
    // meta, deficit = meta menos trabajado menos tiempo cubierto (con goce + compensacion).
    // Para el resto, se usan las sumas per-día habituales. Retorno compartido por el resumen
    // (preview) y por la aplicación de la resolución para que ambas coincidan.
    // English: Period detection totals. For a Fija-with-no-shift (EsMetaSemanal) they are
    // computed against the weekly meta (period HorasBase, default 48h): extra = worked over the
    // meta, deficit = meta minus worked minus covered time (paid leave + compensation). For the
    // rest, the usual per-day sums are used. Shared by the resumen (preview) and the resolution
    // application so both agree.
    private sealed record DeteccionPeriodoTotales(
        bool EsMetaSemanal,
        bool EsPorHoras,
        int MinutosMetaSemanal,
        int MinutosTrabajadosMetaSemanal,
        int ExtraDetectado,
        int FaltanteBruto,
        int FaltanteNeto,
        int Retardo,
        int SalidaAnticipada,
        int NetoDetectado,
        // Extra crudo de días Fija-con-turno cuyo excedente NO superó el umbral (p.ej. 2 y 15
        // min con umbral 30). NO se reporta como extra detectado (no es pagadero por sí mismo),
        // PERO entra al POOL del neteo para tapar faltante/retardo/salida/banco de otros días.
        // Tras el neteo, el sobrante pagable se topa al extra detectado (sobre umbral) → el
        // bajo-umbral nunca se paga, sólo ayuda a cubrir deducciones. 0 para meta-semanal/PorHoras.
        // English: Raw extra from Fija-with-shift days whose surplus did NOT clear the threshold
        // (e.g. 2 and 15 min with a 30-min threshold). NOT reported as detected extra (not payable
        // on its own), BUT it enters the neteo POOL to cover other days' shortage/late/early/bank.
        // After neteo the payable surplus is capped at the detected (above-threshold) extra → the
        // below-threshold is never paid, it only helps cover deductions. 0 for weekly-meta/PorHoras.
        int ExtraBajoUmbralNoPagadero);

    private static DeteccionPeriodoTotales CalcularDeteccionPeriodo(
        NominaConfiguracion configuracion,
        PeriodicidadPago periodicidad,
        IReadOnlyList<RrhhAsistencia> asistencias,
        IReadOnlyDictionary<DateOnly, int> permisosPorDia)
    {
        // Meta semanal (Fija sin turno): overlay a nivel de periodo contra la meta de HorasBase.
        // El deficit descuenta sueldo (FaltanteDescontable); el extra es autorizable. No hay
        // retardo (sin turno no hay entrada/salida programada). El conGoce cubre el deficit.
        if (RrhhTiempoExtraPolicy.EsPeriodoMetaSemanal(asistencias))
        {
            var metaMinutos = RrhhTiempoExtraPolicy.ObtenerMetaSemanalMinutos(configuracion.ObtenerHorasBase(periodicidad));
            var trabajadoActual = asistencias.Sum(RrhhTiempoExtraPolicy.ObtenerMinutosNetoEfectivo);
            var conGoce = asistencias.Sum(a => permisosPorDia.GetValueOrDefault(a.Fecha))
                        + asistencias.Sum(a => Math.Max(0, a.MinutosCompensacionPermisoAprobados));
            // Umbral normalizado (default 15 si la config es 0): mismo perdón que el cálculo
            // por día, para que el extra del periodo respete el umbral igual que "Ver detalle".
            // English: Normalized threshold (default 15 if config is 0): same forgiveness as
            // the per-day calc so the period extra honors the threshold just like "Ver detalle".
            var umbralNormalizado = RrhhTiempoExtraPolicy.NormalizarMinutosMinimosTiempoExtra(configuracion.MinutosMinimosTiempoExtra);
            var (extraSem, deficitSem) = RrhhTiempoExtraPolicy.CalcularBalanceMetaSemanal(trabajadoActual, conGoce, metaMinutos, umbralNormalizado);
            return new DeteccionPeriodoTotales(
                EsMetaSemanal: true,
                EsPorHoras: false,
                MinutosMetaSemanal: metaMinutos,
                MinutosTrabajadosMetaSemanal: trabajadoActual,
                ExtraDetectado: extraSem,
                FaltanteBruto: deficitSem,
                FaltanteNeto: deficitSem,
                Retardo: 0,
                SalidaAnticipada: 0,
                NetoDetectado: trabajadoActual,
                ExtraBajoUmbralNoPagadero: 0);
        }

        var extraDetectado = asistencias.Sum(a => Math.Max(0, a.MinutosExtra));
        // Extra crudo bajo umbral (Fija-con-turno): excedente per-día (max(0, neto − jornada))
        // que NO superó el umbral (MinutosMinimosTiempoExtra normalizado). a.MinutosExtra ya trae
        // 0 para esos días (el processor lo zeroa bajo umbral), así que NO entran en
        // extraDetectado. Aquí los sumamos aparte para alimentar el POOL del neteo: cubren
        // faltante/retardo/salida/banco de otros días, pero el sobrante pagable se topa al
        // extraDetectado (sobre umbral) → el bajo-umbral nunca se paga, sólo ayuda a netear.
        // Se excluyen PorHoras y días sin jornada (jornada 0) → excedente 0 o sin sentido.
        // English: Below-threshold raw extra (Fija-with-shift): per-day surplus (max(0, net −
        // scheduled)) that did NOT clear the threshold (normalized MinutosMinimosTiempoExtra).
        // a.MinutosExtra is already 0 for those days (the processor zeroes it under threshold),
        // so they're NOT in extraDetectado. Here we sum them separately to feed the neteo POOL:
        // they cover other days' shortage/late/early/bank, but the payable surplus is capped at
        // extraDetectado (above threshold) → below-threshold is never paid, only helps net.
        // PorHoras and no-jornada days (jornada 0) are excluded → surplus 0 or meaningless.
        var umbralConTurno = RrhhTiempoExtraPolicy.NormalizarMinutosMinimosTiempoExtra(configuracion.MinutosMinimosTiempoExtra);
        var extraBajoUmbral = asistencias
            .Where(a => !a.EsPorHoras && a.MinutosJornadaNetaProgramada > 0)
            .Sum(a =>
            {
                var excedente = Math.Max(0, RrhhTiempoExtraPolicy.ObtenerMinutosNetoEfectivo(a) - a.MinutosJornadaNetaProgramada);
                return (excedente > 0 && excedente < umbralConTurno) ? excedente : 0;
            });
        var faltanteBruto = asistencias.Sum(RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteNeto);
        var faltanteNeto = asistencias.Sum(a =>
            RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteDescontable(a, permisosPorDia.GetValueOrDefault(a.Fecha), Math.Max(0, a.MinutosCompensacionPermisoAprobados)));
        var retardo = asistencias.Sum(a => RrhhTiempoExtraPolicy.ObtenerMinutosRetardoEfectivos(a, permisosPorDia.GetValueOrDefault(a.Fecha)));
        // Salida anticipada del periodo (Fase 3b): el sobrante de extra tras faltante y retardo
        // la tapa antes de restaurar banco / ser pagable. Para PorHoras (sin turno) es 0.
        // English: Period early-leave (Phase 3b): extra surplus after shortage and late covers
        // it before restoring bank / becoming payable. For PorHoras (no shift) it's 0.
        var salidaAnticipada = asistencias.Sum(RrhhTiempoExtraPolicy.ObtenerMinutosSalidaAnticipadaEfectivos);
        var neto = asistencias.Sum(RrhhTiempoExtraPolicy.ObtenerMinutosNetoEfectivo);
        var esPorHoras = asistencias.Count > 0 && asistencias.All(a => a.EsPorHoras);
        return new DeteccionPeriodoTotales(false, esPorHoras, 0, 0, extraDetectado, faltanteBruto, faltanteNeto, retardo, salidaAnticipada, neto, extraBajoUmbral);
    }

    private async Task<RrhhResolucionPeriodoResumen> ObtenerResumenPeriodoPorRangoAsync(
        CrmDbContext db, Guid empresaId, Guid empleadoId, bool esAplicable, DateOnly fechaInicio, DateOnly fechaFin,
        NominaPeriodoCalendario calendario, CancellationToken cancellationToken = default)
    {
        var contexto = await _tiempoExtra.ObtenerContextoEmpleadoAsync(db, empresaId, empleadoId, cancellationToken);

        var periodo = await db.RrhhResolucionesTiempoExtraPeriodo
            .FirstOrDefaultAsync(r => r.EmpresaId == empresaId
                && r.EmpleadoId == empleadoId
                && r.PeriodicidadPago == calendario.PeriodicidadPago
                && r.AnioPeriodo == calendario.AnioPeriodo
                && r.NumeroPeriodo == calendario.NumeroPeriodo, cancellationToken);

        var asistencias = await db.RrhhAsistencias
            .AsNoTracking()
            .Where(a => a.EmpresaId == empresaId && a.EmpleadoId == empleadoId
                && a.Fecha >= fechaInicio && a.Fecha <= fechaFin)
            .OrderBy(a => a.Fecha)
            .ToListAsync(cancellationToken);

        var permisosPorDia = await ConstruirPermisosConGocePorDiaAsync(db, empresaId, empleadoId, fechaInicio, fechaFin, cancellationToken);

        var nominaConfig = await NominaConfiguracionLoader.LoadAsync(db, empresaId);
        var bancoConsumidoPeriodo = contexto.Configuracion.BancoHorasHabilitado
            ? await ObtenerMinutosBancoConsumidoPeriodoAsync(db, empresaId, empleadoId, fechaInicio, fechaFin, cancellationToken)
            : 0;

        return ConstruirResumenDesdeDatos(
            nominaConfig, contexto.Configuracion, calendario.PeriodicidadPago,
            asistencias, permisosPorDia, bancoConsumidoPeriodo, contexto.SaldoBancoHorasMinutos,
            periodo, calendario, fechaInicio, fechaFin, esAplicable);
    }

    /// <summary>
    /// Resúmenes de resolución por periodo para VARIOS empleados en una sola pasada (sin N+1).
    /// Carga en batch (~6 queries totales) la configuración, los empleados, las asistencias,
    /// los permisos con goce, los movimientos de banco (saldo + consumido) y las resoluciones
    /// del periodo; luego computea el neteo por empleado con el MISMO <see cref="ConstruirResumenDesdeDatos"/>
    /// que usa el resumen individual → el listado semanal muestra exactamente las mismas columnas
    /// neteadas que el drawer del detalle, sin drift.
    /// English: Period resolution summaries for SEVERAL employees in a single pass (no N+1).
    /// Batch-loads (~6 total queries) config, employees, attendances, paid leaves, bank movements
    /// (balance + consumed) and the period's resolutions; then computes per-employee netting with
    /// the SAME ConstruirResumenDesdeDatos used by the individual summary → the weekly listing shows
    /// exactly the same netted columns as the detail drawer, with no drift.
    /// </summary>
    public async Task<IReadOnlyDictionary<Guid, RrhhResolucionPeriodoResumen>> ObtenerResumenesPeriodoBatchAsync(
        CrmDbContext db, Guid empresaId, IReadOnlyCollection<Guid> empleadoIds,
        DateOnly fechaInicio, DateOnly fechaFin, PeriodicidadPago periodicidad,
        NominaPeriodoCalendario calendario, CancellationToken cancellationToken = default)
    {
        var resultado = new Dictionary<Guid, RrhhResolucionPeriodoResumen>();
        if (empleadoIds.Count == 0)
        {
            return resultado;
        }

        var empIds = empleadoIds.ToList();

        // Configuración (empresa) + snapshot de tiempo extra: una sola carga para todos.
        // English: Config (empresa) + overtime snapshot: a single load for everyone.
        var snapshot = await _tiempoExtra.ObtenerConfiguracionAsync(db, empresaId, cancellationToken);
        var nominaConfig = await NominaConfiguracionLoader.LoadAsync(db, empresaId);

        // TipoNomina por empleado → esAplicable (no Destajo). Una sola query.
        // English: TipoNomina per employee → esAplicable (not Destajo). One query.
        var tipoNominaPorEmp = await db.Empleados
            .AsNoTracking()
            .Where(e => e.EmpresaId == empresaId && empIds.Contains(e.Id))
            .Select(e => new { e.Id, e.TipoNomina })
            .ToDictionaryAsync(e => e.Id, e => e.TipoNomina, cancellationToken);

        // Asistencias del rango para todos los empleados, agrupadas por empleado.
        // English: Range attendances for all employees, grouped by employee.
        var asistencias = await db.RrhhAsistencias
            .AsNoTracking()
            .Where(a => a.EmpresaId == empresaId && empIds.Contains(a.EmpleadoId)
                && a.Fecha >= fechaInicio && a.Fecha <= fechaFin)
            .OrderBy(a => a.Fecha)
            .ToListAsync(cancellationToken);
        var asistenciasPorEmp = asistencias
            .GroupBy(a => a.EmpleadoId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<RrhhAsistencia>)g.ToList());

        // Permisos con goce del rango para todos los empleados, expandidos por día.
        // English: Range paid leaves for all employees, expanded per day.
        var permisos = await db.RrhhAusencias
            .AsNoTracking()
            .Where(a => a.EmpresaId == empresaId
                && empIds.Contains(a.EmpleadoId)
                && a.IsActive
                && a.ConGocePago
                && a.Horas > 0
                && (a.Estatus == EstatusAusenciaRrhh.Aprobada || a.Estatus == EstatusAusenciaRrhh.Aplicada)
                && a.FechaInicio <= fechaFin
                && a.FechaFin >= fechaInicio)
            .ToListAsync(cancellationToken);
        var permisosPorEmpDia = new Dictionary<Guid, Dictionary<DateOnly, int>>();
        foreach (var permiso in permisos)
        {
            var porEmpleado = permisosPorEmpDia.GetValueOrDefault(permiso.EmpleadoId);
            if (porEmpleado is null)
            {
                porEmpleado = new Dictionary<DateOnly, int>();
                permisosPorEmpDia[permiso.EmpleadoId] = porEmpleado;
            }
            var minutosPorDia = RrhhTiempoExtraPolicy.ObtenerMinutosPermisoConGocePorDia(permiso);
            var inicio = permiso.FechaInicio < fechaInicio ? fechaInicio : permiso.FechaInicio;
            var fin = permiso.FechaFin > fechaFin ? fechaFin : permiso.FechaFin;
            for (var fecha = inicio; fecha <= fin; fecha = fecha.AddDays(1))
            {
                porEmpleado[fecha] = porEmpleado.GetValueOrDefault(fecha) + minutosPorDia;
            }
        }

        // Movimientos de banco de todos los empleados: saldo (Σ Horas × 60, igual que
        // ObtenerSaldoBancoHorasAsync) + consumido del periodo (Consumo en rango, excluye
        // cobertura-banco, igual que ObtenerMinutosBancoConsumidoPeriodoAsync). Una sola query.
        // English: Bank movements for all employees: balance (Σ Horas × 60, same as
        // ObtenerSaldoBancoHorasAsync) + period consumed (in-range Consumo, excludes
        // cobertura-banco, same as ObtenerMinutosBancoConsumidoPeriodoAsync). One query.
        var bancoMovs = await db.RrhhBancoHorasMovimientos
            .AsNoTracking()
            .Where(m => m.EmpresaId == empresaId && empIds.Contains(m.EmpleadoId) && m.IsActive)
            .Select(m => new
            {
                m.EmpleadoId,
                m.TipoMovimiento,
                m.Horas,
                m.ReferenciaTipo,
                m.Fecha
            })
            .ToListAsync(cancellationToken);
        var saldoPorEmp = bancoMovs
            .GroupBy(m => m.EmpleadoId)
            .ToDictionary(g => g.Key, g => (int)Math.Round(g.Sum(x => x.Horas) * 60m, MidpointRounding.AwayFromZero));
        var consumidoPorEmp = bancoMovs
            .Where(m => m.TipoMovimiento == TipoMovimientoBancoHorasRrhh.Consumo
                && m.Fecha >= fechaInicio && m.Fecha <= fechaFin
                && !EsCoberturaBanco(m.ReferenciaTipo))
            .GroupBy(m => m.EmpleadoId)
            .ToDictionary(g => g.Key, g => (int)Math.Round(g.Sum(x => -x.Horas) * 60m, MidpointRounding.AwayFromZero));

        // Resoluciones del periodo para todos los empleados (comparten calendario).
        // English: Period resolutions for all employees (shared calendario).
        var resoluciones = await db.RrhhResolucionesTiempoExtraPeriodo
            .AsNoTracking()
            .Include(r => r.Lineas)
            .Where(r => r.EmpresaId == empresaId
                && empIds.Contains(r.EmpleadoId)
                && r.PeriodicidadPago == calendario.PeriodicidadPago
                && r.AnioPeriodo == calendario.AnioPeriodo
                && r.NumeroPeriodo == calendario.NumeroPeriodo)
            .ToListAsync(cancellationToken);
        var resolucionPorEmp = resoluciones.ToDictionary(r => r.EmpleadoId);

        foreach (var empId in empIds)
        {
            var asisEmp = asistenciasPorEmp.GetValueOrDefault(empId) ?? Array.Empty<RrhhAsistencia>();
            var permisosEmp = (IReadOnlyDictionary<DateOnly, int>)permisosPorEmpDia.GetValueOrDefault(empId) ?? new Dictionary<DateOnly, int>();
            var bancoConsumido = snapshot.BancoHorasHabilitado ? consumidoPorEmp.GetValueOrDefault(empId) : 0;
            var saldo = saldoPorEmp.GetValueOrDefault(empId);
            var periodo = resolucionPorEmp.GetValueOrDefault(empId);
            var esAplicable = tipoNominaPorEmp.TryGetValue(empId, out var tipoNomina) && tipoNomina != TipoNomina.Destajo;

            resultado[empId] = ConstruirResumenDesdeDatos(
                nominaConfig, snapshot, periodicidad, asisEmp, permisosEmp,
                bancoConsumido, saldo, periodo, calendario, fechaInicio, fechaFin, esAplicable);
        }

        return resultado;
    }

    // Construye el resumen del periodo a partir de datos ya cargados (puro, sin DB). Lo usan
    // tanto el resumen individual (ObtenerResumenPeriodoPorRangoAsync) como el batch del listado
    // (ObtenerResumenesPeriodoBatchAsync) → ambas vías producen EXACTAMENTE el mismo resumen
    // neteado, así el listado semanal y el drawer del detalle nunca divergen.
    // English: Builds the period summary from already-loaded data (pure, no DB). Used by both the
    // individual summary (ObtenerResumenPeriodoPorRangoAsync) and the listing batch
    // (ObtenerResumenesPeriodoBatchAsync) → both paths produce EXACTLY the same netted summary,
    // so the weekly listing and the detail drawer never diverge.
    private static RrhhResolucionPeriodoResumen ConstruirResumenDesdeDatos(
        NominaConfiguracion nominaConfig,
        RrhhTiempoExtraConfiguracionSnapshot snapshot,
        PeriodicidadPago periodicidad,
        IReadOnlyList<RrhhAsistencia> asistencias,
        IReadOnlyDictionary<DateOnly, int> permisosPorDia,
        int bancoConsumidoPeriodo,
        int saldoBancoHorasMinutos,
        RrhhResolucionTiempoExtraPeriodo? periodo,
        NominaPeriodoCalendario calendario,
        DateOnly fechaInicio,
        DateOnly fechaFin,
        bool esAplicable)
    {
        var dias = asistencias
            .Select(a =>
            {
                var permisoDia = permisosPorDia.GetValueOrDefault(a.Fecha);
                var faltanteBruto = RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteNeto(a);
                var faltanteNeto = RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteDescontable(a, permisoDia, Math.Max(0, a.MinutosCompensacionPermisoAprobados));
                return new RrhhResolucionPeriodoDia
                {
                    Fecha = a.Fecha,
                    MinutosExtra = Math.Max(0, a.MinutosExtra),
                    MinutosFaltante = faltanteBruto,
                    MinutosFaltanteNeto = faltanteNeto,
                    MinutosPermisoConGoce = permisoDia,
                    MinutosRetardo = RrhhTiempoExtraPolicy.ObtenerMinutosRetardoEfectivos(a, permisoDia),
                    // Salida anticipada detectada del día (bruto; el neteo es a nivel periodo).
                    // English: Detected early-leave for the day (raw; netting is at period level).
                    MinutosSalidaAnticipada = RrhhTiempoExtraPolicy.ObtenerMinutosSalidaAnticipadaEfectivos(a),
                    MinutosTrabajadosNetos = RrhhTiempoExtraPolicy.ObtenerMinutosNetoEfectivo(a),
                    // Jornada programada y base pagada por día = misma fórmula que el listado
                    // AsistenciasSemanal ("Normal"). Base-calc sobre asistencias, no marcaciones bruto.
                    // English: Scheduled jornada and paid base per day = same formula as the
                    // AsistenciasSemanal listing ("Normal"). Base-calc on asistencias, not raw marks.
                    MinutosJornadaProgramada = Math.Max(0, a.MinutosJornadaNetaProgramada),
                    MinutosBasePagada = RrhhTiempoExtraPolicy.ObtenerMinutosBasePagada(a)
                };
            })
            .ToList();

        var deteccion = CalcularDeteccionPeriodo(nominaConfig, periodicidad, asistencias, permisosPorDia);
        var extraDetectado = deteccion.ExtraDetectado;
        var extraBajoUmbral = deteccion.ExtraBajoUmbralNoPagadero;
        var faltanteDetectado = deteccion.FaltanteBruto;
        var faltanteNetoPeriodo = deteccion.FaltanteNeto;
        var retardoPeriodo = deteccion.Retardo;
        var salidaAnticipadaPeriodo = deteccion.SalidaAnticipada;

        // Cadena de neteo NetoVsNeto (Fase 2 + 3 + 3b + 4) delegada al helper único
        // (RrhhTiempoExtraPolicy.CalcularNeteoNetoVsNeto): faltante → retardo → salida → banco.
        // El extra de un día tapa en orden las deducciones de otros días; el sobrante repone el
        // banco consumido; el sobrante final —topado al extraDetectado— es pagable (el
        // bajo-umbral nunca se paga, sólo cubre). El MISMO helper lo usan la autorización
        // (AplicarResolucionPeriodoAsync) y —vía el batch— el snapshot de nómina → cero drift
        // con Asistencia Semanal.
        // English: NetoVsNeto net chain (Phase 2 + 3 + 3b + 4) delegated to the single helper
        // (RrhhTiempoExtraPolicy.CalcularNeteoNetoVsNeto): shortage → late → early-leave → bank.
        // One day's extra covers other days' deductions in order; the surplus replenishes
        // consumed bank; the final surplus —capped at extraDetectado— is payable (below-threshold
        // is never paid, only covers). The SAME helper is used by authorization and —via the batch—
        // by the payroll snapshot → zero drift with Asistencia Semanal.
        var (faltanteAbsorbido, retardoAbsorbido, salidaAbsorbido, bancoRestaurado, extraAbsorbible) =
            RrhhTiempoExtraPolicy.CalcularNeteoNetoVsNeto(
                extraDetectado, extraBajoUmbral, faltanteNetoPeriodo, retardoPeriodo, salidaAnticipadaPeriodo, bancoConsumidoPeriodo);

        return new RrhhResolucionPeriodoResumen
        {
            EsAplicable = esAplicable,
            Periodo = periodo,
            PeriodicidadPago = calendario.PeriodicidadPago,
            AnioPeriodo = calendario.AnioPeriodo,
            NumeroPeriodo = calendario.NumeroPeriodo,
            PeriodoKey = ConstruirPeriodoKey(calendario),
            PeriodoEtiqueta = calendario.Periodo,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            MinutosExtraDetectado = extraDetectado,
            // Extra crudo bajo umbral (no pagadero) que entró al pool del neteo. El display lo
            // muestra para que la aritmética del neteo cuadre: detectado + bajo = pool tapado.
            // English: Below-threshold raw extra (non-payable) that entered the neteo pool. The
            // display shows it so the neteo arithmetic reconciles: detected + below = tapped pool.
            MinutosExtraBajoUmbralNoPagadero = extraBajoUmbral,
            MinutosFaltanteDetectado = faltanteDetectado,
            MinutosFaltanteNetoPeriodo = faltanteNetoPeriodo,
            MinutosPermisoConGocePeriodo = dias.Sum(d => d.MinutosPermisoConGoce),
            MinutosRetardoDetectado = retardoPeriodo,
            MinutosSalidaAnticipadaDetectado = salidaAnticipadaPeriodo,
            MinutosTrabajadosNetosDetectado = deteccion.NetoDetectado,
            MinutosExtraAbsorbible = extraAbsorbible,
            MinutosFaltanteAbsorbidoExtra = faltanteAbsorbido,
            MinutosRetardoAbsorbidoExtra = retardoAbsorbido,
            MinutosSalidaAnticipadaAbsorbidoExtra = salidaAbsorbido,
            MinutosBancoConsumidoPeriodo = bancoConsumidoPeriodo,
            MinutosBancoRestauradoExtra = bancoRestaurado,
            MinutosExtraDobles = periodo?.MinutosExtraDobles ?? 0,
            MinutosExtraTriples = periodo?.MinutosExtraTriples ?? 0,
            SaldoBancoHorasMinutos = saldoBancoHorasMinutos,
            TopeBancoMinutos = snapshot.TopeBancoMinutos,
            FactorTiempoExtra = snapshot.FactorTiempoExtra,
            BancoHorasHabilitado = snapshot.BancoHorasHabilitado,
            FactorAcumulacionBancoHoras = snapshot.FactorAcumulacionBancoHoras,
            EsMetaSemanal = deteccion.EsMetaSemanal,
            EsPorHoras = deteccion.EsPorHoras,
            MinutosMetaSemanal = deteccion.MinutosMetaSemanal,
            MinutosTrabajadosMetaSemanal = deteccion.MinutosTrabajadosMetaSemanal,
            // Jornada programada del periodo: para Fija-con-turno es la suma per-día de la jornada
            // neta programada (la meta del turno); para Fija-sin-turno (EsMetaSemanal) las jornadas
            // per-día son 0, por eso se usa la meta semanal. Sirve de base salarial bruta (Hrs Pagadas).
            // English: Scheduled jornada for the period: for Fija-with-shift it's the per-day sum
            // of the scheduled net jornada (the shift meta); for Fija-with-no-shift (EsMetaSemanal)
            // per-day jornadas are 0, so the weekly meta is used. Serves as gross salary base.
            MinutosJornadaProgramadaPeriodo = deteccion.EsMetaSemanal
                ? deteccion.MinutosMetaSemanal
                : asistencias.Sum(a => Math.Max(0, a.MinutosJornadaNetaProgramada)),
            // Base pagada = MISMA fórmula que el listado AsistenciasSemanal ("Normal"):
            // Σ ObtenerMinutosBasePagada por día. El listado es la base canónica del operador,
            // así el detalle (modal + drawer) muestra exactamente lo mismo que el listado.
            // English: Paid base = SAME formula as the AsistenciasSemanal listing ("Normal"):
            // Σ ObtenerMinutosBasePagada per day. The listing is the operator's canonical base,
            // so the detail (modal + drawer) shows exactly the same as the listing.
            MinutosBasePagadaCalculado = asistencias.Sum(a => RrhhTiempoExtraPolicy.ObtenerMinutosBasePagada(a)),
            Dias = dias
        };
    }

    public async Task<RrhhResolucionPeriodoResult> AplicarResolucionPeriodoAsync(
        CrmDbContext db, RrhhResolucionPeriodoCommand command, CancellationToken cancellationToken = default)
    {
        var (empleado, corte) = await CargarEmpleadoYCorteAsync(db, command.EmpresaId, command.EmpleadoId, cancellationToken);
        GarantizarAplicable(empleado);

        // Resuelve el periodo coherente con el command: si trae rango explícito (vista
        // contenedor) usa ese rango — igual que el preview — para que apply y preview vean el
        // MISMO periodo y las mismas asistencias. Sin el rango, cae al legado por FechaReferencia.
        // English: Resolves the period consistent with the command: if it carries an explicit
        // range (container view) it uses that range — same as the preview — so apply and preview
        // see the SAME period and the same asistencias. Without the range, legacy by FechaReferencia.
        var calendario = ResolverPeriodoCommand(empleado, command, corte);
        var fechaInicio = DateOnly.FromDateTime(calendario.Inicio);
        var fechaFin = DateOnly.FromDateTime(calendario.Fin);

        var periodo = await ObtenerOCrearPeriodoPorCalendarioAsync(db, command.EmpresaId, command.EmpleadoId, calendario, cancellationToken);
        var contexto = await _tiempoExtra.ObtenerContextoEmpleadoAsync(db, command.EmpresaId, command.EmpleadoId, cancellationToken);

        // Detección del periodo recalculada en vivo (snapshot autoritativo al autorizar).
        var asistencias = await db.RrhhAsistencias
            .AsNoTracking()
            .Where(a => a.EmpresaId == command.EmpresaId && a.EmpleadoId == command.EmpleadoId
                && a.Fecha >= fechaInicio && a.Fecha <= fechaFin)
            .ToListAsync(cancellationToken);

        var permisosPorDia = await ConstruirPermisosConGocePorDiaAsync(db, command.EmpresaId, command.EmpleadoId, fechaInicio, fechaFin, cancellationToken);

        // Permiso por diferencia (sintético) — se crea ANTES del neteo F2/F3/F4 para que
        // las categorías ConGoce (Banco y ConGoceSinBanco) entren al prorrateo de permisos
        // con goce y reduzcan el faltante del periodo. Categoría SinGoce fluye al descuento
        // manual del periodo (no entra a permisosPorDia, pero se descuenta del salario).
        // Idempotente: AplicarPermisosAsync revierte sintéticas previas del periodo.
        if (command.PermisosPorDiferencia is { Count: > 0 })
        {
            await _permisoPorDiferencia.AplicarPermisosAsync(
                db, command.EmpresaId, command.EmpleadoId,
                fechaReferencia: fechaFin,
                inputs: command.PermisosPorDiferencia,
                usuarioActual: command.UsuarioActual,
                cancellationToken);
            // Re-leer permisosPorDia (ahora incluye las sintéticas recién creadas).
            permisosPorDia = await ConstruirPermisosConGocePorDiaAsync(
                db, command.EmpresaId, command.EmpleadoId, fechaInicio, fechaFin, cancellationToken);
        }

        var configuracion = await NominaConfiguracionLoader.LoadAsync(db, command.EmpresaId);
        var deteccion = CalcularDeteccionPeriodo(configuracion, calendario.PeriodicidadPago, asistencias, permisosPorDia);
        var extraDetectado = deteccion.ExtraDetectado;
        var extraBajoUmbral = deteccion.ExtraBajoUmbralNoPagadero;
        var faltanteDetectado = deteccion.FaltanteBruto;
        var faltanteNetoPeriodo = deteccion.FaltanteNeto;
        var retardoDetectado = deteccion.Retardo;
        var salidaAnticipadaDetectado = deteccion.SalidaAnticipada;
        var netoDetectado = deteccion.NetoDetectado;

        var bancoHorasHabilitado = contexto.Configuracion.BancoHorasHabilitado;
        var bancoConsumidoPeriodo = bancoHorasHabilitado
            ? await ObtenerMinutosBancoConsumidoPeriodoAsync(db, command.EmpresaId, command.EmpleadoId, fechaInicio, fechaFin, cancellationToken)
            : 0;

        // Cadena de neteo NetoVsNeto (Fase 2 + 3 + 3b + 4) delegada al helper único
        // (RrhhTiempoExtraPolicy.CalcularNeteoNetoVsNeto): el extra tapa faltante neto (F2) →
        // retardo (F3) → salida anticipada (F3b); el sobrante REPONE el banco consumido (F4) con
        // un movimiento positivo al banco; el sobrante final —topado al extraDetectado— es
        // pagable (el bajo-umbral nunca se paga, sólo tapa). Para meta semanal extra/déficit son
        // mutuamente excluyentes y extraBajoUmbral=0 → faltanteAbsorbido=0 y el extra → absorbible.
        // El MISMO helper lo usan el resumen display (ConstruirResumenDesdeDatos) y —vía el batch—
        // el snapshot de nómina → cero drift con Asistencia Semanal.
        // English: NetoVsNeto net chain (Phase 2 + 3 + 3b + 4) delegated to the single helper
        // (RrhhTiempoExtraPolicy.CalcularNeteoNetoVsNeto): extra covers net shortage (P2) → late
        // (P3) → early-leave (P3b); the surplus REPLENISHES consumed bank (P4) with a positive bank
        // movement; the final surplus —capped at extraDetectado— is payable (below-threshold never
        // paid, only covers). For weekly meta extra/deficit are mutually exclusive and
        // extraBajoUmbral=0 → faltanteAbsorbed=0 and extra → absorbible. The SAME helper is used by
        // the display summary (ConstruirResumenDesdeDatos) and —via the batch— the payroll snapshot
        // → zero drift with Asistencia Semanal.
        var (faltanteAbsorbido, retardoAbsorbido, salidaAbsorbido, bancoRestaurado, extraAbsorbible) =
            RrhhTiempoExtraPolicy.CalcularNeteoNetoVsNeto(
                extraDetectado, extraBajoUmbral, faltanteNetoPeriodo, retardoDetectado, salidaAnticipadaDetectado, bancoConsumidoPeriodo);

        // F9 — DESCARTAR el extra: el operador acepta la detección (el periodo queda
        // resuelto y desbloquea el gate de prenómina) pero NO autoriza compensación
        // ni pago. La compensación NO es automática: requiere autorización explícita
        // (cualquier otro modo); sin ella, el faltante/retardo/salida del periodo se
        // descuenta COMPLETO. Se anula el neteo (absorbidos=0) → sourcing lee
        // absorbidos=0 y descuenta el faltante/retardo/salida en su totalidad.
        if (command.DescartarExtra)
        {
            if (command.MinutosBasePago > 0 || command.MinutosBaseBanco > 0 || command.Lineas.Count > 0)
            {
                throw new InvalidOperationException(
                    "Descartar el tiempo extra es incompatible con pagar o bancar minutos: no envíes líneas ni bases de pago/banco.");
            }
            faltanteAbsorbido = 0;
            retardoAbsorbido = 0;
            salidaAbsorbido = 0;
            bancoRestaurado = 0;
            extraAbsorbible = 0;
        }

        int pagoBase;
        int bancoBase;
        int pago;            // minutos factorados a pago (bitácora)
        int banco;           // minutos factorados a banco (movimiento del ledger)
        int minutosDobles;
        int minutosTriples;
        int minutosSimples;
        decimal horasExtraFactoradas;
        decimal? factorTiempoExtraAplicado;
        decimal? factorAcumulacionBancoAplicado;

        if (command.Lineas.Count > 0)
        {
            // Fase 8 — autorización por líneas: cada segmento lleva su factor y destino.
            pagoBase = command.Lineas.Where(l => l.Destino == RrhhDestinoTiempoExtraLinea.Pago).Sum(l => Math.Max(0, l.Minutos));
            bancoBase = command.Lineas.Where(l => l.Destino == RrhhDestinoTiempoExtraLinea.Banco).Sum(l => Math.Max(0, l.Minutos));

            if (pagoBase + bancoBase > extraAbsorbible)
            {
                throw new InvalidOperationException(
                    $"La suma de las líneas ({pagoBase} pago + {bancoBase} banco = {pagoBase + bancoBase}) no puede exceder el tiempo extra absorbible del periodo ({extraAbsorbible} min). "
                    + $"Extra {extraDetectado} (sobre umbral) + {extraBajoUmbral} (bajo umbral, no pagadero) = {extraDetectado + extraBajoUmbral} − faltante neto {faltanteNetoPeriodo} − retardo {retardoDetectado} − banco consumido {bancoConsumidoPeriodo}, pagable topado al sobre umbral = {extraAbsorbible} min.");
            }

            if (!bancoHorasHabilitado && bancoBase > 0)
            {
                throw new InvalidOperationException("El banco de horas no está habilitado para esta empresa.");
            }

            // Pago factorado (bitácora) = Σ pago.Minutos × Factor.
            pago = (int)Math.Round(command.Lineas
                .Where(l => l.Destino == RrhhDestinoTiempoExtraLinea.Pago)
                .Sum(l => Math.Max(0, l.Minutos) * Math.Max(0m, l.Factor)), MidpointRounding.AwayFromZero);
            // Banco factorado (movimiento del ledger) = Σ banco.Minutos × Factor (acumulación por línea, simétrica al pago).
            banco = bancoHorasHabilitado
                ? (int)Math.Round(command.Lineas
                    .Where(l => l.Destino == RrhhDestinoTiempoExtraLinea.Banco)
                    .Sum(l => Math.Max(0, l.Minutos) * Math.Max(0m, l.Factor)), MidpointRounding.AwayFromZero)
                : 0;

            // Dobles/triples/simples derivados del factor de cada línea de pago (no del techo legal).
            minutosDobles = command.Lineas.Where(l => l.Destino == RrhhDestinoTiempoExtraLinea.Pago && l.Factor == 2m).Sum(l => Math.Max(0, l.Minutos));
            minutosTriples = command.Lineas.Where(l => l.Destino == RrhhDestinoTiempoExtraLinea.Pago && l.Factor == 3m).Sum(l => Math.Max(0, l.Minutos));
            minutosSimples = command.Lineas.Where(l => l.Destino == RrhhDestinoTiempoExtraLinea.Pago && l.Factor != 2m && l.Factor != 3m).Sum(l => Math.Max(0, l.Minutos));

            horasExtraFactoradas = command.Lineas
                .Where(l => l.Destino == RrhhDestinoTiempoExtraLinea.Pago)
                .Sum(l => Math.Max(0, l.Minutos) / 60m * Math.Max(0m, l.Factor));

            // Con líneas, el factor único ya no aplica: el calculador usa HorasExtraFactoradas.
            factorTiempoExtraAplicado = null;
            factorAcumulacionBancoAplicado = null;
        }
        else
        {
            // Path legado (bucket único + split legal dobles/triples).
            var factorTiempoExtra = command.FactorTiempoExtraOverride is > 0m
                ? command.FactorTiempoExtraOverride!.Value
                : contexto.Configuracion.FactorTiempoExtra;
            var factorAcumulacionBanco = command.FactorTiempoExtraOverride is > 0m
                ? command.FactorTiempoExtraOverride!.Value
                : contexto.Configuracion.FactorAcumulacionBancoHoras;

            pagoBase = Math.Max(0, command.MinutosBasePago);
            bancoBase = Math.Max(0, command.MinutosBaseBanco);

            // El cap es el extra ABSORBIBLE (tras tapar faltante, retardo y restaurar banco).
            if (pagoBase + bancoBase > extraAbsorbible)
            {
                throw new InvalidOperationException(
                    $"La suma base de pago ({pagoBase}) y banco ({bancoBase}) no puede exceder el tiempo extra absorbible del periodo ({extraAbsorbible} min). "
                    + $"Extra {extraDetectado} (sobre umbral) + {extraBajoUmbral} (bajo umbral, no pagadero) = {extraDetectado + extraBajoUmbral} − faltante neto {faltanteNetoPeriodo} − retardo {retardoDetectado} − banco consumido {bancoConsumidoPeriodo}, pagable topado al sobre umbral = {extraAbsorbible} min.");
            }

            if (!bancoHorasHabilitado && bancoBase > 0)
            {
                throw new InvalidOperationException("El banco de horas no está habilitado para esta empresa.");
            }

            pago = (int)Math.Round(pagoBase * Math.Max(1m, factorTiempoExtra), MidpointRounding.AwayFromZero);
            banco = bancoHorasHabilitado
                ? (int)Math.Round(bancoBase * factorAcumulacionBanco, MidpointRounding.AwayFromZero)
                : 0;

            // Fase 5 — split legal del PAGO: los primeros minutos hasta el techo
            // configurable (HorasExtraDoblesPorSemana) se pagan como dobles; el
            // excedente como triples. Solo aplica al PAGO; el banco no se reparte.
            var configuracionNomina = await NominaConfiguracionLoader.LoadAsync(db, command.EmpresaId);
            var minutosDoblesTope = Math.Max(0, configuracionNomina.HorasExtraDoblesPorSemana) * 60;
            minutosDobles = Math.Min(pagoBase, minutosDoblesTope);
            minutosTriples = Math.Max(0, pagoBase - minutosDobles);
            minutosSimples = 0;
            horasExtraFactoradas = 0m;
            factorTiempoExtraAplicado = factorTiempoExtra;
            factorAcumulacionBancoAplicado = factorAcumulacionBanco;
        }

        // Movimientos previos del periodo (extra-banco + restauracion-banco): se
        // reemplazan al re-autorizar (idempotencia del ledger).
        var referenciaExtraBanco = ConstruirReferenciaPeriodo(command.EmpleadoId, periodo.PeriodoKey, "extra-banco");
        var referenciaRestauracionBanco = ConstruirReferenciaPeriodo(command.EmpleadoId, periodo.PeriodoKey, "restauracion-banco");
        var movimientosPrevios = await db.RrhhBancoHorasMovimientos
            .Where(m => m.EmpresaId == command.EmpresaId
                && m.EmpleadoId == command.EmpleadoId
                && m.IsActive
                && (m.ReferenciaTipo == referenciaExtraBanco || m.ReferenciaTipo == referenciaRestauracionBanco))
            .ToListAsync(cancellationToken);

        // minutosBancoPrevios = neto de los movimientos previos (ambos positivos:
        // extra-banco genera banco, restauracion-banco repone consumido).
        var minutosBancoPrevios = (int)Math.Round(movimientosPrevios.Sum(m => m.Horas) * 60m, MidpointRounding.AwayFromZero);
        var saldoBancoDisponible = Math.Max(0, contexto.SaldoBancoHorasMinutos - minutosBancoPrevios);

        // Tope de banco (acumulado). La RESTAURACION está exenta del tope (repone
        // tiempo consumido, no es acumulación nueva — mismo principio que el path
        // diario legado eximía cubiertoBanco). Solo el banco del operador se topa.
        var saldoTrasRestauracion = saldoBancoDisponible + bancoRestaurado;
        var saldoFinalBanco = saldoTrasRestauracion + banco;
        var topeBancoMinutos = contexto.Configuracion.TopeBancoMinutos;
        if (banco > Math.Max(0, topeBancoMinutos - saldoTrasRestauracion))
        {
            var maximoAcumulable = Math.Max(0, topeBancoMinutos - saldoTrasRestauracion);
            throw new InvalidOperationException(
                $"La resolución excede el tope de banco de horas ({topeBancoMinutos} min). Máximo acumulable con esta decisión: {maximoAcumulable} min.");
        }

        if (movimientosPrevios.Count > 0)
        {
            db.RrhhBancoHorasMovimientos.RemoveRange(movimientosPrevios);
        }

        // Fase 8 — idempotencia de líneas: reemplazar las previas del periodo antes
        // de insertar las nuevas (solo cuando se autoriza por líneas).
        var lineasPrevias = await db.RrhhResolucionesTiempoExtraLinea
            .Where(l => l.ResolucionPeriodoId == periodo.Id)
            .ToListAsync(cancellationToken);
        if (lineasPrevias.Count > 0)
        {
            db.RrhhResolucionesTiempoExtraLinea.RemoveRange(lineasPrevias);
        }

        if (command.Lineas.Count > 0)
        {
            var orden = 0;
            foreach (var linea in command.Lineas)
            {
                db.RrhhResolucionesTiempoExtraLinea.Add(new RrhhResolucionTiempoExtraLinea
                {
                    Id = Guid.NewGuid(),
                    EmpresaId = command.EmpresaId,
                    EmpleadoId = command.EmpleadoId,
                    ResolucionPeriodoId = periodo.Id,
                    Orden = orden++,
                    Destino = linea.Destino,
                    Minutos = Math.Max(0, linea.Minutos),
                    Factor = Math.Max(0m, linea.Factor),
                    Observaciones = linea.Observaciones,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = command.UsuarioActual,
                    IsActive = true
                });
            }
        }

        // Snapshot de la detección en la entidad (autoritativo al momento de autorizar).
        periodo.MinutosExtraDetectado = extraDetectado;
        periodo.MinutosFaltanteDetectado = faltanteDetectado;
        periodo.MinutosFaltanteNetoDetectado = faltanteNetoPeriodo;
        periodo.MinutosFaltanteAbsorbidoExtra = faltanteAbsorbido;
        periodo.MinutosRetardoAbsorbidoExtra = retardoAbsorbido;
        periodo.MinutosSalidaAnticipadaAbsorbidoExtra = salidaAbsorbido;
        periodo.MinutosBancoConsumidoDetectado = bancoConsumidoPeriodo;
        periodo.MinutosBancoRestauradoExtra = bancoRestaurado;
        periodo.MinutosRetardoDetectado = retardoDetectado;
        periodo.MinutosSalidaAnticipadaDetectado = salidaAnticipadaDetectado;
        periodo.MinutosTrabajadosNetosDetectado = netoDetectado;
        periodo.MinutosExtraPago = pagoBase;
        periodo.MinutosExtraBanco = bancoBase;
        periodo.MinutosExtraDobles = minutosDobles;
        periodo.MinutosExtraTriples = minutosTriples;
        periodo.MinutosExtraSimples = minutosSimples;
        periodo.HorasExtraFactoradas = horasExtraFactoradas;
        periodo.FactorTiempoExtraAplicado = factorTiempoExtraAplicado;
        periodo.FactorAcumulacionBancoHorasAplicado = factorAcumulacionBancoAplicado;
        periodo.Resolucion = command.Resolucion;
        periodo.Estatus = RrhhResolucionPeriodoEstatus.Autorizada;
        periodo.ExtraDescartado = command.DescartarExtra;
        periodo.AutorizadoPor = command.UsuarioActual;
        periodo.FechaAutorizacion = DateTime.UtcNow;
        periodo.Observaciones = command.Observaciones;
        periodo.UpdatedAt = DateTime.UtcNow;
        periodo.UpdatedBy = command.UsuarioActual;

        if (banco > 0)
        {
            db.RrhhBancoHorasMovimientos.Add(new RrhhBancoHorasMovimiento
            {
                Id = Guid.NewGuid(),
                EmpresaId = command.EmpresaId,
                EmpleadoId = command.EmpleadoId,
                Fecha = fechaFin,
                TipoMovimiento = TipoMovimientoBancoHorasRrhh.GeneradoPorHorasExtra,
                Horas = banco / 60m,
                ReferenciaTipo = referenciaExtraBanco,
                Observaciones = string.IsNullOrWhiteSpace(command.Observaciones)
                    ? $"Generado desde resolución de periodo {periodo.PeriodoEtiqueta}."
                    : command.Observaciones.Trim(),
                EsAutomatico = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = command.UsuarioActual,
                IsActive = true
            });
        }

        // Fase 4 — restauración del banco consumido en el periodo: movimiento
        // POSITIVO que repone el saldo consumido (mismo TipoMovimiento que la
        // generación por extra; se distingue por ReferenciaTipo).
        if (bancoRestaurado > 0)
        {
            db.RrhhBancoHorasMovimientos.Add(new RrhhBancoHorasMovimiento
            {
                Id = Guid.NewGuid(),
                EmpresaId = command.EmpresaId,
                EmpleadoId = command.EmpleadoId,
                Fecha = fechaFin,
                TipoMovimiento = TipoMovimientoBancoHorasRrhh.GeneradoPorHorasExtra,
                Horas = bancoRestaurado / 60m,
                ReferenciaTipo = referenciaRestauracionBanco,
                Observaciones = $"Restauración de banco consumido en el periodo {periodo.PeriodoEtiqueta}.",
                EsAutomatico = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = command.UsuarioActual,
                IsActive = true
            });
        }

        return new RrhhResolucionPeriodoResult
        {
            Periodo = periodo,
            SaldoBancoActualMinutos = saldoFinalBanco,
            TopeBancoMinutos = topeBancoMinutos,
            FactorTiempoExtra = factorTiempoExtraAplicado ?? contexto.Configuracion.FactorTiempoExtra,
            BancoHorasHabilitado = bancoHorasHabilitado,
            FactorAcumulacionBancoHoras = factorAcumulacionBancoAplicado ?? contexto.Configuracion.FactorAcumulacionBancoHoras,
            MinutosBasePagoAplicados = pagoBase,
            MinutosBaseBancoAplicados = bancoBase,
            MinutosPagoAplicados = pago,
            MinutosBancoAplicados = banco,
            BitacoraDetalle = $"empleado={command.EmpleadoId};periodo={periodo.PeriodoKey};resolucion={command.Resolucion};extraDetectado={extraDetectado};faltanteNeto={faltanteNetoPeriodo};retardo={retardoDetectado};bancoConsumido={bancoConsumidoPeriodo};bancoRestaurado={bancoRestaurado};pagoBase={pagoBase};pagoFactorado={pago};bancoBase={bancoBase};bancoFactorado={banco};obs={command.Observaciones}"
        };
    }

    public async Task ReabrirPeriodoAsync(
        CrmDbContext db, Guid empresaId, Guid empleadoId, DateOnly fechaReferencia, string usuarioActual, CancellationToken cancellationToken = default)
        => await ReabrirPeriodoAsync(db, empresaId, empleadoId, fechaInicioPeriodo: null, fechaFinPeriodo: null, fechaReferencia, usuarioActual, cancellationToken);

    // Overload con rango explícito (vista contenedor): reabre el MISMO periodo que el operador
    // ve en pantalla, evitando resolver desde FechaReferencia (que puede caer en otra semana).
    // English: Overload with explicit range (container view): reopens the SAME period the
    // operator sees on screen, avoiding resolving from FechaReferencia (which may fall in a
    // different week).
    public async Task ReabrirPeriodoAsync(
        CrmDbContext db, Guid empresaId, Guid empleadoId,
        DateOnly? fechaInicioPeriodo, DateOnly? fechaFinPeriodo, DateOnly fechaReferencia,
        string usuarioActual, CancellationToken cancellationToken = default)
    {
        var (empleado, corte) = await CargarEmpleadoYCorteAsync(db, empresaId, empleadoId, cancellationToken);
        if (empleado.TipoNomina == TipoNomina.Destajo)
        {
            return;
        }

        var calendario = fechaInicioPeriodo is { } fi && fechaFinPeriodo is { } ff
            ? ResolverPeriodoDesdeFechas(empleado, fi, ff, corte)
            : ResolverPeriodo(empleado, fechaReferencia, corte);
        await ReabrirPeriodoPorCalendarioAsync(db, empresaId, empleadoId, calendario, usuarioActual, cancellationToken);
    }

    private async Task ReabrirPeriodoPorCalendarioAsync(
        CrmDbContext db, Guid empresaId, Guid empleadoId, NominaPeriodoCalendario calendario, string usuarioActual, CancellationToken cancellationToken)
    {
        var periodo = await db.RrhhResolucionesTiempoExtraPeriodo
            .FirstOrDefaultAsync(r => r.EmpresaId == empresaId
                && r.EmpleadoId == empleadoId
                && r.PeriodicidadPago == calendario.PeriodicidadPago
                && r.AnioPeriodo == calendario.AnioPeriodo
                && r.NumeroPeriodo == calendario.NumeroPeriodo, cancellationToken);

        if (periodo is null || periodo.Estatus != RrhhResolucionPeriodoEstatus.Autorizada)
        {
            return;
        }

        var referenciaExtraBanco = ConstruirReferenciaPeriodo(empleadoId, periodo.PeriodoKey, "extra-banco");
        var referenciaRestauracionBanco = ConstruirReferenciaPeriodo(empleadoId, periodo.PeriodoKey, "restauracion-banco");
        var movimientosPrevios = await db.RrhhBancoHorasMovimientos
            .Where(m => m.EmpresaId == empresaId
                && m.EmpleadoId == empleadoId
                && m.IsActive
                && (m.ReferenciaTipo == referenciaExtraBanco || m.ReferenciaTipo == referenciaRestauracionBanco))
            .ToListAsync(cancellationToken);

        if (movimientosPrevios.Count > 0)
        {
            db.RrhhBancoHorasMovimientos.RemoveRange(movimientosPrevios);
        }

        // Fase PermisoPorDiferenciaPeriodo: revertir permisos sintéticos del periodo
        // (silencioso — el reopen queda en Observaciones como auditoría). Las sintéticas
        // se borran junto con sus movimientos de banco asociados.
        await _permisoPorDiferencia.RevertirPermisosAsync(
            db, empresaId, empleadoId,
            fechaReferencia: periodo.FechaFin,
            cancellationToken);

        // Fase 8 — al reabrir, también descartar las líneas de la resolución previa.
        var lineasPrevias = await db.RrhhResolucionesTiempoExtraLinea
            .Where(l => l.ResolucionPeriodoId == periodo.Id)
            .ToListAsync(cancellationToken);
        if (lineasPrevias.Count > 0)
        {
            db.RrhhResolucionesTiempoExtraLinea.RemoveRange(lineasPrevias);
        }

        periodo.Estatus = RrhhResolucionPeriodoEstatus.Reabierta;
        periodo.ExtraDescartado = false;
        periodo.MinutosExtraPago = 0;
        periodo.MinutosExtraBanco = 0;
        periodo.MinutosExtraDobles = 0;
        periodo.MinutosExtraTriples = 0;
        periodo.MinutosExtraSimples = 0;
        periodo.HorasExtraFactoradas = 0m;
        periodo.FactorTiempoExtraAplicado = null;
        periodo.FactorAcumulacionBancoHorasAplicado = null;
        periodo.FechaAutorizacion = null;
        periodo.Observaciones = $"Reabierto por corrección de marcación ({usuarioActual}).";
        periodo.UpdatedAt = DateTime.UtcNow;
        periodo.UpdatedBy = usuarioActual;
    }

    public async Task<RrhhResolucionPeriodoBackfillResult> BackfillDesdeAutorizacionDiariaAsync(
        CrmDbContext db, Guid? empresaId = null, string usuarioActual = "backfill", CancellationToken cancellationToken = default)
    {
        // Solo asistencias con autorización diaria heredada (pago o banco > 0).
        var asistencias = await db.RrhhAsistencias
            .AsNoTracking()
            .Where(a => (empresaId == null || a.EmpresaId == empresaId.Value)
                && (a.MinutosExtraAutorizadosPago > 0 || a.MinutosExtraAutorizadosBanco > 0))
            .ToListAsync(cancellationToken);

        if (asistencias.Count == 0)
        {
            return new RrhhResolucionPeriodoBackfillResult();
        }

        var empleadoIds = asistencias.Select(a => a.EmpleadoId).Distinct().ToList();
        var empresaIds = asistencias.Select(a => a.EmpresaId).Distinct().ToList();

        var empleados = await db.Empleados
            .AsNoTracking()
            .Where(e => empleadoIds.Contains(e.Id))
            .Select(e => new { e.Id, e.EmpresaId, e.PeriodicidadPago, e.TipoNomina })
            .ToListAsync(cancellationToken);
        var empleadosPorId = empleados.ToDictionary(e => e.Id);

        var cortes = await db.NominaCortesRrhh
            .AsNoTracking()
            .Where(c => empresaIds.Contains(c.EmpresaId))
            .ToListAsync(cancellationToken);
        var cortesPorEmpresaPeriodicidad = cortes
            .GroupBy(c => (c.EmpresaId, c.PeriodicidadPago))
            .ToDictionary(g => g.Key, g => g.First());

        // Entidades de periodo ya existentes (para idempotencia: no sobreescribir).
        var existentes = await db.RrhhResolucionesTiempoExtraPeriodo
            .Where(r => (empresaId == null || r.EmpresaId == empresaId.Value)
                && empleadoIds.Contains(r.EmpleadoId))
            .Select(r => new { r.EmpresaId, r.EmpleadoId, r.PeriodicidadPago, r.AnioPeriodo, r.NumeroPeriodo })
            .ToListAsync(cancellationToken);
        var existentesClave = existentes
            .Select(r => (r.EmpresaId, r.EmpleadoId, r.PeriodicidadPago, r.AnioPeriodo, r.NumeroPeriodo))
            .ToHashSet();

        var periodosCreados = 0;
        var periodosOmitidos = 0;
        var empleadosProcesados = new HashSet<Guid>();

        foreach (var grupoEmpleado in asistencias.GroupBy(a => a.EmpleadoId))
        {
            if (!empleadosPorId.TryGetValue(grupoEmpleado.Key, out var empleado))
            {
                continue;
            }
            if (empleado.TipoNomina == TipoNomina.Destajo)
            {
                continue;
            }

            cortesPorEmpresaPeriodicidad.TryGetValue((empleado.EmpresaId, empleado.PeriodicidadPago), out var corte);

            // Agrupa por periodo resuelto (clave: periodicidad + año + número).
            // Variante contenedor: cada asistencia se asigna al periodo que la
            // CONTIENE, no al último corte cerrado (que para semanal Wed–Tue manda
            // Wed-Mon al periodo anterior y sólo Tue al actual → partía la semana
            // y creaba registros cuya FechaInicio/FechaFin no contenían sus días).
            // Así las claves coinciden con el flujo regular y con el viewer.
            foreach (var grupoPeriodo in grupoEmpleado.GroupBy(a =>
                {
                    var cal = NominaPeriodoHelper.ObtenerPeriodoContenedor(empleado.PeriodicidadPago, a.Fecha.ToDateTime(TimeOnly.MinValue), corte);
                    return (cal.PeriodicidadPago, cal.AnioPeriodo, cal.NumeroPeriodo);
                }))
            {
                var (periodicidad, anio, numero) = grupoPeriodo.Key;
                if (existentesClave.Contains((empleado.EmpresaId, empleado.Id, periodicidad, anio, numero)))
                {
                    periodosOmitidos++;
                    continue;
                }

                var calendario = NominaPeriodoHelper.ObtenerPeriodoContenedor(
                    empleado.PeriodicidadPago,
                    grupoPeriodo.First().Fecha.ToDateTime(TimeOnly.MinValue),
                    corte);

                var lista = grupoPeriodo.ToList();
                var permisosPorDiaPeriodo = await ConstruirPermisosConGocePorDiaAsync(
                    db, empleado.EmpresaId, empleado.Id,
                    DateOnly.FromDateTime(calendario.Inicio), DateOnly.FromDateTime(calendario.Fin),
                    cancellationToken);
                var pagoBase = lista.Sum(a => Math.Max(0, a.MinutosExtraAutorizadosPago));
                var bancoBase = lista.Sum(a => Math.Max(0, a.MinutosExtraAutorizadosBanco));
                var extraDetectado = lista.Sum(a => Math.Max(0, a.MinutosExtra));
                var faltanteDetectado = lista.Sum(RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteNeto);
                var retardoDetectado = lista.Sum(a => RrhhTiempoExtraPolicy.ObtenerMinutosRetardoEfectivos(a, permisosPorDiaPeriodo.GetValueOrDefault(a.Fecha)));
                var netoDetectado = lista.Sum(RrhhTiempoExtraPolicy.ObtenerMinutosNetoEfectivo);

                db.RrhhResolucionesTiempoExtraPeriodo.Add(new RrhhResolucionTiempoExtraPeriodo
                {
                    Id = Guid.NewGuid(),
                    EmpresaId = empleado.EmpresaId,
                    EmpleadoId = empleado.Id,
                    PeriodicidadPago = periodicidad,
                    AnioPeriodo = anio,
                    NumeroPeriodo = numero,
                    PeriodoKey = $"{periodicidad}-{anio}-{numero:00}",
                    PeriodoEtiqueta = calendario.Periodo,
                    FechaInicio = DateOnly.FromDateTime(calendario.Inicio),
                    FechaFin = DateOnly.FromDateTime(calendario.Fin),
                    MinutosExtraDetectado = extraDetectado,
                    MinutosFaltanteDetectado = faltanteDetectado,
                    MinutosRetardoDetectado = retardoDetectado,
                    MinutosTrabajadosNetosDetectado = netoDetectado,
                    MinutosExtraPago = pagoBase,
                    MinutosExtraBanco = bancoBase,
                    Estatus = RrhhResolucionPeriodoEstatus.Autorizada,
                    AutorizadoPor = usuarioActual,
                    FechaAutorizacion = DateTime.UtcNow,
                    Observaciones = "Migración one-shot desde autorización diaria histórica.",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = usuarioActual
                });

                periodosCreados++;
                empleadosProcesados.Add(empleado.Id);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return new RrhhResolucionPeriodoBackfillResult
        {
            EmpleadosProcesados = empleadosProcesados.Count,
            PeriodosCreados = periodosCreados,
            PeriodosOmitidos = periodosOmitidos
        };
    }

    public async Task<RrhhResolucionPeriodoBackfillLineasResult> SembrarLineasEnResolucionesAutorizadasAsync(
        CrmDbContext db, Guid? empresaId = null, string usuarioActual = "backfill", CancellationToken cancellationToken = default)
    {
        // Fase 9 — resoluciones Autorizada PRE-Fase 8 (sin líneas). Se reconstruyen las
        // líneas a partir de los escalares persistidos para que el nuevo UI las muestre y
        // para que la resolución pase al path por líneas (factoradas). El monto se preserva:
        // ver SembrarLineasDesdeEscalares para la demostración de equivalencia.
        var periodos = await db.RrhhResolucionesTiempoExtraPeriodo
            .Where(r => (empresaId == null || r.EmpresaId == empresaId.Value)
                && r.Estatus == RrhhResolucionPeriodoEstatus.Autorizada
                && r.IsActive)
            .ToListAsync(cancellationToken);

        if (periodos.Count == 0)
        {
            return new RrhhResolucionPeriodoBackfillLineasResult();
        }

        var periodosIds = periodos.Select(p => p.Id).ToList();
        var periodosConLineas = await db.RrhhResolucionesTiempoExtraLinea
            .Where(l => periodosIds.Contains(l.ResolucionPeriodoId))
            .Select(l => l.ResolucionPeriodoId)
            .Distinct()
            .ToListAsync(cancellationToken);
        var conLineasSet = periodosConLineas.ToHashSet();

        // Cache de configuración de nómina por empresa (factores + tope dobles).
        var empresaIds = periodos.Select(p => p.EmpresaId).Distinct().ToList();
        var configuraciones = new Dictionary<Guid, NominaConfiguracion>();
        foreach (var emp in empresaIds)
        {
            configuraciones[emp] = await NominaConfiguracionLoader.LoadAsync(db, emp);
        }

        var procesados = 0;
        var omitidos = 0;
        var lineasCreadas = 0;

        foreach (var periodo in periodos)
        {
            if (conLineasSet.Contains(periodo.Id))
            {
                omitidos++;
                continue;
            }

            var config = configuraciones[periodo.EmpresaId];
            var lineasSembradas = SembrarLineasDesdeEscalares(periodo, config, usuarioActual);
            if (lineasSembradas.Count == 0)
            {
                omitidos++; // sin pago ni banco que sembrar
                continue;
            }

            foreach (var linea in lineasSembradas)
            {
                db.RrhhResolucionesTiempoExtraLinea.Add(linea);
            }
            lineasCreadas += lineasSembradas.Count;

            // Recalcular escalares derivados desde las líneas sembradas y conmutar al
            // path por líneas (FactorTiempoExtraAplicado=null → el sourcing usa factoradas).
            periodo.MinutosExtraDobles = lineasSembradas
                .Where(l => l.Destino == RrhhDestinoTiempoExtraLinea.Pago && l.Factor == 2m)
                .Sum(l => l.Minutos);
            periodo.MinutosExtraTriples = lineasSembradas
                .Where(l => l.Destino == RrhhDestinoTiempoExtraLinea.Pago && l.Factor == 3m)
                .Sum(l => l.Minutos);
            periodo.MinutosExtraSimples = lineasSembradas
                .Where(l => l.Destino == RrhhDestinoTiempoExtraLinea.Pago && l.Factor != 2m && l.Factor != 3m)
                .Sum(l => l.Minutos);
            periodo.HorasExtraFactoradas = lineasSembradas
                .Where(l => l.Destino == RrhhDestinoTiempoExtraLinea.Pago)
                .Sum(l => l.Minutos / 60m * l.Factor);
            periodo.FactorTiempoExtraAplicado = null;
            periodo.FactorAcumulacionBancoHorasAplicado = null;
            periodo.UpdatedAt = DateTime.UtcNow;
            periodo.UpdatedBy = usuarioActual;

            procesados++;
        }

        await db.SaveChangesAsync(cancellationToken);
        return new RrhhResolucionPeriodoBackfillLineasResult
        {
            PeriodosProcesados = procesados,
            PeriodosOmitidos = omitidos,
            LineasCreadas = lineasCreadas
        };
    }

    /// <summary>
    /// Reconstruye las líneas de una resolución Autorizada pre-Fase 8 a partir de los
    /// escalares persistidos. Reproduce el monto del path legado:
    /// <list type="bullet">
    /// <item><b>Override</b> (FactorTiempoExtraAplicado con valor): el legado aplicaba ese
    /// factor a dobles Y triples → dos líneas @ ese factor. monto = (dobles+triples)/60 × F × sh
    /// = factoradas × sh. ✓</item>
    /// <item><b>Config</b> (factor null, backfill desde daily): el legado caía a
    /// FactorHoraExtra/FactorHoraExtraTriple → dos líneas con esos factores. monto =
    /// dobles/60×F2×sh + triples/60×F3×sh = factoradas × sh. ✓</item>
    /// </list>
    /// El split dobles/triples usa los escalares persistidos si los hay (Fase 5); si no
    /// (backfill desde daily), se deriva por el techo HorasExtraDoblesPorSemana.
    /// El banco usa una línea @ (FactorAcumulacionBancoHorasAplicado ?? config).
    /// </summary>
    private static List<RrhhResolucionTiempoExtraLinea> SembrarLineasDesdeEscalares(
        RrhhResolucionTiempoExtraPeriodo periodo, NominaConfiguracion config, string usuarioActual)
    {
        var lineas = new List<RrhhResolucionTiempoExtraLinea>();
        var orden = 0;

        var pagoBase = Math.Max(0, periodo.MinutosExtraPago);
        var bancoBase = Math.Max(0, periodo.MinutosExtraBanco);

        if (pagoBase > 0)
        {
            // Split dobles/triples: persistido (Fase 5) o derivado por tope (backfill desde daily).
            int doblesMin;
            int triplesMin;
            if (periodo.MinutosExtraDobles > 0 || periodo.MinutosExtraTriples > 0)
            {
                doblesMin = Math.Min(pagoBase, Math.Max(0, periodo.MinutosExtraDobles));
                triplesMin = Math.Max(0, pagoBase - doblesMin);
            }
            else
            {
                var topeMin = Math.Max(0, config.HorasExtraDoblesPorSemana) * 60;
                doblesMin = Math.Min(pagoBase, topeMin);
                triplesMin = Math.Max(0, pagoBase - doblesMin);
            }

            // Factores: override (mismo factor a dobles y triples) o config (F2/F3 distintos).
            decimal factorDobles;
            decimal factorTriples;
            if (periodo.FactorTiempoExtraAplicado.HasValue && periodo.FactorTiempoExtraAplicado.Value > 0m)
            {
                factorDobles = periodo.FactorTiempoExtraAplicado.Value;
                factorTriples = periodo.FactorTiempoExtraAplicado.Value;
            }
            else
            {
                factorDobles = Math.Max(0m, config.FactorHoraExtra);
                factorTriples = Math.Max(0m, config.FactorHoraExtraTriple);
            }

            if (doblesMin > 0)
            {
                lineas.Add(NuevaLineaSembrada(periodo, orden++, RrhhDestinoTiempoExtraLinea.Pago, doblesMin, factorDobles, usuarioActual));
            }
            if (triplesMin > 0)
            {
                lineas.Add(NuevaLineaSembrada(periodo, orden++, RrhhDestinoTiempoExtraLinea.Pago, triplesMin, factorTriples, usuarioActual));
            }
        }

        if (bancoBase > 0)
        {
            var factorBanco = periodo.FactorAcumulacionBancoHorasAplicado.HasValue
                && periodo.FactorAcumulacionBancoHorasAplicado.Value > 0m
                    ? periodo.FactorAcumulacionBancoHorasAplicado.Value
                    : Math.Max(0m, config.BancoHorasFactorAcumulacion);
            lineas.Add(NuevaLineaSembrada(periodo, orden++, RrhhDestinoTiempoExtraLinea.Banco, bancoBase, factorBanco, usuarioActual));
        }

        return lineas;
    }

    private static RrhhResolucionTiempoExtraLinea NuevaLineaSembrada(
        RrhhResolucionTiempoExtraPeriodo periodo, int orden, RrhhDestinoTiempoExtraLinea destino,
        int minutos, decimal factor, string usuarioActual)
        => new()
        {
            Id = Guid.NewGuid(),
            EmpresaId = periodo.EmpresaId,
            EmpleadoId = periodo.EmpleadoId,
            ResolucionPeriodoId = periodo.Id,
            Orden = orden,
            Destino = destino,
            Minutos = minutos,
            Factor = factor,
            Observaciones = "Línea sembrada por backfill Fase 9 desde escalares pre-Fase 8.",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = usuarioActual,
            IsActive = true
        };

    private async Task<(Empleado Empleado, NominaCorteRrhh? Corte)> CargarEmpleadoYCorteAsync(
        CrmDbContext db, Guid empresaId, Guid empleadoId, CancellationToken cancellationToken)
    {
        var empleado = await db.Empleados
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == empleadoId && e.EmpresaId == empresaId, cancellationToken)
            ?? throw new InvalidOperationException("No se encontró el empleado.");

        var corte = await db.NominaCortesRrhh
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.EmpresaId == empresaId && c.PeriodicidadPago == empleado.PeriodicidadPago, cancellationToken);

        return (empleado, corte);
    }

    /// <summary>
    /// Minutos de permiso CON GOCE prorrateados por día del periodo (misma regla
    /// que usa la vista semanal). El faltante cubierto por permiso NO debe ser
    /// tapado por el extra (Fase 2): el "faltante neto" es el bruto menos esto.
    /// </summary>
    private static async Task<Dictionary<DateOnly, int>> ConstruirPermisosConGocePorDiaAsync(
        CrmDbContext db, Guid empresaId, Guid empleadoId, DateOnly fechaInicio, DateOnly fechaFin, CancellationToken cancellationToken)
    {
        var permisos = await db.RrhhAusencias
            .AsNoTracking()
            .Where(a => a.EmpresaId == empresaId
                && a.EmpleadoId == empleadoId
                && a.IsActive
                && a.ConGocePago
                && a.Horas > 0
                && (a.Estatus == EstatusAusenciaRrhh.Aprobada || a.Estatus == EstatusAusenciaRrhh.Aplicada)
                && a.FechaInicio <= fechaFin
                && a.FechaFin >= fechaInicio)
            .ToListAsync(cancellationToken);

        var resultado = new Dictionary<DateOnly, int>();
        foreach (var permiso in permisos)
        {
            var minutosPorDia = RrhhTiempoExtraPolicy.ObtenerMinutosPermisoConGocePorDia(permiso);
            var inicio = permiso.FechaInicio < fechaInicio ? fechaInicio : permiso.FechaInicio;
            var fin = permiso.FechaFin > fechaFin ? fechaFin : permiso.FechaFin;
            for (var fecha = inicio; fecha <= fin; fecha = fecha.AddDays(1))
            {
                resultado[fecha] = resultado.GetValueOrDefault(fecha) + minutosPorDia;
            }
        }
        return resultado;
    }

    /// <summary>
    /// Minutos de banco CONSUMIDOS en el periodo (Fase 4): suma de movimientos
    /// <see cref="TipoMovimientoBancoHorasRrhh.Consumo"/> con Fecha dentro del
    /// periodo. Se EXCLUYE la cobertura-banco (<c>Asistencia:{id}:cobertura-banco</c>),
    /// porque ese consumo ya está representado como faltante neto en Fase 2
    /// (no hay RrhhAusencias que lo descuente) — incluirlo sería doble conteo.
    /// El Consumo se guarda con Horas NEGATIVAS, de ahí el signo invertido.
    /// </summary>
    private static async Task<int> ObtenerMinutosBancoConsumidoPeriodoAsync(
        CrmDbContext db, Guid empresaId, Guid empleadoId, DateOnly fechaInicio, DateOnly fechaFin, CancellationToken cancellationToken)
    {
        var consumos = await db.RrhhBancoHorasMovimientos
            .AsNoTracking()
            .Where(m => m.EmpresaId == empresaId
                && m.EmpleadoId == empleadoId
                && m.IsActive
                && m.TipoMovimiento == TipoMovimientoBancoHorasRrhh.Consumo
                && m.Fecha >= fechaInicio
                && m.Fecha <= fechaFin)
            .Select(m => new { m.Horas, m.ReferenciaTipo })
            .ToListAsync(cancellationToken);

        var consumidoHoras = consumos
            .Where(c => !EsCoberturaBanco(c.ReferenciaTipo))
            .Sum(c => -c.Horas); // Consumo se guarda negativo → invertir para obtener minutos consumidos

        return (int)Math.Round(consumidoHoras * 60m, MidpointRounding.AwayFromZero);
    }

    private static bool EsCoberturaBanco(string? referenciaTipo)
        => !string.IsNullOrEmpty(referenciaTipo) && referenciaTipo.Contains("cobertura-banco", StringComparison.Ordinal);

    private static NominaPeriodoCalendario ResolverPeriodo(Empleado empleado, DateOnly fechaReferencia, NominaCorteRrhh? corte)
        => NominaPeriodoHelper.ObtenerPeriodo(
            empleado.PeriodicidadPago,
            fechaReferencia.ToDateTime(TimeOnly.MinValue),
            corte);

    private static NominaPeriodoCalendario ResolverPeriodoDesdeFechas(Empleado empleado, DateOnly fechaInicio, DateOnly fechaFin, NominaCorteRrhh? corte)
    {
        var inicio = fechaInicio.ToDateTime(TimeOnly.MinValue);
        var fin = fechaFin.ToDateTime(TimeOnly.MinValue);
        var calendario = NominaPeriodoHelper.ObtenerPeriodo(empleado.PeriodicidadPago, fin, corte);

        // Cuando el rango forzado no coincide con el periodo de cierre tradicional
        // (p.ej. vista contenedor vs periodo cerrado), reconstruimos el calendario
        // para reflejar exactamente el inicio/fin recibido, conservando año y número
        // de periodo calculados a partir del día final.
        if (calendario.Inicio != inicio || calendario.Fin != fin)
        {
            calendario = new NominaPeriodoCalendario
            {
                PeriodicidadPago = empleado.PeriodicidadPago,
                Inicio = inicio,
                Fin = fin,
                AnioPeriodo = calendario.AnioPeriodo,
                NumeroPeriodo = calendario.NumeroPeriodo,
                Periodo = empleado.PeriodicidadPago == PeriodicidadPago.Semanal
                    ? NominaPeriodoHelper.ConstruirEtiquetaSemanal(inicio, fin)
                    : $"{calendario.Periodo} (ajustado)",
                NumeroNomina = calendario.NumeroNomina
            };
        }

        return calendario;
    }

    private static void GarantizarAplicable(Empleado empleado)
    {
        if (empleado.TipoNomina == TipoNomina.Destajo)
        {
            throw new InvalidOperationException(
                "Los empleados de destajo no participan en la resolución de tiempo extra por periodo.");
        }
    }

    private static string ConstruirPeriodoKey(NominaPeriodoCalendario calendario)
        => $"{calendario.PeriodicidadPago}-{calendario.AnioPeriodo}-{calendario.NumeroPeriodo:00}";

    private static string ConstruirReferenciaPeriodo(Guid empleadoId, string periodoKey, string sufijo)
        => $"{ReferenciaPeriodoExtraBancoPrefix}:{empleadoId:N}:{periodoKey}:{sufijo}";

    public async Task<IReadOnlyList<Guid>> ObtenerEmpleadosConExtraSinAutorizarAsync(
        CrmDbContext db, Guid empresaId, PeriodicidadPago periodicidad,
        DateTime inicio, DateTime fin, CancellationToken cancellationToken = default)
    {
        var fechaInicio = DateOnly.FromDateTime(inicio);
        var fechaFin = DateOnly.FromDateTime(fin);

        // Empleados con extra detectado en el rango (MinutosExtra >= 0, así que cualquier
        // fila > 0 implica Sum > 0). Distinct evita duplicar por día.
        var empleadosConExtra = await db.RrhhAsistencias
            .AsNoTracking()
            .Where(a => a.EmpresaId == empresaId
                && a.Fecha >= fechaInicio && a.Fecha <= fechaFin
                && a.MinutosExtra > 0)
            .Select(a => a.EmpleadoId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (empleadosConExtra.Count == 0)
            return Array.Empty<Guid>();

        // Acota a activos de la periodicidad (prenómina/nómina son por periodicidad).
        var activosPeriodicidad = await db.Empleados
            .AsNoTracking()
            .Where(e => e.EmpresaId == empresaId && e.IsActive && e.PeriodicidadPago == periodicidad
                && empleadosConExtra.Contains(e.Id))
            .Select(e => e.Id)
            .ToListAsync(cancellationToken);

        if (activosPeriodicidad.Count == 0)
            return Array.Empty<Guid>();

        // Resoluciones Autorizadas del periodo (lookup por fechas, igual que el snapshot).
        var autorizados = await db.RrhhResolucionesTiempoExtraPeriodo
            .AsNoTracking()
            .Where(r => r.EmpresaId == empresaId
                && r.FechaInicio == fechaInicio && r.FechaFin == fechaFin
                && r.Estatus == RrhhResolucionPeriodoEstatus.Autorizada
                && r.IsActive
                && activosPeriodicidad.Contains(r.EmpleadoId))
            .Select(r => r.EmpleadoId)
            .ToListAsync(cancellationToken);

        var autorizadosSet = autorizados.ToHashSet();
        return activosPeriodicidad.Where(id => !autorizadosSet.Contains(id)).ToList();
    }
}