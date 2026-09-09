# Task Contract — NTSD28-B2-AI-ORDINARY-NONHELD-RNG-001

> 状态：`FOCUSED_TEST_PASS / NONHELD_SITES_READY / SURPLUS_69_ISOLATED / PRODUCTION_UNCONNECTED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2`  
> 建立日期：2026-09-03

## 目标

闭合 NTSD 2.8-Logan ordinary combat 的非 held synchronized RNG 路径：动态
`0x1A/0x1C`、`0x1B/0x1D` movement sites，`0x1E→0x21` common tail，单一
`0x26→0x27` low-HP continuation，以及 `0x37→0x3B` profiled tail。同步候选不得再进入
两个重复 prewrite 或旧 profiled helper，由此隔离剩余 4 个无 live ID 随机表达式；legacy CRT
行为保持不变。

## Authority 与字段裁决

- Authority：`source/ntsd28_core/src/simulation/native_ai.cpp` 的
  `derive_difficulty_scalars`、`step_ordinary_combat`、`try_generic_close_attack` 和
  `step_profiled_combat`，均在正式 playable build closure 中。
- `battle_mode` 是静态比赛模式，不是 Unity 每 tick 交替的 input cadence phase；同步候选新增
  `AiDecisionWorldState.BattleMode`，production snapshot 从现有 `BattleGameModeId` 原样捕获。
- `global_direction_lock_0049f608` 在正式 playable `GameSession28::make_tick_options()` 没有写入，
  因而保持 `NativeAiMainContext28` 构造默认 `0`；同步候选不得用 Unity `MoveMode` 冒充。
- 当前 Direction B Config 没有 `use_ai:`，因此 profiled classifier 使用原 object id：family 仅
  `{2,4,6,7,8,9,10,11,33,34}`，`0x3A` 仅 oid34，`0x3B` 仅 oid1。
- legacy 三按钮桥保持：`KeyJump=attack`、`KeyDefend=jump`、`KeyAttack=defend`。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Ai/Kernel/AiDecisionKernel.cs`
- `Assets/NTSD/Scripts/Simulation/Ai/Snapshots/AiDecisionSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Ai/Runtime/SimulationAiDecisionModule.cs`
- `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28AiOrdinaryNonHeldRandomEditorTests.cs` 及 `.meta`
- 本 Task、Change Record、Ledger、STATE、handoff、总表与 B2 AI RNG manifest

禁止修改 Config/DAT/Scene/Prefab/ProjectSettings、authority、held 分支、production synchronized
cursor capture/commit 或 native combo producer/action 接线。

## 不变量

- legacy snapshot 不带 synchronized cursor 时，现有 CRT 分支逐表达式保持。
- synchronized movement 以静态 `BattleMode` 计算 low-HP spacing，且方向锁固定使用已证明的 production
  值 0；不能用 `MoveMode` 或 cadence `InputPhase` 替代。
- `0x1F` 非 `<3` 时不消费 `0x20`；`0x26` 非零或 self state7 时不消费 `0x27`。
- `0x37` 在 direction gate 与 `0x38` 前；`0x39/0x3A/0x3B` 保持条件消费顺序。
- 非 held 包不接 `0x28..0x35`；link/held 路径继续 fail closed，留独立包。

## 验收

- test-first 捕获缺少 BattleMode/native helper 的预期编译失败；
- focused tests 覆盖普通/low-HP 双向动态 ID、MoveMode 不冒充全局锁、1F/20 与 26/27 短路、
  0x21 阈值80、profile family/3A/3B/动作桥、完整非 held 顺序与 4096 zero-allocation；
- 全 AI sensing/decision/character/shadow 回归、Unity compile 0、SelfCheck、Console0；
- Change Ledger validator 通过；production cursor 仍明确未连接。

## 回滚

移除同步 ordinary helper、BattleMode transient carrier 与专用测试，恢复同步路径在这些点 fail closed；
legacy production 由于本包不连接 synchronized cursor，不受回滚影响。

## 结果

- test-first：11 个预期缺口，且无任务外编译错误；
- focused：15/15，job `43f0b6daa03e48e69f9a323d376aeb58`；
- 全 AI sensing/decision/character/shadow：342/342，job
  `a9775155524a4aa89dacdaf60a830826`；
- full non-held candidate 到达 `Complete`，site 顺序为
  `14→3C→1C→1E→1F→21→37→38`；
- 4096 次 warm helper evaluation+commit 为 0 managed allocation；
- SelfCheck 2026-09-03 10:34:40 PASS，清理预期负例后 Console 0 error。
- Change Ledger validator PASS：125 records / 74 governed code files。

生产 synchronized cursor、held `0x28..0x35`、正式 joint trace 和 G-04 mode 语义全矩阵仍未关闭。
