# Task Contract — NTSD28-B5-STANDARD-REDUCED-KNOCKOUT-PRODUCER-001

> 状态：`VERIFIED / RED_1_OF_4 / FOCUSED_4_OF_4 / RELATED_270_OF_270 / TARGETED_PLAY_4_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / EXACT_KO_PRODUCER_READY / TYPE3_WEAPON_FLUTE_STATS_NEXT`
> 依赖：`NTSD28-B5-LEGACY-DAMAGE-STAT-WRITER-RETIREMENT-READINESS-AUDIT-001 / VERIFIED`、
> `NTSD28-B5-ORDINARY-CREDIT-GATE-2F4-PRODUCER-CONSUMER-CORRECTION-001 / VERIFIED`。

## 目标

为standard unarmored与reduced/armor两条character damage事务补齐Authority exact
`KnockoutCount358` producer。lethal判定必须在HP扣减前，以与score相同的
`ResolveNativeStandardHitCredit()` attribution和exact +0x2F4 gate写入；HitPlan shadow必须观察同一credit实体
的KO计数。旧`KillStat/KillStats/ComboCount*`在本包保持，待后继retirement。

## Authority

- `battle_world.cpp:6680..6690`与`7114..7124`：target必须type0、+0x2F4==-1、attribution有效、
  HP before>0且effective damage>=HP before，随后在HP mutation前调用`record_native_knockout`。
- `record_native_knockout:1403..1417`只对resolved credit entity执行`knockout_count_358++`并产生world event。
- Unity本包只闭合已有canonical counter；world KO event/feed没有当前等价owner，继续后置，禁止用legacy
  `KillStats`冒充。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`
- 新focused test、`BattleHitExecutionPlanEditorTests.cs`与`BattleRuntimeSelfCheck.cs`的精确断言。
- 本Task/Change与治理恢复文档。

## 不变量

- 不改damage值、HP/HPBound/PP、score、kind4 pending count、owner chain深度、fall/rest/motion/RNG/pass order。
- gate0、nonlethal、already-dead、missing attribution与non-character target均不写KO。
- redirected source与最多两层owner链必须和existing score helper解析到同一credit。
- actual与HitPlan仅新增exact KO observable；旧stats writer及carrier/schema不动。

## Test-first验收

1. direct/two-owner/redirected source三类lethal credit分别只增最终credit KO一次。
2. gate0、nonlethal、already-dead与missing redirected source均保持sentinel。
3. standard与reduced actual均覆盖；HitPlan lethal standard/reduced shadow difference mask为0并断言exact KO。
4. focused、完整HitPlan/B5相关、build、full SelfCheck、真实Play及Scene不变。

## 回滚

移除actual KO helper/calls、HitPlan snapshot/projection/diff字段与本包测试，不回退+0x2F4 correction。
