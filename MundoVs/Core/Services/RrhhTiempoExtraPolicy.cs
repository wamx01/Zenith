using MundoVs.Core.Entities;

namespace MundoVs.Core.Services;

public static class RrhhTiempoExtraPolicy
{
    public static int ObtenerMinutosNetoEfectivo(RrhhAsistencia asistencia)
        => Math.Max(0, asistencia.MinutosTrabajadosNetos + Math.Max(0, asistencia.MinutosPerdonadosManual));

    public static int ObtenerMinutosRetardoEfectivos(RrhhAsistencia asistencia)
        => ObtenerMinutosRetardoEfectivos(asistencia, 0);

    public static int ObtenerMinutosRetardoEfectivos(RrhhAsistencia asistencia, int minutosPermisoAplicados)
    {
        var perdonRestante = Math.Max(0, asistencia.MinutosPerdonadosManual);
        var retardo = Math.Max(0, asistencia.MinutosRetardo);
        var retardoDespuesPerdon = Math.Max(0, retardo - Math.Min(retardo, perdonRestante));
        return Math.Max(0, retardoDespuesPerdon - Math.Min(retardoDespuesPerdon, Math.Max(0, minutosPermisoAplicados)));
    }

    public static int ObtenerMinutosSalidaAnticipadaEfectivos(RrhhAsistencia asistencia)
    {
        var perdonRestante = Math.Max(0, asistencia.MinutosPerdonadosManual);
        var retardo = Math.Max(0, asistencia.MinutosRetardo);

        var retardoCubiertoPorPerdon = Math.Min(retardo, perdonRestante);
        perdonRestante = Math.Max(0, perdonRestante - retardoCubiertoPorPerdon);

        var salidaAnticipada = Math.Max(0, asistencia.MinutosSalidaAnticipada);
        return Math.Max(0, salidaAnticipada - Math.Min(salidaAnticipada, perdonRestante));
    }

    public static int ObtenerMinutosDescuentoOperacional(RrhhAsistencia asistencia, int minutosDescuentoManual = 0)
        => Math.Max(0, ObtenerMinutosRetardoEfectivos(asistencia) + ObtenerMinutosSalidaAnticipadaEfectivos(asistencia) + Math.Max(0, minutosDescuentoManual));

    public static int ObtenerMinutosDescuentoTotal(RrhhAsistencia asistencia, int minutosDescuentoManual = 0)
        => ObtenerMinutosDescuentoTotal(asistencia, minutosDescuentoManual, 0);

    public static int ObtenerMinutosDescuentoTotal(RrhhAsistencia asistencia, int minutosDescuentoManual, int minutosCompensadosAprobados)
        => Math.Max(0,
            ObtenerMinutosRetardoEfectivos(asistencia)
            + ObtenerMinutosSalidaAnticipadaEfectivos(asistencia)
            + ObtenerMinutosFaltanteDescontable(asistencia, minutosCompensadosAprobados)
            + Math.Max(0, minutosDescuentoManual));

    public static int ObtenerMinutosDescuentoTotal(RrhhAsistencia asistencia, int minutosDescuentoManual, int minutosPermisoAplicados, int minutosCompensadosAprobados)
        => Math.Max(0,
            ObtenerMinutosRetardoEfectivos(asistencia, minutosPermisoAplicados)
            + ObtenerMinutosSalidaAnticipadaEfectivos(asistencia)
            + ObtenerMinutosFaltanteDescontable(asistencia, minutosPermisoAplicados, minutosCompensadosAprobados)
            + Math.Max(0, minutosDescuentoManual));

    public static int ObtenerMinutosDescansoNoPagadoProgramado(RrhhAsistencia asistencia)
        => Math.Max(0, asistencia.MinutosJornadaProgramada - asistencia.MinutosJornadaNetaProgramada);

    [Obsolete("Usar RrhhAsistencia.MinutosCompensacionPermisoAprobados (columna autoritativa). "
        + "Este parser de bitácora se conserva sólo para el backfill one-shot de la Fase 6.")]
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

    // Un día se trata como "sin referencia de jornada" cuando no hay jornada neta
    // esperada contra la que comparar. Cubre los 3 casos que antes colapsaban en el
    // modo "SinTurno": (1) día no laborable con turno asignado (jornada neta 0),
    // (2) empleado sin turno asignado (jornada neta 0), y (3) esquema PorHoras
    // (EsPorHoras, sin jornada fija aunque el turno exista). El procesador ya no
    // persiste ModoSugerenciaExtra="SinTurno" (refactor I11); la derivación es
    // puramente por EsPorHoras o jornada neta <= 0.
    private static bool EsSinReferenciaJornada(RrhhAsistencia asistencia)
        => asistencia.EsPorHoras
           || asistencia.MinutosJornadaNetaProgramada <= 0;

    public static int ObtenerMinutosBasePagada(RrhhAsistencia asistencia)
    {
        // Empleado sin turno fijo (o día no laborable con turno asignado): todo el
        // tiempo trabajado es visible. El usuario decide cuánto es extra mediante la
        // resolución de tiempo extra. El extra aprobado se resta del base para no
        // duplicar: si el usuario aprueba 2h de extra sobre 10h trabajadas, el base
        // visible = 8h y el visible total = 8h + 2h = 10h.
        if (EsSinReferenciaJornada(asistencia))
        {
            var netoEfectivoSinTurno = ObtenerMinutosNetoEfectivo(asistencia);
            var extraAprobadoSinTurno = ObtenerMinutosExtraAprobados(asistencia);
            return Math.Max(0, netoEfectivoSinTurno - extraAprobadoSinTurno);
        }

        if (asistencia.MinutosJornadaNetaProgramada <= 0)
        {
            return 0;
        }

        var netoEfectivo = ObtenerMinutosNetoEfectivo(asistencia);
        var extraDetectado = Math.Max(0, asistencia.MinutosExtra);
        var baseNeta = Math.Max(0, netoEfectivo - extraDetectado);
        return Math.Min(baseNeta, asistencia.MinutosJornadaNetaProgramada);
    }

    public static int ObtenerMinutosExtraAprobados(RrhhAsistencia asistencia)
    {
        var aprobados = Math.Max(0, asistencia.MinutosExtraAutorizadosPago) + Math.Max(0, asistencia.MinutosExtraAutorizadosBanco);
        var detectados = Math.Max(0, asistencia.MinutosExtra);

        // Sin turno: el procesador no auto-detecta extra (MinutosExtra = 0).
        // El usuario aprueba manualmente cuánto del tiempo trabajado es extra,
        // así que no se limita por detectados.
        if (EsSinReferenciaJornada(asistencia))
        {
            return aprobados;
        }

        return detectados > 0
            ? Math.Min(aprobados, detectados)
            : aprobados;
    }

    public static int ObtenerMinutosExtraPagoFactorados(RrhhAsistencia asistencia, decimal factorTiempoExtra)
        => (int)Math.Round(Math.Max(0, asistencia.MinutosExtraAutorizadosPago) * Math.Max(1m, factorTiempoExtra), MidpointRounding.AwayFromZero);

    // Prorratea las horas de un permiso con goce entre los días que cubre.
    // El campo Horas de la ausencia es el total del permiso; repartirlo entre los
    // días evita sumar el total a cada día (sobre-conteo en permisos multi-día).
    // Si Dias no está poblado, se infiere del rango FechaInicio..FechaFin.
    public static int ObtenerMinutosPermisoConGocePorDia(RrhhAusencia ausencia)
    {
        var minutosTotales = (int)Math.Round(Math.Max(0m, ausencia.Horas) * 60m, MidpointRounding.AwayFromZero);
        var dias = Math.Max(1, Math.Max(0, ausencia.Dias));
        if (dias <= 1 && ausencia.FechaFin >= ausencia.FechaInicio)
        {
            dias = Math.Max(1, ausencia.FechaFin.DayNumber - ausencia.FechaInicio.DayNumber + 1);
        }

        return (int)Math.Round((decimal)minutosTotales / dias, MidpointRounding.AwayFromZero);
    }

    // Permiso visible canónico = permiso con goce prorrateado al día + banco-cobertura
    // (faltante cubierto consumiendo banco de horas). Es la única definición que
    // deben usar todas las vistas (lista diaria, semanal, modal) para no divergir.
    public static int ObtenerMinutosPermisoVisible(RrhhAsistencia asistencia, int minutosPermisoConGoceDia)
        => Math.Max(0, minutosPermisoConGoceDia) + Math.Max(0, asistencia.MinutosCubiertosBancoHoras);

    // Visible con permiso con goce prorrateado al día: el banco-cobertura lo añade
    // el policy vía ObtenerMinutosPermisoVisible, así el caller no lo duplica.
    public static int ObtenerMinutosTiempoVisible(RrhhAsistencia asistencia, int minutosPermisoConGoceDia, int minutosCompensadosAprobados)
        => Math.Max(0, ObtenerMinutosBasePagada(asistencia)
            + ObtenerMinutosPermisoVisible(asistencia, minutosPermisoConGoceDia)
            + Math.Max(0, minutosCompensadosAprobados)
            + ObtenerMinutosExtraAprobados(asistencia));

    // Sobrecarga conservada para callers que no pasan permiso con goce explícito
    // (el banco-cobertura sigue sumándose internamente). Redirige a la canonical.
    public static int ObtenerMinutosTiempoVisible(RrhhAsistencia asistencia, int minutosCompensadosAprobados)
        => ObtenerMinutosTiempoVisible(asistencia, 0, minutosCompensadosAprobados);

    public static int ObtenerMinutosAusenciaBrutaSugerida(RrhhAsistencia asistencia)
        => Math.Max(0, asistencia.MinutosJornadaProgramada - asistencia.MinutosTrabajadosBrutos);

    public static int ObtenerMinutosFaltanteNeto(RrhhAsistencia asistencia)
        => Math.Max(0, asistencia.MinutosJornadaNetaProgramada - ObtenerMinutosNetoEfectivo(asistencia));

    public static int ObtenerMinutosFaltanteDescontable(RrhhAsistencia asistencia)
        => ObtenerMinutosFaltanteDescontable(asistencia, 0, 0);

    public static int ObtenerMinutosFaltanteDescontable(RrhhAsistencia asistencia, int minutosCompensadosAprobados)
        => ObtenerMinutosFaltanteDescontable(asistencia, 0, minutosCompensadosAprobados);

    public static int ObtenerMinutosFaltanteDescontable(RrhhAsistencia asistencia, int minutosPermisoAplicados, int minutosCompensadosAprobados)
    {
        var faltante = ObtenerMinutosFaltanteNeto(asistencia);
        return Math.Max(0, faltante - Math.Max(0, minutosPermisoAplicados) - Math.Max(0, minutosCompensadosAprobados));
    }

    public static int ObtenerMinutosPermisoSugeridos(RrhhAsistencia asistencia, int minutosCompensadosAprobados = 0)
        => Math.Max(0, ObtenerMinutosFaltanteNeto(asistencia) - Math.Max(0, minutosCompensadosAprobados));

    public static int ObtenerMinutosDescansoNoPagadoExcluidosDelPermiso(RrhhAsistencia asistencia)
    {
        var descansoNoPagadoProgramado = ObtenerMinutosDescansoNoPagadoProgramado(asistencia);
        var ausenciaBruta = ObtenerMinutosAusenciaBrutaSugerida(asistencia);
        var permisoSugerido = ObtenerMinutosPermisoSugeridos(asistencia);
        return Math.Min(descansoNoPagadoProgramado, Math.Max(0, ausenciaBruta - permisoSugerido));
    }

    public static int ObtenerMinutosExtraResolubles(RrhhAsistencia asistencia, decimal factorTiempoExtra)
    {
        // Sin turno (incluye día no laborable con turno asignado, donde la jornada
        // neta programada es 0 y el procesador no detecta extra automático): el
        // máximo resoluble es el tiempo total trabajado, ya que el usuario decide
        // manualmente cuánto de ese tiempo es extra.
        if (EsSinReferenciaJornada(asistencia))
        {
            return Math.Max(0, asistencia.MinutosTrabajadosNetos);
        }

        return Math.Max(0, asistencia.MinutosExtra);
    }

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

        // Sin turno: siempre se muestra el apartado de tiempo extra para que el usuario
        // pueda decidir cuánto del tiempo trabajado es extra.
        if (EsSinReferenciaJornada(asistencia))
        {
            return asistencia.RequiereRevision
                || Math.Max(0, asistencia.MinutosTrabajadosNetos) > 0;
        }

        return asistencia.RequiereRevision
            || ObtenerMinutosFaltanteNeto(asistencia) > 0
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
