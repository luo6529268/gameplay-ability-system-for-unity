[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$LibraryRoot,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath,

    [Parameter(Mandatory = $false)]
    [Alias('MaxTxtBytes')]
    [long]$MaxTextBytes = 1048576,

    [Parameter(Mandatory = $false)]
    [int]$MaxFileCount = 100000,

    [Parameter(Mandatory = $false)]
    [string]$SupplementalRoot = ''
)

# This scanner is deliberately limited to metadata and file names. It never reads
# DAT/BMP contents, never decrypts assets, and only writes the requested output.
$ErrorActionPreference = 'Stop'

function Assert-NonEmptyPath {
    param(
        [string]$Value,
        [string]$Name
    )

    if ([string]::IsNullOrEmpty($Value)) {
        throw "$Name must not be empty."
    }
    if ($Value.IndexOf([char]0) -ge 0) {
        throw "$Name contains a NUL character."
    }
}

function Get-ExistingDirectory {
    param(
        [string]$Path,
        [string]$Name
    )

    Assert-NonEmptyPath -Value $Path -Name $Name
    $item = Get-Item -LiteralPath $Path -Force -ErrorAction Stop
    if (-not $item.PSIsContainer) {
        throw "$Name is not a directory: $Path"
    }

    $resolved = (Resolve-Path -LiteralPath $item.FullName -ErrorAction Stop).Path
    while ($resolved.Length -gt 3 -and ($resolved.EndsWith('\') -or $resolved.EndsWith('/'))) {
        $resolved = $resolved.Substring(0, $resolved.Length - 1)
    }
    return $resolved
}

function Get-FullOutputPath {
    param(
        [string]$Path
    )

    Assert-NonEmptyPath -Value $Path -Name 'OutputPath'
    try {
        return [IO.Path]::GetFullPath($Path)
    }
    catch {
        throw "OutputPath is not a valid path: $Path"
    }
}

function Test-SameOrUnderPath {
    param(
        [string]$Candidate,
        [string]$Root
    )

    $candidateValue = $Candidate.Replace('/', '\').TrimEnd('\')
    $rootValue = $Root.Replace('/', '\').TrimEnd('\')
    if ([string]::Equals($candidateValue, $rootValue, [StringComparison]::OrdinalIgnoreCase)) {
        return $true
    }
    if ($rootValue.Length -eq 0) {
        return $false
    }

    return $candidateValue.StartsWith($rootValue + '\', [StringComparison]::OrdinalIgnoreCase)
}

function Get-RelativePathFromRoot {
    param(
        [string]$Path,
        [string]$Root
    )

    $pathValue = $Path.Replace('/', '\')
    $rootValue = $Root.Replace('/', '\').TrimEnd('\')
    if ([string]::Equals($pathValue, $rootValue, [StringComparison]::OrdinalIgnoreCase)) {
        return ''
    }

    $prefix = $rootValue + '\'
    if (-not $pathValue.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Path is outside the expected root: $Path"
    }
    return $pathValue.Substring($prefix.Length).Replace('\', '/')
}

function Get-RelativePathInfo {
    param(
        [string]$Path
    )

    if ($null -eq $Path -or $Path.IndexOf([char]0) -ge 0) {
        return [PSCustomObject]@{
            Valid = $false
            Code = 'nul-in-path'
            Message = 'The declared path contains a NUL character.'
            Value = $null
        }
    }

    $trimmed = $Path.Trim()
    $portable = $trimmed.Replace('\', '/')
    if ([string]::IsNullOrEmpty($portable)) {
        return [PSCustomObject]@{
            Valid = $false
            Code = 'invalid-path'
            Message = 'The declared DAT path is empty.'
            Value = $null
        }
    }
    if ($portable.StartsWith('/') -or $portable -match '^[A-Za-z]:') {
        return [PSCustomObject]@{
            Valid = $false
            Code = 'path-traversal'
            Message = 'Absolute DAT paths are rejected.'
            Value = $null
        }
    }

    $segments = @($portable.Split('/'))
    $invalidFileNameCharacters = [IO.Path]::GetInvalidFileNameChars()
    foreach ($segment in $segments) {
        if ($segment -eq '..') {
            return [PSCustomObject]@{
                Valid = $false
                Code = 'path-traversal'
                Message = 'DAT paths containing parent traversal are rejected.'
                Value = $null
            }
        }
        if ($segment -eq '.') {
            return [PSCustomObject]@{
                Valid = $false
                Code = 'path-traversal'
                Message = 'DAT paths containing current-directory traversal are rejected.'
                Value = $null
            }
        }
        if ($segment.IndexOf([char]0) -ge 0) {
            return [PSCustomObject]@{
                Valid = $false
                Code = 'nul-in-path'
                Message = 'The declared path contains a NUL character.'
                Value = $null
            }
        }
        if ($segment.IndexOfAny($invalidFileNameCharacters) -ge 0) {
            return [PSCustomObject]@{
                Valid = $false
                Code = 'invalid-path'
                Message = 'The declared DAT path contains a Windows-invalid file name character.'
                Value = $null
            }
        }
    }

    $normalizedSegments = @($segments | Where-Object { $_ -ne '' })
    if ($normalizedSegments.Count -eq 0) {
        return [PSCustomObject]@{
            Valid = $false
            Code = 'invalid-path'
            Message = 'The declared DAT path is empty.'
            Value = $null
        }
    }

    return [PSCustomObject]@{
        Valid = $true
        Code = $null
        Message = $null
        Value = ($normalizedSegments -join '/')
    }
}

function New-Diagnostic {
    param(
        [string]$Code,
        [string]$Severity,
        [string]$Message,
        [string]$ManifestPath = $null,
        [int]$Line = $null,
        [long]$Oid = $null,
        [long]$Type = $null,
        [string]$RecordId = $null,
        [string]$RelatedManifestPath = $null,
        [string]$RelatedRecordId = $null
    )

    return [ordered]@{
        code = $Code
        severity = $Severity
        message = $Message
        manifestPath = $ManifestPath
        line = $Line
        oid = $Oid
        type = $Type
        recordId = $RecordId
        relatedManifestPath = $RelatedManifestPath
        relatedRecordId = $RelatedRecordId
    }
}

function Get-PackageId {
    param(
        [string]$RelativeDirectory
    )

    $identity = if ([string]::IsNullOrEmpty($RelativeDirectory)) { '.' } else { $RelativeDirectory }
    $bytes = [Text.Encoding]::UTF8.GetBytes($identity)
    $sha = [Security.Cryptography.SHA256]::Create()
    try {
        $digest = $sha.ComputeHash($bytes)
    }
    finally {
        $sha.Dispose()
    }
    return 'pkg-' + ([BitConverter]::ToString($digest)).Replace('-', '').ToLowerInvariant().Substring(0, 16)
}

function Get-PackageLabel {
    param(
        [string]$RelativeDirectory
    )

    if ([string]::IsNullOrEmpty($RelativeDirectory)) {
        return 'root'
    }
    $segments = @($RelativeDirectory.Split('/'))
    return $segments[$segments.Count - 1]
}

function Add-PackageDiagnostic {
    param(
        [object]$Package,
        [object]$Diagnostic
    )

    [void]$Package.Diagnostics.Add($Diagnostic)
}

function Get-PathKey {
    param(
        [string]$Path
    )

    return $Path.Replace('/', '\').ToLowerInvariant()
}

function Get-DirectoryChildPath {
    param(
        [string]$Root,
        [string]$RelativeDirectory
    )

    if ([string]::IsNullOrEmpty($RelativeDirectory)) {
        return $Root
    }
    return [IO.Path]::GetFullPath((Join-Path -Path $Root -ChildPath $RelativeDirectory.Replace('/', '\')))
}

if ($MaxTextBytes -le 0) {
    throw 'MaxTextBytes must be greater than zero.'
}
if ($MaxFileCount -le 0) {
    throw 'MaxFileCount must be greater than zero.'
}

$libraryRootFull = Get-ExistingDirectory -Path $LibraryRoot -Name 'LibraryRoot'
$outputPathFull = Get-FullOutputPath -Path $OutputPath
$supplementalRootFull = $null
if (-not [string]::IsNullOrEmpty($SupplementalRoot)) {
    $supplementalRootFull = Get-ExistingDirectory -Path $SupplementalRoot -Name 'SupplementalRoot'
}

if (Test-SameOrUnderPath -Candidate $outputPathFull -Root $libraryRootFull) {
    throw 'OutputPath must not be equal to or inside LibraryRoot.'
}
if ($null -ne $supplementalRootFull -and (Test-SameOrUnderPath -Candidate $outputPathFull -Root $supplementalRootFull)) {
    throw 'OutputPath must not be equal to or inside SupplementalRoot.'
}
if ($outputPathFull -match '^(?i)J:') {
    throw 'OutputPath on the J drive is not permitted.'
}
if ([IO.Directory]::Exists($outputPathFull)) {
    throw 'OutputPath must name a file, not a directory.'
}

$libraryFiles = @(Get-ChildItem -LiteralPath $libraryRootFull -Recurse -Force -File -ErrorAction Stop | Sort-Object -Property FullName)
$supplementalFiles = @()
if ($null -ne $supplementalRootFull) {
    $supplementalFiles = @(Get-ChildItem -LiteralPath $supplementalRootFull -Recurse -Force -File -ErrorAction Stop | Sort-Object -Property FullName)
}
$totalFileCount = $libraryFiles.Count + $supplementalFiles.Count
if ($totalFileCount -gt $MaxFileCount) {
    throw "Total file count $totalFileCount exceeds MaxFileCount $MaxFileCount."
}

# If a caller mirrors the supplemental tree inside the library, do not treat
# those editor-only files as source manifests or source assets a second time.
$sourceFiles = @($libraryFiles)
if ($null -ne $supplementalRootFull -and (Test-SameOrUnderPath -Candidate $supplementalRootFull -Root $libraryRootFull)) {
    $sourceFiles = @($libraryFiles | Where-Object {
            -not (Test-SameOrUnderPath -Candidate $_.FullName -Root $supplementalRootFull)
        })
}

$datFilesByPath = @{}
foreach ($file in @($sourceFiles | Where-Object { $_.Extension -ieq '.dat' })) {
    $key = Get-PathKey -Path $file.FullName
    if (-not $datFilesByPath.ContainsKey($key)) {
        $datFilesByPath[$key] = $file
    }
}

$packagesByKey = @{}
$globalDiagnostics = New-Object System.Collections.ArrayList

function Get-OrCreatePackage {
    param(
        [string]$RelativeDirectory,
        [string]$DirectoryFull
    )

    $packageKey = $RelativeDirectory.ToLowerInvariant()
    if (-not $packagesByKey.ContainsKey($packageKey)) {
        $packagesByKey[$packageKey] = [PSCustomObject]@{
            Key = $packageKey
            RelativeDirectory = $RelativeDirectory
            DirectoryFull = $DirectoryFull
            Records = New-Object System.Collections.ArrayList
            Diagnostics = New-Object System.Collections.ArrayList
        }
    }
    return $packagesByKey[$packageKey]
}

function Read-ManifestFile {
    param(
        [object]$ManifestFile,
        [string]$ManifestSource,
        [string]$ManifestRoot,
        [string]$PackageRelativeDirectory,
        [string]$PackageDirectoryFull
    )

    $manifestPath = Get-RelativePathFromRoot -Path $ManifestFile.FullName -Root $ManifestRoot
    $package = $null
    if ($ManifestFile.Length -gt $MaxTextBytes) {
        [void]$globalDiagnostics.Add((New-Diagnostic `
                -Code 'manifest-too-large' `
                -Severity 'error' `
                -Message "Manifest exceeds MaxTextBytes ($MaxTextBytes bytes): $manifestPath" `
                -ManifestPath $manifestPath))
        return
    }

    try {
        $content = Get-Content -LiteralPath $ManifestFile.FullName -Raw -Encoding UTF8 -ErrorAction Stop
    }
    catch {
        [void]$globalDiagnostics.Add((New-Diagnostic `
                -Code 'manifest-read-failed' `
                -Severity 'error' `
                -Message "Unable to read manifest: $manifestPath" `
                -ManifestPath $manifestPath))
        return
    }
    if ($null -eq $content) {
        $content = ''
    }

    $lines = [regex]::Split([string]$content, '\r\n|\n|\r')
    $registrationPattern = '(?i)\bid\s*:\s*([+-]?\d+)[ \t]+(type|tupe|tpye)\s*:\s*([+-]?\d+)[ \t]+file\s*:\s*([^\r\n]*?\.dat)(?:[ \t]+#.*)?[ \t]*$'
    $recordOrdinal = 0
    for ($lineIndex = 0; $lineIndex -lt $lines.Count; $lineIndex += 1) {
        $lineText = [string]$lines[$lineIndex]
        $match = [regex]::Match($lineText, $registrationPattern)
        if (-not $match.Success) {
            continue
        }

        if ($null -eq $package) {
            $package = Get-OrCreatePackage -RelativeDirectory $PackageRelativeDirectory -DirectoryFull $PackageDirectoryFull
        }

        try {
            $oid = [Int64]::Parse($match.Groups[1].Value)
            $type = [Int64]::Parse($match.Groups[3].Value)
        }
        catch {
            Add-PackageDiagnostic -Package $package -Diagnostic (New-Diagnostic `
                    -Code 'integer-out-of-range' `
                    -Severity 'error' `
                    -Message "Manifest registration has an out-of-range id or type: $manifestPath" `
                    -ManifestPath $manifestPath `
                    -Line ($lineIndex + 1))
            continue
        }

        $typeToken = $match.Groups[2].Value.ToLowerInvariant()
        $declaredPath = $match.Groups[4].Value.Trim()
        $safePathInfo = Get-RelativePathInfo -Path $declaredPath
        $recordOrdinal += 1
        $recordId = "$ManifestSource`:$manifestPath`:$($lineIndex + 1):$recordOrdinal"
        $logicalPath = $null
        if ($safePathInfo.Valid) {
            $logicalPath = if ([string]::IsNullOrEmpty($PackageRelativeDirectory)) {
                $safePathInfo.Value
            }
            else {
                "$PackageRelativeDirectory/$($safePathInfo.Value)"
            }
        }
        $recordOutput = [ordered]@{
            recordId = $recordId
            oid = $oid
            type = $type
            typeToken = $typeToken
            file = $declaredPath
            normalizedFile = if ($safePathInfo.Valid) { $safePathInfo.Value } else { $null }
            basename = if ($safePathInfo.Valid) { [IO.Path]::GetFileName($safePathInfo.Value.Replace('/', '\')) } else { $null }
            logicalPath = $logicalPath
            manifestSource = $ManifestSource
            manifestPath = $manifestPath
            line = $lineIndex + 1
            resolution = 'missing'
            resolvedPath = $null
            effective = $true
            overriddenBy = $null
        }
        $record = [PSCustomObject]@{
            Output = $recordOutput
            PackageKey = $package.Key
            ManifestSource = $ManifestSource
            Oid = $oid
            Type = $type
            NormalizedFile = if ($safePathInfo.Valid) { $safePathInfo.Value } else { $null }
            BasenameKey = if ($safePathInfo.Valid) { [IO.Path]::GetFileName($safePathInfo.Value.Replace('/', '\')).ToLowerInvariant() } else { $null }
            LogicalPath = $logicalPath
            Resolution = 'missing'
        }

        if ($typeToken -eq 'tupe' -or $typeToken -eq 'tpye') {
            Add-PackageDiagnostic -Package $package -Diagnostic (New-Diagnostic `
                    -Code 'typo-field-token' `
                    -Severity 'warning' `
                    -Message "Manifest uses '$typeToken' instead of 'type': $manifestPath" `
                    -ManifestPath $manifestPath `
                    -Line ($lineIndex + 1) `
                    -Oid $oid `
                    -Type $type `
                    -RecordId $recordId)
        }

        if (-not $safePathInfo.Valid) {
            $record.Output.resolution = 'rejected'
            $record.Resolution = 'rejected'
            Add-PackageDiagnostic -Package $package -Diagnostic (New-Diagnostic `
                    -Code $safePathInfo.Code `
                    -Severity 'error' `
                    -Message "$($safePathInfo.Message) Manifest: $manifestPath" `
                    -ManifestPath $manifestPath `
                    -Line ($lineIndex + 1) `
                    -Oid $oid `
                    -Type $type `
                    -RecordId $recordId)
        }
        else {
            $relativeNativePath = $safePathInfo.Value.Replace('/', '\')
            $candidatePath = [IO.Path]::GetFullPath((Join-Path -Path $PackageDirectoryFull -ChildPath $relativeNativePath))
            if (-not (Test-SameOrUnderPath -Candidate $candidatePath -Root $PackageDirectoryFull) -or
                -not (Test-SameOrUnderPath -Candidate $candidatePath -Root $libraryRootFull)) {
                $record.Output.resolution = 'rejected'
                $record.Resolution = 'rejected'
                Add-PackageDiagnostic -Package $package -Diagnostic (New-Diagnostic `
                        -Code 'path-traversal' `
                        -Severity 'error' `
                        -Message "Resolved DAT path leaves the package directory: $declaredPath" `
                        -ManifestPath $manifestPath `
                        -Line ($lineIndex + 1) `
                        -Oid $oid `
                        -Type $type `
                        -RecordId $recordId)
            }
            else {
                $candidateKey = Get-PathKey -Path $candidatePath
                if ($datFilesByPath.ContainsKey($candidateKey)) {
                    $resolvedFile = $datFilesByPath[$candidateKey]
                    $record.Output.resolution = 'resolved'
                    $record.Output.logicalPath = Get-RelativePathFromRoot -Path $resolvedFile.FullName -Root $libraryRootFull
                    $record.LogicalPath = $record.Output.logicalPath
                    $record.Resolution = 'resolved'
                }
                else {
                    Add-PackageDiagnostic -Package $package -Diagnostic (New-Diagnostic `
                            -Code 'missing-dat' `
                            -Severity 'warning' `
                            -Message "Registered DAT was not found under the package directory: $declaredPath" `
                            -ManifestPath $manifestPath `
                            -Line ($lineIndex + 1) `
                            -Oid $oid `
                            -Type $type `
                            -RecordId $recordId)
                }
            }
        }

        [void]$package.Records.Add($record)
    }
}

$sourceManifestFiles = @($sourceFiles | Where-Object { $_.Extension -ieq '.txt' } | Sort-Object -Property FullName)
foreach ($manifestFile in $sourceManifestFiles) {
    $packageRelativeDirectory = Get-RelativePathFromRoot -Path $manifestFile.DirectoryName -Root $libraryRootFull
    $packageDirectoryFull = $manifestFile.DirectoryName
    Read-ManifestFile `
        -ManifestFile $manifestFile `
        -ManifestSource 'source' `
        -ManifestRoot $libraryRootFull `
        -PackageRelativeDirectory $packageRelativeDirectory `
        -PackageDirectoryFull $packageDirectoryFull
}

$supplementalManifestFiles = @()
if ($null -ne $supplementalRootFull) {
    $supplementalManifestFiles = @(
        $supplementalFiles |
            Where-Object { $_.Name -match '^ID\.editor-.*\.txt$' } |
            Sort-Object -Property FullName
    )
    foreach ($manifestFile in $supplementalManifestFiles) {
        $packageRelativeDirectory = Get-RelativePathFromRoot -Path $manifestFile.DirectoryName -Root $supplementalRootFull
        $packageDirectoryFull = Get-DirectoryChildPath -Root $libraryRootFull -RelativeDirectory $packageRelativeDirectory
        Read-ManifestFile `
            -ManifestFile $manifestFile `
            -ManifestSource 'supplemental' `
            -ManifestRoot $supplementalRootFull `
            -PackageRelativeDirectory $packageRelativeDirectory `
            -PackageDirectoryFull $packageDirectoryFull
    }
}

# Supplemental records are authoritative only for an explicit source record
# whose declaration is being corrected. Both records remain in the index.
foreach ($package in @($packagesByKey.Values | Sort-Object -Property RelativeDirectory)) {
    $sourceRecords = @($package.Records | Where-Object { $_.ManifestSource -eq 'source' })
    $supplementalRecords = @($package.Records | Where-Object { $_.ManifestSource -eq 'supplemental' })
    foreach ($sourceRecord in $sourceRecords) {
        $candidates = @($supplementalRecords | Where-Object {
                $_.Oid -eq $sourceRecord.Oid -and $_.Type -eq $sourceRecord.Type
            })
        if ($candidates.Count -eq 0 -and $null -ne $sourceRecord.BasenameKey) {
            $candidates = @($supplementalRecords | Where-Object {
                    $_.BasenameKey -eq $sourceRecord.BasenameKey
                })
        }
        if ($candidates.Count -eq 0) {
            continue
        }

        $candidate = @($candidates | Sort-Object -Property @{ Expression = { $_.Output.manifestPath } }, @{ Expression = { $_.Output.line } }, @{ Expression = { $_.Output.recordId } })[0]
        $sameDeclaration = $false
        if ($null -ne $sourceRecord.NormalizedFile -and $null -ne $candidate.NormalizedFile) {
            $sameDeclaration = [string]::Equals($sourceRecord.NormalizedFile, $candidate.NormalizedFile, [StringComparison]::OrdinalIgnoreCase)
        }
        if ($sameDeclaration) {
            continue
        }

        $sourceRecord.Output.effective = $false
        $sourceRecord.Output.overriddenBy = $candidate.Output.recordId
        Add-PackageDiagnostic -Package $package -Diagnostic (New-Diagnostic `
                -Code 'supplemental-overridden' `
                -Severity 'warning' `
                -Message "Supplemental registration takes priority over the source declaration for OID $($sourceRecord.Oid), type $($sourceRecord.Type)." `
                -ManifestPath $sourceRecord.Output.manifestPath `
                -Line $sourceRecord.Output.line `
                -Oid $sourceRecord.Oid `
                -Type $sourceRecord.Type `
                -RecordId $sourceRecord.Output.recordId `
                -RelatedManifestPath $candidate.Output.manifestPath `
                -RelatedRecordId $candidate.Output.recordId)
        $recoveryMessage = if ($candidate.Resolution -eq 'resolved') {
            'Supplemental declaration resolved to a DAT under LibraryRoot.'
        }
        else {
            'Supplemental declaration was retained but did not resolve to a DAT under LibraryRoot.'
        }
        Add-PackageDiagnostic -Package $package -Diagnostic (New-Diagnostic `
                -Code 'supplemental-recovery' `
                -Severity $(if ($candidate.Resolution -eq 'resolved') { 'info' } else { 'warning' }) `
                -Message $recoveryMessage `
                -ManifestPath $candidate.Output.manifestPath `
                -Line $candidate.Output.line `
                -Oid $candidate.Oid `
                -Type $candidate.Type `
                -RecordId $candidate.Output.recordId `
                -RelatedManifestPath $sourceRecord.Output.manifestPath `
                -RelatedRecordId $sourceRecord.Output.recordId)
    }
}

$packageOutputs = @()
foreach ($package in @($packagesByKey.Values | Sort-Object -Property RelativeDirectory)) {
    $packageDatFiles = @(
        $sourceFiles |
            Where-Object {
                $_.Extension -ieq '.dat' -and (Test-SameOrUnderPath -Candidate $_.FullName -Root $package.DirectoryFull)
            } |
            ForEach-Object { Get-RelativePathFromRoot -Path $_.FullName -Root $libraryRootFull } |
            Sort-Object
    )
    $packageBmpFiles = @(
        $sourceFiles |
            Where-Object {
                $_.Extension -ieq '.bmp' -and (Test-SameOrUnderPath -Candidate $_.FullName -Root $package.DirectoryFull)
            } |
            ForEach-Object { Get-RelativePathFromRoot -Path $_.FullName -Root $libraryRootFull } |
            Sort-Object
    )

    $recordOutputs = @()
    foreach ($record in @($package.Records | Sort-Object -Property @{ Expression = { $_.Output.manifestSource } }, @{ Expression = { $_.Output.manifestPath } }, @{ Expression = { $_.Output.line } }, @{ Expression = { $_.Output.oid } }, @{ Expression = { $_.Output.type } }, @{ Expression = { $_.Output.recordId } })) {
        $recordOutputs += ,[ordered]@{
            oid = $record.Oid
            type = $record.Type
            file = $record.Output.file
            logicalPath = $record.LogicalPath
            manifestSource = $record.ManifestSource
            manifestPath = $record.Output.manifestPath
        }
    }
    $diagnosticOutputs = @()
    foreach ($diagnostic in @($package.Diagnostics |
            Sort-Object -Property @{ Expression = { $_.code } }, @{ Expression = { $_.manifestPath } }, @{ Expression = { $_.line } }, @{ Expression = { $_.recordId } }, @{ Expression = { $_.message } })) {
        $diagnosticOutputs += ,[ordered]@{
            code = $diagnostic.code
            severity = $diagnostic.severity
            message = $diagnostic.message
        }
    }

    $packageOutputs += ,[ordered]@{
        packageId = Get-PackageId -RelativeDirectory $package.RelativeDirectory
        relativeDirectory = $package.RelativeDirectory
        label = Get-PackageLabel -RelativeDirectory $package.RelativeDirectory
        records = $recordOutputs
        datFiles = $packageDatFiles
        bmpFiles = $packageBmpFiles
        diagnostics = $diagnosticOutputs
    }
}

$document = [ordered]@{
    schemaVersion = 1
    schema = 'ntsd-patch-index'
    pathBase = 'LibraryRoot'
    supplementalManifestPattern = 'ID.editor-*.txt'
    limits = [ordered]@{
        maxTextBytes = $MaxTextBytes
        maxFileCount = $MaxFileCount
    }
    scan = [ordered]@{
        libraryFileCount = $libraryFiles.Count
        sourceFileCount = $sourceFiles.Count
        supplementalFileCount = $supplementalFiles.Count
        sourceTextFileCount = $sourceManifestFiles.Count
        supplementalManifestFileCount = $supplementalManifestFiles.Count
        totalFileCount = $totalFileCount
        datBmpSource = 'LibraryRoot only'
    }
    packages = $packageOutputs
    diagnostics = @(
        $globalDiagnostics |
            Sort-Object -Property @{ Expression = { $_.code } }, @{ Expression = { $_.manifestPath } }, @{ Expression = { $_.message } } |
            ForEach-Object {
                [ordered]@{
                    code = $_.code
                    severity = $_.severity
                    message = $_.message
                }
            }
    )
}

$json = $document | ConvertTo-Json -Depth 32
$outputParent = Split-Path -Parent $outputPathFull
if ([string]::IsNullOrEmpty($outputParent)) {
    $outputParent = (Get-Location).Path
}
if (-not [IO.Directory]::Exists($outputParent)) {
    [IO.Directory]::CreateDirectory($outputParent) | Out-Null
}
$utf8NoBom = New-Object -TypeName System.Text.UTF8Encoding -ArgumentList $false
[IO.File]::WriteAllText($outputPathFull, ($json + [Environment]::NewLine), $utf8NoBom)

Write-Output "Wrote patch index: $outputPathFull"
