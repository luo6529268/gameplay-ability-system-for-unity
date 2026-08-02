[CmdletBinding()]
param(
    [string[]]$WitnessId,
    [switch]$ExecutableOnly,
    [switch]$ValidateOnly,
    [switch]$DryRun,
    [string]$UnityExe = $env:UNITY_EXE,
    [string]$ProjectPath,
    [string]$AuthorityAssembly,
    [string]$AuthorityGameRoot = $env:NTSD_AUTHORITY_GAME_ROOT,
    [string]$OutputRoot,
    [ValidateRange(1, 3600)]
    [int]$UnityTimeoutSeconds = 300
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Resolve-FullPath {
    param([Parameter(Mandatory = $true)][string]$Path, [Parameter(Mandatory = $true)][string]$BasePath)
    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }
    return [System.IO.Path]::GetFullPath((Join-Path $BasePath $Path))
}

function Resolve-RepositoryRelativePath {
    param(
        [Parameter(Mandatory = $true)][string]$RelativePath,
        [Parameter(Mandatory = $true)][string]$RepositoryRoot
    )

    if ([System.IO.Path]::IsPathRooted($RelativePath)) {
        throw "Manifest paths must be repository-relative: $RelativePath"
    }

    $candidate = Resolve-FullPath -Path $RelativePath -BasePath $RepositoryRoot
    $rootWithSeparator = $RepositoryRoot.TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
    if (-not $candidate.StartsWith($rootWithSeparator, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Manifest path escapes repository root: $RelativePath"
    }
    return $candidate
}

function ConvertTo-ProcessArgument {
    param([Parameter(Mandatory = $true)][string]$Value)
    return '"' + $Value.Replace('"', '\"') + '"'
}

function Assert-NoUnityEditorIsRunning {
    $unityProcesses = @(Get-CimInstance Win32_Process -Filter "Name = 'Unity.exe'" -ErrorAction Stop)
    if ($unityProcesses.Count -gt 0) {
        $processIds = ($unityProcesses | ForEach-Object { $_.ProcessId }) -join ', '
        throw "Refusing to start a second Unity Editor. Existing Unity.exe process id(s): $processIds. Close it and retry."
    }
}

function Get-UnityTraceProcessOutcome {
    param(
        [Parameter(Mandatory = $true)][System.Diagnostics.Process]$Process,
        [Parameter(Mandatory = $true)][string]$TracePath,
        [Parameter(Mandatory = $true)][string]$LogPath
    )

    # Windows PowerShell 5 can surface a completed Start-Process object whose
    # ExitCode remains null until the underlying handle is refreshed.
    $Process.Refresh()
    $exitCode = $null
    try {
        $exitCode = $Process.ExitCode
    }
    catch {
        $exitCode = $null
    }

    if ($null -ne $exitCode) {
        if ($exitCode -ne 0) {
            throw "Unity trace exporter failed with exit code $exitCode. See $LogPath"
        }
        return [ordered]@{ exitCode = 0; exitCodeSource = 'process' }
    }

    if (-not $Process.HasExited) {
        throw 'Unity trace exporter did not exit after WaitForExit completed.'
    }

    $traceWritten = (Test-Path -LiteralPath $TracePath -PathType Leaf) -and
        ((Get-Item -LiteralPath $TracePath).Length -gt 0)
    $logReportsSuccess = (Test-Path -LiteralPath $LogPath -PathType Leaf) -and
        [bool](Select-String -LiteralPath $LogPath -SimpleMatch '[BattleParityTraceEditor] Trace written:' -Quiet)
    if ($traceWritten -and $logReportsSuccess) {
        return [ordered]@{ exitCode = $null; exitCodeSource = 'trace-and-log-fallback' }
    }

    throw "Unity trace exporter exited but did not expose an exit code, and its success artifacts are incomplete. See $LogPath"
}

function Write-Summary {
    param(
        [Parameter(Mandatory = $true)][System.Collections.IDictionary]$Summary,
        [Parameter(Mandatory = $true)][string]$Path
    )
    $Summary.completedAtUtc = [DateTime]::UtcNow.ToString('o')
    $Summary | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $Path -Encoding utf8
}

$scriptDirectory = Split-Path -Parent $PSCommandPath
$repositoryRoot = Resolve-FullPath -Path '../..' -BasePath $scriptDirectory
$manifestPath = Join-Path $scriptDirectory 'authority400-witness-manifest.v1.json'
$parityProject = Join-Path $scriptDirectory 'NTSDParity.csproj'

if ([string]::IsNullOrWhiteSpace($ProjectPath)) {
    $ProjectPath = $repositoryRoot
}
$ProjectPath = Resolve-FullPath -Path $ProjectPath -BasePath $repositoryRoot
if (-not (Test-Path -LiteralPath (Join-Path $ProjectPath 'Assets'))) {
    throw "ProjectPath is not a Unity project root: $ProjectPath"
}

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $repositoryRoot '.omc/validation/authority400-witness'
}
$OutputRoot = Resolve-FullPath -Path $OutputRoot -BasePath $repositoryRoot

$summary = [ordered]@{
    schema = 'ntsd-authority400-witness-summary.v1'
    certificateEligible = $false
    evidenceClass = 'diagnostic-witness-only'
    manifestPath = $manifestPath
    projectPath = $ProjectPath
    outputRoot = $OutputRoot
    validateOnly = [bool]$ValidateOnly
    dryRun = [bool]$DryRun
    startedAtUtc = [DateTime]::UtcNow.ToString('o')
    results = @()
}

$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
if ($manifest.schema -ne 'ntsd-authority400-witness-manifest.v1') {
    throw "Unexpected witness manifest schema: $($manifest.schema)"
}
if ($manifest.profile.id -ne 'Authority400' -or $manifest.profile.runtimeSlotCount -ne 400) {
    throw 'Manifest profile must be Authority400 with exactly 400 runtime slots.'
}
if ((@($manifest.traceSchemas) -join ',') -ne 'ntsd-battle-trace-v3,ntsd-battle-trace-v4') {
    throw 'Manifest traceSchemas must list v3 then v4.'
}
if ($manifest.defaultComparison.detail -ne 'full' -or $manifest.defaultComparison.profile -notin @('strict', 'fixed-world-camera') -or
    $manifest.defaultComparison.dataFixture -ne 'authority-dat-diagnostic' -or
    $manifest.defaultComparison.allowDiagnostic -ne $true -or
    $manifest.defaultComparison.requireCertificate -ne $false) {
    throw 'Manifest defaultComparison must be full diagnostic comparison with certificates disabled.'
}

$expectedContracts = [ordered]@{
    C01 = 'pass order'
    C02 = 'live ascending scan'
    C03 = 'high-slot newborn same pass'
    C04 = 'low-slot reuse next pass'
    C05 = 'Transit->TU->snapshot'
    C06 = 'character hit/random drop/object hit consume order'
    C07 = 'late state/recovery/frame/collision/opoint/tail'
    C08 = 'segmented opoint flush'
    C09 = 'free/deferred unregister/generation'
    C10 = 'RNG state/calls/ascending consumption'
    C11 = 'holder/target/link'
    C12 = 'presentation no-writeback'
    C13 = 'cursor-local opoint immediate/high-slot visibility'
    C14 = 'allocator bands/start search'
}
$contracts = @($manifest.contracts)
if ($contracts.Count -ne $expectedContracts.Count) {
    throw 'Manifest must define C01 through C14 contract descriptions exactly once.'
}
foreach ($contractId in $expectedContracts.Keys) {
    $contract = @($contracts | Where-Object { $_.id -eq $contractId })
    if ($contract.Count -ne 1 -or $contract[0].description -ne $expectedContracts[$contractId]) {
        throw "Manifest contract $contractId is missing or differs from the Slice 0 plan."
    }
}

$allWitnesses = @($manifest.witnesses)
if (@($allWitnesses | Select-Object -ExpandProperty id | Sort-Object -Unique).Count -ne $allWitnesses.Count) {
    throw 'Manifest has duplicate witness ids.'
}
if (@($allWitnesses | Select-Object -ExpandProperty id | Sort-Object) -join ',' -ne 'W01,W02,W03,W04,W05,W06,W07,W08') {
    throw 'Manifest must define W01 through W08 exactly once.'
}

$coverage = @($manifest.coverage)
if ($coverage.Count -ne 14 -or (@($coverage | Select-Object -ExpandProperty contract | Sort-Object) -join ',') -ne 'C01,C02,C03,C04,C05,C06,C07,C08,C09,C10,C11,C12,C13,C14') {
    throw 'Manifest must define coverage for C01 through C14 exactly once.'
}
$expectedCoverage = [ordered]@{
    C01 = @('W01', 'partial-observable-edge')
    C02 = @('W03', 'diagnostic-source-callchain')
    C03 = @('W03', 'diagnostic-source-callchain')
    C04 = @('W03', 'diagnostic-source-callchain')
    C05 = @('W02', 'partial-observable-edge')
    C06 = @('W06', 'planned')
    C07 = @('W05', 'planned')
    C08 = @('W05', 'planned')
    C09 = @('W03', 'diagnostic-source-callchain')
    C10 = @('W01', 'trace-direct')
    C11 = @('W07', 'diagnostic-source-callchain/partial')
    C12 = @('W08', 'planned')
    C13 = @('W05', 'planned')
    C14 = @('W04', 'diagnostic-source-callchain')
}

foreach ($witness in $allWitnesses) {
    if ($witness.status -in @('current-v3-runnable', 'current-v4-runnable')) {
        if ([string]::IsNullOrWhiteSpace($witness.scenario)) {
            throw "Runnable witness $($witness.id) has no scenario."
        }
        $scenarioPath = Resolve-RepositoryRelativePath -RelativePath $witness.scenario -RepositoryRoot $repositoryRoot
        if (-not (Test-Path -LiteralPath $scenarioPath -PathType Leaf)) {
            throw "Runnable witness scenario is missing: $($witness.scenario)"
        }
        $scenario = Get-Content -LiteralPath $scenarioPath -Raw | ConvertFrom-Json
        if ($scenario.gameRoot -ne '${AUTHORITY_GAME_ROOT}') {
            throw "Scenario $($witness.scenario) must use the portable `${AUTHORITY_GAME_ROOT} token."
        }
    }
    elseif ($witness.status -ne 'requires-v4-structural-events/source-callchain-plus-focused-test') {
        throw "Witness $($witness.id) has an unsupported status: $($witness.status)"
    }
}

foreach ($entry in $coverage) {
    $witness = @($allWitnesses | Where-Object { $_.id -eq $entry.witness })
    $expected = $expectedCoverage[$entry.contract]
    if ($witness.Count -ne 1 -or $entry.contract -notin @($witness[0].covers) -or
        $null -eq $expected -or $entry.witness -ne $expected[0] -or
        $entry.coverageLevel -ne $expected[1] -or $entry.status -ne $expected[1]) {
        throw "Coverage $($entry.contract) does not match its witness declaration."
    }
}

if ($ValidateOnly) {
    $summary.status = 'validated'
    $summary.results = @($allWitnesses | ForEach-Object {
            [ordered]@{
                id = $_.id
                status = $_.status
                selected = $false
                compare = [ordered]@{
                    diagnosticComparison = $true
                    certificateEligible = $false
                    certificateClass = 'diagnostic-witness-only'
                }
            }
        })
    if (-not $DryRun) {
        New-Item -ItemType Directory -Path $OutputRoot -Force | Out-Null
        $summaryPath = Join-Path $OutputRoot 'summary.validate-only.json'
        Write-Summary -Summary $summary -Path $summaryPath
        Write-Output $summaryPath
    }
    return
}

$selectedWitnesses = $allWitnesses
if ($WitnessId -and $WitnessId.Count -gt 0) {
    $selectedWitnesses = @($allWitnesses | Where-Object { $_.id -in $WitnessId })
    if ($selectedWitnesses.Count -ne $WitnessId.Count) {
        $known = @($allWitnesses | Select-Object -ExpandProperty id) -join ', '
        throw "Unknown witness id requested. Known ids: $known"
    }
}
elseif ($ExecutableOnly -or -not $WitnessId) {
    $selectedWitnesses = @($allWitnesses | Where-Object { $_.status -in @('current-v3-runnable', 'current-v4-runnable') })
}

$nonRunnable = @($selectedWitnesses | Where-Object { $_.status -notin @('current-v3-runnable', 'current-v4-runnable') })
if ($nonRunnable.Count -gt 0) {
    $blocked = ($nonRunnable | ForEach-Object { "$($_.id) ($($_.status))" }) -join ', '
    throw "Selected witness(es) are not executable: $blocked"
}
if ($DryRun) {
    $summary.status = 'dry-run'
    $summary.results = @($selectedWitnesses | ForEach-Object {
            [ordered]@{
                id = $_.id
                status = 'would-run'
                scenario = $_.scenario
                compare = [ordered]@{
                    diagnosticComparison = $true
                    certificateEligible = $false
                    certificateClass = 'diagnostic-witness-only'
                }
            }
        })
    Write-Output ($summary | ConvertTo-Json -Depth 12)
    return
}
if ([string]::IsNullOrWhiteSpace($AuthorityGameRoot)) {
    throw 'AuthorityGameRoot is required to materialize portable scenarios. Pass -AuthorityGameRoot or set NTSD_AUTHORITY_GAME_ROOT.'
}
$AuthorityGameRoot = Resolve-FullPath -Path $AuthorityGameRoot -BasePath $repositoryRoot
if (-not (Test-Path -LiteralPath (Join-Path $AuthorityGameRoot 'data/data.txt') -PathType Leaf)) {
    throw "AuthorityGameRoot does not contain data/data.txt: $AuthorityGameRoot"
}
if ([string]::IsNullOrWhiteSpace($UnityExe)) {
    throw 'UnityExe is required. Pass -UnityExe or set UNITY_EXE.'
}
$UnityExe = Resolve-FullPath -Path $UnityExe -BasePath $repositoryRoot
if (-not (Test-Path -LiteralPath $UnityExe -PathType Leaf)) {
    throw "Unity executable was not found: $UnityExe"
}

Assert-NoUnityEditorIsRunning
New-Item -ItemType Directory -Path $OutputRoot -Force | Out-Null

$buildArguments = @('build', $parityProject, '--nologo')
if (-not [string]::IsNullOrWhiteSpace($AuthorityAssembly)) {
    $AuthorityAssembly = Resolve-FullPath -Path $AuthorityAssembly -BasePath $repositoryRoot
    if (-not (Test-Path -LiteralPath $AuthorityAssembly -PathType Leaf)) {
        throw "AuthorityAssembly was not found: $AuthorityAssembly"
    }
    $buildArguments += "--property:AuthorityAssembly=$AuthorityAssembly"
}
& dotnet @buildArguments
if ($LASTEXITCODE -ne 0) {
    throw "NTSDParity build failed with exit code $LASTEXITCODE."
}

foreach ($witness in $selectedWitnesses) {
    $witnessDirectory = Join-Path $OutputRoot $witness.id
    New-Item -ItemType Directory -Path $witnessDirectory -Force | Out-Null
    $sourceScenarioPath = Resolve-RepositoryRelativePath -RelativePath $witness.scenario -RepositoryRoot $repositoryRoot
    $resolvedScenarioPath = Join-Path $witnessDirectory 'scenario.resolved.json'
    $scenarioText = Get-Content -LiteralPath $sourceScenarioPath -Raw
    $scenarioText = $scenarioText.Replace('${AUTHORITY_GAME_ROOT}', $AuthorityGameRoot.Replace('\', '\\'))
    Set-Content -LiteralPath $resolvedScenarioPath -Value $scenarioText -Encoding utf8

    $authorityTrace = Join-Path $witnessDirectory 'authority.trace.jsonl'
    $unityTrace = Join-Path $witnessDirectory 'unity.trace.jsonl'
    $comparison = Join-Path $witnessDirectory 'comparison.json'
    $unityLog = Join-Path $witnessDirectory 'unity.log'
    $result = [ordered]@{
        id = $witness.id
        scenario = $witness.scenario
        status = 'running'
        outputDirectory = $witnessDirectory
        authorityTrace = $authorityTrace
        unityTrace = $unityTrace
        compare = [ordered]@{
            output = $comparison
            detail = 'full'
            diagnosticComparison = $true
            certificateEligible = $false
            certificateClass = 'diagnostic-witness-only'
        }
    }

    try {
        $runArguments = @('run', '--no-build', '--project', $parityProject)
        if (-not [string]::IsNullOrWhiteSpace($AuthorityAssembly)) {
            $runArguments += "--property:AuthorityAssembly=$AuthorityAssembly"
        }
        $runArguments += @('--', 'trace-authority', '--scenario', $resolvedScenarioPath, '--output', $authorityTrace, '--detail', $manifest.defaultComparison.detail)
        & dotnet @runArguments
        if ($LASTEXITCODE -ne 0) {
            throw "Authority exporter failed with exit code $LASTEXITCODE."
        }

        $unityArguments = @(
            '-batchmode', '-nographics', '-projectPath', $ProjectPath,
            '-executeMethod', 'NTSD.EditorTools.BattleParityTraceEditor.RunFromCommandLine',
            '-ntsdParityScenario', $resolvedScenarioPath,
            '-ntsdParityOutput', $unityTrace,
            '-ntsdParityDetail', $manifest.defaultComparison.detail,
            '-ntsdParityDataFixture', $manifest.defaultComparison.dataFixture,
            '-logFile', $unityLog
        )
        $processArguments = ($unityArguments | ForEach-Object { ConvertTo-ProcessArgument -Value $_ }) -join ' '
        $unityProcess = Start-Process -FilePath $UnityExe -ArgumentList $processArguments -PassThru -NoNewWindow
        if (-not $unityProcess.WaitForExit($UnityTimeoutSeconds * 1000)) {
            $unityProcess.Kill()
            throw "Unity timed out after $UnityTimeoutSeconds seconds."
        }
        $result.unityExporter = Get-UnityTraceProcessOutcome -Process $unityProcess -TracePath $unityTrace -LogPath $unityLog

        & dotnet run --no-build --project $parityProject -- compare --authority $authorityTrace --unity $unityTrace --output $comparison --detail full --profile $manifest.defaultComparison.profile --allow-diagnostic
        $comparisonExitCode = $LASTEXITCODE
        $comparisonPayload = if (Test-Path -LiteralPath $comparison) { Get-Content -LiteralPath $comparison -Raw | ConvertFrom-Json } else { $null }
        $result.compare.exitCode = $comparisonExitCode
        $result.compare.status = if ($comparisonPayload) { $comparisonPayload.status } else { 'missing-report' }
        $result.status = if ($comparisonExitCode -eq 0) { 'completed' } else { 'difference-or-failure' }
    }
    catch {
        $result.status = 'failed'
        $result.error = $_.Exception.Message
    }
    $summary.results += $result
}

$summary.status = if (@($summary.results | Where-Object { $_.status -ne 'completed' }).Count -eq 0) { 'completed' } else { 'completed-with-failures' }
$summaryPath = Join-Path $OutputRoot 'summary.json'
Write-Summary -Summary $summary -Path $summaryPath
Write-Output $summaryPath
if ($summary.status -ne 'completed') {
    exit 1
}
