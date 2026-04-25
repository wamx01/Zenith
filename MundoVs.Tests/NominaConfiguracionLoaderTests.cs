using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Tests;

public sealed class NominaConfiguracionLoaderTests
{
    [Fact]
    public async Task LoadGlobalAsync_PrefiereConfiguracionActivaMasVigente()
    {
        await using var db = CreateDbContext();

        db.NominaConfiguracionesGlobales.AddRange(
            new NominaConfiguracionGlobal
            {
                Id = Guid.NewGuid(),
                UmaDiaria = 115m,
                SalarioMinimoGeneral = 280m,
                SalarioMinimoFrontera = 420m,
                TablaIsrJson = "old",
                TablaSubsidioJson = "old",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true
            },
            new NominaConfiguracionGlobal
            {
                Id = Guid.NewGuid(),
                UmaDiaria = 120m,
                SalarioMinimoGeneral = 285m,
                SalarioMinimoFrontera = 425m,
                TablaIsrJson = "newer-created",
                TablaSubsidioJson = "newer-created",
                CreatedAt = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true
            },
            new NominaConfiguracionGlobal
            {
                Id = Guid.NewGuid(),
                UmaDiaria = 130m,
                SalarioMinimoGeneral = 290m,
                SalarioMinimoFrontera = 430m,
                TablaIsrJson = "inactive",
                TablaSubsidioJson = "inactive",
                CreatedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                IsActive = false
            });

        await db.SaveChangesAsync();

        var result = await NominaConfiguracionLoader.LoadGlobalAsync(db);

        Assert.NotNull(result);
        Assert.Equal(115m, result.UmaDiaria);
        Assert.Equal("old", result.TablaIsrJson);
    }

    private static CrmDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new CrmDbContext(options);
    }
}
