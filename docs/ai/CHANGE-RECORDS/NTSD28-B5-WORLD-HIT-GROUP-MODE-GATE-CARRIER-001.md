# NTSD28-B5-WORLD-HIT-GROUP-MODE-GATE-CARRIER-001

<!-- CHANGE-RECORD
id: NTSD28-B5-WORLD-HIT-GROUP-MODE-GATE-CARRIER-001
status: VERIFIED
change-kind: CODE_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeState.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldCoreScalarSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshotRestore.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5WorldHitGroupModeGateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5WorldHitResourceRulesCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleStateSnapshotRestoreEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4StatusMotionCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5HitResourceSuppressionCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Kind4SourceCountCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5SpecialHitLatchCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5StandardHitRestDataCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C23C24WorldClockEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25FJStateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25ResourceDisplayCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28CollisionYReferenceCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28EnvironmentStateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeActionDataCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeFunctionKeySessionStateEditorTests.cs
authority: NTSD 2.8-Logan GameSession28 selected_mode_hit_group_gate_18 projection and BattleWorld28 active_mode_hit_group_gate_18_; EXE B1E13AE1, closure 39DDDA15.
evidence: RED compile9; focused19+12+21 pass; B5 529/529; Unity-side NTSD28 1087/1087; build0; 20:27:08 SelfCheck PASS; clean Console; Scene unchanged; Ledger332/285 PASS.
-->

> 状态：`VERIFIED / CARRIER_READY / BEHAVIOR_AND_CONTENT_UNCONNECTED`

新增world `+0x18`确定性carrier；行为接线、background content producer与full-restore consumer均不在本包。

## 实际改动

- `NTSD28HitResourceRulesRuntimeState`新增`ActiveModeHitGroupGate18`、default/reset与4参数snapshot restore；
  旧3参数helper保留并显式恢复default0，避免历史测试夹具无意继承非零模式值。
- core scalar capture/full restore、checksum及full parity新增该字段；schema推进为core9、battle18、checksum21，
  entity runtime仍为11。
- 新专属carrier测试覆盖default/reset/restore、immutable capture、checksum/parity、schema与warm zero-allocation；
  既有精确schema断言机械同步。
- 没有candidate/consumer行为、background content producer、C25 full-restore consumer、Scene或Authority写入。

## 验证

RED compile9；focused19/19、12/12、21/21；B5 529/529（`d9350d30...`）；Unity侧NTSD28
1087/1087（`6754731d...`）；build0；20:27:08 SelfCheck PASS；清空历史预期自检日志后Console0；
Scene dirty=false/root13/SHA `50FD4D8...FF3C`；Ledger332/285 PASS。
