using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Core.Models;

namespace MundoVs.Core.Services;

public sealed class RrhhAsistenciaCorreccionAdvisor : IRrhhAsistenciaCorreccionAdvisor
{
    public RrhhAsistenciaCorreccionAdvice Analizar(
        RrhhAsistencia asistencia,
        RrhhAusencia? permisoDia,
        int minutosCompensadosAprobados,
        int minutosRecuperablesAprobables,
        bool bancoHorasHabilitado,
        bool puedeAprobarTiempoExtra,
        decimal factorTiempoExtra,
        int saldoBancoHorasMinutos)
    {
        var faltanteMinutos = Math.Max(0, RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteBanco(asistencia));
        var permisoSugeridoMinutos = Math.Max(0, RrhhTiempoExtraPolicy.ObtenerMinutosPermisoSugeridos(asistencia, minutosCompensadosAprobados));
        var descansoNoPagadoProgramadoMinutos = Math.Max(0, RrhhTiempoExtraPolicy.ObtenerMinutosDescansoNoPagadoProgramado(asistencia));
        var extraMinutos = Math.Max(0, asistencia.MinutosExtra);
        var extraResoluble = Math.Max(0, RrhhTiempoExtraPolicy.ObtenerMinutosExtraResolubles(asistencia, factorTiempoExtra));
        var permisoMinutos = permisoDia == null ? 0 : (int)Math.Round(permisoDia.Horas * 60m, MidpointRounding.AwayFromZero);
        var faltanteAjustadoMinutos = Math.Max(0, faltanteMinutos - Math.Max(0, minutosCompensadosAprobados));
        var permisoCubriendoMinutos = Math.Min(faltanteAjustadoMinutos, permisoMinutos);
        var faltanteRemanenteMinutos = Math.Max(0, faltanteAjustadoMinutos - permisoCubriendoMinutos);
        var minutosCompensacionUtiles = ObtenerMinutosCompensacionUtiles(faltanteMinutos, permisoMinutos, minutosRecuperablesAprobables);
        var marcacionesIncompletas = asistencia.Estatus is RrhhAsistenciaEstatus.Incompleta or RrhhAsistenciaEstatus.MarcaNoReconocida || asistencia.TotalMarcaciones <= 1;
        var turnoInconsistente = asistencia.Estatus == RrhhAsistenciaEstatus.TurnoNoAsignado
            || (asistencia.Observaciones?.Contains("cambio de turno", StringComparison.OrdinalIgnoreCase) ?? false)
            || (asistencia.RequiereRevision && asistencia.TurnoBaseId == null);
        var salidaTempranaCompensaDescanso = asistencia.Observaciones?.Contains("salida anticipada sugiere permiso o descanso no tomado", StringComparison.OrdinalIgnoreCase) ?? false;

        var resolucionesDisponibles = ConstruirResolucionesDisponibles(extraMinutos, extraResoluble, faltanteRemanenteMinutos, bancoHorasHabilitado, puedeAprobarTiempoExtra, saldoBancoHorasMinutos);
        var extraAprobadaMinutos = RrhhTiempoExtraPolicy.ObtenerMinutosExtraAprobados(asistencia);
        var segmentos = ConstruirSegmentos(asistencia, permisoCubriendoMinutos, faltanteRemanenteMinutos, extraMinutos, extraAprobadaMinutos, minutosCompensadosAprobados);

        if (marcacionesIncompletas)
        {
            return new RrhhAsistenciaCorreccionAdvice(
                "MarcacionesIncompletas",
                "Primero corrige las marcaciones",
                "El día no tiene información suficiente o consistente. Antes de registrar permisos o resolver banco, completa o ajusta las checadas.",
                "text-bg-warning",
                "bi-fingerprint",
                RrhhAsistenciaCorreccionTabs.Marcaciones,
                "Ir a marcaciones",
                null,
                segmentos,
                resolucionesDisponibles,
                faltanteRemanenteMinutos > 0 ? "Aunque existe faltante, primero conviene validar si una marcación ausente está provocando el cálculo." : null,
                extraMinutos > 0 ? "Aunque existe tiempo extra, primero conviene validar si las marcaciones del día son correctas." : null,
                true,
                false,
                false);
        }

        if (extraMinutos > 0 && faltanteRemanenteMinutos > 0)
        {
            return new RrhhAsistenciaCorreccionAdvice(
                "JornadaMixta",
                "El día mezcla extra y faltante",
                "Se detectaron señales contradictorias. Revisa turno o marcaciones antes de confirmar pagos, banco o permisos.",
                "text-bg-warning",
                "bi-exclamation-diamond",
                turnoInconsistente ? RrhhAsistenciaCorreccionTabs.Permisos : RrhhAsistenciaCorreccionTabs.Marcaciones,
                turnoInconsistente ? "Revisar turno del día" : "Revisar marcaciones",
                null,
                segmentos,
                resolucionesDisponibles,
                "No registres un permiso solo por ver faltante si el origen parece ser una marcación o un turno incorrecto.",
                "No se muestran resoluciones como acción principal hasta validar la causa del faltante y del extra.",
                !turnoInconsistente,
                turnoInconsistente,
                false);
        }

        if (salidaTempranaCompensaDescanso)
        {
            return new RrhhAsistenciaCorreccionAdvice(
                "SalidaTempranaCompensaDescanso",
                "La salida temprana puede sustituir el descanso",
                "Se detectó que la salida anticipada podría haber absorbido el descanso no marcado. Antes de usar banco o registrar permiso, conviene confirmar esa regla operativa.",
                "text-bg-info",
                "bi-box-arrow-left",
                RrhhAsistenciaCorreccionTabs.Resumen,
                "Confirmar regla del descanso",
                null,
                segmentos,
                [],
                "Si confirmas que la salida temprana sustituyó el descanso, evita aplicar permiso o banco por ese mismo tramo.",
                "Las resoluciones de tiempo se ocultan hasta confirmar si la salida temprana ya compensó el descanso no tomado.",
                false,
                true,
                false);
        }

        if (minutosCompensacionUtiles > 0 && minutosCompensadosAprobados <= 0)
        {
            return new RrhhAsistenciaCorreccionAdvice(
                "CompensacionPermisoPendiente",
                $"Tiempo recuperable útil: {FormatearMinutos(minutosCompensacionUtiles)}",
                "El empleado recuperó tiempo, pero solo conviene aprobarlo si de verdad reduce el permiso o el faltante del día.",
                "text-bg-info",
                "bi-hourglass-bottom",
                RrhhAsistenciaCorreccionTabs.Permisos,
                "Revisar compensación",
                null,
                segmentos,
                resolucionesDisponibles,
                permisoDia != null
                    ? $"Se detectaron {FormatearMinutos(minutosRecuperablesAprobables)} recuperables y hasta {FormatearMinutos(minutosCompensacionUtiles)} sí ayudan a reducir el permiso efectivo del día."
                    : $"Se detectaron {FormatearMinutos(minutosRecuperablesAprobables)} recuperables y hasta {FormatearMinutos(minutosCompensacionUtiles)} sí ayudan a reducir el faltante real del día.",
                resolucionesDisponibles.Count > 0 ? "La resolución de tiempo queda como ajuste secundario; primero valida si esta compensación realmente cambia el día." : null,
                false,
                true,
                false);
        }

        if (permisoDia != null && faltanteRemanenteMinutos == 0)
        {
            return new RrhhAsistenciaCorreccionAdvice(
                "PermisoCubreFaltante",
                "El permiso ya cubre el faltante",
                "El permiso parcial registrado ya absorbió el faltante neto del día. Si ves diferencia operativa, revisa marcaciones o turno antes de mover banco de horas.",
                "text-bg-info",
                "bi-shield-check",
                RrhhAsistenciaCorreccionTabs.Permisos,
                "Revisar permiso del día",
                null,
                segmentos,
                resolucionesDisponibles,
                permisoDia.ConGocePago
                    ? $"Permiso registrado: {FormatearMinutos(permisoMinutos)}. {(minutosCompensadosAprobados > 0 ? $"Compensación aprobada: {FormatearMinutos(minutosCompensadosAprobados)}. " : string.Empty)}Ya no queda faltante remanente por cubrir."
                    : $"Permiso sin goce registrado: {FormatearMinutos(permisoMinutos)}. {(minutosCompensadosAprobados > 0 ? $"Compensación aprobada: {FormatearMinutos(minutosCompensadosAprobados)}. " : string.Empty)}Ya no queda faltante remanente por cubrir.",
                resolucionesDisponibles.Count > 0 ? "Las acciones de tiempo quedan como ajuste secundario; este faltante ya está cubierto por el permiso." : null,
                false,
                true,
                false);
        }

        if (faltanteRemanenteMinutos > 0)
        {
            var sugerirBanco = bancoHorasHabilitado && puedeAprobarTiempoExtra;
            var notaPermiso = descansoNoPagadoProgramadoMinutos > 0
                ? $"El permiso debe capturarse por tiempo neto. Este turno ya descuenta {FormatearMinutos(descansoNoPagadoProgramadoMinutos)} de descanso no pagado, así que si la ausencia total incluye ese tramo no lo sumes al permiso. Sugerido para este día: {FormatearMinutos(permisoSugeridoMinutos)}."
                : $"El permiso parcial debe capturarse solo por el tiempo neto ausente. Sugerido para este día: {FormatearMinutos(permisoSugeridoMinutos)}.";
            return new RrhhAsistenciaCorreccionAdvice(
                "Faltante",
                permisoMinutos > 0
                    ? $"Faltante remanente: {FormatearMinutos(faltanteRemanenteMinutos)}"
                    : $"Faltante detectado: {FormatearMinutos(faltanteRemanenteMinutos)}",
                sugerirBanco
                    ? "Primero revisa el permiso neto del día. Si después todavía quieres absorber el faltante con banco, hazlo como un segundo paso desde la pestaña de tiempo."
                    : "No hay una cobertura automática clara. Revisa marcaciones o registra permiso parcial si el faltante corresponde a una ausencia real.",
                sugerirBanco ? "text-bg-primary" : "text-bg-secondary",
                "bi-person-check",
                RrhhAsistenciaCorreccionTabs.Permisos,
                "Revisar permiso neto",
                null,
                segmentos,
                resolucionesDisponibles,
                permisoMinutos > 0
                    ? $"Ya existe un permiso registrado por {FormatearMinutos(permisoMinutos)} y aún quedan {FormatearMinutos(faltanteRemanenteMinutos)} por cubrir. {(minutosCompensadosAprobados > 0 ? $"Compensación aprobada aplicada: {FormatearMinutos(minutosCompensadosAprobados)}. " : string.Empty)}{notaPermiso}"
                    : (minutosCompensadosAprobados > 0 ? $"Compensación aprobada aplicada: {FormatearMinutos(minutosCompensadosAprobados)}. {notaPermiso}" : notaPermiso),
                sugerirBanco
                    ? "Si después del permiso aún quieres ajustar el faltante con banco de horas, hazlo desde Tiempo / Bitácora. Este día no tiene tiempo extra resoluble."
                    : "Este día no tiene tiempo extra resoluble; solo se permiten acciones compatibles con faltante.",
                false,
                true,
                false);
        }

        if (extraMinutos > 0)
        {
            var sugerirBanco = bancoHorasHabilitado && puedeAprobarTiempoExtra;
            return new RrhhAsistenciaCorreccionAdvice(
                "TiempoExtra",
                $"Tiempo extra detectado: {FormatearMinutos(extraMinutos)}",
                sugerirBanco
                    ? "La acción sugerida es resolver el extra pagándolo o enviándolo a banco. No hace falta registrar permiso si el día ya tiene tiempo adicional trabajado."
                    : "El día tiene tiempo extra. La acción sugerida es resolverlo como pago, según autorización operativa.",
                "text-bg-success",
                "bi-plus-circle",
                RrhhAsistenciaCorreccionTabs.Tiempo,
                "Resolver tiempo extra",
                sugerirBanco ? "BancoTodo" : "PagarTodo",
                segmentos,
                resolucionesDisponibles,
                "No registres permiso para compensar un día con tiempo extra salvo que la asistencia esté mal capturada y debas corregir la causa raíz.",
                "Solo se muestran resoluciones compatibles con tiempo extra; no se ofrece cubrir faltante porque no existe en este día.",
                false,
                false,
                true);
        }

        if (turnoInconsistente)
        {
            return new RrhhAsistenciaCorreccionAdvice(
                "TurnoInconsistente",
                "El turno del día requiere revisión",
                "La jornada no coincide de forma natural con el turno asignado. Lo más útil es revisar el turno del día antes de capturar otras correcciones.",
                "text-bg-info",
                "bi-calendar-week",
                RrhhAsistenciaCorreccionTabs.Permisos,
                "Revisar turno del día",
                null,
                segmentos,
                resolucionesDisponibles,
                permisoDia != null ? "Ya existe un permiso registrado; confirma primero si el turno también debe ajustarse." : null,
                resolucionesDisponibles.Count > 0 ? "Después de corregir el turno, vuelve a revisar si aún hace falta resolver banco o tiempo extra." : null,
                false,
                true,
                false);
        }

        if (permisoDia != null)
        {
            return new RrhhAsistenciaCorreccionAdvice(
                "PermisoRegistrado",
                "Ya existe un permiso parcial registrado",
                "El permiso ya forma parte del cálculo del día. Usa esta sección solo para ajustarlo o retirarlo si fue capturado incorrectamente.",
                "text-bg-info",
                "bi-shield-check",
                RrhhAsistenciaCorreccionTabs.Permisos,
                "Revisar permiso del día",
                null,
                segmentos,
                resolucionesDisponibles,
                permisoDia.ConGocePago ? "El permiso con goce impacta el banco de horas del empleado." : "El permiso sin goce no consume banco de horas.",
                resolucionesDisponibles.Count > 0 ? "La resolución de tiempo permanece disponible solo como ajuste secundario." : null,
                false,
                true,
                false);
        }

        return new RrhhAsistenciaCorreccionAdvice(
            "JornadaConsistente",
            "Jornada consistente",
            "No se detectan conflictos importantes. Usa el modal solo si necesitas una corrección operativa puntual o revisar la bitácora del día.",
            "text-bg-light",
            "bi-check-circle",
            RrhhAsistenciaCorreccionTabs.Resumen,
            "Revisar resumen",
            null,
            segmentos,
            resolucionesDisponibles,
            null,
            resolucionesDisponibles.Count > 0 ? "Hay acciones de tiempo disponibles, pero no se consideran la necesidad principal del día." : null,
            false,
            false,
            false);
    }

    private static IReadOnlyList<RrhhAsistenciaCorreccionResolucionOption> ConstruirResolucionesDisponibles(
        int extraMinutos,
        int extraResoluble,
        int faltanteMinutos,
        bool bancoHorasHabilitado,
        bool puedeAprobarTiempoExtra,
        int saldoBancoHorasMinutos)
    {
        var opciones = new List<RrhhAsistenciaCorreccionResolucionOption>();
        if (!puedeAprobarTiempoExtra)
        {
            return opciones;
        }

        if (extraMinutos > 0 && extraResoluble > 0)
        {
            opciones.Add(new("PagarTodo", "Pagar tiempo extra", "Autoriza todo el tiempo extra como pago."));
            if (bancoHorasHabilitado)
            {
                opciones.Add(new("BancoTodo", "Enviar extra a banco", "Acumula todo el tiempo extra en banco de horas."));
                opciones.Add(new("MitadMitad", "Mitad pago / mitad banco", "Divide el tiempo extra entre pago y banco."));
            }
        }

        if (faltanteMinutos > 0 && bancoHorasHabilitado && saldoBancoHorasMinutos > 0)
        {
            opciones.Add(new("CubrirFaltanteConBanco", "Cubrir faltante con banco", "Consume saldo del banco para cubrir el faltante neto que permanezca después de revisar el permiso del día."));
        }

        if (opciones.Count > 0)
        {
            opciones.Add(new("SinAccion", "Sin acción", "No aplicar una resolución automática en este momento."));
        }

        return opciones;
    }

    private static IReadOnlyList<RrhhAsistenciaCorreccionSegmento> ConstruirSegmentos(RrhhAsistencia asistencia, int permisoMinutos, int faltanteMinutos, int extraMinutos, int extraAprobadaMinutos, int compensadoMinutos)
    {
        var trabajoMinutos = RrhhTiempoExtraPolicy.ObtenerMinutosTrabajadosVisibles(asistencia, compensadoMinutos);
        var descansoMinutos = Math.Max(0, asistencia.MinutosDescansoTomado);
        var extraPendienteMinutos = Math.Max(0, extraMinutos - extraAprobadaMinutos);
        var baseVisual = Math.Max(1, trabajoMinutos + descansoMinutos + permisoMinutos + faltanteMinutos + extraPendienteMinutos);

        var segmentos = new List<RrhhAsistenciaCorreccionSegmento>();
        AgregarSegmento(segmentos, "trabajo", "Trabajado", trabajoMinutos, baseVisual, "asis-timeline__segment--worked", FormatearMinutos(trabajoMinutos));
        AgregarSegmento(segmentos, "descanso", "Descansos", descansoMinutos, baseVisual, "asis-timeline__segment--break", FormatearMinutos(descansoMinutos));
        AgregarSegmento(segmentos, "permiso", "Permiso", permisoMinutos, baseVisual, "asis-timeline__segment--permission", FormatearMinutos(permisoMinutos), true);
        AgregarSegmento(segmentos, "faltante", "Faltante", faltanteMinutos, baseVisual, "asis-timeline__segment--missing", FormatearMinutos(faltanteMinutos), true);
        AgregarSegmento(segmentos, "extra-pendiente", "Extra detectada", extraPendienteMinutos, baseVisual, "asis-timeline__segment--extra", FormatearMinutos(extraPendienteMinutos), true, true);

        if (segmentos.Count == 0)
        {
            segmentos.Add(new RrhhAsistenciaCorreccionSegmento("sin-datos", "Sin datos", 1, 100m, "asis-timeline__segment--neutral", "Sin datos"));
        }

        return segmentos;
    }

    private static void AgregarSegmento(
        ICollection<RrhhAsistenciaCorreccionSegmento> segmentos,
        string clave,
        string titulo,
        int minutos,
        int totalVisual,
        string cssClass,
        string textoCorto,
        bool esInformativo = false,
        bool esAjuste = false)
    {
        if (minutos <= 0)
        {
            return;
        }

        var width = totalVisual <= 0
            ? 0m
            : Math.Max(6m, Math.Round((minutos / (decimal)totalVisual) * 100m, 2));
        segmentos.Add(new RrhhAsistenciaCorreccionSegmento(clave, titulo, minutos, width, cssClass, textoCorto, esInformativo, esAjuste));
    }

    private static string FormatearMinutos(int minutos)
    {
        var horas = minutos / 60;
        var minutosRestantes = Math.Abs(minutos % 60);
        return $"{horas}:{minutosRestantes:00} h";
    }

    private static int ObtenerMinutosCompensacionUtiles(int faltanteMinutos, int permisoMinutos, int minutosRecuperablesAprobables)
    {
        var baseCompensable = Math.Max(0, Math.Max(faltanteMinutos, permisoMinutos));
        return Math.Max(0, Math.Min(baseCompensable, minutosRecuperablesAprobables));
    }
}
