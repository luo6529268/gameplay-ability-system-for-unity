<!-- CHANGE-RECORD
id: NTSD28-USER-STATE9996-CHILD-RATIO-001
status: FOCUSED_TEST_PASS
change-kind: D024_STATE9996_SHARED_RELATIVE_CHILD_BIRTH_RATIO
code-path: Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06State9996DirectSpawnEditorTests.cs
authority: shipped Logan playable BattleWorld28::advance_native_special_clones, formal source-final row0, user D-024 fixed-view battle-distance exception
evidence: docs/ai/TASKS/NTSD28-USER-STATE9996-CHILD-RATIO-001.md
-->

# NTSD28-USER-STATE9996-CHILD-RATIO-001

Status: `FOCUSED_TEST_PASS / RUNTIME_PENDING`. The linked Task specifies current source/Unity formulas, measured first difference, exact two-script scope, side effects, RED→GREEN acceptance and rollback. Pre-change Unity computed integer parent X plus raw synchronized X offset, Z+1, then copied those ints into precise direct and initial integer task coordinates. Actual production change in `BattleLateEntityLifecycleModule.SpawnState9996Children`: draw `relativeX` exactly once in the old order, apply `world.FixedViewRunDistanceScale` only when adding the child-relative X offset to the parent's current integer X, and apply `world.FixedViewRunVerticalDistanceScale` to the relative Z+1. Store the resulting double precise X/Z and cast-after-scale integer mirrors. Y, raw velocities, child identity/action/team, structural materializer and formal DAT remain unchanged. The test method in `NTSD28Q06State9996DirectSpawnEditorTests` now validates all five children, both axes, precise/integer birth and fractions in factor1/configured Worlds.

Original Editor evidence: strict ratio job `f15b213dffd9458a8d5fd73e3606f0d1` RED 1/2 on configured child precise X327 against target325.39084771192796; factor1 passed. After production change `68803d1fe0df40749ff8512f17231d4b` passed 2/2. Strengthened five-child final job `f1dc8c0b632a43d4a36c33380a865e53` passed 2/2. Adjacent exact formal row0 immediate and following-tick tests `eae331fdc81a48958c91e511d9da16be` passed 2/2. These focused checks do not close Battle Scene Play, formal EXE visible comparison, other views or the general source-rule reference-coordinate/snapshot/reset/checksum contract. Rollback is inverse only the two reviewed script diffs, preserving all prior dirty work.
