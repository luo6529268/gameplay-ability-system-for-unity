# NTSD-BATTLE-HUD-VIEW-TEST-REMOVAL-001
Requirement: remove obsolete BattleHudViewEditorTests and its embedded request runner only. Before: untracked test script contains four view tests, fixture helpers and auto request runner. After: production HUD, event tests and runtime unchanged.
Acceptance: exact two files absent; no source/asset GUID/type references; protected file hashes unchanged; ledger validator. Independent compile if feasible; no Unity Play, save, scene switch or test start. Runtime acceptance remains pending.
Rollback: restore verified byte backups including meta GUID under separate recorded restore operation. Files are untracked so Git restore alone is insufficient.
