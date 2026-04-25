namespace MundoVs.Core.Entities;

public enum TipoDocumentoCxP
{
    Factura = 1,
    Nota = 2
}

public enum EstatusCxP
{
    Pendiente = 1,
    ParcialmentePagada = 2,
    Pagada = 3,
    Cancelada = 4
}

public class CuentaPorPagar
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid ProveedorId { get; set; }
    public Proveedor Proveedor { get; set; } = null!;

    public TipoDocumentoCxP TipoDocumento { get; set; } = TipoDocumentoCxP.Factura;
    public string NumeroDocumento { get; set; } = string.Empty;
    public string? Concepto { get; set; }
    public DateTime FechaEmision { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Impuestos { get; set; }
    public decimal Total { get; set; }
    public EstatusCxP Estatus { get; set; } = EstatusCxP.Pendiente;
    public string? Notas { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public decimal TotalPagado => Pagos.Sum(p => p.Monto);
    public decimal Saldo => Total - TotalPagado;

    public ICollection<PagoCxP> Pagos { get; set; } = [];
}
