using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Services;

public class TenantFileStorageService : ITenantFileStorageService
{
    private const string TenantFilesRoutePrefix = "/tenant-files";
    private readonly IWebHostEnvironment _env;
    private readonly IDbContextFactory<CrmDbContext> _dbFactory;
    private readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

    public TenantFileStorageService(IWebHostEnvironment env, IDbContextFactory<CrmDbContext> dbFactory)
    {
        _env = env;
        _dbFactory = dbFactory;
    }

    public Task<string> SaveAsync(Guid empresaId, string area, string fileNamePrefix, string extension, Stream content, string? existingRelativePath = null, CancellationToken cancellationToken = default)
        => SaveAsync(new TenantFileStorageRequest
        {
            EmpresaId = empresaId,
            Area = area,
            FileNamePrefix = fileNamePrefix,
            Extension = extension,
            Modulo = area,
            Entidad = area
        }, content, existingRelativePath, cancellationToken);

    public async Task<string> SaveAsync(TenantFileStorageRequest request, Stream content, string? existingRelativePath = null, CancellationToken cancellationToken = default)
    {
        if (request.EmpresaId == Guid.Empty)
            throw new InvalidOperationException("Se requiere una empresa válida para guardar archivos.");

        var effective = await ResolveEffectiveConfigurationAsync(request.EmpresaId, cancellationToken);
        var normalizedExtension = request.Extension.StartsWith('.') ? request.Extension.ToLowerInvariant() : $".{request.Extension.ToLowerInvariant()}";
        var fileName = $"{NormalizeSegment(request.FileNamePrefix)}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{normalizedExtension}";
        var storageDirectory = await BuildStorageDirectoryAsync(request, effective.PathTemplate, cancellationToken);
        var storagePath = string.IsNullOrWhiteSpace(storageDirectory) ? fileName : $"{storageDirectory}/{fileName}";
        var physicalRoot = ResolvePhysicalRoot(effective.BasePath);
        var physicalDirectory = Path.Combine(physicalRoot, storageDirectory.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(physicalDirectory);

        var physicalPath = Path.Combine(physicalRoot, storagePath.Replace('/', Path.DirectorySeparatorChar));
        await using (var fs = new FileStream(physicalPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await content.CopyToAsync(fs, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(existingRelativePath))
            await DeleteIfExistsAsync(existingRelativePath, cancellationToken);

        return BuildPublicPath(request.EmpresaId, storagePath);
    }

    public async Task DeleteIfExistsAsync(string? relativePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return;

        if (TryParseTenantFilePath(relativePath, out var empresaId, out var storagePath))
        {
            var effective = await ResolveEffectiveConfigurationAsync(empresaId, cancellationToken);
            var physicalPath = Path.Combine(ResolvePhysicalRoot(effective.BasePath), storagePath.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(physicalPath))
                File.Delete(physicalPath);
            return;
        }

        if (relativePath.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            var physicalPath = GetLegacyUploadsPhysicalPath(relativePath);
            if (File.Exists(physicalPath))
                File.Delete(physicalPath);
        }
    }

    public string GetRelativeDirectory(Guid empresaId, string area)
        => BuildPublicPath(empresaId, NormalizeSegment(area));

    public async Task<TenantStoredFile?> OpenReadAsync(Guid empresaId, string storagePath, CancellationToken cancellationToken = default)
    {
        if (empresaId == Guid.Empty || string.IsNullOrWhiteSpace(storagePath))
            return null;

        var effective = await ResolveEffectiveConfigurationAsync(empresaId, cancellationToken);
        var physicalPath = Path.Combine(ResolvePhysicalRoot(effective.BasePath), storagePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(physicalPath))
            return null;

        var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (!_contentTypeProvider.TryGetContentType(physicalPath, out var contentType))
            contentType = "application/octet-stream";

        return new TenantStoredFile
        {
            Content = stream,
            ContentType = contentType,
            FileName = Path.GetFileName(physicalPath)
        };
    }

    public static bool TryParseTenantFilePath(string? publicPath, out Guid empresaId, out string storagePath)
    {
        empresaId = Guid.Empty;
        storagePath = string.Empty;

        if (string.IsNullOrWhiteSpace(publicPath) || !publicPath.StartsWith(TenantFilesRoutePrefix, StringComparison.OrdinalIgnoreCase))
            return false;

        var segments = publicPath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 3 || !string.Equals(segments[0], TenantFilesRoutePrefix.Trim('/'), StringComparison.OrdinalIgnoreCase))
            return false;

        if (!Guid.TryParseExact(segments[1], "N", out empresaId))
            return false;

        storagePath = string.Join('/', segments.Skip(2));
        return !string.IsNullOrWhiteSpace(storagePath);
    }

    private async Task<(StorageProviderEnum Provider, string? BasePath, string PathTemplate)> ResolveEffectiveConfigurationAsync(Guid empresaId, CancellationToken cancellationToken)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var global = await db.StorageConfiguracionesGlobales
            .AsNoTracking()
            .OrderBy(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var empresa = await db.Empresas
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == empresaId, cancellationToken);
        if (empresa == null)
            throw new InvalidOperationException("No se encontró la empresa para resolver la configuración de almacenamiento.");

        var overrideConfig = await db.EmpresasStorageConfiguracion
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.EmpresaId == empresaId, cancellationToken);

        if (overrideConfig is { UsaConfiguracionGlobal: false })
        {
            return (overrideConfig.StorageProvider,
                string.IsNullOrWhiteSpace(overrideConfig.BasePath) ? null : overrideConfig.BasePath.Trim(),
                string.IsNullOrWhiteSpace(overrideConfig.PathTemplate) ? StoragePathTemplates.Default : overrideConfig.PathTemplate.Trim());
        }

        return (global?.StorageProvider ?? StorageProviderEnum.Local,
            string.IsNullOrWhiteSpace(global?.BasePath) ? null : global.BasePath.Trim(),
            string.IsNullOrWhiteSpace(global?.PathTemplate) ? StoragePathTemplates.Default : global.PathTemplate.Trim());
    }

    private async Task<string> BuildStorageDirectoryAsync(TenantFileStorageRequest request, string template, CancellationToken cancellationToken)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var empresa = await db.Empresas.AsNoTracking().FirstAsync(e => e.Id == request.EmpresaId, cancellationToken);

        var empresaToken = !string.IsNullOrWhiteSpace(empresa.Codigo)
            ? empresa.Codigo
            : request.EmpresaId.ToString("N");
        var clienteToken = !string.IsNullOrWhiteSpace(request.ClienteNombre)
            ? request.ClienteNombre
            : request.ClienteId?.ToString("N") ?? "general";
        var documentoToken = !string.IsNullOrWhiteSpace(request.DocumentoFolio)
            ? request.DocumentoFolio
            : request.DocumentoId?.ToString("N") ?? request.FileNamePrefix;
        var replacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["empresa"] = empresaToken,
            ["cliente"] = clienteToken,
            ["nota"] = documentoToken,
            ["documento"] = documentoToken,
            ["modulo"] = request.Modulo ?? request.Area,
            ["entidad"] = request.Entidad ?? request.Area,
            ["anio"] = DateTime.UtcNow.Year.ToString(),
            ["mes"] = DateTime.UtcNow.Month.ToString("D2")
        };

        var rendered = template;
        foreach (var replacement in replacements)
        {
            rendered = rendered.Replace($"{{{replacement.Key}}}", NormalizeSegment(replacement.Value), StringComparison.OrdinalIgnoreCase);
        }

        var parts = rendered
            .Split('/', '\\', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeSegment)
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .ToList();

        return string.Join('/', parts);
    }

    private string ResolvePhysicalRoot(string? configuredBasePath)
    {
        if (!string.IsNullOrWhiteSpace(configuredBasePath))
            return configuredBasePath.Trim();

        return Path.Combine(_env.WebRootPath, "uploads");
    }

    private static string BuildPublicPath(Guid empresaId, string storagePath)
        => $"{TenantFilesRoutePrefix}/{empresaId:N}/{storagePath.Trim('/')}";


    private string GetLegacyUploadsPhysicalPath(string relativePath)
    {
        var normalizedRelativePath = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(_env.WebRootPath, normalizedRelativePath);
    }

    private static string NormalizeSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "files";

        var chars = value
            .Trim()
            .ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray();

        return string.Join(string.Empty, chars).Trim('-');
    }
}
