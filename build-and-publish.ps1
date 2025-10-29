# Quick Build and Publish Script
# This is a convenience script that combines building, testing, and publishing

param(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    
    [Parameter(Mandatory=$false)]
    [string]$ApiKey = "",
    
    [switch]$SkipTests = $false,
    [switch]$SkipPublish = $false,
    [switch]$CreateTag = $false
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Quick Build and Publish - v$Version" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Pack
Write-Host "Step 1: Building and packing..." -ForegroundColor Yellow
$packParams = @{
    Version = $Version
    SkipTests = $SkipTests
}
& .\pack.ps1 @packParams

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Pack failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Step 2: Publish (optional)
if (-not $SkipPublish) {
    Write-Host ""
    Write-Host "Step 2: Publishing to NuGet..." -ForegroundColor Yellow
    
    if (-not $ApiKey) {
        $ApiKey = $env:NUGET_API_KEY
    }
    
    if (-not $ApiKey) {
        Write-Host "⚠ Warning: No API key provided, skipping publish" -ForegroundColor Yellow
        Write-Host "  Use -ApiKey parameter or set NUGET_API_KEY environment variable" -ForegroundColor Gray
    } else {
        & .\publish.ps1 -ApiKey $ApiKey
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "✗ Publish failed!" -ForegroundColor Red
            exit $LASTEXITCODE
        }
    }
} else {
    Write-Host ""
    Write-Host "Skipping publish (use -SkipPublish:`$false to publish)" -ForegroundColor Gray
}

# Step 3: Create Git Tag (optional)
if ($CreateTag) {
    Write-Host ""
    Write-Host "Step 3: Creating Git tag..." -ForegroundColor Yellow
    
    $tagName = "v$Version"
    git tag $tagName
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Tag '$tagName' created" -ForegroundColor Green
        Write-Host "  Push with: git push origin $tagName" -ForegroundColor Gray
    } else {
        Write-Host "⚠ Warning: Failed to create tag" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  All Done! ✓" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if (-not $SkipPublish -and $ApiKey) {
    Write-Host "Package published successfully!" -ForegroundColor Green
    Write-Host "View at: https://www.nuget.org/packages/EmmyLua.LanguageServer.Framework/" -ForegroundColor Cyan
} else {
    Write-Host "Package created in: .\artifacts\" -ForegroundColor Green
    Write-Host "To publish: .\publish.ps1 -ApiKey YOUR_KEY" -ForegroundColor Cyan
}
Write-Host ""
