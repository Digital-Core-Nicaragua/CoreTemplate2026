<#
.SYNOPSIS
    Renombra CoreTemplate por el nombre de tu nuevo sistema.

.DESCRIPTION
    Reemplaza todas las ocurrencias de "CoreTemplate" en:
    - Namespaces y código fuente (.cs, .csproj, .sln, .json, .md)
    - Nombres de archivos
    - Nombres de carpetas
    - Archivo de solución .sln

    Ejecutar desde la raíz del proyecto clonado.

.PARAMETER SystemName
    Nombre del nuevo sistema. Ejemplo: "MiERP", "VentasApp", "GestionHR"
    Solo letras, números y puntos. Sin espacios.

.EXAMPLE
    .\rename.ps1 -SystemName "MiERP"
    .\rename.ps1 -SystemName "VentasApp"

.NOTES
    - Hacer un commit o backup antes de ejecutar.
    - El script es idempotente: si falla a mitad, se puede volver a ejecutar.
    - Requiere PowerShell 5.1 o superior.
#>

param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[a-zA-Z][a-zA-Z0-9.]*$')]
    [string]$SystemName
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ─── Validaciones ─────────────────────────────────────────────────────────────

if ($SystemName -eq "CoreTemplate") {
    Write-Error "El nombre del sistema no puede ser 'CoreTemplate'."
    exit 1
}

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = $scriptDir

if (-not (Test-Path (Join-Path $rootDir "CoreTemplate.sln"))) {
    Write-Error "No se encontró CoreTemplate.sln. Ejecuta el script desde la raíz del proyecto."
    exit 1
}

Write-Host ""
Write-Host "╔══════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║         CoreTemplate → $SystemName" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""
Write-Host "Directorio raíz: $rootDir" -ForegroundColor Gray
Write-Host ""

# Confirmar antes de proceder
$confirm = Read-Host "¿Confirmas el renombrado de 'CoreTemplate' a '$SystemName'? (s/N)"
if ($confirm -notmatch '^[sS]$') {
    Write-Host "Operación cancelada." -ForegroundColor Yellow
    exit 0
}

Write-Host ""

# ─── Extensiones de archivos a procesar ──────────────────────────────────────

$extensionesContenido = @(
    "*.cs",
    "*.csproj",
    "*.sln",
    "*.slnx",
    "*.json",
    "*.md",
    "*.xml",
    "*.yaml",
    "*.yml",
    "*.props",
    "*.targets",
    "*.puml"
)

# Carpetas a excluir del procesamiento
$carpetasExcluidas = @(
    "bin",
    "obj",
    ".git",
    ".vs",
    "node_modules"
)

# ─── Función: reemplazar contenido en archivos ────────────────────────────────

function Replace-ContentInFiles {
    param(
        [string]$Directory,
        [string]$OldValue,
        [string]$NewValue
    )

    $archivosModificados = 0

    foreach ($extension in $extensionesContenido) {
        $archivos = Get-ChildItem -Path $Directory -Filter $extension -Recurse -File |
            Where-Object {
                $excluir = $false
                foreach ($carpeta in $carpetasExcluidas) {
                    if ($_.FullName -match [regex]::Escape("\$carpeta\")) {
                        $excluir = $true
                        break
                    }
                }
                -not $excluir
            }

        foreach ($archivo in $archivos) {
            $contenido = Get-Content -Path $archivo.FullName -Raw -Encoding UTF8
            if ($contenido -and $contenido.Contains($OldValue)) {
                $nuevoContenido = $contenido.Replace($OldValue, $NewValue)
                Set-Content -Path $archivo.FullName -Value $nuevoContenido -Encoding UTF8 -NoNewline
                $archivosModificados++
                Write-Host "  [contenido] $($archivo.FullName.Replace($Directory, '.'))" -ForegroundColor DarkGray
            }
        }
    }

    return $archivosModificados
}

# ─── Función: renombrar archivos ──────────────────────────────────────────────

function Rename-Files {
    param(
        [string]$Directory,
        [string]$OldValue,
        [string]$NewValue
    )

    $archivosRenombrados = 0

    # Procesar de más profundo a menos profundo para evitar conflictos
    $archivos = Get-ChildItem -Path $Directory -Recurse -File |
        Where-Object { $_.Name.Contains($OldValue) } |
        Where-Object {
            $excluir = $false
            foreach ($carpeta in $carpetasExcluidas) {
                if ($_.FullName -match [regex]::Escape("\$carpeta\")) {
                    $excluir = $true
                    break
                }
            }
            -not $excluir
        } |
        Sort-Object { $_.FullName.Length } -Descending

    foreach ($archivo in $archivos) {
        $nuevoNombre = $archivo.Name.Replace($OldValue, $NewValue)
        $nuevaRuta = Join-Path $archivo.DirectoryName $nuevoNombre
        Rename-Item -Path $archivo.FullName -NewName $nuevoNombre
        $archivosRenombrados++
        Write-Host "  [archivo]   $($archivo.Name) → $nuevoNombre" -ForegroundColor DarkGray
    }

    return $archivosRenombrados
}

# ─── Función: renombrar carpetas ──────────────────────────────────────────────

function Rename-Folders {
    param(
        [string]$Directory,
        [string]$OldValue,
        [string]$NewValue
    )

    $carpetasRenombradas = 0

    # Procesar de más profundo a menos profundo
    $carpetas = Get-ChildItem -Path $Directory -Recurse -Directory |
        Where-Object { $_.Name.Contains($OldValue) } |
        Where-Object {
            $excluir = $false
            foreach ($carpeta in $carpetasExcluidas) {
                if ($_.Name -eq $carpeta -or $_.FullName -match [regex]::Escape("\$carpeta\")) {
                    $excluir = $true
                    break
                }
            }
            -not $excluir
        } |
        Sort-Object { $_.FullName.Length } -Descending

    foreach ($carpeta in $carpetas) {
        $nuevoNombre = $carpeta.Name.Replace($OldValue, $NewValue)
        $nuevaRuta = Join-Path $carpeta.Parent.FullName $nuevoNombre

        if (-not (Test-Path $nuevaRuta)) {
            Rename-Item -Path $carpeta.FullName -NewName $nuevoNombre
            $carpetasRenombradas++
            Write-Host "  [carpeta]   $($carpeta.Name) → $nuevoNombre" -ForegroundColor DarkGray
        }
    }

    return $carpetasRenombradas
}

# ─── Paso 1: Reemplazar contenido en archivos ─────────────────────────────────

Write-Host "Paso 1/3 — Reemplazando contenido en archivos..." -ForegroundColor Yellow
$totalContenido = Replace-ContentInFiles -Directory $rootDir -OldValue "CoreTemplate" -NewValue $SystemName
Write-Host "  → $totalContenido archivos modificados." -ForegroundColor Green
Write-Host ""

# ─── Paso 2: Renombrar archivos ───────────────────────────────────────────────

Write-Host "Paso 2/3 — Renombrando archivos..." -ForegroundColor Yellow
$totalArchivos = Rename-Files -Directory $rootDir -OldValue "CoreTemplate" -NewValue $SystemName
Write-Host "  → $totalArchivos archivos renombrados." -ForegroundColor Green
Write-Host ""

# ─── Paso 3: Renombrar carpetas ───────────────────────────────────────────────

Write-Host "Paso 3/3 — Renombrando carpetas..." -ForegroundColor Yellow
$totalCarpetas = Rename-Folders -Directory $rootDir -OldValue "CoreTemplate" -NewValue $SystemName
Write-Host "  → $totalCarpetas carpetas renombradas." -ForegroundColor Green
Write-Host ""

# ─── Resumen ──────────────────────────────────────────────────────────────────

Write-Host "╔══════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║  ✅  Renombrado completado exitosamente               ║" -ForegroundColor Green
Write-Host "╚══════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""
Write-Host "  Archivos con contenido modificado : $totalContenido" -ForegroundColor White
Write-Host "  Archivos renombrados              : $totalArchivos" -ForegroundColor White
Write-Host "  Carpetas renombradas              : $totalCarpetas" -ForegroundColor White
Write-Host ""
Write-Host "Próximos pasos:" -ForegroundColor Cyan
Write-Host "  1. Abre $SystemName.sln en Visual Studio" -ForegroundColor White
Write-Host "  2. Configura la cadena de conexión en appsettings.json" -ForegroundColor White
Write-Host "  3. Ejecuta: dotnet ef database update (para cada módulo)" -ForegroundColor White
Write-Host "  4. Ejecuta: dotnet run" -ForegroundColor White
Write-Host ""
