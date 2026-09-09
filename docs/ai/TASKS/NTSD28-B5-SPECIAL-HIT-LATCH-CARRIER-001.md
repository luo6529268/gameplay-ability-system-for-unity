# Task Contract — NTSD28-B5-SPECIAL-HIT-LATCH-CARRIER-001

> 状态：`VERIFIED / CARRIER_READY / BEHAVIOR_UNCONNECTED`
> 依赖：`NTSD28-B5-REMAINING-EXIT-AUDIT-006 / VERIFIED`

## 目标

新增与 Authority `EntityState28::special_hit_latch_0eb` 一一对应的独立 bool runtime carrier，闭合
出生/复用清零、input-only reset保留、canonical copy、entity/aggregate snapshot、checksum、parity以及
C++/Unity raw entity trace schema。本包不接任何candidate、hit consumer、type3 producer或HitPlan行为。

## 允许路径

- `Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldEntityRuntimeSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Core/BattleEcsWorld.cs`
- `Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs`
- `Tools/NTSD28Parity/EntityFieldContract.cs`
- `Tools/NTSD28Parity/TraceContractSelfTest.cs`
- `Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp`
- 新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28B5SpecialHitLatchCarrierEditorTests.cs` 与 `.meta`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs`
- `Tools/NTSD28AuthorityTrace/README.md`
- 仅同步受影响schema断言的既有carrier/snapshot Editor tests
- 本Task/Change、Ledger、STATE、handoff、总表

## 不变量

- `SpecialHitLatch0EB` 独立于旧 `HitConfirm2`；本包不得修改任一producer、consumer或逐tick clear。
- 默认构造与full reset为false；`ResetInputState()`必须保留；canonical copy和snapshot roundtrip必须保留。
- entity schema `10→11`、aggregate schema `16→17`、checksum schema `19→20`。
- 只改变latch必须改变lockstep checksum、ECS diagnostic hash与parity；warmed copy/checksum保持0 B。
- Unity raw projection新增严格布尔字段`combat.specialHitLatch0eb`，field/verified计数`48/42→49/43`，missing仍为6。
- parity contract与C++ source-model capture使用同名同类型字段；不得写入正式Authority目录。

## 验收

- test-first得到仅由缺失carrier/schema/raw binding导致的有效red。
- Unity focused carrier/raw/schema tests、B5 tests、Unity侧NTSD28自动回归通过。
- `Tools/NTSD28Parity` build+self-test与AuthorityTrace build/self-test通过。
- fresh compile、BattleRuntimeSelfCheck、Console、Scene hash/dirty与Change Ledger验证通过。

## 回滚

移除新carrier/raw字段与测试，恢复schema `10/16/19`、raw计数`48/42`及工具合同；不得触碰
`HitConfirm2`或任何现有命中行为。

## 完成证据

- test-first external compile red：仅1条`CS1061`指向缺失`SpecialHitLatch0EB`；新测试随后经Unity显式导入。
- fresh Unity compile / external Editor csproj compile均为0 error。
- focused carrier `8553aece3723483d8d3eefbf7b0f2d37` 5/5；Unity entity raw
  `4f0c88c77789437fbb4a324dafe686ba` 3/3。
- schema/carrier相关最终批次：`2a2e861d8d744809ab42323117e4dd90` 10/10、
  `3a791d9625cc4303ae385c7193e34b9c` 8/8、`0264acac5d3a4baaa45670ce237659f4` 9/9、
  Unity raw capture `24c1aef8fd274aceabd4ed9539355f21` 12/12。
- B5 `47d71e479cff4728b7f5bae1bfd9b990` 520/520；Unity侧完整`NTSD28`
  `63d8652e2c8c46a6a52c49abc1d65423` 1078/1078。
- `NTSD28Parity` Release build 0 warning/0 error，contract self-test 21/21、raw comparator 5/5。
- Authority source-model runner build成功；3 ticks/6 entities capture含严格bool
  `specialHitLatch0eb`，validation为`valid-source-model-capture`。这仍是diagnostic，不冒充正式EXE trace。
- `2026-09-06 18:25:08` BattleRuntimeSelfCheck PASS；最终Console 0 error。
- Scene `50FD4D8F...AFF3C` unchanged，active `isDirty=false`、rootCount13。
- Change Ledger 327 records / 282 governed files PASS。

行为producer/consumer/HitPlan仍未接；下一步是独立owner audit。
