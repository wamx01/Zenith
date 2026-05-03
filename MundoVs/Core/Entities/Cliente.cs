using MundoVs.Core.Entities.Calzado;

namespace MundoVs.Core.Entities;

public class Cliente : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string Codigo { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreComercial { get; set; }
    public string RfcCif { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? EmailFacturacion { get; set; }
    public string? EmailCobranza { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? Estado { get; set; }
    public string? CodigoPostal { get; set; }
    public string? Pais { get; set; }
    public string? DomicilioFiscalCp { get; set; }
    public string? RegimenFiscalReceptor { get; set; }
    public string? UsoCfdi { get; set; }
    public bool RfcValidado { get; set; }
    public DateTime? FechaValidacionRfc { get; set; }
    public int? DiasCredito { get; set; }
    public decimal? LimiteCredito { get; set; }
    
    public IndustriaEnum Industria { get; set; }
    public string? IndustriaPersonalizada { get; set; }
    
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    public ICollection<Contacto> Contactos { get; set; } = new List<Contacto>();
    public ICollection<ProductoCliente> Productos { get; set; } = new List<ProductoCliente>();
    public ICollection<ClienteReglaVariacionPrecio> ReglasVariacionPrecio { get; set; } = new List<ClienteReglaVariacionPrecio>();
    public ICollection<ClienteTallaCalzado> TallasCalzadoConfiguradas { get; set; } = new List<ClienteTallaCalzado>();
    public ICollection<ClienteFraccionCalzado> FraccionesCalzado { get; set; } = new List<ClienteFraccionCalzado>();
    public ICollection<NotaEntrega> NotasEntrega { get; set; } = new List<NotaEntrega>();
    public ICollection<Factura> Facturas { get; set; } = new List<Factura>();
    public ICollection<PagoRecibido> PagosRecibidos { get; set; } = new List<PagoRecibido>();
    public ICollection<CargoManualCxC> CargosManualesCxC { get; set; } = new List<CargoManualCxC>();
    public ICollection<ClienteDatoFiscalSnapshot> DatosFiscalesSnapshot { get; set; } = new List<ClienteDatoFiscalSnapshot>();
}

public enum IndustriaEnum
{
    Calzado = 1,
    Serigrafia = 2,
    Ambas = 3
}
