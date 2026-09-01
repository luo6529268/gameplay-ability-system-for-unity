# CLIENT-FORMAL-KERNEL-STAGE-SPAWN-VALUE-SEAM-001

<!-- CHANGE-RECORD
id: CLIENT-FORMAL-KERNEL-STAGE-SPAWN-VALUE-SEAM-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Simulation/BattleStageSpawnValue.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleRuntimeState.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleStageCampaignLoader.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorld.StageWave.partial.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/StageSpawnTaskConfigurator.cs
code-path: Assets/NTSD/Scripts/Test/Editor/FormalKernelStageSpawnValueSeamEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Server same-ID Task/Change Record; closed Queue0ch audit; C++ release StageSpawnData fields; user standing bounded Client authorization.
evidence: FOCUSED_TEST_PASS / STAGE_SPAWN_VALUE_SEAM_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / USER_STANDING_AUTHORIZED / TEST_FIRST_CS0246_X6_CS1061_X1 / UNITY_COMPILE_0 / FOCUSED_4_4 / RELATED_23_23 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED
-->

> Status: `FOCUSED_TEST_PASS / STAGE_SPAWN_VALUE_SEAM_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / USER_STANDING_AUTHORIZED / TEST_FIRST_CS0246_X6_CS1061_X1 / UNITY_COMPILE_0 / FOCUSED_4_4 / RELATED_23_23 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`

## Scope and intent

Introduce an immutable eight-scalar stage-spawn value, adapt loader/normal/reserve
paths, remove the mutable reserve scratch and preserve allocation-free behavior.
No shared-owner, asset, rule, tick, transport/recovery, AI or marker action.

## Actual implementation and verification

- Added immutable eight-scalar `BattleStageSpawnValue`, DTO projection and
  shared normal/reserve task/factory mapping; removed the mutable reserve scratch.
- Added focused and SelfCheck coverage. Focused job
  `4c07c8388e644326afb017573c4b6fb2` passed `4/4`, including warmed `0 B`;
  related jobs passed `2/2`, `4/4` and `17/17`; fresh SelfCheck wrote `PASS`.
- Unity compile has zero current C# errors. Server Debug/Release and all four
  self-hosted suites passed. Formal marker and S0 remain unchanged.
