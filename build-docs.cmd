@echo off
REM Build documentation using DocFX

echo.
echo ========================================
echo   JumpStart Documentation Builder
echo ========================================
echo.

REM Check if DocFX is installed
where docfx >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: DocFX is not installed or not in PATH
    echo.
    echo To install DocFX, run:
    echo   dotnet tool install -g docfx
    echo.
    echo Or update if already installed:
    echo   dotnet tool update -g docfx
    echo.
    pause
    exit /b 1
)

REM Get DocFX version
echo Checking DocFX version...
docfx --version
echo.

REM Clean previous build
if exist "_site" (
    echo Cleaning previous build...
    rmdir /s /q "_site"
    echo.
)

if exist "api" (
    echo Cleaning previous API metadata...
    rmdir /s /q "api"
    echo.
)

if exist "obj" (
    echo Cleaning obj folder...
    rmdir /s /q "obj"
    echo.
)

REM Build the documentation
echo Building documentation...
echo.
docfx docfx.json

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Documentation build failed!
    echo.
    pause
    exit /b 1
)

echo.
echo ========================================
echo   Documentation build completed!
echo ========================================
echo.
echo Output location: _site\
echo.
echo To view the documentation:
echo   1. Open _site\index.html in a browser
echo   2. Or run: docfx serve _site
echo.

REM Ask if user wants to serve the docs
set /p serve="Would you like to serve the documentation now? (Y/N): "
if /i "%serve%"=="Y" (
    echo.
    echo Starting documentation server...
    echo Press Ctrl+C to stop the server
    echo.
    docfx serve _site
)

pause
