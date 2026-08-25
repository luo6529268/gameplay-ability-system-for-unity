# R3-AI-LIFE-001 — dead / respawn-window AI input eligibility

<!-- CHANGE-RECORD
id: R3-AI-LIFE-001
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorld.AiInput.partial.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/AiDecisionKernel.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/AiSensingKernel.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\core\main.cpp + src\input\input_handler.cpp + src\entity\game_tick.cpp release live path
evidence: SOURCE-CONTRACT-VERIFIED / STATIC-PASS / UNITY-COMPILE-PASS / FOCUSED-SELF-CHECK-PASS / RUNTIME-PENDING
-->

> 创建日期：2026-08-22  
> 状态：`RUNTIME_PENDING`  
> 所属 Work Package：`R3-AI-LIFE-01`

## 1. Scope

只处理 `D-INP-002`：移除 legacy 和 indexed/data-oriented AI input pipeline 对 AI **自身** `HP <= 0`
的 global skip，使 C++ source 已定义的 pre-death/respawn `prepare_ai_input → apply_input` 字段链能够运行。
首次 dual-profile self-check 已发现 indexed kernel 的前置 `AiSensingKernel.TryFindNearestCore` 仍有同一
self-HP reject；该第三 gate 会令 unified authority 在 no-target contract 前误判 fallback。它属于同一
已闭合 source contract，故在写入前扩展本 Record 的已允许代码路径。

允许代码文件仅为：

- `Assets/NTSD/Scripts/Simulation/SimulationWorld.AiInput.partial.cs`；
- `Assets/NTSD/Scripts/Simulation/Ai/AiDecisionKernel.cs`；
- `Assets/NTSD/Scripts/Simulation/Ai/AiSensingKernel.cs`；
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`。

不改 AI target/candidate/RNG policy、input resolver、death/respawn writer、frame advance、held/CPoint/link、
collision/opoint/render/DAT/scene/C++ 或保护边界。

## 2. Authority / source contract

- C++ `main.cpp:5505-5522`: caller visits every active current `obj_type==0` character DAT regardless of
  self HP, then executes `prepare_ai_input` (AI) and `apply_input`;
- C++ `input_handler.cpp:1615-2353`: no self-HP return in `prepare_ai_input`; dead **targets** are filtered,
  but a no-target self still rolls/clears keys, runs no-target fallback and writes edge state;
- C++ `game_tick.cpp:1249-1421`: input callback precedes frame advance / state9998 / state14 respawn cleanup;
- Unity self-HP eligibility gates: `SimulationWorld.AiInput.partial.cs` legacy core,
  `Simulation/Ai/AiDecisionKernel.cs` indexed kernel, and
  `Simulation/Ai/AiSensingKernel.cs:74-88` `TryFindNearestCore` self eligibility filter. The first
  self-check run verified that the third gate otherwise turns the C++ no-target contract into a unified-authority
  forbidden fallback.

`R1-WP02` remains BLOCKED; C++ source is authority, not executable trace.

## 3. Initial implementation / acceptance contract

Both `LegacyCanonical` and `DataOrientedCanonical` must take their existing no-target decision path for an
active HP=0 AI with no eligible target. The self slot must remain a valid sensing subject even with HP=0;
candidate-target HP filters remain intact. Focused test asserts only input lifecycle (`Prev*` rolls, `Key*` clears,
no stale direct action); it does not assert a full target-bearing AI combat behavior or respawn result.

## 4. Protected boundaries / stop conditions

- Keep CentralOnly / Texture2DArray / dynamic Mesh / URP, 1.5× scale, fixed-world camera, capacity, 30 Hz,
  FrameInputSet, SoA/ECS, pool, worker, zero-GC, T8 deferment;
- stop if making HP=0 AI enter the existing pipeline needs a change to RNG, target selection, action resolver,
  death/respawn, frame advance, CPoint/held/link/collision or C++;
- correction requires a new Change Record; do not use destructive Git operations.

## 5. Planned verification

1. static three-gate check and respawn / target-HP gate preservation;
2. legacy + data-oriented no-target HP=0 focused self-check;
3. `Tools/Validate-ChangeLedger.ps1`, `git diff --check`, UnityMCP compile / `error CS` / full self-check;
4. report no Play Mode / C++ trace / target-bearing dead-AI evidence as `RUNTIME_PENDING`.

## 6. 实际改动

| 文件 | 类型 / 方法 | 实际改动 | 预期副作用 |
|---|---|---|---|
| `SimulationWorld.AiInput.partial.cs` | `PrepareAiInputBasicLegacyCore` | 保留 null guard，移除 `self.Runtime.HP <= 0` 的 global return。 | active current character DAT 的 HP=0 AI 继续进入既有 legacy decision path；death/respawn pass 未改。 |
| `Simulation/Ai/AiDecisionKernel.cs` | `TryEvaluateCore` | 移除 self-HP `InvalidSelf` early exit。 | indexed decision 与 C++ source caller 一样进入 existing coordinate / no-target / target chain。 |
| `Simulation/Ai/AiSensingKernel.cs` | `TryFindNearestCore` | 移除 self-HP sensing-subject reject，保留 included、role-index readiness 与 coordinate validity gate。 | no-target 的 `SelectedSlot=-1` 能抵达既有 roll/clear branch；candidate target HP filter 未动。 |
| `BattleRuntimeSelfCheck.cs` | `CheckDeadAiInputEligibility` | 增加 legacy + `DataOrientedCanonical` 无目标、HP=0 AI fixture。 | 防止任一 profile 将 self HP=0 错当整个 input callback 的 no-op。 |

## 7. 验收与证据

| 层级 | 命令 / 场景 / 输入 | 实际结果 | 状态 |
|---|---|---|---|
| static | three self-HP gate guard + retained coordinate/respawn boundary + ledger | PASS；第三 gate 是首次 fixture 实际揭示后、先扩 Record 再修正。 | `PASS` |
| 编译 | 已打开 Unity Editor；UnityMCP `refresh_unity(force/scripts/compile)`，2026-08-22 01:02 | 成功；filtered `error CS` 返回 0。 | `PASS` |
| focused test / self-check | `NTSD/验证/运行战斗运行时自检` | dual-profile no-target HP=0 fixture 与完整 `BattleRuntimeSelfCheck` 均通过；结果文件 01:02:51 +08:00 为 `PASS`。 | `PASS` |
| Play Mode / 集成 | dead → respawn、target-bearing AI、场景输入 | 未运行；需 R3/R5 lifecycle joint fixture 与后续场景验收。 | `RUNTIME_PENDING` |
| C++ authority 对照 | release source caller / `prepare_ai_input` / `game_tick` | source contract 已闭合；禁止运行/修改 C++ executable。 | `SOURCE-VERIFIED` |
| 可选 full trace | R1-WP02 | 未获得；当前保持 blocked。 | `BLOCKED` |

## 8. 风险、回滚与未关闭项

- 已知风险：C++ source 明确 no-target roll/clear，但 target-bearing dead AI、state-specific OID、held/caught
  与 real respawn presentation 仍未取得 joint evidence；本 Record 不得用这个 fixture 推断它们；
- 未关闭项：Play Mode、R3/R5 lifecycle joint fixture、C++ runtime trace；`R1-WP02` full trace 仍 BLOCKED；
- 回滚方式：若独立 joint fixture 证明这三个 prefilters 不应是 global eligibility 规则，使用本 Record
  的三处最小 diff 定向回退；不得回退 unrelated existing dirty-worktree changes；

## 9. Git / 交接

- 修改前工作树基线：仓库已存在用户/历史的 scene、project settings、资源 meta、文档、`NTSDBattleTickSystem`、
  `SimulationWorld.Passes` 和 `BattleRuntimeSelfCheck` 等 dirty changes；本 Record 不拥有、未回退它们；
- 实际 diff 范围：仅上表三处 self-HP eligibility gate 与 `CheckDeadAiInputEligibility` fixture；
  `BattleRuntimeSelfCheck.cs` 的其他 R2/R3 diff 属于既有 Change Records，未重新归属；
- 提交 hash（若已提交）：无；
- `Tools/Validate-ChangeLedger.ps1` 结果：PASS，2026-08-22（7 records / 10 governed code files）；
- 交接需优先阅读的文件：本 Record、`TASKS/R3-AI-LIFE-01-dead-respawn-ai-input-eligibility.md`、
  `HANDOFFS/HANDOFF-R3-AI-LIFE-01-dead-respawn-ai-input-eligibility.md`。
