namespace MundoVs.Core.Entities;

public enum EstatusPrenomina
{
    Abierta = 1,
    Cerrada = 2,
    Aplicada = 3,
    Cancelada = 4
}

public class Prenomina : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string? Folio { get; set; }
    public PeriodicidadPago PeriodicidadPago { get; set; } = PeriodicidadPago.Semanal;
    public int AnioPeriodo { get; set; }
    public int NumeroPeriodo { get; set; }
    public string Periodo { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public EstatusPrenomina Estatus { get; set; } = EstatusPrenomina.Abierta;
    public string? Notas { get; set; }

    public string? SnapshotConfiguracionJson { get; set; }
    public DateTime? FechaCierre { get; set; }
    public string? CerradaPor { get; set; }

    public ICollection<PrenominaDetalle> Detalles { get; set; } = [];
}