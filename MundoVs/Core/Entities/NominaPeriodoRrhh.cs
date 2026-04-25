namespace MundoVs.Core.Entities;

public class NominaCorteRrhh : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public PeriodicidadPago PeriodicidadPago { get; set; } = PeriodicidadPago.Semanal;
    public DayOfWeek DiaCorteSemana { get; set; } = DayOfWeek.Sunday;
    public int DiaCorteMes { get; set; } = 15;
    public DayOfWeek DiaPagoSugerido { get; set; } = DayOfWeek.Friday;
}

public class FestivoRrhh : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public DateTime Fecha { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public FestivoTipoRrhh Tipo { get; set; } = FestivoTipoRrhh.Oficial;
    public bool AplicaPrimaEspecial { get; set; } = true;
    public decimal FactorPago { get; set; } = 2m;
    public bool EsOficial { get; set; } = true;
}

public enum FestivoTipoRrhh
{
    Oficial = 1,
    Empresa = 2,
    Electoral = 3,
    Regional = 4
}
