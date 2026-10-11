## 2026-10-10 当前阶段更正：H-06

| 当前阶段 | 当前次序 | 原风险等级 |
|---|---|---|
| 父项目标保留／子项暂不续办 | 不单独排队 | 高 |

Role限定默认和FindEffect既有成果保留；父项开放，Role精确配对诊断不续办。

本表覆盖下方旧评审卡的建议优先级/排程，技术证据与原验收范围保留。当前排序不证明收益、不自动授权跨域实施。[完整34项当前排程](../android-mobile-readiness-priority-risk-register.md) · [唯一进度](../battle-optimization-progress-tracker.md)。

---

> **2026-10-10 排程更正：Role限定默认准入已完成，H-06父项最终门仍开放。** 已完成成果保留，不重复推广或重跑未受影响证据；H06-ROLE-EXACT记录链/前置修复不再续办。当前首个讨论项为H-07的既有[H07-INPUT](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/H07-INPUT.md)，未批准新网格、空间索引或AI行为改变。下方旧“下一Role/尚未默认”只保留原时点。

> 2026-10-09 Role限定准入完成：空配置正常生产默认采用既有Role，显式Brute/Role和非法安全回退保留；当前双布局四窗完整Driver稳态0GC、容量/关闭和可辨收益通过，实际World/query及关闭重建13/13 PASS。H06父项/H07/Android仍开放，95OFF，旧Goal不恢复。

> **已完成单点的独立说明：** [优化点 A：FindEffect 从原代码到修复后代码、效果与边界](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/IMPLEMENTATION-RECORDS/H06-FIND-EFFECT-ALLOCATION.md)。单点完成不等于本父项或Role默认推广完成。

> 2026-10-09 FindEffect单点修复 VERIFIED（仅本Task范围）：100对象语义/候选顺序/载荷/双RNG/checksum一致，collector三次22→0事件；原千人混合collector八次234→0事件，容量与清理门PASS。H06限定准入仍未完成/未切默认，当前待同配置收益和接线门；D1=A不变，H06父项/H07/Android开放、95OFF、旧Goal不恢复。Change NTSD-H06-FIND-EFFECT-ZEROGC-001。

<!-- OPTIMIZATION-REVIEW-20261009 -->
# H-06 当前评审卡（2026-10-09）

文档角色：本优化点的唯一方案；当前事实与排程以本卡和[进度](../battle-optimization-progress-tracker.md)文首为准。原正文全部保留为详细设计/验收/历史证据。评审建议不覆盖未获批准的强制门，旧“立即继续/Goal active”不授予恢复执行权限。

| 字段 | 本轮整理 |
|---|---|
| 类别 / 建议优先级 | CPU / 高 |
| 当前状态 | D1=A／Role限定准入完成，空配置生产默认Role；父项/H07最终门开放 |
| 依赖的最小范围 | H-09受影响正确性、H-11适用容量、M-09/M-15基线；不依赖Mono整体 |
| 下一步与具体方向 | 保留Role默认与FindEffect限定成果；H06-ROLE-EXACT不续办。当前转H07-INPUT方案讨论，空间分区保留既有条件、不自动实施 |
| 完成边界 | 限定准入必要门通过后可先推广；H06父项/H07整体P95等门未过仍开放 |
| 方案成熟度 | 独立窗口/生产Change已VERIFIED，实际Role/Brute/Role关闭重建通过；仅限定准入，非最终性能认证 |
| 范围与回滚 | 保留本项原不变量；脚本前Task/Change，保存当前dirty字节。只回滚本项增量，任何覆盖/删除依原授权审计，不从HEAD恢复用户工作 |

[跨项依赖](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DEPENDENCIES.md) · [共同执行与证据复用规则](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/README.md) · [待用户决策](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DECISIONS.md)

子方案（不增加父项数量）：[H06-ROLE-EXACT 现有Role精确配对成本](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/H06-ROLE-EXACT.md) · [H06-SPATIAL 保守空间候选裁剪](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/H06-SPATIAL.md)。

**D1=A已批准：** 用户确认可先用安全有效的Role方案。限定准入通过四场景强一致、受影响完整0GC/容量/关闭及可追溯收益门后，允许默认推广，不必先达到整tick P95<33ms。原33ms、无持续backlog、正常drop0等仍是H06父项/H07最终性能门，未通过不得关闭。下方旧“完成性能门后才切配置”对这个限定子阶段被本决定取代；其他正确性和最终验收不变。2026-10-09用户已授权具名实施；[Task](../../../../docs/ai/TASKS/NTSD-H06-ROLE-SCOPED-ADMISSION-20261009.md)、[当前证据](../../../../artifacts/diagnostics/NTSD-H06-ROLE-SCOPED-ADMISSION-20261009/REPORT.md)。尚未通过全部门，未切默认。

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
