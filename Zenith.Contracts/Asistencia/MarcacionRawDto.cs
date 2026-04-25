namespace Zenith.Contracts.Asistencia;

public sealed class MarcacionRawDto
{
    public Guid EmpresaId { get; set; }
    public Guid ChecadorId { get; set; }
    public string CodigoChecador { get; set; } = string.Empty;
    public DateTime? FechaHoraMarcacionLocal { get; set; }
    public DateTime FechaHoraMarcacionUtc { get; set; }
    public string? ZonaHorariaAplicada { get; set; }
    public string? TipoMarcacionRaw { get; set; }
    public string? Origen { get; set; }
    public string? EventoIdExterno { get; set; }
    public string? PayloadRaw { get; set; }
}