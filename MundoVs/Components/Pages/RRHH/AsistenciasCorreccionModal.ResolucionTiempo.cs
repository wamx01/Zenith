using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Core.Models;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Components.Pages.RRHH;

public partial class AsistenciasCorreccionModal
{
    private string ObtenerResumenDestinoTiempoActual()
    {
        if (AsistenciaActual == null)
        {
            return "Sin resolución de tiempo.";
        }

        var pago = Math.Max(0, AsistenciaActual.MinutosExtraAutorizadosPago);
        var banco = Math.Max(0, AsistenciaActual.MinutosExtraAutorizadosBanco);
        var bancoAcumulado = ObtenerMinutosBancoAcumuladosActual();
        var cubiertoBanco = Math.Max(0, AsistenciaActual.MinutosCubiertosBancoHoras);

        if (cubiertoBanco > 0)
        {
            return $"Cubierto con banco: {FormatearMinutos(cubiertoBanco)}.";
        }

        if (pago > 0 && banco > 0)
        {
            return bancoAcumulado > 0 && bancoAcumulado != banco
                ? $"Resolución mixta · Pago: {FormatearMinutos(pago)} · Banco base: {FormatearMinutos(banco)} · Banco acumulado: {FormatearMinutos(bancoAcumulado)}."
                : $"Resolución mixta · Pago: {FormatearMinutos(pago)} · Banco: {FormatearMinutos(banco)}.";
        }

        if (pago > 0)
        {
            return $"Enviado a pago: {FormatearMinutos(pago)}.";
        }

        if (banco > 0)
        {
            return bancoAcumulado > 0 && bancoAcumulado != banco
                ? $"Enviado a banco: {FormatearMinutos(banco)} base · acumulado {FormatearMinutos(bancoAcumulado)}."
                : $"Enviado a banco: {FormatearMinutos(banco)}.";
        }

        return string.IsNullOrWhiteSpace(AsistenciaActual.ResolucionTiempoExtra)
            ? "Sin resolución aplicada todavía."
            : $"Resolución actual: {AsistenciaActual.ResolucionTiempoExtra}.";
    }

    private string ObtenerResumenSaldoBancoHoras()
        => $"Saldo banco de horas: {FormatearMinutos(Math.Max(0, saldoBancoHorasSeleccionado))} de {FormatearMinutos(Math.Max(0, topeBancoHorasConfigurado))}.";

    private int ObtenerMinutosBancoAcumuladosActual()
        => Math.Max(0, minutosExtraBancoAcumuladosActual);

    private int ObtenerMinutosCompensadosAprobadosActual()
    {
        if (!TieneCompensacionPermisoAprobada())
        {
            return 0;
        }
        return Math.Max(0, minutosCompensadosPermisoAprobados);
    }

    private bool TieneResolucionTiempoActual()
        => AsistenciaActual != null && RrhhTiempoExtraPolicy.TieneResolucionTiempoAplicada(AsistenciaActual);

    private int ObtenerMinutosExtraAprobados(RrhhAsistencia asistencia)
    {
        // Sin turno: el usuario puede aprobar extra sin que el procesador lo haya detectado.
        // Se muestran los minutos aprobados aunque no haya ResolucionTiempoExtra.
        if (asistencia.TurnoBaseId is null)
        {
            return RrhhTiempoExtraPolicy.ObtenerMinutosExtraAprobados(asistencia);
        }

        if (string.IsNullOrWhiteSpace(asistencia.ResolucionTiempoExtra))
        {
            return 0;
        }
        return RrhhTiempoExtraPolicy.ObtenerMinutosExtraAprobados(asistencia);
    }

    private int ObtenerMinutosResolubles(RrhhAsistencia asistencia)
        => RrhhTiempoExtraPolicy.ObtenerMinutosExtraResolubles(asistencia, factorTiempoExtraConfigurado);

    private int ObtenerMinutosFaltanteNeto(RrhhAsistencia asistencia)
        => RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteNeto(asistencia);

    /// <summary>
    /// Sugerencia "neto vs neto": neto trabajado − neto esperado del turno.
    /// Debe coincidir con la secuencia del procesador (CalcularMinutosExtra modo NetoVsNeto):
    /// NO incluye perdón manual en el neto trabajado. El procesador ya persiste
    /// MinutosExtra con manual incluido y umbral mínimo aplicado.
    /// Sin turno, usa jornada legal de 8h (480 min) solo como referencia visual;
    /// el procesador NO auto-detecta extra, por lo que el usuario decide manualmente.
    /// </summary>
    private int ObtenerSugerenciaExtraNetoVsNeto(RrhhAsistencia asistencia)
    {
        var netoTrabajado = Math.Max(0, asistencia.MinutosTrabajadosNetos);
        // Sin turno: usar jornada legal como referencia para sugerir, pero el usuario
        // puede aprobar cualquier valor. Con turno: usar jornada programada.
        var netoEsperado = asistencia.TurnoBaseId is null
            ? 480
            : Math.Max(0, asistencia.MinutosJornadaNetaProgramada);
        return Math.Max(0, netoTrabajado - netoEsperado);
    }

    /// <summary>
    /// Retorna los minutos de extra sugeridos según el modo actualmente seleccionado en el modal.
    /// SinTurno: sugiere el excedente sobre 8h (480 min) como referencia.
    /// NetoVsNeto: neto trabajado − neto esperado.
    /// EntradaSalida: usa el extra detectado por el procesador.
    /// </summary>
    private int ObtenerMinutosExtraSugeridosModo(RrhhAsistencia asistencia)
        => modoSugerenciaExtra switch
        {
            "SinTurno" => Math.Max(0, Math.Max(0, asistencia.MinutosTrabajadosNetos) - 480),
            "NetoVsNeto" => ObtenerSugerenciaExtraNetoVsNeto(asistencia),
            _ => asistencia.MinutosExtra
        };

    private string ObtenerDescripcionModoSugerencia(RrhhAsistencia asistencia)
    {
        var porEntradaSalida = asistencia.MinutosExtra;
        var porNeto = ObtenerSugerenciaExtraNetoVsNeto(asistencia);
        var netoEsperadoDescripcion = asistencia.TurnoBaseId is null
            ? "jornada legal 8h"
            : FormatearMinutos(asistencia.MinutosJornadaNetaProgramada);

        if (modoSugerenciaExtra == "SinTurno")
        {
            var sugeridoSinTurno = Math.Max(0, Math.Max(0, asistencia.MinutosTrabajadosNetos) - 480);
            return $"Sin turno: el tiempo trabajado ({FormatearMinutos(asistencia.MinutosTrabajadosNetos)}) se considera normal. Sugerencia de extra sobre 8h: {FormatearMinutos(sugeridoSinTurno)}. El usuario decide cuánto aprobar como extra.";
        }

        return modoSugerenciaExtra == "NetoVsNeto"
            ? $"Neto trabajado ({FormatearMinutos(asistencia.MinutosTrabajadosNetos)}) − neto esperado ({netoEsperadoDescripcion}) = {FormatearMinutos(porNeto)}."
            : $"Detectado por el procesador: entrada/salida real vs programada = {FormatearMinutos(porEntradaSalida)}. Alternativa neto vs neto: {FormatearMinutos(porNeto)}.";
    }

    private string? ObtenerExplicacionTiempoExtraWizard()
    {
        if (AsistenciaActual == null || AsistenciaActual.MinutosExtra <= 0)
        {
            return null;
        }

        var detectado = AsistenciaActual.MinutosExtra;
        var aprobable = ObtenerMinutosResolubles(AsistenciaActual);
        var aprobado = ObtenerMinutosExtraAprobados(AsistenciaActual);

        if (aprobado > detectado)
        {
            return $"Lo aprobado puede verse mayor que lo detectado porque la aprobación usa el factor configurado. Detectado: {FormatearMinutos(detectado)}. Aprobable: {FormatearMinutos(aprobable)}.";
        }

        if (aprobado > 0)
        {
            return $"Ya hay {FormatearMinutos(aprobado)} aprobados. Si cambias la decisión, puedes guardarla de nuevo o quitar la resolución actual.";
        }

        return $"Detectado: {FormatearMinutos(detectado)}. Máximo aprobable con la regla actual: {FormatearMinutos(aprobable)}.";
    }

    private string ObtenerEstadoPasoTiempoWizard()
    {
        if (AsistenciaActual == null)
        {
            return "Sin datos";
        }

        if (asesorCorreccionActual?.PriorizarTiempo == true && !RrhhTiempoExtraPolicy.TieneResolucionTiempoAplicada(AsistenciaActual))
        {
            return "Pendiente";
        }

        if (RrhhTiempoExtraPolicy.TieneResolucionTiempoAplicada(AsistenciaActual))
        {
            return "Resuelto";
        }

        if (AsistenciaActual.MinutosExtra > 0)
        {
            return "Informativo";
        }

        return PuedeMostrarResolucionTiempo() ? "Pendiente" : "Sin acción";
    }

    private bool EsPasoTiempoWizardCompletado()
    {
        if (AsistenciaActual == null)
        {
            return false;
        }

        if (asesorCorreccionActual?.PriorizarTiempo == true)
        {
            return RrhhTiempoExtraPolicy.TieneResolucionTiempoAplicada(AsistenciaActual);
        }

        return true;
    }

    private bool PuedeMostrarResolucionTiempo()
        => asesorCorreccionActual?.ResolucionesDisponibles.Count > 0;

    /// <summary>Fase 8 — la captura de tiempo extra del modal diario solo aplica cuando el flujo
    /// por periodo está apagado (legacy). Con el gate Fase 7 on, el extra se autoriza por periodo
    /// en Asistencias Semanal; aquí solo se muestra un aviso.</summary>
    private bool PuedeCapturarResolucionTiempoDiaria()
        => !requiereResolucionAutorizadaParaNomina && PuedeMostrarResolucionTiempo();

    private string ObtenerEstadoTiempoSeccion()
    {
        if (AsistenciaActual == null)
        {
            return "Sin datos";
        }

        if (RrhhTiempoExtraPolicy.TieneResolucionTiempoAplicada(AsistenciaActual))
        {
            return "Resuelto";
        }

        return PuedeMostrarResolucionTiempo()
            ? "Pendiente"
            : "Sin pendiente";
    }

    private string ObtenerClaseEstadoTiempoSeccion()
    {
        if (AsistenciaActual == null)
        {
            return "text-bg-light";
        }

        if (RrhhTiempoExtraPolicy.TieneResolucionTiempoAplicada(AsistenciaActual))
        {
            return "text-bg-success";
        }

        return PuedeMostrarResolucionTiempo()
            ? "text-bg-warning"
            : "text-bg-light";
    }

    private string ObtenerGuiaTiempoResolucion()
    {
        if (AsistenciaActual == null)
        {
            return "Sin datos para resolver.";
        }

        if (requiereResolucionAutorizadaParaNomina)
        {
            return "El tiempo extra se autoriza por periodo en Asistencias Semanal. Aquí solo revisa marcaciones, permisos, turno y perdón manual del día.";
        }

        if (AsistenciaActual.MinutosExtra > 0)
        {
            return "Cuando el permiso y las marcaciones ya están claros, aquí decides si el extra se paga o se envía a banco.";
        }

        if (ObtenerMinutosFaltanteNeto(AsistenciaActual) > 0)
        {
            return "Si después de revisar permiso y compensación aún queda faltante real, aquí puedes cubrirlo con banco si hay saldo.";
        }

        return "Usa esta sección solo cuando quede una decisión pendiente de pago o banco.";
    }

    private int ObtenerMaximoMinutosResolublesResumen()
    {
        if (AsistenciaActual == null)
        {
            return 0;
        }

        // Sin turno: como el procesador no auto-detecta extra, el máximo resoluble
        // es el tiempo total trabajado (neto), para que el usuario pueda decidir
        // cuánto de ese tiempo es extra.
        if (AsistenciaActual.TurnoBaseId is null)
        {
            return Math.Max(0, AsistenciaActual.MinutosTrabajadosNetos);
        }

        return ObtenerMinutosResolubles(AsistenciaActual);
    }

    private int ObtenerMaximoMinutosCubrirBancoResumen()
        => AsistenciaActual == null ? 0 : ObtenerMinutosFaltanteNeto(AsistenciaActual);

    private string ObtenerTituloCapturaTiempoResumen()
        => tipoResolucionTiempoExtra switch
        {
            "PagarTodo" => "Minutos aprobados a pagar",
            "BancoTodo" => "Minutos aprobados a banco",
            "MitadMitad" => "Minutos aprobados",
            "CubrirFaltanteConBanco" => "Minutos a cubrir con banco",
            _ => "Minutos aprobados"
        };

    private string ObtenerAyudaCapturaTiempoResumen()
    {
        if (AsistenciaActual == null)
        {
            return "Sin datos para capturar.";
        }

        var resoluble = ObtenerMinutosResolubles(AsistenciaActual);
        var faltante = ObtenerMinutosFaltanteNeto(AsistenciaActual);

        return tipoResolucionTiempoExtra switch
        {
            "PagarTodo" => $"Puedes aprobar hasta {FormatearMinutos(resoluble)} para pago.",
            "BancoTodo" => $"Puedes aprobar hasta {FormatearMinutos(resoluble)} para banco.",
            "MitadMitad" => $"La suma de pago y banco no debe exceder {FormatearMinutos(resoluble)}.",
            "CubrirFaltanteConBanco" => $"Puedes cubrir hasta {FormatearMinutos(faltante)} con banco.",
            _ => "Selecciona una resolución para capturar el aprobado."
        };
    }

    private static string FormatearMinutosCaptura(int minutos)
    {
        var horas = Math.Max(0, minutos) / 60;
        var restantes = Math.Max(0, minutos) % 60;
        return $"{horas}:{restantes:00}";
    }

    private void AjustarResolucionTiempoSugerida()
    {
        if (AsistenciaActual == null)
        {
            return;
        }

        var resoluble = RrhhTiempoExtraPolicy.ObtenerMinutosExtraResolubles(AsistenciaActual, factorTiempoExtraConfigurado);

        // Sin turno: sugerir el excedente sobre 8h (480 min) como extra, pero el usuario
        // puede ajustar manualmente cualquier valor hasta el total trabajado.
        // Día de descanso con turno (jornada neta = 0) y trabajo real: sugerir todo lo
        // trabajado como extra porque la jornada programada no detectó nada.
        var jornadaNetaProgramada = Math.Max(0, AsistenciaActual.MinutosJornadaNetaProgramada);
        var sugerido = (AsistenciaActual.TurnoBaseId is null
                        || jornadaNetaProgramada <= 0) && Math.Max(0, AsistenciaActual.MinutosTrabajadosNetos) > 0
            ? (AsistenciaActual.TurnoBaseId is null
                ? Math.Max(0, AsistenciaActual.MinutosTrabajadosNetos - 480)
                : Math.Max(0, AsistenciaActual.MinutosTrabajadosNetos))
            : ObtenerMinutosExtraSugeridosModo(AsistenciaActual);

        var baseCaptura = Math.Min(sugerido, resoluble);
        var faltante = RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteNeto(AsistenciaActual);
        minutosExtraPagoCaptura = 0;
        minutosExtraBancoCaptura = 0;
        minutosCubrirBancoCaptura = 0;

        switch (tipoResolucionTiempoExtra)
        {
            case "PagarTodo":
                minutosExtraPagoCaptura = baseCaptura;
                break;
            case "BancoTodo":
                minutosExtraBancoCaptura = baseCaptura;
                break;
            case "MitadMitad":
                minutosExtraPagoCaptura = baseCaptura / 2;
                minutosExtraBancoCaptura = baseCaptura - minutosExtraPagoCaptura;
                break;
            case "CubrirFaltanteConBanco":
                minutosCubrirBancoCaptura = faltante;
                break;
        }
    }

    private async Task GuardarModoSugerenciaYReprocesarAsync()
    {
        if (AsistenciaActual == null || !PuedeAprobarTiempoExtra)
        {
            return;
        }

        error = null;
        ok = null;

        var usuarioActual = await ObtenerUsuarioActualAsync();
        await using var db = await DbFactory.CreateDbContextAsync();
        try
        {
            var asistencia = await db.RrhhAsistencias.FirstOrDefaultAsync(a => a.Id == AsistenciaActual.Id);
            if (asistencia == null)
            {
                error = "No se encontró la asistencia actual.";
                return;
            }

            asistencia.ModoSugerenciaExtra = modoSugerenciaExtra;
            asistencia.UpdatedAt = DateTime.UtcNow;
            asistencia.UpdatedBy = usuarioActual;
            await db.SaveChangesAsync();

            await ReprocesarYRefrescarDiaAsync(db, AsistenciaActual.Fecha, $"Modo de cálculo cambiado a {(modoSugerenciaExtra == "NetoVsNeto" ? "neto vs neto" : "entrada/salida")}. Día reprocesado.", recargarResolucion: true);

            tipoResolucionTiempoExtra = string.IsNullOrWhiteSpace(asistencia.ResolucionTiempoExtra) ? "PagarTodo" : asistencia.ResolucionTiempoExtra;
            AjustarResolucionTiempoSugerida();
            minutosExtraPagoTexto = FormatearMinutosCaptura(minutosExtraPagoCaptura);
            minutosExtraBancoTexto = FormatearMinutosCaptura(minutosExtraBancoCaptura);
        }
        catch (Exception ex)
        {
            error = ex.InnerException?.Message ?? ex.Message;
        }
    }

    private void CargarCapturaResolucion(RrhhAsistencia asistencia)
    {
        if (!string.IsNullOrWhiteSpace(asistencia.ResolucionTiempoExtra))
        {
            tipoResolucionTiempoExtra = asistencia.ResolucionTiempoExtra;
            minutosExtraPagoCaptura = asistencia.MinutosExtraAutorizadosPago;
            minutosExtraBancoCaptura = asistencia.MinutosExtraAutorizadosBanco;
            minutosCubrirBancoCaptura = asistencia.MinutosCubiertosBancoHoras;
            minutosExtraPagoTexto = FormatearMinutosCaptura(minutosExtraPagoCaptura);
            minutosExtraBancoTexto = FormatearMinutosCaptura(minutosExtraBancoCaptura);
            factorTiempoExtraOverrideCaptura = factorTiempoExtraConfigurado;
            return;
        }

        AjustarResolucionTiempoSugerida();
        minutosExtraPagoTexto = FormatearMinutosCaptura(minutosExtraPagoCaptura);
        minutosExtraBancoTexto = FormatearMinutosCaptura(minutosExtraBancoCaptura);
        factorTiempoExtraOverrideCaptura = factorTiempoExtraConfigurado;
    }

    private void CambiarModoSugerenciaExtra(string nuevoModo)
    {
        if (AsistenciaActual == null || nuevoModo == modoSugerenciaExtra)
        {
            return;
        }

        modoSugerenciaExtra = nuevoModo;
        AjustarResolucionTiempoSugerida();
        minutosExtraPagoTexto = FormatearMinutosCaptura(minutosExtraPagoCaptura);
        minutosExtraBancoTexto = FormatearMinutosCaptura(minutosExtraBancoCaptura);
        StateHasChanged();
    }

    private void AlternarAccionesRapidasModoExtra()
    {
        var nuevoEstado = !_mostrarAccionesRapidasModoExtra;
        _mostrarAccionesRapidasModoExtra = nuevoEstado;
        if (nuevoEstado)
        {
            _mostrarAccionesRapidasPermiso = false;
            _mostrarAccionesRapidasTurno = false;
            _mostrarResumenTiempoExtraBanco = false;
        }
    }

    private void AlternarResumenTiempoExtraBanco()
    {
        var nuevoEstado = !_mostrarResumenTiempoExtraBanco;
        _mostrarResumenTiempoExtraBanco = nuevoEstado;
        if (nuevoEstado)
        {
            _mostrarAccionesRapidasPermiso = false;
            _mostrarAccionesRapidasTurno = false;
            _mostrarAccionesRapidasModoExtra = false;
        }
    }

    private async Task CargarContextoTiempoExtraAsync(CrmDbContext db)
    {
        if (AsistenciaActual == null)
        {
            return;
        }

        var contexto = await TiempoExtraResolutionService.ObtenerContextoEmpleadoAsync(db, _empresaId, AsistenciaActual.EmpleadoId);
        AplicarContextoTiempoExtra(contexto);
    }

    private void AplicarContextoTiempoExtra(RrhhTiempoExtraEmpleadoContexto contexto)
    {
        saldoBancoHorasSeleccionado = contexto.SaldoBancoHorasMinutos;
        topeBancoHorasConfigurado = contexto.Configuracion.TopeBancoMinutos;
        factorTiempoExtraConfigurado = contexto.Configuracion.FactorTiempoExtra;
        bancoHorasHabilitadoConfigurado = contexto.Configuracion.BancoHorasHabilitado;
        factorAcumulacionBancoHorasConfigurado = contexto.Configuracion.FactorAcumulacionBancoHoras;
        requiereResolucionAutorizadaParaNomina = contexto.Configuracion.RequiereResolucionAutorizadaParaNomina;
    }

    private static bool DebeMostrarResolucionTiempo(RrhhAsistencia asistencia)
        => asistencia.MinutosExtra > 0 || RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteNeto(asistencia) > 0 || asistencia.MinutosCubiertosBancoHoras > 0;
}