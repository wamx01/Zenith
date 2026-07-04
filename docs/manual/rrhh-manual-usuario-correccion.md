# Manual de Usuario — Corrección de Asistencias y Tiempo Extra

## Índice
1. [Abrir el modal de corrección](#abrir-el-modal-de-corrección)
2. [Interpretar el resumen del día](#interpretar-el-resumen-del-día)
3. [Con turno: revisar bloques del timeline](#con-turno-revisar-bloques-del-timeline)
4. [Sin turno: decidir tiempo extra](#sin-turno-decidir-tiempo-extra)
5. [Resolver tiempo extra](#resolver-tiempo-extra)
6. [Registrar permiso parcial](#registrar-permiso-parcial)
7. [Guardar y cerrar](#guardar-y-cerrar)

---

## Abrir el modal de corrección

Al abrir el modal de un día, el sistema **reprocesa automáticamente** para garantizar que los valores mostrados sean correctos según las reglas actuales.

### Encabezado

- **PROG.**: Horario programado del turno (vacío si no hay turno)
- **REAL**: Hora real de entrada y salida según marcaciones
- **Sin turno**: Indica que el empleado no tiene turno asignado para ese día

### Chips superiores

| Chip | Significado |
|---|---|
| **Visible** | Tiempo total trabajado = base + compensación + extra aprobado |
| **Base** | Jornada neta programada (excluye descansos no pagados) |
| **Ret.** | Minutos de retardo efectivo (después de perdón y permiso) |
| **S.Ant.** | Minutos de salida anticipada efectiva |
| **Extra** | Minutos de extra detectado por el procesador (no aprobado) |
| **Extra aprobada** | Minutos de extra ya aprobados (se suman al visible) |
| **Comp. día** | Compensación aprobada del mismo día |

---

## Interpretar el resumen del día

### Estado de corrección

| Estado | Color | Significado |
|---|---|---|
| **Permiso aplicado** | Verde | Ya existe un permiso parcial registrado |
| **Regla confirmada** | Verde | Se confirmó que la salida temprana sustituyó el descanso |
| **Tiempo resuelto** | Verde | Ya se aprobó tiempo extra o se cubrió con banco |
| **Sin pendiente** | Azul | No hay nada pendiente de revisar |
| **Corregir marcaciones** | Amarillo | Primero revisa las marcaciones del día |
| **Falta permiso o turno** | Amarillo | Registra permiso o revisa el turno del día |
| **Resolver tiempo** | Amarillo | Hay tiempo extra pendiente de aprobar |
| **Revisar día** | Amarillo | Hay algo que requiere tu atención |

### Grilla de cálculo

| Campo | Descripción |
|---|---|
| **Jornada principal** | Rango de entrada → salida real seleccionado como jornada |
| **Trabajo neto detectado** | Tiempo trabajado total (ya descuenta descansos no pagados) |
| **Descansos aplicados** | Minutos de descanso descontados del neto |
| **Tiempo visible** | Base trabajada + compensación + extra aprobado |
| **Tiempo extra** | Minutos de extra detectado por el procesador |
| **Destino extra / banco** | Si ya se resolvió: a pago, a banco, o mixto |
| **Compensación del día** | Si hay compensación aprobada, se suma al visible |

---

## Con turno: revisar bloques del timeline

El timeline muestra los segmentos del día. Cada segmento es un par de marcaciones consecutivas.

### Tipos de segmento

| Tipo | Color | Descripción |
|---|---|---|
| **Turno esperado** | Gris | Referencia del horario configurado |
| **Trabajo detectado** | Azul | Tramo tomado como trabajo principal |
| **Descanso detectado** | Naranja | Tramo interpretado como descanso |
| **Descanso inferido** | Naranja | Descanso inferido automáticamente por coincidencia con ventana configurada |
| **Descanso no descontado** | Verde claro | El empleado no tomó el descanso; el tiempo cuenta como trabajo |
| **Pausa sugerida** | Naranja | Tramo sugerido como descanso (revisa si corresponde) |
| **Bloque extra** | Morado | Tramo detectado antes/después del turno |
| **Salida temporal** | Amarillo | Tramo marcado como salida temporal |
| **Permiso** | Rojo | Tramo marcado como permiso |
| **Tramo no considerado** | Gris oscuro | Tramo excluido del cálculo |

### Editar un segmento

1. Haz clic en el botón de editar del segmento
2. Selecciona la acción:
   - **Trabajo principal**: El tramo se cuenta como parte de la jornada
   - **Bloque extra**: El tramo se cuenta como tiempo adicional
   - **Descanso**: El tramo se descuenta del neto trabajado
   - **No descontar descanso**: El descanso no se descuenta (el empleado no lo tomó)
   - **Salida temporal**: El tramo se excluye del tiempo laborado
   - **Permiso**: El tramo se marca como permiso
   - **No considerar**: El tramo se excluye del cálculo
3. Para descanso, puedes capturar minutos aplicados manualmente (override)
4. Guardar: el sistema reproduce el día con el cambio aplicado

### Reglas de alternancia

Todos los pares de marcas intermedias (entre entrada y salida principal) representan **salida-regreso del puesto** (descansos). El trabajo está implícito entre los descansos.

```
Entrada ──trabajo── Descanso1 ──trabajo── Descanso2 ──trabajo── Salida
```

---

## Sin turno: decidir tiempo extra

Cuando el empleado no tiene turno asignado, el sistema funciona diferente:

### Reglas sin turno

1. **Todo el tiempo trabajado es normal**: el procesador NO detecta tiempo extra automáticamente
2. **El modo de cálculo es "SinTurno"**: asignado automáticamente, no se puede cambiar
3. **El apartado de tiempo extra siempre está visible**: para que decidas cuánto es extra
4. **La sugerencia es el excedente sobre 8h**: si trabajó 10h, sugiere 2h de extra (referencia)
5. **Puedes aprobar cualquier cantidad**: hasta el total trabajado

### Pasos para aprobar extra sin turno

1. Abre el modal del día sin turno
2. Ve a la pestaña **"Tiempo extra"** (siempre visible sin turno)
3. Selecciona el tipo de resolución:
   - **Pagar tiempo extra**: aprueba minutos para pago
   - **Enviar extra a banco**: acumula en banco de horas
   - **Mitad pago / mitad banco**: divide entre ambos
4. Captura los minutos base de pago y/o banco
   - La sugerencia automática es el excedente sobre 8h
   - Puedes capturar cualquier valor hasta el total trabajado
5. (Opcional) Activa el override de factor si quieres un factor de pago diferente
6. Haz clic en **"Aplicar"**
7. El visible refleja: tiempo normal + extra aprobado = tiempo total trabajado

### Ejemplo

- Empleado trabaja de 8:50 a 18:41 = 9:51h (591 min)
- Sugerencia: 1:51h (excedente sobre 8h)
- Usuario aprueba "Pagar 2h" (120 min)
- Resultado: 7:51h normales + 2:00h extra = 9:51h total
- El visible no duplica: siempre coincide con el tiempo total trabajado

---

## Resolver tiempo extra (con turno)

Cuando el procesador detecta tiempo extra (empleado trabajó más del horario programado):

### Pasos

1. El sistema muestra "Extra detectada" en los chips superiores
2. Ve a la pestaña **"Tiempo extra"**
3. El tipo de resolución sugerida depende del escenario:
   - **PagarTodo**: pagar todo el extra
   - **BancoTodo**: enviar todo al banco de horas
   - **MitadMitad**: dividir entre pago y banco
   - **CubrirFaltanteConBanco**: usar banco para cubrir faltante (si hay)
4. Los minutos sugeridos se calculan según el modo:
   - **EntradaSalida**: entrada anticipada + salida posterior
   - **NetoVsNeto**: neto trabajado − neto esperado
5. Puedes ajustar manualmente los minutos de pago y banco
6. (Opcional) Override de factor: usar un factor de pago diferente al configurado
7. Haz clic en **"Aplicar"**
8. El estado cambia a "Tiempo resuelto"

### Quitar resolución

Si quieres revertir una resolución aplicada:
1. Ve a la pestaña "Tiempo extra"
2. Haz clic en **"Quitar resolución"**
3. Los minutos aprobados vuelven a 0

---

## Registrar permiso parcial

Si el empleado tuvo un permiso durante el día:

1. Ve a la pestaña **"Permiso"**
2. Captura las horas del permiso
3. Selecciona si es **con goce** o **sin goce**
   - Con goce: descuenta del banco de horas del empleado
   - Sin goce: no consume banco
4. (Opcional) Captura motivo y observaciones
5. Haz clic en **"Guardar permiso"**
6. El sistema reproduce el día con el permiso aplicado

### Permiso existente

Si ya existe un permiso, puedes:
- **Ajustar**: cambiar horas, goce o motivo
- **Quitar**: retirar el permiso (restaura banco de horas)

---

## Guardar y cerrar

### Cambios pendientes

Cuando haces cambios en el modal (editar segmentos, resolver tiempo, permisos), el sistema marca "Hay cambios locales sin guardar".

### Guardar

1. Los cambios se aplican automáticamente al hacer clic en "Aplicar" o "Guardar"
2. El sistema reproduce el día con cada cambio
3. Para guardar definitivamente: haz clic en **"Guardar cambios"** al cerrar

### Confirmación de cierre

Si intentas cerrar con cambios pendientes:
1. El sistema pregunta si quieres guardar
2. **Guardar y cerrar**: reproduce, guarda y notifica al listado
3. **Cerrar sin guardar**: descarta los cambios locales

### Botones de cierre

| Botón | Acción |
|---|---|
| **Cerrar** | Si hay cambios pendientes, pregunta confirmación |
| **Guardar cambios** | Reproduce, guarda y cierra |

---

## Conceptos clave

| Término | Definición |
|---|---|
| **Tiempo visible** | Tiempo total que se paga: base + compensación + extra aprobado |
| **Tiempo extra detectado** | Extra que el procesador calcula automáticamente (con turno) |
| **Tiempo extra aprobado** | Extra que el usuario autoriza para pago o banco |
| **Faltante descontable** | Diferencia entre jornada esperada y tiempo trabajado |
| **Banco de horas** | Saldo acumulado de tiempo extra para uso futuro |
| **Compensación** | Tiempo recuperado por el empleado que reduce el faltante |
| **Permiso parcial** | Tiempo autorizado de ausencia durante el día |
| **Override de factor** | Factor de pago manual diferente al configurado |
| **Modo SinTurno** | Modo automático sin turno: el usuario decide el extra |
| **Reprocesar** | Recalcular el día con las reglas actuales del procesador |