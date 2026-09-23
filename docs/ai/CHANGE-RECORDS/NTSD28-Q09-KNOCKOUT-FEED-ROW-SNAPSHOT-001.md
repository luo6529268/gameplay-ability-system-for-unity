# NTSD28-Q09-KNOCKOUT-FEED-ROW-SNAPSHOT-001

Status: `PLANNED` before script modification. Parent BATCH-05/Q09, R17. Task Contract: `artifacts/diagnostics/NTSD28-Q09-KNOCKOUT-FEED-ROW-SNAPSHOT-001/TASK-CONTRACT.md`.

Authority and current Unity state: formal playable `GameSession28::render_snapshot` and core `render_snapshot.cpp:832-853,1977-2099` define knockout feed names and rows. Unity has Q08 event storage and selected mode lifetime, but `BattlePresentationFrame` currently stores only entities, hit records and commands; no row storage/copy/consumer. Unity `GameLocalSettings` has four ordinary player names, unlike the formal ten battle-name defaults. The selected feed is captured by `LoganModeComboInput`, but has no row publication. Existing Q09 icon/WORDS publication work and all uncommitted changes remain protected.

Declared exact code paths/symbols: `Assets/NTSD/Scripts/App/MatchConfig.cs` (optional battle-only names/brackets); `Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs` (`ApplyMatchConfig`/published-feed preparation); `Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs` (`BattlePresentationFrame` copy/reset and `BattlePresentationCoordinator` capture); new `Assets/NTSD/Scripts/Simulation/Presentation/BattleKnockoutFeedRowProjection.cs` plus meta (row projection); new `Assets/NTSD/Scripts/Test/Editor/NTSD28Q09KnockoutFeedRowSnapshotEditorTests.cs` plus meta (focused acceptance). No other code path is authorized by this record without a pre-edit correction.

Expected effects: immutable battle-session input and deterministic knockout-feed row data in the published frame, preserving native record order, row gaps, label bytes and 30-tick edge across CentralOnly worker and frozen-frame copy. World event/stats, Q08 retention, ordinary HUD/menu, Scene, audio and nonbattle behavior remain untouched. Data-only publication does not imply icon/WORDS pixels or layer parity.

Acceptance: source-first witness and matched focused Unity cases for window edges, gating, missing/excluded actors, icon type, name/align, multi-record spacing, copy/reset and worker path; current source compile, original Editor focused tests after the existing WORDS run reaches a known terminal state, plus `git diff --check` and ChangeLedger validator. Real scene pixel and formal EXE comparison remain Q09/R17 dependencies. Risk: incorrect raw-slot lifetime or sharing mutable input across worker/publication. Rollback: reviewed reversal of this package's exact diff only.

<!-- CHANGE-RECORD
id: NTSD28-Q09-KNOCKOUT-FEED-ROW-SNAPSHOT-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/App/MatchConfig.cs
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattleKnockoutFeedRowProjection.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09KnockoutFeedRowSnapshotEditorTests.cs
authority: formal NTSD2.8-Logan playable GameSession28 render_snapshot and core render_snapshot.cpp knockout feed row path
evidence: artifacts/diagnostics/NTSD28-Q09-KNOCKOUT-FEED-ROW-SNAPSHOT-001/ACCEPTANCE-PENDING.md
-->

Actual change (2026-09-22): the five declared `.cs` paths and two new `.meta` files now implement the battle-only session input, row projection, frame copy/reset/capture and focused test. `SimulationTickDriver` also clears the projection after worker join in its existing ordered shutdown stage. The implementation does not draw rows or change nonbattle UI. Current-source offline runtime+Editor MSBuild exited 0, and `git diff --check` passed; original Editor source import/NUnit, scene pixels and formal EXE parity are pending. See `ACCEPTANCE-PENDING.md`. The status is `CODE_WRITTEN`, not `VERIFIED`; rollback remains package-specific reviewed reversal only.

2026-09-22 pre-edit correction within declared paths: playable `scenario28.cpp:710-725` rejects nonempty player-name tables unless they have exactly ten entries with at most ten bytes each and no NUL. The current Unity `SetNames` silently accepts partial and overlong battle overrides. Correct that input boundary in `BattleKnockoutFeedRowProjection.cs` and the focused test before claiming row data parity. This does not authorize UI, Scene or other input changes.

Correction implemented in `BattleKnockoutFeedRowProjection.SetNames` and `NativeNameOverridesRequireTenBoundedPayloads`; existing override test now supplies ten slots. Validation: original-project offline runtime+Editor MSBuild exit 0 (Temp-only current-sources targets), `git diff --check` exit 0, and ChangeLedger validator PASS (692 records, 27 governed code files). No new original-Editor runtime result; status remains `CODE_WRITTEN` and Q09/R17 remain open.

2026-09-22 pre-edit focused-test correction: original Editor `run_tests` job `c4b9fdf9b056401bac3e9d5508b654ac` is terminal `failed` after five target methods; only `BattleOnlyNamesAndModeGateDoNotUseLocalPlayerSettings` reported expected 1/actual 0. Its second `SetNames` clears the feed per match-reset contract, while the fixture omits production's following `SetFeed`. Modify only this already declared test method to restore the selected feed before checking the disabled-display record count. Production remains unchanged; rerun the one failed method and preserve the RED result.

Test fixture corrected in the declared method only. Original Editor `Assembly-CSharp-Editor.dll` rebuilt at 10:33:35Z after that source edit and domain reload completed. The failed method alone reran in job `30798c0742634db9b2ec83c7b33cbf40`, terminal `succeeded`, summary 1/1 passed; the prior job had executed all five methods and recorded only this fixture failure. Thus all five focused cases have passing evidence across the two targeted runs, with the four unchanged cases from the first run. This is `FOCUSED_TEST_PASS` for row data, not same-tick native parity, screen icon/WORDS consumption, battle Scene pixels, exit/re-entry, or Q09/R17 completion.

Current exact fixture was then rerun as a five-test-only original-Editor job `42d19ac30b5f44dfaff03be3ff84fecd`: terminal `succeeded`, summary 5 passed/0 failed/0 skipped, all five named results Passed. This supersedes the weaker across-run inference above. No broad test suite or other Unity project was run. Q09/R17 remain open for native same-tick and actual visual/lifetime acceptance.

Durable original-Editor results: `artifacts/diagnostics/NTSD28-Q09-KNOCKOUT-FEED-ROW-SNAPSHOT-001/original-editor-{first-red,single-green,focused}-result.json`. Post-correction ChangeLedger validator PASS (692 records/27 governed code files); `git diff --check` exit 0. `total=8208` in TestJobManager discovery progress is not the executed count; the filtered result summary is exactly 5/5.
