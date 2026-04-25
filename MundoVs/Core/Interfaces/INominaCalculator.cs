using MundoVs.Core.Entities;
using MundoVs.Core.Services;

namespace MundoVs.Core.Interfaces;

public interface INominaCalculator
{
    NominaCalculationResult Calculate(NominaCalculationInput input);
}
