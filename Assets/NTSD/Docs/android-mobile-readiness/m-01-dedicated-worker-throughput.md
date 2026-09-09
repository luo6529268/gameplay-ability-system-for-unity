# M-01 Dedicated Worker 吞吐优化方案

> 优先级：中  
> 状态：`OPEN / SOLUTION_DOCUMENTED / PROFILING_REQUIRED`  
> 最后更新：2026-09-06  
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

当前 dedicated worker 是单一模拟线程、输入容量 1，并在 publication 后等待表现 acknowledgment。它能隔离主线程但不会自动并行 1000 AI。线程数量不是规则；任何拆分都必须保持 pass、双 RNG、slot 和消费顺序。

## 解决方案

1. 先分别测量 AI sensing、decision、candidate、hit、lifecycle、publication 与 ack 等待时间。
2. 优先减少算法复杂度和无效工作；H-06 Broadphase 通常比盲目增加线程更优先。
3. 只有 M-11 L1 边界完成后，才把无 Unity Object、无共享写入、可确定性合并的纯 kernel 评估为 Burst/Jobs 或分区并行候选。
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

- 当前证据：`SimulationTickDriver.cs:1180-1189`、`BattleSimulationWorkerBoundary.cs:443-653`。
- 保存每次 profile、候选 kernel 的确定性合同、shadow first difference 和性能结果。
- 2026-09-06：方案建立；未选择并行算法。
