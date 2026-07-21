using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
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
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    [Parameter] public bool Visible { get; set; }
    [Parameter] public Guid EmpleadoId { get; set; }
    [Parameter] public string NombreEmpleado { get; set; } = string.Empty;
    [Parameter] public DateOnly FechaReferencia { get; set; }
    [Parameter] public int MinutosExtraDetectado { get; set; }
    [Parameter] public int SaldoBancoHoras { get; set; }
    [Parameter] public bool PuedeAprobarTiempoExtra { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnUpdated { get; set; }

    private Guid _empresaId;
    private string _usuarioActual = string.Empty;
    private Guid _ultimoEmpleadoCargado;
    private bool _cargandoDatos;
    private bool cargando;
    private string? error;
    private string? ok;

    private RrhhResolucionTiempoExtraPeriodo? _resolucion;
    private RrhhResolucionPeriodoResumen? _resumenPeriodo;
    private EdicionResolucionPeriodo _edicion = new();

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

        // Recargar sólo cuando cambia el empleado o era distinto a cero (evita
        // recargas redundantes por re-render del padre).
        if (_ultimoEmpleadoCargado == EmpleadoId && _resumenPeriodo is not null)
        {
            return;
        }

        _ultimoEmpleadoCargado = EmpleadoId;
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
            _resumenPeriodo = await ResolucionPeriodo.ObtenerResumenPeriodoAsync(
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
        _resumenPeriodo = null;
        _resolucion = null;
        _edicion = new EdicionResolucionPeriodo();
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
        // detección sin pagar/bancar (descuento completo). Cualquier otro modo con
        // 0 minutos sí es error (Autorizar no debe cerrar el periodo en cero).
        if (!_edicion.DescartarExtra && totalLineasMin == 0)
        {
            error = "No hay minutos asignados: usa 'Todo pago', 'Descartar' o edita las líneas.";
            return;
        }

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
                Resolucion = _edicion.Resolucion,
                MinutosBasePago = Math.Max(0, _edicion.MinutosBasePago),
                MinutosBaseBanco = Math.Max(0, _edicion.MinutosBaseBanco),
                DescartarExtra = _edicion.DescartarExtra,
                Observaciones = _edicion.Observaciones,
                UsuarioActual = _usuarioActual ?? string.Empty,
                Lineas = _edicion.DescartarExtra
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
                        .ToList()
            });
            await db.SaveChangesAsync();
            ok = _edicion.DescartarExtra
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
            await ResolucionPeriodo.ReabrirPeriodoAsync(db, _empresaId, EmpleadoId, FechaReferencia, _usuarioActual ?? string.Empty);
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
            null => "Sin resolución",
            { Estatus: RrhhResolucionPeriodoEstatus.Autorizada, ExtraDescartado: true } => "Autorizada (descartado)",
            { Estatus: RrhhResolucionPeriodoEstatus.Autorizada } => "Autorizada",
            { Estatus: RrhhResolucionPeriodoEstatus.Reabierta } => "Reabierta",
            _ => "Pendiente"
        };

    private static string ObtenerClaseEstatus(RrhhResolucionTiempoExtraPeriodo? resolucion)
        => resolucion switch
        {
            null => "bg-light text-dark",
            { Estatus: RrhhResolucionPeriodoEstatus.Autorizada } => "bg-success",
            { Estatus: RrhhResolucionPeriodoEstatus.Reabierta } => "bg-warning text-dark",
            _ => "bg-secondary"
        };

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