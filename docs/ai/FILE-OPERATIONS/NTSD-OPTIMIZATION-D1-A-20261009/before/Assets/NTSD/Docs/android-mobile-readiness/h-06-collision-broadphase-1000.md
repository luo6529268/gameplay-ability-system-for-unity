<!-- OPTIMIZATION-REVIEW-20261009 -->
# H-06 当前评审卡（2026-10-09）

文档角色：本优化点的唯一方案；当前事实与排程以本卡和[进度](../battle-optimization-progress-tracker.md)文首为准。原正文全部保留为详细设计/验收/历史证据。评审建议不覆盖未获批准的强制门，旧“立即继续/Goal active”不授予恢复执行权限。

| 字段 | 本轮整理 |
|---|---|
| 类别 / 建议优先级 | CPU / 高 |
| 当前状态 | 候选值得继续／D1待决定；本轮仅文档，未实施/复测 |
| 依赖的最小范围 | H-09受影响正确性、H-11适用容量、M-09/M-15基线；不依赖Mono整体 |
| 下一步与具体方向 | 先现有Role生产准入设计，复用81/82四形状/Driver证据，只补实际覆盖缺口；空间分区为独立后续候选 |
| 完成边界 | D1批准前不降低原父项P95门、不推广；建议候选准入与H07阶段分别记录 |
| 方案成熟度 | 以下为文档评审；准确写域、当前代码与受影响验证须在Task中冻结，不能仅凭此卡开始修改 |
| 范围与回滚 | 保留本项原不变量；脚本前Task/Change，保存当前dirty字节。只回滚本项增量，任何覆盖/删除依原授权审计，不从HEAD恢复用户工作 |

[跨项依赖](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DEPENDENCIES.md) · [共同执行与证据复用规则](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/README.md) · [待用户决策](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DECISIONS.md)

子方案（不增加父项数量）：[H06-SPATIAL 保守空间候选裁剪](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/H06-SPATIAL.md)。

**D1待确认：** 原父项仍含四场景强一致与整tick P95<33ms门。本轮仅建议将Role候选准入设为独立子阶段；未获批准，不切默认、不降低或删除原验收。

## 原方案与历史证据（原始正文保留）

> 2026-10-07 首阶段限定准入评估完成 / NO_DEFAULT_PROMOTION：第25批四1000-participant collector形状候选序列/handle/hit/RNG同，实际18/18；Direct/Sweep/tree在分散有局部收益，但集中261150pair仍1.1–1.5秒。实际OPoint/完整tick/checksum/native336和可靠GC未闭合，不默认推广，不重写索引。详[第25批报告](../../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH25-BOUNDED-ASSESSMENTS-20261007/REPORT.md)。原下方方案未来正式准入不变。

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
