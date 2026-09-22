$ErrorActionPreference = 'Stop'
$sources = @(
 'Assets/NTSD/Scripts/Animation/BattleContentSource.cs',
 'Assets/NTSD/Scripts/Animation/LoganModeComboInput.cs',
 'Assets/NTSD/Scripts/Animation/LoganContentIdentity.cs',
 'Assets/NTSD/Scripts/Animation/LoganObjectCatalog.cs',
 'Assets/NTSD/Scripts/Animation/LoganFusionCatalogInput.cs',
 'Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatTokenizer.cs',
 'Assets/NTSD/Scripts/DatParser/Runtime/Models/Lf2DatProperty.cs',
 'Assets/NTSD/Scripts/DatParser/Runtime/Parsing/LoganFusionCatalogParser.cs',
 'Assets/NTSD/Scripts/DatParser/Runtime/Models/LoganFusionCatalog.cs',
 'Assets/NTSD/Scripts/Simulation/Lockstep/Session/LockstepSessionIdentity.cs'
)
Add-Type -Path $sources
function Require($condition, $message) { if (!$condition) { throw $message }; Write-Output "PASS $message" }
$parts = @(0..4 | ForEach-Object { $c = $_; [Convert]::ToHexString([byte[]]@(0..31 | ForEach-Object { $c * 32 + $_ })) })
$v1 = [NTSD.Animation.LoganContentIdentity]::FromBattleComponents($parts[0],$parts[1],$parts[2])
$v2 = [NTSD.Animation.LoganContentIdentity]::FromBattleComponents($parts[0],$parts[1],$parts[2],$parts[3],$parts[4])
Require ($v1.RawDefinitionFingerprint -eq 'C32F2109ABE09F390E5D13936B32E174AD704D64B1B56A9516D573BCF3BE55EF') 'V1 raw vector'
Require ($v1.SemanticFingerprint -eq '68EE16B9C9BCAB61B44C040C057C7686E642662F25DA2B0999FD7359729D0111') 'V1 semantic vector'
$preimage = [Text.Encoding]::ASCII.GetBytes("NTSD28_LOGAN_BATTLE_INPUTS_V2`0") + [byte[]](0..159)
$expected = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([byte[]]$preimage))
Require ($v2.RawDefinitionFingerprint -eq $expected) 'V2 independent raw vector'
Write-Output "V2 raw=$expected semantic=$($v2.SemanticFingerprint)"
$formal = [NTSD.Animation.LoganObjectCatalog]::Read([NTSD.Animation.BattleContentSource]::ForLoganRuntime('J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime'))
$staged = [NTSD.Animation.LoganObjectCatalog]::Read([NTSD.Animation.BattleContentSource]::ForLoganRuntime((Join-Path $PWD 'Assets/NTSD/Content/LoganRuntime')))
Require ($formal.ContentIdentity.SemanticFingerprint -eq $staged.ContentIdentity.SemanticFingerprint) 'Formal/staged complete catalog identity equality'
Require ($staged.ContentIdentity.BattleInputContractTag -eq 'NTSD28_LOGAN_BATTLE_INPUTS_V2') 'Formal/staged use V2'
Require ((@($staged.ModeComboInput.Bound,$staged.ModeComboInput.Facing,$staged.ModeComboInput.Respond,$staged.ModeComboInput.CaughtAct) -join '/') -eq '1/1/50/1') 'Captured tuple 1/1/50/1'
$formal.ModeComboInput.AssertInputsCurrent()
$staged.ModeComboInput.AssertInputsCurrent()
$fixture = Join-Path $PSScriptRoot ('pure-fixture-' + [Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory((Join-Path $fixture 'decoded_dat/data/mode')) | Out-Null
[IO.File]::WriteAllText((Join-Path $fixture 'catalog.csv'), "registry_section,registry_index,id,type,source_path,published_folder`nobject,0,56,0,a.dat,missing`n")
[IO.File]::WriteAllText((Join-Path $fixture 'decoded_dat/a.dat'), '<frame> 0 standing <frame_end>')
$fixtureSource = [NTSD.Animation.BattleContentSource]::ForLoganRuntime($fixture)
$absent = [NTSD.Animation.LoganObjectCatalog]::Read($fixtureSource)
Require ($absent.ContentIdentity.BattleInputContractTag -eq 'NTSD28_LOGAN_BATTLE_INPUTS_V1') 'Absent mode catalog uses V1'
[IO.File]::Copy((Join-Path $staged.Source.DatRoot 'data/mode.dat'), (Join-Path $fixture 'decoded_dat/data/mode.dat'))
[IO.File]::Copy((Join-Path $staged.Source.DatRoot 'data/mode/ntsd.dat'), (Join-Path $fixture 'decoded_dat/data/mode/ntsd.dat'))
$present = [NTSD.Animation.LoganObjectCatalog]::Read($fixtureSource)
Require ($absent.SourceCacheKey -ne $present.SourceCacheKey) 'Mode appearance changes source cache key'
foreach ($relative in @('data/mode.dat','data/mode/ntsd.dat')) {
 [IO.File]::AppendAllText((Join-Path $fixture "decoded_dat/$relative"), "`n")
 $changed = [NTSD.Animation.LoganObjectCatalog]::Read($fixtureSource)
 Require ($changed.ModeComboInput.SemanticFingerprint -eq $present.ModeComboInput.SemanticFingerprint) "Same tuple after byte edit $relative"
 Require ($changed.SourceCacheKey -ne $present.SourceCacheKey) "Byte edit invalidates cache key $relative"
 $rejected = $false
 try { $present.ModeComboInput.AssertInputsCurrent() } catch { $rejected = $_.Exception.InnerException -is [IO.InvalidDataException] }
 Require $rejected "Captured mode freshness rejects byte edit $relative"
 $present = $changed
}
[IO.File]::Move((Join-Path $fixture 'decoded_dat/data/mode.dat'), (Join-Path $fixture 'mode.dat.removed'))
$removed = [NTSD.Animation.LoganObjectCatalog]::Read($fixtureSource)
Require ($removed.SourceCacheKey -eq $absent.SourceCacheKey) 'Mode removal returns exact absent V1 key'
Write-Output "Formal/staged raw=$($staged.ContentIdentity.RawDefinitionFingerprint) semantic=$($staged.ContentIdentity.SemanticFingerprint) modeInput=$($staged.ModeComboInput.InputFingerprint)"
foreach ($file in $sources) { $hash = Get-FileHash -LiteralPath $file -Algorithm SHA256; Write-Output "SOURCE $($hash.Hash) $file" }
Write-Output 'External pure .NET only; no Unity compile, publication, host or Play execution.'
