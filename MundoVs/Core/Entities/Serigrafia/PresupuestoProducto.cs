namespace MundoVs.Core.Entities.Serigrafia;

public class PresupuestoProducto : BaseEntity
{
    public Guid ProductoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal SueldoSemanalBase { get; set; }

    // Totales calculados
    public decimal TotalMateriaPrima => Detalles?.Where(d => d.Categoria == PresupuestoCategoria.MateriaPrima).Sum(d => d.Costo) ?? 0;
    public decimal TotalInsumosBasicos => Detalles?.Where(d => d.Categoria == PresupuestoCategoria.InsumoBasico).Sum(d => d.Costo) ?? 0;
    public decimal TotalInsumosDiversos => Detalles?.Where(d => d.Categoria == PresupuestoCategoria.InsumoDiverso).Sum(d => d.Costo) ?? 0;
    public decimal TotalManoObra => Detalles?.Where(d => d.Categoria == PresupuestoCategoria.ManoObra).Sum(d => d.Costo) ?? 0;
    public decimal TotalGastosFijos => Detalles?.Where(d => d.Categoria == PresupuestoCategoria.GastoFijo).Sum(d => d.Costo) ?? 0;
    public decimal CostoTotal => TotalMateriaPrima + TotalInsumosBasicos + TotalInsumosDiversos + TotalManoObra + TotalGastosFijos;

    // Navegación
    public Producto Producto { get; set; } = null!;
    public ICollection<PresupuestoDetalle> Detalles { get; set; } = new List<PresupuestoDetalle>();
}

public class PresupuestoDetalle : BaseEntity
{
    public Guid PresupuestoProductoId { get; set; }
    public PresupuestoCategoria Categoria { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public decimal Consumo { get; set; }
    public decimal Costo { get; set; }

    // Para Mano de Obra
    public decimal? SueldoSugerido { get; set; }
    public decimal? TiempoCompleto { get; set; }
    public decimal? TiempoBasico { get; set; }

    public int Orden { get; set; }

    // Navegación
    public PresupuestoProducto PresupuestoProducto { get; set; } = null!;
}

public enum PresupuestoCategoria
{
    MateriaPrima = 1,
    InsumoBasico = 2,
    InsumoDiverso = 3,
    ManoObra = 4,
    GastoFijo = 5
}
