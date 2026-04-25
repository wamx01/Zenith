namespace MundoVs.Core.Entities;

using MundoVs.Core.Entities.Auth;

public class Empresa
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Codigo { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreComercial { get; set; }
    public string? Rfc { get; set; }
    public string? Slogan { get; set; }
    public EmpresaEstado Estado { get; set; } = EmpresaEstado.Demo;
    public DateTime? TrialEndsAt { get; set; }
    public bool IsSuspended { get; set; }
    public Guid? PlanActualId { get; set; }
    public Plan? PlanActual { get; set; }
    public int? MaxUsuarios { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public bool AplicaSalarioMinimoFrontera { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<SuscripcionEmpresa> Suscripciones { get; set; } = new List<SuscripcionEmpresa>();
    public ICollection<EmpresaModuloAcceso> ModulosAcceso { get; set; } = new List<EmpresaModuloAcceso>();
}

public enum EmpresaEstado
{
    Demo = 0,
    Activa = 1,
    Suspendida = 2,
    Cancelada = 3
}
