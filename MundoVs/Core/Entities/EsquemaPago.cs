namespace MundoVs.Core.Entities;

public class EsquemaPago : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public TipoEsquemaPago Tipo { get; set; }
    public bool IncluyeSueldoBase { get; set; }
    public decimal? SueldoBaseSugerido { get; set; }

    // Bono por cumplimiento de pedidos
    public bool UsaMetaPorPedidos { get; set; }
    public decimal? BonoCumplimientoMonto { get; set; }
    public decimal? BonoAdelantoMonto { get; set; }
    public bool BonoRepartirPorSueldo { get; set; } = true;
    public bool BonoRepartirPorAsistencia { get; set; } = true;

    public ICollection<EsquemaPagoTarifa> Tarifas { get; set; } = [];
    public ICollection<EmpleadoEsquemaPago> Empleados { get; set; } = [];
}

public enum TipoEsquemaPago
{
    SueldoFijo = 1,
    DestajoPorPieza = 2,
    DestajoPorOperacion = 3,
    BonoMetaPedidos = 4,
    Mixto = 5
}
