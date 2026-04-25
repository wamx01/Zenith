using Zenith.Workers.Asistencia.Models;

namespace Zenith.Workers.Asistencia.Abstractions;

public interface IAsistenciaSyncService
{
    Task<AsistenciaSyncCycleResult> EjecutarCicloAsync(CancellationToken cancellationToken);
}