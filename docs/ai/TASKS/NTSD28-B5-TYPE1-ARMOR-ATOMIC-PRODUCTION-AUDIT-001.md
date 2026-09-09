# Task Contract — NTSD28-B5-TYPE1-ARMOR-ATOMIC-PRODUCTION-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / PREREQUISITES_ROUTED`
> 依赖：`NTSD28-B5-TYPE1-ARMOR-RUNTIME-PROFILE-INTEGRATION-001 / VERIFIED`

## 审计结论

- Authority顺序为ordinary defense优先；未防御时要求恰好一个type1 armor，随后match→activation；
  bypass/candidate rejection/资源不足回unarmored，armor HP耗尽先写-1再走带broken armor的unarmored tail；
  apply时走selected reduced damage/rest/tail。
- match与activation的effective injury来自`native_attacking_injury28`，需要`stats.attacking`优先、active-mode
  `+0x1C`fallback和32位乘法/截断。Unity已有active-mode与pure helper，但缺definition typed carrier。
- actual两入口和HitPlan均由`LF2AlternateDamageResolver.ShouldUseAlternateHurt`布尔分流，不能表达selected armor、
  bypass、activation失败或broken armor。
- actual alternate writer目前固定null armor；未写PP/`InputMpConsumedTotal350`、`InputHpConsumedTotal34C`、
  `RuntimeArmorHp118`与armor ratio reaction threshold。
- HitPlan snapshot/compare缺runtime armor HP和HP/MP consumption surfaces；必须先补carrier再接shadow projection。
- broken armor的`-1→armor.action→0`必须在unarmored horizontal后、vertical/effect override前保持等价；
  不得塞进profile初始化或recovery包。
- armor sound/spark保留给B10/B9；正式armor content继续受H/B11门约束。

## 后续顺序

1. `NTSD28-B5-DEFINITION-ATTACKING-CARRIER-001`。
2. `NTSD28-B5-TYPE1-ARMOR-HITPLAN-RUNTIME-CARRIERS-001`。
3. `NTSD28-B5-TYPE1-ARMOR-ATOMIC-PRODUCTION-INTEGRATION-001`。

本审计不修改C#、content或Scene。
