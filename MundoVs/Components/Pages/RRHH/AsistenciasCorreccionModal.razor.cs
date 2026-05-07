using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Core.Models;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Components.Pages.RRHH;

public partial class AsistenciasCorreccionModal : ComponentBase
{
    [Inject] private IDbContextFactory<CrmDbContext> DbFactory { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private IRrhhAsistenciaProcessor RrhhAsistenciaProcessor { get; set; } = default!;
    [Inject] private IRrhhAsistenciaCorreccionAdvisor CorreccionAdvisor { get; set; } = default!;
    [Inject] private IRrhhTiempoExtraResolutionService TiempoExtraResolutionService { get; set; } = default!;

    [Parameter] public bool Visible { get; set; }
    [Parameter] public RrhhAsistencia? Asistencia { get; set; }
    [Parameter] public IReadOnlyList<TurnoBase> Turnos { get; set; } = [];
    [Parameter] public bool PuedeReprocesar { get; set; }
    [Parameter] public bool PuedeAprobarTiempoExtra { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnUpdated { get; set; }

    private sealed record ResumenVisualBarra(
        string Clave,
        string Titulo,
        string Icono,
        string CssClass,
        int Minutos,
        string Estado,
        string? Aprobacion);

    private sealed record ResumenCalculoItem(string Etiqueta, string Valor, string? Nota = null, string? CssClass = null);

    private sealed record TimelineSegmentoDia(
        string Titulo,
        string Rango,
        int Minutos,
        decimal WidthPercent,
        string CssClass,
        string Detalle,
        int? NumeroDescanso = null,
        int? MinutosProgramados = null,
        int? MinutosAplicados = null,
        string? OrigenAplicado = null,
        bool EsReferenciaTurno = false,
        Guid? MarcacionInicioId = null,
        Guid? MarcacionFinId = null,
        TipoClasificacionMarcacionRrhh? ClasificacionInicioSugerida = null,
        TipoClasificacionMarcacionRrhh? ClasificacionFinSugerida = null,
        string Accion = "trabajo",
        EstadoSegmentoResolucionRrhh EstadoResolucion = EstadoSegmentoResolucionRrhh.RequiereRevision,
        bool FueInferidoAutomaticamente = false);

    private sealed record SegmentoAccionOpcion(string Clave, string Etiqueta, string Ayuda);

    private sealed record ResumenLateralItem(string Etiqueta, string Valor, string CssClass);

    private delegate void AplicarCambiosSegmentoDelegate(RrhhMarcacion inicio, RrhhMarcacion fin, string usuarioActual, CrmDbContext db);

    private Guid _empresaId;
    private Guid? _ultimaAsistenciaCargadaId;
    private bool cargando;
    private bool guardandoPermisoDia;
    private string? error;
    private string? ok;
    private RrhhAsistencia? AsistenciaActual;
    private List<RrhhMarcacion> marcacionesDia = [];
    private List<RrhhSegmentoResolucion> resolucionesSegmentoDia = [];
    private string turnoDiaSeleccionadoIdTexto = string.Empty;
    private string? turnoDiaObservaciones;
    private string manualHoraTexto = string.Empty;
    private TipoClasificacionMarcacionRrhh manualClasificacion = TipoClasificacionMarcacionRrhh.Entrada;
    private string? manualObservacion;
    private Guid? marcacionManualEditandoId;
    private string marcacionManualEditandoHoraTexto = string.Empty;
    private string? marcacionManualEditandoObservacion;
    private List<RrhhLogChecador> bitacoraCorreccionDia = [];
    private string tipoResolucionTiempoExtra = "PagarTodo";
    private string? resolucionTiempoExtraObservaciones;
    private int saldoBancoHorasSeleccionado;
    private int topeBancoHorasConfigurado = 40 * 60;
    private decimal factorTiempoExtraConfigurado = 2m;
    private bool bancoHorasHabilitadoConfigurado;
    private decimal factorAcumulacionBancoHorasConfigurado = 1m;
    private int minutosMinimosTiempoExtraConfigurado = 30;
    private int minutosCompensacionPermisoCaptura;
    private int minutosExtraPagoCaptura;
    private int minutosExtraBancoCaptura;
    private int minutosCubrirBancoCaptura;
    private string minutosExtraPagoTexto = "0:00";
    private string minutosExtraBancoTexto = "0:00";
    private bool usarFactorTiempoExtraOverride;
    private decimal factorTiempoExtraOverrideCaptura;
    private int minutosCompensadosPermisoAprobados;
    private int minutosRecuperablesPermisoAprobables;
    private RrhhAusencia? permisoDiaSeleccionado;
    private decimal horasPermisoDiaCaptura;
    private bool permisoDiaConGoceCaptura = true;
    private string? permisoDiaMotivo;
    private string? permisoDiaObservaciones;
    private string? resumenAusenciaActual;
    private RrhhAsistenciaCorreccionAdvice? asesorCorreccionActual;
    private bool mostrarAyudaReglas;
    private Guid? segmentoEditandoInicioId;
    private Guid? segmentoEditandoFinId;
    private string segmentoAccionSeleccionada = "trabajo";
    private int? segmentoMinutosAplicadosCaptura;
    private Guid? segmentoGuardandoInicioId;
    private Guid? segmentoGuardandoFinId;
    private CrmDbContext? _draftDb;
    private bool _tieneCambiosPendientes;
    private bool _mostrarConfirmacionCierre;
    private bool _mostrarAccionesRapidasTiempo;
    private bool _mostrarAccionesRapidasPermiso;
    private bool _mostrarAccionesRapidasTurno;
    private bool _mostrarBitacora;
    private bool _mostrarMarcacionesDia;
    private int _toleranciaExcesoDescansoMinutos = RrhhAsistenciaDescansoSettings.ToleranciaExcesoDescansoDefault;

    protected override async Task OnParametersSetAsync()
    {
        if (!Visible || Asistencia == null)
        {
            await DisposeDraftContextAsync();
            _ultimaAsistenciaCargadaId = null;
            AsistenciaActual = null;
            asesorCorreccionActual = null;
            return;
        }

        if (_ultimaAsistenciaCargadaId == Asistencia.Id && AsistenciaActual != null)
        {
            return;
        }

        await InicializarAsync();
    }

    private async Task InicializarAsync()
    {
        error = null;
        ok = null;
        _tieneCambiosPendientes = false;
        _mostrarConfirmacionCierre = false;
        await CargarEmpresaAsync();

        if (Asistencia == null || _empresaId == Guid.Empty)
        {
            return;
        }

        await DisposeDraftContextAsync();
        _draftDb = await DbFactory.CreateDbContextAsync();
        AsistenciaActual = await _draftDb.RrhhAsistencias
            .Include(a => a.Empleado)
            .Include(a => a.TurnoBase)
            .FirstOrDefaultAsync(a => a.Id == Asistencia.Id)
            ?? Asistencia;

        _ultimaAsistenciaCargadaId = AsistenciaActual.Id;
        ReiniciarEstadoVisual();
        tipoResolucionTiempoExtra = "PagarTodo";
        resolucionTiempoExtraObservaciones = null;
        await CargarContextoTiempoExtraAsync(_draftDb);
        _toleranciaExcesoDescansoMinutos = (await RrhhAsistenciaDescansoSettings.LoadAsync(_draftDb, _empresaId)).ToleranciaExcesoDescansoMinutos;
        CargarCapturaResolucion(AsistenciaActual);
        await CargarContextoDiaAsync(_draftDb, AsistenciaActual.Fecha, recargarAsistencia: false);
    }

    private async Task DisposeDraftContextAsync()
    {
        if (_draftDb == null)
        {
            return;
        }

        await _draftDb.DisposeAsync();
        _draftDb = null;
    }

    private void RegistrarCambioPendiente(string mensaje)
    {
        _tieneCambiosPendientes = true;
        ok = mensaje;
        ActualizarAsesorCorreccion();
        StateHasChanged();
    }

    private void ReiniciarEstadoVisual()
    {
        turnoDiaSeleccionadoIdTexto = AsistenciaActual?.TurnoBaseId?.ToString() ?? string.Empty;
        turnoDiaObservaciones = null;
        manualHoraTexto = string.Empty;
        manualClasificacion = TipoClasificacionMarcacionRrhh.Entrada;
        manualObservacion = null;
        ReiniciarEdicionManual();
        CancelarEdicionSegmento();
        _mostrarAccionesRapidasTiempo = false;
        _mostrarAccionesRapidasPermiso = false;
        _mostrarAccionesRapidasTurno = false;
        _mostrarBitacora = false;
    }

    private void ReiniciarEdicionManual()
    {
        marcacionManualEditandoId = null;
        marcacionManualEditandoHoraTexto = string.Empty;
        marcacionManualEditandoObservacion = null;
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
    }

    private async Task CargarEmpresaAsync()
    {
        if (_empresaId != Guid.Empty)
        {
            return;
        }

        var state = await AuthStateProvider.GetAuthenticationStateAsync();
        _ = Guid.TryParse(state.User.FindFirst("EmpresaId")?.Value, out _empresaId);
    }

    private async Task CargarMarcacionesDiaAsync()
    {
        if (AsistenciaActual == null)
        {
            LimpiarContextoDia();
            return;
        }

        if (_draftDb == null)
        {
            return;
        }

        await CargarContextoDiaAsync(_draftDb, AsistenciaActual.Fecha, recargarAsistencia: false);
    }

    private void LimpiarContextoDia()
    {
        marcacionesDia = [];
        resolucionesSegmentoDia = [];
        bitacoraCorreccionDia = [];
        permisoDiaSeleccionado = null;
        resumenAusenciaActual = null;
        ActualizarAsesorCorreccion();
    }

    private async Task CargarContextoDiaAsync(CrmDbContext db, DateOnly fecha, bool recargarAsistencia)
    {
        if (recargarAsistencia)
        {
            await RecargarAsistenciaActualAsync(db, fecha);
        }

        if (AsistenciaActual == null)
        {
            LimpiarContextoDia();
            return;
        }

        var (desdeUtc, hastaUtc) = ObtenerVentanaConsultaUtc(fecha);

        var marcacionesCandidatas = await db.RrhhMarcaciones
            .AsNoTracking()
            .Include(m => m.Checador)
            .Where(m => m.EmpresaId == _empresaId
                && m.EmpleadoId == AsistenciaActual.EmpleadoId
                && m.FechaHoraMarcacionUtc >= desdeUtc
                && m.FechaHoraMarcacionUtc < hastaUtc)
            .OrderBy(m => m.FechaHoraMarcacionUtc)
            .ToListAsync();

        marcacionesDia = marcacionesCandidatas
            .Where(m => DateOnly.FromDateTime(ObtenerFechaHoraLocalMarcacion(m)) == AsistenciaActual.Fecha)
            .OrderBy(ObtenerFechaHoraLocalMarcacion)
            .ToList();

        resolucionesSegmentoDia = await db.RrhhSegmentosResoluciones
            .AsNoTracking()
            .Where(r => r.EmpresaId == _empresaId
                && r.EmpleadoId == AsistenciaActual.EmpleadoId
                && r.Fecha == AsistenciaActual.Fecha
                && r.IsActive)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();

        bitacoraCorreccionDia = await db.RrhhLogsChecador
            .AsNoTracking()
            .Where(l => l.EmpresaId == _empresaId
                && l.Mensaje.Contains("corrección de asistencia")
                && l.Detalle != null
                && l.Detalle.Contains($"empleado={AsistenciaActual.EmpleadoId}")
                && l.Detalle.Contains($"fecha={AsistenciaActual.Fecha:yyyy-MM-dd}"))
            .OrderByDescending(l => l.FechaUtc)
            .Take(10)
            .ToListAsync();

        minutosCompensadosPermisoAprobados = RrhhTiempoExtraPolicy.ObtenerMinutosPermisoCompensadosAprobados(bitacoraCorreccionDia, AsistenciaActual.EmpleadoId, AsistenciaActual.Fecha);
        minutosRecuperablesPermisoAprobables = RrhhPermisoCompensationPolicy.ObtenerMinutosRecuperablesAprobables(AsistenciaActual, minutosMinimosTiempoExtraConfigurado);
        var minutosCompensacionSugeridos = ObtenerMinutosCompensacionPermisoSugeridosAprobacion();
        minutosCompensacionPermisoCaptura = minutosCompensadosPermisoAprobados > 0
            ? minutosCompensadosPermisoAprobados
            : minutosCompensacionSugeridos;

        var ausenciasDia = await db.RrhhAusencias
            .AsNoTracking()
            .Where(a => a.EmpresaId == _empresaId
                && a.EmpleadoId == AsistenciaActual.EmpleadoId
                && a.IsActive
                && (a.Estatus == EstatusAusenciaRrhh.Aprobada || a.Estatus == EstatusAusenciaRrhh.Aplicada)
                && a.FechaInicio <= AsistenciaActual.Fecha
                && a.FechaFin >= AsistenciaActual.Fecha)
            .OrderByDescending(a => a.FechaAprobacion ?? a.UpdatedAt ?? a.CreatedAt)
            .ToListAsync();

        permisoDiaSeleccionado = ausenciasDia
            .Where(a => a.Tipo == TipoAusenciaRrhh.Permiso)
            .FirstOrDefault();

        resumenAusenciaActual = ausenciasDia.Count == 0
            ? null
            : string.Join(" | ", ausenciasDia.Select(FormatearAusencia).Distinct());

        if (permisoDiaSeleccionado == null)
        {
            AplicarCapturaPermisoSugerida();
        }
        else
        {
            AplicarCapturaPermisoExistente();
        }

        ActualizarAsesorCorreccion();
    }

    private static (DateTime DesdeUtc, DateTime HastaUtc) ObtenerVentanaConsultaUtc(DateOnly fecha)
    {
        DateTime fechaBase = fecha.ToDateTime(TimeOnly.MinValue);
        var desdeUtc = DateTime.SpecifyKind(fechaBase.AddHours(-14), DateTimeKind.Utc);
        var hastaUtc = DateTime.SpecifyKind(fechaBase.AddDays(1).AddHours(14), DateTimeKind.Utc);
        return (desdeUtc, hastaUtc);
    }

    private void AplicarCapturaPermisoSugerida()
    {
        horasPermisoDiaCaptura = AsistenciaActual == null
            ? 0
            : decimal.Round(ObtenerMinutosPermisoSugeridos(AsistenciaActual) / 60m, 2, MidpointRounding.AwayFromZero);
        permisoDiaConGoceCaptura = true;
        permisoDiaMotivo = null;
        permisoDiaObservaciones = null;
    }

    private void AplicarCapturaPermisoExistente()
    {
        if (permisoDiaSeleccionado == null)
        {
            return;
        }

        horasPermisoDiaCaptura = permisoDiaSeleccionado.Horas;
        permisoDiaConGoceCaptura = permisoDiaSeleccionado.ConGocePago;
        permisoDiaMotivo = permisoDiaSeleccionado.Motivo;
        permisoDiaObservaciones = permisoDiaSeleccionado.Observaciones;
    }

    private async Task CerrarAsync()
    {
        if (_tieneCambiosPendientes)
        {
            _mostrarConfirmacionCierre = true;
            return;
        }

        await CerrarSinGuardarAsync();
    }

    private async Task CerrarSinGuardarAsync()
    {
        _ultimaAsistenciaCargadaId = null;
        AsistenciaActual = null;
        error = null;
        ok = null;
        mostrarAyudaReglas = false;
        _mostrarConfirmacionCierre = false;
        _tieneCambiosPendientes = false;
        await DisposeDraftContextAsync();
        if (OnClose.HasDelegate)
        {
            await OnClose.InvokeAsync();
        }
    }

    private async Task GuardarYCerrarAsync()
    {
        if (_draftDb == null || AsistenciaActual == null)
        {
            await CerrarSinGuardarAsync();
            return;
        }

        error = null;
        ok = null;

        try
        {
            await ReprocesarYRefrescarDiaAsync(_draftDb, AsistenciaActual.Fecha, recargarResolucion: true);
            await _draftDb.SaveChangesAsync();
            _tieneCambiosPendientes = false;
            _mostrarConfirmacionCierre = false;
            await NotificarActualizacionAsync();
            await CerrarSinGuardarAsync();
        }
        catch (Exception ex)
        {
            error = ex.InnerException?.Message ?? ex.Message;
        }
    }

    private async Task QuitarResolucionTiempoAsync()
    {
        if (AsistenciaActual == null || !PuedeAprobarTiempoExtra)
        {
            return;
        }

        error = null;
        ok = null;

        var resolucionAnterior = AsistenciaActual.ResolucionTiempoExtra;
        tipoResolucionTiempoExtra = string.IsNullOrWhiteSpace(resolucionAnterior) ? "SinAccion" : resolucionAnterior;
        minutosExtraPagoCaptura = 0;
        minutosExtraBancoCaptura = 0;
        minutosCubrirBancoCaptura = 0;
        minutosExtraPagoTexto = "0:00";
        minutosExtraBancoTexto = "0:00";
        usarFactorTiempoExtraOverride = false;
        factorTiempoExtraOverrideCaptura = factorTiempoExtraConfigurado;
        resolucionTiempoExtraObservaciones = "Resolución revertida desde el wizard de asistencias.";

        await AplicarResolucionTiempoAsync();

        if (string.IsNullOrWhiteSpace(error))
        {
            ok = "Se quitó la resolución de tiempo extra / banco de horas del día.";
        }
    }

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
                ObtenerMinutosTrabajadosBaseVisibles(AsistenciaActual),
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
                ObtenerMinutosFaltanteBanco(AsistenciaActual),
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
            ObtenerMinutosTrabajadosVisibles(AsistenciaActual),
            ObtenerMinutosTrabajadosBaseVisibles(AsistenciaActual),
            AsistenciaActual.MinutosExtra,
            ObtenerMinutosPermisoSugeridos(AsistenciaActual),
            ObtenerMinutosPermisoCapturados(),
            minutosCompensadosPermisoAprobados,
            ObtenerMinutosCompensacionPermisoSugeridosAprobacion(),
            ObtenerMinutosFaltanteBanco(AsistenciaActual),
            1
        }.Max();

        var porcentaje = decimal.Round(minutos * 100m / referencia, 2, MidpointRounding.AwayFromZero);
        return Math.Clamp(porcentaje, 8m, 100m);
    }

    private void PrepararPermisoSugeridoResumen()
        => AplicarPermisoDiaSugerido();

    private async Task AprobarCompensacionSugeridaResumenAsync()
    {
        minutosCompensacionPermisoCaptura = TieneCompensacionPermisoAprobada()
            ? minutosCompensadosPermisoAprobados
            : ObtenerMinutosCompensacionPermisoSugeridosAprobacion();

        await AprobarCompensacionPermisoAsync();
    }

    private void AbrirTiempoDesdeResumen()
        => AlternarAccionesRapidasTiempo();

    private void AbrirMarcacionesDesdeResumen()
    {
    }

    private void OnResumenTipoResolucionChanged(ChangeEventArgs args)
        => CambiarTipoResolucionTiempoExtra(args.Value?.ToString() ?? "SinAccion");

    private int ObtenerMaximoMinutosResolublesResumen()
        => AsistenciaActual == null ? 0 : ObtenerMinutosResolubles(AsistenciaActual);

    private int ObtenerMaximoMinutosCubrirBancoResumen()
        => AsistenciaActual == null ? 0 : ObtenerMinutosFaltanteBanco(AsistenciaActual);

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
        var faltante = ObtenerMinutosFaltanteBanco(AsistenciaActual);

        return tipoResolucionTiempoExtra switch
        {
            "PagarTodo" => $"Puedes aprobar hasta {FormatearMinutos(resoluble)} para pago.",
            "BancoTodo" => $"Puedes aprobar hasta {FormatearMinutos(resoluble)} para banco.",
            "MitadMitad" => $"La suma de pago y banco no debe exceder {FormatearMinutos(resoluble)}.",
            "CubrirFaltanteConBanco" => $"Puedes cubrir hasta {FormatearMinutos(faltante)} con banco.",
            _ => "Selecciona una resolución para capturar el aprobado."
        };
    }

    private string ObtenerResumenPermisoRapido()
    {
        if (AsistenciaActual == null)
        {
            return "Sin datos para permiso.";
        }

        var sugerido = ObtenerMinutosPermisoSugeridos(AsistenciaActual);
        var capturado = ObtenerMinutosPermisoCapturados();
        var remanente = ObtenerMinutosFaltanteRemanenteActual();

        if (permisoDiaSeleccionado != null)
        {
            return $"Permiso actual: {permisoDiaSeleccionado.Horas:0.##} h {(permisoDiaSeleccionado.ConGocePago ? "con goce" : "sin goce")}. Remanente actual: {FormatearMinutos(remanente)}.";
        }

        if (sugerido <= 0)
        {
            return "Hoy no se sugiere permiso adicional, salvo que quieras registrar una justificación operativa del día.";
        }

        return capturado > 0
            ? $"Sugerido: {FormatearMinutos(sugerido)}. Con tu captura actual vas en {FormatearMinutos(capturado)} y quedarían {FormatearMinutos(remanente)} por cubrir."
            : $"Sugerido neto: {FormatearMinutos(sugerido)}. Puedes ajustar horas y guardar aquí mismo.";
    }

    private string ObtenerHoraMarcacion(RrhhMarcacion marcacion)
        => ObtenerFechaHoraLocalMarcacion(marcacion).ToString("HH:mm");

    private int ObtenerMinutosTrabajadosBaseVisibles(RrhhAsistencia asistencia)
        => RrhhTiempoExtraPolicy.ObtenerMinutosTrabajadosBaseVisibles(asistencia);

    private int ObtenerMinutosExtraAprobados(RrhhAsistencia asistencia)
        => RrhhTiempoExtraPolicy.ObtenerMinutosExtraAprobados(asistencia);

    private int ObtenerMinutosCompensadosAprobadosActual()
        => Math.Max(0, minutosCompensadosPermisoAprobados);

    private bool TieneResolucionTiempoActual()
        => AsistenciaActual != null && RrhhTiempoExtraPolicy.TieneResolucionTiempoAplicada(AsistenciaActual);

    private string ObtenerAuditoriaMarcacionPresentacion(RrhhMarcacion marcacion)
        => ObtenerAuditoriaMarcacion(marcacion);

    private async Task CambiarClasificacionMarcacionDesdeUiAsync((Guid MarcacionId, TipoClasificacionMarcacionRrhh Clasificacion) cambio)
        => await CambiarClasificacionMarcacionAsync(cambio.MarcacionId, cambio.Clasificacion);

    private void CambiarTipoResolucionTiempoExtra(string tipo)
    {
        tipoResolucionTiempoExtra = tipo;
        AjustarResolucionTiempoSugerida();
        minutosExtraPagoTexto = FormatearMinutosCaptura(minutosExtraPagoCaptura);
        minutosExtraBancoTexto = FormatearMinutosCaptura(minutosExtraBancoCaptura);
    }

    private static string FormatearMinutosCaptura(int minutos)
    {
        var horas = Math.Max(0, minutos) / 60;
        var restantes = Math.Max(0, minutos) % 60;
        return $"{horas}:{restantes:00}";
    }

    private static int ParsearMinutosCaptura(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return 0;
        }

        if (TimeSpan.TryParse(texto, out var hora))
        {
            return Math.Max(0, (int)Math.Round(hora.TotalMinutes, MidpointRounding.AwayFromZero));
        }

        return 0;
    }

    private int ObtenerMaximoMinutosBaseDisponibles()
        => AsistenciaActual == null ? 0 : Math.Max(0, AsistenciaActual.MinutosExtra);

    private string ObtenerResumenAprobacionBaseTiempo()
    {
        if (AsistenciaActual == null)
        {
            return "Sin datos de tiempo extra.";
        }

        var disponibles = ObtenerMaximoMinutosBaseDisponibles();
        var capturados = Math.Max(0, minutosExtraPagoCaptura) + Math.Max(0, minutosExtraBancoCaptura);
        return $"Disponible base: {FormatearMinutos(disponibles)} · Capturado: {FormatearMinutos(capturados)}";
    }

    private string ObtenerResumenDestinoTiempoActual()
    {
        if (AsistenciaActual == null)
        {
            return "Sin resolución de tiempo.";
        }

        var pago = Math.Max(0, AsistenciaActual.MinutosExtraAutorizadosPago);
        var banco = Math.Max(0, AsistenciaActual.MinutosExtraAutorizadosBanco);
        var cubiertoBanco = Math.Max(0, AsistenciaActual.MinutosCubiertosBancoHoras);

        if (cubiertoBanco > 0)
        {
            return $"Cubierto con banco: {FormatearMinutos(cubiertoBanco)}.";
        }

        if (pago > 0 && banco > 0)
        {
            return $"Resolución mixta · Pago: {FormatearMinutos(pago)} · Banco: {FormatearMinutos(banco)}.";
        }

        if (pago > 0)
        {
            return $"Enviado a pago: {FormatearMinutos(pago)}.";
        }

        if (banco > 0)
        {
            return $"Enviado a banco: {FormatearMinutos(banco)}.";
        }

        return string.IsNullOrWhiteSpace(AsistenciaActual.ResolucionTiempoExtra)
            ? "Sin resolución aplicada todavía."
            : $"Resolución actual: {AsistenciaActual.ResolucionTiempoExtra}.";
    }

    private string ObtenerResumenSaldoBancoHoras()
        => $"Saldo banco de horas: {FormatearMinutos(Math.Max(0, saldoBancoHorasSeleccionado))} de {FormatearMinutos(Math.Max(0, topeBancoHorasConfigurado))}.";

    private string ObtenerResumenFactorTiempoExtra()
    {
        var factorActivo = usarFactorTiempoExtraOverride && factorTiempoExtraOverrideCaptura > 0m
            ? factorTiempoExtraOverrideCaptura
            : factorTiempoExtraConfigurado;

        return usarFactorTiempoExtraOverride && factorTiempoExtraOverrideCaptura > 0m
            ? $"Factor manual activo: x{factorActivo:0.##}"
            : $"Factor configurado: x{factorActivo:0.##}";
    }

    private static string FormatearMensajeBitacora(RrhhLogChecador log)
        => log.Mensaje.Replace("Se aplicó corrección de asistencia: ", string.Empty);

    private static string FormatearFechaBitacora(RrhhLogChecador log)
        => $"{log.FechaUtc.ToLocalTime():dd/MM/yyyy HH:mm} · {ObtenerUsuarioBitacora(log)}";

    private string ObtenerDecisionRapidaWizard()
        => asesorCorreccionActual == null
            ? "Sin diagnóstico actual."
            : $"Ahora mismo te conviene: {ObtenerDecisionRapidaLegible()}.";

    private string ObtenerResumenVisibleExplicado()
        => AsistenciaActual == null
            ? string.Empty
            : $"Base {FormatearMinutos(ObtenerMinutosTrabajadosBaseVisibles(AsistenciaActual))} + compensación día {FormatearMinutos(ObtenerMinutosCompensadosAprobadosActual())} + extra aprobada {FormatearMinutos(ObtenerMinutosExtraAprobados(AsistenciaActual))}.";

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
                FormatearMinutos(ObtenerMinutosTrabajadosVisibles(AsistenciaActual)),
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

    private IReadOnlyList<TimelineSegmentoDia> ObtenerTimelineSegmentosDia()
    {
        if (AsistenciaActual == null || marcacionesDia.Count == 0)
        {
            return [];
        }

        var segmentos = new List<TimelineSegmentoDia>();
        var detalleTurno = ObtenerDetalleTurnoSeleccionadoDia();
        var marcacionesOrdenadas = marcacionesDia
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

            var clasificacion = ClasificarSegmentoDia(inicioMarcacion, finMarcacion, detalleTurno, inicio, fin, minutos);
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

    private (string Titulo, string CssClass, string Detalle, TipoClasificacionMarcacionRrhh ClasificacionInicio, TipoClasificacionMarcacionRrhh ClasificacionFin, string Accion, EstadoSegmentoResolucionRrhh EstadoResolucion, bool FueInferidoAutomaticamente) ClasificarSegmentoDia(RrhhMarcacion inicio, RrhhMarcacion fin, TurnoBaseDetalle? detalleTurno, DateTime inicioLocal, DateTime finLocal, int minutos)
    {
        var resolucion = ObtenerResolucionSegmento(inicio.Id, fin.Id);
        var accionPayload = resolucion == null
            ? RrhhMarcacionSegmentActionHelper.ResolveAction(inicio.PayloadRaw, fin.PayloadRaw)
            : MapearAccionSegmento(resolucion.TipoSegmento);
        var estadoResolucion = resolucion?.Estado ?? EstadoSegmentoResolucionRrhh.RequiereRevision;
        var fueInferidoAutomaticamente = resolucion?.FueInferidoAutomaticamente ?? false;

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

        if (resolucion?.TipoSegmento == TipoSegmentoResolucionRrhh.Extra)
        {
            return ("Bloque extra", "asis-dayline__segment--extra", "Tramo fijado manualmente como tiempo adicional al turno; no debe volver a interpretarse como descanso.", TipoClasificacionMarcacionRrhh.Entrada, TipoClasificacionMarcacionRrhh.Salida, "extra", estadoResolucion, fueInferidoAutomaticamente);
        }

        if (resolucion?.TipoSegmento == TipoSegmentoResolucionRrhh.Trabajo)
        {
            return ("Trabajo principal", "asis-dayline__segment--trabajo", "Tramo fijado manualmente como parte de la jornada principal; no debe volver a interpretarse como descanso.", TipoClasificacionMarcacionRrhh.Entrada, TipoClasificacionMarcacionRrhh.Salida, "trabajo", estadoResolucion, fueInferidoAutomaticamente);
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

    private (int? NumeroDescanso, int? MinutosProgramados, int? MinutosAplicados, string? OrigenAplicado) ObtenerPresentacionDescanso(TurnoBaseDetalle? detalleTurno, Guid inicioId, Guid finId, TimeSpan inicio, TimeSpan fin, int minutosDetectados)
    {
        var resolucion = ObtenerResolucionSegmento(inicioId, finId);
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
        if (segmento.Accion != "descanso" || !segmento.MinutosAplicados.HasValue)
        {
            return string.Empty;
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

        var candidatos = new List<(int Numero, TimeSpan Inicio, TimeSpan Fin)>();
        if (detalleTurno.CantidadDescansos >= 1 && detalleTurno.Descanso1Inicio.HasValue && detalleTurno.Descanso1Fin.HasValue)
        {
            candidatos.Add((1, detalleTurno.Descanso1Inicio.Value, detalleTurno.Descanso1Fin.Value));
        }

        if (detalleTurno.CantidadDescansos >= 2 && detalleTurno.Descanso2Inicio.HasValue && detalleTurno.Descanso2Fin.HasValue)
        {
            candidatos.Add((2, detalleTurno.Descanso2Inicio.Value, detalleTurno.Descanso2Fin.Value));
        }

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

        return numeroDescanso switch
        {
            1 when detalleTurno.Descanso1Inicio.HasValue && detalleTurno.Descanso1Fin.HasValue
                => Math.Max(0, (int)Math.Round((detalleTurno.Descanso1Fin.Value - detalleTurno.Descanso1Inicio.Value).TotalMinutes)),
            2 when detalleTurno.Descanso2Inicio.HasValue && detalleTurno.Descanso2Fin.HasValue
                => Math.Max(0, (int)Math.Round((detalleTurno.Descanso2Fin.Value - detalleTurno.Descanso2Inicio.Value).TotalMinutes)),
            _ => null
        };
    }

    private RrhhSegmentoResolucion? ObtenerResolucionSegmento(Guid inicioId, Guid finId)
        => resolucionesSegmentoDia.FirstOrDefault(r => r.MarcacionInicioId == inicioId && r.MarcacionFinId == finId && r.Estado != EstadoSegmentoResolucionRrhh.Obsoleta);

    private static string MapearAccionSegmento(TipoSegmentoResolucionRrhh tipoSegmento)
        => tipoSegmento switch
        {
            TipoSegmentoResolucionRrhh.Extra => "extra",
            TipoSegmentoResolucionRrhh.Descanso => "descanso",
            TipoSegmentoResolucionRrhh.SalidaTemporal => "temporal",
            TipoSegmentoResolucionRrhh.Permiso => "permiso",
            TipoSegmentoResolucionRrhh.NoConsiderar => "ignorar",
            _ => "trabajo"
        };

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

    private int ObtenerDuracionBloquePrincipal()
    {
        var segmentos = ObtenerTimelineSegmentosDia()
            .Where(s => !s.EsReferenciaTurno)
            .OrderByDescending(s => s.Minutos)
            .ToList();

        return segmentos.Count == 0 ? 0 : segmentos[0].Minutos;
    }

    private static TipoSegmentoResolucionRrhh MapearTipoSegmentoResolucion(string accion)
        => accion switch
        {
            "extra" => TipoSegmentoResolucionRrhh.Extra,
            "descanso" => TipoSegmentoResolucionRrhh.Descanso,
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
            new("temporal", "Salida temporal", "Se conserva fuera del tiempo laborado y no se trata como descanso; debe verse explícitamente como salida temporal."),
            new("permiso", "Permiso", "Marca el tramo como permiso del día para diferenciarlo de una salida temporal o de un descanso."),
            new("ignorar", "No considerar", "Saca el tramo del cálculo del día sin romper el timeline; seguirá visible como tramo no considerado.")
        ];
    }

    private string ObtenerAyudaAccionSegmentoSeleccionada(TimelineSegmentoDia segmento)
        => ObtenerOpcionesAccionSegmento(segmento).FirstOrDefault(o => o.Clave == segmentoAccionSeleccionada)?.Ayuda
            ?? "Selecciona cómo debe interpretarse este tramo del día.";

    private IReadOnlyList<ResumenLateralItem> ObtenerResumenLateralTimeline()
    {
        if (AsistenciaActual == null)
        {
            return [];
        }

        return
        [
            new("Visible", FormatearMinutos(ObtenerMinutosTrabajadosVisibles(AsistenciaActual)), "asis-summary-side__item--primary"),
            new("Extra", FormatearMinutos(AsistenciaActual.MinutosExtra), "asis-summary-side__item--accent"),
            new("Permiso", permisoDiaSeleccionado == null ? FormatearMinutos(ObtenerMinutosPermisoSugeridos(AsistenciaActual)) : FormatearMinutos(ObtenerMinutosPermisoCapturados()), "asis-summary-side__item--warn"),
            new("Compensación", FormatearMinutos(ObtenerMinutosCompensadosAprobadosActual()), "asis-summary-side__item--info"),
            new("Descanso", FormatearMinutos(AsistenciaActual.MinutosDescansoTomado), "asis-summary-side__item--muted")
        ];
    }

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
            "temporal" => (TipoClasificacionMarcacionRrhh.InicioDescanso, TipoClasificacionMarcacionRrhh.FinDescanso),
            "permiso" => (TipoClasificacionMarcacionRrhh.InicioDescanso, TipoClasificacionMarcacionRrhh.FinDescanso),
            _ => (TipoClasificacionMarcacionRrhh.Entrada, TipoClasificacionMarcacionRrhh.Salida)
        };

        var mensaje = segmentoAccionSeleccionada switch
        {
            "descanso" => "Segmento marcado como descanso y día reprocesado.",
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

                var numeroDescanso = segmentoAccionSeleccionada == "descanso"
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

    private async Task RefrescarDiaAsync(CrmDbContext db, DateOnly fecha, string? mensajeOk = null, bool recargarResolucion = false)
    {
        await CargarContextoDiaAsync(db, fecha, recargarAsistencia: true);
        if (recargarResolucion && AsistenciaActual != null)
        {
            CargarCapturaResolucion(AsistenciaActual);
        }

        if (!string.IsNullOrWhiteSpace(mensajeOk))
        {
            ok = mensajeOk;
        }
    }

    private async Task ReprocesarYRefrescarDiaAsync(CrmDbContext db, DateOnly fecha, string? mensajeOk = null, bool recargarResolucion = false)
    {
        if (AsistenciaActual == null)
        {
            return;
        }

        await ReconciliarResolucionesSegmentoDiaAsync(db, fecha);
        await RrhhAsistenciaProcessor.ReprocesarRangoAsync(db, _empresaId, fecha, fecha, AsistenciaActual.EmpleadoId);
        await RefrescarDiaAsync(db, fecha, mensajeOk, recargarResolucion);
        RegistrarCambioPendiente(string.IsNullOrWhiteSpace(mensajeOk) ? "Cambios pendientes en el día. Guarda al cerrar para aplicarlos definitivamente." : mensajeOk);
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

    private static string ExtraerTextoBloque(string observaciones, int indice)
    {
        var segmento = observaciones[indice..];
        var punto = segmento.IndexOf('.');
        return (punto >= 0 ? segmento[..punto] : segmento).Trim();
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

    private string ObtenerDecisionRapidaLegible()
        => asesorCorreccionActual?.Escenario == "CompensacionPermisoPendiente"
            ? "Revisar si la compensación realmente reduce el permiso o faltante"
            : (asesorCorreccionActual?.AccionPrincipalTexto ?? "Revisar el día");

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

    private void IrATabSugerida()
    {
        if (asesorCorreccionActual == null)
        {
            return;
        }

        if (asesorCorreccionActual.PriorizarTiempo)
        {
            _mostrarAccionesRapidasTiempo = true;
        }
        else if (asesorCorreccionActual.PriorizarPermiso)
        {
            _mostrarAccionesRapidasPermiso = true;
        }

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

    private async Task NotificarActualizacionAsync()
    {
        if (OnUpdated.HasDelegate)
        {
            await OnUpdated.InvokeAsync();
        }
    }

    private async Task RecargarAsistenciaActualAsync(CrmDbContext db, DateOnly fecha)
    {
        if (AsistenciaActual == null)
        {
            return;
        }

        AsistenciaActual = await db.RrhhAsistencias
            .AsNoTracking()
            .Include(a => a.Empleado)
            .Include(a => a.TurnoBase)
            .FirstOrDefaultAsync(a => a.EmpresaId == _empresaId
                && a.EmpleadoId == AsistenciaActual.EmpleadoId
                && a.Fecha == fecha);
    }

    private async Task GuardarPermisoDiaAsync()
    {
        if (AsistenciaActual == null)
        {
            return;
        }

        error = null;
        ok = null;

        if (!PuedeReprocesar)
        {
            error = "No tienes permisos para registrar permisos del día.";
            return;
        }

        if (horasPermisoDiaCaptura <= 0)
        {
            error = "Captura las horas del permiso parcial.";
            return;
        }

        guardandoPermisoDia = true;
        try
        {
            var usuarioActual = await ObtenerUsuarioActualAsync();
            await using var db = await DbFactory.CreateDbContextAsync();
            var fecha = AsistenciaActual.Fecha;
            var permiso = await db.RrhhAusencias.FirstOrDefaultAsync(a => a.EmpresaId == _empresaId
                && a.EmpleadoId == AsistenciaActual.EmpleadoId
                && a.Tipo == TipoAusenciaRrhh.Permiso
                && a.IsActive
                && a.FechaInicio <= fecha
                && a.FechaFin >= fecha);

            if (permiso == null)
            {
                permiso = new RrhhAusencia
                {
                    Id = Guid.NewGuid(),
                    EmpresaId = _empresaId,
                    EmpleadoId = AsistenciaActual.EmpleadoId,
                    Tipo = TipoAusenciaRrhh.Permiso,
                    FechaInicio = fecha,
                    FechaFin = fecha,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = usuarioActual,
                    IsActive = true
                };
                db.RrhhAusencias.Add(permiso);
            }

            permiso.Estatus = EstatusAusenciaRrhh.Aplicada;
            permiso.Dias = 1;
            permiso.Horas = decimal.Round(horasPermisoDiaCaptura, 2, MidpointRounding.AwayFromZero);
            permiso.ConGocePago = permisoDiaConGoceCaptura;
            permiso.Motivo = string.IsNullOrWhiteSpace(permisoDiaMotivo) ? "Permiso parcial desde asistencias." : permisoDiaMotivo.Trim();
            permiso.Observaciones = string.IsNullOrWhiteSpace(permisoDiaObservaciones) ? null : permisoDiaObservaciones.Trim();
            permiso.FechaAprobacion ??= DateTime.UtcNow;
            permiso.AprobadoPor = usuarioActual;
            permiso.UpdatedAt = DateTime.UtcNow;
            permiso.UpdatedBy = usuarioActual;
            permiso.IsActive = true;

            if (permiso.ConGocePago)
            {
                saldoBancoHorasSeleccionado = await TiempoExtraResolutionService.AplicarPermisoConGoceBancoHorasAsync(db, new RrhhPermisoBancoHorasCommand
                {
                    EmpresaId = _empresaId,
                    EmpleadoId = AsistenciaActual.EmpleadoId,
                    AusenciaId = permiso.Id,
                    Fecha = fecha,
                    HorasPermiso = permiso.Horas,
                    Observaciones = $"Permiso parcial con goce desde asistencias. Motivo: {permiso.Motivo}",
                    UsuarioActual = usuarioActual
                });
            }
            else
            {
                saldoBancoHorasSeleccionado = await TiempoExtraResolutionService.RemoverPermisoBancoHorasAsync(db, _empresaId, AsistenciaActual.EmpleadoId, permiso.Id);
            }

            await RegistrarBitacoraCorreccionAsync(db, "Se aplicó corrección de asistencia: permiso parcial del día.", $"empleado={AsistenciaActual.EmpleadoId};fecha={fecha:yyyy-MM-dd};horas={permiso.Horas:0.##};goce={permiso.ConGocePago};ausencia={permiso.Id};motivo={permiso.Motivo};saldoBanco={saldoBancoHorasSeleccionado}");
            await ReprocesarYRefrescarDiaAsync(
                db,
                fecha,
                permiso.ConGocePago
                ? "Permiso parcial guardado, banco descontado y asistencia reprocesada."
                : "Permiso parcial guardado y asistencia reprocesada.");
        }
        catch (Exception ex)
        {
            error = ex.InnerException?.Message ?? ex.Message;
        }
        finally
        {
            guardandoPermisoDia = false;
        }
    }

    private async Task QuitarPermisoDiaAsync()
    {
        if (AsistenciaActual == null || permisoDiaSeleccionado == null)
        {
            return;
        }

        error = null;
        ok = null;

        if (!PuedeReprocesar)
        {
            error = "No tienes permisos para quitar permisos del día.";
            return;
        }

        guardandoPermisoDia = true;
        try
        {
            var usuarioActual = await ObtenerUsuarioActualAsync();
            await using var db = await DbFactory.CreateDbContextAsync();
            var fecha = AsistenciaActual.Fecha;
            var permiso = await db.RrhhAusencias.FirstOrDefaultAsync(a => a.Id == permisoDiaSeleccionado.Id);
            if (permiso == null)
            {
                error = "No se encontró el permiso del día.";
                return;
            }

            permiso.IsActive = false;
            permiso.Estatus = EstatusAusenciaRrhh.Cancelada;
            permiso.UpdatedAt = DateTime.UtcNow;
            permiso.UpdatedBy = usuarioActual;

            saldoBancoHorasSeleccionado = await TiempoExtraResolutionService.RemoverPermisoBancoHorasAsync(db, _empresaId, AsistenciaActual.EmpleadoId, permiso.Id);

            await RegistrarBitacoraCorreccionAsync(db, "Se aplicó corrección de asistencia: retiro de permiso parcial del día.", $"empleado={AsistenciaActual.EmpleadoId};fecha={fecha:yyyy-MM-dd};ausencia={permiso.Id};saldoBanco={saldoBancoHorasSeleccionado}");
            await ReprocesarYRefrescarDiaAsync(db, fecha, "Permiso parcial retirado, banco restaurado y asistencia reprocesada.");
        }
        catch (Exception ex)
        {
            error = ex.InnerException?.Message ?? ex.Message;
        }
        finally
        {
            guardandoPermisoDia = false;
        }
    }

    private string ObtenerResumenPermisoDiaCaptura()
    {
        if (permisoDiaSeleccionado == null)
        {
            return "Sin permiso parcial registrado. Si el empleado tuvo permiso ese día, regístralo aquí para que el reproceso no descuente doble los descansos no marcados.";
        }

        var partes = new List<string>
        {
            $"Permiso actual: {permisoDiaSeleccionado.Horas:0.##} h",
            permisoDiaSeleccionado.ConGocePago ? "con goce" : "sin goce"
        };

        if (permisoDiaSeleccionado.ConGocePago)
        {
            partes.Add("descontado del banco de horas");
        }

        if (!string.IsNullOrWhiteSpace(permisoDiaSeleccionado.Motivo))
        {
            partes.Add(permisoDiaSeleccionado.Motivo.Trim());
        }

        return string.Join(" · ", partes);
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

    private bool PuedeMostrarResolucionTiempo()
        => asesorCorreccionActual?.ResolucionesDisponibles.Count > 0;

    private bool EsEscenarioSalidaTempranaCompensaDescanso()
        => asesorCorreccionActual?.Escenario == "SalidaTempranaCompensaDescanso";

    private bool TieneConfirmacionSalidaTempranaCompensaDescanso()
        => bitacoraCorreccionDia.Any(l => l.Mensaje.Contains("salida temprana sustituyó descanso no marcado", StringComparison.OrdinalIgnoreCase));

    private void AbrirAyudaReglas()
        => mostrarAyudaReglas = true;

    private void CerrarAyudaReglas()
        => mostrarAyudaReglas = false;

    private void AlternarAccionesRapidasTiempo()
    {
        var nuevoEstado = !_mostrarAccionesRapidasTiempo;
        _mostrarAccionesRapidasTiempo = nuevoEstado;
        if (nuevoEstado)
        {
            error = null;
            _mostrarAccionesRapidasPermiso = false;
            _mostrarAccionesRapidasTurno = false;
            minutosExtraPagoTexto = FormatearMinutosCaptura(minutosExtraPagoCaptura);
            minutosExtraBancoTexto = FormatearMinutosCaptura(minutosExtraBancoCaptura);
        }
    }

    private void AlternarAccionesRapidasPermiso()
    {
        var nuevoEstado = !_mostrarAccionesRapidasPermiso;
        _mostrarAccionesRapidasPermiso = nuevoEstado;
        if (nuevoEstado)
        {
            _mostrarAccionesRapidasTiempo = false;
            _mostrarAccionesRapidasTurno = false;
        }
    }

    private void AlternarAccionesRapidasTurno()
    {
        var nuevoEstado = !_mostrarAccionesRapidasTurno;
        _mostrarAccionesRapidasTurno = nuevoEstado;
        if (nuevoEstado)
        {
            _mostrarAccionesRapidasTiempo = false;
            _mostrarAccionesRapidasPermiso = false;
        }
    }

    private void AlternarBitacora()
        => _mostrarBitacora = !_mostrarBitacora;

    private void AlternarMarcacionesDia()
        => _mostrarMarcacionesDia = !_mostrarMarcacionesDia;

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
        else if (ObtenerMinutosFaltanteBanco(AsistenciaActual) > 0)
        {
            reglas.Add("El banco de horas solo debe usarse para cubrir faltante neto real; no para compensar un descanso que ya fue absorbido por una salida anticipada o por un permiso parcial válido.");
        }

        if (AsistenciaActual.MinutosExtra > 0)
        {
            reglas.Add($"El tiempo extra solo se resuelve cuando realmente existe extra del día ({FormatearMinutos(AsistenciaActual.MinutosExtra)}). Si hay faltante o marcaciones dudosas, primero conviene corregir la causa raíz.");
        }

        if (permisoDiaSeleccionado != null)
        {
            reglas.Add(permisoDiaSeleccionado.ConGocePago
                ? "El permiso parcial con goce descuenta saldo del banco de horas del empleado."
                : "El permiso parcial sin goce no consume banco de horas.");
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

    private async Task ConfirmarSalidaTempranaCompensaDescansoAsync()
    {
        if (AsistenciaActual == null)
        {
            return;
        }

        error = null;
        ok = null;

        if (!PuedeReprocesar)
        {
            error = "No tienes permisos para confirmar esta regla operativa.";
            return;
        }

        if (TieneConfirmacionSalidaTempranaCompensaDescanso())
        {
            ok = "Esta regla ya fue confirmada previamente.";
            return;
        }

        try
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            await RegistrarBitacoraCorreccionAsync(
                db,
                "Se aplicó corrección de asistencia: salida temprana sustituyó descanso no marcado.",
                $"empleado={AsistenciaActual.EmpleadoId};fecha={AsistenciaActual.Fecha:yyyy-MM-dd};salidaAnticipada={AsistenciaActual.MinutosSalidaAnticipada};descansoProgramado={AsistenciaActual.MinutosDescansoProgramado}");

            await db.SaveChangesAsync();
            await CargarMarcacionesDiaAsync();
            ok = "Se confirmó en bitácora que la salida temprana sustituyó el descanso no marcado.";
        }
        catch (Exception ex)
        {
            error = ex.InnerException?.Message ?? ex.Message;
        }
    }

    private void AjustarResolucionTiempoSugerida()
    {
        if (AsistenciaActual == null)
        {
            return;
        }

        var resoluble = RrhhTiempoExtraPolicy.ObtenerMinutosExtraResolubles(AsistenciaActual, factorTiempoExtraConfigurado);
        var faltante = RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteBanco(AsistenciaActual);
        minutosExtraPagoCaptura = 0;
        minutosExtraBancoCaptura = 0;
        minutosCubrirBancoCaptura = 0;

        switch (tipoResolucionTiempoExtra)
        {
            case "PagarTodo":
                minutosExtraPagoCaptura = resoluble;
                break;
            case "BancoTodo":
                minutosExtraBancoCaptura = resoluble;
                break;
            case "MitadMitad":
                minutosExtraPagoCaptura = resoluble / 2;
                minutosExtraBancoCaptura = resoluble - minutosExtraPagoCaptura;
                break;
            case "CubrirFaltanteConBanco":
                minutosCubrirBancoCaptura = faltante;
                break;
        }
    }

    private async Task AplicarResolucionTiempoAsync()
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
            var resultado = await TiempoExtraResolutionService.AplicarResolucionAsync(db, new RrhhTiempoExtraResolutionCommand
            {
                EmpresaId = _empresaId,
                AsistenciaId = AsistenciaActual.Id,
                Resolucion = tipoResolucionTiempoExtra,
                FactorTiempoExtraOverride = usarFactorTiempoExtraOverride ? factorTiempoExtraOverrideCaptura : null,
                MinutosBasePago = minutosExtraPagoCaptura,
                MinutosBaseBanco = minutosExtraBancoCaptura,
                MinutosPago = minutosExtraPagoCaptura,
                MinutosBanco = minutosExtraBancoCaptura,
                MinutosCubrirBanco = minutosCubrirBancoCaptura,
                Observaciones = resolucionTiempoExtraObservaciones,
                UsuarioActual = usuarioActual
            });

            await RegistrarBitacoraCorreccionAsync(db, "Se aplicó corrección de asistencia: resolución de tiempo extra/banco.", resultado.BitacoraDetalle);
            await db.SaveChangesAsync();

            saldoBancoHorasSeleccionado = resultado.SaldoBancoActualMinutos;
            topeBancoHorasConfigurado = resultado.TopeBancoMinutos;
            factorTiempoExtraConfigurado = resultado.FactorTiempoExtra;
            bancoHorasHabilitadoConfigurado = resultado.BancoHorasHabilitado;
            factorAcumulacionBancoHorasConfigurado = resultado.FactorAcumulacionBancoHoras;

            await RefrescarDiaAsync(db, AsistenciaActual.Fecha, "Resolución de tiempo aplicada correctamente.", recargarResolucion: true);
            RegistrarCambioPendiente("Resolución de tiempo aplicada localmente. Guarda cambios para reflejarla en otros reportes.");
        }
        catch (InvalidOperationException ex)
        {
            error = ex.Message;
        }
    }

    private async Task GuardarTurnoDiaAsync()
    {
        if (AsistenciaActual == null)
        {
            return;
        }

        error = null;
        ok = null;

        var usuarioActual = await ObtenerUsuarioActualAsync();
        await using var db = await DbFactory.CreateDbContextAsync();
        var fecha = AsistenciaActual.Fecha;
        var vigencias = await db.RrhhEmpleadosTurno
            .Where(v => v.EmpresaId == _empresaId && v.EmpleadoId == AsistenciaActual.EmpleadoId && v.IsActive)
            .ToListAsync();

        foreach (var vigente in vigencias.Where(v => v.VigenteDesde <= fecha && (v.VigenteHasta == null || v.VigenteHasta >= fecha)))
        {
            if (vigente.VigenteDesde == fecha && vigente.VigenteHasta == fecha)
            {
                db.RrhhEmpleadosTurno.Remove(vigente);
                continue;
            }

            if (vigente.VigenteDesde == fecha)
            {
                vigente.VigenteDesde = fecha.AddDays(1);
            }
            else if (vigente.VigenteHasta == null || vigente.VigenteHasta >= fecha)
            {
                vigente.VigenteHasta = fecha.AddDays(-1);
            }
            vigente.UpdatedAt = DateTime.UtcNow;
            vigente.UpdatedBy = usuarioActual;
        }

        if (Guid.TryParse(turnoDiaSeleccionadoIdTexto, out var turnoId) && turnoId != Guid.Empty)
        {
            db.RrhhEmpleadosTurno.Add(new RrhhEmpleadoTurno
            {
                Id = Guid.NewGuid(),
                EmpresaId = _empresaId,
                EmpleadoId = AsistenciaActual.EmpleadoId,
                TurnoBaseId = turnoId,
                VigenteDesde = fecha,
                VigenteHasta = fecha,
                Observaciones = string.IsNullOrWhiteSpace(turnoDiaObservaciones) ? "Cambio de turno desde asistencia." : turnoDiaObservaciones.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = usuarioActual,
                IsActive = true
            });
        }

        await RegistrarBitacoraCorreccionAsync(db, "Se aplicó corrección de asistencia: cambio de turno por día.", $"empleado={AsistenciaActual.EmpleadoId};fecha={fecha:yyyy-MM-dd};turno={turnoDiaSeleccionadoIdTexto};obs={turnoDiaObservaciones}");
        await ReprocesarYRefrescarDiaAsync(db, fecha, "Turno del día actualizado y asistencia reprocesada.");
    }

    private async Task GuardarMarcacionManualDiaAsync()
    {
        if (AsistenciaActual == null)
        {
            return;
        }

        error = null;
        ok = null;

        if (!TimeSpan.TryParse(manualHoraTexto, out var hora))
        {
            error = "La hora debe tener formato HH:mm.";
            return;
        }

        var usuarioActual = await ObtenerUsuarioActualAsync();
        await using var db = await DbFactory.CreateDbContextAsync();
        var empleado = await db.Empleados.FirstOrDefaultAsync(e => e.Id == AsistenciaActual.EmpleadoId);
        if (empleado == null)
        {
            error = "No se encontró el empleado.";
            return;
        }

        var checador = await db.RrhhChecadores
            .Where(c => c.EmpresaId == _empresaId && c.IsActive)
            .OrderBy(c => c.Nombre)
            .FirstOrDefaultAsync();

        if (checador == null)
        {
            error = "No hay checador activo configurado para registrar la marcación manual.";
            return;
        }

        var fechaLocal = AsistenciaActual.Fecha.ToDateTime(TimeOnly.MinValue).Add(hora);
        var zonaHorariaAplicada = ObtenerZonaHorariaAplicada(checador.ZonaHoraria);
        var fechaUtc = ConvertirLocalChecadorAUtc(fechaLocal, zonaHorariaAplicada);
        db.RrhhMarcaciones.Add(new RrhhMarcacion
        {
            Id = Guid.NewGuid(),
            EmpresaId = _empresaId,
            ChecadorId = checador.Id,
            EmpleadoId = AsistenciaActual.EmpleadoId,
            CodigoChecador = empleado.CodigoChecador ?? empleado.NumeroEmpleado,
            FechaHoraMarcacionLocal = DateTime.SpecifyKind(fechaLocal, DateTimeKind.Unspecified),
            FechaHoraMarcacionUtc = fechaUtc,
            ZonaHorariaAplicada = zonaHorariaAplicada,
            TipoMarcacionRaw = manualClasificacion.ToString(),
            Origen = "Manual",
            EventoIdExterno = $"MANUAL-{Guid.NewGuid():N}",
            HashUnico = $"manual:{_empresaId}:{AsistenciaActual.EmpleadoId}:{fechaUtc:O}",
            EsManual = true,
            ClasificacionOperativa = manualClasificacion,
            Procesada = false,
            ObservacionManual = string.IsNullOrWhiteSpace(manualObservacion) ? null : manualObservacion.Trim(),
            PayloadRaw = "Marcación manual creada desde Asistencias.",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = usuarioActual,
            IsActive = true
        });

        await RegistrarBitacoraCorreccionAsync(db, "Se aplicó corrección de asistencia: alta de marcación manual.", $"empleado={AsistenciaActual.EmpleadoId};fecha={AsistenciaActual.Fecha:yyyy-MM-dd};hora={manualHoraTexto};clasificacion={manualClasificacion};obs={manualObservacion}");
        await ReprocesarYRefrescarDiaAsync(db, AsistenciaActual.Fecha, "Marcación manual agregada y día reprocesado.");

        manualHoraTexto = string.Empty;
        manualObservacion = null;
    }

    private async Task CambiarClasificacionMarcacionAsync(Guid marcacionId, TipoClasificacionMarcacionRrhh clasificacion)
    {
        if (AsistenciaActual == null)
        {
            return;
        }

        var usuarioActual = await ObtenerUsuarioActualAsync();
        await using var db = await DbFactory.CreateDbContextAsync();
        var marcacion = await db.RrhhMarcaciones.FirstOrDefaultAsync(m => m.Id == marcacionId);
        if (marcacion == null)
        {
            return;
        }

        marcacion.ClasificacionOperativa = clasificacion;
        marcacion.TipoMarcacionRaw = clasificacion.ToString();
        marcacion.UpdatedAt = DateTime.UtcNow;
        marcacion.UpdatedBy = usuarioActual;
        await RegistrarBitacoraCorreccionAsync(db, "Se aplicó corrección de asistencia: cambio de clasificación de marcación.", $"empleado={AsistenciaActual.EmpleadoId};fecha={AsistenciaActual.Fecha:yyyy-MM-dd};marcacion={marcacionId};clasificacion={clasificacion}");
        await ReprocesarYRefrescarDiaAsync(db, AsistenciaActual.Fecha);
    }

    private async Task AlternarAnulacionMarcacionAsync(Guid marcacionId)
    {
        if (AsistenciaActual == null)
        {
            return;
        }

        var usuarioActual = await ObtenerUsuarioActualAsync();
        await using var db = await DbFactory.CreateDbContextAsync();
        var marcacion = await db.RrhhMarcaciones.FirstOrDefaultAsync(m => m.Id == marcacionId);
        if (marcacion == null)
        {
            return;
        }

        marcacion.EsAnulada = !marcacion.EsAnulada;
        marcacion.Procesada = false;
        marcacion.UpdatedAt = DateTime.UtcNow;
        marcacion.UpdatedBy = usuarioActual;
        await RegistrarBitacoraCorreccionAsync(db, "Se aplicó corrección de asistencia: cambio de estado de marcación.", $"empleado={AsistenciaActual.EmpleadoId};fecha={AsistenciaActual.Fecha:yyyy-MM-dd};marcacion={marcacionId};anulada={marcacion.EsAnulada}");
        await ReprocesarYRefrescarDiaAsync(db, AsistenciaActual.Fecha);
    }

    private void IniciarEdicionMarcacionManual(RrhhMarcacion marcacion)
    {
        marcacionManualEditandoId = marcacion.Id;
        marcacionManualEditandoHoraTexto = ObtenerFechaHoraLocalMarcacion(marcacion).ToString("HH:mm");
        marcacionManualEditandoObservacion = marcacion.ObservacionManual;
    }

    private void CancelarEdicionMarcacionManual()
    {
        marcacionManualEditandoId = null;
        marcacionManualEditandoHoraTexto = string.Empty;
        marcacionManualEditandoObservacion = null;
    }

    private async Task GuardarEdicionMarcacionManualAsync()
    {
        if (AsistenciaActual == null || !marcacionManualEditandoId.HasValue)
        {
            return;
        }

        error = null;
        ok = null;

        if (!TimeSpan.TryParse(marcacionManualEditandoHoraTexto, out var hora))
        {
            error = "La hora editada debe tener formato HH:mm.";
            return;
        }

        var usuarioActual = await ObtenerUsuarioActualAsync();
        await using var db = await DbFactory.CreateDbContextAsync();
        var marcacion = await db.RrhhMarcaciones
            .Include(m => m.Checador)
            .FirstOrDefaultAsync(m => m.Id == marcacionManualEditandoId.Value);
        if (marcacion == null || !marcacion.EsManual)
        {
            error = "Solo se pueden editar marcaciones manuales.";
            return;
        }

        var fechaLocal = AsistenciaActual.Fecha.ToDateTime(TimeOnly.MinValue).Add(hora);
        var zonaHorariaAplicada = string.IsNullOrWhiteSpace(marcacion.ZonaHorariaAplicada)
            ? ObtenerZonaHorariaAplicada(marcacion.Checador?.ZonaHoraria)
            : marcacion.ZonaHorariaAplicada;
        var fechaUtc = ConvertirLocalChecadorAUtc(fechaLocal, zonaHorariaAplicada);
        var hashNuevo = $"manual:{_empresaId}:{AsistenciaActual.EmpleadoId}:{fechaUtc:O}";
        var duplicada = await db.RrhhMarcaciones.AnyAsync(m => m.Id != marcacion.Id && m.HashUnico == hashNuevo);
        if (duplicada)
        {
            error = "Ya existe otra marcación manual con esa hora.";
            return;
        }

        marcacion.FechaHoraMarcacionLocal = DateTime.SpecifyKind(fechaLocal, DateTimeKind.Unspecified);
        marcacion.FechaHoraMarcacionUtc = fechaUtc;
        marcacion.ZonaHorariaAplicada = zonaHorariaAplicada;
        marcacion.HashUnico = hashNuevo;
        marcacion.ObservacionManual = string.IsNullOrWhiteSpace(marcacionManualEditandoObservacion) ? null : marcacionManualEditandoObservacion.Trim();
        marcacion.Procesada = false;
        marcacion.UpdatedAt = DateTime.UtcNow;
        marcacion.UpdatedBy = usuarioActual;
        await RegistrarBitacoraCorreccionAsync(db, "Se aplicó corrección de asistencia: edición de marcación manual.", $"empleado={AsistenciaActual.EmpleadoId};fecha={AsistenciaActual.Fecha:yyyy-MM-dd};marcacion={marcacion.Id};hora={marcacionManualEditandoHoraTexto};obs={marcacionManualEditandoObservacion}");
        await ReprocesarYRefrescarDiaAsync(db, AsistenciaActual.Fecha, "Marcación manual actualizada y día reprocesado.");

        CancelarEdicionMarcacionManual();
    }

    private async Task RegistrarBitacoraCorreccionAsync(CrmDbContext db, string mensaje, string detalle)
    {
        var usuarioActual = await ObtenerUsuarioActualAsync();
        db.RrhhLogsChecador.Add(new RrhhLogChecador
        {
            Id = Guid.NewGuid(),
            EmpresaId = _empresaId,
            FechaUtc = DateTime.UtcNow,
            Nivel = "Information",
            Mensaje = mensaje,
            Detalle = $"usuario={usuarioActual};{detalle}",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = usuarioActual,
            IsActive = true
        });
    }

    private async Task<string> ObtenerUsuarioActualAsync()
    {
        var state = await AuthStateProvider.GetAuthenticationStateAsync();
        return state.User.Identity?.Name?.Trim() is { Length: > 0 } usuario
            ? usuario
            : "desconocido";
    }

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
        if (detalle == null || !detalle.Labora || detalle.CantidadDescansos == 0)
        {
            return "Sin descansos configurados.";
        }

        var descansos = new List<string>();
        if (detalle.CantidadDescansos >= 1 && detalle.Descanso1Inicio.HasValue && detalle.Descanso1Fin.HasValue)
        {
            descansos.Add($"D1 {FormatearHoraTurno(detalle.Descanso1Inicio)}-{FormatearHoraTurno(detalle.Descanso1Fin)}{(detalle.Descanso1EsPagado ? " pagado" : string.Empty)}");
        }

        if (detalle.CantidadDescansos >= 2 && detalle.Descanso2Inicio.HasValue && detalle.Descanso2Fin.HasValue)
        {
            descansos.Add($"D2 {FormatearHoraTurno(detalle.Descanso2Inicio)}-{FormatearHoraTurno(detalle.Descanso2Fin)}{(detalle.Descanso2EsPagado ? " pagado" : string.Empty)}");
        }

        return descansos.Count == 0 ? "Sin descansos configurados." : string.Join(" · ", descansos);
    }

    private TurnoBaseDetalle? ObtenerDetalleTurnoSeleccionadoDia()
    {
        if (AsistenciaActual == null)
        {
            return null;
        }

        var turnoIdTexto = string.IsNullOrWhiteSpace(turnoDiaSeleccionadoIdTexto)
            ? AsistenciaActual.TurnoBaseId?.ToString()
            : turnoDiaSeleccionadoIdTexto;

        if (!Guid.TryParse(turnoIdTexto, out var turnoId) || turnoId == Guid.Empty)
        {
            return null;
        }

        var turno = Turnos.FirstOrDefault(t => t.Id == turnoId);
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

    private bool TieneAusenciaActual()
        => !string.IsNullOrWhiteSpace(resumenAusenciaActual);

    private string ObtenerResumenAusenciaActual()
        => resumenAusenciaActual ?? "—";

    private static string FormatearAusencia(RrhhAusencia ausencia)
        => ausencia.Tipo == TipoAusenciaRrhh.Vacaciones
            ? "Vacaciones"
            : $"Permiso {(ausencia.ConGocePago ? "con goce" : "sin goce")}{(ausencia.Horas > 0 ? $" ({ausencia.Horas:0.##} h)" : string.Empty)}";

    private string ObtenerResumenResolucion(RrhhAsistencia asistencia)
        => RrhhTiempoExtraPolicy.ObtenerResumenResolucionOperativa(asistencia, resumenAusenciaActual);

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

    private string ObtenerEstadoPermisoSeccion()
        => permisoDiaSeleccionado == null
            ? "Pendiente"
            : "Registrado";

    private string ObtenerClaseEstadoPermisoSeccion()
        => permisoDiaSeleccionado == null
            ? "text-bg-warning"
            : "text-bg-success";

    private string ObtenerEstadoCompensacionSeccion()
    {
        if (TieneCompensacionPermisoAprobada())
        {
            return "Aprobada";
        }

        return minutosRecuperablesPermisoAprobables > 0
            ? "Por revisar"
            : "No aplica";
    }

    private string ObtenerClaseEstadoCompensacionSeccion()
    {
        if (TieneCompensacionPermisoAprobada())
        {
            return "text-bg-info";
        }

        return minutosRecuperablesPermisoAprobables > 0
            ? "text-bg-warning"
            : "text-bg-light";
    }

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

    private string ObtenerGuiaCompensacionPermiso()
    {
        if (TieneCompensacionPermisoAprobada())
        {
            return "Ya existe una compensación aprobada. Ajústala solo si el tiempo recuperado validado cambió.";
        }

        return minutosRecuperablesPermisoAprobables > 0
            ? "Úsala solo si el empleado recuperó tiempo y quieres descontarlo del faltante o del permiso sugerido." 
            : "No hay tiempo recuperable pendiente; primero revisa permiso, turno o marcaciones.";
    }

    private string ObtenerGuiaTiempoResolucion()
    {
        if (AsistenciaActual == null)
        {
            return "Sin datos para resolver.";
        }

        if (AsistenciaActual.MinutosExtra > 0)
        {
            return "Cuando el permiso y las marcaciones ya están claros, aquí decides si el extra se paga o se envía a banco.";
        }

        if (ObtenerMinutosFaltanteBanco(AsistenciaActual) > 0)
        {
            return "Si después de revisar permiso y compensación aún queda faltante real, aquí puedes cubrirlo con banco si hay saldo.";
        }

        return "Usa esta sección solo cuando quede una decisión pendiente de pago o banco.";
    }

    private string ObtenerGuiaRapidaPermiso()
        => permisoDiaSeleccionado == null
            ? "Paso rápido: usa el sugerido, confirma si es con goce y guarda para reprocesar el día."
            : "Ya existe un permiso. Ajusta horas, goce o motivo y guarda para volver a reprocesar.";

    private void AplicarPermisoDiaSugerido()
    {
        if (AsistenciaActual == null)
        {
            return;
        }

        horasPermisoDiaCaptura = decimal.Round(ObtenerMinutosPermisoSugeridos(AsistenciaActual) / 60m, 2, MidpointRounding.AwayFromZero);
    }

    private int ObtenerMinutosResolubles(RrhhAsistencia asistencia)
        => RrhhTiempoExtraPolicy.ObtenerMinutosExtraResolubles(asistencia, factorTiempoExtraConfigurado);

    private int ObtenerMinutosFaltanteBanco(RrhhAsistencia asistencia)
        => RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteBanco(asistencia);

    private int ObtenerMinutosPermisoSugeridos(RrhhAsistencia asistencia)
        => RrhhTiempoExtraPolicy.ObtenerMinutosPermisoSugeridos(asistencia, minutosCompensadosPermisoAprobados);

    private int ObtenerMinutosTrabajadosVisibles(RrhhAsistencia asistencia)
        => RrhhTiempoExtraPolicy.ObtenerMinutosTrabajadosVisibles(asistencia, minutosCompensadosPermisoAprobados);

    private int ObtenerMinutosDescansoNoPagadoProgramado(RrhhAsistencia asistencia)
        => RrhhTiempoExtraPolicy.ObtenerMinutosDescansoNoPagadoProgramado(asistencia);

    private int ObtenerMinutosDescansoNoPagadoExcluidosDelPermiso(RrhhAsistencia asistencia)
        => RrhhTiempoExtraPolicy.ObtenerMinutosDescansoNoPagadoExcluidosDelPermiso(asistencia);

    private int ObtenerMinutosPermisoCapturados()
        => (int)Math.Round(Math.Max(0m, horasPermisoDiaCaptura) * 60m, MidpointRounding.AwayFromZero);

    private int ObtenerMinutosFaltanteRemanenteActual()
    {
        if (AsistenciaActual == null)
        {
            return 0;
        }

        var faltanteBruto = ObtenerMinutosPermisoSugeridos(AsistenciaActual);
        return Math.Max(0, faltanteBruto - Math.Min(faltanteBruto, ObtenerMinutosPermisoCapturados()));
    }

    private bool TieneCompensacionPermisoAprobada()
        => minutosCompensadosPermisoAprobados > 0;

    private IReadOnlyList<RrhhAsistenciaCorreccionSegmento> ObtenerSegmentosBaseTimeline()
        => asesorCorreccionActual?.Segmentos.Where(s => !s.EsAjuste).ToList() ?? [];

    private IReadOnlyList<RrhhAsistenciaCorreccionSegmento> ObtenerSegmentosAjusteTimeline()
        => asesorCorreccionActual?.Segmentos.Where(s => s.EsAjuste).ToList() ?? [];

    private bool PuedeAprobarCompensacionPermiso()
        => PuedeAprobarTiempoExtra && (ObtenerMaximoMinutosCompensacionPermisoEditable() > 0 || minutosCompensadosPermisoAprobados > 0);

    private int ObtenerMinutosCompensacionPermisoSugeridosAprobacion()
    {
        if (AsistenciaActual == null)
        {
            return 0;
        }

        var faltanteNeto = Math.Max(0, RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteBanco(AsistenciaActual));
        return Math.Max(0, Math.Min(faltanteNeto, minutosRecuperablesPermisoAprobables));
    }

    private int ObtenerMaximoMinutosCompensacionPermisoEditable()
        => Math.Max(minutosCompensadosPermisoAprobados, ObtenerMinutosCompensacionPermisoSugeridosAprobacion());

    private int ObtenerMinutosCompensacionPermisoPendientes()
        => Math.Max(0, minutosRecuperablesPermisoAprobables - minutosCompensadosPermisoAprobados);

    private string ObtenerResumenCompensacionPermiso()
    {
        if (minutosRecuperablesPermisoAprobables <= 0 && minutosCompensadosPermisoAprobados <= 0)
        {
            return "No se detecta tiempo recuperable por aprobación para este día.";
        }

        var minutosSugeridos = ObtenerMinutosCompensacionPermisoSugeridosAprobacion();

        if (minutosCompensadosPermisoAprobados > 0)
        {
            var pendientes = ObtenerMinutosCompensacionPermisoPendientes();
            return pendientes > 0
                ? $"Compensación aprobada: {FormatearMinutos(minutosCompensadosPermisoAprobados)}. Ya reduce el faltante y el permiso sugerido del día. Quedan {FormatearMinutos(pendientes)} recuperables detectados por revisar."
                : $"Compensación aprobada: {FormatearMinutos(minutosCompensadosPermisoAprobados)}. Ya fue aplicada al faltante y al tiempo visible operativo del día.";
        }

        return minutosSugeridos < minutosRecuperablesPermisoAprobables
            ? $"Tiempo recuperable detectado: {FormatearMinutos(minutosRecuperablesPermisoAprobables)}. Para este día solo conviene aprobar hasta {FormatearMinutos(minutosSugeridos)} porque ese es el faltante neto a compensar."
            : $"Tiempo recuperable detectado: {FormatearMinutos(minutosRecuperablesPermisoAprobables)}. Requiere aprobación para ajustar faltante, permiso sugerido y tiempo visible del día.";
    }

    private async Task AprobarCompensacionPermisoAsync()
    {
        if (AsistenciaActual == null)
        {
            return;
        }

        error = null;
        ok = null;

        if (!PuedeAprobarTiempoExtra)
        {
            error = "No tienes permisos para aprobar compensaciones del día.";
            return;
        }

        if (!PuedeAprobarCompensacionPermiso())
        {
            error = "No hay minutos recuperables disponibles para aprobar en este día.";
            return;
        }

        try
        {
            var usuarioActual = await ObtenerUsuarioActualAsync();
            await using var db = await DbFactory.CreateDbContextAsync();
            var fecha = AsistenciaActual.Fecha;
            var minutosAprobados = Math.Max(0, minutosCompensacionPermisoCaptura);
            var maximoEditable = ObtenerMaximoMinutosCompensacionPermisoEditable();

            if (minutosAprobados > maximoEditable)
            {
                error = $"Solo puedes aprobar hasta {FormatearMinutos(maximoEditable)} para este día.";
                return;
            }

            saldoBancoHorasSeleccionado = await TiempoExtraResolutionService.RemoverCompensacionPermisoBancoHorasAsync(db, _empresaId, AsistenciaActual.EmpleadoId, fecha);

            await RegistrarBitacoraCorreccionAsync(
                db,
                "Se aplicó corrección de asistencia: compensación aprobada de permiso.",
                $"empleado={AsistenciaActual.EmpleadoId};fecha={fecha:yyyy-MM-dd};minutosCompensados={minutosAprobados};permisoActual={permisoDiaSeleccionado?.Horas.ToString("0.##") ?? "0"};saldoBanco={saldoBancoHorasSeleccionado};usuario={usuarioActual}");

            await db.SaveChangesAsync();
            await CargarMarcacionesDiaAsync();
            await NotificarActualizacionAsync();
            ok = minutosAprobados > 0
                ? $"Compensación guardada por {FormatearMinutos(minutosAprobados)}. Solo ajusta el faltante y el permiso sugerido del día."
                : "Compensación retirada. Se restauraron el faltante y el permiso sugerido del día.";
        }
        catch (Exception ex)
        {
            error = ex.InnerException?.Message ?? ex.Message;
        }
    }

    private async Task QuitarCompensacionPermisoAsync()
    {
        minutosCompensacionPermisoCaptura = 0;
        await AprobarCompensacionPermisoAsync();
    }

    private string ObtenerResumenPermisoSugerido()
    {
        if (AsistenciaActual == null)
        {
            return "Sin datos para sugerir permiso.";
        }

        var faltanteBruto = ObtenerMinutosPermisoSugeridos(AsistenciaActual);
        var permisoCapturado = ObtenerMinutosPermisoCapturados();
        var permisoAplicado = Math.Min(faltanteBruto, permisoCapturado);
        var faltanteRemanente = Math.Max(0, faltanteBruto - permisoAplicado);
        var descansoNoPagado = ObtenerMinutosDescansoNoPagadoExcluidosDelPermiso(AsistenciaActual);
        if (faltanteBruto <= 0)
        {
            return descansoNoPagado > 0
                ? $"Del tiempo ausente, {FormatearMinutos(descansoNoPagado)} ya corresponden a descanso no pagado; hoy no se sugiere capturar permiso adicional."
                : "Hoy no se sugiere capturar permiso adicional.";
        }

        if (permisoCapturado > 0 && faltanteRemanente == 0)
        {
            return $"Con lo capturado ({FormatearMinutos(permisoCapturado)}), el permiso ya cubre el faltante neto del día.";
        }

        if (permisoCapturado > 0 && faltanteRemanente > 0)
        {
            return $"Con lo capturado ({FormatearMinutos(permisoCapturado)}), todavía quedarían {FormatearMinutos(faltanteRemanente)} por cubrir.";
        }

        if (descansoNoPagado <= 0)
        {
            return $"Permiso neto sugerido: {FormatearMinutos(faltanteBruto)}.";
        }

        return $"Permiso neto sugerido: {FormatearMinutos(faltanteBruto)}. Del tiempo ausente, {FormatearMinutos(descansoNoPagado)} corresponden a descanso no pagado y no deben capturarse como permiso.";
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

    private static bool DebeMostrarResolucionTiempo(RrhhAsistencia asistencia)
        => asistencia.MinutosExtra > 0 || RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteBanco(asistencia) > 0 || asistencia.MinutosCubiertosBancoHoras > 0;

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
}
