using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using MundoVs.Infrastructure.Data;
using MundoVs.Core.Interfaces;

namespace MundoVs.Core.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IEmpresaContext _empresaContext;
    private readonly IModuloAccesoService _moduloAccesoService;
    private readonly IDbContextFactory<CrmDbContext> _contextFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CustomAuthStateProvider> _logger;
    private static readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());
    private ClaimsPrincipal? _cachedUser;

    public CustomAuthStateProvider(IJSRuntime jsRuntime, IEmpresaContext empresaContext, IModuloAccesoService moduloAccesoService, IDbContextFactory<CrmDbContext> contextFactory, IHttpContextAccessor httpContextAccessor, ILogger<CustomAuthStateProvider> logger)
    {
        _jsRuntime = jsRuntime;
        _empresaContext = empresaContext;
        _moduloAccesoService = moduloAccesoService;
        _contextFactory = contextFactory;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            if (principal?.Identity?.IsAuthenticated != true)
            {
                _empresaContext.SetEmpresaId(Guid.Empty);
                _cachedUser = _anonymous;
                return new AuthenticationState(_anonymous);
            }

            var validatedPrincipal = await RestoreValidatedPrincipalAsync(principal);
            _cachedUser = validatedPrincipal;
            return new AuthenticationState(validatedPrincipal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restaurando el estado de autenticación desde la cookie del servidor.");
            return new AuthenticationState(_cachedUser ?? _anonymous);
        }
    }

    public async Task<AuthBrowserResult> LoginAsync(string email, string password)
    {
        var result = await _jsRuntime.InvokeAsync<AuthBrowserResult>("mundoVsAuth.login", "/auth/session/login", new
        {
            email,
            password
        });

        if (result.Succeeded)
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

        return result;
    }

    public async Task<bool> RefreshSessionAsync()
    {
        var result = await _jsRuntime.InvokeAsync<AuthBrowserResult>("mundoVsAuth.refresh", "/auth/session/refresh");
        if (!result.Succeeded)
            return false;

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        return true;
    }

    public async Task LogoutAsync()
    {
        await _jsRuntime.InvokeVoidAsync("mundoVsAuth.logout", "/auth/session/logout");
        _empresaContext.SetEmpresaId(Guid.Empty);
        _cachedUser = _anonymous;
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }

    private async Task<ClaimsPrincipal> RestoreValidatedPrincipalAsync(ClaimsPrincipal principal)
    {
        var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            _logger.LogWarning("Cookie descartada porque no contiene un UserId válido.");
            _empresaContext.SetEmpresaId(Guid.Empty);
            return _anonymous;
        }

        await using var context = await _contextFactory.CreateDbContextAsync();
        var usuario = await context.Usuarios
            .IgnoreQueryFilters()
            .Include(u => u.TipoUsuario)
            .Include(u => u.Empresa)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        if (usuario == null)
        {
            _logger.LogWarning("Cookie descartada porque el usuario ya no está disponible o activo.");
            _empresaContext.SetEmpresaId(Guid.Empty);
            return _anonymous;
        }

        var suscripcionActual = await context.SuscripcionesEmpresa.IgnoreQueryFilters()
            .Where(s => s.EmpresaId == usuario.EmpresaId)
            .OrderByDescending(s => s.FechaInicio)
            .ThenByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();

        if (!EmpresaPuedeOperar(usuario.Empresa, suscripcionActual))
        {
            _logger.LogWarning("Cookie descartada porque la empresa del usuario no puede operar.");
            _empresaContext.SetEmpresaId(Guid.Empty);
            return _anonymous;
        }

        var capacidades = await context.TipoUsuarioCapacidades
            .Where(tc => tc.TipoUsuarioId == usuario.TipoUsuarioId)
            .Select(tc => tc.Capacidad.Clave)
            .ToListAsync();

        var isSuperAdmin = string.Equals(usuario.TipoUsuario.Nombre, "SuperAdmin", StringComparison.OrdinalIgnoreCase);
        var capacidadesFiltradas = await _moduloAccesoService.FiltrarCapacidadesAsync(usuario.EmpresaId, capacidades, isSuperAdmin);
        var modulosHabilitados = await _moduloAccesoService.ObtenerModulosHabilitadosAsync(usuario.EmpresaId);

        _empresaContext.SetEmpresaId(usuario.EmpresaId);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Name, usuario.NombreCompleto),
            new(ClaimTypes.Role, usuario.TipoUsuario.Nombre),
            new("TipoUsuarioId", usuario.TipoUsuarioId.ToString()),
            new("EmpresaId", usuario.EmpresaId.ToString()),
            new("RequirePasswordChange", usuario.RequiereCambioPassword.ToString())
        };

        claims.AddRange(capacidadesFiltradas.Select(cap => new Claim("Capacidad", cap)));
        claims.AddRange(modulosHabilitados.Select(modulo => new Claim("Modulo", modulo)));

        var validatedPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "CookieAuth"));

        _logger.LogInformation(
            "Sesión restaurada desde cookie estándar para {Email}. Capabilities: {Capabilities}",
            usuario.Email,
            string.Join(", ", capacidadesFiltradas));

        return validatedPrincipal;
    }

    private static bool EmpresaPuedeOperar(MundoVs.Core.Entities.Empresa? empresa, MundoVs.Core.Entities.SuscripcionEmpresa? suscripcion)
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

    public sealed class AuthBrowserResult
    {
        public bool Succeeded { get; set; }
        public string? Error { get; set; }
        public string? RedirectUrl { get; set; }
    }
}
