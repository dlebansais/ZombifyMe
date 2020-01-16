@echo off

if not exist ".\packages\OpenCover.4.7.922\tools\OpenCover.Console.exe" goto error1
if not exist ".\packages\Codecov.1.9.0\tools\codecov.exe" goto error2
if not exist ".\ZombifyMe\bin\x64\Debug\ZombifyMe.dll" goto error3

if exist .\Test-ZombifyMe\obj\x64\Debug\Coverage-Debug_coverage.xml del .\Test-ZombifyMe\obj\x64\Debug\Coverage-Debug_coverage.xml

call .\coverage\app.bat ZombifyMe Debug coverageCancel
call .\coverage\wait.bat 60

call .\coverage\app.bat ZombifyMe Debug coverageOk
call .\coverage\wait.bat 60

call ..\Certification\set_tokens.bat
rem if exist .\Test-ZombifyMe\obj\x64\Debug\Coverage-Debug_coverage.xml .\packages\Codecov.1.9.0\tools\codecov -f ".\Test-ZombifyMe\obj\x64\Debug\Coverage-Debug_coverage.xml" -t "%ZOMBIFYME_CODECOV_TOKEN%"
goto end

:error1
echo ERROR: OpenCover.Console not found. Restore it with Nuget.
goto end

:error2
echo ERROR: Codecov uploader not found. Restore it with Nuget.
goto end

:error3
echo ERROR: ZombifyMe.dll not built.
goto end

:end
