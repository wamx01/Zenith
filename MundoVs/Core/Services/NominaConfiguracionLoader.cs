using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Services;

public static class NominaConfiguracionLoader
{
    public static IQueryable<NominaConfiguracionGlobal> OrderByVigencia(IQueryable<NominaConfiguracionGlobal> query)
        => query
            .Where(c => c.IsActive)
            .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
            .ThenByDescending(c => c.CreatedAt);

    public static async Task<NominaConfiguracion> LoadAsync(CrmDbContext db)
    {
        var config = await db.AppConfigs
            .AsNoTracking()
            .ToDictionaryAsync(c => c.Clave, c => c.Valor ?? string.Empty);

        var global = await LoadGlobalAsync(db);
        return Build(config, global);
    }

    public static async Task<NominaConfiguracion> LoadAsync(CrmDbContext db, Guid empresaId)
    {
        var config = await db.AppConfigs
            .AsNoTracking()
            .Where(c => c.EmpresaId == empresaId)
            .ToDictionaryAsync(c => c.Clave, c => c.Valor ?? string.Empty);

        var global = await LoadGlobalAsync(db);
        return Build(config, global);
    }

    public static Task<NominaConfiguracionGlobal?> LoadGlobalAsync(CrmDbContext db)
        => OrderByVigencia(db.NominaConfiguracionesGlobales
            .AsNoTracking())
            .FirstOrDefaultAsync();

    public static NominaConfiguracion Build(IReadOnlyDictionary<string, string> config, NominaConfiguracionGlobal? global)
    {
        var umaDiariaEmpresa = ObtenerDecimal(config, ClavesConfiguracionNomina.UmaDiaria, 113.14m);
        var salarioMinimoGeneralEmpresa = ObtenerDecimal(config, ClavesConfiguracionNomina.SalarioMinimoGeneral, 278.80m);
        var salarioMinimoFronteraEmpresa = ObtenerDecimal(config, ClavesConfiguracionNomina.SalarioMinimoFrontera, 419.88m);
        var tablaIsrEmpresa = ObtenerTexto(config, ClavesConfiguracionNomina.TablaIsrJson, NominaConfiguracion.TablaIsrDefaultJson);
        var tablaSubsidioEmpresa = ObtenerTexto(config, ClavesConfiguracionNomina.TablaSubsidioJson, NominaConfiguracion.TablaSubsidioDefaultJson);

        return new NominaConfiguracion
        {
            FactorHoraExtra = ObtenerDecimal(config, ClavesConfiguracionNomina.FactorHoraExtra, 2m),
            FactorHoraExtraTriple = ObtenerDecimal(config, ClavesConfiguracionNomina.FactorHoraExtraTriple, 3m),
            MinutosMinimosTiempoExtra = Math.Max(0, ObtenerEntero(config, ClavesConfiguracionNomina.MinutosMinimosTiempoExtra, 30)),
            FactorFestivoTrabajado = ObtenerDecimal(config, ClavesConfiguracionNomina.FactorFestivoTrabajado, 2m),
            FactorDescansoTrabajado = ObtenerDecimal(config, ClavesConfiguracionNomina.FactorDescansoTrabajado, 2m),
            BancoHorasHabilitado = ObtenerBooleano(config, ClavesConfiguracionNomina.BancoHorasHabilitado, false),
            BancoHorasFactorAcumulacion = ObtenerDecimal(config, ClavesConfiguracionNomina.BancoHorasFactorAcumulacion, 1m),
            BancoHorasTopeHoras = ObtenerDecimal(config, ClavesConfiguracionNomina.BancoHorasTopeHoras, 40m),
            DiasBaseSemanal = ObtenerEntero(config, ClavesConfiguracionNomina.DiasBaseSemanal, 7),
            DiasBaseQuincenal = ObtenerEntero(config, ClavesConfiguracionNomina.DiasBaseQuincenal, 15),
            DiasBaseMensual = ObtenerEntero(config, ClavesConfiguracionNomina.DiasBaseMensual, 30),
            HorasBaseSemanal = ObtenerEntero(config, ClavesConfiguracionNomina.HorasBaseSemanal, 48),
            HorasBaseQuincenal = ObtenerEntero(config, ClavesConfiguracionNomina.HorasBaseQuincenal, 96),
            HorasBaseMensual = ObtenerEntero(config, ClavesConfiguracionNomina.HorasBaseMensual, 208),
            UmaDiaria = global?.UmaDiaria > 0m ? global.UmaDiaria : umaDiariaEmpresa,
            SalarioMinimoGeneral = global?.SalarioMinimoGeneral > 0m ? global.SalarioMinimoGeneral : salarioMinimoGeneralEmpresa,
            SalarioMinimoFrontera = global?.SalarioMinimoFrontera > 0m ? global.SalarioMinimoFrontera : salarioMinimoFronteraEmpresa,
            TasaImssObrera = ObtenerDecimal(config, ClavesConfiguracionNomina.TasaImssObrera, 0.025m),
            TasaImssPatronal = ObtenerDecimal(config, ClavesConfiguracionNomina.TasaImssPatronal, 0.18m),
            PrimaRiesgoTrabajo = ObtenerDecimal(config, ClavesConfiguracionNomina.PrimaRiesgoTrabajo, 0.005m),
            PrimaVacacionalMinima = ObtenerDecimal(config, ClavesConfiguracionNomina.PrimaVacacionalMinima, 0.25m),
            DiasAguinaldoMinimo = ObtenerEntero(config, ClavesConfiguracionNomina.DiasAguinaldoMinimo, 15),
            TopeSbcEnUma = ObtenerDecimal(config, ClavesConfiguracionNomina.TopeSbcEnUma, 25m),
            RetencionIsrHabilitada = ObtenerBooleano(config, ClavesConfiguracionNomina.RetencionIsrHabilitada, true),
            TablaVacacionesJson = ObtenerTexto(config, ClavesConfiguracionNomina.TablaVacacionesJson, NominaConfiguracion.TablaVacacionesDefaultJson),
            ReglasPrenominaJson = ObtenerTexto(config, ClavesConfiguracionNomina.ReglasPrenominaJson, NominaConfiguracion.ReglasPrenominaDefaultJson),
            TablaIsrJson = string.IsNullOrWhiteSpace(global?.TablaIsrJson) ? tablaIsrEmpresa : global.TablaIsrJson,
            TablaSubsidioJson = string.IsNullOrWhiteSpace(global?.TablaSubsidioJson) ? tablaSubsidioEmpresa : global.TablaSubsidioJson
        };
    }

    private static int ObtenerEntero(IReadOnlyDictionary<string, string> config, string clave, int valorDefault)
        => config.TryGetValue(clave, out var valor) && int.TryParse(valor, out var resultado)
            ? resultado
            : valorDefault;

    private static decimal ObtenerDecimal(IReadOnlyDictionary<string, string> config, string clave, decimal valorDefault)
        => config.TryGetValue(clave, out var valor)
            && decimal.TryParse(valor, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var resultado)
                ? resultado
                : valorDefault;

    private static bool ObtenerBooleano(IReadOnlyDictionary<string, string> config, string clave, bool valorDefault)
        => config.TryGetValue(clave, out var valor) && bool.TryParse(valor, out var resultado)
            ? resultado
            : valorDefault;

    private static string ObtenerTexto(IReadOnlyDictionary<string, string> config, string clave, string valorDefault)
        => config.TryGetValue(clave, out var valor) && !string.IsNullOrWhiteSpace(valor)
            ? valor
            : valorDefault;
}
