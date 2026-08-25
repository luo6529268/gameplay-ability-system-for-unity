# R1-SOURCE-004 — Unity candidate / collision / hit crosswalk 与差异

> 状态：COMPLETED（静态 source inventory；runtime / joint fixture 待后续阶段）。  
> C++ authority：J:\QQFile\NTSD2.4\ntsd_release 的 release live source。  
> Unity evidence：当前工作树 source。没有 C++ runtime trace、Unity 编译、self-check 或 Play Mode 结果。

## 1. Unity checkpoint crosswalk

| C++ checkpoint | Unity source | 当前静态映射 |
|---|---|---|
| C00 candidate carrier clear | BruteForceSceneQuery.cs:2060-2077；LF2Entity.cs:4830-4833 | 每次 collect 前清 HitConfirm2、HitCandidateCount、nearest/kind1 distance；等价目标是上一 tick C++ tail 后的 clean carrier。 |
| C01 prev_frame2 freeze | SimulationWorld.Passes.partial.cs:1269-1308；LF2Entity.cs:4479-4506 | CaptureCollisionFrameSnapshotsAll 在 candidate 前为 active slot capture collision frame / Prev2。 |
| C02 vrest pair decrement | SimulationWorld.Passes.partial.cs:1316-1350；RuntimeRestStore.cs:1098-1209 | Unity 将 C++ pair scan 的 vrest decrement 拆成独立 active roster pass，按 victim row 只递减另一个 marked active slot 的 vrest。 |
| C03-C05 collect | BruteForceSceneQuery.cs:1497-1691,2079-2119,5263-5659,6282-6889 | brute force 保持 runtime-slot i/j 和双方向；formal broadphase 生成 pair 后按 authority ordinal 排序并仍双向 narrow phase；candidate store/list 均记录 target slot、itr index、body X 和 runtime ITR metadata。 |
| C06 Loop1 / Loop2 consume | SimulationWorld.Passes.partial.cs:1943-2201；LF2Entity.cs:2493-2498；BattleHitCandidateSequenceRunner.cs:46-246 | Character pass 与 Object pass 分离，按当前 DAT type==0 / >0 选择；fallback 与 data-oriented 参与者最终都调用同一 TryConsumeCaptured，从被冻结 range 按 ordinal 消费。 |
| C07 consumer runtime ITR | BattleHitCandidateSequenceRunner.cs:81-233；BruteForceSceneQuery.cs:5908-6059 | kind5/4/9 runtime replacement 在 candidate consume 时发生；target slot 以当前 occupant resolve，符合 C++ slot-based consume 的设计。 |
| C08 writer | BattleDamageWriter.cs；BattleInteractionWriter.cs:14-197；LF2CharacterDatHitResolver.cs | 伤害、grab、pickup、weapon/special branches由 typed writer/resolver 分派；不能只因分成多个 Unity 文件判为差异。 |

## 2. 已确认静态差异

### D-COL-001 — hit_confirm2 的整 attacker abort 未进入 Unity candidate runner

- C++ authority：collision.cpp:65。若 attacker.hit_confirm2 非零且当前 candidate target 是 character DAT，C++ 立即 next_attacker，停止该 attacker 剩余 candidate。
- Unity current code：BattleHitCandidateSequenceRunner.cs:60-76,81-246 只逐 candidate 处理；全局搜索当前 production scripts 中 HitConfirm2 仅有写入、snapshot/checksum 和 tail/reset，未找到在 candidate consume 前对 attacker HitConfirm2 的同等 read/abort。
- 静态差异：T11 中一个 character 命中 weapon/special 并把该 object 的 HitConfirm2 设为 1 后，C++ T13 可能阻止该 object 对已冻结 character candidate 的消费；Unity ObjectInteraction 当前仍可继续 dispatch。
- 最小 fixture：slot A（character）在 Loop1 命中 slot B（object）并设置 B.HitConfirm2=1；B 在 C01-C05 已对 slot C（character）拥有 candidate。比较 C++ Loop2 与 Unity ObjectInteraction 是否消费 B→C。
- 状态：待处理（静态确认）。后续修复应是 candidate runner 的 C++ gate 语义，不得仅在单个 weapon 类里补特殊判断。

### D-COL-002 — caught cpoint consumer guard 没有在 Unity 的统一 dispatch 边界执行

- C++ authority：collision.cpp:69-79。target prev_frame2 cpoint.kind==2、catcher/caught slot 关系匹配且 catcher prev2 cpoint.hurtable==0 时，当前 candidate 在 switch 前被跳过。
- Unity current code：BruteForceSceneQuery.cs:6255-6412 已实现 IsReleaseConsumerPairBlocked / TargetBeingCaughtPairBlocked helper；但 BattleHitCandidateSequenceRunner.cs:124-158 对 kind1/kind3/pickup 直接 Dispatch，且 142-147 对 kind6 直接写 target.HitConfirmCounter，没有统一调用该 helper。BattleInteractionWriter.cs:14-166 的 grab/pickup writer 也不持有该 guard。
- 静态差异：C++ 的通用 consume gate 只在 Unity 的部分 Hit / weapon compatibility paths 中出现，统一 candidate runner 对 grab、pickup、kind6 和 character-DAT damage 分派没有同一必经 gate。
- 最小 fixture：target prev2 cpoint.kind=2；catcher.CaughtSlotIndex=attacker slot；catcher prev2 cpoint.hurtable=0；分别运行 kind1、kind2、kind3、kind6、kind7、kind0。验收是所有 C++ 会 skip 的 candidate 在 Unity 也不写 frame、relation、hit confirmation、damage 或 vrest。
- 状态：待处理（静态确认）。需要先在 R1-SOURCE-005 闭合 cpoint relation field mapping，再以单一 runner gate 修复。

### D-COL-003 — effect 21 的 consume-time current-state abort 缺失

- C++ authority：collision.cpp:188-194。runtime ITR kind=0、effect=21 且 target current state 为 18 或 19 时，C++ next_attacker，停止该 attacker 剩余 candidate。
- Unity current code：BruteForceSceneQuery.cs:6657-6682 只在 candidate collect 时根据 target previous frame 做 Kind0EffectAllowed；BattleHitCandidateSequenceRunner.cs:81-233、BattleDamageWriter.cs 和 LF2CharacterDatHitResolver.cs 中未出现 C++ 等价的 effect=21 + target current state 18/19 consume abort。
- 静态差异：collect-time previous-state filter 不能覆盖 consume-time current-state gate，尤其当更早 slot 的 consume 已改变 target current frame。
- 最小 fixture：候选冻结后让 target current state 在 consume 前为 18 或 19，attacker 具有 effect21 kind0 candidate 与后续 candidate。验收是 C++/Unity 都不 dispatch 当前 candidate，且不消费该 attacker 后续 candidate。
- 状态：待处理（静态确认）。修复位置必须保证终止范围是 entire attacker sequence，而不是只 skip current candidate。

### D-COL-004 — Unity-only pure transition smoke candidate gate

- C++ authority：collision_collect.cpp:104-120,242-360 的 pair/ITR gates 中没有 oid999、SpawnSemantic 或 pure-transition-smoke 全局排除。
- Unity current code：BruteForceSceneQuery.cs:6282-6300 在 candidate pair gate 调用 IsPureTransitionSmoke；其定义在 6414-6441，会根据 oid999、state3005、pic/next 和 body 做全局排除。
- 静态差异：Unity 多出一个 candidate collection gate。该 gate 是否会命中有 release ITR/Bdy 的实际 DAT 链目前没有 fixture / DAT reachability 证据；不能写成已复现 root cause。
- 最小 fixture：一个 active oid999 transition-like entity 分别作为 attacker/target，覆盖有/无 ITR、有/无 body、state3005、pic999/next1000。比较 candidate presence、vrest decrement、consume 与 lifecycle。
- 状态：待处理（静态确认；DAT 可达性待验）。

### D-COL-005 — kind1 对非 character target 的静态处理边界不同

- C++ authority：collision_collect.cpp:276-278 只对 kind3、kind8 写入 character target 的显式
  obj_type==0 gate；kind1 仍可在其余 pair / bdy / state / team filter 通过后被记录。
  collision.cpp:921-994 的 kind1 consume 随后直接以 common Entity fields 写 Vx、facing、
  catching/caught frame、位置和 caught/catcher relation，没有再检查 victim obj_type。
- Unity current code：BattleInteractionWriter.TryApplyGrab（14-28）把 kind1 和 kind3 一并
  要求 target 为 LF2ObjectType.Character。CharacterInteractionResolver（123-134）、
  CharacterDatInteractionResolver（43-54）和 SpecialAttack（398-412）的 kind1 分派均会
  进入该统一 gate；不满足时不会执行 C++ 的 direct relation / frame writes。
- 静态差异：C++ 把 kind1 与 character-only 的 kind3 区分处理，Unity 将两者合并为
  character-only。现有 DAT 是否让 kind1 真实命中 type1/type2/type3/type4 target 尚未证明，
  因此不能宣称已复现，但代码路径的可接受集合已不同。
- 最小 fixture：一名 kind1 attacker 分别对 type0、type1、type2、type3、type4 target 使用
  已通过 geometry / vrest / team filter 的 ITR；记录 candidate presence、consume return、
  frame、Vx、caught/catcher、duration 和 fall。kind3 同组作为 control，预期两侧均排除
  non-character。
- 状态：待处理（静态确认；DAT 可达性待验）。后续必须先确认 kind1 的实际 DAT reachability
  与 CPoint/held lifecycle，再决定是否扩展 Unity writer；不得简单地去掉所有 target type
  guard。

### D-HIT-001 — type3 special normal damage 丢失 vital/stat writes

- C++ authority：collision.cpp:561-585 对非 type6 target 进入 apply_hurt；hit.cpp:104-155 对该 standard path 写 victim HP、hp_max、combo_count_vic，以及满足 unk_344 条件时的 g_damage_stats。该函数不排除 obj_type==3；hit.cpp:206-488 再继续处理 type3 的 fall/motion/effect tail。
- Unity current code：BattleDamageWriter.ApplySpecialAttackDamage（322-369）在 kind0 分支只调用 ApplySpecialObjectHurtTail 和 ApplyKind0Type3Tail。ApplySpecialObjectHurtTail（525-658）处理 fall、frame、arest/vrest、sound 和 motion，但没有写 victim.Health.HP、Health.HPBound、ComboCountVic 或 world.DamageStats。
- 静态差异：type3 special attack 的 normal kind0 hit 在 C++ 会损失 HP 并参与相应统计；Unity 当前只更新命中反应，不扣 special 的 Health。因此紧接的 late entity death/lifecycle 也可能分叉。
- 最小 fixture：type3 victim 的 Health.HP/HPBound、ComboCountVic、Unk344 和 world.DamageStats 设为已知非边界值；用 kind0 injury 命中，比较四个字段及下一 late update 的 death path。
- 状态：待处理（静态确认）。后续需在 typed damage writer 中补全 C++ standard vital/stat contract，不能仅把 type3 HitConfirm2 或 frame 写入视为等价。

### D-HIT-002 — Kind10/11、Kind16 / normal weapon hit 的 raw frame 与 Unity helper 副作用不等价

- C++ authority：collision.cpp:1213-1217 对 character kind10/11 直接写 frame182；hit.cpp:720-723 直接写 kind16 victim frame200 后单独写 attacking=0；collision.cpp:589-631 对 type1/type2/type4/type6 weapon response 直接写 frame；这些分支均不经统一 frame transition helper。
- Unity current code：LF2CharacterDatHitResolver.ApplyFluteCharacterForce（793-803）用 ImmediateFrame 写 frame182；ApplyKind16（BattleDamageWriter.cs:171-174）用 ImmediateFrame 写 frame200；LF2Entity.ImmediateFrame（1196-1212）额外写 Frame.PN、AttackingCounter、sprite 和 FrameTransistor。ApplyWeaponDamage / ApplyKind0WeaponVictimTail 混用 SetFrameDirect / ImmediateFrame；LF2WeaponBase.SetFrameDirect（929-939）也会清 AttackingCounter / 同步 transistor。
- 静态差异：上述 C++ raw frame 写入与 Unity helper 的 frame-history、attacking、wait/transistor 副作用不是同一操作。D-MOV-002 是 landing 的同类问题；本项覆盖已确认的 kind10/11、kind16 和 normal weapon consume writer。
- 已核对但不预设为差异：BattleInteractionWriter 的 kind1/kind3 使用 SetCpointRawFramePreserveWait，kind2 使用 DirectWriteRawFramePreserveWaitCounter，二者意图上保留 raw frame / wait。它们仍要由 R1-SOURCE-005 闭合 CPoint/held consumer，而不是在本项中误报为 helper 差异。
- 最小 fixture：kind10/11 character frame182、kind16 frame200 与 type1/type2/type4/type6 weapon hit frame 各一组，记录 frame、Prev/Prev2、wait、attacking、next candidate 和 late frame tick 行为。
- 状态：待处理（静态确认）。必须先分类每个 C++ writer 所需的 raw/update subset，再新增最小 Unity writer；不得全局替换为 ImmediateFrame 或回退为旧 Sprite path。

### D-HIT-003 — type1 / type2 / type4 normal damage 丢失公共 vital/stat writes

- C++ authority：collision.cpp:559-585 只将 type6 分流至 apply_hurt_reaction；type1/type2/type4
  继续进入 apply_hurt。hit.cpp:104-155 随后对公共 Entity hp、hp_max、combo_count_vic 和
  g_damage_stats 执行 normal damage write；game_world.h:143-165 证明这些是每个 Entity 都
  持有的公共字段，而非 type0-only wrapper。
- Unity current code：LF2Weapon.cs:451-459 统一把 kind0 交给
  BattleDamageWriter.ApplyWeaponDamage。该 writer（183-319）只处理 WeaponFlightCounter、
  fall/hit/frame/rest/relationship/hit record，没有写 victim.Health.HP、Health.HPBound、
  ComboCountVic 或 world.DamageStats。LF2WeaponBase.cs:31-32、876-882 证明 weapon 的
  Health 在 Unity 也是实际初始化、绑定到 runtime 的字段。
- 静态差异：type1/type2/type4 的普通命中在 C++ 同时改变公共 vital/stat 字段和武器耐久；
  Unity 当前只改变后者。type6 不包含在本项，因为 C++ 已明确将其放入 reaction-only 分支。
- 最小 fixture：分别生成 type1、type2、type4 victim，将 Health.HP、HPBound、ComboCountVic、
  Unk344、DamageStats 和 WeaponFlightCounter 设置为已知非边界值，以 kind0 injury 命中。
  验收 C++/Unity 的公共 vital/stat 与 weapon durability 字段均按各自合同变化；其对后续
  weapon lifecycle 的完整影响由 R1-SOURCE-005 联合验收。
- 状态：待处理（静态确认）。后续必须在 typed weapon damage contract 中补足 C++ 的公共
  field writes，不能把 WeaponFlightCounter 当作 HP 的替代。

## 3. kind dispatch 静态审计状态

下表只表示“C++ source 路径和 Unity source 路径已定位并完成当前静态判断”；不是 runtime
equivalence 证明，也不覆盖 R1-SOURCE-005 的 CPoint/held/lifecycle consumers。

| kind / target family | C++ contract | Unity current crosswalk | 当前结论 |
|---|---|---|---|
| kind0 → type0 | collision.cpp:561-585 → hit.cpp apply_hurt | CharacterDatHitResolver → BattleDamageWriter.ApplyStandardCharacterDamage | 主 writer 已映射；无本包新增 static difference。normal / alternate 的联合运行时仍待后续 fixture。 |
| kind0 → type1/2/4 | apply_hurt 的 public HP/stat + unk_31C 与 weapon response | LF2Weapon.Hit → ApplyWeaponDamage | D-HIT-003（公共 vital/stat 漏写）与 D-HIT-002（raw frame helper）。 |
| kind0 → type3 | apply_hurt 的 public HP/stat + type3 tail | LF2SpecialAttack.Hit → ApplySpecialAttackDamage | D-HIT-001；identity / held tail 的精细字段继续关联 SOURCE-005。 |
| kind0 → type6 | collision.cpp 明确 apply_hurt_reaction | LF2Weapon.Hit → ApplyWeaponDamage | reaction-only writer 已定位；当前未登记新的静态差异。 |
| kind1 | 无 target-type gate 的 common-field grab consume | unified InteractionWriter.TryApplyGrab | D-COL-005；type reachability 等 SOURCE-005 / fixture。 |
| kind2 / kind7 | pickup/link、holder/target、frame、attacking write | BattleInteractionWriter.TryApplyPickup | 主 writer 已定位；T14/T15/T16 relation consumer 交 SOURCE-005。 |
| kind3 | collect 明确 character-only 后 raw grab consume | BattleInteractionWriter.TryApplyGrab | 当前静态 mapping；后续与 CPoint consumer 联合验收。 |
| kind4 / kind5 | consume-time runtime ITR replacement | BruteForceSceneQuery.ResolveRuntimeItrForPair | source field-copy和时点已映射；kind5 holder relation 交 SOURCE-005。 |
| kind6 / kind8 / kind14 | hit confirm；heal/frame/position；direction block | unified runner + typed target resolver / BoundaryWriter | source entry与写入路径已定位；未登记新的静态差异。 |
| kind9 | consume-time type transform与 attacker HP zero | ResolveRuntimeItrForPair + ApplyConsumeEffects / typed writer | source transform与零 HP 时点已定位；type3 tail仍受 D-HIT-001 影响。 |
| kind10 / kind11 | flute weight、force、frame182、stat | character / weapon typed resolver | force/stat mapping已定位；frame writer落入 D-HIT-002。 |
| kind15 | wind force / object force | character / weapon typed resolver | source writer已定位；联合物理结果待后续 fixture。 |
| kind16 | injury/stat、frame200、vrest、held release | BattleDamageWriter.ApplyKind16 | 主 writer已定位；frame helper落入 D-HIT-002，held release交 SOURCE-005。 |

## 4. 已核对但暂不登记为差异的 adapter

| Unity adapter | 静态结论 | 原因 / 待验条件 |
|---|---|---|
| collision snapshot + separate vrest pass | 逻辑已映射，待测试 | C++ 在每个 active pair collect 前递减双方 vrest；Unity先标记全部 active roster、再只递减同为 eligible 的 attacker/victim row。对于同一 active slot 集合，其每个有序 vrest entry 一 tick 减一次。需 fixture验证 roster、dormant/structural flush 边界。 |
| role-aware / loose broadphase | 不是自动差异 | Unity formal path保留 authority ordinal sort、双方向 narrow phase，并在失败时回退 brute force。仍需 R1/R7 以 candidate sequence fixture确认 optimized/fallback 等价。 |
| candidate store/list + generation metadata | 不是自动差异 | C++ consume 使用 slot；Unity store read明确以当前 TargetSlot occupant 解析，TargetHandle仅作 diagnostic metadata。 |
| candidate capacity | 静态一致 | C++ game_world.h 的 HIT_CANDIDATE_MAX 为 20；Unity BattleEcsHitExecutionPlan.cs:287,369 也固定每 slot 20 entries，并在 plan capture 处 fail closed 检查范围 count。 |
| Loop1 / Loop2 participant partition | 静态一致 | C++ 按 consume 当刻 obj_type==0 / >0 分流；Unity LF2Entity.UsesCharacterDatInteractionPhase 也按当前 DAT object type 分流，fallback/optimized 均进入同一 candidate runner。 |
| kind4 / kind5 / kind9 runtime ITR | 逻辑已映射，待测试 | C++ collision.cpp:91-186 的 kind5 field copy、kind4 velocity flip、held release、type2 halve 与 kind9 transform，对应 BruteForceSceneQuery.ResolveRuntimeItrForPair:5924-6058；held relation字段的最后闭合交 SOURCE-005。 |
| alternate hurt selection | 逻辑已映射，待测试 | C++ collision.cpp:496-585 的 oid37/6/52、defend/prev2 状态分流，对应 LF2AlternateDamageResolver:318-372；不把旧 C# self-check 当作 runtime 证书。 |
| oid300 redirect | 逻辑已映射，待测试 | C++ collision.cpp:540-557 的 hit-state、body-X、team/frame-delay 路径，对应 LF2CharacterDatHitResolver:500-523；raw frame / later lifecycle仍由 SOURCE-005 fixture闭合。 |
| SuppressCollisionCandidateUntilTick | 当前无 production reachability 证据 | production source search只见 runtime reset/snapshot及 test/benchmark writer，未见普通 battle writer。它是 dormant adapter，不能当作当前 gameplay 差异；若出现 production writer再重新登记。 |
| EndCollisionCandidateConsumption | 逻辑已映射，待测试 | Unity在 ObjectInteraction 后失效 range/归还 list；C++ tail 后清 carrier。当前已读 C++ cpoint/weapon/frame advance 未在该区间读取 carrier；Runtime HitCandidateCount 仍保留至下一次 collect。 |
| kind0 hit record formula / owner | 逻辑已映射，待测试 | C++ collision.cpp:457-491 与 Unity LF2Entity.cs:633-684 都按 Z、slot 决定 owner、按当前 attacker frame 计算 anchor，并各消耗两次 0..8 随机偏移。完整 RNG stream/call-count 仍需后续 joint fixture，不能以静态同形当 runtime verified。 |

## 5. D-MOV-002 对 C03 / C07 的消费影响判定

- **VERIFIED（source）— 没有找到 Frame.PN / AttackingCounter 作为 C03/C07 的直接读入。**
  C++ candidate collect 读取 current frame、prev_frame、prev_frame2 和 DAT wpoint.attacking
  （collision_collect.cpp:248-254,324-333；collision.cpp:41-84,188-194）；其中
  wpoint.attacking 是 authoring frame field，不是 Entity runtime attacking counter。
  Unity BruteForceSceneQuery / BattleHitCandidateSequenceRunner 对应读取 collision frame、
  Frame.Prev / Runtime.PrevFrame2、current target 和 runtime ITR，没有读取 Frame.PN 或
  AttackingCounter 作为 candidate filter / entire-attacker abort gate。
- **INFERRED — D-MOV-002 的 helper 副作用仍能影响后续 frame progression。**
  ImmediateFrame 的 PN / attacking / transistor write 并非因此可视为安全；它会改变后续
  late frame tick 或下一 tick 的 frame history。该影响不在 C03/C07 的直接 read set 内，
  需由 D-HIT-002 的 writer fixture 与 R1-SOURCE-003/005 的 frame/CPoint consumer 合同共同验收。

## 6. 仍需完成的本包静态审计

1. type3 Karasu identity replacement、type0 alternate exception与 type1/2/4 common HP 的
   lifecycle readers，已定位 hit writer；它们之后的 held / identity / pool consumer由
   SOURCE-005 审计，未发现本包新增 writer difference；
2. C++ kind1/3/2/7 对位、frame-history、link/holder fields的 CPoint / held consumer审计；
   D-COL-005 的 kind1 target reachability、CPoint / held lifecycle依赖交 SOURCE-005；
3. hit record 的 global RNG call-count、presentation handoff 对照；逻辑记录留在本包，
   中央渲染展示归 R1-SOURCE-006；
4. candidate carrier clear 与 EndCollisionCandidateConsumption 之间除已查 CPoint/weapon/
   frame advance 外的 live consumer read set，未证实部分不得写成等价。

## 7. 不可回退边界

本包将来若需要修复，只能在 SimulationWorld / candidate runner / typed writer 的最小 adapter 中完成；不得回退 CentralOnly、Texture2DArray、dynamic Mesh、URP、MobileExtended / DesktopExtended 容量、FrameInputSet、30Hz、SoA/ECS store、pool 或 worker。
