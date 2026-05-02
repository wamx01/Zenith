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

    private const string TabModalResumen = "resumen";
    private const string TabModalMarcaciones = "marcaciones";
    private const string TabModalPermisos = "permisos";
    private const string TabModalTiempo = "tiempo";

    private Guid _empresaId;
    private Guid? _ultimaAsistenciaCargadaId;
    private string _tabModalActiva = TabModalResumen;
    private bool cargando;
    private bool guardandoPermisoDia;
    private string? error;
    private string? ok;
    private RrhhAsistencia? AsistenciaActual;
    private List<RrhhMarcacion> marcacionesDia = [];
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

    protected override async Task OnParametersSetAsync()
    {
        if (!Visible || Asistencia == null)
        {
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
        _tabModalActiva = TabModalResumen;
        await CargarEmpresaAsync();

        if (Asistencia == null || _empresaId == Guid.Empty)
        {
            return;
        }

        await using var db = await DbFactory.CreateDbContextAsync();
        AsistenciaActual = await db.RrhhAsistencias
            .AsNoTracking()
            .Include(a => a.Empleado)
            .Include(a => a.TurnoBase)
            .FirstOrDefaultAsync(a => a.Id == Asistencia.Id)
            ?? Asistencia;

        _ultimaAsistenciaCargadaId = AsistenciaActual.Id;
        turnoDiaSeleccionadoIdTexto = AsistenciaActual.TurnoBaseId?.ToString() ?? string.Empty;
        turnoDiaObservaciones = null;
        manualHoraTexto = string.Empty;
        manualClasificacion = TipoClasificacionMarcacionRrhh.Entrada;
        manualObservacion = null;
        marcacionManualEditandoId = null;
        marcacionManualEditandoHoraTexto = string.Empty;
        marcacionManualEditandoObservacion = null;
        tipoResolucionTiempoExtra = "PagarTodo";
        resolucionTiempoExtraObservaciones = null;
        await CargarContextoTiempoExtraAsync(db);
        CargarCapturaResolucion(AsistenciaActual);
        await CargarMarcacionesDiaAsync();
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
            marcacionesDia = [];
            bitacoraCorreccionDia = [];
            permisoDiaSeleccionado = null;
            resumenAusenciaActual = null;
            ActualizarAsesorCorreccion();
            return;
        }

        await using var db = await DbFactory.CreateDbContextAsync();
        var fecha = AsistenciaActual.Fecha.ToDateTime(TimeOnly.MinValue);
        var desdeUtc = DateTime.SpecifyKind(fecha.AddHours(-14), DateTimeKind.Utc);
        var hastaUtc = DateTime.SpecifyKind(fecha.AddDays(1).AddHours(14), DateTimeKind.Utc);

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
            horasPermisoDiaCaptura = AsistenciaActual == null
                ? 0
                : decimal.Round(ObtenerMinutosPermisoSugeridos(AsistenciaActual) / 60m, 2, MidpointRounding.AwayFromZero);
            permisoDiaConGoceCaptura = true;
            permisoDiaMotivo = null;
            permisoDiaObservaciones = null;
            ActualizarAsesorCorreccion();
            return;
        }

        horasPermisoDiaCaptura = permisoDiaSeleccionado.Horas;
        permisoDiaConGoceCaptura = permisoDiaSeleccionado.ConGocePago;
        permisoDiaMotivo = permisoDiaSeleccionado.Motivo;
        permisoDiaObservaciones = permisoDiaSeleccionado.Observaciones;
        ActualizarAsesorCorreccion();
    }

    private async Task CerrarAsync()
    {
        _ultimaAsistenciaCargadaId = null;
        AsistenciaActual = null;
        error = null;
        ok = null;
        mostrarAyudaReglas = false;
        if (OnClose.HasDelegate)
        {
            await OnClose.InvokeAsync();
        }
    }

    private void SeleccionarTabModal(string tab)
        => _tabModalActiva = tab;

    private string ObtenerClaseTabModal(string tab)
        => _tabModalActiva == tab ? "nav-link active" : "nav-link";

    private void IrATabSugerida()
    {
        if (asesorCorreccionActual == null)
        {
            return;
        }

        _tabModalActiva = asesorCorreccionActual.TabSugerida;
        if (!string.IsNullOrWhiteSpace(asesorCorreccionActual.ResolucionSugerida))
        {
            tipoResolucionTiempoExtra = asesorCorreccionActual.ResolucionSugerida;
            AjustarResolucionTiempoSugerida();
        }
    }

    private bool EsTabSugerida(string tab)
        => asesorCorreccionActual?.TabSugerida == tab;

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
            await RrhhAsistenciaProcessor.ReprocesarRangoAsync(db, _empresaId, fecha, fecha, AsistenciaActual.EmpleadoId);

            await RecargarAsistenciaActualAsync(db, fecha);
            await CargarMarcacionesDiaAsync();
            await NotificarActualizacionAsync();
            ok = permiso.ConGocePago
                ? "Permiso parcial guardado, banco descontado y asistencia reprocesada."
                : "Permiso parcial guardado y asistencia reprocesada.";
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
            await RrhhAsistenciaProcessor.ReprocesarRangoAsync(db, _empresaId, fecha, fecha, AsistenciaActual.EmpleadoId);

            await RecargarAsistenciaActualAsync(db, fecha);
            await CargarMarcacionesDiaAsync();
            await NotificarActualizacionAsync();
            ok = "Permiso parcial retirado, banco restaurado y asistencia reprocesada.";
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
            return;
        }

        AjustarResolucionTiempoSugerida();
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

            await RecargarAsistenciaActualAsync(db, AsistenciaActual.Fecha);
            if (AsistenciaActual != null)
            {
                CargarCapturaResolucion(AsistenciaActual);
            }
            await CargarMarcacionesDiaAsync();
            await NotificarActualizacionAsync();
            ok = "Resolución de tiempo aplicada correctamente.";
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
        await RrhhAsistenciaProcessor.ReprocesarRangoAsync(db, _empresaId, fecha, fecha, AsistenciaActual.EmpleadoId);

        await RecargarAsistenciaActualAsync(db, fecha);
        await CargarMarcacionesDiaAsync();
        await NotificarActualizacionAsync();
        ok = "Turno del día actualizado y asistencia reprocesada.";
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
        await RrhhAsistenciaProcessor.ReprocesarRangoAsync(db, _empresaId, AsistenciaActual.Fecha, AsistenciaActual.Fecha, AsistenciaActual.EmpleadoId);

        manualHoraTexto = string.Empty;
        manualObservacion = null;
        await RecargarAsistenciaActualAsync(db, AsistenciaActual.Fecha);
        await CargarMarcacionesDiaAsync();
        await NotificarActualizacionAsync();
        ok = "Marcación manual agregada y día reprocesado.";
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
        await RrhhAsistenciaProcessor.ReprocesarRangoAsync(db, _empresaId, AsistenciaActual.Fecha, AsistenciaActual.Fecha, AsistenciaActual.EmpleadoId);
        await RecargarAsistenciaActualAsync(db, AsistenciaActual.Fecha);
        await CargarMarcacionesDiaAsync();
        await NotificarActualizacionAsync();
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
        await RrhhAsistenciaProcessor.ReprocesarRangoAsync(db, _empresaId, AsistenciaActual.Fecha, AsistenciaActual.Fecha, AsistenciaActual.EmpleadoId);
        await RecargarAsistenciaActualAsync(db, AsistenciaActual.Fecha);
        await CargarMarcacionesDiaAsync();
        await NotificarActualizacionAsync();
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
        await RrhhAsistenciaProcessor.ReprocesarRangoAsync(db, _empresaId, AsistenciaActual.Fecha, AsistenciaActual.Fecha, AsistenciaActual.EmpleadoId);

        CancelarEdicionMarcacionManual();
        await RecargarAsistenciaActualAsync(db, AsistenciaActual.Fecha);
        await CargarMarcacionesDiaAsync();
        await NotificarActualizacionAsync();
        ok = "Marcación manual actualizada y día reprocesado.";
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
            || observaciones.Contains("Entrada anticipada", StringComparison.OrdinalIgnoreCase))
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
