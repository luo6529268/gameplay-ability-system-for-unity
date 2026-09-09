# Task Contract — NTSD28-B6-OBJECT-AI-TARGET-3F8-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / EXISTING_CARRIER_RECLASSIFIED / OWNER_354_CORRUPTION_CURRENT_REACHABLE / THREE_PACKAGE_SPLIT_DEFINED / PRODUCTION_HELD`
> 依赖：`NTSD28-B6-LEGACY-WEAPON-STATE-OWNER-AUDIT-001 / VERIFIED`、`NTSD28-B6-WPOINT-DVX-EXCLUDED-GROUP-CARRIER-AUDIT-001 / VERIFIED`

## 目标

冻结 current Authority `Entity28+0x3F8 object_ai_target_slot` 的默认值、producer/consumer、与+0x354
owner及+0x2F8 excluded-group source的独立性；审计Unity `PickerStableId`、`OwnerEntityIndex`和未接线
`OPointCreateTask.trackedTargetSlot`，定义不新增第二份真值的最小生产修复。

本 Task 只修改治理记录，不修改 C#、Config、Scene、Prefab、资源、ProjectSettings、Server 或 Authority。

## Authority 字段合同

### 三个独立物理槽字段

| Authority field | 默认 | owner / 用途 |
|---|---:|---|
| `owner_slot` / +0x354 | -1；普通spawn按root owner传播 | damage/KO/score attribution、kind2 relation、type3 transfer等所有权链。 |
| `object_ai_excluded_group_source_slot_2f8` / +0x2F8 | -1 | 只由held type1/4/6 DVX写holder physical slot；common target scan据此排除holder group。 |
| `object_ai_target_slot_3f8` / +0x3F8 | -1 | non-character hit_Fa cached/preassigned target；不是picker、owner或general spawner。 |

三个字段可以同时不同，任何互相复用都会改变目标选择、归属、slot reuse与checksum。

### +0x3F8 producer / consumer

- `spawn_transient()`初始化为-1；普通OPoint不把parent/owner自动写入+0x3F8。
- common hit_Fa 1/2/3/12/14先验证cached +0x3F8；失效时升序扫描type0目标，以strict `< best`
  保留最低physical slot并把结果写回+0x3F8。扫描没有命中时不会先清除非-1 stale值；仅+0x3F8仍为-1时
  才走HP=0 no-target分支。
- hit_Fa4/7跳过common cached validation/nearest scan，直接消费producer预先写入的+0x3F8。
- hit_Fa5生成每个living same-group character对应的OID219 child：child `owner_slot`继承source owner，
  +0x3F8写friendly target slot。
- hit_Fa6生成至多7个opposing-character OID220 child：同样保留source owner并把target写+0x3F8。
- 当前Authority tests的正式OID219 hit_Fa5→4见证明确断言child owner=source owner7而+0x3F8=target0；
  release OID875 hit_Fa3→7见证则证明common prelude产生target cache，后继preassigned branch只消费该cache。

## Unity closure 与首差

### 已有可复用carrier

- `NTSDEntityRuntime.PickerStableId`默认-1，进入canonical copy、ECS Links shadow/fingerprint、entity snapshot、
  lockstep checksum及parity JSON `pickerIdx`；`LF2WeaponBase.PickerStableId`只是它的属性别名。
- 当前唯一非reset生产writer是specialized `LF2WeaponFrameLogicResolver`的hit_Fa12 nearest scan；其cached
  target读取也使用该字段。OID124实际证明底层字段已经按+0x3F8工作，而不是按“拾取者”工作。
- held DVX writer已经不再写`PickerStableId`；pickup/held relation使用Holder/Target/Link字段。
  因此无需新增另一个持久化int，也不能把+0x3F8改绑到Owner或Spawner。

### generic错误复用+0x354

`LF2Entity.ResolveFrameLogicTargetByHitFa()`当前：

- hit_Fa4直接把`OwnerEntityIndex`当preassigned target；
- 其余common行为把`OwnerEntityIndex`当cached target，并在nearest scan后写回best slot；
- hit_Fa11再以`OwnerEntityIndex<0`判断no-target；
- 这使任何generic non-character scan都覆盖exact owner/credit chain，而`PickerStableId`仍为-1。

`RunHitFa5FrameLogic()`和`RunHitFa6Or9FrameLogic(6)`又把chosen target写到
`OPointCreateTask.ownerEntityIndex`。Factory随后写child `OwnerSlotIndex=target`；已存在的
`trackedTargetSlot`在Task reset外没有producer、copy、factory consumer，是尚未接线的明确seam。

generic与specialized target scan还共享两个需在producer integration中同时修正的字段内算法差异：

- Unity在scan结束时无条件把best初始化-1写回（generic写Owner、specialized写Picker），会清除Authority
  明确保留的invalid non-sentinel stale +0x3F8；随后又把“stale inactive”和“原本sentinel -1”合并成null，
  可能错误进入HP=0分支。
- Authority只在candidate state14且旧cache非-1时排除state14；非state14 candidate无论旧cache值都必须
  通过`abs(render_phase)<=2`。Unity把`state14 || abs(HitStun)>2`放在同一
  `currentTargetSlot != -1` gate下，旧cache为-1时会错误接受非state14且render phase超界的target。

Unity对hit_Fa4/7还提供raw inactive-slot runtime适配，而standalone C++ model在storage已释放时报告
`unresolved_target_context`；正式EXE会继续跟随物理slot内存。该边界必须保留为具名EXE joint-trace项，不能仅凭
standalone model删除raw适配，也不能让它继续掩盖owner/+0x3F8字段混用。

当前hit_Fa8/9/13等Unity旧spawn算法与Authority仍有独立unsupported/recovery边界；本Task不借carrier修复
预判它们，只处理Authority已闭合的common、4/5/6/7 target字段。

## Current reachability

Direction-B indexed projection中，正式target-cache相关frames为：

| 路径 | 当前帧数 | Unity owner后果 |
|---|---:|---|
| generic common 1/3/12/14（type3/type5） | 206 | 首次cache/scan读取并覆盖+0x354；+0x3F8未写。 |
| specialized OID124 type4 hit_Fa12 | 16 | 已正确使用现有`PickerStableId`底层carrier；作为正向对照。 |
| generic preassigned hit_Fa4/7 | 4 / 22 | 读取错误owner；追踪可偶然工作但归属已污染。 |
| generic hit_Fa5 source | 1 | OID219 action51生成child时把target写成owner。 |

正式 current witnesses：

- OID207 action62..64为hit_Fa3循环；给source owner sentinel7、敌方slot1时，Authority写+0x3F8=1并保留
  owner7，Unity写OwnerSlot=1且target carrier仍-1。checksum与后续attribution同tick分叉。
- OID219 action51/hit_Fa5是current和release共同语料。current weapon6 actions62/71通过OPoint action50生成
  OID219，50→51后正式fanout；Authority child保留source owner并写friendly target，Unity把friendly target
  写进owner。child action0..3随即以hit_Fa4消费这个错误字段。
- current有9条可闭合的hit_Fa3→7 next链，来自OID210、221、228、400、421、433；common 3应生产
  +0x3F8，后继7应保留owner并消费相同target。现实现只因两端都误用owner而表面连续。
- no-target也会分叉：Authority保持owner不变并只在target cache为-1时kill；Unity nearest scan把owner写-1。

## 三包拆分

### 1. Carrier semantic reclassification

`NTSD28-B6-OBJECT-AI-TARGET-3F8-CARRIER-001`

- 在`NTSDEntityRuntime`/`LF2Entity`提供canonical `ObjectAiTargetSlot3F8`语义入口，复用现有
  `PickerStableId`底层storage；旧名称只作临时compat alias并禁止新production caller；
- default/reset/canonical copy/ECS/snapshot/checksum现有int位置不变，不增加重复field、不升级persistent schema；
- parity key `pickerIdx`与B0 raw 49-field contract是否同包改名/扩展，先按trace schema规则建立versioned
  projection，不静默改变外部JSON；
- focused证明alias同一storage、slot0/-1/high slot、copy/restore/checksum、4096 warm 0 B。

### 2. Common/child writer integration

`NTSD28-B6-OBJECT-AI-TARGET-3F8-PRODUCER-INTEGRATION-001`

- generic common scan与hit_Fa4/7/11全部改读写canonical +0x3F8，绝不写OwnerSlot；
- hit_Fa5/6 task分别写`ownerEntityIndex=source owner`与`trackedTargetSlot=selected character`；factory/logic
  factory在注册完成后的固定点消费trackedTarget到+0x3F8；Task copy/reset覆盖该字段且不改变普通OPoint；
- specialized weapon hit_Fa12迁canonical入口但保持值与算法；
- common/specialized scan按Authority拆开state14与render-phase gate、在scan miss时保留non-sentinel stale cache，
  并区分sentinel no-target与inactive-stale outcome；raw inactive-slot适配只在EXE trace裁决后改变；
- 对current正式206 common、OID219 5→4、9条3→7链做actual/owner/target/checksum矩阵。

### 3. +0x2F8 consumer completion

既有`NTSD28-B6-OBJECT-AI-EXCLUDED-GROUP-CONSUMER-001`必须依赖前两包：common target scan同时读取新
+0x2F8排除group与canonical +0x3F8 cache。不得继续用Spawner作+0x2F8，也不得在接excluded-group时再次
重写target/owner逻辑。

## 验收矩阵

- OID207 action62、OID200/204/220/225/228/450 hit_Fa12、OID300 type5 hit_Fa14：cached valid/invalid、
  target state14/render phase/HP/team、equal-distance lower slot、best-distance初值10000；owner sentinel始终不变；
- OID219 current action50→51→fanout→action0 hit_Fa4：source owner7、多个friendly slots、slot0/high slot、
  child owner7与target各自正确，close-range action60/heal100及far chase；
- 9条current 3→7链跨frame advance保持target并保留owner；release OID875同链作为H后置见证；
- OID124 specialized 16-frame loop迁canonical name前后checksum/motion不变（除独立WeaponState retirement）；
- no-target、stale non-sentinel inactive slot、same-tick despawn/reuse必须按Authority unresolved/ABA边界测试；
- `OwnerSlotIndex`后续hit/KO/score attribution、+0x2F8、Spawner、held relation与RNG cursor均不变；
- compile、focused、B3-C02/B5/B6/NTSD28 regression、SelfCheck、formal Play与joint trace。缺runtime证据时最多
  `RUNTIME_PENDING`。

## 不变量 / 阻塞 / 回滚

- 不修改Authority、Config/Scene/Prefab/resource/importer、C02 placement、hit_Fa算法常量、未恢复的8/9/13
  special spawn、physics、RNG、relation/lifecycle或shutdown顺序。
- 不新增第二份+0x3F8 truth；不删除兼容field或改变persistent snapshot schema。
- 当前B6 runtime栈与Unity实例未清，三包均保持held，不叠加脚本修改。
- 本审计回滚仅删除新增治理记录与摘要；没有脚本、content、Scene或Authority回滚。
