# NTSD-BATTLE-HUD-REFRESH-001 / PLANNED
Authorization: source thread 01a0ef91-2a9a-763b-af75-4367dfcd1020 explicitly directs implementing the current field-only HUD, treating pending edits as baseline, displaying character name, preserving scenes/resources.
Executor: delegated Codex task, repository I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity.
Operation: targeted script edits plus append-only governance records. Exact existing paths and SHA/backups: artifacts/diagnostics/NTSD-BATTLE-HUD-REFRESH-001/prechange.json. New paths: docs/ai/CHANGE-RECORDS/NTSD-BATTLE-HUD-REFRESH-001.md, docs/ai/TASKS/NTSD-BATTLE-HUD-REFRESH-001.md, Assets/NTSD/Scripts/Test/Editor/BattleHudViewEditorTests.cs and its meta.
Planned command: guarded Python literal replacements invoked through PowerShell; no deletions, scene saves or asset replacement. Recovery: verify latest state then restore the exact manifest backup with a separately recorded operation. HEAD is not the backup.

Implementation attempt 1: Python guard AssertionError after writing BattleHudView only; driver not written. Scope remains unchanged; retry will use precise shutdown anchor. UTC 2026-10-03T20:33:02.9313830Z.

Existing-script writes completed; test script/meta created. Current UTC 2026-10-03T20:36:21.8525793Z. Test output paths are new files under artifacts/diagnostics/NTSD-BATTLE-HUD-REFRESH-001; request runner refuses Play or dirty scenes, creates result once, deletes no files.

Final status VERIFIED (file operation only), 2026-10-03T20:42:52.773004+00:00. Script replacements and governance edits completed. First guarded attempt partially wrote view, driver retried with exact shutdown anchor; no files deleted. Tests incremental backup: tests-before-host-check.txt; Record before final metadata: record-before-final.txt. New compile target/receipts and request result are within the declared diagnostic folder. Full postchange hashes in postchange.json. Runtime acceptance remains pending; no claim of gameplay verification.
