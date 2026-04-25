# 14. Recibo de nómina en `RRHH`

## Objetivo
Este manual explica para qué sirve el `Recibo de nómina`, qué relación tiene con la `Nómina` ya calculada y por qué debe entenderse como una salida documental del periodo, no como el lugar donde nace el cálculo.

## Alcance
Incluye:
- propósito del recibo de nómina
- relación con nómina, prenómina y conceptos del periodo
- contenido esperado del recibo
- validaciones previas a su emisión o revisión

No incluye:
- timbrado fiscal detallado
- dispersión bancaria
- cálculo inicial del periodo

## Ruta principal relacionada
- `RRHH > Recibo de nómina`
- referencia de origen: `RRHH > Nóminas`

## Para qué sirve un `Recibo de nómina`
El recibo sirve para presentar de forma clara el resultado final del periodo pagado al empleado.

No es donde se calcula el pago.
No es donde se decide la lógica del empleado.
No es donde se captura el destajo, bono o deducción por primera vez.

Es el documento de salida que resume:
- quién recibe el pago
- qué periodo se está pagando
- qué conceptos integran el total
- cuánto corresponde a percepciones
- cuánto corresponde a deducciones
- cuál es el neto final

## Relación con `Prenómina`
La `Prenómina` revisa y valida de forma preliminar la información del periodo.

Eso significa que el recibo no debería depender de datos improvisados en el último momento, sino de una revisión previa ya hecha.

La relación correcta es:

`Prenómina -> revisión preliminar`

`Nómina -> cálculo final`

`Recibo de nómina -> salida documental del cálculo final`

## Relación con `Nómina`
La relación con la `Nómina` es directa.

El recibo toma como base el resultado final del cálculo del periodo.

Por eso, si la nómina tiene errores, el recibo solo los reflejará; no los corrige por sí mismo.

### Regla práctica
- la `Nómina` calcula
- el `Recibo de nómina` muestra y documenta

## Qué debe reflejar un recibo
Un recibo bien construido debe mostrar al menos:
- empleado
- periodo pagado
- percepciones
- deducciones
- total bruto o equivalente según diseño
- total neto
- desglose suficiente para entender el pago

También debe permitir responder preguntas como:
- qué parte fue sueldo base
- qué parte fue destajo
- qué parte fue bono
- qué deducciones se aplicaron
- cuánto recibió finalmente el empleado

## Qué relación tiene con bonos, destajo y deducciones
El recibo no inventa estos conceptos.

Solo refleja lo que ya fue integrado en la `Nómina`.

### Ejemplo de flujo correcto
- `Esquema de pago` define la lógica
- `Vales de destajo`, asistencias, bonos y deducciones alimentan la revisión
- `Prenómina` valida
- `Nómina` cierra
- `Recibo de nómina` presenta el resultado final

## Validaciones antes de revisar o emitir el recibo
Antes de considerar correcto el recibo, validar:
- que la nómina del periodo ya esté revisada
- que el empleado y el periodo sean correctos
- que percepciones y deducciones cuadren con la nómina
- que el neto final coincida con el cálculo final
- que no falten conceptos relevantes del periodo

## Qué problemas puede revelar un recibo
Aunque no corrige el cálculo, sí ayuda a detectar anomalías visibles, por ejemplo:
- percepciones faltantes
- deducciones inesperadas
- netos atípicos
- conceptos duplicados
- periodo incorrecto

Cuando eso ocurra, la corrección normalmente debe hacerse en la fuente del cálculo, no en el recibo aislado.

## Errores comunes a evitar
- usar el recibo como si fuera la pantalla principal de cálculo
- intentar corregir en el recibo lo que nació mal en nómina
- revisar el recibo sin haber validado antes la nómina
- asumir que el recibo sustituye la revisión de prenómina

## Tabla rápida de relación
| Concepto | Para qué sirve | Impacta `Prenómina` | Impacta `Nómina` | Impacta `Recibo de nómina` |
| --- | --- | --- | --- | --- |
| `Prenómina` | Revisa el cálculo preliminar | Sí, es su función principal | Sí, sirve como base previa | Indirectamente |
| `Nómina` | Cierra el cálculo final del periodo | No es su etapa principal | Sí, es su función principal | Sí, porque el recibo toma ese resultado |
| `Recibo de nómina` | Presenta documentalmente el resultado final | No | Sí, como salida derivada | Sí, es su función principal |
| `Vale de destajo` | Aporta componente variable validado | Sí | Sí | Sí, pero solo reflejado si ya fue integrado |
| `Bonos y deducciones` | Ajustan percepciones y descuentos del periodo | Sí | Sí | Sí, como parte del desglose final |

## Checklist rápido
Antes de cerrar esta etapa, validar:
- nómina final revisada
- periodo correcto
- empleado correcto
- percepciones correctas
- deducciones correctas
- neto correcto
- recibo entendible y trazable

## Relación con manuales anteriores
Este manual ocurre después de:
- [11. Prenómina en RRHH](./11-rrhh-prenomina.md)
- [12. Nómina en RRHH](./12-rrhh-nomina.md)
- [13. Vales de destajo en RRHH](./13-rrhh-vales-destajo.md)

## Referencias relacionadas
- `MundoVs/Components/Pages/RRHH/ReciboNomina.razor`
- `MundoVs/Core/Services/NominaReciboBuilder.cs`
- `MundoVs/Core/Interfaces/INominaReciboBuilder.cs`

## Siguiente manual sugerido
El siguiente paso lógico es:
- `15-rrhh-bonos-y-deducciones.md`
