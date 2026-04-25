using MundoVs.Core.Entities;

namespace MundoVs.Core.Interfaces;

public interface IProductoRepository : IRepository<Producto>
{
    Task<Producto?> GetByCodigoAsync(string codigo);
    Task<IEnumerable<Producto>> GetByIndustriaAsync(IndustriaEnum industria);
}
