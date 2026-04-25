using MundoVs.Core.Entities;
using MundoVs.Core.Models;
using MundoVs.Core.Services;

namespace MundoVs.Tests;

public sealed class RrhhAsistenciaCorreccionAdvisorTests
{
    [Fact]
    public void Analizar_CuandoHayFaltante_PriorizaPermisosAntesDeBanco()
    {
        var advisor = new RrhhAsistenciaCorreccionAdvisor();
        var asistencia = new RrhhAsistencia
        {
            MinutosJornadaProgramada = 540,
            MinutosJornadaNetaProgramada = 480,
            MinutosTrabajadosNetos = 240,
            MinutosExtra = 0,
            TotalMarcaciones = 2,
            Estatus = RrhhAsistenciaEstatus.AsistenciaNormal,
            RequiereRevision = false
        };

        var advice = advisor.Analizar(asistencia, null, true, true, 2m, 180);

        Assert.Equal(RrhhAsistenciaCorreccionTabs.Permisos, advice.TabSugerida);
        Assert.Equal("Revisar permiso neto", advice.AccionPrincipalTexto);
        Assert.Null(advice.ResolucionSugerida);
        Assert.True(advice.PriorizarPermiso);
        Assert.False(advice.PriorizarTiempo);
        Assert.Contains("descanso no pagado", advice.NotaPermiso ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(advice.ResolucionesDisponibles, x => x.Value == "CubrirFaltanteConBanco");
    }

    [Fact]
    public void Analizar_CuandoPermisoCubreFaltante_YaNoMarcaFaltaRemanente()
    {
        var advisor = new RrhhAsistenciaCorreccionAdvisor();
        var asistencia = new RrhhAsistencia
        {
            MinutosJornadaProgramada = 540,
            MinutosJornadaNetaProgramada = 480,
            MinutosTrabajadosNetos = 304,
            MinutosExtra = 0,
            TotalMarcaciones = 2,
            Estatus = RrhhAsistenciaEstatus.AsistenciaNormal,
            RequiereRevision = false
        };
        var permiso = new RrhhAusencia
        {
            Tipo = TipoAusenciaRrhh.Permiso,
            Horas = 176m / 60m,
            ConGocePago = true
        };

        var advice = advisor.Analizar(asistencia, permiso, true, true, 2m, 180);

        Assert.Equal("PermisoCubreFaltante", advice.Escenario);
        Assert.Equal("El permiso ya cubre el faltante", advice.Titulo);
        Assert.Equal(RrhhAsistenciaCorreccionTabs.Permisos, advice.TabSugerida);
        Assert.DoesNotContain(advice.Segmentos, x => x.Clave == "faltante");
        Assert.DoesNotContain(advice.ResolucionesDisponibles, x => x.Value == "CubrirFaltanteConBanco");
        Assert.Contains("ya no queda faltante remanente", advice.NotaPermiso ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }
}
