using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Core.Models;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Components.Pages.RRHH;

public partial class Asistencias : ComponentBase
{
    [Inject] private IRrhhAsistenciasPageService AsistenciasPageService { get; set; } = default!;

    private readonly List<RrhhAsistenciaEstatus> estatusDisponibles = Enum.GetValues<RrhhAsistenciaEstatus>().ToList();
    private List<RrhhAsistencia> lista = [];
    private List<TurnoBase> turnos = [];
    private List<Empleado> empleadosReproceso = [];
    private bool _puedeVer;
    private bool _puedeReprocesar;
    private bool _puedeAprobarTiempoExtra;
    private Guid _empresaId;
    private bool cargando;
    private bool reprocesando;
    private bool mostrarReproceso;
    private bool mostrarCorreccionDia;
    private bool exportandoCsv;
    private string? error;
    private string? ok;
    private DateTime? filtroDesde = DateTime.Today.AddDays(-15);
    private DateTime? filtroHasta = DateTime.Today;
    private string filtroTurnoIdTexto = string.Empty;
    private string filtroEstatus = "todos";
    private string filtroRevision = "todos";
    private string? filtroEmpleado;
    private DateTime? reprocesoDesde = DateTime.Today.AddDays(-7);
    private DateTime? reprocesoHasta = DateTime.Today;
    private string reprocesoEmpleadoIdTexto = string.Empty;
    private string? reprocesoMotivo;
    private RrhhAsistencia? asistenciaSeleccionada;
    private Dictionary<string, string> ausenciasPorDia = new();
    private Dictionary<string, int> compensacionesPorDia = new();

    protected override async Task OnInitializedAsync()
    {
        await CargarAccesoAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || !_puedeVer)
        {
            return;
        }

        await CargarFiltrosPersistidosAsync();
        await CargarAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task CargarAccesoAsync()
    {
        var state = await AuthStateProvider.GetAuthenticationStateAsync();
        _puedeVer = state.User.HasClaim("Capacidad", "empleados.ver") || state.User.HasClaim("Capacidad", "nominas.ver");
        _puedeReprocesar = state.User.HasClaim("Capacidad", "empleados.editar") || state.User.HasClaim("Capacidad", "nominas.editar");
        _puedeAprobarTiempoExtra = state.User.HasClaim("Capacidad", "rrhh.tiempoextra.aprobar") || state.User.HasClaim("Capacidad", "nominas.editar");
        _ = Guid.TryParse(state.User.FindFirst("EmpresaId")?.Value, out _empresaId);
    }

    private async Task CargarAsync()
    {
        cargando = true;
        error = null;

        try
        {
            if (_empresaId == Guid.Empty)
            {
                error = "No hay empresa activa.";
                return;
            }

            await using var db = await DbFactory.CreateDbContextAsync();
            var resultado = await AsistenciasPageService.CargarAsync(db, _empresaId, CrearEstadoFiltros());
            turnos = resultado.Turnos.ToList();
            empleadosReproceso = resultado.EmpleadosReproceso.ToList();
            lista = resultado.Asistencias.ToList();
            ausenciasPorDia = resultado.AusenciasPorDia;
            compensacionesPorDia = resultado.CompensacionesPorDia;
            await GuardarFiltrosPersistidosAsync();
        }
        catch (Exception ex)
        {
            error = ex.InnerException?.Message ?? ex.Message;
        }
        finally
        {
            cargando = false;
        }
    }

    private async Task LimpiarFiltros()
    {
        AplicarFiltros(RrhhAsistenciasFiltroState.CreateDefault());
        await LimpiarFiltrosPersistidosAsync();
        await CargarAsync();
    }

    private async Task CargarFiltrosPersistidosAsync()
    {
        try
        {
            var filtrosJson = await JS.InvokeAsync<string?>("mundoVsAuth.getLocal", RrhhAsistenciasFiltroState.StorageKey);
            if (string.IsNullOrWhiteSpace(filtrosJson))
            {
                AplicarFiltros(RrhhAsistenciasFiltroState.CreateDefault());
                return;
            }

            var filtros = JsonSerializer.Deserialize<RrhhAsistenciasFiltroState>(filtrosJson);
            AplicarFiltros(filtros ?? RrhhAsistenciasFiltroState.CreateDefault());
        }
        catch
        {
            AplicarFiltros(RrhhAsistenciasFiltroState.CreateDefault());
        }
    }

    private async Task GuardarFiltrosPersistidosAsync()
    {
        var filtros = CrearEstadoFiltros();
        var filtrosJson = JsonSerializer.Serialize(filtros);
        await JS.InvokeVoidAsync("mundoVsAuth.setLocal", RrhhAsistenciasFiltroState.StorageKey, filtrosJson);
    }

    private async Task LimpiarFiltrosPersistidosAsync()
        => await JS.InvokeVoidAsync("mundoVsAuth.removeLocal", RrhhAsistenciasFiltroState.StorageKey);

    private RrhhAsistenciasFiltroState CrearEstadoFiltros()
        => new()
        {
            Desde = filtroDesde,
            Hasta = filtroHasta,
            TurnoIdTexto = filtroTurnoIdTexto,
            Estatus = filtroEstatus,
            Revision = filtroRevision,
            Empleado = string.IsNullOrWhiteSpace(filtroEmpleado) ? null : filtroEmpleado.Trim()
        };

    private void AplicarFiltros(RrhhAsistenciasFiltroState filtros)
    {
        filtroDesde = filtros.Desde;
        filtroHasta = filtros.Hasta;
        filtroTurnoIdTexto = filtros.TurnoIdTexto ?? string.Empty;
        filtroEstatus = string.IsNullOrWhiteSpace(filtros.Estatus) ? "todos" : filtros.Estatus;
        filtroRevision = string.IsNullOrWhiteSpace(filtros.Revision) ? "todos" : filtros.Revision;
        filtroEmpleado = string.IsNullOrWhiteSpace(filtros.Empleado) ? null : filtros.Empleado.Trim();
    }

    private void AlternarReproceso()
    {
        mostrarReproceso = !mostrarReproceso;
        if (!mostrarReproceso)
        {
            reprocesoMotivo = null;
            reprocesoEmpleadoIdTexto = string.Empty;
            reprocesoDesde = DateTime.Today.AddDays(-7);
            reprocesoHasta = DateTime.Today;
        }
    }

    private async Task EjecutarReprocesoAsync()
    {
        error = null;
        ok = null;

        if (!_puedeReprocesar)
        {
            error = "No tienes permisos para reprocesar asistencias.";
            return;
        }

        if (_empresaId == Guid.Empty)
        {
            error = "No hay empresa activa.";
            return;
        }

        if (!reprocesoDesde.HasValue || !reprocesoHasta.HasValue)
        {
            error = "Debes capturar el rango de fechas a reprocesar.";
            return;
        }

        var fechaDesde = DateOnly.FromDateTime(reprocesoDesde.Value.Date);
        var fechaHasta = DateOnly.FromDateTime(reprocesoHasta.Value.Date);
        if (fechaHasta < fechaDesde)
        {
            error = "La fecha final no puede ser menor a la fecha inicial.";
            return;
        }

        var rangoDias = fechaHasta.DayNumber - fechaDesde.DayNumber + 1;
        var maxDiasReproceso = Math.Max(1, Configuration.GetValue<int?>("Asistencia:ReprocesoMaxDays") ?? 31);
        if (rangoDias > maxDiasReproceso)
        {
            error = $"El rango máximo permitido para reproceso es de {maxDiasReproceso} día(s).";
            return;
        }

        var empleadoId = Guid.TryParse(reprocesoEmpleadoIdTexto, out var empleadoSeleccionadoId) && empleadoSeleccionadoId != Guid.Empty
            ? empleadoSeleccionadoId
            : (Guid?)null;

        reprocesando = true;
        try
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            var grupos = await RrhhAsistenciaProcessor.ReprocesarRangoAsync(db, _empresaId, fechaDesde, fechaHasta, empleadoId);

            var state = await AuthStateProvider.GetAuthenticationStateAsync();
            db.RrhhLogsChecador.Add(new RrhhLogChecador
            {
                Id = Guid.NewGuid(),
                EmpresaId = _empresaId,
                FechaUtc = DateTime.UtcNow,
                Nivel = "Information",
                Mensaje = "Se ejecutó un reproceso histórico de asistencias desde la UI.",
                Detalle = $"usuario={state.User.Identity?.Name ?? "desconocido"};desde={fechaDesde:yyyy-MM-dd};hasta={fechaHasta:yyyy-MM-dd};empleado={empleadoId};grupos={grupos};motivo={(string.IsNullOrWhiteSpace(reprocesoMotivo) ? "sin motivo" : reprocesoMotivo.Trim())}",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });
            await db.SaveChangesAsync();

            ok = $"Reproceso completado. Grupos recalculados: {grupos}.";
            mostrarReproceso = false;
            await CargarAsync();
        }
        catch (Exception ex)
        {
            error = ex.InnerException?.Message ?? ex.Message;
        }
        finally
        {
            reprocesando = false;
        }
    }

    private Task AbrirCorreccionDiaAsync(RrhhAsistencia asistencia)
    {
        asistenciaSeleccionada = asistencia;
        mostrarCorreccionDia = true;
        return Task.CompletedTask;
    }

    private Task RecargarListadoAsync()
        => CargarAsync();

    private void CerrarCorreccionDia()
    {
        mostrarCorreccionDia = false;
        asistenciaSeleccionada = null;
    }

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
            return EsRevisionOperativaLeve(asistencia)
                ? "bg-success"
                : "bg-warning text-dark";
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

    private static PresentacionRevision ObtenerVisualRevision(RrhhAsistencia asistencia)
    {
        var senal = ObtenerSenalRevisionOperativa(asistencia);
        var observaciones = asistencia.Observaciones ?? string.Empty;
        var resumenResolucion = RrhhTiempoExtraPolicy.ObtenerResumenResolucion(asistencia);

        if (TieneResolucionTiempoAplicada(asistencia))
        {
            return new PresentacionRevision(
                senal switch
                {
                    "Posible cambio de turno" => "Turno ok",
                    "Jornada irregular" => "Extra ok",
                    _ => "Resuelta"
                },
                CombinarTooltip(
                    senal is null ? "Incidencia atendida manualmente." : $"{senal} atendida manualmente.",
                    resumenResolucion,
                    observaciones),
                "asis-chip--resolved",
                "bi-shield-check",
                true);
        }

        if (!asistencia.RequiereRevision)
        {
            return new PresentacionRevision(
                TieneResolucionTiempoAplicada(asistencia) || !string.IsNullOrWhiteSpace(asistencia.ResolucionTiempoExtra)
                    ? "Verificada"
                    : "Revisada",
                senal ?? "El día ya fue verificado y no tiene intervención manual pendiente.",
                "asis-chip--success",
                "bi-check-circle-fill",
                true);
        }

        if (EsRevisionOperativaLeve(asistencia))
        {
            return new PresentacionRevision(
                "Ajuste fácil",
                CombinarTooltip(
                    "La variación detectada es menor y normalmente se resuelve rápido.",
                    observaciones),
                "asis-chip--success",
                "bi-magic",
                false);
        }

        if (observaciones.Contains("zona ambigua", StringComparison.OrdinalIgnoreCase))
        {
            return new PresentacionRevision("Ambigua", "El día quedó entre umbrales y requiere confirmación operativa.", "asis-chip--warn", "bi-exclamation-triangle", false);
        }

        if (observaciones.Contains("salió temprano en lugar de tomar el descanso", StringComparison.OrdinalIgnoreCase))
        {
            return new PresentacionRevision("Compensa?", "Validar si la salida temprana sustituyó el descanso del turno.", "asis-chip--info", "bi-hourglass-split", false);
        }

        return new PresentacionRevision(
            senal switch
            {
                "Posible cambio de turno" => "Turno?",
                "Jornada irregular" => "Irregular",
                _ => "Revisar"
            },
            senal == null ? "Hay incidencias del día que requieren validación." : observaciones,
            "asis-chip--danger",
            "bi-exclamation-circle",
            false);
    }

    private static bool EsRevisionOperativaLeve(RrhhAsistencia asistencia)
    {
        if (!asistencia.RequiereRevision)
        {
            return false;
        }

        if (asistencia.Estatus != RrhhAsistenciaEstatus.AsistenciaNormal)
        {
            return false;
        }

        if (asistencia.MinutosRetardo > 0
            || asistencia.MinutosSalidaAnticipada > 0
            || asistencia.MinutosExtra > 0)
        {
            return false;
        }

        var observaciones = asistencia.Observaciones ?? string.Empty;
        if (observaciones.Contains("zona ambigua", StringComparison.OrdinalIgnoreCase)
            || observaciones.Contains("solo se detectó una marcación", StringComparison.OrdinalIgnoreCase)
            || observaciones.Contains("marca no reconocida", StringComparison.OrdinalIgnoreCase)
            || observaciones.Contains("sin turno", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return observaciones.Contains("cambio de turno", StringComparison.OrdinalIgnoreCase)
            || observaciones.Contains("jornada extraordinaria", StringComparison.OrdinalIgnoreCase);
    }

    private static PresentacionDescansos ObtenerVisualDescansos(RrhhAsistencia asistencia)
    {
        var observaciones = asistencia.Observaciones ?? string.Empty;
        var resumen = string.IsNullOrWhiteSpace(asistencia.ResumenDescansos) ? "Sin detalle capturado." : asistencia.ResumenDescansos!;
        var resumenMinutos = $"{asistencia.MinutosDescansoTomado} / {asistencia.MinutosDescansoProgramado}m";
        var tooltip = CombinarTooltip(
            resumen,
            $"Tomado {FormatearMinutos(asistencia.MinutosDescansoTomado)} · Prog. {FormatearMinutos(asistencia.MinutosDescansoProgramado)}");

        if (asistencia.MinutosDescansoProgramado == 0)
        {
            return new PresentacionDescansos("Sin desc.", CombinarTooltip(resumen, "Sin tiempo configurado para descontar."), "0 / 0m", "asis-chip--muted", "asis-break-card--muted", "bi-moon-stars");
        }

        if (observaciones.Contains("Se descontaron", StringComparison.OrdinalIgnoreCase))
        {
            return new PresentacionDescansos("Auto", tooltip, resumenMinutos, "asis-chip--success", "asis-break-card--success", "bi-magic");
        }

        if (observaciones.Contains("salió temprano en lugar de tomar el descanso", StringComparison.OrdinalIgnoreCase))
        {
            return new PresentacionDescansos("Conf. sal.", tooltip, resumenMinutos, "asis-chip--info", "asis-break-card--info", "bi-box-arrow-left");
        }

        if (observaciones.Contains("zona ambigua", StringComparison.OrdinalIgnoreCase))
        {
            return new PresentacionDescansos("Ambiguo", tooltip, resumenMinutos, "asis-chip--warn", "asis-break-card--warn", "bi-question-diamond");
        }

        if (asistencia.MinutosDescansoTomado > 0)
        {
            return new PresentacionDescansos("Detect.", tooltip, resumenMinutos, "asis-chip--success", "asis-break-card--success", "bi-cup-hot");
        }

        return new PresentacionDescansos("Sin marca", tooltip, resumenMinutos, "asis-chip--muted", "asis-break-card--muted", "bi-clock-history");
    }

    private PresentacionDato ObtenerVisualAusencia(RrhhAsistencia asistencia)
    {
        var resumen = ObtenerResumenAusencia(asistencia);
        if (resumen == "—")
        {
            return new PresentacionDato("—", "Sin ausencia registrada.", "asis-chip--muted", "bi-calendar2-check");
        }

        if (resumen.Contains("Vacaciones", StringComparison.OrdinalIgnoreCase))
        {
            return new PresentacionDato("Vac.", resumen, "asis-chip--info", "bi-umbrella");
        }

        return new PresentacionDato(
            resumen.Contains("con goce", StringComparison.OrdinalIgnoreCase) ? "Perm. +" : "Perm. -",
            resumen,
            resumen.Contains("con goce", StringComparison.OrdinalIgnoreCase) ? "asis-chip--warn" : "asis-chip--dark",
            "bi-door-open");
    }

    private PresentacionResolucion ObtenerVisualResolucion(RrhhAsistencia asistencia)
    {
        var resumenAusencia = ObtenerResumenAusencia(asistencia);
        var tooltip = ObtenerResumenResolucion(asistencia, resumenAusencia);
        var resumen = new List<string>();

        if (asistencia.MinutosExtraAutorizadosPago > 0)
        {
            resumen.Add($"P {asistencia.MinutosExtraAutorizadosPago}m");
        }

        if (asistencia.MinutosExtraAutorizadosBanco > 0)
        {
            resumen.Add($"B {asistencia.MinutosExtraAutorizadosBanco}m");
        }

        if (asistencia.MinutosCubiertosBancoHoras > 0)
        {
            resumen.Add($"C {asistencia.MinutosCubiertosBancoHoras}m");
        }

        if (resumen.Count > 0)
        {
            return new PresentacionResolucion(
                asistencia.MinutosExtraAutorizadosPago > 0 && asistencia.MinutosExtraAutorizadosBanco > 0
                    ? "Mixta"
                    : asistencia.MinutosExtraAutorizadosPago > 0
                        ? "Pago"
                        : asistencia.MinutosExtraAutorizadosBanco > 0
                            ? "Banco"
                            : "Cubre",
                string.Join(" · ", resumen),
                tooltip,
                asistencia.MinutosExtraAutorizadosPago > 0 && asistencia.MinutosExtraAutorizadosBanco > 0
                    ? "asis-chip--resolved"
                    : asistencia.MinutosExtraAutorizadosPago > 0
                        ? "asis-chip--success"
                        : asistencia.MinutosExtraAutorizadosBanco > 0
                            ? "asis-chip--info"
                            : "asis-chip--warn",
                asistencia.MinutosExtraAutorizadosPago > 0 && asistencia.MinutosExtraAutorizadosBanco > 0
                    ? "bi-shuffle"
                    : asistencia.MinutosExtraAutorizadosPago > 0
                        ? "bi-cash-coin"
                        : asistencia.MinutosExtraAutorizadosBanco > 0
                            ? "bi-piggy-bank"
                            : "bi-shield-check");
        }

        if (!string.IsNullOrWhiteSpace(asistencia.ResolucionTiempoExtra))
        {
            return new PresentacionResolucion("Ajuste", ObtenerTextoResolucionCorto(asistencia.ResolucionTiempoExtra), tooltip, "asis-chip--resolved", "bi-sliders");
        }

        if (asistencia.MinutosExtra > 0)
        {
            return new PresentacionResolucion(
                "Pend.",
                $"Sin aprobar {asistencia.MinutosExtra}m",
                CombinarTooltip(
                    tooltip,
                    "Hay tiempo extra detectado, pero todavía no tiene aprobación para pago o banco."),
                "asis-chip--warn",
                "bi-hourglass-split");
        }

        if (RrhhTiempoExtraPolicy.TieneCoberturaAusencia(resumenAusencia))
        {
            var esVacacion = resumenAusencia.Contains("Vacaciones", StringComparison.OrdinalIgnoreCase);
            return new PresentacionResolucion(
                esVacacion ? "Ausencia" : "Permiso",
                ObtenerResumenResolucionAusenciaCorto(resumenAusencia),
                CombinarTooltip(
                    resumenAusencia,
                    asistencia.RequiereRevision
                        ? "Existe una cobertura registrada; si aún ves revisión, valida marcaciones o turno."
                        : "El día ya tiene una cobertura operativa registrada."),
                esVacacion ? "asis-chip--info" : "asis-chip--resolved",
                esVacacion ? "bi-umbrella" : "bi-shield-check");
        }

        if (!RrhhTiempoExtraPolicy.TieneResolucionOperativaPendiente(asistencia, resumenAusencia))
        {
            return new PresentacionResolucion("OK", "Sin pend.", tooltip, "asis-chip--success", "bi-check2-circle");
        }

        return new PresentacionResolucion("Pend.", "Revisar", tooltip, "asis-chip--muted", "bi-hourglass-split");
    }

    private static string ObtenerClaseMetricaIncidencia(int minutos)
        => minutos > 0 ? "asis-chip--warn" : "asis-chip--muted";

    private static string ObtenerClaseMetricaExtra(int minutos)
        => minutos > 0 ? "asis-chip--success" : "asis-chip--muted";

    private static string FormatearMinutosCortos(int minutos)
        => minutos <= 0 ? "0 min" : $"{minutos} min";

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

    private static string ObtenerTooltipObservaciones(RrhhAsistencia asistencia)
        => CombinarTooltip(ObtenerObservacionPrincipal(asistencia), ObtenerObservacionSecundaria(asistencia));

    private static string TruncarTexto(string? texto, int longitudMaxima)
    {
        if (string.IsNullOrWhiteSpace(texto) || texto.Length <= longitudMaxima)
        {
            return string.IsNullOrWhiteSpace(texto) ? "—" : texto;
        }

        return texto[..Math.Max(0, longitudMaxima - 1)].TrimEnd() + "…";
    }

    private static string FormatearMinutos(int minutos)
    {
        var horas = minutos / 60;
        var minutosRestantes = Math.Abs(minutos % 60);
        return $"{horas}:{minutosRestantes:00} h";
    }

    private static string FormatearHoraCompacta(TimeSpan? hora)
        => hora?.ToString("hh\\:mm") ?? "—";

    private static string ObtenerDiaSemanaCorto(DateOnly fecha)
        => fecha.DayOfWeek switch
        {
            DayOfWeek.Monday => "lun",
            DayOfWeek.Tuesday => "mar",
            DayOfWeek.Wednesday => "mié",
            DayOfWeek.Thursday => "jue",
            DayOfWeek.Friday => "vie",
            DayOfWeek.Saturday => "sáb",
            _ => "dom"
        };

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

    private static bool TieneResolucionTiempoAplicada(RrhhAsistencia asistencia)
        => asistencia.MinutosExtraAutorizadosPago > 0
            || asistencia.MinutosExtraAutorizadosBanco > 0
            || asistencia.MinutosCubiertosBancoHoras > 0
            || !string.IsNullOrWhiteSpace(asistencia.ResolucionTiempoExtra);

    private static string CombinarTooltip(params string?[] textos)
    {
        var partes = textos
            .Where(texto => !string.IsNullOrWhiteSpace(texto))
            .Select(texto => texto!.Trim())
            .Distinct()
            .ToList();

        return partes.Count == 0 ? "Sin detalle adicional." : string.Join(" | ", partes);
    }

    private static string ObtenerTextoResolucionCorto(string resolucion)
        => resolucion switch
        {
            "PagarTodo" => "Todo pago",
            "BancoTodo" => "Todo banco",
            "MitadMitad" => "50/50",
            "CubrirFaltanteConBanco" => "Cubre banco",
            "SinAccion" => "Sin acción",
            _ => resolucion
        };

    private static string ObtenerResumenResolucionAusenciaCorto(string resumenAusencia)
    {
        if (resumenAusencia.Contains("Vacaciones", StringComparison.OrdinalIgnoreCase))
        {
            return "Vacaciones";
        }

        if (resumenAusencia.Contains("con goce", StringComparison.OrdinalIgnoreCase))
        {
            return "Con goce";
        }

        if (resumenAusencia.Contains("sin goce", StringComparison.OrdinalIgnoreCase))
        {
            return "Sin goce";
        }

        return "Aplicado";
    }

    private static string ObtenerTextoEstatusCorto(RrhhAsistenciaEstatus estatus)
        => estatus switch
        {
            RrhhAsistenciaEstatus.DescansoTrabajado => "Desc. trab.",
            RrhhAsistenciaEstatus.Incompleta => "Incompl.",
            RrhhAsistenciaEstatus.TurnoNoAsignado => "Sin turno",
            RrhhAsistenciaEstatus.MarcaNoReconocida => "No recon.",
            _ => ObtenerTextoEstatus(estatus)
        };

    private static string ObtenerClaseFilaAsistencia(RrhhAsistencia asistencia)
    {
        if (asistencia.RequiereRevision)
        {
            return "asis-row--review";
        }

        if (TieneResolucionTiempoAplicada(asistencia))
        {
            return "asis-row--resolved";
        }

        return "asis-row--normal";
    }

    private static string CrearClaveAusencia(Guid empleadoId, DateOnly fecha)
        => $"{empleadoId:N}:{fecha:yyyyMMdd}";

    private static string CrearClaveCompensacion(Guid empleadoId, DateOnly fecha)
        => $"{empleadoId:N}:{fecha:yyyyMMdd}";

    private static string FormatearAusencia(RrhhAusencia ausencia)
        => ausencia.Tipo == TipoAusenciaRrhh.Vacaciones
            ? "Vacaciones"
            : $"Permiso {(ausencia.ConGocePago ? "con goce" : "sin goce")}{(ausencia.Horas > 0 ? $" ({ausencia.Horas:0.##} h)" : string.Empty)}";

    private string ObtenerResumenAusencia(RrhhAsistencia asistencia)
        => ausenciasPorDia.TryGetValue(CrearClaveAusencia(asistencia.EmpleadoId, asistencia.Fecha), out var resumen)
            ? resumen
            : "—";

    private int ObtenerMinutosCompensados(RrhhAsistencia asistencia)
        => compensacionesPorDia.TryGetValue(CrearClaveCompensacion(asistencia.EmpleadoId, asistencia.Fecha), out var minutos)
            ? Math.Max(0, minutos)
            : 0;

    private int ObtenerMinutosTrabajadosVisibles(RrhhAsistencia asistencia)
        => RrhhTiempoExtraPolicy.ObtenerMinutosTrabajadosVisibles(asistencia, ObtenerMinutosCompensados(asistencia));

    private async Task ExportarCsvAsync()
    {
        error = null;
        ok = null;

        if (_empresaId == Guid.Empty)
        {
            error = "No hay empresa activa.";
            return;
        }

        exportandoCsv = true;
        try
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            Guid? turnoFiltroId = Guid.TryParse(filtroTurnoIdTexto, out var turnoId) && turnoId != Guid.Empty ? turnoId : null;
            RrhhAsistenciaEstatus? estatusFiltro = int.TryParse(filtroEstatus, out var estatusValor) && Enum.IsDefined(typeof(RrhhAsistenciaEstatus), estatusValor)
                ? (RrhhAsistenciaEstatus)estatusValor
                : null;
            var textoEmpleadoFiltro = string.IsNullOrWhiteSpace(filtroEmpleado) ? null : filtroEmpleado.Trim();

            var asistencias = await db.RrhhAsistencias
                .AsNoTracking()
                .Include(a => a.Empleado)
                .Include(a => a.TurnoBase)
                .Where(a => a.EmpresaId == _empresaId)
                .Where(a => !filtroDesde.HasValue || a.Fecha >= DateOnly.FromDateTime(filtroDesde.Value.Date))
                .Where(a => !filtroHasta.HasValue || a.Fecha <= DateOnly.FromDateTime(filtroHasta.Value.Date))
                .Where(a => !turnoFiltroId.HasValue || a.TurnoBaseId == turnoFiltroId.Value)
                .Where(a => !estatusFiltro.HasValue || a.Estatus == estatusFiltro.Value)
                .Where(a => filtroRevision != "si" || a.RequiereRevision)
                .Where(a => filtroRevision != "no" || !a.RequiereRevision)
                .Where(a => string.IsNullOrWhiteSpace(textoEmpleadoFiltro)
                    || a.Empleado.Nombre.Contains(textoEmpleadoFiltro)
                    || (a.Empleado.ApellidoPaterno != null && a.Empleado.ApellidoPaterno.Contains(textoEmpleadoFiltro))
                    || (a.Empleado.ApellidoMaterno != null && a.Empleado.ApellidoMaterno.Contains(textoEmpleadoFiltro))
                    || a.Empleado.NumeroEmpleado.Contains(textoEmpleadoFiltro)
                    || (a.Empleado.CodigoChecador != null && a.Empleado.CodigoChecador.Contains(textoEmpleadoFiltro)))
                .OrderBy(a => a.Fecha)
                .ThenBy(a => a.Empleado.Nombre)
                .ThenBy(a => a.Empleado.ApellidoPaterno)
                .ToListAsync();

            if (asistencias.Count == 0)
            {
                error = "No hay asistencias para exportar con los filtros seleccionados.";
                return;
            }

            var ausenciasExportacion = await AsistenciasPageService.CargarAsync(db, _empresaId, CrearEstadoFiltros());
            var ausenciasPorDiaExportacion = ausenciasExportacion.AusenciasPorDia;
            var empleadoIds = asistencias.Select(a => a.EmpleadoId).Distinct().ToList();
            var fechaMinima = asistencias.Min(a => a.Fecha).ToDateTime(TimeOnly.MinValue).AddHours(-14);
            var fechaMaximaExclusiva = asistencias.Max(a => a.Fecha).AddDays(1).ToDateTime(TimeOnly.MinValue).AddHours(14);
            var desdeUtc = DateTime.SpecifyKind(fechaMinima, DateTimeKind.Utc);
            var hastaUtc = DateTime.SpecifyKind(fechaMaximaExclusiva, DateTimeKind.Utc);

            var marcaciones = await db.RrhhMarcaciones
                .AsNoTracking()
                .Include(m => m.Checador)
                .Where(m => m.EmpresaId == _empresaId
                    && m.IsActive
                    && !m.EsAnulada
                    && m.EmpleadoId.HasValue
                    && empleadoIds.Contains(m.EmpleadoId.Value)
                    && m.FechaHoraMarcacionUtc >= desdeUtc
                    && m.FechaHoraMarcacionUtc < hastaUtc)
                .OrderBy(m => m.FechaHoraMarcacionUtc)
                .ToListAsync();

            var marcacionesPorDia = marcaciones
                .Where(m => m.EmpleadoId.HasValue)
                .GroupBy(m => CrearClaveAusencia(m.EmpleadoId!.Value, DateOnly.FromDateTime(ObtenerFechaHoraLocalMarcacion(m))))
                .ToDictionary(g => g.Key, g => g.OrderBy(x => ObtenerFechaHoraLocalMarcacion(x)).ToList());

            var saldosBanco = (await db.RrhhBancoHorasMovimientos
                .AsNoTracking()
                .Where(m => m.EmpresaId == _empresaId && m.IsActive && empleadoIds.Contains(m.EmpleadoId))
                .GroupBy(m => m.EmpleadoId)
                .Select(g => new { EmpleadoId = g.Key, Horas = g.Sum(x => x.Horas) })
                .ToListAsync())
                .ToDictionary(x => x.EmpleadoId, x => (int)Math.Round(x.Horas * 60m, MidpointRounding.AwayFromZero));

            var minutosPagoEstimado = CalcularMinutosPagoDobleyTriple(asistencias);

            var builder = new StringBuilder();
            builder.AppendLine("Fecha,Empleado,NumeroEmpleado,CodigoChecador,Turno,Estatus,RequiereRevision,EntradaProgramada,SalidaProgramada,EntradaReal,SalidaReal,Descanso1Entrada,Descanso1Salida,Descanso2Entrada,Descanso2Salida,TiempoDescanso1,TiempoDescanso1Min,TiempoDescanso2,TiempoDescanso2Min,TiempoTrabajadoNeto,TiempoTrabajadoNetoMin,TiempoTrabajadoBruto,TiempoTrabajadoBrutoMin,DescansoProgramado,DescansoProgramadoMin,DescansoTomado,DescansoTomadoMin,DescansoPagado,DescansoPagadoMin,DescansoNoPagado,DescansoNoPagadoMin,RetardoMin,SalidaAnticipadaMin,HorasExtra,HorasExtraMin,ExtraAutorizadoPago,ExtraAutorizadoPagoMin,PagadasDoble,PagadasDobleMin,PagadasTriple,PagadasTripleMin,BancoHorasGenerado,BancoHorasGeneradoMin,CubiertoConBanco,CubiertoConBancoMin,SaldoBancoHorasActual,SaldoBancoHorasActualMin,JornadaProgramada,JornadaProgramadaMin,JornadaNetaProgramada,JornadaNetaProgramadaMin,TotalMarcaciones,Ausencia,ResumenDescansos,ResolucionTiempoExtra,Observaciones,CriterioPagoExtra");

            foreach (var asistencia in asistencias)
            {
                var claveDia = CrearClaveAusencia(asistencia.EmpleadoId, asistencia.Fecha);
                marcacionesPorDia.TryGetValue(claveDia, out var marcacionesDiaExportacion);
                var descansos = ObtenerDescansosExportacion(marcacionesDiaExportacion ?? []);
                var pagoExtra = minutosPagoEstimado.GetValueOrDefault(asistencia.Id, PagoTiempoExtraExportacion.Vacio);
                var saldoBancoMin = saldosBanco.GetValueOrDefault(asistencia.EmpleadoId);

                builder.AppendLine(string.Join(",",
                    EscapeCsv(asistencia.Fecha.ToString("yyyy-MM-dd")),
                    EscapeCsv(asistencia.Empleado.NombreCompleto),
                    EscapeCsv(asistencia.Empleado.NumeroEmpleado),
                    EscapeCsv(asistencia.Empleado.CodigoChecador),
                    EscapeCsv(asistencia.TurnoBase?.Nombre ?? "Sin turno"),
                    EscapeCsv(ObtenerTextoEstatus(asistencia.Estatus)),
                    EscapeCsv(asistencia.RequiereRevision ? "Sí" : "No"),
                    EscapeCsv(FormatearHoraTurnoCsv(asistencia.HoraEntradaProgramada)),
                    EscapeCsv(FormatearHoraTurnoCsv(asistencia.HoraSalidaProgramada)),
                    EscapeCsv(FormatearHoraTurnoCsv(asistencia.HoraEntradaReal)),
                    EscapeCsv(FormatearHoraTurnoCsv(asistencia.HoraSalidaReal)),
                    EscapeCsv(descansos.Descanso1Entrada),
                    EscapeCsv(descansos.Descanso1Salida),
                    EscapeCsv(descansos.Descanso2Entrada),
                    EscapeCsv(descansos.Descanso2Salida),
                    EscapeCsv(FormatearMinutos(descansos.MinutosDescanso1)),
                    descansos.MinutosDescanso1.ToString(),
                    EscapeCsv(FormatearMinutos(descansos.MinutosDescanso2)),
                    descansos.MinutosDescanso2.ToString(),
                    EscapeCsv(FormatearMinutos(asistencia.MinutosTrabajadosNetos)),
                    asistencia.MinutosTrabajadosNetos.ToString(),
                    EscapeCsv(FormatearMinutos(asistencia.MinutosTrabajadosBrutos)),
                    asistencia.MinutosTrabajadosBrutos.ToString(),
                    EscapeCsv(FormatearMinutos(asistencia.MinutosDescansoProgramado)),
                    asistencia.MinutosDescansoProgramado.ToString(),
                    EscapeCsv(FormatearMinutos(asistencia.MinutosDescansoTomado)),
                    asistencia.MinutosDescansoTomado.ToString(),
                    EscapeCsv(FormatearMinutos(asistencia.MinutosDescansoPagado)),
                    asistencia.MinutosDescansoPagado.ToString(),
                    EscapeCsv(FormatearMinutos(asistencia.MinutosDescansoNoPagado)),
                    asistencia.MinutosDescansoNoPagado.ToString(),
                    asistencia.MinutosRetardo.ToString(),
                    asistencia.MinutosSalidaAnticipada.ToString(),
                    EscapeCsv(FormatearMinutos(asistencia.MinutosExtra)),
                    asistencia.MinutosExtra.ToString(),
                    EscapeCsv(FormatearMinutos(asistencia.MinutosExtraAutorizadosPago)),
                    asistencia.MinutosExtraAutorizadosPago.ToString(),
                    EscapeCsv(FormatearMinutos(pagoExtra.MinutosDobles)),
                    pagoExtra.MinutosDobles.ToString(),
                    EscapeCsv(FormatearMinutos(pagoExtra.MinutosTriples)),
                    pagoExtra.MinutosTriples.ToString(),
                    EscapeCsv(FormatearMinutos(asistencia.MinutosExtraAutorizadosBanco)),
                    asistencia.MinutosExtraAutorizadosBanco.ToString(),
                    EscapeCsv(FormatearMinutos(asistencia.MinutosCubiertosBancoHoras)),
                    asistencia.MinutosCubiertosBancoHoras.ToString(),
                    EscapeCsv(FormatearMinutos(saldoBancoMin)),
                    saldoBancoMin.ToString(),
                    EscapeCsv(FormatearMinutos(asistencia.MinutosJornadaProgramada)),
                    asistencia.MinutosJornadaProgramada.ToString(),
                    EscapeCsv(FormatearMinutos(asistencia.MinutosJornadaNetaProgramada)),
                    asistencia.MinutosJornadaNetaProgramada.ToString(),
                    asistencia.TotalMarcaciones.ToString(),
                    EscapeCsv(ausenciasPorDiaExportacion.GetValueOrDefault(claveDia, "—")),
                    EscapeCsv(asistencia.ResumenDescansos),
                    EscapeCsv(ObtenerResumenResolucion(asistencia, ausenciasPorDiaExportacion.GetValueOrDefault(claveDia, "—"))),
                    EscapeCsv(asistencia.Observaciones),
                    EscapeCsv("Primeras 9 horas extra pagadas por semana como dobles; excedente como triple.")));
            }

            var nombreArchivo = $"asistencias-{DateTime.Now:yyyyMMdd-HHmmss}.csv";
            await JS.InvokeVoidAsync("mundoVsAuth.downloadCsv", nombreArchivo, builder.ToString());
            ok = $"CSV generado correctamente con {asistencias.Count} registro(s).";
        }
        catch (Exception ex)
        {
            error = ex.InnerException?.Message ?? ex.Message;
        }
        finally
        {
            exportandoCsv = false;
        }
    }

    private static Dictionary<Guid, PagoTiempoExtraExportacion> CalcularMinutosPagoDobleyTriple(IEnumerable<RrhhAsistencia> asistencias)
    {
        const int minutosDoblesPorSemana = 9 * 60;
        var resultado = new Dictionary<Guid, PagoTiempoExtraExportacion>();

        foreach (var grupo in asistencias.GroupBy(a => new { a.EmpleadoId, Semana = ObtenerInicioSemanaExportacion(a.Fecha) }))
        {
            var disponibleDoble = minutosDoblesPorSemana;
            foreach (var asistencia in grupo.OrderBy(a => a.Fecha).ThenBy(a => a.CreatedAt))
            {
                var minutosPago = Math.Max(0, asistencia.MinutosExtraAutorizadosPago);
                var dobles = Math.Min(disponibleDoble, minutosPago);
                var triples = Math.Max(0, minutosPago - dobles);
                disponibleDoble = Math.Max(0, disponibleDoble - dobles);
                resultado[asistencia.Id] = new PagoTiempoExtraExportacion(dobles, triples);
            }
        }

        return resultado;
    }

    private static DateOnly ObtenerInicioSemanaExportacion(DateOnly fecha)
    {
        var desplazamiento = ((int)fecha.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return fecha.AddDays(-desplazamiento);
    }

    private static DescansosExportacion ObtenerDescansosExportacion(IEnumerable<RrhhMarcacion> marcaciones)
    {
        var listaMarcaciones = marcaciones
            .Where(m => !m.EsAnulada)
            .OrderBy(ObtenerFechaHoraLocalMarcacion)
            .ToList();

        var iniciosDescanso = listaMarcaciones
            .Where(m => m.ClasificacionOperativa == TipoClasificacionMarcacionRrhh.InicioDescanso)
            .Select(ObtenerFechaHoraLocalMarcacion)
            .ToList();

        var finesDescanso = listaMarcaciones
            .Where(m => m.ClasificacionOperativa == TipoClasificacionMarcacionRrhh.FinDescanso)
            .Select(ObtenerFechaHoraLocalMarcacion)
            .ToList();

        DateTime? descanso1Inicio = iniciosDescanso.Count > 0 ? iniciosDescanso[0] : null;
        DateTime? descanso1Fin = finesDescanso.Count > 0 ? finesDescanso[0] : null;
        DateTime? descanso2Inicio = iniciosDescanso.Count > 1 ? iniciosDescanso[1] : null;
        DateTime? descanso2Fin = finesDescanso.Count > 1 ? finesDescanso[1] : null;

        return new DescansosExportacion(
            FormatearMarcacionLocal(descanso1Inicio),
            FormatearMarcacionLocal(descanso1Fin),
            FormatearMarcacionLocal(descanso2Inicio),
            FormatearMarcacionLocal(descanso2Fin),
            CalcularMinutosDescanso(descanso1Inicio, descanso1Fin),
            CalcularMinutosDescanso(descanso2Inicio, descanso2Fin));
    }

    private static int CalcularMinutosDescanso(DateTime? inicioUtc, DateTime? finUtc)
    {
        if (!inicioUtc.HasValue || !finUtc.HasValue || finUtc.Value < inicioUtc.Value)
        {
            return 0;
        }

        return (int)Math.Round((finUtc.Value - inicioUtc.Value).TotalMinutes, MidpointRounding.AwayFromZero);
    }

    private static string FormatearMarcacionLocal(DateTime? fechaHoraUtc)
        => fechaHoraUtc.HasValue ? fechaHoraUtc.Value.ToString("HH:mm") : string.Empty;

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

    private static DateTime ConvertirLocalMarcacionDesdeUtc(DateTime fechaHoraUtc, string? zonaHoraria)
    {
        var utc = fechaHoraUtc.Kind == DateTimeKind.Utc ? fechaHoraUtc : DateTime.SpecifyKind(fechaHoraUtc, DateTimeKind.Utc);
        return DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(utc, ResolverZonaHoraria(zonaHoraria)), DateTimeKind.Unspecified);
    }

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

    private static string FormatearHoraTurnoCsv(TimeSpan? hora)
        => hora?.ToString("hh\\:mm") ?? string.Empty;

    private static string EscapeCsv(string? valor)
    {
        var texto = valor ?? string.Empty;
        return $"\"{texto.Replace("\"", "\"\"")}\"";
    }

    private string ObtenerResumenResolucion(RrhhAsistencia asistencia)
        => ObtenerResumenResolucion(asistencia, ObtenerResumenAusencia(asistencia));

    private static string ObtenerResumenResolucion(RrhhAsistencia asistencia, string? resumenAusencia)
        => RrhhTiempoExtraPolicy.ObtenerResumenResolucionOperativa(asistencia, resumenAusencia);

    private sealed record PresentacionRevision(string Titulo, string Tooltip, string BadgeClass, string Icono, bool Resuelta);

    private sealed record PresentacionDescansos(string Titulo, string Tooltip, string ResumenMinutos, string BadgeClass, string CardClass, string Icono);

    private sealed record PresentacionResolucion(string Titulo, string ResumenCorto, string Tooltip, string BadgeClass, string Icono);

    private sealed record PresentacionDato(string Titulo, string Tooltip, string BadgeClass, string Icono);

    private sealed record PagoTiempoExtraExportacion(int MinutosDobles, int MinutosTriples)
    {
        public static readonly PagoTiempoExtraExportacion Vacio = new(0, 0);
    }

    private sealed record DescansosExportacion(
        string Descanso1Entrada,
        string Descanso1Salida,
        string Descanso2Entrada,
        string Descanso2Salida,
        int MinutosDescanso1,
        int MinutosDescanso2);
}
