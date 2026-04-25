# 15. Bonos y deducciones en `RRHH`

## Objetivo
Este manual explica para qué sirven los `Bonos` y las `Deducciones`, cómo se relacionan con `Prenómina`, `Nómina` y `Recibo de nómina`, y por qué deben manejarse como conceptos controlados del periodo.

## Alcance
Incluye:
- propósito de bonos y deducciones
- diferencia entre percepción adicional y descuento
- relación con prenómina, nómina y recibo
- validaciones antes del cierre

No incluye:
- cálculo fiscal detallado por obligación externa
- dispersión bancaria
- timbrado CFDI detallado

## Rutas principales relacionadas
- `Admin > Bonos nómina`
- `RRHH > Prenóminas`
- `RRHH > Nóminas`
- `RRHH > Recibo de nómina`

## Para qué sirven los `Bonos`
Los bonos sirven para agregar una percepción adicional al pago del empleado.

Pueden representar, por ejemplo:
- cumplimiento de meta
- incentivo por productividad
- apoyo o percepción especial del periodo
- distribución de bono definida por la empresa

En términos prácticos, el bono aumenta el total de percepciones del periodo cuando corresponde.

## Para qué sirven las `Deducciones`
Las deducciones sirven para registrar descuentos o disminuciones aplicables al pago del empleado.

Pueden representar, por ejemplo:
- descuentos autorizados
- ajustes del periodo
- conceptos que reducen el neto final

En términos prácticos, la deducción reduce el pago neto del empleado.

## Diferencia práctica
### `Bono`
- suma al pago
- forma parte de percepciones

### `Deducción`
- resta al pago
- forma parte de descuentos del periodo

## Relación con `Prenómina`
La `Prenómina` es donde conviene revisar si bonos y deducciones del periodo están completos, correctos y justificados.

Aquí ayudan a responder preguntas como:
- ¿el empleado debe recibir un bono este periodo?
- ¿el monto está correcto?
- ¿hay una deducción que deba aplicarse?
- ¿la deducción corresponde realmente a este corte?

La idea es que en prenómina se detecten errores antes del cierre final.

## Relación con `Nómina`
La `Nómina` toma los bonos y deducciones ya revisados para incorporarlos al cálculo final.

Por eso, en esta etapa ya no deberían entrar como ocurrencias de último minuto, sino como conceptos previamente validados.

### Regla práctica
- `Prenómina` revisa si deben existir y cuánto deben valer
- `Nómina` los consolida en el cálculo final

## Relación con `Recibo de nómina`
El `Recibo de nómina` no crea bonos ni deducciones.

Solo refleja el resultado final ya integrado en la nómina.

Por eso, si un bono falta o una deducción está mal, el lugar correcto para corregir suele ser la fuente del cálculo y no el recibo aislado.

## Flujo correcto
La secuencia recomendada es:

`Definir bono o deducción -> revisar en prenómina -> integrar en nómina -> reflejar en recibo`

## Qué debe validarse en un bono
Antes de integrarlo al periodo, revisar:
- que corresponda al empleado o grupo correcto
- que el monto sea correcto
- que la regla o motivo esté claro
- que no esté duplicado
- que corresponda al periodo correcto

## Qué debe validarse en una deducción
Antes de integrarla al periodo, revisar:
- que exista justificación
- que el monto sea correcto
- que el descuento corresponda al periodo
- que no se esté aplicando doble
- que no afecte indebidamente el neto del empleado

## Cuándo sí impactan y cuándo no
### Sí impactan cuando:
- corresponden al periodo correcto
- fueron revisados
- están ligados al empleado o grupo correcto
- deben formar parte real del cálculo del periodo

### No deberían impactar cuando:
- son capturas provisionales sin validar
- pertenecen a otro periodo
- están duplicados
- no existe justificación clara

## Tabla rápida de relación
| Concepto | Para qué sirve | Impacta `Prenómina` | Impacta `Nómina` | Impacta `Recibo de nómina` |
| --- | --- | --- | --- | --- |
| `Bono` | Suma percepción adicional al periodo | Sí, se revisa antes del cierre | Sí, se integra al cálculo final | Sí, se refleja como percepción |
| `Deducción` | Resta importe del neto del periodo | Sí, se revisa antes del cierre | Sí, se integra al cálculo final | Sí, se refleja como descuento |
| `Prenómina` | Revisa y valida conceptos del periodo | Es su función principal | Sirve como base previa | Indirectamente |
| `Nómina` | Consolida el cálculo final | No es su etapa principal | Es su función principal | Sí, alimenta el recibo |
| `Recibo de nómina` | Muestra el resultado final | No | Sí, como salida derivada | Es su función principal |

## Errores comunes a evitar
- capturar bonos sin validar motivo o monto
- meter deducciones al cierre sin revisión previa
- duplicar bonos o descuentos del mismo periodo
- corregir en el recibo lo que nació mal en prenómina o nómina
- mezclar bonos permanentes con ajustes extraordinarios sin distinguirlos

## Checklist rápido
Antes de cerrar esta etapa, validar:
- bonos correctos
- deducciones correctas
- periodo correcto
- sin duplicados
- prenómina revisada
- nómina consolidada
- recibo reflejando bien el resultado final

## Relación con manuales anteriores
Este manual ocurre después de:
- [11. Prenómina en RRHH](./11-rrhh-prenomina.md)
- [12. Nómina en RRHH](./12-rrhh-nomina.md)
- [14. Recibo de nómina en RRHH](./14-rrhh-recibo-nomina.md)

## Referencias relacionadas
- `MundoVs/Components/Pages/Admin/BonosNomina.razor`
- `MundoVs/Components/Pages/RRHH/Nominas.razor`
- `MundoVs/Components/Pages/RRHH/ReciboNomina.razor`
- `MundoVs/Core/Entities/NominaBonoRrhh.cs`
- `MundoVs/Core/Entities/NominaDeduccionRrhh.cs`

## Siguiente manual sugerido
El siguiente paso lógico es:
- continuar con manuales por subflujo específico de `RRHH`
- o abrir la secuencia de `Ventas`
