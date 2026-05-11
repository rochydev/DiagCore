@echo off
REM ============================================================
REM  Diagnosticador de Sistemas - Lanzador
REM  Autor: RochyDev
REM ============================================================
REM  Lanza el script PowerShell con permisos de administrador
REM ============================================================

title Diagnosticador de Sistemas - by RochyDev

REM Comprueba si ya estamos elevados
net session >nul 2>&1
if %errorLevel% == 0 (
    goto :run
) else (
    echo Solicitando permisos de administrador...
    powershell -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
    exit /b
)

:run
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Diagnosticador.ps1"
pause
