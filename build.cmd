@echo off
cd "build\Statiq.Web.Build"
dotnet run -- %*
set exitcode=%errorlevel%
cd %~dp0
exit /b %exitcode%