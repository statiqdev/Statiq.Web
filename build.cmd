@echo off
dotnet run --project "build/Statiq.Web.Build/Statiq.Web.Build.csproj" -- %*
set exitcode=%errorlevel%
cd %~dp0
exit /b %exitcode%