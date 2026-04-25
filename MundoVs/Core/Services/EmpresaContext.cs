using MundoVs.Core.Interfaces;

namespace MundoVs.Core.Services;

public class EmpresaContext : IEmpresaContext
{
    public Guid EmpresaId { get; private set; }

    public void SetEmpresaId(Guid empresaId)
    {
        EmpresaId = empresaId;
    }
}
