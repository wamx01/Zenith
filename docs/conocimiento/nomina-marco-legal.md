# Marco legal — Módulo de Nómina

> **Propósito de este documento:** Referencia normativa para el desarrollo del módulo de nómina en el ERP. Cada sección indica la ley, los artículos relevantes, el resumen de la obligación y el impacto directo en el sistema.

---

## Índice

1. [Ley Federal del Trabajo (LFT)](#1-ley-federal-del-trabajo-lft)
2. [Ley del Seguro Social (LSS) — Cuotas IMSS](#2-ley-del-seguro-social-lss--cuotas-imss)
3. [Ley del INFONAVIT](#3-ley-del-infonavit)
4. [Ley del Impuesto Sobre la Renta (LISR) — Retención ISR](#4-ley-del-impuesto-sobre-la-renta-lisr--retención-isr)
5. [CFDI de Nómina — Complemento SAT versión 1.2](#5-cfdi-de-nómina--complemento-sat-versión-12)
6. [Unidad de Medida y Actualización (UMA)](#6-unidad-de-medida-y-actualización-uma)
7. [Salario Mínimo — CONASAMI](#7-salario-mínimo--conasami)
8. [Resolución Miscelánea Fiscal (RMF)](#8-resolución-miscelánea-fiscal-rmf)
9. [Resumen de tablas y valores que el sistema debe parametrizar](#9-resumen-de-tablas-y-valores-que-el-sistema-debe-parametrizar)
10. [Referencias oficiales](#10-referencias-oficiales)

---

## 1. Ley Federal del Trabajo (LFT)

**Fuente oficial:** https://www.diputados.gob.mx/LeyesBiblio/pdf/LFT.pdf

### 1.1 Salario y tipos de salario

| Artículo | Concepto | Resumen |
|----------|----------|---------|
| Art. 82 | Definición de salario | Es la retribución que el patrón paga al trabajador por su trabajo. Incluye cuota diaria, percepciones, habitación, primas, comisiones, prestaciones en especie y cualquier otra cantidad o prestación que se entregue al trabajador. |
| Art. 83 | Salario a destajo | El salario puede fijarse por unidad de tiempo, por unidad de obra (destajo), por comisión, a precio alzado o de cualquier otra manera. |
| Art. 84 | Salario Diario Integrado (SDI) | El salario incluye cuota diaria más la parte proporcional de gratificaciones, percepciones, habitación, primas, comisiones, prestaciones en especie y cualquier otra cantidad o prestación. **Este artículo es la base de cálculo del SDI que alimenta las cuotas IMSS.** |
| Art. 85 | Salario remunerador | El salario debe ser remunerador y nunca menor al fijado como mínimo. En el destajo, la cuantía de cada unidad debe asegurar un ingreso mínimo igual al salario mínimo. |
| Art. 86 | Igualdad de salario | A trabajo igual, jornada y condiciones iguales, corresponde salario igual, sin importar sexo o nacionalidad. |
| Art. 88 | Periodicidad de pago | Los plazos para el pago del salario nunca pueden ser mayores a una semana para quienes realizan trabajo material, y de quince días para los demás trabajadores. |
| Art. 89 | Base de cálculo de indemnizaciones | Para determinar indemnizaciones se toma como base el salario correspondiente al día en que nazca el derecho, incluyendo la cuota diaria y la parte proporcional de las prestaciones. |

> **Impacto en el sistema:** El campo `SueldoSemanal` en la entidad `Empleado` no es suficiente. El sistema debe calcular el SDI automáticamente sumando la cuota diaria más el proporcional de gratificaciones (aguinaldo), prima vacacional y demás prestaciones. El SDI debe almacenarse o calcularse antes de cerrar cualquier nómina.

---

### 1.2 Jornada de trabajo y horas extra

| Artículo | Concepto | Resumen |
|----------|----------|---------|
| Art. 58 | Jornada de trabajo | Tiempo durante el cual el trabajador está a disposición del patrón para prestar su trabajo. |
| Art. 59 | Distribución de la jornada | El trabajador y el patrón pueden distribuir las horas de trabajo. |
| Art. 60 | Tipos de jornada | Diurna: entre 6:00 y 20:00 hrs. Nocturna: entre 20:00 y 6:00 hrs. Mixta: abarca períodos de la diurna y nocturna, siempre que el período nocturno sea menor de 3.5 horas. |
| Art. 61 | Duración máxima | Jornada diurna: 8 horas. Nocturna: 7 horas. Mixta: 7.5 horas. |
| Art. 66 | Horas extra permitidas | Pueden prolongarse la jornada por circunstancias extraordinarias sin exceder 3 horas diarias ni 3 veces a la semana. |
| Art. 67 | Pago de horas extra | Las primeras 9 horas extra a la semana se pagan al doble del salario por hora ordinaria (100% de recargo). |
| Art. 68 | Límite y triple pago | Las horas extra que excedan de 9 horas a la semana deben pagarse con un 200% de recargo (triple del salario ordinario). El patrón no puede exigir más de 3 horas diarias ni 3 veces por semana. |

> **Impacto en el sistema:** La configuración en `AppConfigs` debe almacenar: horas base por jornada según tipo (diurna/nocturna/mixta), factor de horas extra doble (2.0) y factor de horas extra triple (3.0). `Nominas.razor` debe aplicar la lógica: primeras 9 hrs/semana × 2, excedente × 3.
>
> Para control de asistencia operativo, la jornada del empleado debe poder modelarse por turno y por día con entrada, salida y descansos configurables. El sistema debe distinguir descansos pagados y no pagados para calcular tiempo neto trabajado y horas extra sobre una base consistente con la jornada real configurada por la empresa.

---

### 1.3 Días de descanso y días festivos

| Artículo | Concepto | Resumen |
|----------|----------|---------|
| Art. 69 | Descanso semanal | Por cada seis días de trabajo el trabajador tiene derecho a un día de descanso, como mínimo, con goce de salario íntegro. |
| Art. 71 | Prima dominical | Los trabajadores que presten servicio en su día de descanso, independientemente del salario que les corresponda, tienen derecho a un prima adicional de 25% sobre el salario de los días ordinarios. |
| Art. 74 | Días de descanso obligatorio | Son días de descanso obligatorio: 1 de enero, primer lunes de febrero (Constitución), tercer lunes de marzo (Natalicio de Benito Juárez), 1 de mayo, 16 de septiembre, tercer lunes de noviembre (Revolución), 1 de diciembre cada 6 años (transmisión del Ejecutivo), 25 de diciembre, y los que determinen las leyes federales y locales electorales. |
| Art. 75 | Pago en días festivos | Si el trabajador labora en día festivo obligatorio, se le pagará independientemente del salario que le corresponda por ese día, un salario doble. |

> **Impacto en el sistema:** El sistema debe tener un catálogo de días festivos oficiales actualizable. Al registrar asistencia o calcular nómina, debe identificar automáticamente si un día trabajado es festivo o día de descanso semanal y aplicar el recargo correspondiente. La **prenómina** debe ser la fuente operativa de incidencias del período: asistencias, faltas, incapacidades, descansos trabajados, domingos y días efectivamente trabajados.

---

### 1.4 Vacaciones y prima vacacional

| Artículo | Concepto | Resumen |
|----------|----------|---------|
| Art. 76 | Período vacacional | Los trabajadores que tengan más de un año de servicios disfrutarán de un período anual de vacaciones pagadas. **Reforma 2023:** primer año 12 días; segundo año 14 días; tercero 16; cuarto 18; quinto al noveno año, 20 días; del décimo al catorceavo, 22 días; del quinceavo al diecinueveavo, 24 días; del vigésimo al vigésimocuarto, 26 días; y así sucesivamente. |
| Art. 80 | Prima vacacional | Los trabajadores tendrán derecho a una prima no menor del 25% sobre los salarios que les correspondan durante el período de vacaciones. |
| Art. 81 | Época de vacaciones | Las vacaciones deberán concederse a los trabajadores dentro de los seis meses siguientes al cumplimiento del año de servicios. |

> **Impacto en el sistema:** El cálculo proporcional de prima vacacional entra en el SDI (Art. 84). La fórmula es: `(días_vacaciones × 0.25 × salario_diario) / 365`. Este valor diario se suma al SDI base del trabajador. Operativamente, la prenómina debe capturar vacaciones disfrutadas en el período, distinguirlas de faltas e incapacidades, y mantener saldo por antigüedad, días ganados, días usados y días pagados.

---

### 1.5 Aguinaldo

| Artículo | Concepto | Resumen |
|----------|----------|---------|
| Art. 87 | Aguinaldo | Los trabajadores tienen derecho a un aguinaldo anual que deberá pagarse antes del día 20 de diciembre, equivalente a 15 días de salario como mínimo. Los que no hayan cumplido el año de servicios, independientemente de que se encuentren laborando o no, tendrán derecho al pago proporcional. |

> **Impacto en el sistema:** El proporcional diario de aguinaldo también integra el SDI. La fórmula es: `(15 × salario_diario) / 365`. El sistema debe calcular y mostrar el aguinaldo acumulado del período al generar la nómina de diciembre.

---

### 1.6 Participación de los Trabajadores en las Utilidades (PTU)

| Artículo | Concepto | Resumen |
|----------|----------|---------|
| Art. 117–131 | PTU | Los trabajadores participarán en las utilidades de las empresas. El porcentaje lo fija la Comisión Nacional para la Participación de los Trabajadores en las Utilidades (actualmente 10% de la renta gravable). Debe repartirse dentro de los 60 días siguientes a la fecha en que deba pagarse el ISR anual. |
| Art. 123 | Distribución | Se divide en dos partes iguales: la primera se reparte por igual entre todos los trabajadores según días trabajados; la segunda en proporción a los salarios devengados. |

> **Impacto en el sistema:** La PTU no entra en el cálculo periódico de nómina pero sí debe registrarse como percepción exenta/gravada en el CFDI de nómina cuando se pague. Debe haber un tipo de percepción específico para PTU en el catálogo del SAT.

---

### 1.7 Trabajo a destajo — garantía de salario mínimo

| Artículo | Concepto | Resumen |
|----------|----------|---------|
| Art. 83 | Destajo y salario mínimo | Cuando el salario se fije por unidad de obra (destajo), la cuantía de cada unidad debe asegurar un ingreso mínimo igual al salario mínimo por jornada ordinaria. |
| Art. 85 | Salario remunerador | Nunca inferior al mínimo. En el destajo, si el trabajador no alcanza el mínimo por causas ajenas a él, el patrón debe cubrir la diferencia. |

> **Impacto en el sistema:** **Validación crítica.** Al cerrar un `ValeDestajo` o al calcular la nómina, el sistema debe comparar el total destajado del período con el salario mínimo proporcional a los días trabajados. Si el destajo es inferior, el sistema debe alertar y registrar la diferencia como complemento de salario. Esto aplica directamente a las fábricas de calzado en León. Los días trabajados que alimentan esta validación no deben capturarse manualmente en la nómina final; deben venir de la prenómina del período.

---

## 2. Ley del Seguro Social (LSS) — Cuotas IMSS

**Fuente oficial:** https://www.diputados.gob.mx/LeyesBiblio/pdf/LSS.pdf

### 2.1 Salario Base de Cotización (SBC)

| Artículo | Concepto | Resumen |
|----------|----------|---------|
| Art. 27 | Integración del SBC | El SBC se integra con los pagos hechos en efectivo por cuota diaria, gratificaciones, percepciones, alimentación, habitación, primas, comisiones, prestaciones en especie y cualquier otra cantidad o prestación que se entregue al trabajador. Es equivalente al SDI del Art. 84 LFT. |
| Art. 28 | Tope máximo del SBC | El SBC máximo para efectos del seguro de enfermedades y maternidad, invalidez y vida, guarderías, y cesantía/vejez es de **25 veces la UMA diaria vigente**. |
| Art. 28 A | Sin tope en retiro e INFONAVIT | Para el seguro de retiro y las aportaciones al INFONAVIT no existe tope superior. |
| Art. 29 | Tope mínimo del SBC | El SBC mínimo es el salario mínimo general vigente. |

> **Impacto en el sistema:** El sistema debe calcular el SBC para cada empleado afiliado al IMSS. Para los seguros con tope, aplicar: `SBC_efectivo = min(SBC_calculado, 25 × UMA_diaria)`. Para retiro e INFONAVIT usar el SBC sin tope. La UMA debe ser un parámetro actualizable en `AppConfigs`. El modelo debe permitir marcar por empleado si aplica seguridad social, porque en empresas pequeñas puede coexistir personal afiliado y no afiliado; cuando no aplique IMSS, la nómina debe omitir cuotas obreras/patronales y no asumir registro patronal en todos los casos.

---

### 2.2 Ramas del seguro y tasas de cotización

| Rama | Artículos | Patrón | Trabajador | Base | Notas |
|------|-----------|--------|------------|------|-------|
| Enfermedades y maternidad — cuota fija | Art. 106 fracc. I | 20.40% de 1 SMA | — | Cuota fija diaria por trabajador | SMA = Salario Mínimo Area geográfica A |
| Enfermedades y maternidad — excedente | Art. 106 fracc. II | 1.10% | 0.40% | SBC − 3 UMA | Solo sobre el excedente de 3 UMA |
| Enfermedades y maternidad — prestaciones en especie pensionados | Art. 25 LSS | 1.05% | 0.375% | SBC topado a 25 UMA | — |
| Invalidez y vida | Art. 147 | 1.75% | 0.625% | SBC topado | — |
| Retiro | Art. 168 fracc. I | 2.00% | — | SBC sin tope | Solo patrón |
| Cesantía y vejez | Art. 168 fracc. II | 3.150% | 1.125% | SBC topado | — |
| Guarderías y prestaciones sociales | Art. 211 | 1.00% | — | SBC topado | Solo patrón |
| Riesgo de trabajo | Art. 73 | Variable (prima SIPA) | — | SBC sin tope | Ver sección 2.3 |

> **Impacto en el sistema:** Las tasas de cotización deben almacenarse en `AppConfigs` o en un catálogo dedicado, actualizables sin modificar código. El sistema debe calcular la cuota obrera (deducción al trabajador) y la cuota patronal (costo para la empresa) de forma separada. La cuota obrera se muestra como deducción en el recibo de nómina. Esta capa de cálculo debe ejecutarse solo para empleados marcados como afiliados al IMSS; el sistema debe soportar nómina mixta dentro de la misma empresa.

---

### 2.3 Riesgos de trabajo — Prima SIPA

| Artículo | Concepto | Resumen |
|----------|----------|---------|
| Art. 72–76 | Sistema de Prima de Riesgo de Trabajo | La prima que cubren los patrones se determina conforme al Reglamento de la LSSS para la Clasificación de Empresas y Determinación de la Prima en el Seguro de Riesgos de Trabajo. |
| Art. 74 | Revisión anual | Los patrones deben revisar anualmente su siniestralidad y presentar la declaración anual de confirmación o modificación de prima en febrero. |
| Reglamento SIPA | Clases de riesgo | Las empresas se clasifican en 5 clases de riesgo (I al V). Para manufactura de calzado la clase suele ser III o IV. La prima varía entre 0.50% y 15% del SBC. |

> **Impacto en el sistema:** La prima SIPA de cada empresa cliente debe ser un parámetro configurable en `AppConfigs`. El sistema no puede asumir una prima fija; debe permitir que el administrador la actualice cada febrero tras presentar su declaración ante el IMSS.

---

## 3. Ley del INFONAVIT

**Fuente oficial:** https://www.diputados.gob.mx/LeyesBiblio/pdf/LINFONAVIT.pdf

| Artículo | Concepto | Resumen |
|----------|----------|---------|
| Art. 29 fracc. II | Aportación patronal | Los patrones deben aportar el equivalente al 5% del salario de cada trabajador a su cuenta individual del INFONAVIT. |
| Art. 29 fracc. III | Descuento de crédito | Si el trabajador tiene un crédito INFONAVIT activo, el patrón está obligado a descontar de su salario el pago de las amortizaciones y enterarlas al Instituto. El importe lo fija INFONAVIT en la resolución de crédito. |
| Art. 143 | Base de cotización | El salario para el cálculo de las aportaciones es el SBC sin tope máximo (igual que retiro IMSS). |

> **Impacto en el sistema:** La aportación del 5% es un costo patronal que no aparece en el recibo del trabajador (no es deducción). Si el trabajador tiene crédito activo, sí aparece como deducción y requiere un campo específico en la entidad `Empleado` o `NominaDetalle` para el número de crédito y el factor de descuento. En el CFDI de nómina va en el nodo `<Deducciones>` con el tipo `002 - ISR` no; con tipo específico de INFONAVIT del catálogo SAT.

---

## 4. Ley del Impuesto Sobre la Renta (LISR) — Retención ISR

**Fuente oficial:** https://www.diputados.gob.mx/LeyesBiblio/pdf/LISR.pdf

### 4.1 Obligación de retención

| Artículo | Concepto | Resumen |
|----------|----------|---------|
| Art. 94 | Ingresos gravados por salarios | Son ingresos por salarios y en general por la prestación de un servicio personal subordinado: salarios, sueldos, asimilados, prestaciones, PTU, viáticos no comprobados, y cualquier otro ingreso derivado de la relación laboral. |
| Art. 96 | Obligación del patrón de retener | Los patrones que hagan pagos por salarios están obligados a efectuar retenciones y enteros mensuales que tendrán carácter de pagos provisionales a cuenta del impuesto anual. |
| Art. 97 | Tabla de retención periódica | La retención se calcula aplicando la tarifa del Art. 96 al ingreso gravable del período (tabla de ISR mensual, quincenal o semanal publicada en el Anexo 8 de la RMF). |

### 4.2 Procedimiento de cálculo de retención (Art. 96)

El cálculo paso a paso que el sistema debe implementar:

```
1. INGRESO TOTAL DEL PERÍODO
   = Sueldo base + destajo + horas extra + bonos + otras percepciones

2. PERCEPCIONES EXENTAS (no se gravan)
   - Horas extra: exentas hasta 5 veces la UMA por semana
   - Aguinaldo: exento hasta 30 UMA anuales
   - Prima vacacional: exenta hasta 15 UMA anuales
   - Prima dominical: exenta hasta 1 UMA por domingo trabajado
   - PTU: exenta hasta 15 UMA anuales
   - Cajas de ahorro: exentas hasta 1.3 UMA anual

3. INGRESO GRAVABLE
   = Ingreso total − Percepciones exentas

4. APLICAR TARIFA DEL ART. 96 (tabla por período)
   ISR_periodo = Cuota fija + (Ingreso gravable − Límite inferior) × Tasa marginal

5. SUBSIDIO AL EMPLEO
   = Valor de la tabla de subsidio al empleo por rango de ingreso
   (Publicada en el Reglamento de la LISR y Anexo 8 RMF)

6. ISR A RETENER
   Si ISR_periodo > Subsidio: retener (ISR_periodo − Subsidio)
   Si ISR_periodo <= Subsidio: NO retener; el excedente es crédito al salario
```

> **Impacto en el sistema:** Las tablas de ISR y subsidio al empleo deben ser parámetros actualizables en `AppConfigs` (o en un catálogo dedicado), separados por periodicidad (semanal, quincenal, mensual). Se publican en el Anexo 8 de la RMF cada año. El sistema NO debe tener estas tablas hardcodeadas en el código.

### 4.3 Percepciones exentas — tabla de referencia

| Percepción | Artículo | Límite de exención |
|------------|----------|--------------------|
| Horas extra | Art. 93 fracc. I | Hasta 5 veces la UMA semanal |
| Aguinaldo | Art. 93 fracc. XIV | Hasta 30 UMA anuales |
| Prima vacacional | Art. 93 fracc. XIV | Hasta 15 UMA anuales |
| Prima dominical | Art. 93 fracc. XII | Hasta 1 UMA por domingo |
| PTU | Art. 93 fracc. XIV | Hasta 15 UMA anuales |
| Indemnizaciones | Art. 93 fracc. XIII | Hasta 90 UMA × años servicio |
| Vales de despensa | Art. 93 fracc. XIV | Hasta 40% de UMA mensual |

---

## 5. CFDI de Nómina — Complemento SAT versión 1.2

**Fuente oficial:**
- Guía de llenado: https://www.sat.gob.mx/cs/Satellite?blobcol=urldata&blobkey=id&blobtable=MungoBlobs&blobwhere=1461174964951
- Catálogos: https://www.sat.gob.mx/informacion_fiscal/factura_electronica/Paginas/catalogos_complemento_nomina.aspx
- XSD del complemento: https://www.sat.gob.mx/sitio_internet/cfd/nomina/nomina12.xsd

### 5.1 Estructura del nodo Nómina

El CFDI de nómina es un CFDI versión 4.0 con un complemento específico. Los nodos principales del complemento son:

```xml
<nomina12:Nomina
  Version="1.2"
  TipoNomina="O"              <!-- O = Ordinaria, E = Extraordinaria -->
  FechaPago="2024-12-15"
  FechaInicialPago="2024-12-09"
  FechaFinalPago="2024-12-15"
  NumDiasPagados="7"
  TotalPercepciones="3500.00"
  TotalDeducciones="485.20"
  TotalOtrosPagos="0.00">

  <nomina12:Emisor
    Curp="CURP del representante legal"
    RegistroPatronal="Número registro IMSS"
    RfcPatronOrigen="RFC del patrón"/>

  <nomina12:Receptor
    Curp="CURP del trabajador"
    NumSeguridadSocial="NSS"
    FechaInicioRelLaboral="2020-01-06"
    Antigüedad="P4Y"          <!-- Formato ISO 8601 duración -->
    TipoContrato="01"         <!-- Catálogo c_TipoContrato -->
    Sindicalizado="No"
    TipoJornada="01"          <!-- Catálogo c_TipoJornada -->
    TipoRegimen="02"          <!-- Catálogo c_TipoRegimen -->
    NumEmpleado="EMP-001"
    Departamento="Producción"
    Puesto="Operador"
    RiesgoPuesto="2"          <!-- Catálogo c_RiesgoPuesto -->
    PeriodicidadPago="02"     <!-- Catálogo c_PeriodicidadPago -->
    Banco="006"               <!-- Catálogo c_Banco, si aplica -->
    CuentaBancaria="CLABE 18 dígitos"
    SalarioBaseCotApor="200.00"
    SalarioDiarioIntegrado="230.50"
    ClaveEntFed="GTO"/>       <!-- Catálogo c_Estado -->

  <nomina12:Percepciones
    TotalSueldos="3200.00"
    TotalSeparacionIndemnizacion="0"
    TotalJubilacionPensionRetiro="0"
    TotalGravado="2850.00"
    TotalExento="650.00">

    <nomina12:Percepcion
      TipoPercepcion="001"    <!-- Catálogo c_TipoPercepcion -->
      Clave="001"
      Concepto="Sueldo"
      ImporteGravado="2800.00"
      ImporteExento="0"/>

  </nomina12:Percepciones>

  <nomina12:Deducciones
    TotalOtrasDeducciones="285.20"
    TotalImpuestosRetenidos="200.00">

    <nomina12:Deduccion
      TipoDeduccion="002"     <!-- 002 = ISR -->
      Clave="001"
      Concepto="ISR"
      Importe="200.00"/>

    <nomina12:Deduccion
      TipoDeduccion="001"     <!-- 001 = Seguridad Social -->
      Clave="002"
      Concepto="Cuota IMSS"
      Importe="85.20"/>

  </nomina12:Deducciones>

</nomina12:Nomina>
```

### 5.2 Catálogos SAT críticos para el módulo

| Catálogo | Descripción | Dónde se usa |
|----------|-------------|--------------|
| `c_TipoPercepcion` | Tipos de percepción (sueldo, horas extra, aguinaldo, destajo, bono, etc.) | Nodo `Percepcion` |
| `c_TipoDeduccion` | Tipos de deducción (ISR, IMSS, INFONAVIT, préstamos, etc.) | Nodo `Deduccion` |
| `c_TipoContrato` | Tipo de contratación (base, temporal, honorarios, etc.) | Nodo `Receptor` |
| `c_TipoJornada` | Tipo de jornada (diurna, nocturna, mixta, por hora, etc.) | Nodo `Receptor` |
| `c_TipoRegimen` | Régimen de contratación (asalariados, asimilados, etc.) | Nodo `Receptor` |
| `c_PeriodicidadPago` | Frecuencia de pago (diario, semanal, quincenal, mensual, etc.) | Nodo `Receptor` |
| `c_RiesgoPuesto` | Clase de riesgo del puesto (1 al 5) | Nodo `Receptor` |
| `c_Banco` | Clave de banco para CLABE | Nodo `Receptor` |
| `c_Estado` | Clave de entidad federativa | Nodo `Receptor` |
| `c_TipoOtroPago` | Subsidio al empleo, reintegro de ISR, etc. | Nodo `OtroPago` |

> **Impacto en el sistema:** La entidad `Empleado` debe tener campos para: CURP, NSS, RFC, número de empleado, fecha de inicio de relación laboral, tipo de contrato, tipo de jornada, tipo de régimen, riesgo del puesto, CLABE bancaria, banco y entidad federativa. Todos referenciados con las claves de los catálogos SAT.

### 5.3 Proceso de timbrado

El XML del CFDI de nómina generado por el sistema debe enviarse a un **PAC (Proveedor Autorizado de Certificación)** para su timbrado. El PAC valida el XML contra el XSD del SAT, agrega el sello fiscal y devuelve el XML timbrado (con UUID).

El sistema debe:
1. Generar el XML del complemento de nómina por cada `NominaDetalle`.
2. Firmarlo con el CSD (Certificado de Sello Digital) del patrón.
3. Enviarlo al PAC vía API (SOAP o REST, según el PAC).
4. Almacenar el UUID del timbre y el XML timbrado.
5. Generar la representación impresa (PDF) del recibo de nómina.

---

## 6. Unidad de Medida y Actualización (UMA)

**Fuente oficial:** https://www.inegi.org.mx/temas/uma/

| Concepto | Descripción |
|----------|-------------|
| Qué es | Unidad de referencia económica para determinar montos de obligaciones fiscales y de seguridad social. Sustituyó al salario mínimo como referencia en el IMSS e INFONAVIT desde 2017. |
| Quién la publica | INEGI. Se publica en el DOF durante febrero de cada año. |
| Vigencia | Del 1 de febrero de un año al 31 de enero del año siguiente. |
| Valor diario 2024 | $108.57 MXN (verificar valor actualizado en DOF para 2025-2026) |
| Valor mensual 2024 | $3,300.53 MXN |
| Valor anual 2024 | $39,606.36 MXN |

> **Impacto en el sistema:** La UMA debe almacenarse en `AppConfigs` con su fecha de vigencia. El sistema debe usar la UMA vigente al período de cálculo, no la del momento en que se desarrolló el módulo. Al iniciar cada año fiscal, el administrador debe actualizar este valor. El sistema puede mostrar una alerta si la UMA en `AppConfigs` tiene más de 365 días sin actualizarse.

---

## 7. Salario Mínimo — CONASAMI

**Fuente oficial:** https://www.gob.mx/conasami

| Concepto | Descripción |
|----------|-------------|
| Quién lo fija | Comisión Nacional de los Salarios Mínimos (CONASAMI). |
| Vigencia | Se publica en el DOF. Puede haber incrementos en enero y en julio de cada año. |
| Zona libre de la frontera norte | Existe un salario mínimo diferenciado para los municipios de la frontera norte (actualmente mayor que el general). |
| Salario mínimo general 2024 | $248.93 MXN diarios (general). Verificar vigente para el período de cálculo. |
| Salario mínimo frontera norte 2024 | $374.89 MXN diarios. |

> **Impacto en el sistema:** El salario mínimo debe ser un parámetro en `AppConfigs` diferenciado por zona (general / frontera norte). Se usa para: (1) validar que ningún empleado cobre menos del mínimo, (2) calcular la cuota fija del seguro de enfermedades y maternidad en el IMSS (cuota del patrón = 20.40% de 1 salario mínimo por trabajador por día).

---

## 8. Resolución Miscelánea Fiscal (RMF)

**Fuente oficial:** https://www.sat.gob.mx/consulta/65413/conoce-la-resolucion-miscelánea-fiscal

| Anexo | Contenido relevante para nómina |
|-------|----------------------------------|
| Anexo 8 | Tablas de ISR para retención periódica (mensual, quincenal, semanal, diaria) y tabla de subsidio al empleo. Se publican al inicio de cada año. |
| Anexo 20 | Especificaciones técnicas del CFDI versión 4.0, incluyendo el complemento de nómina. |
| Regla 3.13 | Disposiciones específicas sobre el cálculo anual del ISR por parte del patrón (ajuste de retenciones en diciembre). |

> **Impacto en el sistema:** Las tablas del Anexo 8 son las más sensibles al tiempo. Deben poder actualizarse en `AppConfigs` sin tocar código. Idealmente el sistema debe tener un módulo de administración de tablas fiscales donde se ingresen los rangos, cuotas fijas, tasas marginales y valores de subsidio al empleo por periodicidad y por año fiscal.

---

## 9. Resumen de tablas y valores que el sistema debe parametrizar

Esta sección consolida todos los parámetros que **no deben estar hardcodeados** en el código y que deben poder actualizarse desde `Admin/ConfiguracionNomina.razor` o desde `AppConfigs`.

| Parámetro | Fuente legal | Frecuencia de actualización |
|-----------|-------------|----------------------------|
| UMA diaria, mensual y anual | INEGI / DOF | Anual (febrero) |
| Salario mínimo general | CONASAMI / DOF | Anual o semestral |
| Salario mínimo frontera norte | CONASAMI / DOF | Anual o semestral |
| Tablas ISR por periodicidad (semanal, quincenal, mensual) | RMF Anexo 8 | Anual |
| Tabla de subsidio al empleo por periodicidad | RMF Anexo 8 | Anual |
| Tasas de cuotas IMSS (enfermedades, invalidez, retiro, cesantía, guarderías) | LSS Art. 106 y 168 | Raramente cambian; revisar anualmente |
| Prima de riesgo de trabajo (SIPA) por empresa | Reglamento SIPA | Anual (declaración febrero) |
| Factor de horas extra doble (2.0) | LFT Art. 67 | Estable |
| Factor de horas extra triple (3.0) | LFT Art. 68 | Estable |
| Días de aguinaldo mínimo (15) | LFT Art. 87 | Estable |
| Prima vacacional mínima (25%) | LFT Art. 80 | Estable |
| Tabla de días de vacaciones por antigüedad | LFT Art. 76 | Revisar tras reformas |
| Límites de exención por percepción (en UMA) | LISR Art. 93 | Anual |
| CSD del patrón (certificado y clave privada) | SAT | Cada 4 años o al renovar |
| URL y credenciales del PAC | Contrato con PAC | Al cambiar de PAC |

---

## 10. Referencias oficiales

| Documento | URL |
|-----------|-----|
| Ley Federal del Trabajo (LFT) | https://www.diputados.gob.mx/LeyesBiblio/pdf/LFT.pdf |
| Ley del Seguro Social (LSS) | https://www.diputados.gob.mx/LeyesBiblio/pdf/LSS.pdf |
| Reglamento SIPA — Riesgos de Trabajo | https://www.diputados.gob.mx/LeyesBiblio/pdf/RSIPASRT.pdf |
| Ley del INFONAVIT | https://www.diputados.gob.mx/LeyesBiblio/pdf/LINFONAVIT.pdf |
| Ley del ISR (LISR) | https://www.diputados.gob.mx/LeyesBiblio/pdf/LISR.pdf |
| Catálogos CFDI de Nómina (SAT) | https://www.sat.gob.mx/informacion_fiscal/factura_electronica/Paginas/catalogos_complemento_nomina.aspx |
| XSD Complemento Nómina 1.2 | https://www.sat.gob.mx/sitio_internet/cfd/nomina/nomina12.xsd |
| Guía de llenado Complemento Nómina | https://www.sat.gob.mx/cs/Satellite?blobcol=urldata&blobkey=id&blobtable=MungoBlobs&blobwhere=1461174964951 |
| UMA vigente (INEGI) | https://www.inegi.org.mx/temas/uma/ |
| Salario mínimo (CONASAMI) | https://www.gob.mx/conasami |
| Resolución Miscelánea Fiscal (RMF) | https://www.sat.gob.mx/consulta/65413/conoce-la-resolucion-miscelánea-fiscal |
| Diario Oficial de la Federación (DOF) | https://www.dof.gob.mx |
| Portal del IMSS — Cuotas | https://www.imss.gob.mx/patrones/cuotas |
| Portal del INFONAVIT — Patrones | https://www.infonavit.org.mx/trabajadores/patrones |

---

*Documento generado para el repositorio de conocimiento del módulo de nómina. Revisar y actualizar los valores numéricos (UMA, salario mínimo, tablas ISR) al inicio de cada año fiscal. Las URLs de fuentes oficiales son estables pero verificar vigencia ante posibles reestructuras de portales gubernamentales.*
