<!-- CHANGE-RECORD
id: NTSD28-R06-NATURAL-REVIVAL-UNITY-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28R06NaturalRevivalEditorTests.cs
authority: root Logan EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable GameSession28 step / BattleWorld28 advance_native_revivals; alignment R06
evidence: docs/ai/TASKS/NTSD28-R06-NATURAL-REVIVAL-UNITY-001.md
-->

# NTSD28-R06-NATURAL-REVIVAL-UNITY-001

Before edit: `NTSD28-R06-NATURAL-REVIVAL-ENTRY-001` proved the formally paired playable source naturally reaches death→lying→ordinary revival with target initial lives2 at ticks16/27/54, but the root LFR CLI lacks a life-count override and the current Unity replay schema for the otherwise identical selected-armor fixture strictly requires target lives1. The previous Lee controlled full-Driver test injects a dead initial state. No current 70-tick same-state Unity natural revival evidence exists.

Declared script edits are exactly the two diagnostic C# paths above. Add a new strict lives2/70tick schema while retaining the existing Q08 lives1/26tick schema and every unrelated helper branch; one focused Editor test records selected fields and detects the first same-state divergence. Scenario JSON/output goes to the named artifact directory. No production behavior, DAT values, Scene, mode Asset, nonbattle module or current shared uncommitted work may be changed. Expected side effect is only Editor recompilation and diagnostic output. Acceptance and rollback are in the Task. Validation pending.

Actual edits: `NTSD28UnityRawCaptureEditor.ValidateScenario` accepts the new strict lives2/70tick schema alongside the existing lives1/26tick branch. `NTSD28R06NaturalRevivalEditorTests.NaturalArmorLethalToOrdinaryRevival_MatchesPairedPlayableSource` replays the complete Driver and captures 70 raw rows. Its first Editor run compiled and executed, but reported a diagnostic-only tick1 position mismatch because this fixture has `SourceRulePositionInitialized=false`; the test incorrectly read the uninitialized source-coordinate integers. The nine other compared combat fields matched on all 70 rows. The test now records physical fallback when source coordinates are absent and separately checks the D-024 scaled physical position against formal source position with 1.5-pixel tolerance for per-step integer rounding. That test edit is not yet recompiled or rerun; final status remains pending focused evidence.

Superseding validation: original Editor PID11944 Tundra script compile succeeded with no C# errors; MCP EditMode job `2fe3ea1b55b0471ba793fd499f37c7ff` ran exactly one selected test, `1 passed / 0 failed / 0 skipped`. Nine discrete battle fields matched 630/630, source/Unity key ticks16/27/54 matched, and max scaled X/Z errors were 1.154/0 px. `WithLoganScenarioForReplayTests` completed its ordered-shutdown postcondition with World objects, claimed slots and borrowers all zero. The Battle/Menu Scenes and GameConfig/ProjectBattleModeConfig SHA-256 values were unchanged. Raw CSV, first diagnostic failure and final limits are recorded in `artifacts/diagnostics/NTSD28-R06-NATURAL-REVIVAL-UNITY-001/ACCEPTANCE.md`. This does not validate a physical-key Play sequence or queued revival, and does not settle D-024 collision-domain policy. Rollback remains limited to the declared diagnostic hunks and new test file, respecting protected user work.

Post-edit audit: `pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` passed with historical path warnings; `git -c core.safecrlf=false diff --check` returned 0. No broad suite was run because this package changes only a guarded diagnostic schema and one focused test.
