using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Services;

/// <summary>
/// Implementación de <see cref="IRrhhPermisoPorDiferenciaService"/>. Genera permisos
/// sintéticos al cierre del periodo cuando el neto es negativo, es decir, cuando
/// (faltante + retardo) &gt; extra. El déficit (faltante descontable + retardo efectivo,
/// espejo del neteo F2/F3) menos el extra es la diferencia que se materializa como permiso.
///
/// Notas de diseño (Fase PermisoPorDiferenciaPeriodo):
///  - Las filas se concentran en FechaInicio=FechaFin=periodo.FechaFin con Dias=1: el
///    cálculo diario (Fase 2/3/4) NO prorratea y aplica la cobertura completa al último día.
///  - Categoría "Banco": llama a AplicarPermisoConGoceBancoHorasAsync (prefijo
///    "permiso-banco:{ausenciaId:N}"). El banco consumido entra al F4 como cualquier
///    Consumo del periodo, así que si el periodo tiene extra, el extra repone el consumo.
///  - Categoría "SinGoce": fluye al descuento manual en ConstruirPermisosConGocePorDiaAsync
///    (no aplica al prorrateo de permiso con goce, pero igual se resta del salario).
///  - Idempotencia: RevertirPermisosAsync borra todas las sintéticas previas del periodo
///    y los movimientos de banco asociados, antes de aplicar las nuevas.
///  - Reapertura: <see cref="IRrhhResolucionPeriodoService.ReabrirPeriodoAsync"/> invoca
///    RevertirPermisosAsync en silencio.
/// </summary>
public sealed class RrhhPermisoPorDiferenciaService : IRrhhPermisoPorDiferenciaService
{
    private const string PrefijoReferenciaBanco = "permiso-banco";

    private readonly IRrhhTiempoExtraResolutionService _tiempoExtra;

    public RrhhPermisoPorDiferenciaService(IRrhhTiempoExtraResolutionService tiempoExtra)
    {
        _tiempoExtra = tiempoExtra;
    }

    public async Task<PermisoDiferenciaSugerencia> CalcularSugerenciaAsync(
        CrmDbContext db, Guid empresaId, Guid empleadoId,
        DateOnly fechaReferencia, CancellationToken cancellationToken = default)
    {
        // Reutilizamos el lookup que ya hace RrhhResolucionPeriodoService para resolver
        // el calendario y cargar empleado + corte. ResolverPeriodo es privado en ese
        // servicio, así que replicamos la lectura aquí (mismo patrón que BackfillDesdeAutorizacionDiariaAsync).
        var (empleado, corte) = await CargarEmpleadoYCorteAsync(db, empresaId, empleadoId, cancellationToken);
        var calendario = NominaPeriodoHelper.ObtenerPeriodoContenedor(
            empleado.PeriodicidadPago,
            fechaReferencia.ToDateTime(TimeOnly.MinValue),
            corte);

        var fechaInicio = DateOnly.FromDateTime(calendario.Inicio);
        var fechaFin = DateOnly.FromDateTime(calendario.Fin);

        var permisosPorDia = await ConstruirPermisosConGocePorDiaAsync(db, empresaId, empleadoId, fechaInicio, fechaFin, cancellationToken);

        var (extraDetectado, deficitDetectado) = await CalcularDeteccionPeriodoAsync(
            db, empresaId, empleadoId, fechaInicio, fechaFin, permisosPorDia, cancellationToken);

        // Diferencia = neto negativo del periodo (faltante + retardo que el extra no tapa).
        // Difference = period's negative net (faltante + retardo not covered by extra).
        var diferencia = Math.Max(0, deficitDetectado - extraDetectado);
        var bancoDisponible = diferencia > 0
            ? await _tiempoExtra.ObtenerSaldoBancoHorasAsync(db, empresaId, empleadoId, cancellationToken)
            : 0;

        return new PermisoDiferenciaSugerencia
        {
            DiferenciaMinutos = diferencia,
            BancoDisponibleMinutos = bancoDisponible,
            PeriodoEtiqueta = calendario.Periodo,
            PeriodoKey = $"{calendario.PeriodicidadPago}-{calendario.AnioPeriodo}-{calendario.NumeroPeriodo:00}"
        };
    }

    public async Task<List<RrhhAusencia>> AplicarPermisosAsync(
        CrmDbContext db, Guid empresaId, Guid empleadoId,
        DateOnly fechaReferencia, IReadOnlyList<PermisoDiferenciaInput> inputs,
        string usuarioActual, CancellationToken cancellationToken = default)
    {
        if (inputs is null)
        {
            throw new ArgumentNullException(nameof(inputs));
        }

        // 1. Resolver el periodo (crea la entidad si no existe — mismo patrón que
        //    AplicarResolucionPeriodoAsync).
        var (empleado, corte) = await CargarEmpleadoYCorteAsync(db, empresaId, empleadoId, cancellationToken);
        if (empleado.TipoNomina == TipoNomina.Destajo)
        {
            throw new InvalidOperationException(
                "Los empleados de destajo no participan en la resolución de tiempo extra por periodo.");
        }
        var calendario = NominaPeriodoHelper.ObtenerPeriodoContenedor(
            empleado.PeriodicidadPago,
            fechaReferencia.ToDateTime(TimeOnly.MinValue),
            corte);
        var fechaFinPeriodo = DateOnly.FromDateTime(calendario.Fin);
        var fechaInicioPeriodo = DateOnly.FromDateTime(calendario.Inicio);
        var periodoKey = $"{calendario.PeriodicidadPago}-{calendario.AnioPeriodo}-{calendario.NumeroPeriodo:00}";

        // 2. Validar inputs.
        foreach (var input in inputs)
        {
            if (input.Minutos < 0)
            {
                throw new InvalidOperationException("Los Minutos de cada permiso por diferencia deben ser >= 0.");
            }
            if (input.Factor < 1m)
            {
                throw new InvalidOperationException("El Factor de cada permiso por diferencia debe ser >= 1.");
            }
        }

        var totalMinutos = inputs.Sum(i => i.Minutos);
        if (totalMinutos == 0)
        {
            // Sin minutos = sin cambios. Revertimos y salimos limpio.
            await RevertirPermisosAsync(db, empresaId, empleadoId, fechaReferencia, cancellationToken);
            return new List<RrhhAusencia>();
        }

        var permisosPorDia = await ConstruirPermisosConGocePorDiaAsync(db, empresaId, empleadoId, fechaInicioPeriodo, fechaFinPeriodo, cancellationToken);
        var (extraDetectado, deficitDetectado) = await CalcularDeteccionPeriodoAsync(
            db, empresaId, empleadoId, fechaInicioPeriodo, fechaFinPeriodo, permisosPorDia, cancellationToken);
        // Diferencia = neto negativo del periodo (faltante + retardo que el extra no tapa).
        // Difference = period's negative net (faltante + retardo not covered by extra).
        var diferenciaMax = Math.Max(0, deficitDetectado - extraDetectado);

        if (totalMinutos > diferenciaMax)
        {
            throw new InvalidOperationException(
                $"La suma de minutos por permiso por diferencia ({totalMinutos}) no puede exceder la diferencia neta del periodo ({diferenciaMax} min = déficit {deficitDetectado} [faltante + retardo] − extra {extraDetectado}).");
        }

        // 3. Validar que la categoría Banco no exceda el saldo disponible (lectura previa
        //    al SaveChanges — el helper AplicarPermisoConGoceBancoHorasAsync también valida,
        //    pero aquí lanzamos antes para evitar medias escrituras si algo viene mal).
        var minutosBanco = inputs
            .Where(i => i.Categoria == CategoriaPermisoDiferencia.Banco)
            .Sum(i => i.Minutos);
        if (minutosBanco > 0)
        {
            var saldoDisponible = await _tiempoExtra.ObtenerSaldoBancoHorasAsync(db, empresaId, empleadoId, cancellationToken);
            if (minutosBanco > saldoDisponible)
            {
                throw new InvalidOperationException(
                    $"Los minutos por banco ({minutosBanco}) exceden el saldo disponible del banco de horas ({saldoDisponible} min).");
            }
        }

        // 4. Idempotencia — revertir sintéticas previas del periodo.
        await RevertirPermisosAsync(db, empresaId, empleadoId, fechaReferencia, cancellationToken);

        // 5. Crear filas nuevas (solo para las categorías con Minutos > 0).
        var ausenciasCreadas = new List<RrhhAusencia>();
        var ahora = DateTime.UtcNow;

        foreach (var input in inputs)
        {
            if (input.Minutos <= 0)
            {
                continue;
            }

            var (tipo, conGoce, descuentaBanco) = input.ResolverFlags();
            var ausencia = new RrhhAusencia
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresaId,
                EmpleadoId = empleadoId,
                Tipo = tipo,
                Estatus = EstatusAusenciaRrhh.Aplicada,
                FechaInicio = fechaFinPeriodo,
                FechaFin = fechaFinPeriodo,
                Dias = 1,
                Horas = Math.Round(input.Minutos / 60m, 2),
                ConGocePago = conGoce,
                DescuentaBancoHoras = descuentaBanco,
                OrigenAusencia = OrigenAusenciaRrhh.SinteticoPorPeriodo,
                PeriodoKey = periodoKey,
                Motivo = $"Permiso por diferencia ({input.Categoria})",
                Observaciones = input.Observaciones,
                FechaAprobacion = ahora,
                AprobadoPor = usuarioActual,
                CreatedAt = ahora,
                CreatedBy = usuarioActual,
                IsActive = true
            };
            db.RrhhAusencias.Add(ausencia);
            ausenciasCreadas.Add(ausencia);
        }

        if (ausenciasCreadas.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        // 6. Categoría Banco → consumir el saldo vía el helper existente
        //    (reusa el prefijo "permiso-banco:{ausenciaId:N}" → la reversión los borra por el mismo).
        //    El helper agrega el RrhhBancoHorasMovimiento al DbSet pero NO persiste
        //    (asume que el caller tiene una transacción mayor); aquí hacemos SaveChanges
        //    explícito para que el flujo quede completo si solo se invoca este método.
        var huboConsumoBanco = ausenciasCreadas.Any(a => a.DescuentaBancoHoras);
        if (huboConsumoBanco)
        {
            foreach (var ausencia in ausenciasCreadas.Where(a => a.DescuentaBancoHoras))
            {
                await _tiempoExtra.AplicarPermisoConGoceBancoHorasAsync(
                    db,
                    new RrhhPermisoBancoHorasCommand
                    {
                        EmpresaId = empresaId,
                        EmpleadoId = empleadoId,
                        AusenciaId = ausencia.Id,
                        Fecha = fechaFinPeriodo,
                        HorasPermiso = ausencia.Horas,
                        Observaciones = ausencia.Observaciones ?? $"Permiso por diferencia de periodo {periodoKey}.",
                        UsuarioActual = usuarioActual
                    },
                    cancellationToken);
            }
            await db.SaveChangesAsync(cancellationToken);
        }

        return ausenciasCreadas;
    }

    public async Task RevertirPermisosAsync(
        CrmDbContext db, Guid empresaId, Guid empleadoId,
        DateOnly fechaReferencia, CancellationToken cancellationToken = default)
    {
        var (empleado, corte) = await CargarEmpleadoYCorteAsync(db, empresaId, empleadoId, cancellationToken);
        var calendario = NominaPeriodoHelper.ObtenerPeriodoContenedor(
            empleado.PeriodicidadPago,
            fechaReferencia.ToDateTime(TimeOnly.MinValue),
            corte);
        var periodoKey = $"{calendario.PeriodicidadPago}-{calendario.AnioPeriodo}-{calendario.NumeroPeriodo:00}";

        // Cargamos las ausencias sintéticas del periodo para resolver el Id → referencia de banco.
        var ausenciasSinteticas = await db.RrhhAusencias
            .Where(a => a.EmpresaId == empresaId
                && a.EmpleadoId == empleadoId
                && a.OrigenAusencia == OrigenAusenciaRrhh.SinteticoPorPeriodo
                && a.PeriodoKey == periodoKey
                && a.Tipo == TipoAusenciaRrhh.PermisoPorDiferenciaPeriodo
                && a.IsActive)
            .ToListAsync(cancellationToken);

        if (ausenciasSinteticas.Count == 0)
        {
            return;
        }

        // Borrar movimientos de banco asociados (prefijo "permiso-banco:{ausenciaId:N}").
        // Iteramos por ausencia para no arrastrar movimientos manuales (cualquier
        // referencia con ese prefijo corresponde a un consumo de esta ausencia sintética).
        foreach (var ausencia in ausenciasSinteticas.Where(a => a.DescuentaBancoHoras))
        {
            var referencia = ConstruirReferenciaBanco(ausencia.Id);
            var movimientos = await db.RrhhBancoHorasMovimientos
                .Where(m => m.EmpresaId == empresaId
                    && m.EmpleadoId == empleadoId
                    && m.IsActive
                    && m.ReferenciaTipo == referencia)
                .ToListAsync(cancellationToken);
            if (movimientos.Count > 0)
            {
                db.RrhhBancoHorasMovimientos.RemoveRange(movimientos);
            }
        }

        db.RrhhAusencias.RemoveRange(ausenciasSinteticas);
        await db.SaveChangesAsync(cancellationToken);
    }

    // ─── helpers ───────────────────────────────────────────────────────────────────

    private static string ConstruirReferenciaBanco(Guid ausenciaId)
        => $"{PrefijoReferenciaBanco}:{ausenciaId:N}";

    private static async Task<(Empleado empleado, NominaCorteRrhh? corte)> CargarEmpleadoYCorteAsync(
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
    /// Réplica mínima del prorrateo de permisos con goce que usa
    /// <c>ConstruirPermisosConGocePorDiaAsync</c> en RrhhResolucionPeriodoService.
    /// Solo lectura (AsNoTracking). Se usa para calcular la diferencia neta sin
    /// efectos sobre el periodo resuelto.
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
    /// Detección agregada del periodo: extra (líquido) y déficit (lado negativo del neto).
    /// Déficit = faltante descontable + retardo efectivo, espejo del neto del periodo que
    /// calcula <c>AplicarResolucionPeriodoAsync</c> (extra − faltanteNeto − retardo − banco).
    /// La diferencia sugerida para el permiso es max(0, déficit − extra), es decir, el neto
    /// negativo del periodo. El faltante descontable resta los permisos con goce y la
    /// compensación aprobada (igual que el neteo) para no doble-contar una ausencia ya
    /// cubierta por un permiso.
    /// English: Period detection — returns extra (liquid) and deficit (negative side of the
    /// neto = faltante descontable + retardo efectivo). The suggested permiso difference is
    /// max(0, deficit − extra), i.e. the period's negative net. Faltante descontable subtracts
    /// con-goce permits and approved compensation (mirroring the neteo) so an absence already
    /// covered by a permit is not double-counted.
    /// </summary>
    private static async Task<(int extraDetectado, int deficitDetectado)> CalcularDeteccionPeriodoAsync(
        CrmDbContext db, Guid empresaId, Guid empleadoId,
        DateOnly fechaInicio, DateOnly fechaFin,
        Dictionary<DateOnly, int> permisosPorDia,
        CancellationToken cancellationToken)
    {
        var asistencias = await db.RrhhAsistencias
            .AsNoTracking()
            .Where(a => a.EmpresaId == empresaId && a.EmpleadoId == empleadoId
                && a.Fecha >= fechaInicio && a.Fecha <= fechaFin)
            .ToListAsync(cancellationToken);

        // Meta semanal (Fija sin turno): el déficit descuenta sueldo como FaltanteDescontable,
        // NO genera permiso por diferencia. Se anula la detección para que diferencia = 0
        // (panel oculto) y cualquier input positivo se rechaza en AplicarPermisos.
        // English: Weekly meta (Fija with no shift): the deficit docks salary as
        // FaltanteDescontable, it does NOT generate a permiso por diferencia. Detection is
        // zeroed so diferencia = 0 (panel hidden) and any positive input is rejected.
        if (RrhhTiempoExtraPolicy.EsPeriodoMetaSemanal(asistencias))
        {
            return (0, 0);
        }

        var extraDetectado = asistencias.Sum(a => Math.Max(0, a.MinutosExtra));
        // Déficit = lado negativo del neto: faltante descontable + retardo efectivo.
        // Deficit = negative side of the neto: descontable faltante + effective retardo.
        var faltanteDetectado = asistencias.Sum(a =>
            RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteDescontable(
                a, permisosPorDia.GetValueOrDefault(a.Fecha), Math.Max(0, a.MinutosCompensacionPermisoAprobados)));
        var retardoDetectado = asistencias.Sum(a =>
            RrhhTiempoExtraPolicy.ObtenerMinutosRetardoEfectivos(a, permisosPorDia.GetValueOrDefault(a.Fecha)));
        var deficitDetectado = faltanteDetectado + retardoDetectado;

        return (extraDetectado, deficitDetectado);
    }
}