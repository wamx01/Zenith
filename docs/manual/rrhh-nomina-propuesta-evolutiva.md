# RRHH - Propuesta evolutiva de nómina

## Objetivo
Completar el modelo actual de nómina reutilizando la base ya implementada en `MundoVs`, agregando soporte escalable para provisiones por concepto, reglas recurrentes por empleado y separación explícita entre:
- neto pagado al empleado;
- obligaciones a terceros;
- aportaciones patronales;
- provisiones y reservas;
- costo total empresa.

## Resumen ejecutivo
La recomendación **no** es rehacer nómina. El sistema ya cuenta con una base útil y consistente para continuar:
- `Nomina` y `NominaDetalle` como cabecera y resultado por empleado;
- `Prenomina` y `PrenominaDetalle` como fuente operativa;
- cálculo de `CuotaImssObrera`, `CuotaImssPatronal`, `RetencionIsr`, `SubsidioEmpleo` y `MontoInfonavit`;
- provisiones base en `NominaDetalle` mediante `AguinaldoProvision` y `PrimaVacacionalProvision`;
- captura estructurada de percepciones y deducciones mediante `NominaPercepcion` y `NominaDeduccion`.

Lo faltante no es la deducción en sí, sino:
1. reglas recurrentes por empleado;
2. provisiones configurables por concepto;
3. separación funcional y visual de terceros, patronales y reservas;
4. cierre de costo empresa.

## Estado actual reutilizable

### Entidades actuales que se deben conservar
- `Nomina`
- `NominaDetalle`
- `Prenomina`
- `PrenominaDetalle`
- `NominaPercepcionTipo`
- `NominaPercepcion`
- `DeduccionTipoRrhh`
- `NominaDeduccion`
- `NominaConfiguracion`
- `NominaConfiguracionGlobal`

### Campos actuales ya útiles en `NominaDetalle`
- `CuotaImssObrera`
- `CuotaImssPatronal`
- `RetencionIsr`
- `SubsidioEmpleo`
- `MontoInfonavit`
- `AguinaldoProvision`
- `PrimaVacacionalProvision`

### Conclusión de reutilización
- Las deducciones existentes sí sirven para descuentos fijos y variables.
- Las percepciones existentes sí sirven para ajustes e ingresos manuales.
- IMSS e ISR base ya existen a nivel resultado.
- La parte faltante es la capa de configuración recurrente y el desglose de provisiones/terceros/costo patronal.

## Aclaración legal y funcional

### ISR
En la propuesta, **ISR no tiene aportación patronal** como concepto separado equivalente al IMSS patronal.

Lo que existe en nómina frente a SAT es principalmente:
- `ISR retenido al trabajador`;
- subsidio al empleo cuando aplique;
- enteros y obligaciones fiscales derivadas del cálculo, pero no una cuota patronal ISR igual al esquema de IMSS.

### IMSS
Sí debe separarse en:
- `CuotaImssObrera`;
- `CuotaImssPatronal`.

Actualmente eso ya existe en el modelo.

### Otros conceptos patronales que conviene contemplar
Además del IMSS patronal, la propuesta deja preparada la arquitectura para:
- aportación patronal relacionada con vivienda/seguridad social si se modela después;
- impuesto estatal sobre nómina;
- reservas laborales;
- PTU provisionada;
- otras obligaciones patronales futuras.

## Propuesta funcional completa

### Bloque 1. Neto del empleado
Debe seguir representando lo que realmente cobra el trabajador:
- sueldo base;
- destajo;
- bonos;
- percepciones manuales;
- subsidio al empleo;
- menos deducciones;
- menos descuento por minutos;
- menos IMSS obrero;
- menos ISR retenido;
- menos Infonavit;
- menos descuentos fijos u otros conceptos del empleado.

### Bloque 2. Obligaciones a terceros
Debe verse separado del neto:
- ISR retenido por enterar al SAT;
- IMSS obrero retenido;
- IMSS patronal;
- Infonavit retenido;
- otros enteros a terceros.

### Bloque 3. Aportaciones patronales
Debe existir explícitamente como subtotal distinto de provisiones:
- IMSS patronal;
- aportaciones patronales futuras que se modelen;
- impuesto sobre nómina si aplica.

### Bloque 4. Provisiones y reservas
No deben mezclarse con deducciones ni con pago a terceros del período:
- aguinaldo;
- vacaciones;
- prima vacacional;
- PTU;
- bono anual;
- provisiones fijas o especiales.

### Bloque 5. Costo empresa
Debe poder calcularse y verse como:
- percepciones del período
- `+` aportaciones patronales
- `+` provisiones
- `=` costo empresa.

## Nuevas tablas recomendadas

### 1. `rrhh_nomina_concepto_config`
Catálogo configurable por empresa para conceptos recurrentes y escalables.

#### Uso
Define si un concepto es:
- deducción;
- obligación;
- provisión;
- ajuste.

También define cómo se calcula:
- monto fijo;
- porcentaje;
- cantidad por tarifa;
- fórmula;
- manual.

#### Clase sugerida
`NominaConceptoConfigRrhh`

#### Campos sugeridos
- `Id`
- `EmpresaId`
- `Clave`
- `Nombre`
- `Naturaleza`
- `Destino`
- `TipoCalculo`
- `MontoFijoDefault`
- `PorcentajeDefault`
- `CantidadDefault`
- `TarifaDefault`
- `Orden`
- `EsRecurrente`
- `AplicaPorEmpleado`
- `AfectaNetoEmpleado`
- `AfectaCostoEmpresa`
- `AfectaPasivoSat`
- `AfectaPasivoImss`
- `AfectaProvision`
- `AfectaBaseIsr`
- `AfectaBaseImss`
- `EsLegal`
- `Activo`
- `Observaciones`

### 2. `rrhh_empleado_concepto`
Asignación por empleado de conceptos recurrentes.

#### Uso
Permite configurar por persona:
- descuento fijo;
- descuento porcentual;
- ajuste de seguridad social;
- provisión específica;
- conceptos vigentes por fecha.

#### Clase sugerida
`EmpleadoConceptoRrhh`

#### Campos sugeridos
- `Id`
- `EmpresaId`
- `EmpleadoId`
- `ConceptoConfigId`
- `Monto`
- `Porcentaje`
- `Cantidad`
- `Tarifa`
- `Saldo`
- `Limite`
- `FechaInicio`
- `FechaFin`
- `EsRecurrente`
- `Activo`
- `Observaciones`

### 3. `rrhh_nomina_provision_detalle`
Tabla separada para provisiones por recibo y concepto.

#### Justificación
Es la opción más escalable porque evita seguir agregando columnas duras por cada nueva provisión.

#### Clase sugerida
`NominaProvisionDetalleRrhh`

#### Campos sugeridos
- `Id`
- `EmpresaId`
- `NominaDetalleId`
- `EmpleadoId`
- `ConceptoConfigId`
- `Importe`
- `BaseCalculo`
- `Cantidad`
- `Tarifa`
- `PeriodoInicio`
- `PeriodoFin`
- `EsAjusteManual`
- `Observaciones`
- `CreatedAt`
- `UpdatedAt`
- `IsActive`

#### Ejemplos de conceptos en esta tabla
- `PROV_AGUINALDO`
- `PROV_VACACIONES`
- `PROV_PRIMA_VACACIONAL`
- `PROV_PTU`
- `PROV_BONO_ANUAL`
- `PROV_FONDO_FIJO`

## Reglas de reutilización con lo actual

### Deducciones fijas
No requieren un sistema nuevo de resultados.

#### Configuración
- se definen en `rrhh_nomina_concepto_config`;
- se asignan al empleado en `rrhh_empleado_concepto`.

#### Resultado
Al calcular la nómina, se traducen a:
- `NominaDeduccion`;
- incremento en `NominaDetalle.Deducciones`.

### Percepciones recurrentes
Se pueden resolver igual que las deducciones recurrentes, pero traducidas al resultado existente:
- `NominaPercepcion`;
- y acumuladas en los importes del detalle.

### Provisiones
Se generan como renglones en `rrhh_nomina_provision_detalle` y luego se reflejan en los totales del detalle.

## Qué hacer con IMSS fijo

### Regla principal
**No se debe modelar un IMSS legal fijo.**

El IMSS legal sigue calculándose por fórmula y debe conservarse en:
- `CuotaImssObrera`;
- `CuotaImssPatronal`.

### Si el negocio quiere un descuento fijo adicional
Debe modelarse como concepto aparte, por ejemplo:
- `AJUSTE_FIJO_SEGURIDAD_SOCIAL`;
- `DESCUENTO_FIJO_EMPLEADO`.

#### Si solo afecta neto
- Naturaleza: `Deduccion`
- Destino: `Empleado` o `OtroTercero`

#### Si realmente se enterará a IMSS
- Naturaleza: `Obligacion`
- Destino: `Imss`

Así no se contamina el cálculo legal del IMSS.

## Tipos de provisión soportados

### Provisión por monto fijo
Ejemplos:
- reserva fija mensual;
- bono garantizado;
- fondo administrativo fijo.

Fórmula:
- `Importe = MontoFijo`

### Provisión por porcentaje
Ejemplos:
- PTU estimada;
- provisión sobre sueldo base;
- bono anual porcentual.

Fórmula:
- `Importe = Base * Porcentaje`

### Provisión por cantidad por tarifa
Ejemplos:
- vacaciones devengadas;
- horas por tarifa;
- eventos por monto unitario.

Fórmula:
- `Importe = Cantidad * Tarifa`

## Ajustes sugeridos a `NominaDetalle`
La recomendación es conservar los campos actuales y agregar totales de lectura y cierre:
- `TotalObligacionesTerceros`
- `TotalAportacionesPatronales`
- `TotalProvisiones`
- `CostoEmpresa`

### Observación importante
`CuotaImssPatronal` ya existe, por lo que la parte patronal de IMSS no falta en entidad base; lo que falta es:
- visualizarla mejor;
- sumarla a subtotales patronales;
- reflejarla en costo empresa;
- agruparla con otras obligaciones patronales futuras.

## Qué mostrar en UI

### En resumen de nómina
Agregar bloques visibles:
1. `Neto empleado`
2. `Obligaciones a terceros`
3. `Aportaciones patronales`
4. `Provisiones`
5. `Costo empresa`

### En recibo del empleado
El recibo debe seguir mostrando solo lo que corresponde al trabajador:
- percepciones;
- deducciones;
- neto.

No conviene mezclar en el recibo del empleado:
- provisiones;
- costo empresa;
- reservas patronales.

### En vista administrativa
Sí conviene mostrar:
- ISR retenido;
- IMSS obrero;
- IMSS patronal;
- Infonavit;
- provisiones por concepto;
- costo empresa.

## Propuesta de enums

### `NaturalezaConceptoNominaRrhh`
- `Deduccion`
- `Obligacion`
- `Provision`
- `Ajuste`

### `DestinoConceptoNominaRrhh`
- `Empleado`
- `Sat`
- `Imss`
- `Reserva`
- `OtroTercero`

### `TipoCalculoConceptoNominaRrhh`
- `MontoFijo`
- `Porcentaje`
- `CantidadPorTarifa`
- `Formula`
- `Manual`

## Flujo recomendado de cálculo
1. calcular percepciones base desde nómina actual;
2. aplicar percepciones recurrentes configuradas por empleado;
3. calcular IMSS, ISR, subsidio e Infonavit;
4. aplicar deducciones recurrentes configuradas por empleado;
5. generar provisiones en `rrhh_nomina_provision_detalle`;
6. recalcular subtotales de terceros, patronales y provisiones;
7. persistir neto y costo empresa.

## Impacto funcional esperado

### Beneficios
- no duplica el modelo ya existente;
- mantiene compatibilidad con recibos actuales;
- hace escalables las provisiones;
- habilita descuentos fijos recurrentes;
- permite separar claramente empleado, terceros, patronal y reservas;
- deja lista la base para crecimiento legal y contable por cliente.

### Cambios mínimos obligatorios
- nueva tabla de conceptos configurables;
- nueva tabla de asignación por empleado;
- nueva tabla de provisiones por detalle;
- nuevos subtotales en `NominaDetalle` o en modelo de resumen administrativo.

## Recomendación final
Implementar en este orden:
1. `rrhh_nomina_concepto_config`;
2. `rrhh_empleado_concepto`;
3. `rrhh_nomina_provision_detalle`;
4. ampliar `NominaDetalle` con totales administrativos;
5. ampliar resumen UI de RRHH;
6. dejar el recibo del empleado sin mezclar componentes patronales.

## Dónde se va a configurar

### Configuración central por empresa
La pantalla base para administrar esta evolución debe ser:
- `/configuracion/nomina`

Ahí deben vivir:
- catálogo de conceptos de nómina configurables;
- banderas de afectación fiscal y operativa;
- provisiones configurables por concepto;
- conceptos recurrentes que luego puedan asignarse a empleados.

### Configuración por empleado
La asignación concreta de conceptos recurrentes debe quedar en una fase posterior dentro del flujo de RRHH por empleado, usando la nueva tabla `rrhh_empleado_concepto`.

Mientras tanto, la fuente maestra y funcional del catálogo sigue siendo `/configuracion/nomina`.

## Siguiente paso técnico recomendado
A partir de esta propuesta, el siguiente entregable debería ser:
- clases C# de las tres nuevas entidades;
- configuración EF Core en `CrmDbContext`;
- migración;
- ajuste de `NominaCalculator` para poblar provisiones y subtotales;
- ajuste de páginas RRHH para mostrar terceros, patronales y costo empresa.
