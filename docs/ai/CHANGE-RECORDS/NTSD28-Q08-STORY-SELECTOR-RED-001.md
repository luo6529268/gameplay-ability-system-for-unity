<!-- CHANGE-RECORD
id: NTSD28-Q08-STORY-SELECTOR-RED-001
status: FOCUSED_TEST_PASS
change-kind: Q08_STORY_SELECTOR_TEST_ORACLE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q08BattleFlowRedProbeEditorTests.cs
authority: matching formal Logan playable GameSession28::step story mission/child selection and parent Q08 story selector gate audit
evidence: initial 4PASS/1FAIL stage-proxy oracle retracted after caller audit; corrected isolated Unity EditMode 5PASS/0FAIL; see STORY-SELECTOR-CALLER-CORRECTION
-->

# NTSD28-Q08-STORY-SELECTOR-RED-001

Parent Task: `docs/ai/TASKS/NTSD28-Q08-STORY-SELECTOR-RED-001.md`. Created before project script edit.

Before: the Unity result producer runs after combat on the existing two-side guard without a story-selector gate. `BattleGameModeId=1` is also the default direct-battle mode; `StageProgressionValid` is derived from a selected campaign and has not been proved equivalent to the playable host's paired story IDs. Existing Q08 group/timing RED and revive2 focused PASS remain intact.

Planned exact script change: append two focused methods in the existing Q08 Editor test class, with one mode-1 direct control and one mode-1 configured campaign case. No existing method will be changed. The intended observable is the current result guard/phase through the complete tick after a group becomes ineligible. No production correction is authorized here.

Expected side effects: tests only; the story case may intentionally fail because the producer has no gate. Preserve all existing tests and Unity/GAS/nonbattle code. Acceptance: targeted Unity compilation/XML in independent Library, clear PASS/RED interpretation, Scene protection, diff check and Ledger validator. Unverified: formal EXE UI, exact Unity-to-formal story selector equivalence and Q08 parent acceptance. Rollback only appended methods under repository approval rules.

Actual change: appended `ModeOneDirectBattleStillStartsTerminalGuard` and `ModeOneSelectedStageDoesNotStartOrdinaryVsGuard` to the declared existing test class; its prior three methods were not changed. Isolated copy class/category renamed only. Unity focused XML is 5 total / 4 passed / 1 target RED, with selected-stage phase expected 0, actual 1; direct mode-1 control and old three pass. Scene SHA unchanged, compile log no CS error. Exact evidence and limits: `artifacts/diagnostics/NTSD28-Q08-RESULT-GROUP-CARRIER-AUDIT-001/STORY-SELECTOR-UNITY-RED.md`. Production behavior is not corrected, so this Record remains `CODE_WRITTEN`, and Q08 remains open.

Postchange checks: `Tools/Validate-ChangeLedger.ps1` exit 0 (658 Records / 24 governed diff files), scoped `git diff --check` exit 0, Unity target class compiled and executed 5 tests. No broader SelfCheck or original Editor Play was run for this diagnostic-only RED.

Correction: the first stage test assumed `StageProgressionValid` was the formal paired story selection. Caller audit disproved that as a warranted assumption: current menu constructors do not set stage campaign fields, and `MatchConfig` has no paired story IDs. Only this Change ID's appended method was renamed and its assertion changed to the no-story-selector current contract. Fresh isolated Unity XML passed 5/5 with zero compile markers; the previous failed XML remains archived, but is not an authority-backed production defect. See `STORY-SELECTOR-CALLER-CORRECTION.md`. This Record is now `FOCUSED_TEST_PASS` for the corrected test oracle only; original Editor Play, formal story selection and Q08 parent remain pending.
