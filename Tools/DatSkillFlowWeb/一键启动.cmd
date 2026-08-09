@echo off
@echo off
setlocal

rem Double-click defaults to the isolated Test workspace.
rem Pass -Mode Project explicitly when real-project mode is required.
set "LaunchArgs=%*"
set "HasMode="
if not "%~1"=="" (
    for %%A in (%*) do if /I "%%~A"=="-Mode" set "HasMode=1"
)
if not defined HasMode set "LaunchArgs=-Mode Test %LaunchArgs%"

powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\start-local.ps1" %LaunchArgs%
if errorlevel 1 (
    echo.
    echo Startup failed. Review the error above.
    pause
)

endlocal
