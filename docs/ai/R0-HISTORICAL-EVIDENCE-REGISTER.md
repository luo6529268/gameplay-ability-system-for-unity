# R0 历史证据分类台账

> 记录日期：2026-08-20
> 裁决规则：只有 `J:\QQFile\NTSD2.4\ntsd_release` 中参与 `ntsd_new.exe` release 构建的 live path 能定义战斗规则。下列 A/B/C 分类说明历史材料的后续用途，不重新判定其当时记录是否真实。

## A. 可复用历史回归

这些资产可用于构建夹具、定位 Unity 回归、比较性能或检查测试覆盖；其通过结果仍不等价于 C++ release 对齐。

| 资产 | 复用用途 | 当前边界 |
|---|---|---|
| `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`、`BattleRuntimeSelfCheckCore.cs`、Editor 入口 | Unity 侧 focused regression、夹具和运行入口 | 现有断言中大量 `authority` 指向 C# 历史语义；R1 后需按 C++ trace 逐项补证或重命名，不在 R0 修改测试逻辑。 |
| `Assets/NTSD/Docs/BATTLE_RUNTIME_VERIFICATION.md` 的已执行自检/Play Mode 记录 | 可复现案例与历史验证索引 | 记录不能单独签发 C++ 对齐；文首已加迁移声明。 |
| 中央渲染、zero-GC、1000 AI、worker 和对象池的测量报告 | 性能/分配回归基线 | 仅证明其报告范围内的 Unity 结果；要保留为默认 fast path 还需 C++ trace 等价。 |
| `Tools/NTSDParity` 的 schema、compare/self-test 工具与 Authority400 夹具 | R1 trace schema/比较器设计的历史输入 | 当前是 C# / Unity diagnostic 工具；不能视作 C++ authority trace 或 production certificate。 |

## B. 必须重新验证

这些文档或结论主要以 C# 基线、Unity self-check、checksum、diagnostic trace 或 fast-path A/B 为依据。它们可保留历史价值，但在 R1 C++ trace 合同完成前都不能关闭 C++ 行为差异。

| 资料组 | 重新验证原因 | 后续阶段 |
|---|---|---|
| `Assets/NTSD/Docs/csharp-vs-unity-battle-alignment.md` | 大量差异关闭、pass 顺序、`Authority400` witness 和 self-check 明确采用 C# authority。文首已有 C++ migration declaration。 | R1 建 trace，R2–R6 按模块重审。 |
| `Assets/NTSD/Docs/HANDOFF-codex-battle-alignment.md` | 历史接手结论和性能/自检 PASS 未提供 C++ release 对照。文首已有 C++ migration declaration。 | R1 起逐项取用，不能直接继承“已关闭”。 |
| `Assets/NTSD/Docs/unified-battle-*.md`、`unified-battle-lockstep-ecs-server-architecture-plan.md` | U0–U9 的 architecture、fast-path、worker、checksum 和性能结论以 C# / Unity 等价为主；统一架构总纲已有迁移声明。 | R1 trace 后，R7 逐优化重认证。 |
| `Assets/NTSD/Docs/central-battle-render-system-plan.md` | 中央渲染顺序/表现性能资料不能替代 C++ renderer handoff。文首已有 render authority migration declaration。 | R6、R7。 |
| `Assets/NTSD/Docs/battle-runtime-zero-gc-architecture-plan.md`、`battle-zero-gc-and-structure-plan.md`、`singleplayer-1000ai-performance-plan.md` | 0 GC、吞吐、A/B hash 和容量资料不验证 C++ observable behavior。 | R7；性能仍作为附加门。 |
| `Assets/NTSD/Docs/wpoint-alignment-report.md`、`GPT_HANDOFF_LF2_Hit_Injury.md` | 使用反汇编、旧 C# 或 Unity 历史实现线索，未按当前 C++ release live path 完整闭合。 | R4、R5。 |
| `Assets/NTSD/Docs/future-server-lockstep-architecture.md`、`lockstep-knowledge-base-audit.md` | 未来架构文本含旧 C# pass 描述；已改为 C++ authority 口径，但不进入当前实现范围。 | R1 之后按需重审；当前不实施。 |

## C. 已明确与当前 authority 规则冲突

此类冲突指**权威口径冲突**，不代表已经证明某一项 gameplay 行为与 C++ 不同。R0 已在不改 runtime 的前提下修正了仍处于有效规范位置的冲突文本。

| 原冲突 | 处理 | 证据 |
|---|---|---|
| 根 `AGENTS.md` 同时声明 C++ 为唯一权威，却将 Unity 适配、pass、输入、验证和新增字段写为“权威 C#”。 | 已改为 C++ release live path，并明确 C# 仅作历史辅助。 | R0 Git diff；`AGENTS.md` 第 5–11 节。 |
| `BATTLE_RUNTIME_VERIFICATION.md` 将 C++ release 描述为“中间还原工程”，可能使其失去最终裁决地位。 | 已改为 C++ release live path 为最终裁决，并给全部历史验证增加降级声明。 | R0 Git diff；该文档文首。 |
| 未来服务器/知识审计文档将 C# pass 作为当前规则来源。 | 已增加迁移说明并将有效规范句改为 C++ release live path。 | R0 Git diff；两份文档文首。 |

## R0 后的读取规则

1. 需要定义行为时：先读 C++ release live 调用链与 release build 参与性。
2. 需要找到 Unity 对应处或已有夹具时：可读 C#、历史文档和 self-check，但标为历史辅助。
3. 需要保留优化时：先做 C++ trace → Unity fallback → Unity optimized 三向比较。
4. 无法从 C++ live path 得出的结论必须标为 `UNKNOWN`，不能由旧 C# 结论自动补齐。
