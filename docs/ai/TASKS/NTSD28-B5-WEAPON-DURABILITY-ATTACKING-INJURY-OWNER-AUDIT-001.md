# Task Contract — NTSD28-B5-WEAPON-DURABILITY-ATTACKING-INJURY-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / ATOMIC_INTEGRATION_READY`
> 依赖：`NTSD28-B5-REMAINING-EXIT-AUDIT-005 / VERIFIED`

## Owner结论

- pure arithmetic唯一复用 `BattleOrdinaryCharacterDamageRouteResolver.ResolveNativeAttackingInjury`；不得复制另一套乘法/overflow实现。
- actual唯一修改面为 `BattleDamageWriter.ApplyWeaponDamage` 的type1/2/4/6 durability block；先算effective injury，raw `bdefend == 100`仍最终强制 `-1`。
- HitPlan唯一修改面为 `ProjectStandardObjectDamageWriterEffect` 对 `TargetWeaponFlightCounter` 的同值投影；writer snapshot/DifferenceMask已有该字段，不需schema扩张。
- attacker definition来源为 `LF2HitResolveRuntimeData.ResolveCharacterData(attacker).definition_attacking`；mode来源为world `NativeHitResourceRules.ActiveModeAttackingPercent1C`。
- HP/display/status仍按各自既有raw/effective规则；type0/3/5不写weapon durability，不扩大完整resource transaction。

## 下一步

以同一原子包新增type1/2/4/6、definition-vs-mode优先级、raw fallback、bdefend100及HitPlan shadow测试，再接actual/HitPlan。

