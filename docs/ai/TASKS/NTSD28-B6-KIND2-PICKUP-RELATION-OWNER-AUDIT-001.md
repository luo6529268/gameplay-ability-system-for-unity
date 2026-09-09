# Task Contract — NTSD28-B6-KIND2-PICKUP-RELATION-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / THREE_PACKAGE_SPLIT_DEFINED / CURRENT_OID120_WITNESS / KIND7_DORMANT_RETIREMENT / PRODUCTION_HELD`
> 依赖：`NTSD28-B6-HORIZONTAL-IMPULSE-FINALIZER-EXIT-AUDIT-001 / VERIFIED`

## 目标

冻结 Authority kind-2 candidate gate、holder/object relation transaction、system `<weapon_throw>` table、relation
count、owner/group/link字段、WPoint action tail和unsupported target语义；对照 Unity actual/HitPlan/legacy
kind2/kind7 pickup，测量两端正式语料，并拆出 carrier/decision/atomic integration 包。

只读 Authority、Unity 与两端 DAT；不修改 C#、Config、Scene、Prefab、资源、ProjectSettings、package、Server或
Authority。

## Authority kind-2 完整合同

### candidate gate

`rebuild_geometric_hit_candidates()` 只在以下条件把几何有效kind2候选加入multiple buffer：

- attacker attack按键本tick rising edge；且
- target current state为1004且attacker `interaction_state==0`，或target current state为2004。

state2004分支不要求holder当前relation为0，因此可以覆盖既有held child；后续relation count是否增加由writer前值
单独裁决。

### relation writer与固定顺序

`resolve_special_relation_hit()` 对kind2按 target object type：

| target type | holder relation | child relation | initial holder action | 其他 |
|---:|---:|---:|---:|---|
| 1 | OID在`weapon_throw`表则101，否则1 | -1 | 115 | child即使holder=101也保持-1。 |
| 2 | 2 | -2 | 116 | — |
| 4 | 4 | -4 | 115 | — |
| 6 | HP>0为6，否则4 | 对应负值 | 115 | HP≤0先清child `weapon_hp_31c`。 |
| other | 不建关系 | 不变 | 不预写 | 仍执行后述公共tail并返回applied。 |

成功建关系时：

1. holder旧`interaction_state==0`才把`interaction_relation_count_35c`加1；
2. 写双方relation、holder `linked_child_slot`、child `linked_parent_slot`；
3. child `owner_slot=holder physical slot`、battle group=holder group；
4. 写initial holder action。

随后无论target type是否支持都把 holder `frame.frame_counter=0`，读取target current frame的第一个WPoint；
`weaponact!=0`时最后覆盖holder action。整个kind2 tail返回applied。无RNG、HP/MP伤害、rest或catch字段写入。

正式 runtime `data/system.dat` 的 `<weapon_throw>` 精确为 `{120,124}`；只对type1查表，所以type4 OID124仍走
relation4，当前/release type1 OID120走101。

### kind7 边界

kind7候选可进入special-relation dispatcher，但当前 `resolve_special_relation_hit()` 没有kind7实现并返回
unsupported；它不是无按键pickup的别名。current/release ITR corpus中kind7均为0。

## Unity 首差与 owner

### shared actual `BattleInteractionWriter.TryApplyPickup()`

- 同时接kind2与kind7；kind2按type写1/2/4/6和115/116，但没有`weapon_throw`表，所以OID120错误为1。
- 只写child `HolderStableId`和compat `HolderCopySlot`，漏 exact `OwnerSlotIndex`。
- `PickupCount++`无条件执行；Authority只在holder旧LinkState0时增加`+0x35C`。`PickupCount`是当前候选carrier，
  已进入reset/copy/ECS/checksum/parity，但尚缺B0 raw binding证明，实施前不得仅凭名称晋级exact。
- 不执行target WPoint tail；unsupported type直接false，漏frame-counter reset/applied disposition。
- type6 dead child的WeaponFlightCounter清零已有，但当前/release candidate-state corpus无type6 witness。

### shadow与legacy

- `BattleEcsHitExecutionPlan.ProjectPickupWriterEffect()`复制同样kind7与kind2逻辑，已有`TargetOwnerSlot`字段却不写；
  `AttackerPickupCount`同样无条件加1；unsupported kind2返回handled但不投影公共tail。
- `LF2CharacterDatInteractionResolver`和`LF2CharacterInteractionResolver`调用shared writer；后者另用
  `HeldWeaponReferenceInternal`作Unity compat。
- `LF2SpecialAttack.TryApplyPickupCandidate()`复制一套kind2/kind7 writer，同样缺system table、exact owner、
  conditional relation count和WPoint tail。
- 现有SelfCheck/HitPlan/Play tests明确要求kind7 pickup以及kind2 PickupCount无条件增加；这些必须按新Authority
  重新分类，不能作为绿灯继承。

## 语料测量

### ITR corpus

| Corpus | kind2 ITR | kind7 ITR |
|---|---:|---:|
| Direction-B Unity | 1 | 0 |
| 2.8 release decoded | 375 | 0 |

Direction-B唯一kind2位于`Character/naruto_clone.dat` frame65，`vrest=1`。

### indexed candidate target states

逐`data.txt` indexed definition统计state1004/2004 frames：

| Corpus | total | type1/1004 | type2/1004 | type2/2004 | type4/1004 | unsupported type | primary WPoint present/nonzero weaponact |
|---|---:|---:|---:|---:|---:|---:|---:|
| Direction-B Unity | 17 | 1 | 2 | 3 | 11 | 0 | 0/0 |
| 2.8 release | 53 | 17 | 13 | 5 | 18 | 0 | 0/0 |

因此：

- current OID120 type1 frame64/state1004是正式首差：attack rising edge命中唯一kind2 ITR时，Authority holder
  LinkState101/child-1，Unity holder1/child-1。
- 每个成功current pickup都应写child OwnerSlotIndex；Unity只写HolderCopy，故exact owner差异同样可达。
- current有3个type2/state2004 frames，允许existing positive relation下覆盖；conditional +35C差异有静态语料，
  真实already-held overlap Play待验。
- unsupported target公共tail和WPoint override在两端当前indexed candidate-state corpus中均无触发；属于规则/
  synthetic边界，不是当前内容首差。
- kind7无两端语料，Unity extra pickup应退休但只能报告dormant差异。

## 后续三包

### 1. relation-count/system-rule owner与carrier

`NTSD28-B6-KIND2-PICKUP-CARRIERS-AND-SYSTEM-RULES-001`：

- 用raw/joint binding证明或替换`PickupCount ↔ interaction_relation_count_35c`，覆盖reset/copy/snapshot/checksum/
  parity/render handoff需要；
- 为`weapon_throw={120,124}`建立具名immutable match-rule carrier/fixture，不能继续藏在kind7 hardcode；
- alternate/unknown table fail-closed与H内容策略分开，默认正式表来自当前authority runtime；
- 只建carrier/rules，不接pickup行为。

### 2. pure relation decision

`NTSD28-B6-KIND2-PICKUP-RELATION-PURE-CORE-001`：输入kind、target type/OID/HP、prior holder relation、
system membership与target WPoint weaponact；输出supported/applied、双方relation、initial/final action、counter reset、
relation-count delta、dead type6 HP cache reset与exact field mask。无world查询、RNG或分配。

### 3. atomic actual/HitPlan/legacy integration

`NTSD28-B6-KIND2-PICKUP-RELATION-ATOMIC-PRODUCTION-001`：

- candidate rising-edge/state gate保持candidate owner，writer不重复猜测；
- shared actual、HitPlan、SpecialAttack legacy duplicate同时接pure decision；
- 写 exact `OwnerSlotIndex`并继续同步HeldWeaponReference compat；`HolderCopySlot`已被
  `NTSD28-B6-LEGACY-HOLDERCOPY-MULTIPLEXED-SLOT-OWNER-AUDIT-001`纠正为待退休混合carrier，
  只能在B5 linked-holder/旧stats consumers尚未迁移时临时保留，不得再称长期exact/compat合同；
- conditional +35C、OID120 relation101/child-1、type2/4/6、unsupported common tail、WPoint final override严格排序；
- kind7从pickup dispatch/projection/legacy中退休，candidate若出现按Authority unsupported，不转普通伤害；
- actual/shadow/disposition与slot/lifecycle fields原子一致。

## 验收矩阵

- current Naruto clone frame65→OID120 frame64 formal fixture：attack rising/non-rising、LinkState0、holder101、child-1、
  owner/group/slots/action115/counter0/count+1；target OID124 type4保持4。
- types1/2/4/6，type6 HP正/零/负，type2 state1004/2004，prior relation0/nonzero，slot0/extended high。
- unsupported target + WPoint0/nonzero；valid target WPoint0/nonzero；证明counter reset与final action覆盖顺序。
- kind7 synthetic必须unsupported且全状态不变（除dispatcher diagnostic/disposition），两端corpus0 guard。
- exact OwnerSlot与compat HolderCopy故意不同；snapshot/checksum/parity/HitPlan均显示exact写入。
- current17/release53 candidate-state inventory、release375 kind2 records、formal Play拾取OID120与heavy overwrite。
- compile、focused、B5/B6/NTSD28、SelfCheck、legacy/default/HitPlan、4096 warmed zero allocation、same-tick trace。

## 不变量 / 阻塞

- 不改 catch kind1/3、kind10+ impact、candidate geometry/order、WPoint held release、positive-link validation、lifecycle
  cleanup、random weapon exception、content/Scene/Authority。
- 不把OID120/124旧hardcode解释成完整system table owner；需要具名rules carrier。
- current B6 runtime栈未清，三包均held；release内容Play受H策略约束，本轮无脚本修改。

## 回滚

仅移除本治理记录与摘要；没有代码、content、Scene、package或Authority回滚。

## Corpus correction（2026-09-08）

本记录的Direction-B `kind2=1 / 唯一Naruto clone`与supported target `17`已由
`NTSD28-B6-KIND2-PICKUP-CORPUS-CORRECTION-001`纠正为`117 tuples / 39 definitions`与`31 target frames`。
release375/53、kind7两端0、target WPoint 0/0以及全部规则/owner/三包结论保留。以后继correction为准。
