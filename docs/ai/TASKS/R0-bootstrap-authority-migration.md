# Task Contract — R0-BOOTSTRAP-AUTHORITY-MIGRATION

## Goal

建立可跨会话恢复的长期项目状态层，并将文档中 C++ release live path 作为唯一行为 authority 的口径收束；把历史 C# authority、trace、fast-path proof 和对齐结论分级，而不改变 battle runtime。

## Scope

- 阅读并以 `Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md` 为 R0–R8 总纲。
- 检查 `AGENTS.md`、主要架构/对齐/渲染/验证文档、NTSDParity trace 资料与 `BattleRuntimeSelfCheck` 入口。
- 新建 `docs/ai/STATE.md`、`DECISIONS.md`、本 Task Contract、证据台账与 handoff。
- 更正仍把 C# 写为最终 authority 的项目规则或主要计划文本；保留旧历史事实，并加迁移说明。

## Authority / Evidence

- C++：`J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp::game_tick(...)` 及其 release `Makefile` 所列 live 模块。
- 项目总纲：`Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md`。
- 项目规则：根 `AGENTS.md`。
- C# 与 Unity 资料：仅作历史移植、回归、性能或诊断证据。

## Files Likely Involved

- `AGENTS.md`
- `Assets/NTSD/Docs/BATTLE_RUNTIME_VERIFICATION.md`
- `Assets/NTSD/Docs/future-server-lockstep-architecture.md`
- `Assets/NTSD/Docs/lockstep-knowledge-base-audit.md`
- `Assets/NTSD/Docs/singleplayer-1000ai-performance-plan.md`
- `docs/ai/`

## Unknowns

- 历史 C# 结论与 C++ release 的逐项行为差异。
- C++/Unity trace schema、稳定输入 journal、初始状态夹具及 first-difference witness。
- 本机实际 Unity Editor 可执行路径和可运行状态。

## Deliverables

- 长期状态、决策、R0 task contract、历史证据分类台账和 handoff。
- 已修正的治理性 authority 文案，且没有 gameplay 改动。

## Verification

- 再次搜索根规则和主要文档，确认不存在未被迁移声明覆盖的 C# 最终 authority 规范。
- 审查 Git diff，确认只含文档/工作流文件，且不触及 `Assets/NTSD/Scripts/`、DAT、场景或资源。
- 不运行 Unity、C++ 构建、自检、Play Mode 或 trace；它们不验证 R0 的完成条件。

## Stop Conditions

- 发现 authority 不能从 C++ release live path 或 Makefile 参与性确认时停止。
- 需要实现 C++/Unity trace、调整 pass 或修复 gameplay 时停止，改由后续 R1/R2 task contract 处理。
- 用户提出改变 authority、架构、pass 顺序或验证标准的要求时，先作为 Change Request 评估。

## Out of Scope

- R1 trace implementation。
- R2–R8 的 gameplay、性能或渲染实现。
- Unity/C++ runtime、DAT、场景、资源、测试逻辑修改。
- 服务器 S0–S9、T8 默认 `stage.dat` 部署和 Android 验收。
