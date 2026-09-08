@echo off
title Uni-Shop Remote Developer Sandbox (Novita AI)
echo ===================================================
echo   Connecting Uni-Shop App to Developer Sandbox...
echo ===================================================
echo.
cd mobile
start "Uni-Shop Mobile App" cmd /k "C:\flutter\bin\flutter.bat run -d android --android-skip-build-dependency-validation --dart-define=API_BASE_URL=https://5100-imn4rbh8cctut85id3vs1-5c13a017.sandbox.novita.ai"
