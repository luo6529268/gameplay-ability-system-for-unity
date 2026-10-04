# NTSD-BATTLE-HUD-EVENTS-001 / PLANNED
User/source thread 01a0ef91-2a9a-763b-af75-4367dfcd1020 explicitly authorizes replacing prior HUD polling with data-driven MMEventManager notifications, complete ready state, generation isolation and worker/main-thread handoff. Existing prior edits are baseline.
Executor: current delegated Codex task, I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity.
Exact existing paths, pre-change SHA, Git state, verified byte backups: artifacts/diagnostics/NTSD-BATTLE-HUD-EVENTS-001/prechange.json. New code: Assets/NTSD/Scripts/Simulation/Presentation/BattleHudChangeTracker.cs and meta, Assets/NTSD/Scripts/Test/Editor/BattleHudEventEditorTests.cs and meta.
Operation: guarded literal Python edits, new files and append-only governance; no file deletion, scene save, DAT/assets or settings changes. Preserve unrelated changes. Restore only exact manifest backup after checking later edits, under a separate recorded operation.
Tests/build receipts are new uniquely named files under this diagnostic folder. Editor operation allowed only when idle, no dirty-scene save, no second project Editor. Main command entrypoints: Python stdin through PowerShell; dotnet generated-project compile; existing Editor TestRunner API if available; Validate-ChangeLedger.ps1.

Initial implementation commands completed exit 0. UTC 2026-10-03T21:40:01.2893238Z. Exact paths per manifest; new tracker/meta created. No Scene or asset writes.

Editor validation action: foreground current PID105896 clean NTSD_Menu, request asset refresh only, no scene save/switch/Play. UI Automation preflight found one clean-title window with no busy dialog. Test request is new and guarded against compile/Play/dirty/busy state. Scene/image/font guard SHA saved in protected-before-editor.json. UTC 2026-10-03T21:45:26.4166994Z.

Round 2 PLANNED 2026-10-03T21:50:16.601338+00:00: exact script backups round2-prechange.json. Correct EditMode lifecycle invocation and add real Battle Play two-cycle acceptance using existing AppManager shutdown owner. No save; restore original clean scene after exit. New result/screenshot files under round2. First run result retained (11 pass, 1 failed test fixture).

Final: no deletions or scene/asset writes. Protected hashes unchanged; request has final failure result and is closed. Code/document postchange hashes follow postchange.json; prior failed tests retained. Status RUNTIME_PENDING due real Play runner failure.
