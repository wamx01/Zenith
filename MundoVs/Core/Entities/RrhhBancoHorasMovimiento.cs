namespace MundoVs.Core.Entities;

public class RrhhBancoHorasMovimiento : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public DateOnly Fecha { get; set; }
    public TipoMovimientoBancoHorasRrhh TipoMovimiento { get; set; }
    public decimal Horas { get; set; }
    public Guid? NominaDetalleId { get; set; }
    public NominaDetalle? NominaDetalle { get; set; }
    public string? ReferenciaTipo { get; set; }
    public string? Observaciones { get; set; }
    public bool EsAutomatico { get; set; }
}

public enum TipoMovimientoBancoHorasRrhh
{
    GeneradoPorHorasExtra = 1,
    AjusteManual = 2,
    Consumo = 3
}
