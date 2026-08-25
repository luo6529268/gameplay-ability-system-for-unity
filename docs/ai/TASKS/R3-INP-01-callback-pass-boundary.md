# R3-INP-01 — callback / OID maintenance pass boundary

> 建立日期：2026-08-21  
> 状态：`RUNTIME_PENDING`（`R3-INP-001` 已通过 Unity compile 与 focused self-check；仍缺 joint fixture / Play Mode / C++ trace）  
> 顶层目标：`cpp-release-vs-unity-battle-realignment-plan.md` 的 R3。  
> 唯一行为 authority：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live runtime。  
> 用户已批准总计划的连续执行；本文件在创建 `R3-INP-001` Change Record 后授权其最小脚本改动。

## Goal

只处理 `D-SCHED-005`：恢复 C++ `post_cooldown_input` callback 的完整边界，使 Unity 的
human poll、AI prepare 和所有 active character 的 input application 在 normal path 中全部完成后，
才运行 OID 7/8/51 runtime maintenance。

这不是 F1/F2 gate、battle-entry clear、held/caught input、dead/respawn AI、physical binding 或
FrameInputSet API 的修复；它们必须留给后续独立 Work Package。

## Authority / Evidence

### C++ release live source — VERIFIED(source)

- `src/entity/game_tick.cpp:945-947`：live `game_tick(...)` 接收 `post_cooldown_input` callback；
- `game_tick.cpp:990-1005`：cooldown 后处理 F1/F2 step gate；如果 gate 允许，完整调用
  `post_cooldown_input()`；
- `src/core/main.cpp:4607-4608`：该 callback 先 poll P1/P2；
- `main.cpp:5505-5522`：在同一个 callback 内，按 slot 升序遍历全部 active character DAT；对
  AI 先 `prepare_ai_input`，再 `apply_input`；
- `game_tick.cpp:1006-1008`：callback 返回后才开始 slot `0..19` 的 OID 7/8/51 maintenance。

因此 authority 规则是：**完整 callback 完成 → OID maintenance**，而不是“human poll 完成 →
OID maintenance → character input”。C++ runtime trace 尚未取得，故这是 source contract，不能
写成 runtime trace 已验证。

### Unity current source — VERIFIED(source)

- `NTSDBattleTickSystem.cs:254-260`：cooldown 后执行 `PostCooldownHumanInput`，再进入
  `RunFrameAdvancePhase`；
- `NTSDBattleTickSystem.cs:282-296`：当前先 `Oid5152RuntimeMaintenance`，再检查
  `NeedClearInput`，最后才 `CharacterInput`；
- `SimulationWorld.Passes.partial.cs:232-335`：`CharacterInputAll` 对 active character DAT
  构建 AI snapshot / prepare AI / apply character input；
- `SimulationWorld.Passes.partial.cs:345-378`：OID maintenance 会修改 identity、frame、slot/
  dormant、HP 与 runtime snapshot，不能被视为无副作用的普通清理。

## Proposed implementation contract — READY AFTER CHANGE RECORD

开始脚本修改前，必须先建立 `R3-INP-001` Change Record；不需要再次请求计划内的单包确认。

### Normal path target order

```text
Cooldown
→ PostCooldownHumanInput
→ CharacterInput (all active character DAT, including AI prepare)
→ Oid5152RuntimeMaintenance
→ EarlyFrameAdvance / FrameLogic / FrameAdvance ...
```

### Battle-entry clear preservation

`NeedClearInput` 不是 C++ F1/F2 gate，属于 `D-SCHED-010 / R3-INP-02`。本子包不得改变其
语义。为避免把 callback-order patch 扩大为 entry-clear rewrite，实施时必须明确保持当前 Unity
entry-clear branch 的 maintenance-before-clear 行为，或先以 focused fixture 证明另一种安排不改变
既有 entry-clear contract；若无法证明，停止并将该分支移交 `R3-INP-02`。

### Allowed Unity files after approval

- `Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs`；
- 必要时同一 scheduler 的无分配 private pass wrapper；
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`，只允许增加该顺序的 focused fixture。

默认不得改 `SimulationWorld.Passes.partial.cs` 中 OID 7/8/51 的公式、`FrameInputSet`、Input Action
asset、`SimulationTickDriver`、AI decision kernel、CentralOnly、renderer、slot profile、pool、worker、
scene、DAT 或资源。

## Planned acceptance / verification

| 层级 | 要求 | 当前状态 |
|---|---|---|
| S0 source | C++ callback 前后与 Unity pass mapping 的源码坐标闭合。 | `PASS (source)` |
| S1 static order | normal path 的 CharacterInput 位于 OID maintenance 前；entry-clear contract 单列。 | `PASS (local static check, 2026-08-22)` |
| S2 focused fixture | 同 tick input writer 可改变 OID maintenance 所读的 frame；slot 顺序和 OID7/8→51 副作用可断言。 | `PASS (2026-08-22 request self-check)` |
| S3 compile / self-check | 现有 Unity Editor compile + targeted `BattleRuntimeSelfCheck`。 | `PASS (UnityMCP refresh/compile; `error CS`=0; result PASS)` |
| S4 joint fixture | OID7/8/51 + human/AI input journal，记录 frame、key/prev/cd、`Unk338`、identity、spawn/relation。 | `PENDING` |
| S5 Play Mode | 不作为该顺序 patch 的先决条件；若要验证物理键位，交给用户 / R3-INP-03。 | `PENDING / out of scope` |
| S6 C++ trace | `R1-WP02` 仍 BLOCKED。 | `BLOCKED` |

## Unknowns / required preflight after approval

1. 当前 `NeedClearInput` 分支是否依赖“OID maintenance 在 clear 之前”的 Unity-only bootstrap
   行为；不得猜测。
2. OID 7/8/51 maintenance 在同 tick 读取 input 写入的全部字段集合（至少 identity、frame、HP、
   `Unk338`、slot/dormant、relation）需要由 focused fixture 固化。
3. `CharacterInputAll` 的 optimized/SoA path 与 fallback 对同 fixture 是否拥有等价的 writer
   顺序；不能因性能路径而跳过 fallback evidence。
4. C++ F1/F2 gate、negative link、dead AI、edge packet 和 physical binding 均不在本包；这些仍
   分别属于 R3-INP-02～05。

## Stop conditions

立即停止，而不是扩张实现，若：

1. 移动 pass 需要改 OID maintenance 公式、identity、slot/dormant、pool 或 lifecycle；
2. 需要重写 `NeedClearInput`、F1/F2、FrameInputSet、Input Action asset 或 physical key binding；
3. 必须修改 AI targeting / decision kernel、held/caught gate、death/respawn 或 frame advance；
4. focused fixture 表明同 tick OID 结果依赖尚未审计的 CPoint/held/link/collision writer；
5. C++ source contract 与上述坐标冲突，或用户改变范围。

## Out of scope

- D-SCHED-010、D-INP-001～006、D-MOV-001～005；
- C++ executable、C++ source/build/config/resource，及其 authority 目录中的任何写入；
- C++/Unity full trace、comparator、server、network、performance test、Play Mode 物理输入验收；
- CentralOnly / Texture2DArray / dynamic Mesh / URP、1.5× visual scale、fixed-world camera、
  extended capacity、30Hz、FrameInputSet、SoA/ECS、pool/worker/0-GC 的已批准边界；
- T8 默认 `stage.dat` 部署。

## Required follow-up before any code

本包已按 `D-009` 连续执行到 compile / focused self-check；后续集成验收必须按顺序：

1. 已新建 `docs/ai/CHANGE-RECORDS/R3-INP-001.md` 并登记覆盖的脚本路径；
2. 已仅实现 normal-path pass relocation 和 focused fixture；
3. 已通过现有 Unity Editor 运行 scripts compile、filtered C# error query、self-check；
4. 继续前必须以独立 Record / fixture 关闭 R3 joint input、Play Mode 和可用 C++ trace 证据；
5. 更新 Ledger、STATE、总差异登记和 handoff，状态最多到与证据相称的层级。
