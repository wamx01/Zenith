using MundoVs.Core.Entities;

namespace MundoVs.Core.Services;

/// <summary>
/// Categoría de un permiso por diferencia neta (generado al cierre del periodo cuando
/// |retardoDetectado − extraDetectado| > 0). El operador decide la repartición.
/// </summary>
public enum CategoriaPermisoDiferencia
{
    /// <summary>
    /// Con goce de sueldo + consume saldo del banco de horas.
    /// DescuentaBancoHoras=true, ConGocePago=true.
    /// </summary>
    Banco = 1,

    /// <summary>
    /// Con goce de sueldo, sin tocar el banco (la empresa absorbe el costo).
    /// DescuentaBancoHoras=false, ConGocePago=true.
    /// </summary>
    ConGoceSinBanco = 2,

    /// <summary>
    /// Sin goce de sueldo (se descuenta del salario del periodo).
    /// DescuentaBancoHoras=false, ConGocePago=false.
    /// </summary>
    SinGoce = 3
}

/// <summary>
/// Input del operador para crear / actualizar permisos por diferencia de un periodo.
/// Mapea 1:1 a una fila sintética de RrhhAusencia (Tipo=PermisoPorDiferenciaPeriodo).
/// </summary>
public sealed class PermisoDiferenciaInput
{
    public CategoriaPermisoDiferencia Categoria { get; init; }
    /// <summary>Minutos >= 0. Una categoría con Minutos=0 no genera fila.</summary>
    public int Minutos { get; init; }
    /// <summary>Factor informativo >= 1. v1 lo acepta y lo persiste; sin impacto en cálculo de nómina del banco.</summary>
    public decimal Factor { get; init; } = 1m;
    public string? Observaciones { get; init; }

    /// <summary>
    /// Devuelve el TipoAusenciaRrhh canónico para esta categoría (siempre PermisoPorDiferenciaPeriodo)
    /// y los flags ConGocePago / DescuentaBancoHoras resultantes. Se persiste con esos valores.
    /// </summary>
    public (TipoAusenciaRrhh Tipo, bool ConGocePago, bool DescuentaBancoHoras) ResolverFlags()
        => Categoria switch
        {
            CategoriaPermisoDiferencia.Banco
                => (TipoAusenciaRrhh.PermisoPorDiferenciaPeriodo, true, true),
            CategoriaPermisoDiferencia.ConGoceSinBanco
                => (TipoAusenciaRrhh.PermisoPorDiferenciaPeriodo, true, false),
            CategoriaPermisoDiferencia.SinGoce
                => (TipoAusenciaRrhh.PermisoPorDiferenciaPeriodo, false, false),
            _ => throw new InvalidOperationException($"Categoría de permiso por diferencia no soportada: {Categoria}")
        };
}

/// <summary>
/// Sugerencia (read-only) que la UI consume antes de capturar. El operador decide
/// la repartición — el servicio no auto-sugiere categorías.
/// </summary>
public sealed class PermisoDiferenciaSugerencia
{
    /// <summary>|retardoDetectado − extraDetectado| del periodo. Si ≤ 0, no se ofrece permiso.</summary>
    public int DiferenciaMinutos { get; init; }
    /// <summary>Saldo actual del banco de horas del empleado, en minutos.</summary>
    public int BancoDisponibleMinutos { get; init; }
    /// <summary>Periodo al que aplica la sugerencia (etiqueta legible: ej. "Sem 31 · 2026").</summary>
    public string PeriodoEtiqueta { get; init; } = string.Empty;
    /// <summary>PeriodoKey (ej. "Semanal-2026-31") para que la UI confirme al reabrir.</summary>
    public string PeriodoKey { get; init; } = string.Empty;
}