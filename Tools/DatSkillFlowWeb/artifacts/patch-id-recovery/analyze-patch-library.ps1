[CmdletBinding()]
param(
    [string]$LibraryRoot = "J:\QQFile\NTSD2.4大量人物补丁（2）",
    [string]$OutputRoot = ""
)

$ErrorActionPreference = "Stop"
if ($OutputRoot -eq "") {
    $OutputRoot = Join-Path $PSScriptRoot "runs"
}
$encryptionKey = [Text.Encoding]::ASCII.GetBytes("SiuHungIsAGoodBearBecauseHeIsVeryGood")
$prefixLength = 123

function Get-Sha256([byte[]]$Bytes) {
    $sha = [Security.Cryptography.SHA256]::Create()
    try {
        return ([BitConverter]::ToString($sha.ComputeHash($Bytes))).Replace("-", "")
    }
    finally {
        $sha.Dispose()
    }
}

function Read-DatInfo([IO.FileInfo]$File) {
    $raw = [IO.File]::ReadAllBytes($File.FullName)
    if ($raw.Length -gt $prefixLength) {
        $plain = New-Object byte[] ($raw.Length - $prefixLength)
        for ($index = 0; $index -lt $plain.Length; $index += 1) {
            $absolute = $prefixLength + $index
            $plain[$index] = [byte](($raw[$absolute] - $encryptionKey[$absolute % $encryptionKey.Length] + 256) % 256)
        }
    }
    else {
        $plain = $raw
    }

    $text = [Text.Encoding]::GetEncoding(1252).GetString($plain)
    $nameMatch = [regex]::Match($text, '(?im)^\s*name\s*:\s*([^\r\n#]+)')
    $headMatch = [regex]::Match($text, '(?im)^\s*head\s*:\s*([^\r\n#]+)')
    $smallMatch = [regex]::Match($text, '(?im)^\s*small\s*:\s*([^\r\n#]+)')
    $referencedOids = @(
        [regex]::Matches($text, '(?im)\boid\s*:\s*([+-]?\d+)') |
            ForEach-Object { [int]$_.Groups[1].Value } |
            Sort-Object -Unique
    )

    return [PSCustomObject]@{
        Path = $File.FullName
        FileName = $File.Name
        Directory = $File.DirectoryName
        RawSha256 = Get-Sha256 $raw
        PlainSha256 = Get-Sha256 $plain
        Name = if ($nameMatch.Success) { $nameMatch.Groups[1].Value.Trim() } else { "" }
        Head = if ($headMatch.Success) { $headMatch.Groups[1].Value.Trim() } else { "" }
        Small = if ($smallMatch.Success) { $smallMatch.Groups[1].Value.Trim() } else { "" }
        HasWeaponHp = [regex]::IsMatch($text, '(?im)^\s*weapon_hp\s*:')
        FrameCount = [regex]::Matches($text, '(?im)^\s*<frame>').Count
        ReferencedOids = $referencedOids
    }
}

function Relative-ToLibrary([string]$Path) {
    return $Path.Substring($LibraryRoot.Length).TrimStart("\")
}

if (-not (Test-Path -LiteralPath $LibraryRoot -PathType Container)) {
    throw "Patch library is unavailable: $LibraryRoot"
}

$allDat = @(Get-ChildItem -LiteralPath $LibraryRoot -Recurse -File -Filter "*.dat")
$textFiles = @(
    Get-ChildItem -LiteralPath $LibraryRoot -Recurse -File -Filter "*.txt" |
        Where-Object { $_.Length -le 1MB }
)
$manifestRecords = @()

foreach ($textFile in $textFiles) {
    $content = Get-Content -LiteralPath $textFile.FullName -Encoding Default -Raw
    if ($null -eq $content) {
        continue
    }
    $matches = [regex]::Matches(
        $content,
        '(?im)\bid[ \t]*:[ \t]*(\d+)[ \t]+(type|tupe|tpye)[ \t]*:[ \t]*(\d+)[ \t]+file[ \t]*:[ \t]*([^#\r\n]+?\.dat)'
    )
    foreach ($match in $matches) {
        $lineNumber = 1 + [regex]::Matches($content.Substring(0, $match.Index), '\r?\n').Count
        $manifestRecords += [PSCustomObject]@{
            Manifest = $textFile.FullName
            ManifestDirectory = $textFile.DirectoryName
            Line = $lineNumber
            Oid = [int]$match.Groups[1].Value
            FieldToken = $match.Groups[2].Value.ToLowerInvariant()
            Type = [int]$match.Groups[3].Value
            Declared = $match.Groups[4].Value.Trim()
            Resolved = $null
            Resolution = $null
        }
    }
}

$manifestDirectoryIndexes = @{}
foreach ($record in $manifestRecords) {
    $directoryKey = $record.ManifestDirectory.ToLowerInvariant()
    if (-not $manifestDirectoryIndexes.ContainsKey($directoryKey)) {
        $byName = @{}
        foreach ($file in @(Get-ChildItem -LiteralPath $record.ManifestDirectory -Recurse -File -Filter "*.dat")) {
            $nameKey = $file.Name.ToLowerInvariant()
            if (-not $byName.ContainsKey($nameKey)) {
                $byName[$nameKey] = @()
            }
            $byName[$nameKey] += $file.FullName
        }
        $manifestDirectoryIndexes[$directoryKey] = $byName
    }

    $segments = @($record.Declared -split "[\\/]+" | Where-Object { $_ -ne "" })
    if ($segments.Count -eq 0) {
        $record.Resolution = "invalid"
        continue
    }
    $baseName = $segments[-1]
    if ($baseName.IndexOfAny([IO.Path]::GetInvalidFileNameChars()) -ge 0) {
        $record.Resolution = "invalid"
        continue
    }

    $byName = $manifestDirectoryIndexes[$directoryKey]
    $matches = if ($byName.ContainsKey($baseName.ToLowerInvariant())) {
        @($byName[$baseName.ToLowerInvariant()])
    }
    else {
        @()
    }
    $relative = $record.Declared -replace "[\\/]", [IO.Path]::DirectorySeparatorChar
    $exact = Join-Path $record.ManifestDirectory $relative
    $flat = Join-Path $record.ManifestDirectory $baseName

    if (Test-Path -LiteralPath $exact -PathType Leaf) {
        $record.Resolved = (Resolve-Path -LiteralPath $exact).Path
        $record.Resolution = "exact"
    }
    elseif (Test-Path -LiteralPath $flat -PathType Leaf) {
        $record.Resolved = (Resolve-Path -LiteralPath $flat).Path
        $record.Resolution = "flat-basename"
    }
    elseif ($matches.Count -eq 1) {
        $record.Resolved = $matches[0]
        $record.Resolution = "recursive-unique"
    }
    elseif ($matches.Count -gt 1) {
        $record.Resolution = "ambiguous"
    }
    else {
        $record.Resolution = "missing"
    }
}

$mappedPaths = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach ($record in $manifestRecords) {
    if ($null -ne $record.Resolved) {
        [void]$mappedPaths.Add($record.Resolved)
    }
}

$datInfoByPath = @{}
foreach ($dat in $allDat) {
    $datInfoByPath[$dat.FullName.ToLowerInvariant()] = Read-DatInfo $dat
}

$knownByRawHash = @{}
$knownByPlainHash = @{}
foreach ($record in $manifestRecords) {
    if ($null -eq $record.Resolved) {
        continue
    }
    $info = $datInfoByPath[$record.Resolved.ToLowerInvariant()]
    $mapping = [PSCustomObject]@{
        Oid = $record.Oid
        Type = $record.Type
        Declared = $record.Declared
        Manifest = $record.Manifest
    }
    foreach ($index in @($knownByRawHash, $knownByPlainHash)) {
        $hash = if ($index -eq $knownByRawHash) { $info.RawSha256 } else { $info.PlainSha256 }
        if (-not $index.ContainsKey($hash)) {
            $index[$hash] = @()
        }
        $index[$hash] += $mapping
    }
}

$unmapped = @()
foreach ($dat in $allDat) {
    if ($mappedPaths.Contains($dat.FullName)) {
        continue
    }
    $info = $datInfoByPath[$dat.FullName.ToLowerInvariant()]
    $mappingCandidates = @()
    if ($knownByRawHash.ContainsKey($info.RawSha256)) {
        $mappingCandidates += $knownByRawHash[$info.RawSha256]
    }
    if ($knownByPlainHash.ContainsKey($info.PlainSha256)) {
        $mappingCandidates += $knownByPlainHash[$info.PlainSha256]
    }
    $pairs = @(
        $mappingCandidates |
            ForEach-Object { "$($_.Oid)/$($_.Type)" } |
            Sort-Object -Unique
    )
    $inferredType = if ($info.Name -ne "" -and $info.Head -ne "" -and $info.Small -ne "") {
        0
    }
    elseif ($info.HasWeaponHp) {
        "weapon-unknown"
    }
    else {
        3
    }
    $unmapped += [PSCustomObject]@{
        Path = $info.Path
        RelativePath = Relative-ToLibrary $info.Path
        Directory = $info.Directory
        FileName = $info.FileName
        Name = $info.Name
        FrameCount = $info.FrameCount
        ReferencedOids = $info.ReferencedOids
        InferredType = $inferredType
        ExactMappings = $pairs
    }
}

$candidatePackages = @()
foreach ($directoryGroup in @($unmapped | Group-Object Directory)) {
    $objects = @($directoryGroup.Group)
    $roots = @($objects | Where-Object { $_.InferredType -eq 0 })
    if ($roots.Count -eq 0) {
        continue
    }
    $candidatePackages += [PSCustomObject]@{
        Directory = $directoryGroup.Name
        RelativeDirectory = Relative-ToLibrary $directoryGroup.Name
        Roots = $roots
        Objects = $objects
    }
}

$runId = "run-" + (Get-Date -Format "yyyyMMdd-HHmmss")
$runRoot = Join-Path $OutputRoot $runId
$supplementRoot = Join-Path $runRoot "supplemental"
New-Item -ItemType Directory -Path $supplementRoot -Force | Out-Null

$generated = @()
$needsReview = @()
foreach ($package in $candidatePackages) {
    $resolvedObjects = @()
    $complete = $true
    foreach ($object in $package.Objects) {
        if ($object.ExactMappings.Count -ne 1) {
            $complete = $false
            break
        }
        $parts = $object.ExactMappings[0].Split("/")
        $resolvedObjects += [PSCustomObject]@{
            Oid = [int]$parts[0]
            Type = [int]$parts[1]
            FileName = $object.FileName
            Evidence = "exact DAT content fingerprint"
        }
    }
    if (-not $complete -or @($resolvedObjects | Where-Object { $_.Type -eq 0 }).Count -eq 0) {
        $needsReview += $package
        continue
    }

    $outputDirectory = Join-Path $supplementRoot $package.RelativeDirectory
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
    $outputPath = Join-Path $outputDirectory "ID.editor-recovered.txt"
    $lines = @(
        "# Editor-local recovered manifest. Source J: package remains unchanged.",
        "# DAT paths are relative to this package directory."
    )
    foreach ($object in @($resolvedObjects | Sort-Object Type, Oid, FileName)) {
        $lines += "id: $($object.Oid) type: $($object.Type) file: $($object.FileName)"
    }
    Set-Content -LiteralPath $outputPath -Value $lines -Encoding UTF8
    $generated += [PSCustomObject]@{
        Package = $package.RelativeDirectory
        Output = $outputPath
        Objects = $resolvedObjects
    }
}

$inventory = [PSCustomObject]@{
    LibraryRoot = $LibraryRoot
    GeneratedAt = (Get-Date).ToString("o")
    TextFilesScanned = $textFiles.Count
    ManifestFiles = @($manifestRecords | Select-Object -ExpandProperty Manifest -Unique).Count
    ExplicitRecords = $manifestRecords.Count
    ResolvedExplicitRecords = @($manifestRecords | Where-Object { $null -ne $_.Resolved }).Count
    DatFiles = $allDat.Count
    ExplicitlyMappedDatFiles = $mappedPaths.Count
    UnmappedDatFiles = $unmapped.Count
    CandidatePackages = $candidatePackages.Count
    GeneratedSupplementalManifests = $generated.Count
    ExplicitRecordProblems = @(
        $manifestRecords |
            Where-Object { $null -eq $_.Resolved } |
            ForEach-Object {
                [PSCustomObject]@{
                    Manifest = Relative-ToLibrary $_.Manifest
                    Line = $_.Line
                    Oid = $_.Oid
                    Type = $_.Type
                    Declared = $_.Declared
                    Resolution = $_.Resolution
                }
            }
    )
    Unmapped = $unmapped
    CandidatePackageDetails = $candidatePackages
    Generated = $generated
}
$inventory | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath (Join-Path $runRoot "inventory.json") -Encoding UTF8

$report = @(
    "# NTSD patch ID recovery audit",
    "",
    "- Library: $LibraryRoot",
    "- Text files scanned: $($textFiles.Count)",
    "- Manifest files discovered by content: $($inventory.ManifestFiles)",
    "- Explicit object records: $($manifestRecords.Count)",
    "- Explicit records resolved to DAT: $($inventory.ResolvedExplicitRecords)",
    "- DAT files: $($allDat.Count)",
    "- Explicitly mapped DAT files: $($mappedPaths.Count)",
    "- Unmapped DAT files: $($unmapped.Count)",
    "- Candidate packages with an unmapped structural type-0 DAT: $($candidatePackages.Count)",
    "- Supplemental manifests generated from exact fingerprints: $($generated.Count)",
    "",
    "## Generated supplemental manifests",
    ""
)
if ($generated.Count -eq 0) {
    $report += "None."
}
else {
    foreach ($item in $generated) {
        $report += "- $($item.Package) -> $($item.Output)"
    }
}
$report += @("", "## Packages requiring review", "")
foreach ($package in $needsReview) {
    $report += "### $($package.RelativeDirectory)"
    $report += ""
    foreach ($object in $package.Objects) {
        $mapping = if ($object.ExactMappings.Count -eq 0) { "unknown" } else { $object.ExactMappings -join ", " }
        $report += "- $($object.FileName): inferred type $($object.InferredType); exact mapping $mapping; frames $($object.FrameCount)"
    }
    $report += ""
}
$report | Set-Content -LiteralPath (Join-Path $runRoot "REPORT.md") -Encoding UTF8

Write-Output "Audit output: $runRoot"
Write-Output "Manifest files: $($inventory.ManifestFiles)"
Write-Output "Explicit records resolved: $($inventory.ResolvedExplicitRecords)/$($manifestRecords.Count)"
Write-Output "DAT mapped explicitly: $($mappedPaths.Count)/$($allDat.Count)"
Write-Output "Candidate packages: $($candidatePackages.Count)"
Write-Output "Supplemental manifests generated: $($generated.Count)"
