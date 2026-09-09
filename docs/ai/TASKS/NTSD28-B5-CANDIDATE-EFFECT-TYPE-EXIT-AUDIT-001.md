# Task Contract — NTSD28-B5-CANDIDATE-EFFECT-TYPE-EXIT-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / CANDIDATE_EFFECT_TYPE_SPECIFIC_FAMILY_EXIT_READY`
> 依赖：`NTSD28-B5-CANDIDATE-EFFECT-TYPE-PRODUCTION-FILTER-001 / VERIFIED`

## 结论

- brute、loose、role-aware正式候选路径最终共享 `ItrAllowedCore` 或等价 cached wrapper，均调用同一pure resolver。
- direct/runtime查询同样进入该core；shared candidate runner在runtime ITR替换后、disposition/writer前二次调用resolver。
- effect 13/14/15/16矩阵与晚阶段effect-action override保持独立，没有把两种不同语义混合。
- 该规则族无剩余首差，允许退出；不代表整个候选生成、碰撞或B5已退出。

本审计只改治理文档，不改代码、content或Scene。
