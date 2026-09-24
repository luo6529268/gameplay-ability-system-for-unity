<!-- CHANGE-RECORD
id: NTSD28-USER-WEAPON-IDENTITY-X-RATIO-001
status: FOCUSED_TEST_PASS
change-kind: USER_APPROVED_WEAPON_IDENTITY_X_RATIO
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Weapon.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4IdentityXExtrasEditorTests.cs
authority: D-024 and paired playable physics_integrator.cpp independent identity X displacement
evidence: docs/ai/TASKS/NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001.md
-->

# NTSD28-USER-WEAPON-IDENTITY-X-RATIO-001

Status `FOCUSED_TEST_PASS / PLAY_AND_FORMAL_VISIBLE_PENDING`. Task: `docs/ai/TASKS/NTSD28-USER-WEAPON-IDENTITY-X-RATIO-001.md`.

Original Editor registered-World RED job `a6bae7590d4d48b69d17490563dee13e`: default-view positive/negative identity extras passed; configured-view OID120 expected203.072768192 vs raw202, OID101 expected196.927231808 vs raw198. Both identify the direct extra X outlet, independent of shared integration.

Pre-change: `LF2Weapon.WeaponFlightPhysics` adds kernel identity X extra directly before shared physics. Shared final integration scales its own travel but not this direct extra. Formal raw source behavior remains authority; D-024 changes only Unity final screen-fraction displacement.

Intended after: multiply the already resolved identity X extra once by the registered World X factor, keeping kernel, raw velocity, gravity, type3 visual Z, identity routing and DAT unchanged. Actual registered World RED/GREEN and adjacent focused test, Editor compile, Ledger/diff. Full Play/EXE visible certificate remains open.

Rollback: reviewed inverse of declared lines only, preserving all other current work.

Actual code: `LF2Weapon.WeaponFlightPhysics` multiplies only the independent identity X extra by the registered World X factor after the kernel resolves its raw result. Raw velocity and all identity branches remain unchanged. Original Editor refreshed/compiled; configured/default positive/negative RED `a6bae7590d4d48b69d17490563dee13e` (two configured failures) became entire focused class GREEN `1c11dcfc1d9e46f0a4c5faddb9de0946` 10/10. Full physics tick/Play/EXE visible screen fraction remain open.
