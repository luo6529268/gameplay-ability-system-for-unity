# Task Contract — NTSD28-B5-KIND4-ENVIRONMENT-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / CARRIER_AND_ATOMIC_SPLIT_DEFINED`
> 依赖：`NTSD28-B5-REMAINING-EXIT-AUDIT-004 / VERIFIED`

## 所有权结论

1. `EnvironmentState320` carrier已存在，继续作为kind4正值gate；不得读取或双写 `WeaponCount`。
2. 缺失的 `Kind4SourceCount92` 是跨candidate/consumer的16位持久计数，不是每tick input transient：只在完整初始化清零，
   candidate按 `(value + 1) & 0xFFFF` 递增，普通伤害成功尾部在低16位非零时递减。
3. carrier包必须覆盖 `NTSDEntityRuntime` full reset/copy、world runtime snapshot roundtrip、lockstep checksum、parity snapshot
   及独立focused tests；不接行为。
4. 后续atomic包由 `BruteForceSceneQuery.TryRecordReleaseCandidate` 前的kind4 producer、
   `ResolveRuntimeItrForPair` actual、`BattleEcsHitExecutionPlan` projection、`BattleDamageWriter` ordinary damage attribution/decrement
   共同闭合；不能只把两个 `WeaponCount` 条件替换掉。
5. `CatchSourceSlot90` 已有独立carrier；kind4计数只决定是否按其低16位尝试重定向，不修改低word本身。
6. `EnvironmentState320` 的cpoint throw producer归B6，B5可用程序化正值完成consumer验证，但不得扩大成B6完成。

下一 `NTSD28-B5-KIND4-SOURCE-COUNT-CARRIER-001`，再做atomic production integration。
