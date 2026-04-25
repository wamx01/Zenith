# 18. Configuración de bonos en `Admin`

## Objetivo
Este manual explica cómo configurar los `Bonos de nómina` en `Zenith`, incluyendo rubros, estructuras de bono y asignación por posición, para después poder operar `Bonos distribuidos`, `Prenómina`, `Nómina` y `Recibo de nómina`.

## Alcance
Incluye:
- rubros de bono
- estructuras de bono
- tipo de captura
- asignación por posición
- monto base a distribuir

No incluye:
- captura del reparto por empleado en el periodo
- cálculo fiscal externo
- dispersión bancaria

## Ruta principal
- `Admin > Bonos nómina`
- ruta relacionada: `/configuracion/bonos`

## Para qué sirve esta configuración
Esta pantalla sirve para definir la base maestra con la que la empresa administrará bonos.

No es la captura del periodo.
No es la distribución empleado por empleado.

Es la configuración que responde preguntas como:
- qué rubros existen
- qué estructura de bono usa cada posición
- si la captura será por total distribuido u otra modalidad
- cuánto monto base tiene cada posición para repartir cuando aplica

## Parte 1. Rubros de bono
Los rubros son las categorías internas del bono.

Ejemplos:
- productividad
- calidad
- asistencia
- puntualidad
- meta

Cada rubro puede tener:
- clave
- nombre
- orden
- descripción

### Para qué sirven
Sirven para desglosar el bono en componentes entendibles y controlables.

## Parte 2. Estructuras de bono
La estructura define cómo se organiza el bono para un grupo de empleados o una posición.

Puede incluir:
- nombre
- descripción
- tipo de captura
- rubros incluidos
- porcentaje por rubro cuando aplique

## Tipo de captura
La estructura puede trabajar con distintas formas de captura.

En el flujo actual destaca especialmente `TotalDistribuido`.

### `TotalDistribuido`
Se usa cuando existe un monto total a repartir por posición y luego ese monto se distribuye entre empleados por rubro.

Aquí los porcentajes por rubro funcionan como topes máximos dentro del total.

## Parte 3. Asignación por posición
La configuración también permite decir:
- qué posición usa qué estructura
- cuánto monto base distribuirá esa posición

Esto es muy importante porque el empleado no debería inventar su propio bono aislado.

La lógica base del proyecto es:

`Empleado -> Posición -> Estructura de bono`

No se configura el bono directamente por empleado como regla principal.

## Qué relación tiene con `Bonos distribuidos`
La pantalla de `Bonos distribuidos` no debería arrancar sin esta configuración previa.

¿Por qué?
Porque primero necesitas:
- rubros definidos
- estructura definida
- topes definidos
- posición ligada a una estructura
- monto total a repartir

Y solo después puedes capturar el reparto por empleado en el periodo.

## Qué relación tiene con `Prenómina` y `Nómina`
La configuración de bonos por sí sola no paga nada todavía.

Pero sí define la estructura que luego permitirá:
- capturar el reparto
- revisar el impacto en prenómina
- integrar el resultado en nómina
- reflejarlo en el recibo

La secuencia correcta es:

`Configuración de bonos -> Bonos distribuidos -> Prenómina -> Nómina -> Recibo de nómina`

## Qué debe validarse al configurar
Antes de guardar la base de bonos, revisar:
- rubros claros y sin duplicados
- estructura correcta
- tipo de captura correcto
- porcentajes bien definidos cuando aplique
- total de porcentajes consistente
- posición ligada a la estructura correcta
- monto base a distribuir correcto

## Errores comunes a evitar
- crear rubros repetidos
- mezclar conceptos distintos dentro del mismo rubro sin claridad
- asignar estructura incorrecta a la posición
- capturar monto base sin revisar si realmente aplica `TotalDistribuido`
- intentar repartir bonos sin haber dejado primero esta base maestra

## Tabla rápida de relación
| Concepto | Para qué sirve | Impacta `Bonos distribuidos` | Impacta `Prenómina` | Impacta `Nómina` |
| --- | --- | --- | --- | --- |
| `Rubro` | Define categoría del bono | Sí | Sí, indirectamente | Sí, indirectamente |
| `Estructura de bono` | Define cómo se organiza el bono | Sí | Sí, indirectamente | Sí, indirectamente |
| `Posición` | Recibe estructura y monto base | Sí | Sí | Sí |
| `Monto a distribuir` | Define la base repartible del periodo | Sí | Sí, cuando se capture el reparto | Sí, cuando se cierre el cálculo |

## Checklist rápido
Antes de cerrar esta etapa, validar:
- rubros creados
- estructuras creadas
- tipo de captura correcto
- porcentajes correctos si aplica
- posiciones ligadas a estructura correcta
- monto base correcto
- base lista para `Bonos distribuidos`

## Relación con manuales anteriores
Este manual ocurre después de:
- [15. Bonos y deducciones en RRHH](./15-rrhh-bonos-y-deducciones.md)

## Referencias relacionadas
- `MundoVs/Components/Pages/Admin/BonosNomina.razor`
- `MundoVs/Core/Entities/NominaBonoRrhh.cs`
- `MundoVs/Core/Entities/BonoDistribucionPeriodoRrhh.cs`

## Siguiente manual sugerido
El siguiente paso lógico es:
- `19-rrhh-bonos-distribuidos.md`
