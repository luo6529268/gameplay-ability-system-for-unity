# NTSD 2.8-Logan B3 主pass顺序交叉表

> Change ID：`NTSD28-B3-PASS-SKELETON-ENTRY-AUDIT-001`，由
> `NTSD28-B3-BUGFIXED-PASS-CONTRACT-REBASELINE-001`修订  
> 结论：`BUGFIXED-AUTHORITY-REBASELINED / 57-CHECKPOINT-CONTRACT / PRODUCTION-ORDER-STILL-PENDING`  
> 范围：战斗Session pre/core/post与逻辑完成后的render snapshot handoff；不实现B4～B8领域行为。  
> 日期：2026-09-04

> **2026-09-04修订：** 用户确认当前artifact是其修复Bug后的预期版本。本文当前表以EXE
> `B1E13AE1...9033`、82-file playable C++/header closure `39DDDA15...6109`为准；75-file
> source-capture子闭包为`07CD47A0...778F`。旧
> `1277B70B...DAF75`/`C59BD8D3...2D75`基线只用于说明规则变化，不再有裁决权。

## 1. Authority身份与正式构建闭包

| Artifact | SHA-256 | 结论 |
|---|---|---|
| 根`NTSD2.8-Logan.exe` | `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033` | 用户确认的Bug修复版正式行为权威。 |
| `source/README_SOURCE.md` | `C0BA44DB4A9037E5086348668F1046A83A760D351F2BB2D6062E724676BB3D85` | 声明源码快照对应当前发行EXE。 |
| playable C++/header closure manifest（82 files） | `39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109` | 按当前正式`build.ps1 -Target playable`的全部C++ source与core/playable headers计算。 |
| source-capture manifest（75 files） | `07CD47A0623F23D2C439E0E85EABF2ED10F8EAE8FC7D70DDB8396C704B3D778F` | B0/B2捕获器编译的规则相关core/session/scenario子闭包与全部headers。 |
| `simulation_tick_driver.cpp` | `4AA2CA63BDFFB0D9C7D2148354C70BE8EA28697969CEDA2693EBC247A8BC2187` | `SimulationTickDriver28::step`的当前core权威。 |
| `game_session.cpp` | `9CCB6E12A97FCB9FF95DF2FE95BC33F24C0AC32E9F956E41D23E4C14165E4189` | Session pre/post与flow权威。 |
| playable `build.ps1` | `AA8EBF4520D3C36BD5ADBB5B5BB5750957C5C71826AD64ECC63F0E81A23F292E` | `$coreSources`新增`kind_catalog.cpp`、`minibar_catalog.cpp`并含core tick；`-Target playable`编译Session/renderer/main。 |

正式参与性证据：`build.ps1:53-82`把data catalog、core tick及其frame/input/physics/collision/hit/spawn/world/render依赖
纳入`$coreSources`；`build.ps1:628-699`把该集合与`game_session.cpp`、`main.cpp`等编入
`Ntsd28Playable.exe`。重建产物不覆盖根正式EXE，本审计没有执行构建或写入authority。

## 2. Authority完整有序骨架

### 2.1 GameSession pre-core

| Order | Pass / source | 关键边界 | B3或后续owner |
|---:|---|---|---|
| G00 | world/loading guard；`game_session.cpp:2632-2643` | loading时清function-key byte/input/last tick并返回，world不推进。 | B3 gate；loading表现排除，但战斗冻结边界保留。 |
| G01 | queued F-key dispatch；`:2644-2664` | 快照并清队列，严格`F3→F6→F7→F8→F9`，仅accepted写event byte。 | B2 route已闭；B3固定pre-tick placement。 |
| G02 | process globals projection；`:2669-2671` | flow分类前把process规则投影到实体。 | B3 placement；具体字段B5/B8/B11。 |
| G03 | BattleFlow classify；`:2671-2692` | 先看上tick存活组；致死命中要到下一tick才启动结果timer。 | B3 boundary；行为B8。 |
| G04 | upper-scene/selection transition；`:2693-3175` | 可直接返回；完整selection flow为用户排除项。 | B8只保留战斗冻结/离场逻辑。 |
| G05 | transition-state early return；`:3177-3185` | 清active combatant pending input、last tick与diagnostics，不进入core。 | B3 gate。 |
| G06 | story phase controller；`:3187-3191` | flow分类后、逐实体core前；可在此gap发布新slot。 | B3 placement；行为B8/B7。 |
| G07 | globals re-project + input handoff；`:3191-3196` | story改变的globals再次投影；然后把slot input写入实体pending。 | B3 placement；输入producer B2已闭。 |
| G08 | knockout cursor capture；`:3197-3200` | 保存feed起点，随后只调用一次core step。 | B3 placement；feed行为B9/B10。 |

### 2.2 `SimulationTickDriver28::step` core

| Order | Pass / source | 遍历、快照与可见性合同 | Owner |
|---:|---|---|---|
| C00 | input phase advance；`simulation_tick_driver.cpp:383-386` | 1tu恒0；2tu从零初始化值交替1/0。 | B2已闭，B3固定首位。 |
| C01 | native spark advance；`:388-396` | 先推进上tick spark，随后本tick新spark保持base cell完整33ms。 | B3 placement已实现；表现B9。 |
| C02 | physical/object-hit_Fa/AI sample scan；`:398-510` | 升序live slot；非角色hit_Fa可销毁slot，必须重新取指针；新生高slot可见；AI同scan写producer并sample。 | B3 traversal；B2输入算法保留待新身份重证。 |
| C03 | proxy + sampled input route scan；`:511-564` | 第二次升序scan；先精确proxy copy，再route；第一scan已完成producer。 | B2行为保留，B3保留barrier。 |
| C04 | frame motion；`:566-580` | 全slot输入后独立升序current-frame motion。 | B3 phase；行为B4。 |
| C05 | teleport；`:581-590` | 所有motion后独立全slot解析400/401并清三轴。 | B3 phase；行为B4。 |
| C06 | nested physics slot transaction；`:591-609` | 每个升序live slot先physics/audio，随后立即normalize dead-character HP/MP，再到下一slot。不能拆成两个全局scan。 | B3 nested physics合同；规则B4/B10。 |
| C06a | physics；`:591-603` | 当前slot。 | B4/B10。 |
| C06b | dead-character resource normalize；`:604-608` | 当前slot physics后立即执行；本tick后续致死要到下ticknormalize。 | B4/B5。 |
| C07 | revival；`:611-673` | 完整physics slot transaction后、geometry前；可立即生成OID998，高slot对下游可见。 | B4/B7/B9。 |
| C08 | depth clamp #1；`:675-681` | geometry前第一次type-0 stage depth clamp。 | B4/B8。 |
| C09 | held refill #1；`:682-685` | clamp后第一次HP/MP/held消耗结算。 | B6。 |
| C10 | action snapshot；`:687-693` | motion/teleport/physics/refill后冻结碰撞用`+0x7C`。 | B3不可变快照；B5消费。 |
| C11 | geometry/candidate rebuild；`:694-699` | 使用C10快照建立候选。 | B5。 |
| C12 | fusion；`:700-714` | candidate后、hit前；被停用partner本tick不能继续收发hit。 | B5/B7。 |
| C13 | active weapon count；`:715-728` | hit前统计active type1/2/4/6到`DAT_004A115C`。 | B3 placement；drop例外隔离。 |
| C14 | type-0 hit/relation consume；`:730-865`调用`consume_native_hit_pass(true)` | 升序type-0 attacker，再按candidate order；普通与relation共享consumer body。 | B3 pass shape；B5。 |
| C15 | ordinary random drop；`:867-888` | 两段hit caller loop之间消费0x92；正式候选表为空。 | 用户例外；只隔离位置/RNG。 |
| C16 | non-type-0 hit/relation consume；`:890`调用`consume_native_hit_pass(false)` | 升序非type-0 attacker，再按candidate order。 | B3 pass shape；B5。 |
| C17 | catch relation advance；`:892-897` | 两段hit均完成后推进catch关系。 | B6。 |
| C18 | catch settlement；`:898-902` | relation advance后结算并产生caughtact combo。 | B5/B6。 |
| C19 | depth clamp #2；`:904-907` | hit/catch后第二次clamp。 | B4/B8。 |
| C20 | held refill #2；`:908-911` | 第二次clamp后再次结算held refill。 | B6。 |
| C21 | final X/Z stage settlement；`:912-924` | 第二次refill后统一边界settlement。 | B4/B8。 |
| C22 | horizontal impulse finalizer；`:925-929` | **在第二次clamp/refill与stage settlement之后**提交幸存实体的命中水平冲量。 | B3 placement；行为B5。 |
| C23 | begin native resource tick；`:931-938` | 每tick只推进一次process resource phases。 | B3 single owner；B5/B11。 |
| C24 | begin frame tick；`:938` | 每tick只推进一次world sequence/frame phase。 | B3 single owner；B4。 |
| C25 | nested live-slot tail；`:939-1072` | 动态升序；每slot完整执行C25a～C25p。新生高slot同tick继续，落入已过低slot则下tick。 | B3核心遍历；B4/B5/B7/B11。 |
| C25a | definition transition；`:953-962` | 当前slot。 | B7/B11。 |
| C25b | special-state clone materialize；`:964-979` | definition后、resource前，可改变slot集合。 | B7/B11。 |
| C25c | native resources pre-display；`:981-984` | 当前slot。 | B5/B11。 |
| C25d | display values；`:986-989` | 位于同slot两段resource之间。 | B9。 |
| C25e | native resources post-display；`:991-994` | 当前slot。 | B5/B11。 |
| C25f | computer state 7000～7999；`:996-1011` | frame前写`+0x1B8`。 | B3/B4。 |
| C25g | frame step；`:1013-1024` | 当前slot frame/state与audio。 | B4/B10。 |
| C25h | reaction timers；`:1025` | frame后、armor/rest前，仍在当前slot事务。 | B4/B5。 |
| C25i | armor recovery；`:1026` | reaction后。 | B5。 |
| C25j | attacker rest decrement；`:1027` | armor后。 | B5。 |
| C25k | frame-zero opoint；`:1029-1036` | 在state18 particle/lifecycle前生成；可改变slot集合。 | B7。 |
| C25l | state18 broken-weapon particles；`:1038-1045` | opoint后，以post-frame action与`+0x78`比较。 | B7/B9。 |
| C25m | previous action `+0x78` commit；`:1046-1048` | 粒子后提交；与C10`+0x7C`不同。 | B3/B4。 |
| C25n | weapon piece fragments；`:1050-1062` | terminal lifecycle前，可生成更高slot。 | B7/B9/B10。 |
| C25o | pending lifecycle；`:1063-1066` | 若消费slot，跳过healing。 | B7。 |
| C25p | healing；`:1068-1071` | lifecycle未消费时在当前slot尾部执行，不再是全局scan。 | B5/B6。 |
| C26 | ordinary combo expiry；`:1073-1075` | sequence已提交、整个nested tail后expire并返回。 | B5。 |

Authority tests同时锁定两项关键边界：

- `battle_world_tests.cpp:7984-8009`：一个tick必须执行两次held refill（示例+6 MP、child -4 HP）。
- `battle_world_tests.cpp:8403-8425`：wait/next进入的frame在同tick发出opoint。
- `battle_world_tests.cpp:8468-8502`：低slot生成高slot，高slot同tail继续；高slot生成已走过低slot，低slot延到下tick。
- `battle_world_tests.cpp:8505-8552`：geometry→hit→hit-stop→后续impulse finalizer闭合。

### 2.3 GameSession post-core与snapshot handoff

| Order | Pass / source | 边界 | Owner |
|---:|---|---|---|
| G09 | F8/F9 object effect；`game_session.cpp:3201-3216` | core完整返回后消费共享pending；固定dispatch令same-window F9覆盖F8。 | B3 post placement；效果B8/B11。 |
| G10 | F7 full MP；`:3217-3227` | core后升序active slot，仅写`current_mp=500`并清flag。 | B3 post placement；效果B8。 |
| G11 | scoreboard tick；`:3228-3233` | results timer<100才增。 | B8。 |
| G12 | story stop-audio；`:3234-3240` | 将stop-all事件追加到本tickaudio。 | B8/B10。 |
| G13 | knockout feed/prune；`:3241-3253` | 从G08 cursor处理新增KO，再按sequence prune。 | B9/B10。 |
| G14 | earthquake；`:3254` | 所有world/session post逻辑之后。 | B9。 |
| G15 | camera；`:3255` | earthquake后。固定世界相机为用户例外，但逻辑snapshot边界仍需明确。 | 用户例外/B9。 |
| G16 | step返回；`:3256-3257` | diagnostics提交。 | B3 completed-tick boundary。 |
| G17 | render snapshot build；`main.cpp:2607-2630` | 仅在完整logic step返回后build；随后独立render cadence消费/插值。 | B3不可变handoff；表现B9。 |

`main.cpp:2508-2526`先把本轮human input交给Session再step；`:2529-2555`在step后提交audio；
`:2613-2630`才构建completed-world snapshot。高刷新率插值只在后续present阶段消费previous/current snapshot，
不反写逻辑。

## 3. Unity当前生产顺序

基线SHA：

- `NTSDBattleTickSystem.cs`：`2F756D6A...3212`
- `SimulationWorld.cs`：`8E7F4E77...D565`
- `SimulationTickDriver.cs`：`2D1C3EFA...FFF8`
- `SimulationEntityTraversal.cs`：`D06C3951...E5A0`
- `SimulationPassPipeline.cs`：`4ADD8680...0ADD`

| Unity order | 实际调用 | 与2.8关系 |
|---:|---|---|
| UH00 | provider `GetFrameInput/BeforeSimTick`；driver `:492-505` | Unity Host adapter；frame input先冻结。 |
| UH01 | `ApplyPendingBattleFunctionKeyCommandsForTick`；`:506-542` | B2已接；发生在world stage snapshot与frame apply前。 |
| UH02 | `PrepareStageRuntimeSnapshotForTick`；`:543` | Unity-native stage carrier，需保持为逻辑输入快照，不得读取后续Transform。 |
| UH03 | `ApplyFrameInputSet`；`:557-559` | 输入写world后才进入TickSystem。 |
| U00 | `AdvanceBattleFlowTick` | 同时推进input phase/frame mod/toggle；大体对应G03+C00，但职责合并。 |
| U01 | `AdvanceNativeSparkLifecycleAll` | C01 production single writer，已对齐；B9资源表现未闭。 |
| U02 | HumanInput | controller poll已移到Cooldown前，对应C02前的physical sample。 |
| U03 | production CharacterInput | 第一遍按slot交错non-character hit_Fa与character producer，第二遍proxy/route；C02/C03 placement已验证。 |
| U04 | OID5152 production split | 已迁移：combined direct入口只保留compatibility；production fusion位于candidate后/hit前的C12，正值`Unk338`位于per-slot frame后的C25h；input-clear partial不进入二者。 |
| U05 | FrameMotion→NativeTeleport | 已拆为两个全slot barrier；C04基础writer与C05 state400/401 production owner已验证，C04完整字段归B4。 |
| U06 | `NestedPhysics` | 已按升序slot执行现有physics owner→立即dead type0 `HPBound/PP` normalize；exact character保留ECS fast path。物理公式仍归B4/B10。 |
| U07 | Revival | existing revival owner已移到完整C06后；其高slot newborn对后续current C08～C20与临时serial tail可见且不补跑C06。serial已不再夹在C07～C15之间。 |
| U08 | stage-Z clamp #1 | production位置已对齐C08：C07后、serial remainder前；算法/边界来源留B4/B8。 |
| U09 | negative held process #1 | production位置已对齐C09：C08后、serial remainder前；refill/WPoint/release/RNG算法仍归B6。 |
| U10 | CollisionSnapshot；C11 rest prelude已拆 | snapshot-only production位置已对齐C10；rest保持serial后/candidate前，C11 pass-shape待处理。 |
| U11 | pair vrest eligibility/tick | 与C11候选内部rest边界需B5核对。 |
| U12 | candidate collect | 对应C11，但输入frame/action时点仍不同。 |
| U13 | CharacterHit/PostInteraction | 大体对应type-0 caller半段，仍需B5证明pass shape。 |
| U14 | current random weapon drop | 用户例外，保留并隔离RNG/slot副作用。 |
| U15 | ObjectHit | 大体对应non-type-0 caller半段，仍需B5。 |
| U16 | candidate consumption end | Unity cleanup seam。 |
| U17 | CPoint/weapon sync | catch relation/settlement仍需B6。 |
| U18 | positive held validation | 同上。 |
| U19 | stage-Z clamp #2 | 位置近C19，但前置链仍不同。 |
| U20 | negative held process #2 | 不是已证明等价的C20 refill。 |
| U20a | temporary `SerialTickAll` remainder | 当前normal path已在C25之后；只剩type3 state/death、runtime snapshot与全局state9998 cleanup。B3 exit audit确认其仍有行为，必须由B4/B5/B7接管后删除，当前不得视为空壳。 |
| U20b | C23/C24 native world clock | Unity-owned phase12/phase3/sequence已在C22后、临时serial前single-owner提交；reset/snapshot/restore/checksum/parity闭合。C25 consumers仍未实施。 |
| U21 | PreFrameBounds | C21候选owner已移到current C20后、临时serial前；规则/dynamic stage字段仍归B4/B8。 |
| U22 | Stage wave | story phase与stage settlement职责仍混合。 |
| U23 | RenderDispatch | 旧baseline中早于Late/PostFrame/Results；该行已由`NTSD28-B3-C25-NESTED-TAIL-SKELETON-001` supersede，normal Render现位于C25与session tails/Results之后。 |
| U24 | FramePostProcess | C22现有owner已移到C21候选后、临时serial前；step-wait不再跳过。公式仍归B5。 |
| U25 | LateEntityUpdate | canonical/mirror rest tail已归位；其余state/resource/opoint/lifecycle尚未形成C25逐slot16步事务。 |
| U26 | Mode2 random weapon tail | legacy effect；正式F8/F9 handoff另有owner。 |
| U27 | EntityPostFrameTail | heal/catch/carrier混合，发生在presentation capture后。 |
| U28 | BattleResults | 属B8，当前在presentation capture之后。 |
| UF00 | `RefreshBattleEcsShadowAfterTick`；`:312-318` | finally中仅shadow compare/capture，不能成为canonical writer。 |
| UH04 | checksum→sound publication；driver`:559-562` | 同步路径在TickSystem返回后；worker路径在publication消费时完成。 |

Unity `SimulationEntityTraversal`确实按逻辑slot升序动态查询occupant，具备“新高slot可见”的基础；但
`BeginDeferredMutationEntityPass`在scope末尾才flush unregister/destroy。是否与C25 lifecycle逐slot立即消费等价，
必须由B7用同slot birth/death trace证明，不能仅凭动态enumerator判为已对齐。

## 4. S-01～S-16 B3入口裁决

| ID | 当前裁决 | B3必须做 | 不在B3实现 |
|---|---|---|---|
| S-01 | `CONFIRMED_ORDER_DIFF` | 建立2.8唯一有序骨架及生产checkpoint。 | 各pass内部算法。 |
| S-02 | `C01_PLACEMENT_ALIGNED / B9_PENDING` | tick-start spark advance已接入为production唯一logical writer；presentation消费只读。 | spark素材/资源映射与完整最终表现，B9。 |
| S-03 | `C02-C03_PLACEMENT_ALIGNED / B7_REUSE_PENDING` | production已按slot交错non-character hit_Fa/character producer并保持第二遍route；同/低slot立即复用待B7。 | hit_Fa具体行为，B5/B7。 |
| S-04 | `CONFIRMED_GLOBAL_BARRIER_DIFF` | 把frame motion从逐entity混合TU中分离为全局phase。 | motion公式/状态分支，B4。 |
| S-05 | `C04-C06-OWNER_ALIGNED / B4-B10_ALGORITHM_PENDING` | production已固定motion→teleport→每slot physics+dead-resource-normalize；禁止把normalize改成独立全局scan。 | 物理公式/audio细节仍归B4/B10；revival另包。 |
| S-06 | `C07_PLACEMENT_ALIGNED / BEHAVIOR_PENDING` | revival已固定在global physics后、geometry前并验证高slot下游可见。 | revival规则/floor/998 birth与表现，B4/B7/B9。 |
| S-07 | `C08-C09_PLACEMENT_ALIGNED / B6-BEHAVIOR-PENDING` | clamp#1→held-refill#1已固定在serial前。 | refill与held行为，B6。 |
| S-08 | `CONFIRMED_SNAPSHOT_DIFF` | 建立独立collision action snapshot，发生于physics后/frame-step前。 | candidate geometry，B5。 |
| S-09 | `CONFIRMED_BOUNDARY_DIFF` | fusion定位于candidate后/hit前。 | fusion规则与lifecycle，B5/B7。 |
| S-10 | `CONFIRMED_PASS_SHAPE_DIFF` | 固定type-0 hit caller loop→random-drop→non-type-0 hit caller loop；两段内部共享consumer body但不能合并跨过drop。 | relation/damage/armor/termination算法，B5。 |
| S-11 | `USER_EXCEPTION` | 隔离Unity随机drop的RNG与slot副作用。 | 不改其表现/掉落策略。 |
| S-12 | `CONFIRMED_ORDER_DIFF` | 固定catch advance→settle→clamp#2→refill#2→stage settlement→impulse。 | catch/weapon/impulse算法，B5/B6。 |
| S-13 | `CONFIRMED_ORDER_DIFF` | 固定clamp#2→held-refill#2。 | 具体行为，B4/B6/B8。 |
| S-14 | `CONFIRMED_BOUNDARY_DIFF` | stage settlement位于第二次refill后、resource/frame tick前。 | stage规则，B8。 |
| S-15 | `CONFIRMED_NESTED_TRAVERSAL_DIFF` | 建立process phases一次推进与C25a～p逐slot嵌套动态scan。 | definition/resource/display/frame/timer/opoint/fragment/lifecycle/healing，B4/B5/B7/B9/B11。 |
| S-16 | `OLD-GLOBAL-TAIL-SUPERSEDED` | reaction→armor→rest及healing/display已纳入每slot C25；只有combo expiry保持全nested-tail之后。snapshot仍在Session post后。 | 每项算法/表现，B4/B5/B6/B9。 |

## 5. Single-writer与快照边界

1. `FrameInputSet`是Host输入快照；B2已闭，B3不得重新采集物理键。
2. C10 `tick_action_snapshot/+0x7C`是碰撞专用不可变快照；不能用Unity presentation/runtime snapshot替代。
3. C24f `previous_action_078/+0x78`是frame-tail提交字段；不能与C10合并。
4. Unity `RefreshRuntimeSnapshot*`当前在多个pass内刷新兼容投影；它不是新的phase owner。重排时必须先确定
   canonical字段writer，再在明确checkpoint刷新，禁止legacy与ECS各写一次。
5. ECS `TryExecute`/mode切换是互斥canonical writer seam；`RefreshBattleEcsShadowAfterTick`只是观察者。
   后续B3包不得把shadow提升为第二writer。
6. Authority C25是16步逐slot嵌套事务，不允许重写成“所有definition→所有resource→所有frame→所有timer/lifecycle”。
7. Authority G17读取completed state。Unity current U25在U27～U32之前capture，属于明确snapshot boundary差异。
8. dedicated worker与同步路径必须共享同一core有序合同；worker publication可以延迟materialize，但逻辑
   snapshot、checksum和provider `AfterSimTick`只能对应完整返回的同一tick。

## 6. B3实施拆分与首包

### 首包（唯一下一包）

`NTSD28-B3-PASS-ORDER-CONTRACT-001`

- 新建不可变、allocation-free的2.8 pass/order/domain/traversal contract和focused Editor tests。
- 覆盖G01～G17、C00～C30及C24a～h，显式表示C24嵌套slot事务、两个action字段、两次refill、
  function-key pre/post和completed snapshot handoff。
- 更正即将触达文件内“旧game_tick/C# authority”误导性注释。
- 不调用现有行为、不改生产执行顺序；状态预期为
  `FOCUSED_TEST_PASS / IMMUTABLE-PASS-CONTRACT-READY / PRODUCTION-UNCONNECTED`。

该首包现已达到上述状态：red `CS0246`→compile0→focused10/10→related42/42→4096 zero-alloc→
21:17:21 full SelfCheck PASS/Console0。下一步转为production actual-sequence recorder与首差基线，
不是直接把52项一次性接入。

### 后续候选（不得并入首包）

1. production checkpoint recorder与old/new actual-sequence focused red基线。
2. Host/Session pre/post gate与completed-tick snapshot handoff。
3. C04→C07 motion/teleport/per-slot physics+normalize/revival barrier。
4. C08→C22 geometry/type-separated hit/catch/stage/impulse边界；内部行为仍由B5/B6/B8包实现。
5. C23→C26 process tick、C25逐slot16步tail与combo expiry；birth/lifecycle行为由B7证明。
6. B3 joint pass trace与exit audit；只有顺序、snapshot、single-writer、同步/worker均闭合才进入B4。

第1项现已完成基线部分：opt-in实际phase recorder在真实空world完整tick记录30项、input-clear partial记录5项；
首差为共同C00之后`expected CoreSparkAdvance / actual Cooldown`。focused6、相关90、4096 0B与full SelfCheck
均通过；没有移动任何production pass。下一步先做spark advance/publication边界只读审计。

Spark边界审计现已闭合：Authority C01的logical native-cell推进与资源/发布无关，terminal为末位9且只弹
tail；Unity current则在U25 capture后由presentation finalize/no-publication写回，并使用旧valid-age范围，
资源不可用还会冻结逻辑。现有调用不能直接搬动。下一包必须先建立独立native lifecycle core，随后再把
production single writer接到C01并移除U25写回。

Native lifecycle core现已通过focused：14项compile red后，末位9 predicate、non-tail retain、tail-only pop、
0～98 increment、invalid keep、10槽4096次0B均闭合；related67与full SelfCheck通过。它仍无production
caller。下一包接C01并撤除RenderDispatch正式writeback，不能继续由资源可用性决定logical age。

C01 production integration现已验证：input phase后、当前Cooldown前执行resource-independent spark advance；
正式RenderDispatch、同步Host、worker consumed/stop与stress snapshot均改为只读publication acknowledgement，
旧finalize/no-publication writer只剩compatibility tests。actual full/partial occurrence变为31/6，首差下移为
`expected CoreProducerSampleScan / actual Cooldown`。final focused38、相关98、22:14:51 SelfCheck/Console0与
真实kind0 Play tick3～6均PASS；live/frozen ages为`[0]→[1,0]→[2,1,0]`，无发布tick为
`[3,2,1,0]`，Late不二次推进。B9资源映射保持未关闭。

Bug修复版authority随后由用户晋升并完成57-checkpoint重新基线：physics/dead-resource normalize成为per-slot
2步事务，hit为type0→drop→non-type0，stage settlement后才impulse，slot tail扩为16步。red18→compile0、
focused10、B3 related38及22:58:20 SelfCheck通过；旧52项contract已supersede。

C02/C03 production placement现已验证：HumanInput、按slot交错的non-character hit_Fa/character producer和第二遍
proxy/route均在Cooldown前；后续FrameLogic occurrence移除。focused8、input/AI/worker related110、23:24:44
SelfCheck及真实kind0 Play ticks3～6通过。full actual occurrence为30、partial为6；下一首差下移到
`expected CoreFrameMotion / actual Cooldown`。

（历史阶段）Cooldown writer extraction现已验证：production不再调用早期Cooldown；无Itr/state1001 attacker-rest clear在
collision candidate prelude同步canonical/mirror，frame tick后仅在二者原本同owner时提交新canonical值到mirror，
不会覆盖held step12独立状态。focused10、相关136、23:48:17 SelfCheck与真实kind0 Play通过；full/partial
occurrence当时为29/5、首差为`expected CoreFrameMotion / actual RuntimeMaintenance`；该首差已由下段OID拆分闭合。

OID51/52 production split现已验证：C12 fusion读取pre-decrement timer，C25h才在每slot frame后递减；
combined direct compatibility不变，input-clear partial不进入二者。red4、compile0、focused10、相关182、
00:14:06 SelfCheck及真实4503-tick OID Play通过；merge tick末4499，timer0 tick不提前split，下一C12 split后
tick末899/双方PP5。full/partial occurrence为29/4，下一首差为
`expected CoreFrameMotion / actual EarlyFrameAdvance`。

C04/C05/C06 owner审计已闭合：Unity不是单纯缺少一个FrameMotion phase，而是exact/shared character的基础
`dvx/dvy/dvz`已混入C03，non-character又在SimTU内合并C04+C06；Authority C04额外拥有linked-platform、
delay scale和frame `dx/dy/dz` positional motion，Unity当前没有对应carrier。C05 current还包含隔tickgate、
self/Y选择差异和正式playable无对应的state500/501 transform；C06则按entity交织且缺physics后立即dead-resource
normalize。下一包`NTSD28-B3-C04-FRAME-MOTION-PRODUCTION-OWNER-001`只抽离基础C04唯一owner，完整字段/B4、
C05和C06分别处理。

C04基础production owner现已验证：production C03只route，随后显式升序FrameMotion全slot应用现有
dvx/dvy/dvz；production Serial作用域抑制non-character旧重复writer，direct CharacterInputAll/SimTU仍兼容。
red6、compile0、focused11、相关211、00:43:10 SelfCheck与真实kind0 Play均通过；full/partial occurrence为
30/4。完整linked-platform/delay/dxyz仍明确归B4，下一首差为C05 native teleport对combined
EarlyFrameAdvance（含half-cadence与state500/501 extra）。

C05 production现已验证：显式`NativeTeleport`替代combined EarlyFrameAdvance，state400/401每tick运行并修正
self排除、collision-Y和precise sync；state500/501只留direct compatibility。red7、compile0、focused11、
相关209、01:01:47 SelfCheck、真实kind0 no-op Play及state400/401 production两tick均PASS；targeted result
SHA02016B53、cleanup PASS、Scene unchanged、Play exited、Console0。full/partial仍为30/4，下一结构首差为
C06 nested physics/SerialTick。

C06 production现已验证：显式`NestedPhysics`在C05后按升序slot运行现有physics owner并立即对死亡current-DAT
type0写`HPBound/PP=0`；exact character保留ECS fast path，后续serial不重复physics，direct入口保持兼容。red2、
compile0、focused6、actual22、NTSD28 92、frame23、worker20、physics22、01:38:19 SelfCheck及C06/C05 Play均PASS；
targeted SHA50BA937F/8064DFCE、cleanup PASS、Scene unchanged、Play exited、Console0。full/partial为31/4，
下一结构首差是C07 revival对现有serial remainder；C06内部物理公式仍归B4/B10。

C07 placement现已验证：现有`BattleRespawnModule`的production唯一调用已从serial后移到完整C06后；新生高slot
对同tick serial remainder可见且不会倒流补跑C06。red1、compile0、focused3、C04～C07/actual25、W05+worker28、
01:59:07 SelfCheck及targeted Play均PASS；SHA185F9CBD、cleanup PASS、Scene unchanged、Play exited、Console0。
full/partial仍31/4，下一结构首差为C08 stage-depth clamp对serial remainder；floor、terminal primary、
continuation controller/group/visual、RNG和OID998行为差异留B4/B7/B9。

C08 placement现已验证：第一次`StageBounds` production调用已移到C07后、serial remainder前，C19第二次保持原位。
red2、compile0、C04～C08/actual27、stage/frame/worker48、02:11:19 SelfCheck与targeted Play均PASS；当前stage
237..760、slot50 Z910→serial观察760，SHA7841FD33、cleanup PASS、Scene unchanged、Play exited、Console0。
full/partial仍31/4；stage snapshot来源、截断/边界数值与C21 settlement留B4/B8，下一结构首差C09 held refill。

C09 placement现已验证：第一次`HeldObjectProcessAll`已移到C08后、serial前并保留C20第二次。red2、compile0、
focused2、C04～C09/actual29、held/link/frame/snapshot/worker65、02:34:48 SelfCheck与targeted Play均PASS；
serial观察frame5/pose(95,47,199)，SHA62ED5E71、cleanup PASS、Scene unchanged、Play exited、Console0。
full/partial仍31/4；完整refill/exhaust/WPoint/release/RNG行为继续归B6，下一结构首差C10 snapshot对serial。

C10 placement现已验证：snapshot-only已从combined入口拆出并移到C09后、serial前；public combined direct入口仍兼容，
C11 rest prelude保持serial后/candidate前。red2/3、compile0、focused3、C04～C10 final32、related59、
02:51:04 SelfCheck与targeted Play均PASS；current5/snapshot0、serial rest3→final0，SHA D96A619A、cleanup、
Scene unchanged、Play exited、Console0。full/partial仍31/4，下一结构首差C11 candidate/pass-shape对serial remainder。

C11 placement包`NTSD28-B3-C11-CANDIDATE-BUILD-PLACEMENT-001`已启动：把现有rest prelude、PairVRest和
CandidateCollect作为连续transaction从serial后整体移到C10后；candidate/rest算法差异仍归B5。

C11 placement现已验证：rest→PairVRest→CandidateCollect连续位于C10后、serial前。red2、compile0、focused2、
C04～C11/actual34、related28、03:08:13 SelfCheck与Play均PASS；serial rest0、pair visit5，SHA3C43ADC9、
cleanup PASS、Scene unchanged、Play exited、Console0。算法仍归B5；下一结构首差C12 fusion对serial remainder。

C12 placement现已验证：既有OID51/52 fusion owner已从serial后移到C11后、serial前，OID算法、4500/900 timer
和C25h writer均未改。red2、compile0、focused2、C04～C12/actual36、OID+C12 6、03:47:14 SelfCheck
与targeted Play均PASS；真实tick6 route frame10→9/state2后融合为OID51/frame290，serial观察51，timer4499、
partner dormant，SHA FFAD6915，cleanup PASS、Scene unchanged、Play exited、目标Play Console0。初次Play的
synthetic running frames 9～12缺失已作为fixture问题修正并留档。full/partial仍31/4；下一结构首差C13
active weapon count相对serial remainder。

C13 placement包`NTSD28-B3-C13-ACTIVE-WEAPON-COUNT-PLACEMENT-001`已启动：Authority exact扫描仅计active
current definition type1/2/4/6；Unity当前仅在CharacterHit后的random-drop例外里临时统计所有non-character。
本包新增C12后/serial前的authority只读快照，但不改变用户批准保留的drop位置、RNG、候选、门槛或生成行为。

C13 placement现已验证：独立snapshot位于C12后、serial前并只计active current-DAT type1/2/4/6；
random-drop例外正文与all-non-character门槛保持不变。red6、compile0、focused4、C04～C13/actual40、
related13、warmed4096次0B、04:11:29 SelfCheck及targeted Play均PASS；真实tick6 count4/captured tick6，
SHA2D382636、cleanup PASS、Scene unchanged、Play exited、Console0。full/partial为32/4；下一结构首差为
C14 type-zero hit consume相对serial remainder，具体consumer body/termination/damage仍归B5。

C14 placement包`NTSD28-B3-C14-TYPE0-HIT-PLACEMENT-001`已启动：现有PostInteraction caller已具备current-DAT
type0边界，但production位于serial后；serial audit确认普通type0 shell无业务writer，type3 state/death tail与
state9998 cleanup必须继续留后。本包只前移caller，不改变B5 consumer body、C16或random-drop例外。

C14 placement现已验证：现有type0 caller位于C13后、serial前；consumer body与C16/drop未改。red2、compile0、
focused2、C04～C14/actual42、hit related196、04:33:34 SelfCheck及targeted Play均PASS；真实tick6
serial/final hit execution count3，phase14～18为C13/C14/serial/drop/C16，SHA3695117E、cleanup PASS、
Scene unchanged、Play exited、Console0。full/partial仍32/4；当时下一结构首差为Authority C15 drop对残留serial。

后续`NTSD28-B3-RESIDUAL-SERIAL-TAIL-REHOME-001`已闭合该首差：serial caller从C14/C15之间原样后移到
current C20后，C15例外及serial/type3/state9998 body均未改。red2、compile0、focused2、C04～C14/actual44、
related289、05:07:12 SelfCheck与真实Play均PASS；tick6 hit3、RNG5→6/serial6，phase15/16/17/22/23
为C14/C15/C16/current-C20/serial，SHA E3A0B326、cleanup、Scene unchanged、Play exited、Console0。
full/partial仍32/4；下一结构首差推进到C21/C22 owner相对临时serial proxy。C17～C20算法仍归B6，
C25逐slot事务仍未实施。

`NTSD28-B3-C21-C22-PLACEMENT-001`随后把现有`PreFrameBounds`和`FramePostProcess`移到current C20后、
临时serial前；Stage波次不冒充C21，bounds/impulse算法不改。red2、compile0、focused2、C04～C22/actual46、
related21、05:28:56 SelfCheck与真实Play均PASS；slot50 X=-200→-100、Vx10、accumulators0，phase23～27
为C21/C22/serial/Stage/Render，SHA2E3A6570、cleanup、Scene unchanged、Play exited、Console0。
full/partial仍32/4；下一结构首差为C23/C24 single owner和C25逐slot tail。

`NTSD28-B3-C23-C24-WORLD-CLOCK-001`已建立Unity-owned C23 phase12/phase3与C24 sequence single owners，
位置为C22后、临时serial前，并纳入reset、core snapshot6、full snapshot8、checksum11、restore/parity。
red20、compile0、focused14、snapshot/checksum78、C04～C24/actual51、06:01:19 SelfCheck与真实Play均PASS；
tick6为5→6/2→0/5→6，phase24～27正确，SHA186F7F02、Scene unchanged、Play exited、Console0。
full/partial=34/4；下一结构首差仅剩C25 nested live-slot tail。

`NTSD28-B3-C25-WRITER-INVENTORY-AUDIT-001` 随后已把 C25a～p 的 Authority slot writer 与 Unity
`SerialTickAll`、`BattleLateEntityLifecycleModule`、`EntityPostFrameTailAll`、structural/slot visibility
逐项闭合，详见 `docs/ai/MANIFESTS/NTSD28-B3-C25-WRITER-INVENTORY.md`。审计确认 dynamic cursor
基础存在，但三个 global scan、early lifecycle、loop-end flush 与 Render-before-tail 都不能视为 C25。
`NTSD28-B3-C25-NESTED-TAIL-SKELETON-001` 已把 normal production entry移到C24后，并将legacy serial
显式后置、Render移到session tail/Results后；真实Play phase25～33和publishedTick6通过。
`NTSD28-B3-C25A-B-DEFINITION-CLONE-001`进一步闭合production state8000 definition core和locked
state9996五分身：native synchronized 34 callsites、legacy RNG 0、birth fields与dynamic newborn visibility均通过
focused/SelfCheck/Play；target definition stats仍留B11。C25c-e字段审计随后发现current_mp的Runtime.MP/Health.PP
绑定冲突、C25d 8 carrier缺失及max_mp/cmp/chp schema缺口；下一先闭合current MP binding，再做carriers/B11/algorithm，之后F-J/K-P。

## 7. 当前结论

- 已观察事实：Authority和Unity实际生产顺序存在系统性结构差异，不是局部phase rename。
- 已观察事实：Unity具有可复用的升序slot traversal、B2两遍输入、Host worker boundary与ECS互斥writer seam。
- 已观察事实：Unity current presentation capture早于多个逻辑tail，不能代表2.8 completed-tick snapshot。
- 推断边界：具体角色/武器/碰撞结果会如何分叉，要由B4～B8同seed/input/tick trace证明；本审计不把静态
  顺序差直接扩大为每个场景都已实测失败。
- 下一步：`NTSD28-B3-EXIT-GATE-AUDIT-001`已裁决B3 placement足以进入B4，但并非B3 complete。C25后legacy serial的special state/death/snapshot/state9998与global post-tail的F7/carrier cleanup必须随B4/B5/B7/B8接管后删除；C25k/n/o与内容/RNG/声音继续由B7/B10/B11/H闭合。C15随机掉武器例外不得删除或改写。
