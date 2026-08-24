# 12. Nómina en `RRHH`

## Objetivo
Este manual explica cómo usar `Nóminas` en `MundoVs` como etapa final de cálculo y cierre del periodo, inmediatamente después de `Asistencia Semanal`. La antigua etapa de `Prenómina` quedó absorbida dentro de Nómina (fusión 2026-08-22).

## Alcance
Incluye:
- propósito de la nómina
- relación con asistencias y destajo
- captura y cierre del periodo (antigua prenómina, ahora fase de Nómina)
- validaciones previas al cierre
- revisión final por empleado

No incluye:
- timbrado CFDI
- dispersión bancaria
- procesos fiscales externos

## Ruta principal
- `RRHH > Nóminas`

## Qué es la nómina
La nómina es el cálculo final del periodo para cada empleado.

Aquí ya no se trabaja solo con revisión preliminar, sino con el resultado que servirá como base formal de pago.

La nómina consolida información como:
- sueldo base
- días pagables
- asistencias
- horas extra
- vales de destajo aprobados
- bonos
- deducciones
- total a pagar

## Qué pasó con la `Prenómina` (fusión 2026-08-22)
La etapa separada de `Prenómina` fue eliminada. La captura y revisión previa al cierre ahora viven **dentro de Nómina**:

- **Cerrar periodo y calcular** — estampa `FechaCierreCaptura` y ejecuta el cálculo del periodo (solo sobre una semana cerrada, `NominaPeriodoHelper.ObtenerPeriodo`).
- **Reabrir captura** — limpia `FechaCierreCaptura` para que el operador pueda corregir y revisar antes de cerrar de forma definitiva.

La revisión "antes de cerrar" sigue siendo posible: mientras la captura esté abierta (o se reabra) el detalle sigue siendo ajustable. La página `/rrhh/prenominas` ahora redirige a `/rrhh/nominas`.

## Por qué no conviene brincar directo a nómina
Si se genera nómina sin pasar por una revisión previa, pueden quedar errores como:
- asistencias mal interpretadas
- destajo faltante o duplicado
- bonos incompletos
- deducciones incorrectas
- empleados fuera del periodo
- periodicidad equivocada

## Base previa requerida
Antes de generar nómina, ya debe existir:
- asistencias semanal procesada y neteada
- empleados correctos en el periodo
- asistencias revisadas si aplican
- destajo aprobado e integrado si aplica
- incidencias capturadas
- configuración base de nómina validada

## Orden recomendado
La secuencia sugerida es:

`Asistencia Semanal (neteo) -> Nómina: Cargar empleados desde asistencias -> revisar detalle por empleado -> Cerrar periodo y calcular -> (Reabrir captura si hace falta)`

## Neteo y deducciones (fuente única)
La nómina **no recalcula** el neteo `NetoVsNeto` de la semana. El único dueño del neteo es el proceso por lote `RrhhResolucionPeriodoService.ObtenerResumenesPeriodoBatchAsync` → `CalcularNeteoNetoVsNeto`, que es el mismo que pinta `Asistencia Semanal`. La nómina lo consume tal cual:

- un faltante/retardo/salida que en `Asistencia Semanal` se neteó a `—` (cero) **no reaparece** como deducción en nómina, con o sin resolución `Autorizada`;
- el snapshot de nómina (`RrhhNominaSnapshotService`) llama al lote una sola vez y sobrescribe las 3 deducciones diarias con el valor canónico (detectado − absorbido);
- el sourcing de tiempo extra (`NominaTiempoExtraSourcing`) **no recalcula** neteo: en ambos caminos (periodo o incidencia) toma las deducciones del input ya neteado; el camino "periodo" solo intercambia el extra autorizado a pagar.

### Descartar extra (F9)
Cuando el operador descarta el tiempo extra, se anula **solo el pago** del extra, **no el neteo de deducciones**. El faltante/retardo/salida se descuenta según el neteo **en vivo** del periodo (el mismo que muestra Asistencia Semanal). Para forzar un descuento completo, ajusta el neteo en `Asistencia Semanal`; no uses el descarte del extra para eso.

## Paso 1. Confirmar el periodo correcto
Antes de generar o recalcular, validar:
- fecha inicial
- fecha final
- periodicidad
- grupo de empleados
- empresa correcta

Resultado esperado:
- la nómina corresponde exactamente al periodo que se desea cerrar

---

## Paso 2. Integrar la información consolidada
La nómina debe tomar como base:
- sueldo o esquema del empleado
- asistencias y horas extra si aplican (neteo ya calculado por Asistencia Semanal)
- vales de destajo aprobados
- bonos
- deducciones

Resultado esperado:
- cada empleado tiene un cálculo completo del periodo

---

## Paso 3. Revisar detalle por empleado
Antes de cerrar, revisar por empleado:
- sueldo base
- días trabajados o pagables
- destajo
- bonos
- horas extra
- deducciones
- total final

Resultado esperado:
- el total por empleado es entendible y trazable

---

## Paso 4. Detectar diferencias o importes atípicos
Revisar con especial atención:
- empleados con total en cero sin justificación
- empleados con montos muy altos o muy bajos
- duplicidad de destajo
- deducciones excesivas
- horas extra inusuales

Resultado esperado:
- las anomalías se detectan antes del cierre final

---

## Paso 5. Recalcular solo cuando haya causa clara
Si algo no cuadra, revisar primero el origen:
- asistencia
- destajo
- incidencia
- esquema de pago
- periodicidad

Después corregir y recalcular.

Resultado esperado:
- el recálculo se hace con control y no como prueba ciega

---

## Paso 6. Cerrar la nómina del periodo
Cuando el detalle ya fue validado, la nómina puede considerarse lista para cierre operativo.

Resultado esperado:
- existe un cálculo final confiable por empleado y por periodo

## Qué debe salir de una buena nómina
Una nómina bien cerrada debe dejar claro:
- cuánto se paga a cada empleado
- qué conceptos forman el total
- qué parte fue fija
- qué parte fue variable
- qué deducciones se aplicaron
- qué periodo se está pagando

## Errores comunes a evitar
- generar nómina sin tener la asistencias semanal procesada y neteada
- recalcular sin entender qué cambió
- cerrar con empleados faltantes o sobrantes
- no revisar importes atípicos
- mezclar ajustes del periodo actual con errores viejos no investigados

## Checklist rápido
Antes de cerrar esta etapa, validar:
- asistencias semanal procesada y neteada
- periodo correcto
- empleados correctos
- variables integradas correctamente
- importes revisados por empleado
- anomalías investigadas
- nómina lista para cierre

## Relación con manuales anteriores
Este manual ocurre después de:
- [11. Prenómina en RRHH](./11-rrhh-prenomina.md) (ahora redirige a Nómina)
- [10. Marcaciones y asistencias en RRHH](./10-rrhh-marcaciones-y-asistencias.md)
- [08. Esquemas de pago en RRHH](./08-rrhh-esquemas-de-pago.md)

## Referencias relacionadas
- `../modulos/detalle/06-rrhh.md`
- `MundoVs/Components/Pages/RRHH/Nominas.razor`
- `MundoVs/Components/Pages/RRHH/ReciboNomina.razor`

## Siguiente manual sugerido
El siguiente paso lógico es:
- `13-rrhh-vales-destajo.md`
- o `13-rrhh-recibo-nomina.md` si quieres separar la parte de salida documental

---

> Última revisión: 2026-08-22

## Ver también
- [05. Configuración base de nómina](./05-admin-configuracion-nomina.md)
- [10. Marcaciones y asistencias](./10-rrhh-marcaciones-y-asistencias.md)
- [11. Prenómina](./11-rrhh-prenomina.md)
- [13. Vales de destajo](./13-rrhh-vales-destajo.md)
- [14. Recibo de nómina](./14-rrhh-recibo-nomina.md)
- [15. Bonos y deducciones](./15-rrhh-bonos-y-deducciones.md)
- [17. Ausencias](./17-rrhh-ausencias.md)
- [Mapa técnico de nómina](./rrhh-nomina-tecnica.md)
- [Especificación técnica del módulo RRHH](../modulos/detalle/06-rrhh.md)
