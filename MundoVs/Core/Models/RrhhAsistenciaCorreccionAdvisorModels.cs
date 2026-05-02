namespace MundoVs.Core.Models;

public static class RrhhAsistenciaCorreccionTabs
{
    public const string Resumen = "resumen";
    public const string Marcaciones = "marcaciones";
    public const string Permisos = "permisos";
    public const string Tiempo = "tiempo";
}

public sealed record RrhhAsistenciaCorreccionSegmento(
    string Clave,
    string Titulo,
    int Minutos,
    decimal WidthPercent,
    string CssClass,
    string TextoCorto,
    bool EsAjuste = false);

public sealed record RrhhAsistenciaCorreccionResolucionOption(
    string Value,
    string Label,
    string Description);

public sealed record RrhhAsistenciaCorreccionAdvice(
    string Escenario,
    string Titulo,
    string Descripcion,
    string BadgeClass,
    string Icono,
    string TabSugerida,
    string AccionPrincipalTexto,
    string? ResolucionSugerida,
    IReadOnlyList<RrhhAsistenciaCorreccionSegmento> Segmentos,
    IReadOnlyList<RrhhAsistenciaCorreccionResolucionOption> ResolucionesDisponibles,
    string? NotaPermiso,
    string? NotaTiempo,
    bool PriorizarMarcaciones,
    bool PriorizarPermiso,
    bool PriorizarTiempo);
