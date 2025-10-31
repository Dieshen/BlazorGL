# BlazorGL Test Runner Script (PowerShell)
# Runs all tests with code coverage

Write-Host "================================" -ForegroundColor Cyan
Write-Host "BlazorGL Test Suite" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Run tests with coverage
Write-Host "Running tests with coverage..." -ForegroundColor Yellow
dotnet test tests/BlazorGL.Tests/BlazorGL.Tests.csproj `
  --configuration Release `
  --logger "console;verbosity=normal" `
  /p:CollectCoverage=true `
  /p:CoverletOutputFormat=opencover `
  /p:CoverletOutput=./TestResults/coverage.opencover.xml

if ($LASTEXITCODE -eq 0) {
  Write-Host ""
  Write-Host "✓ All tests passed!" -ForegroundColor Green
} else {
  Write-Host ""
  Write-Host "✗ Some tests failed!" -ForegroundColor Red
  exit 1
}

# Generate HTML coverage report
Write-Host ""
Write-Host "Generating HTML coverage report..." -ForegroundColor Yellow

if (Get-Command reportgenerator -ErrorAction SilentlyContinue) {
  reportgenerator `
    -reports:tests/BlazorGL.Tests/TestResults/coverage.opencover.xml `
    -targetdir:tests/BlazorGL.Tests/TestResults/CoverageReport `
    -reporttypes:Html

  Write-Host "Coverage report generated: tests/BlazorGL.Tests/TestResults/CoverageReport/index.html" -ForegroundColor Green
} else {
  Write-Host "Install reportgenerator for HTML reports:" -ForegroundColor Yellow
  Write-Host "  dotnet tool install -g dotnet-reportgenerator-globaltool" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Testing Complete" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
