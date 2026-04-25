# 19. Bonos distribuidos en `RRHH`

## Objetivo
Este manual explica cómo funciona la captura de `Bonos distribuidos` por periodo, cuál es su relación con la configuración maestra de bonos y cómo impacta `Prenómina`, `Nómina` y `Recibo de nómina`.

## Alcance
Incluye:
- captura por periodo
- distribución por posición
- distribución por rubro
- topes porcentuales
- relación con monto base y reparto entre empleados

No incluye:
- definición maestra de rubros y estructuras
- cálculo fiscal externo
- dispersión bancaria

## Ruta principal
- `RRHH > Bonos distribuidos`

## Para qué sirve `Bonos distribuidos`
Esta pantalla sirve para capturar cómo se reparte el bono de un periodo entre los empleados elegibles.

No define el bono desde cero.
No sustituye la configuración maestra.

Lo que hace es tomar una base ya configurada y aterrizarla por periodo.

## Base previa requerida
Antes de usar esta pantalla, ya debe existir:
- rubros de bono
- estructura de bono
- posición ligada a esa estructura
- monto base a distribuir
- empleados activos en la posición

## Cómo funciona la lógica
La pantalla trabaja por:
- período
- posición
- estructura de bono
- rubros
- empleados

Y permite capturar por empleado el porcentaje que recibirá en cada rubro.

## Qué significan los porcentajes por rubro
Los porcentajes configurados por rubro son topes máximos.

Eso significa que:
- el empleado puede recibir menos del tope
- pero no debe excederlo

En otras palabras:
- la estructura define el límite
- la captura del periodo define el reparto real

## Qué relación tiene con el monto base
Cada posición puede tener un monto total a distribuir.

A partir de ese monto:
- se reparten porcentajes por rubro
- y esos porcentajes se convierten en monto asignado por empleado

Por eso la pantalla muestra cosas como:
- monto base
- capturado
- remanente
- monto asignado

## Qué relación tiene con la posición
La posición es clave porque el bono distribuido nace desde la estructura asignada a esa posición.

Esto sigue la regla funcional del proyecto:

`Empleado -> Posición -> Estructura de bono -> Distribución por periodo`

No debe tratarse como una configuración libre por empleado sin base organizacional.

## Qué relación tiene con `Prenómina`
La `Prenómina` usa la captura distribuida para revisar el componente de bono del periodo.

Aquí conviene validar:
- empleados correctos
- rubros correctos
- porcentajes dentro de tope
- montos correctos
- periodo correcto
- sin duplicados

La prenómina no debería inventar el reparto; debería tomarlo ya capturado y revisarlo.

## Qué relación tiene con `Nómina`
La `Nómina` consolida el bono distribuido ya validado en el cálculo final del periodo.

Eso significa que el bono ya no debería entrar como ajuste improvisado, sino como una distribución previamente definida y revisada.

## Qué relación tiene con `Recibo de nómina`
El `Recibo de nómina` refleja el resultado final del bono integrado.

No define el reparto.
No lo corrige.

Solo lo muestra dentro del total final del empleado.

## Regla importante del proyecto
En este dominio, la pantalla `Bonos` define por `Posición` el monto total a distribuir y las categorías o rubros del esquema.

La pantalla `Bonos distribuidos`:
- muestra la captura por rubro en columnas
- permite asignar porcentaje por rubro a cada empleado
- limita cada rubro al tope permitido
- debe mantener control sobre el total capturado

Y funcionalmente debe entenderse como:
- una distribución del monto del periodo
- no una redefinición del monto base ni de la estructura

## Qué debe validarse antes de guardar
Antes de guardar la captura del periodo, revisar:
- período correcto
- posición correcta
- empleados correctos
- rubros correctos
- porcentajes dentro del tope
- monto asignado coherente
- remanente entendido
- observaciones cuando hagan falta

## Errores comunes a evitar
- capturar distribución sin configuración maestra previa
- exceder topes por rubro
- usar la distribución para redefinir el monto base
- mezclar empleados de otra posición
- duplicar reparto del mismo periodo
- cerrar nómina sin revisar esta captura si el bono aplica

## Tabla rápida de relación
| Concepto | Para qué sirve | Impacta `Prenómina` | Impacta `Nómina` | Impacta `Recibo de nómina` |
| --- | --- | --- | --- | --- |
| `Configuración de bonos` | Define rubros, estructura y monto base | Sí, indirectamente | Sí, indirectamente | Sí, indirectamente |
| `Bonos distribuidos` | Captura el reparto real del periodo | Sí, es la base revisable del bono distribuido | Sí, se integra al cálculo final | Sí, se refleja ya integrado |
| `Prenómina` | Revisa el reparto antes del cierre | Es su función principal de validación | Sirve como base previa | Indirectamente |
| `Nómina` | Consolida el bono del periodo | No es su etapa principal de revisión | Es su función principal | Sí |
| `Recibo de nómina` | Muestra el resultado final | No | Sí, como salida derivada | Es su función principal |

## Checklist rápido
Antes de cerrar esta etapa, validar:
- periodo correcto
- posición correcta
- estructura correcta
- rubros correctos
- topes respetados
- montos coherentes
- prenómina lista para revisar el bono

## Relación con manuales anteriores
Este manual ocurre después de:
- [15. Bonos y deducciones en RRHH](./15-rrhh-bonos-y-deducciones.md)
- [18. Configuración de bonos en Admin](./18-admin-configuracion-bonos.md)
- [11. Prenómina en RRHH](./11-rrhh-prenomina.md)
- [12. Nómina en RRHH](./12-rrhh-nomina.md)

## Referencias relacionadas
- `MundoVs/Components/Pages/RRHH/BonosDistribuidos.razor`
- `MundoVs/Components/Pages/Admin/BonosNomina.razor`
- `MundoVs/Core/Entities/BonoDistribucionPeriodoRrhh.cs`
- `MundoVs/Core/Entities/NominaBonoRrhh.cs`

## Cierre del bloque de RRHH
Con este manual, la cobertura funcional principal de `RRHH` queda prácticamente completa para:
- turnos
- asistencias
- ausencias
- banco de horas
- esquemas de pago
- empleados
- prenómina
- nómina
- recibo
- destajo
- bonos
- bonos distribuidos

Todavía podrían existir manuales más finos o especializados, pero la guía operativa principal de `RRHH` ya queda cerrada.
