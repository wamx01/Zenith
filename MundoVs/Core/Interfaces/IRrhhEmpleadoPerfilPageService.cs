using MundoVs.Core.Entities;
using MundoVs.Core.Entities.Serigrafia;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Interfaces;

public interface IRrhhEmpleadoPerfilPageService
{
    Task<EmpleadoPerfilData> CargarAsync(CrmDbContext db, Guid empresaId, Guid empleadoId);
}

public sealed class EmpleadoPerfilData
{
    public Empleado Empleado { get; init; } = null!;
    public Posicion? Posicion { get; init; }
    public string EsquemaVigente { get; init; } = "—";
    public EmpleadoEsquemaPago? EsquemaAsignacionVigente { get; init; }
    public TurnoBase? TurnoVigente { get; init; }
    public RrhhEmpleadoTurno? TurnoAsignacionVigente { get; init; }
    public decimal VacacionesDisponibles { get; init; }
    public decimal SaldoBancoHoras { get; init; }
    public CicloVacacionalInfo? CicloVacacional { get; init; }

    public IReadOnlyList<EmpleadoEsquemaPago> HistorialEsquemas { get; init; } = [];
    public IReadOnlyList<RrhhEmpleadoTurno> HistorialTurnos { get; init; } = [];
    public IReadOnlyList<EmpleadoConceptoRrhh> ConceptosNomina { get; init; } = [];
    public IReadOnlyList<RrhhAsistencia> AsistenciasRecientes { get; init; } = [];
    public IReadOnlyList<RrhhAusencia> AusenciasRecientes { get; init; } = [];
    public IReadOnlyList<RrhhBancoHorasMovimiento> MovimientosBancoHoras { get; init; } = [];

    public IReadOnlyList<Posicion> Posiciones { get; init; } = [];
    public IReadOnlyList<DepartamentoRrhh> Departamentos { get; init; } = [];
    public IReadOnlyList<EsquemaPago> EsquemasPago { get; init; } = [];
    public IReadOnlyList<TurnoBase> TurnosBase { get; init; } = [];
    public IReadOnlyList<NominaConceptoConfigRrhh> ConceptosNominaConfig { get; init; } = [];
}

public sealed class CicloVacacionalInfo
{
    public DateTime InicioCiclo { get; init; }
    public DateTime FinCiclo { get; init; }
    public bool TieneDerechoVacaciones { get; init; }
    public int AnioVacacionalReconocido { get; init; }
    public decimal DiasVacacionesEquivalentes { get; init; }
}