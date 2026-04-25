using MundoVs.Core.Entities;

namespace MundoVs.Core.Services;

public static class RrhhTiempoExtraPolicy
{
    public static int ObtenerMinutosDescansoNoPagadoProgramado(RrhhAsistencia asistencia)
        => Math.Max(0, asistencia.MinutosJornadaProgramada - asistencia.MinutosJornadaNetaProgramada);

    public static int ObtenerMinutosAusenciaBrutaSugerida(RrhhAsistencia asistencia)
        => Math.Max(0, asistencia.MinutosJornadaProgramada - asistencia.MinutosTrabajadosBrutos);

    public static int ObtenerMinutosFaltanteBanco(RrhhAsistencia asistencia)
        => Math.Max(0, asistencia.MinutosJornadaNetaProgramada - asistencia.MinutosTrabajadosNetos);

    public static int ObtenerMinutosPermisoSugeridos(RrhhAsistencia asistencia)
        => ObtenerMinutosFaltanteBanco(asistencia);

    public static int ObtenerMinutosDescansoNoPagadoExcluidosDelPermiso(RrhhAsistencia asistencia)
    {
        var descansoNoPagadoProgramado = ObtenerMinutosDescansoNoPagadoProgramado(asistencia);
        var ausenciaBruta = ObtenerMinutosAusenciaBrutaSugerida(asistencia);
        var permisoSugerido = ObtenerMinutosPermisoSugeridos(asistencia);
        return Math.Min(descansoNoPagadoProgramado, Math.Max(0, ausenciaBruta - permisoSugerido));
    }

    public static int ObtenerMinutosExtraResolubles(RrhhAsistencia asistencia, decimal factorTiempoExtra)
        => (int)Math.Round(Math.Max(0, asistencia.MinutosExtra) * Math.Max(0m, factorTiempoExtra), MidpointRounding.AwayFromZero);

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
