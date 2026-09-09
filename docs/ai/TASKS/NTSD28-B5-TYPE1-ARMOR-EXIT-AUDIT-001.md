# Task Contract — NTSD28-B5-TYPE1-ARMOR-EXIT-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / BREAK_VERTICAL_ORDER_ROUTED`
> 依赖：`NTSD28-B5-TYPE1-ARMOR-ATOMIC-PRODUCTION-INTEGRATION-001 / VERIFIED`

## 目标与结论

复核Authority type1 selection/match/activation/reduced/fallback/break/C25i全链与Unity actual+HitPlan。
数据、选择、激活、数值写面、runtime profile/recovery已闭合；仍有一个type1-specific顺序残差：

- Authority broken fallback：unarmored horizontal→`armor.action`/`-1→0`→vertical→attacker post-hit。
- Unity actual：`ApplyStandardFall`可能在horizontal/break前提交vertical knockback。
- Unity HitPlan：break后先project attacker post-hit，再project vertical。

下一`NTSD28-B5-TYPE1-ARMOR-BREAK-VERTICAL-ORDER-001`统一actual+HitPlan。通用hit-resource transfer、
armor audio/spark、正式content分别保持B5 resource、B10/B9、H/B11，不阻止后续“type1-specific family”退出，
但仍阻止整个4.7/最终对齐完成。

## 允许路径

仅本Task/Change、Ledger、STATE、handoff、总表与armor manifest；不修改C#/content/Scene。
