using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Entities.Serigrafia;
using MundoVs.Core.Interfaces;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Services;

public sealed class RrhhEmpleadoPerfilPageService : IRrhhEmpleadoPerfilPageService
{
    private readonly INominaLegalPolicyService _nominaLegalPolicy;

    public RrhhEmpleadoPerfilPageService(INominaLegalPolicyService nominaLegalPolicy)
    {
        _nominaLegalPolicy = nominaLegalPolicy;
    }

    public async Task<EmpleadoPerfilData> CargarAsync(CrmDbContext db, Guid empresaId, Guid empleadoId)
    {
        var empleado = await db.Empleados
            .Include(e => e.TurnoBase)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == empleadoId && e.EmpresaId == empresaId);

        if (empleado is null)
            throw new InvalidOperationException($"Empleado {empleadoId} no encontrado en empresa {empresaId}.");

        var posiciones = await db.Posiciones.AsNoTracking().Where(p => p.IsActive).OrderBy(p => p.Orden).ThenBy(p => p.Nombre).ToListAsync();
        var departamentos = await db.DepartamentosRrhh.AsNoTracking().Where(d => d.IsActive).OrderBy(d => d.Orden).ThenBy(d => d.Nombre).ToListAsync();
        var esquemasPago = await db.EsquemasPago.AsNoTracking().OrderBy(e => e.Nombre).ToListAsync();
        var turnosBase = await db.TurnosBase.AsNoTracking().OrderByDescending(t => t.IsActive).ThenBy(t => t.Nombre).ToListAsync();
        var conceptosConfig = await db.NominasConceptosConfigRrhh.AsNoTracking().Where(c => c.EmpresaId == empresaId && c.IsActive && c.AplicaPorEmpleado).OrderBy(c => c.Orden).ThenBy(c => c.Nombre).ToListAsync();

        var posicion = empleado.PosicionId.HasValue
            ? posiciones.FirstOrDefault(p => p.Id == empleado.PosicionId.Value)
            : null;

        var hoy = DateOnly.FromDateTime(DateTime.Today);

        // Esquema vigente
        var asignacionEsquema = await db.EmpleadosEsquemaPago
            .Include(a => a.EsquemaPago)
            .AsNoTracking()
            .Where(a => a.EmpleadoId == empleadoId && a.VigenteDesde <= DateTime.Today && (a.VigenteHasta == null || a.VigenteHasta >= DateTime.Today))
            .OrderByDescending(a => a.VigenteDesde)
            .FirstOrDefaultAsync();

        var nombreEsquema = asignacionEsquema?.EsquemaPago?.Nombre ?? "—";

        // Historial esquemas
        var historialEsquemas = await db.EmpleadosEsquemaPago
            .Include(a => a.EsquemaPago)
            .AsNoTracking()
            .Where(a => a.EmpleadoId == empleadoId)
            .OrderByDescending(a => a.VigenteDesde)
            .ToListAsync();

        // Turno vigente
        var asignacionTurno = await db.RrhhEmpleadosTurno
            .Include(v => v.TurnoBase)
                .ThenInclude(t => t.Detalles)
                    .ThenInclude(d => d.Descansos)
            .AsNoTracking()
            .Where(v => v.EmpleadoId == empleadoId && v.IsActive && v.VigenteDesde <= hoy && (v.VigenteHasta == null || v.VigenteHasta >= hoy))
            .OrderByDescending(v => v.VigenteDesde)
            .FirstOrDefaultAsync();

        var turnoVigente = asignacionTurno?.TurnoBase ?? empleado.TurnoBase;

        // Historial turnos
        var historialTurnos = await db.RrhhEmpleadosTurno
            .Include(v => v.TurnoBase)
                .ThenInclude(t => t.Detalles)
                    .ThenInclude(d => d.Descansos)
            .AsNoTracking()
            .Where(v => v.EmpleadoId == empleadoId && v.IsActive)
            .OrderByDescending(v => v.VigenteDesde)
            .ToListAsync();

        // Conceptos nómina del empleado
        var conceptosNomina = await db.EmpleadosConceptosRrhh
            .Include(c => c.ConceptoConfig)
            .AsNoTracking()
            .Where(c => c.EmpleadoId == empleadoId && c.IsActive)
            .OrderBy(c => c.ConceptoConfig.Orden)
            .ThenBy(c => c.ConceptoConfig.Nombre)
            .ToListAsync();

        // Asistencias recientes (últimos 30 días)
        var hace30 = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
        var asistenciasRecientes = await db.RrhhAsistencias
            .AsNoTracking()
            .Where(a => a.EmpleadoId == empleadoId && a.Fecha >= hace30)
            .OrderByDescending(a => a.Fecha)
            .Take(30)
            .ToListAsync();

        // Ausencias recientes
        var ausenciasRecientes = await db.RrhhAusencias
            .AsNoTracking()
            .Where(a => a.EmpleadoId == empleadoId && a.IsActive)
            .OrderByDescending(a => a.FechaInicio)
            .Take(20)
            .ToListAsync();

        // Banco de horas
        var movimientosBancoHoras = await db.RrhhBancoHorasMovimientos
            .AsNoTracking()
            .Where(m => m.EmpleadoId == empleadoId && m.IsActive)
            .OrderByDescending(m => m.Fecha)
            .Take(30)
            .ToListAsync();

        var saldoBancoHoras = movimientosBancoHoras.Sum(m => m.Horas);

        // Vacaciones
        var configuracionNomina = await NominaConfiguracionLoader.LoadAsync(db, empresaId);
        var vacacionesUsadas = await db.RrhhAusencias
            .AsNoTracking()
            .Where(a => a.EmpleadoId == empleadoId
                && a.IsActive
                && a.Tipo == TipoAusenciaRrhh.Vacaciones
                && (a.Estatus == EstatusAusenciaRrhh.Aprobada || a.Estatus == EstatusAusenciaRrhh.Aplicada))
            .ToListAsync();

        var ciclo = _nominaLegalPolicy.ObtenerCicloVacacional(empleado, DateTime.Today, configuracionNomina);
        var vacacionesDisponibles = ciclo.TieneDerechoVacaciones
            ? _nominaLegalPolicy.CalcularDiasVacacionesDisponibles(empleado, DateTime.Today, vacacionesUsadas.Sum(a => CalcularDiasAusenciaEnRango(a, DateOnly.FromDateTime(ciclo.InicioCiclo), DateOnly.FromDateTime(ciclo.FinCiclo))), configuracionNomina)
            : 0m;

        var cicloInfo = new CicloVacacionalInfo
        {
            InicioCiclo = ciclo.InicioCiclo,
            FinCiclo = ciclo.FinCiclo,
            TieneDerechoVacaciones = ciclo.TieneDerechoVacaciones,
            AnioVacacionalReconocido = ciclo.AnioVacacionalReconocido,
            DiasVacacionesEquivalentes = ciclo.DiasVacacionesEquivalentes
        };

        return new EmpleadoPerfilData
        {
            Empleado = empleado,
            Posicion = posicion,
            EsquemaVigente = nombreEsquema,
            EsquemaAsignacionVigente = asignacionEsquema,
            TurnoVigente = turnoVigente,
            TurnoAsignacionVigente = asignacionTurno,
            VacacionesDisponibles = vacacionesDisponibles,
            SaldoBancoHoras = saldoBancoHoras,
            CicloVacacional = cicloInfo,
            HistorialEsquemas = historialEsquemas,
            HistorialTurnos = historialTurnos,
            ConceptosNomina = conceptosNomina,
            AsistenciasRecientes = asistenciasRecientes,
            AusenciasRecientes = ausenciasRecientes,
            MovimientosBancoHoras = movimientosBancoHoras,
            Posiciones = posiciones,
            Departamentos = departamentos,
            EsquemasPago = esquemasPago,
            TurnosBase = turnosBase,
            ConceptosNominaConfig = conceptosConfig
        };
    }

    private static int CalcularDiasAusenciaEnRango(RrhhAusencia ausencia, DateOnly inicio, DateOnly fin)
    {
        var inicioReal = ausencia.FechaInicio > inicio ? ausencia.FechaInicio : inicio;
        var finReal = ausencia.FechaFin < fin ? ausencia.FechaFin : fin;
        if (finReal < inicioReal)
            return 0;
        return (finReal.DayNumber - inicioReal.DayNumber) + 1;
    }
}