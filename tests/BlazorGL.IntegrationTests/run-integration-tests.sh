#!/bin/bash

# BlazorGL Integration Test Runner
# This script starts the test Blazor app and runs Playwright integration tests

set -e  # Exit on error

echo "╔═══════════════════════════════════════════════════════════════╗"
echo "║      BlazorGL Integration Test Runner                        ║"
echo "╚═══════════════════════════════════════════════════════════════╝"
echo ""

# Colors
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}✗ Error: .NET SDK not found${NC}"
    echo "Please install .NET 8.0 SDK from https://dotnet.microsoft.com/download"
    exit 1
fi

echo -e "${BLUE}→ .NET SDK version:${NC}"
dotnet --version
echo ""

# Check if Playwright is installed
echo -e "${BLUE}→ Checking Playwright installation...${NC}"
if [ ! -f "bin/Debug/net8.0/playwright.ps1" ]; then
    echo -e "${YELLOW}⚠ Playwright not found. Building project first...${NC}"
    dotnet build
    echo ""
    echo -e "${YELLOW}→ Installing Playwright browsers...${NC}"
    pwsh bin/Debug/net8.0/playwright.ps1 install chromium
fi
echo ""

# Build the test app
echo -e "${BLUE}→ Building test Blazor app...${NC}"
cd TestApp
dotnet build --configuration Release
if [ $? -ne 0 ]; then
    echo -e "${RED}✗ Failed to build test app${NC}"
    exit 1
fi
echo -e "${GREEN}✓ Test app built successfully${NC}"
echo ""

# Start the test app in the background
echo -e "${BLUE}→ Starting test app on http://localhost:5000...${NC}"
dotnet run --configuration Release --urls "http://localhost:5000" > /dev/null 2>&1 &
TEST_APP_PID=$!

# Function to cleanup on exit
cleanup() {
    echo ""
    echo -e "${BLUE}→ Stopping test app (PID: $TEST_APP_PID)...${NC}"
    kill $TEST_APP_PID 2>/dev/null || true
    wait $TEST_APP_PID 2>/dev/null || true
    echo -e "${GREEN}✓ Cleanup complete${NC}"
}

# Register cleanup function
trap cleanup EXIT

# Wait for the app to start
echo -e "${YELLOW}→ Waiting for test app to start...${NC}"
sleep 8

# Check if the app is running
if ! curl -s http://localhost:5000 > /dev/null; then
    echo -e "${RED}✗ Test app failed to start${NC}"
    exit 1
fi
echo -e "${GREEN}✓ Test app is running${NC}"
echo ""

# Go back to integration tests directory
cd ..

# Run the integration tests
echo -e "${BLUE}→ Running integration tests with Playwright...${NC}"
echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

# Run tests with coverage
dotnet test --configuration Release /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura --logger "console;verbosity=normal"

TEST_EXIT_CODE=$?

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

# Display results
if [ $TEST_EXIT_CODE -eq 0 ]; then
    echo -e "${GREEN}╔═══════════════════════════════════════════════════════════════╗${NC}"
    echo -e "${GREEN}║                                                               ║${NC}"
    echo -e "${GREEN}║  ✓ ALL INTEGRATION TESTS PASSED!                             ║${NC}"
    echo -e "${GREEN}║                                                               ║${NC}"
    echo -e "${GREEN}║  WebGL-dependent code is now fully tested                    ║${NC}"
    echo -e "${GREEN}║                                                               ║${NC}"
    echo -e "${GREEN}╚═══════════════════════════════════════════════════════════════╝${NC}"
    echo ""
    echo -e "${BLUE}Coverage report: coverage.cobertura.xml${NC}"
    exit 0
else
    echo -e "${RED}╔═══════════════════════════════════════════════════════════════╗${NC}"
    echo -e "${RED}║                                                               ║${NC}"
    echo -e "${RED}║  ✗ SOME TESTS FAILED                                         ║${NC}"
    echo -e "${RED}║                                                               ║${NC}"
    echo -e "${RED}║  Please review the test output above for details             ║${NC}"
    echo -e "${RED}║                                                               ║${NC}"
    echo -e "${RED}╚═══════════════════════════════════════════════════════════════╝${NC}"
    exit 1
fi
