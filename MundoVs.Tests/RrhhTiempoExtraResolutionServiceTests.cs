using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Tests;

public sealed class RrhhTiempoExtraResolutionServiceTests
{
    [Fact]
    public async Task ObtenerContextoEmpleadoAsync_CargaSaldoYConfiguracionConsolidada()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var empleado = CreateEmpleado(empresa.Id);

        db.Empresas.Add(empresa);
        db.Empleados.Add(empleado);
        db.AppConfigs.Add(CreateAppConfig(empresa.Id, ClavesConfiguracionNomina.BancoHorasTopeHoras, "45"));
        db.AppConfigs.Add(CreateAppConfig(empresa.Id, ClavesConfiguracionNomina.FactorHoraExtra, "2.5"));
        db.AppConfigs.Add(CreateAppConfig(empresa.Id, ClavesConfiguracionNomina.BancoHorasHabilitado, "true"));
        db.AppConfigs.Add(CreateAppConfig(empresa.Id, ClavesConfiguracionNomina.BancoHorasFactorAcumulacion, "1.5"));
        db.RrhhBancoHorasMovimientos.Add(CreateMovimientoBanco(empresa.Id, empleado.Id, new DateOnly(2026, 1, 1), 8m, TipoMovimientoBancoHorasRrhh.AjusteManual, "saldo-inicial"));
        await db.SaveChangesAsync();

        var service = new RrhhTiempoExtraResolutionService();
        var contexto = await service.ObtenerContextoEmpleadoAsync(db, empresa.Id, empleado.Id);

        Assert.Equal(480, contexto.SaldoBancoHorasMinutos);
        Assert.Equal(2700, contexto.Configuracion.TopeBancoMinutos);
        Assert.Equal(2.5m, contexto.Configuracion.FactorTiempoExtra);
        Assert.True(contexto.Configuracion.BancoHorasHabilitado);
        Assert.Equal(1.5m, contexto.Configuracion.FactorAcumulacionBancoHoras);
    }

    [Fact]
    public async Task AplicarPermisoConGoceBancoHorasAsync_DescuentaSaldoDisponible()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var empleado = CreateEmpleado(empresa.Id);
        var permisoId = Guid.NewGuid();

        db.Empresas.Add(empresa);
        db.Empleados.Add(empleado);
        db.AppConfigs.Add(CreateAppConfig(empresa.Id, ClavesConfiguracionNomina.BancoHorasHabilitado, "true"));
        db.RrhhBancoHorasMovimientos.Add(CreateMovimientoBanco(empresa.Id, empleado.Id, new DateOnly(2026, 1, 1), 8m, TipoMovimientoBancoHorasRrhh.AjusteManual, "saldo-inicial"));
        await db.SaveChangesAsync();

        var service = new RrhhTiempoExtraResolutionService();
        var saldoFinal = await service.AplicarPermisoConGoceBancoHorasAsync(db, new RrhhPermisoBancoHorasCommand
        {
            EmpresaId = empresa.Id,
            EmpleadoId = empleado.Id,
            AusenciaId = permisoId,
            Fecha = new DateOnly(2026, 1, 5),
            HorasPermiso = 3m,
            Observaciones = "Permiso pagado",
            UsuarioActual = "tester"
        });

        await db.SaveChangesAsync();

        Assert.Equal(300, saldoFinal);
        var movimiento = await db.RrhhBancoHorasMovimientos.SingleAsync(m => m.ReferenciaTipo == $"permiso-banco:{permisoId:N}");
        Assert.Equal(TipoMovimientoBancoHorasRrhh.Consumo, movimiento.TipoMovimiento);
        Assert.Equal(-3m, movimiento.Horas);
    }

    [Fact]
    public async Task AplicarPermisoConGoceBancoHorasAsync_PermiteSaldoNegativo()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var empleado = CreateEmpleado(empresa.Id);
        var permisoId = Guid.NewGuid();

        db.Empresas.Add(empresa);
        db.Empleados.Add(empleado);
        db.AppConfigs.Add(CreateAppConfig(empresa.Id, ClavesConfiguracionNomina.BancoHorasHabilitado, "true"));
        db.RrhhBancoHorasMovimientos.Add(CreateMovimientoBanco(empresa.Id, empleado.Id, new DateOnly(2026, 1, 1), 1m, TipoMovimientoBancoHorasRrhh.AjusteManual, "saldo-inicial"));
        await db.SaveChangesAsync();

        var service = new RrhhTiempoExtraResolutionService();
        var saldoFinal = await service.AplicarPermisoConGoceBancoHorasAsync(db, new RrhhPermisoBancoHorasCommand
        {
            EmpresaId = empresa.Id,
            EmpleadoId = empleado.Id,
            AusenciaId = permisoId,
            Fecha = new DateOnly(2026, 1, 5),
            HorasPermiso = 3m,
            UsuarioActual = "tester"
        });

        await db.SaveChangesAsync();

        Assert.Equal(-120, saldoFinal);
        var movimiento = await db.RrhhBancoHorasMovimientos.SingleAsync(m => m.ReferenciaTipo == $"permiso-banco:{permisoId:N}");
        Assert.Equal(TipoMovimientoBancoHorasRrhh.Consumo, movimiento.TipoMovimiento);
        Assert.Equal(-3m, movimiento.Horas);
    }

    [Fact]
    public void ObtenerMinutosPermisoSugeridos_ExcluyeDescansoNoPagadoProgramado()
    {
        var asistencia = new RrhhAsistencia
        {
            MinutosJornadaProgramada = 540,
            MinutosTrabajadosBrutos = 240,
            MinutosJornadaNetaProgramada = 480,
            MinutosTrabajadosNetos = 240
        };

        var ausenciaBruta = RrhhTiempoExtraPolicy.ObtenerMinutosAusenciaBrutaSugerida(asistencia);
        var descansoNoPagado = RrhhTiempoExtraPolicy.ObtenerMinutosDescansoNoPagadoProgramado(asistencia);
        var descansoExcluido = RrhhTiempoExtraPolicy.ObtenerMinutosDescansoNoPagadoExcluidosDelPermiso(asistencia);
        var permisoSugerido = RrhhTiempoExtraPolicy.ObtenerMinutosPermisoSugeridos(asistencia);

        Assert.Equal(300, ausenciaBruta);
        Assert.Equal(60, descansoNoPagado);
        Assert.Equal(60, descansoExcluido);
        Assert.Equal(240, permisoSugerido);
    }

    [Fact]
    public void ObtenerResumenResolucionOperativa_CuandoExistePermiso_NoMuestraPendiente()
    {
        var asistencia = new RrhhAsistencia
        {
            MinutosJornadaNetaProgramada = 510,
            MinutosTrabajadosNetos = 334,
            MinutosExtra = 0,
            RequiereRevision = false
        };

        var resumen = RrhhTiempoExtraPolicy.ObtenerResumenResolucionOperativa(asistencia, "Permiso con goce (2.93 h)");

        Assert.Equal("Permiso con goce (2.93 h)", resumen);
        Assert.False(RrhhTiempoExtraPolicy.TieneResolucionOperativaPendiente(asistencia, "Permiso con goce (2.93 h)"));
    }

    [Fact]
    public void ObtenerResumenResolucionOperativa_CuandoNoHayIncidencias_MuestraSinAjustePendiente()
    {
        var asistencia = new RrhhAsistencia
        {
            MinutosJornadaNetaProgramada = 480,
            MinutosTrabajadosNetos = 480,
            MinutosExtra = 0,
            RequiereRevision = false
        };

        var resumen = RrhhTiempoExtraPolicy.ObtenerResumenResolucionOperativa(asistencia, null);

        Assert.Equal("Sin ajuste pendiente", resumen);
        Assert.False(RrhhTiempoExtraPolicy.TieneResolucionOperativaPendiente(asistencia, null));
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

    private static Empleado CreateEmpleado(Guid empresaId) => new()
    {
        Id = Guid.NewGuid(),
        EmpresaId = empresaId,
        Codigo = "EMP-001",
        NumeroEmpleado = "001",
        Nombre = "Empleado Test",
        CodigoChecador = "3001",
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

    private static RrhhBancoHorasMovimiento CreateMovimientoBanco(Guid empresaId, Guid empleadoId, DateOnly fecha, decimal horas, TipoMovimientoBancoHorasRrhh tipo, string referencia) => new()
    {
        Id = Guid.NewGuid(),
        EmpresaId = empresaId,
        EmpleadoId = empleadoId,
        Fecha = fecha,
        TipoMovimiento = tipo,
        Horas = horas,
        ReferenciaTipo = referencia,
        Observaciones = "Test",
        EsAutomatico = false,
        CreatedAt = DateTime.UtcNow,
        IsActive = true
    };
}
