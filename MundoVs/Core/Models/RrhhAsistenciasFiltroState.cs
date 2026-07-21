using MundoVs.Core.Entities;

namespace MundoVs.Core.Models;

public sealed class RrhhAsistenciasFiltroState
{
    public const string StorageKey = "rrhh.asistencias.filtros";
    public const string WeeklyStorageKey = "rrhh.asistencias.semanal.filtros";

    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public string TurnoIdTexto { get; set; } = string.Empty;
    public string Estatus { get; set; } = "todos";
    public string Revision { get; set; } = "todos";
    public string Orden { get; set; } = "nombre";
    public string? Empleado { get; set; }

    // Solo usa la vista semanal (por periodo de nómina). La lista diaria ignora
    // estos campos y sigue usando Desde/Hasta.
    public int? Periodicidad { get; set; }
    public DateTime? FechaReferenciaPeriodo { get; set; }

    public static RrhhAsistenciasFiltroState CreateDefault()
        => new()
        {
            Desde = DateTime.Today.AddDays(-15),
            Hasta = DateTime.Today,
            TurnoIdTexto = string.Empty,
            Estatus = "todos",
            Revision = "todos",
            Orden = "nombre",
            Empleado = null,
            Periodicidad = (int)PeriodicidadPago.Semanal,
            FechaReferenciaPeriodo = DateTime.Today
        };
}
