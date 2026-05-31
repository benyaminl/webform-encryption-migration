@echo off
echo Restoring NuGet packages...
C:\Apps\nuget.exe restore packages.config -PackagesDir packages
echo.
echo Building WebFormEncryptionApp (Release)...
C:\Progra~2\MSBuild\14.0\Bin\MSBuild.exe WebFormEncryptionApp.csproj /p:Configuration=Release /verbosity:minimal
if %ERRORLEVEL% NEQ 0 (
    echo Build failed.
    pause
    exit /b 1
)
echo.
echo Publishing to publish\ folder...
if exist publish rmdir /s /q publish
mkdir publish
mkdir publish\App_Data\Files

xcopy /Y /S bin publish\bin\
xcopy /Y *.aspx publish\
xcopy /Y *.ashx publish\
xcopy /Y Global.asax publish\
xcopy /Y Web.config publish\

echo.
echo Publish successful! Output: publish\
pause
