<!-- CHANGE-RECORD
id: NTSD28-USER-SOURCE-COORDINATE-REVIVAL-EFFECT-BIRTH-001
status: FOCUSED_TEST_PASS
change-kind: D024_CONTINUATION_EFFECT_SOURCE_BIRTH
code-path: Assets/NTSD/Scripts/Simulation/Passes/Respawn/BattleRespawnModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C07RevivalProductionPlacementEditorTests.cs
authority: user D-024 proportional physical travel decision and formal playable SimulationTickDriver28 continuation OID998 action6 spawn_transient integer birth
evidence: docs/ai/TASKS/NTSD28-USER-SOURCE-COORDINATE-REVIVAL-EFFECT-BIRTH-001.md
-->

# NTSD28-USER-SOURCE-COORDINATE-REVIVAL-EFFECT-BIRTH-001

2026-09-24 focused result: the previous broad job `2ed78c0b98214a5ba14ec92250d3b9f8` lost its handle with original Editor PID288224 and is not acceptance evidence. New original-project Editor PID10576/bridge6403 loaded the current scripts. The actual-factory source-birth matrix passed exact filtered job `0a019ffb7ef6469e85fb2756b58cb7f3ad` 4/4. The C07 class first ran 6/7 in job `9155fd12b743482a85a9dcbe5976f3ad`, failing only an old phase-index assertion. Separate test-only correction `NTSD28-USER-C07-REVIVAL-PHASE-ASSERTION-ORDER-001` updated that assertion, and final exact class job `6f5089efa8e54b7a98cb594ba2d382c2` passed 7/7. No reliable pre-production RED was captured, so the test-first substep remains unproven. State is `FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE`; full Driver, Battle Scene Play, formal EXE and normal revival source average remain pending.

Pre-script `PLANNED`. Unity currently births the continuation effect from scaled physical host integers and leaves independent source-rule child history absent. The formal playable creates OID998/action6 from the host source integer X/Z and Z+1, with precise position rebuilt from those integers. The bounded edit will pass source precise/int through the existing task and final-position helper without changing physical birth, random calls, gameplay readers, DAT, Scene or nonbattle code. Exact tests, side effects and rollback are in the Task. No test or compile result is claimed yet.

2026-09-24 `CODE_WRITTEN`: `BattleRespawnModule.TrySpawnRespawnEffect` now fills source precise/int task fields from the initialized parent's source **integer** X/Z; it leaves task source precise Z at parent integer Z so the shared final-position helper applies the existing +1 exactly once, while the child initial source ZInt is parent integer Z+1. Parent absent-source task remains absent. The physical OID/action/position, RNG, state, admission and recycle statements are unchanged. The C07 Editor fixture now exercises the actual logic-only OID998 factory for factor1/configured view and initialized/absent source, with parent source precise deliberately different from source integers. `git diff --check` and Scene SHA guard pass; compile and focused test pending while a mistakenly unfiltered Unity EditMode job (ID `2ed78c0b98214a5ba14ec92250d3b9f8`, 8420 total) is still running in the original Editor. That all-suite job was caused by a bridge request with filter at the wrong JSON level; it is **not** package validation and its failures must not be counted as this package's result. The installed bridge exposes no cancel-test command, so do not start another test until this job is terminal.
