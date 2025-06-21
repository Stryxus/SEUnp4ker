@echo off
setlocal enabledelayedexpansion

echo Detecting Windows environment and installing dependencies...

:: Check for Chocolatey
where choco >nul 2>nul
if errorlevel 1 (
    echo.
    echo Chocolatey is not installed. Please install it from https://chocolatey.org/
    start https://chocolatey.org/
    exit /b 1
)

echo.
echo Installing required packages with Chocolatey...
choco install -y cmake ninja git

:: Optionally, check for successful installation
where cmake >nul 2>nul
if errorlevel 1 (
    echo Failed to install CMake.
    exit /b 1
)
where ninja >nul 2>nul
if errorlevel 1 (
    echo Failed to install Ninja.
    exit /b 1
)
where git >nul 2>nul
if errorlevel 1 (
    echo Failed to install Git.
    exit /b 1
)

echo.
echo All dependencies installed.

endlocal
exit /b 0
