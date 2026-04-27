# Manual de puesta en marcha — `RRHH` asistencia con `Zenith` + `ZkTecoApi`

## Objetivo
Este manual explica cómo configurar y arrancar el flujo de asistencia con checadores `ZKTeco`, usando:

- `Zenith` como sistema central
- `ZkTecoApi` como agente interno dentro de la red del cliente

También aclara qué significa cada configuración importante, especialmente:

- `NombreAgente`
- `WorkerApiKey`
- `ApiKey`
- `IntervaloSegundos`
- `ASPNETCORE_URLS` o `ASPNETCORE_HTTP_PORTS`
- `Auth:CookieSecurePolicy`
- `Auth:UseHttpsRedirection`

---

## Arquitectura final
El flujo correcto es:

`Checador ZKTeco -> ZkTecoApi interno -> API Zenith -> BD Zenith`

### Qué vive en `Zenith`
Aquí configuras lo funcional y lo de negocio:

- checadores
- empleados
- turnos
- marcaciones
- asistencias
- monitoreo del agente

### Qué vive en `ZkTecoApi`
Aquí configuras lo técnico del agente:

- URL de `Zenith`
- credenciales técnicas
- nombre del agente
- frecuencia de sincronización
- carpeta local de cola
- política de reintentos

---

## Conceptos clave

## 1. `NombreAgente`
Es el nombre lógico de una instalación del agente.

### Qué representa
No es una llave secreta.
No es el nombre del checador.
No es el nombre de la empresa.

Es un identificador legible para distinguir una instalación del agente.

### Para qué sirve
- aparece en heartbeat y logs
- aparece en la pantalla `RRHH/Estado del agente`
- ayuda a distinguir instalaciones si una empresa tiene más de un agente
- puede participar en la validación cuando configuras agentes específicos en `Zenith`

### Ejemplos válidos
- `Agente Planta Norte`
- `Agente RH Matriz`
- `ZkTecoApi Agent`
- `Agente Sucursal GDL`

### Recomendación
Usa un nombre único y entendible por instalación.

Ejemplo recomendado:
- `Agente Planta 1`

---

## 2. `WorkerApiKey`
Es una llave global del lado de `Zenith`.

### Dónde vive
En `MundoVs/appsettings.json` dentro de:

- `Asistencia:WorkerApiKey`

### Para qué sirve
Es una llave general de compatibilidad para autenticar agentes si no quieres todavía configurar una lista detallada de agentes.

### Cuándo usarla
Úsala si quieres una configuración simple:
- un solo agente
- ambiente de pruebas
- instalación rápida

### Ejemplo
```json
{
  "Asistencia": {
    "WorkerApiKey": "MI-LLAVE-GLOBAL-SEGURA-123",
    "IntervaloSegundos": 60,
    "HeartbeatStaleMinutes": 5,
    "ReprocesoMaxDays": 31,
    "Agentes": []
  }
}
```

---

## 3. `ApiKey`
Es la llave que el agente `ZkTecoApi` envía a `Zenith` en cada request.

### Dónde vive
En `..\ZkTecoApi_Zenith\appsettings.json` dentro de:

- `AsistenciaAgent:ApiKey`

### Qué hace
Se manda en el header:

- `X-Zenith-Worker-Key`

### Cómo sabe `Zenith` si es correcta
`Zenith` acepta la autenticación si la `ApiKey` del agente coincide con una de estas opciones:

#### Opción A. Llave global
Coincide con:
- `Asistencia:WorkerApiKey`

#### Opción B. Llave por agente
Coincide con:
- `Asistencia:Agentes[n].ApiKey`

Y además puede validar:
- `EmpresaId`
- `NombreAgente`

### Conclusión práctica
La `ApiKey` del agente es correcta si coincide exactamente con la llave que `Zenith` espera.

---

## 4. ¿Qué conviene usar: `WorkerApiKey` o `Agentes`?

## Opción simple para pruebas
Usar solo:
- `Asistencia:WorkerApiKey`

Y en el agente:
- `AsistenciaAgent:ApiKey = ese mismo valor`

### Ventaja
- rápido
- simple
- suficiente para piloto

## Opción recomendada para operación más ordenada
Configurar agentes específicos en `Zenith` en:
- `Asistencia:Agentes`

### Ventaja
- mejor control por empresa
- mejor control por nombre de agente
- permite múltiples agentes

---

## 5. `IntervaloSegundos`
Es cada cuántos segundos el agente corre su ciclo de sincronización.

### Qué incluye ese ciclo
En cada ciclo, el agente:
1. consulta configuración
2. intenta reenviar pendientes
3. lee checadores
4. manda lotes nuevos
5. manda heartbeat

### Ejemplos
- `30` = cada 30 segundos
- `60` = cada 60 segundos
- `120` = cada 2 minutos

### Recomendación para piloto
Usa:
- `30` o `60`

### Dónde se puede definir
#### En el agente
- `AsistenciaAgent:IntervaloSegundos`

#### En `Zenith`
- `Asistencia:IntervaloSegundos`
- o por agente en `Asistencia:Agentes[n].IntervaloSegundos`

### Recomendación
Si vas a controlar el comportamiento desde central, usa la configuración de `Zenith`.

---

## 6. Puertos y URLs de hosting
Define en qué puerto y dirección escucha cada app.

### Para qué sirve
Sirve especialmente en publicación, porque evita recompilar solo para cambiar el puerto.

### Dónde vive
#### En `Zenith`
- `ASPNETCORE_URLS`
- `ASPNETCORE_HTTP_PORTS`

#### En `ZkTecoApi`
- `Hosting:Urls` en `..\ZkTecoApi_Zenith\appsettings.json`

### Ejemplos válidos
- `http://*:5130`
- `http://*:5370`
- `https://localhost:7176;http://localhost:5130`
- `http://192.168.1.50:5370`

### Recomendación
#### Para `Zenith` publicado
- preferir `ASPNETCORE_HTTP_PORTS=8080` o el puerto real que quieras exponer detrás de proxy
- usar `ASPNETCORE_URLS` solo si necesitas forzar URLs explícitas y aceptas que eso sobreescriba `HTTP_PORTS` / `HTTPS_PORTS`

#### Para `ZkTecoApi` publicado
- `http://*:5370`

#### Para desarrollo
- `Zenith`: `https://localhost:7176;http://localhost:5130`
- `ZkTecoApi`: `http://localhost:5370`

---

## 7. `Auth:CookieSecurePolicy` y `Auth:UseHttpsRedirection`
Controlan si `Zenith` exige HTTPS para mantener la sesión del login.

### Cuándo importan
Importan sobre todo cuando publicas `Zenith` por `HTTP` interno sin certificado todavía.

Si dejas la cookie en modo seguro estricto y entras por `http://`, el login puede parecer exitoso pero la sesión no se conserva.

### Recomendación para piloto interno por `HTTP`
Usa:

```json
"Auth": {
  "CookieSecurePolicy": "SameAsRequest",
  "SameSite": "Lax",
  "UseHttpsRedirection": false
}
```

### Recomendación para ambiente formal con `HTTPS`
Usa:

```json
"Auth": {
  "CookieSecurePolicy": "Always",
  "SameSite": "Strict",
  "UseHttpsRedirection": true
}
```

### Conclusión práctica
- para pruebas internas en `HTTP`: usar `SameAsRequest` y sin redirección HTTPS
- para producción con certificado: usar `Always` y redirección HTTPS activa

---

## Configuración de `Zenith`

## Archivo
- `MundoVs/appsettings.json`

## Ejemplo simple con llave global
```json
{
  "ConnectionStrings": {
    "ZenithConnection": "Server=SERVIDOR;Port=3306;Database=CrmMundoVs;User=usuario;Password=password;CharSet=utf8mb4;"
  },
  "Hosting": {
    "Urls": "http://*:5130"
  },
  "Auth": {
    "CookieSecurePolicy": "SameAsRequest",
    "SameSite": "Lax",
    "UseHttpsRedirection": false
  },
  "Asistencia": {
    "WorkerApiKey": "MI-LLAVE-GLOBAL-SEGURA-123",
    "IntervaloSegundos": 60,
    "HeartbeatStaleMinutes": 5,
    "ReprocesoMaxDays": 31,
    "Agentes": []
  }
}
```

## Ejemplo recomendado con agente específico
```json
{
  "ConnectionStrings": {
    "ZenithConnection": "Server=SERVIDOR;Port=3306;Database=CrmMundoVs;User=usuario;Password=password;CharSet=utf8mb4;"
  },
  "Hosting": {
    "Urls": "http://*:5130"
  },
  "Auth": {
    "CookieSecurePolicy": "SameAsRequest",
    "SameSite": "Lax",
    "UseHttpsRedirection": false
  },
  "Asistencia": {
    "WorkerApiKey": "",
    "IntervaloSegundos": 60,
    "HeartbeatStaleMinutes": 5,
    "ReprocesoMaxDays": 31,
    "Agentes": [
      {
        "EmpresaId": "11111111-1111-1111-1111-111111111111",
        "NombreAgente": "Agente Planta 1",
        "ApiKey": "APIKEY-PLANTA-1-SEGURA",
        "Activo": true,
        "IntervaloSegundos": 30,
        "PermitirLecturaUsuarios": false,
        "ModoDiagnostico": false
      }
    ]
  }
}
```

## Cuándo sé que está bien configurado
Está correcto si:
- `EmpresaId` coincide con la empresa real
- `NombreAgente` coincide con el del agente interno
- `ApiKey` coincide exactamente con la del agente

---

## Configuración de `ZkTecoApi`

## Archivo
- `..\ZkTecoApi_Zenith\appsettings.json`

## Ejemplo funcional
```json
{
  "Hosting": {
    "Urls": "http://*:5370"
  },
  "AsistenciaAgent": {
    "EmpresaId": "11111111-1111-1111-1111-111111111111",
    "NombreAgente": "Agente Planta 1",
    "IntervaloSegundos": 30,
    "ApiBaseUrl": "https://zenith.midominio.local",
    "ApiConfiguracionPath": "/api/rrhh/agentes/configuracion",
    "ApiMarcacionesPath": "/api/rrhh/marcaciones/sync",
    "ApiHeartbeatPath": "/api/rrhh/agentes/heartbeat",
    "ApiLogsPath": "/api/rrhh/agentes/logs",
    "ApiKey": "APIKEY-PLANTA-1-SEGURA",
    "ZkTecoProgId": "zkemkeeper.CZKEM",
    "LocalStateDirectory": "C:\\ZkTecoApi\\data",
    "InitialRetryDelaySeconds": 15,
    "MaxRetryDelaySeconds": 300,
    "MaxRetryAttempts": 40,
    "PendingBatchRetentionDays": 30,
    "CheckpointRetentionDays": 45,
    "AllowClearGLog": false,
    "ClearGLogAdminKey": "",
    "Checadores": []
  }
}
```

---

## Significado de cada campo del agente

### `EmpresaId`
La empresa a la que pertenece esta instalación del agente.

### `Hosting:Urls`
Puerto y dirección donde escucha el propio `ZkTecoApi`.

#### Ejemplo
- `http://*:5370`

No afecta cómo se conecta a `Zenith`; solo afecta cómo escucharás al agente localmente.

### `NombreAgente`
Nombre legible de esta instalación.

### `ApiBaseUrl`
URL base de `Zenith`.

Ejemplos:
- `https://zenith.midominio.local`
- `https://192.168.1.50:5001`

### `ApiKey`
La llave que envía el agente a `Zenith`.
Debe coincidir con:
- `WorkerApiKey`, o
- `Agentes[n].ApiKey`

### `LocalStateDirectory`
Carpeta local donde el agente guarda:
- lotes pendientes
- checkpoints

Debe tener permisos de escritura.

### `InitialRetryDelaySeconds`
Primer delay de reintento.

### `MaxRetryDelaySeconds`
Máximo delay entre reintentos.

### `MaxRetryAttempts`
Máximo número de reintentos antes de purgar un lote viejo.

### `PendingBatchRetentionDays`
Cuántos días retener lotes pendientes locales.

### `CheckpointRetentionDays`
Cuántos días retener checkpoints antiguos.

### `AllowClearGLog`
Permite o no borrar checadas del reloj.

#### Recomendación
En producción:
- `false`

### `ClearGLogAdminKey`
Llave administrativa opcional para permitir `ClearGLog`.

#### Recomendación
Déjala vacía si no vas a usar borrado administrativo.

---

## Cómo saber si la `ApiKey` es correcta

## Caso 1. Configuración simple
Si en `Zenith` tienes:
```json
"WorkerApiKey": "MI-LLAVE-GLOBAL-SEGURA-123"
```
Entonces en el agente debe ir exactamente:
```json
"ApiKey": "MI-LLAVE-GLOBAL-SEGURA-123"
```

Si son distintas, el agente no autentica.

## Caso 2. Configuración por agente
Si en `Zenith` tienes:
```json
"Agentes": [
  {
    "EmpresaId": "11111111-1111-1111-1111-111111111111",
    "NombreAgente": "Agente Planta 1",
    "ApiKey": "APIKEY-PLANTA-1-SEGURA"
  }
]
```
Entonces el agente debe usar:
```json
"EmpresaId": "11111111-1111-1111-1111-111111111111",
"NombreAgente": "Agente Planta 1",
"ApiKey": "APIKEY-PLANTA-1-SEGURA"
```

## Cómo detecto que está mal
Si está mal, normalmente verás:
- `401 Unauthorized`
- el agente no aparece en `RRHH/Estado del agente`
- no llegan heartbeats
- no llegan marcaciones

---

## Alta de checadores en `Zenith`

## Pantalla
- `RRHH/Checadores`

## Qué capturar por checador
- `Nombre`
- `Ip`
- `Puerto`
- `NumeroMaquina`
- `Ubicacion`
- `ZonaHoraria`
- `NumeroSerie`
- `Activo`

## Ejemplo
- Nombre: `Recepción`
- IP: `192.168.1.20`
- Puerto: `4370`
- Número de máquina: `1`
- Ubicación: `Entrada principal`
- Zona horaria: `UTC`
- Activo: `Sí`

---

## Relación entre empleado y reloj
Para que las marcaciones se asignen correctamente, el empleado debe tener:

- `CodigoChecador`

## Ejemplo
Si en el reloj el empleado marca como `102`, entonces en el sistema:
- `Empleado.CodigoChecador = 102`

Si no coincide, la marca llega pero queda sin empleado asociado.

---

## Cómo se ve en el sistema

## Estado del agente
Pantalla:
- `RRHH/Estado del agente`

Ahí ves:
- agente
- host
- heartbeat
- estado
- errores
- logs recientes

## Checadores
Pantalla:
- `RRHH/Checadores`

Ahí ves:
- última sincronización
- último evento leído
- activo/inactivo

## Marcaciones
Pantalla:
- `RRHH/Marcaciones`

Ahí ves:
- marcas crudas
- empleado asociado
- estado de procesamiento

## Asistencias
Pantalla:
- `RRHH/Asistencias`

Ahí ves:
- entrada real
- salida real
- retardo
- salida anticipada
- tiempo extra

### También ves ahora
- tiempo trabajado bruto
- tiempo trabajado neto
- resumen de descansos detectados
- observaciones de revisión cuando faltan descansos o sobran marcaciones intermedias

---

## Configuración de jornada y descansos

## Dónde se configura
La jornada diaria se configura en:

- `RRHH/Turnos`

Y la asignación del turno al empleado se configura en:

- `RRHH/Empleados`

## Qué se configura por día del turno
Por cada día se puede definir:

- si labora o descansa
- hora de entrada
- hora de salida
- `0`, `1` o `2` descansos
- horario del descanso 1
- horario del descanso 2
- si cada descanso es pagado o no pagado

## Qué representa un descanso pagado
Un descanso pagado:

- sí cuenta como parte de la jornada neta esperada
- no se descuenta del tiempo neto trabajado

## Qué representa un descanso no pagado
Un descanso no pagado:

- sí existe dentro de la jornada bruta
- pero se descuenta del tiempo neto trabajado

## Ejemplo
Turno:

- entrada `08:00`
- salida `17:00`
- descanso 1 `13:00 - 13:30` no pagado
- descanso 2 `16:00 - 16:15` pagado

Entonces:

- jornada bruta esperada = `9:00`
- descanso no pagado = `0:30`
- jornada neta esperada = `8:30`

---

## Cómo interpreta `Zenith` las marcaciones del día

## Regla base
El sistema toma:

- primera marca = entrada
- última marca = salida
- marcas intermedias = descansos tomados por pares

## Casos típicos
### 2 marcas
- entrada
- salida

### 4 marcas
- entrada
- salida a descanso
- regreso de descanso
- salida final

### 6 marcas
- entrada
- salida descanso 1
- regreso descanso 1
- salida descanso 2
- regreso descanso 2
- salida final

## Qué calcula
Por día y por empleado, `Zenith` calcula:

- total de marcaciones
- jornada programada
- jornada neta programada
- minutos trabajados brutos
- minutos trabajados netos
- minutos de descanso programado
- minutos de descanso tomados
- minutos de descanso pagados
- minutos de descanso no pagados
- retardo
- salida anticipada
- tiempo extra

---

## Reglas actuales de revisión de descansos

El sistema marca revisión cuando:

- faltan descansos esperados
- se detectan más descansos de los configurados
- hay una marcación intermedia sin par
- un descanso excede su duración programada

## Regla importante
Si el empleado no salió exactamente a la hora configurada del descanso, pero:

- sí tomó el descanso,
- y la duración tomada coincide o no excede la duración esperada,

no se penaliza automáticamente solo por haberse movido la hora del descanso.

## Ejemplo
Si el turno espera comida de `13:00 a 13:30`, pero el empleado marca:

- salida a comida `13:10`
- regreso `13:40`

el sistema puede aceptarlo sin penalización automática porque la duración sigue siendo `30` minutos.

---

## Cómo corregir histórico

## Caso 1. Primero llegaron marcaciones y luego capturas al empleado
Hoy `Zenith` ya puede:

- religar marcaciones por `CodigoChecador`
- reprocesar asistencias por rango

Esto se hace desde:

- `RRHH/Asistencias` -> `Reprocesar`

## Caso 2. El empleado cambió de turno
Debes:

1. editar el empleado en `RRHH/Empleados`
2. elegir turno vigente
3. capturar `Vigente desde`
4. guardar
5. reprocesar el rango afectado

## Caso 3. Cambió la definición del turno
Si cambias en `RRHH/Turnos`:

- horas de entrada/salida
- descansos
- si un descanso es pagado o no

debes reprocesar el rango afectado para recalcular la asistencia histórica.

---

## Checklist de primera puesta en marcha

## En `Zenith`
- [ ] Existe la empresa correcta
- [ ] Existen empleados con `CodigoChecador`
- [ ] Existen turnos base
- [ ] Existen checadores dados de alta
- [ ] Se aplicaron migraciones

## En `ZkTecoApi`
- [ ] `EmpresaId` correcto
- [ ] `NombreAgente` correcto
- [ ] `ApiBaseUrl` correcta
- [ ] `ApiKey` correcta
- [ ] `LocalStateDirectory` con permisos
- [ ] acceso al reloj por red

## En operación
- [ ] aparece heartbeat
- [ ] aparecen logs
- [ ] aparecen marcaciones
- [ ] aparecen asistencias

---

## Ejemplo recomendado para piloto interno

## En `Zenith`
Usa agente específico:
```json
"Asistencia": {
  "WorkerApiKey": "",
  "IntervaloSegundos": 60,
  "HeartbeatStaleMinutes": 5,
  "ReprocesoMaxDays": 31,
  "Agentes": [
    {
      "EmpresaId": "11111111-1111-1111-1111-111111111111",
      "NombreAgente": "Agente Planta 1",
      "ApiKey": "APIKEY-PLANTA-1-SEGURA",
      "Activo": true,
      "IntervaloSegundos": 30,
      "PermitirLecturaUsuarios": false,
      "ModoDiagnostico": false
    }
  ]
}
```

## En `ZkTecoApi`
```json
"AsistenciaAgent": {
  "EmpresaId": "11111111-1111-1111-1111-111111111111",
  "NombreAgente": "Agente Planta 1",
  "IntervaloSegundos": 30,
  "ApiBaseUrl": "https://zenith.midominio.local",
  "ApiKey": "APIKEY-PLANTA-1-SEGURA",
  "LocalStateDirectory": "C:\\ZkTecoApi\\data",
  "InitialRetryDelaySeconds": 15,
  "MaxRetryDelaySeconds": 300,
  "MaxRetryAttempts": 40,
  "PendingBatchRetentionDays": 30,
  "CheckpointRetentionDays": 45,
  "AllowClearGLog": false,
  "ClearGLogAdminKey": ""
}
```

---

## Recomendación final
Si quieres empezar rápido y bien:

1. usa un solo agente
2. usa un solo checador primero
3. configura `IntervaloSegundos = 30`
4. valida heartbeat
5. valida marcaciones
6. valida asistencias
7. luego agregas más checadores

---

## Archivo relacionado
Este manual complementa:
- `docs/rrhh-asistencia-zkteco-arquitectura-fases.md`
