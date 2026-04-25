using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MundoVs.Core.Interfaces;
using MundoVs.Core.Security;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Services;

public class ModuloAccesoService : IModuloAccesoService
{
    private readonly IDbContextFactory<CrmDbContext> _contextFactory;
    private readonly IMemoryCache _cache;

    public ModuloAccesoService(IDbContextFactory<CrmDbContext> contextFactory, IMemoryCache cache)
    {
        _contextFactory = contextFactory;
        _cache = cache;
    }

    public async Task<HashSet<string>> ObtenerModulosHabilitadosAsync(Guid empresaId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"modulos-empresa:{empresaId}";
        if (_cache.TryGetValue<HashSet<string>>(cacheKey, out var cached) && cached is not null)
            return new HashSet<string>(cached, StringComparer.OrdinalIgnoreCase);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var modulosGlobales = await context.ModulosAcceso
            .IgnoreQueryFilters()
            .Where(m => m.IsActive && m.EsGlobal)
            .Select(m => m.Clave)
            .ToListAsync(cancellationToken);

        var modulos = modulosGlobales;

        if (empresaId != Guid.Empty)
        {
            var hoy = DateTime.UtcNow.Date;
            var modulosEmpresa = await context.EmpresasModulosAcceso
                .IgnoreQueryFilters()
                .Where(m => m.EmpresaId == empresaId
                    && m.IsActive
                    && m.Habilitado
                    && m.VigenteDesde <= hoy
                    && (!m.VigenteHasta.HasValue || m.VigenteHasta.Value >= hoy)
                    && m.ModuloAcceso.IsActive)
                .Select(m => m.ModuloAcceso.Clave)
                .ToListAsync(cancellationToken);

            modulos.AddRange(modulosEmpresa);
            modulos.Add(ModuloAccesoCatalog.Administracion);
        }

        var result = modulos
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var resultSet = result.ToHashSet(StringComparer.OrdinalIgnoreCase);
        _cache.Set(cacheKey, resultSet, TimeSpan.FromMinutes(5));
        return new HashSet<string>(resultSet, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<bool> TieneModuloAsync(Guid empresaId, string moduloClave, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(moduloClave))
            return false;

        var modulos = await ObtenerModulosHabilitadosAsync(empresaId, cancellationToken);
        return modulos.Contains(moduloClave);
    }

    public async Task<List<string>> FiltrarCapacidadesAsync(Guid empresaId, IEnumerable<string> capacidades, bool esSuperAdmin, CancellationToken cancellationToken = default)
    {
        var capacidadesSolicitadas = capacidades
            .Where(cap => !string.IsNullOrWhiteSpace(cap))
            .Select(cap => cap.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (capacidadesSolicitadas.Count == 0)
            return [];

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var query = context.Capacidades
            .IgnoreQueryFilters()
            .Where(c => capacidadesSolicitadas.Contains(c.Clave) && c.ModuloAcceso.IsActive);

        if (esSuperAdmin)
        {
            query = query.Where(c => c.ModuloAcceso.EsGlobal);
        }
        else
        {
            var modulosHabilitados = await ObtenerModulosHabilitadosAsync(empresaId, cancellationToken);
            query = query.Where(c => modulosHabilitados.Contains(c.ModuloAcceso.Clave));
        }

        return await query
            .Select(c => c.Clave)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
