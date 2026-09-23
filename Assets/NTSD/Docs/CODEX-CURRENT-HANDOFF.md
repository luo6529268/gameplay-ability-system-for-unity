> 2026-09-23 用户技能物理输入优先包 `NTSD28-USER-RASENGAN-PHYSICAL-PLAY-001 / FOCUSED_TEST_PASS / USER_SYMPTOM_OPEN`：原项目Battle Scene Play，鸣人OID2 authored action241，首次动作253后按物理J（tick591→592）与第二次253后按物理J（tick1291→1292）均经真实FrameInputSet held/pressed32、KeyJump1转301；两份原始报告已封存。正式DAT动作253有hit_a:300，254无hit_a。两Play均退出，Scene SHA不变，DAT/Scene无改；真实EXE画面截止和用户失败时按键时点仍未对齐，不能称第二问题已修好。第一问题鸣人16:9持续D+K跑距比例局部Play已PASS，全实体D-024仍开放；Q07新包后置。

> 2026-09-23 鸣人持续D+K落地奔跑局部出口：原Editor真实Battle Scene Play一次D/一次K持续过落地PASS，right725/jump730/airborne736/landing756、动作215→215→0→7，3tick精确X Unity11.778944736/2048=正式源码模型7.666666667/1333=0.005751437859画面宽；源模型40tick严格有效但certificate false。旧探针一tick释放/重按误躲2tu，已仅修held变体。DAT/Scene无改、Scene SHA不变、Ledger PASS。仅鸣人16:9此序列，正式EXE可见/其他实体D-024开放；下一优先螺旋丸真实物理J与画面时序，Q07新包后置。详LANDING-RATIO-PLAY.md。

> 2026-09-23 用户螺旋丸窗口优先包 `NTSD28-USER-RASENGAN-WINDOW-PARITY-001 / FOCUSED_TEST_PASS / PHYSICAL_PLAY_PENDING`：原Editor聚焦3/3、正式源码模型/Unity三组各32tick，鸣人action/counter/MP全同；J在场景tick25开始持续2tick两端完成tick26转301，tick26才按两端错过。太晚路径生成OID434运动X在完成tick30源0/Unity550另记。诊断夹具关闭零对象/槽/borrower，生产和DAT未改；物理键入帧、渲染反馈、正式EXE可见时机未验，用户技能问题仍未收尾，Q07新包后置。详FULL-WINDOW-COMPARISON.md。

> 同包原Editor真实Play补试只见changing阶段/陈旧快照，未运行物理探针；stop后新鲜idle。已静态追踪AttackAction.performed→SetAttackActionPressed→跨名jump载体→LocalSimulationFrameInputProvider人类roster采样；下一须稳定Play实测该链与落地长按D+K，不可据静态或受控trace判两项症状已修复。最终Ledger通过、Scene SHA不变。

> 2026-09-23 `NTSD28-USER-CORE-MOTION-OUTPUT-RATIO-001 / FOCUSED_TEST_PASS / RUNTIME_PENDING`：脚本前Task/Change已建并实施，跑/冲刺Writer早乘速度已撤回，角色/非角色共用物理X/Z位置积分及两直接位移按World比例一次性换算。原Editor刷新编译后聚焦4/4、11/11、13/13、4/4、5/5通过，首个共享Type1过滤器0/0系命名空间错误且已重跑正确4/4。尚无完整Driver tick/真实Play；Y、OPoint生成、持有/传送、边界/几何及其他视口待后批。DAT禁止修改，Q07/总目标开放。

> 2026-09-23 D-024下轮优先：已发现当前跑/冲刺局部倍率早于物理摩擦应用，Writer测试通过但逻辑速度不变量未证；若全实体照搬会改变动作读者。先读`artifacts/diagnostics/NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001/INITIAL-INVENTORY.md`的语义审计，定一次性运动出口，并处理已有跑速倍率的迁移/双乘风险；完整tick应同时验画面位移比例、摩擦后逻辑速度与至少一个非角色对象。DAT数据禁止修改，Q07/总目标未闭。

> 2026-09-23 D-024 用户新全局规则：角色、武器、道具、飞行物等所有战斗实体的位移出口改为同条件画面占比一致，不再以正式/Unity同tick原始像素相等为该项出口。保留完整背景固定相机，DAT未经另行要求不得修改。`NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001 / IN_PROGRESS`已有初步词法候选清单和生产路径分类，详`artifacts/diagnostics/NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001/INITIAL-INVENTORY.md`；仍须语义穷尽、分批实现和真实Play，不能宣称全实体已对齐。Training 16:9样本横向2048/1333，纵向及纵深投影1152/730；其他地图/画面比单独量测。现有角色跑/冲刺局部包已经纠正Z倍率，Naruto自然操作、螺旋丸时机及Q07仍开放。

> 2026-09-23 用户此前决定已成有界执行包`NTSD28-USER-FIXED-VIEW-RUN-RATIO-001 / IN_PROGRESS`：保留相机及完整背景，通用奔跑/冲刺实际距离按正式视口1333与当前16:9背景视宽2048比例目标调整，并明确接受与正式战斗位移分叉。用户新增硬约束：除非另行要求，DAT任何数据不得修改。GameConfig/World及共享/旧/Native-ECS Writer通用出口已改；原Editor比例2/2、实际Action215测试先红后绿11/11、地面普通/重载跑13/13通过。首次真实Play在最终Writer修改前长按方向+跳跃过落地三tick仅7像素且转walking；最终Writer后重测合成按键未入FrameInputSet，不能算用户场景通过。正式源码模型两组自然持续按键也在落地后由215转普通行走；正式EXE及用户按键起点仍待对照。螺旋丸技能输入窗口未完，Q07/总目标开放。

> 2026-09-23 R15双端B0 v2诊断采集器限定通过：源码模型kind案例三流双跑相同且九次严格校验通过，Unity端原Editor编译与聚焦7/7 PASS、v2 neutral B0严格3tick有效；旧v1默认/拒绝保持。Unity正式type3/action同场景尚未构建，跨端kind主/B2/B0及正式EXE可见对照未验；当前最早未闭Q07，Q10音频局部成果保持，Q07/R15/总目标开放。

> 2026-09-23 R15新证据：NTSD28-R15-B0-SOURCE-V2-PRODUCER-001 / FOCUSED_TEST_PASS。源码模型kind案例显式B0 v2三tick两次，main/B0/B2逐字节相等，九次严格校验通过；tick1槽1 OID206→213且epoch1→1。默认v1 neutral有效，v1 kind准确拒绝。Unity v2 producer已建包实施中；同场景Unity type3/action与正式EXE可见对照待，Q07/R15/总目标开放。

> 2026-09-23 R15最新：NTSD28-R15-B0-OBJECT-ID-CHANGE-CONTRACT-001 / FOCUSED_TEST_PASS。B0 v2诊断契约聚焦33/33与比较器11/11 PASS，旧双端v1各3tick仍有效，混版拒绝；源/Unity producer仍是v1，kind同场景跨端和正式EXE可见对照未验。下一先接两个v2 producer，再做Unity type3/action场景；Q07/R15/总目标开放。

> 2026-09-23 R15 kind依赖源码模型预检：`NTSD28-R15-KIND-DEPENDENT-SOURCE-PREFLIGHT-001 / SOURCE_MODEL_3TICK_MAIN_B2_VALID / UNITY_PENDING`。正式OID213/action176对OID206/action0同seed无输入三tick双跑逐字节一致，首tick slot1原位206→213/type3、action/latch/previous40、owner0/group1/HP445，主raw和B2严格校验通过、certificate false。B0因为同slot/epoch原位改ID触发当前诊断合同异常并标INVALID；Unity capture尚无初始action且默认type0，不能宣称跨端对齐。下一独立诊断Task/Change先补严格identity-change事件与type3/action场景入口，不动战斗生产，不伪造正式pup.dat不存在的frame40；Q07/R15/总目标开放。详同ID REPORT。

> 2026-09-23 Q07 Player限定出口：`NTSD28-Q07-MENU-FIRST-WINDOWS-PLAYER-001 / VERIFIED_SCOPED_PLAYER_CALLBACK`与`NTSD28-Q07-WINDOWS-PLAYER-MANIFEST-V2-001 / VERIFIED_PLAYER_CONTENT_COPY_GATE`。原Editor编译与Menu/Battle顺序Windows Mono Development构建0错；1371正式侧载文件/46883057字节逐项清单SHA一致。隐藏图形Player两次真实Menu冷启动经组件回调预热正式资源、Naruto OID2、Additive BattleRunning/World2、三发布键一致，有序回Menu且Stopped/池借用0；第二次exit0。首次`-nographics`预热因阴影材质语义FAIL，不计PASS。双Scene及BuildSettings SHA不变。此仅Player回调/内容闭包，物理输入、像素、Android及正式EXE行为未验，Q07/总目标开放；详同ID REPORT。

> 2026-09-23 Q07 Player `NTSD28-Q07-MENU-FIRST-WINDOWS-PLAYER-001 / CODE_WRITTEN`：用户确认Menu第一、Battle第二；只扩展原项目Windows诊断构建和Development Player探针。V2资源门已编译；Player冷启动/回菜单待验，生产和Scene不动。

> 2026-09-23 Q10受控整链证据：原Battle Scene直接Play实际为battle mode0，首个完整致死探针在创建夹具前准确FAIL，原报告已另存。重跑时选定正式KO feed已发布，只在Play-only World切mode1，临时战斗体经真实Driver tick5→6令受击者HP=-10、KO事件0→1、m_join.wav入队，场景音频播放0→1、未预热拒绝0；Editor退出后Scene SHA 9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0不变。此为受控mode1临时战斗体整tick，不证明真实菜单选mode、物理玩家输入、扬声器可听、Player或正式EXE等价；Q10/总目标开放。详NTSD28-Q10-KNOCKOUT-MODE-SOUND-001 Record和scene-natural-ko-audio-probe.json。

> 2026-09-23 Q10真实场景补证：原项目Editor PID173216在保存的NTSD_Battle Scene两次Play（第二次退出重进）中，选定正式两WAV已在封存音频目录，每轮用合成PendingSoundEvent经生产NTSDSoundPlayer呈现，调用播放计数0→2、未预热拒绝0→0；两轮退出后Editor非Play，Scene SHA-256 9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0不变。原Editor聚焦8/8 PASS。此为合成事件的真实场景音频呈现证据，不能替代自然击倒整链、可听设备输出、正式EXE听感或Player/其余cue验收；Q10及总目标开放。详NTSD28-Q10-KNOCKOUT-MODE-SOUND-001 Record和scene-play-audio-probe-first.json / scene-play-audio-probe.json。

> 2026-09-23 Q10最新：原项目Editor PID173216 编译后定向EditMode 8/8 PASS；真实选定LoganRuntime两mode cue已在NTSDSoundPlayer.PrepareBattleCuesAsync中加载，战斗目录封存后各有非空AudioClip，未发生未预热cue拒绝。两正式WAV SHA匹配且GUID各唯一。仅此EditMode夹具的封存预热已验；真实Battle Scene可听、退出重进、Player打包及正式EXE音画行为对照未验，Q10/总目标开放。详NTSD28-Q10-KNOCKOUT-MODE-SOUND-001 Record与editor-sealed-prewarm-result.json。

> 2026-09-23 Q10新证据：原项目Editor PID173216当前编译后，NTSD28-Q10-KNOCKOUT-MODE-SOUND-001 定向EditMode 7/7 PASS；真实选定LoganRuntime中两mode cue进入战斗预热集合，NTSDSoundPlayer将路径解析到Assets/NTSD/Sound/data，现有资源加载器将两正式WAV解码为非空AudioClip。两WAV SHA匹配，meta GUID在Assets各唯一。首次路径分隔符断言失败已修正测试，生产逻辑未改。完整封存预热、Scene可听、退出重进、Player打包及正式EXE听感未验；Q10/总目标继续开放。详Q10包Record与editor-audio-decode-rerun-result.json。

> 2026-09-23 Q10当前进度：原项目 Unity Editor PID173216 路径为 I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity；独立 I:\UnityPreject\test 自09-21已打开，不用于本任务验证。NTSD28-Q10-KNOCKOUT-MODE-SOUND-001 已在原项目编译并完成定向 EditMode 5/5 PASS，正式 mode KO 新事件选择双阵营 cue、同tick位置、无重播及门槛有覆盖；两正式WAV按SHA暂存。首轮断言误计普通命中声的RED保留在Record。预热实际加载、场景可听、退出重进、Player及正式EXE对照仍待；Q10与总目标开放。下方早期 PLANNED/Editor无进程记录为历史快照。

> 2026-09-23 Q10下个独立包 `NTSD28-Q10-KNOCKOUT-MODE-SOUND-001 / PLANNED`：脚本修改前Task/Change已登记，准确范围为战斗tick、Host mode发布、现有战斗音频预热、聚焦测试及两条正式WAV/meta；此时尚无脚本/资源修改。正式同tick新KO追加音频且在tail裁剪前，选定sound1/sound2，Unity当前缺生产与m_join预热、两条Sound/data资源。保留Q06已闭frame-sound、Scene/非战斗/旧Click音频；Player打包、可听EXE与剩余cue后置。详本包Task/Record和Q10静态审计；总目标开放。

> 2026-09-23 08:56+08 原项目当前进度：Unity Editor PID173216（确认项目路径为本仓库）已对Q09图标当前测试重新编译并完成聚焦 job a825513e8f674351b5dc006768d49502，EditMode 1/1 PASS。前一轮 job 44ab32a4831547aa893ba0c8606465ef 因未声明预期BMPLoader错误日志失败，测试修正和两份终态JSON均已留证；生产代码未随之修改。`NTSD28-Q09-KILL-ICON-PUBLICATION-001 / FOCUSED_TEST_PASS` 仅覆盖7槽/3图发布、损坏Temp图保旧代及目录租约回收，图文屏幕consumer、同tick、Play/EXE像素、退出重进仍待。下方“原Editor无进程/旧PID33236活跃”均为不同时点快照；独立I:\UnityPreject\test不作本项目证据。Q09/R17及总目标开放。

> 2026-09-23 08:50+08 当前会话再核对：原项目 Unity Editor PID173216 于08:46经Hub启动，进程路径明确为 I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity，Responding=True；08:46之前“原项目无Editor”的记录仅是当时快照。原项目程序集仍停留2026-09-22 20:19/18:44本地时间，当前修订编译与Q09聚焦测试须以新会话新鲜状态再验。I:\UnityPreject\test 独立会话不作本项目证据。总目标开放。

> 2026-09-23 当前会话：原项目无运行中的 Unity Editor；下方 PID33236/worker 活跃为昨日快照。I:\UnityPreject\test 是独立项目，不作本项目验收。Q09 图标当前修订仅离线编译通过，原 Editor 编译/NUnit 待；Q10 击倒音频静态首差详 NTSD28-Q10-KNOCKOUT-AUDIO-HANDOFF-AUDIT-001/REPORT.md。总目标开放。

> Q09图标`NTSD28-Q09-KILL-ICON-PUBLICATION-001 / CODE_WRITTEN / OFFLINE_COMPILE_PASS`：正式3PNG/7槽同代发布代码和无效PNG保旧代反例已写；原Editor10:44Z编译早于10:54Z测试修订，当前Editor编译/NUnit未证。PID33236/worker5、8活跃，bridge超时，勿开第二Unity。图文screen consumer/正式像素、退出待，Q09/R17开放。

> Q09最新：原项目WORDS聚焦1/1 PASS、行快照当前五项聚焦5/5 PASS；首跑夹具失败与修正见Change，正式同tick/图标发布/Scene画面/退出未验。两Change均`FOCUSED_TEST_PASS`，Q09/R17开放。当前原项目PID33236，勿用其他Unity项目或computer-use。

> Q08 `NTSD28-Q08-NATIVE-KNOCKOUT-EVENT-STATE-001 / CODE_WRITTEN`：标准致死逐次事件及单次统计、tail过期、快照/恢复、锁步校验、扩展/lockstep parity v2、reset已接；frozen Authority400 v3投影保持。Temp-only绝对targets原项目离线runtime+Editor编译0错，新测试类型入DLL；原Editor编译/NUnit/SelfCheck/同seed/Play/退出重进仍未验。正式选定mode 70tick为捕获常量，Q09需数据驱动；R15版本化检查待。Q09 WORDS旧TestRunner无结果，不并发发Q08测试/不开第二Unity。详Record。

> Q08 `NTSD28-Q08-NATIVE-KNOCKOUT-EVENT-STATE-001 / IN_PROGRESS`：脚本修改前Task/Change已建且补NTSDBattleTickSystem准确路径，事件生产、单次致死统计、尾部过期、快照/校验/恢复/清理为同一合同；开始接线但尚无通过结论。按runtime slots预热、超额保留事件并计量分配，不任意封顶。Q09图文/Q10声音后置；原Editor WORDS请求无结果，不并发重跑/不开第二Unity。

> 当前Q08→Q09击倒提示链 `NTSD28-Q08-Q09-KNOCKOUT-FEED-CHAIN-AUDIT-001 / STATIC_PRODUCER_AND_CONSUMER_GAP_CONFIRMED`：正式致死统计之外还有持久事件、Session尾部过期、30tick显示窗及mode图文；Unity所检脚本已有统计但未见事件载体、mode读者、击倒行命令。Q08先补事件/时序且不得双加统计，Q09补图文，Q10补声音；均需独立Task/Change与原项目验证。详本包REPORT。WORDS请求已消费无结果，不并发重发；不启动第二Unity/computer-use。

> 当前Q09击倒提示图标内容前置 `NTSD28-Q09-NATIVE-KNOCKOUT-FEED-ICONS-STAGING-001 / VERIFIED_EXACT_CONTENT_STAGING_ONLY`：正式mode `#killtext`选定三PNG已在原项目逐SHA暂存，四新GUID各唯一；现337 DAT/1031 PNG，缺224正式PNG。Unity reader/画面与EXE像素未验，Q09/R17及总目标开放。WORDS原Editor请求已消费仍无结果，禁止并发重发；详本包Acceptance。下方1028/缺227为本包前快照。

> 当前Q09活动INKHUD内容前置 `NTSD28-Q09-ACTIVE-FRAME-HUD-CONTENT-STAGING-001 / VERIFIED_EXACT_CONTENT_STAGING_ONLY`：正式active frame双DAT与七PNG精确暂存，逐SHA、全Assets新meta GUID唯一；现337 DAT/1028 PNG，缺227正式PNG。仅内容就绪，Unity HUD reader/画面及Q09/R17未闭。WORDS聚焦请求已被原Editor消费但结果未产生，禁止并发重发；详Q09两包Acceptance/Run-Pending。

> 当前Q07 `NTSD28-Q07-MODE-COMBO-PUBLISHED-ACTIVATION-001 / COMPILE_PASS / RUNTIME_PENDING`：原Editor PID33236于07:58Z编译并重载晚于本包全部源码/测试的程序集；mode双DAT参与五组件V2身份和发布新鲜度，正式tuple在共用seal前由已发布catalog赋给World。公开seal/reset/tick0 restore focused NUnit仍未运行；旧正式V1 trace固定断言与native比较需R15独立迁移，Q07未闭。Q09 WORDS最终定向测试现已编译但无结果。禁computer-use/新Unity副本。

> 当前Q07 `NTSD28-Q07-MODE-COMBO-INPUT-PROJECTION-001 / CODE_WRITTEN`：只新增正式mode双DAT不可变投影`LoganModeComboInput.cs`+meta；Add-Type编译、正式/暂存指纹一致、反例2/2通过。World/identity/menu/结果页未改；随后另包原子处理联合身份与`ApplyMatchConfig`激活。原项目Editor程序集仍旧、Unity测试未编译，不开新Unity项目/禁computer-use。

> Q07→Q09正式全局spark与combo内容接入（2026-09-22）：原项目精确新增正式`spark`依赖三文件与`mode.dat`、其`ntsd.dat`子表、`combo_hits.png`三文件，逐SHA一致；暂存335 DAT/1012 PNG，默认stage.dat不动。旧SPARK.bmp仍在且Unity生产仍读旧布局；正式combo tuple已暂存但生产World仍默认未启用，先按Q07 `MODE-COMBO-LIVE-TUPLE-AUDIT-001`接内容激活，Q09再做Reader/命令/画面验收；Q06 producer不重做。证据Q07 `GLOBAL-SPARK-CONTENT-ENTRY-001`和`MODE-COMBO-CONTENT-ENTRY-001` Task。

> Q07当前生产根已设：序列化GameConfig.asset=`Assets/NTSD/Content/LoganRuntime`，Battle/Menu共用；正式/暂存catalog及330索引DAT哈希一致，暂存331 DAT/1010 PNG/0 WAV。正式DAT还有74未暂存（背景24+25、其他data18、story/stage7），默认stage.dat部署暂缓；拟将其余73整批复制的预检发现menu/minibar非战斗、音频与背景/故事跨owner，已停止复制并转逐caller/资源依赖审。Unity production C#仅字面读到stage.dat且在另一个StreamingAssets路径，负搜索不代表不可达。原Editor正式内容相机单例已过，Player/Menu整链和全技能仍待。详`artifacts/diagnostics/NTSD28-Q07-PRODUCTION-ROOT-CURRENT-STATE-001/REPORT.md`及CSV。不要按旧“生产根空”重复改配置。

> Q10帧声音入口已在Q06实现；正式330对象DAT路径清单976唯一cue，正式VFS 976在、Unity同路径71在/905缺；71重名WAV整文件SHA全异，PCM 28同/43异、17个时长异。直接Battle Scene的AudioController `AudioList: []`，默认本地缺失cue无该场景clip回退；静态差异不等于已证静音。正式playable优先VFS，但暂存LoganRuntime无WAV，单改读该根无效；需独立确定音频部署或外部VFS。typed帧事件、Player侧载及真实播放待核，WAV未整体迁移。详`artifacts/diagnostics/NTSD28-Q10-FRAME-SOUND-ENTRY-READINESS-001/REPORT.md`及CSV。Q10仍依赖Q07/Q08，只用原项目，禁computer-use/新验证副本。

> Q08 mode 2/3/4 静态消费边界：正式playable在timer350发28/128/202；所检host未消费模式专属命令，仅冻结旧World。Unity同样保留命令并拒绝旧World继续tick；模式专属前端/后续动作仍待正式权威可达入口，不得接普通选人或battle-only重开。详`artifacts/diagnostics/NTSD28-Q08-RESULT-TRANSITION-HOST-AUDIT-001/MODE-SPECIFIC-CONSUMER-BOUNDARY.md`。只用原Unity项目继续，禁computer-use/新验证副本。

> Q08 `NTSD28-Q08-COMBAT-LETHAL-PRECOMBAT-TIMING-001 / CODE_WRITTEN / COMPILE_PENDING`：准确单个EditMode用例已写，完整`RunReleaseTick`中kind0 itr伤害20HP目标，计划验致死tick native timer0、次tick timer1及锁定胜组；原Editor测试程序集仍早于新源码，不能报编译/测试通过。生产、Scene、正式资源与非战斗未改；Task/Record/Ledger齐备，禁computer-use。

> Q08新聚焦见证`NTSD28-Q08-COMBAT-LETHAL-PRECOMBAT-TIMING-001 / PLANNED`：正式playable已有实际Attack致死tick12计时0、次tick13计时1双跑见证；Unity现有Q08用例仅跨tick直接设HP，缺完整tick内部碰撞致死。准确单测试脚本Task/Change已建，生产不改；等待用例写入及原Editor编译/聚焦验收。禁computer-use、非战斗/Scene/资源改动。

> Q08正式PNG源纹理原项目验收（2026-09-22）：原Unity Editor PID33236定向`FormalSourceBindingProducesProductionCameraPixels` NUnit 1/1 PASS；原项目正式fingerprint、SourceTexture2D、145页预算回退、tick33四中央命令及960x540非白像素2012均有JSON/PNG，图已查看，Scene哈希不变。归档`artifacts/diagnostics/NTSD28-Q08-FORMAL-SOURCE-PIXEL-WITNESS-001/original-editor-focused-20260922-1247.xml/.json/.png`。这次运行的Editor程序集早于v2结束标记源码，故仅证明旧测试体通过；v2编译、正式EXE画面同条件及自然技能归属仍待，Q08未闭。禁computer-use。

> Q08第二次battle-only结果状态回访（2026-09-22）：早期两次正式内容Scene在atlas分配处OOM；后续`ATLAS-BUDGET-SOURCE-BINDING-001`修正后，同一个`SecondResultEntersOrdinaryUpperSelectionInSameWorld`正式内容Scene测试1/1 PASS（XML SHA-256 `E70E58EAA8D64150B314F843E8320F97D3CAD5CB3F5317A9A552B692D7692B77`）。这解除该脚本化双结果路径的OOM阻塞，仍非自然KO、原项目Editor或正式EXE可见验收；`SECOND-BATTLE-ONLY-LOGICAL-SELECTION-001`保持`RUNTIME_PENDING`。详本包Task/Record和`UNITY-SECOND-BATTLE-SOURCE-BUDGET.xml`。

> Q08 timer101 consumer回访：正式playable核心在101设置瞬时`result_record_created`，所检GameSession没有消费它创建持久记录，render snapshot以`result_visible`阶段/胜组显示；不要据此新增Unity持久结果记录。native timer/phase及实际render handoff/350出口仍需验。详Q08 RESULT-101-CONSUMER-AUDIT-001/REPORT，P-19/G-08表现例外保持，禁computer-use。

> Q08 mode-4 reserve 专项：正式playable完整GameSession mode-4一存活组timer1/350转202双跑一致；所检BattleConfig无Unity committed-result-reserve同条件入口，不能据源码负搜索删现有RESULT-RESERVE-09补reserve逻辑。详MODE4-RESERVE-CALLER-AUDIT；同条件正式EXE可见结果待证。Q08未闭。

> Q08 `NTSD28-Q08-RESULT-GROUP-CARRIER-AUDIT-001 / NATIVE_GROUP_TIMING_COMBAT_LETHAL_PASS / UNITY_GROUP_RED_4_TIMING_DIRECT_RED_2_FULLTICK_RED_1`：正式playable隔离源码`GameSession28::step()`组六例+计时锁定/80/101/144→350+真实Attack致死tick12计时0/tick13计时1，双跑同输出；隔离Unity组别4例、直接producer计时锁定/过早结果页2例及`RunReleaseTick`恢复组别timer不增1例目标RED（类3PASS/7FAIL）。TeamIds[2]/HadBoth绑定结果UI、schema1快照与checksum；独立原生组合同见CARRIER-CONTRACT，下一Unity同条件自然命中/准确Change。未动生产，Q08未闭、Q06出口保持，禁computer-use。

> Q08 `NTSD28-Q08-REVIVE-LIVES-LIVING-GROUP-001 / FOCUSED_TEST_PASS_ISOLATED`：正式HP0/剩余生命分支已用现有HP2Orig接入；同SHA隔离Unity三例RED 1/3→PASS 3/3，相邻结果seam 2/2+3/3。原Editor/完整SelfCheck/真实战斗及Q08其它结果语义待验；详ACCEPTANCE。Q06出口保持，禁computer-use。

> Q08 `NTSD28-Q08-RESULT-FLOW-SOURCE-MATCHED-001 / REVIVE2_DIRECT_AND_UNITY_FULLTICK_RED_REPRODUCED`：隔离Unity EditMode直接writer与RunReleaseTick第2 tick各1例RED，组1/2、HP0/HP2Orig2实测BattleEndPhase1，正式source对应timer0；均编译并在目标断言失败，证据见CALLER-AUDIT/REPORT与XML。正式端同初态完整driver、其它分支及精确生产Change仍待；Q06出口保持，禁computer-use。

> 当前Q07 `NTSD28-Q07-SASUKE-EDITOR-PREVIEW-FORMAL-IMAGE-001 / ISOLATED_VISIBLE_PREVIEW_PASS`：原/隔离副本六项输入SHA一致，Unity正式PNG 1/1、预览类11/11及现有图形验证JSON/PNG PASS；画面已查看、Scene hash保持。原Editor Reload/现场预览待，Q07未闭。详PROGRESS，禁computer-use。

> Q07 Sasuke旧图磁盘owner回访：退场CSV为修改前快照；当前磁盘Scene旧GUID引用0、精确脚本文字仅保留旧BMP网格测试1处，旧空根DAT动态路径仍可达；Editor未Reload，删授权仍0。详SASUKE-EDITOR-PREVIEW-FORMAL-IMAGE-001/OLD-ASSET-OWNER-REFRESH.md。

> 当前Q07 `NTSD28-Q07-SASUKE-EDITOR-PREVIEW-FORMAL-IMAGE-001 / CODE_WRITTEN / UNITY_MODAL_RELOAD_PENDING`：正式sasu.png importer因RED1024先由独立子Task仅调nPOTScale并1/1PASS；预览PNG alpha、Editor示例/验证、Battle Scene禁用预览GUID及y881已精确写入，Scene仅两字段diff、HUDBg x30/旧BMP/Menu保持。现有Editor外部Scene修改弹窗阻断后续编译/预览图验收，已请用户手动Reload，禁computer-use/第二Editor；详PROGRESS。Q07未闭。

> Q08结果设置stage计数当前权威面修正：正式EXE SHA匹配，但其host只有战斗Scene loop/GUI/smoke及转换边界冻结的LFR回放，已检live path没有mode-4结果设置stage动作。不能继续把“直接取同条件EXE见证”当可执行下一步，更不能把赛前24背景ID写入结果计数；Unity零计数仍仅静态候选。详Q08 RESULT-STAGE-COUNT-AUTHORITY-AUDIT-001/SHIPPED-HOST-SURFACE-ADDENDUM.md；Q08其他项继续，禁computer-use。

> 当前Q07未索引旧`effect/weapon4.dat`静态调用链已审：它不是data.txt OID120的`chars/weapon4.dat`；默认正式/旧索引/手动刷新/正式catalog/Editor补丁未见已知reader，GUID无序列化owner。仅`STATIC_NO_KNOWN_READER`，不证明全局不可达，仍`deleteAuthorized=false`、未删。详UNINDEXED-EFFECT-WEAPON4-REACHABILITY-001/REPORT.md；Q07未闭，禁computer-use。

> 当前Q07 `NTSD28-Q07-WINDOWS-PLAYER-NATURAL-SKILL-001 / VERIFIED_PLAYER_NARUTO_REPRESENTATIVE_ONLY`：Windows Mono Development build-3 0错，独立Player物理L/D/J于tick4/6→frame285 tick8→OID33隐藏tick13/正式ncl.png pic1 tick14，进程exit0/PASS；关闭Stopped/borrowers0、双Scene SHA保持。前build-1编译/前run-2探针方向码错误保留，生产未改。详本包ACCEPTANCE；Q07整批仍IN_PROGRESS，Q06本地出口保持，禁computer-use。

> 当前Q07 `NTSD28-Q07-LEGACY-DAT-IMAGE-RETIREMENT-GATE-AUDIT-001 / VERIFIED_STATIC_RETIREMENT_GATE_ONLY`：旧DAT138中137由旧data.txt直接索引、1个effect/weapon4.dat仍待动态归属；旧索引图片383有保留的legacy/Editor读取，1张sasuke_0.bmp仍是Battle Scene预览序列化引用；174其他图片归HUD/Menu/地图/阴影等独立owner。521行退场门槛表逐项`deleteAuthorized=false`，正式默认战斗仍用LoganRuntime，未删/重绑任何文件。详本包REPORT/CSV；Q07 IN_PROGRESS、Q06本地出口保持，禁computer-use。

> 当前Q08 `NTSD28-Q08-F4-PLAYER-CLOSE-OWNER-001 / VERIFIED_PLAYER_F4_CLOSE_SCOPE`：正式F4的Player关闭效果已接入战斗宿主；两次真实Windows Player物理F4均tick3→3、Stopped/对象池0，第二次进程退出码0；Editor物理键PASS、F4拒绝路由8/8、Scene保持。录像save-pending保护因Unity无对应owner仍待，Q08结果计数和总目标未闭；详本包ACCEPTANCE。下条只读缺口是实施前历史，已由本包局部取代；禁computer-use。

> Q08 `NTSD28-Q08-F4-CLOSE-OWNER-AUDIT-001 / CONFIRMED_PRODUCTION_EFFECT_GAP`：正式playable的F4经录像待保存保护关闭整应用；Unity已有物理键handoff但仅测试诊断消费，生产无关闭owner。先精确Task/Change再接战斗host/Player关闭与有序shutdown验收；不得改为返回菜单。Q07 Scene closure首Scene选择已异步询问，其他可独立工作继续，禁computer-use。

> `NTSD28-Q07-SASUKE-NEEDLE-PHYSICAL-001 / VERIFIED_SOURCE_FORMULA_AND_SCOPED_PLAY_ONLY`：正式Sasuke OID11自然L/D/J两次真实Play通过，tick15帧264四个OID440，四子位置/速度逐项符合正式源码公式、chi.png pic0绑定观察通过；Editor退出、Scene hash/dirty保持、Console error0、Ledger653/15PASS。原EXE同条件tick、命中/完整生命周期/屏幕表现及Q07出口仍待；详ACCEPTANCE，禁computer-use。

> 当前Q07旧图owner清单已复核：52个现存旧路径序列化引用=禁用Editor预览角色图1、Battle HUD/UI 26、Menu UI 14、GameConfig UI 8、地图2、通用阴影1。唯一旧角色图sasuke_0.bmp仍被Battle Scene预览及Editor测试引用，正式sasu.png仅为待验证重绑候选；清单见Q07 OLD-ASSET-REFERENCE-REFRESH-001/SERIALIZED-OWNER-CLASSIFICATION.md。没有重绑或删除，Scene/非战斗保持；Q07仍IN_PROGRESS，Q08 mode-4结果计数仍待权威同条件见证，禁computer-use。

> 当前Q08 `NTSD28-Q08-RESULT-STAGE-COUNT-AUTHORITY-AUDIT-001` 已闭只读边界：正式playable的24有效背景ID进入赛前post-roster菜单；mode-4战果设置stage action在所检playable live path未给出同条件规则。Unity正式根`RuntimeStageCount`静态为0但尚非已证首差，禁止直接写24。下一需正式EXE mode-4同stage/按键与Unity同条件见证；Q08其他有权威证据的子项可先行。详同ID REPORT；Q07仍IN_PROGRESS、Q06本地出口保持，禁computer-use。

> 当前Q07 `NTSD28-Q07-LEGACY-DATA-LAZY-LOAD-001` 限定VERIFIED：GameDataManager初始化不再隐式读取旧data.txt；空根显式加载保留，聚焦EditMode两次1/1、正式完整发布1/1、序列化根menu Play q07-lazy-menu-1 PASS。最终无行为的空覆盖删除后重新编译/聚焦通过，完整发布/Play为此前等效路径。Q08背景计数、Menu Scene空Build Settings、旧资源删除权限仍未闭合；详本包ACCEPTANCE，禁computer-use。

> 当前BATCH-04/Q07 `NTSD28-Q07-NARUTO-CLONE-SPRITE-BINDING-001`限定VERIFIED：正式Naruto自然物理键真实Play的clone tick11 pic999无sprite→tick12 pic1正式ncl.png key(33,1)/79×79/中央binding有效；初FAIL保留、fresh PASS、Editor退出及双Scene哈希不变。仅catalog绑定，像素/排序/阴影留Q09/Q12；Menu Scene空Build Settings首差仍保留。Q07 ACTIVE、Q06不重开，禁computer-use。详ACCEPTANCE.md。

> 活跃依赖`NTSD28-Q07-MENU-SCENE-CALLBACK-PLAY-001`仍BLOCKED于空Build Settings Scene闭包：真实Menu的正式预热/选角/Fight通过，Battle加法加载未达，既有RESULT及负例保持。不能由克隆sprite目录单例PASS代替此出口。

> 当前BATCH-04/Q07 `NTSD28-Q07-PLAYER-COMPILE-GUARD-001`及`NTSD28-Q07-WINDOWS-PORTABLE-CONTENT-PACKAGING-001`限定VERIFIED：真实Windows Mono build errors0、正式侧载1343/1343逐hash、SPARK一致，AI聚焦11/11；详WINDOWS-PLAYER-PACKAGING-ACCEPTANCE.md。GameConfig空根、实际Player运行/生产切换待，Scene保持；四个Odin构建副产物保留，禁清理。



> 当前BATCH-04/Q07 `NTSD28-Q07-STAGED-FORMAL-CALLER-PLAY-001` 限定VERIFIED：真实App及退出后Menu Play正式暂存内容通过，旧/新owner零残留、borrowers0、两帧Stopped；详STAGED-CALLER-PLAY-ACCEPTANCE.md。GameConfig空根，Scene/旧资源保持；下一独立Player打包/根映射、实际Player与生产切换。

> 当前Q07 `STAGED-FULL-PUBLICATION-001` FOCUSED_TEST_PASS：完整正式暂存330对象/906有效图片通过真实Unity解码及manager/data/UI发布，显式调用生产OnDestroy后29,400跟踪资源零残留，job1711ab45 1/1PASS。此前EditMode夹具DestroyImmediate未触发OnDestroy的RED保留；真实Play生命周期/App/menu未验。Player打包与运行时根映射缺口见Q07 BUILD-PORTABILITY-AUDIT；GameConfig仍空根、Q07未交付。

> Q07下一子包`NTSD28-Q07-STAGED-FULL-PUBLICATION-001` PLANNED：用现有隔离owner fixture对项目本地完整正式候选做真实发布/图片解码/回收测试；Task/Change已建，GameConfig仍空根。另须处理构建携带路径后才可生产切换。

> 当前Q07 `STAGED-CANDIDATE-IDENTITY-001` FOCUSED_TEST_PASS：真实Unity EditMode job f0b2e9a7 1/1，正式和本地候选330对象/906有效图片及object/fusion/composite/visual指纹一致；Q01原始1010与有效906区分，旧RED保留。1343资源导入后SHA仍一致，Scene保护哈希不变，Ledger640PASS。GameConfig仍空根，下一精确验证发布/解码/实际caller和退出，再决定切换；Q07未交付。

> Q07下一精确子包`NTSD28-Q07-STAGED-CANDIDATE-IDENTITY-001` PLANNED：仅新建定向Editor测试以真实候选路径比较暂存root和正式root；Task/Change已建立，尚未写测试或运行。生产root仍空。

> 当前BATCH-04/Q07 `NTSD28-Q07-PORTABLE-OBJECT-CONTENT-STAGING-001`仅资源暂存VERIFIED：1343正式文件/46,594,829字节逐hash一致，新根`Assets/NTSD/Content/LoganRuntime`；GameConfig仍空根，旧内容生产未切换。下一对staged root做candidate/identity/加载验收，再声明生产切换Task；旧资源和Scene保留，禁computer-use。详Q07 READINESS.md和Task/Record。

> 当前BATCH-03/Q06 `DELIVERED_SCOPED / Q06_LOCAL_EXIT`：NTSD28-Q06-OPOINT-ZERO-FRAME-SLOT-VISIBILITY-001 VERIFIED，source4/Unity完整tick4+组件1、稳定SelfCheck15:44:25Z、真实Renderer Play3与Q05关闭15:47:21Z、Ledger638/46PASS，Scene SHA保持；详本包ACCEPTANCE与EXIT-RECONCILIATION-001/CLOSED-EXIT.md。下一BATCH-04/Q07 READY，正式DAT/角色图片未迁移，先做只读catalog/parser/引用/GUID清单再精确Task。Q08～Q12/R及例外保持，禁computer-use/非战斗/Scene/未列清单资源改动。

> 当前Q06 DAMAGE-REMAINING-SPECIAL-KINDS-001 限定VERIFIED：source17/source4双跑、实测RED后kind9/15、type3原始Z、state1002同步RNG及缺帧原始绑定已修；四组立即/下一tick PASS，稳定SelfCheck15:12:22Z、真实Scene Renderer18/关闭15:13:27Z PASS，Scene哈希未变。详ACCEPTANCE-FINAL4.md。Q06旧CPoint/DamageWriter代码出口阻塞均解除，下一仅做Q06/R最终回访和范围保持出口审计；未审前BATCH-03仍IN_PROGRESS，Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

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

> 当前准确Task NTSD28-Q06-STATE13-EXIT-TAIL-RETIREMENT-001 / IN_PROGRESS：先六例UnityRED，无生产修改。

> 当前Q06 STATE13-EXIT-TAIL-SOURCE-WITNESS-001 FOCUSED_TEST_PASS / SOURCE6：完整driver双跑SHAE7C23807，state13/action200退出/保持与neutral无新增/RNG/audio，state18正向7粒子。详本包SOURCE-RESULT.md。Unity旧分支候选差异尚待同例RED，不能仅据source直接删除；selfcheck反射slot-counter和virtual N30职责须保护。下一准确Unity Task/Change；CPoint限定VERIFIED，Q06未完/Q07未迁移，禁computer-use/非战斗/Scene/资源改动。

> 当前精确source Task NTSD28-Q06-STATE13-EXIT-TAIL-SOURCE-WITNESS-001 IN_PROGRESS；六代表完整tick含state18正向控制，Unity生产未改；CPoint限定VERIFIED保持，Q06未完/Q07未迁移。

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

> 当前Q06：新NTSD28-Q06-FUSION-COMPOSITE-CONTENT-IDENTITY-001 IN_PROGRESS，准确11脚本预声明，尚未改代码。三端审计发现Unityrawcapture两处仍传object-onlyraw，Parity7合成header点也需同步；新合同保留O，组合C=SHA256(ASCII专用tag+NUL+O/F/S三32byte)，semanticV3及LE64，header显式3分量/新scope；旧V2历史保留但current拒绝。独立Python正式向量C3A7FF5…C37CC4/MFD18D6…008147/projection0FEFD4B968D618FD已存，不是当前runtimeheader。Unity单root明确canonical双root同值限制，nativecapture须用其真实独立两实参；不从ImageRoot猜根。下一按Task实施模型/candidate/Unitytrace/native/Parity和新focused tests，再三端新鲜交叉验证；不得仅改tag。前parser24/freeze14/source4-1737保持；Worldcatalog/carriers/完整融合仍未实现，Q06未完/Q07未迁移/目标ACTIVE，禁computer-use/Scene/资源/非战斗修改。

> 当前Q06：NTSD28-Q06-FUSION-INPUT-FREEZE-001 已限定VERIFIED，准确2新脚本、Unity14/14(job5d8aa21ce3dd47b89fac788cfd9155f6)/CS0/reviewPASS。5原生路径优先级、firstexistinginvalid失败、missing-onlylocked2、byte冻结/高优先级出现失效及fusion专用双hash已证；未接Host/World/全局LoganContentIdentity，不声称端到端融合完成。Parser24/24及source4/1737保持。下一必须精确全局contentidentity/preparedcatalog接线合同：现LoganObjectCatalog只hash角色DAT，需将fusion身份显式纳入且同步source/tool校验后才能activate；明确extractedroot不同于统一runtime根，不能猜。随后独立carrier/schema(190/AIalias/drop/globalfeaturepair，318复用RenderPicOffset)及source4UnityRED完整事务。未改资源/Scene/非战斗，Q06未完/Q07未迁移/总目标ACTIVE；无运行job/build/agent。

> 活跃 NTSD28-Q06-FUSION-INPUT-FREEZE-001 IN_PROGRESS，准确2新脚本，冻结fusion输入及专用fingerprint，全局contentidentity/host接线仍待；禁止错把统一runtime根当extracted根。

> 当前Q06：NTSD28-Q06-FUSION-CATALOG-PARSER-001 已限定VERIFIED / IMMUTABLE_TEXT_PARSER_ONLY，准确3新脚本、Unity24/24(job b1873ce0593347a99062b15d6537fc99)/CS0/独立review通过，未接生产，不重复SelfCheck/Play。父NTSD28-Q06-FUSION-CATALOG-TRANSACTION-WITNESS-001仍IN_PROGRESS，source4/1737证据保持，Unity完整事务未实现。carrier审计确认318复用RenderPicOffset；190独立持久值不可复用Unk338；AIalias/drop在lockedkind换definition时保留，不能始终derive当前wrapper；两fusionfeaturegate是全局且与entityFeatureGate分离。下一建立准确preparedfusioncatalog/contentidentity和carrier/schema任务（详UNITY-CARRIER-ENTRY-AUDIT），声明路径后才改，随后source4UnityRED及完整事务。C12/C25h调度保持；Q06未完/Q07未迁移/总目标ACTIVE；无运行测试/build/agent，禁computer-use/非战斗/Scene/资源修改。

> 活跃 NTSD28-Q06-FUSION-CATALOG-PARSER-001 IN_PROGRESS，准确3新脚本，纯融合目录解析，不接战斗生产；source4父见证及完整融合事务仍待。

> 当前Q06 NTSD28-Q06-FUSION-CATALOG-TRANSACTION-WITNESS-001 IN_PROGRESS / SOURCE4_PASS_UNITY_DATA_CONTRACT_PENDING。正式fusion.dat两record实际load，source4双跑SHAff7083f5…a4d0b4bf/58471bytes，独立可见状态1737checksPASS：record1/2 merge-defuse（record2真实implicit310），HP177拒绝、缺原partner定义分离失败公开state保持。timer0为明确测试干预，不称4500/200自然到期；private suspended无公开读取不称全字节证明。完整following已capture尚未Unity对照。下一先精确Unity carrier/catalog合同：核对display190/revive318/AI-drop/第二featuregate及snapshot/hash，不凭grep加字段；利用现有preparedRuntimeDataCatalog边界，另建精确Task后才实施。Unity生产/资源尚未改，不能只改getter或保留旧partner.Reset假装融合对齐；C12/C25h调度保持已验。前firstBDY两包限定VERIFIED，Q06未完/Q07未迁移/总目标ACTIVE；无运行build/test/agent，禁computer-use/Scene/资源/非战斗修改。

> 当前Q06 NTSD28-Q06-FUSION-CATALOG-TRANSACTION-WITNESS-001 IN_PROGRESS / SOURCE_WITNESS_FIRST。只读审计发现正式fusion.dat两record（7/8→51与10/11→52），Unity硬编码第一条且gate/历史/定义发布/分离Reset/失败前置不同，不能getter-only修复。准确Task/Change只声明新诊断CPP，source4基线计划两record merge/defuse+HP边界拒绝+缺partner定义拆分失败；生产/资源未改。保持已验C12/C25h调度；完整差异见AUTHORITY-GAP-AUDIT。前firstBDY两个包已限定VERIFIED。Q06未完/Q07未迁移/目标ACTIVE；禁computer-use/Scene/资源/非战斗修改。

> 当前BATCH-03/Q06：NTSD28-Q06-FIRST-BDY-NATIVE-FRAME-BINDING-001 与 NTSD28-Q06-FIRST-BDY-NATIVE-COUNTER-CARRIER-001 已限定VERIFIED。source8/222、主8smoke4 before/after/following零差异、joint19/19、SelfCheck04:53:09Z、sameWorld replay4场景8tick（含负action目标释放）、Play8与Q05close04:57:08Z均PASS。Scenechecksum/borrowers2→2、restore4→4/final World slots pools0/Stopped2frames，dirtyfalse/root14/hashBCD1047B…0E9FB6保持。旧B5仅encoded counter错载体精确纠正，原决策/RNG等保持。next唯一恢复入口remaining-live-readers审计：优先只读核对Oid5152FusionScanAll→BattleOid5152RuntimeModule→TryApplyRuntimeIdentity fixed290/112及retained split action/857 gate的完整source事务与已闭职责，再建新Task；尚未改identity生产或建新sourceTask。lateEffects/display/postdisplay/CPointselector及raw3/platform/previousXYZ待办保留。Q06未完/Q07正式DAT图片未迁移/目标ACTIVE；无运行job/build/agent/Play，Editor33236复用，禁computer-use/非战斗/Scene/资源修改。

> 新依赖 NTSD28-Q06-FIRST-BDY-NATIVE-COUNTER-CARRIER-001 IN_PROGRESS，encoded真实计数器RED已确认，准确3文件先登记；父firstBDY仍IN_PROGRESS。

> 当前Q06 NTSD28-Q06-FIRST-BDY-NATIVE-FRAME-BINDING-001 IN_PROGRESS / SOURCE8_PASS_UNITY_PENDING。source8已构建/双跑一致SHA98ca9c2…10a97f8，focused222 PASS；初次私有入口编译失败保留后改公开ordinary入口。formal330全部DAT及catalog.csv哈希匹配：55simple+157encoded/目标全显式max705，encoded攻击者112/800/999skip；不声称正式玩家Bug。下一先准确声明Unity单fixture，以真实CharacterInteraction/shared consumer复建before并比即时/following，不能直接DamageWriter绕过firstBDY。生产两旧binder尚未改；已有B5规则/RNG/hold/manualdamage不重做。前lockedkind/snapshot已限定VERIFIED，Q06未完/Q07未迁移/目标ACTIVE；无运行build/test/agent，禁computer-use/Scene/资源/非战斗修改。

> 当前Q06 NTSD28-Q06-FIRST-BDY-NATIVE-FRAME-BINDING-001 IN_PROGRESS / SOURCE_WITNESS_FIRST，准确Task/Change已建，第一写域仅新source CPP；8分支代表、真实candidate→public hit→following，Unity shared consumer要求已写。尚未改生产/Unity fixture。前lockedkind与mutable snapshot两包已限定VERIFIED，不重做。Q06未完/Q07未迁移/目标ACTIVE；禁computer-use/Scene/资源/非战斗修改。

> 当前 BATCH-03/Q06：NTSD28-Q06-LOCKED-KIND-NATIVE-FRAME40-ADMISSION-001 与 NTSD28-Q06-SNAPSHOT-MUTABLE-DAT-IDENTITY-RESTORE-001 已限定VERIFIED。最终entity-only类型校验保留独立raw原值；joint42/42（b834ddc108714f14ae50ab78d3b76f48）、SelfCheck04:31:47Z、renderer replay2及Q05close04:32:46Z全部PASS，Scene checksum/hash/borrowers2→2、restore4→4/final World slots pools0/Stopped2frames保持。错误raw等同假设及中断失败完整保留，两ACCEPTANCE有准确范围。跨type仍拒绝/weapon派生覆盖与跨Worldepoch未提升。下一回NTSD28-Q06-NATIVE-FRAME-REMAINING-LIVE-READERS-AUDIT-001：FIRST-BDY-NEXT-ACCESS-AUDIT.md已只读定位BattleFirstBodyResponseWriter两个旧binder，先审formal catalog可达域并建精确Task/source约8代表，必须shared candidate入口而非直接DamageWriter；已有B5决策/RNG/hold/manualdamage/早退职责不重做。尚未创建新sourceTask/修改该生产。Q06未完/Q07正式DAT图片未迁移/总目标ACTIVE；现有Editor33236已恢复，禁computer-use/非战斗/Scene/资源修改。

> 当前 BATCH-03/Q06：NTSD28-Q06-SNAPSHOT-MUTABLE-DAT-IDENTITY-RESTORE-001 IN_PROGRESS / CORRECTION_WRITTEN_VALIDATION_PENDING。首次绑定修复13/13 PASS仍为旧版本证据；后新增raw==entity type守卫错误，联合job84364ef670d3421baa4ca9cc3df6c5af最后观测36完成/4FAIL，无最终XML。已按RuntimeSlotTable及Q05独立raw合同纠正为仅entity类型检查，raw保真控制替代错误拒绝断言；当前纠正尚未编译/运行，禁止引用旧13/13称最终PASS。证据joint-interrupted/OBSERVATION.md，独立review已明确撤回raw等同假设。旧Editor62860已退出/6403拒绝连接/Temp结果消失；当前三个Open Project窗口、unity status无实例，项目lock实际共享冲突被占用，禁止第二实例或删lock。下一恢复现有连接后跑focused8+父replay及相关raw/native测试，稳定后一次SelfCheck/代表rendererPlay关闭。父lockedkind仍IN_PROGRESS，Q06未完/Q07未迁移/总目标ACTIVE；禁computer-use/非战斗/Scene/资源修改。

> 当前 BATCH-03/Q06：NTSD28-Q06-SNAPSHOT-MUTABLE-DAT-IDENTITY-RESTORE-001 IN_PROGRESS。精确生产恢复顺序已写；新type0/type3成功例初RED2及晚slot拒绝PASS已存focused-red。修后相关snapshot类+原lockedkind变身前回放13/13 PASS（cfffb63e14c74ea0b4f5eb2d92715aa5），CS0，证据after-binding-fix/results.xml。原EntityIdentityMismatch已不再当前失败，但父NTSD28-Q06-LOCKED-KIND-NATIVE-FRAME40-ADMISSION-001仍IN_PROGRESS，依赖待反例硬化与retained-renderer验收。下一先审snapshot private raw/entity runtime类型一致性，声明精确额外路径再补无修改preflight校验与坏catalog/缺帧反例；禁止Load掩盖坏payload。独立review认可Load顺序，反对ModuleBind/初始化。尚未重跑SelfCheck/Play，待生产稳定一次联合；跨type仍拒绝/weapon子类待证据。Q06未完/Q07未迁移/总目标ACTIVE；保护Scene/HUDBg30和非战斗，禁computer-use。以下旧检查点仅历史。

> 当前 BATCH-03/Q06：NTSD28-Q06-LOCKED-KIND-NATIVE-FRAME40-ADMISSION-001 仍 IN_PROGRESS。source2/67、focused9/9、captured2/2、SelfCheck03:32:42Z、Play4及Q05关闭03:38:12Z已有PASS，证据归档 play-pass-replay-pending/ACCEPTANCE-PENDING.md；原sameWorld变身前snapshot回放FAIL EntityIdentityMismatch，不能称完成。新依赖 NTSD28-Q06-SNAPSHOT-MUTABLE-DAT-IDENTITY-RESTORE-001 PLANNED：expected200/current213被preflight拒绝，且当前帧恢复仍读现有DAT，禁止仅删除检查或清local shell绕过。下一先审FrameCache.Load/控制器副作用并做精确测试，再实现保留同shell/Renderer的原DAT恢复；生产尚未改。稳定身份/类型/失败无修改必须保持。按分支代表验证，复用未受影响证据。Q06未完/Q07未迁移/总目标ACTIVE；禁止computer-use、非战斗、Scene和资源修改。以下较早检查点只保留历史，以本条为准。

> 当前Q06 NTSD28-Q06-LOCKED-KIND-NATIVE-FRAME40-ADMISSION-001 IN_PROGRESS / SOURCE_WITNESS_FIRST。准确Task/Change已建，仅新CPP由cpoint_acceptance编写，尚未build/Unity/production修改。正式kind.dat加载优先decoded路径已确认；同表实际SHA39e30d…00011。indexed绑定8当前type0有40、213type3缺40且7条kind0攻击、209未在indexed行；仅静态不称动态Bug。计划真实catalog213→200 explicit40/implicit40两例，targetcurrent10state3000，完整identity/history/pendingCount/after/following，不能用只变ID或改targetstate0遮蔽snapshot差异。前generic binder已VERIFIED不重做，generic高位projection保持独立后继。Q06未完/Q07未迁移/总目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> NTSD28-Q06-LOCKED-KIND-NATIVE-FRAME40-ADMISSION-001 IN_PROGRESS / SOURCE_WITNESS_FIRST，仅新CPP预声明；正式kind.dat优先路径确认，213缺40且有kind0 ITR，动态transform待见证。生产未改。

> 当前BATCH-03/Q06：TYPE3-TARGET-GENERIC-NATIVE-FRAME-BINDING-001已限定VERIFIED。source11双跑SHA c4d7afdc…d5ebee/独立419；Unity11+smoke5零差异、联合13/13、captured两mode2/2、SelfCheck03:11:12Z、representative sameWorld5场景10重放tick、Play10及Q05关闭03:14:12Z全PASS。Scenechecksum/borrowers2→2、restore4→4/worldslots两pool0/两帧Stopped、dirtyfalse/root14/hashBCD1047B…0E9FB6保持。只修单generic binder，真实pair中间900最终70/71及贡献count1→2正确；highpair投影旧gate未覆盖不称PASS。首次新方法pre-reload空test0已保留不计PASS。下一优先只读审计ApplyNativeLockedKindTransform HasFrame40/oldrawbinder及lockedprojection门，源无显式40要求；报告LOCKED-KIND-NEXT-ACCESS-AUDIT.md含真实catalog加载链、formal decoded kind.dat已观察与fallback同表/SHA39e30d…00011。尚未建下一Task/source或改production；需精确Task和显式/隐式40 wholehit见证，不重做已闭genericowner/attacker职责。generic高位projection后继独立，其他identity/late/display/platform/raw3保留。Q06未完/Q07正式DAT图片未迁移/总目标ACTIVE，无运行job/build/agent/Play；复用Editor62860，禁computer-use/非战斗/Scene/资源修改，按分支代表验证。

> NTSD28-Q06-TYPE3-TARGET-GENERIC-NATIVE-FRAME-BINDING-001 IN_PROGRESS / SOURCE_WITNESS_FIRST，仅新CPP声明11代表含真实后置pairreset；actual binder与projection门分开，生产未改。

> 当前BATCH-03/Q06：TYPE3-ATTACKER-POSTHIT-NATIVE-FRAME-ACCESS-001已限定VERIFIED。source13双跑SHAfb3f06bf…bf0e31/独立214、Unity主13+smoke6零差异、旧11+新2联合13/13、Object captured两mode source字段通过、SelfCheck02:49:00Z、代表sameWorld6场景12重放tick、真实Play12与Q05关闭02:51:18Z全PASS。Scenechecksum/borrowers2→2、restore4→4/worldslots两pool0/两帧Stopped、dirtyfalse/root14/hashBCD1047B…0E9FB6保持。captured初用错Character pass已纠正；type3target3005非匹配pair被既有CanProjectStandardType3DamageWriterEffect明确排除，只证明actual，不称writerprojectionPASS。原失败与准确scope见ACCEPTANCE。下一回remaining-live-readers，先只读审计BattleDamageWriter.ApplyNativeType3TargetGenericContinuation的DirectWriteHeldFramePreserveWaitCounter单caller及target response frame/owned关系完整source事务；与locked kind transform/identity及旧投影HasFrame门区分，避免重做已验owner/清pending职责。尚未建下一sourceTask/改该生产caller。Q06未完/Q07正式DAT图片未迁移/总目标ACTIVE，无运行job/build/agent/Play；复用Editor62860，禁computer-use/非战斗/Scene/资源修改，按分支代表验证。

> NTSD28-Q06-TYPE3-ATTACKER-POSTHIT-NATIVE-FRAME-ACCESS-001 IN_PROGRESS / SOURCE_WITNESS_FIRST，准确新CPP写域，13代表/三live消费者，生产未改。前Kind0/reaction已VERIFIED不重做。

> 当前BATCH-03/Q06：KIND0-POST-EFFECT-NATIVE-FRAME-ACCESS-001及STANDARD-REACTION-HISTORY-NATIVE-READERS-001均限定VERIFIED。source6+reaction2双跑/默认6bytes不变；联合25/25、SelfCheck02:24:27Z、代表sameWorld4场景8重放tick、真实Play10与Q05关闭02:27:23Z PASS，Scene checksum/borrowers2→2、restore4→4/worldslots两pool0/两帧Stopped、dirtyfalse/root14/hashBCD1047B…0E9FB6保持。reaction最终仅实际+投影两个previous reader；snapshot原fallback控制已过不改。父case1为early拒绝，不能称post-effect state18抑制覆盖。准确限制见两ACCEPTANCE。下一回remaining-live-readers审计，优先Type3 attacker post-hit共享selected reader/binder；POST-EFFECT-REMAINING-ACCESS-AUDIT已有定位，先确认standard type0/armored type0/特殊对象当前live消费者及原B5证据，已native unarmored/reduced helper不要重做。尚未建新sourceTask/修改type3生产。Q06未完/Q07正式DAT图片未迁移/总目标ACTIVE；无运行job/build/agent/Play，复用Editor62860，禁computer-use/非战斗/Scene/资源修改，按分支代表验证。

> 当前BATCH-03/Q06：KIND0-POST-EFFECT-NATIVE-FRAME-ACCESS-001 IN_PROGRESS，两访问点已修但依赖未关。source6双跑SHAc01031a3…c2bcae4/独立124PASS；其中5真实hit+1early拒绝（effect20 prev18前置拒绝，不能声称后置18抑制覆盖）。Unity原RED主6即时10/后继12；两访问修后job440cbbd2bef64c5e9f7f49ce15b76b7e旧15PASS/新两矩阵FAIL，仅case0普通reaction剩fall20vs80/action220vs186/pendingX0vs5/Y17vs10，before均0；其余5例零差异。下一唯一Task NTSD28-Q06-STANDARD-REACTION-HISTORY-NATIVE-READERS-001 PLANNED：source CPP增加--reaction的snapshot900state12/state0两例（尚未写），默认6字节保持，之后准确声明普通ApplyStandardFall与投影4reader。父证据production-red/after-post-effect-access保存；完整SelfCheck/Play待依赖修好联合一次。当前无运行job/build/agent/Play；Q06未完/Q07未迁移/总目标ACTIVE，前3包VERIFIED保持不重做。禁computer-use/非战斗/Scene/资源修改，保留HUDBg30。

> NTSD28-Q06-KIND0-POST-EFFECT-NATIVE-FRAME-ACCESS-001 IN_PROGRESS / SOURCE_WITNESS_FIRST。只声明新CPP，生产未改；6代表，保护既有B5规则，前3包已VERIFIED不重做。

> 当前BATCH-03/Q06：EFFECT-OVERRIDE-NATIVE-FRAME-ACCESS、STANDARD-HIT-FALL80-PRESERVATION、STANDARD-HIT-PENDING-Y-PROJECTION三个001包已限定VERIFIED。联合30/30、完整SelfCheck01:55:20Z、代表sameWorld6场景12重放tick、真实Play22与Q05关闭02:00:04Z均PASS；Scene checksum/borrowers2→2、restore4→4/worldslots两pool0/两帧Stopped，dirtyfalse/root14/hashBCD1047B…0E9FB6保持。初次Play测试日志context失败已保留并只修测试入口，未再跑全套。准确scope/限制见各ACCEPTANCE。下一唯一入口回remaining-live-readers审计，先Kind0 post-effect旧previous reader+200/203 binder（报告POST-EFFECT-REMAINING-ACCESS-AUDIT.md已有6代表建议），需新Task/Change及source witness后才改生产；随后type3共享selected reader/binder，但已native的unarmored/reduced helper不重做。Q06未完/Q07正式DAT图片未迁移/总目标ACTIVE。无运行job/build/agent/Play；复用Editor62860，禁computer-use/非战斗/Scene/资源修改，按分支代表验证。

> 当前BATCH-03/Q06：effect native frame access、standard hit Fall80保持、pendingY projection三关联包生产修复已联合30/30 PASS（job d41e706b23d048448436ee57d9f2f0c9，1.777秒），完整SelfCheck 2026-09-21T01:55:20Z PASS；证据各包joint-pass。source3/default dvy0与vertical2非零/截断及effect16+smoke6已验证，旧失败保留。新增仅测试的6代表sameWorld replay与一次Play22合并入口已写，Editor正在刷新编译，尚未运行新增replay/Play；下一先读Editor readiness/CS0，再只跑新增replay与改动的captured tests，之后EffectPlay请求和Q05关闭，不重复完整SelfCheck/旧矩阵。三个Task仍IN_PROGRESS，Q06未完/Q07未迁移/总目标ACTIVE；Scene/HUDBg30和非战斗保持，禁computer-use。

> NTSD28-Q06-STANDARD-HIT-PENDING-Y-PROJECTION-001 IN_PROGRESS；source3原输出保持，--vertical补2边界，尚未改投影条件。Fall80两clear/2oracle已写待新编译验证，effect父等待联合出口。

> NTSD28-Q06-STANDARD-HIT-FALL80-PRESERVATION-001 IN_PROGRESS / SOURCE_WITNESS_FIRST：仅新CPP3代表预声明；effect四访问修后before0/即时1/后继1只余致死Fall80独立clear，生产clear未改，父effect等待本包。

> EFFECT-OVERRIDE-NATIVE-FRAME-ACCESS-001 source16已build/double-run一致SHA52093e69d3e286ddd5e496ab862efb7e35ec385b63b41472fa5b0dda51696f0d，focused独立365检查PASS；仅gate/override/descriptor/历史字段保持及固定injury5 HP，不模拟全hit sideeffects/RNG/following。初版target显式wait误填37（源41）两失败已保留并纠正。真实普通unarmored source正常HP395/致死-4，高位latch900state602/BDY50抑制及previous900state12匹配已触发。下一准确扩Record单Unity fixture，slot0/70完整before及whole标准hit后/下tick对照，先RED再决定normal两reader/两binder。实际标准hurt支持帧180/186/220已在同源DAT显式声明以隔离别的binder。HitPlan身份投影两reader另需真实转换代表，普通16不关闭它；Kind0PostEffect/Type3PostHit仍独立后继。生产未改；Q06未完/Q07未迁移/总目标ACTIVE，无运行build/job/agent/Play，Editor62860复用，禁computer-use/Scene/资源/非战斗修改，按分支代表验证。

> EFFECT-OVERRIDE-NATIVE-FRAME-ACCESS-001 IN_PROGRESS / SOURCE_WITNESS_FIRST：正式effect8字段801条，catchingact/pickedact全0。审计纠正旧getter会拒绝显式857..999；normal latch/previous两reader及两binder需见证，identity projectedDAT两reader另需实际转换证据。source16新CPP由cpoint_acceptance编写中，未build/run/Unity/生产修改。原B5职责保持，Kind0PostEffect/Type3PostHit不混改，测试按代表等价类。前IMPACT已VERIFIED不重跑；Q06未完/Q07未迁移/总目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> NTSD28-Q06-EFFECT-OVERRIDE-NATIVE-FRAME-ACCESS-001 IN_PROGRESS / SOURCE_WITNESS_FIRST：只新CPP预声明16代表，identity投影另需证据，生产未改。旧getter对显式高帧亦拒绝，审计错误已纠正。

> Q06 IMPACT-NATIVE-FRAME-BINDING-001已限定VERIFIED：source20/18285、20+6零差异、联合53/53、SelfCheck01:06:28Z、代表replay12场景24tick、Play12与Q05关闭01:09:41Z全PASS；Scene checksum/borrowers2→2、restore4→4/worldslots两pool0/两帧Stopped、dirtyfalse/root14。单Action caller修复，原B6职责保持。下一回remaining-live-reader审计，优先只读检查BattleDamageWriter.ApplyNativeEffectActionOverride两caller及其正式kind0/weapon/type3消费上下文；旁边Kind0PostEffectAction是独立计数器/朝向事务，须分清顺序再定最小source代表，不批改getter。其它identity/CPoint/lateEffects/display/post-display/platform/raw3保持。Q06未完/Q07未迁移/总目标ACTIVE；无运行job/build/agent/Play，复用Editor62860，禁computer-use/非战斗/Scene/资源修改，按用户分支等价类验证不重复全套。

> IMPACT单caller修后fullSelfCheck 2026-09-21T01:06:28.907250+00:00 PASS已归档after-binding-fix；此前运行中状态由本条覆盖，当前无运行job/build/agent/Play。联合53/53与20+6零差异保持，独立生产review单行通过。下一仅6代表sameWorld replay/Play及Q05关闭，不重复已过矩阵或SelfCheck。Task IN_PROGRESS/Q06未完/Q07未迁移/总目标ACTIVE。

> IMPACT-NATIVE-FRAME-BINDING-001生产单caller已修：source20/独立18285，Unity有效RED主20 before0/即时15/后继5、smoke6 before0/即时3/后继1；Action操作改native binder后20+6均before/即时/following0。联合job5b92ec860e18446cb36b010bef8608cb 53/53PASS(新2+literal3+相关HitPlan48)，约1.42秒，未跑旧B6全量；证据after-binding-fix，原JSON数字tag夹具失败及有效RED均保留。fullSelfCheck请求已提交，现Editor62860执行，结果待新Temp/NTSD_BattleRuntimeSelfCheck.result；不得重复启动/并行Unitytests或C#编辑。下一仅6代表sameWorld replay与Play(Authority/DataOriented/renderer6+Mobile/Legacy/logic6)及Q05关闭，脚本前准确登记Record；无需再跑矩阵/旧测试除非新失败。Task未关闭，Q06未完/Q07未迁移，总目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> IMPACT-NATIVE-FRAME-BINDING-001 source20已build并双跑一致SHA4041ba46914f43292e97058fb9c8687f67482cf384dbcb3590e1d0a0946d6edd，独立18285检查PASS（captured-before→四实体即时after、准入/descriptor/pending/RNG，不独立重建spawn或following）。Build manifest闭包07CD47…778F，源CPP FFBAFC…60D8A。下一准确扩Record新增单Unity fixture，复用正式catalog type与四实体factory，全部spawn后恢复0→1→2 ownerchain及target70 before；额外capture environment/catchSource/impactSource/pendingXYZ/descriptor，sourcepreviousXYZ仍明确source-only；主20代表+必要smoke，不扩乘积。先before0/即时/following分开RED再决定唯一BattleDamageWriter Action caller，生产未改。静态formal respond全0且唯一implicit182 OID899无bdy/itr，不认定实战Bug。前state1218已验不重做；Q06未完/Q07未迁移/总目标ACTIVE，无运行build/job/agent/Play，Editor62860复用；禁computer-use/非战斗/Scene/资源修改。

> Q06 IMPACT-NATIVE-FRAME-BINDING-001 IN_PROGRESS / SOURCE_WITNESS_FIRST。独立审计确认唯一旧binder位于BattleDamageWriter.TryApplyNativeImpact的Action操作；HitPlan.ProjectNativeImpactWriterEffect不读descriptor，旁边kind15不是本包。原B6 I1/I2/I3已验行为保持。formal330 kind10=121/kind11=61/respond全0；type0 target182显式157/隐式1(OID899 spe)，但该对象22frame无bdy/itr，不能认定实战可达差异。20代表source runner由cpoint_acceptance编写，只有新Tools CPP写域，尚未build/run/Unitytest/生产修改。报告remaining审计FORMAL330-IMPACT两JSON及新包UNITY-MAPPING-AUDIT.md。测试按用户等价类原则，不做profile/角色全乘积。前state1218两包VERIFIED保持，Q06未完/Q07未迁移/总目标ACTIVE；禁computer-use/非战斗/Scene/资源修改。

> NTSD28-Q06-IMPACT-NATIVE-FRAME-BINDING-001 IN_PROGRESS / SOURCE_WITNESS_FIRST：仅新CPP预声明20代表，生产未改；正式respond全0且type0潜在隐式182仅OID899，非动态可达证明。

> Q06 STATE1218-CONTACT/AIRBORNE-NATIVE-FRAME-BINDING两包已限定VERIFIED。source480/55双跑与独立模型、联合29/29、SelfCheck00:45:27Z、代表sameWorld40场景80tick、真实Play40与Q05关闭00:48:58Z全PASS。Scene checksum/borrowers2→2、restore4→4/worldslots两pool0/两帧Stopped；dirtyfalse/root14/hashBCD1047B…0E9FB6保持。按用户要求不重复四配置矩阵及SelfCheck；代表限制见ACCEPTANCE。下一唯一入口回NTSD28-Q06-NATIVE-FRAME-REMAINING-LIVE-READERS-AUDIT-001，先只读核对BattleDamageWriter.TryApplyNativeImpact的Action操作与正式resolve_special_relation_hit对应完整事务及已有B6证据，再定最小source witness；禁止重做已闭行为或共享getter批改。其它impact/effect/identity/CPoint/lateEffects/display/post-display及platform/raw3/previousXYZ保留。Q06未完/Q07未迁移/总目标ACTIVE；无运行job/build/agent/Play，Editor62860可复用，禁computer-use/非战斗/Scene/资源修改。

> state12/18 contact+airborne联合29/29及最新完整SelfCheck 2026-09-21T00:45:27.002427+00:00 PASS，证据两包joint-pass。当前无运行job/build/agent/Play；下一仅代表sameWorld replay/Play与关闭，不重跑全矩阵或无新生产修改的SelfCheck。两个Task仍IN_PROGRESS；Q06未完/Q07未迁移/总目标ACTIVE，测试等价类收敛规则保持。

> 按用户分支等价类验证：airborne源55双跑一致SHAa4d7c8fd…52e691/独立24696检查PASS；Unity RED55即时30与smoke8即时7，before/following0，后仅改LF2Entity.ApplyCurrentDatType0AirborneAction单binder。联合job e6d47ce73ce2473180ce73ea5b4eca50已29/29PASS（airborne17旧+contact9旧+新55/8两测试+contact主路径480一个测试），实际8.03秒。新55/8与contact480 before/即时/following均0，证据两包joint-pass。不要重复另外三contact矩阵；已过分支复用证据。完整SelfCheck请求已消费，PID62860当前运行，等新结果勿并行C#改动/Unitytests。下一用代表案例补sameWorld replay与一次Play：contact选覆盖soft/pending/invalid/explicit999/hardmotion的约12例，两条配置路径各验证必要代表；airborne8代表；不要跑旧3840全乘积Probe。代表filter/计数变更前扩Record。两包仍IN_PROGRESS，Q06未完/Q07未迁移，总目标ACTIVE，禁computer-use/Scene/资源/非战斗修改。

> 用户要求收敛重复验证（2026-09-21）：按相关分支/数据等价类选代表案例，不能只按角色名扩矩阵；局部修改先编译+原差异最小案例+受影响边界。未变化已通过证据复用，只有新修改/失败/未决风险才扩测。完整SelfCheck与相关联合回归在一个闭合执行包出口集中一次；Play按实际受影响路径/生命周期选代表，不每改一行都重跑所有配置/角色。Q07内容迁移与Q12完整集成出口不缩减，原失败不得删除/排除来变绿。

> NTSD28-Q06-STATE1218-AIRBORNE-NATIVE-FRAME-BINDING-001 IN_PROGRESS / SOURCE_WITNESS_FIRST，只新CPP预声明；contact两binder修后job45daa31087ba4319ac0fa19b0f9e36fb待终态，不混改生产。

> STATE1218-CONTACT-NATIVE-FRAME-BINDING-001已测RED：job87f3c88664724d3b80bf2501db16de3b终态FAILED4/4，两profile×两路径各480 before0/immediate410/following120；即时392 contact+18 airborne-control，后继120全部contact。production-red四JSON/XML已存；生产未改。下一先精确扩Record仅LF2Entity.ApplyCurrentDatType0State1218ContactAction两个raw binder，修后保持完整480控制不删；airborne18另建独立Task/source见证，不在contact包顺手改第三caller。源480/228481模型PASS，但stepflags/following未独立模型（review泛称flags由模型承担不适用当前脚本，按实际scope）。静态正式330默认目标全声明及19461 ITR无自定义pending动作保持，不把synthetic RED称正式玩家Bug；动态身份域仍待。前ordinary/candidate已VERIFIED不重做，Q06未完/Q07未迁移/总目标ACTIVE。无运行job/build/agent/Play，复用Editor62860；禁computer-use/非战斗/Scene/资源修改。

> 当前Q06 STATE1218-CONTACT-NATIVE-FRAME-BINDING-001 IN_PROGRESS。源480 build/double-run一致SHAd898b84d…ffc6c1，独立初态/即时228481检查PASS；原模型implicit999失败8处已保留纠正。正式330默认contact目标hard2512/soft1853全部显式，19461 ITR无非0pickedact/pickingact，仅静态域不证明动态身份切换；不称已确认玩家可见Bug。新Unity单fixture pending7字段已恢复，CS0，job87f3c88664724d3b80bf2501db16de3b运行四直接矩阵，必须读终态；生产尚未修改，airborne另属后继。源与fixture准确Record已建。前ordinary/candidate两个包VERIFIED不重做；Q06未完/Q07未迁移/总目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> NTSD28-Q06-STATE1218-CONTACT-NATIVE-FRAME-BINDING-001 IN_PROGRESS / SOURCE_WITNESS_FIRST；只声明新CPP，生产未改。airborne独立后继，保护已闭物理行为。

> 普通落地NATIVE-FRAME-BINDING及CANDIDATE-COLLISION-REFERENCE-RESET均已限定VERIFIED。source186四矩阵0差异、联合15+旧6、sameWorld744场景1488tick、完整SelfCheck、真实Play1488、退出重进Q05关闭00:17:02Z/00:18:05Z全部PASS；Scene checksum/borrowers2→2、restore4→4/worldslots两pool0/两帧Stopped，dirtyfalse/root14/hashBCD1047B…0E9FB6保持。下一回NTSD28-Q06-NATIVE-FRAME-REMAINING-LIVE-READERS-AUDIT-001：优先只读确认state12/18 contact/airborne剩余三个raw caller及完整authority事务，必要时新建源见证Task，禁止重做已闭物理规则或批量改getter。platform op30/previousXYZ/raw3明确保留，identity/impact/lateEffects/display/post-display继续待。Q06未完/Q07未迁移/总目标ACTIVE；无运行job/build/agent/Play，Editor62860可复用；禁computer-use/非战斗/Scene/资源修改。

> 最新验收：candidate reference reset与普通落地native binder联合15/15PASS，source186×四组before/即时/following均0；旧B4普通落地已以正确Editor namespace补跑6/6PASS（job4191c162de2543eb83166574b9983c84）。完整SelfCheck 2026-09-21T00:11:47.663892Z PASS文件与时间证据已存candidate包。CS0、Ledger597/11PASS、diffcheck无错误，Scene hashBCD1047B…0E9FB6保持。下一唯一工作：同一普通落地186夹具补同World回放，再真实Play两profile/两路径/两factory及Q05关闭重入；脚本扩展前登记Record准确符号。两个包仍IN_PROGRESS（尚未完成replay/Play），不是平台op30域对齐。当前无运行test/SelfCheck/build/agent/Play；复用Editor62860，禁computer-use/非战斗/Scene/资源修改，Q06未完/Q07未迁移/总目标ACTIVE。

> NTSD28-Q06-CANDIDATE-COLLISION-REFERENCE-RESET-001 IN_PROGRESS：Q06候选高度参考reset独立包已生产写入；联合job0bbead38ad244ed1adc8ac109ebdabd3 SUCCEEDED15/15（新5/载体6/源矩阵4），普通落地186×两profile×两路径before/即时/following全部0。实际未运行旧B4 fixture（初始filter漏Editor namespace），后继须NTSD.Test.Editor.NTSD28B4Type0OrdinaryLandingEditorTests补跑。完整SelfCheck请求已消费，现有Editor PID62860执行中，实际日志Logs/kind8-real-play-editor.log已有RunAllChecksStatic/CheckActivatedRuntimeProfileContracts调用；结果待新Temp/NTSD_BattleRuntimeSelfCheck.result，禁止重复启动或测试并行。之后同World回放/真实Play及关闭仍待。平台op30/previousXYZ/其他两个reference字段仍未闭合，不称整域完成。Q06 active/Q07未迁移/禁computer-use/非战斗/Scene/资源修改。

> NTSD28-Q06-CANDIDATE-COLLISION-REFERENCE-RESET-001 IN_PROGRESS / TEST_FIRST；准确query入口及新focused测试预声明，生产未改。父落地完整tick依赖本项，Q06仍active。

> 当前BATCH-03/Q06：普通落地单caller native绑定已写。job01658e8a1a8241caab89c3c3fb25234e终态FAILED4/4，四组各186 before0/immediate0/following78，仅combat.collisionYReference=-10 expected0；证据after-binding-fix。源battle_world.cpp4026在pair pass前清零collision_y_reference/platform_source_slot_f4/render_shadow_offset_10c；Unity仅carrier reset已有，需审完整生产pass及平台依赖，不能在fixture清零掩盖。下一先只读确认并独立Task/Change声明必要生产修复，再完整回归/回放/SelfCheck/Play。CS查询0、Ledger596/9PASS、diffcheck无错误、Scene SHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6保持。落地Task仍IN_PROGRESS，Q06未完/Q07未迁移/总目标ACTIVE；禁computer-use与非战斗/Scene/资源修改。

> NTSD28-Q06-ORDINARY-LANDING-NATIVE-FRAME-BINDING-001 IN_PROGRESS / SOURCE_WITNESS_FIRST；源单CPP写域，Unity生产未改。

> 当前BATCH-03/Q06：KIND8-NATIVE-RAW-BINDING-001已限定VERIFIED。source134/62045、两profile0差异、53回归、268场景536replay、SelfCheck成功日志、真实Play536与关闭23:33:57Z全PASS；Scene dirtyfalse/root14/hashBCD1047B…0E9FB6保持。下一唯一Task NTSD28-Q06-ORDINARY-LANDING-NATIVE-FRAME-BINDING-001 PLANNED / SOURCE_WITNESS_FIRST，只有新CPP预声明，尚未写脚本。ordinary landing规则及canonical tail已闭不重做，只核对raw descriptor binding；formal330静态7隐式hit_g不是动态可达证明。目标Editor62860当前idle/notPlaying，MCP6403/status-b1b02287可复用，禁止第二个同项目实例。无运行test/build/agent。CPoint370保留source-only、selectors正式全零排后，identity/impact/lateEffects/display/post-display仍待；Q06未完/Q07未迁移/总目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。

> 当前Q06 KIND8-NATIVE-RAW-BINDING-001 IN_PROGRESS / FOCUSED_REPLAY_SELF_CHECK_PASS_PLAY_PENDING。kind8单caller原生binder修复后两profile134 before/立即/following0；联合53/53PASS含268场景536重放tick。旧projection反射2FAIL已独立KIND8-PROJECTION-TEST-ENTRY-001限定VERIFIED，原失败保留。完整SelfCheck本轮batch PID122096已终止，日志成功marker与SHA已存full-selfcheck-completion-proof.json；退出后Temp结果缺失，不声称文件归档。下一在同新Editor fixture添加Play536入口，真实场景两profile两factory保护及Q05关闭；目前入口未写，勿重复旧source/回归。场景hashBCD1047B…0E9FB6保持，无目标Unity/job/build运行。CPoint370仅source capture/局部oracle且正式selectors全零，排后未取消目标；Q06未完/Q07未迁移/总目标ACTIVE，禁computer-use/非战斗/资源/Scene改动。

> NTSD28-Q06-KIND8-PROJECTION-TEST-ENTRY-001 IN_PROGRESS / TEST_ONLY，修两旧projection用例反射缺两个可选参数；source134两profile已0差异，父验收未完成。

> 2026-09-21当前Q06：KIND8-NATIVE-RAW-BINDING-001 IN_PROGRESS。源134双跑一致SHAc0543119…b26009，62045独立即时transition检查0失败，132applied/2拒绝；新单Editor fixture已写、生产未改。目标项目原未打开，root启动Unity2022.3.62f3独立EditMode PID51800（Temp/NTSD28_Kind8Batch.pid），测试filter NTSD.Test.NTSD28Q06Kind8NativeRawBindingEditorTests，结果Logs/kind8-134-editmode.xml/日志kind8-134-editor.log；当前等待该进程终态，不重复启动同项目实例。CPoint选择370源双跑/2590部分oracle已存，formal330八selector均0，因此source-only保留，排在kind8正式内容30隐式目标之后，非已完成。held已验不重做；Q06未完/Q07未迁移/总目标ACTIVE，禁止computer-use与非战斗/Scene/资源修改。

> NTSD28-Q06-KIND8-NATIVE-RAW-BINDING-001 IN_PROGRESS / SOURCE_WITNESS_FIRST；有正式DAT隐式目标正例，生产优先于CPoint全零selector。后者只源诊断构建中session28190，不改Unity；准确范围见各Record。

> NTSD28-Q06-CPOINT-INPUT-ACTION-SELECTION-001 IN_PROGRESS / SOURCE_WITNESS_FIRST，只有新CPP写域，Unity生产未改。

> 当前BATCH-03/Q06：HELD-NATIVE-FRAME-BINDING-001已VERIFIED（初次140/释放1150/补给242及formal330静态内容声明范围），最终SelfCheck/Play968/关闭21:09:16Z证据保持。SyncHeldPose仅定义；同2033边selectedchild10/12/18均0，动态identity保留关系必须后继回访。当前source无child12/18unsupported，旧游标表述已纠正。下一唯一执行Task NTSD28-Q06-CPOINT-INPUT-ACTION-SELECTION-001 READY_SOURCE_WITNESS，Record PLANNED只声明新CPP，尚未改脚本：完整8路输入选择/最终0取消/原生selected frame与vaction，保护既有throw和kind2。raw-binding矩阵已交付，canonical kind8 dvx999哨兵等排后；identity/rawsetter/lateEffects/display/post-display继续保留。Q06未完/Q07未迁移/总目标ACTIVE；无运行job/build/agent/Play，禁computer-use/非战斗/Scene/资源修改。

> 当前BATCH-03/Q06：HELD-NATIVE-FRAME-BINDING-001 IN_PROGRESS / INITIAL_RELEASE_REFILL_SCOPES_VERIFIED_REMAINING_CALLERS_PENDING。refill242两profile正常0差异、generic202两profile0、484场景968重放tick、旧100回归与联合11/11、完整SelfCheck21:07:43Z、真实Play968全部PASS；关闭21:09:16Z restore4→4/worldslots两pool0/两帧Stopped。Scene dirtyfalse/root14/hashBCD1047B…0E9FB6保持。两个refill oracle已限定VERIFIED，旧失败保留。下一回remaining-live-reader审计：SyncHeldPose全仓仅定义无caller，不修改；held damaged12/10与源unsupported12/18及formal330域需最小只读核对后决定是否本Task仍有真实残余。不要重做已验140/1150/242，不批量改共享getter。identity/rawsetter/lateEffects/display/post-display后继保持；Q06未完/Q07未迁移/总目标ACTIVE。无运行job/build/agent/Play；禁computer-use、非战斗/Scene/资源修改。

> refill两独立oracle已VERIFIED / TEST_ONLY：REFILL-NATIVE-RNG-EDITOR-ORACLE-001与REFILL-REAL-OID-SELF-CHECK-ORACLE-001；100/100及新完整SelfCheck21:07:43Z PASS。父refill Play run-refill968正在执行，尚待结果及关闭。

> Q06 refill生产修复已两profile242 before/立即/following全0；旧100回归经独立RNG oracle修订后100PASS。SelfCheck原21:00:51Z伪OID992失败保留，独立REFILL-REAL-OID-SELF-CHECK-ORACLE-001只改该const为122，尚待新fullSelfCheck。当前job a2a98665947e4e2babea21e39e0aa359：generic202两profile、refill242 replay两profile、补给Editor最终标记断言回归，必须查询终态。refill Play run-refill968入口已写未运行；原release/initial已验不重做。父IN_PROGRESS，两个oracle未关闭，Q06未完/Q07未迁移/总目标ACTIVE；禁止computer-use、非战斗/Scene/资源修改。

> NTSD28-Q06-REFILL-REAL-OID-SELF-CHECK-ORACLE-001 IN_PROGRESS / TEST_ONLY，准确测试范围见Record；不改production。

> NTSD28-Q06-REFILL-NATIVE-RNG-EDITOR-ORACLE-001 IN_PROGRESS / TEST_ONLY，准确测试范围见Record；不改production。

> 当前Q06 refill已获得确定RED：source242双跑一致SHA83af13d…42d5dc0、229293独立检查PASS，原140/1150字节保持；Unity job fd1f7330323c4a5e9e1f3893c13c372d两profile242均before0/immediate660/following992，终态FAILED，证据refill-production-red。差异含耗尽旧legacy RNG、隐式0 wait、type0通用实体未补给；fixture已模拟正式type_sub默认，初态完全一致。下一先冻结最小共享补给事务生产符号/Record，再修已测分支，不动框架/Scene/资源。cpoint_acceptance只读审最小改动设计；无Unity job/build/Play运行。父HELD-NATIVE-FRAME-BINDING IN_PROGRESS，初次140及release1150限定验收保持不重做；Q06未完/Q07未迁移/总目标ACTIVE，禁computer-use。

> 当前BATCH-03/Q06：HELD-NATIVE-FRAME-BINDING-001 IN_PROGRESS / INITIAL_AND_RELEASE_SCOPES_VERIFIED_REFILL_PENDING。release源1150/独立296552 PASS，正常及generic664两profile零差异；旧回归91/query5、同World replay2300场景4600tick、完整SelfCheck20:39:17Z、真实Play4600全部PASS；关闭20:45:41Z restore4→4、world/slots/logic/render0、两帧Stopped，Scene dirtyfalse/root14/hashBCD1047B…0E9FB6保持。独立HELD-RELEASE-FIELD-SELF-CHECK-ORACLE-001已VERIFIED（测试字段范围）。下一同held Task扩refill源向量，精确核对耗尽action0/nativeRNG/Zz及源system_rules对象id门；不要重做初次140或release1150。remaining reader/identity/rawsetter/lateEffects/display/post-display仍待。Q06未完/Q07未迁移/总目标ACTIVE，无运行job/build/agent/Play；禁止computer-use、非战斗/Scene/资源修改。

> 当前Q06：HELD-RELEASE-FIELD-SELF-CHECK-ORACLE-001 VERIFIED / TEST_ONLY；独立字段断言已修，完整SelfCheck2026-09-14T20:39:17Z PASS，原失败保留。父HELD-NATIVE-FRAME-BINDING仍IN_PROGRESS；已扩单Editor fixture的release1150两profile同World回放及run-release Play4600入口，等待编译和实际运行。普通1150/generic664零差异保持，下一先release replay再Play/关闭。Q06未完/Q07未迁移/总目标ACTIVE；无agent运行，禁止computer-use及非战斗/Scene/资源修改。

> NTSD28-Q06-HELD-RELEASE-FIELD-SELF-CHECK-ORACLE-001 IN_PROGRESS / TEST_ONLY：准确限定两个SelfCheck方法，修订2F8与Spawner独立字段断言；生产不改，验收待新完整SelfCheck。

> Q06最新验证：release1150正常路径及generic武器DAT664子集两profile均0差异，query5/5与旧回归91/91 PASS；但新完整SelfCheck 2026-09-14 20:35:34 UTC FAIL R5-HOLD-002（旧断言要求释放写Spawner，源写独立+2F8）。失败已归档，下一先独立审计该oracle并建立准确Task/Change后修改测试，再SelfCheck及release replay/Play。父HELD-NATIVE-FRAME-BINDING仍IN_PROGRESS，Q07未迁移，总目标ACTIVE。无运行Unity test/build/Play；只读cpoint_acceptance正在审计oracle。禁止computer-use、Scene/资源/非战斗修改。

> 当前 BATCH-03/Q06：HELD-NATIVE-FRAME-BINDING-001 IN_PROGRESS。初次绑定140限定验收保持；release源1150双跑及296552独立检查PASS，正常factory两profile1150 before/立即/following均0差异（job b573b82a834a415c9925f12441a11b0f，release-after-fix）。通用LF2OtherObject承载type1/2/4/6武器DAT的664行两profile补验已SUCCEEDED 2/2、before/立即/following全0（job f60db972d4724aacb78ecd0a5c51aa1d，release-generic-pass）；限定受控CLR适配子集。释放阶段最新生产修复后的replay/SelfCheck/Play仍待，不能复用初次140旧验收代替。refill及remaining live readers、display/post-display继续待处理。Q06未关闭，Q07正式资源未迁移，总目标ACTIVE；保留用户HUDBg30，禁止computer-use及非战斗/Scene/资源修改。以下旧检查点保留历史，以本条覆盖。

> 当前BATCH-03/Q06：HELD-NATIVE-FRAME-BINDING-001 IN_PROGRESS / INITIAL_BINDING_VERIFIED_RELEASE_READERS_PENDING。源140两profile立即/fulltick0、query5/5、旧回归91/91、280replay/560tick、SelfCheck20:01:02Z、真实Play560及关闭20:02:44Z全PASS；Scene checksum/borrowers2→2、restore4→4/worldslots两pool0/两帧Stopped，hashBCD1047B…0E9FB6保持。非holder-query、旧fixture、slotreuse-oracle三个独立子包已VERIFIED声明范围。下一同held Task先保留source140，再扩kind1/3+DVX等后继原生源见证；type2非kind3-DVX旧legacy RNG与后续frame40/随机/补给0旧setter待对照，不重做初次绑定或批量改共享接口。formal330 cover2/state12/18静态域仍0，限定dormant报告已存。remaining reader总审计/identity/rawsetter/lateEffects等继续保留；Q06未完/Q07未迁移/总目标ACTIVE。无运行job/build/agent/Play；禁止computer-use/非战斗/Scene/资源修改。

> NTSD28-Q06-HELD-SLOT-REUSE-SELF-CHECK-ORACLE-001 IN_PROGRESS / TEST_ONLY：SelfCheck same-slot关系清理期望-1纠正为source/既有writer0，缓存及反向关系断言保持。

> NTSD28-Q06-HELD-LEGACY-FIXTURE-NATIVE-CONTRACT-001 IN_PROGRESS / TEST_ONLY，保留新140全部零差异；两旧fixture初始化和失效oracle独立修订，未改productionbinder。

> 新独立 NTSD28-Q06-HELD-QUERY-NONHOLDER-RELATION-PRESERVATION-001 IN_PROGRESS / TEST_FIRST。持有140已before0/立即0，fulltick90差异定位CPoint同步后角色GetHeldEntity把负/零关系清为0/-1；只拆非持有者cache查询分支，正关系失效处理保持，先精确测试。父HELD-NATIVE-FRAME-BINDING仍IN_PROGRESS，Q06未完。

> 当前Q06 NTSD28-Q06-HELD-NATIVE-FRAME-BINDING-001 IN_PROGRESS / SOURCE140_PASS_UNITY_PENDING。源build/double-run140行1467643bytes SHA8ddb447…5b9610，105valid/21unsupported/14terminal，1344+420独立检查PASS；following126alive/全部lifecycleSuccess，完整following尚未Unity对照。初版reciprocal建立过早导致零分支，失败归档后已修。下一在同Record先新增准确单Editor fixture路径，以两profile/两factory复建完整before并比立即与following，再根据RED限定held两caller native读帧；不要直接改共享setter(type3命中也调用)。remaining reader审计187occurrences/初步matrix仍IN_PROGRESS，identity/rawsetter/lateEffects authority等后继保留，前已验三包不重做。无运行build/test/agent/Play；Q06未完/Q07未部署/总目标ACTIVE，禁止computer-use/Scene/资源/非战斗修改。

> 当前Q06剩余reader审计IN_PROGRESS：187词法occurrences已存inventory，初步live/compat/unknown矩阵已写。下一独立 NTSD28-Q06-HELD-NATIVE-FRAME-BINDING-001 IN_PROGRESS / SOURCE_WITNESS_FIRST；仅新诊断CPP预声明，正式held两条分支旧HasFrame857/descriptor阻断隐式与高帧后续定位，先source完整状态，不批量替换getter。identity/fusion、通用rawsetter、lateEffects authority及其它hit/AI/spawn caller仍待审计；旧throw/shared native-input绕过路径保留。前cost/physics/oracle三包已限定VERIFIED不重做。Q06整体未完成、Q07未部署、目标ACTIVE。

> 当前BATCH-03/Q06：NATIVE-INPUT-ACTION-COST-FRAME-READERS-001、CANONICAL-CHARACTER-PHYSICS-TAIL-001及NATIVE-DJA-UNAVAILABLE-SELF-CHECK-ORACLE-001均VERIFIED（各自声明范围）。源151四完整tick0差异、302replay/604tick、56实际physics、SelfCheck19:09:57Z、真实Play1208+56、最终关闭19:11:02Z全部PASS；Scene checksum/borrowers2→2、restore4→4、world/slots/pools0、两帧Stopped，dirtyfalse/root14/HUDBg30/hashBCD1047B…0E9FB6保持。下一唯一Task NTSD28-Q06-NATIVE-FRAME-REMAINING-LIVE-READERS-AUDIT-001 READY_READ_ONLY：按实际caller分类余下legacy getter/raw setter，禁止批量替换/重做已关闭包；之后回DISPLAY-PROGRESSION/POST-DISPLAY。Q06整体未完成、Q07正式内容未迁移、raw3缺口/跨Worldepoch/stage USER_HOLD/例外保持；总目标ACTIVE。无运行job/build/Play，禁止computer-use/非战斗/Scene/资源修改。

> NTSD28-Q06-NATIVE-DJA-UNAVAILABLE-SELF-CHECK-ORACLE-001 IN_PROGRESS / TEST_ONLY：只修CheckComboLocalShadowCommitContracts失败夹具399→1000。

> 当前新增独立 NTSD28-Q06-CANONICAL-CHARACTER-PHYSICS-TAIL-001 IN_PROGRESS / RED_CONFIRMED。源边界统一后151×两profile Legacy0差异、DataOriented326差异；准确Task/Change已建，只修快速物理尾部接线并新增定向测试。父输入费用Task等待该出口；Q06/总目标仍未完成。

> 最新Q06费用fulltick隔离：job53acd6f9d30f483daebeda0f0cdc12c2四组151 before0，Legacy各44差异(仅负X钳制)，DataOriented各370(还含state12落地动作/counter/Vy)。CS0，失败归档following-path-isolation-red。下一统一横向世界边界后再验证physics差异并建立独立准确Task/Change；勿扩大BCAW费用生产范围或用Legacy代替快路径验收。当前测试已终止、无运行job，Q06未关闭/Q07未迁移/总目标ACTIVE。

> 当前 BATCH-03/Q06：NATIVE-INPUT-ACTION-COST-FRAME-READERS-001 IN_PROGRESS。151输入费用两profile端点0差异、Combo18/18通过；新增完整相邻tick两profile失败并归档following-initial-red。下一先统一world stage边界(source Z0/Unity默认ZMin180)，再对照Legacy/DataOriented physics：快速路径缺少普通路径已有state12/18 contact调用，尚未运行分离验证，不判定完成。Q07未迁移，总目标ACTIVE；禁止computer-use/Scene/资源/非战斗修改。此前顶部READY/生产未改描述已过时，仅保留历史。

> 当前BATCH-03/Q06：CPOINT-THROW-NATIVE-RAW-BINDING-001与NATIVE-INPUT-MISSING-STATE-ROUTING-001均已VERIFIED（各自声明范围）。source392/432、两profile输入及投掷立即/fulltick、112场景224重放tick、SelfCheck18:13:58Z、真实Play18:15:16Z1568+864及关闭18:16:07Z全部PASS；Scene checksum保持/Renderer2→2、恢复4→4、World/slots/两pool0、两帧Stopped，Editor idle/notPlaying、dirtyfalse/root14、HUDBg30/hash BCD1047B…0E9FB6保持。下一唯一Task NTSD28-Q06-NATIVE-INPUT-ACTION-COST-FRAME-READERS-001 READY_LIVE_SOURCE_MAPPING：BCAW ApplyNativeInputActionCore、rowing与builtin cost仍旧Has/Get；先追真实source apply_action/资源重定向与fallback完整事务并建准确Record，未改该项脚本。不要重做已关闭Cpoint/source/replay/Play。Q06/full alignment仍未完成，Q07正式资源未迁移，raw3缺口/跨Worldepoch/stage USER_HOLD/例外保持，禁止computer-use/非战斗/Scene/资源修改，总目标ACTIVE，无运行job/build/agent/Play。

> 当前BATCH-03/Q06：qualification真实driver补验8例16:54:03Z PASS、关闭16:55:03Z PASS；qualification/collision-frame/prelude-feedback三父包已按声明范围VERIFIED，旧失败不再当前阻塞。下一唯一Task NTSD28-Q06-CPOINT-THROW-NATIVE-RAW-BINDING-001 READY_SOURCE_WITNESS_AND_EXACT_RECORD（已创建精确Task，尚未改脚本）：生产ApplyThrow旧next预取+两个Cpoint raw setter旧857门/descriptor，会让原生高帧action与snapshot分离；先当前source见证，保护RunKind2Validation212，禁止全局getter替换。987/reduced/gain/各端点及kind2/kind3/C25已验不重做。BDEFEND等其它父字段记录需按各自出口回链，不据三包VERIFIED自动标全Q06。现无build/test/agent/Play运行；Q07资源迁移未启动，原raw3缺口/跨Worldepoch/stage USER_HOLD/用户例外保持，禁止computer-use/非战斗/Scene/资源修改，总目标ACTIVE。

> 当前阶段BATCH-03/Q06。VERIFIED / DECLARED_NONCHARACTER_REDUCED_AND_FALLBACK_SCOPE。987四组before0/diff0，资源联合19/19、type3联合27/27、完整SelfCheck16:36:40Z PASS；本批local replay54场景/108重放tick（job7fa21a8a1a9546ef96d214ed84048cdb 2/2 PASS）；真实Play16:45:17Z 3948/3948 PASS，两factory/direct-Shadow/Scene checksum保持/Renderer2→2；关闭16:45:31Z PASS，restore4→4、World/slots/两pool0、两帧Stopped。独立最终review四生产路径未发现确定新错误。当前四个qualification完整driver回访job65d082698b534717b51999e3be656a5b 4/4 PASS；原失败保留。不据此关闭全部Q06、跨World恢复或正式内容/视听；资源0/1hop/gain边界及两级resolver focused为实际范围，未穷尽所有owner组合。 源子项同样限定VERIFIED。下一最小任务：qualification父出口审查/其明确目标Play证据（若缺则只补四个完整driver在真实Play的场景保护验证），随后收口qualification/collision-frame依赖并返回NATIVE-FRAME-RUNTIME-READER-MIGRATION剩余live reader调用图。不要重做原3264端点、source构建、spark/feedback/weapon/type5/reduced；旧父文档失败检查点已追加纠正。当前无build/test/agent/Play运行，Editor已退Play；Q07正式DAT/图片仍未部署，raw3缺口/epoch/stage USER_HOLD及用户例外保持，总目标ACTIVE。禁止computer-use/非战斗/Scene/资源修改，未提交推送。

> 当前阶段仍BATCH-03/Q06。NONCHARACTER-REDUCED-HIT-TRANSACTION-001 IN_PROGRESS / ALL987_AND_SELF_CHECK_PASS。source owner987双跑SHA42367528…44bc9/545439PASS；Unity先before0 direct140/Shadow284，确认旧helper读Runtime.MP错误，root新gain块改Runtime.PP/MPMax，旧MP/helper保持；HitPlan独立resource-owner tuple(PP/消费/类型/上限/localflag/冻结slot+handle)及最多2级独立resolver已写，gain非0观察限制已移除。job6c1197e1854b4f2c8d420f73393a5d3a 19/19 PASS含987四组0差异/owner解析/旧纯函数/scope。后C30 SelfCheck暴露type3 target hit声，parent生产和标准/D1预测修正，音频oracle重新开/关留痕；job a568ca84a4a948e5942c57e0e2244a85 27/27 PASS含987及type3专项，完整SelfCheck16:36:40Z PASS。XML/失败/最新PASS全部parent artifact。下一直接本批真实Play987×两factory×directShadow=3948、关闭及本批local replay；尚未新增对应probe，不要重跑已过矩阵或重做source。若需两级resource-owner实际Shadow/非零localMode边缘可补必要focused，当前987只有0/1hop/invalid、旧resolver两级7项已过，不声称整个owner所有输入穷尽。当前无job/build/agent运行，工作树HEAD观察为f9f7b133（本任务未提交），既有外部提交不回退。Q07资源未部署、schema/raw缺口/跨World epoch/例外保持，总目标ACTIVE，禁止computer-use/非战斗/Scene/资源修改。

> 2026-09-15 当前阶段：BATCH-03 / Q06，IN_PROGRESS。Q01～Q05已达到各自限定出口；Q07正式DAT/角色图片迁移尚未启动，Q08～Q12等待前置。当前Q06子项为NONCHARACTER-REDUCED-HIT-TRANSACTION：843四组0差异及原984回归已通过；正在补非零gain/resource-owner独立观察。新增source owner987（含144 owner/gain边界）已双跑一致SHA42367528b7429d0dfb525a5c3208151f1bdd0937db7657b7b4a0b40066044bc9、545439独立检查PASS；Unity fixture已准备切owner987并恢复OwnerSlotIndex，但尚未刷新/跑987 RED，Shadow仍gain!=0不覆盖。下一步明确：刷新→987 before0及覆盖RED→扩独立resource-owner tuple→复测，再本批SelfCheck/Play/关闭/replay。无运行build/test/agent；不得将source987通过当Unity987通过。以下旧检查点仅保留历史，当前入口以此为准。

> 当前NTSD28-Q06-NONCHARACTER-REDUCED-HIT-TRANSACTION-001 IN_PROGRESS / ALL843_FOCUSED_PASS。root已新增LF2Entity.NativeHitCandidateScope可空原Route/嵌套恢复，runner先Resolve再Begin；DamageWriter weapon/type5/type3水平后按明确BrokenFallback消费、bypass -1保持、type3 generic音效去除；HitPlan已同步原route与独立fallback预测。job46a5d9c9928c4c13b69d75976a1feb87 5/5 PASS：843四组before0/diff0+scope嵌套异常恢复。原回归jobf25fb55daf6941ca8c49a4cbe64d36b1 18项17PASS/1旧type3音频断言FAIL，原984/684early/Bdefend/prelude replay全PASS；独立oracle NTSD28-Q06-TYPE3-NATIVE-AUDIO-ORACLE-001 VERIFIED，单断言修正后job4b866e2569774961ad93f303a054d398 4/4 PASS。所有XML同parent artifact保留。下一必须项非零gain/resource-owner独立捕获/比较与源/Unity向量，现CanProjectNativeNoncharacterReduced明确gain非0拒绝，不能当完成；还需新selfcheck、Play两factory/关闭及本批local replay和必要type3专项音频回归。当前无job/build/agent运行；本批四生产文件+单新fixture及单oracle，未改Scene/资源/非战斗。Scene SHA BCD1047B…0E9FB6保持。总目标ACTIVE/Q07未部署，禁止computer-use。

> 新子项 NTSD28-Q06-TYPE3-NATIVE-AUDIO-ORACLE-001 PLANNED，等待当前回归终态后修正旧音频断言；parent仍IN_PROGRESS。

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

> 活跃 `NTSD28-Q06-TYPE5-MATCHED-PAIR-EARLY-001` IN_PROGRESS / TEST_FIRST_ONLY；root单Editor测试读取source DAT/before，源worker仍在验证，未改生产。

> 当前执行 `NTSD28-Q06-TYPE5-MATCHED-PAIR-SOURCE-WITNESS-001` IN_PROGRESS / SOURCE_ONLY；两个Tools文件由worker独占，根代理检查Unity映射。普通type5/weapon已验收不重做，尚未改本批Unity生产。

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

> 活跃依赖 `NTSD28-Q06-WEAPON-REACTION-SELF-CHECK-ORACLE-001` IN_PROGRESS：完整SelfCheck10:16:55Z旧HitConfirm2断言FAIL；定向收集后按正式源纠正，禁止回退已过8400生产。

> 当前武器批次：`NTSD28-Q06-UNARMORED-WEAPON-REACTION-001` IN_PROGRESS / DIRECT_2100_PASS_SHADOW_PENDING。三生产路径已改、两profile各2100直接对照0差异；当前正编译并补Shadow独立预测。源见证`NTSD28-Q06-WEAPON-REACTION-SOURCE-WITNESS-001` VERIFIED / SOURCE_MODEL_ONLY（2100/43202）。非战斗/HUDBg30保留，Q07未部署、完整对齐尚未完成。旧下文TEST_FIRST_ONLY/生产未改描述仅为历史检查点。

> 活跃 `NTSD28-Q06-UNARMORED-WEAPON-REACTION-001` IN_PROGRESS / TEST_FIRST_ONLY，source2100与单Editor测试，生产尚未改。

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

> 活跃测试子项 `NTSD28-Q06-BDEFEND-FORMAL-CANDIDATE-ENTRY-ORACLE-001` IN_PROGRESS，保持原断言，只纠正direct入口边界。

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

> `NTSD28-Q06-NONCHARACTER-ARMOR-FEEDBACK-001` IN_PROGRESS / TEST_FIRST_ONLY，当前仅准确单测试脚本；生产未改，源码证据与RED并行准备。

> 当前执行 `NTSD28-Q06-PREARMOR-FEEDBACK-SOURCE-WITNESS-001` IN_PROGRESS / SOURCE_ONLY。反馈前的heavy release旧RNG/字段写入存在差异，先原完整前置取证，不直接早返；Unity生产本轮未改。

当前唯一恢复游标（2026-09-14 08:14Z）：

- `NTSD28-Q06-HIT-SPARK-UNITY-001` VERIFIED / DECLARED_HIT_SPARK_TRANSACTION_SCOPE。公共Append源guard/owner/capacity/编码/snapshot/cover/整数平均/target Z/Y-X CRT，candidate瞬时scope，角色route owner唯一emission、两个重复外层移除，Shadow独立CRT capture/compare已闭合。
- 最终34/34 PASS：source438×2端点876、角色actual282×2=564（复制itr/原index2）、异常嵌套/非0 guard、原完整driver四组8向量及Shadow0差异、local replay8case/16replayed ticks；完整SelfCheck08:06:43Z PASS。真实Play两factory端点+actual共1440于08:11:26Z PASS，Scene checksum不变/Renderer2→2；08:11:53Z关闭PASS，原位恢复4→4、World/slots/两pool全0/连续两帧Stopped。
- 旧C01 hook与SelfCheck观测分别由`NTSD28-Q06-SPARK-C01-TEST-HOOK-001`、`NTSD28-Q06-HIT-SPARK-SELF-CHECK-ORACLE-001` VERIFIED纠正；全部旧FAIL保留。唯一详细证据：`artifacts/diagnostics/NTSD28-Q06-HIT-SPARK-UNITY-001/REPORT.md`。主包8脚本+两个单测试子项，共10脚本；HEAD61b3b6cf，未提交。
- **下一唯一Task：`NTSD28-Q06-NONCHARACTER-ARMOR-FEEDBACK-001 / READY_SOURCE_ORDER_AND_EXACT_RECORD`。** 公共spark已就绪，先闭合原selected armor/special-link-rest前置及各Unity活入口，准确新Record后再接反馈Append(...,armor,false,false)，不能无条件早返或只保留Bdefend。非角色首type0原96例曾误伤害；type1不能从type0推断。之后UNARMORED-WEAPON-REACTION和TYPE5-HIT-PLAN-COVERAGE，再回BDEFEND256/父collision/qualification。96/64/32是之前生产的测量，未按本轮重测，不当新鲜结果。
- 父BDEFEND/collision/qualification、reader umbrella与总目标仍IN_PROGRESS / ACTIVE，FULL_ALIGNMENT_INCOMPLETE；旧四组完整driver的Spark RNG首差已经清除，不再当当前阻塞。reader/display其它职责后才Q07，正式DAT/角色图片**尚未部署**。Scene仍旧内容/合成fixture，未做物理按键或Logan图片一致性验收。
- 正式EXE SHA B1E13AE1…D2819033、源438 trace b5df6113…ce6f8复核保持；Scene dirtyfalse/root14/SHA bcd1047b…保持，用户HUDBg x30确认归其或其他任务并保留。schema15/23/26/2/2、raw47/3、跨World allocation epoch恢复缺口、stage.dat USER_HOLD及既有例外保持。禁止computer-use、非战斗/Unity-GAS框架/Scene/资源/Server改动。

以下历史检查点由以上当前游标优先：

> `NTSD28-Q06-HIT-SPARK-SELF-CHECK-ORACLE-001` 已VERIFIED；完整SelfCheck08:06:43Z PASS，以下活跃旧行由本状态替代。

> `NTSD28-Q06-SPARK-C01-TEST-HOOK-001` 已VERIFIED，旧回调fixture修正后22/22；以下活跃旧行由本状态替代。

> 活跃测试子项 `NTSD28-Q06-SPARK-C01-TEST-HOOK-001` IN_PROGRESS，见同ID Record。

> 活跃测试子项 `NTSD28-Q06-HIT-SPARK-SELF-CHECK-ORACLE-001` IN_PROGRESS，见同ID Record。

> 当前执行 `NTSD28-Q06-HIT-SPARK-UNITY-001` IN_PROGRESS / TEST_FIRST，准确七脚本；公共writer与candidate瞬时scope、route owner一起闭合，禁止computer-use。

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

> NTSD28-Q06-BDEFEND-TEST-ORACLE-001 IN_PROGRESS / TEST_ONLY；07:14:34Z SelfCheck旧HitStateCount观测失败，按原Bdefend字段更新准确fixture并重验。

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

> NTSD28-Q06-COLLISION-ROLE-MATRIX-ORACLE-001 IN_PROGRESS / TEST_ONLY；70回归69PASS/1旧期望FAIL，按原snapshot几何改为1，保留测试断言。

> NTSD28-Q06-COLLISION-CURRENT-SNAPSHOT-QUALIFICATION-001 IN_PROGRESS / TEST_FIRST；准确三脚本，原480非零state/frozen eligibility加完整driver2例，先Unity RED，平台current规则不改。

> 当前执行 `NTSD28-Q06-COLLISION-QUALIFICATION-SOURCE-WITNESS-001` IN_PROGRESS / SOURCE_ONLY；单CPP补pair非零state/原eligibility与完整driver换帧见证，Unity父包仍未关闭。

> **当前唯一恢复游标（2026-09-14 05:51Z）：** COLLISION-FRAME-UNITY-001仍IN_PROGRESS / READER_RUNTIME_PASS_QUALIFICATION_PENDING。三生产reader已改（LF2Entity collision、BruteForce current/Prev2、pair factory current/previous），新336×4 descriptor/raw/catch0，但各24candidate首差（合96）保留。kind2/3旧八组PASS、pair/group/catch64PASS；完整SelfCheck05:48:49Z PASS、真实Play168/Renderer2→2/Scene checksum与有序关闭全0/两帧Stopped通过。独立COLLISION-FIXTURE-DEFINITION-IDENTITY-001两处测试修订VERIFIED，原两SelfCheck FAIL保留。源COLLISION-FRAME-SOURCE-WITNESS-001追加84后336限定VERIFIED、旧252前缀不变。
> **下一唯一Task：`NTSD28-Q06-COLLISION-CURRENT-SNAPSHOT-QUALIFICATION-001 / READY_SOURCE_CONSUMER_AND_QUALIFICATION_MAP`。** 原普通geometry按snapshot itr，platform另按current itr；Unity carrier/current itr/body/null门额外拒绝。先原consumer/pair非零state/当前0语义和正式可达性，准确子Record后整体处理普通/cached/consumer，不豁免24失败。无需重做kind2/3和已验证source336。parent collision未关闭，pair state/filter与actual+Shadow新边界尚欠证据。
> 当前无运行中test/build/exec；Editor idle非Play、CS0、Scene dirtyfalse/root14/SHA bcd1047b…保持。用户HUDBg x30确认归其或其他任务并保留。总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE；raw47/3、schema15/23/26/2/2、epoch恢复缺口、Q07未部署及stage.dat USER_HOLD/用户例外不变。禁止computer-use、非战斗/Unity-GAS/Server改动。

以下为历史恢复检查点，以上当前游标优先：

> NTSD28-Q06-COLLISION-FIXTURE-DEFINITION-IDENTITY-001 IN_PROGRESS / TEST_ONLY；新SelfCheck失败是invalidFirst仍带body定义、只改Frame指针的旧夹具，先用真实空定义修夹具，保留断言。

> NTSD28-Q06-COLLISION-FRAME-SOURCE-WITNESS-001 追加84个current=snapshot原输入，暂IN_PROGRESS；旧252验证不撤销、不覆盖。Unity同一job9efe00d15694402e9a63636d58cb2ea4仍待终态，不因MCP观测timeout重跑。

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

> 当前必要测试子项 `NTSD28-Q06-STATE18-RNG-EXCEPTION-FIXTURE-001` IN_PROGRESS；pending motion三脚本6/6已通过，先独立C17测量后限定完整tick legacy断言。

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

> `NTSD28-Q06-C25L-STATE18-SPAWN-SOURCE-WITNESS-001` IN_PROGRESS补正式999内容向量，原1550证据保持，Unity RED作业继续。

> 当前实施 `NTSD28-Q06-C25L-STATE18-SPAWN-TRANSACTION-001` IN_PROGRESS / TEST_FIRST，准确五脚本，Native RNG/generic birth/即时slot及flush；B8动态delay保持独立，禁止computer-use。

> **当前恢复游标（2026-09-14）：** C25L-STATE18-SPAWN-SOURCE-WITNESS已VERIFIED_SOURCE_MODEL_ONLY，1550行775出生+775完整driver、12759检查，重复字节相同；18条预设pending/无定义动作motion诊断留证，frame/lifecycle错误0。原证mixed OPoint slot50→state18 slots51..57→weapon pieces，源20/70顺序；generic HP/MP500/owner-1/team0/right、精确/整数位置分离；seed2/7/12/17/22/24覆盖持续1粒。**F08 Unity仍旧Match.Rng/普通OPoint出生，尚未修。下一唯一Task `NTSD28-Q06-C25L-STATE18-SPAWN-TRANSACTION-001 / READY_UNITY_RED_AND_EXACT_RECORD`**，保留已正确C25L owner与state13/200，只闭合剩余生成事务；之后回父Frame联合/reader/display-post/Q07。本轮仅一native诊断CPP，未新增Unity/Play结论，前碎片20/12/SelfCheck/173Play成果保持；schema15/23/26/2/2、raw47/3、资源未迁移、用户Scene bcd1047b…保持，禁止computer-use/非战斗/框架改动，总目标ACTIVE。

> 当前执行 `NTSD28-Q06-C25L-STATE18-SPAWN-SOURCE-WITNESS-001` IN_PROGRESS，F08原生成/RNG/身份及C25组合见证；不是重做owner位置，Unity未改，禁止computer-use。

> **当前恢复游标（2026-09-14）：** WEAPON-PIECE-SPAWN-ADMISSION-EDGE及父NATIVE-WEAPON-PIECE-TRANSACTION均已VERIFIED / WEAPON_PIECE_SCOPE_ONLY。三生产+一测试脚本：native OID0专用准入、共用动作预检、Renderer factory在Init前用现有pool API绑定目标World；普通OPoint保持。原1800/Unity5152差异0、20/20及binding后12/12、完整SelfCheck PASS、Play172+Renderer耗尽1、旧Scene两route四例0/5/0/5和4→4、关闭World/slot/两pool全0及两帧Stopped，已退出Play。两个Play失败及修正有留证。**下一唯一Task `NTSD28-Q06-NATIVE-FRAME-TRANSACTION-INTEGRATION-001 / READY_FINAL_C25_CALLER_AND_TRACE_JOIN`**，返回父组合顺序及剩余出口联验，不重做已验2676/core/carrier/death/fragment；其后reader余项/display-post/Q07。15/23/26/2/2、raw47/3、正式资源未迁移，Q06/总目标ACTIVE。用户HUDBg x30/Scene bcd1047b…保留，禁止computer-use/非战斗/框架修改；不是全B1-B12或正式图像完整对齐结论。

> `NTSD28-Q06-WEAPON-PIECE-ADMISSION-SOURCE-WITNESS-001` IN_PROGRESS诊断epoch投影纠正，原出生/slot/RNG事实保持。

> 当前实施 `NTSD28-Q06-WEAPON-PIECE-SPAWN-ADMISSION-EDGE-001` IN_PROGRESS / TEST_FIRST，准确四脚本；OID0专用准入/共同动作校验/失败回收，普通OPoint保持，禁止computer-use。

> **当前恢复游标（2026-09-14）：** `NTSD28-Q06-WEAPON-PIECE-ADMISSION-SOURCE-WITNESS-001` 已 VERIFIED_SOURCE_MODEL_ONLY：900出生+900完整driver/17286断言、重复字节相同、frame/lifecycle/diagnostic错误0、最终构建无warning。原证OID0可生成、999需声明且高slot同tick删除/低slot保留、variant RNG先于缺catalog/无slot；两Unity factory统一oid<=0仍为待修差异。**下一唯一Task `NTSD28-Q06-WEAPON-PIECE-SPAWN-ADMISSION-EDGE-001 / READY_UNITY_RED_AND_EXACT_RECORD`**，源见证不重跑；核对Init/ModuleBind后准确Record、两factory/失败回收/高低slot动态验收，再回父frame/reader/display-post/Q07。前type2 fixture4/2592及完整SelfCheck PASS保持，本轮仅一个诊断CPP，无新Unity Play结论；用户HUDBg x30/Scene bcd1047b…保留，禁止computer-use及非战斗/框架修改。15/23/26/2/2、raw47/3、Q07正式资源未迁移，Q06/总目标ACTIVE。

> 当前执行 `NTSD28-Q06-WEAPON-PIECE-ADMISSION-SOURCE-WITNESS-001` IN_PROGRESS / SOURCE_MODEL_ONLY；原生成准入及高低slot完整driver见证，Unity未改，禁止computer-use。

> **当前恢复游标（2026-09-14）：** TYPE2-LANDING-FACING-SOURCE-WITNESS和FIXTURE均已限定VERIFIED：原648 physics+648 full tick，648对朝向无差异；原报告场景right→left物理flip在C25保留。Unity两外壳×两端点4组/2592比较全PASS，旧heavyBounce自检已按双初始朝向修正，**完整BattleRuntimeSelfCheck新鲜PASS**，生产未修改。前death prelude退休/STATE9998/64联合成果保持，不恢复旧state2000按vx覆盖。**下一唯一Task `NTSD28-Q06-WEAPON-PIECE-SPAWN-ADMISSION-EDGE-001 / READY_SOURCE_WITNESS_AND_CALLER_MAP`**：先原函数证OID0/声明或缺失999/非法初始动作/失败RNG及高低slot完整driver参与，再准确Record成组实现，不能把既有157+3或单个条件放宽当完整fragment。之后回父frame/其余reader/display-post与Q07资源。15/23/26/2/2、raw47/3；资源未迁移，Q06/总目标ACTIVE。用户HUDBg x30/Scene bcd1047b…保持，禁止computer-use、非战斗/Unity-GAS框架改动。证据见type2两个REPORT；完整自检PASS不等于全部对齐。

> NTSD28-Q06-TYPE2-LANDING-FACING-FIXTURE-001 IN_PROGRESS，原1296确认物理flip在完整tick保留；生产正确，修旧fixture并新增对照。

> 当前执行 NTSD28-Q06-TYPE2-LANDING-FACING-SOURCE-WITNESS-001 IN_PROGRESS；原type2物理明确翻面，完整tick保持性待实测，Unity未改。禁止computer-use。

> **当前恢复游标（2026-09-14）：** C25-EXTRA-DEATH-PRELUDE-RETIREMENT已 **VERIFIED / EXTRA_PRELUDE_REMOVAL_ONLY**：准确八脚本退休额外death/bounce/drop及hook，真实hit/physics/WPoint保持；6480原函数区分3240frame端点与3240完整tick，四配置各3240向量及RNG0差异，最终64/64 PASS，独立目标SelfCheck与真实Scene HP0+kind2持有C25保持frame0/Y/Vy0/links1,-1、4→4 checksum及关闭全0/两帧Stopped通过。两个clock/lifecycle fixture Record限定VERIFIED；STATE9998 retirement此前48次上游差异已消除，恢复限定VERIFIED。**完整SelfCheck仍FAIL，下一唯一Task `NTSD28-Q06-TYPE2-LANDING-FACING-AUDIT-001 / READY_SOURCE_PHYSICS_AND_C25_WITNESS`**：type2高速落地实际left与旧state2000强制right期望，先原函数/fixture调用链，不恢复旧行为迁就；随后fragment OID0/999准入及slot完整driver、父frame/其它reader/display-post和Q07资源。15/23/26/2/2、raw47/3，资源未迁移；Q06/总目标ACTIVE。用户HUDBg x30/Scene bcd1047b…保持，禁止computer-use、非战斗及Unity/GAS框架改动；旧死亡前置不可恢复。证据见death-prelude REPORT。

> NTSD28-Q06-LATE-SNAPSHOT-LIFECYCLE-FIXTURE-001 IN_PROGRESS，准确旧夹具范围，保留原FAIL。

> NTSD28-Q06-LATE-NOOP-NATIVE-CLOCK-FIXTURE-001 IN_PROGRESS，准确旧夹具范围，保留原FAIL。

> 当前执行 NTSD28-Q06-C25-EXTRA-DEATH-PRELUDE-RETIREMENT-001 IN_PROGRESS / TEST_FIRST，准确八脚本；6480原函数区分frame端点与完整driver，退休额外C25死亡前置与hook，真实hit/physics/WPoint保持。禁止computer-use/非战斗改动。

> 当前 NTSD28-Q06-DEAD-CHARACTER-FRAME-AND-HELD-SOURCE-WITNESS-001 IN_PROGRESS；先原函数frame/完整driver/held矩阵，尚未修改死亡逻辑。

> **当前恢复游标（2026-09-14）：** GT08 fixture已VERIFIED_TEST_ONLY（17/17及完整SelfCheck越过），STATE9998-SOURCE-DRIVER-WITNESS已VERIFIED_SOURCE_MODEL（224场景672完整tick全存活）。**STATE9998-LEGACY-CLEANUP-RETIREMENT为 COMPILE_PASS / FOCUSED_PARTIAL / SCOPED_PLAY_PASS**：准确五脚本，移除Serial末尾额外9998删除、其余职责/顺序不动；224场景删除差异0，联合28/29、最终6/7，保留type0 HP0共48次动作差异，beforeSerial已186。真实Scene当前descriptor9998经Serial存活/checksum恢复4→4/关闭全0及两帧Stopped通过。**下一唯一Task `NTSD28-Q06-C25-DEAD-CHARACTER-EXTRA-BOUNCE-AUDIT-001 / READY_SOURCE_CALLER_MAPPING`**，先查C25额外death bounce与held关系，不直接删真实hit/physics反应；随后核验完整SelfCheck最新type2落地方向FAIL（已过GT08/GT09），再回fragment OID0/999准入及完整driver/父frame。15/23/26/2/2、raw47/3保持；Q07资源未迁移，Q06/总目标ACTIVE。用户HUDBg x30/Scene bcd1047b…保持，禁止computer-use及非战斗/Unity-GAS框架改动。证据见两个STATE9998 REPORT；不得把6/7或限定Play改写为完整对齐。

> 当前执行 NTSD28-Q06-STATE9998-LEGACY-CLEANUP-RETIREMENT-001 IN_PROGRESS / TEST_FIRST；原完整driver224/672全存活，移除Unity额外9998删除须先RED并保留其余Serial职责。GT08 17/17及完整SelfCheck已越过，后续landing matrix有独立首差异。禁止computer-use/非战斗修改。

> NTSD28-Q06-STATE9998-SOURCE-DRIVER-WITNESS-001 IN_PROGRESS；先原完整driver测量state9998再裁决旧Serial残余，不依据grep直接删除。

> 当前执行 NTSD28-Q06-GT08-LIFECYCLE-FIXTURE-REBASELINE-001 IN_PROGRESS，准确两测试脚本；GT08改实际producer及Native state独立断言，GT09 state9998原链缺证另建见证，不沿用旧权威。禁止computer-use。

> **当前恢复游标（2026-09-14）：** 原函数WEAPON-PIECE-SOURCE-WITNESS已VERIFIED；NATIVE-WEAPON-PIECE-TRANSACTION为 **FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / FULL_TRANSACTION_INCOMPLETE**。六脚本两阶段及两个factory专用出生已写，两profile各157/762片与实际3/50片0差异、49联合通过；真实旧内容Scene DataOriented完整Late pass四向量（两factory×healthy0片/broken5片）通过，每次checksum恢复4→4，关闭全0/两帧Stopped。RAW-OBJECT-TYPE-PROJECTION与三个fixture Record已限定VERIFIED。**完整SelfCheck仍FAIL，已越过武器/LC02/GT07，下一唯一Task `NTSD28-Q06-GT08-LIFECYCLE-FIXTURE-REBASELINE-001 / READY_SOURCE_AND_FIXTURE_MAPPING`**：旧1299→HitStun=-199与mock/Native pending须成组核验；随后fragment OID0/999/非法动作/pool失败/高低slot完整driver边缘，再回父FRAME-TRANSACTION。15/23/26/2/2、raw47/3，Q07正式资源未迁移；Q06/总目标ACTIVE。用户已确认HUDBg x30归本人/其他任务，Scene bcd1047b…保持。禁止computer-use、非战斗或Unity/GAS框架改动。证据详见fragment REPORT；先前SELF_CHECK_PENDING_WEAPON_PIECES被本游标更新，不能沿用“producer未写”。

> NTSD28-Q06-GT07-BREAK-TYPE-MATRIX-FIXTURE-001 IN_PROGRESS，准确GT07矩阵段，原FAIL保留。

> NTSD28-Q06-LC02-INVALID-FRAME-FIXTURE-001 IN_PROGRESS，单SelfCheck构造器两个赋值857→1000；原FAIL存档。

> NTSD28-Q06-WEAPON-BREAK-SELF-CHECK-FIXTURE-001 IN_PROGRESS，单SelfCheck脚本准确fixture100/999与Native pending，原FAIL保留。

> NTSD28-Q06-RAW-OBJECT-TYPE-PROJECTION-001 IN_PROGRESS：仅两脚本raw投影，当前粗分类objectType不能称完整绑定，先七类型RED。父武器生成继续。

> 当前执行 NTSD28-Q06-NATIVE-WEAPON-PIECE-TRANSACTION-001 / IN_PROGRESS / TEST_FIRST；原函数见证VERIFIED（157+3例/762+50片），准确六脚本两生成阶段及两factory出生适配。完整SelfCheck仍FAIL，禁止只改断言。用户确认HUDBg x30为本人/其他任务修改，保留。禁止computer-use。

> 当前执行 NTSD28-Q06-WEAPON-PIECE-SOURCE-WITNESS-001 / IN_PROGRESS / SOURCE_MODEL_DIAGNOSTIC_ONLY；一个native runner验证两种碎片的RNG/slot/完整出生规则，Unity未改，父碎片实现继续。禁止computer-use。

> **当前恢复游标（2026-09-14）：** LIFECYCLE-STATE-CARRIER-001为 **FOCUSED_TEST_PASS / SCOPED_PLAY_PASS**；父FRAME-TRANSACTION-INTEGRATION仍IN_PROGRESS。三字段/copy/checksum/raw及C25尾部接线已写，当前 **15/23/26/2/2、raw47/3**；2676×22两端点/实际Late两路径、41 focused、386/386联合、88工具、真实Scene encoded恢复4→4/关闭全0两帧Stopped通过。Module已改，旧857/HitStun消费者已移除，private shadow已移除。**完整SelfCheck FAIL（旧PendingFlushDestroy断言且OID100无碎片旧假设），碎片producer确实未实现；不能只改断言变绿。下一唯一Task `NTSD28-Q06-NATIVE-WEAPON-PIECE-TRANSACTION-001 / READY_SOURCE_WITNESS_AND_EXACT_RECORD`**，完整内置+DAT生成/slot/RNG/出生及SelfCheck native fixture后返回父验收。原始FAIL/invalid raw原因与修复都留证；Scene用户bcd1047b…保持，CS0/dirtyfalse/root14。总目标ACTIVE，禁止computer-use/非战斗改动。

> 当前执行 NTSD28-Q06-NATIVE-LIFECYCLE-STATE-CARRIER-001 / IN_PROGRESS / TEST_FIRST，三字段及15/23/26/2/2、raw47/3联合迁移未发布；父frame紧接着消费，不保留private影子。禁止computer-use。

> **当前恢复游标（2026-09-14）：** `NTSD28-Q06-NATIVE-FRAME-TRANSACTION-INTEGRATION-001` **IN_PROGRESS / CORE_FOCUSED_PASS / FULL_TRANSACTION_INCOMPLETE**。正式C25 core已写，2676例×14字段由9103差异降到0；core9+相关19最终28/28通过，独立C25-RENDER-PHASE-20-FIXTURE已VERIFIED_TEST_ONLY。**Module尚未修改，仍旧857/HitStun消费，不能做完整高位Play或发布完整frame对齐。下一必要Task `NTSD28-Q06-NATIVE-LIFECYCLE-STATE-CARRIER-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**：runtime_state_code与render_phase分离，建立pending/code持久真值并替换当前未接线private结果，随后返回本事务按OPoint/state18/078/weapon pieces/lifecycle顺序闭合。两生产+新测试已变，未跑本轮完整SelfCheck/Play；14/22/25/2/2保持，Scene用户bcd1047b…保持。总目标/Q06 ACTIVE，禁止computer-use/非战斗改动。

> 附属测试修正 NTSD28-Q06-C25-RENDER-PHASE-20-FIXTURE-001 / IN_PROGRESS，纠正旧action202自动写20假设，父FRAME-TRANSACTION仍IN_PROGRESS。

> 当前执行 NTSD28-Q06-NATIVE-FRAME-TRANSACTION-INTEGRATION-001 / IN_PROGRESS / TEST_FIRST；准确四脚本先Unity原2676 core RED与C25成组接线，完整driver/particles/weapon pieces等出口未完不关闭。14/22/25/2/2保持，禁止computer-use。

> **当前恢复游标（2026-09-14）：** `NTSD28-Q06-NATIVE-SOUND-LATCH-CARRIER-001` 已 VERIFIED / SOUND_LATCH_CARRIER_AND_SCHEMA_ONLY：独立NativeSoundActionLatch默认/reset -1、canonical copy/ECS fingerprint/完整checksum/full parity，联合版本 **14/22/25/2/2** 已344/344、完整SelfCheck、真实Scene sound857/frame1分离保存/两checksum恢复4→4及关闭全0/两帧Stopped验证。工具88/88，新鲜native/Unity content与五版本头一致，原raw50仍44相等/6MISSING（新sound latch不在该raw表）。准确22脚本，Scene用户HUDBg x30/bcd1047b…保持，CS0/dirtyfalse/root14，无新增缺失。**下一唯一Task `NTSD28-Q06-NATIVE-FRAME-TRANSACTION-INTEGRATION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**：数据前置已备，直接依据2676矩阵和CALLER-MAP做Unity RED/成组frame、成本、终止及声音事件接线，定义/fusion的latch重置待；实际WAV播放由Q10回访，不形成循环。Q06/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式资源未迁移；禁止computer-use/非战斗改动。

> 当前执行 NTSD28-Q06-NATIVE-SOUND-LATCH-CARRIER-001 / IN_PROGRESS / TEST_FIRST，Runtime独立声音latch数据前置及entity14/aggregate22/checksum25未发布联合迁移；shell2/2不变，producer仍由父帧事务后继。禁止computer-use。

> **当前恢复游标（2026-09-14）：** `NTSD28-Q06-NATIVE-FRAME-STEP-LIFECYCLE-SOURCE-WITNESS-001` 已 VERIFIED / SOURCE_MODEL_DIAGNOSTIC_ONLY：仅一native诊断脚本，真实World step_frame_slot/resolve_pending_lifecycle完成2676向量，复跑逐字节一致；正式EXE/75源码身份保持。Unity生产本轮未改。**下一唯一Task `NTSD28-Q06-NATIVE-FRAME-TRANSACTION-INTEGRATION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**：读取CALLER-MAP与矩阵，先完整数据/行为Record；正负999/YReference、原destination负HP/MP成本及fallback、212 stayed、encoded reset的latch/collision镜像、sound独立latch及完整driver tail必须闭合。禁止仅替换857或把两个端点见证当完整driver。前快照绑定244/SelfCheck/Play成果保持，用户Scene bcd1047b…保持；Q06/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式资源未迁移，禁止computer-use/非战斗改动。

> 当前执行 NTSD28-Q06-NATIVE-FRAME-STEP-LIFECYCLE-SOURCE-WITNESS-001 / IN_PROGRESS / SOURCE_MODEL_DIAGNOSTIC_ONLY；仅一native诊断脚本，先完整帧成本/生命周期原函数向量，Unity生产不变。禁止computer-use。

> **当前恢复游标（2026-09-14）：** `NTSD28-Q06-NATIVE-FRAME-SNAPSHOT-BINDING-001` 已 VERIFIED / SNAPSHOT_NATIVE_DESCRIPTOR_BINDING_ONLY：准确三脚本，Native current/collision descriptor原地与跨World恢复；20项RED16失败→20/20，联合244/244、完整SelfCheck、真实Scene857/998两份checksum恢复4→4及关闭全0/两帧Stopped通过。13/21/24/2/2不变，未证明高位动作完整tick。**下一唯一Task `NTSD28-Q06-NATIVE-FRAME-RUNTIME-READER-MIGRATION-001 / READY_FRAME_STEP_LIFECYCLE_MAPPING`**：frame绑定/direct/next/cost/terminal必须成组，source数据声明999可读但C25存活<999，负next先翻面再999解析；先完整调用表与原函数见证，不能批量857→1000。父零帧reader、其余display出生、post、Q07正式资源均未完。Scene用户HUDBg x30/bcd1047b…保持，CS0/dirtyfalse/root14，无新增缺失。总目标/Q06 ACTIVE/FULL_ALIGNMENT_INCOMPLETE；禁止computer-use与非战斗/Unity-GAS框架改动。

> 当前实施 NTSD28-Q06-NATIVE-FRAME-SNAPSHOT-BINDING-001 / IN_PROGRESS / TEST_FIRST，准确三脚本快照帧绑定；数据声明999可读与C25存活<999是不同合同，frame推进/终止成组后继，禁止computer-use/非战斗改动。

> **当前恢复游标（2026-09-14）：** `NTSD28-Q06-NATIVE-FRAME-ACCESSOR-RESOURCE-ADMISSION-001` 已 VERIFIED / NATIVE_ACCESSOR_AND_RESOURCE_OWNER_ADMISSION_ONLY：独立Native查询支持0..998零帧/声明999，旧Get/Has/Max857保持；仅资源资格/chp/cmp迁移。原函数81行（63合法/18错误AST）、223/223＋独立旧接口1/1、完整SelfCheck、实际资源owner两入口857/998/999六例/恢复4→4/关闭全0及两帧Stopped通过。INVALID-FRAME-FIXTURE已VERIFIED_TEST_ONLY；HP/MP两个Record恢复限定资源事务VERIFIED，但不能扩大为高位动作完整tick。**下一唯一Task `NTSD28-Q06-NATIVE-FRAME-RUNTIME-READER-MIGRATION-001 / READY_FOR_LIVE_CALLER_MAPPING`**：先实际frame绑定/direct/next/快照读取，逐组迁移其它live reader；父ZERO-FRAME-CACHE-CONTRACT、完整display与post均未完。原388检索清单为改前，406条最新候选见reader-inventory-after-accessor.json，不全是live调用方。Scene用户HUDBg x30/bcd1047b…保持，CS0/dirtyfalse/root14，资源未迁移；13/21/24/2/2与所有例外不变，总目标/Q06 ACTIVE/FULL_ALIGNMENT_INCOMPLETE，禁止computer-use/非战斗改动。

> NTSD28-Q06-NATIVE-INVALID-FRAME-FIXTURE-CORRECTION-001 / PLANNED / TEST_ONLY；HP/MP旧invalid7夹具改真正无效9999，随当前Native访问器包继续。

> 当前实施 NTSD28-Q06-NATIVE-FRAME-ACCESSOR-RESOURCE-ADMISSION-001 / IN_PROGRESS / TEST_FIRST，四脚本新增Native访问器并修资源读取；旧API保持，全部reader迁移/父零帧任务未完。禁止computer-use。

> **当前恢复游标（2026-09-14）：** `NTSD28-Q06-OPOINT-SPAWN-VITALS-TRANSACTION-001` 已 VERIFIED / OPOINT_VITALS_AND_DISPLAY_BIRTH_ONLY：3716 native、329实际Logan正OID（0按原规则跳过）、212/212、完整SelfCheck、两完整生成路径各两次真实Play/原模式与checksum恢复4→4/关闭全0及两帧Stopped通过。五脚本、仅三生产文件；正式资源/Scene不变，用户HUDBg x30与bcd1047b…保持，CS0/dirtyfalse/root14。**下一唯一Task `NTSD28-Q06-NATIVE-ZERO-FRAME-CACHE-CONTRACT-001 / READY_READONLY`**：native0..998隐式零帧与Unity缓存857/HasFrame声明判定不同；HP、MP两个资源Record已降为 FOCUSED_TEST_PASS / REOPENED_ZERO_FRAME_QUALIFICATION，原有效声明帧公式/phase/Play证据保留。先闭合该reader合同和必要修复，再回DISPLAY-PROGRESSION补非OPoint出生初值/联验，随后POST-DISPLAY。父display/Q06/总目标仍ACTIVE / FULL_ALIGNMENT_INCOMPLETE；13/21/24/2/2、Unity/GAS/非战斗/例外保持，禁止computer-use。

> 资格覆盖纠正：NTSD28-Q06-NATIVE-HP-RESOURCE-TRANSACTION-001 与 NTSD28-Q06-NATIVE-MP-RESOURCE-TRANSACTION-001 降为 FOCUSED_TEST_PASS / REOPENED_ZERO_FRAME_QUALIFICATION。已测有效声明帧成果保留；未声明0..998/cache857/HasFrame需ZERO-FRAME-CACHE-CONTRACT独立闭合，当前出生资源继续。

> 当前实施 NTSD28-Q06-OPOINT-SPAWN-VITALS-TRANSACTION-001 / IN_PROGRESS / TEST_FIRST，准确五脚本。新增零帧reader差异须独立追踪，不能将旧HP/MP有效帧通过推广；父display出生仍待，禁止computer-use。

> **当前恢复游标（2026-09-14）：** `NTSD28-Q06-NATIVE-DISPLAY-PROGRESSION-001` 仍 IN_PROGRESS，当前递推/slot阶段 FOCUSED_TEST_PASS / SCOPED_PLAY_PASS：新13测试含980 native全部通过，相关181不同测试经177PASS/4旧FAIL＋独立6PASS逐项闭合，完整SelfCheck/真实两tick显示与source真值保持/checksum恢复4→4/关闭全0及两帧Stopped通过。独立C25-POISON-PHASE-FIXTURE、C25-DISPLAY-SCHEMA-FIXTURE均VERIFIED_TEST_ONLY。**出生初始化未完成，不能关闭完整display。下一唯一Task `NTSD28-Q06-OPOINT-SPAWN-VITALS-TRANSACTION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**：原OPoint hp/mp及ohp/omp选择/百分比与两生成入口先原子处理，再返回display补全部出生初值与联验，随后POST-DISPLAY（WAIT_DISPLAY_OWNER）。准确四脚本+两个独立测试修正，无非战斗/资源/Scene变更；用户HUDBg x30/Scene bcd1047b…保持，CS0/root14/dirtyfalse。13/21/24/2/2保持，R05/R07显示子条件PARTIAL_RETURN；Q06/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，禁止computer-use。

> NTSD28-Q06-C25-DISPLAY-SCHEMA-FIXTURE-001 / IN_PROGRESS / TEST_ONLY，独立旧fixture纠正；父display任务及出生依赖仍待。

> NTSD28-Q06-C25-POISON-PHASE-FIXTURE-001 / IN_PROGRESS / TEST_ONLY，独立旧fixture纠正；父display任务及出生依赖仍待。

> 当前实施 NTSD28-Q06-NATIVE-DISPLAY-PROGRESSION-001 / IN_PROGRESS / TEST_FIRST，准确四脚本递推+slot适配；出生初值发现OPoint hp/mp/ohp/omp依赖，未处理前不能关闭完整display任务。禁止computer-use/非战斗改动。

> **当前恢复游标（2026-09-14）：** `NTSD28-Q06-DISPLAY-POST-DISPLAY-RESOURCE-AUDIT-001` 已 VERIFIED_AUDIT_ONLY；`NTSD28-Q06-DISPLAY-POST-DISPLAY-SOURCE-WITNESS-001` 已 VERIFIED / SOURCE_MODEL_DIAGNOSTIC_ONLY（980 display＋2379 post向量，直接stdout复跑逐字节一致，正式EXE及75源码/header身份复核）。确认生产C25缺display/post owner，且display需出生初值和独立slot资格；frame_0mp保留原frame状态、display先于limit等边界已测。**下一唯一Task `NTSD28-Q06-NATIVE-DISPLAY-PROGRESSION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**：先闭合现有出生/复用/clone与slot适配，再准确Record实施完整C25d；随后POST-DISPLAY-RESOURCE-TRANSACTION（WAIT_DISPLAY_OWNER）。本轮仅一新native诊断脚本，HP/MP六脚本hash和用户HUDBg x30/Scene bcd1047b保持，无新Unity/Play结论；前HP134/SelfCheck/Play成果保留。R05/R07待生产接通再回访，Q07正式资源未迁移，13/21/24/2/2保持，总目标/Q06 ACTIVE / FULL_ALIGNMENT_INCOMPLETE。禁止computer-use与非战斗改动。

> 当前必要诊断 NTSD28-Q06-DISPLAY-POST-DISPLAY-SOURCE-WITNESS-001 / IN_PROGRESS / SOURCE_MODEL_ONLY，一新runner；父资源显示/post-display审计继续，禁止computer-use，不改Unity生产。

> **当前恢复游标（2026-09-14）：** Q06 HP事务 `NTSD28-Q06-NATIVE-HP-RESOURCE-TRANSACTION-001` 已 VERIFIED / SCOPED_HP_TRANSACTION_AND_DEFAULT_PRODUCTION_PASS；134/134（含3768 HP及2028 MP原函数向量）、两profile真实Logan12tick、完整SelfCheck、旧内容真实Scene HP/恢复4→4/有序关闭全0与两帧Stopped通过。独立 `NTSD28-Q06-HP-SELF-CHECK-WORLD-CONTEXT-001` 已 VERIFIED_TEST_ONLY。正式默认mode28=1已接；下方旧计划mode0/未写/待验为历史。Scene用户HUDBg x30与bcd1047b…保持，CS0/dirtyfalse/root14，无新增缺失。**下一唯一Task `NTSD28-Q06-DISPLAY-POST-DISPLAY-RESOURCE-AUDIT-001 / READY_READONLY`**；先资源display/post-display消费链审计，再精确实施，Q08正式模式投影及Q07资源迁移仍待。13/21/24/2/2保持，Q06及总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE；禁止computer-use及非战斗改动。

> NTSD28-Q06-HP-SELF-CHECK-WORLD-CONTEXT-001 / IN_PROGRESS / TEST_ONLY；GT-06无World夹具补私有World phase，保持HP/MP及反向类型断言，不改生产。

> HP默认来源纠正：正式GameSession/scenario默认28=1，核心ResourceSystemRules孤立默认0不能替代playable入口；生产将传1。HP原版3768向量/13项RED11已取得，追加Logan12tick见证。

> 当前执行 NTSD28-Q06-NATIVE-HP-RESOURCE-TRANSACTION-001 / IN_PROGRESS / TEST_FIRST，准确五脚本；HP完整分支与共同资格、World phase12、不可变mode28默认0；保留MP成果及用户确认HUDBg x30/Scene bcd1047b…，禁止computer-use/非战斗改动。

> **2026-09-14最新确认与游标：** 用户已确认HUDBg x50→30为自己或其他任务修改，Scene bcd1047b…现状必须保留，旧SCENE_ORIGIN_PENDING标记已解除。NTSD28-Q06-NATIVE-MP-RESOURCE-TRANSACTION-001 已VERIFIED / SCOPED_MP_TRANSACTION_AND_DEFAULT_PRODUCTION_PASS（2028 native/119 tests/SelfCheck/实际MP及关闭全0，原数值首差闭合，只剩6MISSING）。下一唯一Task NTSD28-Q06-NATIVE-HP-RESOURCE-TRANSACTION-001 / READY_FOR_EXACT_PRECHANGE_RECORD；Q08正式mode投影和其他Q06/资源迁移仍待。禁止computer-use及非战斗改动，总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> **Q06 MP限定出口（2026-09-13）：** NTSD28-Q06-NATIVE-MP-RESOURCE-TRANSACTION-001 / FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / SCENE_ORIGIN_PENDING。完整MP事务及两caller已接，native2028向量、119/119、完整SelfCheck、真实Scene tick5→6默认MP200/显式mode0=201/weak抑制/恢复4→4/关闭全0通过。当前同源raw44已绑定字段一致，只剩原6MISSING，MP200/201差异已消除；NTSD28-Q06-DEFAULT-MP-RECOVERY-FIXTURE-CORRECTION-001已VERIFIED_TEST_ONLY，原34FAIL与SelfCheck旧断言留证。**Scene HUDBg x50→30，SHA bcd1047b…，15:38:24Z保存，早于Play；已异步询问来源，保留不回退，不能写Scene unchanged。** 下一唯一Task NTSD28-Q06-NATIVE-HP-RESOURCE-TRANSACTION-001 / READY_FOR_EXACT_PRECHANGE_RECORD，可继续独立HP工作。Q08正式mode注入仍待，13/21/24/2/2及所有例外保持，正式资源未迁移，总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，禁止computer-use及非战斗改动。

> NTSD28-Q06-DEFAULT-MP-RECOVERY-FIXTURE-CORRECTION-001 / IN_PROGRESS / TEST_ONLY；34个旧默认MP/phase断言按当前native纠正，生产规则保持。

> 当前执行 NTSD28-Q06-NATIVE-MP-RESOURCE-TRANSACTION-001 / IN_PROGRESS / TEST_FIRST，准确五脚本；完整MP事务+两caller，显式不可变mode值/source默认1和F6输入，不加未校验World字段或版本。先native分支见证和RED，禁止computer-use及非战斗改动。

> **当前Q06入口（2026-09-13）：** NTSD28-Q06-RESOURCE-MP-FIRST-DIFFERENCE-AUDIT-001 已 VERIFIED_AUDIT_ONLY / CAUSE_CONFIRMED。当前正式EXE/runner及75源码-header hash已复核；四次native模式原值对照确认gate恰1抑制普通MP恢复，0/2/-1则tick3增加1。当前Unity新鲜capture 1/1 PASS且内容头完整同值，MP差异仍200/201；未修改脚本/资源，不能写成已修。下一唯一Task **NTSD28-Q06-NATIVE-MP-RESOURCE-TRANSACTION-001 / READY_FOR_EXACT_PRECHANGE_RECORD**：完整frame.cmp/regen族/阈值/bound/weak/F6/mode/限幅及两生产caller，明确不可变模式输入与Q08投影边界，不偷加未校验mutable字段/版本。Q05限定交付保持；BATCH-03/Q06 ACTIVE，总目标FULL_ALIGNMENT_INCOMPLETE，禁止computer-use及非战斗改动。

> **当前恢复游标（2026-09-13）：BATCH-02 / Q05 已限定交付，下一 BATCH-03 / Q06。** NTSD28-Q05-JOINT-SNAPSHOT-RESTORE-REPLAY-VALIDATION-001、NTSD28-Q05-SNAPSHOT-RETIRED-SHELL-POOL-RETURN-001、NTSD28-Q05-SNAPSHOT-RENDERER-REGISTRY-RETENTION-001 均 VERIFIED / SNAPSHOT_REPLAY_SCOPE_ONLY。实际Logan两profile24tick/22tick重放、slot/pool/错误identity、最终82/82、完整SelfCheck、两次旧内容真实Scene恢复4→4/关闭全0/两帧Stopped/重入通过；CS0，Scene旧SHA/root14/dirtyfalse。三Record共6脚本，仅2个生产snapshot文件，保留Unity/GAS/非战斗。13/21/24/2/2联合schema基线已验证，trace3/raw-source2/50字段保持；正式资源未迁移、六MISSING及MP200/201仍由Q06/后继解决。下一唯一Task **NTSD28-Q06-RESOURCE-MP-FIRST-DIFFERENCE-AUDIT-001 / READY_READONLY**。禁止computer-use；总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。以下较早启动语句仅历史，不应重开已验Q05。

> 当前必要修复 `NTSD28-Q05-SNAPSHOT-RENDERER-REGISTRY-RETENTION-001 / IN_PROGRESS / TEST_FIRST`：真实Scene两个Renderer计入ObjectCount但不占战斗槽，原地restore拒绝；准确三脚本保留原注册/活动计数分域。父Q05未关闭，pool修复保持，禁止computer-use。

> NTSD28-Q05-WORLD-CLOCK-PHASE-FIXTURE-001 / IN_PROGRESS / TEST_ONLY，clock相邻phase顺序夹具纠正；生产pass保持。

> Q05真实恢复验证发现退休shell未归还pool；当前必要修复 `NTSD28-Q05-SNAPSHOT-RETIRED-SHELL-POOL-RETURN-001 / IN_PROGRESS / TEST_FIRST`，准确2脚本。移动回放两profile已过，攻击测试入口需改用既有logic-only executor；禁止computer-use，父验收继续。

> 当前执行 `NTSD28-Q05-JOINT-SNAPSHOT-RESTORE-REPLAY-VALIDATION-001 / IN_PROGRESS / VALIDATION_IMPLEMENTATION`，准确2个Editor脚本，真实Logan两profile/24tick恢复回放与pool/slot验收；生产规则保持，禁止computer-use，版本13/21/24/2/2未发布。

> **Q05 trace/raw身份与字段限定出口（2026-09-13）：** `NTSD28-Q05-TRACE-RAW-IDENTITY-JOINT-UPGRADE-001 / FOCUSED_TEST_PASS / SAME_CONTENT_CAPTURE_PASS / RAW_PARITY_DIFFERENT`。19脚本，trace v3/raw-source v2/50字段44绑定6MISSING；实际Logan输入native/Unity raw4EFE/semanticDB57/projection3900完整同值，native复跑字节相同。50不同Unity tests、88工具tests、native2F8非默认unit、完整SelfCheck、旧内容真实Play/零新增缺失通过；Scene旧SHA保持，禁止computer-use。已修诊断入口当前MP误作最大MP及首差排序，生产战斗规则未改。真实3tick比较仍7类差异：原6MISSING与tick3 currentMp native200/Unity201（Q06待追实际consumer）。**下一唯一Task `NTSD28-Q05-JOINT-SNAPSHOT-RESTORE-REPLAY-VALIDATION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**，完成同版本有意义的restore/replay/slot-pool/重入验收再关Q05；13/21/24/2/2仍未发布，Q05/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式资源未迁移。


> 当前执行 `NTSD28-Q05-TRACE-RAW-IDENTITY-JOINT-UPGRADE-001 / IN_PROGRESS / TEST_FIRST`，准确19脚本（含CLI退出码与独立native字段见证），真实内容根/语义头/trace v3/raw v2/50字段；13/21/24/2/2未发布，禁止computer-use，非战斗/正式资源保持。

> **Q05五版本及恢复头部已限定验证（2026-09-13）：** `NTSD28-Q05-JOINT-SNAPSHOT-CHECKSUM-VERSION-001 / FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / TRACE_IDENTITY_PENDING`。当前正式代码常量已为 **13/21/24/2/2**，仍INTERMEDIATE_UNPUBLISHED_Q05_WINDOW；修复外层有效而内层旧版仍可恢复的漏洞，复用原发布全子域header predicate。RED11+1→最终287不同测试有通过证据（主286PASS/1旧phaseFAIL经独立`NTSD28-Q05-WORLD-CLOCK-PHASE-FIXTURE-001`定向1PASS闭合），新增23全PASS、完整SelfCheck/CS0/真实暂停World双队列Play tick5对象4→4通过。`NTSD28-Q05-REMAINING-CONTENT-HASH-CONSUMER-AUDIT-001`已VERIFIED_AUDIT_ONLY：未发现额外frame/meta生产hash漏项，明确trace仍strategy-pending/缺完整语义头/49字段。**下一唯一Task `NTSD28-Q05-TRACE-RAW-IDENTITY-JOINT-UPGRADE-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**，同窗口trace v3/raw/source v2/50字段2F8及真实source/raw/decode/semantic/schema绑定，再完整replay/Play；Q05/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式资源未迁移。Scene旧SHA/Foot18既有缺失保持，禁止computer-use及非战斗改动。


> NTSD28-Q05-WORKER-LATE-OPOINT-FLOOR-FIXTURE-001 / IN_PROGRESS / TEST_ONLY，旧worker Y45预期按native落地改5并断言parent0；不改生产规则。

> 当前执行 `NTSD28-Q05-JOINT-SNAPSHOT-CHECKSUM-VERSION-001 / IN_PROGRESS / TEST_FIRST / INTERMEDIATE_UNPUBLISHED`，准确15脚本，五版本目标13/21/24/2/2；hash消费者审计已闭合，trace身份/50字段后继，禁止computer-use。

> **Q05快照边界限定出口（2026-09-13）：** `NTSD28-Q05-OPOINT-SNAPSHOT-BOUNDARY-GUARD-001 / FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / JOINT_SCHEMA_PENDING`。11脚本统一完整Host/core/worker/kernel tick、structural及两OPoint owner前置；原body/pass保持，拒绝无队列/World/worker副作用。RED14与HostRED1→最终187/187、完整SelfCheck、真实暂停World tick5/对象4→4双队列拒绝与空闲capture通过；该Play无dedicated worker，不宣称物理技能或worker实战。独立`NTSD28-Q05-WORKER-LATE-OPOINT-FLOOR-FIXTURE-001 / VERIFIED_TEST_ONLY`按native落地修旧child45→5并断言parent0；首次175中174PASS/1旧FAIL留证。Scene旧SHA/Foot18缺失保持，无新增缺失，CS0。**下一唯一Task `NTSD28-Q05-REMAINING-CONTENT-HASH-CONSUMER-AUDIT-001 / READY_READONLY_CONTRACT`**，再联合13/21/24/2/2、trace/replay；当前12/20/23/1/1未发布，Q05/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式资源未迁移，禁止computer-use及非战斗改动。


# CODEX-CURRENT-HANDOFF

> 当前执行 `NTSD28-Q05-OPOINT-SNAPSHOT-BOUNDARY-GUARD-001 / IN_PROGRESS / TEST_FIRST`，准确9脚本，统一tick/structural/双队列/host快照前置，拒绝无副作用；不改pass和关闭顺序，禁止computer-use，版本/trace后继。


> **Q05语义身份已接线并限定验证（2026-09-13）：** `NTSD28-Q05-SEMANTIC-CONTENT-IDENTITY-001 / FOCUSED_TEST_PASS / SCOPED_PUBLICATION_PLAY_PASS / JOINT_SCHEMA_PENDING`。raw DAT/visual算法保持，V2 tag+raw32 SHA256/LE ulong进入catalog/candidate/cache、两publisher和本地验证session；75主回归PASS、Visual旧类6PASS/1过期六DAT断言由`NTSD28-Q05-FORMAL-VISUAL-CANDIDATE-ADMISSION-FIXTURE-001 / VERIFIED_TEST_ONLY`定向1PASS闭合（82不同focused有通过证据），完整SelfCheck/独立Python hash通过。正式330/906输入capture成功；隔离native格式源实际menu重进cache1/三key同/World4/46资源全释放/borrower0/两帧Stopped通过，非正式330全渲染或整技能结论。**下一唯一Task `NTSD28-Q05-OPOINT-SNAPSHOT-BOUNDARY-GUARD-001 / READY_FOR_EXACT_PRECHANGE_RECORD`**，继续父步骤3双OPoint guard，再相关内容hash/联合版本/trace/replay。当前12/20/23/1/1未发布，Q05/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式资源未迁移；禁止computer-use，Unity/GAS/非战斗、Scene旧SHA/Foot18缺失/例外保持。


> `NTSD28-Q05-FORMAL-VISUAL-CANDIDATE-ADMISSION-FIXTURE-001 / IN_PROGRESS / TEST_ONLY`更正已解决六DAT后遗留的formal candidate拒绝断言；父semantic identity75PASS，SelfCheck/Play后继。


> 当前执行 `NTSD28-Q05-SEMANTIC-CONTENT-IDENTITY-001 / IN_PROGRESS / TEST_FIRST`，准确7脚本接冻结semantic身份、现有cache/publication与本地验证；raw指纹/协议保持，双OPoint guard/版本/trace后继，禁止computer-use。


> **Q05 HolderCopy载体已清理，步骤2限定出口（2026-09-13）：** `NTSD28-Q05-HOLDERCOPY-CARRIER-RETIREMENT-001 / FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / JOINT_SCHEMA_PENDING`。36脚本/13生产仅runtime/Entity/task/ECS/校验/HitPlan旧载体删除，bit33退休空洞、真实关系保持；首次863=859PASS/4旧统计预期FAIL，独立`NTSD28-Q05-RETIRED-HOLDER-STATS-FIXTURE-CORRECTION-001 / VERIFIED_TEST_ONLY`经27中26PASS+最窄1PASS逐项闭合（保留Cpoint原nativeKO1）。完整SelfCheck与真实pickup/replacement/current OPoint tick/注销Play通过，去旧holderCopy后的有效见证前后完全一致/对象4→4。**下一唯一Task `NTSD28-Q05-CONTENT-IDENTITY-AND-CAPTURE-BOUNDARY-001 / READY_FOR_EXACT_PRECHANGE_RECORD`（父步骤3）**，再联合13/21/24/2/2及trace/回放；五类载体不重做。当前12/20/23/1/1未发布，Q05/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。禁止computer-use；Unity/GAS/非战斗、Scene旧SHA/Foot18缺失/例外保持，正式资源未迁移。


> `NTSD28-Q05-RETIRED-HOLDER-STATS-FIXTURE-CORRECTION-001 / IN_PROGRESS / TEST_ONLY`处理父HolderCopy focused863中的4旧统计预期冲突；production不改，父SelfCheck/Play待。


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

> 附属test-only `NTSD28-Q05-SPRITE-CORPUS-ADMISSION-FIXTURE-CORRECTION-001 / IN_PROGRESS`：旧Q02全语料夹具新增原版3失败应拒绝断言，402有效逐sheet保持；BMP/stats主包继续。

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

> 当前执行 `NTSD28-Q05-NATIVE-STRENGTH-TABLE-ADMISSION-001 / IN_PROGRESS / TEST_FIRST`；现有Scene变化保护，来源确认未回，不重复询问。

> **Q05-A2 ITR40/strength19 record已写（2026-09-13）：** `NTSD28-Q05-ITR40-STRENGTH19-RECORD-DECODING-001 / FOCUSED_TEST_PASS / TABLE_SOURCE_INTEGRATION_PENDING`保持活跃；三字段/clone/projection/hash、40/19单记录decoder已落盘。RED120→初次488/489，改用native真实FieldBag后最终489/489，完整SelfCheck PASS、CS0、旧138无新增旧投影差异。表头合同已纠正：native仅1..9/拒绝重复，caption非字段，catalog拒绝错误definition。**Scene文件12:15:20多出HUDCamera/ScenesCamera/Canvas disabled及UI坐标差异，ORIGIN_PENDING；已异步询问用户，保留未回退，isDirty=false不可当Scene unchanged。** 下一唯一Task `NTSD28-Q05-NATIVE-STRENGTH-TABLE-ADMISSION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`；source/identity/schema/Play及其他后继保持，总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> 当前执行 `NTSD28-Q05-ITR40-STRENGTH19-RECORD-DECODING-001 / IN_PROGRESS / TEST_FIRST`；表头准入已按native更正为1..9/拒绝重复；整表和来源接线后继。

> **Q05-A2 Geometry已写/待接线（2026-09-13）：** `NTSD28-Q05-GEOMETRY-CONTENT-CONTRACT-001 / FOCUSED_TEST_PASS / SOURCE_AND_ALGORITHM_INTEGRATION_PENDING`保持活跃；BDY ZWidth/HasGeometry、ITR z/有效性及copy/projection/fingerprint已落盘。native58文件88记录双跑一致，RED92→首次371/375，独立`NTSD28-Q05-RELATED-HITPLAN-FIXTURE-CORRECTION-001 / VERIFIED_TEST_ONLY`纠正4旧HolderCopy/kind7夹具后最终375/375；完整SelfCheck PASS、CS0/Scene clean/root14、旧138无新增旧投影差异。没有修候选算法或切来源，Q03旧27首差留Q06。下一唯一Task `NTSD28-Q05-ITR-STRENGTH-CONTENT-CONTRACT-001 / READY_FOR_EXACT_PRECHANGE_RECORD`；CPoint/OPoint/Geometry均在A2/Q05来源/identity/Play回访，其他后继与例外保持，总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> 关联测试修正 `NTSD28-Q05-RELATED-HITPLAN-FIXTURE-CORRECTION-001 / IN_PROGRESS`；Geometry新92通过，完整375中4旧HolderCopy/kind7夹具需按已实施合同修正。

> 当前执行 `NTSD28-Q05-GEOMETRY-CONTENT-CONTRACT-001 / IN_PROGRESS / TEST_FIRST`；CPoint27/OPoint24仍SOURCE_INTEGRATION_PENDING。

> **Q05-A2 OPoint24已写/待接线（2026-09-13）：** `NTSD28-Q05-OPOINT24-CONTENT-CONTRACT-001 / FOCUSED_TEST_PASS / SOURCE_INTEGRATION_PENDING`保持活跃。Value/DTO/adapter24与native block decoder已落盘；RED49FAIL→98PASS（新49+旧6+CPoint43）、native43文件45x24、实际多转单和pool复用、完整SelfCheck PASS；CS0/Scene clean/root14。旧138加载成功，相对CPoint轮无新增投影差异；旧工具19/8投影不证明新27/24。没有切来源、修materializer或发布新版本。下一唯一Task `NTSD28-Q05-GEOMETRY-CONTENT-CONTRACT-001 / READY_FOR_EXACT_PRECHANGE_RECORD`；CPoint与OPoint均在A2/Q05来源/identity/Play回访，全部后继约束保持，总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> 当前执行 `NTSD28-Q05-OPOINT24-CONTENT-CONTRACT-001 / IN_PROGRESS / TEST_FIRST`。CPoint27仍FOCUSED_TEST_PASS/SOURCE_INTEGRATION_PENDING。

> **Q05-A2 CPoint27已写/待接线（2026-09-13）：** `NTSD28-Q05-CPOINT27-CONTENT-CONTRACT-001 / FOCUSED_TEST_PASS / SOURCE_INTEGRATION_PENDING`保持活跃。DTO/value/canonical27与3float32/独立hurt/new8int已落盘，新block decoder未切manager；RED42FAIL/1PASS→实际97+13 PASS、完整SelfCheck PASS、CS0/Scene clean/root14。旧138加载成功，710处throwvz均native float32舍入，不能称数值无变；raw资源未改。CPoint新ABI尚未和外层identity/schema一起完成，禁止发布半迁移baseline。下一唯一Task `NTSD28-Q05-OPOINT24-CONTENT-CONTRACT-001 / READY_FOR_EXACT_PRECHANGE_RECORD`；CPoint接线/投影/identity/Play在A2/Q05回访，旧phase/landing ULP/stage暂缓保持。总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> 当前Q05-A2执行 `NTSD28-Q05-CPOINT27-CONTENT-CONTRACT-001 / IN_PROGRESS / TEST_FIRST`；先9脚本CPoint27契约，不切来源、不升版本、不部署资源。

> **Q05-A1限定交付（2026-09-13）：** `NTSD28-Q05-NATIVE-NUMERIC-DECODER-001 / VERIFIED_NUMERIC_HELPER_ONLY`。RED41失败→Unity43/43，含native3622数值与raw969字段共14742扩展比较0差异；完整SelfCheck PASS、CS0/Scene clean/root14、Ledger475/56PASS，保护3059与Q04出口同差异/零缺失。旧Converter/caller/schema/正式资源未改，不宣称运行时接线。下一唯一Task `NTSD28-Q05-LOGAN-CONTENT-MODEL-INTEGRATION-001 / READY_FOR_EXACT_PRECHANGE_RECORD`，先准确路径Record再同Q05窗口迁移模型/入口；115候选inventory复用。Q05及总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，旧phase/landing ULP与stage暂缓保持。

> **Q05当前执行（2026-09-13）：** `NTSD28-Q05-NATIVE-NUMERIC-DECODER-001 / IN_PROGRESS`，新增加载期pure decoder，旧Converter/caller/schema/正式资源尚未改。RED41失败→Unity41/41；native3622数值10866比较及raw969字段3876比较0差异。扩展两组Unity回归待重载后运行；不是Q05出口。115候选路径inventory已存父Q05工件，下一继续同窗口模型/身份/runtime迁移，不重做Q03/Q04。总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> Q05窗口已启动；当前子Change `NTSD28-Q05-NATIVE-NUMERIC-DECODER-001 / IN_PROGRESS / TEST_FIRST`，准确Record已建，旧caller/schema未改。

> **Q04已交付，下一Q05（2026-09-13）：** Q04-B `NTSD28-Q04-OSCILLATE-CONSUMER-RETIREMENT-001 / VERIFIED_LEGACY_OSCILLATE_READER_ONLY`：RED8FAIL/6PASS→28/28、完整SelfCheck、实际CentralOnly Play slot0偏移/Blink/timeout/延迟速度及共享production快照/恢复PASS，CS0/Scene clean/root14/Ledger474-52PASS。SpriteRenderer前置失败保留，验证改用实际managed表现路径，不宣称GPU全域。Q04-A/B仅行为退休完成，carrier/schema仍旧。下一Q05 Task `NTSD28-Q05-JOINT-CONTENT-RUNTIME-SCHEMA-MIGRATION-001 / READY_PRECHANGE_INVENTORY`，先准确code-path/Record再同窗口迁移；旧phase/landing ULP后继保留，R13行为PARTIAL_RETURN；总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> 当前Q04-B `NTSD28-Q04-OSCILLATE-CONSUMER-RETIREMENT-001 / IN_PROGRESS / TEST_FIRST`；先按Record跑RED，production未改，carrier/schema留Q05。

> **Q04-A已验证，下一Q04-B（2026-09-13）：** `NTSD28-Q04-MASS-FRICTION-GATE-RETIREMENT-001 / VERIFIED_MASS_GATE_ONLY`。production单gate；RED6FAIL/3PASS→focused14/14、完整SelfCheck PASS、真实driver ground/landing三mass0/-2/1一致、cleanup通过、Scene dirtyfalse/root14、CS0、Ledger473/49PASS。首次Play坐标被stage钳制的失败保留，probe已取实际范围中点后通过。相关20项中1个空World旧phase断言失败保留到Q12；另实测既有landing乘1/3与native除3有一ULP首差，加入Q06/B4精确数值待办，不宣称完整landing parity。下一唯一Task `NTSD28-Q04-OSCILLATE-CONSUMER-RETIREMENT-001 / READY_FOR_PRECHANGE_RECORD`，先建Record/RED，只退旧reader。mass/reserved/schema保留到Q05；R13 mass行为PARTIAL_RETURN，总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> **Q04-A当前执行（2026-09-13）：** `NTSD28-Q04-MASS-FRICTION-GATE-RETIREMENT-001 / FOCUSED_TEST_PASS / FULL_SELFCHECK_PASS / PLAY_PENDING`。production仅移除CharacterMechanics mass>0条件；RED6失败/3通过，focused14/14、完整SelfCheck PASS。相关20项有1个空World旧phase[28]断言失败已保留，未改pass。首次Play摩擦正确但fixture Z200被stage min237钳制，cleanup通过；已改probe先warmup取真实stage中点，下一复验Play，不重跑已完成审计或覆盖现有Record。mass/snapshot/schema仍留Q05。总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> 当前实施Change：`NTSD28-Q04-MASS-FRICTION-GATE-RETIREMENT-001 / IN_PROGRESS / TEST_FIRST`；先跑RED，production未改，准确Record已建立。

> **Q03已交付，进入Q04（2026-09-13）：** `NTSD28-NATIVE-DAT-AND-JOINT-FIELD-CONTRACT-AUDIT-001 / DELIVERED_CONTRACT_ONLY`，出口证据Q03-EXIT-REPORT.md。数值见证37 native双跑稳定、Unity源链接37/37，333值113异；几何42项15同/27异。出口复核更正：旧Oscillate晚帧reader/base-shell仍在，Q04仅退reader，Q05统一删载体并base1→2；其producer退休保持。Q05版本集合12→13/20→21/23→24/character1→2/base1→2，当前版本均未改。下一唯一入口Q04-A Task `NTSD28-Q04-MASS-FRICTION-GATE-RETIREMENT-001`，先建立准确Change Record与RED测试；随后Q04-B OSCILLATE-CONSUMER。Q03 numeric Change限定VERIFIED，Ledger472/45PASS。R13/R15仅合同PARTIAL_RETURN，Q02成果保持，正式资源未迁移；总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE、BATCH-02未完成。

> Q03 closed diagnostic Change `NTSD28-Q03-NUMERIC-DECODE-WITNESS-001 / VERIFIED_SOURCE_LINKED_NUMERIC_CAPTURE_ONLY`，同ID Task/Record已建；先完成native位模式见证，production/schema/资源不变。

> **Q03版本/身份合同推进（2026-09-13）：** 同Q03工件 `VERSION-IDENTITY-AND-CAPTURE-CONTRACT.md` 已记录联合12→13/20→21/23→24/character1→2，其他payload保持；OPoint双owner空队列capture/restore前置（不Flush）、语义摘要确定编码、CPoint27顺序及float32 bit规范、339处reserved/alias分类和+2F8两个object-AI reader。只读合同与规范向量，尚非生产实现。下一唯一动作：native数值语法/bit witness，再逐项核对Q03-A/B完整出口；不重复Q02/几何14/42/两条factory清单。父Q03仍IN_PROGRESS，总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，schema/资源/production保持。

> **Q03消费合同推进（2026-09-13）：** OPoint24字段及logic-only/renderer两条factory/initializer/PostInit/copy/reset已记录于同Q03工件 `OPOINT-AND-HELD-DEPTH-CONTRACT.md`。确认显式team与hp/mp有后处理覆盖、缺definition/无slot的整loop终止、未声明0..998零frame、held candidate取holder当前WPoint选择武器strength。WeaponStrength index+8对native index+19的新内容缺口纳入Q05同窗口；不把旧无caller ProcessAttack接成正式路径。此轮只读代码/更新合同，无新脚本/测试/资源变更。父Q03仍IN_PROGRESS；下一为canonical float、reserved/AI全reader、decode-version与Lockstep identity绑定、snapshot的OPoint队列边界。几何14/42见证已交付，不重复；Q02关闭职责保持，总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

> **Q03几何见证已交付（2026-09-13）：** `NTSD28-Q03-COLLISION-GEOMETRY-WITNESS-001 / VERIFIED_CAPTURE_ONLY`；同DAT native14用例与真实Unity三模式42项比较，15相同/27不同，明确深度端点、BDY zwidth、ITR z、负宽度及缺失几何差异；不是parity PASS。正式indexed内容有1127个非零BDY zwidth/78个非零ITR z。报告位于同ID artifacts/diagnostics/REPORT.md；CS0、Scene dirtyfalse/root14、Ledger471/43PASS。只新增诊断工具/Editor测试，production/schema/正式资源不变。父Q03仍IN_PROGRESS，下一继续held strength有效深度、两条OPoint materializer、canonical float及联合字段全reader；新增BDY/presence合同纳入Q05同一窗口。总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，Q02限定交付保持，不重做已关闭职责。

> Q03诊断Change：`NTSD28-Q03-COLLISION-GEOMETRY-WITNESS-001 / VERIFIED_CAPTURE_ONLY`，结果15同/27异已记录；production/schema/资源不变，父Q03合同未冻结。先读取该Change和同ID Task。

> **Q03字段审计基线（2026-09-13）：** 六DAT九frame、CPoint float/alias和27/9/24形状已测；完整consumer/联合字段仍待。详细进度见 `artifacts/diagnostics/NTSD28-NATIVE-DAT-AND-JOINT-FIELD-CONTRACT-AUDIT-001/Q03-PROGRESS-REPORT.md`，不得恢复已交付Q02或提前执行Q04/Q05。

> **当前执行游标（2026-09-13）：** 总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，BATCH-02继续。Q02加载基础已DELIVERED / VERIFIED_LOAD_INFRASTRUCTURE_ONLY；E3 `NTSD28-B11-SOURCE-CACHE-CALLER-PRODUCTION-001 / VERIFIED_SOURCE_CACHE_CALLER_LOAD_GATES_ONLY`：最终focused40/40、完整SelfCheck最终PASS、native direct/app/menu/menu重进4次Play（每次World4/三key一致/实际pool0/46资源零残留/两帧仍Stopped），默认旧内容App回归也PASS。CS0、NTSD_Battle dirtyfalse/root14、Ledger470/40PASS、保护3059中3047不变/12声明脚本变化/零缺失。首轮late-injection Play失败保留，BeforeSceneLoad测试clone解决注入顺序，原asset未改。下一唯一入口：docs/ai/TASKS/NTSD28-NATIVE-DAT-AND-JOINT-FIELD-CONTRACT-AUDIT-001.md / READY_CONTRACT（Q03六DAT九frame及CPoint27/OPoint24/+2F8/mass/reserved联合合同）；先只读权威消费链/字段矩阵，不提前升schema或部署资源。Q07正式DAT/图片迁移及B11/B12完整验收未完成；schema12/20/23、33ms、十一阶段、Unity/GAS与非战斗行为保持。

> **Q02 E2进行中（2026-09-13最新）：** `NTSD28-B11-SOURCE-ATOMIC-PUBLICATION-001 / FOCUSED_TEST_PASS`只覆盖候选输入绑定与verified decoder：Unity20/20（7+13）、CS0、Scene dirtyfalse、Ledger468/25PASS。新增LoganVisualContentCandidate绑定catalog/config与sheet/head/small hash；BMPLoader在实际decode bytes上验证SHA。E2整体仍IN_PROGRESS，下一继续同一Task/Change的实际staging/停止世代/prepared object-UI/无await提交/资源重绑退休；不是新子目标或完整交付。已确认旧commit提前退休及IsPrewarmCompleted早于UI，uGUI Image保留旧Sprite引用，细节在CANDIDATE-INPUT-REPORT.md；后续脚本前扩充准确Record。global/非战斗/资源未切换，schema12/20/23保持；Q03六DAT阻塞仍在，E3/Q07后继。总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE；以下旧READY/下一步以本条覆盖。

> Q02 E2 `NTSD28-B11-SOURCE-ATOMIC-PUBLICATION-001 / IN_PROGRESS / CANDIDATE_INPUT_BINDING`；同一Change先做定义+图片绑定和verified decode，随后继续原子提交/生命周期/UI资源绑定；首段通过不关闭E2。

> **Q02目录E1已验证（2026-09-13最新）：** `NTSD28-B11-LOGAN-CATALOG-CONFIG-CANDIDATE-001 / VERIFIED_CATALOG_AND_CONFIG_CANDIDATE_GATES_ONLY`；真实source-linked native330/330与Unity目录逐项一致，Unity39/39（本包15+路径24）、CS0、Scene dirtyfalse、Ledger467/23PASS。正式config仍由6个DAT Converter失败阻止，未返回partial，不能称新内容整体可用。DefinitionFingerprint仅catalog/DAT，PNG/head/small等完整身份留E2/E3。下一 `docs/ai/TASKS/NTSD28-B11-SOURCE-ATOMIC-PUBLICATION-001.md / READY_CONTRACT`；父CATALOG-PUBLICATION为E1已交付/E2原子发布/E3缓存caller未做。Q03字段合同可独立准备；Q02/BATCH-02/总目标ACTIVE，FULL_ALIGNMENT_INCOMPLETE。global/非战斗/正式资源未切换，schema12/20/23保持；以下旧下一步以本条覆盖。

> Q02目录事务E1 `NTSD28-B11-LOGAN-CATALOG-CONFIG-CANDIDATE-001 / IN_PROGRESS / TEST_FIRST`；先native catalog与全量候选，任何转换失败禁止partial返回；E2原子发布/E3缓存caller后继，global/菜单未切源。

> **Q02 PNG alpha已验证（2026-09-13最新）：** `NTSD28-B11-PNG-SHEET-ALPHA-CONTRACT-001 / VERIFIED_SOURCE_PNG_SHEET_AND_GPU_SAMPLES_ONLY`；Unity21/21，本包6+PNG13+Overlap2。隔离/正式nar实际sheet staging→atlas pixels→SpriteCatalog→现有shader共10个GPU样点通过（Direct3D11/URP/Gamma），CS0，Scene dirtyfalse，Ledger466/19PASS。旧UI/BMP/processor/shader保持，资源未迁移，schema12/20/23不变。下一 `docs/ai/TASKS/NTSD28-B11-CATALOG-PUBLICATION-CONTRACT-001.md / READY_CONTRACT`，从Q02-A已确认共享caller冻结目录/source/cache/publication事务；Q03字段合同也可准备。R17 raw/range/alpha子条件PARTIAL_RETURN；Q02/BATCH-02及总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。未验全局prewarm/正式迁移/全场Play和所有绘制路径，以下旧下一步由本条覆盖。

> Q02 PNG alpha子包 `NTSD28-B11-PNG-SHEET-ALPHA-CONTRACT-001 / IN_PROGRESS / TEST_FIRST` 已启动；准确范围为BMPLoader格式metadata、manager明确source sheet入口及测试；旧UI/BMP/全局source不变，最终验证待执行。

> **Q02-C已验证（2026-09-13最新）：** `NTSD28-B11-NATIVE-SPRITE-RANGE-CONTRACT-001 / VERIFIED_SOURCE_RANGE_ADMISSION_ONLY`；真实Unity14/14，405DAT/773sheet原始声明/有效范围/尺寸/路径与native capture一致，CS0，Scene dirtyfalse，Ledger465/18PASS。新增Logan配对parser/builder；旧入口和字段形状/schema12/20/23保持。3059保护中仅本包parser/manager及前包BMPLoader声明变化，零缺失。下一 `docs/ai/TASKS/NTSD28-B11-PNG-SHEET-ALPHA-CONTRACT-001.md / READY_CONTRACT`，再做catalog/cache/publication；Q03也可准备。Q02/BATCH-02与总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式资源未迁移、未验整场Play/GPU；R17 raw+range仅PARTIAL_RETURN。以下旧下一步以本条为准。

> Q02-C已启动：NTSD28-B11-NATIVE-SPRITE-RANGE-CONTRACT-001 / IN_PROGRESS / TEST_FIRST；新增明确Logan源parser/builder，AST保留declared、现有内容字段消费native effective。旧入口/非战斗/12-20-23版本保持；正式源/cache/publication未切换。

> **Q02-B已验证：** NTSD28-B11-PNG-WORKER-DECODE-001 / VERIFIED_RAW_WORKER_DECODE_ONLY；正式1255/1255尺寸/RGBA hash匹配、Unity13/13及最大17.5MP worker补测1/1，BMP/mainthread保持，CS0/Scene dirtyfalse。原3059文件仅BMPLoader声明变更，正式PNG未改。下一Q02-C `docs/ai/TASKS/NTSD28-B11-NATIVE-SPRITE-RANGE-CONTRACT-001.md`；另新增P-21/PNG-SHEET-ALPHA-CONTRACT-001，现有sheet去黑处理会丢PNG半透明，必须独立解决。R17 raw输入/解码PARTIAL_RETURN；Q02/range/alpha/catalog/cache/publication/迁移/GPU未闭合，总目标ACTIVE。

> Q02限定子包已验证：`NTSD28-B11-CONTENT-SOURCE-PATH-CONTRACT-001 / VERIFIED_PURE_PATH_ONLY`；源链接24/24与真实Unity24/24、CS错误0、3059既有文件hash不变、Scene dirtyfalse。Q02-A审计已交付：catalog.csv/registry_index才是对象目录权威；data.txt不能替代；25文件仍有条件性sprite range集合差异。BATCH-02/Q02仍IN_PROGRESS；下一`docs/ai/TASKS/NTSD28-B11-PNG-WORKER-DECODE-001.md / READY`（已测量1255PNG的palette1/2/4/8与RGBA8格式）。既有全局cache、菜单和加载caller未切源；PNG、range与catalog/cache/publication未闭合，不关闭Q02或总目标。

> **总目标活动中，第一批已交付（2026-09-13）：** `BATCH-01 / Q01 / NTSD28-B11-CONTENT-ENTRY-INVENTORY-001 / DELIVERED / VERIFIED_OFFLINE_AUDIT_ONLY`。正式330对象/24背景，405新版DAT与138旧DAT实际双端捕获，输入/源码/header身份通过；6文件9帧Converter拒绝及root/PNG/字段缺口已登记。报告：`artifacts/diagnostics/NTSD28-B11-CONTENT-ENTRY-INVENTORY-001/Q01-REPORT.md`。R15仅Q01身份子条件PARTIAL_RETURN；Q05/Q07条件保留。下一`BATCH-02 / Q02-A / docs/ai/TASKS/NTSD28-B11-SOURCE-ROOT-AND-CACHE-CONTRACT-AUDIT-001.md / READY`，从source/cache合同审计开始；Q03也具备准备前置。3059文件保护hash不变，production/非战斗/资源/Scene未修改，未运行Unity/Play。总目标仍ACTIVE/FULL_ALIGNMENT_INCOMPLETE；下方旧HOLD/准备态和旧下一包均为历史，不覆盖当前游标。


> **2026-09-13 Foot Marker 六帧动画代码完成、Unity验收待连接：** `BATTLE-CENTRAL-FOOT-MARKER-ANIMATION-001 / COMPILE_PASS / STATIC_ANIMATION_CONTRACT_PASS / UNITY_FOCUSED_PENDING / PLAY_PENDING`。用户重导出的frame_01～06均为128×48、Point/no-mip且bbox一致；GameConfig已按序绑定6×80ms，draw按unscaled presentation time换texture，全部Self同步并保持单Foot draw。runtime/editor build均0 error、静态合同PASS、validator 461/8 PASS；Unity Pipeline无实例，focused/Play未运行。用户PNG/meta未修改。


> **2026-09-13 Foot Marker 已改走 GameConfig：** `BATTLE-CENTRAL-FOOT-MARKER-GAMECONFIG-001 / COMPILE_PASS / STATIC_CONTRACT_PASS / UNITY_FOCUSED_PENDING / PLAY_PENDING`。生产脚本固定路径和 Scene 独立 Sprite 字段已移除；静态 fallback 最初绑定旧 FootSelf，后由 `BATTLE-CENTRAL-FOOT-MARKER-ANIMATION-001` 改绑 frame_01。runtime/editor 外部编译均0 error、静态合同PASS；focused/Play待连接。


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

> **CURRENT — USER HOLD / GLM PROGRESS AUDIT（2026-09-09）：** 用户要求暂停
> `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，由GLM先核验现有进度、生产脚本和证据，再整理遗漏与未处理项；
> 禁止从头重做已完成逻辑。不得自动恢复实现或验证。
> 单一转发提示词：`docs/ai/GLM-INCREMENTAL-CONTINUATION-PROMPT-2026-09-09.md`；
> 详细证据交接：`docs/ai/GLM-REALIGNMENT-HANDOFF-2026-09-09.md`。

> **CURRENT — B6 POSITIVE-LINK VALIDATION RETIRED（2026-09-09）：** `NTSD28-B6-POSITIVE-LINK-VALIDATION-RETIREMENT-PRODUCTION-001 / VERIFIED / RED_4_FAIL_2_PASS_OF_6 / FOCUSED_6_OF_6 / RELATED_33_OF_33 / B6_CATEGORY_103_OF_103 / NTSD28_229_OF_229 / STRESS_255_OF_256_1_UNRELATED_AI_REPORT / REAL_BATTLE_GRAB_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_R2_CPOINT_SYNC / CONSOLE_0_ERROR / SCENE_UNCHANGED / PHASE_33 / NO_POSITIVE_EVENT`。正式post-catch阶段由34压至33并直接进入stage/held，World/stress/U6/W07旧owner已退休；obsolete兼容入口保持0B no-op，lifecycle、negative held诊断与AI projection不变。下一先做B6 remaining/exit只读复核。

> **CURRENT — B6 HELD INJURY CAUGHTACT EVENT VERIFIED（2026-09-09）：** `NTSD28-B6-HELD-INJURY-CAUGHTACT-EVENT-PRODUCTION-001 / VERIFIED / RED_5_FAIL_10_PASS_OF_15 / FOCUSED_15_OF_15 / B6_CATEGORY_97_OF_97 / NTSD28_223_OF_223 / HITPLAN_185_OF_185 / PREINTERACTION_15_OF_15 / REAL_BATTLE_GRAB_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_POSITIVE_LINK / CONSOLE_0_ERROR / SCENE_UNCHANGED / LEDGER_PASS_428_364 / POST_SETTLEMENT_EVENT_EXACT`。transient event只记录真实正injury，并在完整settlement后复用B5 producer；真实Play count0→1且不重复。下一strict首差为positive-link validation retirement；MP/world KO/schema/content后置。

> **B6 HELD INJURY CAUGHTACT EVENT 启动记录（已由上条VERIFIED关闭）：** 本包曾以`IN_PROGRESS / TEST_FIRST / PRODUCTION_UNCHANGED`启动；恢复时以上条最终证据为准。

> **CURRENT — B6 HELD INJURY ACCOUNTING/COVER VERIFIED（2026-09-09）：** `NTSD28-B6-HELD-INJURY-ACCOUNTING-COVER-PRODUCTION-001 / VERIFIED / RED_14_FAIL_3_PASS_OF_17 / FOCUSED_17_OF_17 / B6_CATEGORY_82_OF_82 / NTSD28_208_OF_208 / HITPLAN_185_OF_185 / PREINTERACTION_15_OF_15 / RELATED_FIXTURES_9_OF_9 / REAL_BATTLE_GRAB_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_POSITIVE_LINK / CONSOLE_0_ERROR / SCENE_UNCHANGED / LEDGER_PASS_427_363 / CANONICAL_ACCOUNTING_COVER_EXACT`。actual canonical damage/score/KO与cover timer已闭合，真实Battle grab通过；下一caughtact event，MP/world KO feed后置。

> **CURRENT — B6 SETTLEMENT VACTION PREFLIGHT VERIFIED（2026-09-09）：** `NTSD28-B6-CATCH-SETTLEMENT-VACTION-PREFLIGHT-PRODUCTION-001 / VERIFIED / RED_4_FAIL_4_PASS_OF_8 / FOCUSED_8_OF_8 / B6_CATEGORY_65_OF_65 / NTSD28_191_OF_191 / HITPLAN_185_OF_185 / PREINTERACTION_15_OF_15 / TARGETED_PLAY_8_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_UNCHANGED / LEDGER_PASS_426_362 / ACTUAL_PREFLIGHT_EXACT`。signed/zero提交和post-action frame/kind2 fence已闭合；SelfCheck首差回到held injury accounting，下一严格包处理它。

> **CURRENT — B6 MIXED CATCH ADVANCE/EXACT CONSUMER VERIFIED（2026-09-09）：** `NTSD28-B6-CATCH-EXACT-CONSUMER-AND-ADVANCE-ORDER-PRODUCTION-001 / VERIFIED / RED_1_PASS_7_FAIL_OF_8 / FOCUSED_16_OF_16 / PREINTERACTION_15_OF_15 / B6_CATEGORY_57_OF_57 / HITPLAN_185_OF_185 / NTSD28_183_OF_183 / TARGETED_PLAY_16_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_UNCHANGED / LEDGER_PASS_425_361 / SINGLE_MIXED_ADVANCE_EXACT_CONSUMERS`。single mixed advance、三个plain exact consumer及两个terminal fence已闭合；下一严格包为settlement vaction preflight，之后才是held accounting。

> **CURRENT — B6 CATCH RELATION EXACT-FIELD PRODUCTION VERIFIED（2026-09-09）：** `NTSD28-B6-CATCH-RELATION-EXACT-FIELDS-PRODUCTION-001 / VERIFIED / RED_0_OF_3 / FOCUSED_19_OF_19 / B6_CATEGORY_41_OF_41 / HITPLAN_185_OF_185 / NTSD28_167_OF_167 / TARGETED_PLAY_19_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_UNCHANGED / EXACT_RELATION_ATOMIC`。actual/shadow、current criminal、missing/signed/respond、kind1与0B均绿；下一严格包为mixed catch advance/control-flow fences，再到vaction/accounting。

> **CURRENT — B6 INVALID NEGATIVE-HELD RECIPROCAL PRESERVE VERIFIED（2026-09-09）：** `NTSD28-B6-HELD-INVALID-RECIPROCAL-PRESERVE-PRODUCTION-001 / VERIFIED / RED_2_OF_2 / FOCUSED_7_OF_7 / B6_CATEGORY_22_OF_22 / RELATED_47_OF_47 / BROAD_56_OF_62_6_UNRELATED_NATIVE_INPUT_PROXY / TARGETED_PLAY_7_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_BASELINE_RESTORED / DIAGNOSTIC_PRESERVE`。invalid negative child已counter/trace+preserve，lifecycle cleanup阻断正常ABA。下一严格包为`NTSD28-B6-CATCH-RELATION-EXACT-FIELDS-PRODUCTION-001`，再到mixed advance/vaction/accounting。

> **CURRENT — B6 ENTITY-LINK LIFECYCLE CLEANUP VERIFIED（2026-09-09）：** `NTSD28-B6-ENTITY-LINK-LIFECYCLE-CLEANUP-PRODUCTION-001 / VERIFIED / FOCUSED_7_OF_7 / B6_CATEGORY_15_OF_15 / RELATED_91_OF_91 / TARGETED_PLAY_7_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_BASELINE_RESTORED / ATOMIC_RELEASE_CLEANUP / RED_NOT_EXECUTED`。single writer已在release-success与generation release/reuse之间清held/catch exact+compat并阻断same-slot ABA；P7旧夹具已纠正。下一严格包为`NTSD28-B6-HELD-INVALID-RECIPROCAL-PRESERVE-PRODUCTION-001`，catch/positive validation仍后置。

> **CURRENT — B6 CPOINT THROW EXACT SUBSET VERIFIED（2026-09-09）：** `NTSD28-B6-CPOINT-THROW-ENVIRONMENT-VZ-PRODUCTION-001 / VERIFIED / FOCUSED_8_OF_8 / RELATED_17_OF_17 / TARGETED_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_CURRENT_BASELINE_UNCHANGED / FULL_RESOURCE_DEFERRED / ILLEGAL_CATEGORY_RETIRED`。许可恢复后8/8、17/17与真实抓取Play通过；SelfCheck已越过throw并停在held injury accounting。下一严格route为该held accounting owner/production治理链；不恢复旧Combo stats或提前连接完整MP resource。

> **CURRENT — B5 NEGATIVE ENVIRONMENT SHARED RECOVERY VERIFIED（2026-09-09）：** `NTSD28-B5-NEGATIVE-ENVIRONMENT-RECOVERY-PRODUCTION-001 / VERIFIED / RED_11_OF_12 / FOCUSED_12_OF_12 / RELATED_154_OF_154 / RELATED_B5_981_OF_981 / TARGETED_PLAY_12_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / SINGLE_EXACT_TRANSACTION / FLUTE_FALSE_POSITIVE_RETIRED`。两路production与derived已共享EnvironmentState/native phase/rule/900-scale/two-hop exact transaction，overkill post-clamp与flute control通过；其当时的CPoint throw Vz阻塞已由后继B6包关闭，fresh SelfCheck推进到held injury accounting。

> **CURRENT — NEGATIVE ENVIRONMENT CLAMP CORRECTION VERIFIED（2026-09-09）：** `NTSD28-B5-NEGATIVE-ENVIRONMENT-CLAMP-CORRECTION-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / SOURCE_HASH_MATCH / CLAMP_TO_ZERO_REQUIRED / PRIOR_NO_CLAMP_CLAUSE_SUPERSEDED`。source SHA EB37...匹配既有manifest；exact counters后HP/HPBound clamp0，旧audit仅no-clamp一句作废。下一single consumer按纠正后合同实施。

> **CURRENT — B5 NEGATIVE ENVIRONMENT RULE CARRIER VERIFIED（2026-09-09）：** `NTSD28-B5-NEGATIVE-ENVIRONMENT-RULE-CARRIER-001 / VERIFIED / RED_13 / FOCUSED_6_OF_6 / RELATED_106_OF_106 / NTSD28_1394_OF_1394 / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_BASELINE_UNCERTIFIED / CARRIER_READY / RECOVERY_CONSUMER_NEXT`。default9/raw restore、world snapshot/checksum/parity、schema `11/20/23`与0B已闭合；RED13、focused6、related106、exact NTSD28 broad1394、builds/Ledger绿。full SelfCheck仍为既有CPoint阻塞，Scene基线未认证。下一独立single recovery transaction。

> **CURRENT — B5 NEGATIVE ENVIRONMENT RECOVERY OWNER AUDIT VERIFIED（2026-09-09）：** `NTSD28-B5-NEGATIVE-ENVIRONMENT-RECOVERY-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / WEAPONCOUNT_WRONG_CARRIER / EXACT_TRANSACTION_REQUIRED / TWO_PACKAGE_ROUTE`。先补NativeHitResourceRules +0x90/default9 carrier，再用single transaction替换两路WeaponCount旧分支；B6 producer/B8 event/schema不混入。

> **CURRENT — B5 INPUT HP-COST SHARED TRANSACTION VERIFIED（2026-09-09）：** `NTSD28-B5-INPUT-HP-COST-COMPAT-SHARED-TRANSACTION-PRODUCTION-001 / VERIFIED / RED_0_OF_6 / FOCUSED_6_OF_6 / RELATED_INPUT_187_OF_187 / RELATED_B5_926_OF_926 / TARGETED_PLAY_6_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / TWO_INPUT_WRITERS_RETIRED / NEGATIVE_RECOVERY_AUDIT_NEXT`。single exact core与三路入口闭合；下一审计negative recovery source/owner，CPoint/schema继续后置。

> **CURRENT — B5 INPUT HP-COST COMPAT AUDIT VERIFIED（2026-09-09）：** `NTSD28-B5-INPUT-HP-COST-COMPAT-STAT-RETIREMENT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / COMPAT_PATH_LIVE / PARTIAL_RETIREMENT_UNSAFE / SHARED_TRANSACTION_PRODUCTION_DEFINED`。Legacy compat仍可达且旧cost算法不完整；下一包必须共享现有exact generic action transaction，再退休两个ComboVic writer，negative recovery/B6 held/schema保持排除。

> **CURRENT — B5 TYPE3/WEAPON/FLUTE LEGACY STATS RETIREMENT VERIFIED（2026-09-09）：** `NTSD28-B5-TYPE3-WEAPON-FLUTE-LEGACY-STAT-WRITER-RETIREMENT-001 / VERIFIED / RED_0_OF_4 / FOCUSED_4_OF_4 / RELATED_B5_777_OF_777 / TARGETED_PLAY_4_CASES / LIVE_COLLISION_MATRIX_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / SCOPED_LEGACY_STATS_RETIRED / INPUT_COMPAT_AUDIT_NEXT`。三族Authority-extra stats已从actual/HitPlan退休；exact累计与其余writer不变。下一只读input HP cost compat审计。

> **CURRENT — B5 STANDARD/REDUCED EXACT KO VERIFIED（2026-09-09）：** `NTSD28-B5-STANDARD-REDUCED-KNOCKOUT-PRODUCER-001 / VERIFIED / RED_1_OF_4 / FOCUSED_4_OF_4 / RELATED_270_OF_270 / TARGETED_PLAY_4_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / EXACT_KO_PRODUCER_READY / TYPE3_WEAPON_FLUTE_STATS_NEXT`。same standard credit在HP前exact KO+1，HitPlan observable闭合；下一退type3/weapon/flute legacy镜像。

> **CURRENT — B5 LEGACY STATS RETIREMENT READINESS VERIFIED（2026-09-09）：** `NTSD28-B5-LEGACY-DAMAGE-STAT-WRITER-RETIREMENT-READINESS-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / GLOBAL_RETIREMENT_BLOCKED / FOUR_PREREQUISITE_ROUTES`。全量retirement不能直接执行：先补standard/reduced exact KO；type3/weapon/flute单独退；input/recovery与B6 held各自闭合。下一standard/reduced KO production。

> **CURRENT — B5 ORDINARY CREDIT GATE 2F4 VERIFIED（2026-09-09）：** `NTSD28-B5-ORDINARY-CREDIT-GATE-2F4-PRODUCER-CONSUMER-CORRECTION-001 / VERIFIED / RED_0_OF_5 / FOCUSED_5_OF_5 / RELATED_287_OF_287 / TARGETED_PLAY_5_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / EXACT_2F4_READERS_PRODUCERS / LEGACY_STATS_WRITER_NEXT`。damage/CPoint/HitPlan、legacy/ECS recovery与两个OPoint factory已改绑exact +0x2F4；Play/Console/Scene绿，full SelfCheck独立CPoint阻塞。下一route6 legacy damage-stat writer retirement。

> **CURRENT — B5 TYPE3 HOLDERCOPY WRITER RETIREMENT VERIFIED（2026-09-09）：** `NTSD28-B5-TYPE3-LEGACY-HOLDERCOPY-WRITER-RETIREMENT-001 / VERIFIED / RED_0_OF_3 / FOCUSED_4_OF_4 / RELATED_284_OF_284 / TARGETED_PLAY_3_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / FOUR_TYPE3_WRITERS_RETIRED / TYPE3_SPECIFIC_FAMILY_EXIT_READY / ORDINARY_CREDIT_GATE_2F4_NEXT`。actual1+HitPlan3处Authority不存在的write已退休；target99/source77、group/owner/control/action/motion、Console/Scene均保持。full SelfCheck仍为既有CPoint阻塞；下一严格route为B5 `OrdinaryCreditGate2F4` producer/consumer correction。

> **CURRENT — B5 KIND5 LINKED-PARENT VERIFIED / TYPE3 NEXT（2026-09-09）：** `NTSD28-B5-KIND5-LINKED-PARENT-SLOT-CORRECTION-001 / VERIFIED / RED_0_OF_5 / FOCUSED_5_OF_5 / RELATED_280_OF_280 / TARGETED_PLAY_5_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / EXACT_LINKED_PARENT_BOUND / HOLDERCOPY_UNCHANGED / TYPE3_WRITER_NEXT`。两个consumer helper现精确绑定HolderStableId/implicit-zero；related/Play/build/Scene绿，SelfCheck独立CPoint阻塞。下一type3 extra writer。

> **CURRENT — B4 REVIVAL EXIT READY / NEXT B5（2026-09-09）：** `NTSD28-B4-REVIVAL-EXIT-AUDIT-001 / VERIFIED / REVIVAL_TRACE_EQUAL_13_RECORDS_416_FIELDS / DOUBLE_RUN_BYTE_STABLE / RELATED_54_OF_54 / TARGETED_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / B4_REVIVAL_EXIT_READY / PRODUCTION_UNCHANGED / B7_H_PRODUCER_PENDING`。Authority/Unity 13/416 first difference空且双跑稳定；Play/Console/Scene绿。full SelfCheck独立CPoint阻塞；下一严格route为B5 HolderCopy binding/extra-write corrections，B7/H producer/schema不被本包提前关闭。

> **CURRENT — B4 NORMAL FLOOR/RNG VERIFIED / EXIT NEXT（2026-09-09）：** `NTSD28-B4-REVIVAL-NORMAL-FLOOR-RNG-PRODUCTION-001 / VERIFIED / RED_0_OF_7 / FOCUSED_7_OF_7 / RELATED_59_OF_59 / TARGETED_PLAY_7_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / EXIT_AUDIT_NEXT`。floor/peer/sumX/sync RNG/precise position/vitals已闭合；下一B4 joint exit trace。

> **CURRENT — B4 QUEUED CONTINUATION VERIFIED / NORMAL NEXT（2026-09-09）：** `NTSD28-B4-REVIVAL-QUEUED-CONTINUATION-PRODUCTION-001 / VERIFIED / RED_4_OF_11 / FOCUSED_13_OF_13 / RELATED_47_OF_47 / TARGETED_PLAY_13_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / NORMAL_ROUTE_NEXT / PRODUCER_B7_H_PENDING / SCHEMA_DEFERRED`。controller/defer/group/visual/queued/action/counter/hold已闭合；下一normal floor/RNG，B7/H producer和schema后置。

> **CURRENT — B4 REVIVAL PARTICIPANT GATE/BRANCH VERIFIED（2026-09-09）：** `NTSD28-B4-REVIVAL-PARTICIPANT-GATE-KILLCOUNT-CORRECTION-001 / VERIFIED / RED_10_OF_16 / FOCUSED_16_OF_16 / RELATED_34_OF_34 / TARGETED_PLAY_20_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / QUEUED_ROUTE_NEXT`。只退休两个legacy HitStun arm与C07 KillCount/team gate并恢复lives-first四结果；下一queued controller/group/visual，normal floor/RNG与exit后置。

> **CURRENT — B0 DIRECT REVIVAL DEFAULTS VERIFIED / B4 RESUMED（2026-09-09）：** `NTSD28-B0-DIRECT-REVIVAL-DEFAULTS-PRODUCTION-001 / VERIFIED / RED_1_OF_3 / DIRECT_DEFAULTS_1_0_0 / FOCUSED_3_OF_3 / DIRECT_OWNER_REGRESSION_15_OF_15 / TARGETED_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / B4_RUNTIME_RESUMABLE`。slot0/19 active snapshot=1/0/0且raw backing=0/0/0，invalid不写；Play/Console/Scene绿。full SelfCheck独立CPoint阻塞；恢复B4 gate correction。

> **B4 REVIVAL OWNER AUDIT VERIFIED（2026-09-09）：** `NTSD28-B4-REVIVAL-PARTICIPANT-GATE-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / KILLCOUNT_NOT_REVIVAL_AUTHORITY / THREE_ENTRY_POINTS_SPLIT / DIRECT_DEFAULT_PRODUCER_MISSING / FOUR_RUNTIME_ROUTES_DEFINED / PRODUCTION_HELD`。Authority C25/C07无KillCount；Unity三旧gate、branch/primary free及queued/floor/RNG差异已拆分，Direction-B state14=235。先补B0 direct默认producer。

> **CURRENT — B3 1100..1299 CHILD PROPAGATION RETIREMENT VERIFIED（2026-09-09）：** `NTSD28-B3-LEGACY-1100-1299-CHILD-PROPAGATION-RETIREMENT-001 / VERIFIED / RED_0_OF_4 / PRODUCTION_CHILD_SCAN_REMOVED / FOCUSED_4_OF_4 / TARGETED_PLAY_PASS / BUILDS_0_ERROR / SELF_RESET_PRESERVED / CURRENT_ITACHI_1250_COVERED / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED`。RED matched child为0/-99/-149/-198而normal为40；只删world/KillCount child write后focused4/4、真实Play、builds、Console/Scene通过，self reset与Itachi1250保留。full SelfCheck仍由独立CPoint阻塞；下一B4 revival gate。

> **CURRENT — B3 LEGACY STATE501 PRODUCTION RETIREMENT VERIFIED（2026-09-09）：** `NTSD28-B3-LEGACY-STATE501-TRANSFORM-RETIREMENT-001 / VERIFIED / RED_0_OF_1 / PRODUCTION_BRANCH_REMOVED / FOCUSED_1_OF_1 / EARLY_M2_11_OF_11 / TARGETED_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED`。RED为child ObjectId 31→9000；删除fast/fallback/legacy 501分支后两组10实体全部canonical runtime/definition/identity/frame不变，M2 11/11、targeted Play、Console0和Scene hash/dirty不变。full SelfCheck仍由独立CPoint阻塞；严格下一route为B3 1100..1299 child propagation retirement。

> **B3 LEGACY STATE501 AUDIT VERIFIED（2026-09-09）：** `NTSD28-B3-LEGACY-STATE501-OWNED-CHILD-TRANSFORM-RETIREMENT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / NO_AUTHORITY_STATE501_TRANSFORM / DIRECTION_B_GAMEPLAY_ZERO / RELEASE_GAMEPLAY_ZERO / HUD_RADAR_ONLY_TWO_TOKENS / UNITY_SYNTHETIC_BRANCH_CONFIRMED / PRODUCTION_RETIREMENT_DEFINED`。Authority C25仅8000..8999且不扫描owner；Unity early-frame独有synthetic 501 mutation。本审计无code/content/Scene/Authority改动。

> **CURRENT — B2 AI OWNER GUARD FAMILY VERIFIED（2026-09-09）：** `NTSD28-B2-AI-OWNER-SLOT-KILLCOUNT-CORRECTION-001 / VERIFIED / OWNER_GUARD_TRACE_EQUAL_8_RECORDS_48_FIELDS / UNITY_FOCUSED_4_OF_4 / TARGETED_PLAY_PASS / DOUBLE_RUN_BYTE_STABLE / BUILDS_0_ERROR / EXACT_OWNER_SLOT_BINDING / LEGACY_KILLCOUNT_DETACHED_FROM_AI / SELFCHECK_BLOCKED_UNRELATED`。Authority source-model/Unity production snapshot专项trace为8 records/48 fields、first difference空且各自双跑稳定；direct/OPoint/F8 owner 0/7/99均被实际消费。01:37:20真实NTSD_Battle Play通过、Console0、Scene dirty=false/root13/SHA不变；01:39:06 focused 4/4。full SelfCheck仍在独立CPoint Vz断言停止；下一严格route是B3 state501 legacy child transform retirement audit。

> **CURRENT — B0 OWNER-SLOT PRODUCER EXIT VERIFIED / B2 RUNTIME RESUMED（2026-09-09）：** `NTSD28-B0-OWNER-SLOT-PRODUCTION-EXIT-AUDIT-001 / VERIFIED / OWNER_TRACE_EQUAL_15_RECORDS_135_FIELDS / DOUBLE_RUN_BYTE_STABLE / TARGETED_PLAY_PASS / ROUTES_1_TO_4_REGRESSION_32_OF_32 / BUILDS_0_ERROR / SELFCHECK_BLOCKED_BY_UNRELATED_CPOINT / B0_OWNER_PRODUCER_EXIT_READY / B2_RUNTIME_RESUMABLE / PRODUCTION_UNCHANGED`。Authority与Unity双端trace覆盖self、owner/target分离、two-hop OPoint、F8 99、state9996 -1、type3 mutation和slot reuse，comparison first difference空；双端各自双跑稳定。目标Play于01:06:52通过、Console0、Scene不变。full SelfCheck仍被更早既有CPoint阻塞；本结论不含full parity/B8 physical F8。下一恢复`NTSD28-B2-AI-OWNER-SLOT-KILLCOUNT-CORRECTION-001` runtime验收。

> **CURRENT — B0 OWNER-SLOT ROUTE 5 EXIT AUDIT IN PROGRESS（2026-09-09）：** `NTSD28-B0-OWNER-SLOT-PRODUCTION-EXIT-AUDIT-001 / IN_PROGRESS / B0_OWNER_PRODUCER_ROUTE_5 / OWNER_ONLY_JOINT_TRACE_DESIGN / PRODUCTION_UNCHANGED`。通用两角色raw capture不能覆盖dynamic owner生命周期；新专项trace只比较+0x354/OwnerSlot，计划覆盖direct self、two-hop OPoint、F8 99、state9996 -1、type3 mutation与slot reuse。F8 content/count/position、full parity和B8 physical consumer排除；诊断runner/focused test与验证待做。

> **CURRENT — B0 ORDINARY OPOINT OWNER ROUTE 4 FOCUSED PASS（2026-09-09）：** `NTSD28-B0-OPOINT-OWNER-PROPAGATION-PRODUCTION-001 / FOCUSED_TEST_PASS / UNITY_RUNTIME_COMPILE_PASS / OUT_OF_PROCESS_COMPILE_PASS / UNITY_FOCUSED_7_OF_7 / SELFCHECK_BLOCKED_BEFORE_PRESENTATION_ASSERT_BY_UNRELATED_CPOINT / RUNTIME_PENDING / B0_OWNER_PRODUCER_ROUTE_4 / LOGIC_AND_PRESENTATION_PRODUCERS_WRITTEN / FRAGMENT_AND_LEGACY_BRANCHES_EXCLUDED`。logic/presentation两个world-owned producer各只写parent literal owner；精确RED=`2/7`、builds 0 error、Unity 00:09:55 GREEN=`7/7`。00:11:22 SelfCheck在更早CPoint停止，未到presentation新增断言；Play/joint待验。下一route5 exit audit。

> **CURRENT — B0 F8 OWNER99 ROUTE 3 FOCUSED PASS（2026-09-08）：** `NTSD28-B0-F8-OWNER99-PRODUCTION-001 / FOCUSED_TEST_PASS / UNITY_RUNTIME_COMPILE_PASS / OUT_OF_PROCESS_COMPILE_PASS / UNITY_FOCUSED_3_OF_3 / SELFCHECK_REACHED_UNRELATED_CPOINT_AFTER_NEW_CHECK / RUNTIME_PENDING / B0_OWNER_PRODUCER_ROUTE_3 / MODE2_MATERIALIZER_OWNER_ONLY / PHYSICAL_F8_EFFECT_WIRING_EXCLUDED`。精确RED=`1/3`后只在mode2 factory前写task owner=`99`；builds均0 error，Unity 23:25:56 GREEN=`3/3`覆盖slot50/399、raw backing `-1`、frame/位置/四次RNG及normal owner`-1`/六次RNG。23:27:20 SelfCheck通过本包检查后停在较后的既有CPoint throw-Vz；物理F8/Play/joint待验。下一route 4 ordinary OPoint owner propagation。

> **CURRENT — B0 ROUTE 4 OPOINT/FRAGMENT BOUNDARY CORRECTED（2026-09-08）：** Authority ordinary OPoint传播parent literal owner；native hit_Fa5/6已分离owner/target。built-in OID999 weapon fragments与state9996保持owner -1，只有DAT `<weapon_piece>` fragment继承source owner。Unity正式late OPoint producer位于`BattleLogicObjectPointRuntime.ProcessOneLateOpoint()`且缺task owner；built-in/state9996现状正确，DAT weapon_piece无parser/materializer仅有pass skeleton，归B7；8/9/13保持独立。route 4只补ordinary single/multi producer并验two-hop/first claimed active-slot runtime/raw-trace projection，独立raw backing保持`-1`，不做factory全局parent推断。route 2 focused已通过，route 3先行；本轮无code/content/Scene。

> **CURRENT — B0 F8 OWNER99 ROUTE 3 READ-ONLY BOUNDARY CLOSED（2026-09-08）：** Authority F8在`GameSession28::step()`的post-tick tail消费pending，`NativeFunctionKeyDropSpawn28.owner_slot=99`经request原样进入`spawn_at`。Unity正式F8的`FunctionKeys.PendingObjectCommand`尚无生产consumer；既有`Mode2Request==1 -> SpawnMode2RandomWeapons()`来自legacy diagnostic latch且task owner仍为-1。route 2 focused gate现已满足；route 3只修该materializer的owner=99并覆盖slot50/high first claimed active-slot runtime/raw-trace projection；独立raw backing保持`-1`，保留`requiredRuntimeSlot=-1` lowest-free factory分配且不做post-register fix-up。physical F8 effect wiring仍归B8；`RunNormalDrop`继续默认owner -1，用户保留的candidate/RNG/position不动。本轮无code/content/Scene。

> **CURRENT — B0 DIRECT/STAGE SELF OWNER FOCUSED PASS（2026-09-08）：** `NTSD28-B0-DIRECT-ENTITY-SELF-OWNER-PRODUCTION-001 / FOCUSED_TEST_PASS / UNITY_RUNTIME_COMPILE_PASS / OUT_OF_PROCESS_COMPILE_PASS / UNITY_FOCUSED_15_OF_15 / SELFCHECK_BLOCKED_BY_UNRELATED_CPOINT / RUNTIME_PENDING / B0_OWNER_PRODUCER_ROUTE_2 / DIRECT_AND_STAGE_SELF_OWNER_ONLY`。direct App/bootstrap在ModuleBind前声明required/self slot，adapter按Authority只接受physical slot `0..19`，actual slot不匹配时统一reset/recycle并跳过roster；stage task携带owner=required，各entity initializer在首次注册前消费explicit owner，stage/results-reserve不再尾部覆盖-1。runtime `0 error / 47 warnings`、Editor进程外compile `0 error / 104 warnings`；重启后的Unity于22:51:33刷新程序集。首轮7/15暴露测试错误的raw backing假设，纠正为claimed entity runtime并保护raw backing `-1`后，22:51:46实际`15/15`通过。22:52:37 full SelfCheck在更早的既有CPoint throw-Vz断言停止；Play/joint trace仍待验。F8、ordinary OPoint、state9996、8/9/13与+0x2F8独立。

> **CURRENT — B0 OBJECT-AI TARGET +0X3F8 FOCUSED 7/7 PASS（2026-09-08）：** `NTSD28-B0-OBJECT-AI-TARGET-3F8-DECONFLICTION-001 / FOCUSED_TEST_PASS / UNITY_RUNTIME_COMPILE_PASS / 7_OF_7_PASS / SELF_CHECK_BLOCKED_BY_UNRELATED_CPOINT / PLAY_PENDING / JOINT_TRACE_PENDING / RUNTIME_PENDING / B0_OWNER_PRODUCER_PREREQUISITE / EXISTING_STORAGE_REUSED / NO_SCHEMA_CHANGE`。复用`PickerStableId`底层int；Authority闭合的common/4/7/11、5/6 child与specialized 4/12已完成owner/target分离及stale/gate修正。8/9/13、+0x2F8、schema与raw inactive adapter保持独立。Unity compile无CS错误，v2 Unity EditMode focused于20:48实际通过7/7；完整SelfCheck被更早既有CPoint throw-Vz阻塞，Play/joint trace仍待。已满足下一direct/stage self-owner包的前置。

> **CURRENT — B0 OWNER-SLOT PRODUCTION OWNER VERIFIED / B2 RUNTIME BLOCKED（2026-09-08）：** `NTSD28-B0-OWNER-SLOT-PRODUCTION-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / FIELD_BINDING_RETAINED / FORMAL_SELF_OWNER_MISSING / OPOINT_OWNER_PROPAGATION_MISSING / F8_OWNER99_MISSING / TARGET_MULTIPLEXING_CONFLICT / FIVE_ROUTES_DEFINED / B2_RUNTIME_BLOCKED / PRODUCTION_HELD`。Unity缺direct/stage self、OPoint parent-owner和F8 99 producers，且OwnerSlot仍被generic hit_Fa当+3F8 target。先deconflict再补三类producer；B2消费代码保留但不能宣称行为闭合。本轮audit无code。

> **CURRENT — B2 AI OWNER-SLOT CORRECTION RUNTIME PENDING（2026-09-08）：** `NTSD28-B2-AI-OWNER-SLOT-KILLCOUNT-CORRECTION-001 / RUNTIME_PENDING / EXACT_OWNER_SLOT_BINDING / LEGACY_KILLCOUNT_DETACHED_FROM_AI / ISOLATED_RUNTIME_AND_EDITOR_COMPILE_PASS / FOCUSED_PURE_PASS / SELFCHECK_BLOCKED_UNRELATED`。OID122/123 AI guard、SoA/unified/legacy row已改读OwnerSlotIndex，publisher不再投影KillCount；两套build 0 error，owner sentinel harness PASS。SelfCheck在无关CPoint throw Vz断言失败；Unity NUnit/Play/joint trace待验。

> **CURRENT — B5 LEGACY DAMAGE/STATS OWNER VERIFIED / B2-B4 BINDINGS REOPENED（2026-09-08）：** `NTSD28-B5-LEGACY-DAMAGE-STATS-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / NO_SINGLE_LEGACY_STATS_AUTHORITY / KILLCOUNT_MULTIPLEXED / EARLIER_PHASE_BINDINGS_REOPENED / EXACT_NATIVE_CARRIERS_EXIST / SEVEN_ROUTES_DEFINED / JOINT_SCHEMA_DIRECTION_GATED / PRODUCTION_HELD`。`KillCount`误承载+2F4、AI owner、revival和child lifecycle；Combo/Kill/world arrays与exact accumulators并行。577行/生产207行29文件已穷尽。严格顺序恢复到B2 AI→B3 child→B4 revival→B5 +2F4/stats；联合schema新增roster1→2。本轮无code/content/Scene。

> **CURRENT — B6 LEGACY HOLDERCOPY OWNER VERIFIED / B5 BINDING CORRECTIONS REQUIRED（2026-09-08）：** `NTSD28-B6-LEGACY-HOLDERCOPY-MULTIPLEXED-SLOT-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / NO_SINGLE_AUTHORITY_FIELD / MULTIPLEXED_OWNER_CONFIRMED / B5_EXIT_CORRECTIONS_REQUIRED / CURRENT_TWO_HOP_GRAPH_WITNESS / ROUTES_SPLIT / SCHEMA_DIRECTION_GATED / PRODUCTION_HELD`。HolderCopy合并了direct holder/root/legacy stats/type3/stage slot，Authority无此field。Unity linked-holder frozen pair/BruteForce/HitPlan误读HolderCopy，type3仍extra-write。current117 pickup/62 OPoint-kind2，另71 rows→14 type0 OIDs/30 pairs的two-hop graph可使root!=immediate holder。旧stats audit已闭合并回溯B2-B4；先修早期binding及B5 linked-parent/type3，再退B6/B7 producer/schema。本轮无code/content/Scene。

> **CURRENT — B6 LEGACY GRABBEDBY MIRROR VERIFIED / BEHAVIOR AND SCHEMA HELD（2026-09-08）：** `NTSD28-B6-LEGACY-GRABBEDBY-RELATION-MIRROR-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / NO_AUTHORITY_SECOND_RELATION_FIELD / CURRENT_OPOINT_WRITER_REACHABLE / LEGACY_READER_ROUTED / TWO_NEW_PACKAGES_PLUS_EXISTING_CONSUMER / SCHEMA_DIRECTION_GATED / PRODUCTION_HELD`。Authority只认LinkState+parent/child slots。Unity在current62 OPoint-kind2/39 Character sources写child -1，shared117 ITR-kind2却不写；字段进runtime snapshot/ECS fingerprint而不进checksum/parity。复用tracker raw-kind5 consumer退休→退nonzero writers→联合schema；runtime/用户方向未清，无code/content/Scene。

> **CURRENT — B6 LEGACY TRACKER RELATION OWNER VERIFIED / THREE PACKAGES HELD（2026-09-08）：** `NTSD28-B6-LEGACY-TRACKER-RELATION-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / NO_AUTHORITY_TRACKER_LAYER / CURRENT_OPOINT_WRITERS_REACHABLE / LEGACY_READERS_DEFAULT_BYPASSED / THREE_PACKAGE_SPLIT_DEFINED / SCHEMA_DIRECTION_GATED / PRODUCTION_HELD`。Authority只用reciprocal link；Unity factory为current62 OPoint-kind2/56 edges额外写TrackerFlag和managed parent。shared kind5已按link转换，旧raw readers默认绕过，current target-kind5交集0。producer→consumer/cache→联合schema；lifecycle runtime与用户schema方向待清，无code/content/Scene。

> **CURRENT — B6 OBJECT-AI TARGET +0X3F8 OWNER VERIFIED / THREE PACKAGES HELD（2026-09-08）：** `NTSD28-B6-OBJECT-AI-TARGET-3F8-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / EXISTING_CARRIER_RECLASSIFIED / OWNER_354_CORRUPTION_CURRENT_REACHABLE / THREE_PACKAGE_SPLIT_DEFINED / PRODUCTION_HELD`。Authority +0x354 owner、+0x2F8 excluded source、+0x3F8 target独立。Unity OID124正向使用`PickerStableId` storage，但generic hit_Fa用OwnerSlot读写target。current206 common frames、OID219 5→4、9条3→7链会污染归属。后继carrier语义→common/child producer→+0x2F8 consumer；runtime未清，无code/content/Scene。

> **CURRENT — B6 LEGACY WEAPONSTATE OWNER VERIFIED / BEHAVIOR HELD / SCHEMA DIRECTION REQUIRED（2026-09-08）：** `NTSD28-B6-LEGACY-WEAPON-STATE-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / NO_AUTHORITY_PARALLEL_STATE / BEHAVIOR_RETIREMENT_DEFINED / CURRENT_OID124_WITNESS / CARRIER_SCHEMA_DIRECTION_GATED / PRODUCTION_HELD`。Authority始终读取actual frame state；Unity唯一reader却驱动额外1002→2000→3000与Vx halving。OID124 action40..55的16帧state1002/hit_Fa12、Tenten/Criminal2 direct spawn与kind2+Naruto clone DVX均可达：tick1 checksum、tick2 motion首差。先退休behavior/dynamic writers并暂留reserved0；carrier删除与ReleaseTick联合schema13/20/23需用户方向。本轮无code/content/Scene。

> **CURRENT — B6 LEGACY RELEASETICK OWNER VERIFIED / PRODUCER HELD / SCHEMA DIRECTION REQUIRED（2026-09-08）：** `NTSD28-B6-LEGACY-RELEASE-TICK-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / NO_AUTHORITY_FIELD_OR_READER / TWO_PACKAGE_SPLIT / CURRENT_RELEASE_WITNESS / SCHEMA_DIRECTION_GATED / PRODUCTION_HELD`。2.8无对应字段或reader；Unity DVX/kind3/consume写tick并进入checksum/parity。先去动态writers并暂留reserved-1；删除carrier与snapshot/checksum schema13/20/23另需用户方向。current DVX2125、kind3 11116、refill edges3/35。本轮无code/content/Scene。

> **CURRENT — B6 HELD MISSING-ACTION CONTINUE OWNER VERIFIED / PRODUCTION HELD（2026-09-08）：** `NTSD28-B6-WPOINT-MISSING-ACTION-CONTINUE-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / ACTION_WRITE_THEN_CONTINUE / REAL_AND_GENERIC_OWNER / CURRENT_16796_WITNESS / DORMANT_RULES_UNION_RECONFIRMED / PRODUCTION_HELD`。Authority只写missing child action后diagnostic/continue，保留relation且零RNG；Unity real/generic会继续pose/DVX/kind3，real null-entry还漏后续refill/recovery。union16796/2402；cover2/state12/18重验0。cleanup→terminal runtime先行，无code/content/Scene。

> **CURRENT — B6 HELD RELATION PRODUCER DOMAIN CORRECTED / FOLLOW-UP AUDITS COMPLETE（2026-09-08）：** `NTSD28-B6-HELD-RELATION-PRODUCER-DOMAIN-CORRECTION-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / ITR_AND_OPOINT_UNION / CURRENT_REACHABILITY_REBASELINED / OWNERS_RETAINED / PRODUCTION_HELD`。39-holder只覆盖ITR kind2 pickup；62条OPoint kind2 direct-link使完整union成为41 sources/668 edges。current primary7624、kind3 811、nonkind3 DVX186、terminal33/23 defs、missing-action16796/2402。missing owner已闭合，cover/state12/18重验0；无code/content/Scene。

> **CURRENT — B6 TERMINAL WPOINT STRUCTURAL OWNER VERIFIED / RELATION DOMAIN CORRECTED / PRODUCTION HELD（2026-09-08）：** `NTSD28-B6-WPOINT-TERMINAL-STRUCTURAL-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / FREE_NOT_DESTROY_OWNER / POST_REFILL_PRE_POSE_GATE / CURRENT_33_WITNESS / CORRECTED_RELATION_DOMAIN / PRODUCTION_HELD`。terminal必须在refill/exhaustion后、任何child frame/pose/DVX/kind3前返回transient outcome，由world调用无破碎音效的`StructuralWriter.Free`；`Destroy`不是authority despawn。完整ITR+OPoint union terminal33/23 definitions且全为1000；原28/22是pickup子集。lifecycle cleanup runtime先行，本轮无code/content/Scene。

> **CURRENT — B6 DIRECTION-B MULTILINE CORPUS REBASELINED / RELATION DOMAIN FOLLOW-UP APPLIED（2026-09-08）：** `NTSD28-B6-DIRECTION-B-MULTILINE-CORPUS-CORRECTION-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / ROOT_CAUSE_MULTILINE_OMISSION / CURRENT_REACHABILITY_REBASELINED / OWNER_RULES_RETAINED / PRODUCTION_HELD`。projection实际WPoint7995/CPoint1426/ITR4437；后继OPoint domain纠正后完整held union为terminal33、kind3 811/authored overlap1、DVX186。CPoint kind1/state9 776/760、front/back170、injury223、Tayuya impact15/5、OID417 invalid40不变。missing-action owner与dormant union复核已闭合，无code/content/Scene。

> **CURRENT — B6 KIND2 PICKUP CORPUS CORRECTED / OWNER RETAINED（2026-09-08）：** `NTSD28-B6-KIND2-PICKUP-CORPUS-CORRECTION-001 / VERIFIED / GOVERNANCE_ONLY / CURRENT_CORPUS_CORRECTED / OWNER_AND_THREE_PACKAGES_RETAINED / PRODUCTION_HELD`。frozen projection为current117 kind2 / 39 holder definitions与31 supported target frames，不是旧1/1/17；漏项含OID447/449/501/502/506的15个type1 frames。release375/53、kind7 0/0、target WPoint 0/0不变。OID120 witness非唯一，owner/三包保留。本轮无code/content/Scene。

> **CURRENT — B6 POSITIVE-LINK VALIDATION RETIREMENT OWNER VERIFIED / POST-LIFECYCLE HELD（2026-09-08）：** `NTSD28-B6-POSITIVE-LINK-VALIDATION-RETIREMENT-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / UNITY_ONLY_MUTATING_PASS_CONFIRMED / POST_LIFECYCLE_RETIREMENT_PACKAGE_DEFINED / PRODUCTION_HELD`。Authority无post-catch standalone validation；Unity却单边清holder LinkState并保留其他正反向字段/发额外event。先做registry lifecycle cleanup并取runtime绿灯，再原子退休phase/pass/config/index/parity旧witness；AI relation projection保留，invalid-preserve另包。本轮无code/content/Scene。

> **CURRENT — B6 KIND2 PICKUP RELATION OWNER VERIFIED / CORPUS CORRECTED / THREE PACKAGES HELD（2026-09-08）：** `NTSD28-B6-KIND2-PICKUP-RELATION-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / THREE_PACKAGE_SPLIT_DEFINED / CURRENT_OID120_WITNESS / KIND7_DORMANT_RETIREMENT / PRODUCTION_HELD`。current有117条kind2、39个holder；OID120 frame64是非唯一witness，Authority写101而Unity写1并漏target OwnerSlot/conditional +35C。release375 kind2，kind7两端0；supported ground frames current31/release53均无WPoint。后续carrier/rules→pure→atomic，runtime未清无code/content/Scene。

> **CURRENT — B6 C22 HORIZONTAL IMPULSE FINALIZER STANDALONE EXIT READY（2026-09-08）：** `NTSD28-B6-HORIZONTAL-IMPULSE-FINALIZER-EXIT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / C22_STANDALONE_EXIT_READY / PRODUCER_AND_STAGE_DEPENDENCIES_ROUTED / JOINT_TRACE_PENDING / NO_NEW_PRODUCTION_PACKAGE`。hold/count/pending三轴、公式、count0/negative清理与active scan均等价；无独立C22代码包。catch escape、impact、B8 stage removal和B12 joint trace仍各自未闭合，不能称整体impulse已对齐。

> **CURRENT — B6 KIND2 CPOINT HURT-ACTION CONSUMER OWNER VERIFIED / RETIREMENT HELD（2026-09-08）：** `NTSD28-B6-CPOINT-KIND2-HURT-ACTION-CONSUMER-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / UNITY_ONLY_HURT_OVERRIDE_CONFIRMED / ATOMIC_ACTUAL_SHADOW_LEGACY_RETIREMENT_DEFINED / CURRENT_CORPUS_WITNESS / PRODUCTION_HELD`。current2.8 closure没有CPoint front/back hit-tail consumer；Unity旧alias却在default actual、HitPlan和legacy覆盖action。OID300 criminal抓OID33 clone到130后，第三方nonknockdown hit可触发Unity132/131 extra write。后续atomic retirement包与旧tests一起改，schema再后继；runtime未清，本轮无code/content/Scene。

> **CURRENT — B6 CPOINT 27-SCALAR SCHEMA OWNER VERIFIED / CONTENT+RUNTIME GATED（2026-09-08）：** `NTSD28-B6-CPOINT-27-SCALAR-SCHEMA-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / AUTHORITY_27_SCALARS_REQUIRED / RELEASE_DRAIN_WITNESS / LEGACY_ALIAS_CONSUMER_AUDIT_REQUIRED / CONTENT_STRATEGY_GATED / PRODUCTION_HELD`。Unity 19-scalar value缺8个2.8字段；release OID63/27/449 action414有3条drain600正式witness。旧converter又把kind2 front/back alias到Injury/Cover，4/398条数据被actual/HitPlan旧hurt-action消费，不能只追加字段。下一先审kind2 consumer，再versioned 27-scalar；resource等B7/B8/B11/H与内容策略。本轮无code/content/Scene/package。

> **CURRENT — B6 CATCH CONTROL-FLOW FENCES OWNER VERIFIED / CORPUS REBASED / PRODUCTION HELD（2026-09-08）：** `NTSD28-B6-CATCH-CONTROL-FLOW-FENCES-OWNER-AUDIT-001 / VERIFIED / ADVANCE_PACKAGE_AMENDED / SETTLEMENT_PACKAGE_DEFINED / CURRENT_AND_RELEASE_TREE_WITNESS / PRODUCTION_HELD`。Authority mismatch/negative escape立即continue；Unity误继续tail且写错counter。current kind1/state9/negative=776/760/204，OID417 tree有40个invalid pairs；release OID555有134。advance与settlement owners不变，runtime/Play pending。

> **CURRENT — B6 DEAD FLUTEFORCE API OWNER VERIFIED / PRODUCTION HELD（2026-09-08）：** `NTSD28-B6-NTSDSPEC-DEAD-FLUTE-API-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / DEAD_API_RETIREMENT_PACKAGE_DEFINED / IMPACT_SEPARATE / PRODUCTION_HELD`。只有base旧mass/threshold方法与weapon空override，全repo无caller；actual impact也不经过它。后续单包删除两符号并加guard，不把impact塞回shell virtual。runtime阻塞未变，本轮无code。

> **CURRENT — B6 NTSDSPEC CHARACTER MASS OWNER VERIFIED / PRODUCTION HELD（2026-09-08）：** `NTSD28-B6-NTSDSPEC-MASS-CARRIER-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / CHARACTER_MASS_RETIREMENT_PACKAGE_DEFINED / FORMAL_OUTPUT_UNCHANGED / PRODUCTION_HELD`。Authority friction无mass；Unity仅`ctx.mass>0`读取。所有正式type0得到1，当前formal输出不变；synthetic mass却可改physics并被shell snapshot携带。后续单包删除context/character/snapshot mass、shell schema1→2；dead Flute lookup另包。runtime阻塞未变，本轮无code。

> **CURRENT — B6 NTSDSPEC COMPAT WEAPON ACTION OWNER VERIFIED / PRODUCTION HELD（2026-09-08）：** `NTSD28-B6-NTSDSPEC-COMPAT-WEAPON-ACTION-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / SHARED_SELECTOR_PACKAGE_DEFINED / DEFAULT_PROFILE_UNAFFECTED / PRODUCTION_HELD`。默认DataOriented已用native selector；Legacy仍正式可配置。旧四bool均不拥有合法relation决策；Legacy还有type6 neutral 52-vs55、heavy stats、relation4 all-directions和nonzero fields缺口。下一单包抽shared selector并删旧wrappers；current OID122/123可作Legacy witness。runtime阻塞未变，本轮无code。

> **CURRENT — B6 NATIVE IMPACT 10/11/17/18 OWNER VERIFIED / CORRECTED CURRENT WITNESS / PRODUCTION HELD（2026-09-08）：** `NTSD28-B6-NATIVE-IMPACT-10111718-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / THREE_PACKAGE_SPLIT_DEFINED / CURRENT_AND_RELEASE_WITNESS / PRODUCTION_HELD`。kind11的真实gate是Environment320，17/18分character/object；character写environment/encoded credit/physical source/respond，object按type+201/202 immunity，统一精确`/1.07`与Y尾。Unity误用并写WeaponCount、+11 legacy stats、漏三exact字段/owner/17/18。current/release kind10/11为15/5与121/61；current20条均在OID36 Tayuya actions243..247，17/18两端仍0。HitPlan carrier→pure core→atomic三包已冻结；runtime阻塞未变，本轮无code。

> **CURRENT — B6 OLD NTSDSPEC OWNER INVENTORY VERIFIED / CORPUS CORRECTED / PRODUCTION HELD（2026-09-08）：** `NTSD28-B6-NTSDSPEC-PRODUCTION-OWNER-INVENTORY-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / FIVE_PACKAGE_SPLIT_DEFINED / PRODUCTION_HELD`。6行7个生产调用表达式已穷尽。Authority无mass/oscillate字段：ground friction无条件，held action由link-state+linked DAT stats选择；actual impact与render-phase另有owner。当前/release linked action stats均0 entry；impact current/release kind10/11/17/18已纠正为15/5/0/0与121/61/0/0。dead FluteForce/EffectCreate无repo producer。旧ID已指向不同对象。后续拆mass、compat weapon、native impact、B9 oscillate、empty-shell五包；runtime阻塞未变，本轮无code。

> **CURRENT — B6 CATCH ADVANCE MIXED SLOT ORDER ROUTED / PRODUCTION HELD（2026-09-08）：** `NTSD28-B6-CATCH-ADVANCE-SLOT-ORDER-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / SINGLE_ASCENDING_MIXED_PASS_REQUIRED / EXACT_CONSUMER_PACKAGE_DEFINED / PRODUCTION_HELD`。Authority逐slot混合kind1/current-kind2；Unity全kind1→全kind2并不等价。release有2条action343 throw到vaction132/next344与134个type0 action132-kind2定义，可构造低/高slot witness；当前语料无触发。后续mixed pass同时迁exact CatchSourceSlot90 consumer，settlement仍独立；Play与runtime gate待清，本轮无code。

> **CURRENT — B6 HELD WPOINT RULES CORPUS CORRECTED / DORMANT UNION RECONFIRMED（2026-09-08）：** `NTSD28-B6-HELD-WPOINT-DORMANT-RULES-AUDIT-001 / VERIFIED / CORRECTED_BY_MULTILINE_AND_RELATION_DOMAIN_AUDITS / DORMANT_RULES_UNION_RECONFIRMED / PRODUCTION_HELD`。current full WPoint7995，完整held union41 definitions/7624 primary、terminal33、kind3 811、nonkind3 DVX186；terminal owner已闭合。cover2与referenced state12/18在668-edge union仍0；generic Vz/damaged-drop owner保留。本轮无code。

> **CURRENT — B6 CATCH RELATION EXACT-FIELD OWNER VERIFIED / PRODUCTION HELD（2026-09-08）：** `NTSD28-B6-CATCH-RELATION-EXACT-FIELDS-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / ATOMIC_ACTUAL_AND_SHADOW_PACKAGE_DEFINED / PRODUCTION_HELD`。kind3 relation的双frame preflight、signed first action、anchor/motion、exact reciprocal slots、respond timeout与Fall清零已冻结。Unity actual/HitPlan漏`CatchSourceSlot90`，compat `CatcherSlotIndex`不能替代；当前/release语料1/548均无非零respond或负/缺失action。下一actual+shadow包等待runtime栈清理，本轮无code。

> **CURRENT — B6 ENTITY-LINK LIFECYCLE OWNER VERIFIED / PRODUCTION HELD（2026-09-08）：** `NTSD28-B6-ENTITY-LINK-LIFECYCLE-CLEANUP-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / TWO_PRODUCTION_PACKAGE_SPLIT_DEFINED / PRODUCTION_HELD`。held/catch exact与compat字段、Kind4/HolderCopy exclusions及registry原子点已冻结；先做cleanup production，runtime绿后才允许invalid-preserve。本轮无code。

> **CURRENT — B6 HELD RELATION LIFECYCLE REACHABILITY CONFIRMED（2026-09-08）：** `NTSD28-B6-HELD-RECIPROCAL-LIFECYCLE-REACHABILITY-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / LIFECYCLE_REACHABILITY_CONFIRMED / ATOMIC_DESPAWN_LINK_CLEANUP_REQUIRED / PRODUCTION_HELD`。Authority release前清全部held/catch引用；Unity先release/可same-tick reuse，C09/C20补清导致窗口与ABA。下一必须先做exact-field/structural owner audit，再按cleanup→invalid-handler实施；当前无code。

> **CURRENT — B6 INVALID HELD RECIPROCAL OLD REACHABILITY SUPERSEDED（2026-09-08）：** `NTSD28-B6-HELD-RECIPROCAL-FAILURE-AUDIT-001 / SUPERSEDED / REACHABILITY_RESOLVED_BY_NTSD28-B6-HELD-RECIPROCAL-LIFECYCLE-REACHABILITY-AUDIT-001`。同状态差异保留；动态可达与正确owner已由顶部后继记录闭合。

> **CURRENT — B6 HELD DVX +0x2F8 SPLIT / PRODUCTION HELD（2026-09-08）：** `NTSD28-B6-WPOINT-DVX-EXCLUDED-GROUP-CARRIER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / THREE_PACKAGE_SPLIT_DEFINED / PRODUCTION_HELD`。Native +0x2F8只属held type1/4/6 writer→non-character AI excluded-group consumer；Unity general Spawner有额外writers，禁止复用。已拆carrier、writer、consumer三包，consumer仍需C02/legacy owner审计；当前无code。

> **CURRENT — B6 HELD DVX WEAPON HP DIFFERENCE ROUTED / PRODUCTION HELD（2026-09-08）：** `NTSD28-B6-WPOINT-DVX-WEAPON-HP-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / SUBSEQUENT_WEAPON_HP_PRESERVATION_ROUTED / PRODUCTION_HELD`。Authority DVX不写weapon HP，Unity `OnThrown()`却重置至definition值；生产type1/2/4/6均受影响，当前/release non-kind3 DVX为3/599。下一actual只移除此重置，但kind3与runtime gate在前，本轮无code。

> **CURRENT — B6 WPOINT KIND3 OWNER CORRECTED / PRODUCTION HELD（2026-09-08）：** `NTSD28-B6-WPOINT-KIND3-RELEASE-OWNER-CORRECTION-001 / VERIFIED / GOVERNANCE_ONLY / TWO_ACTUAL_BRANCHES_DEFINED / PRODUCTION_HELD`。Authority DVX后仍继续独立kind3；Unity real/generic均提前return。production必须覆盖`RunStep12()` continuation和`DropRandomly()` final writer，type1/4/6四draw、type2五draw；旧sole-owner record已SUPERSEDED。当前多个B6包仍runtime pending，未叠加新code。

> **CURRENT — B6 HELD-REFILL MP/EXHAUSTION USER HOLD（2026-09-09）：** `NTSD28-B6-HELD-REFILL-MP-EXHAUSTION-PRODUCTION-001 / RUNTIME_PENDING / FOCUSED_7_OF_7 / C09_RELATED_2_OF_2 / BUILDS_0_ERROR / PLAY_PENDING / SELFCHECK_BLOCKED_UNRELATED_R2_CPOINT_SYNC / HP_BASEMAX_DEFERRED / USER_HOLD`。7-case与C09 placement已真实运行；专门refill Play仍待，暂停期间不得继续。

> **CURRENT — UNITY LICENSE DIAGNOSIS EXHAUSTED WITHOUT CREDENTIAL USE（2026-09-08）：** 第四次batch能启动versioned LicensingClient并handshake，但无Hub token而exit1；Hub CLI因沙箱不允许原子写AppData user-settings而EPERM。未读取/注入/回显敏感token；停止许可尝试。所4次都未进Test Runner且无XML，throw仍`RUNTIME_PENDING`。

> **CURRENT — B6 TRUE EARLIEST C09 BRANCH CORRECTED（2026-09-08）：** kind3之前的OID122/123 exhaustion才是更早可达差异：Authority random Vx、Vy=0、Vz保留；Unity Vy=-8/Vz=0。OID123还错过HP<=0入场并以KillCount误写holder MP cap。`NTSD28-B6-HELD-WPOINT-ENTRY-CORRECTION-AUDIT-001`已更正为`HELD_REFILL_MP_EXHAUSTION_ROUTED`；kind3 owner仍有效但后置。

> **CURRENT — B6 HELD-REFILL MP/EXHAUSTION OWNER VERIFIED（2026-09-08）：** `NTSD28-B6-HELD-REFILL-MP-EXHAUSTION-OWNER-AUDIT-001 / VERIFIED / ACTUAL_ONLY_PACKAGE_DEFINED / HP_BASEMAX_DEFERRED`。下一actual仅修`LF2WeaponHeldStateResolver.ProcessDrinkConsumption()`；OID122的authoritative baseMax clamp继续等B11/H。由于throw package仍受Unity Licensing阻塞，未叠加新code。

> **CURRENT — B6 GLOBAL ENTRY ORDER CORRECTED（relation domain rebased，2026-09-08）：** `NTSD28-B6-HELD-WPOINT-ENTRY-CORRECTION-AUDIT-001 / VERIFIED / GLOBAL_B6_ORDER_CORRECTED / WPOINT_KIND3_ROUTED`。C09第一次held settlement早于geometry/hit/catch；旧throw“首差”限定为post-hit catch子链。release kind3=2744；完整current held-union kind3=811且已有Rock Lee authored-DV overlap。refill仍是更早分支，terminal/current kind3均为后继。

> **CURRENT — B6 WPOINT KIND3 OLD OWNER SUPERSEDED（2026-09-08）：** `NTSD28-B6-WPOINT-KIND3-RELEASE-OWNER-AUDIT-001 / SUPERSEDED / CORRECTED_BY_NTSD28-B6-WPOINT-KIND3-RELEASE-OWNER-CORRECTION-001`。四draw/final-write事实保留，但sole-`DropRandomly`实施边界无效；以顶部correction为准。

> **CURRENT — UNITY LICENSE RETRY 3 STILL BLOCKED（2026-09-08）：** stale lock PID 7192已不存在，单一batch focused run成功拉起LicensingClient PID 53168，但`LicenseClient-Logan` channel在60.01秒内未建立，Unity return199、XML未生成。throw package仍`RUNTIME_PENDING`；不得声称focused/SelfCheck/Play已运行。日志`Temp/NTSD28-B6-CpointThrow-focused-retry.log`。

> **CURRENT — B6 HELD-INJURY OWNER VERIFIED（2026-09-08）：** `NTSD28-B6-HELD-INJURY-ACCOUNTING-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / TWO_PRODUCTION_PACKAGE_SPLIT_DEFINED / FULL_RESOURCE_DEFERRED`。下一候选是accounting+cover actual：`IncomingDamageScale340`、直接owner/type0-self credit、HP/HPBound/HP-consumed/score/KO与cover timer；不写legacy Kill/Combo/global stats。caughtact event必须在整个settlement后单独接线；world KO feed和完整MP resource仍后置。throw package未有Unity runtime绿灯前不叠加新行为修改。

> **CURRENT — B6 POST-THROW FIRST DIFFERENCE ROUTED（corpus rebased，2026-09-08）：** `NTSD28-B6-POST-THROW-SETTLEMENT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / HELD_INJURY_ACCOUNTING_ROUTED`。release正held injury484；current kind1正injury223，cover为0:205/1:15/11:3。Unity误用FallDamageDiv/KillCount/HolderCopySlot，漏canonical HP consumed/score/KO，并错误处理cover timer；owner不变、matrix扩大。

> **CURRENT — B6 POST-THROW READ-ONLY AUDIT（2026-09-08）：** `NTSD28-B6-POST-THROW-SETTLEMENT-AUDIT-001 / IN_PROGRESS / GOVERNANCE_ONLY`。throw代码仍RUNTIME_PENDING；当前只读继续settle_catch_relations/held/caughtact尾，选择下一首差并维持full-resource B7/B8/B11/H blocker。不改code/content/Scene。

> **CURRENT — B6 CPOINT THROW CODE WRITTEN / UNITY LICENSE BLOCKED（2026-09-08）：** `NTSD28-B6-CPOINT-THROW-ENVIRONMENT-VZ-PRODUCTION-001 / RUNTIME_PENDING / ISOLATED_COMPILE_PASS / FULL_RESOURCE_DEFERRED`。invalid MP call已移除；display/environment/self-source/WeaponCount exclusion/Vz XOR及8-case test已写。Runtime47/0、Editor104/0、source7/7、Ledger355/307、Scene SHA通过；Unity Test Runner两次未启动即因Licensing IPC return199退出，SelfCheck/Play也未跑，禁止报VERIFIED。

> **CURRENT — B6 CPOINT THROW SCOPE CORRECTED（2026-09-08）：** `NTSD28-B6-CPOINT-THROW-ENVIRONMENT-VZ-PRODUCTION-001 / IN_PROGRESS / TEST_FIRST / FULL_RESOURCE_DEFERRED`。旧atomic包因用MPMax提前连接完整MP transaction而SUPERSEDED；后继包移除该调用，只闭合可精确的display/environment/self-source/WeaponCount exclusion/Vz。B7/B8/B11/H依赖保持显式。Unity batch两次Licensing IPC 199，无runtime绿灯。

> **CURRENT — B6 CPOINT THROW ATOMIC PRODUCTION（2026-09-07）：** `NTSD28-B6-CPOINT-THROW-ATOMIC-PRODUCTION-001 / IN_PROGRESS / TEST_FIRST`。只改BattleCpointWriter、focused test、SelfCheck与grab Play probe；复用B5 resource core，正injury写Environment/self source且保持WeaponCount，无独占depth输入保留Vz。RED优先；HitPlan/content/Scene不改。

> **CURRENT — B6 CPOINT THROW OWNER VERIFIED（2026-09-07）：** `NTSD28-B6-CPOINT-THROW-OWNER-AUDIT-001 / VERIFIED / ATOMIC_PRODUCTION_SPLIT_DEFINED`。actual-only、现有schema/parity足够、HitPlan排除；完整CPoint字段与held settlement继续后置。

> **CURRENT — B6 CPOINT THROW OWNER AUDIT（2026-09-07）：** `NTSD28-B6-CPOINT-THROW-OWNER-AUDIT-001 / IN_PROGRESS / GOVERNANCE_ONLY / READ_ONLY_OWNER_SPLIT`。B6入口首差是kind-1 throw：Authority执行resource transaction并写caught EnvironmentState320/self source，且无独占depth输入时保留Vz；Unity误写WeaponCount、缺resource transaction并清零Vz。当前只读冻结formal/actual/HitPlan责任，不改code/content/Scene。

> **CURRENT — B6 ENTRY AUDIT VERIFIED（2026-09-07）：** `NTSD28-B6-ENTRY-CATCH-SETTLEMENT-AUDIT-001 / VERIFIED / CPOINT_THROW_SETTLEMENT_ROUTED`。post-hit order与4019-CPoint正式corpus已闭合。完整CPoint input action规则缺口当前corpus不可达，保留后置；下一owner audit。

> **CURRENT — B6 ENTRY AUDIT STARTED（2026-09-07）：** `NTSD28-B6-ENTRY-CATCH-SETTLEMENT-AUDIT-001 / IN_PROGRESS / GOVERNANCE_ONLY`。只读复核post-hit catch relation/settlement、held/cpoint与impulse tail，选择B6首个production差异；caughtact combo与cpoint resource等真实settlement event。当前不改code/content/Scene。

> **CURRENT — B5 EXIT READY / NEXT B6（2026-09-07）：** `NTSD28-B5-EXIT-GATE-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / B5_PLACEMENT_EXIT_READY / B5_FULL_CLOSE_DEFERRED`。standalone B5 first-difference scan已扫到Authority driver return，无新独立B5首差；下一阶段4.8/B6。B6/B7/B8/B10/B11/H及最终joint trace仍后置，禁止写成整个命中系统或全项目已完全一致。

> **CURRENT — B5 NATIVE COMBO EXPIRY VERIFIED（2026-09-07）：** `NTSD28-B5-NATIVE-COMBO-EXPIRY-001 / VERIFIED / CORE_COMBO_EXPIRE_ROUTED / FORMAL_TUPLE_INACTIVE`。C25 live-slot尾后按record/negative-respond gate与inclusive elapsed扫描active slots，只清positive count并保留lastTick；focused5、B5-831、NTSD28-1178、fresh SelfCheck、Console0通过，Scene unchanged。B6 caughtact、B10显示、H激活仍未接；下一步重新执行B5 exit gate。

> **CURRENT — B5 NATIVE COMBO ORDINARY PRODUCER VERIFIED（2026-09-07）：** `NTSD28-B5-NATIVE-COMBO-ORDINARY-PRODUCER-001 / VERIFIED / PRODUCTION_ROUTED / FORMAL_TUPLE_INACTIVE`。shared runner普通Damage成功后按slot回查并执行bound/type/facing/one-hop/current-process-tick生产；focused7、B5-826（唯一MCP污染项isolated1）、NTSD28-1173、fresh SelfCheck与Console0通过，Scene unchanged。expiry、B6/B10/H未接；下一包为C25后expiry。

> **CURRENT — B5 NATIVE COMBO CARRIERS VERIFIED（2026-09-07）：** `NTSD28-B5-NATIVE-COMBO-CARRIERS-001 / VERIFIED / CARRIERS_READY / BEHAVIOR_UNCONNECTED`。独立entity count/lastTick与world tuple已闭合deterministic state链；focused6、related29、B5-819、NTSD28-1166与fresh SelfCheck通过，Scene unchanged。默认关闭，不接producer/expiry/B6/B10/H；下一包为ordinary producer。

> **CURRENT — B5 NATIVE COMBO OWNER AUDIT VERIFIED（2026-09-07）：** `NTSD28-B5-NATIVE-COMBO-RUNTIME-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / FIVE_PACKAGE_SPLIT_DEFINED`。carrier→ordinary producer→expiry；caughtact B6、presentation B10、activation H。

> **CURRENT — B5 NATIVE COMBO RUNTIME OWNER AUDIT STARTED（2026-09-07）：** `NTSD28-B5-NATIVE-COMBO-RUNTIME-OWNER-AUDIT-001 / IN_PROGRESS / GOVERNANCE_ONLY / READ_ONLY_OWNER_SPLIT`。冻结mode/entity载体、ordinary/caughtact producer、C25后expiry与B10表现边界；当前只读。

> **CURRENT — B5 REMAINING EXIT AUDIT 011 VERIFIED（2026-09-07）：** `NTSD28-B5-REMAINING-EXIT-AUDIT-011 / VERIFIED / GOVERNANCE_ONLY / NATIVE_COMBO_RUNTIME_ROUTED / B5_EXIT_NOT_READY`。Authority正式combo runtime可达；Unity仅有输入combo/伤害累计，无专用count/last tick、producer或实际expiry。下一owner audit。

> **CURRENT — B5 REMAINING EXIT AUDIT 011 STARTED（2026-09-07）：** `NTSD28-B5-REMAINING-EXIT-AUDIT-011 / IN_PROGRESS / GOVERNANCE_ONLY / READ_ONLY_FIRST_DIFFERENCE_SCAN`。state2000 away damping已闭合；继续只读扫描reduced余尾、unarmored/non-character continuation、hit-record与命中后生命周期；不改code/content/Scene。

> **CURRENT — B5 REDUCED STATE2000 AWAY DAMPING VERIFIED（2026-09-07）：** `NTSD28-B5-REDUCED-STATE2000-AWAY-DAMPING-PRODUCTION-001 / VERIFIED / REDUCED_STATE2000_AWAY_DAMPING_ALIGNED`。actual/HitPlan共享double-X严格away predicate并`/2.5`；RED10、focused14、HitPlan185、B5-813；NTSD28 broad唯一MCP日志污染项isolated1通过；SelfCheck PASS、Console0、Scene unchanged。下一audit011。

> **CURRENT — B5 REDUCED STATE2000 AWAY DAMPING STARTED（2026-09-07）：** `NTSD28-B5-REDUCED-STATE2000-AWAY-DAMPING-PRODUCTION-001 / IN_PROGRESS / TEST_FIRST`。Authority double-X away-only；actual/HitPlan使用XInt与toward极性；formal OID150可达。下一步focused RED后最小共享判定。

> **CURRENT — B5 REMAINING EXIT AUDIT 010 VERIFIED（2026-09-07）：** `NTSD28-B5-REMAINING-EXIT-AUDIT-010 / VERIFIED / GOVERNANCE_ONLY / REDUCED_STATE2000_AWAY_DAMPING_ROUTED / B5_EXIT_NOT_READY`。下一首差已唯一定位；无code/content/Scene写入。

> **CURRENT — B5 REMAINING EXIT AUDIT 010 STARTED（2026-09-07）：** `NTSD28-B5-REMAINING-EXIT-AUDIT-010 / IN_PROGRESS / GOVERNANCE_ONLY / READ_ONLY_FIRST_DIFFERENCE_SCAN`。201/214 route/order已闭合；现从C++其后target-type/effect/audio/spark/deferred lifecycle及reduced tail继续只读选择下一首差，不改code/content/Scene/Authority。

> **CURRENT — B5 SYSTEM-TABLE ATTACKER TERMINAL VERIFIED（2026-09-07）：** `NTSD28-B5-SYSTEM-TABLE-ATTACKER-TERMINAL-PRODUCTION-001 / VERIFIED / SYSTEM_TABLE_ATTACKER_TERMINAL_ALIGNED`。201/214不再泄漏到armor/defense reduced；214按Authority早期清零，201保留slot到hit-record tail后释放，broken fallback仍消费。RED2+order RED；focused11、HitPlan185、B5-149、NTSD28-330、18:45:27Z SelfCheck；Console0、Scene unchanged。下一audit010。

> **CURRENT — B5 SYSTEM-TABLE ATTACKER TERMINAL PRODUCTION STARTED（2026-09-07）：** `NTSD28-B5-SYSTEM-TABLE-ATTACKER-TERMINAL-PRODUCTION-001 / IN_PROGRESS / TEST_FIRST`。Authority 201/214表只属于unarmored type0 target续行；Unity旧`LF2SpecialAttack` tail会在armor/defense reduced hit后也销毁/清零攻击者，且214时点偏晚。先写两个reduced RED，再迁入DamageWriter精确顺序。SceneView/Hierarchy修复已验证且保持，不保存Scene。

> **CURRENT — B5 REMAINING EXIT AUDIT 009 VERIFIED（2026-09-07）：** `NTSD28-B5-REMAINING-EXIT-AUDIT-009 / VERIFIED / GOVERNANCE_ONLY / SYSTEM_TABLE_ATTACKER_TERMINAL_ROUTED / B5_EXIT_NOT_READY`。首差已唯一定位为system-DAT henry_arrow201/john_biscuit214 route/order；下一production001。审计无code/content/Scene写入。

> **CURRENT — B5 REMAINING EXIT AUDIT 009 STARTED（2026-09-07）：** `NTSD28-B5-REMAINING-EXIT-AUDIT-009 / IN_PROGRESS / GOVERNANCE_ONLY / READ_ONLY_FIRST_DIFFERENCE_SCAN`。first-BDY规则族已闭合；现从C++成功/失败后的下一语句继续审计hit tail、Unity owner、formal reachability和既有family记录，只选下一唯一首差，不改代码/content/Scene/Authority。

> **CURRENT — B5 FIRST-BDY RESPONSE ATOMIC PRODUCTION VERIFIED（2026-09-07）：** `NTSD28-B5-FIRST-BDY-RESPONSE-ATOMIC-PRODUCTION-INTEGRATION-001 / VERIFIED / FIRST_BDY_RESPONSE_PRODUCTION_ALIGNED / FORMAL_CRIMINAL_PLAY_PASS`。shared runner在consume前统一处理1xxx/2xxx/encoded、同步RNG、实际写入与per-attacker abort；HitPlan/DataOriented同步。正式criminal见证揭示并修复Unity `Oid300Redirect`提前绕过响应。RED→focused17、B5-138、HitPlan185、NTSD28-318、01:37:51 SelfCheck及formal Play11 PASS（frame30/kind1033→frame33/group1/hold3,-3/counter77/HP100/vrest0，cleanup true，artifact SHA F5286842）；Console0、已退出Play、Scene dirtyfalse/root13/SHA不变、Ledger341/297。下一`NTSD28-B5-REMAINING-EXIT-AUDIT-009`。

> **CURRENT — B5 FIRST-BDY RESPONSE PURE CORE VERIFIED（2026-09-07）：** `NTSD28-B5-FIRST-BDY-RESPONSE-PURE-CORE-001 / VERIFIED / PURE_CORE_READY / BEHAVIOR_UNCONNECTED`。外置roll的pure resolver已覆盖1xxx/2xxx strict bounds、respond、encoded decimal/chance/actions/effect与raw injury；warm100000次0 allocation。RED CS0246、focused39、B5 653、NTSD28 1118、SelfCheck PASS、Console0、Scene不变、Ledger340/295。下一`NTSD28-B5-FIRST-BDY-RESPONSE-ATOMIC-PRODUCTION-INTEGRATION-001`。

> **CURRENT — B5 FIRST-BDY RESPONSE CARRIER VERIFIED（2026-09-06）：** `NTSD28-B5-FIRST-BDY-RESPONSE-CARRIER-001 / VERIFIED / CARRIER_READY / BEHAVIOR_UNCONNECTED`。保留formal X/Y/W/H与旧suppression字段，一般化first-kind入口及first-respond已完成。RED7、focused4/4、B5 614/614、NTSD28 1079/1079、SelfCheck PASS、Console0、Scene不变、Ledger339/293。runner/damage/RNG/HitPlan未接；下一`NTSD28-B5-FIRST-BDY-RESPONSE-PURE-CORE-001`。

> **CURRENT — B5 FIRST-BDY RESPONSE OWNER AUDIT VERIFIED（2026-09-06）：** `NTSD28-B5-FIRST-BDY-RESPONSE-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / THREE_PACKAGE_SPLIT_DEFINED`。formal body几何保持X/Y/W/H；first kind/respond carrier、外置roll pure core、shared pre-consume actual/RNG writer、HitPlan attempt shadow与per-attacker abort已冻结。下一`NTSD28-B5-FIRST-BDY-RESPONSE-CARRIER-001`→pure→atomic；本审计无code/content/Scene/authority写入。

> **CURRENT — B5 REMAINING EXIT AUDIT 008 VERIFIED（2026-09-06）：** `NTSD28-B5-REMAINING-EXIT-AUDIT-008 / VERIFIED / GOVERNANCE_ONLY / FIRST_BDY_RESPONSE_ROUTED / B5_EXIT_NOT_READY`。下一首差是目标当前帧第一个BDY的1xxx/2xxx/encoded response：Unity丢失BDY `respond`，且缺同步RNG、actual/HitPlan、成功后跳过普通尾部/combo和per-attacker abort。Authority runtime有55+157个可达首BDY帧，Unity冻结Config也有22个1xxx帧。下一`NTSD28-B5-FIRST-BDY-RESPONSE-OWNER-AUDIT-001`；本审计无code/content/Scene/authority写入。

> **CURRENT — B5 HIT-GROUP ELIGIBILITY FAMILY EXIT READY（2026-09-06）：** `NTSD28-B5-HIT-GROUP-ELIGIBILITY-EXIT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / HIT_GROUP_ELIGIBILITY_FAMILY_EXIT_READY`。三collector、pre-nearest/capacity/RNG、frozen shared/cached consumer、kind5 holder、store/HitPlan identity及旧truth-table退役均复审闭合；下一`NTSD28-B5-REMAINING-EXIT-AUDIT-008`。本审计无code/content/Scene写入。

> **CURRENT — B5 HIT-GROUP ELIGIBILITY ATOMIC PRODUCTION VERIFIED（2026-09-06）：** `NTSD28-B5-HIT-GROUP-ELIGIBILITY-ATOMIC-PRODUCTION-INTEGRATION-001 / VERIFIED / HIT_GROUP_ELIGIBILITY_PRODUCTION_ALIGNED`。formal三collector在nearest/capacity/RNG前冻结/筛选，shared/cached consumer只读valid pair并闭合kind5 holder；red8、focused25、RoleAware/store/HitPlan255、B5 610、NTSD28 1168、build0、collision-hit Play10 candidates+cleanup、22:38 SelfCheck、Console0、Scene dirtyfalse/root13/SHA不变、Ledger335/292 PASS。下一exit audit。

> **CURRENT — B5 HIT-GROUP ELIGIBILITY PURE CORE VERIFIED（2026-09-06）：** `NTSD28-B5-HIT-GROUP-ELIGIBILITY-PURE-CORE-001 / VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`。七步规则及zero-allocation已闭合；red7、focused52、B5 585、Unity侧NTSD28 1143、build0、21:34 SelfCheck、Console0、Scene dirtyfalse/root13/SHA不变、Ledger334/290 PASS。下一atomic production；本包未接query/runner/HitPlan。

> **CURRENT — B5 HIT-CANDIDATE PAIR SNAPSHOT CARRIER VERIFIED（2026-09-06）：** `NTSD28-B5-HIT-CANDIDATE-PAIR-SNAPSHOT-CARRIER-001 / VERIFIED / CARRIER_READY / FORMAL_PRODUCER_UNCONNECTED`。Authority完整18-payload+valid snapshot已贯穿SceneQueryHit/store/shadow/cached rebuild/shared runner/HitPlan identity；formal producer仍为invalid，无eligibility或persistent schema变化。red2、dedicated4、HitPlan185、RoleAware92、B5 533、Unity侧NTSD28 1091、build0、21:03 SelfCheck、Console0、Scene dirtyfalse/root13/SHA不变、Ledger333/288 PASS。下一pure core。

> **CURRENT — B5 WORLD HIT-GROUP MODE GATE CARRIER VERIFIED（2026-09-06）：** `NTSD28-B5-WORLD-HIT-GROUP-MODE-GATE-CARRIER-001 / VERIFIED / CARRIER_READY / BEHAVIOR_AND_CONTENT_UNCONNECTED`。world `ActiveModeHitGroupGate18`已闭合default/reset/restore/core snapshot/checksum/full parity，schema9/18/21且entity11不变；red9、focused19+12+21、B5 529、Unity侧NTSD28 1087、build0、20:27 SelfCheck、Console0、Scene dirtyfalse/root13/SHA不变、Ledger332/285 PASS。下一pair snapshot carrier；candidate/consumer/content/full-restore behavior未接。

> **CURRENT — B5 HIT-GROUP OWNER AUDIT VERIFIED（2026-09-06）：** `NTSD28-B5-HIT-GROUP-ELIGIBILITY-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / FOUR_PACKAGE_SPLIT_DEFINED`。完整18-payload+valid pair snapshot、`TryRecordReleaseCandidate`三collector汇合点、shared/cached consumer、store shadow、HitPlan identity与world mode18 scalar/schema责任已冻结。下一world carrier→pair carrier→pure core→atomic production；B8/H producer独立。本审计无code/content/Scene/authority写入。

> **CURRENT — B5 REMAINING EXIT AUDIT 007 VERIFIED（2026-09-06）：** `NTSD28-B5-REMAINING-EXIT-AUDIT-007 / VERIFIED / GOVERNANCE_ONLY / HIT_GROUP_ELIGIBILITY_FROZEN_PAIR_ROUTED / B5_EXIT_NOT_READY`。下一首差是native hit-group eligibility+candidate frozen pair：Unity使用collision-state和旧kind集合，缺state190反转/state180/mode gate，type0/type3 opposing-facing极性相反，consumer还会重读live group/action/facing。下一`NTSD28-B5-HIT-GROUP-ELIGIBILITY-OWNER-AUDIT-001`；本审计无code/content/Scene/authority写入。

> **CURRENT — B5 SPECIAL-HIT LATCH ATOMIC VERIFIED（2026-09-06）：** `NTSD28-B5-SPECIAL-HIT-LATCH-ATOMIC-PRODUCTION-INTEGRATION-001 / VERIFIED / SPECIAL_HIT_LATCH_PRODUCTION_ALIGNED / LEGACY_WEAPON_CONFIRM_PRESERVED`。四type3 actual、shared pre-writer type0 gate与五HitPlan projection/diff已原子迁独立latch；普通weapon旧`HitConfirm2`与两clear边界保持。red4、atomic4/type3-18/HitPlan184/B5-524/NTSD28-1082、18:58 SelfCheck、collision-hit Play（10 candidates；weapon旧confirm、type3新latch、whole-attacker abort）均PASS；最终Console0、已退出Play、Scene dirtyfalse/root13/SHA不变。下一remaining exit audit 007。

> **CURRENT — B5 SPECIAL-HIT LATCH OWNER AUDIT VERIFIED（2026-09-06）：** `NTSD28-B5-SPECIAL-HIT-LATCH-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / ATOMIC_INTEGRATION_READY`。Authority四producer/两consumer与Unity四actual/单shared runner/五HitPlan projection均已冻结；下一必须test-first原子迁移，普通weapon旧`HitConfirm2`与两clear边界保留。本审计无code/content/Scene/authority写入。

> **CURRENT — B5 SPECIAL-HIT LATCH CARRIER VERIFIED（2026-09-06）：** `NTSD28-B5-SPECIAL-HIT-LATCH-CARRIER-001 / VERIFIED / CARRIER_READY / BEHAVIOR_UNCONNECTED`。独立`SpecialHitLatch0EB`已闭合full reset/input-preserve/copy/snapshot/checksum/ECS hash/parity及Unity/C++ raw；schema11/17/20、raw49/43/6。focused5/raw3、related39、B5 520、NTSD28 1078、Parity21/5、C++ raw3ticks/6entities、18:25:08 SelfCheck、最终Console0、Scene unchanged/dirtyfalse/root13、Ledger327/282 PASS。下一独立owner audit；producer/consumer/HitPlan与旧`HitConfirm2`行为仍未改。

> **CURRENT — B5 REMAINING EXIT AUDIT 006 VERIFIED（2026-09-06）：** `NTSD28-B5-REMAINING-EXIT-AUDIT-006 / VERIFIED / GOVERNANCE_ONLY / SPECIAL_HIT_LATCH_LIFECYCLE_ROUTED / B5_EXIT_NOT_READY`。下一首差是独立`special_hit_latch_0eb`：Authority跨tick保持到实体生命周期结束，Unity复用`HitConfirm2`并在C25/C11清零；普通weapon又共享写`HitConfirm2`，故不能直接删clear。下一先补`SpecialHitLatch0EB` snapshot/checksum/parity/raw carrier，再owner audit与actual/HitPlan/runner atomic迁移。无code/content/Scene/authority写入。

> **CURRENT — SCENEVIEW/HIERARCHY VISIBILITY FIX VERIFIED（2026-09-06）：** `BATTLE-SCENEVIEW-HIERARCHY-VISIBILITY-001 / VERIFIED / SCENEVIEW_ISOLATED / TRANSIENT_GO_HIERARCHY_VISIBLE`。正常战斗camera gate只允许exact Base world camera，真实Play world19/7且SceneView gate/lease false、pixels0；benchmark runner/presenter/render children/camera统一DontSave且Hierarchy可见，second red exact、green1、benchmark38，最终09:06:42Z SelfCheck PASS。非GameObject临时资源保持隐藏。Test Runner保存了既有dirty Scene状态，未回退用户内容；下一B5 audit006。

> **CURRENT — B5 WEAPON DURABILITY FAMILY EXIT READY（2026-09-06）：** `NTSD28-B5-WEAPON-DURABILITY-EXIT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / WEAPON_DURABILITY_ATTACKING_INJURY_EXIT_READY`。actual/HitPlan及全部type/priority/fallback/bdefend100/HP隔离复审无首差。下一remaining audit 006。

> **CURRENT — B5 WEAPON DURABILITY ATTACKING INJURY VERIFIED（2026-09-06）：** `NTSD28-B5-WEAPON-DURABILITY-ATTACKING-INJURY-PRODUCTION-001 / VERIFIED / WEAPON_DURABILITY_ATTACKING_INJURY_ALIGNED`。red7/10；focused10、HitPlan184、B5 515、Unity侧 `NTSD28` 自动回归1073、08:28:37Z SelfCheck、Console/Scene PASS。下一专项exit audit。

> **CURRENT — B5 WEAPON DURABILITY ATTACKING INJURY OWNER READY（2026-09-06）：** `NTSD28-B5-WEAPON-DURABILITY-ATTACKING-INJURY-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / ATOMIC_INTEGRATION_READY`。复用既有pure arithmetic；actual/HitPlan各一处写面。下一production。

> **CURRENT — B5 REMAINING EXIT AUDIT 005（2026-09-06）：** `NTSD28-B5-REMAINING-EXIT-AUDIT-005 / VERIFIED / GOVERNANCE_ONLY / WEAPON_DURABILITY_ATTACKING_INJURY_ROUTED / B5_EXIT_NOT_READY`。type1/2/4/6耐久actual/HitPlan仍扣raw injury，Authority扣native attacking injury；下一owner audit。

> **CURRENT — B5 KIND4 SPECIFIC FAMILY EXIT READY（2026-09-06）：** `NTSD28-B5-KIND4-EXIT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / KIND4_SPECIFIC_FAMILY_EXIT_READY`。三candidate模式、carrier、runtime/direct/HitPlan、heavy-held与damage attribution/decrement均闭合；production无kind4/WeaponCount gate。下一remaining audit 005；B6 producer排除。

> **CURRENT — B5 KIND4 DEAD WEAPONCOUNT SELECTION RETIRED（2026-09-06）：** `NTSD28-B5-KIND4-DEAD-WEAPONCOUNT-SELECTION-RETIREMENT-001 / VERIFIED / DEAD_WEAPONCOUNT_KIND4_SELECTION_RETIRED`。red1/31；focused31、B5 505、Unity侧 `NTSD28` 自动回归1063、08:08:03Z SelfCheck、Console/Scene PASS。下一kind4-specific exit audit。

> **CURRENT — B5 KIND4 ATOMIC PRODUCTION VERIFIED（2026-09-06）：** `NTSD28-B5-KIND4-ATOMIC-PRODUCTION-INTEGRATION-001 / VERIFIED / KIND4_ENVIRONMENT_CONSUMPTION_ALIGNED`。valid red8/10；focused30、HitPlan184、RoleAware67、B5 504、Unity侧 `NTSD28` 自动回归1062、07:47:21Z SelfCheck、Console/Scene PASS。下一kind4-specific exit audit；B6 producer不混入。

> **CURRENT — B5 KIND4 +92 CARRIER VERIFIED（2026-09-06）：** `NTSD28-B5-KIND4-SOURCE-COUNT-CARRIER-001 / VERIFIED / CARRIER_READY / BEHAVIOR_UNCONNECTED`。red13；focused5、related47、B5 474、Unity侧NTSD28回归939、06:26:19Z SelfCheck、Console/Scene/Ledger PASS。下一atomic behavior。

> **CURRENT — B5 KIND4 ENVIRONMENT OWNER AUDIT VERIFIED（2026-09-06）：** `NTSD28-B5-KIND4-ENVIRONMENT-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / CARRIER_AND_ATOMIC_SPLIT_DEFINED`。先补Kind4SourceCount92 carrier，再原子接candidate/actual/HitPlan/attribution/decrement；WeaponCount禁止复用，B6 producer独立。

> **CURRENT — B5 KIND4 ENVIRONMENT CHAIN ROUTED（2026-09-06）：** `NTSD28-B5-REMAINING-EXIT-AUDIT-004 / VERIFIED / GOVERNANCE_ONLY / KIND4_ENVIRONMENT_CONSUMPTION_ROUTED / B5_EXIT_NOT_READY`。EnvironmentState320已存在；WeaponCount误用与+92 producer/attribution/decrement缺失。下一owner audit。

> **CURRENT — B5 MULTI-BODY CANDIDATE FAMILY EXIT READY（2026-09-06）：** `NTSD28-B5-MULTI-BODY-CANDIDATE-EXIT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / MULTI_BODY_CANDIDATE_FAMILY_EXIT_READY`。该规则族无首差；其他B5继续。

> **CURRENT — B5 MULTI-BODY CANDIDATE PRODUCTION VERIFIED（2026-09-06）：** `NTSD28-B5-MULTI-BODY-CANDIDATE-PRODUCTION-001 / VERIFIED / MULTI_BODY_CANDIDATE_MULTIPLICITY_ALIGNED`。red7/8；focused8、RoleAware67、HitPlan184、B5 469、broad934、05:51:00Z SelfCheck、Console/Scene/Ledger PASS。下一exit audit。

> **CURRENT — B5 MULTI-BODY CANDIDATE OWNER AUDIT VERIFIED（2026-09-06）：** `NTSD28-B5-MULTI-BODY-CANDIDATE-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / PRODUCTION_SEAMS_FROZEN`。brute/loose与role-aware exact/fallback边界及唯一TryRecord owner已冻结；下一test-first production。

> **CURRENT — B5 MULTI-BODY CANDIDATE MULTIPLICITY ROUTED（2026-09-06）：** `NTSD28-B5-REMAINING-EXIT-AUDIT-003 / VERIFIED / GOVERNANCE_ONLY / MULTI_BODY_CANDIDATE_MULTIPLICITY_ROUTED / B5_EXIT_NOT_READY`。Authority每个重叠BDY逐个candidate；Unity brute/cached首重叠即返回。下一owner audit。

> **CURRENT — B5 CANDIDATE EFFECT/TYPE FAMILY EXIT READY（2026-09-06）：** `NTSD28-B5-CANDIDATE-EFFECT-TYPE-EXIT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / CANDIDATE_EFFECT_TYPE_SPECIFIC_FAMILY_EXIT_READY`。该规则族无首差；其余candidate/B5继续。

> **CURRENT — B5 CANDIDATE EFFECT/TYPE PRODUCTION FILTER VERIFIED（2026-09-06）：** `NTSD28-B5-CANDIDATE-EFFECT-TYPE-PRODUCTION-FILTER-001 / VERIFIED / CANDIDATE_AND_CONSUMER_FILTER_ALIGNED`。red6/15；focused15、HitPlan184、B5 461、broad926、05:13:45Z SelfCheck、Console/Scene/Ledger PASS。下一exit audit。

> **CURRENT — B5 CANDIDATE EFFECT/TYPE PURE CORE VERIFIED（2026-09-06）：** `NTSD28-B5-CANDIDATE-EFFECT-TYPE-PURE-CORE-001 / VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`。矩阵与zero-allocation闭合；red10/26、focused26、B5 446、broad911、04:53:26Z SelfCheck、Console/Scene/Ledger PASS。下一candidate/runtime defensive production filter。

> **CURRENT — B5 CANDIDATE EFFECT/TYPE FILTER ROUTED（2026-09-06）：** `NTSD28-B5-REMAINING-EXIT-AUDIT-002 / VERIFIED / GOVERNANCE_ONLY / CANDIDATE_EFFECT_TYPE_FILTER_ROUTED / B5_EXIT_NOT_READY`。下一首差为candidate几何前effect13..16 target-type过滤；下一先建独立pure core，不能复用晚阶段action-override矩阵。

> **CURRENT — B5 KIND8 FAMILY EXIT READY（2026-09-06）：** `NTSD28-B5-KIND8-EXIT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / KIND8_SPECIFIC_FAMILY_EXIT_READY`。candidate/runtime defensive/shared四壳actual/HitPlan无kind8-specific首差；其他B5 family仍继续。

> **CURRENT — B5 KIND8 CONTROL RELATION VERIFIED（2026-09-06）：** `NTSD28-B5-KIND8-ATOMIC-PRODUCTION-INTEGRATION-001 / VERIFIED / KIND8_CONTROL_RELATION_ALIGNED`。candidate/consumer selector、shared runner唯一actual writer与HitPlan完整事务已闭合；red14、focused14、HitPlan184、collision258、B5 420、clean broad885、04:33:16Z SelfCheck、Console/Scene/Ledger PASS。下一kind8 exit audit。

> **CURRENT — B5 KIND8 ATOMIC INTEGRATION READY（2026-09-06）：** `NTSD28-B5-KIND8-PRODUCTION-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / ATOMIC_INTEGRATION_READY`。下一包原子接candidate gate、shared runner唯一actual writer与HitPlan；字段已齐，group/owner/mode=`RelationTeam/OwnerSlotIndex/BattleGameModeId`，int坐标不写。

> **CURRENT — B5 KIND8 ELIGIBILITY PURE CORE VERIFIED（2026-09-06）：** `NTSD28-B5-KIND8-ELIGIBILITY-PURE-CORE-001 / VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`。0..6 exact、7 weapon group、8 unrestricted及respond0..4真值表闭合；red至少25/35、focused35、B5 406、broad871、03:59:36Z SelfCheck、clean Console/Scene/Ledger PASS。下一production owner/write-surface audit；candidate/actual/HitPlan尚未接。

> **CURRENT — B5 REMAINING EXIT AUDIT ROUTED KIND8（2026-09-06）：** `NTSD28-B5-REMAINING-EXIT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / KIND8_FAMILY_ROUTED / B5_EXIT_NOT_READY`。下一独立首差为kind8 control relation：candidate错误硬限制type0，actual/HitPlan缺完整selector与side-effect transaction。下一先建eligibility pure core，再原子接candidate+consumer；跨阶段阻塞不混入。

> **CURRENT — B5 UNARMORED HP CONSUMPTION VERIFIED（2026-09-06）：** `NTSD28-B5-UNARMORED-HP-CONSUMPTION-PRODUCTION-001 / VERIFIED / UNARMORED_HP_CONSUMPTION_ALIGNED / FULL_RESOURCE_BLOCKED`。type0/1/2/3/4/5按effective HP damage累加`+0x34C`，type6跳过，actual+HitPlan闭合；red13/15、focused15、HitPlan184、B5 371、broad836、03:33:54Z SelfCheck、Console/Scene/diff/Ledger PASS。下一B5 remaining exit audit；完整MP resource仍受跨阶段依赖阻塞。

> **CURRENT — B5 HIT-RESOURCE PRODUCTION READINESS AUDIT 002 VERIFIED（2026-09-06）：** `NTSD28-B5-HIT-RESOURCE-PRODUCTION-READINESS-AUDIT-002 / VERIFIED / GOVERNANCE_ONLY / THREE_BLOCKERS_REMAIN / HP_CONSUMPTION_READY`。stats/type1 blockers已解除；完整MP transaction仍等baseMax H/B11、mode B8/H、child suppression B7/C25b，cpoint归B6。下一独立unarmored +0x34C production。

> **CURRENT — B5 TYPE1 ARMOR SPECIFIC FAMILY EXIT READY（2026-09-06）：** `NTSD28-B5-TYPE1-ARMOR-EXIT-AUDIT-002 / VERIFIED / GOVERNANCE_ONLY / TYPE1_ARMOR_SPECIFIC_FAMILY_EXIT_READY`。selection/match/activation/reduced/fallback/break/runtime recovery/actual+HitPlan已闭合，无第二production owner。cross-family resource/audio/spark/content仍阻止4.7完成；下一hit-resource production readiness audit 002。

> **CURRENT — B5 TYPE1 ARMOR BREAK/VERTICAL ORDER VERIFIED（2026-09-06）：** `NTSD28-B5-TYPE1-ARMOR-BREAK-VERTICAL-ORDER-001 / VERIFIED / BREAK_VERTICAL_POSTHIT_ORDER_ALIGNED`。broken fallback现按horizontal→break→vertical→attacker post-hit提交actual+HitPlan；red2/3、focused3、HitPlan184、B5 356、broad821、SelfCheck/Console/Scene/diff/Ledger PASS。下一exit audit 002。

> **CURRENT — B5 TYPE1 ARMOR EXIT AUDIT ROUTED BREAK ORDER（2026-09-06）：** `NTSD28-B5-TYPE1-ARMOR-EXIT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / BREAK_VERTICAL_ORDER_ROUTED`。actual在horizontal/break前可能先写vertical，HitPlan在break后先写attacker post-hit再写vertical；下一`NTSD28-B5-TYPE1-ARMOR-BREAK-VERTICAL-ORDER-001`。跨族resource/audio/spark/content仍独立。

> **CURRENT — B5 TYPE1 ARMOR ATOMIC PRODUCTION INTEGRATION VERIFIED（2026-09-06）：** `NTSD28-B5-TYPE1-ARMOR-ATOMIC-PRODUCTION-INTEGRATION-001 / VERIFIED / PRODUCTION_CONNECTED / TYPE1_ARMOR_CORE_TRANSACTION_ALIGNED`。defense优先、type1 match/activation、selected reduced、unarmored fallback与broken `-1→action→0`已原子接actual+HitPlan；最终focused10、B5 353、HitPlan184、broad818、SelfCheck/Console/Scene/diff/Ledger PASS。下一type1 armor exit audit；resource/audio/spark/content保持独立。

> **CURRENT — B5 TYPE1 ARMOR HITPLAN RUNTIME CARRIERS VERIFIED（2026-09-06）：** `NTSD28-B5-TYPE1-ARMOR-HITPLAN-RUNTIME-CARRIERS-001 / VERIFIED / HITPLAN_CARRIERS_READY / ARMOR_TRANSACTION_UNCONNECTED`。runtime armor HP与input HP/MP消费累计已进入writer-effect capture/compare；red4、focused4、B5 343、broad808、SelfCheck/Console/Scene/Ledger PASS。下一type1 armor atomic production integration；本包未接selection/damage/content/Scene。

> **CURRENT — B5 DEFINITION ATTACKING CARRIER VERIFIED（2026-09-06）：** `NTSD28-B5-DEFINITION-ATTACKING-CARRIER-001 / VERIFIED / DATA_CARRIER_READY / PRODUCTION_CONSUMPTION_UNCONNECTED`。已补`stats.attacking` typed field与正式converter；red4、focused4、B5 328、broad804、SelfCheck/Console/Scene/Ledger PASS。下一HitPlan runtime armor/consumption carriers；本包未接命中行为、content或Scene。

> **CURRENT — B5 TYPE1 ARMOR ATOMIC PRODUCTION AUDIT VERIFIED（2026-09-06）：** `NTSD28-B5-TYPE1-ARMOR-ATOMIC-PRODUCTION-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / PREREQUISITES_ROUTED`。actual+HitPlan布尔分流不足；先补definition attacking，再补HitPlan runtime armor/consumption carriers，最后原子接selection/activation/reduced/fallback/break。content仍gated。

> **CURRENT — B5 TYPE1 ARMOR RUNTIME PROFILE INTEGRATION VERIFIED（2026-09-06）：** `NTSD28-B5-TYPE1-ARMOR-RUNTIME-PROFILE-INTEGRATION-001 / VERIFIED / PRODUCTION_PROFILE_CONNECTED / HIT_SELECTION_UNCONNECTED`。首armor profile、ModuleBind出生/reuse、C25i生产读取与snapshot-skip已闭合；red7、focused8、B5 324、broad800、01:06:53Z SelfCheck、Console/Scene/Ledger PASS。下一atomic production audit；未接selection/damage/content。

> **CURRENT — B5 TYPE1 ARMOR RUNTIME INITIALIZATION AUDIT VERIFIED（2026-09-06）：** `NTSD28-B5-TYPE1-ARMOR-RUNTIME-INITIALIZATION-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / IMPLEMENTATION_SPLIT_DEFINED`。Authority出生/C25i均读首armor；Unity carrier/kernel存在但profile固定false、出生仍0/-1。下一runtime-profile integration；break tail留atomic hit，content继续gated。

> **CURRENT — B5 TYPE1 ARMOR ACTIVATION PURE CORE VERIFIED（2026-09-06）：** `NTSD28-B5-TYPE1-ARMOR-ACTIVATION-PURE-CORE-001 / VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`。MP两级64位成本、最低1、exact availability与runtime armor HP严格破甲`-1`已闭合；red11、focused12、B5 316、broad792、00:49:44Z SelfCheck、Console/Scene/Ledger PASS。下一runtime-initialization audit；未接production/content。

> **CURRENT — B5 TYPE1 ARMOR MATCH PURE CORE VERIFIED（2026-09-06）：** `NTSD28-B5-TYPE1-ARMOR-MATCH-PURE-CORE-001 / VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`。kind/facing/strict threshold/effect-id bypass/frame-state OR/2.8.3.3 invalid-state truth table已闭合；red24、focused25、B5 304、broad780、00:36:52Z SelfCheck、Console/Scene/Ledger PASS。下一activation pure core；未接production/content。

> **CURRENT — B5 TYPE1 ARMOR DATA CONTRACT VERIFIED（2026-09-06）：** `NTSD28-B5-TYPE1-ARMOR-DATA-CONTRACT-001 / VERIFIED / DATA_CONTRACT_READY / PRODUCTION_SELECTION_UNCONNECTED`。ArmorRecord28完整typed model、frame双整数parser、last-win/fallback converter、deep-copy/fingerprint与formal loader seam已闭合；red6、focused7、B5 279、broad755、00:19:49Z SelfCheck、Console/Scene/Ledger PASS。下一type1 armor match pure core；activation/content仍未接。

> **CURRENT — B5 ORDINARY-DEFENSE/NULL-ARMOR FAMILY EXIT READY（2026-09-06）：** `NTSD28-B5-ORDINARY-DEFENSE-REDUCED-HIT-EXIT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / NULL_ARMOR_FAMILY_EXIT_READY`。actual两入口、HitPlan consumers及damage/rest pure owner已闭合；旧selection启发式无production残留。下一type1 armor data contract；本审计无code/content/Scene修改。

> **CURRENT — B5 ORDINARY-DEFENSE/NULL-ARMOR REDUCED-HIT PRODUCTION VERIFIED（2026-09-06）：** `NTSD28-B5-ORDINARY-DEFENSE-REDUCED-HIT-PRODUCTION-INTEGRATION-001 / VERIFIED / PRODUCTION_CONNECTED / NULL_ARMOR_DEFENSE_ALIGNED`。actual selector/DamageWriter/HitPlan已原子接入current-state defense、raw `/10` + target scale及exact reduced rest；red3、focused3、B5 272、HitPlan184、NTSD28 broad748、23:53:08Z SelfCheck、filtered CS0、Scene/Ledger PASS。下一exit audit；type1 content仍受H/B11门约束。

> **CURRENT — B5 ITR DEFENSE FIELDS CARRIER VERIFIED（2026-09-06）：** `NTSD28-B5-ITR-DEFENSE-FIELDS-CARRIER-001 / VERIFIED / CARRIER_READY / BEHAVIOR_UNCONNECTED`。ITR `spark/dbdefend` typed parser、CopyFrom、HitPlan projection/fingerprint已闭合；red4、focused4、B5 269、HitPlan184、NTSD28 broad745、23:18:31Z SelfCheck、filtered CS0、Scene/Ledger PASS。下一ordinary-defense/reduced-hit原子生产接线。

> **CURRENT — B5 REDUCED-HIT DAMAGE PURE CORE VERIFIED（2026-09-06）：** `NTSD28-B5-REDUCED-HIT-DAMAGE-PURE-CORE-001 / VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`。selected-armor damage、HP/MP分流、runtime armor-HP delta与target +0x340缩放已闭合；red20、focused21、B5 265、HitPlan184、NTSD28 broad741、22:59:44Z SelfCheck、filtered CS0、Scene/Ledger PASS。下一ordinary-defense/reduced-hit原子生产接线。

> **CURRENT — B5 ORDINARY-DEFENSE PURE RESOLVER VERIFIED（2026-09-06）：** `NTSD28-B5-ORDINARY-DEFENSE-PURE-RESOLVER-001 / VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`。kind/effect、state7/70/75、HP、facing、spark/dbdefend/dvx与OID822真值表已闭合；red13、focused14、B5+HitPlan439、NTSD28 broad720、22:41:19Z SelfCheck、filtered CS0、Scene/Ledger PASS。下一reduced-hit damage pure。

> **CURRENT — B5 REDUCED-HIT REST PURE RESOLVER VERIFIED（2026-09-06）：** `NTSD28-B5-REDUCED-HIT-REST-PURE-RESOLVER-001 / VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`。default/effect/reduction、signed packed-delay hold、frame-counter flag与direct 4/12/native-byte rest已闭合；red13、focused14、B5+HitPlan425、NTSD28 broad706、22:27:22Z SelfCheck、filtered CS0、Scene/Ledger PASS。下一ordinary-defense pure。

> **CURRENT — B5 ARMOR/REDUCED-HIT AUDIT VERIFIED（2026-09-06）：** `NTSD28-B5-ARMOR-REDUCED-HIT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / IMPLEMENTATION_SPLIT_DEFINED / TYPE1_CONTENT_GATED`。formal armor18（type1-12/type0-6）vs Unity0；selection/damage/rest/HP/tail首差已拆包。下一reduced-hit-rest pure；type1正式content仍受H门约束。

> **CURRENT — B5 STANDARD-HIT REST FAMILY EXIT READY（2026-09-06）：** `NTSD28-B5-STANDARD-HIT-REST-EXIT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / STANDARD_REST_FAMILY_EXIT_READY`。Authority 2 caller、Unity actual 4→one adapter、HitPlan 6→one adapter已闭合；残留固定公式均属于OID300/kind9、alternate/armor或unreachable legacy tail。下一`NTSD28-B5-ARMOR-REDUCED-HIT-AUDIT-001`。

> **CURRENT — B5 STANDARD-HIT REST PRODUCTION INTEGRATION VERIFIED（2026-09-06）：** `NTSD28-B5-STANDARD-HIT-REST-PRODUCTION-INTEGRATION-001 / VERIFIED / PRODUCTION_CONNECTED / STANDARD_REST_ALIGNED`。character/weapon/special/other/initial pair actual与HitPlan六个standard投影已统一使用pure resolver；red7、focused7、B5+HitPlan411、NTSD28 broad692、22:03:59Z SelfCheck、filtered CS0、Scene/Ledger PASS。下一standard-rest exit audit。

> **CURRENT — B5 STANDARD-HIT REST PURE RESOLVER VERIFIED（2026-09-06）：** `NTSD28-B5-STANDARD-HIT-REST-PURE-RESOLVER-001 / VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`。recover/definition-effect/reduction/arest/uint8-vrest allocation-free truth table已闭合；red20、focused21、B5+HitPlan404、exact105/779、21:32:09Z SelfCheck、filtered CS0、Scene/Ledger PASS。下一接actual/HitPlan生产链。

> **CURRENT — B5 STANDARD-HIT REST DATA CARRIERS VERIFIED（2026-09-06）：** `NTSD28-B5-STANDARD-HIT-REST-DATA-CARRIERS-001 / VERIFIED / CARRIER_READY / BEHAVIOR_UNCONNECTED`。typed recover、definition bmp effect和world timing-reduction0..5已进入parser/copy/HitPlan fingerprint/reset/snapshot/restore/checksum/parity，schema8/15/18。red5；focused5、related74、B5+HitPlan383、exact104/758、21:17:06Z SelfCheck、filtered CS0、Scene/Ledger PASS。下一pure resolver；无selection UI/行为接线。

> **CURRENT — B5 STANDARD-HIT REST/RECOVER AUDIT VERIFIED（2026-09-06）：** `NTSD28-B5-STANDARD-HIT-REST-RECOVER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / CARRIER_AND_RESOLVER_SPLIT_DEFINED`。Unity缺ITR recover、definition effect与world timing-reduction三个typed carrier；正式405 DAT与Unity138 DAT前两者当前均0，default reduction0下现有rest值无首差，nondefault1..5会改变行为。下一`NTSD28-B5-STANDARD-HIT-REST-DATA-CARRIERS-001`；不实现排除的selection UI。

> **CURRENT — B5 TYPE3-SPECIFIC FAMILY EXIT READY（2026-09-06）：** `NTSD28-B5-TYPE3-POST-HIT-EXIT-AUDIT-003 / VERIFIED / GOVERNANCE_ONLY / TYPE3_SPECIFIC_FAMILY_EXIT_READY`。candidate、initial early branch、attacker、generic/transform、late pair/hold和effect handoff已无specific首差；common rest→B5/F11、audio/spark→B10、relation→B6、content→H。下一`NTSD28-B5-STANDARD-HIT-REST-RECOVER-AUDIT-001`。

> **CURRENT — B5 TYPE3 MATCHED-PAIR EARLY BRANCH VERIFIED（2026-09-06）：** `NTSD28-B5-TYPE3-MATCHED-PAIR-EARLY-BRANCH-001 / VERIFIED / CLOSED`。initial matching3005/3006现于damage/audio/status/hit-record前只写rests+pair reset+hold并return；HitPlan同步。red3/1；focused4、HitPlan183、B5+HitPlan378、exact103/753、20:40:26Z SelfCheck、filtered CS0、Scene/Ledger PASS。下一第三次type3 exit audit；全局standard-rest数值仍归B5/F11。

> **CURRENT — B5 TYPE3 RE-EXIT AUDIT VERIFIED, FAMILY NOT CLOSED（2026-09-06）：** `NTSD28-B5-TYPE3-POST-HIT-EXIT-AUDIT-002 / VERIFIED / GOVERNANCE_ONLY / EARLY_BRANCH_DIFFERENCE_ROUTED`。正式`battle_world.cpp:6615-6651`对non-character matching 3005/3006在damage/audio/status/hit-record前只写rests+pair reset+hold并return；Unity先完成伤害再尾部reset，产生可观察首差。下一`NTSD28-B5-TYPE3-MATCHED-PAIR-EARLY-BRANCH-001`；审计无C#/content/Scene写入。

> **CURRENT — B5 TYPE3 PAIR/HOLD/EFFECT-TAIL VERIFIED（2026-09-06）：** `NTSD28-B5-TYPE3-PAIR-RESET-HOLD-EFFECT-TAIL-001 / VERIFIED / CLOSED`。matching pair逐实体读action-latch帧`hit_Uj`（0→20）且仅清pending impulse；hold release无条件后置；legacy type3 effect5000/6000/23退役并同步HitPlan。red6/1；focused7、HitPlan183、B5+HitPlan374、exact102/749、20:14:00Z SelfCheck、filtered CS0、Scene/Ledger PASS。下一`NTSD28-B5-TYPE3-POST-HIT-EXIT-AUDIT-002`。

> **CURRENT — B5 TYPE3 POST-HIT EXIT AUDIT VERIFIED, FAMILY NOT CLOSED（2026-09-06）：** `NTSD28-B5-TYPE3-POST-HIT-EXIT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / REMAINING_TAIL_DIFFERENCES_ROUTED`。新确认matching pair需读双方latch-frame hit_Uj且只清pending total、motion-hold release需无条件执行、Unity legacy type3 5000/6000/23 tail无权威分支。下一`NTSD28-B5-TYPE3-PAIR-RESET-HOLD-EFFECT-TAIL-001`。

> **CURRENT — B5 TYPE3 KIND-CATALOG TRANSFORM VERIFIED（2026-09-06）：** `NTSD28-B5-TYPE3-KIND-CATALOG-TRANSFORM-001 / VERIFIED / TYPE3_LOCKED_KIND_TRANSFORM_ALIGNED`。3 bound×7 respond、direct attacker definition/id/type/group/owner、action/latch/Prev40、pending-total-only与transferred-definition effect projection已闭合；active209扫描退役。red4/7+effect red1→focused190、B5+HitPlan367、exact101/742、19:31:26Z SelfCheck、filtered CS0、Scene/Ledger PASS。下一`NTSD28-B5-TYPE3-POST-HIT-EXIT-AUDIT-001`。

> **CURRENT — B5 TYPE3 KIND-CATALOG TRANSFORM AUDIT VERIFIED（2026-09-06）：** `NTSD28-B5-TYPE3-KIND-CATALOG-TRANSFORM-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / IMPLEMENTATION_SEAM_DEFINED`。正式kind.dat SHA39E30DF8、唯一effect209/frame40、bound3/respond7与playable加载链已闭合；candidate gate等价，transform确认错误依赖active209并错写identity/owner/history/motion/weapon count。carrier已齐，无需部署kind.dat。下一`NTSD28-B5-TYPE3-KIND-CATALOG-TRANSFORM-001`。

> **CURRENT — B5 TYPE3 TARGET GENERIC CONTINUATION VERIFIED（2026-09-06）：** `NTSD28-B5-TYPE3-TARGET-GENERIC-CONTINUATION-001 / VERIFIED / TYPE3_TARGET_GENERIC_CONTINUATION_ALIGNED`。非kind candidate的state3005 skip、direct/active-parent ownership、仅清pending total、hit_Fj/Uj与hit-plan TargetOwnerSlot已闭合；red7/11→focused11、HitPlan182、B5+HitPlan359、exact100/735、18:29:12Z SelfCheck、filtered CS0、Ledger PASS、Scene unchanged。下一`NTSD28-B5-TYPE3-KIND-CATALOG-TRANSFORM-AUDIT-001`。

> **CURRENT — B5 TYPE3 TARGET PREREQUISITE AUDIT VERIFIED（2026-09-06）：** `NTSD28-B5-TYPE3-TARGET-CONTINUATION-PREREQUISITE-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / RUNTIME_CARRIERS_READY`。OwnerSlotIndex/AnimCounter/RelationTeam/HitConfirm2及KnockbackXYZ+HitCount carrier均已存在；缺口是生产事务和hit-plan TargetOwnerSlot。下一`NTSD28-B5-TYPE3-TARGET-GENERIC-CONTINUATION-001`，kind catalog后置。

> **CURRENT — B5 TYPE3 ATTACKER POST-HIT ACTION VERIFIED（2026-09-06）：** `NTSD28-B5-TYPE3-ATTACKER-POST-HIT-ACTION-001 / VERIFIED / TYPE3_ATTACKER_POST_HIT_ACTION_ALIGNED`。frame cover、state3000/state3007-cover、hit_Fj仅0回退10、selected dvx→Z及四条actual/hit-plan已闭合；red9→focused11、B5+hit-plan348、exact99/724、17:34:52Z SelfCheck、filtered CS0、Ledger PASS。下一`NTSD28-B5-TYPE3-TARGET-CONTINUATION-PREREQUISITE-AUDIT-001`。

> **CURRENT — B5 TYPE3 POST-HIT ACTION AUDIT VERIFIED（2026-09-06）：** `NTSD28-B5-TYPE3-POST-HIT-ACTION-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / IMPLEMENTATION_SPLIT_DEFINED`。攻击者固定10、错误dvz/漏Z、缺state3007 cover；target固定20/30、owner/control/impulse事务及kind catalog均确认不等价。Authority corpus state3000=902、同帧hit_Fj=187。下一先实施`NTSD28-B5-TYPE3-ATTACKER-POST-HIT-ACTION-001`。

> **CURRENT — B5 KIND0 DIRECT POST-EFFECT ACTION VERIFIED（2026-09-06）：** `NTSD28-B5-KIND0-DIRECT-POST-EFFECT-ACTION-001 / VERIFIED / KIND0_DIRECT_POST_EFFECT_ACTION_ALIGNED`。3/30→200、2/21/22/合格20→203、previous-state gate、final-X facing及actual/hit-plan已闭合；red10/6→focused16、related336、clean exact98/713、16:25:18Z SelfCheck、Console0、Ledger PASS。下一type3 post-hit action审计。

> **PREVIOUS — B5 REMAINING FALLDAMAGEDIV AUDIT VERIFIED（2026-09-05）：** `NTSD28-B5-REMAINING-FALLDAMAGEDIV-CONSUMER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / REMAINING_OWNERS_ROUTED`。alternate→armor/H、cpoint→B6、landing/recovery→B4/B7、results→B8；另确认Unity itr.kind16伤害/旋风无2.8 authority分支，effect16是另一字段。

> **PREVIOUS — B5 NONCHAR UNARMORED DAMAGE SCALE VERIFIED（2026-09-05）：** `NTSD28-B5-NONCHAR-UNARMORED-DAMAGE-SCALE-CONSUMER-001 / VERIFIED / TYPE1_5_UNARMORED_SCALE_WEAK_ALIGNED / TYPE6_SKIP_VERIFIED`。weapon type1/2/4及type3/5 normal、hit-plan projection已迁`+340 -> weak/2`；raw durability/display与type6 skip保持。red5→focused6、related290、精确NTSD28 broad670、14:37:47Z SelfCheck，Console/Scene/Ledger通过。

> **PREVIOUS — B5 NONCHAR DAMAGE-SCALE OWNER AUDIT VERIFIED（2026-09-05）：** `NTSD28-B5-NONCHAR-DAMAGE-SCALE-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / NONCHAR_MIGRATION_SEAM_DEFINED`。Authority只以`IncomingDamageScale340`作为伤害除数；Unity `FallDamageDiv`是Results等旧owner的独立遗留字段，不能双轨/双重缩放。其余consumer分流B4/B6/B8/B5。

> **PREVIOUS — B5 TYPE0 UNARMORED DAMAGE SCALE VERIFIED（2026-09-05）：** `NTSD28-B5-TYPE0-UNARMORED-DAMAGE-SCALE-CONSUMER-001 / VERIFIED / TYPE0_UNARMORED_SCALE_WEAK_ALIGNED`。32位`target +340 -> attacker weak/2`、effective HP/stat/KO与raw display分离闭合；red4→focused9、related284、精确NTSD28 broad664、14:14:56Z SelfCheck，Console/Scene/Ledger通过。

> **PREVIOUS — B5 DAMAGE-SCALE/EFFECT AUDIT VERIFIED（2026-09-05）：** `NTSD28-B5-DAMAGE-SCALE-EFFECT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / DAMAGE_AND_EFFECT_SPLIT_DEFINED`。Authority unarmored严格`target +340 -> attacker weak/2`，selected armor只+340；Unity三条standard damage owner均未按此消费。producer/armor/effect eligibility/post-action/override/audio/spark独立处理。

> **PREVIOUS — B5 RESOURCE PRODUCTION READINESS AUDIT VERIFIED（2026-09-05）：** `NTSD28-B5-RESOURCE-PRODUCTION-READINESS-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / PRODUCTION_DEPENDENCIES_ROUTED`。pure core/resolver/world rules/F6已ready；definition attacking/baseMax、mode override、child suppression、armor及cpoint六类依赖分别路由H/B11、B8/H、B7和B6，禁止局部接入伪造0值。

> **PREVIOUS — B5 F6 RESOURCE GATE PROJECTION VERIFIED（2026-09-05）：** `NTSD28-B5-F6-RESOURCE-GATE-PROJECTION-001 / VERIFIED / ACTIVE_AND_REGISTRATION_PROJECTION_ALIGNED`。accepted gate-change同tick occupied-slot投影、locked no-op、成功registration继承、entity/raw同步和zero allocation闭合；red3→focused5、clean related69+isolated1、精确NTSD28 broad655、13:49:24Z SelfCheck，Console/Scene/Ledger通过。

> **PREVIOUS — B5 WORLD HIT-RESOURCE RULES CARRIER VERIFIED（2026-09-05）：** `NTSD28-B5-WORLD-HIT-RESOURCE-RULES-CARRIER-001 / VERIFIED / CARRIER_READY / F6_PROJECTION_DEFERRED`。world numeric `1C/34/38`默认/reset、snapshot/restore、checksum/parity及schema7/14/17闭合；red24→focused16、related59、精确NTSD28 broad650、13:29:18Z SelfCheck，Console/Scene/Ledger通过。

> **PREVIOUS — B5 WORLD RESOURCE RULES/F6 AUDIT VERIFIED（2026-09-05）：** `NTSD28-B5-WORLD-RESOURCE-RULES-F6-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / WORLD_RULES_SPLIT_DEFINED`。FunctionKeys保持唯一local gate；1C/34/38建numeric world carrier；F6 active projection与registration inheritance独立接；selected-mode override归B8/H。

> **CURRENT — B5 RESOURCE-ATTACKER RESOLVER VERIFIED（2026-09-05）：** `NTSD28-B5-RESOURCE-ATTACKER-RESOLVER-001 / VERIFIED / RESOLVER_READY / PRODUCTION_UNCONNECTED`。active physical slot、negative/self terminal、两跳上限、missing-next fail-closed与stable/holder字段隔离闭合；red8→focused7、相关302、精确NTSD28 broad645、13:03:42Z SelfCheck；Scene unchanged。下一world rules/F6审计。

> **CURRENT — B5 HIT-RESOURCE SUPPRESSION CARRIER VERIFIED（2026-09-05）：** `NTSD28-B5-HIT-RESOURCE-SUPPRESSION-CARRIER-001 / VERIFIED / CARRIER_READY / PRODUCER_DEFERRED`。default/input/full reset、copy、snapshot、checksum、full parity及schema9/13/16闭合；red12→focused6、相关69、精确NTSD28 broad638、12:49:59Z SelfCheck；Scene unchanged。下一resource-attacker resolver；child producer仍归B7/C25b。

> **CURRENT — B5 RESOURCE CARRIER/ATTRIBUTION AUDIT VERIFIED（2026-09-05）：** `NTSD28-B5-RESOURCE-CARRIER-ATTRIBUTION-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / CARRIER_ATTRIBUTION_SPLIT_DEFINED`。resource attacker固定`OwnerSlotIndex`两跳、missing next失败；suppression缺carrier；F6缺world transaction投影；stats.attacking/baseMax保持B11/H gate。下一suppression carrier。

> **CURRENT — B5 RESOURCE TRANSACTION PURE CORE VERIFIED（2026-09-05）：** `NTSD28-B5-RESOURCE-TRANSACTION-PURE-CORE-001 / VERIFIED / PURE_TRANSACTION_READY / PRODUCTION_UNCONNECTED`。local gate、type0 reward、suppression、drain/gain all-or-nothing与reward→drain→gain顺序闭合；red6→focused7、related256、精确NTSD28 broad632、12:26:10Z SelfCheck；Scene unchanged。下一carrier/attribution审计，production仍未接。

> **CURRENT — B5 RESOURCE INJURY PURE CORE VERIFIED（2026-09-05）：** `NTSD28-B5-RESOURCE-INJURY-PURE-CORE-001 / VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`。double、definition/mode优先、49/50 rounding、negative remainder与32位overflow闭合；red3→focused9、related249、精确NTSD28 broad625、12:12:35Z SelfCheck；Scene unchanged。production仍等待carrier；下一resource transaction pure core。

> **CURRENT — B5 HIT DISPLAY-STEP PRODUCER VERIFIED（2026-09-05）：** `NTSD28-B5-HIT-DISPLAY-STEP-PRODUCER-001 / VERIFIED / TYPE0_5_DISPLAY_STEPS_ALIGNED / TYPE6_SKIP_VERIFIED`。positive `/10,/10,/10,/20`、zero/negative preserve、type0..5 production与type6 skip闭合；red2→focused10、related255、精确NTSD28 broad616、12:00:34Z SelfCheck；Scene unchanged。下一resource injury pure core。

> **CURRENT — B5 HIT-RESOURCE PREREQUISITE AUDIT VERIFIED（2026-09-05）：** `NTSD28-B5-HIT-RESOURCE-PREREQUISITE-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / RESOURCE_SPLIT_DEFINED`。四display-step写入位于local/F6 gate前且carrier ready，可立即实施；resource rules/F6/attribution另包，armor/weapon-strength保持H/B11 gate，effect/damage scale另审。下一display-step producer。

> **CURRENT — B5 NONTYPE0 HIT-MOTION ARM VERIFIED（2026-09-05）：** `NTSD28-B5-NONTYPE0-HIT-MOTION-ARM-001 / VERIFIED / TYPE1_6_HIT_MOTION_ARM_ALIGNED`。weapon/special/other均在reaction后、horizontal前接arm；type1..6 armed、type6 status-skip+arm及strict bypass闭合；red7/pass1→focused8、完整hit related241、精确NTSD28 broad606、11:45:00Z SelfCheck；Scene unchanged。producer family闭合，下一审计B5 resource/armor/effect剩余链。

> **CURRENT — B5 NONTYPE0 ENCODED+JOIN VERIFIED（2026-09-05）：** `NTSD28-B5-NONTYPE0-ENCODED-JOIN-001 / VERIFIED / TYPE1_5_ENCODED_JOIN_ALIGNED / TYPE6_SKIP_VERIFIED`。type1/2/4 weapon与type3/5 special/other已接producer+join，non-type0 mimic不enable，type6保持zero-RNG/status/join；red11→focused11、完整hit related233、精确NTSD28 broad598、11:30:59Z SelfCheck；Scene unchanged。下一non-type0 arm。

> **CURRENT — B5 OTHER-TARGET PRODUCER AUDIT VERIFIED（2026-09-05）：** `NTSD28-B5-OTHER-TARGET-PRODUCER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / OTHER_TARGET_SPLIT_DEFINED`。type1/2/3/4/5执行encoded+join，type6跳过；type1..6全部执行arm。Unity weapon/special/other owners与两包拆分已闭合；下一non-type0 encoded+join，resource/armor/effect另包。

> **CURRENT — B5 TYPE0 JOIN/MIMIC SIDE EFFECTS VERIFIED（2026-09-05）：** `NTSD28-B5-TYPE0-JOIN-MIMIC-SIDE-EFFECTS-001 / VERIFIED / TYPE0_JOIN_MIMIC_IMMEDIATE_EFFECTS_ALIGNED / OTHER_TYPES_DEFERRED`。join gate/re-entry/original group与mimic type/counter/enabled/source slot、encoded→same-hit activation闭合；red5→focused10、related44、精确NTSD28 broad587、11:13:36Z SelfCheck；Scene unchanged。下一审计其他target type confirmed-hit生产链。

> **CURRENT — B5 TYPE0 HIT-MOTION ARM VERIFIED（2026-09-05）：** `NTSD28-B5-TYPE0-HIT-MOTION-ARM-001 / VERIFIED / TYPE0_HIT_MOTION_ARM_ALIGNED / OTHER_TYPES_DEFERRED`。strict bypass、-5/+5边界、pending-Z、facing/motion/gain/default/explicit action及production order闭合；red5→focused10、related34、精确NTSD28 broad577、11:00:36Z SelfCheck；Scene unchanged。下一type0 join/mimic immediate side effects；其他target type/B8后置。

> **CURRENT — B5 TYPE0 ENCODED STATUS VERIFIED（2026-09-05）：** `NTSD28-B5-TYPE0-ENCODED-STATUS-001 / VERIFIED / TYPE0_ENCODED_PRODUCER_ALIGNED / ARM_AND_OTHER_TYPES_DEFERRED`。red3→focused4、related216、精确NTSD28 broad567、10:42:11Z SelfCheck；Scene/Console/Ledger通过。下一type0 arm。

> **CURRENT — B5 ITR STATUS FIELDS VERIFIED（2026-09-05）：** `NTSD28-B5-ITR-STATUS-FIELDS-001 / VERIFIED / ITR_STATUS_CONTRACT_READY / PRODUCER_DEFERRED`。red53→focused5、related195、精确NTSD28 broad563、09:02:27Z SelfCheck；Scene/Console/Ledger通过。下一type0 encoded producer；content不改。

> **CURRENT — B5 STATUS PRODUCER AUDIT VERIFIED（2026-09-05）：** `NTSD28-B5-STATUS-PRODUCER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / PRODUCER_SPLIT_DEFINED`。两producer/13-field ITR/RNG order/type owner已闭合；Unity content0 vs authority92保持Direction B hold。下一ITR carrier。

> **CURRENT — STATE12/18 ENVIRONMENT CREDIT VERIFIED（2026-09-05）：** `NTSD28-B4-F04-STATE12-18-ENVIRONMENT-CREDIT-001 / VERIFIED / DAMAGE_CREDIT_COUNT_ALIGNED / B8_EVENT_DEFERRED`。red6/7→focused7、related71、精确NTSD28 broad558、08:45:39Z SelfCheck；Scene/Console/Ledger通过。B8 event/B5 producer后置。

> **CURRENT — STATE12/18 CONTACT ACTION VERIFIED（2026-09-05）：** `NTSD28-B4-F04-STATE12-18-CONTACT-ACTION-001 / VERIFIED / CONTACT_ACTION_SINGLE_OWNER / ENVIRONMENT_DAMAGE_DEFERRED`。red→final focused9、related64、精确NTSD28 broad551；旧WeaponCount夹具x4更正后08:32:06Z SelfCheck，Scene/Console/Ledger通过。下一environment damage/credit；B5/B8后置。

> **CURRENT — HARD-MOTION CONSUMER VERIFIED（2026-09-05）：** `NTSD28-B4-F04-HARD-MOTION-CONSUMER-001 / VERIFIED / PURE_KERNEL_READY / PRODUCTION_TRANSACTION_DEFERRED`。red7→focused15、related55、精确NTSD28 broad542、08:12:40Z SelfCheck；Scene/Console/Ledger通过。下一state12/18 contact transaction；B5/B8后置。

> **CURRENT — STATUS MOTION CARRIER VERIFIED（2026-09-05）：** `NTSD28-B4-F04-STATUS-MOTION-CARRIER-001 / VERIFIED / CARRIER_READY / PRODUCER_CONSUMER_DEFERRED`。red21→final focused6、related60、精确NTSD28 broad527、08:00:57Z SelfCheck；Scene/Console/Ledger通过。下一hard-motion consumer；raw 48-leaf exact schema、B5 producer与B8 event后置。

> **CURRENT — STATE12/18 TRANSACTION PREREQUISITE AUDIT VERIFIED（2026-09-05）：** `NTSD28-B4-F04-STATE12-18-TRANSACTION-PREREQUISITE-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / CARRIER_AND_OWNER_SPLIT_DEFINED`。缺7-field status carrier；env/credit已有、KO event sink缺失。下一carrier，B5 producer/B8 event后置。

> **CURRENT — STATE12/18 AIRBORNE VERIFIED（2026-09-05）：** `NTSD28-B4-F04-STATE12-18-AIRBORNE-001 / VERIFIED / SINGLE_AIRBORNE_SELECTOR / LANDING_TRANSACTION_PENDING`。compile-red→focused17、related99、broad521、07:26:33Z SelfCheck；Scene/Console/Ledger PASS。下一landing transaction prerequisite audit。

> **CURRENT — STATE12/18 AIRBORNE AUDIT VERIFIED（2026-09-05）：** `NTSD28-B4-F04-STATE12-18-AIRBORNE-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / SELECTOR_SEAM_DEFINED`。现有duplicate selector错读WeaponCount/tickIndex/absolute y；下一实现。

> **CURRENT — TYPE0 ORDINARY LANDING VERIFIED（2026-09-05）：** `NTSD28-B4-F04-TYPE0-ORDINARY-LANDING-001 / VERIFIED / TYPE0_ORDINARY_SINGLE_BODY / STATE12_18_PENDING`。red2/6→focused6、related77、final broad504；SelfCheck旧state13夹具更正后07:08:54Z PASS，Scene/Console/Ledger PASS。下一state12/18 airborne selector审计。

> **CURRENT — TYPE0 ACTION AUDIT VERIFIED（2026-09-05）：** `NTSD28-B4-F04-TYPE0-ACTION-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / TYPE0_SPLIT_DEFINED`。ordinary首差是hit_g遗漏/negative floor重写0；state12/18的upcoming phase、environment transaction、credit/KO与缺失status carriers已分流。下一ordinary single body。

> **CURRENT — TYPE3/OID999 REFERENCE VERIFIED（2026-09-05）：** `NTSD28-B4-F04-TYPE3-OID999-REFERENCE-001 / VERIFIED / HIT_G_AND_REMAINING_NONCHAR_REFERENCE / TYPE0_PRODUCERS_AUDIO_PENDING`。hit_g carrier补齐；behavior red6→focused7、related93、final broad498。SelfCheck两条旧y0夹具更正后06:44:53Z PASS；Scene/Console/Ledger PASS。下一type0 action审计。

> **CURRENT — TYPE3/OID999 AUDIT VERIFIED（2026-09-05）：** `NTSD28-B4-F04-TYPE3-OID999-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / REMAINING_NONCHAR_SEAM_DEFINED`。Unity旧OID999 y0/全停与正式+9 tail相反；普通type5仍漏reference core。下一实现，不改producer/type0/Audio。

> **CURRENT — DERIVED WEAPON REFERENCE VERIFIED（2026-09-05）：** `NTSD28-B4-F04-DERIVED-WEAPON-REFERENCE-001 / VERIFIED / DERIVED_TYPE1_2_4_6_REFERENCE / TYPE3_OID999_PENDING`。red5→focused5、related59、final broad491、06:21:49Z SelfCheck；Scene/Console/Ledger PASS。specialization/virtual seam/snapshot保留且无新跨tick字段；下一type3/OID999。

> **CURRENT — DERIVED WEAPON OWNER AUDIT VERIFIED（2026-09-05）：** `NTSD28-B4-F04-DERIVED-WEAPON-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / MIGRATION_SEAM_DEFINED`。正常pooled LF2Weapon仍走legacy bool/y0与absolute-Y gate；下一test-first迁result core并保留specialization/virtual seam/snapshot。type3/OID999/producer另包。

> **CURRENT — SHARED TYPE2/4/6 REFERENCE VERIFIED（2026-09-05）：** `NTSD28-B4-F04-SHARED-TYPE2-4-6-REFERENCE-001 / VERIFIED / SHARED_TYPE2_4_6_REFERENCE / DERIVED_AND_OID999_PENDING`。red6→focused6、related31、broad486、06:03:19Z SelfCheck；Scene/Console/Ledger PASS。下一derived physics owner审计；type3/OID999/producer仍待。

> **CURRENT — SHARED TYPE1 REFERENCE VERIFIED（2026-09-05）：** `NTSD28-B4-F04-SHARED-TYPE1-REFERENCE-001 / VERIFIED / SHARED_TYPE1_REFERENCE / DERIVED_AND_OTHER_TYPES_PENDING`。初次red因fixture fallback type5作废；更正后green2、focused4、related48、broad480、05:47:11Z SelfCheck，Scene/Console/Ledger PASS。下一shared type2/4/6；derived与OID999另包。

> **CURRENT — NONCHAR RESULT SEAM AUDIT VERIFIED（2026-09-05）：** `NTSD28-B4-F04-NONCHARACTER-RESULT-SEAM-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / SHARED_FIRST_SPLIT`。WeaponDynamics bool不足以承载contact/effective-floor；下一先迁shared path，derived/on-landed与OID999另包。

> **CURRENT — IDENTITY X EXTRAS VERIFIED（2026-09-05）：** `NTSD28-B4-F04-IDENTITY-X-EXTRAS-001 / VERIFIED / INDEPENDENT_IDENTITY_EXTRAS / FLOOR_LANDING_PENDING`。red3/6→focused6、related44、broad476、05:27:46Z SelfCheck；Scene/Console/Ledger PASS。下一non-character result seam审计。

> **CURRENT — NONCHAR PHYSICS AUDIT VERIFIED（2026-09-05）：** `NTSD28-B4-F04-NONCHARACTER-PHYSICS-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY`。non-character已拆为identity、result seam、type1/2/4/6、type3/OID999和owner retirement；下一identity小包。

> **CURRENT — TYPE0 PHYSICS CORE VERIFIED（2026-09-05）：** `NTSD28-B4-F04-TYPE0-PHYSICS-CORE-001 / VERIFIED / TYPE0_CORE / ACTIONS_AND_PRODUCERS_PENDING`。red3/5→focused5、related33、broad470；SelfCheck更正旧epsilon/contact-side夹具后05:16:05Z PASS；Scene/Console/Ledger PASS。下一non-character core审计。

> **CURRENT — TYPE0 PHYSICS CORE AUDIT VERIFIED（2026-09-05）：** `NTSD28-B4-F04-TYPE0-PHYSICS-CORE-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY`。零地面假设和缺strict crossing是当前首差，现有CharacterMechanics可作为独立consumer seam。

> **CURRENT — B4 F04 COLLISION-Y CARRIER VERIFIED（2026-09-05）：** `NTSD28-B4-F04-COLLISION-Y-CARRIER-001 / VERIFIED / CARRIER_READY / PRODUCERS_UNCONNECTED`。red14→focused6、related29、fixture3、tool21+5、final broad465、04:58:24Z SelfCheck；Scene/Console/Ledger PASS。schema7/11/14、raw42/6已闭合；下一physics core consumer审计，所有producer仍未接。

> **CURRENT — COLLISION-Y OWNER AUDIT VERIFIED（2026-09-05）：** `NTSD28-B4-F04-COLLISION-Y-CARRIER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / CARRIER_BOUNDARY_DEFINED`。Authority writer/readers与previous-tick C11→next-tick C06时序已闭合；Unity无等价字段。下一carrier-only；operation30/physics/teleport/next999/input/hit另包。

> **CURRENT — B4 F04 TYPE1 LANDING VERIFIED（2026-09-05）：** `NTSD28-B4-F04-TYPE1-LANDING-001 / VERIFIED / TYPE1_THRESHOLD_BRANCH / FULL_PHYSICS_PENDING`。red1/3→focused4、related33、final broad459、04:35:25Z SelfCheck；Scene/Console/Ledger PASS。普通state1002 finite impact现action70，只有strict超正式巨大threshold才action7。下一collision-Y carrier审计；完整physics/type0/2/3/4/6/Audio仍待。

> **CURRENT — B4 F04 PHYSICS OWNER AUDIT VERIFIED（2026-09-05）：** `NTSD28-B4-F04-PHYSICS-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / PHYSICS_SPLIT_DEFINED`。gate、XYZ、friction/floor、gravity、landing与sync矩阵已闭合；首个独立首差为type1/state1002的Unity 9.9阈值与正式巨大double不符。详见`docs/ai/MANIFESTS/NTSD28-B4-F04-PHYSICS-OWNER.md`；完整F04仍未对齐。

> **CURRENT — B4 F02 FRAME-MOTION KERNEL VERIFIED（2026-09-05）：** `NTSD28-B4-F02-FRAME-MOTION-KERNEL-001 / VERIFIED / PRODUCTION_NATIVE_KERNEL / STRICT_DEPTH_INTENT`。production C04 XYZ与strict XOR depth intent闭合，双按不写Vz；legacy direct不动。red1→focused5、related45、broad455、04:12:57Z SelfCheck、Scene/Console/Ledger PASS。下一F04 physics audit。

> **CURRENT — B4 FRAME-MOTION ENTRY FIRST DIFFERENCE VERIFIED（2026-09-05）：** `NTSD28-B4-ENTRY-FRAME-MOTION-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY`。XYZ主体匹配；首差为Authority双depth键none，而Unity cooldown tie-break仍写Vz。下一F02 single kernel；physics/revival/frame-step不混入。无code/content/Scene/Authority写入。

> **CURRENT — B3 PLACEMENT EXIT READY / FULL CLOSE DEFERRED（2026-09-05）：** `NTSD28-B3-EXIT-GATE-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY`。可进入B4，但C25后临时SerialTickAll仍有special state/death、snapshot/state9998 cleanup，global post-tail仍有F7/carrier cleanup；路由B4/B5/B7/B8/B10/B11/H，接管前不删除、不写B3 complete。下一B4 entry audit。无code/content/Scene/Authority写入。

> **CURRENT — C25M SURVIVOR COMMIT VERIFIED（2026-09-05）：** `NTSD28-B3-C25M-PREVIOUS-ACTION-COMMIT-001 / VERIFIED / SURVIVOR_COMMIT / TERMINAL_PATH_PENDING_B7`。C25l后commit；virtual tail以try/finally old-Prev context保留state13/200和所有覆写，C25p在snapshot前。red4→focused3；related69、broad450、03:58:35Z SelfCheck、Scene/Console/Ledger PASS。terminal path留B7/C25o；下一B3 exit gate audit。

> **CURRENT — C25M OVERRIDE AUDIT VERIFIED（2026-09-05）：** `NTSD28-B3-C25M-PREVIOUS-ACTION-OVERRIDE-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / TRANSIENT_CONTEXT_DEFINED`。生产LF2Character与探针virtual dispatch必须保留；下一以try/finally调用栈old-Prev上下文前移C25m，同时保持state13/200兼容branch旧值语义。terminal early exit留B7/C25o。无code/content/Scene/Authority写入。

> **CURRENT — C25L PROGRAMMATIC ORDER VERIFIED / RESOURCE RUNTIME PENDING（2026-09-05）：** `NTSD28-B3-C25L-STATE18-PARTICLE-OWNER-001 / RUNTIME_PENDING / PROGRAMMATIC_ORDER_VERIFIED / RESOURCE_SPAWN_PENDING`。state18/19 branch已在OPoint后、cleanup/C25m前；state13/200 virtual tail/N30保持。red4→focused4/related63；SelfCheck捕获并修复空任务extra flush，最终18、broad447、03:44:32Z SelfCheck、Scene/Console/Ledger PASS。正式OID999粒子/slot/RNG tuple Play待；下一C25m override audit。

> **CURRENT — C25K/M PREREQUISITE AUDIT VERIFIED（2026-09-05）：** `NTSD28-B3-C25K-M-PREREQUISITE-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / DEPENDENCY_ORDER_CORRECTED`。Authority terminal pending由C25g保留原始code写入；Unity缺carrier且部分999已规范化丢失，故C25k不可独立前移。C25m须在C25l消费旧Prev后提交。下一C25l/m；C25k并入B7 terminal producer/carrier/C25o包。无code/content/Scene/Authority写入。

> **CURRENT — C25P HEALING OWNER VERIFIED（2026-09-05）：** `NTSD28-B3-C25P-HEALING-OWNER-001 / VERIFIED / PER_SLOT_HEALING_OWNER / GLOBAL_DUPLICATE_REMOVED`。type0 living survivor现于dynamic slot tail推进encoded→ordinary→state1700；global post-tail不再重复治疗且保留F7/carrier/transient/snapshot。red3→focused6；related32+44、broad443、03:27:04Z SelfCheck；Scene unchanged/Console0/Ledger PASS。下一C25k/m old-Prev与terminal分类。

> **CURRENT — C25K-P OWNER AUDIT VERIFIED（2026-09-05）：** `NTSD28-B3-C25K-P-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / IMPLEMENTATION_SPLIT_DEFINED`。Authority同槽k→l→m→n→o→p及o消费跳p已闭合；Unity的early lifecycle、mixed transition、late Prev、缺weapon pieces/pending carrier和global healing已确认。下一先做C25p逐槽healing owner；k/m、l、n/o独立分包，n/o依赖B7/B10/B11/H。无code/content/Scene/Authority写入。

> **CURRENT — C25I ARMOR RECOVERY CORE VERIFIED（2026-09-05）：** `NTSD28-B3-C25I-ARMOR-RECOVERY-001 / VERIFIED / PROGRAMMATIC_CORE_AND_PLACEMENT / FORMAL_CONTENT_PENDING`。h→i→j、timer/reload/gate与默认false profile seam闭合；red8→focused5，一次C25h误早退14项被原测试捕获并修复，最终related96、broad437、11:05:02 SelfCheck；Scene unchanged/Console0。正式armor仍待B5+H/B11；下一C25k-p。

> **CURRENT — C25G COMMON FRAME BODY VERIFIED（2026-09-05）：** `NTSD28-B3-C25G-FRAME-BODY-001 / VERIFIED / DOWNSTREAM_BEHAVIOR_ROUTED`。exact/fallback共用单一core；production terminal state14 hold、type3 state3007与Unity-only heavy早退闭合。red4→focused4、related85、NTSD28 broad432、10:50:29 SelfCheck；Scene unchanged/Console0。剩余frame细节保留B4/B7/B10/B11/H；下一C25i。

> **CURRENT — C25G FRAME-BODY OWNER AUDIT VERIFIED（2026-09-05）：** `NTSD28-B3-C25G-FRAME-BODY-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY`。C25g已在同一live-slot的f→g→h结构中；不再移动/重复writer。terminal/type3/next999/cost/pending/audio等行为差异已分流B4/B7/B10/B11/H。下一C25i armor recovery owner审计；无code/asset/Scene/Authority写入。

> **CURRENT — AI RENDER-PHASE CONSUMERS VERIFIED（2026-09-05）：** `NTSD28-B3-AI-RENDER-PHASE-CONSUMERS-001 / VERIFIED / PHYSICAL_Y_PRESERVED`。target role/index、abnormal/C8、held blocker与synchronized state17已从Y迁到verified HitStop，真实高度Y保留；3组旧Y fixture闭合。red4→compile0；focused121、all-AI385、NTSD28 broad428、10:29:46 SelfCheck PASS；Scene unchanged/Console0。下一C25g/C25i。

> **CURRENT — AI RENDER-PHASE CONSUMER AUDIT VERIFIED（2026-09-05）：** `NTSD28-B3-AI-RENDER-PHASE-CONSUMER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY`。target role/index、abnormal、held blocker与synchronized state17的Y误读已闭合；HitStop carrier已有，真实物理高度Y明确保留。下一`NTSD28-B3-AI-RENDER-PHASE-CONSUMERS-001`按互异矩阵test-first；无code/asset/Scene/Authority写入。

> **CURRENT — C25F/H/J TIMER OWNERS VERIFIED（2026-09-05）：** `NTSD28-B3-C25F-H-J-TIMER-OWNERS-001 / VERIFIED / C25F-H-J-PRODUCTION-OWNERS / JOINT-RAW-RENDER-PHASE-EQUAL`。live-slot现为f refresh→g→h完整reaction/status/poison/cleanup→j rest；direct compatibility保留，legacy -0.5经caller审计不在post-C25 production serial。red13→compile0；focused15、adjacent29、broad425、related38、tool21+5、09:48:48 SelfCheck；Scene unchanged/not playing/Console0。下一AI render-phase consumer/C25g/C25i。

> **CURRENT — C25F-J STATE CARRIERS VERIFIED（2026-09-05）：** `NTSD28-B3-C25F-J-STATE-CARRIERS-001 / VERIFIED / STATE_CARRIERS / SNAPSHOT-CHECKSUM-CLOSED / ALGORITHMS-EXCLUDED`。12字段已进入reset/copy/snapshot6/full10/checksum13/parity；raw armor变为41/7并双端equal。red60→compile0；joint66、broad410、tool21+5、09:11:30 SelfCheck；Scene unchanged/not playing/Console0。下一C25f/h/j timer owners，content仍受H/B11 gate。

> **CURRENT — RENDER PHASE BINDING VERIFIED（2026-09-05）：** `NTSD28-B3-C25F-J-RENDER-PHASE-BINDING-001 / VERIFIED / RENDER-PHASE-HITSTOP-BINDING / JOINT-RAW-EQUAL / NO-RUNTIME-BEHAVIOR-CHANGE`。48-field raw以Y/phase互异3tick证明`render_phase_008 -> Runtime.HitStop`；6 pairs/288 occurrences中renderPhase全等。red1/3；tool21+5、focused15、broad406、08:53:55 SelfCheck；Scene unchanged/not playing/Console0。旧AI Y consumer和C25h显式owner仍待。

> **CURRENT — C25F-J FIELD/OWNER AUDIT VERIFIED（2026-09-05）：** `NTSD28-B3-C25F-J-FIELD-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / FIELD_OWNER_MATRIX_COMPLETE / NEXT_RENDER_PHASE_BINDING`。五段gate/order与Unity writer矩阵已闭合。关键发现：HitStop/render_phase与旧AI Y投影冲突；Bdefend恢复率错误；timer bank/positive-HP status/computer/armor缺owner或carrier；Authority armor18/poison28/delay16、Unity0。无code/content/Scene/Authority写入；下一render-phase trace binding。

> **CURRENT — C25C-E ENTITY CARRIERS VERIFIED（2026-09-05）：** `NTSD28-B3-C25C-E-ENTITY-CARRIERS-001 / VERIFIED / ENTITY-CARRIERS / SNAPSHOT-CHECKSUM-CLOSED / ALGORITHM-EXCLUDED`。21个C25c/e status/attribution与C25d value/step载体已进入reset/canonical copy/snapshot5/full9/checksum12和独立parity组。red100→compile0；focused4、snapshot/checksum29、NTSD28 broad405、08:16:42 SelfCheck PASS；Scene unchanged、not playing、Console0。这里只完成载体，C25c/d/e算法、B11 schema及B5/B8 producers仍待。

> **CURRENT — C25C-E CURRENT MP BINDING VERIFIED（2026-09-05）：** `NTSD28-B3-C25C-E-CURRENT-MP-BINDING-CORRECTION-001 / VERIFIED / CURRENT-MP-PP-BINDING / RAW-PROJECTION-CORRECTED / NO-RUNTIME-BEHAVIOR-CHANGE`。MP200/PP173红灯证明旧raw取错MP；contract/exporter现唯一读Health.PP/Runtime.PP，旧MP字段保留但无authority身份。tool build0/0、21/21、raw5/5、format；compile0、joint49、NTSD28 broad401、07:42:52 SelfCheck PASS；SHA1AE87A06，Scene unchanged、not playing、post-clear Console0。下一C25c-e runtime carriers。

> **CURRENT — B3 C25C-E INVENTORY VERIFIED（2026-09-05）：** `NTSD28-B3-C25C-E-RESOURCE-DISPLAY-INVENTORY-001 / VERIFIED / GOVERNANCE_ONLY / FIELD_MATRIX_COMPLETE / CURRENT-MP-CONFLICT-FOUND`。C25d 8个value/step carrier全缺，PpDisplay不可复用；Authority locked object DAT有max_mp158、cmp7、chp4而Unity schema无承载。发现硬前置：B0 parity/raw把current_mp映射Runtime.MP，正式action/damage/C06/C25b使用Health.PP，初值500掩盖冲突。下一用非零resource joint trace唯一纠正current MP，再加runtime carriers；B11 gate不变。无code/asset/Scene修改，validator PASS。

> **CURRENT — B3 C25A-B VERIFIED（2026-09-05）：** `NTSD28-B3-C25A-B-DEFINITION-CLONE-001 / VERIFIED / C25A-B-PRODUCTION / TARGETED-PLAY-PASS / B11-DEFINITION-STATS-PENDING`。production现仅执行state8000..8999的atomic definition/source-next切换，旧9995/4000/render-offset140链留direct compatibility；state9996五分身使用native synchronized 34个精确callsite，legacy RNG不推进，birth字段闭合。red5/6→compile0；focused11、related30、NTSD28 broad399、07:13:17 SelfCheck及Play PASS；Play OID7/action3/type3、native34/legacy0、5 clones，SHA15285774，Scene unchanged、Play exited、Console0。C25a definition stats留B11；下一C25c-e resource/display。

> **CURRENT — B3 C25 SKELETON VERIFIED（2026-09-05）：** `NTSD28-B3-C25-NESTED-TAIL-SKELETON-001 / VERIFIED / C25-SINGLE-PRODUCTION-ENTRY / COMPLETED-TICK-RENDER / TARGETED-PLAY-PASS / C25A-P-BEHAVIOR-PENDING`。normal tick现在为C24→dynamic late-slot C25→legacy serial→Stage/session tails/Results→Render；step-wait旧路径保持。red1/2→compile0；focused2、placement/W05/worker51、presentation14、NTSD28 broad123、06:38:37 SelfCheck及Play PASS。tick6 phase25～33 exact、publishedTick6、SHA8AE8AB88、Scene unchanged、Play exited、Console0。下一C25a-b definition/special clone；legacy serial、global post-tail和C25c-p仍待。

> **CURRENT — B3 C25 WRITER INVENTORY VERIFIED（2026-09-05）：** `NTSD28-B3-C25-WRITER-INVENTORY-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / C25A-P-WRITERS-MAPPED / NEXT-C25-SKELETON`。Authority C25a～p与Unity serial/late/post-tail/structural writer已逐项闭合。Unity已有dynamic cursor和高slot同tick/低slot下tick证据，但三段global scan、OPoint前early lifecycle、loop-end mutation flush及Render-before-tail均为confirmed difference；C25d/f/i/n完整owner缺失。下一包先建立C24后C25 single production entry、live-slot skeleton与completed-tick presentation placement，再分A-B/C-E/F-J/K-P实现。详见`docs/ai/MANIFESTS/NTSD28-B3-C25-WRITER-INVENTORY.md`。

> **CURRENT — B3 C23/C24 WORLD CLOCK VERIFIED（2026-09-05）：** `NTSD28-B3-C23-C24-WORLD-CLOCK-001 / VERIFIED / C23-C24-SINGLE-OWNER / SNAPSHOT-CHECKSUM-CLOSED / TARGETED-PLAY-PASS / C25-PENDING`。Unity runtime自有phase12/phase3/sequence已在C22后/serial前single-owner提交并进入reset、snapshot6/8、checksum11、restore/parity；共享Server package与C25 consumers不改。red20→compile0；focused14、snapshot/checksum78、C04-C24/actual51、06:01:19 SelfCheck及Play PASS。真实tick6 5→6/2→0/5→6，SHA186F7F02、Scene unchanged、Play exited、Console0。full/partial34/4；next C25 nested tail inventory/skeleton。

> **CURRENT — B3 C21/C22 PLACEMENT VERIFIED（2026-09-05）：** `NTSD28-B3-C21-C22-PLACEMENT-001 / VERIFIED / C21-C22-PLACEMENT / TARGETED-PLAY-PASS / ALGORITHMS-PENDING-B4-B5-B8`。`PreFrameBounds`与`FramePostProcess`已移到current C20后/serial前；`CurrentWaveStage`及算法不改。red2→compile0；focused2、C04-C22/actual46、related21、05:28:56 SelfCheck与Play PASS。真实tick6 X-200→-100、Vx10、HitCount/Knockback0，phase23～27正确，SHA2E3A6570、cleanup、Scene unchanged、Play exited、Console0。full/partial32/4；next C23/C24/C25 owner。

> **CURRENT — B3 RESIDUAL SERIAL TAIL REHOME VERIFIED（2026-09-05）：** `NTSD28-B3-RESIDUAL-SERIAL-TAIL-REHOME-001 / VERIFIED / C14-C15-ADJACENCY / SERIAL-AFTER-C20 / TARGETED-PLAY-PASS / BODY-PRESERVED`。serial caller已从C14/C15之间原样后移到current C20后；C15例外及serial/type3/state9998本体不改。red2→compile0；focused2、C04-C14/actual44、related289、05:07:12 SelfCheck及Play PASS。真实tick6 hit3、RNG5→6/serial6，phase15/16/17/22/23正确，SHAE3A0B326、cleanup、Scene unchanged、Play exited、Console0。full/partial32/4；next C21/C22 owner vs temporary serial proxy，C17～C20 behavior仍归B6，C25 nested未实施。

> **CURRENT — B3 C14 TYPE-ZERO HIT PLACEMENT VERIFIED（2026-09-05）：** `NTSD28-B3-C14-TYPE0-HIT-PLACEMENT-001 / VERIFIED / C14-PLACEMENT / TARGETED-PLAY-PASS / HIT-ALGORITHM-PRESERVED`。type0 caller已在C13后/serial前；consumer不改。red2→compile0；focused2、C04-C14/actual42、hit related196、04:33:34 SelfCheck与Play PASS。真实tick6 serial/final hit count3，phase14～18=C13/C14/serial/drop/C16；SHA3695117E、cleanup、Scene unchanged、Play exited、Console0。旧W06断言已修正。full/partial32/4；next residual serial ownership before C15，C15用户例外本体不改。

> **CURRENT — B3 C13 ACTIVE WEAPON COUNT PLACEMENT VERIFIED（2026-09-05）：** `NTSD28-B3-C13-ACTIVE-WEAPON-COUNT-PLACEMENT-001 / VERIFIED / C13-PLACEMENT / EXACT-TYPE-SET / RANDOM-DROP-EXCEPTION-PRESERVED / TARGETED-PLAY-PASS`。C13现位于C12后/serial前并只计active current-DAT 1/2/4/6；用户随机掉武器例外正文/all-non-character门槛未改。red6→compile0；focused4、C04-C13/actual40、related13、4096=0B、04:11:29 SelfCheck与Play PASS。真实tick6 count4/captured tick6，SHA2D382636、cleanup、Scene unchanged、Play exited、Console0。full/partial32/4；next C14 type-zero hit consume vs serial remainder。

> **CURRENT — B3 C12 FUSION BARRIER PLACEMENT VERIFIED（2026-09-05）：** `NTSD28-B3-C12-FUSION-BARRIER-PLACEMENT-001 / VERIFIED / C12-PLACEMENT / TARGETED-PLAY-PASS / EXISTING-ALGORITHM-PRESERVED`。C12已固定在C11后、serial前；OID算法/C25h timer未改。red2→compile0；focused2、C04-C12/actual36、OID+C12 6、03:47:14 SelfCheck及Play PASS。真实tick6 route frame10→9/state2，C12融合OID7/8→51/frame290，serial看到51，timer4499、partner dormant；SHA FFAD6915、cleanup、Scene unchanged、Play exited、目标Play Console0。初次Play的missing frame9～12 fixture伪失败已修正并留档。full/partial31/4；next C13 active weapon count vs serial remainder。

> **CURRENT — B3 C11 CANDIDATE TRANSACTION PLACEMENT VERIFIED（2026-09-05）：** `NTSD28-B3-C11-CANDIDATE-BUILD-PLACEMENT-001 / VERIFIED / C11-PLACEMENT / TARGETED-PLAY-PASS / ALGORITHM-PENDING-B5`。rest→PairVRest→CandidateCollect已整体位于C10后、serial前。red2→compile0；focused2、C04-C11/actual34、related28、03:08:13 SelfCheck及Play PASS；serial rest0、pair visit5，SHA3C43ADC9、cleanup、Scene unchanged、Play exited、Console0。full/partial31/4，next C12。

> **HISTORICAL STEP — B3 C10 COLLISION-ACTION SNAPSHOT PLACEMENT VERIFIED（2026-09-05）：** `NTSD28-B3-C10-COLLISION-ACTION-SNAPSHOT-PLACEMENT-001 / VERIFIED / C10-SNAPSHOT-PLACEMENT / FOLLOWUP-C11-SUPERSEDED-TEMP-REST-BOUNDARY`。C10 snapshot-only位置与SHA D96A619A证据仍有效；关闭时rest暂留serial后的边界已由上方C11取代，当前为C10→C11→serial。

> **CURRENT — B3 C04 BASIC PRODUCTION OWNER VERIFIED（2026-09-05）：** `NTSD28-B3-C04-FRAME-MOTION-PRODUCTION-OWNER-001 / VERIFIED / BASIC-C04-OWNER / REAL-PLAY-PASS / FULL-C04-B4-PENDING / NEXT-C05`。dvx/dvy/dvz已从production C03/non-character SimTU抽到显式C04全slot唯一writer；direct兼容保留。red6、compile0、focused11、related211、00:43:10 SelfCheck、kind0 Play均PASS，result SHA5E379F2D，Scene unchanged、Play exited、Console0。full/partial=30/4；完整linked-platform/delay/dxyz留B4，下一C05 native teleport/combined EarlyFrameAdvance。

> **HISTORICAL STEP — B3 C04/C05/C06 OWNER AUDIT VERIFIED（2026-09-05）：** `NTSD28-B3-FRAME-MOTION-OWNER-AUDIT-001 / VERIFIED / C04-BASIC-IMPLEMENTED`。基础C04 owner差异已由上方包闭合；完整字段、C05与C06剩余项继续分包。

> **CURRENT — B3 OID51/52 PRODUCTION SPLIT VERIFIED（2026-09-05）：** `NTSD28-B3-OID5152-PRODUCTION-SPLIT-001 / VERIFIED / C12-FUSION / C25H-TIMER / REAL-PLAY-PASS / NEXT-FRAME-MOTION`。production fusion已在candidate后/hit前读取pre-decrement timer；C25h per-slot frame后递减`Unk338`，combined direct入口保持原语义。red4、compile0、focused10、related182、00:14:06 SelfCheck、4503-tick OID Play均PASS；merge4499、timer0 tick不split、下一C12 split后899/双方PP5，result SHA D003E910，Scene unchanged、Play exited、Console0。actual full/partial=29/4；下一首差FrameMotion/EarlyFrameAdvance。

> **HISTORICAL STEP — B3 OID51/52 MAINTENANCE AUDIT VERIFIED（2026-09-04）：** `NTSD28-B3-OID5152-MAINTENANCE-PLACEMENT-AUDIT-001 / VERIFIED / IMPLEMENTED-BY-NTSD28-B3-OID5152-PRODUCTION-SPLIT-001`。该审计确认C12/C25h边界；发现的combined maintenance差异已由上方production split闭合。

> **HISTORICAL STEP — B3 EARLY COOLDOWN EXTRACTION VERIFIED（2026-09-04）：** `NTSD28-B3-COOLDOWN-WRITER-EXTRACTION-001 / VERIFIED / C11-C25J-OWNERS / REAL-PLAY-PASS / NEXT-STEP-COMPLETED`。该包移除早期Cooldown并把rest writer归位；当时full/partial=29/5与RuntimeMaintenance首差，现已由上方OID production split继续推进为29/4与FrameMotion/EarlyFrameAdvance首差。

> **CURRENT — B3 C02/C03 PRODUCTION PLACEMENT VERIFIED（2026-09-04）：** `NTSD28-B3-C02-C03-PRODUCTION-PLACEMENT-001 / VERIFIED / REAL-PLAY-PASS / NEXT-FRAME-MOTION-VS-COOLDOWN`。Human poll、slot-interleaved non-character hit_Fa/character producer、第二遍proxy/route已移到Cooldown前，旧后续FrameLogic occurrence移除；direct CharacterInputAll不扩义。red1、compile0、focused8、related110、23:24:44 SelfCheck、Console0；真实kind0 Play tick3～6与C01/RNG/presentation/cleanup PASS，result SHAC52DE7F5，Scene SHA0D74E174/mtime不变，Play已退出。下一步拆Cooldown职责，不能整体搬动。

> **CURRENT — BUG修复版AUTHORITY晋升与B0-B3影响分类VERIFIED（2026-09-04）：** `GOVERNANCE-NTSD28-AUTHORITY-PROMOTION-002 / VERIFIED / USER_CONFIRMED / AUTHORITY-PROMOTED`。当前唯一正式EXE为`B1E13AE1...9033`，82-file playable closure`39DDDA15...6109`，75-file capture子闭包`07CD47A0...778F`；旧SHA只保留历史。capture build、3场景9 stream byte-equal、三validator、tool49、formal human/AI 900-tick同SHA、system.dat 33/3均通过；authority零写入。

> **CURRENT — B3 PRODUCER SCAN / COOLDOWN AUDIT RESUMED AS PASS REBASELINE（2026-09-04）：** `NTSD28-B3-PRODUCER-SCAN-COOLDOWN-BOUNDARY-AUDIT-001 / IN_PROGRESS / AUTHORITY-PROMOTED / READ_ONLY-REBASELINE`。新版core确认C18以后有真实顺序变化：impulse移到第二次clamp/refill/stage settlement后，display/reaction/armor/rest/opoint/healing进入逐slotnested tail。先修订manifest与immutable contract，再重新定位actual首差和Cooldown拆分；不沿用旧52项表直接改生产代码。

> **CURRENT — BUG修复版B3 PASS CONTRACT FOCUSED PASS（2026-09-04）：** `NTSD28-B3-BUGFIXED-PASS-CONTRACT-REBASELINE-001 / FOCUSED_TEST_PASS / 57-CHECKPOINT / PRODUCTION-UNCONNECTED`。旧52项contract已被57项取代；physics/dead-resource normalize为2步per-slot，hit为type0→drop→non-type0，stage后才impulse，slot tail为16步。red18、compile0、focused10、B3 related38、22:58:20 SelfCheck PASS、Console0。C00～C03不变，producer/Cooldown仍是下一production首差。

> **CURRENT — B3 NATIVE SPARK C01 INTEGRATION VERIFIED（2026-09-04）：** `NTSD28-B3-NATIVE-SPARK-C01-INTEGRATION-001 / VERIFIED / PRODUCTION-C01-SINGLE-WRITER / PRESENTATION-READ-ONLY / REAL-PLAY-PASS / NEXT-FIRST-DIFF-CORE-PRODUCER-SCAN-VS-COOLDOWN`。C01已在BattleFlow/input phase后、Cooldown前按slot推进；正式RenderDispatch/同步Host/worker/stress不再调用legacy logical writeback，只做capture/materialize/read-only acknowledgement。final focused38、related98、22:14:51 SelfCheck、Console0；实际kind0 Play tick3～6年龄`[0]→[1,0]→[2,1,0]→[3,2,1,0]`，commands1/2/3、Late只读、warm alloc0/0、cleanup和Scene SHA pre/post均PASS，result SHA397AD7D9。B9资源映射另阶段；当前继续B3 producer scan相对Cooldown的首差。

> **CURRENT — B3 NATIVE SPARK LIFECYCLE CORE FOCUSED PASS（2026-09-04）：** `NTSD28-B3-NATIVE-SPARK-LIFECYCLE-CORE-001 / FOCUSED_TEST_PASS / NATIVE-SPARK-CORE-READY / TERMINAL-TAIL-EXACT / ZERO-ALLOC / PRODUCTION-UNCONNECTED / NEXT-C01-INTEGRATION`。red14后末位9/non-tail retain/tail-only pop/0～98 increment/invalid keep闭合；compile0、focused16、related67、满10槽4096次0B、21:35:21 SelfCheck PASS、Console0。下一C01 production integration。

> **CURRENT — B3 SPARK ADVANCE BOUNDARY AUDIT VERIFIED（2026-09-04）：** `NTSD28-B3-SPARK-ADVANCE-BOUNDARY-AUDIT-001 / VERIFIED / PRESENTATION-DRIVEN-LIFECYCLE-CONFIRMED / EXISTING-CALL-NOT-MOVABLE / NEXT-NATIVE-SPARK-LIFECYCLE-CORE`。Authority为0～99 native cell、末位9 terminal/tail-only pop，new hit在C01后不进位；Unity current由U25 presentation/no-publication写逻辑，旧valid age范围不同且受资源可用性控制。不能直接搬现有方法；下一先建native lifecycle core。

> **CURRENT — B3 ACTUAL PHASE SEQUENCE BASELINE FOCUSED PASS（2026-09-04）：** `NTSD28-B3-ACTUAL-PHASE-SEQUENCE-BASELINE-001 / FOCUSED_TEST_PASS / ACTUAL-SEQUENCE-READY / FULL-30-PARTIAL-5 / FIRST-DIFF-SPARK-VS-COOLDOWN / ZERO-ALLOC / PRODUCTION-BEHAVIOR-UNCHANGED`。red18后fixed64 occurrence recorder已落地；真实完整tick30项、input-clear partial5项，首差为共同C00后`expected CoreSparkAdvance / actual Cooldown`。compile0、focused6、related90、4096 0B、21:24:11 SelfCheck PASS、Console0。下一spark boundary audit。

> **CURRENT — B3 PASS ORDER CONTRACT FOCUSED PASS（2026-09-04）：** `NTSD28-B3-PASS-ORDER-CONTRACT-001 / FOCUSED_TEST_PASS / IMMUTABLE-PASS-CONTRACT-READY / ZERO-ALLOC / PRODUCTION-UNCONNECTED / NEXT-ACTUAL-SEQUENCE-BASELINE`。red CS0246后52项ID/5 domains/8 traversals/7 flags已落地；C24 nested、double refill、dual-action、F-key pre/post、random-drop exception、completed snapshot均有断言。compile0、focused10、related42、4096 0B、21:17:21 SelfCheck PASS、Console0。production仍未切；下一actual-sequence red基线。

> **CURRENT — B3 PASS SKELETON ENTRY AUDIT VERIFIED（2026-09-04）：** `NTSD28-B3-PASS-SKELETON-ENTRY-AUDIT-001 / VERIFIED / ENTRY-AUDIT-CLOSED / ORDER-AND-BOUNDARY-DIFFERENCES-CONFIRMED / NEXT-PASS-ORDER-CONTRACT`。Authority G00～G17、C00～C30/C24a～h与Unity UH/U/UF实际顺序已manifest；global motion→teleport→physics barrier、collision action snapshot、逐slot nested tail及completed-tick presentation均确认结构差异。S11保留随机掉落例外；下一不可变pass contract。

> **CURRENT — B2 EXIT READY，下一步 B3 ENTRY AUDIT（2026-09-04）：** `NTSD28-B2-EXIT-GATE-AUDIT-001 / VERIFIED / B2-EXIT-READY / FUNCTION-KEY-PHYSICAL-PASS / DOWNSTREAM-OWNERS-ROUTED / NEXT-B3-ENTRY-AUDIT`。B2 input/proxy/combo/AI/dual-RNG基础、NONAI 50+2 owner crosswalk与F1～F12 route/carrier/production/real Play均闭合；AI tick3首差归B11，下游producer/effects按B3～B12继续，不误报为B2未完成。

> **CURRENT — B2 FUNCTION-KEY PHYSICAL PLAY PROBE VERIFIED（2026-09-04）：** `NTSD28-B2-FUNCTION-KEY-PHYSICAL-PLAY-PROBE-001 / VERIFIED / REAL-PLAY-PHYSICAL-F3-F12-PASS / SCENE-UNCHANGED / PRODUCTION-UNCHANGED`。真实NTSD_Battle、driver Running、Keyboard device 1、tick 0→5；F6、F7、F8+F9、F3+F6、plain F10、F11+F12 held/release、F4、Ctrl+F10九组PASS。result SHA `881C44CC...355E1`；Play已退出，Scene SHA `20984749...018B` 前后不变；20:50:48 SelfCheck PASS、Console0、Ledger157/115。

> **CURRENT — B2 FUNCTION-KEY PRODUCTION INTEGRATION VERIFIED（2026-09-04）：** `NTSD28-B2-FUNCTION-KEY-PRODUCTION-INTEGRATION-001 / VERIFIED / PRODUCTION-CONNECTED / SESSION-EXACTLY-ONCE / LEGACY-PHYSICAL-ISOLATED / REAL-PLAY-PHYSICAL-PASS / EFFECTS-DEFERRED-DOWNSTREAM`。red23；compile0、focused11/11、相关96/96、worker与4096 zero-alloc通过，随后真实Play九组physical InputSystem state-event验证通过。F4/F6～F12最终effects仍归B3/B5/B8/B10/B11。

> **CURRENT — B2 FUNCTION-KEY SESSION CARRIER FOCUSED PASS（2026-09-04）：** `NTSD28-B2-FUNCTION-KEY-SESSION-STATE-CARRIER-001 / FOCUSED_TEST_PASS / SESSION-CARRIER-READY / SNAPSHOT-CHECKSUM-RESTORE-READY / PRODUCTION-UNCONNECTED / NEXT-PRODUCTION-INTEGRATION`。red50；compile0、新9/9、相关65/65、4096 zero-alloc、20:19:23 SelfCheck PASS、Console0、Ledger155/110。下一production integration。

> **CURRENT — B2 FUNCTION-KEY PURE ROUTE FOCUSED PASS（2026-09-04）：** `NTSD28-B2-FUNCTION-KEY-ROUTE-CONTRACT-001 / FOCUSED_TEST_PASS / PURE-ROUTE-READY / PRODUCTION-UNCONNECTED / NEXT-SESSION-STATE-CARRIER`。red123；compile0、router8/8、相关19/19、4096 zero-alloc、20:01:21 SelfCheck PASS、Console0、Ledger154/106。physical/runtime/effects未接；下一Session carrier。

> **CURRENT — B2 FUNCTION-KEY CROSSWALK VERIFIED（2026-09-04）：** `NTSD28-B2-FUNCTION-KEY-ROUTE-CROSSWALK-001 / VERIFIED / FULL-F1-F12-CROSSWALK / IMPLEMENTATION-SPLIT-DEFINED / NEXT-ROUTE-CONTRACT`。F1～F12、route priority、Host/Session/continuous/maintenance、mask0xF4、fixed dispatch与same-window F9 wins已manifest。Unity旧F7全属性500不等于Authority current-MP-only；下一pure route contract，B2不退出。

> **CURRENT — B2 NON-AI RNG CALLSITE CROSSWALK VERIFIED（2026-09-04）：** `NTSD28-B2-NONAI-RNG-CALLSITE-CROSSWALK-001 / VERIFIED / FRESH-50-SYNC-PLUS-2-CRT-CLOSED / DOWNSTREAM-OWNERS-ROUTED / NEXT-FUNCTION-KEY-CROSSWALK`。BW32+IN2+TD1+GS15及CRT2已逐项manifest，旧43已纠正；Unity fresh87语法/86调用候选已分类。input/BGM已迁移，其余按B3～B8/B11/B12随single-owner行为迁移，不在B2机械切流。下一function-key crosswalk。

> **CURRENT — B2 EXIT GATE AUDIT VERIFIED（2026-09-04）：** `NTSD28-B2-EXIT-GATE-AUDIT-001 / VERIFIED / B2-EXIT-READY / FUNCTION-KEY-PHYSICAL-PASS / DOWNSTREAM-OWNERS-ROUTED / NEXT-B3-ENTRY-AUDIT`。B2职责及真实Play physical门均闭合；明确属于B3+的pass placement、producer、effects与B11内容首差不再作为B2 blocker。下一步先做B3入口审计。

> **CURRENT — B2 FORMAL EXE HEADLESS INPUT OBSERVATION VERIFIED（2026-09-04）：** `NTSD28-B2-FORMAL-EXE-HEADLESS-INPUT-OBSERVATION-001 / VERIFIED / FORMAL-EXE-HUMAN-AND-AI-HEADLESS-PASS / INPUT-RESET-OBSERVED / PER-CALL-RNG-NOT-EXPOSED`。SHA锁定根formal EXE的human/AI两次均exit0、900tick PASS；human双人七键mask127、动作/无输入对照与双方reset clean通过。报告SHA65D4FF97/2F845CBD；authority清单签名71275342前后不变。exact/per-call不在smoke schema。

> **CURRENT — B2 AI EXACT INPUT STORE ROUNDTRIP FOCUSED PASS（2026-09-04）：** `NTSD28-B2-AI-EXACT-INPUT-STORE-ROUNDTRIP-001 / FOCUSED_TEST_PASS / AI-TICKS1-2-EXACT-EQUAL / NEXT-FIRST-DIFFERENCE-B11-CONTENT-DOWNSTREAM-TICK3 / FORMAL_EXE_PENDING`。redc83e0f44后post-route AI runtime已回写canonical store；compile0、exporter11、broad200，common/standing equal，AI ticks1-2 exact+RNG equal；tick3 current首差由tick2 action650/9 B11内容分叉传导。19:21:55 SelfCheck、Console0。下一步formal EXE证据，仍处B2。

> **CURRENT — B2 AI HOST PENDING PROJECTION FOCUSED PASS（2026-09-04）：** `NTSD28-B2-AI-HOST-PENDING-PROJECTION-001 / FOCUSED_TEST_PASS / AI_HOST_PREVIOUS_READY / RUN_TRIGGER_READY / NEXT_FIRST_DIFFERENCE_AI_HISTORY_ROUNDTRIP / ACTION_CONTENT_B11 / FORMAL_EXE_PENDING`。red0b7d3ad5后compile0；exporter11、broad169，common/standing均3tick/6pairs equal，AI previous/run闭合且首差下移tick2 history[3] 4/-1；19:09:33 SelfCheck、Console0。action9/650归B11；下一包处理store roundtrip，仍处B2。

> **CURRENT — B2 AI NATIVE HISTORY BIND ORDER FOCUSED PASS（2026-09-04）：** `NTSD28-B2-AI-NATIVE-HISTORY-BIND-ORDER-001 / FOCUSED_TEST_PASS / NATIVE_HISTORY_PRE_BIND_READY / AI_TICK1_EXACT_EQUAL / NEXT_FIRST_DIFFERENCE_TICK2_PREVIOUS_MASK / FORMAL_EXE_PENDING`。redc9956771后成功registration改为init后Bind；compile0、exporter11、broad183，18:50:38 SelfCheck、Console0。common/standing equal；AI tick1 exact+RNG全equal，新首差tick2 previousMask0/2。下一包处理store/native roundtrip，仍处B2。

> **B2 FIRST-TICK INPUT/AI READINESS FOCUSED PASS（2026-09-04）：** `NTSD28-B2-FIRST-TICK-CHARACTER-INPUT-AI-READINESS-001 / FOCUSED_TEST_PASS / FIRST_TICK_READY / AI_NATIVE_RNG_JOINT_EQUAL / FIRST-DIFFERENCE-SUPERSEDED-BY-AI-NATIVE-HISTORY-BIND-ORDER / FORMAL_EXE_PENDING`。first-tick与RNG逐次已闭合；当时捕获的keyHistory首差也已由后续Bind-order包关闭。证据保留，当前首差为tick2 previousMask。

> **B2 ACCEPTED AI RNG TRACE HISTORICAL FIRST DIFFERENCE（2026-09-04）：** `NTSD28-B2-AI-ACCEPTED-RNG-PER-CALL-JOINT-TRACE-001 / FOCUSED_TEST_PASS / ACCEPTED-AI-TRACE-READY / FIRST-DIFFERENCE-SUPERSEDED-BY-FIRST-TICK-FIX / FORMAL-EXE-PENDING`。该包当时准确捕获AI首次eligible晚1tick；后续first-tick包已关闭该差异并证明AI RNG逐次equal。accepted-only observer证据保留，当前首差改为AI keyHistory。

> **CURRENT — B2 DIRECT RNG PER-CALL JOINT TRACE FOCUSED PASS（2026-09-04）：** `NTSD28-B2-DIRECT-RNG-PER-CALL-JOINT-TRACE-001 / FOCUSED_TEST_PASS / DIRECT-PER-CALL-V2-READY / INPUT-COMMON-STANDING-EQUAL / AI-FORMAL-PENDING`。red Unity5/.NET3后C++/.NET/Unity compile0；selftests5+5/21/12/6、Unity31+25、AI/lockstep86全PASS。input-common及standing-attack均3tick/6pairs equal，后者tick2精确site0x82/bound2/result1；18:08:40 SelfCheck、Console0。AI cursor/formal EXE另待。

> **CURRENT — B2 DEFEND RE-ENTRY EXACT REFRESH FOCUSED PASS（2026-09-04）：** `NTSD28-B2-DEFEND-REENTRY-EXACT-FRAME-REFRESH-001 / FOCUSED_TEST_PASS / EXACT-REFRESH-READY / INPUT-COMMON-JOINT-EQUAL / FORMAL-PER-CALL-PENDING`。red3后compile0、focused75/75；writer同步legacy+exact。input-common B2 exact input/native RNG 3tick/6pairs全equal、firstDifference null；17:47:38 SelfCheck、Console0。formal/per-call仍待。

> **CURRENT — B2 DIRECT-BATTLE RNG PRE-DRAW FOCUSED PASS（2026-09-04）：** `NTSD28-B2-NATIVE-RNG-DIRECT-BATTLE-BOOTSTRAP-001 / FOCUSED_TEST_PASS / DIRECT-BATTLE-RNG-CURSOR-READY / JOINT-FIRST-DIFFERENCE-CLOSED / AUDIO-SELECTION-B10`。red4后compile0；focused28/28+17/17，local/lockstep/diagnostic共享seed→0x004021E0事务。B2 joint initial双RNG全equal，首差下移tick2 slot1 defend cooldown3/0；17:38:34 SelfCheck、Console0。音频留B10，generic reset/restore与selection flow不动。

> **CURRENT — B2 EXACT INPUT/NATIVE DUAL RNG RAW FOCUSED PASS（2026-09-04）：** `NTSD28-B2-INPUT-RNG-JOINT-RAW-SCHEMA-001 / FOCUSED_TEST_PASS / INPUT-RNG-OBSERVABILITY-READY / REAL-FIRST-DIFFERENCE-CAPTURED / DIAGNOSTIC_ONLY`。red.NET12/Unity2/C++1后build0；selftest4+旧5/12/6、Unity9+相关40。双端valid3tick/6entity；首差sync counter1/0=Random BGM 0x004021E0，downstream exact仅defend cooldown3/0。SelfCheck/Console0；production未改。

> **CURRENT — B2 UNITY TRACE PHYSICAL BUTTON FIX FOCUSED PASS（2026-09-04）：** `NTSD28-B2-UNITY-TRACE-PHYSICAL-BUTTON-MAPPING-001 / FOCUSED_TEST_PASS / TRACE_TRANSLATION_CORRECTED / PRODUCTION_UNCHANGED / JOINT_TRACE_RERUN_PASS`。red3后compile0、final8/8、相关20/20；同场景joint action/state/counter全equal，37 equal/10 diff，首差转B11 baseMaxMp500/200，其余9 missing；16:56:26 SelfCheck、Console0、Ledger140/94。production输入不动。

> **CURRENT — B2 INPUT JOINT TRACE VERIFIED（2026-09-04）：** `NTSD28-B2-INPUT-JOINT-TRACE-001 / VERIFIED / B2-SCOPE-JOINT-CLOSED / HUMAN-SCENARIOS-EQUAL / AI-TICKS1-2-EXACT-AND-RNG-EQUAL / FORMAL-BEHAVIOR-PASS / DOWNSTREAM-FIRST-DIFFERENCE-B11`。common/standing全等；AI ticks1～2 exact+RNG闭合，tick3首差归B11 action/content。formal human/AI 900tick behavior与function-key real Play已补齐；formal per-call internals未暴露，因此不冒充该层证书。

> **CURRENT — B2 GROUND+AIR PRODUCTION CONNECTED（2026-09-04）：** `NTSD28-B2-NATIVE-TYPE0-BUILTINS-PRODUCTION-INTEGRATION-001 / FOCUSED_TEST_PASS / PRODUCTION_CONNECTED / RUNTIME_PENDING / JOINT_TRACE_PENDING`。native exact顺序及single-owner handoff已接；dead type0早退，Legacy profile保留。compile0；integration8、NTSD28 group167、CharacterInput37、worker/AI101、snapshot/checksum/ring31、SelfCheck均PASS，Console0，Ledger138/94。真实Play/joint trace待验；environment producer/raw与Config/DAT/Scene不动。

> **CURRENT — B2 NATIVE GROUND+AIR CORE READY（2026-09-04）：** `NTSD28-B2-NATIVE-TYPE0-AIR-DASH-REDIRECT-BUILTINS-001 / FOCUSED_TEST_PASS / GROUND_AIR_CORE_READY / PRODUCTION_UNCONNECTED`。action215、state4/5、state85/86、rowing core已完成；compile0，air10、ground+air22、NTSD28 group159、CharacterInput37、snapshot/checksum/ring31、SelfCheck均PASS，Console0，Ledger137/93。统一接线另包，Config/DAT/Scene与environment producer/raw不动。

> **CURRENT — B2 ENVIRONMENT STATE CARRIER READY（2026-09-04）：** `NTSD28-B2-ENVIRONMENT-STATE-CARRIER-001 / FOCUSED_TEST_PASS / CARRIER_READY / PRODUCERS_UNCONNECTED / AIR_CONSUMER_PENDING`。独立signed scalar与snapshot4/aggregate6/checksum9已完成；compile0，新5、carrier10、snapshot/checksum/ring31、NTSD28 group149、SelfCheck均PASS，Console0，Ledger136/92。raw仍missing，producer和air consumer另包。

> **CURRENT — B2 NATIVE TYPE0 GROUND BUILTINS FOCUSED PASS（2026-09-04）：** `NTSD28-B2-NATIVE-TYPE0-GROUND-BUILTINS-001 / FOCUSED_TEST_PASS / GROUND_CORE_READY / HIGH_FRAME_READY / PRODUCTION_UNCONNECTED / AIR_PENDING`。red37→10/11定位cache600；formal max856后cache857 exclusive，final12/12、B2 broad259/259。三处旧600 SelfCheck夹具逐项重基线后15:46:50 PASS，预期7日志清除后Console0、Ledger135/91。two-pass/resolver未改；下一步air/dash/redirect core。

> **CURRENT — B2 NATIVE TYPE0 BUILTIN DATA SEAMS FOCUSED PASS（2026-09-04）：** `NTSD28-B2-NATIVE-TYPE0-BUILTIN-DATA-SEAMS-001 / FOCUSED_TEST_PASS / DATA_SEAMS_READY / CONSUMERS_UNCONNECTED`。red37后，4组sequence与9个stats的model/parser/carrier/converter闭合；compile0、new6/6、parser/carrier20/20、B2 broad247/247、15:18:00 SelfCheck PASS、预期7日志清除后Console0、Ledger134/89。Config/DAT和runtime consumer未改；下一步ground built-ins，再做air/dash/redirect与joint trace。

> **CURRENT — B2 NATIVE DIRECT/HOLD/DIRECTION FOCUSED PASS（2026-09-04）：** `NTSD28-B2-NATIVE-DIRECT-HOLD-DIRECTION-ROUTING-001 / FOCUSED_TEST_PASS / DIRECT_HOLD_DIRECTION_READY / PRODUCTION_CONNECTED / BUILTINS_JOINT_TRACE_PENDING`。red31后，production按combo→three-button→direction→projection接线；field逐次重读current frame，保留失败仍消费、同tick多field、朝向重判、counter不清和depth不对称cleanup，DataOriented legacy owner为release-only。compile0；new12/12、input-owner37/37、B2-related241/241、snapshot/checksum32/32、worker/AI-shadow101/101、14:55:49 SelfCheck PASS、预期7日志清除后Console0、Ledger133/86。真实Play、type0 built-ins、sync82/83/84与joint trace仍待；B2未关闭。

> **B2 NATIVE COMBO ACTION TRANSACTION FOCUSED PASS（2026-09-04）：** `NTSD28-B2-NATIVE-COMBO-ACTION-TRANSACTION-001 / FOCUSED_TEST_PASS / COMBO_ACTION_TRANSACTION_READY / PRODUCTION_CONNECTED / JOINT_TRACE_PENDING`。red65；current remap/bound、exact combo production caller、generic lock/redirect/source-cost/resource/fallback/facing与`hit_ja`事务已闭合；DataOriented由native combo单一所有者处理，legacy mirror保留且direct/release继续执行。authority46/46 main closure与fresh binary PASS；gameplay Unity PID51752 compile0，focused18/18 job `1ad4...675e`、B2 broad261/261 `21fd...1661`、含B0 raw-capture超集267/267、snapshot31/31、14:31:34 SelfCheck PASS、7条预期日志清除后Console0、Ledger132/84。direct/hold/direction、type0 built-ins、sync82/83/84、跨阶段producer和joint trace另包；B2未关闭。

> **B2 NATIVE ACTION DATA CARRIERS FOCUSED PASS（2026-09-03）：** `NTSD28-B2-NATIVE-ACTION-DATA-CARRIERS-001 / FOCUSED_TEST_PASS / DATA_CARRIERS_READY / PRODUCTION_UNCONNECTED`。red128→compile0；runtime/frame/definition carrier、reset/deep-copy/fail-closed、schema3/5/8、逐字段checksum闭合。final10/10、B2 255/255、snapshot31/31、0B；13:55:53 SelfCheck PASS、Console0、Ledger131/80。Config/DAT/Scene未由本包改；action/timer/B3/B5/B7/B8 producer未接。

> **B2 NATIVE ACTION ROUTING CROSSWALK VERIFIED（2026-09-03）：** `NTSD28-B2-NATIVE-ACTION-ROUTING-CROSSWALK-001 / VERIFIED / SOURCE_CHAIN_CLOSED / IMPLEMENTATION_SPLIT_DEFINED / GOVERNANCE_ONLY`。正式build、46个由`main`实际调用的authority tests和step_sampled完整路由顺序已闭合；carrier addendum冻结20项type/default/producer owner及snapshot/checksum schema升级，明确B2只建carrier、B3/B5/B7/B8闭正式producer。Unity selector尚无production caller，缺hold/direction/action carriers、remap/jump-suppress与sync82/83/84。manifest拆为5个依赖包；初始44计数已按直接重计纠正，无source改动。

> **B2 NATIVE INPUT PRODUCER FOCUSED + REAL PLAY PASS（2026-09-03）：** `NTSD28-B2-NATIVE-INPUT-PRODUCER-MIGRATION-001 / FOCUSED_TEST_PASS / PRODUCTION_TWO_PASS_READY / REAL_PLAY_DDJ_DRA_PASS / JOINT_TRACE_PENDING`。DataOriented producer raw-only、proxy后native edge/history/combo once、history-1与dead clear闭合；final focused `e0c1...` 8/8、broad239/239、AI367/367、zero-alloc。最终DDJ exact1→2→3/Frame271/ticks2-4-15，DRA exact1→2→4/Frame263/ticks40-42-53，均1/1/1 attempts；batch自动退Play。13:19:28 SelfCheck PASS、Console0、Ledger130/77。中间probe red与DRA Frame274前置失败均留痕但不冒充production failure；action fields/joint trace仍待。

> **B2 NATIVE INPUT PRODUCER MIGRATION IN PROGRESS（2026-09-03）：** `NTSD28-B2-NATIVE-INPUT-PRODUCER-MIGRATION-001 / IN_PROGRESS / PRE_CODE / TEST_FIRST_PENDING`。将DataOriented human/AI producer改为raw-only，第二遍严格proxy→native edge/history/combo once→legacy projection；Legacy不变。exact combo action fields和joint trace另包。

> **B2 AI SYNC RNG PRODUCTION COMMIT FOCUSED PASS（2026-09-03）：** `NTSD28-B2-AI-SYNC-RNG-PRODUCTION-COMMIT-001 / FOCUSED_TEST_PASS / PRODUCTION_SYNC_COMMIT_READY / LEGACY_RNG_ISOLATED / JOINT_TRACE_PENDING`。production每AI同步cursor只由accepted writer唯一提交；same-generation stale拒绝，oracle/shadow/fallback丢弃，legacy RNG隔离，call-site进入严格比较。red5/81；final81/81、AI362/362、broad234/234、补强shadow69/69+native12/12、11:36:54 SelfCheck、Console0、Ledger128/76。下一包迁移native input producer；B2 joint trace仍待。

> **B2 AI RNG OWNER SELFCHECK SEAM VERIFIED（2026-09-03）：** `NTSD28-B2-SELFCHECK-AI-NATIVE-RNG-SEAM-001 / VERIFIED / FULL_SELFCHECK_PASS / TEST_ONLY`。11:29:43旧R3断言红灯后改为双seed/双owner合同；legacy推进旧RNG，IndexedCanonical保持旧RNG并推进NativeRandom。11:36:54 full PASS，production零改。

> **B2 VALIDATION FOUND AI RNG OWNER SELFCHECK SEAM（2026-09-03）：** `NTSD28-B2-SELFCHECK-AI-NATIVE-RNG-SEAM-001 / IN_PROGRESS / RED_SELF_CHECK_CAPTURED / TEST_ONLY`。11:29:43 full SelfCheck的target均为7，仅旧R3断言仍要求IndexedCanonical推进legacy RNG；当前权威路径应提交NativeRandom并保持legacy RNG不变。仅修fixture/owner断言，production零改。

> **B2 AI SYNC RNG PRODUCTION COMMIT IN PROGRESS（2026-09-03）：** `NTSD28-B2-AI-SYNC-RNG-PRODUCTION-COMMIT-001 / IN_PROGRESS / PRE_CODE / TEST_FIRST_PENDING`。将把NativeRandom cursor接到IndexedCanonical capture→witness→precommit→writer；只accepted commit一次，oracle/shadow/fallback不提交，legacy RNG不被sync witness覆盖，并补same-generation stale防护。

> **B2 AI HELD RNG FOCUSED PASS（2026-09-03）：** `NTSD28-B2-AI-HELD-RNG-001 / FOCUSED_TEST_PASS / HELD_SITES_28_35_READY / ALL_40_LIVE_IDS_READY / PRODUCTION_UNCONNECTED`。valid link-before-28、subject-group blocker、115/300阈值、weapon-run早停、state17 Y与combo index4/5完成；red1、19/19 job `feaa...b18a`、AI361/361 job `323e...9f87`、full `14→3C→1C→28` HeldDecision、4096 zero-alloc、10:56:19 SelfCheck、Console0、Ledger126/75。下一包接accepted canonical唯一cursor commit。

> **B2 AI HELD RNG IN PROGRESS（2026-09-03）：** `NTSD28-B2-AI-HELD-RNG-001 / IN_PROGRESS / PRE_CODE / TEST_FIRST_PENDING`。将闭合0x28..0x35、native return/early-stop、subject-group blocker、115/300阈值、state17 render phase和combo index4/5；legacy与production cursor不动。

> **B2 AI ORDINARY NON-HELD RNG FOCUSED PASS（2026-09-03）：** `NTSD28-B2-AI-ORDINARY-NONHELD-RNG-001 / FOCUSED_TEST_PASS / NONHELD_SITES_READY / SURPLUS_69_ISOLATED / PRODUCTION_UNCONNECTED`。静态BattleMode/cadence分离；动态1A/1C、1B/1D与1E..21、26/27、37..3B完成，sync模式已隔离全部69个无live ID表达式。red11、15/15 job `43f0...eb58`、AI342/342 job `a977...0826`、full Complete order、4096 zero-alloc、10:34:40 SelfCheck、Console0、Ledger125/74。下一包held0x28..35，之后才可接production cursor。

> **B2 AI ORDINARY NON-HELD RNG IN PROGRESS（2026-09-03）：** `NTSD28-B2-AI-ORDINARY-NONHELD-RNG-001 / IN_PROGRESS / PRE_CODE / TEST_FIRST_PENDING`。authority确认`battle_mode`为静态模式、不是cadence `InputPhase`；正式playable没有写`global_direction_lock_0049f608`，production值为默认0。将闭合动态1A/1C、1B/1D与1E..21、26/27、37..3B并隔离剩余4个prewrite surplus；held/production cursor另包。

> **B2 AI SPECIAL-PROFILE RNG FOCUSED PASS（2026-09-03）：** `NTSD28-B2-AI-SPECIAL-PROFILE-RNG-001 / FOCUSED_TEST_PASS / SITES_3C_6C_READY / SURPLUS_65_ISOLATED / PRODUCTION_UNCONNECTED`。candidate严格`0x3C→optional0x6C`，oid33置combo index2=hit_Ua；旧profile树65个surplus已隔离，legacy保持。red5、6/6、AI303/303、4096 zero-alloc、10:01:37 SelfCheck、Console0、Ledger124/72。

> **B2 STALE ROUTING VISIBILITY TEST CORRECTED（2026-09-03）：** `NTSD28-B2-TWO-PASS-ROUTING-VISIBILITY-TEST-001 / VERIFIED / TWO_PASS_EXPECTATION_CORRECTED / TEST_ONLY`。296/297唯一失败要求routing回流later producer，违反all-producer→all-routing；已更名并期望cached slot0。最终297/297、SelfCheck、Console0；production零改。

> **B2 AI PICKUP RNG FOCUSED PASS（2026-09-03）：** `NTSD28-B2-AI-PICKUP-RNG-001 / FOCUSED_TEST_PASS / PICKUP_SITES_16_17_READY / PRODUCTION_UNCONNECTED`。native1000/2004、legacy1004/2004与`0x16/0x17`闭合；red1、6/6、相关297/297、4096 zero-alloc、09:50:31 SelfCheck、Console0、Ledger123/71。完整selection/production另包。

> **B2 AI TARGET-PREFIX RNG FOCUSED PASS（2026-09-03）：** `NTSD28-B2-AI-TARGET-PREFIX-RNG-001 / FOCUSED_TEST_PASS / SITES_13_14_15_18_19_READY / PRODUCTION_UNCONNECTED`。`0x13/14/15/18/19`、cached/type顺序、state7短路、boundary/force隔离与abnormal二选一闭合；red7、final7/7、AI185/185、4096 zero-alloc、09:31:53 SelfCheck、Console0、Ledger121/68。production/pickup未接。

> **B2 SCRIPTED AI RNG TRANSACTION FOCUSED PASS（2026-09-03）：** `NTSD28-B2-AI-SCRIPTED-RNG-TRANSACTION-001 / FOCUSED_TEST_PASS / SCRIPTED_TRANSACTION_READY / PRODUCTION_UNCONNECTED`。snapshot/call-site trace→kernel candidate→witness→explicit owner commit已闭合；左右远距只消费`0x11/0x12`。red19、6/6、AI178/178、4096 zero-alloc、09:18:37 SelfCheck、Console0、Ledger120/67；production capture/commit未接。

> **B2 AI SYNCHRONIZED RNG STREAM SEAM FOCUSED PASS（2026-09-03）：** `NTSD28-B2-AI-SYNC-RNG-STREAM-SEAM-001 / FOCUSED_TEST_PASS / STREAM_SEAM_READY / PRODUCTION_UNCONNECTED`。显式site synchronized cursor、site/bound/raw/value trace、mode fail-closed与commit seam已实现；red34、7/7、18/18、18/18、AI172/172、5000 bit-exact、4096 zero-alloc、09:03:38 SelfCheck、Console0、Ledger119/64。旧CRT和production AI未切换。

> **B2 AI RNG LIVE-CALL CROSSWALK VERIFIED（2026-09-03）：** `NTSD28-B2-AI-RNG-CALLSITE-CROSSWALK-001 / VERIFIED / LIVE_CLOSURE_CLOSED / IMPLEMENTATION_SPLIT_DEFINED / GOVERNANCE_ONLY`。authority42文本表达式排除4个nonlive后为38 live expressions/40 possible IDs；Unity canonical为107 expressions，surplus69不得伪造ID。legacy三按钮字段按`KeyJump=attack/KeyDefend=jump/KeyAttack=defend`交叉映射；真实差异为消费/阈值/combo index、缺force-attack/use_ai carrier及`0x26/27`重复，production未改。

> **B2 AI SYNCHRONIZED RNG CURSOR FOCUSED PASS（2026-09-03）：** `NTSD28-B2-AI-SYNC-RNG-CURSOR-001 / FOCUSED_TEST_PASS / SYNCHRONIZED_CURSOR_READY / CONSUMERS_UNMIGRATED`。共享3001-byte table、独立scalar cursor与generation commit gate已实现；11/11+16/16、5000 bit-exact、4096 zero-alloc、08:35:12 SelfCheck、Console0。generation不进snapshot/checksum，AI call-sites未接。

> **B2 NATIVE COMBO ROUTER FIELDS FOCUSED PASS（2026-09-03）：** `NTSD28-B2-NATIVE-COMBO-ROUTER-FIELDS-001 / FOCUSED_TEST_PASS / FRAME_FIELDS_AND_SELECTOR_READY / PRODUCTION_UNCONNECTED`。`hit_aj/ad/jd` frame/parser、pure combo10 fixed-priority selector和explicit attempt consume已实现；test-first13、final17/17、related42+1skip、08:21:06 SelfCheck、Console0、zero-alloc。Config未改；special/action/producer迁移另包。

> **B2 NATIVE COMBO PRODUCTION CROSSWALK VERIFIED（2026-09-03）：** `NTSD28-B2-NATIVE-COMBO-PRODUCTION-CROSSWALK-001 / VERIFIED / CROSSWALK_CLOSED / IMPLEMENTATION_SPLIT_DEFINED / GOVERNANCE_ONLY`。2.8在第一遍producer/sample/record之后，于第二遍proxy copy后才推进edge/history/combo10并route；Unity human/AI仍在producer提前推进旧edge/combo。Unity缺`hit_aj/ad/jd`字段/解析，权威1110/1089/99处而Unity Config全0。已拆router-fields→producer migration→RNG call-sites→joint trace；只读无production改动。

> **B2 PRODUCTION INPUT TWO-PASS FOCUSED+PLAY PASS（2026-09-03）：** `NTSD28-B2-AI-SAMPLE-PROXY-TWO-PASS-001 / FOCUSED_TEST_PASS / PRODUCTION_TWO_PASS_READY / REAL_PLAY_INPUT_ROUTE_PASS / JOINT_TRACE_PENDING`。production已拆为all producer/sample freeze→ascending exact proxy copy+route；producer更新可供后续AI读取，route结果不泄漏回同tick AI。focused7/7、AI80/80、B2联合198/198、07:51:26 SelfCheck、真实DDJ/DRA Play与Console0。legacy AI→exact仍是迁移桥；native combo producer/action readers和joint trace仍待。

> **B2 PROXY CONTROL LIFECYCLE FOCUSED PASS（2026-09-03）：** `NTSD28-B2-PROXY-CONTROL-LIFECYCLE-001 / FOCUSED_TEST_PASS / CONTROL_CORE_READY / PRODUCTION_UNCONNECTED`。counter status、type0 hit activation、positive-HP/body-gated decrement与unconditional expiry core已实现；13/13+44/44、zero-alloc、SelfCheck、Console0。hit writer归B5、global tail归B3，当前未接production。

> **B2 NATIVE COMBO BRIDGE FOCUSED PASS（2026-09-03）：** `NTSD28-B2-NATIVE-COMBO-BRIDGE-001 / FOCUSED_TEST_PASS / COMBO10_CORE_READY / PRODUCTION_UNCONNECTED`。exact combo10、edge/history order、same-sample/early-terminal、history priority、tail/clear与严格legacy projection已实现；19/19→31/31、SelfCheck、Console0。native `hit_aj/ad/jd`不伪映射；production调用另包。

> **B2 NATIVE INPUT STATE CARRIER FOCUSED PASS（2026-09-03）：** `NTSD28-B2-NATIVE-INPUT-STATE-CARRIER-001 / FOCUSED_TEST_PASS / CARRIER_READY / WRITERS_UNCONNECTED`。per-runtime exact33-byte block+counter/source/enabled已闭合reset、deep-copy、entity/aggregate snapshot与checksum；5/5+38/38+23/23、full SelfCheck、Console0。legacy字段和production combo/control/two-pass/joint trace仍未接。

> **B2 PROXY PRODUCTION CROSSWALK VERIFIED（2026-09-03）：** `NTSD28-B2-PROXY-INTEGRATION-AUDIT-001 / VERIFIED / CROSSWALK_CLOSED / IMPLEMENTATION_SPLIT_DEFINED / GOVERNANCE_ONLY`。7 keys/edges与CdDefendLock可映射；native combo10不等于Unity旧ComboD*9，control3+tail缺失；authority两遍而Unity AI+combo逐实体交错。实施拆为carrier→combo→control→two-pass→joint trace，尚未接production。

> **B2 EXACT INPUT PROXY BLOCK FOCUSED PASS（2026-09-03）：** `NTSD28-B2-INPUT-PROXY-BLOCK-001 / FOCUSED_TEST_PASS / PROXY_BLOCK_READY / PRODUCTION_UNCONNECTED`。test-first CS0246；`9bda...d161` 5/5，33-byte offsets、deep copy/exclusion/fail-closed及4096 zero-alloc通过；compile/Console0。entity/AI/two-pass待。

> **B2 INPUT PHASE CADENCE FOCUSED PASS（2026-09-03）：** `NTSD28-B2-INPUT-PHASE-CADENCE-001 / FOCUSED_TEST_PASS / PHASE_CADENCE_READY / AI_PROXY_PENDING`。test-first14；pending七键、default2tu 1/0、oneTu恒0及snapshot/checksum落地。final `73c9...737f` 86/86、full SelfCheck、compile/Console0；tick2 edge5/tick3=4/tick6=1。AI/proxy/RNG consumer待。

> **B2 DUAL RNG WORLD-STATE FOCUSED PASS（2026-09-03）：** `NTSD28-B2-RNG-WORLD-STATE-001 / FOCUSED_TEST_PASS / WORLD_STATE_READY / CONSUMERS_UNMIGRATED`。new5/5、final related `9c40...1509` 50/50；初次7 restore failures定位tableSeed provenance并修正。core1024/restore128/checksum256零分配，fresh transfer、full SelfCheck、compile/Console0。consumer和B0 exporter/JSON trace未迁移。

> **B2 LEGACY SELFCHECK SEAMS VERIFIED（2026-09-03）：** private-member、ItrRest owner-path、runtime-slot accessor三个独立test-only包均 `VERIFIED / FULL_SELFCHECK_PASS`。camera property注入、重组后4 calls/3 owners、当前internal slot seam均通过；production零改动。

> **B2 VALIDATION FOUND LEGACY ITREST OWNER PATHS（2026-09-03）：** `NTSD28-B2-SELFCHECK-REST-OWNER-PATH-001 / IN_PROGRESS / RED_SELF_CHECK_CAPTURED / TEST_ONLY`。相机helper修复后SelfCheck继续到ItrRest path guard；4个production调用均在当前Runtime/Passes/Lockstep-Snapshot三个owner，测试仍匹配旧路径。仅重基线精确path/store断言。

> **B2 VALIDATION FOUND LEGACY SELFCHECK REFLECTION SEAM（2026-09-03）：** `NTSD28-B2-SELFCHECK-PRIVATE-MEMBER-SEAM-001 / IN_PROGRESS / RED_SELF_CHECK_CAPTURED / TEST_ONLY`。full SelfCheck在camera stale-state注入失败；`_cameraX/_cameraVel`已是private property，旧helper只查field且静默no-op。仅修test helper支持field/property和missing fail-closed；production不改。

> **B2 DUAL RNG WORLD-STATE STARTED（2026-09-03）：** `NTSD28-B2-RNG-WORLD-STATE-001 / IN_PROGRESS / TEST_FIRST / PRE_CODE`。只接world owner、default/match/bootstrap seed、reset、allocation-free scalar snapshot/fresh restore/runtime checksum；legacy RNG/consumer保持，B0 exporter/JSON trace另包。先取missing-seam red。

> **B2 STANDALONE DUAL RNG PRIMITIVE FOCUSED PASS（2026-09-03）：** `NTSD28-B2-NATIVE-DUAL-RNG-PRIMITIVE-001 / FOCUSED_TEST_PASS / PRIMITIVE_READY / WORLD_UNCONNECTED`。test-first 12个CS0246；final `736e...c58f`：本包6/6、shared RNG1/1、boundary RNG3/3。B0 authority seed精确复现CRT state1758127634/table hash `A1BA1B90EA55796D`；compile/Console0。world/AI/consumer仍未接。

> **B2 INPUT/RNG SOURCE AUDIT VERIFIED（2026-09-03）：** `NTSD28-B2-INPUT-RNG-SOURCE-AUDIT-001 / VERIFIED / SOURCE_INVENTORY_CLOSED / IMPLEMENTATION_SPLIT_DEFINED / GOVERNANCE_ONLY`。authority input 1tu=0、2tu=1/0，AI每tick/human phase0 sampling，recording-before-proxy与精确0x21-byte proxy已闭合；RNG为CRT+3001-byte synchronized双流，生产83 expressions/80 IDs、direct CRT2。Unity目前通用RNG37、AI `.Rand`文本168，无同步表状态。下一包仅做未接world的双流基元+authority vectors；尚未对齐。

> **B1 EXIT VERIFIED / B2 READY（2026-09-03）：** `NTSD28-B1-TIME-HOST-EXIT-AUDIT-001 / VERIFIED / B1_CURRENT_PRODUCTION_READY / B2_READY / GOVERNANCE_ONLY`。current production Host 33/3ms、two-interval、F1/F2/F5/pause闭合；focused7/7、related31/31、Play32.71883/3.88752ms、ratio0.118816、report `DAC9D1...C0ED`。worker当前因Unity bindings ineligible，B9解除前回B1；OS实体键留B12。下一阶段B2 input+dual RNG。

> **B1 WORKER PACING BOUNDARY CLOSED（2026-09-03）：** `NTSD28-B1-WORKER-PACING-AUDIT-001 / FOCUSED_TEST_PASS / REAL_REASON_CAPTURED / CURRENT_PATH_INACTIVE / B9_REVALIDATION_TRIGGER`。Scene设useWorker=true但真实reason为`unity-presentation-bindings-are-still-attached`，failure/submission空；current inline path已过。single-in-flight不能同Update第二tick，故B9解除binding前必须补B1 worker cadence实现/trace。report `DAC9D1...C0ED`。

> **B1 UNITY HOST-LOOP/PRESENT BRIDGE FOCUSED+PLAY PASS（2026-09-03）：** `NTSD28-B1-UNITY-HOST-LOOP-BRIDGE-001 / FOCUSED_TEST_PASS / REAL_PLAY_PASS / WORKER_PATH_PENDING`。red6/7→final7/7、related31/31；Play Normal32.76602ms、Fast4.4956583ms、ratio0.1372049、max jump2，report `07364B...B95F`。每Update最多排空2interval；当前worker inactive，worker第二submit未证。

> **B1 LEGACY PLAY POLLER NO-REQUEST ISOLATION PASS（2026-09-03）：** `NTSD28-B1-EDITOR-PROBE-REQUEST-ISOLATION-001 / FOCUSED_TEST_PASS / REAL_PLAY_NO_REQUEST_PASS / REQUEST_SCENARIO_NOT_RERUN`。OID5152/CentralLiveness request check已前移到所有unpause之前；final F1 pause稳定且SetPaused仅bootstrap一次。旧R8有request完整场景未重跑。

> **B1 HOST PHYSICAL EDGE LATCH FOCUSED+DEVICE PATH PASS（2026-09-03）：** `NTSD28-B1-HOST-PHYSICAL-EDGE-LATCH-001 / FOCUSED_TEST_PASS / UNITY_COMPILE_0 / EDGE_LATCH_7_7 / REAL_PLAY_DEVICE_PATH_PASS`。false→true、held no-repeat、release/clear rearm通过；related31/31，真实Play临时Input System Keyboard走production入口通过。OS实体键未自动化。

> **B1 REAL PLAY HOST TRACE PASS WITH BOUNDARIES（2026-09-03）：** `NTSD28-B1-HOST-RUNTIME-TRACE-001 / FOCUSED_TEST_PASS / REAL_PLAY_SYNTHETIC_DEVICE_PASS / OS_PHYSICAL_PENDING / WORKER_INACTIVE`。report `07364B...B95F`：Normal32.76602ms、Fast4.4956583ms、F2 +1、running F2无latent、max jump2。自动化为临时虚拟Keyboard的production路径；OS实体键和worker runtime未覆盖。

> **B1 F1/F2/F5 HOST CONTROL FOCUSED PASS（2026-09-03）：** `NTSD28-B1-HOST-CONTROL-001 / FOCUSED_TEST_PASS / UNITY_COMPILE_0 / HOST_CONTROL_6_6 / RUNTIME_PENDING`。LocalFreeRun生产入口已接F1 pause、F2 paused-only one-step、F5 33/3ms+debt reset；single-step复用production internal tick/worker入口。job `3625...b6a0` 6/6、related `a92c...ee4` 33/33，Console 0 error，tool6/12/21/5、Ledger95/22通过。Manual/Lockstep不消费物理Host键；真实Play物理按键/cadence/worker runtime为下一包。

> **B1 CADENCE CONTRACT FOCUSED PASS（2026-09-03）：** `NTSD28-B1-CADENCE-CONTRACT-001 / FOCUSED_TEST_PASS / UNITY_COMPILE_0 / HOSTPOLICY_4_4 / RUNTIME_PENDING`。red `bbd2...a50b`捕获旧33.333ms与8tick debt；final `d112...b3a6`验证精确33/3ms、cap2、one/update、cadence change clear；related11/11、stress3/3、B0 joint9/9。Manual domain SHA不变；F1/F2/F5和真实Play下一包。

> **B1 TIME/HOST SOURCE AUDIT VERIFIED（2026-09-03）：** `NTSD28-B1-TIME-HOST-SOURCE-AUDIT-001 / VERIFIED / SOURCE_CHAIN_CLOSED / IMPLEMENTATION_SPLIT_DEFINED / GOVERNANCE_ONLY`。authority精确33/3ms、每host loop最多1tick、debt cap2 intervals；F5清logic/render debt，pause清logic debt，F2只在paused立即单步且running不排队。Unity当前1/30、8tick cap、pause不清debt且无F1/F2/F5 Host控制。下一包先落纯cadence/HostPolicy合同。

> **B0 BASELINE EXIT VERIFIED / B1 READY（2026-09-03）：** `NTSD28-B0-BASELINE-EXIT-AUDIT-001 / VERIFIED / B0_BASELINE_READY / B1_READY / GOVERNANCE_ONLY`。B0双端schema/input/RNG topology/slot+epoch/47字段/validator/comparator基线闭合；仅表示可进入B1，不表示战斗parity。RNG首差→B2，9 missing→B4/B5/B7，baseMaxMp→B11，formal EXE certificate/full campaign→B12。下一阶段B1时间与Host。

> **B0 DOMAIN FIRST-DIFFERENCE FOCUSED PASS（2026-09-03）：** `NTSD28-B0-DOMAIN-FIRST-DIFFERENCE-001 / FOCUSED_TEST_PASS / BUILD_0_0 / COMPARATOR_6_6 / REAL_FIRST_DIFFERENCE / PRODUCTION_UNCHANGED`。真实3tick input、slot occupant/epoch、lifecycle全equal；capacity1000/400只应用用户例外。首差`rng.streamAvailability / STREAM_TOPOLOGY_DIFFERENCE`，authority CRT+sync均0/0/0，Unity deterministic1/2/1；report `B0AED0...CF185`。RNG映射未伪造，留B2。

> **B0 UNITY DOMAIN RAW EXPORTER FOCUSED PASS（2026-09-03）：** `NTSD28-B0-UNITY-DOMAIN-RAW-EXPORTER-001 / FOCUSED_TEST_PASS / UNITY_COMPILE_0 / JOINT_9_9 / REAL_DOMAIN_VALID / PRODUCTION_UNCHANGED`。job `1f2d...e9dc` 9/9；entity/domain `ADD7AE...ED97D`/`D88820...BBFC8`两次确定性，domain validator valid。mask同authority；Unity RNG delta1/2/1 vs authority0/0/0已记录为B2差异；capacity400例外、slot0/1 epoch1。跨端entity raw20/27/75，首差tick3 slot0 controlSlot2/1。

> **B0 AUTHORITY DOMAIN RAW EXPORTER FOCUSED PASS（2026-09-03）：** `NTSD28-B0-AUTHORITY-DOMAIN-RAW-EXPORTER-001 / FOCUSED_TEST_PASS / CPP_BUILD_0_0 / REAL_DOMAIN_VALID / AUTHORITY_READ_ONLY`。真实mask `(17,2)/(1,96)/(0,12)`；initial CRT/sync calls `3000/1`且三tickdelta全0；slot0/1 epoch1稳定、无伪birth。entity raw `609D39...56C32`、domain raw `A3C337...C39A6`两次确定性并通过validator，旧optional模式也valid。source-model/certificate false；下一包接Unity exporter。

> **B0 INPUT/RNG/SLOT RAW CONTRACT FOCUSED PASS（2026-09-03）：** `NTSD28-B0-INPUT-RNG-SLOT-RAW-CONTRACT-001 / FOCUSED_TEST_PASS / BUILD_0_0 / DOMAIN_12_12 / EXISTING_21_21 / RAW_5_5 / PRODUCTION_UNCHANGED`。合同SHA `7185B5D2...DCC68AD`；七动作held input、authority CRT/synchronized、Unity deterministic三个source-native stream、slot occupant/epoch和snapshot-derived birth/death/reuse已冻结。availability/null、RNG delta、slot/event一致性fail-closed；Ledger88/16与format通过。exporter/非空场景/真实双端raw仍待，B0未完成。

> **B0 LIFECYCLE PENDING BINDING CORRECTION FOCUSED PASS（2026-09-03）：** `NTSD28-B0-LIFECYCLE-PENDING-BINDING-CORRECTION-001 / FOCUSED_TEST_PASS / BUILD_0_0 / UNITY_COMPILE_0 / JOINT_6_6 / MATURITY_38_0_9 / REAL_CLASSIFICATION_CORRECTED`。authority terminal-code pending+lifecycle_code支持encoded reset，Unity PendingFlushDestroy是更广direct-release标志；错误candidate已改missing/null。首次self-test旧guard已修正，最终21/21；job `d4bc39d450b94265b18d0e4214140bd4` 证明true不泄漏；raw为10/37/60，生产实现留B7。

> **B0 ENVIRONMENT STATE BINDING CORRECTION FOCUSED PASS（2026-09-03）：** `NTSD28-B0-ENVIRONMENT-STATE-BINDING-CORRECTION-001 / FOCUSED_TEST_PASS / BUILD_0_0 / UNITY_COMPILE_0 / JOINT_6_6 / MATURITY_38_1_8 / REAL_CLASSIFICATION_CORRECTED`。native Entity+0x320 environment_state在Unity无等价字段；错误Unk328 candidate已改为missing/null。job `a86feda5f97e4e2a9f4423d0cd2ba22a` 证明-3不泄漏；raw仍9/38/54，生产实现留B4/B5。

> **B0 OWNER SLOT BINDING FOCUSED PASS（2026-09-03）：** `NTSD28-B0-OWNER-SLOT-BINDING-001 / FOCUSED_TEST_PASS / BUILD_0_0 / UNITY_COMPILE_0 / JOINT_6_6 / MATURITY_38_2_7 / REAL_BASELINE_UNCHANGED`。native Entity+0x354 owner_slot→Unity OwnerSlotIndex晋级VERIFIED；job `b4b2472b4c85491291a05d28bc6978cd` 以17并用19/21/23防误绑；raw仍9/38/54，不改production owner。

> **B0 PARTICIPANT CLASS BINDING FOCUSED PASS（2026-09-03）：** `NTSD28-B0-PARTICIPANT-CLASS-BINDING-001 / FOCUSED_TEST_PASS / BUILD_0_0 / UNITY_COMPILE_0 / JOINT_6_6 / MATURITY_37_3_7 / REAL_BASELINE_UNCHANGED`。native Entity+0x344 participant_class→Unity Unk344晋级VERIFIED；job `e879e34cb57b4f4291d1b33d01d71f9e` 以非零4通过；raw仍9/38/54，不改production。

> **B0 BATTLE GROUP BINDING FOCUSED PASS（2026-09-03）：** `NTSD28-B0-BATTLE-GROUP-BINDING-001 / FOCUSED_TEST_PASS / BUILD_0_0 / UNITY_COMPILE_0 / JOINT_6_6 / MATURITY_36_4_7 / REAL_BASELINE_UNCHANGED`。native Entity+0x364 battle_group已从错误Team candidate更正为RelationTeam并晋级VERIFIED；job `6bc4214b1894458a85cbb8bab998577c` 以Team11/RelationTeam13验证输出13；raw仍为9/38/54，不改production relation。

> **B0 HP BOUND BINDINGS FOCUSED PASS（2026-09-03）：** `NTSD28-B0-HP-BOUND-BINDINGS-001 / FOCUSED_TEST_PASS / BUILD_0_0 / UNITY_COMPILE_0 / JOINT_6_6 / MATURITY_35_5_7 / REAL_BASELINE_UNCHANGED`。effective_max_hp/base_max_hp→HPBound/HP3源链闭合；job `b050dd2519714976b62eb10f7c1b05bc` 以480/500互异值通过；raw仍为9 differences/38 equal/54 occurrences。首次参数未嵌套误跑1601项有10项任务外失败，精确目标6/6通过；不改production vitals或DAT。

> **B0 ATTACKER REST BINDING FOCUSED PASS（2026-09-03）：** `NTSD28-B0-ATTACKER-REST-BINDING-001 / FOCUSED_TEST_PASS / BUILD_0_0 / UNITY_COMPILE_0 / JOINT_6_6 / MATURITY_33_7_7 / REAL_BASELINE_UNCHANGED`。attacker_rest→AttackExempt源链闭合；job `d43cf6e368094a6c81d2151055a9e787` 以非零5通过；raw仍为9 differences/38 equal/54 occurrences，不改production rest。

> **B0 HIT REACTION / FALL CORRECTION FOCUSED PASS（2026-09-03）：** `NTSD28-B0-HIT-REACTION-BINDING-001 / FOCUSED_TEST_PASS / BUILD_0_0 / UNITY_COMPILE_0 / JOINT_6_6 / MATURITY_32_8_7 / REAL_BASELINE_UNCHANGED`。旧hitReactionTimer→HitStop错误candidate已改为Fall；job `42b910d17b5a47fd8aa2b4614aaa12fb` 以Fall60/HitStop4通过；raw仍为9 differences/38 equal/54 occurrences，不改production hit。

> **B0 REVIVAL TRIPLET CORRECTION FOCUSED PASS（2026-09-03）：** `NTSD28-B0-REVIVAL-FIELD-BINDINGS-001 / FOCUSED_TEST_PASS / BUILD_0_0 / AUTHORITY_RAW_VALID / UNITY_COMPILE_0 / JOINT_6_6 / REAL_DIFF_CLOSED / MATURITY_31_9_7`。旧reviveLives→RespawnCount错误已原子更正为HP2Orig/HPOrig/RespawnCount三字段；job `34e6199d932240f797443549ee5d468c` 以4/7/320通过；Unity raw `97617669...ED37B7D`，真实差异11→9、equal36→38、occurrence66→54；不改production revival。

> **B0 MOTION HOLD / FRAME DELAY BINDING FOCUSED PASS（2026-09-03）：** `NTSD28-B0-MOTION-HOLD-BINDING-001 / FOCUSED_TEST_PASS / BUILD_0_0 / UNITY_COMPILE_0 / JOINT_6_6 / REAL_DIFF_CLOSED / MATURITY_28_10_9`。权威 `motion_hold_timer` 绑定Unity `FrameDelay`；job `0f6ee3e497964378b1d2b8d9b7c141b8` 覆盖+3/-5；raw SHA `362F1811...7C09D95`，真实差异12→11、equal35→36、occurrence72→66；不改生产逻辑。

> **B0 WEAPON HP BINDING FOCUSED PASS（2026-09-03）：** `NTSD28-B0-WEAPON-HP-BINDING-001 / FOCUSED_TEST_PASS / BUILD_0_0 / UNITY_COMPILE_0 / JOINT_6_6 / REAL_DIFF_CLOSED / MATURITY_27_10_10 / GLOBAL_LEDGER_PASS`。权威 `weapon_hp_31c` 绑定Unity `WeaponFlightCounter`；job `16bf469ff22d48f9ad1ba1d977496949` 覆盖37/-1；raw SHA `AE935277...24F67E`，真实差异13→12、equal34→35、occurrence78→72；Ledger 77/14 PASS，不改生产writer/DAT/资源。

> **B0 FRAME HISTORY BINDINGS FOCUSED PASS（2026-09-03）：** `NTSD28-B0-FRAME-HISTORY-BINDINGS-001 / FOCUSED_TEST_PASS / REAL_BASELINE_PASS / REQUEST_2_2_DETERMINISTIC / JOINT_6_6 / MATURITY_26_10_11`。Unity compile0，同一Editor request两次raw同SHA，真实差异14→13；同一job实际通过非零Frame.Prev9/PrevFrame2=6分离断言。

> **B0 ACTION LATCH / WAIT COUNTER BINDING FOCUSED PASS（2026-09-03）：** `NTSD28-B0-ACTION-LATCH-BINDING-001 / FOCUSED_TEST_PASS / BUILD_0_0 / UNITY_COMPILE_0 / FINAL_JOINT_6_6 / REAL_DIFF_CLOSED / MATURITY_24_11_12`。非零WaitCounter8已测；真实差异15→14、equal32→33、occurrence90→84；tickActionSnapshot另包。

> **B0 CONTROL SLOT / ANIM COUNTER BINDING FOCUSED PASS（2026-09-02）：** `NTSD28-B0-CONTROL-SLOT-BINDING-001 / FOCUSED_TEST_PASS / BUILD_0_0 / UNITY_COMPILE_0 / JOINT_6_6 / REAL_DIFF_CLOSED / MATURITY_23_11_13`。native +0x000现绑定AnimCounter，非零5已测；差异16→15、equal31→32、occurrence96→90。不改production writer。

> **B0 ALLOCATION EPOCH NORMALIZATION FOCUSED PASS（2026-09-02）：** `NTSD28-B0-ALLOCATION-EPOCH-NORMALIZATION-001 / FOCUSED_TEST_PASS / CPP_BUILD_0_0 / REAL_DIFF_CLOSED / AUTHORITY_GUARD_PASS / GLOBAL_LEDGER_PASS`。authority raw现按槽首次epoch1；真实差异17→16、equal30→31、occurrence99→96，首差异转为identity.controlSlot。不改authority或Unity Slot模型。

> **B0 UNITY COMPLETED-TICK BOUNDARY FOCUSED PASS（2026-09-02）：** `NTSD28-B0-UNITY-COMPLETED-TICK-BOUNDARY-001 / FOCUSED_TEST_PASS / UNITY_COMPILE_0 / JOINT_6_6 / EXACT_CHARACTER_DELTA_2 / GLOBAL_LEDGER_PASS`。diagnostic bootstrap不再注入entry-clear，每个输出tick硬验两个exact character frame-tick；不改production tick/input。

> **B0 FRAME COUNTER BINDING FOCUSED PASS（2026-09-02）：** `NTSD28-B0-FRAME-COUNTER-BINDING-001 / FOCUSED_TEST_PASS / BUILD_0_0 / SELFTEST_21_21 / RAW_SELFTEST_5_5 / UNITY_COMPILE_0 / JOINT_6_6 / REAL_DIFF_CLOSED / MATURITY_22_11_14`。frameCounter现读AttackingCounter，真实值1/2/3，差异18→17、equal29→30；FrameWaitCounter与production writer未改。contract SHA `B186E4C6...834C3`。

> **B0 RAW ENTITY COMPARATOR FOCUSED PASS（2026-09-02）：** `NTSD28-B0-RAW-ENTITY-DIFFERENCE-001 / FOCUSED_TEST_PASS / BUILD_0_0 / RAW_SELFTEST_5_5 / EXISTING_SELFTEST_21_21 / REAL_29_EQUAL_18_DIFFERENT / GLOBAL_LEDGER_PASS`。真实报告3 ticks/6 pairs/282字段occurrences，105个差异occurrences；14 missing与4 non-null差异分开，首差异allocationEpoch，未自动归一化任何例外。下一包先修正frameCounter映射。

> **B0 UNITY RAW EXPORTER FOCUSED PASS（2026-09-02）：** `NTSD28-B0-UNITY-RAW-SCENARIO-EXPORTER-001 / FOCUSED_TEST_PASS / UNITY_COMPILE_0_ERROR / EDITMODE_3_3_PASS / REAL_3_TICKS_6_ENTITIES / OID99_FAIL_CLOSED / DETERMINISTIC_RERUN / GLOBAL_LEDGER_PASS`。OID99只在新权威存在，原99/7场景保留为内容缺口证据。公共OID2/7场景真实输出tick1..3；Unity raw SHA `5293992E...D5D9C7`，同场景authority raw SHA `954D3F77...80566`且valid。首盘29字段相等、18字段差异（14 missing + allocationEpoch/frameCounter/baseMaxMp/environmentState）；下一包做repeatable raw first-difference与语义归因。

> **B0 UNITY ENTITY PROJECTION FOCUSED PASS（2026-09-02）：** `NTSD28-B0-UNITY-ENTITY-PROJECTION-001 / FOCUSED_TEST_PASS / UNITY_COMPILE_0_ERROR / EDITMODE_3_3_PASS / FIELDS_47_BINDINGS_21_12_14 / MISSING_NULL / GLOBAL_LEDGER_PASS`。同一 Editor 经 MCP refresh/domain reload；前两次 focused 真实暴露 fixture reset 和 RawRuntime/live runtime 所有权问题，最终投影读取 `view.Entity.Runtime`，第三次 3/3 pass。下一包接 completed-tick scenario writer。

> **Ledger C++ coverage verified（2026-09-02）：** `CHANGE-LEDGER-CPP-COVERAGE-001 / VERIFIED / CPP_ACTUAL_DIFF_COVERED / SYNTHETIC_UNRECORDED_CPP_FAIL_CLOSED / GLOBAL_LEDGER_PASS`。Tools authored C/C++ 已纳入与 C#/scripts 相同的 Change Ledger gate。

> **B0 AUTHORITY SOURCE CAPTURE READY（2026-09-02）：** `NTSD28-B0-AUTHORITY-SOURCE-CAPTURE-001 / FOCUSED_TEST_PASS / SOURCE_MODEL_CAPTURE_READY / CPP_BUILD_0_WARN_0_ERROR / REAL_3_TICKS_6_ENTITY_SNAPSHOTS / SELF_TEST_21_21 / AUTHORITY_OUTPUT_GUARD_PASS / GLOBAL_LEDGER_PASS / CERTIFICATE_FALSE`。formal EXE/source/runner/exact binary identity 分离；所有 build/output 在 workspace Temp。不是 formal EXE runtime trace；下一包接 Unity raw exporter。

> **HISTORICAL B0 AUTHORITY EXPORTER DISCOVERY（2026-09-02，身份已由PROMOTION-002取代）：** 当时source `parity_runner_main.cpp`/`trace_json28` 只输出 `ntsd28-trace/1.0`，authority树未发现预编译runner/LFR fixture；ScenarioLoader锁定legacy `5EDA5144...19D86B`，与当时正式EXE `1277B70B...DAF75`不同。当前正式身份只读本文顶部与`docs/ai/CURRENT-AUTHORITY.md`；本条不得用于恢复旧SHA。

> **B0 ENTITY FIELD SCHEMA READY（2026-09-02）：** `NTSD28-B0-ENTITY-FIELD-SCHEMA-001 / FOCUSED_TEST_PASS / TRACE_SCHEMA_V2 / ENTITY_FIELDS_47 / BINDINGS_21_VERIFIED_12_CANDIDATE_14_MISSING / BUILD_0_WARN_0_ERROR / SELF_TEST_17_17 / CONTRACT_SHA_F5E0154A / GLOBAL_LEDGER_PASS`。所有字段 strict，候选/缺失不能静默忽略；exporter/runtime 尚未开始，下一包从 authority 现有只读输出能力审计开始。

> **Ledger governance metadata correction verified（2026-09-02）：** `CHANGE-LEDGER-GOVERNANCE-ONLY-METADATA-001 / VERIFIED / PARSE_PASS / GLOBAL_LEDGER_PASS / NEGATIVE_FIXTURES_PASS`。validator 只允许唯一 `GOVERNANCE_ONLY + code-path: NONE`，并证明 NONE 不能覆盖脚本 diff；旧 Frame Structure parent 已按 Server 同 ID 证据准确修正，无 Unity/runtime/resource/authority 变化。

> **B0 TRACE CONTRACT CLOSED（2026-09-02）：** `NTSD28-B0-TRACE-CONTRACT-001 / FOCUSED_TEST_PASS / TOOL_CONTRACT_READY / RELEASE_BUILD_0_WARN_0_ERROR / SELF_TEST_13_13 / CONTRACT_SHA_5B5E4ABF / FORMAT_PASS / GLOBAL_LEDGER_PASS`。独立 `Tools/NTSD28Parity` 已实现 2.8 trace contract、validator、streaming comparator；旧 `Tools/NTSDParity` 未修改。exporter、真实双端 trace、Unity/C++ runtime 与资源仍未开始；下一包必须先冻结 entity-field schema。

> **NTSD24_AUTHORITY_SUPERSEDED（2026-09-02）：** 本文仍保留 NTSD 2.4 旧权威路径，仅作为历史证据；不得据此定义当前战斗规则、pass、timing、slot、RNG、字段、生命周期、表现或“已对齐”状态。任何恢复先读 `docs/ai/CURRENT-AUTHORITY.md`；当前权威是 NTSD 2.8-Logan 正式 EXE 及其对应 playable 源码，旧结论一律 `REBASELINE_REQUIRED`。

> **HISTORICAL AUTHORITY MIGRATION（2026-09-02，正式SHA已由PROMOTION-002取代）：** 此处原锁定`1277B70B...DAF75`的决定只保留迁移历史。当前唯一战斗行为权威必须从本文顶部和`docs/ai/CURRENT-AUTHORITY.md`恢复，为Bug修复版`B1E13AE1...9033`及其当前source闭包；不得从本条恢复旧SHA。NTSD 2.4/C#/旧game_tick废止边界仍持续有效。

> **NTSD 2.8 REALIGNMENT MASTER（2026-09-02 用户确认）：** 当前唯一差异总表为 `Assets/NTSD/Docs/ntsd28-logan-vs-unity-battle-alignment.md`，状态 `NTSD28-UNITY-BATTLE-REALIGNMENT-001 / STATIC_INVENTORY_COMPLETE / IMPLEMENTATION_NOT_STARTED / TRACE_BASELINE_PENDING`。最终目标是非例外战斗规则、状态、时序和战斗表现与正式 NTSD 2.8-Logan 完全一致。Slot 容量继续使用 Unity；头顶血条、FootSelf、移动端取景、多边形边界、当前随机掉武器和固定世界相机按用户决定保留；完整原生 HUD、结果表现、背景多层/cycle、完整选择流程按用户决定排除。旧 `NTSDSpec` 与内容/数值/资源差异必须处理；内容策略待用户决定。本轮只有文档，无运行代码或资源改动。

> **Runtime FootSelf code written, Unity runtime validation pending editor recovery:** `BATTLE-CENTRAL-RUNTIME-FOOTSELF-001 / CODE_WRITTEN / EXTERNAL_COMPILE_PASS / UNITY_COMPILE_PENDING / FOCUSED_PENDING / PLAY_RUNTIME_PENDING / PRESENTATION_ONLY`。human roster input binding判Self、稳定ground anchor、Preview 64×24/offset/tint、单Mesh/submesh与submission lease已接；FootSelf→原Shadow/actor→HP。dotnet compile0；当前Editor 6401 bridge timeout且ScriptAssemblies未fresh，退出Play/恢复bridge后继续Unity compile/focused/Play截图，勿启动第二实例。

> **Existing Shadow + FootSelf central Editor preview ready:** `BATTLE-CENTRAL-EDITOR-FOOT-MARKER-PREVIEW-001 / FOCUSED_TEST_PASS / EDITOR_PREVIEW_READY / USER_VISUAL_REVIEW_PENDING / RUNTIME_NOT_STARTED / PRESENTATION_ONLY / GLOBAL_LEDGER_BLOCKED_EXTERNAL`。正式common Shadow与新增FootSelf为独立central batch，顺序shadow→marker→actor→health；Shadow复用正式native size/Pivot，FootSelf支持128×48 size+offset authoring。compile0、focused10/10；1000 Shadow与1000 FootSelf各自单segment/单draw；offscreen shadow1/segment1、foot1/segment1、yellow75、green0、Scene clean。正式runtime own-player FootSelf snapshot/batching未接，等用户先确认视觉；global validator仅受任务外记录阻塞。

> **Direction B (2026-09-02, current but strategy pending):** Unity Git-restored current DAT remains the protected content-value authority until a new content strategy is approved；C++ release remains battle-rule/order authority. The user has now required the Unity-vs-2.8 content/resource gap to be processed, so Direction B is not a permanent exclusion. Until the user selects full switch, missing-only supplementation, or classified authority, only read-only catalog/projection work is allowed；no DAT/resource overwrite is authorized.

> **Simulation directory reorganization implementation complete, global governance externally blocked:** `SIMULATION-DIRECTORY-REORGANIZATION-001 / BLOCKED / IMPLEMENTATION_COMPLETE / UNITY_COMPILE_0 / MANIFEST_142_142 / GUID_142_142 / CONTENT_HASH_142_142 / ROOT_CS_0 / OLD_PATH_0 / PATH_MATRIX_BASELINE_PRESERVED / FULL_1585_EXECUTED_5_EXTERNAL / TWO_CLEAN_PLAY_STOP / SCENE_DIRTY_FALSE / GLOBAL_LEDGER_BLOCKED_EXTERNAL`。142个cs+meta已完成责任分层，7个源码路径读取者已精确更新；无namespace/API/logic/asmdef变化。fresh SelfCheck仍为既有central P4。全局validator只因任务外`CLIENT-CONTENT-FRAME-STRUCTURE-ALIGNMENT-001`缺code-path而exit1；不要顺手修改该记录。

> **SimulationWorld extraction active:** `SIMULATION-WORLD-MODULE-EXTRACTION-001 / IN_PROGRESS / PARTIAL_0 / M1-M9_FOCUSED_PASS / M10_CODE_COMPLETE / FINAL_ACCEPTANCE_BLOCKED_EXTERNAL`。runtime/editor compile0；M10 AI 158/158、worker/checksum/shutdown/architecture 35/35、stale owner-path 3/3；两轮clean Play/Stop无目标cleanup warning且Scene不脏。full 1763被position38/package/static guard/并行S0 WPoint既有基线阻塞，fresh SelfCheck停在任务外central-render P4断言。World为6040行，超过2500报警线的剩余根职责已登记；不得报告整个计划完成。

> **Ordered shutdown implementation active:** `BATTLE-RUNTIME-ORDERED-SHUTDOWN-001 / IN_PROGRESS / PRE_CODE / USER_APPROVED / RUNTIME_LIFECYCLE_ONLY`。用户批准按完整合同实施固定 11 阶段 `Running→Stopping→Stopped`；Change/Task/Ledger/State 已在 C# 前建立。当前范围只含 lifecycle、worker/spawn gate、publication/task/renderer/World/pool/boundary cleanup 和验证；禁止改变 Running battle pass、30Hz、checksum、Scene/DAT/Server/C++ 或顺手完成全量 Mono/Core 分层。

> **Scene teardown fix verified:** `BATTLE-SCENE-TEARDOWN-SINGLETON-001 / VERIFIED / COMPILE_0 / FOCUSED_1_1_PASS / LIVE_TEARDOWN_PASS / CLEANUP_WARNING_0`。关闭 Scene 的 allocation unseal 已改用 factory/pool `TryGetInstance()`；正常 prepare/seal 仍可按需创建。真实 Play 中两者各1，退出后均0，目标 cleanup warning 0；Scene/战斗规则未改。整组 lifecycle fixture 另有一个既有无关 RestartPolicy expected5/actual1 失败，未包装为全组通过。

> **Runtime sprite follow-up verified:** `BATTLE-SPRITE-GRID-SEPARATOR-001 / VERIFIED / COMPILE_0 / FOCUSED_29_29_PASS / LIVE_GREEN_SCAN_0 / PRESENTATION_ONLY`。根因是整 sheet 中不透明绿色网格 separator 会被中央图集上传并在 UV 边界暴露；现按 BMP source 自身像素拓扑清高覆盖率横/纵 separator alpha，避免同 BMP 因不同 DAT 声明产生冲突，禁止全局 green-key。真实 Play Mode 全图无长度 >=8 的匹配绿线，两名角色邻域匹配绿色像素均为0；Scene/战斗逻辑未改。

> **Runtime presentation package verified:** `BATTLE-CENTRAL-RUNTIME-HEALTH-001 / VERIFIED / COMPILE_0 / RUNTIME_PREVIEW_14_14_PASS / CENTRAL_20_20_PASS / LIVE_STYLE_AND_STABLE_ANCHOR_PASS / PRESENTATION_ONLY`。真实 `LF2Character HP/HPBound/HP3` 已进入 immutable frame，central submission 双缓冲各持有一张 health mesh，RenderFeature 在 actors 后至多一次 draw；后一帧 HP 宽度更新已测。Play Mode 实际复用 Editor authoring 的120x10/-16样式，两个不同动画姿势的血条位置稳定。Scene/战斗规则/Server/lockstep未改。

> **User-directed presentation follow-up:** `BATTLE-CENTRAL-EDITOR-PREVIEW-001 / FOCUSED_TEST_PASS / BMP-GRID-SEPARATOR-RECT-FIXED / PERSISTENT-SCENEVIEW-AUTHORING / GLOBAL-LEDGER-BLOCKED-BY-UNRELATED-RECORD / EDITOR-ONLY / PRESENTATION_ONLY`。Editor 示例/验证已改用正式左上 Rect；compile0、focused6/6、pixel637/70/green0、Scene dirty unchanged。全局 Ledger 仍受无关 Change Record 阻塞；正式runtime HP接线未改。

> **Capability-package execution (2026-09-02, current for the Server S0～S9 roadmap):** CAP-S0-1 remains ACTIVE but is stopped at Server Task F.12. The user accepted the `46460c36...` environment diagnosis and Goal comparison；the next pre-compile WPoint manifest was `3d07a4e...e6afb`, not frozen `afe65d17...cf89c26`, with exactly 11 Git-clean DAT mismatches. No Unity compile, WPoint baseline, ParserV2 WPoint, A37, replay or restore followed；formal marker false；S0 NOT_VERIFIED.

> **Latest process gate:** at `2026-09-01 14:03:01 +08:00`, the user-authorized resume still found the same `NTSD_reconstructed` PID66860/start13:49:15. It stopped before MCP/compile/Unity/baseline；Server Task F.7 is current.

> **External NTSD process:** final read-only audit observed `NTSD_reconstructed` PID 62268 from the separate `J:\QQFile\NTSD 2.4.1\...` tree, started 11:14:59. This task did not launch/use/terminate it；future CAP-S0-1 resumption must first recheck the zero-process precondition (Server Task F.4).

> **CAP-S0-1 Client record:** `S0-FORMAL-CONTENT-CLOSURE-001 / IN_PROGRESS / ACTIVE / WPOINT_MANIFEST_DRIFT_BLOCKED / CONFIG_DAT_HEAD_MATCH_OBSERVED_AGAIN / WPOINT_CURRENT_MANIFEST_3D07A4E1 / WPOINT_BASELINE_NOT_CAPTURED / WPOINT_PRODUCTION_EDIT_NOT_STARTED / WPOINT_A37_RESOURCE_EDIT_NOT_STARTED` exists in both repositories. Server Task F.12 owns the 11-file SHA table. Resume only after the user selects a governed recovery; do not promote current HEAD DATs to authority. CAP-S0-1/S0 remain open.

> **Queue selection (superseded 2026-08-31 by the capability consolidation above):** Queue0cu/0cx/0d1/0d5/0d6/0db/0df/0dg/0dk-a and parent0dk are VERIFIED/CLOSED. Queue0dk-b `CLIENT-CONTENT-FRAME-SCALAR-ALIGNMENT-001` was READY, then GATED by the runtime-safety incident, and is now SUPERSEDED into `S0-FORMAL-CONTENT-CLOSURE-001`. Formal marker/S0 unchanged.

> **Parent0dk governance evidence:** coverage52/52 unique；six serial scalar/Itr/Bdy/OPoint/WPoint/topology child batches frozen；no Client resource/source/Unity action.

> **Queue0dk-a verified evidence:** `CLIENT-CPP-FRAME-MULTIVALUE-PARSER-ALIGNMENT-001 / VERIFIED / CLOSED`；compile0、focused4/4、related287/287、fresh SelfCheck15:35:48、Server dual and validators PASS；no DAT/resource changed.

> **Queue0dg verified evidence:** `CLIENT-FORMAL-KERNEL-CPOINT-VALUE-SEAM-001 / VERIFIED / CLOSED`；compile0、focused13/13、related295/295、fresh SelfCheck15:00:19、corpus、warmed0B、Server dual and validators PASS.

> **Queue0df verified evidence:** `CLIENT-CPP-CPOINT-RESOLVED-HURT-ACTION-ALIGNMENT-001 / VERIFIED / CLOSED`；compile0、focused5/5、related238/238、fresh SelfCheck14:17:26、corpus、Server dual and validators PASS.

> **Queue0db verified evidence:** `CLIENT-FORMAL-KERNEL-BPOINT-CATALOG-SEAM-001 / VERIFIED / CLOSED`；compile0、focused7/7、related78/78、fresh SelfCheck13:41:15、corpus、Server dual and validators PASS. No HUD runtime or battle-state field added.

> **Queue0d6 verified evidence:** `CLIENT-FORMAL-KERNEL-WPOINT-VALUE-SEAM-001 / VERIFIED / CLOSED`；compile0、focused7/7、related239/239、fresh SelfCheck13:11:31、corpus、warmed0B、Server dual and validators PASS. Extra full1522 run had six recorded unrelated failures and is not labeled full-pass.

> **Queue0d5 verified evidence:** test-first red；fresh compile0；focused job `63aea56535a140e1a03a02aba02d2ee5` 10/10；related job `113db6d11aea4d03b78170234810d0bb` 232/232；fresh SelfCheck 12:36:26；frozen WPoint corpus and Server dual configuration PASS. Only `WeaponPoint.kind` default plus focused/SelfCheck changed；converter/buffer production source stayed unchanged.

> **Queue0cu `CLIENT-FORMAL-KERNEL-OPOINT-VALUE-SEAM-001` verified evidence / bridge correction:** fresh Unity compile0；official EditMode job `c1f48ca2ef7b4c4c9d1d395b19131ff2` 52/52 PASS；fresh SelfCheck 11:12:51 PASS；Server dual PASS。Fresh `6401 LISTENING` and framed handshake prove the user's Stdio panel was active；the earlier no-listener diagnosis is superseded. Queue0cu CLOSED；formal marker/S0 unchanged。

> **Queue0cx `CLIENT-FORMAL-KERNEL-BDY-VALUE-SEAM-001` verified:** final Unity compile0；EditMode job `8a4bb5df745a44659ccae65e1824ff49` 212/212 PASS；fresh SelfCheck 11:52:09 PASS；frozen SHA/warmed0B/Server dual PASS。Queue0cx CLOSED；marker/S0 unchanged。

> **Queue0d1 `CLIENT-CPP-ITR-PARSER-DEFAULTS-ALIGNMENT-001` verified:** fresh Unity compile0；focused6/6；EditMode `53db60de214d49c982be616e17518057` 212/212；SelfCheck12:09:45 PASS；corpus SHA/Server dual PASS。Queue0d1 CLOSED；marker/S0 unchanged.

> **Formal-content Server consumer result:** future content values/validation/writers belong to shared Core；Unity and Server retain adapter I/O；Server must load before readiness and verify identity/selection/closure before world mutation. No parallel Server DTO/DAT parser or placeholder factory is lawful.

> **Background/bundle contract results:** four-int background/ascending catalog and full bundle/selection/admission/OPoint closure are frozen；background corpus=38 LF/2941 bytes/SHA `B3AFCC...4074`；bundle corpus=32 LF/3510 bytes/SHA `408AD4...A9AB`。All are governance-closed/read-only。

> **Queue0dp-c result:** Release has17 numeric backgrounds；Width/Z are simulation identity，perspective/shadow/layers are presentation。Client data.txt has0 backgrounds，single `Sunagakure` string map is unbound，Scene float derivation is not formal content。

> **Queue0dp-b result:** Artifact/room-selection layers and preworld transitive admission are frozen. A complete Stage identity cannot omit release background width/Z/perspective content；producer/hash remain deferred.

> **Queue0do-c result:** `ANALYSIS_COMPLETE / FULL_CATALOG_PRODUCTION_PARSER_PROJECTION_CLOSED / RESOURCE_PARSER_PRESENTATION_SCOPES_SEPARATED / CLIENT_GATES_FROZEN / GOVERNANCE_CLOSED / READ_ONLY`. Client/release entries are15,395/15,377 and last-wins IDs15,371/15,377；Queue0dk has a 52-OID exact field/block scope；Queue0dk-a records 241 common-Frame pair-token losses；309 sound differences are locator mappings. Old 977/three-item/312-sound claims are superseded.

> **Queue0dp/0dp-a results:** Queue0dp froze immutable object/source-order catalog values, 18 exact binary64 default bits and writer/admission/exclusions. Queue0dp-a froze a 24 LF/4039-byte corpus with SHA `5ACC300E4D07149869884FFCA9DF03DE45411041809E2E2205D7D3076B2E1FE4`. Both are governance-closed/read-only.

> **Queue0do result:** `GOVERNANCE-S0-FORMAL-CHARACTER-CONTENT-AUTHORITY-BOUNDARY-001 / ANALYSIS_COMPLETE / UNITY_CHARACTER_OBJECT_GRAPH_MAPPED / RELEASE_CHARDATA_SCHEMA_CONFIRMED / EIGHTEEN_BINARY64_MOVEMENT_FIELDS_CONFIRMED / FOUR_HUNDRED_SEVENTEEN_WIDTH_FIRST_DIFFERENCES_CONFIRMED / OID_TYPE_CATALOG_BINDING_CONFIRMED / CATALOG_SOURCE_ORDER_BATTLE_SEMANTIC / FRAMESET_OWNER_CONFIRMED / WEAPON_SOUND_RESOURCE_DIFFERENCES_CONFIRMED / PRESENTATION_METADATA_EXCLUDED / SPRITE_COLLISION_ADAPTER_MISOWNERSHIP_CONFIRMED / CHARACTER_CONTRACT_SELECTED / CLIENT_GATES_RECORDED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Client/release catalog IDs/types/order and numeric text match across 137 objects；Client has 417 binary-width and 156 weapon-sound identity differences. Queue0dq/0dr/0ds are gated and unstarted.

> **Queue0dm result:** `GOVERNANCE-S0-FRAME-CROSS-CONSUMER-CONTRACT-001 / ANALYSIS_COMPLETE / GOLDEN_CORPUS_FROZEN / STRUCTURE_CHECK_PASS / DUAL_DIGEST_PASS / FRAME_CLIENT_GATES_RECORDED / CHARACTER_CONTENT_BOUNDARY_SELECTED / GOVERNANCE_CLOSED / NO_PRODUCTION_SOURCE_CHANGE / NO_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Exact corpus is 30 LF/4780 bytes, SHA `747C2754BE8E7E65E993A25C8BA1F1D5715D83FC27FE5470BCC7BEC42D922BEC`.

> **Queue0dl result:** `GOVERNANCE-S0-FORMAL-FRAME-AUTHORITY-FIELD-CONTRACT-001 / ANALYSIS_COMPLETE / IMMUTABLE_FRAME_API_FROZEN / RELEASE_TWENTY_TWO_INT_SCHEMA_FROZEN / SOUND_AND_SIX_LIST_IDENTITY_FROZEN / PRESENCE_AND_EMPTY_FALLBACK_FROZEN / FRAME_ID_SORT_AND_DUPLICATE_REJECTION_FROZEN / SIGNED_SENTINEL_PRESERVATION_FROZEN / METADATA_AND_RUNTIME_STATE_EXCLUDED / CLIENT_RESOURCE_AND_POINT_DEPENDENCIES_FROZEN / FRAME_CORPUS_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Exact `BattleFrameValue`/`BattleFrameSetValue` and canonical writer contract frozen；Queue0dn remains gated.

> **Queue0dj schema result / incidence superseded:** `GOVERNANCE-S0-FORMAL-FRAME-AUTHORITY-BOUNDARY-001 / ANALYSIS_COMPLETE / RELEASE_TWENTY_TWO_INT_PLUS_SOUND_SCHEMA_CONFIRMED / DEFAULT_AND_EMPTY_FALLBACK_MATCHED / SOURCE_ORDER_LAST_WINS_MATCHED / FRAME_VACTION_SCHEMA_GAP_CONFIRMED / CURRENT_CONTENT_INCIDENCE_SUPERSEDED_BY_QUEUE0DO_C / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Keep generic schema/lookup evidence；do not reuse the old 977/three-item incidence.

> **Queue0dh result:** `GOVERNANCE-S0-FORMAL-WEAPON-STRENGTH-AUTHORITY-BOUNDARY-001 / ANALYSIS_COMPLETE / UNITY_LEGACY_GRAPH_MAPPED / PRODUCTION_CALL_GRAPH_UNREACHABLE / CURRENT_312_WPOINT_ATTACKING_ZERO / RELEASE_SCHEMA_ABSENT / RELEASE_KIND5_ITR_OWNER_CONFIRMED / FORMAL_CONTENT_EXCLUDED / CLIENT_RETIREMENT_GATE_RECORDED / FRAME_BOUNDARY_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Seven blocks/28 entries are legacy-only; all 312 authored WPoint attacking values are zero; release owns kind-5 through holder-frame Itr. Queue0di Client retirement remains authorization-gated and unstarted.

> **Queue0de result:** `GOVERNANCE-S0-CPOINT-CROSS-CONSUMER-CONTRACT-001 / ANALYSIS_COMPLETE / GOLDEN_CORPUS_FROZEN / DUAL_DIGEST_PASS / CPOINT_CLIENT_GATES_RECORDED / WEAPON_STRENGTH_BOUNDARY_SELECTED / GOVERNANCE_CLOSED / NO_PRODUCTION_SOURCE_CHANGE / NO_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Exact corpus is 16 LF/3700 bytes, SHA `7FDEA9EB056452FD204BA1302E46F6D042F7818CF3EECB4C6D112AD514C75E88`.

> **Queue0dd result:** `GOVERNANCE-S0-FORMAL-CPOINT-AUTHORITY-FIELD-CONTRACT-001 / ANALYSIS_COMPLETE / IMMUTABLE_CPOINT_API_FROZEN / RELEASE_NINETEEN_SCALAR_SCHEMA_FROZEN / ZERO_DEFAULT_AND_SIGNED_PRESERVATION_FROZEN / ALIAS_RESOLUTION_AND_FINGERPRINT_FROZEN / ORDERED_LIST_AND_PRIMARY_FROZEN / RUNTIME_ENTITY_WRITER_BOUNDARY_FROZEN / CLIENT_CORRECTION_AND_SEAM_GATES_FROZEN / CPOINT_CORPUS_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Nineteen-scalar/list/alias/writer contract is frozen.

> **Queue0dc result:** `GOVERNANCE-S0-FORMAL-CPOINT-VALUE-BOUNDARY-001 / ANALYSIS_COMPLETE / UNITY_CPOINT_GRAPH_MAPPED / RELEASE_NINETEEN_SCALAR_SET_CONFIRMED / ALIAS_SOURCE_ORDER_MATCHED / UNITY_RESOLVED_HURT_CONSUMER_FIRST_DIFFERENCE_CONFIRMED / UNITY_SINGLETON_LAST_WINS_DIFFERENCE_CONFIRMED / RUNTIME_ENTITY_WRITER_OWNER_CONFIRMED / CURRENT_33_BLOCK_INCIDENCE_FROZEN / CPOINT_AUTHORITY_CONTRACT_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Unity graph/current 33 blocks were mapped; parser aliases match release, but hit consumers need resolved injury/cover later.

> **Queue0da result:** `GOVERNANCE-S0-BPOINT-CROSS-CONSUMER-CONTRACT-001 / ANALYSIS_COMPLETE / GOLDEN_CORPUS_FROZEN / DUAL_DIGEST_PASS / BPOINT_CLIENT_SEAM_GATED / CPOINT_BOUNDARY_SELECTED / GOVERNANCE_CLOSED / NO_PRODUCTION_SOURCE_CHANGE / NO_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Exact corpus is 13 LF/597 bytes, SHA `AD8B3E1DD4D020196183C2F2B8B76C1E27F5CFD8FBD938A48AA8DBA95FC81647`.

> **Queue0d9 result:** `GOVERNANCE-S0-FORMAL-BPOINT-CATALOG-VALUE-CONTRACT-001 / ANALYSIS_COMPLETE / IMMUTABLE_BPOINT_API_FROZEN / TWO_SCALAR_SCHEMA_FROZEN / ORDERED_LIST_AND_PRIMARY_FROZEN / EMPTY_LIST_DISTINCT_FROM_ZERO_VALUE / CATALOG_WRITER_FROZEN / BATTLE_STATE_EXCLUSIONS_FROZEN / CLIENT_SEAM_FROZEN / BPOINT_CORPUS_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Two-scalar list/catalog writer and battle-state exclusions are frozen.

> **Queue0d8 result:** `GOVERNANCE-S0-FORMAL-BPOINT-DOMAIN-BOUNDARY-001 / ANALYSIS_COMPLETE / UNITY_BPOINT_GRAPH_MAPPED / RELEASE_TWO_SCALAR_SET_CONFIRMED / RENDERER_ONLY_LIVE_USE_CONFIRMED / BATTLE_STATE_AND_CHECKSUM_EXCLUDED / CATALOG_IDENTITY_INCLUDED / UNITY_SINGLETON_LAST_WINS_DIFFERENCE_CONFIRMED / CURRENT_DEPLOYED_BPOINT_ZERO / BPOINT_CATALOG_VALUE_CONTRACT_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. BPoint is catalog identity plus optional Client presentation, never Server battle state; current Unity content has zero entries.

> **Queue0d7 result:** `GOVERNANCE-S0-WPOINT-KIND5-FALLBACK-REACHABILITY-001 / ANALYSIS_COMPLETE / STATIC_PRODUCTION_CALL_GRAPH_UNREACHABLE / RUNNER_PREPROCESS_ALWAYS_APPLIED / DISABLED_SHADOW_DATA_ORIENTED_MODES_CLOSED / INVALID_PLAN_FALLBACK_REUSES_RUNNER / DIRECT_TEST_DIAGNOSTIC_ENTRY_RETAINED / WPOINT_EXTRAS_NOT_FORMAL / FUTURE_FAIL_CLOSED_REMOVAL_GATE_FROZEN / BPOINT_BOUNDARY_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. All current production modes and invalid-plan fallback preprocess kind-5 through the shared runner; direct internal test entry remains a later fail-closed cleanup gate.

> **Queue0d4 result:** `GOVERNANCE-S0-WPOINT-CROSS-CONSUMER-CONTRACT-001 / ANALYSIS_COMPLETE / GOLDEN_CORPUS_FROZEN / DUAL_DIGEST_PASS / WPOINT_CLIENT_GATES_RECORDED / KIND5_FALLBACK_AUDIT_SELECTED / GOVERNANCE_CLOSED / NO_PRODUCTION_SOURCE_CHANGE / NO_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Exact corpus is 16 LF/1608 bytes, SHA `5A3B6B197BBEBA859ECCD4C4EE853CA8A655B3ABF378FE34E1FF7641DB95A926`.

> **Queue0d3 result:** `GOVERNANCE-S0-FORMAL-WPOINT-AUTHORITY-FIELD-CONTRACT-001 / ANALYSIS_COMPLETE / IMMUTABLE_WPOINT_API_FROZEN / RELEASE_NINE_SCALAR_SCHEMA_FROZEN / ZERO_DEFAULT_FROZEN / SOURCE_ORDER_AND_PRIMARY_ENTRY_FROZEN / UNITY_EXTRAS_FAIL_CLOSED / EMPTY_PRIMARY_FALLBACK_FROZEN / CLIENT_GATES_FROZEN / WPOINT_CORPUS_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Nine-scalar API, full-list identity, primary-entry runtime and empty default are frozen.

> **Queue0d2 result:** `GOVERNANCE-S0-FORMAL-WPOINT-VALUE-BOUNDARY-001 / ANALYSIS_COMPLETE / UNITY_WPOINT_GRAPH_MAPPED / RELEASE_NINE_SCALAR_SET_CONFIRMED / KIND_DEFAULT_FIRST_DIFFERENCE_CONFIRMED / UNITY_EXTRAS_CLASSIFIED / FIRST_ENTRY_RUNTIME_OWNER_CONFIRMED / LEGACY_KIND5_FALLBACK_RISK_MAPPED / WPOINT_AUTHORITY_CONTRACT_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Unity graph and current 312 WPoint blocks were mapped first; no implementation occurred.

> **Queue0d0 result:** `GOVERNANCE-S0-ITR-CROSS-CONSUMER-CONTRACT-001 / ANALYSIS_COMPLETE / GOLDEN_CORPUS_FROZEN / DUAL_DIGEST_PASS / ITR_CLIENT_CORRECTION_GATED / WPOINT_VALUE_BOUNDARY_SELECTED / GOVERNANCE_CLOSED / NO_PRODUCTION_SOURCE_CHANGE / NO_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Exact corpus is 13 LF/3442 bytes, SHA `0F43B27514C3E26B4DBAC75C4CA7EF8AB2B994730BC5EEAE705DDDE2086516D1`.

> **Queue0cz result:** `GOVERNANCE-S0-FORMAL-ITR-AUTHORITY-FIELD-CONTRACT-001 / ANALYSIS_COMPLETE / UNITY_CONSUMER_ORDER_FROZEN / IMMUTABLE_ITR_API_FROZEN / RELEASE_26_SCALAR_SCHEMA_FROZEN / ZWIDTH_DEFAULT_FROZEN / PAIR_AND_SECONDARY_FINGERPRINT_FROZEN / UNITY_EXTRAS_FAIL_CLOSED / MUTABLE_RUNTIME_PROJECTION_FROZEN / ITR_CORPUS_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Unity controls dependency/migration order; C++ release controls battle semantics rather than Server project order. Queue0d1 is a separate Client authorization gate.

> **Queue0cy result:** `GOVERNANCE-S0-FORMAL-ITR-VALUE-BOUNDARY-001 / ANALYSIS_COMPLETE / UNITY_ITR_GRAPH_MAPPED / ZWIDTH_DEFAULT_FIRST_DIFFERENCE_CONFIRMED / PAIR_ENCODING_FIRST_DIFFERENCE_CONFIRMED / UNITY_EXTRA_FIELDS_CLASSIFIED / KIND5_RUNTIME_COPY_MATCHED / ITR_AUTHORITY_CONTRACT_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Unity content has 358 structurally closed Itr blocks plus one unclosed raw start in weapon4 Frame48；no implementation occurred.

> **Queue0cw result:** `GOVERNANCE-S0-BDY-CROSS-CONSUMER-CONTRACT-001 / ANALYSIS_COMPLETE / GOLDEN_CORPUS_FROZEN / DUAL_DIGEST_PASS / BDY_CLIENT_SEAM_GATED / ITR_VALUE_BOUNDARY_SELECTED / GOVERNANCE_CLOSED / NO_PRODUCTION_SOURCE_CHANGE / NO_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Corpus is 10 LF/508 bytes, SHA `309F4F41AAF152DCCA352A2ABEE4DBD49E0B13221C6734E3849404B6B32EE650`; no code/build/Unity action occurred.

> **Queue0cv result:** `GOVERNANCE-S0-FORMAL-BDY-VALUE-CONTRACT-001 / ANALYSIS_COMPLETE / IMMUTABLE_BDY_API_FROZEN / KIND_RAW_EXCLUDED / SOURCE_ORDER_AND_RAW_GEOMETRY_FROZEN / FULL_HEIGHT_SENTINEL_FROZEN / CLIENT_SEAM_SCOPE_FROZEN / BDY_CORPUS_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Exact Bdy X/Y/W/H and external geometry-resolver ownership are frozen; no source/build/Unity action occurred.

> **Queue0ct result:** `GOVERNANCE-S0-OPOINT-CROSS-CONSUMER-CONTRACT-001 / ANALYSIS_COMPLETE / GOLDEN_CORPUS_FROZEN / DUAL_DIGEST_PASS / OPOINT_CLIENT_SEAM_GATED / BDY_VALUE_CONTRACT_SELECTED / GOVERNANCE_CLOSED / NO_PRODUCTION_SOURCE_CHANGE / NO_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Corpus is 10 LF/852 bytes, SHA `2363910A2686D28D5FDE161C00C1777717408FD0736AEF3D5AB7A7CC57C7360E`; no code/build/Unity action occurred.

> **Queue0cs result:** `GOVERNANCE-S0-FORMAL-OPOINT-VALUE-CONTRACT-001 / ANALYSIS_COMPLETE / IMMUTABLE_OPOINT_API_FROZEN / ORDER_AND_ALIAS_FROZEN / LEGACY_TASK_ADAPTER_FROZEN / INVALID_ENTRY_PRESERVATION_FROZEN / CLIENT_SEAM_SCOPE_FROZEN / OPOINT_CORPUS_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Exact value/API, compatibility/task conversion and later Client seam scope are frozen; no implementation was authorized or performed.

> **Queue0cr result:** `GOVERNANCE-S0-FORMAL-POINT-VALUE-BOUNDARY-001 / ANALYSIS_COMPLETE / UNITY_POINT_GRAPH_MAPPED / OPOINT_EIGHT_SCALAR_SEMANTIC_SET_CONFIRMED / UNITY_EXTRA_FIELDS_CLASSIFIED / OTHER_POINT_BLOCKERS_MAPPED / OPOINT_VALUE_CONTRACT_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Unity actual point flow was mapped first; ObjectPoint was selected, other point families remain pending, and no source/build/Unity action occurred.

> **Queue0cq result:** `CLIENT-FORMAL-KERNEL-STAGE-CONTAINER-SHARED-OWNER-001 / FOCUSED_TEST_PASS / SHARED_STAGE_CONTAINER_OWNER_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / USER_STANDING_AUTHORIZED / UNITY_COMPILE_0 / PACKAGE_8_8 / STAGE_RELATED_11_11 / S0_LOCKSTEP_24_24 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Server Core now owns the single immutable source/GUID; Unity and direct/locked .NET 0.8.0 consumers passed. Adapter/runtime/hash/marker were unchanged.

> **Stage-container cross-consumer result:** `GOVERNANCE-S0-STAGE-CONTAINER-CROSS-CONSUMER-CONTRACT-001 / ANALYSIS_COMPLETE / GOLDEN_CORPUS_FROZEN / DUAL_DIGEST_PASS / STAGE_CONTAINER_SHARED_OWNER_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_PRODUCTION_SOURCE_CHANGE`. Ten lines/656 bytes and SHA `39816AB63F6BD54E04CE70A589B5CCB40A4D321DCCB9D50328D31B40CD774848`; no source/build/Unity action.

> **Stage-container seam result:** `CLIENT-FORMAL-KERNEL-STAGE-CONTAINER-SEAM-001 / FOCUSED_TEST_PASS / STAGE_CONTAINER_SEAM_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / TEST_FIRST_MISSING_SEAM_RED / UNITY_COMPILE_0 / FOCUSED_5_5 / RELATED_39_39 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Immutable world content, defensive copies and atomic projection are focused ready; mutable runtime/snapshot behavior remains separate and unchanged.

> **Stage-container seam contract result:** `GOVERNANCE-S0-FORMAL-STAGE-CONTAINER-SEAM-CONTRACT-001 / ANALYSIS_COMPLETE / IMMUTABLE_CONTAINER_API_FROZEN / DEFENSIVE_COPY_AND_FAIL_CLOSED_FROZEN / SOURCE_ORDER_AND_COMMENT_CLASSIFICATION_FROZEN / STAGE_CONTAINER_SEAM_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY`. Exact three-type BCL API, atomic projection, comment metadata and runtime/snapshot separation are frozen; next0co.

> **Stage parser defaults result:** `CLIENT-CPP-STAGE-CAMPAIGN-PARSER-DEFAULTS-ALIGNMENT-001 / FOCUSED_TEST_PASS / STAGE_CAMPAIGN_PARSER_DEFAULTS_ALIGNED / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / TEST_FIRST_2_FAIL_2_PASS / UNITY_COMPILE_0 / FOCUSED_4_4 / RELATED_30_30 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Failed optional parses now preserve C++ initialized defaults `-1/1`; valid/order/duplicate behavior and all excluded systems are unchanged.

> **Stage-container boundary result:** `GOVERNANCE-S0-FORMAL-STAGE-CONTAINER-BOUNDARY-001 / ANALYSIS_COMPLETE / UNITY_CONTENT_RUNTIME_SPLIT_MAPPED / ORDER_AND_DUPLICATE_BEHAVIOR_MAPPED / PARSER_DEFAULT_FIRST_DIFFERENCE_CONFIRMED / LOADER_DEFAULT_ALIGNMENT_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_BUILD_OR_UNITY`. Static content is separate from loader/progression/wave buffers/snapshot. Unity failed `out` writes zero where C++ preserves initialized `-1/1`; next0cm selected before immutable containers.

> **Shared stage-spawn value owner result:** `CLIENT-FORMAL-KERNEL-STAGE-SPAWN-VALUE-SHARED-OWNER-001 / FOCUSED_TEST_PASS / SHARED_STAGE_SPAWN_VALUE_OWNER_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / PACKAGE_0_7_0_DIRECT_AND_LOCKED_ARTIFACT_PASS / UNITY_COMPILE_0 / UNITY_PACKAGE_7_7 / RELATED_27_27 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. One unchanged source/GUID is Server Core-owned; Unity still consumes it in loader→DTO→value→task/factory→world order. No adapter/gameplay/hash/marker changed.

> **Stage-spawn cross-consumer result:** `GOVERNANCE-S0-STAGE-SPAWN-CROSS-CONSUMER-CONTRACT-001 / ANALYSIS_COMPLETE / UNITY_ORDER_MAPPED / GOLDEN_CORPUS_FROZEN / DUAL_DIGEST_PASS / STAGE_SPAWN_SHARED_OWNER_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_PRODUCTION_SOURCE_CHANGE`. Fourteen lines/1269 bytes and SHA `EF0DE76F5DE89D3CE429E80D9F26CB2252DBE90EA77D80EB24A0A2F3F4C03591`; no Client run/source move.

> **Stage-spawn value seam result:** `CLIENT-FORMAL-KERNEL-STAGE-SPAWN-VALUE-SEAM-001 / FOCUSED_TEST_PASS / STAGE_SPAWN_VALUE_SEAM_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / UNITY_COMPILE_0 / FOCUSED_4_4 / RELATED_23_23 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Eight immutable scalars and DTO/normal/reserve adapters are focused ready; warmed mapping is 0 B and mutable scratch is gone.

> **Content model closure result:** `GOVERNANCE-S0-FORMAL-CONTENT-MODEL-CLOSURE-001 / ANALYSIS_COMPLETE / CONTENT_GRAPH_LAYERED / FULL_CATALOG_CLOSURE_SELECTED / ORDERED_MIGRATION_CUTS_FROZEN / STAGE_SPAWN_VALUE_SEAM_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_CHANGE`. Full catalog plus transitive validation and ordered content cuts are frozen. The mutable Results reserve scratch is the first spawn-value blocker; Queue0ci selected.

> **Content producer/binding result:** `GOVERNANCE-S0-FORMAL-CONTENT-PRODUCER-BINDING-BOUNDARY-001 / ANALYSIS_COMPLETE / PRODUCER_OWNERSHIP_MAPPED / NO_REAL_SERVER_ONLY_PRODUCER / PREWORLD_COMPARISON_POINT_DEFINED / CONTENT_MODEL_CLOSURE_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_CHANGE`. Server has no real five-domain producer; current test digests are fixtures only. Actual comparison belongs at the future formal factory before construction/allocation/RNG. Queue0ch selected.

> **Server formal content identity value result:** `S0-SERVER-FORMAL-CONTENT-IDENTITY-VALUE-001 / FOCUSED_TEST_PASS / SERVER_FORMAL_CONTENT_IDENTITY_VALUE_READY / GOVERNANCE_CLOSED / SERVER_ONLY / CLIENT_INTEGRATION_REQUIRED / DEBUG_RELEASE_0_WARN_0_ERROR / SERVER_CHAIN_PASS / NO_NETWORK_HOST_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Server StartBarrier now requires domain/schema/sha256 rule/catalog/stage/build/world-factory values. Client remained untouched; no digest is yet bound to actual loaded content.

> **Canonicalization contract result:** `GOVERNANCE-S0-FORMAL-CONTENT-CANONICALIZATION-CONTRACT-001 / ANALYSIS_COMPLETE / CANONICAL_IDENTITY_LAYERS_FROZEN / DUPLICATES_FAIL_CLOSED / SHA256_DOMAIN_VALUE_SELECTED / SERVER_IDENTITY_VALUE_PACKAGE_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_CHANGE`. Semantic manifest/order/normalization/version layers and verify-before-mutation are frozen; Queue0cf selected and subsequently Server-focused closed.

> **Content/factory identity boundary result:** `GOVERNANCE-S0-FORMAL-CONTENT-FACTORY-IDENTITY-BOUNDARY-001 / ANALYSIS_COMPLETE / IDENTITY_TOKENS_UNBOUND / UNITY_BOOTSTRAP_ORDER_MAPPED / CANONICALIZATION_CONTRACT_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_CHANGE`. Current Client/Server rule/catalog/stage values are unbound tokens; build and formal-factory identities are absent. The required order is resolve immutable Unity content → verify all identities → construct/mutate world. Queue0ce selected.

> **World-bootstrap factory seam result:** `CLIENT-FORMAL-KERNEL-WORLD-BOOTSTRAP-FACTORY-SEAM-001 / FOCUSED_TEST_PASS / WORLD_BOOTSTRAP_FACTORY_SEAM_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / UNITY_COMPILE_0 / FOCUSED_4_4 / RELATED_114_114 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Exact current bootstrap behavior is explicit and Client-owned; it is not a shared formal factory and binds no content/stage/AI identity.

> **Atomic-result boundary result:** `GOVERNANCE-S0-FORMAL-WORLD-ATOMIC-RESULT-BOUNDARY-001 / ANALYSIS_COMPLETE / TERMINAL_WORLD_DISCARD_BOUNDARY_DEFINED / IMMUTABLE_RESULT_DEFERRED / WORLD_BOOTSTRAP_FACTORY_SEAM_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_CHANGE`. Failed S0 worlds are terminal/discarded and never retried; rollback remains S3/S5; final completed-result schema is premature; next0cc.

> **Full-return commit seam result:** `CLIENT-FORMAL-KERNEL-FULL-RETURN-COMMIT-SEAM-001 / FOCUSED_TEST_PASS / FULL_RETURN_COMMIT_SEAM_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / UNITY_COMPILE_0 / FOCUSED_3_3 / RELATED_110_110 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Logic-only host publication now requires a complete tail return. Failed world rollback/discard, immutable result schema, complete shared world and marker remain pending.

> **Formal snapshot/marker readiness result:** `GOVERNANCE-S0-FORMAL-SNAPSHOT-MARKER-READINESS-001 / ANALYSIS_COMPLETE / FORMAL_S0_PROOF_MATRIX_CLOSED / FULL_RETURN_COMMIT_SEAM_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_CHANGE`. Shared foundations are not a complete world/tick. Existing Client snapshot is S3 foundation only; hidden tick early returns and separate mutable checksum/history writes mean no immutable completed-tick result exists. Formal AI/event/marker gates remain.

> **Results reserve terminal integration result:** `CLIENT-CPP-RESULTS-RESERVE-TERMINAL-INTEGRATION-001 / FOCUSED_TEST_PASS / RESULTS_RESERVE_TERMINAL_INTEGRATION_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / UNITY_COMPILE_0 / FOCUSED_4_4 / RELATED_103_103 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Persistent full-domain teams, mode4 reserve-before-guard and exact success/failure writes are focused ready; marker remains false.

> **Results reserve terminal integration audit:** `GOVERNANCE-S0-RESULTS-RESERVE-TERMINAL-INTEGRATION-001 / ANALYSIS_COMPLETE / RESULTS_RESERVE_TERMINAL_INTEGRATION_SELECTED / GOVERNANCE_CLOSED / READ_ONLY`. Team0 is valid; two IDs persist in first-slot order; third teams are ignored; both alive pauses rather than resets; reserve success alone writes phase0/pending-1.

> **Results reserve transaction seam result:** `CLIENT-CPP-RESULTS-RESERVE-TRANSACTION-SEAM-001 / FOCUSED_TEST_PASS / RESULTS_RESERVE_TRANSACTION_SEAM_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / UNITY_COMPILE_0 / FOCUSED_2_2 / RELATED_101_101 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Direct seam aligns slot20..399, no-RNG gates, one Z RNG, per-entry partial commit and rest-conflict fail-closed behavior. It remains unreachable from terminal observation.

> **Results reserve boundary audit:** `GOVERNANCE-S0-RESULTS-RESERVE-TRANSACTION-BOUNDARY-001 / ANALYSIS_COMPLETE / RESULTS_RESERVE_TRANSACTION_SEAM_SELECTED / GOVERNANCE_CLOSED / READ_ONLY`. C++ per-entry partial commit, slot/data/RNG/entity/rest/committed order and Client owner/gap matrix are closed. Current StageSpawn cannot be called directly because it consumes extra X RNG and hard-codes side2.

> **Results activation-reset result:** `CLIENT-CPP-RESULTS-ACTIVATION-RESET-ALIGNMENT-001 / FOCUSED_TEST_PASS / RESULTS_ACTIVATION_RESET_ALIGNMENT_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / UNITY_COMPILE_0 / FOCUSED_2_2 / RELATED_94_94 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Only the existing table reset followed by live-guard reset was added; test-first0/2, final2/2+94/94, fresh SelfCheck and Server dual-configuration evidence pass. Scan/reserve/schema/host action remain unchanged.

> **Results terminal-alignment audit:** `GOVERNANCE-S0-RESULTS-TERMINAL-ALIGNMENT-SELECTION-001 / ANALYSIS_COMPLETE / RESULTS_ACTIVATION_RESET_ALIGNMENT_SELECTED / GOVERNANCE_CLOSED / READ_ONLY`. Full-domain observation is coupled to the absent mode-4 reserve transaction; phase-11 table/live-guard reset is the first dependency-closed correction.

> **Results outcome-host writer seam result:** `CLIENT-CPP-RESULTS-OUTCOME-HOST-WRITER-SEAM-001 / FOCUSED_TEST_PASS / RESULTS_OUTCOME_HOST_WRITER_SEAM_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / UNITY_COMPILE_0 / FOCUSED_2_2 / RELATED_92_92 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Dedicated terminal observer and Results navigation writers are fully regressed; behavior/fields/schema/reserve/marker remain frozen.

> **Results outcome/host seam audit:** `GOVERNANCE-S0-RESULTS-OUTCOME-HOST-SEAM-SELECTION-001 / ANALYSIS_COMPLETE / RESULTS_OUTCOME_HOST_WRITER_SEAM_SELECTED / GOVERNANCE_CLOSED / READ_ONLY`. Exact ownership/projection groups are mapped. C++ full-domain observation, reserve spawn and live-guard reset differences are explicit later gates.

> **Results scene host-tick result:** `CLIENT-CPP-RESULTS-SCENE-HOST-TICK-ALIGNMENT-001 / FOCUSED_TEST_PASS / RESULTS_SCENE_HOST_TICK_ALIGNMENT_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / UNITY_COMPILE_0 / FOCUSED_3_3 / RELATED_90_90 / SELFCHECK_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Test-first `0/3`; final focused `3/3`, related `90/90`, fresh SelfCheck and Server dual-configuration regressions pass. Results math/schema/package remain frozen.

> **Results host/kernel audit:** `GOVERNANCE-S0-CUT-G-RESULTS-HOST-KERNEL-BOUNDARY-001 / ANALYSIS_COMPLETE / RESULTS_HOST_KERNEL_BOUNDARY_MAPPED / RESULTS_SCENE_HOST_TICK_ALIGNMENT_SELECTED / GOVERNANCE_CLOSED / READ_ONLY`. C++ host-global Results/full-tick order, Unity early-return first difference, reserve bridge and projection coupling are mapped.

> **Roster/label shared-owner result:** `CLIENT-FORMAL-KERNEL-ROSTER-LABEL-SHARED-OWNER-001 / FOCUSED_TEST_PASS / SHARED_ROSTER_LABEL_OWNER_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / PACKAGE_0_6_0_DIRECT_AND_LOCKED_ARTIFACT_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Single source/GUID、58-line exact corpus、direct+locked0.6.0、Unity compile0、11/11+87/87+SelfCheck and Server dual-configuration regressions pass.

> **Roster/label shared-owner lifecycle anchor:** `CLIENT-FORMAL-KERNEL-ROSTER-LABEL-SHARED-OWNER-001` = `CLIENT_INTEGRATION_REQUIRED / FOCUSED_TEST_PASS / SHARED_ROSTER_LABEL_OWNER_READY`.

> **Roster/label contract:** 58 lines，SHA `F4DB5DA03345C08EC1854F67B2146EC47CE2E9EF22BF2290036AF11CABF89FD2`，dual digest pass；no Client source/build/Unity action。

> **Roster/label bootstrap seam:** `CLIENT-FORMAL-KERNEL-ROSTER-LABEL-BOOTSTRAP-SEAM-001 / FOCUSED_TEST_PASS / ROSTER_LABEL_BOOTSTRAP_SEAM_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / SOURCE_SEAM_ONLY / S0_NOT_VERIFIED`. Compile0、5/5、10/10、87/87 and fresh SelfCheck pass；package/results/root/marker unchanged。

> **Roster/label seam lifecycle anchor:** `CLIENT-FORMAL-KERNEL-ROSTER-LABEL-BOOTSTRAP-SEAM-001` = `CLIENT_INTEGRATION_REQUIRED / FOCUSED_TEST_PASS / ROSTER_LABEL_BOOTSTRAP_SEAM_READY`.

> **Cut F boundary result:** `GOVERNANCE-S0-CUT-F-ROSTER-RESULTS-BOUNDARY-001 / ANALYSIS_COMPLETE / ROSTER_LABEL_BOOTSTRAP_SEAM_SELECTED / RESULTS_HOST_SPLIT_REQUIRED / GOVERNANCE_CLOSED / READ_ONLY`. Direct Client/C++ evidence mapped slot/label versus host-loop result state；no source/build/Unity action。

> **Cut E shared-owner result:** `CLIENT-FORMAL-KERNEL-WORLD-SCALAR-SHARED-OWNER-001 / FOCUSED_TEST_PASS / SHARED_WORLD_SCALAR_OWNER_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / PACKAGE_0_5_0_DIRECT_AND_LOCKED_ARTIFACT_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Single source/GUID、direct+locked0.5.0、compile0、10/10+83/83+SelfCheck and Server dual-config regressions pass.

> **Cut E scalar contract:** `GOVERNANCE-S0-WORLD-SCALAR-CROSS-CONSUMER-CONTRACT-001 / ANALYSIS_COMPLETE / GOLDEN_CORPUS_FROZEN / DUAL_DIGEST_PASS / GOVERNANCE_CLOSED / READ_ONLY`. 18 lines、SHA `1A1C2E...E554` and field order `9+10+4+8+22` pass；no Client source action。

> **Cut E scalar seam:** `CLIENT-FORMAL-KERNEL-WORLD-SCALAR-SEAM-001 / FOCUSED_TEST_PASS / WORLD_SCALAR_SEAM_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / S0_NOT_VERIFIED`. Unity compile0、focused5/5、related83/83 and fresh SelfCheck pass.

> **Cut E scalar seam lifecycle anchor:** `CLIENT-FORMAL-KERNEL-WORLD-SCALAR-SEAM-001` = `CLIENT_INTEGRATION_REQUIRED / FOCUSED_TEST_PASS / WORLD_SCALAR_SEAM_READY`.

> **Cut E boundary audit:** `GOVERNANCE-S0-CUT-E-WORLD-CORE-BOUNDARY-001 / ANALYSIS_COMPLETE / WORLD_SCALAR_SEAM_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_CHANGE`. Broad Runtime root、mutable content/catalog and entity moves were rejected from direct dependencies.

> **Cut E scalar lifecycle anchor:** `CLIENT-FORMAL-KERNEL-WORLD-SCALAR-SHARED-OWNER-001` = `CLIENT_INTEGRATION_REQUIRED / FOCUSED_TEST_PASS / SHARED_WORLD_SCALAR_OWNER_READY`.

> **Cut D shared-owner result:** `CLIENT-FORMAL-KERNEL-REST-STATE-SHARED-OWNER-001 / FOCUSED_TEST_PASS / SHARED_REST_STATE_OWNER_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. One RuntimeRestStore source/GUID is Server-owned at `0.4.0`; direct/locked artifact, Unity compile0、1/1+26/26+17/17+21/21、fresh SelfCheck and Server dual-configuration regressions pass.

> **Cut D lifecycle anchor:** `CLIENT-FORMAL-KERNEL-REST-STATE-SHARED-OWNER-001` = `CLIENT_INTEGRATION_REQUIRED / FOCUSED_TEST_PASS / SHARED_REST_STATE_OWNER_READY`.

> **Client authorization policy:** `GOVERNANCE-S0-S9-STANDING-CLIENT-AUTHORIZATION-002` remains active. Queue-selected packages proceed after independent pre-change Task/Change；retained stops continue. Queue0dk-a `CLIENT-CPP-FRAME-MULTIVALUE-PARSER-ALIGNMENT-001` is READY.

> **Goal metadata correction:** Goal `01a0324a-1bc3-7702-9787-b5e1ccff5111` was actually blocked before the current resumption audit. After G-24 closes, the executing session replaces/resumes it with the capability-level objective and G-22 standing authorization.

> **Latest Cut D seam result:** `CLIENT-FORMAL-KERNEL-REST-STATE-SEAM-001 / FOCUSED_TEST_PASS / CUT_D_SEAM_READY / GOVERNANCE_CLOSED / CLIENT_INTEGRATION_REQUIRED / S0_NOT_VERIFIED`. Queue `0bf`已关闭；57-line digest/all hashes、dense/sparse order+warmed0B、Unity compile、related38/38（S0 8/8+lockstep9/9）、extra21/21、fresh SelfCheck与Server Release通过。Source move、package/version、snapshot schema/recovery、formal AI与marker仍冻结；后续shared-owner须新具名授权。

> **Latest rest-vector result:** `GOVERNANCE-S0-REST-STATE-CROSS-CONSUMER-CONTRACT-001 / ANALYSIS_COMPLETE / GOLDEN_CORPUS_FROZEN / DUAL_GENERATOR_AND_DIGEST_PASS / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_CHANGE / S0_NOT_VERIFIED`. 57 lines and SHA-256 `E10CF6D96104F69F574AA73503AFF9F03C0AD85633E66AE02054A435D86434E8` pass two generators, document extraction and the closed Client seam test. No ACTIVE/READY row remains; later source movement is separately gated.

> **Latest Cut D audit:** `GOVERNANCE-S0-CUT-D-REST-CHECKSUM-PROJECTION-BOUNDARY-001 / ANALYSIS_COMPLETE / REST_CORE_BCL_ONLY / REVERSE_PROJECTION_DEPENDENCIES_CONFIRMED / SEAM_FIRST_SELECTED / GOVERNANCE_CLOSED / READ_ONLY / NO_SOURCE_CHANGE / S0_NOT_VERIFIED`. It selected vector-before-seam; both later prerequisites have now closed. RuntimeRestStore source movement remains separately gated.

> **Cut C result:** `CLIENT-FORMAL-KERNEL-SLOT-LIFECYCLE-SHARED-OWNER-001 / FOCUSED_TEST_PASS / SHARED_SLOT_LIFECYCLE_OWNER_READY / GOVERNANCE_CLOSED / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`. Queue `0bc`已关闭且全回归通过；后续Cut D boundary/vector/seam也已关闭。当前无READY项，rest source move仍未授权。

> **Independent Tools result:** `WEB-PREVIEW-PRESENTATION-002 / RUNTIME_PENDING / BUILD_PASS / FOCUSED_TEST_PASS / PRESENTATION_ONLY`. DatSkillFlow 主预览已按 2.8 本地 render/presentation 参考接入30/60/120Hz、精确坐标与continuity gates，并分离authority overlay。build `20260830084617618-18ef901e469444d9b80e355a62838458`；focused23/23、unit315+1skip、nonbuild integration78/78、manifest/server25/25、Ledger PASS。DAT、Native CLI、server save、Unity Client、C++ battle和30Hz逻辑未改；localhost浏览器权限拒绝，E4待用户观察，不能标VERIFIED。Record：`docs/ai/CHANGE-RECORDS/WEB-PREVIEW-PRESENTATION-002.md`。

> **Cut C seam prerequisite:** `CLIENT-FORMAL-KERNEL-SLOT-LIFECYCLE-SEAM-001 / FOCUSED_TEST_PASS / SLOT_LIFECYCLE_SEAM_READY / GOVERNANCE_CLOSED`. Queue `0bb`关闭了provisional claim/rest side effect/commit/rollback seam；随后独立授权的Queue `0bc`现已关闭shared-owner move。两者均未实现formal AI、snapshot/recovery、marker promotion或phase verification。

> **Latest Server-only result:** `S2-SERVER-INDIVIDUAL-DEPARTURE-OWNERSHIP-REQUEST-001 / FOCUSED_TEST_PASS / SERVER_SINGLE_SLOT_OWNERSHIP_REQUEST_READY / ClientImpact=NONE`. One witness slot transitions safely; other Windows slot/pending input remain; AI-owned frame stops fail-closed. Client remained frozen.

> **Latest Server governance prerequisite:** `GOVERNANCE-GP09-DEPARTURE-OWNERSHIP-REQUEST-SELECTION-001` selected Queue `2.10`; that Server source row has now closed at focused-test pass as recorded above. Client remains frozen; formal AI/frame advance/recovery/wire remain excluded.

> **Latest Server-only result:** `S2-SERVER-INDIVIDUAL-DEPARTURE-WITNESS-001 / FOCUSED_TEST_PASS / SERVER_INDIVIDUAL_DEPARTURE_WITNESS_READY / ClientImpact=NONE`. Recorded-entry identity, caller monotonic admission time and stable full-duration witness pass. Client remained frozen; no timer/ownership/AI/recovery/wire/actor was implemented.

> **Latest Server governance prerequisite:** `GOVERNANCE-GP09-INDIVIDUAL-DEPARTURE-WITNESS-SELECTION-001` selected the Server-only caller-timed witness; that source row has now closed at focused-test pass as recorded above. Client remains frozen; no AI/ownership/recovery/wire action is authorized.

> **Latest Server-only result:** `S1-SERVER-INDIVIDUAL-DEPARTURE-ADMISSION-001 / FOCUSED_TEST_PASS / SERVER_INDIVIDUAL_DEPARTURE_ADMISSION_READY / ClientImpact=NONE`. Command/journal, per-slot participation, accepted-input-safe successor and Windows two-to-one admission pass. Client remained frozen; no 30-second AI, wire, recovery or actor was implemented.

> **2026-08-30 GP-09 Server contract:** `GOVERNANCE-GP09-INDIVIDUAL-DEPARTURE-COMMAND-CONTRACT-001` selected the Server-only admission row; that row has now subsequently closed at focused-test pass as recorded above. Client remains frozen; 30-second AI, wire, recovery and actor are not included.

> **2026-08-30 active governance package:** `GOVERNANCE-GP09-INDIVIDUAL-DEPARTURE-COMMAND-CONTRACT-001 / ANALYSIS_IN_PROGRESS / NO_SOURCE_CHANGE / PHASE_STATUS_UNCHANGED`. 只读冻结per-slot departure command与input-participation合同；不创建DTO，不改/编译/测试Client，不实现timer/AI/recovery/transport。

> **2026-08-30 GP-09 one-shot audit result:** `GOVERNANCE-GP09-ORIGINAL-ONESHOT-CONSUMER-MAPPING-001 / ANALYSIS_COMPLETE / MASK_A_FE_AND_MASK_B_1E_CONSUMERS_MAPPED / OR_ONCE_FIXED_ORDER_CONFIRMED / GP09_EVIDENCE_COMPLETE / GOVERNANCE_CLOSED / NO_SOURCE_CHANGE`. 原版offsets 10/12是共享F1～F9/feature events；F4不是个人离场。未来room/session command仍需独立合同/实现；未改/编译/测试Client或改变阶段。

> **2026-08-30 StageSpawn rest correction result:** `CLIENT-CPP-STAGE-SPAWN-REST-ALIGNMENT-001 / FOCUSED_TEST_PASS / STAGE_SPAWN_REST_ALIGNMENT_READY / GOVERNANCE_CLOSED / USER_AUTHORIZED / S0_NOT_VERIFIED`. 成功StageSpawn现按C++清ARest、VRest victim row和attacker column；冲突lease路径保持零mutation、lease有效、无pool leak、无成功allocation event。Unity compile `error CS=0`、focused `2/2`、fresh SelfCheck、S0 `8/8`和lockstep `9/9`通过；普通registration/pass未改，S0/S5/marker未晋升。

> **2026-08-30 StageSpawn rest authority audit:** `GOVERNANCE-S0-STAGE-SPAWN-REST-ALIGNMENT-PREREQUISITE-001 / ANALYSIS_COMPLETE / CPP_CLEAR_ON_SUCCESS_AUTHORITY / UNITY_PRESERVE_MISMATCH_CONFIRMED / GOVERNANCE_CLOSED / NO_SOURCE_CHANGE`. 审计关闭时识别出`CLIENT-CPP-STAGE-SPAWN-REST-ALIGNMENT-001`门禁；该门禁随后已focused关闭。审计本身未改/运行Client，不能冒充后续实现证据。

> **2026-08-30 Cut C golden-journal result:** `GOVERNANCE-S0-SLOT-LIFECYCLE-CROSS-CONSUMER-CONTRACT-001 / ANALYSIS_COMPLETE / GOLDEN_JOURNALS_FROZEN / DUAL_GENERATOR_AND_DIGEST_PASS / GOVERNANCE_CLOSED / NO_SOURCE_CHANGE`. PowerShell、JavaScript与document extraction均得到48行及SHA-256 `22F25272BCD5E4616AFB92B50A6E080E546B6AA53A11DAB96647387F1C4381B7`。后续StageSpawn authority audit也已关闭；Client correction/seam/source仍未授权。

> **2026-08-30 Cut C identity audit result:** `GOVERNANCE-S0-CUT-C-SLOT-LIFECYCLE-IDENTITY-001 / ANALYSIS_COMPLETE / ALLOCATION_ORDER_MAPPED / FORMAL_ALLOCATION_EPOCH_DEFINED / CUT_C_SEAM_CLIENT_GATED / GOVERNANCE_CLOSED / NO_SOURCE_CHANGE`. Authority400 `0/20/50` ascending first-free order is mapped; C++ has no native generation; formal cross-runtime witness is `(slot, allocationEpoch)` and Unity Generation remains local lease safety. Subsequent vector and StageSpawn authority prerequisites are now closed; no Client source action was authorized.

> **2026-08-30 FrameInput shared-owner result:** `CLIENT-FORMAL-KERNEL-FRAME-INPUT-SHARED-OWNER-001 / CLIENT_INTEGRATION_REQUIRED / FOCUSED_TEST_PASS / SHARED_FRAME_INPUT_OWNER_READY / GOVERNANCE_CLOSED`. 单一source/GUID现由Server-owned `Runtime/Abstractions`持有；`0.2.0` direct/locked-artifact、Unity2/2+48/48+8/8+9/9+SelfCheck、Server Debug/Release和双Ledger通过。Marker仍false，S0/S5仍非VERIFIED。

> **2026-08-30 FrameInput seam result:** `CLIENT-FORMAL-KERNEL-FRAME-INPUT-SEAM-001 / CLIENT_INTEGRATION_REQUIRED / FOCUSED_TEST_PASS / FRAME_INPUT_SEAM_READY / GOVERNANCE_CLOSED`. Dual-repository Task/Change Records preceded all Client script edits. Public value/hash、Client capture、reusable preallocation与dense trace已分离；Unity compile0、seam4/4、related44/44、S0 8/8、existing9/9、fresh SelfCheck、warmed0B和Ledger通过。该seam包本身未移动source；后续shared-owner Cut B已按独立授权focused关闭。Cut C seam/source、formal AI与marker promotion仍分别受新门禁。

> **2026-08-30 shared RNG owner result:** `CLIENT-FORMAL-KERNEL-DETERMINISTIC-RNG-SHARED-OWNER-001 / CLIENT_INTEGRATION_REQUIRED / FOCUSED_TEST_PASS / SHARED_RNG_OWNER_READY`. The single production source/GUID is now Server-owned and consumed by Unity/.NET. Frozen vectors, direct/artifact consumers, Unity1/1、S0 8/8、existing9/9、fresh SelfCheck和Server Debug/Release均通过。Formal marker仍false，S0/S5仍非VERIFIED；Cut B后来已由独立授权完成，Cut C及formal AI仍需各自新授权。

> **最新源码包结果（2026-08-30）：** `S0-REAL-ENTITY-TEN-DOMAIN-CONTINUITY-001 / FOCUSED_TEST_PASS / CLIENT_TEST_ONLY / TEN_DOMAIN_CONTINUITY_READY / S0_NOT_VERIFIED`。MCP new1/1、S0 8/8、existing9/9、fresh self-check PASS、error CS0；只改一个Editor test文件，未改production runtime或Scene/资源。

> **最近治理包（2026-08-30）：** Server `GOVERNANCE-S0-FORMAL-KERNEL-NEXT-PACKAGE-SELECTION-001 / ANALYSIS_COMPLETE / S0_TEST_PACKAGE_SELECTED / CLOSED`。完整shared Kernel源码因UnityEngine/profiling/presentation/LF2依赖与打包拓扑未闭合而非READY；已选择当前S0 test-only continuity包，S0/S5阶段状态不变。

> **最新S0 Client包结果（2026-08-30）：** `S0-WITNESS-001 / FOCUSED_TEST_PASS / CLIENT_S0_WITNESS_READY / S0_NOT_VERIFIED`。Unity MCP连接唯一实例`gameplay-ability-system-for-unity@b1b02287`；S0 `7/7`与existing lockstep `9/9`均0 failed/skipped，fresh self-check为PASS，Console `error CS`为0。本轮未新增Client源码diff、未保存Scene；下一门槛是独立formal shared-Kernel/C++ domain mapping包。

> 生成日期：2026-08-24  
> 最后更新：2026-09-02
> 用途：将旧 Codex 任务 `01a02f58-c229-7830-a50b-7406c1d7d061` 最近三天有效事实迁移到当前持续目标；后续不依赖旧会话。  
> 证据口径：本文件区分“已观察/已验证”“用户明确决定”“推断/待验证”。它不取代 C++ release 的 battle authority，也不把历史 self-check 写成完整 C++ 对齐证书。
> 最近实测更新：2026-08-30，`S0-WITNESS-001` 已取得 `FOCUSED_TEST_PASS / CLIENT_S0_WITNESS_READY / S0_NOT_VERIFIED`；本轮通过MCP运行现有实现，没有新增Unity Client源码修改，也不把S0写成`VERIFIED`。

## 1. 当前结论

- **当前用户主线是 NTSD 2.8-Logan 战斗重新对齐**：先维护 `Assets/NTSD/Docs/ntsd28-logan-vs-unity-battle-alignment.md` 的完整差异矩阵，再按具名 ID 建立独立 Task/Change；当前只完成 inventory，没有 C#/Scene/Prefab/DAT/资源实现或 2.8 双端 trace。
- **当前 Client 仓库**是 `I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity`，当前分支为 `NTSD_2.8_C++`；Unity 项目版本以 `ProjectSettings/ProjectVersion.txt` 为准。工作树中的既有修改和未跟踪文件属于用户工作，不能清理、回退、批量格式化或顺手提交。
- **Server-first 是保留的历史/并行主线，不是当前用户任务**：其既有 package 状态和证据继续有效，但当前不得让 Server queue 自动抢占 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，也不得用旧 30 Hz、旧 Client freeze 或 generic TestKernel 覆盖本轮 2.8 对齐决定。用户再次明确恢复 Server 时，再读取 Server workflow/queue。
- **最新 held-only Server 结果（2026-08-29，优先于下方旧 pending/active 表述）：** 用户指定的 Server `NTSD28-ORIGINAL-ONLINE-LOCKSTEP-EVIDENCE-001.md` 已作为 `S-PROTO-001` 与 human missing carry 决议完整执行。`S1-SERVER-FORMAL-FRAME-INPUT-CONTRACT-001` 已关闭为 `FOCUSED_TEST_PASS / SERVER_HUMAN_AUTHORITY_INPUT_READY / CLIENT_PAUSED / S1-S2-PREIMPLEMENTATION`：原版稀疏 bit、deep-immutable held-only submission、Android 1 / Windows 1～2 / room 20 human ownership、稳定 1/2/8/20 聚合、all-released baseline、Server locked edge、deadline neutral 与 held carry 0 均通过 test-first、focused、Debug/Release 十项目 `0/0`、full Server tests、no-network local host 和治理校验。没有执行任何 Client 动作，也未实现 formal Kernel AI/state hash、numeric grace/deadline/delay、ownership transfer、snapshot/history recovery、wire/transport 或 S1/S2 `VERIFIED`。
- **当前逐项产品决策（2026-08-29）：** Server `DECISIONS/PENDING-ONLINE-GAMEPLAY-POLICY-CONFIRMATIONS-001.md` 是单项确认清单；当前 `GP-01` 已按公开成熟帧同步资料修订为“固定30Hz与既有TargetTick/InputDelay语义，按每个Client动态调整冗余/补发，严格连续补帧、有界追帧、严重落后snapshot recovery、表现插值不反写逻辑、deadline后故障human slot neutral”，状态仍为`USER_CONFIRMATION_PENDING`。证据与不可照抄边界见 Server `AUDITS/KING-OF-GLORY-PUBLIC-FRAME-SYNC-EVIDENCE-001.md`；这不是源码授权。
- **阶段治理结构（2026-08-29）：** `GOVERNANCE-S0-S9-STAGE-DOSSIERS-001` 已关闭为`FOCUSED_TEST_PASS / STAGE_DOSSIERS_READY / GOVERNANCE_CLOSED / PHASE_STATUS_FROZEN`。总设计保留跨阶段不变量；`server-lockstep-s0-s9-progress.md`是状态总账；[`ServerLockstepStages/README.md`](ServerLockstepStages/README.md)索引十份固定模板阶段档案；package Task Contract 与 Change Record 继续分别拥有包范围和实际改动证据。该治理包只调整文档与必要`.meta`，所有阶段状态保持原样，不能因档案创建而晋升`VERIFIED`。
- **最新 Server-first 同步（2026-08-25，优先于本文件下方旧的“最近完成”历史叙述）：** `S0-SERVER-BOOTSTRAP-NODE-IDENTITY-001 / FOCUSED_TEST_PASS / SERVER_BOOTSTRAP_NODE_IDENTITY_READY / S0_SERVER_FIRST_CORRECTION / CLIENT_PAUSED` 已使有效 `NodeId` 成为 no-network bootstrap/health 输出事实；`S1-SERVER-POLICY-VERSION-VALUE-001 / FOCUSED_TEST_PASS / SERVER_POLICY_VERSION_VALUE_READY / S1_SERVER_FIRST_PREIMPLEMENTATION / CLIENT_PAUSED` 已将用户确认 Model B/C1 的既有 policy identity 收束为 Protocol/BattleHost 强类型。后者保留`InputSubmission`无 policy field、activation journal/cursor/ack、next-tick `ServerProgress`和acknowledged-prefix gap/ready边界，绝不改变`TargetTick`/`InputDelayFrames`语义。两包均完成test-first、focused、Debug/Release十项目`0/0`、full Server tests、no-network local host、declared-path audit与Server final workflow/Ledger`31 / 51`；均不代表formal S0/S1/S2 `VERIFIED`，不授权 Client/wire/transport/snapshot/recovery/rebarrier/missing-input/battle-rule工作。
- **当前范围校正（优先于本文件后方的历史 Client validation 授权记录）：** 目前不得修改、导入、编译、测试、self-check 或回滚 Unity Client。S2 已完成的 Server-only protocol-owner 覆盖为 sequence/conflict、deadline boundary、ACK/confirmed range、redundancy、bounded ready buffer、inbound/downlink logical disorder 和 gap response；但 S2 正式关闭仍需真实 Client 连续消费、单客户端黑洞/极端抖动矩阵及用户批准的 grace/neutral/recovery 行为。`KernelAbstractionsAssemblyMarker.IsFormalBattleKernelImplemented=false`，所以也不得用 generic/TestKernel snapshot 冒充 S3/S5 formal Kernel 进展。这是阶段/范围门槛，不是总目标暂停。
- **当前 formal 输入边界（2026-08-29）：** held-only、Server-derived edge、all-released baseline 与 deadline zero-carry neutral 已由上述 Server-only 包落实，不得再次询问。仍待的是 Client capture/wire、formal Kernel AI/state hash、authority-frame-to-world tick mapping、ownership barrier 和 recovery；这些必须另建明确包，不能反写成当前包已完成。
- **前一完成 Server 输入值包（2026-08-25）：** `S1-SERVER-HUMAN-FRAME-INPUT-VALUE-001 / FOCUSED_TEST_PASS / SERVER_HUMAN_FRAME_INPUT_VALUE_READY / CLIENT-PAUSED / S1-PREIMPLEMENTATION / EXHAUSTIVE-REGRESSION-HARDENED`。上位设计与C++ release `InputHandler::poll`/`battle_bootstrap.cpp`共同支持并已实现不可变七action human held/pressed/released value、edge derivation/order、equality/hash与Server Protocol tests。test-first missing-type red evidence、全`128 × 128 = 16,384` pair regression、Debug/Release `0/0`、focused/full Server tests、no-network local host、declared-source content audit与final Ledger`22 / 72`均有证据。它不改Client、不绑定capture/tick mapping、不选择missing-input、AI、Kernel、transport/recovery或S1验证。
- **最近完成 Server policy 包（2026-08-25）：** `S1-SERVER-POLICY-ACTIVATION-SCHEDULE-001 / FOCUSED_TEST_PASS / SERVER_POLICY_ACTIVATION_SCHEDULE_READY / CLIENT-PAUSED / S1-PREIMPLEMENTATION`。用户确认Model B：`InputSubmission`不带per-submission `PolicyVersion`；Server room/session以target authority tick解析append-only future-effective activation schedule，并把resolved version写入现有immutable locked envelope history。已覆盖activation exact boundary、已接受future `TargetTick`不重算、locked history不回写、schedule ordering/terminal/contract mismatch拒绝以及room adapter顺序执行不变。test-first red、Debug/Release十项目`0/0`、full Server tests、no-network host、declared-source audit与Ledger`23 / 74`均通过。它不改Client、transport、battle rules、30 Hz、missing-input、InputDelayFrames、rebarrier、cross-version recovery或S1验证。
- **C++ battle-entry input 前置结论（2026-08-25，只读）：** Server [`S1-FORMAL-INPUT-BOOTSTRAP-CAPTURE-BOUNDARY-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S1-FORMAL-INPUT-BOOTSTRAP-CAPTURE-BOUNDARY-001.md) 证明battle bootstrap及首callback会清除key/prev/cooldown/history：tick 1的`poll`结果不会进入normal `apply_input`，normal human input消费只在更晚的`world.game_tick > 1` callback发生。因此all-released是C++ pre-history事实，但不能拿它推断generic `InitialAuthorityTick`、Client capture或StartBarrier formal mapping。
- **S1 policy binding 已确认且有 Server-only 证据（2026-08-25）：** Server [`PENDING-S1-POLICY-VERSION-BINDING-001.md`](../../../../NTSD_Server/docs/ai/DECISIONS/PENDING-S1-POLICY-VERSION-BINDING-001.md) 记录的Model B已由用户确认并由[`S1-SERVER-POLICY-ACTIVATION-SCHEDULE-001`](../../../../NTSD_Server/docs/ai/CHANGE-RECORDS/S1-SERVER-POLICY-ACTIVATION-SCHEDULE-001.md)实现：不向`InputSubmission`追加policy field，StartBarrier initial policy加Server-owned future-effective activation schedule按target authority tick解析，resolved policy写入immutable envelope history。已形成`TargetTick`或`InputDelayFrames`语义不得通过该包改变；需要变化须另建rebarrier/versioned contract。Client capture/wire、cross-boundary redundancy、reconnect/replay和stale disposition仍未定也未实现。
- **Cross-policy history consumer 门槛（2026-08-25，只读）：** Model B schedule已使Server authority history可含多个resolved policy version；但现有gap responder、ready buffer、ACK tracker仍exact-match initial contract policy。于是later-policy progress/envelope/gap range的canonical delivery、ACK、ready、reconnect/replay与failure witness尚未定义。详见[`S2-S3-CROSS-POLICY-HISTORY-CONSUMER-PREREQUISITE-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S2-S3-CROSS-POLICY-HISTORY-CONSUMER-PREREQUISITE-001.md)。不得通过放宽guard、加per-submission field、Client/transport/recovery或rebarrier代码绕过；需要新的版本化合同和独立Change Record。
- **已确认的 cross-policy C1 合同：** [`PENDING-S2-S3-CROSS-POLICY-HISTORY-DELIVERY-001.md`](../../../../NTSD_Server/docs/ai/DECISIONS/PENDING-S2-S3-CROSS-POLICY-HISTORY-DELIVERY-001.md) 现记录用户确认的activation journal/cursor独立immutable history fact、per-envelope resolved witness，以及仅在receiving side已确认journal prefix后允许mixed-policy frame range。它不授权Client、wire、transport或recovery实现。
- **最近 C1 Server 结果（2026-08-25）：** 用户授权的[`S2-SERVER-CROSS-POLICY-ACTIVATION-JOURNAL-001`](../../../../NTSD_Server/docs/ai/CHANGE-RECORDS/S2-SERVER-CROSS-POLICY-ACTIVATION-JOURNAL-001.md) 已在其Server Protocol/BattleHost与Server tests范围关闭为`FOCUSED_TEST_PASS / SERVER_CROSS_POLICY_JOURNAL_READY / CLIENT_PAUSED / S2-PREIMPLEMENTATION`：activation journal cursor/ack、`ServerProgress` next-tick policy语义和确认prefix保护的gap/ready边界已实现，Debug/Release十项目`0/0`、full Server tests、no-network host、C1 source audit与final Ledger `24 / 78`均通过。不得修改Client、wire、transport、snapshot/recovery、rebarrier、InputDelay或missing-input规则。
- **当前执行门禁（2026-08-29）：** 当前无 ACTIVE/READY Server 源码包。下一精确 gate 是 order 2 的 formal Kernel AI ownership/state-hash 与实测 numeric short-grace/deadline/delay 合同；重连仍依赖 S3 snapshot/history recovery。此状态不是 Server test 失败，也不授权 placeholder AI、hidden default、Client/wire/transport 或 generic recovery。
- **最新选包结论（2026-08-25）：** Server [`S0-S1-SERVER-FIRST-NEXT-SOURCE-AUDIT-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S0-S1-SERVER-FIRST-NEXT-SOURCE-AUDIT-001.md) 已完成。SessionId、NodeId与当前已实现的Model B/C1 PolicyVersion边界均已收束；当前无下一个可直接开工的Server源码包。该结论不是目标暂停或泛化授权请求：最早触发为`S-PROTO-001`的明确输入edge/capture合同，独立触发为`S-NET-001/002`，其余为已记录的Client/formal-Kernel/S3/S5 gate。收到命名决定后，应只更新相应Server queue row为READY并立即建立Record，不再重复索取“允许继续Server”的确认。
- **Server generic terminal-tick boundary（2026-08-25，只读）：** [`S1-S2-TERMINAL-AUTHORITY-TICK-BOUNDARY-AUDIT-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S1-S2-TERMINAL-AUTHORITY-TICK-BOUNDARY-AUDIT-001.md) 确认当前generic contract把`long.MaxValue`限定为terminal next cursor或exact empty ready range，未发现新的独立源码缺陷；当前工作树再次取得Debug `0/0`、Release Server self-hosted chain、no-network local host与Ledger`21 / 70`证据。它不等同于C++ tick mapping、history/recovery、formal Kernel、Client runtime、transport或阶段`VERIFIED`。
- **C++ release snapshot/RNG 前置结论（2026-08-25，只读）：** release `Makefile` 确认 live source set 包含 `simulation_tick_driver.cpp`、`game_tick.cpp` 和 `input_handler.cpp`。`InputHandler::snapshot()` 仅复制上一帧按键，`snapshot_phase210_table()` 仅保存结算/UI 表，并非已确认的 BattleWorld snapshot；`g_ntsd_rand_seed` 的 LCG 又在 input、game tick、collision、frame advance 中被广泛消费。因此未来 formal snapshot/recovery 必须覆盖 battle-world state、slot/generation、event cursor 与精确 RNG seed/call ordering，不能以 generic/TestKernel frame history 替代。本结论不授权实现 snapshot。
- **字段级前置清单：** Server 侧 [`S3-FORMAL-SNAPSHOT-PREREQUISITE-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S3-FORMAL-SNAPSHOT-PREREQUISITE-001.md) 记录必须 capture/restore 的 C++ release domain、明确排除项和 future implementation gate；它是分析文档，不是 snapshot 已实现的证据。
- **restore 顺序补充：** `spawn_at` 会 reset slot cooldown 行/列；因此 `s_arest`/`s_vrest` 和外部 battle globals 只能在所有 stable slot rehydrate 后恢复。详见同一 Server audit，仍不是 snapshot 已实现的证据。
- **全阶段证据矩阵：** Server 侧 [`S0-S9-FORMAL-READINESS-MATRIX-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S0-S9-FORMAL-READINESS-MATRIX-001.md) 将 S0-S9 的需求、当前证据、未闭合门槛和合法下一步逐项列出；它不把 Server-only 测试扩展成阶段 `VERIFIED`。
- **最近完成的 Server correction 是 `S2-SERVER-READY-BUFFER-HORIZON-001 / FOCUSED_TEST_PASS / SERVER_READY_BUFFER_HORIZON_READY / CLIENT-PAUSED / S2-CORRECTION`**：它让 generic `InMemoryAuthorityFrameReadyBuffer` 必须接收 caller-supplied、nonnegative future-envelope horizon；zero合法且无production jitter/delay default。non-late far envelope会在duplicate/conflict/capacity mutation前以 appended `RejectedFutureTickLimit` fail closed，避免远future tick占满有限buffer并拒绝near contiguous frame。invalid/zero/exact/over-limit/no-mutation/near-capacity/moving-window/disorder regressions通过；Debug/Release各10项目`0 warnings / 0 errors`、focused/full Server tests、`SequentialSingleWriter` no-network local run、final Ledger `19 / 70`和fixed-string scoped audit均通过。它不实现actual Client buffer、transport、ACK/retransmit、weak-network、history/recovery、battle rules、formal Kernel或S2 `VERIFIED`。
- **前一项 Server correction 是 `S1-SERVER-AUTHORITY-TICK-RANGE-001 / FOCUSED_TEST_PASS / SERVER_AUTHORITY_TICK_RANGE_READY / CLIENT-PAUSED / S1-CORRECTION`**：它定义 Protocol-owned addressable authority-frame tick range为`[0, long.MaxValue - 1]`，将`long.MaxValue`保留为final legal frame后的terminal next cursor。contract/barrier/input/envelope/frame/deadline均拒绝terminal frame fact；assembler在terminal cursor以`AuthorityTickExhausted`于任何missing-input/history mutation前fail closed，direct `InMemoryAuthorityRoom.TryAdvance(...)`也在frame construction前以`InMemoryAuthorityFrameRejection.AuthorityTickExhausted`返回`false`。final legal session/direct-room/journal/ready-buffer/progress/ACK cursor与terminal no-second-kernel/no-policy-fill/no-journal-append regressions通过；Debug/Release各10项目`0 warnings / 0 errors`、focused/full Server tests、`SequentialSingleWriter` no-network local run、final Ledger `18 / 70`和expanded fixed-string scoped audit均通过。它不宣称C++ `world.game_tick`同range，不选择30 Hz、battle rules、Client、transport、missing-input、formal Kernel或snapshot/recovery，更不是S1/S2 `VERIFIED`。
- **前一项 Server correction 是 `S1-SERVER-FUTURE-TARGET-BOUND-001 / FOCUSED_TEST_PASS / SERVER_FUTURE_TARGET_BOUND_READY / CLIENT-PAUSED / S1-CORRECTION`**：它让 generic `InMemoryAuthorityFrameAssembler` 和其 room adapter 都必须接收 caller-supplied、nonnegative future-target distance；zero合法且无production `InputDelayFrames` default。越界 target 会在 sequence/pending mutation 前以 appended `RejectedTargetBeyondFutureLimit` fail closed，比较使用 `TargetTick - NextAuthorityTick`，避免 `next + limit` overflow。negative/zero/exact/over-limit/no-mutation/sequence reuse/moving-bound/near-terminal/adapter与既有fixtures均通过；Debug/Release各10项目`0 warnings / 0 errors`、focused/full Server tests、`SequentialSingleWriter` no-network local run、final Ledger `17 / 70`和fixed-string scoped audit均通过。它不选择 production delay、deadline/missing-input policy、raw packet/MTU/bandwidth、Client、transport、battle rules、formal Kernel或snapshot/recovery，更不是S1/S2 `VERIFIED`。
- **最新 Server 前置审计是 `S1-CLIENT-SEQUENCE-RETENTION-PREREQUISITE-001`（只读、非实现包）**：`submissionsBySequence` 会在对应frame lock后继续保留，但 ACK、redundancy和client-reported confirmed cursor都没有定义安全retirement proof。因此不得把 future-target cap误称为完整sequence-memory cap，也不得擅自加锁帧删除、LRU/count cap、sequence reset、new disposition、reconnect或snapshot逻辑；先取得 lifecycle/replay/overload的版本化协议/产品决定。
- **最新 S3 history 前置审计是 `S3-AUTHORITY-HISTORY-RETENTION-PREREQUISITE-001`（只读、非实现包）**：generic `lockedFrames` 与room journal无限append，gap responder的单次response cap不是retention cap，且其索引假定完整initial-prefix仍可用。不得把它们称为`FrameHistoryRing`或S3 recovery，也不得擅自truncate、引入hidden ring、history-expired error、snapshot/replay或Client恢复；先取得formal Kernel、retained range/snapshot base和recovery disposition合同。
- **最新 S1 protocol-evolution 前置审计是 `S1-PROTOCOL-VERSION-EVOLUTION-PREREQUISITE-001`（只读、非实现包）**：`AuthorityFrameProtocolVersion=1`当前只是in-memory marker；exact-match与append-only source enum不是wire ABI/旧端兼容证明。不得私自bump version、添加serializer/capability/unknown fallback或Client/transport upgrade行为；先取得S5/S6的version meaning、bump rules、admission/upgrade/replay supersede合同和real serialization matrix。
- **前一项 Server correction 是 `S2-SERVER-REDUNDANCY-INGRESS-CAPACITY-001 / FOCUSED_TEST_PASS / SERVER_REDUNDANCY_INGRESS_CAPACITY_READY / CLIENT-PAUSED`**：它使`InMemoryRedundantSubmissionIngress`拥有positive、caller-supplied actual-entry cap；oversized matching window在任何assembler delegation/状态变更前以`RejectedServerEntryLimit` fail closed，at-cap window保持原有顺序/outcomes。Debug/Release各10项目`0 warnings / 0 errors`、focused/full Server tests、`SequentialSingleWriter` no-network local run、final Ledger `16 / 70`和fixed-string scoped audit均通过。它不选择production redundancy count、raw packet/MTU/bandwidth policy、Client、transport、deadline/missing-input policy、battle rules、formal Kernel或snapshot/recovery，更不是S2 `VERIFIED`。
- **前一项 Server correction 是 `S1-SERVER-MISSING-INPUT-PROVENANCE-001 / FOCUSED_TEST_PASS / SERVER_MISSING_INPUT_PROVENANCE_READY / CLIENT-PAUSED`**：它让 `AuthorityFrameInputSource` / `MissingInputFillReason` 只能构造六种一致、已知的 provenance pair，拒绝 cross-labelled 或 unknown enum；immutable envelope 与 generic missing-policy resolution使用同一个 Protocol owner。Debug/Release各10项目`0 warnings / 0 errors`、focused/full Server tests、`SequentialSingleWriter` no-network local run、final Ledger `15 / 70`和fixed-string scoped audit均通过。它没有选择任何 payload、grace、neutral、AI、disconnect/reconnect或产品规则，不实现 Client、battle rules、formal Kernel、snapshot/recovery、transport或数据库，更不是S0/S1 `VERIFIED`。
- **前一项 Server correction 是 `S1-SERVER-INITIAL-AUTHORITY-TICK-001 / FOCUSED_TEST_PASS / SERVER_INITIAL_AUTHORITY_TICK_READY / CLIENT-PAUSED`**：它修复了合法 non-zero `AuthorityFrameProtocolContract.InitialAuthorityTick` 与 existing StartBarrier/session/journal 的零起点矛盾。StartBarrier 现保存 validated immutable tick origin，session/journal从同一 origin 起步，adapter在构造前拒绝 protocol/barrier mismatch；negative tick、non-zero direct session、room+journal、adapter以及mismatch fail-closed fixtures通过。Debug/Release各10项目`0 warnings / 0 errors`、focused/full Server tests、`SequentialSingleWriter` no-network local run、final Ledger `14 / 70`和scoped audit均通过。它不实现 Client、battle rules、formal Kernel、snapshot/recovery、transport、数据库或缺失输入产品规则，更不是S0/S1 `VERIFIED`。
- **C++ tick-identity 前置结论（只读）**：C++ `reset_battle_runtime()` 将 `world.game_tick`/`input_phase`/`g_frame_toggle` 归零，而 `step_one_tick -> game_tick` 会在所有 battle passes 前递增/切换它们。因此 Server `InitialAuthorityTick` 只是 generic authority-history identity，不能自动等同于 C++ world tick；future formal schema必须显式验证 `authorityFrameTick`、`worldCompletedTick` 和 `nextAuthorityFrameTick` 的关系。该审计不实现 snapshot/recovery，不解除 Client freeze，也不改变S0～S9验证状态。
- **S1 policy-version input-binding gate（只读）**：设计明确 session-wide policy version 和 future effective tick，但没有规定它必须位于每个 `InputSubmission`。当前 contract/envelope/progress/gap有 version，submission/redundancy window没有；应先决定 per-submission binding 或 session/connection binding，以及旧version window在activation tick前后的处置。详见 Server [`S1-POLICY-VERSION-INPUT-BOUNDARY-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S1-POLICY-VERSION-INPUT-BOUNDARY-001.md)。在决定前不得私自新增DTO字段、stale-policy rejection、connection state、Client或transport行为。
- **formal checksum first-difference gate（只读）**：current generic Server kernel仅能返回aggregate checksum，mismatch没有domain、slot/generation、RNG或event cursor；它不能证明S0/S3所需ten-domain witness。future formal Kernel必须在同一completed tick boundary保留版本化domain list和first difference；C++ runtime views只是inventory线索。详见 Server [`S0-FORMAL-CHECKSUM-WITNESS-GATE-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S0-FORMAL-CHECKSUM-WITNESS-GATE-001.md)。不得以TestKernel/aggregate包装或Unity表现状态伪造此门槛。
- **前一项 S2 correction 是 `S2-SERVER-DISORDER-ACTION-VALIDATION-001 / FOCUSED_TEST_PASS / SERVER_DISORDER_ACTION_VALIDATION_READY / CLIENT-PAUSED`**：两个公开 disorder instruction constructor 的未知 enum fail-closed guard与聚焦fixtures已写入；Debug/Release各10项目`0 warnings / 0 errors`、inbound/downlink invalid-action fixtures及既有合法行为、`SequentialSingleWriter` no-network local run、final Ledger `13 / 70`和scoped audit均通过。它只修复非法 enum，不改变正常 Deliver/Drop、delivery order、budget、deadline、ACK、ready/gap、battle state 或网络范围；不是S2 `VERIFIED`。
- **最近完成的 Server 包是 `S2-SERVER-INMEMORY-SUBMISSION-DISORDER-001 / FOCUSED_TEST_PASS / SERVER_INBOUND_DISORDER_READY / CLIENT-PAUSED / S2-PREIMPLEMENTATION`**：它向既有redundancy ingress预编排投递或丢弃完整input windows的harness与聚焦fixtures已写入，覆盖inbound logical delay/drop/duplicate/reorder；Debug/Release各10项目`0 warnings / 0 errors`、BattleHost inbound-disorder checks、`SequentialSingleWriter` no-network local run、final Ledger `12 / 70`和scoped audit均通过。下一步仅可只读审计下一项Server-only包，先建新的Change Record再写源码；本包不能触发deadline/lock或选择`MissingInputPolicy`，不实现Client、packet/serialization/transport/retransmit/Jitter/weak-network runtime、snapshot/recovery、battle rules、数据库或公网，更不是S2 `VERIFIED`。
- **最近完成的 Server 包是 `S2-SERVER-AUTHORITY-FRAME-GAP-001 / FOCUSED_TEST_PASS / SERVER_GAP_RESPONDER_READY / CLIENT-PAUSED / S2-PREIMPLEMENTATION`**：missing authority-frame request、从现有assembler locked history取得有界顺序切片的Server responder与聚焦fixtures已写入；Debug/Release各10项目`0 warnings / 0 errors`、Protocol gap-request与BattleHost gap-responder checks、`SequentialSingleWriter` no-network local run、final Ledger `11 / 68`和scoped audit均通过；它也不是S2 `VERIFIED`。
- **最近完成的 Server 包是 `S2-SERVER-INPUT-REDUNDANCY-001 / FOCUSED_TEST_PASS / SERVER_INPUT_REDUNDANCY_READY / CLIENT-PAUSED / S2-PREIMPLEMENTATION`**：current+unconfirmed完整`InputSubmission` window、ordered ingress与聚焦测试已写入；首次`netstandard2.1` guard兼容性错误已受限修复，Debug/Release各10项目`0 warnings / 0 errors`、Protocol redundancy-window与BattleHost redundancy-ingress checks、`SequentialSingleWriter` no-network local run、final Ledger `10 / 64`和scoped audit均通过；它也不是S2 `VERIFIED`。
- **当前范围与下一步**：不实现真实Socket、数据库、Gateway、Matchmaker或公网，也不堆叠TestKernel。GP-09已focused推进到30秒witness和单slot ownership barrier；AI-owned frame正确停在`FormalAiKernelRequired`。Queue `0bb / CLIENT-FORMAL-KERNEL-SLOT-LIFECYCLE-SEAM-001`已focused关闭；Queue `0bc / CLIENT-FORMAL-KERNEL-SLOT-LIFECYCLE-SHARED-OWNER-001`需要新的具名授权，当前不得自动进入source move或formal AI。不得扩大成S0/S1/S2 VERIFIED。
- **Server 侧没有隐藏的 S0 编码余量**：`NTSD.Battle.Kernel.Abstractions` 明确标记 formal battle kernel 尚未实现；新增 `IBattleKernel`、snapshot/restore 或共享 runtime adapter 是设计中的 S5 shared-Kernel/独立进程工作，不能用它替代当前 S0 Client 的十域 witness 缺口。
- **已观察的当前环境事实**：`I:\GitHub\Unity_GAS\NTSD_Server` 已有独立 Git 仓库、`NTSD.Server.sln`、.NET 10 工程与 Server 自己的 Ledger/State/Handoff/Change Record；`dotnet --version` 在 `global.json` 下解析为 `10.0.400`。旧任务关于 sibling root 未热重载和 .NET 10 缺失的内容均是已解除的历史环境事实。
- **当前没有 S0 bootstrap、Unity编译或TestRunner硬环境 blocker**。唯一Editor实例下fresh compile/self-check、RNG 1/1、S0 8/8和existing lockstep 9/9均通过。C++ full-trace观察链路与完整shared formal Kernel仍是独立未解边界。
- **Unity S0 witness与RNG Cut A已完成focused验收，但阶段仍未VERIFIED**：真实test-only character的1 Server+2 Client world、连续journal、十named hashes、RNG/slot-generation typed first-difference已通过；`DeterministicRng`也已成为Server-owned UPM/.NET单一源码。仍缺完整formal Kernel的其余Cuts与C++ completed-tick/domain/event mapping；跨进程/跨runtime一致性仍属于S5。
- **当前战斗规则唯一 authority**：固定 SHA 的正式 NTSD 2.8-Logan EXE，以及 `source\README_SOURCE.md` 声明对应并进入 playable build closure 的 C++ live path。Unity/C#、NTSD 2.4、旧 self-check、性能报告和旧 Play Mode 都只能用于回归或定位，不能裁决当前规则。
- **当前 C++→Unity 主计划**：`NTSD28-UNITY-BATTLE-REALIGNMENT-001` 已完成静态 inventory，实施和双端 trace 尚未开始。旧 R1/R07/R08 只保留历史证据，不能继续其旧 Change 链。
- **HFR 不是独立 gameplay 规则**：新权威正常逻辑间隔为 33 ms、F5 为 3 ms；30/60/120 只属于 presentation sampling/interpolation，绝不能改变 DAT、tick、输入、碰撞、AI、opoint、RNG 或逻辑真值。
- **Web cadence 实验是独立诊断，不是 Unity HFR 或 C++ release parity 证书**：`WEB-CADENCE-001` 已 build、focused test、Native HTTP 生命周期验证，但仍 `RUNTIME_PENDING`，因为 Canvas 人工三栏视觉验收未完成。

- **S5 kernel / room exception boundary（2026-08-25，只读、非实现包）：** Server generic `IInMemoryAuthorityKernel.Advance(...)` 与 caller-owned missing-input policy没有异常、原子性或回滚合同；`InMemoryAuthorityRoom`会先append journal再推进kernel，session/adapter也不catch。因而一次throw可能留下locked/journaled frame而没有formal completed tick，不能安全以catch/retry/journal removal/generic rollback/fault logging“修复”。详见 Server [`S5-KERNEL-ROOM-FAULT-BOUNDARY-PREREQUISITE-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S5-KERNEL-ROOM-FAULT-BOUNDARY-PREREQUISITE-001.md)。必须先在formal Kernel/S5 Host范围定义atomic commit、fault witness、room/process isolation与snapshot/recovery，当前不改变任何Client或Server源码。

- **S1 input-payload immutability boundary（2026-08-25，只读、非实现包）：** generic Server 的`ReadOnlyCollection`/record只保证collection structure；`InputSubmission<TInput>`、slot input、pending/locked/journal/ready owners与missing-policy结果都会直接保留opaque `TInput`，源码已明确将value/deep-copy semantics留给future formal input-contract owner。因此不能把“不可变frame”误写为payload deep immutability，也不能以reflection clone/default identity copy/JSON序列化自行解决。详见 Server [`S1-SERVER-INPUT-PAYLOAD-IMMUTABILITY-PREREQUISITE-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S1-SERVER-INPUT-PAYLOAD-IMMUTABILITY-PREREQUISITE-001.md)。必须先在formal Kernel/S1输入范围定义canonical value、capture boundary、equality/hash/serialization、missing-policy关系与mutable-alias regression；本审计不修改任何Client或Server源码。

- **S1 formal FrameInputSet shape boundary（2026-08-25，只读、非实现包）：** `ntsd_new.exe` release `Makefile`纳入`input_handler.cpp`与`game_tick.cpp`；live input basis是right/left/up/down/attack/jump/defend七个logical action，poll会从held state派生prev/rising edge/history/cooldown，AI则由world/input_phase/RNG在kernel内写入同一domain。SDL/Unity key binding、`InputHandler::snapshot()`、prev/history/cooldown/AI与post-`apply_input` state都不是raw Client intent。详见 Server [`S1-FORMAL-FRAME-INPUT-SHAPE-PREREQUISITE-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S1-FORMAL-FRAME-INPUT-SHAPE-PREREQUISITE-001.md)。formal action value、capture/edge derivation、human/AI slot ownership、tick mapping与real-world replay仍待正式scope；本审计不修改任何Client或Server源码。
- **Formal AI/state-hash Client gate（2026-08-30）：** RNG Cut A、FrameInput seam和FrameInput shared-source Cut B均已focused关闭。Cut C～G、formal AI/state-hash与完整BattleKernel仍须后续各自独立Task/Change和必要授权；不能把AI放进Client submission、generic missing policy或复制Server实现。

- **S5 single-writer room actor boundary（2026-08-25，只读、非实现包）：** 当前Server的`SequentialSingleWriter`只是`SequentialRoomExecutionBoundary`/`LocalBootstrapHost`输出的bootstrap metadata；没有运行中的room actor、mailbox、queue、scheduler或并发顺序证明，generic in-memory owners也未声明thread-safe。详见 Server [`S5-SINGLE-WRITER-ROOM-ACTOR-PREREQUISITE-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S5-SINGLE-WRITER-ROOM-ACTOR-PREREQUISITE-001.md)。不能以临时`lock`/queue决定input/deadline/advance/fault顺序；formal S5 Host必须先定义operation order、backpressure、lifecycle、commit与fault isolation。此审计不改Client或Server源码。

- **C1 后的 S3 recovery 门槛（2026-08-25，只读）：** Server [`S3-AUTHORITY-HISTORY-RETENTION-PREREQUISITE-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S3-AUTHORITY-HISTORY-RETENTION-PREREQUISITE-001.md) 已按完成的 C1 更新：activation journal 与每帧 resolved policy witness 现仅为进程内事实，没有 retained-base/epoch、persistence/serializer、snapshot/restore 或 reconnect/recovery disposition。未来恢复合同必须绑定 initial policy、连续 activation prefix、retained envelope range、snapshot tick/checksum 与 target replay tick，并明确 receiver activation-ack 在 restore/reconnect 后是恢复、失效还是重新确认。它不是新源码授权；不得据此修改Client、wire、transport、snapshot/recovery、rebarrier、InputDelay或missing-input。

- **缺失输入产品决策门槛（2026-08-25，只读）：** Server [`PENDING-S1-S2-MISSING-INPUT-PRODUCT-CONTRACT-001.md`](../../../../NTSD_Server/docs/ai/DECISIONS/PENDING-S1-S2-MISSING-INPUT-PRODUCT-CONTRACT-001.md) 已将 S-NET-001/002 收敛为用户必须确认的模式、deadline/grace/max-missing、transient canonical input、persistent neutral/AI/disconnect、policy refusal/fault 与 reconnect/recovery 语义。现有 deadline/provenance 机制不等于选择任何 payload 或产品规则；不得修改Client、wire、transport、snapshot/recovery、rebarrier、InputDelay或missing-input源码。

- **持久执行工作流（2026-08-25）：** 后续 Codex 不得依赖本聊天或旧 session 选择工作；必须先读 Server [`S0-S9-EXECUTION-WORKFLOW.md`](../../../../NTSD_Server/docs/ai/S0-S9-EXECUTION-WORKFLOW.md) 与 [`S0-S9-NEXT-PACKAGE-QUEUE.md`](../../../../NTSD_Server/docs/ai/S0-S9-NEXT-PACKAGE-QUEUE.md)，从最早 READY row 执行。局部 GATED/DEFERRED 不等于总目标暂停；只有 queue 没有 READY/ACTIVE row 时才报告准确的外部 gate，不能反复索要泛化范围确认。

- **持久工作流验证（2026-08-25）：** Server GOVERNANCE-S0-S9-EXECUTION-WORKFLOW-001 已在其治理范围关闭；[`Validate-S0S9ExecutionWorkflow.ps1`](../../../../NTSD_Server/scripts/Validate-S0S9ExecutionWorkflow.ps1) 会校验 workflow/queue 锚点、queue 状态、最多一个 ACTIVE row 和 no-READY 声明。任何后续选包或交接更新后必须运行它，并同时运行 Change Ledger validator；它不验证 battle correctness，也不授权任何 Client 动作。

## 2. 当前主线任务

当前主线是 NTSD 2.8-Logan 与 Unity 的完整战斗重新对齐，按总表的依赖顺序推进：

```text
CURRENT-AUTHORITY + 完整差异总表（已建立）
    ↓
B0：2.8/Unity trace、字段、输入、RNG、slot/lifecycle 基线
    ↓
B1～B8：时间、输入/RNG、pass、frame/physics、hit、held、spawn、battle flow
    ↓
B9～B10：非例外战斗表现与音频
    ↓
B11：用户确认策略后的内容/数值/资源处理
    ↓
B12：同 seed/输入/tick 全场景 parity campaign
```

范围边界：

```text
正式 NTSD 2.8-Logan EXE + playable 源码   # 行为权威，只读
                ↓
Unity battle runtime                     # 实现目标
                ↓
focused / SelfCheck / Play / trace       # 证据，不反向定义规则
```

Server S0～S9 保留为独立并行计划；当前对齐任务不自动修改 Server、transport、协议、数据库、Gateway、Matchmaker 或公网环境。任何脚本实现仍须从总表选择具名差异 ID，并在修改前建立独立 Task/Change。

## 3. 权威文档

后续执行前，按下表顺序读取与当前操作相关的文档。更具体路径的规则优先于泛化说明；任何与根 `AGENTS.md` 冲突的历史文字都以根规则为准。

| 优先级 | 文档 | 用途与阅读规则 |
|---:|---|---|
| 1 | `AGENTS.md` 与 `docs/ai/CURRENT-AUTHORITY.md` | 全项目安全和 NTSD 2.8-Logan 当前 authority；正常 33 ms、F5 3 ms、1000 slot 与双 RNG 只是新权威已观察基线，Unity 仍待重新基线。 |
| 2 | `Assets/NTSD/Docs/ntsd28-logan-vs-unity-battle-alignment.md` | 当前唯一差异总表、用户例外、E/H 专项、实施顺序和完成门槛；新发现差异必须先写入该表。 |
| 3 | `Assets/NTSD/Docs/CODEX-CURRENT-HANDOFF.md`（本文） | 当前主线、近三天用户决定、当前环境复核与可执行续接顺序；不替代行为 authority。 |
| 3 | `Assets/NTSD/Docs/server-lockstep-s0-s9-progress.md` | S0～S9 当前进度、Resume Card、开放决策、问题台账。其“旧任务无法写 sibling root”的环境描述为历史记录；目录/权限以当前任务实测为准。 |
| 4 | `Assets/NTSD/Docs/server-lockstep-s0-s9-design.md` | S0～S9 设计、输入/传输分层、single-slow-client 合同、修复流程与关闭标准。详细设计以它为准。 |
| 5 | `docs/ai/STATE.md` | 全项目长期状态和活跃 Change ID；阅读其日期与覆盖语句。里面旧沙箱路径描述同样不能覆盖当前会话的实际 writable roots。 |
| 6 | `I:\GitHub\Unity_GAS\NTSD_Server\docs\ai\CURRENT-HANDOFF.md`、`STATE.md`、`CHANGE-LEDGER.md` 与最新 `TASKS/CHANGE-RECORDS/S2-SERVER-READY-BUFFER-HORIZON-001.md` | 最近 Server-only 证据、formal Client gate、精确范围、命令与回滚合同。先读它们，不能凭旧 bootstrap 指令重做工程。 |
| 7 | `docs/ai/CHANGE-RECORDS/S0-INPROC-AUTHORITY-001.md` 与 `docs/ai/CHANGE-LEDGER.md` | 冻结的 Unity S0 代码范围和治理总账；只读确认，不得在没有新批准时借此恢复 Unity 验证。 |
| 8 | `Assets/NTSD/Docs/high-frame-rate-presentation-plan.md` | HFR 当前保留方案；恢复前必须先按 NTSD 2.8-Logan render/tick 合同重新盘点。 |

## 4. 最近 3 天有效上下文

### 4.1 读取边界

本次只读取了旧任务的 2026-08-23 至 2026-08-24 记录。其下一页直接回到 2026-08-20，因此未把 8 月 20 日及更早的完整讨论搬入本文件；只在这些近三天记录引用到的权威文档中提取了必要定调。

### 4.2 用户已明确决定

1. **服务端优先、冻结 Client**（2026-08-23/24）

   - 暂不继续修改 Unity Client；用户现已仅批准既有 S0 的读/编译/focused test/`BattleRuntimeSelfCheck`，不批准 Client 源码、Scene、资源或配置改动。
   - 已经写入的 Unity S0 多 world 代码保留，不回滚、不删除；当前状态为 `FOCUSED_TEST_PASS / SELFCHECK_PASS / EXISTING_LOCKSTEP_PASS / WITNESS_IMPLEMENTATION_REQUIRED / RUNTIME_PENDING`。
   - `CLIENT_INTEGRATION_REQUIRED` 已记录并获得 validation-only 批准；若后续需要修改 Client，先建立独立 Change Record，再请求/记录相应实现范围。

2. **独立服务端根目录**

   - 服务端固定为 `I:\GitHub\Unity_GAS\NTSD_Server`，作为 Unity Client 的兄弟目录，而非 `Assets/`、`Tools/` 或 Client 根的临时目录。
   - 服务端应拥有独立 Git、solution、SDK、依赖、配置、测试、部署与自己的治理记录；不得为了绕开目录/权限问题把 server 代码暂写到 Unity/Tools/Temp 再迁移。

3. **技术路线和阶段划分**

   - ServerHost/Gateway 采用 **C# + .NET**；共享的未来 `Protocol` / `Kernel` 边界必须保持 Unity 可消费的 `netstandard2.1` 约束，独立 Server Host 基线为 **.NET 10 LTS**。
   - C++ release live runtime 继续定义 battle rules；不另外复制一套 C++ server battle logic，也不让 .NET ServerHost 重写伤害、技能、碰撞、RNG、对象生命周期或 pass 顺序。
   - S1～S3 先冻结应用层协议语义；S6 才评估 UDP/KCP/ENet/LiteNetLib 等实际 transport。不得提前把某一个 transport 库耦合进 BattleKernel。
   - 控制面将来是 HTTPS/TLS；战斗数据面是低延迟、应用层有 sequence/ACK/redundancy/deadline/jitter 语义的通道。正常客户端只提交离散输入，不上传 Transform、HP、命中、伤害、武器或技能结果作为权威状态。

4. **单慢客户端的硬定调**

   - 不采用“每帧无限等待所有玩家”的 pure wait lockstep。
   - 应采用输入延迟、deadline、不可改写的 authority frame、缺失输入原因、ACK、冗余、Jitter Buffer、恢复与长期缺失状态机。
   - deadline 后的迟到输入不能改历史；短缺包不能伪造 pressed/released/J/K/L/组合边沿；长期缺失的降级只影响该玩家，健康玩家持续跟随服务器。
   - PvP 长期缺失后的 neutral/托管/结局仍是 `S-NET-001 / PENDING_PRODUCT_RULE`，不得凭经验擅自写死。

5. **公网和多地域的边界**

   - 用户确认未来可以使用两个候选公网 IP：`129.204.124.151`、`124.71.139.127`；它们仅是 S6/S7 的获授权测试候选，尚未对其扫描、登录、部署或改安全组。
   - 进入 S6 前仍须由资源所有者确认资源类型、region、OS、CPU/内存/带宽、SSH/RDP/控制台访问、可开的 TCP/UDP 端口、外部测试授权与长期使用条件。
   - 一局 battle 永远只运行在一台权威 Battle Server 上；多地域只能把不同房间分配到不同节点，不能把同一 BattleWorld 拆到两个地区一起推进。

### 4.3 当前代码与验证状态

| 主题 | 已观察事实 / 最新记录 | 不能据此声称 |
|---|---|---|
| Unity S0 多 world / shared RNG | Unity MCP fresh jobs：RNG 1/1、S0 fixture 8/8、existing lockstep 9/9，均0 failed/skipped；self-check PASS、`error CS` 0。真实test-only character三world、十named hashes、RNG/slot-generation首差与Server-owned single RNG source已通过focused范围。 | 完整shared formal Kernel、完整C++ completed-tick/domain/event mapping或S0 `VERIFIED`。跨进程/跨runtime一致性属于S5。 |
| S0 syntax unblock | Record 追加说明：两处 switch 解析括号修正后，force-all 脚本刷新曾得到 Editor DLL 更新与 Console `error=0`。 | S0 自身所有 acceptance 或 runtime 测试已通过。 |
| Server bootstrap | `S0-SERVER-BOOTSTRAP-001` 已在独立 Server Git 仓库达到 `FOCUSED_TEST_PASS / SERVER_CODE_READY / CLIENT_INTEGRATION_PENDING`：bootstrap 两次、Debug/Release build、四项自托管测试、架构边界、Ledger validator 和 no-network local run 均通过。 | formal BattleKernel、authority frames、transport、数据库、Unity 集成、跨端 checksum，或 S0 `VERIFIED`。 |
| Server authority-session | `S0-SERVER-INMEMORY-AUTHORITY-001` 已达到 `FOCUSED_TEST_PASS / SERVER_TESTKERNEL_READY / CLIENT_INTEGRATION_REQUIRED`：generic frame/barrier/session、96 帧 TestKernel journal、Debug/Release build、四项 tests、no-network run、Ledger/static audit 均通过。 | formal NTSD BattleKernel、Unity multi-world、十域 checksum、S0 `VERIFIED` 或 S1。 |
| Server initial tick origin | `S1-SERVER-INITIAL-AUTHORITY-TICK-001` 已达到 `FOCUSED_TEST_PASS / SERVER_INITIAL_AUTHORITY_TICK_READY / CLIENT-PAUSED`：StartBarrier/session/journal 与 protocol contract 可在同一个合法 non-zero authority tick 起步，mismatch 在 kernel step 前 fail closed；Debug/Release 0 error、focused/full self-hosted chain、no-network run、Ledger `14 / 70` 和 scoped audit已通过。 | formal snapshot/recovery、formal Kernel、Client integration、C++ battle alignment、real transport，或 S0/S1 `VERIFIED`。 |
| Server missing-input provenance | `S1-SERVER-MISSING-INPUT-PROVENANCE-001` 已达到 `FOCUSED_TEST_PASS / SERVER_MISSING_INPUT_PROVENANCE_READY / CLIENT-PAUSED`：source/fill-reason 仅允许六种一致、已知 pair；immutable envelope 与 generic policy resolution 均 fail closed。Debug/Release 0 error、focused/full self-hosted chain、no-network run、Ledger `15 / 70` 与 fixed-string audit已通过。 | 任何 missing-input payload、grace、neutral/carry、AI、disconnect/reconnect产品行为、formal Kernel、Client integration，或 S0/S1 `VERIFIED`。 |
| Server redundancy ingress capacity | `S2-SERVER-REDUNDANCY-INGRESS-CAPACITY-001` 已达到 `FOCUSED_TEST_PASS / SERVER_REDUNDANCY_INGRESS_CAPACITY_READY / CLIENT-PAUSED`：ingress-own actual-entry cap拒绝oversize window且zero mutation，at-cap window保留原有顺序/outcomes。Debug/Release 0 error、focused/full self-hosted chain、no-network run、Ledger `16 / 70` 与 fixed-string audit已通过。 | production count、raw packet/MTU/bandwidth cap、Client resend、transport、deadline/missing-input policy、formal Kernel/recovery，或 S2 `VERIFIED`。 |
| Server future-target admission bound | `S1-SERVER-FUTURE-TARGET-BOUND-001` 已达到 `FOCUSED_TEST_PASS / SERVER_FUTURE_TARGET_BOUND_READY / CLIENT-PAUSED`：generic assembler/room adapter要求caller-supplied nonnegative bound，zero合法；exact boundary可接受，over-limit target在managed sequence/pending mutation前以稳定disposition拒绝。Debug/Release 0 error、focused/full self-hosted chain、no-network run、Ledger `17 / 70` 与 fixed-string audit已通过。 | production `InputDelayFrames`/default、deadline、missing-input policy、raw packet/MTU、Client、transport、formal Kernel/recovery，或 S1/S2 `VERIFIED`。 |
| Server authority-tick numeric range | `S1-SERVER-AUTHORITY-TICK-RANGE-001` 已达到 `FOCUSED_TEST_PASS / SERVER_AUTHORITY_TICK_RANGE_READY / CLIENT-PAUSED`：`long.MaxValue`不再是可推进frame tick，只作为final legal tick后的terminal next cursor；terminal fact、assembler lock和direct room call均fail closed。Debug/Release 0 error、focused/full self-hosted chain、no-network run、Ledger `18 / 70` 与 expanded fixed-string audit已通过。 | C++ `world.game_tick` mapping、30 Hz/battle语义、Client、transport、missing-input、formal Kernel/recovery，或 S1/S2 `VERIFIED`。 |
| Server client-known confirmed tick range | `S1-SERVER-CONFIRMED-TICK-RANGE-001` 已达到 `FOCUSED_TEST_PASS / SERVER_CONFIRMED_TICK_RANGE_READY / CLIENT-PAUSED`：`InputSubmission.ClientKnownConfirmedAuthorityTick`只接受既有`-1` sentinel或Protocol addressable tick；terminal `long.MaxValue`在DTO构造前fail closed。`-1`、final addressable与terminal regressions，Debug/Release 0 error、focused/full Server chain、no-network run、declared-source audit与Ledger `20 / 70`均通过。 | reported cursor与target/current tick关系、ACK/retransmit/retention、Client、transport、payload、policy、formal Kernel/recovery，或 S1 `VERIFIED`。 |
| Server ACK / ready / gap tick range | `S2-SERVER-ACK-READY-GAP-TICK-RANGE-001` 已达到 `FOCUSED_TEST_PASS / SERVER_ACK_READY_GAP_TICK_RANGE_READY / CLIENT-PAUSED`：ACK/gap/non-empty ready range只能表示addressable frame fact；progress必须是exact successor；terminal empty ready range继续合法。final-addressable/terminal/empty-range/successor regressions、Debug/Release 0 error、focused/full Server chain、no-network run、declared-source audit与Ledger `21 / 70`均通过。 | real Client ACK/ready/gap flow、retransmit/retention/recovery、transport、payload、policy、formal Kernel/recovery，或 S2 `VERIFIED`。 |
| Server ready-buffer future horizon | `S2-SERVER-READY-BUFFER-HORIZON-001` 已达到 `FOCUSED_TEST_PASS / SERVER_READY_BUFFER_HORIZON_READY / CLIENT-PAUSED`：far envelope不能先占尽buffer count并排挤near contiguous frame；exact horizon保留正常行为。Debug/Release 0 error、focused/full self-hosted chain、no-network run、Ledger `19 / 70` 与 fixed-string audit已通过。 | production jitter/delay、actual Client buffer、transport/ACK/retransmit、weak-network runtime、history/recovery，或 S2 `VERIFIED`。 |
| Server client-sequence retention | `S1-CLIENT-SEQUENCE-RETENTION-PREREQUISITE-001` 只读审计完成：accepted sequence map跨锁帧保留，现有 ACK/冗余/reported cursor不能作为safe eviction floor。 | sequence lifecycle/rollover/reconnect、idempotency horizon、retirement proof、post-expiry disposition/witness、snapshot/replay、capacity/overload的版本化决定；未决定前不得写eviction或count refusal。 |
| Server authority-history retention | `S3-AUTHORITY-HISTORY-RETENTION-PREREQUISITE-001` 只读审计完成：locked envelope/journal list无限增长，当前gap索引需要完整initial-prefix，单次gap response cap不是history cap。 | formal Kernel、retained range/snapshot base、history-expired/recovery disposition、ACK/sequence relationship、bounded capacity与real Server/Client restore-replay evidence；未决定前不得写generic ring/truncation/recovery。 |
| Server protocol-version evolution | `S1-PROTOCOL-VERSION-EVOLUTION-PREREQUISITE-001` 只读审计完成：version `1`是in-memory marker，当前不存在wire codec/ABI、capability negotiation、unknown-disposition或rolling upgrade模型。 | S5/S6版本语义、compatibility/bump规则、session admission、wire header/codec、upgrade/downgrade、replay/schema supersede与real serialized peer fixtures；未决定前不得bump或加compatibility路径。 |
| C++→Unity 对齐 | R1 static source inventory 完成；R2 pass 包仍 `RUNTIME_PENDING`；R07A 的 `D-SCHED-009 + D-RENDER-002` 获 Unity joint S4 Play/automatic evidence，结论是 `UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED`。 | 整个 battle runtime、R7/R8 或 C++ release full trace 已闭环。 |
| R7 performance | `R7-PERF-001` 的 fresh compile、focused `15/15`、warmed `0 B`、full self-check 已有记录，但仍 `RUNTIME_PENDING`，缺真实 battle Play Mode 和 C++ runtime trace。 | 1000 AI 已稳定达到 30 Hz，或性能/对齐已最终认证。 |
| 当前 Unity self-check | 现有 Editor request 已被消费，`Temp/NTSD_BattleRuntimeSelfCheck.result` 于17:07:33 写入 `PASS`；Editor.log 同时记录“自检完成”。 | self-check 覆盖 S0 focused NUnit、formal multi-world 或 C++/Server alignment。 |
| HFR | HFR-00～HFR-09 均 `NOT_STARTED`；`RenderAlpha` 未接入中央 Mesh/Shaders，中央顶点无 previous-position。 | Unity 已有 60/120 Hz presentation support。 |
| Web cadence | `WEB-CADENCE-001`：build、focused `48/48`、Native `open → 16-tick preview → close`、只读 `403` 均有证据；全量 npm 为 `392 passed / 2 existing unrelated failures / 1 skipped`。 | Canvas 三栏人工视觉已验收，或它证明 Unity/C++ release gameplay parity。 |

> **Server S5 异常边界前置结论（2026-08-25，只读）：** 当前 generic kernel/policy 的throw路径不是已实现的 room fault/recovery 行为；journal 与world的提交原子性、fault witness、隔离与恢复均尚未定义。因此任何局部catch/retry/rollback/日志实现都会越过formal Kernel/S5 Host gate，不能被写成已完成的Server实现或S5进展。

> **Server S5 single-writer 前置结论（2026-08-25，只读）：** `SequentialSingleWriter`目前只在bootstrap health metadata中出现，并不证明线程安全、room actor、队列顺序、backpressure或多room isolation。future S5 Host必须把submit/deadline/lock/advance/ACK/ready/gap/fault等操作置于一个明确的deterministic admission order，并用formal Kernel/Client证据验证；在此之前不得以`lock`/background task/queue伪造完成。

> **Server S1 输入 payload 前置结论（2026-08-25，只读）：** 目前已验证的是frame/window的结构不可变，不是任意引用型`TInput`的deep snapshot。正式输入值、capture时点、canonical equality/hash/serialization与missing-policy payload必须与formal Kernel/Client/C++ release-live input evidence共同定义；在此之前不能增设通用clone或把当前Server-only测试称为不可变payload证据。

> **Server S1 formal input shape 前置结论（2026-08-25，只读）：** C++ release live path已确认七个logical held action及由runtime派生的prev/edge/history/cooldown；AI由world/input_phase/RNG在kernel内生成。未来`FrameInputSet`必须定义player input capture/edge contract与human/AI ownership，不能上传SDL/Unity binding、input history、cooldown、AI或post-input state，也不能将`InputHandler::snapshot()`当作battle snapshot。

### 4.4 性能、HFR 与 battle alignment 的固定结论

- 新权威正常逻辑间隔为 **33 ms**，F5 快速模式为 **3 ms**；Unity 当前 `1f/30f` 是待修差异，旧“恒为 30 Hz”结论已废止。`SimulationTickDriver -> NTSDBattleTickSystem -> SimulationWorld`、`FrameInputSet`、slot/generation、SoA/ECS、pool、CentralOnly、Texture2DArray、动态 Mesh/URP 与 battle-time 0-GC 目标仍是可保留的 Unity 实现边界，但必须证明不改变新权威结果。
- 既有性能文档的最近可用结论是：1000 AI 的稳定 30 Hz gate 仍没有被证明关闭；不要把 catch-up 限制、单个 focused `0 B` 或平均 FPS 当作容量验收。后续性能报告必须报告 tick P50/P95/P99、GC、backlog/dropped tick、实体容量和真实 profile。
- HFR 的 v1 只能做“一个逻辑 tick 延迟的 previous/current presentation interpolation”；出生/销毁、slot/generation/lineage 变化、frame/pic/facing、hit/opoint/overlay 等结构性事件应保持离散，异常时 fail closed 回到 current-only。
- `R1-WP02` 自动化 C++ full trace 是额外的定位/比较 blocker，不阻断已经有 C++ source contract 的最小 Unity 工作包；但没有它或同等级的 C++ runtime evidence 时，所有相关结果都必须保留在 `RUNTIME_PENDING`/相应层级。

## 5. 已完成事项

### 5.1 已实现且已有相应验证

- `R8-WP01G-R07A`：`D-SCHED-009` 与 `D-RENDER-002` 的 Unity joint S4 证据已完成到可用证据上限。Record/Task 记载 actual collision/hit → frozen publication → same-tick writeback → central materialization → Late idempotence 与 next-tick RNG/lifecycle 的 joint 证据，且有 fresh compile、focused suites、full self-check、Play probe、Console0、ledger 的记录。**仍缺 C++ full trace；不可扩大成完整 battle/C++ verified。**
- `R7-PERF-001`：已移除 stale PreInteraction cross-pass proof；既有记录为 compile0、focused `15/15`、warmed `0 B` 和 full self-check PASS。当前状态仍为 `RUNTIME_PENDING`，不是完整 runtime 对齐。
- `WEB-CADENCE-001`：独立只读 render-cadence 入口、纯 presentation sampler、只读 server flag、专用 launcher 与 focused/HTTP 生命周期验证已完成；默认 DAT 编辑器、Unity、C++、DAT 和资源未改。
- 服务器设计/治理层：`server-lockstep-s0-s9-design.md`、`server-lockstep-s0-s9-progress.md`、`S0-SERVER-BOOTSTRAP-001` Task Contract 和该冻结 Unity S0 Change Record 已建立。
- `S0-SERVER-BOOTSTRAP-001`：独立 Server Git/.NET 10 solution、模块边界、Server Ledger/State/Handoff/Record、bootstrap/build/test/run-local、架构检查和 no-network local health skeleton 已实际完成并验证；状态为 `FOCUSED_TEST_PASS / SERVER_CODE_READY / CLIENT_INTEGRATION_PENDING`。
- `S0-SERVER-INMEMORY-AUTHORITY-001`：generic immutable frame、StartBarrier、authority-first session、replica checksum witness 和 tests 内 TestKernel 已实际完成；Debug/Release 0 error、四项 tests、no-network run、Ledger/static audit 已验证；状态为 `FOCUSED_TEST_PASS / SERVER_TESTKERNEL_READY / CLIENT_INTEGRATION_REQUIRED`。
- `S1-SERVER-INITIAL-AUTHORITY-TICK-001`：Server generic StartBarrier/session/journal/protocol initial-tick alignment与non-zero/mismatch regressions已实际完成；Debug/Release 0 error、focused/full self-hosted Server tests、no-network run、Ledger`14 / 70`和scoped audit已验证；状态为 `FOCUSED_TEST_PASS / SERVER_INITIAL_AUTHORITY_TICK_READY / CLIENT-PAUSED`。
- `S1-SERVER-MISSING-INPUT-PROVENANCE-001`：Server protocol provenance pair validator、immutable envelope/resolution guard与legal/mismatched/unknown regressions已实际完成；Debug/Release 0 error、focused/full self-hosted Server tests、no-network run、Ledger`15 / 70`和fixed-string audit已验证；状态为 `FOCUSED_TEST_PASS / SERVER_MISSING_INPUT_PROVENANCE_READY / CLIENT-PAUSED`。它不选择任何 missing-input 产品行为。
- `S2-SERVER-REDUNDANCY-INGRESS-CAPACITY-001`：Server-owned redundancy actual-entry cap、oversized-window no-mutation rejection与at-cap/disorder regressions已实际完成；Debug/Release 0 error、focused/full self-hosted Server tests、no-network run、Ledger`16 / 70`和fixed-string audit已验证；状态为 `FOCUSED_TEST_PASS / SERVER_REDUNDANCY_INGRESS_CAPACITY_READY / CLIENT-PAUSED`。它不选择production count、wire/MTU或任何missing-input产品行为。
- `S1-SERVER-FUTURE-TARGET-BOUND-001`：Server generic future-target admission bound、adapter propagation、stable over-limit disposition与negative/zero/exact/no-mutation/near-terminal regressions已实际完成；Debug/Release 0 error、focused/full self-hosted Server tests、no-network run、Ledger`17 / 70`和fixed-string audit已验证；状态为 `FOCUSED_TEST_PASS / SERVER_FUTURE_TARGET_BOUND_READY / CLIENT-PAUSED`。它不选择production delay/default、deadline/missing-input产品行为、Client、transport或任何S1/S2验证状态。
- `S1-SERVER-AUTHORITY-TICK-RANGE-001`：Server generic authority tick range、terminal fact rejection、terminal lock rejection与final session/room/journal/ready-buffer/progress/ACK regressions已实际完成；Debug/Release 0 error、focused/full self-hosted Server tests、no-network run、Ledger`18 / 70`和fixed-string audit已验证；状态为 `FOCUSED_TEST_PASS / SERVER_AUTHORITY_TICK_RANGE_READY / CLIENT-PAUSED`。它不定义C++ tick mapping、battle/30 Hz、Client、transport或任何S1/S2验证状态。
- `S2-SERVER-READY-BUFFER-HORIZON-001`：Server generic ready-buffer future horizon、stable far-envelope rejection与invalid/zero/exact/no-mutation/near-capacity/moving-window/disorder regressions已实际完成；Debug/Release 0 error、focused/full self-hosted Server tests、no-network run、Ledger`19 / 70`和fixed-string audit已验证；状态为 `FOCUSED_TEST_PASS / SERVER_READY_BUFFER_HORIZON_READY / CLIENT-PAUSED`。它不选择production jitter/delay、不实现Client、transport、weak-network或任何S2验证状态。
- `S0-INPROC-AUTHORITY-001` validation-only：fresh Unity assembly/Editor.log compile evidence 与 `BattleRuntimeSelfCheck=PASS` 已实际获得；没有修改 Client 源码、场景、资源或配置。

### 5.2 已实现但未运行时验收 / 明确冻结

- `S0-INPROC-AUTHORITY-001`：Unity 同进程 server + 两个 client world 的骨架及五项 Editor tests 已写；self-check 与 S0-focused NUnit 5/5 已通过，但既有 lockstep tests、真实多 world journal 和 C++/runtime 证据仍缺。当前只允许验证，禁止 Client 代码修改。
- 所有要从当前 Unity battle alignment 延伸到真实 C++ release trace、真实 Play Mode、真实 DAT/scene 的包，除有明确 task evidence 外，仍应按各自 Change Record 的 `RUNTIME_PENDING` 处理。
- `WEB-CADENCE-001` 的 Canvas 人工视觉三栏验收仍待；全量 npm 的两项历史 `main.ts` 静态正则失败没有被本包修复。

### 5.3 仅分析/设计完成

- S0～S9 的分阶段设计、协议职责、slow-client 降级原则、恢复/快照/ACK/Jitter 方向、服务端模块边界与 S5/S8/S9 的责任划分。
- HFR 的 HFR-00～HFR-09 计划和 HFR Off/On 不改逻辑的验收矩阵。
- 多地域、Gateway、Matchmaker、Room Allocator、容量调度的后期架构说明；未写对应生产代码。

### 5.4 已废弃、不得继续或不得误用

- 不再使用“C# release / Unity self-check 是最终 battle authority”的旧口径；唯一裁决是 C++ release live path。
- 不采用“一个慢客户端无限拖住整局”的网络模型。
- 不在 S0～S5 之前绑定真实 transport、写 Socket/数据库/公网 listener，或把 TestKernel 称为正式 NTSD BattleKernel。
- 不把 Web cadence 诊断、HFR 计划、历史 self-check、性能 0-B 结论当成 C++ full-trace/真实 Play Mode 战斗认证。
- T8 默认 `stage.dat` 资产部署继续按用户决定暂缓；不要为测试变绿私自生成或加入默认资产。

## 6. 当前阻塞

> **2026-08-29 优先更新：** held-only 与 missing-input carry 已解除，不再是阻塞。当前 Server 硬 gate 是 formal Kernel AI ownership/state hash、实测 numeric short-grace/deadline/delay，以及后续 S3 snapshot/history recovery；下表中更早的 Client/trace/Web 项仅保留其各自历史范围。

| 优先级 | 阻塞 | 已观察原因 | 影响范围 | 清除后的下一步 |
|---:|---|---|---|---|
| 已解除 | Server bootstrap 环境前置 | 当前 Server 根的 `global.json` 解析 `.NET SDK 10.0.400`；独立 Git/workspace 已存在。 | `S0-SERVER-BOOTSTRAP-001` 已不再被 SDK/目录/sandbox 阻塞。 | 后续 Server 扩展另建 Change Record；不自动扩大为 S0 battle verification。 |
| 已解除 | Existing lockstep regression runner | Unity MCP final job `714ba0d70461400587887ea234ceb440`为9/9。 | RNG 1/1、S0 8/8与existing lockstep 9/9均具fresh MCP job证据。 | 不重复这些fixtures；按Queue进入下一具名formal shared-owner/C++ mapping包。 |
| P1 | C++ release 自动 full trace 观察通道未解决 | `R1-WP02` 保持 `BLOCKED`；没有已确认的只读、可重复、覆盖 full schema 的观察方式。 | 不能取得 C++ full-trace/comparator 证书；不阻断已闭合的 C++ source contract 与最小 Unity work package。 | 仅在获得已有的无 authority 写入观察方式后再继续；严禁 instrumentation、hook、patch、注入、重建或新增 trace sink。 |
| 已部分解除 | 当前 Unity fresh verification | fresh assemblies、`BattleRuntimeSelfCheck=PASS`、MCP RNG 1/1、S0 8/8、existing 9/9、Console `error CS` 0已取得。 | 仍不能证明完整shared formal Kernel、完整C++ mapping或S0阶段VERIFIED。 | RNG Cut A已关闭；下一个Client Cut必须另建Task/Change并重新授权。 |
| P2 | 工作树高度脏且治理文档本身未提交 | `git status` 显示大量脚本、资源、场景、工具和 Docs 修改/未跟踪项；`docs/ai/` 也处于未跟踪状态。 | broad build、提交、回滚和大范围 diff 很容易误触用户工作。 | 每次只按 Task/Record scoped diff；不 `reset`/`clean`/`restore`，不提交未审查文件。 |
| P3 | Web cadence 最后视觉验收 | 自动/HTTP 证据齐全，但当前没有浏览器 Canvas 人工观察证据。 | 仅影响 `WEB-CADENCE-001` 的最终 runtime 级别，不影响 Server bootstrap。 | 用户或后续任务在实际浏览器选择有位移的技能，观察 30/60/120 三栏并记录结果。 |

## 7. 下一步执行顺序

> **2026-08-30 优先更新：** FrameInput Cut B、Cut C identity/vectors、StageSpawn correction与`CLIENT-FORMAL-KERNEL-SLOT-LIFECYCLE-SEAM-001`均已focused关闭。当前无READY/ACTIVE源码包；Queue `0bc / CLIENT-FORMAL-KERNEL-SLOT-LIFECYCLE-SHARED-OWNER-001`须新的独立授权，仍不得扩张到formal AI、marker、Scene/资源/Input Actions/transport/recovery。

> **2026-08-25 执行更新：** 下方 bootstrap 步骤是历史完成记录，**不得重做**。Client 冻结仍然有效，但不会暂停 Server-first 总目标；S1/S2 的 Server-only packages以及最近`S2-SERVER-ACK-READY-GAP-TICK-RANGE-001`已各自在独立 .NET 范围通过。当前没有活跃 Server source package：先阅读 `NTSD_Server/docs/ai/CURRENT-HANDOFF.md`、`STATE.md`、Ledger、最新`TASKS/CHANGE-RECORDS/S2-SERVER-ACK-READY-GAP-TICK-RANGE-001.md`以及`AUDITS/S1-SERVER-INPUT-PAYLOAD-IMMUTABILITY-PREREQUISITE-001.md`、`AUDITS/S1-CLIENT-SEQUENCE-RETENTION-PREREQUISITE-001.md`、`AUDITS/S3-AUTHORITY-HISTORY-RETENTION-PREREQUISITE-001.md`、`AUDITS/S5-KERNEL-ROOM-FAULT-BOUNDARY-PREREQUISITE-001.md`，随后只做前置审计或创建有明确缺口的新 Server-only Change Record。不得修改任何 Client 源码，也不得用 generic/TestKernel绕过formal Kernel、snapshot/recovery、transport或产品规则门槛。

> **补充读取门槛：** 在下一项Server源码包前，还必须阅读 `NTSD_Server/docs/ai/AUDITS/S5-KERNEL-ROOM-FAULT-BOUNDARY-PREREQUISITE-001.md`；除非已获得formal Kernel/S5 Host的atomic commit、fault witness、isolation与recovery合同，不得以局部异常处理绕过它。

> **补充读取门槛：** 在下一项触及`TInput`、submission、locked envelope、missing-policy payload、journal/replay或ready-buffer value semantics的Server源码包前，必须先阅读 `NTSD_Server/docs/ai/AUDITS/S1-SERVER-INPUT-PAYLOAD-IMMUTABILITY-PREREQUISITE-001.md`；未获得formal input-contract范围时不得添加通用deep clone、reflection copy或ad-hoc serializer。

每次新包仍须更新对应 Task Contract、Change Record、Ledger、`docs/ai/STATE.md` 和 server progress；不要只在聊天中宣布状态。

1. **补齐 .NET 10 SDK 前置条件（用户动作）。**

   - 不自动安装 SDK、不改 PATH 或 profile。
   - 验收：在任意 shell 中 `dotnet --list-sdks` 出现 `10.0.*`；随后在 Server 根的 future `global.json` 约束下 `dotnet --version` 解析为受支持的 10.0 SDK。

2. **恢复 `S0-SERVER-BOOTSTRAP-001`，但先做只读/治理 preflight。**

   - 重新阅读本文、第 3 节文档、Task Contract、Server 根目录及 `git status`；确认 `NTSD_Server` 不含用户文件或先吸收其已有内容。
   - 在写入第一个服务器脚本之前，**在 `NTSD_Server/docs/ai/CHANGE-RECORDS/` 创建真正的 `S0-SERVER-BOOTSTRAP-001` Change Record**，并建立该 Server 仓库自己的 Ledger/State；不要在 Unity Ledger 伪造一个外部路径 Record。
   - 验收：Change Record 明确覆盖首包文件、authority/需求、模块边界、验证与回滚；Unity Client scoped diff 没有被此包新增修改。

3. **仅实现 Server 工程骨架。**

   - 创建独立 `.git`、`global.json`、`Directory.Build.props`、`Directory.Packages.props`、`NTSD.Server.sln`、`README.md`、`AGENTS.md`、`src/`、`tests/`、`scripts/`、`config/`、`deploy/` 与 `docs/ai/`，目录严格遵守 S0 Task Contract。
   - `Protocol` / `Kernel.Abstractions` 无 Unity、Host、DB、transport 依赖；`BattleHost` 只拥有 room 的顺序执行边界；不创建无 owner 的 `Common` 项目；TestKernel 必须显式测试专用。
   - 验收：`scripts/bootstrap.ps1` 可重复运行且 fail-fast；architecture tests 能拒绝禁止的项目引用；没有 Socket、DB、真实 battle rule 或 Unity Client diff。

4. **完成纯 .NET build/test/run 链。**

   - 运行 Task Contract 指定的 `scripts/build.ps1 -Configuration Release`、`scripts/test.ps1 -Configuration Release` 和最小 `run-local`/health 验证；执行 Server 侧 Ledger validator 或等价检查。
   - 验收：Release build/test 为成功退出；无 `bin/obj/TestResults/logs/secrets` 被纳入 Git；状态最多可到 `SERVER_CODE_READY / CLIENT_INTEGRATION_PENDING`，不是 S0 `VERIFIED`。

5. **CLIENT_INTEGRATION_REQUIRED 已建立，并由持续授权按 Queue 顺序执行。**

   - Server progress 与 Server Handoff 已列出 Client files、formal Kernel 共享边界、纯 Server TestKernel 的不足、预期 checksum/fixture、风险与回滚。
   - 验收：具名 Queue 包在独立事前 Task/Change 闭合后，可直接继续对应的 `S0-INPROC-AUTHORITY-001` focused test、`BattleRuntimeSelfCheck`、同 journal 的 server + two-client world、十域 witness 和真实运行时验证；跨进程/跨 runtime checksum 仍另按 S5 处理，且不得借此把 S0 标成 `VERIFIED`。

6. **不要并行自动恢复非服务器主线。**

   - battle alignment：当前仅保留各 Change Record 的 `RUNTIME_PENDING` 事实；在用户恢复该主线后，从指定的 R8/repair Task、C++ source contract 与最小 scoped validation 继续。
   - HFR：用户明确批准后才新建 HFR-00 Change Record，从 baseline/feature gate 开始，不能直接改 shader/mesh。
   - Web cadence：只有要关闭其 `RUNTIME_PENDING` 时再做浏览器 Canvas 人工视觉验收；不能替代 Unity HFR。

## 8. 技术定调与禁止事项

### 8.1 Battle authority、tick 与 Unity 实现边界

- 规则只由当前固定 SHA 的 NTSD 2.8-Logan 正式 EXE，以及对应 playable build closure 定义；从 `source\ntsd28_playable\src\game_session.cpp::GameSession28::step()`、`source\ntsd28_core\src\simulation\simulation_tick_driver.cpp::SimulationTickDriver28::step(...)` 和 `BattleWorld28` 继续追踪。NTSD 2.4/C# 只作历史线索。
- 正常逻辑间隔对齐 **33 ms**，F5 对齐 **3 ms**。Unity `Update`/`FixedUpdate`/`LateUpdate` 不定义 battle rule；tick 内不能用 `Time.deltaTime`/`Time.fixedDeltaTime` 决定战斗结果。
- Transform、Animator、Camera、SpriteRenderer、Mesh、URP 只读逻辑/表现快照，绝不反写 position、velocity、frame、HP/PP、link/holder/target、input、RNG 或碰撞真值。
- 每个 gameplay/Server adapter 改动必须拥有闭合 C++ authority、Unity mapping、focused check、必要 Play Mode 和与风险相称的 C++/server checksum evidence；不能把静态阅读、编译0、self-check、单一测试或历史 Pass 外推为“已对齐”。

### 8.2 Server/lockstep 定调

- 同一 `FrameInputSet` 应是单机、回放、Client 和 Server 的共同逻辑入口；Server 只组装/锁定 authority frames，不能另写 gameplay。
- 正常网络只同步 input/ack/checksum/recovery data；不以 Client Transform 或状态包为真相，也不以客户端本地 snapshot 作为 authority restore。
- 同一 BattleWorld 一个顺序、单写者 tick owner；多个房间才可按房间并行。不得为了性能把同一局 battle passes 随意并发。
- 不让慢客户端无限阻塞健康客户端；但具体 input delay、deadline、grace 和 PvP 长缺失产品规则仍待 `S-NET-*` 证据/用户决定。
- 在 S6 前不得选择/耦合实际 transport；在 S8 前不得实现 Gateway、Auth、Matchmaker、Room Allocator、多地域调度、数据库或消息队列；无授权不得探测或操作公网 IP。

### 8.3 HFR、表现与性能定调

- HFR 只影响 presentation sampling；HFR Off/On 的 logic checksum、RNG、slot/generation、frame、HP/PP、事件、command identity/order 必须一致。
- 不能以提高显示帧率来改 33 ms 正常逻辑 cadence 下的 DAT wait、碰撞、输入窗口、AI、hit、opoint、随机数或对象生灭时序；F5 3 ms 是独立权威 host 模式。
- CentralOnly、Texture2DArray、中央 Mesh、动态 quad、URP、slot/generation/pool、MobileExtended 1000 active 与 DesktopExtended 无固定产品 active cap 不能被对齐/性能修复回退。
- 性能验收必须看预热后的 0 B hot path、P50/P95/P99、GC、backlog、entities、draw、Mesh build，而不是仅平均 FPS 或一次 catch-up 行为。

### 8.4 资源、验证、Git 与安全

- `T8` 默认 `stage.dat` 资产部署继续暂缓。不要为测试加入/生成默认资产；需要 stage 的测试显式使用 fixture 或报告前置条件。
- 未经用户批准不修改 `Assets/NTSD/Scripts/Gen/`、`Assets/Plugins/`、C++ authority、Git hooks/config 或公网环境；不 push。
- 不 `git reset --hard`、`git restore`、`git clean`、删除/覆盖用户文件，也不通过换目录/Temp/Tools 绕开 Server 根和 Client 冻结边界。
- 本次当前线程先完成交接迁移，随后完成独立 Server bootstrap；实际运行了 Server bootstrap/build/test/run-local 与 Server Ledger validation。全程没有运行 Unity、EditMode、Play Mode、SelfCheck、C++ trace、浏览器视觉验收或公网操作，也没有修改 Unity Client 代码。

## 9. 关键文件与入口

| 路径 / 命令 | 作用 | 当前使用注意事项 |
|---|---|---|
| `J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan\source\ntsd28_core\src\simulation\simulation_tick_driver.cpp` → `SimulationTickDriver28::step(...)` | 当前 Battle C++ playable live authority 入口。 | 继续追 `GameSession28`、`BattleWorld28`、frame/physics/input/hit/spawn/render，并确认 playable build participation。 |
| `Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs` | Unity 当前逻辑帧外层入口。 | 当前 `1f/30f` 待重新基线；不得让渲染帧反写 tick。 |
| `Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs` 与 `SimulationWorld.cs`/各模块 | Unity battle pass、world state 和逻辑真值。 | 按 NTSD 2.8 总差异表与当前 C++ source contract，不按旧 C#/2.4 语义猜测。 |
| `Assets/NTSD/Docs/ntsd28-logan-vs-unity-battle-alignment.md` | 当前唯一完整差异总表。 | 先选择具名差异 ID，再建立 Task/Change；用户例外不得丢失。 |
| `Assets/NTSD/Scripts/Simulation/Lockstep/InProcessBattleKernelHost.cs` | 每个 S0 in-process replica 的 `SimulationWorld + NTSDBattleTickSystem` owner。 | 已读/编译证据已取；没有修改，focused/runtime仍待。 |
| `Assets/NTSD/Scripts/Simulation/Lockstep/InProcessLockstepAuthoritySession.cs` | server → clients 的 authority journal、推进和 first-difference 捕获。 | 已读/编译证据已取；没有修改，focused/runtime仍待。 |
| `Assets/NTSD/Scripts/Simulation/Lockstep/LockstepStartBarrier.cs` / `LockstepSessionIdentity.cs` | S0 session identity、barrier fingerprint、canonical slots。 | identity exposure 安全调整已随 S0 写入；不要对外暴露可变数组。 |
| `Assets/NTSD/Scripts/Test/Editor/InProcessLockstepAuthoritySessionEditorTests.cs` | S0 focused Editor tests。 | 五项 NUnit 独立 fixture；当前未运行，因为项目被现有 Editor 锁定且无远程 runner。 |
| `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` 与 `.../Editor/BattleRuntimeSelfCheckEditor.cs` | Unity battle runtime 自检入口。 | 仅在解除 Client 冻结后使用；当前 result 文件不存在，历史 PASS 必须按日期引用。 |
| `Tools/Validate-ChangeLedger.ps1` | 检查工作树的脚本 diff 是否被 Change Record 覆盖。 | 任何含脚本改动的交付/提交前必须跑；文档迁移本身未触发。 |
| `I:\GitHub\Unity_GAS\NTSD_Server` | 独立 Server 根。 | `main` Git 仓库与 `.NET 10` solution 已建立；实际证据见其 `docs/ai/` 下的 Server Record/Ledger/State/Handoff。 |
| `I:\GitHub\Unity_GAS\NTSD_Server\src\NTSD.Server.BattleHost\InMemory\InMemoryAuthoritySession.cs` | Server-only generic authority-first/fail-closed 调度容器。 | 不是 formal BattleKernel，也不定义 S1 protocol 或战斗规则。 |
| `I:\GitHub\Unity_GAS\NTSD_Server\tests\NTSD.Server.BattleHost.Tests\InMemoryAuthoritySessionTests.cs` | 96 帧 TestKernel journal 与 reject/mismatch matrix。 | 已通过；只能证明容器行为。 |
| `I:\GitHub\Unity_GAS\NTSD_Server\docs\ai\CURRENT-HANDOFF.md` | Server 当前 resume card 与 `CLIENT_INTEGRATION_REQUIRED` 范围。 | 在任何 Server 或 Client 下一步前优先阅读。 |
| `docs/ai/CHANGE-RECORDS/S0-INPROC-AUTHORITY-001.md` | 冻结 Client S0 的真实改动和未验证项。 | 是事实来源，不是继续 Client 工作的授权。 |
| `docs/ai/CHANGE-RECORDS/WEB-CADENCE-001.md` | 独立 Web presentation diagnostic 的范围与证明。 | 不是 battle authority / HFR runtime certificate。 |
| `dotnet --list-sdks` / `dotnet --version` | Server SDK preflight。 | 当前 `global.json` 下已解析 `.NET SDK 10.0.400`；仍不得用 8/9 临时顶替。 |
| `& $env:UNITY_EXE -batchmode ... -runTests -testPlatform EditMode ...` | Unity EditMode 测试命令模板。 | 只在用户解除 Client 冻结且确认不与现有 Editor 争用 Library 后执行；实际 Editor 路径以 `ProjectSettings/ProjectVersion.txt` 和本机安装为准。 |

## 10. 给下一个 Codex 的启动指令

```text
请先阅读 docs/ai/CURRENT-AUTHORITY.md、Assets/NTSD/Docs/ntsd28-logan-vs-unity-battle-alignment.md、根 AGENTS.md、docs/ai/STATE.md 和 Assets/NTSD/Docs/CODEX-CURRENT-HANDOFF.md。不要依赖旧会话上下文，也不要从 Git 历史恢复已删除的 NTSD 2.4/C# 对齐计划。

当前战斗主线是 NTSD28-UNITY-BATTLE-REALIGNMENT-001。只从总表选择具名差异 ID，先闭合 NTSD 2.8 playable authority 调用链、Unity 生产调用链、前置条件和验收，再建立独立 Task Contract 与 Change Record。不得用旧 2.4/C#、历史 self-check 或旧 VERIFIED 状态补写当前行为。

用户批准的 Unity 例外必须保留并披露：Slot 容量、头顶血条、FootSelf、移动端取景、多边形边界、当前随机掉武器、固定世界相机。用户排除完整 HUD、结果表现、背景多层/cycle 和完整选择流程。旧 NTSDSpec 与内容/数值/资源差异必须处理；内容策略未决定前只允许只读 inventory，不得覆盖 DAT/资源。

Server/lockstep 是并行历史主线；只有任务触及它时才继续读取 I:\GitHub\Unity_GAS\NTSD_Server 的 workflow/queue。不得让 generic TestKernel、网络协议或旧 30 Hz 假设覆盖当前 NTSD 2.8 战斗规则。

没有同 seed、同输入、同 tick 的 2.8/Unity trace、focused test、编译、SelfCheck 和必要 Play/表现证据时，不得把任何差异写成 ALIGNED 或宣称整个战斗无差异。
```

## 11. 本次迁移自检

- 已读取旧任务的近三天页面；未复制 2026-08-20 及更早聊天全文。
- 已保留近三天用户明确的 server-first、Client freeze、独立目录、.NET 10、网络/公网边界和持续留痕决定。
- 已用当前工作区重新核验 Server 目录、Git 状态、SDK、Unity 版本、SelfCheck result 是否存在及 HFR/Server/Alignment 文档状态。
- 已将旧任务的“sandbox 未热重载”写为历史事实，而不是当前未验证的 blocker；当前仍以 .NET 10 缺失为明确 blocker。
- 未把 `CODE_WRITTEN`、历史 compile/self-check、focused test、Web 自动验证或设计文档写成完整 runtime/C++/Play Mode 完成。
- 交接迁移子任务仅新增 Client handoff 及其 `.meta`；同一线程后续在独立 `NTSD_Server` 仓库创建了 Server bootstrap 文件和验证链。没有修改 Unity battle/runtime、scene、asset、C++ authority、Git 配置或公网资源。

## Goal18F 收尾追加（2026-09-12，VERIFIED，限定 impact 三包）
本追加更正此前 Goal18 的 PLANNED/IN_PROGRESS/CODE_WRITTEN、SHARED_PENDING 等恢复状态；历史段落、乱码、失败和证据均保留。用户本轮授权为断点续传，没有重做或回退 I1/I2/I3。
- I1 `NTSD28-B6-NATIVE-IMPACT-HITPLAN-CARRIERS-PRODUCTION-001`：VERIFIED。Authority 为 battle_world.cpp:5405-5531 的 environment/+0x90/+0x164 写入；瞬态 HitPlan 捕获/投影/mask，复用既有 catch-source。历史 RED 11 FAIL/2 PASS；本次共享 focused 13/13 PASS。
- I2 `NTSD28-B6-NATIVE-IMPACT-PURE-CORE-PRODUCTION-001`：VERIFIED。Authority 同段的 owner/type/immunity/respond/motion 有序事务；纯计划 resolver，拒绝零写、对象 Y 步长 2.3、字符 3.0 与除法位模式。历史 RED 执行147项，报告至少25失败且 capped，精确失败总数未知，禁止写成147 FAIL；本次147/147 PASS，warmed0B。
- I3 `NTSD28-B6-NATIVE-IMPACT-ATOMIC-PRODUCTION-INTEGRATION-PRODUCTION-001`：由 CODE_WRITTEN 经本次 FOCUSED_TEST_PASS 推进为 VERIFIED。Authority 为 hit_candidates.cpp:134-256、battle_world.cpp:4540-4580/5270-5309/5405-5531、game_session.cpp:4171-4182；concrete/generic/legacy/HitPlan 共用既有 shared writer。历史 RED137 FAIL/57 PASS，本次194/194 PASS，Shadow valid/0 mismatch。当前 Record C++ 原文与现场源码逐字匹配，正式 EXE SHA B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033；见 Temp/Goal18F_AuthorityIdentity.json。

F1：原四项旧 focused 新鲜4/4 PASS，原失败源码守卫在续传前已经修好；第二份旧 XML 实为3/3，不能误报它覆盖四项。证据 Temp/Goal18_F1_OldFocused_Result.json 与 Goal18_F1_OldFocused.xml。
F2：Temp/Goal18_PlayF_attempt1.json 与 attempt2.json 均 PASS/cleanup=true；第二次补全 PP/rest 观测。current Tayuya OID36/frame243，经真实 SimulationTickDriver，seed424242、empty input、采样 tick6/7，kind11 只有 index3 候选。environment=-20、catch_source8242、impact_source50、action182；kind10 Y=-2/Vy=-6，kind11 Y=-3.75、Vy=-8.9，Vx/Vz=/1.07 位模式匹配。HP/PP/rest/delay/WeaponCount 与 impact 边界 RNG 保持。98个 C++/Unity 比较字段 firstDifference=null，详见 Temp/Goal18_PlayF_Comparison2.json、Goal18_AuthorityTrace.json 与 Goal18_AuthorityProvenance.json。
见证边界：C++ harness 调用当前 playable core 的几何候选与 impact 阶段，恢复 Unity 采样边界 CRT 状态、使用冻结 Direction-B 帧夹具并平移 Z 原点，排除无关实体；不是完整 C++ GameSession host replay。PP/sourceArest/targetArest/targetVrest 由 Unity 新鲜采样证明不变，未宣称这些字段由 C++ 输出。RNG 不抽仅指 impact 阶段，整个 driver tick 仍有其他既有 RNG 消费。object2.3、自定义 respond 与 kind17/18 由 focused 夹具覆盖；kind17/18 为 PLAY_NOT_PERFORMED_NO_PRODUCER。

F3：唯一共享 B6 job032efcd73b5949f1bada58270878a741 执行964=610前置+354新增，原始963 PASS/1 FAIL；失败为旧 impact 名称守卫，已按原批类内期望授权只修改四个字符串。定向完整守卫4/4 + refill9/9，于 jobb3527a23f1324eb7872d68f1e64eb9dc 合计13/13 PASS；首次定向启动0tests超时单独保留。合并最新定向结果后964个用例均有PASS证据，未进行第二次整批B6，也未篡改原963/964 XML。全部前置92/80/17/72/24/23/32/140与Goal17三包13/4/10覆盖数量核对一致，见 Temp/Goal18F_B6_Coverage.json。full SelfCheck于2026-09-12 08:37:27Z新鲜PASS；汇总 Temp/Goal18F_Regression.json，原始 Temp/Goal18F_B6.xml、Goal18F_GuardAndRefill.xml、Goal18F_SelfCheck.result。
双构建实际命令：dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly；Editor同命令。均exit0/0error，warnings47/104；最终Editor增量后再次0error，见 Temp/Goal18F_RuntimeBuild.txt、Goal18F_EditorBuild_Final.txt。Unity重载后无编译错误，SelfCheck Console7条均预期registration/rest负向夹具，不声称Console0。
Tools/Validate-ChangeLedger.ps1通过452 records/2 governed code files；最终文件 Temp/Goal18F_Validator.txt。PowerShell默认把Git全局ignore不可读与CRLF warning当作终止错误，因此仅对子进程追加 core.excludesFile=NUL、core.safecrlf=false，保留既有safe.directory设置；不写.git/config，不绕读受限文件。历史Record非当前diff警告保留。
SelfCheck曾切换为空场景，已通过既有编辑器重新打开 NTSD_Battle，isDirty=false/root13；Scene SHA仍为D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11，见Temp/Goal18F_FinalSceneRestored.json。

F4：本次只改两个授权测试文件（probe增加8行、旧impact守卫4个字符串）与治理文档；无生产脚本变更，无schema/RNG写入/HP/PP/rest/delay主体修改，无FluteForce/+11复活。三个既有UI图片修改与.claude用户目录保留。无git add/commit/push。全部历史中文乱码只保留，本次追加为UTF-8正常中文；metadata状态和Ledger当前状态更新，不重写历史事实。回滚仍仅限获批后反向本次测试/文档增量，不回退既有三包。
三包 VERIFIED 仅指本轮批准的 impact 范围与上述分层证据，不代表整个战斗系统完全对齐；既有 USER_HOLD、内容Direction-B与默认stage资产暂缓继续有效，不启动后继任务。

Goal19 / 2026-09-12 / PLANNED / TEST_FIRST: `NTSD28-B6-NTSDSPEC-COMPAT-WEAPON-ACTION-PRODUCTION-001`, `NTSD-BATTLE-MESH-SUBMESH-GROWTH-INITIALIZATION-001`. W1 then M1; shared regression once at batch end. User authorization limited to declared paths. Records/tasks established before scripts. Evidence Temp/Goal19_*.

Goal19 progress: `NTSD28-B6-NTSDSPEC-COMPAT-WEAPON-ACTION-PRODUCTION-001` FOCUSED_TEST_PASS / 771_OF_771 / canonical current-weapon Play32PASS / LegacyPlay pending; `NTSD-BATTLE-MESH-SUBMESH-GROWTH-INITIALIZATION-001` PLANNED, no M1 script edits. Shared regression not yet run. Evidence Temp/Goal19_W1_*.

Goal19 W1 `NTSD28-B6-NTSDSPEC-COMPAT-WEAPON-ACTION-PRODUCTION-001` RUNTIME_PENDING: focused771PASS + dual current weapon Play32/32, 512 fields no difference, cleanup/asset hashes pass; only shared batch gates pending. M1 `NTSD-BATTLE-MESH-SUBMESH-GROWTH-INITIALIZATION-001` IN_PROGRESS / TEST_FIRST / PRODUCTION_UNCHANGED.

Goal19 pre-shared gate: W1 and M1 RUNTIME_PENDING solely for shared batch acceptance; W1 focused771 + dualPlay32/32/512fields equal; M1 focused22 + real rendering Play +0B stable high-water path. Shared regression will run once using Temp/Goal19_SharedRegressionPlan.json.

## Goal19 closure / 2026-09-12 / VERIFIED (scoped)

W1 `NTSD28-B6-NTSDSPEC-COMPAT-WEAPON-ACTION-PRODUCTION-001`: shared nine-field nonzero/fallback selector; canonical delegates without selection behavior change; Legacy relation/stats and exact held call-site gates/counters/direct writes corrected; old table bool action consumers retired, NTSDSpec API unchanged. Authority input_routing.cpp808-879/997-1093/1163-1169/1200-1313/1350-1490, formal EXE SHA B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033; README_SOURCE + playable build inclusion checked. RED672=209FAIL/463PASS, plus separate boundary RED2/6,1/2,4/10. Final focused771/771. Real driver NTSD_Battle Naruto2 + current weapons122/123, profiles32/32 each, seed424242/tick21..52, 512 compared fields firstDifference=null, cleanuptrue. Nonzero stats remain fixture-only; current zero stats and pic999 content mean fallback/field proof, no physical keyboard or weapon pixel claim.

M1 `NTSD-BATTLE-MESH-SUBMESH-GROWTH-INITIALIZATION-001`: RED10=7PASS/3FAIL, one actual MinMaxAABB at growth plus three index-range overlap warnings and NaN accepted. Not falsified. Selected batch SetSubMeshes candidate with reusable value-only staging, current finite vertex upload first, batch only changed counts/ranges, stable active-prefix and stale-tail handling retained. No Build/CreateMesh/index template changes; no log filtering or enlarged bounds. Final focused22/22 (14new+8existing), includes original3 SelfCheck groups covering the8 reported sites, cache recovery and NaN/+Inf/-Inf. Stable high-water4096/active1,256Builds:0B and4.930078125us average in this environment. Real normal Play tick2759 observed; full1920x1080 battle image visually verified in Temp/Goal19_M1_RenderPlay_Camera.png (byte-identical to first screenshot); original composited capture2errors retained, not a mesh failure. No GPU performance claim from zero EditorStats.

Exactly one shared regression job637100deade44a0eb4bf25d0d9950b9c:1778/1778 PASS = originalB6964 + W1771 + M114 + existingbackend8 + refill9 + nativeground12. Original B6 class counts unchanged; no old case missing. Full SelfCheck requested once, fresh PASS at2026-09-12T10:09:52Z. Actual builds: `dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly` and Editor equivalent, both exit0/errors0, warnings47/104. Validator passes with explicit RepositoryRoot and process-only Git warning settings; first default-parameter invocation failure retained, validator source/.git unchanged. Final Console9errors =7expected registration/rest negative fixtures +2composited screenshot-tool errors; MinMaxAABB0/overlap0, do not claim Console0.

Scene NTSD_Battle remains loaded, isDirty=false/root13; SHA D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11. Existing user modifications preserved; no unexpected script paths or staged files. No schema/NTSDSpec/Gen/Plugins/input sampling/content/Scene changes, no git add/commit/push. These facts close only Goal19 scope, not full battle parity or outstanding content strategy.

Authoritative batch evidence index: Temp/Goal19_FinalSummary.json; raw RED/focused/shared XML, Play comparisons, screenshots, Console, build and validator files linked there. Earlier PLANNED/CODE_WRITTEN/RUNTIME_PENDING entries are superseded by this closure, while their failures/corrections remain preserved.

Goal20 R1 `NTSD28-B6-LEGACY-TRACKER-PRODUCER-RETIREMENT-PRODUCTION-001` / IN_PROGRESS / TEST_FIRST. Contract and Record established before scripts; exact scope in docs/ai/TASKS/NTSD28-B6-LEGACY-TRACKER-PRODUCER-RETIREMENT-PRODUCTION-001.md. R2 waits for R1; R3-R5 preflight only. Evidence Temp/Goal20_*. Schema/carriers preserved; no Git mutation.

Goal20 R1 CODE_WRITTEN/focused10of10 (new5+kind5existing5), runtime/shared pending. R2 `NTSD28-B6-LEGACY-GRABBEDBY-NONZERO-PRODUCER-RETIREMENT-PRODUCTION-001` IN_PROGRESS/TEST_FIRST; exact Task/Record created before scripts.

Goal20 progress: `NTSD28-B6-LEGACY-TRACKER-PRODUCER-RETIREMENT-PRODUCTION-001` FOCUSED_TEST_PASS (new5+kind5existing5); `NTSD28-B6-LEGACY-GRABBEDBY-NONZERO-PRODUCER-RETIREMENT-PRODUCTION-001` FOCUSED_TEST_PASS (new6+G16P3existing140). R1RED3FAIL after2reachablePASS; R2 correctedRED6FAIL. Play/shared/build/validator pending.

Goal20 `NTSD28-B6-LEGACY-WEAPON-STATE-BEHAVIOR-RETIREMENT-PRODUCTION-001` IN_PROGRESS / TEST_FIRST / PRODUCTION_UNCHANGED. Exact Task/Record created; main agent controls Unity serial validation. Schema/reserved shape unchanged. Evidence Temp/Goal20_*.

Goal20 `NTSD28-B6-LEGACY-RELEASE-TICK-PRODUCER-RETIREMENT-PRODUCTION-001` IN_PROGRESS / TEST_FIRST / PRODUCTION_UNCHANGED. Exact Task/Record created; main agent controls Unity serial validation. Schema/reserved shape unchanged. Evidence Temp/Goal20_*.

Goal20 R1/R2 RUNTIME_PENDING solely for batch shared gates; focused10/146PASS and targeted PlayPASS (G16 two actual pickup witnesses, current51/279->213/0 OPoint relationship seam, fulltick/unregister reserved0/null, objects4->4). Temp/Goal20_R12_PlaySummary.json defines precise boundary; no full skill/keyboard claim.

Goal20 R5 `NTSD28-B6-LEGACY-HOLDERCOPY-RESIDUAL-RETIREMENT-PRODUCTION-001` IN_PROGRESS / TEST_FIRST / PRODUCTION_UNCHANGED; Task/Record and fresh52production references/22files callgraph Temp/Goal20_R5_Callgraph.json established. True lifecycle defaults99/-1 retained, root-copy/stage/held/legacy-holder stats behavior retired only after RED. Main owns integration and Unity serial execution.

Goal20 R1-R5 RUNTIME_PENDING for shared batch gates. R1focused10/R2focused146/R3focused5/R4focused13/R5focused157 all PASS; R3 native normalized firstDifference=null and currentPlayPASS, R4 currentPlay4PASS only ReleaseTick differs, R1/R2 targetedPlayPASS. Shared1824 planned once (B6 1735+37), fullSelfCheck/two builds/finalvalidator pending.

Goal20 final package statuses:

- `NTSD28-B6-LEGACY-TRACKER-PRODUCER-RETIREMENT-PRODUCTION-001` / VERIFIED / RED 3FAIL after factory reachability2PASS; focused10/10.

- `NTSD28-B6-LEGACY-GRABBEDBY-NONZERO-PRODUCER-RETIREMENT-PRODUCTION-001` / VERIFIED / RED 6FAIL; focused146/146.

- `NTSD28-B6-LEGACY-WEAPON-STATE-BEHAVIOR-RETIREMENT-PRODUCTION-001` / VERIFIED / RED 3FAIL/2PASS; focused5/5; native normalized firstDifference/firstChecksumDifference/firstMotionDifference null.

- `NTSD28-B6-LEGACY-RELEASE-TICK-PRODUCER-RETIREMENT-PRODUCTION-001` / VERIFIED / RED 9FAIL/4PASS; focused13/13; current Play4/4 only retired carrier differs.

- `NTSD28-B6-LEGACY-HOLDERCOPY-RESIDUAL-RETIREMENT-PRODUCTION-001` / VERIFIED / RED 7FAIL/1PASS; focused157/157; fresh production references52/22files to29/13files, only defaults/carrier/diagnostics remain.

Goal20 final scoped closure, 2026-09-12: VERIFIED for the authorized R1-R5 behavior retirement only. Full SelfCheck fresh PASS: Temp/Goal20_FinalSelfCheck.result (attempt4; earlier failures retained). Actual commands: dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly and dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly; both exit0/0errors, 47/104 warnings, Temp/Goal20_AcceptanceRuntimeBuild.txt and AcceptanceEditorBuild.txt. One shared1824 job completed with20 old ReleaseTick expectation failures; only authorized assertion rebaseline followed by affected24/24 PASS. Original broad FAILED receipt is retained, no second broad run or standalone all-green1824 claim; Temp/Goal20_SharedRegressionReconciliation.json. B6 coverage1735+37=1772, refill9 included.

Targeted runtime evidence: Temp/Goal20_FinalReservedPlayResult.json, Goal20_R3_PlayWitness.json, Goal20_R4_CurrentPlay_GREEN.json. Current OPoint seam, G16 pickup witnesses, weapon prepass and release pass are covered; no physical-key/full-skill or full native-world checksum parity claim. R3 native comparator covers two isolated hit_Fa pre-frame-advance calls, seed424242/empty input, identical normalized schema; tick1 checksum/tick2 motion differences disappear. No full tick/physics equivalence claim.

Disk Scene SHA remains D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11. Final editor scene isDirty=true/root13; source of dirty flag UNKNOWN, no save/clear performed, so scene-dirty-unchanged is NOT claimed. External UIPanels deletions/new images appeared during work and were not performed or modified by this batch; preserve them. Final Console snapshot: MinMaxAABB0/Overlap0; 7 expected fault-injection errors plus1 MCP disposed-connection error, warnings0; do not claim Console0errors. Temp/Goal20_FinalScene.json, FinalErrors.json, FinalWarnings.json, FinalScopeAudit.json.

Reserved contract: GrabbedBy0, TrackerFlag0/TrackerParentnull, WeaponState0, ReleaseTick-1. HolderCopy retains actual type/lifecycle defaults (runtime/Character/SpecialAttack99; Weapon/Other-1; task-1), not a new uniform default. Existing synthetic sentinels remain for no-write/fingerprint tests. Schema/snapshot/checksum/parity/ECS fingerprint structures, +2F8, NTSDSpec, Gen, Plugins and task content/Scene remain untouched; no staged files or git add/commit/push. Broader battle alignment and joint schema migration remain incomplete. Earlier progress statements are superseded by this closure; failures and correction history are retained. Final evidence index: Temp/Goal20_FinalSummary.json. Final validator receipt is appended after execution.

Final validator executed: Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity; exit0 PASS,459 records/28 governed code files, Temp/Goal20_Validator.txt. Process-only Git config environment avoided unavailable user global ignore; no Git config files changed. Final git diff --check exit0. All changed scripts also explicitly covered by these five Goal20 Records.

NTSD28-Q06-TYPE3-BDEFEND-EDITOR-ORACLE-001 VERIFIED / TEST_ORACLE_ONLY (13/13 PASS)。两条旧type3 Editor字段观测修订；原失败保留，生产不改，详见同ID Record。

NTSD28-Q06-NATIVE-INPUT-MISSING-STATE-ROUTING-001 IN_PROGRESS / SOURCE_WITNESS_FIRST：准确Change Record已建立，Tools+单测试+BCAW限定范围，生产尚未改。

NTSD28-Q06-NATIVE-INPUT-ACTION-COST-FRAME-READERS-001 IN_PROGRESS / SOURCE_WITNESS_FIRST。准确Record已建立，生产未改；三BCAW符号及单Source/Editor测试。
> 当前BATCH-04/Q07 `NTSD28-Q07-PORTABLE-OBJECT-CONTENT-STAGING-001`资源字节暂存VERIFIED：1343正式文件/46,594,829字节逐hash一致，精确新根`Assets/NTSD/Content/LoganRuntime`；GameConfig仍空根，旧内容生产未切换、Q07未交付。下一对staged root做candidate/identity/加载与退出验收，再声明生产切换Task；不可删旧文件或使用computer-use。证据见Q07 READINESS.md和Task/Record。Q06 DELIVERED_SCOPED保持。
> 当前BATCH-04/Q07 `NTSD28-Q07-WINDOWS-PLAYER-RUNTIME-001`已限定VERIFIED_PLAYER_CONTENT_BOOTSTRAP_ONLY：新Windows Mono Development build errors0/正式1343侧载，独立隐藏Player run-3 exit0/PASS；正式fingerprint、三owner相同key、World4、29400资源关闭后存活0、pool借用0、两帧Stopped。Player log有13条旧SFX目录缺失，Q10/portability回访；GameConfig仍空根，生产切换和自然技能/可见表现后续。Q06 DELIVERED_SCOPED不重开，Scene旧SHA保持，禁computer-use/非战斗修改。详Q07 WINDOWS-PLAYER-RUNTIME-ACCEPTANCE.md。
> 当前BATCH-04/Q07 `NTSD28-Q07-PRODUCTION-CONTENT-ROOT-SWITCH-001` IN_PROGRESS：修改前精确Task/Change已建，待GameConfig资产单字段正式根、既有Development Player探针序列化配置模式与实际构建/运行验收。Q06 DELIVERED_SCOPED、显式根Player PASS保持；Scene/旧资源/非战斗不动，禁computer-use。
> 当前BATCH-04/Q07 `NTSD28-Q07-SERIALIZED-MENU-CALLER-001` IN_PROGRESS：GameConfig正式根Player战斗Scene通过；共享资产的菜单预热caller须聚焦回访，准确Task/Change已建，下一仅改既有Editor Play探针Request/PrepareRequest并运行真实Play。Q06保持DELIVERED_SCOPED，禁computer-use/非战斗生产/Scene/旧资源改动。
> 当前BATCH-04/Q07 `NTSD28-Q07-NARUTO-FORWARD-ATTACK-PHYSICAL-001` IN_PROGRESS：正式EXE/source身份与staged Naruto DAT SHA已核对，站立hit_Fa285、frame286 OID33；修改前Task/Change已建，下一仅扩展BattleComboPlayModeProbeEditor测试脚本以唯一Q07请求做物理L+前+J、frame/OID轨迹。GameConfig正式根、Q06及旧引用审计保持；禁computer-use/生产/Scene/旧资源改动。
> Q07动态旧资源读取新发现：正式GameConfig根下GameDataManager仍无条件读取旧`data.txt`，正式对象发布保留旧背景；旧背景0条、正式目录24条，SimulationTickDriver将当前BackgroundCount写入RuntimeStageCount，BattleResultsWriter用该值判定stage轮换。静态候选首差交Q08做正式playable/Unity同条件见证与最窄修复，不能提前宣称运行时差异。详`artifacts/diagnostics/NTSD28-Q07-OLD-ASSET-REFERENCE-REFRESH-001/DYNAMIC-REACHABILITY.md`。旧data.txt和资源均保留、禁删；Q07继续自然技能/图片引用和Menu闭包，Q06保持DELIVERED_SCOPED，禁computer-use。
> 当前Q07 `NTSD28-Q07-NARUTO-CLONE-CENTRAL-PIXEL-WITNESS-001` 限定VERIFIED：两首轮测试FAIL保留；fresh q07-clone-pixel-3及同轮自然L/D/J q07-naruto-clone-5 PASS，tick12 OID33 pic1中央命令stableId103/slot51，生产相机全图2583/投影区1087非清屏像素，PNG已留。Editor退出、Scene dirtyfalse/root14/双SHA不变、Ledger651/13 PASS。只闭单例渲染路径，原EXE像素/排序/阴影/完整技能继续Q09/Q12；详同ID ACCEPTANCE，Q07仍IN_PROGRESS，禁computer-use。
> 当前Q07 `NTSD28-Q07-LEGACY-DATA-LAZY-LOAD-001` 限定VERIFIED：初始化旧`data.txt`隐式读取已退，空根显式加载保留；聚焦EditMode两次1/1、正式完整发布1/1、序列化根menu Play q07-lazy-menu-1 PASS，关闭零残留。最终无行为的空覆盖删除后重新编译/聚焦通过，完整发布/Play是其前等效路径；详ACCEPTANCE。Q08正式背景/结果计数待权威见证，旧资源/Scene/非战斗保持。
> 当前Q08 `NTSD28-Q08-F4-PLAYER-CLOSE-OWNER-001 / IN_PROGRESS`：正式F4整应用关闭效果的精确Task/Change已建立，限定SimulationTickDriver、AppManager及定向Player探针/构建入口；脚本实施与Player验证待。Editor不得被F4退出，录像save-pending保护因Unity尚无对应owner不冒称已闭。Q07首Scene决定、Q08结果计数独立待办，Q06本地出口保持。
> 当前Q07 `NTSD28-Q07-SASUKE-EDITOR-PREVIEW-FORMAL-IMAGE-001 / FOCUSED_TEST_PASS_ISOLATED`：原/隔离副本六项输入SHA一致，Unity 2022.3.62f3正式PNG 1/1、预览类11/11 EditMode PASS，XML见PROGRESS；原Editor仍待Scene手动Reload及可见预览图验收，Q07未闭。禁computer-use。
> Q08 `NTSD28-Q08-STORY-SELECTOR-RED-001 / FOCUSED_TEST_PASS`纠正：真实Menu MatchConfig无stage campaign赋值且缺正式paired story ID，前4/1 stage代理RED不构成authority缺陷，原XML保留。校正后隔离Unity 5/5PASS，正式story selector仍未接；直接战斗group/timing RED不受影响。详STORY-SELECTOR-CALLER-CORRECTION；生产/Scene/资源不改，Q06本地出口保持，禁computer-use。
> Q08 `NTSD28-Q08-NATIVE-RESULT-CARRIER-001 / RUNTIME_PENDING`：独立正式组/预战斗计时producer与roster2/aggregate26/checksum29已写，隔离Unity 8/8+restore1/1+相邻2/2/3/3及完整SelfCheck日志PASS；Q05目标schema断言PASS，全类另4项clone/旧raw夹具FAIL如实保留。真实战斗/close-reenter、>=144继续、旧UI101消费仍待；mode4 reserve和正式story入口不由本包删除/推断。原Scene SHA保持，Q06本地出口保持，禁computer-use。详本包ACCEPTANCE-PENDING。
> Q08下一精确差异：`NTSD28-Q08-RESULT-CONTINUE-INPUT-AUDIT-001`只读确认正式战斗前effective-combatant槽位Attack/Jump held、增至144即跳350；Unity新native载体未接此输入，旧结果UI读P1/P2 pressed且在战斗后。先闭roster/physical slot映射，再建独立Task/Change与聚焦RED，详同ID REPORT。Q08未闭、Q06出口保持，禁computer-use。
> Q08 `NTSD28-Q08-RESULT-CONTINUE-HELD-INPUT-001 / RUNTIME_PENDING`：正式有效参战槽位held Attack/Jump在计时>=144当tick跳350已接入native载体；隔离Unity RED0/2→聚焦10/10、相邻3/3、SelfCheck日志PASS，Battle Scene SHA保持。真实自然继续、旧UI101、phase3 combat skip仍待，mode4 reserve未动。详ACCEPTANCE-PENDING；Q08未闭、Q06出口保持，禁computer-use。
> Q08结果切场host审计`NTSD28-Q08-RESULT-TRANSITION-HOST-AUDIT-001`：正式350当tick及后继上层状态跳过combat driver，Unity当前native phase3后仍运行全战斗pass；旧结果UI phase11提前激活，`PendingHostAction`未见生产reader。禁止用单行早退掩盖结果UI/切场owner；下一正式349→350→后继tick与Unity全tick RED，按普通2/battle-only/mode4 202分支建准确Task/Change。详REPORT；Q08未闭、Q06出口保持，禁computer-use。
> Q08 `NTSD28-Q08-TRANSITION-FREEZE-SOURCE-WITNESS-001 / FOCUSED_TEST_PASS_SOURCE_MODEL_ONLY`：J:正式source hash匹配、独立C++ build exit0；mode4完整会话349→350→next双跑同SHA，所测实体字段/双RNG不变、last_tick两次为空。下一Unity同条件完整tick RED，再普通state2/battle-only分支；详TRANSITION-HOST-AUDIT REPORT。Q08未闭、Q06出口保持，禁computer-use。
> Q08 `NTSD28-Q08-TRANSITION-FREEZE-UNITY-RED-001 / RUNTIME_PENDING_INTENTIONAL_RED`：正式mode4切场冻结源码双跑已有；隔离Unity完整tick先通过350/transition202，再在FrameSequence预期349实际350目标RED，next断言未达。生产未改，下一host/结果UI owner整合，不能只早退；详TRANSITION-HOST-AUDIT REPORT。Q08未闭、Q06出口保持，禁computer-use。
> Q08 `NTSD28-Q08-ORDINARY-REMATCH-SOURCE-WITNESS-001 / FOCUSED_TEST_PASS_SOURCE_MODEL_ONLY`：普通与battle-only完整GameSession隔离双跑同SHA，350同样冻结；next分别选人(state1/同World)与显式重建(原死者HP500)，不可归并mode4 202。Unity host/结果UI101/真实Play待，详TRANSITION-HOST-AUDIT REPORT。Q08未闭、Q06出口保持，禁computer-use。
> Q08 P-19/G-08范围纠正：正式结果页表现/完整选人UI已由用户排除；撤回“Unity结果页必须101才显示”的验收要求，101仅作为逻辑结果记录事件。旧UI phase11是可能反写逻辑的耦合风险，须隔离；350停止战斗及切场请求仍必需，现有非战斗菜单不修改。详TRANSITION-HOST-AUDIT REPORT Scope correction；Q08未闭、Q06出口保持，禁computer-use。
> Q08 P-19/G-08例外的逻辑边界已审：旧Results `IsActive`提前改变human poll/CharacterInput/NeedClearInput，旧设置可写旧战斗实体及Match/Reserve；正式350前战斗仍推进、350起冻结。101须逻辑结果记录而非原生UI显示；现有UI表现保留但不得改战斗真值，ordinary2/battle-only/mode4 202各自host owner。详TRANSITION-HOST-AUDIT/P19-EXCEPTION-LOGIC-COUPLING.md；Q08未闭、Q06出口保持，禁computer-use。
> Q08 `NTSD28-Q08-RESULT-PAGE-COMBAT-INPUT-ISOLATION-001 / RUNTIME_PENDING`：旧Unity结果页`IsActive`对human poll、CharacterInput、NeedClearInput三处combat门槛已解耦，仍仅控制post-world页面输入。隔离Unity准确RED0/1→结果场景4/4、相邻held continue2/2 PASS；完整SelfCheck初旧oracle FAIL留证、修正后fresh PASS；Ledger664/6PASS、Scene SHA不变。真实战斗与350切场/冻结host仍待，详PAGE-COMBAT-INPUT-ACCEPTANCE；Q06出口保持，禁computer-use。
> Q08 `NTSD28-Q08-NATIVE-TRANSITION-COMBAT-FREEZE-001 / RUNTIME_PENDING`：正式350当tick及后继旧战斗核心已跳过，保留普通2→1和mode4 202分支；隔离Unity新增RED0/2→目标3/3、相邻17/17、完整SelfCheck PASS，正式身份/Scene SHA不变。driver仍先ApplyFrameInputSet/host tick，AppManager无native transition reader；必须另Task接host，不能标Q08闭。详TRANSITION-COMBAT-FREEZE-ACCEPTANCE；Q06出口保持，禁computer-use。
> Q08 `NTSD28-Q08-TRANSITION-HOST-TICK-ADMISSION-001 / RUNTIME_PENDING`：driver自动/Manual/Lockstep及暂停F2后继旧World tick/ApplyFrameInputSet已按非零native transition守门；隔离Unity目标RED0/1→PASS1/1、相邻25/25、完整SelfCheck PASS。AppManager无命令消费，需独立host route且保留2/28/128/202区别；详HOST-TICK-ADMISSION-ACCEPTANCE。Q08未闭、Q06出口保持，禁computer-use。
> Q08切场host下一入口`HOST-ROUTING-PRECHANGE-AUDIT.md`：driver在同步/worker完成后可读native transition，AppManager.UnloadBattle现固定回MenuMain，MenuUI已有ShowSelectCharacter；普通2→1与battle-only重开、28/128/202必须分路。EditorBuildSettings当前`m_Scenes: []`为Player路由部署门槛，不擅改Scene列表。下一建立准确host Task/Change并做定向route验收；Q08未闭、Q06出口保持，禁computer-use。
> Q08 `NTSD28-Q08-ORDINARY-RESULT-SELECTION-HOST-001 / RUNTIME_PENDING`：普通native命令2在主线程接AppManager有序卸载→现有选人；临时Menu/Battle真实Play三分支3/3、相邻28/28、完整SelfCheck及Ledger通过，原Scene SHA保持。前两轮测试因Preparing夹具错误不作有效RED，已在Record纠正。原Menu→Battle端到端受BuildSettings空表限制仍待；battle-only与28/128/202另Task，Q08未闭、Q06出口保持，禁computer-use。
> Q08 battle-only重开只读owner审计：正式根EXE与`game_session.cpp`/`main.cpp`新鲜SHA已核对；release启动配置位决定battle-only，Unity`MatchConfig`尚无显式载体，直接Battle Scene由`BattleTestBootstrap`创建MenuMain状态AppManager并恢复driver，普通Menu+Battle命令2接收者不会处理它。正式`start_selected_battle`首次成功后静态写`battle_scene_only_loop=false`，既有native夹具仅验第一次重建；下一必须先做第二次结果动态见证，再定Unity显式owner/RNG和input phase连续性，不凭Menu缺席猜模式或直接调用空World重建。详`artifacts/diagnostics/NTSD28-Q08-RESULT-TRANSITION-HOST-AUDIT-001/BATTLE-ONLY-OWNER-AND-SECOND-CYCLE-AUDIT.md`。Q08未闭，Q06出口保持，禁computer-use。
> 当前`NTSD28-Q08-BATTLE-ONLY-SECOND-CYCLE-SOURCE-WITNESS-001 / IN_PROGRESS`：准确Task/Change先建，拟以新诊断CPP及隔离原生测试拷贝对第一次重建后的第二次终局双跑见证；正式J:与Unity代码不动。未运行前第二轮分支只为源码推断，Q08未闭，Q06出口保持，禁computer-use。
> `NTSD28-Q08-BATTLE-ONLY-SECOND-CYCLE-SOURCE-WITNESS-001 / FOCUSED_TEST_PASS_SOURCE_MODEL_ONLY`：正式/隔离`game_session.cpp`同SHA，isolated focused build exit0，双跑exit0/同SHA；首次battle-only结果重建HP500且mode位改false，第二次350切场后下一调用进入selection/state1，same World/原死者HP0。详SECOND-CYCLE-SOURCE-WITNESS；下一直开Battle Scene Unity trace与显式mode/RNG/input连续owner，正式EXE可见第二轮仍待；Q08未闭/Q06出口保持，禁computer-use。
> `NTSD28-Q08-DIRECT-BATTLE-RESULT-HOST-PROBE-001 / IN_PROGRESS`：准确测试Task/Change已建；仅隔离Unity真实Battle Scene Play测bootstrap创建AppManager及命令2待处理/旧World冻结，不改生产/原Scene/资源。测试结果出来后再定显式battle-only host Task，Q08未闭/Q06出口保持，禁computer-use。
> `NTSD28-Q08-DIRECT-BATTLE-RESULT-HOST-PROBE-001 / VERIFIED_DIAGNOSTIC_ONLY`：隔离Unity真实Battle Scene D3D11 Play1/1，正式内容bootstrap完成，AppManager `[TestBootstrap]`仍MenuMain且无Menu，seed native结果命令2后五帧旧tick冻结/命令待处理。前两轮短等待和NullGfx最大4096失败不算行为首差，原Scene SHA保持；详DIRECT-BATTLE-RESULT-PLAY-ACCEPTANCE。下一显式battle-only首次重建Task需RNG/input连续及第二轮选人边界；Q08未闭/Q06出口保持，禁computer-use。
> 当前`NTSD28-Q08-BATTLE-ONLY-RNG-PHASE-SOURCE-WITNESS-001 / IN_PROGRESS`：准确Task/Change已建，仅新诊断CPP及隔离原生test拷贝，量首轮重开前后NativeRandom双流/input phase和默认BGM一次抽取；build/双跑待。Unity生产/Scene/资源/J:不改，Q08未闭/Q06出口保持，禁computer-use。
> 当前`NTSD28-Q08-BATTLE-ONLY-RNG-PHASE-SOURCE-WITNESS-001 / FOCUSED_TEST_PASS_SOURCE_MODEL_ONLY`：聚焦build0、正式runtime两次exit0且逐字节同SHA `4B6AEAE8…058EE2D`；第一轮重开CRT state/calls不变，input phase1→1，同步RNG calls1→2/lastSite `0x004021E0`（恢复后默认BGM）。报告Q08 RESULT-TRANSITION-HOST-AUDIT-001/RNG-PHASE-SOURCE-WITNESS.md。下一独立Unity host Task须先界定直接Battle Scene重开配置/World roster与第二轮回普通选择，再按恢复后BGM抽取顺序接入并定向Play；不能复用空World `RecreateWorld()`或seed重置。Q08未闭/Q06出口保持，禁computer-use。
> 当前`NTSD28-Q08-BATTLE-ONLY-REMATCH-UNITY-RED-001 / IN_PROGRESS`：准确测试路径Task/Change已建，真实Battle Scene形式内容启动后注入已证结果命令2，要求新World/roster/继续战斗；隔离Unity定向RED待。生产rematch owner尚未修改，审计见Q08 RESULT-TRANSITION-HOST-AUDIT-001/DIRECT-REMATCH-IMPLEMENTATION-BOUNDARY.md；Q08未闭/Q06出口保持，禁computer-use。
> 当前`NTSD28-Q08-BATTLE-ONLY-REMATCH-UNITY-RED-001 / TARGET_RED_CONFIRMED`：隔离Unity真实Battle Scene正式内容bootstrap完成，命令2后等待15秒World仍旧实例，XML1FAIL目标断言51行，无CS/NullGfx、原/克隆Scene SHA `9E7B8A…0AC0`保持。详Q08 RESULT-TRANSITION-HOST-AUDIT-001/BATTLE-ONLY-REMATCH-UNITY-RED.md及DIRECT-REMATCH-IMPLEMENTATION-BOUNDARY.md。下一另建准确生产Task/Change：显式battle-only owner、同scene十一阶段shutdown/map真清、direct roster再生、原生RNG/input restore后BGM一次抽取，首次后flag false，次轮普通选择；已有pending-command诊断测试届时需supersede。Q08未闭/Q06出口保持，禁computer-use。
> 当前`NTSD28-Q08-BATTLE-ONLY-FIRST-REMATCH-HOST-001 / IN_PROGRESS`：生产Task/Change已先建，4脚本准确声明，真实Battle Scene首轮RED及正式source双跑RNG/二轮证据已在Q08 RESULT-TRANSITION-HOST-AUDIT-001。先接显式直接owner、AppManager有序shutdown/map真清、新World同配置角色重生、旧RNG/input恢复后BGM一次抽取、封印/继续；失败不得恢复部分World。普通菜单和Q06已闭职责不改，次轮无Menu选人尚独立待；禁computer-use。
> 当前`NTSD28-Q08-BATTLE-ONLY-FIRST-REMATCH-HOST-001 / RUNTIME_PENDING`：准确4脚本首轮direct host已写，隔离D3D11正式内容Play2/2、旧诊断更新1/1、普通选择3/3、完整SelfCheck PASS；原/clone Battle Scene SHA `9E7B8A…0AC0`保持。详Q08 RESULT-TRANSITION-HOST-AUDIT-001/FIRST-REMATCH-HOST-ACCEPTANCE-PENDING.md。下一先单独证次轮结果350→普通上层选择state1/同World：当前已禁止第二次重开，但没有Menu Scene的AppManager不能完成选人；需新准确Task/Change+真实Play RED，再接逻辑路由，用户批准的选择UI视觉例外和非战斗不顺手改。自然KO/EXE可见仍待，Q08未闭/Q06出口保持，禁computer-use。
> 当前`NTSD28-Q08-SECOND-BATTLE-ONLY-RESULT-UNITY-RED-001 / IN_PROGRESS`：独立新测试路径Task/Change先建，隔离真实Battle Scene正式内容首轮重开后再造第二结果350，预期下步transition1/同World/冻结tick；尚未改次轮生产。按正式SECOND-CYCLE-SOURCE-WITNESS先得目标RED，再准确Task/Change接无Menu逻辑选人owner，不改已批准选择UI视觉例外。Q08未闭/Q06出口保持，禁computer-use。
> 当前`NTSD28-Q08-SECOND-BATTLE-ONLY-LOGICAL-SELECTION-001 / IN_PROGRESS`：准确3脚本生产Task/Change已建，正式source二轮sameWorld/transition1与隔离临时host RED2≠1归档SECOND-CYCLE-RED-AND-MEMORY.md；真实正式资源Scene两次在broken.png附近OOM，非逻辑失败。下一只接显式direct owner次轮命令2→1，保持同World/冻结tick、普通Menu owner与UI不动，跑代理+普通相邻；正式Scene复验待内存环境。Q08未闭/Q06出口保持，禁computer-use。
> Q08第二轮直接战斗结果 `NTSD28-Q08-SECOND-BATTLE-ONLY-LOGICAL-SELECTION-001 / RUNTIME_PENDING`：正式source二轮同World进入selection/state1；Unity显式direct owner第二轮命令2→1已写，隔离无图临时host目标RED→Play1/1 PASS，普通Menu+Battle相邻3/3 PASS。正式内容Battle Scene两次图片加载OOM，无第二轮真实场景、自然KO或选择前端验收；详Q08 RESULT-TRANSITION-HOST-AUDIT-001/SECOND-BATTLE-LOGICAL-SELECTION-ACCEPTANCE-PENDING.md。下一先确认OOM后再做真实二轮，且复核upper-selection roster语义。Q08未闭，Q06限定出口保持，禁computer-use。
> `NTSD28-Q08-ATLAS-ALLOCATION-TRACE-001 / PLANNED`：Q08正式资源Scene两次在atlas页Texture2D创建处OOM、无第二轮逻辑XML；仅opt-in诊断`BattleAtlasResources.cs`的准确Task/Change已建，拟记录页数/array决策/页进度并外部采样进程内存。尚未改脚本或再跑Scene；Q08未闭，Q06出口保持，禁computer-use。
> `NTSD28-Q08-ATLAS-ALLOCATION-TRACE-001 / RUNTIME_PENDING`：隔离正式内容Scene opt-in trace得145张2048²页、array 2.43GB超512MiB预算拒绝、第87页完成/第88页创建OOM，进程私有字节两秒采样峰值18.0GB；无XML/结果逻辑结论。详Q08 RESULT-TRANSITION-HOST-AUDIT-001/ATLAS-ALLOCATION-TRACE-RESULT.md。仅一脚本诊断开关，下一另立资源生命周期/环境Task，不能盲增array预算或改战斗逻辑；Q08未闭、Q06出口保持，禁computer-use。
> `NTSD28-Q08-ATLAS-ORDERED-PAGE-STREAMING-001 / PLANNED`：已按145页/第88页OOM证据建立准确两脚本Task/Change；仅当array被capability policy拒绝时拟逐页CPU拼装/上传，保持同plan/像素/绑定/array fallback。尚未改生产；先多页等价测试后正式内容Scene+内存采样。Q08未闭，Q06出口保持，禁computer-use。
> `NTSD28-Q08-ATLAS-ORDERED-PAGE-STREAMING-001 / RUNTIME_PENDING`：仅array拒绝分支逐页CPU拼装/上传，原数组路径保留；两页逐像素等价及绑定聚焦3/3 PASS。正式内容Scene第88→119页、采样峰值18.02→17.40GB但仍OOM/无XML；需独立source texture/atlas双重驻留策略与真实表现验收，不能关闭Q08。详Q08 RESULT-TRANSITION-HOST-AUDIT-001/ATLAS-ORDERED-STREAMING-PARTIAL.md。下一先审现有SourceTexture2D中央绑定的像素/排序/owner，再准确Task；禁盲增array预算。Q06出口保持，禁computer-use。
> `NTSD28-Q08-ATLAS-BUDGET-SOURCE-BINDING-001 / PLANNED`：正式Auto145页2.43GB超既有AtlasMemoryBudget512MiB，ordered逐页后仍第119页OOM；准确policy/manager/atlas测试3脚本Task/Change已建。拟Auto超预算保留现有SourceTexture2D中央绑定，显式atlas模式与小内容保持；未改生产，需源绑定/排序/像素与真实Scene验收。Q08未闭、Q06出口保持，禁computer-use。

> 本项进展修正为`RUNTIME_PENDING`：三脚本已实施；隔离Unity聚焦4/4、正式内容D3D11 Battle Scene第二轮1/1且进程exit0，私有字节采样峰值13,242,007,552。早期统一入口遗漏所致RED3/4保留。代表像素/绘制顺序及正式EXE可见画面仍待，不关闭Q08或重开Q06。详Change Record及Q08报告，禁computer-use。

> `NTSD28-Q08-FORMAL-SOURCE-PIXEL-WITNESS-001 / PLANNED`：正式内容SourceTexture2D回退的结构绑定/顺序与结果流已验，真实相机像素仍待。单个战斗Play测试的Task/Change已建，尚未改脚本；只验证实际生产相机出图及政策/指纹，不冒称正式EXE逐像素一致。Q08/总目标未闭、Q06本地出口保持，禁computer-use。

> Q08正式源纹理相机见证（2026-09-22）：`NTSD28-Q08-FORMAL-SOURCE-PIXEL-WITNESS-001 / RUNTIME_PENDING`在隔离非batch D3D11 Editor NUnit1/1PASS，正式指纹、Auto145页2.43GB超512MiB预算、SourceTexture2D、中央tick34/4命令及960×540图2015非白像素有证；原项目现有Editor无第二进程亦产生同hash PNG/JSON，原NUnit回调结果未落盘，不能报原Editor测试PASS。原项目同时Q07物理Naruto探针首键8次未入FrameInputSet而FAIL，与源纹理像素分开审。Scene磁盘SHA保持；正式EXE像素/全角色与技能未验，Q08/总目标未闭、Q06本地出口保持。详`artifacts/diagnostics/NTSD28-Q08-FORMAL-SOURCE-PIXEL-WITNESS-001/ACCEPTANCE-PENDING.md`，禁computer-use。
> Q09全局spark静态首差（2026-09-22）：正式`resource.dat` index43 `SPARK.png` + `system.dat` 99×79；Unity生产仍旧`SPARK.bmp`/20帧旧矩形，正式全局PNG未暂存。清单与后续原项目验证出口见`artifacts/diagnostics/NTSD28-Q09-GLOBAL-SPARK-RESOURCE-BOUNDARY-001/REPORT.md`。未改脚本/资源/场景，Q06事件职责不重做。原项目Editor活但Q08新测试尚未重编；CLI无Pipeline连接，禁开第二项目与computer-use。

> NTSD28-R15-VERSIONED-TRACE-HEADER-001 / IN_PROGRESS（2026-09-22）：修改前准确Task/Change及Ledger已建立，限定Unity trace emitter、parity validator/self-test、两项正式根Editor断言。目标是V1/V2严格头与17/25/28/2/2、17/26/29/2/2精确schema身份；native V2诊断捕获、原项目Unity编译/运行及同seed/input验收仍另待。Q07/R15未闭，禁computer-use/第二Unity项目。

> NTSD28-R15-VERSIONED-TRACE-HEADER-001 / CODE_WRITTEN（2026-09-22）：Unity emitter严格分V1无mode及V2五组件，parity工具严格验证版本/哈希/旧17-25-28与当前17-26-29 schema并在header拒绝跨版本；Release build0错、自测157/157、Ledger682通过。正式根Q05/Q06两Editor断言已迁V2，review发现的fallback跨V1/V2弱断言已纠正；原项目Unity程序集仍旧，五脚本未在原Editor编译/运行。native source-model仍V1/25/28、正式新V2双端同seed/input比较未做，另有固定V1 Play探针需独立回访；Q07/R15未闭，禁computer-use/第二Unity项目。证据artifacts/diagnostics/NTSD28-R15-VERSIONED-TRACE-HEADER-001/parity-self-test.json及同ID Change Record。

> NTSD28-R15-FORMAL-PROBE-V2-VECTOR-001 / IN_PROGRESS（2026-09-22）：修改前准确Task/Change和Ledger已建，限定九个战斗验证脚本十处正式根旧V1固定身份文字为当前V2独立向量；不改输入/行为/Scene/生产资源/非战斗。原Editor编译/探针待，Q07/R15未闭，禁computer-use/第二Unity项目。

> NTSD28-R15-FORMAL-PROBE-V2-VECTOR-001 / CODE_WRITTEN（2026-09-22）：九个战斗测试/探针十处正式根固定V1值精确改为已复核V2 semantic FF1218FF...及projection 9F40EB3FFF1812FF；旧值源码搜索0，diff仅十行预期文字。纯源正式/暂存向量检查PASS，原Editor程序集/探针仍未编译运行。native V2 source-model下一包的只读准确边界见artifacts/diagnostics/NTSD28-R15-NATIVE-MODE-CAPTURE-CONTRACT-001/REPORT.md；正式源码不改、旧native V1捕获不重标。Q07/R15未闭，禁computer-use/第二Unity项目。

> NTSD28-R15-NATIVE-MODE-V2-CAPTURE-001 / IN_PROGRESS（2026-09-22）：修改前准确Task/Change及Ledger已建，唯一代码路径为Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp；正式playable源码/EXE只读，旧V1 JSONL不重标。目标新鲜source-model正式mode双DAT五组件V2、实际session.config完整核对、17/26/29当前trace兼容声明及bundle失败门槛；原项目Unity编译/同条件双端仍待。见同ID Task和R15 NATIVE-MODE-CAPTURE-CONTRACT报告。Q07/R15未闭，禁computer-use/第二Unity项目。

> NTSD28-R15-NATIVE-MODE-V2-CAPTURE-001 / FOCUSED_TEST_PASS_SOURCE_MODEL_ONLY（2026-09-22）：仅仓库诊断runner读取正式mode父/子DAT并核对实际GameSession完整combo配置；未改J:正式源码/Unity生产/Scene。final-build 0错，正式3tick主/domain/B2双跑各逐字节同；五组件V2原始/语义/投影与正式及暂存Unity纯投影一致，三个validator PASS。无mode V1保留；缺子/残缺/初始化及运行中路径、优先级、字节变化均失效并标三路bundle。证据artifacts/diagnostics/NTSD28-R15-NATIVE-MODE-V2-CAPTURE-001/及同ID Record。仍仅SOURCE_MODEL_DIAGNOSTIC_ONLY；原项目Unity新程序集/同seed双端tick首差/正式EXE可见表现未验，Q07/R15未闭，禁computer-use/第二Unity项目。

> Q09 combo显示链只读入口（2026-09-22）：`NTSD28-Q09-NATIVE-COMBO-DRAW-CHAIN-AUDIT-001`核对正式mode子DAT的`%d_hit`、offset20/21、effect20、15x15/16x16 atlas和同Z实体命令顺序；Unity Q06计数/到期字段已有，Q07仅World tuple，presentation链未见计数字段读者，正式combo_hits.png已暂存。Q09需独立全视觉记录/命令/图集消费包，先完成原项目Q07新程序集及mode激活验证，再同seed命令和实际像素对照；禁重做Q06、改菜单/结果。详同ID REPORT 与对齐总表。原Editor PID33236仍运行但程序集截至04:23:44Z早于新代码；项目请求文件只能执行已加载入口，不等于刷新。禁computer-use/第二Unity项目。

> Q07/R15离线源码编译（2026-09-22）：`NTSD28-Q07-OFFLINE-COMPILE-PROBE-001 / OFFLINE_CSHARP_COMPILE_PASS`。原生成csproj漏两个新增文件导致直接MSBuild CS0246；Temp targets明确补齐`LoganModeComboInput.cs`及Q07 Editor测试后，运行时与Editor构建exit0，`UNITY_INCLUDE_TESTS`与Compile输入核对通过。Library原Editor程序集仍旧，Unity compile/NUnit/Play/同seed trace未验；不能把离线结果升级为Q07/R15完成。详同ID REPORT；不重启第二Editor，不用computer-use。

> Q09正式全局spark原始ID/atlas入口（2026-09-22）：`NTSD28-Q09-NATIVE-SPARK-ATLAS-REACHABILITY-001`把resource43、system99×79、正式500×320 PNG、native 10×10逻辑ID/100×80步长及D3D11黑色键/越界跳画闭合；全100 ID表只有0-4/10-14/20-24/30-34在图内。Unity旧20图age mapper有28个映射age且20/21合并，同一旧路径被legacy/central使用；Q09需独立formal raw-ID发布与表现接线，不能简单替换SPARK.bmp，也不动Q06 producer/lifecycle。详同ID REPORT/CSV与对齐总表；正式PNG已暂存，原Editor仍旧程序集，Play/R14/R17未验。禁computer-use/第二Unity项目。
> Q07 三张角色头像 PNG 已按正式字节暂存（2026-09-22）：`c/0/M.png`、`custom/1genma/Mclone.png`、`custom/1genma/Mgenma.png` SHA/长度逐项一致，暂存 PNG 1015/正式尚缺 240。正式 parser/host/renderer 证明其 `layer:` 仅用于 `<menu_face>` 选择头像；早先战斗层推断已在原 Task/Acceptance 追加纠正，不能当 Q09 进度或改菜单逻辑。原项目仍在，禁新 Unity 项目/computer-use；Q07/Q09 未闭。
> Q09 `NTSD28-Q09-NATIVE-WORDS-PNG-STAGING-001 / VERIFIED_STAGING_ONLY`（2026-09-22）：正式 `resource.dat` 索引16～21及 playable 名牌/HUD绘制链确认战斗内使用六张 `sprite/UI/WORDS0.png`～`WORDS5.png`；原项目精确暂存并逐SHA通过，PNG现1021/正式缺234（b110/sprite124）。Unity仍读旧WORDS BMP，正式图reader/画面/EXE同条件未验，Q09/R17未闭。仅原项目，无新Editor/computer-use；详同ID Task/Acceptance。
> Q09 WORDS生产链只读纠正（2026-09-22）：`CharacterAnimtorManager` 只Build Shadow+Spark，`WithWords`仅测试使用；旧BMP描述是API/诊断文案，**生产没有发布WORDS绑定**，先前“仍读旧WORDS BMP”表述作废。正式六PNG已按SHA暂存，原生/Unity字形几何基本相容；下一精确Q09 Task/Change接六表解码、原子发布、身份/关闭和原项目画面，不可只换路径。详 `NTSD28-Q09-NATIVE-WORDS-PUBLICATION-CALLER-AUDIT-001/REPORT.md`，Q09/R17未闭，无新Unity项目/computer-use。
> Q09 WORDS新鲜度边界（2026-09-22）：现有 `LoganVisualContentCandidate`只哈希角色`files/head/small`，不捕获六张全局WORDS PNG；正式图预热接入前须版本化候选/发布身份并在commit前复核。原Editor PID33236仍运行，但Unity2022.3无Pipeline，CLI `STATUS_NO_INSTANCES`；Library程序集04:23:43/44Z早于Q07新源码，不能宣称新Unity编译/Play。无安装/新实例/computer-use。详Q09 `NATIVE-WORDS-PUBLICATION-CALLER-AUDIT-001/REPORT.md`，Q07/Q09/R17仍开。
> `NTSD28-Q09-WORDS-INPUT-IDENTITY-001 / PLANNED`（2026-09-22）：精确Task/Change已在脚本前建，声明仅`LoganVisualContentCandidate.cs`和新聚焦Editor测试/meta；正式resource.dat索引16..21与六PNG进入可选V2视觉身份/新鲜度，缺resource保留V1，不改战斗规则身份、菜单或生产绘制。原项目Editor新程序集仍待；无新Unity/computer-use。
> `NTSD28-Q09-WORDS-INPUT-IDENTITY-001 / CODE_WRITTEN / OFFLINE_COMPILE_PASS / ORIGINAL_UNITY_PENDING`（2026-09-22）：仅`LoganVisualContentCandidate.cs`与新聚焦测试/meta按Task/Change改；正式resource.dat索引16..21及六PNG进入可选V2视觉身份，缺resource保持V1/906角色图，battle规则身份不变。离线原项目runtime/Editor编译0错、编译后实际输入投影正式/暂存六图SHA指纹相同 `0E963A...5B24`，缺失/图字节/DAT字节/越界四项独立夹具PASS。原Editor NUnit/Play、实际WORDS原子发布与画面仍待；详同ID ACCEPTANCE-PENDING。禁新Unity/computer-use。
> `NTSD28-Q09-WORDS-PUBLICATION-001 / COMPILE_PASS / FOCUSED_RESULT_PENDING`（2026-09-22）：脚本前Task/Change已建，原项目`CharacterAnimtorManager.cs`+新聚焦Editor测试/meta；正式resource索引16..21及六PNG经原预热/公共目录/atlas/ownership通道发布，保留tRNS中间alpha。原Editor PID33236本地桥接一次刷新成功，最终Editor DLL 08:11:44Z晚于测试源码且重载；`Temp/NTSD28_Q09_WordsPublication.request.json`已从true被消费为false，但`artifacts/diagnostics/NTSD28-Q09-WORDS-PUBLICATION-001/original-editor-test-result.txt`仍未产生。先观察同一次运行/诊断终态，不能报NUnit通过或盲重跑。画面/取消/关闭仍待；详本包`ORIGINAL-EDITOR-RUN-PENDING.md`。禁第二Editor/computer-use。
> Q08 `NTSD28-Q08-NONSTANDARD-KNOCKOUT-EVENT-PRODUCERS-001 / CODE_WRITTEN`：正式三处非标准KO writer已在既有单次计数旁追加事件，shared writer接受有效credit/无源slot并保留-1/1000默认字段；三个既有聚焦fixture已加字段/拒绝断言。精确七路径见改前Task/Change；Ledger PASS、原项目Temp-only runtime+Editor离线编译0错且测试入输入。原Editor编译/NUnit、SelfCheck、同seed、Play/关闭仍待。WORDS请求无终态，禁并发测试/第二Unity/computer-use。
> Q08/Q09 `#killtext`寿命静态首差：正式`GameSession28`仅配置载入时按`times`裁剪，bound0或mode未允许仍裁剪，缺配置不裁剪；Unity现无条件70。Q09发布mode身份保护的可选完整feed config后Q08接寿命，30tick显示窗独立。详`NTSD28-Q08-Q09-KILLTEXT-LIFETIME-HANDOFF-AUDIT-001/REPORT.md`；本轮无脚本/运行验证。原项目Editor PID33236程序集早于Q08新代码，WORDS请求无结果；禁第二Editor/computer-use。
> Q09 `NTSD28-Q09-KILLTEXT-MODE-INPUT-001 / CODE_WRITTEN`：改前Task/Change精确声明的双脚本+新meta现已从mode双DAT已捕获child字节投影完整`#killtext`，缺配置≠bound0，重复mode/id与图/sound路径保留；V2 raw身份不变。原项目Temp-only离线runtime+Editor编译0错，新测试入输入，反射直调正式字段/反例PASS；原Editor NUnit、图片身份/发布、World tick/画面后置。WORDS旧请求无终态，不并发TestRunner/第二项目/computer-use。
> Q08/Q09 `NTSD28-Q08-Q09-KILLTEXT-RUNTIME-LIFETIME-001 / CODE_WRITTEN`：已发布正式mode child可选寿命到World首tick前，Q08结果后仅配置存在时裁剪；reset/core snapshot13/aggregate28/checksum31/Unity extended+lockstep parity v3已接，frozen Authority400 v3不变。改前Task/Change十二准确路径，原项目Temp-only runtime+Editor离线编译0错、Ledger PASS；原Editor NUnit/SelfCheck/同seed/Play与R15版本化复核待。WORDS旧请求无终态，禁并发TestRunner/第二项目/computer-use。
> Q09 `NTSD28-Q09-KILL-ICON-INPUT-IDENTITY-001 / COMPILE_PASS`：原项目候选已把七槽正式图标路径/存在/字节并入V3视觉身份、发布前复核，保留旧WORDS和无feed V1/V2；compiled图标输入正式/暂存相同。原Editor 09:44Z Tundra成功/新测试导入，但域重载、NUnit、全候选及画面待；旧WORDS TestRunner无结果，禁并发测试/第二Editor/computer-use。详本包ACCEPTANCE-PENDING。
> Q09表现接线只读回访：正式战斗名字表10槽/括号8标志与Unity普通4玩家名不同；原Scene HUDCamera Canvas与中央world-camera渲染分离。下一先保留正式名字合同、做战斗行冻结快照；实际屏幕consumer须原Scene层级/坐标定向证据后选，不能靠中央实体命令或修改现有HUDBg x30蒙混。详Q09 ROW-PROJECTION-AUDIT报告；原Editor运行待，禁第二Editor/computer-use。
> Q09 `NTSD28-Q09-KNOCKOUT-FEED-ROW-SNAPSHOT-001 / PLANNED`：准确Task/Change/Ledger已建立，五路径仅战斗名字输入、已发布feed接线、帧快照/投影及聚焦测试；**无脚本改动**。正式+29/+30/+31、排除/缺slot行距、原记录顺序、名字、worker和冻结复制必须逐项验。原Editor WORDS测试已启动但无终态/结果，不并发发新的测试；Q09屏幕消费/层级/像素独立后继，禁第二Unity/computer-use。详本包Task Contract。
> Q09 `NTSD28-Q09-KNOCKOUT-FEED-ROW-SNAPSHOT-001 / CODE_WRITTEN / OFFLINE_COMPILE_PASS`：五个已声明脚本+两meta已写，正式mode feed/名字进入行快照，+29/+30/+31、缺失/排除行距、图标槽位、冻结副本和CentralOnly capture已实现；离线原项目runtime+Editor编译0错，`git diff --check`通过。原Editor尚未加载本次程序集，WORDS运行无结果，不并发发新TestRunner。screen consumer/图像发布、正式EXE视觉与真实Play仍待，Q09/R17开放。详本包ACCEPTANCE-PENDING；上一PLANNED为历史快照。
> 18:20原Editor状态纠正：Unity-MCP端口6404返回的是09:44:49 UTC旧缓存（`sequence=9`），`tests.is_running=false`非新鲜终态；PID33236仍活、WORDS结果仍缺、Library DLL仍旧。未发新测试或重启Editor。Scene YAML静态证实HUDCamera深度0晚于world base深度-1，Canvas直接子序HUD→ControBg/Combo/按钮/HP；HUD后控制前仅是screen consumer候选，需原Scene Play验证，不动HUDBg x30/普通HUD。详WORDS PENDING和ROW-PROJECTION-AUDIT。
> Q09 `NTSD28-Q09-KNOCKOUT-FEED-ROW-SNAPSHOT-001 / CODE_WRITTEN`：正式playable场景名字覆盖的10槽/每槽≤10字节/无NUL边界已在原声明战斗脚本中修正，聚焦测试增非法反例；原项目离线runtime+Editor编译0错、diff check/Ledger通过。原Editor仍缺本轮NUnit和正式同tick/图文画面证据，Q09/R17未关。两个其他Unity项目窗口是预存进程，不能算本目标验收；仅用原仓库PID33236，禁computer-use/第二项目。
> 2026-09-23 原项目会话更正：当前无运行中的原项目 Unity Editor；下方 PID33236/worker 存活是昨日快照。`I:\UnityPreject\test` 的 Editor 是独立项目，不用于本对齐，也不在其运行时另启原项目。Q09 图标当前修订 `CODE_WRITTEN / OFFLINE_COMPILE_PASS / ORIGINAL_EDITOR_RECOMPILE_PENDING`，聚焦测试尚未运行。Q10 击倒提示音频正式 tick 后追加事件与 Unity 缺生产入口的静态首差已归档到 `artifacts/diagnostics/NTSD28-Q10-KNOCKOUT-AUDIO-HANDOFF-AUDIT-001/REPORT.md`；先做正式事件见证和独立 Task/Change，再实施，勿重做 Q06 frame-sound。总目标开放。
> 2026-09-23 Q07续接：原项目Editor PID173216内，用户确认并写入Menu→Battle Build Settings两条启用Scene。原Editor刷新后Menu真实组件回调→Additive BattleRunning→Unload回Menu聚焦PASS，Naruto OID2/正式三owner/Stopped/池借用0，Scene SHA未变；首次旧缓存预检FAIL留证。Record `NTSD28-Q07-PRODUCTION-SCENE-LIST-001 / FOCUSED_TEST_PASS`。下一仍是Q07 DAT/图片动态闭合及Player冷启动/菜单完整验证，Q07和总目标开放；独立`I:\UnityPreject\test`不用于本任务。

> 2026-09-23 Q07接续游标：正式`data/kind.dat`同SHA暂存后，`NTSD28-Q07-KIND-DAT-READER-AND-IDENTITY-CONTRACT-AUDIT-001`已只读闭合解析、缺失fallback/畸形拒绝、候选type3/kind9顺序、变身frame和Unity两个consumer/联合身份/R15依赖。当前生产仍用硬编码；正式330对象无OID209，自然场景差异待证。下一建立准确parser/identity Task与Change，再实现与聚焦验证；旧固定值暂保留，Q07/总目标不闭。

> 2026-09-23 当前执行包`NTSD28-Q07-KIND-DAT-PARSER-SELECTION-001 / IN_PROGRESS`：已先建独立Task/Change并入Ledger/STATE，允许仅新增kind模型、原生解析器、选源封存和定向Editor测试四脚本及meta。联合身份与战斗consumer待下一包，不得把parser基础报成Q07完成。

> 2026-09-23 Q07 parser包`NTSD28-Q07-KIND-DAT-PARSER-SELECTION-001 / FOCUSED_TEST_PASS`：原项目Editor脚本编译，首轮聚焦13/13、整数边界补证后最终24/24PASS，结果`artifacts/diagnostics/NTSD28-Q07-KIND-DAT-PARSER-SELECTION-001/focused-test-job-rerun.json`；正式kind文件和fallback语义相等、输入身份区分，四GUID唯一、Scene未改。下一是联合内容身份/prepared World发布并处理R15版本门槛，再接两combat consumer；硬编码仍在，Q07未闭。
> 2026-09-23 Q07当前接续：`NTSD28-Q07-KIND-DAT-IDENTITY-PREPARED-WORLD-001 / PLANNED`的Task/Change/改前审计已建，下一步按七组件V3接Logan目录、候选新鲜度、World封存及trace版本门槛。已验证parser 24/24，战斗consumer尚未接。原Editor/原项目；禁computer-use与第二Unity项目；Q07/R15未闭。
> 2026-09-23 Q07当前接续：`NTSD28-Q07-KIND-DAT-IDENTITY-PREPARED-WORLD-001 / FOCUSED_TEST_PASS`已在原项目Editor编译，定向3/3、相邻16/16、Q06向量17/17、parity162/162；正式kind selected/fallback按V3或V3_KIND_ONLY进入共同内容身份、候选新鲜度和prepared World，旧V1/V2仍为历史。双Scene SHA不变，Ledger通过。尚无V3真实Battle Scene/Player或native V3同seed trace；`BruteForceSceneQuery`/`BattleDamageWriter`字面值还未退休。下一精准Task是kind候选/变身consumer，随后R15正式双端版本化捕获；Q07/总目标开放，禁computer-use/第二Unity项目。
> 2026-09-23 Q07 V3 Scene补证：原Editor保存Battle Scene发起Menu真实回调，`artifacts/diagnostics/NTSD28-Q07-MENU-SCENE-CALLBACK-PLAY-001/q07-kind-v3-menu-smoke.json` PASS；正式V3 key、Naruto OID2、Additive BattleRunning、三发布键一致、退出回Menu Stopped/借用0，双Scene SHA保持。只证明V3没有打断现有场景发布链；kind实际候选/变身consumer、Player及native同seed trace仍待。Q07/R15开放。
> 2026-09-23 Q07接续脚本包：`NTSD28-Q07-KIND-DAT-COMBAT-CONSUMERS-001 / PLANNED`已先建Task/Change。接World已封存kind表到BruteForce候选及BattleDamageWriter/ECS type3变身，先做受控改表RED，后原Editor聚焦/Scene验收；现有硬编码未动。详Task和Q07 KIND-DAT-READER-AUDIT，Q07/R15开放；禁computer-use/第二项目。
> 2026-09-23 Q07当前游标：`NTSD28-Q07-KIND-DAT-COMBAT-CONSUMERS-001 / FOCUSED_TEST_PASS`。正式kind表已由prepared World或精确锁定fallback进入候选门和type3变身/ECS投影；原项目Editor编译，聚焦B5 10/10、ECS ShadowCompare 2/2 PASS。缺帧999经有效RED后已按正式源码保留原始动作写入。默认正式catalog无OID209，受控kind依赖真实Scene、Player冷启动及R15 native V3同seed trace未验；Q07/总目标仍开放。下一先做R15 native V3源模型与双端trace，再做kind依赖Scene证据；详Change Record及对齐总表0.11游标。禁computer-use/第二Unity项目；用户既有dirty与保存Scene保持。
> 2026-09-23 R15下一包 `NTSD28-R15-NATIVE-KIND-V3-CAPTURE-001 / PLANNED`：已建独立Task/Change，准确只改仓库诊断runner；正式host选择kind、Unity V3身份及当前V2-only首差已核。下一先做runner改动和正式内容新捕获，再补Unity同seed双端trace；Q07/R15/总目标开放，禁computer-use/第二项目，保留dirty与历史捕获。
> 2026-09-23 R15新状态 NTSD28-R15-NATIVE-KIND-V3-CAPTURE-001 / FOCUSED_TEST_PASS：正式源码模型新V3双跑逐字节一致、三路source validator通过；原Editor同seed3tick raw300/300、B2联合流相等。B0 Unity tick多aiAcceptedTrace而严格validator报tick-properties，domain未比较。正式kind依赖Scene、Player冷启动、更多场景和正式EXE行为仍待，Q07/R15/总目标开放。下一先处理B0契约/producer归属，再做kind依赖真实战斗；详artifact REPORT及对齐总表0.11游标。原Scene SHA不变、禁computer-use/第二项目。
> 2026-09-23 R15 B0下一包 NTSD28-R15-B0-UNITY-AI-DIAGNOSTIC-EXTENSION-001 / PLANNED：当前正式/Unity同seed B0比较首阻为Unity专属aiAcceptedTrace额外字段；已建准确工具两文件Task/Change，正式共享六字段和旧v1保持，先聚焦RED再严格校验扩展并复比。Q07/R15/总目标开放，禁computer-use/第二项目。
> 2026-09-23 R15 B0工具包 NTSD28-R15-B0-UNITY-AI-DIAGNOSTIC-EXTENSION-001 / FOCUSED_TEST_PASS：只对Unity生产者可选aiAcceptedTrace做严格五字段校验，正式B0共享六字段及旧捕获不变；RED15例1失败后最终16/16。原同seed3tick B0共享input/slots/lifecycle相等、RNG source/Unity流拓扑不同；不能宣称RNG等价。Q07/R15/总目标开放，下一Q07 kind依赖真实运行及更广trace；详REPORT/Record和总表0.11游标。禁computer-use/第二项目。
> 2026-09-23 Q07定向包 `NTSD28-Q07-KIND-DAT-LIVE-SCENE-TICK-001 / VERIFIED`：原Editor编译后，保存Battle Scene正式selected kind表(effect209/frame40)与临时type3 OID213→206走生产driver完整tick5→6，目标OID213/action40/team4/owner7/latch/定义转移PASS。注销后World4/池借用2不增，Editor返回EditMode、双Scene SHA保持。仅新Editor诊断脚本，生产/Scene/非战斗未改；自然技能/Player冷启动/正式EXE像素仍待，Q07/总目标开放。详Task/Record/result.json。
> 2026-09-23 R15 standing-attack V3双端补证：`NTSD28-R15-STANDING-ATTACK-V3-TRACE-001 / VERIFIED_SCOPED_TRACE`。正式源码模型主/B0/B2各双跑逐字节一致；原Editor正式资源同seed3tick，J→动作65及同步RNG call-site0x82/上界2/结果1，raw300/300零差、B2无首差、B0共享input/slots/lifecycle相等但流拓扑不同。全部为certificate=false的source-model诊断；Unity main曾误送authority-only validator而报header属性错，正确跨生产者comparator与Unity B0/B2验证通过，误用结果保留并在REPORT说明。代码/Scene/资源未改；下一Q07 Player冷启动及R15 kind依赖trace，Q07/总目标开放。
> 2026-09-23 Q07 Player前置 `NTSD28-Q07-WINDOWS-PLAYER-MANIFEST-V2-001 / COMPILE_PASS`：改前核现暂存1371文件/46883057字节全对正式SHA，旧Windows后处理器冻结1343/46594829。准确Task/Change已先建，只写新的冻结V2清单和该Editor postbuild处理器常量，旧清单保留；后续Menu-first Player冷启动须另包。原Editor/Scene/非战斗保持。
> 2026-09-23 R15 B0 v2诊断契约包 `NTSD28-R15-B0-OBJECT-ID-CHANGE-CONTRACT-001 / IN_PROGRESS` 已在脚本修改前建立Task/Change/Ledger。四个Tools诊断文件有明确范围；保留v1旧捕获和拒绝行为，v2新增同epoch对象ID变化事件并拒绝混版比较。尚未实施/验证，producer、Unity kind场景与正式EXE可见对照仍待；Q07/R15/总目标开放。
> 2026-09-23 R15源码模型v2采集包 `NTSD28-R15-B0-SOURCE-V2-PRODUCER-001 / IN_PROGRESS` 已在脚本修改前建立Task/Change/Ledger，仅声明诊断runner一文件；默认v1保持，显式v2将记录同epoch对象ID变化。待编译/双跑/校验，Unity producer与同场景对照仍未做；Q07/R15/总目标开放。
> 2026-09-23 R15 Unity B0 v2采集包 `NTSD28-R15-B0-UNITY-V2-PRODUCER-001 / IN_PROGRESS` 已在脚本修改前建立Task/Change/Ledger，仅声明Editor采集与聚焦测试两文件；默认v1保留，显式v2严格记录同epoch ID变化。type3/action场景和跨端对照仍是独立后继；Q07/R15/总目标开放。
> 2026-09-23 R15 `NTSD28-R15-TYPE3-ACTION-UNITY-SCENARIO-001 / IN_PROGRESS`：已在脚本修改前建立Task/Change/Ledger，精确只声明Unity Editor raw capture及其聚焦测试两脚本。正式场景OID213/action176与OID206/action0均为type3；当前捕获器强制LF2Character且忽略action，是同seed kind消费trace的初态阻断。下一实施真实type3诊断shell与action初始化，然后原Editor聚焦测试及main/B2/B0首差对照；Q07/R15/总目标开放。
> 2026-09-23 R15 `NTSD28-R15-TYPE3-ACTION-UNITY-SCENARIO-001 / FOCUSED_TEST_PASS / FIRST_DIFFERENCE_FOUND`：原Editor诊断捕获编译、定向3/3，正式源码模型及Unity同场景main/B0v2/B2三tick严格有效。tick1正式仅slot1 OID213/action40，Unity额外slot50 OID206/action40；B0独有birth/B2实体数1对2。下一个生产候选是non-character hit_Fa7错误克隆及运动/顺序全链，须独立Task/Change与原Editor受影响分支验收；正式EXE行为未证，Q07/R15/总目标开放。
> 2026-09-23 Q07下一准确生产包 `NTSD28-Q07-NONCHARACTER-HITFA7-PRODUCTION-001 / PLANNED`：已按R15首差先建Task/Change/Ledger，只声明LF2Entity与新聚焦测试。正式non-character behavior7缺预指定目标时不克隆，目标存在时完整运动/动作顺序待RED和实现；当前生产脚本尚未在此包修改。Q07/R15/总目标开放。
> 2026-09-23 Q07 `NTSD28-Q07-NONCHARACTER-HITFA7-PRODUCTION-001 / FOCUSED_TEST_PASS`：原Editor聚焦RED2/2→最终GREEN2/2、相关非角色/AI回归2/2。独立review发现的float常量和过早整数Y同步已修，角色路径未改。冻结kind/type3同seed三tickmain150字段0差、B2相等、B0共享域相等且RNG拓扑仍不同；有目标完整tick previous-Y及正式EXE/Player可见对照待。双Scene哈希保持，Q07/R15/总目标开放；详同ID REPORT/Record。下一可做有目标完整tick源模型/Unity见证，再处理Q07其余内容与表现出口。
> 2026-09-23 Q07 `NTSD28-Q07-HITFA7-TARGET-FULLTICK-WITNESS-001 / PLANNED`：正式OID875/action55的dvy1在AI与physics之间，当前局部AI→physics测试Y=-21.6不是真实完整tick预期。脚本前Task/Change/Ledger已建，仅声明仓库诊断runner opt-in与既有Q07 Editor测试；下一用正式源码模型/Unity真实driver同初值捕获目标存在完整tick previous-Y和首差，不改正式源码/生产/Scene。Q07/R15/总目标开放。
> 2026-09-23 Q07 `NTSD28-Q07-HITFA7-TARGET-FULLTICK-WITNESS-001 / CODE_WRITTEN`：脚本前改用新单用途runner，既有R15脏runner未改。正式源码模型Stage23合法三tick已编译捕获：tick1 action55/Vy5.2/YInt-20/previousY-30，tick2 action60/previousY-20，tick3 action61/实体3。原Editor首测因初版2tick/Z100/120违反诊断入口合同而在战斗前FAIL，夹具已同步修为3tick/Z600/620；Unity当前编译/重跑待Editor退出别的Play切换。Q07/R15/总目标开放。
> 2026-09-23 Q07最新首差 `NTSD28-Q07-HITFA7-TARGET-FULLTICK-WITNESS-001 / CODE_WRITTEN / FIRST_DIFFERENCE_FOUND`：正式源码模型OID875/action55目标槽0、初始Vy3.8完整tick1 Vy5.2/YInt-20；原项目Unity真实Driver同初态Vy6.2/YInt-19。静态链确认原生FrameMotion后，type3 NativePhysics内旧非角色帧推进再次应用dvy1，生产未改；tick2 Y/previous-Y为后继差。聚焦断言已改数值比较尚待新运行。错误桥接过滤参数导致意外全量EditMode job `807dbc7e717a4928bb4be19941856920`仍运行且有无关失败，勿并发；终态后仅用`testNames`跑目标。Q07/R15/总目标开放，Q10成果未撤销；详本包REPORT/Record。
> 2026-09-23 Q07待执行生产包 `NTSD28-Q07-NONCHARACTER-DVY-SINGLE-APPLICATION-001 / PLANNED`：生产脚本前Task/Change/Ledger已建，准确仅声明SimulationWorld原生物理pass抑制重复旧帧速度。先让当前全量job终止，再用`testNames`复跑Q07数值RED；生产尚未改。正式EXE可见/Player、tick3 OPoint前置仍独立待，Q07/R15/总目标开放。
> 2026-09-23 Q07当前 `NTSD28-Q07-NONCHARACTER-DVY-SINGLE-APPLICATION-001 / FOCUSED_TEST_PASS`：原Editor PID173216退出使误发全量job最后7706/8259后无终态；新原项目Editor PID103064已精确Q07数值RED 0/1→生产修复后GREEN 1/1、相邻5/5。正式paired源码重建、同冻结kind/type3三tick原Editor主raw150字段0差/B2相等/B0共享域相等但RNG拓扑不同；双Scene SHA保持。生产仅World原生物理pass抑制重复dvy，诊断测试仅binary64容差。下一Q07 tick3 OPoint诊断池、正式EXE可见/Player与更广真实战斗；Q07/R15/总目标开放。详本包REPORT/Record。
> 2026-09-23 Q07下一诊断 `NTSD28-Q07-HITFA7-TICK3-OPOINT-WITNESS-001 / PLANNED`：Task/Change/Ledger/STATE已在测试脚本修改前登记，只在既有Q07 Editor测试增精确第3 tick见证和Temp异常记录。源模型tick3 action61/实体3；先判明Unity OPoint任务World归属及完整异常，旧pool.Get异常不能直接判为Renderer夹具问题。生产/Scene/资源不在本包，Q07/R15及总目标开放。
> 2026-09-23 Q07第3 tick `NTSD28-Q07-HITFA7-TICK3-OPOINT-WITNESS-001 / FOCUSED_TEST_PASS`：原Editor目标1/1、相邻1/1；同冻结Stage23源模型和Unity真实Driver tick3 action61/YInt-15/previousY-15/实体3及所比字段相等，关闭ObjectPoolQuiesced且World/slots/borrowers0。旧异常来自未封存时Driver清除logic-only及EditMode池未Awake；仅测试夹具改动，Scene SHA保持，Console error0。正式EXE可见/Player、其余OPoint及Q07/R15/总目标未闭，Q10局部成果不撤销。详本包REPORT/Record。
> 2026-09-23 用户鸣人落地奔跑视觉距离优先：`NTSD28-USER-CAMERA-VIEWPORT-SCALE-001 / IN_PROGRESS`。用户确认按正式视口比例修正当前全背景战斗相机；只改Battle背景表现脚本及聚焦测试，固定中心长期跑出视野风险必须评估，不能改逻辑速度或声称镜头完整对齐。任务合同、Change Record、Ledger、STATE已在脚本前登记；原Editor验证待。螺旋丸续按攻击完整自然物理输入仍待；Q07及总目标开放。
> 2026-09-23 更正优先：用户明确保留当前战斗相机尺寸，要求按比例调整角色奔跑距离。此前`NTSD28-USER-CAMERA-VIEWPORT-SCALE-001`误解为相机修改，已`ROLLED_BACK`；两脚本无Git diff，Battle Scene SHA不变。相机临时版21/21不算交付验证。鸣人源模型/Unity同tick29/48像素已相等，因此放大真实逻辑位移会偏离正式战斗规则；应先区分用户希望改变战斗真值还是仅改变屏幕呈现，并评估碰撞/边界/技能影响。螺旋丸完整自然按键待，Q07及总目标开放。
> 2026-09-23 D-024全实体位移比例接续：`NTSD28-USER-CORE-MOTION-OUTPUT-RATIO-001`共用X/Z积分出口和两直接位移已写；原Editor新增合成World完整core tick 1/1（job `87a7cee42fff4f4d97010c02e07a1096`）及生产物化type-1武器物理通道1/1（job `e5279a84494743079e17e45da450389c`）通过，早先聚焦结果见Change Record。一次测试桥接误用不支持的`filter`字段启动全量EditMode job `5192eb9e273741398e88a56379e3e9ff`，不能算目标验收；确认终态前不并发发测试。Y、OPoint、持有、传送、边界/碰撞及真实Battle Scene仍开放；DAT禁改。详Task/Change和`artifacts/diagnostics/NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001/INITIAL-INVENTORY.md`。
> 2026-09-23 D-024下一包`NTSD28-USER-OPOINT-BIRTH-RATIO-001 / PLANNED / EDITOR_JOB_PENDING`：已在修改脚本前创建Task、Change Record与Ledger，并补齐第二条实际生产路径。正式`ObjectSpawnPlanner28::plan_frame`以父位置+OPoint相对偏移生成child；Unity `BattleLogicObjectPointRuntime`和组件`LF2ObjectPointFactory`各有`ConfigureLateOpointPosition`，必须同改同测。当前仅父移动已按World比例，late出生偏移待接。只改相对X/Z、含Z+1并保持precise/int一致，Y/其他spawn、DAT及原始launch速度不改。原Editor误发全量job `5192eb9e273741398e88a56379e3e9ff`未证终态，不并发新测试或修改脚本。详Task/Change及全实体inventory。
> 2026-09-23 用户螺旋丸时机优先包 `NTSD28-USER-RASENGAN-WINDOW-PARITY-001 / IN_PROGRESS`：Task/Change/Ledger已在新诊断脚本前登记。下一原项目Editor聚焦跑正式Logan action241起始32tick，对照源码模型物理J tick25/26生效边界；生产、DAT、Scene均不改，真实Play/正式EXE可见时机待，Q07新包后置。
