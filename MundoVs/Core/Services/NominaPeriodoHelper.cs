using System.Globalization;
using MundoVs.Core.Entities;

namespace MundoVs.Core.Services;

public sealed class NominaPeriodoCalendario
{
    public required PeriodicidadPago PeriodicidadPago { get; init; }
    public required DateTime Inicio { get; init; }
    public required DateTime Fin { get; init; }
    public required int AnioPeriodo { get; init; }
    public required int NumeroPeriodo { get; init; }
    public required string Periodo { get; init; }
    public required string NumeroNomina { get; init; }
}

public static class NominaPeriodoHelper
{
    public static NominaPeriodoCalendario ObtenerPeriodo(PeriodicidadPago periodicidad, DateTime fechaReferencia, NominaCorteRrhh? corte = null)
        => periodicidad switch
        {
            PeriodicidadPago.Quincenal => ObtenerPeriodoQuincenal(fechaReferencia, corte?.DiaCorteMes ?? 15),
            PeriodicidadPago.Mensual => ObtenerPeriodoMensual(fechaReferencia, corte?.DiaCorteMes ?? 31),
            _ => ObtenerPeriodoSemanalCalendario(fechaReferencia, corte?.DiaCorteSemana ?? DayOfWeek.Sunday)
        };

    public static (DateTime Inicio, DateTime Fin) ObtenerPeriodoSemanal(DateTime fechaReferencia, DayOfWeek diaCorteSemana)
    {
        var fecha = fechaReferencia.Date;
        var diferencia = ((int)fecha.DayOfWeek - (int)diaCorteSemana + 7) % 7;
        var fin = fecha.AddDays(-diferencia);
        var inicio = fin.AddDays(-6);
        return (inicio, fin);
    }

    public static NominaPeriodoCalendario ObtenerPeriodoSemanalCalendario(DateTime fechaReferencia, DayOfWeek diaCorteSemana)
    {
        var (inicio, fin) = ObtenerPeriodoSemanal(fechaReferencia, diaCorteSemana);
        var anioPeriodo = fin.Year;
        var numeroPeriodo = ObtenerNumeroPeriodoSemanal(fin, diaCorteSemana);

        return new NominaPeriodoCalendario
        {
            PeriodicidadPago = PeriodicidadPago.Semanal,
            Inicio = inicio,
            Fin = fin,
            AnioPeriodo = anioPeriodo,
            NumeroPeriodo = numeroPeriodo,
            Periodo = ConstruirEtiquetaSemanal(inicio, fin),
            NumeroNomina = $"Sem{numeroPeriodo:00}"
        };
    }

    public static NominaPeriodoCalendario ObtenerPeriodoQuincenal(DateTime fechaReferencia, int diaCorteMes)
    {
        var fecha = fechaReferencia.Date;
        var diaCorte = Math.Clamp(diaCorteMes, 1, 27);
        var inicioMes = new DateTime(fecha.Year, fecha.Month, 1);
        var finPrimeraQuincena = new DateTime(fecha.Year, fecha.Month, diaCorte);
        var finMes = new DateTime(fecha.Year, fecha.Month, DateTime.DaysInMonth(fecha.Year, fecha.Month));
        var primeraQuincena = fecha.Day <= diaCorte;
        var inicio = primeraQuincena ? inicioMes : finPrimeraQuincena.AddDays(1);
        var fin = primeraQuincena ? finPrimeraQuincena : finMes;
        var numeroPeriodo = ((fecha.Month - 1) * 2) + (primeraQuincena ? 1 : 2);

        return new NominaPeriodoCalendario
        {
            PeriodicidadPago = PeriodicidadPago.Quincenal,
            Inicio = inicio,
            Fin = fin,
            AnioPeriodo = fecha.Year,
            NumeroPeriodo = numeroPeriodo,
            Periodo = $"Quincena {(primeraQuincena ? 1 : 2)} · {inicio:dd/MM} - {fin:dd/MM/yyyy}",
            NumeroNomina = $"Quin{numeroPeriodo:00}"
        };
    }

    public static NominaPeriodoCalendario ObtenerPeriodoMensual(DateTime fechaReferencia, int diaCorteMes)
    {
        var fecha = fechaReferencia.Date;
        var finCorteMesActual = ObtenerFechaCorteMensual(fecha.Year, fecha.Month, diaCorteMes);
        DateTime inicio;
        DateTime fin;

        if (fecha <= finCorteMesActual)
        {
            fin = finCorteMesActual;
            var mesAnterior = fecha.AddMonths(-1);
            inicio = ObtenerFechaCorteMensual(mesAnterior.Year, mesAnterior.Month, diaCorteMes).AddDays(1);
        }
        else
        {
            inicio = finCorteMesActual.AddDays(1);
            var mesSiguiente = fecha.AddMonths(1);
            fin = ObtenerFechaCorteMensual(mesSiguiente.Year, mesSiguiente.Month, diaCorteMes);
        }

        return new NominaPeriodoCalendario
        {
            PeriodicidadPago = PeriodicidadPago.Mensual,
            Inicio = inicio,
            Fin = fin,
            AnioPeriodo = fin.Year,
            NumeroPeriodo = fin.Month,
            Periodo = ConstruirEtiquetaMensual(inicio, fin),
            NumeroNomina = $"Mes{fin.Month:00}"
        };
    }

    public static string ConstruirEtiquetaSemanal(DateTime inicio, DateTime fin)
        => $"Semana {inicio:dd/MM} - {fin:dd/MM/yyyy}";

    public static string ConstruirNumeroNominaSemanal(DateTime inicio, DateTime fin)
    {
        var fechaReferencia = fin.Date;
        var semanaMes = ((fechaReferencia.Day - 1) / 7) + 1;
        return $"Sem{semanaMes}";
    }

    public static string ConstruirNumeroNominaQuincenal(DateTime inicio, DateTime fin)
        => $"Quin{(fin.Date.Day <= 15 ? 1 : 2)}";

    public static string ConstruirNumeroNominaMensual(DateTime inicio, DateTime fin)
        => $"Mes{fin.Date.Month}";

    public static string ConstruirNumeroNomina(PeriodicidadPago periodicidad, DateTime inicio, DateTime fin)
        => periodicidad switch
        {
            PeriodicidadPago.Semanal => ConstruirNumeroNominaSemanal(inicio, fin),
            PeriodicidadPago.Quincenal => ConstruirNumeroNominaQuincenal(inicio, fin),
            PeriodicidadPago.Mensual => ConstruirNumeroNominaMensual(inicio, fin),
            _ => ConstruirNumeroNominaSemanal(inicio, fin)
        };

    public static string ConstruirEtiquetaMensual(DateTime inicio, DateTime fin)
    {
        var inicioMes = new DateTime(fin.Year, fin.Month, 1);
        var finMes = new DateTime(fin.Year, fin.Month, DateTime.DaysInMonth(fin.Year, fin.Month));
        return inicio.Date == inicioMes.Date && fin.Date == finMes.Date
            ? $"Mes {CultureInfo.GetCultureInfo("es-MX").DateTimeFormat.GetMonthName(fin.Month)} {fin:yyyy}"
            : $"Mensual {inicio:dd/MM} - {fin:dd/MM/yyyy}";
    }

    private static int ObtenerNumeroPeriodoSemanal(DateTime fin, DayOfWeek diaCorteSemana)
    {
        var primerDia = new DateTime(fin.Year, 1, 1);
        var diferencia = ((int)diaCorteSemana - (int)primerDia.DayOfWeek + 7) % 7;
        var primerCorte = primerDia.AddDays(diferencia);
        return fin < primerCorte
            ? 1
            : ((fin.Date - primerCorte.Date).Days / 7) + 1;
    }

    private static DateTime ObtenerFechaCorteMensual(int anio, int mes, int diaCorteMes)
    {
        var ultimoDia = DateTime.DaysInMonth(anio, mes);
        var dia = Math.Clamp(diaCorteMes, 1, 31);
        return new DateTime(anio, mes, Math.Min(dia, ultimoDia));
    }
}
