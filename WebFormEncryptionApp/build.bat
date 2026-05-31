@echo off
echo Restoring NuGet packages...
C:\Apps\nuget.exe restore packages.config -PackagesDir packages
echo.
echo Building WebFormEncryptionApp...
C:\Progra~2\MSBuild\14.0\Bin\MSBuild.exe WebFormEncryptionApp.csproj /p:Configuration=Debug /verbosity:minimal
if %ERRORLEVEL% EQU 0 (
    echo.
    echo Build successful!
) else (
    echo.
    echo Build failed.
)
pause
