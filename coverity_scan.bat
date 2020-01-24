set PATH=%PATH%;"C:\Applications\cov-analysis-win64-2019.03\bin"

"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\msbuild" /t:clean
cov-build --dir cov-int "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\msbuild"

C:\Applications\7-Zip\7z a cov-int cov-int -tzip

call ..\Certification\set_tokens.bat
call ..\Certification\coverity_scan_any.bat ZombifyMe %ZOMBIFYME_COVERITYSCAN_TOKEN%

rd /S /Q cov-int
del cov-int.zip