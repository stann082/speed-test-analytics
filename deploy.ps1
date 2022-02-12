function download_latest_package {
    $DST_ROOT_DIR = $args[0]

    Write-Output "Locating the latest package to download..."
    $WIN_DOWNLOAD_LINK = ((Invoke-WebRequest -UseBasicParsing -Uri 'https://www.speedtest.net/apps/cli').Links | Where-Object outerHTML -Like "*Download for Windows*").href
    if (!$WIN_DOWNLOAD_LINK) {
        Write-Output "Could not locate a download link. Please do it manually at https://www.speedtest.net/apps/cli"
        exit 1
    }

    $DOWNLOAD_DIR = "${DST_ROOT_DIR}\download"
    Write-Output "Downloading the latest speed-test package..."
    New-Item -ItemType Directory -Force -Path $DOWNLOAD_DIR | Out-Null
    Start-BitsTransfer -Source $WIN_DOWNLOAD_LINK -Destination $DOWNLOAD_DIR

    $ZIP_PACKAGE_NAME = Split-Path $WIN_DOWNLOAD_LINK -Leaf
    $ZIP_PACKAGE_PATH = "${DOWNLOAD_DIR}\${ZIP_PACKAGE_NAME}"
    if (-not (Test-Path -Path $ZIP_PACKAGE_PATH -PathType Leaf)) {
        Write-Output "Package not found..."
        exit 1
    }

    Expand-Archive -LiteralPath $ZIP_PACKAGE_PATH -DestinationPath $DST_ROOT_DIR -Force
}

$ProgressPreference = 'SilentlyContinue'

$DOWNLOAD_LATEST = $args[0]
$SRC_ROOT_DIR = Split-Path -Path $MyInvocation.MyCommand.Definition

$DST_ROOT_DIR = "$Env:appdata\SpeedTest"
$DST_ROOT_DIR_EXISTS = Test-Path -Path $DST_ROOT_DIR
if (-not $DST_ROOT_DIR_EXISTS -or ($DOWNLOAD_LATEST -eq "--download-latest")) {
    download_latest_package "$DST_ROOT_DIR"
}

$SRC_ETC_DIR = "$SRC_ROOT_DIR\etc"
$DST_ETC_DIR = "$DST_ROOT_DIR\etc"
Remove-Item -Path $DST_ETC_DIR -Recurse -Force
Copy-Item -Path $SRC_ETC_DIR -Destination $DST_ETC_DIR -Recurse -Force

$SRC_APP_DIR = "$SRC_ROOT_DIR\runner\src\app"
$DST_APP_DIR = "$DST_ROOT_DIR\app"
Remove-Item -Path $DST_APP_DIR -Recurse -Force
Write-Output "Publishing the app to $DST_APP_DIR..."
dotnet publish $SRC_APP_DIR -o $DST_APP_DIR -c Release

Get-ChildItem -Path $DST_APP_DIR -Include "*.pdb" -Recurse | Remove-Item -Force

$ProgressPreference = 'Continue' 
