namespace MundoVs.Core.Entities;

public class EmpleadoEsquemaPago : BaseEntity
{
    public Guid EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public Guid EsquemaPagoId { get; set; }
    public EsquemaPago EsquemaPago { get; set; } = null!;

    public decimal? SueldoBaseOverride { get; set; }
    public DateTime VigenteDesde { get; set; }
    public DateTime? VigenteHasta { get; set; }
}
