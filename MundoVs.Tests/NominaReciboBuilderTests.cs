using MundoVs.Core.Entities;
using MundoVs.Core.Services;

namespace MundoVs.Tests;

public sealed class NominaReciboBuilderTests
{
    [Fact]
    public void Build_UsaConceptosSatAutomaticosParaConceptosBaseDelSistema()
    {
        var detalle = new NominaDetalle
        {
            Empleado = new Empleado
            {
                Id = Guid.NewGuid(),
                Nombre = "Juan",
                ApellidoPaterno = "Pérez"
            },
            Nomina = new Nomina
            {
                Empresa = new Empresa
                {
                    Id = Guid.NewGuid(),
                    RazonSocial = "Empresa Test"
                },
                FechaInicio = new DateTime(2026, 1, 1),
                FechaFin = new DateTime(2026, 1, 15)
            },
            SueldoBase = 3500m,
            MontoDestajo = 800m,
            HorasExtraBase = 1m,
            HorasExtraDobles = 1m,
            MontoHorasExtra = 250m,
            MontoDescuentoMinutos = 75m,
            RetencionIsr = 120m,
            CuotaImssObrera = 95m,
            MontoInfonavit = 60m
        };

        var recibo = new NominaReciboBuilder().Build(detalle);

        Assert.Contains(recibo.Percepciones, x => x.Codigo == NominaSatCatalogos.Sistema.PercepcionSueldos && x.Importe == 3500m);
        Assert.Contains(recibo.Percepciones, x => x.Codigo == NominaSatCatalogos.Sistema.PercepcionComisiones && x.Importe == 800m);
        Assert.Contains(recibo.Percepciones, x => x.Codigo == NominaSatCatalogos.Sistema.PercepcionHorasExtra && x.Importe == 250m);
        Assert.Contains(recibo.Percepciones, x => x.Codigo == NominaSatCatalogos.Sistema.PercepcionHorasExtra && x.Concepto.Contains("1 h base", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(recibo.Percepciones, x => x.Codigo == NominaSatCatalogos.Sistema.PercepcionHorasExtra && x.Concepto.Contains("1 h dobles", StringComparison.OrdinalIgnoreCase));

        Assert.Contains(recibo.Deducciones, x => x.Codigo == NominaSatCatalogos.Sistema.DeduccionIsr && x.Importe == 120m);
        Assert.Contains(recibo.Deducciones, x => x.Codigo == NominaSatCatalogos.Sistema.DeduccionSeguridadSocial && x.Importe == 95m);
        Assert.Contains(recibo.Deducciones, x => x.Codigo == NominaSatCatalogos.Sistema.DeduccionCreditoInfonavit && x.Importe == 60m);
        Assert.Contains(recibo.Deducciones, x => x.Codigo == NominaSatCatalogos.Sistema.DeduccionAusentismo && x.Importe == 75m);
    }
}
