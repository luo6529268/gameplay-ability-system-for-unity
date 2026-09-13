# 战斗优化提案索引（供综合评审使用）

> 创建日期：2026-09-13
>
> 性质：导航索引。本文件不含独立方案内容，仅汇总当前会话产出的提案文档、
> 各自范围、依赖关系与评审注意事项。所有提案均为
> `DOCUMENTED / IMPLEMENTATION_NOT_STARTED / USER_HOLD`，未获实施授权。

## 一、提案文档清单

| 文档 | 计划标识 | 治理对象 | 一句话方案 |
|---|---|---|---|
| `battle-performance-stats120-roadmap-plan.md` | `BATTLE-PERF-STATS120-ROADMAP-001` | 每帧时间（Stats 渲染帧率） | 1000 实体 Stats 120：测量 → 表现侧安全区 → 边界重构前置 → tick worker 化 → 热点优化 |
| `battle-atlas-memory-lowend-roadmap-plan.md` | `BATTLE-ATLAS-MEMORY-LOWEND-ROADMAP-001` | 纹理内存与设备适配 | 全量一本图集改为"公共册+按角色分册"，构建期预印 + ASTC 变体 + 设备档位预算门禁 |
| `simulation-mono-nonmono-boundary-refactor-plan.md` | `SIMULATION-MONO-BOUNDARY-REFACTOR-001` | Mono/非 Mono 架构边界 | 既有计划（非本次产出）；本次仅追加第 19 节外部评审附录 |

## 二、三份文档的关系

```text
性能计划 (PERF) ──依赖──> 边界重构计划 (MONO) B0-B6 + L1
     ▲                            ▲
     │ 无依赖，正交并行            │ 其第 19 节评审附录
资源计划 (ATLAS) ────────────────┘
（资源计划与性能计划互为独立工程：一个治"每帧时间"，一个治"纹理内存"）
```

- PERF 的 Step B（tick worker 化）以 MONO 计划完成为前置；
- ATLAS 无前置工程依赖，M0 测量可独立先行；
- 三份文档共享同一套不变量：逻辑 33 ms 契约、NTSD 2.8-Logan 行为权威、
  checksum 逐位一致、十一阶段关闭、Direction B 内容权威。

## 三、本次会话新增产出记录（2026-09-13）

1. PERF 计划文档（新建）；
2. ATLAS 计划文档（新建）；
3. MONO 计划第 19 节外部评审附录（追加）：总评"方案合理，可作实施合同
   冻结"，含诊断真实性抽查（三条反向依赖代码核实）、认可要点 6 条、
   建议 S1–S6（其中 S6 指出该计划第 3 节"30 Hz"表述与头部 33 ms 权威更新
   的内部不一致）。

## 四、评审注意事项（提交综合评审时建议关注）

1. 三份文档均对陈述标注证据等级（`已验证-代码/文档`、`推演-待测`、
   `提案-待批`），评审时请按等级区分事实与假设；
2. PERF 计划的瓶颈模型（tick ≈33 ms）是待 M0 证实的推演，文档内建了
   "模型被推翻则重排路线"的修正出口，这不属于方案缺陷；
3. ATLAS 计划的 opoint 可达闭包穷尽性是正确性硬门禁（漏扫 = fail-closed
   拒画），评审请重点检查该机制设计；
4. 两计划各自列有开放决策清单（PERF D1–D4、ATLAS D1–D5），需用户拍板后
   才能进入实施；
5. 所有明确的排除项（模拟 LOD/降频、运行时动态图集、按时间回收、运行时
   压缩等）均来自会话内论证，评审若建议重启其中任何一项，请同时给出
   对应权威证据或合同变更理由。

## 五、当前全局状态

```text
BATTLE-PERF-STATS120-ROADMAP-001      DOCUMENTED / IMPLEMENTATION_NOT_STARTED / USER_HOLD
BATTLE-ATLAS-MEMORY-LOWEND-ROADMAP-001 DOCUMENTED / IMPLEMENTATION_NOT_STARTED / USER_HOLD
SIMULATION-MONO-BOUNDARY-REFACTOR-001  DOCUMENTED / IMPLEMENTATION_NOT_STARTED / USER_HOLD
```

综合评审结论不影响上述状态；任何提案进入实施均须用户单独批准，并按
`docs/ai/CHANGE-LEDGER.md` 规则独立立项。
