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
    public Dictionary<string, int> CompensacionesPorDia { get; init; } = new();
}
