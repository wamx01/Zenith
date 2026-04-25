# Propuesta técnica formal — corte de nómina, festivos, bonos, comisiones, destajo manual y deducciones

## Objetivo
Definir un plan formal de implementación para fortalecer `RRHH` y `Nómina` en `MundoVs`, cubriendo estos vacíos funcionales:

1. configuración del día de corte de nómina semanal,
2. catálogo y aplicación de días festivos,
3. motor de bonos con dos modalidades,
4. captura manual de comisiones y destajo para nómina,
5. catálogo de deducciones y captura estructurada por detalle de nómina.

La propuesta se alinea con:
- `docs/conocimiento/nomina-marco-legal.md`
- `docs/noma-proposal.md`
- `docs/nomina-checklist.md`

---

## Resumen ejecutivo
Hoy el módulo ya soporta una nómina base con:
- sueldo fijo,
- destajo desde vales,
- bono manual simple,
- deducción manual simple,
- prima vacacional,
- IMSS obrero básico,
- prenómina como fuente operativa de incidencias.

Sin embargo, todavía faltan piezas críticas para que el proceso sea entendible, configurable y operativo en escenarios reales:
- el período semanal no debe asumirse fijo; debe depender del día de corte de cada empresa,
- los días festivos deben parametrizarse para reflejar pago especial y asistencia,
- los bonos necesitan una estructura reusable y no solo importes sueltos,
- comisiones y destajos manuales deben quedar trazables por concepto,
- las deducciones deben dejar de ser un solo monto libre y pasar a un catálogo tipificado.

La recomendación es evolucionar la nómina hacia un modelo por **conceptos** y dejar a `NominaDetalle` como resumen acumulado calculado.

---

# 1. Alcance funcional

## 1.1 Día de corte de nómina semanal
Se requiere permitir que la empresa configure el día de corte para nómina semanal.

Ejemplos válidos:
- lunes a domingo,
- martes a lunes,
- miércoles a martes.

### Resultado esperado
El sistema debe:
- calcular automáticamente el rango semanal a partir de una fecha base y el día de corte configurado,
- usar ese rango para:
  - prenómina,
  - asistencias,
  - horas extra,
  - destajo,
  - bonos,
  - comisiones,
  - deducciones.

## 1.2 Días festivos
Se requiere un catálogo de días festivos para:
- identificar descanso obligatorio,
- reflejarlo en asistencia,
- aplicar pago especial en nómina,
- separar festivo trabajado vs festivo no trabajado.

## 1.3 Bonos
Se requieren dos modalidades:

### A. Bono por rubro con cantidad
Ejemplo:
- puntualidad = `300`
- productividad = `500`
- asistencia = `200`

### B. Bono total distribuido por rubro y porcentaje
Ejemplo:
- total bono = `1,000`
- puntualidad = `30%`
- productividad = `50%`
- asistencia = `20%`

Resultado:
- puntualidad = `300`
- productividad = `500`
- asistencia = `200`

## 1.4 Comisiones y destajo manual para nómina
Se requiere un apartado manual inicial para capturar:
- comisiones,
- destajos manuales,
- ajustes extraordinarios.

Esto no sustituye el destajo desde producción; lo complementa.

## 1.5 Deducciones y tipos de deducción
Se requiere tipificar deducciones.

Ejemplos:
- préstamo,
- Infonavit,
- caja de ahorro,
- descuento por faltante,
- descuento por herramienta,
- pensión alimenticia,
- otras deducciones.

Cada deducción debe poder capturarse como concepto individual, con importe y observación.

---

# 2. Diseño funcional propuesto

## 2.1 Enfoque general
La nómina debe separarse en dos capas:

### Capa 1: Configuración maestra por empresa
Parámetros reutilizables:
- día de corte semanal,
- catálogo de festivos,
- catálogo de rubros de bono,
- catálogo de tipos de deducción,
- configuración de distribución de bono cuando aplique.

### Capa 2: Captura operativa por período y empleado
Movimientos del período:
- incidencias,
- percepciones,
- bonos,
- comisiones,
- destajo manual,
- deducciones.

---

# 3. Modelo de datos propuesto

## 3.1 Configuración de corte semanal
### Tabla propuesta
`rrhh_nomina_corte`

### Campos sugeridos
- `Id`
- `EmpresaId`
- `PeriodicidadPago`
- `DiaCorteSemana`
- `DiaPagoSugerido`
- `Activo`
- `CreatedAt`
- `UpdatedAt`

### Nota
Para primera fase puede haber solo una configuración activa por empresa para la periodicidad `Semanal`.

---

## 3.2 Catálogo de días festivos
### Tabla propuesta
`rrhh_festivo`

### Campos sugeridos
- `Id`
- `EmpresaId`
- `Fecha`
- `Nombre`
- `Tipo`
- `AplicaPrimaEspecial`
- `FactorPago`
- `EsOficial`
- `Activo`
- `CreatedAt`
- `UpdatedAt`

### Tipos sugeridos
- `Oficial`
- `Empresa`
- `Electoral`
- `Regional`

### Regla inicial
`FactorPago` puede iniciar en `2.0` para festivo trabajado, conforme a LFT art. 75.

---

## 3.3 Catálogo de rubros de bono
### Tabla propuesta
`rrhh_bono_rubro`

### Campos sugeridos
- `Id`
- `EmpresaId`
- `Clave`
- `Nombre`
- `Descripcion`
- `Orden`
- `Activo`
- `CreatedAt`
- `UpdatedAt`

Ejemplos:
- `PUNTUALIDAD`
- `ASISTENCIA`
- `PRODUCTIVIDAD`
- `CALIDAD`
- `META_SEMANAL`

---

## 3.4 Definición de bono por empleado o período
### Tabla propuesta
`rrhh_nomina_bono`

### Campos sugeridos
- `Id`
- `EmpresaId`
- `NominaId` nullable
- `NominaDetalleId` nullable
- `EmpleadoId` nullable
- `TipoCaptura`
- `MontoTotal`
- `Observaciones`
- `Activo`
- `CreatedAt`
- `UpdatedAt`

### `TipoCaptura`
- `PorRubroImporte`
- `TotalDistribuido`

### Tabla hija
`rrhh_nomina_bono_detalle`

### Campos sugeridos
- `Id`
- `NominaBonoId`
- `RubroBonoId`
- `Porcentaje`
- `Importe`
- `Orden`

### Regla
- si `TipoCaptura = PorRubroImporte`, el usuario captura `Importe` por rubro,
- si `TipoCaptura = TotalDistribuido`, el usuario captura `MontoTotal` y porcentajes; el sistema calcula importes.

---

## 3.5 Percepciones manuales de nómina
### Tabla propuesta
`rrhh_nomina_percepcion_tipo`

### Campos sugeridos
- `Id`
- `EmpresaId`
- `Clave`
- `Nombre`
- `Categoria`
- `AfectaBaseImss`
- `AfectaBaseIsr`
- `Orden`
- `Activo`

### Categorías iniciales
- `Comision`
- `DestajoManual`
- `BonoManual`
- `OtroIngreso`

### Tabla operativa
`rrhh_nomina_percepcion`

### Campos sugeridos
- `Id`
- `NominaDetalleId`
- `TipoPercepcionId`
- `Descripcion`
- `Importe`
- `Origen`
- `Referencia`
- `Observaciones`
- `CreatedAt`
- `UpdatedAt`
- `Activo`

### Orígenes sugeridos
- `Manual`
- `Produccion`
- `Ajuste`
- `Importacion`

---

## 3.6 Catálogo de deducciones
### Tabla propuesta
`rrhh_deduccion_tipo`

### Campos sugeridos
- `Id`
- `EmpresaId`
- `Clave`
- `Nombre`
- `Descripcion`
- `EsLegal`
- `AfectaRecibo`
- `Orden`
- `Activo`

Ejemplos:
- `INFONAVIT`
- `PRESTAMO`
- `CAJA_AHORRO`
- `HERRAMIENTA`
- `PENSION`
- `OTRAS`

### Tabla operativa
`rrhh_nomina_deduccion`

### Campos sugeridos
- `Id`
- `NominaDetalleId`
- `TipoDeduccionId`
- `Descripcion`
- `Importe`
- `EsRetencionLegal`
- `Observaciones`
- `CreatedAt`
- `UpdatedAt`
- `Activo`

---

# 4. Impacto sobre entidades existentes

## 4.1 `NominaDetalle`
`NominaDetalle` debe conservarse como resumen calculado, pero dejar de depender de campos sueltos para todo.

### Recomendación
Mantener temporalmente:
- `Bonos`
- `Deducciones`
- `MontoDestajo`

Pero evolucionar a que esos valores se calculen desde:
- `rrhh_nomina_bono(_detalle)`
- `rrhh_nomina_percepcion`
- `rrhh_nomina_deduccion`

### Nuevo criterio de cálculo
- `MontoBono` = suma de bonos estructurados
- `Bonos` = usar solo como compatibilidad temporal o para migración
- `MontoDestajo` = producción + destajo manual cuando corresponda
- `Deducciones` = suma de deducciones tipificadas no IMSS

---

# 5. Cambios en UI propuestos

## 5.1 `Admin/ConfiguracionNomina.razor`
Agregar sección:

### Corte semanal
- periodicidad,
- día de corte,
- día de pago sugerido.

### Festivos
- lista de festivos,
- alta/edición rápida,
- marcar si genera pago especial.

## 5.2 Catálogos nuevos
Crear páginas o submódulos compactos para:
- `configuracion/bonos-rubros`
- `configuracion/deducciones-tipos`
- opcional: `configuracion/percepciones-tipos`

## 5.3 `RRHH/Prenominas.razor`
Ajustes propuestos:
- mostrar si el período incluye festivos,
- sugerir incidencias de festivo trabajado,
- consolidar corte semanal con base en configuración.

## 5.4 `RRHH/Nominas.razor`
Agregar por empleado:
- sección de bonos estructurados,
- sección de percepciones manuales,
- sección de deducciones tipificadas,
- desglose claro del neto.

### UX recomendada
Usar panel expandible o modal por empleado con pestañas:
- `Resumen`
- `Bonos`
- `Percepciones`
- `Deducciones`
- `Recibo`

---

# 6. Reglas de negocio propuestas

## 6.1 Corte semanal
- el período semanal no se captura libremente cuando exista configuración activa,
- el sistema propone `FechaInicio` y `FechaFin` según el día de corte,
- si el corte es lunes, el período puede cerrar lunes 23:59:59 y abrir martes,
- la prenómina y nómina deben compartir exactamente el mismo rango.

## 6.2 Festivos
- si un festivo cae dentro del período, debe reflejarse en prenómina,
- si el empleado trabajó festivo, marcar `DiasFestivoTrabajado` o evento equivalente,
- el monto derivado puede modelarse como percepción específica de festivo trabajado.

## 6.3 Bonos
### Tipo A — por rubro importe
`Total bono = suma(importes de rubros)`

### Tipo B — total distribuido
`Importe rubro = MontoTotal × (Porcentaje / 100)`

Validaciones:
- en distribución porcentual, la suma debe ser `100%`,
- no permitir importes negativos,
- dejar observación obligatoria cuando el bono sea manual extraordinario.

## 6.4 Comisiones y destajo manual
- deben capturarse como percepciones independientes,
- destajo manual no debe reemplazar automáticamente el destajo de producción,
- el total de destajo del detalle puede ser:
  - `destajo producción + destajo manual`.

## 6.5 Deducciones
- toda deducción debe tener tipo,
- deducciones legales deben quedar diferenciadas de otras deducciones,
- IMSS obrero sigue siendo cálculo propio del motor, no captura manual ordinaria.

---

# 7. Motor de cálculo propuesto

## 7.1 Recomendación arquitectónica
Crear o ampliar un servicio tipo:
- `NominaCalculator`
- o `NominaConceptoCalculator`

## 7.2 Flujo sugerido
1. obtener rango por corte semanal,
2. cargar prenómina consolidada,
3. cargar festivos del período,
4. cargar vales y destajo producción,
5. cargar bonos estructurados,
6. cargar percepciones manuales,
7. cargar deducciones tipificadas,
8. calcular IMSS/ISR cuando aplique,
9. recalcular neto y resumen,
10. persistir resumen en `NominaDetalle`.

## 7.3 Fórmula temporal sugerida del neto
`Neto = sueldo base + destajo producción + destajo manual + comisiones + bonos + prima vacacional + complemento salario mínimo + horas extra + otras percepciones - IMSS obrero - deducciones tipificadas - otras retenciones`

---

# 8. Plan de implementación por fases

## Vista general recomendada

La secuencia sugerida para implementarlo sin romper la nómina actual es esta:

1. parametrización del período,
2. reglas base del período,
3. conceptos positivos estructurados,
4. conceptos negativos estructurados,
5. refactor del cálculo,
6. integración legal fina.

La prioridad no es agregar primero más campos a `NominaDetalle`, sino construir primero la estructura que permita calcular y auditar el detalle.

## Fase 1 — Parametrización base
### Objetivo
Cerrar configuración estructural sin tocar todavía cálculo fiscal complejo.

### Entregables
- `rrhh_nomina_corte`
- `rrhh_festivo`
- UI en `ConfiguracionNomina.razor`
- helpers para calcular rango semanal por día de corte

### Trabajo puntual
- agregar configuración activa de corte semanal por empresa,
- permitir seleccionar día de corte y día sugerido de pago,
- crear catálogo de festivos por empresa,
- soportar festivos oficiales y festivos internos,
- recalcular automáticamente `FechaInicio` y `FechaFin` al crear prenómina o nómina semanal.

### Criterio de salida
- una prenómina semanal ya no depende de captura manual ambigua del rango,
- el sistema puede saber si un período contiene festivos,
- las pantallas de prenómina y nómina usan el mismo helper de rango.

### Resultado
La prenómina y nómina ya no dependerán de semanas “manuales” ambiguas.

---

## Fase 2 — Bonos estructurados
### Entregables
- `rrhh_bono_rubro`
- `rrhh_nomina_bono`
- `rrhh_nomina_bono_detalle`
- UI de captura por rubro o por total distribuido
- cálculo de sumatorias en `NominaDetalle`

### Trabajo puntual
- crear catálogo de rubros reutilizables,
- permitir bono por rubro con importe directo,
- permitir bono total con distribución porcentual,
- validar suma de porcentajes al `100%`,
- reflejar el bono en el recibo y en el neto.

### Criterio de salida
- el usuario puede capturar bonos sin usar un solo campo libre,
- cada bono queda trazable por rubro,
- `MontoBono` ya puede calcularse desde estructura y no solo desde captura plana.

### Resultado
Bonos trazables, auditables y reutilizables.

---

## Fase 3 — Percepciones manuales
### Entregables
- `rrhh_nomina_percepcion_tipo`
- `rrhh_nomina_percepcion`
- captura de:
  - comisión,
  - destajo manual,
  - bono manual,
  - otro ingreso.

### Trabajo puntual
- crear catálogo de tipos de percepción,
- capturar comisiones manuales por empleado,
- capturar destajo manual por empleado,
- separar claramente percepción manual vs destajo de producción,
- agregar origen y referencia para auditoría.

### Criterio de salida
- el usuario puede meter comisión y destajo manual desde nómina,
- esos importes ya no se pierden en campos genéricos,
- el resumen del empleado distingue producción vs manual.

### Resultado
La nómina puede absorber operaciones manuales sin deformar campos generales.

---

## Fase 4 — Deducciones tipificadas
### Entregables
- `rrhh_deduccion_tipo`
- `rrhh_nomina_deduccion`
- captura estructurada de deducciones
- desglose visible en recibo y pantalla de nómina

### Trabajo puntual
- crear catálogo de tipos de deducción,
- permitir múltiples deducciones por empleado en un mismo período,
- marcar deducciones legales vs internas,
- reflejar el desglose en la UI de nómina,
- mostrar el desglose en el recibo imprimible.

### Criterio de salida
- desaparece la dependencia de un solo monto libre de deducciones,
- cada descuento tiene tipo, importe y observación,
- el neto ya se explica visualmente por conceptos.

### Resultado
Deducciones limpias y trazables por tipo.

---

## Fase 5 — Integración con cálculo legal más fino
### Entregables
- festivo trabajado como percepción formal,
- prima dominical,
- vínculo con ISR,
- vínculo con CFDI nómina,
- compatibilidad con Infonavit.

### Trabajo puntual
- convertir festivo trabajado en percepción estructurada,
- soportar prima dominical,
- preparar compatibilidad con tablas ISR y subsidio,
- preparar deducción Infonavit tipificada,
- alinear desglose con recibo y futura salida CFDI.

### Criterio de salida
- el sistema ya tiene una base compatible con cumplimiento legal más fino,
- los conceptos estructurados pueden mapearse después a CFDI nómina,
- el cálculo legal ya no depende de campos improvisados.

### Resultado
Base preparada para cumplimiento legal más completo.

---

## Fase 6 — Refactor del motor de nómina

### Objetivo
Mover la lógica desde campos sueltos a un cálculo consolidado por conceptos.

### Entregables
- `NominaCalculator` o `NominaConceptoCalculator`
- pipeline de cálculo por período
- resumen final persistido en `NominaDetalle`
- integración de recibo con conceptos estructurados

### Trabajo puntual
- recalcular percepciones desde bonos, comisiones, destajo manual y otras percepciones,
- recalcular deducciones desde catálogo estructurado,
- mantener compatibilidad temporal con campos legacy,
- migrar `Nominas.razor` a una captura por conceptos,
- centralizar el neto en un solo servicio.

### Criterio de salida
- la UI deja de calcular por su cuenta importes sensibles,
- el neto proviene de un motor central,
- `NominaDetalle` queda como resumen calculado y no como origen principal.

---

## Fase 7 — Cierre operativo y auditoría

### Objetivo
Dejar el módulo listo para operar con trazabilidad clara.

### Entregables
- bitácora de cambios por concepto
- validaciones de duplicidad
- simulación previa al cierre
- validaciones de período cerrado

### Trabajo puntual
- impedir capturas duplicadas por concepto cuando aplique,
- registrar quién agregó o modificó bonos, percepciones y deducciones,
- permitir vista previa antes de recalcular,
- bloquear cambios en nómina aprobada o pagada.

### Criterio de salida
- la operación es auditable,
- el usuario entiende qué cambió y por qué cambió el neto,
- el cierre de nómina ya no depende de memoria operativa.

---

# 9. Priorización recomendada

## Sprint 1
- corte semanal configurable,
- cálculo de período automático,
- catálogo de festivos.

## Sprint 2
- catálogo de rubros de bono,
- captura de bonos por rubro,
- captura de bono total distribuido.

## Sprint 3
- percepciones manuales:
  - comisiones,
  - destajo manual,
  - otros ingresos.

## Sprint 4
- tipos de deducción,
- deducciones estructuradas,
- ajuste de recibo y desglose en nómina.

## Sprint 5
- integración fina con cálculo legal y recibo/CFDI.

---

# 9.1 Roadmap por fases recomendado

## Fase A — Base de período
Duración sugerida: `1 sprint`

Incluye:
- corte semanal,
- festivos,
- helpers de rango,
- ajuste de prenómina y nómina para usar el mismo rango.

## Fase B — Bonos
Duración sugerida: `1 sprint`

Incluye:
- rubros de bono,
- bono por importe,
- bono por porcentaje sobre total,
- desglose visible en recibo.

## Fase C — Percepciones manuales
Duración sugerida: `1 sprint`

Incluye:
- comisiones,
- destajo manual,
- otros ingresos,
- integración al neto.

## Fase D — Deducciones
Duración sugerida: `1 sprint`

Incluye:
- catálogo de tipos,
- captura múltiple,
- desglose por recibo,
- integración al neto.

## Fase E — Motor central de cálculo
Duración sugerida: `1 sprint`

Incluye:
- servicio central de cálculo,
- compatibilidad temporal con campos legacy,
- recálculo estructurado.

## Fase F — Cierre legal fino
Duración sugerida: `1-2 sprints`

Incluye:
- festivo trabajado,
- prima dominical,
- ISR,
- subsidio,
- Infonavit,
- futura conexión con CFDI nómina.

---

# 10. Riesgos y decisiones

## Riesgos
- mezclar captura libre con cálculo automático puede duplicar importes,
- no separar catálogos de movimientos puede volver opaca la nómina,
- si el corte semanal cambia sin reglas de vigencia, puede afectar históricos.

## Decisiones recomendadas
- toda configuración de corte debe tener vigencia,
- todos los conceptos manuales deben quedar auditables,
- `NominaDetalle` debe leerse como resumen, no como única fuente de verdad operativa,
- los festivos deben parametrizarse en tabla y no hardcodearse.

---

# 11. Plan ejecutable resumido

## Paso 1
Implementar `rrhh_nomina_corte` y ajustar `ConfiguracionNomina.razor`.

## Paso 2
Implementar `rrhh_festivo` y vincularlo a prenómina/nómina.

## Paso 3
Implementar `rrhh_bono_rubro`, `rrhh_nomina_bono` y `rrhh_nomina_bono_detalle`.

## Paso 4
Implementar `rrhh_nomina_percepcion_tipo` y `rrhh_nomina_percepcion`.

## Paso 5
Implementar `rrhh_deduccion_tipo` y `rrhh_nomina_deduccion`.

## Paso 6
Refactorizar `Nominas.razor` para operar por conceptos.

## Paso 7
Actualizar `ReciboNomina.razor` para mostrar desglose estructurado.

---

# 12. Recomendación final
La propuesta correcta no es seguir agregando más campos aislados a `NominaDetalle`, sino mover el módulo a un esquema de:
- configuración por empresa,
- catálogos de conceptos,
- movimientos por empleado/período,
- detalle calculado como resumen.

Ese enfoque permite crecer después hacia:
- ISR,
- subsidio al empleo,
- Infonavit,
- CFDI nómina,
- bonos más complejos,
- trazabilidad y auditoría completas.

---

# 13. Checklist técnico por fase

## Fase A — Base de período
- [ ] Crear entidad `NominaCorteRrhh` o equivalente para `rrhh_nomina_corte`
- [ ] Crear entidad `FestivoRrhh` o equivalente para `rrhh_festivo`
- [ ] Configurar tablas en `CrmDbContext`
- [ ] Crear migración inicial de corte semanal y festivos
- [ ] Agregar sección de corte semanal en `MundoVs/Components/Pages/Admin/ConfiguracionNomina.razor`
- [ ] Agregar sección de festivos en `MundoVs/Components/Pages/Admin/ConfiguracionNomina.razor`
- [ ] Crear helper de rango semanal por `DiaCorteSemana`
- [ ] Reutilizar el helper en `RRHH/Prenominas.razor`
- [ ] Reutilizar el helper en `RRHH/Nominas.razor`
- [ ] Mostrar aviso cuando el período contiene festivos

## Fase B — Bonos
- [ ] Crear entidad `BonoRubroRrhh` o equivalente para `rrhh_bono_rubro`
- [ ] Crear entidad `NominaBono` para `rrhh_nomina_bono`
- [ ] Crear entidad `NominaBonoDetalle` para `rrhh_nomina_bono_detalle`
- [ ] Configurar relaciones en `CrmDbContext`
- [ ] Crear migración de rubros y bonos
- [ ] Crear catálogo UI `configuracion/bonos-rubros`
- [ ] Agregar captura de bono por rubro en `RRHH/Nominas.razor`
- [ ] Agregar captura de bono total distribuido en `RRHH/Nominas.razor`
- [ ] Validar suma de porcentajes al `100%`
- [ ] Reflejar bonos estructurados en `ReciboNomina.razor`

## Fase C — Percepciones manuales
- [ ] Crear entidad `NominaPercepcionTipo` para `rrhh_nomina_percepcion_tipo`
- [ ] Crear entidad `NominaPercepcion` para `rrhh_nomina_percepcion`
- [ ] Configurar catálogo de categorías: comisión, destajo manual, bono manual, otro ingreso
- [ ] Crear migración de percepciones manuales
- [ ] Crear UI opcional `configuracion/percepciones-tipos`
- [ ] Agregar captura de comisiones en `RRHH/Nominas.razor`
- [ ] Agregar captura de destajo manual en `RRHH/Nominas.razor`
- [ ] Agregar captura de otros ingresos en `RRHH/Nominas.razor`
- [ ] Distinguir destajo producción vs destajo manual en el resumen
- [ ] Reflejar percepciones manuales en `ReciboNomina.razor`

## Fase D — Deducciones
- [ ] Crear entidad `DeduccionTipoRrhh` para `rrhh_deduccion_tipo`
- [ ] Crear entidad `NominaDeduccion` para `rrhh_nomina_deduccion`
- [ ] Configurar catálogo base de deducciones
- [ ] Crear migración de deducciones tipificadas
- [ ] Crear catálogo UI `configuracion/deducciones-tipos`
- [ ] Agregar captura múltiple de deducciones en `RRHH/Nominas.razor`
- [ ] Separar deducciones legales de deducciones internas
- [ ] Reflejar desglose de deducciones en `ReciboNomina.razor`
- [ ] Mantener compatibilidad temporal con `NominaDetalle.Deducciones`

## Fase E — Motor central de cálculo
- [ ] Crear servicio `NominaCalculator` o `NominaConceptoCalculator`
- [ ] Crear modelos auxiliares de cálculo por concepto
- [ ] Mover cálculo de neto fuera de la UI
- [ ] Consolidar bonos estructurados dentro del cálculo
- [ ] Consolidar percepciones manuales dentro del cálculo
- [ ] Consolidar deducciones tipificadas dentro del cálculo
- [ ] Mantener sincronización temporal con campos legacy de `NominaDetalle`
- [ ] Recalcular resumen antes de guardar nómina
- [ ] Mostrar desglose calculado desde servicio en `RRHH/Nominas.razor`

## Fase F — Cierre legal fino
- [ ] Mapear festivo trabajado a percepción formal
- [ ] Agregar prima dominical
- [ ] Preparar tabla/configuración de ISR por periodicidad
- [ ] Preparar subsidio al empleo
- [ ] Preparar compatibilidad con Infonavit
- [ ] Preparar mapeo futuro a CFDI nómina

## Fase G — Auditoría y cierre operativo
- [ ] Registrar bitácora por cambios en bonos
- [ ] Registrar bitácora por cambios en percepciones
- [ ] Registrar bitácora por cambios en deducciones
- [ ] Bloquear edición cuando la nómina esté aprobada o pagada
- [ ] Agregar simulación previa al cierre
- [ ] Agregar validaciones de duplicidad por concepto

---

# 14. Backlog inicial de implementación

## Epic 1 — Parametrización del período

### Historia 1.1
Como usuario de nómina quiero configurar el día de corte semanal para que el sistema proponga automáticamente el rango correcto del período.

#### Tareas
- [ ] crear entidad y tabla `rrhh_nomina_corte`
- [ ] agregar configuración en `ConfiguracionNomina.razor`
- [ ] crear helper `GetWeeklyRange(...)`
- [ ] usar helper en creación de prenómina
- [ ] usar helper en creación de nómina

### Historia 1.2
Como usuario de RRHH quiero capturar días festivos para que el sistema identifique períodos especiales y pagos extraordinarios.

#### Tareas
- [ ] crear entidad y tabla `rrhh_festivo`
- [ ] crear UI de alta/edición en configuración de nómina
- [ ] agregar consulta de festivos por rango
- [ ] reflejar festivos en prenómina

## Epic 2 — Bonos estructurados

### Historia 2.1
Como usuario quiero definir rubros de bono reutilizables para no capturar siempre texto libre.

#### Tareas
- [ ] crear `rrhh_bono_rubro`
- [ ] crear catálogo UI
- [ ] precargar rubros base opcionales

### Historia 2.2
Como usuario quiero capturar bonos por importe directo por rubro para asignar montos específicos al empleado.

#### Tareas
- [ ] crear `rrhh_nomina_bono`
- [ ] crear `rrhh_nomina_bono_detalle`
- [ ] agregar captura por rubro con importe
- [ ] sumar el resultado al neto

### Historia 2.3
Como usuario quiero capturar un bono total y repartirlo por porcentaje entre rubros para agilizar la captura.

#### Tareas
- [ ] agregar modalidad `TotalDistribuido`
- [ ] validar suma de porcentajes
- [ ] calcular importes automáticos por rubro
- [ ] mostrar el desglose en recibo

## Epic 3 — Percepciones manuales

### Historia 3.1
Como usuario quiero capturar comisiones manuales para integrarlas al pago del empleado.

#### Tareas
- [ ] crear `rrhh_nomina_percepcion_tipo`
- [ ] crear `rrhh_nomina_percepcion`
- [ ] agregar tipo `Comision`
- [ ] capturar importe, referencia y observaciones

### Historia 3.2
Como usuario quiero capturar destajo manual para registrar pagos extraordinarios fuera del flujo automático de producción.

#### Tareas
- [ ] agregar tipo `DestajoManual`
- [ ] distinguirlo del destajo de producción
- [ ] sumar ambos en el resumen total

## Epic 4 — Deducciones estructuradas

### Historia 4.1
Como usuario quiero administrar tipos de deducción para registrar descuentos con trazabilidad.

#### Tareas
- [ ] crear `rrhh_deduccion_tipo`
- [ ] crear catálogo UI
- [ ] definir tipos base

### Historia 4.2
Como usuario quiero capturar múltiples deducciones por empleado para dejar claro de dónde sale el descuento total.

#### Tareas
- [ ] crear `rrhh_nomina_deduccion`
- [ ] agregar captura múltiple en `Nominas.razor`
- [ ] reflejar el desglose en `ReciboNomina.razor`

## Epic 5 — Motor de cálculo

### Historia 5.1
Como sistema quiero calcular el neto desde conceptos estructurados para evitar lógica duplicada en UI.

#### Tareas
- [ ] crear servicio central de cálculo
- [ ] centralizar percepciones
- [ ] centralizar deducciones
- [ ] actualizar resumen en `NominaDetalle`

### Historia 5.2
Como usuario quiero ver un desglose consistente del neto en pantalla y recibo para entender cómo se formó el pago.

#### Tareas
- [ ] consumir resultado del motor en `Nominas.razor`
- [ ] consumir resultado del motor en `ReciboNomina.razor`
- [ ] mostrar fórmula y conceptos aplicados

## Epic 6 — Cierre y auditoría

### Historia 6.1
Como usuario quiero que el sistema registre cambios por concepto para poder auditar quién modificó la nómina.

#### Tareas
- [ ] registrar auditoría de bonos
- [ ] registrar auditoría de percepciones
- [ ] registrar auditoría de deducciones

### Historia 6.2
Como usuario quiero bloquear ediciones en nóminas cerradas para proteger la información final.

#### Tareas
- [ ] bloquear edición en estatus `Aprobada`
- [ ] bloquear edición en estatus `Pagada`
- [ ] permitir solo consulta y recibo

---

# 15. Orden recomendado de ejecución real

## Opción recomendada

1. `rrhh_nomina_corte`
2. `rrhh_festivo`
3. `rrhh_deduccion_tipo` y `rrhh_nomina_deduccion`
4. `rrhh_nomina_percepcion_tipo` y `rrhh_nomina_percepcion`
5. `rrhh_bono_rubro`, `rrhh_nomina_bono`, `rrhh_nomina_bono_detalle`
6. `NominaCalculator`
7. refactor UI de `Nominas.razor`
8. refactor de `ReciboNomina.razor`
9. auditoría y bloqueos

## Justificación
- primero se define el período,
- luego se estructura el lado negativo de la nómina,
- después el lado positivo manual,
- luego bonos,
- y al final se concentra el cálculo en un motor central.

Ese orden reduce el riesgo de romper el cálculo actual mientras se migra de campos legacy a conceptos estructurados.
