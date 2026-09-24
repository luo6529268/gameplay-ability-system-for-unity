# NTSD28-USER-STATE18-PARTICLE-RATIO-001

Status `FOCUSED_TEST_PASS / PLAY_AND_FORMAL_VISIBLE_PENDING`. Parent D-024 all-entity ratio; Q07 paused.

Paired playable `BattleWorld28::materialize_state18_broken_weapon_particles` creates OID999 at the source integer XYZ, then sets only the child's precise X to source precise X plus random_x. Unity `BattleNativeState18ParticleWriter.Materialize` mirrors this as direct precise X `preciseX+dx` and initial integer X `x`. The configured full-background view requires scaling only that relative precise X, with integer source X deliberately unchanged. Source witness `NTSD28-Q06-C25L-STATE18-SPAWN-SOURCE-WITNESS-001/native.jsonl` contains a reachable leaving-state18 case with nonzero random X.

Declared scripts: `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNativeState18ParticleWriter.cs` precise-X final direct task write and `Assets/NTSD/Scripts/Test/Editor/NTSD28Q06State18SpawnEditorTests.cs` actual structural birth. Test default/configured view and source precise/int distinction; preserve RNG sequence, random Y, velocity, source absolute position, initial integer XYZ and DAT. Verify Original Editor focused and a narrow existing default-source matrix slice, then Ledger/diff. Full Play/EXE visual and Y/floor contract stay open.

Rollback: inverse this Change ID's reviewed lines only, preserving other current work.

Evidence: original Editor RED `22cdabf0cc7c427e89f1492e4b3e1c9f` (configured-view precise X first-difference), GREEN `a7205161599545cc9772cf067163de87` 2/2, adjacent default original-source 64-case chunk `e7b9765dc93e42bf9b28144c169618c6` 1/1. Integer mirror and random draw sequence remain source-controlled.
