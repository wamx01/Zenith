using System.Globalization;
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

// Code-behind de AsistenciasSemanal.razor: el marcado (@page, @using, @inject,
// @rendermode y el árbol de render) vive en el .razor; aquí sólo la lógica C#.
// Las dependencias inyectadas (DbFactory, AuthStateProvider, JS,
// ResolucionPeriodo) las declara el .razor con @inject y el compilador las
// genera como propiedades de esta misma partial class.
public partial class AsistenciasSemanal
{
    private static readonly CultureInfo CulturaEsMx = new("es-MX");
    private static readonly List<RrhhAsistenciaEstatus> estatusDisponibles = Enum.GetValues<RrhhAsistenciaEstatus>().ToList();
    private List<DateOnly> diasPeriodo = [];
    private List<ResumenSemanalEmpleado> resumenes = [];
    private List<TurnoBase> turnos = [];
    private Dictionary<DateOnly, int> totalesPorDia = new();
    private Dictionary<string, int> permisosPorEmpleado = new();
    private Dictionary<string, int> permisosSinGocePorEmpleado = new();
    private bool _puedeVer;
    private bool _puedeReprocesar;
    private bool _puedeAprobarTiempoExtra;
    private bool _filtrosInicialesCargados;
    private bool mostrarCorreccionDia;
    private Guid _empresaId;
    private string _usuarioActual = string.Empty;
    private bool cargando;
    // Bandera dedicada al recálculo de periodo dentro del drawer: cubre TODO el
    // lapso (reproceso + reload grilla + refresco drawer) sin parpadeo, a diferencia
    // de `cargando` que CargarAsync deja en false a mitad de la operación.
    // English: Dedicated flag for the in-drawer period recalc: spans the whole run
    // (reprocess + grid reload + drawer refresh) without flicker, unlike `cargando`
    // which CargarAsync flips to false mid-operation.
    private bool _recalcPeriodo;
    private bool exportandoCsv;
    private string? error;
    private string? ok;
    private RrhhAsistencia? asistenciaSeleccionada;
    private PeriodicidadPago periodicidadSeleccionada = PeriodicidadPago.Semanal;
    private DateTime fechaReferenciaPeriodo = DateTime.Today;
    private DateOnly periodoFechaInicio;
    private DateOnly periodoFechaFin;
    private string periodoEtiquetaActual = string.Empty;
    private NominaCorteRrhh? cortePeriodo;
    private Dictionary<Guid, RrhhResolucionTiempoExtraPeriodo> resolucionPorEmpleado = new();
    // Resumen neteado por periodo por empleado (batch, sin N+1): las MISMAS 9 columnas
    // del drawer "Resumen de nómina del periodo" traídas al listado para que coincidan.
    // English: Netted per-period summary per employee (batch, no N+1): the SAME 9 columns
    // from the drawer "Resumen de nómina del periodo" brought into the listing so they match.
    private IReadOnlyDictionary<Guid, RrhhResolucionPeriodoResumen> resumenNeteoPorEmpleado = new Dictionary<Guid, RrhhResolucionPeriodoResumen>();
    // Empleados cuyo extra del periodo YA se pagó (Nomina.Estatus==Pagada para
    // este periodo que los incluye). Se calcula en CargarAsync junto con la
    // resolución, y lo usa el drawer para pintar "Extra pagado" (verde) vs
    // "Extra por pagar" (azul, autorizado pero nómina no pagada) vs "Extra
    // detectado" (ámbar, periodo aún no autorizado).
    private HashSet<Guid> empleadosPagadosPeriodo = new();
    private bool _resolucionVisible;
    private Guid _resolucionEmpleadoId;
    private string _resolucionNombre = string.Empty;
    private int _resolucionMinutosExtra;
    private int _resolucionSaldoBanco;
    // F9 — aceptación en lote del tiempo del periodo (go a prenomina).
    private bool _mostrarConfirmacionLote;
    private bool _lotePagarTodo;
    // Drawer lateral de detalle: muestra la grilla de días + todas las métricas
    // del empleado seleccionado. Reusa el resumen ya cargado en memoria (sin
    // query nueva) y el modal de resolución existente para "Resolver extra".
    private bool _detalleVisible;
    private Guid _detalleEmpleadoId;
    // Bruto por día del empleado del drawer, calculado bajo demanda desde sus
    // marcaciones al abrir el detalle (CargarBrutoDetalleAsync).
    private Dictionary<DateOnly, int> _detalleBrutoPorDia = new();
    private int _detalleTotalBruto;
    // Resumen de nómina del periodo (mismo DTO que el modal de resolución) para mostrar
    // la "tablita" en el drawer en vez de las cards. Se carga al abrir el detalle.
    // English: Period payroll summary (same DTO as the resolution modal) to show the
    // "little table" in the drawer instead of the cards. Loaded when the detail opens.
    private RrhhResolucionPeriodoResumen? _detalleResumen;
    private RrhhResolucionTiempoExtraPeriodo? _detalleResolucion;
    private string filtroTurnoIdTexto = string.Empty;
    private string filtroEstatus = "todos";
    private string filtroRevision = "todos";
    private string filtroOrden = "nombre";
    private string? filtroEmpleado;
    private int totalMinutosPeriodo;
    private int promedioMinutosEmpleado;
    private int totalMinutosPagadosPeriodo;
    private int promedioMinutosPagadosEmpleado;
    private Dictionary<string, int> permisosPorDia = new();
    private Dictionary<string, RrhhAsistencia> asistenciasPorEmpleadoDia = new();
    private HashSet<DateOnly> festivosPeriodo = [];

    protected override async Task OnInitializedAsync()
    {
        await CargarAccesoAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_puedeVer || _filtrosInicialesCargados)
        {
            return;
        }

        try
        {
            await CargarFiltrosPersistidosAsync();
            _filtrosInicialesCargados = true;
            await CargarAsync();
            await InvokeAsync(StateHasChanged);
        }
        catch (InvalidOperationException)
        {
        }
    }

    private async Task CargarAccesoAsync()
    {
        var state = await AuthStateProvider.GetAuthenticationStateAsync();
        _puedeVer = state.User.HasClaim("Capacidad", "rrhh.asistencias.ver")
            || state.User.HasClaim("Capacidad", "empleados.ver")
            || state.User.HasClaim("Capacidad", "nominas.ver");
        _puedeReprocesar = state.User.HasClaim("Capacidad", "rrhh.asistencias.editar")
            || state.User.HasClaim("Capacidad", "empleados.editar")
            || state.User.HasClaim("Capacidad", "nominas.editar");
        _puedeAprobarTiempoExtra = state.User.HasClaim("Capacidad", "rrhh.tiempoextra.aprobar") || state.User.HasClaim("Capacidad", "nominas.editar");
        _ = Guid.TryParse(state.User.FindFirst("EmpresaId")?.Value, out _empresaId);
        _usuarioActual = state.User.Identity?.Name ?? string.Empty;
    }

    private async Task CargarAsync()
    {
        cargando = true;
        error = null;
        ok = null;

        try
        {
            if (_empresaId == Guid.Empty)
            {
                error = "No hay empresa activa.";
                return;
            }

            await using var db = await DbFactory.CreateDbContextAsync();
            await CargarCortePeriodoAsync(db);
            var (desde, hasta) = ObtenerPeriodoActual();
            periodoFechaInicio = desde;
            periodoFechaFin = hasta;
            diasPeriodo = Enumerable.Range(0, hasta.DayNumber - desde.DayNumber + 1)
                .Select(desde.AddDays)
                .ToList();

            turnos = await db.TurnosBase
                .Include(t => t.Detalles)
                .AsNoTracking()
                .Where(t => t.EmpresaId == _empresaId && t.IsActive)
                .OrderBy(t => t.Nombre)
                .ToListAsync();

            var asistencias = await ConstruirConsulta(db, desde, hasta)
                .OrderBy(a => a.Empleado.Nombre)
                .ThenBy(a => a.Empleado.ApellidoPaterno)
                .ThenBy(a => a.Empleado.ApellidoMaterno)
                .ThenBy(a => a.Fecha)
                .ToListAsync();

            festivosPeriodo = await db.FestivosRrhh
                .AsNoTracking()
                .Where(f => f.IsActive && f.Fecha >= desde.ToDateTime(TimeOnly.MinValue) && f.Fecha <= hasta.ToDateTime(TimeOnly.MinValue))
                .Select(f => DateOnly.FromDateTime(f.Fecha.Date))
                .ToHashSetAsync();

            asistenciasPorEmpleadoDia = asistencias.ToDictionary(a => CrearClaveAsistencia(a.EmpleadoId, a.Fecha));

            permisosPorEmpleado = await ConstruirPermisosPorEmpleadoAsync(db, asistencias, desde, hasta);
            permisosSinGocePorEmpleado = await ConstruirPermisosPorEmpleadoAsync(db, asistencias, desde, hasta, conGoce: false);
            permisosPorDia = await ConstruirPermisosPorDiaAsync(db, asistencias, desde, hasta);

            resumenes = asistencias
                .GroupBy(a => new
                {
                    a.EmpleadoId,
                    Nombre = a.Empleado.NombreCompleto,
                    a.Empleado.NumeroEmpleado,
                    a.Empleado.CodigoChecador
                })
                .Select(grupo => new ResumenSemanalEmpleado
                {
                    EmpleadoId = grupo.Key.EmpleadoId,
                    NombreCompleto = grupo.Key.Nombre,
                    NumeroEmpleado = grupo.Key.NumeroEmpleado,
                    CodigoChecador = grupo.Key.CodigoChecador,
                    EsPorHoras = grupo.All(a => a.EsPorHoras),
                    TotalMinutosNormales = grupo.Sum(a => RrhhTiempoExtraPolicy.ObtenerMinutosBasePagada(a)),
                    TotalMinutosExtra = grupo.Sum(a => Math.Max(0, a.MinutosExtra)),
                    TotalMinutosExtraAprobadoPago = grupo.Sum(a => Math.Max(0, a.MinutosExtraAutorizadosPago)),
                    TotalMinutosExtraAprobadoBanco = grupo.Sum(a => 0),
                    TotalMinutosBancoAplicado = grupo.Sum(a => Math.Max(0, a.MinutosCubiertosBancoHoras)),
                    TotalMinutosFaltante = grupo.Sum(a => RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteNeto(a)),
                     TotalMinutosDescuento = grupo.Sum(a => RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteDescontable(a, ObtenerMinutosPermisoAplicados(a), ObtenerMinutosCompensados(a))),
                    TotalMinutosRetardo = grupo.Sum(a => RrhhTiempoExtraPolicy.ObtenerMinutosRetardoEfectivos(a, ObtenerMinutosPermisoAplicados(a))),
                    TotalMinutosSalidaAnticipada = grupo.Sum(a => RrhhTiempoExtraPolicy.ObtenerMinutosSalidaAnticipadaEfectivos(a)),
                     MinutosDescuentoPorDia = grupo.GroupBy(a => a.Fecha).ToDictionary(g => g.Key, g => g.Sum(x => RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteDescontable(x, ObtenerMinutosPermisoAplicados(x), ObtenerMinutosCompensados(x)))),
                    TotalDiasTrabajados = grupo.Where(a => a.Estatus is RrhhAsistenciaEstatus.AsistenciaNormal or RrhhAsistenciaEstatus.Retardo or RrhhAsistenciaEstatus.Incompleta or RrhhAsistenciaEstatus.SalidaAnticipada or RrhhAsistenciaEstatus.DescansoTrabajado).Select(a => a.Fecha).Distinct().Count(),
                    TotalDiasFestivoTrabajado = grupo.Where(a => a.Estatus is RrhhAsistenciaEstatus.AsistenciaNormal or RrhhAsistenciaEstatus.Retardo or RrhhAsistenciaEstatus.Incompleta or RrhhAsistenciaEstatus.SalidaAnticipada or RrhhAsistenciaEstatus.DescansoTrabajado).Count(a => EsFestivoTrabajado(a.Fecha)),
                    TotalDiasAusentismo = grupo.Count(a => a.Estatus == RrhhAsistenciaEstatus.Falta),
                    TotalDiasRetardo = grupo.Count(a => a.Estatus == RrhhAsistenciaEstatus.Retardo),
                    TotalDiasIncompleto = grupo.Count(a => a.Estatus == RrhhAsistenciaEstatus.Incompleta),
                    MinutosRetardoPorDia = grupo.GroupBy(a => a.Fecha).ToDictionary(g => g.Key, g => g.Sum(x => RrhhTiempoExtraPolicy.ObtenerMinutosRetardoEfectivos(x, ObtenerMinutosPermisoAplicados(x)))),
                    MinutosSalidaAnticipadaPorDia = grupo.GroupBy(a => a.Fecha).ToDictionary(g => g.Key, g => g.Sum(x => RrhhTiempoExtraPolicy.ObtenerMinutosSalidaAnticipadaEfectivos(x))),
                    DiasTrabajadosDetalle = grupo.Where(a => a.Estatus is RrhhAsistenciaEstatus.AsistenciaNormal or RrhhAsistenciaEstatus.Retardo or RrhhAsistenciaEstatus.Incompleta or RrhhAsistenciaEstatus.SalidaAnticipada or RrhhAsistenciaEstatus.DescansoTrabajado).Select(a => a.Fecha).Distinct().ToList(),
                    DiasFestivoTrabajadoDetalle = grupo.Where(a => a.Estatus is RrhhAsistenciaEstatus.AsistenciaNormal or RrhhAsistenciaEstatus.Retardo or RrhhAsistenciaEstatus.Incompleta or RrhhAsistenciaEstatus.SalidaAnticipada or RrhhAsistenciaEstatus.DescansoTrabajado).Select(a => a.Fecha).Where(EsFestivoTrabajado).Distinct().ToList(),
                    TotalMinutosCompensacion = grupo.Sum(a => ObtenerMinutosCompensados(a)),
                    TotalMinutosPermisoConGoce = permisosPorEmpleado.GetValueOrDefault(grupo.Key.EmpleadoId.ToString("N")),
                    TotalMinutosPermisoSinGoce = permisosSinGocePorEmpleado.GetValueOrDefault(grupo.Key.EmpleadoId.ToString("N")),
                    MinutosPorDia = grupo
                        .GroupBy(a => a.Fecha)
                        .ToDictionary(g => g.Key, g => g.Sum(x => ObtenerMinutosTiempoAcreditado(x))),
                    MinutosDebidosPorDia = grupo
                        .GroupBy(a => a.Fecha)
                        .ToDictionary(g => g.Key, g => g.Sum(x => Math.Max(0, x.MinutosJornadaNetaProgramada)))
                    // El bruto por día se calcula bajo demanda al abrir el detalle
                    // del empleado (CargarBrutoDetalleAsync), no aquí.
                })
                .ToList();

            var referenciasBancoExtraPeriodo = asistencias
                .Select(a => RrhhTiempoExtraPolicy.ConstruirReferenciaResolucion(a.Id, "extra-banco"))
                .Distinct()
                .ToList();

            var bancoExtraPorAsistencia = await db.RrhhBancoHorasMovimientos
                .AsNoTracking()
                .Where(m => m.EmpresaId == _empresaId
                    && m.IsActive
                    && m.TipoMovimiento == TipoMovimientoBancoHorasRrhh.GeneradoPorHorasExtra
                    && referenciasBancoExtraPeriodo.Contains(m.ReferenciaTipo))
                .Select(m => new
                {
                    m.EmpleadoId,
                    m.ReferenciaTipo,
                    Minutos = (int)Math.Round(m.Horas * 60m, MidpointRounding.AwayFromZero)
                })
                .ToListAsync();

            var bancoExtraMinutosPorEmpleado = bancoExtraPorAsistencia
                .GroupBy(m => m.EmpleadoId)
                .ToDictionary(g => g.Key, g => g.Sum(x => Math.Max(0, x.Minutos)));

            // Saldo banco de horas por empleado (acumulado histórico hasta fecha de corte)
            var empIds = resumenes.Select(r => r.EmpleadoId).ToHashSet();
            var bancoMovs = await db.RrhhBancoHorasMovimientos
                .AsNoTracking()
                .Where(m => m.EmpresaId == _empresaId && empIds.Contains(m.EmpleadoId))
                .GroupBy(m => m.EmpleadoId)
                .Select(g => new
                {
                    EmpleadoId = g.Key,
                    SaldoHoras = g.Sum(m => m.TipoMovimiento == TipoMovimientoBancoHorasRrhh.Consumo ? -m.Horas : m.Horas)
                })
                .ToDictionaryAsync(x => x.EmpleadoId, x => (int)(x.SaldoHoras * 60));

            // Una sola pasada para aplicar ambos totales del banco (extra acumulado + saldo).
            resumenes = resumenes.Select(r => r with
            {
                TotalMinutosExtraAprobadoBanco = bancoExtraMinutosPorEmpleado.GetValueOrDefault(r.EmpleadoId),
                SaldoBancoHoras = bancoMovs.GetValueOrDefault(r.EmpleadoId)
            }).ToList();

            // Resoluciones de tiempo extra del periodo: una sola query para todos los
            // empleados mostrados (comparten periodicidad → mismo periodo de nómina).
            // Variante contenedor: el viewer muestra el periodo que contiene la
            // fecha-referencia (incluye el en curso), no el último cerrado.
            var calendarioPeriodo = NominaPeriodoHelper.ObtenerPeriodoContenedor(periodicidadSeleccionada, fechaReferenciaPeriodo, cortePeriodo);
            var resoluciones = await db.RrhhResolucionesTiempoExtraPeriodo
                .AsNoTracking()
                .Include(r => r.Lineas)
                .Where(r => r.EmpresaId == _empresaId
                    && r.PeriodicidadPago == periodicidadSeleccionada
                    && r.AnioPeriodo == calendarioPeriodo.AnioPeriodo
                    && r.NumeroPeriodo == calendarioPeriodo.NumeroPeriodo
                    && empIds.Contains(r.EmpleadoId))
                .ToListAsync();
            resolucionPorEmpleado = resoluciones.ToDictionary(r => r.EmpleadoId);

            // Resumen neteado por periodo en batch (una sola pasada para todos los empleados
            // mostrados): trae al listado las 9 columnas neteadas del drawer "Resumen de
            // nómina del periodo" usando el MISMO neteo del servicio → sin drift listado vs
            // drawer. Reusa calendarioPeriodo + rango ya calculados (coinciden con el drawer,
            // que deriva su calendario de periodoFechaInicio/Fin).
            // English: Batched per-period netted summary (one pass for all displayed employees):
            // brings into the listing the 9 netted columns from the drawer using the SAME service
            // netting → no listing-vs-drawer drift. Reuses calendarioPeriodo + range already
            // computed (they match the drawer, which derives its calendario from periodoFechaInicio/Fin).
            resumenNeteoPorEmpleado = await ResolucionPeriodo.ObtenerResumenesPeriodoBatchAsync(
                db, _empresaId, empIds, periodoFechaInicio, periodoFechaFin,
                periodicidadSeleccionada, calendarioPeriodo);

            // Empleados del periodo cuya nómina ya está Pagada → su extra aprobado
            // ya se cobró. Se usa en el drawer para pintar "Extra pagado" (verde)
            // vs "Extra por pagar" (azul). Una sola query por periodo; el filtro
            // por empleado se hace con Contains en el markup.
            var empleadosPagados = await db.Nominas
                .AsNoTracking()
                .Where(n => n.EmpresaId == _empresaId
                    && n.PeriodicidadPago == periodicidadSeleccionada
                    && n.AnioPeriodo == calendarioPeriodo.AnioPeriodo
                    && n.NumeroPeriodo == calendarioPeriodo.NumeroPeriodo
                    && n.Estatus == EstatusNomina.Pagada)
                .SelectMany(n => n.Detalles.Select(d => d.EmpleadoId))
                .Distinct()
                .ToListAsync();
            empleadosPagadosPeriodo = empleadosPagados.ToHashSet();

            // Overlay de la resolución por periodo: el flujo de periodo guarda el extra
            // aprobado en RrhhResolucionTiempoExtraPeriodo.MinutosExtraPago/Banco y NO
            // escribe los campos diarios (RrhhAsistencia.MinutosExtraAutorizados*),
            // así que el build inicial desde asistencias no ve el extra aceptado.
            // Sin este overlay, "Pagadas" y "Acreditado" no reflejan el tiempo aceptado.
            var referenciasPeriodoBancoExtra = resoluciones
                .Where(r => r.Estatus == RrhhResolucionPeriodoEstatus.Autorizada && !r.ExtraDescartado)
                .Select(r => $"Periodo:{r.EmpleadoId:N}:{r.PeriodoKey}:extra-banco")
                .ToList();

            var bancoExtraPeriodoPorEmpleado = new Dictionary<Guid, int>();
            if (referenciasPeriodoBancoExtra.Count > 0)
            {
                var bancoExtraPeriodo = await db.RrhhBancoHorasMovimientos
                    .AsNoTracking()
                    .Where(m => m.EmpresaId == _empresaId
                        && m.IsActive
                        && m.TipoMovimiento == TipoMovimientoBancoHorasRrhh.GeneradoPorHorasExtra
                        && referenciasPeriodoBancoExtra.Contains(m.ReferenciaTipo))
                    .Select(m => new
                    {
                        m.EmpleadoId,
                        Minutos = (int)Math.Round(m.Horas * 60m, MidpointRounding.AwayFromZero)
                    })
                    .ToListAsync();

                bancoExtraPeriodoPorEmpleado = bancoExtraPeriodo
                    .GroupBy(m => m.EmpleadoId)
                    .ToDictionary(g => g.Key, g => g.Sum(x => Math.Max(0, x.Minutos)));
            }

            resumenes = resumenes.Select(r =>
            {
                var resolucion = resolucionPorEmpleado.GetValueOrDefault(r.EmpleadoId);
                var autorizada = resolucion is { Estatus: RrhhResolucionPeriodoEstatus.Autorizada };

                if (!autorizada)
                {
                    // Periodo no autorizado: sin datos de extra/banco, pero marcamos que está pendiente.
                    return r with
                    {
                        PeriodoAutorizado = false,
                        TotalMinutosExtraPagoBase = 0,
                        TotalMinutosExtraBancoBase = 0,
                        ExtraPagoFactorDetalle = string.Empty,
                        ExtraBancoFactorDetalle = string.Empty,
                        ExtraDescartado = false
                    };
                }

                if (resolucion!.ExtraDescartado)
                {
                    return r with
                    {
                        PeriodoAutorizado = true,
                        ExtraDescartado = true,
                        TotalMinutosExtraAprobadoPago = 0,
                        TotalMinutosExtraAprobadoBanco = 0,
                        TotalMinutosExtraPagoBase = 0,
                        TotalMinutosExtraBancoBase = 0,
                        ExtraPagoFactorDetalle = "Descartado",
                        ExtraBancoFactorDetalle = string.Empty
                    };
                }

                var extraPago = Math.Max(0, resolucion.MinutosExtraPago);
                var extraBancoBase = Math.Max(0, resolucion.MinutosExtraBanco);
                var extraPagoFactorDetalle = ConstruirFactorDetalle(resolucion, RrhhDestinoTiempoExtraLinea.Pago);
                var extraBancoFactorDetalle = ConstruirFactorDetalle(resolucion, RrhhDestinoTiempoExtraLinea.Banco);

                return r with
                {
                    PeriodoAutorizado = true,
                    ExtraDescartado = false,
                    TotalMinutosNormales = r.EsPorHoras
                        ? Math.Max(0, r.TotalMinutosNormales - extraPago)
                        : r.TotalMinutosNormales,
                    TotalMinutosExtraAprobadoPago = extraPago,
                    TotalMinutosExtraAprobadoBanco = bancoExtraPeriodoPorEmpleado.GetValueOrDefault(r.EmpleadoId),
                    TotalMinutosExtraPagoBase = extraPago,
                    TotalMinutosExtraBancoBase = extraBancoBase,
                    ExtraPagoFactorDetalle = extraPagoFactorDetalle,
                    ExtraBancoFactorDetalle = extraBancoFactorDetalle,
                    TotalMinutosAdjustment = r.EsPorHoras ? 0 : extraPago + extraBancoBase
                };
            }).ToList();

            resumenes = filtroOrden switch
            {
                "numero" => resumenes.OrderBy(x => x.NumeroEmpleado).ThenBy(x => x.NombreCompleto).ToList(),
                "checador" => resumenes.OrderBy(x => x.CodigoChecador).ThenBy(x => x.NombreCompleto).ToList(),
                _ => resumenes.OrderBy(x => x.NombreCompleto).ThenBy(x => x.NumeroEmpleado).ToList()
            };

            totalMinutosPeriodo = resumenes.Sum(x => x.TotalMinutos);
            promedioMinutosEmpleado = resumenes.Count == 0 ? 0 : totalMinutosPeriodo / resumenes.Count;
            totalMinutosPagadosPeriodo = resumenes.Sum(x => x.TotalMinutosPagados);
            promedioMinutosPagadosEmpleado = resumenes.Count == 0 ? 0 : totalMinutosPagadosPeriodo / resumenes.Count;
            totalesPorDia = diasPeriodo.ToDictionary(dia => dia, dia => resumenes.Sum(x => x.MinutosPorDia.GetValueOrDefault(dia)));
            await GuardarFiltrosPersistidosAsync();
        }
        catch (Exception ex)
        {
            error = ex.InnerException?.Message ?? ex.Message;
            diasPeriodo = [];
            resumenes = [];
            turnos = [];
            totalesPorDia = new();
            permisosPorEmpleado = new();
            permisosSinGocePorEmpleado = new();
            permisosPorDia = new();
            asistenciasPorEmpleadoDia = new();
            festivosPeriodo = [];
            resolucionPorEmpleado = new();
            empleadosPagadosPeriodo = new();
            totalMinutosPeriodo = 0;
            promedioMinutosEmpleado = 0;
            totalMinutosPagadosPeriodo = 0;
            promedioMinutosPagadosEmpleado = 0;
        }
        finally
        {
            cargando = false;
        }
    }

    private IQueryable<RrhhAsistencia> ConstruirConsulta(CrmDbContext db, DateOnly desde, DateOnly hasta)
    {
        var query = db.RrhhAsistencias
            .AsNoTracking()
            .Include(a => a.Empleado)
            .Include(a => a.TurnoBase)
            .Where(a => a.EmpresaId == _empresaId && a.Fecha >= desde && a.Fecha <= hasta
                && a.Empleado.PeriodicidadPago == periodicidadSeleccionada
                && a.Empleado.TipoNomina != TipoNomina.Destajo);

        if (Guid.TryParse(filtroTurnoIdTexto, out var turnoId) && turnoId != Guid.Empty)
        {
            query = query.Where(a => a.TurnoBaseId == turnoId);
        }

        if (int.TryParse(filtroEstatus, out var estatusValor) && Enum.IsDefined(typeof(RrhhAsistenciaEstatus), estatusValor))
        {
            var estatus = (RrhhAsistenciaEstatus)estatusValor;
            query = query.Where(a => a.Estatus == estatus);
        }

        query = filtroRevision switch
        {
            "si" => query.Where(a => a.RequiereRevision),
            "no" => query.Where(a => !a.RequiereRevision),
            _ => query
        };

        if (!string.IsNullOrWhiteSpace(filtroEmpleado))
        {
            var texto = filtroEmpleado.Trim();
            query = query.Where(a =>
                a.Empleado.Nombre.Contains(texto) ||
                (a.Empleado.ApellidoPaterno != null && a.Empleado.ApellidoPaterno.Contains(texto)) ||
                (a.Empleado.ApellidoMaterno != null && a.Empleado.ApellidoMaterno.Contains(texto)) ||
                (a.Empleado.NumeroEmpleado != null && a.Empleado.NumeroEmpleado.Contains(texto)) ||
                (a.Empleado.CodigoChecador != null && a.Empleado.CodigoChecador.Contains(texto)));
        }

        return query;
    }

    private (DateOnly Desde, DateOnly Hasta) ObtenerPeriodoActual()
    {
        if (_empresaId == Guid.Empty)
        {
            throw new InvalidOperationException("No hay empresa activa.");
        }

        var calendario = NominaPeriodoHelper.ObtenerPeriodoContenedor(periodicidadSeleccionada, fechaReferenciaPeriodo, cortePeriodo);
        periodoEtiquetaActual = calendario.Periodo;
        return (DateOnly.FromDateTime(calendario.Inicio), DateOnly.FromDateTime(calendario.Fin));
    }

    private async Task CargarCortePeriodoAsync(CrmDbContext db)
    {
        cortePeriodo = await db.NominaCortesRrhh
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.EmpresaId == _empresaId && c.PeriodicidadPago == periodicidadSeleccionada);
    }

    private async Task MoverPeriodoAsync(int direccionPeriodos)
    {
        var calendario = NominaPeriodoHelper.ObtenerPeriodoContenedor(periodicidadSeleccionada, fechaReferenciaPeriodo, cortePeriodo);
        fechaReferenciaPeriodo = direccionPeriodos > 0
            ? calendario.FechaReferenciaSiguiente()
            : calendario.FechaReferenciaAnterior();
        await GuardarFiltrosPersistidosAsync();
        await CargarAsync();
    }

    private async Task LimpiarFiltrosAsync()
    {
        AplicarFiltros(RrhhAsistenciasFiltroState.CreateDefault());
        await LimpiarFiltrosPersistidosAsync();
        await CargarAsync();
    }

    private async Task CargarFiltrosPersistidosAsync()
    {
        try
        {
            var filtrosJson = await JS.InvokeAsync<string?>("mundoVsAuth.getLocal", RrhhAsistenciasFiltroState.WeeklyStorageKey);
            if (string.IsNullOrWhiteSpace(filtrosJson))
            {
                filtrosJson = await JS.InvokeAsync<string?>("mundoVsAuth.getLocal", RrhhAsistenciasFiltroState.StorageKey);
                if (!string.IsNullOrWhiteSpace(filtrosJson))
                {
                    var filtrosHeredados = JsonSerializer.Deserialize<RrhhAsistenciasFiltroState>(filtrosJson);
                    var filtrosIniciales = filtrosHeredados ?? RrhhAsistenciasFiltroState.CreateDefault();
                    AplicarFiltros(filtrosIniciales);
                    await GuardarFiltrosPersistidosAsync();
                    return;
                }

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
        await JS.InvokeVoidAsync("mundoVsAuth.setLocal", RrhhAsistenciasFiltroState.WeeklyStorageKey, filtrosJson);
    }

    private async Task LimpiarFiltrosPersistidosAsync()
        => await JS.InvokeVoidAsync("mundoVsAuth.removeLocal", RrhhAsistenciasFiltroState.WeeklyStorageKey);

    private RrhhAsistenciasFiltroState CrearEstadoFiltros()
        => new()
        {
            TurnoIdTexto = filtroTurnoIdTexto,
            Estatus = filtroEstatus,
            Revision = filtroRevision,
            Orden = filtroOrden,
            Empleado = string.IsNullOrWhiteSpace(filtroEmpleado) ? null : filtroEmpleado.Trim(),
            Periodicidad = (int)periodicidadSeleccionada,
            FechaReferenciaPeriodo = fechaReferenciaPeriodo
        };

    private void AplicarFiltros(RrhhAsistenciasFiltroState filtros)
    {
        periodicidadSeleccionada = filtros.Periodicidad.HasValue && Enum.IsDefined(typeof(PeriodicidadPago), filtros.Periodicidad.Value)
            ? (PeriodicidadPago)filtros.Periodicidad.Value
            : PeriodicidadPago.Semanal;
        fechaReferenciaPeriodo = filtros.FechaReferenciaPeriodo ?? DateTime.Today;
        filtroTurnoIdTexto = filtros.TurnoIdTexto ?? string.Empty;
        filtroEstatus = string.IsNullOrWhiteSpace(filtros.Estatus) ? "todos" : filtros.Estatus;
        filtroRevision = string.IsNullOrWhiteSpace(filtros.Revision) ? "todos" : filtros.Revision;
        filtroOrden = string.IsNullOrWhiteSpace(filtros.Orden) ? "nombre" : filtros.Orden;
        filtroEmpleado = string.IsNullOrWhiteSpace(filtros.Empleado) ? null : filtros.Empleado.Trim();
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
            RrhhAsistenciaEstatus.SalidaAnticipada => "Salida anticipada",
            _ => "Pendiente"
        };

    private async Task ExportarCsvAsync()
    {
        if (resumenes.Count == 0)
        {
            error = "No hay información para exportar.";
            return;
        }

        exportandoCsv = true;
        error = null;
        ok = null;

        try
        {
            var builder = new StringBuilder();
            var encabezados = new List<string> { "Empleado", "NumeroEmpleado", "CodigoChecador" };
            encabezados.AddRange(diasPeriodo.Select(d => d.ToString("yyyy-MM-dd")));
            encabezados.Add("HorasPagadasSemanal");
            encabezados.Add("HorasPagadasSemanalMin");
            encabezados.Add("HorasNormalesSemanal");
            encabezados.Add("HorasNormalesSemanalMin");
            encabezados.Add("HorasBancoAplicadoSemanal");
            encabezados.Add("HorasBancoAplicadoSemanalMin");
            encabezados.Add("TiempoExtraSemanal");
            encabezados.Add("TiempoExtraSemanalMin");
            encabezados.Add("TiempoExtraPagoBaseSemanal");
            encabezados.Add("TiempoExtraPagoBaseSemanalMin");
            encabezados.Add("TiempoExtraPagoFactorDetalle");
            encabezados.Add("TiempoExtraBancoBaseSemanal");
            encabezados.Add("TiempoExtraBancoBaseSemanalMin");
            encabezados.Add("TiempoExtraBancoFactorDetalle");
            encabezados.Add("TiempoExtraAprobadoPagoSemanal");
            encabezados.Add("TiempoExtraAprobadoPagoSemanalMin");
            encabezados.Add("TiempoExtraAprobadoBancoAcumuladoSemanal");
            encabezados.Add("TiempoExtraAprobadoBancoAcumuladoSemanalMin");
            encabezados.Add("PermisosConGoceSemanal");
            encabezados.Add("PermisosConGoceSemanalMin");
            encabezados.Add("PermisosSinGoceSemanal");
            encabezados.Add("PermisosSinGoceSemanalMin");
            encabezados.Add("CompensacionesSemanal");
            encabezados.Add("CompensacionesSemanalMin");
            encabezados.Add("DescuentoSemanal");
            encabezados.Add("DescuentoSemanalMin");
            encabezados.Add("RetardoSemanal");
            encabezados.Add("RetardoSemanalMin");
            encabezados.Add("DiasAusentismo");
            encabezados.Add("DiasRetardo");
            encabezados.Add("DiasIncompleto");
            encabezados.Add("TotalIncidencias");
            encabezados.Add("SaldoBancoHoras");
            encabezados.Add("SaldoBancoHorasMin");
            encabezados.Add("TotalHoras");
            encabezados.Add("TotalMinutos");
            builder.AppendLine(string.Join(",", encabezados.Select(EscapeCsv)));

            foreach (var resumen in resumenes)
            {
                var columnas = new List<string>
                {
                    EscapeCsv(resumen.NombreCompleto),
                    EscapeCsv(resumen.NumeroEmpleado),
                    EscapeCsv(resumen.CodigoChecador)
                };

                columnas.AddRange(diasPeriodo.Select(dia => EscapeCsv(FormatearMinutos(resumen.MinutosPorDia.GetValueOrDefault(dia)))));
                columnas.Add(EscapeCsv(FormatearMinutos(resumen.TotalMinutosPagados)));
                columnas.Add(resumen.TotalMinutosPagados.ToString());
                columnas.Add(EscapeCsv(FormatearMinutos(resumen.TotalMinutosNormales)));
                columnas.Add(resumen.TotalMinutosNormales.ToString());
                columnas.Add(EscapeCsv(FormatearMinutos(resumen.TotalMinutosBancoAplicado)));
                columnas.Add(resumen.TotalMinutosBancoAplicado.ToString());
                columnas.Add(EscapeCsv(FormatearMinutos(resumen.TotalMinutosExtra)));
                columnas.Add(resumen.TotalMinutosExtra.ToString());
                columnas.Add(EscapeCsv(FormatearMinutos(resumen.TotalMinutosExtraPagoBase)));
                columnas.Add(resumen.TotalMinutosExtraPagoBase.ToString());
                columnas.Add(EscapeCsv(resumen.ExtraPagoFactorDetalle));
                columnas.Add(EscapeCsv(FormatearMinutos(resumen.TotalMinutosExtraBancoBase)));
                columnas.Add(resumen.TotalMinutosExtraBancoBase.ToString());
                columnas.Add(EscapeCsv(resumen.ExtraBancoFactorDetalle));
                columnas.Add(EscapeCsv(FormatearMinutos(resumen.TotalMinutosExtraAprobadoPago)));
                columnas.Add(resumen.TotalMinutosExtraAprobadoPago.ToString());
                columnas.Add(EscapeCsv(FormatearMinutos(resumen.TotalMinutosExtraAprobadoBanco)));
                columnas.Add(resumen.TotalMinutosExtraAprobadoBanco.ToString());
                columnas.Add(EscapeCsv(FormatearMinutos(resumen.TotalMinutosPermisoConGoce)));
                columnas.Add(resumen.TotalMinutosPermisoConGoce.ToString());
                columnas.Add(EscapeCsv(FormatearMinutos(resumen.TotalMinutosPermisoSinGoce)));
                columnas.Add(resumen.TotalMinutosPermisoSinGoce.ToString());
                columnas.Add(EscapeCsv(FormatearMinutos(resumen.TotalMinutosCompensacion)));
                columnas.Add(resumen.TotalMinutosCompensacion.ToString());
                columnas.Add(EscapeCsv(FormatearMinutos(resumen.TotalMinutosDescuento)));
                columnas.Add(resumen.TotalMinutosDescuento.ToString());
                columnas.Add(EscapeCsv(FormatearMinutos(resumen.TotalMinutosRetardo)));
                columnas.Add(resumen.TotalMinutosRetardo.ToString());
                columnas.Add(resumen.TotalDiasAusentismo.ToString());
                columnas.Add(resumen.TotalDiasRetardo.ToString());
                columnas.Add(resumen.TotalDiasIncompleto.ToString());
                columnas.Add(resumen.TotalIncidencias.ToString());
                columnas.Add(EscapeCsv(FormatearMinutos(resumen.SaldoBancoHoras)));
                columnas.Add(resumen.SaldoBancoHoras.ToString());
                columnas.Add(EscapeCsv(FormatearMinutos(resumen.TotalMinutos)));
                columnas.Add(resumen.TotalMinutos.ToString());
                builder.AppendLine(string.Join(",", columnas));
            }

            var totales = new List<string>
            {
                EscapeCsv("Totales"),
                EscapeCsv(null),
                EscapeCsv(null)
            };

            totales.AddRange(diasPeriodo.Select(dia => EscapeCsv(FormatearMinutos(totalesPorDia.GetValueOrDefault(dia)))));
            totales.Add(EscapeCsv(FormatearMinutos(totalMinutosPagadosPeriodo)));
            totales.Add(totalMinutosPagadosPeriodo.ToString());
            totales.Add(EscapeCsv(FormatearMinutos(resumenes.Sum(x => x.TotalMinutosNormales))));
            totales.Add(resumenes.Sum(x => x.TotalMinutosNormales).ToString());
            totales.Add(EscapeCsv(FormatearMinutos(resumenes.Sum(x => x.TotalMinutosBancoAplicado))));
            totales.Add(resumenes.Sum(x => x.TotalMinutosBancoAplicado).ToString());
            totales.Add(EscapeCsv(FormatearMinutos(resumenes.Sum(x => x.TotalMinutosExtra))));
            totales.Add(resumenes.Sum(x => x.TotalMinutosExtra).ToString());
            totales.Add(EscapeCsv(FormatearMinutos(resumenes.Sum(x => x.TotalMinutosExtraAprobadoPago))));
            totales.Add(resumenes.Sum(x => x.TotalMinutosExtraAprobadoPago).ToString());
            totales.Add(EscapeCsv(FormatearMinutos(resumenes.Sum(x => x.TotalMinutosExtraAprobadoBanco))));
            totales.Add(resumenes.Sum(x => x.TotalMinutosExtraAprobadoBanco).ToString());
            totales.Add(EscapeCsv(FormatearMinutos(resumenes.Sum(x => x.TotalMinutosPermisoConGoce))));
            totales.Add(resumenes.Sum(x => x.TotalMinutosPermisoConGoce).ToString());
            totales.Add(EscapeCsv(FormatearMinutos(resumenes.Sum(x => x.TotalMinutosPermisoSinGoce))));
            totales.Add(resumenes.Sum(x => x.TotalMinutosPermisoSinGoce).ToString());
            totales.Add(EscapeCsv(FormatearMinutos(resumenes.Sum(x => x.TotalMinutosCompensacion))));
            totales.Add(resumenes.Sum(x => x.TotalMinutosCompensacion).ToString());
            totales.Add(EscapeCsv(FormatearMinutos(resumenes.Sum(x => x.TotalMinutosDescuento))));
            totales.Add(resumenes.Sum(x => x.TotalMinutosDescuento).ToString());
            totales.Add(EscapeCsv(FormatearMinutos(resumenes.Sum(x => x.TotalMinutosRetardo))));
            totales.Add(resumenes.Sum(x => x.TotalMinutosRetardo).ToString());
            totales.Add(resumenes.Sum(x => x.TotalDiasAusentismo).ToString());
            totales.Add(resumenes.Sum(x => x.TotalDiasRetardo).ToString());
            totales.Add(resumenes.Sum(x => x.TotalDiasIncompleto).ToString());
            totales.Add(resumenes.Sum(x => x.TotalIncidencias).ToString());
            totales.Add(EscapeCsv(FormatearMinutos(resumenes.Sum(x => x.SaldoBancoHoras))));
            totales.Add(resumenes.Sum(x => x.SaldoBancoHoras).ToString());
            totales.Add(EscapeCsv(FormatearMinutos(totalMinutosPeriodo)));
            totales.Add(totalMinutosPeriodo.ToString());
            builder.AppendLine(string.Join(",", totales));

            var nombreArchivo = $"asistencias-semanal-{DateTime.Now:yyyyMMdd-HHmmss}.csv";
            await JS.InvokeVoidAsync("mundoVsAuth.downloadCsv", nombreArchivo, builder.ToString());
            ok = $"CSV generado correctamente con {resumenes.Count} empleado(s).";
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

    private static string ObtenerAbreviaturaDia(DateOnly fecha)
    {
        var texto = fecha.ToString("ddd", CulturaEsMx);
        return string.IsNullOrWhiteSpace(texto)
            ? fecha.DayOfWeek.ToString()
            : char.ToUpperInvariant(texto[0]) + texto[1..].TrimEnd('.');
    }

    private static bool EsFinSemana(DateOnly fecha)
        => fecha.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    private static string FormatearMinutos(int minutos)
    {
        if (minutos <= 0)
        {
            return "—";
        }

        var horas = minutos / 60;
        var resto = minutos % 60;
        return $"{horas:D2}:{resto:D2}";
    }

    // Horas decimales (ej. 3212 min → "53.53") para la tablita de nómina del drawer,
    // mismo formato que el modal de resolución. / Decimal hours (e.g. 3212 min →
    // "53.53") for the drawer payroll table, same format as the resolution modal.
    private static string FormatearHorasDecimales(int minutos) => (minutos / 60.0).ToString("0.00");

    // Jornada programada (base pagada como salario) del resumen del drawer. Se toma directa
    // de la jornada programada/meta del periodo (MinutosBasePagadaPeriodo) en vez de
    // trabajado − extraDetectado: el extra per-día tiene umbral y puede ser menor que
    // (trabajado − meta), y esos minutos "perdidos" se colaban como base (bug 48.03 vs 48.00).
    // English: Scheduled jornada (base paid as salary) for the drawer summary. Taken directly
    // from the period's scheduled jornada/meta (MinutosBasePagadaPeriodo) instead of worked −
    // detectedExtra: per-day extra has a threshold and can be less than (worked − meta), and
    // those "lost" minutes leaked into the base (48.03 vs 48.00 bug).
    private int CalcularJornadaProgramadaDetalle()
    {
        return _detalleResumen?.MinutosBasePagadaPeriodo ?? 0;
    }

    // Extra que queda tras el neteo del periodo: el extra detectado tapa primero el
    // faltante neto, luego el retardo, y por último repone el banco consumido. Lo que
    // sobra es lo realmente pagable/autorizable (las "líneas" del modal de resolución).
    // English: extra left after period netting — detected extra first covers the net
    // shortfall, then tardiness, then restores consumed bank. The remainder is what's
    // actually payable/authorizable (the "lines" in the resolution modal).
    private int CalcularExtraAbsorbibleDetalle()
    {
        if (_detalleResumen is null) return 0;
        return Math.Max(0, _detalleResumen.MinutosExtraAbsorbible);
    }

    // Deducciones NETEADAS del periodo: lo que realmente descuenta la nómina tras el neteo
    // del extra (detectado − absorbido). Espejo exacto del sourcing "periodo". Hrs Pagadas
    // = jornada − (estas tres), así: Hrs Pagadas + Ded.faltante + Ded.retardo + Ded.salida
    // = jornada. Bruto detectado está en el bloque de neteo (y por día como "det.").
    // English: NETTED period deductions: what payroll actually docks after extra nets them
    // (detected − absorbed). Exact mirror of the "periodo" sourcing. Paid Hours = jornada
    // − (these three), so: Paid Hours + Ded.shortage + Ded.late + Ded.leave = jornada.
    // Raw detected is in the neteo block (and per-day as "det.").
    private int CalcularDedFaltanteNetaDetalle()
        => _detalleResumen is null ? 0 : Math.Max(0, _detalleResumen.MinutosFaltanteNetoPeriodo - _detalleResumen.MinutosFaltanteAbsorbidoExtra);
    private int CalcularDedRetardoNetoDetalle()
        => _detalleResumen is null ? 0 : Math.Max(0, _detalleResumen.MinutosRetardoDetectado - _detalleResumen.MinutosRetardoAbsorbidoExtra);
    private int CalcularDedSalidaNetaDetalle()
        => _detalleResumen is null ? 0 : Math.Max(0, _detalleResumen.MinutosSalidaAnticipadaDetectado - _detalleResumen.MinutosSalidaAnticipadaAbsorbidoExtra);

    // Variantes por empleado para el LISTADO (operan sobre el resumen batch, no sobre
    // _detalleResumen). Espejo exacto de las del drawer → listado y drawer coinciden.
    // English: Per-employee variants for the LISTING (operate on the batch summary, not on
    // _detalleResumen). Exact mirror of the drawer ones → listing and drawer match.
    private static int CalcularDedFaltanteNeta(RrhhResolucionPeriodoResumen? r)
        => r is null ? 0 : Math.Max(0, r.MinutosFaltanteNetoPeriodo - r.MinutosFaltanteAbsorbidoExtra);
    private static int CalcularDedRetardoNeta(RrhhResolucionPeriodoResumen? r)
        => r is null ? 0 : Math.Max(0, r.MinutosRetardoDetectado - r.MinutosRetardoAbsorbidoExtra);
    private static int CalcularDedSalidaNeta(RrhhResolucionPeriodoResumen? r)
        => r is null ? 0 : Math.Max(0, r.MinutosSalidaAnticipadaDetectado - r.MinutosSalidaAnticipadaAbsorbidoExtra);

    private static string EscapeCsv(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return "\"\"";
        }

        return $"\"{valor.Replace("\"", "\"\"")}\"";
    }

    // Tiempo bruto trabajado de un día a partir de los instantes (hora local) de
    // sus marcaciones, ordenados. Es el "tal cual entrada-salida, sin reglas":
    // span(primera -> última) menos la suma de los pares del medio, que se
    // interpretan como descansos (salida/regreso). NO depende de
    // ClasificacionOperativa (el processor la infiere por posición y no la
    // persiste, así que la mayoría de las marcas quedan SinClasificar). Los
    // descansos no tomados (sin pares del medio) no se descuentan: el span
    // completo cuenta como trabajado. Un par del medio non-decreasing se
    // ignora. Con menos de 2 marcas no hay bruto.
    private static int CalcularMinutosBrutosTrabajados(List<DateTime> instantes)
    {
        if (instantes is null || instantes.Count < 2)
        {
            return 0;
        }

        var spanTotal = (int)Math.Round((instantes[^1] - instantes[0]).TotalMinutes, MidpointRounding.AwayFromZero);
        if (spanTotal <= 0)
        {
            return 0;
        }

        // Pares del medio: marcas entre la primera y la última, emparejadas
        // (1,2),(3,4),... como descansos salida->regreso.
        var descansoMinutos = 0;
        for (var i = 1; i + 1 < instantes.Count; i += 2)
        {
            var salida = instantes[i];
            var regreso = instantes[i + 1];
            if (regreso > salida)
            {
                descansoMinutos += (int)Math.Round((regreso - salida).TotalMinutes, MidpointRounding.AwayFromZero);
            }
        }

        return Math.Max(0, spanTotal - descansoMinutos);
    }

    // Hora local de una marcación: usa FechaHoraMarcacionLocal si está poblada
    // (la ingestión siempre la setea); si no, convierte la UTC con la zona del
    // checador (o la zona por defecto de México). Es el mismo criterio que
    // RrhhAsistenciaProcessor.ObtenerFechaHoraLocalMarcacion.
    private static DateTime ObtenerFechaHoraLocalMarcacion(RrhhMarcacion marcacion)
    {
        if (marcacion.FechaHoraMarcacionLocal.HasValue)
        {
            return DateTime.SpecifyKind(marcacion.FechaHoraMarcacionLocal.Value, DateTimeKind.Unspecified);
        }

        var zonaHoraria = string.IsNullOrWhiteSpace(marcacion.ZonaHorariaAplicada)
            ? marcacion.Checador?.ZonaHoraria
            : marcacion.ZonaHorariaAplicada;
        var utc = marcacion.FechaHoraMarcacionUtc.Kind == DateTimeKind.Utc
            ? marcacion.FechaHoraMarcacionUtc
            : DateTime.SpecifyKind(marcacion.FechaHoraMarcacionUtc, DateTimeKind.Utc);
        return DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(utc, ResolverZonaHorariaMarcacion(zonaHoraria)), DateTimeKind.Unspecified);
    }

    private static TimeZoneInfo ResolverZonaHorariaMarcacion(string? zonaHoraria)
    {
        if (!string.IsNullOrWhiteSpace(zonaHoraria))
        {
            var normalizada = zonaHoraria.Trim();
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(normalizada);
            }
            catch (TimeZoneNotFoundException)
            {
                if (string.Equals(normalizada, "America/Mexico_City", StringComparison.OrdinalIgnoreCase))
                {
                    try { return TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)"); } catch (TimeZoneNotFoundException) { }
                }
            }
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
    }

    private int ObtenerMinutosTiempoAcreditado(RrhhAsistencia asistencia)
        => RrhhTiempoExtraPolicy.ObtenerMinutosTiempoVisible(asistencia, ObtenerMinutosPermisoAplicados(asistencia), ObtenerMinutosCompensados(asistencia));

    private int ObtenerMinutosCompensados(RrhhAsistencia asistencia)
        => Math.Max(0, asistencia.MinutosCompensacionPermisoAprobados);

    private int ObtenerMinutosPermisoAplicados(RrhhAsistencia asistencia)
        => permisosPorDia.TryGetValue(CrearClaveDia(asistencia.EmpleadoId, asistencia.Fecha), out var minutos)
            ? Math.Max(0, minutos)
            : 0;

    private static string CrearClaveDia(Guid empleadoId, DateOnly fecha)
        => $"{empleadoId:N}:{fecha:yyyyMMdd}";

    private static string CrearClaveAsistencia(Guid empleadoId, DateOnly fecha)
        => $"{empleadoId:N}:{fecha:yyyyMMdd}";

    private int ObtenerTotalColumnasTabla()
        => 12; // Empleado + Días + 9 columnas neteadas + Detalle.
               // English: Employee + Days + 9 netted columns + Detail.

    // Estado semántico de la celda de un día. Sustituye al ternario anidado que
    // decidía la clase CSS; el orden de prioridad (extra > sin movimiento > ok >
    // warn > danger) se preserva en ObtenerEstadoCelda.
    private enum EstadoCeldaSemanal
    {
        Muted,
        Ok,
        Warn,
        Danger,
        ExtraPending,
        ExtraReviewed
    }

    private static EstadoCeldaSemanal ObtenerEstadoCelda(
        int minutosExtra, bool extraResuelto, int visible, int debido, int delta, int descuentoDia)
    {
        if (minutosExtra > 0)
        {
            return extraResuelto ? EstadoCeldaSemanal.ExtraReviewed : EstadoCeldaSemanal.ExtraPending;
        }
        if (visible <= 0 && debido <= 0)
        {
            return EstadoCeldaSemanal.Muted;
        }
        // El descuento real (FaltanteDescontable del día) manda sobre el delta visible.
        // Un día puede cubrir el "debe" con cobertura del banco de horas (que suma al
        // visible) y aun así tener descuento, porque la cobertura NO reduce el
        // FaltanteDescontable. Sin esto, la celda se ve verde mintiendo sobre el descuento.
        if (descuentoDia > 0)
        {
            return descuentoDia <= 30 ? EstadoCeldaSemanal.Warn : EstadoCeldaSemanal.Danger;
        }
        if (delta >= 0)
        {
            return EstadoCeldaSemanal.Ok;
        }
        return Math.Abs(delta) <= 30 ? EstadoCeldaSemanal.Warn : EstadoCeldaSemanal.Danger;
    }

    private static string ObtenerClaseCelda(EstadoCeldaSemanal estado)
        => estado switch
        {
            EstadoCeldaSemanal.ExtraPending => "asis-week-cell__button--extra-pending",
            EstadoCeldaSemanal.ExtraReviewed => "asis-week-cell__button--extra-reviewed",
            EstadoCeldaSemanal.Muted => "asis-week-cell__button--muted",
            EstadoCeldaSemanal.Ok => "asis-week-cell__button--ok",
            EstadoCeldaSemanal.Warn => "asis-week-cell__button--warn",
            EstadoCeldaSemanal.Danger => "asis-week-cell__button--danger",
            _ => string.Empty
        };

    private PresentacionCeldaSemanal ObtenerPresentacionDia(ResumenSemanalEmpleado resumen, DateOnly dia, Dictionary<DateOnly, int>? brutoPorDia = null)
    {
        var acreditado = resumen.MinutosPorDia.GetValueOrDefault(dia);
        var debido = resumen.MinutosDebidosPorDia.GetValueOrDefault(dia);
        var delta = acreditado - debido;
        var descuentoDia = resumen.MinutosDescuentoPorDia.GetValueOrDefault(dia);
        asistenciasPorEmpleadoDia.TryGetValue(CrearClaveAsistencia(resumen.EmpleadoId, dia), out var asistencia);
        var minutosExtra = asistencia == null ? 0 : Math.Max(0, asistencia.MinutosExtra);
        var minutosExtraAprobados = asistencia == null ? 0 : Math.Max(0, asistencia.MinutosExtraAutorizadosPago + asistencia.MinutosExtraAutorizadosBanco);
        // "Resuelto" = el extra ya fue autorizado/aplicado. Con el flujo semanal (Fase 7/8)
        // la autorización vive en la resolución del PERIODO (RrhhResolucionTiempoExtraPeriodo),
        // no en los campos por día de la asistencia, que ya no se pueblan. Si sólo
        // miráramos TieneResolucionTiempoAplicada(asistencia), los días con extra se
        // quedarían siempre en morado (pendiente) aunque el periodo esté Autorizada.
        // Por eso también consideramos autorizado el periodo del empleado.
        var periodoAutorizado = resolucionPorEmpleado.GetValueOrDefault(resumen.EmpleadoId) is { Estatus: RrhhResolucionPeriodoEstatus.Autorizada };
        var extraResuelto = (asistencia != null && RrhhTiempoExtraPolicy.TieneResolucionTiempoAplicada(asistencia)) || periodoAutorizado;

        var cssClass = ObtenerClaseCelda(ObtenerEstadoCelda(
            minutosExtra, extraResuelto, acreditado, debido, delta, descuentoDia));

        var deltaTexto = delta == 0
            ? "En objetivo"
            : delta > 0
                ? $"+{FormatearMinutos(delta)}"
                : $"-{FormatearMinutos(Math.Abs(delta))}";

        if (minutosExtra > 0)
        {
            // Bajo el flujo semanal, el aprobado por día es 0 (la autorización está en el
            // periodo). Si el periodo está autorizado, mostramos el extra detectado del día
            // como "revisado" para no pintar "Extra rev. 0" en verde.
            var minutosExtraRevisado = minutosExtraAprobados > 0 ? minutosExtraAprobados : minutosExtra;
            deltaTexto = extraResuelto
                ? $"Extra rev. {FormatearMinutos(minutosExtraRevisado)}"
                : $"Extra pend. {FormatearMinutos(minutosExtra)}";
        }

        var acreditadoTexto = acreditado > 0 ? FormatearMinutos(acreditado) : "—";
        var debidoTexto = debido > 0 ? FormatearMinutos(debido) : "—";
        var diferenciaTexto = delta == 0
            ? "En objetivo"
            : delta > 0
                ? $"+{FormatearMinutos(delta)}"
                : $"-{FormatearMinutos(Math.Abs(delta))}";

        // Tiempo bruto del día: sólo se pasa al abrir el detalle del empleado
        // (brutoPorDia), donde se calcula bajo demanda desde las marcaciones.
        var brutoDia = brutoPorDia is null ? 0 : brutoPorDia.GetValueOrDefault(dia);
        var brutoTexto = brutoDia > 0 ? FormatearMinutos(brutoDia) : "—";
        var tooltipBruto = brutoDia > 0
            ? $" | Tiempo reloj (marcaciones): {brutoTexto}"
            : string.Empty;

        var tooltipExtra = minutosExtra > 0
            ? $" | Extra detectado: {FormatearMinutos(minutosExtra)} | Extra aprobado: {FormatearMinutos(minutosExtraAprobados)}"
            : string.Empty;

        // Bajo las nuevas reglas, los descansos SIEMPRE se descuentan (no se
        // compensan con salida anticipada). "Acreditado" ya los excluye; lo
        // explicitamos para que el delta sea interpretable.
        var descansosDescontados = asistencia == null ? 0 : RrhhTiempoExtraPolicy.ObtenerMinutosDescansoNoPagadoProgramado(asistencia);
        var tooltipDescuentos = (descansosDescontados > 0 || descuentoDia > 0)
            ? $" | Descansos descontados: {FormatearMinutos(descansosDescontados)} | Descuento: {FormatearMinutos(descuentoDia)}"
            : string.Empty;

        var fechaTexto = dia.ToString("dddd, dd/MM/yyyy", CulturaEsMx);
        var tooltip = $"{fechaTexto}{tooltipBruto} | Acreditado (sin descansos): {acreditadoTexto} | Debe: {debidoTexto} | Diferencia: {diferenciaTexto}{tooltipDescuentos}{tooltipExtra}";

        return new PresentacionCeldaSemanal(
            acreditadoTexto,
            debidoTexto,
            deltaTexto,
            cssClass,
            tooltip);
    }

    private Task AbrirCorreccionDiaAsync(Guid empleadoId, DateOnly fecha)
    {
        if (asistenciasPorEmpleadoDia.TryGetValue(CrearClaveAsistencia(empleadoId, fecha), out var asistencia))
        {
            asistenciaSeleccionada = asistencia;
            mostrarCorreccionDia = true;
        }

        return Task.CompletedTask;
    }

    // ¿El periodo del empleado del día seleccionado está Autorizada? Si sí, el modal
    // del día se abre en sólo-lectura (no se puede editar sin reabrir el periodo).
    private bool PeriodoSeleccionAutorizado =>
        asistenciaSeleccionada != null
        && resolucionPorEmpleado.GetValueOrDefault(asistenciaSeleccionada.EmpleadoId) is { Estatus: RrhhResolucionPeriodoEstatus.Autorizada };

    // Navegación entre días del periodo desde las flechas del modal. El modal no
    // sabe del periodo: sólo pide "llévame a esta fecha". El padre resuelve la
    // asistencia del MISMO empleado en el destino (el modal queda abierto y se
    // recarga al cambiar Asistencia.Id). Si el empleado no tiene asistencia ese
    // día (día dentro del periodo sin registro), se queda en el día actual sin
    // navegar. El clamping a los bordes del periodo lo hace el modal con
    // FechaMinimaPeriodo/FechaMaximaPeriodo.
    private Task CambiarDiaCorreccionAsync(DateOnly destino)
    {
        if (asistenciaSeleccionada == null)
        {
            return Task.CompletedTask;
        }

        if (asistenciasPorEmpleadoDia.TryGetValue(CrearClaveAsistencia(asistenciaSeleccionada.EmpleadoId, destino), out var asistencia))
        {
            asistenciaSeleccionada = asistencia;
            mostrarCorreccionDia = true;
        }

        return Task.CompletedTask;
    }

    private void CerrarCorreccionDia()
    {
        mostrarCorreccionDia = false;
        asistenciaSeleccionada = null;
    }

    private Task RecargarResumenSemanalAsync()
        => CargarAsync();

    private Task AbrirResolucionAsync(ResumenSemanalEmpleado resumen)
    {
        _resolucionEmpleadoId = resumen.EmpleadoId;
        _resolucionNombre = resumen.NombreCompleto;
        _resolucionMinutosExtra = resumen.TotalMinutosExtra;
        _resolucionSaldoBanco = resumen.SaldoBancoHoras;
        _resolucionVisible = true;
        return Task.CompletedTask;
    }

    private Task CerrarResolucion()
    {
        _resolucionVisible = false;
        _resolucionEmpleadoId = Guid.Empty;
        return Task.CompletedTask;
    }

    // F9 — "aceptar tiempos": empleados con extra detectado cuya resolución del
    // periodo NO está Autorizada. Aceptarlos (pagar o sólo cerrar) es lo que le da
    // el go al cálculo de nómina (gate de Fase 7 en Nominas.razor — fusión prenómina→nómina).
    // English: F9 — "accept times": employees with detected overtime whose period resolution
    // is NOT Authorized. Accepting them (pay or just close) is what unblocks the nómina
    // calculation (Phase 7 gate in Nominas.razor — prenómina→nómina fusion).
    private List<ResumenSemanalEmpleado> ObtenerPendientesAceptacion()
        => resumenes
            .Where(r => r.TotalMinutosExtra > 0
                && resolucionPorEmpleado.GetValueOrDefault(r.EmpleadoId)?.Estatus != RrhhResolucionPeriodoEstatus.Autorizada)
            .ToList();

    private int EmpleadosConExtraCount => resumenes.Count(r => r.TotalMinutosExtra > 0);

    private int AceptadosCount => resumenes.Count(r => r.TotalMinutosExtra > 0
        && resolucionPorEmpleado.GetValueOrDefault(r.EmpleadoId)?.Estatus == RrhhResolucionPeriodoEstatus.Autorizada);

    private int PendientesAceptacionCount => ObtenerPendientesAceptacion().Count;

    private void AbrirConfirmacionLote(bool pagarTodo)
    {
        _lotePagarTodo = pagarTodo;
        _mostrarConfirmacionLote = true;
        error = null;
        ok = null;
    }

    private void CancelarLote() => _mostrarConfirmacionLote = false;

    private async Task ConfirmarLoteAsync()
    {
        var pendientes = ObtenerPendientesAceptacion();
        if (pendientes.Count == 0)
        {
            _mostrarConfirmacionLote = false;
            ok = "No hay empleados con tiempo extra pendiente de aceptar.";
            return;
        }

        cargando = true;
        error = null;
        ok = null;
        var fechaRef = DateOnly.FromDateTime(fechaReferenciaPeriodo);
        var aplicados = 0;
        var omitidos = 0;
        string? primerError = null;
        try
        {
            foreach (var emp in pendientes)
            {
                await using var db = await DbFactory.CreateDbContextAsync();
                var minutosPago = 0;
                if (_lotePagarTodo)
                {
                    // PagarTodo = todo el extra absorbible (tras neteo) al factor de config.
                    // Usar el rango del periodo en pantalla (igual que el preview individual)
                    // para que el cálculo y la autorización correspondan a la semana mostrada.
                    // English: PagarTodo = all absorbible extra (after netting) at config factor.
                    // Use the on-screen period range (same as the individual preview) so the
                    // calculation and the authorization match the displayed week.
                    var resumen = await ResolucionPeriodo.ObtenerResumenPeriodoAsync(db, _empresaId, emp.EmpleadoId, periodoFechaInicio, periodoFechaFin);
                    minutosPago = Math.Max(0, resumen.MinutosExtraAbsorbible);
                }

                try
                {
                    await ResolucionPeriodo.AplicarResolucionPeriodoAsync(db, new RrhhResolucionPeriodoCommand
                    {
                        EmpresaId = _empresaId,
                        EmpleadoId = emp.EmpleadoId,
                        FechaReferencia = fechaRef,
                        // Rango explícito del periodo en pantalla: el apply resuelve sobre este
                        // rango, igual que el preview, evitando autorizar otra semana.
                        // English: Explicit on-screen period range: the apply resolves over this
                        // range, same as the preview, avoiding authorizing a different week.
                        FechaInicioPeriodo = periodoFechaInicio,
                        FechaFinPeriodo = periodoFechaFin,
                        Resolucion = _lotePagarTodo ? "PagarTodo" : "Descartado",
                        MinutosBasePago = minutosPago,
                        MinutosBaseBanco = 0,
                        // "Aceptar sin pagar" = DESCARTAR el extra: no se compensa, no
                        // se paga. El faltante/retardo del periodo se descuenta COMPLETO
                        // (el neteo queda anulado → absorbidos=0). El periodo sí queda
                        // Autorizada, así que desbloquea el gate de prenómina.
                        DescartarExtra = !_lotePagarTodo,
                        Observaciones = _lotePagarTodo
                            ? "Aceptado en lote — pagar todo al factor de configuración."
                            : "Aceptado en lote sin pagar — extra descartado (descuento completo, sin compensación).",
                        UsuarioActual = _usuarioActual,
                        Lineas = Array.Empty<RrhhResolucionPeriodoLineaCommand>()
                    });
                    await db.SaveChangesAsync();
                    aplicados++;
                }
                catch (Exception ex)
                {
                    omitidos++;
                    primerError ??= ex.InnerException?.Message ?? ex.Message;
                }
            }

            _mostrarConfirmacionLote = false;
            await CargarAsync();
            ok = _lotePagarTodo
                ? $"Tiempo aceptado y pagado para {aplicados} empleado(s) del periodo."
                : $"Tiempo aceptado sin pagar para {aplicados} empleado(s) del periodo.";
            if (omitidos > 0)
            {
                ok += $" {omitidos} omitido(s) por error.";
                error = primerError;
            }
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

    private async Task AbrirDetalleAsync(Guid empleadoId)
    {
        _detalleEmpleadoId = empleadoId;
        _detalleVisible = true;
        await CargarBrutoDetalleAsync(empleadoId);
        await CargarResumenDetalleAsync(empleadoId);
    }

    // Carga el resumen de nómina del periodo (mismo DTO que el modal de "Aceptar
    // tiempo") para pintar la tablita en el drawer con los mismos números que ve
    // el cliente en el modal. Incluye la resolución persistida (pago/banco autorizado).
    // English: Loads the period payroll summary (same DTO as the "Accept time" modal)
    // to render the little table in the drawer with the same numbers the client sees in
    // the modal. Includes the persisted resolution (authorized pay/bank).
    private async Task CargarResumenDetalleAsync(Guid empleadoId)
    {
        _detalleResumen = null;
        _detalleResolucion = null;
        if (empleadoId == Guid.Empty || _empresaId == Guid.Empty)
        {
            return;
        }

        try
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            _detalleResumen = await ResolucionPeriodo.ObtenerResumenPeriodoAsync(
                db, _empresaId, empleadoId, periodoFechaInicio, periodoFechaFin);
            if (_detalleResumen is null)
            {
                return;
            }
            _detalleResolucion = await db.RrhhResolucionesTiempoExtraPeriodo
                .AsNoTracking()
                .Include(r => r.Lineas)
                .FirstOrDefaultAsync(r => r.EmpresaId == _empresaId
                    && r.EmpleadoId == empleadoId
                    && r.PeriodicidadPago == _detalleResumen.PeriodicidadPago
                    && r.AnioPeriodo == _detalleResumen.AnioPeriodo
                    && r.NumeroPeriodo == _detalleResumen.NumeroPeriodo);
        }
        catch
        {
            // Si falla, el drawer sigue mostrando la grilla de días; la tablita queda en 0.
            _detalleResumen = null;
            _detalleResolucion = null;
        }
    }

    // Bruto del empleado del drawer, por día y total, calculado bajo demanda
    // desde sus marcaciones del periodo (solo las de este empleado). Se calcula
    // al abrir el detalle y se guarda en _detalleBrutoPorDia/_detalleTotalBruto.
    private async Task CargarBrutoDetalleAsync(Guid empleadoId)
    {
        _detalleBrutoPorDia = new();
        _detalleTotalBruto = 0;
        if (empleadoId == Guid.Empty || diasPeriodo.Count == 0)
        {
            return;
        }

        try
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            var (desde, hasta) = ObtenerPeriodoActual();
            var desdeUtc = DateTime.SpecifyKind(desde.ToDateTime(TimeOnly.MinValue).AddHours(-14), DateTimeKind.Utc);
            var hastaUtc = DateTime.SpecifyKind(hasta.AddDays(1).ToDateTime(TimeOnly.MinValue).AddHours(14), DateTimeKind.Utc);

            var marcaciones = await db.RrhhMarcaciones
                .AsNoTracking()
                .Include(m => m.Checador)
                .Where(m => m.EmpleadoId == empleadoId
                    && !m.EsAnulada
                    && m.FechaHoraMarcacionUtc >= desdeUtc
                    && m.FechaHoraMarcacionUtc < hastaUtc)
                .ToListAsync();

            var porDia = marcaciones
                .Select(ObtenerFechaHoraLocalMarcacion)
                .Where(t => t.Date >= desde.ToDateTime(TimeOnly.MinValue) && t.Date <= hasta.ToDateTime(new TimeOnly(23, 59, 59)))
                .GroupBy(t => DateOnly.FromDateTime(t.Date))
                .ToDictionary(g => g.Key, g => CalcularMinutosBrutosTrabajados(g.OrderBy(x => x).ToList()));

            _detalleBrutoPorDia = diasPeriodo.ToDictionary(d => d, d => porDia.GetValueOrDefault(d));
            _detalleTotalBruto = _detalleBrutoPorDia.Values.Sum();
        }
        catch
        {
            // Si falla la carga del bruto, el drawer sigue mostrando el resto.
            _detalleBrutoPorDia = new();
            _detalleTotalBruto = 0;
        }
    }

    private void CerrarDetalle()
    {
        _detalleVisible = false;
        _detalleEmpleadoId = Guid.Empty;
        _detalleBrutoPorDia = new();
        _detalleTotalBruto = 0;
        _detalleResumen = null;
        _detalleResolucion = null;
    }

    // Recalcular por periodo de UN solo empleado desde el drawer de detalle: pregunta bajo qué
    // método (default del empleado / Vs horario / Marcaje de reloj) y luego impone ese método
    // sobre TODOS los días del periodo visible, pisando los ajustes manuales por día. Es el
    // mismo "Recalcular por periodo" de Asistencias.razor pero acotado al empleado abierto y
    // disparado desde "Ver detalle".
    // English: Period reprocess for ONE employee from the detail drawer: asks under which
    // method (employee default / Vs schedule / Clock punch) and then enforces it over EVERY
    // day of the visible period, overriding per-day manual tweaks. Same as the "Recalculate
    // by period" button in Asistencias.razor but scoped to the open employee.
    private bool _recalcPeriodoVisible;
    private Guid _recalcEmpleadoId;

    private void AbrirRecalcPeriodo(Guid empleadoId)
    {
        _recalcEmpleadoId = empleadoId;
        _recalcPeriodoVisible = true;
    }

    private void CerrarRecalcPeriodo()
    {
        _recalcPeriodoVisible = false;
        _recalcEmpleadoId = Guid.Empty;
    }

    private async Task ConfirmarRecalcPeriodoAsync(RecalcularPeriodoOpcion opcion)
    {
        var empleadoId = _recalcEmpleadoId;
        _recalcPeriodoVisible = false;
        await EjecutarRecalcPeriodoAsync(empleadoId, opcion);
    }

    // Traduce la opción del UI a los parámetros del processor y ejecuta el reproceso.
    // English: Translates the UI option to the processor params and runs the reprocess.
    private static (bool forzarDefaultEmpleado, RrhhModoCalculoForzado? modoForzado, string etiqueta) TraducirOpcion(RecalcularPeriodoOpcion opcion)
        => opcion switch
        {
            RecalcularPeriodoOpcion.DefaultEmpleado => (true, null, "default del empleado"),
            RecalcularPeriodoOpcion.VsHorario => (false, RrhhModoCalculoForzado.VsHorario, "Vs horario (con reglas)"),
            RecalcularPeriodoOpcion.MarcajeReloj => (false, RrhhModoCalculoForzado.MarcajeReloj, "marcaje de reloj (tal cual)"),
            _ => (true, null, "default del empleado")
        };

    private async Task EjecutarRecalcPeriodoAsync(Guid empleadoId, RecalcularPeriodoOpcion opcion)
    {
        if (!_puedeReprocesar || _empresaId == Guid.Empty || empleadoId == Guid.Empty || diasPeriodo.Count == 0)
        {
            return;
        }

        var (forzarDefaultEmpleado, modoForzado, etiquetaMetodo) = TraducirOpcion(opcion);
        cargando = true;
        _recalcPeriodo = true;
        error = null;
        ok = null;
        try
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            var grupos = await RrhhAsistenciaProcessor.ReprocesarRangoAsync(
                db, _empresaId, periodoFechaInicio, periodoFechaFin, empleadoId, forzarDefaultEmpleado, modoForzado);

            db.RrhhLogsChecador.Add(new RrhhLogChecador
            {
                Id = Guid.NewGuid(),
                EmpresaId = _empresaId,
                FechaUtc = DateTime.UtcNow,
                Nivel = "Information",
                Mensaje = "Se ejecutó un reproceso de periodo de un empleado desde el resumen semanal.",
                Detalle = $"usuario={_usuarioActual};desde={periodoFechaInicio:yyyy-MM-dd};hasta={periodoFechaFin:yyyy-MM-dd};empleado={empleadoId};grupos={grupos};metodo={etiquetaMetodo}",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });
            await db.SaveChangesAsync();

            await CargarAsync();
            // CargarAsync deja cargando=false en su finally; lo mantenemos activo hasta
            // refrescar los datos del drawer abierto (bruto + resumen) para que el botón no
            // parpadee habilitado entre el reload de la grilla y el del drawer.
            cargando = true;
            await CargarBrutoDetalleAsync(empleadoId);
            await CargarResumenDetalleAsync(empleadoId);

            if (error is null)
            {
                ok = $"Periodo recalculado: {grupos} día(s) reprocesado(s) con método {etiquetaMetodo}.";
            }
        }
        catch (Exception ex)
        {
            error = ex.InnerException?.Message ?? ex.Message;
        }
        finally
        {
            cargando = false;
            _recalcPeriodo = false;
        }
    }

    // Resumen del empleado actualmente abierto en el drawer (null si no hay).
    private ResumenSemanalEmpleado? DetalleResumen
        => _detalleEmpleadoId == Guid.Empty
            ? null
            : resumenes.FirstOrDefault(r => r.EmpleadoId == _detalleEmpleadoId);

    // Presentación del extra A PAGO en el drawer según el ciclo de vida del
    // periodo: pagado en nómina (verde) → aprobado por pagar (azul) → pendiente
    // de autorizar (gris). El valor siempre es TotalMinutosExtraAprobadoPago
    // (0 mientras no se autoriza); el color/etiqueta comunica el estado.
    private record PresentacionExtraPago(string Etiqueta, string CssClass, string Tooltip);

    private PresentacionExtraPago ObtenerPresentacionExtraPago(ResumenSemanalEmpleado detalle)
    {
        if (empleadosPagadosPeriodo.Contains(detalle.EmpleadoId))
        {
            return new PresentacionExtraPago(
                "Extra pagado",
                "asis-detail-card--good",
                "Extra aprobado a pago ya pagado en la nómina de este periodo.");
        }

        if (resolucionPorEmpleado.GetValueOrDefault(detalle.EmpleadoId) is { Estatus: RrhhResolucionPeriodoEstatus.Autorizada })
        {
            return new PresentacionExtraPago(
                "Extra por pagar",
                "asis-detail-card--primary",
                "Extra aprobado a pago. Se paga al correr la nómina del periodo.");
        }

        return new PresentacionExtraPago(
            "Extra por pagar",
            "asis-detail-card--info",
            "Aún no se autoriza el extra del periodo. Apruébalo con «Aceptar tiempo».");
    }

    private async Task AlCambiarPeriodicidadAsync()
    {
        fechaReferenciaPeriodo = DateTime.Today;
        await GuardarFiltrosPersistidosAsync();
        await CargarAsync();
    }

    private async Task IrAPeriodoActualAsync()
    {
        fechaReferenciaPeriodo = DateTime.Today;
        await GuardarFiltrosPersistidosAsync();
        await CargarAsync();
    }

    private static string ObtenerEtiquetaEstatusResolucion(RrhhResolucionTiempoExtraPeriodo? resolucion)
        => resolucion switch
        {
            null => "Pendiente de autorizar",
            { Estatus: RrhhResolucionPeriodoEstatus.Autorizada, ExtraDescartado: true } => "Autorizada (descartado)",
            { Estatus: RrhhResolucionPeriodoEstatus.Autorizada } => "Autorizada",
            { Estatus: RrhhResolucionPeriodoEstatus.Reabierta } => "Reabierta",
            _ => "Pendiente"
        };

    private static string ObtenerClaseEstatusResolucion(RrhhResolucionTiempoExtraPeriodo? resolucion)
        => resolucion switch
        {
            null => "bg-warning text-dark",
            { Estatus: RrhhResolucionPeriodoEstatus.Autorizada } => "bg-success",
            { Estatus: RrhhResolucionPeriodoEstatus.Reabierta } => "bg-warning text-dark",
            _ => "bg-warning text-dark"
        };

    private async Task<Dictionary<string, int>> ConstruirPermisosPorEmpleadoAsync(CrmDbContext db, IReadOnlyCollection<RrhhAsistencia> asistencias, DateOnly desde, DateOnly hasta, bool conGoce = true)
    {
        if (asistencias.Count == 0)
        {
            return new Dictionary<string, int>();
        }

        var empleadoIds = asistencias.Select(a => a.EmpleadoId).Distinct().ToList();
        var query = db.RrhhAusencias
            .AsNoTracking()
            .Where(a => a.EmpresaId == _empresaId
                && empleadoIds.Contains(a.EmpleadoId)
                && a.IsActive
                && a.ConGocePago == conGoce
                && a.Tipo == TipoAusenciaRrhh.Permiso
                && (a.Estatus == EstatusAusenciaRrhh.Aprobada || a.Estatus == EstatusAusenciaRrhh.Aplicada)
                && a.FechaInicio <= hasta
                && a.FechaFin >= desde);

        var permisos = await query.ToListAsync();

        return permisos
            .GroupBy(a => a.EmpleadoId.ToString("N"))
            .ToDictionary(
                g => g.Key,
                g => g.Sum(a => (int)Math.Round(Math.Max(0m, a.Horas) * 60m, MidpointRounding.AwayFromZero)));
    }

    private async Task<Dictionary<string, int>> ConstruirPermisosPorDiaAsync(CrmDbContext db, IReadOnlyCollection<RrhhAsistencia> asistencias, DateOnly desde, DateOnly hasta)
    {
        if (asistencias.Count == 0)
        {
            return new Dictionary<string, int>();
        }

        var empleadoIds = asistencias.Select(a => a.EmpleadoId).Distinct().ToList();
        var permisos = await db.RrhhAusencias
            .AsNoTracking()
            .Where(a => a.EmpresaId == _empresaId
                && empleadoIds.Contains(a.EmpleadoId)
                && a.IsActive
                && a.ConGocePago // cualquier ausencia con goce y horas compensa tiempo esperado
                && a.Horas > 0
                && (a.Estatus == EstatusAusenciaRrhh.Aprobada || a.Estatus == EstatusAusenciaRrhh.Aplicada)
                && a.FechaInicio <= hasta
                && a.FechaFin >= desde)
            .ToListAsync();

        var resultado = new Dictionary<string, int>();
        foreach (var permiso in permisos)
        {
            var minutosPorDia = RrhhTiempoExtraPolicy.ObtenerMinutosPermisoConGocePorDia(permiso);
            var inicio = permiso.FechaInicio < desde ? desde : permiso.FechaInicio;
            var fin = permiso.FechaFin > hasta ? hasta : permiso.FechaFin;
            for (var fecha = inicio; fecha <= fin; fecha = fecha.AddDays(1))
            {
                var clave = CrearClaveDia(permiso.EmpleadoId, fecha);
                resultado[clave] = resultado.GetValueOrDefault(clave) + minutosPorDia;
            }
        }

        return resultado;
    }

    private bool EsFestivoTrabajado(DateOnly fecha)
        => festivosPeriodo.Contains(fecha);

    private string ObtenerTooltipRetardo(ResumenSemanalEmpleado resumen)
    {
        var dias = resumen.MinutosRetardoPorDia
            .Where(x => x.Value > 0)
            .OrderBy(x => x.Key)
            .Select(x => $"{x.Key:dd/MM}: {x.Value} min")
            .ToList();

        return dias.Count == 0
            ? "Sin retardo en el período."
            : $"Retardo calculado con: {string.Join(" | ", dias)}";
    }

    private string ObtenerTooltipDescuento(ResumenSemanalEmpleado resumen)
    {
        var dias = resumen.MinutosDescuentoPorDia
            .Where(x => x.Value > 0)
            .OrderBy(x => x.Key)
            .Select(x => $"{x.Key:dd/MM}: {x.Value} min")
            .ToList();

        return dias.Count == 0
            ? "Sin descuento efectivo en el período."
            : $"Descuento efectivo calculado con: {string.Join(" | ", dias)}";
    }

    private string ObtenerTooltipSalidaAnticipada(ResumenSemanalEmpleado resumen)
    {
        var dias = resumen.MinutosSalidaAnticipadaPorDia
            .Where(x => x.Value > 0)
            .OrderBy(x => x.Key)
            .Select(x => $"{x.Key:dd/MM}: {x.Value} min")
            .ToList();

        return dias.Count == 0
            ? "Sin salida anticipada en el período."
            : $"Salida anticipada calculada con: {string.Join(" | ", dias)}";
    }

    private string ObtenerTooltipDiasTrabajados(ResumenSemanalEmpleado resumen)
    {
        var dias = resumen.DiasTrabajadosDetalle
            .OrderBy(x => x)
            .Select(x => x.ToString("dd/MM"))
            .ToList();

        return dias.Count == 0
            ? "Sin días trabajados en el período."
            : $"Días trabajados: {string.Join(", ", dias)}";
    }

    private string ObtenerTooltipFestivoTrabajado(ResumenSemanalEmpleado resumen)
    {
        var dias = resumen.DiasFestivoTrabajadoDetalle
            .OrderBy(x => x)
            .Select(x => x.ToString("dd/MM"))
            .ToList();

        return dias.Count == 0
            ? "Sin festivos trabajados en el período."
            : $"Festivos trabajados: {string.Join(", ", dias)}";
    }

    private static string ObtenerClaseMetricaRetardo(int minutos)
        => minutos > 0 ? "asis-week-metric--retardo" : string.Empty;

    private static string ObtenerClaseMetricaDescuento(int minutos)
        => minutos > 0 ? "asis-week-metric--retardo" : string.Empty;

    private static string ObtenerClaseMetricaSalidaAnticipada(int minutos)
        => minutos > 0 ? "asis-week-metric--salida" : string.Empty;

    private sealed record ResumenSemanalEmpleado
    {
        public Guid EmpleadoId { get; init; }
        public string NombreCompleto { get; init; } = string.Empty;
        public string? NumeroEmpleado { get; init; }
        public string? CodigoChecador { get; init; }
        public bool EsPorHoras { get; init; }
        public int TotalMinutosNormales { get; init; }
        public int TotalMinutosExtra { get; init; }
        public int TotalMinutosExtraAprobadoPago { get; init; }
        public int TotalMinutosExtraAprobadoBanco { get; init; }
        public int TotalMinutosBancoAplicado { get; init; }
        public int TotalMinutosFaltante { get; init; }
        public int TotalMinutosDescuento { get; init; }
        public int TotalMinutosRetardo { get; init; }
        public int TotalMinutosSalidaAnticipada { get; init; }
        public int TotalDiasTrabajados { get; init; }
        public int TotalDiasFestivoTrabajado { get; init; }
        public int TotalMinutosPermisoConGoce { get; init; }
        public int TotalMinutosPermisoSinGoce { get; init; }
        public int TotalMinutosCompensacion { get; init; }
        public int TotalDiasAusentismo { get; init; }
        public int TotalDiasRetardo { get; init; }
        public int TotalDiasIncompleto { get; init; }
        public int TotalIncidencias => TotalDiasAusentismo + TotalDiasRetardo + TotalDiasIncompleto;
        public int SaldoBancoHoras { get; init; }
        public Dictionary<DateOnly, int> MinutosDescuentoPorDia { get; init; } = new();
        public Dictionary<DateOnly, int> MinutosRetardoPorDia { get; init; } = new();
        public Dictionary<DateOnly, int> MinutosSalidaAnticipadaPorDia { get; init; } = new();
        public List<DateOnly> DiasTrabajadosDetalle { get; init; } = [];
        public List<DateOnly> DiasFestivoTrabajadoDetalle { get; init; } = [];
        public Dictionary<DateOnly, int> MinutosPorDia { get; init; } = new();
        public Dictionary<DateOnly, int> MinutosDebidosPorDia { get; init; } = new();
        public int TotalMinutosPagados => TotalMinutosNormales + TotalMinutosPermisoConGoce + TotalMinutosCompensacion + TotalMinutosBancoAplicado;
        // Extra a pago (minutos base, sin factorar) desde la resolución del periodo.
        public int TotalMinutosExtraPagoBase { get; init; }
        // Extra a banco (minutos base) desde la resolución del periodo.
        public int TotalMinutosExtraBancoBase { get; init; }
        // Etiqueta legible del detalle de factores del extra a pago (ej. "×2.0", "1h×2 + 1h×1").
        public string ExtraPagoFactorDetalle { get; init; } = string.Empty;
        // Etiqueta legible del detalle de factores del extra a banco (ej. "×1.0").
        public string ExtraBancoFactorDetalle { get; init; } = string.Empty;
        // Si el periodo está autorizado (tiene resolución).
        public bool PeriodoAutorizado { get; init; }
        // Si el extra fue descartado.
        public bool ExtraDescartado { get; init; }
        // Ajuste al TotalMinutos para reflejar el extra aprobado por periodo que no
        // está en los campos diarios (MinutosExtraAutorizados*). El resumen diario usa
        // ObtenerMinutosExtraAprobados(asistencia) que lee de la asistencia; el flujo de
        // periodo NO escribe esos campos, así que el extra aprobado por periodo se
        // suma aquí para que "Acreditado" lo refleje.
        public int TotalMinutosAdjustment { get; init; }
        public int TotalMinutos => MinutosPorDia.Values.Sum() + TotalMinutosAdjustment;
    }

    private sealed record PresentacionCeldaSemanal(string Acreditado, string Debido, string Delta, string CssClass, string Tooltip);

    /// <summary>
    /// Construye una etiqueta legible del detalle de factores para un destino (Pago/Banco)
    /// desde las líneas de la resolución del periodo (Fase 8). Si no hay líneas, usa el
    /// factor escalar legado.
    /// </summary>
    private static string ConstruirFactorDetalle(RrhhResolucionTiempoExtraPeriodo resolucion, RrhhDestinoTiempoExtraLinea destino)
    {
        var lineas = resolucion.Lineas?
            .Where(l => l.Destino == destino && l.Minutos > 0)
            .OrderBy(l => l.Orden)
            .ToList() ?? [];

        if (lineas.Count == 0)
        {
            // Path legado (sin líneas): usar factor escalar
            if (destino == RrhhDestinoTiempoExtraLinea.Pago)
            {
                var factor = resolucion.FactorTiempoExtraAplicado ?? 2m;
                return $"×{factor:0.#}";
            }
            else
            {
                var factor = resolucion.FactorAcumulacionBancoHorasAplicado ?? 1m;
                return $"×{factor:0.#}";
            }
        }

        // Fase 8: construir detalle desde líneas
        var partes = new List<string>();
        foreach (var linea in lineas)
        {
            var horas = linea.Minutos / 60m;
            if (linea.Factor == 1m)
            {
                partes.Add($"{horas:0.#}h");
            }
            else
            {
                partes.Add($"{horas:0.#}h×{linea.Factor:0.#}");
            }
        }

        return string.Join(" + ", partes);
    }
}
