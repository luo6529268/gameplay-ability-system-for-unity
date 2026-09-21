<!-- CHANGE-RECORD
id: NTSD28-Q08-REVIVE-LIVES-LIVING-GROUP-001
status: FOCUSED_TEST_PASS
change-kind: SOURCE_MATCHED_RESULT_LIVING_REVIVE_LIVES
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Results/BattleResultsOutcomeHostWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q08BattleFlowRedProbeEditorTests.cs
authority: formal Logan battle_flow.cpp classify and GameSession28 playable caller; D-023 formal battle content
evidence: isolated Unity three-case RED 1PASS/2FAIL then 3/3 PASS; adjacent result seams 2/2 and 3/3 PASS; original Editor/real battle pending; see ACCEPTANCE
-->

# NTSD28-Q08-REVIVE-LIVES-LIVING-GROUP-001

Parent Task: `docs/ai/TASKS/NTSD28-Q08-REVIVE-LIVES-LIVING-GROUP-001.md`. Before: the result writer uses HP-positive only and excludes HP0/`HP2Orig=2` even though formal `BattleFlow28::classify` retains it. Both a direct writer and an actual Unity tick-2 fixture failed at `BattleEndPhase=1` while native source classification expects timer0. This Record is created before project script edits.

Planned exact change: migrate the two isolated focused fixtures into the new Editor test file, add HP0/one-life boundary, then change only the writer's living predicate to permit `HP2Orig>1`. Preserve active-result gate, mode-4 reserve, current two-bucket/`HadBoth` behavior and after-world result-page input; their separate source-matched redesign remains in the parent Q08 Task. No new state, snapshot, checksum or schema field is authorized in this subtask.

Expected side effect: a character with remaining native revival lives suppresses premature terminal observation. Acceptance: source witness, RED→PASS for direct and full Unity tick fixtures, one-life negative boundary, focused adjacent results seam, compile0, unchanged protected Scene, Ledger validation. Real battle and full Q08 result timeline are later exits. Rollback only exact owned hunks under repository approval rules; no reset/clean.

Actual code paths and duties: `NTSD28Q08BattleFlowRedProbeEditorTests.cs` and `.meta` added as a three-case focused fixture; the test copy is SHA-256 identical to the pre-change isolated copy. With the old predicate, isolated Unity EditMode ran all three cases: HP0/one-life PASS, HP0/two-life direct writer FAIL, HP0/two-life full tick FAIL (`Q08-ReviveLives-ThreeCase-RED.xml` outside repository; prior exact RED XML archived in the caller audit). `BattleResultsOutcomeHostWriter.cs` changes only the living gate from HP-positive to HP-positive-or-`HP2Orig>1`, preserving every other branch. Post-change compile/test and runtime acceptance remain pending. No Scene, data, image or nonbattle file was changed by this Change ID.

Post-change validation: the source and isolated-copy test/writer SHA-256 matched before Unity 2022.3.62f3 EditMode; three focused cases passed 3/3, adjacent outcome seam 2/2 and results-scene host seam 3/3. XML and exact scope are archived in `artifacts/diagnostics/NTSD28-Q08-REVIVE-LIVES-LIVING-GROUP-001/ACCEPTANCE.md`. This is a compile and focused-test pass in a hash-matched copy. Original Editor reload, full SelfCheck, actual battle terminal/continue and same-state native full-driver trace remain unverified; Q08 parent stays active. The earlier sentence saying post-change tests are pending is historical pre-validation state.
