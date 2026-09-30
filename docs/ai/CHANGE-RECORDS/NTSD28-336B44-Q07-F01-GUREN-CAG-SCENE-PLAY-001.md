<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-F01-GUREN-CAG-SCENE-PLAY-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07GurenCagBattlePlayProbeEditor.cs
authority: selected formal 336B44 root natural Guren/OID619/Lee witness; original Unity Battle Scene production Driver
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-F01-GUREN-CAG-SCENE-PLAY-001.md
-->

# NTSD28-336B44-Q07-F01-GUREN-CAG-SCENE-PLAY-001

Created before script edits. The existing Unity bdefend field fix has focused/self-check support, and the separate root EXE natural contact witness is complete; the original Battle Scene's production Driver has not observed this same content chain. This change owns only an Editor diagnostic request carrier. It may alter initial actor runtime fields in a Play clone and write a diagnostic report, but must leave saved assets and production code unchanged. [Task](../TASKS/NTSD28-336B44-Q07-F01-GUREN-CAG-SCENE-PLAY-001.md) specifies timing, failure exit, rollback and verification.

Actual code added: `NTSD28Q07GurenCagBattlePlayProbeEditor.cs` registers one request carrier, only enters Play from the sole clean saved Battle Scene, configures Guren/Lee in the Play clone before Bootstrap Start, pauses a stable production World, maps source-rule initial positions through `BattleSpatialProjection`, executes at most 20 complete neutral Driver ticks, records actor/child/HP data and exits Play. It does not change a production rule or saved asset. Unity import/compile and Play validation are pending.

Validation addendum: original Editor `Assets/Refresh` led to Tundra build success with zero observed C# errors; the single request `guren-cag-scene-01` completed PASS/DONE/exitedPlay with scene clean. Relative tick11 OID619 and tick12 Lee damage/action matched selected formal source and frozen-root witness. Unity/source comparison over six fields for 20 declared ticks was 120/120 equal with no first difference. Battle/Menu Scene and both protected config Asset hashes were unchanged. Scope limit: the Play fixture sets action/positions in the running clone; no physical player input or full-world parity was tested. [Report](../../../artifacts/diagnostics/NTSD28-336B44-Q07-F01-GUREN-CAG-SCENE-PLAY-001/REPORT.md). Rollback is limited to this diagnostic probe after reviewing the dirty tree; do not revert shared work or delete produced evidence.

Final audit: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` PASS (1051 records, 11 governed code files in shared diff); `git -c core.safecrlf=false diff --check` PASS. Original Editor returned idle/non-Play, and protected Scene/asset hashes were rechecked. No broad Unity suite was rerun because the only new code is an Editor-only diagnostic carrier and its one bounded Play run supplied its acceptance evidence.
