namespace MundoVs.Core.Interfaces;

public interface INominaPdfService
{
    Task<byte[]> GenerateReciboPdfAsync(Guid nominaDetalleId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateRecibosPdfAsync(Guid nominaId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateDashboardCostosPdfAsync(NominaDashboardCostosReport report, CancellationToken cancellationToken = default);
}

public sealed class NominaDashboardCostosReport
{
    public string Empresa { get; init; } = string.Empty;
    public DateTime Desde { get; init; }
    public DateTime Hasta { get; init; }
    public string Periodicidad { get; init; } = "Todas";
    public string EmpleadoFiltro { get; init; } = "Todos";
    public decimal NetoPagado { get; init; }
    public decimal CostoEmpresa { get; init; }
    public decimal ImssObrero { get; init; }
    public decimal ImssPatronal { get; init; }
    public decimal Isr { get; init; }
    public decimal Infonavit { get; init; }
    public decimal Provisiones { get; init; }
    public decimal HorasExtra { get; init; }
    public decimal Bonos { get; init; }
    public decimal Deducciones { get; init; }
    public decimal DescuentoMinutos { get; init; }
    public IReadOnlyList<NominaDashboardConceptoItem> PercepcionesAplicadas { get; init; } = [];
    public IReadOnlyList<NominaDashboardConceptoItem> PrestacionesAplicadas { get; init; } = [];
    public IReadOnlyList<NominaDashboardConceptoItem> DescuentosAplicados { get; init; } = [];
    public IReadOnlyList<NominaDashboardDepartamentoItem> CostosPorDepartamento { get; init; } = [];
    public IReadOnlyList<NominaDashboardEmpleadoItem> EmpleadosTop { get; init; } = [];
}

public sealed class NominaDashboardConceptoItem
{
    public string Nombre { get; init; } = string.Empty;
    public decimal Total { get; init; }
    public string Categoria { get; init; } = string.Empty;
}

public sealed class NominaDashboardDepartamentoItem
{
    public string Departamento { get; init; } = string.Empty;
    public int Empleados { get; init; }
    public decimal Neto { get; init; }
    public decimal CostoEmpresa { get; init; }
    public decimal ImssTotal { get; init; }
    public decimal Isr { get; init; }
    public decimal Infonavit { get; init; }
    public decimal Provisiones { get; init; }
    public decimal HorasExtra { get; init; }
    public decimal Bonos { get; init; }
    public decimal Deducciones { get; init; }
    public decimal DescuentoMinutos { get; init; }
}

public sealed class NominaDashboardEmpleadoItem
{
    public string Empleado { get; init; } = string.Empty;
    public string Departamento { get; init; } = string.Empty;
    public decimal Neto { get; init; }
    public decimal CostoEmpresa { get; init; }
}
