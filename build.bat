@echo off
setlocal
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\build-builder.ps1" %*
if errorlevel 1 (
  echo.
  echo Build failed. See messages above.
  pause
  exit /b 1
)
echo.
echo Build succeeded. Launch from Build\Builder.exe
pause
exit /b 0
