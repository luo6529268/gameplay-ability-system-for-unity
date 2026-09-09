# Task Contract — NTSD28-B0-OWNER-SLOT-PRODUCTION-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / FIELD_BINDING_RETAINED / FORMAL_SELF_OWNER_MISSING / OPOINT_OWNER_PROPAGATION_MISSING / F8_OWNER99_MISSING / TARGET_MULTIPLEXING_CONFLICT / FIVE_ROUTES_DEFINED / B2_RUNTIME_BLOCKED / PRODUCTION_HELD`
> 来源：`NTSD28-B2-AI-OWNER-SLOT-KILLCOUNT-CORRECTION-001`实施后producer复核

## 目标与边界

在已验证`identity.ownerSlot -> NTSDEntityRuntime.OwnerSlotIndex`字段绑定基础上，穷尽正式direct spawn、
stage、OPoint、native object-AI、weapon-piece、special clone、F8、hit relation及pool reset的producer，确认
Unity是否在相同生命周期边界写入Authority `Entity28+0x354 owner_slot`。

本Task只修改治理文档，不修改C#、content、Scene、Prefab、ProjectSettings、Server或Authority。

## 结论摘要

既有`NTSD28-B0-OWNER-SLOT-BINDING-001`只证明raw exporter字段映射与trace contract，未证明production
初始化。当前Unity有127行`OwnerSlotIndex|OwnerEntityIndex`引用：59行生产代码/18文件，68行test/editor/14文件。
Authority正式session与spawn链表明，Unity至少缺三类producer，而且legacy generic hit_Fa把同一carrier当
`+0x3F8` target cache；在解除该冲突前不能直接全局补owner。

## Authority producer矩阵

| 路径 | Authority owner写入 |
|---|---|
| local/selected combatant | `game_session.cpp` `request.owner_slot = combatant.slot` |
| story/stage runtime row | `game_session.cpp` `request.owner_slot = physical slot` |
| ordinary OPoint | `ObjectSpawnPlanner28::plan()`写`intent.owner_slot=parent.owner_slot`，materialize原样写child |
| native object-AI hit_Fa5/6 | child `owner_slot=source.owner_slot` |
| weapon-piece fragment | **2026-09-08更正：** DAT `<weapon_piece>` child继承`source.owner_slot`；前置built-in OID999 fragment沿用默认`-1`。 |
| state9996 five-clone | `SpawnRequest28`默认`-1`，没有source owner传播 |
| F8 drop | `NativeFunctionKeyDropSpawn28.owner_slot=99` |
| type3/kind relation hits | 按分支写attacker physical slot或source `owner_slot`；已有B5 exact writer证据 |

这些核心/session文件均在正式playable closure。`-1`只可用于对应native默认/isolated边界，不能把正式direct
participant的self owner或F8 99归一成同一sentinel。

## Unity实际差异

### 1. Direct participant / stage缺self owner

- `AppManager`和`BattleTestBootstrap`创建primary character后不写OwnerSlot；`LF2Character` reset/init默认`-1`。
- `SimulationStageWaveModule`在factory/direct创建并已确认`requiredRuntimeSlot`后，仍显式写
  `OwnerEntityIndex=-1`（ordinary stage与results-reserve各一处）。
- `SimulationRegistryModule`分配slot、refresh并bind stores，但不会把standalone entity的owner设为self。

因此formal direct character的Unity raw `OwnerSlotIndex`保持-1，而Authority为physical self slot。B2 AI虽已改读
正确carrier，formal direct characters仍不会触发`owner_slot>=0` guard，故B2不能VERIFIED。

### 2. Ordinary OPoint缺parent owner传播

两个factory的`PostInitLiving()`传播Team/RelationTeam、HolderCopy、OwnerId和legacy KillCount，但不写
`OwnerEntityIndex=parent.OwnerSlotIndex`。`OPointCreateTask.ownerEntityIndex`默认-1，只在部分legacy hit_Fa
特殊路径被显式赋值；普通DAT OPoint不赋值。Authority所有ordinary OPoint intent均传播parent exact owner，
与kind、child type或`+0x2F4`无关。

### 3. F8错误保留-1

`BattleRandomWeaponDropModule`创建kind0/null-parent task后未写owner，并在post-init只写`KillCount=-1`；
最终`OwnerSlotIndex=-1`。Authority F8固定写99。这与用户保留的当前随机掉武器路径相交，必须修exact owner，
但不能改变用户批准的候选/位置/RNG实现。

### 4. `OwnerSlotIndex`又被当作`+0x3F8` target cache

generic `LF2Entity.ResolveFrameLogicTargetByHitFa()`对hit_Fa4及common target scan读写`OwnerEntityIndex`；
hit_Fa5/6/8/9/13 task又把chosen target写入`ownerEntityIndex`。Authority这些target语义属于
`object_ai_target_slot_3f8`，不是`owner_slot`。该问题已由
`NTSD28-B6-OBJECT-AI-TARGET-3F8-OWNER-AUDIT-001`确认current可达；补self/parent owner前必须先迁移target，
否则owner初值会被误当有效target并改变扫描分支。

### 5. 已有正确/独立路径

- B5 type3/kind writers对`OwnerSlotIndex`的exact mutation保留。
- state9996 late clone写-1与Authority默认一致，不得随OPoint传播修改。
- pool reset写-1是未分配状态，不是formal spawn最终值；应保留reset，再在slot/来源明确时写producer。
- snapshot/canonical copy/ECS/checksum/parity已覆盖OwnerSlot；本轮无需新carrier或schema。

## 后继路由与顺序

1. `NTSD28-B0-OBJECT-AI-TARGET-3F8-DECONFLICTION-001`：把已有B6三包提前为B0 prerequisite，先让
   generic/common/hit_Fa child target全部使用`PickerStableId`对应的`+0x3F8`，OwnerSlot停止承载target。
2. `NTSD28-B0-DIRECT-ENTITY-SELF-OWNER-PRODUCTION-001`：primary与ordinary stage实体在runtime slot确定后、
   store/snapshot消费前写self slot；results-reserve按正式对应路径单列断言，不借HUD排除跳过逻辑字段。
3. `NTSD28-B0-F8-OWNER99-PRODUCTION-001`：只写owner 99并覆盖slot50/high；候选、RNG、位置与用户保留
   random-drop路径不变。
4. `NTSD28-B0-OPOINT-OWNER-PROPAGATION-PRODUCTION-001`：ordinary DAT OPoint按parent literal owner传播，
   并回归已闭合的native object-AI hit_Fa5/6 owner/target分离；state9996与built-in OID999 fragment明确维持-1。
   DAT `<weapon_piece>`完整行为及owner归B7；hit_Fa8/9/13仍须独立权威闭环，不能在本包猜测改写。
5. `NTSD28-B0-OWNER-SLOT-PRODUCTION-EXIT-AUDIT-001`：同seed/input/tick raw trace覆盖self、two-hop OPoint、
   F8 99、state9996 -1、type3 mutation与slot reuse，之后才能恢复B2 runtime验收和后续B3/B4/B5包。

## 验收矩阵

- direct：primary slot0/9/10/19、stage slot20/high、pool reuse；owner==self physical slot。
- OPoint：parent self/root/nonself，两跳child，kind1/2、type0/non-type0；child owner恒等parent owner而非parent slot。
- object-AI：`owner_slot != target_3f8` sentinel，hit_Fa4/5/6/common/child路径分别验证。
- F8：owner99、current random-drop候选/RNG/position checksum不变。
- state9996：5 children owner仍-1；不能被通用parent propagation覆盖。
- actual/HitPlan/ECS/raw/checksum/parity、compile、focused、SelfCheck、Play与C++/Unity first-difference。

## 不变量与回滚

- 不改Authority、slot容量、candidate/RNG/pass order、damage、content、Scene/Prefab、shutdown及用户例外。
- 本轮治理回滚只移除本Task/Change及摘要；production包须各自独立Task/Change与可回滚最小diff。

## 2026-09-08 F8 route 3 前实施补充审计

### 已观察事实：Authority post-tick owner=99 链

- 已重新核对正式EXE SHA-256仍为
  `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`；
  `source/README_SOURCE.md`仍声明当前源码快照对应该发行EXE。
- `ntsd28_playable/src/game_session.cpp::GameSession28::step()`先执行
  `tick_driver_.step(...)`，随后在post-entity function-key tail读取并清除共享
  `pending_object_command`；值为`drop_objects`时调用`consume_native_f8_drop()`。
- `ntsd28_playable/include/ntsd28_playable/game_session.h::NativeFunctionKeyDropSpawn28`
  将`owner_slot`默认初始化为精确`99`。`consume_native_f8_drop()`再把
  `trace.owner_slot`原样写入`SpawnRequest28.owner_slot`并调用`world_->spawn_at(...)`。
- 同一函数按registry index排序候选，从physical slot 50开始找首个free slot，并在每个实际候选
  materialize前依次消费四次同步RNG（D1～D4）。这些事实只用于界定owner字段写入边界；本B0包
  不迁移或改变候选、模式gate、RNG、位置、HP、pass placement或native的200项stack安全偏差。

### 已观察事实：Unity正式F8 effect尚未接到现有Mode2生成器

- `NTSD28NativeFunctionKeySessionState`已让正式F8 dispatch写入
  `PendingObjectCommand=DropObjects`，但生产代码没有调用
  `ConsumePendingObjectCommand()`；当前调用者只有Editor tests。
- `SimulationTickDriver`现有`SetMode2Request(1)`生产写入只来自已注明为legacy diagnostic的
  `BattleFunctionKeyInputLatch`。`BattleRandomWeaponDropModule.RunMode2Tail()`在
  `Mode2Request==1`时进入`SpawnMode2RandomWeapons()`，后者创建null-parent task但未写
  `ownerEntityIndex`，因此经当前factory materialize后的owner仍为`-1`。
- `RunNormalDrop()`是独立的每tick当前随机掉落路径，不是上述F8 post-tick effect。用户明确保留的
  当前random-drop候选/RNG/位置行为不得因owner99包被改写，也不得把normal-drop实体误标为F8 owner。

### 路由裁决

- `NTSD28-B0-F8-OWNER99-PRODUCTION-001`在route 2 focused gate通过后，只允许给
  `SpawnMode2RandomWeapons()`的生成task写精确owner `99`，并验证首个free slot 50与占用前缀后的
  high slot在first raw snapshot中均为99；同时证明normal-drop路径与既有RNG/位置结果未变化。
- 该包只闭合“既有F8 effect materializer的owner字段”。物理F8从
  `FunctionKeys.PendingObjectCommand`到post-tick effect的生产consumer仍归既有B8/function-key effect
  路线；未接线前不得把owner99 focused结果表述为正式物理F8完整对齐。
- state9996默认`-1`、ordinary OPoint parent-owner传播与B0 exit joint trace继续保持独立后继。

### 2026-09-08 route 3 materializer与测试预检补充

- `SpawnMode2RandomWeapons()`当前先只检查dynamic range仍有free slot，再让同步factory以
  `requiredRuntimeSlot=-1`走lowest-free allocation；单次materialize期间没有并发structural mutation，
  因而slot50与occupied-prefix后的high slot仍由既有分配器确定。owner99包不得顺手改成required-slot
  预选或改写allocation算法。
- owner唯一允许落在mode2循环内、`factory.CreateObjectImmediate(spawnTask)`之前的
  `spawnTask.ownerEntityIndex=99`。`RunNormalDrop()`另有自己的task构造且必须继续保持默认owner `-1`；
  不能抽成两个路径共享的“random weapon owner”。
- route 2 initializer进入Unity程序集后，logic-only与presentation factory都会在首次`Register`前消费
  task owner；mode2尾部不需要再写第二遍owner，也不应增加post-registration fix-up。
- focused应以production `SetMode2Request(1) -> Mode2RandomWeaponDropTailAll()`驱动logic-only world，
  分别覆盖首个free slot50和occupied prefix后的high slot，并检查claimed entity runtime owner为99、
  独立raw-slot backing仍为`-1`。
  同一fixture还应证明既有candidate次序、四次position RNG、frame/position以及normal-drop owner/RNG
  不因该单字段写入而改变；物理`PendingObjectCommand`消费仍不属于本包。

## 2026-09-08 route 4 OPoint/fragment 前实施更正审计

### Authority精确分类

- `object_spawning.cpp::ObjectSpawnPlanner28::plan_frame()`对每个有效frame OPoint都写
  `intent.owner_slot = parent.owner_slot`；`battle_world.cpp::spawn_from_opoint_intents()`再把该值
  原样写入`SpawnRequest28.owner_slot`。这与kind、child type、multi-spawn ordinal及parent physical slot
  无关；kind2的`linked_parent_slot=parent physical slot`是另一个字段。
- `native_ai.cpp`已闭合的hit_Fa5/6 child均在source尚有效时缓存`source->owner_slot`并写入child request，
  同时把chosen target另写`object_ai_target_slot_3f8`。Unity对应5/6 task已由route 1改为
  `ownerEntityIndex=OwnerEntityIndex`与`trackedTargetSlot=target`；route 2 initializer实际进入Unity程序集后，
  该值才能在first registration snapshot前可见。
- `battle_world.cpp::materialize_weapon_piece_fragments()`必须拆成两族：DAT `<weapon_piece>` fragments
  明确写`request.owner_slot=source_owner`；其前置built-in OID999 fragment family没有写owner/group，沿用
  `SpawnRequest28`默认`owner_slot=-1`与`battle_group=0`。正式
  `weapon_piece_corpus_tests.cpp`分别断言built-in为`-1/0`、DAT fragments继承测试source的`3/7`。
- state9996 five-clone同样不写owner，沿用`SpawnRequest28.owner_slot=-1`。因此built-in broken fragments
  与state9996都不是ordinary parent-owner传播对象。

### Unity production入口与缺口

- 当前正式late OPoint链是
  `BattleLateEntityLifecycleModule -> BattleStructuralWriter.ProcessLateOpointSegment() ->
  BattleLogicObjectPointRuntime.ProcessOneLateOpoint()`。该producer已经设置parent、required slot与位置，
  但没有把`spawner.OwnerEntityIndex`写入task；这就是ordinary OPoint first-snapshot缺口。
- `LF2ObjectPointModule`的single/multiple enqueue同样不写owner；当前项目源码扫描没有找到其
  `ProcessFrame()`生产调用者。它只能作为兼容/备用入口单独裁决，不能代替world-owned structural链。
  两个multi materializer已能把`OPointCreateMultipleTask.ownerEntityIndex`复制到single task。
- Unity built-in `SpawnBrokenWeaponFragments()`使用null parent、默认owner，当前与Authority `-1`一致；
  `BattleLateEntityLifecycleModule`的state9996 task也是null parent，并在materialize后再次显式保持owner `-1`。
- 当前Unity自编写代码中没有DAT `<weapon_piece>`数据模型、parser或materializer；除
  `NTSD28BattlePassOrder.SlotWeaponPieceFragments`骨架及其顺序测试外没有行为实现。因此B0不能只补owner
  就宣称weapon-piece已闭合，该完整行为与source-owner写入必须归B7 OPoint/Spawn/Lifecycle路线。
- hit_Fa8/9/13仍有把chosen target写入`ownerEntityIndex`的legacy路径，而当前playable源码对相关分支仍为
  fail-closed/独立未闭合边界；不得在ordinary OPoint包内猜测改成source owner，也不得把route 1的5/6证据
  扩大到8/9/13。

### 更正后的route 4边界

1. `NTSD28-B0-OPOINT-OWNER-PROPAGATION-PRODUCTION-001`只闭合world-owned ordinary frame OPoint
   single/multi task的`parent.owner -> child.owner`，必要时对仍受支持的`LF2ObjectPointModule`备用入口
   使用同一显式producer合同；不在factory中按`parent!=null`进行全局猜测。
2. focused至少覆盖parent self/root/nonself、two-hop、kind1/kind2、type0/non-type0、single/multi及
   first raw snapshot，并证明child owner等于parent literal owner而不是parent physical slot。
3. built-in OID999 fragment与state9996继续断言`-1`；F8继续断言99；hit_Fa5/6继续断言
   `owner != target_3f8`。hit_Fa8/9/13保持独立未闭合项。
4. DAT `<weapon_piece>`完整materializer及其source owner进入B7，不再列为本B0 production包已实现验收项。
