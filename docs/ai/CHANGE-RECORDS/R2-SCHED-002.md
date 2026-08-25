# R2-SCHED-002 — mode2 tail flag reset 时点

<!-- CHANGE-RECORD
id: R2-SCHED-002
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp::game_tick(...) release live path
evidence: SOURCE-CONTRACT-VERIFIED / UNITY-COMPILE-PASS / FOCUSED-SELF-CHECK-PASS / PLAY-MODE-PENDING / CPP-TRACE-BLOCKED
-->

> 创建日期：2026-08-21  
> 最后更新：2026-08-21  
> 类型：battle / scheduler / tail / self-check

## 1. 状态与范围

- 当前状态：`RUNTIME_PENDING`；本次 source 已在现有 Unity Editor 编译，focused self-check 已实际
  返回 `PASS`；仍没有 Play Mode、C++ runtime trace 或 joint fixture 证据；
- 所属 Work Package：`R2-PASS-02`；
- 覆盖差异：仅 D-SCHED-011 的 `Mode2Request` reset 子边界；
- 不属于本次范围：D-SCHED-006～010、012 的 writer/算法/输入改动，`g_init_stats` / F7、
  candidate、Stage-Z、slot allocator、renderer、DAT、scene、资源与性能；
- 关联 Change ID：`R2-SCHED-001` 是前序 scheduler spine；它仍为 `RUNTIME_PENDING`，
  但不被本 Record 修改或重新验收。

## 2. Authority / 需求依据

- C++ release build 参与性：`Makefile` 包含 `src/entity/game_tick.cpp`；唯一入口为
  `game_tick(...)`；
- C++ source contract：`game_tick.cpp:2083-2089` 先执行 late entity，再 mode2 tail，再
  entity postframe tail，最后清 `g_init_stats` / `g_game_mode2`；
- Unity 原状：`Mode2RandomWeaponDropTailAll` 在自己的内部清零 `Mode2Request`，使其早于
  `EntityPostFrameTailAll` 消失；
- Evidence 等级：C++ 顺序为 `VERIFIED（source）`；Unity 现状为 `VERIFIED（source）`；
  C++ runtime trace / Play Mode 为 `PENDING`。

## 3. 计划改动

| 文件 | 类型 / 方法 | 改前职责 | 目标职责 |
|---|---|---|---|
| `Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs` | `Mode2RandomWeaponDropTailAll` / 新的 tail-reset adapter | mode2 effect 完成后立即清 request。 | 只执行 mode2 effect；不在这里清 request。 |
| `Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs` | `RunPresentationAndCleanupPhase` | mode2 tail 后直接 entity postframe → results。 | entity postframe 后、results 前调用明确的 mode2 reset adapter。 |
| `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` | R2 focused tail check | 未显式验证 request 的跨-tail 生命周期。 | 验证 request 在两个 tail 间保留、reset 后归零。 |

## 4. 不可回退边界

- 不改 CentralOnly、Texture2DArray、dynamic Mesh、URP、1.5× visual scale 或 fixed-world camera；
- 不改 Authority400 / MobileExtended / DesktopExtended、slot/generation、SoA/ECS、pool、worker、0-GC；
- 不新增 `InitStats` 字段，不接入 F7，不改 input；
- 不改 C++ Release、`Assets/NTSD/Scripts/Gen/`、`Assets/Plugins/`、DAT、scene 或资源。

## 5. 实际改动

| 文件 | 类型 / 方法 | 实际改动 | 预期副作用 |
|---|---|---|---|
| `Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs` | `Mode2RandomWeaponDropTailAll` / `ClearMode2RequestAfterPostFrameTail` | 移除 mode2 effect 结束处的立即 `SetMode2Request(0)`；新增只负责 tail 后清零的 adapter。 | request 在同 tick 的 entity postframe tail 期间仍为 1/2；tail 完成后归零。 |
| `Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs` | `RunPresentationAndCleanupPhase` / private reset wrapper | 在 `EntityPostFrameTail` 之后、`BattleResultsFlow` 之前调用新 adapter；保留原 diagnostics phase 划分。 | 与 C++ `g_game_mode2` 清零边界相同；不改变 mode2 effect、entity tail 或 results writer。 |
| `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` | `CheckReleaseTickMode2ResetFollowsEntityPostFrameTail` / `Mode2TailSelfCheckEntity` | 新增最小 probe，观察 mode2 request 在 entity tail carrier-clear 时仍可见、随后才被清零。 | focused regression 会阻止回到提前清零。 |

## 6. 验收、风险与回滚

| 层级 | 验收 | 状态 |
|---|---|---|
| 静态顺序 | mode2 tail → entity postframe → reset → results。 | `PASS`（source + focused probe） |
| focused self-check | mode2 request 的 tail 可见性和结束 reset。 | `PASS`（2026-08-21 22:58:30，request result） |
| Unity compile | 已打开 Unity 实例的脚本编译。 | `PASS`（UnityMCP force scripts refresh；C# `error CS` 查询为 0） |
| Play Mode / C++ trace | 不在本 Record 可独立关闭。 | `PENDING / BLOCKED` |

- 已知风险：任何未发现的 mode2 consumer 可能依赖旧的提前清零；已对 scheduler、tail、ECS、
  StageWave/StageRender 和 self-check consumer 做静态搜索，未发现 entity postframe tail 内的该类 reader；
- 回滚方式：若局部验证失败，创建 correction Record，仅恢复本 Record 的 reset 调用位置，
  不使用破坏性 Git 回退；
- Stop condition：若修复要求 `InitStats`、F7、candidate、Stage-Z、slot 或 render 改动，停止并
  拆为对应 Work Package。

### 本轮实际验证证据

- UnityMCP 连接唯一现有 Editor `gameplay-ability-system-for-unity@b1b02287`（Unity `2022.3.62f3`）；
  无第二个 Unity 进程、本轮未进入 Play Mode；
- `refresh_unity(force / scripts / compile / wait_for_ready)` 完成并恢复 ready；
  `Library/ScriptAssemblies/Assembly-CSharp.dll` 的 UTC 写入时间为
  `2026-08-21T14:53:15.0258553Z`；
- UnityMCP `read_console` 以 `error CS` 过滤返回 0 项；
- `Temp/NTSD_BattleRuntimeSelfCheck.request` 被 Editor 消费，结果文件于
  `2026-08-21T14:58:30.0976482Z` 写入 `PASS`；
- 全 Console 的 4 条 Error 不属于本 Record 的编译/新行为失败：其中 2 条是 MCP bridge 临时
  stdio session 释放时的 `Cannot access a disposed object`，另 2 条是既有 self-check 的
  runtime-rest negative-path 断言日志；最终 self-check result 为 `PASS`。这些日志不构成
  C++ runtime 或 Play Mode 验收。

## 7. Git / 交接

- 修改前工作树基线：branch `NTSD_2_4_C++`，HEAD
  `2c53f1eb0086ef76c892fa335bfe1adfdd87facc`；现有用户/历史未提交修改不归属本 Record；
- 当前实际脚本 diff：`NTSDBattleTickSystem.cs`、`SimulationWorld.Passes.partial.cs`、
  `BattleRuntimeSelfCheck.cs`；其中前两个文件也含前序 `R2-SCHED-001` 的已登记 diff；
- 提交 hash：无；
- 交接优先阅读：`R2-PASS-02-tail-adapter-boundaries.md`、
  `R1-SOURCE-007-dependency-graph-and-repair-batches.md`、
  `HANDOFF-R2-SCHED-001-scheduler-pass-boundary.md`。
