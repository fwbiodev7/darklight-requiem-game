@echo off
set "BUILD_DIR=%~dp0outputs\lucius-test-build-20261111\Windows"
if not exist "%BUILD_DIR%\DarklightRequiem.exe" set "BUILD_DIR=%~dp0outputs\lucius-test-build-20261110\Windows"
cd /d "%BUILD_DIR%"
start "" "DarklightRequiem.exe" %*
