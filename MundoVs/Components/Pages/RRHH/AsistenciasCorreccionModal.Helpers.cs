using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Core.Models;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Components.Pages.RRHH;

public partial class AsistenciasCorreccionModal
{
    #region Formato y métricas estáticas

    private static string ObtenerTextoEstatus(RrhhAsistenciaEstatus estatus)
        => estatus switch
        {
            RrhhAsistenciaEstatus.AsistenciaNormal => "Normal",
            RrhhAsistenciaEstatus.Retardo => "Retardo",
            RrhhAsistenciaEstatus.Falta => "Falta",
            RrhhAsistenciaEstatus.Descanso => "Descanso",
            RrhhAsistenciaEstatus.DescansoTrabajado => "Descanso trabajado",
            RrhhAsistenciaEstatus.Incompleta => "Incompleta",
            RrhhAsistenciaEstatus.TurnoNoAsignado => "Sin turno",
            RrhhAsistenciaEstatus.MarcaNoReconocida => "Marca no reconocida",
            _ => "Pendiente"
        };

    private static string ObtenerClaseEstatus(RrhhAsistencia asistencia)
    {
        if (asistencia.RequiereRevision)
        {
            return "bg-warning text-dark";
        }

        return asistencia.Estatus switch
        {
            RrhhAsistenciaEstatus.AsistenciaNormal => "bg-success",
            RrhhAsistenciaEstatus.Retardo => "bg-warning text-dark",
            RrhhAsistenciaEstatus.Falta => "bg-danger",
            RrhhAsistenciaEstatus.Descanso => "bg-secondary",
            RrhhAsistenciaEstatus.DescansoTrabajado => "bg-info text-dark",
            RrhhAsistenciaEstatus.Incompleta => "bg-warning text-dark",
            RrhhAsistenciaEstatus.TurnoNoAsignado => "bg-secondary",
            RrhhAsistenciaEstatus.MarcaNoReconocida => "bg-dark",
            _ => "bg-primary"
        };
    }

    private static string ObtenerClaseMetricaIncidencia(int minutos)
        => minutos > 0 ? "asis-chip--warn" : "asis-chip--muted";

    private static string ObtenerClaseMetricaExtra(int minutos)
        => minutos > 0 ? "asis-chip--success" : "asis-chip--muted";

    private static string FormatearMinutosCortos(int minutos)
        => minutos <= 0 ? "0 min" : $"{minutos} min";

    private static string FormatearMinutos(int minutos)
    {
        var horas = minutos / 60;
        var minutosRestantes = Math.Abs(minutos % 60);
        return $"{horas}:{minutosRestantes:00} h";
    }

    private static string FormatearMensajeBitacora(RrhhLogChecador log)
        => log.Mensaje.Replace("Se aplicó corrección de asistencia: ", string.Empty);

    private static string FormatearFechaBitacora(RrhhLogChecador log)
        => $"{log.FechaUtc.ToLocalTime():dd/MM/yyyy HH:mm} · {ObtenerUsuarioBitacora(log)}";

    private static string ObtenerUsuarioBitacora(RrhhLogChecador log)
    {
        if (string.IsNullOrWhiteSpace(log.Detalle))
        {
            return "desconocido";
        }

        var segmento = log.Detalle.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault(p => p.StartsWith("usuario=", StringComparison.OrdinalIgnoreCase));
        return string.IsNullOrWhiteSpace(segmento) ? "desconocido" : segmento.Split('=', 2)[1];
    }

    private static string ObtenerAuditoriaMarcacion(RrhhMarcacion marcacion)
    {
        var usuario = !string.IsNullOrWhiteSpace(marcacion.UpdatedBy)
            ? marcacion.UpdatedBy
            : (!string.IsNullOrWhiteSpace(marcacion.CreatedBy) ? marcacion.CreatedBy : "sistema");
        var fecha = marcacion.UpdatedAt ?? marcacion.CreatedAt;
        return $"{usuario} · {fecha.ToLocalTime():dd/MM HH:mm}";
    }

    private static string ObtenerObservacionPrincipal(RrhhAsistencia asistencia)
    {
        if (string.IsNullOrWhiteSpace(asistencia.Observaciones))
        {
            return "—";
        }

        var partes = asistencia.Observaciones
            .Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        return partes.Length == 0 ? asistencia.Observaciones : partes[0] + ".";
    }

    private static string? ObtenerObservacionSecundaria(RrhhAsistencia asistencia)
    {
        if (string.IsNullOrWhiteSpace(asistencia.Observaciones))
        {
            return null;
        }

        var partes = asistencia.Observaciones
            .Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (partes.Length <= 1)
        {
            return null;
        }

        return string.Join(" ", partes.Skip(1).Select(p => p + '.'));
    }

    private static string ConstruirResumenTurno(TurnoBase turno)
    {
        var dias = turno.Detalles
            .Where(d => d.Labora)
            .OrderBy(d => d.DiaSemana)
            .Select(d => $"{AbreviarDia(d.DiaSemana)} {FormatearHoraTurno(d.HoraEntrada)}-{FormatearHoraTurno(d.HoraSalida)}")
            .ToList();

        return dias.Count == 0 ? "Sin horario laborable." : string.Join(" · ", dias);
    }

    private static string AbreviarDia(DiaSemanaTurno dia) => dia switch
    {
        DiaSemanaTurno.Lunes => "Lun",
        DiaSemanaTurno.Martes => "Mar",
        DiaSemanaTurno.Miercoles => "Mié",
        DiaSemanaTurno.Jueves => "Jue",
        DiaSemanaTurno.Viernes => "Vie",
        DiaSemanaTurno.Sabado => "Sáb",
        DiaSemanaTurno.Domingo => "Dom",
        _ => dia.ToString()
    };

    private static string FormatearHoraTurno(TimeSpan? hora)
        => hora?.ToString("hh\\:mm") ?? "—";

    private static string ExtraerTextoBloque(string observaciones, int indice)
    {
        var segmento = observaciones[indice..];
        var punto = segmento.IndexOf('.');
        return (punto >= 0 ? segmento[..punto] : segmento).Trim();
    }

    #endregion

    #region Zona horaria

    private static DateTime ObtenerFechaHoraLocalMarcacion(RrhhMarcacion marcacion)
    {
        if (marcacion.FechaHoraMarcacionLocal.HasValue)
        {
            return DateTime.SpecifyKind(marcacion.FechaHoraMarcacionLocal.Value, DateTimeKind.Unspecified);
        }

        var zonaHoraria = string.IsNullOrWhiteSpace(marcacion.ZonaHorariaAplicada)
            ? marcacion.Checador?.ZonaHoraria
            : marcacion.ZonaHorariaAplicada;

        return ConvertirLocalMarcacionDesdeUtc(marcacion.FechaHoraMarcacionUtc, zonaHoraria);
    }

    private static string ObtenerZonaHorariaAplicada(string? zonaHoraria)
        => string.IsNullOrWhiteSpace(zonaHoraria)
            ? ResolverZonaHoraria(null).Id
            : zonaHoraria.Trim();

    private static DateTime ConvertirLocalMarcacionDesdeUtc(DateTime fechaHoraUtc, string? zonaHoraria)
    {
        var utc = fechaHoraUtc.Kind == DateTimeKind.Utc ? fechaHoraUtc : DateTime.SpecifyKind(fechaHoraUtc, DateTimeKind.Utc);
        return DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(utc, ResolverZonaHoraria(zonaHoraria)), DateTimeKind.Unspecified);
    }

    private static DateTime ConvertirLocalChecadorAUtc(DateTime fechaLocal, string? zonaHoraria)
        => TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(fechaLocal, DateTimeKind.Unspecified), ResolverZonaHoraria(zonaHoraria));

    private static TimeZoneInfo ResolverZonaHoraria(string? zonaHoraria)
    {
        const string defaultMexicoTimeZone = "Central Standard Time (Mexico)";

        if (string.IsNullOrWhiteSpace(zonaHoraria))
        {
            return ResolverZonaHoraria(defaultMexicoTimeZone);
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

    private static readonly Dictionary<string, string> TimeZoneAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["America/Mexico_City"] = "Central Standard Time (Mexico)",
        ["Etc/UTC"] = "UTC"
    };

    #endregion

    #region Métricas de tiempo

    private int ObtenerMinutosBasePagada(RrhhAsistencia asistencia)
        => RrhhTiempoExtraPolicy.ObtenerMinutosBasePagada(asistencia);

    private int ObtenerMinutosRetardoVisible(RrhhAsistencia asistencia)
    {
        var permisoAplicadoActual = permisoDiaSeleccionado == null
            ? 0
            : RrhhTiempoExtraPolicy.ObtenerMinutosPermisoConGocePorDia(permisoDiaSeleccionado);
        return RrhhTiempoExtraPolicy.ObtenerMinutosRetardoEfectivos(asistencia, permisoAplicadoActual);
    }

    private int ObtenerMinutosSalidaAnticipadaVisible(RrhhAsistencia asistencia)
        => RrhhTiempoExtraPolicy.ObtenerMinutosSalidaAnticipadaEfectivos(asistencia);

    private int ObtenerMinutosTiempoVisible(RrhhAsistencia asistencia)
    {
        var permisoAplicadoActual = permisoDiaSeleccionado == null
            ? 0
            : permisoDiaSeleccionado.ConGocePago
                ? RrhhTiempoExtraPolicy.ObtenerMinutosPermisoConGocePorDia(permisoDiaSeleccionado)
                : 0;
        return RrhhTiempoExtraPolicy.ObtenerMinutosTiempoVisible(asistencia, permisoAplicadoActual, minutosCompensadosPermisoAprobados);
    }

    private int ObtenerMinutosDescansoNoPagadoProgramado(RrhhAsistencia asistencia)
        => RrhhTiempoExtraPolicy.ObtenerMinutosDescansoNoPagadoProgramado(asistencia);

    private int ObtenerMinutosDescansoNoPagadoExcluidosDelPermiso(RrhhAsistencia asistencia)
        => RrhhTiempoExtraPolicy.ObtenerMinutosDescansoNoPagadoExcluidosDelPermiso(asistencia);

    private static bool TieneSenalRevisionOperativa(RrhhAsistencia asistencia)
        => ObtenerSenalRevisionOperativa(asistencia) is not null;

    private static string? ObtenerSenalRevisionOperativa(RrhhAsistencia asistencia)
    {
        var observaciones = asistencia.Observaciones ?? string.Empty;
        if (observaciones.Contains("cambio de turno", StringComparison.OrdinalIgnoreCase))
        {
            return "Posible cambio de turno";
        }

        if (observaciones.Contains("jornada extraordinaria", StringComparison.OrdinalIgnoreCase)
            || (observaciones.Contains("Entrada anticipada", StringComparison.OrdinalIgnoreCase)
                && asistencia.MinutosExtra > 0))
        {
            return "Jornada irregular";
        }

        return null;
    }

    #endregion

    #region Resumen visual y cálculo

    private IReadOnlyList<ResumenVisualBarra> ObtenerBarrasResumen()
    {
        if (AsistenciaActual == null)
        {
            return [];
        }

        var permisoMinutos = permisoDiaSeleccionado != null
            ? ObtenerMinutosPermisoCapturados()
            : ObtenerMinutosPermisoSugeridos(AsistenciaActual);
        var compensacionSugerida = ObtenerMinutosCompensacionPermisoSugeridosAprobacion();
        var compensacionVisible = TieneCompensacionPermisoAprobada()
            ? minutosCompensadosPermisoAprobados
            : compensacionSugerida;

        return
        [
            new ResumenVisualBarra(
                "base",
                "Tiempo trabajado",
                "bi bi-stopwatch",
                "asis-time-bar__fill--base",
                ObtenerMinutosBasePagada(AsistenciaActual),
                "Base trabajada sin extra no aprobada.",
                null),
            new ResumenVisualBarra(
                "extra",
                "Tiempo extra",
                "bi bi-plus-circle",
                "asis-time-bar__fill--extra",
                AsistenciaActual.MinutosExtra,
                AsistenciaActual.MinutosExtra > 0
                    ? $"Resoluble: {FormatearMinutos(ObtenerMinutosResolubles(AsistenciaActual))}."
                    : "No se detecta extra pendiente.",
                ObtenerMinutosExtraAprobados(AsistenciaActual) > 0
                    ? $"Aprobada: {FormatearMinutos(ObtenerMinutosExtraAprobados(AsistenciaActual))}"
                    : null),
            new ResumenVisualBarra(
                "compensacion",
                "Compensación",
                "bi bi-arrow-left-right",
                "asis-time-bar__fill--compensacion",
                compensacionVisible,
                ObtenerResumenCompensacionPermiso(),
                TieneCompensacionPermisoAprobada()
                    ? $"Aprobada: {FormatearMinutos(minutosCompensadosPermisoAprobados)}"
                    : (compensacionSugerida > 0 ? $"Sugerida: {FormatearMinutos(compensacionSugerida)}" : null)),
            new ResumenVisualBarra(
                "permiso",
                "Permiso",
                "bi bi-calendar-x",
                "asis-time-bar__fill--permiso",
                permisoMinutos,
                ObtenerResumenPermisoSugerido(),
                permisoDiaSeleccionado != null
                    ? $"Capturado: {FormatearMinutos(ObtenerMinutosPermisoCapturados())}"
                    : (permisoMinutos > 0 ? $"Sugerido: {FormatearMinutos(permisoMinutos)}" : null)),
            new ResumenVisualBarra(
                "faltante",
                "Faltante neto",
                "bi bi-exclamation-circle",
                "asis-time-bar__fill--faltante",
                ObtenerMinutosFaltanteNeto(AsistenciaActual),
                "Solo debe cubrirse si sigue existiendo después de revisar marcaciones, permiso y compensación.",
                null)
        ];
    }

    private decimal ObtenerPorcentajeBarraResumen(int minutos)
    {
        if (AsistenciaActual == null || minutos <= 0)
        {
            return 0;
        }

        var referencia = new[]
        {
            AsistenciaActual.MinutosJornadaNetaProgramada,
            ObtenerMinutosTiempoVisible(AsistenciaActual),
            ObtenerMinutosBasePagada(AsistenciaActual),
            AsistenciaActual.MinutosExtra,
            ObtenerMinutosPermisoSugeridos(AsistenciaActual),
            ObtenerMinutosPermisoCapturados(),
            minutosCompensadosPermisoAprobados,
            ObtenerMinutosCompensacionPermisoSugeridosAprobacion(),
            ObtenerMinutosFaltanteNeto(AsistenciaActual),
            1
        }.Max();

        var porcentaje = decimal.Round(minutos * 100m / referencia, 2, MidpointRounding.AwayFromZero);
        return Math.Clamp(porcentaje, 8m, 100m);
    }

    private string ObtenerResumenVisibleExplicado()
        => AsistenciaActual == null
            ? string.Empty
            : $"Base {FormatearMinutos(AsistenciaActual.MinutosJornadaNetaProgramada)} + compensación día {FormatearMinutos(ObtenerMinutosCompensadosAprobadosActual())} + extra aprobada {FormatearMinutos(ObtenerMinutosExtraAprobados(AsistenciaActual))}{(AsistenciaActual.MinutosPerdonadosManual > 0 ? $" · perdón manual {FormatearMinutos(AsistenciaActual.MinutosPerdonadosManual)}" : string.Empty)}.";

    // Cifras centrales del día (display only). Bruto y neto vienen de campos
    // persistidos por el procesador; visible del helper que ya alinea
    // permisoDiaSeleccionado + compensación aprobada; pagado = visible menos el
    // extra autorizado al banco de horas (parte que no se paga, se acumula).
    private DesgloseDiaCifras? ObtenerDesgloseBrutoNetoVisiblePagado()
    {
        if (AsistenciaActual == null)
        {
            return null;
        }

        var bruto = Math.Max(0, AsistenciaActual.MinutosTrabajadosBrutos);
        var neto = RrhhTiempoExtraPolicy.ObtenerMinutosNetoEfectivo(AsistenciaActual);
        var visible = ObtenerMinutosTiempoVisible(AsistenciaActual);
        var extraAlBanco = Math.Max(0, AsistenciaActual.MinutosExtraAutorizadosBanco);
        var pagado = Math.Max(0, visible - extraAlBanco);
        return new DesgloseDiaCifras(bruto, neto, visible, pagado);
    }

    // Deltas legibles bruto→neto→visible. Bruto→neto: lo que el procesador descontó
    // (descansos no pagados, salidas temporales, no considerados) ya viene reflejado
    // en MinutosTrabajadosNetos; se muestra como una sola resta. Neto→visible: lo
    // que el policy suma sobre la base (permiso visible, compensación aprobada,
    // extra aprobado). Sólo se incluyen los deltas no nulos.
    private IReadOnlyList<DeltaDiaLegible> ObtenerDeltasDiaLegibles()
    {
        if (AsistenciaActual == null)
        {
            return [];
        }

        var bruto = Math.Max(0, AsistenciaActual.MinutosTrabajadosBrutos);
        var neto = RrhhTiempoExtraPolicy.ObtenerMinutosNetoEfectivo(AsistenciaActual);
        var deltas = new List<DeltaDiaLegible>();

        var descansosReduccion = Math.Max(0, bruto - neto);
        if (descansosReduccion > 0)
        {
            deltas.Add(new DeltaDiaLegible("descansos / no considerados", descansosReduccion, EsSuma: false));
        }

        var permisoConGoceDia = permisoDiaSeleccionado == null
            ? 0
            : permisoDiaSeleccionado.ConGocePago
                ? RrhhTiempoExtraPolicy.ObtenerMinutosPermisoConGocePorDia(permisoDiaSeleccionado)
                : 0;
        var permisoVisible = RrhhTiempoExtraPolicy.ObtenerMinutosPermisoVisible(AsistenciaActual, permisoConGoceDia);
        if (permisoVisible > 0)
        {
            deltas.Add(new DeltaDiaLegible("permiso visible", permisoVisible, EsSuma: true));
        }

        var compensacion = ObtenerMinutosCompensadosAprobadosActual();
        if (compensacion > 0)
        {
            deltas.Add(new DeltaDiaLegible("compensación", compensacion, EsSuma: true));
        }

        var extra = ObtenerMinutosExtraAprobados(AsistenciaActual);
        if (extra > 0)
        {
            deltas.Add(new DeltaDiaLegible("extra aprobado", extra, EsSuma: true));
        }

        return deltas;
    }

    // Barra de fórmula legible del día: Bruto − descansos = Neto + permiso + compensación + extra
    // = Visible − banco = Pagado. Reusa los mismos valores que ObtenerDeltasDiaLegibles y
    // ObtenerDesgloseBrutoNetoVisiblePagado para no duplicar lógica. Sólo incluye términos no
    // nulos; el primer término (Bruto) lleva signo vacío. Visible→Pagado resta el extra
    // autorizado al banco (MinutosExtraAutorizadosBanco), que no se paga como dinero.
    private IReadOnlyList<FormulaTermino> ObtenerFormulaDiaLegible()
    {
        if (AsistenciaActual == null)
        {
            return [];
        }

        var cifras = ObtenerDesgloseBrutoNetoVisiblePagado();
        if (cifras == null)
        {
            return [];
        }

        var terminos = new List<FormulaTermino>
        {
            new("Bruto", cifras.Bruto, string.Empty)
        };

        var descansosReduccion = Math.Max(0, cifras.Bruto - cifras.Neto);
        if (descansosReduccion > 0)
        {
            terminos.Add(new FormulaTermino("descansos", descansosReduccion, "−"));
        }

        terminos.Add(new FormulaTermino("Neto", cifras.Neto, "="));

        var permisoConGoceDia = permisoDiaSeleccionado == null
            ? 0
            : permisoDiaSeleccionado.ConGocePago
                ? RrhhTiempoExtraPolicy.ObtenerMinutosPermisoConGocePorDia(permisoDiaSeleccionado)
                : 0;
        var permisoVisible = RrhhTiempoExtraPolicy.ObtenerMinutosPermisoVisible(AsistenciaActual, permisoConGoceDia);
        if (permisoVisible > 0)
        {
            terminos.Add(new FormulaTermino("permiso", permisoVisible, "+"));
        }

        var compensacion = ObtenerMinutosCompensadosAprobadosActual();
        if (compensacion > 0)
        {
            terminos.Add(new FormulaTermino("compensación", compensacion, "+"));
        }

        var extra = ObtenerMinutosExtraAprobados(AsistenciaActual);
        if (extra > 0)
        {
            terminos.Add(new FormulaTermino("extra", extra, "+"));
        }

        terminos.Add(new FormulaTermino("Visible", cifras.Visible, "="));

        var extraAlBanco = Math.Max(0, AsistenciaActual.MinutosExtraAutorizadosBanco);
        if (extraAlBanco > 0)
        {
            terminos.Add(new FormulaTermino("banco", extraAlBanco, "−"));
        }

        terminos.Add(new FormulaTermino("Pagado", cifras.Pagado, "="));
        return terminos;
    }

    private IReadOnlyList<ResumenCalculoItem> ObtenerResumenCalculoDia()
    {
        if (AsistenciaActual == null)
        {
            return [];
        }

        var items = new List<ResumenCalculoItem>
        {
            new(
                "Jornada principal",
                FormatearRangoPrincipalJornada(),
                $"Se usa el bloque que mejor coincide con el turno del día: {ObtenerHorarioTurnoSeleccionadoDia().Replace("Día de descanso", "sin horario")}."),
            new(
                "Trabajo neto detectado",
                FormatearMinutos(AsistenciaActual.MinutosTrabajadosNetos),
                "Incluye jornada principal y tiempo adicional detectado; ya descuenta descansos no pagados aplicados."),
            new(
                "Descansos aplicados",
                FormatearMinutos(AsistenciaActual.MinutosDescansoTomado),
                string.IsNullOrWhiteSpace(AsistenciaActual.ResumenDescansos) ? "Sin detalle de descansos." : AsistenciaActual.ResumenDescansos,
                AsistenciaActual.MinutosDescansoTomado > 0 ? "asis-calculation-grid__item--warn" : null),
            new(
                "Tiempo visible",
                FormatearMinutos(ObtenerMinutosTiempoVisible(AsistenciaActual)),
                ObtenerResumenVisibleExplicado(),
                "asis-calculation-grid__item--info")
        };

        var bloquesAdicionales = ObtenerResumenBloquesAdicionales();
        if (bloquesAdicionales != null)
        {
            items.Insert(1, bloquesAdicionales);
        }

        if (AsistenciaActual.MinutosExtra > 0 || ObtenerMinutosExtraAprobados(AsistenciaActual) > 0)
        {
            items.Add(new ResumenCalculoItem(
                "Tiempo extra",
                FormatearMinutos(AsistenciaActual.MinutosExtra),
                ObtenerMinutosExtraAprobados(AsistenciaActual) > 0
                    ? $"Aprobada: {FormatearMinutos(ObtenerMinutosExtraAprobados(AsistenciaActual))}."
                    : "Detectada y pendiente de resolución.",
                "asis-calculation-grid__item--accent"));
        }

        items.Add(new ResumenCalculoItem(
            "Destino extra / banco",
            ObtenerResumenDestinoTiempoActual(),
            ObtenerResumenSaldoBancoHoras(),
            "asis-calculation-grid__item--info"));

        if (ObtenerMinutosCompensadosAprobadosActual() > 0)
        {
            items.Add(new ResumenCalculoItem(
                "Compensación del día",
                FormatearMinutos(ObtenerMinutosCompensadosAprobadosActual()),
                "Se suma al visible solo cuando la compensación ya fue aprobada.",
                "asis-calculation-grid__item--success"));
        }

        return items;
    }

    private ResumenCalculoItem? ObtenerResumenBloquesAdicionales()
    {
        if (AsistenciaActual == null)
        {
            return null;
        }

        var observaciones = AsistenciaActual.Observaciones ?? string.Empty;
        var bloques = new List<string>();

        var indicePrevio = observaciones.IndexOf("bloque previo al turno de ", StringComparison.OrdinalIgnoreCase);
        if (indicePrevio >= 0)
        {
            bloques.Add(ExtraerTextoBloque(observaciones, indicePrevio));
        }

        var indicePosterior = observaciones.IndexOf("bloque posterior al turno de ", StringComparison.OrdinalIgnoreCase);
        if (indicePosterior >= 0)
        {
            bloques.Add(ExtraerTextoBloque(observaciones, indicePosterior));
        }

        if (bloques.Count == 0)
        {
            return null;
        }

        return new ResumenCalculoItem(
            "Bloques adicionales",
            string.Join(" · ", bloques),
            "Se tomaron fuera de la jornada principal y se revisan como tiempo adicional del día.",
            "asis-calculation-grid__item--accent");
    }

    private string FormatearRangoPrincipalJornada()
        => AsistenciaActual == null
            ? "—"
            : $"{(AsistenciaActual.HoraEntradaReal?.ToString("hh\\:mm") ?? "—")} → {(AsistenciaActual.HoraSalidaReal?.ToString("hh\\:mm") ?? "—")}";

    #endregion

    #region Turno

    private string ObtenerResumenTurnoActual(RrhhAsistencia asistencia)
    {
        if (!asistencia.TurnoBaseId.HasValue)
        {
            return "Sin detalle del turno.";
        }

        var turno = Turnos.FirstOrDefault(t => t.Id == asistencia.TurnoBaseId.Value);
        return turno == null ? "Sin detalle del turno." : ConstruirResumenTurno(turno);
    }

    private string ObtenerHorarioTurnoSeleccionadoDia()
    {
        var detalle = ObtenerDetalleTurnoSeleccionadoDia();
        if (detalle == null)
        {
            return "Sin horario configurado para este día.";
        }

        return detalle.Labora
            ? $"{FormatearHoraTurno(detalle.HoraEntrada)} - {FormatearHoraTurno(detalle.HoraSalida)}"
            : "Día de descanso";
    }

    private string ObtenerDescansosTurnoSeleccionadoDia()
    {
        var detalle = ObtenerDetalleTurnoSeleccionadoDia();
        if (detalle == null || !detalle.Labora || detalle.Descansos.Count == 0)
        {
            return "Sin descansos configurados.";
        }

        var descansos = detalle.Descansos
            .Where(d => d.HoraInicio.HasValue && d.HoraFin.HasValue)
            .OrderBy(d => d.Orden)
            .Select(d => $"D{d.Orden} {FormatearHoraTurno(d.HoraInicio)}-{FormatearHoraTurno(d.HoraFin)}{(d.EsPagado ? " pagado" : string.Empty)}")
            .ToList();

        return descansos.Count == 0 ? "Sin descansos configurados." : string.Join(" · ", descansos);
    }

    private async Task ResolverTurnoVigenteCacheAsync()
    {
        if (AsistenciaActual == null || _draftDb == null)
        {
            return;
        }

        var empleadoId = AsistenciaActual.EmpleadoId;
        var fecha = AsistenciaActual.Fecha;
        var vigencia = await _draftDb.RrhhEmpleadosTurno
            .AsNoTracking()
            .Include(v => v.TurnoBase)
                .ThenInclude(t => t.Detalles)
                    .ThenInclude(d => d.Descansos)
            .Where(v => v.EmpresaId == _empresaId
                && v.EmpleadoId == empleadoId
                && v.IsActive
                && v.VigenteDesde <= fecha
                && (v.VigenteHasta == null || v.VigenteHasta >= fecha))
            .OrderByDescending(v => v.VigenteDesde)
            .FirstOrDefaultAsync();

        var turno = vigencia?.TurnoBase;
        if (turno == null && AsistenciaActual.Empleado?.TurnoBase != null)
        {
            turno = AsistenciaActual.Empleado.TurnoBase;
        }

        if (turno == null && AsistenciaActual.TurnoBaseId.HasValue)
        {
            turno = await _draftDb.TurnosBase
                .AsNoTracking()
                .Include(t => t.Detalles)
                    .ThenInclude(d => d.Descansos)
                .FirstOrDefaultAsync(t => t.Id == AsistenciaActual.TurnoBaseId && t.EmpresaId == _empresaId);
        }

        if (turno != null)
        {
            AsistenciaActual.TurnoBaseId = turno.Id;
            _detalleTurnoVigenteCache = turno.Detalles.FirstOrDefault(d => d.DiaSemana == MapDiaSemana(fecha.DayOfWeek));
            turnoDiaSeleccionadoIdTexto = turno.Id.ToString();
        }
    }

    private static DiaSemanaTurno MapDiaSemana(DayOfWeek dayOfWeek)
        => dayOfWeek switch
        {
            DayOfWeek.Monday => DiaSemanaTurno.Lunes,
            DayOfWeek.Tuesday => DiaSemanaTurno.Martes,
            DayOfWeek.Wednesday => DiaSemanaTurno.Miercoles,
            DayOfWeek.Thursday => DiaSemanaTurno.Jueves,
            DayOfWeek.Friday => DiaSemanaTurno.Viernes,
            DayOfWeek.Saturday => DiaSemanaTurno.Sabado,
            _ => DiaSemanaTurno.Domingo
        };

    private TurnoBaseDetalle? ObtenerDetalleTurnoSeleccionadoDia()
    {
        if (AsistenciaActual == null)
        {
            return null;
        }

        if (_detalleTurnoVigenteCache != null)
        {
            return _detalleTurnoVigenteCache;
        }

        var turnoIdTexto = string.IsNullOrWhiteSpace(turnoDiaSeleccionadoIdTexto)
            ? AsistenciaActual.TurnoBaseId?.ToString()
            : turnoDiaSeleccionadoIdTexto;

        if (!Guid.TryParse(turnoIdTexto, out var turnoId) || turnoId == Guid.Empty)
        {
            return null;
        }

        var turno = Turnos.FirstOrDefault(t => t.Id == turnoId);
        if (turno == null && _draftDb != null)
        {
            turno = _draftDb.TurnosBase
                .AsNoTracking()
                .Include(t => t.Detalles)
                    .ThenInclude(d => d.Descansos)
                .FirstOrDefault(t => t.Id == turnoId && t.EmpresaId == _empresaId);
        }

        if (turno == null)
        {
            return null;
        }

        var dia = AsistenciaActual.Fecha.DayOfWeek switch
        {
            DayOfWeek.Monday => DiaSemanaTurno.Lunes,
            DayOfWeek.Tuesday => DiaSemanaTurno.Martes,
            DayOfWeek.Wednesday => DiaSemanaTurno.Miercoles,
            DayOfWeek.Thursday => DiaSemanaTurno.Jueves,
            DayOfWeek.Friday => DiaSemanaTurno.Viernes,
            DayOfWeek.Saturday => DiaSemanaTurno.Sabado,
            _ => DiaSemanaTurno.Domingo
        };

        return turno.Detalles.FirstOrDefault(d => d.DiaSemana == dia);
    }

    #endregion

    #region Bitácora y estado

    private string ObtenerUltimaCorreccionResumen()
    {
        var ultimo = bitacoraCorreccionDia.FirstOrDefault();
        if (ultimo == null)
        {
            return "Sin correcciones registradas.";
        }

        return $"{ultimo.FechaUtc.ToLocalTime():dd/MM/yyyy HH:mm} · {ObtenerUsuarioBitacora(ultimo)}";
    }

    private string ObtenerBitacoraCorreccionResumen()
    {
        if (bitacoraCorreccionDia.Count == 0)
        {
            return "Sin movimientos en la bitácora del día.";
        }

        return string.Join(" | ", bitacoraCorreccionDia.Take(3).Select(l => $"{l.FechaUtc.ToLocalTime():HH:mm} {ObtenerUsuarioBitacora(l)}: {l.Mensaje.Replace("Se aplicó corrección de asistencia: ", string.Empty)}"));
    }

    private void AlternarBitacora()
        => _mostrarBitacora = !_mostrarBitacora;

    private string ObtenerEstadoCorreccionActual()
    {
        if (AsistenciaActual == null)
        {
            return "Sin datos";
        }

        if (permisoDiaSeleccionado != null)
        {
            return "Permiso aplicado";
        }

        if (TieneConfirmacionSalidaTempranaCompensaDescanso())
        {
            return "Regla confirmada";
        }

        if (RrhhTiempoExtraPolicy.TieneResolucionTiempoAplicada(AsistenciaActual))
        {
            return "Tiempo resuelto";
        }

        if (!RrhhTiempoExtraPolicy.TieneResolucionOperativaPendiente(AsistenciaActual, resumenAusenciaActual))
        {
            return "Sin pendiente";
        }

        if (asesorCorreccionActual?.PriorizarMarcaciones == true)
        {
            return "Corregir marcaciones";
        }

        if (asesorCorreccionActual?.PriorizarPermiso == true)
        {
            return "Falta permiso o turno";
        }

        if (asesorCorreccionActual?.PriorizarTiempo == true)
        {
            return "Resolver tiempo";
        }

        return "Revisar día";
    }

    private string ObtenerClaseEstadoCorreccionActual()
    {
        if (AsistenciaActual == null)
        {
            return "border-secondary-subtle bg-light";
        }

        if (permisoDiaSeleccionado != null || TieneConfirmacionSalidaTempranaCompensaDescanso() || RrhhTiempoExtraPolicy.TieneResolucionTiempoAplicada(AsistenciaActual))
        {
            return "border-success-subtle bg-success-subtle";
        }

        if (!RrhhTiempoExtraPolicy.TieneResolucionOperativaPendiente(AsistenciaActual, resumenAusenciaActual))
        {
            return "border-info-subtle bg-info-subtle";
        }

        return "border-warning-subtle bg-warning-subtle";
    }

    private string ObtenerSiguientePasoCorreccionActual()
        => asesorCorreccionActual?.AccionPrincipalTexto ?? "Revisar resumen";

    #endregion

    #region Asesor y wizard

    private string ObtenerDecisionRapidaWizard()
        => asesorCorreccionActual == null
            ? "Sin diagnóstico actual."
            : $"Ahora mismo te conviene: {ObtenerDecisionRapidaLegible()}.";

    private string ObtenerDecisionRapidaLegible()
        => asesorCorreccionActual?.Escenario == "CompensacionPermisoPendiente"
            ? "Revisar si la compensación realmente reduce el permiso o faltante"
            : (asesorCorreccionActual?.AccionPrincipalTexto ?? "Revisar el día");

    private void IrATabSugerida()
    {
        if (asesorCorreccionActual == null)
        {
            return;
        }

        if (asesorCorreccionActual.PriorizarPermiso)
        {
            _mostrarAccionesRapidasPermiso = true;
        }

        // PriorizarTiempo ya no abre panel local: el tiempo extra se autoriza por periodo en
        // Asistencias Semanal. El diagnóstico sigue visible en el asesor lateral como lectura.

        if (!string.IsNullOrWhiteSpace(asesorCorreccionActual.ResolucionSugerida))
        {
            tipoResolucionTiempoExtra = asesorCorreccionActual.ResolucionSugerida;
            AjustarResolucionTiempoSugerida();
        }
    }

    private void ActualizarAsesorCorreccion()
    {
        if (AsistenciaActual == null)
        {
            asesorCorreccionActual = null;
            return;
        }

        asesorCorreccionActual = CorreccionAdvisor.Analizar(
            AsistenciaActual,
            permisoDiaSeleccionado,
            minutosCompensadosPermisoAprobados,
            minutosRecuperablesPermisoAprobables,
            bancoHorasHabilitadoConfigurado,
            PuedeAprobarTiempoExtra,
            factorTiempoExtraConfigurado,
            saldoBancoHorasSeleccionado);

        if (asesorCorreccionActual.ResolucionesDisponibles.Count == 0)
        {
            tipoResolucionTiempoExtra = "SinAccion";
            minutosExtraPagoCaptura = 0;
            minutosExtraBancoCaptura = 0;
            minutosCubrirBancoCaptura = 0;
            return;
        }

        if (!asesorCorreccionActual.ResolucionesDisponibles.Any(o => o.Value == tipoResolucionTiempoExtra))
        {
            tipoResolucionTiempoExtra = asesorCorreccionActual.ResolucionesDisponibles.First().Value;
        }

        AjustarResolucionTiempoSugerida();
    }

    private IReadOnlyList<string> ObtenerReglasAyudaActual()
    {
        if (AsistenciaActual == null)
        {
            return [];
        }

        var reglas = new List<string>
        {
            $"La jornada neta programada del día es {FormatearMinutos(AsistenciaActual.MinutosJornadaNetaProgramada)} y el trabajo neto detectado es {FormatearMinutos(AsistenciaActual.MinutosTrabajadosNetos)}.",
            AsistenciaActual.MinutosDescansoProgramado > 0
                ? $"Los descansos no pagados se descuentan automáticamente según el turno. En este día se programaron {FormatearMinutos(AsistenciaActual.MinutosDescansoProgramado)} y se detectaron/aplicaron {FormatearMinutos(AsistenciaActual.MinutosDescansoTomado)}."
                : "Este turno no tiene descansos no pagados configurados para descontar."
        };

        if (asesorCorreccionActual?.Escenario == "SalidaTempranaCompensaDescanso")
        {
            reglas.Add("Cuando no se marca un descanso pero la salida anticipada cubre ese tramo, el sistema pide confirmación y no recomienda consumir banco ni registrar permiso de inmediato.");
        }
        else if (ObtenerMinutosFaltanteNeto(AsistenciaActual) > 0)
        {
            reglas.Add("El banco de horas solo debe usarse para cubrir faltante neto real; no para compensar un descanso que ya fue absorbido por una salida anticipada o por un permiso parcial válido.");
        }

        if (AsistenciaActual.MinutosExtra > 0)
        {
            reglas.Add($"El tiempo extra solo se resuelve cuando realmente existe extra del día ({FormatearMinutos(AsistenciaActual.MinutosExtra)}). Si hay faltante o marcaciones dudosas, primero conviene corregir la causa raíz.");
        }

        if (permisoDiaSeleccionado != null)
        {
            reglas.Add(permisoDiaSeleccionado.DescuentaBancoHoras
                ? "El permiso parcial con goce descuenta saldo del banco de horas del empleado."
                : "Este permiso parcial no consume banco de horas.");
        }

        if (asesorCorreccionActual != null)
        {
            reglas.Add($"La pestaña sugerida cambia según el escenario detectado: {asesorCorreccionActual.Titulo}. La intención es llevarte primero a la acción más compatible con el caso.");
        }

        return reglas;
    }

    private IReadOnlyList<string> ObtenerReglasAyudaGenerales()
        =>
        [
            "Los descansos no pagados del turno pueden descontarse automáticamente aunque no existan marcaciones perfectas de descanso.",
            "Si el empleado salió antes y ese tiempo cubre un descanso no tomado, primero debe confirmarse esa regla antes de usar banco o registrar permiso por el mismo tramo.",
            "El banco de horas solo debe usarse para cubrir faltante neto real o para acumular tiempo extra autorizado, nunca para duplicar un ajuste ya absorbido por descanso o permiso.",
            "Los permisos parciales con goce descuentan banco de horas del empleado; los permisos sin goce no lo consumen.",
            "Cuando existen marcaciones incompletas o inconsistentes, primero conviene corregir checadas antes de aplicar decisiones de pago, banco o permiso."
        ];

    private void AbrirAyudaReglas()
        => mostrarAyudaReglas = true;

    private void CerrarAyudaReglas()
        => mostrarAyudaReglas = false;

    #endregion
}