# Task Contract — NTSD28-B5-KIND8-EXIT-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / KIND8_SPECIFIC_FAMILY_EXIT_READY`
> 依赖：`NTSD28-B5-KIND8-ATOMIC-PRODUCTION-INTEGRATION-001 / VERIFIED`

## 结论

- brute/loose/role-aware candidate最终都进入`ItrAllowed/ItrAllowedCached -> ItrAllowedCore`，kind8统一调用pure eligibility。
- runtime consume防御路径同样进入该core；shared runner的所有四壳production dispatch统一调用唯一control writer。
- HitPlan使用同一eligibility并投影完整字段；existing shadow comparison为0。
- 旧character/shared/special/weapon direct实现仍有历史行为，但production已绕过，只保留compatibility，不构成第二owner。

kind8-specific candidate/consumer/HitPlan无剩余首差，允许退出该规则族。此结论不覆盖其他kind/effect、完整resource、
CPoint、audio/spark/content或全B5退出。

