using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Services;

public sealed class NominaSatCatalogInitializer : INominaSatCatalogInitializer
{
    private readonly IDbContextFactory<CrmDbContext> _dbFactory;

    public NominaSatCatalogInitializer(IDbContextFactory<CrmDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await SeedPercepcionesAsync(db, cancellationToken);
        await SeedDeduccionesAsync(db, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedPercepcionesAsync(CrmDbContext db, CancellationToken cancellationToken)
    {
        var existentes = await db.NominasPercepcionesTipos
            .Where(x => x.IsActive)
            .ToDictionaryAsync(x => x.Clave, cancellationToken);

        foreach (var item in NominaSatCatalogos.TiposPercepcion.Values)
        {
            if (existentes.ContainsKey(item.Clave))
            {
                continue;
            }

            db.NominasPercepcionesTipos.Add(new NominaPercepcionTipo
            {
                Id = Guid.NewGuid(),
                Clave = item.Clave,
                Nombre = Limitar(item.Descripcion, 120),
                Categoria = ResolverCategoriaPercepcion(item.Clave),
                AfectaBaseImss = true,
                AfectaBaseIsr = true,
                Orden = int.TryParse(item.Clave, out var orden) ? orden : 999,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });
        }
    }

    private static async Task SeedDeduccionesAsync(CrmDbContext db, CancellationToken cancellationToken)
    {
        var existentes = await db.DeduccionesTiposRrhh
            .Where(x => x.IsActive)
            .ToDictionaryAsync(x => x.Clave, cancellationToken);

        foreach (var item in NominaSatCatalogos.TiposDeduccion.Values)
        {
            if (existentes.ContainsKey(item.Clave))
            {
                continue;
            }

            db.DeduccionesTiposRrhh.Add(new DeduccionTipoRrhh
            {
                Id = Guid.NewGuid(),
                Clave = item.Clave,
                Nombre = Limitar(item.Descripcion, 120),
                Descripcion = item.Descripcion.Length > 120 ? Limitar(item.Descripcion, 300) : null,
                Orden = int.TryParse(item.Clave, out var orden) ? orden : 999,
                EsLegal = EsDeduccionLegal(item.Clave),
                AfectaRecibo = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });
        }
    }

    private static CategoriaPercepcionNomina ResolverCategoriaPercepcion(string clave)
        => clave switch
        {
            "028" => CategoriaPercepcionNomina.Comision,
            "029" or "030" or "031" or "032" or "033" or "034" or "035" or "036" or "037" => CategoriaPercepcionNomina.BonoManual,
            _ => CategoriaPercepcionNomina.OtroIngreso
        };

    private static bool EsDeduccionLegal(string clave)
        => clave is "001" or "002" or "005" or "006" or "007" or "009" or "010" or "011" or "020" or "021" or "022";

    private static string Limitar(string valor, int maximo)
        => string.IsNullOrWhiteSpace(valor)
            ? string.Empty
            : valor.Length <= maximo
                ? valor.Trim()
                : valor[..maximo].Trim();
}
