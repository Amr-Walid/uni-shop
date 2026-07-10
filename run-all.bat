@echo off
title FTD TechZone Project Runner
echo ===================================================
echo   Starting FTD TechZone Web App and Web Service API...
echo ===================================================
echo.

:: Start FTD.Web MVC Application (Port 5000)
echo [1/2] Launching FTD.Web MVC App on http://localhost:5000...
start "FTD.Web Website" cmd /k "dotnet run --project FTD.Web/FTD.Web.csproj"

:: Start FTD.Api Web Service (Port 5100)
echo [2/2] Launching FTD.Api Web Service on http://localhost:5100...
start "FTD.Api Service" cmd /k "dotnet run --project FTD.Api/FTD.Api.csproj"

echo.
echo ===================================================
echo   Both applications are starting in separate windows.
echo   - Website URL: http://localhost:5000
echo   - Web Service API URL: http://localhost:5100
echo ===================================================
echo.
pause
