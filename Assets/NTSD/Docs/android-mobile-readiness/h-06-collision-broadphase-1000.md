# H-06 1000 实体碰撞 Broadphase 方案

> 优先级：高  
> 状态：`OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED`  
> 最后更新：2026-09-06  
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

生产配置空值解析为 BruteForce，1000 实体完全配对上限为 499,500。LooseQuadtree 已存在，但缺少当前权威、当前代码下候选顺序和最终结果的强一致证据，不能直接切换默认值。

## 解决方案

1. 建立同 seed、输入、roster、出生点和 tick 的 BruteForce/LooseQuadtree shadow A/B。
2. 比较候选集合与消费顺序、input、双 RNG、slot/generation、aRest/vRest、hit、events、OPoint、stats 和 overall checksum。
3. 预烘焙每个 OID/frame 的局部 bdy/itr AABB、最大范围和有权威依据的类型过滤表；运行时只变换并插入动态索引。
4. 统计 invalid AABB、fallback-all participant、整 tick fallback、pair count 和分布，防止“名义启用空间索引、实际仍接近平方扫描”。
5. 所有一致性门通过后，用独立 Change 切换生产默认，并保留诊断性 BruteForce 对照入口。

## 实施步骤

1. 冻结 BruteForce 基线和四类正式 workload。
2. 补充逐 tick candidate/hash 诊断，不改变消费语义。
3. 执行 LooseQuadtree shadow compare，修复只属于索引的漏项、重复和顺序差异。
4. 完成性能门后再切生产配置；不得同时修改命中规则。

## 验收条件

- Dispersed、Combat、Concentrated、OPoint burst 四场景强一致域全部相同。
- Concentrated1000 不再长期产生 499,500 对生产负载。
- fallback 分布处于明确预算，没有无诊断的整 tick 退回 BruteForce。
- Logic P95 满足 `<33 ms`，P99 不形成持续 backlog，warmup 后 `0 B/tick`。

## 测试条件

| 测试 | 条件 | 通过标准 |
|---|---|---|
| Candidate parity | 逐 tick 双后端 | 集合、顺序、重复数和 first difference 全可比较 |
| 状态 parity | 相同 seed/input 运行完整采样 | RNG、slot、hit、OPoint、checksum 一致 |
| 分散场景 | 1000 分散实体 | 不回退、性能不劣于基线 |
| 集中场景 | 1000 聚集/范围技能 | pair 显著受控且无漏命中 |
| 生命周期 | 分身/召唤/销毁/slot reuse | 无 ghost、旧索引项和 generation 错绑 |

## 证据与留痕

- 当前证据：`BattleCollisionBroadphaseName` 为空并解析为 `BruteForce`；历史集中场景记录过 499,500 pair peak。
- 实施时保存：双后端 request/report、逐 tick first difference、fallback 统计、性能分位数和生产配置变更证据。
- 2026-09-06：方案文档建立；未批准切换生产 broadphase。
