using MundoVs.Core.Entities;
using MundoVs.Core.Services;

namespace MundoVs.Tests;

public sealed class NominaVacacionesImssTests
{
    private readonly NominaCalculator _calculator = new();
    private readonly NominaLegalPolicyService _policy = new();

    [Fact]
    public void Calculate_PrimaVacacional_SePagaAlTomarVacaciones_CuandoLaPoliticaEsAlTomarVacaciones()
    {
        var empleado = CrearEmpleado();
        var configuracion = CrearConfiguracion();
        var resultado = _calculator.Calculate(new NominaCalculationInput
        {
            Empleado = empleado,
            Configuracion = configuracion,
            SueldoReferencia = 2500m,
            DiasPagados = 7,
            DiasVacaciones = 2,
            AplicaImss = true,
            AplicaIsr = false,
            AniosServicio = 1
        });

        Assert.Equal(178.57m, resultado.MontoPrimaVacacional);
        Assert.Equal(20.55m, resultado.PrimaVacacionalProvision);
    }

    [Fact]
    public void Calculate_PrimaVacacional_NoSePagaAlTomarVacaciones_CuandoLaPoliticaEsAnual()
    {
        var empleado = CrearEmpleado();
        var configuracion = CrearConfiguracion();
        configuracion.FormaPagoPrimaVacacional = FormaPagoPrimaVacacionalRrhh.Anual;

        var resultado = _calculator.Calculate(new NominaCalculationInput
        {
            Empleado = empleado,
            Configuracion = configuracion,
            SueldoReferencia = 2500m,
            DiasPagados = 7,
            DiasVacaciones = 2,
            AplicaImss = true,
            AplicaIsr = false,
            AniosServicio = 1
        });

        Assert.Equal(0m, resultado.MontoPrimaVacacional);
        Assert.Equal(20.55m, resultado.PrimaVacacionalProvision);
    }

    [Fact]
    public void Calculate_CuandoAplicaImss_CalculaParteObreraYPatronal()
    {
        var empleado = CrearEmpleado();
        var configuracion = CrearConfiguracion();

        var resultado = _calculator.Calculate(new NominaCalculationInput
        {
            Empleado = empleado,
            Configuracion = configuracion,
            SueldoReferencia = 2500m,
            DiasPagados = 7,
            AplicaImss = true,
            AplicaIsr = false,
            AniosServicio = 1
        });

        Assert.Equal(65.58m, resultado.CuotaImssObrera);
        Assert.Equal(485.31m, resultado.CuotaImssPatronal);
    }

    [Fact]
    public void Calculate_CuandoNoAplicaImss_NoCalculaCuotas()
    {
        var empleado = CrearEmpleado();
        var configuracion = CrearConfiguracion();

        var resultado = _calculator.Calculate(new NominaCalculationInput
        {
            Empleado = empleado,
            Configuracion = configuracion,
            SueldoReferencia = 2500m,
            DiasPagados = 7,
            AplicaImss = false,
            AplicaIsr = false,
            AniosServicio = 1
        });

        Assert.Equal(0m, resultado.CuotaImssObrera);
        Assert.Equal(0m, resultado.CuotaImssPatronal);
    }

    [Fact]
    public void ObtenerCicloVacacional_Aniversario_AntesDelPrimerAnio_QuedaEnAnio0()
    {
        var empleado = CrearEmpleado(new DateTime(2026, 4, 20));
        var configuracion = CrearConfiguracion();
        configuracion.TipoCicloVacacional = TipoCicloVacacionalRrhh.AniversarioContratacion;

        var ciclo = _policy.ObtenerCicloVacacional(empleado, new DateTime(2027, 4, 19), configuracion);

        Assert.False(ciclo.TieneDerechoVacaciones);
        Assert.Equal(0, ciclo.AnioVacacionalReconocido);
    }

    [Fact]
    public void ObtenerCicloVacacional_Aniversario_DespuesDelPrimerAnio_EntraAAnio1()
    {
        var empleado = CrearEmpleado(new DateTime(2026, 4, 20));
        var configuracion = CrearConfiguracion();
        configuracion.TipoCicloVacacional = TipoCicloVacacionalRrhh.AniversarioContratacion;

        var ciclo = _policy.ObtenerCicloVacacional(empleado, new DateTime(2027, 4, 20), configuracion);

        Assert.True(ciclo.TieneDerechoVacaciones);
        Assert.Equal(1, ciclo.AnioVacacionalReconocido);
    }

    [Fact]
    public void ObtenerCicloVacacional_CorteFijo_EmpleadoSinAntiguedadMinima_QuedaEnAnio0()
    {
        var empleado = CrearEmpleado(new DateTime(2026, 4, 20));
        var configuracion = CrearConfiguracion();
        configuracion.TipoCicloVacacional = TipoCicloVacacionalRrhh.CorteFijoAnual;
        configuracion.MesInicioCicloVacacional = 1;
        configuracion.DiaInicioCicloVacacional = 1;
        configuracion.MesesMinimosPrimerAnioVacacional = 12;

        var ciclo = _policy.ObtenerCicloVacacional(empleado, new DateTime(2027, 1, 5), configuracion);

        Assert.False(ciclo.TieneDerechoVacaciones);
        Assert.Equal(0, ciclo.AnioVacacionalReconocido);
    }

    [Fact]
    public void ObtenerCicloVacacional_CorteFijo_EmpleadoConAntiguedadMinima_EntraAAnio1()
    {
        var empleado = CrearEmpleado(new DateTime(2026, 1, 1));
        var configuracion = CrearConfiguracion();
        configuracion.TipoCicloVacacional = TipoCicloVacacionalRrhh.CorteFijoAnual;
        configuracion.MesInicioCicloVacacional = 1;
        configuracion.DiaInicioCicloVacacional = 1;
        configuracion.MesesMinimosPrimerAnioVacacional = 12;

        var ciclo = _policy.ObtenerCicloVacacional(empleado, new DateTime(2027, 1, 1), configuracion);

        Assert.True(ciclo.TieneDerechoVacaciones);
        Assert.Equal(1, ciclo.AnioVacacionalReconocido);
    }

    [Fact]
    public void CalcularDiasVacacionesDisponibles_CorteFijo_SoloDescuentaLasVacacionesDelCicloReconocido()
    {
        var empleado = CrearEmpleado(new DateTime(2024, 1, 1));
        var configuracion = CrearConfiguracion();
        configuracion.TipoCicloVacacional = TipoCicloVacacionalRrhh.CorteFijoAnual;
        configuracion.MesInicioCicloVacacional = 1;
        configuracion.DiaInicioCicloVacacional = 1;
        configuracion.MesesMinimosPrimerAnioVacacional = 12;

        var disponibles = _policy.CalcularDiasVacacionesDisponibles(empleado, new DateTime(2026, 5, 1), 4m, configuracion);

        Assert.Equal(10m, disponibles);
    }

    [Fact]
    public void ObtenerCicloVacacional_CorteFijo_PrimerCicloProporcional_CalculaDiasEquivalentes()
    {
        var empleado = CrearEmpleado(new DateTime(2026, 4, 1));
        var configuracion = CrearConfiguracion();
        configuracion.TipoCicloVacacional = TipoCicloVacacionalRrhh.CorteFijoAnual;
        configuracion.PoliticaPrimerCicloVacacional = PoliticaPrimerCicloVacacionalRrhh.Proporcional;
        configuracion.MesInicioCicloVacacional = 1;
        configuracion.DiaInicioCicloVacacional = 1;
        configuracion.MesesMinimosPrimerAnioVacacional = 6;

        var ciclo = _policy.ObtenerCicloVacacional(empleado, new DateTime(2027, 2, 1), configuracion);

        Assert.True(ciclo.TieneDerechoVacaciones);
        Assert.Equal(1, ciclo.AnioVacacionalReconocido);
        Assert.Equal(9.04m, ciclo.DiasVacacionesEquivalentes);
    }

    [Fact]
    public void Calculate_PrimerCicloProporcional_UsaDiasEquivalentesParaProvisionEImss()
    {
        var empleado = CrearEmpleado(new DateTime(2026, 4, 1));
        var configuracion = CrearConfiguracion();
        configuracion.TipoCicloVacacional = TipoCicloVacacionalRrhh.CorteFijoAnual;
        configuracion.PoliticaPrimerCicloVacacional = PoliticaPrimerCicloVacacionalRrhh.Proporcional;
        configuracion.MesInicioCicloVacacional = 1;
        configuracion.DiaInicioCicloVacacional = 1;
        configuracion.MesesMinimosPrimerAnioVacacional = 6;
        var ciclo = _policy.ObtenerCicloVacacional(empleado, new DateTime(2027, 2, 1), configuracion);

        var resultado = _calculator.Calculate(new NominaCalculationInput
        {
            Empleado = empleado,
            Configuracion = configuracion,
            SueldoReferencia = 2500m,
            DiasPagados = 7,
            DiasVacaciones = 0,
            AplicaImss = true,
            AplicaIsr = false,
            AniosServicio = 1,
            DiasVacacionesAnualesOverride = ciclo.DiasVacacionesEquivalentes
        });

        Assert.Equal(15.48m, resultado.PrimaVacacionalProvision);
        Assert.Equal(65.46m, resultado.CuotaImssObrera);
        Assert.Equal(484.37m, resultado.CuotaImssPatronal);
    }

    private static Empleado CrearEmpleado(DateTime? fechaContratacion = null)
        => new()
        {
            Id = Guid.NewGuid(),
            EmpresaId = Guid.NewGuid(),
            Nombre = "Cesar",
            ApellidoPaterno = "Araiza",
            FechaContratacion = fechaContratacion ?? new DateTime(2025, 4, 20),
            SueldoSemanal = 2500m,
            PeriodicidadPago = PeriodicidadPago.Semanal,
            AplicaImss = true,
            AplicaIsr = false,
            IsActive = true
        };

    private static NominaConfiguracion CrearConfiguracion()
        => new()
        {
            DiasBaseSemanal = 7,
            HorasBaseSemanal = 48,
            PrimaVacacionalMinima = 0.25m,
            PoliticaPrimerCicloVacacional = PoliticaPrimerCicloVacacionalRrhh.Completo,
            DiasAguinaldoMinimo = 15,
            TasaImssObrera = 0.025m,
            TasaImssPatronal = 0.18m,
            PrimaRiesgoTrabajo = 0.005m,
            TopeSbcEnUma = 25m,
            UmaDiaria = 113.14m,
            SalarioMinimoGeneral = 278.80m,
            FormaPagoPrimaVacacional = FormaPagoPrimaVacacionalRrhh.AlTomarVacaciones,
            TipoCicloVacacional = TipoCicloVacacionalRrhh.AniversarioContratacion,
            MesInicioCicloVacacional = 1,
            DiaInicioCicloVacacional = 1,
            MesesMinimosPrimerAnioVacacional = 12,
            TablaVacacionesJson = "{\"1\":12,\"2\":14,\"3\":16,\"4\":18,\"5\":20}"
        };
}
