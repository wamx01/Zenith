# 10. Marcaciones y asistencias en `RRHH`

## Objetivo
Este manual explica cómo pasar de las marcaciones crudas a una asistencia interpretable para que `Zenith` pueda soportar control de tiempo, incidencias, prenómina y nómina.

## Alcance
Incluye:
- relación entre checadores, empleados y marcaciones
- diferencia entre `Marcaciones` y `Asistencias`
- validaciones previas al reproceso
- orden recomendado de operación

No incluye:
- instalación técnica del agente
- configuración detallada de `ZkTecoApi`
- cálculo final de nómina

## Páginas principales
- `RRHH > Checadores`
- `RRHH > Marcaciones`
- `RRHH > Asistencias`
- `RRHH > Control de tiempo`
- `RRHH > Estado del agente` si aplica

## Concepto clave
### `Marcación`
Es el registro crudo que llega del checador o del mecanismo de captura.

Ejemplos:
- entrada
- salida a comida
- regreso de comida
- salida final

Por sí sola, la marcación todavía no dice si hubo:
- retardo
- salida anticipada
- horas extra
- asistencia correcta

### `Asistencia`
Es la interpretación del día laboral del empleado a partir de:
- turno
- marcaciones
- descansos
- reglas de jornada

Aquí es donde el sistema concluye cosas como:
- hora de entrada real
- hora de salida real
- retardo
- tiempo trabajado bruto
- tiempo trabajado neto
- tiempo extra
- observaciones

## Orden recomendado
La secuencia sugerida es:

`Checadores -> Empleados con código correcto -> Marcaciones -> Revisión -> Asistencias -> Reproceso si hace falta`

## Paso 1. Configurar checadores, si aplica
Ruta sugerida:
- `RRHH > Checadores`

Validar:
- nombre del equipo
- identificación del checador
- estado activo
- relación con la operación real

Resultado esperado:
- el sistema sabe de qué equipos recibirá eventos

---

## Paso 2. Validar empleados ligados al código correcto
Antes de revisar asistencia, confirmar que el empleado tenga correctamente su identificador de checador o relación operativa equivalente.

Esto es importante porque si la marcación entra sin poder ligarse al empleado, después habrá errores o reprocesos innecesarios.

Resultado esperado:
- las marcaciones pueden vincularse al empleado correcto

---

## Paso 3. Revisar marcaciones crudas
Ruta sugerida:
- `RRHH > Marcaciones`

Revisar:
- fecha y hora
- checador origen
- empleado ligado
- eventos sin religar
- posibles duplicados o inconsistencias

Resultado esperado:
- existe visibilidad del dato crudo antes de interpretarlo como asistencia

---

## Paso 4. Procesar o reprocesar asistencias
Ruta sugerida:
- `RRHH > Asistencias`

Aquí el sistema interpreta el día trabajado considerando:
- turno del empleado
- marcaciones disponibles
- descansos programados
- ventanas de jornada

Resultado esperado:
- cada día queda convertido en una asistencia revisable

---

## Paso 5. Validar resultados de asistencia
Después del procesamiento, revisar:
- entrada real
- salida real
- total de marcaciones
- retardo
- salida anticipada
- tiempo trabajado bruto
- tiempo trabajado neto
- tiempo extra
- observaciones

Resultado esperado:
- la asistencia ya sirve como base operativa para control y pago

---

## Paso 6. Usar `Control de tiempo` para análisis operativo
Ruta sugerida:
- `RRHH > Control de tiempo`

Esta vista ayuda a revisar el comportamiento consolidado de:
- checadores
- marcaciones
- asistencias

Resultado esperado:
- el responsable detecta anomalías antes de que impacten prenómina o nómina

## Cuándo reprocesar
Conviene reprocesar cuando:
- se corrigió el turno de un empleado
- se corrigió el código de checador
- llegaron marcaciones atrasadas
- se ajustó una regla de jornada relevante
- hubo fallas de ligue entre marcación y empleado

## Errores comunes a evitar
- querer usar `Asistencias` sin revisar antes `Marcaciones`
- operar checadores sin validar el vínculo con empleados
- reprocesar sin entender qué dato estaba mal
- asumir que toda marcación ya es asistencia válida
- no revisar turnos antes de interpretar retardos o tiempo extra

## Checklist rápido
Antes de cerrar esta etapa, validar:
- checadores configurados si aplica
- empleados ligados correctamente al código de checador
- marcaciones visibles y revisables
- asistencias procesadas
- anomalías detectadas antes de prenómina
- reproceso entendido como herramienta de corrección y no como rutina ciega

## Relación con manuales anteriores
Este manual ocurre después de:
- [06. Configuración inicial de RRHH](./06-rrhh-configuracion-inicial.md)
- [07. Configuración de turnos en RRHH](./07-rrhh-turnos.md)
- [09. Alta y administración de empleados en RRHH](./09-rrhh-empleados.md)

## Referencias relacionadas
- `../manuales/rrhh-asistencia-zkteco-puesta-en-marcha.md`
- `../modulos/detalle/06-rrhh.md`
- `MundoVs/Components/Pages/RRHH/Marcaciones.razor`
- `MundoVs/Components/Pages/RRHH/Asistencias.razor`
- `MundoVs/Components/Pages/RRHH/ControlTiempo.razor`

## Siguiente manual sugerido
El siguiente paso lógico es:
- `11-rrhh-prenomina.md`
- o `11-rrhh-zkteco-puesta-en-marcha.md` si quieres separar todavía más la parte técnica y operativa del agente
