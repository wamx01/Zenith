using MundoVs.Core.Entities;

namespace MundoVs.Core.Services;

public class NominaResumenInput
{
    public required IReadOnlyCollection<NominaDetalle> Detalles { get; init; }
    public required NominaConfiguracion Configuracion { get; init; }
    public DateTime FechaReferencia { get; init; }
}

public class NominaResumenResult
{
    public IReadOnlyDictionary<Guid, NominaDetalleResumen> Detalles { get; init; } = new Dictionary<Guid, NominaDetalleResumen>();
    public decimal TotalSueldoBase { get; init; }
    public decimal TotalDestajo { get; init; }
    public decimal TotalBono { get; init; }
    public decimal TotalFestivoTrabajado { get; init; }
    public decimal TotalDescansoTrabajado { get; init; }
    public decimal TotalPrimaDominical { get; init; }
    public decimal TotalPrimaVacacional { get; init; }
    public decimal TotalComplementoSalarioMinimo { get; init; }
    public decimal TotalHorasExtraDobles { get; init; }
    public decimal TotalHorasExtraTriples { get; init; }
    public decimal TotalHorasExtraBanco { get; init; }
    public decimal TotalHorasExtra { get; init; }
    public decimal TotalPercepcionesManuales { get; init; }
    public decimal TotalImssObrero { get; init; }
    public decimal TotalIsr { get; init; }
    public decimal TotalImssPatronal { get; init; }
    public decimal TotalObligacionesTerceros { get; init; }
    public decimal TotalAportacionesPatronales { get; init; }
    public decimal TotalProvisiones { get; init; }
    public decimal TotalCostoEmpresa { get; init; }
    public decimal TotalInfonavit { get; init; }
    public decimal TotalDeducciones { get; init; }
    public decimal TotalDescuentoMinutos { get; init; }
    public decimal TotalNeto { get; init; }
}

public class NominaDetalleResumen
{
    public required Guid NominaDetalleId { get; init; }
    public decimal HorasExtraDoblesPreview { get; init; }
    public decimal HorasExtraTriplesPreview { get; init; }
    public decimal MontoHorasExtraPreview { get; init; }
    public decimal TotalPagarPreview { get; init; }
}
