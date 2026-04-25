namespace MundoVs.Core.Entities.Calzado;

public enum SistemaNumeracionCalzadoEnum
{
    MX = 1,
    EU = 2,
    US = 3,
    UK = 4
}

public class CatalogoTallaCalzado : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string Talla { get; set; } = string.Empty;
    public SistemaNumeracionCalzadoEnum SistemaNumeracion { get; set; } = SistemaNumeracionCalzadoEnum.MX;
    public int Orden { get; set; }
    public bool Activa { get; set; } = true;

    public ICollection<ClienteTallaCalzado> ClientesConfigurados { get; set; } = new List<ClienteTallaCalzado>();
    public ICollection<ClienteFraccionCalzadoDetalle> FraccionesDetalle { get; set; } = new List<ClienteFraccionCalzadoDetalle>();
}

public class ClienteTallaCalzado : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public Guid CatalogoTallaCalzadoId { get; set; }
    public CatalogoTallaCalzado CatalogoTallaCalzado { get; set; } = null!;

    public string Talla { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool EsTallaBaseDefault { get; set; }
    public decimal PorcentajeVariacionDefault { get; set; }
    public bool Activa { get; set; } = true;
}

public class ClienteFraccionCalzado : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal UnidadesPorFraccion { get; set; }
    public bool Activa { get; set; } = true;

    public ICollection<ClienteFraccionCalzadoDetalle> Detalles { get; set; } = new List<ClienteFraccionCalzadoDetalle>();
    public ICollection<ProductoCliente> ProductosClienteDefault { get; set; } = new List<ProductoCliente>();
    public ICollection<PedidoDetalle> PedidoDetalles { get; set; } = new List<PedidoDetalle>();
}

public class ClienteFraccionCalzadoDetalle : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid ClienteFraccionCalzadoId { get; set; }
    public ClienteFraccionCalzado ClienteFraccionCalzado { get; set; } = null!;

    public Guid? ClienteTallaCalzadoId { get; set; }
    public ClienteTallaCalzado? ClienteTallaCalzado { get; set; }

    public Guid? CatalogoTallaCalzadoId { get; set; }
    public CatalogoTallaCalzado? CatalogoTallaCalzado { get; set; }

    public string Talla { get; set; } = string.Empty;
    public int Orden { get; set; }
    public decimal Unidades { get; set; }
    public decimal PorcentajeVariacion { get; set; }
}
