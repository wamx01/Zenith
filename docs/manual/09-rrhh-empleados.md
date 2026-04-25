# 09. Alta y administración de empleados en `RRHH`

## Objetivo
Este manual explica cómo capturar empleados en `Zenith` para que queden listos para asistencia, control de tiempo, destajo, prenómina y nómina.

## Alcance
Incluye:
- alta de empleados
- relación con departamento y posición
- relación con turno y esquema de pago
- validaciones básicas antes de operar

No incluye:
- cálculo de nómina
- captura diaria de asistencias
- operación detallada de vales de destajo

## Ruta principal
- `RRHH > Empleados`

## Base previa requerida
Antes de capturar empleados, ya debe existir:
- departamentos
- posiciones
- turnos
- esquemas de pago
- configuración base de nómina

## Por qué este orden importa
El empleado no debe darse de alta como un registro aislado.

Debe quedar ligado desde el inicio a su contexto real:
- área organizacional
- puesto
- turno
- periodicidad
- forma de pago

Si eso no queda claro desde el alta, después se generan errores en:
- asistencias
- horas extra
- destajo
- prenómina
- nómina

## Información principal del empleado
Al dar de alta un empleado, revisar al menos:
- datos personales
- datos laborales
- número o clave interna si aplica
- departamento
- posición
- turno vigente o base
- periodicidad de pago
- esquema de pago vigente

## Parte 1. Datos organizacionales
### Departamento
Sirve para ubicar al empleado dentro de la empresa.

### Posición
Sirve para definir su función organizacional.

Importante:
- `Posición` no es lo mismo que `Tipo de usuario`
- `Posición` define el puesto real
- `Tipo de usuario` define acceso al sistema

## Parte 2. Datos operativos
### Turno
El turno ayuda a interpretar:
- entrada esperada
- salida esperada
- descansos
- retardos
- salidas anticipadas
- tiempo extra

### Periodicidad de pago
Debe quedar clara desde el alta, por ejemplo:
- semanal
- quincenal
- mensual

### Esquema de pago
Debe corresponder a la lógica real con la que se le pagará al empleado.

Ejemplos:
- sueldo fijo
- destajo por pieza
- destajo por operación
- esquema mixto

## Parte 3. Relación con historial
Si el sistema maneja historial de esquemas o turnos, conviene que el alta inicial ya nazca correctamente para evitar correcciones posteriores.

La idea es que el empleado inicie con:
- turno correcto
- esquema correcto
- vigencia correcta si aplica

## Validaciones mínimas después del alta
Antes de considerar listo al empleado, validar:
- departamento correcto
- posición correcta
- turno correcto
- periodicidad correcta
- esquema de pago correcto
- estado activo o inactivo según corresponda

## Qué debe quedar listo con un empleado bien capturado
Un empleado bien configurado puede participar correctamente en:
- marcaciones
- asistencias
- control de tiempo
- vales de destajo
- prenómina
- nómina

## Errores comunes a evitar
- capturar empleados sin posición
- capturar empleados sin turno cuando sí usan asistencia
- asignar un esquema de pago provisional y dejarlo así
- confundir puesto con rol del sistema
- registrar periodicidad incorrecta
- no revisar si el empleado realmente entra al módulo de asistencia o destajo

## Casos prácticos rápidos
### Caso A. Empleado administrativo
Recomendación:
- departamento administrativo
- posición administrativa
- turno administrativo
- esquema de sueldo fijo

### Caso B. Operador de producción con destajo
Recomendación:
- departamento de producción
- posición operativa
- turno productivo
- esquema de destajo correcto

### Caso C. Supervisor con fijo más bono
Recomendación:
- posición de supervisión
- turno correspondiente
- esquema mixto o fijo más bono

## Checklist rápido
Antes de cerrar esta etapa, validar:
- empleados capturados
- datos organizacionales correctos
- turno asignado correctamente
- periodicidad confirmada
- esquema de pago asignado correctamente
- empleados listos para asistencia o nómina según aplique

## Relación con manuales anteriores
Este manual ocurre después de:
- [06. Configuración inicial de RRHH](./06-rrhh-configuracion-inicial.md)
- [07. Configuración de turnos en RRHH](./07-rrhh-turnos.md)
- [08. Esquemas de pago en RRHH](./08-rrhh-esquemas-de-pago.md)

## Referencias relacionadas
- `../modulos/detalle/06-rrhh.md`
- `MundoVs/Components/Pages/RRHH/Empleados.razor`

## Siguiente manual sugerido
El siguiente paso lógico es:
- `10-rrhh-marcaciones-y-asistencias.md`
- o `10-rrhh-prenomina.md` si primero se documentará el cálculo operativo
