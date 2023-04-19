rem dotnet xxx.dll --environment Production
set currentpath=%~dp0%
cd..
set parentPath=%cd%
echo %currentpath%

cd /d %parentPath%
set paa=%parentPath%
echo %paa%
dotnet pack  %paa%\FluentHttp.Abstractions\FluentHttp.Abstractions.csproj --configuration release --output %parentPath%\nupkgs
dotnet pack  %paa%\FluentHttp.Ext\FluentHttp.Ext.csproj --configuration release --output %parentPath%\nupkgs
dotnet pack  %paa%\FluentHttp\FluentHttp.csproj --configuration release --output %parentPath%\nupkgs
dotnet pack  %paa%\FluentHttp.SourceGenerator\FluentHttp.SourceGenerator.csproj --configuration release --output %parentPath%\nupkgs

cd /d %parentPath%\nupkgs
rem dotnet nuget push "*.nupkg" -s D:\cache\nupkgs
pause