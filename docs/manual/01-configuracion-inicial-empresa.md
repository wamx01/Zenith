# 01. Configuración inicial de una empresa nueva

## Objetivo
Este manual define el orden recomendado para dar de alta una empresa nueva en `Zenith` y dejarla lista para comenzar a operar sin omitir catálogos ni configuraciones base.

## Orden recomendado

### Paso 1. Alta de la empresa
Ruta sugerida:
- `SuperAdmin > Empresas`

Capturar:
- razón social
- nombre comercial
- RFC
- plan o trial
- límite de usuarios
- estado
- usuario administrador inicial

Resultado esperado:
- la empresa queda creada
- existe un usuario administrador inicial para entrar al tenant

---

### Paso 2. Activación de módulos
Ruta sugerida:
- `Admin > Configuración`

Configurar:
- módulos habilitados para la empresa
- accesos disponibles según la operación real del cliente

Resultado esperado:
- la navegación y las opciones visibles quedan alineadas al alcance contratado

---

### Paso 3. Roles o tipos de usuario
Ruta sugerida:
- `Admin > Tipos de usuario`

Configurar:
- perfiles administrativos
- perfiles operativos
- capacidades por módulo

Resultado esperado:
- quedan definidos los roles base que después se asignarán a los usuarios

---

### Paso 4. Usuarios
Ruta sugerida:
- `Admin > Usuarios`

Configurar:
- usuarios internos
- tipo de usuario de cada uno
- estado
- contraseña inicial o mecanismo de acceso

Resultado esperado:
- el equipo base ya puede entrar con permisos controlados

---

### Paso 5. Catálogos base
Configurar según aplique:
- `Admin > Departamentos`
- `Admin > Posiciones`
- `Admin > Tipos de inventario`
- otros catálogos operativos generales

Resultado esperado:
- quedan listas las estructuras mínimas para empleados, operación e inventario

---

### Paso 6. Configuración por módulo
Configurar solo los módulos activos.

Ejemplos:
- `Admin > Configuración nómina`
- catálogos de procesos productivos
- configuraciones comerciales
- configuraciones de inventario

Resultado esperado:
- cada módulo queda parametrizado antes de capturar operación real

---

### Paso 7. Carga de catálogos maestros operativos
Según el alcance del cliente, cargar:
- clientes
- productos
- productos por cliente
- proveedores
- esquemas de pago
- turnos

Resultado esperado:
- ya existe información base para empezar la captura transaccional

---

### Paso 8. Configuración inicial de `RRHH`, si aplica
Orden sugerido:
- departamentos
- posiciones
- turnos
- esquemas de pago
- empleados

Si además usa asistencia:
- checadores
- agente interno `ZkTecoApi`
- marcaciones
- asistencias

Resultado esperado:
- la empresa queda lista para controlar asistencia y preparar prenómina o nómina

---

### Paso 9. Configuración comercial, si aplica
Orden sugerido:
- clientes
- productos
- productos por cliente
- precios y reglas comerciales necesarias

Resultado esperado:
- ya se pueden capturar pedidos sin improvisar catálogos

---

### Paso 10. Inicio de operación
Comenzar con:
- pedidos
- compras
- cuentas por cobrar o pagar
- producción
- prenóminas y nóminas

Resultado esperado:
- la empresa entra en operación con estructura previa suficiente

## Resumen corto
La secuencia base recomendada es:

`Empresa -> Módulos -> Roles -> Usuarios -> Catálogos base -> Configuración por módulo -> Catálogos operativos -> Operación`

## Variantes rápidas por tipo de arranque

### Si la empresa inicia por `RRHH`
`Empresa -> Módulos -> Roles -> Usuarios -> Departamentos -> Posiciones -> Turnos -> Esquemas de pago -> Empleados -> Checadores -> Marcaciones -> Asistencias -> Prenómina -> Nómina`

### Si la empresa inicia por `Ventas`
`Empresa -> Módulos -> Roles -> Usuarios -> Clientes -> Productos -> Productos por cliente -> Pedidos`

### Si la empresa inicia por operación mixta
`Empresa -> Módulos -> Roles -> Usuarios -> Catálogos base -> RRHH -> Ventas -> Compras -> Operación diaria`

## Siguiente manual sugerido
El siguiente documento puede ser:
- `02-alta-de-empresa-superadmin.md`
- o un manual por módulo, empezando por `Admin` o `RRHH`
