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

    #region Records internos

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
        string? SugerenciaAlternada = null,
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

    #endregion

    #region Campos de estado

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
    private int minutosExtraBancoAcumuladosActual;
    private int minutosMinimosTiempoExtraConfigurado = 30;
    private int minutosCompensacionPermisoCaptura;
    private int minutosPerdonManualCaptura;
    private int minutosExtraPagoCaptura;
    private int minutosExtraBancoCaptura;
    private int minutosCubrirBancoCaptura;
    private string minutosExtraPagoTexto = "0:00";
    private string minutosExtraBancoTexto = "0:00";
    private bool usarFactorTiempoExtraOverride;
    private decimal factorTiempoExtraOverrideCaptura;
    // "EntradaSalida" | "NetoVsNeto"
    private string modoSugerenciaExtra = "EntradaSalida";
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
    private bool _mostrarAccionesRapidasModoExtra;
    private bool _mostrarBitacora;
    private bool _mostrarMarcacionesDia;
    private bool _mostrarDescansosNoDescontar;
    private HashSet<int> _descansosNoDescontarSeleccionados = [];
    private int _toleranciaExcesoDescansoMinutos = RrhhAsistenciaDescansoSettings.ToleranciaExcesoDescansoDefault;
    private TurnoBaseDetalle? _detalleTurnoVigenteCache;
    private static readonly SemaphoreSlim _initLock = new(1, 1);

    #endregion

    #region Lifecycle

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

        await _initLock.WaitAsync();
        try
        {
            if (_ultimaAsistenciaCargadaId == Asistencia.Id && AsistenciaActual != null)
            {
                return;
            }

            await InicializarAsync();
        }
        finally
        {
            _initLock.Release();
        }
    }

    #endregion

    #region Inicialización y contexto

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
                .ThenInclude(e => e.TurnoBase)
                    .ThenInclude(t => t.Detalles)
                        .ThenInclude(d => d.Descansos)
            .Include(a => a.TurnoBase)
                .ThenInclude(t => t.Detalles)
                    .ThenInclude(d => d.Descansos)
            .FirstOrDefaultAsync(a => a.Id == Asistencia.Id)
            ?? Asistencia;

        await ResolverTurnoVigenteCacheAsync();

        // Reprocesar silenciosamente para garantizar que MinutosExtra y demás métricas
        // reflejen las reglas actuales del procesador. Sin esto, un valor obsoleto
        // persistido en la BD (de un cálculo anterior) podría mostrarse incorrectamente
        // y confundir al usuario (ej. "Extra detectada 9:52 h" con solo 2h trabajadas).
        // Guardar cualquier cambio pendiente del contexto antes de reprocesar.
        if (_draftDb.ChangeTracker.HasChanges())
        {
            await _draftDb.SaveChangesAsync();
        }
        await RrhhAsistenciaProcessor.ReprocesarRangoAsync(_draftDb, _empresaId, AsistenciaActual.Fecha, AsistenciaActual.Fecha, AsistenciaActual.EmpleadoId);

        // Recargar la asistencia con los valores recalculados
        _draftDb.ChangeTracker.Clear();
        AsistenciaActual = await _draftDb.RrhhAsistencias
            .Include(a => a.Empleado)
                .ThenInclude(e => e.TurnoBase)
                    .ThenInclude(t => t.Detalles)
                        .ThenInclude(d => d.Descansos)
            .Include(a => a.TurnoBase)
                .ThenInclude(t => t.Detalles)
                    .ThenInclude(d => d.Descansos)
            .FirstOrDefaultAsync(a => a.Id == Asistencia.Id)
            ?? AsistenciaActual;

        _ultimaAsistenciaCargadaId = AsistenciaActual.Id;
        ReiniciarEstadoVisual();
        tipoResolucionTiempoExtra = "PagarTodo";
        resolucionTiempoExtraObservaciones = null;
        modoSugerenciaExtra = AsistenciaActual.ModoSugerenciaExtra ?? "EntradaSalida";
        await CargarContextoTiempoExtraAsync(_draftDb);
        _toleranciaExcesoDescansoMinutos = (await RrhhAsistenciaDescansoSettings.LoadAsync(_draftDb, _empresaId)).ToleranciaExcesoDescansoMinutos;
        CargarDescansosNoDescontar(AsistenciaActual);
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

        var horasBancoAcumuladasActual = await db.RrhhBancoHorasMovimientos
            .AsNoTracking()
            .Where(m => m.EmpresaId == _empresaId
                && m.EmpleadoId == AsistenciaActual.EmpleadoId
                && m.IsActive
                && m.ReferenciaTipo == RrhhTiempoExtraPolicy.ConstruirReferenciaResolucion(AsistenciaActual.Id, "extra-banco"))
            .SumAsync(m => (decimal?)m.Horas) ?? 0m;

        minutosExtraBancoAcumuladosActual = (int)Math.Round(horasBancoAcumuladasActual * 60m, MidpointRounding.AwayFromZero);

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

    private async Task RefrescarDiaAsync(CrmDbContext db, DateOnly fecha, string? mensajeOk = null, bool recargarResolucion = false)
    {
        await CargarContextoDiaAsync(db, fecha, recargarAsistencia: true);
        if (recargarResolucion && AsistenciaActual != null)
        {
            CargarCapturaResolucion(AsistenciaActual);
        }

        if (AsistenciaActual != null)
        {
            CargarDescansosNoDescontar(AsistenciaActual);
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

        // Guardar cambios pendientes en el contexto antes de reprocesar.
        // El procesador consulta marcaciones desde la BD con ToListAsync, no desde el
        // ChangeTracker, por lo que los cambios no guardados (clasificación, payload,
        // EsAnulada) no se verían al reprocesar, causando inconsistencias entre los
        // bloques fijados manualmente y los cálculos del tiempo visible.
        await db.SaveChangesAsync();
        await ReconciliarResolucionesSegmentoDiaAsync(db, fecha);
        await RrhhAsistenciaProcessor.ReprocesarRangoAsync(db, _empresaId, fecha, fecha, AsistenciaActual.EmpleadoId);
        await RefrescarDiaAsync(db, fecha, mensajeOk, recargarResolucion);
        RegistrarCambioPendiente(string.IsNullOrWhiteSpace(mensajeOk) ? "Cambios pendientes en el día. Guarda al cerrar para aplicarlos definitivamente." : mensajeOk);
    }

    #endregion

    #region Estado visual y cierre

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
        minutosPerdonManualCaptura = AsistenciaActual?.MinutosPerdonadosManual ?? 0;
        ReiniciarEdicionManual();
        CancelarEdicionSegmento();
        _mostrarAccionesRapidasTiempo = false;
        _mostrarAccionesRapidasPermiso = false;
        _mostrarAccionesRapidasTurno = false;
        _mostrarAccionesRapidasModoExtra = false;
        _mostrarBitacora = false;
        modoSugerenciaExtra = "EntradaSalida";
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

    private async Task NotificarActualizacionAsync()
    {
        if (OnUpdated.HasDelegate)
        {
            await OnUpdated.InvokeAsync();
        }
    }

    private void AbrirMarcacionesDesdeResumen()
    {
    }

    #endregion

    #region Bitácora y usuario

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

    #endregion
}