[CmdletBinding()]
param(
    [ValidateSet('Build', 'Smoke', 'Full', 'All')]
    [string]$Mode = 'Build',
    [string]$InputRoot = 'J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan\resources\runtime\decoded_dat',
    [string]$Compiler = '',
    [string]$SourceRoot = 'J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan\source'
)

$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$tempRoot = Join-Path $repoRoot 'Temp\NTSD28ContentAudit\native'
$artifactRoot = Join-Path $repoRoot 'artifacts\diagnostics\NTSD28-B11-CONTENT-ENTRY-INVENTORY-001\native'
$coreRoot = Join-Path $SourceRoot 'ntsd28_core'
$includeRoot = Join-Path $coreRoot 'include'
$captureSource = Join-Path $repoRoot 'Tools\NTSD28ContentAudit\AuthorityContentCapture.cpp'
$executable = Join-Path $tempRoot 'AuthorityContentCapture.exe'
$compileLog = Join-Path $artifactRoot 'compile.log'
$buildManifest = Join-Path $artifactRoot 'build-manifest.json'

New-Item -ItemType Directory -Force -Path $tempRoot, $artifactRoot | Out-Null

if ([string]::IsNullOrWhiteSpace($Compiler)) {
    $pathCompiler = Get-Command 'g++.exe' -ErrorAction SilentlyContinue
    if ($pathCompiler) {
        $Compiler = $pathCompiler.Source
    }
}
if ([string]::IsNullOrWhiteSpace($Compiler)) {
    $legacyCompiler = 'G:\GoggleDownload\x86_64-15.1.0-release-win32-seh-msvcrt-rt_v12-rev0\mingw64\bin\g++.exe'
    if (Test-Path -LiteralPath $legacyCompiler -PathType Leaf) {
        $Compiler = $legacyCompiler
    }
}
if ([string]::IsNullOrWhiteSpace($Compiler) -or
    -not (Test-Path -LiteralPath $Compiler -PathType Leaf)) {
    throw 'C++ compiler not found. Pass -Compiler or make g++.exe available.'
}
if (-not (Test-Path -LiteralPath $SourceRoot -PathType Container)) {
    throw "authority source root not found: $SourceRoot"
}
if (-not (Test-Path -LiteralPath $includeRoot -PathType Container)) {
    throw "authority include root not found: $includeRoot"
}
if (-not (Test-Path -LiteralPath $captureSource -PathType Leaf)) {
    throw "capture source not found: $captureSource"
}

$minimalSources = @(
    (Join-Path $coreRoot 'src\data\dat_document.cpp'),
    (Join-Path $coreRoot 'src\data\dat_parser.cpp'),
    (Join-Path $coreRoot 'src\simulation\combat_records.cpp'),
    (Join-Path $coreRoot 'src\simulation\object_spawning.cpp')
)
$fallbackSources = @(
    (Join-Path $coreRoot 'src\simulation\collision_geometry.cpp'),
    (Join-Path $coreRoot 'src\simulation\frame_motion.cpp'),
    (Join-Path $coreRoot 'src\simulation\physics_integrator.cpp'),
    (Join-Path $coreRoot 'src\simulation\frame_machine.cpp')
)
foreach ($source in $minimalSources + $fallbackSources) {
    if (-not (Test-Path -LiteralPath $source -PathType Leaf)) {
        throw "authority source file not found: $source"
    }
}

$compileFlags = @(
    '-std=c++17',
    '-O2',
    '-Wall',
    '-Wextra',
    '-Wpedantic',
    '-ffunction-sections',
    '-fdata-sections',
    '-finput-charset=UTF-8',
    '-fexec-charset=UTF-8',
    '-static-libgcc',
    '-static-libstdc++',
    '-Wl,--gc-sections',
    ('-I' + (Join-Path $coreRoot 'include'))
)

function Invoke-NativeCompile {
    param(
        [Parameter(Mandatory = $true)] [string[]]$Sources,
        [Parameter(Mandatory = $true)] [string]$LogPath
    )

    $arguments = @($compileFlags) + $Sources + @($captureSource, '-o', $executable)
    $stdoutPath = "$LogPath.stdout"
    $stderrPath = "$LogPath.stderr"
    $previousErrorAction = $ErrorActionPreference
    $ErrorActionPreference = 'SilentlyContinue'
    try {
        & $Compiler @arguments 1> $stdoutPath 2> $stderrPath
    } finally {
        $ErrorActionPreference = $previousErrorAction
    }
    $exitCode = $LASTEXITCODE
    if (Test-Path -LiteralPath $stdoutPath) {
        Get-Content -LiteralPath $stdoutPath | Set-Content -LiteralPath $LogPath -Encoding utf8
    } else {
        New-Item -ItemType File -Force -Path $LogPath | Out-Null
    }
    if (Test-Path -LiteralPath $stderrPath) {
        Get-Content -LiteralPath $stderrPath | Add-Content -LiteralPath $LogPath -Encoding utf8
    }
    Get-Content -LiteralPath $LogPath | Write-Host
    return $exitCode
}

$exitCode = Invoke-NativeCompile -Sources $minimalSources -LogPath $compileLog
$selectedSources = $minimalSources
if ($exitCode -ne 0) {
    Write-Host '[build] minimal closure failed; retrying with read-only geometry/frame closure.'
    $selectedSources = $minimalSources + $fallbackSources
    $exitCode = Invoke-NativeCompile -Sources $selectedSources -LogPath $compileLog
}
if ($exitCode -ne 0) {
    throw "AuthorityContentCapture compilation failed with exit code $exitCode. See $compileLog"
}

$compilerVersion = (& $Compiler '--version' | Select-Object -First 1)
$sourceManifest = foreach ($source in $selectedSources + @($captureSource)) {
    $hash = Get-FileHash -LiteralPath $source -Algorithm SHA256
    [ordered]@{
        path = $source
        sha256 = $hash.Hash
        bytes = (Get-Item -LiteralPath $source).Length
    }
}
$conservativeHeaders = Get-ChildItem -LiteralPath $includeRoot -File -Recurse |
    Where-Object { $_.Extension.ToLowerInvariant() -in @('.h', '.hpp') } |
    Sort-Object FullName |
    ForEach-Object {
        $hash = Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256
        [ordered]@{
            path = $_.FullName
            sha256 = $hash.Hash
            bytes = $_.Length
        }
    }
$compilerHash = Get-FileHash -LiteralPath $Compiler -Algorithm SHA256
$manifest = [ordered]@{
    sourceModel = 'SOURCE_MODEL_DIAGNOSTIC_ONLY'
    closure = if ($selectedSources.Count -eq $minimalSources.Count) { 'minimal' } else { 'fallback-required-collision-geometry' }
    authoritySourceRoot = $SourceRoot
    authorityIncludeRoot = $includeRoot
    conservativeHeaderCount = @($conservativeHeaders).Count
    conservativeHeaders = @($conservativeHeaders)
    compiler = $Compiler
    compilerVersion = $compilerVersion
    compilerSha256 = $compilerHash.Hash
    compileFlags = $compileFlags
    sources = @($sourceManifest)
    executable = $executable
    executableSha256 = (Get-FileHash -LiteralPath $executable -Algorithm SHA256).Hash
    generatedUtc = [DateTime]::UtcNow.ToString('o')
}
$manifest | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $buildManifest -Encoding utf8
Write-Host "[build] complete: $executable"
Write-Host "[build] manifest: $buildManifest"

function Get-RelativeDatPath {
    param([Parameter(Mandatory = $true)] [string]$Root, [Parameter(Mandatory = $true)] [string]$Path)
    $rootFull = [System.IO.Path]::GetFullPath($Root)
    $pathFull = [System.IO.Path]::GetFullPath($Path)
    $rootUri = [System.Uri]::new($rootFull.TrimEnd('\') + '\')
    $pathUri = [System.Uri]::new($pathFull)
    $relative = $rootUri.MakeRelativeUri($pathUri).ToString()
    return [System.Uri]::UnescapeDataString($relative).Replace('\', '/')
}

function Write-DatHashes {
    param(
        [Parameter(Mandatory = $true)] [string]$Root,
        [Parameter(Mandatory = $true)] [string]$Path
    )
    $records = Get-ChildItem -LiteralPath $Root -File -Recurse -Filter '*.dat' |
        ForEach-Object {
            $hash = Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256
            [ordered]@{
                path = Get-RelativeDatPath -Root $Root -Path $_.FullName
                sha256 = $hash.Hash
                bytes = $_.Length
            }
        } |
        Sort-Object path
    $records | ForEach-Object { $_ | ConvertTo-Json -Compress } |
        Set-Content -LiteralPath $Path -Encoding utf8
    return @($records).Count
}

function Invoke-Capture {
    param(
        [Parameter(Mandatory = $true)] [string]$Root,
        [Parameter(Mandatory = $true)] [string]$OutputPath,
        [Parameter(Mandatory = $true)] [string]$LogPath
    )
    $stdoutPath = "$LogPath.stdout"
    $stderrPath = "$LogPath.stderr"
    $previousErrorAction = $ErrorActionPreference
    $ErrorActionPreference = 'SilentlyContinue'
    try {
        & $executable '--input-root' $Root '--output' $OutputPath 1> $stdoutPath 2> $stderrPath
    } finally {
        $ErrorActionPreference = $previousErrorAction
    }
    $exit = $LASTEXITCODE
    if (Test-Path -LiteralPath $stdoutPath) {
        Get-Content -LiteralPath $stdoutPath | Set-Content -LiteralPath $LogPath -Encoding utf8
    } else {
        New-Item -ItemType File -Force -Path $LogPath | Out-Null
    }
    if (Test-Path -LiteralPath $stderrPath) {
        Get-Content -LiteralPath $stderrPath | Add-Content -LiteralPath $LogPath -Encoding utf8
    }
    Get-Content -LiteralPath $LogPath | Write-Host
    return $exit
}

function New-SmokeDat {
    $smokeRoot = Join-Path $tempRoot 'sample-input'
    New-Item -ItemType Directory -Force -Path $smokeRoot | Out-Null
    $sample = @'
<bmp_begin>
name: sample
file(0-1): sample.bmp w: 1 h: 1 row: 1 col: 2
<bmp_end>
<frame> 0 capture_sample
centerx: 0 centery: 0
cpoint:
kind: 1 x: 2 y: 3 injury: 4 cover: 5 vaction: 6 aaction: 7 jaction: 8 daction: 9 taction: 10 faction: 11 baction: 12 uzaction: 13 dzaction: 14 throwvx: 1.5 throwvy: -2.5 hurtable: 1 fronthurtact: 15 backhurtact: 16 decrease: 17 dircontrol: 18 throwinjury: 19 throwvz: 2.25 z: 20 recover: 21 drain: 22 gain: 23
gain: 23 unknown_capture_field: 77
cpoint_end:
opoint:
kind: 1 x: 2 y: 3 z: 4 action: 5 dvx: 6 dvy: 7 dvz: 8 oid: 9 facing: 10 hp: 11 mp: 12 team: 13 reserve: 14 effect: 15 pic: 16 centerx: 17 centery: 18 centerz: 19 framea: 20 attacking: 21 join: 22 join_reserve: 23 join_pic: 24
opoint_end:
wpoint:
kind: 1 x: 2 y: 3 weaponact: 4 attacking: 5 cover: 6 dvx: 7 dvy: 8 dvz: 9
wpoint_end:
itr:
kind: 1 x: 2 y: 3 w: 4 h: 5 dvx: 6 dvy: 7 fall: 8 arest: 9 vrest: 10 respond: 11 effect: 12 drain: 13 spark: 14 recover: 15 dbdefend: 16 bdefend: 17 injury: 18 zwidth: 19 z: 20 dvz: 21 sound: 22 cover: 23 caughtact: 24 25 catchingact: 26 27 pickedact: 28 29 pickingact: 30 31 delay: 32 poison: 33 confus: 34 weak: 35 manacle: 36 join: 37 mimic: 38 bound: 39 facing: 40 dx: 41 dy: 42 dz: 43 gain: 44
itr_end:
bdy:
kind: 0 x: 1 y: 2 w: 3 h: 4 zwidth: 5 z: 6
bdy_end:
<frame_end>
'@
    $samplePath = Join-Path $smokeRoot 'sample.dat'
    $sample | Set-Content -LiteralPath $samplePath -Encoding utf8
    return $smokeRoot
}

function Assert-SmokeCapture {
    param([Parameter(Mandatory = $true)] [string]$Path)
    $row = Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
    if (-not $row.parseSuccess) {
        throw "fixture parseSuccess=false: $Path"
    }
    $frames = @($row.frames)
    if ($frames.Count -ne 1 -or $frames[0].id -ne 0) {
        throw "fixture frame projection is padded or incomplete: frameCount=$($frames.Count)"
    }
    $subblocks = @($frames[0].subblocks)
    $cpoint = @($subblocks | Where-Object { $_.kind -eq 'cpoint' })
    $opoint = @($subblocks | Where-Object { $_.kind -eq 'opoint' })
    if ($cpoint.Count -ne 1 -or $opoint.Count -ne 1) {
        throw 'fixture CPoint/OPoint block count mismatch'
    }
    $cpointKeys = @($cpoint[0].normalized.psobject.Properties.Name)
    $opointKeys = @($opoint[0].normalized.psobject.Properties.Name)
    if ($cpointKeys.Count -ne 27 -or $opointKeys.Count -ne 24) {
        throw "fixture normalized field counts mismatch: cpoint=$($cpointKeys.Count), opoint=$($opointKeys.Count)"
    }
    if ($cpoint[0].normalized.throwvx -ne 1.5 -or
        $cpoint[0].normalized.throwvz -ne 2.25 -or
        $cpoint[0].normalized.gain -ne 23 -or
        $opoint[0].normalized.oid -ne 9 -or
        $opoint[0].normalized.join_pic -ne 24) {
        throw 'fixture normalized CPoint/OPoint value assertion failed'
    }
    $gainFields = @($cpoint[0].fields | Where-Object { $_.key -eq 'gain' })
    $unknownFields = @($cpoint[0].fields | Where-Object { $_.key -eq 'unknown_capture_field' })
    if ($gainFields.Count -ne 2 -or $unknownFields.Count -ne 1) {
        throw 'fixture duplicate or unknown raw field preservation failed'
    }
    if (@($subblocks | Where-Object { $_.conversionError }).Count -ne 0) {
        throw 'fixture normalized conversionError is non-empty'
    }
    Write-Host "[assert] fixture PASS: declaredFrames=$($frames.Count) cpoint=$($cpointKeys.Count) opoint=$($opointKeys.Count) duplicateRaw=$($gainFields.Count)"
    return [ordered]@{
        output = $Path
        sha256 = (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash
        declaredFrames = $frames.Count
        cpointFields = $cpointKeys.Count
        opointFields = $opointKeys.Count
        duplicateRawGainFields = $gainFields.Count
        unknownRawFields = $unknownFields.Count
    }
}

function Assert-NarCapture {
    param([Parameter(Mandatory = $true)] [string]$Path)
    $row = Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
    if (-not $row.parseSuccess -or $row.path -ne 'nar.dat' -or @($row.frames).Count -le 0) {
        throw "nar.dat smoke assertion failed: $Path"
    }
    Write-Host "[assert] nar.dat PASS: declaredFrames=$(@($row.frames).Count) diagnostics=$(@($row.diagnostics).Count)"
    return [ordered]@{
        output = $Path
        sha256 = (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash
        declaredFrames = @($row.frames).Count
        diagnostics = @($row.diagnostics).Count
    }
}

if ($Mode -eq 'Smoke' -or $Mode -eq 'All') {
    $smokeRoot = New-SmokeDat
    $smokeOutput = Join-Path $tempRoot 'smoke-content.jsonl'
    $smokeLog = Join-Path $artifactRoot 'smoke-cpoint-opoint.log'
    $smokeExit = Invoke-Capture -Root $smokeRoot -OutputPath $smokeOutput -LogPath $smokeLog
    if ($smokeExit -ne 0) {
        throw "fixture capture failed with exit code $smokeExit. See $smokeLog"
    }
    $fixtureReceipt = Assert-SmokeCapture -Path $smokeOutput

    $narSource = Join-Path $InputRoot 'c\nar\nar.dat'
    if (-not (Test-Path -LiteralPath $narSource -PathType Leaf)) {
        throw "formal nar.dat not found: $narSource"
    }
    $narRoot = Join-Path $tempRoot 'nar-sample-input'
    New-Item -ItemType Directory -Force -Path $narRoot | Out-Null
    Copy-Item -LiteralPath $narSource -Destination (Join-Path $narRoot 'nar.dat') -Force
    $narOutput = Join-Path $tempRoot 'nar-content.jsonl'
    $narLog = Join-Path $artifactRoot 'smoke-nar.log'
    $narExit = Invoke-Capture -Root $narRoot -OutputPath $narOutput -LogPath $narLog
    if ($narExit -ne 0) {
        throw "nar.dat capture failed with exit code $narExit. See $narLog"
    }
    $narReceipt = Assert-NarCapture -Path $narOutput
    [ordered]@{
        sourceModel = 'SOURCE_MODEL_DIAGNOSTIC_ONLY'
        fixture = $fixtureReceipt
        nar = $narReceipt
        generatedUtc = [DateTime]::UtcNow.ToString('o')
    } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $artifactRoot 'smoke-receipt.json') -Encoding utf8
}

if ($Mode -eq 'Full' -or $Mode -eq 'All') {
    if (-not (Test-Path -LiteralPath $InputRoot -PathType Container)) {
        throw "decoded DAT input root not found: $InputRoot"
    }
    $hashBefore = Join-Path $artifactRoot 'authority-input-sha256-before.jsonl'
    $datCount = Write-DatHashes -Root $InputRoot -Path $hashBefore
    Write-Host "[capture] DAT inputs: $datCount"

    $fullOutput = Join-Path $artifactRoot 'authority-content.jsonl'
    $fullLog = Join-Path $artifactRoot 'authority-content.log'
    $fullExit = Invoke-Capture -Root $InputRoot -OutputPath $fullOutput -LogPath $fullLog
    Write-Host "[capture] first export exit code (parse diagnostics are retained): $fullExit"

    $rerunOutput = Join-Path $artifactRoot 'authority-content-rerun.jsonl'
    $rerunLog = Join-Path $artifactRoot 'authority-content-rerun.log'
    $rerunExit = Invoke-Capture -Root $InputRoot -OutputPath $rerunOutput -LogPath $rerunLog
    Write-Host "[capture] rerun export exit code (parse diagnostics are retained): $rerunExit"

    $hashAfter = Join-Path $artifactRoot 'authority-input-sha256-after.jsonl'
    [void](Write-DatHashes -Root $InputRoot -Path $hashAfter)
    $outputHash = Get-FileHash -LiteralPath $fullOutput -Algorithm SHA256
    $rerunHash = Get-FileHash -LiteralPath $rerunOutput -Algorithm SHA256
    $stability = [ordered]@{
        sourceModel = 'SOURCE_MODEL_DIAGNOSTIC_ONLY'
        datCount = $datCount
        firstOutput = $fullOutput
        firstSha256 = $outputHash.Hash
        rerunOutput = $rerunOutput
        rerunSha256 = $rerunHash.Hash
        stable = ($outputHash.Hash -eq $rerunHash.Hash)
        inputHashesBefore = $hashBefore
        inputHashesAfter = $hashAfter
        generatedUtc = [DateTime]::UtcNow.ToString('o')
    }
    $stability | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $artifactRoot 'capture-stability.json') -Encoding utf8
    Write-Host "[capture] output SHA256: $($outputHash.Hash)"
    Write-Host "[capture] rerun SHA256: $($rerunHash.Hash)"
    Write-Host "[capture] stable: $($stability.stable)"
}
