namespace MundoVs.Core.Entities;

public class NominaDetalle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid NominaId { get; set; }
    public Nomina Nomina { get; set; } = null!;

    public Guid EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public TipoNomina TipoPago { get; set; } = TipoNomina.Semanal;

    // Esquema de pago usado para este periodo
    public Guid? EsquemaPagoId { get; set; }
    public EsquemaPago? EsquemaPago { get; set; }

    // Sueldo semanal fijo
    public decimal SueldoBase { get; set; }

    // Destajo (legacy: cálculo plano)
    public int PiezasProducidas { get; set; }
    public decimal TarifaPorPieza { get; set; }
    public decimal MontoDestajoLegacy => PiezasProducidas * TarifaPorPieza;

    // Destajo (nuevo: desde vales)
    public int TotalPiezas { get; set; }
    public decimal MontoDestajo { get; set; }
    public decimal MontoBono { get; set; }

    // Incidencias desde prenómina
    public int DiasTrabajados { get; set; }
    public int DiasPagados { get; set; }
    public int DiasVacaciones { get; set; }
    public int DiasFaltaJustificada { get; set; }
    public int DiasFaltaInjustificada { get; set; }
    public int DiasIncapacidad { get; set; }
    public int DiasDescansoTrabajado { get; set; }
    public int DiasConMarcacion { get; set; }
    public int DiasDomingoTrabajado { get; set; }
    public int DiasFestivoTrabajado { get; set; }
    public decimal MontoFestivoTrabajado { get; set; }
    public decimal MontoDescansoTrabajado { get; set; }
    public decimal MontoPrimaDominical { get; set; }
    public decimal MontoPrimaVacacional { get; set; }
    public decimal ComplementoSalarioMinimo { get; set; }
    public bool AplicaImss { get; set; }
    public decimal CuotaImssObrera { get; set; }
    public decimal CuotaImssPatronal { get; set; }
    public decimal MontoInfonavit { get; set; }

    // Retención fiscal al trabajador (art. 96 LISR) y subsidio al empleo acreditado.
    public decimal RetencionIsr { get; set; }
    public decimal SubsidioEmpleo { get; set; }

    // Provisiones patronales (no se reflejan en neto del trabajador ni en el recibo).
    public decimal AguinaldoProvision { get; set; }
    public decimal PrimaVacacionalProvision { get; set; }

    // Extras
    public decimal HorasExtraBase { get; set; }
    public decimal HorasExtraDobles { get; set; }
    public decimal HorasExtraTriples { get; set; }
    public decimal HorasExtraBanco { get; set; }
    public decimal HorasExtra { get; set; }
    public decimal MontoHorasExtra { get; set; }
    public decimal Bonos { get; set; }
    public decimal Deducciones { get; set; }
    public int MinutosRetardo { get; set; }
    public int MinutosSalidaAnticipada { get; set; }
    public int MinutosDescuentoManual { get; set; }
    public decimal MontoDescuentoMinutos { get; set; }
    public string? ConceptoDeducciones { get; set; }

    public decimal TotalPagar => SueldoBase + MontoDestajo + MontoBono
                                 + MontoFestivoTrabajado + MontoDescansoTrabajado + MontoPrimaDominical + MontoPrimaVacacional + ComplementoSalarioMinimo
                                 + MontoHorasExtra + Bonos + SubsidioEmpleo
                                 - Deducciones - MontoDescuentoMinutos - CuotaImssObrera - MontoInfonavit - RetencionIsr;

    public string? Notas { get; set; }

    public ICollection<ValeDestajo> ValesDestajo { get; set; } = [];
    public ICollection<NominaBono> BonosEstructurados { get; set; } = [];
    public ICollection<NominaPercepcion> PercepcionesManuales { get; set; } = [];
    public ICollection<NominaDeduccion> DeduccionesEstructuradas { get; set; } = [];
}
