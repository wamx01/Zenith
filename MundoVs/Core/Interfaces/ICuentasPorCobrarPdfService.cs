namespace MundoVs.Core.Interfaces;

public interface ICuentasPorCobrarPdfService
{
    Task<byte[]> GenerateReportePdfAsync(CuentasPorCobrarPdfReport report, CancellationToken cancellationToken = default);
}

public sealed class CuentasPorCobrarPdfReport
{
    public string Empresa { get; init; } = "Zenith";
    public DateTime Desde { get; init; }
    public DateTime Hasta { get; init; }
    public string ClienteFiltro { get; init; } = "Todos";
    public decimal TotalPendiente { get; init; }
    public decimal TotalCobrado { get; init; }
    public decimal MontoVencido { get; init; }
    public int ClientesConAdeudo { get; init; }
    public decimal Bucket0a30 { get; init; }
    public decimal Bucket31a60 { get; init; }
    public decimal Bucket61a90 { get; init; }
    public decimal BucketMas90 { get; init; }
    public IReadOnlyList<CuentasPorCobrarAntiguedadPdfItem> AntiguedadClientes { get; init; } = [];
    public IReadOnlyList<CuentasPorCobrarVencidaPdfItem> DocumentosVencidos { get; init; } = [];
    public CuentasPorCobrarEstadoCuentaPdf EstadoCuenta { get; init; } = new();
    public IReadOnlyList<CuentasPorCobrarMovimientoPdfItem> Movimientos { get; init; } = [];
}

public sealed class CuentasPorCobrarAntiguedadPdfItem
{
    public string Cliente { get; init; } = string.Empty;
    public decimal Rango0a30 { get; init; }
    public decimal Rango31a60 { get; init; }
    public decimal Rango61a90 { get; init; }
    public decimal RangoMas90 { get; init; }
    public decimal Total => Rango0a30 + Rango31a60 + Rango61a90 + RangoMas90;
}

public sealed class CuentasPorCobrarVencidaPdfItem
{
    public string Cliente { get; init; } = string.Empty;
    public string Documento { get; init; } = string.Empty;
    public DateTime FechaEmision { get; init; }
    public DateTime? FechaVencimiento { get; init; }
    public int DiasAtraso { get; init; }
    public decimal Saldo { get; init; }
}

public sealed class CuentasPorCobrarEstadoCuentaPdf
{
    public string Cliente { get; init; } = "Sin selección";
    public decimal SaldoInicial { get; init; }
    public decimal SaldoFinal { get; init; }
    public IReadOnlyList<CuentasPorCobrarEstadoCuentaMovimientoPdfItem> Movimientos { get; init; } = [];
}

public sealed class CuentasPorCobrarEstadoCuentaMovimientoPdfItem
{
    public DateTime Fecha { get; init; }
    public string Tipo { get; init; } = string.Empty;
    public string Referencia { get; init; } = string.Empty;
    public string Concepto { get; init; } = string.Empty;
    public decimal Cargo { get; init; }
    public decimal Abono { get; init; }
    public decimal Saldo { get; init; }
}

public sealed class CuentasPorCobrarMovimientoPdfItem
{
    public DateTime Fecha { get; init; }
    public string Cliente { get; init; } = string.Empty;
    public string Tipo { get; init; } = string.Empty;
    public string Referencia { get; init; } = string.Empty;
    public string Concepto { get; init; } = string.Empty;
    public decimal Cargo { get; init; }
    public decimal Abono { get; init; }
    public decimal Saldo { get; init; }
}
