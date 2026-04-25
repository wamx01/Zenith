using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;

namespace MundoVs.Core.Services;

public class NominaReciboBuilder : INominaReciboBuilder
{
    public NominaReciboResult Build(NominaDetalle detalle)
    {
        var percepciones = BuildPercepciones(detalle);
        var deducciones = BuildDeducciones(detalle);

        return new NominaReciboResult
        {
            Percepciones = percepciones,
            Deducciones = deducciones,
            TotalPercepciones = percepciones.Sum(x => x.Importe),
            TotalDeducciones = deducciones.Sum(x => x.Importe),
            NetoPagar = detalle.TotalPagar
        };
    }

    private static IReadOnlyList<NominaReciboConcepto> BuildPercepciones(NominaDetalle detalle)
    {
        var conceptos = new List<NominaReciboConcepto>();
        AddIfPositive(conceptos, NominaSatCatalogos.Sistema.PercepcionSueldos, "Sueldo base", detalle.SueldoBase);
        AddIfPositive(conceptos, NominaSatCatalogos.Sistema.PercepcionComisiones, "Destajo", detalle.MontoDestajo);

        var bonosEstructurados = detalle.BonosEstructurados
            .Where(b => b.IsActive)
            .SelectMany(b => b.Detalles.Where(d => d.IsActive))
            .OrderBy(d => d.Orden)
            .ThenBy(d => d.BonoRubroRrhh.Nombre)
            .ToList();

        if (bonosEstructurados.Count > 0)
        {
            foreach (var bono in bonosEstructurados)
                AddIfPositive(conceptos, ResolverClavePercepcionBono(bono.BonoRubroRrhh.Clave), $"Bono {bono.BonoRubroRrhh.Nombre}", bono.Importe);
        }
        else
        {
            AddIfPositive(conceptos, NominaSatCatalogos.Sistema.PercepcionOtrosIngresosSalarios, "Bono producción", detalle.MontoBono);
        }

        AddIfPositive(conceptos, NominaSatCatalogos.Sistema.PercepcionDescansoObligatorioLaborado, "Festivo trabajado", detalle.MontoFestivoTrabajado);
        AddIfPositive(conceptos, NominaSatCatalogos.Sistema.PercepcionPrimaDominical, "Prima dominical", detalle.MontoPrimaDominical);
        AddIfPositive(conceptos, NominaSatCatalogos.Sistema.PercepcionPrimaVacacional, "Prima vacacional", detalle.MontoPrimaVacacional);
        AddIfPositive(conceptos, NominaSatCatalogos.Sistema.PercepcionOtrosIngresosSalarios, "Complemento salario mínimo", detalle.ComplementoSalarioMinimo);
        AddIfPositive(conceptos, NominaSatCatalogos.Sistema.PercepcionHorasExtra, ConstruirConceptoHorasExtra(detalle), detalle.MontoHorasExtra);
        AddIfPositive(conceptos, NominaSatCatalogos.Sistema.PercepcionOtrosIngresosSalarios, "Subsidio para el empleo", detalle.SubsidioEmpleo);

        var percepcionesManuales = detalle.PercepcionesManuales
            .Where(p => p.IsActive)
            .OrderBy(p => p.TipoPercepcion.Orden)
            .ThenBy(p => p.TipoPercepcion.Nombre)
            .ThenBy(p => p.Descripcion)
            .ToList();

        if (percepcionesManuales.Count > 0)
        {
            foreach (var percepcion in percepcionesManuales)
            {
                var etiqueta = string.IsNullOrWhiteSpace(percepcion.Descripcion)
                    ? percepcion.TipoPercepcion.Nombre
                    : $"{percepcion.TipoPercepcion.Nombre} - {percepcion.Descripcion}";
                var codigo = string.IsNullOrWhiteSpace(percepcion.TipoPercepcion.Clave)
                    ? NominaSatCatalogos.Sistema.PercepcionOtrosIngresosSalarios
                    : percepcion.TipoPercepcion.Clave;
                AddIfPositive(conceptos, codigo, etiqueta, percepcion.Importe);
            }
        }
        else
        {
            AddIfPositive(conceptos, NominaSatCatalogos.Sistema.PercepcionOtrosIngresosSalarios, "Percepciones manuales", detalle.Bonos);
        }

        return conceptos;
    }

    private static IReadOnlyList<NominaReciboConcepto> BuildDeducciones(NominaDetalle detalle)
    {
        var conceptos = new List<NominaReciboConcepto>();
        AddIfPositive(conceptos, NominaSatCatalogos.Sistema.DeduccionIsr, "ISR retenido", detalle.RetencionIsr);
        AddIfPositive(conceptos, NominaSatCatalogos.Sistema.DeduccionSeguridadSocial, "IMSS obrero", detalle.CuotaImssObrera);
        AddIfPositive(conceptos, NominaSatCatalogos.Sistema.DeduccionCreditoInfonavit, "Infonavit", detalle.MontoInfonavit);
        AddIfPositive(conceptos, NominaSatCatalogos.Sistema.DeduccionAusentismo, "Descuento por retardos / minutos", detalle.MontoDescuentoMinutos);

        var deduccionesEstructuradas = detalle.DeduccionesEstructuradas
            .Where(d => d.IsActive)
            .OrderBy(d => d.TipoDeduccion.Orden)
            .ThenBy(d => d.TipoDeduccion.Nombre)
            .ThenBy(d => d.Descripcion)
            .ToList();

        if (deduccionesEstructuradas.Count > 0)
        {
            foreach (var deduccion in deduccionesEstructuradas)
            {
                var etiqueta = string.IsNullOrWhiteSpace(deduccion.Descripcion)
                    ? deduccion.TipoDeduccion.Nombre
                    : $"{deduccion.TipoDeduccion.Nombre} - {deduccion.Descripcion}";
                var codigo = string.IsNullOrWhiteSpace(deduccion.TipoDeduccion.Clave)
                    ? NominaSatCatalogos.Sistema.DeduccionAusentismo
                    : deduccion.TipoDeduccion.Clave;
                AddIfPositive(conceptos, codigo, etiqueta, deduccion.Importe);
            }
        }
        else
        {
            AddIfPositive(conceptos, NominaSatCatalogos.Sistema.DeduccionAusentismo, string.IsNullOrWhiteSpace(detalle.ConceptoDeducciones) ? "Otras deducciones" : detalle.ConceptoDeducciones, detalle.Deducciones);
        }

        return conceptos;
    }

    private static void AddIfPositive(List<NominaReciboConcepto> conceptos, string codigo, string concepto, decimal importe)
    {
        if (importe > 0)
            conceptos.Add(new NominaReciboConcepto(codigo, concepto, importe));
    }

    private static string ConstruirConceptoHorasExtra(NominaDetalle detalle)
    {
        var horasBase = Math.Max(0m, detalle.HorasExtraBase > 0 ? detalle.HorasExtraBase : detalle.HorasExtra);
        var horasDobles = Math.Max(0m, detalle.HorasExtraDobles);
        var horasTriples = Math.Max(0m, detalle.HorasExtraTriples);

        if (horasDobles <= 0 && horasTriples <= 0)
        {
            return horasBase > 0
                ? $"Horas extra ({FormatearHoras(horasBase)} base)"
                : "Horas extra";
        }

        var partes = new List<string>();
        if (horasBase > 0)
        {
            partes.Add($"{FormatearHoras(horasBase)} base");
        }

        if (horasDobles > 0)
        {
            partes.Add($"{FormatearHoras(horasDobles)} dobles");
        }

        if (horasTriples > 0)
        {
            partes.Add($"{FormatearHoras(horasTriples)} triples");
        }

        return $"Horas extra ({string.Join(" · ", partes)})";
    }

    private static string FormatearHoras(decimal horas)
        => $"{horas:0.##} h";

    private static string ResolverClavePercepcionBono(string? claveConfigurada)
        => string.IsNullOrWhiteSpace(claveConfigurada) ? NominaSatCatalogos.Sistema.PercepcionOtrosIngresosSalarios : claveConfigurada;
}
