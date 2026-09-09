# Task Contract — NTSD28-B6-WPOINT-MISSING-ACTION-CONTINUE-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / ACTION_WRITE_THEN_CONTINUE / REAL_AND_GENERIC_OWNER / CURRENT_16796_WITNESS / DORMANT_RULES_UNION_RECONFIRMED / PRODUCTION_HELD`
> 依赖：`NTSD28-B6-HELD-RELATION-PRODUCER-DOMAIN-CORRECTION-AUDIT-001 / VERIFIED`

## 目标

冻结 Authority held/refill pass 在 `weaponact < 1000` 但child definition未声明目标action时的精确
write/diagnostic/continue边界；定位Unity real/generic多写、第二pass不恢复与RNG/relation泄漏，并定义单一后继
production package。同步用完整ITR+OPoint relation edge union复核cover2与state12/18 dormant结论。

## Authority 闭环

`BattleWorld28::settle_held_refill_objects()`在reciprocal/refill/terminal之后按以下顺序执行：

1. `child->frame.action = parent_wpoint->weapon_action`；signed负值也原样写入。
2. 用写后的action解析child definition frame，再读取该frame的primary WPoint。
3. action不存在时设置pass `success=false`、递增`unsupported_links`、写diagnostic并立即`continue`。
4. 该分支保留刚写入的child action以及双方relation；不写child facing、motion hold timer、position、precise
   position或motion，不执行DVX/kind3，不消费RNG，也不读child textual WPoint。
5. 下一次C09/C20调用仍从reciprocal/refill开始；如果holder改到target已声明的weaponact，child必须能从此前
   missing frame恢复到正常follow。current child frame是否存在不是pass entry gate。

声明了target frame但没有文本WPoint不是unsupported：`first_weapon_point()`返回native全零fixed record并继续
pose/release。实现不得把“missing action”和“valid action with zero/default WPoint”混为一谈。

## Unity 当前差异

### Real weapon

`LF2WeaponHeldStateResolver.Act()`先以 `weapon.Frame.D == null`提前返回，随后才做refill。正常首次进入时它：

- 写target action并令`Frame.D=null`；
- 仍切facing、复制`FrameDelay`、写`WeaponState=WeaponOnHand`；
- 以center/WPoint零值重算位置并应用cover offset；
- DVX时可能改action40或消耗type2 RNG、写motion并清relation；kind3时外层writer继续四draw随机drop。

第二次held pass因entry guard直接返回，既不重新执行refill，也不能在holder切换到valid weaponact后恢复。

### Generic fallback

`BattleHeldObjectWriter.RunStep12()`的non-weapon路径在`SyncHeldFrameAndPosition()`中写missing action后仍执行
facing/frame-delay/pose；随后也可能执行damaged、DVX或kind3 tail。它没有real path的entry guard，但同样缺少
action-resolution continue。

### Dead compatibility path

`LF2CharacterWeaponLinkResolver.ReleaseHeldObjectByWPoint()`包含另一份类似逻辑，但当前只有未接线的
`LF2WeaponPointFactory/LF2WeaponPointModule`引用，production world不调用。该路径不得成为第二owner；source
guard必须锁定无生产caller，未来若接线须独立迁移同一合同。

## Current Direction-B edge-aware evidence

关系域使用41 source definitions / 668真实producer edges，不做无条件笛卡尔积：

| 指标 | corrected current |
|---|---:|
| missing holder-frame/edge rows | 16,796 |
| distinct `(source definition, weaponact, target definition)` triples | 2,402 |
| OPoint-only edge增量 | 549 rows / 84 triples |
| signed `weaponact=-888` | 119 rows / 17 triples |
| affected source definitions | 39 |
| target type1 / 2 / 3 / 4 / 6 rows | 2,941 / 12,778 / 10 / 528 / 539 |
| missing rows with parent `dvx!=0` | 613 |
| missing rows with parent `kind==3` | 2,081 |

current witnesses：

- Sakura OID1 actions12..16/50等对OPoint-held OID122/123写`weaponact=10`，target无action10；
- Sakura action51还带DVX70：Authority只保留action10+relation，Unity会继续throw/release；
- Sakura actions220..等kind3对OID150写missing35：Authority零RNG，Unity会四draw drop；
- Sakon OID35 actions235..239/247/248写signed `-888`，包括OPoint-held OID123与pickup targets。

这些是formal current-content可达关系/帧组合；真实输入与时序仍须Play验证。

## Dormant rules union recheck

- 完整41-source WPoint union中`cover=2`仍为0。
- 对668 edges，所有target已声明且被source `weaponact<1000`引用的frame中，state12/18匹配仍为0。
- 因此cover2 no-offset与Unity extra damaged-held-drop在current完整relation域仍dormant；先前
  `RECHECK_REQUIRED`恢复为`DORMANT_CURRENT_AND_RELEASE`。release既有cover2/state12/18结果均为0。
- generic DVX Vz preserve是synthetic fallback规则差异，owner不受本次edge join影响。

## Production owner

后续单一包：`NTSD28-B6-WPOINT-MISSING-ACTION-CONTINUE-PRODUCTION-001`，依赖lifecycle cleanup与terminal
production依次取得runtime绿灯。

1. `WeaponActResult`增加transient `UnsupportedWeaponAction` outcome；不进入runtime snapshot/checksum。
2. real resolver移除“current `Frame.D`必须存在”的entry gate：先执行state17 refill/exhaustion，再由terminal
   package处理`>=1000`，然后写`weaponact`并立即解析新`Frame.D`；null时只置unsupported并返回。
3. 只有新target frame存在时，real path才允许facing、FrameDelay、WeaponState、pose、damaged/DVX/kind3。
4. generic writer同样先写action、解析target frame，null时置unsupported并返回；重构pose helper避免二次写action。
5. world consumer对unsupported保留双方relation，刷新child已写action的runtime snapshot并继续下个slot；不得调用
   `Free/Destroy`，不得发structural mutation event。
6. terminal outcome优先于action write；exhaustion优先于terminal；unsupported优先于DVX/kind3。三个outcome必须
   互斥并通过顺序test锁定。

## 验收矩阵

- real/generic：missing正action、signed -888、valid frame无WPoint、missing→valid recovery；
- state17：current child frame missing仍执行refill；exhaustion early return；nonexhausted后才检查target action；
- side effect sentinels：facing、FrameDelay、WeaponState、XYZ/precise、VXYZ、relation、weaponHP、RNG cursor；
- parent DVX type1/2/4/6与kind3 missing均不得release/draw；下一slot仍处理；C09/C20各自重复诊断而不多写；
- source guard：完整relation domain固定41/668，missing固定16,796/2,402，cover2/state12/18保持0；dead factory无caller；
- compile、focused、B6 regression、SelfCheck、Sakura OID122/123与Sakon -888 targeted Play、同seed/input/tick
  first-difference。运行证据不全时最多`RUNTIME_PENDING`。

## 不变量 / 阻塞

- 不修改Config/Scene/Prefab/resource/importer、Authority、capacity、pass placement、refill数值、terminal structural
  owner、kind3/DVX规则值、catch或shutdown顺序。
- 不以提前preflight阻止Authority必须保留的child action write；不把diagnostic分支错误变成unlink/despawn。
- 当前Unity runtime栈未清，且前置lifecycle/terminal production未验证；本轮只做owner审计。

## 回滚

仅移除治理记录；没有脚本、content、Scene或Authority回滚。
