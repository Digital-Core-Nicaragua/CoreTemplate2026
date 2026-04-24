# rename.ps1
# Script para renombrar CoreTemplate al nombre de tu sistema.
#
# Uso:
#   .\rename.ps1 -SystemName "MiSistema"
#
# Qué hace:
#   1. Renombra todos los namespaces CoreTemplate → MiSistema
#   2. Renombra todos los nombres de proyectos .csproj
#   3. Renombra las carpetas de proyectos
#   4. Actualiza referencias en .sln, Program.cs, appsettings, etc.
#
# Prerequisitos:
#   - PowerShell 5.1+
#   - Ejecutar desde la raíz del repositorio
#   - Hacer commit o backup antes de ejecutar

param(
    [Parameter(Mandatory = $true)]
    [string]$SystemName
)

$ErrorActionPreference = "Stop"
$OldName = "CoreTemplate"
$Root = $PSScriptRoot

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  CoreTemplate → $SystemName" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Validar que no se ejecute en el directorio equivocado
if (-not (Test-Path "$Root\src\Host")) {
    Write-Error "Ejecuta este script desde la raíz del repositorio CoreTemplate."
    exit 1
}

# Validar nombre del sistema
if ($SystemName -notmatch '^[a-zA-Z][a-zA-Z0-9]*$') {
    Write-Error "El nombre del sistema solo puede contener letras y números, y debe empezar con letra. Ej: MiSistema"
    exit 1
}

Write-Host "Sistema: $SystemName" -ForegroundColor Green
Write-Host "Reemplazando '$OldName' por '$SystemName'..." -ForegroundColor Yellow
Write-Host ""

# ─── Paso 1: Reemplazar contenido de archivos ────────────────────────────────

$extensions = @("*.cs", "*.csproj", "*.sln", "*.slnx", "*.json", "*.md", "*.ps1", "*.http", "*.puml", "*.yaml", "*.yml")
$excludeDirs = @("bin", "obj", ".git", "node_modules")

Write-Host "Paso 1/3: Reemplazando contenido de archivos..." -ForegroundColor Yellow

$files = Get-ChildItem -Path $Root -Recurse -Include $extensions | Where-Object {
    $path = $_.FullName
    $exclude = $false
    foreach ($dir in $excludeDirs) {
        if ($path -match "\\$dir\\") { $exclude = $true; break }
    }
    -not $exclude
}

$count = 0
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    if ($content -match $OldName) {
        $newContent = $content -replace $OldName, $SystemName
        Set-Content -Path $file.FullName -Value $newContent -Encoding UTF8 -NoNewline
        $count++
        Write-Host "  ✓ $($file.Name)" -ForegroundColor Gray
    }
}

Write-Host "  → $count archivos actualizados" -ForegroundColor Green
Write-Host ""

# ─── Paso 2: Renombrar archivos ───────────────────────────────────────────────

Write-Host "Paso 2/3: Renombrando archivos..." -ForegroundColor Yellow

$filesToRename = Get-ChildItem -Path $Root -Recurse -File | Where-Object {
    $_.Name -match $OldName -and $_.FullName -notmatch "\\(bin|obj|\.git)\\"
}

foreach ($file in $filesToRename) {
    $newName = $file.Name -replace $OldName, $SystemName
    $newPath = Join-Path $file.DirectoryName $newName
    Rename-Item -Path $file.FullName -NewName $newName
    Write-Host "  ✓ $($file.Name) → $newName" -ForegroundColor Gray
}

Write-Host "  → $($filesToRename.Count) archivos renombrados" -ForegroundColor Green
Write-Host ""

# ─── Paso 3: Renombrar carpetas ───────────────────────────────────────────────

Write-Host "Paso 3/3: Renombrando carpetas..." -ForegroundColor Yellow

# Renombrar de más profundo a menos profundo para evitar conflictos
$dirsToRename = Get-ChildItem -Path $Root -Recurse -Directory | Where-Object {
    $_.Name -match $OldName -and $_.FullName -notmatch "\\(bin|obj|\.git)\\"
} | Sort-Object { $_.FullName.Length } -Descending

foreach ($dir in $dirsToRename) {
    $newName = $dir.Name -replace $OldName, $SystemName
    $newPath = Join-Path $dir.Parent.FullName $newName
    if (-not (Test-Path $newPath)) {
        Rename-Item -Path $dir.FullName -NewName $newName
        Write-Host "  ✓ $($dir.Name) → $newName" -ForegroundColor Gray
    }
}

Write-Host "  → $($dirsToRename.Count) carpetas renombradas" -ForegroundColor Green
Write-Host ""

# ─── Resumen ──────────────────────────────────────────────────────────────────

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  ¡Renombrado completado!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Próximos pasos:" -ForegroundColor Yellow
Write-Host "  1. Revisar appsettings.json — actualizar JwtIssuer, JwtAudience"
Write-Host "  2. Actualizar la cadena de conexión con el nombre de tu BD"
Write-Host "  3. Ejecutar: dotnet build"
Write-Host "  4. Ejecutar migraciones para cada módulo"
Write-Host "  5. dotnet run"
Write-Host ""
Write-Host "Si algo salió mal, restaura desde Git:" -ForegroundColor Red
Write-Host "  git checkout -- ." -ForegroundColor Red
Write-Host ""
