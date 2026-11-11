@echo off
set "GAME_DIR=%~dp0outputs\build-windows-estabilidade-20261110\Windows"
set "GAME_EXE=%GAME_DIR%\DarklightRequiem.exe"

if exist "%GAME_EXE%" (
    echo Iniciando DARKLIGHT: REQUIEM [Build Estabilidade 10/11/2026]...
    start "" /D "%GAME_DIR%" "%GAME_EXE%" %*
) else (
    echo [ERRO] Executavel nao encontrado em: %GAME_EXE%
    pause
)
