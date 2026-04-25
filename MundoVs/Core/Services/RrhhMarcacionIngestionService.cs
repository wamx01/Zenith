using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Core.Models;
using MundoVs.Infrastructure.Data;
using Zenith.Contracts.Asistencia;

namespace MundoVs.Core.Services;

public sealed class RrhhMarcacionIngestionService : IRrhhMarcacionIngestionService
{
    private const string DefaultMexicoTimeZone = "Central Standard Time (Mexico)";
    private static readonly Dictionary<string, string> TimeZoneAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["America/Mexico_City"] = "Central Standard Time (Mexico)",
        ["Etc/UTC"] = "UTC"
    };

    public async Task<RrhhMarcacionIngestionResult> IngerirLoteAsync(CrmDbContext db, MarcacionSyncBatchDto batch, CancellationToken cancellationToken = default)
    {
        if (batch.EmpresaId == Guid.Empty || batch.ChecadorId == Guid.Empty)
        {
            return new RrhhMarcacionIngestionResult
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Response = new SyncResultDto
                {
                    ChecadorId = batch.ChecadorId,
                    Mensaje = "Empresa y checador son requeridos."
                }
            };
        }

        var checador = await db.RrhhChecadores
            .FirstOrDefaultAsync(c => c.Id == batch.ChecadorId && c.EmpresaId == batch.EmpresaId && c.IsActive, cancellationToken);

        if (checador == null)
        {
            return new RrhhMarcacionIngestionResult
            {
                StatusCode = StatusCodes.Status404NotFound,
                Response = new SyncResultDto
                {
                    ChecadorId = batch.ChecadorId,
                    Mensaje = "El checador no existe o no pertenece a la empresa indicada."
                }
            };
        }

        IReadOnlyCollection<MarcacionRawDto> marcaciones = batch.Marcaciones ?? [];
        var leidas = marcaciones.Count;
        var duplicadas = 0;
        var creadas = 0;
        var fallidas = 0;

        var codigos = marcaciones
            .Select(m => m.CodigoChecador?.Trim())
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Cast<string>()
            .ToList();

        var empleadosPorCodigo = codigos.Count == 0
            ? new Dictionary<string, Empleado>(StringComparer.OrdinalIgnoreCase)
            : await db.Empleados
                .Where(e => e.EmpresaId == batch.EmpresaId && e.IsActive && e.CodigoChecador != null && codigos.Contains(e.CodigoChecador))
                .ToDictionaryAsync(e => e.CodigoChecador!, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var hashes = marcaciones
            .Where(EsMarcacionValida)
            .Select(m => CalcularHashMarcacion(m, AsegurarUtc(m.FechaHoraMarcacionUtc)))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var hashesExistentes = hashes.Count == 0
            ? new HashSet<string>(StringComparer.Ordinal)
            : (await db.RrhhMarcaciones
                .Where(m => hashes.Contains(m.HashUnico))
                .Select(m => m.HashUnico)
                .ToListAsync(cancellationToken))
                .ToHashSet(StringComparer.Ordinal);

        foreach (var marcacion in marcaciones)
        {
            if (!EsMarcacionValida(marcacion))
            {
                fallidas++;
                continue;
            }

            var fechaHoraUtc = AsegurarUtc(marcacion.FechaHoraMarcacionUtc);
            var zonaHorariaAplicada = ObtenerZonaHorariaAplicada(marcacion.ZonaHorariaAplicada, checador.ZonaHoraria);
            var fechaHoraLocal = ObtenerFechaHoraLocal(marcacion, fechaHoraUtc, zonaHorariaAplicada);
            var hash = CalcularHashMarcacion(marcacion, fechaHoraUtc);
            if (!hashesExistentes.Add(hash))
            {
                duplicadas++;
                continue;
            }

            empleadosPorCodigo.TryGetValue(marcacion.CodigoChecador.Trim(), out var empleado);

            db.RrhhMarcaciones.Add(new RrhhMarcacion
            {
                Id = Guid.NewGuid(),
                EmpresaId = batch.EmpresaId,
                ChecadorId = batch.ChecadorId,
                EmpleadoId = empleado?.Id,
                CodigoChecador = marcacion.CodigoChecador.Trim(),
                FechaHoraMarcacionLocal = fechaHoraLocal,
                FechaHoraMarcacionUtc = fechaHoraUtc,
                ZonaHorariaAplicada = zonaHorariaAplicada,
                TipoMarcacionRaw = LimpiarTexto(marcacion.TipoMarcacionRaw),
                Origen = LimpiarTexto(marcacion.Origen),
                EventoIdExterno = LimpiarTexto(marcacion.EventoIdExterno),
                HashUnico = hash,
                Procesada = false,
                ResultadoProcesamiento = empleado == null ? "Empleado no identificado" : "Pendiente de clasificación",
                PayloadRaw = LimpiarTexto(marcacion.PayloadRaw),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });

            creadas++;
        }

        checador.UltimaSincronizacionUtc = DateTime.UtcNow;
        checador.UltimoEventoLeido = marcaciones
            .Where(EsMarcacionValida)
            .OrderByDescending(m => m.FechaHoraMarcacionUtc)
            .Select(m => m.EventoIdExterno ?? m.FechaHoraMarcacionUtc.ToString("O"))
            .FirstOrDefault();

        if (leidas > 0)
        {
            db.RrhhLogsChecador.Add(new RrhhLogChecador
            {
                Id = Guid.NewGuid(),
                EmpresaId = batch.EmpresaId,
                ChecadorId = batch.ChecadorId,
                FechaUtc = DateTime.UtcNow,
                Nivel = fallidas > 0 ? "Warning" : "Information",
                Mensaje = $"Se recibieron {leidas} marcación(es); nuevas: {creadas}; duplicadas: {duplicadas}; fallidas: {fallidas}.",
                Detalle = fallidas > 0 ? "Se descartaron marcaciones con código de checador vacío o fecha inválida." : null,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });
        }

        await db.SaveChangesAsync(cancellationToken);

        return new RrhhMarcacionIngestionResult
        {
            StatusCode = StatusCodes.Status200OK,
            Response = new SyncResultDto
            {
                ChecadorId = batch.ChecadorId,
                Leidas = leidas,
                Enviadas = creadas,
                Duplicadas = duplicadas,
                Fallidas = fallidas,
                Mensaje = "Lote recibido correctamente por el ERP."
            }
        };
    }

    private static bool EsMarcacionValida(MarcacionRawDto marcacion)
        => marcacion is not null
            && !string.IsNullOrWhiteSpace(marcacion.CodigoChecador)
            && marcacion.FechaHoraMarcacionUtc != default;

    private static string CalcularHashMarcacion(MarcacionRawDto marcacion, DateTime fechaHoraUtc)
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

    private static string? LimpiarTexto(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static DateTime AsegurarUtc(DateTime fechaHoraUtc)
        => fechaHoraUtc.Kind == DateTimeKind.Utc ? fechaHoraUtc : DateTime.SpecifyKind(fechaHoraUtc, DateTimeKind.Utc);

    private static DateTime ObtenerFechaHoraLocal(MarcacionRawDto marcacion, DateTime fechaHoraUtc, string zonaHorariaAplicada)
    {
        if (marcacion.FechaHoraMarcacionLocal.HasValue)
        {
            return DateTime.SpecifyKind(marcacion.FechaHoraMarcacionLocal.Value, DateTimeKind.Unspecified);
        }

        var fechaLocal = TimeZoneInfo.ConvertTimeFromUtc(fechaHoraUtc, ResolverZonaHoraria(zonaHorariaAplicada));
        return DateTime.SpecifyKind(fechaLocal, DateTimeKind.Unspecified);
    }

    private static string ObtenerZonaHorariaAplicada(string? zonaHorariaMarcacion, string? zonaHorariaChecador)
    {
        var zonaHoraria = LimpiarTexto(zonaHorariaMarcacion) ?? LimpiarTexto(zonaHorariaChecador);
        if (string.IsNullOrWhiteSpace(zonaHoraria))
        {
            return ResolverZonaHoraria(null).Id;
        }

        _ = ResolverZonaHoraria(zonaHoraria);
        return zonaHoraria;
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
                return TimeZoneInfo.FindSystemTimeZoneById(alias);
            }
        }
        catch (InvalidTimeZoneException)
        {
        }

        return TimeZoneInfo.Utc;
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
}
