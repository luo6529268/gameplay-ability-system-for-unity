# Task Contract — NTSD28-B5-HIT-RESOURCE-PRODUCTION-READINESS-AUDIT-002

> 状态：`VERIFIED / GOVERNANCE_ONLY / THREE_BLOCKERS_REMAIN / HP_CONSUMPTION_READY`
> 依赖：`NTSD28-B5-TYPE1-ARMOR-EXIT-AUDIT-002 / VERIFIED`

## 复审结论

旧audit 001的`stats.attacking`与selected type1 armor阻塞已经解除；pure injury/transaction、两跳resource
attacker、world 1C/34/38、F6 gate、suppression carrier、actual/HitPlan armor写面均已存在。

完整hit-resource MP transaction仍不可原子接入：

1. positive gain需要Authority `base_max_mp`；Unity `MPMax`现有500与release trace 200是H/B11内容差异，
   尚无获批内容策略，不能猜测。
2. selected battle mode对1C/34/38的override仍无production mapping；归B8/H。
3. type0 parent生成special child时必须写suppression literal1，Unity只有carrier、缺B7/C25b producer。
4. cpoint held/throw caller仍归B6；weapon-strength self-cost仍归B5/H独立链。

可立即继续且不依赖上述三项的独立差异：Authority unarmored/type0 fallback在damage后把effective HP injury累加到
`input_hp_consumed_total(+0x34C)`；Unity standard writer与HitPlan尚未写。下一
`NTSD28-B5-UNARMORED-HP-CONSUMPTION-PRODUCTION-001`。

## 边界

本审计只改治理文档，不改C#/content/Scene；不以默认0、500或75/75伪装完整production readiness。
