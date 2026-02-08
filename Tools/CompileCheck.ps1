$UnityExe = "D:\Unity\HubEditor\2022.3.4f1\Editor\Unity.exe"
$ProjectPath = "I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity"
$LogFile = "$ProjectPath\UnityCompile.log"

Write-Host "Running Unity compile check..."
Write-Host "Unity: $UnityExe"
Write-Host "Project: $ProjectPath"

$proc = Start-Process -FilePath $UnityExe -ArgumentList @(
    "-batchmode",
    "-nographics",
    "-quit",
    "-projectPath", $ProjectPath,
    "-logFile", $LogFile
) -NoNewWindow -Wait -PassThru

Write-Host "Exit code: $($proc.ExitCode)"

if ($proc.ExitCode -eq 0) {
    Write-Host "Compilation succeeded!" -ForegroundColor Green
} else {
    Write-Host "Compilation failed! Check $LogFile" -ForegroundColor Red
}

exit $proc.ExitCode
