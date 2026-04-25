# 03. Administración de usuarios y roles de la empresa

## Objetivo
Este manual explica cómo debe configurar el administrador de la empresa los `Tipos de usuario` y los `Usuarios` dentro del tenant para que cada persona opere con permisos correctos.

## Alcance
Incluye:
- definición de roles internos
- asignación de capacidades por módulo
- creación de usuarios
- validación básica de acceso

No incluye:
- capacidades globales de plataforma
- administración global de empresas
- suscripciones

## Rutas principales
- `Admin > Tipos de usuario`
- `Admin > Usuarios`

## Orden recomendado
Primero:
- crear tipos de usuario

Después:
- crear usuarios

Nunca al revés, porque el usuario debe nacer ya con un perfil claro.

## Parte 1. Tipos de usuario

### Objetivo de esta etapa
Definir perfiles reutilizables para no asignar permisos manualmente usuario por usuario.

### Roles mínimos sugeridos
Dependiendo de la empresa, crear al menos:
- `Administrador`
- `Auxiliar administrativo`
- `Supervisor`
- `Operación`
- `RH`
- `Nómina`
- `Ventas`
- `Compras`

La empresa puede tener menos o más roles, pero conviene iniciar con perfiles simples.

### Qué configurar en cada tipo de usuario
Definir:
- nombre del rol
- descripción interna si aplica
- capacidades por módulo
- alcance real de trabajo

### Recomendaciones prácticas
- dar solo permisos necesarios
- evitar que usuarios operativos tengan permisos administrativos
- separar `consulta` de `edición` cuando el sistema lo permita
- separar `RH`, `Nómina`, `Ventas` y `Compras` si las funciones no las hace la misma persona

### Resultado esperado
- existe un catálogo de roles claro
- los permisos ya no se asignan de forma improvisada

---

## Parte 2. Usuarios

### Objetivo de esta etapa
Crear cuentas de acceso para cada persona con el tipo de usuario correcto.

### Qué capturar por usuario
- nombre
- correo o identificador de acceso
- tipo de usuario
- estado
- contraseña inicial o mecanismo equivalente

### Reglas recomendadas
- un usuario por persona
- no compartir cuentas entre empleados
- no usar cuentas genéricas para operar diario
- inactivar usuarios que ya no laboran en la empresa

### Asignación correcta
Cada usuario debe quedar ligado a:
- un tipo de usuario
- el tenant correcto
- un estado válido

### Resultado esperado
- cada persona puede entrar con permisos coherentes a su función

---

## Parte 3. Validación inicial de permisos
Después de crear roles y usuarios, validar con pruebas simples.

### Validaciones mínimas
Probar que:
- el administrador vea configuración y catálogos administrativos
- un usuario operativo no vea opciones administrativas
- un usuario de `RH` vea empleados, turnos y asistencias si aplica
- un usuario de `Ventas` vea clientes, productos y pedidos si aplica
- un usuario de `Compras` vea proveedores y cuentas por pagar si aplica

### Qué hacer si algo sale mal
Revisar en este orden:
1. tipo de usuario asignado
2. capacidades configuradas en el tipo de usuario
3. módulos habilitados para la empresa
4. estado del usuario

---

## Errores comunes a evitar
- crear usuarios antes de definir roles
- dar demasiados permisos al rol de operación
- usar un solo rol para toda la empresa
- dejar usuarios activos cuando ya no deben entrar
- mezclar responsabilidades administrativas con operativas sin necesidad

## Checklist rápido
Antes de cerrar esta etapa, validar:
- tipos de usuario creados
- capacidades revisadas por rol
- usuarios principales creados
- usuarios ligados al rol correcto
- pruebas básicas de acceso realizadas

## Relación con manuales anteriores
Este manual ocurre después de:
- [01. Configuración inicial de una empresa nueva](./01-configuracion-inicial-empresa.md)
- [02. Administración inicial dentro de la empresa](./02-admin-empresa-configuracion-base.md)

## Siguiente manual sugerido
Dependiendo del orden de implementación:
- `04-admin-estructura-organizacional.md`
- `04-rrhh-configuracion-inicial.md`
- `04-ventas-configuracion-inicial.md`
