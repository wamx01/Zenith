namespace MundoVs.Core.Entities.Serigrafia;

public class CotizacionVariantePrecio : BaseEntity
{
    public Guid CotizacionSerigrafiaId { get; set; }
    public Guid? ProductoVarianteId { get; set; }
    public string Etiqueta { get; set; } = string.Empty;
    public string? Talla { get; set; }
    public string? Color { get; set; }
    public bool EsPrecioBase { get; set; }
    public decimal PorcentajeVariacionAplicado { get; set; }
    public decimal PrecioContado { get; set; }
    public decimal PrecioCredito { get; set; }
    public bool Aceptada { get; set; } = true;

    public CotizacionSerigrafia CotizacionSerigrafia { get; set; } = null!;
    public ProductoVariante? ProductoVariante { get; set; }
}
