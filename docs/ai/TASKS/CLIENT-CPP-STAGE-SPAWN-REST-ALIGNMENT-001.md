# Task Contract — CLIENT-CPP-STAGE-SPAWN-REST-ALIGNMENT-001

> 状态：`FOCUSED_TEST_PASS / STAGE_SPAWN_REST_ALIGNMENT_READY / GOVERNANCE_CLOSED / USER_AUTHORIZED / S0_NOT_VERIFIED`
> 创建：2026-08-30

## 1. 目标

按 C++ release normal-build live path 修正 Unity `StageSpawnAt` 的复用槽 rest 行为：成功占用复用 runtime slot 时，必须清除该槽的 ARest、VRest victim row 和 VRest attacker column；若该槽已有冲突的 Unity-native rest binding lease，则整个 spawn 必须 fail closed，不能失效或改写既有 lease，不能泄漏 renderer/logic pool，也不能产生成功 `allocationEpoch`。

## 2. Authority

- 用户于 2026-08-30 对 `CLIENT-CPP-STAGE-SPAWN-REST-ALIGNMENT-001` 的精确授权。
- C++ release live path：`spawn_stage_immediate_entry_slot -> spawn_at -> spawn_into_slot -> reset_cooldowns(slot)`。
- `reset_cooldowns(slot)` 的可观察副作用：清 `s_arest[slot]`、`s_vrest[slot][*]` 与 `s_vrest[*][slot]`。
- Server audit：`NTSD_Server/docs/ai/AUDITS/S0-STAGE-SPAWN-REST-ALIGNMENT-PREREQUISITE-001.md`。

## 3. 允许修改范围

- `Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs`
- `Assets/NTSD/Scripts/Simulation/SimulationWorld.StageWave.partial.cs`
- `Assets/NTSD/Scripts/Simulation/SimulationQueryAndLinkModule.cs`
- `Assets/NTSD/Scripts/Simulation/RuntimeRestStore.cs`
- 必要时 `Assets/NTSD/Scripts/Animation/Character/LF2ItrRestTracker.cs`
- StageSpawn reused-slot / rest-lease rollback focused Editor tests
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` 中对应 StageSpawn/rest fixture
- 本 Change 的 Task、Record、Ledger、State、Handoff、S0/S5 阶段记录与必要 `.meta`

## 4. 必须保持的不变量

- 成功 StageSpawn 清除复用槽的三类 rest 数据，并且仍使用既有 first-free runtime slot 与 StageSpawn semantic。
- 冲突 lease 在失败后仍有效；其 token、ARest、VRest 数据均不被修改。
- 失败路径不保留 runtime-slot occupant，不增加成功 allocationEpoch，不泄漏 renderer 或 logic pool 实例，不改变 kill/stats。
- 普通非 StageSpawn 注册行为、slot/generation local lease safety、Authority400 容量与现有对象池边界保持不变。
- 不把 Unity `Generation` 改成 formal allocation epoch；本包只是 Cut C 前置 authority correction。

## 5. 测试与验收

1. 先建立聚焦 red tests，覆盖成功复用槽清理和冲突 lease 原子失败。
2. Unity 脚本编译 `error CS=0`。
3. StageSpawn/rest focused tests 通过。
4. `BattleRuntimeSelfCheck` fresh PASS。
5. S0 witness 与 existing lockstep regressions 通过。
6. Client/Server Ledger、S0-S9 workflow、ClientImpact matrix 与 scoped diff 检查通过。

上述 focused 证据不等于完整 Play Mode 场景对齐，也不晋升 S0/S5 或 formal marker。

## 6. 禁止事项

- 禁止修改 battle rules、30 Hz、Input Actions、`TargetTick`/`InputDelayFrames`、transport、Socket、数据库、公网、snapshot/recovery、formal AI、formal marker、Scene、资源或默认 `stage.dat` 部署。
- 禁止为通过测试而清除/抢占冲突 lease，或在失败后补偿性重建一个不同 lease。
- 禁止移动 Cut C slot/lifecycle owner、修改 shared package ABI 或提前执行后续 Cut。

## 7. 回滚

只回退本 Change Record 列出的 StageSpawn/rest transaction、focused tests 和相应治理状态；保留用户工作树、FrameInput/RNG shared-owner、S0 witness、Cut C audits 与所有无关修改。

## 8. 完成证据

- Unity compile：新测试已导入，Console `error CS=0`。
- StageSpawn/rest focused：`2/2` 通过。
- `BattleRuntimeSelfCheck`：fresh result `PASS`（2026-08-30 14:13:29）。
- S0 witness：`8/8` 通过；existing lockstep：`9/9` 通过。
- 本包只达到 focused correction ready；未晋升 S0/S5、未翻转 formal marker、未执行 Scene/资源/网络/恢复等禁止项。
