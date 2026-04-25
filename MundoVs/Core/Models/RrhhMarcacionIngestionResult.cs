using Zenith.Contracts.Asistencia;

namespace MundoVs.Core.Models;

public sealed class RrhhMarcacionIngestionResult
{
    public int StatusCode { get; init; }
    public SyncResultDto Response { get; init; } = new();

    public bool IsSuccess => StatusCode is >= 200 and < 300;
}
