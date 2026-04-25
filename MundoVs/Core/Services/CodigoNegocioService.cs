using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Services;

public class CodigoNegocioService
{
    private const string CompanyCodeShortKey = "CompanyCodeShort";
    private readonly IDbContextFactory<CrmDbContext> _dbFactory;

    public CodigoNegocioService(IDbContextFactory<CrmDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<string> GetCompanyShortCodeAsync(Guid empresaId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var empresa = await db.Empresas.IgnoreQueryFilters().FirstAsync(e => e.Id == empresaId, cancellationToken);
        return await EnsureCompanyShortCodeAsync(db, empresa, cancellationToken);
    }

    public string BuildCompanyCodePreview(string? nombreComercial, string? razonSocial)
    {
        if (string.IsNullOrWhiteSpace(nombreComercial) && string.IsNullOrWhiteSpace(razonSocial))
            return string.Empty;

        return BuildCompanyCodeBase(nombreComercial, razonSocial ?? string.Empty);
    }

    public async Task<string> EnsureCompanyShortCodeAsync(CrmDbContext db, Empresa empresa, CancellationToken cancellationToken = default)
    {
        var config = await db.AppConfigs.IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.EmpresaId == empresa.Id && c.Clave == CompanyCodeShortKey, cancellationToken);

        if (!string.IsNullOrWhiteSpace(config?.Valor))
            return NormalizeSegment(config.Valor);

        var companyCode = EsCodigoEmpresaCorto(empresa.Codigo)
            ? NormalizeSegment(empresa.Codigo)
            : await GenerateUniqueCompanyCodeAsync(db, empresa.NombreComercial, empresa.RazonSocial, empresa.Id, cancellationToken);

        if (config == null)
        {
            db.AppConfigs.Add(new AppConfig
            {
                EmpresaId = empresa.Id,
                Clave = CompanyCodeShortKey,
                Valor = companyCode,
                Descripcion = "Código corto operativo de la empresa"
            });
        }
        else
        {
            config.Valor = companyCode;
            config.Descripcion = "Código corto operativo de la empresa";
            config.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
        return companyCode;
    }

    public async Task<string> GenerateUniqueCompanyCodeAsync(CrmDbContext db, string? nombreComercial, string razonSocial, Guid? empresaIdExcluida = null, CancellationToken cancellationToken = default)
    {
        var existentes = await ObtenerCodigosEmpresaAsync(db, empresaIdExcluida, cancellationToken);
        var baseCode = BuildCompanyCodeBase(nombreComercial, razonSocial);
        return EnsureUnique(baseCode, existentes);
    }

    public async Task<string> GetNextEntityCodeAsync(Guid empresaId, string entityPrefix, string counterKey, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var companyCode = await GetCompanyShortCodeAsync(empresaId, cancellationToken);
        var sequence = await GetNextSequenceAsync(db, empresaId, counterKey, cancellationToken);
        return $"{NormalizeSegment(entityPrefix)}-{companyCode}-{sequence:D4}";
    }

    public async Task<(string Codigo, string NumeroEmpleado)> GetNextEmployeeIdentifiersAsync(Guid empresaId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var companyCode = await GetCompanyShortCodeAsync(empresaId, cancellationToken);
        var sequence = await GetNextSequenceAsync(db, empresaId, "Consecutivo:Empleado", cancellationToken);
        return ($"EMP-{companyCode}-{sequence:D4}", $"{companyCode}-{sequence:D4}");
    }

    public async Task<string> GetNextPeriodFolioAsync(Guid empresaId, string prefix, string counterKey, DateTime periodDate, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var companyCode = await GetCompanyShortCodeAsync(empresaId, cancellationToken);
        var sequence = await GetNextSequenceAsync(db, empresaId, counterKey, cancellationToken);
        return $"{NormalizeSegment(prefix)}-{companyCode}-{periodDate:yyyyMM}-{sequence:D3}";
    }

    private static bool EsCodigoEmpresaCorto(string? codigo)
    {
        var normalized = NormalizeSegment(codigo);
        return !string.IsNullOrWhiteSpace(normalized) && normalized.Length is >= 3 and <= 6;
    }

    private static string BuildCompanyCodeBase(string? nombreComercial, string razonSocial)
    {
        var source = string.IsNullOrWhiteSpace(nombreComercial) ? razonSocial : nombreComercial;
        var tokens = Tokenize(source).ToList();

        var initials = new string(tokens
            .Where(t => !StopWords.Contains(t))
            .Take(6)
            .Select(t => t[0])
            .ToArray());

        var candidate = NormalizeSegment(initials);
        if (candidate.Length >= 3)
            return candidate[..Math.Min(candidate.Length, 6)];

        var condensed = NormalizeSegment(string.Concat(tokens));
        if (condensed.Length >= 3)
            return condensed[..Math.Min(condensed.Length, 6)];

        return NormalizeSegment((condensed + "ZEN")[..3]);
    }

    private static IEnumerable<string> Tokenize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return [];

        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category == UnicodeCategory.NonSpacingMark)
                continue;

            builder.Append(char.IsLetterOrDigit(ch) ? char.ToUpperInvariant(ch) : ' ');
        }

        return builder.ToString()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(token => token.Length > 0);
    }

    private static string EnsureUnique(string baseCode, HashSet<string> existing)
    {
        var candidate = baseCode;
        var suffix = 2;
        while (existing.Contains(candidate))
        {
            var suffixText = suffix.ToString(CultureInfo.InvariantCulture);
            var trunk = baseCode[..Math.Min(baseCode.Length, Math.Max(1, 6 - suffixText.Length))];
            candidate = $"{trunk}{suffixText}";
            suffix++;
        }

        return candidate;
    }

    private async Task<HashSet<string>> ObtenerCodigosEmpresaAsync(CrmDbContext db, Guid? empresaIdExcluida, CancellationToken cancellationToken)
    {
        var codigosEmpresas = await db.Empresas.IgnoreQueryFilters()
            .Where(e => !empresaIdExcluida.HasValue || e.Id != empresaIdExcluida.Value)
            .Select(e => e.Codigo)
            .ToListAsync(cancellationToken);

        var codigosCortos = await db.AppConfigs.IgnoreQueryFilters()
            .Where(c => c.Clave == CompanyCodeShortKey && (!empresaIdExcluida.HasValue || c.EmpresaId != empresaIdExcluida.Value))
            .Select(c => c.Valor)
            .ToListAsync(cancellationToken);

        return codigosEmpresas
            .Concat(codigosCortos)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(NormalizeSegment)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static string NormalizeSegment(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();
        foreach (var ch in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(ch))
                builder.Append(char.ToUpperInvariant(ch));
        }

        return builder.ToString();
    }

    private async Task<int> GetNextSequenceAsync(CrmDbContext db, Guid empresaId, string counterKey, CancellationToken cancellationToken)
    {
        var config = await db.AppConfigs.IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.EmpresaId == empresaId && c.Clave == counterKey, cancellationToken);

        var next = 1;
        if (config == null)
        {
            db.AppConfigs.Add(new AppConfig
            {
                EmpresaId = empresaId,
                Clave = counterKey,
                Valor = next.ToString(CultureInfo.InvariantCulture),
                Descripcion = $"Consecutivo para {counterKey}"
            });
        }
        else
        {
            _ = int.TryParse(config.Valor, NumberStyles.Integer, CultureInfo.InvariantCulture, out var current);
            next = current + 1;
            config.Valor = next.ToString(CultureInfo.InvariantCulture);
            config.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
        return next;
    }

    private static readonly HashSet<string> StopWords =
    [
        "DE", "DEL", "LA", "LAS", "LOS", "SA", "CV", "SAPI", "RL", "SC", "THE", "Y", "E", "EN"
    ];
}
