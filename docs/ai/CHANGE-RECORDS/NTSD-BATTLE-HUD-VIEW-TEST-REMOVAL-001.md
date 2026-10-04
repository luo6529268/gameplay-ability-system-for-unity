<!-- CHANGE-RECORD
id: NTSD-BATTLE-HUD-VIEW-TEST-REMOVAL-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHudViewEditorTests.cs
authority: Explicit user deletion request via delegated source thread
evidence: docs/ai/FILE-OPERATIONS/NTSD-BATTLE-HUD-VIEW-TEST-REMOVAL-001/RECORD.md
-->
# NTSD-BATTLE-HUD-VIEW-TEST-REMOVAL-001
Requirement: remove obsolete BattleHudViewEditorTests and its embedded request runner only. Before: untracked test script contains four view tests, fixture helpers and auto request runner. After: production HUD, event tests and runtime unchanged.
Acceptance: exact two files absent; no source/asset GUID/type references; protected file hashes unchanged; ledger validator. Independent compile if feasible; no Unity Play, save, scene switch or test start. Runtime acceptance remains pending.
Rollback: restore verified byte backups including meta GUID under separate recorded restore operation. Files are untracked so Git restore alone is insufficient.

Verified limited deletion: exact two files absent; backups hash-match; no remaining Assets C#/Scene/Prefab/asset references to removed type/runner/GUID. Production BattleHudView, BattleHudEventEditorTests and both scenes hash-unchanged. No new compile or Unity test run: Editor ownership remains unconfirmed, and this task deletes an unreferenced fixture/runner only. Historical HUD runtime acceptance is still RUNTIME_PENDING. Whole-repository ledger result is recorded separately; no unrelated record corrections.

Validate-ChangeLedger.ps1 PASSED (1224 records, 25 governed diff files); receipt artifacts/diagnostics/NTSD-BATTLE-HUD-VIEW-TEST-REMOVAL-001/ledger.txt. Earlier unrelated Q09 failure no longer appears in current validation; this cleanup did not edit that record.
