[CmdletBinding()]
param(
    [string]$AuthorityRoot = 'J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan',
    [string]$OutputDirectory = '',
    [string]$Compiler = '',
    [string]$RunnerSource = '',
    [string]$ExecutableName = 'ntsd28_authority_source_capture.exe',
    [string]$ManifestName = 'build-manifest.json'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = [System.IO.Path]::GetFullPath(
    (Join-Path $PSScriptRoot '..\..'))
$authorityFullPath = [System.IO.Path]::GetFullPath($AuthorityRoot).TrimEnd('\')
if ([string]::IsNullOrWhiteSpace($OutputDirectory))
{
    $OutputDirectory = Join-Path $repositoryRoot 'Temp\NTSD28AuthorityTrace\build'
}
$outputFullPath = [System.IO.Path]::GetFullPath($OutputDirectory).TrimEnd('\')

if ($outputFullPath.Equals(
        $authorityFullPath,
        [System.StringComparison]::OrdinalIgnoreCase) -or
    $outputFullPath.StartsWith(
        $authorityFullPath + '\',
        [System.StringComparison]::OrdinalIgnoreCase))
{
    throw 'OutputDirectory must not be inside the authority root.'
}
if (-not ($outputFullPath.Equals(
            $repositoryRoot.TrimEnd('\'),
            [System.StringComparison]::OrdinalIgnoreCase) -or
        $outputFullPath.StartsWith(
            $repositoryRoot.TrimEnd('\') + '\',
            [System.StringComparison]::OrdinalIgnoreCase)))
{
    throw 'OutputDirectory must stay inside the current Unity repository.'
}

$formalExecutable = Join-Path $authorityFullPath 'NTSD2.8-Logan.exe'
$expectedFormalSha =
    'B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033'
if (-not (Test-Path -LiteralPath $formalExecutable -PathType Leaf))
{
    throw "Formal authority executable is missing: $formalExecutable"
}
$formalSha = (Get-FileHash -LiteralPath $formalExecutable -Algorithm SHA256).Hash
if (-not $formalSha.Equals(
        $expectedFormalSha,
        [System.StringComparison]::OrdinalIgnoreCase))
{
    throw "Formal authority executable SHA mismatch: $formalSha"
}

$sourceRoot = Join-Path $authorityFullPath 'source'
$coreRoot = Join-Path $sourceRoot 'ntsd28_core'
$playableRoot = Join-Path $sourceRoot 'ntsd28_playable'
if ([string]::IsNullOrWhiteSpace($RunnerSource))
{
    $RunnerSource = Join-Path $PSScriptRoot 'authority_source_capture_main.cpp'
}
$runnerSource = [System.IO.Path]::GetFullPath($RunnerSource)
if (-not (Test-Path -LiteralPath $runnerSource -PathType Leaf))
{
    throw "Capture runner source is missing: $runnerSource"
}
$repositoryPrefix = $repositoryRoot.TrimEnd('\') + '\'
if (-not $runnerSource.StartsWith(
        $repositoryPrefix,
        [System.StringComparison]::OrdinalIgnoreCase))
{
    throw 'RunnerSource must stay inside the current Unity repository.'
}
if ([System.IO.Path]::GetFileName($ExecutableName) -ne $ExecutableName -or
    [string]::IsNullOrWhiteSpace($ExecutableName))
{
    throw 'ExecutableName must be a non-empty leaf file name.'
}
if ([System.IO.Path]::GetFileName($ManifestName) -ne $ManifestName -or
    [string]::IsNullOrWhiteSpace($ManifestName))
{
    throw 'ManifestName must be a non-empty leaf file name.'
}

if ([string]::IsNullOrWhiteSpace($Compiler))
{
    $compilerCommand = Get-Command 'g++.exe' -ErrorAction SilentlyContinue
    if ($compilerCommand)
    {
        $Compiler = $compilerCommand.Source
    }
}
if ([string]::IsNullOrWhiteSpace($Compiler) -or
    -not (Test-Path -LiteralPath $Compiler -PathType Leaf))
{
    throw 'g++.exe was not found. Pass -Compiler or add it to PATH.'
}

$coreRelativeSources = @(
    'src\data\dat_document.cpp',
    'src\data\dat_parser.cpp',
    'src\data\fusion_catalog.cpp',
    'src\data\kind_catalog.cpp',
    'src\data\minibar_catalog.cpp',
    'src\data\object_catalog.cpp',
    'src\simulation\frame_machine.cpp',
    'src\simulation\frame_motion.cpp',
    'src\simulation\native_random.cpp',
    'src\simulation\native_ai.cpp',
    'src\simulation\input_routing.cpp',
    'src\simulation\physics_integrator.cpp',
    'src\simulation\collision_geometry.cpp',
    'src\simulation\hit_candidates.cpp',
    'src\simulation\hit_response.cpp',
    'src\simulation\combat_records.cpp',
    'src\simulation\damage_resolution.cpp',
    'src\simulation\defense_resolution.cpp',
    'src\simulation\armor_resolution.cpp',
    'src\simulation\object_spawning.cpp',
    'src\simulation\battle_world.cpp',
    'src\simulation\battle_flow.cpp',
    'src\simulation\simulation_tick_driver.cpp',
    'src\rendering\render_snapshot.cpp',
    'src\rendering\background_definition.cpp',
    'src\rendering\native_resource_catalog.cpp',
    'src\rendering\native_frame_hud.cpp',
    'src\rendering\native_combo_hud.cpp'
)
$authoritySources = @($coreRelativeSources | ForEach-Object {
    Join-Path $coreRoot $_
}) + @(
    (Join-Path $playableRoot 'src\game_session.cpp'),
    (Join-Path $playableRoot 'src\selection_flow.cpp'),
    (Join-Path $playableRoot 'src\scenario28.cpp')
)
foreach ($source in $authoritySources)
{
    if (-not (Test-Path -LiteralPath $source -PathType Leaf))
    {
        throw "Authority source is missing: $source"
    }
}

function Get-SourceManifestSha256
{
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Paths,
        [Parameter(Mandatory = $true)]
        [string]$RelativeRoot
    )

    $sha = [System.Security.Cryptography.SHA256]::Create()
    try
    {
        foreach ($path in ($Paths | Sort-Object))
        {
            $relative = [System.IO.Path]::GetRelativePath($RelativeRoot, $path).
                Replace('\', '/')
            $nameBytes = [System.Text.Encoding]::UTF8.GetBytes($relative)
            [void]$sha.TransformBlock(
                $nameBytes, 0, $nameBytes.Length, $nameBytes, 0)
            $separator = [byte[]]@(0)
            [void]$sha.TransformBlock($separator, 0, 1, $separator, 0)
            $fileBytes = [System.IO.File]::ReadAllBytes($path)
            [void]$sha.TransformBlock(
                $fileBytes, 0, $fileBytes.Length, $fileBytes, 0)
        }
        [void]$sha.TransformFinalBlock([byte[]]::new(0), 0, 0)
        return [System.Convert]::ToHexString($sha.Hash)
    }
    finally
    {
        $sha.Dispose()
    }
}

$authorityHeaders = @(
    Get-ChildItem -LiteralPath (Join-Path $coreRoot 'include') -Recurse -File |
        Where-Object { $_.Extension -in @('.h', '.hpp') } |
        ForEach-Object { $_.FullName }
) + @(
    Get-ChildItem -LiteralPath (Join-Path $playableRoot 'include') -Recurse -File |
        Where-Object { $_.Extension -in @('.h', '.hpp') } |
        ForEach-Object { $_.FullName }
)
$authorityManifestSha = Get-SourceManifestSha256 `
    -Paths ($authoritySources + $authorityHeaders) `
    -RelativeRoot $sourceRoot
$runnerSourceSha = (Get-FileHash -LiteralPath $runnerSource -Algorithm SHA256).Hash

New-Item -ItemType Directory -Force -Path $outputFullPath | Out-Null
$executable = Join-Path $outputFullPath $ExecutableName
$arguments = @(
    '-std=c++17',
    '-O2',
    '-Wall',
    '-Wextra',
    '-Wpedantic',
    '-municode',
    '-finput-charset=UTF-8',
    '-fexec-charset=UTF-8',
    ('-I' + (Join-Path $coreRoot 'include')),
    ('-I' + (Join-Path $playableRoot 'include'))
) + $authoritySources + @(
    $runnerSource,
    '-Wl,--wrap=_ZN6ntsd2814NativeRandom288crt_nextEv',
    '-Wl,--wrap=_ZN6ntsd2814NativeRandom2817synchronized_nextEji',
    '-static-libgcc',
    '-static-libstdc++',
    '-o',
    $executable
)

& $Compiler @arguments
if ($LASTEXITCODE -ne 0)
{
    throw "Authority source capture build failed with exit code $LASTEXITCODE"
}

$binarySha = (Get-FileHash -LiteralPath $executable -Algorithm SHA256).Hash
$manifest = [ordered]@{
    schema = 'ntsd28-authority-source-capture-build/1.0'
    evidenceClass = 'SOURCE_MODEL_DIAGNOSTIC_ONLY'
    formalExeSha256 = $formalSha
    authoritySourceManifestSha256 = $authorityManifestSha
    captureRunnerSourceSha256 = $runnerSourceSha
    captureBinarySha256 = $binarySha
    compiler = [System.IO.Path]::GetFullPath($Compiler)
    executable = $executable
    authorityWriteAllowed = $false
}
$manifestPath = Join-Path $outputFullPath $ManifestName
$manifest | ConvertTo-Json -Depth 4 |
    Set-Content -LiteralPath $manifestPath -Encoding utf8NoBOM

Write-Output "executable=$executable"
Write-Output "manifest=$manifestPath"
Write-Output "formalExeSha256=$formalSha"
Write-Output "authoritySourceManifestSha256=$authorityManifestSha"
Write-Output "captureRunnerSourceSha256=$runnerSourceSha"
Write-Output "captureBinarySha256=$binarySha"
