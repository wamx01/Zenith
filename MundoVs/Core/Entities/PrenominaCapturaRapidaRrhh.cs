namespace MundoVs.Core.Entities;

public class PrenominaBono : BaseEntity
{
    public Guid PrenominaDetalleId { get; set; }
    public PrenominaDetalle PrenominaDetalle { get; set; } = null!;

    public Guid BonoRubroRrhhId { get; set; }
    public BonoRubroRrhh BonoRubroRrhh { get; set; } = null!;

    public decimal Importe { get; set; }
    public string? Observaciones { get; set; }
}

public class PrenominaPercepcion : BaseEntity
{
    public Guid PrenominaDetalleId { get; set; }
    public PrenominaDetalle PrenominaDetalle { get; set; } = null!;

    public Guid TipoPercepcionId { get; set; }
    public NominaPercepcionTipo TipoPercepcion { get; set; } = null!;

    public decimal Importe { get; set; }
    public string? Referencia { get; set; }
    public string? Observaciones { get; set; }
}
