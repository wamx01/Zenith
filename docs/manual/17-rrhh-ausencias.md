# 17. Ausencias en `RRHH`

## Objetivo
Este manual explica para qué sirven las `Ausencias`, qué tipos maneja el sistema y cómo se relacionan con `Asistencias`, `Prenómina` y `Nómina` en `Zenith`.

## Alcance
Incluye:
- propósito del registro de ausencias
- vacaciones y permisos
- estatus de la ausencia
- relación con goce de pago
- impacto en prenómina y nómina

No incluye:
- cálculo fiscal detallado
- dispersión bancaria
- reglas externas específicas fuera del flujo funcional del sistema

## Ruta principal
- `RRHH > Ausencias`

## Para qué sirve una `Ausencia`
La ausencia sirve para registrar formalmente que el empleado no laborará o no laboró en un rango determinado bajo una causa controlada.

En este flujo, la pantalla está pensada principalmente para manejar:
- `Vacaciones`
- `Permisos`

Con historial por empleado.

## Qué información registra
Una ausencia puede guardar:
- empleado
- tipo
- estatus
- fecha inicio
- fecha fin
- días
- horas
- si es con goce de pago
- motivo
- observaciones
- aprobación

## Tipos principales
### `Vacaciones`
Se usan para registrar periodos vacacionales del empleado.

En este flujo, normalmente se consideran con goce de pago.

### `Permiso`
Se usa para registrar una ausencia autorizada que puede ser:
- con goce
- sin goce

Según la política interna y la captura realizada.

## Estatus de la ausencia
El estatus ayuda a distinguir si la ausencia:
- apenas fue solicitada
- ya fue aprobada
- ya fue aplicada
- sigue pendiente o requiere revisión

Esto es importante porque no toda ausencia capturada debería impactar igual de inmediato el cálculo del periodo.

## Relación con `Asistencias`
La ausencia no sustituye a la asistencia, pero sí la explica o la complementa.

Por ejemplo:
- si el empleado no aparece con jornada normal
- la ausencia puede justificar ese comportamiento
- evita tratar automáticamente el día como si fuera solo falta injustificada

### Regla práctica
- `Asistencias` muestra lo que pasó en tiempo y marcaciones
- `Ausencias` explica por qué el empleado no laboró o no debía laborar en ese rango

## Relación con `Prenómina`
La `Prenómina` debe considerar las ausencias para revisar correctamente conceptos como:
- días trabajados
- días pagables
- vacaciones
- permisos
- incidencias del periodo

Aquí es donde conviene validar si la ausencia:
- corresponde al periodo
- tiene estatus correcto
- debe afectar pago o no
- tiene goce o no

## Relación con `Nómina`
La `Nómina` toma la información ya validada del periodo.

Eso significa que una ausencia correctamente revisada puede afectar el cálculo final según su naturaleza.

Ejemplos:
- vacaciones pagadas
- permiso con goce
- permiso sin goce
- otra incidencia que reduzca días pagables

La idea correcta es:

`Ausencia -> registro controlado de la incidencia`

`Prenómina -> revisión de su impacto`

`Nómina -> aplicación final al periodo`

## Qué significa `Con goce de pago`
Este campo ayuda a distinguir si la ausencia:
- conserva pago
- o reduce pago

Es clave para no mezclar:
- vacaciones pagadas
- permisos pagados
- permisos sin goce

## Qué debe revisarse antes de aplicarla
Antes de considerar una ausencia lista para impacto operativo, revisar:
- empleado correcto
- tipo correcto
- rango correcto
- días y horas correctos
- estatus correcto
- motivo claro
- goce de pago correcto

## Errores comunes a evitar
- capturar ausencia sin rango correcto
- aplicar permiso sin definir si tiene goce
- duplicar ausencias del mismo periodo
- dejar estatus ambiguo y aun así usarla en cálculo
- tratar vacaciones y permisos como si fueran iguales siempre

## Tabla rápida de relación
| Concepto | Para qué sirve | Impacta `Asistencias` | Impacta `Prenómina` | Impacta `Nómina` |
| --- | --- | --- | --- | --- |
| `Ausencia` | Registra una incidencia formal del empleado | Sí, como explicación del día o rango | Sí, porque afecta revisión del periodo | Sí, porque puede modificar días pagables o incidencias |
| `Con goce de pago` | Define si la ausencia conserva pago | Indirectamente | Sí | Sí |
| `Vacaciones` | Registra descanso vacacional | Sí | Sí | Sí |
| `Permiso` | Registra ausencia autorizada | Sí | Sí | Sí |

## Checklist rápido
Antes de cerrar esta etapa, validar:
- ausencia capturada con empleado correcto
- tipo correcto
- rango correcto
- goce de pago correcto
- estatus correcto
- impacto revisado en prenómina
- impacto final entendido para nómina

## Relación con manuales anteriores
Este manual ocurre después de:
- [10. Marcaciones y asistencias en RRHH](./10-rrhh-marcaciones-y-asistencias.md)
- [11. Prenómina en RRHH](./11-rrhh-prenomina.md)
- [12. Nómina en RRHH](./12-rrhh-nomina.md)

## Referencias relacionadas
- `MundoVs/Components/Pages/RRHH/Ausencias.razor`
- `MundoVs/Core/Entities/RrhhAusencia.cs`

## Siguiente manual sugerido
El siguiente paso lógico es:
- `18-admin-configuracion-bonos.md`
