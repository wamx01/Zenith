# ?? MundoVs CRM - Sistema de Gestión para Calzado y Serigrafía

## ?? Descripción

Sistema CRM completo tipo Odoo adaptado específicamente para las industrias de:
- **?? Calzado**: Gestión de hormas, tallas, colecciones, materiales
- **?? Serigrafía**: Diseños, tintas, pantallas, colores Pantone

Desarrollado con:
- **.NET 10** (Preview)
- **Blazor Server** (Interactive)
- **Entity Framework Core 9.0**
- **MariaDB** (MySQL compatible)
- **Pomelo EntityFrameworkCore Provider**

---

## ??? Arquitectura del Proyecto

```
MundoVs/
??? Core/
?   ??? Entities/                 # Entidades del dominio
?   ?   ??? BaseEntity.cs
?   ?   ??? Cliente.cs
?   ?   ??? Contacto.cs
?   ?   ??? Pedido.cs
?   ?   ??? PedidoDetalle.cs
?   ?   ??? Producto.cs
?   ?   ??? Calzado/             # Entidades específicas de calzado
?   ?   ?   ??? ProductoCalzado.cs
?   ?   ?   ??? TallaCalzado.cs
?   ?   ?   ??? Horma.cs
?   ?   ??? Serigrafia/          # Entidades específicas de serigrafía
?   ?       ??? ProductoSerigrafia.cs
?   ?       ??? ColorSerigrafia.cs
?   ?       ??? Tinta.cs
?   ?       ??? Pantalla.cs
?   ?       ??? Diseno.cs
?   ??? Interfaces/              # Interfaces de repositorios
?       ??? IRepository.cs
?       ??? IClienteRepository.cs
?       ??? IProductoRepository.cs
?       ??? IPedidoRepository.cs
??? Infrastructure/
?   ??? Data/
?   ?   ??? CrmDbContext.cs      # Contexto de Entity Framework
?   ??? Repositories/            # Implementación de repositorios
?       ??? Repository.cs
?       ??? ClienteRepository.cs
?       ??? ProductoRepository.cs
?       ??? PedidoRepository.cs
??? Components/
?   ??? Pages/
?   ?   ??? Clientes.razor       # Gestión de clientes
?   ?   ??? Productos.razor      # Catálogo de productos
?   ??? Layout/
?       ??? NavMenu.razor
??? Program.cs                   # Configuración principal
```

---

## ?? Configuración de Base de Datos

### 1. Instalar MariaDB

**Windows:**
```powershell
# Descargar desde: https://mariadb.org/download/
# O usar chocolatey:
choco install mariadb
```

**Linux (Ubuntu/Debian):**
```bash
sudo apt update
sudo apt install mariadb-server
sudo mysql_secure_installation
```

**macOS:**
```bash
brew install mariadb
brew services start mariadb
```

### 2. Crear la Base de Datos

Conectarse a MariaDB:
```bash
mysql -u root -p
```

Ejecutar:
```sql
CREATE DATABASE CrmMundoVs CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER 'crmuser'@'localhost' IDENTIFIED BY 'tu_password_segura';
GRANT ALL PRIVILEGES ON CrmMundoVs.* TO 'crmuser'@'localhost';
FLUSH PRIVILEGES;
EXIT;
```

### 3. Configurar Connection String

Editar `appsettings.json` o `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=CrmMundoVs;User=crmuser;Password=tu_password_segura;CharSet=utf8mb4;"
  }
}
```

### 4. Crear las Migraciones

```powershell
# Desde la raíz del proyecto MundoVs
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Si tienes problemas con dotnet ef, instala la herramienta:
```powershell
dotnet tool install --global dotnet-ef
```

---

## ?? Ejecución del Proyecto

### Modo Desarrollo

```powershell
cd MundoVs
dotnet run
```

El sitio estará disponible en: `https://localhost:5001`

### Modo Producción

```powershell
dotnet publish -c Release -o ./publish
cd publish
dotnet MundoVs.dll
```

---

## ?? Módulos Implementados

### ? Módulos Core (Completados)

#### ?? Gestión de Clientes
- Crear, editar y listar clientes
- Información de contacto completa
- Clasificación por industria (Calzado/Serigrafía/Ambas)
- Múltiples contactos por cliente
- RFC/CIF, direcciones, etc.

#### ?? Catálogo de Productos
- CRUD completo de productos
- Filtros por industria
- Control de stock (mínimo/actual)
- Alertas de stock bajo
- Precios y unidades de medida
- Categorización

#### ?? Gestión de Pedidos
- Estados: Cotización, Confirmado, En Producción, Terminado, Entregado, Cancelado
- Detalles de pedido con productos
- Cálculo de totales e impuestos
- Historial por cliente

---

## ?? Módulos Específicos (Estructura Creada)

### ?? Calzado

#### ProductoCalzado
- Modelos y diseños
- Materiales (cuero, sintético, etc.)
- Tipos de suela
- Colores
- Temporadas (Primavera/Verano, Otoño/Invierno)
- Género (Hombre, Mujer, Niño, Niña, Unisex)

#### TallaCalzado
- Tallas por sistema (MX, US, EU, UK)
- Stock por talla
- Control de inventario detallado

#### Hormas
- Código y nombre
- Tallas disponibles
- Medidas específicas
- Estados (Disponible, En Uso, Mantenimiento, Desgastada)
- Control de uso

### ?? Serigrafía

#### ProductoSerigrafia
- Tipo de impresión (Textil, Papel, Plástico, Madera, Metal, Vidrio)
- Material base
- Número de colores
- Tamaño de impresión
- Tipo de tinta

#### ColorSerigrafia
- Nombre del color
- Código Pantone
- Código Hexadecimal
- Orden de impresión

#### Tintas
- Código y nombre
- Código Pantone y HEX
- Tipos: Base Agua, Plastisol, Discharge, Sublimación, UV
- Cantidad disponible
- Stock mínimo
- Unidades de medida

#### Pantallas
- Código único
- Número de malla
- Dimensiones
- Estados (Nueva, Activa, Desgastada, Recuperable, Inservible)
- Contador de usos
- Diseño asociado

#### Diseños
- Código y nombre
- Cliente asociado
- Archivo de diseño
- Número de colores
- Dimensiones
- Estado de aprobación

---

## ?? Próximos Pasos de Desarrollo

### Pendientes Inmediatos

1. **Crear páginas Blazor para módulos de Calzado:**
   - `/calzado/hormas` - Gestión de hormas
   - `/calzado/tallas` - Control de tallas

2. **Crear páginas Blazor para módulos de Serigrafía:**
   - `/serigrafia/disenos` - Biblioteca de diseños
   - `/serigrafia/tintas` - Inventario de tintas
   - `/serigrafia/pantallas` - Gestión de pantallas

3. **Funcionalidades avanzadas:**
   - Dashboard con KPIs
   - Reportes y gráficas
   - Gestión de órdenes de producción
   - Sistema de facturación
   - Control de proveedores
   - Usuarios y permisos

4. **Mejoras técnicas:**
   - Validaciones con FluentValidation
   - DTOs y AutoMapper
   - Sistema de auditoría
   - Logs estructurados con Serilog
   - Pruebas unitarias
   - API REST (opcional)

---

## ??? Estructura de Base de Datos

### Tablas Principales

- **Clientes** - Información de clientes
- **Contactos** - Contactos asociados a clientes
- **Productos** - Catálogo general
- **Pedidos** - Órdenes de compra/venta
- **PedidoDetalles** - Líneas de pedido

### Tablas Calzado

- **ProductosCalzado** - Detalles específicos de calzado
- **TallasCalzado** - Tallas y stock
- **Hormas** - Inventario de hormas

### Tablas Serigrafía

- **ProductosSerigrafia** - Detalles de productos de serigrafía
- **ColoresSerigrafia** - Colores por producto
- **Tintas** - Inventario de tintas
- **Pantallas** - Pantallas de serigrafía
- **Disenos** - Biblioteca de diseños

---

## ??? Comandos Útiles

### Entity Framework

```powershell
# Crear migración
dotnet ef migrations add NombreMigracion

# Aplicar migraciones
dotnet ef database update

# Revertir a migración específica
dotnet ef database update NombreMigracion

# Eliminar última migración
dotnet ef migrations remove

# Ver SQL de una migración
dotnet ef migrations script

# Recrear base de datos (¡CUIDADO! Elimina todos los datos)
dotnet ef database drop
dotnet ef database update
```

### Restaurar Paquetes

```powershell
dotnet restore
```

### Limpiar y Compilar

```powershell
dotnet clean
dotnet build
```

---

## ?? Notas Importantes

1. **Versiones:**
   - .NET 10 está en preview, para producción considera usar .NET 8 LTS
   - Entity Framework Core 9.0 es la última versión estable compatible con Pomelo

2. **MariaDB vs MySQL:**
   - El proyecto usa MariaDB pero es compatible con MySQL
   - Pomelo soporta ambos motores

3. **Seguridad:**
   - Cambiar las contraseñas por defecto
   - En producción, usar variables de entorno
   - Implementar autenticación y autorización

4. **Rendimiento:**
   - Habilitar compresión de respuestas
   - Usar caché donde sea apropiado
   - Optimizar consultas con índices

---

## ?? Recursos

- [Documentación .NET](https://docs.microsoft.com/dotnet)
- [Blazor](https://docs.microsoft.com/aspnet/core/blazor)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [Pomelo EF Core MySQL](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql)
- [MariaDB](https://mariadb.org/documentation/)

---

## ????? Desarrollo

Para contribuir al proyecto o extenderlo:

1. Las entidades están en `Core/Entities/`
2. Los repositorios en `Infrastructure/Repositories/`
3. Las páginas Blazor en `Components/Pages/`
4. Los componentes compartidos en `Components/Shared/`

---

## ?? Licencia

Proyecto desarrollado para MundoVs.

---

**¡Listo para empezar! ??**

Ejecuta las migraciones y comienza a usar tu CRM personalizado para la industria del calzado y serigrafía.
