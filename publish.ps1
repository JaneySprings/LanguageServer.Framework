# NuGet Package Publishing Script
# This script publishes the Language Server Framework to NuGet.org

param(
    [string]$ApiKey = "",
    [string]$Source = "https://api.nuget.org/v3/index.json",
    [switch]$DryRun = $false,
    [switch]$Help = $false
)

if ($Help) {
    Write-Host @"
NuGet Package Publishing Script

Usage:
    .\publish.ps1 -ApiKey <key> [-Source <url>] [-DryRun] [-Help]

Parameters:
    -ApiKey <key>   : Your NuGet API key (required)
    -Source <url>   : NuGet source URL (default: nuget.org)
    -DryRun         : Validate package without publishing
    -Help           : Show this help message

Examples:
    .\publish.ps1 -ApiKey YOUR_API_KEY
    .\publish.ps1 -ApiKey YOUR_API_KEY -DryRun
    .\publish.ps1 -ApiKey YOUR_API_KEY -Source https://api.nuget.org/v3/index.json

Environment Variables:
    NUGET_API_KEY   : Can be used instead of -ApiKey parameter

"@
    exit 0
}

# Check for API key
if (-not $ApiKey) {
    $ApiKey = $env:NUGET_API_KEY
}

if (-not $ApiKey) {
    Write-Host "✗ Error: NuGet API key is required!" -ForegroundColor Red
    Write-Host "  Use: .\publish.ps1 -ApiKey YOUR_KEY" -ForegroundColor Yellow
    Write-Host "  Or set environment variable: `$env:NUGET_API_KEY = 'YOUR_KEY'" -ForegroundColor Yellow
    exit 1
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  NuGet Package Publishing" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if packages exist
if (-not (Test-Path "artifacts")) {
    Write-Host "✗ Error: No artifacts folder found!" -ForegroundColor Red
    Write-Host "  Please run .\pack.ps1 first" -ForegroundColor Yellow
    exit 1
}

$packages = Get-ChildItem -Path "artifacts" -Filter "*.nupkg" | Where-Object { $_.Name -notlike "*.symbols.nupkg" }
if ($packages.Count -eq 0) {
    Write-Host "✗ Error: No packages found in artifacts folder!" -ForegroundColor Red
    Write-Host "  Please run .\pack.ps1 first" -ForegroundColor Yellow
    exit 1
}

Write-Host "Found packages:" -ForegroundColor Cyan
foreach ($package in $packages) {
    Write-Host "  - $($package.Name)" -ForegroundColor White
}
Write-Host ""

if ($DryRun) {
    Write-Host "DRY RUN MODE - No packages will be published" -ForegroundColor Yellow
    Write-Host ""
}

# Publish each package
foreach ($package in $packages) {
    Write-Host "Publishing: $($package.Name)" -ForegroundColor Yellow
    
    if ($DryRun) {
        Write-Host "  [DRY RUN] Would publish to: $Source" -ForegroundColor Gray
        continue
    }
    
    $pushArgs = @(
        "nuget", "push"
        $package.FullName
        "--api-key", $ApiKey
        "--source", $Source
        "--skip-duplicate"
    )
    
    & dotnet $pushArgs
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Published successfully" -ForegroundColor Green
    } else {
        Write-Host "✗ Publishing failed" -ForegroundColor Red
        exit $LASTEXITCODE
    }
    Write-Host ""
}

# Check for symbol packages
$symbolPackages = Get-ChildItem -Path "artifacts" -Filter "*.snupkg"
if ($symbolPackages.Count -gt 0 -and -not $DryRun) {
    Write-Host "Publishing symbol packages..." -ForegroundColor Cyan
    foreach ($symbolPkg in $symbolPackages) {
        Write-Host "  - $($symbolPkg.Name)" -ForegroundColor White
        
        $pushArgs = @(
            "nuget", "push"
            $symbolPkg.FullName
            "--api-key", $ApiKey
            "--source", $Source
            "--skip-duplicate"
        )
        
        & dotnet $pushArgs
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✓ Symbol package published" -ForegroundColor Green
        }
    }
    Write-Host ""
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Publishing Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Your package is now available on NuGet!" -ForegroundColor Green
Write-Host "It may take a few minutes to appear in search results." -ForegroundColor Yellow
Write-Host ""
Write-Host "View your package at:" -ForegroundColor Cyan
Write-Host "  https://www.nuget.org/packages/EmmyLua.LanguageServer.Framework/" -ForegroundColor White
Write-Host ""
