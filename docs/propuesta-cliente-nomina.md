# Zenith ERP — Nuevo Módulo de Nómina, Esquemas de Pago y Vales de Destajo

> **Versión:** 1.0  
> **Fecha:** Marzo 2026  
> **Preparado por:** Arzmec Desarrollo  
> **Dirigido a:** Responsables de operación, supervisores de producción y administración de nómina

---

## Índice

1. [Resumen ejecutivo](#1-resumen-ejecutivo)
2. [¿Qué problemas resuelve?](#2-qué-problemas-resuelve)
3. [¿Qué incluye esta mejora?](#3-qué-incluye-esta-mejora)
4. [Flujo de trabajo paso a paso](#4-flujo-de-trabajo-paso-a-paso)
5. [Pantallas nuevas y mejoradas](#5-pantallas-nuevas-y-mejoradas)
   - 5.1 Esquemas de Pago
   - 5.2 Vales de Destajo
   - 5.3 Empleados (mejorado)
   - 5.4 Nóminas (mejorado)
6. [Tipos de esquema de pago disponibles](#6-tipos-de-esquema-de-pago-disponibles)
7. [Ciclo de vida de un vale de destajo](#7-ciclo-de-vida-de-un-vale-de-destajo)
8. [Cálculo automático de nómina](#8-cálculo-automático-de-nómina)
9. [Beneficios clave](#9-beneficios-clave)
10. [Comparación: antes y después](#10-comparación-antes-y-después)
11. [Preguntas frecuentes](#11-preguntas-frecuentes)

---

## 1. Resumen ejecutivo

Se incorpora al sistema **Zenith ERP** un conjunto de mejoras en el módulo de Recursos Humanos que permiten:

- **Definir esquemas de pago flexibles** (sueldo fijo, destajo por pieza, bono por metas, esquemas mixtos).
- **Registrar la producción diaria** mediante **vales de destajo** con folio, aprobación y trazabilidad completa.
- **Calcular la nómina automáticamente** a partir de los vales aprobados y el esquema asignado a cada empleado.

Estas mejoras eliminan la captura manual de tarifas y piezas en la nómina, reducen errores de cálculo y brindan visibilidad completa sobre qué se produjo, quién lo hizo y cuánto se debe pagar.

---

## 2. ¿Qué problemas resuelve?

| # | Situación actual | Problema | Cómo se resuelve |
|---|-----------------|----------|-------------------|
| 1 | El tipo de pago del empleado es solo una etiqueta (Semanal, Destajo, Mixto) | No define tarifas, reglas ni configuración real | Se reemplaza por un **esquema de pago** configurable con tarifas, bonos y reglas claras |
| 2 | Las tarifas de destajo se capturan a mano en cada registro | No hay forma de auditarlas ni garantizar que sean correctas | Las tarifas se definen una sola vez en el esquema y se **aplican automáticamente** |
| 3 | No existe un documento que agrupe el trabajo diario del operador | No hay forma de que el supervisor valide antes de que entre a nómina | Se crea el **vale de destajo**: documento con folio, detalle y flujo de aprobación |
| 4 | La nómina calcula "piezas × tarifa" de forma plana | No jala datos reales de producción | La nómina ahora **suma automáticamente** los vales aprobados del periodo |
| 5 | Los bonos se capturan como un monto libre sin reglas | No hay forma de calcularlos ni auditarlos | Los bonos se calculan automáticamente según **cumplimiento de pedidos** |
| 6 | No hay historial de cambios de esquema por empleado | Si cambian el tipo de pago, no se sabe qué tenía antes | Se guarda un **historial de esquemas** con fecha de vigencia |
| 7 | El registro de producción está limitado a un tipo de trabajo específico | No sirve para operaciones genéricas | Los vales de destajo funcionan con **cualquier tipo de proceso** configurado |

---

## 3. ¿Qué incluye esta mejora?

### Dos pantallas nuevas

| Pantalla | Ubicación en el menú | ¿Quién la usa? |
|----------|---------------------|-----------------|
| **Esquemas de Pago** | Recursos Humanos → Esquemas de Pago | Administrador de nómina |
| **Vales de Destajo** | Recursos Humanos → Vales de Destajo | Supervisor de producción, administrador |

### Dos pantallas mejoradas

| Pantalla | Cambio principal | ¿Quién la usa? |
|----------|-----------------|-----------------|
| **Empleados** | Ahora muestra el esquema de pago asignado con historial | Recursos Humanos |
| **Nóminas** | Calcula automáticamente desde los vales aprobados | Administrador de nómina |

### Nuevo menú de Recursos Humanos

```
📁 Recursos Humanos
  └─ 👤 Empleados
  └─ 📋 Esquemas de Pago     ← NUEVO
  └─ 📝 Vales de Destajo     ← NUEVO
  └─ 💰 Nóminas
```

---

## 4. Flujo de trabajo paso a paso

El sistema sigue un flujo lógico de cuatro etapas:

```
 ① CONFIGURAR          ② ASIGNAR           ③ OPERAR             ④ PAGAR
 (una sola vez)        (al contratar)       (cada día)           (cada semana)
                                                            
┌──────────────┐    ┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│  Esquemas    │    │  Empleados   │    │  Vales de    │    │  Nóminas     │
│  de Pago     │───→│  + asignar   │───→│  Destajo     │───→│  + calcular  │
│  + tarifas   │    │    esquema   │    │  + aprobar   │    │  + pagar     │
└──────────────┘    └──────────────┘    └──────────────┘    └──────────────┘
```

### Etapa 1 — Configurar (una sola vez o cuando haya cambios)

El administrador crea los esquemas de pago que necesita la empresa. Por ejemplo:
- "Sueldo fijo estándar" — para personal administrativo.
- "Destajo mesa" — para operadores que trabajan por pieza.
- "Mixto producción" — sueldo base + destajo.
- "Bono semanal por metas" — sueldo base + bono si se cumplen entregas.

Cada esquema incluye las **tarifas por proceso y posición** que correspondan.

### Etapa 2 — Asignar (al contratar o cambiar puesto)

Al dar de alta o editar un empleado, se le asigna su esquema de pago con una **fecha de vigencia**. Si en el futuro cambia de esquema, el anterior queda registrado en el historial.

### Etapa 3 — Operar (cada día)

El supervisor de producción captura los **vales de destajo** diarios:
1. Selecciona al empleado.
2. Agrega las líneas de trabajo (proceso + cantidad de piezas).
3. La tarifa se llena automáticamente según el esquema del empleado.
4. Guarda como borrador o aprueba directamente.

### Etapa 4 — Pagar (cada periodo de nómina)

Al generar la nómina del periodo:
1. El sistema **sincroniza** los empleados activos.
2. **Jala automáticamente** los vales aprobados del periodo.
3. **Calcula** sueldo + destajo + bonos + extras − deducciones.
4. El administrador revisa, ajusta si es necesario y aprueba.

---

## 5. Pantallas nuevas y mejoradas

### 5.1 Esquemas de Pago (nueva)

**¿Para qué sirve?** Para definir las formas de pago de la empresa y las tarifas que aplican a cada proceso de producción.

**Vista de lista:**

```
┌─────────────────────────────────────────────────────────────────────┐
│  Esquemas de Pago                                [+ Nuevo esquema]  │
│                                                                     │
│  ┌──────────────────────┬──────────────────┬────────┬────────────┐  │
│  │ Nombre               │ Tipo             │ Sueldo │ Acciones   │  │
│  ├──────────────────────┼──────────────────┼────────┼────────────┤  │
│  │ Sueldo fijo estándar │ Sueldo Fijo      │ ✅ Sí  │ Editar  ×  │  │
│  │ Destajo mesa         │ Destajo/Pieza    │ — No   │ Editar  ×  │  │
│  │ Mixto producción     │ Mixto            │ ✅ Sí  │ Editar  ×  │  │
│  │ Bono semanal metas   │ Bono por Meta    │ ✅ Sí  │ Editar  ×  │  │
│  └──────────────────────┴──────────────────┴────────┴────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
```

**Formulario de configuración:**

Al crear o editar un esquema se puede definir:

- **Nombre y descripción** del esquema.
- **Tipo de esquema** (sueldo fijo, destajo por pieza, destajo por operación, bono por metas, mixto).
- **Sueldo base sugerido** (cuando aplica).
- **Configuración de bono** (monto por cumplimiento, monto por adelanto, criterios de reparto).
- **Tabla de tarifas** por proceso y posición:

```
┌───────────────┬──────────────┬────────────┐
│ Proceso       │ Posición     │ Tarifa     │
├───────────────┼──────────────┼────────────┤
│ Mesa          │ Operador A   │ $1.20      │
│ Mesa          │ (cualquiera) │ $1.00      │
│ Pulpo         │ (cualquiera) │ $0.90      │
│ (cualquiera)  │ (cualquiera) │ $0.80      │
└───────────────┴──────────────┴────────────┘
```

> **Nota:** Las tarifas se aplican por prioridad. Si existe una tarifa específica para el proceso y posición del empleado, se usa esa. Si no, se busca la tarifa general del proceso. Si no existe ninguna, se usa la tarifa por defecto del esquema.

---

### 5.2 Vales de Destajo (nueva)

**¿Para qué sirve?** Para registrar diariamente lo que cada operador produjo, con un documento formal que el supervisor puede aprobar antes de que entre a nómina.

**Vista de lista con filtros:**

```
┌──────────────────────────────────────────────────────────────────────┐
│  Vales de Destajo                                  [+ Nuevo vale]    │
│                                                                      │
│  Filtros: Empleado [Todos ▼]  Desde [__/__/____] Hasta [__/__/____]  │
│           Estatus [Todos ▼]                                          │
│                                                                      │
│  ┌────────┬──────────────┬────────────┬────────┬──────────┬────────┐ │
│  │ Folio  │ Empleado     │ Fecha      │ Piezas │ Importe  │ Estatus│ │
│  ├────────┼──────────────┼────────────┼────────┼──────────┼────────┤ │
│  │ VD-042 │ Juan Pérez   │ 16/Mar/26  │ 100    │ $111.00  │ ✅ Apr │ │
│  │ VD-041 │ María López  │ 16/Mar/26  │ 80     │ $96.00   │ 📝 Brr │ │
│  │ VD-040 │ Juan Pérez   │ 15/Mar/26  │ 120    │ $132.00  │ 💰 Pag │ │
│  └────────┴──────────────┴────────────┴────────┴──────────┴────────┘ │
└──────────────────────────────────────────────────────────────────────┘
```

**Captura del vale:**

```
┌──────────────────────────────────────────────────────────────────────┐
│  Vale de Destajo                                        VD-0043      │
│                                                                      │
│  Empleado: [Juan Pérez              ▼]  Fecha: [16/Mar/2026]        │
│  Esquema activo: Destajo estándar (automático)                       │
│                                                                      │
│  ── Detalle de producción ───────────────────────────────────        │
│  ┌───────────────┬──────────┬─────────┬──────────┬──────────┐       │
│  │ Proceso       │ Pedido   │ Piezas  │ Tarifa   │ Importe  │       │
│  ├───────────────┼──────────┼─────────┼──────────┼──────────┤       │
│  │ Mesa          │ PED-23   │ 50      │ $1.20    │ $60.00   │       │
│  │ Pulpo         │ PED-25   │ 30      │ $0.90    │ $27.00   │       │
│  │ Mesa          │ (libre)  │ 20      │ $1.20    │ $24.00   │       │
│  └───────────────┴──────────┴─────────┴──────────┴──────────┘       │
│                                                                      │
│  Total piezas: 100               Total importe: $111.00              │
│  Observaciones: [________________________________]                   │
│                                                                      │
│  [Cancelar]  [Guardar borrador]  [Aprobar vale]                      │
└──────────────────────────────────────────────────────────────────────┘
```

**Puntos importantes para el usuario:**
- Al seleccionar un empleado, el sistema muestra automáticamente su esquema de pago vigente.
- Al seleccionar un proceso, la tarifa se llena automáticamente — no es necesario capturarla a mano.
- El campo "Pedido" es opcional: se puede registrar trabajo ligado a un pedido específico o trabajo libre.
- El importe se calcula automáticamente (piezas × tarifa) y no se puede modificar manualmente.
- Cada vale tiene un **folio único** asignado automáticamente.

---

### 5.3 Empleados (mejorado)

**¿Qué cambió?** Se reemplazó la etiqueta simple de "Tipo de nómina" por una sección completa de **esquema de pago** con historial.

**Sección nueva en el formulario del empleado:**

```
── Esquema de pago activo ──────────────────────────────
Esquema: [Destajo estándar          ▼]
Vigente desde: [01/Ene/2026]
Sueldo base personalizado: [$____] (opcional — si se deja vacío, 
                                     se usa el sugerido del esquema)

📋 Historial de esquemas:
┌───────────────────────┬────────────┬──────────────┬─────────────┐
│ Esquema               │ Desde      │ Hasta        │ Sueldo      │
├───────────────────────┼────────────┼──────────────┼─────────────┤
│ Destajo estándar      │ 01/Ene/26  │ (vigente)    │ — (esquema) │
│ Sueldo fijo           │ 01/Jun/25  │ 31/Dic/25    │ $2,200      │
└───────────────────────┴────────────┴──────────────┴─────────────┘
```

**Beneficios:**
- Se puede ver de un vistazo cómo se paga a cada empleado.
- Si se cambia el esquema, queda registrado cuándo y cuál era el anterior.
- Se puede personalizar el sueldo base de un empleado sin modificar el esquema general.

---

### 5.4 Nóminas (mejorado)

**¿Qué cambió?** La nómina ahora se alimenta automáticamente de los vales de destajo aprobados y del esquema de pago de cada empleado.

**Vista del detalle de nómina:**

```
┌──────────────────────────────────────────────────────────────────────────┐
│  Nómina: Semana 12 (11-16 Mar 2026)         Estatus: Borrador           │
│                                                                          │
│  [Sincronizar empleados]                                                 │
│                                                                          │
│  ┌────────────┬─────────────┬────────┬──────────┬───────┬───────┬──────┐ │
│  │ Empleado   │ Esquema     │ Sueldo │ Destajo  │ Bono  │ Extras│Total │ │
│  ├────────────┼─────────────┼────────┼──────────┼───────┼───────┼──────┤ │
│  │ Juan Pérez │ Destajo std │ $0     │ $543.00  │ $0    │ $0    │$543  │ │
│  │ María López│ Mixto prod  │ $2,400 │ $312.00  │ $0    │ $150  │$2,862│ │
│  │ Carlos R.  │ Sueldo fijo │ $3,000 │ —        │ —     │ $0    │$3,000│ │
│  └────────────┴─────────────┴────────┴──────────┴───────┴───────┴──────┘ │
│                                                                          │
│  Total nómina: $6,405.00                                                 │
│                                                                          │
│  [Guardar cambios]  [Aprobar nómina]                                     │
└──────────────────────────────────────────────────────────────────────────┘
```

**Desglose por empleado (al expandir una fila):**

```
┌─ Vales del periodo ──────────────────────────────────────────────┐
│ VD-040  15/Mar  Mesa 50 pzs → $60  + Pulpo 30 pzs → $27  = $87 │
│ VD-042  16/Mar  Mesa 70 pzs → $84  + Pulpo 40 pzs → $36  = $120│
│ VD-044  17/Mar  Mesa 80 pzs → $96  + Pulpo 60 pzs → $54  = $150│
│ VD-045  18/Mar  Mesa 90 pzs → $108 + Pulpo 20 pzs → $18  = $126│
│ VD-046  19/Mar  Mesa 50 pzs → $60                         = $60 │
├──────────────────────────────────────────────────────────────────┤
│                            Total vales: 500 pzs    $543.00      │
└──────────────────────────────────────────────────────────────────┘
```

**Beneficios:**
- Ya no se capturan piezas ni tarifas manualmente en la nómina.
- El destajo se calcula sumando los vales aprobados del periodo.
- Se puede ver exactamente de dónde viene cada peso del pago.
- Horas extra, deducciones y notas se siguen editando normalmente.

---

## 6. Tipos de esquema de pago disponibles

| Tipo | Descripción | Incluye sueldo base | Incluye destajo | Incluye bono |
|------|-------------|:-------------------:|:---------------:|:------------:|
| **Sueldo Fijo** | Pago fijo sin depender de producción | ✅ | — | — |
| **Destajo por Pieza** | Pago según cantidad de piezas producidas. La tarifa varía por proceso y posición | — | ✅ | — |
| **Destajo por Operación** | Similar al anterior, con tarifas por tipo de operación | — | ✅ | — |
| **Bono por Meta de Pedidos** | Sueldo base + bono si los pedidos se entregan a tiempo | ✅ | — | ✅ |
| **Mixto** | Combina sueldo base + destajo + bono (según configuración) | ✅ | ✅ | Opcional |

> Cada esquema se configura una sola vez y se asigna a los empleados que correspondan. Si cambian las condiciones, se puede crear un esquema nuevo o modificar el existente.

---

## 7. Ciclo de vida de un vale de destajo

Un vale de destajo pasa por las siguientes etapas:

```
  📝 Borrador ──→ ✅ Aprobado ──→ 📦 En Nómina ──→ 💰 Pagado
                      │
                      └──→ ❌ Cancelado
```

| Etapa | ¿Quién? | ¿Qué se puede hacer? |
|-------|---------|---------------------|
| **Borrador** | Supervisor | Editar libremente: agregar/quitar líneas, cambiar cantidades |
| **Aprobado** | Supervisor | Ya no se puede editar. Solo cancelar si hay un error |
| **En Nómina** | Sistema (automático) | Se asignó a una nómina. Pendiente de pago |
| **Pagado** | Sistema (automático) | La nómina se marcó como pagada. Proceso completado |
| **Cancelado** | Supervisor | Vale anulado. No entra a nómina |

---

## 8. Cálculo automático de nómina

Al presionar **"Sincronizar empleados"** en la nómina, el sistema ejecuta el siguiente proceso automático:

### Para cada empleado activo:

1. **Identifica su esquema de pago** vigente en el periodo.

2. **Calcula el sueldo base:**
   - Si el esquema incluye sueldo base → usa el monto personalizado del empleado o el sugerido del esquema.
   - Si el esquema es solo destajo → sueldo base = $0.

3. **Calcula el destajo:**
   - Busca todos los vales de destajo **aprobados** cuya fecha esté dentro del periodo.
   - Suma los importes de todos los vales → ese es el monto de destajo.

4. **Calcula el bono** (si el esquema lo incluye):
   - Revisa los pedidos del periodo.
   - Si todos se entregaron a tiempo → aplica el bono de cumplimiento.
   - Si alguno se entregó antes de lo previsto → aplica bono adicional por adelanto.
   - El bono total se reparte entre los empleados del esquema, ponderando por sueldo y días de asistencia.

5. **Suma horas extra** (si las hay) con el factor configurado.

6. **Genera el detalle de nómina** con todos los conceptos calculados.

7. **Vincula los vales** al detalle de nómina y los marca como "En Nómina".

### Fórmula del total a pagar:

```
Total = Sueldo Base + Destajo + Bono + Horas Extra − Deducciones
```

---

## 9. Beneficios clave

### Para el supervisor de producción
- ✅ Registra la producción diaria en un formato claro con folio y detalle.
- ✅ Las tarifas se aplican automáticamente — no hay que recordarlas ni calcularlas.
- ✅ Puede revisar y aprobar antes de que el dato llegue a nómina.

### Para el administrador de nómina
- ✅ La nómina se calcula automáticamente desde datos reales de producción.
- ✅ Puede ver el desglose completo de cada empleado: qué vales generaron su pago.
- ✅ Los bonos se calculan con reglas claras y auditables.
- ✅ El historial de esquemas permite saber cómo se pagó a cada empleado en cualquier periodo.

### Para la dirección
- ✅ Trazabilidad completa: del vale de producción al pago en nómina.
- ✅ Flexibilidad para configurar diferentes esquemas según el puesto o área.
- ✅ Datos confiables para análisis de costos de producción y productividad.
- ✅ Reducción de errores y discrepancias en el pago.

---

## 10. Comparación: antes y después

| Concepto | Antes | Después |
|----------|-------|---------|
| Tipo de pago del empleado | Etiqueta simple (Semanal / Destajo / Mixto) sin configuración | Esquema de pago completo con tarifas, bonos y reglas |
| Tarifa de destajo | Se captura a mano en cada registro | Se define en el esquema y se aplica automáticamente |
| Registro de producción | Ligado solo a un tipo de trabajo, sin documento formal | Vale con folio, detalle por proceso, aprobación del supervisor |
| Cálculo de nómina | Piezas × tarifa capturados manualmente | Suma automática de vales aprobados del periodo |
| Bonos | Monto libre sin reglas | Calculado automáticamente según cumplimiento de pedidos |
| Historial de cambios | No existía | Historial completo con fechas de vigencia por empleado |
| Auditoría | Difícil rastrear de dónde viene cada monto | Cada peso de la nómina se puede rastrear hasta su vale y proceso |

---

## 11. Preguntas frecuentes

### ¿Puedo seguir usando la nómina como antes mientras hacemos la transición?

Sí. Los datos anteriores se conservan intactos. El sistema funciona con los nuevos esquemas y vales para los registros nuevos, sin afectar las nóminas ya generadas.

### ¿Qué pasa si un empleado no tiene esquema de pago asignado?

El sistema lo trata como sueldo fijo usando su sueldo base actual. No se perderá ningún empleado al generar la nómina.

### ¿Puedo tener diferentes tarifas para el mismo proceso según quién lo haga?

Sí. Las tarifas se configuran por **proceso + posición**, lo que permite que un operador con más experiencia tenga una tarifa diferente al de nuevo ingreso en el mismo proceso.

### ¿Los vales se pueden corregir después de aprobados?

No. Un vale aprobado ya no se puede editar para garantizar la integridad del dato. Si hay un error, se cancela el vale y se crea uno nuevo corregido.

### ¿Qué empleados ven cada pantalla?

| Pantalla | ¿Quién puede verla? | ¿Quién puede editar? |
|----------|---------------------|---------------------|
| Esquemas de Pago | Personal con permiso de nómina | Solo quien tiene permiso de edición de nómina |
| Vales de Destajo | Personal con permiso de nómina (ver) | Quien tiene permiso de edición de nómina (crear/aprobar) |
| Empleados | Personal con permiso de empleados | Quien tiene permiso de edición de empleados |
| Nóminas | Personal con permiso de nómina | Quien tiene permiso de edición de nómina |

### ¿Puedo cambiar el esquema de un empleado a mitad de periodo?

Sí. Al cambiar el esquema se registra la fecha de vigencia. Los vales que ya se capturaron con el esquema anterior conservan sus tarifas originales. Los nuevos vales usarán las tarifas del nuevo esquema.

### ¿El bono por metas se calcula automáticamente?

Sí, siempre que el esquema del empleado tenga activada la opción de "Meta por pedidos". El sistema revisa los pedidos del periodo, evalúa si se cumplieron las fechas de entrega, y reparte el bono entre los empleados participantes.

---

> **Zenith ERP** — Módulo de Nómina v2.0  
> Documento preparado por Arzmec Desarrollo · Marzo 2026
