# NTSD28 B2 非 AI RNG live-call 交叉表

> Change ID：`NTSD28-B2-NONAI-RNG-CALLSITE-CROSSWALK-001`  
> Authority：NTSD 2.8-Logan formal playable source closure  
> 口径：源码中的`synchronized_next(`文本表达式；动态helper/循环的运行次数另列，不把一次文本调用误写成一次每tick调用。

## 1. Fresh 计数与旧口径纠正

| 文件 | synchronized文本数 | 本表范围 |
|---|---:|---|
| `battle_world.cpp` | 32 | BW-01～BW-32 |
| `input_routing.cpp` | 2 | IN-01～IN-02 |
| `simulation_tick_driver.cpp` | 1 | TD-01 |
| `game_session.cpp` | 15 | GS-01～GS-15 |
| 非AI合计 | **50** | 全部逐项列于下表 |
| `native_ai.cpp` | 42 | 不在本表；其中4个helper表达式不在`step_main` live closure，38个live表达式已由AI专项闭合 |
| 全production文本合计 | **92** | 50非AI + 42 AI |

旧`NTSD28-B2-INPUT-RNG-SOURCE-AUDIT-001`使用29/1/1/12加40个AI可能ID得到83，并据此写成非AI43。
该数字不再适用于当前authority文件；本表按fresh `rg -o "synchronized_next\\("`重计，旧数字只保留历史。
另有`append_confirmed_native_spark()`中的2个direct `crt_next()`，见CRT-01～02。

## 2. `battle_world.cpp` 32个 synchronized 表达式

| ID | 行 / owner | call-site / bound | 严格条件与运行次数 | 可观察副作用 | Unity候选 / owner阶段 |
|---|---|---|---|---|---|
| BW-01 | 380 `apply_native_unarmored_attacker_post_hit` | `0xEE / 16` | confirmed unarmored hit，attacker frame state1002 | attacker action0..15、motion | 多个HitResolver/DamageWriter `BattleRandInt(0,16)`；B5统一consumer时迁移 |
| BW-02 | 402 `apply_native_reduced_attacker_post_hit` | `0xF3 / 16` | defended/armor reduced branch，state1002 | attacker action0..15、motion | 同上；B5，不能让重复legacy/ECS writer双消费 |
| BW-03 | 951 `apply_encoded_status` | caller传入 / `100` | `encoded!=0`才消费；同一confirmed hit按weak→bound→facing→manacle→delay→join→mimic→dx→dy→dz→gain→confus顺序最多12次 | 状态timer、lock、proxy/remap等 | Unity无统一call-site owner；B5 status transaction |
| BW-04 | 969 `apply_confirmed_input_statuses` | `0x0041649C / 100` | poison非零先于BW-03全部字段 | poison type/timer/strength | B5 damage/status writer |
| BW-05 | 1008 same | `0x004166B2 / 7` | confus的BW-03成功后，对7个remap位置各消费一次；碰撞只线性探测、不额外抽取 | remap permutation | B5 status writer；现有input remap carrier可复用 |
| BW-06 | 2680 `materialize_special_state_clones` | `0x0041F792 / 7` | state9996、mode gate1、规则resolved；每个clone先抽X | clone X offset | `BattleLateEntityLifecycleModule` 10个legacy calls；B7，pass位置先由B3确定 |
| BW-07 | 2681 same | `0x0041F7B6 / 7` | 每clone第二次 | Y offset | B7 |
| BW-08 | 2682 same | `0x0041F818 / 15` | 每clone第三次 | vertical speed | B7 |
| BW-09 | 2686 same | `0x0041F8A9 / 2` | clone index0/2 | positive Z speed | B7 |
| BW-10 | 2689 same | `0x0041F87F / 2` | clone index1/3；与BW-09互斥 | negative Z speed | B7 |
| BW-11 | 2694 same | `0x0041F8DD / 3` | clone index0/1 | negative X speed | B7 |
| BW-12 | 2697 same | `0x0041F908 / 3` | clone index2/3；与BW-11/BW-13互斥 | positive X speed | B7 |
| BW-13 | 2700 same | `0x0041F92E / 7` | clone index4 | fifth X speed | B7 |
| BW-14 | 2702 same | `0x0041F955 / 4` | 每clone，在position/velocity抽取后 | action0..3 | B7 |
| BW-15 | 2704 same | `0x0041F96B / 2` | 每clone最后一次 | facing | B7；Unity当前legacy顺序总体相似但未用NativeRandom |
| BW-16 | 2799 `materialize_state18_broken_weapon_particles` | `0x00416A40 / 4` | previous state18/19且current仍18/19；global delay>0时零消费；非零roll停止 | 是否生成1个粒子 | `LF2Entity` state18 branch legacy call；B4 frame gate+B7 spawn |
| BW-17 | 2835 same | `0x004212DE / 29` | 已决定spawn且先确认free slot；每粒子tuple第一项 | Y | B4/B7 |
| BW-18 | 2836 same | `0x004212FE / 59` | tuple第二项 | X | B4/B7 |
| BW-19 | 2838 same | `0x00421339 / 11` | tuple第三项 | motion X | B4/B7 |
| BW-20 | 2840 same | `0x00421367 / 1` | tuple第四项；bound1仍推进stream并恒返0 | action140 | B4/B7；不得因恒0优化掉消费 |
| BW-21 | 3793 `rebuild_geometric_hit_candidates` | `0x86 / 2` | kind1、buffer未满、与当前nearest-source等距时才消费 | 等距source选择 | Unity候选构建未见正式RNG owner；B5 |
| BW-22 | 3890 same | `0x85 / 2` | ordinary nearest候选等距时消费 | 等距target替换 | B5；spatial/brute-force必须共享同一抽取顺序 |
| BW-23 | 5665 `resolve_confirmed_unarmored_hit` | `0xEC / 6` | kind0命中、holder relation2及reciprocal type2 child有效 | released child action | DamageWriter/Held resolver多处`0..6`候选；B5/B6单一owner |
| BW-24 | 5803 same | `chance / 100` | encoded first-body chance在1..99时消费；call-site本身为chance数值 | encoded body是否应用 | Unity ECS/legacy hit路径无等价site；B5 |
| BW-25 | 7012 `settle_held_refill_objects` | HP:`0x004181C9` / MP:`0x004182C0`, bound7 | refill child刚耗尽时恰好一次 | child kick X | held/refill路径；B6 |
| BW-26 | 7127 same | `0x0041865E / 6` | wpoint dvx非零且child type2 | released child action | weapon-link/held writer`0..6`重复候选；B6 |
| BW-27 | 7145 same | `0x00418726 / 6` | parent wpoint kind3，tuple第一项 | child action | B6 |
| BW-28 | 7146 same | `0x0041873A / 7` | kind3 tuple第二项 | fallback X speed | B6 |
| BW-29 | 7147 same | `0x00418756 / 4` | kind3 tuple第三项 | fallback Y speed | B6 |
| BW-30 | 7148 same | `0x00418772 / 5` | kind3 tuple第四项 | fallback Z speed | B6 |
| BW-31 | 7568 `advance_native_revivals` | `0x90 / 0x33` | state14有life、同组peer且`sum_x!=0`；先X | respawn X jitter `-25..25` | `BattleRespawnModule` legacy `0..51 -26`偏一；B4 |
| BW-32 | 7571 same | `0x91 / 0x1F` | BW-31后 | respawn Z jitter `-15..15` | legacy `0..31 -16`偏一；B4 |

## 3. direct CRT 两个表达式

| ID | 行 / owner | 公式与顺序 | 条件 / 副作用 | Unity候选 / owner阶段 |
|---|---|---|---|---|
| CRT-01 | 782 `append_confirmed_native_spark` | 第一次`crt_next()%9-4` | confirmed hit且spark host有容量；先Y jitter | LF2Entity/CharacterDat/ECS plan存在多套legacy jitter；B5唯一spark writer |
| CRT-02 | 783 same | 第二次`crt_next()%9-4` | CRT-01后，X jitter | B5；必须使用NativeRandom CRT流且保持Y→X顺序 |

## 4. 输入与tick-driver三个表达式

| ID | 行 / owner | call-site / bound | 状态 | 说明 |
|---|---|---|---|---|
| IN-01 | `input_routing.cpp:845 route_native_standing_attack` | `0x82 / 2` | `MIGRATED_B2` | `BattleCharacterActionWriter`已使用NativeRandom；standing fixture逐次equal |
| IN-02 | `input_routing.cpp:855 same` | interaction101:`0x83`，否则`0x84`; `/2` | `MIGRATED_B2` | conditional site已接；同一文本表达式代表两个互斥site |
| TD-01 | `simulation_tick_driver.cpp:806 step` | `0x92 / 200` | `FORMAL_CONSUMPTION_MISSING` | selected-mode +0x4C非1/2且active weapon count<4时消费；roll0后locked empty table停止。用户只批准保留Unity掉落效果，不批准删除正式0x92 stream消费。gate producer归B8、phase placement归B3；B12证明与例外legacy RNG隔离 |

## 5. `game_session.cpp` 15个表达式

| ID | 行 / owner | call-site / bound | 条件与顺序 | Unity候选 / owner阶段 |
|---|---|---|---|---|
| GS-01 | 1623 `spawn_native_story_instance` | `0x114 / zSpan` | free slot/definition/hp完成后先抽Z | `SimulationStageWaveModule`当前最后抽Z；B8 stage spawn，顺序差异 |
| GS-02 | 1627 same | `0x116 / 2` | `x==-1000`时在Z后抽side | StageWave legacy side draw；B8 |
| GS-03 | 1629 same | `0x117 / 300` | GS-02==0的positive-X分支 | StageWave legacy bound-side draw；B8 |
| GS-04 | 1631 same | `0x118 / 300` | GS-02!=0的negative-X分支；与GS-03互斥 | B8 |
| GS-05 | 1634 same | `0x115 / 300` | authored `x!=-1000`；与GS-02～04互斥 | B8 |
| GS-06 | 2044 `transition_native_story_stage` | `0x10E / depthSpan` | retain type0逐物理slot，在位置/action/HP/MP恢复后 | Stage transition当前无同序NativeRandom；B8 |
| GS-07 | 2563 `consume_native_f8_drop` | `0xD1 / 30` | F8每个ordered candidate tuple第一项 | `RunMode2Tail` legacy四抽；function-key route先B2，效果/内容B8/B11 |
| GS-08 | 2564 same | `0xD2 / 30` | tuple第二项 | B8/B11 |
| GS-09 | 2565 same | `0xD3 / 30` | tuple第三项 | B8/B11 |
| GS-10 | 2566 same | `0xD4 / 30` | tuple第四项 | B8/B11；当前Unity另有OID122筛选随机，不能冒充该tuple |
| GS-11 | 2822 `step`→`SelectionFlow28::step_cpu_fill` | `0xD9 / candidateCount` | 每个active random-origin slot按逻辑slot顺序 | 用户排除完整选择流程；B8只需保证传入battle的seed/roster/位置合同，不实现UI流程 |
| GS-12 | 2916 `step` random background | `0xDA / concreteBackgroundCount` | pre-battle menu选Random且count>0 | 选择流程用户排除；B8接口/B11内容catalog |
| GS-13 | 2970 `step` combatant X | CPU:`0xDB`，human:`0xDD`; `/xUpper` | start-battle按logical slot，先X | `AppManager` legacy两个raw初始位置抽取；选择UI排除，B8 battle-entry字段映射 |
| GS-14 | 2976 same combatant Z | CPU:`0xDC`，human:`0xDE`; `/zUpper` | GS-13后 | B8 battle-entry字段映射 |
| GS-15 | 3942 `resolve_native_bgm_selection` | `0x004021E0 / bgmCount` | selection0且world有效 | `MIGRATED_B2` direct-battle bootstrap；音频实际播放归B10 |

## 6. Unity fresh候选库存（不能当live call数量）

对非Test、非AI目录搜索`BattleRandInt(`与`Rng.NextInt/NextRaw/NextUInt(`得到87个语法命中，其中
`LF2Entity.BattleRandInt`方法声明1个，实际调用候选86个。它们包含legacy/optimized重复writer、用户例外和
尚未按B3+顺序统一的路径，不能按数量与authority 50+2一一配对。

| Unity文件/簇 | 语法命中 | 分类 |
|---|---:|---|
| CharacterDatHit/CharacterHit resolver | 4 | B5 legacy hit候选 |
| `LF2Entity` | 17（含1声明） | spark、state18 particles、special/held等混合legacy路径 |
| CharacterWeaponLink + WeaponHeldState | 8 | B6 held/weapon候选 |
| `BattleEcsHitExecutionPlan` | 9 | B5 projection-only RNG；不得与writer双推进 |
| `BattleDamageWriter` | 10 | B5 production writer候选 |
| `BattleHeldObjectWriter` | 6 | B6 production writer候选 |
| `BattleLateEntityLifecycleModule` | 10 | BW-06～15最接近候选，仍使用legacy stream |
| `BattleRandomWeaponDropModule` | 12 | 用户例外normal drop +旧mode2 F8混合；必须与formal stream分开 |
| `BattleRespawnModule` | 2 | BW-31/32候选，offset当前偏一 |
| `SimulationStageWaveModule` | 6 | GS-01～06候选，抽取顺序不同 |
| `AppManager` | 2 | excluded selection的legacy battle-entry position候选 |
| `SimulationWorld.Rand` | 1 | AI legacy wrapper定义，不是非AI调用 |

此外多个AI legacy `.Rand(...)`路径仍存在，但DataOrientedCanonical生产owner已由AI同步cursor隔离；它们不进入
本表的非AI迁移，也不得在shadow/oracle时推进NativeRandom。

## 7. 同tick owner顺序与实施路由

1. GameSession/host前置：selection/story/F-key/BGM根据上层state发生；用户排除选择UI不等于允许污染battle-entry RNG。
2. tick内：AI synchronized → input0x82/83/84 → frame/physics/revival0x90/91 → candidate0x86/85 →
   hit/status/post-hit/CRT/0xEC/encoded-body → normal-drop0x92 → held/refill → late frame/lifecycle particles/clones。
3. 当前B2只具备统一NativeRandom owner、snapshot/checksum、AI、input与BGM消费。其余调用不能在旧pass中机械替换，
   否则会把尚未对齐的重复writer和错误pass顺序固化为“正式”。

按依赖拆包：

- `NTSD28-B2-FUNCTION-KEY-ROUTE-CROSSWALK-001`：下一包；先闭合F3～F12 route/reject及effect owner，避免F8
  D1～D4在旧F8合同下接错。
- B3建立native pass骨架、normal-drop 0x92所在边界及single-owner consumption seam。
- B4迁移BW-16～20、BW-31～32；B5迁移BW-01～05、BW-21～24、CRT-01～02；B6迁移BW-25～30；
  B7迁移BW-06～15及spawn visibility；B8迁移TD-01 gate producer、GS-01～14及F4/F6～F9 effects；
  B10消费音频但不得新增战斗RNG；B11决定内容catalog；B12按joint call log验证完整顺序。

结论：非AI call-site库存已闭合，但不能在B2把B3～B8旧行为直接改用NativeRandom。B2剩余可独立闭合的真正
阻断是完整function-key route/gate；后续每个domain包必须按本manifest迁移自己的RNG调用并加入joint trace。
