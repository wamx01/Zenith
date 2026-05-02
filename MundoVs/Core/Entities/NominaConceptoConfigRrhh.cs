namespace MundoVs.Core.Entities;

public enum NaturalezaConceptoNominaRrhh
{
    Deduccion = 1,
    Obligacion = 2,
    Provision = 3,
    Ajuste = 4,
    Percepcion = 5
}

public enum DestinoConceptoNominaRrhh
{
    Empleado = 1,
    Sat = 2,
    Imss = 3,
    Reserva = 4,
    OtroTercero = 5
}

public enum TipoCalculoConceptoNominaRrhh
{
    MontoFijo = 1,
    Porcentaje = 2,
    CantidadPorTarifa = 3,
    Formula = 4,
    Manual = 5
}

public class NominaConceptoConfigRrhh : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string Clave { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public NaturalezaConceptoNominaRrhh Naturaleza { get; set; } = NaturalezaConceptoNominaRrhh.Deduccion;
    public DestinoConceptoNominaRrhh Destino { get; set; } = DestinoConceptoNominaRrhh.Empleado;
    public TipoCalculoConceptoNominaRrhh TipoCalculo { get; set; } = TipoCalculoConceptoNominaRrhh.MontoFijo;
    public decimal MontoFijoDefault { get; set; }
    public decimal PorcentajeDefault { get; set; }
    public decimal CantidadDefault { get; set; }
    public decimal TarifaDefault { get; set; }
    public int Orden { get; set; }
    public bool EsRecurrente { get; set; } = true;
    public bool AplicaPorEmpleado { get; set; } = true;
    public bool AfectaNetoEmpleado { get; set; }
    public bool AfectaCostoEmpresa { get; set; }
    public bool AfectaPasivoSat { get; set; }
    public bool AfectaPasivoImss { get; set; }
    public bool AfectaProvision { get; set; }
    public bool AfectaBaseIsr { get; set; }
    public bool AfectaBaseImss { get; set; }
    public bool EsLegal { get; set; }
    public string? Observaciones { get; set; }

    public ICollection<EmpleadoConceptoRrhh> EmpleadosConceptos { get; set; } = [];
    public ICollection<NominaProvisionDetalleRrhh> ProvisionesDetalle { get; set; } = [];
}

public class EmpleadoConceptoRrhh : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public Guid ConceptoConfigId { get; set; }
    public NominaConceptoConfigRrhh ConceptoConfig { get; set; } = null!;

    public decimal Monto { get; set; }
    public decimal Porcentaje { get; set; }
    public decimal Cantidad { get; set; }
    public decimal Tarifa { get; set; }
    public decimal Saldo { get; set; }
    public decimal Limite { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public bool EsRecurrente { get; set; } = true;
    public string? Observaciones { get; set; }
}

public class NominaProvisionDetalleRrhh : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid NominaDetalleId { get; set; }
    public NominaDetalle NominaDetalle { get; set; } = null!;

    public Guid EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public Guid ConceptoConfigId { get; set; }
    public NominaConceptoConfigRrhh ConceptoConfig { get; set; } = null!;

    public decimal Importe { get; set; }
    public decimal BaseCalculo { get; set; }
    public decimal Cantidad { get; set; }
    public decimal Tarifa { get; set; }
    public DateTime? PeriodoInicio { get; set; }
    public DateTime? PeriodoFin { get; set; }
    public bool EsAjusteManual { get; set; }
    public string? Observaciones { get; set; }
}
