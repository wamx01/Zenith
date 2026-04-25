# 13. Vales de destajo en `RRHH`

## Objetivo
Este manual explica para qué sirven los `Vales de destajo`, cómo deben capturarse y cuál es su relación con `Esquemas de pago`, `Prenómina` y `Nómina` en `Zenith`.

## Alcance
Incluye:
- propósito del vale de destajo
- relación con procesos, pedidos y empleados
- diferencia entre captura y pago
- impacto en prenómina y nómina
- importancia del estatus de aprobación

No incluye:
- cálculo fiscal final
- dispersión bancaria
- cierre documental del recibo de nómina

## Ruta principal
- `RRHH > Vales de destajo`

## Para qué sirve un `Vale de destajo`
El vale de destajo sirve para registrar trabajo productivo variable que después puede convertirse en pago.

No es todavía la nómina.
No es todavía el pago final.

Es un documento operativo que deja evidencia de:
- qué empleado trabajó
- en qué proceso trabajó
- en qué pedido trabajó
- cuánta producción realizó
- qué tarifa se aplicó
- qué importe generó

En términos prácticos, el vale convierte producción registrada en un importe variable controlado.

## Qué relación tiene con `Esquema de pago`
El vale no vive aislado.

Su sentido depende del `Esquema de pago` del empleado.

### Relación correcta
- el `Esquema de pago` define la lógica
- el `Vale de destajo` registra la evidencia variable del periodo
- la `Prenómina` revisa e integra esa evidencia
- la `Nómina` la convierte en parte del cálculo final

### Ejemplo
Si un empleado tiene esquema:
- `Sueldo fijo`

Normalmente el vale de destajo no sería su fuente principal de pago variable.

Si un empleado tiene esquema:
- `Destajo por operación`
- `Destajo por pieza`
- `Mixto`

Entonces el vale sí puede impactar directamente su cálculo del periodo.

## Qué información registra un vale
Normalmente un vale de destajo guarda:
- empleado
- proceso o tipo de proceso
- pedido relacionado
- cantidad producida
- tarifa aplicada
- importe generado
- estatus del vale
- fechas del periodo o captura

## Por qué no debe brincarse esta etapa
Si el destajo se mete directo a nómina sin pasar por vales, se vuelve más difícil:
- auditar de dónde salió el importe
- saber qué proceso lo generó
- validar si corresponde al periodo correcto
- evitar duplicados
- revisar aprobaciones

El vale sirve como capa de control operativo antes del pago.

## Estatus típicos del vale
Los vales suelen pasar por estados como:
- borrador
- aprobado
- enviado a nómina
- pagado
- cancelado

La idea es que no todo lo capturado se pague automáticamente.

## Importancia del estatus `Aprobado`
Este punto es clave.

Un vale capturado no debería impactar automáticamente la `Prenómina` o la `Nómina` solo por existir.

Primero debe validarse.

### Por qué
Porque puede haber errores en:
- cantidad
- proceso
- pedido
- tarifa
- empleado
- periodo

### Regla práctica
- `capturado` no significa `pagable`
- `aprobado` es el estado que normalmente lo vuelve elegible para integrarse

## Relación con `Prenómina`
La `Prenómina` toma los vales para construir el cálculo preliminar del periodo.

Pero debería considerar solo los vales que cumplan condiciones como:
- estar aprobados
- pertenecer al periodo correcto
- corresponder al empleado correcto
- no estar duplicados
- no haber sido ya integrados indebidamente en otro cálculo

### Para qué le sirven a la prenómina
Le sirven para:
- sumar el componente variable del empleado
- revisar cuánto del periodo viene por producción
- detectar diferencias antes del cierre

En otras palabras:
- el vale aporta evidencia productiva
- la prenómina la revisa e integra de manera preliminar

## Relación con `Nómina`
La `Nómina` usa lo ya validado en `Prenómina` para cerrar el cálculo final.

Aquí el destajo ya no debería entrar como dato improvisado, sino como un importe previamente revisado.

### Para qué le sirven a la nómina
Le sirven para:
- consolidar el pago variable del periodo
- sumarlo al total final del empleado
- dejar trazabilidad del origen del componente variable

La idea correcta es:

`Vale de destajo -> evidencia operativa`

`Prenómina -> revisión e integración preliminar`

`Nómina -> integración final al pago`

## Cuándo sí impacta y cuándo no
### Sí impacta cuando:
- el empleado usa destajo o esquema mixto
- el vale está aprobado
- el vale pertenece al periodo correcto
- la tarifa y el importe están validados

### No debería impactar cuando:
- el vale está en borrador
- el vale está cancelado
- el vale no corresponde al periodo
- el vale tiene errores de captura
- el empleado no usa una lógica de pago que deba integrar ese destajo

## Relación con procesos y pedidos
El vale también sirve para conectar el pago variable con la operación real.

Eso ayuda a responder preguntas como:
- qué pedido generó el destajo
- en qué proceso trabajó el empleado
- cuál fue la tarifa aplicada
- cuánto produjo realmente

Esa trazabilidad es importante para revisar producción y pago al mismo tiempo.

## Errores comunes a evitar
- capturar vales sin validar tarifa
- integrar vales en borrador a prenómina
- usar vales fuera del periodo correcto
- duplicar vales del mismo trabajo
- pagar destajo sin poder rastrear proceso o pedido
- mezclar captura operativa con pago final sin etapa de revisión

## Tabla rápida de relación
| Concepto | Para qué sirve | Impacta `Prenómina` | Impacta `Nómina` |
| --- | --- | --- | --- |
| `Esquema de pago` | Define si el empleado puede usar lógica de destajo, fija o mixta | Sí | Sí |
| `Vale de destajo` | Registra evidencia productiva e importe variable | Sí, si está aprobado y en periodo | Sí, como parte del cálculo final |
| `Prenómina` | Revisa e integra el destajo antes del cierre | Es la etapa principal de validación | Sirve como base previa |
| `Nómina` | Consolida el pago final del empleado | No es su etapa principal de revisión | Sí, integra el importe final |

## Checklist rápido
Antes de cerrar esta etapa, validar:
- vales capturados con empleado correcto
- proceso y pedido correctos
- tarifa correcta
- importe revisado
- estatus aprobado cuando corresponda
- integración correcta a prenómina
- sin duplicados antes de nómina

## Relación con manuales anteriores
Este manual ocurre después de:
- [08. Esquemas de pago en RRHH](./08-rrhh-esquemas-de-pago.md)
- [11. Prenómina en RRHH](./11-rrhh-prenomina.md)
- [12. Nómina en RRHH](./12-rrhh-nomina.md)

## Referencias relacionadas
- `../modulos/detalle/06-rrhh.md`
- `MundoVs/Components/Pages/RRHH/ValesDestajo.razor`

## Siguiente manual sugerido
El siguiente paso lógico es:
- `14-rrhh-recibo-nomina.md`
- o `14-rrhh-bonos-y-deducciones.md`
