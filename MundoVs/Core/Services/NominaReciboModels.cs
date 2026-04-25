namespace MundoVs.Core.Services;

public class NominaReciboResult
{
    public IReadOnlyList<NominaReciboConcepto> Percepciones { get; init; } = [];
    public IReadOnlyList<NominaReciboConcepto> Deducciones { get; init; } = [];
    public decimal TotalPercepciones { get; init; }
    public decimal TotalDeducciones { get; init; }
    public decimal NetoPagar { get; init; }
}

public sealed record NominaReciboConcepto(string Codigo, string Concepto, decimal Importe);
