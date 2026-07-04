using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Core.Models;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Components.Pages.RRHH;

public partial class AsistenciasCorreccionModal
{
    private int ObtenerDuracionBloquePrincipal()
    {
        var segmentos = ObtenerTimelineSegmentosDia()
            .Where(s => !s.EsReferenciaTurno)
            .OrderByDescending(s => s.Minutos)
            .ToList();

        return segmentos.Count == 0 ? 0 : segmentos[0].Minutos;
    }

    private bool PuedeEditarSegmento(TimelineSegmentoDia segmento)
        => PuedeReprocesar && !segmento.EsReferenciaTurno && segmento.MarcacionInicioId.HasValue && segmento.MarcacionFinId.HasValue;

    private bool EsSegmentoEnEdicion(TimelineSegmentoDia segmento)
        => segmentoEditandoInicioId == segmento.MarcacionInicioId && segmentoEditandoFinId == segmento.MarcacionFinId;

    private bool EsSegmentoGuardando(TimelineSegmentoDia segmento)
        => segmentoGuardandoInicioId == segmento.MarcacionInicioId && segmentoGuardandoFinId == segmento.MarcacionFinId;

    private void EditarSegmento(TimelineSegmentoDia segmento)
    {
        if (!PuedeEditarSegmento(segmento))
        {
            return;
        }

        segmentoEditandoInicioId = segmento.MarcacionInicioId;
        segmentoEditandoFinId = segmento.MarcacionFinId;
        segmentoAccionSeleccionada = ObtenerClaveAccionSegmento(segmento);
        segmentoMinutosAplicadosCaptura = segmento.Accion == "descanso"
            ? segmento.MinutosAplicados ?? segmento.Minutos
            : null;
    }

    private void CancelarEdicionSegmento()
    {
        segmentoEditandoInicioId = null;
        segmentoEditandoFinId = null;
        segmentoAccionSeleccionada = "trabajo";
        segmentoMinutosAplicadosCaptura = null;
    }

    private static string ObtenerClaveAccionSegmento(TimelineSegmentoDia segmento)
        => segmento.Accion;

    private static TipoSegmentoResolucionRrhh MapearTipoSegmentoResolucion(string accion)
        => accion switch
        {
            "extra" => TipoSegmentoResolucionRrhh.Extra,
            "descanso" => TipoSegmentoResolucionRrhh.Descanso,
            "descansoNoDescontar" => TipoSegmentoResolucionRrhh.DescansoNoDescontar,
            "temporal" => TipoSegmentoResolucionRrhh.SalidaTemporal,
            "permiso" => TipoSegmentoResolucionRrhh.Permiso,
            "ignorar" => TipoSegmentoResolucionRrhh.NoConsiderar,
            _ => TipoSegmentoResolucionRrhh.Trabajo
        };

    private IReadOnlyList<SegmentoAccionOpcion> ObtenerOpcionesAccionSegmento(TimelineSegmentoDia segmento)
    {
        if (segmento.EsReferenciaTurno)
        {
            return [];
        }

        return
        [
            new("trabajo", "Trabajo principal", "Usa Entrada + Salida para que el tramo se tome como parte de la jornada efectiva."),
            new("extra", "Bloque extra", "Se mapea igual que trabajo, pero el texto del timeline lo presenta como tramo adicional al turno."),
            new("descanso", "Descanso", "Usa InicioDescanso + FinDescanso para descontar el tramo como descanso."),
            new("descansoNoDescontar", "No descontar descanso", "El empleado no tomó el descanso pero ese tiempo se cuenta como trabajo efectivo; no se descuenta del neto trabajado."),
            new("temporal", "Salida temporal", "Se conserva fuera del tiempo laborado y no se trata como descanso; debe verse explícitamente como salida temporal."),
            new("permiso", "Permiso", "Marca el tramo como permiso del día para diferenciarlo de una salida temporal o de un descanso."),
            new("ignorar", "No considerar", "Saca el tramo del cálculo del día sin romper el timeline; seguirá visible como tramo no considerado.")
        ];
    }

    private string ObtenerAyudaAccionSegmentoSeleccionada(TimelineSegmentoDia segmento)
        => ObtenerOpcionesAccionSegmento(segmento).FirstOrDefault(o => o.Clave == segmentoAccionSeleccionada)?.Ayuda
            ?? "Selecciona cómo debe interpretarse este tramo del día.";

    private static string ObtenerEtiquetaSugerenciaAlternada(TimelineSegmentoDia segmento)
        => string.IsNullOrWhiteSpace(segmento.SugerenciaAlternada) ? string.Empty : segmento.SugerenciaAlternada!;

    private IReadOnlyList<TimelineSegmentoDia> ObtenerTimelineSegmentosDia()
    {
        if (AsistenciaActual == null || marcacionesDia.Count == 0)
        {
            return [];
        }

        var segmentos = new List<TimelineSegmentoDia>();
        var detalleTurno = ObtenerDetalleTurnoSeleccionadoDia();
        var marcacionesOrdenadas = marcacionesDia
            .Where(m => !m.EsAnulada)
            .OrderBy(ObtenerFechaHoraLocalMarcacion)
            .ToList();

        if (detalleTurno?.HoraEntrada is TimeSpan entradaProgramada && detalleTurno.HoraSalida is TimeSpan salidaProgramada)
        {
            var minutosTurno = Math.Max(1, (int)Math.Round((salidaProgramada - entradaProgramada).TotalMinutes));
            segmentos.Add(new TimelineSegmentoDia(
                "Turno esperado",
                $"{FormatearHoraTurno(detalleTurno.HoraEntrada)} → {FormatearHoraTurno(detalleTurno.HoraSalida)}",
                minutosTurno,
                100m,
                "asis-dayline__segment--turno",
                "Referencia del horario configurado para comparar bloques detectados.",
                null,
                null,
                null,
                null,
                null,
                true));
        }

        var referencia = Math.Max(1, marcacionesOrdenadas.Zip(marcacionesOrdenadas.Skip(1), (inicio, fin) => Math.Max(0, (int)Math.Round((ObtenerFechaHoraLocalMarcacion(fin) - ObtenerFechaHoraLocalMarcacion(inicio)).TotalMinutes))).DefaultIfEmpty(1).Max());

        for (var i = 0; i + 1 < marcacionesOrdenadas.Count; i++)
        {
            var inicioMarcacion = marcacionesOrdenadas[i];
            var finMarcacion = marcacionesOrdenadas[i + 1];
            var inicio = ObtenerFechaHoraLocalMarcacion(inicioMarcacion);
            var fin = ObtenerFechaHoraLocalMarcacion(finMarcacion);
            var minutos = Math.Max(0, (int)Math.Round((fin - inicio).TotalMinutes));
            if (minutos <= 0)
            {
                continue;
            }

            var clasificacion = ClasificarSegmentoDia(inicioMarcacion, finMarcacion, detalleTurno, inicio, fin, minutos, i);
            var sugerenciaAlternada = ObtenerSugerenciaAlternadaSegmento(i, clasificacion.Accion, clasificacion.EstadoResolucion, clasificacion.FueInferidoAutomaticamente, detalleTurno);
            var porcentaje = Math.Clamp(decimal.Round(minutos * 100m / referencia, 2, MidpointRounding.AwayFromZero), 10m, 100m);
            var (numeroDescanso, minutosProgramados, minutosAplicados, origenAplicado) = clasificacion.Accion == "descanso"
                ? ObtenerPresentacionDescanso(detalleTurno, inicioMarcacion.Id, finMarcacion.Id, inicio.TimeOfDay, fin.TimeOfDay, minutos)
                : (null, null, null, null);

            segmentos.Add(new TimelineSegmentoDia(
                clasificacion.Titulo,
                $"{inicio:HH:mm} → {fin:HH:mm}",
                minutos,
                porcentaje,
                clasificacion.CssClass,
                clasificacion.Detalle,
                sugerenciaAlternada,
                numeroDescanso,
                minutosProgramados,
                minutosAplicados,
                origenAplicado,
                false,
                inicioMarcacion.Id,
                finMarcacion.Id,
                clasificacion.ClasificacionInicio,
                clasificacion.ClasificacionFin,
                clasificacion.Accion,
                clasificacion.EstadoResolucion,
                clasificacion.FueInferidoAutomaticamente));
        }

        return segmentos;
    }

    private (string Titulo, string CssClass, string Detalle, TipoClasificacionMarcacionRrhh ClasificacionInicio, TipoClasificacionMarcacionRrhh ClasificacionFin, string Accion, EstadoSegmentoResolucionRrhh EstadoResolucion, bool FueInferidoAutomaticamente) ClasificarSegmentoDia(RrhhMarcacion inicio, RrhhMarcacion fin, TurnoBaseDetalle? detalleTurno, DateTime inicioLocal, DateTime finLocal, int minutos, int indiceSegmento)
    {
        var resolucion = ObtenerResolucionSegmento(inicio.Id, fin.Id);
        var accionPayload = resolucion == null
            ? RrhhMarcacionSegmentActionHelper.ResolveAction(inicio.PayloadRaw, fin.PayloadRaw)
            : MapearAccionSegmento(resolucion.TipoSegmento);
        var estadoResolucion = resolucion?.Estado ?? EstadoSegmentoResolucionRrhh.RequiereRevision;
        var fueInferidoAutomaticamente = resolucion?.FueInferidoAutomaticamente ?? false;
        var resolucionManualVigente = resolucion is { Estado: EstadoSegmentoResolucionRrhh.Vigente, FueInferidoAutomaticamente: false };

        if (inicio.EsAnulada || fin.EsAnulada)
        {
            return ("Tramo no considerado", "asis-dayline__segment--ignorado", "El tramo se excluye del cálculo del día, pero se mantiene visible en el timeline para contexto.", TipoClasificacionMarcacionRrhh.Entrada, TipoClasificacionMarcacionRrhh.Salida, "ignorar", estadoResolucion, fueInferidoAutomaticamente);
        }

        if (accionPayload == "permiso")
        {
            return ("Permiso", "asis-dayline__segment--permiso", "Tramo marcado como permiso; no se toma como descanso ni como trabajo efectivo.", TipoClasificacionMarcacionRrhh.InicioDescanso, TipoClasificacionMarcacionRrhh.FinDescanso, "permiso", estadoResolucion, fueInferidoAutomaticamente);
        }

        if (accionPayload == "temporal")
        {
            return ("Salida temporal", "asis-dayline__segment--temporal", "Tramo marcado como salida temporal fuera del horario laboral; no se trata como descanso.", TipoClasificacionMarcacionRrhh.InicioDescanso, TipoClasificacionMarcacionRrhh.FinDescanso, "temporal", estadoResolucion, fueInferidoAutomaticamente);
        }

        if (resolucionManualVigente && resolucion?.TipoSegmento == TipoSegmentoResolucionRrhh.Extra)
        {
            return ("Bloque extra", "asis-dayline__segment--extra", "Tramo fijado manualmente como tiempo adicional al turno; no debe volver a interpretarse como descanso.", TipoClasificacionMarcacionRrhh.Entrada, TipoClasificacionMarcacionRrhh.Salida, "extra", estadoResolucion, fueInferidoAutomaticamente);
        }

        if (resolucionManualVigente && resolucion?.TipoSegmento == TipoSegmentoResolucionRrhh.Trabajo)
        {
            return ("Trabajo principal", "asis-dayline__segment--trabajo", "Tramo fijado manualmente como parte de la jornada principal; no debe volver a interpretarse como descanso.", TipoClasificacionMarcacionRrhh.Entrada, TipoClasificacionMarcacionRrhh.Salida, "trabajo", estadoResolucion, fueInferidoAutomaticamente);
        }

        if (resolucionManualVigente && resolucion?.TipoSegmento == TipoSegmentoResolucionRrhh.DescansoNoDescontar)
        {
            return ("Descanso no descontado", "asis-dayline__segment--descanso-no-descontado", "Descanso que el empleado no tomó; el tiempo se cuenta como trabajo efectivo y no se descuenta del neto.", TipoClasificacionMarcacionRrhh.Entrada, TipoClasificacionMarcacionRrhh.Salida, "descansoNoDescontar", estadoResolucion, fueInferidoAutomaticamente);
        }

        if (inicio.ClasificacionOperativa == TipoClasificacionMarcacionRrhh.InicioDescanso || fin.ClasificacionOperativa == TipoClasificacionMarcacionRrhh.FinDescanso)
        {
            return ("Descanso detectado", "asis-dayline__segment--descanso", "Se interpreta como descanso marcado o emparejado en la jornada.", TipoClasificacionMarcacionRrhh.InicioDescanso, TipoClasificacionMarcacionRrhh.FinDescanso, "descanso", estadoResolucion, fueInferidoAutomaticamente);
        }

        if (DebeInferirseComoDescanso(detalleTurno, inicioLocal.TimeOfDay, finLocal.TimeOfDay, minutos))
        {
            return ("Descanso inferido", "asis-dayline__segment--descanso", "El tramo coincide con una ventana de descanso del turno y se presenta como descanso inferido automáticamente.", TipoClasificacionMarcacionRrhh.InicioDescanso, TipoClasificacionMarcacionRrhh.FinDescanso, "descanso", estadoResolucion, true);
        }

        var inicioHora = inicio.FechaHoraMarcacionLocal?.TimeOfDay;
        var finHora = fin.FechaHoraMarcacionLocal?.TimeOfDay;
        if (detalleTurno?.HoraEntrada is TimeSpan entradaProgramada && detalleTurno.HoraSalida is TimeSpan salidaProgramada && inicioHora.HasValue && finHora.HasValue)
        {
            if (finHora.Value <= entradaProgramada)
            {
                return ("Bloque previo", "asis-dayline__segment--extra", "Bloque detectado antes del inicio del turno; revisar si cuenta como tiempo adicional.", TipoClasificacionMarcacionRrhh.Entrada, TipoClasificacionMarcacionRrhh.Salida, "extra", estadoResolucion, fueInferidoAutomaticamente);
            }

            if (inicioHora.Value >= salidaProgramada)
            {
                return ("Bloque posterior", "asis-dayline__segment--extra", "Bloque detectado después del final del turno; revisar si cuenta como tiempo adicional.", TipoClasificacionMarcacionRrhh.Entrada, TipoClasificacionMarcacionRrhh.Salida, "extra", estadoResolucion, fueInferidoAutomaticamente);
            }
        }

        if (resolucion == null)
        {
            // Sin turno: el procesador asigna TODOS los pares intermedios como descansos
            // (no hay descansos configurados de referencia).
            // Con turno: TODOS los pares de marcas intermedias representan salida-regreso
            // del puesto, es decir, descansos. El trabajo está implícito entre los pares.
            // No hay alternancia pausa-trabajo entre pares: todos son pausa.
            if (detalleTurno == null)
            {
                return ("Descanso inferido", "asis-dayline__segment--descanso", "Sin turno configurado: todos los pares intermedios se interpretan como descansos tomados.", TipoClasificacionMarcacionRrhh.InicioDescanso, TipoClasificacionMarcacionRrhh.FinDescanso, "descanso", estadoResolucion, true);
            }

            return ("Descanso sugerido", "asis-dayline__segment--descanso", "Tramo intermedio: se espera como descanso (salida-regreso del puesto). Confirma si corresponde descanso, permiso o salida temporal.", TipoClasificacionMarcacionRrhh.InicioDescanso, TipoClasificacionMarcacionRrhh.FinDescanso, "descanso", estadoResolucion, true);
        }

        return ("Trabajo detectado", "asis-dayline__segment--trabajo", "Segmento tomado como parte de la jornada principal o del trabajo efectivo del día.", TipoClasificacionMarcacionRrhh.Entrada, TipoClasificacionMarcacionRrhh.Salida, "trabajo", estadoResolucion, fueInferidoAutomaticamente);
    }

    private static bool DebeInferirseComoDescanso(TurnoBaseDetalle? detalleTurno, TimeSpan inicio, TimeSpan fin, int minutos)
    {
        if (detalleTurno == null || minutos <= 0)
        {
            return false;
        }

        return ObtenerNumeroDescansoMasCercano(detalleTurno, inicio, fin).HasValue;
    }

    private static string? ObtenerSugerenciaAlternadaSegmento(int indiceSegmento, string accionActual, EstadoSegmentoResolucionRrhh estadoResolucion, bool fueInferidoAutomaticamente, TurnoBaseDetalle? detalleTurno)
    {
        if (estadoResolucion == EstadoSegmentoResolucionRrhh.Vigente && !fueInferidoAutomaticamente)
        {
            return null;
        }

        // Todos los pares de marcas intermedias representan salida-regreso del puesto,
        // es decir, descansos. El trabajo está implícito entre los pares.
        // No hay alternancia pausa-trabajo entre pares: todos son pausa.
        return accionActual is "descanso" or "descansoNoDescontar" or "permiso" or "temporal"
            ? "Esperado: descanso"
            : "Sugiere descanso; todos los pares intermedios se esperan como pausa (salida-regreso).";
    }

    private static bool MostrarSugerenciaAlternada(TimelineSegmentoDia segmento)
        => !string.IsNullOrWhiteSpace(segmento.SugerenciaAlternada)
            && segmento.SugerenciaAlternada.Contains("sugiere", StringComparison.OrdinalIgnoreCase);

    private static string ObtenerTituloVisualSegmento(TimelineSegmentoDia segmento, int indiceVisual)
        => segmento.EsReferenciaTurno
            ? "Turno esperado"
            : $"{indiceVisual}. {segmento.Titulo}";

    private static string ObtenerEstadoVisualSegmento(TimelineSegmentoDia segmento)
    {
        if (segmento.EstadoResolucion == EstadoSegmentoResolucionRrhh.Vigente)
        {
            return segmento.FueInferidoAutomaticamente ? "Inferido" : "Fijo";
        }

        return "Revisar";
    }

    private static string ObtenerClaseEstadoVisualSegmento(TimelineSegmentoDia segmento)
        => segmento.EstadoResolucion == EstadoSegmentoResolucionRrhh.Vigente
            ? "asis-dayline-list__status--ok"
            : "asis-dayline-list__status--warn";

    private static bool MostrarDetalleLargoSegmento(TimelineSegmentoDia segmento)
        => segmento.EsReferenciaTurno || segmento.Accion == "descanso" || segmento.EstadoResolucion != EstadoSegmentoResolucionRrhh.Vigente || segmento.FueInferidoAutomaticamente;

    private (int? NumeroDescanso, int? MinutosProgramados, int? MinutosAplicados, string? OrigenAplicado) ObtenerPresentacionDescanso(TurnoBaseDetalle? detalleTurno, Guid inicioId, Guid finId, TimeSpan inicio, TimeSpan fin, int minutosDetectados)
    {
        var resolucion = ObtenerResolucionSegmento(inicioId, finId);
        if (resolucion?.TipoSegmento == TipoSegmentoResolucionRrhh.DescansoNoDescontar)
        {
            var numeroNd = ObtenerNumeroDescansoMasCercano(detalleTurno, inicio, fin);
            var programadoNd = numeroNd.HasValue ? ObtenerMinutosProgramadosDescanso(detalleTurno, numeroNd.Value) : null;
            return (numeroNd, programadoNd, 0, "No descontado");
        }

        if (resolucion?.TipoSegmento == TipoSegmentoResolucionRrhh.Descanso && resolucion.MinutosAplicadosOverride.HasValue)
        {
            var numeroManual = ObtenerNumeroDescansoMasCercano(detalleTurno, inicio, fin);
            var programadoManual = numeroManual.HasValue ? ObtenerMinutosProgramadosDescanso(detalleTurno, numeroManual.Value) : null;
            return (numeroManual, programadoManual, resolucion.MinutosAplicadosOverride.Value, "Manual");
        }

        var numero = ObtenerNumeroDescansoMasCercano(detalleTurno, inicio, fin);
        if (!numero.HasValue)
        {
            return (null, null, minutosDetectados, "Detectado");
        }

        var minutosProgramados = ObtenerMinutosProgramadosDescanso(detalleTurno, numero.Value);
        var minutosAplicados = minutosDetectados;
        var origenAplicado = "Detectado";

        if (minutosProgramados.HasValue)
        {
            var umbral = Math.Max(0, _toleranciaExcesoDescansoMinutos);
            if (minutosDetectados <= minutosProgramados.Value + umbral)
            {
                minutosAplicados = minutosProgramados.Value;
                origenAplicado = "Programado";
            }
        }

        return (numero, minutosProgramados, minutosAplicados, origenAplicado);
    }

    private string ObtenerFormulaDescanso(TimelineSegmentoDia segmento)
    {
        if (segmento.Accion != "descanso" && segmento.Accion != "descansoNoDescontar")
        {
            return string.Empty;
        }

        if (!segmento.MinutosAplicados.HasValue)
        {
            return string.Empty;
        }

        if (string.Equals(segmento.OrigenAplicado, "No descontado", StringComparison.OrdinalIgnoreCase))
        {
            return segmento.MinutosProgramados.HasValue
                ? $"Cálculo: descanso D{segmento.NumeroDescanso?.ToString() ?? "?"} programado {FormatearMinutos(segmento.MinutosProgramados.Value)} no descontado; el tiempo se cuenta como trabajo efectivo."
                : "Cálculo: descanso no descontado; el tiempo se cuenta como trabajo efectivo.";
        }

        if (!segmento.MinutosProgramados.HasValue)
        {
            return $"Cálculo: sin descanso programado asociado, se aplica el real {FormatearMinutos(segmento.MinutosAplicados.Value)}.";
        }

        if (string.Equals(segmento.OrigenAplicado, "Manual", StringComparison.OrdinalIgnoreCase))
        {
            return $"Cálculo: override manual = {FormatearMinutos(segmento.MinutosAplicados.Value)}.";
        }

        var limite = segmento.MinutosProgramados.Value + Math.Max(0, _toleranciaExcesoDescansoMinutos);
        return segmento.MinutosAplicados.Value == segmento.MinutosProgramados.Value
            ? $"Cálculo: real {FormatearMinutos(segmento.Minutos)} ≤ programado {FormatearMinutos(segmento.MinutosProgramados.Value)} + umbral {FormatearMinutos(_toleranciaExcesoDescansoMinutos)} = {FormatearMinutos(limite)}; se aplica programado {FormatearMinutos(segmento.MinutosProgramados.Value)}."
            : $"Cálculo: real {FormatearMinutos(segmento.Minutos)} > programado {FormatearMinutos(segmento.MinutosProgramados.Value)} + umbral {FormatearMinutos(_toleranciaExcesoDescansoMinutos)} = {FormatearMinutos(limite)}; se aplica real {FormatearMinutos(segmento.MinutosAplicados.Value)}.";
    }

    private static int? ObtenerNumeroDescansoMasCercano(TurnoBaseDetalle? detalleTurno, TimeSpan inicio, TimeSpan fin)
    {
        if (detalleTurno == null)
        {
            return null;
        }

        var candidatos = detalleTurno.Descansos
            .Where(d => d.HoraInicio.HasValue && d.HoraFin.HasValue)
            .Select(d => new { Numero = (int)d.Orden, Inicio = d.HoraInicio!.Value, Fin = d.HoraFin!.Value })
            .ToList();

        var mejor = candidatos
            .Select(v => new
            {
                v.Numero,
                Duracion = Math.Max(0, (int)Math.Round((v.Fin - v.Inicio).TotalMinutes)),
                Diferencia = Math.Abs((int)Math.Round((inicio - v.Inicio).TotalMinutes)) + Math.Abs((int)Math.Round((fin - v.Fin).TotalMinutes))
            })
            .Where(v => v.Duracion <= 0 || minutosCoinciden(v.Duracion, inicio, fin))
            .OrderBy(v => v.Diferencia)
            .FirstOrDefault();

        return mejor?.Numero;

        static bool minutosCoinciden(int duracionProgramada, TimeSpan inicioBloque, TimeSpan finBloque)
        {
            var duracionBloque = Math.Max(0, (int)Math.Round((finBloque - inicioBloque).TotalMinutes));
            return duracionBloque <= duracionProgramada + 30;
        }
    }

    private static int? ObtenerMinutosProgramadosDescanso(TurnoBaseDetalle? detalleTurno, int numeroDescanso)
    {
        if (detalleTurno == null)
        {
            return null;
        }

        var descanso = detalleTurno.Descansos
            .FirstOrDefault(d => d.Orden == numeroDescanso && d.HoraInicio.HasValue && d.HoraFin.HasValue);

        return descanso == null
            ? null
            : Math.Max(0, (int)Math.Round((descanso.HoraFin!.Value - descanso.HoraInicio!.Value).TotalMinutes));
    }

    private RrhhSegmentoResolucion? ObtenerResolucionSegmento(Guid inicioId, Guid finId)
        => resolucionesSegmentoDia.FirstOrDefault(r => r.MarcacionInicioId == inicioId && r.MarcacionFinId == finId && r.Estado != EstadoSegmentoResolucionRrhh.Obsoleta);

    private static string MapearAccionSegmento(TipoSegmentoResolucionRrhh tipoSegmento)
        => tipoSegmento switch
        {
            TipoSegmentoResolucionRrhh.Extra => "extra",
            TipoSegmentoResolucionRrhh.Descanso => "descanso",
            TipoSegmentoResolucionRrhh.DescansoNoDescontar => "descansoNoDescontar",
            TipoSegmentoResolucionRrhh.SalidaTemporal => "temporal",
            TipoSegmentoResolucionRrhh.Permiso => "permiso",
            TipoSegmentoResolucionRrhh.NoConsiderar => "ignorar",
            _ => "trabajo"
        };

    private async Task AplicarEdicionSegmentoAsync(TimelineSegmentoDia segmento)
    {
        if (!PuedeEditarSegmento(segmento) || !segmento.MarcacionInicioId.HasValue || !segmento.MarcacionFinId.HasValue)
        {
            return;
        }

        if (segmentoAccionSeleccionada == "ignorar")
        {
            await AplicarIgnorarSegmentoAsync(segmento);
            return;
        }

        var (clasificacionInicio, clasificacionFin) = segmentoAccionSeleccionada switch
        {
            "descanso" => (TipoClasificacionMarcacionRrhh.InicioDescanso, TipoClasificacionMarcacionRrhh.FinDescanso),
            "descansoNoDescontar" => (TipoClasificacionMarcacionRrhh.Entrada, TipoClasificacionMarcacionRrhh.Salida),
            "temporal" => (TipoClasificacionMarcacionRrhh.InicioDescanso, TipoClasificacionMarcacionRrhh.FinDescanso),
            "permiso" => (TipoClasificacionMarcacionRrhh.InicioDescanso, TipoClasificacionMarcacionRrhh.FinDescanso),
            _ => (TipoClasificacionMarcacionRrhh.Entrada, TipoClasificacionMarcacionRrhh.Salida)
        };

        var mensaje = segmentoAccionSeleccionada switch
        {
            "descanso" => "Segmento marcado como descanso y día reprocesado.",
            "descansoNoDescontar" => "Segmento marcado como descanso no descontado; el tiempo se cuenta como trabajo efectivo.",
            "temporal" => "Segmento marcado como salida temporal y día reprocesado.",
            "permiso" => "Segmento marcado como permiso y día reprocesado.",
            "extra" => "Segmento marcado como bloque extra y día reprocesado.",
            _ => "Segmento marcado como trabajo principal y día reprocesado."
        };

        await AplicarCambioSegmentoAsync(
            segmento,
            (inicio, fin, usuarioActual, db) =>
            {
                inicio.ClasificacionOperativa = clasificacionInicio;
                inicio.TipoMarcacionRaw = clasificacionInicio.ToString();
                inicio.EsAnulada = false;
                inicio.PayloadRaw = RrhhMarcacionSegmentActionHelper.SetAction(inicio.PayloadRaw, segmentoAccionSeleccionada is "temporal" or "permiso" ? segmentoAccionSeleccionada : null);
                inicio.Procesada = false;
                inicio.UpdatedAt = DateTime.UtcNow;
                inicio.UpdatedBy = usuarioActual;

                fin.ClasificacionOperativa = clasificacionFin;
                fin.TipoMarcacionRaw = clasificacionFin.ToString();
                fin.EsAnulada = false;
                fin.PayloadRaw = RrhhMarcacionSegmentActionHelper.SetAction(fin.PayloadRaw, segmentoAccionSeleccionada is "temporal" or "permiso" ? segmentoAccionSeleccionada : null);
                fin.Procesada = false;
                fin.UpdatedAt = DateTime.UtcNow;
                fin.UpdatedBy = usuarioActual;

                var numeroDescanso = segmentoAccionSeleccionada is "descanso" or "descansoNoDescontar"
                    ? ObtenerNumeroDescansoMasCercano(ObtenerDetalleTurnoSeleccionadoDia(), ObtenerFechaHoraLocalMarcacion(inicio).TimeOfDay, ObtenerFechaHoraLocalMarcacion(fin).TimeOfDay)
                    : null;

                GuardarResolucionSegmento(db, usuarioActual, segmento, MapearTipoSegmentoResolucion(segmentoAccionSeleccionada), segmentoAccionSeleccionada == "ignorar"
                    ? "Bloque marcado para no considerar."
                    : $"Bloque fijado como {segmentoAccionSeleccionada}{(numeroDescanso.HasValue ? $" (D{numeroDescanso.Value})" : string.Empty)}.", segmentoAccionSeleccionada == "descanso" ? segmentoMinutosAplicadosCaptura : null);
            },
            $"empleado={AsistenciaActual!.EmpleadoId};fecha={AsistenciaActual.Fecha:yyyy-MM-dd};inicio={segmento.MarcacionInicioId};fin={segmento.MarcacionFinId};accion={segmentoAccionSeleccionada};clasInicio={clasificacionInicio};clasFin={clasificacionFin}",
            "Se aplicó corrección de asistencia: cambio de interpretación de segmento.",
            mensaje);
    }

    private async Task AplicarIgnorarSegmentoAsync(TimelineSegmentoDia segmento)
    {
        if (AsistenciaActual == null || !segmento.MarcacionInicioId.HasValue || !segmento.MarcacionFinId.HasValue)
        {
            return;
        }

        await AplicarCambioSegmentoAsync(
            segmento,
            (inicio, fin, usuarioActual, db) =>
            {
                inicio.EsAnulada = true;
                inicio.PayloadRaw = RrhhMarcacionSegmentActionHelper.SetAction(inicio.PayloadRaw, "ignorar");
                inicio.Procesada = false;
                inicio.UpdatedAt = DateTime.UtcNow;
                inicio.UpdatedBy = usuarioActual;

                fin.EsAnulada = true;
                fin.PayloadRaw = RrhhMarcacionSegmentActionHelper.SetAction(fin.PayloadRaw, "ignorar");
                fin.Procesada = false;
                fin.UpdatedAt = DateTime.UtcNow;
                fin.UpdatedBy = usuarioActual;

                GuardarResolucionSegmento(db, usuarioActual, segmento, TipoSegmentoResolucionRrhh.NoConsiderar, "Bloque marcado para no considerar.");
            },
            $"empleado={AsistenciaActual.EmpleadoId};fecha={AsistenciaActual.Fecha:yyyy-MM-dd};inicio={segmento.MarcacionInicioId};fin={segmento.MarcacionFinId};accion=ignorar",
            "Se aplicó corrección de asistencia: segmento ignorado desde resumen.",
            "Segmento ignorado y día reprocesado.");
    }

    private async Task AplicarCambioSegmentoAsync(
        TimelineSegmentoDia segmento,
        AplicarCambiosSegmentoDelegate aplicarCambios,
        string detalleBitacora,
        string mensajeBitacora,
        string mensajeOk)
    {
        if (AsistenciaActual == null || !segmento.MarcacionInicioId.HasValue || !segmento.MarcacionFinId.HasValue)
        {
            return;
        }

        error = null;
        ok = null;
        cargando = true;
        segmentoGuardandoInicioId = segmento.MarcacionInicioId;
        segmentoGuardandoFinId = segmento.MarcacionFinId;

        try
        {
            var usuarioActual = await ObtenerUsuarioActualAsync();
            await using var db = await DbFactory.CreateDbContextAsync();
            var inicioId = segmento.MarcacionInicioId.Value;
            var finId = segmento.MarcacionFinId.Value;
            var marcaciones = await db.RrhhMarcaciones
                .Where(m => m.Id == inicioId || m.Id == finId)
                .ToListAsync();

            var inicio = marcaciones.FirstOrDefault(m => m.Id == inicioId);
            var fin = marcaciones.FirstOrDefault(m => m.Id == finId);
            if (inicio == null || fin == null)
            {
                error = "No se encontraron las marcaciones del segmento para aplicar el cambio.";
                return;
            }

            aplicarCambios(inicio, fin, usuarioActual, db);
            await RegistrarBitacoraCorreccionAsync(db, mensajeBitacora, detalleBitacora);
            await ReprocesarYRefrescarDiaAsync(db, AsistenciaActual.Fecha);
            CancelarEdicionSegmento();
            ok = mensajeOk;
        }
        catch (Exception ex)
        {
            error = ex.InnerException?.Message ?? ex.Message;
        }
        finally
        {
            cargando = false;
            segmentoGuardandoInicioId = null;
            segmentoGuardandoFinId = null;
        }
    }

    private void GuardarResolucionSegmento(CrmDbContext db, string usuarioActual, TimelineSegmentoDia segmento, TipoSegmentoResolucionRrhh tipoSegmento, string observaciones, int? minutosAplicadosOverride = null)
    {
        if (AsistenciaActual == null || !segmento.MarcacionInicioId.HasValue || !segmento.MarcacionFinId.HasValue)
        {
            return;
        }

        var resolucion = db.ChangeTracker.Entries<RrhhSegmentoResolucion>()
            .Select(e => e.Entity)
            .FirstOrDefault(r => r.EmpresaId == _empresaId
                && r.EmpleadoId == AsistenciaActual.EmpleadoId
                && r.Fecha == AsistenciaActual.Fecha
                && r.MarcacionInicioId == segmento.MarcacionInicioId.Value
                && r.MarcacionFinId == segmento.MarcacionFinId.Value)
            ?? db.RrhhSegmentosResoluciones.FirstOrDefault(r => r.EmpresaId == _empresaId
            && r.EmpleadoId == AsistenciaActual.EmpleadoId
            && r.Fecha == AsistenciaActual.Fecha
            && r.MarcacionInicioId == segmento.MarcacionInicioId.Value
            && r.MarcacionFinId == segmento.MarcacionFinId.Value);

        if (resolucion == null)
        {
            resolucion = new RrhhSegmentoResolucion
            {
                Id = Guid.NewGuid(),
                EmpresaId = _empresaId,
                EmpleadoId = AsistenciaActual.EmpleadoId,
                Fecha = AsistenciaActual.Fecha,
                MarcacionInicioId = segmento.MarcacionInicioId.Value,
                MarcacionFinId = segmento.MarcacionFinId.Value,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = usuarioActual,
                IsActive = true
            };

            db.RrhhSegmentosResoluciones.Add(resolucion);
        }

        resolucion.TipoSegmento = tipoSegmento;
        resolucion.Estado = EstadoSegmentoResolucionRrhh.Vigente;
        resolucion.FueInferidoAutomaticamente = false;
        resolucion.MinutosAplicadosOverride = tipoSegmento == TipoSegmentoResolucionRrhh.Descanso
            ? minutosAplicadosOverride
            : null;
        resolucion.Observaciones = observaciones;
        resolucion.UpdatedAt = DateTime.UtcNow;
        resolucion.UpdatedBy = usuarioActual;
    }

    private async Task ReconciliarResolucionesSegmentoDiaAsync(CrmDbContext db, DateOnly fecha)
    {
        if (AsistenciaActual == null)
        {
            return;
        }

        var (desdeUtc, hastaUtc) = ObtenerVentanaConsultaUtc(fecha);
        var marcacionesActivas = await db.RrhhMarcaciones
            .AsNoTracking()
            .Include(m => m.Checador)
            .Where(m => m.EmpresaId == _empresaId
                && m.EmpleadoId == AsistenciaActual.EmpleadoId
                && !m.EsAnulada
                && m.FechaHoraMarcacionUtc >= desdeUtc
                && m.FechaHoraMarcacionUtc < hastaUtc)
            .OrderBy(m => m.FechaHoraMarcacionUtc)
            .ToListAsync();

        var paresActivos = marcacionesActivas
            .Where(m => DateOnly.FromDateTime(ObtenerFechaHoraLocalMarcacion(m)) == fecha)
            .OrderBy(ObtenerFechaHoraLocalMarcacion)
            .Zip(marcacionesActivas
                .Where(m => DateOnly.FromDateTime(ObtenerFechaHoraLocalMarcacion(m)) == fecha)
                .OrderBy(ObtenerFechaHoraLocalMarcacion)
                .Skip(1), (inicio, fin) => (inicio.Id, fin.Id))
            .ToHashSet();

        var resoluciones = await db.RrhhSegmentosResoluciones
            .Where(r => r.EmpresaId == _empresaId
                && r.EmpleadoId == AsistenciaActual.EmpleadoId
                && r.Fecha == fecha
                && r.IsActive)
            .ToListAsync();

        foreach (var resolucion in resoluciones)
        {
            var sigueVigente = paresActivos.Contains((resolucion.MarcacionInicioId, resolucion.MarcacionFinId));
            resolucion.Estado = sigueVigente
                ? (resolucion.FueInferidoAutomaticamente ? EstadoSegmentoResolucionRrhh.RequiereRevision : EstadoSegmentoResolucionRrhh.Vigente)
                : EstadoSegmentoResolucionRrhh.Obsoleta;
            resolucion.UpdatedAt = DateTime.UtcNow;
        }
    }

    private IReadOnlyList<ResumenLateralItem> ObtenerResumenLateralTimeline()
    {
        if (AsistenciaActual == null)
        {
            return [];
        }

        var extraLabel = string.IsNullOrWhiteSpace(AsistenciaActual.ResolucionTiempoExtra)
            ? "Extra (sugerido)"
            : "Extra";
        var extraValor = string.IsNullOrWhiteSpace(AsistenciaActual.ResolucionTiempoExtra)
            ? ObtenerMinutosExtraSugeridosModo(AsistenciaActual)
            : AsistenciaActual.MinutosExtra;

        return
        [
            new("Visible", FormatearMinutos(ObtenerMinutosTrabajadosVisibles(AsistenciaActual)), "asis-summary-side__item--primary"),
            new(extraLabel, FormatearMinutos(extraValor), "asis-summary-side__item--accent"),
            new("Permiso", permisoDiaSeleccionado == null ? FormatearMinutos(0) : FormatearMinutos(ObtenerMinutosPermisoCapturados()), "asis-summary-side__item--warn"),
            new("Compensación", FormatearMinutos(ObtenerMinutosCompensadosAprobadosActual()), "asis-summary-side__item--info"),
            new("Descanso", FormatearMinutos(AsistenciaActual.MinutosDescansoTomado), "asis-summary-side__item--muted")
        ];
    }

    private IReadOnlyList<RrhhAsistenciaCorreccionSegmento> ObtenerSegmentosBaseTimeline()
        => asesorCorreccionActual?.Segmentos.Where(s => !s.EsAjuste).ToList() ?? [];

    private IReadOnlyList<RrhhAsistenciaCorreccionSegmento> ObtenerSegmentosAjusteTimeline()
        => asesorCorreccionActual?.Segmentos.Where(s => s.EsAjuste).ToList() ?? [];
}