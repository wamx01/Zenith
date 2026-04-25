namespace MundoVs.Core.Interfaces;

public interface IEmpresaContext
{
    Guid EmpresaId { get; }
    void SetEmpresaId(Guid empresaId);
}
