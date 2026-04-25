namespace MundoVs.Core.Interfaces;

public interface IFacturacionFiscalService
{
    Task<FacturacionFiscalResult> EmitirFacturaAsync(Guid facturaId, CancellationToken cancellationToken = default);
}

public sealed class FacturacionFiscalResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? ExternalDocumentId { get; init; }
    public string? UuidFiscal { get; init; }
    public string? SerieFiscal { get; init; }
    public string? FolioFiscal { get; init; }
    public string? XmlUrl { get; init; }
    public string? PdfUrl { get; init; }
    public DateTime? FechaTimbrado { get; init; }
    public string? ErrorCode { get; init; }
}
