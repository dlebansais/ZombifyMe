@echo off
echo Application: ZombifyMeMonitor
echo Parameter #1: %3
echo Parameter #2: %~4
setlocal
if not exist coverage.xml goto nomerge
set MERGE=-mergeoutput
:nomerge
start "ZombifyMe" /B ".\Test\Test-%1\bin\x64\%2\net48\Test-%1.exe" %~4 > set_process_id.bat
PING -n 2 -w 1000 127.1 > NUL
call set_process_id.bat
echo Id: %TEST_ZOMBIFY_PROCESS_ID%
start "ZombifyMeMonitor" /B ".\packages\OpenCover.4.7.922\tools\OpenCover.Console.exe" -register:Path64 -target:".\ZombifyMeMonitor\bin\x64\%2\net48\ZombifyMeMonitor.exe" -targetargs:"%TEST_ZOMBIFY_PROCESS_ID% %~3" "-output:coverage.xml" %MERGE%
