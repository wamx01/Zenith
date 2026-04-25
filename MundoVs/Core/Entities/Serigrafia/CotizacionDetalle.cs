namespace MundoVs.Core.Entities.Serigrafia;

public class CotizacionDetalle : BaseEntity
{
    public Guid CotizacionSerigrafiaId { get; set; }
    public CotizacionCategoria Categoria { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public int Orden { get; set; }

    // Campos compartidos
    public decimal Consumo { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Costo { get; set; }

    // Tintas
    public decimal? PesoInicial { get; set; }
    public decimal? PesoFinal { get; set; }

    // Mano de Obra
    public decimal? Tiempo { get; set; }
    public decimal? Precio { get; set; }
    public decimal? GananciaPorcentaje { get; set; }

    // FK opcionales al catálogo
    public Guid? MateriaPrimaId { get; set; }
    public Guid? InsumoId { get; set; }
    public Guid? PosicionId { get; set; }
    public Guid? TipoProcesoId { get; set; }
    public Guid? GastoFijoId { get; set; }

    // Navegación
    public CotizacionSerigrafia CotizacionSerigrafia { get; set; } = null!;
    public MateriaPrima? MateriaPrima { get; set; }
    public Insumo? Insumo { get; set; }
    public Posicion? Posicion { get; set; }
    public TipoProceso? TipoProceso { get; set; }
    public GastoFijo? GastoFijo { get; set; }
}

public enum CotizacionCategoria
{
    Tinta = 1,
    InsumoBasico = 2,
    InsumoDiverso = 3,
    ManoObra = 4,
    GastoFijo = 5
}
