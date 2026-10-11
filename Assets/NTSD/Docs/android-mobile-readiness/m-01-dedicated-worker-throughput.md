## 2026-10-10 当前阶段更正：M-01

| 当前阶段 | 当前次序 | 原风险等级 |
|---|---|---|
| P2／当前后续评审 | 6 | 中 |

第6项：核worker实际eligible/active/fallback/等待；不把默认开关当实际并行。

本表覆盖下方旧评审卡的建议优先级/排程，技术证据与原验收范围保留。当前排序不证明收益、不自动授权跨域实施。[完整34项当前排程](../android-mobile-readiness-priority-risk-register.md) · [唯一进度](../battle-optimization-progress-tracker.md)。

---

<!-- OPTIMIZATION-REVIEW-20261009 -->
# M-01 当前评审卡（2026-10-09）

文档角色：本优化点的唯一方案；当前事实与排程以本卡和[进度](../battle-optimization-progress-tracker.md)文首为准。原正文全部保留为详细设计/验收/历史证据。评审建议不覆盖未获批准的强制门，旧“立即继续/Goal active”不授予恢复执行权限。

| 字段 | 本轮整理 |
|---|---|
| 类别 / 建议优先级 | CPU / 中 |
| 当前状态 | 候选待归因／专项边界保持；本轮仅文档，未实施/复测 |
| 依赖的最小范围 | 生产资格深化依赖M-11 B0-B6/L1与PERF门；局部只读归因不依赖其全完成 |
| 下一步与具体方向 | 先核eligible/active/fallback/等待；不将worker默认true当实际并行 |
| 完成边界 | 所选资格/等待改进有证且Host cadence/有界ack/关闭保持；整tick33ms父门不改 |
| 方案成熟度 | 以下为文档评审；准确写域、当前代码与受影响验证须在Task中冻结，不能仅凭此卡开始修改 |
| 范围与回滚 | 保留本项原不变量；脚本前Task/Change，保存当前dirty字节。只回滚本项增量，任何覆盖/删除依原授权审计，不从HEAD恢复用户工作 |

[跨项依赖](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DEPENDENCIES.md) · [共同执行与证据复用规则](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/README.md) · [待用户决策](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DECISIONS.md)

## 原方案与历史证据（原始正文保留）

# M-01 Dedicated Worker 吞吐优化方案

> 优先级：中  
> 状态：`OPEN / PROFILING_REQUIRED / WAITING_USER_APPROVAL`
> 最后更新：2026-10-06
> 本轮共同合同与启动门：[2026-10-06复核](../battle-optimization-rebaseline-and-start-gates-20261006.md)；本轮未运行本项测试/测量，实施待用户批准。
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

dedicated worker当前仍是条件性单线程/单飞，输入容量1、publication/ack背压；默认启用开关不能证明生产场景持续active。Unity presentation bindings等资格拒绝可能让tick同步执行。当前AI已有DataOrientedCanonical、SoA/增量查询和fallback，先复用这些路径，不假定1000个旧Mono AI。任何优化保持pass/RNG/slot/消费顺序。

## 解决方案

1. 第一组先测eligible/active/实际线程、拒绝原因、同步fallback比例、in-flight与ack/publication latency；再拆AI sensing/查询/decision、candidate/hit/lifecycle与捕获物化。
2. 按实测热点排名复用现有AI/碰撞优化路径、减无效扫描与缓存失效，不预设H-06必然最大瓶颈或盲目增加线程。
3. worker生产准入深化遵循PERF StepB及M-11前置；Burst/Jobs只对实测热点kernel独立评估，固定版本/FloatMode/精度，分别x86_64/ARM64逐位shadow compare。单线程Burst不自动确定性，差异即保持managed/canonical。
4. 并行结果按稳定 slot/order 归并；每个 RNG 调用的 stream、次数和顺序不得变化。
5. 保留串行 canonical 路径做 shadow compare 和回滚。

## 验收条件

- 找到有数据支持的主热点，优化前后报告使用同一 workload/fingerprint。
- worker 不触碰 Unity Object；并行或优化路径与 canonical checksum/事件顺序一致。
- Logic P95 `<33 ms`，P99 不形成 backlog，warmup 后 `0 B/tick`。
- acknowledgment 等待和主线程消费不会形成死锁或无界 publication backlog。

## 测试条件

| 测试 | 条件 | 通过标准 |
|---|---|---|
| 分段 profiling | Dispersed/Combat/Concentrated1000 | 各阶段 P50/P95/P99/Max 可定位 |
| Shadow compare | 串行与候选优化同 tick | input、RNG、world、events、checksum 一致 |
| Thread guard | Dedicated worker 正式路径 | 无 Unity API/Object 访问 |
| 背压 | 主线程故意变慢、关闭中止 | 有界等待、可停止、无死锁 |
| 长测 | Combat1000 1800 tick 和热测 | 无 backlog、GC 或吞吐退化 |

## 证据与留痕

- 2026-10-06重扫：`Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs:207,1516-1537`；默认开关和资格拒绝存在。当前AI默认见`Simulation/Ai/Runtime/BattleAiExecutionProfile.cs:24`，实际活跃/收益仍待测。
- 保存每次 profile、候选 kernel 的确定性合同、shadow first difference 和性能结果。
- 2026-09-06：方案建立；未选择并行算法。
