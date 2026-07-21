using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Core.Models;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Services;

public sealed class RrhhAsistenciasPageService : IRrhhAsistenciasPageService
{
    public async Task<RrhhAsistenciasPageData> CargarAsync(CrmDbContext db, Guid empresaId, RrhhAsistenciasFiltroState filtros, CancellationToken cancellationToken = default)
    {
        var turnos = await db.TurnosBase
            .Include(t => t.Detalles)
                .ThenInclude(d => d.Descansos)
            .AsNoTracking()
            .Where(t => t.EmpresaId == empresaId && t.IsActive)
            .OrderBy(t => t.Nombre)
            .ToListAsync(cancellationToken);

        var empleadosReproceso = await db.Empleados
            .AsNoTracking()
            .Where(e => e.EmpresaId == empresaId && e.IsActive)
            .OrderBy(e => e.Nombre)
            .ThenBy(e => e.ApellidoPaterno)
            .ToListAsync(cancellationToken);

        var asistencias = await ConstruirConsultaAsistencias(db, empresaId, filtros)
            .OrderByDescending(a => a.Fecha)
            .ThenBy(a => a.Empleado.Nombre)
            .Take(200)
            .ToListAsync(cancellationToken);

        var ausenciasPorDia = await ConstruirAusenciasPorDiaAsync(db, empresaId, asistencias, cancellationToken);
        var permisosVisiblesPorDia = await ConstruirPermisosVisiblesPorDiaAsync(db, empresaId, asistencias, cancellationToken);

        return new RrhhAsistenciasPageData
        {
            Asistencias = asistencias,
            Turnos = turnos,
            EmpleadosReproceso = empleadosReproceso,
            AusenciasPorDia = ausenciasPorDia,
            PermisosVisiblesPorDia = permisosVisiblesPorDia
        };
    }

    private static IQueryable<RrhhAsistencia> ConstruirConsultaAsistencias(CrmDbContext db, Guid empresaId, RrhhAsistenciasFiltroState filtros)
    {
        var query = db.RrhhAsistencias
            .AsNoTracking()
            .Include(a => a.Empleado)
            .Include(a => a.TurnoBase)
            .Where(a => a.EmpresaId == empresaId);

        if (filtros.Desde.HasValue)
        {
            var desde = DateOnly.FromDateTime(filtros.Desde.Value.Date);
            query = query.Where(a => a.Fecha >= desde);
        }

        if (filtros.Hasta.HasValue)
        {
            var hasta = DateOnly.FromDateTime(filtros.Hasta.Value.Date);
            query = query.Where(a => a.Fecha <= hasta);
        }

        if (Guid.TryParse(filtros.TurnoIdTexto, out var turnoId) && turnoId != Guid.Empty)
        {
            query = query.Where(a => a.TurnoBaseId == turnoId);
        }

        if (int.TryParse(filtros.Estatus, out var estatusValor) && Enum.IsDefined(typeof(RrhhAsistenciaEstatus), estatusValor))
        {
            var estatus = (RrhhAsistenciaEstatus)estatusValor;
            query = query.Where(a => a.Estatus == estatus);
        }

        query = filtros.Revision switch
        {
            "si" => query.Where(a => a.RequiereRevision),
            "no" => query.Where(a => !a.RequiereRevision),
            _ => query
        };

        if (!string.IsNullOrWhiteSpace(filtros.Empleado))
        {
            var texto = filtros.Empleado.Trim();
            query = query.Where(a =>
                a.Empleado.Nombre.Contains(texto) ||
                (a.Empleado.ApellidoPaterno != null && a.Empleado.ApellidoPaterno.Contains(texto)) ||
                (a.Empleado.ApellidoMaterno != null && a.Empleado.ApellidoMaterno.Contains(texto)) ||
                a.Empleado.NumeroEmpleado.Contains(texto) ||
                (a.Empleado.CodigoChecador != null && a.Empleado.CodigoChecador.Contains(texto)));
        }

        return query;
    }

    private static async Task<Dictionary<string, string>> ConstruirAusenciasPorDiaAsync(CrmDbContext db, Guid empresaId, IReadOnlyCollection<RrhhAsistencia> asistencias, CancellationToken cancellationToken)
    {
        if (asistencias.Count == 0)
        {
            return new Dictionary<string, string>();
        }

        var empleadoIds = asistencias.Select(a => a.EmpleadoId).Distinct().ToList();
        var fechaMin = asistencias.Min(a => a.Fecha);
        var fechaMax = asistencias.Max(a => a.Fecha);
        var ausencias = await db.RrhhAusencias
            .AsNoTracking()
            .Where(a => a.EmpresaId == empresaId
                && empleadoIds.Contains(a.EmpleadoId)
                && a.IsActive
                && (a.Estatus == EstatusAusenciaRrhh.Aprobada || a.Estatus == EstatusAusenciaRrhh.Aplicada)
                && a.FechaInicio <= fechaMax
                && a.FechaFin >= fechaMin)
            .ToListAsync(cancellationToken);

        var mapa = new Dictionary<string, List<string>>();
        foreach (var ausencia in ausencias)
        {
            var inicio = ausencia.FechaInicio < fechaMin ? fechaMin : ausencia.FechaInicio;
            var fin = ausencia.FechaFin > fechaMax ? fechaMax : ausencia.FechaFin;
            for (var fecha = inicio; fecha <= fin; fecha = fecha.AddDays(1))
            {
                var clave = CrearClaveAusencia(ausencia.EmpleadoId, fecha);
                if (!mapa.TryGetValue(clave, out var valores))
                {
                    valores = [];
                    mapa[clave] = valores;
                }

                valores.Add(FormatearAusencia(ausencia));
            }
        }

        return mapa.ToDictionary(kvp => kvp.Key, kvp => string.Join(" | ", kvp.Value.Distinct()));
    }

    private static async Task<Dictionary<string, int>> ConstruirPermisosVisiblesPorDiaAsync(CrmDbContext db, Guid empresaId, IReadOnlyCollection<RrhhAsistencia> asistencias, CancellationToken cancellationToken)
    {
        if (asistencias.Count == 0)
        {
            return new Dictionary<string, int>();
        }

        var empleadoIds = asistencias.Select(a => a.EmpleadoId).Distinct().ToList();
        var fechaMin = asistencias.Min(a => a.Fecha);
        var fechaMax = asistencias.Max(a => a.Fecha);
        var permisos = await db.RrhhAusencias
            .AsNoTracking()
            .Where(a => a.EmpresaId == empresaId
                && empleadoIds.Contains(a.EmpleadoId)
                && a.IsActive
                && a.ConGocePago
                && a.Horas > 0
                && (a.Estatus == EstatusAusenciaRrhh.Aprobada || a.Estatus == EstatusAusenciaRrhh.Aplicada)
                && a.FechaInicio <= fechaMax
                && a.FechaFin >= fechaMin)
            .ToListAsync(cancellationToken);

        var resultado = new Dictionary<string, int>();
        foreach (var permiso in permisos)
        {
            var minutosDia = RrhhTiempoExtraPolicy.ObtenerMinutosPermisoConGocePorDia(permiso);
            var inicio = permiso.FechaInicio < fechaMin ? fechaMin : permiso.FechaInicio;
            var fin = permiso.FechaFin > fechaMax ? fechaMax : permiso.FechaFin;
            for (var fecha = inicio; fecha <= fin; fecha = fecha.AddDays(1))
            {
                var clave = CrearClaveDia(permiso.EmpleadoId, fecha);
                resultado[clave] = resultado.GetValueOrDefault(clave) + minutosDia;
            }
        }

        return resultado;
    }

    private static string CrearClaveAusencia(Guid empleadoId, DateOnly fecha)
        => $"{empleadoId:N}:{fecha:yyyyMMdd}";

    private static string CrearClaveDia(Guid empleadoId, DateOnly fecha)
        => $"{empleadoId:N}:{fecha:yyyyMMdd}";

    private static string FormatearAusencia(RrhhAusencia ausencia)
    {
        var nombre = ausencia.Tipo switch
        {
            TipoAusenciaRrhh.Vacaciones => "Vacaciones",
            TipoAusenciaRrhh.PermisoConGoce => "Permiso con goce",
            TipoAusenciaRrhh.PermisoSinGoce => "Permiso sin goce",
            TipoAusenciaRrhh.Capacitacion => "Capacitación",
            TipoAusenciaRrhh.Incapacidad => "Incapacidad",
            TipoAusenciaRrhh.FaltaInjustificada => "Falta injustificada",
            TipoAusenciaRrhh.Suspension => "Suspensión",
            TipoAusenciaRrhh.DiasEconomicos => "Días económicos",
            TipoAusenciaRrhh.PermisoPaternidad => "Paternidad",
            TipoAusenciaRrhh.PermisoMaternidad => "Maternidad",
            _ => $"Permiso {(ausencia.ConGocePago ? "con goce" : "sin goce")}"
        };
        return ausencia.Horas > 0 ? $"{nombre} ({ausencia.Horas:0.##} h)" : nombre;
    }
}
