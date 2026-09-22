<!-- CHANGE-RECORD
id: NTSD28-Q08-TRANSITION-FREEZE-UNITY-RED-001
status: RUNTIME_PENDING
change-kind: Q08_TRANSITION_FREEZE_FULL_TICK_RED_TEST
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q08BattleFlowRedProbeEditorTests.cs
authority: formal Logan mode4 GameSession28 timer349/350/next source witness and battle_flow transition202
evidence: isolated Unity target compiled and reached mode4 timer350/transition202 then RED expected FrameSequence349 actual350; production integration pending
-->

# NTSD28-Q08-TRANSITION-FREEZE-UNITY-RED-001

Created before the Q08 Editor test edit. Task: `docs/ai/TASKS/NTSD28-Q08-TRANSITION-FREEZE-UNITY-RED-001.md`. Before: Unity native result phase3/transition is produced before combat, but `NTSDBattleTickSystem.RunTick` proceeds through frame/world-clock and collision passes. Formal mode4 full-session source double-run keeps sampled entities/dual RNG stable and `last_tick=null` at350 and after. Planned test uses the same two-group one-dead setup and compares Unity `NativeWorldClock.FrameSequence` across 349→350→next. This is test-only; no production or user content change. Expected outcome is target RED that supports a future integrated host Task, not a workaround early return. Validation and rollback as in Task.

Actual test hunk written: `NTSD28Q08BattleFlowRedProbeEditorTests.ModeFourTransitionStopsCombatWorldOn350AndFollowingTick` drives full `RunReleaseTick` through native timer349, then asserts transition202 at350 and unchanged `NativeWorldClock.FrameSequence` at350/next. Isolated Unity compile/RED remains pending; current `IN_PROGRESS`.

Validation: isolated Unity 2022.3.62f3 target test compiled without C# error, XML `artifacts/diagnostics/NTSD28-Q08-RESULT-TRANSITION-HOST-AUDIT-001/UNITY-MODE4-FREEZE-RED.xml` reports 0/1 PASS with expected 349 versus actual 350 at the exact transition-tick FrameSequence assertion. Timer350 and transition202 assertions passed first; next-tick assertion was not reached. The preceding pending sentence is historical code-written state; current Record `RUNTIME_PENDING` because the future integrated host change is absent. This is an intentional RED, not a green test or Q08 acceptance. Old Q08 focused 10/10 and adjacent3/3 are prior to this new RED test, so the enlarged class is not claimed all-green. No production, Scene, content or nonbattle script edit under this Record.
