<!-- CHANGE-RECORD
id: NTSD28-Q07-INSPECTOR-FORMAL-REFRESH-001
status: VERIFIED
change-kind: BATTLE_CONTENT_EDITOR_ENTRY
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B11SourceCacheCallerEditorTests.cs
authority: D-023 formal character content and Q07 current old-image owner audit
evidence: docs/ai/TASKS/NTSD28-Q07-INSPECTOR-FORMAL-REFRESH-001.md
-->

# NTSD28-Q07-INSPECTOR-FORMAL-REFRESH-001

Before: the Inspector action always parses old `data.txt` and reads old character images, even while a formal Logan root is selected. This creates a manual authoring path that can republish old content against the project's configured default. Normal configured prewarm already provides an owned, cancellable formal publication path with project mode snapshot and cache freshness.

Planned exact symbols: `CharacterAnimtorManager.RefreshAllData` selects the existing `PrewarmConfiguredLoganContentAsync` when `HasConfiguredLoganContent`; its empty-root legacy branch stays intact. One focused `NTSD28B11SourceCacheCallerEditorTests` case invokes the actual delayed Inspector action with a fixture root and waits for the formal source key. No other script or resource path is authorized under this Change ID.

Expected side effects: the button publishes current formal content when selected; active-battle prewarm remains rejected by existing guard; empty-root authoring behavior stays as before. No DAT token, sprite file, Scene, Prefab, GameConfig, menu UI, GAS or battle tick changes.

Acceptance and rollback: original Editor compile, narrow actual-button test plus adjacent caller check, Scene saved hashes unchanged, ledger validator and diff check. On failure, correct within exact scope or report RUNTIME_PENDING; rollback requires repository approval and targets only this Change ID, without touching user work.

Actual diff: `RefreshAllData` now routes a nonempty configured Logan root through the already-owned `PrewarmConfiguredLoganContentAsync` and exits after publication; the old explicit parse/sprite branch is unchanged for an empty root. Added one actual-button Editor test in the declared B11 caller fixture. No DAT, picture, Scene, Prefab, GameConfig, normal UI, GAS or tick logic changed.

Verification: original Editor refresh compiled, idle/non-Play. Pre-change exact test job `6e0333854715472dab260ba556765284` failed 0/1 on expected formal source key versus null. Post-change exact button plus adjacent direct caller job `5b37372a4480401aa9a4fb0ccf27d8d7` passed 2/2; explicit legacy lazy-load job `f9fa73b6383a4f51be1cf102d4bc91be` passed 1/1. Battle Scene saved SHA `9409F2BCFE3E657A6C3C88A7527045CC384D50AAACC99197D53AACA38F3B3A39` and Menu Scene saved SHA `3B0F58AA88BEC495AA999D014CB2779E935B21F0374826357B4DC64AE5B80228` remained as before. This is verified only for the Inspector's configured formal-root branch; no old-file deletion certificate or natural battle parity claim. See package ACCEPTANCE.
