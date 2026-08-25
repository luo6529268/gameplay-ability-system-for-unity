# HANDOFF — R3-HOLD-INP-01 negative-link character input eligibility

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`  
> Change Record：`R3-HOLD-INP-001 / RUNTIME_PENDING`

## 已完成的最小闭环

- C++ authority 已从 `main.cpp:5505-5522` 与 `input_handler.cpp:2742-3096` 闭合：active current
  character DAT 的 callback caller 不按 negative link skip，`apply_input` 也没有函数级 negative-link
  return；relation restriction 是 combo/state-local 的；
- Unity 的 `LF2Entity`、`LF2Character` 及 exact-AI `BattleEcsCharacterInputPass` 曾在 entry 把
  `Runtime.LinkState < 0` 整段跳过；该三处现已只保留 runtime/null / current-DAT-type eligibility；
- 新增 `CheckNegativeLinkCharacterInputEligibility`：valid parent/child negative relation 下，human
  real-character world path 与 shared character-DAT compatibility path 都消费 direct edge，进入 frame 10
  并执行 resulting-frame `dvx=7` tail；
- static guard check、`Tools/Validate-ChangeLedger.ps1`、`git diff --check`、UnityMCP scripts compile、
  filtered `error CS`=0 与完整 BattleRuntimeSelfCheck 都已实际通过；result file 为
  `Temp/NTSD_BattleRuntimeSelfCheck.result = PASS`（2026-08-22 00:43:55）。

## 未关闭项 / 不能扩大结论

- 没有运行 C++ executable，`R1-WP02` full trace 仍 `BLOCKED`；
- 没有 caught/held input、release、frame/lifecycle 的 Play Mode 或 R5 relation joint fixture；
- 没有独立的 data-oriented AI runtime fixture；AI entry removal 是同一 source contract 下的 static
  coverage，不能写成 AI behavior complete；
- `IsBlockedByReleaseLinkOrCaughtCpoint()`、two-pass held、CPoint、positive/negative link cleanup、opoint、
  collision、dead/respawn、input packet / physical binding 全部没有修改。

## 推荐的连续下一步

按 `D-010` 进入 `R3-AI-LIFE-01` 的 **只读** preflight：只核对 C++ active-character caller 对 AI self
HP / respawn 的调用资格与 Unity legacy/data-oriented prefilter、death/respawn producer 的字段边界。先建立
Task Contract；在 Contract 和 Change Record 未建立前不要修改脚本。若必须触及 death/respawn、CPoint、held
或 packet writer，停止并拆新包。
