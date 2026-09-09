# Task Contract — NTSD28-B6-LEGACY-TRACKER-RELATION-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / NO_AUTHORITY_TRACKER_LAYER / CURRENT_OPOINT_WRITERS_REACHABLE / LEGACY_READERS_DEFAULT_BYPASSED / THREE_PACKAGE_SPLIT_DEFINED / SCHEMA_DIRECTION_GATED / PRODUCTION_HELD`
> 依赖：`NTSD28-B6-OBJECT-AI-TARGET-3F8-OWNER-AUDIT-001 / VERIFIED`、`NTSD28-B6-ENTITY-LINK-LIFECYCLE-CLEANUP-OWNER-AUDIT-001 / VERIFIED`

## 目标

审计Unity `TrackerFlag`、managed `TrackerParent`及其OPoint kind2 producer、kind5/transform consumer、
snapshot/ECS传播，确认当前2.8 Authority是否存在第二套关系层，并拆分不影响canonical reciprocal link的退休顺序。

本Task只修改治理记录；不修改C#、Config、Scene、Prefab、资源、ProjectSettings、Server或Authority。

## Authority关系合同

- `BattleWorld28::spawn_from_opoint_intents()`对OPoint kind2只写：child `interaction_state=-1`、
  `linked_parent_slot=parent slot`；parent `interaction_state=1/101`、`linked_child_slot=child slot`；再次同步
  child battle group。`EntityState28`没有TrackerFlag或managed parent引用。
- native kind5 substitution只读取attacker negative relation、reciprocal linked parent、holder tick-action snapshot的
  first WPoint `attacking`与attacker definition的weapon-strength row；不比较stable id或额外flag。
- type3 effect3/OID213 ownership continuation同样在attacker relation为negative时沿`linked_parent_slot`取holder，
  再复制battle group/owner；不存在对象引用cache fallback。
- hit后的active-holder FrameDelay写入也属于reciprocal relation consumer，不需要第二套tracker identity。
- Authority despawn在slot释放前清理关系；因此stale managed reference不是保持行为所需的fallback，而是Unity当前
  lifecycle cleanup缺口可能掩盖出的附加状态。

## Unity production closure

### Producers

`LF2ObjectPointFactory.PostInitLiving()`和`BattleLogicEntityFactory`在每个OPoint kind2 materialization中额外写：

- parent `TrackerFlag=1`；
- child `TrackerFlag=-1`；
- child `TrackerParent=parent` managed引用。

随后同一代码才建立canonical LinkState/TargetSlot/HeldWeapon与HolderSlot关系。当前Direction-B有62条OPoint
kind2 records、39个source definitions、56个distinct source-target edges，因此这些extra writes current reachable。

### Consumers

- `LF2CharacterHitResolver`与`LF2CharacterDatHitResolver`各复制一套raw kind5旧逻辑：从victim
  `ResolveTrackerParentFromRuntime()`取parent，再要求`parent.TrackerFlag==attacker.StableId`，随后错误地从
  attacker frame WPoint取替代字段。
- production `BattleHitCandidateSequenceRunner`已在dispatch前调用
  `BruteForceSceneQuery.ResolveRuntimeItrForPair()`；它按attacker negative reciprocal relation、holder collision
  snapshot WPoint attacking与holder ITR完成当前Authority kind5 replacement。成功时传给两个Hit resolver的是kind0，
  所以上述raw-kind5 duplicate在默认正式路径被绕过。
- `LF2SpecialAttack`旧OID213 effect3分支调用`ResolveTrackerParentFromRuntime()`，但该方法先按
  HolderStableId/negative LinkState/reverse TargetSlot解析active holder，只有active query失败时才尝试managed cache。
  当前B5 canonical type3 transaction已有自己的exact linked-parent owner；旧fallback不得成为第二真值。
- `LF2HitResolveRuntimeData.ApplyActiveHolderFrameDelay()`与shared kind5/group consumers均已使用canonical relation，
  不读TrackerFlag/TrackerParent。

### Propagation / determinism

- `TrackerFlag`进入`NTSDEntityRuntime` reset/canonical copy、ECS Links shadow与ECS fingerprint，但没有进入
  lockstep checksum或parity JSON；在仍有direct gameplay reader时这是determinism blind spot。
- `TrackerParent`是managed shell字段，`BattleWorldEntityBaseShellSnapshot`把它序列化成generation-aware handle，
  restore又要求该handle可解析；base-shell schema当前1，full snapshot schema19。
- 两者均不是Authority字段。当前快照保存同一关系两遍：一次在canonical runtime link字段，一次在managed handle。

## Current reachability分类

- extra producer：62条OPoint kind2全部可达，立即改变TrackerFlag/ECS fingerprint与base-shell snapshot handle。
- current indexed ITR kind5共有353条；但13个OPoint-kind2 target OIDs
  `{101,120,121,122,123,150,213,422,434,437,444,447,500}`中kind5 ITR为0。因此“由当前OPoint kind2建立
  relation后再以同child kind5攻击”的formal current交集为0；旧raw reader current dormant，不代表规则可保留。
- raw/direct public `Hit(kind5)`或alternate content仍可进入旧duplicate；这些入口必须source-guard，不得因默认路径
  绕过而留下可重新激活的错误规则。
- OID213是current OPoint-kind2 target，managed parent cache可进入旧type3 continuation；canonical B5 writer和relation
  link才是正式owner，测试必须证明移除cache后结果不变。

## 三个后继包

### 1. Dynamic producer retirement

`NTSD28-B6-LEGACY-TRACKER-RELATION-PRODUCER-RETIREMENT-001`

- 从两个factory的OPoint kind2 tail删除TrackerFlag与TrackerParent writes；
- 保留并验证canonical parent/child LinkState、TargetSlot、HeldWeapon、HolderSlot、battle group与TrackerFlag sentinel0；
- 暂留fields/copy/ECS/snapshot位置，不改变persistent schema；
- current 62 records/56 edges、type1/2/3/4/6、slot0/high/reuse与OPoint-only OID52/58 source纳入matrix。

### 2. Legacy consumer retirement

`NTSD28-B6-LEGACY-TRACKER-RELATION-CONSUMER-RETIREMENT-001`

- 删除两个Character hit resolver的raw-kind5 duplicate；所有formal kind5只能由shared runtime ITR resolver生成；
- OID213旧fallback改用canonical linked-parent helper，或在B5 canonical writer已完全覆盖后删除重复tail；
- `ResolveTrackerParentFromRuntime()`移除managed cache fallback并改名为exact relation helper；
- 依赖entity-link lifecycle cleanup production/runtime绿灯，避免用缓存继续掩盖stale/ABA link；
- source guard证明production不再读取TrackerFlag或TrackerParent对象引用。

### 3. Carrier/schema disposition

`NTSD28-B6-LEGACY-TRACKER-RELATION-CARRIER-DISPOSITION-001 / USER_DIRECTION_REQUIRED`

- 删除`TrackerFlag` field/copy/reset/ECS array/fingerprint与`TrackerParent` field/base-shell handle/restore failure分支；
- base-shell snapshot `1→2`、entity runtime snapshot `12→13`、full snapshot `19→20`；TrackerFlag本来不在
  lockstep checksum，不能伪称只删field就完成checksum migration；
- 与ReleaseTick/WeaponState/GrabbedBy/HolderCopySlot carrier disposition协调为一个联合full/entity/checksum migration，
  避免多个包各自产生13/20/23伪中间版本；`GrabbedBy`本身未进checksum，不得将
  22→23错记为由它单独触发；
- snapshot/recovery兼容和旧snapshot拒绝/迁移策略需要用户方向，普通B6退休包无权决定。

## 验收矩阵

- current OPoint kind2 62/56全矩阵：producer前后canonical links/group/action/position/RNG相同，Tracker extras恒0/null；
- formal/synthetic kind5：valid reciprocal、negative relation、holder tick snapshot、attacking1..9、missing zero row、
  target==holder、invalid reciprocal；shared actual/HitPlan一致且legacy duplicate不可调用；
- OID213 current link、owner/group transfer与B5 type3 transaction不依赖managed cache；
- lifecycle：holder/child先后despawn、same-tick slot reuse、snapshot capture/restore、invalid relation，cache不能复活旧对象；
- carrier包若获批，覆盖schema mismatch、snapshot history/session、ECS fingerprint、4096 warm copy/hash 0 B；
- compile、focused、B5/B6/NTSD28 regression、SelfCheck、OPoint-linked OID213 Play及joint trace。无runtime证据时最多
  `RUNTIME_PENDING`。

## 不变量 / 回滚

- 不修改Authority、Config/Scene/Prefab/resource/importer、kind5 canonical算法、B5 type3结果、relation字段、RNG、
  target+0x3F8、excluded-source+0x2F8或shutdown顺序。
- producer/consumer退休不删除carrier、不升级schema；carrier disposition未获用户方向前不实施。
- 本审计回滚仅删除治理记录与摘要；没有脚本、content、Scene或Authority回滚。
