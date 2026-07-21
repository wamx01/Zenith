using MundoVs.Core.Entities;
using MundoVs.Core.Services;

namespace MundoVs.Tests;

public sealed class NominaPeriodoHelperTests
{
    [Fact]
    public void ObtenerPeriodo_Semanal_RespetaDiaCorteConfigurado()
    {
        var corte = new NominaCorteRrhh
        {
            PeriodicidadPago = PeriodicidadPago.Semanal,
            DiaCorteSemana = DayOfWeek.Sunday
        };

        var periodo = NominaPeriodoHelper.ObtenerPeriodo(PeriodicidadPago.Semanal, new DateTime(2026, 4, 23), corte);

        Assert.Equal(new DateTime(2026, 4, 13), periodo.Inicio);
        Assert.Equal(new DateTime(2026, 4, 19), periodo.Fin);
        Assert.Equal(PeriodicidadPago.Semanal, periodo.PeriodicidadPago);
    }

    [Fact]
    public void ObtenerPeriodo_Quincenal_ParteMesPorDiaCorte()
    {
        var corte = new NominaCorteRrhh
        {
            PeriodicidadPago = PeriodicidadPago.Quincenal,
            DiaCorteMes = 15
        };

        var periodo = NominaPeriodoHelper.ObtenerPeriodo(PeriodicidadPago.Quincenal, new DateTime(2026, 4, 23), corte);

        Assert.Equal(new DateTime(2026, 4, 16), periodo.Inicio);
        Assert.Equal(new DateTime(2026, 4, 30), periodo.Fin);
        Assert.Equal(8, periodo.NumeroPeriodo);
    }

    [Fact]
    public void ObtenerPeriodo_Mensual_UsaMesCalendarioCuandoCorteEs31()
    {
        var corte = new NominaCorteRrhh
        {
            PeriodicidadPago = PeriodicidadPago.Mensual,
            DiaCorteMes = 31
        };

        var periodo = NominaPeriodoHelper.ObtenerPeriodo(PeriodicidadPago.Mensual, new DateTime(2026, 4, 23), corte);

        Assert.Equal(new DateTime(2026, 4, 1), periodo.Inicio);
        Assert.Equal(new DateTime(2026, 4, 30), periodo.Fin);
        Assert.Equal(4, periodo.NumeroPeriodo);
        Assert.Contains("abril", periodo.Periodo, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ObtenerPeriodoContenedor_Semanal_DevuelvePeriodoQueContieneLaFecha()
    {
        var corte = new NominaCorteRrhh
        {
            PeriodicidadPago = PeriodicidadPago.Semanal,
            DiaCorteSemana = DayOfWeek.Tuesday
        };

        // 2026-07-18 es sábado; el periodo Wed–Tue que lo contiene es 2026-07-15..2026-07-21.
        var periodo = NominaPeriodoHelper.ObtenerPeriodoContenedor(PeriodicidadPago.Semanal, new DateTime(2026, 7, 18), corte);

        Assert.Equal(new DateTime(2026, 7, 15), periodo.Inicio);
        Assert.Equal(new DateTime(2026, 7, 21), periodo.Fin);
    }

    [Fact]
    public void ObtenerPeriodoContenedor_Semanal_EnDiaDeCorte_IncluyeEseDia()
    {
        var corte = new NominaCorteRrhh
        {
            PeriodicidadPago = PeriodicidadPago.Semanal,
            DiaCorteSemana = DayOfWeek.Sunday
        };

        // 2026-04-26 es domingo (día de corte) → [2026-04-20, 2026-04-26] (lo contiene como último día).
        var periodo = NominaPeriodoHelper.ObtenerPeriodoContenedor(PeriodicidadPago.Semanal, new DateTime(2026, 4, 26), corte);

        Assert.Equal(new DateTime(2026, 4, 20), periodo.Inicio);
        Assert.Equal(new DateTime(2026, 4, 26), periodo.Fin);
    }

    [Fact]
    public void ObtenerPeriodoContenedor_QuincenalYMensual_CoincideConObtenerPeriodo()
    {
        var refDate = new DateTime(2026, 4, 23);

        Assert.Equal(
            NominaPeriodoHelper.ObtenerPeriodo(PeriodicidadPago.Quincenal, refDate, new NominaCorteRrhh { DiaCorteMes = 15 }).Fin,
            NominaPeriodoHelper.ObtenerPeriodoContenedor(PeriodicidadPago.Quincenal, refDate, new NominaCorteRrhh { DiaCorteMes = 15 }).Fin);

        Assert.Equal(
            NominaPeriodoHelper.ObtenerPeriodo(PeriodicidadPago.Mensual, refDate, new NominaCorteRrhh { DiaCorteMes = 31 }).Fin,
            NominaPeriodoHelper.ObtenerPeriodoContenedor(PeriodicidadPago.Mensual, refDate, new NominaCorteRrhh { DiaCorteMes = 31 }).Fin);
    }
}
