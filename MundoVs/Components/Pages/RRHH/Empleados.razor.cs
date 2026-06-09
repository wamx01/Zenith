using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Entities.Serigrafia;
using MundoVs.Core.Interfaces;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Components.Pages.RRHH;

public partial class Empleados
{
    [Inject] private IDbContextFactory<CrmDbContext> DbFactory { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private CodigoNegocioService CodigoNegocio { get; set; } = default!;
    [Inject] private INominaLegalPolicyService NominaLegalPolicy { get; set; } = default!;

    private List<Empleado> lista = [];
    private List<Posicion> posiciones = [];
    private List<DepartamentoRrhh> departamentos = [];
    private List<EsquemaPago> esquemasPago = [];
    private List<TurnoBase> turnosBase = [];
    private Dictionary<Guid, string> esquemaActivoPorEmpleado = new();
    private Dictionary<Guid, decimal> vacacionesDisponiblesPorEmpleado = new();
    private Dictionary<Guid, decimal> bancoHorasPorEmpleado = new();
    private List<NominaConceptoConfigRrhh> conceptosNominaDisponibles = [];
    private List<ConceptoEmpleadoEditor> conceptosEmpleadoEditando = [];
    private ConceptoEmpleadoEditor conceptoEmpleadoEnCaptura = CrearConceptoEmpleadoVacio();
    private string conceptoEmpleadoTipoSeleccionadoId = string.Empty;
    private HashSet<Guid> empleadosSeleccionados = [];
    private string filtroTexto = string.Empty;
    private Guid? filtroTurnoId;
    private string? filtroDepartamento;
    private string ordenColumna = "Nombre";
    private bool ordenAscendente = true;
    private Empleado editando = new();
    private Guid? esquemaSeleccionadoId;
    private Guid? turnoSeleccionadoId;
    private Guid? turnoBulkSeleccionadoId;
    private DateTime esquemaVigenteDesde = DateTime.Today;
    private DateTime turnoVigenteDesde = DateTime.Today;
    private DateTime? turnoVigenteHasta;
    private decimal? esquemaSueldoOverride;
    private List<HistorialEsquemaVm> historialEsquemas = [];
    private List<HistorialTurnoVm> historialTurnos = [];
    private string? turnoObservaciones;
    private bool mostrarForm, guardando;
    private string? error, ok;
    private bool _puedeVer;
    private bool _puedeEditar;
    private Guid _empresaId;
    private void OrdenarNoEmpleado() => AlternarOrden("NoEmpleado");
    private void OrdenarCodigoChecador() => AlternarOrden("CodigoChecador");
    private void OrdenarNombre() => AlternarOrden("Nombre");
    private void OrdenarPosicion() => AlternarOrden("Posicion");
    private void OrdenarDepartamento() => AlternarOrden("Departamento");
    private void OrdenarSueldo() => AlternarOrden("Sueldo");
    private void OrdenarPeriodicidad() => AlternarOrden("Periodicidad");
    private void OrdenarFechaContratacion() => AlternarOrden("FechaContratacion");
    private void OrdenarActivo() => AlternarOrden("Activo");

    private string IconoNoEmpleado => IconoOrden("NoEmpleado");
    private string IconoCodigoChecador => IconoOrden("CodigoChecador");
    private string IconoNombre => IconoOrden("Nombre");
    private string IconoPosicion => IconoOrden("Posicion");
    private string IconoDepartamento => IconoOrden("Departamento");
    private string IconoSueldo => IconoOrden("Sueldo");
    private string IconoPeriodicidad => IconoOrden("Periodicidad");
    private string IconoFechaContratacion => IconoOrden("FechaContratacion");
    private string IconoActivo => IconoOrden("Activo");

    private bool EstanTodosSeleccionados => ListaFiltrada.Any() && empleadosSeleccionados.Count == ListaFiltrada.Count();

    private IEnumerable<Empleado> ListaFiltrada
    {
        get
        {
            var query = lista.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(filtroTexto))
            {
                var termino = filtroTexto.Trim().ToLowerInvariant();
                query = query.Where(e =>
                    (e.Nombre ?? "").ToLowerInvariant().Contains(termino) ||
                    (e.ApellidoPaterno ?? "").ToLowerInvariant().Contains(termino) ||
                    (e.ApellidoMaterno ?? "").ToLowerInvariant().Contains(termino) ||
                    (e.NumeroEmpleado ?? "").ToLowerInvariant().Contains(termino) ||
                    (e.CodigoChecador ?? "").ToLowerInvariant().Contains(termino));
            }

            if (filtroTurnoId.HasValue && filtroTurnoId.Value != Guid.Empty)
            {
                query = query.Where(e => e.TurnoBaseId == filtroTurnoId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filtroDepartamento))
            {
                var depto = filtroDepartamento.Trim();
                query = query.Where(e => string.Equals(e.Departamento, depto, StringComparison.OrdinalIgnoreCase));
            }

            query = ordenColumna switch
            {
                "NoEmpleado" => ordenAscendente
                    ? query.OrderBy(e => e.NumeroEmpleado, StringComparer.OrdinalIgnoreCase)
                    : query.OrderByDescending(e => e.NumeroEmpleado, StringComparer.OrdinalIgnoreCase),
                "CodigoChecador" => ordenAscendente
                    ? query.OrderBy(e => e.CodigoChecador, StringComparer.OrdinalIgnoreCase)
                    : query.OrderByDescending(e => e.CodigoChecador, StringComparer.OrdinalIgnoreCase),
                "Nombre" => ordenAscendente
                    ? query.OrderBy(e => e.Nombre).ThenBy(e => e.ApellidoPaterno).ThenBy(e => e.ApellidoMaterno)
                    : query.OrderByDescending(e => e.Nombre).ThenByDescending(e => e.ApellidoPaterno).ThenByDescending(e => e.ApellidoMaterno),
                "Posicion" => ordenAscendente
                    ? query.OrderBy(e => ObtenerNombreActividad(e), StringComparer.OrdinalIgnoreCase)
                    : query.OrderByDescending(e => ObtenerNombreActividad(e), StringComparer.OrdinalIgnoreCase),
                "Departamento" => ordenAscendente
                    ? query.OrderBy(e => e.Departamento, StringComparer.OrdinalIgnoreCase)
                    : query.OrderByDescending(e => e.Departamento, StringComparer.OrdinalIgnoreCase),
                "Sueldo" => ordenAscendente
                    ? query.OrderBy(e => e.SueldoSemanal)
                    : query.OrderByDescending(e => e.SueldoSemanal),
                "Periodicidad" => ordenAscendente
                    ? query.OrderBy(e => e.PeriodicidadPago)
                    : query.OrderByDescending(e => e.PeriodicidadPago),
                "FechaContratacion" => ordenAscendente
                    ? query.OrderBy(e => e.FechaContratacion)
                    : query.OrderByDescending(e => e.FechaContratacion),
                "Activo" => ordenAscendente
                    ? query.OrderBy(e => e.IsActive)
                    : query.OrderByDescending(e => e.IsActive),
                _ => query.OrderBy(e => e.Nombre).ThenBy(e => e.ApellidoPaterno)
            };

            return query;
        }
    }

    private void AlternarOrden(string columna)
    {
        if (ordenColumna == columna)
        {
            ordenAscendente = !ordenAscendente;
        }
        else
        {
            ordenColumna = columna;
            ordenAscendente = true;
        }
    }

    private string IconoOrden(string columna) => ordenColumna == columna
        ? (ordenAscendente ? "bi bi-sort-up" : "bi bi-sort-down")
        : "bi bi-sort-down opacity-25";

    private string FiltroTurnoSeleccionado
    {
        get => filtroTurnoId?.ToString() ?? string.Empty;
        set => filtroTurnoId = Guid.TryParse(value, out var id) && id != Guid.Empty ? id : null;
    }

    protected override async Task OnInitializedAsync()
    {
        await CargarAcceso();
        if (_puedeVer)
        {
            await CargarPosiciones();
            await CargarDepartamentos();
            await CargarEsquemas();
            await CargarTurnos();
            await CargarConceptosNomina();
            await Cargar();
        }
    }

    private async Task CargarPosiciones()
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        posiciones = await db.Posiciones
            .AsNoTracking()
            .Where(a => a.IsActive)
            .OrderBy(a => a.Orden)
            .ThenBy(a => a.Nombre)
            .ToListAsync();
    }

    private async Task CargarDepartamentos()
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        departamentos = await db.DepartamentosRrhh
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.Orden)
            .ThenBy(d => d.Nombre)
            .ToListAsync();
    }

    private IEnumerable<string> DepartamentosDisponibles
        => departamentos.Select(d => d.Nombre)
            .Append(editando.Departamento ?? string.Empty)
            .Where(nombre => !string.IsNullOrWhiteSpace(nombre))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(nombre => nombre);

    private async Task CargarEsquemas()
    {
        try
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            esquemasPago = await db.EsquemasPago.AsNoTracking().OrderBy(e => e.Nombre).ToListAsync();
        }
        catch { esquemasPago = []; }
    }

    private async Task CargarTurnos()
    {
        try
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            turnosBase = await db.TurnosBase
                .AsNoTracking()
                .OrderByDescending(t => t.IsActive)
                .ThenBy(t => t.Nombre)
                .ToListAsync();
        }
        catch
        {
            turnosBase = [];
        }
    }

    private async Task CargarConceptosNomina()
    {
        try
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            var conceptosExistentes = await db.NominasConceptosConfigRrhh
                .AsNoTracking()
                .Where(c => c.EmpresaId == _empresaId && c.IsActive)
                .Select(c => c.Clave)
                .ToListAsync();

            var clavesBaseRequeridas = new[]
            {
                "DESCUENTO_FIJO",
                "IMSS_FIJO_OBRERO",
                "IMSS_FIJO_PATRONAL",
                "ISR_FIJO",
                "PERCEPCION_RECURRENTE",
                "PREMIO_PUNTUALIDAD_REC",
                "PREMIO_ASISTENCIA_REC"
            };

            if (_empresaId != Guid.Empty && clavesBaseRequeridas.Any(clave => !conceptosExistentes.Contains(clave, StringComparer.OrdinalIgnoreCase)))
            {
                var conceptosBase = new[]
                {
                    new NominaConceptoConfigRrhh
                    {
                        Id = Guid.NewGuid(), EmpresaId = _empresaId, Clave = "DESCUENTO_FIJO", Nombre = "Descuento fijo empleado",
                        Naturaleza = NaturalezaConceptoNominaRrhh.Deduccion, Destino = DestinoConceptoNominaRrhh.Empleado, TipoCalculo = TipoCalculoConceptoNominaRrhh.MontoFijo,
                        Orden = 1, EsRecurrente = true, AplicaPorEmpleado = true, AfectaNetoEmpleado = true, AfectaCostoEmpresa = false,
                        AfectaPasivoSat = false, AfectaPasivoImss = false, AfectaProvision = false, AfectaBaseIsr = false, AfectaBaseImss = false,
                        EsLegal = false, Observaciones = "Descuento fijo recurrente aplicado directo al neto del trabajador.", CreatedAt = DateTime.UtcNow, IsActive = true
                    },
                    new NominaConceptoConfigRrhh
                    {
                        Id = Guid.NewGuid(), EmpresaId = _empresaId, Clave = "IMSS_FIJO_OBRERO", Nombre = "IMSS fijo obrero",
                        Naturaleza = NaturalezaConceptoNominaRrhh.Obligacion, Destino = DestinoConceptoNominaRrhh.Imss, TipoCalculo = TipoCalculoConceptoNominaRrhh.MontoFijo,
                        Orden = 5, EsRecurrente = true, AplicaPorEmpleado = true, AfectaNetoEmpleado = true, AfectaCostoEmpresa = false,
                        AfectaPasivoSat = false, AfectaPasivoImss = true, AfectaProvision = false, AfectaBaseIsr = false, AfectaBaseImss = false,
                        EsLegal = false, Observaciones = "Ajuste fijo de IMSS obrero capturado por empleado.", CreatedAt = DateTime.UtcNow, IsActive = true
                    },
                    new NominaConceptoConfigRrhh
                    {
                        Id = Guid.NewGuid(), EmpresaId = _empresaId, Clave = "IMSS_FIJO_PATRONAL", Nombre = "IMSS fijo patronal",
                        Naturaleza = NaturalezaConceptoNominaRrhh.Obligacion, Destino = DestinoConceptoNominaRrhh.Imss, TipoCalculo = TipoCalculoConceptoNominaRrhh.MontoFijo,
                        Orden = 6, EsRecurrente = true, AplicaPorEmpleado = true, AfectaNetoEmpleado = false, AfectaCostoEmpresa = true,
                        AfectaPasivoSat = false, AfectaPasivoImss = true, AfectaProvision = false, AfectaBaseIsr = false, AfectaBaseImss = false,
                        EsLegal = false, Observaciones = "Ajuste fijo de IMSS patronal capturado por empleado.", CreatedAt = DateTime.UtcNow, IsActive = true
                    },
                    new NominaConceptoConfigRrhh
                    {
                        Id = Guid.NewGuid(), EmpresaId = _empresaId, Clave = "ISR_FIJO", Nombre = "ISR fijo",
                        Naturaleza = NaturalezaConceptoNominaRrhh.Obligacion, Destino = DestinoConceptoNominaRrhh.Sat, TipoCalculo = TipoCalculoConceptoNominaRrhh.MontoFijo,
                        Orden = 7, EsRecurrente = true, AplicaPorEmpleado = true, AfectaNetoEmpleado = true, AfectaCostoEmpresa = false,
                        AfectaPasivoSat = true, AfectaPasivoImss = false, AfectaProvision = false, AfectaBaseIsr = false, AfectaBaseImss = false,
                        EsLegal = false, Observaciones = "Ajuste fijo de ISR capturado por empleado.", CreatedAt = DateTime.UtcNow, IsActive = true
                    },
                    new NominaConceptoConfigRrhh
                    {
                        Id = Guid.NewGuid(), EmpresaId = _empresaId, Clave = "PERCEPCION_RECURRENTE", Nombre = "Percepción recurrente",
                        Naturaleza = NaturalezaConceptoNominaRrhh.Percepcion, Destino = DestinoConceptoNominaRrhh.Empleado, TipoCalculo = TipoCalculoConceptoNominaRrhh.MontoFijo,
                        Orden = 13, EsRecurrente = true, AplicaPorEmpleado = true, AfectaNetoEmpleado = true, AfectaCostoEmpresa = true,
                        AfectaPasivoSat = false, AfectaPasivoImss = false, AfectaProvision = false, AfectaBaseIsr = true, AfectaBaseImss = true,
                        EsLegal = false, Observaciones = "Percepción recurrente genérica para el empleado; se refleja en el recibo como otros ingresos por salarios.", CreatedAt = DateTime.UtcNow, IsActive = true
                    },
                    new NominaConceptoConfigRrhh
                    {
                        Id = Guid.NewGuid(), EmpresaId = _empresaId, Clave = "PREMIO_PUNTUALIDAD_REC", Nombre = "Premio puntualidad recurrente",
                        Naturaleza = NaturalezaConceptoNominaRrhh.Percepcion, Destino = DestinoConceptoNominaRrhh.Empleado, TipoCalculo = TipoCalculoConceptoNominaRrhh.MontoFijo,
                        Orden = 14, EsRecurrente = true, AplicaPorEmpleado = true, AfectaNetoEmpleado = true, AfectaCostoEmpresa = true,
                        AfectaPasivoSat = false, AfectaPasivoImss = false, AfectaProvision = false, AfectaBaseIsr = true, AfectaBaseImss = true,
                        EsLegal = false, Observaciones = "Percepción recurrente para premio de puntualidad del empleado.", CreatedAt = DateTime.UtcNow, IsActive = true
                    },
                    new NominaConceptoConfigRrhh
                    {
                        Id = Guid.NewGuid(), EmpresaId = _empresaId, Clave = "PREMIO_ASISTENCIA_REC", Nombre = "Premio asistencia recurrente",
                        Naturaleza = NaturalezaConceptoNominaRrhh.Percepcion, Destino = DestinoConceptoNominaRrhh.Empleado, TipoCalculo = TipoCalculoConceptoNominaRrhh.MontoFijo,
                        Orden = 15, EsRecurrente = true, AplicaPorEmpleado = true, AfectaNetoEmpleado = true, AfectaCostoEmpresa = true,
                        AfectaPasivoSat = false, AfectaPasivoImss = false, AfectaProvision = false, AfectaBaseIsr = true, AfectaBaseImss = true,
                        EsLegal = false, Observaciones = "Percepción recurrente para premio de asistencia del empleado.", CreatedAt = DateTime.UtcNow, IsActive = true
                    }
                };

                var faltantes = conceptosBase
                    .Where(baseConcepto => !conceptosExistentes.Contains(baseConcepto.Clave, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                if (faltantes.Count > 0)
                {
                    db.NominasConceptosConfigRrhh.AddRange(faltantes);
                    await db.SaveChangesAsync();
                }
            }

            conceptosNominaDisponibles = await db.NominasConceptosConfigRrhh
                .AsNoTracking()
                .Where(c => c.EmpresaId == _empresaId && c.IsActive && c.AplicaPorEmpleado)
                .OrderBy(c => c.Orden)
                .ThenBy(c => c.Nombre)
                .ToListAsync();
        }
        catch
        {
            conceptosNominaDisponibles = [];
        }
    }

    private async Task CargarAcceso()
    {
        var state = await AuthStateProvider.GetAuthenticationStateAsync();
        _puedeVer = state.User.HasClaim("Capacidad", "empleados.ver");
        _puedeEditar = state.User.HasClaim("Capacidad", "empleados.editar");
        _ = Guid.TryParse(state.User.FindFirst("EmpresaId")?.Value, out _empresaId);
    }

    private async Task Cargar()
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        lista = await db.Empleados
            .Include(e => e.TurnoBase)
            .AsNoTracking()
            .OrderBy(e => e.Nombre)
            .ToListAsync();

        try
        {
            var hoy = DateTime.Today;
            var asignaciones = await db.EmpleadosEsquemaPago
                .Include(a => a.EsquemaPago)
                .AsNoTracking()
                .Where(a => a.VigenteDesde <= hoy && (a.VigenteHasta == null || a.VigenteHasta >= hoy))
                .ToListAsync();
            esquemaActivoPorEmpleado = asignaciones
                .GroupBy(a => a.EmpleadoId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.VigenteDesde).First().EsquemaPago.Nombre);
        }
        catch { esquemaActivoPorEmpleado = new(); }

        await CargarSaldosEmpleadoAsync(db);
    }

    private async Task CargarSaldosEmpleadoAsync(CrmDbContext db)
    {
        var configuracionNomina = await NominaConfiguracionLoader.LoadAsync(db, _empresaId);

        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var idsEmpleado = lista.Select(e => e.Id).ToList();

        var vacacionesUsadas = await db.RrhhAusencias
            .AsNoTracking()
            .Where(a => idsEmpleado.Contains(a.EmpleadoId)
                && a.IsActive
                && a.Tipo == TipoAusenciaRrhh.Vacaciones
                && (a.Estatus == EstatusAusenciaRrhh.Aprobada || a.Estatus == EstatusAusenciaRrhh.Aplicada)
                && a.FechaFin <= hoy)
            .ToListAsync();

        bancoHorasPorEmpleado = await db.RrhhBancoHorasMovimientos
            .AsNoTracking()
            .Where(m => idsEmpleado.Contains(m.EmpleadoId) && m.IsActive)
            .GroupBy(m => m.EmpleadoId)
            .ToDictionaryAsync(g => g.Key, g => g.Sum(m => m.Horas));

        vacacionesDisponiblesPorEmpleado = lista.ToDictionary(
            e => e.Id,
            e => CalcularSaldoVacaciones(e, configuracionNomina, vacacionesUsadas.Where(a => a.EmpleadoId == e.Id).ToList()));
    }

    private decimal CalcularSaldoVacaciones(Empleado empleado, NominaConfiguracion configuracionNomina, IReadOnlyCollection<RrhhAusencia> vacacionesUsadas)
    {
        var ciclo = NominaLegalPolicy.ObtenerCicloVacacional(empleado, DateTime.Today, configuracionNomina);
        if (!ciclo.TieneDerechoVacaciones)
            return 0m;

        var usadasEnCiclo = vacacionesUsadas.Sum(a => CalcularDiasAusenciaEnRango(a, DateOnly.FromDateTime(ciclo.InicioCiclo), DateOnly.FromDateTime(ciclo.FinCiclo)));
        return NominaLegalPolicy.CalcularDiasVacacionesDisponibles(empleado, DateTime.Today, usadasEnCiclo, configuracionNomina);
    }

    private static int CalcularDiasAusenciaEnRango(RrhhAusencia ausencia, DateOnly inicio, DateOnly fin)
    {
        var inicioReal = ausencia.FechaInicio > inicio ? ausencia.FechaInicio : inicio;
        var finReal = ausencia.FechaFin < fin ? ausencia.FechaFin : fin;
        if (finReal < inicioReal)
            return 0;

        return (finReal.DayNumber - inicioReal.DayNumber) + 1;
    }

    private decimal ObtenerSaldoVacaciones(Guid empleadoId)
        => empleadoId != Guid.Empty && vacacionesDisponiblesPorEmpleado.TryGetValue(empleadoId, out var saldo) ? saldo : 0m;

    private decimal ObtenerSaldoBancoHoras(Guid empleadoId)
        => empleadoId != Guid.Empty && bancoHorasPorEmpleado.TryGetValue(empleadoId, out var saldo) ? saldo : 0m;

    private static string FormatearHorasBanco(decimal horas)
        => $"{horas:0.##} h";

    private async Task Nuevo()
    {
        if (!_puedeEditar)
        {
            error = "No tienes permisos para crear empleados.";
            return;
        }

        error = ok = null;
        var identificadores = _empresaId == Guid.Empty
            ? (Codigo: string.Empty, NumeroEmpleado: string.Empty)
            : await CodigoNegocio.GetNextEmployeeIdentifiersAsync(_empresaId);

        editando = new()
        {
            Id = Guid.Empty,
            Codigo = identificadores.Codigo,
            NumeroEmpleado = string.Empty,
            AplicaIsr = false,
            AplicaInfonavit = false,
            IsActive = true,
            FechaContratacion = DateTime.Today
        };
        esquemaSeleccionadoId = null;
        turnoSeleccionadoId = null;
        esquemaVigenteDesde = DateTime.Today;
        turnoVigenteDesde = DateTime.Today;
        turnoVigenteHasta = null;
        turnoObservaciones = null;
        esquemaSueldoOverride = null;
        historialEsquemas = [];
        historialTurnos = [];
        conceptosEmpleadoEditando = [];
        conceptoEmpleadoEnCaptura = CrearConceptoEmpleadoVacio();
        conceptoEmpleadoTipoSeleccionadoId = string.Empty;
        mostrarForm = true;
    }

    private async Task Editar(Empleado e)
    {
        if (!_puedeEditar)
        {
            error = "No tienes permisos para editar empleados.";
            return;
        }

        error = ok = null;
        editando = new()
        {
            Id = e.Id, Codigo = e.Codigo, NumeroEmpleado = e.NumeroEmpleado, CodigoChecador = e.CodigoChecador, Nombre = e.Nombre, ApellidoPaterno = e.ApellidoPaterno,
            ApellidoMaterno = e.ApellidoMaterno, Curp = e.Curp, Nss = e.Nss,
            Telefono = e.Telefono, Email = e.Email, Direccion = e.Direccion,
            FechaNacimiento = e.FechaNacimiento, FechaContratacion = e.FechaContratacion,
            PosicionId = e.PosicionId,
            Puesto = e.Puesto, Departamento = e.Departamento,
            TurnoBaseId = e.TurnoBaseId,
            SueldoSemanal = e.SueldoSemanal, PeriodicidadPago = e.PeriodicidadPago, TipoNomina = e.TipoNomina,
            AplicaImss = e.AplicaImss, AplicaIsr = e.AplicaIsr, AplicaInfonavit = e.AplicaInfonavit, NumeroCreditoInfonavit = e.NumeroCreditoInfonavit,
            FactorDescuentoInfonavit = e.FactorDescuentoInfonavit, IsActive = e.IsActive, Notas = e.Notas
        };

        turnoSeleccionadoId = e.TurnoBaseId;
        await CargarHistorialEsquemas(e.Id);
        await CargarHistorialTurnos(e.Id);
        await CargarConceptosEmpleado(e.Id);
        mostrarForm = true;
    }

    private async Task CargarConceptosEmpleado(Guid empleadoId)
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        var conceptos = await db.EmpleadosConceptosRrhh
            .Include(c => c.ConceptoConfig)
            .AsNoTracking()
            .Where(c => c.EmpleadoId == empleadoId && c.IsActive)
            .OrderBy(c => c.ConceptoConfig.Orden)
            .ThenBy(c => c.ConceptoConfig.Nombre)
            .ToListAsync();

        conceptosEmpleadoEditando = conceptos.Select(c => new ConceptoEmpleadoEditor
        {
            Id = c.Id,
            ConceptoConfigId = c.ConceptoConfigId,
            ConceptoNombre = c.ConceptoConfig.Nombre,
            Naturaleza = c.ConceptoConfig.Naturaleza,
            Destino = c.ConceptoConfig.Destino,
            Orden = c.ConceptoConfig.Orden,
            Monto = c.Monto,
            Porcentaje = c.Porcentaje,
            Cantidad = c.Cantidad,
            Tarifa = c.Tarifa,
            Saldo = c.Saldo,
            Limite = c.Limite,
            FechaInicio = c.FechaInicio,
            FechaFin = c.FechaFin,
            EsRecurrente = c.EsRecurrente,
            Observaciones = c.Observaciones
        }).ToList();

        conceptoEmpleadoEnCaptura = CrearConceptoEmpleadoVacio();
        conceptoEmpleadoTipoSeleccionadoId = string.Empty;
    }

    private async Task CargarHistorialEsquemas(Guid empleadoId)
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        var asignaciones = await db.EmpleadosEsquemaPago
            .Include(a => a.EsquemaPago)
            .AsNoTracking()
            .Where(a => a.EmpleadoId == empleadoId)
            .OrderByDescending(a => a.VigenteDesde)
            .ToListAsync();

        historialEsquemas = asignaciones.Select(a => new HistorialEsquemaVm
        {
            NombreEsquema = a.EsquemaPago.Nombre,
            Desde = a.VigenteDesde,
            Hasta = a.VigenteHasta,
            Override = a.SueldoBaseOverride
        }).ToList();

        var vigente = asignaciones.FirstOrDefault(a => a.VigenteHasta == null || a.VigenteHasta >= DateTime.Today);
        esquemaSeleccionadoId = vigente?.EsquemaPagoId;
        esquemaVigenteDesde = vigente?.VigenteDesde ?? DateTime.Today;
        esquemaSueldoOverride = vigente?.SueldoBaseOverride;
    }

    private async Task CargarHistorialTurnos(Guid empleadoId)
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        var vigencias = await db.RrhhEmpleadosTurno
            .Include(v => v.TurnoBase)
            .AsNoTracking()
            .Where(v => v.EmpleadoId == empleadoId)
            .OrderByDescending(v => v.VigenteDesde)
            .ToListAsync();

        historialTurnos = vigencias.Select(v => new HistorialTurnoVm
        {
            NombreTurno = v.TurnoBase.Nombre,
            Desde = v.VigenteDesde,
            Hasta = v.VigenteHasta,
            Observaciones = v.Observaciones
        }).ToList();

        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var vigente = vigencias
            .FirstOrDefault(v => v.VigenteDesde <= hoy && (v.VigenteHasta == null || v.VigenteHasta >= hoy));

        turnoSeleccionadoId = vigente?.TurnoBaseId ?? editando.TurnoBaseId;
        turnoVigenteDesde = vigente?.VigenteDesde.ToDateTime(TimeOnly.MinValue)
            ?? editando.FechaContratacion?.Date
            ?? DateTime.Today;
        turnoVigenteHasta = vigente?.VigenteHasta?.ToDateTime(TimeOnly.MinValue);
        turnoObservaciones = vigente?.Observaciones;
    }

    private async Task Guardar()
    {
        error = ok = null; guardando = true;
        try
        {
            if (!_puedeEditar)
            {
                error = "No tienes permisos para guardar empleados.";
                return;
            }

            if (string.IsNullOrWhiteSpace(editando.Nombre))
            { error = "El nombre es requerido."; return; }

            var empresaId = _empresaId;
            if (empresaId == Guid.Empty) { error = "No hay empresa activa."; return; }

            await using var db = await DbFactory.CreateDbContextAsync();
            if (editando.Id == Guid.Empty && string.IsNullOrWhiteSpace(editando.Codigo))
            {
                var identificadores = await CodigoNegocio.GetNextEmployeeIdentifiersAsync(empresaId);
                editando.Codigo = identificadores.Codigo;
            }

            var numeroEmpleado = editando.NumeroEmpleado.Trim();
            if (string.IsNullOrWhiteSpace(numeroEmpleado))
            {
                error = "El número de empleado es requerido.";
                return;
            }

            var codigoChecador = string.IsNullOrWhiteSpace(editando.CodigoChecador)
                ? null
                : editando.CodigoChecador.Trim();

            var numeroDuplicado = await db.Empleados
                .AnyAsync(e => e.EmpresaId == empresaId && e.NumeroEmpleado == numeroEmpleado && e.Id != editando.Id);
            if (numeroDuplicado)
            {
                error = "Ya existe un empleado con ese número de empleado.";
                return;
            }

            if (!string.IsNullOrWhiteSpace(codigoChecador))
            {
                var codigoChecadorDuplicado = await db.Empleados
                    .AnyAsync(e => e.EmpresaId == empresaId && e.CodigoChecador == codigoChecador && e.Id != editando.Id);
                if (codigoChecadorDuplicado)
                {
                    error = "Ya existe un empleado con ese código checador.";
                    return;
                }
            }

            Guid empId;
            if (editando.Id == Guid.Empty)
            {
                var nuevoEmpleado = new Empleado
                {
                    EmpresaId = empresaId, Codigo = editando.Codigo.Trim(), NumeroEmpleado = numeroEmpleado, Nombre = editando.Nombre.Trim(),
                    ApellidoPaterno = editando.ApellidoPaterno?.Trim(), ApellidoMaterno = editando.ApellidoMaterno?.Trim(),
                    Curp = editando.Curp?.Trim(), Nss = editando.Nss?.Trim(),
                    Telefono = editando.Telefono?.Trim(), Email = editando.Email?.Trim(),
                    Direccion = editando.Direccion?.Trim(), CodigoChecador = codigoChecador, FechaNacimiento = editando.FechaNacimiento,
                    FechaContratacion = editando.FechaContratacion, PosicionId = editando.PosicionId, Puesto = ObtenerNombrePosicion(editando.PosicionId),
                    Departamento = editando.Departamento?.Trim(), SueldoSemanal = editando.SueldoSemanal,
                    PeriodicidadPago = editando.PeriodicidadPago,
                    TipoNomina = editando.TipoNomina, AplicaImss = editando.AplicaImss, AplicaIsr = editando.AplicaIsr, AplicaInfonavit = editando.AplicaInfonavit,
                    NumeroCreditoInfonavit = editando.AplicaInfonavit ? editando.NumeroCreditoInfonavit?.Trim() : null,
                    FactorDescuentoInfonavit = editando.AplicaInfonavit ? editando.FactorDescuentoInfonavit : 0m,
                    IsActive = editando.IsActive, Notas = editando.Notas?.Trim()
                };

                db.Empleados.Add(nuevoEmpleado);
                empId = nuevoEmpleado.Id;
            }
            else
            {
                var ex = await db.Empleados.FirstOrDefaultAsync(e => e.Id == editando.Id);
                if (ex == null) return;
                ex.NumeroEmpleado = numeroEmpleado; ex.Nombre = editando.Nombre.Trim();
                ex.ApellidoPaterno = editando.ApellidoPaterno?.Trim(); ex.ApellidoMaterno = editando.ApellidoMaterno?.Trim();
                ex.Curp = editando.Curp?.Trim(); ex.Nss = editando.Nss?.Trim();
                ex.Telefono = editando.Telefono?.Trim(); ex.Email = editando.Email?.Trim();
                ex.Direccion = editando.Direccion?.Trim(); ex.CodigoChecador = codigoChecador; ex.FechaNacimiento = editando.FechaNacimiento;
                ex.FechaContratacion = editando.FechaContratacion; ex.PosicionId = editando.PosicionId; ex.Puesto = ObtenerNombrePosicion(editando.PosicionId);
                ex.Departamento = editando.Departamento?.Trim(); ex.SueldoSemanal = editando.SueldoSemanal;
                ex.PeriodicidadPago = editando.PeriodicidadPago;
                ex.TipoNomina = editando.TipoNomina; ex.AplicaImss = editando.AplicaImss; ex.AplicaIsr = editando.AplicaIsr; ex.AplicaInfonavit = editando.AplicaInfonavit;
                ex.NumeroCreditoInfonavit = editando.AplicaInfonavit ? editando.NumeroCreditoInfonavit?.Trim() : null;
                ex.FactorDescuentoInfonavit = editando.AplicaInfonavit ? editando.FactorDescuentoInfonavit : 0m; ex.IsActive = editando.IsActive;
                ex.Notas = editando.Notas?.Trim(); ex.UpdatedAt = DateTime.UtcNow;
                empId = ex.Id;
            }

            await db.SaveChangesAsync();

            await GuardarVigenciaTurnoAsync(
                db,
                empresaId,
                empId,
                turnoSeleccionadoId,
                DateOnly.FromDateTime(turnoVigenteDesde.Date),
                turnoVigenteHasta.HasValue ? DateOnly.FromDateTime(turnoVigenteHasta.Value.Date) : null,
                turnoObservaciones);

            await SincronizarTurnoBaseActualAsync(db, empresaId, empId);
            await db.SaveChangesAsync();

            await GuardarConceptosEmpleadoAsync(db, empId);
            await db.SaveChangesAsync();

            // Guardar asignación de esquema si se seleccionó uno
            if (esquemaSeleccionadoId.HasValue && esquemaSeleccionadoId.Value != Guid.Empty)
            {
                var asignacionExistente = await db.EmpleadosEsquemaPago
                    .FirstOrDefaultAsync(a => a.EmpleadoId == empId
                        && a.EsquemaPagoId == esquemaSeleccionadoId.Value
                        && (a.VigenteHasta == null || a.VigenteHasta >= DateTime.Today));

                if (asignacionExistente == null)
                {
                    // Cerrar asignación anterior
                    var anteriores = await db.EmpleadosEsquemaPago
                        .Where(a => a.EmpleadoId == empId && a.VigenteHasta == null)
                        .ToListAsync();
                    foreach (var ant in anteriores)
                    {
                        ant.VigenteHasta = esquemaVigenteDesde.AddDays(-1);
                        ant.UpdatedAt = DateTime.UtcNow;
                    }

                    db.EmpleadosEsquemaPago.Add(new EmpleadoEsquemaPago
                    {
                        EmpleadoId = empId,
                        EsquemaPagoId = esquemaSeleccionadoId.Value,
                        VigenteDesde = esquemaVigenteDesde,
                        SueldoBaseOverride = esquemaSueldoOverride > 0 ? esquemaSueldoOverride : null
                    });
                    await db.SaveChangesAsync();
                }
                else if (asignacionExistente.SueldoBaseOverride != esquemaSueldoOverride)
                {
                    asignacionExistente.SueldoBaseOverride = esquemaSueldoOverride > 0 ? esquemaSueldoOverride : null;
                    asignacionExistente.UpdatedAt = DateTime.UtcNow;
                    await db.SaveChangesAsync();
                }
            }

            ok = "Guardado."; mostrarForm = false; await Cargar();
        }
        catch (Exception ex) { error = ex.InnerException?.Message ?? ex.Message; }
        finally { guardando = false; }
    }

    private async Task GuardarConceptosEmpleadoAsync(CrmDbContext db, Guid empleadoId)
    {
        var existentes = await db.EmpleadosConceptosRrhh
            .Where(c => c.EmpleadoId == empleadoId)
            .ToListAsync();

        var idsEditor = conceptosEmpleadoEditando.Where(c => c.Id != Guid.Empty).Select(c => c.Id).ToHashSet();
        var eliminar = existentes.Where(c => !idsEditor.Contains(c.Id)).ToList();
        if (eliminar.Count > 0)
            db.EmpleadosConceptosRrhh.RemoveRange(eliminar);

        foreach (var item in conceptosEmpleadoEditando)
        {
            var entity = item.Id == Guid.Empty ? null : existentes.FirstOrDefault(c => c.Id == item.Id);
            if (entity == null)
            {
                entity = new EmpleadoConceptoRrhh
                {
                    Id = Guid.NewGuid(),
                    EmpresaId = _empresaId,
                    EmpleadoId = empleadoId,
                    ConceptoConfigId = item.ConceptoConfigId,
                    Monto = item.Monto,
                    Porcentaje = item.Porcentaje,
                    Cantidad = item.Cantidad,
                    Tarifa = item.Tarifa,
                    Saldo = item.Saldo,
                    Limite = item.Limite,
                    FechaInicio = item.FechaInicio,
                    FechaFin = item.FechaFin,
                    EsRecurrente = item.EsRecurrente,
                    Observaciones = string.IsNullOrWhiteSpace(item.Observaciones) ? null : item.Observaciones.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                db.EmpleadosConceptosRrhh.Add(entity);
                continue;
            }

            entity.ConceptoConfigId = item.ConceptoConfigId;
            entity.Monto = item.Monto;
            entity.Porcentaje = item.Porcentaje;
            entity.Cantidad = item.Cantidad;
            entity.Tarifa = item.Tarifa;
            entity.Saldo = item.Saldo;
            entity.Limite = item.Limite;
            entity.FechaInicio = item.FechaInicio;
            entity.FechaFin = item.FechaFin;
            entity.EsRecurrente = item.EsRecurrente;
            entity.Observaciones = string.IsNullOrWhiteSpace(item.Observaciones) ? null : item.Observaciones.Trim();
            entity.IsActive = true;
            entity.UpdatedAt = DateTime.UtcNow;
        }
    }

    private void PrepararNuevoConceptoEmpleado()
    {
        conceptoEmpleadoEnCaptura = CrearConceptoEmpleadoVacio();
        conceptoEmpleadoTipoSeleccionadoId = string.Empty;
        error = ok = null;
    }

    private void AgregarOActualizarConceptoEmpleado()
    {
        if (!Guid.TryParse(conceptoEmpleadoTipoSeleccionadoId, out var conceptoId))
        {
            error = "Selecciona un concepto configurable.";
            return;
        }

        var concepto = conceptosNominaDisponibles.FirstOrDefault(c => c.Id == conceptoId);
        if (concepto == null)
        {
            error = "No se encontró el concepto seleccionado.";
            return;
        }

        if (conceptosEmpleadoEditando.Any(c => c.Id != conceptoEmpleadoEnCaptura.Id && c.ConceptoConfigId == conceptoId))
        {
            error = "Ese concepto ya está asignado al empleado.";
            return;
        }

        if (conceptoEmpleadoEnCaptura.Id == Guid.Empty)
        {
            conceptosEmpleadoEditando.Add(new ConceptoEmpleadoEditor
            {
                Id = Guid.Empty,
                ConceptoConfigId = concepto.Id,
                ConceptoNombre = concepto.Nombre,
                Naturaleza = concepto.Naturaleza,
                Destino = concepto.Destino,
                Orden = concepto.Orden,
                Monto = conceptoEmpleadoEnCaptura.Monto,
                Porcentaje = conceptoEmpleadoEnCaptura.Porcentaje,
                Cantidad = conceptoEmpleadoEnCaptura.Cantidad,
                Tarifa = conceptoEmpleadoEnCaptura.Tarifa,
                Saldo = conceptoEmpleadoEnCaptura.Saldo,
                Limite = conceptoEmpleadoEnCaptura.Limite,
                FechaInicio = conceptoEmpleadoEnCaptura.FechaInicio,
                FechaFin = conceptoEmpleadoEnCaptura.FechaFin,
                EsRecurrente = conceptoEmpleadoEnCaptura.EsRecurrente,
                Observaciones = conceptoEmpleadoEnCaptura.Observaciones
            });
        }
        else
        {
            var actual = conceptosEmpleadoEditando.FirstOrDefault(c => c.Id == conceptoEmpleadoEnCaptura.Id && c.ConceptoConfigId == conceptoEmpleadoEnCaptura.ConceptoConfigId);
            if (actual == null)
                actual = conceptosEmpleadoEditando.FirstOrDefault(c => c.Id == conceptoEmpleadoEnCaptura.Id);
            if (actual == null)
                return;

            actual.ConceptoConfigId = concepto.Id;
            actual.ConceptoNombre = concepto.Nombre;
            actual.Naturaleza = concepto.Naturaleza;
            actual.Destino = concepto.Destino;
            actual.Orden = concepto.Orden;
            actual.Monto = conceptoEmpleadoEnCaptura.Monto;
            actual.Porcentaje = conceptoEmpleadoEnCaptura.Porcentaje;
            actual.Cantidad = conceptoEmpleadoEnCaptura.Cantidad;
            actual.Tarifa = conceptoEmpleadoEnCaptura.Tarifa;
            actual.Saldo = conceptoEmpleadoEnCaptura.Saldo;
            actual.Limite = conceptoEmpleadoEnCaptura.Limite;
            actual.FechaInicio = conceptoEmpleadoEnCaptura.FechaInicio;
            actual.FechaFin = conceptoEmpleadoEnCaptura.FechaFin;
            actual.EsRecurrente = conceptoEmpleadoEnCaptura.EsRecurrente;
            actual.Observaciones = conceptoEmpleadoEnCaptura.Observaciones;
        }

        conceptosEmpleadoEditando = conceptosEmpleadoEditando.OrderBy(c => c.Orden).ThenBy(c => c.ConceptoNombre).ToList();
        PrepararNuevoConceptoEmpleado();
    }

    private void EditarConceptoEmpleado(ConceptoEmpleadoEditor item)
    {
        conceptoEmpleadoEnCaptura = new ConceptoEmpleadoEditor
        {
            Id = item.Id,
            ConceptoConfigId = item.ConceptoConfigId,
            ConceptoNombre = item.ConceptoNombre,
            Naturaleza = item.Naturaleza,
            Destino = item.Destino,
            Orden = item.Orden,
            Monto = item.Monto,
            Porcentaje = item.Porcentaje,
            Cantidad = item.Cantidad,
            Tarifa = item.Tarifa,
            Saldo = item.Saldo,
            Limite = item.Limite,
            FechaInicio = item.FechaInicio,
            FechaFin = item.FechaFin,
            EsRecurrente = item.EsRecurrente,
            Observaciones = item.Observaciones
        };
        conceptoEmpleadoTipoSeleccionadoId = item.ConceptoConfigId.ToString();
        error = ok = null;
    }

    private void EliminarConceptoEmpleado(ConceptoEmpleadoEditor item)
    {
        conceptosEmpleadoEditando.RemoveAll(c => c == item || (item.Id != Guid.Empty && c.Id == item.Id));
        if (conceptoEmpleadoEnCaptura.Id == item.Id)
            PrepararNuevoConceptoEmpleado();
    }

    private async Task AplicarTurnoBulk()
    {
        if (!_puedeEditar)
        {
            error = "No tienes permisos para asignar turnos.";
            return;
        }

        if (empleadosSeleccionados.Count == 0)
        {
            error = "Selecciona al menos un empleado.";
            return;
        }

        error = ok = null;
        guardando = true;

        try
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            var empleados = await db.Empleados
                .Where(e => empleadosSeleccionados.Contains(e.Id))
                .ToListAsync();

            foreach (var empleado in empleados)
            {
                await GuardarVigenciaTurnoAsync(db, _empresaId, empleado.Id, turnoBulkSeleccionadoId, DateOnly.FromDateTime(DateTime.Today), null, "Asignación masiva desde RRHH/Empleados.");
                await SincronizarTurnoBaseActualAsync(db, _empresaId, empleado.Id);
            }

            await db.SaveChangesAsync();
            ok = turnoBulkSeleccionadoId.HasValue
                ? "Turno base aplicado a la selección."
                : "Se quitó el turno base de la selección.";
            empleadosSeleccionados.Clear();
            await Cargar();
        }
        catch (Exception ex)
        {
            error = ex.InnerException?.Message ?? ex.Message;
        }
        finally
        {
            guardando = false;
        }
    }

    private void AlternarSeleccionTodos()
    {
        if (EstanTodosSeleccionados)
        {
            empleadosSeleccionados.Clear();
            return;
        }

        empleadosSeleccionados = ListaFiltrada.Select(e => e.Id).ToHashSet();
    }

    private void AlternarSeleccionEmpleado(Guid empleadoId, object? value)
    {
        var seleccionado = value is bool boolValue && boolValue;
        if (seleccionado)
        {
            empleadosSeleccionados.Add(empleadoId);
            return;
        }

        empleadosSeleccionados.Remove(empleadoId);
    }

    public static async Task GuardarVigenciaTurnoAsync(CrmDbContext db, Guid empresaId, Guid empleadoId, Guid? turnoBaseId, DateOnly vigenteDesde, DateOnly? vigenteHasta, string? observaciones)
    {
        var empleado = await db.Empleados.FirstOrDefaultAsync(e => e.Id == empleadoId);
        if (empleado == null)
        {
            return;
        }

        var vigencias = await db.RrhhEmpleadosTurno
            .Where(v => v.EmpresaId == empresaId && v.EmpleadoId == empleadoId && v.IsActive)
            .OrderBy(v => v.VigenteDesde)
            .ToListAsync();

        if (vigencias.Count == 0 && empleado.TurnoBaseId.HasValue)
        {
            var inicioInicial = DateOnly.FromDateTime((empleado.FechaContratacion ?? DateTime.Today).Date);
            var vigenciaInicial = new RrhhEmpleadoTurno
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresaId,
                EmpleadoId = empleadoId,
                TurnoBaseId = empleado.TurnoBaseId.Value,
                VigenteDesde = inicioInicial,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            db.RrhhEmpleadosTurno.Add(vigenciaInicial);
            vigencias.Add(vigenciaInicial);
        }

        var observacionesLimpias = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim();
        var vigenteHastaNormalizado = vigenteHasta.HasValue && vigenteHasta.Value < vigenteDesde ? vigenteDesde : vigenteHasta;
        var siguienteVigencia = vigencias
            .Where(v => v.VigenteDesde > vigenteDesde)
            .OrderBy(v => v.VigenteDesde)
            .FirstOrDefault();
        var nuevoHasta = vigenteHastaNormalizado ?? siguienteVigencia?.VigenteDesde.AddDays(-1);
        var vigenciaExacta = vigencias.FirstOrDefault(v => v.VigenteDesde == vigenteDesde);

        foreach (var vigente in vigencias.Where(v => v.Id != vigenciaExacta?.Id && v.VigenteDesde <= vigenteDesde && (v.VigenteHasta == null || v.VigenteHasta >= vigenteDesde)))
        {
            vigente.VigenteHasta = vigenteDesde.AddDays(-1);
            vigente.UpdatedAt = DateTime.UtcNow;
        }

        if (!turnoBaseId.HasValue)
        {
            if (vigenciaExacta != null)
            {
                db.RrhhEmpleadosTurno.Remove(vigenciaExacta);
            }

            return;
        }

        if (vigenciaExacta != null)
        {
            vigenciaExacta.TurnoBaseId = turnoBaseId.Value;
            vigenciaExacta.VigenteHasta = nuevoHasta;
            vigenciaExacta.Observaciones = observacionesLimpias;
            vigenciaExacta.UpdatedAt = DateTime.UtcNow;
            return;
        }

        db.RrhhEmpleadosTurno.Add(new RrhhEmpleadoTurno
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            EmpleadoId = empleadoId,
            TurnoBaseId = turnoBaseId.Value,
            VigenteDesde = vigenteDesde,
            VigenteHasta = nuevoHasta,
            Observaciones = observacionesLimpias,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
    }

    public static async Task SincronizarTurnoBaseActualAsync(CrmDbContext db, Guid empresaId, Guid empleadoId)
    {
        var empleado = await db.Empleados.FirstOrDefaultAsync(e => e.Id == empleadoId);
        if (empleado == null)
        {
            return;
        }

        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var turnoVigente = await db.RrhhEmpleadosTurno
            .Where(v => v.EmpresaId == empresaId
                && v.EmpleadoId == empleadoId
                && v.IsActive
                && v.VigenteDesde <= hoy
                && (v.VigenteHasta == null || v.VigenteHasta >= hoy))
            .OrderByDescending(v => v.VigenteDesde)
            .FirstOrDefaultAsync();

        empleado.TurnoBaseId = turnoVigente?.TurnoBaseId;
        empleado.UpdatedAt = DateTime.UtcNow;
    }

    private string ObtenerEsquemaActivo(Guid empleadoId)
        => esquemaActivoPorEmpleado.TryGetValue(empleadoId, out var nombre) ? nombre : "—";

    private static string ObtenerEtiquetaTurno(TurnoBase turno)
        => turno.IsActive ? turno.Nombre : $"{turno.Nombre} (inactivo)";

    private string ObtenerNombreActividad(Empleado empleado)
        => ObtenerNombrePosicion(empleado.PosicionId) ?? empleado.Puesto ?? "—";

    private static string TipoEsquemaLabel(TipoEsquemaPago tipo) => tipo switch
    {
        TipoEsquemaPago.SueldoFijo => "Sueldo Fijo",
        TipoEsquemaPago.DestajoPorPieza => "Destajo/Pieza",
        TipoEsquemaPago.DestajoPorOperacion => "Destajo/Op",
        TipoEsquemaPago.BonoMetaPedidos => "Bono Meta",
        TipoEsquemaPago.Mixto => "Mixto",
        _ => tipo.ToString()
    };

    private string? ObtenerNombrePosicion(Guid? posicionId)
        => posicionId.HasValue
            ? posiciones.FirstOrDefault(a => a.Id == posicionId.Value)?.Nombre
            : null;

    private class HistorialEsquemaVm
    {
        public string NombreEsquema { get; set; } = "";
        public DateTime Desde { get; set; }
        public DateTime? Hasta { get; set; }
        public decimal? Override { get; set; }
    }

    private class HistorialTurnoVm
    {
        public string NombreTurno { get; set; } = "";
        public DateOnly Desde { get; set; }
        public DateOnly? Hasta { get; set; }
        public string? Observaciones { get; set; }
    }

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

    private static ConceptoEmpleadoEditor CrearConceptoEmpleadoVacio()
        => new() { EsRecurrente = true, FechaInicio = DateTime.Today };
}