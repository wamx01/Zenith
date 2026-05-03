using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MundoVs.Core.Entities;
using MundoVs.Core.Entities.Auth;
using MundoVs.Core.Entities.Calzado;
using MundoVs.Core.Entities.Inventario;
using MundoVs.Core.Entities.Serigrafia;
using MundoVs.Core.Interfaces;

namespace MundoVs.Infrastructure.Data;

public class CrmDbContext : DbContext
{
    private readonly Guid _empresaId;

    public CrmDbContext(DbContextOptions<CrmDbContext> options, IEmpresaContext? empresaContext = null) : base(options)
    {
        _empresaId = empresaContext?.EmpresaId ?? Guid.Empty;
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        SetEmpresaIdOnNewEntities();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        SetEmpresaIdOnNewEntities();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void SetEmpresaIdOnNewEntities()
    {
        if (_empresaId == Guid.Empty) return;

        foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added))
        {
            var prop = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "EmpresaId");
            if (prop != null && prop.CurrentValue is Guid id && id == Guid.Empty)
            {
                prop.CurrentValue = _empresaId;
            }
        }
    }

    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Plan> Planes => Set<Plan>();
    public DbSet<SuscripcionEmpresa> SuscripcionesEmpresa => Set<SuscripcionEmpresa>();
    public DbSet<PagoSuscripcion> PagosSuscripcion => Set<PagoSuscripcion>();
    public DbSet<AppConfig> AppConfigs => Set<AppConfig>();
    public DbSet<NominaConfiguracionGlobal> NominaConfiguracionesGlobales => Set<NominaConfiguracionGlobal>();
    public DbSet<StorageConfiguracionGlobal> StorageConfiguracionesGlobales => Set<StorageConfiguracionGlobal>();
    public DbSet<EmpresaStorageConfiguracion> EmpresasStorageConfiguracion => Set<EmpresaStorageConfiguracion>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<ClienteReglaVariacionPrecio> ClientesReglasVariacionPrecio => Set<ClienteReglaVariacionPrecio>();
    public DbSet<Contacto> Contactos => Set<Contacto>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoDetalle> PedidoDetalles => Set<PedidoDetalle>();
    public DbSet<PedidoDetalleTalla> PedidosDetalleTalla => Set<PedidoDetalleTalla>();
    public DbSet<PedidoConcepto> PedidoConceptos => Set<PedidoConcepto>();
    public DbSet<PedidoSeguimiento> PedidosSeguimiento => Set<PedidoSeguimiento>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<ProductoVariante> ProductosVariantes => Set<ProductoVariante>();
    public DbSet<ProductoCliente> ProductosClientes => Set<ProductoCliente>();
    
    public DbSet<ProductoCalzado> ProductosCalzado => Set<ProductoCalzado>();
    public DbSet<TallaCalzado> TallasCalzado => Set<TallaCalzado>();
    public DbSet<CatalogoTallaCalzado> CatalogoTallasCalzado => Set<CatalogoTallaCalzado>();
    public DbSet<ClienteTallaCalzado> ClientesTallasCalzado => Set<ClienteTallaCalzado>();
    public DbSet<ClienteFraccionCalzado> ClientesFraccionesCalzado => Set<ClienteFraccionCalzado>();
    public DbSet<ClienteFraccionCalzadoDetalle> ClientesFraccionesCalzadoDetalle => Set<ClienteFraccionCalzadoDetalle>();
    public DbSet<Horma> Hormas => Set<Horma>();
    
    public DbSet<ProductoSerigrafia> ProductosSerigrafia => Set<ProductoSerigrafia>();
    public DbSet<ColorSerigrafia> ColoresSerigrafia => Set<ColorSerigrafia>();
    public DbSet<CategoriaInventario> CategoriasInventario => Set<CategoriaInventario>();
    public DbSet<TipoInventario> TiposInventario => Set<TipoInventario>();
    public DbSet<InventarioItem> InventarioItems => Set<InventarioItem>();
    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();
    public DbSet<InventarioFinishedGood> InventariosFinishedGoods => Set<InventarioFinishedGood>();
    public DbSet<MovimientoFinishedGood> MovimientosFinishedGoods => Set<MovimientoFinishedGood>();
    public DbSet<Posicion> Posiciones => Set<Posicion>();
    public DbSet<GastoFijo> GastosFijos => Set<GastoFijo>();
    public DbSet<Pantalla> Pantallas => Set<Pantalla>();
    public DbSet<Diseno> Disenos => Set<Diseno>();
    public DbSet<TipoProceso> TiposProceso => Set<TipoProceso>();
    public DbSet<TipoProcesoConsumo> TiposProcesoConsumos => Set<TipoProcesoConsumo>();
    public DbSet<EscalaSerigrafia> EscalasSerigrafia => Set<EscalaSerigrafia>();
    public DbSet<PedidoSerigrafia> PedidosSerigrafia => Set<PedidoSerigrafia>();
    public DbSet<PedidoSerigrafiaProcesoDetalle> PedidoSerigrafiaProcesoDetalles => Set<PedidoSerigrafiaProcesoDetalle>();
    public DbSet<PedidoSerigrafiaTalla> PedidoSerigrafiaTallas => Set<PedidoSerigrafiaTalla>();
    public DbSet<PedidoSerigrafiaTallaProceso> PedidoSerigrafiaTallaProcesos => Set<PedidoSerigrafiaTallaProceso>();
    public DbSet<RegistroDestajoProceso> RegistrosDestajoProceso => Set<RegistroDestajoProceso>();
    public DbSet<CotizacionSerigrafia> CotizacionesSerigrafia => Set<CotizacionSerigrafia>();
    public DbSet<CotizacionDetalle> CotizacionDetalles => Set<CotizacionDetalle>();
    public DbSet<CotizacionSerigrafiaProceso> CotizacionSerigrafiaProcesos => Set<CotizacionSerigrafiaProceso>();
    public DbSet<CotizacionVariantePrecio> CotizacionVariantePrecios => Set<CotizacionVariantePrecio>();
    public DbSet<EscalaSerigrafiaTalla> EscalaSerigrafiaTallas => Set<EscalaSerigrafiaTalla>();
    public DbSet<PresupuestoProducto> PresupuestosProducto => Set<PresupuestoProducto>();
    public DbSet<PresupuestoDetalle> PresupuestosDetalle => Set<PresupuestoDetalle>();
    public DbSet<PagoPedido> PagosPedido => Set<PagoPedido>();

    // Facturacion / Cobranza
    public DbSet<NotaEntrega> NotasEntrega => Set<NotaEntrega>();
    public DbSet<NotaEntregaPedido> NotasEntregaPedidos => Set<NotaEntregaPedido>();
    public DbSet<NotaEntregaAsignacion> NotasEntregaAsignaciones => Set<NotaEntregaAsignacion>();
    public DbSet<NotaEntregaDetalle> NotasEntregaDetalle => Set<NotaEntregaDetalle>();
    public DbSet<NotaEntregaDetalleTalla> NotasEntregaDetalleTalla => Set<NotaEntregaDetalleTalla>();
    public DbSet<Factura> Facturas => Set<Factura>();
    public DbSet<FacturaNotaEntrega> FacturasNotasEntrega => Set<FacturaNotaEntrega>();
    public DbSet<FacturaDetalle> FacturaDetalles => Set<FacturaDetalle>();
    public DbSet<FacturaImpuesto> FacturaImpuestos => Set<FacturaImpuesto>();
    public DbSet<FacturaRelacionada> FacturasRelacionadas => Set<FacturaRelacionada>();
    public DbSet<FacturaEvento> FacturaEventos => Set<FacturaEvento>();
    public DbSet<PagoRecibido> PagosRecibidos => Set<PagoRecibido>();
    public DbSet<PagoAplicacionDocumento> PagosAplicacionDocumento => Set<PagoAplicacionDocumento>();
    public DbSet<CargoManualCxC> CargosManualesCxC => Set<CargoManualCxC>();
    public DbSet<ComplementoPago> ComplementosPago => Set<ComplementoPago>();
    public DbSet<ComplementoPagoDocumentoRelacionado> ComplementosPagoDocumentosRelacionados => Set<ComplementoPagoDocumentoRelacionado>();
    public DbSet<ClienteDatoFiscalSnapshot> ClientesDatosFiscalesSnapshot => Set<ClienteDatoFiscalSnapshot>();

    // Cuentas por Pagar
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<CuentaPorPagar> CuentasPorPagar => Set<CuentaPorPagar>();
    public DbSet<PagoCxP> PagosCxP => Set<PagoCxP>();

    // RRHH / Nóminas
    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<TurnoBase> TurnosBase => Set<TurnoBase>();
    public DbSet<TurnoBaseDetalle> TurnosBaseDetalle => Set<TurnoBaseDetalle>();
    public DbSet<RrhhEmpleadoTurno> RrhhEmpleadosTurno => Set<RrhhEmpleadoTurno>();
    public DbSet<DepartamentoRrhh> DepartamentosRrhh => Set<DepartamentoRrhh>();
    public DbSet<NominaCorteRrhh> NominaCortesRrhh => Set<NominaCorteRrhh>();
    public DbSet<FestivoRrhh> FestivosRrhh => Set<FestivoRrhh>();
    public DbSet<RrhhChecador> RrhhChecadores => Set<RrhhChecador>();
    public DbSet<RrhhMarcacion> RrhhMarcaciones => Set<RrhhMarcacion>();
    public DbSet<RrhhAsistencia> RrhhAsistencias => Set<RrhhAsistencia>();
    public DbSet<RrhhEstadoAgente> RrhhEstadosAgente => Set<RrhhEstadoAgente>();
    public DbSet<RrhhLogChecador> RrhhLogsChecador => Set<RrhhLogChecador>();
    public DbSet<RrhhAusencia> RrhhAusencias => Set<RrhhAusencia>();
    public DbSet<Prenomina> Prenominas => Set<Prenomina>();
    public DbSet<PrenominaDetalle> PrenominaDetalles => Set<PrenominaDetalle>();
    public DbSet<PrenominaBono> PrenominasBonos => Set<PrenominaBono>();
    public DbSet<PrenominaPercepcion> PrenominasPercepciones => Set<PrenominaPercepcion>();
    public DbSet<Nomina> Nominas => Set<Nomina>();
    public DbSet<NominaDetalle> NominaDetalles => Set<NominaDetalle>();
    public DbSet<BonoRubroRrhh> BonosRubrosRrhh => Set<BonoRubroRrhh>();
    public DbSet<BonoEstructuraRrhh> BonosEstructurasRrhh => Set<BonoEstructuraRrhh>();
    public DbSet<BonoEstructuraDetalleRrhh> BonosEstructurasDetalleRrhh => Set<BonoEstructuraDetalleRrhh>();
    public DbSet<BonoDistribucionPeriodoRrhh> BonosDistribucionesPeriodoRrhh => Set<BonoDistribucionPeriodoRrhh>();
    public DbSet<BonoDistribucionEmpleadoRrhh> BonosDistribucionesEmpleadosRrhh => Set<BonoDistribucionEmpleadoRrhh>();
    public DbSet<BonoDistribucionEmpleadoDetalleRrhh> BonosDistribucionesEmpleadosDetalleRrhh => Set<BonoDistribucionEmpleadoDetalleRrhh>();
    public DbSet<NominaBono> NominasBonos => Set<NominaBono>();
    public DbSet<NominaBonoDetalle> NominasBonosDetalle => Set<NominaBonoDetalle>();
    public DbSet<NominaPercepcionTipo> NominasPercepcionesTipos => Set<NominaPercepcionTipo>();
    public DbSet<NominaPercepcion> NominasPercepciones => Set<NominaPercepcion>();
    public DbSet<DeduccionTipoRrhh> DeduccionesTiposRrhh => Set<DeduccionTipoRrhh>();
    public DbSet<NominaDeduccion> NominasDeducciones => Set<NominaDeduccion>();
    public DbSet<NominaConceptoConfigRrhh> NominasConceptosConfigRrhh => Set<NominaConceptoConfigRrhh>();
    public DbSet<EmpleadoConceptoRrhh> EmpleadosConceptosRrhh => Set<EmpleadoConceptoRrhh>();
    public DbSet<NominaProvisionDetalleRrhh> NominasProvisionesDetalleRrhh => Set<NominaProvisionDetalleRrhh>();
    public DbSet<RrhhBancoHorasMovimiento> RrhhBancoHorasMovimientos => Set<RrhhBancoHorasMovimiento>();

    // Esquemas de Pago / Destajo
    public DbSet<EsquemaPago> EsquemasPago => Set<EsquemaPago>();
    public DbSet<EsquemaPagoTarifa> EsquemasPagoTarifa => Set<EsquemaPagoTarifa>();
    public DbSet<EmpleadoEsquemaPago> EmpleadosEsquemaPago => Set<EmpleadoEsquemaPago>();
    public DbSet<ValeDestajo> ValesDestajo => Set<ValeDestajo>();
    public DbSet<ValeDestajoDetalle> ValesDestajoDetalle => Set<ValeDestajoDetalle>();

    // Auth
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<TipoUsuario> TiposUsuario => Set<TipoUsuario>();
    public DbSet<ModuloAcceso> ModulosAcceso => Set<ModuloAcceso>();
    public DbSet<EmpresaModuloAcceso> EmpresasModulosAcceso => Set<EmpresaModuloAcceso>();
    public DbSet<Capacidad> Capacidades => Set<Capacidad>();
    public DbSet<TipoUsuarioCapacidad> TipoUsuarioCapacidades => Set<TipoUsuarioCapacidad>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigurarEmpresa(modelBuilder);
        ConfigurarAuditoria(modelBuilder);
        ConfigurarPlanesSuscripciones(modelBuilder);
        ConfigurarAppConfig(modelBuilder);
        ConfigurarNominaConfiguracionGlobal(modelBuilder);
        ConfigurarStorage(modelBuilder);
        ConfigurarCliente(modelBuilder);
        ConfigurarContacto(modelBuilder);
        ConfigurarPedido(modelBuilder);
        ConfigurarPedidoDetalle(modelBuilder);
        ConfigurarPedidoConcepto(modelBuilder);
        ConfigurarPedidoSeguimiento(modelBuilder);
        ConfigurarProducto(modelBuilder);
        ConfigurarProductoCliente(modelBuilder);
        ConfigurarCalzado(modelBuilder);
        ConfigurarInventario(modelBuilder);
        ConfigurarSerigrafia(modelBuilder);
        ConfigurarCatalogosProduccion(modelBuilder);
        ConfigurarPresupuesto(modelBuilder);
        ConfigurarPagoPedido(modelBuilder);
        ConfigurarFacturacion(modelBuilder);
        ConfigurarCuentasPorPagar(modelBuilder);
        ConfigurarTurnos(modelBuilder);
        ConfigurarAsistenciaRrhh(modelBuilder);
        ConfigurarEmpleados(modelBuilder);
        ConfigurarPrenominas(modelBuilder);
        ConfigurarNominas(modelBuilder);
        ConfigurarEsquemasPago(modelBuilder);
        ConfigurarAuth(modelBuilder);
        NormalizarNombresTablasMinusculas(modelBuilder);
        ConfigurarQueryFilters(modelBuilder);
    }

    private static void NormalizarNombresTablasMinusculas(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.IsOwned())
            {
                continue;
            }

            var tableName = entityType.GetTableName();
            if (string.IsNullOrWhiteSpace(tableName))
            {
                continue;
            }

            entityType.SetTableName(tableName.ToLowerInvariant());
        }
    }

    private void ConfigurarAuditoria(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Accion).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Entidad).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Detalle).HasMaxLength(2000);
            entity.Property(e => e.IpAddress).HasMaxLength(100);

            entity.HasIndex(e => e.EmpresaId);
            entity.HasIndex(e => e.UsuarioId);
            entity.HasIndex(e => e.Fecha);
            entity.HasIndex(e => e.Entidad);
        });
    }

    private void ConfigurarAsistenciaRrhh(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RrhhChecador>(entity =>
        {
            entity.ToTable("rrhh_checador");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(120).IsRequired();
            entity.Property(e => e.NumeroSerie).HasMaxLength(100);
            entity.Property(e => e.Marca).HasMaxLength(60);
            entity.Property(e => e.Modelo).HasMaxLength(60);
            entity.Property(e => e.Ip).HasMaxLength(50);
            entity.Property(e => e.Ubicacion).HasMaxLength(150);
            entity.Property(e => e.ZonaHoraria).HasMaxLength(60);
            entity.Property(e => e.UltimoEventoLeido).HasMaxLength(200);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.EmpresaId, e.Nombre });
            entity.HasIndex(e => new { e.EmpresaId, e.NumeroSerie }).IsUnique();
        });

        modelBuilder.Entity<RrhhMarcacion>(entity =>
        {
            entity.ToTable("rrhh_marcacion");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CodigoChecador).HasMaxLength(100).IsRequired();
            entity.Property(e => e.FechaHoraMarcacionLocal).HasColumnType("datetime(6)");
            entity.Property(e => e.ZonaHorariaAplicada).HasMaxLength(100);
            entity.Property(e => e.TipoMarcacionRaw).HasMaxLength(50);
            entity.Property(e => e.Origen).HasMaxLength(50);
            entity.Property(e => e.EventoIdExterno).HasMaxLength(100);
            entity.Property(e => e.HashUnico).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ClasificacionOperativa).HasConversion<int>();
            entity.Property(e => e.ResultadoProcesamiento).HasMaxLength(500);
            entity.Property(e => e.ObservacionManual).HasMaxLength(500);
            entity.Property(e => e.PayloadRaw).HasMaxLength(4000);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Checador)
                .WithMany(c => c.Marcaciones)
                .HasForeignKey(e => e.ChecadorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Empleado)
                .WithMany(e => e.Marcaciones)
                .HasForeignKey(e => e.EmpleadoId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.HashUnico).IsUnique();
            entity.HasIndex(e => new { e.EmpresaId, e.CodigoChecador, e.FechaHoraMarcacionUtc });
            entity.HasIndex(e => new { e.ChecadorId, e.FechaHoraMarcacionUtc });
        });

        modelBuilder.Entity<RrhhAsistencia>(entity =>
        {
            entity.ToTable("rrhh_asistencia");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Fecha).HasColumnType("date");
            entity.Property(e => e.HoraEntradaProgramada).HasColumnType("time(6)");
            entity.Property(e => e.HoraSalidaProgramada).HasColumnType("time(6)");
            entity.Property(e => e.HoraEntradaReal).HasColumnType("time(6)");
            entity.Property(e => e.HoraSalidaReal).HasColumnType("time(6)");
            entity.Property(e => e.Estatus).HasConversion<int>();
            entity.Property(e => e.Observaciones).HasMaxLength(500);
            entity.Property(e => e.ResumenDescansos).HasMaxLength(1000);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Empleado)
                .WithMany(e => e.Asistencias)
                .HasForeignKey(e => e.EmpleadoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TurnoBase)
                .WithMany()
                .HasForeignKey(e => e.TurnoBaseId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.EmpresaId, e.EmpleadoId, e.Fecha }).IsUnique();
            entity.HasIndex(e => new { e.EmpresaId, e.Fecha, e.Estatus });
        });

        modelBuilder.Entity<RrhhLogChecador>(entity =>
        {
            entity.ToTable("rrhh_logchecador");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nivel).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Mensaje).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Detalle).HasMaxLength(2000);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Checador)
                .WithMany(c => c.Logs)
                .HasForeignKey(e => e.ChecadorId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.EmpresaId, e.FechaUtc });
            entity.HasIndex(e => e.ChecadorId);
        });

        modelBuilder.Entity<RrhhEstadoAgente>(entity =>
        {
            entity.ToTable("rrhh_estado_agente");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NombreAgente).HasMaxLength(120).IsRequired();
            entity.Property(e => e.Hostname).HasMaxLength(120);
            entity.Property(e => e.Version).HasMaxLength(40);
            entity.Property(e => e.UltimoError).HasMaxLength(500);
            entity.Property(e => e.UltimoLogNivel).HasMaxLength(20);
            entity.Property(e => e.UltimoLogMensaje).HasMaxLength(500);
            entity.Property(e => e.UltimoLogDetalle).HasMaxLength(2000);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.EmpresaId, e.NombreAgente }).IsUnique();
            entity.HasIndex(e => new { e.EmpresaId, e.UltimoHeartbeatUtc });
        });
    }

    private void ConfigurarTurnos(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TurnoBase>(entity =>
        {
            entity.ToTable("rrhh_turno");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(120).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(300);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.EmpresaId, e.Nombre }).IsUnique();
        });

        modelBuilder.Entity<TurnoBaseDetalle>(entity =>
        {
            entity.ToTable("rrhh_turno_detalle");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DiaSemana).HasConversion<int>();
            entity.Property(e => e.HoraEntrada).HasColumnType("time(6)");
            entity.Property(e => e.HoraSalida).HasColumnType("time(6)");
            entity.Property(e => e.Descanso1Inicio).HasColumnType("time(6)");
            entity.Property(e => e.Descanso1Fin).HasColumnType("time(6)");
            entity.Property(e => e.Descanso2Inicio).HasColumnType("time(6)");
            entity.Property(e => e.Descanso2Fin).HasColumnType("time(6)");

            entity.HasOne(e => e.TurnoBase)
                .WithMany(t => t.Detalles)
                .HasForeignKey(e => e.TurnoBaseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.TurnoBaseId, e.DiaSemana }).IsUnique();
        });

        modelBuilder.Entity<RrhhEmpleadoTurno>(entity =>
        {
            entity.ToTable("rrhh_empleado_turno");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VigenteDesde).HasColumnType("date");
            entity.Property(e => e.VigenteHasta).HasColumnType("date");
            entity.Property(e => e.Observaciones).HasMaxLength(500);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Empleado)
                .WithMany(e => e.TurnosVigencia)
                .HasForeignKey(e => e.EmpleadoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.TurnoBase)
                .WithMany(t => t.EmpleadosVigencia)
                .HasForeignKey(e => e.TurnoBaseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.EmpresaId, e.EmpleadoId, e.VigenteDesde }).IsUnique();
            entity.HasIndex(e => new { e.EmpresaId, e.EmpleadoId, e.VigenteDesde, e.VigenteHasta });
        });

        modelBuilder.Entity<DepartamentoRrhh>(entity =>
        {
            entity.ToTable("rrhh_departamento");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(120).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(300);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.EmpresaId, e.Nombre }).IsUnique();
        });

        modelBuilder.Entity<NominaCorteRrhh>(entity =>
        {
            entity.ToTable("rrhh_nomina_corte");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PeriodicidadPago).HasConversion<int>();
            entity.Property(e => e.DiaCorteSemana).HasConversion<int>();
            entity.Property(e => e.DiaCorteMes).HasDefaultValue(15);
            entity.Property(e => e.DiaPagoSugerido).HasConversion<int>();

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.EmpresaId, e.PeriodicidadPago }).IsUnique();
        });

        modelBuilder.Entity<FestivoRrhh>(entity =>
        {
            entity.ToTable("rrhh_festivo");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Fecha).HasColumnType("date");
            entity.Property(e => e.Nombre).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Tipo).HasConversion<int>();
            entity.Property(e => e.FactorPago).HasPrecision(8, 4);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.EmpresaId, e.Fecha }).IsUnique();
        });

        modelBuilder.Entity<RrhhAusencia>(entity =>
        {
            entity.ToTable("rrhh_ausencia");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Tipo).HasConversion<int>();
            entity.Property(e => e.Estatus).HasConversion<int>();
            entity.Property(e => e.FechaInicio).HasColumnType("date");
            entity.Property(e => e.FechaFin).HasColumnType("date");
            entity.Property(e => e.Horas).HasPrecision(18, 2);
            entity.Property(e => e.Motivo).HasMaxLength(250);
            entity.Property(e => e.Observaciones).HasMaxLength(500);
            entity.Property(e => e.AprobadoPor).HasMaxLength(120);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Empleado)
                .WithMany(e => e.Ausencias)
                .HasForeignKey(e => e.EmpleadoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.EmpleadoId);
            entity.HasIndex(e => new { e.EmpresaId, e.EmpleadoId, e.FechaInicio, e.FechaFin });
            entity.HasIndex(e => new { e.EmpresaId, e.Estatus, e.Tipo });
        });
    }

    private void ConfigurarEmpresa(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Codigo).HasMaxLength(20).IsRequired();
            entity.Property(e => e.RazonSocial).HasMaxLength(200).IsRequired();
            entity.Property(e => e.NombreComercial).HasMaxLength(200);
            entity.Property(e => e.Rfc).HasMaxLength(20);
            entity.Property(e => e.Slogan).HasMaxLength(200);
            entity.Property(e => e.Estado).HasConversion<int>().HasDefaultValue(EmpresaEstado.Demo);

            entity.HasOne(e => e.PlanActual)
                .WithMany()
                .HasForeignKey(e => e.PlanActualId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.Codigo).IsUnique();
            entity.HasIndex(e => e.Estado);
            entity.HasIndex(e => e.IsSuspended);
        });
    }

    private void ConfigurarPlanesSuscripciones(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Plan>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.PrecioMensual).HasPrecision(18, 2);
            entity.Property(e => e.PrecioAnual).HasPrecision(18, 2);
            entity.Property(e => e.ModulosIncluidos).HasMaxLength(500);

            entity.HasIndex(e => e.Nombre).IsUnique();
        });

        modelBuilder.Entity<SuscripcionEmpresa>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Estado).HasConversion<int>();
            entity.Property(e => e.Periodicidad).HasConversion<int>();

            entity.HasOne(e => e.Empresa)
                .WithMany(e => e.Suscripciones)
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Plan)
                .WithMany(p => p.Suscripciones)
                .HasForeignKey(e => e.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.EmpresaId);
            entity.HasIndex(e => e.PlanId);
            entity.HasIndex(e => e.Estado);
        });

        modelBuilder.Entity<PagoSuscripcion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Monto).HasPrecision(18, 2);
            entity.Property(e => e.MetodoPago).HasConversion<int>();
            entity.Property(e => e.Referencia).HasMaxLength(100);
            entity.Property(e => e.Notas).HasMaxLength(500);

            entity.HasOne(e => e.SuscripcionEmpresa)
                .WithMany(s => s.Pagos)
                .HasForeignKey(e => e.SuscripcionEmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.SuscripcionEmpresaId);
            entity.HasIndex(e => e.FechaPago);
        });
    }

    private void ConfigurarFacturacion(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotaEntrega>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NumeroNota).HasMaxLength(50).IsRequired();
            entity.Property(e => e.FechaNota).HasColumnType("datetime(6)");
            entity.Property(e => e.Estatus).HasConversion<int>();
            entity.Property(e => e.NoRequiereFactura).HasDefaultValue(false);
            entity.Property(e => e.Observaciones).HasMaxLength(500);
            entity.Property(e => e.TextoPagare).HasMaxLength(4000);
            entity.Property(e => e.PdfUrl).HasMaxLength(500);
            entity.Property(e => e.Subtotal).HasPrecision(18, 2);
            entity.Property(e => e.Impuestos).HasPrecision(18, 2);
            entity.Property(e => e.Total).HasPrecision(18, 2);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.NotasEntrega)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Pedido)
                .WithMany(p => p.NotasEntrega)
                .HasForeignKey(e => e.PedidoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Navigation(e => e.PedidosRelacionados).AutoInclude();

            entity.HasIndex(e => new { e.EmpresaId, e.NumeroNota }).IsUnique();
            entity.HasIndex(e => e.ClienteId);
            entity.HasIndex(e => e.PedidoId);
            entity.HasIndex(e => e.FechaNota);
        });

        modelBuilder.Entity<NotaEntregaPedido>(entity =>
        {
            entity.ToTable("contabilidad_nota_entrega_pedido");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Orden).HasDefaultValue(0);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.NotaEntrega)
                .WithMany(n => n.PedidosRelacionados)
                .HasForeignKey(e => e.NotaEntregaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Pedido)
                .WithMany(p => p.NotasEntregaRelacionadas)
                .HasForeignKey(e => e.PedidoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.NotaEntregaId, e.PedidoId }).IsUnique();
            entity.HasIndex(e => new { e.PedidoId, e.EsPrincipal });
        });

        modelBuilder.Entity<NotaEntregaAsignacion>(entity =>
        {
            entity.ToTable("contabilidad_nota_entrega_asignacion");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TipoOrigen).HasConversion<int>();
            entity.Property(e => e.Cantidad).HasPrecision(18, 4);
            entity.Property(e => e.CantidadFgTomada).HasPrecision(18, 4);
            entity.Property(e => e.PrecioUnitario).HasPrecision(18, 4);
            entity.Property(e => e.Importe).HasPrecision(18, 2);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.NotaEntrega)
                .WithMany(n => n.Asignaciones)
                .HasForeignKey(e => e.NotaEntregaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Pedido)
                .WithMany(p => p.NotasEntregaAsignaciones)
                .HasForeignKey(e => e.PedidoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PedidoDetalle)
                .WithMany()
                .HasForeignKey(e => e.PedidoDetalleId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.PedidoDetalleTalla)
                .WithMany()
                .HasForeignKey(e => e.PedidoDetalleTallaId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.PedidoConcepto)
                .WithMany()
                .HasForeignKey(e => e.PedidoConceptoId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.NotaEntregaId);
            entity.HasIndex(e => e.PedidoId);
            entity.HasIndex(e => e.PedidoDetalleId);
            entity.HasIndex(e => e.PedidoDetalleTallaId);
            entity.HasIndex(e => e.PedidoConceptoId);
        });

        modelBuilder.Entity<NotaEntregaDetalle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Descripcion).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Cantidad).HasPrecision(18, 4);
            entity.Property(e => e.PrecioUnitario).HasPrecision(18, 4);
            entity.Property(e => e.Importe).HasPrecision(18, 2);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.NotaEntrega)
                .WithMany(n => n.Detalles)
                .HasForeignKey(e => e.NotaEntregaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.PedidoDetalle)
                .WithMany(d => d.NotasEntregaDetalle)
                .HasForeignKey(e => e.PedidoDetalleId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.PedidoConcepto)
                .WithMany()
                .HasForeignKey(e => e.PedidoConceptoId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.NotaEntregaId);
            entity.HasIndex(e => e.PedidoDetalleId);
            entity.HasIndex(e => e.PedidoConceptoId);
        });

        modelBuilder.Entity<NotaEntregaDetalleTalla>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Talla).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Cantidad).HasPrecision(18, 4);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.NotaEntregaDetalle)
                .WithMany(d => d.Tallas)
                .HasForeignKey(e => e.NotaEntregaDetalleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.NotaEntregaDetalleId);
            entity.HasIndex(e => new { e.NotaEntregaDetalleId, e.Talla });
        });

        modelBuilder.Entity<Factura>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Serie).HasMaxLength(20);
            entity.Property(e => e.FolioInterno).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SerieFiscal).HasMaxLength(20);
            entity.Property(e => e.FolioFiscal).HasMaxLength(50);
            entity.Property(e => e.ExternalDocumentId).HasMaxLength(100);
            entity.Property(e => e.UuidFiscal).HasMaxLength(50);
            entity.Property(e => e.Moneda).HasMaxLength(10).IsRequired();
            entity.Property(e => e.TipoCambio).HasPrecision(18, 6);
            entity.Property(e => e.LugarExpedicionCp).HasMaxLength(10);
            entity.Property(e => e.MetodoPagoSat).HasMaxLength(10).IsRequired();
            entity.Property(e => e.FormaPagoSat).HasMaxLength(10).IsRequired();
            entity.Property(e => e.UsoCfdi).HasMaxLength(10);
            entity.Property(e => e.Exportacion).HasMaxLength(10);
            entity.Property(e => e.CondicionesPago).HasMaxLength(250);
            entity.Property(e => e.Observaciones).HasMaxLength(500);
            entity.Property(e => e.Subtotal).HasPrecision(18, 2);
            entity.Property(e => e.DescuentoTotal).HasPrecision(18, 2);
            entity.Property(e => e.TotalImpuestosTrasladados).HasPrecision(18, 2);
            entity.Property(e => e.TotalImpuestosRetenidos).HasPrecision(18, 2);
            entity.Property(e => e.Total).HasPrecision(18, 2);
            entity.Property(e => e.CancellationStatus).HasMaxLength(50);
            entity.Property(e => e.XmlUrl).HasMaxLength(500);
            entity.Property(e => e.PdfUrl).HasMaxLength(500);
            entity.Property(e => e.AcuseCancelacionUrl).HasMaxLength(500);
            entity.Property(e => e.ErrorCode).HasMaxLength(50);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.Property(e => e.JsonSnapshotComercial).HasColumnType("longtext");
            entity.Property(e => e.TipoComprobante).HasConversion<int>();
            entity.Property(e => e.Estatus).HasConversion<int>();

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.Facturas)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Pedido)
                .WithMany(p => p.Facturas)
                .HasForeignKey(e => e.PedidoId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.NotaEntrega)
                .WithMany()
                .HasForeignKey(e => e.NotaEntregaId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.EmpresaId, e.FolioInterno }).IsUnique();
            entity.HasIndex(e => e.ClienteId);
            entity.HasIndex(e => e.PedidoId);
            entity.HasIndex(e => e.NotaEntregaId);
            entity.HasIndex(e => e.Estatus);
            entity.HasIndex(e => e.UuidFiscal);
            entity.HasIndex(e => e.FechaEmision);
        });

        modelBuilder.Entity<FacturaNotaEntrega>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Subtotal).HasPrecision(18, 2);
            entity.Property(e => e.Impuestos).HasPrecision(18, 2);
            entity.Property(e => e.Total).HasPrecision(18, 2);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Factura)
                .WithMany(f => f.NotasEntregaRelacionadas)
                .HasForeignKey(e => e.FacturaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.NotaEntrega)
                .WithMany(n => n.FacturasRelacionadas)
                .HasForeignKey(e => e.NotaEntregaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.FacturaId);
            entity.HasIndex(e => e.NotaEntregaId);
        });

        modelBuilder.Entity<FacturaDetalle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReferenciaOrigen).HasMaxLength(100);
            entity.Property(e => e.Cantidad).HasPrecision(18, 4);
            entity.Property(e => e.ClaveUnidadSat).HasMaxLength(10);
            entity.Property(e => e.Unidad).HasMaxLength(50);
            entity.Property(e => e.ClaveProductoServicioSat).HasMaxLength(20);
            entity.Property(e => e.Descripcion).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ValorUnitario).HasPrecision(18, 4);
            entity.Property(e => e.Descuento).HasPrecision(18, 2);
            entity.Property(e => e.ObjetoImpuesto).HasMaxLength(10);
            entity.Property(e => e.Importe).HasPrecision(18, 2);

            entity.HasOne(e => e.Factura)
                .WithMany(f => f.Detalles)
                .HasForeignKey(e => e.FacturaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.FacturaId);
        });

        modelBuilder.Entity<FacturaImpuesto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Impuesto).HasMaxLength(20).IsRequired();
            entity.Property(e => e.TipoFactor).HasMaxLength(20).IsRequired();
            entity.Property(e => e.TasaOCuota).HasPrecision(18, 6);
            entity.Property(e => e.Base).HasPrecision(18, 2);
            entity.Property(e => e.Importe).HasPrecision(18, 2);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Factura)
                .WithMany(f => f.Impuestos)
                .HasForeignKey(e => e.FacturaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.FacturaDetalle)
                .WithMany(d => d.Impuestos)
                .HasForeignKey(e => e.FacturaDetalleId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.FacturaId);
            entity.HasIndex(e => e.FacturaDetalleId);
        });

        modelBuilder.Entity<FacturaRelacionada>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TipoRelacionCfdi).HasMaxLength(10).IsRequired();
            entity.Property(e => e.UuidRelacionado).HasMaxLength(50).IsRequired();

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Factura)
                .WithMany(f => f.FacturasRelacionadas)
                .HasForeignKey(e => e.FacturaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.FacturaId);
        });

        modelBuilder.Entity<FacturaEvento>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Tipo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Usuario).HasMaxLength(100);
            entity.Property(e => e.Descripcion).HasMaxLength(1000);
            entity.Property(e => e.DatosJson).HasColumnType("longtext");

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Factura)
                .WithMany(f => f.Eventos)
                .HasForeignKey(e => e.FacturaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.FacturaId);
            entity.HasIndex(e => e.Fecha);
        });

        modelBuilder.Entity<PagoRecibido>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Monto).HasPrecision(18, 2);
            entity.Property(e => e.FormaPagoSat).HasMaxLength(10).IsRequired();
            entity.Property(e => e.MedioCobroInterno).HasMaxLength(50);
            entity.Property(e => e.Referencia).HasMaxLength(100);
            entity.Property(e => e.Notas).HasMaxLength(500);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.PagosRecibidos)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Pedido)
                .WithMany(p => p.PagosRecibidos)
                .HasForeignKey(e => e.PedidoId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.NotaEntrega)
                .WithMany()
                .HasForeignKey(e => e.NotaEntregaId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.ClienteId);
            entity.HasIndex(e => e.PedidoId);
            entity.HasIndex(e => e.NotaEntregaId);
            entity.HasIndex(e => e.FechaPago);
        });

        modelBuilder.Entity<PagoAplicacionDocumento>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ImporteAplicado).HasPrecision(18, 2);
            entity.Property(e => e.SaldoAnterior).HasPrecision(18, 2);
            entity.Property(e => e.SaldoInsoluto).HasPrecision(18, 2);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.PagoRecibido)
                .WithMany(p => p.Aplicaciones)
                .HasForeignKey(e => e.PagoRecibidoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Factura)
                .WithMany(f => f.PagosAplicados)
                .HasForeignKey(e => e.FacturaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.PagoRecibidoId);
            entity.HasIndex(e => e.FacturaId);
        });

        modelBuilder.Entity<CargoManualCxC>(entity =>
        {
            entity.ToTable("contabilidad_cargo_manual_cxc");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Tipo).HasConversion<int>();
            entity.Property(e => e.FechaCargo).HasColumnType("datetime(6)");
            entity.Property(e => e.Referencia).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Concepto).HasMaxLength(250).IsRequired();
            entity.Property(e => e.Monto).HasPrecision(18, 2);
            entity.Property(e => e.Notas).HasMaxLength(500);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.CargosManualesCxC)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Pedido)
                .WithMany()
                .HasForeignKey(e => e.PedidoId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.NotaEntrega)
                .WithMany()
                .HasForeignKey(e => e.NotaEntregaId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Factura)
                .WithMany()
                .HasForeignKey(e => e.FacturaId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.EmpresaId, e.ClienteId, e.FechaCargo });
            entity.HasIndex(e => e.FacturaId);
            entity.HasIndex(e => e.NotaEntregaId);
        });

        modelBuilder.Entity<ComplementoPago>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Estatus).HasConversion<int>();
            entity.Property(e => e.Serie).HasMaxLength(20);
            entity.Property(e => e.FolioInterno).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SerieFiscal).HasMaxLength(20);
            entity.Property(e => e.FolioFiscal).HasMaxLength(50);
            entity.Property(e => e.ExternalDocumentId).HasMaxLength(100);
            entity.Property(e => e.UuidFiscal).HasMaxLength(50);
            entity.Property(e => e.Moneda).HasMaxLength(10).IsRequired();
            entity.Property(e => e.TipoCambio).HasPrecision(18, 6);
            entity.Property(e => e.MontoTotalPagos).HasPrecision(18, 2);
            entity.Property(e => e.XmlUrl).HasMaxLength(500);
            entity.Property(e => e.PdfUrl).HasMaxLength(500);
            entity.Property(e => e.ErrorCode).HasMaxLength(50);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Cliente)
                .WithMany()
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PagoRecibido)
                .WithMany(p => p.ComplementosPago)
                .HasForeignKey(e => e.PagoRecibidoId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.EmpresaId, e.FolioInterno }).IsUnique();
            entity.HasIndex(e => e.ClienteId);
            entity.HasIndex(e => e.PagoRecibidoId);
            entity.HasIndex(e => e.UuidFiscal);
        });

        modelBuilder.Entity<ComplementoPagoDocumentoRelacionado>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UuidDocumentoRelacionado).HasMaxLength(50);
            entity.Property(e => e.MonedaDocumento).HasMaxLength(10).IsRequired();
            entity.Property(e => e.TipoCambioDocumento).HasPrecision(18, 6);
            entity.Property(e => e.SaldoAnterior).HasPrecision(18, 2);
            entity.Property(e => e.ImportePagado).HasPrecision(18, 2);
            entity.Property(e => e.SaldoInsoluto).HasPrecision(18, 2);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ComplementoPago)
                .WithMany(c => c.DocumentosRelacionados)
                .HasForeignKey(e => e.ComplementoPagoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Factura)
                .WithMany(f => f.ComplementosRelacionados)
                .HasForeignKey(e => e.FacturaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.ComplementoPagoId);
            entity.HasIndex(e => e.FacturaId);
        });

        modelBuilder.Entity<ClienteDatoFiscalSnapshot>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RazonSocial).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Rfc).HasMaxLength(20).IsRequired();
            entity.Property(e => e.RegimenFiscalReceptor).HasMaxLength(10);
            entity.Property(e => e.DomicilioFiscalCp).HasMaxLength(10);
            entity.Property(e => e.UsoCfdi).HasMaxLength(10);
            entity.Property(e => e.EmailFactura).HasMaxLength(100);
            entity.Property(e => e.JsonDatos).HasColumnType("longtext");

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.DatosFiscalesSnapshot)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Factura)
                .WithMany()
                .HasForeignKey(e => e.FacturaId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.ClienteId);
            entity.HasIndex(e => e.FacturaId);
        });
    }

    private void ConfigurarAppConfig(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppConfig>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Clave).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Valor).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(500);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.EmpresaId, e.Clave }).IsUnique();
        });
    }

    private void ConfigurarNominaConfiguracionGlobal(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NominaConfiguracionGlobal>(entity =>
        {
            entity.ToTable("rrhh_nomina_configuracion_global");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TablaIsrJson).IsRequired();
            entity.Property(e => e.TablaSubsidioJson).IsRequired();
        });
    }

    private void ConfigurarStorage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StorageConfiguracionGlobal>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StorageProvider).HasConversion<int>();
            entity.Property(e => e.BasePath).HasMaxLength(1000);
            entity.Property(e => e.PathTemplate).HasMaxLength(500).IsRequired();
        });

        modelBuilder.Entity<EmpresaStorageConfiguracion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StorageProvider).HasConversion<int>();
            entity.Property(e => e.BasePath).HasMaxLength(1000);
            entity.Property(e => e.PathTemplate).HasMaxLength(500).IsRequired();

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.EmpresaId).IsUnique();
        });
    }

    private void ConfigurarCliente(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Codigo).HasMaxLength(20).IsRequired();
            entity.Property(e => e.RazonSocial).HasMaxLength(200).IsRequired();
            entity.Property(e => e.NombreComercial).HasMaxLength(200);
            entity.Property(e => e.RfcCif).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EmailFacturacion).HasMaxLength(100);
            entity.Property(e => e.EmailCobranza).HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.Direccion).HasMaxLength(250);
            entity.Property(e => e.Ciudad).HasMaxLength(100);
            entity.Property(e => e.Estado).HasMaxLength(100);
            entity.Property(e => e.CodigoPostal).HasMaxLength(10);
            entity.Property(e => e.Pais).HasMaxLength(50);
            entity.Property(e => e.IndustriaPersonalizada).HasMaxLength(120);
            entity.Property(e => e.DomicilioFiscalCp).HasMaxLength(10);
            entity.Property(e => e.RegimenFiscalReceptor).HasMaxLength(10);
            entity.Property(e => e.UsoCfdi).HasMaxLength(10);
            entity.Property(e => e.LimiteCredito).HasPrecision(18, 2);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.EmpresaId, e.Codigo }).IsUnique();
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.RfcCif);
        });

        modelBuilder.Entity<ClienteReglaVariacionPrecio>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Dimension).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Valor).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PorcentajeVariacionSugerido).HasPrecision(9, 4);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.ReglasVariacionPrecio)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ClienteId);
            entity.HasIndex(e => new { e.ClienteId, e.Dimension, e.Valor, e.Orden });
        });
    }

    private void ConfigurarPedidoSeguimiento(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PedidoSeguimiento>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Titulo).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(2000);

            entity.HasOne(e => e.Pedido)
                .WithMany(p => p.Seguimientos)
                .HasForeignKey(e => e.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.PedidoId);
            entity.HasIndex(e => e.Fecha);
            entity.HasIndex(e => e.Completado);
        });
    }

    private void ConfigurarContacto(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contacto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Cargo).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.Movil).HasMaxLength(20);
            
            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.Contactos)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(e => e.ClienteId);
        });
    }

    private void ConfigurarPedido(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NumeroPedido).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Subtotal).HasPrecision(18, 2);
            entity.Property(e => e.Impuestos).HasPrecision(18, 2);
            entity.Property(e => e.Total).HasPrecision(18, 2);
            entity.Property(e => e.Observaciones).HasMaxLength(500);

            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.Pedidos)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.NumeroPedido).IsUnique();
            entity.HasIndex(e => e.ClienteId);
            entity.HasIndex(e => e.Estado);
            entity.HasIndex(e => e.FechaPedido);
        });
    }

    private void ConfigurarPagoPedido(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PagoPedido>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Monto).HasPrecision(18, 2);
            entity.Property(e => e.MetodoPagoSat).HasMaxLength(10).IsRequired();
            entity.Property(e => e.MedioCobroInterno).HasMaxLength(50);
            entity.Property(e => e.Referencia).HasMaxLength(100);
            entity.Property(e => e.Notas).HasMaxLength(500);

            entity.HasOne(e => e.Pedido)
                .WithMany(p => p.Pagos)
                .HasForeignKey(e => e.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.PedidoId);
            entity.HasIndex(e => e.FechaPago);
        });
    }

    private void ConfigurarPedidoDetalle(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PedidoDetalle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PrecioUnitario).HasPrecision(18, 2);
            entity.Property(e => e.Descuento).HasPrecision(18, 2);
            entity.Property(e => e.Total).HasPrecision(18, 2);
            entity.Property(e => e.TallaBaseCalzado).HasMaxLength(10);
            entity.Property(e => e.VariacionValor).HasMaxLength(50);
            entity.Property(e => e.Especificaciones).HasMaxLength(500);
            
            entity.HasOne(e => e.Pedido)
                .WithMany(p => p.Detalles)
                .HasForeignKey(e => e.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Producto)
                .WithMany(p => p.PedidoDetalles)
                .HasForeignKey(e => e.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ProductoVariante)
                .WithMany(v => v.PedidoDetalles)
                .HasForeignKey(e => e.ProductoVarianteId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.CotizacionSerigrafia)
                .WithMany()
                .HasForeignKey(e => e.CotizacionSerigrafiaId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ClienteFraccionCalzado)
                .WithMany(f => f.PedidoDetalles)
                .HasForeignKey(e => e.ClienteFraccionCalzadoId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasIndex(e => e.PedidoId);
            entity.HasIndex(e => e.ProductoId);
            entity.HasIndex(e => e.ProductoVarianteId);
            entity.HasIndex(e => e.CotizacionSerigrafiaId);
            entity.HasIndex(e => e.ClienteFraccionCalzadoId);
        });

        modelBuilder.Entity<PedidoDetalleTalla>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Talla).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Cantidad).HasPrecision(18, 4);
            entity.Property(e => e.PrecioUnitario).HasPrecision(18, 4);
            entity.Property(e => e.Total).HasPrecision(18, 2);
            entity.Property(e => e.PorcentajeVariacion).HasPrecision(9, 4);

            entity.HasOne(e => e.PedidoDetalle)
                .WithMany(d => d.DetallesTalla)
                .HasForeignKey(e => e.PedidoDetalleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ProductoVariante)
                .WithMany(v => v.PedidoDetallesTalla)
                .HasForeignKey(e => e.ProductoVarianteId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.PedidoDetalleId);
            entity.HasIndex(e => e.ProductoVarianteId);
            entity.HasIndex(e => new { e.PedidoDetalleId, e.Talla });
        });
    }

    private void ConfigurarPedidoConcepto(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PedidoConcepto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Descripcion).HasMaxLength(500).IsRequired();
            entity.Property(e => e.PrecioUnitario).HasPrecision(18, 2);
            entity.Property(e => e.Total).HasPrecision(18, 2);
            entity.Property(e => e.Completado).HasDefaultValue(false);
            entity.Property(e => e.FechaCompletado).HasColumnType("datetime(6)");

            entity.HasOne(e => e.Pedido)
                .WithMany(p => p.Conceptos)
                .HasForeignKey(e => e.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.PedidoId);
            entity.HasIndex(e => e.Tipo);
        });
    }

    private void ConfigurarProducto(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Codigo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Referencia).HasMaxLength(100).HasColumnName("Referencia");
            entity.Property(e => e.Nombre).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.PrecioBase).HasPrecision(18, 2);
            entity.Property(e => e.UnidadMedida).HasMaxLength(20);
            entity.Property(e => e.Categoria).HasMaxLength(100);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Variantes)
                .WithOne(v => v.Producto)
                .HasForeignKey(v => v.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.EmpresaId, e.Codigo }).IsUnique();
            entity.HasIndex(e => e.Industria);
        });

        modelBuilder.Entity<ProductoVariante>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Sku).HasMaxLength(80).IsRequired();
            entity.Property(e => e.Talla).HasMaxLength(20);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.PrecioOverride).HasPrecision(18, 2);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Producto)
                .WithMany(p => p.Variantes)
                .HasForeignKey(e => e.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.EmpresaId, e.Sku }).IsUnique();
            entity.HasIndex(e => new { e.ProductoId, e.Talla, e.Color });
        });
    }

    private void ConfigurarProductoCliente(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductoCliente>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TallaBaseCalzado).HasMaxLength(10);

            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.Productos)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Producto)
                .WithMany(p => p.Clientes)
                .HasForeignKey(e => e.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ClienteFraccionCalzado)
                .WithMany(f => f.ProductosClienteDefault)
                .HasForeignKey(e => e.ClienteFraccionCalzadoId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.ClienteId);
            entity.HasIndex(e => e.ProductoId);
            entity.HasIndex(e => e.ClienteFraccionCalzadoId);
            entity.HasIndex(e => new { e.ClienteId, e.ProductoId }).IsUnique();
        });
    }

    private void ConfigurarCalzado(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductoCalzado>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Modelo).HasMaxLength(100);
            entity.Property(e => e.Material).HasMaxLength(100);
            entity.Property(e => e.TipoSuela).HasMaxLength(100);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.Temporada).HasMaxLength(50);
            
            entity.HasOne(e => e.Producto)
                .WithMany()
                .HasForeignKey(e => e.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(e => e.ProductoId);
        });

        modelBuilder.Entity<CatalogoTallaCalzado>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Talla).HasMaxLength(10).IsRequired();

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.EmpresaId, e.SistemaNumeracion, e.Talla }).IsUnique();
            entity.HasIndex(e => new { e.EmpresaId, e.Orden });
        });

        modelBuilder.Entity<ClienteTallaCalzado>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Talla).HasMaxLength(10).IsRequired();
            entity.Property(e => e.PorcentajeVariacionDefault).HasPrecision(9, 4);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.TallasCalzadoConfiguradas)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CatalogoTallaCalzado)
                .WithMany(c => c.ClientesConfigurados)
                .HasForeignKey(e => e.CatalogoTallaCalzadoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.ClienteId, e.CatalogoTallaCalzadoId }).IsUnique();
            entity.HasIndex(e => new { e.ClienteId, e.Orden });
        });

        modelBuilder.Entity<ClienteFraccionCalzado>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Codigo).HasMaxLength(30).IsRequired();
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UnidadesPorFraccion).HasPrecision(18, 4);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.FraccionesCalzado)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.ClienteId, e.Codigo }).IsUnique();
            entity.HasIndex(e => new { e.ClienteId, e.Nombre });
        });

        modelBuilder.Entity<ClienteFraccionCalzadoDetalle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Talla).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Unidades).HasPrecision(18, 4);
            entity.Property(e => e.PorcentajeVariacion).HasPrecision(9, 4);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ClienteFraccionCalzado)
                .WithMany(f => f.Detalles)
                .HasForeignKey(e => e.ClienteFraccionCalzadoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ClienteTallaCalzado)
                .WithMany()
                .HasForeignKey(e => e.ClienteTallaCalzadoId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.CatalogoTallaCalzado)
                .WithMany(c => c.FraccionesDetalle)
                .HasForeignKey(e => e.CatalogoTallaCalzadoId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.ClienteFraccionCalzadoId);
            entity.HasIndex(e => new { e.ClienteFraccionCalzadoId, e.Talla }).IsUnique();
            entity.HasIndex(e => new { e.ClienteFraccionCalzadoId, e.Orden });
        });

        modelBuilder.Entity<TallaCalzado>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Talla).HasMaxLength(10).IsRequired();
            entity.Property(e => e.TallaUS).HasMaxLength(10);
            entity.Property(e => e.TallaEU).HasMaxLength(10);
            entity.Property(e => e.TallaUK).HasMaxLength(10);
            
            entity.HasOne(e => e.ProductoCalzado)
                .WithMany(p => p.Tallas)
                .HasForeignKey(e => e.ProductoCalzadoId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(e => e.ProductoCalzadoId);
        });

        modelBuilder.Entity<Horma>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Codigo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.Talla).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Medidas).HasMaxLength(100);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.EmpresaId, e.Codigo }).IsUnique();
            entity.HasIndex(e => e.Talla);
        });
    }

    private void ConfigurarInventario(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CategoriaInventario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Codigo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.EmpresaId, e.Codigo }).IsUnique();
            entity.HasIndex(e => new { e.EmpresaId, e.Nombre }).IsUnique();
            entity.HasIndex(e => e.Orden);
        });

        modelBuilder.Entity<TipoInventario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Codigo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CategoriaInventario)
                .WithMany(c => c.Tipos)
                .HasForeignKey(e => e.CategoriaInventarioId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.EmpresaId, e.CategoriaInventarioId, e.Codigo }).IsUnique();
            entity.HasIndex(e => new { e.EmpresaId, e.CategoriaInventarioId, e.Nombre }).IsUnique();
            entity.HasIndex(e => e.Orden);
        });

        modelBuilder.Entity<InventarioItem>(entity =>
        {
            entity.ToTable("logistica_inventario_item");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Codigo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.CodigoPantone).HasMaxLength(20);
            entity.Property(e => e.CodigoHex).HasMaxLength(7);
            entity.Property(e => e.PrecioUnitario).HasPrecision(18, 4);
            entity.Property(e => e.Cantidad).HasPrecision(18, 2);
            entity.Property(e => e.UnidadMedida).HasMaxLength(10);
            entity.Property(e => e.StockMinimo).HasPrecision(18, 2);
            entity.Property(e => e.OrigenLegacy).HasConversion<int>();

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CategoriaInventario)
                .WithMany()
                .HasForeignKey(e => e.CategoriaInventarioId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.TipoInventario)
                .WithMany()
                .HasForeignKey(e => e.TipoInventarioId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.EmpresaId, e.Codigo }).IsUnique();
            entity.HasIndex(e => e.CategoriaInventarioId);
            entity.HasIndex(e => e.TipoInventarioId);
            entity.HasIndex(e => e.MateriaPrimaOrigenId).IsUnique();
            entity.HasIndex(e => e.InsumoOrigenId).IsUnique();
        });

        modelBuilder.Entity<TipoProcesoConsumo>(entity =>
        {
            entity.HasOne(e => e.InventarioItem)
                .WithMany()
                .HasForeignKey(e => e.InventarioItemId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.InventarioItemId);
        });

        modelBuilder.Entity<MovimientoInventario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Cantidad).HasPrecision(18, 2);
            entity.Property(e => e.ExistenciaAnterior).HasPrecision(18, 2);
            entity.Property(e => e.ExistenciaNueva).HasPrecision(18, 2);
            entity.Property(e => e.CostoUnitario).HasPrecision(18, 4);
            entity.Property(e => e.Referencia).HasMaxLength(100);
            entity.Property(e => e.Observaciones).HasMaxLength(500);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.InventarioItem)
                .WithMany()
                .HasForeignKey(e => e.InventarioItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.EmpresaId);
            entity.HasIndex(e => e.Origen);
            entity.HasIndex(e => e.TipoMovimiento);
            entity.HasIndex(e => e.FechaMovimiento);
            entity.HasIndex(e => e.InventarioItemId);
        });

        modelBuilder.Entity<InventarioFinishedGood>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Sku).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Talla).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.CantidadDisponible).HasPrecision(18, 2);
            entity.Property(e => e.CantidadReservada).HasPrecision(18, 2);
            entity.Property(e => e.Ubicacion).HasMaxLength(100);
            entity.Property(e => e.Observaciones).HasMaxLength(500);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Cliente)
                .WithMany()
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Producto)
                .WithMany()
                .HasForeignKey(e => e.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ProductoVariante)
                .WithMany()
                .HasForeignKey(e => e.ProductoVarianteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.EmpresaId, e.ClienteId, e.ProductoVarianteId }).IsUnique();
            entity.HasIndex(e => new { e.EmpresaId, e.Sku });
            entity.HasIndex(e => e.ClienteId);
        });

        modelBuilder.Entity<MovimientoFinishedGood>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Cantidad).HasPrecision(18, 2);
            entity.Property(e => e.ExistenciaAnterior).HasPrecision(18, 2);
            entity.Property(e => e.ExistenciaNueva).HasPrecision(18, 2);
            entity.Property(e => e.Referencia).HasMaxLength(100);
            entity.Property(e => e.Observaciones).HasMaxLength(500);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.InventarioFinishedGood)
                .WithMany(i => i.Movimientos)
                .HasForeignKey(e => e.InventarioFinishedGoodId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Pedido)
                .WithMany()
                .HasForeignKey(e => e.PedidoId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.PedidoSerigrafia)
                .WithMany()
                .HasForeignKey(e => e.PedidoSerigrafiaId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.PedidoDetalleTalla)
                .WithMany()
                .HasForeignKey(e => e.PedidoDetalleTallaId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.NotaEntrega)
                .WithMany()
                .HasForeignKey(e => e.NotaEntregaId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.EmpresaId);
            entity.HasIndex(e => e.InventarioFinishedGoodId);
            entity.HasIndex(e => e.TipoMovimiento);
            entity.HasIndex(e => e.FechaMovimiento);
            entity.HasIndex(e => e.PedidoId);
            entity.HasIndex(e => e.NotaEntregaId);
        });
    }

    private void ConfigurarSerigrafia(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductoSerigrafia>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MaterialBase).HasMaxLength(100);
            entity.Property(e => e.TamanoImpresion).HasMaxLength(50);
            entity.Property(e => e.TipoTinta).HasMaxLength(50);
            
            entity.HasOne(e => e.Producto)
                .WithMany()
                .HasForeignKey(e => e.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(e => e.ProductoId);
        });

        modelBuilder.Entity<ColorSerigrafia>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NombreColor).HasMaxLength(100).IsRequired();
            entity.Property(e => e.CodigoPantone).HasMaxLength(20);
            entity.Property(e => e.CodigoHex).HasMaxLength(7);
            
            entity.HasOne(e => e.ProductoSerigrafia)
                .WithMany(p => p.Colores)
                .HasForeignKey(e => e.ProductoSerigrafiaId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(e => e.ProductoSerigrafiaId);
        });

        modelBuilder.Entity<Pantalla>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Codigo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.Dimensiones).HasMaxLength(50);
            entity.Property(e => e.DisenoPara).HasMaxLength(200);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.EmpresaId, e.Codigo }).IsUnique();
        });

        modelBuilder.Entity<Diseno>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Codigo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Nombre).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.RutaArchivo).HasMaxLength(500);
            entity.Property(e => e.Dimensiones).HasMaxLength(50);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Cliente)
                .WithMany()
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.EmpresaId, e.Codigo }).IsUnique();
            entity.HasIndex(e => e.ClienteId);
        });

        modelBuilder.Entity<TipoProceso>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.MinutosEstandar).HasPrecision(18, 2);
            entity.Property(e => e.MultiplicadorDefault).HasPrecision(18, 2);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Posicion)
                .WithMany()
                .HasForeignKey(e => e.PosicionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.Nombre);
            entity.HasIndex(e => e.Activo);
            entity.HasIndex(e => e.PosicionId);
        });

        modelBuilder.Entity<EscalaSerigrafia>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Ignore(e => e.Total);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EscalaSerigrafiaTalla>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Talla).HasMaxLength(10).IsRequired();

            entity.HasOne(e => e.EscalaSerigrafia)
                .WithMany(es => es.Tallas)
                .HasForeignKey(e => e.EscalaSerigrafiaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.EscalaSerigrafiaId);
        });

        modelBuilder.Entity<PedidoSerigrafiaProcesoDetalle>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.PedidoSerigrafia)
                .WithMany(p => p.TiposProceso)
                .HasForeignKey(e => e.PedidoSerigrafiaId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.TipoProceso)
                .WithMany(t => t.PedidosProcesos)
                .HasForeignKey(e => e.TipoProcesoId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasIndex(e => e.PedidoSerigrafiaId);
            entity.HasIndex(e => e.TipoProcesoId);
        });

        modelBuilder.Entity<PedidoSerigrafia>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Estilo).HasMaxLength(100).IsRequired();
            entity.Property(e => e.CombinacionColor).HasMaxLength(200);
            entity.Property(e => e.OrdenCompra).HasMaxLength(50);
            entity.Property(e => e.LoteCliente).HasMaxLength(50);
            entity.Property(e => e.Corrida).HasMaxLength(50);
            entity.Property(e => e.Factura).HasMaxLength(500);
            entity.Ignore(e => e.CantidadTotal);

            entity.HasOne(e => e.PedidoDetalle)
                .WithMany()
                .HasForeignKey(e => e.PedidoDetalleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.PedidoDetalleId);
            entity.HasIndex(e => e.Hecho);
            entity.HasIndex(e => e.FechaEstimada);
        });

        modelBuilder.Entity<PedidoSerigrafiaTalla>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Talla).HasMaxLength(10).IsRequired();

            entity.HasOne(e => e.PedidoSerigrafia)
                .WithMany(p => p.Tallas)
                .HasForeignKey(e => e.PedidoSerigrafiaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.PedidoSerigrafiaId);
        });

        modelBuilder.Entity<PedidoSerigrafiaTallaProceso>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FechaPaso).HasColumnType("datetime(6)");
            entity.Property(e => e.Completado).HasDefaultValue(false);

            entity.HasOne(e => e.PedidoSerigrafia)
                .WithMany()
                .HasForeignKey(e => e.PedidoSerigrafiaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.PedidoSerigrafiaTalla)
                .WithMany()
                .HasForeignKey(e => e.PedidoSerigrafiaTallaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.TipoProceso)
                .WithMany()
                .HasForeignKey(e => e.TipoProcesoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Empleado)
                .WithMany()
                .HasForeignKey(e => e.EmpleadoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.PedidoSerigrafiaId, e.PedidoSerigrafiaTallaId, e.TipoProcesoId }).IsUnique();
            entity.HasIndex(e => e.EmpleadoId);
        });

        modelBuilder.Entity<RegistroDestajoProceso>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Fecha).HasColumnType("datetime(6)");
            entity.Property(e => e.TarifaUnitario).HasPrecision(18, 4);
            entity.Property(e => e.Importe).HasPrecision(18, 2);
            entity.Property(e => e.Observaciones).HasMaxLength(500);

            entity.HasOne(e => e.PedidoSerigrafia)
                .WithMany()
                .HasForeignKey(e => e.PedidoSerigrafiaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.PedidoSerigrafiaTallaProceso)
                .WithMany()
                .HasForeignKey(e => e.PedidoSerigrafiaTallaProcesoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.PedidoSerigrafiaTalla)
                .WithMany()
                .HasForeignKey(e => e.PedidoSerigrafiaTallaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ValeDestajoDetalle)
                .WithMany(vd => vd.RegistrosOrigen)
                .HasForeignKey(e => e.ValeDestajoDetalleId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.TipoProceso)
                .WithMany(t => t.RegistrosDestajo)
                .HasForeignKey(e => e.TipoProcesoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Empleado)
                .WithMany()
                .HasForeignKey(e => e.EmpleadoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.PedidoSerigrafiaId);
            entity.HasIndex(e => e.PedidoSerigrafiaTallaProcesoId);
            entity.HasIndex(e => e.ValeDestajoDetalleId);
            entity.HasIndex(e => e.EmpleadoId);
            entity.HasIndex(e => e.Fecha);
        });

        modelBuilder.Entity<CotizacionSerigrafia>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Descripcion).HasMaxLength(500);

            entity.Property(e => e.CostoTotalPorPar).HasPrecision(18, 2);
            entity.Property(e => e.CostoTotalPorTarea).HasPrecision(18, 2);
            entity.Property(e => e.Utilidad).HasPrecision(18, 2);
            entity.Property(e => e.PrecioSugerido).HasPrecision(18, 2);
            entity.Property(e => e.PrecioFinalContado).HasPrecision(18, 2);
            entity.Property(e => e.PrecioCredito).HasPrecision(18, 2);
            entity.Property(e => e.Ganancia).HasPrecision(18, 2);

            entity.Ignore(e => e.TotalTintas);
            entity.Ignore(e => e.TotalInsumosBasicos);
            entity.Ignore(e => e.TotalInsumosDiversos);
            entity.Ignore(e => e.TotalManoObra);
            entity.Ignore(e => e.TotalGastosFijos);
            entity.Ignore(e => e.CostoTotal);

            entity.HasOne(e => e.Producto)
                .WithMany()
                .HasForeignKey(e => e.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Cliente)
                .WithMany()
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.ProductoId);
            entity.HasIndex(e => e.ClienteId);
        });

        modelBuilder.Entity<CotizacionDetalle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Concepto).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Consumo).HasPrecision(18, 4);
            entity.Property(e => e.PrecioUnitario).HasPrecision(18, 4);
            entity.Property(e => e.Costo).HasPrecision(18, 4);
            entity.Property(e => e.PesoInicial).HasPrecision(18, 4);
            entity.Property(e => e.PesoFinal).HasPrecision(18, 4);
            entity.Property(e => e.Tiempo).HasPrecision(18, 2);
            entity.Property(e => e.Precio).HasPrecision(18, 2);
            entity.Property(e => e.GananciaPorcentaje).HasPrecision(18, 4);

            entity.HasOne(e => e.CotizacionSerigrafia)
                .WithMany(c => c.Detalles)
                .HasForeignKey(e => e.CotizacionSerigrafiaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CotizacionSerigrafiaProceso)
                .WithMany()
                .HasForeignKey(e => e.CotizacionSerigrafiaProcesoId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.InventarioItem)
                .WithMany()
                .HasForeignKey(e => e.InventarioItemId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Posicion)
                .WithMany()
                .HasForeignKey(e => e.PosicionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.TipoProceso)
                .WithMany()
                .HasForeignKey(e => e.TipoProcesoId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.TipoProcesoConsumo)
                .WithMany()
                .HasForeignKey(e => e.TipoProcesoConsumoId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.GastoFijo)
                .WithMany()
                .HasForeignKey(e => e.GastoFijoId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.CotizacionSerigrafiaId);
            entity.HasIndex(e => e.Categoria);
            entity.HasIndex(e => e.CotizacionSerigrafiaProcesoId);
            entity.HasIndex(e => e.InventarioItemId);
            entity.HasIndex(e => e.TipoProcesoConsumoId);
        });

        modelBuilder.Entity<CotizacionSerigrafiaProceso>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Multiplicador).HasPrecision(18, 2);
            entity.Property(e => e.MinutosEstandarAplicados).HasPrecision(18, 2);
            entity.Property(e => e.TiempoTotal).HasPrecision(18, 2);

            entity.HasOne(e => e.CotizacionSerigrafia)
                .WithMany(c => c.Procesos)
                .HasForeignKey(e => e.CotizacionSerigrafiaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.TipoProceso)
                .WithMany()
                .HasForeignKey(e => e.TipoProcesoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.CotizacionSerigrafiaId);
            entity.HasIndex(e => e.TipoProcesoId);
        });

        modelBuilder.Entity<CotizacionVariantePrecio>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Etiqueta).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Talla).HasMaxLength(50);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.PorcentajeVariacionAplicado).HasPrecision(9, 4);
            entity.Property(e => e.PrecioContado).HasPrecision(18, 2);
            entity.Property(e => e.PrecioCredito).HasPrecision(18, 2);

            entity.HasOne(e => e.CotizacionSerigrafia)
                .WithMany(c => c.PreciosVariantes)
                .HasForeignKey(e => e.CotizacionSerigrafiaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ProductoVariante)
                .WithMany()
                .HasForeignKey(e => e.ProductoVarianteId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.CotizacionSerigrafiaId);
            entity.HasIndex(e => e.ProductoVarianteId);
        });
    }

    private void ConfigurarCatalogosProduccion(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Insumo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Codigo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PrecioUnitario).HasPrecision(18, 4);
            entity.Property(e => e.Cantidad).HasPrecision(18, 2);
            entity.Property(e => e.UnidadMedida).HasMaxLength(10);
            entity.Property(e => e.StockMinimo).HasPrecision(18, 2);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.TipoInventario)
                .WithMany()
                .HasForeignKey(e => e.TipoInventarioId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.EmpresaId, e.Codigo }).IsUnique();
            entity.HasIndex(e => e.TipoInsumo);
            entity.HasIndex(e => e.TipoInventarioId);
        });

        modelBuilder.Entity<Posicion>(entity =>
        {
            entity.ToTable("Posiciones");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(200).IsRequired();
            entity.Property(e => e.TarifaPorMinuto).HasPrecision(18, 4);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.Nombre);
        });

        modelBuilder.Entity<GastoFijo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Concepto).HasMaxLength(200).IsRequired();
            entity.Property(e => e.CostoMensual).HasPrecision(18, 2);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.Concepto);
        });
    }

    private void ConfigurarPresupuesto(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PresupuestoProducto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.SueldoSemanalBase).HasPrecision(18, 2);

            entity.HasOne(e => e.Producto)
                .WithMany()
                .HasForeignKey(e => e.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.ProductoId);

            entity.Ignore(e => e.TotalMateriaPrima);
            entity.Ignore(e => e.TotalInsumosBasicos);
            entity.Ignore(e => e.TotalInsumosDiversos);
            entity.Ignore(e => e.TotalManoObra);
            entity.Ignore(e => e.TotalGastosFijos);
            entity.Ignore(e => e.CostoTotal);
        });

        modelBuilder.Entity<PresupuestoDetalle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Concepto).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Consumo).HasPrecision(18, 4);
            entity.Property(e => e.Costo).HasPrecision(18, 4);
            entity.Property(e => e.SueldoSugerido).HasPrecision(18, 2);
            entity.Property(e => e.TiempoCompleto).HasPrecision(18, 2);
            entity.Property(e => e.TiempoBasico).HasPrecision(18, 2);

            entity.HasOne(e => e.PresupuestoProducto)
                .WithMany(p => p.Detalles)
                .HasForeignKey(e => e.PresupuestoProductoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.PresupuestoProductoId);
            entity.HasIndex(e => e.Categoria);
        });
    }

    private void ConfigurarCuentasPorPagar(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Codigo).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Nombre).HasMaxLength(200).IsRequired();
            entity.Property(e => e.RazonSocial).HasMaxLength(200);
            entity.Property(e => e.Rfc).HasMaxLength(20);
            entity.Property(e => e.RegimenFiscal).HasMaxLength(10);
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.EmailFiscal).HasMaxLength(100);
            entity.Property(e => e.EmailOrdenesCompra).HasMaxLength(100);
            entity.Property(e => e.Direccion).HasMaxLength(250);
            entity.Property(e => e.Ciudad).HasMaxLength(100);
            entity.Property(e => e.Estado).HasMaxLength(100);
            entity.Property(e => e.CodigoPostal).HasMaxLength(10);
            entity.Property(e => e.Pais).HasMaxLength(50);
            entity.Property(e => e.Contacto).HasMaxLength(200);
            entity.Property(e => e.Banco).HasMaxLength(100);
            entity.Property(e => e.CuentaBancaria).HasMaxLength(50);
            entity.Property(e => e.Clabe).HasMaxLength(25);
            entity.Property(e => e.TitularCuenta).HasMaxLength(200);
            entity.Property(e => e.Notas).HasMaxLength(500);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.EmpresaId, e.Codigo }).IsUnique();
        });

        modelBuilder.Entity<CuentaPorPagar>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NumeroDocumento).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Concepto).HasMaxLength(500);
            entity.Property(e => e.Subtotal).HasPrecision(18, 2);
            entity.Property(e => e.Impuestos).HasPrecision(18, 2);
            entity.Property(e => e.Total).HasPrecision(18, 2);
            entity.Property(e => e.Notas).HasMaxLength(500);
            entity.Ignore(e => e.TotalPagado);
            entity.Ignore(e => e.Saldo);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Proveedor)
                .WithMany(p => p.CuentasPorPagar)
                .HasForeignKey(e => e.ProveedorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.ProveedorId);
            entity.HasIndex(e => e.Estatus);
            entity.HasIndex(e => e.FechaEmision);
        });

        modelBuilder.Entity<PagoCxP>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Monto).HasPrecision(18, 2);
            entity.Property(e => e.Referencia).HasMaxLength(100);
            entity.Property(e => e.Notas).HasMaxLength(500);

            entity.HasOne(e => e.CuentaPorPagar)
                .WithMany(c => c.Pagos)
                .HasForeignKey(e => e.CuentaPorPagarId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.CuentaPorPagarId);
            entity.HasIndex(e => e.FechaPago);
        });
    }

    private void ConfigurarEmpleados(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Posicion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(120).IsRequired();
            entity.Property(e => e.TarifaPorMinuto).HasPrecision(18, 4);
            entity.Property(e => e.MontoBonoDistribuido).HasPrecision(18, 2).HasDefaultValue(0m);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.BonoEstructuraRrhh)
                .WithMany()
                .HasForeignKey(e => e.BonoEstructuraRrhhId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.EmpresaId, e.Nombre }).IsUnique();
            entity.HasIndex(e => e.BonoEstructuraRrhhId);
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Codigo).HasMaxLength(20).IsRequired();
            entity.Property(e => e.NumeroEmpleado).HasMaxLength(30).IsRequired();
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ApellidoPaterno).HasMaxLength(100);
            entity.Property(e => e.ApellidoMaterno).HasMaxLength(100);
            entity.Property(e => e.Curp).HasMaxLength(18);
            entity.Property(e => e.Nss).HasMaxLength(15);
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Direccion).HasMaxLength(250);
            entity.Property(e => e.CodigoChecador).HasMaxLength(100);
            entity.Property(e => e.Puesto).HasMaxLength(100);
            entity.Property(e => e.Departamento).HasMaxLength(100);
            entity.Property(e => e.NumeroCreditoInfonavit).HasMaxLength(50);
            entity.Property(e => e.SueldoSemanal).HasPrecision(18, 2);
            entity.Property(e => e.FactorDescuentoInfonavit).HasPrecision(18, 4);
            entity.Property(e => e.Notas).HasMaxLength(500);
            entity.Ignore(e => e.NombreCompleto);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Posicion)
                .WithMany()
                .HasForeignKey(e => e.PosicionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.TurnoBase)
                .WithMany(t => t.Empleados)
                .HasForeignKey(e => e.TurnoBaseId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.EmpresaId, e.Codigo }).IsUnique();
            entity.HasIndex(e => new { e.EmpresaId, e.NumeroEmpleado }).IsUnique();
            entity.HasIndex(e => new { e.EmpresaId, e.CodigoChecador }).IsUnique();
            entity.HasIndex(e => e.PosicionId);
            entity.HasIndex(e => e.TurnoBaseId);
            entity.HasIndex(e => e.Departamento);
            entity.HasIndex(e => e.AplicaImss);
        });
    }

    private void ConfigurarPrenominas(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Prenomina>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Folio).HasMaxLength(30);
            entity.Property(e => e.PeriodicidadPago).HasConversion<int>();
            entity.Property(e => e.Periodo).HasMaxLength(80).IsRequired();
            entity.Property(e => e.Estatus).HasConversion<int>();
            entity.Property(e => e.Notas).HasMaxLength(500);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.EmpresaId, e.Folio }).IsUnique();
            entity.HasIndex(e => new { e.EmpresaId, e.FechaInicio, e.FechaFin }).IsUnique();
            entity.HasIndex(e => new { e.EmpresaId, e.PeriodicidadPago, e.AnioPeriodo, e.NumeroPeriodo }).IsUnique();
            entity.HasIndex(e => e.Estatus);
        });

        modelBuilder.Entity<PrenominaDetalle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.HorasTrabajadasNetas).HasPrecision(18, 2);
            entity.Property(e => e.HorasExtraBase).HasPrecision(18, 2);
            entity.Property(e => e.HorasExtra).HasPrecision(18, 2);
            entity.Property(e => e.HorasBancoAcumuladas).HasPrecision(18, 2);
            entity.Property(e => e.HorasBancoConsumidas).HasPrecision(18, 2);
            entity.Property(e => e.HorasDescansoTomado).HasPrecision(18, 2);
            entity.Property(e => e.HorasDescansoPagado).HasPrecision(18, 2);
            entity.Property(e => e.HorasDescansoNoPagado).HasPrecision(18, 2);
            entity.Property(e => e.FactorPagoTiempoExtra).HasPrecision(18, 4);
            entity.Property(e => e.MontoDestajoInformativo).HasPrecision(18, 2);
            entity.Property(e => e.DiasVacacionesDisponibles).HasPrecision(18, 2);
            entity.Property(e => e.DiasVacacionesRestantes).HasPrecision(18, 2);
            entity.Property(e => e.ComplementoSalarioMinimoSugerido).HasPrecision(18, 2);
            entity.Property(e => e.Notas).HasMaxLength(500);
            entity.Property(e => e.DiasDomingoTrabajado).HasDefaultValue(0);

            entity.HasOne(e => e.Prenomina)
                .WithMany(p => p.Detalles)
                .HasForeignKey(e => e.PrenominaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Empleado)
                .WithMany(emp => emp.PrenominaDetalles)
                .HasForeignKey(e => e.EmpleadoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.PrenominaId, e.EmpleadoId }).IsUnique();
            entity.HasIndex(e => e.EmpleadoId);
        });

        modelBuilder.Entity<PrenominaBono>(entity =>
        {
            entity.ToTable("rrhh_prenomina_bono");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Importe).HasPrecision(18, 2);
            entity.Property(e => e.Observaciones).HasMaxLength(300);

            entity.HasOne(e => e.PrenominaDetalle)
                .WithMany(d => d.BonosRapidos)
                .HasForeignKey(e => e.PrenominaDetalleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.BonoRubroRrhh)
                .WithMany()
                .HasForeignKey(e => e.BonoRubroRrhhId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.PrenominaDetalleId);
            entity.HasIndex(e => new { e.PrenominaDetalleId, e.BonoRubroRrhhId }).IsUnique();
        });

        modelBuilder.Entity<PrenominaPercepcion>(entity =>
        {
            entity.ToTable("rrhh_prenomina_percepcion");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Importe).HasPrecision(18, 2);
            entity.Property(e => e.Referencia).HasMaxLength(120);
            entity.Property(e => e.Observaciones).HasMaxLength(300);

            entity.HasOne(e => e.PrenominaDetalle)
                .WithMany(d => d.PercepcionesRapidas)
                .HasForeignKey(e => e.PrenominaDetalleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.TipoPercepcion)
                .WithMany()
                .HasForeignKey(e => e.TipoPercepcionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.PrenominaDetalleId);
            entity.HasIndex(e => new { e.PrenominaDetalleId, e.TipoPercepcionId }).IsUnique();
        });
    }

    private void ConfigurarNominas(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Nomina>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Folio).HasMaxLength(30);
            entity.Property(e => e.NumeroNomina).HasMaxLength(20);
            entity.Property(e => e.PeriodicidadPago).HasConversion<int>();
            entity.Property(e => e.Periodo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Notas).HasMaxLength(500);
            entity.Ignore(e => e.TotalNomina);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Prenomina)
                .WithOne()
                .HasForeignKey<Nomina>(e => e.PrenominaId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.EmpresaId, e.Folio }).IsUnique();
            entity.HasIndex(e => new { e.EmpresaId, e.Periodo, e.NumeroNomina }).IsUnique();
            entity.HasIndex(e => new { e.EmpresaId, e.PeriodicidadPago, e.AnioPeriodo, e.NumeroPeriodo }).IsUnique();
            entity.HasIndex(e => e.Estatus);
            entity.HasIndex(e => e.FechaInicio);
        });

        modelBuilder.Entity<NominaDetalle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SueldoBase).HasPrecision(18, 2);
            entity.Property(e => e.TarifaPorPieza).HasPrecision(18, 4);
            entity.Property(e => e.MontoPrimaVacacional).HasPrecision(18, 2);
            entity.Property(e => e.ComplementoSalarioMinimo).HasPrecision(18, 2);
            entity.Property(e => e.CuotaImssObrera).HasPrecision(18, 2);
            entity.Property(e => e.CuotaImssPatronal).HasPrecision(18, 2);
            entity.Property(e => e.MontoFestivoTrabajado).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(e => e.MontoPrimaDominical).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(e => e.MontoInfonavit).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(e => e.HorasTrabajadasNetas).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(e => e.HorasExtraBase).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(e => e.HorasExtraDobles).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(e => e.HorasExtraTriples).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(e => e.HorasExtraBanco).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(e => e.HorasExtra).HasPrecision(18, 2);
            entity.Property(e => e.MontoHorasExtra).HasPrecision(18, 2);
            entity.Property(e => e.Bonos).HasPrecision(18, 2);
            entity.Property(e => e.Deducciones).HasPrecision(18, 2);
            entity.Property(e => e.ConceptoDeducciones).HasMaxLength(500);
            entity.Property(e => e.Notas).HasMaxLength(500);
            entity.Ignore(e => e.MontoDestajoLegacy);
            entity.Ignore(e => e.TotalPagar);
            entity.Property(e => e.TotalPiezas).HasDefaultValue(0);
            entity.Property(e => e.MontoDestajo).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(e => e.MontoBono).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(e => e.DiasTrabajados).HasDefaultValue(0);
            entity.Property(e => e.DiasPagados).HasDefaultValue(0);
            entity.Property(e => e.DiasVacaciones).HasDefaultValue(0);
            entity.Property(e => e.DiasFaltaJustificada).HasDefaultValue(0);
            entity.Property(e => e.DiasFaltaInjustificada).HasDefaultValue(0);
            entity.Property(e => e.DiasIncapacidad).HasDefaultValue(0);
            entity.Property(e => e.DiasDescansoTrabajado).HasDefaultValue(0);
            entity.Property(e => e.DiasDomingoTrabajado).HasDefaultValue(0);
            entity.Property(e => e.DiasFestivoTrabajado).HasDefaultValue(0);

            entity.HasOne(e => e.EsquemaPago)
                .WithMany()
                .HasForeignKey(e => e.EsquemaPagoId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Nomina)
                .WithMany(n => n.Detalles)
                .HasForeignKey(e => e.NominaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Empleado)
                .WithMany(e => e.NominaDetalles)
                .HasForeignKey(e => e.EmpleadoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.NominaId);
            entity.HasIndex(e => e.EmpleadoId);
        });

        modelBuilder.Entity<NominaConceptoConfigRrhh>(entity =>
        {
            entity.ToTable("rrhh_nomina_concepto_config");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Clave).HasMaxLength(40).IsRequired();
            entity.Property(e => e.Nombre).HasMaxLength(120).IsRequired();
            entity.Property(e => e.Naturaleza).HasConversion<int>();
            entity.Property(e => e.Destino).HasConversion<int>();
            entity.Property(e => e.TipoCalculo).HasConversion<int>();
            entity.Property(e => e.MontoFijoDefault).HasPrecision(18, 2);
            entity.Property(e => e.PorcentajeDefault).HasPrecision(9, 4);
            entity.Property(e => e.CantidadDefault).HasPrecision(18, 4);
            entity.Property(e => e.TarifaDefault).HasPrecision(18, 4);
            entity.Property(e => e.Observaciones).HasMaxLength(500);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.EmpresaId, e.Clave }).IsUnique();
            entity.HasIndex(e => new { e.EmpresaId, e.Nombre });
            entity.HasIndex(e => new { e.EmpresaId, e.Naturaleza, e.Destino });
        });

        modelBuilder.Entity<EmpleadoConceptoRrhh>(entity =>
        {
            entity.ToTable("rrhh_empleado_concepto");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Monto).HasPrecision(18, 2);
            entity.Property(e => e.Porcentaje).HasPrecision(9, 4);
            entity.Property(e => e.Cantidad).HasPrecision(18, 4);
            entity.Property(e => e.Tarifa).HasPrecision(18, 4);
            entity.Property(e => e.Saldo).HasPrecision(18, 2);
            entity.Property(e => e.Limite).HasPrecision(18, 2);
            entity.Property(e => e.FechaInicio).HasColumnType("date");
            entity.Property(e => e.FechaFin).HasColumnType("date");
            entity.Property(e => e.Observaciones).HasMaxLength(500);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Empleado)
                .WithMany(e => e.ConceptosNomina)
                .HasForeignKey(e => e.EmpleadoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ConceptoConfig)
                .WithMany(c => c.EmpleadosConceptos)
                .HasForeignKey(e => e.ConceptoConfigId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.EmpresaId, e.EmpleadoId, e.ConceptoConfigId, e.FechaInicio });
            entity.HasIndex(e => new { e.EmpresaId, e.EmpleadoId, e.IsActive });
        });

        modelBuilder.Entity<NominaProvisionDetalleRrhh>(entity =>
        {
            entity.ToTable("rrhh_nomina_provision_detalle");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Importe).HasPrecision(18, 2);
            entity.Property(e => e.BaseCalculo).HasPrecision(18, 2);
            entity.Property(e => e.Cantidad).HasPrecision(18, 4);
            entity.Property(e => e.Tarifa).HasPrecision(18, 4);
            entity.Property(e => e.PeriodoInicio).HasColumnType("date");
            entity.Property(e => e.PeriodoFin).HasColumnType("date");
            entity.Property(e => e.Observaciones).HasMaxLength(500);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.NominaDetalle)
                .WithMany(n => n.ProvisionesDetalle)
                .HasForeignKey(e => e.NominaDetalleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Empleado)
                .WithMany()
                .HasForeignKey(e => e.EmpleadoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ConceptoConfig)
                .WithMany(c => c.ProvisionesDetalle)
                .HasForeignKey(e => e.ConceptoConfigId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.EmpresaId, e.NominaDetalleId, e.ConceptoConfigId });
            entity.HasIndex(e => new { e.EmpresaId, e.EmpleadoId, e.PeriodoInicio, e.PeriodoFin });
        });

        modelBuilder.Entity<BonoRubroRrhh>(entity =>
        {
            entity.ToTable("rrhh_bono_rubro");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Clave).HasMaxLength(40).IsRequired();
            entity.Property(e => e.Nombre).HasMaxLength(120).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(300);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.EmpresaId, e.Clave }).IsUnique();
            entity.HasIndex(e => new { e.EmpresaId, e.Nombre }).IsUnique();
        });

        modelBuilder.Entity<BonoEstructuraRrhh>(entity =>
        {
            entity.ToTable("rrhh_bono_estructura");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(120).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(300);
            entity.Property(e => e.TipoCaptura).HasConversion<int>();

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.EmpresaId, e.Nombre }).IsUnique();
        });

        modelBuilder.Entity<BonoEstructuraDetalleRrhh>(entity =>
        {
            entity.ToTable("rrhh_bono_estructura_detalle");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Porcentaje).HasPrecision(9, 4);

            entity.HasOne(e => e.BonoEstructuraRrhh)
                .WithMany(e => e.Detalles)
                .HasForeignKey(e => e.BonoEstructuraRrhhId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.BonoRubroRrhh)
                .WithMany(r => r.EstructurasDetalle)
                .HasForeignKey(e => e.BonoRubroRrhhId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.BonoEstructuraRrhhId);
            entity.HasIndex(e => new { e.BonoEstructuraRrhhId, e.BonoRubroRrhhId }).IsUnique();
        });

        modelBuilder.Entity<BonoDistribucionPeriodoRrhh>(entity =>
        {
            entity.ToTable("rrhh_bono_distribucion_periodo");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Periodo).HasMaxLength(80).IsRequired();
            entity.Property(e => e.Departamento).HasMaxLength(100);
            entity.Property(e => e.MontoTotalDistribuir).HasPrecision(18, 2);
            entity.Property(e => e.Observaciones).HasMaxLength(300);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Posicion)
                .WithMany()
                .HasForeignKey(e => e.PosicionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.BonoEstructuraRrhh)
                .WithMany()
                .HasForeignKey(e => e.BonoEstructuraRrhhId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.EmpresaId, e.FechaInicio, e.FechaFin, e.PosicionId, e.Departamento }).IsUnique();
            entity.HasIndex(e => e.BonoEstructuraRrhhId);
        });

        modelBuilder.Entity<BonoDistribucionEmpleadoRrhh>(entity =>
        {
            entity.ToTable("rrhh_bono_distribucion_empleado");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Porcentaje).HasPrecision(9, 4);
            entity.Property(e => e.MontoAsignado).HasPrecision(18, 2);
            entity.Property(e => e.Observaciones).HasMaxLength(300);

            entity.HasOne(e => e.BonoDistribucionPeriodoRrhh)
                .WithMany(e => e.Detalles)
                .HasForeignKey(e => e.BonoDistribucionPeriodoRrhhId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Empleado)
                .WithMany()
                .HasForeignKey(e => e.EmpleadoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.EmpleadoId);
            entity.HasIndex(e => new { e.BonoDistribucionPeriodoRrhhId, e.EmpleadoId }).IsUnique();
        });

        modelBuilder.Entity<BonoDistribucionEmpleadoDetalleRrhh>(entity =>
        {
            entity.ToTable("rrhh_bono_distribucion_empleado_detalle");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Porcentaje).HasPrecision(9, 4);
            entity.Property(e => e.MontoAsignado).HasPrecision(18, 2);

            entity.HasOne(e => e.BonoDistribucionEmpleadoRrhh)
                .WithMany(e => e.Detalles)
                .HasForeignKey(e => e.BonoDistribucionEmpleadoRrhhId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.BonoRubroRrhh)
                .WithMany()
                .HasForeignKey(e => e.BonoRubroRrhhId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.BonoDistribucionEmpleadoRrhhId);
            entity.HasIndex(e => e.BonoRubroRrhhId);
            entity.HasIndex(e => new { e.BonoDistribucionEmpleadoRrhhId, e.BonoRubroRrhhId }).IsUnique();
        });

        modelBuilder.Entity<NominaBono>(entity =>
        {
            entity.ToTable("rrhh_nomina_bono");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TipoCaptura).HasConversion<int>();
            entity.Property(e => e.MontoTotal).HasPrecision(18, 2);
            entity.Property(e => e.Observaciones).HasMaxLength(500);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.NominaDetalle)
                .WithMany(d => d.BonosEstructurados)
                .HasForeignKey(e => e.NominaDetalleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.NominaDetalleId);
            entity.HasIndex(e => new { e.EmpresaId, e.NominaDetalleId }).IsUnique();
        });

        modelBuilder.Entity<NominaBonoDetalle>(entity =>
        {
            entity.ToTable("rrhh_nomina_bono_detalle");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Porcentaje).HasPrecision(9, 4);
            entity.Property(e => e.Importe).HasPrecision(18, 2);

            entity.HasOne(e => e.NominaBono)
                .WithMany(b => b.Detalles)
                .HasForeignKey(e => e.NominaBonoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.BonoRubroRrhh)
                .WithMany(r => r.BonosDetalle)
                .HasForeignKey(e => e.BonoRubroRrhhId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.NominaBonoId);
            entity.HasIndex(e => new { e.NominaBonoId, e.BonoRubroRrhhId }).IsUnique();
        });

        modelBuilder.Entity<NominaPercepcionTipo>(entity =>
        {
            entity.ToTable("rrhh_nomina_percepcion_tipo");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Clave).HasMaxLength(40).IsRequired();
            entity.Property(e => e.Nombre).HasMaxLength(120).IsRequired();
            entity.Property(e => e.Categoria).HasConversion<int>();

            entity.HasIndex(e => e.Clave).IsUnique();
            entity.HasIndex(e => e.Nombre).IsUnique();
        });

        modelBuilder.Entity<NominaPercepcion>(entity =>
        {
            entity.ToTable("rrhh_nomina_percepcion");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Descripcion).HasMaxLength(250);
            entity.Property(e => e.Importe).HasPrecision(18, 2);
            entity.Property(e => e.Origen).HasConversion<int>();
            entity.Property(e => e.Referencia).HasMaxLength(120);
            entity.Property(e => e.Observaciones).HasMaxLength(500);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.NominaDetalle)
                .WithMany(d => d.PercepcionesManuales)
                .HasForeignKey(e => e.NominaDetalleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.TipoPercepcion)
                .WithMany(t => t.Percepciones)
                .HasForeignKey(e => e.TipoPercepcionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.NominaDetalleId);
            entity.HasIndex(e => e.TipoPercepcionId);
            entity.HasIndex(e => new { e.EmpresaId, e.NominaDetalleId });
        });

        modelBuilder.Entity<DeduccionTipoRrhh>(entity =>
        {
            entity.ToTable("rrhh_deduccion_tipo");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Clave).HasMaxLength(40).IsRequired();
            entity.Property(e => e.Nombre).HasMaxLength(120).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(300);

            entity.HasIndex(e => e.Clave).IsUnique();
            entity.HasIndex(e => e.Nombre).IsUnique();
        });

        modelBuilder.Entity<NominaDeduccion>(entity =>
        {
            entity.ToTable("rrhh_nomina_deduccion");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Descripcion).HasMaxLength(250);
            entity.Property(e => e.Importe).HasPrecision(18, 2);
            entity.Property(e => e.Observaciones).HasMaxLength(500);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.NominaDetalle)
                .WithMany(d => d.DeduccionesEstructuradas)
                .HasForeignKey(e => e.NominaDetalleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.TipoDeduccion)
                .WithMany(t => t.Deducciones)
                .HasForeignKey(e => e.TipoDeduccionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.NominaDetalleId);
            entity.HasIndex(e => e.TipoDeduccionId);
            entity.HasIndex(e => new { e.EmpresaId, e.NominaDetalleId });
        });

        modelBuilder.Entity<RrhhBancoHorasMovimiento>(entity =>
        {
            entity.ToTable("rrhh_banco_horas_movimiento");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Fecha).HasColumnType("date");
            entity.Property(e => e.TipoMovimiento).HasConversion<int>();
            entity.Property(e => e.Horas).HasPrecision(18, 2);
            entity.Property(e => e.ReferenciaTipo).HasMaxLength(80);
            entity.Property(e => e.Observaciones).HasMaxLength(500);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Empleado)
                .WithMany(e => e.BancoHorasMovimientos)
                .HasForeignKey(e => e.EmpleadoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.NominaDetalle)
                .WithMany()
                .HasForeignKey(e => e.NominaDetalleId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.EmpleadoId);
            entity.HasIndex(e => new { e.EmpresaId, e.EmpleadoId, e.Fecha });
            entity.HasIndex(e => new { e.EmpresaId, e.NominaDetalleId, e.TipoMovimiento });
        });
    }

    private void ConfigurarEsquemasPago(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EsquemaPago>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.Tipo).HasConversion<int>();
            entity.Property(e => e.SueldoBaseSugerido).HasPrecision(18, 2);
            entity.Property(e => e.BonoCumplimientoMonto).HasPrecision(18, 2);
            entity.Property(e => e.BonoAdelantoMonto).HasPrecision(18, 2);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.EmpresaId, e.Nombre }).IsUnique();
            entity.HasIndex(e => e.Tipo);
        });

        modelBuilder.Entity<EsquemaPagoTarifa>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Tarifa).HasPrecision(18, 4);
            entity.Property(e => e.Descripcion).HasMaxLength(200);

            entity.HasOne(e => e.EsquemaPago)
                .WithMany(ep => ep.Tarifas)
                .HasForeignKey(e => e.EsquemaPagoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.TipoProceso)
                .WithMany()
                .HasForeignKey(e => e.TipoProcesoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Posicion)
                .WithMany()
                .HasForeignKey(e => e.PosicionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.EsquemaPagoId);
            entity.HasIndex(e => new { e.EsquemaPagoId, e.TipoProcesoId, e.PosicionId });
        });

        modelBuilder.Entity<EmpleadoEsquemaPago>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SueldoBaseOverride).HasPrecision(18, 2);

            entity.HasOne(e => e.Empleado)
                .WithMany(emp => emp.EsquemasPago)
                .HasForeignKey(e => e.EmpleadoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.EsquemaPago)
                .WithMany(ep => ep.Empleados)
                .HasForeignKey(e => e.EsquemaPagoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.EmpleadoId);
            entity.HasIndex(e => e.EsquemaPagoId);
            entity.HasIndex(e => new { e.EmpleadoId, e.VigenteDesde });
        });

        modelBuilder.Entity<ValeDestajo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Folio).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Fecha).HasColumnType("datetime(6)");
            entity.Property(e => e.Estatus).HasConversion<int>();
            entity.Property(e => e.Observaciones).HasMaxLength(500);
            entity.Ignore(e => e.TotalPiezas);
            entity.Ignore(e => e.TotalImporte);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Empleado)
                .WithMany(emp => emp.ValesDestajo)
                .HasForeignKey(e => e.EmpleadoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.EsquemaPago)
                .WithMany()
                .HasForeignKey(e => e.EsquemaPagoId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.NominaDetalle)
                .WithMany(nd => nd.ValesDestajo)
                .HasForeignKey(e => e.NominaDetalleId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.EmpresaId, e.Folio }).IsUnique();
            entity.HasIndex(e => e.EmpleadoId);
            entity.HasIndex(e => e.Fecha);
            entity.HasIndex(e => e.Estatus);
            entity.HasIndex(e => e.NominaDetalleId);
        });

        modelBuilder.Entity<ValeDestajoDetalle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TarifaAplicada).HasPrecision(18, 4);
            entity.Property(e => e.Importe).HasPrecision(18, 2);
            entity.Property(e => e.Observaciones).HasMaxLength(500);

            entity.HasOne(e => e.ValeDestajo)
                .WithMany(v => v.Detalles)
                .HasForeignKey(e => e.ValeDestajoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.TipoProceso)
                .WithMany()
                .HasForeignKey(e => e.TipoProcesoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Pedido)
                .WithMany()
                .HasForeignKey(e => e.PedidoId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.EsquemaPagoTarifa)
                .WithMany()
                .HasForeignKey(e => e.EsquemaPagoTarifaId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.ValeDestajoId);
            entity.HasIndex(e => e.TipoProcesoId);
            entity.HasIndex(e => e.PedidoId);
        });
    }

    private void ConfigurarAuth(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ModuloAcceso>(entity =>
        {
            entity.ToTable("auth_modulo");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Clave).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(200);
            entity.HasIndex(e => e.Clave).IsUnique();
            entity.HasIndex(e => new { e.Orden, e.Nombre });
        });

        modelBuilder.Entity<EmpresaModuloAcceso>(entity =>
        {
            entity.ToTable("auth_empresa_modulo");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VigenteDesde).HasColumnType("date");
            entity.Property(e => e.VigenteHasta).HasColumnType("date");
            entity.Property(e => e.Origen).HasConversion<int>();

            entity.HasOne(e => e.Empresa)
                .WithMany(e => e.ModulosAcceso)
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ModuloAcceso)
                .WithMany(m => m.Empresas)
                .HasForeignKey(e => e.ModuloAccesoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.EmpresaId, e.ModuloAccesoId }).IsUnique();
            entity.HasIndex(e => new { e.EmpresaId, e.Habilitado, e.VigenteDesde, e.VigenteHasta });
        });

        modelBuilder.Entity<TipoUsuario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(200);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.EmpresaId, e.Nombre }).IsUnique();
        });

        modelBuilder.Entity<Capacidad>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Clave).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Modulo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(200);

            entity.HasOne(e => e.ModuloAcceso)
                .WithMany(m => m.Capacidades)
                .HasForeignKey(e => e.ModuloAccesoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.Clave).IsUnique();
            entity.HasIndex(e => new { e.ModuloAccesoId, e.Nombre });
        });

        modelBuilder.Entity<TipoUsuarioCapacidad>(entity =>
        {
            entity.HasKey(e => new { e.TipoUsuarioId, e.CapacidadId });

            entity.HasOne(e => e.TipoUsuario)
                .WithMany(t => t.Capacidades)
                .HasForeignKey(e => e.TipoUsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Capacidad)
                .WithMany(c => c.TiposUsuario)
                .HasForeignKey(e => e.CapacidadId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(e => _empresaId == Guid.Empty || e.TipoUsuario.EmpresaId == _empresaId);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.Property(e => e.NombreCompleto).HasMaxLength(200).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(e => e.RequiereCambioPassword).HasDefaultValue(false);
            entity.Property(e => e.IntentosFallidos).HasDefaultValue(0);
            entity.Property(e => e.PasswordResetTokenHash).HasMaxLength(200);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.TipoUsuario)
                .WithMany(t => t.Usuarios)
                .HasForeignKey(e => e.TipoUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.BloqueadoHastaUtc);
        });
    }

    private void ConfigurarQueryFilters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Empresa>().HasQueryFilter(e => _empresaId == Guid.Empty || e.Id == _empresaId);
        modelBuilder.Entity<AuditLog>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == null || e.EmpresaId == _empresaId);
        modelBuilder.Entity<AppConfig>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<SuscripcionEmpresa>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<PagoSuscripcion>().HasQueryFilter(e => _empresaId == Guid.Empty || e.SuscripcionEmpresa.EmpresaId == _empresaId);
        modelBuilder.Entity<Cliente>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<ClienteReglaVariacionPrecio>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<Contacto>().HasQueryFilter(e => _empresaId == Guid.Empty || e.Cliente.EmpresaId == _empresaId);
        modelBuilder.Entity<Pedido>().HasQueryFilter(e => _empresaId == Guid.Empty || e.Cliente.EmpresaId == _empresaId);
        modelBuilder.Entity<PedidoDetalle>().HasQueryFilter(e => _empresaId == Guid.Empty || e.Pedido.Cliente.EmpresaId == _empresaId);
        modelBuilder.Entity<PedidoDetalleTalla>().HasQueryFilter(e => _empresaId == Guid.Empty || e.PedidoDetalle.Pedido.Cliente.EmpresaId == _empresaId);
        modelBuilder.Entity<PedidoConcepto>().HasQueryFilter(e => _empresaId == Guid.Empty || e.Pedido.Cliente.EmpresaId == _empresaId);
        modelBuilder.Entity<PedidoSeguimiento>().HasQueryFilter(e => _empresaId == Guid.Empty || e.Pedido.Cliente.EmpresaId == _empresaId);
        modelBuilder.Entity<PagoPedido>().HasQueryFilter(e => _empresaId == Guid.Empty || e.Pedido.Cliente.EmpresaId == _empresaId);
        modelBuilder.Entity<NotaEntrega>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<NotaEntregaDetalle>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<NotaEntregaDetalleTalla>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<Factura>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<FacturaNotaEntrega>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<FacturaDetalle>().HasQueryFilter(e => _empresaId == Guid.Empty || e.Factura.EmpresaId == _empresaId);
        modelBuilder.Entity<FacturaImpuesto>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<FacturaRelacionada>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<FacturaEvento>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<PagoRecibido>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<PagoAplicacionDocumento>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<ComplementoPago>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<ComplementoPagoDocumentoRelacionado>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<ClienteDatoFiscalSnapshot>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<Producto>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<ProductoVariante>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<ProductoCliente>().HasQueryFilter(e => _empresaId == Guid.Empty || e.Cliente.EmpresaId == _empresaId);
        modelBuilder.Entity<ProductoCalzado>().HasQueryFilter(e => _empresaId == Guid.Empty || e.Producto.EmpresaId == _empresaId);
        modelBuilder.Entity<TallaCalzado>().HasQueryFilter(e => _empresaId == Guid.Empty || e.ProductoCalzado.Producto.EmpresaId == _empresaId);
        modelBuilder.Entity<CatalogoTallaCalzado>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<ClienteTallaCalzado>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<ClienteFraccionCalzado>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<ClienteFraccionCalzadoDetalle>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<ProductoSerigrafia>().HasQueryFilter(e => _empresaId == Guid.Empty || e.Producto.EmpresaId == _empresaId);
        modelBuilder.Entity<ColorSerigrafia>().HasQueryFilter(e => _empresaId == Guid.Empty || e.ProductoSerigrafia.Producto.EmpresaId == _empresaId);
        modelBuilder.Entity<CategoriaInventario>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<TipoInventario>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<InventarioItem>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<MovimientoInventario>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<InventarioFinishedGood>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<MovimientoFinishedGood>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<EmpresaStorageConfiguracion>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<Usuario>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<TipoUsuario>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<EmpresaModuloAcceso>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<TipoProceso>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<Posicion>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<GastoFijo>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<Pantalla>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<Diseno>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<EscalaSerigrafia>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<EscalaSerigrafiaTalla>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EscalaSerigrafia.EmpresaId == _empresaId);
        modelBuilder.Entity<CotizacionSerigrafia>().HasQueryFilter(e => _empresaId == Guid.Empty || e.Producto.EmpresaId == _empresaId);
        modelBuilder.Entity<CotizacionDetalle>().HasQueryFilter(e => _empresaId == Guid.Empty || e.CotizacionSerigrafia.Producto.EmpresaId == _empresaId);
        modelBuilder.Entity<CotizacionSerigrafiaProceso>().HasQueryFilter(e => _empresaId == Guid.Empty || e.CotizacionSerigrafia.Producto.EmpresaId == _empresaId);
        modelBuilder.Entity<CotizacionVariantePrecio>().HasQueryFilter(e => _empresaId == Guid.Empty || e.CotizacionSerigrafia.Producto.EmpresaId == _empresaId);
        modelBuilder.Entity<PresupuestoProducto>().HasQueryFilter(e => _empresaId == Guid.Empty || e.Producto.EmpresaId == _empresaId);
        modelBuilder.Entity<PresupuestoDetalle>().HasQueryFilter(e => _empresaId == Guid.Empty || e.PresupuestoProducto.Producto.EmpresaId == _empresaId);
        modelBuilder.Entity<PedidoSerigrafia>().HasQueryFilter(e => _empresaId == Guid.Empty || e.PedidoDetalle.Pedido.Cliente.EmpresaId == _empresaId);
        modelBuilder.Entity<PedidoSerigrafiaProcesoDetalle>().HasQueryFilter(e => _empresaId == Guid.Empty || e.PedidoSerigrafia.PedidoDetalle.Pedido.Cliente.EmpresaId == _empresaId);
        modelBuilder.Entity<PedidoSerigrafiaTalla>().HasQueryFilter(e => _empresaId == Guid.Empty || e.PedidoSerigrafia.PedidoDetalle.Pedido.Cliente.EmpresaId == _empresaId);
        modelBuilder.Entity<PedidoSerigrafiaTallaProceso>().HasQueryFilter(e => _empresaId == Guid.Empty || e.PedidoSerigrafia.PedidoDetalle.Pedido.Cliente.EmpresaId == _empresaId);
        modelBuilder.Entity<RegistroDestajoProceso>().HasQueryFilter(e => _empresaId == Guid.Empty || e.PedidoSerigrafia.PedidoDetalle.Pedido.Cliente.EmpresaId == _empresaId);
        modelBuilder.Entity<Horma>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<Proveedor>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<CuentaPorPagar>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<PagoCxP>().HasQueryFilter(e => _empresaId == Guid.Empty || e.CuentaPorPagar.EmpresaId == _empresaId);
        modelBuilder.Entity<TurnoBase>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<TurnoBaseDetalle>().HasQueryFilter(e => _empresaId == Guid.Empty || e.TurnoBase.EmpresaId == _empresaId);
        modelBuilder.Entity<DepartamentoRrhh>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<NominaCorteRrhh>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<FestivoRrhh>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<BonoRubroRrhh>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<BonoEstructuraRrhh>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<BonoEstructuraDetalleRrhh>().HasQueryFilter(e => _empresaId == Guid.Empty || e.BonoEstructuraRrhh.EmpresaId == _empresaId);
        modelBuilder.Entity<BonoDistribucionPeriodoRrhh>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<BonoDistribucionEmpleadoRrhh>().HasQueryFilter(e => _empresaId == Guid.Empty || (e.BonoDistribucionPeriodoRrhh.EmpresaId == _empresaId && e.Empleado.EmpresaId == _empresaId));
        modelBuilder.Entity<BonoDistribucionEmpleadoDetalleRrhh>().HasQueryFilter(e => _empresaId == Guid.Empty || (e.BonoDistribucionEmpleadoRrhh.BonoDistribucionPeriodoRrhh.EmpresaId == _empresaId && e.BonoRubroRrhh.EmpresaId == _empresaId));
        modelBuilder.Entity<NominaBono>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<NominaBonoDetalle>().HasQueryFilter(e => _empresaId == Guid.Empty || e.NominaBono.EmpresaId == _empresaId);
        modelBuilder.Entity<NominaPercepcionTipo>().HasQueryFilter(e => e.IsActive);
        modelBuilder.Entity<NominaPercepcion>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<DeduccionTipoRrhh>().HasQueryFilter(e => e.IsActive);
        modelBuilder.Entity<NominaDeduccion>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<RrhhChecador>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<RrhhMarcacion>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<RrhhAsistencia>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<RrhhLogChecador>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<Empleado>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<RrhhAusencia>().HasQueryFilter(e => _empresaId == Guid.Empty || (e.EmpresaId == _empresaId && e.Empleado.EmpresaId == _empresaId));
        modelBuilder.Entity<RrhhBancoHorasMovimiento>().HasQueryFilter(e => _empresaId == Guid.Empty || (e.EmpresaId == _empresaId && e.Empleado.EmpresaId == _empresaId));
        modelBuilder.Entity<RrhhEmpleadoTurno>().HasQueryFilter(e => _empresaId == Guid.Empty || (e.EmpresaId == _empresaId && e.Empleado.EmpresaId == _empresaId && e.TurnoBase.EmpresaId == _empresaId));
        modelBuilder.Entity<RrhhEstadoAgente>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<Prenomina>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<PrenominaDetalle>().HasQueryFilter(e => _empresaId == Guid.Empty || e.Prenomina.EmpresaId == _empresaId);
        modelBuilder.Entity<PrenominaBono>().HasQueryFilter(e => _empresaId == Guid.Empty || (e.PrenominaDetalle.Prenomina.EmpresaId == _empresaId && e.BonoRubroRrhh.EmpresaId == _empresaId));
        modelBuilder.Entity<PrenominaPercepcion>().HasQueryFilter(e => _empresaId == Guid.Empty || e.PrenominaDetalle.Prenomina.EmpresaId == _empresaId);
        modelBuilder.Entity<Nomina>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<NominaDetalle>().HasQueryFilter(e => _empresaId == Guid.Empty || e.Nomina.EmpresaId == _empresaId);
        modelBuilder.Entity<EsquemaPago>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<EsquemaPagoTarifa>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EsquemaPago.EmpresaId == _empresaId);
        modelBuilder.Entity<EmpleadoEsquemaPago>().HasQueryFilter(e => _empresaId == Guid.Empty || e.Empleado.EmpresaId == _empresaId);
        modelBuilder.Entity<ValeDestajo>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
        modelBuilder.Entity<ValeDestajoDetalle>().HasQueryFilter(e => _empresaId == Guid.Empty || e.ValeDestajo.EmpresaId == _empresaId);
        modelBuilder.Entity<CatalogoTallaCalzado>().HasQueryFilter(e => _empresaId == Guid.Empty || e.EmpresaId == _empresaId);
    }
}
