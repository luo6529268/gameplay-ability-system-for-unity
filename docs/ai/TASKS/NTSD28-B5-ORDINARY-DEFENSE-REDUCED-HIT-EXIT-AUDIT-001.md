# Task Contract — NTSD28-B5-ORDINARY-DEFENSE-REDUCED-HIT-EXIT-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / NULL_ARMOR_FAMILY_EXIT_READY`
> 依赖：`NTSD28-B5-ORDINARY-DEFENSE-REDUCED-HIT-PRODUCTION-INTEGRATION-001 / VERIFIED`

## 目标与结果

复核 ordinary-defense / selected-armor-null reduced-hit 的所有 actual 与 HitPlan caller、pure owner、旧启发式残留、
测试证据与未关闭边界。结论：该子家族允许退出；下一进入 type1 armor typed data contract，不部署正式 content。

## 审计结论

- actual 两个角色命中入口统一调用 `LF2AlternateDamageResolver.ShouldUseAlternateHurt`，其内部唯一调用
  `BattleOrdinaryDefenseResolver`；HitPlan 的 dispatch/can-project/project 也调用同一 adapter。
- actual `ApplyAlternateDamage` 与 HitPlan alternate projection 各只调用一次 reduced damage/rest pure core；
  `FallDamageDiv` 与 attacker weak 已从 null-armor reduced damage 中移除。
- production selector 不再包含 OID37/6/52、Prev2 或 bdefend<=60 启发式；残留 Prev2 defending 仅属于
  reduced reaction 的 defend-break tail，不参与 selection。
- direct `ApplyAlternateDamage` caller 仅为 self-check/editor tests；正式 actual caller 为上述两入口。
- type1 armor model/parser/activation/runtime HP/recovery/content 尚未接，不属于本次退出结论。

本包无脚本、content 或 Scene 修改；回滚仅删除本 Task/Change/Ledger/STATE/handoff/总表记录。

