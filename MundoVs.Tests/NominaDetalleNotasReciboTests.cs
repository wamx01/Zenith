using MundoVs.Core.Entities;

namespace MundoVs.Tests;

/// <summary>
/// Precedencia de <see cref="NominaDetalle.NotasRecibo"/>: el override manual
/// (<see cref="NominaDetalle.NotasManual"/>) gana sobre el autogenerado (<see cref="NominaDetalle.Notas"/>)
/// cuando no está en blanco. Blank/null cae a la sugerencia automática.
/// English: NotasRecibo precedence — the manual override (NotasManual) wins over the
/// auto-generated (Notas) when non-blank; blank/null falls back to the auto suggestion.
/// </summary>
public sealed class NominaDetalleNotasReciboTests
{
    [Fact]
    public void NotasRecibo_usa_manual_cuando_no_esta_en_blanco()
    {
        var detalle = new NominaDetalle { Notas = "auto: 3 ausencias", NotasManual = "editado por el usuario" };

        Assert.Equal("editado por el usuario", detalle.NotasRecibo);
    }

    [Fact]
    public void NotasRecibo_cae_a_notas_cuando_manual_es_null()
    {
        var detalle = new NominaDetalle { Notas = "auto: 3 ausencias", NotasManual = null };

        Assert.Equal("auto: 3 ausencias", detalle.NotasRecibo);
    }

    [Fact]
    public void NotasRecibo_cae_a_notas_cuando_manual_es_solo_espacios()
    {
        var detalle = new NominaDetalle { Notas = "auto: 3 ausencias", NotasManual = "   " };

        Assert.Equal("auto: 3 ausencias", detalle.NotasRecibo);
    }

    [Fact]
    public void NotasRecibo_es_null_cuando_ambas_estan_vacias()
    {
        var detalle = new NominaDetalle { Notas = null, NotasManual = null };

        Assert.Null(detalle.NotasRecibo);
    }
}