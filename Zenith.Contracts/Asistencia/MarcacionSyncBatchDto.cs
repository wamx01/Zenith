namespace Zenith.Contracts.Asistencia;

public sealed class MarcacionSyncBatchDto
{
    public Guid EmpresaId { get; set; }
    public Guid ChecadorId { get; set; }
    public string? NumeroSerieChecador { get; set; }
    public IReadOnlyCollection<MarcacionRawDto> Marcaciones { get; set; } = [];
}