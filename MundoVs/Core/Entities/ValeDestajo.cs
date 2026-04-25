namespace MundoVs.Core.Entities;

public class ValeDestajo : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string Folio { get; set; } = string.Empty;
    public Guid EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public DateTime Fecha { get; set; }
    public EstatusValeDestajo Estatus { get; set; } = EstatusValeDestajo.Borrador;

    public Guid? EsquemaPagoId { get; set; }
    public EsquemaPago? EsquemaPago { get; set; }

    public Guid? NominaDetalleId { get; set; }
    public NominaDetalle? NominaDetalle { get; set; }

    public string? Observaciones { get; set; }

    public int TotalPiezas => Detalles.Sum(d => d.Cantidad);
    public decimal TotalImporte => Detalles.Sum(d => d.Importe);

    public ICollection<ValeDestajoDetalle> Detalles { get; set; } = [];
}

public enum EstatusValeDestajo
{
    Borrador = 1,
    Aprobado = 2,
    EnNomina = 3,
    Pagado = 4,
    Cancelado = 5
}
