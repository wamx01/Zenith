namespace MundoVs.Core.Entities;

public class Producto : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string Codigo { get; set; } = string.Empty;
    public string? Referencia { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public IndustriaEnum Industria { get; set; }
    public decimal PrecioBase { get; set; }
    public string? UnidadMedida { get; set; }
    public string? Categoria { get; set; }
    public bool TieneVariantes { get; set; }
    public bool UsaVariantesTalla { get; set; }
    public bool UsaVariantesColor { get; set; }
    
    public ICollection<PedidoDetalle> PedidoDetalles { get; set; } = new List<PedidoDetalle>();
    public ICollection<ProductoCliente> Clientes { get; set; } = new List<ProductoCliente>();
    public ICollection<ProductoVariante> Variantes { get; set; } = new List<ProductoVariante>();
}
