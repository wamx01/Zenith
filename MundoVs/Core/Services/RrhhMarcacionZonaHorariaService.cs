using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Core.Models;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Services;

public sealed class RrhhMarcacionZonaHorariaService : IRrhhMarcacionZonaHorariaService
{
    private const string DefaultMexicoTimeZone = "Central Standard Time (Mexico)";
    private static readonly Dictionary<string, string> TimeZoneAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["America/Mexico_City"] = "Central Standard Time (Mexico)",
        ["Etc/UTC"] = "UTC"
    };

    public async Task<RrhhMarcacionZonaHorariaCorrectionResult> CorregirMarcacionesGuardadasComoHoraLocalAsync(CrmDbContext db, RrhhMarcacionZonaHorariaCorrectionRequest request, CancellationToken cancellationToken = default)
    {
        var (checador, zonaHorariaAplicada, zona) = await ResolverContextoAsync(db, request, cancellationToken);
        var inicioCapturado = request.FechaDesde.ToDateTime(TimeOnly.MinValue);
        var finCapturadoExclusivo = request.FechaHasta.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var marcaciones = await db.RrhhMarcaciones
            .Where(m => m.EmpresaId == request.EmpresaId
                && m.ChecadorId == request.ChecadorId
                && m.FechaHoraMarcacionUtc >= inicioCapturado
                && m.FechaHoraMarcacionUtc < finCapturadoExclusivo)
            .OrderBy(m => m.FechaHoraMarcacionUtc)
            .ToListAsync(cancellationToken);

        if (marcaciones.Count == 0)
        {
            return new RrhhMarcacionZonaHorariaCorrectionResult
            {
                EmpresaId = request.EmpresaId,
                ChecadorId = request.ChecadorId,
                FechaDesde = request.FechaDesde,
                FechaHasta = request.FechaHasta,
                ZonaHorariaAplicada = zona.Id,
                MarcacionesEncontradas = 0,
                MarcacionesCorregidas = 0
            };
        }

        var propuestas = marcaciones
            .Select(m => new MarcacionCorregida(
                m,
                ConvertirHoraLocalCapturadaAUtc(m.FechaHoraMarcacionUtc, zona),
                CalcularHashMarcacion(m, ConvertirHoraLocalCapturadaAUtc(m.FechaHoraMarcacionUtc, zona))))
            .ToList();

        var hashesPropuestos = propuestas.Select(x => x.HashCorregido).ToList();
        var idsMarcaciones = propuestas.Select(x => x.Marcacion.Id).ToHashSet();
        var colisiones = await db.RrhhMarcaciones
            .Where(m => hashesPropuestos.Contains(m.HashUnico) && !idsMarcaciones.Contains(m.Id))
            .Select(m => m.HashUnico)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (colisiones.Count > 0)
        {
            throw new InvalidOperationException("La corrección produciría marcaciones duplicadas por hash. Ajusta el rango o revisa si ya se sincronizaron marcaciones corregidas.");
        }

        foreach (var propuesta in propuestas)
        {
            propuesta.Marcacion.FechaHoraMarcacionLocal = DateTime.SpecifyKind(propuesta.Marcacion.FechaHoraMarcacionUtc, DateTimeKind.Unspecified);
            propuesta.Marcacion.FechaHoraMarcacionUtc = propuesta.FechaHoraUtcCorregida;
            propuesta.Marcacion.ZonaHorariaAplicada = string.IsNullOrWhiteSpace(zonaHorariaAplicada) ? zona.Id : zonaHorariaAplicada.Trim();
            propuesta.Marcacion.HashUnico = propuesta.HashCorregido;
            propuesta.Marcacion.Procesada = false;
            propuesta.Marcacion.ResultadoProcesamiento = "Pendiente de reclasificación por corrección de zona horaria.";
            propuesta.Marcacion.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);

        return new RrhhMarcacionZonaHorariaCorrectionResult
        {
            EmpresaId = request.EmpresaId,
            ChecadorId = request.ChecadorId,
            FechaDesde = request.FechaDesde,
            FechaHasta = request.FechaHasta,
            ZonaHorariaAplicada = zona.Id,
            MarcacionesEncontradas = marcaciones.Count,
            MarcacionesCorregidas = propuestas.Count
        };
    }

    public async Task<RrhhMarcacionZonaHorariaCorrectionResult> ReconstruirHoraLocalDesdeUtcAsync(CrmDbContext db, RrhhMarcacionZonaHorariaCorrectionRequest request, CancellationToken cancellationToken = default)
    {
        var (_, zonaHorariaAplicada, zona) = await ResolverContextoAsync(db, request, cancellationToken);
        var inicioUtc = DateTime.SpecifyKind(request.FechaDesde.ToDateTime(TimeOnly.MinValue).AddHours(-14), DateTimeKind.Utc);
        var finUtcExclusivo = DateTime.SpecifyKind(request.FechaHasta.AddDays(1).ToDateTime(TimeOnly.MinValue).AddHours(14), DateTimeKind.Utc);

        var candidatas = await db.RrhhMarcaciones
            .Where(m => m.EmpresaId == request.EmpresaId
                && m.ChecadorId == request.ChecadorId
                && m.FechaHoraMarcacionUtc >= inicioUtc
                && m.FechaHoraMarcacionUtc < finUtcExclusivo)
            .OrderBy(m => m.FechaHoraMarcacionUtc)
            .ToListAsync(cancellationToken);

        var marcaciones = candidatas
            .Select(m => new MarcacionReconstruida(m, ConvertirUtcAZona(m.FechaHoraMarcacionUtc, zona)))
            .Where(x => DateOnly.FromDateTime(x.FechaHoraLocalReconstruida) >= request.FechaDesde
                && DateOnly.FromDateTime(x.FechaHoraLocalReconstruida) <= request.FechaHasta)
            .ToList();

        foreach (var reconstruida in marcaciones)
        {
            reconstruida.Marcacion.FechaHoraMarcacionLocal = DateTime.SpecifyKind(reconstruida.FechaHoraLocalReconstruida, DateTimeKind.Unspecified);
            reconstruida.Marcacion.ZonaHorariaAplicada = zonaHorariaAplicada;
            reconstruida.Marcacion.Procesada = false;
            reconstruida.Marcacion.ResultadoProcesamiento = "Pendiente de reclasificación por reconstrucción de hora local.";
            reconstruida.Marcacion.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);

        return new RrhhMarcacionZonaHorariaCorrectionResult
        {
            EmpresaId = request.EmpresaId,
            ChecadorId = request.ChecadorId,
            FechaDesde = request.FechaDesde,
            FechaHasta = request.FechaHasta,
            ZonaHorariaAplicada = zonaHorariaAplicada,
            MarcacionesEncontradas = marcaciones.Count,
            MarcacionesCorregidas = marcaciones.Count
        };
    }

    private static async Task<(RrhhChecador Checador, string ZonaHorariaAplicada, TimeZoneInfo Zona)> ResolverContextoAsync(CrmDbContext db, RrhhMarcacionZonaHorariaCorrectionRequest request, CancellationToken cancellationToken)
    {
        var checador = await db.RrhhChecadores
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.ChecadorId && c.EmpresaId == request.EmpresaId, cancellationToken)
            ?? throw new InvalidOperationException("No se encontró el checador indicado.");

        var zonaHorariaAplicada = string.IsNullOrWhiteSpace(request.ZonaHoraria)
            ? checador.ZonaHoraria
            : request.ZonaHoraria;
        var zona = ResolverZonaHoraria(zonaHorariaAplicada);
        return (checador, string.IsNullOrWhiteSpace(zonaHorariaAplicada) ? zona.Id : zonaHorariaAplicada.Trim(), zona);
    }

    private static DateTime ConvertirHoraLocalCapturadaAUtc(DateTime fechaCapturada, TimeZoneInfo zona)
    {
        var fechaLocal = DateTime.SpecifyKind(fechaCapturada, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(fechaLocal, zona);
    }

    private static DateTime ConvertirUtcAZona(DateTime fechaUtc, TimeZoneInfo zona)
    {
        var utc = fechaUtc.Kind == DateTimeKind.Utc ? fechaUtc : DateTime.SpecifyKind(fechaUtc, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utc, zona);
    }

    private static string CalcularHashMarcacion(RrhhMarcacion marcacion, DateTime fechaHoraUtc)
    {
        var raw = string.Join("|",
            marcacion.EmpresaId,
            marcacion.ChecadorId,
            marcacion.CodigoChecador.Trim().ToUpperInvariant(),
            fechaHoraUtc.ToUniversalTime().ToString("O"),
            marcacion.TipoMarcacionRaw?.Trim().ToUpperInvariant() ?? string.Empty,
            marcacion.EventoIdExterno?.Trim().ToUpperInvariant() ?? string.Empty,
            marcacion.Origen?.Trim().ToUpperInvariant() ?? string.Empty,
            marcacion.PayloadRaw?.Trim().ToUpperInvariant() ?? string.Empty);

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hash);
    }

    private static TimeZoneInfo ResolverZonaHoraria(string? zonaHoraria)
    {
        if (string.IsNullOrWhiteSpace(zonaHoraria))
        {
            return ResolverZonaHoraria(DefaultMexicoTimeZone);
        }

        if (TryCreateUtcOffsetTimeZone(zonaHoraria, out var zonaHorariaOffset))
        {
            return zonaHorariaOffset;
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(zonaHoraria.Trim());
        }
        catch (TimeZoneNotFoundException)
        {
            if (TimeZoneAliases.TryGetValue(zonaHoraria.Trim(), out var alias))
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(alias);
                }
                catch (TimeZoneNotFoundException)
                {
                }
            }
        }
        catch (InvalidTimeZoneException)
        {
        }

        throw new InvalidOperationException($"La zona horaria '{zonaHoraria}' no es válida o no está disponible en este servidor.");
    }

    private static bool TryCreateUtcOffsetTimeZone(string zonaHoraria, out TimeZoneInfo timeZone)
    {
        var normalized = zonaHoraria.Trim().ToUpperInvariant();
        if (normalized == "UTC")
        {
            timeZone = TimeZoneInfo.Utc;
            return true;
        }

        if (!normalized.StartsWith("UTC", StringComparison.Ordinal) || normalized.Length <= 3)
        {
            timeZone = TimeZoneInfo.Utc;
            return false;
        }

        var offsetText = normalized[3..];
        if (offsetText.Length == 3)
        {
            offsetText += ":00";
        }

        if (offsetText.Length != 6
            || (offsetText[0] != '+' && offsetText[0] != '-')
            || offsetText[3] != ':'
            || !int.TryParse(offsetText[1..3], out var hours)
            || !int.TryParse(offsetText[4..6], out var minutes))
        {
            timeZone = TimeZoneInfo.Utc;
            return false;
        }

        var totalMinutes = (hours * 60) + minutes;
        var offset = offsetText[0] == '-'
            ? TimeSpan.FromMinutes(-totalMinutes)
            : TimeSpan.FromMinutes(totalMinutes);

        try
        {
            var id = $"UTC{(offset >= TimeSpan.Zero ? "+" : "-")}{Math.Abs(offset.Hours):00}:{Math.Abs(offset.Minutes):00}";
            timeZone = TimeZoneInfo.CreateCustomTimeZone(id, offset, id, id);
            return true;
        }
        catch
        {
            timeZone = TimeZoneInfo.Utc;
            return false;
        }
    }

    private sealed record MarcacionCorregida(RrhhMarcacion Marcacion, DateTime FechaHoraUtcCorregida, string HashCorregido);
    private sealed record MarcacionReconstruida(RrhhMarcacion Marcacion, DateTime FechaHoraLocalReconstruida);
}
