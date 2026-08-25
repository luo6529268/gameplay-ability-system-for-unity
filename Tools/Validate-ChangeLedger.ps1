# Alignment contract: OPS-TRACE-001.
# Read-only validator for repository-local script change records.
[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Split-Path -Parent $PSScriptRoot),
    [switch]$StagedOnly,
    [switch]$SkipUntracked,
    [string]$SimulateChangedPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$AllowedStatuses = @(
    'PLANNED',
    'IN_PROGRESS',
    'CODE_WRITTEN',
    'COMPILE_PASS',
    'FOCUSED_TEST_PASS',
    'RUNTIME_PENDING',
    'VERIFIED',
    'BLOCKED',
    'ABANDONED',
    'ROLLED_BACK',
    'SUPERSEDED'
)

$ActiveStatuses = @(
    'PLANNED',
    'IN_PROGRESS',
    'CODE_WRITTEN',
    'COMPILE_PASS',
    'FOCUSED_TEST_PASS',
    'RUNTIME_PENDING',
    'BLOCKED'
)

$ScriptExtensions = @(
    '.cs',
    '.shader',
    '.hlsl',
    '.compute',
    '.cginc',
    '.ps1',
    '.py',
    '.js',
    '.ts',
    '.tsx',
    '.jsx'
)

$GovernedRoots = @(
    'Assets/NTSD/Scripts/',
    'Tools/'
)

$ExcludedPrefixes = @(
    'Assets/NTSD/Scripts/Gen/',
    'Assets/Plugins/',
    'Tools/DatSkillFlowWeb/dist/'
)

function Normalize-RepoPath
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $normalized = $Path.Trim().Replace('\', '/')
    while ($normalized.StartsWith('./', [System.StringComparison]::Ordinal))
    {
        $normalized = $normalized.Substring(2)
    }

    return $normalized
}

function Test-GovernedCodePath
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $normalized = Normalize-RepoPath $Path
    $isUnderGovernedRoot = $false
    foreach ($root in $GovernedRoots)
    {
        if ($normalized.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase))
        {
            $isUnderGovernedRoot = $true
            break
        }
    }

    if (-not $isUnderGovernedRoot)
    {
        return $false
    }

    foreach ($prefix in $ExcludedPrefixes)
    {
        if ($normalized.StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase))
        {
            return $false
        }
    }

    $extension = [System.IO.Path]::GetExtension($normalized)
    return $ScriptExtensions -contains $extension.ToLowerInvariant()
}

function Invoke-GitLines
{
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    $lines = & git -C $RepositoryRoot @Arguments 2>$null
    if ($LASTEXITCODE -ne 0)
    {
        throw ('git command failed: git -C "{0}" {1}' -f
            $RepositoryRoot,
            ($Arguments -join ' '))
    }

    return @($lines)
}

function Get-RecordMetadata
{
    param(
        [Parameter(Mandatory = $true)]
        [System.IO.FileInfo]$RecordFile
    )

    $content = [System.IO.File]::ReadAllText($RecordFile.FullName)
    $match = [System.Text.RegularExpressions.Regex]::Match(
        $content,
        '(?s)<!--\s*CHANGE-RECORD\s*\r?\n(?<body>.*?)\r?\n-->'
    )
    if (-not $match.Success)
    {
        return $null
    }

    $metadata = @{
        File = $RecordFile
        CodePaths = [System.Collections.Generic.List[string]]::new()
    }

    foreach ($line in ($match.Groups['body'].Value -split '\r?\n'))
    {
        $lineMatch = [System.Text.RegularExpressions.Regex]::Match(
            $line,
            '^\s*(?<key>[A-Za-z][A-Za-z0-9-]*):\s*(?<value>.*?)\s*$'
        )
        if (-not $lineMatch.Success)
        {
            continue
        }

        $key = $lineMatch.Groups['key'].Value.ToLowerInvariant()
        $value = $lineMatch.Groups['value'].Value
        if ($key -eq 'code-path')
        {
            if (-not [string]::IsNullOrWhiteSpace($value))
            {
                $metadata.CodePaths.Add((Normalize-RepoPath $value))
            }

            continue
        }

        $metadata[$key] = $value
    }

    return $metadata
}

if (-not (Get-Command git -ErrorAction SilentlyContinue))
{
    throw 'git was not found on PATH.'
}

$RepositoryRoot = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$gitDirectory = Join-Path $RepositoryRoot '.git'
if (-not (Test-Path -LiteralPath $gitDirectory))
{
    throw ('Repository root does not contain .git: {0}' -f $RepositoryRoot)
}

$ledgerPath = Join-Path $RepositoryRoot 'docs/ai/CHANGE-LEDGER.md'
$recordDirectory = Join-Path $RepositoryRoot 'docs/ai/CHANGE-RECORDS'
$statePath = Join-Path $RepositoryRoot 'docs/ai/STATE.md'

$errors = [System.Collections.Generic.List[string]]::new()
$warnings = [System.Collections.Generic.List[string]]::new()

if (-not (Test-Path -LiteralPath $ledgerPath))
{
    $errors.Add('Missing docs/ai/CHANGE-LEDGER.md.')
}

if (-not (Test-Path -LiteralPath $recordDirectory))
{
    $errors.Add('Missing docs/ai/CHANGE-RECORDS directory.')
}

$records = [System.Collections.Generic.List[hashtable]]::new()
$pathsToRecordIds = @{}
$recordIds = @{}

if ($errors.Count -eq 0)
{
    $recordFiles = Get-ChildItem -LiteralPath $recordDirectory -File -Filter '*.md' |
        Where-Object { $_.Name -ne 'README.md' } |
        Sort-Object Name

    foreach ($recordFile in $recordFiles)
    {
        $metadata = Get-RecordMetadata $recordFile
        if ($null -eq $metadata)
        {
            $errors.Add(('Missing CHANGE-RECORD metadata: {0}' -f
                (Normalize-RepoPath $recordFile.FullName.Substring($RepositoryRoot.Length + 1))))
            continue
        }

        foreach ($requiredKey in @('id', 'status', 'authority', 'evidence'))
        {
            if (-not $metadata.ContainsKey($requiredKey) -or
                [string]::IsNullOrWhiteSpace($metadata[$requiredKey]))
            {
                $errors.Add(('Missing {0} metadata in {1}.' -f
                    $requiredKey,
                    $recordFile.Name))
            }
        }

        if ($metadata.CodePaths.Count -eq 0)
        {
            $errors.Add(('Missing code-path metadata in {0}.' -f $recordFile.Name))
        }

        if ($metadata.ContainsKey('id'))
        {
            $id = $metadata['id']
            if ($recordIds.ContainsKey($id))
            {
                $errors.Add(('Duplicate Change ID {0} in {1} and {2}.' -f
                    $id,
                    $recordIds[$id].File.Name,
                    $recordFile.Name))
            }
            else
            {
                $recordIds[$id] = $metadata
            }
        }

        if ($metadata.ContainsKey('status') -and
            $AllowedStatuses -notcontains $metadata['status'])
        {
            $errors.Add(('Unsupported status {0} in {1}.' -f
                $metadata['status'],
                $recordFile.Name))
        }

        if ($metadata.ContainsKey('status') -and
            @('CODE_WRITTEN', 'COMPILE_PASS', 'FOCUSED_TEST_PASS', 'RUNTIME_PENDING', 'VERIFIED') -contains
                $metadata['status'] -and
            $metadata.ContainsKey('evidence') -and
            $metadata['evidence'].StartsWith('PENDING', [System.StringComparison]::OrdinalIgnoreCase))
        {
            $errors.Add(('Status {0} requires non-pending evidence in {1}.' -f
                $metadata['status'],
                $recordFile.Name))
        }

        foreach ($codePath in $metadata.CodePaths)
        {
            if (-not (Test-GovernedCodePath $codePath))
            {
                $errors.Add(('Record {0} declares non-governed code-path {1}.' -f
                    $metadata['id'],
                    $codePath))
                continue
            }

            if (-not $pathsToRecordIds.ContainsKey($codePath))
            {
                $pathsToRecordIds[$codePath] =
                    [System.Collections.Generic.List[string]]::new()
            }

            $pathsToRecordIds[$codePath].Add($metadata['id'])
        }

        $records.Add($metadata)
    }
}

$changedPaths = [System.Collections.Generic.HashSet[string]]::new(
    [System.StringComparer]::OrdinalIgnoreCase
)

if (-not $StagedOnly)
{
    foreach ($path in (Invoke-GitLines @('diff', '--name-only', '--diff-filter=ACMRD', 'HEAD', '--')))
    {
        if (-not [string]::IsNullOrWhiteSpace($path))
        {
            [void]$changedPaths.Add((Normalize-RepoPath $path))
        }
    }
}
else
{
    foreach ($path in (Invoke-GitLines @('diff', '--cached', '--name-only', '--diff-filter=ACMRD', '--')))
    {
        if (-not [string]::IsNullOrWhiteSpace($path))
        {
            [void]$changedPaths.Add((Normalize-RepoPath $path))
        }
    }
}

if (-not $SkipUntracked -and -not $StagedOnly)
{
    foreach ($path in (Invoke-GitLines @('ls-files', '--others', '--exclude-standard')))
    {
        if (-not [string]::IsNullOrWhiteSpace($path))
        {
            [void]$changedPaths.Add((Normalize-RepoPath $path))
        }
    }
}

if (-not [string]::IsNullOrWhiteSpace($SimulateChangedPath))
{
    $simulatedPath = Normalize-RepoPath $SimulateChangedPath
    if (-not (Test-GovernedCodePath $simulatedPath))
    {
        throw ('SimulateChangedPath must be a governed authored script path: {0}' -f
            $simulatedPath)
    }

    [void]$changedPaths.Add($simulatedPath)
}

$changedCodePaths = @($changedPaths | Where-Object { Test-GovernedCodePath $_ } | Sort-Object)
foreach ($path in $changedCodePaths)
{
    if (-not $pathsToRecordIds.ContainsKey($path))
    {
        $errors.Add(('Unrecorded authored script diff: {0}' -f $path))
    }
}

if (Test-Path -LiteralPath $ledgerPath)
{
    $ledgerContent = [System.IO.File]::ReadAllText($ledgerPath)
    foreach ($record in $records)
    {
        $id = $record['id']
        if ($ledgerContent.IndexOf($id, [System.StringComparison]::Ordinal) -lt 0)
        {
            $errors.Add(('Change ID {0} is missing from CHANGE-LEDGER.md.' -f $id))
        }
    }
}

if (Test-Path -LiteralPath $statePath)
{
    $stateContent = [System.IO.File]::ReadAllText($statePath)
    foreach ($record in $records)
    {
        if ($ActiveStatuses -contains $record['status'] -and
            $stateContent.IndexOf($record['id'], [System.StringComparison]::Ordinal) -lt 0)
        {
            $errors.Add(('Active Change ID {0} is missing from STATE.md.' -f
                $record['id']))
        }
    }
}
else
{
    $warnings.Add('STATE.md is missing; active Change IDs could not be cross-checked.')
}

foreach ($record in $records)
{
    foreach ($path in $record.CodePaths)
    {
        if ($changedCodePaths -notcontains $path)
        {
            $warnings.Add(('Record {0} declares {1}, which is not in the current code diff.' -f
                $record['id'],
                $path))
        }
    }
}

if ($errors.Count -gt 0)
{
    Write-Host 'Change ledger validation FAILED.' -ForegroundColor Red
    foreach ($errorMessage in $errors)
    {
        Write-Host ('  ERROR: {0}' -f $errorMessage) -ForegroundColor Red
    }

    foreach ($warningMessage in $warnings)
    {
        Write-Host ('  WARNING: {0}' -f $warningMessage) -ForegroundColor Yellow
    }

    exit 1
}

Write-Host 'Change ledger validation PASSED.' -ForegroundColor Green
Write-Host ('  Records: {0}' -f $records.Count)
Write-Host ('  Governed code files in diff: {0}' -f $changedCodePaths.Count)
foreach ($path in $changedCodePaths)
{
    $ids = $pathsToRecordIds[$path] -join ', '
    Write-Host ('  COVERED: {0} -> {1}' -f $path, $ids)
}

foreach ($warningMessage in $warnings)
{
    Write-Host ('  WARNING: {0}' -f $warningMessage) -ForegroundColor Yellow
}
