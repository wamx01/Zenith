using MundoVs.Core.Entities;

namespace MundoVs.Core.Services;

public static class RrhhTiempoExtraPolicy
{
    public static int ObtenerMinutosTrabajadosNetosEfectivos(RrhhAsistencia asistencia)
        => Math.Max(0, asistencia.MinutosTrabajadosNetos + Math.Max(0, asistencia.MinutosPerdonadosManual));

    public static int ObtenerMinutosRetardoEfectivos(RrhhAsistencia asistencia)
    {
        var perdonRestante = Math.Max(0, asistencia.MinutosPerdonadosManual);
        var retardo = Math.Max(0, asistencia.MinutosRetardo);
        return Math.Max(0, retardo - Math.Min(retardo, perdonRestante));
    }

    public static int ObtenerMinutosSalidaAnticipadaEfectivos(RrhhAsistencia asistencia)
    {
        var perdonRestante = Math.Max(0, asistencia.MinutosPerdonadosManual);
        var retardo = Math.Max(0, asistencia.MinutosRetardo);
        perdonRestante = Math.Max(0, perdonRestante - Math.Min(retardo, perdonRestante));
        var salidaAnticipada = Math.Max(0, asistencia.MinutosSalidaAnticipada);
        return Math.Max(0, salidaAnticipada - Math.Min(salidaAnticipada, perdonRestante));
    }

    public static int ObtenerMinutosDescuentoEfectivos(RrhhAsistencia asistencia, int minutosDescuentoManual = 0)
        => Math.Max(0, ObtenerMinutosRetardoEfectivos(asistencia) + ObtenerMinutosSalidaAnticipadaEfectivos(asistencia) + Math.Max(0, minutosDescuentoManual));

    public static int ObtenerMinutosDescansoNoPagadoProgramado(RrhhAsistencia asistencia)
        => Math.Max(0, asistencia.MinutosJornadaProgramada - asistencia.MinutosJornadaNetaProgramada);

    public static int ObtenerMinutosPermisoCompensadosAprobados(IEnumerable<RrhhLogChecador> bitacora, Guid empleadoId, DateOnly fecha)
    {
        const string prefijo = "minutosCompensados=";

        foreach (var log in bitacora
                     .Where(l => l.Detalle != null
                         && l.Mensaje.Contains("compensación aprobada de permiso", StringComparison.OrdinalIgnoreCase)
                         && l.Detalle.Contains($"empleado={empleadoId}", StringComparison.OrdinalIgnoreCase)
                         && l.Detalle.Contains($"fecha={fecha:yyyy-MM-dd}", StringComparison.OrdinalIgnoreCase))
                     .OrderByDescending(l => l.FechaUtc))
        {
            var detalle = log.Detalle!;
            var indice = detalle.IndexOf(prefijo, StringComparison.OrdinalIgnoreCase);
            if (indice < 0)
            {
                continue;
            }

            indice += prefijo.Length;
            var fin = detalle.IndexOf(';', indice);
            var texto = fin >= 0 ? detalle[indice..fin] : detalle[indice..];
            if (int.TryParse(texto, out var minutos))
            {
                return Math.Max(0, minutos);
            }
        }

        return 0;
    }

    public static int ObtenerMinutosTrabajadosBaseVisibles(RrhhAsistencia asistencia)
        => Math.Max(0, Math.Min(ObtenerMinutosTrabajadosNetosEfectivos(asistencia), asistencia.MinutosJornadaNetaProgramada > 0 ? asistencia.MinutosJornadaNetaProgramada : ObtenerMinutosTrabajadosNetosEfectivos(asistencia)));

    public static int ObtenerMinutosExtraAprobados(RrhhAsistencia asistencia)
    {
        var aprobados = Math.Max(0, asistencia.MinutosExtraAutorizadosPago) + Math.Max(0, asistencia.MinutosExtraAutorizadosBanco);
        var detectados = Math.Max(0, asistencia.MinutosExtra);
        return detectados > 0
            ? Math.Min(aprobados, detectados)
            : aprobados;
    }

    public static int ObtenerMinutosExtraPagoFactorados(RrhhAsistencia asistencia, decimal factorTiempoExtra)
        => (int)Math.Round(Math.Max(0, asistencia.MinutosExtraAutorizadosPago) * Math.Max(1m, factorTiempoExtra), MidpointRounding.AwayFromZero);

    public static int ObtenerMinutosTrabajadosVisibles(RrhhAsistencia asistencia, int minutosCompensadosAprobados)
        => Math.Max(0, ObtenerMinutosTrabajadosBaseVisibles(asistencia) + Math.Max(0, minutosCompensadosAprobados) + ObtenerMinutosExtraAprobados(asistencia));

    public static int ObtenerMinutosAusenciaBrutaSugerida(RrhhAsistencia asistencia)
        => Math.Max(0, asistencia.MinutosJornadaProgramada - asistencia.MinutosTrabajadosBrutos);

    public static int ObtenerMinutosFaltanteBanco(RrhhAsistencia asistencia)
        => Math.Max(0, asistencia.MinutosJornadaNetaProgramada - ObtenerMinutosTrabajadosNetosEfectivos(asistencia));

    public static int ObtenerMinutosPermisoSugeridos(RrhhAsistencia asistencia, int minutosCompensadosAprobados = 0)
        => Math.Max(0, ObtenerMinutosFaltanteBanco(asistencia) - Math.Max(0, minutosCompensadosAprobados));

    public static int ObtenerMinutosDescansoNoPagadoExcluidosDelPermiso(RrhhAsistencia asistencia)
    {
        var descansoNoPagadoProgramado = ObtenerMinutosDescansoNoPagadoProgramado(asistencia);
        var ausenciaBruta = ObtenerMinutosAusenciaBrutaSugerida(asistencia);
        var permisoSugerido = ObtenerMinutosPermisoSugeridos(asistencia);
        return Math.Min(descansoNoPagadoProgramado, Math.Max(0, ausenciaBruta - permisoSugerido));
    }

    public static int ObtenerMinutosExtraResolubles(RrhhAsistencia asistencia, decimal factorTiempoExtra)
        => Math.Max(0, asistencia.MinutosExtra);

    public static string ConstruirReferenciaResolucion(Guid asistenciaId, string sufijo)
        => $"Asistencia:{asistenciaId:N}:{sufijo}";

    public static string ObtenerResumenResolucionAplicada(RrhhAsistencia asistencia)
        => $"Pago {asistencia.MinutosExtraAutorizadosPago} min / Banco {asistencia.MinutosExtraAutorizadosBanco} min";

    public static string ObtenerResumenResolucion(RrhhAsistencia asistencia)
    {
        if (asistencia.MinutosCubiertosBancoHoras > 0)
        {
            return $"{ObtenerResumenResolucionAplicada(asistencia)} · Banco cubrió {asistencia.MinutosCubiertosBancoHoras} min";
        }

        if (asistencia.MinutosExtraAutorizadosPago > 0 || asistencia.MinutosExtraAutorizadosBanco > 0)
        {
            return ObtenerResumenResolucionAplicada(asistencia);
        }

        return string.IsNullOrWhiteSpace(asistencia.ResolucionTiempoExtra)
            ? "Pendiente"
            : asistencia.ResolucionTiempoExtra + " (actualizado)";
    }

    public static bool TieneCoberturaAusencia(string? resumenAusencia)
        => !string.IsNullOrWhiteSpace(resumenAusencia)
            && !string.Equals(resumenAusencia.Trim(), "—", StringComparison.Ordinal);

    public static bool TieneResolucionTiempoAplicada(RrhhAsistencia asistencia)
        => asistencia.MinutosExtraAutorizadosPago > 0
            || asistencia.MinutosExtraAutorizadosBanco > 0
            || asistencia.MinutosCubiertosBancoHoras > 0
            || !string.IsNullOrWhiteSpace(asistencia.ResolucionTiempoExtra);

    public static bool TieneResolucionOperativaPendiente(RrhhAsistencia asistencia, string? resumenAusencia)
    {
        if (TieneCoberturaAusencia(resumenAusencia) || TieneResolucionTiempoAplicada(asistencia))
        {
            return false;
        }

        return asistencia.RequiereRevision
            || ObtenerMinutosFaltanteBanco(asistencia) > 0
            || Math.Max(0, asistencia.MinutosExtra) > 0;
    }

    public static string ObtenerResumenResolucionOperativa(RrhhAsistencia asistencia, string? resumenAusencia)
    {
        if (TieneCoberturaAusencia(resumenAusencia))
        {
            return resumenAusencia!.Trim();
        }

        if (TieneResolucionTiempoAplicada(asistencia))
        {
            return ObtenerResumenResolucion(asistencia);
        }

        return TieneResolucionOperativaPendiente(asistencia, resumenAusencia)
            ? "Pendiente"
            : "Sin ajuste pendiente";
    }
}
