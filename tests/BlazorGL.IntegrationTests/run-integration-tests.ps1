# BlazorGL Integration Test Runner (PowerShell)
# This script starts the test Blazor app and runs Playwright integration tests

$ErrorActionPreference = "Stop"

Write-Host "╔═══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║      BlazorGL Integration Test Runner                        ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Check if dotnet is installed
try {
    $dotnetVersion = dotnet --version
    Write-Host "→ .NET SDK version: $dotnetVersion" -ForegroundColor Blue
    Write-Host ""
} catch {
    Write-Host "✗ Error: .NET SDK not found" -ForegroundColor Red
    Write-Host "Please install .NET 8.0 SDK from https://dotnet.microsoft.com/download"
    exit 1
}

# Check if Playwright is installed
Write-Host "→ Checking Playwright installation..." -ForegroundColor Blue
if (-not (Test-Path "bin\Debug\net8.0\playwright.ps1")) {
    Write-Host "⚠ Playwright not found. Building project first..." -ForegroundColor Yellow
    dotnet build
    Write-Host ""
    Write-Host "→ Installing Playwright browsers..." -ForegroundColor Yellow
    & "bin\Debug\net8.0\playwright.ps1" install chromium
}
Write-Host ""

# Build the test app
Write-Host "→ Building test Blazor app..." -ForegroundColor Blue
Push-Location TestApp
dotnet build --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Failed to build test app" -ForegroundColor Red
    Pop-Location
    exit 1
}
Write-Host "✓ Test app built successfully" -ForegroundColor Green
Write-Host ""

# Start the test app in the background
Write-Host "→ Starting test app on http://localhost:5000..." -ForegroundColor Blue
$testAppProcess = Start-Process -FilePath "dotnet" -ArgumentList "run --configuration Release --urls http://localhost:5000" -PassThru -WindowStyle Hidden

# Function to cleanup on exit
function Cleanup {
    Write-Host ""
    Write-Host "→ Stopping test app (PID: $($testAppProcess.Id))..." -ForegroundColor Blue
    Stop-Process -Id $testAppProcess.Id -Force -ErrorAction SilentlyContinue
    Write-Host "✓ Cleanup complete" -ForegroundColor Green
}

# Register cleanup
try {
    # Wait for the app to start
    Write-Host "→ Waiting for test app to start..." -ForegroundColor Yellow
    Start-Sleep -Seconds 8

    # Check if the app is running
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000" -UseBasicParsing -TimeoutSec 5
        Write-Host "✓ Test app is running" -ForegroundColor Green
    } catch {
        Write-Host "✗ Test app failed to start" -ForegroundColor Red
        Cleanup
        Pop-Location
        exit 1
    }
    Write-Host ""

    # Go back to integration tests directory
    Pop-Location

    # Run the integration tests
    Write-Host "→ Running integration tests with Playwright..." -ForegroundColor Blue
    Write-Host ""
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    Write-Host ""

    # Run tests with coverage
    dotnet test --configuration Release /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura --logger "console;verbosity=normal"

    $testExitCode = $LASTEXITCODE

    Write-Host ""
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    Write-Host ""

    # Display results
    if ($testExitCode -eq 0) {
        Write-Host "╔═══════════════════════════════════════════════════════════════╗" -ForegroundColor Green
        Write-Host "║                                                               ║" -ForegroundColor Green
        Write-Host "║  ✓ ALL INTEGRATION TESTS PASSED!                             ║" -ForegroundColor Green
        Write-Host "║                                                               ║" -ForegroundColor Green
        Write-Host "║  WebGL-dependent code is now fully tested                    ║" -ForegroundColor Green
        Write-Host "║                                                               ║" -ForegroundColor Green
        Write-Host "╚═══════════════════════════════════════════════════════════════╝" -ForegroundColor Green
        Write-Host ""
        Write-Host "Coverage report: coverage.cobertura.xml" -ForegroundColor Blue
    } else {
        Write-Host "╔═══════════════════════════════════════════════════════════════╗" -ForegroundColor Red
        Write-Host "║                                                               ║" -ForegroundColor Red
        Write-Host "║  ✗ SOME TESTS FAILED                                         ║" -ForegroundColor Red
        Write-Host "║                                                               ║" -ForegroundColor Red
        Write-Host "║  Please review the test output above for details             ║" -ForegroundColor Red
        Write-Host "║                                                               ║" -ForegroundColor Red
        Write-Host "╚═══════════════════════════════════════════════════════════════╝" -ForegroundColor Red
    }

    Cleanup
    exit $testExitCode

} catch {
    Write-Host "✗ Error: $_" -ForegroundColor Red
    Cleanup
    Pop-Location
    exit 1
}
