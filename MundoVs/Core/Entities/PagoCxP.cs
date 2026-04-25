namespace MundoVs.Core.Entities;

public enum MetodoPagoCxP
{
    Efectivo = 1,
    Transferencia = 2,
    Cheque = 3,
    TarjetaCredito = 4,
    Otro = 5
}

public class PagoCxP
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CuentaPorPagarId { get; set; }
    public CuentaPorPagar CuentaPorPagar { get; set; } = null!;

    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; }
    public MetodoPagoCxP MetodoPago { get; set; } = MetodoPagoCxP.Efectivo;
    public string? Referencia { get; set; }
    public string? Notas { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
