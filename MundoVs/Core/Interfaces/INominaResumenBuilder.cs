using MundoVs.Core.Services;

namespace MundoVs.Core.Interfaces;

public interface INominaResumenBuilder
{
    NominaResumenResult Build(NominaResumenInput input);
}
