namespace MundoVs.Core.Entities;

public enum EstatusNomina
{
    Borrador = 1,
    Aprobada = 2,
    Pagada = 3,
    Cancelada = 4
}

public class Nomina
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string? Folio { get; set; }
    public string? NumeroNomina { get; set; }
    public PeriodicidadPago PeriodicidadPago { get; set; } = PeriodicidadPago.Semanal;
    public int AnioPeriodo { get; set; }
    public int NumeroPeriodo { get; set; }
    public string Periodo { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public DateTime? FechaPago { get; set; }
    public Guid? PrenominaId { get; set; }
    public Prenomina? Prenomina { get; set; }
    public EstatusNomina Estatus { get; set; } = EstatusNomina.Borrador;
    public string? Notas { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public decimal TotalNomina => Detalles.Sum(d => d.TotalPagar);

    public ICollection<NominaDetalle> Detalles { get; set; } = [];
}
