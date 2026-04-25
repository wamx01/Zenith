using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities.Auth;
using MundoVs.Core.Interfaces;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Services;

public class AuthService : IAuthService
{
    private readonly IDbContextFactory<CrmDbContext> _contextFactory;
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;
    private const int MaxIntentosFallidos = 5;
    private static readonly TimeSpan DuracionBloqueo = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan DuracionTokenRecuperacion = TimeSpan.FromMinutes(30);

    public AuthService(IDbContextFactory<CrmDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Usuario?> LoginAsync(string email, string password)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var usuario = await context.Usuarios
            .IgnoreQueryFilters()
            .Include(u => u.TipoUsuario)
            .Include(u => u.Empresa)
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

        if (usuario == null)
            return null;

        if (usuario.BloqueadoHastaUtc.HasValue && usuario.BloqueadoHastaUtc.Value > DateTime.UtcNow)
            return null;

        if (!VerifyPassword(password, usuario.PasswordHash))
        {
            usuario.IntentosFallidos++;
            if (usuario.IntentosFallidos >= MaxIntentosFallidos)
            {
                usuario.BloqueadoHastaUtc = DateTime.UtcNow.Add(DuracionBloqueo);
                usuario.IntentosFallidos = 0;
            }

            usuario.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return null;
        }

        var suscripcionActual = await context.SuscripcionesEmpresa.IgnoreQueryFilters()
            .Where(s => s.EmpresaId == usuario.EmpresaId)
            .OrderByDescending(s => s.FechaInicio)
            .ThenByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();

        if (!PuedeIniciarSesion(usuario.Empresa, suscripcionActual))
            return null;

        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHastaUtc = null;
        usuario.UltimoAcceso = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return usuario;
    }

    public async Task<bool> CambiarPasswordAsync(Guid usuarioId, string passwordActual, string passwordNuevo)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId);
        if (usuario == null || !VerifyPassword(passwordActual, usuario.PasswordHash))
            return false;

        if (ValidarPassword(passwordNuevo) != null)
            return false;

        usuario.PasswordHash = HashPassword(passwordNuevo);
        usuario.PasswordChangedAt = DateTime.UtcNow;
        usuario.PasswordResetTokenHash = null;
        usuario.PasswordResetTokenExpiresAt = null;
        usuario.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<string?> GenerarPasswordTemporalAsync(Guid usuarioId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var usuario = await context.Usuarios
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == usuarioId && u.IsActive);
        if (usuario == null)
            return null;

        var passwordTemporal = GenerateTemporaryPassword();
        usuario.PasswordHash = HashPassword(passwordTemporal);
        usuario.RequiereCambioPassword = true;
        usuario.PasswordChangedAt = null;
        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHastaUtc = null;
        usuario.PasswordResetTokenHash = null;
        usuario.PasswordResetTokenExpiresAt = null;
        usuario.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return passwordTemporal;
    }

    public async Task<bool> CambiarPasswordInicialAsync(Guid usuarioId, string passwordNuevo)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId && u.RequiereCambioPassword);
        if (usuario == null)
            return false;

        if (ValidarPassword(passwordNuevo) != null)
            return false;

        usuario.PasswordHash = HashPassword(passwordNuevo);
        usuario.RequiereCambioPassword = false;
        usuario.PasswordChangedAt = DateTime.UtcNow;
        usuario.PasswordResetTokenHash = null;
        usuario.PasswordResetTokenExpiresAt = null;
        usuario.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<string?> GenerarTokenRecuperacionAsync(string email)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var usuario = await context.Usuarios
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        if (usuario == null)
            return null;

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        usuario.PasswordResetTokenHash = HashToken(token);
        usuario.PasswordResetTokenExpiresAt = DateTime.UtcNow.Add(DuracionTokenRecuperacion);
        usuario.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return token;
    }

    public async Task<bool> RestablecerPasswordAsync(string email, string token, string passwordNuevo)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var usuario = await context.Usuarios
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        if (usuario == null)
            return false;

        if (ValidarPassword(passwordNuevo) != null)
            return false;

        if (string.IsNullOrWhiteSpace(usuario.PasswordResetTokenHash)
            || !usuario.PasswordResetTokenExpiresAt.HasValue
            || usuario.PasswordResetTokenExpiresAt.Value <= DateTime.UtcNow)
            return false;

        if (!string.Equals(usuario.PasswordResetTokenHash, HashToken(token), StringComparison.Ordinal))
            return false;

        usuario.PasswordHash = HashPassword(passwordNuevo);
        usuario.PasswordChangedAt = DateTime.UtcNow;
        usuario.RequiereCambioPassword = false;
        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHastaUtc = null;
        usuario.PasswordResetTokenHash = null;
        usuario.PasswordResetTokenExpiresAt = null;
        usuario.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return true;
    }

    public string? ValidarPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return "La contraseña es obligatoria.";

        if (password.Length < 8)
            return "La contraseña debe tener al menos 8 caracteres.";

        if (!password.Any(char.IsUpper))
            return "La contraseña debe incluir al menos una mayúscula.";

        if (!password.Any(char.IsLower))
            return "La contraseña debe incluir al menos una minúscula.";

        if (!password.Any(char.IsDigit))
            return "La contraseña debe incluir al menos un número.";

        if (!password.Any(c => !char.IsLetterOrDigit(c)))
            return "La contraseña debe incluir al menos un carácter especial.";

        return null;
    }

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split('.');
        if (parts.Length != 2) return false;

        var salt = Convert.FromBase64String(parts[0]);
        var hash = Convert.FromBase64String(parts[1]);
        var computedHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return CryptographicOperations.FixedTimeEquals(hash, computedHash);
    }

    public async Task<List<string>> GetCapacidadesAsync(Guid tipoUsuarioId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TipoUsuarioCapacidades
            .Where(tc => tc.TipoUsuarioId == tipoUsuarioId)
            .Select(tc => tc.Capacidad.Clave)
            .ToListAsync();
    }

    private static bool PuedeIniciarSesion(MundoVs.Core.Entities.Empresa? empresa, MundoVs.Core.Entities.SuscripcionEmpresa? suscripcion)
    {
        if (empresa == null || !empresa.IsActive || empresa.IsSuspended)
            return false;

        if (empresa.Estado is MundoVs.Core.Entities.EmpresaEstado.Suspendida or MundoVs.Core.Entities.EmpresaEstado.Cancelada)
            return false;

        if (empresa.Estado == MundoVs.Core.Entities.EmpresaEstado.Demo && empresa.TrialEndsAt.HasValue && empresa.TrialEndsAt.Value.Date < DateTime.UtcNow.Date)
            return false;

        if (suscripcion != null)
        {
            if (suscripcion.Estado is MundoVs.Core.Entities.EstadoSuscripcion.Suspendida or MundoVs.Core.Entities.EstadoSuscripcion.Vencida or MundoVs.Core.Entities.EstadoSuscripcion.Cancelada)
                return false;

            if (suscripcion.Estado is MundoVs.Core.Entities.EstadoSuscripcion.Trial or MundoVs.Core.Entities.EstadoSuscripcion.Activa)
            {
                if (suscripcion.FechaFin.HasValue && suscripcion.FechaFin.Value.Date < DateTime.UtcNow.Date)
                    return false;
            }
        }

        return true;
    }

    private static string GenerateTemporaryPassword()
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string special = "!@$%*-_";
        var all = upper + lower + digits + special;

        Span<char> password = stackalloc char[12];
        password[0] = upper[RandomNumberGenerator.GetInt32(upper.Length)];
        password[1] = lower[RandomNumberGenerator.GetInt32(lower.Length)];
        password[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
        password[3] = special[RandomNumberGenerator.GetInt32(special.Length)];

        for (var i = 4; i < password.Length; i++)
        {
            password[i] = all[RandomNumberGenerator.GetInt32(all.Length)];
        }

        for (var i = password.Length - 1; i > 0; i--)
        {
            var swapIndex = RandomNumberGenerator.GetInt32(i + 1);
            (password[i], password[swapIndex]) = (password[swapIndex], password[i]);
        }

        return new string(password);
    }

    private static string HashToken(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
