using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Services;

public class AuditService : IAuditService
{
    private readonly IDbContextFactory<CrmDbContext> _dbFactory;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(IDbContextFactory<CrmDbContext> dbFactory, AuthenticationStateProvider authStateProvider, IHttpContextAccessor httpContextAccessor)
    {
        _dbFactory = dbFactory;
        _authStateProvider = authStateProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(string accion, string entidad, Guid? entidadId = null, string? detalle = null, Guid? empresaId = null)
    {
        try
        {
            var state = await _authStateProvider.GetAuthenticationStateAsync();
            var user = state.User;
            Guid? userId = Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId)
                ? parsedUserId
                : null;
            Guid? empresaIdClaim = Guid.TryParse(user.FindFirst("EmpresaId")?.Value, out var parsedEmpresaId)
                ? parsedEmpresaId
                : null;

            await using var db = await _dbFactory.CreateDbContextAsync();
            db.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresaId ?? empresaIdClaim,
                UsuarioId = userId,
                Accion = accion,
                Entidad = entidad,
                EntidadId = entidadId,
                Detalle = detalle,
                Fecha = DateTime.UtcNow,
                IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
        catch
        {
        }
    }
}
