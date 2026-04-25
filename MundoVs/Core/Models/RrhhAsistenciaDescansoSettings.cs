using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Models;

public sealed class RrhhAsistenciaDescansoSettings
{
    public const string ToleranciaCoincidenciaBrutaKey = "RRHH.Asistencia.Descansos.ToleranciaCoincidenciaBrutaMinutos";
    public const string ToleranciaCoincidenciaNetaKey = "RRHH.Asistencia.Descansos.ToleranciaCoincidenciaNetaMinutos";
    public const string ZonaAmbiguaHastaKey = "RRHH.Asistencia.Descansos.ZonaAmbiguaHastaMinutos";
    public const string ToleranciaExcesoDescansoKey = "RRHH.Asistencia.Descansos.ToleranciaExcesoDescansoMinutos";
    public const string ToleranciaRetardoKey = "RRHH.Asistencia.ToleranciaRetardoMinutos";

    public const int ToleranciaCoincidenciaBrutaDefault = 15;
    public const int ToleranciaCoincidenciaNetaDefault = 15;
    public const int ZonaAmbiguaHastaDefault = 30;
    public const int ToleranciaExcesoDescansoDefault = 5;
    public const int ToleranciaRetardoDefault = 5;

    public int ToleranciaCoincidenciaBrutaMinutos { get; set; } = ToleranciaCoincidenciaBrutaDefault;
    public int ToleranciaCoincidenciaNetaMinutos { get; set; } = ToleranciaCoincidenciaNetaDefault;
    public int ZonaAmbiguaHastaMinutos { get; set; } = ZonaAmbiguaHastaDefault;
    public int ToleranciaExcesoDescansoMinutos { get; set; } = ToleranciaExcesoDescansoDefault;
    public int ToleranciaRetardoMinutos { get; set; } = ToleranciaRetardoDefault;

    public void Normalize()
    {
        if (ToleranciaCoincidenciaBrutaMinutos < 0)
        {
            ToleranciaCoincidenciaBrutaMinutos = ToleranciaCoincidenciaBrutaDefault;
        }

        if (ToleranciaCoincidenciaNetaMinutos < 0)
        {
            ToleranciaCoincidenciaNetaMinutos = ToleranciaCoincidenciaNetaDefault;
        }

        if (ToleranciaExcesoDescansoMinutos < 0)
        {
            ToleranciaExcesoDescansoMinutos = ToleranciaExcesoDescansoDefault;
        }

        if (ToleranciaRetardoMinutos < 0)
        {
            ToleranciaRetardoMinutos = ToleranciaRetardoDefault;
        }

        var minimoZonaAmbigua = Math.Max(ToleranciaCoincidenciaBrutaMinutos, ToleranciaCoincidenciaNetaMinutos);
        if (ZonaAmbiguaHastaMinutos < minimoZonaAmbigua)
        {
            ZonaAmbiguaHastaMinutos = Math.Max(ZonaAmbiguaHastaDefault, minimoZonaAmbigua);
        }
    }

    public static async Task<RrhhAsistenciaDescansoSettings> LoadAsync(IAppConfigRepository repository)
    {
        var settings = new RrhhAsistenciaDescansoSettings
        {
            ToleranciaCoincidenciaBrutaMinutos = ParseOrDefault(await repository.GetValueAsync(ToleranciaCoincidenciaBrutaKey), ToleranciaCoincidenciaBrutaDefault),
            ToleranciaCoincidenciaNetaMinutos = ParseOrDefault(await repository.GetValueAsync(ToleranciaCoincidenciaNetaKey), ToleranciaCoincidenciaNetaDefault),
            ZonaAmbiguaHastaMinutos = ParseOrDefault(await repository.GetValueAsync(ZonaAmbiguaHastaKey), ZonaAmbiguaHastaDefault),
            ToleranciaExcesoDescansoMinutos = ParseOrDefault(await repository.GetValueAsync(ToleranciaExcesoDescansoKey), ToleranciaExcesoDescansoDefault),
            ToleranciaRetardoMinutos = ParseOrDefault(await repository.GetValueAsync(ToleranciaRetardoKey), ToleranciaRetardoDefault)
        };

        settings.Normalize();
        return settings;
    }

    public static async Task<RrhhAsistenciaDescansoSettings> LoadAsync(CrmDbContext db, Guid empresaId, CancellationToken cancellationToken = default)
    {
        var configuraciones = await db.AppConfigs
            .AsNoTracking()
            .Where(c => c.EmpresaId == empresaId
                && (c.Clave == ToleranciaCoincidenciaBrutaKey
                    || c.Clave == ToleranciaCoincidenciaNetaKey
                    || c.Clave == ZonaAmbiguaHastaKey
                    || c.Clave == ToleranciaExcesoDescansoKey
                    || c.Clave == ToleranciaRetardoKey))
            .ToDictionaryAsync(c => c.Clave, c => c.Valor, cancellationToken);

        var settings = new RrhhAsistenciaDescansoSettings
        {
            ToleranciaCoincidenciaBrutaMinutos = ParseOrDefault(configuraciones.GetValueOrDefault(ToleranciaCoincidenciaBrutaKey), ToleranciaCoincidenciaBrutaDefault),
            ToleranciaCoincidenciaNetaMinutos = ParseOrDefault(configuraciones.GetValueOrDefault(ToleranciaCoincidenciaNetaKey), ToleranciaCoincidenciaNetaDefault),
            ZonaAmbiguaHastaMinutos = ParseOrDefault(configuraciones.GetValueOrDefault(ZonaAmbiguaHastaKey), ZonaAmbiguaHastaDefault),
            ToleranciaExcesoDescansoMinutos = ParseOrDefault(configuraciones.GetValueOrDefault(ToleranciaExcesoDescansoKey), ToleranciaExcesoDescansoDefault),
            ToleranciaRetardoMinutos = ParseOrDefault(configuraciones.GetValueOrDefault(ToleranciaRetardoKey), ToleranciaRetardoDefault)
        };

        settings.Normalize();
        return settings;
    }

    private static int ParseOrDefault(string? value, int fallback)
        => int.TryParse(value, out var parsed) ? parsed : fallback;
}
