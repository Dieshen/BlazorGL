#!/bin/bash

# BlazorGL Test Runner Script
# Runs all tests with code coverage

echo "================================"
echo "BlazorGL Test Suite"
echo "================================"
echo ""

# Run tests with coverage
echo "Running tests with coverage..."
dotnet test tests/BlazorGL.Tests/BlazorGL.Tests.csproj \
  --configuration Release \
  --logger "console;verbosity=normal" \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=opencover \
  /p:CoverletOutput=./TestResults/coverage.opencover.xml

# Check if tests passed
if [ $? -eq 0 ]; then
  echo ""
  echo "✓ All tests passed!"
else
  echo ""
  echo "✗ Some tests failed!"
  exit 1
fi

# Generate HTML coverage report (requires reportgenerator)
echo ""
echo "Generating HTML coverage report..."
if command -v reportgenerator &> /dev/null; then
  reportgenerator \
    -reports:tests/BlazorGL.Tests/TestResults/coverage.opencover.xml \
    -targetdir:tests/BlazorGL.Tests/TestResults/CoverageReport \
    -reporttypes:Html

  echo "Coverage report generated: tests/BlazorGL.Tests/TestResults/CoverageReport/index.html"
else
  echo "Install reportgenerator for HTML reports:"
  echo "  dotnet tool install -g dotnet-reportgenerator-globaltool"
fi

echo ""
echo "================================"
echo "Testing Complete"
echo "================================"
