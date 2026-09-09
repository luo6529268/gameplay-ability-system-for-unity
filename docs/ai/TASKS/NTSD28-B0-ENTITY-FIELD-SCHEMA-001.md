# Task Contract — NTSD28-B0-ENTITY-FIELD-SCHEMA-001

> 状态：`FOCUSED_TEST_PASS / TRACE-SCHEMA-V2 / SELF-TEST-17-17 / GLOBAL-LEDGER-PASS`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B0`  
> 建立日期：2026-09-02

## 1. 目标

把 B0 v1 只有五个 identity 字段的 entity trace 扩展为版本化的核心实体状态 schema，并明确
记录每个字段的 2.8 authority 来源、Unity 候选来源、绑定置信度和严格比较策略。该 schema 是后续
C++/Unity exporter 的共同消费者合同，不是 runtime parity 证书。

## 2. Authority 与现状

- Authority：`ntsd28_core/include/ntsd28/battle_world.h::EntityState28`、
  `frame_machine.h::FrameCursor28`、`physics_integrator.h::Position28` 与
  `frame_motion.h::Motion28`。
- Playable build participation：`ntsd28_playable/scripts/build.ps1` 明确编译
  `src/simulation/battle_world.cpp` 与 `simulation_tick_driver.cpp`。
- Unity：`Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs` 与
  `Assets/NTSD/Scripts/Simulation/Ecs/` 下的 identity/frame-motion/vital/relation stores。
- 已观察：slot/object/action/position/motion/vital 有直接或近直接候选；authority 的
  `control_slot_000`、多套 action snapshot、armor、platform source 等字段没有已确认的一一绑定，
  必须标成 `MISSING` 或 `CANDIDATE`，不得按名称猜测为 `VERIFIED`。

## 3. 允许文件

- `Tools/NTSD28Parity/EntityFieldContract.cs`（新增）
- `Tools/NTSD28Parity/TraceContract.cs`
- `Tools/NTSD28Parity/TraceComparator.cs`
- `Tools/NTSD28Parity/TraceContractSelfTest.cs`
- `Tools/NTSD28Parity/README.md`
- 本 Task、同 ID Change Record、Ledger、STATE、handoff、对齐总表

不修改 Unity runtime、C++ authority、Scene、Prefab、DAT、资源或旧 `Tools/NTSDParity`。

## 4. Schema 合同

- 版本升级为新的 trace/descriptor/comparison/validation/self-test schema；v1 仅保留为首包历史证据。
- entity 使用固定嵌套组：root identity、`identity`、`frame`、`position`、`motion`、
  `vitals`、`combat`、`lifecycle`。
- 每个组必须 exact-property；缺字段、额外字段、错误 JSON 类型均 fail closed。
- 整数、布尔与 double 类型严格验证；double 必须有限值。
- 所有非用户例外核心字段默认 strict compare；`MISSING/CANDIDATE` 描述的是 exporter 绑定成熟度，
  不能用于静默忽略差异。
- input、relations、rests、events、presentation 仍由各自 domain package 扩展；本包不得把它们
  混入 entity group。

## 5. 验收

1. Release build 0 warning / 0 error。
2. descriptor 稳定并包含完整字段表与绑定状态计数。
3. self-test 在既有 13 case 基础上新增 entity missing/extra/type/binding contract 负向验证。
4. `dotnet format --verify-no-changes` 通过。
5. 全局 Change Ledger validator 通过。
6. 不产生 Unity/C++ runtime 或资源行为变化。

## 6. 回滚

删除新增字段合同并恢复 v1 工具文件；把本 Change 标记 `ROLLED_BACK`，保留 Task/Record 与失败
证据，不删除首包、用户文件或任何权威内容。

## 7. 实际结果

- 核心 entity contract：47 fields；`VERIFIED 21 / CANDIDATE 12 / MISSING 14`；全字段 strict。
- v2 exact-property/type validator 已实现；缺、增、错类型和 binding inventory 均有负向测试。
- Release build 0 warning / 0 error；self-test 17/17；format、diff check、全局 Ledger PASS。
- canonical contract SHA-256：
  `F5E0154AC9EA68576CE4CF2515CA7A271AA71BE8537A3569822C2842DCD6900D`（两次一致）。
- exporter/runtime/resource 未修改，故当前状态不是行为对齐或运行时验收。
