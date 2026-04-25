# 05. Configuración base de nómina

## Objetivo
Este manual explica cómo debe dejar preparada el administrador de la empresa la configuración base de nómina antes de capturar prenóminas o nóminas en `Zenith`.

## Alcance
Incluye:
- parámetros base de nómina
- validación inicial de periodicidad
- preparación para `RRHH`

No incluye:
- alta de empleados
- esquemas de pago por empleado
- captura de incidencias
- cálculo operativo de nómina

## Ruta principal
- `Admin > Configuración nómina`

## Cuándo debe hacerse
Esta configuración debe quedar lista después de:
- estructura organizacional
- usuarios y roles

Y antes de:
- alta masiva de empleados
- prenóminas
- nóminas

## Objetivo práctico
Evitar que `RRHH` capture empleados o periodos sobre parámetros incompletos o incorrectos.

## Parámetros base a revisar
En esta pantalla se deben revisar al menos:
- días base
- horas base
- factor de horas extra
- factor general de festivo trabajado
- factor de descanso trabajado
- comportamiento por periodicidad

## Paso 1. Definir base de jornada
Revisar:
- cuántos días base aplican a la operación
- cuántas horas base forman la jornada o cálculo esperado

Resultado esperado:
- existe una base consistente para cálculos posteriores

---

## Paso 2. Definir factor de horas extra
Revisar:
- factor aplicable a horas extra
- criterio interno de pago o integración que usará la empresa

Resultado esperado:
- el sistema queda parametrizado para tratar tiempo extra con una regla base clara

---

## Paso 3. Definir factores de festivo y descanso trabajado
Revisar:
- factor general de `festivo trabajado`
- factor de `descanso trabajado`
- calendario de festivos por fecha

Regla operativa recomendada:
- el `festivo trabajado` puede tener un factor específico por fecha
- si la fecha no tiene factor o está en `0`, el sistema debe usar el factor general configurado en nómina
- el `descanso trabajado` usa su factor global de configuración

Resultado esperado:
- la empresa puede manejar una regla general de festivos y además excepciones por fecha sin perder consistencia en nómina

---

## Paso 4. Revisar periodicidades de nómina
Validar qué periodicidades usará la empresa, por ejemplo:
- semanal
- quincenal
- mensual

Si la empresa opera más de una periodicidad, revisar que la configuración quede clara para cada caso.

Resultado esperado:
- la empresa sabe sobre qué periodicidad va a trabajar cada grupo de empleados

---

## Paso 5. Validar congruencia con la operación real
Antes de cerrar esta configuración, revisar con `RRHH` o administración:
- horario típico de operación
- forma en que se manejarán horas extra
- forma en que se pagarán festivos y descansos trabajados
- periodicidad real de pago
- necesidad de destajo, bonos o esquemas mixtos

Resultado esperado:
- la configuración ya refleja la práctica real de la empresa y no un valor provisional improvisado

---

## Paso 6. Dejar lista la base para `RRHH`
Con esta pantalla configurada, ya se puede continuar con:
- esquemas de pago
- empleados
- turnos
- asistencias
- prenómina
- nómina

Resultado esperado:
- `RRHH` puede operar sobre una base administrativa ya definida

## Qué validar antes de continuar
Checklist rápido:
- días base revisados
- horas base revisadas
- factor de horas extra definido
- factor general de festivo trabajado definido
- factor de descanso trabajado definido
- calendario de festivos revisado
- periodicidad confirmada
- validación con administración o `RRHH` completada

## Errores comunes a evitar
- capturar nómina antes de revisar configuración base
- asumir una periodicidad sin validarla con la empresa
- dejar factores de horas extra con valores provisionales
- mezclar configuración administrativa con reglas individuales por empleado

## Relación con manuales anteriores
Este manual ocurre después de:
- [01. Configuración inicial de una empresa nueva](./01-configuracion-inicial-empresa.md)
- [02. Administración inicial dentro de la empresa](./02-admin-empresa-configuracion-base.md)
- [03. Administración de usuarios y roles de la empresa](./03-admin-usuarios-y-roles.md)
- [04. Estructura organizacional de la empresa](./04-admin-estructura-organizacional.md)

## Siguiente manual sugerido
El siguiente paso lógico es:
- `06-rrhh-configuracion-inicial.md`
- o `06-rrhh-esquemas-de-pago.md` si la empresa ya tiene definida su estructura de pago
