using MundoVs.Core.Entities;

namespace MundoVs.Core.Entities.Auth;

public class ModuloAcceso : BaseEntity
{
    public string Clave { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int Orden { get; set; }
    public bool EsGlobal { get; set; }

    public ICollection<Capacidad> Capacidades { get; set; } = [];
    public ICollection<EmpresaModuloAcceso> Empresas { get; set; } = [];
}

public class EmpresaModuloAcceso : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Entities.Empresa Empresa { get; set; } = null!;

    public Guid ModuloAccesoId { get; set; }
    public ModuloAcceso ModuloAcceso { get; set; } = null!;

    public bool Habilitado { get; set; } = true;
    public DateTime VigenteDesde { get; set; } = DateTime.UtcNow.Date;
    public DateTime? VigenteHasta { get; set; }
    public OrigenModuloEmpresa Origen { get; set; } = OrigenModuloEmpresa.Sistema;
}

public enum OrigenModuloEmpresa
{
    Sistema = 1,
    Plan = 2,
    Manual = 3,
    Override = 4
}
