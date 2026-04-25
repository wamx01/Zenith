using MundoVs.Core.Entities.Auth;

namespace MundoVs.Core.Interfaces;

public interface IAuthService
{
    Task<Usuario?> LoginAsync(string email, string password);
    Task<bool> CambiarPasswordAsync(Guid usuarioId, string passwordActual, string passwordNuevo);
    Task<bool> CambiarPasswordInicialAsync(Guid usuarioId, string passwordNuevo);
    Task<string?> GenerarPasswordTemporalAsync(Guid usuarioId);
    Task<string?> GenerarTokenRecuperacionAsync(string email);
    Task<bool> RestablecerPasswordAsync(string email, string token, string passwordNuevo);
    string? ValidarPassword(string password);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    Task<List<string>> GetCapacidadesAsync(Guid tipoUsuarioId);
}
