namespace MundoVs.Core.Entities;

public class PagoSuscripcion : BaseEntity
{
    public Guid SuscripcionEmpresaId { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; } = DateTime.UtcNow;
    public MetodoPagoSuscripcion MetodoPago { get; set; } = MetodoPagoSuscripcion.Transferencia;
    public string? Referencia { get; set; }
    public string? Notas { get; set; }

    public SuscripcionEmpresa SuscripcionEmpresa { get; set; } = null!;
}

public enum MetodoPagoSuscripcion
{
    Transferencia = 1,
    Tarjeta = 2,
    Efectivo = 3,
    Otro = 4
}
