# ?? Inicio Rápido - MundoVs CRM

## ? Configuración en 5 Minutos

### 1?? Prerequisitos

Asegúrate de tener instalado:
- ? .NET 10 SDK
- ? MariaDB o MySQL
- ? Visual Studio 2022+ o VS Code

### 2?? Configurar Base de Datos

**Opción A - Con script SQL completo:**

```bash
# Crear base de datos y usuario
mysql -u root -p

# En el prompt de MySQL:
CREATE DATABASE CrmMundoVs CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER 'crmuser'@'localhost' IDENTIFIED BY 'MiPassword123!';
GRANT ALL PRIVILEGES ON CrmMundoVs.* TO 'crmuser'@'localhost';
FLUSH PRIVILEGES;
EXIT;
```

**Opción B - Usar conexión root temporalmente:**

Edita `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=CrmMundoVs_Dev;User=root;Password=tu_password;CharSet=utf8mb4;"
  }
}
```

### 3?? Aplicar Migraciones

```powershell
# Instalar herramientas de EF Core (si no las tienes)
dotnet tool install --global dotnet-ef

# Desde la carpeta MundoVs/
cd MundoVs

# Crear y aplicar migración inicial
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4?? Ejecutar la Aplicación

```powershell
# Modo desarrollo con hot reload
dotnet watch run

# O simplemente
dotnet run
```

?? **¡Listo!** Abre tu navegador en `https://localhost:5001`

---

## ?? Datos de Prueba (Opcional)

```bash
# Cargar datos de ejemplo
mysql -u root -p CrmMundoVs < database-seed.sql
```

---

## ?? Primeros Pasos en la Aplicación

1. **Ir a Clientes** (`/clientes`)
   - Crea tu primer cliente
   - Asígnale una industria (Calzado/Serigrafía)

2. **Ir a Productos** (`/productos`)
   - Agrega productos de tu catálogo
   - Configura precios y stock

3. **Explorar el menú lateral**
   - Módulos de Calzado
   - Módulos de Serigrafía

---

## ?? Solución de Problemas Comunes

### Error: "Connection refused" o "Unable to connect"

```powershell
# Verificar que MariaDB está corriendo
# Windows:
Get-Service -Name "MariaDB" 

# Si no está activo:
Start-Service -Name "MariaDB"

# Linux/Mac:
sudo systemctl status mariadb
sudo systemctl start mariadb
```

### Error: "dotnet-ef command not found"

```powershell
dotnet tool install --global dotnet-ef
# Reiniciar la terminal después
```

### Error: "Access denied for user"

Verifica tu connection string en `appsettings.json`:
- Usuario correcto
- Password correcto
- Base de datos existe
- Permisos otorgados

### Error de compilación con Pomelo

```powershell
# Limpiar y restaurar
dotnet clean
dotnet restore
dotnet build
```

---

## ?? Scripts de PowerShell Útiles

```powershell
# Cargar funciones útiles
. .\setup-scripts.ps1

# Ver comandos disponibles
Show-Help

# Iniciar desarrollo
Start-Development

# Crear nueva migración
New-Migration "AgregarNuevoModulo"

# Resetear base de datos (?? elimina datos)
Reset-Database
```

---

## ?? Estructura del Proyecto

```
MundoVs/
??? Core/                    # Entidades y lógica de negocio
?   ??? Entities/           # Modelos de datos
?   ??? Interfaces/         # Contratos de repositorios
??? Infrastructure/          # Acceso a datos
?   ??? Data/               # DbContext
?   ??? Repositories/       # Implementaciones
??? Components/             # UI Blazor
?   ??? Pages/             # Páginas de la app
?   ??? Layout/            # Componentes de diseño
??? Program.cs             # Configuración principal
```

---

## ?? Próximos Pasos

Una vez que tengas el sistema funcionando:

1. **Personaliza el Connection String** en producción
2. **Implementa las páginas faltantes:**
   - `/pedidos` - Gestión de pedidos
   - `/calzado/hormas` - Gestión de hormas
   - `/serigrafia/tintas` - Inventario de tintas
   - `/serigrafia/pantallas` - Gestión de pantallas
   - `/serigrafia/disenos` - Biblioteca de diseños

3. **Agrega funcionalidades:**
   - Dashboard con métricas
   - Sistema de reportes
   - Gestión de usuarios
   - Módulo de facturación

---

## ?? Recursos Adicionales

- **README completo**: Ver `README.md` para documentación detallada
- **Datos de prueba**: `database-seed.sql`
- **Scripts de ayuda**: `setup-scripts.ps1`

---

## ?? ¿Necesitas Ayuda?

1. Revisa el README.md completo
2. Verifica los logs en la consola
3. Revisa la conexión a la base de datos con `Get-DatabaseInfo`

---

**¡Feliz desarrollo! ??**

Sistema desarrollado para MundoVs - CRM para Calzado y Serigrafía
