namespace MundoVs.Core.Entities;

public class PrenominaDetalle : BaseEntity
{
    public Guid PrenominaId { get; set; }
    public Prenomina Prenomina { get; set; } = null!;

    public Guid EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public int DiasTrabajados { get; set; }
    public int DiasPagados { get; set; }
    public int DiasVacaciones { get; set; }
    public int DiasFaltaJustificada { get; set; }
    public int DiasFaltaInjustificada { get; set; }
    public int DiasIncapacidad { get; set; }
    public int DiasDescansoTrabajado { get; set; }
    public int DiasDomingoTrabajado { get; set; }
    public int DiasFestivoTrabajado { get; set; }
    public bool AplicaImss { get; set; }

    public decimal HorasTrabajadasNetas { get; set; }
    public decimal HorasExtraBase { get; set; }
    public decimal HorasExtra { get; set; }
    public decimal HorasBancoAcumuladas { get; set; }
    public decimal HorasBancoConsumidas { get; set; }
    public decimal HorasDescansoTomado { get; set; }
    public decimal HorasDescansoPagado { get; set; }
    public decimal HorasDescansoNoPagado { get; set; }
    public int DiasConMarcacion { get; set; }
    public int MinutosRetardo { get; set; }
    public int MinutosSalidaAnticipada { get; set; }
    public int MinutosPerdonadosManual { get; set; }
    public int MinutosFaltanteDescontable { get; set; }
    public int MinutosDescuentoManual { get; set; }
    public decimal FactorPagoTiempoExtra { get; set; }
    public decimal MontoDestajoInformativo { get; set; }
    public decimal DiasVacacionesDisponibles { get; set; }
    public decimal DiasVacacionesRestantes { get; set; }
    public decimal ComplementoSalarioMinimoSugerido { get; set; }
    public string? Notas { get; set; }

    public ICollection<PrenominaBono> BonosRapidos { get; set; } = [];
    public ICollection<PrenominaPercepcion> PercepcionesRapidas { get; set; } = [];
}