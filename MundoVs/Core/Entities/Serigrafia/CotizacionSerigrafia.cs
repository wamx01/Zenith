namespace MundoVs.Core.Entities.Serigrafia;

public class CotizacionSerigrafia : BaseEntity
{
    public Guid ProductoId { get; set; }
    public Guid? ClienteId { get; set; }
    public string? Descripcion { get; set; }
    public int NumeroPersonas { get; set; }

    // Cálculos finales
    public int ParesPorLamina { get; set; }
    public decimal Utilidad { get; set; }
    public decimal PrecioSugerido { get; set; }
    public decimal PrecioFinalContado { get; set; }
    public decimal PrecioCredito { get; set; }
    public decimal Ganancia { get; set; }
    public decimal CostoTotalPorPar { get; set; }
    public decimal CostoTotalPorTarea { get; set; }

    // Totales calculados desde detalles
    public decimal TotalTintas => Detalles?.Where(d => d.Categoria == CotizacionCategoria.Tinta).Sum(d => d.Costo) ?? 0;
    public decimal TotalInsumosBasicos => Detalles?.Where(d => d.Categoria == CotizacionCategoria.InsumoBasico).Sum(d => d.Costo) ?? 0;
    public decimal TotalInsumosDiversos => Detalles?.Where(d => d.Categoria == CotizacionCategoria.InsumoDiverso).Sum(d => d.Costo) ?? 0;
    public decimal TotalManoObra => Detalles?.Where(d => d.Categoria == CotizacionCategoria.ManoObra).Sum(d => d.Costo) ?? 0;
    public decimal TotalGastosFijos => Detalles?.Where(d => d.Categoria == CotizacionCategoria.GastoFijo).Sum(d => d.Costo) ?? 0;
    public decimal CostoTotal => TotalTintas + TotalInsumosBasicos + TotalInsumosDiversos + TotalManoObra + TotalGastosFijos;

    // Navegación
    public Producto Producto { get; set; } = null!;
    public Cliente? Cliente { get; set; }
    public ICollection<CotizacionDetalle> Detalles { get; set; } = new List<CotizacionDetalle>();
    public ICollection<CotizacionSerigrafiaProceso> Procesos { get; set; } = new List<CotizacionSerigrafiaProceso>();
    public ICollection<CotizacionVariantePrecio> PreciosVariantes { get; set; } = new List<CotizacionVariantePrecio>();
}
