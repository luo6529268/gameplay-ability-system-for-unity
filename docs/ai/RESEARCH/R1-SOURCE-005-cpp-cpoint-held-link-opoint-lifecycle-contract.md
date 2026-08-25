# R1-SOURCE-005 — C++ CPoint / held / link / opoint / 生命周期源码合同

> 状态：COMPLETED（静态 source 审计；runtime / joint fixture 待后续阶段）。  
> 行为 authority：J:\QQFile\NTSD2.4\ntsd_release 中实际参与 release 构建的 live source。  
> 证据等级：除非另有标记，以下均为 VERIFIED(source)，不是 executable trace 或 Play Mode 证据。

## 1. 审计边界

本文件只记录 C++ Release 的 live source 合同，重点追踪：

- game_tick.cpp 的两轮 negative-link held 扫描、CPoint/weapon sync、positive link validation；
- cpoint.cpp、weapon.cpp、frame_advance.cpp、collision.cpp 的直接写入者；
- Entity reset、slot 重用和 late-loop 中 opoint newborn 的可见边界。

不把历史 C# 注释、Unity self-check、旧 parity 或 performance 数据提升为 C++ authority。
未运行 executable，未向 C++ authority 目录写入任何内容。

## 2. C++ pass 与字段合同

| 合同 | C++ source | 规则（按 source） | 同 tick 消费者 / 影响 |
|---|---|---|---|
| C05-C01：negative held 第一轮 | game_tick.cpp:1441-1643 | 升序扫描 0..MAX_OBJECTS-1；只处理 active 且 link_state < 0 的实体。holder_idx 越界、holder 非 active 或 holder.target_idx 不等于当前 slot 时，只把 child.link_state 写为 0。其余有效对象按 holder 当前 frame/state、wpoint、饮料、drop/throw 规则处理。 | 在 candidate collect 前执行；可改变 held frame、位置、速度、link、holder frame_delay。 |
| C05-C02：CPoint kind 1 / kind 2 | cpoint.cpp:23-218 | kind 1 使用 attacker.prev_frame2 的 cpoint；校验 caught_idx / catcher_idx / victim.prev_frame2 kind 2。kind 2 第二轮使用当前 frame 校验 catcher 关系，不成立则写 frame=212、vy=-3、y 至少为 -2。 | 直接改抓取关系、frame、速度、caught_duration、攻击状态与 throw 分支。 |
| C05-C03：current-frame CPoint sync / held validation | weapon.cpp:13-132 | weapon_sync_runtime_pass 升序扫描所有 active 有 DAT 的实体。collision_check2_cpoint 仅在 current frame 的 cpoint.kind=1 且 state=9 时同步抓取受害者、处理 injury、cover 和位置；其后校验 held_weapon_slot 对应 child 仍 active、link_state<0、holder_idx 对应。 | 位于 object collision 后、positive link validation 前。 |
| C05-C04：positive link validation | game_tick.cpp:1827-1846 | 升序扫描 active 且 link_state>0 的 holder。target_idx 越界、target 非 active 或 target.holder_idx 不等于 holder slot 时，只写 holder.link_state=0。source 没有在此 pass 清 target_idx 或 held_weapon_slot。 | 为第二轮 held 之前的关系完整性门。 |
| C05-C05：negative held 第二轮 | game_tick.cpp:1860-2018 | 与第一轮同为升序的 link_state<0 扫描；在 CPoint/weapon sync 和 positive-link validation 后执行。包含 oid 122/123 消耗、current wpoint 位置同步、state 10/12 drop、wpoint.dvx throw 和 kind 3 random drop。 | 是 C++ 当前 tick 中最后一轮 held 写入；后续进入 preframe / render / late update。 |
| C05-C06：opoint spawn gate | frame_advance.cpp:102-172 | late entity loop 中，当前 frame 的第一个 opoint 必须 kind>0、oid>0 且 attacker.attacking=0；当前 DAT 为 character 时 frame_delay 必须为 0。逐 opoint、逐 spawn count 生成。 | child 立即 active，父/子可在同一 late scan 中继续被更高 slot cursor 访问；所有本 tick candidate/character/object collision pass 已在此前结束，因此标准 late opoint child 最早下一逻辑 tick 才能进入 candidate/consume。 |
| C05-C07：opoint child initialization | collision.cpp:1271-1371 | 从最低空闲 slot 50 起寻找；先 Entity.reset，随后 child active、slot、当前 action frame、position、velocity、team、holder_copy、owner_id=-1、spawner_slot=-1、child data identity。kind 2 写 parent.link_state=1、parent.target_idx=child slot、parent.held_weapon_slot=child slot、child.link_state=-1、child.holder_idx=parent slot。 | child reset 后 prev_frame2=0；spawn_from_opoint 只写 frame=action，不改 prev_frame2。 |
| C05-C08：opoint multi-spawn | frame_advance.cpp:126-169 | facing>10 表示数量与 facing mode；spread 为 i*10/(n-1)-5；对同 batch 两两写 vrest=40，并写 attack_exempt 扇形值。state 3003 另对 linked_slot 与 child 写 vrest=10。 | 可直接影响后续 candidate / hit rest。 |
| C05-C09：late lifecycle / free | game_tick.cpp:577-647、2190-2194；game_world.h:216-258 | late loop 为升序 slot scan。无效 frame 调用 free_entity，后者只立即 active=false、object_count--；真正 reset 发生在下次 spawn_into_slot / spawn_from_opoint 的 Entity.reset。 | 同 tick 新对象能复用低 slot，但低 slot cursor 已经过；高于当前 cursor 的 newborn 可加入本轮 late loop。 |

## 3. 关键精确规则

### 3.1 CPoint 的 frame 取样和 raw writer

- kind 1 主检查读取 attacker.prev_frame2（cpoint.cpp:30），不是 current frame；
- current-frame 抓取同步属于 weapon.cpp:22-107 的 collision_check2_cpoint，条件为 state=9；
- cpoint.cpp 对关系失效、动作、duration 耗尽等分支直接写 Entity.frame，未写 Entity.wait_counter；
- throw 分支在 cpoint.cpp:160-180 直接写 attacker.frame、attacker.prev_frame2、victim.frame、victim.prev_frame2 和速度。

### 3.2 正、负 link 的 source of truth

- positive holder：link_state>0、target_idx 指向 child，child.holder_idx 必须等于 holder slot；
- negative child：link_state<0、holder_idx 指向 holder，holder.target_idx 必须等于 child slot；
- C++ invalid positive link cleanup 只清 holder.link_state；
- C++ invalid negative link cleanup 只清 child.link_state，保留 child.holder_idx；
- oid 122/123 的消耗释放例外会同时写 child.link_state=0、holder.link_state=0、holder.target_idx=0、child.holder_idx=0，并清 held_weapon_slot。

### 3.3 held throw 的类型特例

- current wpoint.dvx 非零时，child DAT type 1 / 4 / 6：写 child.spawner_slot=holder slot、frame=40、vx/vy，随后解除 link；
- DAT type 2：写 child.frame=rand%6、vx/vy，随后解除 link；当前已读 C++ source 没有在这个 branch 写 child.frame_delay 或 child.spawner_slot；
- wpoint.kind=3：解除 link，frame=rand%6，写随机 vx/vy/vz；
- current held frame state 10 或 12：解除 link，frame=rand%16，继承 holder knockback 或 velocity 的投掷值。

### 3.4 opoint 与 slot / newborn

- normal opoint 只从 slot 50 开始找最低空槽；这是 C++ Authority400 语义，不是 Unity production 容量上限；
- C++ fixed 400 slot 仅定义 authority fixture，不能回退 MobileExtended 或 DesktopExtended 的容量边界；
- C++ reset 把 prev_frame2 归零；opoint child 的 current frame 是 action，而 prev_frame2 保持该 reset 值 0；
- C++ late-loop 以当前 cursor 升序读取 world.objects，因此同 tick 可见性受 newborn slot 相对 cursor 决定。
- C++ render callback 位于 frame postprocess / late entity update 之前（game_tick.cpp:2071-2083）；标准 late opoint child 的首个 render handoff 不属于当前 tick。本项的 Unity central presentation 可见性由 SOURCE-006 验收，不能按旧 SpriteRenderer 经验判断。

## 4. 已闭合的静态 mapping（不是对齐验收）

| C++ contract | Unity 对应 source | 当前结论 |
|---|---|---|
| slot 50 起的最低动态槽分配 | SimulationWorld.Registry.partial.cs:42、1117-1184；SimulationWorld.Passes.partial.cs:1264-1267 | 已映射；Unity 扩展容量是已批准 adapter，Authority400 时保持 slot 50 起的顺序。 |
| current / prev2 CPoint 两类读 | LF2Entity.cs:4479-4506；BattleCpointWriter.cs:12-136 | 已映射；GetCollisionFrameData 优先 Prev2，SyncHeldCpoint 使用 current frame。 |
| kind 2 opoint relation | BattleLogicEntityFactory.cs:299-319；LF2CharacterWeaponLinkResolver.cs:136-154 | 已映射；parent/child positive-negative link 与 slot relation 结构对应。 |
| multi-spawn spread / vrest | BattleLogicObjectPointRuntime.cs:258-279、360-419 | 已映射；spread、vrest=40、state3003 vrest=10 均有 source 对应。 |
| slot generation / pool | SimulationWorld.Registry.partial.cs:1054-1105、1206-1378；BattleStructuralWriter.cs:290-344 | Unity-only safety adapter；C++ 没有 generation 字段。必须以 slot-order、newborn 和可观察生命周期验收其等价性。 |

## 5. 已确认的差异 / 待处理候选

| ID | 结论 | C++ source | Unity source | 最小后续夹具 |
|---|---|---|---|---|
| D-SCHED-004 | C++ 有两轮 negative-held pass；Unity 当前主 tick 只有 HeldProcess 一轮。 | game_tick.cpp:1441-1643、1860-2018 | NTSDBattleTickSystem.cs:313-333；SimulationQueryAndLinkModule.cs:39-89 | 建立有效 child 在第一轮产生写入、CPoint/link 在中间改变关系、第二轮再次可见的 tick fixture。 |
| D-LINK-001 | invalid positive link：C++ 只清 link_state；Unity 同时清 TargetSlotIndex 与 HeldWeaponStableId。 | game_tick.cpp:1829-1845 | BattleEcsPositiveLinkValidationPass.cs:235-241；SimulationQueryAndLinkModule.cs:140-146 | target inactive / holder mismatch 后读取残留 target / held field 的 next-pass fixture。 |
| D-LINK-002 | invalid negative link：C++ 只清 child.link_state；Unity 还清 child.HolderStableId。 | game_tick.cpp:1450-1457、1866-1872 | SimulationQueryAndLinkModule.cs:53-61 | 失效 held child 后命中传播 / holder lookup fixture。 |
| D-HOLD-001 | DAT type 2 held throw：C++ branch 未写 frame_delay；Unity writer 写 held.FrameDelay=1。 | game_tick.cpp:1621-1630、1999-2006 | BattleHeldObjectWriter.cs:79-85 | type 2 wpoint.dvx throw 后本 tick / next tick frame-delay fixture。 |
| D-HOLD-002 | DAT type 2 held throw：C++ branch 未写 spawner_slot；Unity weapon Act 无条件写 SpawnerEntityIndex。 | game_tick.cpp:1597-1631、1977-2007 | LF2WeaponHeldStateResolver.cs:104-125 | type 2 thrown entity 后续 spawner-dependent state fixture。 |
| D-CPT-001 | CPoint relation/action/duration writer：C++ 是 raw frame assignment 且不写 wait_counter；Unity 多处调用 ImmediateWaitReset，至少清 Runtime.FrameWaitCounter。 | cpoint.cpp:35-124、212-216 | BattleCpointWriter.cs:24-72、148-182；LF2Entity.cs:5911-5916 | broken cpoint、action、duration expiry 三类 frame/wait fixture。 |
| D-CPT-002 | CPoint injury：C++ 同时写 global kill/damage statistics；Unity CpointWriter 只写实体 HP/HPBound/combo/holder KillStat，没有 world KillStats / DamageStats 写入。 | weapon.cpp:50-75 | BattleCpointWriter.cs:212-254 | lethal 和 non-lethal cpoint injury 的 stat fixture。 |
| D-OP-001 | normal opoint child 的初始 prev_frame2：C++ reset 后为 0；Unity 初始化为 action。 | game_world.h:216-258；collision.cpp:1285-1299 | LF2Character.cs:923-931；LF2WeaponBase.cs:846-859 | action 非 0 的 opoint child 在下一次 collision/CPoint snapshot 的 frame-history fixture。 |

## 6. UNKNOWN / 后续需要闭合

- Unity TrackerFlag / TrackerParent 在 kind 2 opoint 后被写入，并被 kind 5 hit path 读取；当前已读 C++ spawn_from_opoint 直接写的 relation 字段中没有同名字段。需在 R1-SOURCE-004/005 joint fixture 中追到 C++ 对应辅助字段和消费者，不能因 Unity 注释自行视作对齐。
- Unity PendingFlushDestroy、generation release 与 C++ active=false / 下次 reset 的可观察 slot-reuse 等价性尚无 joint fixture；目前仅可列为 adapter，不能宣布已对齐。
- 普通 Unity materializer 与 logic-only materializer 在 late opoint 的任务队列 / immediate boundary 均需在 SOURCE-006/007 做结构事件与可见性联合验收；不得借此回退 CentralOnly。
- Unity 另有 frame-logic / special-object interaction 的 queued object-point flush；C++ frame_advance.cpp 也有 hit_Fa 专项生成路径，但它不是本表的标准 process_opoint_spawn 调用链。SOURCE-003 的 frame-logic 合同已覆盖其入口；若发现该专项路径与 standard late opoint 共用字段时，须以 joint fixture 关闭，不能把二者混为同一时点。
- 本文件没有 C++ executable trace，所有差异尚未经过 runtime 复现。

## 7. SOURCE-005 验收设计输入

R2 之前每项至少需要记录：tick、slot、当前/prev/prev2 frame、wait、link_state、
target、holder、held slot、spawner、frame_delay、position/velocity、opoint action、child slot，
以及 C++ source contract 与 Unity fallback/optimized 的读写边界。对 D-CPT-002 另记录
KillStats 和 DamageStats。
