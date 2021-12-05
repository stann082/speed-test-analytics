function download_latest_package {
    $ProgressPreference = 'SilentlyContinue'

    $SPEED_TEST_MAIN_DIR = $args[0]

    echo "Locating the latest package to download..."
    $WIN_DOWNLOAD_LINK = ((Invoke-WebRequest -UseBasicParsing -Uri https://www.speedtest.net/apps/cli).Links | Where outerHTML -like "*Download for Windows*").href
    if (!$WIN_DOWNLOAD_LINK) {
        echo "Could not locate a download link. Please do it manually at https://www.speedtest.net/apps/cli"
        exit 1
    }

    $DOWNLOAD_DIR = "${SPEED_TEST_MAIN_DIR}\download"
    echo "Downloading the latest speed-test package..."
    New-Item -ItemType Directory -Force -Path $DOWNLOAD_DIR | Out-Null
    Start-BitsTransfer -Source $WIN_DOWNLOAD_LINK -Destination $DOWNLOAD_DIR

    $ZIP_PACKAGE_NAME = Split-Path $WIN_DOWNLOAD_LINK -Leaf
    $ZIP_PACKAGE_PATH = "${DOWNLOAD_DIR}\${ZIP_PACKAGE_NAME}"
    if (-Not (Test-Path -Path $ZIP_PACKAGE_PATH -PathType Leaf)) {
        echo "Package not found..."
        exit 1
    }

    Expand-Archive -LiteralPath $ZIP_PACKAGE_PATH -DestinationPath $SPEED_TEST_MAIN_DIR -Force

    $ProgressPreference = 'Continue' 
}

$SPEED_TEST_MAIN_DIR = "$env:appdata\SpeedTest"
$SPEED_TEST_MAIN_DIR_EXISTS = Test-Path -Path $SPEED_TEST_MAIN_DIR

$DOWNLOAD_LATEST=$args[0]
if (-Not $SPEED_TEST_MAIN_DIR_EXISTS -or ($DOWNLOAD_LATEST -eq "--download-latest")) {
    download_latest_package "$SPEED_TEST_MAIN_DIR"
}

echo "Now the other stuff..."
# Download https://github.com/winsw/winsw/releases/download/v2.11.0/WinSW-x64.exe
# Extract WinSW
# Setup config file
# 
