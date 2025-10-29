# NuGet Package Build Script for Windows PowerShell
# This script builds and packs the Language Server Framework for NuGet

param(
    [string]$Version = "",
    [switch]$SkipTests = $false,
    [switch]$Help = $false
)

if ($Help) {
    Write-Host @"
NuGet Package Build Script

Usage:
    .\pack.ps1 [-Version <version>] [-SkipTests] [-Help]

Parameters:
    -Version <version>  : Specify package version (e.g., 1.0.0)
    -SkipTests         : Skip running tests before packing
    -Help              : Show this help message

Examples:
    .\pack.ps1
    .\pack.ps1 -Version 1.0.0
    .\pack.ps1 -Version 1.0.0 -SkipTests

"@
    exit 0
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Language Server Framework - Pack" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Clean previous artifacts
Write-Host "[1/5] Cleaning previous artifacts..." -ForegroundColor Yellow
if (Test-Path "artifacts") {
    Remove-Item -Recurse -Force "artifacts"
}
New-Item -ItemType Directory -Path "artifacts" | Out-Null
Write-Host "✓ Cleaned" -ForegroundColor Green
Write-Host ""

# Restore dependencies
Write-Host "[2/5] Restoring dependencies..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Restore failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}
Write-Host "✓ Restored" -ForegroundColor Green
Write-Host ""

# Build
Write-Host "[3/5] Building in Release mode..." -ForegroundColor Yellow
dotnet build --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Build failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}
Write-Host "✓ Built" -ForegroundColor Green
Write-Host ""

# Run tests (optional)
if (-not $SkipTests) {
    Write-Host "[4/5] Running tests..." -ForegroundColor Yellow
    dotnet test --configuration Release --no-build --verbosity normal
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Tests failed!" -ForegroundColor Red
        exit $LASTEXITCODE
    }
    Write-Host "✓ Tests passed" -ForegroundColor Green
} else {
    Write-Host "[4/5] Skipping tests..." -ForegroundColor Yellow
}
Write-Host ""

# Pack
Write-Host "[5/5] Creating NuGet package..." -ForegroundColor Yellow
$packArgs = @(
    "pack"
    "LanguageServer.Framework\LanguageServer.Framework.csproj"
    "--configuration", "Release"
    "--no-build"
    "--output", "artifacts"
)

if ($Version) {
    $packArgs += "-p:PackageVersion=$Version"
    Write-Host "Using version: $Version" -ForegroundColor Cyan
}

& dotnet $packArgs
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Pack failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}
Write-Host "✓ Packed" -ForegroundColor Green
Write-Host ""

# List created packages
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Package Created Successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Package location: .\artifacts\" -ForegroundColor Cyan
Get-ChildItem -Path "artifacts" -Filter "*.nupkg" | ForEach-Object {
    Write-Host "  - $($_.Name)" -ForegroundColor White
    Write-Host "    Size: $([math]::Round($_.Length / 1KB, 2)) KB" -ForegroundColor Gray
}
Write-Host ""

# Show next steps
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Test locally: dotnet nuget push .\artifacts\*.nupkg --source local-feed" -ForegroundColor White
Write-Host "  2. Publish to NuGet: dotnet nuget push .\artifacts\*.nupkg --api-key YOUR_KEY --source https://api.nuget.org/v3/index.json" -ForegroundColor White
Write-Host "  3. Or use GitHub Actions for automated publishing" -ForegroundColor White
Write-Host ""
