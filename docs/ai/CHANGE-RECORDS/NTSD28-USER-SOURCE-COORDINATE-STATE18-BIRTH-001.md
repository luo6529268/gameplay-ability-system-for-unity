<!-- CHANGE-RECORD
id: NTSD28-USER-SOURCE-COORDINATE-STATE18-BIRTH-001
status: FOCUSED_TEST_PASS
change-kind: D024_STATE18_RAW_SOURCE_PARTICLE_BIRTH
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNativeState18ParticleWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06State18SpawnEditorTests.cs
authority: user D-024 ratio decision; paired playable BattleWorld28 materialize_state18_broken_weapon_particles precise/int birth order
evidence: docs/ai/TASKS/NTSD28-USER-SOURCE-COORDINATE-STATE18-BIRTH-001.md
-->

# NTSD28-USER-SOURCE-COORDINATE-STATE18-BIRTH-001

Pre-script PLANNED record. Unity's physical state-18/19 particle X applies the approved view ratio but the independent formal source-rule child history is absent. Current formal source writes child precise X from source precise X plus already-drawn raw random X, preserves source precise Z, and leaves generic-birth integer mirrors until sync. This package propagates those source precise/int values in the existing task, reusing the same random draws. It must not alter the physical position, RNG, DAT, Scene, camera, admission or source-rule consumers. Acceptance and rollback are in the Task; focused first-difference/runtime evidence pending.

## Execution evidence 2026-09-24

- Changed BattleNativeState18ParticleWriter.Materialize only: for initialized parent history, task source precise X = parent SourceRuleX + existing dx, Z = parent SourceRuleZ; task initial source ints = parent SourceRuleXInt/ZInt. Physical values, random draws/order, Y/velocities, admission, default-Z skip remain unchanged. Existing dirty physical-ratio edits were preserved.
- Added NTSD28Q06State18SpawnEditorTests.ParticleBirthPreservesIndependentSourcePreciseAndIntegerHistory: initialized/absent by factor1/configured 2048x1152; independent negative source X precise/int and Z precise/int; exact witness random calls/CRT count, child count, physical coordinates and independent source fields.
- Original Editor RED job dc51741a3def4b858cc89621cdc87972: 4 executed, absent controls 2 PASS, initialized 2 FAIL (expected child initialized true, actual false). GREEN job 6a2dc3bdf77941218379ca945b28e1af: 6/6 PASS, new four plus TransitionParticlePreciseXRelativeOffsetUsesViewRatio two; no full matrix. Fresh refresh/domain reload completed, error console 0 entries.
- Commands: bridge refresh_unity/run_tests/get_test_job/read_console; Tools/Validate-ChangeLedger.ps1 PASS (748 records; log Temp/NTSD28-USER-SOURCE-COORDINATE-STATE18-BIRTH-001-ledger.log); git -c core.safecrlf=false diff --check PASS. NTSD_Battle SHA256 9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0 unchanged; git status for Content/Config/Scene/ProjectSettings empty.
- Limits: focused writer acceptance only, no fresh full SelfCheck, real Play or new formal EXE capture; existing paired formal witness used. CARRIER_NOT_ACTIVE; no source gameplay consumer changes. Overall alignment/Q07 remains open.
