# Build script for QR IFC Viewer Revit Plugin

param(
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',
    
    [Parameter()]
    [switch]$Clean
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "QR IFC Viewer - Revit Plugin Build" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$projectPath = Join-Path $PSScriptRoot "QrIfcPlugin\QrIfcPlugin.csproj"

if ($Clean) {
    Write-Host "Cleaning..." -ForegroundColor Yellow
    dotnet clean $projectPath -c $Configuration
    Write-Host "Clean completed." -ForegroundColor Green
    Write-Host ""
}

Write-Host "Building configuration: $Configuration" -ForegroundColor Yellow
Write-Host ""

# Restore packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore $projectPath

# Build
Write-Host ""
Write-Host "Building project..." -ForegroundColor Yellow
dotnet build $projectPath -c $Configuration --no-restore

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Build succeeded!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Plugin installed to:" -ForegroundColor Cyan
    Write-Host "$env:APPDATA\Autodesk\Revit\Addins\2024\" -ForegroundColor White
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "1. Start Revit 2024" -ForegroundColor White
    Write-Host "2. Look for 'QR Viewer' tab in the ribbon" -ForegroundColor White
    Write-Host "3. Configure settings before first use" -ForegroundColor White
} else {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "Build failed!" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    exit 1
}
