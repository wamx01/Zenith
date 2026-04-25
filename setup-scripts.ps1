# ==========================================
# Scripts de PowerShell para MundoVs CRM
# ==========================================

# INSTRUCCIONES: Ejecutar cada comando según sea necesario

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "  MundoVs CRM - Scripts de Ayuda" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

# ==========================================
# 1. VERIFICAR INSTALACIONES
# ==========================================

function Test-Prerequisites {
    Write-Host "Verificando prerequisitos..." -ForegroundColor Yellow
    
    # Verificar .NET
    Write-Host "`nVerificando .NET SDK..." -ForegroundColor Gray
    dotnet --version
    
    # Verificar Entity Framework Tools
    Write-Host "`nVerificando dotnet-ef..." -ForegroundColor Gray
    dotnet ef --version
    
    # Si dotnet-ef no está instalado
    if ($LASTEXITCODE -ne 0) {
        Write-Host "`ndotnet-ef no encontrado. Instalando..." -ForegroundColor Red
        dotnet tool install --global dotnet-ef
    }
    
    Write-Host "`n? Prerequisitos verificados" -ForegroundColor Green
}

# ==========================================
# 2. CONFIGURACIÓN DE BASE DE DATOS
# ==========================================

function Initialize-Database {
    Write-Host "`nConfigurando base de datos..." -ForegroundColor Yellow
    
    # Crear primera migración
    Write-Host "`nCreando migración inicial..." -ForegroundColor Gray
    dotnet ef migrations add InitialCreate
    
    # Aplicar migración
    Write-Host "`nAplicando migración a la base de datos..." -ForegroundColor Gray
    dotnet ef database update
    
    Write-Host "`n? Base de datos configurada" -ForegroundColor Green
    Write-Host "`nPara insertar datos de prueba, ejecuta:" -ForegroundColor Cyan
    Write-Host "mysql -u root -p CrmMundoVs < database-seed.sql" -ForegroundColor White
}

# ==========================================
# 3. GESTIÓN DE MIGRACIONES
# ==========================================

function New-Migration {
    param([string]$Name)
    
    if ([string]::IsNullOrEmpty($Name)) {
        $Name = Read-Host "Nombre de la migración"
    }
    
    Write-Host "`nCreando migración: $Name..." -ForegroundColor Yellow
    dotnet ef migrations add $Name
    
    Write-Host "`n? Migración creada" -ForegroundColor Green
    Write-Host "Para aplicarla, ejecuta: Update-Database" -ForegroundColor Cyan
}

function Update-Database {
    Write-Host "`nActualizando base de datos..." -ForegroundColor Yellow
    dotnet ef database update
    Write-Host "`n? Base de datos actualizada" -ForegroundColor Green
}

function Remove-LastMigration {
    Write-Host "`nEliminando última migración..." -ForegroundColor Yellow
    dotnet ef migrations remove
    Write-Host "`n? Migración eliminada" -ForegroundColor Green
}

function Get-Migrations {
    Write-Host "`nListando migraciones..." -ForegroundColor Yellow
    dotnet ef migrations list
}

function Reset-Database {
    Write-Host "`n??  ADVERTENCIA: Esto eliminará TODOS los datos ??" -ForegroundColor Red
    $confirm = Read-Host "¿Estás seguro? (si/no)"
    
    if ($confirm -eq "si") {
        Write-Host "`nEliminando base de datos..." -ForegroundColor Yellow
        dotnet ef database drop --force
        
        Write-Host "`nRecreando base de datos..." -ForegroundColor Yellow
        dotnet ef database update
        
        Write-Host "`n? Base de datos reseteada" -ForegroundColor Green
    } else {
        Write-Host "`nOperación cancelada" -ForegroundColor Gray
    }
}

# ==========================================
# 4. DESARROLLO
# ==========================================

function Start-Development {
    Write-Host "`nIniciando aplicación en modo desarrollo..." -ForegroundColor Yellow
    Write-Host "Presiona Ctrl+C para detener`n" -ForegroundColor Gray
    
    dotnet watch run
}

function Build-Project {
    param([switch]$Release)
    
    if ($Release) {
        Write-Host "`nCompilando en modo Release..." -ForegroundColor Yellow
        dotnet build -c Release
    } else {
        Write-Host "`nCompilando en modo Debug..." -ForegroundColor Yellow
        dotnet build
    }
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n? Compilación exitosa" -ForegroundColor Green
    } else {
        Write-Host "`n? Error en compilación" -ForegroundColor Red
    }
}

function Clean-Project {
    Write-Host "`nLimpiando proyecto..." -ForegroundColor Yellow
    
    dotnet clean
    
    # Eliminar carpetas bin y obj
    Get-ChildItem -Path . -Include bin,obj -Recurse -Directory | Remove-Item -Recurse -Force
    
    Write-Host "`n? Proyecto limpiado" -ForegroundColor Green
}

function Restore-Packages {
    Write-Host "`nRestaurando paquetes NuGet..." -ForegroundColor Yellow
    dotnet restore
    Write-Host "`n? Paquetes restaurados" -ForegroundColor Green
}

# ==========================================
# 5. PUBLICACIÓN
# ==========================================

function Publish-Application {
    param(
        [string]$OutputPath = "./publish",
        [switch]$SelfContained
    )
    
    Write-Host "`nPublicando aplicación..." -ForegroundColor Yellow
    
    if ($SelfContained) {
        dotnet publish -c Release -o $OutputPath --self-contained true
    } else {
        dotnet publish -c Release -o $OutputPath
    }
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n? Aplicación publicada en: $OutputPath" -ForegroundColor Green
    } else {
        Write-Host "`n? Error al publicar" -ForegroundColor Red
    }
}

# ==========================================
# 6. UTILIDADES
# ==========================================

function Get-DatabaseInfo {
    Write-Host "`nInformación de la base de datos:" -ForegroundColor Yellow
    
    $appSettings = Get-Content -Path "appsettings.json" | ConvertFrom-Json
    $connectionString = $appSettings.ConnectionStrings.DefaultConnection
    
    Write-Host "`nConnection String:" -ForegroundColor Gray
    Write-Host $connectionString -ForegroundColor White
    
    # Extraer información
    if ($connectionString -match "Server=([^;]+)") { 
        Write-Host "`nServidor: $($matches[1])" -ForegroundColor Cyan 
    }
    if ($connectionString -match "Database=([^;]+)") { 
        Write-Host "Base de Datos: $($matches[1])" -ForegroundColor Cyan 
    }
    if ($connectionString -match "User=([^;]+)") { 
        Write-Host "Usuario: $($matches[1])" -ForegroundColor Cyan 
    }
}

function Show-Help {
    Write-Host "`n==================================" -ForegroundColor Cyan
    Write-Host "  Comandos Disponibles" -ForegroundColor Cyan
    Write-Host "==================================" -ForegroundColor Cyan
    Write-Host ""
    
    Write-Host "CONFIGURACIÓN INICIAL:" -ForegroundColor Yellow
    Write-Host "  Test-Prerequisites       - Verificar instalaciones" -ForegroundColor White
    Write-Host "  Initialize-Database      - Crear base de datos inicial" -ForegroundColor White
    Write-Host ""
    
    Write-Host "MIGRACIONES:" -ForegroundColor Yellow
    Write-Host "  New-Migration [nombre]   - Crear nueva migración" -ForegroundColor White
    Write-Host "  Update-Database          - Aplicar migraciones pendientes" -ForegroundColor White
    Write-Host "  Remove-LastMigration     - Eliminar última migración" -ForegroundColor White
    Write-Host "  Get-Migrations           - Listar todas las migraciones" -ForegroundColor White
    Write-Host "  Reset-Database           - ??  Eliminar y recrear BD" -ForegroundColor White
    Write-Host ""
    
    Write-Host "DESARROLLO:" -ForegroundColor Yellow
    Write-Host "  Start-Development        - Iniciar con hot reload" -ForegroundColor White
    Write-Host "  Build-Project [-Release] - Compilar proyecto" -ForegroundColor White
    Write-Host "  Clean-Project            - Limpiar archivos compilados" -ForegroundColor White
    Write-Host "  Restore-Packages         - Restaurar paquetes NuGet" -ForegroundColor White
    Write-Host ""
    
    Write-Host "PUBLICACIÓN:" -ForegroundColor Yellow
    Write-Host "  Publish-Application [-OutputPath] [-SelfContained]" -ForegroundColor White
    Write-Host ""
    
    Write-Host "UTILIDADES:" -ForegroundColor Yellow
    Write-Host "  Get-DatabaseInfo         - Mostrar info de conexión" -ForegroundColor White
    Write-Host "  Show-Help                - Mostrar esta ayuda" -ForegroundColor White
    Write-Host ""
    
    Write-Host "EJEMPLOS:" -ForegroundColor Yellow
    Write-Host "  New-Migration 'AgregarTablaVentas'" -ForegroundColor Gray
    Write-Host "  Build-Project -Release" -ForegroundColor Gray
    Write-Host "  Publish-Application -OutputPath 'C:\inetpub\wwwroot\crm'" -ForegroundColor Gray
    Write-Host ""
}

# ==========================================
# INICIO AUTOMÁTICO
# ==========================================

Write-Host "Para ver todos los comandos disponibles, ejecuta:" -ForegroundColor Cyan
Write-Host "Show-Help" -ForegroundColor White
Write-Host ""

# Si es la primera ejecución, preguntar si desea inicializar
$migrationsPath = ".\Migrations"
if (-not (Test-Path $migrationsPath)) {
    Write-Host "??  No se detectaron migraciones existentes" -ForegroundColor Yellow
    $init = Read-Host "¿Deseas inicializar la base de datos ahora? (si/no)"
    
    if ($init -eq "si") {
        Test-Prerequisites
        Initialize-Database
    }
}
