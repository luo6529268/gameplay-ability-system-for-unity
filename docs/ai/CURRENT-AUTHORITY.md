> 2026-09-23 Q10受控整链证据：原Battle Scene直接Play实际为battle mode0，首个完整致死探针在创建夹具前准确FAIL，原报告已另存。重跑时选定正式KO feed已发布，只在Play-only World切mode1，临时战斗体经真实Driver tick5→6令受击者HP=-10、KO事件0→1、m_join.wav入队，场景音频播放0→1、未预热拒绝0；Editor退出后Scene SHA 9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0不变。此为受控mode1临时战斗体整tick，不证明真实菜单选mode、物理玩家输入、扬声器可听、Player或正式EXE等价；Q10/总目标开放。详NTSD28-Q10-KNOCKOUT-MODE-SOUND-001 Record和scene-natural-ko-audio-probe.json。

> 2026-09-23 Q10真实场景补证：原项目Editor PID173216在保存的NTSD_Battle Scene两次Play（第二次退出重进）中，选定正式两WAV已在封存音频目录，每轮用合成PendingSoundEvent经生产NTSDSoundPlayer呈现，调用播放计数0→2、未预热拒绝0→0；两轮退出后Editor非Play，Scene SHA-256 9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0不变。原Editor聚焦8/8 PASS。此为合成事件的真实场景音频呈现证据，不能替代自然击倒整链、可听设备输出、正式EXE听感或Player/其余cue验收；Q10及总目标开放。详NTSD28-Q10-KNOCKOUT-MODE-SOUND-001 Record和scene-play-audio-probe-first.json / scene-play-audio-probe.json。

> 2026-09-23 Q10最新：原项目Editor PID173216 编译后定向EditMode 8/8 PASS；真实选定LoganRuntime两mode cue已在NTSDSoundPlayer.PrepareBattleCuesAsync中加载，战斗目录封存后各有非空AudioClip，未发生未预热cue拒绝。两正式WAV SHA匹配且GUID各唯一。仅此EditMode夹具的封存预热已验；真实Battle Scene可听、退出重进、Player打包及正式EXE音画行为对照未验，Q10/总目标开放。详NTSD28-Q10-KNOCKOUT-MODE-SOUND-001 Record与editor-sealed-prewarm-result.json。

> 2026-09-23 Q10新证据：原项目Editor PID173216当前编译后，NTSD28-Q10-KNOCKOUT-MODE-SOUND-001 定向EditMode 7/7 PASS；真实选定LoganRuntime中两mode cue进入战斗预热集合，NTSDSoundPlayer将路径解析到Assets/NTSD/Sound/data，现有资源加载器将两正式WAV解码为非空AudioClip。两WAV SHA匹配，meta GUID在Assets各唯一。首次路径分隔符断言失败已修正测试，生产逻辑未改。完整封存预热、Scene可听、退出重进、Player打包及正式EXE听感未验；Q10/总目标继续开放。详Q10包Record与editor-audio-decode-rerun-result.json。

> 2026-09-23 Q10当前进度：原项目 Unity Editor PID173216 路径为 I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity；独立 I:\UnityPreject\test 自09-21已打开，不用于本任务验证。NTSD28-Q10-KNOCKOUT-MODE-SOUND-001 已在原项目编译并完成定向 EditMode 5/5 PASS，正式 mode KO 新事件选择双阵营 cue、同tick位置、无重播及门槛有覆盖；两正式WAV按SHA暂存。首轮断言误计普通命中声的RED保留在Record。预热实际加载、场景可听、退出重进、Player及正式EXE对照仍待；Q10与总目标开放。下方早期 PLANNED/Editor无进程记录为历史快照。

> 2026-09-23 08:56+08 原项目当前进度：Unity Editor PID173216（确认项目路径为本仓库）已对Q09图标当前测试重新编译并完成聚焦 job a825513e8f674351b5dc006768d49502，EditMode 1/1 PASS。前一轮 job 44ab32a4831547aa893ba0c8606465ef 因未声明预期BMPLoader错误日志失败，测试修正和两份终态JSON均已留证；生产代码未随之修改。`NTSD28-Q09-KILL-ICON-PUBLICATION-001 / FOCUSED_TEST_PASS` 仅覆盖7槽/3图发布、损坏Temp图保旧代及目录租约回收，图文屏幕consumer、同tick、Play/EXE像素、退出重进仍待。下方“原Editor无进程/旧PID33236活跃”均为不同时点快照；独立I:\UnityPreject\test不作本项目证据。Q09/R17及总目标开放。

> 2026-09-23 08:50+08 当前会话再核对：原项目 Unity Editor PID173216 于08:46经Hub启动，进程路径明确为 I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity，Responding=True；08:46之前“原项目无Editor”的记录仅是当时快照。原项目程序集仍停留2026-09-22 20:19/18:44本地时间，当前修订编译与Q09聚焦测试须以新会话新鲜状态再验。I:\UnityPreject\test 独立会话不作本项目证据。总目标开放。

> 2026-09-23 当前会话：原项目没有运行中的 Unity Editor；下方 PID33236/worker 活跃记录均为昨日快照。运行中的 I:\UnityPreject\test 是独立项目，不作为本项目编译或 Play 证据。Q09 图标当前修订仅离线编译通过，原 Editor 编译/NUnit 待；Q10 击倒音频静态首差详 NTSD28-Q10-KNOCKOUT-AUDIO-HANDOFF-AUDIT-001/REPORT.md。总目标开放。

> 2026-09-22 Q09最新接续：WORDS原Editor聚焦1/1 PASS、行快照当前五项5/5 PASS。图标包`NTSD28-Q09-KILL-ICON-PUBLICATION-001 / CODE_WRITTEN / OFFLINE_COMPILE_PASS`已接正式3 PNG/7类型原战斗预热、目录租约及无效PNG保旧代聚焦反例；生产脚本10:44Z原Editor编译，但测试10:54Z后修订尚未被原Editor编译/运行。原Editor PID33236与导入worker存活，桥接超时；禁第二Unity/computer-use。screen consumer、同tick、Play/EXE像素、退出仍待，Q09/R17和总目标开放。

> 2026-09-22 18:20 Q09当前接续：`NTSD28-Q09-KNOCKOUT-FEED-ROW-SNAPSHOT-001 / CODE_WRITTEN / OFFLINE_COMPILE_PASS`，战斗行数据、冻结副本、CentralOnly捕获、Host已发布mode与十槽名字输入已接；原Editor聚焦NUnit、正式同tick与图文像素仍待，Q09/R17开放。原Editor PID33236仍在原项目但Library DLL停留17:44；WORDS测试无结果。Unity-MCP `get_editor_state`返回09:44 UTC旧`sequence=9`缓存，**不可据其中tests.is_running=false判定当前终态**。不并发发测试、不启第二Unity或computer-use。场景静态HUDCamera/Canvas子序给出待验screen-layer候选，不能宣称层级对齐。详Q09 ROW-SNAPSHOT ACCEPTANCE、WORDS ORIGINAL-EDITOR-RUN-PENDING和ROW-PROJECTION-AUDIT；下方Q08是历史阶段快照。

> 2026-09-22 Q08击倒事件当前态：`NTSD28-Q08-NATIVE-KNOCKOUT-EVENT-STATE-001 / CODE_WRITTEN`。原项目标准致死新增正式逐次记录，既有KO统计只加一次；选定mode 70tick newest-tail过期、snapshot/restore、锁步校验、扩展/lockstep parity v2及reset已接，frozen Authority400 v3保持。Temp-only绝对targets离线runtime+Editor编译0错并确认新测试类型入DLL；**原Editor编译、focused NUnit、SelfCheck、同seed/Play、退出重进仍未验**。Q09须把70从正式mode数据发布并实现图文，Q10声音，R15版本化复核。WORDS旧TestRunner仍无结果，不并发发Q08请求、不启动第二Unity/computer-use。详Change Record；下方PLANNED为改前快照。

> 2026-09-22 Q08击倒事件实施准备：`NTSD28-Q08-NATIVE-KNOCKOUT-EVENT-STATE-001 / PLANNED`已有脚本修改前Task/Change，明确正式逐次事件、原有统计只加一次、newest-tail过期、快照/校验/恢复/清理必须同合同；尚未修改脚本或验证。正式记录在连续事件下可持续积累，不能任意封顶；已定赛前至少按runtime slots预热、超额仍保留事件并由现有内存边界计量分配的策略，具体code-path须在编辑前复核。Q09图文/Q10声音后置。详本包Task/Record；原Editor WORDS请求无结果，禁并发重跑、第二Unity及computer-use。

> 2026-09-22 Q08→Q09击倒提示链首差：正式标准致死除`knockout_count_358`外另记持久`WorldKnockoutEvent28`，经Session尾部过期、snapshot 30tick显示窗、mode `#killtext`、D3D11图文消费。原项目已实现致死统计，但所检脚本未见对应事件载体、mode读者或击倒行命令；pass ID声明不等于执行。Q08先补事件/时序且不得双加统计，Q09再做图文消费，声音归Q10。静态审计详`artifacts/diagnostics/NTSD28-Q08-Q09-KNOCKOUT-FEED-CHAIN-AUDIT-001/REPORT.md`；无新运行验收，Q08/Q09/R17开放。原Editor PID33236仍为唯一当前目标，WORDS请求消费后无结果，禁并发重跑/第二Unity/computer-use。

> 2026-09-22 最新Q09内容续证：正式mode子表`#killtext`所引用的三张`sprite/kill/{c,sk1,sk2}.png`已在原项目逐SHA精确暂存，新增四meta GUID各唯一；`LoganRuntime`现337 DAT/1031 PNG，正式1255 PNG仍缺224。仅是战斗击倒提示内容前置，Unity reader/实际画面/EXE像素未验，Q09/R17和总目标开放。原Editor PID33236仍是原项目；WORDS一次性focused请求已消费但无结果文件，不得报NUnit通过或并发重试。详`artifacts/diagnostics/NTSD28-Q09-NATIVE-KNOCKOUT-FEED-ICONS-STAGING-001/ACCEPTANCE.md`。下方1028/缺227是本包前快照；禁computer-use和第二Unity项目。

> 2026-09-22 最新内容与运行续证：正式EXE SHA `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033` 复核。Q09活动INKHUD `frame.dat`+`frame/INKHUD.dat`+7正式PNG已在原项目精确暂存并逐文件SHA核对；`LoganRuntime`现337 DAT/1028 PNG，仍缺227正式PNG。此为内容暂存，非HUD画面验收。原 Editor PID33236 的runtime DLL 07:58:15Z、Editor DLL 08:11:44Z晚于Q07 mode/Q09 WORDS源码与最终测试，重载已证；**WORDS一次性focused请求虽已消费，结果文件仍无，不能报NUnit PASS/FAIL或并发重试**。详`NTSD28-Q09-ACTIVE-FRAME-HUD-CONTENT-STAGING-001/ACCEPTANCE.md`及`NTSD28-Q09-WORDS-PUBLICATION-001/ORIGINAL-EDITOR-RUN-PENDING.md`；仅用原项目、不用computer-use。下方1021/缺234与旧程序集均为历史快照。

> Q07/R15离线编译门槛（2026-09-22历史快照）：原项目生成的csproj遗漏新`LoganModeComboInput.cs`和新Editor测试，直接MSBuild因旧项目输入报CS0246；在仅位于Temp的targets中明确补齐两文件后，原项目当前运行时与Editor源码离线MSBuild返回0，`UNITY_INCLUDE_TESTS`及两Compile项已核对。**当时原Editor Library程序集仍是04:23:43/44Z旧版本，Unity导入/编译、NUnit/Play及同seed V2 trace均未验。** 详`artifacts/diagnostics/NTSD28-Q07-OFFLINE-COMPILE-PROBE-001/REPORT.md`。不启动第二Unity，不使用computer-use。

> Q07正式mode combo联合激活（2026-09-22）：`NTSD28-Q07-MODE-COMBO-PUBLISHED-ACTIVATION-001 / CODE_WRITTEN / UNITY_COMPILE_PENDING`已把正式mode双DAT捕获并纳入五组件V2内容身份、候选新鲜度及共用seal首tick前World tuple；无mode保留V1。新增真实公开seal/reset/tick0 restore聚焦测试尚未被原Editor编译/运行，旧正式V1 trace与probe断言须按R15版本化。外部纯身份编译/Record-Ledger通过不是Unity验收；Q07/R15未闭。只用原项目，禁computer-use/新Unity副本。

> Q07正式mode combo输入投影（2026-09-22）：`NTSD28-Q07-MODE-COMBO-INPUT-PROJECTION-001 / CODE_WRITTEN`只新增`LoganModeComboInput.cs`+meta；复用现有DAT tokenizer，正式/暂存指纹相等、tuple1/1/50/1、反例与缺mode稳定性外部Add-Type检查通过；Ledger680/当前代码1覆盖PASS。**原Unity Editor程序集早于源文件，Unity编译/Play未验。** 生产World仍默认未激活，下一独立包须把mode双DAT输入与联合内容身份/发布新鲜度和`ApplyMatchConfig`重置后首tick前激活原子接线，不改菜单/结果页或Q06 producer。详同ID Task/Record。

> Q07正式战斗表现依赖六文件已精确暂存（2026-09-22）：`resource.dat`、`system.dat`、全局`SPARK.png`以及`mode.dat`、其`mode/ntsd.dat`子表、`combo_hits.png`进入原项目`LoganRuntime`，逐文件SHA与正式VFS一致；暂存现为335 DAT/1012 PNG，另70正式DAT未暂存，默认stage.dat仍暂缓。Unity生产仍读旧SPARK.bmp/20帧；更靠前的正式combo tuple虽已暂存，生产World仍默认RecordPresent=false/bound0/caughtact0，须先按Q07 `MODE-COMBO-LIVE-TUPLE-AUDIT-001`接入并验tick；Q09的99×79发布/画面另待。Q07/Q09仍开。详Q07 `GLOBAL-SPARK-CONTENT-ENTRY-001`、`MODE-COMBO-CONTENT-ENTRY-001` Task及Q09报告。未改Scene/ProjectSettings/脚本/旧资源。

> Q07序列化生产根当前态（2026-09-22）：`GameConfig.asset`已设`Assets/NTSD/Content/LoganRuntime`，Battle/Menu场景引用其GUID；脚本空值是新建对象默认，不是生产资产。正式/暂存catalog SHA一致、330索引DAT逐SHA一致；暂存331 DAT/1010 PNG/0 WAV。正式DAT另74个未暂存（背景24+25、其他data18、story/stage7），默认stage.dat部署仍暂缓，不能盲拷贝；详`artifacts/diagnostics/NTSD28-Q07-PRODUCTION-ROOT-CURRENT-STATE-001/REPORT.md`及CSV。原Editor正式内容相机单例1/1，但Player/Menu整链与全技能未验，Q07仍IN_PROGRESS；旧“生产根空”是历史快照。

> Q10音频入口回访（2026-09-22）：正式C25帧声音前20/声明顺序/跳空值/current→destination逻辑producer已在Q06接入并有既有focused/完整tick证据，不重做。正式catalog 330对象DAT词法cue清单976唯一文件路径，正式VFS 976在、Unity同路径71在/905缺；71重名WAV整文件SHA全异，PCM 28同/43异、17个时长异。原项目直接Battle Scene的AudioController `AudioList: []`，默认本地路径缺失cue无该场景clip回退。静态路径/内容差异不等于已证静音或听感差异；Player侧载/实际播放待核。正式playable音频resolver优先正式VFS，但当前项目暂存LoganRuntime无WAV，单改读该根无效；需独立确定外部VFS或有界音频部署，D-023不自动整体替换WAV。详`artifacts/diagnostics/NTSD28-Q10-FRAME-SOUND-ENTRY-READINESS-001/REPORT.md`及CSV；Q10仍WAIT_DEPENDENCY。

> Q08 mode 2/3/4 结果命令边界（2026-09-22）：正式EXE SHA已复核；正式playable `BattleFlow28` 在timer350发28/128/202，其所检`GameSession28`仅消费普通2→1，其余非零命令冻结旧World；Unity静态对应发命令并冻结，不派发模式专属前端。后续页面/动作`AUTHORITY_SURFACE_PENDING`，不能误接普通选人或重开。详`artifacts/diagnostics/NTSD28-Q08-RESULT-TRANSITION-HOST-AUDIT-001/MODE-SPECIFIC-CONSUMER-BOUNDARY.md`。原项目Editor新致死时序测试仍`COMPILE_PENDING`，禁computer-use及新的Unity验证副本。

> Q08正式PNG源纹理原项目验收（2026-09-22）：原Unity Editor PID33236定向`FormalSourceBindingProducesProductionCameraPixels` NUnit 1/1 PASS；原项目正式fingerprint、SourceTexture2D、145页预算回退、tick33四中央命令及960x540非白像素2012均有JSON/PNG，图已查看，Scene哈希不变。归档`artifacts/diagnostics/NTSD28-Q08-FORMAL-SOURCE-PIXEL-WITNESS-001/original-editor-focused-20260922-1247.xml/.json/.png`。这次运行的Editor程序集早于v2结束标记源码，故仅证明旧测试体通过；v2编译、正式EXE画面同条件及自然技能归属仍待，Q08未闭。禁computer-use。

> Q08第二次battle-only结果状态回访（2026-09-22）：早期两次正式内容Scene在atlas分配处OOM；后续`ATLAS-BUDGET-SOURCE-BINDING-001`修正后，同一个`SecondResultEntersOrdinaryUpperSelectionInSameWorld`正式内容Scene测试1/1 PASS（XML SHA-256 `E70E58EAA8D64150B314F843E8320F97D3CAD5CB3F5317A9A552B692D7692B77`）。这解除该脚本化双结果路径的OOM阻塞，仍非自然KO、原项目Editor或正式EXE可见验收；`SECOND-BATTLE-ONLY-LOGICAL-SELECTION-001`保持`RUNTIME_PENDING`。详本包Task/Record和`UNITY-SECOND-BATTLE-SOURCE-BUDGET.xml`。

> Q08 timer101 consumer回访：正式playable核心在101设置瞬时`result_record_created`，所检GameSession没有消费它创建持久记录，render snapshot以`result_visible`阶段/胜组显示；不要据此新增Unity持久结果记录。native timer/phase及实际render handoff/350出口仍需验。详Q08 RESULT-101-CONSUMER-AUDIT-001/REPORT，P-19/G-08表现例外保持，禁computer-use。

> Q08 mode-4 reserve 专项：正式playable完整GameSession一存活组timer1/350转202双跑一致；所检BattleConfig无Unity committed-result-reserve同条件入口，不能据源码负搜索删现有RESULT-RESERVE-09补reserve逻辑。详Q08 RESULT-GROUP-CARRIER-AUDIT-001/MODE4-RESERVE-CALLER-AUDIT；同条件正式EXE可见结果待证。

> Q08存活组下一依赖：NTSD28-Q08-RESULT-GROUP-CARRIER-AUDIT-001正式playable隔离源码GameSession组六例+计时锁定/80/101/144→350及真实Attack致死tick12计时0/tick13计时1双跑一致；隔离Unity组别4例、直接计时2例及完整tick计时1例目标RED。当前TeamIds[2]/HadBoth绑定结果UI与schema1快照/校验和，独立原生组/快照合同见同ID CARRIER-CONTRACT。下一Unity同条件自然命中与准确Change，未动生产，禁computer-use。

> Q08 revive2分支已聚焦接入：NTSD28-Q08-REVIVE-LIVES-LIVING-GROUP-001同SHA隔离Unity三例RED 1/3→PASS 3/3、相邻结果seam 2/2+3/3，原Editor/完整SelfCheck/真实战斗与Q08其它语义待验；详同ID ACCEPTANCE，禁computer-use。

> Q07 Sasuke预览隔离验证：原/隔离副本六项输入SHA一致，Unity EditMode正式PNG 1/1、预览类11/11及现有图形验证JSON/PNG PASS，画面已查看、Scene hash保持；原Editor Scene重载与现场预览仍待，不能宣称Q07关闭。详SASUKE-EDITOR-PREVIEW-FORMAL-IMAGE-001/PROGRESS.md，禁computer-use。

> Q07 Sasuke旧图磁盘owner回访：退场CSV为修改前快照；当前磁盘Scene旧GUID引用0、精确脚本文字仅保留旧BMP网格测试1处，旧空根DAT动态路径仍可达；Editor未Reload，删授权仍0。详SASUKE-EDITOR-PREVIEW-FORMAL-IMAGE-001/OLD-ASSET-OWNER-REFRESH.md。

> 当前Q07 `NTSD28-Q07-SASUKE-EDITOR-PREVIEW-FORMAL-IMAGE-001 / CODE_WRITTEN / UNITY_MODAL_RELOAD_PENDING`：正式sasu.png importer因RED1024先由独立子Task仅调nPOTScale并1/1PASS；预览PNG alpha、Editor示例/验证、Battle Scene禁用预览GUID及y881已精确写入，Scene仅两字段diff、HUDBg x30/旧BMP/Menu保持。现有Editor外部Scene修改弹窗阻断后续编译/预览图验收，已请用户手动Reload，禁computer-use/第二Editor；详PROGRESS。Q07未闭。

> Q08结果设置stage计数当前权威面修正：正式EXE SHA匹配，但其host只有战斗Scene loop/GUI/smoke及转换边界冻结的LFR回放，已检live path没有mode-4结果设置stage动作。不能继续把“直接取同条件EXE见证”当可执行下一步，更不能把赛前24背景ID写入结果计数；Unity零计数仍仅静态候选。详Q08 RESULT-STAGE-COUNT-AUTHORITY-AUDIT-001/SHIPPED-HOST-SURFACE-ADDENDUM.md；Q08其他项继续，禁computer-use。

> 当前Q07未索引旧`effect/weapon4.dat`静态调用链已审：它不是data.txt OID120的`chars/weapon4.dat`；默认正式/旧索引/手动刷新/正式catalog/Editor补丁未见已知reader，GUID无序列化owner。仅`STATIC_NO_KNOWN_READER`，不证明全局不可达，仍`deleteAuthorized=false`、未删。详UNINDEXED-EFFECT-WEAPON4-REACHABILITY-001/REPORT.md；Q07未闭，禁computer-use。

> 当前Q07 `NTSD28-Q07-LEGACY-DAT-IMAGE-RETIREMENT-GATE-AUDIT-001 / VERIFIED_STATIC_RETIREMENT_GATE_ONLY`：旧DAT138/旧索引图片383逐项退场门槛已记录；旧data.txt直接索引137 DAT，effect/weapon4.dat独立待审；保留legacy/Editor读取与Battle Scene旧sasuke预览引用阻止批量删除。521行均未获删除授权，正式默认仍用LoganRuntime。详REPORT/CSV；Q07及总目标未闭，禁computer-use。

> 当前Q08 `NTSD28-Q08-F4-PLAYER-CLOSE-OWNER-001 / VERIFIED_PLAYER_F4_CLOSE_SCOPE`：Player真实物理F4两次定向通过，第二次退出码0、tick3→3、Stopped/借用0；Editor物理键与F4拒绝路由通过，Scene保持。录像save-pending保护、Q08结果计数和总目标仍未闭，详本包ACCEPTANCE；下条只读缺口为历史。禁computer-use。

> Q08 `NTSD28-Q08-F4-CLOSE-OWNER-AUDIT-001 / CONFIRMED_PRODUCTION_EFFECT_GAP`：正式playable的F4经录像待保存保护关闭整应用；Unity已有物理键handoff但仅测试诊断消费，生产无关闭owner。先精确Task/Change再接战斗host/Player关闭与有序shutdown验收；不得改为返回菜单。Q07 Scene closure首Scene选择已异步询问，其他可独立工作继续，禁computer-use。

> 当前Q07旧图owner清单已复核：52个现存旧路径序列化引用=禁用Editor预览角色图1、Battle HUD/UI 26、Menu UI 14、GameConfig UI 8、地图2、通用阴影1。唯一旧角色图sasuke_0.bmp仍被Battle Scene预览及Editor测试引用，正式sasu.png仅为待验证重绑候选；清单见Q07 OLD-ASSET-REFERENCE-REFRESH-001/SERIALIZED-OWNER-CLASSIFICATION.md。没有重绑或删除，Scene/非战斗保持；Q07仍IN_PROGRESS，Q08 mode-4结果计数仍待权威同条件见证，禁computer-use。

> 当前Q08 `NTSD28-Q08-RESULT-STAGE-COUNT-AUTHORITY-AUDIT-001` 已闭只读边界：正式playable的24有效背景ID进入赛前post-roster菜单；mode-4战果设置stage action在所检playable live path未给出同条件规则。Unity正式根`RuntimeStageCount`静态为0但尚非已证首差，禁止直接写24。下一需正式EXE mode-4同stage/按键与Unity同条件见证；Q08其他有权威证据的子项可先行。详同ID REPORT；Q07仍IN_PROGRESS、Q06本地出口保持，禁computer-use。

> 当前Q07 `NTSD28-Q07-LEGACY-DATA-LAZY-LOAD-001` 限定VERIFIED：GameDataManager初始化不再隐式读取旧data.txt；空根显式加载保留，聚焦EditMode两次1/1、正式完整发布1/1、序列化根menu Play q07-lazy-menu-1 PASS。最终无行为的空覆盖删除后重新编译/聚焦通过，完整发布/Play为此前等效路径。Q08背景计数、Menu Scene空Build Settings、旧资源删除权限仍未闭合；详本包ACCEPTANCE，禁computer-use。

> 当前Q07 `NTSD28-Q07-NARUTO-CLONE-CENTRAL-PIXEL-WITNESS-001` 限定VERIFIED：正式Naruto物理键单例Play中OID33 pic1 tick12中央命令及生产相机投影区1087非清屏像素PASS；前两轮测试FAIL保留，Editor退出、双Scene SHA不变、Ledger651/13 PASS。仅单例命令→像素路径，EXE像素/排序/阴影/完整技能未闭；详同ID ACCEPTANCE.md。Q07/总目标ACTIVE，禁computer-use。

> 当前Q07旧资源动态引用审计`VERIFIED_STATIC_CALL_CHAIN`：正式根启用后`GameDataManager.InitializeSingleton`仍读旧`data.txt`，旧背景0条经正式对象发布保留，正式release有24条；`RuntimeStageCount`静态路径为0，Q08同条件运行时/正式source验收待做。旧资源删除授权0；详Q07 OLD-ASSET-REFERENCE-REFRESH-001/DYNAMIC-REACHABILITY.md。Q06本地出口保持、Q07/总目标ACTIVE，禁computer-use。

> 当前BATCH-04/Q07 `NTSD28-Q07-NARUTO-CLONE-SPRITE-BINDING-001`限定VERIFIED：正式Naruto L/D/J真实Play tick11 OID33 frame241/pic999无entry，tick12 frame242/pic1绑定正式ncl.png、key(33,1)、79×79、中央binding有效；fresh q07-naruto-clone-2 PASS。首轮测试目录缺失/过严frame240条件的FAIL保留并修正，Editor退出、双Scene哈希不变。仅逻辑到正式图片目录绑定，实际屏幕像素/层级/阴影/完整技能仍待Q09/Q12。Menu Scene Build Settings空表首差独立保留，Q07及总目标ACTIVE，禁computer-use。详本包ACCEPTANCE.md。

> 前项`NTSD28-Q07-OLD-ASSET-REFERENCE-REFRESH-001`限定`VERIFIED_STATIC_REFERENCE_REFRESH_ONLY`：旧695路径中686原路径同hash/GUID、6个Foot帧同GUID/hash迁位、3个旧UI路径无当前GUID owner；52个现存旧路径有序列化引用，含战斗Scene旧sasuke_0.bmp，删授权0。详Q07 OLD-ASSET-REFERENCE-REFRESH-001/REPORT.md。

> 当前BATCH-04/Q07 Windows Player构建子包限定VERIFIED：`NTSD28-Q07-PLAYER-COMPILE-GUARD-001`解决实际Player的28个条件编译错误，聚焦AI 11/11PASS；`NTSD28-Q07-WINDOWS-PORTABLE-CONTENT-PACKAGING-001`真实Mono构建errors0，1343文件/46594829字节及SPARK逐hash通过。GameConfig空根，实际Player内正式内容启动/自然技能仍待，Q07未交付。构建生成四个Odin文件保留，Scene保持；禁computer-use/非战斗改动/擅自清理。



> 当前BATCH-04/Q07 `NTSD28-Q07-STAGED-FORMAL-CALLER-PLAY-001` 限定VERIFIED：正式暂存内容真实App Play及退出后的Menu Play通过，正式指纹/三owner同key、World4、旧/新owner资源零残留、borrowers0、两帧Stopped；详STAGED-CALLER-PLAY-ACCEPTANCE.md。GameConfig空根、Scene与旧资源保持；Player打包/根映射、生产切换及自然技能/表现回访待办。禁computer-use/非战斗改动。

> 当前BATCH-04/Q07完整暂存发布已限定FOCUSED_TEST_PASS：`NTSD28-Q07-STAGED-FULL-PUBLICATION-001`真实Unity EditMode job1711ab45 1/1PASS，330对象/906有效图片通过生产候选、解码与manager/data/UI发布，显式调用现有OnDestroy方法后跟踪资源零残留。前三次RED证明EditMode夹具的DestroyImmediate未触发该回调，不能代替真实Play生命周期验收。Player打包/根映射缺口见Q07 BUILD-PORTABILITY-AUDIT，GameConfig仍空根、Scene SHA保持、旧资源未删；Q07未交付。下一真实Play caller/退出重进和Player可携带内容接线；禁computer-use/非战斗改动。

> 当前BATCH-04/Q07暂存内容候选身份已通过：`NTSD28-Q07-STAGED-CANDIDATE-IDENTITY-001`真实Unity EditMode job f0b2e9a7 1/1PASS，正式/项目本地候选均330对象、906有效图片，object/fusion/composite/visual指纹一致；Q01的1010是原始PNG引用，初次计数误用RED已留证。导入后1343文件逐hash无变化、Scene SHA保持、Ledger640PASS。GameConfig仍空根，发布/解码/caller/Play和生产切换待验，Q07未交付。详Q07 READINESS.md。禁computer-use、旧资源删除及非战斗改动。

> 当前BATCH-04/Q07已完成资源字节暂存：`NTSD28-Q07-PORTABLE-OBJECT-CONTENT-STAGING-001`精确复制1343文件/46,594,829字节，逐项SHA一致、无漏/额外文件；GameConfig仍为空根，旧资源和Scene未改。此为`VERIFIED_STAGING_ONLY`，Unity候选/发布/Play和Q07出口仍待。下一验证staged root的candidate/identity及真实加载，再决定生产root切换；禁删旧资源、computer-use和非战斗改动。详Q07 READINESS.md。

> 当前BATCH-03/Q06已`DELIVERED_SCOPED / Q06_LOCAL_EXIT`，详EXIT-RECONCILIATION-001/CLOSED-EXIT.md。原HOLD最后L-02～L-04 current Logan源码四例双跑、Unity完整tick4/4及组件1/1闭合；实测held零计数旧FrameDelay发射门槛已精确修复，稳定SelfCheck、真实Renderer Play3、有序关闭及Ledger通过。Q07正式DAT/角色图片迁移尚未开始，下一BATCH-04/Q07先只读catalog、parser/格式与引用/GUID清单，再精确迁移；Q08～Q12/R及用户例外保持。禁computer-use/非战斗/Scene/未列清单资源改动。

> 当前Q06 DAMAGE-REMAINING-SPECIAL-KINDS-001 限定VERIFIED：source17/source4双跑、实测RED后kind9/15、type3原始Z、state1002同步RNG及缺帧原始绑定已修；四组立即/下一tick PASS，稳定SelfCheck15:12:22Z、真实Scene Renderer18/关闭15:13:27Z PASS，Scene哈希未变。详ACCEPTANCE-FINAL4.md。Q06旧CPoint/DamageWriter代码出口阻塞均解除，下一仅做Q06/R最终回访和范围保持出口审计；未审前BATCH-03仍IN_PROGRESS，Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06 DAMAGE-REMAINING-SPECIAL-KINDS-001 RAW_Z_FOCUSED_PASS：type3 display/两种hit_Fa Z消费者修为原始整数Z，定向7/7、旧role-aware精确1/1 PASS；一次测试夹具CS0117已修复并重新编译，旧程序集运行不算验收。throwing tail只读发现state1002同步RNG/时序候选差异，下一源码见证+Unity RED。稳定SelfCheck/Renderer关闭及Q06出口复审未做；Q06 HOLD/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06 DAMAGE-REMAINING-SPECIAL-KINDS-001 STAGE_FOCUSED_PASS：显式type3 offset碰撞RED2→修后4/4PASS；stage精确Z边界RED3→修后3/3PASS，GT-05旧期望已同步修正。余frame-logic/display Z消费者和throwing tail权威分类，随后稳定SelfCheck、代表Renderer/有序关闭及Q06出口复审。Q06 HOLD/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06 DAMAGE-REMAINING-SPECIAL-KINDS-001 CANDIDATE_PASS：actual fa0e9603六代表+独立case4均PASS。根因LF2Entity.GetCollisionZInt把type3 hit_j当纵深偏移；准确删该fallback，保留显式offset待ownership审计。fixture补角色PostInteraction阶段，早期失败留证。详本包CANDIDATE-RESULT.md。direct17+6/replay6x2复用；下一explicit Type3VisualZOffset写入guard/throwing tail核查，再稳定SelfCheck/Renderer关闭。Q06 HOLD/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06 DAMAGE-REMAINING-SPECIAL-KINDS-001 IN_PROGRESS：同初态RED后准确DamageWriter/WeaponBase修kind9动作归属、kind15仅XZ/保留counter，direct17+6/following全0diff；snapshot replay6x2 PASS。actual candidate测试f680ff21在case4(hit_j857/900)候选0vs源1，原因未定，不跳过不改期望。详本包UNITY-PROGRESS.md。下一核collector/fixture/gate，之后稳定SelfCheck/Renderer关闭；throwing tail待，Q06 HOLD/Q07未开始。

> 当前Q06 DAMAGE-REMAINING-SPECIAL-KINDS-001 SOURCE17_PASS：actual relation+following双跑SHA9E1BE31E一致；kind9 hit_j优先级/owner/counter分支和kind15仅XZ/无201202排除已测源码确认。详本包SOURCE-RESULT.md。下一声明Unity对应fixture，先完整初态和实际candidate路径RED再最小生产修正；throwing tail仍待分类。Q06 HOLD/Q07未迁移，未重跑已闭任务，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06下一 NTSD28-Q06-DAMAGE-REMAINING-SPECIAL-KINDS-001 IN_PROGRESS/SOURCE_FIRST：DamageWriter余调用实际分类见本包CALLER-AUDIT.md。kind9与kind15出现动作/owner/速度语义候选差异，不能只换binder；type3 hurt中非type3受击帧分支不可由正式typed caller进入，旧weapon helpers无caller保持。throwing tail尚待。准确源码见证Task/Change已建，无生产修改；Q06 HOLD/Q07未开始。

> 当前Q06 CPOINT-SETTLEMENT-REMAINDER-FRAME-BINDING-001限定VERIFIED：24x2/旧8复用，following3+snapshot replay全PASS，稳定SelfCheck14:03:31Z，真实Renderer3/关闭14:04:35Z PASS（borrowers2→2、restore4→4、World/slots双pool0、两帧Stopped）。Scene哈希保持，详本包ACCEPTANCE.md。下一只核查DamageWriter剩余旧binder实际gate并处理确认差异；Q06仍HOLD/Q07未开始，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06 CPOINT-SETTLEMENT-REMAINDER-FRAME-BINDING-001 FOCUSED_TEST_PASS：原24x2初态0、实测旧wait1/高帧结算位置RED后仅BattleCpointWriter5调用改原生binder，job615b76c0新24x2+旧8共10/10PASS，raw50/descriptor/relation0diff。详本包FOCUSED-RESULT.md。下一代表following/replay和真实Renderer/关闭，稳定SelfCheck一次；尚非VERIFIED。随后DamageWriter余caller分类。Q06 HOLD/Q07未开始，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06 CPOINT-SETTLEMENT-REMAINDER-FRAME-BINDING-001 IN_PROGRESS/SOURCE24_PASS：实际settle/advance双跑一致，声明857..999 kind2可继续/负值翻向，隐式wait0无kind2早退；失效关系wholepass还执行victim孤立212，超时0/181 counter1。详本包SOURCE-RESULT.md。下一准确Unity fixture/真实pass RED，不只RunKind1误判孤立分支；Unity生产未改。Q06出口HOLD/Q07未开始，禁computer-use/非战斗/Scene/资源改动。

> Q06最终出口审计结论HOLD：原始reader表仍有活跃未闭调用，明确为CPoint settlement vaction及失效/超时raw0/181旧binder；DamageWriter generic/type3剩余调用待按实际gate分类，不批量改。详artifacts/diagnostics/NTSD28-Q06-EXIT-RECONCILIATION-001/EXIT-GATE.md（原要求/R后置映射）。下一NTSD28-Q06-CPOINT-SETTLEMENT-REMAINDER-FRAME-BINDING-001 READY_SOURCE_AUDIT，Task已建/无脚本改动。不重做已闭selector/throw。raw50已闭，Q06未交付/Q07未开始；禁computer-use/非战斗/Scene/资源改动。

> 当前Q06 RAW-REMAINING-THREE-BINDINGS-001限定VERIFIED：raw50/0已接，Unity字段10+完整采集2、工具22+87PASS；fresh Logan同内容源/Unity neutral3tick/6pairs/300字段0diff、50项相等，certificatefalse。详本包ACCEPTANCE.md。不是全场景证书；原raw47/3留历史。下一唯一Q06总出口/live-reader/R02/R04-R13/R16归属最终审计；已使用World epoch及后续mode/视听边界保留，Q07正式迁移尚未开始。禁computer-use/非战斗/Scene/资源改动。

> 当前Q06 RAW-REMAINING-THREE-BINDINGS-001 IN_PROGRESS：三carrier raw绑定50/0已接，Unity字段10/10PASS、tool22/22PASS（首次旧missing预期失败留存，新增三null拒绝）；尚待whole-capture/trace工具及fresh同身份源对照。六准确路径，无战斗算法变更。之后Q06总出口/R回访最终审计；未宣布Q06完成/Q07未迁移，禁止computer-use/非战斗/Scene/资源改动。

> 当前Q06总出口审计确认raw3是实际采集缺口：Runtime平台/环境三carrier已存在，但capture仍null、tool仍Missing。下一NTSD28-Q06-RAW-REMAINING-THREE-BINDINGS-001 PLANNED，准确5路径Task/Change已建，先字段/版本合同与RED再接线，尚未改脚本。详本包AUDIT.md。继续保留reader/R回访总审计、已使用World epoch及Q09shadow后置；不重做已闭生产。Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06 NATIVE-LIFECYCLE-STATE-CARRIER-001限定VERIFIED：已核对原386 XML/载体实际Play及后继full-driver Renderer224/672ticks、碎片和零残留关闭；稳定SelfCheck复用，未重跑。详本包EXIT-RECONCILIATION.md/证据JSON。下一Q06总出口逐项审计及R02/R04-R13/R16后置归属核对，不能只据子任务标签宣告DELIVERED；已使用World epoch/raw3/内容模式视听边界保留。Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06 BDEFEND-FIELD-FAMILY-UNITY-001限定VERIFIED：旧四256通过复用，新增真实Renderer八分支13:27:15Z全PASS（raw/Shadow/legacy241/held-free），13:27:16Z恢复4→4/关闭全0/两帧Stopped；SelfCheck13:19:44Z复用，Scene保持，无生产改动。详本包ACCEPTANCE.md。下一核对NATIVE-LIFECYCLE-STATE-CARRIER父出口与已闭frame/fragment后继证据，再Q06剩余出口；不重做Bdefend。Q06未完/Q07未迁移，平台shadow Q09，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06父出口审计：BDEFEND原四256在两个后继XML均PASS，旧失败不再当前阻塞；FORMAL-CANDIDATE-ENTRY-ORACLE限定VERIFIED，FIELD-FAMILY父改FOCUSED_TEST_PASS但保留真实Renderer字段独立性出口。详本包PARENT-EXIT-AUDIT.md/固定XML证据JSON。下一只补四语义分支代表实际Renderer与legacy241/恢复，不重跑256或已验source/SelfCheck。State13包已闭；Q06未完/Q07未迁移，平台shadow Q09，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06 STATE13-EXIT-TAIL-RETIREMENT-001限定VERIFIED：source6/focused6复用；两处旧15粒子SelfCheck断言已按源证据修正且失败留存，13:19:44Z SelfCheck PASS；13:20:38Z真实Renderer三代表PASS，13:20:39Z恢复4→4/关闭全0/两帧Stopped。Scene哈希保持。详本包ACCEPTANCE.md。下一核对Q06父记录出口与既有证据，优先BDEFEND字段/入口回链，不重复已验矩阵；平台shadow Q09保留。Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06 STATE13-EXIT-TAIL-RETIREMENT-001 FOCUSED_TEST_PASS：实测旧exit各15额外对象/audio1/legacy额外60；准确断开LF2Entity旧producer调用，source6对应job b67b2ac0全PASS。保留C17批准legacy1/virtualN30/state18/反射slot容量helper，未删除资源或非战斗。详本包FOCUSED-RESULT.md。下一稳定SelfCheck/真实Renderer/关闭，尚非VERIFIED；CPoint已闭，Q06未完/Q07未迁移，禁computer-use。

> 当前Q06 STATE13-EXIT-TAIL-SOURCE-WITNESS-001 FOCUSED_TEST_PASS / SOURCE6：完整driver双跑SHAE7C23807，state13/action200退出/保持与neutral无新增/RNG/audio，state18正向7粒子。详本包SOURCE-RESULT.md。Unity旧分支候选差异尚待同例RED，不能仅据source直接删除；selfcheck反射slot-counter和virtual N30职责须保护。下一准确Unity Task/Change；CPoint限定VERIFIED，Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06 CPOINT-INPUT-ACTION-SELECTION-001已限定VERIFIED：即时/后继370×2、replay6、SelfCheck12:57:55Z、真实pooledRenderer2与Q05恢复/关闭全0/Stopped2framesPASS；见本包ACCEPTANCE.md。下一state13/action200旧late tail只读current-source审计，再精确新Task/Change；不重做已闭CPoint/OPoint。平台shadow明确Q09未完成，Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06 CPoint包 FOCUSED_TEST_PASS / FOLLOWING370x2_REPLAY6_PASS：正式Controller+human roster夹具接线后job0fccc80a各370全初态/即时/后继raw47+B2/RNG/关系零差异；job49f1368c六代表snapshot重放及两阶段checksum全PASS。无生产输入改动，旧unbound失败保留。详本包FOLLOWING-REPLAY-RESULT.md。下一稳定SelfCheck+真实Renderer代表/关闭验收；平台shadow Q09、state13/action200审计保留，Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06 CPoint包 FOCUSED_TEST_PASS / FOLLOWING_INPUT_BOUNDARY_DIFFERENCE：扩展B2/RNG/关系初态370×2均0差异；jobc4d57852后继各1194差异（input886/其余308），首差previousMask/edge/history。详本包FOLLOWING-FIRST-DIFFERENCE.md。下一核对source非AI sample_pending与Unity active-human/controller门的等价接线/可达性，禁止夹具强写预期或直接重写输入系统。即时370×2/旧throw已验保持；replay/Play未验，Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06 CPOINT-INPUT-ACTION-SELECTION-001 FOCUSED_TEST_PASS / IMMEDIATE370_TWO_PROFILES：夹具输入投影纠正后RED各2242差异/初态0；准确writer两方法接有序选择、零取消和native绑定，jobac32c386新370×2零差异+旧throw两profile共4测试PASS。详本包IMMEDIATE-RESULT.md。下一完整input/RNG/关系初态与following tick、replay及稳定包验收；不标VERIFIED。平台shadow Q09、state13/action200审计保留，Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06下一任务恢复 NTSD28-Q06-CPOINT-INPUT-ACTION-SELECTION-001 / SOURCE_CAPTURE_ONLY：source370双跑字节/manifest已复核，Unity仍A/T/J逐次Apply，完整选择链待RED/实施；详本包RETURN-AUDIT.md。总表0.11/0.13已纠正旧OPoint和schema游标。平台fulltick/replay3通过，原阴影出口显式归Q09且仍未完成，parent不升格VERIFIED。state13/action200审计待；Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前平台 parent FOCUSED_TEST_PASS / FULLTICK_REPLAY3_PASS：actual Unity job af41921a 三代表连续两tick及snapshot重放全部通过，12逐tick投影/XML已归档；详 artifacts/diagnostics/NTSD28-Q06-PLATFORM-TRANSACTION-001/FULLTICK-UNITY-RESULT.md。无生产新增，复用稳定SelfCheck/Renderer。下一核对parent原阴影出口与Q09明确交接及Q06剩余清单，不能静默缩小VERIFIED范围。Q06未完/Q07未迁移，禁止computer-use/非战斗/Scene/资源改动。

> 当前平台 parent：完整 source tick 三代表已双跑一致（SHA3079EE34），原21分段输出字节不变；首tick建link/次tick跟随已观察。详 artifacts/diagnostics/NTSD28-Q06-PLATFORM-TRANSACTION-001/FULLTICK-SOURCE-RESULT.md。下一仅补Unity相同三代表连续tick及snapshot replay；尚未验证，不升格VERIFIED。复用稳定SelfCheck/Renderer，Q09阴影回访保留；Q06未完/Q07未迁移，禁止computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-FRAME-MOTION-TAIL-001 已限定VERIFIED：source12/61+Unity17+following/replay3，稳定SelfCheck复用；真实Renderer四代表（平台0/physics20、tail7/9）12:22:51Z全PASS，scenechecksum/borrowers2→2，Q05 restore/全0/两帧Stopped12:22:52Z PASS。详tail ACCEPTANCE.md。平台parent仍FOCUSED_TEST_PASS，下一补其actual source完整tick及snapshot replay（不能用分段API组合或tail测试代替），mixed3已验；Q09阴影consumer回访保留。Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-DESTROY-POOL-OWNER-001 已限定VERIFIED。真实factory Renderer三类first12:18:20Z/reenter12:19:39Z各3PASS，原pool归还/独立World/重复Destroy通过，scene checksum及borrowers2→2；各自Q05 restore4→4/Worldslots双pool0/两帧Stopped PASS（末次12:19:40Z）。详本包ACCEPTANCE.md；稳定SelfCheck复用，无生产追加。下一平台/frame-motion代表Renderer语义验收仍独立待做，Q09阴影回访保持；mixed3/旧late两项已闭证据保留。Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-PLATFORM-MIXED-CANDIDATE-001 FOCUSED_TEST_PASS / UNITY3_AND_STABLE_SELFCHECK_PASS。actual World候选job bde7da1f3/3PASS，runtime/range候选0/1/0及integer/preciseY/reference/link一致，无需生产修改；本组一次fullSelfCheck 2026-09-21T12:14:34.368890+00:00新鲜PASS，详本包UNITY-RESULT.md/SELF-CHECK.json。下一准确代表Renderer平台/frame-tail与Destroy原pool归还/退出重进零残留验收，不能把SelfCheck升格整包关闭；Q09阴影consumer回访保留。旧late两失败已闭，frame-tail17+following3复用。Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-PLATFORM-MIXED-CANDIDATE-001 IN_PROGRESS / SOURCE3_PASS。actual source平台20/rider21与attack19或22：ordinary候选0/1/0，证明吸附后用更新integerY而非preciseY；双跑SHA62D0258D。初版攻击者自身被平台吸附导致全0已留证，移至范围外并保留世界攻击框后预期通过。详本包SOURCE-RESULT.md。下一准确Unity3例fixture声明/实际候选入口验证，生产未改；旧late两失败已闭，frame-tail17+following3复用。稳定SelfCheck/Renderer/Destroy关闭与Q09阴影回访待，Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-PLATFORM-MIXED-CANDIDATE-001 IN_PROGRESS / SOURCE_FIRST，准确单CPP测试平台吸附前后slot顺序与ordinary窄几何交错，Unity未改。两旧late失败已限定关闭；Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-STATE2000-FACING-ORACLE-001 已限定VERIFIED / TEST_ORACLE_ONLY。actual source step_frame_slot四例双跑SHAF4846CA2，证明state2000 held正零负速度保留朝向/negative-next才翻转；仅旧测试改名和源期望+翻转/action/counter控制，job492179a3两测试PASS，CS0/Ledger631PASS。详本包ACCEPTANCE.md，无生产改动。此前late两失败已分别由RECOVERY-NOOP-FIXTURE与本包闭合，不改写旧23中21PASS/2FAIL历史。下一回平台mixed普通交互及稳定SelfCheck/Renderer/Destroy关闭；frame-tail17+following3保持，Q09阴影回访保留。Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-STATE2000-FACING-ORACLE-001 IN_PROGRESS / SOURCE_FIRST，先actual source step_frame_slot正零负速度/negative-next控制见证，不改生产或先反转旧断言。回血no-op测试前提已关闭；Q06未完/Q07未迁移。

> 当前 NTSD28-Q06-RECOVERY-NOOP-FIXTURE-001 已限定VERIFIED / TEST_ONLY：只给旧no-op测试显式World phase12/3=1，原HP/PP/NoOp断言保留，额外确认partial调用不推进相位；job b24b523e非周期+周期对照2/2PASS，CS0/Ledger630PASS，详本包ACCEPTANCE.md。此前两个late失败现解决回血前提一项；state2000朝向仍待actual source step_frame_slot见证，不能恢复旧兼容分支来迎合断言。frame-tail17+following3证据保持；平台mixed/稳定SelfCheck/Renderer/Destroy关闭及Q09阴影回访仍待。Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-RECOVERY-NOOP-FIXTURE-001 IN_PROGRESS。已确认旧no-op测试未设非恢复World相位；仅准确测试方法先声明后修前提，保留HP/PP/no-op断言。state2000朝向失败仍待源码见证，平台和frame-tail已验职责保持；Q06未完/Q07未迁移。

> 当前 NTSD28-Q06-FRAME-MOTION-TAIL-001 FOCUSED_TEST_PASS / FOLLOWING_REPLAY3_PASS。实际source fulltick rows1/7/9双跑SHA4F9E3443，原immediate字节保持；Unity夹具AI配置顺序修正后jobe49f2970三个following+snapshot replay全PASS（位置/速度/reference/delay源对照+完整runtime回放checksum；未对照全部native raw/RNG）。详本包FOLLOWING-REPLAY.md，复用17既有证据。平台阴影缺口已审：snapshot/copy/legacy/central未消费offset，shared地面锚点还供脚下标记，必须仅改shadow，见parent SHADOW-CONSUMER-AUDIT.md；Q09回访依赖保留。下一Q06 mixed平台/普通交互与两个late失败回访，再稳定包SelfCheck/Renderer/Destroy关闭；Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-FRAME-MOTION-TAIL-001 FOCUSED_TEST_PASS / SOURCE_TAIL17_PASS。source12/61复用，Unity RED12中5PASS/7尾部FAIL→准确LF2Entity尾部接入后job0bf80a40共17/17PASS（source12+旧kernel5）；新鲜compile CS0，详本包UNITY-FOCUSED.md。现有整数kernel/linked保持，下一真实following tick/replay及平台mixed/shadow，再稳定包SelfCheck/Renderer关闭。两个late失败与Destroy-owner Renderer门槛仍待。Q06未完/Q07未迁移，schema17/25/28 raw47/3；禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-FRAME-MOTION-TAIL-001 IN_PROGRESS / SOURCE12_TABLE61_PASS。actual source API双跑一致SHA2AD148A4，12场景/61表断言通过，身份匹配；详本包SOURCE-WITNESS.md。自身整数速度kernel保留，正delay四分之一与float dx/dy/dz、linked-before-own及pending/失效link已获源证据；Unity RED/实现尚待准确声明。平台/Destroy-owner未验出口和2个late失败回访保留，Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-FRAME-MOTION-TAIL-001 IN_PROGRESS / SOURCE_WITNESS_FIRST。权威frame_motion.cpp证实自身dvx/dvy/dvz按integer读取，纠正此前“自身float速度缺口”的推断；不改已匹配integer kernel。缺口为正DelayTimer134速度×0.25和float dx/dy/dz尾部，先准确两source诊断脚本。平台与Destroy-owner未验出口及2个late失败回访保留。Q06未完/Q07未迁移，schema17/25/28、raw47/3；禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-PLATFORM-TRANSACTION-001 FOCUSED_TEST_PASS / PREVIOUS_Y_CORE：3个成功mechanics尾部记录旧integerY，source20RED1→virtual/ECS+skip8PASS，type1core4PASS。相邻FrameAdvanceRuntimeSnapshot实际23中21PASS/2FAIL（late回血400vs401、state2000朝向rightvsleft），代码入口不经过本次physics但未证历史先前通过；保留失败，需current-source回访，禁止直接改断言。详本包PHYSICS-HISTORY-STAGE.md。下一优先own-frame native float/delay134/dxdy尾部完整源见证，再mixed/fulltick/replay/shadow；Destroy owner子包Renderer/关闭待。schema17/25/28、raw47/3，Q06未完/Q07未迁移/目标ACTIVE；禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-DESTROY-POOL-OWNER-001 FOCUSED_TEST_PASS / LOGIC_ONLY_OWNER_RETURN：三个Destroy override先保存原pool再detach，RED9b993b58三类均失败→e7476287三类全PASS，含独立World隔离/重复Destroy/关闭。详本包FOCUSED-RESULT.md；Renderer路径及稳定包关闭仍待，不能标VERIFIED。下一回平台parent补physics previousY与自身float/delay/dxdy尾部，再mixed/fulltick/replay/shadow，最终联合验证保留本包。HEAD外部推进72ecf16e已保留。schema17/25/28、raw47/3，Q06未完/Q07未迁移/目标ACTIVE；禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-DESTROY-POOL-OWNER-001 IN_PROGRESS / RED_FIRST：独立池归还回访，准确3个Destroy override+新fixture已声明；先逻辑对象3类实际factory/隔离World复现。平台父包保持未关闭，Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-PLATFORM-TRANSACTION-001 FOCUSED_TEST_PASS / LINKED_MOTION_REPRESENTATIVES：linked X/Z/Y已接，初RED21→eaef8c44中20PASS+移除案例90ba7a6a单独1PASS；不是一次21PASS。详本包MOTION-STAGE.md。移除测试现用既有Free契合source直接despawn；另发现Destroy在Unregister后找pool导致独立World清理失败风险，下一单独精确Task/Change回访，不能因fixture绕开而丢弃。随后actual physics previousY、自身float/delay/dxdy尾部、mixed candidate/fulltick/replay/阴影待。schema17/25/28、raw47/3；Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-PLATFORM-TRANSACTION-001 FOCUSED_TEST_PASS / CANDIDATE_SOURCE21_PASS：有平台时slot有序ordinary/platform交错，current ITR+snapshot frame、float32 dvy、点接触/吸附已接；source21×default/brute42PASS，相邻11PASS，初RED保留。详本包CANDIDATE-STAGE.md。下一先准确声明linked motion与physics previousY生产路径，扩展afterMotion/真实following/replay，再阴影。history当前仅fixture种入，未实际生产；非完整平台验收。schema17/25/28、raw47/3；Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-PLATFORM-TRANSACTION-001 FOCUSED_TEST_PASS / CARRIER_TRANSPORT_ONLY：新增platformSlot/shadowOffset/previousY持久carrier，初RED3→恢复/hash/identity4PASS，trace合同87PASS，source诊断编译exit0。新联合schema entity17/aggregate25/checksum28（core12/shell2/2），raw47/3未改。详本包CARRIER-STAGE.md；候选吸附RED2未修，history producer/linked motion/阴影consumer尚未接入。下一声明准确有序候选与位移路径，不称平台完成。Q06未完/Q07未迁移/目标ACTIVE；禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-PLATFORM-TRANSACTION-001 COMPILE_PASS / CANDIDATE_RED_CONFIRMED：job a7ca8aa2执行4，边界2PASS/普通吸附2FAIL（Y期望-20实际-10），初态位置/reference通过。证据本包candidate-red-a7ca8aa2/REPORT.md；仅投影测试，production/history/schema/阴影未实施。下一先精确完整carrier/候选有序路径合同后实施，不跑无关全套。Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前 NTSD28-Q06-PLATFORM-TRANSACTION-001 IN_PROGRESS / FOCUSED_RED_FIRST：准确新平台测试路径已声明，先 nominal/strict-edge 的真实候选投影；生产未改，完整history/schema/阴影仍待。Q06未完/Q07未迁移。

> Q06 平台接入审计更新（2026-09-21）：只读确认普通候选 bdy 过滤不可用于平台点接触，需保持每对 ordinary/platform 双向交错及吸附后缓存语义；已定位角色/共享非角色/武器物理同步和 legacy/central 阴影消费者。详 `artifacts/diagnostics/NTSD28-Q06-PLATFORM-TRANSACTION-SOURCE-WITNESS-001/UNITY-INTEGRATION-AUDIT.md`。下一完成 shared-character/weapon 提前整数同步及 snapshot/hash/capture 精确路径后建 Unity Task/Change。生产尚未实施；本轮无 Unity 测试，复用源 21/277，按受影响分支验证。Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-PLATFORM-TRANSACTION-SOURCE-WITNESS-001 FOCUSED_TEST_PASS / SOURCE21_FORMULA277_PASS。source-final双跑SHA9671B8D3，21/277通过；多平台顺序、snapshot/current分工、slot0/失效link、fractional取整及physics previousXYZ已测，详EXTENDED-ACCEPTANCE.md。诊断2脚本，Unity未实施；API组合不是fulltick证书。下一先精确审Unity physics整数同步/候选顺序/frame-motion及carrier reset/copy/snapshot/hash/shadow合同，再建准确实现Task。旧environment/碰撞reference和OPoint出生已闭职责保持。Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-PLATFORM-TRANSACTION-SOURCE-WITNESS-001 IN_PROGRESS / SOURCE10_PASS_EXTENDED_CONTRACT_PENDING。准确单CPP actual候选→frame-motion已build/双跑SHA74FD90C9，10场景120检查PASS；整数吸附与精确Y不同步、整数origin位移及type3状态门槛已测。详同ID PROGRESS.md。下一补多平台/顺序、snapshot/current、ITRdvy、sentinel及完整tick previous-position源合同，再声明Unitycarrier；尚无Unity生产修改/不称平台已对齐。旧environment/碰撞reference已闭职责保持，OPoint事务限定VERIFIED。Q06未完/Q07未迁移/目标ACTIVE；禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-PLATFORM-TRANSACTION-SOURCE-WITNESS-001 IN_PROGRESS / SOURCE_WITNESS_FIRST。准确单diagnostic CPP已预声明，先actual候选op30→linked frame motion源见证，Unity生产未声明未改。CollisionYReference reset与environment伤害/投掷保持关闭；平台需previous-position/slot/shadow完整合同，不以raw null推断环境carrier缺失。父OPoint事务已限定VERIFIED；Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-OPOINT-MATERIALIZER-TRANSACTION-001 已限定VERIFIED / NATIVE_ORDINARY_LATE_MATERIALIZATION。source23+rest3+boundary6；immediate46、followingReplay2、boundary14、resolver3/V3各329PASS；稳定SelfCheck、Renderer3/关闭及最终guard受影响Renderer2/10:38:57Z关闭PASS。scenechecksum/borrowers2→2、restore4→4/finalWorldslots两pool0/Stopped2frames；详同ID ACCEPTANCE.md。父emission调度仍属上游，不把局部证据当全部场景证书。下一回Q06剩余依赖审计，优先platform/environment及父生成门槛所有权，不重做已关闭出生事务。Q06未完/Q07正式DAT图片未迁移/目标ACTIVE；禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-OPOINT-MATERIALIZER-TRANSACTION-001 IN_PROGRESS / ACCEPTANCE_GATES_PASS_CLOSURE_AUDIT_PENDING。state3003源3/113双跑SHAca4698c6确认无新rest并保留7；精确旧SelfCheck断言纠正后PASS。真实Renderer3于10:31:32Z/关闭10:31:33Z均PASS，scenechecksum/borrowers2→2、restore4→4/finalWorldslots两pool0/Stopped2frames。详同ID ACCEPTANCE-PENDING.md；复用immediate46/followingReplay2/resolver3/V3各329证据。下一收尾审查first-record Kind/Oid整帧早退与random越界action未覆盖分支，必要精确source/test后再修，不能直接宣称全事务完成。Q06未完/Q07未迁移/目标ACTIVE；禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-OPOINT-MATERIALIZER-TRANSACTION-001 IN_PROGRESS / FOLLOWING_REPLAY_PASS_SELF_CHECK_RED。immediate46复用；source0/15真实fulltick+snapshotReplay2PASS；组件准入复用既有resolver修正后capacity/normal/missing3PASS；旧V3指纹fixture精确纠正后两profile各329出生值PASS。SelfCheck旧lives断言按source修正后，重跑停state3003额外bilateral10/mutual40 vrest旧断言；详同ID FOLLOWING-PROGRESS.md，失败均归档。下一先补type3/state3003关联对象源rest见证，不能直接反转断言。真实Renderer probe0/5/15已声明编译未运行，Play/关闭待SelfCheck；Q06未完/Q07未迁移/目标ACTIVE；禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-OPOINT-MATERIALIZER-TRANSACTION-001 IN_PROGRESS / IMMEDIATE_46_PASS。准确四生产路径共用native ordinary birth已写；先6代表PASS，再稳定包job90b6b8f4联合46/46PASS，全部before/after/RNG差异0。详同ID IMMEDIATE-PASS.md及immediate-pass-90b6b8f4。仅logic-only两late caller，真实Renderer/后续tick/replay/稳定包SelfCheck与Play关闭仍待；独立review受agent limit未运行，不冒充完整验收。Scene dirtyfalse/root14/hashBCD1047B保持，Ledger625/122PASS。下一既有fixture补source0/15真实following/replay并审真正Renderer入口，不重跑无关全套。Q06未完/Q07未迁移/目标ACTIVE；禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-OPOINT-MATERIALIZER-TRANSACTION-001 IN_PROGRESS / ADMISSION_WRITTEN_PARTIAL_TEST_PASS。准确两late caller已修kind/缺OID/缺action/容量终止控制；compile CS0，focused job78c84f30仅8例4PASS/4FAIL，19/20通过、18/22拓扑匹配但初始化5项差异仍失败，断言未弱化。详同ID admission-78c84f30/REPORT.md。下一先声明准确初始化生产路径，再补共同birth/RNG/native action/link/double motion；本轮未跑fullSelfCheck/Play。Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-OPOINT-MATERIALIZER-TRANSACTION-001 IN_PROGRESS / UNITY_RED_CONFIRMED。归档job8a2ae10b77b3436188b906a582d62fb9：46例初态全部零差异，生成后2PASS/44FAIL，仅满容量双入口控制通过；详同ID RED-EVIDENCE.md。两actual late caller均为logic-only materializer，不代表Renderer factory/Play已验。生产路径尚未声明、尚未修改；下一审准入/初始化共用边界并先登记准确路径。按影响分支验证，稳定包才集中回归，复用未变证据。Q06未完/Q07未迁移/目标ACTIVE；禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-OPOINT-MATERIALIZER-TRANSACTION-001 IN_PROGRESS / UNITY_RED_FIRST，准确新EditorTests已声明，生产未改。source23/360/双跑身份已验；先两actual late caller逻辑载体代表，Renderer factory/Play另验，不冒充两factory完整通过。Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-OPOINT-MATERIALIZER-SOURCE-WITNESS-001 FOCUSED_TEST_PASS / SOURCE23_FORMULA360_PASS；最终source-final双跑SHA12ACA913…250FD7，正式EXE/closure匹配。详同ID REPORT；准确source2脚本，无Unity生产修改。下一为两actual materializer建立精确Unity RED fixture Record，比较完整初态/出生字段/RNG/link/admission及代表following，再声明生产路径；保护clone/piece/vitals已验职责。Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-OPOINT-MATERIALIZER-SOURCE-WITNESS-001 IN_PROGRESS / SOURCE_WITNESS_FIRST；准确单诊断CPP已预声明，Unity生产未改。父OPoint剩余consumer回访，复用vitals/weaponHp/owner成果；补完整生成顺序、RNG、continuation/depth/link/admission代表。前post/display VERIFIED；Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06游标：NTSD28-Q06-OPOINT-REMAINING-CONSUMER-AUDIT-001 READ_ONLY_RETURN / 父frame依赖已解除，优先R06/R12完整materializer。post/display保持VERIFIED。最新只读审计见artifacts/diagnostics/NTSD28-Q06-REMAINING-DEPENDENCY-RETURN-20260921/AUDIT.md；总表0.11/0.13已纠正旧游标/schema/display状态。平台op30缺完整producer及linked motion，需要previous-position/shadow联合合同；environment两字段已有carrier，raw null不代表未实现，KO事件归Q08。下一先准确source witness Change与两factory调用链，禁止重复旧vitals/owner/clone职责。Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/非战斗/Scene/资源改动。

> 当前Q06：NTSD28-Q06-NATIVE-POST-DISPLAY-RESOURCE-TRANSACTION-001 已限定VERIFIED。准确三脚本；source2379零差异，focused10/10及真实frame/replay+相邻joint6/6通过；一次SelfCheck09:37:56Z、真实pooled Renderer代表1于09:39:41Z、Q05关闭09:39:42Z PASS。scene checksum/borrowers2→2，restore4→4/finalWorldslots两pool0/Stopped两帧；Scene dirtyfalse/root14/hashBCD1047B…0E9FB6。独立review PASS/GO-WITH-NOTES；synthetic catalog和source-derived405边界保留，不冒充正式DAT/画面对齐。下一回Q06剩余live-path/platform/environment及Host依赖清单，先只读定位，不重做已关闭display/post职责。Q06未完/Q07未迁移/目标ACTIVE；禁止computer-use/非战斗/Scene/资源改动。

> 当前Q06：NTSD28-Q06-NATIVE-POST-DISPLAY-RESOURCE-TRANSACTION-001 FOCUSED_TEST_PASS / 10_CASES_AND_2379_VECTORS。job8866ce8e实际10/10 PASS，sourceVectorsExecuted=2379/differences=0；local/native pending两例及raw-action后computer两方向通过。准确三脚本生产已写；保留初RED、pending夹具纠正及computer真实RED。仍待真实frame body接续、组合snapshot/replay代表、稳定包联合/一次SelfCheck/Play关闭；不是VERIFIED。Q06未完/Q07未迁移/目标ACTIVE，禁止computer-use/非战斗/Scene/资源改动；复用未变证据，不重跑全角色。

> 当前执行NTSD28-Q06-NATIVE-POST-DISPLAY-RESOURCE-TRANSACTION-001 IN_PROGRESS / SOURCE_REUSE_AND_FOCUSED_RED_FIRST，准确三路径Task/Record已建。post2379 SHA4C8CC601…8B644/sourceclosure07CD47复用，先向量及生产位置RED；新无状态writer整事务，不重做preHP/MP/display。前clone/display均VERIFIED；Q06未完/Q07未迁移/目标ACTIVE，禁止computer-use/非战斗/Scene/资源改动。

> 当前Q06：STATE9996-DIRECT-SPAWN-TRANSACTION-001与父NATIVE-DISPLAY-PROGRESSION-001均限定VERIFIED。子包准确13脚本，source12/813、immediate/shared20、following/C25六、SelfCheck09:06:05.218Z、真实Renderer2/关闭09:07:01Z PASS；父display新joint13/980 PASS。保留初RED/旧identity及nativeHP10自检失败和纠正证据；限定诊断source/单following tick/相对容量，不冒充EXE画面。Scene dirtyfalse/root14/hashBCD1047B…0E9FB6保持，raw47/3/schema不变。下一唯一任务NTSD28-Q06-NATIVE-POST-DISPLAY-RESOURCE-TRANSACTION-001 READY_FOR_EXACT_PRECHANGE_RECORD，WAIT_DISPLAY_OWNER解除：复用2379source，先精确Change/路径与RED，补405非零motion；尚未生产实施。Q06未完/Q07正式DAT图片未迁移/目标ACTIVE；禁止computer-use/非战斗/Scene/资源修改，不重做已验职责。

> 当前Q06：NTSD28-Q06-STATE9996-DIRECT-SPAWN-TRANSACTION-001 FOCUSED_TEST_PASS / IMMEDIATE_AND_SHARED_BIRTH_REPRESENTATIVES；非VERIFIED。准确10脚本（source2/test2/生产6），native direct500/implicit/current descriptor/pending/RNG后容量及两factory已写；初RED12中9FAIL/初态全同，修复后clone12+poolreuse1+admission5 PASS。piece旧V3前fingerprint先失败，准确声明后只修测试身份前置；shared piece+state18两个代表再PASS，共20项有效证据。编译CS0、scene dirtyfalse/root14/hashBCD1047B…0E9FB6、Ledger622/113 PASS。下一补source following0/2（helper后driver确实再生5个，禁止counter补偿）、renderer代表、旧C25 native oracle后稳定包验收；本轮未跑fullSelfCheck/Play。详本包PROGRESS.md。父display/Q06未完，Q07未迁移/目标ACTIVE；禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-STATE9996-DIRECT-SPAWN-TRANSACTION-001 IN_PROGRESS / UNITY_RED_CONFIRMED_IMPLEMENTING。2026-09-21 08:44 UTC job3056fb54实际12例3PASS/9FAIL，所有beforeDiff0，证据unity-red-3056fb54；差异为native出生500、display/frame历史/pending motion、implicit准入、容量后RNG、pending guard。已先声明准确6生产路径，再实施专用transient clone标记/Clear、两factory/shared direct birth及producer顺序；新增pool归还后普通OPoint隔离测试待编译。不是完整验收；following/双materializer/稳定包回归仍待。按受影响分支测试，未跑全角色；父display/Q06未完、Q07未迁移、总目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-STATE9996-DIRECT-SPAWN-TRANSACTION-001 IN_PROGRESS / SOURCE12_PASS_UNITY_RED_PENDING，准确source2+新fixture1路径声明，Unity生产未改。source-final/first.jsonl才是最终证据：12例/163239bytes双跑SHA090B9773…1D09F6，独立813/root复跑PASS；native500/500、implicit帧、partial/full仍34RNG已实测。following是helper后driver，未改counter所以再生成5个，不能误作两自然tick。UNITY-SEAM-AUDIT记录专用transientflag/Clear、两即时factory/共享direct birth候选；不得冒充weaponpiece或改普通OPoint。native1000 vsUnity400/1050容量案例只比相对空槽边界，不伪报全occupancy。新Unity fixture正在编写，未运行。前普通Stage/AIalias保持VERIFIED；父display/Q06未完，Q07未迁移/目标ACTIVE；禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-STATE9996-DIRECT-SPAWN-TRANSACTION-001 IN_PROGRESS / SOURCE_WITNESS_FIRST，准确2工具脚本预声明，Unity未改。当前source07CD47采用direct requestHP/MP500、implicit0..3可用、先随机tuple后spawn_transient容量拒绝且继续剩余attempt；Unity仍HP10/HasAuthoredFrame/提前容量break。旧B3克隆record的closure39DDDA15已追加current-authority回访说明，不能重用旧HP10/容量假设；成功5克隆拓扑和callsite顺序职责保留。下一实际source API代表见证+独立校验→准确Unity RED/整事务接线，不只改HP。普通/Stage显示已VERIFIED/证据复用；父display随后联验再post-display2379。Q06未完/Q07未迁移/目标ACTIVE；禁computer-use/Scene/资源/非战斗修改。

> 当前Q06：NTSD28-Q06-ORDINARY-STAGE-DISPLAY-BIRTH-001 已限定VERIFIED / ORDINARY_STAGE_DISPLAY_BIRTH，准确4脚本；复用identity匹配的display980源证据，初7个真实显示RED+1snapshot夹具假设错误保留，joint21/21PASS；snapshot先清local释放slot再restore八markers通过。一次SelfCheck08:15:15.455Z、Play普通/type3 Stage Renderer2于08:15:55.922Z、Q05close08:15:56.504Z PASS；scene checksum/borrowers2→2、restore4→4/finalworldslots两pool0/Stopped2帧，CS0/dirtyfalse/root14/hashBCD1047B…0E9FB6保持。下一明确依赖state9996 direct clone整出生资源：source battle_world2998 requestHP/MP500(EngineProfile)，Unity LateModule native分支792后硬写10且先用普通OPoint stats，先准确Task/Change+实际source见证，不能仅把display同步10或修改通用OPoint已验算法。随后父DISPLAY-PROGRESSION联验，再接POST-DISPLAY原2379事务。AIalias/普通出生/OPoint/融合已验职责复用。Q06未完/Q07正式DAT图片未迁移/总目标ACTIVE，无运行job/build/Play；禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-ORDINARY-STAGE-DISPLAY-BIRTH-001 IN_PROGRESS / FOCUSED_RED_FIRST，准确4脚本已声明，生产尚未改。DISPLAY-BIRTH-RETURN-AUDIT确认普通Initialize/Stage最终HP未同步出生显示；旧OPoint与weapon-piece出生已VERIFIED不可重做。另发现state9996 native direct clone应HP/MP500，当前Unity普通OPoint后硬写10，需独立整事务源见证，禁止只把display同步到10。优先普通/Stage显示子包→clone完整资源→父display联验→既有post-display2379事务；modeQ08/正式内容Q07仍待。前AIalias已VERIFIED/证据复用；Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/Scene/资源/非战斗修改。

> 当前Q06：NTSD28-Q06-NATIVE-AI-PERSISTED-ALIAS-001 已限定VERIFIED / NATIVE_AI_PERSISTED_ALIAS_CONSUMPTION，准确9脚本，详本包ACCEPTANCE。source18/408双跑SHA E3CCEF…C07D，初carrier RED4/decision RED8of15保留，joint27PASS；真实source16/17两tick+snapshot恢复3/3、正gatecaller1及staleguard1通过。一次fullSelfCheck07:50:14.786Z、Play pooledRenderer2于07:51:03.615Z、Q05关闭07:51:04.211Z PASS；scene checksum/borrowers2→2，restore4→4/finalWorldslots两pool0/Stopped2帧，dirtyfalse/root14/hashBCD1047B…0E9FB6保持。仅fixture初始化canonical/acceptedtrace/Match.Difficulty0纠正，源DAT/pending未变，无每tick补偿。默认/配置DataOrientedCanonical静态已确认；异常fallback/显式legacy未制造，不宣称全部AI路径。下一回Q06剩余live-path/late-display-postdisplay/platform清单，先审最接近消费者及依赖；已有alias/融合职责不要重做。Q06未完/Q07正式DAT图片未迁移/总目标ACTIVE，无运行job/build/Play；禁computer-use/非战斗/Scene/资源修改，按受影响分支验证。

> 当前Q06：NTSD28-Q06-NATIVE-AI-PERSISTED-ALIAS-001 IN_PROGRESS / ROWS_AND_KERNEL_FOCUSED_PASS，source18/408双跑一致，carrier初RED4，decision初RED8/15→joint27/27PASS。四row+kernel已写且独立review限定PASS；actualID1追击保留，unsupported只停AI decision不停止tick。准确9脚本已声明，新增真实two-tick/restore/stale fixture编写中，尚未运行；main正gate新测试待运行。详本包PROGRESS.md；整包未验收、Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/非战斗/Scene/资源修改，不重跑已验融合。

> 当前Q06：NTSD28-Q06-NATIVE-AI-PERSISTED-ALIAS-001 IN_PROGRESS / SOURCE_WITNESS_FIRST，准确初始两个diagnostic路径已声明，Unity脚本尚未修改。先实际source API证明classifier/match/actualID分离及special提前返回/RNG/采样，再Unity RED和准确生产接线。审计见NATIVE-AI-ALIAS-CONSUMER-AUDIT-001；融合证据复用，不跑全角色。Q06未完/Q07未迁移/目标ACTIVE；禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NATIVE-AI-ALIAS-CONSUMER-AUDIT-001 静态审计及独立复核完成，详 artifacts/diagnostics/NTSD28-Q06-NATIVE-AI-ALIAS-CONSUMER-AUDIT-001/AUDIT.md。已确认派生AI行缺alias，classifier/match语义不同，special 0x3c后非零alias停止ordinary但仍采样/推进tick；正式方法名step_profiled_combat。下一先准确source witness Task/Change与Unity RED，再修行采集/增长/比较及特定kernel分支，禁止全局替换ObjectId或重复已验融合职责。本轮仅文档，无脚本/Unity测试/Play；按受影响分支代表验证，复用无变化证据。Q06未完/Q07未迁移/总目标ACTIVE；禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：融合四包FUSION-RECORD-TRANSACTION / PERSISTENT-CARRIERS / SELFCHECK-ORACLE / CATALOG-TRANSACTION-WITNESS均按各自限定scope VERIFIED，详各ACCEPTANCE。原source4/1737保持；当前即时4/following2/replay4/AI2、requiredslot拒绝通过；oracle初2/7→joint17/18→仅mirror1/1纠正，非虚报18/18独立job。一次fullSelfCheck06:59:18.984Z PASS；真实Play6于07:00:39.067Z PASS(renderer4+Mobilelogic2，scene fast64/borrowers2→2)；Q05close07:00:39.650Z PASS，restore4→4/finalworldslots两pool0/Stopped2frames。Editor idle非Play，Scene dirtyfalse/root14/hashBCD1047B…0E9FB6保持。schema16/24/27/core12，raw47/3不提升。不可把本批解释为完整Host/AI：featurekey准入/跨场景保留、poststory第二投影/stage时序、NativeAI alias消费者、previousXYZ/opoint诊断仍待；source suspended槽本来禁止reuse，不重构allocator。下一回remaining live-path审计，优先精确追踪NativeAI persisted alias→Unity row/resolver再建Task，不凭字段存在称AI已齐；late/display/postdisplay/platform等backlog保持。Q06未完/Q07资源未迁移/总目标ACTIVE，无运行job/build/Play，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-FUSION-SELFCHECK-ORACLE-001 IN_PROGRESS / FOCUSED_RED_FIRST，准确SelfCheck七方法及新反射runner两路径已声明；修正旧partialRecovery/partnerReset/latch断言，不改已验融合生产。先七方法定向实测，再更新源依据；一次fullSelfCheck留稳定后。融合事务前即时4/following2/replay4/AI2保持，代表Play/关闭仍待，Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-FUSION-RECORD-TRANSACTION-001 IN_PROGRESS / IMMEDIATE_FOLLOWING_REPLAY_PASS_SELFCHECK_PLAY_PENDING，准确3脚本已改。source4 before均0，原RED差异已归零；最终job33a3f30bde7640f481836e941a08db2f即时4+following2全PASS，jobe0a516a9f1564cf4999c0b5504b45d62融合前/暂存snapshot replay4+AI成员2全PASS，CS0/独立review。完整record merge/split已写，partner不Reset、失败无部分提交、implicit310/属性/状态保持符合支持tuple。native suspended槽原本就禁止reuse（source1233/1297/1504），不要重构allocator；opointLatch/previousXYZ仍source-only诊断，raw3不提升。下一唯一收口依赖：准确新SelfCheck oracle Task，先7个CheckOid5152方法定向RED再按源纠正旧WaitCounter37/partnerReset/partialRecovery断言；之后本组一次fullSelfCheck与代表Play/有序关闭，未运行这些验收不能关闭整包。persistent carrier父仍IN_PROGRESS；featurekey/poststory/NativeAI回访保留。Q06未完/Q07未迁移/目标ACTIVE，无运行job/build/Play，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-FUSION-RECORD-TRANSACTION-001 IN_PROGRESS / UNITY_SOURCE4_RED_FIRST，准确单新fixture路径已声明，先真实World scan与source4对照，未改融合生产。persistent carrier26路径前阶段证据保持，整包仍待整体验证；global poststory/键准入/NativeAI消费者回访保留。下一根据真实RED完整替换record merge/split并精确声明生产路径，禁止只改getter/partner.Reset/混用formal身份与synthetic数据。Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/Scene/资源/非战斗修改。

> 当前Q06：NTSD28-Q06-FUSION-PERSISTENT-CARRIERS-001 IN_PROGRESS / FEATURE_CONFIG_CORE_PROJECTION_PASS_FULL_FUSION_PENDING，准确26路径。World显式双global配置、registry first→occupied entity/raw(排除dormant/空raw)、共用RunTick preBattleFlow(main/worker)及rawscenario同名bool已接。初RED7，最终job553b98f94db849928070495c94152d5e联合11/11，新10+正式capture1；midtick出生默认false到下一实际tick更新已测。未启动新的worker线程/未跑无关SelfCheckPlay。source poststory第二投影依赖stageHost旧时序待办，完整功能键准入/跨场景保留未接，不得把此stage标完整Host对齐。下一创建准确record-driven融合事务Task和Unity source4 RED；prepared/table/identity/5carrier/birth/input/显式global已足够，替换完整merge/split而非getter，保持C12/C25h/timer已验职责。carrier整包仍IN_PROGRESS等待整体验证，Q06未完/Q07未迁移/目标ACTIVE，无运行job/build/Play，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-FUSION-PERSISTENT-CARRIERS-001 IN_PROGRESS / BIRTH_INPUT_FOCUSED_PASS_GLOBAL_PROJECTION_PENDING，21准确声明路径。alias/drop四shell出生helper及独立ModuleBind开关、snapshot caller false、输入RouteNativeHitJa持久alias已接；常规DAT换绑不覆写。初RED4保留，joint18/18(job13e9bdc8a1264cc385db1123cfc43b6d)，再直接相关fresh-shell/removal replay2/2(job1a0df8ce8660436a8454dde9e7b2441a)，独立review通过。前storage14及schema16/24/27/core12证据保持，不重测无关项。下一同Task先准确声明global配置/first→entity投影：审共同Host/worker入口和source preclassification/poststory时点，rawscenario加同源bool，不改菜单/Host键序列准入、不用getter投影或GameMode猜值；随后完整fusion source4事务。Stage/roster本轮是ModuleBind共享调用链证明非新stagePlay。整包未关闭、Q06未完/Q07未迁移/目标ACTIVE，无运行job/build/Play，禁止computer-use/Scene/资源/非战斗修改。

> 当前Q06：NTSD28-Q06-FUSION-PERSISTENT-CARRIERS-001 IN_PROGRESS / STORAGE_SCHEMA_FOCUSED_PASS_PRODUCERS_PENDING。12声明路径已写，3entity+2World字段copy/reset/corecapture/restore/fast64checksum完成；当前entity16/aggregate24/checksum27/core12、shell2/2。初RED5、初joint21中12PASS9FAIL（fixture误用独立diagnosticJSONchecksum，原结果保留），纠正入口后job130432bfbcb54709bbec6bbf83fb5684新14/14PASS；genericentity/raw1+Q05trace6先前已过且生产未变。Parity84/84+19/19、新nativebuild与Unityfreshheader一致、CS0/review通过。不得关闭整包：下一先准确追加birth/helper/aliasreader/fixture路径，显式覆盖四shell+roster/stage并保留snapshot/普通变身；不可复用OPoint-only writer或偷偷扩展armorflag语义。global配置及投影时序也待落实，完整fusion未实现。诊断JSON与raw47/3未自动扩展。无运行job/build/Play，Scene dirtyfalse/root14/hashBCD1047B…0E9FB6保持，Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-FUSION-PERSISTENT-CARRIERS-001 IN_PROGRESS，准确12初始脚本声明；联合entity16/aggregate24/checksum27/core12存储合同，先字段/copy/reset/快照/hash，出生与消费路径必须追加准确声明后继续，尚未闭合。旧prepared9/9与identity33/33证据保持各自版本范围；raw47/3不提升。总目标ACTIVE/Q06未完/Q07未迁移，禁computer-use/Scene/资源/非战斗修改。

> 当前Q06：NTSD28-Q06-FUSION-CARRIER-PRODUCER-AUDIT-001 只读审计完成，新增证据已归档CARRIER-PRODUCER-MATRIX.md。正式global第一feature值明确投影到entitygate（init/切换/分类前与story出生后pre-tick），二者不同所有权但有关联，不能简单称完全独立；toggle可反转/LFR恢复，Host键准入未对齐。AIalias/drop仅birth/fusion写，FrameCache.Load非通用初始化点；四shell显式birth及stage/roster需覆盖，OPoint vitals helper不完整。当前尚未改carrier代码；下一建准确联合carrier/schemaTask（建议entity16/aggregate24/checksum27/core12一次升级，当前仍15/23/26/core11），先copy/reset/snapshot/hash负例再显式birth/input reader；不自动改AI/菜单或覆盖entitygate时序。prepared接线9/9保持VERIFIED；完整fusion/source4仍待，Q06未完/Q07未迁移/目标ACTIVE，无运行job/build/Play，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06：NTSD28-Q06-FUSION-PREPARED-WORLD-CATALOG-001 已限定VERIFIED / PREPARED_DATA_WIRING_ONLY，准确6脚本。新World table/identity、Host已发布双manager引用守卫beforeUnseal、两rawcapture/replay入口接线完成；legacy显式null且重备清旧引用；Logan staged resolver避免拒绝时部分修改。初RED4保留，最终job20024fe8ede64b78a70e919d4c6bf2d3联合9/9（新7+旧catalog1+正式capture1）、CS0、独立review通过，新actualheader保持V3。未激活融合consumer；null config容许partial，不能称全部融合资源ready/整个wrapper深冻结。未跑无关SelfCheck/Play。下一准确carrier/schema及出生/变身持久化合同：190独立、AIalias/drop不可当前wrapper派生、全局featurepair与entitygate分离、318复用RenderPicOffset，随后source4完整融合事务RED/修复。Q06未完/Q07未迁移/目标ACTIVE；Scene hashBCD1047B…0E9FB6保持，无运行job/build/Play；禁止computer-use/Scene/资源/非战斗修改。

> 当前Q06：NTSD28-Q06-FUSION-PREPARED-WORLD-CATALOG-001 IN_PROGRESS / PREPARED_DATA_ONLY，准确6路径已声明。把已冻结Logan table/identity一起传入既有World catalog和pre-seal入口；legacy显式null，不从环境猜fallback；不改融合事务/新carrier/schema。先新focused RED，再实施及相关capture验证，不跑无关全量。前COMPOSITE身份已VERIFIED。总目标ACTIVE/Q06未完/Q07未迁移，禁止computer-use/Scene/资源/非战斗修改。

> 当前Q06：NTSD28-Q06-FUSION-COMPOSITE-CONTENT-IDENTITY-001 已限定VERIFIED / COMPOSITE_CONTENT_IDENTITY_ONLY，准确12脚本。Unity33/33(job906f2e19a88c4e1d82bbe615868b17e6)、CS0、Parity81/81+raw19/19、新鲜native与Unity实际raw完整content header及独立Python向量一致。对象O保留，融合F/S入组合C和V3；旧V2/tag-only不能current，legacy保持；candidate同bytes高优先级失效并沿既有cache IOException路径重载。source诊断非正式EXE录像；native双root不同值仅静态核对，本次实跑canonical同root。neutral3tick比较仍47equal/3MISSING（platformSourceSlot/environmentState/environmentSourceSlot），不伪报全对齐。未重跑无关SelfCheck/Play；Scene dirtyfalse/root14/hashBCD1047B…0E9FB6保持。下一回父FUSION-CATALOG-TRANSACTION-WITNESS：先精确声明prepared World fusion catalog接线，再carrier/schema及完整融合事务；禁止只接getter假闭环。Q06未完/Q07未迁移/总目标ACTIVE；无运行job/build/Play，禁computer-use/Scene/资源/非战斗修改。

> 当前Q06：新NTSD28-Q06-FUSION-COMPOSITE-CONTENT-IDENTITY-001 PLANNED，准确11脚本预声明，尚未改代码。三端审计发现Unityrawcapture两处仍传object-onlyraw，Parity7合成header点也需同步；新合同保留O，组合C=SHA256(ASCII专用tag+NUL+O/F/S三32byte)，semanticV3及LE64，header显式3分量/新scope；旧V2历史保留但current拒绝。独立Python正式向量C3A7FF5…C37CC4/MFD18D6…008147/projection0FEFD4B968D618FD已存，不是当前runtimeheader。Unity单root明确canonical双root同值限制，nativecapture须用其真实独立两实参；不从ImageRoot猜根。下一按Task实施模型/candidate/Unitytrace/native/Parity和新focused tests，再三端新鲜交叉验证；不得仅改tag。前parser24/freeze14/source4-1737保持；Worldcatalog/carriers/完整融合仍未实现，Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/Scene/资源/非战斗修改。

> 当前Q06：NTSD28-Q06-FUSION-INPUT-FREEZE-001 已限定VERIFIED，准确2新脚本、Unity14/14(job5d8aa21ce3dd47b89fac788cfd9155f6)/CS0/reviewPASS。5原生路径优先级、firstexistinginvalid失败、missing-onlylocked2、byte冻结/高优先级出现失效及fusion专用双hash已证；未接Host/World/全局LoganContentIdentity，不声称端到端融合完成。Parser24/24及source4/1737保持。下一必须精确全局contentidentity/preparedcatalog接线合同：现LoganObjectCatalog只hash角色DAT，需将fusion身份显式纳入且同步source/tool校验后才能activate；明确extractedroot不同于统一runtime根，不能猜。随后独立carrier/schema(190/AIalias/drop/globalfeaturepair，318复用RenderPicOffset)及source4UnityRED完整事务。未改资源/Scene/非战斗，Q06未完/Q07未迁移/总目标ACTIVE；无运行job/build/agent。

> 当前Q06：NTSD28-Q06-FUSION-CATALOG-PARSER-001 已限定VERIFIED / IMMUTABLE_TEXT_PARSER_ONLY，准确3新脚本、Unity24/24(job b1873ce0593347a99062b15d6537fc99)/CS0/独立review通过，未接生产，不重复SelfCheck/Play。父NTSD28-Q06-FUSION-CATALOG-TRANSACTION-WITNESS-001仍IN_PROGRESS，source4/1737证据保持，Unity完整事务未实现。carrier审计确认318复用RenderPicOffset；190独立持久值不可复用Unk338；AIalias/drop在lockedkind换definition时保留，不能始终derive当前wrapper；两fusionfeaturegate是全局且与entityFeatureGate分离。下一建立准确preparedfusioncatalog/contentidentity和carrier/schema任务（详UNITY-CARRIER-ENTRY-AUDIT），声明路径后才改，随后source4UnityRED及完整事务。C12/C25h调度保持；Q06未完/Q07未迁移/总目标ACTIVE；无运行测试/build/agent，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06 NTSD28-Q06-FUSION-CATALOG-TRANSACTION-WITNESS-001 IN_PROGRESS / SOURCE4_PASS_UNITY_DATA_CONTRACT_PENDING。正式fusion.dat两record实际load，source4双跑SHAff7083f5…a4d0b4bf/58471bytes，独立可见状态1737checksPASS：record1/2 merge-defuse（record2真实implicit310），HP177拒绝、缺原partner定义分离失败公开state保持。timer0为明确测试干预，不称4500/200自然到期；private suspended无公开读取不称全字节证明。完整following已capture尚未Unity对照。下一先精确Unity carrier/catalog合同：核对display190/revive318/AI-drop/第二featuregate及snapshot/hash，不凭grep加字段；利用现有preparedRuntimeDataCatalog边界，另建精确Task后才实施。Unity生产/资源尚未改，不能只改getter或保留旧partner.Reset假装融合对齐；C12/C25h调度保持已验。前firstBDY两包限定VERIFIED，Q06未完/Q07未迁移/总目标ACTIVE；无运行build/test/agent，禁computer-use/Scene/资源/非战斗修改。

> 当前Q06 NTSD28-Q06-FUSION-CATALOG-TRANSACTION-WITNESS-001 IN_PROGRESS / SOURCE_WITNESS_FIRST。只读审计发现正式fusion.dat两record（7/8→51与10/11→52），Unity硬编码第一条且gate/历史/定义发布/分离Reset/失败前置不同，不能getter-only修复。准确Task/Change只声明新诊断CPP，source4基线计划两record merge/defuse+HP边界拒绝+缺partner定义拆分失败；生产/资源未改。保持已验C12/C25h调度；完整差异见AUTHORITY-GAP-AUDIT。前firstBDY两个包已限定VERIFIED。Q06未完/Q07未迁移/目标ACTIVE；禁computer-use/Scene/资源/非战斗修改。

> 当前BATCH-03/Q06：NTSD28-Q06-FIRST-BDY-NATIVE-FRAME-BINDING-001 与 NTSD28-Q06-FIRST-BDY-NATIVE-COUNTER-CARRIER-001 已限定VERIFIED。source8/222、主8smoke4 before/after/following零差异、joint19/19、SelfCheck04:53:09Z、sameWorld replay4场景8tick（含负action目标释放）、Play8与Q05close04:57:08Z均PASS。Scenechecksum/borrowers2→2、restore4→4/final World slots pools0/Stopped2frames，dirtyfalse/root14/hashBCD1047B…0E9FB6保持。旧B5仅encoded counter错载体精确纠正，原决策/RNG等保持。next唯一恢复入口remaining-live-readers审计：优先只读核对Oid5152FusionScanAll→BattleOid5152RuntimeModule→TryApplyRuntimeIdentity fixed290/112及retained split action/857 gate的完整source事务与已闭职责，再建新Task；尚未改identity生产或建新sourceTask。lateEffects/display/postdisplay/CPointselector及raw3/platform/previousXYZ待办保留。Q06未完/Q07正式DAT图片未迁移/目标ACTIVE；无运行job/build/agent/Play，Editor33236复用，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06 NTSD28-Q06-FIRST-BDY-NATIVE-FRAME-BINDING-001 IN_PROGRESS / SOURCE8_PASS_UNITY_PENDING。source8已构建/双跑一致SHA98ca9c2…10a97f8，focused222 PASS；初次私有入口编译失败保留后改公开ordinary入口。formal330全部DAT及catalog.csv哈希匹配：55simple+157encoded/目标全显式max705，encoded攻击者112/800/999skip；不声称正式玩家Bug。下一先准确声明Unity单fixture，以真实CharacterInteraction/shared consumer复建before并比即时/following，不能直接DamageWriter绕过firstBDY。生产两旧binder尚未改；已有B5规则/RNG/hold/manualdamage不重做。前lockedkind/snapshot已限定VERIFIED，Q06未完/Q07未迁移/目标ACTIVE；无运行build/test/agent，禁computer-use/Scene/资源/非战斗修改。

> 当前Q06 NTSD28-Q06-FIRST-BDY-NATIVE-FRAME-BINDING-001 IN_PROGRESS / SOURCE_WITNESS_FIRST，准确Task/Change已建，第一写域仅新source CPP；8分支代表、真实candidate→public hit→following，Unity shared consumer要求已写。尚未改生产/Unity fixture。前lockedkind与mutable snapshot两包已限定VERIFIED，不重做。Q06未完/Q07未迁移/目标ACTIVE；禁computer-use/Scene/资源/非战斗修改。

> 当前 BATCH-03/Q06：NTSD28-Q06-LOCKED-KIND-NATIVE-FRAME40-ADMISSION-001 与 NTSD28-Q06-SNAPSHOT-MUTABLE-DAT-IDENTITY-RESTORE-001 已限定VERIFIED。最终entity-only类型校验保留独立raw原值；joint42/42（b834ddc108714f14ae50ab78d3b76f48）、SelfCheck04:31:47Z、renderer replay2及Q05close04:32:46Z全部PASS，Scene checksum/hash/borrowers2→2、restore4→4/final World slots pools0/Stopped2frames保持。错误raw等同假设及中断失败完整保留，两ACCEPTANCE有准确范围。跨type仍拒绝/weapon派生覆盖与跨Worldepoch未提升。下一回NTSD28-Q06-NATIVE-FRAME-REMAINING-LIVE-READERS-AUDIT-001：FIRST-BDY-NEXT-ACCESS-AUDIT.md已只读定位BattleFirstBodyResponseWriter两个旧binder，先审formal catalog可达域并建精确Task/source约8代表，必须shared candidate入口而非直接DamageWriter；已有B5决策/RNG/hold/manualdamage/早退职责不重做。尚未创建新sourceTask/修改该生产。Q06未完/Q07正式DAT图片未迁移/总目标ACTIVE；现有Editor33236已恢复，禁computer-use/非战斗/Scene/资源修改。

> 当前 BATCH-03/Q06：NTSD28-Q06-SNAPSHOT-MUTABLE-DAT-IDENTITY-RESTORE-001 IN_PROGRESS / CORRECTION_WRITTEN_VALIDATION_PENDING。首次绑定修复13/13 PASS仍为旧版本证据；后新增raw==entity type守卫错误，联合job84364ef670d3421baa4ca9cc3df6c5af最后观测36完成/4FAIL，无最终XML。已按RuntimeSlotTable及Q05独立raw合同纠正为仅entity类型检查，raw保真控制替代错误拒绝断言；当前纠正尚未编译/运行，禁止引用旧13/13称最终PASS。证据joint-interrupted/OBSERVATION.md，独立review已明确撤回raw等同假设。旧Editor62860已退出/6403拒绝连接/Temp结果消失；当前三个Open Project窗口、unity status无实例，项目lock实际共享冲突被占用，禁止第二实例或删lock。下一恢复现有连接后跑focused8+父replay及相关raw/native测试，稳定后一次SelfCheck/代表rendererPlay关闭。父lockedkind仍IN_PROGRESS，Q06未完/Q07未迁移/总目标ACTIVE；禁computer-use/非战斗/Scene/资源修改。

> 当前 BATCH-03/Q06：NTSD28-Q06-SNAPSHOT-MUTABLE-DAT-IDENTITY-RESTORE-001 IN_PROGRESS。精确生产恢复顺序已写；新type0/type3成功例初RED2及晚slot拒绝PASS已存focused-red。修后相关snapshot类+原lockedkind变身前回放13/13 PASS（cfffb63e14c74ea0b4f5eb2d92715aa5），CS0，证据after-binding-fix/results.xml。原EntityIdentityMismatch已不再当前失败，但父NTSD28-Q06-LOCKED-KIND-NATIVE-FRAME40-ADMISSION-001仍IN_PROGRESS，依赖待反例硬化与retained-renderer验收。下一先审snapshot private raw/entity runtime类型一致性，声明精确额外路径再补无修改preflight校验与坏catalog/缺帧反例；禁止Load掩盖坏payload。独立review认可Load顺序，反对ModuleBind/初始化。尚未重跑SelfCheck/Play，待生产稳定一次联合；跨type仍拒绝/weapon子类待证据。Q06未完/Q07未迁移/总目标ACTIVE；保护Scene/HUDBg30和非战斗，禁computer-use。以下旧检查点仅历史。

> 当前 BATCH-03/Q06：NTSD28-Q06-LOCKED-KIND-NATIVE-FRAME40-ADMISSION-001 仍 IN_PROGRESS。source2/67、focused9/9、captured2/2、SelfCheck03:32:42Z、Play4及Q05关闭03:38:12Z已有PASS，证据归档 play-pass-replay-pending/ACCEPTANCE-PENDING.md；原sameWorld变身前snapshot回放FAIL EntityIdentityMismatch，不能称完成。新依赖 NTSD28-Q06-SNAPSHOT-MUTABLE-DAT-IDENTITY-RESTORE-001 PLANNED：expected200/current213被preflight拒绝，且当前帧恢复仍读现有DAT，禁止仅删除检查或清local shell绕过。下一先审FrameCache.Load/控制器副作用并做精确测试，再实现保留同shell/Renderer的原DAT恢复；生产尚未改。稳定身份/类型/失败无修改必须保持。按分支代表验证，复用未受影响证据。Q06未完/Q07未迁移/总目标ACTIVE；禁止computer-use、非战斗、Scene和资源修改。以下较早检查点只保留历史，以本条为准。

> 当前Q06 NTSD28-Q06-LOCKED-KIND-NATIVE-FRAME40-ADMISSION-001 IN_PROGRESS / SOURCE_WITNESS_FIRST。准确Task/Change已建，仅新CPP由cpoint_acceptance编写，尚未build/Unity/production修改。正式kind.dat加载优先decoded路径已确认；同表实际SHA39e30d…00011。indexed绑定8当前type0有40、213type3缺40且7条kind0攻击、209未在indexed行；仅静态不称动态Bug。计划真实catalog213→200 explicit40/implicit40两例，targetcurrent10state3000，完整identity/history/pendingCount/after/following，不能用只变ID或改targetstate0遮蔽snapshot差异。前generic binder已VERIFIED不重做，generic高位projection保持独立后继。Q06未完/Q07未迁移/总目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> 当前BATCH-03/Q06：TYPE3-TARGET-GENERIC-NATIVE-FRAME-BINDING-001已限定VERIFIED。source11双跑SHA c4d7afdc…d5ebee/独立419；Unity11+smoke5零差异、联合13/13、captured两mode2/2、SelfCheck03:11:12Z、representative sameWorld5场景10重放tick、Play10及Q05关闭03:14:12Z全PASS。Scenechecksum/borrowers2→2、restore4→4/worldslots两pool0/两帧Stopped、dirtyfalse/root14/hashBCD1047B…0E9FB6保持。只修单generic binder，真实pair中间900最终70/71及贡献count1→2正确；highpair投影旧gate未覆盖不称PASS。首次新方法pre-reload空test0已保留不计PASS。下一优先只读审计ApplyNativeLockedKindTransform HasFrame40/oldrawbinder及lockedprojection门，源无显式40要求；报告LOCKED-KIND-NEXT-ACCESS-AUDIT.md含真实catalog加载链、formal decoded kind.dat已观察与fallback同表/SHA39e30d…00011。尚未建下一Task/source或改production；需精确Task和显式/隐式40 wholehit见证，不重做已闭genericowner/attacker职责。generic高位projection后继独立，其他identity/late/display/platform/raw3保留。Q06未完/Q07正式DAT图片未迁移/总目标ACTIVE，无运行job/build/agent/Play；复用Editor62860，禁computer-use/非战斗/Scene/资源修改，按分支代表验证。

> 当前BATCH-03/Q06：TYPE3-ATTACKER-POSTHIT-NATIVE-FRAME-ACCESS-001已限定VERIFIED。source13双跑SHAfb3f06bf…bf0e31/独立214、Unity主13+smoke6零差异、旧11+新2联合13/13、Object captured两mode source字段通过、SelfCheck02:49:00Z、代表sameWorld6场景12重放tick、真实Play12与Q05关闭02:51:18Z全PASS。Scenechecksum/borrowers2→2、restore4→4/worldslots两pool0/两帧Stopped、dirtyfalse/root14/hashBCD1047B…0E9FB6保持。captured初用错Character pass已纠正；type3target3005非匹配pair被既有CanProjectStandardType3DamageWriterEffect明确排除，只证明actual，不称writerprojectionPASS。原失败与准确scope见ACCEPTANCE。下一回remaining-live-readers，先只读审计BattleDamageWriter.ApplyNativeType3TargetGenericContinuation的DirectWriteHeldFramePreserveWaitCounter单caller及target response frame/owned关系完整source事务；与locked kind transform/identity及旧投影HasFrame门区分，避免重做已验owner/清pending职责。尚未建下一sourceTask/改该生产caller。Q06未完/Q07正式DAT图片未迁移/总目标ACTIVE，无运行job/build/agent/Play；复用Editor62860，禁computer-use/非战斗/Scene/资源修改，按分支代表验证。

> 当前BATCH-03/Q06：KIND0-POST-EFFECT-NATIVE-FRAME-ACCESS-001及STANDARD-REACTION-HISTORY-NATIVE-READERS-001均限定VERIFIED。source6+reaction2双跑/默认6bytes不变；联合25/25、SelfCheck02:24:27Z、代表sameWorld4场景8重放tick、真实Play10与Q05关闭02:27:23Z PASS，Scene checksum/borrowers2→2、restore4→4/worldslots两pool0/两帧Stopped、dirtyfalse/root14/hashBCD1047B…0E9FB6保持。reaction最终仅实际+投影两个previous reader；snapshot原fallback控制已过不改。父case1为early拒绝，不能称post-effect state18抑制覆盖。准确限制见两ACCEPTANCE。下一回remaining-live-readers审计，优先Type3 attacker post-hit共享selected reader/binder；POST-EFFECT-REMAINING-ACCESS-AUDIT已有定位，先确认standard type0/armored type0/特殊对象当前live消费者及原B5证据，已native unarmored/reduced helper不要重做。尚未建新sourceTask/修改type3生产。Q06未完/Q07正式DAT图片未迁移/总目标ACTIVE；无运行job/build/agent/Play，复用Editor62860，禁computer-use/非战斗/Scene/资源修改，按分支代表验证。

> 当前BATCH-03/Q06：KIND0-POST-EFFECT-NATIVE-FRAME-ACCESS-001 IN_PROGRESS，两访问点已修但依赖未关。source6双跑SHAc01031a3…c2bcae4/独立124PASS；其中5真实hit+1early拒绝（effect20 prev18前置拒绝，不能声称后置18抑制覆盖）。Unity原RED主6即时10/后继12；两访问修后job440cbbd2bef64c5e9f7f49ce15b76b7e旧15PASS/新两矩阵FAIL，仅case0普通reaction剩fall20vs80/action220vs186/pendingX0vs5/Y17vs10，before均0；其余5例零差异。下一唯一Task NTSD28-Q06-STANDARD-REACTION-HISTORY-NATIVE-READERS-001 PLANNED：source CPP增加--reaction的snapshot900state12/state0两例（尚未写），默认6字节保持，之后准确声明普通ApplyStandardFall与投影4reader。父证据production-red/after-post-effect-access保存；完整SelfCheck/Play待依赖修好联合一次。当前无运行job/build/agent/Play；Q06未完/Q07未迁移/总目标ACTIVE，前3包VERIFIED保持不重做。禁computer-use/非战斗/Scene/资源修改，保留HUDBg30。

> 当前BATCH-03/Q06：EFFECT-OVERRIDE-NATIVE-FRAME-ACCESS、STANDARD-HIT-FALL80-PRESERVATION、STANDARD-HIT-PENDING-Y-PROJECTION三个001包已限定VERIFIED。联合30/30、完整SelfCheck01:55:20Z、代表sameWorld6场景12重放tick、真实Play22与Q05关闭02:00:04Z均PASS；Scene checksum/borrowers2→2、restore4→4/worldslots两pool0/两帧Stopped，dirtyfalse/root14/hashBCD1047B…0E9FB6保持。初次Play测试日志context失败已保留并只修测试入口，未再跑全套。准确scope/限制见各ACCEPTANCE。下一唯一入口回remaining-live-readers审计，先Kind0 post-effect旧previous reader+200/203 binder（报告POST-EFFECT-REMAINING-ACCESS-AUDIT.md已有6代表建议），需新Task/Change及source witness后才改生产；随后type3共享selected reader/binder，但已native的unarmored/reduced helper不重做。Q06未完/Q07正式DAT图片未迁移/总目标ACTIVE。无运行job/build/agent/Play；复用Editor62860，禁computer-use/非战斗/Scene/资源修改，按分支代表验证。

> 当前BATCH-03/Q06：effect native frame access、standard hit Fall80保持、pendingY projection三关联包生产修复已联合30/30 PASS（job d41e706b23d048448436ee57d9f2f0c9，1.777秒），完整SelfCheck 2026-09-21T01:55:20Z PASS；证据各包joint-pass。source3/default dvy0与vertical2非零/截断及effect16+smoke6已验证，旧失败保留。新增仅测试的6代表sameWorld replay与一次Play22合并入口已写，Editor正在刷新编译，尚未运行新增replay/Play；下一先读Editor readiness/CS0，再只跑新增replay与改动的captured tests，之后EffectPlay请求和Q05关闭，不重复完整SelfCheck/旧矩阵。三个Task仍IN_PROGRESS，Q06未完/Q07未迁移/总目标ACTIVE；Scene/HUDBg30和非战斗保持，禁computer-use。

> EFFECT-OVERRIDE-NATIVE-FRAME-ACCESS-001 source16已build/double-run一致SHA52093e69d3e286ddd5e496ab862efb7e35ec385b63b41472fa5b0dda51696f0d，focused独立365检查PASS；仅gate/override/descriptor/历史字段保持及固定injury5 HP，不模拟全hit sideeffects/RNG/following。初版target显式wait误填37（源41）两失败已保留并纠正。真实普通unarmored source正常HP395/致死-4，高位latch900state602/BDY50抑制及previous900state12匹配已触发。下一准确扩Record单Unity fixture，slot0/70完整before及whole标准hit后/下tick对照，先RED再决定normal两reader/两binder。实际标准hurt支持帧180/186/220已在同源DAT显式声明以隔离别的binder。HitPlan身份投影两reader另需真实转换代表，普通16不关闭它；Kind0PostEffect/Type3PostHit仍独立后继。生产未改；Q06未完/Q07未迁移/总目标ACTIVE，无运行build/job/agent/Play，Editor62860复用，禁computer-use/Scene/资源/非战斗修改，按分支代表验证。

> EFFECT-OVERRIDE-NATIVE-FRAME-ACCESS-001 IN_PROGRESS / SOURCE_WITNESS_FIRST：正式effect8字段801条，catchingact/pickedact全0。审计纠正旧getter会拒绝显式857..999；normal latch/previous两reader及两binder需见证，identity projectedDAT两reader另需实际转换证据。source16新CPP由cpoint_acceptance编写中，未build/run/Unity/生产修改。原B5职责保持，Kind0PostEffect/Type3PostHit不混改，测试按代表等价类。前IMPACT已VERIFIED不重跑；Q06未完/Q07未迁移/总目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> Q06 IMPACT-NATIVE-FRAME-BINDING-001已限定VERIFIED：source20/18285、20+6零差异、联合53/53、SelfCheck01:06:28Z、代表replay12场景24tick、Play12与Q05关闭01:09:41Z全PASS；Scene checksum/borrowers2→2、restore4→4/worldslots两pool0/两帧Stopped、dirtyfalse/root14。单Action caller修复，原B6职责保持。下一回remaining-live-reader审计，优先只读检查BattleDamageWriter.ApplyNativeEffectActionOverride两caller及其正式kind0/weapon/type3消费上下文；旁边Kind0PostEffectAction是独立计数器/朝向事务，须分清顺序再定最小source代表，不批改getter。其它identity/CPoint/lateEffects/display/post-display/platform/raw3保持。Q06未完/Q07未迁移/总目标ACTIVE；无运行job/build/agent/Play，复用Editor62860，禁computer-use/非战斗/Scene/资源修改，按用户分支等价类验证不重复全套。

> IMPACT单caller修后fullSelfCheck 2026-09-21T01:06:28.907250+00:00 PASS已归档after-binding-fix；此前运行中状态由本条覆盖，当前无运行job/build/agent/Play。联合53/53与20+6零差异保持，独立生产review单行通过。下一仅6代表sameWorld replay/Play及Q05关闭，不重复已过矩阵或SelfCheck。Task IN_PROGRESS/Q06未完/Q07未迁移/总目标ACTIVE。

> IMPACT-NATIVE-FRAME-BINDING-001生产单caller已修：source20/独立18285，Unity有效RED主20 before0/即时15/后继5、smoke6 before0/即时3/后继1；Action操作改native binder后20+6均before/即时/following0。联合job5b92ec860e18446cb36b010bef8608cb 53/53PASS(新2+literal3+相关HitPlan48)，约1.42秒，未跑旧B6全量；证据after-binding-fix，原JSON数字tag夹具失败及有效RED均保留。fullSelfCheck请求已提交，现Editor62860执行，结果待新Temp/NTSD_BattleRuntimeSelfCheck.result；不得重复启动/并行Unitytests或C#编辑。下一仅6代表sameWorld replay与Play(Authority/DataOriented/renderer6+Mobile/Legacy/logic6)及Q05关闭，脚本前准确登记Record；无需再跑矩阵/旧测试除非新失败。Task未关闭，Q06未完/Q07未迁移，总目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> IMPACT-NATIVE-FRAME-BINDING-001 source20已build并双跑一致SHA4041ba46914f43292e97058fb9c8687f67482cf384dbcb3590e1d0a0946d6edd，独立18285检查PASS（captured-before→四实体即时after、准入/descriptor/pending/RNG，不独立重建spawn或following）。Build manifest闭包07CD47…778F，源CPP FFBAFC…60D8A。下一准确扩Record新增单Unity fixture，复用正式catalog type与四实体factory，全部spawn后恢复0→1→2 ownerchain及target70 before；额外capture environment/catchSource/impactSource/pendingXYZ/descriptor，sourcepreviousXYZ仍明确source-only；主20代表+必要smoke，不扩乘积。先before0/即时/following分开RED再决定唯一BattleDamageWriter Action caller，生产未改。静态formal respond全0且唯一implicit182 OID899无bdy/itr，不认定实战Bug。前state1218已验不重做；Q06未完/Q07未迁移/总目标ACTIVE，无运行build/job/agent/Play，Editor62860复用；禁computer-use/非战斗/Scene/资源修改。

> Q06 IMPACT-NATIVE-FRAME-BINDING-001 IN_PROGRESS / SOURCE_WITNESS_FIRST。独立审计确认唯一旧binder位于BattleDamageWriter.TryApplyNativeImpact的Action操作；HitPlan.ProjectNativeImpactWriterEffect不读descriptor，旁边kind15不是本包。原B6 I1/I2/I3已验行为保持。formal330 kind10=121/kind11=61/respond全0；type0 target182显式157/隐式1(OID899 spe)，但该对象22frame无bdy/itr，不能认定实战可达差异。20代表source runner由cpoint_acceptance编写，只有新Tools CPP写域，尚未build/run/Unitytest/生产修改。报告remaining审计FORMAL330-IMPACT两JSON及新包UNITY-MAPPING-AUDIT.md。测试按用户等价类原则，不做profile/角色全乘积。前state1218两包VERIFIED保持，Q06未完/Q07未迁移/总目标ACTIVE；禁computer-use/非战斗/Scene/资源修改。

> Q06 STATE1218-CONTACT/AIRBORNE-NATIVE-FRAME-BINDING两包已限定VERIFIED。source480/55双跑与独立模型、联合29/29、SelfCheck00:45:27Z、代表sameWorld40场景80tick、真实Play40与Q05关闭00:48:58Z全PASS。Scene checksum/borrowers2→2、restore4→4/worldslots两pool0/两帧Stopped；dirtyfalse/root14/hashBCD1047B…0E9FB6保持。按用户要求不重复四配置矩阵及SelfCheck；代表限制见ACCEPTANCE。下一唯一入口回NTSD28-Q06-NATIVE-FRAME-REMAINING-LIVE-READERS-AUDIT-001，先只读核对BattleDamageWriter.TryApplyNativeImpact的Action操作与正式resolve_special_relation_hit对应完整事务及已有B6证据，再定最小source witness；禁止重做已闭行为或共享getter批改。其它impact/effect/identity/CPoint/lateEffects/display/post-display及platform/raw3/previousXYZ保留。Q06未完/Q07未迁移/总目标ACTIVE；无运行job/build/agent/Play，Editor62860可复用，禁computer-use/非战斗/Scene/资源修改。

> state12/18 contact+airborne联合29/29及最新完整SelfCheck 2026-09-21T00:45:27.002427+00:00 PASS，证据两包joint-pass。当前无运行job/build/agent/Play；下一仅代表sameWorld replay/Play与关闭，不重跑全矩阵或无新生产修改的SelfCheck。两个Task仍IN_PROGRESS；Q06未完/Q07未迁移/总目标ACTIVE，测试等价类收敛规则保持。

> 按用户分支等价类验证：airborne源55双跑一致SHAa4d7c8fd…52e691/独立24696检查PASS；Unity RED55即时30与smoke8即时7，before/following0，后仅改LF2Entity.ApplyCurrentDatType0AirborneAction单binder。联合job e6d47ce73ce2473180ce73ea5b4eca50已29/29PASS（airborne17旧+contact9旧+新55/8两测试+contact主路径480一个测试），实际8.03秒。新55/8与contact480 before/即时/following均0，证据两包joint-pass。不要重复另外三contact矩阵；已过分支复用证据。完整SelfCheck请求已消费，PID62860当前运行，等新结果勿并行C#改动/Unitytests。下一用代表案例补sameWorld replay与一次Play：contact选覆盖soft/pending/invalid/explicit999/hardmotion的约12例，两条配置路径各验证必要代表；airborne8代表；不要跑旧3840全乘积Probe。代表filter/计数变更前扩Record。两包仍IN_PROGRESS，Q06未完/Q07未迁移，总目标ACTIVE，禁computer-use/Scene/资源/非战斗修改。

> contact两binder已修，job45daa31087ba4319ac0fa19b0f9e36fb终态13项：旧contact9PASS/四矩阵仍FAIL；四组各480 before0/即时18(全airborne)/following0。完整产物after-contact-binding。contact生产仅两caller，父任务等待独立airborne出口后一次联合验收，当前不重复旧矩阵。 airborne源runner由cpoint_acceptance运行中；生产airborne未改。用户要求收敛重复验证（2026-09-21）：按相关分支/数据等价类选代表案例，不能只按角色名扩矩阵；局部修改先编译+原差异最小案例+受影响边界。未变化已通过证据复用，只有新修改/失败/未决风险才扩测。完整SelfCheck与相关联合回归在一个闭合执行包出口集中一次；Play按实际受影响路径/生命周期选代表，不每改一行都重跑所有配置/角色。Q07内容迁移与Q12完整集成出口不缩减，原失败不得删除/排除来变绿。

> STATE1218-CONTACT-NATIVE-FRAME-BINDING-001已测RED：job87f3c88664724d3b80bf2501db16de3b终态FAILED4/4，两profile×两路径各480 before0/immediate410/following120；即时392 contact+18 airborne-control，后继120全部contact。production-red四JSON/XML已存；生产未改。下一先精确扩Record仅LF2Entity.ApplyCurrentDatType0State1218ContactAction两个raw binder，修后保持完整480控制不删；airborne18另建独立Task/source见证，不在contact包顺手改第三caller。源480/228481模型PASS，但stepflags/following未独立模型（review泛称flags由模型承担不适用当前脚本，按实际scope）。静态正式330默认目标全声明及19461 ITR无自定义pending动作保持，不把synthetic RED称正式玩家Bug；动态身份域仍待。前ordinary/candidate已VERIFIED不重做，Q06未完/Q07未迁移/总目标ACTIVE。无运行job/build/agent/Play，复用Editor62860；禁computer-use/非战斗/Scene/资源修改。

> 当前Q06 STATE1218-CONTACT-NATIVE-FRAME-BINDING-001 IN_PROGRESS。源480 build/double-run一致SHAd898b84d…ffc6c1，独立初态/即时228481检查PASS；原模型implicit999失败8处已保留纠正。正式330默认contact目标hard2512/soft1853全部显式，19461 ITR无非0pickedact/pickingact，仅静态域不证明动态身份切换；不称已确认玩家可见Bug。新Unity单fixture pending7字段已恢复，CS0，job87f3c88664724d3b80bf2501db16de3b运行四直接矩阵，必须读终态；生产尚未修改，airborne另属后继。源与fixture准确Record已建。前ordinary/candidate两个包VERIFIED不重做；Q06未完/Q07未迁移/总目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> 普通落地NATIVE-FRAME-BINDING及CANDIDATE-COLLISION-REFERENCE-RESET均已限定VERIFIED。source186四矩阵0差异、联合15+旧6、sameWorld744场景1488tick、完整SelfCheck、真实Play1488、退出重进Q05关闭00:17:02Z/00:18:05Z全部PASS；Scene checksum/borrowers2→2、restore4→4/worldslots两pool0/两帧Stopped，dirtyfalse/root14/hashBCD1047B…0E9FB6保持。下一回NTSD28-Q06-NATIVE-FRAME-REMAINING-LIVE-READERS-AUDIT-001：优先只读确认state12/18 contact/airborne剩余三个raw caller及完整authority事务，必要时新建源见证Task，禁止重做已闭物理规则或批量改getter。platform op30/previousXYZ/raw3明确保留，identity/impact/lateEffects/display/post-display继续待。Q06未完/Q07未迁移/总目标ACTIVE；无运行job/build/agent/Play，Editor62860可复用；禁computer-use/非战斗/Scene/资源修改。

> 最新验收：candidate reference reset与普通落地native binder联合15/15PASS，source186×四组before/即时/following均0；旧B4普通落地已以正确Editor namespace补跑6/6PASS（job4191c162de2543eb83166574b9983c84）。完整SelfCheck 2026-09-21T00:11:47.663892Z PASS文件与时间证据已存candidate包。CS0、Ledger597/11PASS、diffcheck无错误，Scene hashBCD1047B…0E9FB6保持。下一唯一工作：同一普通落地186夹具补同World回放，再真实Play两profile/两路径/两factory及Q05关闭重入；脚本扩展前登记Record准确符号。两个包仍IN_PROGRESS（尚未完成replay/Play），不是平台op30域对齐。当前无运行test/SelfCheck/build/agent/Play；复用Editor62860，禁computer-use/非战斗/Scene/资源修改，Q06未完/Q07未迁移/总目标ACTIVE。

> NTSD28-Q06-CANDIDATE-COLLISION-REFERENCE-RESET-001 IN_PROGRESS：Q06候选高度参考reset独立包已生产写入；联合job0bbead38ad244ed1adc8ac109ebdabd3 SUCCEEDED15/15（新5/载体6/源矩阵4），普通落地186×两profile×两路径before/即时/following全部0。实际未运行旧B4 fixture（初始filter漏Editor namespace），后继须NTSD.Test.Editor.NTSD28B4Type0OrdinaryLandingEditorTests补跑。完整SelfCheck请求已消费，现有Editor PID62860执行中，实际日志Logs/kind8-real-play-editor.log已有RunAllChecksStatic/CheckActivatedRuntimeProfileContracts调用；结果待新Temp/NTSD_BattleRuntimeSelfCheck.result，禁止重复启动或测试并行。之后同World回放/真实Play及关闭仍待。平台op30/previousXYZ/其他两个reference字段仍未闭合，不称整域完成。Q06 active/Q07未迁移/禁computer-use/非战斗/Scene/资源修改。

> 当前BATCH-03/Q06：普通落地单caller native绑定已写。job01658e8a1a8241caab89c3c3fb25234e终态FAILED4/4，四组各186 before0/immediate0/following78，仅combat.collisionYReference=-10 expected0；证据after-binding-fix。源battle_world.cpp4026在pair pass前清零collision_y_reference/platform_source_slot_f4/render_shadow_offset_10c；Unity仅carrier reset已有，需审完整生产pass及平台依赖，不能在fixture清零掩盖。下一先只读确认并独立Task/Change声明必要生产修复，再完整回归/回放/SelfCheck/Play。CS查询0、Ledger596/9PASS、diffcheck无错误、Scene SHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6保持。落地Task仍IN_PROGRESS，Q06未完/Q07未迁移/总目标ACTIVE；禁computer-use与非战斗/Scene/资源修改。

> 当前BATCH-03/Q06：KIND8-NATIVE-RAW-BINDING-001已限定VERIFIED。source134/62045、两profile0差异、53回归、268场景536replay、SelfCheck成功日志、真实Play536与关闭23:33:57Z全PASS；Scene dirtyfalse/root14/hashBCD1047B…0E9FB6保持。下一唯一Task NTSD28-Q06-ORDINARY-LANDING-NATIVE-FRAME-BINDING-001 PLANNED / SOURCE_WITNESS_FIRST，只有新CPP预声明，尚未写脚本。ordinary landing规则及canonical tail已闭不重做，只核对raw descriptor binding；formal330静态7隐式hit_g不是动态可达证明。目标Editor62860当前idle/notPlaying，MCP6403/status-b1b02287可复用，禁止第二个同项目实例。无运行test/build/agent。CPoint370保留source-only、selectors正式全零排后，identity/impact/lateEffects/display/post-display仍待；Q06未完/Q07未迁移/总目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06 KIND8-NATIVE-RAW-BINDING-001 IN_PROGRESS / FOCUSED_REPLAY_SELF_CHECK_PASS_PLAY_PENDING。kind8单caller原生binder修复后两profile134 before/立即/following0；联合53/53PASS含268场景536重放tick。旧projection反射2FAIL已独立KIND8-PROJECTION-TEST-ENTRY-001限定VERIFIED，原失败保留。完整SelfCheck本轮batch PID122096已终止，日志成功marker与SHA已存full-selfcheck-completion-proof.json；退出后Temp结果缺失，不声称文件归档。下一在同新Editor fixture添加Play536入口，真实场景两profile两factory保护及Q05关闭；目前入口未写，勿重复旧source/回归。场景hashBCD1047B…0E9FB6保持，无目标Unity/job/build运行。CPoint370仅source capture/局部oracle且正式selectors全零，排后未取消目标；Q06未完/Q07未迁移/总目标ACTIVE，禁computer-use/非战斗/资源/Scene改动。

> 2026-09-21当前Q06：KIND8-NATIVE-RAW-BINDING-001 IN_PROGRESS。源134双跑一致SHAc0543119…b26009，62045独立即时transition检查0失败，132applied/2拒绝；新单Editor fixture已写、生产未改。目标项目原未打开，root启动Unity2022.3.62f3独立EditMode PID51800（Temp/NTSD28_Kind8Batch.pid），测试filter NTSD.Test.NTSD28Q06Kind8NativeRawBindingEditorTests，结果Logs/kind8-134-editmode.xml/日志kind8-134-editor.log；当前等待该进程终态，不重复启动同项目实例。CPoint选择370源双跑/2590部分oracle已存，formal330八selector均0，因此source-only保留，排在kind8正式内容30隐式目标之后，非已完成。held已验不重做；Q06未完/Q07未迁移/总目标ACTIVE，禁止computer-use与非战斗/Scene/资源修改。

> 当前BATCH-03/Q06：HELD-NATIVE-FRAME-BINDING-001已VERIFIED（初次140/释放1150/补给242及formal330静态内容声明范围），最终SelfCheck/Play968/关闭21:09:16Z证据保持。SyncHeldPose仅定义；同2033边selectedchild10/12/18均0，动态identity保留关系必须后继回访。当前source无child12/18unsupported，旧游标表述已纠正。下一唯一执行Task NTSD28-Q06-CPOINT-INPUT-ACTION-SELECTION-001 READY_SOURCE_WITNESS，Record PLANNED只声明新CPP，尚未改脚本：完整8路输入选择/最终0取消/原生selected frame与vaction，保护既有throw和kind2。raw-binding矩阵已交付，canonical kind8 dvx999哨兵等排后；identity/rawsetter/lateEffects/display/post-display继续保留。Q06未完/Q07未迁移/总目标ACTIVE；无运行job/build/agent/Play，禁computer-use/非战斗/Scene/资源修改。

> 当前BATCH-03/Q06：HELD-NATIVE-FRAME-BINDING-001 IN_PROGRESS / INITIAL_RELEASE_REFILL_SCOPES_VERIFIED_REMAINING_CALLERS_PENDING。refill242两profile正常0差异、generic202两profile0、484场景968重放tick、旧100回归与联合11/11、完整SelfCheck21:07:43Z、真实Play968全部PASS；关闭21:09:16Z restore4→4/worldslots两pool0/两帧Stopped。Scene dirtyfalse/root14/hashBCD1047B…0E9FB6保持。两个refill oracle已限定VERIFIED，旧失败保留。下一回remaining-live-reader审计：SyncHeldPose全仓仅定义无caller，不修改；held damaged12/10与源unsupported12/18及formal330域需最小只读核对后决定是否本Task仍有真实残余。不要重做已验140/1150/242，不批量改共享getter。identity/rawsetter/lateEffects/display/post-display后继保持；Q06未完/Q07未迁移/总目标ACTIVE。无运行job/build/agent/Play；禁computer-use、非战斗/Scene/资源修改。

> Q06 refill生产修复已两profile242 before/立即/following全0；旧100回归经独立RNG oracle修订后100PASS。SelfCheck原21:00:51Z伪OID992失败保留，独立REFILL-REAL-OID-SELF-CHECK-ORACLE-001只改该const为122，尚待新fullSelfCheck。当前job a2a98665947e4e2babea21e39e0aa359：generic202两profile、refill242 replay两profile、补给Editor最终标记断言回归，必须查询终态。refill Play run-refill968入口已写未运行；原release/initial已验不重做。父IN_PROGRESS，两个oracle未关闭，Q06未完/Q07未迁移/总目标ACTIVE；禁止computer-use、非战斗/Scene/资源修改。

> 当前Q06 refill已获得确定RED：source242双跑一致SHA83af13d…42d5dc0、229293独立检查PASS，原140/1150字节保持；Unity job fd1f7330323c4a5e9e1f3893c13c372d两profile242均before0/immediate660/following992，终态FAILED，证据refill-production-red。差异含耗尽旧legacy RNG、隐式0 wait、type0通用实体未补给；fixture已模拟正式type_sub默认，初态完全一致。下一先冻结最小共享补给事务生产符号/Record，再修已测分支，不动框架/Scene/资源。cpoint_acceptance只读审最小改动设计；无Unity job/build/Play运行。父HELD-NATIVE-FRAME-BINDING IN_PROGRESS，初次140及release1150限定验收保持不重做；Q06未完/Q07未迁移/总目标ACTIVE，禁computer-use。

> 当前BATCH-03/Q06：HELD-NATIVE-FRAME-BINDING-001 IN_PROGRESS / INITIAL_AND_RELEASE_SCOPES_VERIFIED_REFILL_PENDING。release源1150/独立296552 PASS，正常及generic664两profile零差异；旧回归91/query5、同World replay2300场景4600tick、完整SelfCheck20:39:17Z、真实Play4600全部PASS；关闭20:45:41Z restore4→4、world/slots/logic/render0、两帧Stopped，Scene dirtyfalse/root14/hashBCD1047B…0E9FB6保持。独立HELD-RELEASE-FIELD-SELF-CHECK-ORACLE-001已VERIFIED（测试字段范围）。下一同held Task扩refill源向量，精确核对耗尽action0/nativeRNG/Zz及源system_rules对象id门；不要重做初次140或release1150。remaining reader/identity/rawsetter/lateEffects/display/post-display仍待。Q06未完/Q07未迁移/总目标ACTIVE，无运行job/build/agent/Play；禁止computer-use、非战斗/Scene/资源修改。

> 当前Q06：HELD-RELEASE-FIELD-SELF-CHECK-ORACLE-001 VERIFIED / TEST_ONLY；独立字段断言已修，完整SelfCheck2026-09-14T20:39:17Z PASS，原失败保留。父HELD-NATIVE-FRAME-BINDING仍IN_PROGRESS；已扩单Editor fixture的release1150两profile同World回放及run-release Play4600入口，等待编译和实际运行。普通1150/generic664零差异保持，下一先release replay再Play/关闭。Q06未完/Q07未迁移/总目标ACTIVE；无agent运行，禁止computer-use及非战斗/Scene/资源修改。

> Q06最新验证：release1150正常路径及generic武器DAT664子集两profile均0差异，query5/5与旧回归91/91 PASS；但新完整SelfCheck 2026-09-14 20:35:34 UTC FAIL R5-HOLD-002（旧断言要求释放写Spawner，源写独立+2F8）。失败已归档，下一先独立审计该oracle并建立准确Task/Change后修改测试，再SelfCheck及release replay/Play。父HELD-NATIVE-FRAME-BINDING仍IN_PROGRESS，Q07未迁移，总目标ACTIVE。无运行Unity test/build/Play；只读cpoint_acceptance正在审计oracle。禁止computer-use、Scene/资源/非战斗修改。

> 当前 BATCH-03/Q06：HELD-NATIVE-FRAME-BINDING-001 IN_PROGRESS。初次绑定140限定验收保持；release源1150双跑及296552独立检查PASS，正常factory两profile1150 before/立即/following均0差异（job b573b82a834a415c9925f12441a11b0f，release-after-fix）。通用LF2OtherObject承载type1/2/4/6武器DAT的664行两profile补验已SUCCEEDED 2/2、before/立即/following全0（job f60db972d4724aacb78ecd0a5c51aa1d，release-generic-pass）；限定受控CLR适配子集。释放阶段最新生产修复后的replay/SelfCheck/Play仍待，不能复用初次140旧验收代替。refill及remaining live readers、display/post-display继续待处理。Q06未关闭，Q07正式资源未迁移，总目标ACTIVE；保留用户HUDBg30，禁止computer-use及非战斗/Scene/资源修改。以下旧检查点保留历史，以本条覆盖。

> 当前BATCH-03/Q06：HELD-NATIVE-FRAME-BINDING-001 IN_PROGRESS / INITIAL_BINDING_VERIFIED_RELEASE_READERS_PENDING。源140两profile立即/fulltick0、query5/5、旧回归91/91、280replay/560tick、SelfCheck20:01:02Z、真实Play560及关闭20:02:44Z全PASS；Scene checksum/borrowers2→2、restore4→4/worldslots两pool0/两帧Stopped，hashBCD1047B…0E9FB6保持。非holder-query、旧fixture、slotreuse-oracle三个独立子包已VERIFIED声明范围。下一同held Task先保留source140，再扩kind1/3+DVX等后继原生源见证；type2非kind3-DVX旧legacy RNG与后续frame40/随机/补给0旧setter待对照，不重做初次绑定或批量改共享接口。formal330 cover2/state12/18静态域仍0，限定dormant报告已存。remaining reader总审计/identity/rawsetter/lateEffects等继续保留；Q06未完/Q07未迁移/总目标ACTIVE。无运行job/build/agent/Play；禁止computer-use/非战斗/Scene/资源修改。

> 新独立 NTSD28-Q06-HELD-QUERY-NONHOLDER-RELATION-PRESERVATION-001 IN_PROGRESS / TEST_FIRST。持有140已before0/立即0，fulltick90差异定位CPoint同步后角色GetHeldEntity把负/零关系清为0/-1；只拆非持有者cache查询分支，正关系失效处理保持，先精确测试。父HELD-NATIVE-FRAME-BINDING仍IN_PROGRESS，Q06未完。

> 当前Q06 NTSD28-Q06-HELD-NATIVE-FRAME-BINDING-001 IN_PROGRESS / SOURCE140_PASS_UNITY_PENDING。源build/double-run140行1467643bytes SHA8ddb447…5b9610，105valid/21unsupported/14terminal，1344+420独立检查PASS；following126alive/全部lifecycleSuccess，完整following尚未Unity对照。初版reciprocal建立过早导致零分支，失败归档后已修。下一在同Record先新增准确单Editor fixture路径，以两profile/两factory复建完整before并比立即与following，再根据RED限定held两caller native读帧；不要直接改共享setter(type3命中也调用)。remaining reader审计187occurrences/初步matrix仍IN_PROGRESS，identity/rawsetter/lateEffects authority等后继保留，前已验三包不重做。无运行build/test/agent/Play；Q06未完/Q07未部署/总目标ACTIVE，禁止computer-use/Scene/资源/非战斗修改。

> 当前Q06剩余reader审计IN_PROGRESS：187词法occurrences已存inventory，初步live/compat/unknown矩阵已写。下一独立 NTSD28-Q06-HELD-NATIVE-FRAME-BINDING-001 IN_PROGRESS / SOURCE_WITNESS_FIRST；仅新诊断CPP预声明，正式held两条分支旧HasFrame857/descriptor阻断隐式与高帧后续定位，先source完整状态，不批量替换getter。identity/fusion、通用rawsetter、lateEffects authority及其它hit/AI/spawn caller仍待审计；旧throw/shared native-input绕过路径保留。前cost/physics/oracle三包已限定VERIFIED不重做。Q06整体未完成、Q07未部署、目标ACTIVE。

> 当前BATCH-03/Q06：NATIVE-INPUT-ACTION-COST-FRAME-READERS-001、CANONICAL-CHARACTER-PHYSICS-TAIL-001及NATIVE-DJA-UNAVAILABLE-SELF-CHECK-ORACLE-001均VERIFIED（各自声明范围）。源151四完整tick0差异、302replay/604tick、56实际physics、SelfCheck19:09:57Z、真实Play1208+56、最终关闭19:11:02Z全部PASS；Scene checksum/borrowers2→2、restore4→4、world/slots/pools0、两帧Stopped，dirtyfalse/root14/HUDBg30/hashBCD1047B…0E9FB6保持。下一唯一Task NTSD28-Q06-NATIVE-FRAME-REMAINING-LIVE-READERS-AUDIT-001 READY_READ_ONLY：按实际caller分类余下legacy getter/raw setter，禁止批量替换/重做已关闭包；之后回DISPLAY-PROGRESSION/POST-DISPLAY。Q06整体未完成、Q07正式内容未迁移、raw3缺口/跨Worldepoch/stage USER_HOLD/例外保持；总目标ACTIVE。无运行job/build/Play，禁止computer-use/非战斗/Scene/资源修改。

> 当前新增独立 NTSD28-Q06-CANONICAL-CHARACTER-PHYSICS-TAIL-001 IN_PROGRESS / RED_CONFIRMED。源边界统一后151×两profile Legacy0差异、DataOriented326差异；准确Task/Change已建，只修快速物理尾部接线并新增定向测试。父输入费用Task等待该出口；Q06/总目标仍未完成。

> 最新Q06费用fulltick隔离：job53acd6f9d30f483daebeda0f0cdc12c2四组151 before0，Legacy各44差异(仅负X钳制)，DataOriented各370(还含state12落地动作/counter/Vy)。CS0，失败归档following-path-isolation-red。下一统一横向世界边界后再验证physics差异并建立独立准确Task/Change；勿扩大BCAW费用生产范围或用Legacy代替快路径验收。当前测试已终止、无运行job，Q06未关闭/Q07未迁移/总目标ACTIVE。

> 当前 BATCH-03/Q06：NATIVE-INPUT-ACTION-COST-FRAME-READERS-001 IN_PROGRESS。151输入费用两profile端点0差异、Combo18/18通过；新增完整相邻tick两profile失败并归档following-initial-red。下一先统一world stage边界(source Z0/Unity默认ZMin180)，再对照Legacy/DataOriented physics：快速路径缺少普通路径已有state12/18 contact调用，尚未运行分离验证，不判定完成。Q07未迁移，总目标ACTIVE；禁止computer-use/Scene/资源/非战斗修改。此前顶部READY/生产未改描述已过时，仅保留历史。

> 当前BATCH-03/Q06：CPOINT-THROW-NATIVE-RAW-BINDING-001与NATIVE-INPUT-MISSING-STATE-ROUTING-001均已VERIFIED（各自声明范围）。source392/432、两profile输入及投掷立即/fulltick、112场景224重放tick、SelfCheck18:13:58Z、真实Play18:15:16Z1568+864及关闭18:16:07Z全部PASS；Scene checksum保持/Renderer2→2、恢复4→4、World/slots/两pool0、两帧Stopped，Editor idle/notPlaying、dirtyfalse/root14、HUDBg30/hash BCD1047B…0E9FB6保持。下一唯一Task NTSD28-Q06-NATIVE-INPUT-ACTION-COST-FRAME-READERS-001 READY_LIVE_SOURCE_MAPPING：BCAW ApplyNativeInputActionCore、rowing与builtin cost仍旧Has/Get；先追真实source apply_action/资源重定向与fallback完整事务并建准确Record，未改该项脚本。不要重做已关闭Cpoint/source/replay/Play。Q06/full alignment仍未完成，Q07正式资源未迁移，raw3缺口/跨Worldepoch/stage USER_HOLD/例外保持，禁止computer-use/非战斗/Scene/资源修改，总目标ACTIVE，无运行job/build/agent/Play。

> 当前BATCH-03/Q06：qualification真实driver补验8例16:54:03Z PASS、关闭16:55:03Z PASS；qualification/collision-frame/prelude-feedback三父包已按声明范围VERIFIED，旧失败不再当前阻塞。下一唯一Task NTSD28-Q06-CPOINT-THROW-NATIVE-RAW-BINDING-001 READY_SOURCE_WITNESS_AND_EXACT_RECORD（已创建精确Task，尚未改脚本）：生产ApplyThrow旧next预取+两个Cpoint raw setter旧857门/descriptor，会让原生高帧action与snapshot分离；先当前source见证，保护RunKind2Validation212，禁止全局getter替换。987/reduced/gain/各端点及kind2/kind3/C25已验不重做。BDEFEND等其它父字段记录需按各自出口回链，不据三包VERIFIED自动标全Q06。现无build/test/agent/Play运行；Q07资源迁移未启动，原raw3缺口/跨Worldepoch/stage USER_HOLD/用户例外保持，禁止computer-use/非战斗/Scene/资源修改，总目标ACTIVE。

> 当前阶段BATCH-03/Q06。VERIFIED / DECLARED_NONCHARACTER_REDUCED_AND_FALLBACK_SCOPE。987四组before0/diff0，资源联合19/19、type3联合27/27、完整SelfCheck16:36:40Z PASS；本批local replay54场景/108重放tick（job7fa21a8a1a9546ef96d214ed84048cdb 2/2 PASS）；真实Play16:45:17Z 3948/3948 PASS，两factory/direct-Shadow/Scene checksum保持/Renderer2→2；关闭16:45:31Z PASS，restore4→4、World/slots/两pool0、两帧Stopped。独立最终review四生产路径未发现确定新错误。当前四个qualification完整driver回访job65d082698b534717b51999e3be656a5b 4/4 PASS；原失败保留。不据此关闭全部Q06、跨World恢复或正式内容/视听；资源0/1hop/gain边界及两级resolver focused为实际范围，未穷尽所有owner组合。 源子项同样限定VERIFIED。下一最小任务：qualification父出口审查/其明确目标Play证据（若缺则只补四个完整driver在真实Play的场景保护验证），随后收口qualification/collision-frame依赖并返回NATIVE-FRAME-RUNTIME-READER-MIGRATION剩余live reader调用图。不要重做原3264端点、source构建、spark/feedback/weapon/type5/reduced；旧父文档失败检查点已追加纠正。当前无build/test/agent/Play运行，Editor已退Play；Q07正式DAT/图片仍未部署，raw3缺口/epoch/stage USER_HOLD及用户例外保持，总目标ACTIVE。禁止computer-use/非战斗/Scene/资源修改，未提交推送。

> 当前阶段仍BATCH-03/Q06。NONCHARACTER-REDUCED-HIT-TRANSACTION-001 IN_PROGRESS / ALL987_AND_SELF_CHECK_PASS。source owner987双跑SHA42367528…44bc9/545439PASS；Unity先before0 direct140/Shadow284，确认旧helper读Runtime.MP错误，root新gain块改Runtime.PP/MPMax，旧MP/helper保持；HitPlan独立resource-owner tuple(PP/消费/类型/上限/localflag/冻结slot+handle)及最多2级独立resolver已写，gain非0观察限制已移除。job6c1197e1854b4f2c8d420f73393a5d3a 19/19 PASS含987四组0差异/owner解析/旧纯函数/scope。后C30 SelfCheck暴露type3 target hit声，parent生产和标准/D1预测修正，音频oracle重新开/关留痕；job a568ca84a4a948e5942c57e0e2244a85 27/27 PASS含987及type3专项，完整SelfCheck16:36:40Z PASS。XML/失败/最新PASS全部parent artifact。下一直接本批真实Play987×两factory×directShadow=3948、关闭及本批local replay；尚未新增对应probe，不要重跑已过矩阵或重做source。若需两级resource-owner实际Shadow/非零localMode边缘可补必要focused，当前987只有0/1hop/invalid、旧resolver两级7项已过，不声称整个owner所有输入穷尽。当前无job/build/agent运行，工作树HEAD观察为f9f7b133（本任务未提交），既有外部提交不回退。Q07资源未部署、schema/raw缺口/跨World epoch/例外保持，总目标ACTIVE，禁止computer-use/非战斗/Scene/资源修改。

> 2026-09-15 当前阶段：BATCH-03 / Q06，IN_PROGRESS。Q01～Q05已达到各自限定出口；Q07正式DAT/角色图片迁移尚未启动，Q08～Q12等待前置。当前Q06子项为NONCHARACTER-REDUCED-HIT-TRANSACTION：843四组0差异及原984回归已通过；正在补非零gain/resource-owner独立观察。新增source owner987（含144 owner/gain边界）已双跑一致SHA42367528b7429d0dfb525a5c3208151f1bdd0937db7657b7b4a0b40066044bc9、545439独立检查PASS；Unity fixture已准备切owner987并恢复OwnerSlotIndex，但尚未刷新/跑987 RED，Shadow仍gain!=0不覆盖。下一步明确：刷新→987 before0及覆盖RED→扩独立resource-owner tuple→复测，再本批SelfCheck/Play/关闭/replay。无运行build/test/agent；不得将source987通过当Unity987通过。以下旧检查点仅保留历史，当前入口以此为准。

> 当前NTSD28-Q06-NONCHARACTER-REDUCED-HIT-TRANSACTION-001 IN_PROGRESS / ALL843_FOCUSED_PASS。root已新增LF2Entity.NativeHitCandidateScope可空原Route/嵌套恢复，runner先Resolve再Begin；DamageWriter weapon/type5/type3水平后按明确BrokenFallback消费、bypass -1保持、type3 generic音效去除；HitPlan已同步原route与独立fallback预测。job46a5d9c9928c4c13b69d75976a1feb87 5/5 PASS：843四组before0/diff0+scope嵌套异常恢复。原回归jobf25fb55daf6941ca8c49a4cbe64d36b1 18项17PASS/1旧type3音频断言FAIL，原984/684early/Bdefend/prelude replay全PASS；独立oracle NTSD28-Q06-TYPE3-NATIVE-AUDIO-ORACLE-001 VERIFIED，单断言修正后job4b866e2569774961ad93f303a054d398 4/4 PASS。所有XML同parent artifact保留。下一必须项非零gain/resource-owner独立捕获/比较与源/Unity向量，现CanProjectNativeNoncharacterReduced明确gain非0拒绝，不能当完成；还需新selfcheck、Play两factory/关闭及本批local replay和必要type3专项音频回归。当前无job/build/agent运行；本批四生产文件+单新fixture及单oracle，未改Scene/资源/非战斗。Scene SHA BCD1047B…0E9FB6保持。总目标ACTIVE/Q07未部署，禁止computer-use。

> 当前NTSD28-Q06-NONCHARACTER-REDUCED-HIT-TRANSACTION-001 IN_PROGRESS / REDUCED_CORE_PASS_FALLBACK_RED。三生产文件已改：runner原dispatch传prelude.Route，新DamageWriter.TryApplyNativeNoncharacterReducedHit完整reduced；HitPlan保存原NativeRoute并独立预测（worker单文件完成root已读）。刷新编译无CS错误，job843b7b0c7dea469085d07194dda2a170终态四FAIL：843 before0，direct67差异(原14271)，Shadow94(原14346)；681实际reduced行四组全部0差异，剩余仅162fallback子集中的hp_activation/mp_activation/bypass_minus_one。原red和本轮reduced-core-pass-fallback-red均归档。下一步继续修复原命中route到unarmored fallback的明确broken pointer消费，不能HP==-1猜；type3回退音频及Shadow fallback guard未覆盖也需处理，精确必要新增路径先入Record。潜在方式扩展已有LF2Entity.NativeHitCandidateScope瞬时route并确保嵌套恢复/重置，但尚未声明或改该文件本批。新reduced production有非零gain owner处理，Shadow明确gain!=0不预测，需resource owner tuple与非零专项源/运行验证，不能永久跳过；843gain0仅本范围通过。无SelfCheck/Play/原984新回归，未整批关闭。当前无build/test/agent运行，禁止computer-use/非战斗/Scene/资源修改，Q07未部署、总目标ACTIVE。

> 当前NTSD28-Q06-NONCHARACTER-REDUCED-HIT-TRANSACTION-001 IN_PROGRESS / PRODUCTION_RED_CONFIRMED。新增单Editor fixture接source843。首次job268c98…因integer/precise归一化before274失败（fixture-position-red保留），仅恢复Runtime.XInt/YInt/ZInt修正。第二job256ee489459a428c9e21be219e390c31终态四FAIL：每组843 before0，direct14271差异、Shadow14346差异，production-red目录完整JSON/XML。source独立review可推进该合成矩阵RED，仍明确未激活resource transfer/status/weak/scale、非零reference/其它attacker类型是后续验证边界。下一步在本Record先加入准确SequenceRunner/DamageWriter/HitPlan路径再实施native reduced route传递与完整事务及独立预测，不再重复RED。原parent108小伤害不足以收口；843所有groups及原984保护都需回归。当前无测试job/build/Play运行，本批Unity生产未改。禁止computer-use/非战斗/Scene/资源修改，Q07未部署、总目标ACTIVE。

> 当前 NTSD28-Q06-NONCHARACTER-REDUCED-HIT-TRANSACTION-001 IN_PROGRESS / TEST_FIRST_ONLY，root新增单Editor fixture接843，source子项仍保留实际独立验证边界。尚未改本批Unity生产。

> 当前NTSD28-Q06-NONCHARACTER-REDUCED-SOURCE-WITNESS-001 IN_PROGRESS / ACTION843_PASS_SOURCE_REVIEW。新增96 current4/7/70/75/action30/110/Y±5/Bdefend39/40/41行，总843；成功构建两run exit0/11275383bytes一致SHA c2e036569bb39f3692e34c8fce9061083acc18deec228c5cf3d04f413b1abdd0，459183检查PASS，action843归档。发现并纠正预期：state70/75不论朝向优先defense(armor null/threshold30)，state7同向才type1 armor(threshold40)；实际取current而非snapshot0state4。所有行新增完整CRT递推/sync标量事件/火花geometry与selected-route ID检查；162fallback追加broken-pointer HP0对比bypass初始-1保持、原HP/HPBound及不扣armorMP断言。full reduced raw/extra/rest及全部finalizer已验；fallback其它普通尾部仅source输出，不冒充独立全尾部证明。下一步收敛该源任务实际验证边界并独立审查，进入843 Unity fixture RED（source完整DAT/before/after可直接复建）；不要再无证据扩张源矩阵。保留资源/动作/route所有组，不缩到108小伤害测试。首次build参数输出误入helper错误及case771错误route预期已记录，成功重建数据为准。无build/test/agent运行，本批Unity生产未动，总目标ACTIVE/Q07未部署，禁止computer-use/非战斗修改。

> 当前NTSD28-Q06-NONCHARACTER-REDUCED-SOURCE-WITNESS-001 IN_PROGRESS / RELATION747_PASS_REMAINING_SOURCE_CHECKS。675的validator已强化全部reduced raw/extra/rest独立预测和全部行after→finalizer全状态计算；再加72 negative-parent/packed-delay行共747，构建/双跑exit0、9920711bytes一致SHA1cd3335b9f912a52a2a05c174a3bf192fa3a3f67c096e5e6f59b1701741c1931，379755检查PASS，relation747目录完整归档。有效parent2无reciprocal也复制rest之后hold，不取负；无效9不fallback；delay105/-205有符号计算已验。162 fallback仍只有选择/activation及finalizer检查，完整普通尾部不冒充验证。下一步补current7/70/75、action110阈值与snapshot分离、spark/随机完整独立递推、broken-fallback关键指针差异；然后Unity fixture RED及三生产文件接线。两Tools root独占，worker撤权停止；本批未改Unity生产，无build/test运行。源任务未关闭，总目标ACTIVE/Q07未部署，matched限定VERIFIED不重做，禁止computer-use/非战斗/Scene/资源修改。

> 当前NTSD28-Q06-NONCHARACTER-REDUCED-SOURCE-WITNESS-001 IN_PROGRESS / RESOURCE675_PASS_FULL_TUPLE_PENDING。root两Tools追加324 MP临界、72 armorHP临界、6 bypass_minus_one，共675；构建/两run exit0，8952470bytes一致SHAeb64b286317163c9435bcf1bd42d08d133bc976b52cf52a9c54af34533697bec，独立22239检查PASS。resource675目录含first/repeat/manifest/validation，273和24历史保持。162例确实fallback，validator只验它们decision/activation，不把全部普通尾部算通过；其余513 reduced行已验资源/HP/durability/status/horizontal/post等。新增params armorMp/armorHp/decrease/currentMp/runtimeArmorHp/bdefend，输出armorDecision/activationAvailable/activationMpCost/armorBroken，不仅selectedArmorType。下一步补negative parent hold/current-action阈值、full tuple/finalizer及fallback关键指针差异独立断言，后再Unity RED/三文件实现；本批Unity生产仍未改。无运行build/test/agent，原source worker撤权停止。matched已限定VERIFIED不重做；总目标ACTIVE/Q07未部署/禁止computer-use与非战斗修改。

> 当前NTSD28-Q06-NONCHARACTER-REDUCED-SOURCE-WITNESS-001 IN_PROGRESS / MOTION273_PASS_RESOURCE_PENDING。root两Tools已扩展273例（原24+motion216+effect_position24+post2000九例），构建与双跑exit0、3573700bytes一致SHA5e87945ff3ab9368b5225972a0b436333893cadb45aa3284f375ce07107c3c9b，独立9843检查PASS，motion273子目录含manifest。验证了地面±1/小数半速、Y/reference三侧、1002 0xF3及Z/-1.5、2000远离阻尼/相等不减速、22/23无反向例外。完整源任务仍未关闭：资源/armor临界/不足/破裂/bypass、negative parent hold、current/snapshot/action阈值、full tuple/finalizer断言仍待。下一步直接扩展ReducedCase/dat/emit与validator；armor_resolution.cpp105确认MPcost0强制1、currentMP<cost不足，HP<=effectiveInjury破裂（仅mp0）；记录路由不可只用selectedArmorType推断fallback。现无build/test运行，原worker已撤权停止，本批Unity生产未动。matched限定VERIFIED不重做，总目标ACTIVE/Q07未部署，禁止computer-use和非战斗修改。

> 当前NTSD28-Q06-NONCHARACTER-REDUCED-SOURCE-WITNESS-001 IN_PROGRESS / BASE24_PASS_FULL_MATRIX_PENDING。原worker两次无产出已中断且撤销写权限，root接管并实际新增两Tools。基础24例构建exit0、双跑exit0/309580bytes一致SHA e5f597a7435e5fb03d705b3bc3519f3fd4adc7bd4f034ccfb0160607816774fe，独立792检查PASS，base24子目录完整归档含manifest。六type×active/defense×injury±7，dvx5；明确尚缺全部资源/armor临界/运动/关系扩展，不能标完整源任务完成。下一步直接扩展当前ReducedCase/reduced_dat/emit_reduced和validator，不重做matched，不改本批Unity生产。Build/NTSD28NoncharacterReduced无运行进程；两子代理已停止或完成。精确路由和consumer/horizontal审计在父NONCHARACTER-REDUCED-HIT-TRANSACTION Task。总目标ACTIVE，Q07未部署，禁止computer-use/非战斗修改。

> 当前启动 NTSD28-Q06-NONCHARACTER-REDUCED-SOURCE-WITNESS-001 IN_PROGRESS / SOURCE_ONLY；两Tools脚本worker独占，root核对Unity候选级route消费，尚不改本批Unity生产。matched批次已限定VERIFIED不重做。

> 2026-09-14 15:15Z当前唯一游标：NTSD28-Q06-TYPE5-MATCHED-PAIR-EARLY-001 VERIFIED / DECLARED_NO_ARMOR_TYPE5_MATCHED_SCOPE；源NTSD28-Q06-TYPE5-MATCHED-PAIR-SOURCE-WITNESS-001 VERIFIED / SOURCE_MODEL_ONLY。138四组无差异、24+4相关回归、SelfCheck/Play552/关闭已通过；最后matched回放2/2 PASS，80场景160重放tick。两生产文件本轮未再次修改，只补原测试文件回放。下一唯一任务NONCHARACTER-REDUCED-HIT-TRANSACTION-001：先为完整reduced资源/armor/运动/随机补新source见证及准确Record，不能只把原108小伤害样本修绿。只读定位已写入该Task，三生产候选SequenceRunner/DamageWriter/HitPlan，命中时Route需保留，不用armorHP=-1猜。当前无运行job/build/Play。总目标ACTIVE/full incomplete，Q07未部署，schema/raw缺口/跨World epoch/stage.dat USER_HOLD及用户例外保持。禁止computer-use、非战斗与资源变化。

> 2026-09-14 15:09Z当前游标：TYPE5-MATCHED-PAIR-EARLY生产两文件已修复；四138 before0/diff0，24/24相关测试及另旧type3四项4/4 PASS。SelfCheck15:07:54Z PASS；真实Play15:08:59Z 552 PASS（两factory/direct+Shadow、Renderer2→2、场景checksum保持），关闭15:09:11Z PASS（恢复4→4、World/slots/两pool0、两帧Stopped），Scene dirtyfalse/root14。所有证据同ID artifact。唯一剩余本批验收：专门matched local snapshot replay未补（普通type5/weapon replay已通过，不能替代）；补该项及source/主Task治理收口后再进入NONCHARACTER-REDUCED108。当前无运行测试或build/Play probe，下一步不用重做已过矩阵与Play。总目标ACTIVE，Q07未部署，禁止computer-use/非战斗修改，HUDBg30保留。

> matched生产修复当前：138四组before0/diff0，job2f14606469fe40a6a8dab6c794c2ee2d 24/24通过；旧type3准确namespace补跑job33d31e5cef654979b380316477b99dc6 4/4通过。原Editor文件新增本批552真实Play probe，已请求刷新，尚未执行；本批SelfCheck及matched local replay仍待完成。生产两文件独立review未发现新增错误，Scene hash保持。无测试job运行；不要重复原RED或已通过矩阵。

> 2026-09-14 matched批次更新：source138/78933 PASS，Unity有效RED四组before0/3432差异后，已按准确Record修改DamageWriter和HitPlan：无armor type5 matched入口、native current/latch/raw reset、独立Shadow投影。编译Console暂未见CS错误；job 2f14606469fe40a6a8dab6c794c2ee2d正在执行本批/普通type5/weapon。旧type3实际namespace为NTSD.Test.Editor，需补跑准确名称，不能把未选中的类算回归通过。SelfCheck/Play本批未跑，保持IN_PROGRESS。

> 当前执行 `NTSD28-Q06-TYPE5-MATCHED-PAIR-EARLY-001` IN_PROGRESS / PRODUCTION_RED_CONFIRMED。source worker完成138/78933、两遍SHA f68655ba…2c8be，root独立validator PASS；source治理正式收口待整合。新增Unity测试已运行，修正fixture MaxPP→MaxMP后，job a8f32f45a53d45fab1c784f344d68fb6 四组各138/before0/3432差异，证据在同ID artifact production-red/；首次fixture失败也保留。尚未改本批Unity生产，下一步先精确声明DamageWriter/HitPlan路径，再实施matched早返与native latch/reset/Shadow，随后旧type3/普通type5/weapon回归及SelfCheck/Play。当前无运行中的测试或build。用户再次确认HUDBg30是其或其他任务修改，保持；禁止computer-use和非战斗变化。

当前唯一恢复游标（2026-09-14 14:28Z）：

- `NTSD28-Q06-TYPE5-UNARMORED-UNITY-001` VERIFIED / DECLARED_NO_ARMOR_ORDINARY_TYPE5_SCOPE。两生产文件已改：type5 native40/20/0分档/保留80/动作/机械/rest/post/音频，HitPlan独立type5预测；worker只读原型由root重写集成，reviewer只读复核。不要恢复旧50/30/10、清零80、legacy random/post或旧通用音频，不重做已验weapon。
- 源`TYPE5-UNARMORED-SOURCE-WITNESS-001` VERIFIED / SOURCE_MODEL_ONLY：585/14048，两遍SHA c164b073b4ee789df121f18dcffe8253d0771c21c37703eca35b2e73d4acf30e，source build在Build/NTSD28Type5Witness（避免Editor退出清理Temp），独立validator已保存Tools目录并登记Record。
- 原四组585 before0/直接各2277、Shadow各2862差异已清零；首次22/22 PASS含weapon14与Bdefend四256。后扩展50项46PASS/4FAIL：type5新增14个local replay/28ticks、原34和684早期等全部PASS；4FAIL仅原完整984 reduced108，每组846差异、before0、Shadow额外0。
- `TYPE5-HIT-PLAN-COVERAGE-AUDIT-001` VERIFIED / DECLARED_NO_ARMOR_TYPE5_WRITER_COVERAGE。原16例实测plan valid=true/failure0/dispatch1/writer0（不是实际伤害未执行）；先修真实type5再独立投影，现在Bdefend四256全PASS、每candidate观察1。armor/special states/全部DAT-CLR组合未据此关闭。
- 完整SelfCheck14:21:38Z PASS；真实Play14:24:37Z两factory×direct/Shadow×585=2340 PASS，Scene checksum保持/Renderer2→2；14:24:58Z关闭PASS：restore4→4，World/slots/两pool全0，两帧Stopped。Editor已退出Play、Scene dirtyfalse/root14/hash bcd1047b…保持，生产hash保持。
- **下一唯一Task：`NTSD28-Q06-TYPE5-MATCHED-PAIR-EARLY-001`，READY_NOW。** reviewer已定位native6616非角色初始matched3005/3006早返，Unity仅type3；旧pair reset/latch getter上限857在latch/action900错误。先新源见证和完整RED，正确位置复用rest→target reset→attacker reset→holder hold release，再独立Shadow；不能只加类型gate。正常非match下prev13/snapshot12、非零reference/正向child-rest也补最小向量。精确合同及source位置在Task和artifact independent-review.md。
- 然后`NONCHARACTER-REDUCED-HIT-TRANSACTION-001`处理108例（90active type1+18defense），破甲需进入命中时selected route，不能仅RuntimeArmorHp=-1猜测。父NONCHARACTER-ARMOR-FEEDBACK/BDEFEND/collision及Q06 umbrella未整体关闭；之后回完整984/driver/reader/表现，再Q07。
- 2022 GUI Editor曾退出，原MCP job无终态未伪报；已确认别项目FPSTest2023，两个独立2022 batch分别exit2保留RED。用户重新打开本项目2022 GUI后，预检阻止第二实例，已转原Editor MCP。现在无运行中build/test/Play/request；两个Astra子代理均只读完成。
- 总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE。Q07正式DAT/角色图片未部署，Scene仍旧内容/合成fixture；未验物理按键/Logan图片一致性。schema15/23/26/2/2、raw47/3、跨World allocation epoch缺口、stage.dat USER_HOLD/用户例外保持。用户HUDBg30已确认其或其他任务修改并保留，禁止computer-use、非战斗/Unity-GAS/Scene/资源/Server变化；未提交推送。

以下历史检查点由以上当前游标优先：

> 当前实现入口 `NTSD28-Q06-TYPE5-UNARMORED-UNITY-001` IN_PROGRESS / TEST_FIRST_ONLY。source585/14048通过，源two-run SHA c164b073…cf30e；单新增测试复用完整tuple，生产未改。type5旧16例明确plan valid=true、failure0、writer观察0，实际dispatch1，不能将mask0当覆盖通过。

> 当前必要依赖 `NTSD28-Q06-TYPE5-UNARMORED-SOURCE-WITNESS-001` IN_PROGRESS / SOURCE_ONLY。静态确认type5实际旧反应阈值/80清零与源不同，先585向量，不仅补Shadow。原2022 Editor已退出，旧job无最终证据；已核对只有别项目FPSTest的2023 Editor，当前独立2022 batch定向测试在运行，不操作别项目。

> 当前执行 `NTSD28-Q06-TYPE5-HIT-PLAN-COVERAGE-AUDIT-001` IN_PROGRESS / DIAGNOSTIC_ONLY：单Bdefend测试补完整plan/CLR/当前DAT诊断，生产本批尚未改。上一武器包已验收，不重做。

当前唯一恢复游标（2026-09-14 10:39Z）：

- `NTSD28-Q06-UNARMORED-WEAPON-REACTION-001` VERIFIED / DECLARED_WEAPON_TRANSACTION_SCOPE。三生产路径（DamageWriter、BruteForce kind0 heavy预处理、HitPlan）与测试已闭合声明事务；不要重做旧随机team/frame/self-rest尾部或恢复heavy减半。原source2100/43202，两遍SHA28cad088…55a。
- 最终48/48 PASS：source2100×2profile×direct/Shadow=8400全部0差异；原34、整数X两例、8case本地回放/16ticks、六oracle入口通过。每候选prelude/writer各观察1次。state2000读int X；state1002用native同步0xEE/16，state3000 post只执行一次；保持counter/latch/Fall80/team、精确0.55及type2 low-fall跳过Y/action。
- 独立`NTSD28-Q06-WEAPON-REACTION-SELF-CHECK-ORACLE-001` VERIFIED / TEST_ORACLE_ONLY：七个旧SelfCheck方法按当前源纠正，旧三次FAIL保留。完整SelfCheck10:33:50Z PASS。`NTSD28-Q06-WEAPON-REACTION-SOURCE-WITNESS-001` VERIFIED / SOURCE_MODEL_ONLY。
- 真实Play10:36:58Z两factory×direct/Shadow×2100=8400 PASS，Scene checksum不变/Renderer2→2；10:37:31Z关闭PASS，原位restore4→4、World/slots/两pool全0、两帧Stopped。Editor idle非Play，Console error0，Scene dirtyfalse/root14/hash bcd1047b…保持。生产/正式EXE hash保持，未提交。
- **下一唯一Task：`NTSD28-Q06-TYPE5-HIT-PLAN-COVERAGE-AUDIT-001`**。最新BDEFEND两profile direct256均PASS，Shadow每profile只剩16无armor type5观察guard；需输出valid/count/failure再落实对应owner，不删断言。当前DAT/CLR不一致的weapon旧Shadow guard亦为相关待核线索，未证明全部shell已覆盖。
- 接着`NTSD28-Q06-NONCHARACTER-REDUCED-HIT-TRANSACTION-001`：最新完整984四组各846差异/108例（90type1 active+18defense），before0、Shadow额外0；124无护甲weapon已清。原34、684早期四组与本地回放全PASS。父NONCHARACTER-ARMOR-FEEDBACK/BDEFEND/collision仍未关闭；reduced后回完整984/父矩阵，再reader/表现/Q07。
- 总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE。Q07正式DAT/角色图片未部署；真实Scene仍Unity旧内容与合成fixture，未做物理按键/Logan图片一致性验收。schema15/23/26/2/2、raw47/3、跨World allocation epoch恢复缺口、stage.dat USER_HOLD和既有例外保持。
- 用户确认HUDBg x30由其或其他任务修改，保留且不再询问；禁止computer-use、非战斗/Unity-GAS框架/Scene/资源/Server改动。本批准确记录/原始证据在同ID artifacts REPORT。所有build/test/Play/request均终态，无待轮询job。

以下历史检查点由以上当前游标优先：

> 当前执行保持UNARMORED-WEAPON-REACTION-001：8400 source对照/本地回放通过，原34回归通过；父完整984剩108 reduced、Bdefend只剩type5各16。活跃WEAPON-REACTION-SELF-CHECK-ORACLE-001正按源修订旧HitConfirm2/随机/team/self-rest/audio断言，不能回退新生产。最新额外state2000整数坐标2例RED已修两行，正定向复测；完整SelfCheck和Play尚未过，不标完成。

> 当前武器批次：`NTSD28-Q06-UNARMORED-WEAPON-REACTION-001` IN_PROGRESS / DIRECT_2100_PASS_SHADOW_PENDING。三生产路径已改、两profile各2100直接对照0差异；当前正编译并补Shadow独立预测。源见证`NTSD28-Q06-WEAPON-REACTION-SOURCE-WITNESS-001` VERIFIED / SOURCE_MODEL_ONLY（2100/43202）。非战斗/HUDBg30保留，Q07未部署、完整对齐尚未完成。旧下文TEST_FIRST_ONLY/生产未改描述仅为历史检查点。

> 当前执行 `NTSD28-Q06-WEAPON-REACTION-SOURCE-WITNESS-001` IN_PROGRESS / SOURCE_ONLY。补武器完整反应边界，不重做已过前置；Unity生产本轮尚未改。

当前唯一恢复游标（2026-09-14 09:31Z）：

- `NTSD28-Q06-NONCHARACTER-ARMOR-FEEDBACK-001` IN_PROGRESS / PRELUDE_FEEDBACK_RUNTIME_PASS_DAMAGE_DEPENDENCIES_OPEN。四生产文件已写：新BattleNativeOrdinaryHitPrelude、Resolver专用native入口、Runner真实前置顺序、HitPlan独立前置token/反馈预测。已不再是TEST_ONLY，禁止重复实现。详见同ID artifact REPORT.md。
- source984四组最新before0差异，after每组1518条/232case（原4641/712），Shadow额外错误0/每候选前置观察1次。source684前置/反馈/拒绝契约四组全PASS（210 feedback+294 rejected+180 unsupported），feedback另断言writer观察1。原完整984失败保留，不把684当全包完成。
- 旧34回归全PASS；完整SelfCheck09:17:43Z PASS；真实Play两factory×direct/Shadow×684=2736 PASS，before/after0差异、主Scene checksum保持、Renderer2→2；有序关闭PASS（原位恢复4→4、World/slots/两pool全0、两帧Stopped）。本地回放16场景/32replayed ticks通过，实际随机状态/links/rest/spark/完整checksum一致、Shadow有效。内部SynchronizedGeneration在restore故意失效旧cursor，测试已另验证旧cursor不能commit，非canonical allocation epoch问题。
- **下一唯一执行Task：`NTSD28-Q06-UNARMORED-WEAPON-REACTION-001`。** 处理完整frame/team/hitReaction/rest/legacy随机尾部；BDEFEND256剩64武器例×3 raw=192，新984对应124个无护甲/绕过护甲武器例。原已有源只是部分state/height，按live调用补必要边界，准确新Record后实施；不能只改frame3→186，不能重做已过前置/反馈。
- 然后`TYPE5-HIT-PLAN-COVERAGE-AUDIT-001`剩无armor16 guard（首type0的16已清）；再新Task`NONCHARACTER-REDUCED-HIT-TRANSACTION-001`的108例（90type1 active+18defense）。之后回完整984/BDEFEND256/父collision。完整984剩232=124weapon+108reduced。
- BDEFEND测试direct已纠正到正式candidate入口，独立`NTSD28-Q06-BDEFEND-FORMAL-CANDIDATE-ENTRY-ORACLE-001` IN_PROGRESS / ENTRY_FIXED_PARENT_DAMAGE_PENDING；原raw/HitStateCount241/C25断言保持。当前每profile direct192/Shadow208，四组FAIL待上述依赖，不把测试入口修订当生产伤害全通过。
- 前置关键规则保持：unarmored先reciprocal2/-2 release与special-rest，再type0反馈，再普通rest/firstbody；同步0xEC/6写child raw action，target Vy精确-1.0000000000000258、child Vy不改，getter矩阵[0][2]=45/[1][2]=30，槽历史保留。type1 bypass/active、defense rest早返后的fallback不可合并。新版Shadow在新token中捕获全Native随机与保留字段，旧ZeroAttackerHp consume位置保持，legacy heavy flag不二次执行。Native代次仅用于游标失效，不修改persistent schema。
- 所有build/test/Play均已终态，Editor已退出Play；MCP当前端口6403，每次仍读取状态文件发现，不硬编码。HEAD 61b3b6cf，本轮未提交。生产hash在SelfCheck/Play后保持，仅测试/文档再补回放。总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE，各父记录未关闭。
- Scene hash bcd1047b…、用户HUDBg x30、Unity/GAS/非战斗/Server边界保持，禁止computer-use。Q07正式DAT/角色图片未部署；schema15/23/26/2/2、raw47/3、跨World allocation epoch缺口、stage.dat USER_HOLD及其余例外不变。

以下历史检查点由以上当前游标优先：

当前唯一恢复游标（2026-09-14 08:43Z）：

- `NTSD28-Q06-PREARMOR-FEEDBACK-SOURCE-WITNESS-001` VERIFIED / SOURCE_MODEL_ONLY。984向量、5108检查、两遍逐字节一致SHA12323954821eceea6cfc7aa3155ae66102c4389f4fdb47813dd8febc374c0a4a；正式EXE/75源身份保持。完整source调用/字段/限定范围在同ID artifact REPORT。
- **下一唯一Task仍为`NTSD28-Q06-NONCHARACTER-ARMOR-FEEDBACK-001`，IN_PROGRESS / RED_CONFIRMED_BEFORE_INPUT_MATCHED。** 新984×两profile实际runner对照已跑，before受测raw47/3+links/rest/sparks0差异，after各4641条/712case。原FAIL见同ID artifact red/；当前Change Record只声明单新增测试脚本，尚未修改本批Unity生产。下一步按该REPORT落实单一ordinary hit前置owner、列准确生产code-path/Shadow阶段，再实施；不要继续重复source build或新建泛化审计。
- 必须一起解决的已证顺序：原unarmored在armor/普通rest/first-body前执行reciprocal2/-2 release和special-link-rest；Unity目前CanConsumeRecordedCandidate先查live vrest，first-body又早于ConsumeEffects，不能只在DamageWriter最前面加return。源synchronized0xEC/6→child action；target/holder Vy=-1.0000000000000258而child Vy7.5保持；rest按getter实际矩阵[0][2]=45、[1][2]=30。父子slot历史保留，special gate可作用于dormant，不能用active-only getter替代。
- 分支陷阱已补源样本：type1 ratio15<Bdefend17是bypass，会回unarmored prelude；Bdefend0的active/reduced另序。defended wrapper的reduced在rest提前返回时未保留defense_decision，普通unarmored入口可能继续fallback（state7+type0+rest5仍feedback）；type1顶层直接返回reduced，不可合成一个简单armor/defense bool。action_latch的Unity映射仍Runtime.WaitCounter，勿新造字段。
- 984含720基本组合、48 current第一bdy1033/1100500000、180 type1 active、36 defense/rest向量。source210 feedback、198前置后rest拒绝、180前置后unsupported、96 reduced rest全状态不变；24反馈跳过current第一bdy。此为诊断冻结候选初值，未证明完整driver可达或默认spawn links；测试显式赋source默认links0。
- 当前BDEFEND256也已重新测量：两profile direct各968/Shadow各1000条差异（4组FAIL），保留在source artifact unity-parent-red。新984测试入口已覆盖旧heavy release前置；不要把失败只当已排队weapon reaction而略过feedback/时序。尚未新测Shadow984/真实Play；当前生产上一批Spark的34/34、SelfCheck08:06:43Z、Play1440及关闭证据保持，Spark限定职责不重做。
- 本轮只新增1个源诊断CPP与1个Unity Editor测试及文档；上一批生产未再修改。所有build/test均终态，无运行中job。总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE；父BDEFEND/collision/qualification仍未关闭。反馈之后继续weapon reaction/type5覆盖，再回父包，reader/display后Q07正式资源迁移。
- 保留用户HUDBg x30、Scene hash bcd1047b…、Unity/GAS/非战斗/Server边界；禁止computer-use。Q07未部署、schema15/23/26/2/2、raw47/3、跨World allocation epoch恢复缺口、stage.dat USER_HOLD及其它用户例外不变。

以下历史检查点由以上当前游标优先：

> 当前执行 `NTSD28-Q06-PREARMOR-FEEDBACK-SOURCE-WITNESS-001` IN_PROGRESS / SOURCE_ONLY。反馈前的heavy release旧RNG/字段写入存在差异，先原完整前置取证，不直接早返；Unity生产本轮未改。

当前唯一恢复游标（2026-09-14 08:14Z）：

- `NTSD28-Q06-HIT-SPARK-UNITY-001` VERIFIED / DECLARED_HIT_SPARK_TRANSACTION_SCOPE。公共Append源guard/owner/capacity/编码/snapshot/cover/整数平均/target Z/Y-X CRT，candidate瞬时scope，角色route owner唯一emission、两个重复外层移除，Shadow独立CRT capture/compare已闭合。
- 最终34/34 PASS：source438×2端点876、角色actual282×2=564（复制itr/原index2）、异常嵌套/非0 guard、原完整driver四组8向量及Shadow0差异、local replay8case/16replayed ticks；完整SelfCheck08:06:43Z PASS。真实Play两factory端点+actual共1440于08:11:26Z PASS，Scene checksum不变/Renderer2→2；08:11:53Z关闭PASS，原位恢复4→4、World/slots/两pool全0/连续两帧Stopped。
- 旧C01 hook与SelfCheck观测分别由`NTSD28-Q06-SPARK-C01-TEST-HOOK-001`、`NTSD28-Q06-HIT-SPARK-SELF-CHECK-ORACLE-001` VERIFIED纠正；全部旧FAIL保留。唯一详细证据：`artifacts/diagnostics/NTSD28-Q06-HIT-SPARK-UNITY-001/REPORT.md`。主包8脚本+两个单测试子项，共10脚本；HEAD61b3b6cf，未提交。
- **下一唯一Task：`NTSD28-Q06-NONCHARACTER-ARMOR-FEEDBACK-001 / READY_SOURCE_ORDER_AND_EXACT_RECORD`。** 公共spark已就绪，先闭合原selected armor/special-link-rest前置及各Unity活入口，准确新Record后再接反馈Append(...,armor,false,false)，不能无条件早返或只保留Bdefend。非角色首type0原96例曾误伤害；type1不能从type0推断。之后UNARMORED-WEAPON-REACTION和TYPE5-HIT-PLAN-COVERAGE，再回BDEFEND256/父collision/qualification。96/64/32是之前生产的测量，未按本轮重测，不当新鲜结果。
- 父BDEFEND/collision/qualification、reader umbrella与总目标仍IN_PROGRESS / ACTIVE，FULL_ALIGNMENT_INCOMPLETE；旧四组完整driver的Spark RNG首差已经清除，不再当当前阻塞。reader/display其它职责后才Q07，正式DAT/角色图片**尚未部署**。Scene仍旧内容/合成fixture，未做物理按键或Logan图片一致性验收。
- 正式EXE SHA B1E13AE1…D2819033、源438 trace b5df6113…ce6f8复核保持；Scene dirtyfalse/root14/SHA bcd1047b…保持，用户HUDBg x30确认归其或其他任务并保留。schema15/23/26/2/2、raw47/3、跨World allocation epoch恢复缺口、stage.dat USER_HOLD及既有例外保持。禁止computer-use、非战斗/Unity-GAS框架/Scene/资源/Server改动。

以下历史检查点由以上当前游标优先：

> 当前执行 `NTSD28-Q06-HIT-SPARK-UNITY-001` IN_PROGRESS / TEST_FIRST，准确七脚本；公共writer与candidate瞬时scope、route owner一起闭合，禁止computer-use。

> 工作树补记：当前HEAD61b3b6cf（111），本轮未提交；新source CPP已被工作树外部提交包含且SHA与build一致，最后validator code diff0。不要据旧未提交清单回退或重做。

> **当前唯一恢复游标（2026-09-14 07:37Z）：** HIT-SPARK-SOURCE-WITNESS-001 VERIFIED / SOURCE_MODEL_ONLY。原438/2994断言、两遍一致SHAb5df6113…；366追加+CRT2/72不追加+CRT0，Native0，原EXE/75源身份保持。完整owner/capacity/编码/index/armor/负数几何/CRT数组已证；本轮只新CPP/文档，未改Unity生产、未跑新Unity测试。Scene文件hash bcd1047b…保持。
> **下一唯一Task：`NTSD28-Q06-HIT-SPARK-UNITY-001 / READY_EXACT_CONTEXT_AND_TEST_FIRST_RECORD`。** 先读source artifact REPORT，准确公共emitter/真实caller/瞬时原itr index与hit前selected armor上下文Record，再438 Unity RED/实现。重要纠正：LF2Character.Hit→Dat普通分支ApplyStandardCharacterDamage后RecordKind0Hit即return，普通命中不走底部旧SpawnSpark；不能只改它两行。旧CurrentItrIndex仅Character BeforeDispatch写，generic为空，不能当可靠通用index。route owner保留armor和unarmored/reduced/feedback flags，每次hit只发一次，移除重复外层调用，C01/C25记录生命周期保持。
> 后继顺序仍Spark Unity→NONCHARACTER-ARMOR-FEEDBACK（96）→UNARMORED-WEAPON-REACTION（64）/TYPE5-HIT-PLAN-COVERAGE（32guard）→回BDEFEND256/完整driver。BDEFEND字段已写但父仍IN_PROGRESS；旧完整driver只有RNG差异，所有原失败保留。此前SelfCheck07:18:42Z是旧生产证据，本轮无新Play/自检；无运行中build/test/exec。总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE。
> 用户HUDBg30保留，禁止computer-use、非战斗/Unity-GAS/Scene/资源/Server更改；raw47/3、schema15/23/26/2/2、epoch恢复缺口、Q07未部署、stage.dat USER_HOLD及用户例外保持。

以下历史检查点由以上当前游标优先：

> 当前执行 `NTSD28-Q06-HIT-SPARK-SOURCE-WITNESS-001` IN_PROGRESS / SOURCE_ONLY。普通命中实际走RecordKind0Hit而非底部旧SpawnSpark；原三入口/index/armor/geometry/capacity/CRT一起取证，生产未改。

> **当前唯一恢复游标（2026-09-14 07:21Z）：** BDEFEND-FIELD-FAMILY-UNITY-001仍IN_PROGRESS / FIELD_FIX_WRITTEN_DEPENDENT_HIT_PATHS_OPEN。三生产文件已写Runtime.Bdefend的45/signed累加/阈值、armor delay读Bdefend、Shadow独立transient TargetBdefend（保留legacy观测），C25/compat/schema不改。新256×4仍FAIL，direct各968/Shadow各1000；角色全部和type3/5无armor raw/字段已匹配。当前完整driver四组bdefend0/45已清，仅Spark RNG legacy3/native0/CRT0对源CRT2失败。全部原FAIL保留，未标已对齐。
> **下一唯一Task：`NTSD28-Q06-HIT-SPARK-TRANSACTION-AUDIT-001 / READY_NOW`。** 原非角色所选armor反馈只调用完整spark事务即return，故必须先Spark，再NONCHARACTER-ARMOR-FEEDBACK-001（96例误走damage），再UNARMORED-WEAPON-REACTION-001（64无armor武器team/frame/fall首差）、TYPE5-HIT-PLAN-COVERAGE-AUDIT-001（32Shadow附加guard失败mask0，待打印valid/count）。随后回BDEFEND256/完整driver和两factory运行验收。不能用空return或只保留Bdefend掩盖反馈事务，C17例外仅legacy1。
> 独立BDEFEND-TEST-ORACLE-001 VERIFIED：原SelfCheck07:14:34Z旧HitStateCount观测FAIL保留，准确StandardCharacter/C30/Alternate fixture改Bdefend，07:18:42Z完整PASS；其他断言/compat API保持。本轮尚无新Play验收，不引用旧Play作为新生产通过。Editor idle非Play，无运行中test/build/exec；Scene文件SHA bcd1047b…保持。下次只需继续Spark源调用链和当前失败，不重建已验256/3264端点。
> 父BDEFEND/qualification/collision均进行中，总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE。用户HUDBg30保留；禁止computer-use、非战斗/Unity-GAS/Scene/资源/Server更改。raw47/3、schema15/23/26/2/2、epoch恢复缺口、Q07未部署和stage.dat USER_HOLD/用户例外保持。

以下历史检查点由以上当前游标优先：

> 当前执行 `NTSD28-Q06-BDEFEND-FIELD-FAMILY-UNITY-001` IN_PROGRESS / TEST_FIRST，准确四脚本，原256和独立legacy HitStateCount哨兵，C25/兼容API保持。

> **当前唯一恢复游标（2026-09-14 07:00Z）：** BDEFEND-FIELD-FAMILY-SOURCE-WITNESS-001 VERIFIED / SOURCE_MODEL_ONLY。原256/1280断言、两遍一致SHA6ba2e26f…；分支148覆写45、96非角色首type0反馈保留、6 signed累加、6armorHP保护，接续C25 held/free恢复验证。确认Runtime.Bdefend raw绑定及既有C25h owner正确；旧HitStateCount无当前C25h owner，不可全局alias。当前轮只新诊断CPP/文档，未改Unity生产或资源，未跑新的Unity测试。
> **下一唯一Task：`NTSD28-Q06-BDEFEND-FIELD-FAMILY-UNITY-001 / READY_EXACT_CALLERS_AND_TEST_FIRST_RECORD`。** 先精确actual三类writer/armor route reader/Shadow分支，原256设Bdefend与legacy HitStateCount不同值做RED。无护甲写45，非角色首type0反馈不得写45，reduced signed add不能clamp，armor matcher读Bdefend，Shadow须独立观测该字段；原C25恢复/copy/schema/compat字段保持。OID300/kind7/D1投影及reduced current/Prev2/Y条件不能按grep泛改，按新Task补必要源证据。之后HIT-SPARK-TRANSACTION-AUDIT-001，再回4完整driver FAIL。
> 父COLLISION-CURRENT-SNAPSHOT-QUALIFICATION-001和COLLISION-FRAME-UNITY-001仍IN_PROGRESS；3264端点已0差异，但8个完整driver向量仍bdefend0/45与RNG legacy3/native0/CRT0（源CRT2）首差，未豁免。此前SelfCheck06:27:14Z、Play480/关闭等通过是前轮证据，不报本轮新运行。无运行中build/test/exec；Scene文件SHA bcd1047b…再次保持。总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE。
> 用户HUDBg x30保持；禁止computer-use、非战斗/Unity-GAS/Scene/资源/Server更改。raw47/3、schema15/23/26/2/2、epoch恢复缺口、Q07未部署、stage.dat USER_HOLD及例外不变。

以下历史检查点由以上当前游标优先：

> 当前执行 `NTSD28-Q06-BDEFEND-FIELD-FAMILY-SOURCE-WITNESS-001` IN_PROGRESS / SOURCE_ONLY。已确认Bdefend是+0x0B8且C25h正确恢复，需迁移命中写入/护甲读取/Shadow同字段家族；不把旧HitStateCount全局alias。

> **当前唯一恢复游标（2026-09-14 06:40Z）：** COLLISION-CURRENT-SNAPSHOT-QUALIFICATION-001及父COLLISION-FRAME-UNITY-001仍IN_PROGRESS / QUALIFICATION_RUNTIME_PASS_FULL_DRIVER_DEPENDENCIES。普通/cached/immediate/consumer的current额外门已移除，snapshot几何/current state0分离、pair current-null state0、previous Native；原336×4+新480×4共3264端点全部0差异，旧96candidate首差清零。最终12项8PASS/4FAIL，4FAIL是完整driver两例×四组的独立bdefend0/45与RNG legacy3/native0/CRT0（源CRT2）差异；其余raw/HP499/caught动作/snapshot0一致、Shadow有效/2effects/mask0。原所有FAIL保留，不能标全包VERIFIED。
> **下一唯一Task：`NTSD28-Q06-UNARMORED-BDEFEND-WRITER-AUDIT-001 / READY_SOURCE_WRITER_AND_FIELD_MAP`。** 原battle_world6743无护甲命中直接写bdefend_accumulator45；Unity raw是Runtime.Bdefend，但actual/plan多处仍写独立HitStateCount。先字段/消费者/恢复与分支闭环，准确Record再改；不是DAT默认值，不改converter补45。之后HIT-SPARK-TRANSACTION-AUDIT-001：DatHitResolver.SpawnSpark828/829旧BattleRandInt两次应核整个原CRT/snapshot/容量/owner事务，两factory一起；C17例外仅legacy1，不豁免额外2。两子项后回4完整driver FAIL，不重做已通过kind2/3和3264端点。
> Source COLLISION-QUALIFICATION-SOURCE-WITNESS-001已VERIFIED限定源480+2/3020断言/重复一致；独立COLLISION-ROLE-MATRIX-ORACLE-001 VERIFIED，原70回归69PASS/1旧期望FAIL，按source纠正后4组合PASS。完整SelfCheck06:27:14Z PASS，真实Play480/Renderer2→2/Scene checksum保持、有序关闭06:38:10Z恢复4→4、World/slots/两pool0/两帧Stopped。Editor idle非Play、接口6402（状态文件发现）、CS0、Scene dirtyfalse/root14/SHA bcd1047b…保持。无运行中test/build/exec，不再等旧job。
> 总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE。用户HUDBg x30确认归其或其他任务并保留；禁止computer-use/非战斗/Unity-GAS/Server改动。raw47/3、schema15/23/26/2/2、epoch恢复缺口、Q07未部署、stage.dat USER_HOLD和用户例外不变。

以下历史恢复检查点由以上当前游标优先：

> 当前执行 `NTSD28-Q06-COLLISION-QUALIFICATION-SOURCE-WITNESS-001` IN_PROGRESS / SOURCE_ONLY；单CPP补pair非零state/原eligibility与完整driver换帧见证，Unity父包仍未关闭。

> **当前唯一恢复游标（2026-09-14 05:51Z）：** COLLISION-FRAME-UNITY-001仍IN_PROGRESS / READER_RUNTIME_PASS_QUALIFICATION_PENDING。三生产reader已改（LF2Entity collision、BruteForce current/Prev2、pair factory current/previous），新336×4 descriptor/raw/catch0，但各24candidate首差（合96）保留。kind2/3旧八组PASS、pair/group/catch64PASS；完整SelfCheck05:48:49Z PASS、真实Play168/Renderer2→2/Scene checksum与有序关闭全0/两帧Stopped通过。独立COLLISION-FIXTURE-DEFINITION-IDENTITY-001两处测试修订VERIFIED，原两SelfCheck FAIL保留。源COLLISION-FRAME-SOURCE-WITNESS-001追加84后336限定VERIFIED、旧252前缀不变。
> **下一唯一Task：`NTSD28-Q06-COLLISION-CURRENT-SNAPSHOT-QUALIFICATION-001 / READY_SOURCE_CONSUMER_AND_QUALIFICATION_MAP`。** 原普通geometry按snapshot itr，platform另按current itr；Unity carrier/current itr/body/null门额外拒绝。先原consumer/pair非零state/当前0语义和正式可达性，准确子Record后整体处理普通/cached/consumer，不豁免24失败。无需重做kind2/3和已验证source336。parent collision未关闭，pair state/filter与actual+Shadow新边界尚欠证据。
> 当前无运行中test/build/exec；Editor idle非Play、CS0、Scene dirtyfalse/root14/SHA bcd1047b…保持。用户HUDBg x30确认归其或其他任务并保留。总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE；raw47/3、schema15/23/26/2/2、epoch恢复缺口、Q07未部署及stage.dat USER_HOLD/用户例外不变。禁止computer-use、非战斗/Unity-GAS/Server改动。

以下为历史恢复检查点，以上当前游标优先：

> 当前实施 `NTSD28-Q06-COLLISION-FRAME-UNITY-001` IN_PROGRESS / TEST_FIRST；准确四脚本，先原252输入Unity RED，禁止computer-use及非战斗变更。

> **当前唯一恢复游标（2026-09-14 05:29Z）：** COLLISION-FRAME-SOURCE-WITNESS-001 VERIFIED / SOURCE_MODEL_WITNESS_ONLY。252原输入/3780断言、两遍字节一致，72有候选/108kind1推进、RNG0，正式EXE/75源身份保持。35调用/7文件、无override；本轮只新增诊断CPP，未改Unity生产/资源/Scene。用户已确认HUDBg x30为其或其他任务修改，保留。
> **下一唯一Task：`NTSD28-Q06-COLLISION-FRAME-UNITY-001 / READY_UNITY_RED_AND_LIVE_GATES`。** 原当前定义+snapshot点查不fallback current。LF2Entity、BruteForce current/Prev2及pair factory有耦合旧门，先原向量Unity RED并核对正式时点可达性，准确新Record再实施。source current1000/snapshot0仍保留候选属于诊断分离初值，不能单独授权删全部current门；正式collection前刚snapshot。CPoint kind1用snapshot，孤立kind2用current。旧throw局部缓存/ThrowInjury==-1变身须另找当前authority，不能沿用历史结论。
> 此轮未跑新Unity compile/SelfCheck/Play；上次05:10:10Z SelfCheck是旧生产证据。无运行中build/test/exec。Scene SHA bcd1047b…保持；总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE。raw47/3、schema15/23/26/2/2、epoch恢复缺口、Q07未部署、stage.dat USER_HOLD及所有用户例外不变。禁止computer-use/非战斗/Unity-GAS/Server改动。

以下为历史恢复检查点，以上当前游标优先：

> 当前执行 `NTSD28-Q06-COLLISION-FRAME-SOURCE-WITNESS-001` IN_PROGRESS / SOURCE_ONLY。碰撞 getter 审计已定位 35 处调用/7 文件（另 1 声明）、无 override；原当前定义+snapshot 查询，无 current fallback。仅单 CPP 原函数见证；Unity/资源未改，用户确认 HUDBg x30 为其或其他任务修改并保留。

> **当前唯一恢复游标（2026-09-14 05:12Z）：** KIND2-PICKUP-NATIVE-FRAME-LOOKUP-001和NATIVE-PHYSICS-MISSING-FRAME-GUARD-001均VERIFIED限定职责。原1200/Unity四组4800拾取+紧接holder物理0差异，新16/16、旧与完整frame215/215、SelfCheck05:10:10Z PASS、Play800/Scene checksum/Renderer2→2、有序关闭全0/两帧Stopped。actual/HitPlan Native目标读取及窄raw holder绑定一起闭合；缺帧physics gate保留pending/hold/link先后，valid998/decl999继续运行。无运行中test/build/exec，不再等待旧job。
> **下一唯一Task：`NTSD28-Q06-COLLISION-FRAME-NATIVE-LOOKUP-AUDIT-001 / READY_SOURCE_AND_CALLER_MAP`。** LF2Entity.GetCollisionFrameData仍旧HasFrame门并存在Prev2→current fallback；先列全部真实caller/override及非战斗边界，核对source snapshot/current/definition身份，不能机械换getter。它影响后继碰撞/命中和CPoint，优先于继续throw/direct setter。此前kind3/kind2/C25已关职责不重做；原reader umbrella仍进行中。
> 其它CPoint action/throw原定义快照、raw/held/input/hit/生成reader仍未全部迁移；之后display其余出生/post→Q07正式DAT/角色图。资源未部署、schema15/23/26/2/2、raw47/3、已使用World epoch恢复缺口和所有用户例外/stage.dat USER_HOLD保持。Scene dirtyfalse/root14/用户HUDBg x30/SHA bcd1047b…保持，禁止computer-use/非战斗/Unity-GAS/Server改动。总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE。

以下为历史恢复检查点，状态由以上当前游标替代：

> 当前必要子项 `NTSD28-Q06-NATIVE-PHYSICS-MISSING-FRAME-GUARD-001` IN_PROGRESS / TEST_FIRST；pickup getter/binding已写且拾取端0差异，后继physics1250/route缺帧移动仍待修。准确三脚本，保留hold/link/pending顺序和dead normalization。

> 当前实施 `NTSD28-Q06-KIND2-PICKUP-NATIVE-FRAME-LOOKUP-001` IN_PROGRESS / SOURCE_FIRST，准确五脚本；kind2 getter与raw holder native绑定共同闭合，pure plan/legacy共享入口不改。

> **当前唯一恢复游标（2026-09-14 04:36Z）：** KIND3-CATCH-NATIVE-FRAME-LOOKUP-001已VERIFIED / KIND3_PAIR_ENTRY_ONLY。原800（512允许/288双实体不变拒绝）与Unity四组3200 before/after raw47+catch字段零差异，最终25/25、完整SelfCheck04:34:40Z PASS、真实Play320/Scene checksum/Renderer2→2、有序关闭全0/两帧Stopped。仅actual/HitPlan的kind3 Native descriptor先检后写，kind1/shared setter未改。旧99/98不可用fixture与SelfCheck已按源改1000/1001，原FAIL保留；总计六脚本。无运行中test/native build/exec，不再等待旧job。
> **下一唯一Task：`NTSD28-Q06-KIND2-PICKUP-NATIVE-FRAME-LOOKUP-AUDIT-001 / READY_SOURCE_AND_CALLER_MAP`。** 审计actual TryApplyPickup和HitPlan.ProjectPickupWriterEffect的target legacy getter，以及同一pickup事务SetHolderAction→DirectWriteRawFramePreserveWaitCounter；先原kind2完整分支/已锁定表与caller，不预判或全局改getter。需要改时准确新Record，getter+必要holder绑定一起闭合。继续原Native reader umbrella，CPoint/throw/direct/raw/held/input/hit其它旧reader仍未全部迁移。
> C25/frame已关职责和本kind3不重做。已使用World snapshot canonical allocationEpoch恢复缺口仍OBSERVED/owner-review，不能称任意恢复一致。reader后display其它出生/post→Q07正式DAT/角色图迁移；资源未部署、schema15/23/26/2/2、raw47/3与所有用户例外/stage.dat USER_HOLD保持。Scene dirtyfalse/root14/用户HUDBg x30/SHA bcd1047b…保持，禁止computer-use/非战斗/Unity-GAS/Server改动。总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE。

以下为历史恢复检查点，状态由以上当前游标替代：

> 当前实施 `NTSD28-Q06-KIND3-CATCH-NATIVE-FRAME-LOOKUP-001` IN_PROGRESS / SOURCE_FIRST，kind3 actual与HitPlan都仍旧HasFrame；成对Native descriptor先检后写，准确五脚本。其它direct/CPoint/throw/pickup旧reader未自动迁移，C25已关职责不重做。

> **当前唯一恢复游标（2026-09-14 04:00Z）：** NATIVE-FRAME-FULL-DRIVER-SOURCE-WITNESS与UNITY均已限定VERIFIED，父NATIVE-FRAME-TRANSACTION-INTEGRATION也已VERIFIED / OFFLINE_NATIVE_FRAME_TRANSACTION_SCOPE。原450/1350，Unity20组1800/5400零差异，local/fresh-transfer replay8组32/64及真实Play224/672、Scene checksum/Renderer2→2、有序关闭全0/两帧Stopped通过。生产本轮未改，最新同生产SelfCheck仍03:22:37Z PASS（未重跑）。最终CS0/Editor idle、Scene dirtyfalse/root14/SHA bcd1047b…保持。没有运行中的test或exec，不再等待旧job。
> **下一唯一Task：`NTSD28-Q06-NATIVE-FRAME-RUNTIME-READER-MIGRATION-001 / READY_REMAINING_LIVE_READER_MAP`。** 从当前真实caller继续剩余direct setter/input/命中/CPoint/held/生成Native查询迁移，优先核对LF2Entity.SetFrameTickDirect/SetFrameTickRawDirect/SetFrameTickImmediateRawDirect以及Cpoint raw reader；它们仍可见legacy getter，先追活入口和原函数，不能只凭grep全部改。C25帧body/声音/成本/prev078/fragment/lifecycle与已关闭源向量无需重做。该umbrella Task尚无通用Change Record，选定准确code-path后建立子Record，不把本轮测试Record扩成任意生产授权。
> **保留未修缺口：** SNAPSHOT-ALLOCATION-EPOCH-PRESERVATION-AUDIT-001 OBSERVED / RECOVERY_OWNER_REVIEW_REQUIRED。预分配目标World跨恢复的canonical epoch2/1有32原失败；完整checksum不覆盖该差异，不能称任意恢复已对齐。当前只证local及空World同历史转移，未改shared Kernel/Server或恢复政策。该formal恢复边界原S0已未实现，继续离线reader/display/资源工作，后续涉及恢复必须回访。
> 普通OPoint effect/continuation(type0/5+parent credit门)/defend及其他字段仍见OPoint-REMAINING-CONSUMER-AUDIT；reader后回display其余出生/post→Q07正式DAT/角色图迁移，资源未部署。schema15/23/26/2/2、raw47/3、所有用户例外及stage.dat USER_HOLD保持。用户HUDBg x30保留；禁止computer-use、非战斗/Unity-GAS框架改造。总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE。

以下为历史恢复记录，状态由以上当前游标替代：

> 新发现保留：`NTSD28-Q06-SNAPSHOT-ALLOCATION-EPOCH-PRESERVATION-AUDIT-001` OBSERVED / RECOVERY_OWNER_REVIEW_REQUIRED；已使用目标World转移epoch2/1，原32差异不忽略。formal snapshot preservation原S0边界未实现；当前frame单测试只补同出生历史空World转移，不修改Server/epoch政策或声称所有恢复完成。

> 当前执行 `NTSD28-Q06-NATIVE-FRAME-FULL-DRIVER-UNITY-001` IN_PROGRESS / TEST_ONLY；source450/1350/934声音事件0frame/lifecycle错误、两次一致已VERIFIED_SOURCE_MODEL。先初始raw再三tick，保留raw47/3和C17边界，生产未改。

> 当前执行 `NTSD28-Q06-NATIVE-FRAME-FULL-DRIVER-SOURCE-WITNESS-001` IN_PROGRESS / SOURCE_ONLY，准确单CPP，450 case×3tick；Normal/Practice正式默认HP28=1/MP2c=1/drop4c=2、中性输入，上一轮六修复限定VERIFIED保持。

> **当前唯一恢复入口（2026-09-14 03:23Z）：** C25L-STATE18-SPAWN、LATE-OPOINT-DEPTH-AND-LIVES（generic基线限定）、OPOINT-TARGET-WORLD-INITIALIZATION、NATIVE-PENDING-PRE-C25-MOTION-GUARD、STATE18-RNG-EXCEPTION-FIXTURE、OPOINT-WEAPON-HP-BIRTH 六Record均已VERIFIED各自限定职责。原24回归终态22PASS/2FAIL仅weaponHp；修复后30/30通过含两个失败chunk5。最新full1514+未受影响direct1507合计3021向量0差异，分次证据明确记录；真实Play12+84+96、最终SelfCheck03:22:37Z PASS、有序关闭全0及两帧Stopped、CS0/Scene dirtyfalse/root14/hash bcd1047b…保持。没有运行中的Unity job或未收取exec，不再等待旧5a104/ce0f等job。
> **下一唯一Task：`NTSD28-Q06-NATIVE-FRAME-FULL-DRIVER-SOURCE-WITNESS-001 / READY_SOURCE_DRIVER_JOIN`**，服务于仍IN_PROGRESS的父NATIVE-FRAME-TRANSACTION-INTEGRATION：复用2676端点和既有state18/fragment完整组合成果，补高动作/成本回退连续完整driver、sound/previous078/lifecycle及真实Play证据；不得重做已关闭载体/碎片/本六修复。先核对正式GameSession options（尤其resource mode28=1）和现有FrameCase，准确新source CPP Record后才写。上一轮宣布高动作见证时因真实weaponHp差异暂停，尚未写这个runner。
> 已纠正旧归因：原276 non-RNG=252 pending位置+24普通OPoint weaponHp；独立C17例外546=273稀疏×两mode，未修改随机掉落生产。新增源210 witness明确kind2 positive hp覆盖weapon_hp；普通OPoint effect/continuation(type0/5+parent credit gate)/defend等仍在OPoint-REMAINING-CONSUMER-AUDIT（父frame后）中，不把generic1/0/0写成最终全部生成字段已对齐。
> 父frame完成后自动返回原frame reader/input/碰撞/held/生成余项→display其余出生/post→Q07正式DAT/角色图迁移；资源尚未部署、schema15/23/26/2/2、raw47/3、所有用户例外及stage.dat USER_HOLD保持。用户HUDBg x30保留；禁止computer-use、非战斗或Unity-GAS框架改造。总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE。

以下为历史恢复检查点，状态被上面的当前游标替代：

> **纠正旧276条归因：** 旧24 full chunk中276 non-RNG条目包含252 pending位置条目及24普通OPoint weaponHp=0/17条目；此前声称276全为pending不准确。新回归chunk5已复现weaponHp，不能关闭父包。当前必要子项 `NTSD28-Q06-OPOINT-WEAPON-HP-BIRTH-001` IN_PROGRESS，先原birth/kind2完整字段见证（Tools CPP），待同一Unity job终态后再写Unity；高动作完整driver见证延后到这项闭合后，未创建其脚本。

> **当前唯一执行游标（2026-09-14 03:00Z后）：** 正在回跑原24个失败完整tick组，同一Unity job `5a104f86aa1147a3921e27253a5fcc8f`，共1514向量。不得重复启动/编辑正在执行的脚本；首个64向量chunk零差异不是最终结果。旧82项终态FAILED及24旧chunk均已归档，不再等待旧job。桥接exec session49308已收取并以30秒观测timeout结束，不再poll该session；超时不等于测试失败，继续查询同一Unity job。
> `NTSD28-Q06-NATIVE-PENDING-PRE-C25-MOTION-GUARD-001`两生产入口与World exact gate已写：6/6 focused（84 pending资格+12原full-driver）、真实Play12逻辑夹具、Scene checksum保持/Renderer2→2、有序关闭World/slots/两pool全0且两帧Stopped、最终SelfCheck **02:59:21Z PASS**（请求02:58:40Z）。尚有源实体C25后位置/编码state/pending/code显式断言要在当前24组终态后补入同一pending测试脚本；现full-driver CompareChildren覆盖子粒子raw，并不等于源47字段全部比较，不能漏掉或误报。
> `NTSD28-Q06-STATE18-RNG-EXCEPTION-FIXTURE-001` IN_PROGRESS / TEST_ONLY：独立C17两profile/freeSlots -1/0/1/3共8/8 PASS，稀疏1次、密集0次，实体raw不变/NativeRandom零。据此full-tick只限定legacy 1/0并输出逐向量计数，直接C25仍0，native调用/raw47/生命周期/池断言均保留；生产随机掉落例外未改。546旧guard=273稀疏×两mode；276真实pending首差已由生产修复而非豁免。
> 三关联包C25L particles、late OPoint depth/lives、renderer targetWorld及上述两项仍IN_PROGRESS，待24终态/必要源断言最终证据再限定关闭，随后回父NATIVE-FRAME-TRANSACTION-INTEGRATION的完整C25出口。不要重做已验证源1550/96、direct1507或已关闭碎片。Q06/总目标ACTIVE；15/23/26/2/2、raw47/3、Q07正式资源尚未部署；用户HUDBg x30/Scene SHA BCD1047B…保持。禁止computer-use/非战斗/Unity-GAS框架改造。

> 当前实施 `NTSD28-Q06-NATIVE-PENDING-PRE-C25-MOTION-GUARD-001` IN_PROGRESS / TEST_FIRST，准确三脚本；motion/physics拒绝pending但保留独立dead normalization/C25，禁止computer-use及非战斗修改。

> **最终测试已结束（不再等待旧job）：** `aa6b0c9f033e4b8b83174826936e782c`终态FAILED，实际82项：58PASS/24FAIL，48/48 chunks、3021向量均执行，XML归final-regression-82.xml。直接出生两profile1507向量全0差异；完整tick失败含546条whole-tick legacy-RNG guard及276条真实raw首差。静态C17独立world.Rng用户例外要限定，不能抹除pending=1/code1101的C25前运动首差（原源不动，Unity多走velocity）。**下一唯一Task `NTSD28-Q06-NATIVE-PENDING-PRE-C25-MOTION-GUARD-001 / READY_FOR_CALLER_MAP_AND_EXACT_RECORD`**。先读完整chunk5/原pending向量，闭合motion/physics所有前置资格后针对失败输入重验，再限定RNG例外与回访C25L/最终SelfCheck/Play，不重复已验1550/96或重新开旧测试。
> C25L粒子、late OPoint depth/lives、renderer target World三个Record仍IN_PROGRESS，17 focused/正式192/最终Play96和关闭全0的限定证据保持，但不能升整体VERIFIED。Editor71188/6401，Temp旧bridge已清，内联代码BRIDGE.md/函数store可用；所有此前pending exec handles已结束，不再poll52994。源码/资源/Scene/Unity-GAS框架边界保持，总目标ACTIVE。

> **当前最优先恢复（验证仍运行）：** 同一Unity job `aa6b0c9f033e4b8b83174826936e782c`，已落盘46/48 chunks、2904/3021 vectors，508条whole-tick legacy-RNG guard和276条其他差异。**先等待/归档终态，不重启或修改运行中的脚本。** 已确认其他首差属于pending=1/code1101样例的C25前源运动未抑制（Source过滤346/348/352等；原不动，Unity多走3.25/-1.25/.75），下一必要Task `NTSD28-Q06-NATIVE-PENDING-PRE-C25-MOTION-GUARD-001`。不能把这些raw位置差异作为C17例外忽略。C17的world.Rng独立随机武器流是批准例外，另作限定与计数验证，不要求整tick legacy流零。
> 三实施Record仍IN_PROGRESS：C25L粒子、late OPoint depth/lives、renderer OPoint target World。它们已有17 focused、正式192及Play96/关闭全0，但大矩阵不通过，最终SelfCheck（最后World绑定后）待。当前Editor71188/bridge6401；内联代码见同artifact BRIDGE.md，functions store unityBridgeInline；Temp/Goal13_bridge.py不存在。最后桥接查询exec session52994可能timeout，先收取后查同一job。总目标ACTIVE；当前schema15/23/26/2/2、raw47/3、正式资源未部署、用户Scene bcd1047b保持。

> **最新验证游标：** 同一job `aa6b0c9f033e4b8b83174826936e782c`仍待终态；当前40/48 chunks、2520/3021 vectors、497失败条目，最新批次还含其他差异（见current-test-progress和chunk原文），不能归并为legacy例外；此前返回组主要为full-tick `legacy RNG consumed`。**暂不改断言或重启测试。** 静态已定位C17 BattleRandomWeaponDropModule.RunNormalDrop:65在weaponCount<4时调用world.Rng.NextInt(0,200)；总表R-04/W-07/Unity独有F明确这是用户批准例外。直接C25L phase0两profile已完成1507向量0差异，证明C25L本身未消费legacy流。待job结束核对全部失败是否仅此，并用调用链/定向测量限定例外；full-tick不能笼统要求整个legacy流为0，也不能删除未知下游差异。需要调整时只约束isolated C25并保留full-tick NativeRandom/raw和例外计数证据，再重跑必要失败组。
> 桥接恢复代码已写artifacts/diagnostics/NTSD28-Q06-C25L-STATE18-SPAWN-TRANSACTION-001/BRIDGE.md；functions store `unityBridgeInline`可直接用。其余code/17/Play96/关闭成果及三Record仍IN_PROGRESS，最终SelfCheck/Scene/Ledger还待。

> **当前执行检查点（2026-09-14）：** C25L-STATE18-SPAWN-TRANSACTION、LATE-OPOINT-DEPTH-AND-LIVES、OPOINT-TARGET-WORLD-INITIALIZATION三个Record仍IN_PROGRESS；代码已写，17 focused/正式192端点、SelfCheck（World绑定修复前）及最终正式999两factory完整tick Play96、借用2→2/Scene checksum/关闭全0与两帧Stopped有据。Play先发现普通OPoint未绑定目标World，已修；随后8个空闲Renderer不足人工峰值24，probe用现有诊断API预热到26后96PASS，未改slot400/配置/资产。
> **下一唯一动作：继续等待同一Unity测试job `aa6b0c9f033e4b8b83174826936e782c`，不得重复启动。** 当前已落盘26/48组、1635/3021向量、44差异（最新chunk已出现，待定位），尚无最终XML。选择器含State18新全类（48分组+其余7）、DepthLives6、旧C25L owner、旧SpawnVitals及NativeWeaponPiece。终态后保存Temp/Goal18_LastTestResults.xml、核对实际执行数（不是7540 discovery）、汇总48 chunk；再跑最终完整SelfCheck、Scene hash/CS0/Ledger/diff-check，才决定三项限定关闭及回父Frame。
> **环境恢复：** 外部Editor退出/重开，原PID58092与Temp/Goal13_bridge.py消失；当前同项目PID71188、MCP6401。内联桥接代码保存在functions store `unityBridgeInline`，取出后用PowerShell here-string管道给Python `- <command> '<json>'`；不要依赖已不存在Temp脚本。旧RED三矩阵有文件，第四被中断，未计通过。当前一个get_test_job桥接exec session52994可能返回timeout，先poll该session再查原job，timeout不是终态。Native原formal/synthetic证据在artifacts，部分Temp binary已由外部重启清掉，不需因此重做源见证。
> schema15/23/26/2/2、raw47/3、正式资源未迁移、Scene用户bcd1047b…保持。外部HEAD现2afa54d7，之前部分文件已入基线，代理未commit/push/reset；只保留当前差量。总目标ACTIVE，禁止computer-use/非战斗/Unity-GAS框架改造。

> 当前必要子修复 `NTSD28-Q06-OPOINT-TARGET-WORLD-INITIALIZATION-001` IN_PROGRESS，Play renderer组合错误定义/生命周期，保留无World配置的兼容预览fallback；父C25L/深度lives仍待联验。

> 当前必要子修复 `NTSD28-Q06-LATE-OPOINT-DEPTH-AND-LIVES-001` IN_PROGRESS / TEST_FIRST；源整数Z+point.Z+1及1/0/0出生计数，父C25L仍CODE_WRITTEN/FOCUSED_PARTIAL，禁止computer-use。

> 当前实施 `NTSD28-Q06-C25L-STATE18-SPAWN-TRANSACTION-001` IN_PROGRESS / TEST_FIRST，准确五脚本，Native RNG/generic birth/即时slot及flush；B8动态delay保持独立，禁止computer-use。

> **当前恢复游标（2026-09-14）：** C25L-STATE18-SPAWN-SOURCE-WITNESS已VERIFIED_SOURCE_MODEL_ONLY，1550行775出生+775完整driver、12759检查，重复字节相同；18条预设pending/无定义动作motion诊断留证，frame/lifecycle错误0。原证mixed OPoint slot50→state18 slots51..57→weapon pieces，源20/70顺序；generic HP/MP500/owner-1/team0/right、精确/整数位置分离；seed2/7/12/17/22/24覆盖持续1粒。**F08 Unity仍旧Match.Rng/普通OPoint出生，尚未修。下一唯一Task `NTSD28-Q06-C25L-STATE18-SPAWN-TRANSACTION-001 / READY_UNITY_RED_AND_EXACT_RECORD`**，保留已正确C25L owner与state13/200，只闭合剩余生成事务；之后回父Frame联合/reader/display-post/Q07。本轮仅一native诊断CPP，未新增Unity/Play结论，前碎片20/12/SelfCheck/173Play成果保持；schema15/23/26/2/2、raw47/3、资源未迁移、用户Scene bcd1047b…保持，禁止computer-use/非战斗/框架改动，总目标ACTIVE。

> 当前执行 `NTSD28-Q06-C25L-STATE18-SPAWN-SOURCE-WITNESS-001` IN_PROGRESS，F08原生成/RNG/身份及C25组合见证；不是重做owner位置，Unity未改，禁止computer-use。

> **当前恢复游标（2026-09-14）：** WEAPON-PIECE-SPAWN-ADMISSION-EDGE及父NATIVE-WEAPON-PIECE-TRANSACTION均已VERIFIED / WEAPON_PIECE_SCOPE_ONLY。三生产+一测试脚本：native OID0专用准入、共用动作预检、Renderer factory在Init前用现有pool API绑定目标World；普通OPoint保持。原1800/Unity5152差异0、20/20及binding后12/12、完整SelfCheck PASS、Play172+Renderer耗尽1、旧Scene两route四例0/5/0/5和4→4、关闭World/slot/两pool全0及两帧Stopped，已退出Play。两个Play失败及修正有留证。**下一唯一Task `NTSD28-Q06-NATIVE-FRAME-TRANSACTION-INTEGRATION-001 / READY_FINAL_C25_CALLER_AND_TRACE_JOIN`**，返回父组合顺序及剩余出口联验，不重做已验2676/core/carrier/death/fragment；其后reader余项/display-post/Q07。15/23/26/2/2、raw47/3、正式资源未迁移，Q06/总目标ACTIVE。用户HUDBg x30/Scene bcd1047b…保留，禁止computer-use/非战斗/框架修改；不是全B1-B12或正式图像完整对齐结论。

> 当前实施 `NTSD28-Q06-WEAPON-PIECE-SPAWN-ADMISSION-EDGE-001` IN_PROGRESS / TEST_FIRST，准确四脚本；OID0专用准入/共同动作校验/失败回收，普通OPoint保持，禁止computer-use。

> **当前恢复游标（2026-09-14）：** `NTSD28-Q06-WEAPON-PIECE-ADMISSION-SOURCE-WITNESS-001` 已 VERIFIED_SOURCE_MODEL_ONLY：900出生+900完整driver/17286断言、重复字节相同、frame/lifecycle/diagnostic错误0、最终构建无warning。原证OID0可生成、999需声明且高slot同tick删除/低slot保留、variant RNG先于缺catalog/无slot；两Unity factory统一oid<=0仍为待修差异。**下一唯一Task `NTSD28-Q06-WEAPON-PIECE-SPAWN-ADMISSION-EDGE-001 / READY_UNITY_RED_AND_EXACT_RECORD`**，源见证不重跑；核对Init/ModuleBind后准确Record、两factory/失败回收/高低slot动态验收，再回父frame/reader/display-post/Q07。前type2 fixture4/2592及完整SelfCheck PASS保持，本轮仅一个诊断CPP，无新Unity Play结论；用户HUDBg x30/Scene bcd1047b…保留，禁止computer-use及非战斗/框架修改。15/23/26/2/2、raw47/3、Q07正式资源未迁移，Q06/总目标ACTIVE。

> 当前执行 `NTSD28-Q06-WEAPON-PIECE-ADMISSION-SOURCE-WITNESS-001` IN_PROGRESS / SOURCE_MODEL_ONLY；原生成准入及高低slot完整driver见证，Unity未改，禁止computer-use。

> **当前恢复游标（2026-09-14）：** TYPE2-LANDING-FACING-SOURCE-WITNESS和FIXTURE均已限定VERIFIED：原648 physics+648 full tick，648对朝向无差异；原报告场景right→left物理flip在C25保留。Unity两外壳×两端点4组/2592比较全PASS，旧heavyBounce自检已按双初始朝向修正，**完整BattleRuntimeSelfCheck新鲜PASS**，生产未修改。前death prelude退休/STATE9998/64联合成果保持，不恢复旧state2000按vx覆盖。**下一唯一Task `NTSD28-Q06-WEAPON-PIECE-SPAWN-ADMISSION-EDGE-001 / READY_SOURCE_WITNESS_AND_CALLER_MAP`**：先原函数证OID0/声明或缺失999/非法初始动作/失败RNG及高低slot完整driver参与，再准确Record成组实现，不能把既有157+3或单个条件放宽当完整fragment。之后回父frame/其余reader/display-post与Q07资源。15/23/26/2/2、raw47/3；资源未迁移，Q06/总目标ACTIVE。用户HUDBg x30/Scene bcd1047b…保持，禁止computer-use、非战斗/Unity-GAS框架改动。证据见type2两个REPORT；完整自检PASS不等于全部对齐。

> 当前执行 NTSD28-Q06-TYPE2-LANDING-FACING-SOURCE-WITNESS-001 IN_PROGRESS；原type2物理明确翻面，完整tick保持性待实测，Unity未改。禁止computer-use。

> **当前恢复游标（2026-09-14）：** C25-EXTRA-DEATH-PRELUDE-RETIREMENT已 **VERIFIED / EXTRA_PRELUDE_REMOVAL_ONLY**：准确八脚本退休额外death/bounce/drop及hook，真实hit/physics/WPoint保持；6480原函数区分3240frame端点与3240完整tick，四配置各3240向量及RNG0差异，最终64/64 PASS，独立目标SelfCheck与真实Scene HP0+kind2持有C25保持frame0/Y/Vy0/links1,-1、4→4 checksum及关闭全0/两帧Stopped通过。两个clock/lifecycle fixture Record限定VERIFIED；STATE9998 retirement此前48次上游差异已消除，恢复限定VERIFIED。**完整SelfCheck仍FAIL，下一唯一Task `NTSD28-Q06-TYPE2-LANDING-FACING-AUDIT-001 / READY_SOURCE_PHYSICS_AND_C25_WITNESS`**：type2高速落地实际left与旧state2000强制right期望，先原函数/fixture调用链，不恢复旧行为迁就；随后fragment OID0/999准入及slot完整driver、父frame/其它reader/display-post和Q07资源。15/23/26/2/2、raw47/3，资源未迁移；Q06/总目标ACTIVE。用户HUDBg x30/Scene bcd1047b…保持，禁止computer-use、非战斗及Unity/GAS框架改动；旧死亡前置不可恢复。证据见death-prelude REPORT。

> 当前执行 NTSD28-Q06-C25-EXTRA-DEATH-PRELUDE-RETIREMENT-001 IN_PROGRESS / TEST_FIRST，准确八脚本；6480原函数区分frame端点与完整driver，退休额外C25死亡前置与hook，真实hit/physics/WPoint保持。禁止computer-use/非战斗改动。

> **当前恢复游标（2026-09-14）：** GT08 fixture已VERIFIED_TEST_ONLY（17/17及完整SelfCheck越过），STATE9998-SOURCE-DRIVER-WITNESS已VERIFIED_SOURCE_MODEL（224场景672完整tick全存活）。**STATE9998-LEGACY-CLEANUP-RETIREMENT为 COMPILE_PASS / FOCUSED_PARTIAL / SCOPED_PLAY_PASS**：准确五脚本，移除Serial末尾额外9998删除、其余职责/顺序不动；224场景删除差异0，联合28/29、最终6/7，保留type0 HP0共48次动作差异，beforeSerial已186。真实Scene当前descriptor9998经Serial存活/checksum恢复4→4/关闭全0及两帧Stopped通过。**下一唯一Task `NTSD28-Q06-C25-DEAD-CHARACTER-EXTRA-BOUNCE-AUDIT-001 / READY_SOURCE_CALLER_MAPPING`**，先查C25额外death bounce与held关系，不直接删真实hit/physics反应；随后核验完整SelfCheck最新type2落地方向FAIL（已过GT08/GT09），再回fragment OID0/999准入及完整driver/父frame。15/23/26/2/2、raw47/3保持；Q07资源未迁移，Q06/总目标ACTIVE。用户HUDBg x30/Scene bcd1047b…保持，禁止computer-use及非战斗/Unity-GAS框架改动。证据见两个STATE9998 REPORT；不得把6/7或限定Play改写为完整对齐。

> 当前执行 NTSD28-Q06-STATE9998-LEGACY-CLEANUP-RETIREMENT-001 IN_PROGRESS / TEST_FIRST；原完整driver224/672全存活，移除Unity额外9998删除须先RED并保留其余Serial职责。GT08 17/17及完整SelfCheck已越过，后续landing matrix有独立首差异。禁止computer-use/非战斗修改。

> 当前执行 NTSD28-Q06-GT08-LIFECYCLE-FIXTURE-REBASELINE-001 IN_PROGRESS，准确两测试脚本；GT08改实际producer及Native state独立断言，GT09 state9998原链缺证另建见证，不沿用旧权威。禁止computer-use。

> **当前恢复游标（2026-09-14）：** 原函数WEAPON-PIECE-SOURCE-WITNESS已VERIFIED；NATIVE-WEAPON-PIECE-TRANSACTION为 **FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / FULL_TRANSACTION_INCOMPLETE**。六脚本两阶段及两个factory专用出生已写，两profile各157/762片与实际3/50片0差异、49联合通过；真实旧内容Scene DataOriented完整Late pass四向量（两factory×healthy0片/broken5片）通过，每次checksum恢复4→4，关闭全0/两帧Stopped。RAW-OBJECT-TYPE-PROJECTION与三个fixture Record已限定VERIFIED。**完整SelfCheck仍FAIL，已越过武器/LC02/GT07，下一唯一Task `NTSD28-Q06-GT08-LIFECYCLE-FIXTURE-REBASELINE-001 / READY_SOURCE_AND_FIXTURE_MAPPING`**：旧1299→HitStun=-199与mock/Native pending须成组核验；随后fragment OID0/999/非法动作/pool失败/高低slot完整driver边缘，再回父FRAME-TRANSACTION。15/23/26/2/2、raw47/3，Q07正式资源未迁移；Q06/总目标ACTIVE。用户已确认HUDBg x30归本人/其他任务，Scene bcd1047b…保持。禁止computer-use、非战斗或Unity/GAS框架改动。证据详见fragment REPORT；先前SELF_CHECK_PENDING_WEAPON_PIECES被本游标更新，不能沿用“producer未写”。

> 当前执行 NTSD28-Q06-NATIVE-WEAPON-PIECE-TRANSACTION-001 / IN_PROGRESS / TEST_FIRST；原函数见证VERIFIED（157+3例/762+50片），准确六脚本两生成阶段及两factory出生适配。完整SelfCheck仍FAIL，禁止只改断言。用户确认HUDBg x30为本人/其他任务修改，保留。禁止computer-use。

> 当前执行 NTSD28-Q06-WEAPON-PIECE-SOURCE-WITNESS-001 / IN_PROGRESS / SOURCE_MODEL_DIAGNOSTIC_ONLY；一个native runner验证两种碎片的RNG/slot/完整出生规则，Unity未改，父碎片实现继续。禁止computer-use。

> **当前恢复游标（2026-09-14）：** LIFECYCLE-STATE-CARRIER-001为 **FOCUSED_TEST_PASS / SCOPED_PLAY_PASS**；父FRAME-TRANSACTION-INTEGRATION仍IN_PROGRESS。三字段/copy/checksum/raw及C25尾部接线已写，当前 **15/23/26/2/2、raw47/3**；2676×22两端点/实际Late两路径、41 focused、386/386联合、88工具、真实Scene encoded恢复4→4/关闭全0两帧Stopped通过。Module已改，旧857/HitStun消费者已移除，private shadow已移除。**完整SelfCheck FAIL（旧PendingFlushDestroy断言且OID100无碎片旧假设），碎片producer确实未实现；不能只改断言变绿。下一唯一Task `NTSD28-Q06-NATIVE-WEAPON-PIECE-TRANSACTION-001 / READY_SOURCE_WITNESS_AND_EXACT_RECORD`**，完整内置+DAT生成/slot/RNG/出生及SelfCheck native fixture后返回父验收。原始FAIL/invalid raw原因与修复都留证；Scene用户bcd1047b…保持，CS0/dirtyfalse/root14。总目标ACTIVE，禁止computer-use/非战斗改动。

> 当前执行 NTSD28-Q06-NATIVE-LIFECYCLE-STATE-CARRIER-001 / IN_PROGRESS / TEST_FIRST，三字段及15/23/26/2/2、raw47/3联合迁移未发布；父frame紧接着消费，不保留private影子。禁止computer-use。

> **当前恢复游标（2026-09-14）：** `NTSD28-Q06-NATIVE-FRAME-TRANSACTION-INTEGRATION-001` **IN_PROGRESS / CORE_FOCUSED_PASS / FULL_TRANSACTION_INCOMPLETE**。正式C25 core已写，2676例×14字段由9103差异降到0；core9+相关19最终28/28通过，独立C25-RENDER-PHASE-20-FIXTURE已VERIFIED_TEST_ONLY。**Module尚未修改，仍旧857/HitStun消费，不能做完整高位Play或发布完整frame对齐。下一必要Task `NTSD28-Q06-NATIVE-LIFECYCLE-STATE-CARRIER-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**：runtime_state_code与render_phase分离，建立pending/code持久真值并替换当前未接线private结果，随后返回本事务按OPoint/state18/078/weapon pieces/lifecycle顺序闭合。两生产+新测试已变，未跑本轮完整SelfCheck/Play；14/22/25/2/2保持，Scene用户bcd1047b…保持。总目标/Q06 ACTIVE，禁止computer-use/非战斗改动。

> 当前执行 NTSD28-Q06-NATIVE-FRAME-TRANSACTION-INTEGRATION-001 / IN_PROGRESS / TEST_FIRST；准确四脚本先Unity原2676 core RED与C25成组接线，完整driver/particles/weapon pieces等出口未完不关闭。14/22/25/2/2保持，禁止computer-use。

> **当前恢复游标（2026-09-14）：** `NTSD28-Q06-NATIVE-SOUND-LATCH-CARRIER-001` 已 VERIFIED / SOUND_LATCH_CARRIER_AND_SCHEMA_ONLY：独立NativeSoundActionLatch默认/reset -1、canonical copy/ECS fingerprint/完整checksum/full parity，联合版本 **14/22/25/2/2** 已344/344、完整SelfCheck、真实Scene sound857/frame1分离保存/两checksum恢复4→4及关闭全0/两帧Stopped验证。工具88/88，新鲜native/Unity content与五版本头一致，原raw50仍44相等/6MISSING（新sound latch不在该raw表）。准确22脚本，Scene用户HUDBg x30/bcd1047b…保持，CS0/dirtyfalse/root14，无新增缺失。**下一唯一Task `NTSD28-Q06-NATIVE-FRAME-TRANSACTION-INTEGRATION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**：数据前置已备，直接依据2676矩阵和CALLER-MAP做Unity RED/成组frame、成本、终止及声音事件接线，定义/fusion的latch重置待；实际WAV播放由Q10回访，不形成循环。Q06/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式资源未迁移；禁止computer-use/非战斗改动。

> 当前执行 NTSD28-Q06-NATIVE-SOUND-LATCH-CARRIER-001 / IN_PROGRESS / TEST_FIRST，Runtime独立声音latch数据前置及entity14/aggregate22/checksum25未发布联合迁移；shell2/2不变，producer仍由父帧事务后继。禁止computer-use。

> **当前恢复游标（2026-09-14）：** `NTSD28-Q06-NATIVE-FRAME-STEP-LIFECYCLE-SOURCE-WITNESS-001` 已 VERIFIED / SOURCE_MODEL_DIAGNOSTIC_ONLY：仅一native诊断脚本，真实World step_frame_slot/resolve_pending_lifecycle完成2676向量，复跑逐字节一致；正式EXE/75源码身份保持。Unity生产本轮未改。**下一唯一Task `NTSD28-Q06-NATIVE-FRAME-TRANSACTION-INTEGRATION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**：读取CALLER-MAP与矩阵，先完整数据/行为Record；正负999/YReference、原destination负HP/MP成本及fallback、212 stayed、encoded reset的latch/collision镜像、sound独立latch及完整driver tail必须闭合。禁止仅替换857或把两个端点见证当完整driver。前快照绑定244/SelfCheck/Play成果保持，用户Scene bcd1047b…保持；Q06/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式资源未迁移，禁止computer-use/非战斗改动。

> 当前执行 NTSD28-Q06-NATIVE-FRAME-STEP-LIFECYCLE-SOURCE-WITNESS-001 / IN_PROGRESS / SOURCE_MODEL_DIAGNOSTIC_ONLY；仅一native诊断脚本，先完整帧成本/生命周期原函数向量，Unity生产不变。禁止computer-use。

> **当前恢复游标（2026-09-14）：** `NTSD28-Q06-NATIVE-FRAME-SNAPSHOT-BINDING-001` 已 VERIFIED / SNAPSHOT_NATIVE_DESCRIPTOR_BINDING_ONLY：准确三脚本，Native current/collision descriptor原地与跨World恢复；20项RED16失败→20/20，联合244/244、完整SelfCheck、真实Scene857/998两份checksum恢复4→4及关闭全0/两帧Stopped通过。13/21/24/2/2不变，未证明高位动作完整tick。**下一唯一Task `NTSD28-Q06-NATIVE-FRAME-RUNTIME-READER-MIGRATION-001 / READY_FRAME_STEP_LIFECYCLE_MAPPING`**：frame绑定/direct/next/cost/terminal必须成组，source数据声明999可读但C25存活<999，负next先翻面再999解析；先完整调用表与原函数见证，不能批量857→1000。父零帧reader、其余display出生、post、Q07正式资源均未完。Scene用户HUDBg x30/bcd1047b…保持，CS0/dirtyfalse/root14，无新增缺失。总目标/Q06 ACTIVE/FULL_ALIGNMENT_INCOMPLETE；禁止computer-use与非战斗/Unity-GAS框架改动。

> 当前实施 NTSD28-Q06-NATIVE-FRAME-SNAPSHOT-BINDING-001 / IN_PROGRESS / TEST_FIRST，准确三脚本快照帧绑定；数据声明999可读与C25存活<999是不同合同，frame推进/终止成组后继，禁止computer-use/非战斗改动。

> **当前恢复游标（2026-09-14）：** `NTSD28-Q06-NATIVE-FRAME-ACCESSOR-RESOURCE-ADMISSION-001` 已 VERIFIED / NATIVE_ACCESSOR_AND_RESOURCE_OWNER_ADMISSION_ONLY：独立Native查询支持0..998零帧/声明999，旧Get/Has/Max857保持；仅资源资格/chp/cmp迁移。原函数81行（63合法/18错误AST）、223/223＋独立旧接口1/1、完整SelfCheck、实际资源owner两入口857/998/999六例/恢复4→4/关闭全0及两帧Stopped通过。INVALID-FRAME-FIXTURE已VERIFIED_TEST_ONLY；HP/MP两个Record恢复限定资源事务VERIFIED，但不能扩大为高位动作完整tick。**下一唯一Task `NTSD28-Q06-NATIVE-FRAME-RUNTIME-READER-MIGRATION-001 / READY_FOR_LIVE_CALLER_MAPPING`**：先实际frame绑定/direct/next/快照读取，逐组迁移其它live reader；父ZERO-FRAME-CACHE-CONTRACT、完整display与post均未完。原388检索清单为改前，406条最新候选见reader-inventory-after-accessor.json，不全是live调用方。Scene用户HUDBg x30/bcd1047b…保持，CS0/dirtyfalse/root14，资源未迁移；13/21/24/2/2与所有例外不变，总目标/Q06 ACTIVE/FULL_ALIGNMENT_INCOMPLETE，禁止computer-use/非战斗改动。

> 当前实施 NTSD28-Q06-NATIVE-FRAME-ACCESSOR-RESOURCE-ADMISSION-001 / IN_PROGRESS / TEST_FIRST，四脚本新增Native访问器并修资源读取；旧API保持，全部reader迁移/父零帧任务未完。禁止computer-use。

> **当前恢复游标（2026-09-14）：** `NTSD28-Q06-OPOINT-SPAWN-VITALS-TRANSACTION-001` 已 VERIFIED / OPOINT_VITALS_AND_DISPLAY_BIRTH_ONLY：3716 native、329实际Logan正OID（0按原规则跳过）、212/212、完整SelfCheck、两完整生成路径各两次真实Play/原模式与checksum恢复4→4/关闭全0及两帧Stopped通过。五脚本、仅三生产文件；正式资源/Scene不变，用户HUDBg x30与bcd1047b…保持，CS0/dirtyfalse/root14。**下一唯一Task `NTSD28-Q06-NATIVE-ZERO-FRAME-CACHE-CONTRACT-001 / READY_READONLY`**：native0..998隐式零帧与Unity缓存857/HasFrame声明判定不同；HP、MP两个资源Record已降为 FOCUSED_TEST_PASS / REOPENED_ZERO_FRAME_QUALIFICATION，原有效声明帧公式/phase/Play证据保留。先闭合该reader合同和必要修复，再回DISPLAY-PROGRESSION补非OPoint出生初值/联验，随后POST-DISPLAY。父display/Q06/总目标仍ACTIVE / FULL_ALIGNMENT_INCOMPLETE；13/21/24/2/2、Unity/GAS/非战斗/例外保持，禁止computer-use。

> 当前实施 NTSD28-Q06-OPOINT-SPAWN-VITALS-TRANSACTION-001 / IN_PROGRESS / TEST_FIRST，准确五脚本。新增零帧reader差异须独立追踪，不能将旧HP/MP有效帧通过推广；父display出生仍待，禁止computer-use。

> **当前恢复游标（2026-09-14）：** `NTSD28-Q06-NATIVE-DISPLAY-PROGRESSION-001` 仍 IN_PROGRESS，当前递推/slot阶段 FOCUSED_TEST_PASS / SCOPED_PLAY_PASS：新13测试含980 native全部通过，相关181不同测试经177PASS/4旧FAIL＋独立6PASS逐项闭合，完整SelfCheck/真实两tick显示与source真值保持/checksum恢复4→4/关闭全0及两帧Stopped通过。独立C25-POISON-PHASE-FIXTURE、C25-DISPLAY-SCHEMA-FIXTURE均VERIFIED_TEST_ONLY。**出生初始化未完成，不能关闭完整display。下一唯一Task `NTSD28-Q06-OPOINT-SPAWN-VITALS-TRANSACTION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**：原OPoint hp/mp及ohp/omp选择/百分比与两生成入口先原子处理，再返回display补全部出生初值与联验，随后POST-DISPLAY（WAIT_DISPLAY_OWNER）。准确四脚本+两个独立测试修正，无非战斗/资源/Scene变更；用户HUDBg x30/Scene bcd1047b…保持，CS0/root14/dirtyfalse。13/21/24/2/2保持，R05/R07显示子条件PARTIAL_RETURN；Q06/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，禁止computer-use。

> 当前实施 NTSD28-Q06-NATIVE-DISPLAY-PROGRESSION-001 / IN_PROGRESS / TEST_FIRST，准确四脚本递推+slot适配；出生初值发现OPoint hp/mp/ohp/omp依赖，未处理前不能关闭完整display任务。禁止computer-use/非战斗改动。

> **当前恢复游标（2026-09-14）：** `NTSD28-Q06-DISPLAY-POST-DISPLAY-RESOURCE-AUDIT-001` 已 VERIFIED_AUDIT_ONLY；`NTSD28-Q06-DISPLAY-POST-DISPLAY-SOURCE-WITNESS-001` 已 VERIFIED / SOURCE_MODEL_DIAGNOSTIC_ONLY（980 display＋2379 post向量，直接stdout复跑逐字节一致，正式EXE及75源码/header身份复核）。确认生产C25缺display/post owner，且display需出生初值和独立slot资格；frame_0mp保留原frame状态、display先于limit等边界已测。**下一唯一Task `NTSD28-Q06-NATIVE-DISPLAY-PROGRESSION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**：先闭合现有出生/复用/clone与slot适配，再准确Record实施完整C25d；随后POST-DISPLAY-RESOURCE-TRANSACTION（WAIT_DISPLAY_OWNER）。本轮仅一新native诊断脚本，HP/MP六脚本hash和用户HUDBg x30/Scene bcd1047b保持，无新Unity/Play结论；前HP134/SelfCheck/Play成果保留。R05/R07待生产接通再回访，Q07正式资源未迁移，13/21/24/2/2保持，总目标/Q06 ACTIVE / FULL_ALIGNMENT_INCOMPLETE。禁止computer-use与非战斗改动。

> **当前恢复游标（2026-09-14）：** Q06 HP事务 `NTSD28-Q06-NATIVE-HP-RESOURCE-TRANSACTION-001` 已 VERIFIED / SCOPED_HP_TRANSACTION_AND_DEFAULT_PRODUCTION_PASS；134/134（含3768 HP及2028 MP原函数向量）、两profile真实Logan12tick、完整SelfCheck、旧内容真实Scene HP/恢复4→4/有序关闭全0与两帧Stopped通过。独立 `NTSD28-Q06-HP-SELF-CHECK-WORLD-CONTEXT-001` 已 VERIFIED_TEST_ONLY。正式默认mode28=1已接；下方旧计划mode0/未写/待验为历史。Scene用户HUDBg x30与bcd1047b…保持，CS0/dirtyfalse/root14，无新增缺失。**下一唯一Task `NTSD28-Q06-DISPLAY-POST-DISPLAY-RESOURCE-AUDIT-001 / READY_READONLY`**；先资源display/post-display消费链审计，再精确实施，Q08正式模式投影及Q07资源迁移仍待。13/21/24/2/2保持，Q06及总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE；禁止computer-use及非战斗改动。

> HP默认来源纠正：正式GameSession/scenario默认28=1，核心ResourceSystemRules孤立默认0不能替代playable入口；生产将传1。HP原版3768向量/13项RED11已取得，追加Logan12tick见证。

> 当前执行 NTSD28-Q06-NATIVE-HP-RESOURCE-TRANSACTION-001 / IN_PROGRESS / TEST_FIRST，准确五脚本；HP完整分支与共同资格、World phase12、不可变mode28默认0；保留MP成果及用户确认HUDBg x30/Scene bcd1047b…，禁止computer-use/非战斗改动。

> **2026-09-14最新确认与游标：** 用户已确认HUDBg x50→30为自己或其他任务修改，Scene bcd1047b…现状必须保留，旧SCENE_ORIGIN_PENDING标记已解除。NTSD28-Q06-NATIVE-MP-RESOURCE-TRANSACTION-001 已VERIFIED / SCOPED_MP_TRANSACTION_AND_DEFAULT_PRODUCTION_PASS（2028 native/119 tests/SelfCheck/实际MP及关闭全0，原数值首差闭合，只剩6MISSING）。下一唯一Task NTSD28-Q06-NATIVE-HP-RESOURCE-TRANSACTION-001 / READY_FOR_EXACT_PRECHANGE_RECORD；Q08正式mode投影和其他Q06/资源迁移仍待。禁止computer-use及非战斗改动，总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> **Q06 MP限定出口（2026-09-13）：** NTSD28-Q06-NATIVE-MP-RESOURCE-TRANSACTION-001 / FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / SCENE_ORIGIN_PENDING。完整MP事务及两caller已接，native2028向量、119/119、完整SelfCheck、真实Scene tick5→6默认MP200/显式mode0=201/weak抑制/恢复4→4/关闭全0通过。当前同源raw44已绑定字段一致，只剩原6MISSING，MP200/201差异已消除；NTSD28-Q06-DEFAULT-MP-RECOVERY-FIXTURE-CORRECTION-001已VERIFIED_TEST_ONLY，原34FAIL与SelfCheck旧断言留证。**Scene HUDBg x50→30，SHA bcd1047b…，15:38:24Z保存，早于Play；已异步询问来源，保留不回退，不能写Scene unchanged。** 下一唯一Task NTSD28-Q06-NATIVE-HP-RESOURCE-TRANSACTION-001 / READY_FOR_EXACT_PRECHANGE_RECORD，可继续独立HP工作。Q08正式mode注入仍待，13/21/24/2/2及所有例外保持，正式资源未迁移，总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，禁止computer-use及非战斗改动。

> 当前执行 NTSD28-Q06-NATIVE-MP-RESOURCE-TRANSACTION-001 / IN_PROGRESS / TEST_FIRST，准确五脚本；完整MP事务+两caller，显式不可变mode值/source默认1和F6输入，不加未校验World字段或版本。先native分支见证和RED，禁止computer-use及非战斗改动。

> **当前Q06入口（2026-09-13）：** NTSD28-Q06-RESOURCE-MP-FIRST-DIFFERENCE-AUDIT-001 已 VERIFIED_AUDIT_ONLY / CAUSE_CONFIRMED。当前正式EXE/runner及75源码-header hash已复核；四次native模式原值对照确认gate恰1抑制普通MP恢复，0/2/-1则tick3增加1。当前Unity新鲜capture 1/1 PASS且内容头完整同值，MP差异仍200/201；未修改脚本/资源，不能写成已修。下一唯一Task **NTSD28-Q06-NATIVE-MP-RESOURCE-TRANSACTION-001 / READY_FOR_EXACT_PRECHANGE_RECORD**：完整frame.cmp/regen族/阈值/bound/weak/F6/mode/限幅及两生产caller，明确不可变模式输入与Q08投影边界，不偷加未校验mutable字段/版本。Q05限定交付保持；BATCH-03/Q06 ACTIVE，总目标FULL_ALIGNMENT_INCOMPLETE，禁止computer-use及非战斗改动。

> **当前恢复游标（2026-09-13）：BATCH-02 / Q05 已限定交付，下一 BATCH-03 / Q06。** NTSD28-Q05-JOINT-SNAPSHOT-RESTORE-REPLAY-VALIDATION-001、NTSD28-Q05-SNAPSHOT-RETIRED-SHELL-POOL-RETURN-001、NTSD28-Q05-SNAPSHOT-RENDERER-REGISTRY-RETENTION-001 均 VERIFIED / SNAPSHOT_REPLAY_SCOPE_ONLY。实际Logan两profile24tick/22tick重放、slot/pool/错误identity、最终82/82、完整SelfCheck、两次旧内容真实Scene恢复4→4/关闭全0/两帧Stopped/重入通过；CS0，Scene旧SHA/root14/dirtyfalse。三Record共6脚本，仅2个生产snapshot文件，保留Unity/GAS/非战斗。13/21/24/2/2联合schema基线已验证，trace3/raw-source2/50字段保持；正式资源未迁移、六MISSING及MP200/201仍由Q06/后继解决。下一唯一Task **NTSD28-Q06-RESOURCE-MP-FIRST-DIFFERENCE-AUDIT-001 / READY_READONLY**。禁止computer-use；总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。以下较早启动语句仅历史，不应重开已验Q05。

> 当前必要修复 `NTSD28-Q05-SNAPSHOT-RENDERER-REGISTRY-RETENTION-001 / IN_PROGRESS / TEST_FIRST`：真实Scene两个Renderer计入ObjectCount但不占战斗槽，原地restore拒绝；准确三脚本保留原注册/活动计数分域。父Q05未关闭，pool修复保持，禁止computer-use。

# NTSD 当前权威恢复入口

> Q05真实恢复验证发现退休shell未归还pool；当前必要修复 `NTSD28-Q05-SNAPSHOT-RETIRED-SHELL-POOL-RETURN-001 / IN_PROGRESS / TEST_FIRST`，准确2脚本。移动回放两profile已过，攻击测试入口需改用既有logic-only executor；禁止computer-use，父验收继续。

> 当前执行 `NTSD28-Q05-JOINT-SNAPSHOT-RESTORE-REPLAY-VALIDATION-001 / IN_PROGRESS / VALIDATION_IMPLEMENTATION`，准确2个Editor脚本，真实Logan两profile/24tick恢复回放与pool/slot验收；生产规则保持，禁止computer-use，版本13/21/24/2/2未发布。

> **Q05 trace/raw身份与字段限定出口（2026-09-13）：** `NTSD28-Q05-TRACE-RAW-IDENTITY-JOINT-UPGRADE-001 / FOCUSED_TEST_PASS / SAME_CONTENT_CAPTURE_PASS / RAW_PARITY_DIFFERENT`。19脚本，trace v3/raw-source v2/50字段44绑定6MISSING；实际Logan输入native/Unity raw4EFE/semanticDB57/projection3900完整同值，native复跑字节相同。50不同Unity tests、88工具tests、native2F8非默认unit、完整SelfCheck、旧内容真实Play/零新增缺失通过；Scene旧SHA保持，禁止computer-use。已修诊断入口当前MP误作最大MP及首差排序，生产战斗规则未改。真实3tick比较仍7类差异：原6MISSING与tick3 currentMp native200/Unity201（Q06待追实际consumer）。**下一唯一Task `NTSD28-Q05-JOINT-SNAPSHOT-RESTORE-REPLAY-VALIDATION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**，完成同版本有意义的restore/replay/slot-pool/重入验收再关Q05；13/21/24/2/2仍未发布，Q05/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式资源未迁移。


> 当前执行 `NTSD28-Q05-TRACE-RAW-IDENTITY-JOINT-UPGRADE-001 / IN_PROGRESS / TEST_FIRST`，准确19脚本（含CLI退出码与独立native字段见证），真实内容根/语义头/trace v3/raw v2/50字段；13/21/24/2/2未发布，禁止computer-use，非战斗/正式资源保持。

> **Q05五版本及恢复头部已限定验证（2026-09-13）：** `NTSD28-Q05-JOINT-SNAPSHOT-CHECKSUM-VERSION-001 / FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / TRACE_IDENTITY_PENDING`。当前正式代码常量已为 **13/21/24/2/2**，仍INTERMEDIATE_UNPUBLISHED_Q05_WINDOW；修复外层有效而内层旧版仍可恢复的漏洞，复用原发布全子域header predicate。RED11+1→最终287不同测试有通过证据（主286PASS/1旧phaseFAIL经独立`NTSD28-Q05-WORLD-CLOCK-PHASE-FIXTURE-001`定向1PASS闭合），新增23全PASS、完整SelfCheck/CS0/真实暂停World双队列Play tick5对象4→4通过。`NTSD28-Q05-REMAINING-CONTENT-HASH-CONSUMER-AUDIT-001`已VERIFIED_AUDIT_ONLY：未发现额外frame/meta生产hash漏项，明确trace仍strategy-pending/缺完整语义头/49字段。**下一唯一Task `NTSD28-Q05-TRACE-RAW-IDENTITY-JOINT-UPGRADE-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**，同窗口trace v3/raw/source v2/50字段2F8及真实source/raw/decode/semantic/schema绑定，再完整replay/Play；Q05/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式资源未迁移。Scene旧SHA/Foot18既有缺失保持，禁止computer-use及非战斗改动。


> 当前执行 `NTSD28-Q05-JOINT-SNAPSHOT-CHECKSUM-VERSION-001 / IN_PROGRESS / TEST_FIRST / INTERMEDIATE_UNPUBLISHED`，准确15脚本，五版本目标13/21/24/2/2；hash消费者审计已闭合，trace身份/50字段后继，禁止computer-use。

> **Q05快照边界限定出口（2026-09-13）：** `NTSD28-Q05-OPOINT-SNAPSHOT-BOUNDARY-GUARD-001 / FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / JOINT_SCHEMA_PENDING`。11脚本统一完整Host/core/worker/kernel tick、structural及两OPoint owner前置；原body/pass保持，拒绝无队列/World/worker副作用。RED14与HostRED1→最终187/187、完整SelfCheck、真实暂停World tick5/对象4→4双队列拒绝与空闲capture通过；该Play无dedicated worker，不宣称物理技能或worker实战。独立`NTSD28-Q05-WORKER-LATE-OPOINT-FLOOR-FIXTURE-001 / VERIFIED_TEST_ONLY`按native落地修旧child45→5并断言parent0；首次175中174PASS/1旧FAIL留证。Scene旧SHA/Foot18缺失保持，无新增缺失，CS0。**下一唯一Task `NTSD28-Q05-REMAINING-CONTENT-HASH-CONSUMER-AUDIT-001 / READY_READONLY_CONTRACT`**，再联合13/21/24/2/2、trace/replay；当前12/20/23/1/1未发布，Q05/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式资源未迁移，禁止computer-use及非战斗改动。


> 当前执行 `NTSD28-Q05-OPOINT-SNAPSHOT-BOUNDARY-GUARD-001 / IN_PROGRESS / TEST_FIRST`，准确9脚本，统一tick/structural/双队列/host快照前置，拒绝无副作用；不改pass和关闭顺序，禁止computer-use，版本/trace后继。


> **Q05语义身份已接线并限定验证（2026-09-13）：** `NTSD28-Q05-SEMANTIC-CONTENT-IDENTITY-001 / FOCUSED_TEST_PASS / SCOPED_PUBLICATION_PLAY_PASS / JOINT_SCHEMA_PENDING`。raw DAT/visual算法保持，V2 tag+raw32 SHA256/LE ulong进入catalog/candidate/cache、两publisher和本地验证session；75主回归PASS、Visual旧类6PASS/1过期六DAT断言由`NTSD28-Q05-FORMAL-VISUAL-CANDIDATE-ADMISSION-FIXTURE-001 / VERIFIED_TEST_ONLY`定向1PASS闭合（82不同focused有通过证据），完整SelfCheck/独立Python hash通过。正式330/906输入capture成功；隔离native格式源实际menu重进cache1/三key同/World4/46资源全释放/borrower0/两帧Stopped通过，非正式330全渲染或整技能结论。**下一唯一Task `NTSD28-Q05-OPOINT-SNAPSHOT-BOUNDARY-GUARD-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**，继续父步骤3双OPoint guard，再相关内容hash/联合版本/trace/replay。当前12/20/23/1/1未发布，Q05/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式资源未迁移；禁止computer-use，Unity/GAS/非战斗、Scene旧SHA/Foot18缺失/例外保持。


> 当前执行 `NTSD28-Q05-SEMANTIC-CONTENT-IDENTITY-001 / IN_PROGRESS / TEST_FIRST`，准确7脚本接冻结semantic身份、现有cache/publication与本地验证；raw指纹/协议保持，双OPoint guard/版本/trace后继，禁止computer-use。


> **Q05 HolderCopy载体已清理，步骤2限定出口（2026-09-13）：** `NTSD28-Q05-HOLDERCOPY-CARRIER-RETIREMENT-001 / FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / JOINT_SCHEMA_PENDING`。36脚本/13生产仅runtime/Entity/task/ECS/校验/HitPlan旧载体删除，bit33退休空洞、真实关系保持；首次863=859PASS/4旧统计预期FAIL，独立`NTSD28-Q05-RETIRED-HOLDER-STATS-FIXTURE-CORRECTION-001 / VERIFIED_TEST_ONLY`经27中26PASS+最窄1PASS逐项闭合（保留Cpoint原nativeKO1）。完整SelfCheck与真实pickup/replacement/current OPoint tick/注销Play通过，去旧holderCopy后的有效见证前后完全一致/对象4→4。**下一唯一Task `NTSD28-Q05-CONTENT-IDENTITY-AND-CAPTURE-BOUNDARY-001 / READY_FOR_EXACT_PRECHANGE_RECORD`（父步骤3）**，再联合13/21/24/2/2及trace/回放；五类载体不重做。当前12/20/23/1/1未发布，Q05/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。禁止computer-use；Unity/GAS/非战斗、Scene旧SHA/Foot18缺失/例外保持，正式资源未迁移。


> 当前执行 `NTSD28-Q05-HOLDERCOPY-CARRIER-RETIREMENT-001 / IN_PROGRESS / TEST_FIRST`，准确36脚本，仅退休HolderCopy载体/task/ECS/HitPlan诊断；保留真实关系及其他mask位，禁止computer-use，identity/联合版本后继。


> **Q05 WeaponState载体已清理（2026-09-13）：** `NTSD28-Q05-WEAPONSTATE-CARRIER-RETIREMENT-001 / FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / JOINT_SCHEMA_PENDING`。五生产文件仅旧field/copy/reset/init/ECS hash/checksum/parity删除，真实frame.state/GetResolvedWeaponStateForExternalUse保持；282/282、完整SelfCheck、OID124/action40两次pre-frame Play前后有效观察一致，附带四释放用例PASS/对象4→4。**下一唯一Task `NTSD28-Q05-HOLDERCOPY-CARRIER-RETIREMENT-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**，五类退休载体只剩HolderCopy；之后identity/双OPoint guard/联合13/21/24/2/2/回放继续。当前12/20/23/1/1未发布中间态，Q05/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式资源未迁移。禁止computer-use；非战斗/Unity/GAS、Scene旧SHA/Foot18既有缺失/例外保持。


> 当前执行 `NTSD28-Q05-WEAPONSTATE-CARRIER-RETIREMENT-001 / IN_PROGRESS / TEST_FIRST`，准确10脚本，仅退休WeaponState载体；真实frame.state与有效状态接口保留，禁止computer-use，联合版本后继。


> **Q05 ReleaseTick载体已清理（2026-09-13）：** `NTSD28-Q05-RELEASETICK-CARRIER-RETIREMENT-001 / FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / JOINT_SCHEMA_PENDING`。runtime/copy/reset/ECS hash/checksum/parity及三个无效参数/全部caller已删，四生产文件仅参数变化。267/267、完整SelfCheck复跑、四例当前数据Play前后有效输出一致且对象4→4；首轮SelfCheck旧JSON断言遗漏已纠正并留证。**下一唯一Task `NTSD28-Q05-WEAPONSTATE-CARRIER-RETIREMENT-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**，再HolderCopy、identity/双OPoint guard、联合版本与回放；Q05/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。当前12/20/23/1/1未发布中间态，正式资源未迁移。禁止computer-use；Unity/GAS/非战斗、Scene旧SHA、Foot18既有缺失及例外保持。


> 当前执行 `NTSD28-Q05-RELEASETICK-CARRIER-RETIREMENT-001 / IN_PROGRESS / TEST_FIRST`，准确14脚本，仅退休ReleaseTick载体与无效参数；真实释放关系保持，禁止computer-use，联合版本后继。


> **Q05 GrabbedBy/TrackerFlag载体已清理（2026-09-13）：** `NTSD28-Q05-GRABBEDBY-TRACKERFLAG-CARRIER-RETIREMENT-001 / FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / JOINT_SCHEMA_PENDING`。runtime/Entity/ECS存储、copy/reset/init/自赋值/hash已删；407/SelfCheck/实际pickup-replacement/OPoint tick与unregister Play通过，真实TrackerParent及Owner/Spawner/2F8快照保持。**下一唯一Task `NTSD28-Q05-RELEASETICK-CARRIER-RETIREMENT-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**，后续WeaponState/HolderCopy及identity/OPoint guard/联合版本/回放继续；五类未全部完。Scene旧SHA/Foot18既有缺失保持，禁止computer-use；当前12/20/23/1/1未发布中间态，Q05/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> 当前执行 `NTSD28-Q05-GRABBEDBY-TRACKERFLAG-CARRIER-RETIREMENT-001 / IN_PROGRESS / TEST_FIRST`，五类退休字段内先两flag，保留TrackerParent/Owner/Spawner/2F8；剩余三类和联合版本后继，禁止computer-use。

> **Q05 Mass/Oscillate载体已清理（2026-09-13）：** `NTSD28-Q05-MASS-OSCILLATE-SHELL-CARRIER-RETIREMENT-001 / FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / JOINT_SCHEMA_PENDING`。20处Context参量/旧runtime、spec及两shell字段已移除；890/SelfCheck、实际driver运动与CentralOnly效果snapshot probe通过，三规则核心主体未变。首轮跨ID整tick错误假设已留证并修正夹具，未改规则。**下一唯一Task `NTSD28-Q05-FIVE-RESERVED-CARRIER-RETIREMENT-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**；2F8/raw恢复和内容来源成果保留。当前shell形状已变而版本仍1/1，只属同Q05未发布中间态，后续13/21/24/2/2、identity/OPoint guard/旧版本拒绝/回放/Play必须继续。Scene旧SHA和Foot18任务外缺失保留；禁止computer-use；总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> 当前执行 `NTSD28-Q05-MASS-OSCILLATE-SHELL-CARRIER-RETIREMENT-001 / IN_PROGRESS / TEST_FIRST / INTERMEDIATE_UNPUBLISHED`，准确21脚本，清mass/Oscillate存储与shell，保留Q04已验证行为；禁止computer-use，版本同窗口后继。

> **Q05独立+2F8载体及raw恢复已限定交付（2026-09-13）：** `NTSD28-Q05-OBJECT-AI-2F8-CARRIER-CONTRACT-001` 与 `NTSD28-Q05-UNCLAIMED-RAW-RUNTIME-RESTORE-001` 均FOCUSED_TEST_PASS，46/46/完整SelfCheck/CS0/无新缺失；ObjectAiExcludedGroupSourceSlot2F8独立int/-1、claimed/raw copy/reset/ECS/hash，已补未占用raw恢复漏项并验证旧字段/checksum。**下一唯一Task `NTSD28-Q05-MASS-OSCILLATE-SHELL-CARRIER-RETIREMENT-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**，再五reserved；父Q05步骤3身份/OPoint guard、步骤4统一13/21/24/2/2、步骤5回放/Play仍待。当前仍12/20/23/1/1且字段集合处于INTERMEDIATE_UNPUBLISHED，禁止发布/跨版本交换/Q07。+2F8 held writer/AI消费留Q06。Foot18既有缺失和Scene旧SHA保留；禁止computer-use；总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> +2F8联验发现未占用raw恢复漏项，必要子包`NTSD28-Q05-UNCLAIMED-RAW-RUNTIME-RESTORE-001 / IN_PROGRESS / TEST_FIRST`先闭合已有snapshot职责；2F8仍待，禁止computer-use。

> 当前执行 `NTSD28-Q05-OBJECT-AI-2F8-CARRIER-CONTRACT-001 / IN_PROGRESS / TEST_FIRST / INTERMEDIATE_UNPUBLISHED_Q05_WINDOW`，五脚本独立+2F8，保留Spawner/Owner，Q06消费及联合版本后继；禁止computer-use。

> **Q05护甲/碎片实际来源已交付（2026-09-13）：** `NTSD28-Q05-NATIVE-ARMOR-WEAPON-PIECE-SOURCE-INTEGRATION-001 / FOCUSED_TEST_PASS / VERIFIED_ARMOR_PIECE_SOURCE_ONLY`。39夹具/405语料/18armor/三piece与330实际metadata；1829不同测试/完整SelfCheck/CS0/旧138仅source身份差异，原双参数构造保留，Temp probe重建。Q05步骤1数据来源已限定完成，**下一唯一Task `NTSD28-Q05-RETIRED-CARRIER-AND-2F8-MIGRATION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`（父步骤2）**，再身份/联合schema/consumer/Play，不能直接跳Q07。任务外Foot18文件删除+blue/red/yellow新目录保留，Scene旧SHA保持；禁止computer-use。Q05/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，所有例外保持。

> **用户检测方式约束（2026-09-13）：禁止使用computer-use。** 后续Unity检测使用桥接接口、日志、测试结果、进程状态等方式，不做桌面自动化。

> 当前执行 `NTSD28-Q05-NATIVE-ARMOR-WEAPON-PIECE-SOURCE-INTEGRATION-001 / IN_PROGRESS / TEST_FIRST`，准确12脚本，先native armor/piece结构与typed witness；BMP/stats前包保持，consumer/identity后继。

> **Q05 BMP/stats实际来源已交付（2026-09-13）：** `NTSD28-Q05-NATIVE-BMP-STATS-SOURCE-INTEGRATION-001 / FOCUSED_TEST_PASS / VERIFIED_BMP_STATS_SOURCE_ONLY`。逐行原版BMP/global stats、动作计数/结束/同一行sheet与实际NativeMetadata接线；405语料/330catalog/28夹具、1390不同测试/完整SelfCheck/CS0、旧138零新投影差异。sentinel/cache与旧sprite准入测试更正均留证，附属`NTSD28-Q05-SPRITE-CORPUS-ADMISSION-FIXTURE-CORRECTION-001 / VERIFIED_TEST_ONLY`（402成功/3拒绝）。Scene旧精度差异SHA保持；正式资源、identity/schema/消费者和Play未完成。**下一唯一Task `NTSD28-Q05-NATIVE-ARMOR-WEAPON-PIECE-SOURCE-INTEGRATION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**，继续父metadata，不重做BMP/stats。Q05/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，例外保持。

> 当前执行 `NTSD28-Q05-NATIVE-BMP-STATS-SOURCE-INTEGRATION-001 / IN_PROGRESS / TEST_FIRST`，八脚本事前合同；先BMP/stats AST与实际NativeMetadata关联，保留frame/strength与legacy，armor/piece后继仍待。

> **Q05 metadata字段合同已交付（2026-09-13）：** `NTSD28-Q05-NATIVE-DEFINITION-FIELDSET-CONTRACT-001 / FOCUSED_TEST_PASS / VERIFIED_FIELDSET_MODEL_ONLY`。不可变ordered Bmp/Stats、Ordinal last-win、预解码int/double有效性及caller fallback已写，新增TryFinite64区分invalid/overflow与有效underflow/零。RED4→486PASS（新4+numeric43+typed439）、4045原版optional、完整SelfCheck PASS、CS0/dotnet0error、Ledger485/95PASS。**未接manager/metadata AST，520精度/3096默认等实际差异仍待。下一唯一Task `NTSD28-Q05-NATIVE-DEFINITION-AST-SOURCE-INTEGRATION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**，继续父metadata integration，不重做模型/数值；identity/schema/consumer/Play及例外保持，Q05/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> 当前执行 `NTSD28-Q05-NATIVE-DEFINITION-FIELDSET-CONTRACT-001 / IN_PROGRESS / TEST_FIRST / DATA_MODEL_ONLY`，先optional有效性和不可变字段合同，再父metadata AST/manager/piece接线；原值/资源/例外保持。

> **Q05 definition头部审计已交付（2026-09-13）：** `NTSD28-Q05-NATIVE-DEFINITION-HEADER-CONTRACT-AUDIT-001 / VERIFIED_AUDIT_ONLY / METADATA_GAPS_CONFIRMED`。330真实catalog构建后capture与fresh native同源对照：156角色520个float32精度差、172非角色3096个缺省值差（不冒称全是已复现战斗故障）；stats.max_mp158缺载体，weapon_piece三定义各3组4variant缺结构/载体，BMP shadow30/bound98待非例外Q09。18armor/1320sequence/990weapon sound一致，不重写。stats.y3属平台取景例外，smallb154属HUD、hidden/random各158属选择流程排除。capture1/1、CS0、Ledger484/92PASS，production/资源未改。**下一唯一Task `NTSD28-Q05-NATIVE-DEFINITION-METADATA-INTEGRATION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**，完成metadata native模型/解析接线后再Q05 identity/carrier/联合版本；前包帧内容与330可构建证据保持。Q05/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> 当前执行 `NTSD28-Q05-NATIVE-DEFINITION-HEADER-CONTRACT-AUDIT-001 / IN_PROGRESS / READONLY_CAPTURE`，已声明两个诊断脚本，核对真实metadata/精度/default/presence；帧内容已验证，不重做或更改production。

> **Q05-A2 native typed帧接线已验证（2026-09-13）：** `NTSD28-Q05-NATIVE-FRAME-TYPED-CONVERSION-001 / FOCUSED_TEST_PASS / VERIFIED_TYPED_FRAME_CONTENT_AND_CONSTRUCTION_ONLY`。405 DAT/55348声明frame完整40int+六double+27/24/40/9/geometry/ordered sound投影通过，330实际candidate全构建成功，原六文件九frame异常全部消除（不是角色runtime全对齐）。557不同focused有通过证据（555+2）、binary64 2567/原numeric14742回归、完整SelfCheck PASS、CS0/dotnet0error、旧138零新增投影差异、Ledger483/90PASS。**下一唯一Task `NTSD28-Q05-NATIVE-DEFINITION-HEADER-CONTRACT-AUDIT-001 / READY_READONLY_CONTRACT`**，核对尚未完整覆盖的BMP/stats/armor/weapon等definition域，再推进Q05 identity/carrier/联合版本。新增FrameSounds/profile/centerz/chp/cmp及六double须进入identity，Q06 motion/resource、Q09 centerz、Q10音频回访保持。Scene旧精度差异保护；Q05与总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式资源未迁移。

> 当前执行 `NTSD28-Q05-NATIVE-FRAME-TYPED-CONVERSION-001 / IN_PROGRESS / TEST_FIRST`；AST前置保持，现接37帧标量及27/24/40/9/geometry实际转换，先完整native typed witness与RED。Scene差异保护，identity/schema/Play后继。

> **Q05-A2 native frame AST已验证（2026-09-13）：** `NTSD28-Q05-NATIVE-FRAME-AST-ADMISSION-001 / FOCUSED_TEST_PASS / VERIFIED_FRAME_AST_ONLY`。405 DAT/55348声明frame原版有序结构双跑与Unity对照、37语法夹具通过；ssnk数字键7已修且actual manager成功，当前剩余**五文件八frame**。原始RED370含336换行编码假差异，已保留并更正为34实质RED；后续旧六失败与路径分隔符断言修正留证，当前518不同测试有通过证据（517+单独1）、完整SelfCheck PASS、dotnet/CS0、旧138无新增投影差异、Ledger482/88PASS。**下一唯一入口 `NTSD28-Q05-NATIVE-FRAME-SOURCE-INTEGRATION-001 / TYPED_CONVERTER_NEXT`**，先准确Record，再完整27/24/40/19/geometry/WPoint9及frame标量接线/投影；不重做AST。identity/schema/carrier/Play后继保持，Scene精度差异仍保护，Q05与总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式资源未迁移。

> 当前执行 `NTSD28-Q05-NATIVE-FRAME-AST-ADMISSION-001 / IN_PROGRESS / TEST_FIRST`；父native frame source Task继续，先修原版行边界/AST前置，再接typed converter。strength加载限定证据保持，Scene差异保护。

> **Q05-A2 native strength整表加载已验证（2026-09-13）：** `NTSD28-Q05-NATIVE-STRENGTH-TABLE-ADMISSION-001 / FOCUSED_TEST_PASS / VERIFIED_TABLE_LOAD_ONLY`；34语法夹具、正式405中10个strength定义40条原版双跑，真实Unity manager接线；RED34→189PASS+补正确namespace74PASS（45重复，共218不同测试），完整SelfCheck PASS、CS0/dotnet0error、旧138零新增投影差异、Ledger481/85PASS。Scene先前disabled已恢复，现仅UI精度差异，来源仍pending，未回退/不认证Scene unchanged。**下一唯一Task `NTSD28-Q05-NATIVE-FRAME-SOURCE-INTEGRATION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**：先核对native frame/FieldBag，再完整27/24/40/19/geometry接线与投影；identity/schema/carrier/Play及全部后继保持。Q05与总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式资源未迁移。

> **Q05-A2 ITR40/strength19 record已写（2026-09-13）：** `NTSD28-Q05-ITR40-STRENGTH19-RECORD-DECODING-001 / FOCUSED_TEST_PASS / TABLE_SOURCE_INTEGRATION_PENDING`保持活跃；三字段/clone/projection/hash、40/19单记录decoder已落盘。RED120→初次488/489，改用native真实FieldBag后最终489/489，完整SelfCheck PASS、CS0、旧138无新增旧投影差异。表头合同已纠正：native仅1..9/拒绝重复，caption非字段，catalog拒绝错误definition。**Scene文件12:15:20多出HUDCamera/ScenesCamera/Canvas disabled及UI坐标差异，ORIGIN_PENDING；已异步询问用户，保留未回退，isDirty=false不可当Scene unchanged。** 下一唯一Task `NTSD28-Q05-NATIVE-STRENGTH-TABLE-ADMISSION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`；source/identity/schema/Play及其他后继保持，总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> **Q05-A2 Geometry已写/待接线（2026-09-13）：** `NTSD28-Q05-GEOMETRY-CONTENT-CONTRACT-001 / FOCUSED_TEST_PASS / SOURCE_AND_ALGORITHM_INTEGRATION_PENDING`保持活跃；BDY ZWidth/HasGeometry、ITR z/有效性及copy/projection/fingerprint已落盘。native58文件88记录双跑一致，RED92→首次371/375，独立`NTSD28-Q05-RELATED-HITPLAN-FIXTURE-CORRECTION-001 / VERIFIED_TEST_ONLY`纠正4旧HolderCopy/kind7夹具后最终375/375；完整SelfCheck PASS、CS0/Scene clean/root14、旧138无新增旧投影差异。没有修候选算法或切来源，Q03旧27首差留Q06。下一唯一Task `NTSD28-Q05-ITR-STRENGTH-CONTENT-CONTRACT-001 / READY_FOR_EXACT_PRECHANGE_RECORD`；CPoint/OPoint/Geometry均在A2/Q05来源/identity/Play回访，其他后继与例外保持，总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> **Q05-A2 OPoint24已写/待接线（2026-09-13）：** `NTSD28-Q05-OPOINT24-CONTENT-CONTRACT-001 / FOCUSED_TEST_PASS / SOURCE_INTEGRATION_PENDING`保持活跃。Value/DTO/adapter24与native block decoder已落盘；RED49FAIL→98PASS（新49+旧6+CPoint43）、native43文件45x24、实际多转单和pool复用、完整SelfCheck PASS；CS0/Scene clean/root14。旧138加载成功，相对CPoint轮无新增投影差异；旧工具19/8投影不证明新27/24。没有切来源、修materializer或发布新版本。下一唯一Task `NTSD28-Q05-GEOMETRY-CONTENT-CONTRACT-001 / READY_FOR_EXACT_PRECHANGE_RECORD`；CPoint与OPoint均在A2/Q05来源/identity/Play回访，全部后继约束保持，总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> **Q05-A2 OPoint24当前执行（2026-09-13）：** `NTSD28-Q05-OPOINT24-CONTENT-CONTRACT-001 / IN_PROGRESS / TEST_FIRST`，准确七脚本Record已建；source-linked native43文件/45个24字段向量已捕获，RED待现有Editor重载后运行。CPoint27 Change仍FOCUSED_TEST_PASS/SOURCE_INTEGRATION_PENDING；生产来源、外层身份和版本未切，Q05/总目标ACTIVE。

> **Q05-A2 CPoint27已写/待接线（2026-09-13）：** `NTSD28-Q05-CPOINT27-CONTENT-CONTRACT-001 / FOCUSED_TEST_PASS / SOURCE_INTEGRATION_PENDING`保持活跃。DTO/value/canonical27与3float32/独立hurt/new8int已落盘，新block decoder未切manager；RED42FAIL/1PASS→实际97+13 PASS、完整SelfCheck PASS、CS0/Scene clean/root14。旧138加载成功，710处throwvz均native float32舍入，不能称数值无变；raw资源未改。CPoint新ABI尚未和外层identity/schema一起完成，禁止发布半迁移baseline。下一唯一Task `NTSD28-Q05-OPOINT24-CONTENT-CONTRACT-001 / READY_FOR_EXACT_PRECHANGE_RECORD`；CPoint接线/投影/identity/Play在A2/Q05回访，旧phase/landing ULP/stage暂缓保持。总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> **Q05-A1限定交付（2026-09-13）：** `NTSD28-Q05-NATIVE-NUMERIC-DECODER-001 / VERIFIED_NUMERIC_HELPER_ONLY`。RED41失败→Unity43/43，含native3622数值与raw969字段共14742扩展比较0差异；完整SelfCheck PASS、CS0/Scene clean/root14、Ledger475/56PASS，保护3059与Q04出口同差异/零缺失。旧Converter/caller/schema/正式资源未改，不宣称运行时接线。下一唯一Task `NTSD28-Q05-LOGAN-CONTENT-MODEL-INTEGRATION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`，先准确路径Record再同Q05窗口迁移模型/入口；115候选inventory复用。Q05及总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，旧phase/landing ULP与stage暂缓保持。

> **Q05当前执行（2026-09-13）：** `NTSD28-Q05-NATIVE-NUMERIC-DECODER-001 / IN_PROGRESS`，新增加载期pure decoder，旧Converter/caller/schema/正式资源尚未改。RED41失败→Unity41/41；native3622数值10866比较及raw969字段3876比较0差异。扩展两组Unity回归待重载后运行；不是Q05出口。115候选路径inventory已存父Q05工件，下一继续同窗口模型/身份/runtime迁移，不重做Q03/Q04。总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> **Q04已交付，下一Q05（2026-09-13）：** Q04-B `NTSD28-Q04-OSCILLATE-CONSUMER-RETIREMENT-001 / VERIFIED_LEGACY_OSCILLATE_READER_ONLY`：RED8FAIL/6PASS→28/28、完整SelfCheck、实际CentralOnly Play slot0偏移/Blink/timeout/延迟速度及共享production快照/恢复PASS，CS0/Scene clean/root14/Ledger474-52PASS。SpriteRenderer前置失败保留，验证改用实际managed表现路径，不宣称GPU全域。Q04-A/B仅行为退休完成，carrier/schema仍旧。下一Q05 Task `NTSD28-Q05-JOINT-CONTENT-RUNTIME-SCHEMA-MIGRATION-001 / READY_PRECHANGE_INVENTORY`，先准确code-path/Record再同窗口迁移；旧phase/landing ULP后继保留，R13行为PARTIAL_RETURN；总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> **Q04-A已验证，下一Q04-B（2026-09-13）：** `NTSD28-Q04-MASS-FRICTION-GATE-RETIREMENT-001 / VERIFIED_MASS_GATE_ONLY`。production单gate；RED6FAIL/3PASS→focused14/14、完整SelfCheck PASS、真实driver ground/landing三mass0/-2/1一致、cleanup通过、Scene dirtyfalse/root14、CS0、Ledger473/49PASS。首次Play坐标被stage钳制的失败保留，probe已取实际范围中点后通过。相关20项中1个空World旧phase断言失败保留到Q12；另实测既有landing乘1/3与native除3有一ULP首差，加入Q06/B4精确数值待办，不宣称完整landing parity。下一唯一Task `NTSD28-Q04-OSCILLATE-CONSUMER-RETIREMENT-001 / READY_FOR_PRECHANGE_RECORD`，先建Record/RED，只退旧reader。mass/reserved/schema保留到Q05；R13 mass行为PARTIAL_RETURN，总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> **Q04-A当前执行（2026-09-13）：** `NTSD28-Q04-MASS-FRICTION-GATE-RETIREMENT-001 / FOCUSED_TEST_PASS / FULL_SELFCHECK_PASS / PLAY_PENDING`。production仅移除CharacterMechanics mass>0条件；RED6失败/3通过，focused14/14、完整SelfCheck PASS。相关20项有1个空World旧phase[28]断言失败已保留，未改pass。首次Play摩擦正确但fixture Z200被stage min237钳制，cleanup通过；已改probe先warmup取真实stage中点，下一复验Play，不重跑已完成审计或覆盖现有Record。mass/snapshot/schema仍留Q05。总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> **Q03已交付，进入Q04（2026-09-13）：** `NTSD28-NATIVE-DAT-AND-JOINT-FIELD-CONTRACT-AUDIT-001 / DELIVERED_CONTRACT_ONLY`，出口证据Q03-EXIT-REPORT.md。数值见证37 native双跑稳定、Unity源链接37/37，333值113异；几何42项15同/27异。出口复核更正：旧Oscillate晚帧reader/base-shell仍在，Q04仅退reader，Q05统一删载体并base1→2；其producer退休保持。Q05版本集合12→13/20→21/23→24/character1→2/base1→2，当前版本均未改。下一唯一入口Q04-A Task `NTSD28-Q04-MASS-FRICTION-GATE-RETIREMENT-001`，先建立准确Change Record与RED测试；随后Q04-B OSCILLATE-CONSUMER。Q03 numeric Change限定VERIFIED，Ledger472/45PASS。R13/R15仅合同PARTIAL_RETURN，Q02成果保持，正式资源未迁移；总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE、BATCH-02未完成。

> **Q03版本/身份合同推进（2026-09-13）：** 同Q03工件 `VERSION-IDENTITY-AND-CAPTURE-CONTRACT.md` 已记录联合12→13/20→21/23→24/character1→2，其他payload保持；OPoint双owner空队列capture/restore前置（不Flush）、语义摘要确定编码、CPoint27顺序及float32 bit规范、339处reserved/alias分类和+2F8两个object-AI reader。只读合同与规范向量，尚非生产实现。下一唯一动作：native数值语法/bit witness，再逐项核对Q03-A/B完整出口；不重复Q02/几何14/42/两条factory清单。父Q03仍IN_PROGRESS，总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，schema/资源/production保持。

> **Q03消费合同推进（2026-09-13）：** OPoint24字段及logic-only/renderer两条factory/initializer/PostInit/copy/reset已记录于同Q03工件 `OPOINT-AND-HELD-DEPTH-CONTRACT.md`。确认显式team与hp/mp有后处理覆盖、缺definition/无slot的整loop终止、未声明0..998零frame、held candidate取holder当前WPoint选择武器strength。WeaponStrength index+8对native index+19的新内容缺口纳入Q05同窗口；不把旧无caller ProcessAttack接成正式路径。此轮只读代码/更新合同，无新脚本/测试/资源变更。父Q03仍IN_PROGRESS；下一为canonical float、reserved/AI全reader、decode-version与Lockstep identity绑定、snapshot的OPoint队列边界。几何14/42见证已交付，不重复；Q02关闭职责保持，总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> **Q03几何见证已交付（2026-09-13）：** `NTSD28-Q03-COLLISION-GEOMETRY-WITNESS-001 / VERIFIED_CAPTURE_ONLY`；同DAT native14用例与真实Unity三模式42项比较，15相同/27不同，明确深度端点、BDY zwidth、ITR z、负宽度及缺失几何差异；不是parity PASS。正式indexed内容有1127个非零BDY zwidth/78个非零ITR z。报告位于同ID artifacts/diagnostics/REPORT.md；CS0、Scene dirtyfalse/root14、Ledger471/43PASS。只新增诊断工具/Editor测试，production/schema/正式资源不变。父Q03仍IN_PROGRESS，下一继续held strength有效深度、两条OPoint materializer、canonical float及联合字段全reader；新增BDY/presence合同纳入Q05同一窗口。总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，Q02限定交付保持，不重做已关闭职责。

> **Q03当前游标（2026-09-13）：** 总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，BATCH-02 / Q03 / NTSD28-NATIVE-DAT-AND-JOINT-FIELD-CONTRACT-AUDIT-001 / IN_PROGRESS。六正式DAT九frame已逐块分类（hir frame414为CPoint drain）；复用source-linked双端capture测得CPoint浮点截断/alias、整数准入与WPoint未知字段差异，27/9/24字段形状已捕获。+2F8独立于Spawner、mass/reserved复制/重置/快照链及BDY/ITR候选深度差异已记录；完整consumer/联合版本矩阵仍未冻结。下一继续同一Q03，先完成BDY/ITR live caller与边界证据、两条OPoint materializer、canonical float及全部schema reader矩阵；不跳到Q04/Q05。报告：artifacts/diagnostics/NTSD28-NATIVE-DAT-AND-JOINT-FIELD-CONTRACT-AUDIT-001/Q03-PROGRESS-REPORT.md，联合表JOINT-FIELD-MATRIX.md。Q02限定交付保持；本轮无脚本/正式资源/schema修改，无新增Unity/Play证据。schema12/20/23与character shell1保持，Q07正式迁移未执行；以下旧READY/下一E2均为历史。

> **当前执行游标（2026-09-13）：** 总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，BATCH-02继续。Q02加载基础已DELIVERED / VERIFIED_LOAD_INFRASTRUCTURE_ONLY；E3 `NTSD28-B11-SOURCE-CACHE-CALLER-PRODUCTION-001 / VERIFIED_SOURCE_CACHE_CALLER_LOAD_GATES_ONLY`：最终focused40/40、完整SelfCheck最终PASS、native direct/app/menu/menu重进4次Play（每次World4/三key一致/实际pool0/46资源零残留/两帧仍Stopped），默认旧内容App回归也PASS。CS0、NTSD_Battle dirtyfalse/root14、Ledger470/40PASS、保护3059中3047不变/12声明脚本变化/零缺失。首轮late-injection Play失败保留，BeforeSceneLoad测试clone解决注入顺序，原asset未改。下一唯一入口：docs/ai/TASKS/NTSD28-NATIVE-DAT-AND-JOINT-FIELD-CONTRACT-AUDIT-001.md / READY_CONTRACT（Q03六DAT九frame及CPoint27/OPoint24/+2F8/mass/reserved联合合同）；先只读权威消费链/字段矩阵，不提前升schema或部署资源。Q07正式DAT/图片迁移及B11/B12完整验收未完成；schema12/20/23、33ms、十一阶段、Unity/GAS与非战斗行为保持。

> **Q02 E2进行中（2026-09-13最新）：** `NTSD28-B11-SOURCE-ATOMIC-PUBLICATION-001 / FOCUSED_TEST_PASS`只覆盖候选输入绑定与verified decoder：Unity20/20（7+13）、CS0、Scene dirtyfalse、Ledger468/25PASS。新增LoganVisualContentCandidate绑定catalog/config与sheet/head/small hash；BMPLoader在实际decode bytes上验证SHA。E2整体仍IN_PROGRESS，下一继续同一Task/Change的实际staging/停止世代/prepared object-UI/无await提交/资源重绑退休；不是新子目标或完整交付。已确认旧commit提前退休及IsPrewarmCompleted早于UI，uGUI Image保留旧Sprite引用，细节在CANDIDATE-INPUT-REPORT.md；后续脚本前扩充准确Record。global/非战斗/资源未切换，schema12/20/23保持；Q03六DAT阻塞仍在，E3/Q07后继。总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE；以下旧READY/下一步以本条覆盖。

> **Q02目录E1已验证（2026-09-13最新）：** `NTSD28-B11-LOGAN-CATALOG-CONFIG-CANDIDATE-001 / VERIFIED_CATALOG_AND_CONFIG_CANDIDATE_GATES_ONLY`；真实source-linked native330/330与Unity目录逐项一致，Unity39/39（本包15+路径24）、CS0、Scene dirtyfalse、Ledger467/23PASS。正式config仍由6个DAT Converter失败阻止，未返回partial，不能称新内容整体可用。DefinitionFingerprint仅catalog/DAT，PNG/head/small等完整身份留E2/E3。下一 `docs/ai/TASKS/NTSD28-B11-SOURCE-ATOMIC-PUBLICATION-001.md / READY_CONTRACT`；父CATALOG-PUBLICATION为E1已交付/E2原子发布/E3缓存caller未做。Q03字段合同可独立准备；Q02/BATCH-02/总目标ACTIVE，FULL_ALIGNMENT_INCOMPLETE。global/非战斗/正式资源未切换，schema12/20/23保持；以下旧下一步以本条覆盖。

> **Q02 PNG alpha已验证（2026-09-13最新）：** `NTSD28-B11-PNG-SHEET-ALPHA-CONTRACT-001 / VERIFIED_SOURCE_PNG_SHEET_AND_GPU_SAMPLES_ONLY`；Unity21/21，本包6+PNG13+Overlap2。隔离/正式nar实际sheet staging→atlas pixels→SpriteCatalog→现有shader共10个GPU样点通过（Direct3D11/URP/Gamma），CS0，Scene dirtyfalse，Ledger466/19PASS。旧UI/BMP/processor/shader保持，资源未迁移，schema12/20/23不变。下一 `docs/ai/TASKS/NTSD28-B11-CATALOG-PUBLICATION-CONTRACT-001.md / READY_CONTRACT`，从Q02-A已确认共享caller冻结目录/source/cache/publication事务；Q03字段合同也可准备。R17 raw/range/alpha子条件PARTIAL_RETURN；Q02/BATCH-02及总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。未验全局prewarm/正式迁移/全场Play和所有绘制路径，以下旧下一步由本条覆盖。

> **Q02-C已验证（2026-09-13最新）：** `NTSD28-B11-NATIVE-SPRITE-RANGE-CONTRACT-001 / VERIFIED_SOURCE_RANGE_ADMISSION_ONLY`；真实Unity14/14，405DAT/773sheet原始声明/有效范围/尺寸/路径与native capture一致，CS0，Scene dirtyfalse，Ledger465/18PASS。新增Logan配对parser/builder；旧入口和字段形状/schema12/20/23保持。3059保护中仅本包parser/manager及前包BMPLoader声明变化，零缺失。下一 `docs/ai/TASKS/NTSD28-B11-PNG-SHEET-ALPHA-CONTRACT-001.md / READY_CONTRACT`，再做catalog/cache/publication；Q03也可准备。Q02/BATCH-02与总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式资源未迁移、未验整场Play/GPU；R17 raw+range仅PARTIAL_RETURN。以下旧下一步以本条为准。

> **Q02-B RAW PNG已验证（2026-09-13最新）：** `NTSD28-B11-PNG-WORKER-DECODE-001 / VERIFIED_RAW_WORKER_DECODE_ONLY`；正式1255/1255尺寸/RGBA hash匹配、真实Unity13/13+最大图1/1、CS0、Scene dirtyfalse。只新增纯decoder并接BMPLoader后台PNG分支，旧BMP/mainthread与非战斗逻辑保持；正式资源未迁移。当前BATCH-02/Q02 IN_PROGRESS，下一`docs/ai/TASKS/NTSD28-B11-NATIVE-SPRITE-RANGE-CONTRACT-001.md / READY`。新增P-21/PNG-SHEET-ALPHA-CONTRACT-001：raw之后的旧sheet处理强制alpha255，不能宣称最终PNG表现正确；该项和source/catalog/cache/publication必须继续处理。R17只完成raw输入/解码PARTIAL_RETURN，R15仍PARTIAL_RETURN；总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> **Q02路径合同已验证（2026-09-13最新）：** `NTSD28-B11-CONTENT-SOURCE-PATH-CONTRACT-001 / VERIFIED_PURE_PATH_ONLY`；RED0/20→源链接24/24、真实Unity EditMode24/24，CS错误0，3059既有文件hash不变，NTSD_Battle dirtyfalse。新增BattleContentSource只描述正式catalog/DAT/VFS与旧Unity路径，不切global cache/caller。Q02-A审计已交付，正式对象权威是catalog.csv/registry_index，data.txt不替代它；sprite范围31文件中25个条件性集合仍不同。当前BATCH-02/Q02 IN_PROGRESS，下一`docs/ai/TASKS/NTSD28-B11-PNG-WORKER-DECODE-001.md / READY`；Q02-C range和catalog/cache/publication待做，Q03可独立准备。没有运行整场Play/SelfCheck或迁移资源。主目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，下面旧下一步按本游标覆盖。

> **总目标活动中，第一批已交付（2026-09-13）：** `BATCH-01 / Q01 / NTSD28-B11-CONTENT-ENTRY-INVENTORY-001 / DELIVERED / VERIFIED_OFFLINE_AUDIT_ONLY`。正式330对象/24背景，405新版DAT与138旧DAT实际双端捕获，输入/源码/header身份通过；6文件9帧Converter拒绝及root/PNG/字段缺口已登记。报告：`artifacts/diagnostics/NTSD28-B11-CONTENT-ENTRY-INVENTORY-001/Q01-REPORT.md`。R15仅Q01身份子条件PARTIAL_RETURN；Q05/Q07条件保留。下一`BATCH-02 / Q02-A / docs/ai/TASKS/NTSD28-B11-SOURCE-ROOT-AND-CACHE-CONTRACT-AUDIT-001.md / READY`，从source/cache合同审计开始；Q03也具备准备前置。3059文件保护hash不变，production/非战斗/资源/Scene未修改，未运行Unity/Play。总目标仍ACTIVE/FULL_ALIGNMENT_INCOMPLETE；下方旧HOLD/准备态和旧下一包均为历史，不覆盖当前游标。

> **D-023 内容决定（2026-09-12，用户已确认）：** DAT 与角色相关图片采用当前 NTSD 2.8-Logan 正式 runtime 版本。
> 原 Unity 138-DAT 仅保留为迁移前基线；本范围不再是 CONTENT_STRATEGY_PENDING。资源迁移尚未执行，先完成 parser/loader/引用清单与分批验证。
> “估计全部删除”尚未形成精确删除集合，不清空整个 Config/Sprite；既有 UI/地图/音频和表现例外不自动改变。
> 当前审计直接修订对齐总表第 4 节，并列出已验证子集、确认脚本缺口与待验项；下方旧恢复建议仅作历史。


> **当前任务视图（2026-09-12 已逐项修订）：** `NTSD28-ALIGNMENT-REPLAN-20260912 / CURRENT_MATRIX_RECONCILED / DOCUMENTATION_ONLY / FULL_ALIGNMENT_INCOMPLETE`。
> 用户已通过 D-023 明确 DAT/角色相关图片采用 NTSD 2.8-Logan，迁移未执行；不再等待该内容方向，也未撤销其他例外。
> **总目标＋六批次已准备，禁止执行（2026-09-13最新用户要求）：** `NTSD28-UNITY-BATTLE-REALIGNMENT-001 / PREPARED_NOT_STARTED / EXECUTION_USER_HOLD`。BATCH-01～06均未启动；当前 `PREPARED_BATCH=BATCH-01 / NEXT=Q01 / ACTIVE=NONE / READY_RETURNS=NONE`，Q01为PREPARED_HOLD。
> 恢复先读 `Assets/NTSD/Docs/ntsd28-logan-vs-unity-battle-alignment.md` 第0.14节总目标/批次/启动状态，再读0.11队列、0.12回访、0.13游标；仅准备，不启动Q01审计、goal、线程、自动化或Unity验证。用户明确启动某批才做该批；明确启动总目标持续执行才按批推进。
> 硬边界：保持Unity/GAS框架与非战斗行为；只改经声明的战斗逻辑及必要适配和D-023资源。共用脚本若无法避免影响非战斗功能，该部分先停并说明，未经用户明确扩大范围不得修改。保护已有Foot Marker等用户工作。
> B1当前生产职责完成，B2基础与路由职责关闭；B3/B5仅出口放行、整域后置，B4/B6部分子集完成，B7～B12未完全完成。B1/B2已关闭职责移出实施队列；下游依赖/最终验收不撤销既有成果。
> 确认脚本缺口包括 CPoint27/alias、OPoint24对Unity8、资源根/后台PNG、+2F8语义、mass/联合schema、结果分类/时点、同Z排序/插值和表现音频consumer。
> Q子包完成后立即检查R回访：B4复活等待B7 producer/新版内容，B5资源KO等待CPoint/B8，B3残余等待B7/B8接管，B1 worker等待B9启用条件。已满足触发的回访优先；Q11只核对是否漏做，不能全部拖到B12。
> 数据定义/旧行为退休→一次联合schema窗口→生产接线→内容可用→视听→最终集成；schema不等待整场Play，资源载入不等待最终视听，避免循环依赖。本次仅整理文档，无资源删除或运行时修改，未新增编译/测试/Play证据。
> D-022路线已决定但当前runtime/snapshot/checksum仍12/20/23；正式新内容、整场trace、物理技能键和视听验收尚未完成。

> **USER HOLD（2026-09-09）：** 用户要求停止当前 `NTSD28-UNITY-BATTLE-REALIGNMENT-001` 自动推进，
> 改由 GLM 先核验当前进度、现有生产脚本和证据，再整理真正遗漏与未处理项；禁止从头重做已完成逻辑。
> 暂停期间不得启动新对齐包、继续Play验收、修复独立SelfCheck/stress失败或
> 修改production/content/Scene；新会话可直接复制
> `docs/ai/GLM-INCREMENTAL-CONTINUATION-PROMPT-2026-09-09.md`，详细证据交接见
> `docs/ai/GLM-REALIGNMENT-HANDOFF-2026-09-09.md`。总目标保持 `FULL_ALIGNMENT_INCOMPLETE`。

> **B6 positive-link validation已退休（2026-09-09）：** `NTSD28-B6-POSITIVE-LINK-VALIDATION-RETIREMENT-PRODUCTION-001 / VERIFIED / RED_4_FAIL_2_PASS_OF_6 / FOCUSED_6_OF_6 / RELATED_33_OF_33 / B6_CATEGORY_103_OF_103 / NTSD28_229_OF_229 / STRESS_255_OF_256_1_UNRELATED_AI_REPORT / REAL_BATTLE_GRAB_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_R2_CPOINT_SYNC / CONSOLE_0_ERROR / SCENE_UNCHANGED / PHASE_33 / NO_POSITIVE_EVENT`。正式post-catch现直接进入stage clamp/第二次held；compat entry无写入无event，lifecycle transaction保持唯一原子cleanup owner。下一先做B6剩余入口/exit只读复核，不凭旧backlog直接启动跨域实现。

> **B6 held injury caughtact event已验证（2026-09-09）：** `NTSD28-B6-HELD-INJURY-CAUGHTACT-EVENT-PRODUCTION-001 / VERIFIED / RED_5_FAIL_10_PASS_OF_15 / FOCUSED_15_OF_15 / B6_CATEGORY_97_OF_97 / NTSD28_223_OF_223 / HITPLAN_185_OF_185 / PREINTERACTION_15_OF_15 / REAL_BATTLE_GRAB_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_POSITIVE_LINK / CONSOLE_0_ERROR / SCENE_UNCHANGED / LEDGER_PASS_428_364 / POST_SETTLEMENT_EVENT_EXACT`。真实applied正injury event现于完整settlement后按序消费，Play count0→1且无重复；下一strict首差为positive-link validation retirement。

> **B6 held injury caughtact event启动记录（已由上条VERIFIED关闭）：** 本包曾以`IN_PROGRESS / TEST_FIRST / PRODUCTION_UNCHANGED`启动；恢复时以上条最终证据为准。

> **B6 held injury accounting/cover已验证（2026-09-09）：** `NTSD28-B6-HELD-INJURY-ACCOUNTING-COVER-PRODUCTION-001 / VERIFIED / RED_14_FAIL_3_PASS_OF_17 / FOCUSED_17_OF_17 / B6_CATEGORY_82_OF_82 / NTSD28_208_OF_208 / HITPLAN_185_OF_185 / PREINTERACTION_15_OF_15 / RELATED_FIXTURES_9_OF_9 / REAL_BATTLE_GRAB_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_POSITIVE_LINK / CONSOLE_0_ERROR / SCENE_UNCHANGED / LEDGER_PASS_427_363 / CANONICAL_ACCOUNTING_COVER_EXACT`。`IncomingDamageScale340`、direct owner/type0 self、HP/HPBound/consumed/score/KO与cover exclusion timers已闭合；下一strict entry是caughtact event。

> **B6 settlement vaction preflight已验证（2026-09-09）：** `NTSD28-B6-CATCH-SETTLEMENT-VACTION-PREFLIGHT-PRODUCTION-001 / VERIFIED / RED_4_FAIL_4_PASS_OF_8 / FOCUSED_8_OF_8 / B6_CATEGORY_65_OF_65 / NTSD28_191_OF_191 / HITPLAN_185_OF_185 / PREINTERACTION_15_OF_15 / TARGETED_PLAY_8_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_UNCHANGED / LEDGER_PASS_426_362 / ACTUAL_PREFLIGHT_EXACT`。signed/zero vaction提交与post-action frame/kind2 terminal fence已闭合；下一严格入口是held injury exact accounting。

> **B6 mixed catch advance/exact consumer已验证（2026-09-09）：** `NTSD28-B6-CATCH-EXACT-CONSUMER-AND-ADVANCE-ORDER-PRODUCTION-001 / VERIFIED / RED_1_PASS_7_FAIL_OF_8 / FOCUSED_16_OF_16 / PREINTERACTION_15_OF_15 / B6_CATEGORY_57_OF_57 / HITPLAN_185_OF_185 / NTSD28_183_OF_183 / TARGETED_PLAY_16_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_UNCHANGED / LEDGER_PASS_425_361 / SINGLE_MIXED_ADVANCE_EXACT_CONSUMERS`。single slot升序mixed advance、三个plain exact +0x90 consumer及mismatch/negative-release terminal fence已闭合；下一严格入口是settlement vaction preflight，再到held accounting。

> **B6 catch relation exact-field production已验证（2026-09-09）：** `NTSD28-B6-CATCH-RELATION-EXACT-FIELDS-PRODUCTION-001 / VERIFIED / RED_0_OF_3 / FOCUSED_19_OF_19 / B6_CATEGORY_41_OF_41 / HITPLAN_185_OF_185 / NTSD28_167_OF_167 / TARGETED_PLAY_19_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_UNCHANGED / EXACT_RELATION_ATOMIC`。actual/HitPlan现闭合kind3 first/signed、双frame preflight、exact+compat与respond；current criminal和kind1均绿。下一严格入口是mixed catch advance/control-flow fences。

> **B6 invalid negative-held reciprocal preserve已验证（2026-09-09）：** `NTSD28-B6-HELD-INVALID-RECIPROCAL-PRESERVE-PRODUCTION-001 / VERIFIED / RED_2_OF_2 / FOCUSED_7_OF_7 / B6_CATEGORY_22_OF_22 / RELATED_47_OF_47 / BROAD_56_OF_62_6_UNRELATED_NATIVE_INPUT_PROXY / TARGETED_PLAY_7_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_BASELINE_RESTORED / DIAGNOSTIC_PRESERVE`。C09/C20 missing/out-of-range/mismatch现在只计数、可选trace并preserve；slot0/high、RNG/sentinel、lifecycle-clean与0B已绿。下一严格入口是catch relation exact-field producer。

> **B6 entity-link lifecycle cleanup已验证（2026-09-09）：** `NTSD28-B6-ENTITY-LINK-LIFECYCLE-CLEANUP-PRODUCTION-001 / VERIFIED / FOCUSED_7_OF_7 / B6_CATEGORY_15_OF_15 / RELATED_91_OF_91 / TARGETED_PLAY_7_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_BASELINE_RESTORED / ATOMIC_RELEASE_CLEANUP / RED_NOT_EXECUTED`。成功slot release与generation row release/reuse之间现原子清held/catch exact+compat反向关系；P7旧复用夹具已纠正，full SelfCheck恢复停在独立held injury accounting。下一严格包为invalid reciprocal preserve，不混入catch算法或positive-pass retirement。

> **B6 CPoint throw精确子集已验证（2026-09-09）：** `NTSD28-B6-CPOINT-THROW-ENVIRONMENT-VZ-PRODUCTION-001 / VERIFIED / FOCUSED_8_OF_8 / RELATED_17_OF_17 / TARGETED_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_CURRENT_BASELINE_UNCHANGED / FULL_RESOURCE_DEFERRED / ILLEGAL_CATEGORY_RETIRED`。display/environment/self-source/WeaponCount exclusion/Vz XOR已获真实Unity与Play证据；full SelfCheck已越过throw并停在独立held injury accounting。完整MP resource仍后置B7/B8/B11/H。

> **B5 negative environment shared recovery已验证（2026-09-09）：** `NTSD28-B5-NEGATIVE-ENVIRONMENT-RECOVERY-PRODUCTION-001 / VERIFIED / RED_11_OF_12 / FOCUSED_12_OF_12 / RELATED_154_OF_154 / RELATED_B5_981_OF_981 / TARGETED_PLAY_12_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / SINGLE_EXACT_TRANSACTION / FLUTE_FALSE_POSITIVE_RETIRED`。Legacy/DataOriented/derived现共享negative EnvironmentState/native phase/rule/900-scale/two-hop exact transaction并post-accounting clamp0；其当时的CPoint throw Vz阻塞已由后继B6包关闭，fresh full SelfCheck当前推进到独立held injury accounting。B6 producer、B8 event与联合schema后置。

> **B5 negative environment clamp合同纠正（2026-09-09）：** `NTSD28-B5-NEGATIVE-ENVIRONMENT-CLAMP-CORRECTION-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / SOURCE_HASH_MATCH / CLAMP_TO_ZERO_REQUIRED / PRIOR_NO_CLAMP_CLAUSE_SUPERSEDED`。无漂移playable source与source tests要求exact accounting后HP/HPBound clamp0；先前owner audit仅“no clamp”一句被纠正。

> **B5 negative environment rule carrier已验证（2026-09-09）：** `NTSD28-B5-NEGATIVE-ENVIRONMENT-RULE-CARRIER-001 / VERIFIED / RED_13 / FOCUSED_6_OF_6 / RELATED_106_OF_106 / NTSD28_1394_OF_1394 / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_BASELINE_UNCERTIFIED / CARRIER_READY / RECOVERY_CONSUMER_NEXT`。+0x90/default9 deterministic carrier与schema `11/20/23`闭合，exact NTSD28 broad1394/1394；下一single recovery consumer。full SelfCheck独立CPoint阻塞，Scene基线未认证。

> **B5 negative environment recovery owner审计已闭合（2026-09-09）：** `NTSD28-B5-NEGATIVE-ENVIRONMENT-RECOVERY-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / WEAPONCOUNT_WRONG_CARRIER / EXACT_TRANSACTION_REQUIRED / TWO_PACKAGE_ROUTE`。Authority是negative EnvironmentState320+native phase12+rule90/exact credit；Unity两路WeaponCount分支均错误且被flute -20真实触发。下一先补rule +0x90 carrier，再统一consumer。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B5-INPUT-HP-COST-COMPAT-SHARED-TRANSACTION-PRODUCTION-001 / VERIFIED / RED_0_OF_6 / FOCUSED_6_OF_6 / RELATED_INPUT_187_OF_187 / RELATED_B5_926_OF_926 / TARGETED_PLAY_6_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / TWO_INPUT_WRITERS_RETIRED / NEGATIVE_RECOVERY_AUDIT_NEXT`。registered Legacy/DataOriented与unregistered generic action已共享existing exact transaction，两处ComboVic HP-cost writer归零；下一严格包审计两套negative recovery writer的Authority owner。

> **B5 input HP-cost compatibility stats审计已闭合（2026-09-09）：** `NTSD28-B5-INPUT-HP-COST-COMPAT-STAT-RETIREMENT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / COMPAT_PATH_LIVE / PARTIAL_RETIREMENT_UNSAFE / SHARED_TRANSACTION_PRODUCTION_DEFINED`。一个dead duplicate与一个可配置Legacy production compat都写旧ComboVic；因compat同时缺完整native cost/fallback字段，禁止仅换成+0x34C或只删writer。下一严格包复用现有exact transaction后统一退休两处。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B5-TYPE3-WEAPON-FLUTE-LEGACY-STAT-WRITER-RETIREMENT-001 / VERIFIED / RED_0_OF_4 / FOCUSED_4_OF_4 / RELATED_B5_777_OF_777 / TARGETED_PLAY_4_CASES / LIVE_COLLISION_MATRIX_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / SCOPED_LEGACY_STATS_RETIRED / INPUT_COMPAT_AUDIT_NEXT`。type3/weapon victim/world与flute holder/world Authority-extra legacy镜像已退休，exact HP/KO/+0x2F4与standard/reduced/CPoint/input/recovery/schema保持；下一严格包只读审计input HP cost compat seam。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B5-STANDARD-REDUCED-KNOCKOUT-PRODUCER-001 / VERIFIED / RED_1_OF_4 / FOCUSED_4_OF_4 / RELATED_270_OF_270 / TARGETED_PLAY_4_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / EXACT_KO_PRODUCER_READY / TYPE3_WEAPON_FLUTE_STATS_NEXT`。standard/reduced lethal现按same attribution在HP mutation前写exact `KnockoutCount358`，HitPlan同值；下一严格包退休type3/weapon/flute安全legacy stat镜像。

> **B5 legacy damage-stat writer retirement readiness已闭合（2026-09-09）：** `NTSD28-B5-LEGACY-DAMAGE-STAT-WRITER-RETIREMENT-READINESS-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / GLOBAL_RETIREMENT_BLOCKED / FOUR_PREREQUISITE_ROUTES`。standard/reduced缺exact KO，held缺exact accounting，input compat与negative recovery也未闭合；先补B5 standard/reduced KO，再退type3/weapon/flute安全镜像，不能直接全删。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B5-ORDINARY-CREDIT-GATE-2F4-PRODUCER-CONSUMER-CORRECTION-001 / VERIFIED / RED_0_OF_5 / FOCUSED_5_OF_5 / RELATED_287_OF_287 / TARGETED_PLAY_5_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / EXACT_2F4_READERS_PRODUCERS / LEGACY_STATS_WRITER_NEXT`。standard/reduced/CPoint damage、PP threshold与type0 OPoint传播已重绑exact `OrdinaryCreditGate2F4`，non-type0不再产生该值或legacy KillCount；下一严格route为B5 legacy damage-stat writer retirement，carrier/schema继续后置。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B5-TYPE3-LEGACY-HOLDERCOPY-WRITER-RETIREMENT-001 / VERIFIED / RED_0_OF_3 / FOCUSED_4_OF_4 / RELATED_284_OF_284 / TARGETED_PLAY_3_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / FOUR_TYPE3_WRITERS_RETIRED / TYPE3_SPECIFIC_FAMILY_EXIT_READY / ORDINARY_CREDIT_GATE_2F4_NEXT`。actual kind9与HitPlan三处Authority不存在的HolderCopy write已退休，type3 exact group/owner/control/action/motion保持；下一严格route为B5 `OrdinaryCreditGate2F4` producer/consumer correction，legacy stats与联合schema继续后置。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B5-KIND5-LINKED-PARENT-SLOT-CORRECTION-001 / VERIFIED / RED_0_OF_5 / FOCUSED_5_OF_5 / RELATED_280_OF_280 / TARGETED_PLAY_5_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / EXACT_LINKED_PARENT_BOUND / HOLDERCOPY_UNCHANGED / TYPE3_WRITER_NEXT`。Authority linked-parent现映射HolderStableId/implicit-zero，frozen pair/kind5/negative-link不再读HolderCopy；下一严格route为type3额外HolderCopy writer退休，stats/schema继续后置。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B4-REVIVAL-EXIT-AUDIT-001 / VERIFIED / REVIVAL_TRACE_EQUAL_13_RECORDS_416_FIELDS / DOUBLE_RUN_BYTE_STABLE / RELATED_54_OF_54 / TARGETED_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / B4_REVIVAL_EXIT_READY / PRODUCTION_UNCHANGED / B7_H_PRODUCER_PENDING`。Authority/Unity 13/416 first difference空且双跑稳定，真实Play/Console/Scene通过；full SelfCheck仍为既有CPoint阻塞。B4 revival consumer/exit ready，下一严格route返回B5 HolderCopy binding/extra-write corrections，B7/H producer/schema保持pending。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B4-REVIVAL-NORMAL-FLOOR-RNG-PRODUCTION-001 / VERIFIED / RED_0_OF_7 / FOCUSED_7_OF_7 / RELATED_59_OF_59 / TARGETED_PLAY_7_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / EXIT_AUDIT_NEXT`。normal revival的effective floor、peer/sumX、sync RNG0x90/0x91、precise-only X/Z和vitals tail已闭合；下一严格route为B4 exit audit。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B4-REVIVAL-QUEUED-CONTINUATION-PRODUCTION-001 / VERIFIED / RED_4_OF_11 / FOCUSED_13_OF_13 / RELATED_47_OF_47 / TARGETED_PLAY_13_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / NORMAL_ROUTE_NEXT / PRODUCER_B7_H_PENDING / SCHEMA_DEFERRED`。+0x360 controller/defer/group、queued字段、visual、action219/counter0/hold10与OID998调用前状态已闭合；下一严格route为normal floor/RNG。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B4-REVIVAL-PARTICIPANT-GATE-KILLCOUNT-CORRECTION-001 / VERIFIED / RED_10_OF_16 / FOCUSED_16_OF_16 / RELATED_34_OF_34 / TARGETED_PLAY_20_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / QUEUED_ROUTE_NEXT`。两个legacy HitStun arm与C07 KillCount/team gate已退休，lives-first queued/primary-retain/transient-free/normal分支已恢复；下一严格route为queued continuation细节。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B0-DIRECT-REVIVAL-DEFAULTS-PRODUCTION-001 / VERIFIED / RED_1_OF_3 / DIRECT_DEFAULTS_1_0_0 / FOCUSED_3_OF_3 / DIRECT_OWNER_REGRESSION_15_OF_15 / TARGETED_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / B4_RUNTIME_RESUMABLE`。direct slot0/19现于首次注册前发布Authority默认1/0/0，active snapshot正确且raw backing保持0/0/0；invalid不写。Play/Console/Scene通过，full SelfCheck独立CPoint阻塞。严格恢复B4 revival gate correction。

> **B4 revival owner审计已闭合（2026-09-09）：** `NTSD28-B4-REVIVAL-PARTICIPANT-GATE-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / KILLCOUNT_NOT_REVIVAL_AUTHORITY / THREE_ENTRY_POINTS_SPLIT / DIRECT_DEFAULT_PRODUCER_MISSING / FOUR_RUNTIME_ROUTES_DEFINED / PRODUCTION_HELD`。Authority C25/C07不读KillCount；Unity三个旧gate、wrong branch priority、primary free及queued/floor/RNG后继已拆分。Direction-B state14=235；先回退B0补direct默认字段producer。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B3-LEGACY-1100-1299-CHILD-PROPAGATION-RETIREMENT-001 / VERIFIED / RED_0_OF_4 / PRODUCTION_CHILD_SCAN_REMOVED / FOCUSED_4_OF_4 / TARGETED_PLAY_PASS / BUILDS_0_ERROR / SELF_RESET_PRESERVED / CURRENT_ITACHI_1250_COVERED / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED`。Authority只写self；Unity额外world/KillCount child write已退休。RED matched为0/-99/-149/-198而normal child为40；focused4/4与真实Play通过并覆盖Itachi next1250。full SelfCheck仍由更早CPoint阻塞；严格下一包为B4 revival participant gate correction。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B3-LEGACY-STATE501-TRANSFORM-RETIREMENT-001 / VERIFIED / RED_0_OF_1 / PRODUCTION_BRANCH_REMOVED / FOCUSED_1_OF_1 / EARLY_M2_11_OF_11 / TARGETED_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED`。Authority不存在且双方正式gameplay content为0的early-frame 501 self/KillCount-child transform已从fast/fallback/legacy路径退休；两组10实体canonical runtime/definition/identity/frame均保持。full SelfCheck仍由更早CPoint Vz阻塞；state500、teleport、CPoint、11xx/12xx与schema未动。严格下一包为B3 1100..1299 child propagation retirement。

> **B3 state501审计已闭合（2026-09-09）：** `NTSD28-B3-LEGACY-STATE501-OWNED-CHILD-TRANSFORM-RETIREMENT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / NO_AUTHORITY_STATE501_TRANSFORM / DIRECTION_B_GAMEPLAY_ZERO / RELEASE_GAMEPLAY_ZERO / HUD_RADAR_ONLY_TWO_TOKENS / UNITY_SYNTHETIC_BRANCH_CONFIRMED / PRODUCTION_RETIREMENT_DEFINED`。Authority C25只做8000..8999 definition transition且不扫描owner；Unity early-frame独有501 self/KillCount-child mutation。本审计无code/content/Scene/Authority改动。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B2-AI-OWNER-SLOT-KILLCOUNT-CORRECTION-001 / VERIFIED / OWNER_GUARD_TRACE_EQUAL_8_RECORDS_48_FIELDS / UNITY_FOCUSED_4_OF_4 / TARGETED_PLAY_PASS / DOUBLE_RUN_BYTE_STABLE / BUILDS_0_ERROR / EXACT_OWNER_SLOT_BINDING / LEGACY_KILLCOUNT_DETACHED_FROM_AI / SELFCHECK_BLOCKED_UNRELATED`。Authority source-model与Unity production snapshot/SoA专项trace 8/48 first difference空且各自双跑稳定；B0 direct/OPoint/F8 owner `0/7/99`均被消费。真实NTSD_Battle Play通过、Console0、Scene不变；full SelfCheck仍由较后既有CPoint Vz阻塞。本结论只关闭AI owner/KillCount guard family；严格下一包为B3 legacy state501 child transform retirement audit。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B0-OWNER-SLOT-PRODUCTION-EXIT-AUDIT-001 / VERIFIED / OWNER_TRACE_EQUAL_15_RECORDS_135_FIELDS / DOUBLE_RUN_BYTE_STABLE / TARGETED_PLAY_PASS / ROUTES_1_TO_4_REGRESSION_32_OF_32 / BUILDS_0_ERROR / SELFCHECK_BLOCKED_BY_UNRELATED_CPOINT / B0_OWNER_PRODUCER_EXIT_READY / B2_RUNTIME_RESUMABLE / PRODUCTION_UNCHANGED`。Authority/Unity专项trace已覆盖direct self、owner/target独立、two-hop OPoint、F8 99、state9996 -1、type3 mutation与slot reuse，15/135 first difference空且双端双跑稳定；真实NTSD_Battle Play通过、Console0、Scene不变。full SelfCheck仍被更早CPoint阻塞，不代表full parity或B8 physical F8完成。严格下一步恢复`NTSD28-B2-AI-OWNER-SLOT-KILLCOUNT-CORRECTION-001` runtime验收。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B0-OWNER-SLOT-PRODUCTION-EXIT-AUDIT-001 / IN_PROGRESS / B0_OWNER_PRODUCER_ROUTE_5 / OWNER_ONLY_JOINT_TRACE_DESIGN / PRODUCTION_UNCHANGED`。route1～4已有compile+focused证据；现建立只比较+0x354/OwnerSlot的Authority/Unity规范化trace，覆盖self、two-hop OPoint、F8 99、state9996 -1、type3 mutation和slot reuse。F8 content/count/position、full parity与B8 physical consumer排除；通过前B2 runtime继续阻塞。

> **当前最前置实施包（2026-09-09）：** `NTSD28-B0-OPOINT-OWNER-PROPAGATION-PRODUCTION-001 / FOCUSED_TEST_PASS / UNITY_RUNTIME_COMPILE_PASS / OUT_OF_PROCESS_COMPILE_PASS / UNITY_FOCUSED_7_OF_7 / SELFCHECK_BLOCKED_BEFORE_PRESENTATION_ASSERT_BY_UNRELATED_CPOINT / RUNTIME_PENDING / B0_OWNER_PRODUCER_ROUTE_4 / LOGIC_AND_PRESENTATION_PRODUCERS_WRITTEN`。同一structural segment的logic-only与presentation `ProcessOneLateOpoint()`现各只写`task.ownerEntityIndex=spawner.OwnerEntityIndex`，不在通用factory推断。精确RED=`2/7`，builds均0 error；Unity 00:09:55 GREEN=`7/7`覆盖self/nonself/sentinel、kind/type/single/multi/two-hop/holder/raw backing及built-in `-1`。00:11:22 SelfCheck被更早CPoint阻塞，未到presentation新增断言；state9996/built-in维持`-1`，DAT weapon_piece归B7，8/9/13独立。下一route 5 owner producer exit audit。

> **当前最前置实施包（2026-09-08）：** `NTSD28-B0-F8-OWNER99-PRODUCTION-001 / FOCUSED_TEST_PASS / UNITY_RUNTIME_COMPILE_PASS / OUT_OF_PROCESS_COMPILE_PASS / UNITY_FOCUSED_3_OF_3 / SELFCHECK_REACHED_UNRELATED_CPOINT_AFTER_NEW_CHECK / RUNTIME_PENDING / B0_OWNER_PRODUCER_ROUTE_3 / MODE2_MATERIALIZER_OWNER_ONLY / PHYSICAL_F8_EFFECT_WIRING_EXCLUDED`。既有`Mode2Request==1 -> SpawnMode2RandomWeapons()`现只在factory前写task owner=`99`，由route 2 initializer发布claimed entity runtime；独立raw backing保持`-1`。精确RED=`1/3`，runtime/editor builds均0 error；Unity 23:25:56 GREEN=`3/3`覆盖slot50/399、四/六次RNG、frame/位置和normal owner`-1`。23:27:20 full SelfCheck通过本包检查后仍停在较后的既有CPoint throw-Vz。正式物理F8 pending consumer、Play/joint trace仍待验；下一route 4 ordinary OPoint owner propagation。

> **当前最前置实施包（2026-09-08）：** `NTSD28-B0-DIRECT-ENTITY-SELF-OWNER-PRODUCTION-001 / FOCUSED_TEST_PASS / UNITY_RUNTIME_COMPILE_PASS / UNITY_FOCUSED_15_OF_15 / SELFCHECK_BLOCKED_BY_UNRELATED_CPOINT / RUNTIME_PENDING / B0_OWNER_PRODUCER_ROUTE_2`。direct App/bootstrap已在ModuleBind前声明required/self slot，adapter按Authority只接受physical slot `0..19`，actual slot不匹配时统一reset/recycle并跳过roster；stage task携带owner=required slot，各entity OPoint initializer在首次注册前消费explicit owner，stage/results-reserve不再尾部覆盖-1。runtime `0 error / 47 warnings`、Editor进程外compile `0 error / 104 warnings`；重启后的Unity于22:51:33刷新程序集，纠正active entity runtime/raw backing测试边界后v1实际`15/15`通过。22:52:37 full SelfCheck在更早的既有CPoint throw-Vz断言停止，Play/joint trace仍待验。F8、ordinary OPoint、state9996及其他owner writer不并入。

> **route 4更正边界（2026-09-08）：** ordinary frame OPoint传播parent literal owner；hit_Fa5/6 owner与target +3F8独立。built-in OID999 weapon fragments及state9996保持owner -1，只有DAT `<weapon_piece>`继承source owner。Unity正式late OPoint producer缺task owner；DAT weapon_piece尚无parser/materializer，仅有pass skeleton，完整实现归B7；hit_Fa8/9/13继续独立。B0 route 4不得在factory中按parent做全局推断。

> **下一route 3只读边界（2026-09-08）：** Authority F8在`GameSession28::step()`完成战斗tick后的function-key tail消费pending，并将`NativeFunctionKeyDropSpawn28.owner_slot=99`原样交给`spawn_at`。Unity正式F8目前只写`FunctionKeys.PendingObjectCommand`且没有生产consumer；现有`Mode2Request==1 -> SpawnMode2RandomWeapons()`来自legacy diagnostic latch并缺owner。route 2 focused gate现已满足；route 3只允许在该既有materializer的factory调用前写task owner=99，保留`requiredRuntimeSlot=-1`的lowest-free分配且不做post-register fix-up，并验证claimed active-slot runtime/raw-trace projection，独立raw backing保持`-1`；physical F8 effect wiring继续归B8，`RunNormalDrop`继续默认owner -1，用户保留的candidate/RNG/position不得改变。

> 决策 ID：`GOVERNANCE-NTSD28-LOGAN-AUTHORITY-MIGRATION-001`  
> 生效日期：2026-09-02  
> 状态：`USER_CONFIRMED_BUGFIXED_IDENTITY / ACTIVE / PROMOTION_VERIFIED / B3_ALIGNMENT_IN_PROGRESS`

> **最高优先级恢复结论（2026-09-04 用户确认）：** 用户说明其发现并修复了 NTSD 2.8-Logan
> Bug，并明确要求继续处理。指定根当前 `NTSD2.8-Logan.exe` 的
> `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033` 与当前82-file playable
> C++/header closure manifest `39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109` 已正式晋升为唯一权威。
> 旧 `1277B70B...DAF75` / `C59BD8D3...2D75` 只保留为历史基线；不得再驱动实现。由于新版改变了
> core pass 顺序，B0～当前B3受影响结论必须显式重新核验，不能只替换哈希后继承旧证书。

这是任何新任务、上下文压缩恢复、交接或历史文档检索后必须首先读取的唯一权威入口。
若其他文档、旧 Change Record、旧 Handoff、测试名或注释与本文件冲突，以用户当前明确要求和
本文件为准；不得沿用 NTSD 2.4 的结论继续修改 Unity。

## 1. 当前唯一战斗行为权威

- 根目录：`J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan`
- 正式发行 EXE：`NTSD2.8-Logan.exe`
- EXE SHA-256：`B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`
- EXE FileVersion：`2.8.3.3`
- EXE ProductVersion：`2.8.3.3-development`
- EXE OriginalFilename：`Ntsd28Playable.exe`
- 正式启动器：`Start_NTSD2.8-Logan.cmd`
- 对应源码声明：`source\README_SOURCE.md` 明确说明 `source\` 是当前发行 EXE 对应的 C++ 源代码快照。
- 当前 playable C++/header closure manifest（82 files）：`39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109`。
- 当前 authority source-capture manifest（75 files，规则相关core/session/scenario子闭包与全部headers）：
  `07CD47A0623F23D2C439E0E85EABF2ED10F8EAE8FC7D70DDB8396C704B3D778F`。
- 先前 drift 审计记录的 `5F2E5B41...5FA9` 是旧workspace捕获脚本遗漏
  `kind_catalog.cpp`、`minibar_catalog.cpp` 后计算出的73-file子集，不能再称为完整capture manifest，更不是
  82-file playable闭包。
- 唯一 runtime 资源根：`resources\runtime`；正式启动器同时把 `--resource-root` 和
  `--complete-vfs-root` 指向该目录。

裁决优先级如下：

1. 用户在当前任务中的明确要求。
2. 上述固定 SHA 的正式 `NTSD2.8-Logan.exe` 的实际可观察行为。
3. `source\README_SOURCE.md` 声明对应正式 EXE、且实际进入 playable 构建闭包的源码。
4. 正式启动参数及 `resources\runtime` 中被正式 EXE 消费的数据。
5. 新权威目录中的 tests、diagnostics、候选 build 和研究记录，仅可作辅助证据；不能覆盖正式 EXE。
6. Unity 当前实现、self-check、旧 trace、旧对齐结论和历史文档只能作为待重新核验的实现或证据。

源码重建输出 `source\ntsd28_playable\build\Ntsd28Playable.exe` 不会自动覆盖根目录正式 EXE。
因此“源码可编译”或“候选 EXE 行为”不能自动晋升为正式发行行为；任何晋升必须由用户明确确认，
并更新本文件中的正式 EXE 指纹。

2026-09-04 以前锁定的 `1277B70B...DAF75` EXE 和 `C59BD8D3...2D75` source manifest 已被
`GOVERNANCE-NTSD28-AUTHORITY-PROMOTION-002` supersede。它们仍可用于定位这次 Bug 修复改变了哪些合同，
但不再裁决当前规则。

## 2. 当前源码恢复入口

| 领域 | 当前入口 |
|---|---|
| 正式 host 与每步调用 | `source\ntsd28_playable\src\game_session.cpp` 的 `GameSession28::step()` |
| 战斗主 tick 与 pass 顺序 | `source\ntsd28_core\src\simulation\simulation_tick_driver.cpp` 的 `SimulationTickDriver28::step(...)` |
| World、实体、关系与生命周期 | `source\ntsd28_core\src\simulation\battle_world.cpp` 的 `BattleWorld28` |
| 帧状态与帧运动 | `source\ntsd28_core\src\simulation\frame_machine.cpp`、`frame_motion.cpp` |
| 物理积分 | `source\ntsd28_core\src\simulation\physics_integrator.cpp` |
| 碰撞候选与命中消费 | `source\ntsd28_core\src\simulation\hit_candidates.cpp` 及 `battle_world.cpp` 的消费路径 |
| 输入路由与 AI | `source\ntsd28_core\src\simulation\input_routing.cpp`、`native_ai.cpp` |
| 对象生成与 OPoint | `source\ntsd28_core\src\simulation\object_spawning.cpp` 及 `battle_world.cpp` 的 tick 尾部 |
| 逻辑表现快照 | `source\ntsd28_core\src\rendering\render_snapshot.cpp` |
| 正式 D3D11 表现与插值 | `source\ntsd28_playable\src\d3d11_renderer.cpp`、`presentation_interpolation.cpp` |
| 战斗结果流程与程序入口 | `source\ntsd28_core\src\simulation\battle_flow.cpp`、`source\ntsd28_playable\src\main.cpp` |

这些文件只是入口。处理具体行为时仍须沿调用链追到字段定义、读写者、前置条件、分支顺序、
RNG、slot 生命周期和最终可观察副作用，并确认文件实际进入 playable 构建闭包。

## 3. 已观察的新基线；Unity 尚未据此改动

以下是对当前权威包的只读观察结果，不等同于 Unity 已经对齐：

- `resources\runtime\decoded_dat\data\system.dat`：正常 `fps_value: 33`，F5 快速模式
  `fps_value_f5: 3`。因此旧文档中的“权威固定精确 30 Hz / `1f / 30f`”不能继续作为
  NTSD 2.8-Logan 的规则结论；Unity 当前 `SIM_DT` 状态必须在后续独立任务中重新盘点。
- 引擎 profile 使用 `maximum_slots=1000`、物理 frame id `0..999`，transient allocation
  范围 `50..999`。这是新权威观察事实；用户已经明确要求容量模型继续使用 Unity 现有
  profile/动态逻辑容量，因此容量本身不作为待修差异。物理 slot 的扫描顺序、identity、复用、
  birth visibility 和生命周期语义仍须按新权威对齐。
- 当前权威存在两个 RNG 流：MSVCR80 CRT 流，以及同步的 3000-byte table 流；不得把旧单流
  假设直接移植为新结论。
- playable 支持 30/60/120 render FPS，正式启动器请求 120；表现插值不能反写逻辑状态。

这些基线会使旧 NTSD 2.4 的 pass、timing、slot、RNG、frame、碰撞、输入、生命周期、render
handoff 和“已对齐”结论全部进入 `REBASELINE_REQUIRED`。在完成新权威源码闭环和必要运行证据前，
不得把旧验证状态直接继承为 NTSD 2.8-Logan 的完成证明。

## 4. 历史权威的废止边界

以下内容已被本决策废止为“当前行为权威”，只允许用于历史比较、迁移线索或回归夹具：

- `J:\QQFile\NTSD2.4\ntsd_release`
- `ntsd_new.exe`
- `src\entity\game_tick.cpp::game_tick(...)` 及该旧工程的 release live path
- `J:\QQFile\NTSD2.4\ntsd_release_C#`
- 基于上述旧权威形成的 Authority400、旧三方 trace、旧 Change Record、旧 Handoff、旧
  “VERIFIED / CLOSED / 已对齐”结论

旧 C#/NTSD 2.4 C++ 对齐 campaign 文档已按用户要求完成审计并由用户从工作树删除。它们不属于
当前恢复、决策、实现、验证或 Change continuation 的输入。Git 历史中的旧路径也不得被解释为
当前实施指令，不得因为上下文压缩、搜索命中或旧状态名而恢复。

## 5. 内容权威和当前用户范围决定

**当前覆盖条款：** D-023 替代下方历史条目中 DAT/角色图片的“Direction B 正式值权威/策略未定/只读限制”。
正式目标是 NTSD 2.8-Logan；已部署 Unity 内容尚未迁移。下文历史数量和可达性统计只描述旧内容，必须按内容指纹区分。
音频、非角色图片、已有例外与默认 stage.dat 部署暂缓不因 D-023 自动改变。

- 本次只迁移战斗规则、逻辑顺序、字段语义、时序、生命周期与可观察行为的权威。
- `GOVERNANCE-S0-UNITY-CONTENT-AUTHORITY-DIRECTION-B-001` 已冻结的 Unity
  `Assets/NTSD/Config` 内容数值权威当前仍有效，但用户已明确要求处理 Unity 与新权威的
  内容、数值和资源差异。整体切换、只补缺失或分类权威策略尚未决定；决定前只允许只读内容
  inventory，不授权覆盖 DAT/PNG/WAV、Prefab、Scene 或 importer。
- 用户明确要求处理旧 `NTSDSpec`（来源为旧 `LF2_19 properties.js`）；必须先用 2.8 live path
  替换所有生产调用，再决定是否删除空壳。
- 用户明确保留 Unity 的 Slot 容量模型、头顶血条、FootSelf、移动端底部黑区/平台取景、
  多边形战斗边界、当前随机掉武器路径和固定世界相机。
- 用户明确排除完整原生 HUD、结果页与战斗内结果信息表现、背景多层/cycle 和完整原生选择流程。
- 上述保留项会产生可观察差异，因此最终只能声明“非例外战斗域完全对齐，并保留用户批准例外”，
  不能声明整个应用逐像素无差异。
- 本次只修改文档和治理恢复入口，不修改 C#、Scene、Prefab、DAT、资源、ProjectSettings、
  C++ 权威目录或任何运行行为。
- 旧的 30 Hz、400-slot、单 RNG、pass 顺序和已完成状态只被标记为待重新核验；不在本次文档迁移
  中直接改 Unity 实现。

### 5.1 Direction-B current corpus 的强制读取口径

当前Unity DAT同时存在单行与多行WPoint/CPoint/ITR subblock。任何只匹配
`wpoint: ... wpoint_end:`等单行形式的raw grep都会系统性漏计，不能作为运行时reachability、dormancy或
“唯一witness”证据。B6已由`NTSD28-B6-DIRECTION-B-MULTILINE-CORPUS-CORRECTION-AUDIT-001`纠正该问题。

后续current-content行为计数必须：

- 使用Direction-B冻结的`artifacts/diagnostics/CAP-S0-1-unity-present-content-authority/normalized-projection.tsv`，
  或使用与正式`Lf2DatDecryptor -> Lf2DatParserV2 -> Lf2DatConverter`等价且覆盖单行/多行block的解析；
- 用当前`Assets/NTSD/Config/data.txt`区分indexed production definitions与138-DAT全manifest；
- 分开记录raw explicit property与converter后runtime value，尤其CPoint front/back旧alias；
- 与release decoded corpus比较时保持同一domain、primary/all-record和indexed/all-file口径。

held/refill reachability还必须合并两类正式relation producer：ITR kind2 ground pickup与OPoint kind2
direct-link。只统计前者得到的39个source definitions不是完整held domain。自
`NTSD28-B6-HELD-RELATION-PRODUCER-DOMAIN-CORRECTION-AUDIT-001`起，current完整union为41个source
definitions / 668个source-target edges；所有WPoint branch与missing-action join必须在该edge domain上计算，
不得把pickup target笛卡尔积或OPoint target全集无条件套到无关source。

B6还确认当前playable没有独立于current DAT frame的可变武器状态。non-character `hit_Fa`、physics、hit、
landing与pickup均从实体当前action对应frame读取state；Unity `NTSDEntityRuntime.WeaponState`及其
`1002→2000→3000`、Vx-halving prelude是旧迁移遗留，不能覆盖actual frame state。Direction-B与release均以
OID124 action40..55的16帧`state1002/hit_Fa12`循环证明该差异可达；详见
`NTSD28-B6-LEGACY-WEAPON-STATE-OWNER-AUDIT-001`。动态behavior/producer应先退休，carrier删除及
snapshot/checksum schema迁移仍须与`ReleaseTick` disposition共同取得用户方向。

non-character目标还必须区分三个physical-slot字段：+0x354 owner/credit、held DVX专属+0x2F8 excluded-group
source与hit_Fa专属+0x3F8 cached/preassigned target。Unity现有`PickerStableId`底层int可复用为+0x3F8，
但generic `LF2Entity`当前错误把target读写到`OwnerSlotIndex`，会污染attribution；不得把表面仍能追踪视为
等价。current 206个generic common frames、OID219 hit_Fa5→4与9条hit_Fa3→7 next链均证明可达。完整owner
与三包顺序见`NTSD28-B6-OBJECT-AI-TARGET-3F8-OWNER-AUDIT-001`；+0x2F8 consumer必须依赖target carrier/
producer闭合，不能继续复用Spawner或Owner。

OPoint kind2、kind5 substitution及type3 linked-owner也只使用上述reciprocal relation字段；当前playable没有
Unity `TrackerFlag`或managed `TrackerParent`第二层关系。Unity两个factory对current62条OPoint kind2额外写
flag/cache，正式shared kind5 consumer则已按link解析并绕过旧raw readers。动态producer和旧consumer/cache应分步
退休；删除runtime field与base-shell snapshot handle属于与ReleaseTick/WeaponState相同的联合schema方向问题。
详见`NTSD28-B6-LEGACY-TRACKER-RELATION-OWNER-AUDIT-001`。

Unity `GrabbedBy`也不是Authority的第二relation field；它只是旧signed mirror。current62条
OPoint-kind2会写child `-1`，但shared117条ITR-kind2完全不写，且唯一gameplay readers是两个
raw-kind5旧duplicate。该carrier进入runtime snapshot和ECS fingerprint却不进checksum/parity，不能作为
canonical或compat identity。后继先复用tracker consumer退休，再退nonzero writers；field/ECS/snapshot删除
 并入ReleaseTick/WeaponState/Tracker/HolderCopySlot联合schema方向。详见
`NTSD28-B6-LEGACY-GRABBEDBY-RELATION-MIRROR-AUDIT-001`。

Unity `HolderCopySlot` 更不是一个可绑定的Authority字段；它把direct linked parent、OPoint root、
legacy damage-stat credit、type3 relation copy和stage/self slot混在同一carrier。Authority对这些语义分别使用
`linked_parent_slot`、`owner_slot`、`battle_group`、`control_slot_000`或physical world slot。审计时Unity frozen
pair/BruteForce/HitPlan曾误以HolderCopy解析linked holder；该consumer binding已由
`NTSD28-B5-KIND5-LINKED-PARENT-SLOT-CORRECTION-001`纠正为HolderStableId/implicit-zero。type3 actual/HitPlan
仍额外传播HolderCopy，因此whole-continuation继续只保留core子集，下一步必须退休type3 extra writer。原审计见
`NTSD28-B6-LEGACY-HOLDERCOPY-MULTIPLEXED-SLOT-OWNER-AUDIT-001`。

Unity `KillCount` 同样不是Authority统计或owner字段：它混合了`ordinary_credit_gate_2f4`、AI
`owner_slot`、revival资格以及legacy state501/11xx child扫描。Authority damage累计使用
`input_hp_consumed_total +0x34C`、`input_score_total_348`、`knockout_count_358`，native combo显示另用
`combo_hit_count_1e0/combo_hit_last_tick_1e4`；Unity `ComboCountAtk/Vic`、`KillStat`与world
`DamageStats/KillStats`均不得继续作为并行真相。严格顺序先回到B2 AI、B3 child、B4 revival，再修B5
`+0x2F4`与旧stats；carrier删除必须将roster/results schema1→2纳入既有13/20/23联合方向。详见
`NTSD28-B5-LEGACY-DAMAGE-STATS-OWNER-AUDIT-001`。

既有`NTSD28-B0-OWNER-SLOT-BINDING-001`只验证trace字段绑定，不代表Unity production已写对`+0x354`。
正式session direct/stage实体的owner是自身physical slot，ordinary OPoint/native object-AI/weapon-piece child继承
source owner，F8固定99，state9996 clone默认-1。Unity当前缺direct/stage self、ordinary OPoint传播与F8 99，
且generic hit_Fa仍把OwnerSlot当`+0x3F8` target cache；必须先deconflict target再补producer。B2 AI虽已读
`OwnerSlotIndex`，在本前置闭合前仍不能称formal runtime一致。详见
`NTSD28-B0-OWNER-SLOT-PRODUCTION-OWNER-AUDIT-001`。

最前置target去混淆包`NTSD28-B0-OBJECT-AI-TARGET-3F8-DECONFLICTION-001 / FOCUSED_TEST_PASS / UNITY_RUNTIME_COMPILE_PASS / 7_OF_7_PASS / SELF_CHECK_BLOCKED_BY_UNRELATED_CPOINT / RUNTIME_PENDING`已复用既有
`PickerStableId`底层int作为canonical +0x3F8，只迁移Authority已闭合的common/4/7/11与5/6 child；
hit_Fa8/9/13、+0x2F8、持久化schema及raw inactive-slot裁决保持独立。runtime与临时纳入新test source的
Editor生成工程进程外编译均为0 error，且production/runtime已在20:13 Unity assembly reload无CS错误；
当前程序集SelfCheck实际执行后被更早的既有CPoint throw-Vz断言阻塞；首轮非法category已纠正，v2 Unity
EditMode focused于20:48实际通过7/7。Play与joint trace仍待做，但本包已满足后继owner producer的
Unity compile + focused前置。下一包按既定顺序进入direct/stage self-owner；F8固定99与ordinary OPoint
owner propagation仍不得抢先并入。

## 6. 后续恢复强制流程

1. 先读本文件、`Assets/NTSD/Docs/ntsd28-logan-vs-unity-battle-alignment.md` 和根
   `AGENTS.md`，再读目标模块文档。
2. 检索到 `NTSD24_AUTHORITY_SUPERSEDED` 时，只把该文档当历史证据。
3. 从第 2 节最接近问题的当前入口追踪正式 build closure 和完整调用链。
4. 明确区分“新权威已观察”“Unity 当前状态”“推断”“未知”和“用户确认”。
5. 从新对齐总表选择具名差异 ID；新建独立 Task/Change 后才允许修改 Unity 脚本。
6. 没有新权威证据的行为保持 `UNKNOWN / REBASELINE_REQUIRED`，不得沿用旧结论补写。
> 当前BATCH-04/Q07已完成`NTSD28-Q07-PORTABLE-OBJECT-CONTENT-STAGING-001`的资源字节暂存：正式runtime清单1343文件/46,594,829字节逐项源和目标SHA一致、无漏/额外文件；仅新增`Assets/NTSD/Content/LoganRuntime`，GameConfig仍为空根、旧资源和Scene未改。此为`VERIFIED_STAGING_ONLY`，未证明Unity候选/发布/Play或Q07出口。下一精确验证staged root的candidate、identity和加载/回退/退出，再决定生产root切换；不得删旧资源、越过stage.dat暂停、使用computer-use或改非战斗职责。详Q07 READINESS.md和PORTABLE-OBJECT-CONTENT-STAGING-001 Task/Record。
> 当前BATCH-04/Q07 `NTSD28-Q07-WINDOWS-PLAYER-RUNTIME-001`已限定`VERIFIED_PLAYER_CONTENT_BOOTSTRAP_ONLY`：新Windows Mono Development构建0错、1343正式侧载；独立隐藏Player运行exit0/PASS，正式指纹及三owner一致、World4、29400资源退出零残留/借用0/两帧Stopped。Player log有13条旧音频SFX目录缺失，留Q10；生产GameConfig仍空根，Q07正式切换/自然技能表现未验。详WINDOWS-PLAYER-RUNTIME-ACCEPTANCE.md。Q06限定交付保持；禁computer-use、非战斗/Scene/旧资源改动。
> 当前BATCH-04/Q07 `NTSD28-Q07-PRODUCTION-CONTENT-ROOT-SWITCH-001` IN_PROGRESS：已在修改前建立精确Task/Change；仅GameConfig正式内容根单字段与Development Player序列化根验收模式，先证明生产默认来源。Q06限定交付与既有Player显式根PASS保持；Scene/旧资源/非战斗不动，禁computer-use。
> 当前BATCH-04/Q07 `NTSD28-Q07-SERIALIZED-MENU-CALLER-001` IN_PROGRESS：GameConfig正式根战斗Scene Player已验；菜单场景共享该资产，现只在既有Editor Play探针加入读取序列化根的menu caller聚焦回访。修改前Task/Change已建，生产/Scene/旧资源不改；禁computer-use。
> 当前BATCH-04/Q07 `NTSD28-Q07-NARUTO-FORWARD-ATTACK-PHYSICAL-001` IN_PROGRESS：修改前准确Task/Change已建；以正式Naruto DAT hit_Fa285→frame286 OID33为代表，仅扩展既有Editor物理Input System探针，独立Q07请求/结果、正式根/指纹与生成轨迹，生产/Scene/旧资源不改。Q06限定交付及前Q07接入/旧引用审计保持；禁computer-use。
> Q07 Sasuke预览隔离验证：原/隔离副本六项输入SHA一致，Unity EditMode正式PNG 1/1、预览类11/11 PASS；原Editor Scene重载与可见图仍待，不能宣称Q07关闭。详SASUKE-EDITOR-PREVIEW-FORMAL-IMAGE-001/PROGRESS.md，禁computer-use。
> 当前Q08（2026-09-22）：`NTSD28-Q08-NATIVE-TRANSITION-COMBAT-FREEZE-001`已把正式350切场当tick/后继旧战斗核心pass冻结（目标3/3、相邻17/17、SelfCheck PASS）；`NTSD28-Q08-TRANSITION-HOST-TICK-ADMISSION-001`又守住后继driver自动/显式/暂停F2旧World输入与host tick（RED0/1→目标1/1、相邻25/25、SelfCheck PASS），详`artifacts/diagnostics/NTSD28-Q08-RESULT-TRANSITION-HOST-AUDIT-001/`两份ACCEPTANCE。正式EXE/source及Scene SHA不变。AppManager尚未消费普通2/1及28/128/202，101逻辑结果记录、mode4 reserve、真实Play/replay亦未闭；两个子包均RUNTIME_PENDING，Q08/总目标继续，Q06本地出口保持。禁computer-use，保护Scene/非战斗/旧资源。
> Q08 `NTSD28-Q08-ORDINARY-RESULT-SELECTION-HOST-001 / RUNTIME_PENDING`（2026-09-22）：正式普通结果命令2已接主线程AppManager有序卸载→现有Unity选人；临时Menu/Battle Play 3/3、相邻28/28、完整SelfCheck与Ledger通过，旧夹具Preparing失败不是有效RED。原Menu→Battle端到端受BuildSettings空表限制未验；battle-only及28/128/202仍待，Q08未闭，Q06限定出口保持。详`artifacts/diagnostics/NTSD28-Q08-RESULT-TRANSITION-HOST-AUDIT-001/ORDINARY-SELECT-ACCEPTANCE.md`。禁computer-use。
> Q08 battle-only重开只读审计（2026-09-22）：正式`main.cpp`显式启动位与Unity直接Battle Scene测试bootstrap owner不同；Unity现无battle-only MatchConfig位，普通命令2接收者不得根据Menu缺席猜分支。正式`start_selected_battle`首次重建后源码写battle-only=false，既有夹具仅证第一轮；第二轮需新动态见证，再定显式Unity owner/RNG及输入相位合同。详`artifacts/diagnostics/NTSD28-Q08-RESULT-TRANSITION-HOST-AUDIT-001/BATTLE-ONLY-OWNER-AND-SECOND-CYCLE-AUDIT.md`。Q08未闭，Q06限定出口保持。
> Q08 battle-only第二轮见证（2026-09-22）：`NTSD28-Q08-BATTLE-ONLY-SECOND-CYCLE-SOURCE-WITNESS-001 / FOCUSED_TEST_PASS_SOURCE_MODEL_ONLY`已以同SHA正式/隔离`game_session.cpp`、focused build及两次同SHA完整会话确认首次直接重开后配置位false、第二次结果350后进入普通selection/state1，旧World/原死者HP0保持。上条“第二轮需见证”已由此更新；Unity direct Scene重开及正式EXE前端可见第二轮仍待，Q08未闭。详`artifacts/diagnostics/NTSD28-Q08-RESULT-TRANSITION-HOST-AUDIT-001/SECOND-CYCLE-SOURCE-WITNESS.md`。
> Q08 Unity direct Battle Scene结果host（2026-09-22）：`NTSD28-Q08-DIRECT-BATTLE-RESULT-HOST-PROBE-001 / VERIFIED_DIAGNOSTIC_ONLY`隔离真实Scene D3D11 Play1/1，正式内容bootstrap完成；`AppManager [TestBootstrap]`状态MenuMain、无Menu Scene，seed native命令2后五帧旧tick冻结而命令仍待处理。NullGfx 4096限制的前轮失败不是战斗差异。正式首轮rematch与RNG/input连续仍待，Q08未闭，Q06限定出口保持。详`artifacts/diagnostics/NTSD28-Q08-RESULT-TRANSITION-HOST-AUDIT-001/DIRECT-BATTLE-RESULT-PLAY-ACCEPTANCE.md`。
> Q08首轮battle-only重开源见证：正式同SHA playable `game_session_tests`聚焦build0、正式runtime双跑同字节；CRT和输入阶段恢复，默认BGM在恢复后同步RNG抽取一次（site `0x004021E0`）。Unity直接Battle Scene首轮重开host仍缺，详Q08 RESULT-TRANSITION-HOST-AUDIT-001/RNG-PHASE-SOURCE-WITNESS.md。Q08未闭/Q06本地出口保持，禁computer-use。
> Q08直接Battle Scene首轮重开真实Play已得目标RED：formal内容bootstrap后注入结果命令2，旧World 15秒内不重建，XML 1/1目标失败，Scene SHA保持；详Q08 RESULT-TRANSITION-HOST-AUDIT-001/BATTLE-ONLY-REMATCH-UNITY-RED.md。`RecreateWorld()`仅空World且当前direct bootstrap无重入配置，需另建生产Task/Change完整处理有序关闭、map carrier、roster、原生RNG/input和次轮选择。Q08未闭/Q06本地出口保持，禁computer-use。
> Q08直接Battle Scene首轮重开限定验收：显式owner用AppManager十一阶段关闭/map真清后重建同Scene roster，旧native/local RNG与input phase恢复、默认BGM同步抽取一次。正式内容隔离真实Play2/2、修订旧诊断1/1、普通路由3/3、完整SelfCheck PASS；详Q08 RESULT-TRANSITION-HOST-AUDIT-001/FIRST-REMATCH-HOST-ACCEPTANCE-PENDING.md。次轮无Menu选择、自然KO/正式EXE可见行为待，Q08未闭/Q06出口保持，禁computer-use。
> Q08第二轮直接战斗结果 `NTSD28-Q08-SECOND-BATTLE-ONLY-LOGICAL-SELECTION-001 / RUNTIME_PENDING`：正式source二轮同World进入selection/state1；Unity已接显式direct owner第二轮命令2→1，隔离无图临时host目标RED→Play1/1 PASS，普通Menu+Battle相邻3/3 PASS。正式内容Battle Scene两次在图片加载期间OOM，尚无第二轮真实场景、自然KO或选择前端验收；不可把代理测试提升为完整Q08或总目标完成。详Q08 RESULT-TRANSITION-HOST-AUDIT-001/SECOND-BATTLE-LOGICAL-SELECTION-ACCEPTANCE-PENDING.md。Q06限定出口保持，禁computer-use。
> Q08正式场景atlas分配见证：`NTSD28-Q08-ATLAS-ALLOCATION-TRACE-001 / RUNTIME_PENDING`在隔离正式内容Scene单例测得145张2048²页、array 2.43GB超既有512MiB预算而直接用ordered pages；第87页完成/第88页创建OOM，进程外私有字节采样峰值18.0GB、无测试XML或战斗结果。详Q08 RESULT-TRANSITION-HOST-AUDIT-001/ATLAS-ALLOCATION-TRACE-RESULT.md。下一独立资源生命周期/环境诊断，不能用无图代理关闭Q08；Q06限定出口保持，禁computer-use。
> Q08 atlas逐页CPU拼装 `NTSD28-Q08-ATLAS-ORDERED-PAGE-STREAMING-001 / RUNTIME_PENDING`：原145页全缓存/第88页OOM，现仅array被拒时逐页拼装与上传；多页逐像素等价及原atlas绑定聚焦3/3PASS。正式内容真实Scene第118页完成、第119页OOM，私有字节采样峰值18.02→17.40GB，仍无结果XML。已证改善不等于解决；源Texture2D与ordered页Texture2D重叠需独立资源策略审计，详Q08 RESULT-TRANSITION-HOST-AUDIT-001/ATLAS-ORDERED-STREAMING-PARTIAL.md。Q08未闭/Q06出口保持，禁computer-use。

> Q08正式内容atlas预算回退（2026-09-22）：`NTSD28-Q08-ATLAS-BUDGET-SOURCE-BINDING-001 / RUNTIME_PENDING`已让超512MiB预算的Auto发布复用既有SourceTexture2D中央绑定，隔离Unity聚焦4/4、D3D11正式内容Battle Scene第二轮结果1/1且进程exit0，私有字节采样峰值13,242,007,552；早期RED3/4留证。代表像素/顺序和正式EXE画面未验，Q08未闭，BATCH-03/Q06仍`DELIVERED_SCOPED / Q06_LOCAL_EXIT`。详Change Record与Q08 ATLAS-SOURCE-BUDGET-ACCEPTANCE-PENDING.md，禁computer-use。

> Q08正式源纹理相机见证（2026-09-22）：`NTSD28-Q08-FORMAL-SOURCE-PIXEL-WITNESS-001 / RUNTIME_PENDING`在隔离非batch D3D11 Editor NUnit1/1PASS，正式指纹、Auto145页2.43GB超512MiB预算、SourceTexture2D、中央tick34/4命令及960×540图2015非白像素有证；原项目现有Editor无第二进程亦产生同hash PNG/JSON，原NUnit回调结果未落盘，不能报原Editor测试PASS。原项目同时Q07物理Naruto探针首键8次未入FrameInputSet而FAIL，与源纹理像素分开审。Scene磁盘SHA保持；正式EXE像素/全角色与技能未验，Q08/总目标未闭、Q06本地出口保持。详`artifacts/diagnostics/NTSD28-Q08-FORMAL-SOURCE-PIXEL-WITNESS-001/ACCEPTANCE-PENDING.md`，禁computer-use。
> Q09全局命中闪光内容边界（2026-09-22）：正式playable以`resource.dat` index43取`vfs/sprite/UI/SPARK.png`、`system.dat` 99×79与1px gutter生成spark矩形；Unity当前仍从旧`Assets/NTSD/Sprite/UIPanels/SPARK.bmp`建20帧旧布局，正式全局PNG未进暂存LoganRuntime。已静态确认资源/切片首差，未改资源或生产代码、未作画面验收；按Q09先列exact owner和接入清单，不重做Q06 hit/spark事件。详`artifacts/diagnostics/NTSD28-Q09-GLOBAL-SPARK-RESOURCE-BOUNDARY-001/REPORT.md`。原项目Editor仍活，当前Editor程序集早于Q08新测试；Unity CLI对该项目返回`STATUS_NO_INSTANCES`（未装Pipeline），不能当成已编译/已跑。
> Q07 三张 `c/custom` 正式 PNG 暂存分类纠正（2026-09-22）：SHA 逐项一致，暂存 1015/正式缺 240；正式 `<menu_face>` 调用链仅证明它们是角色选择头像层，不是 Q09 战斗表现已对齐证据。原项目、菜单逻辑均保持；详 Q07 `CHARACTER-LAYER-PNG-STAGING-001` 的追加纠正。
> Q09 正式战斗名牌 `WORDS0`～`WORDS5.png` 六图仅完成原项目精确字节暂存（2026-09-22）：正式/暂存 `resource.dat` 路径及 playable 名牌/HUD读链已核，现PNG1021/正式缺234；Unity仍用旧BMP，正式读图与画面验收未完成。详 `NTSD28-Q09-NATIVE-WORDS-PNG-STAGING-001` Task/Acceptance；不算Q09/R17关闭。
> Q09 WORDS生产状态纠正（2026-09-22）：正式六PNG已暂存，但Unity生产 `CharacterAnimtorManager` 仅Build Shadow+Spark、从未WithWords；“仍用旧BMP”只是API诊断语，不是生产读图事实。当前无WORDS绑定，Q09/R17需要独立发布/画面验收；详 `NTSD28-Q09-NATIVE-WORDS-PUBLICATION-CALLER-AUDIT-001/REPORT.md`。
> Q09 WORDS未闭依赖（2026-09-22）：六正式PNG虽暂存，但当前`LoganVisualContentCandidate`只哈希角色`files/head/small`，全局WORDS不进VisualFingerprint/发布新鲜度；生产也未WithWords。原Editor仍在但Unity CLI无Pipeline返回`STATUS_NO_INSTANCES`，新Q07代码未进入当前Library程序集。下一精确Q09 Task/Change须先纳入全局图身份，再原子发布/画面验收；详 `NTSD28-Q09-NATIVE-WORDS-PUBLICATION-CALLER-AUDIT-001/REPORT.md`。
> 2026-09-22 Q08生产者覆盖当前态：`NTSD28-Q08-NONSTANDARD-KNOCKOUT-EVENT-PRODUCERS-001 / CODE_WRITTEN`。正式五处KO记录调用中，原已接的两条标准命中之外，环境地面接触、负环境恢复与held CPoint三处现也在既有单次统计旁追加逐次事件；无源slot按正式-1/1000记录。原项目Temp-only runtime+Editor离线编译0错、Ledger验证通过；**原Editor编译/NUnit、SelfCheck、同seed/Play/关闭仍未验**。Q08首包也仍未运行验收，Q09 mode70数据驱动/图文与Q10声音未闭。详新增Task/Change/Acceptance；只用原项目，不开第二Editor/computer-use。下方标准命中包状态仍有效但不再代表全部五生产者。
> 2026-09-22 Q08/Q09 `#killtext`寿命首差：正式Session仅在成功载入feed config时，以该配置`times`于core后裁剪事件；`bound`与allowed modes不关裁剪，配置缺失不裁剪，负寿命是no-op。现Unity每tick无条件70仅适配当前选中正式子表，其他内容状态不等价。Q09需同mode DAT身份/新鲜度捕获并首tick前发布可选完整配置，Q08消费寿命；30tick画面窗口另立。只读审计详`NTSD28-Q08-Q09-KILLTEXT-LIFETIME-HANDOFF-AUDIT-001/REPORT.md`；**未改脚本、原Editor无新编译/NUnit/Play证据**。原Editor PID33236仍在且WORDS旧请求无结果，不启动第二项目或computer-use。
> 2026-09-22 Q09 mode击倒提示输入：`NTSD28-Q09-KILLTEXT-MODE-INPUT-001 / CODE_WRITTEN`已从既有mode双DAT已捕获child字节解析完整`#killtext`标量、重复mode/id、七图路径、双sound；缺配置与bound0分离，V2 raw身份仍覆盖该DAT。原项目Temp-only离线runtime+Editor编译0错且新测试入输入，反射直接调用编译后parser核正式值70/40/0,1,4及反例PASS，Change Ledger validator PASS。**原Editor编译/NUnit、图像身份/发布、World有条件裁剪、图文/声音/Play/退出重进仍待**；详Task/Record/Acceptance。仅原项目、不启第二Unity或computer-use。
> 2026-09-22 Q08/Q09 mode寿命消费当前态：`NTSD28-Q08-Q09-KILLTEXT-RUNTIME-LIFETIME-001 / CODE_WRITTEN`。已发布mode child `#killtext`的可选寿命在原项目World首tick前接入；结果后仅配置存在时按该值执行newest-tail裁剪，bound0仍裁剪、缺配置不裁剪，负寿命沿用no-op。reset与core scalar snapshot13/aggregate28/lockstep checksum31/Unity extended和lockstep parity v3已纳入，frozen Authority400 v3保持。原项目Temp-only runtime+Editor离线编译0错、Ledger validator PASS；**原Editor编译/NUnit/SelfCheck/同seed/Play/退出重进仍未验**，R15版本化复核和Q09图文/Q10声音待。详Task/Record/Acceptance；不启第二Unity或computer-use。
> 2026-09-22 Q09行快照名字边界：同一`NTSD28-Q09-KNOCKOUT-FEED-ROW-SNAPSHOT-001`已按正式playable场景入口把非空名字覆盖限定为完整10槽、每项≤10 UTF-8字节且无NUL，并增反例。原项目离线runtime+Editor编译0错，diff check/Ledger通过；原Editor NUnit、同tick及图文画面仍未验。只用原仓库Editor，不用`I:\UnityPreject\test`/FPSTest作本目标证据；Q09/R17及总目标开放。
> 2026-09-23 原项目 Editor 状态更正：本轮核对 Win32 Unity 进程，`I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity` 无运行中的 Editor；先前 PID33236/worker 存活叙述是 2026-09-22 历史快照，不是当前状态。现存 Unity Editor 指向独立的 `I:\UnityPreject\test`（进程 2026-09-21 启动），不得将它用于本战斗对齐的编译或 Play 验收，也不在其运行期间另启原项目 Editor。原项目 Q09 图标测试仍为 `CODE_WRITTEN / OFFLINE_COMPILE_PASS / ORIGINAL_EDITOR_RECOMPILE_PENDING`；继续原项目源码/离线工作，Editor 验收待原项目会话可用。Q10 击倒提示音频生产入口静态首差见 `artifacts/diagnostics/NTSD28-Q10-KNOCKOUT-AUDIO-HANDOFF-AUDIT-001/REPORT.md`；Q10/总目标开放。
> 2026-09-23 Q07原项目Scene入口状态：用户已确认Menu首场景/Battle第二场景，`EditorBuildSettings.asset`仅两条启用Scene记录；原项目Editor刷新后Menu回调到Battle Additive/有序退出回Menu的聚焦Play PASS（`q07-menu-scene-closure-2.json`），双Scene SHA不变。此不代表Player冷启动、Android或Q07全域完成。`I:\UnityPreject\test`是此前独立项目，本NTSD任务不在该项目执行。
