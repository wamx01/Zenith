using MundoVs.Core.Entities;

namespace MundoVs.Core.Interfaces;

public interface IClienteRepository : IRepository<Cliente>
{
    Task<Cliente?> GetByCodigoAsync(string codigo);
    Task<IEnumerable<Cliente>> GetByIndustriaAsync(IndustriaEnum industria);
    Task<bool> ExistsByCodigoAsync(string codigo);
}
