# CLIENT-FORMAL-KERNEL-BDY-VALUE-SEAM-001

<!-- CHANGE-RECORD
id: CLIENT-FORMAL-KERNEL-BDY-VALUE-SEAM-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Simulation/BattleBodyBoxValue.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleBodyBoxValueAdapter.cs
code-path: Assets/NTSD/Scripts/Animation/LF2FrameData.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs
code-path: Assets/NTSD/Scripts/Animation/Character/PhysicsState.cs
code-path: Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterHitResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Weapon.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleEcsWorld.cs
code-path: Assets/NTSD/Scripts/Tools/NTSDHitboxGizmos.cs
code-path: Assets/NTSD/Scripts/Animation/Editor/CharacterFramePreviewWindow.cs
code-path: Assets/NTSD/Scripts/Test/Editor/FormalKernelBodyBoxValueSeamEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/RoleAwareCollisionShadowSelfCheckTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: User standing authorization GOVERNANCE-S0-S9-STANDING-CLIENT-AUTHORIZATION-002; Queue0cv/0cw; C++ release BdyData parse/collision live path.
evidence: VERIFIED / TEST_FIRST_MISSING_SEAM_RED / UNITY_COMPILE_PASS / UNITY_EDITMODE_212_212_PASS / BATTLE_RUNTIME_SELFCHECK_PASS / S0_WITNESS_PASS / EXISTING_LOCKSTEP_PASS / GOLDEN_CORPUS_SHA_PASS / WARMED_0B_PASS / SERVER_DUAL_CONFIGURATION_PASS / CLIENT_INTEGRATION_REQUIRED / STANDING_CLIENT_AUTHORIZED / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED
-->

> Status: `VERIFIED / PRE_CHANGE_SCOPE_RECORDED / TEST_FIRST_MISSING_SEAM_RED / UNITY_COMPILE_PASS / UNITY_EDITMODE_212_212_PASS / BATTLE_RUNTIME_SELFCHECK_PASS / S0_WITNESS_PASS / EXISTING_LOCKSTEP_PASS / GOLDEN_CORPUS_SHA_PASS / WARMED_0B_PASS / SERVER_DUAL_CONFIGURATION_PASS / CLIENT_INTEGRATION_REQUIRED / STANDING_CLIENT_AUTHORIZED / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`

## Plan

Move ordered frame Bdy content to the exact immutable `X,Y,W,H` value, keep
legacy `kind`/raw properties outside canonical content, adapt declared
consumers without changing collision/full-height/mirror/overflow behavior, and
prove the frozen corpus plus existing regressions. The Server Task Contract is
the complete authority/scope/test/rollback specification.

First compile amendment: Unity reported one remaining test-only type
first-difference at `RoleAwareCollisionShadowSelfCheckTests.cs:4672`; that
exact path is declared before adapting its local value type. Assertions and
collision expectations remain frozen.

## Actual result

Immutable ordered Bdy content and declared direct consumers are implemented.
Fresh Unity compile0；focused job `f0b9963974324e95b9db2a7fbac667a7`
6/6；final job `8a4bb5df745a44659ccae65e1824ff49` 212/212；
fresh SelfCheck 11:52:09 PASS；Server Debug/Release PASS；Client Ledger136/88.
Queue0cx is verified；formal marker/S0 unchanged.
