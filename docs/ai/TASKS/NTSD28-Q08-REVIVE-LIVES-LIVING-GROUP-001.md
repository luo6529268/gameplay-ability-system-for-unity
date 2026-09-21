# NTSD28-Q08-REVIVE-LIVES-LIVING-GROUP-001

Status: `FOCUSED_TEST_PASS_ISOLATED / RUNTIME_PENDING`. Parent: `NTSD28-Q08-RESULT-FLOW-SOURCE-MATCHED-001`, BATCH-04/Q08 G-05. The three-case RED→PASS and adjacent 5 tests are archived in `artifacts/diagnostics/NTSD28-Q08-REVIVE-LIVES-LIVING-GROUP-001/ACCEPTANCE.md`. This is the HP/revive-lives eligibility branch only; it does not close Q08 result flow.

Authority: formal root `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`; matching playable `GameSession28::step` calls `BattleFlow28::step`, whose `classify` permits an object-type-0 combatant with HP<=0 when `revive_lives_30c>1`. The source core test `test_remaining_native_life_suppresses_result_timer` passed in the fresh `battle_flow_tests.exe`. Unity `LF2Entity.HP2Orig` is the existing revive-lives carrier. The isolated Unity direct writer and `RunReleaseTick` tick-2 RED each compiled and failed at actual `BattleEndPhase=1` versus source timer0; exact fixtures/XML are in `artifacts/diagnostics/NTSD28-Q08-RESULT-FLOW-CALLER-AUDIT-001/`.

Before: `BattleResultsOutcomeHostWriter.UpdateSummaryActivation` excludes every `Health.HP<=0` combatant before counting groups, so a combatant awaiting native revival can incorrectly start the terminal guard. The old `HadBoth`, two-bucket and mode-4 reserve behavior are separate Q08 differences.

Owned paths: `Assets/NTSD/Scripts/Simulation/Ecs/Results/BattleResultsOutcomeHostWriter.cs`; new focused `Assets/NTSD/Scripts/Test/Editor/NTSD28Q08BattleFlowRedProbeEditorTests.cs` and its `.meta`. No Scene, Prefab, DAT, image, nonbattle UI, GAS, generated code or third-party edit. Do not rewrite or disable existing SelfCheck assertions in this subtask.

Change: count a character as living when `HP>0 || HP2Orig>1`, retaining all other current gates and ordering for the parent Q08 change. First migrate the two archived RED tests to the project, then apply the single producer predicate correction. Acceptance: the two tests switch from RED to PASS in Unity; add a one-life boundary test to show HP0/HP2Orig1 still enters the current terminal path; run the adjacent result seam tests, zero-error compile, and Change Ledger. Real battle, original Editor, same-state native full-driver, reserve and complete result flow remain parent exit items. Any original Scene change must remain intact.

Rollback: reverse only this Change ID's two test/script paths under the repository's approval rules; do not reset/clean the shared worktree.
