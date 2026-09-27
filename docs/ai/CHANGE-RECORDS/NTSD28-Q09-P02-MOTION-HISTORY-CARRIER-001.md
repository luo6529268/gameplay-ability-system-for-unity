<!-- CHANGE-RECORD
id: NTSD28-Q09-P02-MOTION-HISTORY-CARRIER-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattlePresentationBeginFrameReuseEditorTests.cs
authority: formal Logan render_snapshot.cpp presentation rows and presentation_interpolation.cpp adjacent-snapshot gate
evidence: artifacts/diagnostics/NTSD28-Q09-P02-MOTION-HISTORY-CARRIER-001/TASK-CONTRACT.md
-->

# NTSD28-Q09-P02-MOTION-HISTORY-CARRIER-001

Pre-script status was `PLANNED`. The [Task Contract](../../../artifacts/diagnostics/NTSD28-Q09-P02-MOTION-HISTORY-CARRIER-001/TASK-CONTRACT.md) fixes authority, current Unity ownership, exact paths, side effects, invariants, acceptance and rollback.

Before: alternating published frames discard prior logical movement data and are reused after one more tick; frozen presentation copies lack an adjacent motion pair. After: a published frame self-contains current and adjacent-previous pure numeric rows and their tick identity, copied into a frozen submission without old catalog or renderer ownership. This does not yet change visible presentation.

Expected changed symbols: `BattlePresentationFrame` storage/copy/reset; `BattlePresentationCoordinator.CaptureBuildAndPublishFrame` and `CaptureAndBuild` motion capture; one focused Editor test class. No logic truth, movement scale, pass order, 33/3 ms cadence, shutdown order, data values or nonbattle entry changes. Existing coordinator reset clears the new counts; no new stage is introduced.

Risk: current-pass traversal can differ from formal occupied World slots, source/view coordinate confusion, relation sentinel mismatch, and stale A/B history across tick gaps. Tests must expose these explicitly. If any is unresolved, this remains a carrier prerequisite with the gap reported, not Q09/P-02 completion.

2026-09-27 script edit: `BattlePresentationShadowBuild.cs` now defines `BattlePresentationMotionState`, captures source-rule XYZ plus separate physical view XYZ, motion XYZ and raw relation candidates from current registered entities before sprite visibility filtering. `BattlePresentationFrame` owns current and adjacent-previous value arrays and ticks; `CaptureBuildAndPublishFrame` copies the prior current values before reusing the write frame, `CopyFrom` freezes both arrays, and `Reset` clears counts/tick identity. `BattlePresentationBeginFrameReuseEditorTests.cs` adds a focused A/B reuse and frozen-copy test with hidden body, skipped tick, reset, both coordinate domains and six relation values. The new rows are currently unconsumed by the renderer; visible behavior and battle logic are unchanged by design in this prerequisite.

Status `CODE_WRITTEN`. Compile, original Editor focused test, adjacent presentation/purity regression and Play remain pending; no result is inferred from code presence. Risk remains that current-pass traversal or relation sentinels differ from formal World rows, and that source/view interpolation scale needs later formal cross-check. Rollback is limited to the reviewed hunks of the two declared script paths; no destructive Git operation against other work.

2026-09-27 acceptance supersedes the preceding pending-verification sentence: `FOCUSED_TEST_PASS / CARRIER_ONLY`. Final offline Runtime/Editor builds exited 0, original Editor compiled current sources and final focused test 1/1 PASS; adjacent presentation/purity 3/3 and worker ack 1/1 were observed PASS before the final source-availability marker revision. Ledger validator and diff check passed; both protected Scene hashes were stable. Exact job IDs, caveats and evidence are in [ACCEPTANCE](../../../artifacts/diagnostics/NTSD28-Q09-P02-MOTION-HISTORY-CARRIER-001/ACCEPTANCE.md). No visible interpolation, Play or formal EXE visual acceptance is claimed. Earlier `CODE_WRITTEN` is historical.
