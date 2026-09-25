@echo off
setlocal

rem Publishes a release of the Samples Gallery from GitHub Actions, without building anything locally.
rem The tag comes from the AssemblyInformationalVersion in WiceAot\Properties\AssemblyVersionInfo.cs,
rem so bump that first when the number has to change.
rem A version that is already released stops the run rather than replacing it.
rem
rem Needs the GitHub CLI, https://cli.github.com, signed in with "gh auth login".
rem The workflow also has to exist on the default branch, which is where GitHub looks for it.
rem
rem   release.bat           builds every architecture and publishes the release.
rem   release.bat build     builds every architecture and leaves the binaries as artifacts.

where gh >nul 2>&1
if errorlevel 1 (
    echo the GitHub CLI is needed. see https://cli.github.com
    exit /b 1
)

set RELEASE=true
if /i "%~1"=="build" set RELEASE=false

echo starting the workflow with release=%RELEASE%
gh workflow run build.yml -f release=%RELEASE%
if errorlevel 1 exit /b 1

rem the run takes a moment to be queued before it can be found.
timeout /t 6 /nobreak >nul

set RUNID=
for /f "delims=" %%i in ('gh run list --workflow=build.yml --limit 1 --json databaseId --jq ".[0].databaseId"') do set RUNID=%%i
if "%RUNID%"=="" (
    echo the run was started but could not be found. see it at the Actions tab.
    exit /b 0
)

gh run watch %RUNID%
