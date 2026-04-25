# 16. Banco de horas en `RRHH`

## Objetivo
Este manual explica para qué sirve el `Banco de horas`, cómo se alimenta, cómo se consume y qué relación tiene con `Asistencias`, `Prenómina` y `Nómina` en `Zenith`.

## Alcance
Incluye:
- propósito del banco de horas
- movimientos automáticos y manuales
- relación con horas extra
- relación con prenómina y nómina
- control de saldo por empleado

No incluye:
- configuración técnica de checadores
- cálculo fiscal final
- dispersión bancaria

## Ruta principal
- `RRHH > Banco de horas`

## Para qué sirve el `Banco de horas`
El banco de horas sirve para guardar tiempo a favor del empleado cuando la política de la empresa no paga todo el tiempo extra de inmediato como efectivo.

En este proyecto, la regla funcional esperada es:
- por cada hora extra trabajada, se paga `1` hora
- y otra `1` hora se acumula en banco de horas
- con un tope configurable

Eso significa que el banco de horas funciona como una reserva de tiempo acumulado que después puede:
- mantenerse como saldo
- consumirse
- ajustarse manualmente con control

## Qué registra el banco de horas
El banco maneja movimientos por empleado, por ejemplo:
- generado por horas extra
- ajuste manual
- consumo

Cada movimiento debe permitir entender:
- fecha
- tipo de movimiento
- horas registradas
- referencia
- notas u observaciones

## Tipos de movimiento
### `GeneradoPorHorasExtra`
Es el movimiento automático que nace cuando el empleado genera horas extra elegibles para banco.

### `AjusteManual`
Sirve para corregir o regularizar saldo con control administrativo.

Puede ser:
- positivo
- negativo

### `Consumo`
Sirve para descontar del saldo horas ya usadas por el empleado.

## Relación con `Asistencias`
La fuente natural del banco de horas es la asistencia ya interpretada.

Primero debe saberse:
- cuánto tiempo extra trabajó realmente el empleado
- si ese tiempo fue válido
- si entra a la política del banco

Después de eso, puede generarse el movimiento correspondiente.

### Regla práctica
- `Asistencias` detecta el tiempo extra
- `Banco de horas` guarda la parte acumulable de ese tiempo

## Relación con `Prenómina`
La `Prenómina` debe saber que el tiempo extra puede tener dos efectos:
- una parte pagable en el periodo
- otra parte acumulable en banco de horas

Por eso, el banco no reemplaza a la prenómina, pero sí afecta cómo se interpreta el tiempo extra del periodo.

La relación correcta es:

`Asistencia -> detecta tiempo extra`

`Banco de horas -> acumula la parte que no se pagará completa en efectivo`

`Prenómina -> revisa cómo impacta ese periodo`

## Relación con `Nómina`
La `Nómina` toma el resultado ya validado del periodo.

Eso significa que, si parte del tiempo extra se acumuló a banco, la nómina no debería tratar ese componente como si todo fuera pago directo.

En otras palabras:
- la nómina paga lo que corresponda pagar
- el banco conserva lo que corresponda acumular

## Qué es el saldo del banco
El saldo actual del empleado es el resultado de:
- acumulado automático
- más o menos ajustes manuales
- menos consumos

Ese saldo debe ser visible y trazable para evitar:
- pagos dobles
- acumulación incorrecta
- consumos sin respaldo

## Cuándo usar ajustes manuales
Los ajustes manuales deben usarse con control, por ejemplo cuando:
- hubo una corrección operativa validada
- se migró información previa
- se detectó una diferencia histórica
- se autorizó una regularización

No conviene usarlos como sustituto de corregir el origen si el problema viene de asistencia o reglas del periodo.

## Cuándo registrar consumo
Conviene registrar consumo cuando el empleado ya hizo uso del saldo acumulado.

El objetivo es que el banco refleje la realidad y no solo acumulación teórica.

## Tabla rápida de relación
| Concepto | Para qué sirve | Impacta `Prenómina` | Impacta `Nómina` |
| --- | --- | --- | --- |
| `Asistencias` | Detecta tiempo extra real | Sí | Sí |
| `Banco de horas` | Guarda la parte acumulable del tiempo extra | Sí, porque afecta la revisión del periodo | Sí, porque evita pagar como efectivo lo que se acumuló |
| `Ajuste manual` | Corrige saldo con control | Indirectamente | Indirectamente |
| `Consumo` | Descuenta horas ya usadas del saldo | Indirectamente | Indirectamente |

## Errores comunes a evitar
- acumular horas sin validar primero la asistencia
- usar ajustes manuales para ocultar errores de origen
- consumir saldo sin registrar movimiento
- pagar como efectivo horas que ya se fueron a banco
- perder trazabilidad del saldo por empleado

## Checklist rápido
Antes de cerrar esta etapa, validar:
- política de banco de horas definida
- movimientos automáticos entendidos
- ajustes manuales controlados
- consumos registrados
- saldo por empleado visible
- relación con asistencia y nómina entendida

## Relación con manuales anteriores
Este manual ocurre después de:
- [10. Marcaciones y asistencias en RRHH](./10-rrhh-marcaciones-y-asistencias.md)
- [11. Prenómina en RRHH](./11-rrhh-prenomina.md)
- [12. Nómina en RRHH](./12-rrhh-nomina.md)

## Referencias relacionadas
- `MundoVs/Components/Pages/RRHH/BancoHoras.razor`
- `MundoVs/Core/Entities/RrhhBancoHorasMovimiento.cs`
- `MundoVs/Core/Entities/NominaConfiguracion.cs`

## Siguiente manual sugerido
El siguiente paso lógico es:
- `17-rrhh-ausencias.md`
