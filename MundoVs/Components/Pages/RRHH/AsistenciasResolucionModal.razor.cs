using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Components.Pages.RRHH;

// Modal aparte para resolver el tiempo extra del PERIODO de nómina de un empleado.
// Reemplaza la fila inline que antes se abría debajo de la fila de la tabla semanal.
// La detección sigue siendo diaria (RrhhAsistencia.MinutosExtra); la liquidación
// (pago / banco) se autoriza por periodo en una sola decisión, vía
// IRrhhResolucionPeriodoService (Fase 2/3/4/8: neteo faltante→retardo→banco + líneas).
public partial class AsistenciasResolucionModal : ComponentBase
{
    [Inject] private IDbContextFactory<CrmDbContext> DbFactory { get; set; } = default!;
    [Inject] private IRrhhResolucionPeriodoService ResolucionPeriodo { get; set; } = default!;
    [Inject] private IRrhhPermisoPorDiferenciaService PermisoPorDiferencia { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    [Parameter] public bool Visible { get; set; }
    [Parameter] public Guid EmpleadoId { get; set; }
    [Parameter] public string NombreEmpleado { get; set; } = string.Empty;
    [Parameter] public DateOnly FechaReferencia { get; set; }
    [Parameter] public DateOnly? FechaInicioPeriodo { get; set; }
    [Parameter] public DateOnly? FechaFinPeriodo { get; set; }
    [Parameter] public int MinutosExtraDetectado { get; set; }
    [Parameter] public int SaldoBancoHoras { get; set; }
    [Parameter] public bool PuedeAprobarTiempoExtra { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnUpdated { get; set; }

    private Guid _empresaId;
    private string _usuarioActual = string.Empty;
    private Guid _ultimoEmpleadoCargado;
    private DateOnly? _ultimoInicioPeriodo;
    private DateOnly? _ultimoFinPeriodo;
    private bool _cargandoDatos;
    private bool cargando;
    private string? error;
    private string? ok;

    private RrhhResolucionTiempoExtraPeriodo? _resolucion;
    private RrhhResolucionPeriodoResumen? _resumenPeriodo;
    private EdicionResolucionPeriodo _edicion = new();

    // Permiso por diferencia (sintético, F2/F3/F4): se activa cuando
    // |retardoDetectado − extraDetectado| > 0 al cierre del periodo.
    private PermisoDiferenciaSugerencia? _sugerenciaDiferencia;
    private EdicionPermisoDiferencia _edicionPermisoDiferencia = new();

    // Turno asignado al empleado para el periodo (cargado para mostrarlo en el encabezado
    // del modal y dar contexto al operador: jornada esperada vs. tiempo trabajado/extra).
    // English: Employee's assigned shift for the period (loaded to show it in the modal
    // header so the operator has context: expected jornada vs. worked/extra time).
    private string _turnoEtiqueta = string.Empty;
    private string _turnoHorario = string.Empty;

    /// <summary>
    /// Estado de la UI para los 3 permisos por diferencia (Banco / ConGoce / SinGoce).
    /// Se empaqueta en <see cref="PermisoDiferenciaInput"/> al autorizar.
    /// </summary>
    private sealed class EdicionPermisoDiferencia
    {
        public int BancoMinutos { get; set; }
        public decimal BancoFactor { get; set; } = 1m;
        public string? BancoObservaciones { get; set; }

        public int ConGoceMinutos { get; set; }
        public decimal ConGoceFactor { get; set; } = 1m;
        public string? ConGoceObservaciones { get; set; }

        public int SinGoceMinutos { get; set; }
        public decimal SinGoceFactor { get; set; } = 1m;
        public string? SinGoceObservaciones { get; set; }

        public int TotalMinutos => Math.Max(0, BancoMinutos) + Math.Max(0, ConGoceMinutos) + Math.Max(0, SinGoceMinutos);
    }

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthStateProvider.GetAuthenticationStateAsync();
        _ = Guid.TryParse(state.User.FindFirst("EmpresaId")?.Value, out _empresaId);
        _usuarioActual = state.User.Identity?.Name ?? string.Empty;
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!Visible || EmpleadoId == Guid.Empty || _empresaId == Guid.Empty)
        {
            return;
        }

        // Recargar sólo cuando cambia el empleado o el rango de fechas (evita
        // recargas redundantes por re-render del padre).
        var claveCarga = (EmpleadoId, FechaInicioPeriodo, FechaFinPeriodo);
        if (_ultimoEmpleadoCargado == EmpleadoId && _resumenPeriodo is not null
            && _ultimoInicioPeriodo == FechaInicioPeriodo && _ultimoFinPeriodo == FechaFinPeriodo)
        {
            return;
        }

        _ultimoEmpleadoCargado = EmpleadoId;
        _ultimoInicioPeriodo = FechaInicioPeriodo;
        _ultimoFinPeriodo = FechaFinPeriodo;
        error = null;
        ok = null;
        await CargarDatosAsync();
    }

    private async Task CargarDatosAsync()
    {
        _cargandoDatos = true;
        try
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            _resumenPeriodo = FechaInicioPeriodo.HasValue && FechaFinPeriodo.HasValue
                ? await ResolucionPeriodo.ObtenerResumenPeriodoAsync(
                    db, _empresaId, EmpleadoId, FechaInicioPeriodo.Value, FechaFinPeriodo.Value)
                : await ResolucionPeriodo.ObtenerResumenPeriodoAsync(
                    db, _empresaId, EmpleadoId, FechaReferencia);

            _resolucion = await db.RrhhResolucionesTiempoExtraPeriodo
                .AsNoTracking()
                .Include(r => r.Lineas)
                .FirstOrDefaultAsync(r => r.EmpresaId == _empresaId
                    && r.EmpleadoId == EmpleadoId
                    && r.PeriodicidadPago == _resumenPeriodo.PeriodicidadPago
                    && r.AnioPeriodo == _resumenPeriodo.AnioPeriodo
                    && r.NumeroPeriodo == _resumenPeriodo.NumeroPeriodo);

            _edicion = CrearEdicionInicial(_resolucion, _resumenPeriodo);

            // Cargar el turno asignado al empleado (vigente, sincronizado en Empleado.TurnoBase)
            // para mostrarlo en el encabezado. Incluye los detalles diarios (horario laborable).
            // English: Load the employee's assigned shift (current, synced on Empleado.TurnoBase)
            // to show it in the header. Includes the daily details (working schedule).
            var empleado = await db.Empleados
                .AsNoTracking()
                .Include(e => e.TurnoBase!.Detalles)
                .FirstOrDefaultAsync(e => e.Id == EmpleadoId && e.EmpresaId == _empresaId);
            (_turnoEtiqueta, _turnoHorario) = FormatearTurno(empleado?.TurnoBase, _resumenPeriodo?.EsMetaSemanal ?? false);

            // Sugerencia de permiso por diferencia para este periodo. Si la diferencia
            // es ≤ 0, la sección UI no se renderiza (Hidden). Si hay diferencia, también
            // cargamos las ausencias sintéticas ya existentes para pre-rellenar la UI.
            _sugerenciaDiferencia = await PermisoPorDiferencia.CalcularSugerenciaAsync(
                db, _empresaId, EmpleadoId, FechaReferencia, cancellationToken: default);

            _edicionPermisoDiferencia = await CargarEdicionPermisoDiferenciaAsync(
                db, _edicionPermisoDiferencia, _sugerenciaDiferencia.DiferenciaMinutos,
                _sugerenciaDiferencia.BancoDisponibleMinutos);
        }
        catch (Exception ex)
        {
            error = ex.InnerException?.Message ?? ex.Message;
        }
        finally
        {
            _cargandoDatos = false;
        }
    }

    private async Task CerrarAsync()
    {
        _ultimoEmpleadoCargado = Guid.Empty;
        _ultimoInicioPeriodo = null;
        _ultimoFinPeriodo = null;
        _resumenPeriodo = null;
        _resolucion = null;
        _edicion = new EdicionResolucionPeriodo();
        _sugerenciaDiferencia = null;
        _edicionPermisoDiferencia = new EdicionPermisoDiferencia();
        _turnoEtiqueta = string.Empty;
        _turnoHorario = string.Empty;
        await OnClose.InvokeAsync();
    }

    private int ExtraAbsorbible
        => Math.Max(0, _resumenPeriodo?.MinutosExtraAbsorbible ?? MinutosExtraDetectado);

    private void AplicarQuickResolucion(string modo)
    {
        var resumenPeriodo = _resumenPeriodo;
        var factorPago = resumenPeriodo?.FactorTiempoExtra > 0m ? resumenPeriodo.FactorTiempoExtra : 2m;
        var factorBanco = resumenPeriodo?.FactorAcumulacionBancoHoras > 0m ? resumenPeriodo.FactorAcumulacionBancoHoras : 1m;
        var bancoHabilitado = resumenPeriodo?.BancoHorasHabilitado ?? false;

        var extra = ExtraAbsorbible;
        _edicion.Resolucion = modo;
        // F9 — "Descartar": acepta la detección sin pagar ni bancar (descuento
        // completo, sin compensación). Las bases y líneas quedan en 0.
        if (modo == "Descartado")
        {
            _edicion.DescartarExtra = true;
            _edicion.MinutosBasePago = 0;
            _edicion.MinutosBaseBanco = 0;
            _edicion.Lineas.Clear();
            return;
        }

        _edicion.DescartarExtra = false;
        _edicion.MinutosBasePago = modo switch
        {
            "PagarTodo" => extra,
            "BancoTodo" => 0,
            "MitadMitad" => (int)Math.Round(extra / 2m, MidpointRounding.AwayFromZero),
            _ => _edicion.MinutosBasePago
        };
        _edicion.MinutosBaseBanco = modo switch
        {
            "PagarTodo" => 0,
            "BancoTodo" => extra,
            "MitadMitad" => Math.Max(0, extra - _edicion.MinutosBasePago),
            _ => _edicion.MinutosBaseBanco
        };

        _edicion.Lineas.Clear();
        if (modo == "PagarTodo")
        {
            _edicion.Lineas.Add(new LineaEdicionResolucion { Destino = RrhhDestinoTiempoExtraLinea.Pago, Minutos = extra, Factor = factorPago });
        }
        else if (modo == "BancoTodo")
        {
            if (bancoHabilitado)
            {
                _edicion.Lineas.Add(new LineaEdicionResolucion { Destino = RrhhDestinoTiempoExtraLinea.Banco, Minutos = extra, Factor = factorBanco });
            }
            else
            {
                _edicion.Lineas.Add(new LineaEdicionResolucion { Destino = RrhhDestinoTiempoExtraLinea.Pago, Minutos = extra, Factor = factorPago });
            }
        }
        else if (modo == "MitadMitad")
        {
            _edicion.Lineas.Add(new LineaEdicionResolucion { Destino = RrhhDestinoTiempoExtraLinea.Pago, Minutos = _edicion.MinutosBasePago, Factor = factorPago });
            if (bancoHabilitado && _edicion.MinutosBaseBanco > 0)
            {
                _edicion.Lineas.Add(new LineaEdicionResolucion { Destino = RrhhDestinoTiempoExtraLinea.Banco, Minutos = _edicion.MinutosBaseBanco, Factor = factorBanco });
            }
        }
    }

    private void AgregarLinea()
    {
        var factorPago = _resumenPeriodo?.FactorTiempoExtra > 0m ? _resumenPeriodo.FactorTiempoExtra : 2m;
        _edicion.Lineas.Add(new LineaEdicionResolucion
        {
            Destino = RrhhDestinoTiempoExtraLinea.Pago,
            Minutos = 0,
            Factor = factorPago
        });
    }

    private void QuitarLinea(int indice)
    {
        if (indice >= 0 && indice < _edicion.Lineas.Count)
        {
            _edicion.Lineas.RemoveAt(indice);
        }
    }

    private async Task AutorizarAsync()
    {
        if (cargando)
        {
            return;
        }

        // Calcular excedeCap al momento de invocar (mismas reglas que en la vista).
        var totalLineasMin = _edicion.Lineas.Sum(l => Math.Max(0, l.Minutos));
        var extraAbsorbible = ExtraAbsorbible;
        if (totalLineasMin > extraAbsorbible)
        {
            error = $"Las líneas suman {totalLineasMin} min y exceden el extra absorbible ({extraAbsorbible} min). Ajusta antes de autorizar.";
            return;
        }

        // F9 — descartar es la excepción intencional a "0 minutos": acepta la
        // detección sin pagar/bancar (descuento completo). Si el operador marcó
        // "Descartar" O limpió TODAS las líneas a 0 y autoriza, se trata como
        // descarte explícito: el extra NO absorbe faltante/retardo (descuento
        // completo), igual que el botón "Descartar". Evita el estado ambiguo "neteo
        // sin pago" (el extra absorbería el faltante reduciendo el descuento aunque
        // no se pague nada). Se usa una variable local (no se muta _edicion) para
        // que si el apply falla, el operador pueda re-agregar líneas sin quedar
        // pegado en modo descarte.
        // English: F9 — discard is the intentional exception to "0 minutes": accepts
        // the detection without paying/banking (full deduction). If the operator
        // clicked "Descartar" OR cleared ALL lines to 0 and authorizes, treat it as
        // explicit discard: the extra does NOT absorb faltante/retardo (full
        // deduction), same as the "Descartar" button. Avoids the ambiguous "net
        // without pay" state. A local var is used (no _edicion mutation) so that
        // if the apply fails, the operator can re-add lines without being stuck in
        // discard mode.
        var descartar = _edicion.DescartarExtra || totalLineasMin == 0;

        cargando = true;
        error = null;
        ok = null;
        try
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            var result = await ResolucionPeriodo.AplicarResolucionPeriodoAsync(db, new RrhhResolucionPeriodoCommand
            {
                EmpresaId = _empresaId,
                EmpleadoId = EmpleadoId,
                FechaReferencia = FechaReferencia,
                // Rango explícito del periodo en pantalla (vista contenedor): el apply resuelve
                // sobre ESTE rango, igual que el preview, evitando autorizar otra semana si
                // FechaReferencia cae fuera (p.ej. "hoy" al revisar una semana pasada).
                // English: Explicit on-screen period range (container view): the apply resolves
                // over THIS range, same as the preview, avoiding authorizing a different week if
                // FechaReferencia falls outside (e.g. "today" while reviewing a past week).
                FechaInicioPeriodo = FechaInicioPeriodo,
                FechaFinPeriodo = FechaFinPeriodo,
                Resolucion = _edicion.Resolucion,
                MinutosBasePago = descartar ? 0 : Math.Max(0, _edicion.MinutosBasePago),
                MinutosBaseBanco = descartar ? 0 : Math.Max(0, _edicion.MinutosBaseBanco),
                DescartarExtra = descartar,
                Observaciones = _edicion.Observaciones,
                UsuarioActual = _usuarioActual ?? string.Empty,
                Lineas = descartar
                    ? Array.Empty<RrhhResolucionPeriodoLineaCommand>()
                    : _edicion.Lineas
                        .Where(l => l.Minutos > 0)
                        .Select(l => new RrhhResolucionPeriodoLineaCommand
                        {
                            Destino = l.Destino,
                            Minutos = Math.Max(0, l.Minutos),
                            Factor = Math.Max(0m, l.Factor),
                            Observaciones = l.Observaciones
                        })
                        .ToList(),
                PermisosPorDiferencia = ConstruirPermisosPorDiferenciaCommand()
            });
            await db.SaveChangesAsync();
            ok = descartar
                ? "Periodo autorizado con extra descartado: sin pago ni compensación (descuento completo)."
                : $"Periodo autorizado: pago {result.MinutosPagoAplicados} min, banco {result.MinutosBancoAplicados} min.";
            await OnUpdated.InvokeAsync();
            await CerrarAsync();
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

    private async Task ReabrirAsync()
    {
        cargando = true;
        error = null;
        ok = null;
        try
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            // Reabre el periodo en pantalla (rango explícito), no el que resuelva FechaReferencia.
            // English: Reopens the on-screen period (explicit range), not whatever FechaReferencia resolves to.
            await ResolucionPeriodo.ReabrirPeriodoAsync(db, _empresaId, EmpleadoId, FechaInicioPeriodo, FechaFinPeriodo, FechaReferencia, _usuarioActual ?? string.Empty);
            await db.SaveChangesAsync();
            ok = "Periodo reabierto. Revisa las marcaciones y vuelve a autorizar.";
            await OnUpdated.InvokeAsync();
            await CerrarAsync();
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

    private static string FormatearMinutos(int minutos)
        => minutos <= 0 ? "0" : $"{minutos / 60:0}h {minutos % 60:00}m";

    private static string FormatearHorasDecimales(int minutos)
        => (minutos / 60.0).ToString("0.00");

    /// <summary>
    /// Jornada programada del periodo = horas base que se pagan como salario,
    /// independientemente del extra (el extra se paga aparte como "Adic. a Pago").
    /// Espejo del cálculo de nómina: el sueldo base cubre la jornada completa.
    /// = trabajado − extra detectado + faltante neto + permiso con goce
    ///   (los dos últimos reconstruyen el faltante bruto no trabajado).
    /// Period scheduled hours = base hours paid as salary, regardless of extra
    /// (extra is paid separately as "Adic. a Pago"). Mirrors the payroll calc:
    /// base salary covers the full scheduled jornada.
    /// = worked − detected extra + net absence + paid leave
    ///   (the last two reconstruct the gross unworked absence).
    /// </summary>
    private int CalcularJornadaProgramada()
    {
        // Hrs Pagadas = jornada base pagada como salario (sin extra). Se toma directa de la
        // jornada programada/meta del periodo (MinutosBasePagadaPeriodo) en vez de
        // trabajado − extraDetectado: el extra per-día tiene umbral y puede ser menor que
        // (trabajado − meta), y esos minutos "perdidos" se colaban como base (bug 48.03 vs 48.00).
        // English: Paid Hours = base jornada paid as salary (no extra). Taken directly from the
        // period's scheduled jornada/meta (MinutosBasePagadaPeriodo) instead of worked −
        // detectedExtra: per-day extra has a threshold and can be less than (worked − meta),
        // and those "lost" minutes leaked into the base (48.03 vs 48.00 bug).
        return _resumenPeriodo?.MinutosBasePagadaPeriodo ?? 0;
    }

    private int CalcularTotalHorasAdicionales()
    {
        if (_resumenPeriodo is null) return 0;
        var extraPago = _resolucion?.MinutosExtraPago ?? 0;
        return _resumenPeriodo.MinutosBancoConsumidoPeriodo + extraPago;
    }

    // Deducciones NETEADAS del periodo: lo que realmente descuenta la nómina tras el neteo del
    // extra (detectado − absorbido). Espejo del sourcing "periodo". Hrs Pagadas = jornada − estas
    // tres → Hrs Pagadas + las 3 deducciones = jornada. El bruto detectado está en el bloque neteo.
    // English: NETTED period deductions: what payroll actually docks after extra nets them
    // (detected − absorbed). Mirror of "periodo" sourcing. Paid Hours = jornada − these three
    // → Paid Hours + the 3 deductions = jornada. Raw detected is in the neteo block.
    private int CalcularDedFaltanteNeta()
        => _resumenPeriodo is null ? 0 : Math.Max(0, _resumenPeriodo.MinutosFaltanteNetoPeriodo - _resumenPeriodo.MinutosFaltanteAbsorbidoExtra);
    private int CalcularDedRetardoNeto()
        => _resumenPeriodo is null ? 0 : Math.Max(0, _resumenPeriodo.MinutosRetardoDetectado - _resumenPeriodo.MinutosRetardoAbsorbidoExtra);
    private int CalcularDedSalidaNeta()
        => _resumenPeriodo is null ? 0 : Math.Max(0, _resumenPeriodo.MinutosSalidaAnticipadaDetectado - _resumenPeriodo.MinutosSalidaAnticipadaAbsorbidoExtra);

    /// <summary>
    /// Construye la etiqueta y el horario del turno para el encabezado del modal.
    /// Si no hay turno asignado, distingue "meta semanal" (Fija sin turno) del caso
    /// genérico "sin turno". El horario resume los días laborables con entrada/salida.
    /// English: Builds the shift label and schedule for the modal header. When no shift
    /// is assigned, distinguishes "weekly meta" (Fija with no shift) from a plain "no shift".
    /// The schedule summarizes the working days with entry/exit times.
    /// </summary>
    private static (string Etiqueta, string Horario) FormatearTurno(TurnoBase? turno, bool esMetaSemanal)
    {
        if (turno is null)
        {
            return (esMetaSemanal ? "Sin turno (meta semanal)" : "Sin turno asignado", string.Empty);
        }

        var dias = turno.Detalles
            .Where(d => d.Labora)
            .OrderBy(d => d.DiaSemana)
            .Select(d => $"{AbreviarDiaSemana(d.DiaSemana)} {FormatearHoraTurno(d.HoraEntrada)}-{FormatearHoraTurno(d.HoraSalida)}")
            .ToList();
        var horario = dias.Count == 0 ? string.Empty : string.Join(" · ", dias);
        return ($"Turno: {turno.Nombre}", horario);
    }

    private static string AbreviarDiaSemana(DiaSemanaTurno dia) => dia switch
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

    private static string FormatearHoraTurno(TimeSpan? hora) => hora?.ToString("hh\\:mm") ?? "—";

    private static string ObtenerAbreviaturaDia(DateOnly fecha)
        => fecha.DayOfWeek switch
        {
            DayOfWeek.Sunday => "Dom",
            DayOfWeek.Monday => "Lun",
            DayOfWeek.Tuesday => "Mar",
            DayOfWeek.Wednesday => "Mié",
            DayOfWeek.Thursday => "Jue",
            DayOfWeek.Friday => "Vie",
            DayOfWeek.Saturday => "Sáb",
            _ => string.Empty
        };

    private static string ObtenerEtiquetaEstatus(RrhhResolucionTiempoExtraPeriodo? resolucion)
        => resolucion switch
        {
            null => "Pendiente de autorizar",
            { Estatus: RrhhResolucionPeriodoEstatus.Autorizada, ExtraDescartado: true } => "Autorizada (descartado)",
            { Estatus: RrhhResolucionPeriodoEstatus.Autorizada } => "Autorizada",
            { Estatus: RrhhResolucionPeriodoEstatus.Reabierta } => "Reabierta",
            _ => "Pendiente"
        };

    private static string ObtenerClaseEstatus(RrhhResolucionTiempoExtraPeriodo? resolucion)
        => resolucion switch
        {
            null => "bg-warning text-dark",
            { Estatus: RrhhResolucionPeriodoEstatus.Autorizada } => "bg-success",
            { Estatus: RrhhResolucionPeriodoEstatus.Reabierta } => "bg-warning text-dark",
            _ => "bg-warning text-dark"
        };

    /// <summary>
    /// Empaqueta el estado de los 3 inputs (Banco / ConGoce / SinGoce) en una lista
    /// de <see cref="PermisoDiferenciaInput"/>. Si la suma es 0, devuelve null (el
    /// servicio interpreta null/list-vacía como "sin permisos por diferencia", que
    /// para nuestro servicio es idempotente: revierte sintéticas previas si las hay).
    /// </summary>
    private List<PermisoDiferenciaInput>? ConstruirPermisosPorDiferenciaCommand()
    {
        var ed = _edicionPermisoDiferencia;
        var inputs = new List<PermisoDiferenciaInput>
        {
            new() { Categoria = CategoriaPermisoDiferencia.Banco,          Minutos = Math.Max(0, ed.BancoMinutos), Factor = Math.Max(1m, ed.BancoFactor), Observaciones = ed.BancoObservaciones },
            new() { Categoria = CategoriaPermisoDiferencia.ConGoceSinBanco, Minutos = Math.Max(0, ed.ConGoceMinutos), Factor = Math.Max(1m, ed.ConGoceFactor), Observaciones = ed.ConGoceObservaciones },
            new() { Categoria = CategoriaPermisoDiferencia.SinGoce,         Minutos = Math.Max(0, ed.SinGoceMinutos), Factor = Math.Max(1m, ed.SinGoceFactor), Observaciones = ed.SinGoceObservaciones }
        };
        return inputs.Sum(i => i.Minutos) > 0 ? inputs : null;
    }

    /// <summary>
    /// Carga la edición de permisos por diferencia: si ya hay sintéticas del periodo
    /// las pre-rellena con sus valores; si la diferencia > 0 y no hay sintéticas,
    /// pre-rellena la categoría Banco con min(diferencia, saldo banco) y 0 las otras.
    /// </summary>
    private async Task<EdicionPermisoDiferencia> CargarEdicionPermisoDiferenciaAsync(
        CrmDbContext db, EdicionPermisoDiferencia actual, int diferenciaMinutos, int bancoDisponibleMinutos)
    {
        if (diferenciaMinutos <= 0)
        {
            return new EdicionPermisoDiferencia();
        }

        // ¿Hay permisos por diferencia ya existentes para este periodo?
        var existentes = await db.RrhhAusencias
            .AsNoTracking()
            .Where(a => a.EmpresaId == _empresaId
                && a.EmpleadoId == EmpleadoId
                && a.OrigenAusencia == OrigenAusenciaRrhh.SinteticoPorPeriodo
                && a.Tipo == TipoAusenciaRrhh.PermisoPorDiferenciaPeriodo
                && a.IsActive)
            .ToListAsync();

        var nuevos = new EdicionPermisoDiferencia();
        foreach (var a in existentes)
        {
            var minutos = (int)Math.Round(a.Horas * 60m, MidpointRounding.AwayFromZero);
            if (a.DescuentaBancoHoras)
            {
                nuevos.BancoMinutos = minutos;
                nuevos.BancoFactor = 1m;
                nuevos.BancoObservaciones = a.Observaciones;
            }
            else if (a.ConGocePago)
            {
                nuevos.ConGoceMinutos = minutos;
                nuevos.ConGoceFactor = 1m;
                nuevos.ConGoceObservaciones = a.Observaciones;
            }
            else
            {
                nuevos.SinGoceMinutos = minutos;
                nuevos.SinGoceFactor = 1m;
                nuevos.SinGoceObservaciones = a.Observaciones;
            }
        }

        // Si no hay existentes, default sugerido: la diferencia, repartida en Banco
        // (hasta donde alcance el saldo) y el resto en ConGoce.
        if (existentes.Count == 0)
        {
            var bancoPorMinutos = Math.Min(diferenciaMinutos, Math.Max(0, bancoDisponibleMinutos));
            nuevos.BancoMinutos = bancoPorMinutos;
            nuevos.ConGoceMinutos = diferenciaMinutos - bancoPorMinutos;
        }

        return nuevos;
    }

    private static EdicionResolucionPeriodo CrearEdicionInicial(
        RrhhResolucionTiempoExtraPeriodo? periodo,
        RrhhResolucionPeriodoResumen? resumen)
    {
        var factorPagoDefault = resumen?.FactorTiempoExtra > 0m ? resumen.FactorTiempoExtra : 2m;
        var factorBancoDefault = resumen?.FactorAcumulacionBancoHoras > 0m ? resumen.FactorAcumulacionBancoHoras : 1m;

        if (periodo is { Estatus: RrhhResolucionPeriodoEstatus.Autorizada })
        {
            var edicion = new EdicionResolucionPeriodo
            {
                MinutosBasePago = periodo.MinutosExtraPago,
                MinutosBaseBanco = periodo.MinutosExtraBanco,
                Resolucion = string.IsNullOrWhiteSpace(periodo.Resolucion) ? "PagarTodo" : periodo.Resolucion,
                Observaciones = periodo.Observaciones
            };

            if (periodo.Lineas is { Count: > 0 } lineas)
            {
                foreach (var l in lineas.OrderBy(x => x.Orden))
                {
                    edicion.Lineas.Add(new LineaEdicionResolucion
                    {
                        Destino = l.Destino,
                        Minutos = l.Minutos,
                        Factor = l.Factor > 0m ? l.Factor : (l.Destino == RrhhDestinoTiempoExtraLinea.Banco ? factorBancoDefault : factorPagoDefault),
                        Observaciones = l.Observaciones
                    });
                }
            }
            else
            {
                if (periodo.MinutosExtraPago > 0)
                {
                    edicion.Lineas.Add(new LineaEdicionResolucion
                    {
                        Destino = RrhhDestinoTiempoExtraLinea.Pago,
                        Minutos = periodo.MinutosExtraPago,
                        Factor = periodo.FactorTiempoExtraAplicado ?? factorPagoDefault,
                        Observaciones = null
                    });
                }
                if (periodo.MinutosExtraBanco > 0)
                {
                    edicion.Lineas.Add(new LineaEdicionResolucion
                    {
                        Destino = RrhhDestinoTiempoExtraLinea.Banco,
                        Minutos = periodo.MinutosExtraBanco,
                        Factor = periodo.FactorAcumulacionBancoHorasAplicado ?? factorBancoDefault,
                        Observaciones = null
                    });
                }
            }
            return edicion;
        }

        // Pendiente: por defecto "Todo pago" con el extra absorbible YA cargado en la
        // línea, para que el operador vea los minutos y Autorizar haga algo útil de
        // entrada. Antes la línea venía en 0 y Autorizar cerraba el periodo en cero
        // (lo marcaba Autorizada sin pagar nada → el botón desaparecía y parecía que
        // "no se podía usar").
        var extraAbsorbibleInicial = Math.Max(0, resumen?.MinutosExtraAbsorbible ?? 0);
        return new EdicionResolucionPeriodo
        {
            Resolucion = "PagarTodo",
            MinutosBasePago = extraAbsorbibleInicial,
            Lineas = new List<LineaEdicionResolucion>
            {
                new() { Destino = RrhhDestinoTiempoExtraLinea.Pago, Minutos = extraAbsorbibleInicial, Factor = factorPagoDefault }
            }
        };
    }

    private sealed class EdicionResolucionPeriodo
    {
        public int MinutosBasePago { get; set; }
        public int MinutosBaseBanco { get; set; }
        public string Resolucion { get; set; } = "PagarTodo";
        public string? Observaciones { get; set; }
        // F9 — descartar el extra detectado: acepta la detección (desbloquea el gate
        // de prenómina) pero NO autoriza compensación ni pago. El faltante/retardo
        // del periodo se descuenta COMPLETO (neteo anulado). Incompatible con líneas
        // y con bases de pago/banco (van en 0).
        public bool DescartarExtra { get; set; }
        public List<LineaEdicionResolucion> Lineas { get; set; } = new();
    }

    private sealed class LineaEdicionResolucion
    {
        public RrhhDestinoTiempoExtraLinea Destino { get; set; } = RrhhDestinoTiempoExtraLinea.Pago;
        public int Minutos { get; set; }
        public decimal Factor { get; set; } = 1m;
        public string? Observaciones { get; set; }
    }
}