# NTSD28-USER-WEAPON-PIECE-BIRTH-RATIO-001

Status `FOCUSED_TEST_PASS / PLAY_AND_FORMAL_VISIBLE_PENDING`. Parent D-024 all-entity movement ratio; Q07 paused.

Formal paired playable `BattleWorld28::materialize_weapon_piece_fragments` uses random parent-relative dx/dz for built-in OID999 and DAT weapon-piece fragments. Unity `BattleNativeWeaponPieceWriter.Materialize` currently adds these raw offsets to an already positioned parent and passes an absolute direct birth task. Under D-024, only relative X/Z offset needs view ratio; the parent's absolute position, random draw order and raw launch velocity remain unchanged. Height dy, DAT values and data selection remain raw pending floor/height contract.

Declared scripts: `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNativeWeaponPieceWriter.cs`, `Assets/NTSD/Scripts/Test/Editor/NTSD28Q06NativeWeaponPieceEditorTests.cs`. Test actual structural birth using existing formal synthetic witness at default/configured view, first built-in and DAT-group fragments, including precise and integer mirrors and unchanged RNG sequence. Then factor only random dx/dz at the final spawn position before direct task materialization. Test existing default source comparison, original Editor compile and scoped diff. Full Play/EXE visible and height/floor semantics remain open.

Rollback: inverse only reviewed lines of this package, preserving all other dirty work.

Evidence: original Editor structural RED `8b9f3bfadef943bea9b1b6ac1a8f81de` (two default PASS, two configured X first-difference FAIL), GREEN `40eb744afd284722811f6d688c4fa98b` 4/4 (X/Z precise and integer), adjacent full birth/random witness `bf85b44eb1c6496aba3f9a2ccdbe14b1` 2/2 runtime profiles × 157 cases each.
