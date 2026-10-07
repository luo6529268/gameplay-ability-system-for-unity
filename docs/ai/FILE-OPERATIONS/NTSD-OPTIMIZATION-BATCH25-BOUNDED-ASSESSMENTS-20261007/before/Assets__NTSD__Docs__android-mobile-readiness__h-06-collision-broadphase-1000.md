# H-06 1000 实体碰撞 Broadphase 方案

> 优先级：高  
> 状态：`OPEN / OPTIMIZED_PATHS_EXIST / PARITY_AND_PROFILING_REQUIRED / WAITING_USER_APPROVAL`
> 最后更新：2026-10-06
> 本轮共同合同与启动门：[2026-10-06复核](../battle-optimization-rebaseline-and-start-gates-20261006.md)；本轮未运行本项测试/测量，实施待用户批准。
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

生产配置空值仍解析为 BruteForce。当前已有 Role-aware 分组、Direct/Sweep 与树候选路径，不能按“尚需新建空间索引”重写。499,500只是1000实体简单两两组合数；实际方向、角色、itr/bdy矩形、平台及fallback会改变比较次数，不是当前实测值，也不是所有实现的严格上限。候选准入须以当前336B44基线与新鲜成本证据裁定，不能直接切默认值。

## 解决方案

1. 建立同seed/input/roster/出生点/tick的BruteForce与现有Role-aware Direct/Sweep/tree shadow A/B；分散、密集、平台与fallback分别计量。
2. 比较候选集合与消费顺序、input、双 RNG、slot/generation、aRest/vRest、hit、events、OPoint、stats 和 overall checksum。
3. 先复用当前已存在的局部body/itr模板和缓存，清点命中率/失效/精确几何；只有实测有缺口才评估构建期预处理，不重复实现，不改变当前bdy.ZWidth或命中规则。
4. 统计 invalid AABB、fallback-all participant、整 tick fallback、pair count 和分布，防止“名义启用空间索引、实际仍接近平方扫描”。
5. 所有一致性门通过后，用独立 Change 切换生产默认，并保留诊断性 BruteForce 对照入口。

## 实施步骤

1. 冻结 BruteForce 基线和四类正式 workload。
2. 补充逐 tick candidate/hash 诊断，不改变消费语义。
3. 对现有Role-aware/Sweep/tree执行shadow compare，阈值262144/8192只记录为当前配置事实，是否调整由实测决定；修复仅索引自身漏项/重复/顺序差异。
4. 完成性能门后再切生产配置；不得同时修改命中规则。

## 验收条件

- Dispersed、Combat、Concentrated、OPoint burst 四场景强一致域全部相同。
- 当前worst-case的itr×bdy实际比较、精确检测、平台分支与fallback可分别解释；不强制密集且真实相交场景少于499,500，不以漏检测换少pair。
- fallback 分布处于明确预算，没有无诊断的整 tick 退回 BruteForce。
- Logic P95 满足 `<33 ms`，P99 不形成持续 backlog，warmup 后 `0 B/tick`。

## 测试条件

| 测试 | 条件 | 通过标准 |
|---|---|---|
| Candidate parity | 逐 tick 双后端 | 集合、顺序、重复数和 first difference 全可比较 |
| 状态 parity | 相同 seed/input 运行完整采样 | RNG、slot、hit、OPoint、checksum 一致 |
| 分散场景 | 1000 分散实体 | 不回退、性能不劣于基线 |
| 集中场景 | 1000聚集/多itr-bdy/平台/范围技能 | 无漏命中；实际比较/成本与退化原因可解释，不设虚假固定pair门 |
| 生命周期 | 分身/召唤/销毁/slot reuse | 无 ghost、旧索引项和 generation 错绑 |

## 证据与留痕

- 2026-10-06重扫：`Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs:28-29,2084-2091,2674,3267`；默认BruteForce，LooseQuadtree配置选Role-aware，现有Direct/Sweep/tree与fallback仍须当前认证。旧pair记录仅历史参考。
- 实施时保存：双后端 request/report、逐 tick first difference、fallback 统计、性能分位数和生产配置变更证据。
- 2026-09-06：方案文档建立；未批准切换生产 broadphase。
