<!-- CHANGE-RECORD
id: NTSD28-Q08-RESULT-PAGE-COMBAT-INPUT-ISOLATION-001
status: RUNTIME_PENDING
change-kind: Q08_RESULTS_PAGE_COMBAT_INPUT_GATE
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleResultsSceneHostTickAlignmentEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: formal Logan GameSession28 combat driver continues with participant input until result transition 350; P-19/G-08 visual exception
evidence: isolated Unity RED 0/1 then focused result-scene 4/4, adjacent held-continue 2/2, fresh full SelfCheck PASS; Ledger 664/6 PASS; real battle and Q08 parent pending
-->

# NTSD28-Q08-RESULT-PAGE-COMBAT-INPUT-ISOLATION-001

Created before code edits. Task: `docs/ai/TASKS/NTSD28-Q08-RESULT-PAGE-COMBAT-INPUT-ISOLATION-001.md`. Current `NTSDBattleTickSystem.RunTick` reads `Results.IsActive` and uses it to skip `PollHumanInput`, change `NativeProducerSampleAndInputRoute` admission, and disable `NeedClearInput` consumption. The old focused test explicitly enshrines null human polling. Formal `GameSession28::step()` still calls the combat driver before transition350; result-page visuals are an approved exception, not an alternate combat-input authority.

Planned exact change: first add a focused active-page/inactive-page same-tick test and obtain RED, then remove `Results.IsActive` from those three combat gates only. Keep it for existing Results UI post-world input. Correct the obsolete test expectation. No host transition, Scene, resource or nonbattle edit. Expected side effect: page visibility no longer changes pre350 combat input or battle-entry clear; post-world page input remains unchanged. Validation and rollback are in the Task. Record actual diff, commands and limitations after scripts.

Actual: `BattleResultsSceneHostTickAlignmentEditorTests.ResultsPageVisibilityDoesNotSuppressCombatHumanInput` compared active/inactive pages on the same combat tick. Isolated Unity `UNITY-PAGE-INPUT-RED.xml` compiled and failed 0/1 exactly at expected `HumanInputPolledExternally=true`, actual false for active page; inactive control passed. In `NTSDBattleTickSystem.RunTick`, only the three combat gates no longer read `resultsActiveAtTickStart`; that flag still reaches post-world `BattleResultsFlow` for Unity UI input. Updated the old Results-active test to expect combat input and input-clear ownership before the following full tail. Existing P1/P2 pressed-edge UI test now expects human combat poll but keeps its page-input assertions. Postchange isolated focused test is running; no acceptance asserted yet. Mode4 transition350 combat freeze and host routing remain outside this task and still RED/pending.

Postchange: isolated Unity focused Results scene input class `UNITY-PAGE-INPUT-POST.xml` 4/4 PASS; adjacent Q08 held-continue pair `UNITY-PAGE-INPUT-ADJACENT.xml` 2/2 PASS. Both runs produced XML and no C# compile error. `Tools/Validate-ChangeLedger.ps1` passed with 664 records/five governed code diff files; exact diff was reviewed. The preceding “running” sentence is historical. Current status is `RUNTIME_PENDING` because original Editor/Player natural battle and the Q08 parent transition/freeze remain unverified. See `PAGE-COMBAT-INPUT-ACCEPTANCE.md` in the Q08 transition audit directory.

Before SelfCheck script edit, additional exact path declared: `BattleRuntimeSelfCheck.cs` only at `CheckAudit7ResultsActiveGate`. Fresh full isolated `UNITY-PAGE-INPUT-SELFCHECK.log` failed at old `BATTLE-AUDIT7-F6` oracle: it expected no human poll and an unconsumed `NeedClearInput` while activating Results. The new combat gate correctly consumes the entry-clear request and returns before full tail; the focused Editor test covers that. Planned SelfCheck correction is to enter the full-tail case with `NeedClearInput=false`, expect human poll, and keep candidate-prelude and P2 UI assertions. This is a test-oracle correction, not new runtime behavior. Rerun compile/full SelfCheck after the edit; do not claim current SelfCheck pass.

SelfCheck correction applied only there: the full-tail fixture starts with `NeedClearInput=false`, expects human input polled and keeps the candidate prelude/P2 page input checks. The second isolated Unity `UNITY-PAGE-INPUT-SELFCHECK-2.log` compiled and logged both SelfCheck PASS and completion; the first failure remains archived. Final `PAGE-INPUT-LEDGER.log` passed 664 records/six governed code diff files, `git diff --check` exit 0 and original Battle Scene SHA unchanged. The preceding rerun instruction is historical; the remaining `RUNTIME_PENDING` is for natural battle/host transition, not SelfCheck.
