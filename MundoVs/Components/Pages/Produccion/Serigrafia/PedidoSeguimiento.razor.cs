using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Entities.Inventario;
using MundoVs.Core.Entities.Serigrafia;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Components.Pages.Produccion.Serigrafia;

public partial class PedidoSeguimiento
{
    [Parameter] public Guid PedidoId { get; set; }

    private Pedido? pedido;
    private PedidoSerigrafia? pedidoSerigrafia;
    private List<PedidoDetalleTalla> detalleTallasPedido = new();
    private List<PedidoConcepto> conceptosPedido = new();
    private List<NotaEntrega> notasEntrega = new();
    private Dictionary<Guid, List<RegistroDestajoProceso>> destajosPorSeguimiento = new();
    private List<Empleado> empleadosActivos = new();
    private List<PedidoSerigrafiaTallaProceso> seguimientoTallaProcesos = new();
    private List<EntregaTallaEditor> corridaPedido = new();
    private DateTime fechaNotaEntrega = DateTime.Today;
    private int cantidadNotaEntrega;
    private string? observacionesNotaEntrega;
    private string textoPagareNota = string.Empty;
    private bool noRequiereFacturaNota;
    private bool aplicaIvaNota;
    private bool guardandoNota;
    private bool modalConfirmarNotaAbierto;
    private string? errorNota;
    private string? okNota;
    private Dictionary<Guid, decimal> existenciasFgPorDetalleTalla = new();
    private int piezasPedidoTotal;
    private int piezasEntregadas;
    private int piezasPendientes;
    private Guid empresaIdActual;
    private bool modalDestajoAbierto;
    private Guid? modalSeguimientoId;
    private bool modalConfirmarCheckAbierto;
    private Guid? confirmacionSeguimientoId;
    private bool modalConfirmarIngresoFgAbierto;
    private bool confirmacionIngresoFgEsUltimoSku;
    private string modalTallaNombre = string.Empty;
    private string modalProcesoNombre = string.Empty;
    private string confirmacionTallaNombre = string.Empty;
    private string confirmacionProcesoNombre = string.Empty;
    private FinishedGoodPendienteItem? confirmacionIngresoFgItem;
    private string? modalDestajoError;
    private bool puedeReabrirProcesos;
    private bool puedeIngresarFinishedGoods;
    private bool inventarioFinishedGoodsDisponible = true;
    private bool guardandoFg;
    private string? errorFg;
    private string? okFg;
    private HashSet<Guid> detalleTallasIngresadasFg = new();
    private readonly SemaphoreSlim _opLock = new(1, 1);

    // Stepper / secciones colapsables
    private bool seccionServiciosAbierta = true;
    private bool seccionFgAbierta = true;
    private bool seccionSkuAbierta = false;
    private bool seccionMatrizAbierta = true;
    private bool seccionEntregaAbierta = true;

    private int StepActivo()
    {
        if (pedidoSerigrafia == null) return 1;
        var procesos = pedidoSerigrafia.TiposProceso.Select(tp => tp.TipoProceso).ToList();
        var todasCompletas = pedidoSerigrafia.Tallas.Any() && pedidoSerigrafia.Tallas.All(t => EsTallaCompletada(t.Id, procesos));
        var hayFgPendiente = ObtenerSkusPendientesFinishedGoods().Any();
        var hayNotas = notasEntrega.Any();
        var todasEntregadas = piezasPendientes == 0 && hayNotas;

        if (todasEntregadas) return 4;
        if (!hayFgPendiente && todasCompletas) return 3;
        if (todasCompletas) return 3;
        return 2;
    }

    private PedidoSerigrafiaTallaProceso? modalSeguimiento
        => modalSeguimientoId.HasValue
            ? seguimientoTallaProcesos.FirstOrDefault(s => s.Id == modalSeguimientoId.Value)
            : null;

    private PedidoSerigrafiaTallaProceso? confirmacionSeguimiento
        => confirmacionSeguimientoId.HasValue
            ? seguimientoTallaProcesos.FirstOrDefault(s => s.Id == confirmacionSeguimientoId.Value)
            : null;

    protected override async Task OnInitializedAsync()
    {
        empresaIdActual = await ObtenerEmpresaActualIdAsync();
        puedeReabrirProcesos = await TieneCapacidadAsync("pedidos.produccion.reabrir");
        puedeIngresarFinishedGoods = await TieneCapacidadAsync("inventario.fg.ingresar") || await TieneCapacidadAsync("serigrafia.editar");
        pedido = await PedidoRepository.GetByIdAsync(PedidoId);
        conceptosPedido = pedido?.Conceptos.OrderBy(c => c.CreatedAt).ToList() ?? new();
        empleadosActivos = await DbContext.Empleados
            .AsNoTracking()
            .Where(e => e.IsActive)
            .OrderBy(e => e.Nombre)
            .ThenBy(e => e.ApellidoPaterno)
            .ToListAsync();
        await CargarSerigrafiaAsync();
        await CargarNotasEntregaAsync();
    }

    private bool EsTallaCompletada(Guid tallaId, IEnumerable<TipoProceso> procesos)
    {
        var procesoIds = procesos.Select(p => p.Id).ToList();
        return procesoIds.All(pid => seguimientoTallaProcesos.Any(s => s.PedidoSerigrafiaTallaId == tallaId && s.TipoProcesoId == pid && s.Completado));
    }

    private async Task CargarSerigrafiaAsync()
    {
        pedidoSerigrafia = await DbContext.PedidosSerigrafia
            .Include(p => p.PedidoDetalle)
                .ThenInclude(pd => pd.Pedido)
            .Include(p => p.PedidoDetalle)
                .ThenInclude(pd => pd.CotizacionSerigrafia)
                    .ThenInclude(c => c.Detalles)
            .Include(p => p.TiposProceso)
                .ThenInclude(tp => tp.TipoProceso)
                    .ThenInclude(t => t.Posicion)
            .Include(p => p.Tallas)
            .FirstOrDefaultAsync(p => p.PedidoDetalle.PedidoId == PedidoId);

        if (pedidoSerigrafia != null)
        {
            detalleTallasPedido = await DbContext.PedidosDetalleTalla
                .AsNoTracking()
                .Include(t => t.ProductoVariante)
                .Where(t => t.PedidoDetalleId == pedidoSerigrafia.PedidoDetalleId && t.IsActive)
                .OrderBy(t => t.Orden)
                .ThenBy(t => t.Talla)
                .ToListAsync();

            seguimientoTallaProcesos = await DbContext.PedidoSerigrafiaTallaProcesos
                .Include(s => s.Empleado)
                .Where(s => s.PedidoSerigrafiaId == pedidoSerigrafia.Id)
                .ToListAsync();

            destajosPorSeguimiento = await DbContext.RegistrosDestajoProceso
                .Where(r => r.PedidoSerigrafiaId == pedidoSerigrafia.Id)
                .OrderBy(r => r.Fecha)
                .ThenBy(r => r.CreatedAt)
                .GroupBy(r => r.PedidoSerigrafiaTallaProcesoId)
                .ToDictionaryAsync(g => g.Key, g => g.ToList());

            await CargarEstadoFinishedGoodsAsync();
        }
        else
        {
            detalleTallasPedido = new();
            destajosPorSeguimiento = new();
            existenciasFgPorDetalleTalla = new();
            detalleTallasIngresadasFg = new();
        }
    }

    private async Task CargarEstadoFinishedGoodsAsync()
    {
        if (pedidoSerigrafia == null)
        {
            inventarioFinishedGoodsDisponible = false;
            existenciasFgPorDetalleTalla = new();
            detalleTallasIngresadasFg = new();
            return;
        }

        try
        {
            var variantesIds = detalleTallasPedido
                .Where(t => t.ProductoVarianteId.HasValue)
                .Select(t => t.ProductoVarianteId!.Value)
                .Distinct()
                .ToList();

            var inventariosFg = variantesIds.Count == 0
                ? []
                : await DbContext.InventariosFinishedGoods
                    .AsNoTracking()
                    .Where(i => i.EmpresaId == empresaIdActual
                        && i.ClienteId == pedidoSerigrafia.PedidoDetalle.Pedido.ClienteId
                        && variantesIds.Contains(i.ProductoVarianteId)
                        && i.IsActive)
                    .ToListAsync();

            existenciasFgPorDetalleTalla = detalleTallasPedido.ToDictionary(
                t => t.Id,
                t => t.ProductoVarianteId.HasValue
                    ? inventariosFg.FirstOrDefault(i => i.ProductoVarianteId == t.ProductoVarianteId.Value)?.CantidadDisponible ?? 0m
                    : 0m);

            detalleTallasIngresadasFg = await DbContext.MovimientosFinishedGoods
                .Where(m => m.PedidoSerigrafiaId == pedidoSerigrafia.Id
                    && m.TipoMovimiento == MovimientoFinishedGoodTipoEnum.IngresoProduccion
                    && m.PedidoDetalleTallaId.HasValue
                    && m.IsActive)
                .Select(m => m.PedidoDetalleTallaId!.Value)
                .ToHashSetAsync();

            inventarioFinishedGoodsDisponible = true;
        }
        catch (Exception ex) when (EsTablaFinishedGoodsInexistente(ex))
        {
            inventarioFinishedGoodsDisponible = false;
            existenciasFgPorDetalleTalla = detalleTallasPedido.ToDictionary(t => t.Id, _ => 0m);
            detalleTallasIngresadasFg = new();
            errorFg ??= "El esquema de finished goods aún no existe en la base de datos. Aplica la migración pendiente para habilitar FG.";
        }
    }

    private async Task CargarNotasEntregaAsync()
    {
        notasEntrega = await DbContext.NotasEntrega
            .IgnoreAutoIncludes()
            .Include(n => n.Detalles)
                .ThenInclude(d => d.PedidoConcepto)
            .Include(n => n.Detalles)
                .ThenInclude(d => d.Tallas)
            .Include(n => n.FacturasRelacionadas)
                .ThenInclude(r => r.Factura)
            .Where(n => n.PedidoId == PedidoId && n.IsActive)
            .OrderByDescending(n => n.FechaNota)
            .ToListAsync();

        piezasPedidoTotal = ObtenerPiezasPedidoTotal();
        piezasEntregadas = notasEntrega.Sum(ObtenerCantidadNota);
        piezasPendientes = Math.Max(0, piezasPedidoTotal - piezasEntregadas);
        ConstruirCorridaEntrega();
    }

    private async Task OnCheckTallaProcesoClickAsync(PedidoSerigrafiaTallaProceso seg, string talla, string? proceso)
    {
        if (!PuedeEditarProcesosProduccion())
        {
            return;
        }

        var pendientesFgAntes = ObtenerIdsSkusPendientesFinishedGoods();

        if (seg.Completado)
        {
            await DesmarcarProcesoAsync(seg);
            return;
        }

        var cantidadObjetivo = ObtenerCantidadTallaSeguimiento(seg);
        if (cantidadObjetivo > 0 && ObtenerCantidadAsignada(seg.Id) >= cantidadObjetivo)
        {
            await _opLock.WaitAsync();
            try
            {
                await SincronizarCompletadoProcesoAsync(seg, await ObtenerUsuarioActualAsync());
                DbContext.PedidoSerigrafiaTallaProcesos.Update(seg);
                await DbContext.SaveChangesAsync();
            }
            finally
            {
                _opLock.Release();
            }

            await CargarSerigrafiaAsync();
            await ActualizarEstadoPedidoAsync();
            LanzarConfirmacionIngresoFinishedGoodsSiAplica(pendientesFgAntes);
            return;
        }

        AbrirModalConfirmacionCheck(seg, talla, proceso);
    }

    private async Task DesmarcarProcesoAsync(PedidoSerigrafiaTallaProceso seg)
    {
        await _opLock.WaitAsync();
        try
        {
            var usuarioActual = await ObtenerUsuarioActualAsync();
            var destajos = ObtenerDestajos(seg.Id);
            var autoDestajos = destajos.Where(d => d.IsActive && d.EmpleadoId == Guid.Empty).ToList();

            foreach (var autoDestajo in autoDestajos)
            {
                destajos.Remove(autoDestajo);
                if (autoDestajo.Id != Guid.Empty)
                {
                    DbContext.RegistrosDestajoProceso.Remove(autoDestajo);
                }
            }

            seg.Completado = false;
            if (!destajos.Any(d => d.IsActive))
            {
                seg.FechaPaso = null;
            }

            await SincronizarCompletadoProcesoAsync(seg, usuarioActual);
            DbContext.PedidoSerigrafiaTallaProcesos.Update(seg);
            await DbContext.SaveChangesAsync();
        }
        finally
        {
            _opLock.Release();
        }

        await CargarSerigrafiaAsync();
        await ActualizarEstadoPedidoAsync();
    }

    private void AbrirModalConfirmacionCheck(PedidoSerigrafiaTallaProceso seg, string talla, string? proceso)
    {
        confirmacionSeguimientoId = seg.Id;
        confirmacionTallaNombre = talla;
        confirmacionProcesoNombre = string.IsNullOrWhiteSpace(proceso) ? "Proceso" : proceso.Trim();
        modalConfirmarCheckAbierto = true;
    }

    private void CerrarModalConfirmacionCheck()
    {
        modalConfirmarCheckAbierto = false;
        confirmacionSeguimientoId = null;
        confirmacionTallaNombre = string.Empty;
        confirmacionProcesoNombre = string.Empty;
    }

    private async Task ConfirmarCheckConEmpleadosAsync()
    {
        if (!PuedeEditarProcesosProduccion())
        {
            return;
        }

        var seg = confirmacionSeguimiento;
        var talla = confirmacionTallaNombre;
        var proceso = confirmacionProcesoNombre;
        CerrarModalConfirmacionCheck();

        if (seg == null)
        {
            return;
        }

        if (!ObtenerDestajos(seg.Id).Any(d => d.IsActive))
        {
            await AgregarDestajo(seg);
        }

        AbrirModalDestajo(seg, talla, proceso);
    }

    private async Task ConfirmarCheckSinEmpleadosAsync()
    {
        if (!PuedeEditarProcesosProduccion())
        {
            return;
        }

        var seg = confirmacionSeguimiento;
        CerrarModalConfirmacionCheck();

        if (seg == null)
        {
            return;
        }

        await CompletarProcesoSinEmpleadosAsync(seg);
    }

    private async Task CompletarProcesoSinEmpleadosAsync(PedidoSerigrafiaTallaProceso seg)
    {
        if (!PuedeEditarProcesosProduccion())
        {
            return;
        }

        var pendientesFgAntes = ObtenerIdsSkusPendientesFinishedGoods();

        await _opLock.WaitAsync();
        try
        {
            var usuarioActual = await ObtenerUsuarioActualAsync();
            seg.EmpleadoId = ObtenerDestajos(seg.Id)
                .Where(d => d.IsActive && d.EmpleadoId != Guid.Empty)
                .Select(d => (Guid?)d.EmpleadoId)
                .FirstOrDefault();
            await SincronizarCompletadoProcesoAsync(seg, usuarioActual, forzarCompletado: true);
            DbContext.PedidoSerigrafiaTallaProcesos.Update(seg);
            await DbContext.SaveChangesAsync();
        }
        finally
        {
            _opLock.Release();
        }

        await CargarSerigrafiaAsync();
        await ActualizarEstadoPedidoAsync();
        LanzarConfirmacionIngresoFinishedGoodsSiAplica(pendientesFgAntes);
    }

    private async Task OnFechaTallaProceso(PedidoSerigrafiaTallaProceso seg, DateTime? value)
    {
        if (!PuedeEditarProcesosProduccion())
        {
            return;
        }

        await _opLock.WaitAsync();
        try
        {
            seg.FechaPaso = value;
            seg.UpdatedAt = DateTime.UtcNow;
            seg.UpdatedBy = await ObtenerUsuarioActualAsync();
            DbContext.PedidoSerigrafiaTallaProcesos.Update(seg);
            await DbContext.SaveChangesAsync();
            await ActualizarEstadoPedidoAsync();
        }
        finally
        {
            _opLock.Release();
        }
    }

    private List<RegistroDestajoProceso> ObtenerDestajos(Guid seguimientoId)
    {
        if (!destajosPorSeguimiento.TryGetValue(seguimientoId, out var destajos))
        {
            destajos = new();
            destajosPorSeguimiento[seguimientoId] = destajos;
        }

        return destajos;
    }

    private async Task AgregarDestajo(PedidoSerigrafiaTallaProceso seg)
    {
        if (!PuedeEditarProcesosProduccion())
        {
            return;
        }

        var cantidadDefault = pedidoSerigrafia?.Tallas.FirstOrDefault(t => t.Id == seg.PedidoSerigrafiaTallaId)?.Cantidad ?? 0;
        var proceso = pedidoSerigrafia?.TiposProceso
            .FirstOrDefault(tp => tp.TipoProcesoId == seg.TipoProcesoId)?
            .TipoProceso;
        var destajos = ObtenerDestajos(seg.Id);
        var cantidadNueva = 0;
        int? minutosDefault = null;
        var tarifaDefault = 0m;

        if (proceso != null && seg.EmpleadoId.HasValue && seg.EmpleadoId.Value != Guid.Empty)
        {
            var tarifaResultado = await ResolverTarifaDestajoAsync(seg.TipoProcesoId, seg.EmpleadoId.Value, cantidadDefault, seg.FechaPaso ?? DateTime.UtcNow);
            tarifaDefault = tarifaResultado.Tarifa;
        }
        else if (proceso != null)
        {
            tarifaDefault = ObtenerTarifaDestajoPorPieza(seg.TipoProcesoId, cantidadDefault);
        }

        destajos.Add(new RegistroDestajoProceso
        {
            PedidoSerigrafiaTallaProcesoId = seg.Id,
            PedidoSerigrafiaId = seg.PedidoSerigrafiaId,
            PedidoSerigrafiaTallaId = seg.PedidoSerigrafiaTallaId,
            TipoProcesoId = seg.TipoProcesoId,
            EmpleadoId = seg.EmpleadoId ?? Guid.Empty,
            Fecha = seg.FechaPaso ?? DateTime.UtcNow,
            CantidadProcesada = cantidadNueva,
            TarifaUnitario = tarifaDefault,
            TiempoMinutos = minutosDefault,
            IsActive = true
        });
    }

    private static string ObtenerEmpleadoValue(RegistroDestajoProceso destajo)
        => destajo.EmpleadoId == Guid.Empty ? string.Empty : destajo.EmpleadoId.ToString();

    private async Task OnDestajoEmpleadoChanged(RegistroDestajoProceso destajo, string? empleadoId)
    {
        if (!PuedeEditarProcesosProduccion())
        {
            return;
        }

        destajo.EmpleadoId = Guid.TryParse(empleadoId, out var empleadoGuid) ? empleadoGuid : Guid.Empty;

        if (destajo.EmpleadoId == Guid.Empty)
        {
            return;
        }

        var tallaCantidad = pedidoSerigrafia?.Tallas.FirstOrDefault(t => t.Id == destajo.PedidoSerigrafiaTallaId)?.Cantidad ?? 0;
        var resultado = await ResolverTarifaDestajoAsync(destajo.TipoProcesoId, destajo.EmpleadoId, tallaCantidad, destajo.Fecha);
        if (resultado.Tarifa > 0)
        {
            destajo.TarifaUnitario = resultado.Tarifa;
            destajo.Importe = CalcularImporteDestajo(destajo);
        }
    }

    private EventCallback<ChangeEventArgs> CrearCambioEmpleadoCallback(RegistroDestajoProceso destajo)
        => EventCallback.Factory.Create<ChangeEventArgs>(this, async e => await OnDestajoEmpleadoChanged(destajo, e.Value?.ToString()));

    private EventCallback CrearGuardarDestajoCallback(PedidoSerigrafiaTallaProceso seg, RegistroDestajoProceso destajo)
        => EventCallback.Factory.Create(this, () => GuardarDestajo(seg, destajo));

    private EventCallback CrearEliminarDestajoCallback(PedidoSerigrafiaTallaProceso seg, RegistroDestajoProceso destajo)
        => EventCallback.Factory.Create(this, () => EliminarDestajo(seg, destajo));

    private EventCallback CrearAgregarDestajoCallback(PedidoSerigrafiaTallaProceso seg)
        => EventCallback.Factory.Create(this, async () => await AgregarDestajo(seg));

    private void AbrirModalDestajo(PedidoSerigrafiaTallaProceso seg, string talla, string? proceso)
    {
        modalSeguimientoId = seg.Id;
        modalTallaNombre = talla;
        modalProcesoNombre = string.IsNullOrWhiteSpace(proceso) ? "Proceso" : proceso.Trim();
        modalDestajoError = null;
        modalDestajoAbierto = true;
    }

    private void CerrarModalDestajo()
    {
        modalDestajoAbierto = false;
        modalSeguimientoId = null;
        modalTallaNombre = string.Empty;
        modalProcesoNombre = string.Empty;
        modalDestajoError = null;
    }

    private async Task AgregarDestajoDesdeModal(PedidoSerigrafiaTallaProceso seg)
    {
        if (!PuedeEditarProcesosProduccion())
        {
            return;
        }

        if (ObtenerDestajos(seg.Id).Any(d => d.IsActive && d.Id == Guid.Empty && d.EmpleadoId == Guid.Empty && d.CantidadProcesada == 0))
        {
            modalDestajoError = "Completa o elimina el registro vacío antes de agregar otra persona.";
            return;
        }

        if (ObtenerCantidadDisponible(seg) <= 0)
        {
            modalDestajoError = "Ya no hay piezas disponibles para asignar en esta operación.";
            return;
        }

        await AgregarDestajo(seg);
        modalDestajoError = null;
        await InvokeAsync(StateHasChanged);
    }

    private void OnCantidadDestajoCapturada(PedidoSerigrafiaTallaProceso seg, RegistroDestajoProceso destajo)
    {
        var maximoDisponible = ObtenerCantidadDisponible(seg, destajo);

        if (destajo.CantidadProcesada < 0)
            destajo.CantidadProcesada = 0;

        if (destajo.CantidadProcesada > maximoDisponible)
        {
            destajo.CantidadProcesada = maximoDisponible;
            modalDestajoError = $"Solo puedes capturar hasta {maximoDisponible} pieza(s) disponibles en este registro.";
        }

        destajo.TiempoMinutos = destajo.CantidadProcesada > 0
            ? ObtenerMinutosCotizacion(seg.TipoProcesoId, ObtenerCantidadTallaSeguimiento(seg), destajo.CantidadProcesada)
            : null;
        destajo.Importe = CalcularImporteDestajo(destajo);
        modalDestajoError = null;
    }

    private int ObtenerCantidadAsignada(Guid seguimientoId)
    {
        var asignado = ObtenerDestajos(seguimientoId)
            .Where(d => d.IsActive)
            .Sum(d => Math.Max(0, d.CantidadProcesada));

        if (asignado > 0)
        {
            return asignado;
        }

        var seg = seguimientoTallaProcesos.FirstOrDefault(s => s.Id == seguimientoId);
        return seg?.Completado == true ? ObtenerCantidadTallaSeguimiento(seg) : 0;
    }

    private int ObtenerCantidadTallaSeguimiento(PedidoSerigrafiaTallaProceso seg)
        => pedidoSerigrafia?.Tallas.FirstOrDefault(t => t.Id == seg.PedidoSerigrafiaTallaId)?.Cantidad ?? 0;

    private int ObtenerCantidadDisponible(PedidoSerigrafiaTallaProceso seg, RegistroDestajoProceso? excludingDestajo = null)
    {
        var total = ObtenerCantidadTallaSeguimiento(seg);
        var asignado = ObtenerDestajos(seg.Id)
            .Where(d => d.IsActive)
            .Where(d => excludingDestajo == null
                || (excludingDestajo.Id != Guid.Empty
                    ? d.Id != excludingDestajo.Id
                    : !ReferenceEquals(d, excludingDestajo)))
            .Sum(d => Math.Max(0, d.CantidadProcesada));

        if (asignado == 0 && excludingDestajo == null && seg.Completado)
        {
            return 0;
        }

        return Math.Max(0, total - asignado);
    }

    private int ObtenerCantidadRestanteDespuesDeRegistro(PedidoSerigrafiaTallaProceso seg, RegistroDestajoProceso destajo)
        => Math.Max(0, ObtenerCantidadTallaSeguimiento(seg) - ObtenerCantidadDisponible(seg, destajo) - Math.Max(0, destajo.CantidadProcesada));

    private int ObtenerPorcentajeAsignado(PedidoSerigrafiaTallaProceso seg)
    {
        var total = ObtenerCantidadTallaSeguimiento(seg);
        if (total <= 0)
            return 0;

        return (int)Math.Round((double)ObtenerCantidadAsignada(seg.Id) / total * 100);
    }

    private static IEnumerable<RegistroDestajoProceso> OrdenarDestajos(IEnumerable<RegistroDestajoProceso> destajos)
        => destajos.OrderBy(d => d.CreatedAt == default ? d.Fecha : d.CreatedAt)
            .ThenBy(d => d.Fecha);

    private decimal ObtenerImporteAsignado(Guid seguimientoId)
        => ObtenerDestajos(seguimientoId)
            .Where(d => d.IsActive)
            .Sum(CalcularImporteDestajo);

    private decimal ObtenerTarifaDestajoPorPieza(Guid tipoProcesoId, int cantidadTotal)
    {
        var minutosPorPieza = ObtenerMinutosCotizacionPorPieza(tipoProcesoId, cantidadTotal);
        if (minutosPorPieza <= 0)
        {
            return 0m;
        }

        var tarifaMinuto = ObtenerTarifaOperacionPorMinuto(tipoProcesoId);
        if (tarifaMinuto <= 0)
        {
            return 0m;
        }

        return Math.Round(minutosPorPieza * tarifaMinuto, 4);
    }

    private async Task<DestajoTarifaResultado> ResolverTarifaDestajoAsync(Guid tipoProcesoId, Guid empleadoId, int cantidadTotal, DateTime fecha)
    {
        var proceso = pedidoSerigrafia?.TiposProceso
            .FirstOrDefault(tp => tp.TipoProcesoId == tipoProcesoId)?
            .TipoProceso;
        if (proceso == null)
        {
            return new DestajoTarifaResultado();
        }

        var empleado = empleadosActivos.FirstOrDefault(e => e.Id == empleadoId);
        if (empleado == null)
        {
            return new DestajoTarifaResultado();
        }

        var minutosPorPieza = ObtenerMinutosCotizacionPorPieza(tipoProcesoId, cantidadTotal);
        var tarifasEsquema = await ObtenerTarifasEsquemaEmpleadoAsync(empleadoId, fecha);
        return DestajoTarifaResolver.Resolver(proceso, empleado.PosicionId, minutosPorPieza, tarifasEsquema);
    }

    private async Task<List<EsquemaPagoTarifa>> ObtenerTarifasEsquemaEmpleadoAsync(Guid empleadoId, DateTime fecha)
    {
        var asignacion = await DbContext.EmpleadosEsquemaPago
            .AsNoTracking()
            .Where(a => a.EmpleadoId == empleadoId
                && a.VigenteDesde <= fecha
                && (a.VigenteHasta == null || a.VigenteHasta >= fecha))
            .OrderByDescending(a => a.VigenteDesde)
            .FirstOrDefaultAsync();

        if (asignacion == null)
        {
            return [];
        }

        return await DbContext.EsquemasPagoTarifa
            .AsNoTracking()
            .Where(t => t.EsquemaPagoId == asignacion.EsquemaPagoId)
            .ToListAsync();
    }

    private decimal ObtenerTarifaOperacionPorMinuto(Guid tipoProcesoId)
        => Math.Round(
            pedidoSerigrafia?.TiposProceso
                .FirstOrDefault(tp => tp.TipoProcesoId == tipoProcesoId)?
                .TipoProceso?
                .Posicion?
                .TarifaPorMinuto
            ?? 0m,
            4);

    private decimal ObtenerMinutosCotizacionPorPieza(Guid tipoProcesoId, int cantidadTotal)
    {
        var minutosProrrateados = ObtenerMinutosCotizacion(tipoProcesoId, cantidadTotal, 1);
        if (!minutosProrrateados.HasValue)
        {
            return 0m;
        }

        return Math.Round((decimal)minutosProrrateados.Value, 4);
    }

    private int? ObtenerMinutosCotizacion(Guid tipoProcesoId, int cantidadTotal, int cantidadAsignada)
    {
        var detalles = pedidoSerigrafia?.PedidoDetalle?.CotizacionSerigrafia?.Detalles;
        if (detalles == null || !detalles.Any())
        {
            return null;
        }

        var minutosBase = detalles
            .Where(d => d.Categoria == CotizacionCategoria.ManoObra && d.TipoProcesoId == tipoProcesoId && d.Tiempo.HasValue)
            .Sum(d => d.Tiempo ?? 0m);

        if (minutosBase <= 0)
        {
            return null;
        }

        if (cantidadTotal <= 0 || cantidadAsignada <= 0)
        {
            return (int)Math.Ceiling(minutosBase);
        }

        var minutosProrrateados = (minutosBase * cantidadAsignada) / cantidadTotal;
        return (int)Math.Ceiling(minutosProrrateados);
    }

    private decimal CalcularImporteDestajo(RegistroDestajoProceso destajo)
    {
        var cantidad = Math.Max(0, destajo.CantidadProcesada);
        var tarifa = Math.Max(0, destajo.TarifaUnitario);
        return cantidad * tarifa;
    }

    private Task SincronizarCompletadoProcesoAsync(PedidoSerigrafiaTallaProceso seg, string? usuarioActual, bool forzarCompletado = false)
    {
        var cantidadObjetivo = ObtenerCantidadTallaSeguimiento(seg);
        var cantidadAsignada = ObtenerCantidadAsignada(seg.Id);
        var completar = forzarCompletado || (cantidadObjetivo > 0 && cantidadAsignada >= cantidadObjetivo);

        seg.Completado = completar;
        seg.FechaPaso = completar
            ? seg.FechaPaso ?? DateTime.UtcNow
            : cantidadAsignada == 0 ? null : seg.FechaPaso;
        seg.UpdatedAt = DateTime.UtcNow;
        seg.UpdatedBy = usuarioActual;

        return Task.CompletedTask;
    }

    private async Task GuardarDestajo(PedidoSerigrafiaTallaProceso seg, RegistroDestajoProceso destajo)
    {
        if (!PuedeEditarProcesosProduccion())
        {
            modalDestajoError = "La producción está cerrada para edición.";
            return;
        }

        if (destajo.EmpleadoId == Guid.Empty || pedidoSerigrafia == null)
        {
            modalDestajoError = "Selecciona un empleado antes de guardar.";
            return;
        }

        var pendientesFgAntes = ObtenerIdsSkusPendientesFinishedGoods();

        var maximoDisponible = ObtenerCantidadDisponible(seg, destajo);
        var cantidadCapturada = Math.Max(0, destajo.CantidadProcesada);

        if (cantidadCapturada <= 0)
        {
            modalDestajoError = "La cantidad procesada debe ser mayor a cero.";
            return;
        }

        if (cantidadCapturada > maximoDisponible)
        {
            modalDestajoError = $"No puedes asignar {cantidadCapturada} piezas. Solo quedan {maximoDisponible} disponibles para esta operación.";
            return;
        }

        await _opLock.WaitAsync();
        try
        {
            var usuarioActual = await ObtenerUsuarioActualAsync();
            destajo.PedidoSerigrafiaId = seg.PedidoSerigrafiaId;
            destajo.PedidoSerigrafiaTallaProcesoId = seg.Id;
            destajo.PedidoSerigrafiaTallaId = seg.PedidoSerigrafiaTallaId;
            destajo.TipoProcesoId = seg.TipoProcesoId;
            destajo.EmpleadoId = destajo.EmpleadoId;
            destajo.Fecha = seg.FechaPaso ?? DateTime.UtcNow;
            destajo.CantidadProcesada = Math.Max(0, destajo.CantidadProcesada);
            destajo.TarifaUnitario = Math.Max(0, destajo.TarifaUnitario);
            destajo.Importe = CalcularImporteDestajo(destajo);
            destajo.IsActive = true;

            seg.EmpleadoId = ObtenerDestajos(seg.Id)
                .Where(d => d.IsActive && d.EmpleadoId != Guid.Empty)
                .Select(d => (Guid?)d.EmpleadoId)
                .FirstOrDefault();
            await SincronizarCompletadoProcesoAsync(seg, usuarioActual);
            DbContext.PedidoSerigrafiaTallaProcesos.Update(seg);

            if (destajo.Id == Guid.Empty)
            {
                destajo.Id = Guid.NewGuid();
                destajo.CreatedAt = DateTime.UtcNow;
                destajo.CreatedBy = usuarioActual;
                DbContext.RegistrosDestajoProceso.Add(destajo);
            }
            else
            {
                destajo.UpdatedAt = DateTime.UtcNow;
                destajo.UpdatedBy = usuarioActual;
                DbContext.RegistrosDestajoProceso.Update(destajo);
            }

            await DbContext.SaveChangesAsync();
            modalDestajoError = null;
            await CargarSerigrafiaAsync();
            await ActualizarEstadoPedidoAsync();
            LanzarConfirmacionIngresoFinishedGoodsSiAplica(pendientesFgAntes);
        }
        finally
        {
            _opLock.Release();
        }
    }

    private async Task EliminarDestajo(PedidoSerigrafiaTallaProceso seg, RegistroDestajoProceso destajo)
    {
        if (!PuedeEditarProcesosProduccion())
        {
            modalDestajoError = "La producción está cerrada para edición.";
            return;
        }

        var destajos = ObtenerDestajos(seg.Id);

        if (destajo.Id == Guid.Empty)
        {
            destajos.Remove(destajo);
            return;
        }

        await _opLock.WaitAsync();
        try
        {
            destajos.Remove(destajo);
            DbContext.RegistrosDestajoProceso.Remove(destajo);

            var usuarioActual = await ObtenerUsuarioActualAsync();
            var siguienteEmpleadoId = destajos
                .Where(d => d.Id != destajo.Id && d.IsActive && d.EmpleadoId != Guid.Empty)
                .Select(d => (Guid?)d.EmpleadoId)
                .FirstOrDefault();

            seg.EmpleadoId = siguienteEmpleadoId;
            await SincronizarCompletadoProcesoAsync(seg, usuarioActual);
            DbContext.PedidoSerigrafiaTallaProcesos.Update(seg);

            await DbContext.SaveChangesAsync();
            modalDestajoError = null;
            await CargarSerigrafiaAsync();
            await ActualizarEstadoPedidoAsync();
        }
        finally
        {
            _opLock.Release();
        }
    }

    private async Task OnConceptoCompletadoChanged(PedidoConcepto concepto, bool value)
    {
        if (PedidoCerradoParaCambiosOperativos())
        {
            return;
        }

        if (!value && ServicioYaEntregado(concepto.Id))
        {
            errorNota = "No puedes desactivar un servicio que ya fue incluido en una nota de entrega activa. Cancela primero la nota correspondiente.";
            return;
        }

        await _opLock.WaitAsync();
        try
        {
            concepto.Completado = value;
            concepto.FechaCompletado = value ? (concepto.FechaCompletado ?? DateTime.UtcNow) : null;
            concepto.UpdatedAt = DateTime.UtcNow;
            concepto.UpdatedBy = await ObtenerUsuarioActualAsync();
            DbContext.PedidoConceptos.Update(concepto);
            await DbContext.SaveChangesAsync();
            await ActualizarEstadoPedidoAsync();
        }
        finally
        {
            _opLock.Release();
        }
    }

    private async Task OnConceptoFechaChanged(PedidoConcepto concepto, DateTime? value)
    {
        if (PedidoCerradoParaCambiosOperativos())
        {
            return;
        }

        if (!value.HasValue && ServicioYaEntregado(concepto.Id))
        {
            errorNota = "No puedes limpiar la fecha de un servicio que ya fue entregado en una nota activa. Cancela primero la nota correspondiente.";
            return;
        }

        await _opLock.WaitAsync();
        try
        {
            concepto.FechaCompletado = value;
            concepto.Completado = value.HasValue || concepto.Completado;
            concepto.UpdatedAt = DateTime.UtcNow;
            concepto.UpdatedBy = await ObtenerUsuarioActualAsync();
            DbContext.PedidoConceptos.Update(concepto);
            await DbContext.SaveChangesAsync();
            await ActualizarEstadoPedidoAsync();
        }
        finally
        {
            _opLock.Release();
        }
    }

    private void SolicitarCrearNotaEntrega()
    {
        errorNota = null;
        textoPagareNota = ConstruirTextoPagareSugerido();
        modalConfirmarNotaAbierto = true;
    }

    private void CerrarModalConfirmarNota()
    {
        modalConfirmarNotaAbierto = false;
    }

    private async Task CrearNotaEntregaAsync()
    {
        errorNota = okNota = null;
        modalConfirmarNotaAbierto = false;
        var esPedidoSoloServicios = !detalleTallasPedido.Any() && conceptosPedido.Any(c => c.IsActive);
        var conceptosServicioEntregables = conceptosPedido
            .Where(c => c.IsActive && c.Completado && !ServicioYaEntregado(c.Id))
            .OrderBy(c => c.CreatedAt)
            .ToList();

        if (pedido == null)
        {
            errorNota = "No se encontró el pedido.";
            return;
        }

        if (PedidoCerradoParaCambiosOperativos())
        {
            errorNota = "El pedido está cerrado para cambios operativos.";
            return;
        }

        if (!esPedidoSoloServicios && piezasPendientes <= 0)
        {
            errorNota = "El pedido ya no tiene piezas pendientes por entregar.";
            return;
        }

        RecalcularEntregaActual();

        if (!esPedidoSoloServicios && cantidadNotaEntrega <= 0)
        {
            errorNota = "Captura una cantidad válida por talla para la parcialidad.";
            return;
        }

        if (!esPedidoSoloServicios && cantidadNotaEntrega > piezasPendientes)
        {
            errorNota = $"La cantidad excede el pendiente actual ({piezasPendientes} piezas).";
            return;
        }

        if (esPedidoSoloServicios && conceptosServicioEntregables.Count == 0)
        {
            errorNota = "Marca como completado al menos un servicio antes de generar la nota de entrega.";
            return;
        }

        guardandoNota = true;

        try
        {
            var usuario = await ObtenerUsuarioActualAsync();
            var numeroNota = await AppConfigRepository.GetNextCodeAsync("NOT-", "Consecutivo:NotaEntrega");
            var pedidoDetalleId = pedidoSerigrafia?.PedidoDetalleId ?? pedido.Detalles.FirstOrDefault()?.Id;
            var detallesNota = esPedidoSoloServicios
                ? ConstruirDetallesNotaEntregaServicios(usuario, conceptosServicioEntregables)
                : ConstruirDetallesNotaEntrega(pedidoDetalleId, usuario);
            if (!detallesNota.Any())
            {
                errorNota = "No se pudieron construir las líneas comerciales de la nota.";
                return;
            }

            var salidasFg = detalleTallasPedido.Any()
                ? await ConstruirSalidasFinishedGoodsAsync()
                : [];

            if (detalleTallasPedido.Any() && corridaPedido.Any(t => t.CantidadEntregar > 0) && !salidasFg.Any())
            {
                errorNota = "No se encontraron existencias de finished goods para las tallas seleccionadas.";
                return;
            }

            var subtotal = detallesNota.Sum(d => d.Importe);
            var impuestos = aplicaIvaNota ? Math.Round(subtotal * 0.16m, 2) : 0m;
            var total = Math.Round(subtotal + impuestos, 2);

            var nota = new NotaEntrega
            {
                Id = Guid.NewGuid(),
                PedidoId = pedido.Id,
                ClienteId = pedido.ClienteId,
                NumeroNota = numeroNota,
                FechaNota = fechaNotaEntrega,
                Estatus = NotaEntregaEstatus.Emitida,
                NoRequiereFactura = noRequiereFacturaNota,
                Observaciones = LimpiarTexto(observacionesNotaEntrega),
                TextoPagare = LimpiarTexto(textoPagareNota),
                Subtotal = subtotal,
                Impuestos = impuestos,
                Total = total,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = usuario,
                IsActive = true
            };

            nota.PedidosRelacionados.Add(new NotaEntregaPedido
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresaIdActual,
                PedidoId = pedido.Id,
                Orden = 0,
                EsPrincipal = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = usuario,
                IsActive = true
            });

            foreach (var detalleNota in detallesNota)
            {
                nota.Detalles.Add(detalleNota);
            }

            foreach (var salida in salidasFg)
            {
                salida.Inventario.CantidadDisponible = salida.ExistenciaNueva;
                salida.Inventario.UpdatedAt = DateTime.UtcNow;
                salida.Inventario.UpdatedBy = usuario;

                DbContext.MovimientosFinishedGoods.Add(new MovimientoFinishedGood
                {
                    Id = Guid.NewGuid(),
                    EmpresaId = salida.Inventario.EmpresaId,
                    InventarioFinishedGoodId = salida.Inventario.Id,
                    TipoMovimiento = MovimientoFinishedGoodTipoEnum.SalidaNotaEntrega,
                    Cantidad = salida.Cantidad,
                    ExistenciaAnterior = salida.ExistenciaAnterior,
                    ExistenciaNueva = salida.ExistenciaNueva,
                    FechaMovimiento = DateTime.UtcNow,
                    Referencia = numeroNota,
                    Observaciones = $"Salida por nota de entrega {numeroNota}",
                    PedidoId = pedido.Id,
                    PedidoSerigrafiaId = pedidoSerigrafia?.Id,
                    PedidoDetalleTallaId = salida.Detalle.Id,
                    NotaEntregaId = nota.Id,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = usuario,
                    IsActive = true
                });
            }

            DbContext.NotasEntrega.Add(nota);
            await DbContext.SaveChangesAsync();
            try
            {
                await NotaEntregaPdfService.GenerateAndStoreAsync(nota.Id);
            }
            catch
            {
                foreach (var salida in salidasFg)
                {
                    salida.Inventario.CantidadDisponible = salida.ExistenciaAnterior;
                    salida.Inventario.UpdatedAt = DateTime.UtcNow;
                    salida.Inventario.UpdatedBy = usuario;
                }

                var movimientosSalida = await DbContext.MovimientosFinishedGoods
                    .Where(m => m.NotaEntregaId == nota.Id && m.TipoMovimiento == MovimientoFinishedGoodTipoEnum.SalidaNotaEntrega)
                    .ToListAsync();
                if (movimientosSalida.Any())
                {
                    DbContext.MovimientosFinishedGoods.RemoveRange(movimientosSalida);
                }

                DbContext.NotasEntrega.Remove(nota);
                await DbContext.SaveChangesAsync();
                throw;
            }

            await CargarNotasEntregaAsync();
            await ActualizarEstadoPedidoAsync();

            observacionesNotaEntrega = null;
            textoPagareNota = ConstruirTextoPagareSugerido();
            noRequiereFacturaNota = false;
            aplicaIvaNota = false;
            fechaNotaEntrega = DateTime.Today;
            okNota = $"Nota {numeroNota} creada correctamente.";
        }
        catch (Exception ex)
        {
            errorNota = ex.InnerException?.Message ?? ex.Message;
        }
        finally
        {
            guardandoNota = false;
        }
    }

    private async Task ActualizarEstadoPedidoAsync()
    {
        if (pedido == null) return;

        var conceptos = await DbContext.PedidoConceptos
            .Where(c => c.PedidoId == PedidoId && c.IsActive)
            .ToListAsync();

        conceptosPedido = conceptos;

        var totalConceptos = conceptos.Count;
        var conceptosCompletados = conceptos.Count(c => c.Completado);
        var serviciosEnProceso = conceptos.Any(c => c.Completado || c.FechaCompletado.HasValue);
        var serviciosCompletos = totalConceptos == 0 || conceptosCompletados == totalConceptos;

        var productoCompleto = false;
        var productoEnProceso = false;

        if (pedidoSerigrafia != null)
        {
            var procesos = pedidoSerigrafia.TiposProceso.Select(tp => tp.TipoProcesoId).ToList();
            var tallas = pedidoSerigrafia.Tallas.Select(t => t.Id).ToList();
            var segs = await DbContext.PedidoSerigrafiaTallaProcesos
                .Where(s => s.PedidoSerigrafiaId == pedidoSerigrafia.Id)
                .ToListAsync();

            var totalComb = procesos.Count * tallas.Count;
            var completados = segs.Count(s => s.Completado && procesos.Contains(s.TipoProcesoId) && tallas.Contains(s.PedidoSerigrafiaTallaId));
            productoCompleto = totalComb > 0 && completados == totalComb;
            productoEnProceso = completados > 0;
        }

        var nuevoEstado = EstadoPedidoEnum.Nuevo;
        await CargarNotasEntregaAsync();

        var totalPagado = await DbContext.PagosPedido
            .Where(pg => pg.PedidoId == PedidoId && pg.IsActive)
            .SumAsync(pg => pg.Monto);

        var tieneNotas = notasEntrega.Any();
        var notasFacturadas = notasEntrega.Count(n => n.FacturasRelacionadas.Any(r => r.IsActive && r.Factura.IsActive && r.Factura.Estatus != FacturaEstatus.Cancelado));
        var entregaCompleta = piezasPedidoTotal > 0 && piezasEntregadas >= piezasPedidoTotal;
        var cumplimientoOperativoCompleto = pedidoSerigrafia != null
            ? productoCompleto && serviciosCompletos
            : totalConceptos > 0 && serviciosCompletos;
        var cumplimientoOperativoEnProceso = pedidoSerigrafia != null
            ? productoEnProceso || serviciosEnProceso
            : serviciosEnProceso;

        if (cumplimientoOperativoCompleto && entregaCompleta && tieneNotas && notasFacturadas == notasEntrega.Count)
        {
            nuevoEstado = pedido.Total > 0 && totalPagado >= pedido.Total
                ? EstadoPedidoEnum.Pagado
                : EstadoPedidoEnum.Facturado;
        }
        else if (cumplimientoOperativoCompleto && entregaCompleta && tieneNotas)
        {
            nuevoEstado = EstadoPedidoEnum.Entregado;
        }
        else if (cumplimientoOperativoCompleto)
        {
            nuevoEstado = EstadoPedidoEnum.Producido;
        }
        else if (cumplimientoOperativoEnProceso || tieneNotas)
        {
            nuevoEstado = EstadoPedidoEnum.EnProceso;
        }

        if (pedido != null && pedido.Estado != nuevoEstado)
        {
            pedido.Estado = nuevoEstado;
            pedido.UpdatedAt = DateTime.UtcNow;
            pedido.UpdatedBy = await ObtenerUsuarioActualAsync();
            DbContext.Pedidos.Update(pedido);
            await DbContext.SaveChangesAsync();
        }
    }

    private async Task<string?> ObtenerUsuarioActualAsync()
    {
        var state = await AuthStateProvider.GetAuthenticationStateAsync();
        return state.User.Identity?.Name;
    }

    private async Task<Guid> ObtenerEmpresaActualIdAsync()
    {
        var state = await AuthStateProvider.GetAuthenticationStateAsync();
        return Guid.TryParse(state.User.FindFirst("EmpresaId")?.Value, out var empresaId)
            ? empresaId
            : Guid.Empty;
    }

    private async Task<bool> TieneCapacidadAsync(string clave)
    {
        var state = await AuthStateProvider.GetAuthenticationStateAsync();
        return state.User.HasClaim("Capacidad", clave);
    }

    private bool PuedeEditarProcesosProduccion()
        => !ProduccionCerradaParaEdicion() || puedeReabrirProcesos;

    private bool PedidoCerradoParaCambiosOperativos()
        => pedido?.Estado is EstadoPedidoEnum.Entregado or EstadoPedidoEnum.Facturado or EstadoPedidoEnum.Pagado;

    private bool PuedeIngresarFinishedGoods()
        => inventarioFinishedGoodsDisponible && puedeIngresarFinishedGoods;

    private bool ProduccionCerradaParaEdicion()
        => pedido?.Estado is EstadoPedidoEnum.Producido or EstadoPedidoEnum.Entregado or EstadoPedidoEnum.Facturado or EstadoPedidoEnum.Pagado;

    private string? ObtenerObservacionOperativaPedido()
    {
        if (pedido is null)
        {
            return null;
        }

        if (!inventarioFinishedGoodsDisponible)
        {
            return "Finished goods pendiente de habilitar en base de datos";
        }

        if (ObtenerSkusPendientesFinishedGoods().Any())
        {
            return "Producción completada por SKU · Pendiente de ingresar a finished goods";
        }

        if (pedido.Estado == EstadoPedidoEnum.Producido && piezasPendientes > 0)
        {
            return "Producción completada · Entrega pendiente";
        }

        if (!PuedeEditarProcesosProduccion())
        {
            return "Producción cerrada para edición de checks y fechas";
        }

        return null;
    }

    private IEnumerable<FinishedGoodPendienteItem> ObtenerSkusPendientesFinishedGoods()
    {
        if (pedidoSerigrafia == null || !detalleTallasPedido.Any() || !pedidoSerigrafia.TiposProceso.Any())
        {
            return [];
        }

        if (!inventarioFinishedGoodsDisponible)
        {
            return [];
        }

        var procesos = pedidoSerigrafia.TiposProceso
            .Select(tp => tp.TipoProceso)
            .Where(tp => tp != null)
            .Cast<TipoProceso>()
            .ToList();

        return detalleTallasPedido
            .Where(t => t.ProductoVarianteId.HasValue && t.Cantidad > 0)
            .Select(t => new { Detalle = t, TallaProduccion = ResolverTallaProduccion(t) })
            .Where(x => x.TallaProduccion != null)
            .Where(x => EsTallaCompletada(x.TallaProduccion!.Id, procesos))
            .Where(x => !detalleTallasIngresadasFg.Contains(x.Detalle.Id))
            .OrderBy(x => x.Detalle.Orden)
            .ThenBy(x => x.Detalle.Talla)
            .Select(x => new FinishedGoodPendienteItem
            {
                PedidoDetalleTallaId = x.Detalle.Id,
                Sku = x.Detalle.ProductoVariante?.Sku ?? "—",
                Talla = x.Detalle.Talla,
                Color = x.Detalle.ProductoVariante?.Color ?? pedidoSerigrafia.CombinacionColor,
                Cantidad = x.Detalle.Cantidad,
                PrecioUnitario = x.Detalle.PrecioUnitario
            });
    }

    private HashSet<Guid> ObtenerIdsSkusPendientesFinishedGoods()
        => ObtenerSkusPendientesFinishedGoods()
            .Select(x => x.PedidoDetalleTallaId)
            .ToHashSet();

    private void LanzarConfirmacionIngresoFinishedGoodsSiAplica(HashSet<Guid> pendientesAntes)
    {
        if (!PuedeIngresarFinishedGoods() || modalConfirmarIngresoFgAbierto)
        {
            return;
        }

        var pendientesActuales = ObtenerSkusPendientesFinishedGoods().ToList();
        var nuevoPendiente = pendientesActuales
            .FirstOrDefault(x => !pendientesAntes.Contains(x.PedidoDetalleTallaId));

        if (nuevoPendiente == null)
        {
            return;
        }

        confirmacionIngresoFgItem = nuevoPendiente;
        confirmacionIngresoFgEsUltimoSku = pendientesActuales.Count == 1;
        modalConfirmarIngresoFgAbierto = true;
    }

    private void PosponerIngresoFinishedGoods()
    {
        modalConfirmarIngresoFgAbierto = false;
        confirmacionIngresoFgItem = null;
        confirmacionIngresoFgEsUltimoSku = false;
    }

    private async Task ConfirmarIngresoFinishedGoodsDetectadoAsync()
    {
        var item = confirmacionIngresoFgItem;
        PosponerIngresoFinishedGoods();

        if (item == null)
        {
            return;
        }

        await IngresarDetalleTallaAFinishedGoodsAsync(item.PedidoDetalleTallaId);
    }

    private PedidoSerigrafiaTalla? ResolverTallaProduccion(PedidoDetalleTalla detalle)
    {
        if (pedidoSerigrafia == null)
        {
            return null;
        }

        var coincidencia = pedidoSerigrafia.Tallas.FirstOrDefault(t => string.Equals(t.Talla, detalle.Talla, StringComparison.OrdinalIgnoreCase));
        return coincidencia;
    }

    private async Task IngresarTodosLosSkusPendientesAFinishedGoodsAsync()
    {
        foreach (var skuPendiente in ObtenerSkusPendientesFinishedGoods().ToList())
        {
            await IngresarDetalleTallaAFinishedGoodsAsync(skuPendiente.PedidoDetalleTallaId, mostrarMensajes: false);
            if (!string.IsNullOrWhiteSpace(errorFg))
            {
                return;
            }
        }

        okFg = "Todos los SKU completos pendientes se ingresaron a finished goods.";
    }

    private async Task IngresarDetalleTallaAFinishedGoodsAsync(Guid pedidoDetalleTallaId, bool mostrarMensajes = true)
    {
        if (!PuedeIngresarFinishedGoods())
        {
            errorFg = inventarioFinishedGoodsDisponible
                ? "No tienes permisos para ingresar SKU a finished goods."
                : "Finished goods no está disponible hasta aplicar la migración correspondiente.";
            return;
        }

        if (empresaIdActual == Guid.Empty || pedido is null || pedidoSerigrafia is null)
        {
            errorFg = "No se pudo resolver la empresa o el pedido activo para finished goods.";
            return;
        }

        errorFg = null;
        if (mostrarMensajes)
        {
            okFg = null;
        }

        await _opLock.WaitAsync();
        guardandoFg = true;
        try
        {
            var detalle = await DbContext.PedidosDetalleTalla
                .Include(t => t.ProductoVariante)
                .Include(t => t.PedidoDetalle)
                    .ThenInclude(pd => pd.Producto)
                .FirstOrDefaultAsync(t => t.Id == pedidoDetalleTallaId);

            if (detalle == null || detalle.ProductoVarianteId == null || detalle.ProductoVariante == null)
            {
                errorFg = "El SKU seleccionado ya no existe o no tiene variante asociada.";
                return;
            }

            if (detalleTallasIngresadasFg.Contains(detalle.Id))
            {
                errorFg = "Ese SKU ya fue ingresado previamente a finished goods.";
                return;
            }

            var tallaProduccion = ResolverTallaProduccion(detalle);
            var procesos = pedidoSerigrafia.TiposProceso.Select(tp => tp.TipoProceso).Where(tp => tp != null).Cast<TipoProceso>().ToList();
            if (tallaProduccion == null || !EsTallaCompletada(tallaProduccion.Id, procesos))
            {
                errorFg = "Ese SKU todavía no completa todos sus procesos de producción.";
                return;
            }

            var usuario = await ObtenerUsuarioActualAsync();
            var inventario = await DbContext.InventariosFinishedGoods
                .FirstOrDefaultAsync(i => i.EmpresaId == empresaIdActual
                    && i.ClienteId == pedido.ClienteId
                    && i.ProductoVarianteId == detalle.ProductoVarianteId.Value);

            var existenciaAnterior = inventario?.CantidadDisponible ?? 0m;
            var cantidadIngreso = detalle.Cantidad;

            if (inventario == null)
            {
                inventario = new InventarioFinishedGood
                {
                    Id = Guid.NewGuid(),
                    EmpresaId = empresaIdActual,
                    ClienteId = pedido.ClienteId,
                    ProductoId = detalle.PedidoDetalle.ProductoId,
                    ProductoVarianteId = detalle.ProductoVarianteId.Value,
                    Sku = detalle.ProductoVariante.Sku,
                    Talla = detalle.Talla,
                    Color = detalle.ProductoVariante.Color ?? pedidoSerigrafia.CombinacionColor,
                    CantidadDisponible = cantidadIngreso,
                    CantidadReservada = 0m,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = usuario,
                    IsActive = true
                };

                DbContext.InventariosFinishedGoods.Add(inventario);
            }
            else
            {
                inventario.CantidadDisponible += cantidadIngreso;
                inventario.Color ??= detalle.ProductoVariante.Color ?? pedidoSerigrafia.CombinacionColor;
                inventario.UpdatedAt = DateTime.UtcNow;
                inventario.UpdatedBy = usuario;
                DbContext.InventariosFinishedGoods.Update(inventario);
            }

            DbContext.MovimientosFinishedGoods.Add(new MovimientoFinishedGood
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresaIdActual,
                InventarioFinishedGood = inventario,
                TipoMovimiento = MovimientoFinishedGoodTipoEnum.IngresoProduccion,
                Cantidad = cantidadIngreso,
                ExistenciaAnterior = existenciaAnterior,
                ExistenciaNueva = existenciaAnterior + cantidadIngreso,
                FechaMovimiento = DateTime.UtcNow,
                Referencia = pedido.NumeroPedido,
                Observaciones = $"Ingreso desde producción del pedido {pedido.NumeroPedido}",
                PedidoId = pedido.Id,
                PedidoSerigrafiaId = pedidoSerigrafia.Id,
                PedidoDetalleTallaId = detalle.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = usuario,
                IsActive = true
            });

            await DbContext.SaveChangesAsync();
            await CargarSerigrafiaAsync();
            detalleTallasIngresadasFg.Add(detalle.Id);

            if (mostrarMensajes)
            {
                okFg = $"El SKU {detalle.ProductoVariante.Sku} se ingresó correctamente a finished goods.";
            }
        }
        catch (Exception ex)
        {
            errorFg = ex.InnerException?.Message ?? ex.Message;
        }
        finally
        {
            guardandoFg = false;
            _opLock.Release();
        }
    }

    private static string ObtenerEtiquetaCotizacion(CotizacionSerigrafia cotizacion)
    {
        var descripcion = string.IsNullOrWhiteSpace(cotizacion.Descripcion) ? "Sin descripción" : cotizacion.Descripcion.Trim();
        return $"Cotización: {descripcion} · {cotizacion.CreatedAt:dd/MM/yyyy}";
    }

    private int ObtenerPiezasPedidoTotal()
    {
        if (detalleTallasPedido.Any())
        {
            return (int)Math.Round(detalleTallasPedido.Sum(t => t.Cantidad));
        }

        if (pedidoSerigrafia?.Tallas.Any() == true)
        {
            return pedidoSerigrafia.Tallas.Sum(t => t.Cantidad);
        }

        if (conceptosPedido.Any(c => c.IsActive))
        {
            return conceptosPedido.Where(c => c.IsActive).Sum(c => c.Cantidad);
        }

        return pedido?.Detalles.Sum(d => d.Cantidad) ?? 0;
    }

    private bool PuedeCrearNotaEntregaServicios()
        => !corridaPedido.Any() && conceptosPedido.Any(c => c.IsActive);

    private bool ServicioYaEntregado(Guid pedidoConceptoId)
        => notasEntrega.Any(n => n.IsActive
            && n.Estatus != NotaEntregaEstatus.Cancelada
            && n.Detalles.Any(d => d.PedidoConceptoId == pedidoConceptoId && d.IsActive));

    private string ConstruirTextoPagareSugerido()
    {
        if (pedido == null)
        {
            return string.Empty;
        }

        var totalSugerido = PuedeCrearNotaEntregaServicios()
            ? conceptosPedido.Where(c => c.IsActive && c.Completado).Sum(c => c.Total)
            : CalcularSubtotalNotaEntrega() + (aplicaIvaNota ? Math.Round(CalcularSubtotalNotaEntrega() * 0.16m, 2) : 0m);

        var importeEnLetra = ConvertirImporteALetrasLocal(totalSugerido);
        var nombreEmpresa = pedido.Cliente?.NombreComercial;
        var acreedor = string.IsNullOrWhiteSpace(nombreEmpresa) ? "ZENITH" : nombreEmpresa.Trim().ToUpperInvariant();
        var lugarPago = string.IsNullOrWhiteSpace(pedido.Cliente?.Ciudad) ? "________________" : pedido.Cliente.Ciudad.Trim().ToUpperInvariant();
        var fechaVencimiento = pedido.TipoPrecio == TipoPrecioEnum.Credito && pedido.Cliente?.DiasCredito.GetValueOrDefault() > 0
            ? fechaNotaEntrega.AddDays(pedido.Cliente.DiasCredito!.Value).ToString("dd/MM/yyyy")
            : "A LA VISTA";
        var vencimiento = fechaVencimiento == "A LA VISTA" ? "A LA VISTA" : $"EL DÍA {fechaVencimiento}";

        return $"Debo y pagaré incondicionalmente a la orden de {acreedor}, en {lugarPago}, {vencimiento}, la cantidad de {importeEnLetra} ({totalSugerido:C2}), valor recibido a mi entera satisfacción según nota pendiente de generar del pedido {pedido.NumeroPedido}. En caso de mora, acepto cubrir los accesorios y gastos de cobranza que legalmente procedan conforme a la legislación mercantil mexicana aplicable.";
    }

    private void ConstruirCorridaEntrega()
    {
        var tallasPedido = detalleTallasPedido.Any()
            ? detalleTallasPedido
                .OrderBy(t => t.Orden)
                .ThenBy(t => decimal.TryParse(t.Talla, out var numero) ? numero : decimal.MaxValue)
                .ThenBy(t => t.Talla)
                .Select(t => new EntregaTallaEditor
                {
                    Talla = t.Talla,
                    Orden = t.Orden,
                    CantidadPedido = (int)Math.Round(t.Cantidad),
                    PrecioUnitario = t.PrecioUnitario,
                    CantidadEntregada = ObtenerCantidadEntregadaPorTalla(t.Talla),
                    CantidadEntregar = 0
                })
                .ToList()
            : pedidoSerigrafia?.Tallas
                .OrderBy(t => decimal.TryParse(t.Talla, out var numero) ? numero : decimal.MaxValue)
                .ThenBy(t => t.Talla)
                .Select((t, index) => new EntregaTallaEditor
                {
                    Talla = t.Talla,
                    Orden = index + 1,
                    CantidadPedido = t.Cantidad,
                    PrecioUnitario = 0m,
                    CantidadEntregada = ObtenerCantidadEntregadaPorTalla(t.Talla),
                    CantidadEntregar = 0
                })
                .ToList()
            ?? new();

        foreach (var talla in tallasPedido)
        {
            talla.CantidadPendiente = Math.Max(0, talla.CantidadPedido - talla.CantidadEntregada);
        }

        corridaPedido = tallasPedido;
        RecalcularEntregaActual();
    }

    private decimal CalcularSubtotalNotaEntrega()
    {
        if (PuedeCrearNotaEntregaServicios())
        {
            return Math.Round(conceptosPedido.Where(c => c.IsActive && c.Completado).Sum(c => c.Total), 2);
        }

        if (!corridaPedido.Any(t => t.CantidadEntregar > 0))
        {
            return 0m;
        }

        if (detalleTallasPedido.Any())
        {
            return Math.Round(corridaPedido.Sum(t => t.CantidadEntregar * t.PrecioUnitario), 2);
        }

        var proporcion = piezasPedidoTotal > 0 ? (decimal)cantidadNotaEntrega / piezasPedidoTotal : 1m;
        return Math.Round(pedido?.Subtotal * proporcion ?? 0m, 2);
    }

    private decimal ObtenerCantidadFinishedGoodDisponible(EntregaTallaEditor talla)
    {
        var detalle = ResolverDetallePedidoTalla(talla);
        return detalle != null && existenciasFgPorDetalleTalla.TryGetValue(detalle.Id, out var cantidad)
            ? cantidad
            : 0m;
    }

    private int ObtenerCantidadEntregadaPorTalla(string talla)
        => (int)Math.Round(notasEntrega
            .SelectMany(n => n.Detalles)
            .SelectMany(d => d.Tallas)
            .Where(t => string.Equals(t.Talla, talla, StringComparison.OrdinalIgnoreCase))
            .Sum(t => t.Cantidad));

    private void RecalcularEntregaActual()
    {
        if (!corridaPedido.Any())
        {
            cantidadNotaEntrega = conceptosPedido.Where(c => c.IsActive).Sum(c => c.Cantidad);
            return;
        }

        foreach (var talla in corridaPedido)
        {
            if (talla.CantidadEntregar < 0)
            {
                talla.CantidadEntregar = 0;
            }

            if (talla.CantidadEntregar > talla.CantidadPendiente)
            {
                talla.CantidadEntregar = talla.CantidadPendiente;
            }
        }

        cantidadNotaEntrega = corridaPedido.Sum(t => t.CantidadEntregar);
    }

    private bool PuedeTomarTodoPendiente()
    {
        if (pedidoSerigrafia == null || !pedidoSerigrafia.Tallas.Any() || !pedidoSerigrafia.TiposProceso.Any())
        {
            return true;
        }

        var procesos = pedidoSerigrafia.TiposProceso.Select(tp => tp.TipoProceso).Where(tp => tp != null).Cast<TipoProceso>();
        return pedidoSerigrafia.Tallas.All(t => EsTallaCompletada(t.Id, procesos));
    }

    private void AutollenarEntregaPendiente()
    {
        if (!PuedeTomarTodoPendiente())
        {
            errorNota = "Primero termina los procesos de producción antes de tomar todo lo pendiente.";
            return;
        }

        foreach (var talla in corridaPedido)
        {
            talla.CantidadEntregar = talla.CantidadPendiente;
        }

        RecalcularEntregaActual();
    }

    private static int ObtenerCantidadNota(NotaEntrega nota)
    {
        var cantidadTallas = nota.Detalles.SelectMany(d => d.Tallas).Sum(t => t.Cantidad);
        return cantidadTallas > 0
            ? (int)Math.Round(cantidadTallas)
            : (int)Math.Round(nota.Detalles.Sum(d => d.Cantidad));
    }

    private string ConstruirDescripcionNota()
    {
        if (pedidoSerigrafia != null)
        {
            return string.IsNullOrWhiteSpace(pedidoSerigrafia.CombinacionColor)
                ? $"Entrega parcial pedido {pedido?.NumeroPedido} - {pedidoSerigrafia.Estilo}"
                : $"Entrega parcial pedido {pedido?.NumeroPedido} - {pedidoSerigrafia.Estilo} / {pedidoSerigrafia.CombinacionColor}";
        }

        return $"Entrega parcial pedido {pedido?.NumeroPedido}";
    }

    private List<NotaEntregaDetalle> ConstruirDetallesNotaEntrega(Guid? pedidoDetalleId, string? usuario)
    {
        var detalles = new List<NotaEntregaDetalle>();

        foreach (var talla in corridaPedido
            .Where(t => t.CantidadEntregar > 0)
            .OrderBy(t => t.Orden)
            .ThenBy(t => t.Talla))
        {
            var detallePedidoTalla = ResolverDetallePedidoTalla(talla);
            var precioUnitario = detallePedidoTalla?.PrecioUnitario ?? talla.PrecioUnitario;
            var importe = Math.Round(talla.CantidadEntregar * precioUnitario, 2);

            var detalleNota = new NotaEntregaDetalle
            {
                Id = Guid.NewGuid(),
                PedidoDetalleId = pedidoDetalleId,
                Descripcion = ConstruirDescripcionNotaTalla(talla, detallePedidoTalla),
                Cantidad = talla.CantidadEntregar,
                PrecioUnitario = precioUnitario,
                Importe = importe,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = usuario,
                IsActive = true
            };

            detalleNota.Tallas.Add(new NotaEntregaDetalleTalla
            {
                Id = Guid.NewGuid(),
                PedidoDetalleTallaId = detallePedidoTalla?.Id,
                Talla = talla.Talla,
                Orden = talla.Orden,
                Cantidad = talla.CantidadEntregar,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = usuario,
                IsActive = true
            });

            detalles.Add(detalleNota);
        }

        return detalles;
    }

    private List<NotaEntregaDetalle> ConstruirDetallesNotaEntregaServicios(string? usuario, IReadOnlyCollection<PedidoConcepto> conceptosEntregables)
    {
        return conceptosEntregables
            .OrderBy(c => c.CreatedAt)
            .Select(c => new NotaEntregaDetalle
            {
                Id = Guid.NewGuid(),
                PedidoDetalleId = null,
                PedidoConceptoId = c.Id,
                Descripcion = c.Descripcion,
                Cantidad = c.Cantidad,
                PrecioUnitario = c.PrecioUnitario,
                Importe = c.Total,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = usuario,
                IsActive = true
            })
            .ToList();
    }

    private static string ConvertirImporteALetrasLocal(decimal total)
    {
        var entero = (int)Math.Floor(total);
        var centavos = (int)Math.Round((total - entero) * 100, MidpointRounding.AwayFromZero);
        return $"{NumeroALetrasLocal(entero)} pesos {centavos:00}/100 M.N.".ToUpperInvariant();
    }

    private static string NumeroALetrasLocal(int numero)
    {
        if (numero == 0) return "cero";
        if (numero < 0) return "menos " + NumeroALetrasLocal(Math.Abs(numero));

        string[] unidades = ["", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve", "diez", "once", "doce", "trece", "catorce", "quince", "dieciséis", "diecisiete", "dieciocho", "diecinueve"];
        string[] decenas = ["", "", "veinte", "treinta", "cuarenta", "cincuenta", "sesenta", "setenta", "ochenta", "noventa"];
        string[] centenas = ["", "ciento", "doscientos", "trescientos", "cuatrocientos", "quinientos", "seiscientos", "setecientos", "ochocientos", "novecientos"];

        if (numero == 100) return "cien";
        if (numero < 20) return unidades[numero];
        if (numero < 30) return numero == 20 ? "veinte" : "veinti" + unidades[numero - 20];
        if (numero < 100) return numero % 10 == 0 ? decenas[numero / 10] : $"{decenas[numero / 10]} y {unidades[numero % 10]}";
        if (numero < 1000) return numero % 100 == 0 ? centenas[numero / 100] : $"{centenas[numero / 100]} {NumeroALetrasLocal(numero % 100)}";
        if (numero < 1000000)
        {
            var miles = numero / 1000;
            var resto = numero % 1000;
            var textoMiles = miles == 1 ? "mil" : $"{NumeroALetrasLocal(miles)} mil";
            return resto == 0 ? textoMiles : $"{textoMiles} {NumeroALetrasLocal(resto)}";
        }

        var millones = numero / 1000000;
        var restoMillones = numero % 1000000;
        var textoMillones = millones == 1 ? "un millón" : $"{NumeroALetrasLocal(millones)} millones";
        return restoMillones == 0 ? textoMillones : $"{textoMillones} {NumeroALetrasLocal(restoMillones)}";
    }

    private async Task<List<SalidaFinishedGoodPlan>> ConstruirSalidasFinishedGoodsAsync()
    {
        if (!inventarioFinishedGoodsDisponible)
        {
            errorNota = "Finished goods no está disponible en la base de datos. Aplica la migración pendiente antes de generar notas con descuento de FG.";
            return [];
        }

        var salidas = new List<SalidaFinishedGoodPlan>();

        foreach (var talla in corridaPedido
            .Where(t => t.CantidadEntregar > 0)
            .OrderBy(t => t.Orden)
            .ThenBy(t => t.Talla))
        {
            var detallePedidoTalla = ResolverDetallePedidoTalla(talla);
            if (detallePedidoTalla?.ProductoVarianteId == null)
            {
                errorNota = $"La talla {talla.Talla} no tiene SKU asociado para descontar finished goods.";
                return [];
            }

            var inventario = await DbContext.InventariosFinishedGoods
                .FirstOrDefaultAsync(i => i.EmpresaId == empresaIdActual
                    && i.ClienteId == pedido!.ClienteId
                    && i.ProductoVarianteId == detallePedidoTalla.ProductoVarianteId.Value);

            if (inventario == null || inventario.CantidadDisponible < talla.CantidadEntregar)
            {
                var disponible = inventario?.CantidadDisponible ?? 0m;
                errorNota = $"No hay suficiente finished goods para la talla {talla.Talla}. Disponible: {disponible:0.##}, solicitado: {talla.CantidadEntregar:0.##}.";
                return [];
            }

            salidas.Add(new SalidaFinishedGoodPlan
            {
                Detalle = detallePedidoTalla,
                Inventario = inventario,
                Cantidad = talla.CantidadEntregar,
                ExistenciaAnterior = inventario.CantidadDisponible,
                ExistenciaNueva = inventario.CantidadDisponible - talla.CantidadEntregar
            });
        }

        return salidas;
    }

    private PedidoDetalleTalla? ResolverDetallePedidoTalla(EntregaTallaEditor talla)
    {
        var coincidencia = detalleTallasPedido.FirstOrDefault(t => t.Orden == talla.Orden
            && string.Equals(t.Talla, talla.Talla, StringComparison.OrdinalIgnoreCase));

        coincidencia ??= detalleTallasPedido.FirstOrDefault(t => string.Equals(t.Talla, talla.Talla, StringComparison.OrdinalIgnoreCase));
        return coincidencia;
    }

    private string ConstruirDescripcionNotaTalla(EntregaTallaEditor talla, PedidoDetalleTalla? detallePedidoTalla)
    {
        var descripcionBase = ConstruirDescripcionNota();
        var sku = detallePedidoTalla?.ProductoVariante?.Sku;

        return string.IsNullOrWhiteSpace(sku)
            ? $"{descripcionBase} · Talla {talla.Talla}"
            : $"{descripcionBase} · Talla {talla.Talla} · SKU {sku}";
    }

    private static string? LimpiarTexto(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static bool EsTablaFinishedGoodsInexistente(Exception ex)
    {
        var actual = ex;
        while (actual != null)
        {
            if (actual.Message.Contains("movimientosfinishedgoods", StringComparison.OrdinalIgnoreCase)
                || actual.Message.Contains("inventariosfinishedgoods", StringComparison.OrdinalIgnoreCase)
                || actual.Message.Contains("doesn't exist", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            actual = actual.InnerException!;
        }

        return false;
    }

    private static string ObtenerResumenFacturaNota(NotaEntrega nota)
    {
        var facturas = nota.FacturasRelacionadas
            .Where(r => r.IsActive && r.Factura.IsActive && r.Factura.Estatus != FacturaEstatus.Cancelado)
            .Select(r => r.Factura.FolioFiscal ?? r.Factura.FolioInterno)
            .Distinct()
            .ToList();

        return facturas.Count == 0 ? "Pendiente" : string.Join(", ", facturas);
    }

    private static string? ObtenerResumenTallasNota(NotaEntrega nota)
    {
        var tallas = nota.Detalles
            .SelectMany(d => d.Tallas)
            .OrderBy(t => t.Orden)
            .ThenBy(t => t.Talla)
            .Select(t => $"{t.Talla}: {t.Cantidad:0.##}")
            .ToList();

        return tallas.Count == 0 ? null : string.Join(" · ", tallas);
    }

    private sealed class EntregaTallaEditor
    {
        public string Talla { get; set; } = string.Empty;
        public int Orden { get; set; }
        public int CantidadPedido { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int CantidadEntregada { get; set; }
        public int CantidadPendiente { get; set; }
        public int CantidadEntregar { get; set; }
    }

    private sealed class SalidaFinishedGoodPlan
    {
        public PedidoDetalleTalla Detalle { get; set; } = null!;
        public InventarioFinishedGood Inventario { get; set; } = null!;
        public decimal Cantidad { get; set; }
        public decimal ExistenciaAnterior { get; set; }
        public decimal ExistenciaNueva { get; set; }
    }

    private sealed class FinishedGoodPendienteItem
    {
        public Guid PedidoDetalleTallaId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Talla { get; set; } = string.Empty;
        public string? Color { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}
