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
}
