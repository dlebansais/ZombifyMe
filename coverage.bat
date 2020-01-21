@echo off

if not exist ".\packages\OpenCover.4.7.922\tools\OpenCover.Console.exe" goto error1
if not exist ".\packages\Codecov.1.9.0\tools\codecov.exe" goto error2
if not exist ".\ZombifyMe\bin\x64\Debug\ZombifyMe.dll" goto error3

if exist .\Test-ZombifyMe\obj\x64\Debug\Coverage-Debug_coverage.xml del .\Test-ZombifyMe\obj\x64\Debug\Coverage-Debug_coverage.xml

call .\coverage\app-monitor.bat ZombifyMe Debug 0
call .\coverage\wait.bat 10

call .\coverage\app-monitor.bat ZombifyMe Debug "! .\Test-ZombifyMe\obj\x64\Debug\Test-ZombifyMe.exe coverageCancel NotCoverage 0 watching Restart 0"
call .\coverage\wait.bat 10

call .\coverage\app-monitor.bat ZombifyMe Debug "! .\Test-ZombifyMe\obj\x64\Debug\Test-ZombifyMe.exe coverageCancel Coverage 0 watching Restart 0"
call .\coverage\wait.bat 10

call .\coverage\app-monitor.bat ZombifyMe Debug "0 .\Test-ZombifyMe\obj\x64\Debug\Test-ZombifyMe.exe coverageCancel Coverage ! watching Restart 0"
call .\coverage\wait.bat 10

call .\coverage\app-monitor.bat ZombifyMe Debug "0 .\Test-ZombifyMe\obj\x64\Debug\Test-ZombifyMe.exe coverageCancel Coverage 0 watching Restart !"
call .\coverage\wait.bat 10

call .\coverage\app-monitor.bat ZombifyMe Debug "0 .\Test-ZombifyMe\obj\x64\Debug\Test-ZombifyMe.exe coverageCancel Coverage 0 watching Restart 0"
call .\coverage\wait.bat 10
goto skip

call .\coverage\app.bat ZombifyMe Debug coverageCancel
call .\coverage\wait.bat 60

call .\coverage\app.bat ZombifyMe Debug "arg1 arg2 arg3 arg4"
call .\coverage\wait.bat 60

call .\coverage\app.bat ZombifyMe Debug coverageNoForward
call .\coverage\wait.bat 60

call .\coverage\app.bat ZombifyMe Debug coverageBadFolder
call .\coverage\wait.bat 60

call .\coverage\app.bat ZombifyMe Debug coverageNotSymetric
call .\coverage\wait.bat 60

call .\coverage\app.bat ZombifyMe Debug coverageFailSymetric
call .\coverage\wait.bat 60

call .\coverage\app.bat ZombifyMe Debug coverageFailLaunch
call .\coverage\wait.bat 60

call .\coverage\app.bat ZombifyMe Debug coverageNoKeepAlive
call .\coverage\wait.bat 60

:skip

call ..\Certification\set_tokens.bat
if exist .\Test-ZombifyMe\obj\x64\Debug\Coverage-Debug_coverage.xml .\packages\Codecov.1.9.0\tools\codecov -f ".\Test-ZombifyMe\obj\x64\Debug\Coverage-Debug_coverage.xml" -t "%ZOMBIFYME_CODECOV_TOKEN%"
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
