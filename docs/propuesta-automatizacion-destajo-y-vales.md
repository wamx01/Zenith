# Propuesta formal: automatización de destajo, vales y nómina

> **Sistema:** `Zenith`
> 
> **Objetivo:** definir una mejora funcional para reducir captura manual, mejorar control operativo y alinear el flujo de destajo con buenas prácticas actuales en empresas de producción por operación.

---

## 1. Resumen ejecutivo

Hoy el sistema ya tiene una base sólida para operar producción por destajo, pero el flujo todavía depende de una etapa manual que genera duplicidad:

- la producción real se registra en seguimiento,
- pero el pago por destajo se vuelve a capturar en vales,
- y nómina depende de esos vales manuales.

La recomendación es evolucionar a un modelo donde:

1. el dato fuente sea el registro de producción,
2. el vale sea un documento de consolidación y aprobación,
3. nómina consuma vales aprobados o registros ya consolidados,
4. la tarifa salga del pedido/cotización y de la regla de operación, no del sueldo fijo del empleado como criterio principal.

---

## 2. Alcance del análisis

La propuesta se basa en la revisión del flujo actual en:

- `Components/Pages/Produccion/Serigrafia/PedidosSerigrafia.razor`
- `Components/Pages/Produccion/Serigrafia/PedidoSeguimiento.razor`
- `Components/Pages/RRHH/ValesDestajo.razor`
- `Components/Pages/RRHH/Nominas.razor`
- `Components/Pages/RRHH/Empleados.razor`
- `Components/Pages/RRHH/EsquemasPago.razor`

Y en entidades como:

- `RegistroDestajoProceso`
- `ValeDestajo`
- `ValeDestajoDetalle`
- `EmpleadoEsquemaPago`
- `EsquemaPago`
- `EsquemaPagoTarifa`
- `NominaDetalle`
- `PedidoSerigrafiaTallaProceso`

---

## 3. Estado actual (`AS-IS`)

## 3.1 Flujo actual real

```text
Pedido
  -> Pedido productivo
      -> Seguimiento por talla/proceso
          -> Registro manual por empleado en producción
              -> Captura manual del vale de destajo
                  -> Integración a nómina por periodo
```

## 3.2 Qué hace bien el sistema hoy

### Producción
- El pedido ya puede configurarse con:
  - producto,
  - cotización,
  - procesos,
  - tallas,
  - pagos,
  - estados del pedido.
- El seguimiento por talla/proceso ya permite:
  - marcar avance,
  - registrar fecha,
  - asignar empleado,
  - guardar `RegistroDestajoProceso` por participante.

### RRHH / Nómina
- Ya existe esquema de pago por empleado con vigencia.
- Ya existe vale con estatus y folio.
- Nómina ya jala vales aprobados/en nómina por periodo.

## 3.3 Problemas observados

### Problema 1: doble captura
El mismo hecho operativo puede registrarse dos veces:

- primero en `PedidoSeguimiento` como `RegistroDestajoProceso`,
- después en `ValesDestajo` como `ValeDestajoDetalle`.

**Impacto:**
- más trabajo administrativo,
- riesgo de error,
- diferencias entre producción y pago,
- auditoría más difícil.

### Problema 2: el vale es captura primaria, no cierre
Hoy el vale funciona como si fuera el origen del destajo pagable.

**Lo ideal:** que el vale sea el resumen/aprobación de algo ya registrado en piso.

### Problema 3: la lógica de tarifa no está unificada
Actualmente conviven varias fuentes de tarifa:

- tarifa por minuto de la posición,
- minutos estándar desde cotización,
- cálculo desde sueldo del empleado y horas base,
- existencia de `EsquemaPagoTarifa`, pero sin ser la pieza dominante del cálculo del vale.

**Impacto:**
- la tarifa puede ser correcta en una pantalla y distinta en otra,
- la explicación al supervisor o al operador se vuelve confusa,
- el esquema de pago queda subutilizado.

### Problema 4: nómina depende del vale manual
Aunque la producción ya genera `RegistroDestajoProceso`, nómina no liquida desde ahí.

**Impacto:**
- si no se arma el vale, la producción no se paga,
- existe cuello de botella administrativo.

### Problema 5: el esquema de pago está bien modelado, pero no totalmente aprovechado
El dominio soporta:
- sueldo fijo,
- destajo por pieza,
- destajo por operación,
- bono por meta,
- mixto.

Pero operativamente hoy el flujo fuerte es:
- sueldo base,
- vale manual,
- nómina.

Los bonos y la automatización fina todavía no están completos.

---

## 4. Evaluación contra estándar operativo

## 4.1 ¿El diseño actual es inválido?
No.

Para una empresa pequeña o en transición, es normal que exista:
- captura operativa,
- validación manual,
- vale como comprobante,
- integración a nómina por corte.

## 4.2 ¿Es el mejor estándar actual?
Tampoco.

En empresas más maduras, el estándar recomendable es:

### estándar recomendado
**captura única en piso + consolidación automática o semiautomática + aprobación + nómina**

Es decir:
- producción registra,
- supervisor valida,
- el sistema consolida,
- nómina paga.

## 4.3 Veredicto
- **Como etapa inicial:** aceptable.
- **Como estándar objetivo:** mejorable.
- **Como oportunidad de alto impacto:** sí, muy alta.

---

## 5. Estado objetivo (`TO-BE`)

## 5.1 Principio central
La fuente única de verdad del destajo debe ser el registro de producción.

### fuente primaria recomendada
- `RegistroDestajoProceso`

### documento de control
- `ValeDestajo`

### documento de pago
- `NominaDetalle`

## 5.2 Nuevo flujo propuesto

```text
Pedido
  -> Cotización con tiempos y costos
      -> Seguimiento de producción
          -> RegistroDestajoProceso por empleado
              -> Consolidación automática de vales
                  -> Aprobación por supervisor
                      -> Integración automática a nómina
                          -> Pago
```

## 5.3 Rol del vale en el modelo propuesto
El vale no desaparece.

Cambia de función:

### hoy
- captura manual del trabajo hecho

### propuesto
- consolidación,
- revisión,
- ajuste excepcional,
- aprobación,
- trazabilidad.

---

## 6. Reglas funcionales recomendadas

## 6.1 Regla de origen de tarifa
La tarifa de destajo no debería salir principalmente del sueldo fijo semanal del empleado.

### recomendación
La tarifa debe derivarse de:
1. cotización del pedido,
2. tiempo estándar por proceso,
3. regla de pago del proceso/operación,
4. perfil o excepción del operador si aplica.

### interpretación práctica
- la cotización define el estándar productivo,
- el proceso/posición define el contexto operativo,
- el esquema define cómo se liquida,
- el empleado puede tener excepción, pero no debe ser la base universal.

## 6.2 Regla de captura única
Todo trabajo que se pagará debe quedar registrado una sola vez.

### recomendación
- el supervisor captura en `PedidoSeguimiento`,
- el sistema consolida a vale,
- no se vuelve a recapturar.

## 6.3 Regla de aprobación
Antes de entrar a nómina, el destajo debe pasar por aprobación.

### recomendación
Estados sugeridos:
- `Borrador`
- `PendienteRevision`
- `Aprobado`
- `EnNomina`
- `Pagado`
- `Cancelado`

## 6.4 Regla de no duplicidad
Cada registro productivo debe saber si ya fue:
- consolidado en vale,
- aprobado,
- enviado a nómina,
- pagado.

Esto evita:
- pago doble,
- consolidación múltiple,
- pérdida de trazabilidad.

---

## 7. Propuesta funcional por módulos

## 7.1 Producción
### `PedidoSeguimiento`
Debe convertirse en el punto principal de captura del destajo.

**Debería permitir claramente:**
- registrar empleado por participación,
- cantidad procesada,
- tiempo real o estándar aplicado,
- tarifa aplicada,
- observaciones,
- validación del supervisor.

**Resultado esperado:**
cada `RegistroDestajoProceso` ya es un hecho productivo auditable.

## 7.2 Vales de Destajo
La pantalla actual debería evolucionar a:

### modo propuesto
- seleccionar rango o corte,
- filtrar por empleado,
- traer registros pendientes de consolidar,
- agrupar automáticamente,
- permitir ajustar casos excepcionales,
- aprobar el vale.

### lo que ya no debería hacer como flujo normal
- reconstruir a mano proceso + pedido + cantidad + tarifa desde cero.

## 7.3 Empleados
Debe mantenerse la asignación de esquema por vigencia.

### mejora conceptual
El esquema del empleado debe servir para:
- definir el método de liquidación,
- identificar elegibilidad de bonos,
- definir si incluye sueldo base,
- soportar excepciones controladas.

## 7.4 Nómina
Debe seguir consumiendo vales aprobados, pero con mayor automatización.

### objetivo
- no depender de una captura manual previa,
- solo depender de producción consolidada y aprobada.

---

## 8. Roadmap propuesto

## Fase 1 — consolidación semiautomática
### objetivo
Eliminar la doble captura sin romper el modelo actual.

### cambios funcionales
- seguir capturando `RegistroDestajoProceso` desde producción,
- permitir generar borrador de vale desde registros existentes,
- permitir aprobación del vale,
- marcar registros ya consolidados.

### beneficio
- impacto alto,
- riesgo técnico moderado,
- adopción sencilla.

## Fase 2 — unificación de tarifa
### objetivo
Definir una sola regla oficial de cálculo.

### cambios funcionales
- documentar prioridad de tarifa,
- usar cotización + proceso + posición + esquema,
- dejar el sueldo semanal solo para sueldo base y ciertos mixtos,
- mantener aviso cuando el empleado trabaja fuera de su operación natural.

### beneficio
- consistencia,
- mejor auditoría,
- menos discusión operativa.

## Fase 3 — vales por corte
### objetivo
Automatizar la generación de vales diarios o semanales.

### cambios funcionales
- cierre por fecha,
- generación masiva por empleado,
- vista de diferencias,
- aprobación por lote.

### beneficio
- ahorro administrativo fuerte,
- mejor control por supervisor.

## Fase 4 — nómina automática real
### objetivo
Cerrar el ciclo completo.

### cambios funcionales
- sueldo base automático según esquema,
- destajo desde vales aprobados,
- bonos por meta automáticos,
- trazabilidad completa hasta pago.

### beneficio
- reducción de errores,
- proceso repetible,
- mejor control financiero.

---

## 9. Beneficios esperados

## Operativos
- menos captura manual,
- menos tiempo administrativo,
- menos correcciones,
- mejor visibilidad del trabajo real por operador.

## De control
- trazabilidad completa,
- menor riesgo de pago duplicado,
- conciliación sencilla entre producción y nómina,
- mejor evidencia para supervisión.

## Financieros
- pago más preciso,
- mejor costeo por proceso,
- mejor análisis de productividad,
- base más confiable para bonos y métricas.

---

## 10. Riesgos de no mejorar

Si el sistema permanece igual, los principales riesgos son:

- diferencia entre lo producido y lo pagado,
- omisión de trabajos realizados,
- recaptura con tarifa distinta,
- dependencia de una persona que arma vales,
- auditoría compleja,
- dificultad para escalar la operación.

---

## 11. Recomendación final

Sí conviene automatizar esta parte.

### recomendación concreta
No eliminar el vale.

### recomendación correcta
**convertir el vale en un documento de consolidación y aprobación, no en una captura primaria.**

Ese cambio respeta el modelo actual, aprovecha lo que ya existe y lleva el sistema a un estándar más maduro sin rehacer todo el dominio.

---

## 12. Siguiente paso sugerido

Como siguiente entregable, se recomienda preparar un diseño funcional-técnico con:

1. reglas exactas de cálculo de tarifa,
2. definición de estados del flujo,
3. propuesta de automatización pantalla por pantalla,
4. impacto esperado en entidades y migraciones,
5. plan de implementación por iteraciones.

Ese siguiente documento ya serviría como base directa para desarrollo.