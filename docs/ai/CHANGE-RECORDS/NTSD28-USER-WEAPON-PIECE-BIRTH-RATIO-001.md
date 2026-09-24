<!-- CHANGE-RECORD
id: NTSD28-USER-WEAPON-PIECE-BIRTH-RATIO-001
status: FOCUSED_TEST_PASS
change-kind: USER_APPROVED_FRAGMENT_BIRTH_XZ_RATIO
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNativeWeaponPieceWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06NativeWeaponPieceEditorTests.cs
authority: D-024 and paired playable BattleWorld28 materialize_weapon_piece_fragments
evidence: artifacts/diagnostics/NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001/NON-INTEGRATOR-WRITER-AUDIT.md
-->

# NTSD28-USER-WEAPON-PIECE-BIRTH-RATIO-001

Status `FOCUSED_TEST_PASS / PLAY_AND_FORMAL_VISIBLE_PENDING`. Task: `docs/ai/TASKS/NTSD28-USER-WEAPON-PIECE-BIRTH-RATIO-001.md`.

Original Editor structural-birth RED job `8b9f3bfadef943bea9b1b6ac1a8f81de`: two default-view witness cases passed; configured built-in first fragment expected precise X101.536384096 vs raw101, configured DAT-group first fragment expected98.463615904 vs raw99. X assertion precedes configured Z assertion, which will be covered in GREEN. Random seeds and synthetic source events are unchanged.

Pre-change: `BattleNativeWeaponPieceWriter.Materialize` adds random integer dx/dz directly to the parent integer position and dispatches absolute direct birth. Shared movement integration cannot change birth location. Formal DAT and random source remain authority; D-024 user exception changes screen fraction of the relative X/Z offset only.

Intended: scale random dx/dz once at the final child birth output, preserve parent absolute position, height dy, raw velocity, random draws, slot/order, action and selection. Precise double and integer mirror use existing battle rounding convention. Actual structural birth test precedes production patch. No camera, DAT, Scene, or nonbattle changes.

Rollback: reviewed inverse of the declared patch only; preserve other current work.

Actual change: both built-in OID999 and DAT-group fragment final direct-birth X/Z use parent integer position plus random dx/dz times registered World X/Z factors. `Spawn` accepts precise double X/Z and sets the integer mirrors by midpoint-to-even rounding; Y, all launch velocities, calls to `NativeRandom`, action/slot selection and parent absolute coordinates are unchanged. Original Editor compiled after refresh, structural GREEN job `40eb744afd284722811f6d688c4fa98b` passed default/configured × built-in/DAT 4/4 including X/Z precise and integer mirrors. Adjacent default authority synthetic-source comparison job `bf85b44eb1c6496aba3f9a2ccdbe14b1` passed both runtime profiles 2/2; each test iterated 157 source vectors and checked full births/random state. Full Battle Scene Play, formal EXE visible fragment comparison and floor/height contract remain open.
