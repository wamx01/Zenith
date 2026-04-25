using MundoVs.Core.Entities;
using MundoVs.Core.Entities.Serigrafia;

namespace MundoVs.Core.Services;

public static class DestajoTarifaResolver
{
    public static DestajoTarifaResultado Resolver(
        TipoProceso proceso,
        Guid? posicionEmpleadoId,
        decimal minutosPorPieza,
        IEnumerable<EsquemaPagoTarifa>? tarifasEsquema)
    {
        var tarifaMinutoOperacion = Math.Max(0, proceso.Posicion?.TarifaPorMinuto ?? 0m);
        if (minutosPorPieza > 0 && tarifaMinutoOperacion > 0)
        {
            return new DestajoTarifaResultado
            {
                Tarifa = Math.Round(minutosPorPieza * tarifaMinutoOperacion, 4),
                Origen = $"Cotización: {minutosPorPieza:N4} min/pza × {tarifaMinutoOperacion:C4}/min",
                Prioridad = DestajoTarifaPrioridad.CotizacionOperacion
            };
        }

        var tarifaEsquema = ResolverTarifaEsquema(proceso.Id, posicionEmpleadoId, tarifasEsquema);
        if (tarifaEsquema != null)
        {
            var origen = string.IsNullOrWhiteSpace(tarifaEsquema.Descripcion)
                ? $"Esquema activo como respaldo: tarifa de operación {tarifaEsquema.Tarifa:C4}/pieza."
                : $"Esquema activo como respaldo: {tarifaEsquema.Descripcion.Trim()} ({tarifaEsquema.Tarifa:C4}/pieza).";

            return new DestajoTarifaResultado
            {
                Tarifa = Math.Round(tarifaEsquema.Tarifa, 4),
                EsquemaPagoTarifaId = tarifaEsquema.Id,
                Origen = origen,
                Prioridad = DestajoTarifaPrioridad.Esquema
            };
        }

        if (minutosPorPieza <= 0)
        {
            return new DestajoTarifaResultado
            {
                Origen = "Sin tiempo estándar en la cotización del pedido.",
                Prioridad = DestajoTarifaPrioridad.SinDefinir
            };
        }

        if (tarifaMinutoOperacion <= 0)
        {
            return new DestajoTarifaResultado
            {
                Origen = "Sin tarifa configurada para la operación.",
                Prioridad = DestajoTarifaPrioridad.SinDefinir
            };
        }

        return new DestajoTarifaResultado
        {
            Origen = "No se pudo resolver la tarifa del destajo.",
            Prioridad = DestajoTarifaPrioridad.SinDefinir
        };
    }

    public static EsquemaPagoTarifa? ResolverTarifaEsquema(
        Guid tipoProcesoId,
        Guid? posicionEmpleadoId,
        IEnumerable<EsquemaPagoTarifa>? tarifasEsquema)
    {
        if (tarifasEsquema == null)
        {
            return null;
        }

        return tarifasEsquema
            .Where(t => (t.TipoProcesoId == null || t.TipoProcesoId == tipoProcesoId)
                && (t.PosicionId == null || t.PosicionId == posicionEmpleadoId))
            .OrderBy(t => t.TipoProcesoId == tipoProcesoId && t.PosicionId == posicionEmpleadoId ? 0 :
                          t.TipoProcesoId == tipoProcesoId && t.PosicionId == null ? 1 :
                          t.TipoProcesoId == null && t.PosicionId == posicionEmpleadoId ? 2 :
                          t.TipoProcesoId == null && t.PosicionId == null ? 3 : 4)
            .FirstOrDefault();
    }
}

public sealed class DestajoTarifaResultado
{
    public decimal Tarifa { get; set; }
    public Guid? EsquemaPagoTarifaId { get; set; }
    public string? Origen { get; set; }
    public DestajoTarifaPrioridad Prioridad { get; set; }
}

public enum DestajoTarifaPrioridad
{
    SinDefinir = 0,
    Esquema = 1,
    CotizacionOperacion = 2
}
