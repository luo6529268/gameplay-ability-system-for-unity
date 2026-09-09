# Task Contract — NTSD28-B2-ENVIRONMENT-STATE-CARRIER-001

> 状态：`FOCUSED_TEST_PASS / CARRIER_READY / PRODUCERS_UNCONNECTED / AIR_CONSUMER_PENDING`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2 / NATIVE-TYPE0-BUILTINS`  
> 建立日期：2026-09-04

## 目标

为air/rowing built-ins建立authority `Entity28+0x320 environment_state_320`的精确Unity runtime
carrier：默认0、input-only reset保留、full reset归0、canonical copy、entity/aggregate snapshot和
runtime checksum。只建立数据生命周期，不实现battle_world/physics/hit/catch生产者，不把旧`Unk328`
或其他Unity字段冒充该值。

## Authority 与当前事实

- `input_routing.cpp:726-735`：action182/188 rowing redirect在`environment_state_320<0`时拒绝。
- `physics_integrator.cpp:156`和`battle_world.cpp:1764-1818,2090,3831,3971,4677-4707,5227`：该字段
  同时参与环境伤害、state12/18物理、kind4/11和catch throw-injury；producer属于B4/B5/B6，不能在
  本B2 carrier包猜测。
- authority value-initialized/default实体为0；tests显式覆盖-1、正值及throw injury。
- `NTSD28-B0-ENVIRONMENT-STATE-BINDING-CORRECTION-001`已证明Unity `Unk328`是fusion gate，不能作为
  environment state；当前raw projection故意为null/missing。
- Unity entity runtime snapshot通过`TryCopyCanonicalStateTo`复制完整runtime；新增确定性scalar必须使
  entity snapshot `3→4`、aggregate snapshot `5→6`、checksum `8→9`，否则旧快照可能被误接受。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldEntityRuntimeSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs`
- 新增`Assets/NTSD/Scripts/Test/Editor/NTSD28EnvironmentStateCarrierEditorTests.cs`及`.meta`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28NativeActionDataCarrierEditorTests.cs`（仅更新共享schema断言）
- 本Task、Change Record、Ledger、STATE、handoff与总表

禁止修改raw projection/maturity、battle_world/physics/hit/catch producer、input/air consumer、Config/DAT、
Scene/Prefab、ProjectSettings、Packages或authority。不得将carrier-ready写成environment行为已对齐。

## 不变量

- 默认0；`ResetInputState()`不得清理该战斗状态，`Reset()`必须归0。
- canonical copy与entity snapshot必须精确保留负/零/正值；restore不得alias或改符号。
- runtime checksum必须逐scalar包含该字段；只改变它必须改变checksum。
- schema严格提升到entity4/aggregate6/checksum9，旧版本不能伪装成新版本。
- warm canonical copy/snapshot/checksum保持0 B。
- raw trace在producer未完成前继续`null/missing`，防止默认0被误报为真实生产绑定。

## 验收

- test-first红灯覆盖缺失字段；focused覆盖default、input/full reset、copy、snapshot、checksum、schema与0 B；
- 相关action carrier、snapshot/checksum/ring回归、full SelfCheck、Console0、Ledger/diff通过；
- producer/air consumer仍未连接，后续air core与B4/B5/B6包负责。

## 回滚

移除`EnvironmentState320`及copy/reset/checksum写入，schema恢复3/5/8，删除新测试并恢复既有schema断言；
不触碰ground core、旧Unk328或raw projection。

## 当前证据

- 2026-09-04：新增5个focused tests后触发Unity脚本编译；仅新测试因生产字段
  `NTSDEntityRuntime.EnvironmentState320`不存在而报CS1061/CS0117，test-first红态成立。
- 生产实现后Unity脚本编译最新段0 error；新载体5/5、既有native action carrier 10/10、
  snapshot/restore/checksum/ring 31/31、NTSD28分组149/149全部通过。
- `BattleRuntimeSelfCheck`于约16:01输出“战斗运行时自检通过”；7条预期负向注册/绑定日志清理后
  Console error=0。
- `git diff --check`通过；`Validate-ChangeLedger.ps1`通过（136 records、92 governed code files）。
- raw projection、环境producer和air consumer均未修改；Config/DAT/Scene/Prefab/authority未触碰。
