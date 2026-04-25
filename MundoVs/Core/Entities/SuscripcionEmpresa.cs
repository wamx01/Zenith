namespace MundoVs.Core.Entities;

public class SuscripcionEmpresa : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Guid PlanId { get; set; }
    public DateTime FechaInicio { get; set; } = DateTime.UtcNow.Date;
    public DateTime? FechaFin { get; set; }
    public EstadoSuscripcion Estado { get; set; } = EstadoSuscripcion.Trial;
    public PeriodicidadSuscripcion Periodicidad { get; set; } = PeriodicidadSuscripcion.Mensual;
    public bool RenovacionAutomatica { get; set; }

    public Empresa Empresa { get; set; } = null!;
    public Plan Plan { get; set; } = null!;
    public ICollection<PagoSuscripcion> Pagos { get; set; } = new List<PagoSuscripcion>();
}

public enum EstadoSuscripcion
{
    Trial = 0,
    Activa = 1,
    Suspendida = 2,
    Vencida = 3,
    Cancelada = 4
}

public enum PeriodicidadSuscripcion
{
    Mensual = 1,
    Anual = 2
}
