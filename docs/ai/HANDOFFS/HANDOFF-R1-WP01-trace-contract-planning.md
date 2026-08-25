# Handoff — R1-WP01 Trace 合同规划

> 完成日期：2026-08-21
> Work Package：R1-WP01-trace-contract-planning
> 状态：规划完成并停止；没有开始 R1 C++ read-only trace acquisition、Unity trace、R2 或任何 gameplay 改动。
> 唯一行为 authority：J:\QQFile\NTSD2.4\ntsd_release 中实际构建到 ntsd_new.exe 的 C++ live battle runtime。

## 1. 已完成的交付物

1. 已完整读取本任务指定的六份状态材料：
   - AGENTS.md；
   - Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md；
   - docs/ai/STATE.md；
   - docs/ai/DECISIONS.md；
   - docs/ai/R0-HISTORICAL-EVIDENCE-REGISTER.md；
   - docs/ai/HANDOFFS/HANDOFF-R0-bootstrap-authority-migration.md。
2. 新增 docs/ai/TASKS/R1-WP01-trace-contract-planning.md。
3. 更新 docs/ai/STATE.md 与 docs/ai/DECISIONS.md 的 R1-WP01 状态和 D-005。
4. 本 handoff 记录了下一次实施需要保留的 authority、边界、未知项和推荐顺序。

## 2. R1-WP01 已确认事实

| 事项 | 等级 | 结论 |
|---|---|---|
| C++ live authority | VERIFIED | Makefile target 是 ntsd_new.exe；game_tick、frame advance、physics、collision、hit、weapon、cpoint、input、renderer 都在 release 构建模块中。 |
| C++ 主 tick | VERIFIED | src/entity/game_tick.cpp 的 game_tick(...) 是正式入口；R1 checkpoint 已以其静态边界命名。 |
| Unity 主 tick | VERIFIED | NTSDBattleTickSystem 的 RunTick / RunFrameAdvancePhase / RunInteractionPhase / RunPresentationAndCleanupPhase 为当前 Unity 调度入口。 |
| Unity CPoint / WeaponSync 的当前位置 | VERIFIED | PreInteractionTickAll 在 CandidateCollect 前执行 RunCpointCheckStep10、RunCpointMismatchTailStep10、RunWeaponSyncHeldStep10。 |
| C++ / Unity 三方可比较 trace | UNKNOWN | 当前未实施、未运行。 |
| PreInteraction 的提前时序是否造成行为差异 | INFERRED | 源码静态顺序有风险，但没有 C++ release runtime witness，不能称为 VERIFIED mismatch。 |

## 3. 已固定的 trace 合同

- 新 schema：ntsd-r1-cpp-unity-trace-v1，UTF-8 JSONL。
- producer.role：cpp-release、unity-fallback、unity-optimized。
- 三方 header 必须锁定：fixture、initial state、semantic DAT manifest、stage manifest、slot domain、seed、tick rate、input journal、field registry、pass map。
- runtime slot 是唯一跨端实体主键；Unity generation/stable id 只能 diagnostic，禁止参与 C++ 等价判断。
- 每个 tick 记录 C++ checkpoint、真实 source segment、mappingStatus、slot/world/rest/event 快照。
- 事件必须保留 eventOrdinal，不允许候选、consume、lifecycle 或 render handoff 预排序后再比较。
- 浮点没有全局 epsilon；没有完成 C++ binding 的浮点字段只 capture，不判等。
- first-difference 必须包含 tick、checkpoint/pass、slot、字段、C++ 值、Unity fallback 值、Unity optimized 值、上一个匹配 checkpoint、事件上下文和最短已知重现前缀。
- 旧 NTSDParity / Authority400 v3/v4 仍是历史诊断材料，不能升级为 C++ authority trace 或 certificate。

完整字段、checkpoint、fixture、比较与停止条件以 R1-WP01 Task Contract 为准。

## 4. 关键未知项

- C++ release 的 opt-in trace 开关、JSONL sink、RNG state/call-count 读取点；
- C++ 输入注入与实际 post_cooldown_input callback 的 journal 绑定；
- C++ / Unity 完整 DAT 语义 manifest；
- fixed slot initial-state 和 stage no-data 的可重放 bootstrap；
- Unity fallback / optimized 的独立开关、worker 路径与 presentation descriptor 观察点；
- C++ camera/perspective 是否以及如何进入 R1 required equality 域；
- f64 logical normalization、candidate/consume reason code 的 C++ source binding。

这些是 UNKNOWN，不得由 C# trace、self-check、checksum、0 GC 或 1000 AI 结果自动填充。

## 5. 没有执行的事项

- 没有修改 Unity gameplay、C++ Release runtime、DAT、场景、资源、测试实现、Makefile 或任何 C++ trace implementation；
- 没有运行 C++ build / ntsd_new.exe、Unity 编译、自检、Play Mode、性能或 Android；
- 没有启动 R2、改变 pass 顺序、处理 CPoint / WeaponSync / held/link / collision / input / opoint / render handoff；
- 没有作出任何“已与 C++ 对齐”的结论。

## 6. 推荐的下一实施 Work Package

推荐下一步为 **R1-WP02 — C++ Release read-only trace acquisition**，但需要用户单独确认后才可开始。

推荐理由：只有从实际 ntsd_new.exe release live path 的**未修改 runtime**以外部只读方式采集到的 trace，才能成为后续 Unity fallback / optimized 对照的行为基准。若现有通道不足，WP02 必须报告 blocker；不得增加 C++ instrumentation、trace sink、fixture/input bridge 或修改 release target。

后续依赖顺序：

~~~text
R1-WP02 C++ producer ─────┐
R1-WP03 Unity producers ──┼─> R1-WP06 replay harness ─> R1-WP07 acceptance evidence
R1-WP04 fixture+journal ──┤
R1-WP05 comparator ───────┘
~~~

## 7. 继续前的强制检查

开始 R1-WP02 前，下一执行者必须：

1. 重读 AGENTS.md、STATE.md、DECISIONS.md、R0 evidence register、本 handoff 和 R1-WP01 Task Contract；
2. 重新检查 C++ Makefile target 与 game_tick(...) 的实际调用路径；
3. 明确记录现有外部观察通道、其数据覆盖范围以及“不写入 C++ authority 目录”的证明；
4. 在不修改 C++ 的前提下定义 run identity、输入前置条件和非 authority 输出目录；
5. 若发现必须先改变长期 pass ordering、验收标准、C++ gameplay 或 Unity gameplay，立即停止并作为 Change Request 报告。

## 8. R1-WP01 自身验证

本 WP 只执行了文档/静态验证：

- 完整读取指定状态材料；
- 静态读取 C++ Makefile、game_tick(...)、Unity tick/pass 与旧 parity/trace 资料；
- 使用 diff 检查确认本次不应含 gameplay 文件。

未运行 runtime 测试；这是本工作包的预期边界，不是阻塞或完成 R1 的证据。

## 9. R1-WP02 C++ Release read-only amendment

2026-08-21，用户明确 C++ Release 工程只读。后续 R1-WP02 只能从未修改的 C++ Release runtime 以只读方式获取 trace，并在非 authority 目录保存采集结果和比较资料。此 amendment 覆盖本 handoff 中任何可能被理解为“向 C++ 工程增加 instrumentation、trace sink 或 fixture/input bridge”的旧表述。
