# Handoff — R0 Bootstrap 与权威迁移

> 完成日期：2026-08-20
> Work Package：`R0-BOOTSTRAP-AUTHORITY-MIGRATION`
> 状态：已完成并停止；未进入 R1。

## 完成内容

- 已完整读取 `Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md`，并以它作为 R0–R8 长期总纲。
- 已在 `docs/ai/` 建立 `STATE.md`、`DECISIONS.md`、R0 Task Contract、历史证据分类台账与本 handoff。
- 已确认 C++ authority 目录、`game_tick(...)` 入口和 release Makefile 可读，且 Makefile target 为 `ntsd_new.exe`。
- 已盘点根规则、主要对齐/交接/架构/渲染/验证资料、NTSDParity 资料和 `BattleRuntimeSelfCheck` 入口。
- 已修正根 `AGENTS.md` 中仍将 C# 写为最终行为 authority 的有效规范句。
- 已给旧运行时验证、未来帧同步、知识审计和 1000 AI 计划补充或收束 C++ authority 说明；现有主对齐 ledger、handoff、中央渲染和统一架构计划本来已经含迁移声明，未重复改写其历史段落。

## 已确认事实

- 唯一行为 authority：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live runtime，主入口 `src/entity/game_tick.cpp::game_tick(...)`。
- C# 只允许作历史移植辅助、命名线索和 Unity 回归证据；与 C++ 冲突时 C++ 胜出。
- 计划的 R0 完成条件是治理迁移且不修改 Unity/C++ gameplay；本任务满足该范围。

## 未确认项

- 任何具体历史 C# 结论是否与 C++ release 行为一致。
- C++/Unity trace schema、同输入 journal、first-difference witness。
- Unity 编译、自检、Play Mode 和 C++ build/runtime trace 的当次结果。

## 本次变更范围

- 文档/治理文件：`AGENTS.md`、4 份 `Assets/NTSD/Docs/*.md` 与 `docs/ai/` 新文件。
- 未改动：`Assets/NTSD/Scripts/`、C++ runtime、DAT、场景、资源、测试实现、Unity 项目设置。
- 任务开始前已有的用户 `.meta` 修改与 `.claude/` 未跟踪目录未触碰。

## 推荐下一步：仅 R1-WP01 规划

新会话先读取：

1. `AGENTS.md`
2. `Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md`
3. `docs/ai/STATE.md`
4. `docs/ai/DECISIONS.md`
5. `docs/ai/R0-HISTORICAL-EVIDENCE-REGISTER.md`
6. 本 handoff

然后只产出 `R1-WP01` 的 Task Contract：trace schema、C++ release 观察点、Unity phase 映射、固定 seed/初始 slot/DAT/输入 journal 夹具、fallback/optimized 三向比较、first-difference 输出和停止条件。不要在该规划工作包实现 instrumentation、修改 gameplay 或启动 R2。
