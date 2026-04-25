# Referencias detalladas por módulo

Este directorio contiene documentación detallada por módulo para responder preguntas futuras sobre:
- de dónde sale cierta información,
- qué página la muestra o la captura,
- qué servicio, repositorio o `DbSet` la alimenta,
- y en qué archivos conviene buscar primero.

## Índice
- [`01-auth.md`](./01-auth.md)
- [`02-core.md`](./02-core.md)
- [`03-admin.md`](./03-admin.md)
- [`04-superadmin.md`](./04-superadmin.md)
- [`05-ventas.md`](./05-ventas.md)
- [`06-rrhh.md`](./06-rrhh.md)
- [`07-compras.md`](./07-compras.md)
- [`08-contabilidad.md`](./08-contabilidad.md)
- [`09-produccion.md`](./09-produccion.md)
- [`10-produccion-serigrafia.md`](./10-produccion-serigrafia.md)

## Cómo usar esta documentación
Cuando se necesite ubicar una fuente de información:
1. identificar primero el módulo funcional,
2. revisar la sección `Dónde buscar primero`,
3. ubicar la página involucrada,
4. seguir a servicios, repositorios, entidades o `DbSet` indicados,
5. confirmar si la consulta se hace por repositorio o directamente con `CrmDbContext`.

## Fuente técnica base común
La referencia central de persistencia del sistema es `MundoVs/Infrastructure/Data/CrmDbContext.cs`, donde están los `DbSet` principales del dominio.

Además, varias pantallas usan:
- `IAppConfigRepository` para parámetros y consecutivos,
- `CustomAuthStateProvider` para sesión, claims y empresa activa,
- `AuthenticationStateProvider` para permisos por capacidad,
- acceso directo a `CrmDbContext` cuando la pantalla arma consultas complejas.
