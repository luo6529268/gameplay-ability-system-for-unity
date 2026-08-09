[CmdletBinding()]
param(
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"

$toolRoot = Split-Path -Parent $PSScriptRoot
$cppRoot = "J:\QQFile\NTSD2.4\ntsd_cpp"
$gameRoot = "J:\QQFile\NTSD 2.4.1"
$executable = Join-Path $toolRoot "native\bin\dat_preview_cli.exe"
$artifactRoot = Join-Path $toolRoot "artifacts\native-acceptance"

function Assert-True([bool]$Condition, [string]$Message) {
    if (-not $Condition) {
        throw $Message
    }
}

function Assert-FullCatalog($Preview, [string]$Name) {
    Assert-True ($null -ne $Preview.metadata.catalog) "$Name did not report its data.txt catalog load."
    Assert-True ([int]$Preview.metadata.catalog.entries -gt 100) `
        "$Name did not parse the complete data.txt object catalog."
    Assert-True ([int]$Preview.metadata.catalog.loaded -eq [int]$Preview.metadata.catalog.entries `
        -and [int]$Preview.metadata.catalog.failed -eq 0) `
        "$Name did not load every data.txt DAT entry successfully."
}

function Invoke-Preview(
    [string]$Name,
    [int]$EntryFrame,
    [int]$InitialFrame,
    [string]$InputPlan,
    [int]$Ticks
) {
    $output = Join-Path $artifactRoot "$Name.json"
    $arguments = @(
        "--output", $output,
        "--game-root", $gameRoot,
        "--ticks", [string]$Ticks,
        "--start-frame", [string]$InitialFrame,
        "--entry-frame", [string]$EntryFrame
    )
    if (-not [string]::IsNullOrWhiteSpace($InputPlan)) {
        $arguments += @("--input-plan", $InputPlan)
    }

    Push-Location $cppRoot
    try {
        & $executable @arguments
        if ($LASTEXITCODE -ne 0) {
            throw "Native preview $Name failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        Pop-Location
    }

    return Get-Content -LiteralPath $output -Raw -Encoding UTF8 | ConvertFrom-Json
}

if (-not $NoBuild) {
    & (Join-Path $PSScriptRoot "build-native-preview.ps1")
    if ($LASTEXITCODE -ne 0) {
        throw "Native preview build failed with exit code $LASTEXITCODE."
    }
}

Assert-True (Test-Path -LiteralPath $cppRoot -PathType Container) "C++ reference root is unavailable: $cppRoot"
Assert-True (Test-Path -LiteralPath (Join-Path $gameRoot "data\data.txt") -PathType Leaf) `
    "NTSD 2.4.1 data.txt is unavailable: $gameRoot"
Assert-True (Test-Path -LiteralPath $executable -PathType Leaf) "Native preview adapter is unavailable: $executable"
New-Item -ItemType Directory -Path $artifactRoot -Force | Out-Null

$jump = Invoke-Preview "f210" 210 0 "2:K" 90
Assert-FullCatalog $jump "F210"
$jumpP1 = @($jump.ticks | ForEach-Object { $_.entities | Where-Object slot -eq 0 | Select-Object -First 1 })
$jumpFrames = @($jumpP1.frame)
Assert-True ($jumpFrames -contains 210 -and $jumpFrames -contains 211 -and $jumpFrames -contains 212) `
    "F210 did not naturally advance through 210/211/212."
$firstF212 = $jumpP1 | Where-Object frame -eq 212 | Select-Object -First 1
Assert-True ($null -ne $firstF212 -and [double]$firstF212.v.y -lt 0) `
    "F212 did not receive its Native jump velocity during the 211 -> 212 transition."
$jumpMinimumY = [double](($jumpP1.y | Measure-Object -Minimum).Minimum)
Assert-True ($jumpMinimumY -lt -10) "F210 jump trajectory did not leave the ground."
Assert-True ([double]$jumpP1[-1].y -eq 0) "F210 jump trajectory did not land."

$cloneJump = Invoke-Preview "f265" 265 0 "2:L,4:W,6:J" 120
$cloneJumpP1 = @($cloneJump.ticks | ForEach-Object { $_.entities | Where-Object slot -eq 0 | Select-Object -First 1 })
$cloneJumpFrames = @($cloneJumpP1.frame)
$cloneJumpOids = @($cloneJump.ticks | ForEach-Object { $_.entities } | ForEach-Object { $_.oid })
Assert-True ($cloneJumpFrames -contains 265 -and $cloneJumpFrames -contains 266 -and $cloneJumpFrames -contains 267) `
    "F265 was not reached from the Native hit_Ua input route."
Assert-True ([double](($cloneJumpP1.y | Measure-Object -Minimum).Minimum) -lt -10) `
    "F265/F266 did not apply the DAT-authored vertical motion."
Assert-True ($cloneJumpOids -contains 33) "F265 did not create the existing OID 33 clone."
$cloneRenderResource = $cloneJump.render_resources | Where-Object oid -eq 33 | Select-Object -First 1
$cloneVisibleFrame = $cloneRenderResource.frames | Where-Object frame_id -eq 252 | Select-Object -First 1
Assert-True ($null -ne $cloneRenderResource -and $cloneVisibleFrame.pic -eq 125 `
    -and $cloneVisibleFrame.center_x -eq 39 -and $cloneVisibleFrame.center_y -eq 73) `
    "F265/OID 33 did not expose the C++ render contract used by the Canvas regression."

$massClone = Invoke-Preview "f271" 271 0 "2:L,4:S,6:K" 120
Assert-FullCatalog $massClone "F271"
$massCloneFrames = @($massClone.ticks | ForEach-Object {
    ($_.entities | Where-Object slot -eq 0 | Select-Object -First 1).frame
})
$massCloneOids = @($massClone.ticks | ForEach-Object { $_.entities } | ForEach-Object { $_.oid })
Assert-True ($massCloneFrames -contains 271 -and $massCloneFrames -contains 272) `
    "F271 was not reached from the Native hit_Dj input route."
Assert-True ($massCloneOids -contains 205) "F271/F272 did not create OID 205."
$massClone205Frames = @($massClone.ticks | ForEach-Object {
    $_.entities | Where-Object oid -eq 205 | ForEach-Object frame
})
Assert-True ($massClone205Frames -contains 325 -and $massClone205Frames -notcontains 69 `
    -and $massClone205Frames -notcontains 70) `
    "F271/OID 205 did not follow its type-3 DAT sequence and fell back to the old weapon-frame loop."
Assert-True ($massCloneOids -contains 33) `
    "F271/OID 205 did not complete the Native chain that creates OID 33 clones."
$visibleMassClone = $massClone.ticks | ForEach-Object { $_.entities } |
    Where-Object { $_.oid -eq 33 -and $_.pic -ge 0 -and $_.pic -ne 999 } | Select-Object -First 1
Assert-True ($null -ne $visibleMassClone) `
    "F271 created OID 33 data but never reached a drawable clone frame."
$massCloneEntity = $massClone.ticks | ForEach-Object { $_.entities } | Where-Object oid -eq 205 | Select-Object -First 1
Assert-True ($null -ne $massCloneEntity.display_z -and $null -ne $massCloneEntity.hit_stop) `
    "F271/OID 205 did not expose the Native display fields required by the Canvas renderer."
$massCloneRenderResource = $massClone.render_resources | Where-Object oid -eq 205 | Select-Object -First 1
$massCloneTimerFrame = $massCloneRenderResource.frames | Where-Object frame_id -eq 99 | Select-Object -First 1
Assert-True ($null -ne $massCloneRenderResource -and [int]$massCloneRenderResource.type -eq 3 `
    -and $massCloneTimerFrame.pic -eq 68 `
    -and $massCloneTimerFrame.state -eq 9997 -and $massCloneTimerFrame.center_y -eq 850) `
    "F271/OID 205 did not preserve the data.txt type-3 timer render contract."

$projectile = Invoke-Preview "f263" 263 263 "" 90
$projectileOids = @($projectile.ticks | ForEach-Object { $_.entities } | ForEach-Object { $_.oid })
Assert-True ($projectileOids -contains 121) "Existing F263/OID 121 behavior regressed."
$projectileRenderResource = $projectile.render_resources | Where-Object oid -eq 121 | Select-Object -First 1
Assert-True ($null -ne $projectileRenderResource -and [int]$projectileRenderResource.type -eq 4) `
    "F263/OID 121 did not preserve its data.txt type-4 weapon behavior."

Write-Output "Native preview acceptance passed."
Write-Output "F210 minimum y: $jumpMinimumY; first F212 vy: $($firstF212.v.y)"
Write-Output "F265 minimum y: $((($cloneJumpP1.y | Measure-Object -Minimum).Minimum)); OID 33 observed."
Write-Output "F271 OID 205 type-3 chain produced drawable OID 33 clones; F263 OID 121 remained type 4."
Write-Output "Artifacts: $artifactRoot"
