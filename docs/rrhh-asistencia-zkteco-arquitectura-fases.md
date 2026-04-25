# Arquitectura propuesta por fases — `RRHH` asistencia con `Zenith` + `ZkTecoApi`

## Objetivo
Definir una arquitectura robusta y segura para operar checadores `ZKTeco` cuando `Zenith` vive en un `VPS` o nube, manteniendo el acceso a los dispositivos dentro de la red interna del cliente y centralizando la persistencia, validación y procesamiento en `Zenith`.

---

## Respuesta directa a las 2 preguntas clave

### 1. Si `ZkTecoApi` no estará conectado a la base de datos, ¿cómo guarda `Zenith` los datos del checador?
`Zenith` los guarda en su propia base de datos al recibirlos por API.

El flujo correcto es:

`Checador ZKTeco -> ZkTecoApi interno -> API Zenith -> BD Zenith`

Eso significa:
- `ZkTecoApi` **no necesita** acceso SQL a la base del `VPS`.
- `ZkTecoApi` solo necesita:
  - acceso a la red local donde viven los checadores,
  - salida `HTTPS` hacia `Zenith`.
- `Zenith` expone endpoints para:
  - entregar configuración al agente interno,
  - recibir marcaciones,
  - recibir heartbeat/estado,
  - registrar errores y salud del agente.

En otras palabras: la web de `Zenith` guarda la configuración del checador en su BD, y el agente interno la consume por API y luego le regresa las marcaciones también por API.

### 2. La lógica actual de turnos en `ZkTecoApi` ¿se debe mover después a `Zenith`?
Sí.

La lógica de negocio de turnos, asistencia, retardos, salida anticipada, extras y reglas de clasificación debe quedar en `Zenith`, no en el agente interno.

`ZkTecoApi` debe quedarse con una responsabilidad simple:
- conectarse al dispositivo,
- leer eventos,
- normalizarlos,
- enviarlos de forma segura.

`Zenith` debe encargarse de:
- asociar marcaciones a empleados,
- resolver turno vigente,
- convertir `rrhh_marcacion` en `rrhh_asistencia`,
- recalcular cuando cambie turno, empleado o regla.

Esto permite:
- tener una sola fuente de verdad,
- recalcular histórico,
- evitar duplicar lógica en cada sitio,
- soportar múltiples sucursales o clientes.

---

## Propuesta de arquitectura objetivo

## Componentes

### A. `Zenith` en nube / `VPS`
Responsabilidad central:
- `Blazor` web de administración,
- API central,
- base de datos principal,
- catálogos y configuración,
- procesamiento de asistencia,
- monitoreo de agentes.

### B. `ZkTecoApi` dentro de la red interna del cliente
Responsabilidad local:
- acceder a los checadores por IP privada,
- consultar usuarios y eventos del reloj,
- empaquetar lotes de marcaciones,
- enviar los lotes a `Zenith`,
- almacenar cola local temporal si no hay internet,
- informar estado y errores.

### C. Checadores `ZKTeco`
Dispositivos físicos en la LAN del cliente.

---

## Principio de seguridad
No exponer:
- puertos del checador a internet,
- la base de datos del `VPS` a la red interna del cliente,
- conexiones entrantes desde `Zenith` al sitio del cliente como requisito base.

Sí permitir:
- solo salida `HTTPS` desde `ZkTecoApi` hacia `Zenith`.

Esto reduce superficie de ataque y simplifica operación.

---

## Modelo de responsabilidades

## Lo que queda en `ZkTecoApi`
- leer configuración remota de checadores,
- conectarse al SDK `ZKTeco`,
- leer usuarios si se requiere enriquecer nombre local,
- leer marcaciones crudas,
- construir `MarcacionRawDto`,
- enviar lotes a `Zenith`,
- mantener checkpoint local por checador,
- reintentar si falla internet,
- reportar heartbeat.

## Lo que queda en `Zenith`
- administrar `RrhhChecador`,
- administrar agentes por empresa,
- autenticar agentes,
- validar lotes,
- deduplicar por evento,
- guardar `RrhhMarcacion`,
- mapear `CodigoChecador` a `Empleado`,
- resolver turno vigente,
- generar `RrhhAsistencia`,
- registrar `RrhhLogChecador`,
- mostrar monitoreo y errores en la web.

---

## Entidades actuales que ya ayudan

## Ya existentes en `Zenith`
Alineadas con esta propuesta:
- `MundoVs/Core/Entities/RrhhChecador.cs`
- `MundoVs/Core/Entities/RrhhMarcacion.cs`
- `MundoVs/Core/Entities/RrhhAsistencia.cs`
- `MundoVs/Core/Entities/RrhhLogChecador.cs`

## Contratos ya útiles
- `Zenith.Contracts/Asistencia/ChecadorConfigDto.cs`
- `Zenith.Contracts/Asistencia/MarcacionRawDto.cs`
- `Zenith.Contracts/Asistencia/MarcacionSyncBatchDto.cs`
- `Zenith.Contracts/Asistencia/SyncResultDto.cs`

Eso significa que la base conceptual ya está bien encaminada.

## Contexto real de datos que sí existe hoy
El contexto de datos válido para esta implementación no es `HiramDbContext`.

El contexto real del sistema actual es:
- `MundoVs/Infrastructure/Data/CrmDbContext.cs`

Y ya contiene `DbSet` para:
- `Empleados`
- `TurnosBase`
- `TurnosBaseDetalle`
- `RrhhChecadores`
- `RrhhMarcaciones`
- `RrhhAsistencias`
- `RrhhLogsChecador`

Además, `CrmDbContext` ya mapea estas tablas:
- `rrhh_checador`
- `rrhh_marcacion`
- `rrhh_asistencia`
- `rrhh_logchecador`

También ya existe soporte funcional importante en entidades actuales:
- `Empleado.CodigoChecador`
- `Empleado.TurnoBaseId`
- `TurnoBase`
- `TurnoBaseDetalle`

Conclusión técnica:
- la persistencia y el procesamiento de asistencia deben montarse sobre `CrmDbContext`,
- `Zenith.Contracts` solo aporta contratos compartidos,
- `HiramDbContext` no debe formar parte de la solución objetivo.

---

## Ajuste requerido del proyecto legado `ZkTecoApi`
El `ZkTecoApi` heredado fue hecho para otro proyecto y hoy trae acoplamientos que no existen en `MundoVs`.

## Dependencias legadas que no aplican aquí
No se deben reutilizar tal cual estas piezas del proyecto legado:
- `HiramDbContext`
- `AttendanceClock`
- `Attendance`
- `Employee` legado
- `Shift`
- `ShiftRotation`
- persistencia directa a `Attendances`

## Por qué no aplican
Porque esas clases:
- pertenecen a otro modelo de datos,
- usan claves y entidades distintas,
- resuelven negocio local del sistema legado,
- guardan asistencia final en tablas que no existen en `MundoVs`.

## Qué sí se rescata del legado
Solo debe rescatarse la capa de lectura del dispositivo:
- `CZKEM`
- `Connect_Net`
- `ReadAllUserID`
- `ReadGeneralLogData`
- `SSR_GetGeneralLogData`
- `ClearGLog` como operación administrativa excepcional

## Qué debe reescribirse para `MundoVs`

### 1. Configuración de checadores
En legado:
- se leía de `AttendanceClocks` por `HiramDbContext`.

En objetivo:
- debe salir de `RrhhChecador` en `CrmDbContext`, o
- más correctamente para el agente interno, por API desde `Zenith` usando `ChecadorConfigDto`.

Equivalencia:
- `AttendanceClock` legado -> `RrhhChecador` + `ChecadorConfigDto`

### 2. Empleado asociado a la marca
En legado:
- se resolvía con `Employee.payrolno.ToString() == UserID`.

En objetivo:
- debe resolverse con `Empleado.CodigoChecador == CodigoChecador`.

Equivalencia:
- `Employee.payrolno` legado -> `Empleado.CodigoChecador`

### 3. Turno base del empleado
En legado:
- se resolvía con `Employee.Shift` y `ShiftRotation`.

En objetivo:
- se debe resolver con `Empleado.TurnoBaseId`,
- `TurnoBase`,
- `TurnoBaseDetalle`,
- y posteriormente `rrhh_empleado_turno` para vigencias históricas.

Equivalencia:
- `Shift` legado -> `TurnoBase`
- `Shiftdays` legado -> `TurnoBaseDetalle`
- rotación legado -> futura lógica de vigencias o rotación en `Zenith`

### 3.1 Jornada configurable y descansos
En objetivo actual:

- `TurnoBaseDetalle` ya no solo define entrada y salida,
- también puede definir hasta `2` descansos por día,
- cada descanso puede ser pagado o no pagado,
- el cálculo histórico de asistencia debe interpretar esos descansos a partir de las marcaciones intermedias.

Esto permite soportar jornadas como:

- `08:00 - 17:00` con una comida no pagada,
- `08:00 - 17:00` con dos descansos,
- jornadas donde el descanso se toma fuera de la hora ideal pero con duración válida.

### 4. Guardado de marcas
En legado:
- se insertaba directo en `Attendance`.

En objetivo:
- primero se inserta en `RrhhMarcacion`,
- luego se procesa a `RrhhAsistencia`.

Equivalencia:
- `Attendance` legado -> `RrhhMarcacion` + `RrhhAsistencia`

### 5. Logs de sincronización
En legado:
- no hay una bitácora central equivalente.

En objetivo:
- debe usarse `RrhhLogChecador`.

## Adaptación por clases del legado

### `ChekadoresController`
Se puede conservar la parte de lectura del SDK, pero no debe depender de modelos de `Hiram`.

Debe evolucionar para:
- leer un checador por IP/puerto y devolver datos crudos,
- o mejor aún, mover la lectura a un servicio interno reutilizable.

No debe:
- decidir empresa,
- guardar asistencia final,
- depender de `Attendance` o `Employee` del legado.

### `ChecadorBackgroundService`
La parte actual no es reutilizable tal cual.

Debe cambiar de:
- leer checadores desde `HiramDbContext`,
- pedir a su propia API por HTTP,
- guardar en `Attendances`.

A:
- obtener configuración desde `Zenith`,
- leer el reloj localmente,
- construir `MarcacionSyncBatchDto`,
- enviar a `Zenith` por `HttpClient`,
- manejar cola local y reintentos.

### `Program.cs` del legado
Debe dejar de registrar:
- `AddDbContextFactory<HiramDbContext>`

Y debe registrar en su lugar:
- opciones del agente,
- cliente HTTP a `Zenith`,
- servicio de lectura ZKTeco,
- `BackgroundService` de sync,
- almacenamiento local liviano si aplica.

---

---

## Cómo guarda `Zenith` los checadores sin conexión SQL desde `ZkTecoApi`

## Flujo de configuración
1. Un usuario administra checadores en la web de `Zenith`.
2. `Zenith` guarda esos datos en `RrhhChecador`.
3. `ZkTecoApi` consulta a `Zenith` un endpoint de configuración.
4. `Zenith` devuelve los checadores activos para esa empresa/agente.
5. `ZkTecoApi` usa esa configuración para conectarse localmente a cada reloj.

## Flujo de marcaciones
1. `ZkTecoApi` lee el reloj.
2. Normaliza a `MarcacionRawDto`.
3. Envía lote a `Zenith`.
4. `Zenith` valida y deduplica.
5. `Zenith` inserta `RrhhMarcacion`.
6. `Zenith` deja lista esa marcación para procesamiento de asistencia.

La clave es que la configuración vive en `Zenith`; el agente solo la consume.

---

## Endpoints recomendados en `Zenith`

## 1. Configuración del agente
`GET /api/rrhh/agentes/configuracion`

Devuelve:
- empresa,
- lista de checadores activos,
- intervalo de sincronización,
- zona horaria,
- flags de lectura,
- parámetros operativos del agente.

Sugerencia de respuesta:
- `EmpresaId`
- `NombreAgente`
- `IntervaloSegundos`
- `Checadores[]`
- `ModoDiagnostico`
- `PermitirLecturaUsuarios`

## 2. Recepción de marcaciones
`POST /api/rrhh/marcaciones/sync`

Recibe `MarcacionSyncBatchDto`.

Debe:
- autenticar agente,
- validar empresa/checador,
- deduplicar,
- guardar marcaciones,
- regresar `SyncResultDto`.

## 3. Heartbeat del agente
`POST /api/rrhh/agentes/heartbeat`

Recibe:
- `EmpresaId`
- `NombreAgente`
- `Hostname`
- `Version`
- `UltimaEjecucionUtc`
- `MarcacionesLeidas`
- `MarcacionesEnviadas`
- `ErroresRecientes`

## 4. Logs opcionales del agente
`POST /api/rrhh/agentes/logs`

Para subir errores relevantes cuando no sea suficiente el heartbeat.

---

## Persistencia objetivo en `Zenith`

## Reusar entidades existentes

### `RrhhChecador`
Debe ser el maestro de configuración central del checador.

Campos ya útiles:
- `EmpresaId`
- `Nombre`
- `NumeroSerie`
- `Ip`
- `Puerto`
- `NumeroMaquina`
- `Ubicacion`
- `ZonaHoraria`
- `UltimaSincronizacionUtc`
- `UltimoEventoLeido`

### `RrhhMarcacion`
Debe almacenar el evento crudo normalizado proveniente del reloj.

Campos ya útiles:
- `EmpresaId`
- `ChecadorId`
- `EmpleadoId`
- `CodigoChecador`
- `FechaHoraMarcacionUtc`
- `TipoMarcacionRaw`
- `Origen`
- `EventoIdExterno`
- `HashUnico`
- `Procesada`
- `ResultadoProcesamiento`
- `PayloadRaw`

### `RrhhAsistencia`
Debe representar la asistencia interpretada y procesada.

Además de entrada y salida, ya debe contemplar métricas como:

- total de marcaciones del día,
- jornada programada,
- jornada neta programada,
- minutos trabajados brutos,
- minutos trabajados netos,
- minutos de descanso programado,
- minutos de descanso tomados,
- minutos de descanso pagados,
- minutos de descanso no pagados,
- resumen de descansos para revisión operativa.

### `RrhhLogChecador`
Debe ser la bitácora funcional y técnica.

---

## Nota de alcance para esta propuesta
En este documento ya no se prioriza el detalle de tablas nuevas. El foco principal es el plan de implementación por fases y el orden exacto de trabajo.

Solo se mantiene la referencia conceptual a `rrhh_empleado_turno` porque afecta directamente la lógica de asistencia.

## `rrhh_empleado_turno`
Se considera una pieza futura para resolver turnos históricos por vigencia.

Su objetivo es:
- saber qué turno tenía el empleado en una fecha dada,
- evitar depender solo de `Empleado.TurnoBaseId`,
- permitir reprocesamiento histórico correcto.

## Cómo funciona `rrhh_empleado_turno`
No guarda solo “el turno actual”, sino el historial de vigencias del empleado.

Ejemplo:

- fila 1: empleado `A`, turno `Matutino`, `VigenteDesde=2026-01-01`, `VigenteHasta=2026-03-31`
- fila 2: empleado `A`, turno `Nocturno`, `VigenteDesde=2026-04-01`, `VigenteHasta=null`

Con eso:
- una marcación del `2026-02-10` usa `Matutino`,
- una marcación del `2026-04-15` usa `Nocturno`.

## Cómo resolver el turno del día actual
Para una fecha dada, por ejemplo hoy en fecha local de la empresa o del checador:

1. se toma `EmpleadoId`,
2. se toma la fecha local de la marcación,
3. se busca en `rrhh_empleado_turno` el registro donde:
   - `EmpleadoId` coincida,
   - `VigenteDesde <= fecha`,
   - `VigenteHasta` sea `null` o `VigenteHasta >= fecha`,
4. se ordena por `VigenteDesde desc`,
5. se toma el primero.

En forma funcional, la regla es:

`turno vigente = el último turno asignado cuya vigencia cubre la fecha de la marcación`

## Cómo alimentar `rrhh_empleado_turno`

### Alta inicial
Al arrancar el módulo:
- si el empleado ya tiene `TurnoBaseId`,
- se genera una fila inicial en `rrhh_empleado_turno` con:
  - `EmpleadoId`
  - `TurnoBaseId`
  - `VigenteDesde = FechaAltaEmpleado` o fecha de arranque definida
  - `VigenteHasta = null`

### Cambio manual de turno
Cuando `RRHH` cambie el turno de un empleado:
- se cierra el registro vigente anterior con `VigenteHasta = día anterior`,
- se crea un nuevo registro con el nuevo `TurnoBaseId` y `VigenteDesde = fecha efectiva`.

### Asignación masiva
Si una cuadrilla cambia de turno:
- el sistema debe permitir aplicar el cambio a varios empleados,
- cerrando la vigencia anterior y creando la nueva.

### Regla de integridad importante
No debe haber dos filas activas solapadas para el mismo empleado en la misma fecha.

Validaciones mínimas:
- un solo turno vigente por empleado para una fecha dada,
- no permitir traslapes de vigencia,
- si se corrige historial, reprocesar asistencias afectadas.

## Qué pasa si hoy no existe fila en `rrhh_empleado_turno`
Opciones recomendadas:

1. usar `Empleado.TurnoBaseId` como respaldo temporal durante la transición,
2. registrar `RrhhAsistencia` con estatus `TurnoNoAsignado` si no se pudo resolver,
3. dejar evidencia en `RrhhMarcacion.ResultadoProcesamiento` o `RrhhLogChecador`.

La meta final debe ser depender de `rrhh_empleado_turno`, no solo del campo directo en `Empleado`.

## Relación con rotaciones
Si luego manejas rotaciones complejas:
- `rrhh_empleado_turno` sigue siendo útil,
- porque guarda la asignación base vigente,
- y el procesador de asistencia puede consultar además reglas de rotación para esa fecha.

En la fase urgente, basta con resolver el turno vigente por vigencia.

---

## Cómo sabe `Zenith` de qué empresa es un checador
La empresa no se debe resolver por IP. Se debe resolver por configuración central y credenciales del agente.

## Regla recomendada

### Nivel 1. El agente pertenece a una empresa
En la primera versión robusta:
- cada instalación de `ZkTecoApi` se registra con una empresa,
- recibe una `ApiKey` propia,
- al autenticarse, `Zenith` ya sabe qué `EmpresaId` le corresponde.

### Nivel 2. Cada checador está registrado en `RrhhChecador`
Cada fila de `RrhhChecador` pertenece a una empresa mediante `EmpresaId`.

Por tanto, un checador se identifica por:
- `ChecadorId`
- `EmpresaId`
- `Ip`
- `Puerto`
- `NumeroMaquina`

No por IP solamente.

## Flujo correcto de resolución empresa/checador
1. `ZkTecoApi` se autentica con `ApiKey`.
2. `Zenith` identifica la empresa del agente.
3. `Zenith` responde la configuración con todos los checadores activos de esa empresa.
4. Cada checador viaja con su `ChecadorId`.
5. Al enviar marcaciones, el lote incluye `EmpresaId` y `ChecadorId`.
6. `Zenith` valida que ese `ChecadorId` pertenece a esa `EmpresaId`.

Eso resuelve correctamente el escenario donde una empresa tiene varios checadores.

## Escenario con varios checadores por empresa
Sí está contemplado y debe ser el comportamiento normal.

Ejemplo:
- empresa `X`
- checador `Recepción`
- checador `Producción`
- checador `Almacén`

Los tres viven en `RrhhChecador` con el mismo `EmpresaId` y distinto:
- `Id`
- `Nombre`
- `Ip`
- `Ubicacion`
- `NumeroSerie` si aplica.

El endpoint de configuración debe devolver una colección `Checadores[]`, no un solo checador.

---

## Cómo se configura la IP desde `Zenith`
La IP se captura en la pantalla de checadores dentro de `Zenith`.

## Flujo recomendado
1. En `Zenith`, el usuario entra a `RRHH/Checadores.razor`.
2. Crea un checador ligado a una empresa.
3. Captura:
   - `EmpresaId`
   - `Nombre`
   - `Ip`
   - `Puerto`
   - `NumeroMaquina`
   - `Ubicacion`
   - `ZonaHoraria`
   - `NumeroSerie` opcional
   - `Activo`
4. `Zenith` guarda eso en `RrhhChecador`.
5. `ZkTecoApi` consulta configuración.
6. `Zenith` devuelve todos los checadores activos de esa empresa.
7. `ZkTecoApi` usa esa IP para conectarse localmente.

## Qué no se debe hacer
- no capturar la IP en archivos manuales aislados si ya existe `Zenith`,
- no depender de que `ZkTecoApi` “adivine” de qué empresa es una IP,
- no usar la IP como llave de negocio principal.

## Cuándo sí podrías configurar algo también del lado interno
Solo para parámetros técnicos del agente, por ejemplo:
- URL base de `Zenith`,
- `ApiKey`,
- proxy,
- intervalo mínimo de polling,
- carpeta de cola local.

La configuración de negocio de checadores debe vivir en `Zenith`.

---

## Cuándo usar `borrar checadas`
`ClearGLog` debe tratarse como operación administrativa excepcional, no como parte normal del flujo.

## Cuándo sí usarlo
- pruebas de laboratorio con un checador de desarrollo,
- limpieza controlada después de confirmar que las marcaciones ya fueron sincronizadas y auditadas,
- reinicio deliberado de un reloj que tiene datos basura o duplicados por pruebas.

## Cuándo no usarlo
- al final de cada sync,
- como mecanismo de deduplicación,
- si no existe certeza de que `Zenith` ya recibió y guardó todo,
- en operación normal diaria.

## Recomendación fuerte
En producción, ese endpoint debería:
- quedar deshabilitado por defecto, o
- requerir permiso administrativo especial, o
- pedir confirmación con bitácora y doble validación.

La estrategia correcta no es borrar el reloj en cada ciclo, sino:
- leer,
- deduplicar,
- guardar,
- checkpoint,
- reprocesar si hace falta.

---

## Diseño de sincronización robusta

## Regla 1. La sincronización debe ser idempotente
Si el mismo evento llega dos o más veces, `Zenith` no debe duplicarlo.

Claves recomendadas:
- `EventoIdExterno` si es confiable,
- si no, `HashUnico` generado con:
  - `EmpresaId`
  - `ChecadorId`
  - `CodigoChecador`
  - `FechaHoraMarcacionUtc`
  - `TipoMarcacionRaw`

## Regla 2. El agente no debe depender del estado en memoria
Debe guardar un checkpoint local ligero o cola local.

Opciones:
- `SQLite` local,
- archivo `json` de respaldo,
- ambos si se quiere más robustez.

## Regla 3. Los turnos no se resuelven en el agente
El agente envía dato crudo; `Zenith` interpreta.

## Regla 4. El procesamiento de asistencia debe ser reprocesable
Si cambia:
- el turno,
- el empleado asociado,
- la zona horaria,
- la política de clasificación,

se debe poder recalcular `RrhhAsistencia` a partir de `RrhhMarcacion`.

## Regla 5. Soportar operación offline temporal
Si `Zenith` no está disponible:
- el agente sigue leyendo,
- encola localmente,
- reintenta luego,
- evita pérdida de datos.

---

## Lógica de turnos: dónde debe vivir

## En `ZkTecoApi` hoy
La lógica actual del proyecto legado busca:
- empleado,
- turno,
- rotación,
- asistencia,
- guardado final.

## En la arquitectura propuesta
Esa lógica debe separarse en dos capas:

### Capa 1. Ingesta
Guardar marcación cruda.

### Capa 2. Procesamiento
Transformar marcación cruda en asistencia consolidada.

## Servicio sugerido en `Zenith`
Crear algo como:
- `RrhhAsistenciaProcessor`
- o `RrhhMarcacionProcessor`

Responsabilidades:
- resolver empleado por `CodigoChecador`,
- convertir UTC a zona horaria del checador o empresa,
- agrupar por empleado y fecha local,
- tomar turno vigente del día,
- clasificar entrada/salida,
- calcular retardo,
- calcular salida anticipada,
- calcular tiempo extra,
- generar o actualizar `RrhhAsistencia`.

## Conclusión sobre turnos
Sí: la lógica de turnos se programa después en `Zenith`, y esa es la ubicación correcta.

---

## Plan detallado de ejecución por fases

Esta sección sustituye el enfoque anterior de alto nivel por un plan operativo, secuencial y ejecutable. La intención es tener claro qué se hará primero, qué se entrega en cada fase y qué se deja intencionalmente para la siguiente.

## Tablero de avance actual del repositorio
Estado estimado con base en lo que ya existe hoy en `MundoVs` y `Zenith.Workers.Asistencia`:

- `Fase 0`: `Completada` a nivel de definición.
- `Fase 1`: `Parcial`.
- `Fase 2`: `Parcial avanzada`.
- `Fase 3`: `Parcial avanzada`.
- `Fase 4`: `Completada`.
- `Fase 5`: `Parcial`.
- `Fase 6`: `Parcial`.
- `Fase 7`: `Completada`.
- `Fase 8`: `Completada`.

## Checklist maestro de implementación
Usar este bloque como tablero manual de avance. La idea es ir marcando tareas reales terminadas y dejar visible qué sigue.

### Fase 0 — Definición y alcance
- [x] Confirmar arquitectura `push`.
- [x] Confirmar que `CrmDbContext` es el contexto válido.
- [x] Confirmar que `HiramDbContext` queda fuera.
- [x] Separar ingesta de procesamiento de asistencia.
- [x] Acordar que la primera entrega prioriza ingestión confiable.

### Fase 1 — Configuración central de checadores en `Zenith`
- [x] Existe `RrhhChecador`.
- [x] Existe mapeo en `CrmDbContext`.
- [x] Existe `RRHH/Checadores.razor`.
- [x] Filtrar la carga de checadores por `EmpresaId`.
- [x] Filtrar validaciones de duplicado por `EmpresaId`.
- [ ] Revisar permisos finos de visualización y edición.
- [x] Mostrar `UltimoEventoLeido` en la UI si se requiere.
- [ ] Confirmar navegación visible en menú si aplica.

### Fase 2 — API central para agentes
- [x] Existe `POST /api/rrhh/marcaciones/sync`.
- [x] Existe validación básica por `X-Zenith-Worker-Key`.
- [x] Implementar `GET /api/rrhh/agentes/configuracion`.
- [x] Implementar `POST /api/rrhh/agentes/heartbeat`.
- [x] Implementar `POST /api/rrhh/agentes/logs` si se decide usar.
- [x] Definir DTO de configuración de agente.
- [x] Definir DTO de heartbeat del agente.
- [x] Endurecer autenticación por agente/empresa.

### Fase 3 — Ingesta robusta de marcaciones
- [x] Se valida `EmpresaId` y `ChecadorId`.
- [x] Se calcula `HashUnico`.
- [x] Se evita duplicado por hash.
- [x] Se inserta `RrhhMarcacion`.
- [x] Se intenta resolver `EmpleadoId` por `CodigoChecador`.
- [x] Se actualiza `UltimaSincronizacionUtc` del checador.
- [x] Se registra log en `RrhhLogChecador`.
- [x] Extraer la lógica de ingesta a un servicio dedicado.
- [x] Revisar si el hash cubre todos los escenarios reales.
- [x] Endurecer manejo de errores de lote.

### Fase 4 — Adaptación del agente

#### Ruta recomendada: `Zenith.Workers.Asistencia`
- [x] Existe `ZkTecoMarcacionReader`.
- [x] Existe `AsistenciaSyncService`.
- [x] Existe `HttpMarcacionSyncClient`.
- [x] Existe `StaticChecadorConfigProvider`.
- [x] Sustituir `StaticChecadorConfigProvider` por un provider remoto.
- [x] Consumir configuración desde `Zenith`.
- [x] Consumir endpoint de heartbeat.
- [x] Ajustar opciones del worker para configuración remota.
- [x] Confirmar si esta será la base oficial del agente.

#### Ruta alternativa: adaptar `..\ZkTecoApi_Zenith`
- [ ] Quitar `HiramDbContext` del flujo.
- [ ] Quitar lectura de `AttendanceClock`.
- [ ] Quitar guardado en `Attendance`.
- [ ] Quitar resolución de turnos con `Shift` legado.
- [ ] Mantener solo lectura del SDK ZKTeco.
- [ ] Agregar cliente HTTP hacia `Zenith`.
- [ ] Consumir configuración remota desde `Zenith`.

### Fase 5 — Tolerancia a fallos del agente
- [x] Definir almacenamiento local temporal.
- [x] Separar lectura del reloj del envío HTTP.
- [x] Guardar lotes pendientes localmente.
- [x] Implementar reintentos con backoff.
- [x] Guardar checkpoint local por checador.
- [x] Permitir reenvío de pendientes después de caída.
- [x] Registrar errores técnicos locales.

### Fase 6 — Procesamiento de asistencia en `Zenith`
- [x] Crear `RrhhAsistenciaProcessor` o equivalente.
- [x] Convertir UTC a fecha local por zona horaria.
- [x] Resolver empleado final por `CodigoChecador`.
- [x] Resolver turno por `Empleado.TurnoBaseId` en etapa inicial.
- [x] Incorporar `rrhh_empleado_turno` en etapa posterior.
- [x] Modelar cambios de turno por vigencia para reproceso histórico correcto.
- [x] Agrupar marcas por empleado y fecha.
- [x] Calcular entrada real y salida real.
- [x] Calcular retardo.
- [x] Calcular salida anticipada.
- [x] Calcular tiempo extra.
- [x] Marcar incidencias no clasificadas.
- [x] Actualizar `RrhhMarcacion.Procesada`.

### Fase 7 — UI operativa RRHH
- [x] Existe `RRHH/Checadores.razor`.
- [x] Completar filtros compactos en `Checadores.razor`.
- [x] Crear `RRHH/Marcaciones.razor`.
- [x] Crear `RRHH/Asistencias.razor`.
- [x] Crear vista de estado del agente.
- [x] Mostrar filtros por checador, empleado, fecha y estado.
- [x] Mostrar asistencias calculadas y pendientes de revisión.

### Fase 8 — Endurecimiento final
- [x] Definir política final de reintentos.
- [x] Definir política de reprocesamiento histórico.
- [x] Restringir `borrar checadas` a uso controlado.
- [x] Agregar monitoreo básico.
- [x] Agregar alertas por agente sin heartbeat.
- [x] Agregar pruebas de integración de sync.
- [x] Agregar pruebas de deduplicación.
- [x] Agregar pruebas de procesamiento de asistencia.

## Referencia rápida de archivos ya existentes

### Lado `Zenith` / `MundoVs`
- `MundoVs/Program.cs`
- `MundoVs/Infrastructure/Data/CrmDbContext.cs`
- `MundoVs/Core/Entities/RrhhChecador.cs`
- `MundoVs/Core/Entities/RrhhMarcacion.cs`
- `MundoVs/Core/Entities/RrhhAsistencia.cs`
- `MundoVs/Core/Entities/RrhhLogChecador.cs`
- `MundoVs/Core/Entities/Empleado.cs`
- `MundoVs/Core/Entities/TurnoBase.cs`
- `MundoVs/Components/Pages/RRHH/Checadores.razor`

### Lado agente actual
- `Zenith.Workers.Asistencia/Program.cs`
- `Zenith.Workers.Asistencia/Worker.cs`
- `Zenith.Workers.Asistencia/Services/AsistenciaSyncService.cs`
- `Zenith.Workers.Asistencia/Readers/ZkTecoMarcacionReader.cs`
- `Zenith.Workers.Asistencia/Clients/HttpMarcacionSyncClient.cs`
- `Zenith.Workers.Asistencia/Providers/StaticChecadorConfigProvider.cs`
- `Zenith.Workers.Asistencia/Options/AsistenciaWorkerOptions.cs`

### Lado legado a adaptar o tomar como referencia
- `..\ZkTecoApi_Zenith/Program.cs`
- `..\ZkTecoApi_Zenith/ChecadorBackgroundService.cs`
- `..\ZkTecoApi_Zenith/Controllers/ChecadoresController.cs`

# Fase 0 — Aterrizaje y congelación del alcance
## Objetivo
Definir exactamente qué entra en la primera entrega funcional y qué se difiere.

## Estado actual
`Completada` a nivel de definición técnica.

## Archivos de referencia
- `docs/rrhh-asistencia-zkteco-arquitectura-fases.md`
- `MundoVs/Infrastructure/Data/CrmDbContext.cs`
- `Zenith.Workers.Asistencia/Program.cs`
- `..\ZkTecoApi_Zenith/ChecadorBackgroundService.cs`

## Resultado esperado
Queda decidido que la primera versión hará ingestión confiable de marcaciones y no intentará resolver toda la lógica avanzada de asistencia desde el día uno.

## Paso a paso
1. Confirmar que el flujo oficial será `push`.
2. Confirmar que `ZkTecoApi` vivirá dentro de la red interna.
3. Confirmar que `Zenith` será el receptor central en `VPS`.
4. Confirmar que `HiramDbContext` queda fuera del diseño objetivo.
5. Confirmar que la primera meta funcional será:
   - configurar checadores,
   - recibir marcaciones,
   - deduplicarlas,
   - guardarlas en `Zenith`.
6. Dejar explícito que el cálculo final de asistencia por turno queda para una fase posterior.

## Checklist de cierre
- [ ] Arquitectura `push` confirmada.
- [ ] `CrmDbContext` confirmado como único contexto válido.
- [ ] Separación entre ingesta y procesamiento confirmada.
- [ ] Alcance de la primera entrega congelado.

---

# Fase 1 — Preparar `Zenith` como fuente central de configuración
## Objetivo
Dejar a `Zenith` como el lugar donde se capturan y administran los checadores por empresa.

## Estado actual
`Completada`.

Ya existe:
- entidad `RrhhChecador`,
- mapeo en `CrmDbContext`,
- pantalla `Checadores.razor`.

Falta ajustar:
- filtro explícito por empresa en cargas y duplicados,
- revisión de permisos y validaciones finas,
- mostrar más estado operativo si se requiere.

## Archivos principales a tocar
- `MundoVs/Core/Entities/RrhhChecador.cs`
- `MundoVs/Infrastructure/Data/CrmDbContext.cs`
- `MundoVs/Components/Pages/RRHH/Checadores.razor`
- `MundoVs/Components/Layout/NavMenu.razor` si hiciera falta navegación

## Resultado esperado
La web ya puede registrar varios checadores por empresa y el sistema los conserva listos para ser consumidos por el agente interno.

## Paso a paso
1. Revisar el estado actual de `RrhhChecador` y su uso en `CrmDbContext`.
2. Confirmar qué campos ya existen y cuáles faltan para operación real.
3. Ajustar la entidad `RrhhChecador` solo si hace falta un campo operativo indispensable.
4. Crear o completar la pantalla `MundoVs/Components/Pages/RRHH/Checadores.razor`.
5. Permitir alta de múltiples checadores por empresa.
6. Capturar por checador:
   - nombre,
   - IP,
   - puerto,
   - número de máquina,
   - ubicación,
   - zona horaria,
   - activo.
7. Mostrar en la pantalla el estado operativo mínimo:
   - última sincronización,
   - último evento leído,
   - activo/inactivo.
8. Validar que la consulta de checadores quede filtrada por empresa.

## Criterio de avance visible
Esta fase se considera terminada cuando:
- el usuario puede dar de alta varios checadores de su empresa,
- no ve checadores de otra empresa,
- puede editar IP, puerto, máquina, zona horaria y estado.

## Entregables
- `RrhhChecador` listo para operación.
- UI de `Checadores` usable.
- Captura multi-checador por empresa funcionando.

## Checklist de cierre
- [ ] Se pueden crear varios checadores para una misma empresa.
- [ ] Cada checador queda ligado a `EmpresaId`.
- [ ] La pantalla de checadores ya sirve como maestro central.

---

# Fase 2 — Exponer API central en `Zenith` para agentes internos
## Objetivo
Dar a `ZkTecoApi` una interfaz segura para obtener configuración y enviar datos.

## Estado actual
`Parcial avanzada`.

Ya existe:
- `POST /api/rrhh/marcaciones/sync` en `MundoVs/Program.cs`.
- `GET /api/rrhh/agentes/configuracion`.
- `POST /api/rrhh/agentes/heartbeat`.
- `POST /api/rrhh/agentes/logs`.

Falta:
- administrar credenciales por agente de forma más operativa si luego se saca de configuración,
- endurecimiento posterior de observabilidad y política final en `Fase 8`.

## Archivos principales a tocar
- `MundoVs/Program.cs`
- `Zenith.Contracts/Asistencia/ChecadorConfigDto.cs`
- `Zenith.Contracts/Asistencia/MarcacionSyncBatchDto.cs`
- `Zenith.Contracts/Asistencia/SyncResultDto.cs`
- posible archivo nuevo para DTOs de agente si se separa contrato de configuración/heartbeat

## Resultado esperado
`Zenith` ya puede responder configuración de checadores y recibir lotes de marcaciones desde el agente interno.

## Paso a paso
1. Definir el esquema de autenticación del agente.
2. Crear validación por `ApiKey` o token técnico.
3. Implementar `GET /api/rrhh/agentes/configuracion`.
4. Hacer que ese endpoint devuelva únicamente los checadores activos de la empresa del agente.
5. Incluir en la respuesta:
   - `EmpresaId`,
   - intervalo,
   - lista `Checadores[]`,
   - parámetros operativos básicos.
6. Implementar `POST /api/rrhh/marcaciones/sync`.
7. Validar que el lote recibido pertenece a la empresa autenticada.
8. Validar que el `ChecadorId` recibido existe y pertenece a esa empresa.
9. Implementar `POST /api/rrhh/agentes/heartbeat`.
10. Registrar errores y eventos importantes del agente.

## Criterio de avance visible
Esta fase se considera terminada cuando un agente autenticado puede:
- pedir su configuración,
- enviar marcaciones,
- reportar heartbeat,
- y `Zenith` responde consistentemente.

## Entregables
- endpoint de configuración,
- endpoint de sincronización,
- endpoint de heartbeat,
- endpoint de logs,
- autenticación técnica para agentes.

## Checklist de cierre
- [x] Un agente autenticado ya puede pedir configuración.
- [x] `Zenith` ya puede recibir lotes de marcaciones.
- [x] `Zenith` ya puede registrar heartbeat del agente.

---

# Fase 3 — Implementar ingesta robusta de marcaciones en `Zenith`
## Objetivo
Guardar las marcaciones crudas sin perder datos y sin duplicar registros.

## Estado actual
`Parcial avanzada`.

Ya existe en `MundoVs/Program.cs` y `MundoVs/Core/Services/RrhhMarcacionIngestionService.cs`:
- validación de `ApiKey`,
- validación de `EmpresaId` y `ChecadorId`,
- cálculo de `HashUnico`,
- deduplicación,
- inserción en `RrhhMarcacion`,
- asociación básica con `Empleado` por `CodigoChecador`,
- actualización de `UltimaSincronizacionUtc`,
- log en `RrhhLogChecador`.

Falta revisar o reforzar:
- endurecimiento posterior con pruebas y métricas en `Fase 8`.

## Archivos principales a tocar
- `MundoVs/Program.cs`
- `MundoVs/Core/Entities/RrhhMarcacion.cs`
- `MundoVs/Core/Entities/RrhhLogChecador.cs`
- `MundoVs/Core/Entities/Empleado.cs`
- `MundoVs/Infrastructure/Data/CrmDbContext.cs`

## Resultado esperado
Cada lote recibido desde el agente se convierte en `RrhhMarcacion` persistida y deduplicada.

## Paso a paso
1. Definir la regla exacta para construir `HashUnico`.
2. Implementar la generación de `HashUnico` durante la ingesta.
3. Validar duplicados antes de insertar o usar índice único para resolverlos.
4. Guardar cada evento como `RrhhMarcacion`.
5. Copiar `PayloadRaw` para soporte y auditoría.
6. Intentar resolver `EmpleadoId` usando `Empleado.CodigoChecador`.
7. Si no se puede resolver empleado, guardar la marcación de todas formas.
8. Marcar la marcación como pendiente de procesamiento.
9. Actualizar en el checador:
   - `UltimaSincronizacionUtc`,
   - `UltimoEventoLeido`.
10. Regresar un `SyncResultDto` claro al agente.

## Criterio de avance visible
Esta fase se considera terminada cuando:
- un lote válido se persiste correctamente,
- los duplicados no se repiten,
- las marcaciones sin empleado no se pierden,
- y el checador refleja su última sincronización.

## Entregables
- ingesta estable en `RrhhMarcacion`,
- deduplicación funcional,
- asociación básica con empleado cuando exista match.

## Checklist de cierre
- [x] El mismo evento no se duplica.
- [x] Una marcación sin empleado no se pierde.
- [x] La respuesta al agente informa resultado real del lote.

---

# Fase 4 — Adaptar `ZkTecoApi` al modelo real de `Zenith`
## Objetivo
Quitar la dependencia del proyecto legado y convertir `ZkTecoApi` en un agente interno compatible con `MundoVs`.

## Estado actual
`Completada`.

Hoy existe una base funcional equivalente en `Zenith.Workers.Asistencia`:
- `ZkTecoMarcacionReader` ya lee el reloj,
- `AsistenciaSyncService` ya arma lotes,
- `HttpMarcacionSyncClient` ya publica al ERP,
- `StaticChecadorConfigProvider` todavía usa configuración local.

Esa base se usó solo como referencia de contratos y flujo. El agente interno final quedó implementado en `..\ZkTecoApi_Zenith`.

Eso significa que el trabajo de esta fase tomó esta decisión final:

### Ruta A
Adaptar directamente `..\ZkTecoApi_Zenith` al modelo `Zenith`.

### Ruta B
Usar `Zenith.Workers.Asistencia` solo como referencia temporal de contratos y flujo.

La decisión final de arquitectura para este repositorio es que el agente interno viva en `ZkTecoApi`, porque ese proyecto se desplegará dentro del servidor interno del cliente.

## Archivos de referencia temporal tomados de `Ruta B`
- `Zenith.Workers.Asistencia/Program.cs`
- `Zenith.Workers.Asistencia/Worker.cs`
- `Zenith.Workers.Asistencia/Providers/StaticChecadorConfigProvider.cs`
- `Zenith.Workers.Asistencia/Services/AsistenciaSyncService.cs`
- `Zenith.Workers.Asistencia/Clients/HttpMarcacionSyncClient.cs`
- `Zenith.Workers.Asistencia/Readers/ZkTecoMarcacionReader.cs`
- `Zenith.Workers.Asistencia/Options/AsistenciaWorkerOptions.cs`

## Archivos principales a tocar si se insiste en adaptar `ZkTecoApi`
- `..\ZkTecoApi_Zenith/Program.cs`
- `..\ZkTecoApi_Zenith/ChecadorBackgroundService.cs`
- `..\ZkTecoApi_Zenith/Controllers/ChecadoresController.cs`
- eliminar o dejar fuera `..\ZkTecoApi_Zenith/HiramDbContext.cs`

## Resultado esperado
`ZkTecoApi` deja de depender de `HiramDbContext` en la sincronización y trabaja como lector y publicador hacia `Zenith`.

## Paso a paso
1. Eliminar la dependencia de `HiramDbContext` en el flujo de sincronización.
2. Dejar de leer checadores desde `AttendanceClock`.
3. Dejar de guardar asistencias directas en `Attendance`.
4. Dejar de resolver turnos con `Shift` y `ShiftRotation` del proyecto legado.
5. Mantener únicamente la lógica de lectura del SDK `zkemkeeper`.
6. Crear un cliente HTTP dedicado para hablar con `Zenith`.
7. Implementar el consumo del endpoint de configuración.
8. Cargar desde `Zenith` la lista de checadores que debe atender el agente.
9. Leer cada reloj local usando la configuración recibida.
10. Mapear los eventos del reloj a `MarcacionRawDto`.
11. Agrupar por checador y enviar como `MarcacionSyncBatchDto`.
12. Registrar heartbeat periódico.

## Criterio de avance visible
Esta fase se considera terminada cuando el agente:
- ya no depende de `HiramDbContext`,
- obtiene checadores desde `Zenith`,
- lee el reloj local,
- y publica lotes al endpoint central.

## Entregables
- `ZkTecoApi` desacoplado del modelo legado,
- lectura local de relojes,
- publicación de lotes a `Zenith`.

## Checklist de cierre
- [x] `HiramDbContext` ya no participa en la sincronización.
- [x] `ZkTecoApi` ya puede leer configuración desde `Zenith`.
- [x] `ZkTecoApi` ya puede publicar marcaciones a `Zenith`.

---

# Fase 5 — Hacer el agente tolerante a fallos
## Objetivo
Evitar pérdida de información cuando falle internet, el `VPS` o la API central.

## Estado actual
`Parcial`.

Hoy el agente en `..\ZkTecoApi_Zenith` ya cuenta con:
- cola local persistente en archivos `json`,
- checkpoint persistente por checador,
- reproceso de lotes pendientes,
- reintentos con backoff,
- aislamiento de fallos por checador.

## Archivos principales a tocar
- `..\ZkTecoApi_Zenith/ChecadorBackgroundService.cs`
- `..\ZkTecoApi_Zenith/Options/AsistenciaAgentOptions.cs`
- `..\ZkTecoApi_Zenith/Services/LocalSyncStateStore.cs`
- `..\ZkTecoApi_Zenith/Models/OfflineSyncStateModels.cs`
- `..\ZkTecoApi_Zenith/appsettings.json`

## Resultado esperado
El agente sigue leyendo, guarda temporalmente y reintenta sin perder eventos.

## Paso a paso
1. Definir un almacenamiento local liviano para cola temporal.
2. Separar la lectura del reloj del envío HTTP.
3. Guardar temporalmente los lotes cuando el envío falle.
4. Implementar reintentos con backoff controlado.
5. Guardar checkpoint local por checador.
6. Evitar que un fallo de un checador detenga a los demás.
7. Registrar logs técnicos locales para diagnóstico.
8. Sincronizar esos errores críticos a `Zenith` cuando vuelva la conectividad.

## Criterio de avance visible
Esta fase se considera terminada cuando:
- el agente sigue leyendo aunque falle el envío,
- conserva temporalmente los lotes,
- y los reenvía después sin perder eventos.

## Entregables
- cola local,
- checkpoint local,
- reintentos automáticos,
- tolerancia básica a desconexión.

## Checklist de cierre
- [x] Si `Zenith` cae, el agente no pierde eventos.
- [x] Si un checador falla, el resto sigue operando.
- [x] El agente puede recuperar lotes pendientes después.

---

# Fase 6 — Procesar marcaciones a asistencia dentro de `Zenith`
## Objetivo
Convertir la marca cruda en una asistencia interpretable por `RRHH`.

## Estado actual
`Parcial`.

Hoy `Zenith` ya ingiere `RrhhMarcacion` y ya existe un procesador inicial que clasifica marcaciones pendientes al finalizar el `sync`.
Además, el reproceso ya recalcula por empleado y fecha usando todas las marcaciones relacionadas del día, no solo el lote recién recibido.
También existe ya un punto de entrada administrativo para reprocesar asistencias por empresa, rango de fechas y empleado opcional.
Ahora también resuelve turno histórico desde `rrhh_empleado_turno`, con siembra inicial de vigencias desde `Empleado.TurnoBaseId` para la transición.

## Archivos principales a tocar
- archivo nuevo para `RrhhAsistenciaProcessor` o equivalente
- `MundoVs/Core/Entities/RrhhAsistencia.cs`
- `MundoVs/Core/Entities/Empleado.cs`
- `MundoVs/Core/Entities/TurnoBase.cs`
- `MundoVs/Infrastructure/Data/CrmDbContext.cs`
- `MundoVs/Program.cs` si se registra el servicio ahí

## Resultado esperado
`Zenith` ya transforma `RrhhMarcacion` en `RrhhAsistencia` usando reglas del negocio actual.

## Paso a paso
1. Crear el servicio de procesamiento de marcaciones.
2. Resolver la fecha local desde `FechaHoraMarcacionUtc` y la zona horaria del checador.
3. Resolver el empleado final usando `CodigoChecador`.
4. Tomar el turno del empleado para esa fecha.
5. En primera etapa, usar `Empleado.TurnoBaseId` como base.
6. En segunda etapa, incorporar `rrhh_empleado_turno` para vigencia histórica.
7. Agrupar marcaciones por empleado y fecha local.
8. Determinar entrada real y salida real.
9. Calcular retardo, salida anticipada y minutos extra cuando aplique.
10. Marcar casos especiales:
    - sin empleado,
    - sin turno,
    - marca incompleta,
    - marca no reconocida.
11. Crear o actualizar `RrhhAsistencia`.
12. Marcar `RrhhMarcacion.Procesada` y guardar resultado.

## Criterio de avance visible
Esta fase se considera terminada cuando:
- ya existen asistencias calculadas por día,
- pueden distinguirse las marcas no clasificadas,
- y el reproceso no duplica asistencias.

## Entregables
- servicio `RrhhAsistenciaProcessor`,
- generación básica de `RrhhAsistencia`,
- rastro de procesamiento por marcación.

## Checklist de cierre
- [x] Ya se generan asistencias desde marcas crudas.
- [x] Los casos no resueltos quedan identificados.
- [x] El proceso puede reintentarse sin romper consistencia.

---

# Fase 7 — Completar la UI operativa de `RRHH`
## Objetivo
Permitir que operación y `RRHH` vean, administren y corrijan el flujo completo.

## Estado actual
`Parcial`.

Ya existe:
- `RRHH/Checadores.razor`.

Quedó completada con:
- vista de marcaciones,
- vista de asistencias,
- vista de estado del agente,
- revisión operativa compacta inicial.

## Archivos principales a tocar
- `MundoVs/Components/Pages/RRHH/Checadores.razor`
- archivo nuevo para `MundoVs/Components/Pages/RRHH/Marcaciones.razor`
- archivo nuevo para `MundoVs/Components/Pages/RRHH/Asistencias.razor`
- archivo nuevo para una vista de estado de agentes
- `MundoVs/Components/Layout/NavMenu.razor`

## Resultado esperado
La web ya permite administrar checadores, revisar marcaciones, revisar asistencias y monitorear el estado del agente.

## Paso a paso
1. Completar `RRHH/Checadores.razor` con vista compacta y filtros.
2. Crear o completar una vista de `Marcaciones`.
3. Mostrar filtros por empresa, checador, fecha, empleado y estado.
4. Mostrar si la marcación quedó:
   - procesada,
   - pendiente,
   - duplicada,
   - no reconocida.
5. Crear o completar una vista de `Asistencias`.
6. Mostrar entrada/salida programada y real.
7. Mostrar retardo, salida anticipada, extra y observaciones.
8. Crear una vista de estado de agentes.
9. Mostrar última comunicación, última sincronización y errores recientes.
10. Permitir revisión manual de incidencias si aplica.

## Criterio de avance visible
Esta fase se considera terminada cuando el usuario puede:
- administrar checadores,
- ver marcaciones crudas,
- ver asistencias calculadas,
- y revisar el estado operativo del agente.

## Entregables
- UI de checadores,
- UI de marcaciones,
- UI de asistencias,
- UI de monitoreo del agente.

## Checklist de cierre
- [x] Operación ya puede revisar qué llegó del reloj.
- [x] `RRHH` ya puede revisar asistencias generadas.
- [x] Ya existe una vista básica de salud del agente.

---

# Fase 8 — Cierre productivo y endurecimiento final
## Objetivo
Dejar la solución lista para operar en escenarios reales con menor riesgo.

## Estado actual
`Completada`.

Ya existe una primera capa de monitoreo operativo:
- `rrhh_estado_agente` persiste la última comunicación del agente,
- `heartbeat` y `logs` actualizan estado resumido por agente,
- `EstadoAgente.razor` muestra alertas básicas por heartbeat vencido.

También ya existe una base mínima de pruebas automatizadas en `MundoVs.Tests` para:
- ingesta de marcaciones,
- deduplicación,
- procesamiento base de asistencia.

Política operativa actual definida:
- reintento inicial de `15s` con backoff exponencial hasta `300s`,
- máximo `40` reintentos por lote pendiente,
- retención de cola local por `30` días,
- retención de checkpoints por `45` días,
- reproceso histórico limitado a `31` días por operación con bitácora,
- `ClearGLog` deshabilitado por defecto y solo permitido con confirmación explícita y llave administrativa si se configura,
- almacenamiento principal de marcaciones en UTC y reproceso requerido cuando cambie zona horaria efectiva del checador.

## Archivos principales a tocar
- `MundoVs/Program.cs`
- `Zenith.Workers.Asistencia/Program.cs`
- `Zenith.Workers.Asistencia/Worker.cs`
- `docs/rrhh-asistencia-zkteco-arquitectura-fases.md`
- pruebas o archivos de soporte que se agreguen

## Resultado esperado
La arquitectura queda operable, trazable y más fácil de mantener.

## Paso a paso
1. Definir política final de reintentos.
2. Definir política de expiración o compactación de cola local.
3. Definir manejo de cambio de horario y zona horaria.
4. Definir procedimiento de reprocesamiento histórico.
5. Restringir `borrar checadas` a uso administrativo controlado.
6. Agregar métricas o logs suficientes para soporte.
7. Agregar alertas por agente sin heartbeat.
8. Agregar pruebas de integración para sync.
9. Agregar pruebas de deduplicación.
10. Agregar pruebas del procesamiento de asistencia.

## Criterio de avance visible
Esta fase se considera terminada cuando:
- existe monitoreo básico,
- hay política de recuperación,
- y el sistema ya se puede operar con menor riesgo productivo.

## Entregables
- políticas operativas definidas,
- endurecimiento de seguridad,
- pruebas críticas mínimas,
- operación más segura en producción.

## Checklist de cierre
- [x] Hay procedimiento claro de soporte y recuperación.
- [x] Hay monitoreo de agentes y errores.
- [x] Hay pruebas mínimas para no romper sync y asistencia.

---

## Orden recomendado de implementación real
Si urge pero debe quedar robusto, el orden recomendado es este:

1. `Zenith` guarda y administra checadores.
2. `Zenith` expone endpoints de configuración y sync.
3. `Zenith` ingiere `RrhhMarcacion` con deduplicación.
4. `ZkTecoApi` se adapta para leer local y publicar a `Zenith`.
5. `Zenith` implementa procesamiento de turnos y `RrhhAsistencia`.
6. `Zenith` muestra pantallas operativas y monitoreo.

---

## Decisiones recomendadas
- Mantener arquitectura `push`.
- No exponer SQL del `VPS` a la red del cliente.
- No dejar la lógica de turnos dentro del agente.
- Guardar siempre evento crudo antes de calcular asistencia.
- Soportar reintento offline en el agente.
- Hacer deduplicación fuerte en `Zenith`.

---

## Qué partes del proyecto legado sí conviene rescatar
De `ZkTecoApi` conviene rescatar:
- conexión al SDK `ZKTeco`,
- lectura de usuarios del dispositivo si aporta valor,
- lectura de logs,
- experiencia práctica de polling.

No conviene conservar ahí como fuente principal:
- resolución final de turno,
- guardado de asistencia final,
- acoplamiento a modelos `Employee`, `Shift`, `Attendance`,
- dependencia a una BD de negocio distinta.

---

## Resultado final esperado
Al terminar estas fases, la solución debe quedar así:

- `Zenith` administra y guarda todo el negocio.
- `ZkTecoApi` solo actúa como agente seguro dentro de la LAN.
- La red del cliente nunca toca SQL del `VPS`.
- La lógica de turnos y asistencia vive centralizada en `Zenith`.
- Se puede crecer a varias empresas, varias sucursales y varios agentes sin rehacer el modelo.
