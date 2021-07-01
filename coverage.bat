@echo off

setlocal

set OPENCOVER_VERSION=4.7.1189
set OPENCOVER=OpenCover.%OPENCOVER_VERSION%
set CODECOV_VERSION=1.12.3
set CODECOV=Codecov.%CODECOV_VERSION%

nuget install OpenCover -Version %OPENCOVER_VERSION% -OutputDirectory packages
nuget install CodeCov -Version %CODECOV_VERSION% -OutputDirectory packages

if not exist ".\packages\%OPENCOVER%\tools\OpenCover.Console.exe" goto error1
if not exist ".\packages\%CODECOV%\tools\codecov.exe" goto error2
if not exist ".\ZombifyMe\bin\x64\Debug\net48\ZombifyMe.dll" goto error3

if exist coverage.xml del coverage.xml

call .\coverage\app-monitor.bat ZombifyMe Debug 0
call .\coverage\wait.bat 20

call .\coverage\app-monitor.bat ZombifyMe Debug "! .\Test\Test-ZombifyMe\obj\x64\Debug\net48\Test-ZombifyMe.exe coverageCancel Coverage 0 watching Restart 0"
call .\coverage\wait.bat 20

call .\coverage\app-monitor.bat ZombifyMe Debug "0 .\Test\Test-ZombifyMe\obj\x64\Debug\net48\Test-ZombifyMe.exe coverageCancel NotCoverage 0 watching Restart 0"
call .\coverage\wait.bat 20

call .\coverage\app-monitor.bat ZombifyMe Debug "0 .\Test\Test-ZombifyMe\obj\x64\Debug\net48\Test-ZombifyMe.exe coverageCancel Coverage ! watching Restart 0"
call .\coverage\wait.bat 20

call .\coverage\app-monitor.bat ZombifyMe Debug "0 .\Test\Test-ZombifyMe\obj\x64\Debug\net48\Test-ZombifyMe.exe coverageCancel Coverage -1 watching Restart 0"
call .\coverage\wait.bat 20

call .\coverage\app-monitor.bat ZombifyMe Debug "0 .\Test\Test-ZombifyMe\obj\x64\Debug\net48\Test-ZombifyMe.exe coverageCancel Coverage 0 watching Restart !"
call .\coverage\wait.bat 20

call .\coverage\app-monitor.bat ZombifyMe Debug "0 .\Test\Test-ZombifyMe\obj\x64\Debug\net48\Test-ZombifyMe.exe coverageCancel Coverage 0 watching Restart -1"
call .\coverage\wait.bat 20

call .\coverage\app-monitor.bat ZombifyMe Debug "0 .\Test\Test-ZombifyMe\obj\x64\Debug\net48\Test-ZombifyMe.exe coverageCancel Coverage 0 watching Restart 0"
call .\coverage\wait.bat 20

call .\coverage\app-monitor.bat ZombifyMe Debug "0 .\Test\Test-ZombifyMe\obj\x64\Debug\net48\Test-ZombifyMe.exe coverageCancel Coverage 0 \"\" Restart 0"
call .\coverage\wait.bat 20

call .\coverage\app-monitor-with-id.bat ZombifyMe Debug ".\Test\Test-ZombifyMe\obj\x64\Debug\net48\Test-ZombifyMe.exe coverageCancel Coverage 0 \"\" Restart 0" "monitor"
call .\coverage\wait.bat 20

call .\coverage\app-monitor-with-id.bat ZombifyMe Debug ".\Test\Test-ZombifyMe\obj\x64\Debug\net48\Test-ZombifyMe.exe coverageCancel Coverage 0 watching Restart 0" "monitor cancel"
call .\coverage\wait.bat 25

call .\coverage\app-monitor-with-id.bat ZombifyMe Debug ".\Test\Test-ZombifyMe\obj\x64\Debug\net48\Test-ZombifyMe.exe coverageCancel Coverage 1 watching Restart 1" "monitor cancel"
call .\coverage\wait.bat 25

call .\coverage\app-monitor-with-id.bat ZombifyMe Debug ".\Test\Test-ZombifyMe\obj\x64\Debug\net48\Test-ZombifyMe.exe coverageCancel Coverage 1 \"\" Restart 1" "monitor wait"
call .\coverage\wait.bat 25

call .\coverage\app-monitor-with-id.bat ZombifyMe Debug ".\Test\Test-ZombifyMe\obj\x64\Debug\net48\Test-ZombifyMe.exe coverageCancel Coverage 1 \"\" \"\" 1" "monitor wait"
call .\coverage\wait.bat 25

call .\coverage\app.bat ZombifyMe Debug coverageCancel
call .\coverage\wait.bat 60

call .\coverage\app.bat ZombifyMe Debug "arg1 arg2 arg3 arg4"
call .\coverage\wait.bat 60

call .\coverage\app.bat ZombifyMe Debug coverageNoForward
call .\coverage\wait.bat 60

call .\coverage\app.bat ZombifyMe Debug coverageBadFolder
call .\coverage\wait.bat 60

call .\coverage\app.bat ZombifyMe Debug coverageNotSymmetric
call .\coverage\wait.bat 60

call .\coverage\app.bat ZombifyMe Debug coverageFailSymmetric
call .\coverage\wait.bat 60

call .\coverage\app.bat ZombifyMe Debug coverageFailLaunch
call .\coverage\wait.bat 60

call .\coverage\app.bat ZombifyMe Debug coverageNoKeepAlive
call .\coverage\wait.bat 60

call .\coverage\app.bat ZombifyMe Debug coverageNoAliveTimeout
call .\coverage\wait.bat 60

if exist set_process_id.bat del set_process_id.bat
call ..\Certification\set_tokens.bat
if exist coverage.xml .\packages\%CODECOV%\tools\codecov -f "coverage.xml" -t "%ZOMBIFYME_CODECOV_TOKEN%"
if exist coverage.xml del coverage.xml
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
