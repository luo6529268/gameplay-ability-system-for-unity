# R1 — C++ Release → Unity 全量差异总登记册

> 状态：**STATIC INVENTORY COMPLETE / RUNTIME ACCEPTANCE PENDING**。  
> 目的：把分散在 R1-SOURCE-001～007 的发现汇总为唯一可追踪索引；先全量盘点，后按
> 依赖分模块修复。  
> 行为 authority：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live source。  
> 重要限制：本登记册是静态 source inventory，不是 C++ executable trace、Play Mode
> 验收，也不是“Unity 已完整对齐”的声明。

> 2026-08-23 correction：表内`D-TEST-002`的“unmodified”状态已由`R7-TEST-002`取代；
> 当前状态为`CLOSED / VERIFIED TEST-ONLY`。worker fixture现断言current Left/Attack=1、Prev=0，
> exact 1/1、class 17/17、compile 0 error、02:14:36 fresh self-check PASS；production input未改。

> 2026-08-23 R8-WP01G synthesis：当前68个D-ID已完成无遗漏分类，集合校验为register=68、
> synthesis=68、missing=0、extra=0。20项在限定范围内关闭/具明确Unity证据，20项代码差异已关闭或具
> 高层Unity证据但仍受trace/样本限制，19项代码已写/映射但Unity joint/Play待证，7项source/reachability
> 仍UNKNOWN/INFERRED，2项为批准adapter/未来配置决策。当前没有“source-confirmed + Unity未修复 +
> production可达性已闭合”的可直接代码项；详见`R8-WP01G-certification-synthesis-20260823.md`。

> 2026-08-24 R8-WP01G-R09 final reconciliation：上述2026-08-23分类为历史快照，已由R05～R08-R04后
> 的最终对账取代。当前分类为43项Unity S4/runtime覆盖、5项exact production witness不可得、1项source等价但
> full trace缺失、9项用户排除/未来替换、1项调试功能键policy、3项approved adapter/config decision、6项
> test/worker/performance事实；合计68、missing0、extra0、duplicate0。正常战斗主线没有新增的
> source-confirmed + production-reachable + Unity-unimplemented脚本差异；R1-WP02 full trace仍BLOCKED。

## 1. 使用规则

- 每一项必须有 C++ source 坐标、Unity source 坐标、状态、后续拥有者和最小验收。
- `待处理` 仅表示 C++ contract 与 Unity **当前代码**已出现可确认差异；不等于已知
  用户可见错误，也不等于可立即修复。
- `逻辑已映射，待测试` 表示静态结构能对应，但仍无 C++ runtime / Unity joint evidence。
- `UNKNOWN` 是合法结果；不能被旧 C#、self-check、checksum、Authority400、性能结果或
  主观表现补成 VERIFIED。
- 改 code 前必须先在本登记册把 item 关联到独立 Change ID；脚本改动还必须遵守
  `docs/ai/CHANGE-LEDGER.md` 和 `Tools/Validate-ChangeLedger.ps1`。

## 2. 不可回退 Unity 交付边界

以下不是差异项，任何后续修复都不得通过回退它们来“变绿”：

- 中央表现：`BattleCentralRenderSystem`、central command/descriptor、`CentralOnly`、
  Texture2DArray/atlas、dynamic Mesh/quad、URP；不得恢复逐实体 production
  `SpriteRenderer`。
- 容量：Authority400 仅是 C++ 对照 profile；MobileExtended 维持 1,050 initial slots /
  1,000 active；DesktopExtended 维持动态增长且没有 production active hard cap。
- 模拟：`SimulationTickDriver → NTSDBattleTickSystem → SimulationWorld`、30 Hz、
  `FrameInputSet`、slot/generation、SoA/ECS store、object pool、worker、battle zero-GC
  目标仍有效。
- T8 默认 `stage.dat` 部署持续暂缓。

## 3. 盘点覆盖进度

| Work Package | C++ source 领域 | Unity crosswalk | 状态 |
|---|---|---|---|
| R1-SOURCE-001 | `game_tick(...)` 主 pass sequence | `NTSDBattleTickSystem` | 完成（静态） |
| R1-SOURCE-002 | callback / human / AI / combo / F1-F2 | FrameInput / HumanInput / CharacterInput | 完成（静态） |
| R1-SOURCE-003 | frame advance / physics / movement / landing / state/death | FrameAdvance / late tick / stage clamp | 完成（静态） |
| R1-SOURCE-004 | candidate / collision / hit / grab / weapon consume | snapshot / broadphase / consume | 完成（静态） |
| R1-SOURCE-005 | CPoint / held / link / opoint / lifecycle | structural writer / pool / relations | 完成（静态 source） |
| R1-SOURCE-006 | render handoff / ordering / visibility | central presentation | 完成（静态 source） |
| R1-SOURCE-007 | 汇总、依赖图、修复批次与验收矩阵 | 全局 | 完成（静态 source inventory closure） |

## 4. 当前已登记项

| ID | 领域 | 状态 | C++ → Unity 差异摘要 | 详情 / 下游 |
|---|---|---|---|---|
| D-SCHED-001 | 主调度 | `RUNTIME_PENDING / WP01C-03 UNITY S4 PASS` | C++ CPoint/weapon sync 在 object collision 后；Unity已调整。03 live四pass表证明first-held无damage、PreInteraction唯一写injury/stat，后续不重复；collision consume前置仍归04。 | R1-SOURCE-001；R2-SCHED-001；`RESEARCH/R8-WP01C-03-grab-cpoint-link-held-injury-runtime-evidence-20260823.md`；C++ full trace/04 collision joint仍待。 |
| D-SCHED-002 | 主调度 | `RUNTIME_PENDING / WP01C-03 UNITY S4 PASS` | C++ positive-link validation 在 CPoint/weapon sync 后；03 pass table验证PreInteraction写入后positive-link不重复/改写grab injury。 | R2-SCHED-001；03 runtime evidence；C++ full trace待。 |
| D-SCHED-003 | 主调度 | `RUNTIME_PENDING / WP01C-03 UNITY S4 PASS` | C++ second held loop 在 CPoint/positive-link 与 second Z clamp 后；03第二held观察点保持injury/stats不重复。 | R2-SCHED-001；03 runtime evidence；C++ full trace待。 |
| D-SCHED-004 | 主调度/held | `RUNTIME_PENDING / WP01C-03 UNITY S4 PASS` | C++两轮negative-held；03 invalid-negative residue在first/second两观察点证明只清LinkState并保留HolderStableId。 | R1-SOURCE-005；R2-SCHED-001；03 runtime evidence；C++ full trace待。 |
| D-SCHED-005 | 输入/主调度 | `UNITY NORMAL JOINT S4 PASS / C++ TRACE BLOCKED` | C++ callback内所有input完成后才T03；Unity normal path为HumanInput→CharacterInput→OID maintenance。R03 current physical DDJ/DRA/DK从InputSystem经FrameInputSet进入combo/movement，F3 interaction随后正常运行，证明normal joint接线；entry-clear特殊分支仍以self-check为证。 | `CHANGE-RECORDS/R3-INP-001.md`；`RESEARCH/R8-WP01G-R03-joint-runtime-evidence-20260823.md`。 |
| D-SCHED-006 | Z clamp | `SOURCE-CLOSED / EQUIVALENT WITH APPROVED CAPACITY ADAPTER / RUNTIME_PENDING` | C++两处均按active+current character DAT固定slot升序，只写double Z clamp与截断ZInt；Unity两次pass时点、current-DAT gate、Z/ZInt writer和fresh slot读取一致。pending/dormant映射C++ inactive；高槽扫描是批准的扩展容量adapter。无C++ runtime trace，不能升级为runtime VERIFIED。 | `RESEARCH/R8-WP01G-R01-r2-scheduler-source-reachability-20260823.md`；R1-WP02仍BLOCKED。 |
| D-SCHED-007 | candidate adapter | `UNITY JOINT S4 PASS / C++ TRACE BLOCKED` | Unity snapshot/pair-vRest、role-aware/store adapter保留。fresh formal/shadow/consume 252项PASS；live 10-candidate Play通过。50-AI current 35/35 store+oracle、mismatch/invalid/fallback=0，20项final hash与forced-legacy全等，0 GC且cleanup restored。 | `RESEARCH/R8-WP01G-R05-candidate-preinteraction-joint-evidence-20260823.md`；R1-WP02 full trace仍BLOCKED。 |
| D-SCHED-008 | candidate lifecycle | `USER-EXCLUDED F1/F2 DEBUG-STEP PATH / SOURCE DIFFERENCE RETAINED / NOT NORMAL-COMBAT BACKLOG` | normal completed tick无consume-end→tail reader，Unity提前失效adapter range在结果上等价；差异只在F1/step-wait render后early return跳过tail时出现。用户明确不需要F1/F2战斗调试步进，因此不实现carrier跨debug wait保留；source差异历史保留，不能误写为代码等价。 | `RESEARCH/R8-WP01G-R01-r2-scheduler-source-reachability-20260823.md`；用户F1/F2排除决定；`RESEARCH/R8-WP01G-R09-final-evidence-reconciliation-20260824.md`。 |
| D-SCHED-009 | render handoff | `UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED` | R07A production worker Play用actual kind0 producer证明PreFrame/Stage后的RenderDispatch在FramePostProcess/Late前完成frozen publication与live writeback；phase diagnostics tick843～846闭合，Late不重复推进。 | `RESEARCH/R8-WP01G-R07A-render-writeback-joint-runtime-evidence-20260823.md`；compile0、worker18/18、hit178/178、central13/13、20:25:11 self-check PASS；C++ full trace仍BLOCKED。 |
| D-SCHED-010 | F1/F2 gate | `USER-EXCLUDED DEBUG-STEP PATH / EXISTING CODE RETAINED / NOT NORMAL-COMBAT BACKLOG` | C++ F1/F2 wait 与 Unity NeedClearInput 的 early-return边界已有历史实现和self-check，但用户明确不需要F1/F2战斗调试步进；现有BattleStepMode/Gate代码不在本轮回退，也不再要求Play或C++ trace认证。 | `TASKS/R3-INP-02-step-gate-vs-entry-clear.md`；`CHANGE-RECORDS/R3-INP-002.md`；用户F1/F2排除决定；R09 reconciliation。 |
| D-SCHED-011 | tick tail | `UNITY PRODUCTION PLAY S4 PASS / MODE-CONFIGURED F7-F9 VERIFIED / C++ FULL TRACE BLOCKED` | C++ mode2 tail→entity postframe→clear flags已闭合。`R8-FUNCTIONKEYMODE-001`增加GameConfig exact 0/1白名单、LocalFreeRun-only edge latch与tick边界消费；F7写HP3/HPBound/HP/PP=500，F8/F9复用Mode2 request1/2。Play tick1581/1582/1583通过，request/cleanup通过。 | `TASKS/R8-WP01G-R12-mode-configured-function-keys.md`；focused4/4、snapshot18/18、12:28:41 self-check PASS；C++ full trace仍BLOCKED。 |
| D-SCHED-012 | capacity/slot | 已批准adapter（容量）/`R5-LIFE-001A/01B RUNTIME_PENDING`；R8 low-band cursor S4 PASS | C++ fixed MAX_OBJECTS不成为Unity production cap；slot50 lowest-free、Authority400 cursor、extended lower-hole-first与Mobile/Desktop >399 high/low joint cursor矩阵通过。R8 production Play补证producer52→child53 same-pass、producer53→child52 next-pass和same-slot generation reuse；extended >399真实Play仍待。 | `RESEARCH/R5-LIFE-01A-extended-slot-cursor-preflight-20260822.md`；`RESEARCH/R5-LIFE-01B-pending-free-generation-render-preflight-20260822.md`；`RESEARCH/R8-WP01C-01-opoint-lifecycle-runtime-evidence-20260823.md`；C++ trace/>399 Play待验。 |
| D-CAP-001 | DesktopExtended battle-time capacity | `CLOSED / CONTRACT CLARIFIED / NO CODE` | Desktop没有固定产品级active cap；每局在unsealed loading/reset/preflight边界准备有限、按页归一化容量。`BeginBattleAllocationSeal`后容量冻结以保证strict 0 B，超预算确定性拒绝。默认512是reservation hint，不是产品硬上限；tick内实时无限增长不属于合同。 | `TASKS/R7-CAP-01A-desktop-capacity-zero-gc-admission-contract.md`；fresh capacity/pool 44/44与03:19:45同域self-check PASS。现有production符合，R7-CAP-01B不需要实施；Windows Player >512归R8。 |
| D-INP-001 | input / held | `SOURCE-CLOSED / NATURAL TYPE0 PATH IS AI-DEFERRED / NO NON-AI PLAY BACKLOG` | C++/Unity均允许negative-link current-character-DAT进入input；Unity整体return已移除并有self-check。正式pickup只对type1/2/4/6写negative link；自然type0 negative-link由opoint kind2生成且明确`ai_controlled=true`。用户已排除C++ AI parity，故不伪造human negative-link Play。 | `CHANGE-RECORDS/R3-HOLD-INP-001.md`；`RESEARCH/R8-WP01G-R06-g2-input-residual-preflight-20260823.md`；C++ full trace仍BLOCKED。 |
| D-INP-002 | AI/death | VERIFIED（Unity production Play S4）；C++ trace BLOCKED | C++ caller / `prepare_ai_input` 不先按 self HP 过滤 AI，且 input 在 death/respawn cleanup 前；Unity三处self-HP global gate由R3-AI-LIFE-001移除。R8-DEATHPLAY-001在live world证明HP=0 AI先完成KeyJump→PrevJump roll/clear，再进入state14 arm、countdown和respawn。 | `TASKS/R3-AI-LIFE-01-dead-respawn-ai-input-eligibility.md`；`CHANGE-RECORDS/R3-AI-LIFE-001.md`；`RESEARCH/R8-WP01C-05-death-respawn-ai-integer-runtime-evidence-20260823.md`；compile、AI85/85、Play、13:52:04 self-check PASS。target-bearing full AI policy与C++ full trace不由此扩大。 |
| D-STEP-001 | function-key debug unlock | `USER-EXCLUDED F1/F2 DEBUG-STEP PATH / SOURCE DIFFERENCE RETAINED / NOT NORMAL-COMBAT BACKLOG` | C++ release main的A→B→C unlock与flag1分支确为release live source；Unity没有该debug producer。用户明确不需要F1/F2战斗调试步进，因此不移植unlock；差异记录保留，不能写成C++/Unity等价。 | `RESEARCH/R8-WP01G-R01B-d-step-debug-unlock-source-policy-20260823.md`；用户F1/F2排除决定；R09 reconciliation。 |
| D-INP-003 | input packet | `UNITY INPUTSYSTEM S4 PASS / C++ TRACE BLOCKED` | Unity application消费full held Buttons并重建edge，与C++ current-held poll一致。R03 current DDJ/DRA/DK报告覆盖multi-key、held、release、key/cd/combo/frame/movement；有限pulse只保证Editor采样，不绕过FrameInputSet。 | `CHANGE-RECORDS/R3-INP-003A-001.md`；`CHANGE-RECORDS/R8-JOINTINPUT-PROBE-002.md`；R03 evidence。 |
| D-INP-004 | input capacity/source | `UNITY INPUTSYSTEM S4 PASS / C++ FULL TRACE BLOCKED` | 8-slot extension保持；Player_2已补Attack/Jump/Defend与numpad1/2/3 exact source，Unity正规生成wrapper。two-human Play经physical device state→action callback→FrameInputSet→roster slot0/1→runtime完成11/11 press/held/release/no-cross，stable100/101保持。 | `CHANGE-RECORDS/R8-P2INPUT-001.md`；`RESEARCH/R8-WP01G-R06-p1p2-physical-input-runtime-evidence-20260823.md`；focused2/2、input47/47、19:37:29 self-check PASS；C++ full trace BLOCKED。 |
| D-INP-005 | AI targeting / sensing | `USER-DEFERRED / FUTURE UNITY STATE-BEHAVIOR TREE / NOT AN ALIGNMENT BACKLOG` | C++ sensing已有历史source/test证据，但用户决定不再以C++ AI算法作为Unity对齐目标，未来改用Unity状态树或行为树。现有DataOriented代码保留，不再为C++ AI parity追加Play或修复。未来实现只需按固定tick输出canonical FrameInputSet。 | 用户2026-08-23范围决定；`TASKS/R8-WP01G-R04-ai-sensing-decision-action-joint-runtime.md`已ABANDONED。 |
| D-INP-006 | physical binding | `SOURCE MAPPING CLOSED / UNITY INPUTSYSTEM S4 PASS / HUMAN-HARDWARE EDGE USER-PENDING` | asset的W/S/A/D/J/K/L正确；C++ `DEFAULT_P1`按internal field order实际把L/J/K写入attack(+D3)/jump(+D1)/defend(+D2)，Unity crossed mapping与之吻合。R03 current fresh InputSystem device-state报告：DDJ L/S/K→frame271、DRA L/D/J→frame263、D/K→Right/Defend→airborne/landing均PASS；有限pulse只修Editor采样可重复性，不绕过CharacterInputModule/FrameInputSet。真实人手键盘、Game窗口焦点与OS硬件edge仍由用户验收。 | `include/input_handler.h:9-16`；`CharacterInputModule`；`RESEARCH/R8-WP01G-R03-joint-runtime-evidence-20260823.md`；`CHANGE-RECORDS/R8-JOINTINPUT-PROBE-002.md`。 |
| D-INP-010 | combo wrapper state persistence | `VERIFIED / C++ SOURCE + UNITY RUNTIME` | C++八方向+DJA combo字段按引用逐wrapper即时写回；Unity已直接`ref input.Combo*`，陈旧transaction-discard oracle已修正。 | compile 0 error；08:37:09 self-check PASS；EditMode47/47；InputSystem real-scene DDJ→271与DRA→263 PASS；validator/diff PASS。R1-WP02 full trace仍BLOCKED，D-INP-006 physical edge独立。 |
| D-INP-007A | AI character decision chain | `USER-DEFERRED / FUTURE UNITY STATE-BEHAVIOR TREE / NOT AN ALIGNMENT BACKLOG` | 历史39-position C++ dispatcher移植与自动矩阵保留，但用户决定未来用Unity状态树/行为树取代，不再要求真实Play与C++ AI decision parity。 | 用户2026-08-23范围决定；R04 ABANDONED；保留canonical FrameInputSet接口。 |
| D-INP-007B | AI RNG / gate | `USER-DEFERRED / FUTURE UNITY STATE-BEHAVIOR TREE / NOT AN ALIGNMENT BACKLOG` | C++ AI RNG/gate顺序不再是Unity未来AI算法的验收条件；现有实现和测试留作历史参考，不继续运行时认证。 | 用户2026-08-23范围决定；R04 ABANDONED。 |
| D-INP-008 | AI optimized data contract | `USER-DEFERRED / FUTURE UNITY STATE-BEHAVIOR TREE / NOT AN ALIGNMENT BACKLOG` | 现有SoA/DataOriented AI数据合同保留；未来状态树/行为树可复用或替换，但必须通过固定tick FrameInputSet接入战斗runtime。 | 用户2026-08-23范围决定；不再以C++ AI数据结构裁决。 |
| D-INP-009 | AI acceptance coverage | `USER-DEFERRED / FUTURE UNITY STATE-BEHAVIOR TREE / NOT AN ALIGNMENT BACKLOG` | 39-position覆盖矩阵保留为历史证据；不再新增C++ AI真实场景验收。未来AI验收将按新的Unity AI设计另立计划。 | 用户2026-08-23范围决定；R04 ABANDONED。 |
| D-PERF-002 | broadphase production deployment | `DECISION CLOSED / RETAIN BRUTEFORCE / FUTURE SWITCH EVIDENCE PENDING` | R7-BROAD-02 fresh parity 88/88与same-domain self-check PASS；synthetic reduction成立，但缺current-build real Brute/Loose A/B、R8 scene parity和real fallback distribution。历史1000-AI harness已是Loose仍未达30Hz。 | 当前不改GameConfig；未来切换必须独立配置Record并补real parity/performance证据。 |
| D-TEST-001 | EditMode isolation | `CLOSED / VERIFIED TEST-ONLY` | R7-TEST-001二分到shared-shadow owner把static `LF2FrameCache.EmptyFrame.state`留为14，并修正unified ascending fixture对该污染的隐藏依赖。两fixture现显式own/restore sentinel，dependent使用canonical post-input refresh hook。 | dependent 1/1、class 66/66、AI 286/286；class/AI后same-domain self-check 03:03:54/03:06:15 PASS，fresh 03:07:32 PASS；production未改。 |
| D-TEST-002 | worker human-input fixture | `VERIFIED stale test expectation / unmodified` | worker exact test在同域与fresh-domain均期待首次Left+Attack后的`KeyLeft/KeyAttack=0`，首断言actual=1；C++ human poll与Unity production均保留本tickcurrent key并只把旧key写入prev。 | 独立test-only WP修正两条current-key断言及注释；不得借测试修正改production input。见`RESEARCH/R7-PRES-WORK-01-frozen-publication-ack-recertification-20260822.md`。 |
| D-TEST-003 | worker/central joint acceptance | `AUTOMATED COVERAGE CLOSED / VERIFIED TEST-ONLY` | R7-TEST-003已用formal driver双tickfixture联合覆盖`buildPresentation=true` publication、CentralOnly exact-tick物化、ack/finalize、next-tick unblock与new frame/generation；frozen publication保持immutable。 | exact 1/1、worker+central 31/31、compile 0 error、02:27:37 fresh self-check PASS；R8真实URP Play Mode与C++ trace仍独立待验。 |
| D-PERF-003 | dedicated worker single-flight | `VERIFIED Unity design/deployment fact` | driver/worker在presentation ack前禁止下一tick；production scene同时设置`maxCatchUpTicksPerFrame=1`，当前相容但不能支持未来同帧多tick pipeline。 | 当前不改；若未来放宽catch-up，先设计多版本frozen frame ownership与ack，再做behavior/0B/pressure验收。 |
| D-PERF-004 | production stress initial-service restart | `CLOSED / R8-PERFBOOT-001 VERIFIED` | request processor曾把Bootstrap初始driver/world partial footprint+missing lazy pool误判为runtime invalid；现以Bootstrap ready事实裁决，初始/restart新Play等待，ready-invalid/previously-healthy仍fail-closed。 | compile0、focused263/263、self-check、同一Combat1000 1000-active/180-tick复跑PASS；pool/Bootstrap/gameplay未改。 |
| D-PERF-005 | current-build 1000 active / 30 FPS certificate | `VERIFIED UNITY EDITOR / R8-WP01E` | Dispersed/Combat各1800 tick正式门均通过visible/main/logic P95、logic0B/0 collection、capacity0、central1 draw与cleanup；Desktop capacity focused299/299。 | Legacy/Data同180-tick 12项hash全等；Player/Android/C++ trace不由此关闭。证据`R8-WP01E-current-build-capacity-performance-evidence-20260823.md`。 |
| D-MOV-001 | frame/physics/input lifetime | `UNITY NORMAL MOVEMENT S4 PASS / C++ TRACE BLOCKED` | C++ frame_advance与late frame_tick仍读本tick key；Unity pre-F03 clear已移除。R03 current D/K trace证明Right held→frame210/211/212→DAT Vx/Vy→airborne→release→landing，同tick key lifetime在普通路径联合通过。 | `CHANGE-RECORDS/R3-FRAME-001A.md`；`RESEARCH/R8-WP01G-R03-joint-runtime-evidence-20260823.md`。 |
| D-MOV-002 | landing / frame history | `ORDINARY LANDING S4 PASS / SPECIAL BRANCHES SELF-CHECK / C++ TRACE BLOCKED` | C++ F04 landing多branch raw写frame；Unity exact/shared已用raw writer并保留PN/wait。R03 current physical jump于tick1077落地，ordinary Y/YInt/frame链PASS；state12/13/18等特殊分支仍由16-case fixture覆盖，current DAT无自然witness时不改DAT。 | `CHANGE-RECORDS/R3-LAND-001.md`；R03 F2 report；C++ traceBLOCKED。 |
| D-MOV-003 | frame advance / integer sync | VERIFIED（Unity production Play S4）；C++ trace BLOCKED | C++ integer position仅在成功physics tail写；F03 early-return不写，F05 no-count respawn读取same-relation character-DAT integer x/z。R3-SYNC-RESP-001移除Unity global sync；R8-DEATHPLAY-001 live Play令ally stale ints=(100,40)/(160,20)、live doubles不同，实际按integer average=(130,30)和两次RNG得到(147,39)。 | `TASKS/R3-SYNC-RESP-01-integer-sync-respawn-contract.md`；`CHANGE-RECORDS/R3-SYNC-RESP-001.md`；`RESEARCH/R8-WP01C-05-death-respawn-ai-integer-runtime-evidence-20260823.md`；compile、Play、self-check PASS。其他direct-position writer与C++ full trace不由此扩大。 |
| D-MOV-004 | frame advance / extra gate | 逻辑已写 / compile+self-check PASS / `R8-WP01C-02 S4 PASS` | complete C++ release field inventory确认 `throw_frame_guard` 没有 conditional read或 nonnegative writer；Unity F03/exact F07/fallback F07 three readers已删除。R8 live四type throw/landing通过，type4 bounce保留attacking sentinel。 | 原R3 evidence；`RESEARCH/R8-WP01C-02-pickup-held-throw-landing-runtime-evidence-20260823.md`；C++ full trace仍BLOCKED。 |
| D-MOV-005 | late frame tick / facing | `UNITY PRODUCTION PLAY S4 PASS / C++ FULL TRACE BLOCKED` | C++ state2000按Vx写facing；exact与fallback通用writer均已闭合。正式DAT不存在type0 authored state2000，因此不伪造该样板；R11使用正式OID150 weapon完整tick验证正Vx→right、负Vx→left并恢复production pool/slot基线。 | `TASKS/R8-WP01G-R11-authored-state-runtime-acceptance.md`；`CHANGE-RECORDS/R8-AUTHOREDSTATE-PLAY-001.md`；Play 12:22:17 PASS；C++ full trace仍BLOCKED。 |
| D-COL-001 | candidate consume / hit-confirm2 | 逻辑已写 / compile+self-check PASS / `R8-WP01C-04 S4 PASS` | C++ attacker hit_confirm2 对 character candidate 执行整 attacker abort；Unity shared runner已按 vrest/current-target recheck → C07-A abort → runtime ITR replacement写入最小 gate，且复用 `true` return为 sequence break。R8 live frozen two-candidate witness确认两target HP均100、writer前整attacker abort。 | 原R4 evidence；`RESEARCH/R8-WP01C-04-collision-hit-damage-runtime-evidence-20260823.md`；C++ full trace仍BLOCKED。 |
| D-COL-002 | candidate consume / caught cpoint | 逻辑已写 / compile+self-check PASS / `R8-WP01C-04 S4 PASS` | C++ caught cpoint/hurtable gate 在所有 kind dispatch 前统一 skip；Unity helper存在且active/prev2字段语义已核对。R8 live frozen first-caught/second-ordinary witness确认hurtable0只使first HP100/vrest0，second继续到HP90/vrest>0。 | 原R4 evidence；`RESEARCH/R8-WP01C-04-collision-hit-damage-runtime-evidence-20260823.md`；C++ full trace仍BLOCKED。 |
| D-COL-003 | candidate consume / effect21 | 逻辑已写 / compile+self-check PASS / `R8-WP01C-04 S4 PASS` | C++ effect21 + target current state18/19在 local kind5/4/9 conversion后终止 entire attacker sequence；R8先collect standing Prev、再把first current切state18，live consume确认first/second HP均100且vrest0，整attacker writer前abort。 | 原R4 evidence；`RESEARCH/R8-WP01C-04-collision-hit-damage-runtime-evidence-20260823.md`；C++ full trace仍BLOCKED。 |
| D-COL-004 | candidate collection / oid999 | **部分闭环**：collection逻辑已写 / compile+self-check PASS / runtime pending；immediate query已拆分 | Unity normal/role-aware frozen collection的 `IsPureTransitionSmoke` extra exclusion已移除；synthetic valid oid999 target/attacker在 brute/role-aware formal collector均按 C++ generic rules记录相同 candidate / RNG。当前 production gated frames无有效geometry仍只是DAT可达性事实。immediate `QueryBodyHits`不再被当作同类gate，已拆入D-COL-004B。 | `RESEARCH/R4-COL-04A-oid999-candidate-collection-preflight-20260822.md`；`TASKS/R4-COL-04A-oid999-candidate-collection-contract.md`；`CHANGE-RECORDS/R4-COL-004A.md`；UnityMCP compile（`error CS`=0）和`Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`（2026-08-22 04:33:40 +08:00）；C++ trace / Play Mode待后续。 |
| D-COL-004B | weapon landing / immediate query | **逻辑已写 / compile+self-check PASS / `R8-WP01C-02 S4 PASS`** | Unity `LF2Weapon.OnLanded()`的额外BDY target scan/direct `Hit`已移除。R8 live registered overlap witness验证landing前后target HP333、HPBound444、frame0均不变，同时四type自身落地写入通过。另一条无production静态caller的`ProcessAttack` immediate scan仍为INFERRED dormant。 | 原R4 evidence；`RESEARCH/R8-WP01C-02-pickup-held-throw-landing-runtime-evidence-20260823.md`；C++ full trace仍BLOCKED。 |
| D-COL-005 | kind1 target/attacker type | **05A Unity S4 PASS；05B代码闭环 / RUNTIME_PENDING；R03 interaction joint PASS** | C++只对kind3/8限制character target；kind1使用generic keys并进入common grab，pickup是kind2/7。05B已把Unity kind1 selector改为generic runtime keys、weapon case1改进generic grab；actual weapon attacker self-check通过。R03 held/grab/CPoint/collision联合Play PASS并恢复基线；block-aware全DAT扫描仍为`itr kind1=0`，故05B exact current-production witness不可得。 | `RESEARCH/R8-WP01G-R02-D-COL-005B-generic-kind1-20260823.md`；`RESEARCH/R8-WP01G-R03-joint-runtime-evidence-20260823.md`；`CHANGE-RECORDS/R8-COL-005B-001.md`；C++ trace BLOCKED。 |
| D-HIT-001 | type3 normal damage | 逻辑已写 / compile+self-check PASS / `R8-WP01C-04 S4 PASS` | C++ type3 kind0 normal path先写 HP/HP max/combo/damage stat，再进入type3 tail；R8 actual SpecialAttack→SpecialAttack object pass得到HP100→90、HPBound100→97、combo10、damage stat+10、vrest3、HitConfirm2=1，且没有type0-only kill增量。 | 原R4 evidence；`RESEARCH/R8-WP01C-04-collision-hit-damage-runtime-evidence-20260823.md`；C++ full trace仍BLOCKED。 |
| D-HIT-002 | Kind10/11、Kind16 / weapon raw-frame writer | 代码闭环到自检 + `R8-WP01C-04 S4 PASS`（kind10及weapon live subset） | R8 actual character kind10 live candidate写frame/runtime182，同时PN41、attacking9、wait73保持；actual weapon object pass也通过既有raw tail并落在frame7。kind11/kind16和其余02B/D分支继续由focused/self-check覆盖，本包不把subset扩大为所有raw分支Play已验。 | 原R4 evidence；`RESEARCH/R8-WP01C-04-collision-hit-damage-runtime-evidence-20260823.md`；其余分支Play/C++ full trace仍待。 |
| D-HIT-003 | type1/2/4 normal damage / raw durability | `R4-HIT-003 + R8-WP01C-04 S4 PASS`（type4 live） | C++ normal kind0先按FallDamageDiv写vital/stat，再按raw injury写durability。R8 actual LF2Weapon(type1)→LF2Weapon(type4) object pass得到HP100→80、HPBound100→94、combo20、DamageStats+20、WeaponFlightCounter100→90和vrest3。type1/2/type6其余matrix仍由focused/self-check覆盖。 | 原R4 evidence；`RESEARCH/R8-WP01C-04-collision-hit-damage-runtime-evidence-20260823.md`；其余type Play/C++ full trace仍待。 |
| D-HIT-004 | weapon common writer / early HitConfirm2 and RelationTeam | `R8-WP01C-04 S4 PASS`（normal type4 timing subset） | C++ `apply_hurt`返回后才由type tail写HitConfirm2/relation。R8 actual type4 weapon victim在scaled vital/raw durability之后得到HitConfirm2=1、vrest3，live field matrix通过。CLR weapon shell + non-weapon current DAT shared-dispatch可达性仍为UNKNOWN，未被本包关闭。 | 原R4 evidence；`RESEARCH/R8-WP01C-04-collision-hit-damage-runtime-evidence-20260823.md`；unknown reachability/C++ full trace仍待。 |
| D-HIT-005 | current-DAT target dispatch / CLR shell priority | `RUNTIME_PENDING`；代码闭环，compile/self-check/focused PASS；R03 hit joint PASS | C++ unified Entity依据当前`char_data->obj_type`处理hit；Unity四类attacker已统一current-DAT-first dispatcher。R03 collision/hit matrix以10 candidates覆盖character/weapon/special、damage/stat、durability、vrest与abort并恢复RNG/rest/sound/global状态；matching CLR壳正常联合路径通过。mismatch current type1/2/4/6、type3、type5的production Play夹具仍不可得且C++ full trace BLOCKED。 | `TASKS/R8-HIT-005-current-dat-target-dispatch-contract.md`；`RESEARCH/R8-WP01G-R03-joint-runtime-evidence-20260823.md`；`CHANGE-RECORDS/R8-HIT-005-001.md`；focused 178/178 PASS。 |
| D-LINK-001 | positive link invalid cleanup | `RUNTIME_PENDING / WP01C-03 UNITY S4 PASS` | C++ invalid positive只清link_state；03 worker-active Play实际得到holder link0、target/held slot3/3、reverse holder-1、target link-5保留；focused8/8。 | R5-LINK-001；03 runtime evidence；C++ full trace待。 |
| D-LINK-002 | negative link invalid cleanup | `RUNTIME_PENDING / WP01C-03 UNITY S4 PASS` | C++两轮invalid negative只清child link；03 first/second held实际为link0/0且HolderStableId4/4保留；focused2/2。 | R5-LINK-002；03 runtime evidence；C++ full trace待。 |
| D-HOLD-001 | held type2 frame delay | `RUNTIME_PENDING + R8-WP01C-02 S4 PASS` | C++两轮held pass先复制holder frame_delay，type2不覆盖；R8 live type1/2/4/6均在held delay7、throw delay9下通过，type2未被改成1。 | 原R5 evidence；`RESEARCH/R8-WP01C-02-pickup-held-throw-landing-runtime-evidence-20260823.md`；C++ full trace仍BLOCKED。 |
| D-HOLD-002 | held type2 spawner | `R5-HOLD-002 / RUNTIME_PENDING + R8 S4 PASS` | R8 live type1/4/6均写spawner=holder slot2，type2保留sentinel8765。 | 原R5 evidence；R8-WP01C-02 runtime evidence；C++ full trace仍BLOCKED。 |
| D-HOLD-003 | held throw picker slot | `R5-HOLD-003 / RUNTIME_PENDING + R8 S4 PASS` | R8 live type1/2/4/6 throw均保留picker sentinel7654，没有恢复Unity extra write。 | 原R5 evidence；R8-WP01C-02 runtime evidence；C++ full trace仍BLOCKED。 |
| D-CPT-001 | CPoint raw frame / wait | `R5-CPT-001 / RUNTIME_PENDING / WP01C-03 UNITY S4 PASS` | C++ raw frame不写wait；03 valid/mismatch/escape live cases的FWC sentinels均保持。 | R5-CPT-001；03 runtime evidence；C++ full trace待。 |
| D-CPT-002 | CPoint injury global stat | `R5-CPT-002 / RUNTIME_PENDING / WP01C-03 UNITY S4 PASS` | 03 live lethal injury得到holder kill/combo=1/30、world kill/damage delta=1/30并在cleanup恢复。 | R5-CPT-002；03 runtime evidence；C++ full trace待。 |
| D-CPT-003 | CPoint reciprocal mismatch control flow | `R5-CPT-003 / RUNTIME_PENDING / WP01C-03 UNITY S4 PASS` | 03 live mismatch跳过action/decrease，但fallback frame0→110、victim132、position140/30、velocity8/-4/-3与throw tail均执行。 | R5-CPT-003；03 runtime evidence；C++ full trace待。 |
| D-CPT-004 | CPoint injury phase ownership | `R5-CPT-004 / RUNTIME_PENDING / WP01C-03 UNITY S4 PASS` | 03四pass表first-held无damage，PreInteraction一次写HP/stat/position，positive-link与second-held无重复。 | R5-CPT-004；03 runtime evidence；C++ full trace待。 |
| D-CPT-005 | CPoint valid decrease-negative escape tail | `R5-CPT-005 / RUNTIME_PENDING / WP01C-03 UNITY S4 PASS` | 03 live duration2→-3、frame0/181、dircontrol right、immediate hit1/knockback4/-3及postprocess hit0/velocity4/-3全部PASS。 | R5-CPT-005；03 runtime evidence；C++ full trace待。 |
| D-OP-001 | opoint initial prev2 | `R5-OP-001 RUNTIME_PENDING` + `R8-WP01C-01 S4 PASS` | C++ reset后opoint child prev_frame2=0且只写current frame=action；Unity four initializer/cache adapter已对齐。R8 live production worker/CentralOnly Play中OID33/120/203/999均birth current/runtime/Prev2=0、type/CLR正确并通过release/generation reuse。 | `RESEARCH/R5-OP-01-opoint-initial-prev2-preflight-20260822.md`；`RESEARCH/R8-WP01C-01-opoint-lifecycle-runtime-evidence-20260823.md`；Play result 09:05:09 PASS；C++ full trace仍BLOCKED。 |
| D-LIFE-001 | oid7/8→51 dormant partner slot | `UNITY JOINT S4 PASS / APPROVED DORMANT ADAPTER / C++ FULL TRACE BLOCKED` | 正式OID7/8/51与完整30Hz tick已完成4500-tick merge→dormant→cooldown split：partner保留原slot10/generation1且不占用新slot，split后7/8恢复原slot/generation、current-half HP/HPBound、frame113/state8；Central merged body、dormant suppression、split双body与generation-safe cleanup均PASS。 | `TASKS/R8-WP01G-R08-oid5152-merge-split-central-runtime.md`；`Temp/NTSD_R8_WP01G_R08_Oid5152MergeSplit.result.json`；`R8-MERGESPLIT-001`；C++ full trace仍BLOCKED。 |
| D-RENDER-001 | CentralOnly fail-closed ownership | `UNITY S4 VERIFIED TO AVAILABLE EVIDENCE / COLD SELFCHECK PASS / C++ FULL TRACE BLOCKED` | R07C repair后真实URP Play三态均259px、hash `AE3AFF1E932B491E`一致、Central owner/Legacy suppressed；current214/214/gen216→stale215/214/gen216→replacement215/215/gen217，lease/retire/checksum/cleanup PASS。normal Play Camera enabled/Console0；原B-R8-R07C-01由首次seal清退旧publication关闭。cold只由exact self-check覆盖。 | `RESEARCH/R8-WP01G-R07C-centralonly-failclosed-urp-runtime-evidence-20260823.md`；`CHANGE-RECORDS/R8-CENTRALSEAL-001.md`；C++ full trace保持BLOCKED。 |
| D-RENDER-002 | hit spark presentation writeback 时点 | `UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED` | R07A worker Play由4组正式collision/hit pair跨tick产生record：published frozen ages `[0]`→`[1,0]`→`[2,1,0]`，live写回`[1]`→`[2,1]`→`[3,2,1]`，每tick exact2 RNG；CentralOnly no-publication保持旧cycle并推进live为`[4,3,2,1]`，Late幂等。 | `RESEARCH/R8-WP01G-R07A-render-writeback-joint-runtime-evidence-20260823.md`；Play cleanup/0 allocation delta、focused与self-check均PASS；C++ full trace仍BLOCKED。 |
| D-RENDER-003 | Central capture visibility gate | `UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED` | R07B full-tick Play闭合slot51/gen1 pending/free、Late同槽gen2、T冻结与T+1 generation恢复；R08进一步闭合OID7/8→51 dormant partner command suppression及split后双body恢复。pending、generation、dormant和split四个liveness边界均有正式producer→Central证据。 | `RESEARCH/R8-WP01G-R07B-central-liveness-identity-visibility-runtime-evidence-20260823.md`；`TASKS/R8-WP01G-R08-oid5152-merge-split-central-runtime.md`；R08 PASS result；C++ full trace仍BLOCKED。 |
| D-RENDER-004 | shadow special OID identity | `UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED` | 正式factory OID223/224的current DAT分别为223/224；body snapshot/command/resource/submission均true；shadow snapshot存在且`ShadowVisible=true`，current-DAT gate为`CommandSuppressed`，无shadow command/submission。exact identity self-check继续PASS。 | `RESEARCH/R6-PRES-02-shadow-current-dat-identity-preflight-20260822.md`；`RESEARCH/R8-WP01G-R07B-central-liveness-identity-visibility-runtime-evidence-20260823.md`。 |
| D-RENDER-005 | Unity extra EntityVisible/ShadowVisible gate | `UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED` | R07B证明223/224不是通过额外写ShadowVisible=false隐藏；同一帧baseline正式角色body/common-shadow snapshot/command/resource/submission均true；pool same-slot gen2 visibility恢复，未见production额外隐藏writer。 | `RESEARCH/R6-PRES-03-visibility-cache-writer-preflight-20260822.md`；`RESEARCH/R8-WP01G-R07B-central-liveness-identity-visibility-runtime-evidence-20260823.md`。 |
| D-RENDER-006 | 通用effective-pic / 技能图片/帧/atlas选择错误（用户实测） | `UNITY GPU+GAME+SCENEVIEW+AUTHORED-STATE S4 VERIFIED / C++ FULL TRACE BLOCKED` | 通用state8000 offset/raw999、DAT row、declared range/partial clip均已修复且无OID专项分支。R11更正旧“无authored state8000”结论：正式OID32 frame0/state8032完整tick得到DAT32/frame0/offset140/effective pic140；worker逻辑snapshot经既有主线程materialize生成18条命令，body command/catalog/UV一致。 | R8-WP01D-01..07；`TASKS/R8-WP01G-R11-authored-state-runtime-acceptance.md`；Play 12:22:17 PASS；C++ full trace仍BLOCKED。 |
| D-PERF-001 | PreInteraction no-op proof | `UNITY JOINT S4 PASS / C++ TRACE BLOCKED` | 旧cross-pass cache已删除；当前只在T14同点证明neutral。fresh 15/15覆盖frame/CPoint/link/holder/generation/fail-closed与0 B；grab/CPoint live Play PASS。50-AI fast/forced-legacy两侧SmokePassed、20项hash全等、0 GC、cleanup restored。 | `RESEARCH/R8-WP01G-R05-candidate-preinteraction-joint-evidence-20260823.md`；`CHANGE-RECORDS/R7-PERF-001.md`；C++ full trace仍BLOCKED。 |
| D-LATE-001 | late state-special chain / state9996 | VERIFIED（Unity production Play S4）；C++ trace BLOCKED | R7-LATE-001实现9995→4000→8000 reload与4×217+1×218；R8-SPRITEMAP-001把旧HitStun140纠正为`RenderPicOffset=140`。R8-LATEPLAY-001 worker-active Play验证current catalog natural random、live五子34 RNG、logic-only同调用full chain和authority400 exhaustion。 | `RESEARCH/R7-LATE-01-state-special-chain-9996-preflight-20260822.md`；`CHANGE-RECORDS/R7-LATE-001.md`；`CHANGE-RECORDS/R8-SPRITEMAP-001.md`；`RESEARCH/R8-WP01C-06-random-weapon-late-effect-runtime-evidence-20260823.md`。C++ full trace仍BLOCKED。 |
| A-RENDER-001 | CentralOnly / Texture2DArray / dynamic Mesh / URP | 已批准 adapter（保护） | C++ SDL blit 不会回退为 Unity production SpriteRenderer；中央命令是唯一 production pixel owner。 | R1-SOURCE-006；只能验证 command/observable output。 |
| A-RENDER-002 | 1.5× visual scale / held attachment | `R6-PRES-006 / RUNTIME_PENDING`（已批准adapter；no-code certification） | Unity保留BattleVisualScale=1.5；scale不进入逻辑pixel/world，held compensation在right/left保持wpoint重合，Central/Legacy comparison复用同一helper。19:49:12 full self-check PASS。 | `RESEARCH/R6-PRES-06-visual-scale-held-anchor-adapter-certification-20260822.md`；真实PlayMode相对锚点待验。 |
| A-RENDER-003 | fixed-world logic camera / presentation camera | `R6-PRES-007 / RUNTIME_PENDING`（已批准adapter；no-code certification） | tick边界清零release camera与entity RenderOffset；safe-area只移动presentation camera。stationary entity/shadow fixture在19:49:12 full self-check PASS。 | `RESEARCH/R6-PRES-07-fixed-world-camera-adapter-certification-20260822.md`；URP/safe-area/scene边缘PlayMode待验，snapshot restore residual UNKNOWN。 |
| A-RENDER-004 | production capacity | 已批准 adapter（保护） | C++ 400 只作 Authority400 fixture，不能回退 MobileExtended/DesktopExtended production capacity。 | R1-SOURCE-005～007；slot order/lifecycle fixture。 |
| A-RENDER-005 | active/Z/slot/command painter order | `R6-PRES-01 / RUNTIME_PENDING` | C++ active slot输入、stable signed-Z、same-Z slot与shadow→body→overlay→hit-record已映射到CentralOnly slot capture、stable radix/fallback、indexed rank和baseOrder；dynamic mesh不跨command流重排。no-code包focused command writer 6/6与17:49:18 full self-check PASS。 | `RESEARCH/R6-PRES-01-active-z-slot-command-order-preflight-20260822.md`；C++ trace/PlayMode/GPU像素待验。 |

## 5. 静态盘点闭合与当前不应得出的结论

- 可以说：COV-001～006 的静态 source inventory 已完成，全部当前发现均已具有唯一 ID、
  source family、Unity crosswalk、future owner 与最小验收路径。
- 不能说“所有运行时差异已经找完”；UNKNOWN、DAT/asset reachability、C++ full trace、Play Mode、
  GPU 和性能验收仍未完成。
- 不能说“D-MOV-001 一定是用户报告的每个移动问题的唯一根因”；它是明确的静态差异，
  具体场景影响必须按 fixture 验收。
- 不能说中央渲染、扩展容量、ECS/worker 本身是 C++ 行为错误；它们必须被保留，并按
  C++ logical handoff/observable output适配。
- 不能因为 R1-WP02 blocked 就停止源码盘点；它只阻断自动 full trace/comparator。

## 6. SOURCE-007 closure artifacts

- 依赖图与 future repair batches：
  docs/ai/RESEARCH/R1-SOURCE-007-dependency-graph-and-repair-batches.md
- 每项 D-ID 的分层验收矩阵：
  docs/ai/RESEARCH/R1-SOURCE-007-subflow-acceptance-matrix.md
- 这些材料明确 R2 之前不得修改 gameplay；R2 是下一次用户确认后才可开始的第一实施批次。

## 7. 关键详细资料

- 主 pass：`docs/ai/RESEARCH/R1-SOURCE-001-main-tick-contract.md`
- 主 pass Unity mapping：`docs/ai/RESEARCH/R1-SOURCE-001-unity-crosswalk-and-diff-inventory.md`
- 输入合同：`docs/ai/RESEARCH/R1-SOURCE-002-input-contract.md`
- 输入 mapping：`docs/ai/RESEARCH/R1-SOURCE-002-unity-input-crosswalk-and-diff.md`
- frame/physics working contract：`docs/ai/RESEARCH/R1-SOURCE-003-frame-physics-movement-lifecycle-contract.md`（创建后持续补充）
- frame/physics Unity crosswalk：`docs/ai/RESEARCH/R1-SOURCE-003-unity-crosswalk-and-diff.md`
- CPoint / held / opoint C++ contract：`docs/ai/RESEARCH/R1-SOURCE-005-cpp-cpoint-held-link-opoint-lifecycle-contract.md`
- CPoint / held / opoint Unity crosswalk：`docs/ai/RESEARCH/R1-SOURCE-005-unity-crosswalk-and-diff.md`
- render C++ contract：`docs/ai/RESEARCH/R1-SOURCE-006-cpp-render-handoff-contract.md`
- render Unity crosswalk：`docs/ai/RESEARCH/R1-SOURCE-006-unity-central-presentation-crosswalk-and-diff.md`
- 全路径覆盖与完成门槛：`docs/ai/RESEARCH/R1-SOURCE-INVENTORY-COVERAGE-MATRIX.md`
