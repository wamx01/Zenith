namespace Zenith.Contracts.Asistencia;

public sealed class AgenteConfiguracionDto
{
    public Guid EmpresaId { get; set; }
    public string NombreAgente { get; set; } = string.Empty;
    public int IntervaloSegundos { get; set; } = 60;
    public bool PermitirLecturaUsuarios { get; set; }
    public bool ModoDiagnostico { get; set; }
    public IReadOnlyCollection<ChecadorConfigDto> Checadores { get; set; } = [];
}
