# Task Contract — NTSD28-B6-LEGACY-HOLDERCOPY-MULTIPLEXED-SLOT-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / NO_SINGLE_AUTHORITY_FIELD / MULTIPLEXED_OWNER_CONFIRMED / B5_EXIT_CORRECTIONS_REQUIRED / CURRENT_TWO_HOP_GRAPH_WITNESS / ROUTES_SPLIT / SCHEMA_DIRECTION_GATED / PRODUCTION_HELD`
> 依赖：`NTSD28-B5-HIT-GROUP-ELIGIBILITY-EXIT-AUDIT-001`、`NTSD28-B5-TYPE3-TARGET-GENERIC-CONTINUATION-001`、`NTSD28-B6-HELD-INJURY-ACCOUNTING-OWNER-AUDIT-001`、`NTSD28-B6-NATIVE-IMPACT-10111718-OWNER-AUDIT-001`

## 目标

穷尽 Unity `NTSDEntityRuntime.HolderCopySlotIndex` / `LF2Entity.HolderCopySlot` / `OPointCreateTask.holderCopySlot`
的生产读写与持久化边界，将每个用途分别映射到当前 NTSD 2.8 Authority exact field，并更正
先前把该legacy multiplexed slot当作可保留compat identity的过宽结论。

本Task只修改治理记录；不修改C#、Config、Scene、Prefab、资源、ProjectSettings、Server或Authority。

## Authority结论：不存在单一对应字段

正式`EntityState28`没有default 99、随OPoint复制生成根、同时又代表direct holder的可变字段。
Unity当前合并的语义在Authority中分属：

| 语义 | Authority exact source | Unity应使用 |
|---|---|---|
| negative relation的直接holder | `linked_parent_slot` + `interaction_state` | `Runtime.HolderStableId` + `LinkState` |
| positive relation的直接child | `linked_child_slot` + `interaction_state` | `Runtime.TargetSlotIndex` + `LinkState` |
| OPoint子对象owner/credit | parent literal `owner_slot` | `Runtime.OwnerSlotIndex` |
| 战斗分组 | `battle_group` | `Runtime.RelationTeam` |
| type3 generic control/cycle source | `control_slot_000` | `Runtime.AnimCounter` |
| 实体自身physical slot | `EntityState28::slot` / world slot index | `Runtime.SlotIndex` |
| 伤害/击倒积分 | `input_score_total_348` / `knockout_count_358` + exact owner chain | 已有canonical score/KO carriers |

`object_spawning.cpp::ObjectSpawnPlanner28::plan()`只把parent `owner_slot`与`battle_group`复制到intent；
`spawn_from_opoint_intents()`对kind2另写immediate `linked_parent_slot=parent physical slot`。这两个slot合法且
可不同，不得由一个“根holder copy”合并。相关`battle_world.cpp`、`object_spawning.cpp`、
`hit_candidates.cpp`均在正式playable build closure。

## Unity production closure

不区分大小写法的case-insensitive扫描共得到151行引用，其中非test生产代码63行、26个文件；
测试代码88行。字段由旧`f03773f3`“C# authority parity tooling”阶段引入，该历史无权裁决当前规则。

### 1. Linked-holder与candidate consumers

- `BattleHitCandidatePairSnapshotFactory.Capture()`通过
  `ResolveReleaseNeutralHolderSlotOrImplicitZero()`读`HolderCopySlot`，以此构造frozen
  `LinkedHolderPresent/LinkedHolderBattleGroup`。Authority直接读attacker `linked_parent_slot`。
- `BruteForceSceneQuery.ResolveKind5HolderRuntimeSlot()`与HitPlan kind5 projection使用同一错误helper。
- nearest/body eligibility的`ResolveReleaseNegativeLinkHolderSlotOrImplicitZero()`虽先读
  `HolderStableId`，但invalid时退回`HolderCopySlot`；Authority没有该fallback。

这意味着B5 hit-group pure truth table和frozen carrier本身仍可保留，但linked-holder采样binding不精确；
先前`HIT_GROUP_ELIGIBILITY_FAMILY_EXIT_READY`必须改为“core保留、binding correction pending”。

### 2. Type3 extra writer

`BattleDamageWriter.CopyRelation()`和HitPlan的3个对应projection在写完exact
`RelationTeam/OwnerSlotIndex/AnimCounter`后，仍把source `HolderCopySlot`额外复制到target。
Authority type3 generic/kind-transform transaction没有这项写入。先前type3包的exact group/owner/control、action和
motion证据保留，但“整个continuation aligned”范围必须收窄，直到extra actual/HitPlan write退休。

### 3. Legacy damage/stat attribution

- ordinary/reduced hit actual通过`LF2HitResolveRuntimeData.ResolveHolderCopyEntity()`把伤害和lethal event
  累加到`ComboCountAtk/KillStat`，同时维护`ComboCountVic`与world `DamageStats/KillStats`。
- HitPlan `HolderHandle/HolderComboCountAtk/HolderKillStat`从同一`HolderCopySlot`采样并对比旧写入。
- Authority当前EntityState只有已建立的score/KO/display/native-combo carriers；没有这组legacy damage-total
  fields。但它们的全部reader/writer、UI/BattleFlow消费和snapshot范围须在独立owner audit中穷尽，
  不允许本审计仅因Authority header无同名字段就直接删除。

### 4. B6 impact和held-injury consumers

- character kind10/11旧branch用HolderCopy累加holder combo +11；
  `NTSD28-B6-NATIVE-IMPACT-ATOMIC-PRODUCTION-INTEGRATION-001`已明确退休该额外统计。
- `BattleCpointWriter.ApplyHeldInjury()`用HolderCopy写legacy kill/combo；
  `NTSD28-B6-HELD-INJURY-ACCOUNTING-COVER-PRODUCTION-001`已明确改用一层`OwnerSlotIndex`
  或type0 self credit，并禁止写旧stats。

这两组已有正确owner，本审计不建重复实施包。

### 5. Producers / initialization

- primary/stage character写99或自身slot；OPoint/task/factory及hit_Fa8/13子对象继承parent
  HolderCopy；pickup/HoldWeapon又把target写为direct attacker/holder slot。
- OPoint kind2 `AttachOpointHeldObject()`把held child写为“parent的HolderCopy”，而exact
  `HolderStableId`写为“immediate parent physical slot”；嵌套生成时两者会分离。
- reset、death drop、OID51/52、state9996、respawn和transition effect在99/-1/self之间使用不同默认，
  没有一个Authority lifecycle字段可解释整套writer集合。

### 6. Persistence

`HolderCopySlotIndex`进入runtime reset/canonical copy、entity snapshot schema12、full snapshot schema19、
ECS Links/fingerprint、lockstep checksum schema22和parity JSON `holderCopy`；未进B0 current-authority raw exporter。
所以行为consumer/producer迁移必须先完成，carrier删除才能与其他legacy字段一次升schema。

## Current-content reachability

- current正式relation producers：ITR kind2 117条/39 holder definitions；OPoint kind2 62条/
  39 source definitions/56 distinct source-target edges。这两类均动态写HolderCopy。
- current frozen projection中有71条OPoint会生成“自身又包含kind2 OPoint”的14个type0 definition，
  共30个source-target pairs。例如Naruto action358/360以kind1生成OID2 action300，而OID2自身
  actions81/254/291可以kind2生成held child；Sound Nin action349生成OID30 action350，OID30
  actions371/373/375又含kind2。
- 上述是结构化两跳producer graph见证：第二层holder physical slot与继承root HolderCopy
  必然不同。生成角色是否在真实对局进入对应kind2 action仍需Play/trace，不把静态图夸大为
  已复现runtime首差。
- current还有371条kind5 ITR、2353条kind0 ITR、20条kind10/11和223条正held injury corpus exposure。
  这些是消费路径的内容暴露，不等于每个pair都已在Play命中。

## 后继路由与依赖顺序

### A. 先重开B5字段绑定正确性

1. `NTSD28-B5-KIND5-LINKED-PARENT-SLOT-CORRECTION-001`：把frozen pair、BruteForce kind5、
   nearest/body negative-link和HitPlan统一到`HolderStableId`，移除HolderCopy fallback；覆盖slot0、
   high slot、root!=immediate holder、group/frame不同和invalid reciprocal。
2. `NTSD28-B5-TYPE3-LEGACY-HOLDERCOPY-WRITER-RETIREMENT-001`：只删actual/HitPlan type3额外
   HolderCopy写入，保留已验证的group/owner/control/action/motion。
3. `NTSD28-B5-LEGACY-DAMAGE-STATS-OWNER-AUDIT-001`（已闭合）：已穷尽`ComboCountAtk/Vic`、
   `KillStat`、`KillCount`、`DamageStats/KillStats`的Authority、AI/lifecycle、snapshot和所有
   actual/HitPlan writers；七条后继路由已冻结，动态读写归零前不直接删统计字段。

完成A之前，旧B5 hit-group family exit和type3 whole-continuation aligned都只能作为核心子集证据；
不能支持B5全域完成声明。可继续B6只读owner audit，但不应在这些B5更正前叠加新B6生产代码。

### B. 复用已有B6 owner

- impact由`NTSD28-B6-NATIVE-IMPACT-ATOMIC-PRODUCTION-INTEGRATION-001`退HolderCopy旧stats。
- held injury由`NTSD28-B6-HELD-INJURY-ACCOUNTING-COVER-PRODUCTION-001`退HolderCopy旧stats。
- 对`NTSD28-B6-KIND2-PICKUP-RELATION-ATOMIC-PRODUCTION-001`的纠正：exact
  `OwnerSlotIndex/HolderStableId`和managed held reference可继续；HolderCopy write不得再称为长期compat
  不变量，只能在A项消费者尚未迁移时临时保留。

### C. B6/B7 producer retirement

- `NTSD28-B6-LEGACY-HOLDERCOPY-HELD-PRODUCER-RETIREMENT-001`：在A/B consumers已退后，删除
  HoldWeapon/Attach/pickup/release/death等B6 held路径的HolderCopy writes，不改canonical links。
- OPoint task/factory/hit_Fa8/13的root-copy属于B7 spawn/owner完整交易；后续建立
  `NTSD28-B7-OPOINT-OWNER-AND-HOLDERCOPY-PROPAGATION-AUDIT-001`，不在B6提前修生成逻辑。

### D. Carrier/schema disposition

`NTSD28-B6-LEGACY-HOLDERCOPY-CARRIER-DISPOSITION-001 / USER_DIRECTION_REQUIRED`：

- 只有在B5/B6/B7所有行为读写为零后，才能删field/property/task payload/reset/copy/ECS/
  checksum/parity/test sentinels和99/-1 lifecycle writes；
- 并入`ReleaseTick/WeaponState/Tracker/GrabbedBy`同一次entity `12→13` / full `19→20` /
  checksum `22→23`联合迁移；HolderCopy当前确实进checksum和parity，所以它是22→23的直接参与者。
- snapshot/recovery compatibility方向仍需用户决定，本审计不自行选择拒绝、迁移或保留reserved位。

## 验收矩阵

- linked holder：direct ITR pickup、OPoint kind2、两跳generated character、slot0/high/reuse；
  `HolderStableId != HolderCopySlot` sentinel必须证明frozen pair/kind5/nearest只读exact holder。
- type3：direct source/active holder source、generic/kind transform、actual/HitPlan；HolderCopy sentinel bitwise不变，
  group/owner/control/action/motion继续与旧绿灯一致。
- legacy stats owner audit：ordinary/reduced/type3/B6 impact/CPoint、lethal/nonlethal、owner chain、slot0/high，
  区分canonical score/KO/native combo/display与Unity旧damage totals。
- held producer：117 ITR-kind2、62 OPoint-kind2、71 two-hop authored rows；删writer前后canonical relation、
  owner/group/action/RNG/lifecycle不变。
- carrier包若获批，snapshot mismatch/history/session/checksum/parity/ECS layout、OPoint task reset、
  4096 warm copy/hash 0 B与旧schema policy全覆盖。
- compile、focused B5/B6/HitPlan/NTSD28、SelfCheck、collision-hit/OPoint-linked/type3/held Play及
  same-seed/input/tick first-difference。没有新runtime证据时不得恢复family exit或`VERIFIED` behavior声明。

## 不变量 / 回滚

- 不改Authority、content/Scene/Prefab/importer、relation/group/owner/control exact semantics、candidate顺序、
  damage数值、RNG、shutdown或用户例外。
- 本轮仅登记纠正与路由；回滚只移除本Task/Change及摘要，没有脚本、content、Scene或Authority回滚。
