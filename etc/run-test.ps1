$ProgressPreference = 'SilentlyContinue'

$CURRENT_DIR=(Get-Location).Path

$ROOT_DIR = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
Write-Output $ROOT_DIR
exit 0
$APP_DIR = "$ROOT_DIR\app"
Set-Location -Path $APP_DIR

$Env:SpeedTestFilePath = "$SPEED_TEST_MAIN_DIR\speedtest.exe"
Invoke-Expression .\app.exe

Set-Location -Path $CURRENT_DIR

$ProgressPreference = 'Continue' 
