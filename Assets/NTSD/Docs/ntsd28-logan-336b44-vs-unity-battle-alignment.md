# NTSD 2.8-Logan 336B44 ↔ Unity 战斗对齐总表

状态：`ACTIVE / NEW_AUTHORITY_REBASELINE / Q07-Q12_OPEN`；建立日期：2026-09-30。用户明确选定含 35 项限定修复的新版正式发行，要求以[九天重盘与目标修订](../../../docs/ai/NTSD28-BATTLE-ALIGNMENT-RESET-20260930.md)为范围重新整理、继续执行。本文从此作为**当前执行总表**；[旧 B1E13 总表](ntsd28-logan-vs-unity-battle-alignment.md)保留逐项历史、Task/Change 和具名局部证据，不再以其旧 SHA 或状态自动裁决新版。原总目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001` 继续 ACTIVE，六批次和 Q01～Q12/R01～R18 编号延续，避免产生两个并行完成口径。

## 0. 版本、范围与证据规则

- **当前战斗规则权威**：`J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan\NTSD2.8-Logan.exe`，SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；`source/README_SOURCE.md` 声明的对应源码中实际进入 `ntsd28_playable/scripts/build.ps1 -Target playable` 的 live path；以及正式启动参数实际读取的 `resources/runtime`。身份、82 文件 playable 闭包 `B97DF3C5BB75058D13FD9CCC7A542536BB956A1F496A9140BD98C06D6F8638AA`、75 文件诊断子闭包 `066A8CDEEBB7A48706111443126E8F6EFCFD140B692E85EF9645149C8868520C`、资源树与独立编译层级见[G0 新版身份报告](../../../artifacts/diagnostics/NTSD28-G0-336B44-AUTHORITY-20260930/REPORT.md)。临时重建产物 SHA 不等于正式 EXE，不能晋升为行为权威。
- **历史证据**：`B1E13…19033` 为上一个已确认版本；`1277B…DAF75` 及 NTSD 2.4/C# 更早。旧版通过的 Unity 机制检查可作为实现线索；需按新版修改点、正式可达条件和同状态字段决定是否复用，不能全量作废，也不能直接把旧证书改成新版 PASS。[29 文件/35 ID 版本差异盘点](../../../artifacts/diagnostics/NTSD28-G0-VERSION-DELTA-20260930/REPORT.md)只证明源码变化和条件性首差，不代替新版正式 EXE/Unity paired tick。
- **硬范围**：只对齐战斗场景当前新版可达、非例外的 DAT/角色图消费、输入与逻辑 tick、角色和非角色实体、碰撞/伤害/关系/生命周期、战斗胜负与事件、战斗画面和声音。保留 Unity/GAS 框架、项目地图/可走区、固定完整背景相机及按正式视口比例统一映射的战斗实体/碰撞（D-024）；非角色战斗对象出项目可走区 10 秒**逻辑时间**后清除（D-025）。DAT 数值不改。
- **内容与排除**：D-023 使用新版 `resources/runtime` 的非排除 DAT 与角色相关图片；原版背景、背景模式、`data/mode.dat` 与 `data/mode/ntsd.dat` 不部署，项目自己的背景/地图及 `ProjectBattleModeConfig.asset` 保留。默认 `stage.dat` 部署仍暂缓。旧 521 项资源删除授权仍为 0；音频不因 DAT/角色图决定而获得整包替换授权。原生 HUD、结果页/KO feed 图文、结果页设置/难度/地图/重赛、完整菜单功能、FootSelf、移动端黑区、联机/回滚均不计入本次完成出口。Menu→Battle 仅为证明战斗出生、输入、有序退出取最小接口。
- **判定**：每个新 ID 先证明新版正式可达入口和 source live path，再以相同 seed、初态、输入及逻辑 tick 找 Unity 首差；确认受影响的共用 writer/readers 后做最小改动和聚焦测试，跨 pass/schema/退出再加 SelfCheck、真实原 Battle Scene Play、Player 或整场证据。静态差、编译、单测、受控完整 tick、正式 EXE 可观察结果和设备体验分别标状态；不以角色案例数或旧测试总数替代必要证据。

## 1. 六批次与 Q 阶段新基线

| 批次 / Q | 当前状态 | 新版权威下真正剩余的出口 |
| --- | --- | --- |
| BATCH-01 / Q01 内容与引用清单 | `HISTORICAL_DELIVERED / 336B44_CONTENT_RECHECK_PENDING` | 旧版清单已交付；新版 runtime 树身份已复核。仅对当前所选战斗 consumer 的 staged DAT/角色图和引用做增量逐 SHA 核对；无消费者的缺项与用户排除项不盲复制。 |
| BATCH-02 / Q02～Q05 加载、PNG、schema、旧行为退休 | `HISTORICAL_DELIVERED_SCOPED / IMPACT_TRIGGERED_REVISIT` | 不重做基础迁移；35 项中若改变既有字段、帧、输入或展示读口，只回访受影响的共享契约与真实消费者。 |
| BATCH-03 / Q06 producer 与生命周期接线 | `HISTORICAL_LOCAL_EXIT / IMPACT_TRIGGERED_REVISIT` | 旧已证 producer/slot/关闭局部证书保留版本标签；新版 C011/C012/C022～C024/C029/C031 等触及 pass 时点时，按当前入口做最窄回访。 |
| BATCH-04 / Q07 内容可用、战斗规则及比例域 | `IN_PROGRESS / DIRECT_BATTLE_BIRTH_SCOPED_VERIFIED / C017_UNITY_FULL_TICK_PASS / C011_UNITY_PLAY_PASS / C012_UNITY_FULL_TICK_PASS / C022_UNITY_FULL_TICK_PASS / C023_UNITY_FULL_TICK_PASS / C024_UNITY_FULL_TICK_PASS / C029_UNITY_FULL_TICK_PASS / C031_UNITY_FOCUSED_PASS / F02_UNITY_FOCUSED_PASS / F03_UNITY_FULL_TICK_PASS / F04_UNITY_FOCUSED_PASS / F05_UNITY_FOCUSED_PASS` | Q07/D-024 直接 Battle 场景出生双域首差已单点修复，同一鸣人自然按键 KO/结果 Play 1/1 PASS。[D-024](../../../docs/ai/TASKS/NTSD28-336B44-Q07-D024-DIRECT-BATTLE-BIRTH-001.md)。C017 零血输入原 Editor 聚焦 4/4、修正外部按键/相位夹具后的合成完整 tick 3/3、生产毒计时归零完整 tick 1/1 PASS；DAT 接触自然 Play 与正式根同态待。[C017](../../../docs/ai/TASKS/NTSD28-336B44-Q07-C017-ZERO-HP-INPUT-001.md)。C011 传送相位完整 tick 3/3、原 Battle 四 tick Play PASS，正式根同态待。[C011](../../../docs/ai/TASKS/NTSD28-336B44-Q07-C011-TELEPORT-PHASE-001.md)。C012 特殊命中尾清原 Editor 尾部/前尾控制 4/4、纠正无 Health type3 尾部 2/2、真实候选两轮完整 tick 1/1 PASS，正式根同态与自然 Play 待。[C012](../../../docs/ai/TASKS/NTSD28-336B44-Q07-C012-SPECIAL-HIT-LATCH-TAIL-001.md)。C022 kind2 帧门精确3/3、相邻4/4 PASS，正式根同态/自然 Play 待。[C022](../../../docs/ai/TASKS/NTSD28-336B44-Q07-C022-KIND2-FRAME-GATE-001.md)。C023 空中 idle 转212 原 Editor 完整 tick 5/5、相邻 C022 3/3 与 C25G 4/4 PASS，正式根同态/自然 Play 待。[C023](../../../docs/ai/TASKS/NTSD28-336B44-Q07-C023-AIRBORNE-IDLE-FRAME-001.md)。C024 相对 next 原 Editor 精确4/4、OID315完整tick1/1、相邻 C022 3/3/C023 5/5/C25G 4/4 PASS，正式根同态/自然 Play 待。[C024](../../../docs/ai/TASKS/NTSD28-336B44-Q07-C024-RELATIVE-NEXT-001.md)。C029 kind2物理现有门原Editor OID52/反例/type3最终4/4 PASS，正式根/自然Play/武器专项待。[C029](../../../docs/ai/TASKS/NTSD28-336B44-Q07-C029-KIND2-PHYSICS-001.md)。C031 严格穿地：原Editor等号RED3/3、直接19/19与相邻17/17 PASS，正式根同态/自然Play/C032音效待。[C031](../../../docs/ai/TASKS/NTSD28-336B44-Q07-C031-STRICT-FLOOR-001.md)。F03 环境标记最终帧尾清：原Editor RED2/2，环境/相邻动作21/21、canonical完整tick2/2 PASS；正式根同态/自然Play待。[F03](../../../docs/ai/TASKS/NTSD28-336B44-Q07-F03-ENVIRONMENT-TAIL-001.md)。F04 精确回满保留E4：原Editor kernel RED、生产夹具+1更正后回血类8/8 PASS；正式根同态/自然Play待。[F04](../../../docs/ai/TASKS/NTSD28-336B44-Q07-F04-EXACT-MAX-HEAL-001.md)。F05 终局state14持帧计数：原Editor双mode RED，复活类21/21 PASS；正式根同态/自然Play待。[F05](../../../docs/ai/TASKS/NTSD28-336B44-Q07-F05-TERMINAL-COUNTER-001.md)。F02 高速武器减速后动作40已得原Editor本类30/30、相邻50/50 PASS；全量SelfCheck更早图片预热FAIL、正式根同态/完整tick/自然Play待。[F02](../../../docs/ai/TASKS/NTSD28-336B44-Q07-F02-FAST-WEAPON-ACTION-001.md)。Q07 整组仍开，其他内容与比例域按可达性处理。 |
| BATCH-04 / Q08 胜负、计时和战斗事件 | `IN_PROGRESS / C008_UNITY_NATURAL_PLAY_PASS / F05_UNITY_FOCUSED_PASS / FORMAL_ROOT_PENDING` | **当前 C008**：新版输入轮写 350、下一 host pass 才退出；Unity 原同轮派 transition 的 RED 2/2 后共享 writer 修复，原 Editor 聚焦 2/2、相邻结果类 13/13、同一自然双轮 KO/held 结果 Play 1/1 通过。先前三次自然 Play 被 Q07 出生双域差异挡在命中门，证据全部保留；Q07 单点修复另包。C008 正式根 EXE 同状态 trace 与 Q08 其余项仍待，不能报整组对齐。F05 终局倒地持帧计数原Editor复活类21/21通过，正式根/自然Play待。[F05](../../../docs/ai/TASKS/NTSD28-336B44-Q07-F05-TERMINAL-COUNTER-001.md)。结果页设置/重赛 UI 完全排除。[子包](../../../docs/ai/TASKS/NTSD28-336B44-Q08-C008-DEFERRED-RESULT-EXIT-001.md)。 |
| BATCH-05 / Q09 战斗表现 | `IN_PROGRESS / OLD_SCOPED_EVIDENCE_RETAINED` | 新版 render handoff 与自然合成 Game View、previous/current publication 和插值断点、正式可达非例外遮挡/阴影/火花/出血/地震及可比视口证据；可选 combo 四项仅在新版正式默认或用户实际启用时验，默认关闭不制造入口。 |
| BATCH-05 / Q10 战斗音频 | `IN_PROGRESS / C032_UNITY_FULL_TICK_AND_CLIP_PASS / OLD_SCOPED_EVIDENCE_RETAINED` | C032 声道6事件与已存同PCM `016.wav`：原Editor直接/clip3/3、canonical完整tick2/2、相邻10/10；正式EXE同态及voice/声像/音量/设备未验。[C032](../../../docs/ai/TASKS/NTSD28-336B44-Q10-C032-LANDING-CHANNEL6-001.md)。其他所选自然cue仍按正式事件→Unity准备clip→voice/声像/音量/停止→设备出口分证；Q10旧版鸣人action326/`078.wav`子证书须用336B44重新标版本。只按可达consumer处理资源，不按970路径数整包搬WAV。 |
| BATCH-06 / Q11 账本对齐 | `WAIT_DEPENDENCY` | 对 35 项及历史 R01～R18 逐条写明新版可达、用户例外、当前 Unity 状态、owner 和关闭证据；未闭生产缺口不能推给 Q12。 |
| BATCH-06 / Q12 最终集成验收 | `WAIT_DEPENDENCY` | 用同一冻结 336B44 版本做自然物理键/技能、同 seed/tick 整场、逻辑与画面/声音、长跑、有序退出重进和旧总表第 8 节非例外出口；局部 PASS 不能拼成整场一致。 |

## 2. 新版固定 35 项到 Unity 工作的逐 ID 路由

此表的“新版修复”表示新版 NTSD 源码相对 B1E13 的变动已进入正式 playable 闭包；**不是 Unity 已修、也不是每项都需要改 Unity**。来源为当前正式源码、`RELEASE_INFO.md` 的固定清单和外部工程 `J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\battle_scene_reverse\notes\BATTLE_REPAIR_PROGRESS_20260928.md`；后者仅作发行侧差异说明，不能替代正式 EXE 可观察行为。下列 `STATIC_CANDIDATE` 必须先完成 paired RED；`REBASELINE_ON_REACH` 只在当前正式内容/输入到达相应分支时回访。

| ID | 新版机制与当前归属 | Unity 下一证据门 / 状态 |
| --- | --- | --- |
| F01 | 防御读 ITR `bdefend`；Q07/R09 | 当前所选 ITR 与 Unity 防御 writer 同条件对照；`REBASELINE_ON_REACH` |
| F02 | type4/6 高速 state1000 动作 40；Q07/R06 | 正式 playable 物理在接地减速后用严格`|Vx|>9`选40，落地可覆盖；正式OID600 frame0无hit_Fa。Unity普通路径漏判、共享路径早判经原Editor RED11/30（另4例X额外位移夹具期望更正），共用物理选招后本类30/30、相邻与旧武器自检分支合计50/50 PASS。全量SelfCheck在更早图片预热分支FAIL，正式根同态/完整Driver tick/自然Battle Play仍待；`UNITY_FOCUSED_PASS / RUNTIME_PENDING`。[Task](../../../docs/ai/TASKS/NTSD28-336B44-Q07-F02-FAST-WEAPON-ACTION-001.md) |
| F03 | state12 穿地后环境标记尾清理；Q07/R06 | 正式物理尾按最终动作帧是否state12清`environment_state_320`，kind2早退不清。Unity原Editor穿地/普通非state12 RED2/2；三type0角色路径接共用尾清后，环境/相邻动作21/21、canonical完整tick等号/穿地2/2 PASS，marker10/0与HP100/90正确。正式根同态和自然Battle Play待；`UNITY_FULL_TICK_PASS / RUNTIME_PENDING`。[Task](../../../docs/ai/TASKS/NTSD28-336B44-Q07-F03-ENVIRONMENT-TAIL-001.md) |
| F04 | 恰到最大 HP 时保留回血计时；Q07/R07 | 正式仅候选`before+8>max`清普通E4；Unity旧钳制后`>=`首差由kernel原Editor RED。共享kernel单点修后，校正生产夹具前置+1，回血owner类8/8 PASS，覆盖精确回满/超限/下次受伤和E0/state1700。正式根同态/自然Play待；`UNITY_FOCUSED_PASS / RUNTIME_PENDING`。[Task](../../../docs/ai/TASKS/NTSD28-336B44-Q07-F04-EXACT-MAX-HEAL-001.md) |
| F05 | 终局 state14 保持倒地时清动作计数；Q07/Q08/R07 | 正式物理主槽终局持帧清+0x088；Unity两C25早退原Editor双mode RED计数7非0，改后复活参战者类21/21 PASS，含多命/排队/瞬时槽反例。正式根同态与自然Battle Play待；`UNITY_FOCUSED_PASS / RUNTIME_PENDING`。[Task](../../../docs/ai/TASKS/NTSD28-336B44-Q07-F05-TERMINAL-COUNTER-001.md) |
| N01 | 同场重赛保留原生场景 loop；G-07 用户排除 | 仅当它反向改变正在进行的战斗 tick 才重开；`USER_EXCLUDED` |
| N02 | 原生 HUD/结算板读取冻结计时；P-17/P-19 用户排除 | 战斗内部计时仍由 Q08 验；图文 `USER_EXCLUDED` |
| C008 | 继续输入写 350 后下一 tick 才退出；Q08/R15 | Unity 旧同轮派 transition，原 Editor RED 两例均存储计时 0≠350；共享 writer 修复后 2/2、相邻 13/13 与自然双轮 KO/held Play 1/1 PASS。前三次 Play 候选0失败保留，Q07 出生双域单点修复另包。正式根 EXE 同状态 trace 未取得；`UNITY_NATURAL_PLAY_PASS / FORMAL_ROOT_PENDING`。[Task](../../../docs/ai/TASKS/NTSD28-336B44-Q08-C008-DEFERRED-RESULT-EXIT-001.md) |
| C009 | 第二存活阵营返回暂停倒计时、清赢家，再剩一组恢复；Q08/R07 | 自然复归后 timer/赢家/停战完整 tick；`REBASELINE_ON_REACH` |
| C011 | state400/401 传送按每 tick 翻转相位、仅相位 0 执行；Q07/R04 | Unity canonical tick 已门控；原 Editor 两状态完整 tick RED 2/2、GREEN 加直接机制反例 3/3，原 Battle 四 tick Play PASS、清理/Scene 稳。正式根同态及自然所选内容待；`UNITY_BATTLE_PLAY_PASS / FORMAL_ROOT_PENDING`。[子包](../../../docs/ai/TASKS/NTSD28-336B44-Q07-C011-TELEPORT-PHASE-001.md) |
| C012 | 活跃物件 tick 尾清特殊命中锁存；Q07/R09 | Unity 共享活跃实体尾部已清锁存，原 Editor RED 2/2、尾部/前尾控制 4/4；纠正原 type5/带 Health 夹具后，真正无 Health type3 尾部 2/2 PASS。type3 真实候选首轮被锁存挡住、次轮命中完整 tick 1/1 PASS。自然 Play/正式根同态待；`UNITY_FULL_TICK_PASS / RUNTIME_PENDING`。[子包](../../../docs/ai/TASKS/NTSD28-336B44-Q07-C012-SPECIAL-HIT-LATCH-TAIL-001.md) |
| C017 | 零血 type0 不再全局清输入；Q07/R02 | Unity 两层共享总门已移除；原 Editor 正例 RED 2/2、改后采样保留/standing Attack/state14 反例精确 4/4 PASS；修正外部按键映射及双 tick 采样相位后，完整 tick 零血站立/终局与正血对照 3/3 PASS，生产毒计时 HP5→0 后攻击 1/1 PASS。DAT 接触自然 Play 与正式根同态仍待；`UNITY_FULL_TICK_PASS / RUNTIME_PENDING`。[子包](../../../docs/ai/TASKS/NTSD28-336B44-Q07-C017-ZERO-HP-INPUT-001.md) |
| C022 | 当前帧 CPOINT kind2 保持帧动作/计数；Q07/R05 | 正式 OID52 Naruto(1T) action130 state1700/kind2 的 staged DAT 与六张声明图逐 SHA 同正式资源；正式 playable `BattleWorld28::step_frames_range` 在当前首 kind2 时帧推进前返回。原 Editor 有效抓取完整 tick 计数1→2 RED、type3 native body 计数1→2 RED；共享 `RunNativeC25FrameTransaction` 加当前帧 kind2 早退后，OID52 有效 holder、state10/no-kind2 和 type3 正反精确 3/3 PASS，相邻 C25G 4/4 PASS。无抓取者转212及最初 type3 控制 HP 不降分别是关系/类型夹具问题，已更正并保留原记录。正式根 EXE 同态、自然 Battle Play 与 C029 物理仍待；`UNITY_FULL_TICK_PASS / RUNTIME_PENDING`。[子包](../../../docs/ai/TASKS/NTSD28-336B44-Q07-C022-KIND2-FRAME-GATE-001.md) |
| C023 | 空中 state0→212 的帧计数/wait 时点；Q07/R05 | 当前正式 OID2 `c/nar/nar.dat` 与 staged 同 SHA，action0/state0/wait3、action212/wait1/next0。playable 在计数增加后选212、同 tick 读新 wait/next；原 Editor 完整 tick 空中 counter0/2 RED，真实首差在共用 native C25 事务。已按 Task/Change 单点接入 type≥0 通用转212。修正两个测试夹具预期后，原 Editor 完整 tick 五例 5/5 PASS；相邻 C022 3/3、C25G 4/4 PASS。正式根 EXE 同状态 trace 与自然 Battle Play 待；`UNITY_FULL_TICK_PASS / RUNTIME_PENDING`。[子包](../../../docs/ai/TASKS/NTSD28-336B44-Q07-C023-AIRBORNE-IDLE-FRAME-001.md) |
| C024 | `next=13xx` 同步随机相对动作；Q07/R05 | 正式同 SHA OID315 action300/state15/wait1/next1320 在原 Editor RED 误选终止1320；零跨度 next1300 也误选终止1300。共用 native C25 转移单点接入正式相对选帧及 World 同步 RNG 后，原 Editor 精确4/4、正式 OID315 生产完整 tick 1/1、相邻 C022 3/3/C023 5/5/C25G 4/4 PASS。自然 Battle Play 与正式根 EXE 同状态仍待；`UNITY_FULL_TICK_PASS / RUNTIME_PENDING`。[子包](../../../docs/ai/TASKS/NTSD28-336B44-Q07-C024-RELATIVE-NEXT-001.md) |
| C029 | CPOINT kind2 抑制物理积分；Q07/R06 | 当前正式同SHA OID52 action130/state1700/kind2：原Editor有效holder完整tick保留非零速度/动作/计数，独立物理 X/Y/Vx/Vy 不变；state10无kind2完整tick X积分，synthetic type3 kind2独立物理不积分。既有生产早退满足这组条件，未改生产；最终聚焦类4/4 PASS。正式根同态、自然Battle Play和武器专项仍待；`UNITY_FULL_TICK_PASS / RUNTIME_PENDING`。[子包](../../../docs/ai/TASKS/NTSD28-336B44-Q07-C029-KIND2-PHYSICS-001.md) |
| C031 | state12/18 严格穿地才触发着地；Q07/R06 | 原Editor等号初始RED3/3，两个直接消费点改后仍RED，首差含普通着地后备入口；现共用物理结果的积分后 `Y>floor` 门覆盖动作/环境/后备，等号保留原动作。直接两类19/19、相邻空中17/17 PASS；正式根EXE同态/自然Play与C032音效仍待；`UNITY_FOCUSED_PASS / RUNTIME_PENDING`。[Task](../../../docs/ai/TASKS/NTSD28-336B44-Q07-C031-STRICT-FLOOR-001.md) |
| C032 | state12/18 落地声道 6 事件；Q10/R17 | 正式索引6=`data/016.wav`且Unity现存WAV的PCM同SHA；原Editor严格穿地主路无事件RED，独立封存clip缺失RED。共用落地前置/三调用点与预热修后直接/clip3/3、canonical完整tick等号/穿地2/2、相邻环境10/10 PASS；正式根同态、voice/声像/音量及设备待；`UNITY_FULL_TICK_AND_CLIP_PASS / RUNTIME_PENDING`。[Task](../../../docs/ai/TASKS/NTSD28-336B44-Q10-C032-LANDING-CHANNEL6-001.md) |
| C040 | 抓取定位的当前受害者帧与 vaction 抓取者 CPOINT 分源；Q07/R12 | 同态 X/Z、停顿帧与显示锚点；`REBASELINE_ON_REACH` |
| C042 | 投掷只清抓取者动作计数；Q07/R12 | 被投者计数及后继帧；`REBASELINE_ON_REACH` |
| C043 | 失效持有关系尾仅清关系状态；Q07/R13 | 原持有槽与复用/退出；`REBASELINE_ON_REACH` |
| C044 | 负 decrease 延后写冲量；Q07/R12 | 同轮暂存与后继速度；`REBASELINE_ON_REACH` |
| C045 | CPOINT `recover` 停顿和 `cover` 定位分读；Q07/R12 | 字段、停顿与位置；`REBASELINE_ON_REACH` |
| C048 | 特殊 BDY 提前返回前写防御累计 45；Q07/R09 | 首 BDY 实际消费顺序；`REBASELINE_ON_REACH` |
| C050 | linked rest=-1 跳普通垂直反应；Q07/R10 | 特殊链接与非零 dvy 门；`REBASELINE_ON_REACH` |
| C051 | reduced effect22/23 的相对方向和完整地面 dvx；Q07/R10 | 左右两侧、护甲消费；`REBASELINE_ON_REACH` |
| C052 | effect21 读目标当前状态并终止本攻击者余下候选；Q07/R09 | 同 tick 多攻击者/多目标候选序；`REBASELINE_ON_REACH` |
| C053 | type3 `hit_Uj` 读锁存动作帧；Q07/R09 | current/latch 双读口；`REBASELINE_ON_REACH` |
| C054 | 解融合整数 XYZ 重建伙伴精确位置；Q07/R16 | 解融合双域位置/比例与快照；`REBASELINE_ON_REACH` |
| C056 | 合体停帧保留动作计数、不提前生成 OPoint；Q07/R16 | 同 tick 帧/子体出生；`REBASELINE_ON_REACH` |
| KO-default | 原生 KO feed 默认关闭；P-19 图文用户排除 | 战斗 KO 事件属 Q08、实际声音属 Q10；图文 `USER_EXCLUDED` |
| combo-times | 可选连击显示的 `times` 寿命；Q09/R14 | 仅正式可达/显式启用时验；`CONDITIONAL_PRESENTATION` |
| combo-boundary | 可选连击显示等号边界；Q09/R14 | 同上；`CONDITIONAL_PRESENTATION` |
| C059 | 可选连击显示镜头 X+14 门；Q09/R14 | 固定相机例外下仅可比条件；`CONDITIONAL_PRESENTATION` |
| C060 | 可选连击显示允许非 type0 拥有者；Q09/R14 | 当前所选内容可达时验；`CONDITIONAL_PRESENTATION` |

35 项共计：战斗规则/事件 28、可选展示 4、用户排除的原生页面/HUD/提示图文 3。前 28 项中静态候选 4（C008/C011/C012/C017），其余 24 项并非“已通过 Unity”；它们是按正式可达入口触发的版本回访。旧版对齐的 141 条差异与已关闭职责按旧总表保留，但 Q11 必须做新版复用/失效账，不能把 35 项当作全部待办或全部新 Bug。

## 3. R01～R18 回访触发与执行顺序

R01～R18 仍用旧总表的定义，按触发而非全量重跑：输入/帧/物理与规则的 C011/C017/C022～C031 触发 R02/R04～R08；候选/伤害/抓取/融合的 F01/C012/C040～C056 触发 R09～R13/R16；可选 combo、战斗画面与声道 6 触发 R14/R17；任何 source/schema/正式身份改变触发 R15；自然整链由 R18/Q12 统一验。每个包在本表追加准确 ID、正式条件、Unity 首差、测试与未验项，避免在旧总表的数千行历史中再追加新游标。

1. **G0 版本卡**：本次身份、资源树和 82/75 文件闭包已核，独立 playable 编译通过但新产物哈希不同。G0 不宣称 Unity 兼容或发布 EXE 与候选逐字节相等；后续每个包继续使用根 336B44 和当前资源。先增量复核所选 staged 内容身份，检查正式根启动输入/trace 接口。
2. **G1 BATCH-04 首差**：先 Q08/C008 的正式继续输入→350→下一 tick 出口，随后 Q07/C017、C011、C012，顺序可由新版自然可达性调整；每项先 RED，再决定是否改共用 writer。Q07/D-024 尚开项只按新可达首差推进；C009 与其余 24 项按触发而非一口气复制 35 个实现。
3. **G2 BATCH-05**：Q09 自然合成 Game View 与可比视口、Q10 可达 cue 的事件/音源/设备出口；不改背景/结果页。外观改动必须守住逻辑 checksum 与 source/physical 位置一致。
4. **G3 BATCH-06**：Q11 建新版逐项责任账与旧证据复用/失效表，Q12 在同一冻结版本做整场终验。未证可达或明确用户例外各自记录条件，不伪装成完全对齐。

所有脚本改动先建 Task Contract、Change Record、Ledger/STATE/handoff，再改脚本；修改后跑最窄相关编译和测试、ChangeLedger validator，只有跨 pass/schema/有序关闭或发现新首差才扩检。不得使用 computer-use、另建 Unity 项目、改 DAT 数值、覆盖/删除用户脏文件或以非战斗场景功能作为进度。
