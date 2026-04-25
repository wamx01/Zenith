using MundoVs.Core.Entities.Serigrafia;

namespace MundoVs.Core.Interfaces;

public interface IPresupuestoProductoRepository : IRepository<PresupuestoProducto>
{
    Task<IEnumerable<PresupuestoProducto>> GetAllWithDetailsAsync();
    Task<PresupuestoProducto?> GetWithDetailsAsync(Guid id);
    Task<IEnumerable<PresupuestoProducto>> GetByProductoAsync(Guid productoId);
}
