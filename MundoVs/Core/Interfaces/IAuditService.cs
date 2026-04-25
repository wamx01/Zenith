namespace MundoVs.Core.Interfaces;

public interface IAuditService
{
    Task LogAsync(string accion, string entidad, Guid? entidadId = null, string? detalle = null, Guid? empresaId = null);
}
