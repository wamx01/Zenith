namespace MundoVs.Core.Entities
{
    public enum TipoClasificacionMarcacionRrhh
    {
        SinClasificar = 0,
        Entrada = 1,
        Salida = 2,
        InicioDescanso = 3,
        FinDescanso = 4
    }

    public class RrhhMarcacion : BaseEntity
    {
        public Guid EmpresaId { get; set; }
        public Empresa Empresa { get; set; } = null!;

        public Guid ChecadorId { get; set; }
        public RrhhChecador Checador { get; set; } = null!;

        public Guid? EmpleadoId { get; set; }
        public Empleado? Empleado { get; set; }

        public string CodigoChecador { get; set; } = string.Empty;
        public DateTime? FechaHoraMarcacionLocal { get; set; }
        public DateTime FechaHoraMarcacionUtc { get; set; }
        public string? ZonaHorariaAplicada { get; set; }
        public string? TipoMarcacionRaw { get; set; }
        public string? Origen { get; set; }
        public string? EventoIdExterno { get; set; }
        public string HashUnico { get; set; } = string.Empty;
        public bool EsManual { get; set; }
        public bool EsAnulada { get; set; }
        public TipoClasificacionMarcacionRrhh ClasificacionOperativa { get; set; }
        public bool Procesada { get; set; }
        public string? ResultadoProcesamiento { get; set; }
        public string? ObservacionManual { get; set; }
        public string? PayloadRaw { get; set; }
    }
}