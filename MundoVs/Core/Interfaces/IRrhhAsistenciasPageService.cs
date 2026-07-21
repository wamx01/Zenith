using MundoVs.Core.Entities;
using MundoVs.Core.Models;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Interfaces;

public interface IRrhhAsistenciasPageService
{
    Task<RrhhAsistenciasPageData> CargarAsync(CrmDbContext db, Guid empresaId, RrhhAsistenciasFiltroState filtros, CancellationToken cancellationToken = default);
}

public sealed class RrhhAsistenciasPageData
{
    public IReadOnlyList<RrhhAsistencia> Asistencias { get; init; } = [];
    public IReadOnlyList<TurnoBase> Turnos { get; init; } = [];
    public IReadOnlyList<Empleado> EmpleadosReproceso { get; init; } = [];
    public Dictionary<string, string> AusenciasPorDia { get; init; } = new();
    // Minutos de permiso con goce prorrateados al día (multi-día repartido, no el total
    // a cada día). Fuente canónica del "permiso visible" compartida por lista diaria,
    // semanal y modal. El banco-cobertura NO va aquí: lo añade el policy.
    public Dictionary<string, int> PermisosVisiblesPorDia { get; init; } = new();
}
