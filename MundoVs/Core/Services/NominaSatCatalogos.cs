namespace MundoVs.Core.Services;

public sealed record NominaSatCatalogoItem(string Clave, string Descripcion, string? Uso = null);

public static class NominaSatCatalogos
{
    public static IReadOnlyList<NominaSatCatalogoItem> TiposNomina { get; } =
    [
        new("O", "Nómina Ordinaria", "Pagos periódicos"),
        new("E", "Nómina Extraordinaria", "Pagos no habituales")
    ];

    public static IReadOnlyList<NominaSatCatalogoItem> PeriodicidadesPago { get; } =
    [
        new("01", "Diario"), new("02", "Semanal"), new("03", "Catorcenal"), new("04", "Quincenal"), new("05", "Mensual"), new("06", "Bimestral"),
        new("07", "Unidad obra"), new("08", "Comisión"), new("09", "Precio alzado"), new("10", "Decenal"), new("99", "Otra Periodicidad")
    ];

    public static IReadOnlyList<NominaSatCatalogoItem> TiposRegimenContratacion { get; } =
    [
        new("02", "Sueldos"), new("03", "Jubilados"), new("04", "Pensionados"), new("05", "Asimilados Miembros Sociedades Cooperativas Producción"),
        new("06", "Asimilados Integrantes Sociedades Asociaciones Civiles"), new("07", "Asimilados Miembros consejos"), new("08", "Asimilados comisionistas"),
        new("09", "Asimilados Honorarios"), new("10", "Asimilados Acciones"), new("11", "Asimilados Otros"), new("12", "Jubilados o Pensionados"),
        new("13", "Indemnización o Separación"), new("99", "Otro Régimen")
    ];

    public static IReadOnlyList<NominaSatCatalogoItem> TiposContrato { get; } =
    [
        new("01", "Tiempo indeterminado"), new("02", "Obra determinada"), new("03", "Tiempo determinado"), new("04", "Temporada"),
        new("05", "Sujeto a prueba"), new("06", "Capacitación inicial"), new("07", "Pago por hora"), new("08", "Comisión laboral"),
        new("09", "Sin relación de trabajo"), new("10", "Jubilación, pensión, retiro"), new("99", "Otro contrato")
    ];

    public static IReadOnlyList<NominaSatCatalogoItem> TiposJornada { get; } =
    [
        new("01", "Diurna"), new("02", "Nocturna"), new("03", "Mixta"), new("04", "Por hora"), new("05", "Reducida"),
        new("06", "Continuada"), new("07", "Partida"), new("08", "Por turnos"), new("99", "Otra Jornada")
    ];

    public static IReadOnlyList<NominaSatCatalogoItem> RiesgoPuesto { get; } =
    [
        new("1", "Clase I"), new("2", "Clase II"), new("3", "Clase III"), new("4", "Clase IV"), new("5", "Clase V"), new("99", "No aplica")
    ];

    public static IReadOnlyDictionary<string, NominaSatCatalogoItem> TiposPercepcion { get; } = new Dictionary<string, NominaSatCatalogoItem>
    {
        ["001"] = new("001", "Sueldos, Salarios Rayas y Jornales"),
        ["002"] = new("002", "Gratificación Anual (Aguinaldo)"),
        ["003"] = new("003", "PTU"),
        ["004"] = new("004", "Reembolso de Gastos Médicos Dentales y Hospitalarios"),
        ["005"] = new("005", "Fondo de Ahorro"),
        ["006"] = new("006", "Caja de ahorro"),
        ["009"] = new("009", "Contribuciones a Cargo del Trabajador Pagadas por el Patrón"),
        ["010"] = new("010", "Premios por puntualidad"),
        ["011"] = new("011", "Prima de Seguro de vida"),
        ["012"] = new("012", "Seguro de Gastos Médicos Mayores"),
        ["013"] = new("013", "Cuotas Sindicales Pagadas por el Patrón"),
        ["014"] = new("014", "Subsidios por incapacidad"),
        ["015"] = new("015", "Becas para trabajadores y/o hijos"),
        ["019"] = new("019", "Horas extra"),
        ["020"] = new("020", "Prima dominical"),
        ["021"] = new("021", "Prima vacacional"),
        ["022"] = new("022", "Prima por antigüedad"),
        ["023"] = new("023", "Pagos por separación"),
        ["024"] = new("024", "Seguro de retiro"),
        ["025"] = new("025", "Indemnizaciones"),
        ["026"] = new("026", "Reembolso por funeral"),
        ["027"] = new("027", "Cuotas de seguridad social pagadas por el patrón"),
        ["028"] = new("028", "Comisiones"),
        ["029"] = new("029", "Vales de despensa"),
        ["030"] = new("030", "Vales de restaurante"),
        ["031"] = new("031", "Vales de gasolina"),
        ["032"] = new("032", "Vales de ropa"),
        ["033"] = new("033", "Ayuda para renta"),
        ["034"] = new("034", "Ayuda para artículos escolares"),
        ["035"] = new("035", "Ayuda para anteojos"),
        ["036"] = new("036", "Ayuda para transporte"),
        ["037"] = new("037", "Ayuda para gastos de funeral"),
        ["038"] = new("038", "Otros ingresos por salarios"),
        ["039"] = new("039", "Jubilaciones, pensiones o haberes de retiro"),
        ["044"] = new("044", "Jubilaciones, pensiones o haberes de retiro en parcialidades"),
        ["045"] = new("045", "Ingresos en acciones o títulos valor que representan bienes"),
        ["046"] = new("046", "Ingresos asimilados a salarios"),
        ["047"] = new("047", "Alimentación"),
        ["048"] = new("048", "Habitación"),
        ["049"] = new("049", "Premios por asistencia"),
        ["050"] = new("050", "Viáticos"),
        ["051"] = new("051", "Pagos por gratificaciones, primas, compensaciones, recompensas u otros a extrabajadores derivados de jubilación en parcialidades"),
        ["052"] = new("052", "Pagos a extrabajadores con jubilación en parcialidades por resolución judicial o laudo"),
        ["053"] = new("053", "Pagos a extrabajadores con jubilación en una sola exhibición por resolución judicial o laudo"),
        ["054"] = new("054", "Días de descanso laborados"),
        ["055"] = new("055", "Días de descanso obligatorios laborados")
    };

    public static IReadOnlyList<NominaSatCatalogoItem> TiposHoraExtra { get; } =
    [
        new("01", "Dobles"), new("02", "Triples"), new("03", "Simples")
    ];

    public static IReadOnlyList<NominaSatCatalogoItem> TiposIncapacidad { get; } =
    [
        new("01", "Riesgo de trabajo"), new("02", "Enfermedad en general"), new("03", "Maternidad"), new("04", "Licencia por cuidados médicos de hijos con cáncer")
    ];

    public static IReadOnlyDictionary<string, NominaSatCatalogoItem> TiposDeduccion { get; } = new Dictionary<string, NominaSatCatalogoItem>
    {
        ["001"] = new("001", "Seguridad social"), ["002"] = new("002", "ISR"), ["003"] = new("003", "Aportaciones a retiro, cesantía en edad avanzada y vejez"),
        ["004"] = new("004", "Otros"), ["005"] = new("005", "Aportaciones a Fondo de vivienda"), ["006"] = new("006", "Descuento por incapacidad"),
        ["007"] = new("007", "Pensión alimenticia"), ["008"] = new("008", "Renta"), ["009"] = new("009", "Préstamos provenientes del Fondo Nacional de la Vivienda para los Trabajadores"),
        ["010"] = new("010", "Pago por crédito de vivienda"), ["011"] = new("011", "Pago de abonos INFONACOT"), ["012"] = new("012", "Anticipo de salarios"),
        ["013"] = new("013", "Pagos hechos con exceso al trabajador"), ["014"] = new("014", "Errores"), ["015"] = new("015", "Pérdidas"), ["016"] = new("016", "Averías"),
        ["017"] = new("017", "Adquisición de artículos producidos por la empresa o establecimiento"), ["018"] = new("018", "Cuotas para sociedades cooperativas y cajas de ahorro"),
        ["019"] = new("019", "Cuotas sindicales"), ["020"] = new("020", "Ausencia (Ausentismo)"), ["021"] = new("021", "Cuotas obrero patronales"),
        ["022"] = new("022", "Impuestos Locales"), ["023"] = new("023", "Aportaciones voluntarias"), ["024"] = new("024", "Ajuste en Aguinaldo Exento"),
        ["025"] = new("025", "Ajuste en Aguinaldo Gravado"), ["026"] = new("026", "Ajuste en PTU Exento"), ["027"] = new("027", "Ajuste en PTU Gravado"),
        ["028"] = new("028", "Ajuste en Reembolso de Gastos Médicos Dentales y Hospitalarios Exento"), ["029"] = new("029", "Ajuste en Fondo de ahorro Exento"),
        ["030"] = new("030", "Ajuste en Caja de ahorro Exento"), ["031"] = new("031", "Ajuste en Contribuciones a Cargo del Trabajador Pagadas por el Patrón Exento"),
        ["032"] = new("032", "Ajuste en Premios por puntualidad Gravado"), ["033"] = new("033", "Ajuste en Prima de Seguro de vida Exento"),
        ["034"] = new("034", "Ajuste en Seguro de Gastos Médicos Mayores Exento"), ["035"] = new("035", "Ajuste en Cuotas Sindicales Pagadas por el Patrón Gravado"),
        ["036"] = new("036", "Ajuste en Subsidios por incapacidad Exento"), ["037"] = new("037", "Ajuste en Becas para trabajadores y/o hijos Exento"),
        ["038"] = new("038", "Ajuste en Horas extra Exento"), ["039"] = new("039", "Ajuste en Horas extra Gravado"), ["040"] = new("040", "Ajuste en Prima dominical Exento"),
        ["041"] = new("041", "Ajuste en Prima dominical Gravado"), ["042"] = new("042", "Ajuste en Prima vacacional Exento"), ["043"] = new("043", "Ajuste en Prima vacacional Gravado"),
        ["044"] = new("044", "Ajuste en Prima por antigüedad Exento"), ["045"] = new("045", "Ajuste en Prima por antigüedad Gravado"), ["046"] = new("046", "Ajuste en Pagos por separación Exento"),
        ["047"] = new("047", "Ajuste en Pagos por separación Gravado"), ["048"] = new("048", "Ajuste en Seguro de retiro Exento"), ["049"] = new("049", "Ajuste en Indemnizaciones Exento"),
        ["050"] = new("050", "Ajuste en Indemnizaciones Gravado"), ["051"] = new("051", "Ajuste en Reembolso por funeral Exento"), ["052"] = new("052", "Ajuste en Cuotas de seguridad social pagadas por el patrón Exento"),
        ["053"] = new("053", "Ajuste en Comisiones Gravado"), ["054"] = new("054", "Ajuste en Vales de despensa Exento"), ["055"] = new("055", "Ajuste en Vales de restaurante Exento"),
        ["056"] = new("056", "Ajuste en Vales de gasolina Exento"), ["057"] = new("057", "Ajuste en Vales de ropa Exento"), ["058"] = new("058", "Ajuste en Ayuda para renta Exento"),
        ["108"] = new("108", "Ajuste a días de descanso laborados gravados"), ["109"] = new("109", "Ajuste a días de descanso laborados exentos"),
        ["110"] = new("110", "Ajuste a días de descanso obligatorios laborados gravados"), ["111"] = new("111", "Ajuste a días de descanso obligatorios laborados exentos")
    };

    public static IReadOnlyList<NominaSatCatalogoItem> OtrosTiposPago { get; } =
    [
        new("001", "Reintegro de ISR pagado en exceso"), new("002", "Subsidio para el empleo"), new("003", "Viáticos"), new("004", "Aplicación de saldo a favor por compensación anual"),
        new("005", "Reintegro de ISR retenido en exceso de ejercicio anterior"), new("006", "Alimentos en bienes"), new("007", "ISR ajustado por subsidio"),
        new("008", "Subsidio efectivamente entregado que no correspondía"), new("009", "Reembolso de descuentos efectuados para el crédito de vivienda"), new("999", "Pagos distintos a los listados")
    ];

    public static IReadOnlyList<NominaSatCatalogoItem> BancosFrecuentes { get; } =
    [
        new("002", "Banamex"), new("012", "BBVA"), new("014", "Santander"), new("021", "HSBC"), new("036", "Inbursa"), new("044", "Scotiabank"), new("072", "Banorte")
    ];

    public static IReadOnlyList<NominaSatCatalogoItem> TipoOrigenRecurso { get; } =
    [
        new("IP", "Ingresos Propios"), new("IF", "Ingresos Federales"), new("IM", "Ingresos Mixtos")
    ];

    public static class Sistema
    {
        public const string PercepcionSueldos = "001";
        public const string PercepcionHorasExtra = "019";
        public const string PercepcionPrimaDominical = "020";
        public const string PercepcionPrimaVacacional = "021";
        public const string PercepcionComisiones = "028";
        public const string PercepcionValesDespensa = "029";
        public const string PercepcionPremioPuntualidad = "010";
        public const string PercepcionPremioAsistencia = "049";
        public const string PercepcionFondoAhorro = "005";
        public const string PercepcionCajaAhorro = "006";
        public const string PercepcionDescansoObligatorioLaborado = "055";
        public const string PercepcionOtrosIngresosSalarios = "038";

        public const string DeduccionSeguridadSocial = "001";
        public const string DeduccionIsr = "002";
        public const string DeduccionCreditoInfonavit = "009";
        public const string DeduccionAusentismo = "020";
    }
}
