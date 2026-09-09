# NTSD28-B5-NATIVE-COMBO-CARRIERS-001

<!-- CHANGE-RECORD
id: NTSD28-B5-NATIVE-COMBO-CARRIERS-001
status: VERIFIED
change-kind: BATTLE_STATE_CARRIER
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeState.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Core/BattleEcsWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldEntityRuntimeSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldCoreScalarSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshotRestore.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5NativeComboCarriersEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleWorldEntityRuntimeSnapshotEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4StatusMotionCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5HitResourceSuppressionCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Kind4SourceCountCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5SpecialHitLatchCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5StandardHitRestDataCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5WorldHitGroupModeGateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5WorldHitResourceRulesCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C23C24WorldClockEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25FJStateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28CollisionYReferenceCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25ResourceDisplayCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28EnvironmentStateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeActionDataCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeFunctionKeySessionStateEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan Entity28 combo_hit_count_1e0/combo_hit_last_tick_1e4 and NativeComboRuntimeOptions28 selected-mode tuple; EXE B1E13AE1, closure 39DDDA15.
evidence: missing-type RED; focused 6/6, snapshot-related 29/29, B5 819/819 and NTSD28 1166/1166; fresh SelfCheck PASS; scene hash unchanged.
-->

> 状态：`VERIFIED / CARRIERS_READY / BEHAVIOR_UNCONNECTED`

## Authority与原状

Authority entity拥有独立count/uint64 last tick，world options拥有record/bound/facing/respond/caughtact。
Unity仅有输入combo和伤害累计，world无tuple，entity/ECS/snapshot/checksum也无专用字段；pass描述不等于载体。

## 计划

先写focused carrier/schema测试取得缺字段RED，再按现有runtime/core-scalar模式最小贯通。正式tuple激活留给H；
本包默认关闭且不连接任何producer/expiry/presentation。

## 实施结果

- entity新增独立`NativeComboHitCount1E0`与`NativeComboHitLastTick1E4`，已进入默认值、Reset、canonical copy、
  entity snapshot/restore、ECS shadow、checksum与full parity；未复用输入combo或伤害累计字段。
- world新增`NTSD28NativeComboRuntimeState`，默认严格保持`record=false,bound=0,facing=1,respond=50,caughtact=0`，
  已进入core/full snapshot、restore、checksum与parity；本包没有把正式mode tuple接入内容加载。
- schema按append-only推进：entity runtime `11 -> 12`、core scalar `9 -> 10`、full state `18 -> 19`、
  checksum `21 -> 22`；既有字段顺序未重排。
- 扩大快照兼容测试首次发现其反射填充值分类未支持新增`ulong`字段；随后把
  `BattleWorldEntityRuntimeSnapshotEditorTests.cs`加入实际范围并补齐`ulong`分类，没有改变生产行为。

## 验证

RED：新增focused fixture后Unity编译报`CS0246`，缺少`NTSD28NativeComboRuntimeState`；Test Runner因此
0 tests（job `dd8943c288bb4c54bd41de4fb3c825d9`），Console精确记录1条缺类型错误。此证据取得于
production carrier写入前。

| 层级 | 证据 | 结果 |
|---|---|---|
| focused | job `78e1bb0711d549c0aff38db8cc4f9657` | `6/6 PASS` |
| snapshot/ECS相关 | job `7e3f8b1759a74bcc826c0a600eeac7d9` | `29/29 PASS`；首次job `d61632951fe043f98d8236a155af4f2b`准确暴露缺少`ulong`测试分类 |
| B5 broad | job `20b0aa96579a4ccd96c613527a004eed` | `819/819 PASS` |
| NTSD28 broad | job `9806d4d1540748f18b6c871288fe733c` | `1166/1166 PASS` |
| allocation | focused warm canonical copy + checksum，4096轮 | `0 B` |
| SelfCheck | `Temp/NTSD_BattleRuntimeSelfCheck.result`，2026-09-07 04:23:35 +08 | fresh `PASS` |
| Scene | `Assets/NTSD/Scene/NTSD_Battle.unity` | SHA-256仍为`50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`；mtime未变；未Play/未保存 |
| scoped diff | `git diff --check`目标文件 | `PASS`；仅既有LF/CRLF提示 |
| change ledger | `Tools/Validate-ChangeLedger.ps1` | `PASS`；348 records / 300 governed code files |

SelfCheck会有意触发rest-binding拒绝分支并写出预期error日志，MCP重连亦写入disposed-object日志；最终
Console已清空；随后MCP bridge停止发布实例，故清空后的error读回不可得，但Unity PID 7192仍存活且
`Responding=True`。普通命中producer、C25后expiry、B6 caughtact、B10显示与H正式tuple
激活均未在本包连接，故本结论只证明确定性载体闭合，不等于native combo行为已完成。
