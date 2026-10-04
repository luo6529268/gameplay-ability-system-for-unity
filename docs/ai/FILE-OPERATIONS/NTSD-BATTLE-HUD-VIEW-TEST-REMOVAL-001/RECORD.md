# NTSD-BATTLE-HUD-VIEW-TEST-REMOVAL-001 / PLANNED
Authorization: user explicitly requests deleting BattleHudViewEditorTests script and corresponding test functions; delegated source thread 01a0ef91-2a9a-763b-af75-4367dfcd1020. Executor current Codex, workspace I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity.
Start UTC 2026-10-03T22:31:11.851633+00:00.
Exact paths, pre-change hashes/status, verified byte backups: artifacts/diagnostics/NTSD-BATTLE-HUD-VIEW-TEST-REMOVAL-001/prechange.json. Both files UNTRACKED; Git alone cannot restore them. Backups retain exact current content and meta GUID dcbb2e28374f4640bad3f9e936119ecc; restore by copying each backup to its recorded path only after checking no new file exists. No Git index/history change.
Planned command: PowerShell Remove-Item -LiteralPath for exactly these two resolved paths after root containment/hash checks. No recursive deletion.
Remove four test methods plus private Child/Bind, setup/teardown and embedded BattleHudViewRequestRunner callbacks/Poll. No external C# type references found. Other event/play fixture retained; its optional original-scene SessionState read is safe without this runner and its teardown still exits Play.
No scene/editor interactions, production changes or other test removals. Preserve historical test evidence.

VERIFIED deletion at 2026-10-03T22:31:41.1706887Z, PowerShell PID 116440. Both exact manifest paths removed with Remove-Item -LiteralPath after root/SHA checks; exit success. Backups remain outside deletion scope.
