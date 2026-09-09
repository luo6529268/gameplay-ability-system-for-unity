# Task Contract — NTSD28-B6-WPOINT-DVX-WEAPON-HP-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / SUBSEQUENT_WEAPON_HP_PRESERVATION_ROUTED / PRODUCTION_HELD`
> 依赖：`NTSD28-B6-WPOINT-KIND3-RELEASE-OWNER-CORRECTION-001 / VERIFIED`

## 目标

从kind-3之后的held WPoint DVX release继续first-difference scan，冻结正式Authority不重置
`weapon_hp_31c`与Unity `OnThrown()`重载重置`WeaponFlightCounter`之间的生产差异。

## 证据闭环

- Authority `BattleWorld28::settle_held_refill_objects()`：
  - type1/4/6 DVX分支只写+0x2F8 source、action40、Vx/Vy、exclusive-depth Vz和双方active relation；
  - type2 DVX分支只多消耗一次`[0,6)`action draw，再写Vx/Vy、exclusive-depth Vz和relation；
  - 两条分支均不读写`weapon_hp_31c`。
- Authority中`weapon_hp_31c`由definition初始化，并由physics、hit、special interaction、
  exhaustion等具名路径写回；held DVX release不在其writer集合中。
- Unity `LF2WeaponHeldStateResolver.ThrowHeldWeapon()`在两类DVX release末尾调用
  `weapon.OnThrownInternal()`；唯一override `LF2Weapon.OnThrown()`无条件把
  `FlightCounter`写回`charData.weapon_hp`。
- `WeaponFlightCounter`已由B0正式绑定为`weapon_hp_31c`；生产reference pool对type1/2/4/6
  均创建`LF2Weapon`，因此这不是generic-only或测试替身路径。
- 当前Direction-B Unity Config有312条WPoint，其中3条non-kind3 `dvx!=0`，均在
  `naruto_clone.dat`且action为10/23/35；release decoded corpus有604条`dvx!=0`，扣除5条
  kind3 overlap后有599条non-kind3。当前weapon DAT的`weapon_hp`为非零，因此受损后捡起再投掷
  可观察到重置差异。

## 后续 production 边界

后续`NTSD28-B6-WPOINT-DVX-WEAPON-HP-PRESERVATION-PRODUCTION-001`仅移除held DVX release的
`OnThrownInternal()`重置副作用，并以type1/2/4/6 sentinel验证投掷前后weapon HP不变。

验收至少覆盖：

- sentinel与definition `weapon_hp`不同，四种production `LF2Weapon`类型均保持sentinel；
- type2原有action draw和type1/4/6无draw合同不变；
- Vx/Vy/Vz、frame、relation、+0x2F8候选字段和release tick不被本包改写；
- SelfCheck与真实“先损伤、拾取、DVX投掷”Play probe。

## 不变量 / 后置项

- 不改kind-3、refill、C09/C20 placement、content或Scene。
- +0x2F8是否继续复用`SpawnerSlotIndex`、generic held Vz先清零、terminal、invalid reciprocal、
  cover2与旧damaged-drop额外分支均需独立审计，本记录不预判。
- 当前已有多个B6 production包`RUNTIME_PENDING`；在许可恢复或用户改变叠加策略前，本包
  保持held，不修改脚本。

## 回滚

仅移除治理记录；没有脚本、内容或Scene回滚。

## Current corpus correction（2026-09-08）

current non-kind3 DVX不为3：39个holder definitions内有184条，indexed全体有186条。weaponHP preservation
owner与后继package不变，但current覆盖必须从Naruto clone扩展到完整holder矩阵。详见multiline总correction。

## Held relation producer-domain correction（2026-09-08）

补入OPoint kind2后，完整current held-union non-kind3 DVX为186；原184是pickup-only，恰好等于all-indexed
branch count。weaponHP preservation owner与顺序不变，matrix使用186。
