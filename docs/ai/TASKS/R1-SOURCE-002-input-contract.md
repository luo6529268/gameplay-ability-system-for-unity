# R1-SOURCE-002 — 输入、组合键、AI 与逻辑帧边界源码合同

> 建立日期：2026-08-21  
> 状态：COMPLETED（仅静态 source contract；不代表输入运行时已对齐）  
> 类型：只读 source 审计；不修改任何 gameplay。  
> 依赖：R1-SOURCE-001 已完成主 tick pass map。

## Goal

闭合 C++ post_cooldown_input callback、human input、AI input、组合键边沿、F1/F2 gate 与 Unity HumanInput/CharacterInput/NeedClearInput/SimulationTickDriver 的实际映射，裁决 D-SCHED-005 与 D-SCHED-010 的状态，并输出后续 R3 的输入子流程验收合同。

## Scope

- C++ release：input_handler.cpp、game_tick.cpp、main/bootstrap 中实际提供 post_cooldown_input 的调用链及相关 frame/input 数据；
- Unity：SimulationTickDriver、NTSDBattleTickSystem、SimulationWorld input pass、Input buffer/combination 代码与 AI input adapter；
- 每个 input path 的 pressed/held/released、清理时点、slot 顺序、输入缓冲、AI/human 优先级和慢速/暂停 gate。

## Out of Scope

- 修改 input、组合键、AI、tick driver、R2/R3 pass 或任何技能；
- 运行 C++ executable、Unity Play Mode、trace、compiler/self-check；
- 以旧 C# input 结论替代 C++ source。

## Deliverables

1. docs/ai/RESEARCH/R1-SOURCE-002-input-contract.md
2. docs/ai/RESEARCH/R1-SOURCE-002-unity-input-crosswalk-and-diff.md
3. 对 D-SCHED-005、D-SCHED-010 的更新，以及新的 input difference entries
4. R3 输入子流程的验收与 Change ID 建议
5. State/handoff 更新

## Stop Conditions

- C++ callback provider 无法从 release live source 闭合；
- 需要运行 executable、修改 instrumentation 或 gameplay 才能继续；
- 发现必须调整长期输入/帧同步架构；
- 用户提出新的 Change Request。

## 完成摘要

- 已从 C++ `main.cpp` 中实际传入 `game_tick(...)` 的 `post_cooldown_input`
  callback 闭合到 `InputHandler::poll`、`prepare_ai_input` 与 `apply_input`；
  没有使用旧 C# 作为规则来源。
- 已建立 Unity `FrameInputSet → SimInputBuffer → HumanInput → CharacterInput`
  的 source crosswalk，并核验 runtime-slot 升序 traversal。
- 已裁决 `D-SCHED-005`、`D-SCHED-010` 为静态顺序/语义差异，新增
  `D-INP-001`～`D-INP-006` 的输入差异或待验证项。
- 本 Work Package 没有运行 C++ executable、没有运行 Unity、没有修改
  Unity/C++ gameplay、DAT、场景或资源。
