namespace MundoVs.Core.Entities;

public class Proveedor : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? RazonSocial { get; set; }
    public string? Rfc { get; set; }
    public string? RegimenFiscal { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? EmailFiscal { get; set; }
    public string? EmailOrdenesCompra { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? Estado { get; set; }
    public string? CodigoPostal { get; set; }
    public string? Pais { get; set; }
    public string? Contacto { get; set; }
    public string? Banco { get; set; }
    public string? CuentaBancaria { get; set; }
    public string? Clabe { get; set; }
    public string? TitularCuenta { get; set; }
    public int? DiasPago { get; set; }
    public bool EsCliente { get; set; }
    public string? Notas { get; set; }

    public ICollection<CuentaPorPagar> CuentasPorPagar { get; set; } = [];
}
