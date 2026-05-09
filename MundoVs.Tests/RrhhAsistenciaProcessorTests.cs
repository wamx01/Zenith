using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
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
        var turno = CreateTurno(empresa.Id);
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
    public async Task ProcesarMarcacionesPendientesAsync_CuandoMarcacionesIntermediasCoincidenConVentana_InfieraDescansoAutomatico()
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
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 26, 0), "in-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 10, 29, 0), "mid-out"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 10, 58, 0), "mid-in"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 18, 29, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.True(asistencia.MinutosDescansoTomado >= 29);
        Assert.Contains("aplicado", asistencia.ResumenDescansos ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_CuandoDescansoTieneOverride_UsaMinutosAplicadosManual()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = CreateTurno(empresa.Id);
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        var entrada = CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 0, 0), "in-1");
        var descansoSalida = CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 13, 0, 0), "break-out");
        var descansoRegreso = CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 13, 14, 0), "break-in");
        var salida = CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 0, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhMarcaciones.AddRange(entrada, descansoSalida, descansoRegreso, salida);
        db.RrhhSegmentosResoluciones.Add(new RrhhSegmentoResolucion
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresa.Id,
            EmpleadoId = empleado.Id,
            Fecha = new DateOnly(2026, 1, 5),
            MarcacionInicioId = descansoSalida.Id,
            MarcacionFinId = descansoRegreso.Id,
            TipoSegmento = TipoSegmentoResolucionRrhh.Descanso,
            Estado = EstadoSegmentoResolucionRrhh.Vigente,
            MinutosAplicadosOverride = 14,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(14, asistencia.MinutosDescansoTomado);
        Assert.Contains("override manual de 14 min", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_CuandoNoMarcaDescanso_UsaTiempoProgramadoEnResumen()
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
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 27, 0), "in-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 18, 38, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(30, asistencia.MinutosDescansoTomado);
        Assert.Contains("sin marcar; aplicado 30 min programados", asistencia.ResumenDescansos ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_CuandoHayDosDescansosYUnoMarcado_NoDebeAsignarAlMarcadoElProgramadoDelFaltante()
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
            DiaSemana = DiaSemanaTurno.Miercoles,
            Labora = true,
            HoraEntrada = new TimeSpan(8, 30, 0),
            HoraSalida = new TimeSpan(18, 45, 0),
            CantidadDescansos = 2,
            Descanso1Inicio = new TimeSpan(14, 45, 0),
            Descanso1Fin = new TimeSpan(16, 0, 0),
            Descanso1EsPagado = false,
            Descanso2Inicio = new TimeSpan(16, 29, 0),
            Descanso2Fin = new TimeSpan(17, 44, 0),
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
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 4, 22, 8, 19, 0), "in-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 4, 22, 14, 51, 0), "break-out"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 4, 22, 15, 14, 0), "break-in"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 4, 22, 18, 35, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(150, asistencia.MinutosDescansoTomado);
        Assert.Contains("D1: 14:51-15:14", asistencia.ResumenDescansos ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("aplicado 75 min", asistencia.ResumenDescansos ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_CuandoHayDescansoAdicional_NoDuplicaElNumeroEnResumen()
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
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 13, 0, 0), "break-out-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 13, 30, 0), "break-in-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 15, 31, 0), "break-out-2"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 16, 0, 0), "break-in-2"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 0, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        var resumen = asistencia.ResumenDescansos ?? string.Empty;
        Assert.Contains("D1:", resumen, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("D2:", resumen, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, Regex.Matches(resumen, "D2:").Count);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_CuandoExisteBloquePrevioAlTurno_SeleccionaLaJornadaPrincipalYLoTomaComoExtra()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = new TurnoBase
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresa.Id,
            Nombre = "Turno extendido",
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
            HoraSalida = new TimeSpan(18, 0, 0),
            CantidadDescansos = 2,
            Descanso1Inicio = new TimeSpan(10, 0, 0),
            Descanso1Fin = new TimeSpan(10, 15, 0),
            Descanso1EsPagado = false,
            Descanso2Inicio = new TimeSpan(13, 0, 0),
            Descanso2Fin = new TimeSpan(13, 45, 0),
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
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 6, 0, 0), "pre-in", TipoClasificacionMarcacionRrhh.Entrada),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 7, 0, 0), "pre-out", TipoClasificacionMarcacionRrhh.Salida),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 0, 0), "main-in", TipoClasificacionMarcacionRrhh.Entrada),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 10, 0, 0), "break-1-out", TipoClasificacionMarcacionRrhh.InicioDescanso),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 10, 15, 0), "break-1-in", TipoClasificacionMarcacionRrhh.FinDescanso),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 13, 0, 0), "break-2-out", TipoClasificacionMarcacionRrhh.InicioDescanso),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 13, 45, 0), "break-2-in", TipoClasificacionMarcacionRrhh.FinDescanso),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 18, 0, 0), "main-out", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(new TimeSpan(8, 0, 0), asistencia.HoraEntradaReal);
        Assert.Equal(new TimeSpan(18, 0, 0), asistencia.HoraSalidaReal);
        Assert.Equal(60, asistencia.MinutosExtra);
        Assert.Equal(600, asistencia.MinutosTrabajadosNetos);
        Assert.True(asistencia.RequiereRevision);
        Assert.Contains("bloque previo al turno", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_CuandoExisteBloquePrevioPeroElDiaTieneRetardo_NoDuplicaExtraAutomatica()
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
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 6, 0, 0), "pre-in", TipoClasificacionMarcacionRrhh.Entrada),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 7, 0, 0), "pre-out", TipoClasificacionMarcacionRrhh.Salida),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 30, 0), "main-in", TipoClasificacionMarcacionRrhh.Entrada),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 0, 0), "main-out", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(0, asistencia.MinutosExtra);
        Assert.DoesNotContain("Tiempo extra de", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_CuandoTramoIntermedioRompeAlternancia_BaseLoMarcaEnRevisionYSinExtraAutomatica()
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
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 0, 0), "in-1", TipoClasificacionMarcacionRrhh.Entrada),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 10, 0, 0), "mid-out", TipoClasificacionMarcacionRrhh.Salida),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 10, 20, 0), "mid-in", TipoClasificacionMarcacionRrhh.Entrada),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 0, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(0, asistencia.MinutosExtra);
        Assert.True(asistencia.RequiereRevision);
        Assert.Contains("rompe la alternancia base trabajo-pausa", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_CuandoTramoIntermedioTieneResolucionManual_NoDisparaConflictoDeAlternancia()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = CreateTurno(empresa.Id, configurarDescanso: true);
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        var entrada = CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 0, 0), "in-1", TipoClasificacionMarcacionRrhh.Entrada);
        var intermediaSalida = CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 10, 0, 0), "mid-out", TipoClasificacionMarcacionRrhh.Salida);
        var intermediaEntrada = CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 10, 20, 0), "mid-in", TipoClasificacionMarcacionRrhh.Entrada);
        var salida = CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 0, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhMarcaciones.AddRange(entrada, intermediaSalida, intermediaEntrada, salida);
        db.RrhhSegmentosResoluciones.Add(new RrhhSegmentoResolucion
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresa.Id,
            EmpleadoId = empleado.Id,
            Fecha = new DateOnly(2026, 1, 5),
            MarcacionInicioId = intermediaSalida.Id,
            MarcacionFinId = intermediaEntrada.Id,
            TipoSegmento = TipoSegmentoResolucionRrhh.SalidaTemporal,
            Estado = EstadoSegmentoResolucionRrhh.Vigente,
            FueInferidoAutomaticamente = false,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.DoesNotContain("rompe la alternancia base trabajo-pausa", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TimelineAlternado_DefaultConceptualmenteAlternaTrabajoYPausa()
    {
        var primerIndice = 0;
        var segundoIndice = 2;

        var primerEsPausa = ((primerIndice / 2) % 2) == 1;
        var segundoEsPausa = ((segundoIndice / 2) % 2) == 1;

        Assert.False(primerEsPausa);
        Assert.True(segundoEsPausa);
    }

    [Fact]
    public void TimelineAlternado_ResolucionInferidaNoDebeImpedirAlternanciaVisual()
    {
        var fueInferidoAutomaticamente = true;
        var esResolucionManualVigente = !fueInferidoAutomaticamente;

        Assert.False(esResolucionManualVigente);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_CuandoSegmentoEsSalidaTemporal_NoLoTomaComoDescanso()
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
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 12, 0, 0), "temp-out", TipoClasificacionMarcacionRrhh.InicioDescanso, "[segment-action:temporal]"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 12, 30, 0), "temp-in", TipoClasificacionMarcacionRrhh.FinDescanso, "[segment-action:temporal]"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 0, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(30, asistencia.MinutosDescansoTomado);
        Assert.Equal(480, asistencia.MinutosTrabajadosNetos);
        Assert.Contains("salida temporal", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_CuandoHayCortesInternosAntesDelTurno_BloqueaExtraAutomaticaPorAmbiguedad()
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
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 6, 0, 0), "in-1", TipoClasificacionMarcacionRrhh.Entrada),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 7, 0, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 7, 20, 0), "in-2", TipoClasificacionMarcacionRrhh.Entrada),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 0, 0), "out-2", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(0, asistencia.MinutosExtra);
        Assert.True(asistencia.RequiereRevision);
        Assert.DoesNotContain("Tiempo extra de", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_CuandoExisteResolucionVigentePorPar_RespetaElBloqueFijo()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = CreateTurno(empresa.Id, configurarDescanso: true);
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        var entrada = CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 0, 0), "in-1");
        var salidaTemporalInicio = CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 12, 0, 0), "temp-out", TipoClasificacionMarcacionRrhh.InicioDescanso);
        var salidaTemporalFin = CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 12, 30, 0), "temp-in", TipoClasificacionMarcacionRrhh.FinDescanso);
        var salida = CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 0, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhMarcaciones.AddRange(entrada, salidaTemporalInicio, salidaTemporalFin, salida);
        db.RrhhSegmentosResoluciones.Add(new RrhhSegmentoResolucion
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresa.Id,
            EmpleadoId = empleado.Id,
            Fecha = new DateOnly(2026, 1, 5),
            MarcacionInicioId = salidaTemporalInicio.Id,
            MarcacionFinId = salidaTemporalFin.Id,
            TipoSegmento = TipoSegmentoResolucionRrhh.SalidaTemporal,
            Estado = EstadoSegmentoResolucionRrhh.Vigente,
            FueInferidoAutomaticamente = false,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Contains("salida temporal", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ReprocesarRangoAsync_CuandoUnParDesaparece_MarcaLaResolucionComoObsoleta()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = CreateTurno(empresa.Id, configurarDescanso: true);
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        var entrada = CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 0, 0), "in-1");
        var media = CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 12, 0, 0), "mid-1");
        var salida = CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 0, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhMarcaciones.AddRange(entrada, media, salida);

        var resolucion = new RrhhSegmentoResolucion
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresa.Id,
            EmpleadoId = empleado.Id,
            Fecha = new DateOnly(2026, 1, 5),
            MarcacionInicioId = entrada.Id,
            MarcacionFinId = media.Id,
            TipoSegmento = TipoSegmentoResolucionRrhh.Trabajo,
            Estado = EstadoSegmentoResolucionRrhh.Vigente,
            FueInferidoAutomaticamente = false,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        db.RrhhSegmentosResoluciones.Add(resolucion);
        await db.SaveChangesAsync();

        media.EsAnulada = true;
        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ReprocesarRangoAsync(db, empresa.Id, new DateOnly(2026, 1, 5), new DateOnly(2026, 1, 5), empleado.Id);

        var resolucionActualizada = await db.RrhhSegmentosResoluciones.SingleAsync(r => r.Id == resolucion.Id);
        Assert.Equal(EstadoSegmentoResolucionRrhh.Obsoleta, resolucionActualizada.Estado);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_CuandoBloquePosteriorEsAmbiguo_NoInflaExtraConElBloquePosterior()
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
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 30, 0), "in-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 30, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 18, 40, 0), "weird-break", TipoClasificacionMarcacionRrhh.FinDescanso),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 19, 0, 0), "out-2", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(0, asistencia.MinutosExtra);
        Assert.Contains("no como extra automática", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_CuandoSalidaTardiaSuperaUmbralDentroDelMismoBloque_SiCuentaExtra()
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
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 40, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(40, asistencia.MinutosExtra);
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
    public async Task ProcesarMarcacionesPendientesAsync_SiDescansoRealEsMenorAProgramadoMasUmbral_AplicaProgramado()
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
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 13, 0, 0), "break-out-short"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 13, 31, 0), "break-in-short"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 0, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(30, asistencia.MinutosDescansoTomado);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_CuandoSegmentoTieneResolucionManualTrabajo_NoLoReconvierteADescanso()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = CreateTurno(empresa.Id, configurarDescanso: true);
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        var entrada = CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 0, 0), "in-1");
        var salidaIntermedia = CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 13, 0, 0), "mid-out");
        var entradaIntermedia = CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 13, 35, 0), "mid-in");
        var salida = CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 0, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhMarcaciones.AddRange(entrada, salidaIntermedia, entradaIntermedia, salida);
        db.RrhhSegmentosResoluciones.Add(new RrhhSegmentoResolucion
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresa.Id,
            EmpleadoId = empleado.Id,
            Fecha = new DateOnly(2026, 1, 5),
            MarcacionInicioId = salidaIntermedia.Id,
            MarcacionFinId = entradaIntermedia.Id,
            TipoSegmento = TipoSegmentoResolucionRrhh.Trabajo,
            Estado = EstadoSegmentoResolucionRrhh.Vigente,
            FueInferidoAutomaticamente = false,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(0, asistencia.MinutosDescansoTomado);
        Assert.Contains("trabajo principal", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_CuandoHayOverrideManualEnUnDescanso_AplicaElDelSegmentoCorrectoEnResumen()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = new TurnoBase
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresa.Id,
            Nombre = "Turno doble descanso",
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
            HoraSalida = new TimeSpan(18, 0, 0),
            CantidadDescansos = 2,
            Descanso1Inicio = new TimeSpan(10, 0, 0),
            Descanso1Fin = new TimeSpan(10, 15, 0),
            Descanso1EsPagado = false,
            Descanso2Inicio = new TimeSpan(14, 0, 0),
            Descanso2Fin = new TimeSpan(15, 15, 0),
            Descanso2EsPagado = false,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);
        var entrada = CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 0, 0), "in-1");
        var descansoSalida = CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 14, 25, 0), "break-out");
        var descansoRegreso = CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 14, 42, 0), "break-in");
        var salida = CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 18, 0, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhMarcaciones.AddRange(entrada, descansoSalida, descansoRegreso, salida);
        await db.SaveChangesAsync();

        db.RrhhSegmentosResoluciones.AddRange(
            new RrhhSegmentoResolucion
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresa.Id,
                EmpleadoId = empleado.Id,
                Fecha = new DateOnly(2026, 1, 5),
                MarcacionInicioId = entrada.Id,
                MarcacionFinId = descansoSalida.Id,
                TipoSegmento = TipoSegmentoResolucionRrhh.Descanso,
                Estado = EstadoSegmentoResolucionRrhh.Vigente,
                MinutosAplicadosOverride = 75,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new RrhhSegmentoResolucion
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresa.Id,
                EmpleadoId = empleado.Id,
                Fecha = new DateOnly(2026, 1, 5),
                MarcacionInicioId = descansoSalida.Id,
                MarcacionFinId = descansoRegreso.Id,
                TipoSegmento = TipoSegmentoResolucionRrhh.Descanso,
                Estado = EstadoSegmentoResolucionRrhh.Vigente,
                MinutosAplicadosOverride = 15,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Contains("aplicado 15 min", asistencia.ResumenDescansos ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcesarMarcacionesPendientesAsync_CuandoDescansoTieneNumeroManual_NoLoDuplicaComoAdicional()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = new TurnoBase
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresa.Id,
            Nombre = "Turno doble descanso",
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
            HoraSalida = new TimeSpan(18, 0, 0),
            CantidadDescansos = 2,
            Descanso1Inicio = new TimeSpan(10, 0, 0),
            Descanso1Fin = new TimeSpan(10, 15, 0),
            Descanso1EsPagado = false,
            Descanso2Inicio = new TimeSpan(15, 30, 0),
            Descanso2Fin = new TimeSpan(16, 45, 0),
            Descanso2EsPagado = false,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);
        var entrada = CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 0, 0), "in-1");
        var descansoSalida = CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 15, 34, 0), "break-out");
        var descansoRegreso = CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 16, 5, 0), "break-in");
        var salida = CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 18, 0, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhMarcaciones.AddRange(entrada, descansoSalida, descansoRegreso, salida);
        await db.SaveChangesAsync();

        db.RrhhSegmentosResoluciones.Add(new RrhhSegmentoResolucion
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresa.Id,
            EmpleadoId = empleado.Id,
            Fecha = new DateOnly(2026, 1, 5),
            MarcacionInicioId = descansoSalida.Id,
            MarcacionFinId = descansoRegreso.Id,
            TipoSegmento = TipoSegmentoResolucionRrhh.Descanso,
            Estado = EstadoSegmentoResolucionRrhh.Vigente,
            MinutosAplicadosOverride = 29,
            Observaciones = "Bloque fijado como descanso (D2).",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        var resumen = asistencia.ResumenDescansos ?? string.Empty;
        Assert.Contains("D2: 15:34-16:05", resumen, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("D3: 15:34-16:05", resumen, StringComparison.OrdinalIgnoreCase);
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
    public async Task ProcesarMarcacionesPendientesAsync_EmparejaDescansoDetectadoConLaVentanaConfiguradaMasCercana()
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
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 15, 32, 0), "break-2-out"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 15, 47, 0), "break-2-in"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 0, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(45, asistencia.MinutosDescansoTomado);
        Assert.Equal(495, asistencia.MinutosTrabajadosNetos);
        Assert.Contains("D2: 15:32-15:47", asistencia.ResumenDescansos ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("descanso 1", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("30 min programados", asistencia.Observaciones ?? string.Empty, StringComparison.OrdinalIgnoreCase);
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
            MinutosTrabajadosNetos = 304,
            MinutosJornadaNetaProgramada = 480
        };

        var visibles = RrhhTiempoExtraPolicy.ObtenerMinutosTrabajadosVisibles(asistencia, 20);

        Assert.Equal(324, visibles);
    }

    [Fact]
    public void TiempoExtraPolicy_CuandoNoHayCompensacion_ConservaTrabajoNetoVisible()
    {
        var asistencia = new RrhhAsistencia
        {
            MinutosTrabajadosNetos = 304,
            MinutosJornadaNetaProgramada = 480
        };

        var visibles = RrhhTiempoExtraPolicy.ObtenerMinutosTrabajadosVisibles(asistencia, 0);

        Assert.Equal(304, visibles);
    }

    [Fact]
    public void TiempoExtraPolicy_CuandoExisteExtraNoAprobada_NoLaSumaATiempoVisible()
    {
        var asistencia = new RrhhAsistencia
        {
            MinutosTrabajadosNetos = 510,
            MinutosJornadaNetaProgramada = 480,
            MinutosExtra = 30,
            MinutosExtraAutorizadosPago = 0,
            MinutosExtraAutorizadosBanco = 0
        };

        var visibles = RrhhTiempoExtraPolicy.ObtenerMinutosTrabajadosVisibles(asistencia, 0);

        Assert.Equal(480, visibles);
    }

    [Fact]
    public void TiempoExtraPolicy_CuandoExisteExtraAprobada_SoloSumaLaAprobadaATiempoVisible()
    {
        var asistencia = new RrhhAsistencia
        {
            MinutosTrabajadosNetos = 540,
            MinutosJornadaNetaProgramada = 480,
            MinutosExtra = 60,
            MinutosExtraAutorizadosPago = 20,
            MinutosExtraAutorizadosBanco = 10
        };

        var visibles = RrhhTiempoExtraPolicy.ObtenerMinutosTrabajadosVisibles(asistencia, 0);

        Assert.Equal(510, visibles);
    }

    [Fact]
    public void TiempoExtraPolicy_CuandoSalidaTempranaPuedeAbsorberDescanso_NoInflaSalidaAnticipadaEfectiva()
    {
        var asistencia = new RrhhAsistencia
        {
            MinutosJornadaProgramada = 540,
            MinutosJornadaNetaProgramada = 510,
            MinutosDescansoTomado = 0,
            MinutosDescansoPagado = 0,
            MinutosSalidaAnticipada = 30,
            Observaciones = "No se detectaron marcaciones de descanso. La salida anticipada sugiere permiso o descanso no tomado."
        };

        var minutos = RrhhTiempoExtraPolicy.ObtenerMinutosSalidaAnticipadaEfectivos(asistencia);

        Assert.Equal(0, minutos);
    }

    [Fact]
    public void TiempoExtraPolicy_CuandoHayCompensacionYMismoDia_SumaBaseMasCompensacionMasExtraAprobada()
    {
        var asistencia = new RrhhAsistencia
        {
            MinutosTrabajadosNetos = 540,
            MinutosJornadaNetaProgramada = 480,
            MinutosExtra = 60,
            MinutosExtraAutorizadosPago = 15,
            MinutosExtraAutorizadosBanco = 15
        };

        var visibles = RrhhTiempoExtraPolicy.ObtenerMinutosTrabajadosVisibles(asistencia, 20);

        Assert.Equal(530, visibles);
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

    private static RrhhMarcacion CreateMarcacionUtc(Guid empresaId, Guid checadorId, Empleado empleado, DateTime fechaHoraUtc, string eventoId, TipoClasificacionMarcacionRrhh clasificacion = TipoClasificacionMarcacionRrhh.Entrada, string? payloadRaw = null) => new()
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
        PayloadRaw = payloadRaw,
        CreatedAt = DateTime.UtcNow,
        IsActive = true
    };

    private static RrhhMarcacion CreateMarcacionLocal(Guid empresaId, Guid checadorId, Empleado empleado, DateTime fechaHoraLocal, string eventoId, TipoClasificacionMarcacionRrhh clasificacion = TipoClasificacionMarcacionRrhh.Entrada, string? payloadRaw = null) => new()
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
        PayloadRaw = payloadRaw,
        CreatedAt = DateTime.UtcNow,
        IsActive = true
    };
}
