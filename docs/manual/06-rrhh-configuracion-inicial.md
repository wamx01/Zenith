# 06. Configuración inicial de `RRHH`

## Objetivo
Este manual explica el orden recomendado para dejar listo el módulo de `RRHH` dentro de la empresa antes de entrar a operación diaria de asistencia, prenómina o nómina.

## Alcance
Incluye:
- preparación funcional de `RRHH`
- orden base de catálogos y configuración
- relación entre estructura organizacional, empleados y operación

No incluye:
- operación diaria de asistencias
- procesamiento técnico del agente
- cálculo detallado de prenómina o nómina

## Base previa requerida
Antes de comenzar en `RRHH`, ya debe existir:
- empresa creada
- módulos habilitados
- usuarios y roles base
- departamentos
- posiciones
- configuración base de nómina

## Páginas principales relacionadas
- `RRHH > Empleados`
- `RRHH > Turnos`
- `RRHH > Marcaciones`
- `RRHH > Asistencias`
- `RRHH > Control de tiempo`
- `RRHH > Esquemas de pago`
- `RRHH > Vales de destajo`
- `RRHH > Prenóminas`
- `RRHH > Nóminas`

## Orden recomendado
La secuencia sugerida es:

`Turnos -> Esquemas de pago -> Empleados -> Marcaciones -> Asistencias -> Prenómina -> Nómina`

Si la empresa todavía no usa asistencia, puede iniciar con:

`Esquemas de pago -> Empleados -> Prenómina -> Nómina`

## Paso 1. Configurar turnos
Ruta sugerida:
- `RRHH > Turnos`

Definir:
- horarios base
- días laborables
- descansos
- reglas de jornada diaria

Resultado esperado:
- existe un catálogo de turnos reutilizable para asignar a empleados

---

## Paso 2. Configurar esquemas de pago
Ruta sugerida:
- `RRHH > Esquemas de pago`

Por qué se configura antes de empleados:
- porque el empleado no solo necesita existir, también necesita una regla clara de cálculo
- porque prenómina y nómina deben saber desde el inicio si se pagará sueldo fijo, destajo, bono o mezcla
- porque evita capturar empleados con pagos provisionales o ambiguos
- porque permite que el historial del empleado ya nazca con una forma de pago correcta

Definir según la empresa:
- sueldo fijo
- destajo por pieza
- destajo por operación
- bono por meta
- esquemas mixtos

Si aplica, configurar también:
- tarifas por proceso
- relación con posiciones

### Aclaración importante sobre varios pagos por destajo
Si la empresa maneja varios pagos por destajo, no siempre significa que debas crear un esquema distinto por cada caso.

#### Usar un solo esquema cuando:
- la lógica de pago es la misma
- el tipo de destajo es el mismo
- solo cambian las tarifas por proceso o por posición

Ejemplo:
- un esquema `Destajo por operación`
- dentro del mismo esquema se registran distintas tarifas para `Costura`, `Empaque` o `Corte`
- o distintas tarifas según la `Posición`

En ese caso, conviene mantener un solo esquema y administrar adentro sus tarifas.

#### Separar esquemas cuando:
- cambia la forma de cálculo
- cambia la regla de negocio del pago
- cambia el tipo de incentivo
- una parte del personal gana fijo y otra por destajo
- existen esquemas mixtos distintos entre grupos de empleados

Ejemplos donde sí conviene separar:
- `Destajo por pieza`
- `Destajo por operación`
- `Sueldo fijo + bono por meta`
- `Sueldo fijo + destajo`

La regla práctica es esta:

`Si cambia la tarifa, normalmente basta con otra tarifa dentro del esquema.`

`Si cambia la lógica de cálculo, conviene crear otro esquema.`

### Recomendación operativa
No crear un esquema nuevo por cada importe pequeño o por cada empleado, porque eso vuelve difícil el mantenimiento.

Conviene separar esquemas por familia de pago y luego capturar dentro de cada uno:
- tarifas por proceso
- tarifas por posición
- metas o reglas aplicables

Resultado esperado:
- existe una base clara para asignar forma de pago a cada empleado

---

## Paso 3. Dar de alta empleados
Ruta sugerida:
- `RRHH > Empleados`

Capturar:
- datos personales
- datos laborales
- departamento
- posición
- turno vigente o base
- periodicidad de pago
- esquema de pago vigente

Resultado esperado:
- los empleados quedan listos para asistencia, control de tiempo y nómina

---

## Paso 4. Preparar asistencia, si aplica
Si la empresa usa checadores o control horario, continuar con:
- checadores
- códigos de checador por empleado
- marcaciones
- asistencias
- control de tiempo

Resultado esperado:
- el sistema ya puede recibir marcaciones y convertirlas en asistencia interpretable

---

## Paso 5. Validar relación entre asistencia y nómina
Antes de operar prenómina, revisar:
- empleados con turno correcto
- empleados con esquema de pago correcto
- periodicidad correcta
- configuración base de nómina revisada
- reglas de horas extra claras

Resultado esperado:
- `RRHH` ya tiene base coherente para capturar incidencias y calcular periodos

---

## Paso 6. Iniciar operación de prenómina y nómina
Una vez validados turnos, empleados y esquemas, continuar con:
- `RRHH > Prenóminas`
- `RRHH > Nóminas`

Resultado esperado:
- la empresa puede entrar a operación formal de cálculo de pago

## Checklist rápido
Antes de cerrar la configuración inicial de `RRHH`, validar:
- turnos creados
- esquemas de pago creados
- empleados capturados
- puestos y departamentos correctos
- periodicidad correcta
- asistencia preparada si aplica
- base lista para prenómina

## Errores comunes a evitar
- capturar empleados sin posición o departamento correctos
- iniciar prenómina sin revisar configuración base de nómina
- asignar esquema de pago incorrecto al empleado
- operar asistencia sin turnos claros
- intentar calcular nómina sin validar periodicidad ni horas extra

## Relación con manuales anteriores
Este manual ocurre después de:
- [01. Configuración inicial de una empresa nueva](./01-configuracion-inicial-empresa.md)
- [02. Administración inicial dentro de la empresa](./02-admin-empresa-configuracion-base.md)
- [03. Administración de usuarios y roles de la empresa](./03-admin-usuarios-y-roles.md)
- [04. Estructura organizacional de la empresa](./04-admin-estructura-organizacional.md)
- [05. Configuración base de nómina](./05-admin-configuracion-nomina.md)

## Referencias relacionadas
- `../manuales/rrhh-asistencia-zkteco-puesta-en-marcha.md`
- `../modulos/detalle/06-rrhh.md`

## Siguiente manual sugerido
El siguiente paso lógico es:
- `07-rrhh-turnos.md`
- o `07-rrhh-esquemas-de-pago.md`
