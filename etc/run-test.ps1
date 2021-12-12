$ProgressPreference = 'SilentlyContinue'

$SCRIPT_PATH = $MyInvocation.MyCommand.Definition
$ROOT_DIR = Split-Path (Split-Path -Path $SCRIPT_PATH -Parent) -Parent

$CURRENT_DIR=(Get-Location).Path

$APP_DIR = "$ROOT_DIR\app"
Set-Location -Path $APP_DIR

$Env:SpeedTestFilePath = "$ROOT_DIR\speedtest.exe"
Invoke-Expression .\app.exe

Set-Location -Path $CURRENT_DIR

$ProgressPreference = 'Continue' 
