$ProgressPreference = 'SilentlyContinue'

$Env:SpeedTestFilePath = "$ROOT_DIR\speedtest.exe"
$params = @{
  Name = "TestService"
  BinaryPathName = '"C:\WINDOWS\System32\svchost.exe -k netsvcs"'
  DependsOn = "NetLogon"
  DisplayName = "Test Service"
  StartupType = "Manual"
  Description = "This is a test service."
}
New-Service @params

$ProgressPreference = 'Continue' 
