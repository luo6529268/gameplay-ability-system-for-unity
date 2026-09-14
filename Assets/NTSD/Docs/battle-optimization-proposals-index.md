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

## 六、评审轮次记录

### R1 评审（2026-09-14，评审方：GPT6）

- **裁定**：三份文档均为"修改后采纳"，无退回；文档 3（边界计划）架构基础
  最好，文档 1（性能）需修改最多。
- **阻断级发现 4 项**（已全部核实属实并落入 R1 修订）：
  1. 内容权威已由 D-023（2026-09-12）切换为 NTSD 2.8-Logan 正式
     `resources/runtime`，两份新路线图引用的 Direction B 已过时；
  2. cadence 表述必须统一为精确 33 ms，禁用"30 Hz"（1000/33≈30.30，非
     严格 30；"每 4 渲染帧一 tick"仅为近似）；
  3. dedicated simulation worker 已在 `SimulationTickDriver` 默认启用
     （`useDedicatedSimulationWorker=true`，含资格拒绝与单飞背压），性能
     方案"tick 在主线程同步执行"的事实基线过时；
  4. 性能方案 Step B 原写"worker 自驱 33 ms"会篡改 Host cadence 所有权
     （F1/F2/F5/debt/输入冻结语义），已重写为"Host 拥策略、worker 只执行
     显式提交的 tick"。
- **高/中严重度发现**：draw 公式化（F4）、CentralOnly 跳过 legacy sort
  （F5）、物化频率措辞（F8）、验收 Player 化、ASTC 能力先行与 ETC2 候选、
  切场双预算（steady/transition peak）、逻辑所有权与物理 bank 分离、
  opoint 闭包生产者全集补全、构建确定性分级、`ISimulationTickHost.World`
  泄漏、Presentation ack 非阻塞语义、B6/B7 线程相关 API 前移、证据路径
  笔误等——全部落入 R1 修订或第 19.7 节修正清单。
- **第 19 节 S1–S6 裁定**：S2/S4/S5/S6 采纳（S6 强制）；S1/S3 修改后采纳；
  并新增 S7–S9（端口拆分不暴露 World、ack 非阻塞 + epoch 字段、worker
  准入级 API 清理）。正文修正清单 P-1…P-5 待用户批准后执行。

### R1 修订落地（2026-09-14）

| 文档 | 修订 |
|---|---|
| 性能计划 | R1：事实基线 F2/F2b/F2c/F4/F5/F7 重写，Step B 重写（Host 保留 cadence 所有权），验收 Player 化 + 附加延迟指标，新增 D5/D6，M0 新增 worker 现状取证第一优先组 |
| 资源计划 | R1：D-023 输入口径，P3 预印先于 P2 装载，装载语义"只装载不组装"，双预算（steady/transition peak），逻辑所有权与物理 bank 分离，闭包生产者全集 + 运行时断言，ASTC 能力先行 + ETC2 候选，构建确定性分级，新增 D6/D7 |
| 边界计划 | §19.7 追加 GPT6 裁定与正文修正清单 P-1…P-5（正文待批，未改动） |

### 待办（下一轮）

1. 将三份 R1 文档提交 GPT6 复核；
2. 复核通过后由用户批准：边界计划正文修正 P-1…P-5 落地、PERF D1（M0 启动）、
   ATLAS D1（内容审计启动）；
3. M0 报告产出后回填两份路线图的推演数值，冻结预算与验收阈值。

### R2 复核（2026-09-14，评审方：GPT6）与执行

- **裁定**：PERF `CONDITIONAL_REVIEW_PASS`（条件已在 R2 补齐）；ATLAS
  `REVIEW_PASS`（附 2 项轻微修正，已补齐）；MONO
  `CORRECTION_SET_APPROVED`。上轮 24 项覆盖：19 项入正文、4 项随
  P-1～P-5 回写、1 项（render/GPU 门）R2 补齐，遗漏 0 项。
- **批准与执行（仅文档正文，非代码大包）**：
  - MONO：P-1～P-5 + S1～S6 正文落地——7.1 端口拆分（不再暴露
    `SimulationWorld`）、7.2/7.4 epoch 链 + 比较规则、8.3 非阻塞 ack/slot
    生命周期、9.9/11 线程 API 重划（Debug 硬门）、13.1/13.6 守卫增强、
    3/14/15 33 ms 口径统一、19.6/19.7 排版修复；状态更新为
    `CORRECTION_SET_APPROVED / BODY_UPDATE_DONE`；
  - PERF R2：1.1 端到端 presented frame 硬门 + 归因指标、Step C Burst
    逐位合同、F9/Step B Debug 口径统一、M0 双标签
    （`ContentAuthorityState`/`UnityTestJobConflict`）、完成定义与 D1 更新；
  - ATLAS R2：D7 前移（D1→D3→D6→D7→D2→D5→D4）、P2 回退≠低端认证、
    D1 双标签。
- **D1 状态**：PERF D1 与 ATLAS D1 均 `APPROVED`；实际测量/Editor 扫描
  启动与活跃 `NTSD28-Q06` 恢复任务协调（R2 修正了复核引用的过时 job
  状态：`aa6b0c9f…` 已终态归档，见 CURRENT-AUTHORITY.md 第 3 行）。
- **代码实施**：仍未启动；边界计划保持 B0→B9 分批 + `USER_HOLD`，逐批
  单独批准（P-1 归 B1、P-2 归 B3/B4、P-3 归 B0–B6、P-4 归 B3、P-5 归 B0）。
