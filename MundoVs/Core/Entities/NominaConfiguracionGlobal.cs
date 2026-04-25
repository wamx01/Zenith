namespace MundoVs.Core.Entities;

public class NominaConfiguracionGlobal : BaseEntity
{
    public decimal UmaDiaria { get; set; } = 113.14m;
    public decimal SalarioMinimoGeneral { get; set; } = 278.80m;
    public decimal SalarioMinimoFrontera { get; set; } = 419.88m;
    public string TablaIsrJson { get; set; } = NominaConfiguracion.TablaIsrDefaultJson;
    public string TablaSubsidioJson { get; set; } = NominaConfiguracion.TablaSubsidioDefaultJson;
}
