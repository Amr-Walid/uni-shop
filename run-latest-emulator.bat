@echo off
title Uni-Shop Auto Pull and Run (Android Emulator)
echo ===================================================
echo   Uni-Shop: Pulling latest changes and Launching (Android)...
echo ===================================================
echo.

:: 1. Pull latest code from GitHub
echo [1/4] Pulling latest updates from GitHub...
git stash
git pull origin genspark_ai_developer

:: 2. Launch FTD.Api Backend Service
echo [2/4] Starting Backend API Service (http://localhost:5100)...
start "FTD.Api Service" cmd /k "set ASPNETCORE_ENVIRONMENT=Development&& set ASPNETCORE_URLS=http://0.0.0.0:5100&& dotnet run --project FTD.Api/FTD.Api.csproj"

:: 3. Launch Android Emulator & Flutter App
echo [3/4] Launching Pixel 7 Emulator (if not already open)...
call C:\flutter\bin\flutter.bat emulators --launch Pixel_7

echo [4/4] Launching Uni-Shop App on Android Emulator...
cd mobile
start "Uni-Shop Mobile App" cmd /k "C:\flutter\bin\flutter.bat run -d android --android-skip-build-dependency-validation --dart-define=API_BASE_URL=http://10.0.2.2:5100"

echo.
echo ===================================================
echo   All services launched!
echo ===================================================
