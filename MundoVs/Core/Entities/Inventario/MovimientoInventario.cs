using MundoVs.Core.Entities;
using MundoVs.Core.Entities.Serigrafia;

namespace MundoVs.Core.Entities.Inventario;

public class MovimientoInventario : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public MovimientoInventarioOrigenEnum Origen { get; set; }
    public MovimientoInventarioTipoEnum TipoMovimiento { get; set; }

    public Guid? MateriaPrimaId { get; set; }
    public MateriaPrima? MateriaPrima { get; set; }

    public Guid? InsumoId { get; set; }
    public Insumo? Insumo { get; set; }

    public decimal Cantidad { get; set; }
    public decimal ExistenciaAnterior { get; set; }
    public decimal ExistenciaNueva { get; set; }
    public decimal CostoUnitario { get; set; }
    public DateTime FechaMovimiento { get; set; } = DateTime.UtcNow;
    public string? Referencia { get; set; }
    public string? Observaciones { get; set; }
}

public enum MovimientoInventarioOrigenEnum
{
    MateriaPrima = 1,
    Insumo = 2
}

public enum MovimientoInventarioTipoEnum
{
    Entrada = 1,
    Salida = 2,
    Ajuste = 3
}
