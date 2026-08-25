[CmdletBinding()]
param(
    [switch]$NoBuild,
    [switch]$NoBrowser,
    [switch]$ValidateOnly,
    [switch]$ResetWorkspace,
    [switch]$ReadOnly,
    [ValidatePattern('^/[A-Za-z0-9._/-]*$')]
    [string]$OpenPath = "/",
    [ValidateSet("Project", "Test")]
    [string]$Mode
)

$ErrorActionPreference = "Stop"

$toolRoot = Split-Path -Parent $PSScriptRoot
$repositoryRoot = (Resolve-Path (Join-Path $toolRoot "..\..")).Path
$dataTxtPath = Join-Path $repositoryRoot "Assets\NTSD\Config\data.txt"
$assetWorkspace = "J:\QQFile\NTSD 2.4.1"
$patchWorkspace = "J:\QQFile\NTSD2.4大量人物补丁（2）"
$patchIndexScript = Join-Path $toolRoot "scripts\build-patch-index.ps1"
$patchIndexPath = Join-Path $env:LOCALAPPDATA "DatSkillFlowWeb\patch-index.json"
$patchSupplementalRoot = Join-Path $toolRoot "artifacts\patch-id-recovery\supplemental"
$previewExecutable = Join-Path $toolRoot "native\bin\dat_preview_cli.exe"
$previewBuildScript = Join-Path $toolRoot "scripts\build-native-preview.ps1"
$testWorkspace = Join-Path $env:LOCALAPPDATA "DatSkillFlowWeb\test-workspace"
$sourceConfig = Join-Path $repositoryRoot "Assets\NTSD\Config"
$testConfig = Join-Path $testWorkspace "Assets\NTSD\Config"
$requiredNodeMajor = 24

function Stop-WithMessage([string]$Message) {
    throw $Message
}

function Assert-FileExists([string]$Path, [string]$Description) {
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        Stop-WithMessage "$Description not found: $Path"
    }
}

function Assert-DirectoryExists([string]$Path, [string]$Description) {
    if (-not (Test-Path -LiteralPath $Path -PathType Container)) {
        Stop-WithMessage "$Description not found: $Path"
    }
}

function Assert-NodeVersion {
    $nodeCommand = Get-Command node -ErrorAction SilentlyContinue
    if ($null -eq $nodeCommand) {
        Stop-WithMessage "Node.js was not found. Install Node.js 24.11.1."
    }

    $rawVersion = (& node --version).Trim()
    if ($LASTEXITCODE -ne 0 -or $rawVersion -notmatch '^v(\d+)\.') {
        Stop-WithMessage "Unable to read the Node.js version."
    }

    if ([int]$Matches[1] -ne $requiredNodeMajor) {
        Stop-WithMessage "Node.js 24.x is required. Current version: $rawVersion"
    }
}

function Test-WebBuildRequired {
    $manifestPath = Join-Path $toolRoot "dist\build-manifest.json"
    if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
        return $true
    }

    $manifestTime = (Get-Item -LiteralPath $manifestPath).LastWriteTimeUtc
    $inputs = @(
        Get-Item -LiteralPath (Join-Path $toolRoot "index.html")
        Get-Item -LiteralPath (Join-Path $toolRoot "render-cadence.html")
        Get-Item -LiteralPath (Join-Path $toolRoot "package.json")
        Get-Item -LiteralPath (Join-Path $toolRoot "package-lock.json")
        Get-Item -LiteralPath (Join-Path $toolRoot "tsconfig.json")
        Get-Item -LiteralPath (Join-Path $toolRoot "tsconfig.server.json")
        Get-Item -LiteralPath (Join-Path $toolRoot "vite.config.ts")
        Get-ChildItem -LiteralPath (Join-Path $toolRoot "src") -Recurse -File
        Get-ChildItem -LiteralPath (Join-Path $toolRoot "tests") -Recurse -File
        Get-ChildItem -LiteralPath (Join-Path $toolRoot "scripts") -File
    )
    return $null -ne ($inputs | Where-Object { $_.LastWriteTimeUtc -gt $manifestTime } | Select-Object -First 1)
}

function ConvertTo-NativeArgument([string]$Value) {
    if ($Value.Contains('"')) {
        Stop-WithMessage "Native process arguments must not contain quotes: $Value"
    }
    if ($Value -notmatch '\s') {
        return $Value
    }
    return '"' + $Value + '"'
}

function Test-InteractiveTerminal {
    try {
        return [Environment]::UserInteractive `
            -and $Host.Name -eq "ConsoleHost" `
            -and -not [Console]::IsInputRedirected
    }
    catch {
        return $false
    }
}

function Resolve-LaunchMode {
    if (-not [string]::IsNullOrWhiteSpace($Mode)) {
        return $Mode
    }
    if (-not (Test-InteractiveTerminal)) {
        Stop-WithMessage "Non-interactive startup requires -Mode Project or -Mode Test."
    }

    Write-Host ""
    Write-Host "请选择启动模式："
    Write-Host "  1. 正式项目（可通过安全确认和备份协议写入仓库 DAT）"
    Write-Host "  2. 测试副本（使用 LocalAppData 隔离工作区）"
    Write-Host "  0. 取消"
    while ($true) {
        switch (Read-Host "请输入 1、2 或 0") {
            "1" { return "Project" }
            "2" { return "Test" }
            "0" { return $null }
            default { Write-Host "无效选择，请重新输入。" }
        }
    }
}

function Initialize-TestWorkspace {
    Write-Host "Preparing an isolated DAT test workspace..."
    if ($ResetWorkspace -and (Test-Path -LiteralPath $testWorkspace)) {
        Remove-Item -LiteralPath $testWorkspace -Recurse -Force
    }
    if (-not (Test-Path -LiteralPath $testConfig -PathType Container)) {
        New-Item -ItemType Directory -Path $testConfig -Force | Out-Null
        Copy-Item -Path (Join-Path $sourceConfig "*") -Destination $testConfig -Recurse
    }
}

function Assert-StartupPrerequisites {
    Assert-NodeVersion
    Assert-FileExists $dataTxtPath "Project data.txt"
    Assert-DirectoryExists $assetWorkspace "NTSD asset workspace"
    Assert-DirectoryExists $patchWorkspace "NTSD patch workspace"
    Assert-FileExists $patchIndexScript "Patch package index builder"
    Assert-FileExists (Join-Path $toolRoot "package.json") "Package manifest"
    Assert-FileExists (Join-Path $toolRoot "scripts\start.mjs") "Server entry point"
    if (-not $NoBuild) {
        $nodeExecutable = (Get-Command node).Source
        Assert-FileExists (Join-Path (Split-Path -Parent $nodeExecutable) "node_modules\npm\bin\npm-cli.js") "npm JavaScript entry"
        Assert-FileExists (Join-Path $toolRoot "scripts\build.mjs") "Build script"
        Assert-FileExists $previewBuildScript "Native preview build script"
    }
    else {
        Assert-FileExists $previewExecutable "Native preview executable"
        Assert-FileExists (Join-Path $toolRoot "dist\build-manifest.json") "Build manifest"
    }
}

if ($ValidateOnly) {
    Assert-StartupPrerequisites
    Assert-FileExists $previewExecutable "Native preview executable"
    Write-Host "One-click startup prerequisites passed."
    exit 0
}

$launchMode = Resolve-LaunchMode
if ($null -eq $launchMode) {
    Write-Host "已取消启动。"
    exit 0
}
if ($launchMode -eq "Project" -and $ResetWorkspace) {
    Stop-WithMessage "-ResetWorkspace is only valid with -Mode Test."
}
if ($OpenPath.Contains("..")) {
    Stop-WithMessage "-OpenPath must stay inside the Dat Skill Flow Web static root."
}
Assert-StartupPrerequisites

$workspace = $null
$writableDataTxt = $null
$sidecarPath = $null
if ($launchMode -eq "Project") {
    $workspace = $repositoryRoot
    $writableDataTxt = $dataTxtPath
    $sidecarPath = Join-Path $repositoryRoot ".dat-skill-flow\skills.json"
    Write-Host "当前模式：正式项目"
    Write-Host "警告：确认覆盖后将写入仓库中的真实 DAT。"
}
else {
    Initialize-TestWorkspace
    $workspace = $testWorkspace
    $writableDataTxt = Join-Path $testWorkspace "Assets\NTSD\Config\data.txt"
    $sidecarPath = Join-Path $testWorkspace ".dat-skill-flow\skills.json"
    Write-Host "当前模式：测试副本"
}
Write-Host "可写 workspace：$workspace"
Write-Host "可写 data.txt：$writableDataTxt"
Write-Host "可写技能 sidecar：$sidecarPath"

$process = $null
Push-Location $toolRoot
try {
    Write-Host "正在索引补丁包（只读扫描 J 盘）..."
    & $patchIndexScript -LibraryRoot $patchWorkspace -OutputPath $patchIndexPath -SupplementalRoot $patchSupplementalRoot
    if ($LASTEXITCODE -ne 0) {
        Stop-WithMessage "Patch package indexing failed with exit code $LASTEXITCODE."
    }
    Assert-FileExists $patchIndexPath "Patch package index"

    if (-not $NoBuild) {
        Write-Host "Building C++ battle preview adapter..."
        & $previewBuildScript
        if ($LASTEXITCODE -ne 0) {
            Stop-WithMessage "Native preview build failed with exit code $LASTEXITCODE."
        }
        Assert-FileExists $previewExecutable "Native preview executable"

        if (Test-WebBuildRequired) {
            Write-Host "Building DAT Skill Flow Web..."
            $nodeExecutable = (Get-Command node).Source
            $npmCli = Join-Path (Split-Path -Parent $nodeExecutable) "node_modules\npm\bin\npm-cli.js"
            & $nodeExecutable $npmCli run build
            if ($LASTEXITCODE -ne 0) {
                Stop-WithMessage "Build failed with exit code $LASTEXITCODE."
            }
        }
        else {
            Write-Host "DAT Skill Flow Web build is up to date."
        }
    }

    $nodeArguments = @(
        (Join-Path $toolRoot "scripts\start.mjs"),
        "--root", (Join-Path $toolRoot "dist"),
        "--manifest", (Join-Path $toolRoot "dist\build-manifest.json"),
        "--workspace", $workspace,
        "--data-txt", "Assets/NTSD/Config/data.txt",
        "--asset-workspace", $assetWorkspace,
        "--patch-workspace", $patchWorkspace,
        "--patch-index", $patchIndexPath,
        "--port", "0"
    )
    if ($ReadOnly) {
        $nodeArguments += "--read-only"
    }
    $processInfo = New-Object System.Diagnostics.ProcessStartInfo
    $processInfo.FileName = (Get-Command node).Source
    $processInfo.Arguments = (($nodeArguments | ForEach-Object { ConvertTo-NativeArgument $_ }) -join " ")
    $processInfo.UseShellExecute = $false
    $processInfo.RedirectStandardOutput = $true
    $processInfo.RedirectStandardError = $false
    $previewEnvironmentName = "DAT_SKILL_FLOW_CPP_PREVIEW_EXECUTABLE"
    $gameRootEnvironmentName = "DAT_SKILL_FLOW_CPP_GAME_ROOT"
    $previousPreviewExecutable = [Environment]::GetEnvironmentVariable($previewEnvironmentName, "Process")
    $previousGameRoot = [Environment]::GetEnvironmentVariable($gameRootEnvironmentName, "Process")
    [Environment]::SetEnvironmentVariable($previewEnvironmentName, $previewExecutable, "Process")
    [Environment]::SetEnvironmentVariable($gameRootEnvironmentName, $assetWorkspace, "Process")
    try {
        $process = [System.Diagnostics.Process]::Start($processInfo)
    }
    finally {
        [Environment]::SetEnvironmentVariable($previewEnvironmentName, $previousPreviewExecutable, "Process")
        [Environment]::SetEnvironmentVariable($gameRootEnvironmentName, $previousGameRoot, "Process")
    }

    $url = $null
    while (-not $process.HasExited) {
        $line = $process.StandardOutput.ReadLine()
        if ($null -eq $line) {
            continue
        }
        Write-Host $line
        if ($line -match "Dat Skill Flow server listening at (https?://\S+)") {
            $url = $Matches[1]
            break
        }
    }

    if ($null -eq $url) {
        Stop-WithMessage "The server exited before reporting its listening address."
    }

    if (-not $NoBrowser) {
        Start-Process ("$url$OpenPath")
    }

    Write-Host "Editor ready at: $url$OpenPath"
    if ($ReadOnly) {
        Write-Host "当前为只读渲染帧率对比模式：编辑、保存和技能 sidecar 写入已由服务器拒绝。"
    }
    if ($launchMode -eq "Test") {
        Write-Host "Editable test copy: $testWorkspace"
        Write-Host "Use -Mode Test -ResetWorkspace to replace the test copy on the next launch."
    }
    else {
        Write-Host "正式项目保存仍需通过页面内的安全确认和备份协议。"
    }
    Write-Host "Close this window to stop the server."

    while (-not $process.HasExited) {
        $line = $process.StandardOutput.ReadLine()
        if ($null -ne $line) {
            Write-Host $line
        }
    }
    if ($process.ExitCode -ne 0) {
        Stop-WithMessage "Server exited with code $($process.ExitCode)."
    }
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        $process.Kill()
        $process.WaitForExit()
    }
    Pop-Location
}
