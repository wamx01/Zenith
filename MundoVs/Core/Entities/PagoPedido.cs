namespace MundoVs.Core.Entities;

public class PagoPedido : BaseEntity
{
    public Guid PedidoId { get; set; }
    public DateTime FechaPago { get; set; }
    public decimal Monto { get; set; }
    public string MetodoPagoSat { get; set; } = "PUE";
    public FormaPagoEnum FormaPago { get; set; } = FormaPagoEnum.Transferencia;
    public string? MedioCobroInterno { get; set; }
    public string? Referencia { get; set; }
    public string? Notas { get; set; }

    // Navegación
    public Pedido Pedido { get; set; } = null!;
}

/// <summary>
/// Catálogo simplificado basado en c_FormaPago del SAT (CFDI 4.0)
/// </summary>
public enum FormaPagoEnum
{
    Efectivo = 1,
    Cheque = 2,
    Transferencia = 3,
    TarjetaCredito = 4,
    TarjetaDebito = 28,
    Otro = 99
}
