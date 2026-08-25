# R1-SOURCE-007 — 子流程差异与分层验收矩阵

> 状态：COMPLETED（静态 inventory matrix；没有执行任何 fixture）。  
> Authority：J:\QQFile\NTSD2.4\ntsd_release 的正式 release live source。  
> 作用：把每一个已登记 D-ID 连接到 source evidence、依赖、future code package 与验收层级。

## 1. 统一验收层级

| 层级 | 允许证明什么 | 不能证明什么 |
|---|---|---|
| S0：source review | C++ live contract、Unity 当前 callsite、静态字段差异、approved adapter。 | 实际运行行为、帧号、视觉或性能。 |
| S1：focused code fixture | 特定 writer / pass / slot / frame / field 的 Unity 修复结果。 | 全场景表现或 C++ runtime trace。 |
| S2：joint integration fixture | producer -> consumer 跨 pass 的 tick 序列、slot/newborn/relationship、fallback/optimized result。 | 真实玩家输入手感、GPU 像素。 |
| S3：Unity compile + BattleRuntimeSelfCheck | 当前脚本编译与既有断言覆盖。 | C++ release 对齐的充分证据。 |
| S4：Play Mode | 用户报告的角色、按键、实体、场景的实际行为和可观察表现。 | 未覆盖路径的完整对齐。 |
| S5：C++ Release trace | 同 seed / input / initial fixture 下的 C++ -> Unity first difference。 | 当前不可取得；R1-WP02 BLOCKED。 |

除非某个 item 达到其规定的层级，状态必须保留为“待处理”“待测试”或“UNKNOWN”。

## 2. 主调度与 pass 边界

| ID | C++ source contract | Unity current coordinate | 依赖 / future owner | 最小验收 | 当前证据 |
|---|---|---|---|---|---|
| D-SCHED-001 | game_tick.cpp:1821-1825；CPoint/weapon sync 在 object consume 后。 | NTSDBattleTickSystem 313-334；SimulationWorld.Passes 2226-2345。 | R2-PASS-01，后接 R4/R5。 | held/candidate/object/CPoint same-tick order fixture。 | S0 only。 |
| D-SCHED-002 | game_tick.cpp:1827-1846；positive link 在 CPoint/weapon sync 后。 | NTSDBattleTickSystem 316-318、474-476。 | R2-PASS-01，R5-REL-02。 | invalid/valid link field read before second held pass。 | S0 only。 |
| D-SCHED-003 | game_tick.cpp:1848-2019；second Z clamp 后才有 held#2。 | NTSDBattleTickSystem 319-324；SimulationQueryAndLinkModule 39-89。 | R2-PASS-01，R5-REL-02。 | first-held -> CPoint/link -> second-held fixture。 | S0 only。 |
| D-SCHED-004 | game_tick.cpp:1441-1643 与 1860-2018；两轮 negative held。 | 当前只有一处 HeldObjectProcessAll。 | R2-PASS-01，R5-REL-02。 | 两轮有不同 observable write 的 relation fixture。 | S0 only。 |
| D-SCHED-005 | main.cpp:4566-4608、5505-5522；完整 input callback 在 T03 前。 | NTSDBattleTickSystem 257-296。 | R3-INP-01。 | OID7/8/51 + human/AI same-tick input journal。 | S0 only。 |
| D-SCHED-006 | game_tick first/second Z clamp；double -> int 与 entity filter 未完全闭合。 | BattleEcsCharacterStageZPass、StageBounds calls。 | R2-PASS-02。 | character-DAT、newborn、second clamp write fixture。 | UNKNOWN；不可先改。 |
| D-SCHED-007 | T10 candidate collection -> T11/T13 consume。 | CollisionSnapshot + PairVRest + candidate collection。 | R2-PASS-02，R4-COL-01。 | full slot scan 与 broadphase 的 candidate list/order comparison。 | adapter mapping；runtime pending。 |
| D-SCHED-008 | C++ candidate carrier release 时点未完整闭合。 | EndCollisionCandidateConsumption。 | R2-PASS-02，R4-COL-01。 | consume end -> next candidate pass fixture。 | UNKNOWN。 |
| D-SCHED-009 | game_tick preframe/stage/render 在 postprocess 前。 | RenderDispatch 位于 CurrentWaveStage 后、FramePostProcess 前。 | R6-PRES-01/02。 | frozen frame tick/order command fixture。 | S0 mapping；runtime pending。 |
| D-SCHED-010 | game_tick:994-1005、2066-2077；F1/F2 gate 不是 entry clear。 | NTSDBattleTickSystem 257-291；SimulationTickDriver 1213。 | R3-INP-02。 | F1 wait、F2 one-step、entry clear 三组。 | source/preflight PASS；script pending。 |
| D-SCHED-011 | game_tick mode2 tail → entity postframe tail 后才清 global flags；`g_init_stats` / F7 与 results/ECS shadow adapter 仍未闭合。 | NTSDBattleTickSystem tail / `ClearMode2RequestAfterPostFrameTail` / finally。 | R2-SCHED-002 / R7。 | tail flag/result/ECS shadow next-tick fixture。 | mode2 reset 代码 + compile/self-check PASS；joint / Play Mode / trace pending。 |
| D-SCHED-012 | C++ fixed MAX_OBJECTS slot cursor；fixed count不是 production capacity rule。 | RuntimeSlotTable / Registry / Extended profiles。 | R5-LIFE-01。 | Authority400 slot cursor + extended slot reuse fixture。 | capacity is A-RENDER-004; cursor pending。 |

## 3. 输入、AI、frame 与物理

| ID | C++ source contract | Unity current coordinate | 依赖 / future owner | 最小验收 | 当前证据 |
|---|---|---|---|---|---|
| D-INP-001 | apply_input 没有 negative-link 整段 return。 | LF2Character RunCharacterInputPhase base shell。 | R3-HOLD-INP-01，R5-REL-02。 | negative held/caught input, combo, holder/target fixture。 | S0 only。 |
| D-INP-002 | active character DAT AI caller 不先按 self HP return。 | AiInput.partial 1441-1442。 | R3-AI-LIFE-01，R3-FRAME-01。 | HP=0 / respawn boundary AI key/history fixture。 | S0 only。 |
| D-INP-003 | C++ logical input includes current/prev/cd/history per tick。 | FrameInputSet edges + ApplyFrameInputSet held reconstruction。 | R3-INP-03A。 | press/hold/release/multi-key journal fixture。 | source/static/compile+self-check PASS; physical/trace pending。 |
| D-INP-004 | C++ P1/P2 polling；Unity supports 8 local slots。 | roster/provider mapping。 | R3-INP-04。 | fixed P1/P2 fixture; 3+ is Unity extension only。 | approved extension; Play Mode pending。 |
| D-INP-005 | nearest target equal distance picks low slot。 | brute path preserves tie; optimized path unclosed。 | R3-AI-TGT-01 / R7。 | equal ground/air target fallback vs optimized fixture。 | source mapping; runtime pending。 |
| D-INP-006 | physical keys come from C++ InputConfig/SDL。 | CharacterInputModule logical crossing; asset/Inspector not audited。 | user Play Mode / R3-PHY-01。 | W/S/A/D/J/K/L final binding check。 | UNKNOWN; non-script boundary。 |
| D-MOV-001 | frame_advance/frame_tick reads this-tick key。 | SerialTickAll clears current key before frame advance。 | R3-FRAME-01, R3-INP-01/02。 | walk/run/jump/late frame key lifetime fixture。 | S0 only。 |
| D-MOV-002 | physics landing branches raw-write frame only。 | ImmediateFrame changes PN/attacking/sprite/transistor。 | R3-FRAME-01, R4-HIT-02, R5-REL-01。 | each landing type frame/Prev/Prev2/wait fixture。 | S0 only。 |
| D-MOV-003 | integer sync only after successful C++ physics tail。 | respawn path synchronizes all active runtime first。 | R3-FRAME-01 / R5-LIFE-01。 | delay/link/kind2 early return + respawn coordinate fixture。 | S0 only。 |
| D-MOV-004 | C++ live source has no known ThrowFrameGuard read counterpart。 | frame advance/frame tick guard uses ThrowFrameGuard. | R3-FRAME-02。 | production nonnegative writer/reachability audit then fixture。 | static extra gate; reachability UNKNOWN。 |
| D-MOV-005 | frame_advance state2000 facing follows Vx。 | exact-character data-oriented FrameTick omits legacy branch。 | R3-FRAME-02。 | DAT reaches state2000 + Vx direction fixture。 | S0 only; DAT reachability pending。 |

## 4. Candidate、hit、抓取与武器

| ID | C++ source contract | Unity current coordinate | 依赖 / future owner | 最小验收 | 当前证据 |
|---|---|---|---|---|---|
| D-COL-001 | collision.cpp:65; hit_confirm2 on character target aborts entire attacker sequence。 | unified candidate runner does not read attacker HitConfirm2。 | R4-COL-01。 | Loop1 weapon/special -> Loop2 object candidate fixture。 | S0 only。 |
| D-COL-002 | collision.cpp:69-79; caught CPoint/hurtable gate occurs before every kind dispatch。 | helper exists but unified runner lacks universal boundary。 | R4-COL-01 + R5-REL-01/02。 | caught relation for grab/pickup/kind6/character fixture。 | S0 only。 |
| D-COL-003 | collision.cpp:188-194; effect21 + current state18/19 aborts attacker sequence。 | collect-time prev-state filter only。 | R4-COL-01。 | multi-candidate current-state fixture。 | S0 only。 |
| D-COL-004 | collision_collect has no global oid999 pure-transition-smoke exclusion。 | IsPureTransitionSmoke extra gate。 | R4-COL-02, R5-LIFE-01。 | oid999 DAT/lifecycle reachability fixture。 | static branch gap; DAT pending。 |
| D-COL-005 | only kind3/kind8 have explicit character-only target source gate; kind1 can reach common grab consume。 | Unity restricts kind1 and kind3 to character target。 | R4-COL-02, R5-REL-01。 | kind1 multi-type target fixture。 | static branch gap; DAT pending。 |
| D-HIT-001 | normal type3 kind0 path writes HP/HP max/combo/damage stats。 | ApplySpecialAttackDamage only motion/effect tail。 | R4-HIT-01。 | type3 nonlethal/lethal vital/stat/death fixture。 | S0 only。 |
| D-HIT-002 | kind10/11, kind16, normal weapon response raw-write frame。 | Immediate/SetFrame helpers add extra state writes。 | R4-HIT-02, R3-FRAME-01。 | frame/history/attacking/wait/late tick fixture per writer subset。 | S0 only。 |
| D-HIT-003 | normal type1/2/4 path writes common vital/stat fields。 | weapon writer only writes durability/reaction subset。 | R4-HIT-01。 | type1/2/4 nonlethal/lethal stat fixture。 | S0 only。 |

## 5. CPoint、held、link、opoint 与 lifecycle

| ID | C++ source contract | Unity current coordinate | 依赖 / future owner | 最小验收 | 当前证据 |
|---|---|---|---|---|---|
| D-LINK-001 | game_tick 1829-1845 invalid positive link clears LinkState only。 | BattleEcsPositiveLinkValidationPass clears TargetSlotIndex and HeldWeaponStableId too。 | R5-REL-02。 | inactive target / mismatch residue consumer fixture。 | S0 only。 |
| D-LINK-002 | game_tick 1450-1457,1866-1872 invalid negative link clears child LinkState only。 | Query/link module also clears HolderStableId。 | R5-REL-02。 | invalid child -> holder lookup/hit propagation fixture。 | S0 only。 |
| D-HOLD-001 | type2 held throw random frame/velocity/link branch does not write frame_delay。 | BattleHeldObjectWriter sets FrameDelay=1。 | R5-REL-02。 | type2 wpoint.dvx throw current/next tick fixture。 | S0 only。 |
| D-HOLD-002 | only type1/4/6 branch writes C++ spawner_slot。 | Unity throw writes SpawnerEntityIndex for type2 too。 | R5-REL-02。 | type2 later spawner consumer fixture。 | S0 only。 |
| D-CPT-001 | cpoint raw frame writes do not write C++ wait_counter。 | CPoint writer ImmediateWaitReset clears FrameWaitCounter. | R5-REL-01。 | broken relation/action/duration expiry frame/wait fixture。 | S0 only。 |
| D-CPT-002 | weapon.cpp:50-75 CPoint injury writes global kill/damage stats。 | ApplyHeldInjury lacks World.KillStats/DamageStats writes。 | R5-REL-01, R4-HIT-01。 | CPoint lethal/nonlethal stat fixture。 | S0 only。 |
| D-OP-001 | Entity.reset gives Prev2=0, spawn only sets current action frame。 | character/weapon opoint init writes Prev2=action。 | R5-LIFE-01。 | nonzero action child next collision/CPoint history fixture。 | S0 only。 |
| UNKNOWN-LIFE-001 | C++ auxiliary relation counterpart for Unity TrackerFlag/TrackerParent not closed。 | kind2 opoint writes; kind5 read path exists。 | source-follow-up before touching those fields。 | source consumer coordinate + joint fixture。 | UNKNOWN; no code patch allowed。 |
| UNKNOWN-LIFE-002 | C++ active=false + next reset equivalence to PendingFlushDestroy/generation/pool not proven。 | registry/pool lifecycle adapter。 | R5-LIFE-01。 | lower/higher slot newborn, free/reuse/visibility fixture。 | UNKNOWN / adapter。 |

## 6. Render handoff、visibility 与中央表现

| ID | C++ source contract | Unity current coordinate | 依赖 / future owner | 最小验收 | 当前证据 |
|---|---|---|---|---|---|
| D-RENDER-001 | renderer.cpp active entities directly enter blit loop。 | CentralOnly requires feature/material/camera/catalog/backend ownership and fail-closes. | R6-PRES-01。 | valid entity + feature absent + resource absent + unresolved sprite + valid route diagnostic fixture。 | S0 only。 |
| D-RENDER-002 | renderer.cpp advances/removes spark records during pre-postprocess render callback。 | BattlePresentation freezes then SimulationTickDriver/worker finalizes later。 | R6-PRES-02, R4-HIT-01。 | spark age/expiry and next logic tick read fixture。 | static timing gap; runtime pending。 |
| D-RENDER-003 | C++ selection begins at active only。 | Unity capture skips OidMergeDormant, PendingFlushDestroy, FirstPresentationTick, invalid handle。 | R5-LIFE-01 then R6-PRES-01。 | pending destroy / slot reuse last-visible command fixture。 | S0 only。 |
| D-RENDER-004 | shadow 223/224 gate reads char_data->oid。 | Unity shadow checks ObjectId while body uses CurrentDatObjectId。 | R6-PRES-02。 | dynamic identity / 223-224 shadow fixture。 | static identity gap; runtime pending。 |
| D-RENDER-005 | C++ only has its frame/link/hit-stop/OID body/shadow gates。 | Unity snapshot additionally consumes EntityVisible/ShadowVisible. | R6-PRES-01。 | Hide/HideShadow/death blink/pool reuse/hit-stop fixture。 | static extra gate; reachability pending。 |

## 7. Approved adapter acceptance, not repair items

| ID | Must remain true | Required negative acceptance |
|---|---|---|
| A-RENDER-001 | CentralOnly, Texture2DArray/atlas, dynamic Mesh, URP remain production pixel path. | No R2-R7 patch may re-enable production Legacy SpriteRenderer fallback. |
| A-RENDER-002 | BattleVisualScale remains 1.5 and held attachment compensation remains explicit. | No fix may change scale to 1 or remove compensation to match a raw C++ coordinate. |
| A-RENDER-003 | Unity fixed-world battle logic camera remains separate from display-only BattleCameraSafeArea. | No fix may write camera/Transform interpolation into runtime X/Y/Z or reintroduce C++ camera_x as logic truth. |
| A-RENDER-004 | Authority400 remains fixture-only; MobileExtended/DesktopExtended remain production profiles. | No fix may impose a 400 active production cap or remove desktop growth. |

## 8. R7 performance re-certification discoveries

| ID | C++ source contract | Unity optimized route | Future owner | 最小验收 | 当前证据 |
|---|---|---|---|---|---|
| D-PERF-001 | T14在object consume后从当前active/frame/Prev2/relation重新执行CPoint与weapon-sync。 | death cleanup后缓存cross-pass no-op，仅以occupancy/pending结构量复核；同slot内容变化不会失效。 | R7-PERF-001（R6-PRES-005 fresh验收后） | proof发布后同slot切kind2，cached vs forced-current-point oracle；neutral/0B control。 | SOURCE_CONFIRMED_DIFFERENCE；实现尚未开始。 |
| D-LATE-001 | late state-special按9995→4000→8000 reload chain，随后9996/attacking1生成4×217+1×218并按slot/RNG顺序初始化。 | transform分支提前return；state9996被exact skip且无writer；GT-11错误期望零spawn。 | R7-LATE-001（独立structural package） | direct9996、transform-chain、missing OID、slot exhaustion、34 RNG calls、newborn visibility。 | SOURCE_CONFIRMED_DIFFERENCE；实现尚未开始。 |

## 9. Global change protocol for every future code package

Before code:

1. create docs/ai/CHANGE-RECORDS/<ChangeId>.md with scope, authority coordinates, risk,
   rollback and acceptance matrix row IDs;
2. mark that Change ID in CHANGE-LEDGER.md and STATE.md;
3. list every existing user worktree change in the handoff; do not overwrite it.

After code:

1. perform narrow static/code fixture checks first;
2. run Unity compile and focused tests only when the package authorizes them;
3. run BattleRuntimeSelfCheck only after the relevant contract can reach it;
4. record actual command/result, unrun checks and remaining S4/S5 evidence gaps;
5. run Tools/Validate-ChangeLedger.ps1 before describing a script change as complete.

No result from self-check, checksum, performance profile, central draw count or old C# parity alone may
close a C++ Release source discrepancy.
