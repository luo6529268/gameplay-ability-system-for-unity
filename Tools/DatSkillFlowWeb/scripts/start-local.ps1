[CmdletBinding()]
param(
    [switch]$NoBuild,
    [switch]$NoBrowser,
    [switch]$ValidateOnly,
    [switch]$ResetWorkspace
)

$ErrorActionPreference = "Stop"

$toolRoot = Split-Path -Parent $PSScriptRoot
$repositoryRoot = (Resolve-Path (Join-Path $toolRoot "..\..")).Path
$dataTxtPath = Join-Path $repositoryRoot "Assets\NTSD\Config\data.txt"
$assetWorkspace = "J:\QQFile\NTSD 2.4.1"
$previewExecutable = "J:\QQFile\NTSD2.4\ntsd_cpp\dat_preview_cli.exe"
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

function ConvertTo-NativeArgument([string]$Value) {
    if ($Value.Contains('"')) {
        Stop-WithMessage "Native process arguments must not contain quotes: $Value"
    }
    if ($Value -notmatch '\s') {
        return $Value
    }
    return '"' + $Value + '"'
}

Assert-NodeVersion
Assert-FileExists $dataTxtPath "Project data.txt"
Assert-DirectoryExists $assetWorkspace "NTSD asset workspace"
Assert-FileExists $previewExecutable "Native preview executable"

if ($ValidateOnly) {
    if ($null -eq (Get-Command npm.cmd -ErrorAction SilentlyContinue)) {
        Stop-WithMessage "npm.cmd was not found."
    }
    Assert-FileExists (Join-Path $toolRoot "package.json") "Package manifest"
    Assert-FileExists (Join-Path $toolRoot "scripts\build.mjs") "Build script"
    Assert-FileExists (Join-Path $toolRoot "scripts\start.mjs") "Server entry point"
    Write-Host "One-click startup prerequisites passed."
    exit 0
}

$process = $null
Push-Location $toolRoot
try {
    Write-Host "Preparing an isolated DAT test workspace..."
    if ($ResetWorkspace -and (Test-Path -LiteralPath $testWorkspace)) {
        Remove-Item -LiteralPath $testWorkspace -Recurse -Force
    }
    if (-not (Test-Path -LiteralPath $testConfig -PathType Container)) {
        New-Item -ItemType Directory -Path $testConfig -Force | Out-Null
        Copy-Item -Path (Join-Path $sourceConfig "*") -Destination $testConfig -Recurse
    }

    if (-not $NoBuild) {
        Write-Host "Building DAT Skill Flow Web..."
        & npm.cmd run build
        if ($LASTEXITCODE -ne 0) {
            Stop-WithMessage "Build failed with exit code $LASTEXITCODE."
        }
    }

    $nodeArguments = @(
        (Join-Path $toolRoot "scripts\start.mjs"),
        "--root", (Join-Path $toolRoot "dist"),
        "--manifest", (Join-Path $toolRoot "dist\build-manifest.json"),
        "--workspace", $testWorkspace,
        "--data-txt", "Assets/NTSD/Config/data.txt",
        "--asset-workspace", $assetWorkspace,
        "--port", "0"
    )
    $processInfo = New-Object System.Diagnostics.ProcessStartInfo
    $processInfo.FileName = (Get-Command node).Source
    $processInfo.Arguments = (($nodeArguments | ForEach-Object { ConvertTo-NativeArgument $_ }) -join " ")
    $processInfo.UseShellExecute = $false
    $processInfo.RedirectStandardOutput = $true
    $processInfo.RedirectStandardError = $false
    $process = [System.Diagnostics.Process]::Start($processInfo)

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
        Start-Process $url
    }

    Write-Host "Editor ready at: $url"
    Write-Host "Editable test copy: $testWorkspace"
    Write-Host "Use -ResetWorkspace to replace the test copy on the next launch."
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
