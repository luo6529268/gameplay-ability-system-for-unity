# HANDOFF — R3-AI-LIFE-01 dead / respawn-window AI input eligibility

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`  
> Change Record：`R3-AI-LIFE-001 / RUNTIME_PENDING`

## 已完成的最小闭环

- C++ authority 从 `src/core/main.cpp:5505-5522`、`src/input/input_handler.cpp:1615-2353` 与
  `src/entity/game_tick.cpp:1249-1421` 闭合：每个 active current-character DAT 的 AI callback 不以
  self HP 作 caller gate；`prepare_ai_input` 的 no-target branch 仍会 roll/clear keys，并且发生在
  frame/death/respawn cleanup 之前；
- Unity 共有三个 self-HP global eligibility gate：legacy core、indexed decision kernel、indexed
  sensing `TryFindNearestCore`。三处均已最小移除，而 target HP、snapshot/index readiness、coordinate
  validity、death/respawn writer 和 action resolver 都未改；
- 第三处不是猜测：首次 `DataOrientedCanonical` fixture 实际触发
  `IndexedCanonical attempted fallback after unified snapshot commit`，说明 no-target branch 被 self HP
  sensing reject 截断；在修改该路径前已更新 Task Contract / Change Record / ledger；
- 修改后，`LegacyCanonical` 和 `DataOrientedCanonical` 的 no-target HP=0 fixture 都满足
  `PrevJump: 0→1`、`KeyJump: 1→0`，并保持 `CdAttack=5` / `Frame=0`；
- static three-gate guard、`Tools/Validate-ChangeLedger.ps1`、`git diff --check`、现有 Unity Editor
  的 UnityMCP scripts compile、filtered `error CS`=0 和完整 `BattleRuntimeSelfCheck` 均已通过。最新
  result 是 `Temp/NTSD_BattleRuntimeSelfCheck.result = PASS`（2026-08-22 01:02:51 +08:00）。

## 未关闭项 / 不可扩大结论

- 没有运行、构建、修改、复制或向 C++ authority 目录写入任何内容；R1-WP02 full trace 仍 `BLOCKED`；
- 没有检查 target-bearing dead AI、specific OID/state、held/caught relationship、death→respawn scene
  visibility、RNG call trace 或真实 Play Mode；
- 因而这里只能说明 C++ source 已定义的 **no-target self-HP input eligibility** 在 Unity legacy / data
  oriented profiles 均有 code-level evidence，不能写成完整 dead/respawn 或 AI 行为对齐；
- `AiSensingKernel` 的 target candidate rules、role index、Loose Quadtree/AI performance、packet /
  physical binding、CPoint/held/link/opoint、collision/hit/render 都没有修改。

## 推荐的连续下一步

按 `D-009` 进入 `R3-INP-03` 的**只读 source preflight**：只对照 C++ `prepare_input`/`apply_input`
中 player input edge、hold、journal/slot application 的顺序，与 Unity `FrameInputSet`、input buffer 和
`PostCooldownHumanInputAll` 的 adapter。先建立 Task Contract；只有证据表明一个最小差异可独立修改时，
才建立新的 Change Record 并写脚本。不要把 R3-AI-LIFE 的 self-check 作为 packet / physical input
correctness 证据。
