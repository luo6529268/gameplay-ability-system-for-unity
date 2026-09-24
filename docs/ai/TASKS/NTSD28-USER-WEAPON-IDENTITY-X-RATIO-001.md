# NTSD28-USER-WEAPON-IDENTITY-X-RATIO-001

Status `FOCUSED_TEST_PASS / PLAY_AND_FORMAL_VISIBLE_PENDING`. Parent D-024 all-entity ratio; Q07 paused.

Paired playable `physics_integrator.cpp` applies independent identity/type horizontal displacement at 20% of motion.x before velocity damping. Unity `LF2Weapon.WeaponFlightPhysics` applies `BattleNativeIdentityXExtraKernel.ResolveExtra` directly to `Runtime.X` before common non-character integration. The common X final integration now has D-024's World factor, but this direct extra bypasses it. User-approved fixed full-background screen-fraction exception requires one X factor on this extra displacement, preserving raw velocity and the kernel's identity/type behavior.

Declared scripts: `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Weapon.cs` only the extra X final write and `Assets/NTSD/Scripts/Test/Editor/NTSD28B4IdentityXExtrasEditorTests.cs` actual registered World/configured/default cases. Show RED, patch, run focused identity matrix in original Editor. Preserve DAT, gravity, visual type3 Z, Y/floor, camera and nonbattle.

Rollback: reviewed inverse of this package only; preserve all current dirty work.

Evidence: original Editor registered-World RED `a6bae7590d4d48b69d17490563dee13e` (configured OID120/OID101 raw extras), focused class GREEN `1c11dcfc1d9e46f0a4c5faddb9de0946` 10/10. Whole physics tick/Play and formal EXE visible ratio are not certified.
