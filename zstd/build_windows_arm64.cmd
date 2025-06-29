@echo off
cd /d %~dp0
setlocal enabledelayedexpansion
set SRC_DIR=..\Modules\zstd\build\cmake
set BUILD_DIR=out\win-arm64\build
set OUT_DIR=out\win-arm64

echo Creating build directories...
if not exist "%BUILD_DIR%" mkdir "%BUILD_DIR%"
if not exist "%OUT_DIR%" mkdir "%OUT_DIR%"

echo Configuring CMake for Windows ARM64...
cmake -S "%SRC_DIR%" -B "%BUILD_DIR%" -G Ninja ^
  -DCMAKE_SYSTEM_NAME=Windows ^
  -DBUILD_SHARED_LIBS=ON ^
  -DCMAKE_BUILD_TYPE=Release ^
  -DCMAKE_LIBRARY_OUTPUT_DIRECTORY="%CD%\%OUT_DIR%" ^
  -DCMAKE_ARCHIVE_OUTPUT_DIRECTORY="%CD%\%OUT_DIR%" ^
  -DCMAKE_RUNTIME_OUTPUT_DIRECTORY="%CD%\%OUT_DIR%"

if errorlevel 1 (
    echo CMake configuration failed!
    exit /b 1
)

echo Building with Ninja...
cmake --build "%BUILD_DIR%"
if errorlevel 1 (
    echo Build failed!
    exit /b 1
)

echo Windows ARM64 build complete. Output in %OUT_DIR%
endlocal
exit /b 0
