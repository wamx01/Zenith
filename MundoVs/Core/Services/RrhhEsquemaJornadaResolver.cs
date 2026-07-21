using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Services;

/// <summary>
/// Resuelve el esquema de jornada (<see cref="TipoJornada"/>) vigente para un empleado
/// a una fecha dada, considerando la vigencia (VigenteDesde / VigenteHasta). El esquema
/// PorHoras define que el empleado no tiene jornada fija: se paga el tiempo trabajado,
/// sin faltante/retardo, extra sólo manual, y en festivo el tiempo va al factor festivo.
/// </summary>
public sealed class RrhhEsquemaJornadaResolver
{
    /// <summary>
    /// Devuelve el esquema vigente a <paramref name="fecha"/>. Si no hay ninguno activo,
    /// cae a <see cref="TipoJornada.Fija"/>: por defecto si el empleado no tiene ningún
    /// esquema, o como hueco si existe un esquema futuro (VigenteDesde > fecha) que aún
    /// no entra — el hueco se reporta para que el UI lo valide y no quede el día sin
    /// esquema explícito.
    /// </summary>
    public async Task<EsquemaJornadaResuelto> ObtenerEsquemaVigenteAsync(
        CrmDbContext db, Guid empleadoId, DateTime fecha, CancellationToken cancellationToken = default)
    {
        var activo = await db.EmpleadosEsquemaJornada
            .Where(e => e.EmpleadoId == empleadoId && e.IsActive
                && e.VigenteDesde <= fecha
                && (e.VigenteHasta == null || e.VigenteHasta >= fecha))
            .OrderByDescending(e => e.VigenteDesde)
            .FirstOrDefaultAsync(cancellationToken);

        if (activo is not null)
            return new EsquemaJornadaResuelto(activo.TipoJornada, EsDefault: false, EsHueco: false, activo.Id);

        var existeFuturo = await db.EmpleadosEsquemaJornada
            .AnyAsync(e => e.EmpleadoId == empleadoId && e.IsActive && e.VigenteDesde > fecha, cancellationToken);

        return new EsquemaJornadaResuelto(TipoJornada.Fija, EsDefault: !existeFuturo, EsHueco: existeFuturo, EsquemaId: null);
    }

    /// <summary>Conveniencia: sólo el TipoJornada vigente (lo que usa el procesador).</summary>
    public async Task<TipoJornada> ObtenerTipoJornadaAsync(
        CrmDbContext db, Guid empleadoId, DateTime fecha, CancellationToken cancellationToken = default)
        => (await ObtenerEsquemaVigenteAsync(db, empleadoId, fecha, cancellationToken)).TipoJornada;
}

/// <summary>Resultado de resolver el esquema de jornada a una fecha.</summary>
/// <param name="TipoJornada">Esquema vigente (o Fija por defecto/hueco).</param>
/// <param name="EsDefault">True si cayó al default porque no hay ningún esquema.</param>
/// <param name="EsHueco">True si no hay esquema activo pero existe uno futuro sin entrar.</param>
/// <param name="EsquemaId">Id del esquema vigente, o null si cayó a default/hueco.</param>
public sealed record EsquemaJornadaResuelto(TipoJornada TipoJornada, bool EsDefault, bool EsHueco, Guid? EsquemaId);