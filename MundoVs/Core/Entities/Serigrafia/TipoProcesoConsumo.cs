namespace MundoVs.Core.Entities.Serigrafia;

public class TipoProcesoConsumo : BaseEntity
{
    public Guid TipoProcesoId { get; set; }
    public TipoProceso TipoProceso { get; set; } = null!;
    public TipoProcesoConsumoOrigen Origen { get; set; }
    public CotizacionCategoria CategoriaCotizacion { get; set; }
    public Guid? InventarioItemId { get; set; }
    public Inventario.InventarioItem? InventarioItem { get; set; }
    public decimal CantidadBase { get; set; }
    public int Orden { get; set; }
    public bool Activo { get; set; } = true;
    public string? Observaciones { get; set; }
}

public enum TipoProcesoConsumoOrigen
{
    MateriaPrima = 1,
    Insumo = 2
}
