using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Models;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Tests;

public sealed class RrhhAsistenciaProcessorTests
{
    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_CuandoRetardoSeCompensaConSalidaTardia_NoGeneraExtra()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = CreateTurno(empresa.Id);
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhMarcaciones.AddRange(
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 6, 0), "in-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 30, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(empleado.Id, asistencia.EmpleadoId);
        Assert.Equal(turno.Id, asistencia.TurnoBaseId);
        Assert.Equal(new DateOnly(2026, 1, 5), asistencia.Fecha);
        Assert.Equal(new TimeSpan(8, 0, 0), asistencia.HoraEntradaProgramada);
        Assert.Equal(new TimeSpan(17, 0, 0), asistencia.HoraSalidaProgramada);
        Assert.Equal(new TimeSpan(8, 6, 0), asistencia.HoraEntradaReal);
        Assert.Equal(new TimeSpan(17, 30, 0), asistencia.HoraSalidaReal);
        Assert.Equal(2, asistencia.TotalMarcaciones);
        Assert.Equal(540, asistencia.MinutosJornadaProgramada);
        Assert.Equal(564, asistencia.MinutosTrabajadosNetos);
        Assert.Equal(6, asistencia.MinutosRetardo);
        Assert.Equal(0, asistencia.MinutosSalidaAnticipada);
        Assert.Equal(0, asistencia.MinutosExtra);
        Assert.Equal(RrhhAsistenciaEstatus.Retardo, asistencia.Estatus);
        Assert.False(asistencia.RequiereRevision);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_CuandoSalidaTardiaExcedeRetardo_YaSoloCuentaRemanenteComoExtra()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = CreateTurno(empresa.Id);
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhMarcaciones.AddRange(
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 6, 0), "in-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 40, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(6, asistencia.MinutosRetardo);
        Assert.Equal(34, asistencia.MinutosExtra);
        Assert.Contains("Tiempo extra de 34 min.", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_NoMarcaRetardoDentroDeToleranciaConfigurada()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = CreateTurno(empresa.Id);
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhMarcaciones.AddRange(
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 5, 0), "in-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 0, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(0, asistencia.MinutosRetardo);
        Assert.Equal(RrhhAsistenciaEstatus.AsistenciaNormal, asistencia.Estatus);
        Assert.DoesNotContain("Retardo", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_SiRetardoRebasaTolerancia_SiLoMarca()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = CreateTurno(empresa.Id);
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhMarcaciones.AddRange(
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 6, 0), "in-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 0, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(6, asistencia.MinutosRetardo);
        Assert.Equal(RrhhAsistenciaEstatus.Retardo, asistencia.Estatus);
        Assert.Contains("Retardo de 6 min.", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_UsaHoraLocalGuardadaAunqueCambieZonaDelChecador()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = CreateTurno(empresa.Id);
        var checador = CreateChecador(empresa.Id);
        checador.ZonaHoraria = "UTC-07:00";
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhMarcaciones.AddRange(
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 5, 0), "local-in-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 0, 0), "local-out-1", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(new TimeSpan(8, 5, 0), asistencia.HoraEntradaReal);
        Assert.Equal(new TimeSpan(17, 0, 0), asistencia.HoraSalidaReal);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_NoCuentaEntradaAnticipadaMenorA30MinComoTiempoLaboral()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = CreateTurno(empresa.Id);
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhMarcaciones.AddRange(
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 7, 45, 0), "in-early-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 0, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(new TimeSpan(7, 45, 0), asistencia.HoraEntradaReal);
        Assert.Equal(new TimeSpan(17, 0, 0), asistencia.HoraSalidaReal);
        Assert.Equal(540, asistencia.MinutosTrabajadosNetos);
        Assert.Equal(0, asistencia.MinutosExtra);
        Assert.Equal(0, asistencia.MinutosRetardo);
        Assert.False(asistencia.RequiereRevision);
        Assert.Contains("no se contaron como tiempo laboral", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_NoCuentaSalidaPosteriorMenorA30MinComoTiempoLaboral()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = CreateTurno(empresa.Id);
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhMarcaciones.AddRange(
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 0, 0), "in-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 20, 0), "out-late-1", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(new TimeSpan(8, 0, 0), asistencia.HoraEntradaReal);
        Assert.Equal(new TimeSpan(17, 20, 0), asistencia.HoraSalidaReal);
        Assert.Equal(540, asistencia.MinutosTrabajadosNetos);
        Assert.Equal(0, asistencia.MinutosExtra);
        Assert.Contains("posteriores a la salida programada", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_CuentaTiempoExtraCuandoAlcanza30MinDespuesDeLaSalida()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = CreateTurno(empresa.Id);
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhMarcaciones.AddRange(
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 0, 0), "in-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 30, 0), "out-late-30", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(570, asistencia.MinutosTrabajadosNetos);
        Assert.Equal(30, asistencia.MinutosExtra);
        Assert.Contains("Tiempo extra de 30 min.", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_UsaMinutosMinimosConfiguradosParaTiempoExtra()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = CreateTurno(empresa.Id);
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.AppConfigs.Add(CreateAppConfig(empresa.Id, ClavesConfiguracionNomina.MinutosMinimosTiempoExtra, "10"));
        db.RrhhMarcaciones.AddRange(
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 7, 45, 0), "in-early-config"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 0, 0), "out-config", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(555, asistencia.MinutosTrabajadosNetos);
        Assert.Equal(15, asistencia.MinutosExtra);
        Assert.Contains("Tiempo extra de 15 min.", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_SinDescansosMarcados_NoGeneraExtraSiEntradaYSalidaNoAlcanzanUmbral()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = CreateTurno(empresa.Id, configurarDescanso: true, configurarSegundoDescanso: true);
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhMarcaciones.AddRange(
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 7, 44, 0), "in-early-16"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 12, 0), "out-late-12", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(45, asistencia.MinutosDescansoTomado);
        Assert.Equal(495, asistencia.MinutosTrabajadosNetos);
        Assert.Equal(0, asistencia.MinutosExtra);
        Assert.Contains("Los 16 min previos a la entrada programada no se contaron como tiempo laboral", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Los 12 min posteriores a la salida programada no se contaron como tiempo laboral", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_CasoHorarioConDosDescansosLargos_NoSumaMinutosBajoUmbralComoExtra()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = new TurnoBase
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresa.Id,
            Nombre = "Turno ABY",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        turno.Detalles.Add(new TurnoBaseDetalle
        {
            Id = Guid.NewGuid(),
            TurnoBaseId = turno.Id,
            DiaSemana = DiaSemanaTurno.Martes,
            Labora = true,
            HoraEntrada = new TimeSpan(8, 30, 0),
            HoraSalida = new TimeSpan(18, 30, 0),
            CantidadDescansos = 2,
            Descanso1Inicio = new TimeSpan(10, 0, 0),
            Descanso1Fin = new TimeSpan(10, 15, 0),
            Descanso1EsPagado = false,
            Descanso2Inicio = new TimeSpan(13, 45, 0),
            Descanso2Fin = new TimeSpan(15, 0, 0),
            Descanso2EsPagado = false,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhMarcaciones.AddRange(
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 4, 14, 8, 14, 0), "in-early-16"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 4, 14, 18, 42, 0), "out-late-12", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(new DateOnly(2026, 4, 14), asistencia.Fecha);
        Assert.Equal(90, asistencia.MinutosDescansoTomado);
        Assert.Equal(510, asistencia.MinutosTrabajadosNetos);
        Assert.Equal(0, asistencia.MinutosExtra);
        Assert.Contains("Los 16 min previos a la entrada programada no se contaron como tiempo laboral", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Los 12 min posteriores a la salida programada no se contaron como tiempo laboral", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_DescansoFueraDeVentanaConDuracionCorrecta_NoMarcaRevision()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = CreateTurno(empresa.Id, configurarDescanso: true);
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhMarcaciones.AddRange(
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 0, 0), "in-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 13, 10, 0), "break-out-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 13, 40, 0), "break-in-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 0, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(4, asistencia.TotalMarcaciones);
        Assert.Equal(30, asistencia.MinutosDescansoTomado);
        Assert.Equal(510, asistencia.MinutosTrabajadosNetos);
        Assert.False(asistencia.RequiereRevision);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_SiDescansaMenos_DescuentaElTiempoPlaneado()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = CreateTurno(empresa.Id, configurarDescanso: true);
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhMarcaciones.AddRange(
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 0, 0), "in-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 13, 0, 0), "break-out-short"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 13, 10, 0), "break-in-short"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 0, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(30, asistencia.MinutosDescansoTomado);
        Assert.Equal(510, asistencia.MinutosTrabajadosNetos);
        Assert.False(asistencia.RequiereRevision);
        Assert.Contains("se aplicaron 30 min programados", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_SiExcedeDescansoDentroDeTolerancia_NoLoCastiga()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = CreateTurno(empresa.Id, configurarDescanso: true);
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.AppConfigs.Add(CreateAppConfig(empresa.Id, RrhhAsistenciaDescansoSettings.ToleranciaExcesoDescansoKey, "5"));
        db.RrhhMarcaciones.AddRange(
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 0, 0), "in-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 13, 0, 0), "break-out-tol"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 13, 34, 0), "break-in-tol"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 0, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(30, asistencia.MinutosDescansoTomado);
        Assert.Equal(510, asistencia.MinutosTrabajadosNetos);
        Assert.False(asistencia.RequiereRevision);
        Assert.DoesNotContain("excedió", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_SiExcedeDescansoFueraDeTolerancia_SiLoCastiga()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = CreateTurno(empresa.Id, configurarDescanso: true);
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.AppConfigs.Add(CreateAppConfig(empresa.Id, RrhhAsistenciaDescansoSettings.ToleranciaExcesoDescansoKey, "5"));
        db.RrhhMarcaciones.AddRange(
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 0, 0), "in-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 13, 0, 0), "break-out-over"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 13, 36, 0), "break-in-over"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 0, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(36, asistencia.MinutosDescansoTomado);
        Assert.Equal(504, asistencia.MinutosTrabajadosNetos);
        Assert.True(asistencia.RequiereRevision);
        Assert.Contains("excedió", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_CuandoFaltaUnDescansoEsperado_DescuentaElPlaneadoFaltante()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = CreateTurno(empresa.Id, configurarDescanso: true, configurarSegundoDescanso: true);
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhMarcaciones.AddRange(
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 0, 0), "in-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 13, 0, 0), "break-out-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 13, 30, 0), "break-in-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 0, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(45, asistencia.MinutosDescansoTomado);
        Assert.Equal(495, asistencia.MinutosTrabajadosNetos);
        Assert.False(asistencia.RequiereRevision);
        Assert.Contains("descanso 2", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("15 min programados", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_DescuentaDescansoAutomaticamente_CuandoLaJornadaCoincideConLaBruta()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = CreateTurno(empresa.Id, configurarDescanso: true);
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhMarcaciones.AddRange(
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 0, 0), "in-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 0, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(30, asistencia.MinutosDescansoTomado);
        Assert.Equal(30, asistencia.MinutosDescansoNoPagado);
        Assert.Equal(510, asistencia.MinutosTrabajadosNetos);
        Assert.Equal(0, asistencia.MinutosExtra);
        Assert.False(asistencia.RequiereRevision);
        Assert.Contains("se aplicaron 30 min programados", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("sin marcar; aplicado 30 min programados", asistencia.ResumenDescansos ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_RequiereConfirmacion_CuandoLaJornadaCoincideConLaNeta()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = CreateTurno(empresa.Id, configurarDescanso: true);
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhMarcaciones.AddRange(
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 0, 0), "in-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 16, 30, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(0, asistencia.MinutosDescansoTomado);
        Assert.Equal(510, asistencia.MinutosTrabajadosNetos);
        Assert.Equal(30, asistencia.MinutosSalidaAnticipada);
        Assert.Equal(0, asistencia.MinutosExtra);
        Assert.True(asistencia.RequiereRevision);
        Assert.Contains("permiso o descanso no tomado", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_SiHayPermisoParcial_NoDescuentaDescansosNoMarcados()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = CreateTurno(empresa.Id, configurarDescanso: true, configurarSegundoDescanso: true);
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhAusencias.Add(CreatePermisoParcial(empresa.Id, empleado.Id, new DateOnly(2026, 1, 5), 3m));
        db.RrhhMarcaciones.AddRange(
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 0, 0), "in-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 16, 45, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(0, asistencia.MinutosDescansoTomado);
        Assert.Equal(525, asistencia.MinutosTrabajadosNetos);
        Assert.Equal(15, asistencia.MinutosSalidaAnticipada);
        Assert.True(asistencia.RequiereRevision);
        Assert.Contains("permiso parcial registrado", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("cubierto por permiso del día", asistencia.ResumenDescansos ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PoliticaCompensacionPermiso_CuandoHaySalidaPosteriorBajoUmbral_ObtieneMinutosRecuperablesAprobables()
    {
        var asistencia = new RrhhAsistencia
        {
            HoraEntradaProgramada = new TimeSpan(8, 0, 0),
            HoraSalidaProgramada = new TimeSpan(17, 0, 0),
            HoraEntradaReal = new TimeSpan(8, 0, 0),
            HoraSalidaReal = new TimeSpan(17, 20, 0)
        };

        var minutos = RrhhPermisoCompensationPolicy.ObtenerMinutosRecuperablesAprobables(asistencia, 30);

        Assert.Equal(20, minutos);
    }

    [Fact]
    public void TiempoExtraPolicy_CuandoHayCompensacionAprobada_ReducePermisoSugerido()
    {
        var asistencia = new RrhhAsistencia
        {
            MinutosJornadaNetaProgramada = 480,
            MinutosTrabajadosNetos = 304
        };

        var sugerido = RrhhTiempoExtraPolicy.ObtenerMinutosPermisoSugeridos(asistencia, 20);

        Assert.Equal(156, sugerido);
    }

    [Fact]
    public void TiempoExtraPolicy_CuandoHayCompensacionAprobada_AumentaTrabajoVisible()
    {
        var asistencia = new RrhhAsistencia
        {
            MinutosTrabajadosNetos = 304
        };

        var visibles = RrhhTiempoExtraPolicy.ObtenerMinutosTrabajadosVisibles(asistencia, 20);

        Assert.Equal(324, visibles);
    }

    [Fact]
    public void TiempoExtraPolicy_CuandoNoHayCompensacion_ConservaTrabajoNetoVisible()
    {
        var asistencia = new RrhhAsistencia
        {
            MinutosTrabajadosNetos = 304
        };

        var visibles = RrhhTiempoExtraPolicy.ObtenerMinutosTrabajadosVisibles(asistencia, 0);

        Assert.Equal(304, visibles);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_SiSalidaAnticipadaNoCubreElDescanso_DescuentaElProgramado()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = CreateTurno(empresa.Id, configurarDescanso: true);
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhMarcaciones.AddRange(
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 0, 0), "in-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 16, 45, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(30, asistencia.MinutosDescansoTomado);
        Assert.Equal(495, asistencia.MinutosTrabajadosNetos);
        Assert.Equal(0, asistencia.MinutosExtra);
        Assert.True(asistencia.RequiereRevision);
        Assert.Contains("se aplicaron 30 min programados", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_SiSalidaAnticipadaCubreElSegundoDescanso_NoLoDescuentaYSolicitaConfirmacion()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = CreateTurno(empresa.Id, configurarDescanso: true, configurarSegundoDescanso: true);
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhMarcaciones.AddRange(
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 0, 0), "in-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 12, 0, 0), "break-out-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 12, 30, 0), "break-in-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 16, 45, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(30, asistencia.MinutosDescansoTomado);
        Assert.Equal(495, asistencia.MinutosTrabajadosNetos);
        Assert.Equal(15, asistencia.MinutosSalidaAnticipada);
        Assert.True(asistencia.RequiereRevision);
        Assert.Contains("permiso o descanso no tomado", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    private static CrmDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new CrmDbContext(options);
    }

    private static Empresa CreateEmpresa() => new()
    {
        Id = Guid.NewGuid(),
        Codigo = $"EMP-{Guid.NewGuid():N}"[..12],
        RazonSocial = "Empresa Test"
    };

    private static TurnoBase CreateTurno(Guid empresaId, bool configurarDescanso = false, bool configurarSegundoDescanso = false)
    {
        var turno = new TurnoBase
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            Nombre = "Matutino",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        turno.Detalles.Add(new TurnoBaseDetalle
        {
            Id = Guid.NewGuid(),
            TurnoBaseId = turno.Id,
            DiaSemana = DiaSemanaTurno.Lunes,
            Labora = true,
            HoraEntrada = new TimeSpan(8, 0, 0),
            HoraSalida = new TimeSpan(17, 0, 0),
            CantidadDescansos = (byte)((configurarDescanso ? 1 : 0) + (configurarSegundoDescanso ? 1 : 0)),
            Descanso1Inicio = configurarDescanso ? new TimeSpan(12, 0, 0) : null,
            Descanso1Fin = configurarDescanso ? new TimeSpan(12, 30, 0) : null,
            Descanso1EsPagado = false,
            Descanso2Inicio = configurarSegundoDescanso ? new TimeSpan(15, 30, 0) : null,
            Descanso2Fin = configurarSegundoDescanso ? new TimeSpan(15, 45, 0) : null,
            Descanso2EsPagado = false,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        return turno;
    }

    private static RrhhChecador CreateChecador(Guid empresaId) => new()
    {
        Id = Guid.NewGuid(),
        EmpresaId = empresaId,
        Nombre = "Checador Test",
        NumeroSerie = Guid.NewGuid().ToString("N"),
        ZonaHoraria = "America/Mexico_City",
        Ip = "127.0.0.1",
        Puerto = 4370,
        NumeroMaquina = 1,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    private static Empleado CreateEmpleado(Guid empresaId, Guid turnoId) => new()
    {
        Id = Guid.NewGuid(),
        EmpresaId = empresaId,
        Codigo = "EMP-001",
        NumeroEmpleado = "001",
        Nombre = "Empleado Test",
        CodigoChecador = "3001",
        TurnoBaseId = turnoId,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    private static AppConfig CreateAppConfig(Guid empresaId, string clave, string valor) => new()
    {
        Id = Guid.NewGuid(),
        EmpresaId = empresaId,
        Clave = clave,
        Valor = valor,
        CreatedAt = DateTime.UtcNow
    };

    private static RrhhAusencia CreatePermisoParcial(Guid empresaId, Guid empleadoId, DateOnly fecha, decimal horas) => new()
    {
        Id = Guid.NewGuid(),
        EmpresaId = empresaId,
        EmpleadoId = empleadoId,
        Tipo = TipoAusenciaRrhh.Permiso,
        Estatus = EstatusAusenciaRrhh.Aplicada,
        FechaInicio = fecha,
        FechaFin = fecha,
        Dias = 1,
        Horas = horas,
        ConGocePago = true,
        Motivo = "Permiso parcial",
        FechaAprobacion = DateTime.UtcNow,
        AprobadoPor = "tester",
        CreatedAt = DateTime.UtcNow,
        IsActive = true
    };

    private static RrhhMarcacion CreateMarcacionUtc(Guid empresaId, Guid checadorId, Empleado empleado, DateTime fechaHoraUtc, string eventoId, TipoClasificacionMarcacionRrhh clasificacion = TipoClasificacionMarcacionRrhh.Entrada) => new()
    {
        Id = Guid.NewGuid(),
        EmpresaId = empresaId,
        ChecadorId = checadorId,
        EmpleadoId = empleado.Id,
        CodigoChecador = empleado.CodigoChecador!,
        FechaHoraMarcacionUtc = fechaHoraUtc,
        TipoMarcacionRaw = "0",
        Origen = "Test",
        EventoIdExterno = eventoId,
        HashUnico = Guid.NewGuid().ToString("N"),
        ClasificacionOperativa = clasificacion,
        Procesada = false,
        CreatedAt = DateTime.UtcNow,
        IsActive = true
    };

    private static RrhhMarcacion CreateMarcacionLocal(Guid empresaId, Guid checadorId, Empleado empleado, DateTime fechaHoraLocal, string eventoId, TipoClasificacionMarcacionRrhh clasificacion = TipoClasificacionMarcacionRrhh.Entrada) => new()
    {
        Id = Guid.NewGuid(),
        EmpresaId = empresaId,
        ChecadorId = checadorId,
        EmpleadoId = empleado.Id,
        CodigoChecador = empleado.CodigoChecador!,
        FechaHoraMarcacionLocal = fechaHoraLocal,
        FechaHoraMarcacionUtc = DateTime.SpecifyKind(fechaHoraLocal.AddHours(6), DateTimeKind.Utc),
        ZonaHorariaAplicada = "America/Mexico_City",
        TipoMarcacionRaw = "0",
        Origen = "Test",
        EventoIdExterno = eventoId,
        HashUnico = Guid.NewGuid().ToString("N"),
        ClasificacionOperativa = clasificacion,
        Procesada = false,
        CreatedAt = DateTime.UtcNow,
        IsActive = true
    };
}
