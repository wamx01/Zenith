using MundoVs.Core.Entities.Serigrafia;

namespace MundoVs.Core.Entities;

public class Empleado
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string Codigo { get; set; } = string.Empty;
    public string NumeroEmpleado { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? ApellidoPaterno { get; set; }
    public string? ApellidoMaterno { get; set; }
    public string? Curp { get; set; }
    public string? Nss { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public string? CodigoChecador { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public DateTime? FechaContratacion { get; set; }
    public Guid? PosicionId { get; set; }
    public Guid? TurnoBaseId { get; set; }
    // Método de cálculo por defecto para los días Fija del empleado. null = "Vs horario"
    // (aplica turno/retardo/salida/faltante/extra con umbral); "MarcajeReloj" = cuenta tal
    // cual las marcas, sin reglas. Sólo aplica con jornada Fija (Por horas lo anula el
    // processor). El "Recalcular por periodo" (forzarDefaultEmpleado=true) lo impone a
    // todos los días; el recálculo por día/incremental lo usa como fallback.
    // English: default calculation method for the employee's Fija days. null = "Vs schedule"
    // (applies shift/late/early-leave/shortage/extra with threshold); "MarcajeReloj" = counts
    // marks as-is, no rules. Only meaningful with Fija (Por horas nulls it in the processor).
    // "Recalculate by period" (forzarDefaultEmpleado=true) enforces it on every day; per-day
    // / incremental recalc uses it as a fallback.
    public string? ModoSugerenciaExtraDefault { get; set; }
    public string? Puesto { get; set; }
    public string? Departamento { get; set; }
    public decimal SueldoSemanal { get; set; }
    public TipoNomina TipoNomina { get; set; } = TipoNomina.Semanal;
    public PeriodicidadPago PeriodicidadPago { get; set; } = PeriodicidadPago.Semanal;
    public bool AplicaImss { get; set; }
    public bool AplicaIsr { get; set; }
    public bool AplicaInfonavit { get; set; }
    public string? NumeroCreditoInfonavit { get; set; }
    public decimal FactorDescuentoInfonavit { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notas { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public string NombreCompleto => $"{Nombre} {ApellidoPaterno} {ApellidoMaterno}".Trim();

    public Posicion? Posicion { get; set; }
    public TurnoBase? TurnoBase { get; set; }
    public ICollection<RrhhEmpleadoTurno> TurnosVigencia { get; set; } = [];
    public ICollection<RrhhMarcacion> Marcaciones { get; set; } = [];
    public ICollection<RrhhAsistencia> Asistencias { get; set; } = [];
    public ICollection<RrhhBancoHorasMovimiento> BancoHorasMovimientos { get; set; } = [];
    public ICollection<RrhhAusencia> Ausencias { get; set; } = [];
    public ICollection<NominaDetalle> NominaDetalles { get; set; } = [];
    public ICollection<EmpleadoConceptoRrhh> ConceptosNomina { get; set; } = [];
    public ICollection<EmpleadoEsquemaPago> EsquemasPago { get; set; } = [];
    public ICollection<EmpleadoEsquemaJornada> EsquemasJornada { get; set; } = [];
    public ICollection<ValeDestajo> ValesDestajo { get; set; } = [];
}

public enum TipoNomina
{
    Semanal = 1,
    Destajo = 2,
    Mixto = 3
}
