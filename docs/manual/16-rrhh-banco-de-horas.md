# 16. Banco de horas en `RRHH`

## Objetivo
Este manual explica para qué sirve el `Banco de horas`, cómo se alimenta, cómo se consume y qué relación tiene con `Asistencias`, `Nómina` y el neteo NetoVsNeto en `MundoVs`.

## Alcance
Incluye:
- propósito del banco de horas
- movimientos automáticos y manuales
- relación con horas extra
- relación con nómina y el neteo semanal
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

## Banco como último eslabón del neteo NetoVsNeto

El banco de horas es el **último** paso de la cadena de neteo semanal NetoVsNeto: el pool de
extra (detectado + bajo umbral) tapa primero `faltante → retardo → salida anticipada`, y el
sobrante restante **restaura** el banco consumido (`bancoRestaurado = Min(sobrante, bancoConsumido)`).
El neteo tiene una sola fuente de verdad: el batch `RrhhResolucionPeriodoService.ObtenerResumenesPeriodoBatchAsync`
→ `CalcularNeteoNetoVsNeto`, el mismo que pinta Asistencia Semanal. La nómina **lo consume** sin
recalcular (ver [Reglas de cálculo](./rrhh-reglas-calculo.md#snapshot-de-nómina)).

> **DescartarExtra** (F9): anula el **pago** del extra, no el neteo. Si el operador descarta el
> extra, el banco sigue restaurándose según el neteo **vivo** del periodo (igual que Asistencia
> Semanal); no se fuerza un descuento completo. Para forzarlo, ajustar el neteo en Asistencia
> Semanal.
> English: DescartarExtra annuls the extra PAYMENT, not the neteo. The bank is still restored per
> the period's LIVE neteo (matching Asistencia Semanal); a full dock isn't forced.

## Relación con `Nómina` (fusión 2026-08-22)

La `Prenómina` dejó de existir como etapa separada (fusión prenómina→nómina). La revisión/captura
del periodo ahora vive dentro de Nómina como la fase "Cerrar periodo y calcular" (y "Reabrir
captura"). El banco sigue afectando cómo se interpreta el tiempo extra del periodo:

`Asistencia Semanal -> detecta tiempo extra y netea (pool → faltante → retardo → salida → banco)`

`Banco de horas -> acumula la parte que no se pagará completa en efectivo`

`Nómina -> consume el neteo ya calculado por Asistencia Semanal; paga lo que corresponda pagar`

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
> La `Prenómina` se fusionó con `Nómina` (2026-08-22); la columna "captura" = fase "Cerrar periodo y calcular" dentro de Nómina.
| Concepto | Para qué sirve | Impacta captura en `Nómina` | Impacta `Nómina` |
| --- | --- | --- | --- |
| `Asistencias` | Detecta tiempo extra real y netea | Sí | Sí |
| `Banco de horas` | Guarda la parte acumulable del tiempo extra; último eslabón del neteo | Sí, porque afecta la revisión del periodo | Sí, porque evita pagar como efectivo lo que se acumuló |
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

---

> Última revisión: 2026-08-22

## Ver también
- [10. Marcaciones y asistencias](./10-rrhh-marcaciones-y-asistencias.md)
- [11. Prenómina](./11-rrhh-prenomina.md)
- [12. Nómina](./12-rrhh-nomina.md)
- [17. Ausencias](./17-rrhh-ausencias.md)
- [20. Cálculo de tiempo visible](./20-rrhh-calculo-tiempo-visible.md)
- [Permisos, descansos y banco de horas — plan técnico](./rrhh-permisos-descansos-banco-horas-plan-tecnico.md)
- [Especificación técnica del módulo RRHH](../modulos/detalle/06-rrhh.md)
