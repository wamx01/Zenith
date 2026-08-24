using MundoVs.Core.Entities;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Interfaces;

/// <summary>
/// Servicio para permisos por diferencia neta generados al cierre de un periodo
/// cuando |retardoDetectado − extraDetectado| > 0. Crea filas sintéticas en
/// <see cref="RrhhAusencia"/> (Tipo=PermisoPorDiferenciaPeriodo, OrigenAusencia=SinteticoPorPeriodo)
/// y, cuando la categoría es Banco, escribe el movimiento de Consumo en
/// RrhhBancoHorasMovimientos vía <see cref="IRrhhTiempoExtraResolutionService.AplicarPermisoConGoceBancoHorasAsync"/>.
///
/// Concentra todos los permisos del periodo en <c>FechaInicio=FechaFin=periodo.FechaFin</c>
/// con <c>Dias=1</c>: así el cálculo diario (Fase 2/3/4) no prorratea y aplica
/// la cobertura completa al último día del periodo.
///
/// Idempotente: re-aplicar reemplaza las sintéticas previas del periodo. Reabrir
/// el periodo las borra en silencio (lo hace <see cref="IRrhhResolucionPeriodoService.ReabrirPeriodoAsync"/>).
/// </summary>
public interface IRrhhPermisoPorDiferenciaService
{
    /// <summary>
    /// Lee el periodo que contiene <paramref name="fechaReferencia"/> y devuelve la
    /// diferencia neta (retardoDetectado − extraDetectado) y el saldo de banco disponible.
    /// Read-only: no crea filas ni mueve el ledger.
    /// </summary>
    Task<PermisoDiferenciaSugerencia> CalcularSugerenciaAsync(
        CrmDbContext db, Guid empresaId, Guid empleadoId,
        DateOnly fechaReferencia, CancellationToken cancellationToken = default);

    /// <summary>
    /// Aplica los permisos capturados por el operador: revierte las sintéticas previas
    /// del periodo, crea las nuevas y, para las filas de categoría Banco, consume el
    /// saldo del banco de horas. La suma de Minutos de los inputs no puede exceder
    /// la diferencia neta del periodo. Cada Minutos debe ser >= 0 y Factor >= 1.
    /// Lanza <see cref="InvalidOperationException"/> si el banco excede el saldo o la
    /// suma excede la diferencia. Devuelve las ausencias creadas (en el mismo orden que inputs).
    /// </summary>
    Task<List<RrhhAusencia>> AplicarPermisosAsync(
        CrmDbContext db, Guid empresaId, Guid empleadoId,
        DateOnly fechaReferencia, IReadOnlyList<PermisoDiferenciaInput> inputs,
        string usuarioActual, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revierte todas las ausencias sintéticas del periodo (OrigenAusencia=SinteticoPorPeriodo,
    /// PeriodoKey=X) y los movimientos de banco asociados (prefijo permiso-banco:{ausenciaId}).
    /// Read-modify-write: NO toca ausencias manuales. Usado por la reapertura del periodo.
    /// </summary>
    Task RevertirPermisosAsync(
        CrmDbContext db, Guid empresaId, Guid empleadoId,
        DateOnly fechaReferencia, CancellationToken cancellationToken = default);
}