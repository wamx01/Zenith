namespace MundoVs.Core.Entities;

public enum TipoSegmentoResolucionRrhh
{
    Trabajo = 1,
    Extra = 2,
    Descanso = 3,
    SalidaTemporal = 4,
    Permiso = 5,
    NoConsiderar = 6
}

public enum EstadoSegmentoResolucionRrhh
{
    Vigente = 1,
    Obsoleta = 2,
    RequiereRevision = 3
}

public class RrhhSegmentoResolucion : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public DateOnly Fecha { get; set; }

    public Guid MarcacionInicioId { get; set; }
    public RrhhMarcacion MarcacionInicio { get; set; } = null!;

    public Guid MarcacionFinId { get; set; }
    public RrhhMarcacion MarcacionFin { get; set; } = null!;

    public TipoSegmentoResolucionRrhh TipoSegmento { get; set; }
    public EstadoSegmentoResolucionRrhh Estado { get; set; } = EstadoSegmentoResolucionRrhh.Vigente;
    public bool FueInferidoAutomaticamente { get; set; }
    public int? MinutosAplicadosOverride { get; set; }
    public string? Observaciones { get; set; }
}
