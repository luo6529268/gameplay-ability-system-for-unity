# Task Contract — NTSD28-B5-TYPE1-ARMOR-RUNTIME-INITIALIZATION-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / IMPLEMENTATION_SPLIT_DEFINED`
> 依赖：`NTSD28-B5-TYPE1-ARMOR-ACTIVATION-PURE-CORE-001 / VERIFIED`

## 目标与结论

核对Authority出生初始化、C25i恢复、破甲写回与Unity载体/生产入口，确定后续最小接线边界。

- Authority `battle_world.cpp:1280-1284`在出生时只读第一个armor block：`runtime_armor_hp=hp`，
  `armor_recovery_timer=recover>0?recover:-1`。
- Authority `3806-3839`的C25i同样只读第一个block；Unity字段、snapshot/checksum、C25i placement/kernel已存在，
  但`TryGetNativeArmorRecoveryProfileForWorldPass`固定false，出生Reset仍为`0/-1`，因此production profile不可达。
- snapshot restore已经保存两个runtime字段；snapshot shell不得用definition初始化覆盖恢复值。
- 破甲`-1`、unarmored horizontal后的armor action及自增到0属于命中事务，不进入profile初始化包。
- 正式18个armor的recover均为0；6个hp字段均为1。代码接线可先完成，正式content仍受H/B11门约束。

## 后续拆包

1. `NTSD28-B5-TYPE1-ARMOR-RUNTIME-PROFILE-INTEGRATION-001`：首block profile读取与出生初始化，保护snapshot restore。
2. type1 selection/activation/damage/rest/break actual+HitPlan原子生产接线。
3. 正式armor content部署继续等待H/B11内容策略。

本审计不修改C#、content或Scene。
