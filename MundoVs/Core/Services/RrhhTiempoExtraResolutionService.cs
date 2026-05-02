using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Infrastructure.Data;
using System.Globalization;

namespace MundoVs.Core.Services;

public sealed class RrhhTiempoExtraResolutionService : IRrhhTiempoExtraResolutionService
{
    private const string ReferenciaPermisoBancoPrefix = "permiso-banco";
    private const string ReferenciaCompensacionPermisoBancoPrefix = "permiso-compensacion-banco";

    public async Task<int> ObtenerSaldoBancoHorasAsync(CrmDbContext db, Guid empresaId, Guid empleadoId, CancellationToken cancellationToken = default)
    {
        var horas = await db.RrhhBancoHorasMovimientos
            .AsNoTracking()
            .Where(m => m.EmpresaId == empresaId && m.EmpleadoId == empleadoId && m.IsActive)
            .SumAsync(m => (decimal?)m.Horas, cancellationToken) ?? 0m;
        return (int)Math.Round(horas * 60m, MidpointRounding.AwayFromZero);
    }

    public async Task<RrhhTiempoExtraConfiguracionSnapshot> ObtenerConfiguracionAsync(CrmDbContext db, Guid empresaId, CancellationToken cancellationToken = default)
    {
        var configuraciones = await db.AppConfigs
            .AsNoTracking()
            .Where(c => c.EmpresaId == empresaId
                && (c.Clave == ClavesConfiguracionNomina.BancoHorasTopeHoras
                    || c.Clave == ClavesConfiguracionNomina.FactorHoraExtra
                    || c.Clave == ClavesConfiguracionNomina.BancoHorasHabilitado
                    || c.Clave == ClavesConfiguracionNomina.BancoHorasFactorAcumulacion))
            .ToDictionaryAsync(c => c.Clave, c => c.Valor ?? string.Empty, cancellationToken);

        return new RrhhTiempoExtraConfiguracionSnapshot
        {
            TopeBancoMinutos = ConvertirHorasAMinutos(ObtenerDecimal(configuraciones, ClavesConfiguracionNomina.BancoHorasTopeHoras, 40m)),
            FactorTiempoExtra = Math.Max(0m, ObtenerDecimal(configuraciones, ClavesConfiguracionNomina.FactorHoraExtra, 2m)),
            BancoHorasHabilitado = ObtenerBooleano(configuraciones, ClavesConfiguracionNomina.BancoHorasHabilitado, false),
            FactorAcumulacionBancoHoras = Math.Max(0m, ObtenerDecimal(configuraciones, ClavesConfiguracionNomina.BancoHorasFactorAcumulacion, 1m))
        };
    }

    public async Task<RrhhTiempoExtraEmpleadoContexto> ObtenerContextoEmpleadoAsync(CrmDbContext db, Guid empresaId, Guid empleadoId, CancellationToken cancellationToken = default)
    {
        var saldoBancoHorasMinutos = await ObtenerSaldoBancoHorasAsync(db, empresaId, empleadoId, cancellationToken);
        var configuracion = await ObtenerConfiguracionAsync(db, empresaId, cancellationToken);

        return new RrhhTiempoExtraEmpleadoContexto
        {
            Configuracion = configuracion,
            SaldoBancoHorasMinutos = saldoBancoHorasMinutos
        };
    }

    public async Task<int> ObtenerTopeBancoHorasAsync(CrmDbContext db, Guid empresaId, CancellationToken cancellationToken = default)
        => (await ObtenerConfiguracionAsync(db, empresaId, cancellationToken)).TopeBancoMinutos;

    public async Task<decimal> ObtenerFactorTiempoExtraAsync(CrmDbContext db, Guid empresaId, CancellationToken cancellationToken = default)
        => (await ObtenerConfiguracionAsync(db, empresaId, cancellationToken)).FactorTiempoExtra;

    public async Task<bool> ObtenerBancoHorasHabilitadoAsync(CrmDbContext db, Guid empresaId, CancellationToken cancellationToken = default)
        => (await ObtenerConfiguracionAsync(db, empresaId, cancellationToken)).BancoHorasHabilitado;

    public async Task<decimal> ObtenerFactorAcumulacionBancoHorasAsync(CrmDbContext db, Guid empresaId, CancellationToken cancellationToken = default)
        => (await ObtenerConfiguracionAsync(db, empresaId, cancellationToken)).FactorAcumulacionBancoHoras;

    public async Task<RrhhTiempoExtraResolutionResult> AplicarResolucionAsync(CrmDbContext db, RrhhTiempoExtraResolutionCommand command, CancellationToken cancellationToken = default)
    {
        var asistencia = await db.RrhhAsistencias.FirstOrDefaultAsync(a => a.Id == command.AsistenciaId && a.EmpresaId == command.EmpresaId, cancellationToken)
            ?? throw new InvalidOperationException("No se encontró la asistencia a resolver.");

        var empresaId = command.EmpresaId;
        var empleadoId = asistencia.EmpleadoId;
        var referenciaExtraBanco = RrhhTiempoExtraPolicy.ConstruirReferenciaResolucion(asistencia.Id, "extra-banco");
        var referenciaCoberturaBanco = RrhhTiempoExtraPolicy.ConstruirReferenciaResolucion(asistencia.Id, "cobertura-banco");

        var movimientosPrevios = await db.RrhhBancoHorasMovimientos
            .Where(m => m.EmpresaId == empresaId
                && m.EmpleadoId == empleadoId
                && m.IsActive
                && (m.ReferenciaTipo == referenciaExtraBanco || m.ReferenciaTipo == referenciaCoberturaBanco))
            .ToListAsync(cancellationToken);

        var netoPrevioMinutos = (int)Math.Round(movimientosPrevios.Sum(m => m.Horas) * 60m, MidpointRounding.AwayFromZero);
        var contextoEmpleado = await ObtenerContextoEmpleadoAsync(db, empresaId, empleadoId, cancellationToken);
        var saldoBancoMinutos = contextoEmpleado.SaldoBancoHorasMinutos;
        var topeBancoMinutos = contextoEmpleado.Configuracion.TopeBancoMinutos;
        var factorTiempoExtra = contextoEmpleado.Configuracion.FactorTiempoExtra;
        var bancoHorasHabilitado = contextoEmpleado.Configuracion.BancoHorasHabilitado;
        var factorAcumulacionBanco = contextoEmpleado.Configuracion.FactorAcumulacionBancoHoras;
        var saldoBancoDisponible = Math.Max(0, saldoBancoMinutos - netoPrevioMinutos);
        var extraResoluble = RrhhTiempoExtraPolicy.ObtenerMinutosExtraResolubles(asistencia, factorTiempoExtra);
        var faltante = RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteBanco(asistencia);
        var pago = Math.Max(0, command.MinutosPago);
        var banco = bancoHorasHabilitado
            ? (int)Math.Round(Math.Max(0, command.MinutosBanco) * factorAcumulacionBanco, MidpointRounding.AwayFromZero)
            : 0;
        var cubiertoBanco = Math.Max(0, command.MinutosCubrirBanco);

        if (pago + banco > extraResoluble)
            throw new InvalidOperationException("La suma de pago y banco no puede exceder el total resoluble del tiempo extra según el factor configurado.");

        if (cubiertoBanco > faltante)
            throw new InvalidOperationException("No puedes cubrir con banco más minutos que el faltante neto del día.");

        if (cubiertoBanco > saldoBancoDisponible + banco)
            throw new InvalidOperationException("No hay saldo suficiente en banco para cubrir esa cantidad.");

        var saldoFinalBanco = saldoBancoDisponible + banco - cubiertoBanco;
        if (saldoFinalBanco > topeBancoMinutos)
        {
            var maximoAcumulable = Math.Max(0, topeBancoMinutos - saldoBancoDisponible + cubiertoBanco);
            throw new InvalidOperationException($"La resolución excede el tope de banco de horas ({topeBancoMinutos} min). Máximo acumulable con esta decisión: {maximoAcumulable} min.");
        }

        if (movimientosPrevios.Count > 0)
        {
            db.RrhhBancoHorasMovimientos.RemoveRange(movimientosPrevios);
        }

        asistencia.MinutosExtraAutorizadosPago = pago;
        asistencia.MinutosExtraAutorizadosBanco = banco;
        asistencia.MinutosCubiertosBancoHoras = cubiertoBanco;
        asistencia.ResolucionTiempoExtra = command.Resolucion;
        asistencia.UpdatedAt = DateTime.UtcNow;
        asistencia.UpdatedBy = command.UsuarioActual;

        if (banco > 0)
        {
            db.RrhhBancoHorasMovimientos.Add(new RrhhBancoHorasMovimiento
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresaId,
                EmpleadoId = empleadoId,
                Fecha = asistencia.Fecha,
                TipoMovimiento = TipoMovimientoBancoHorasRrhh.GeneradoPorHorasExtra,
                Horas = banco / 60m,
                ReferenciaTipo = referenciaExtraBanco,
                Observaciones = string.IsNullOrWhiteSpace(command.Observaciones) ? "Generado desde asistencias." : command.Observaciones.Trim(),
                EsAutomatico = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = command.UsuarioActual,
                IsActive = true
            });
        }

        if (cubiertoBanco > 0)
        {
            db.RrhhBancoHorasMovimientos.Add(new RrhhBancoHorasMovimiento
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresaId,
                EmpleadoId = empleadoId,
                Fecha = asistencia.Fecha,
                TipoMovimiento = TipoMovimientoBancoHorasRrhh.Consumo,
                Horas = -(cubiertoBanco / 60m),
                ReferenciaTipo = referenciaCoberturaBanco,
                Observaciones = string.IsNullOrWhiteSpace(command.Observaciones) ? "Consumo por jornada menor a la planeada." : command.Observaciones.Trim(),
                EsAutomatico = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = command.UsuarioActual,
                IsActive = true
            });
        }

        return new RrhhTiempoExtraResolutionResult
        {
            Asistencia = asistencia,
            SaldoBancoActualMinutos = saldoFinalBanco,
            TopeBancoMinutos = topeBancoMinutos,
            FactorTiempoExtra = factorTiempoExtra,
            BancoHorasHabilitado = bancoHorasHabilitado,
            FactorAcumulacionBancoHoras = factorAcumulacionBanco,
            MinutosPagoAplicados = pago,
            MinutosBancoAplicados = banco,
            MinutosCubiertosBancoAplicados = cubiertoBanco,
            BitacoraDetalle = $"empleado={asistencia.EmpleadoId};fecha={asistencia.Fecha:yyyy-MM-dd};resolucion={command.Resolucion};pago={pago};banco={banco};coberturaBanco={cubiertoBanco};obs={command.Observaciones}"
        };
    }

    public async Task<int> AplicarPermisoConGoceBancoHorasAsync(CrmDbContext db, RrhhPermisoBancoHorasCommand command, CancellationToken cancellationToken = default)
    {
        var referencia = ConstruirReferenciaPermisoBanco(command.AusenciaId);
        var movimientosPrevios = await db.RrhhBancoHorasMovimientos
            .Where(m => m.EmpresaId == command.EmpresaId
                && m.EmpleadoId == command.EmpleadoId
                && m.IsActive
                && m.ReferenciaTipo == referencia)
            .ToListAsync(cancellationToken);

        var contextoEmpleado = await ObtenerContextoEmpleadoAsync(db, command.EmpresaId, command.EmpleadoId, cancellationToken);
        var saldoActualMinutos = contextoEmpleado.SaldoBancoHorasMinutos;
        var netoPrevioMinutos = (int)Math.Round(movimientosPrevios.Sum(m => m.Horas) * 60m, MidpointRounding.AwayFromZero);
        var saldoDisponibleMinutos = saldoActualMinutos - netoPrevioMinutos;
        var bancoHorasHabilitado = contextoEmpleado.Configuracion.BancoHorasHabilitado;
        if (!bancoHorasHabilitado)
        {
            throw new InvalidOperationException("El banco de horas no está habilitado para esta empresa.");
        }

        var minutosPermiso = (int)Math.Round(Math.Max(0m, command.HorasPermiso) * 60m, MidpointRounding.AwayFromZero);
        if (minutosPermiso <= 0)
        {
            if (movimientosPrevios.Count > 0)
            {
                db.RrhhBancoHorasMovimientos.RemoveRange(movimientosPrevios);
            }

            return saldoDisponibleMinutos;
        }

        if (movimientosPrevios.Count > 0)
        {
            db.RrhhBancoHorasMovimientos.RemoveRange(movimientosPrevios);
        }

        db.RrhhBancoHorasMovimientos.Add(new RrhhBancoHorasMovimiento
        {
            Id = Guid.NewGuid(),
            EmpresaId = command.EmpresaId,
            EmpleadoId = command.EmpleadoId,
            Fecha = command.Fecha,
            TipoMovimiento = TipoMovimientoBancoHorasRrhh.Consumo,
            Horas = -(minutosPermiso / 60m),
            ReferenciaTipo = referencia,
            Observaciones = string.IsNullOrWhiteSpace(command.Observaciones) ? "Consumo por permiso parcial con goce." : command.Observaciones.Trim(),
            EsAutomatico = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = command.UsuarioActual,
            IsActive = true
        });

        return saldoDisponibleMinutos - minutosPermiso;
    }

    public async Task<int> RemoverPermisoBancoHorasAsync(CrmDbContext db, Guid empresaId, Guid empleadoId, Guid ausenciaId, CancellationToken cancellationToken = default)
    {
        var referencia = ConstruirReferenciaPermisoBanco(ausenciaId);
        var movimientos = await db.RrhhBancoHorasMovimientos
            .Where(m => m.EmpresaId == empresaId
                && m.EmpleadoId == empleadoId
                && m.IsActive
                && m.ReferenciaTipo == referencia)
            .ToListAsync(cancellationToken);

        if (movimientos.Count > 0)
        {
            db.RrhhBancoHorasMovimientos.RemoveRange(movimientos);
        }

        var saldoActualMinutos = await ObtenerSaldoBancoHorasAsync(db, empresaId, empleadoId, cancellationToken);
        var minutosRemovidos = (int)Math.Round(movimientos.Sum(m => m.Horas) * 60m, MidpointRounding.AwayFromZero);
        return saldoActualMinutos - minutosRemovidos;
    }

    public async Task<int> AplicarCompensacionPermisoBancoHorasAsync(CrmDbContext db, RrhhCompensacionPermisoBancoHorasCommand command, CancellationToken cancellationToken = default)
    {
        var referencia = ConstruirReferenciaCompensacionPermisoBanco(command.EmpleadoId, command.Fecha);
        var movimientosPrevios = await db.RrhhBancoHorasMovimientos
            .Where(m => m.EmpresaId == command.EmpresaId
                && m.EmpleadoId == command.EmpleadoId
                && m.IsActive
                && m.ReferenciaTipo == referencia)
            .ToListAsync(cancellationToken);

        var contextoEmpleado = await ObtenerContextoEmpleadoAsync(db, command.EmpresaId, command.EmpleadoId, cancellationToken);
        var saldoActualMinutos = contextoEmpleado.SaldoBancoHorasMinutos;
        var netoPrevioMinutos = (int)Math.Round(movimientosPrevios.Sum(m => m.Horas) * 60m, MidpointRounding.AwayFromZero);
        var saldoDisponibleMinutos = saldoActualMinutos - netoPrevioMinutos;
        var bancoHorasHabilitado = contextoEmpleado.Configuracion.BancoHorasHabilitado;
        if (!bancoHorasHabilitado)
        {
            throw new InvalidOperationException("El banco de horas no está habilitado para esta empresa.");
        }

        var minutosCompensados = Math.Max(0, command.MinutosCompensados);
        if (movimientosPrevios.Count > 0)
        {
            db.RrhhBancoHorasMovimientos.RemoveRange(movimientosPrevios);
        }

        if (minutosCompensados <= 0)
        {
            return saldoDisponibleMinutos;
        }

        db.RrhhBancoHorasMovimientos.Add(new RrhhBancoHorasMovimiento
        {
            Id = Guid.NewGuid(),
            EmpresaId = command.EmpresaId,
            EmpleadoId = command.EmpleadoId,
            Fecha = command.Fecha,
            TipoMovimiento = TipoMovimientoBancoHorasRrhh.GeneradoPorHorasExtra,
            Horas = minutosCompensados / 60m,
            ReferenciaTipo = referencia,
            Observaciones = string.IsNullOrWhiteSpace(command.Observaciones) ? "Compensación aprobada de permiso desde asistencias." : command.Observaciones.Trim(),
            EsAutomatico = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = command.UsuarioActual,
            IsActive = true
        });

        return saldoDisponibleMinutos + minutosCompensados;
    }

    public async Task<int> RemoverCompensacionPermisoBancoHorasAsync(CrmDbContext db, Guid empresaId, Guid empleadoId, DateOnly fecha, CancellationToken cancellationToken = default)
    {
        var referencia = ConstruirReferenciaCompensacionPermisoBanco(empleadoId, fecha);
        var movimientos = await db.RrhhBancoHorasMovimientos
            .Where(m => m.EmpresaId == empresaId
                && m.EmpleadoId == empleadoId
                && m.IsActive
                && m.ReferenciaTipo == referencia)
            .ToListAsync(cancellationToken);

        if (movimientos.Count > 0)
        {
            db.RrhhBancoHorasMovimientos.RemoveRange(movimientos);
        }

        var saldoActualMinutos = await ObtenerSaldoBancoHorasAsync(db, empresaId, empleadoId, cancellationToken);
        var minutosRemovidos = (int)Math.Round(movimientos.Sum(m => m.Horas) * 60m, MidpointRounding.AwayFromZero);
        return saldoActualMinutos - minutosRemovidos;
    }

    private static string ConstruirReferenciaPermisoBanco(Guid ausenciaId)
        => $"{ReferenciaPermisoBancoPrefix}:{ausenciaId:N}";

    private static string ConstruirReferenciaCompensacionPermisoBanco(Guid empleadoId, DateOnly fecha)
        => $"{ReferenciaCompensacionPermisoBancoPrefix}:{empleadoId:N}:{fecha:yyyyMMdd}";

    private static decimal ObtenerDecimal(IReadOnlyDictionary<string, string> configuraciones, string clave, decimal valorDefault)
        => configuraciones.TryGetValue(clave, out var valor)
            && decimal.TryParse(valor, NumberStyles.Any, CultureInfo.InvariantCulture, out var resultado)
                ? resultado
                : valorDefault;

    private static bool ObtenerBooleano(IReadOnlyDictionary<string, string> configuraciones, string clave, bool valorDefault)
        => configuraciones.TryGetValue(clave, out var valor) && bool.TryParse(valor, out var resultado)
            ? resultado
            : valorDefault;

    private static int ConvertirHorasAMinutos(decimal horas)
        => (int)Math.Round(Math.Max(0m, horas) * 60m, MidpointRounding.AwayFromZero);
}
