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

    // Fusión prenómina→nómina: la nómina asume el rol de artefacto congelado. Mientras
    // FechaCierreCaptura == null la nómina está en "Fase A" (captura editable desde el
    // snapshot de asistencias). Al "Cerrar periodo y calcular" se estampa esta fecha →
    // los campos de asistencia se congelan (read-only) y se corre el cálculo. Reabrir
    // la captura la limpia. Aprobada/Pagada bloquean todo (ver EstaBloqueadaNomina).
    // English: prenómina→nómina fusion — the nómina becomes the frozen artifact. While
    // FechaCierreCaptura == null the nómina is in "Phase A" (editable capture from the
    // attendance snapshot). "Close period and calculate" stamps this date → attendance
    // fields freeze (read-only) and calculation runs. Reopening capture clears it.
    // Aprobada/Pagada lock everything (see EstaBloqueadaNomina).
    public DateTime? FechaCierreCaptura { get; set; }
    public string? CerradaCapturaPor { get; set; }

    // Snapshot inmutable de la NominaConfiguracion congelada al cerrar la captura (misma
    // información que antes vivía en Prenomina.SnapshotConfiguracionJson). La sincronización
    // la deserializa para calcular con la config vigente al momento del cierre.
    // English: immutable snapshot of NominaConfiguracion frozen at capture-close time (same
    // data that previously lived in Prenomina.SnapshotConfiguracionJson). Synchronization
    // deserializes it to calculate with the config in effect at close time.
    public string? SnapshotConfiguracionJson { get; set; }

    public EstatusNomina Estatus { get; set; } = EstatusNomina.Borrador;
    public string? Notas { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public decimal TotalNomina => Detalles.Sum(d => d.TotalPagar);

    public ICollection<NominaDetalle> Detalles { get; set; } = [];
}
