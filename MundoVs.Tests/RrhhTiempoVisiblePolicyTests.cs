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
}