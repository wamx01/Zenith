namespace MundoVs.Core.Interfaces;

public interface ITenantFileStorageService
{
    Task<string> SaveAsync(TenantFileStorageRequest request, Stream content, string? existingRelativePath = null, CancellationToken cancellationToken = default);
    Task<string> SaveAsync(Guid empresaId, string area, string fileNamePrefix, string extension, Stream content, string? existingRelativePath = null, CancellationToken cancellationToken = default);
    Task DeleteIfExistsAsync(string? relativePath, CancellationToken cancellationToken = default);
    string GetRelativeDirectory(Guid empresaId, string area);
    Task<TenantStoredFile?> OpenReadAsync(Guid empresaId, string storagePath, CancellationToken cancellationToken = default);
}

public sealed class TenantFileStorageRequest
{
    public Guid EmpresaId { get; set; }
    public string Area { get; set; } = string.Empty;
    public string FileNamePrefix { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string? Modulo { get; set; }
    public string? Entidad { get; set; }
    public Guid? ClienteId { get; set; }
    public string? ClienteNombre { get; set; }
    public Guid? DocumentoId { get; set; }
    public string? DocumentoFolio { get; set; }
}

public sealed class TenantStoredFile
{
    public required Stream Content { get; init; }
    public required string ContentType { get; init; }
    public required string FileName { get; init; }
}
