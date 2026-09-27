<!-- CHANGE-RECORD
id: NTSD28-Q07-DIRECT-BATTLE-SOURCE-BIRTH-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/BattleTestBootstrap.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q08DirectBattleNaturalKoTwoCyclePlayModeTests.cs
authority: formal battle birth position and approved D-024 source/view separation, via existing AppManager.SyncParticipantBirthPosition production seam
evidence: artifacts/diagnostics/NTSD28-Q07-DIRECT-BATTLE-SOURCE-BIRTH-001/TASK-CONTRACT.md
-->

# NTSD28-Q07-DIRECT-BATTLE-SOURCE-BIRTH-001

Before script edit: original Battle Scene direct bootstrap assigns actor `PS.x/z` but never initializes `Runtime.SourceRulePositionInitialized`; actual original Editor Play at tick 1476 shows both actor slots 0/1 false. Menu production birth calls `AppManager.SyncParticipantBirthPosition` after the same position assignment. After this change, direct Battle Scene birth and its direct rematch will call that existing helper after `PS.x/y/z` are set, preserving spawn view position and initializing the original-rule X/Z once.

Scope, risk, exact gate, rollback and protected files are in the Task Contract. Validation pending: original Editor compile, direct Play actor source state/position, Q09 adjacent display-history follow-up, ledger validator and diff check. No stage/Mode DAT or asset modification.

Actual change: one call to the already governed `AppManager.SyncParticipantBirthPosition` after `PS.x/y/z` setup; no second implementation. Original Editor imported/compiled, and direct Battle Scene Play at tick 2 reported slot 0 and 1 `source=True` (pre-change tick 1476 both false). Q09 adjacent central display probe then passed 30 discrete/60 and 120 sampled with runtime position invariants. Direct rematch re-entry has not been separately exercised, so this scoped Change remains `RUNTIME_PENDING`; Q07 overall remains open. `ACCEPTANCE.md` contains exact Play values and unchanged Scene/Asset SHA. The earlier prewarm cancellation was caused by the separate Q09 test probe's invalid timeout teardown, not production content startup.

2026-09-27 before further script editing: extend this same behavioral Change's test ownership only to two `SourceRulePositionInitialized` assertions in the already passing Q08 direct-Battle two-cycle physical-KO Play test helper `AdvanceToNaturalResult`. It is called on the initial and recreated World, so one exact original-Editor run tests the Q07 direct-rematch birth path without changing or duplicating Q08's KO/World/result assertions. The declared test path and pre-edit SHA, exact acceptance and rollback are in the Task Contract. No production, DAT, Scene, mode Asset, nonbattle or Q08 behavior edit is planned. Keep `RUNTIME_PENDING` until the new test actually passes and original Editor/scene state is verified.

2026-09-27 scoped closure: only the two declared test assertions were added; original Editor compiled the edit and ran the exact Editor-registered UnityTest, which entered Play. Preserved XML `artifacts/diagnostics/NTSD28-Q07-DIRECT-BATTLE-SOURCE-BIRTH-001/UNITY-DIRECT-REMATCH-SOURCE-BIRTH-PASS-20260927.xml` SHA-256 `391EBF9FC7DBA51EF9F9CC0F907DC2877AF1D5A84AD5BF4081B42CFCC1E8451E` records 1/1 PASS. Both initial and directly recreated World entered `AdvanceToNaturalResult` with attacker/victim source-rule histories initialized; pre-existing two physical KO/result/World assertions also passed. The accidental PlayMode-filter run executed zero tests and is excluded. MCP job bookkeeping remained stale after Test Runner wrote the final XML; after verifying Editor idle/non-Play and archiving that XML, the stale job was cleared. Battle/Menu/mode Asset disk SHA match the Task pre-run baselines; no production, DAT, Scene or nonbattle change in this extension. This Change is `VERIFIED` only for the direct-Battle birth/re-entry seam; Q07, D-024 aggregate and total alignment remain open. Exact result and scope are in the Task `ACCEPTANCE.md`.

Governance after edit: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` exited 0 with 912 Records and 33 governed code files in the current dirty diff; `git diff --check` exited 0 with only line-ending notices. No broad suite was rerun for this two-assertion diagnostic extension.
