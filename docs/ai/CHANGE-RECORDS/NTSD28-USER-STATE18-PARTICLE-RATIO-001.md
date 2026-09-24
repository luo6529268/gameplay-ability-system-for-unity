<!-- CHANGE-RECORD
id: NTSD28-USER-STATE18-PARTICLE-RATIO-001
status: FOCUSED_TEST_PASS
change-kind: USER_APPROVED_STATE18_PRECISE_X_RATIO
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNativeState18ParticleWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06State18SpawnEditorTests.cs
authority: D-024 and paired playable BattleWorld28 materialize_state18_broken_weapon_particles
evidence: artifacts/diagnostics/NTSD28-Q06-C25L-STATE18-SPAWN-SOURCE-WITNESS-001/native.jsonl
-->

# NTSD28-USER-STATE18-PARTICLE-RATIO-001

Status `FOCUSED_TEST_PASS / PLAY_AND_FORMAL_VISIBLE_PENDING`. Task: `docs/ai/TASKS/NTSD28-USER-STATE18-PARTICLE-RATIO-001.md`.

Original Editor actual structural-birth RED job `22cdabf0cc7c427e89f1492e4b3e1c9f`: default-view state18 source witness passed; configured-view precise X expected117.150225056 vs actual111.25. Integer source coordinate is independently asserted and unchanged in the default case; configured later assertions follow the first failing X.

Pre-change: state18/19 transition particle births at source integer position but precise X is `sourcePreciseX + randomDx`, currently raw under D-024's configured World. Formal random_x and integer/precise split are authority; user exception permits only relative precise X screen-fraction scaling in Unity.

Intended after: factor randomDx once at final `task.directX`; preserve initial integer X/Y/Z, precise parent position, random draw sequence, random Y, velocity, object identity/action and lifecycle. Use original Editor actual structural birth RED/GREEN and default source witness. No DAT/Scene/camera/nonbattle change.

Rollback: reviewed inverse of declared lines only, preserving all other current work.

Actual code: `BattleNativeState18ParticleWriter.Materialize` scales only `dx` at the child's precise-X direct birth task, keeping parent precise X and all initial integer XYZ, random Y, velocity and RNG calls unchanged. Original Editor compiled after refresh. Actual structural RED `22cdabf0cc7c427e89f1492e4b3e1c9f` (default pass, configured precise X111.25 vs expected117.150225056) became GREEN job `a7205161599545cc9772cf067163de87` 2/2 including integer/precise coordinate assertions. Adjacent original-source `BirthAndFullDriverMatchOriginal(Authority400,Legacy,0,1)` job `e7b9765dc93e42bf9b28144c169618c6` passed one chunk of 64 cases. Full Scene Play, EXE visible ratio and Y/floor audit remain open.
