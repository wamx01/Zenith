# 02. Administración inicial dentro de la empresa

## Objetivo
Este manual describe qué debe configurar el administrador de la empresa una vez que el tenant ya fue creado en `Zenith`.

## Perfil que ejecuta este manual
Usuario recomendado:
- administrador de la empresa
- no `SuperAdmin`

## Precondiciones
Antes de iniciar, ya debe existir:
- la empresa dada de alta
- al menos un usuario administrador inicial
- acceso al tenant correcto

## Alcance
En esta etapa el administrador de la empresa deja listo el entorno interno para operar.

No incluye:
- alta global de empresas
- capacidades globales de plataforma
- administración global de suscripciones

Eso corresponde a `SuperAdmin`.

## Secuencia recomendada

### Paso 1. Entrar al tenant correcto
Validar:
- que el nombre de la empresa sea el correcto
- que los módulos visibles correspondan a la empresa
- que el usuario tenga acceso de administración

Resultado esperado:
- el administrador ya está trabajando dentro de la empresa correcta

---

### Paso 2. Revisar configuración general
Ruta sugerida:
- `Admin > Configuración`

Revisar:
- módulos habilitados
- accesos visibles
- catálogos disponibles para esa empresa

Qué validar:
- que no aparezcan módulos fuera del alcance contratado
- que sí aparezcan los catálogos necesarios para la operación

Resultado esperado:
- el menú y la configuración base quedan alineados al uso real del cliente

---

### Paso 3. Definir tipos de usuario
Ruta sugerida:
- `Admin > Tipos de usuario`

Crear al menos:
- administrador
- auxiliar administrativo
- operación
- supervisión
- RH o nómina si aplica

Configurar:
- capacidades por módulo
- restricciones según función real del puesto

Resultado esperado:
- quedan definidos los perfiles internos de trabajo

---

### Paso 4. Crear usuarios internos
Ruta sugerida:
- `Admin > Usuarios`

Dar de alta:
- usuarios administrativos
- usuarios operativos
- responsables de supervisión

Asignar:
- tipo de usuario
- estado
- acceso inicial

Resultado esperado:
- cada persona entra con su perfil correcto y sin permisos sobrantes

---

### Paso 5. Configurar estructura organizacional
Rutas sugeridas:
- `Admin > Departamentos`
- `Admin > Posiciones`

Capturar:
- departamentos reales de la empresa
- posiciones o puestos
- relación esperada para empleados y operación

Resultado esperado:
- queda lista la estructura organizacional base para `RRHH` y operación

---

### Paso 6. Configurar parámetros de nómina, si aplica
Ruta sugerida:
- `Admin > Configuración nómina`

Definir:
- días base
- horas base
- factor de horas extra
- periodicidad aplicable

Resultado esperado:
- `RRHH` puede trabajar prenómina y nómina con parámetros base correctos

---

### Paso 7. Configurar catálogos administrativos adicionales
Según módulos activos, revisar:
- `Tipos de inventario`
- catálogos de procesos
- catálogos de bonos
- configuraciones administrativas de operación

Resultado esperado:
- el tenant queda preparado para empezar la captura funcional sin improvisar catálogos

---

### Paso 8. Entregar a responsables de módulo
Cuando la base administrativa ya esté lista, continuar con los responsables de:
- `Ventas`
- `RRHH`
- `Compras`
- `Producción`
- `Contabilidad`

Resultado esperado:
- cada área ya puede continuar su propia configuración operativa

## Checklist rápido del administrador de empresa
Antes de cerrar esta etapa, validar:
- módulos correctos
- roles creados
- usuarios creados
- departamentos creados
- posiciones creadas
- configuración de nómina revisada si aplica
- catálogos administrativos base listos

## Relación con el manual anterior
Este manual ocurre después de:
- [01. Configuración inicial de una empresa nueva](./01-configuracion-inicial-empresa.md)

## Siguiente manual sugerido
Dependiendo del orden de implementación:
- `03-admin-usuarios-y-roles.md`
- `03-rrhh-configuracion-inicial.md`
- `03-ventas-configuracion-inicial.md`
