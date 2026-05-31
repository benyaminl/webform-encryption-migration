@echo off
echo === Building ===
call build.bat
if %ERRORLEVEL% neq 0 (
    echo Build failed!
    pause
    exit /b 1
)

echo.
echo === Starting IIS Express on http://localhost:8080 ===
echo Press Q to stop the server.
echo.
"C:\Program Files (x86)\IIS Express\iisexpress.exe" /path:"C:\Users\Ben\CodeWin\WebFormEncryptionMigration\WebFormEncryptionApp" /port:8080
