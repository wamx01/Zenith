# Propuesta: Esquemas de Pago y Vales de Destajo

## 1. Contexto y problema actual

### Estado actual del modelo

```
PEDIDOS                    PRODUCCIÓN (hoy)              NÓMINA (hoy)
───────                    ─────────────────              ────────────
Pedido                     PedidoSerigrafiaTallaProceso   Nomina
├─ FechaEntregaEstimada    ├─ Empleado?                   └─ NominaDetalle
├─ Estado                  ├─ Completado                      ├─ TipoPago (enum)
└─ PedidoDetalle           └─ FechaPaso                       ├─ PiezasProducidas
   └─ PedidoSerigrafia                                        └─ TarifaPorPieza
      ├─ FechaEstimada     RegistroDestajoProceso
      ├─ Hecho             ├─ EmpleadoId                  Empleado
      └─ Tallas[]          ├─ TipoProcesoId               ├─ TipoNomina (enum)
                           ├─ CantidadProcesada            ├─ SueldoSemanal
                           ├─ TarifaUnitario (a mano)      └─ PosicionId
                           └─ Importe
```

### Problemas identificados

| # | Problema | Impacto |
|---|---------|---------|
| 1 | `RegistroDestajoProceso` está amarrado a `PedidoSerigrafia` | No sirve para producción genérica |
| 2 | La tarifa se pone a mano en cada registro | No hay forma de auditarla ni configurarla |
| 3 | No existe el concepto de **vale de destajo** | No hay documento agrupador con folio, aprobación ni trazabilidad |
| 4 | `NominaDetalle` calcula `PiezasProducidas × TarifaPorPieza` plano | No jala de los registros reales de producción |
| 5 | `Empleado.TipoNomina` es un enum simple (`Semanal/Destajo/Mixto`) | No configura nada, solo clasifica |
| 6 | Las metas de producción no se basan en pedidos reales | No aprovechan `Pedido.FechaEntregaEstimada` ni `PedidoSerigrafia.FechaEstimada` |

### Esquemas de pago que se necesitan soportar

1. **Sueldo fijo** — nómina normal sin depender de piezas ni metas.
2. **Destajo por pieza** — pago por cantidad producida. La tarifa varía por operación/estación y por perfil del operador (ej: operador A en Mesa = $1.20, operador B cubriendo Mesa = $1.50).
3. **Destajo por operación** — cada tipo de proceso tiene su propia tarifa.
4. **Bono por meta de pedidos** — si los pedidos del periodo se entregan a tiempo (o antes), se genera un bono que se reparte por sueldo y asistencia.
5. **Esquema mixto** — sueldo base + cualquiera de los anteriores.

---

## 2. Modelo propuesto

### Diagrama de entidades

```
┌─────────────────────────────────────────────────────────────────┐
│                       CONFIGURACIÓN                              │
│                                                                  │
│  ┌──────────────────┐       ┌─────────────────────┐             │
│  │  EsquemaPago     │  1:N  │ EsquemaPagoTarifa   │             │
│  │  (catálogo por   │──────→│ (tarifa por          │             │
│  │   empresa)       │       │  proceso × posición) │             │
│  └────────┬─────────┘       └─────────────────────┘             │
│           │ 1:N                                                  │
│  ┌────────▼─────────────┐                                        │
│  │ EmpleadoEsquemaPago  │  ← "Juan usa este esquema desde X"    │
│  └──────────────────────┘                                        │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                       OPERACIÓN DIARIA                           │
│                                                                  │
│  ┌────────────────────┐       ┌───────────────────────┐         │
│  │  ValeDestajo       │  1:N  │ ValeDestajoDetalle    │         │
│  │  (folio, fecha,    │──────→│ (proceso, cantidad,   │         │
│  │   empleado,        │       │  tarifa, pedido?)     │         │
│  │   estatus)         │       └───────────────────────┘         │
│  └────────┬───────────┘                                          │
│           │                                                      │
│           ↓ se suma en                                           │
│  ┌────────────────────┐                                          │
│  │  NominaDetalle     │  ← recalculado desde vales reales       │
│  └────────────────────┘                                          │
└─────────────────────────────────────────────────────────────────┘
```

---

## 3. Entidades nuevas

### 3.1 `EsquemaPago`

Catálogo de esquemas de pago por empresa. Un esquema define **cómo** se paga.

```csharp
public class EsquemaPago : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string Nombre { get; set; } = string.Empty;       // "Destajo mesa", "Mixto producción"
    public string? Descripcion { get; set; }
    public TipoEsquemaPago Tipo { get; set; }
    public bool IncluyeSueldoBase { get; set; }               // true = mixto (sueldo + destajo/bono)
    public decimal? SueldoBaseSugerido { get; set; }          // Monto base sugerido cuando es mixto

    // Configuración de bono por cumplimiento de pedidos
    public bool UsaMetaPorPedidos { get; set; }
    public decimal? BonoCumplimientoMonto { get; set; }       // Bono si se entregan a tiempo
    public decimal? BonoAdelantoMonto { get; set; }           // Bono extra si se adelantan
    public bool BonoRepartirPorSueldo { get; set; } = true;   // Ponderar reparto por sueldo
    public bool BonoRepartirPorAsistencia { get; set; } = true; // Ponderar reparto por asistencia

    // Navegación
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
```

### 3.2 `EsquemaPagoTarifa`

Define la tarifa que aplica para una combinación de proceso + posición dentro de un esquema.

```csharp
public class EsquemaPagoTarifa : BaseEntity
{
    public Guid EsquemaPagoId { get; set; }
    public EsquemaPago EsquemaPago { get; set; } = null!;

    public Guid? TipoProcesoId { get; set; }     // null = aplica a cualquier proceso
    public TipoProceso? TipoProceso { get; set; }

    public Guid? PosicionId { get; set; }         // null = aplica a cualquier posición
    public Posicion? Posicion { get; set; }

    public decimal Tarifa { get; set; }           // $ por pieza o por operación
    public string? Descripcion { get; set; }
}
```

**Ejemplo de resolución de tarifa:**

| TipoProcesoId | PosicionId | Tarifa | Significado |
|---------------|-----------|--------|-------------|
| Mesa          | Operador A | $1.20 | Operador A en su estación normal |
| Mesa          | Operador B | $1.50 | Operador B cubriendo estación ajena (su sueldo es mayor) |
| Mesa          | `null`     | $1.00 | Cualquier otro operador en Mesa |
| Pulpo         | `null`     | $0.90 | Cualquier operador en Pulpo |
| `null`        | `null`     | $0.80 | Tarifa default del esquema |

**Prioridad de resolución:** más específico gana.

### 3.3 `EmpleadoEsquemaPago`

Asignación: qué esquema usa cada empleado, con vigencia y override opcional.

```csharp
public class EmpleadoEsquemaPago : BaseEntity
{
    public Guid EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public Guid EsquemaPagoId { get; set; }
    public EsquemaPago EsquemaPago { get; set; } = null!;

    public decimal? SueldoBaseOverride { get; set; }   // Si este empleado tiene sueldo diferente al sugerido
    public DateTime VigenteDesde { get; set; }
    public DateTime? VigenteHasta { get; set; }        // null = vigente actualmente
}
```

### 3.4 `ValeDestajo`

El documento/comprobante diario de trabajo por destajo. Agrupa líneas de producción, tiene folio y ciclo de vida.

```csharp
public class ValeDestajo : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string Folio { get; set; } = string.Empty;        // VD-0001
    public Guid EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public DateTime Fecha { get; set; }
    public EstatusValeDestajo Estatus { get; set; } = EstatusValeDestajo.Borrador;

    public Guid? EsquemaPagoId { get; set; }                 // Esquema activo al momento del vale
    public EsquemaPago? EsquemaPago { get; set; }

    public Guid? NominaDetalleId { get; set; }                // Se llena cuando se incluye en nómina
    public NominaDetalle? NominaDetalle { get; set; }

    public string? Observaciones { get; set; }

    // Calculados
    public int TotalPiezas => Detalles.Sum(d => d.Cantidad);
    public decimal TotalImporte => Detalles.Sum(d => d.Importe);

    // Navegación
    public ICollection<ValeDestajoDetalle> Detalles { get; set; } = [];
}

public enum EstatusValeDestajo
{
    Borrador = 1,
    Aprobado = 2,
    EnNomina = 3,
    Pagado = 4,
    Cancelado = 5
}
```

### 3.5 `ValeDestajoDetalle`

Cada línea del vale = un trabajo realizado. Puede o no estar ligado a un pedido.

```csharp
public class ValeDestajoDetalle : BaseEntity
{
    public Guid ValeDestajoId { get; set; }
    public ValeDestajo ValeDestajo { get; set; } = null!;

    public Guid TipoProcesoId { get; set; }
    public TipoProceso TipoProceso { get; set; } = null!;

    public Guid? PedidoId { get; set; }                       // Opcional: pedido relacionado
    public Pedido? Pedido { get; set; }

    public int Cantidad { get; set; }                         // Piezas procesadas
    public decimal TarifaAplicada { get; set; }               // Viene del esquema de pago
    public decimal Importe { get; set; }                      // Cantidad × TarifaAplicada

    public Guid? EsquemaPagoTarifaId { get; set; }           // Trazabilidad: de dónde salió la tarifa
    public EsquemaPagoTarifa? EsquemaPagoTarifa { get; set; }

    public int? TiempoMinutos { get; set; }
    public string? Observaciones { get; set; }
}
```

---

## 4. Cambios a entidades existentes

### 4.1 `Empleado`

Quitar el enum `TipoNomina` simple y agregar la relación al nuevo modelo:

```diff
- public TipoNomina TipoNomina { get; set; } = TipoNomina.Semanal;
+ public ICollection<EmpleadoEsquemaPago> EsquemasPago { get; set; } = [];
+ public ICollection<ValeDestajo> ValesDestajo { get; set; } = [];
```

Se **mantiene** `SueldoSemanal` como referencia base (para reparto de bonos y como valor por defecto).

### 4.2 `NominaDetalle`

Reestructurar para que se alimente de vales reales:

```diff
- public TipoNomina TipoPago { get; set; } = TipoNomina.Semanal;
- public int PiezasProducidas { get; set; }
- public decimal TarifaPorPieza { get; set; }
- public decimal MontoDestajo => PiezasProducidas * TarifaPorPieza;
- public decimal Bonos { get; set; }
+ public Guid? EsquemaPagoId { get; set; }
+ public EsquemaPago? EsquemaPago { get; set; }
+ public int TotalPiezas { get; set; }            // Suma de piezas de vales del periodo
+ public decimal MontoDestajo { get; set; }        // Suma de importes de vales del periodo
+ public decimal MontoBono { get; set; }           // Bono calculado por meta de pedidos

  // Se mantienen:
  public decimal SueldoBase { get; set; }
  public decimal HorasExtra { get; set; }
  public decimal MontoHorasExtra { get; set; }
  public decimal Deducciones { get; set; }
  public string? ConceptoDeducciones { get; set; }
  public string? Notas { get; set; }

- public decimal TotalPagar => TipoPago switch { ... };
+ public decimal TotalPagar => SueldoBase + MontoDestajo + MontoBono
+                              + MontoHorasExtra - Deducciones;

+ public ICollection<ValeDestajo> ValesDestajo { get; set; } = [];
```

### 4.3 `Empleado` — Eliminar enum `TipoNomina`

El enum `TipoNomina` (`Semanal = 1, Destajo = 2, Mixto = 3`) se elimina porque ahora el tipo de pago se define en `EsquemaPago.Tipo`.

---

## 5. Flujo de captura: Vales de destajo

### Pantalla: Captura de Vales

```
┌─────────────────────────────────────────────────────────────┐
│  VALE DE DESTAJO                                VD-0042     │
│                                                              │
│  Empleado: [Juan Pérez          ▼]   Fecha: [16/Mar/2026]   │
│  Esquema:  Destajo estándar (auto)                           │
│                                                              │
│  ┌───────────┬──────────┬─────────┬─────────┬────────────┐  │
│  │ Proceso   │ Pedido   │ Piezas  │ Tarifa  │ Importe    │  │
│  ├───────────┼──────────┼─────────┼─────────┼────────────┤  │
│  │ Mesa      │ PED-0023 │ 50      │ $1.20   │ $60.00     │  │
│  │ Pulpo     │ PED-0025 │ 30      │ $0.90   │ $27.00     │  │
│  │ Mesa      │ (libre)  │ 20      │ $1.20   │ $24.00     │  │
│  ├───────────┼──────────┼─────────┼─────────┼────────────┤  │
│  │           │ TOTAL    │ 100     │         │ $111.00    │  │
│  └───────────┴──────────┴─────────┴─────────┴────────────┘  │
│                                                              │
│  [Guardar borrador]  [Aprobar vale]                          │
└─────────────────────────────────────────────────────────────┘
```

### Flujo paso a paso

1. Seleccionar **empleado** → sistema carga su esquema activo (`EmpleadoEsquemaPago` vigente).
2. Agregar líneas: seleccionar **proceso** + capturar **piezas**.
3. **Pedido** es opcional — puede ser trabajo libre (sin pedido específico).
4. **Tarifa** se autocompleta desde `EsquemaPagoTarifa` según el proceso + la posición del empleado.
5. Guardar como **borrador** o **aprobar** directamente.

### Ciclo de vida del vale

```
Borrador → Aprobado → EnNomina → Pagado
                ↓
           Cancelado
```

---

## 6. Flujo de generación de nómina

```
1. Seleccionar periodo (ej: semana 12, 11-16 Marzo 2026)

2. Por cada empleado activo:

   a. Buscar su EsquemaPago activo (vía EmpleadoEsquemaPago vigente)
   
   b. Según el tipo de esquema:
   
      ┌─ SueldoFijo:
      │    SueldoBase = empleado.SueldoSemanal (o override)
      │    MontoDestajo = 0
      │    MontoBono = 0
      │
      ├─ DestajoPorPieza / DestajoPorOperacion:
      │    SueldoBase = 0
      │    MontoDestajo = Σ ValesDestajo aprobados del periodo
      │    MontoBono = 0
      │
      ├─ BonoMetaPedidos:
      │    SueldoBase = empleado.SueldoSemanal
      │    MontoDestajo = 0
      │    MontoBono = calcular bono (ver sección 6.1)
      │
      └─ Mixto:
           SueldoBase = esquema.SueldoBaseSugerido (o override)
           MontoDestajo = Σ ValesDestajo aprobados del periodo
           MontoBono = calcular bono si aplica

   c. Crear NominaDetalle con todo sumado
   d. Vincular vales del periodo → vale.NominaDetalleId = detalle.Id
   e. Cambiar estatus de vales a "EnNomina"

3. Cuando se marca la nómina como Pagada:
   → Cambiar estatus de vales a "Pagado"
```

### 6.1 Cálculo de bono por meta de pedidos

```
Pedidos del periodo = todos los Pedido con FechaEntregaEstimada dentro del rango

entregadosATiempo = pedidos donde Estado >= Entregado y FechaEntregaReal <= FechaEntregaEstimada
entregadosAntes = pedidos donde FechaEntregaReal < FechaEntregaEstimada (adelantados)

Si todos a tiempo:
  bonoTotal = esquema.BonoCumplimientoMonto

Si hay adelantados:
  bonoTotal += esquema.BonoAdelantoMonto

Reparto entre empleados del esquema:
  peso[empleado] = (sueldoBase × díasAsistidos)
  participación[empleado] = peso[empleado] / Σ pesos
  bono[empleado] = bonoTotal × participación[empleado]
```

---

## 7. Qué pasa con `RegistroDestajoProceso`

| Opción | Descripción |
|--------|-------------|
| **A) Reemplazar** | `ValeDestajoDetalle` lo sustituye por completo. Migrar datos existentes. |
| **B) Mantener ambos** | `RegistroDestajoProceso` sigue para tracking granular por talla. `ValeDestajo` es el documento de pago. |

**Decisión: Opción A — Reemplazar.**

`ValeDestajoDetalle` puede linkear a un pedido y a un proceso igual que `RegistroDestajoProceso`. La granularidad extra por talla se puede agregar después si hace falta. Esto simplifica el modelo y evita duplicidad.

`RegistroDestajoProceso` no se elimina de la BD (para no perder datos), pero deja de usarse para nuevos registros.

---

## 8. Comparación: antes vs después

| Concepto | Antes | Después |
|----------|-------|---------|
| Tipo de pago del empleado | `Empleado.TipoNomina` (enum: Semanal/Destajo/Mixto) | `EmpleadoEsquemaPago` → apunta a un esquema configurable con vigencia |
| Sueldo base | `Empleado.SueldoSemanal` (un solo número) | `EsquemaPago.SueldoBaseSugerido` + override por empleado |
| Tarifa de destajo | `Posicion.TarifaPorMinuto` (una sola tarifa global) | `EsquemaPagoTarifa` (tarifa por proceso × posición dentro de cada esquema) |
| Registro de producción | `RegistroDestajoProceso` (amarrado a PedidoSerigrafia, tarifa a mano) | `ValeDestajo` + `ValeDestajoDetalle` (genérico, con folio, aprobación y trazabilidad) |
| Cálculo de nómina | `NominaDetalle.PiezasProducidas × TarifaPorPieza` (plano) | Suma de vales aprobados + bono por metas + sueldo base según esquema |
| Metas de producción | No existían | Basadas en pedidos reales (`FechaEntregaEstimada`) |
| Bono por productividad | `NominaDetalle.Bonos` (campo libre) | Calculado automáticamente, repartido por sueldo × asistencia |

---

## 9. Archivos afectados

### Archivos nuevos

| Archivo | Propósito |
|---------|-----------|
| `Core/Entities/EsquemaPago.cs` | Entidad + enum `TipoEsquemaPago` |
| `Core/Entities/EsquemaPagoTarifa.cs` | Tarifas por proceso × posición |
| `Core/Entities/EmpleadoEsquemaPago.cs` | Asignación empleado ↔ esquema |
| `Core/Entities/ValeDestajo.cs` | Vale de destajo + enum `EstatusValeDestajo` |
| `Core/Entities/ValeDestajoDetalle.cs` | Detalle/líneas del vale |

### Archivos modificados

| Archivo | Cambio |
|---------|--------|
| `Core/Entities/Empleado.cs` | Quitar `TipoNomina`, agregar colecciones `EsquemasPago` y `ValesDestajo` |
| `Core/Entities/NominaDetalle.cs` | Reestructurar campos: quitar cálculo plano, agregar `EsquemaPagoId`, `MontoDestajo`, `MontoBono`, `TotalPiezas`, colección `ValesDestajo` |
| `Infrastructure/Data/CrmDbContext.cs` | Agregar DbSets, configuración Fluent API, query filters |

### Enum eliminado

| Enum | Razón |
|------|-------|
| `TipoNomina` (en Empleado.cs) | Reemplazado por `TipoEsquemaPago` en el esquema configurable |

---

## 10. Notas de migración

- La migración debe considerar que pueden existir datos en `Empleado.TipoNomina` y `NominaDetalle`.
- `RegistroDestajoProceso` se mantiene en la BD pero queda deprecado.
- Los nuevos campos `MontoDestajo`, `MontoBono`, `TotalPiezas` en `NominaDetalle` se crean con default 0.
- El campo `NominaDetalle.Bonos` se renombra conceptualmente a `MontoBono`.
- El folio del vale (`ValeDestajo.Folio`) debe ser único por empresa.
