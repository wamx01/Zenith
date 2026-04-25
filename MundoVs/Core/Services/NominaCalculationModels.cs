using MundoVs.Core.Entities;

namespace MundoVs.Core.Services;

public class NominaCalculationInput
{
    public required Empleado Empleado { get; init; }
    public required NominaConfiguracion Configuracion { get; init; }
    public decimal SueldoReferencia { get; init; }
    public decimal? SueldoBaseOverride { get; init; }
    public int DiasPagados { get; init; }
    public int DiasVacaciones { get; init; }
    public int DiasDescansoTrabajado { get; init; }
    public int DiasDomingoTrabajado { get; init; }
    public int DiasFestivoTrabajado { get; init; }
    public int MinutosDescuento { get; init; }
    public bool AplicaImss { get; init; }
    public bool AplicaIsr { get; init; } = true;
    public decimal HorasExtraBase { get; init; }
    public decimal HorasExtraDobles { get; init; }
    public decimal HorasExtraTriples { get; init; }
    public decimal HorasExtraBanco { get; init; }
    public decimal HorasExtra { get; init; }
    public decimal MontoDestajo { get; init; }
    public decimal MontoBono { get; init; }
    public decimal MontoPercepcionesManuales { get; init; }
    public decimal MontoDeducciones { get; init; }
    public decimal ComplementoSalarioMinimo { get; init; }
    public decimal FactorFestivo { get; init; } = 2m;
    public int AniosServicio { get; init; }
    public bool AplicaSalarioMinimoFrontera { get; init; }
}

public class NominaCalculationResult
{
    public decimal SueldoBase { get; init; }
    public decimal SueldoDiario { get; init; }
    public decimal MontoDestajo { get; init; }
    public decimal MontoBono { get; init; }
    public decimal MontoFestivoTrabajado { get; init; }
    public decimal MontoDescansoTrabajado { get; init; }
    public decimal MontoPrimaDominical { get; init; }
    public decimal MontoPrimaVacacional { get; init; }
    public decimal ComplementoSalarioMinimo { get; init; }
    public decimal HorasExtraBase { get; init; }
    public decimal HorasExtraDobles { get; init; }
    public decimal HorasExtraTriples { get; init; }
    public decimal HorasExtraBanco { get; init; }
    public decimal MontoHorasExtra { get; init; }
    public decimal MontoPercepcionesManuales { get; init; }
    public decimal MontoDeducciones { get; init; }
    public decimal MontoDescuentoMinutos { get; init; }
    public decimal CuotaImssObrera { get; init; }
    public decimal CuotaImssPatronal { get; init; }
    public decimal MontoInfonavit { get; init; }
    public decimal RetencionIsr { get; init; }
    public decimal SubsidioEmpleo { get; init; }
    public decimal AguinaldoProvision { get; init; }
    public decimal PrimaVacacionalProvision { get; init; }
    public decimal TotalPagar { get; init; }
}
