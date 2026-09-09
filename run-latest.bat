@echo off
title Uni-Shop Auto Pull & Run
echo ===================================================
echo   Uni-Shop: Pulling latest changes & Launching...
echo ===================================================
echo.

:: 1. Pull latest code from GitHub
echo [1/3] Pulling latest updates from GitHub...
git pull origin genspark_ai_developer

:: 2. Launch FTD.Api Backend Service in separate window
echo [2/3] Starting Backend API Service (http://localhost:5100)...
start "FTD.Api Service" cmd /k "set ASPNETCORE_ENVIRONMENT=Development&& set ASPNETCORE_URLS=http://0.0.0.0:5100&& dotnet run --project FTD.Api/FTD.Api.csproj"

:: 3. Launch Flutter App
echo [3/3] Launching Mobile App...
cd mobile
start "Uni-Shop Mobile App" cmd /k "C:\flutter\bin\flutter.bat run -d chrome --dart-define=API_BASE_URL=http://localhost:5100"

echo.
echo ===================================================
echo   All services launched!
echo   - Backend API: http://localhost:5100/swagger
echo   - Mobile App: Opening in Chrome...
echo ===================================================
