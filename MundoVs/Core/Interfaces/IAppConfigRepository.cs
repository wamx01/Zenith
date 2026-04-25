using MundoVs.Core.Entities;

namespace MundoVs.Core.Interfaces;

public interface IAppConfigRepository
{
    Task<string?> GetValueAsync(string clave);
    Task SetValueAsync(string clave, string valor, string? descripcion = null);
    Task<IEnumerable<AppConfig>> GetAllAsync();
    Task<string> GetNextCodeAsync(string prefijo, string claveConsecutivo);
}
