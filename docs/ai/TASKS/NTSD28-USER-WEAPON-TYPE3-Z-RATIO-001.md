# NTSD28-USER-WEAPON-TYPE3-Z-RATIO-001

Status `FOCUSED_TEST_PASS / PLAY_AND_FORMAL_VISIBLE_PENDING`. Parent D-024 all-entity ratio; Q07 paused.

Paired playable `physics_integrator.cpp` applies type3 frame `hit_j - 50` to precise Z after motion integration. Unity generic entity production path already scales this final Z displacement while retaining raw `Type3VisualZOffset`; specialized `LF2Weapon.WeaponFlightPhysics` still adds the raw value to both fields. Under D-024, scale only specialized final precise Z displacement. Keep raw tracker, frame DAT, velocity/gravity, Y/floor, camera and nonbattle unchanged.

Declared scripts: `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Weapon.cs` type3 visual-Z final write and `Assets/NTSD/Scripts/Test/Editor/NTSD28B4IdentityXExtrasEditorTests.cs` registered World configured/default caller cases. Show RED/GREEN and focused class in original Editor, then Ledger/diff. Full tick/Play/EXE visible proof remains open.

Rollback: inverse this reviewed diff only; preserve all current work.

Evidence: original Editor registered-World RED `983bcf69755d477cb2475d6d71817c0d` (configured-view first-difference), whole focused class GREEN `3dde7fbe889e4adab9a7eeff7ca6b492` 12/12. Raw visual-offset tracker and integer Z mirror preserve source semantics.
