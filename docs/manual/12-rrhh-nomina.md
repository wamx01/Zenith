# 12. Nómina en `RRHH`

## Objetivo
Este manual explica cómo usar `Nóminas` en `Zenith` como etapa final de cálculo y cierre del periodo después de revisar la `Prenómina`.

## Alcance
Incluye:
- propósito de la nómina
- relación con prenómina, asistencias y destajo
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

## Diferencia entre `Prenómina` y `Nómina`
### `Prenómina`
Sirve para revisar, ajustar y detectar errores antes del cierre.

### `Nómina`
Sirve para formalizar el cálculo del periodo con importes finales.

### Regla práctica
- en `Prenómina` todavía revisas
- en `Nómina` ya cierras el cálculo

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
- prenómina revisada
- empleados correctos en el periodo
- asistencias revisadas si aplican
- destajo aprobado e integrado si aplica
- incidencias capturadas
- configuración base de nómina validada

## Orden recomendado
La secuencia sugerida es:

`Revisar prenómina -> generar nómina -> revisar detalle por empleado -> recalcular si hace falta -> cerrar periodo`

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
- información de prenómina
- asistencias y horas extra si aplican
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
- generar nómina sin revisar prenómina
- recalcular sin entender qué cambió
- cerrar con empleados faltantes o sobrantes
- no revisar importes atípicos
- mezclar ajustes del periodo actual con errores viejos no investigados

## Checklist rápido
Antes de cerrar esta etapa, validar:
- prenómina revisada
- periodo correcto
- empleados correctos
- variables integradas correctamente
- importes revisados por empleado
- anomalías investigadas
- nómina lista para cierre

## Relación con manuales anteriores
Este manual ocurre después de:
- [11. Prenómina en RRHH](./11-rrhh-prenomina.md)
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
