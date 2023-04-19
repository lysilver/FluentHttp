title dapr
set currentpath=%~dp0%
cd..
set parentPath=%cd%
rem echo %currentpath%

cd /d %parentPath%\DaprDemo
dotnet run
pause