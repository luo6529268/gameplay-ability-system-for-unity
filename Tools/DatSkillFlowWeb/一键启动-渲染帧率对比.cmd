@echo off
setlocal

rem This entry always uses the isolated Test workspace and starts the server in read-only mode.
powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\start-local.ps1" -Mode Test -ReadOnly -OpenPath /render-cadence.html
if errorlevel 1 (
    echo.
    echo Render cadence comparison startup failed. Review the error above.
    pause
)

endlocal
