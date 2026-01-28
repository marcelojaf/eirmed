@echo off
cd /d C:\dev\eir-med
set ProgramData=C:\ProgramData
set APPDATA=C:\Users\marce\AppData\Roaming
set LOCALAPPDATA=C:\Users\marce\AppData\Local
set CommonProgramFiles=C:\Program Files\Common Files
set CommonProgramFiles(x86)=C:\Program Files (x86)\Common Files
set CommonProgramW6432=C:\Program Files\Common Files
set ProgramFiles=C:\Program Files
set ProgramFiles(x86)=C:\Program Files (x86)
set ProgramW6432=C:\Program Files

echo === Environment check ===
echo ProgramData=%ProgramData%
echo APPDATA=%APPDATA%

echo.
echo === Building ===
"C:\Program Files\dotnet\dotnet.exe" build
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

echo.
echo === Listing migrations ===
"C:\Users\marce\.dotnet\tools\dotnet-ef.exe" migrations list --project src/EirMed.Infrastructure --startup-project src/EirMed.API

echo.
echo === Applying migrations ===
"C:\Users\marce\.dotnet\tools\dotnet-ef.exe" database update --project src/EirMed.Infrastructure --startup-project src/EirMed.API

echo.
echo Done. Exit code: %ERRORLEVEL%
exit /b %ERRORLEVEL%
