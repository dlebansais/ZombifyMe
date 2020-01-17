@echo off
echo Application: ZombifyMeMonitor
echo Parameter: %3
if not exist ".\Test-%1\obj\x64\%2\Coverage-%2_coverage.xml" goto nomerge
set MERGE=-mergeoutput
:nomerge
start "ZombifyMeMonitor" /B ".\packages\OpenCover.4.7.922\tools\OpenCover.Console.exe" -register:Path64 -target:".\ZombifyMeMonitor\bin\x64\%2\ZombifyMeMonitor.exe" -targetargs:%3 -output:".\Test-%1\obj\x64\%2\Coverage-%2_coverage.xml" %MERGE%
set MERGE=
