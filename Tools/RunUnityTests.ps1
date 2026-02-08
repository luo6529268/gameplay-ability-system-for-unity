param(
  [string]$ProjectPath = "I:\\GitHub\\Unity_GAS\\gameplay-ability-system-for-unity",
  [string]$UnityExePath = $env:UNITY_EXE
)

if ([string]::IsNullOrEmpty($UnityExePath)) {
  # Default Unity path (adjust if needed)
  # Auto-detect: try common Unity install locations
  $candidates = @(
    "D:\\Unity\\HubEditor\\2022.3.4f1c1\\Editor\\Unity.exe",
    "C:\\Program Files\\Unity\\Hub\\Editor\\2022.3.4f1c1\\Editor\\Unity.exe"
  )
  foreach ($c in $candidates) {
    if (Test-Path $c) { $UnityExePath = $c; break }
  }
  if ([string]::IsNullOrEmpty($UnityExePath) -or !(Test-Path $UnityExePath)) {
    Write-Host "ERROR: Unity.exe not found. Set UNITY_EXE env var or update candidates list." -ForegroundColor Red
    exit 1
  }
  Write-Host "Using Unity: $UnityExePath"
}

function Run-UnityTest([string]$platform, [string]$testResults, [string]$logFile) {
  $unityArgs = @(
    "-batchmode",
    "-nographics",
    "-quit",
    "-projectPath", $ProjectPath,
    "-runTests",
    "-testPlatform", $platform,
    "-testResults", $testResults,
    "-logFile", $logFile
  )
  Write-Host "Starting Unity tests for $platform..."
  $proc = Start-Process -FilePath $UnityExePath -ArgumentList $unityArgs -NoNewWindow -Wait -PassThru
  return $proc.ExitCode
}

$exitCodeEdit = Run-UnityTest -platform "EditMode" -testResults "${ProjectPath}\TestResults-EditMode.xml" -logFile "${ProjectPath}\UnityTest-EditMode.log" 2>&1
Write-Host "EditMode exit code: $exitCodeEdit"

$exitCodePlay = Run-UnityTest -platform "PlayMode" -testResults "${ProjectPath}\TestResults-PlayMode.xml" -logFile "${ProjectPath}\UnityTest-PlayMode.log" 2>&1
Write-Host "PlayMode exit code: $exitCodePlay"

if ($exitCodeEdit -ne 0 -or $exitCodePlay -ne 0) {
  Write-Host "One or more Unity test runs failed. Review logs for details." -ForegroundColor Red
  exit 1
} else {
  Write-Host "Unity tests completed successfully." -ForegroundColor Green
  exit 0
}
