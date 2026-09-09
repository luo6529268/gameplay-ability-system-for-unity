# Task Contract — NTSD28-B6-LEGACY-GRABBEDBY-RELATION-MIRROR-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / NO_AUTHORITY_SECOND_RELATION_FIELD / CURRENT_OPOINT_WRITER_REACHABLE / LEGACY_READER_ROUTED / TWO_NEW_PACKAGES_PLUS_EXISTING_CONSUMER / SCHEMA_DIRECTION_GATED / PRODUCTION_HELD`
> 依赖：`NTSD28-B6-LEGACY-TRACKER-RELATION-OWNER-AUDIT-001 / VERIFIED`、`NTSD28-B6-KIND2-PICKUP-RELATION-OWNER-AUDIT-001 / VERIFIED`

## 目标

审计 Unity `NTSDEntityRuntime.GrabbedBy`的全部生产读写、快照/ECS/校验传播与当前内容可达性，
确认它是否对应 NTSD 2.8 Authority 的独立字段，并将behavior退休与carrier/schema处置拆开。

本Task只修改治理记录；不修改C#、Config、Scene、Prefab、资源、ProjectSettings、Server或Authority。

## Authority关系合同

- 正式playable closure的`EntityState28`只以`interaction_state`表示关系类型/符号，以
  `linked_child_slot`和`linked_parent_slot`表示双向physical-slot端点；没有`grabbed_by`、
  signed mirror或第二个interaction-state字段。
- kind2 pickup、OPoint kind2、C09/C20 held refill、input/AI、physics、kind5 substitution和despawn cleanup
  全部直接读写上述canonical字段。`battle_world.cpp`、`native_ai.cpp`、`input_routing.cpp`和
  `object_spawning.cpp`均被`source/ntsd28_playable/scripts/build.ps1 -Target playable`列入正式core closure。
- Authority不要求在holder和child上再复制一份正/负relation值，也不会在release/drop时清理
  一个独立mirror。

## Unity字段闭包

### 动态写者

- `LF2CharacterWeaponLinkResolver.HoldWeapon()`在child mirror为0时写`-1`，但canonical
  `LinkState`可为`-1/-2/-4/-6/-101`，所以这不是数值等价mirror。
- `AttachOpointHeldObject()`无条件把child写`-1`，却不给holder写对应正值。
- `LF2WeaponInteractionResolver.ApplyPickupGrabbedBy()`又使用另一套旧路径，把holder/weapon写为
  `+/- 101/2/4/6`；当前shared `BattleInteractionWriter.TryApplyPickup()`不读写该字段。
- `LF2CharacterWeaponLinkResolver`、`LF2WeaponHeldStateResolver`、`LF2WeaponReleaseFlowResolver`、
  `BattleHeldObjectWriter`及pool lifecycle在drop/release/reset中写0。这些清理只是为旧mirror服务，
  不是Authority lifecycle的独立状态交易。

### 读者

全repo共40个`GrabbedBy`文本引用，其中35个在非test代码。生产代码仅3个`if`读点：

1. `LF2CharacterHitResolver`与`LF2CharacterDatHitResolver`的raw-kind5旧duplicate以`GrabbedBy < 0`
   作为前置门；它们已由`NTSD28-B6-LEGACY-TRACKER-RELATION-CONSUMER-RETIREMENT-001`
   统一退休，不另建第二个重叠consumer包。
2. `LF2CharacterWeaponLinkResolver.HoldWeapon()`的`==0`分支只决定是否写`-1`，不改变
   canonical relation、action、motion、RNG或lifecycle结果；它属于nonzero producer本身。

ECS `Links.GrabbedBy`没有其他读者，只被capture、shadow compare和`BattleRuntimeFingerprint`哈希。

### 持久化与确定性

- `NTSDEntityRuntime.TryCopyCanonicalStateTo()`会复制`GrabbedBy`，因而它进入entity runtime snapshot
  schema 12与full snapshot schema 19。
- 它进入ECS Links和`BattleRuntimeFingerprint`，但不进入`BattleLockstepChecksumModule`、
  `BattleParitySnapshot`或`NTSD28UnityEntityRawCapture`。当它仍有动态writer与gameplay reader时，
  这是明确的lockstep/parity盲区，不能用“没有checksum首差”证明它等价。
- default/full reset为0。它既不是`LinkState`、`HolderStableId`、`HolderCopySlot`、
  `TrackerFlag`、`TrackerParent`、owner+0x354、excluded-source+0x2F8或target+0x3F8。

## Current reachability

- Direction-B frozen projection有62条OPoint kind2、39个source definitions，全部source路径都在
  `Character/`；因此当前factory的character-parent分支都会进入`AttachOpointHeldObject()`，
  child `GrabbedBy=-1`是current reachable的snapshot/fingerprint额外状态。
- 当前117条ITR kind2 / 39 holder definitions由shared pickup writer建立canonical relation，却保持
  `GrabbedBy`旧值/默认0；两个正式relation producer domain对mirror的处理不一致，进一步证明
  它不能作为canonical或compat identity。
- 旧raw-kind5读者的current formal可达性沿用Tracker审计结论：13个OPoint-kind2 target OID
  与indexed production的353条kind5 ITR交集为0，而shared kind5已在dispatch前用canonical link完成替代。
  这只证明旧reader在默认formal current路径旁路/休眠，不改变OPoint writer和快照状态的可达性。

## 后继实施顺序

### 1. 复用既有legacy consumer retirement

`NTSD28-B6-LEGACY-TRACKER-RELATION-CONSUMER-RETIREMENT-001`先删除两个raw-kind5 duplicate及
managed tracker fallback，使正式kind5只有canonical reciprocal-link owner。本审计不重复修改这些文件。

### 2. Nonzero producer retirement

`NTSD28-B6-LEGACY-GRABBEDBY-NONZERO-PRODUCER-RETIREMENT-001`：

- 删除`HoldWeapon()`/`AttachOpointHeldObject()`的`-1` writes和
  `LF2WeaponInteractionResolver.ApplyPickupGrabbedBy()`及两个调用点；
- 保留所有canonical `LinkState/TargetSlotIndex/HolderStableId`、owner/group/action与compat reference行为；
- carrier、reset/copy/ECS与零值cleanup暂留，不升snapshot或checksum schema；
- source guard要求production无条件读、无nonzero writer，formal OPoint/pickup/release的logic和RNG不变。

### 3. Carrier/schema disposition

`NTSD28-B6-LEGACY-GRABBEDBY-CARRIER-DISPOSITION-001 / USER_DIRECTION_REQUIRED`：

- 删除field/property/reset/copy、ECS Links array/shadow compare/fingerprint及剩余zero writes/test sentinels；
- 独立看该字段的持久化影响是entity runtime snapshot `12→13`与full snapshot `19→20`；
  它当前未进checksum，不得伪称删除它自身就要求`22→23`。
- 实际实施应与`ReleaseTick`、`WeaponState`、`TrackerFlag/TrackerParent`、`HolderCopySlot`的既定联合迁移使用一次
  `entity 12→13 / full 19→20 / checksum 22→23`，避免多个伪中间schema。checksum升级来自那些
  已进checksum的carrier，不是`GrabbedBy`。
- snapshot/recovery旧版拒绝、迁移或兼容策略属于用户方向，普通B6行为包无权决定。

## 验收矩阵

- current OPoint kind2 62 records/39 character sources/56 distinct edges：character parent、slot0、high slot、
  same-tick reuse；producer退休前后canonical links/group/action/position/RNG相同，mirror恒0。
- current ITR kind2 117/39：type1/2/4/6、OID120 relation101、heavy overwrite；shared writer不依赖mirror。
- release/drop/refill：DVX、kind3、terminal、drink exhaustion、invalid reciprocal及despawn cleanup只以canonical link
  决策，删除nonzero producer后action/motion/HP/MP/RNG/lifecycle不变。
- shared kind5 valid reciprocal/invalid reciprocal、attacking row、OID213 type3 owner；raw duplicate不可调用，
  `GrabbedBy`不能重新成为gate。
- producer包不改snapshot schema；carrier包若获批，覆盖schema mismatch、snapshot capture/restore、
  ECS layout/fingerprint、slot reset与4096次warm copy/hash零分配。
- Unity compile、focused、B5/B6/NTSD28 regression、BattleRuntimeSelfCheck、formal OPoint-linked OID213与
  OID120 pickup/release Play、same-seed/tick trace。无这些runtime证据时最多`RUNTIME_PENDING`。

## 不变量 / 阻塞

- 不改canonical relation数值、kind2/5规则、holder copy、owner/+0x2F8/+0x3F8、candidate顺序、
  WPoint、RNG、content/Scene/Prefab/importer、Authority或ordered shutdown。
- 当前B6 production stack仍缺Unity Test Runner/SelfCheck/Play绿灯；本轮只登记owner与后继包，
  不叠加C# diff。

## 回滚

仅移除本治理记录与状态摘要；没有代码、content、Scene或Authority回滚。
