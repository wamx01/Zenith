using System.Text.Json;

namespace MundoVs.Core.Entities;

public class NominaConfiguracion
{
    public decimal FactorHoraExtra { get; set; } = 2m;
    public decimal FactorHoraExtraTriple { get; set; } = 3m;
    public int MinutosMinimosTiempoExtra { get; set; } = 30;
    public decimal FactorFestivoTrabajado { get; set; } = 2m;
    public decimal FactorDescansoTrabajado { get; set; } = 2m;
    public bool BancoHorasHabilitado { get; set; }
    public decimal BancoHorasFactorAcumulacion { get; set; } = 1m;
    public decimal BancoHorasTopeHoras { get; set; } = 40m;
    public int DiasBaseSemanal { get; set; } = 7;
    public int DiasBaseQuincenal { get; set; } = 15;
    public int DiasBaseMensual { get; set; } = 30;
    public int HorasBaseSemanal { get; set; } = 48;
    public int HorasBaseQuincenal { get; set; } = 96;
    public int HorasBaseMensual { get; set; } = 208;
    public decimal UmaDiaria { get; set; } = 113.14m;
    public decimal SalarioMinimoGeneral { get; set; } = 278.80m;
    public decimal SalarioMinimoFrontera { get; set; } = 419.88m;
    public decimal TasaImssObrera { get; set; } = 0.025m;
    public decimal TasaImssPatronal { get; set; } = 0.18m;
    public decimal PrimaRiesgoTrabajo { get; set; } = 0.005m;
    public decimal PrimaVacacionalMinima { get; set; } = 0.25m;
    public int DiasAguinaldoMinimo { get; set; } = 15;
    public decimal TopeSbcEnUma { get; set; } = 25m;
    public bool RetencionIsrHabilitada { get; set; } = true;
    public string TablaVacacionesJson { get; set; } = TablaVacacionesDefaultJson;
    public string ReglasPrenominaJson { get; set; } = ReglasPrenominaDefaultJson;
    public string TablaIsrJson { get; set; } = TablaIsrDefaultJson;
    public string TablaSubsidioJson { get; set; } = TablaSubsidioDefaultJson;

    public int ObtenerDiasBase(PeriodicidadPago periodicidadPago) => periodicidadPago switch
    {
        PeriodicidadPago.Quincenal => DiasBaseQuincenal,
        PeriodicidadPago.Mensual => DiasBaseMensual,
        _ => DiasBaseSemanal
    };

    public int ObtenerHorasBase(PeriodicidadPago periodicidadPago) => periodicidadPago switch
    {
        PeriodicidadPago.Quincenal => HorasBaseQuincenal,
        PeriodicidadPago.Mensual => HorasBaseMensual,
        _ => HorasBaseSemanal
    };

    public decimal ObtenerSalarioMinimo(bool zonaFronteriza = false)
        => zonaFronteriza ? SalarioMinimoFrontera : SalarioMinimoGeneral;

    public decimal ObtenerFactorFestivoTrabajado(decimal? factorEspecifico = null)
    {
        var factor = factorEspecifico.HasValue && factorEspecifico.Value > 0m
            ? factorEspecifico.Value
            : FactorFestivoTrabajado;

        return factor > 0m ? factor : 2m;
    }

    public int ObtenerDiasVacacionesPorAntiguedad(int aniosServicio)
    {
        if (aniosServicio <= 0)
        {
            return 0;
        }

        var tabla = ObtenerTablaVacaciones();
        if (tabla.TryGetValue(aniosServicio, out var exacto))
        {
            return exacto;
        }

        var ultimo = tabla
            .Where(kvp => kvp.Key <= aniosServicio)
            .OrderByDescending(kvp => kvp.Key)
            .FirstOrDefault();

        return ultimo.Value > 0 ? ultimo.Value : ObtenerDiasVacacionesDefault(aniosServicio);
    }

    public Dictionary<int, int> ObtenerTablaVacaciones()
    {
        try
        {
            var tabla = JsonSerializer.Deserialize<Dictionary<int, int>>(TablaVacacionesJson);
            if (tabla != null && tabla.Count > 0)
            {
                return tabla;
            }
        }
        catch
        {
        }

        return TablaVacacionesDefault();
    }

    private static int ObtenerDiasVacacionesDefault(int aniosServicio)
    {
        if (aniosServicio <= 0)
        {
            return 0;
        }

        if (aniosServicio == 1) return 12;
        if (aniosServicio == 2) return 14;
        if (aniosServicio == 3) return 16;
        if (aniosServicio == 4) return 18;

        var dias = 20;
        var bloque = (aniosServicio - 5) / 5;
        return dias + (Math.Max(0, bloque) * 2);
    }

    public static Dictionary<int, int> TablaVacacionesDefault() => new()
    {
        [1] = 12,
        [2] = 14,
        [3] = 16,
        [4] = 18,
        [5] = 20,
        [6] = 20,
        [7] = 20,
        [8] = 20,
        [9] = 20,
        [10] = 22,
        [11] = 22,
        [12] = 22,
        [13] = 22,
        [14] = 22,
        [15] = 24,
        [16] = 24,
        [17] = 24,
        [18] = 24,
        [19] = 24,
        [20] = 26,
        [21] = 26,
        [22] = 26,
        [23] = 26,
        [24] = 26
    };

    public static string TablaVacacionesDefaultJson => JsonSerializer.Serialize(TablaVacacionesDefault());

    public static ReglasPrenominaConfiguracion ReglasPrenominaDefault() => new();

    public static string ReglasPrenominaDefaultJson => JsonSerializer.Serialize(ReglasPrenominaDefault());

    // Tarifa ISR mensual LISR art. 96 (default repositorio 2026). Debe revisarse cada ejercicio.
    public static IReadOnlyList<IsrTramo> TablaIsrDefault() => new[]
    {
        new IsrTramo(0.01m,       844.59m,        0.00m,     0.0192m),
        new IsrTramo(844.60m,     7168.51m,       16.22m,    0.0640m),
        new IsrTramo(7168.52m,    12598.02m,      420.95m,   0.1088m),
        new IsrTramo(12598.03m,   14644.64m,      1011.68m,  0.1600m),
        new IsrTramo(14644.65m,   17533.64m,      1339.14m,  0.1792m),
        new IsrTramo(17533.65m,   35362.83m,      1856.84m,  0.2136m),
        new IsrTramo(35362.84m,   55736.68m,      5665.16m,  0.2352m),
        new IsrTramo(55736.69m,   106410.50m,     10457.09m, 0.3000m),
        new IsrTramo(106410.51m,  141880.66m,     25659.23m, 0.3200m),
        new IsrTramo(141880.67m,  425641.99m,     37009.69m, 0.3400m),
        new IsrTramo(425642.00m,  decimal.MaxValue,133488.54m,0.3500m)
    };

    public static string TablaIsrDefaultJson => JsonSerializer.Serialize(TablaIsrDefault());

    // Subsidio para el empleo mensual (vigente 2024). Si ingreso mensual <= límite, aplica monto fijo.
    public static IReadOnlyList<SubsidioTramo> TablaSubsidioDefault() => new[]
    {
        new SubsidioTramo(0.01m,    1768.96m,         407.02m),
        new SubsidioTramo(1768.97m, decimal.MaxValue, 0m)
    };

    public static string TablaSubsidioDefaultJson => JsonSerializer.Serialize(TablaSubsidioDefault());

    public IReadOnlyList<IsrTramo> ObtenerTablaIsr()
    {
        try
        {
            var tabla = JsonSerializer.Deserialize<List<IsrTramo>>(TablaIsrJson);
            if (tabla != null && tabla.Count > 0)
                return tabla;
        }
        catch { }
        return TablaIsrDefault();
    }

    public IReadOnlyList<SubsidioTramo> ObtenerTablaSubsidio()
    {
        try
        {
            var tabla = JsonSerializer.Deserialize<List<SubsidioTramo>>(TablaSubsidioJson);
            if (tabla != null && tabla.Count > 0)
                return tabla;
        }
        catch { }
        return TablaSubsidioDefault();
    }

    public ReglasPrenominaConfiguracion ObtenerReglasPrenomina()
    {
        try
        {
            var reglas = JsonSerializer.Deserialize<ReglasPrenominaConfiguracion>(ReglasPrenominaJson);
            if (reglas != null)
                return reglas;
        }
        catch { }

        return ReglasPrenominaDefault();
    }
}

public record IsrTramo(decimal LimiteInferior, decimal LimiteSuperior, decimal CuotaFija, decimal TasaExcedente);
public record SubsidioTramo(decimal LimiteInferior, decimal LimiteSuperior, decimal Subsidio);

public sealed class ReglasPrenominaConfiguracion
{
    public bool PermitirHorasExtraManual { get; set; } = true;
    public bool ValidarDiasPagadosContraPeriodo { get; set; } = true;
    public bool RequierePrenominaCerradaParaNomina { get; set; } = true;
}

public enum PeriodicidadPago
{
    Semanal = 1,
    Quincenal = 2,
    Mensual = 3
}

public static class ClavesConfiguracionNomina
{
    public const string FactorHoraExtra = "Nomina:FactorHoraExtra";
    public const string FactorHoraExtraTriple = "Nomina:FactorHoraExtra:Triple";
    public const string MinutosMinimosTiempoExtra = "Nomina:FactorHoraExtra:MinutosMinimos";
    public const string FactorFestivoTrabajado = "Nomina:FactorFestivoTrabajado";
    public const string FactorDescansoTrabajado = "Nomina:FactorDescansoTrabajado";
    public const string BancoHorasHabilitado = "Nomina:BancoHoras:Habilitado";
    public const string BancoHorasFactorAcumulacion = "Nomina:BancoHoras:FactorAcumulacion";
    public const string BancoHorasTopeHoras = "Nomina:BancoHoras:TopeHoras";
    public const string DiasBaseSemanal = "Nomina:DiasBase:Semanal";
    public const string DiasBaseQuincenal = "Nomina:DiasBase:Quincenal";
    public const string DiasBaseMensual = "Nomina:DiasBase:Mensual";
    public const string HorasBaseSemanal = "Nomina:HorasBase:Semanal";
    public const string HorasBaseQuincenal = "Nomina:HorasBase:Quincenal";
    public const string HorasBaseMensual = "Nomina:HorasBase:Mensual";
    public const string UmaDiaria = "Nomina:UMA:Diaria";
    public const string SalarioMinimoGeneral = "Nomina:SalarioMinimo:General";
    public const string SalarioMinimoFrontera = "Nomina:SalarioMinimo:Frontera";
    public const string TasaImssObrera = "Nomina:IMSS:TasaObrera";
    public const string TasaImssPatronal = "Nomina:IMSS:TasaPatronal";
    public const string PrimaRiesgoTrabajo = "Nomina:IMSS:PrimaRiesgoTrabajo";
    public const string PrimaVacacionalMinima = "Nomina:PrimaVacacional:Minima";
    public const string DiasAguinaldoMinimo = "Nomina:Aguinaldo:DiasMinimos";
    public const string TopeSbcEnUma = "Nomina:IMSS:TopeSbcEnUma";
    public const string TablaVacacionesJson = "Nomina:Vacaciones:TablaJson";
    public const string ReglasPrenominaJson = "Nomina:Prenomina:ReglasJson";
    public const string TablaIsrJson = "Nomina:ISR:TablaJson";
    public const string TablaSubsidioJson = "Nomina:SubsidioEmpleo:TablaJson";
    public const string RetencionIsrHabilitada = "Nomina:ISR:RetencionHabilitada";
}
