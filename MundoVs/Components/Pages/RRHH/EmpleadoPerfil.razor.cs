using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Entities.Serigrafia;
using MundoVs.Core.Interfaces;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Components.Pages.RRHH;

public partial class EmpleadoPerfil
{
    [Inject] private IDbContextFactory<CrmDbContext> DbFactory { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private IRrhhEmpleadoPerfilPageService PerfilService { get; set; } = default!;
    [Inject] private INominaLegalPolicyService NominaLegalPolicy { get; set; } = default!;
    [Inject] private CodigoNegocioService CodigoNegocio { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private EmpleadoPerfilData? _data;
    private bool cargando = true;
    private string? error;
    private bool _puedeVer;
    private bool _puedeEditar;
    private Guid _empresaId;
    private string _tabActiva = "resumen";
    private string _tabAnterior = "resumen";

    // Edit state
    private bool _editandoPersonal;
    private bool _editandoLaboral;
    private bool _editandoNomina;
    private bool _editandoNotas;
    private string? _notasEditando;
    private bool _guardando;
    private Empleado _editando = new();
    private Guid? _esquemaSeleccionadoId;
    private Guid? _turnoSeleccionadoId;
    private DateTime _esquemaVigenteDesde = DateTime.Today;
    private DateTime _turnoVigenteDesde = DateTime.Today;
    private DateTime? _turnoVigenteHasta;
    private decimal? _esquemaSueldoOverride;
    private string? _turnoObservaciones;
    private List<ConceptoEmpleadoEditor> _conceptosEditando = [];
    private TipoAusenciaRrhh? _filtroAusenciaTipo;

    // Ausencia nueva
    private bool _nuevaAusencia;
    private RrhhAusencia _ausenciaEditando = new();
    private DateTime _ausenciaFechaInicio = DateTime.Today;
    private DateTime _ausenciaFechaFin = DateTime.Today;

    private IReadOnlyList<RrhhAusencia> AusenciasFiltradas =>
        _data is null ? [] : _filtroAusenciaTipo.HasValue
            ? _data.AusenciasRecientes.Where(a => a.Tipo == _filtroAusenciaTipo.Value).ToList()
            : _data.AusenciasRecientes;

    protected override async Task OnInitializedAsync()
    {
        await CargarAccesoAsync();
        if (_puedeVer)
        {
            await CargarAsync();
        }
        else
        {
            cargando = false;
        }
    }

    private async Task CargarAccesoAsync()
    {
        var state = await AuthStateProvider.GetAuthenticationStateAsync();
        _puedeVer = state.User.HasClaim("Capacidad", "empleados.ver");
        _puedeEditar = state.User.HasClaim("Capacidad", "empleados.editar");
        _ = Guid.TryParse(state.User.FindFirst("EmpresaId")?.Value, out _empresaId);
    }

    private async Task CargarAsync()
    {
        cargando = true;
        error = null;
        try
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            _data = await PerfilService.CargarAsync(db, _empresaId, Id);
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

    // ── Personal edit ──────────────────────────────────
    private void IniciarEdicionPersonal()
    {
        if (_data is null) return;
        var e = _data.Empleado;
        _editando = new Empleado
        {
            Id = e.Id, Codigo = e.Codigo, NumeroEmpleado = e.NumeroEmpleado, CodigoChecador = e.CodigoChecador, Nombre = e.Nombre,
            ApellidoPaterno = e.ApellidoPaterno, ApellidoMaterno = e.ApellidoMaterno, Curp = e.Curp, Nss = e.Nss,
            Telefono = e.Telefono, Email = e.Email, Direccion = e.Direccion,
            FechaNacimiento = e.FechaNacimiento, FechaContratacion = e.FechaContratacion,
            IsActive = e.IsActive, Notas = e.Notas
        };
        _editandoPersonal = true;
    }

    private async Task GuardarPersonalAsync()
    {
        if (!_puedeEditar || _data is null) return;
        _guardando = true; error = null;
        try
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            var ex = await db.Empleados.FirstOrDefaultAsync(emp => emp.Id == _editando.Id);
            if (ex is null) { error = "Empleado no encontrado."; return; }
            ex.Nombre = _editando.Nombre.Trim();
            ex.ApellidoPaterno = _editando.ApellidoPaterno?.Trim();
            ex.ApellidoMaterno = _editando.ApellidoMaterno?.Trim();
            ex.Curp = _editando.Curp?.Trim();
            ex.Nss = _editando.Nss?.Trim();
            ex.Telefono = _editando.Telefono?.Trim();
            ex.Email = _editando.Email?.Trim();
            ex.Direccion = _editando.Direccion?.Trim();
            ex.CodigoChecador = string.IsNullOrWhiteSpace(_editando.CodigoChecador) ? null : _editando.CodigoChecador.Trim();
            ex.FechaNacimiento = _editando.FechaNacimiento;
            ex.FechaContratacion = _editando.FechaContratacion;
            ex.IsActive = _editando.IsActive;
            ex.Notas = _editando.Notas?.Trim();
            ex.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            _editandoPersonal = false;
            await CargarAsync();
        }
        catch (Exception ex) { error = ex.InnerException?.Message ?? ex.Message; }
        finally { _guardando = false; }
    }

    // ── Laboral edit ──────────────────────────────────
    private void IniciarEdicionLaboral()
    {
        if (_data is null) return;
        var e = _data.Empleado;
        _editando = new Empleado
        {
            Id = e.Id, PosicionId = e.PosicionId, Puesto = e.Puesto, Departamento = e.Departamento, TurnoBaseId = e.TurnoBaseId,
            FechaContratacion = e.FechaContratacion, IsActive = e.IsActive
        };
        _turnoSeleccionadoId = _data.TurnoAsignacionVigente?.TurnoBaseId ?? e.TurnoBaseId;
        _turnoVigenteDesde = _data.TurnoAsignacionVigente?.VigenteDesde.ToDateTime(TimeOnly.MinValue) ?? e.FechaContratacion?.Date ?? DateTime.Today;
        _turnoVigenteHasta = _data.TurnoAsignacionVigente?.VigenteHasta?.ToDateTime(TimeOnly.MinValue);
        _turnoObservaciones = _data.TurnoAsignacionVigente?.Observaciones;
        _editandoLaboral = true;
    }

    private async Task GuardarLaboralAsync()
    {
        if (!_puedeEditar || _data is null) return;
        _guardando = true; error = null;
        try
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            var ex = await db.Empleados.FirstOrDefaultAsync(emp => emp.Id == _editando.Id);
            if (ex is null) { error = "Empleado no encontrado."; return; }
            ex.PosicionId = _editando.PosicionId;
            ex.Puesto = _editando.Puesto?.Trim();
            ex.Departamento = _editando.Departamento?.Trim();
            ex.IsActive = _editando.IsActive;
            ex.UpdatedAt = DateTime.UtcNow;

            await Empleados.GuardarVigenciaTurnoAsync(db, _empresaId, ex.Id, _turnoSeleccionadoId,
                DateOnly.FromDateTime(_turnoVigenteDesde.Date),
                _turnoVigenteHasta.HasValue ? DateOnly.FromDateTime(_turnoVigenteHasta.Value.Date) : null,
                _turnoObservaciones);
            await Empleados.SincronizarTurnoBaseActualAsync(db, _empresaId, ex.Id);
            await db.SaveChangesAsync();

            _editandoLaboral = false;
            await CargarAsync();
        }
        catch (Exception ex) { error = ex.InnerException?.Message ?? ex.Message; }
        finally { _guardando = false; }
    }

    // ── Nómina edit ──────────────────────────────────
    private void IniciarEdicionNomina()
    {
        if (_data is null) return;
        var e = _data.Empleado;
        _editando = new Empleado
        {
            Id = e.Id, SueldoSemanal = e.SueldoSemanal, PeriodicidadPago = e.PeriodicidadPago, TipoNomina = e.TipoNomina,
            AplicaImss = e.AplicaImss, AplicaIsr = e.AplicaIsr, AplicaInfonavit = e.AplicaInfonavit,
            NumeroCreditoInfonavit = e.NumeroCreditoInfonavit, FactorDescuentoInfonavit = e.FactorDescuentoInfonavit
        };
        _esquemaSeleccionadoId = _data.EsquemaAsignacionVigente?.EsquemaPagoId;
        _esquemaVigenteDesde = _data.EsquemaAsignacionVigente?.VigenteDesde ?? DateTime.Today;
        _esquemaSueldoOverride = _data.EsquemaAsignacionVigente?.SueldoBaseOverride;
        _editandoNomina = true;
    }

    private async Task GuardarNominaAsync()
    {
        if (!_puedeEditar || _data is null) return;
        _guardando = true; error = null;
        try
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            var ex = await db.Empleados.FirstOrDefaultAsync(emp => emp.Id == _editando.Id);
            if (ex is null) { error = "Empleado no encontrado."; return; }
            ex.SueldoSemanal = _editando.SueldoSemanal;
            ex.PeriodicidadPago = _editando.PeriodicidadPago;
            ex.TipoNomina = _editando.TipoNomina;
            ex.AplicaImss = _editando.AplicaImss;
            ex.AplicaIsr = _editando.AplicaIsr;
            ex.AplicaInfonavit = _editando.AplicaInfonavit;
            ex.NumeroCreditoInfonavit = _editando.AplicaInfonavit ? _editando.NumeroCreditoInfonavit?.Trim() : null;
            ex.FactorDescuentoInfonavit = _editando.AplicaInfonavit ? _editando.FactorDescuentoInfonavit : 0m;
            ex.UpdatedAt = DateTime.UtcNow;

            // Guardar esquema si se seleccionó uno
            if (_esquemaSeleccionadoId.HasValue && _esquemaSeleccionadoId.Value != Guid.Empty)
            {
                var asignacionExistente = await db.EmpleadosEsquemaPago
                    .FirstOrDefaultAsync(a => a.EmpleadoId == ex.Id && a.EsquemaPagoId == _esquemaSeleccionadoId.Value
                        && (a.VigenteHasta == null || a.VigenteHasta >= DateTime.Today));
                if (asignacionExistente is null)
                {
                    var anteriores = await db.EmpleadosEsquemaPago.Where(a => a.EmpleadoId == ex.Id && a.VigenteHasta == null).ToListAsync();
                    foreach (var ant in anteriores) { ant.VigenteHasta = _esquemaVigenteDesde.AddDays(-1); ant.UpdatedAt = DateTime.UtcNow; }
                    db.EmpleadosEsquemaPago.Add(new EmpleadoEsquemaPago
                    {
                        EmpleadoId = ex.Id, EsquemaPagoId = _esquemaSeleccionadoId.Value,
                        VigenteDesde = _esquemaVigenteDesde,
                        SueldoBaseOverride = _esquemaSueldoOverride > 0 ? _esquemaSueldoOverride : null
                    });
                }
                else if (asignacionExistente.SueldoBaseOverride != _esquemaSueldoOverride)
                {
                    asignacionExistente.SueldoBaseOverride = _esquemaSueldoOverride > 0 ? _esquemaSueldoOverride : null;
                    asignacionExistente.UpdatedAt = DateTime.UtcNow;
                }
            }

            await db.SaveChangesAsync();
            _editandoNomina = false;
            await CargarAsync();
        }
        catch (Exception ex) { error = ex.InnerException?.Message ?? ex.Message; }
        finally { _guardando = false; }
    }

    // ── Notas edit ──────────────────────────────────
    private void IniciarEdicionNotas()
    {
        if (_data is null) return;
        _notasEditando = _data.Empleado.Notas;
        _editandoNotas = true;
    }

    private async Task GuardarNotasAsync()
    {
        if (!_puedeEditar || _data is null) return;
        _guardando = true; error = null;
        try
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            var ex = await db.Empleados.FirstOrDefaultAsync(emp => emp.Id == _data.Empleado.Id);
            if (ex is null) { error = "Empleado no encontrado."; return; }
            ex.Notas = string.IsNullOrWhiteSpace(_notasEditando) ? null : _notasEditando.Trim();
            ex.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            _editandoNotas = false;
            await CargarAsync();
        }
        catch (Exception ex) { error = ex.InnerException?.Message ?? ex.Message; }
        finally { _guardando = false; }
    }

    // ── Conceptos edit (reuses Empleados logic) ───────
    private void IniciarEdicionConceptos()
    {
        if (_data is null) return;
        _conceptosEditando = _data.ConceptosNomina.Select(c => new ConceptoEmpleadoEditor
        {
            Id = c.Id, ConceptoConfigId = c.ConceptoConfigId, ConceptoNombre = c.ConceptoConfig.Nombre,
            Naturaleza = c.ConceptoConfig.Naturaleza, Destino = c.ConceptoConfig.Destino, Orden = c.ConceptoConfig.Orden,
            Monto = c.Monto, Porcentaje = c.Porcentaje, Cantidad = c.Cantidad, Tarifa = c.Tarifa,
            Saldo = c.Saldo, Limite = c.Limite, FechaInicio = c.FechaInicio, FechaFin = c.FechaFin,
            EsRecurrente = c.EsRecurrente, Observaciones = c.Observaciones
        }).ToList();
    }

    // ── Nueva ausencia ──────────────────────────────────
    private void IniciarNuevaAusencia()
    {
        _ausenciaEditando = new RrhhAusencia
        {
            Tipo = TipoAusenciaRrhh.Permiso,
            Estatus = EstatusAusenciaRrhh.Solicitada,
            ConGocePago = false,
            Dias = 1,
            Horas = 0
        };
        _ausenciaFechaInicio = DateTime.Today;
        _ausenciaFechaFin = DateTime.Today;
        _nuevaAusencia = true;
    }

    private void CancelarNuevaAusencia()
    {
        _nuevaAusencia = false;
        _ausenciaEditando = new();
    }

    private async Task GuardarNuevaAusenciaAsync()
    {
        if (!_puedeEditar || _data is null) return;
        _guardando = true;
        error = null;
        try
        {
            if (_ausenciaFechaFin < _ausenciaFechaInicio)
            {
                error = "La fecha fin no puede ser menor a la fecha inicio.";
                return;
            }
            if (_ausenciaEditando.Dias <= 0 && _ausenciaEditando.Horas <= 0)
            {
                error = "Captura días u horas para la ausencia.";
                return;
            }

            await using var db = await DbFactory.CreateDbContextAsync();
            var entity = new RrhhAusencia
            {
                EmpresaId = _empresaId,
                EmpleadoId = _data.Empleado.Id,
                Tipo = _ausenciaEditando.Tipo,
                Estatus = _ausenciaEditando.Estatus,
                FechaInicio = DateOnly.FromDateTime(_ausenciaFechaInicio),
                FechaFin = DateOnly.FromDateTime(_ausenciaFechaFin),
                Dias = _ausenciaEditando.Dias,
                Horas = _ausenciaEditando.Horas,
                ConGocePago = _ausenciaEditando.Tipo == TipoAusenciaRrhh.Vacaciones || _ausenciaEditando.ConGocePago,
                Motivo = string.IsNullOrWhiteSpace(_ausenciaEditando.Motivo) ? null : _ausenciaEditando.Motivo.Trim(),
                Observaciones = string.IsNullOrWhiteSpace(_ausenciaEditando.Observaciones) ? null : _ausenciaEditando.Observaciones.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (entity.Estatus == EstatusAusenciaRrhh.Aprobada || entity.Estatus == EstatusAusenciaRrhh.Aplicada)
            {
                var state = await AuthStateProvider.GetAuthenticationStateAsync();
                entity.FechaAprobacion = DateTime.UtcNow;
                entity.AprobadoPor = state.User.Identity?.Name;
            }

            db.RrhhAusencias.Add(entity);
            await db.SaveChangesAsync();
            _nuevaAusencia = false;
            _ausenciaEditando = new();
            await CargarAsync();
        }
        catch (Exception ex)
        {
            error = ex.InnerException?.Message ?? ex.Message;
        }
        finally
        {
            _guardando = false;
        }
    }

    // ── Helpers ────────────────────────────────────────
    private static string ObtenerEtiquetaEstatus(RrhhAsistenciaEstatus estatus) => estatus switch
    {
        RrhhAsistenciaEstatus.Pendiente => "Pendiente",
        RrhhAsistenciaEstatus.AsistenciaNormal => "Normal",
        RrhhAsistenciaEstatus.Retardo => "Retardo",
        RrhhAsistenciaEstatus.Falta => "Falta",
        RrhhAsistenciaEstatus.Descanso => "Descanso",
        RrhhAsistenciaEstatus.DescansoTrabajado => "Descanso trab.",
        RrhhAsistenciaEstatus.Incompleta => "Incompleta",
        RrhhAsistenciaEstatus.TurnoNoAsignado => "Sin turno",
        RrhhAsistenciaEstatus.MarcaNoReconocida => "Sin reconocer",
        _ => estatus.ToString()
    };

    private static string ObtenerClaseEstatus(RrhhAsistenciaEstatus estatus) => estatus switch
    {
        RrhhAsistenciaEstatus.AsistenciaNormal => "bg-success",
        RrhhAsistenciaEstatus.Retardo => "bg-warning text-dark",
        RrhhAsistenciaEstatus.Falta => "bg-danger",
        RrhhAsistenciaEstatus.Descanso => "bg-secondary",
        RrhhAsistenciaEstatus.DescansoTrabajado => "bg-info text-dark",
        RrhhAsistenciaEstatus.Incompleta => "bg-warning text-dark",
        _ => "bg-secondary"
    };

    private static string ObtenerEtiquetaNaturaleza(NaturalezaConceptoNominaRrhh naturaleza) => naturaleza switch
    {
        NaturalezaConceptoNominaRrhh.Deduccion => "Deducción",
        NaturalezaConceptoNominaRrhh.Obligacion => "Obligación",
        NaturalezaConceptoNominaRrhh.Provision => "Provisión",
        NaturalezaConceptoNominaRrhh.Ajuste => "Ajuste",
        NaturalezaConceptoNominaRrhh.Percepcion => "Percepción",
        _ => naturaleza.ToString()
    };

    private static string ObtenerEtiquetaTipoCalculo(TipoCalculoConceptoNominaRrhh tipo) => tipo switch
    {
        TipoCalculoConceptoNominaRrhh.MontoFijo => "Monto fijo",
        TipoCalculoConceptoNominaRrhh.Porcentaje => "Porcentaje",
        TipoCalculoConceptoNominaRrhh.CantidadPorTarifa => "Cant. × tarifa",
        TipoCalculoConceptoNominaRrhh.Formula => "Fórmula",
        TipoCalculoConceptoNominaRrhh.Manual => "Manual",
        _ => tipo.ToString()
    };

    private static string ObtenerEtiquetaAusenciaTipo(TipoAusenciaRrhh tipo) => tipo switch
    {
        TipoAusenciaRrhh.Vacaciones => "Vacaciones",
        TipoAusenciaRrhh.Permiso => "Permiso",
        _ => tipo.ToString()
    };

    private static string ObtenerEtiquetaEstatusAusencia(EstatusAusenciaRrhh estatus) => estatus switch
    {
        EstatusAusenciaRrhh.Solicitada => "Solicitada",
        EstatusAusenciaRrhh.Aprobada => "Aprobada",
        EstatusAusenciaRrhh.Rechazada => "Rechazada",
        EstatusAusenciaRrhh.Aplicada => "Aplicada",
        EstatusAusenciaRrhh.Cancelada => "Cancelada",
        _ => estatus.ToString()
    };

    private static string ObtenerClaseEstatusAusencia(EstatusAusenciaRrhh estatus) => estatus switch
    {
        EstatusAusenciaRrhh.Aprobada => "bg-success",
        EstatusAusenciaRrhh.Aplicada => "bg-success",
        EstatusAusenciaRrhh.Solicitada => "bg-warning text-dark",
        EstatusAusenciaRrhh.Rechazada => "bg-danger",
        EstatusAusenciaRrhh.Cancelada => "bg-secondary",
        _ => "bg-secondary"
    };

    private static string ObtenerEtiquetaMovimientoBanco(TipoMovimientoBancoHorasRrhh tipo) => tipo switch
    {
        TipoMovimientoBancoHorasRrhh.GeneradoPorHorasExtra => "Horas extra",
        TipoMovimientoBancoHorasRrhh.AjusteManual => "Ajuste manual",
        TipoMovimientoBancoHorasRrhh.Consumo => "Consumo",
        _ => tipo.ToString()
    };

    private static string FormatearHorasBanco(decimal horas) => $"{horas:0.##} h";

    private string? ObtenerNombrePosicion(Guid? posicionId)
        => posicionId.HasValue ? _data?.Posiciones.FirstOrDefault(p => p.Id == posicionId.Value)?.Nombre : null;

    private string ObtenerNombreTablaDepartamento(string? dept)
        => _data?.Departamentos.FirstOrDefault(d => d.Nombre == dept)?.Nombre ?? dept ?? "—";

    private string ObtenerEsquemaActivo(Guid empleadoId)
        => _data?.EsquemaVigente ?? "—";

    private static string ObtenerEtiquetaTurno(TurnoBase turno)
        => turno.IsActive ? turno.Nombre : $"{turno.Nombre} (inactivo)";

    private IEnumerable<string> DepartamentosDisponibles
        => (_data?.Departamentos ?? []).Select(d => d.Nombre)
            .Append(_editando.Departamento ?? string.Empty)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n);

    // Reuse editor class from Empleados page
    private class ConceptoEmpleadoEditor
    {
        public Guid Id { get; set; }
        public Guid ConceptoConfigId { get; set; }
        public string ConceptoNombre { get; set; } = string.Empty;
        public NaturalezaConceptoNominaRrhh Naturaleza { get; set; }
        public DestinoConceptoNominaRrhh Destino { get; set; }
        public int Orden { get; set; }
        public decimal Monto { get; set; }
        public decimal Porcentaje { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Tarifa { get; set; }
        public decimal Saldo { get; set; }
        public decimal Limite { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public bool EsRecurrente { get; set; } = true;
        public string? Observaciones { get; set; }
    }

    private static string ObtenerIniciales(string? nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return "?";
        var partes = nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length >= 2) return $"{partes[0][0]}{partes[^1][0]}".ToUpperInvariant();
        return nombre[0].ToString().ToUpperInvariant();
    }

    private void CambiarTab(string tab) { _tabAnterior = _tabActiva; _tabActiva = tab; }
    private string TabClass(string tab) => _tabActiva == tab ? "active" : "";
    private string TabContentClass(string tab) => _tabActiva == tab ? "ep-tab-panel active" : "ep-tab-panel";
    private bool _esHoy(DiaSemanaTurno dia) => (int)dia == (int)DateTime.Now.DayOfWeek;
}