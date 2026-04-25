using MundoVs.Core.Entities.Calzado;

namespace MundoVs.Core.Entities;

public class ProductoCliente : BaseEntity
{
    public Guid ClienteId { get; set; }
    public Guid ProductoId { get; set; }
    public string? TallaBaseCalzado { get; set; }
    public bool AplicaFraccionCalzado { get; set; }
    public Guid? ClienteFraccionCalzadoId { get; set; }

    public Cliente Cliente { get; set; } = null!;
    public Producto Producto { get; set; } = null!;
    public ClienteFraccionCalzado? ClienteFraccionCalzado { get; set; }
}
