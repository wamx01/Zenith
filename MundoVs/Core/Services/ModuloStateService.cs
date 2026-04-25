namespace MundoVs.Core.Services;

public class ModuloStateService
{
    public bool Calzado { get; private set; }
    public bool Serigrafia { get; private set; } = true;

    public event Action? OnChange;

    public void SetState(bool calzado, bool serigrafia)
    {
        Calzado = calzado;
        Serigrafia = serigrafia;
        OnChange?.Invoke();
    }
}
