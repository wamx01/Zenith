using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Interfaces;

// Método de cálculo a forzar explícitamente en un reproceso por periodo, elegido por el
// operador en el diálogo previo al recálculo. Distinto de "default del empleado": aquí el
// operador impone un método concreto sin importar lo configurado en Empleado. null = no
// forzar (aplica la lógica existente: forzarDefaultEmpleado o preservar override + default).
// English: Calculation method to explicitly force in a period reprocess, chosen by the
// operator in the pre-recalc dialog. Distinct from "employee default": the operator imposes a
// concrete method regardless of Empleado config. null = don't force (existing logic applies:
// forzarDefaultEmpleado or preserve override + default fallback).
public enum RrhhModoCalculoForzado { VsHorario, MarcajeReloj }

public interface IRrhhAsistenciaProcessor
{
    Task ProcesarMarcacionesPendientesAsync(CrmDbContext db, Guid empresaId, Guid checadorId, CancellationToken cancellationToken = default);
    // forzarDefaultEmpleado=true impone el método por defecto del empleado a todos los días
    // (reproceso por periodo: el default gana sobre overrides manuales por día). false (default)
    // preserva el override por día y usa el default sólo como fallback (recálculo por día /
    // incremental). modoCalculoForzado, si no es null, gana SOBRE todo lo demás: impone un método
    // concreto (VsHorario/MarcajeReloj) elegido por el operador, sin importar el default del
    // empleado. English: forzarDefaultEmpleado=true enforces the employee's default method on
    // every day (period reprocess: default wins over per-day overrides). false (default)
    // preserves the per-day override and uses the default only as a fallback (per-day /
    // incremental). modoCalculoForzado, when non-null, wins OVER everything: enforces a concrete
    // method (VsHorario/MarcajeReloj) chosen by the operator, regardless of the employee default.
    Task<int> ReprocesarRangoAsync(CrmDbContext db, Guid empresaId, DateOnly fechaDesde, DateOnly fechaHasta, Guid? empleadoId = null, bool forzarDefaultEmpleado = false, RrhhModoCalculoForzado? modoCalculoForzado = null, CancellationToken cancellationToken = default);
    Task<int> ReprocesarRangoAsync(CrmDbContext db, Guid empresaId, DateOnly fechaDesde, DateOnly fechaHasta, Guid? empleadoId, IProgress<RrhhAsistenciaReprocesoProgreso>? progress, bool forzarDefaultEmpleado = false, RrhhModoCalculoForzado? modoCalculoForzado = null, CancellationToken cancellationToken = default);
}

public sealed record RrhhAsistenciaReprocesoProgreso(int Procesados, int Total, Guid EmpleadoId, DateOnly Fecha);
