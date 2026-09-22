# NTSD28-Q08-TRANSITION-FREEZE-UNITY-RED-001

Status: `RED_REPRODUCED / PARENT_IMPLEMENTATION_PENDING / TEST_ONLY`. Parent BATCH-04/Q08 G-05/G-06. Formal mode4 source witness: `artifacts/diagnostics/NTSD28-Q08-RESULT-TRANSITION-HOST-AUDIT-001/REPORT.md` and two identical native runs. This Task measures the corresponding Unity first difference; it does not implement a host transition.

Exact governed code path: `Assets/NTSD/Scripts/Test/Editor/NTSD28Q08BattleFlowRedProbeEditorTests.cs` only. Add one complete-`NTSDBattleTickSystem.RunReleaseTick` mode4 case with a living group and a dead group, matching the formal source fixture. At native timer349 capture Unity world `NativeWorldClock.FrameSequence`; on timer350 and the following tick expect no combat-world frame increment. Assert timer/transition first so the RED pinpoints the still-running battle driver. No production, Scene, content, nonbattle or GAS edit. Use independent Unity clone for this RED, preserve original Editor and Scene.

Acceptance: test compiles, reaches the intended world-frame assertion and fails with measured actual/expected, while prior Q08 focused controls remain in their archived PASS result. A RED establishes a gap, not a fix or Q08 closure. Rollback only this test hunk under repository approval rules, preserving all other uncommitted user work.

Observed: independent Unity target XML `UNITY-MODE4-FREEZE-RED.xml` reports 0/1 PASS, with timer350/transition202 assertions reached, then expected frame sequence349 versus actual350 on the transition tick. Subsequent-tick assertion remains unexecuted until production integration. No broad suite was rerun.
