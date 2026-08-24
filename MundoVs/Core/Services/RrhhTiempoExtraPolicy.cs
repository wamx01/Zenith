using MundoVs.Core.Entities;

namespace MundoVs.Core.Services;

public static class RrhhTiempoExtraPolicy
{
    public static int ObtenerMinutosNetoEfectivo(RrhhAsistencia asistencia)
        => Math.Max(0, asistencia.MinutosTrabajadosNetos
            + Math.Max(0, asistencia.MinutosPerdonadosManual)
            + Math.Max(0, asistencia.MinutosToleranciaRetardoAplicada));

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

    // Empleado Fija SIN turno asignado (TurnoBaseId null, no PorHoras). A diferencia de
    // PorHoras (que se paga el tiempo trabajado sin meta), el Fija-sin-turno tiene una
    // META SEMANAL (default 48h = 2880 min): el extra sobre la meta es autorizable y el
    // déficit bajo la meta descuenta sueldo como FaltanteDescontable. La meta se calcula
    // a nivel de periodo (no per-día), así que este predicado sólo identifica el día; el
    // overlay real vive en la agregación del periodo (RrhhResolucionPeriodoService /
    // RrhhNominaSnapshotService). El día suelto sigue siendo "sin referencia" para los
    // displays per-día (EsSinReferenciaJornada no cambia).
    // English: Fija employee with NO assigned shift (TurnoBaseId null, not PorHoras). Unlike
    // PorHoras (paid for worked time, no meta), Fija-sin-turno has a WEEKLY META (default 48h
    // = 2880 min): extra over the meta is authorizable, deficit under the meta docks salary
    // as FaltanteDescontable. The meta is computed at the period level (not per-day), so this
    // predicate only identifies the day; the real overlay lives in the period aggregation. The
    // standalone day still behaves as "sin referencia" for per-day displays (unchanged).
    public static bool EsJornadaMetaSemanal(RrhhAsistencia asistencia)
        => !asistencia.EsPorHoras && asistencia.TurnoBaseId is null;

    // ¿El periodo entero se rige por la meta semanal? Cuando TODAS las asistencias no-PorHoras
    // son sin turno (sin mezcla turno/sin-turno). Un periodo mixto (algun día con turno, otro
    // sin) es raro (cambio de turno) y cae al behavior per-día existente por seguridad. Un
    // periodo todo-PorHoras no es meta semanal (se paga por horas).
    // English: Is the whole period governed by the weekly meta? When ALL non-PorHoras
    // asistencias have no shift (no turno/sin-turno mix). A mixed period (some days with shift,
    // some without) is rare (shift change) and falls back to the existing per-day behavior for
    // safety. An all-PorHoras period is not weekly-meta (paid by the hour).
    public static bool EsPeriodoMetaSemanal(IEnumerable<RrhhAsistencia> asistencias)
    {
        var lista = asistencias as IReadOnlyList<RrhhAsistencia> ?? asistencias.ToList();
        if (lista.Count == 0)
            return false;

        var noPorHoras = lista.Where(a => !a.EsPorHoras).ToList();
        if (noPorHoras.Count == 0)
            return false;

        return noPorHoras.All(a => a.TurnoBaseId is null);
    }

    // Meta semanal en minutos a partir de las horas base configuradas por empresa
    // (NominaConfiguracion.HorasBaseSemanal, default 48). 48h -> 2880 min.
    // English: Weekly meta in minutes from the company-configured base hours
    // (NominaConfiguracion.HorasBaseSemanal, default 48). 48h -> 2880 min.
    public static int ObtenerMetaSemanalMinutos(int horasBaseSemanal)
        => Math.Max(0, horasBaseSemanal) * 60;

    // Normaliza el umbral mínimo de tiempo extra (MinutosMinimosTiempoExtra): un valor
    // ausente o <= 0 cae al default 15. Es el MISMO perdón que usa el cálculo por día
    // (RrhhAsistenciaProcessor.ObtenerMinutosMinimosTiempoExtra delega aquí), centralizado
    // para que el overlay de meta semanal aplique el mismo umbral que el detalle por día y
    // no haya inconsistencia entre "Ver detalle" y "Aceptar tiempo" cuando la config es 0.
    // English: Normalizes the minimum extra-time threshold (MinutosMinimosTiempoExtra): a
    // missing or <= 0 value falls back to the default 15. This is the SAME forgiveness the
    // per-day calc uses (RrhhAsistenciaProcessor.ObtenerMinutosMinimosTiempoExtra delegates
    // here), centralized so the weekly-meta overlay applies the same threshold as the
    // per-day detail and there is no mismatch between "Ver detalle" and "Aceptar tiempo"
    // when the config value is 0.
    public static int NormalizarMinutosMinimosTiempoExtra(int minutosMinimosTiempoExtra)
        => minutosMinimosTiempoExtra > 0 ? minutosMinimosTiempoExtra : 15;

    // Balance de la meta semanal: extra = trabajo real sobre la meta; deficit = meta menos
    // trabajo real menos el tiempo cubierto (permiso con goce + compensacion aprobada). El
    // conGoce cubre el deficit (es tiempo pagado que satisface la expectativa, espejo del
    // Fija-con-turno donde el permiso con goce cubre el faltante del dia). Extra y deficit son
    // mutuamente excluyentes por construccion (uno siempre es 0).
    // El extra respeta el mismo umbral mínimo que el cálculo por día (MinutosMinimosTiempoExtra):
    // un excedente bajo el umbral NO cuenta como extra (meta 2880, trabajado 2890 → 10 < umbral
    // → 0); al superarlo sí cuenta todo el excedente (2911 → 31 ≥ umbral → 31). El déficit no se
    // ve afectado por el umbral (descuenta sueldo). `minutosMinimosTiempoExtra` default 0 =
    // sin umbral (backward compat para callers que no lo pasan).
    // English: Weekly meta balance: extra = real work over the meta; deficit = meta minus real
    // work minus covered time (paid leave + approved compensation). conGoce covers the deficit
    // (it is paid time that satisfies the expectation, mirroring Fija-con-turno where paid
    // leave covers the day's faltante). Extra and deficit are mutually exclusive by construction.
    // Extra honors the same minimum threshold as the per-day calc (MinutosMinimosTiempoExtra):
    // surplus below the threshold does NOT count as extra (meta 2880, worked 2890 → 10 < threshold
    // → 0); once exceeded, the whole surplus counts (2911 → 31 ≥ threshold → 31). Deficit is
    // unaffected by the threshold (docks salary). `minutosMinimosTiempoExtra` default 0 = no
    // threshold (backward compat for callers that don't pass it).
    public static (int ExtraDetectado, int Deficit) CalcularBalanceMetaSemanal(
        int trabajadoActualMinutos, int conGoceMinutos, int metaMinutos, int minutosMinimosTiempoExtra = 0)
    {
        var extraCrudo = Math.Max(0, trabajadoActualMinutos - metaMinutos);
        var extra = minutosMinimosTiempoExtra > 0 && extraCrudo < minutosMinimosTiempoExtra ? 0 : extraCrudo;
        var deficit = Math.Max(0, metaMinutos - trabajadoActualMinutos - Math.Max(0, conGoceMinutos));
        return (extra, deficit);
    }

    /// <summary>
    /// Cadena de neteo NetoVsNeto del periodo (faltante → retardo → salida → banco). POOL =
    /// <paramref name="extraDetectado"/> (sobre umbral, pagadero) + <paramref name="extraBajoUmbral"/>
    /// (bajo umbral, NO pagadero, sólo tapa deducciones). El extra de un día tapa en orden las
    /// deducciones de otros días; el sobrante repone el banco consumido; el sobrante final —topado
    /// al extraDetectado— es pagable (el bajo-umbral nunca se paga). Único dueño del neteo: lo
    /// usan el resumen display (<c>ConstruirResumenDesdeDatos</c>), la autorización
    /// (<c>AplicarResolucionPeriodoAsync</c>) y —vía el batch— el snapshot de nómina, así los tres
    /// nunca divergen de lo que Asistencia Semanal muestra.
    /// English: NetoVsNeto period net chain (shortage → late → early-leave → bank). POOL =
    /// extraDetectado (above threshold, payable) + extraBajoUmbral (below, NOT payable, only
    /// covers). One day's extra covers other days' deductions in order; the surplus replenishes
    /// consumed bank; the final surplus —capped at extraDetectado— is payable (below-threshold is
    /// never paid). Sole owner of the neteo: used by the display summary, authorization and —via
    /// the batch— the payroll snapshot, so all three never diverge from what Asistencia Semanal
    /// shows.
    /// </summary>
    public static (int FaltanteAbsorbido, int RetardoAbsorbido, int SalidaAbsorbido, int BancoRestaurado, int ExtraAbsorbible) CalcularNeteoNetoVsNeto(
        int extraDetectado, int extraBajoUmbral, int faltanteNeto, int retardo, int salidaAnticipada, int bancoConsumido)
    {
        var pool = extraDetectado + extraBajoUmbral;
        var faltanteAbsorbido = Math.Min(pool, faltanteNeto);
        var sobranteTrasFaltante = Math.Max(0, pool - faltanteNeto);
        var retardoAbsorbido = Math.Min(sobranteTrasFaltante, retardo);
        var sobranteTrasRetardo = Math.Max(0, sobranteTrasFaltante - retardo);
        var salidaAbsorbido = Math.Min(sobranteTrasRetardo, salidaAnticipada);
        var sobranteTrasSalida = Math.Max(0, sobranteTrasRetardo - salidaAnticipada);
        var bancoRestaurado = Math.Min(sobranteTrasSalida, bancoConsumido);
        var extraAbsorbible = Math.Min(Math.Max(0, sobranteTrasSalida - bancoConsumido), extraDetectado);
        return (faltanteAbsorbido, retardoAbsorbido, salidaAbsorbido, bancoRestaurado, extraAbsorbible);
    }

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
    // El permiso cubre el faltante pero NO suma sobre la jornada neta planeada.
    public static int ObtenerMinutosTiempoVisible(RrhhAsistencia asistencia, int minutosPermisoConGoceDia, int minutosCompensadosAprobados)
    {
        var basePagada = ObtenerMinutosBasePagada(asistencia);
        var permisoVisible = Math.Max(0, minutosPermisoConGoceDia) + Math.Max(0, asistencia.MinutosCubiertosBancoHoras);
        var tope = asistencia.MinutosJornadaNetaProgramada;
        var exceso = tope > 0 ? Math.Max(0, basePagada + permisoVisible - tope) : 0;
        permisoVisible = Math.Max(0, permisoVisible - exceso);

        return Math.Max(0, basePagada
            + permisoVisible
            + Math.Max(0, minutosCompensadosAprobados)
            + ObtenerMinutosExtraAprobados(asistencia));
    }

    // Sobrecarga conservada para callers que no pasan permiso con goce explícito
    // (el banco-cobertura sigue sumándose internamente). Redirige a la canonical.
    public static int ObtenerMinutosTiempoVisible(RrhhAsistencia asistencia, int minutosCompensadosAprobados)
        => ObtenerMinutosTiempoVisible(asistencia, 0, minutosCompensadosAprobados);

    public static int ObtenerMinutosAusenciaBrutaSugerida(RrhhAsistencia asistencia)
        => Math.Max(0, asistencia.MinutosJornadaProgramada - asistencia.MinutosTrabajadosBrutos);

    public static int ObtenerMinutosFaltanteNeto(RrhhAsistencia asistencia)
        // Faltante = ausencia genuina only. La tardanza (retardo) y la salida anticipada ya
        // tienen su propio bucket de descuento (ObtenerMinutosRetardoEfectivos /
        // ObtenerMinutosSalidaAnticipadaEfectivos), así que se restan aquí para no
        // contarlos dos veces en el neteo semanal, el descuento de salario y el déficit
        // del Permiso por Diferencia. Espejo del fix de tolerancia: la tardanza tolerada
        // (MinutosToleranciaRetardoAplicada) ya vive dentro de NetoEfectivo y el campo
        // MinutosRetardo queda 0, así que aquí sólo se excluye la tardanza NO tolerada.
        // English: Faltante = genuine absence only. Lateness (retardo) and early-leave
        // (salida anticipada) already have their own deduction buckets, so they are
        // subtracted here to avoid double-counting them in the weekly neteo, salary
        // discount and PermisoDiferencia deficit. Mirrors the tolerance fix: tolerated
        // lateness already lives inside NetoEfectivo (MinutosRetardo is 0), so only the
        // non-tolerated lateness is excluded here.
        => Math.Max(0, asistencia.MinutosJornadaNetaProgramada
            - ObtenerMinutosNetoEfectivo(asistencia)
            - ObtenerMinutosRetardoEfectivos(asistencia)
            - ObtenerMinutosSalidaAnticipadaEfectivos(asistencia));

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
