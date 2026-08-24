using MundoVs.Core.Entities;
using MundoVs.Core.Services;

namespace MundoVs.Tests;

/// <summary>
/// Tests del cálculo de "tiempo visible" en <see cref="RrhhTiempoExtraPolicy"/>.
/// Cubren las tres fuentes de inconsistencia que la Fase A corrige:
///  1) rama sin referencia de jornada (EsSinReferenciaJornada = EsPorHoras ||
///     jornada neta <= 0; cubre día no laborable con turno, sin turno, y PorHoras),
///  2) permiso visible canónico (con-goce prorrateado + banco-cobertura),
///  3) prorrateo de permisos multi-día.
/// </summary>
public sealed class RrhhTiempoVisiblePolicyTests
{
    private static RrhhAsistencia ConTurno(int jornadaNeta, int neto, int extra = 0, int extraPago = 0, int extraBanco = 0)
        => new()
        {
            TurnoBaseId = Guid.NewGuid(),
            ModoSugerenciaExtra = "EntradaSalida",
            MinutosJornadaNetaProgramada = jornadaNeta,
            MinutosTrabajadosNetos = neto,
            MinutosExtra = extra,
            MinutosExtraAutorizadosPago = extraPago,
            MinutosExtraAutorizadosBanco = extraBanco
        };

    private static RrhhAsistencia SinTurnoConTurnoAsignado(int neto, int extraPago = 0)
        => new()
        {
            TurnoBaseId = Guid.NewGuid(),
            MinutosJornadaNetaProgramada = 0,
            MinutosTrabajadosNetos = neto,
            MinutosExtra = 0,
            MinutosExtraAutorizadosPago = extraPago
        };

    private static RrhhAsistencia SinTurnoSinTurnoBase(int neto, int extraPago = 0)
        => new()
        {
            TurnoBaseId = null,
            MinutosJornadaNetaProgramada = 0,
            MinutosTrabajadosNetos = neto,
            MinutosExtra = 0,
            MinutosExtraAutorizadosPago = extraPago
        };

    // 1) El síntoma "repentino": día no laborable con turno asignado (jornada neta 0).
    // Antes del fix el policy ramaba por TurnoBaseId != null → ExtraAprobado=Min(aprobado,0)=0 y
    // BaseVisible=0 → el visible perdía el extra aprobado. Ahora EsSinReferenciaJornada (jornada
    // neta <= 0) refleja el neto trabajado.
    [Fact]
    public void Visible_DiaNoLaborableConTurnoAsignadoJornadaNetaCero_ReflejaExtraAprobado()
    {
        var a = SinTurnoConTurnoAsignado(neto: 480, extraPago: 60);

        var baseVisible = RrhhTiempoExtraPolicy.ObtenerMinutosBasePagada(a);
        var extraAprobado = RrhhTiempoExtraPolicy.ObtenerMinutosExtraAprobados(a);
        var visible = RrhhTiempoExtraPolicy.ObtenerMinutosTiempoVisible(a, 0, 0);

        Assert.Equal(420, baseVisible);       // 480 - 60
        Assert.Equal(60, extraAprobado);      // no se trunca por MinutosExtra=0
        Assert.Equal(480, visible);           // 420 + 60
    }

    // 1b) Empleado sin turno asignado (TurnoBaseId null, jornada neta 0) → sin referencia de jornada.
    [Fact]
    public void Visible_SinTurnoBaseYJornadaNetaCero_SigueTratadoComoSinReferencia()
    {
        var a = SinTurnoSinTurnoBase(neto: 480, extraPago: 60);
        var visible = RrhhTiempoExtraPolicy.ObtenerMinutosTiempoVisible(a, 0, 0);
        Assert.Equal(480, visible);
    }

    // 1c) Esquema PorHoras: EsPorHoras dispara sin-referencia aunque la jornada neta sea > 0
    // (defensivo; el procesador normalmente la pone en 0, pero el policy no depende de eso).
    [Fact]
    public void Visible_PorHoras_EsSinReferenciaJornadaAunqueJornadaNetaPositiva()
    {
        var a = new RrhhAsistencia
        {
            TurnoBaseId = Guid.NewGuid(),
            EsPorHoras = true,
            MinutosJornadaNetaProgramada = 480, // intencionalmente > 0
            MinutosTrabajadosNetos = 300,
            MinutosExtra = 0,
            MinutosExtraAutorizadosPago = 40
        };

        var baseVisible = RrhhTiempoExtraPolicy.ObtenerMinutosBasePagada(a);
        var extraAprobado = RrhhTiempoExtraPolicy.ObtenerMinutosExtraAprobados(a);
        var visible = RrhhTiempoExtraPolicy.ObtenerMinutosTiempoVisible(a, 0, 0);

        Assert.Equal(260, baseVisible);  // 300 - 40
        Assert.Equal(40, extraAprobado); // no se trunca por MinutosExtra=0
        Assert.Equal(300, visible);      // 260 + 40
    }

    // 2) Banco-cobertura cuenta como tiempo visible (acordado con el usuario).
    // Antes del fix la lista diaria (2-arg) no la sumaba; ahora el policy la añade siempre.
    [Fact]
    public void Visible_ConCoberturaBanco_LaSumaAlVisibleSinPermiso()
    {
        var a = ConTurno(jornadaNeta: 480, neto: 420, extra: 0);
        a.MinutosCubiertosBancoHoras = 60;

        var visible = RrhhTiempoExtraPolicy.ObtenerMinutosTiempoVisible(a, 0, 0);

        // base = Min(420, 480) = 420; permiso visible = 0 + 60 (banco); extra = 0
        Assert.Equal(480, visible);
    }

    // 2b) El banco-cobertura no se duplica cuando el caller ya no lo pasa: el policy lo añade.
    [Fact]
    public void Visible_PermisoConGoceMasBancoCobertura_SumaAmbosUnaVez()
    {
        var a = ConTurno(jornadaNeta: 480, neto: 360, extra: 0);
        a.MinutosCubiertosBancoHoras = 60;

        var visible = RrhhTiempoExtraPolicy.ObtenerMinutosTiempoVisible(a, minutosPermisoConGoceDia: 60, minutosCompensadosAprobados: 0);

        // base = Min(360, 480) = 360; permiso visible = 60 (con goce) + 60 (banco) = 120; extra = 0
        Assert.Equal(480, visible);
    }

    // 3) Prorrateo de permiso multi-día: 24h sobre 3 días → 8h/día (no 24h a cada día).
    [Fact]
    public void PermisoConGoce_MultiDia_SeProrrateaPorDia()
    {
        var ausencia = new RrhhAusencia
        {
            Horas = 24m,
            Dias = 3,
            FechaInicio = new DateOnly(2026, 1, 10),
            FechaFin = new DateOnly(2026, 1, 12)
        };

        var porDia = RrhhTiempoExtraPolicy.ObtenerMinutosPermisoConGocePorDia(ausencia);
        Assert.Equal(480, porDia); // 24h / 3 = 8h = 480 min
    }

    // 3b) Permiso de un solo día no se divide.
    [Fact]
    public void PermisoConGoce_UnSoloDia_NoSeDivide()
    {
        var ausencia = new RrhhAusencia
        {
            Horas = 2m,
            Dias = 1,
            FechaInicio = new DateOnly(2026, 1, 10),
            FechaFin = new DateOnly(2026, 1, 10)
        };

        var porDia = RrhhTiempoExtraPolicy.ObtenerMinutosPermisoConGocePorDia(ausencia);
        Assert.Equal(120, porDia); // 2h = 120 min
    }

    // 3c) Si Dias no está poblado, se infiere del rango FechaInicio..FechaFin.
    [Fact]
    public void PermisoConGoce_DiasSinPoblar_SeInfereDelRango()
    {
        var ausencia = new RrhhAusencia
        {
            Horas = 24m,
            Dias = 0,
            FechaInicio = new DateOnly(2026, 1, 10),
            FechaFin = new DateOnly(2026, 1, 12)
        };

        var porDia = RrhhTiempoExtraPolicy.ObtenerMinutosPermisoConGocePorDia(ausencia);
        Assert.Equal(480, porDia); // 3 días por rango → 8h/día
    }

    // 4) Consistencia: el mismo día muestra el mismo visible venga de la firma 2-arg o 3-arg
    // (cuando no hay permiso con goce ni compensación).
    [Fact]
    public void Visible_Firma2ArgY3Arg_CoincidenSinPermisoNiCompensacion()
    {
        var a = ConTurno(jornadaNeta: 480, neto: 540, extra: 60, extraPago: 60);
        a.MinutosCubiertosBancoHoras = 0;

        var visible2 = RrhhTiempoExtraPolicy.ObtenerMinutosTiempoVisible(a, 0);
        var visible3 = RrhhTiempoExtraPolicy.ObtenerMinutosTiempoVisible(a, 0, 0);

        Assert.Equal(visible3, visible2);
        Assert.Equal(540, visible2); // base 480 + extra 60
    }

    // 5) ExtraAprobado se trunca por detectados con turno; sin turno no.
    [Fact]
    public void ExtraAprobado_ConTurno_SeTruncaPorDetectados()
    {
        var a = ConTurno(jornadaNeta: 480, neto: 540, extra: 30, extraPago: 60);
        Assert.Equal(30, RrhhTiempoExtraPolicy.ObtenerMinutosExtraAprobados(a));
    }

    [Fact]
    public void ExtraAprobado_SinTurno_NoSeTrunca()
    {
        var a = SinTurnoConTurnoAsignado(neto: 480, extraPago: 60);
        Assert.Equal(60, RrhhTiempoExtraPolicy.ObtenerMinutosExtraAprobados(a));
    }

    // Tolerancia de retardo que perdona el TIEMPO: los minutos perdonados por el umbral se
    // suman al neto efectivo (junto a los perdonados manuales) y reducen el faltante a 0.
    // English: Retardo tolerance that forgives TIME: threshold-forgiven minutes are added to net
    // effective time (alongside manual forgiveness) and reduce the faltante to 0.
    [Fact]
    public void NetoEfectivo_IncluyeToleranciaRetardoAplicada_YReduceFaltante()
    {
        var a = new RrhhAsistencia
        {
            TurnoBaseId = Guid.NewGuid(),
            ModoSugerenciaExtra = "EntradaSalida",
            MinutosJornadaNetaProgramada = 540,
            MinutosTrabajadosNetos = 535,
            MinutosPerdonadosManual = 0,
            MinutosToleranciaRetardoAplicada = 5
        };
        // 535 trabajado + 5 perdonados por tolerancia = 540 = jornada neta → faltante 0.
        Assert.Equal(540, RrhhTiempoExtraPolicy.ObtenerMinutosNetoEfectivo(a));
        Assert.Equal(0, RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteNeto(a));
    }

    [Fact]
    public void FaltanteNeto_ExcluyeRetardoNoTolerado_DejaFaltanteEnCero()
    {
        var a = new RrhhAsistencia
        {
            TurnoBaseId = Guid.NewGuid(),
            ModoSugerenciaExtra = "EntradaSalida",
            MinutosJornadaNetaProgramada = 540,
            MinutosTrabajadosNetos = 534,
            MinutosPerdonadosManual = 0,
            MinutosToleranciaRetardoAplicada = 0,
            MinutosRetardo = 6
        };
        // La tardanza (6 min) se contabiliza aparte en el bucket de retardo, NO dentro del
        // faltante, para no cobrarla dos veces (neteo semanal + descuento de salario). El
        // NetoEfectivo sigue reflejando el tiempo realmente trabajado (534); el faltante
        // neto excluye el retardo → 0 (día limpio en términos de ausencia; el retardo se
        // descuenta por separado en su bucket).
        // English: Lateness (6 min) is tracked in its own retardo bucket, NOT inside
        // faltante, to avoid charging it twice (weekly neteo + salary discount). NetoEfectivo
        // still reflects actually-worked time (534); faltante neto excludes retardo → 0
        // (clean of absence; the retardo is deducted separately in its own bucket).
        Assert.Equal(534, RrhhTiempoExtraPolicy.ObtenerMinutosNetoEfectivo(a));
        Assert.Equal(0, RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteNeto(a));
    }

    [Fact]
    public void FaltanteNeto_ExcluyeSalidaAnticipada_DejaFaltanteEnCero()
    {
        var a = new RrhhAsistencia
        {
            TurnoBaseId = Guid.NewGuid(),
            ModoSugerenciaExtra = "EntradaSalida",
            MinutosJornadaNetaProgramada = 540,
            MinutosTrabajadosNetos = 510,
            MinutosPerdonadosManual = 0,
            MinutosToleranciaRetardoAplicada = 0,
            MinutosSalidaAnticipada = 30
        };
        // La salida anticipada (30 min) se contabiliza aparte en su bucket, NO dentro del
        // faltante, para no cobrarla dos veces (neteo + descuento de salario). NetoEfectivo
        // sigue reflejando el tiempo trabajado (510); faltante = 540 − 510 − 30 = 0.
        // English: Early-leave (30 min) is tracked in its own bucket, NOT inside faltante,
        // to avoid charging it twice (neteo + salary discount). NetoEfectivo still reflects
        // worked time (510); faltante = 540 − 510 − 30 = 0.
        Assert.Equal(510, RrhhTiempoExtraPolicy.ObtenerMinutosNetoEfectivo(a));
        Assert.Equal(30, RrhhTiempoExtraPolicy.ObtenerMinutosSalidaAnticipadaEfectivos(a));
        Assert.Equal(0, RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteNeto(a));
    }

    [Fact]
    public void DescuentoTotal_RetardoNoTolerado_SeCuentaUnaVez_NoDobleDescuento()
    {
        var a = new RrhhAsistencia
        {
            TurnoBaseId = Guid.NewGuid(),
            ModoSugerenciaExtra = "EntradaSalida",
            MinutosJornadaNetaProgramada = 480,
            MinutosTrabajadosNetos = 473,
            MinutosPerdonadosManual = 0,
            MinutosToleranciaRetardoAplicada = 0,
            MinutosRetardo = 7
        };
        // Red test del bug del doble descuento: un retardo de 7 min producía faltante=7
        // (gap jornada−trabajado) Y retardo=7 → DescuentoTotal = 7+7 = 14 (cobrado dos veces
        // en el descuento de salario y en el neteo semanal). Con A′ el retardo se excluye del
        // faltante → faltante=0, DescuentoTotal = 7 (una sola vez, en su bucket).
        // English: Red test for the double-discount bug: a 7-min retardo produced faltante=7
        // (jornada−worked gap) AND retardo=7 → DescuentoTotal = 7+7 = 14 (charged twice in the
        // salary discount and the weekly neteo). With A′ the retardo is excluded from
        // faltante → faltante=0, DescuentoTotal = 7 (once, in its own bucket).
        Assert.Equal(7, RrhhTiempoExtraPolicy.ObtenerMinutosRetardoEfectivos(a));
        Assert.Equal(0, RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteNeto(a));
        Assert.Equal(7, RrhhTiempoExtraPolicy.ObtenerMinutosDescuentoTotal(a));
    }
}

/// <summary>
/// Tests del predicado y el balance de la meta semanal (Fija sin turno) en
/// <see cref="RrhhTiempoExtraPolicy"/>. La meta semanal activa el overlay a nivel
/// de periodo (no per-día): extra = trabajado − meta; déficit = meta − trabajado − conGoce.
/// English: Tests for the weekly-meta predicate and balance (Fija with no shift) in
/// RrhhTiempoExtraPolicy. The weekly meta activates a period-level overlay (not per-day):
/// extra = worked − meta; deficit = meta − worked − conGoce.
/// </summary>
public sealed class RrhhMetaSemanalPolicyTests
{
    private static RrhhAsistencia FijaSinTurno(int neto) => new()
    {
        TurnoBaseId = null,
        EsPorHoras = false,
        MinutosJornadaNetaProgramada = 0,
        MinutosTrabajadosNetos = neto
    };

    private static RrhhAsistencia FijaConTurno(int jornadaNeta, int neto) => new()
    {
        TurnoBaseId = Guid.NewGuid(),
        EsPorHoras = false,
        MinutosJornadaNetaProgramada = jornadaNeta,
        MinutosTrabajadosNetos = neto
    };

    private static RrhhAsistencia PorHoras(int neto) => new()
    {
        TurnoBaseId = null,
        EsPorHoras = true,
        MinutosJornadaNetaProgramada = 0,
        MinutosTrabajadosNetos = neto
    };

    [Fact]
    public void EsJornadaMetaSemanal_FijaSinTurno_True()
        => Assert.True(RrhhTiempoExtraPolicy.EsJornadaMetaSemanal(FijaSinTurno(480)));

    [Fact]
    public void EsJornadaMetaSemanal_PorHoras_False()
        => Assert.False(RrhhTiempoExtraPolicy.EsJornadaMetaSemanal(PorHoras(480)));

    [Fact]
    public void EsJornadaMetaSemanal_FijaConTurno_False()
        => Assert.False(RrhhTiempoExtraPolicy.EsJornadaMetaSemanal(FijaConTurno(480, 480)));

    [Fact]
    public void EsPeriodoMetaSemanal_TodosSinTurno_True()
    {
        var asistencias = new[] { FijaSinTurno(480), FijaSinTurno(300) };
        Assert.True(RrhhTiempoExtraPolicy.EsPeriodoMetaSemanal(asistencias));
    }

    [Fact]
    public void EsPeriodoMetaSemanal_MezclaConTurno_False()
    {
        var asistencias = new[] { FijaSinTurno(480), FijaConTurno(480, 480) };
        Assert.False(RrhhTiempoExtraPolicy.EsPeriodoMetaSemanal(asistencias));
    }

    [Fact]
    public void EsPeriodoMetaSemanal_TodoPorHoras_False()
    {
        var asistencias = new[] { PorHoras(480), PorHoras(300) };
        Assert.False(RrhhTiempoExtraPolicy.EsPeriodoMetaSemanal(asistencias));
    }

    [Fact]
    public void EsPeriodoMetaSemanal_Vacio_False()
        => Assert.False(RrhhTiempoExtraPolicy.EsPeriodoMetaSemanal(Array.Empty<RrhhAsistencia>()));

    [Fact]
    public void ObtenerMetaSemanalMinutos_EsHorasPor60()
    {
        Assert.Equal(0, RrhhTiempoExtraPolicy.ObtenerMetaSemanalMinutos(0));
        Assert.Equal(2880, RrhhTiempoExtraPolicy.ObtenerMetaSemanalMinutos(48));
        Assert.Equal(0, RrhhTiempoExtraPolicy.ObtenerMetaSemanalMinutos(-5)); // negativo → 0
    }

    [Fact]
    public void CalcularBalanceMetaSemanal_SobreMeta_DevuelveExtraYCeroDeficit()
    {
        var (extra, deficit) = RrhhTiempoExtraPolicy.CalcularBalanceMetaSemanal(3000, 0, 2880);
        Assert.Equal(120, extra);   // 50h − 48h
        Assert.Equal(0, deficit);
    }

    [Fact]
    public void CalcularBalanceMetaSemanal_BajoMeta_DevuelveCeroExtraYDeficit()
    {
        var (extra, deficit) = RrhhTiempoExtraPolicy.CalcularBalanceMetaSemanal(2400, 0, 2880);
        Assert.Equal(0, extra);
        Assert.Equal(480, deficit); // 48h − 40h = 8h
    }

    [Fact]
    public void CalcularBalanceMetaSemanal_ConGoceCubreDeficit_ReduceDeficit()
    {
        // 40h trabajadas + 8h con goce → déficit = 2880 − 2400 − 480 = 0.
        // English: 40h worked + 8h paid leave → deficit = 2880 − 2400 − 480 = 0.
        var (extra, deficit) = RrhhTiempoExtraPolicy.CalcularBalanceMetaSemanal(2400, 480, 2880);
        Assert.Equal(0, extra);
        Assert.Equal(0, deficit);
    }

    [Fact]
    public void CalcularBalanceMetaSemanal_ConGoceNoGeneraExtra()
    {
        // El con goce cubre la meta pero NO genera extra (el extra es sólo lo trabajado sobre la meta).
        // English: Paid leave covers the meta but does NOT generate extra (extra is only worked-over-meta).
        var (extra, deficit) = RrhhTiempoExtraPolicy.CalcularBalanceMetaSemanal(2400, 600, 2880);
        Assert.Equal(0, extra);
        Assert.Equal(0, deficit); // 2880 − 2400 − 600 = -120 → max(0,·) = 0
    }

    [Fact]
    public void CalcularBalanceMetaSemanal_Exacto48h_CeroCero()
    {
        var (extra, deficit) = RrhhTiempoExtraPolicy.CalcularBalanceMetaSemanal(2880, 0, 2880);
        Assert.Equal(0, extra);
        Assert.Equal(0, deficit);
    }

    [Fact]
    public void CalcularBalanceMetaSemanal_ExtraYDeficitMutuamenteExcluyentes()
    {
        // Nunca hay extra y déficit a la vez: si trabajado > meta, déficit = 0; si < meta, extra = 0.
        // English: Extra and deficit are never both positive: if worked > meta, deficit = 0; if < meta, extra = 0.
        var (extra1, deficit1) = RrhhTiempoExtraPolicy.CalcularBalanceMetaSemanal(3000, 0, 2880);
        Assert.True(extra1 > 0 && deficit1 == 0);
        var (extra2, deficit2) = RrhhTiempoExtraPolicy.CalcularBalanceMetaSemanal(2400, 0, 2880);
        Assert.True(extra2 == 0 && deficit2 > 0);
    }

    [Fact]
    public void CalcularBalanceMetaSemanal_ExcedenteBajoUmbral_NoCuentaComoExtra()
    {
        // Meta 2880, trabajado 2890 → excedente 10 < umbral 15 → extra 0 (consistencia con el
        // cálculo por día). El déficit sigue 0 (se trabajó sobre la meta). Sin umbral (default 0)
        // el extra sería 10; el umbral lo zeroa.
        // English: Meta 2880, worked 2890 → surplus 10 < threshold 15 → extra 0 (consistency with
        // the per-day calc). Deficit stays 0 (worked over the meta). Without threshold (default 0)
        // extra would be 10; the threshold zeroes it.
        var (extra, deficit) = RrhhTiempoExtraPolicy.CalcularBalanceMetaSemanal(2890, 0, 2880, 15);
        Assert.Equal(0, extra);
        Assert.Equal(0, deficit);

        // Default 0 = sin umbral → el excedente se reporta tal cual (backward compat).
        // English: Default 0 = no threshold → surplus reported as-is (backward compat).
        var (extraSinUmbral, _) = RrhhTiempoExtraPolicy.CalcularBalanceMetaSemanal(2890, 0, 2880);
        Assert.Equal(10, extraSinUmbral);
    }

    [Fact]
    public void CalcularBalanceMetaSemanal_ExcedenteSobreUmbral_CuentaTodoElExcedente()
    {
        // Meta 2880, trabajado 2911 → excedente 31 ≥ umbral 15 → extra 31 (TODO el excedente,
        // no 31−15). Consistencia con el modo por día y con MarcajeReloj.
        // English: Meta 2880, worked 2911 → surplus 31 ≥ threshold 15 → extra 31 (the WHOLE
        // surplus, not 31−15). Consistency with the per-day mode and MarcajeReloj.
        var (extra, deficit) = RrhhTiempoExtraPolicy.CalcularBalanceMetaSemanal(2911, 0, 2880, 15);
        Assert.Equal(31, extra);
        Assert.Equal(0, deficit);
    }
}