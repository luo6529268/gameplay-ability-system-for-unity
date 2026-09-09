# Task Contract — NTSD28-B5-REMAINING-EXIT-AUDIT-004

> 状态：`VERIFIED / GOVERNANCE_ONLY / KIND4_ENVIRONMENT_CONSUMPTION_ROUTED / B5_EXIT_NOT_READY`
> 依赖：`NTSD28-B5-MULTI-BODY-CANDIDATE-EXIT-AUDIT-001 / VERIFIED`

## 下一首差

Authority的kind4链统一读取攻击者 `environment_state_320`：正值时candidate producer对每个几何有效kind4
递增16位 `kind4_source_count_92`；consumer把kind4转换为kind0并按朝向/速度翻转dvx；普通伤害归属在计数非零时
从 `catch_source_slot_90` 重定向，并在实际伤害尾部递减一次。

Unity已经有独立 `Runtime.EnvironmentState320` carrier，但candidate没有 `kind4_source_count_92` carrier或producer；
`ResolveRuntimeItrForPair`、heavy-held release gate和HitPlan仍错误读取无关的legacy `WeaponCount`，普通伤害归属/递减也缺失。

## 下一步

先做 `NTSD28-B5-KIND4-ENVIRONMENT-OWNER-AUDIT-001`，冻结carrier/reset/snapshot/checksum、candidate producer、
runtime ITR actual/HitPlan、attribution/decrement与B6 cpoint producer边界，禁止把 `WeaponCount` 改名冒充该字段。

本审计不改代码、content或Scene。证据见 `docs/ai/MANIFESTS/NTSD28-B5-KIND4-ENVIRONMENT-CONSUMPTION.md`。
