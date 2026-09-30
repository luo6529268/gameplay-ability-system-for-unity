# NTSD 2.8-Logan 战斗对齐剩余工作与执行顺序

> **2026-09-30 后续盘点覆盖本页执行游标：** 目前根 EXE 身份已漂移，且用户要求进一步剔除非战斗场景逻辑。当前目标、未闭出口与执行门请先读[战斗对齐重新盘点与目标修订](NTSD28-BATTLE-ALIGNMENT-RESET-20260930.md)。本页保存 9 月 29 日快照及其证据，不再作为最新版本身份或下一任务选择依据。

状态：`ACTIVE / EXECUTION_SNAPSHOT`；对账日期：2026-09-29。此文是已启动总目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001` 的短入口，**不替代**[对齐总表](../../Assets/NTSD/Docs/ntsd28-logan-vs-unity-battle-alignment.md)中的差异 ID、Q/R 依赖、原始证据和六批次出口。新增首差、验证和状态仍先记入或回链总表，再同步这里。总目标和 BATCH-01～06、Q01～12、R01～18 的 ID 不变；只有剩余工作的选择方式收敛。本页依据本次重读总表 §0.11～0.14、恢复文档及现存文件核对；**本次没有重跑所有历史场景或宣称全域复验**。

## 目标与边界

剩余目标是在保留 Unity/GAS 框架和已批准例外的情况下，把**当前正式发行可达且非例外**的战斗规则、内容、逻辑帧、实体位移/碰撞、事件和战斗视听表现与 NTSD 2.8-Logan 对齐；每个差异须以正式根 EXE、进入 playable 构建闭包的源码、正式可达内容和同条件 Unity 证据裁决。正式根 EXE SHA-256 本次复核为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。选中内容根 `Assets/NTSD/Content/LoganRuntime`、独立 `Assets/NTSD/Resources/ProjectBattleModeConfig.asset` 和统一比例入口 `BattleSpatialProjection.cs` 均在当前工作树中；存在不等于所有消费者或终验已通过。

用户确认的范围继续生效：DAT 数值不改；正式 DAT/角色关联图片按 NTSD 2.8-Logan 版，原版背景及背景/两类 mode DAT 排除，项目自有背景、地图、可走区、完整背景固定相机和模式 Asset 保留。角色、武器、道具等战斗实体的比例换算应共用 World 入口，不能再散落独立倍率。非角色战斗对象离开项目可走区超过 10 秒逻辑时间清除；角色不适用。默认 `stage.dat` 部署暂缓。旧 521 项因保护性辅助读者继续保留，删除授权为 0。声音没有获准按 D-023 整包替换。Unity 自有结果页的设置、改难度/地图和重赛交互属于 G-07 非战斗例外；菜单只取战斗出生/退出所需最小接口证据，不做菜单功能对齐。不得使用 computer-use、另建 Unity 项目或改非战斗流程。所有受保护的未提交改动照原状保留。

“已通过”只指列明的字段、对象、tick、场景与验证层级；源码/生成工程编译、原 Editor Play、正式根回放、配对源码 GPU 和真实设备听感不能互相替代。旧 §0.10 的 B 阶段快照及 §0.14.4 的早期游标是时间戳历史；下表以较新的 Q 行和 2026-09-29 增补为准。

## 当前进度与实际未闭出口

| 组 / 批次 | 当前裁决 | 尚需何种出口；不重复什么 |
|---|---|---|
| Q01 / BATCH-01 | `DELIVERED`，清单和兼容性首差已交付 | 动态可达性和旧资源退场归 Q07/R17；不重新做全量清单。 |
| Q02～Q05 / BATCH-02 | 各自 `DELIVERED_SCOPED`：加载基础、字段合同、旧行为退休、D-022 schema/replay | 生产消费者、正式内容和整场由后组承担；不把 schema 存在误写为运行时齐全。 |
| Q06 / BATCH-03 | `DELIVERED_SCOPED / Q06_LOCAL_EXIT` | 已声明 producer/consumer/slot/lifecycle 本地出口复用；正式自然内容、mode/KO、画面和声音不倒灌 Q06。 |
| Q07 / BATCH-04 | `IN_PROGRESS`，依赖顺序最早未闭；选中资源字节、Menu/Player 回调、比例入口和若干自然技能/碰撞门有限定通过 | 353 份非排除正式 DAT 中暂存 338、同名 338 SHA 一致；缺 15 已分配 HUD 备用、Menu、P-14 例外或 stage 暂缓。PNG 正式 1255/暂存 1031、公共 1031 SHA 一致；缺 224 含排除背景 110、另 114 已静态归属，8 个声明图片尚无选中战斗消费者证明。`c/custom` 600/600、已声明 sheet 703/703 SHA 同。只对**新证实可达的非例外**内容/引用/自然技能或 D-024 源↔画面坐标首差开包；D-024 整域、非零地图锚点和全部自然技能仍待。旧 521 项不删，不为无消费者的图片或已通过的代表矩阵重测。 |
| Q08 / BATCH-04 | `IN_PROGRESS / COMBAT_RESULT_AND_EVENT_EXITS_OPEN` | stage 选中门、living groups、80/101/350、五类 KO producer、自然双轮和 F4 默认出口已有分范围证据。下一只找正式发行可达的非例外战斗 mode/stage、结果计时/事件首差与聚合出口；正式 LFR 在 350 停战步的序号证书失败须如实保留，World/RNG 冻结子门已独立通过。结果页设置/重赛不属于出口。 |
| Q09 / BATCH-05 | `IN_PROGRESS / AGGREGATE_DEPENDENCY_OPEN`；同 Z、shadow、Words/Spark 等有独立成果 | 自然合成 Game View、post-host snapshot、相邻插值/断点、非例外 custom shadow/bleed/lives/nameplate/spark/earthquake，以及同视口正式发行对照和 30/60/120 采样仍未形成整组证据。P-08 当前等 HP 根 LFR 所选字段 132/132 与配对源码 WARP 3 像素为限定出口；根 EXE GPU 与 Unity 同初态同视口尚未证。固定相机和 HUD 等例外不返工。 |
| Q10 / BATCH-05 | `IN_PROGRESS / Q10_OPEN`；选定 mode KO 两 WAV、事件和原 Scene 整 tick 播放有范围通过 | 真实选 mode/物理键、正式发行与可听对照、Player 侧载，以及其余正式可达 cue 的实际消费、顺序/channel、voice/BGM/stop/衰减、F11/F12 待按消费者清单验证。旧 976/71/905 是词法/2026-09-22 历史数；归一化后正式帧 cue 为 970 个物理路径、当前旧 Sound 同路径 68，其中 1 整文件同、67 不同，实际发声可达性还须逐 consumer 证明。新核的鸣人**螺旋丸后续跳跃分支** frame326 `078.wav` 正式/旧 PCM 不同但自然事件未测；已自然发出的 `003/004.wav` 虽整文件不同而 PCM 相同。见[定向身份报告](../../artifacts/diagnostics/NTSD28-Q10-NARUTO-ORASENGAN-CUE-IDENTITY-20260929/REPORT.md)。不得由总数整包复制 WAV。 |
| Q11 / BATCH-06 | `WAIT_DEPENDENCY` | Q09/Q10 聚合后，逐差异 ID 核 R01～R18、身份、例外与无主责任；只把真正的整场终验带到 Q12。 |
| Q12 / BATCH-06 | `WAIT_DEPENDENCY` | 同一正式版本完成可达角色技能、物理键、整场逻辑/RNG/事件、视听、长跑、退出重进和第 8 节全部验收。不能以各组局部 PASS 代替。 |

阶段 B 与执行组 Q 的出口不同。总表 §0.10 是 2026-09-12 快照，B7 的“zero-frame 待”及 B11 的“迁移未开始”等措辞已被后续 Q06/Q07 证据覆盖；下表只保留当前准确责任，不擅自宣告整阶段完成。

| 原阶段 | 当前状态与剩余归属 |
|---|---|
| B1 时间/Host | 当前生产单线程 cadence/Host 职责限定完成；worker 真启用及物理键/长跑由 R01/Q12 回访。 |
| B2 输入/RNG/AI 基础 | 基础及功能键路由完成；新 consumer、按键效果、自然技能由 R02/R03/Q07/Q08/Q10/Q12 回访。 |
| B3 主 pass | 骨架放行，整阶段未闭；残余 serial/terminal/新 writer 位置由 R04/R05 和 Q08/Q12 对账。 |
| B4 Frame/Physics/Revival | 多个核心子门完成，整阶段未闭；真实 queued 复活、资源及最终视觉/整链由 R06/R07/Q07/Q09/Q12。 |
| B5 Collision/Hit/Damage/Combo | 主线规则放行，整域未闭；新内容影响的候选/护甲和 KO/表现由 R09/R10/R14/Q07～Q12。 |
| B6 Catch/Held/Weapon/旧行为 | 多个子包与联合字段完成，整阶段未闭；自然新内容和新 consumer 仅按 R12/R13/Q07/Q12 首差回访。 |
| B7 Spawn/Lifecycle | Q06 声明的 producer/consumer、slot 与生命周期已有本地出口；正式内容、事件、表现及最终关闭仍待 R08/R16/Q07～Q12。 |
| B8 Stage/BattleFlow/Results | Q08 进行中，已有选中模式/KO/结果子门；正式可达非例外战斗结果/事件聚合未闭，默认 stage 资产保持暂缓。 |
| B9 战斗表现 | Q09 进行中，独立渲染子门已过；自然合成画面、插值与正式同视口未闭。 |
| B10 战斗音频 | Q10 进行中，选中两 cue 局部通过；其余可达事件、资源/Player/设备听感未闭。 |
| B11 正式内容 | Q01/Q02 基础已交，Q07 已有正式资源分批字节门；可达消费、自然技能和受保护旧文件退场未闭。 |
| B12 最终验收 | Q11/Q12 等待依赖，最终同版本整域证书未做。 |

六批次状态是 `BATCH-01 DELIVERED`、`BATCH-02/03 DELIVERED_SCOPED`、`BATCH-04 IN_PROGRESS`、`BATCH-05 PARTIAL_INDEPENDENT_SUBPACKAGES`、`BATCH-06 WAIT_DEPENDENCY`。这解释了为何“已推进 Q10”与“Q07 最早未闭”同时成立。

## 已触发回访与条件项

R 是回访索引，不是要求把 B1～B6 整阶段再做一次。每次只验证被新内容、生产者或表现消费者实际影响的路径。具体原差异 ID、触发关系与原证据仍见总表 §0.12。

| 回访 | 当前剩余责任 / 触发条件 |
|---|---|
| R01 | 仅 worker 真启用或 Host/presentation 边界变化时补 33/3 ms、pause/F2/debt；Q12 物理键/长跑。 |
| R02 | Q07/Q08 新可达内容、AI/RNG/输入 consumer 的同 tick 首差；Q12 自然技能整链。 |
| R03 | Q08 已路由功能键的实际战斗效果；Q10 F11/F12 音量消费，区分路由与生效。 |
| R04～R05 | 只在新 terminal/pieces/session writer 或 pass 插入时复核残余 owner、两 hit caller 和 publication 顺序。 |
| R06 | 普通自然复活 70 tick 630/630、stage Join producer 等已有限定通过；真实 queued 复活的资格/资源/位置/owner/slot、物理 Play/最终画面未闭。默认 stage 资产不因此解禁。 |
| R07 | Q08 mode 生效后联验 HP/MP、display、timer、clamp 和恢复；既有 Q06 的局部 resource/display 证据复用。 |
| R08 | Q09/Q10 接通碎片、火花、终端物件的显示/声音与生命周期时回验，不重做生成本身。 |
| R09 | 新正式内容实际改变候选/护甲时才查命中 first difference；旧已过矩阵不重做。 |
| R10 | Q08/Q10 聚合伤害→资源→KO→held→事件/声音链；已过的选中护甲和普通 KO 代表不例行重跑。 |
| R11～R12 | 仅新内容触发 finalizer、拾取/refill/投掷/持有 consumer 时复验。 |
| R13 | 退休行为/联合字段子门已有出口；新 carrier/旧保留 shell 的受影响 reader 才回验。 |
| R14 | Q09 Spark/Combo 表现与 Q10 对应声音；combo 绘制以正式发行启用门获证为前提。 |
| R15 | Q07/Q08 正式身份和 mode/kind 输入、Q12 同版本 trace；Q01/Q05 已过身份/schema 子门复用。 |
| R16 | 新 queue/worker/renderer 接入先声明十一阶段关闭 owner，再验退出重进和零残留。 |
| R17 | Q07 精确引用及旧 521 保留，Q09/Q10 真实消费与 Player 资源侧载；未授权删除仍为 0。 |
| R18 | Q12 当前正式发行可达非例外技能与整场最终链。 |

条件回访须有**正式可达入口**才上升优先级：当前正式 DAT 无 `state:405` 入口；保存的 Battle Scene 有项目地图，故无地图回退只在该配置可达时验；默认 `stage.dat` 暂缓使波次条件未满足；当前菜单 mode1 与正式根 LFR 无 mode2/3/4 可选择入口，不能伪造必修入口；正式根 headless trace 没有 GPU 像素、也不能从当前根 story 注入 lives2。这些是范围/观测边界，不能写成已对齐，更不能为制造场景修改非战斗页面。若后续证实正式可达，则回到对应 Q/R 包。

## 调整后的执行次序与每批出口

1. **先做 Q07/Q08 的可达首差门（BATCH-04）。** 从当前正式发行输入、正式内容和原 Battle Scene 选一个尚未覆盖的自然战斗条件，核对首个不同的字段/事件；没有新可达 Q07 首差时继续独立 Q08。每包先给出处、Unity writer/reader、前置条件和最窄反例，再决定是否写代码。Q07 只负责内容可用与 D-024 共用比例；Q08 只负责战斗 mode/stage/结果/事件。出口是所有已证新首差修复并按风险通过 source/Unity focused、必要 Play/trace；未证条件保留清晰 owner。**不以加角色数或案例总量作为进度。**
2. **再完成 Q09 和 Q10 的独立视听子包（BATCH-05）。** Q09 先补自然合成视口及 publication/插值边界，再按真实像素首差处理具体效果；P-08 正式根无像素导出须用可观察输出/同视口方法解决，不能把配对源码像素称为根 EXE 像素。Q10 先按正式可达 cue 的生产→预热→播放→退出链查资源缺口，资源复制仅限具名消费者/明确授权；再验证 Player 和实际听感。两组各自验证逻辑 checksum 不因表现改变。
3. **最后 Q11 对账、Q12 终验（BATCH-06）。** Q11 逐项确认 R、141 个原差异及新增首差、用户例外、内容身份和剩余归属；Q12 才做同一版本全可达技能、物理输入、整场/长跑、视听和关闭重进。Q11 不能把未完成生产工作塞给 Q12；Q12 不能由局部 PASS 拼接成“完全一致”。

每个代码包仍须在改脚本前建立 Task/Change Record，写明受影响战斗路径、正式依据、非战斗保持、回滚方式与最窄验证；改后运行 ChangeLedger validator。测试遵循**改哪里、测哪条可观察链**：共享算法用代表性边界和关键 caller；只在跨 pass/schema/关闭或新 first difference 时扩大到 SelfCheck、真实 Play、Player 或整场。已有同条件已过矩阵不机械重跑。批次出口同时交代目标功能和非战斗保持。没有新的可达非例外首差时，不为了维持“正在推进”而扩展到结果页、菜单设置或角色数量堆叠。

## 本次重排的证据边界

本次是对现存总表与状态记录的**当前状态对账**，并复核正式 EXE 身份、正式内容根、项目模式 Asset 和比例代码入口存在。没有修改 DAT、资源、C#、Scene、Prefab 或 ProjectSettings；没有运行新编译、Unity Play、正式 EXE 对战或全量测试。下一实施包必须按其风险补新鲜运行证据。本页状态有新证据时以总表中后续具名验收修订，不能仅改本页把 `IN_PROGRESS` 提升为 `DELIVERED`。
