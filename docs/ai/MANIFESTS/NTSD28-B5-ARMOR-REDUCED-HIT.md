# NTSD28 B5 Armor / Reduced Hit Crosswalk

## Authority

- EXE：`B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。
- playable closure：`39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109`。
- selection：`defense_resolution.cpp`、`armor_resolution.cpp`、`battle_world.cpp:6232-6325, 7410-7535`。
- damage/rest/tail：`damage_resolution.cpp`、`battle_world.cpp:3923-3993, 7023-7269`。
- recovery placement：`simulation_tick_driver.cpp:1026`→`advance_armor_recovery_slot`。

## Formal content inventory

| 项 | NTSD 2.8-Logan runtime | Unity frozen content/model |
|---|---:|---:|
| armor DAT files/blocks | 18 / 18 | 0 / 0 |
| type-1 | 12 | 0 |
| type-0 | 6 | 0 |
| explicit hp | 6，均为1 | 0 |
| explicit recover | 0 | 0 |
| delay | type-1为`-1`或`0`；type-0均`-1` | 无typed armor |
| system invalid states | 8/11/12/13/14/16/18 | 无armor system table |
| two-way defend IDs | 822 | Unity旧hardcode helper，非正式table |

Direction B/H策略确定前不得把这18个block写入`Assets/NTSD/Config`；但它们证明正式type-1 armor存在且会改变战斗。

## Exact reduced-rest contract

- 无selected armor或`delay==-1`：target definition effect非2/4时写`min(0,-5+reduction)`；attacker
  definition effect非2/3时写`max(0,3-reduction)`。这里不读取ITR recover，也不要求attacker当前hold非负。
- packed delay：target hold减`delay%100`；再`delay/=100`，attacker hold加`delay%100`；再除100，
  第三段余数为0时清target frame counter。整数除法向0截断。
- arest：`arest<4 && vrest==0`写4，否则`min(arest,12)`；不做timing reduction。
- vrest：仅raw>0，先转`uint8`；byte<=4写4，否则`min(byte,12)`。
- negative-relation attacker在rest后只mirror到holder，不做unarmored hold negation/release。

## Defense and type-1 selection

1. ordinary defense仅kind0、effect<61、target current state 7/70/75且HP>0；state70/75直接可防。
2. state7在朝向不同、spark奇数、dbdefend1或dvx<0时可防；否则只有attacker OID在system defend表822时可防，未命中则bypass到armor/unarmored。
3. type-1 armor匹配依次检查kind list、facing、ratio与current bdefend、bdefend/fall/injury阈值、effect/id bypass、frame/state/system invalid-state active set。
4. activation：mp非0按decrease再mp换算且最低1；否则hp非0要求runtime armor HP大于effective injury，耗尽时先写-1并回退unarmored。
5. selected damage：type4或null为raw injury/10；type1/2按decrease，mp可把结果转为MP damage；armor hp按raw injury扣；target +340只缩放HP damage，attacker weak不参与。
6. reduced tail不执行unarmored status/join、effect action override、direct post-effect或type3 hold release；但仍执行weapon durability、reaction/impulse、rest、reduced attacker post-hit、audio/spark。

## Unity first differences

- `LF2AlternateDamageResolver`按旧OID 37/6/52与Prev2 defending启发式选路，不等价于current-state defense + system OID822，也没有armor record matcher。
- `ApplyAlternateDamage`统一`FallDamageDiv -> /10`，无type-1 decrease/mp/hp/activation/target-scale分支；runtime armor HP不参与。
- reduced holds固定3/-5，忽略definition effects与world reduction；arest未做native byte输入前的vrest规则。
- armor recovery carrier/kernel和C25i placement已存在，但profile seam固定false；spawn初始化也没有首armor hp/recover。
- HitPlan复制同一旧alternate公式，因此只能证明Unity内部一致，不能证明Authority一致。

## Implementation split

1. `NTSD28-B5-REDUCED-HIT-REST-PURE-RESOLVER-001`：数据无关的exact rest/packed-delay truth table。
2. defense matcher pure core + system OID822 carrier/consumer。
3. reduced/defense actual与HitPlan integration（selected armor=null），先关闭所有角色共有的ordinary defense路径。
4. armor typed model/parser/copy/fingerprint、type1 match/activation/damage pure cores与runtime initialization。
5. type1 production integration、recovery profile与formal content deployment；第4/5步中的正式content写入受H/B11策略门约束。
6. audio/spark留B10/B9，resource attribution依赖H/B11已路由字段，不在rest包偷接。

## Reduced-rest pure resolver result

`NTSD28-B5-REDUCED-HIT-REST-PURE-RESOLVER-001 / VERIFIED / PURE_CORE_READY /
PRODUCTION_UNCONNECTED`已闭合default/effect/reduction、signed packed delay、frame-counter flag、4/12 arest与
native-byte vrest，含4096次零分配。证据为red13、focused14、B5+HitPlan425、NTSD28 broad706、
SelfCheck/Scene/Ledger PASS。下一ordinary-defense pure resolver；不接armor content。

## Ordinary-defense pure resolver result

`NTSD28-B5-ORDINARY-DEFENSE-PURE-RESOLVER-001 / VERIFIED / PURE_CORE_READY /
PRODUCTION_UNCONNECTED`已闭合kind/effect/state/HP与state7 facing/spark/dbdefend/dvx/OID822真值表，
含4096次零分配。证据为red13、focused14、B5+HitPlan439、NTSD28 broad720、SelfCheck/Scene/Ledger PASS。
下一补reduced-hit damage pure core，再规划atomic production integration。

## Reduced-hit damage pure core result

`NTSD28-B5-REDUCED-HIT-DAMAGE-PURE-CORE-001 / VERIFIED / PURE_CORE_READY /
PRODUCTION_UNCONNECTED`已闭合null/type4 signed `/10`、type3 unsupported、decrease、HP/MP分流、runtime armor-HP
delta及HP-only target `+0x340` low-32-bit scale，API不接受attacker weak，含4096次零分配。证据为red20、
focused21、B5 265、HitPlan184、NTSD28 broad741、SelfCheck/Scene/Ledger PASS。下一以三个pure resolver执行
ordinary-defense/reduced-hit原子生产接线；正式type1 content继续受H/B11门约束。

## Pre-integration carrier correction

接线审计发现`InteractionRecord28.spark/dbdefend`在Unity `InteractionArea`、parser、CopyFrom与HitPlan
projection/fingerprint中均无typed carrier。当前Authority正式character/object ITR/weapon-strength与Unity 138-DAT
没有显式二字段，默认0保持不变；但ordinary-defense pure resolver的非默认输入合同不完整。因此先执行
`NTSD28-B5-ITR-DEFENSE-FIELDS-CARRIER-001`，完成后再做atomic production integration。

`NTSD28-B5-ITR-DEFENSE-FIELDS-CARRIER-001 / VERIFIED / CARRIER_READY / BEHAVIOR_UNCONNECTED`已完成：
typed parser、CopyFrom、HitPlan raw/projection fingerprint、constructor与kind5 replacement均承载二字段；red4、
focused4、B5 269、HitPlan184、NTSD28 broad745、SelfCheck/Scene/Ledger PASS。下一atomic production integration。

## Ordinary-defense / null-armor production integration result

`NTSD28-B5-ORDINARY-DEFENSE-REDUCED-HIT-PRODUCTION-INTEGRATION-001 / VERIFIED /
PRODUCTION_CONNECTED / NULL_ARMOR_DEFENSE_ALIGNED`已把actual两入口与HitPlan统一接入current-state defense/OID822、
null-armor raw `/10` + HP-only target `+0x340`、weak exclusion及exact reduced rest；旧OID37/6/52/Prev2启发式
已退休。证据为red3、focused3、B5 272、HitPlan184、NTSD28 broad748、SelfCheck/Scene/Ledger PASS。
下一exit audit；type1 model/content与activation仍受H/B11门约束。

## Ordinary-defense / null-armor exit audit

`NTSD28-B5-ORDINARY-DEFENSE-REDUCED-HIT-EXIT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY /
NULL_ARMOR_FAMILY_EXIT_READY`确认actual两入口、HitPlan consumers及actual/projection damage/rest pure owners已闭合；
production无旧OID37/6/52/Prev2 selection或FallDamageDiv/weak reduced damage残留。下一
`NTSD28-B5-TYPE1-ARMOR-DATA-CONTRACT-001`；不在该包部署正式content。

## Type1 armor data contract result

`NTSD28-B5-TYPE1-ARMOR-DATA-CONTRACT-001 / VERIFIED / DATA_CONTRACT_READY /
PRODUCTION_SELECTION_UNCONNECTED`已建立ArmorRecord28完整typed model、双整数frame parser、last-win/type优先与
ptype fallback转换、重复列表/range/sound、deep-copy、allocation-free fingerprint和formal loader seam。
red6、focused7、B5 279、NTSD28 broad755、SelfCheck/Console/Scene/Ledger均通过。下一type1 armor match pure core；
本包没有接selection/activation/runtime HP/recovery，也没有部署正式content。

## Type1 armor match pure core result

`NTSD28-B5-TYPE1-ARMOR-MATCH-PURE-CORE-001 / VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`
已按`ArmorResolver28::match_type1`闭合kind/facing/strict threshold/effect-id bypass/frame-state OR及2.8.3.3
invalid-state精确匹配核心；red24、focused25、B5 304、broad780、SelfCheck/Console/Scene/Ledger PASS。
下一activation pure core；不接production/content或Scene。

## Type1 armor activation pure core result

`NTSD28-B5-TYPE1-ARMOR-ACTIVATION-PURE-CORE-001 / VERIFIED / PURE_CORE_READY /
PRODUCTION_UNCONNECTED`已按`ArmorResolver28::activation_cost`闭合MP两级64位成本、最低1、exact availability及
runtime armor HP缺失/严格破甲`-1`纯核心；red11、focused12、B5 316、broad792、SelfCheck/Console/Scene/Ledger
PASS。下一runtime-initialization audit；不接production/content或Scene。

## Runtime initialization audit

`NTSD28-B5-TYPE1-ARMOR-RUNTIME-INITIALIZATION-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY /
IMPLEMENTATION_SPLIT_DEFINED`确认Authority出生与C25i只读首armor；Unity字段、snapshot/checksum及C25i kernel已存在，
但profile seam固定false且出生为0/-1。下一runtime-profile integration；破甲`-1→action→0`留atomic hit integration，
正式content继续受H/B11门约束。本审计无C#/content/Scene修改。

## Runtime profile integration result

`NTSD28-B5-TYPE1-ARMOR-RUNTIME-PROFILE-INTEGRATION-001 / VERIFIED / PRODUCTION_PROFILE_CONNECTED /
HIT_SELECTION_UNCONNECTED`已接首armor profile、ModuleBind出生/reuse初始化、C25i生产读取与snapshot-skip；
red7、focused8、B5 324、broad800、SelfCheck/Console/Scene/Ledger PASS。下一atomic production audit；
不接selection/damage/content或Scene。

## Atomic production integration result

`NTSD28-B5-TYPE1-ARMOR-ATOMIC-PRODUCTION-INTEGRATION-001 / VERIFIED / PRODUCTION_CONNECTED /
TYPE1_ARMOR_CORE_TRANSACTION_ALIGNED`已原子接defense优先、type1 match/activation、selected reduced、
unarmored fallback与broken `-1→action→0`到actual+HitPlan；最终focused10、B5 353、HitPlan184、broad818、
SelfCheck/Console/Scene/diff/Ledger PASS。下一type1 armor exit audit；resource/audio/spark/content保持独立。

## Type1 armor exit audit 001

`NTSD28-B5-TYPE1-ARMOR-EXIT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / BREAK_VERTICAL_ORDER_ROUTED`确认
actual在horizontal/break前可能先写vertical，HitPlan在break后先写attacker post-hit再写vertical。下一targeted
order correction；跨族resource/audio/spark/content继续独立。

## Broken armor vertical-order correction result

`NTSD28-B5-TYPE1-ARMOR-BREAK-VERTICAL-ORDER-001 / VERIFIED / BREAK_VERTICAL_POSTHIT_ORDER_ALIGNED`
已统一broken fallback的horizontal→break→vertical→attacker post-hit actual/HitPlan顺序；red2/3、focused3、
HitPlan184、B5 356、broad821、SelfCheck/Console/Scene/diff/Ledger PASS。下一exit audit 002。

## Type1 armor exit audit 002

`NTSD28-B5-TYPE1-ARMOR-EXIT-AUDIT-002 / VERIFIED / GOVERNANCE_ONLY /
TYPE1_ARMOR_SPECIFIC_FAMILY_EXIT_READY`确认type1-specific production owner与Authority链无剩余首差；
cross-family resource、B9/B10 audio/spark与H/B11 content仍阻止4.7/全局完成。下一hit-resource readiness audit 002。

## Atomic production audit

`NTSD28-B5-TYPE1-ARMOR-ATOMIC-PRODUCTION-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY /
PREREQUISITES_ROUTED`确认actual+HitPlan现有布尔alternate分流不能表达selected/bypass/activation/broken armor，
并缺definition attacking及HitPlan runtime armor/consumption写面。下一definition-attacking carrier，再补HitPlan carrier，
最后原子接selection→activation→reduced/fallback→break；content继续gated。本审计无C#/content/Scene修改。

## Definition attacking carrier result

`NTSD28-B5-DEFINITION-ATTACKING-CARRIER-001 / VERIFIED / DATA_CARRIER_READY /
PRODUCTION_CONSUMPTION_UNCONNECTED`已补`stats.attacking` typed field与正式converter；red4、focused4、B5 328、
broad804、SelfCheck/Console/Scene/Ledger PASS。下一HitPlan runtime armor/consumption carriers；不接行为、content或Scene。

## HitPlan runtime carriers result

`NTSD28-B5-TYPE1-ARMOR-HITPLAN-RUNTIME-CARRIERS-001 / VERIFIED / HITPLAN_CARRIERS_READY /
ARMOR_TRANSACTION_UNCONNECTED`已把runtime armor HP与input HP/MP消费累计加入writer-effect capture/compare；
red4、focused4、B5 343、broad808、SelfCheck/Console/Scene/Ledger PASS。下一atomic production integration；
不接selection/damage/content或Scene。
