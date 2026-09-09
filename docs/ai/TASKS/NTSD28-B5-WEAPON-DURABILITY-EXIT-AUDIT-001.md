# Task Contract — NTSD28-B5-WEAPON-DURABILITY-EXIT-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / WEAPON_DURABILITY_ATTACKING_INJURY_EXIT_READY`
> 依赖：`NTSD28-B5-WEAPON-DURABILITY-ATTACKING-INJURY-PRODUCTION-001 / VERIFIED`

## 结论

- type1/2/4/6 actual与HitPlan durability均由共享native attacking injury seam驱动。
- definition positive优先、mode positive fallback、raw fallback、低32位乘法/signed除法、raw0与bdefend100最终破坏语义均闭合。
- HP damage、display/status及kind4 count保持独立；HitPlan已有counter字段且shadow diff为0。

该weapon-durability attacking-injury规则族无剩余首差，允许退出。此结论不覆盖weapon-strength self-cost、完整hit-resource transaction、content或其他B5规则。

