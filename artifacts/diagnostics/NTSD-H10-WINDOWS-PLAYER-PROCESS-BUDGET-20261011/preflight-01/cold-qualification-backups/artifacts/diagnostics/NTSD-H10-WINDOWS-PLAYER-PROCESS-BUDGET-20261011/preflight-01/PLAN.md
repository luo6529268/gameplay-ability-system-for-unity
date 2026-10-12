# H10 Windows Player 进程预算方案 — PRE-FLIGHT-01 / PLANNED

Task：NTSD-H10-WINDOWS-PLAYER-PROCESS-BUDGET-20261011。Change：NTSD-H10-WINDOWS-PLAYER-PROCESS-BUDGET-001。

人类已批准“先把H10收尾，在推进下个优化点”及具体Windows Mono独立诊断包方向。本次派发只授权只读预检、准确方案和PLANNED登记；**以下实现/构建/运行表是供主会话独立审阅的候选额度，不是本轮执行许可**。当前状态PRE_FLIGHT_COMPLETE / NAMED_READ_REVIEW_REQUIRED，不能直接开始实施。H10父项OPEN；resume-03全部原件和诊断源码冻结，不再跑旧Editor组。

## 1. 准确写域和构建入口

只拟新增：
- Assets/NTSD/Scripts/Test/Editor/H10WindowsPlayerBudgetBuild.cs 及同名.meta。
- Assets/NTSD/Scripts/Test/H10WindowsPlayerBudgetProbe.cs 及同名.meta。
- Tools/Diagnostics/H10WindowsPrivateBytesSampler.ps1。

已核五条路径均不存在，build-01也不存在；创建前再核，不覆盖同名内容。生产/旧builder/bootstrap/probes/H10Resident/BattleScoped recorder/Scene/资源/Plugins/Server/Gen冻结。C#新文件分别属于原Editor/runtime程序集，Player probe仅DEVELOPMENT_BUILD、独立opt-in `--ntsd-h10-pc-budget-request <absolute-json>` 安装；无flag时不创建对象、不注册回调。禁止同时传旧stress flag和旧Q07探针flag，避免抢夺所有权或一轮自动Quit。

新builder拟提供唯一MenuItem，接收新根下明确request后通过EditorApplication.delayCall启动一次，CreateNew started标记防重复。只使用原Editor；先读取idle/nonPlay/no compile/update/test/全部Scene clean及原Menu8，activeBuildTarget须已是StandaloneWindows64，后台load/cache/voice安全资格须通过。不得切别的build target来补资格，不安装/修工具、不开第二Editor、不调用旧R8 builder、SaveAssets或Burst改写。一次BuildPipeline.BuildPlayer，Development、StandaloneWindows64、Scenes精确为 `Assets/NTSD/Scene/NTSD_Menu.unity`（index0）和 `Assets/NTSD/Scene/NTSD_Battle.unity`（index1）；不修改EditorBuildSettings。

输出唯一 `artifacts/diagnostics/NTSD-H10-WINDOWS-PLAYER-PROCESS-BUDGET-20261011/build-01/NTSD-H10-Budget.exe`。build-01存在即拒绝；失败保留不清理。产物SHA/BuildReport/程序集/请求/目录逐文件manifest在运行前冻结。构建期间Unity自有Library/Bee输出按实际记录，不宣称全Library不变；脚本编译前另存当前Assembly-CSharp/Editor DLL及存在的符号文件真实前件，旧resume-03 artifact不改。

## 2. 唯一暂态Settings和可归因恢复

磁盘当前Standalone backend=1（IL2CPP）、runInBackground=1、enableFrameTimingStats=0、Windows Burst EnableBurstCompilation=true。预检已保存ProjectSettings原件SHA/字节，**实施前还要重新备份当时真实前件**，本次旧前件不得覆盖后来用户修改。

builder在任何mutation前：核API GetScriptingBackend与磁盘同为IL2CPP；记录API background/frameTiming/activeTarget、全部ProjectSettings指纹、Scene与源资源指纹；落盘Operation逐文件清单及恢复意图。唯一改变为PlayerSettings.SetScriptingBackend(Standalone, Mono2x)。不改变frameTiming/runBackground/Burst/defines/剥离/IL2CPP/资源和Scene。保存Set之后实际文件SHA及精确diff，允许差异只能是Standalone backend字段。

try/finally恢复原API backend，再读取API和ProjectSettings真实字节。字节本已相同即PASS；如仍仅是已归因的单backend token差异，先核当前SHA仍等于本任务最后观测的临时版本，登记恢复Operation并使用真实前件恢复，随后SHA必须与本次build前相同。不允许对未知字段变化或并发dirty做整文件覆盖；此时保留现场、报告SETTINGS_RESTORE_UNRESOLVED，停止所有Player及后续构建。不得SaveAssets来强制绿灯。若Editor崩溃/域重载打断finally，不自动重启/盲写，依journal核现场后由主会话处置。Burst导致构建失败也不关Burst或重建第二版。

## 3. 资源部署manifest与现有缺口

`deployment-manifest-known-domains.json` 已按真实文件建立2752条source→target、SHA、bytes，合计96,956,488B，排除3146个.meta；最长目标路径209字符。**尚未复制任何资源**。

| 源 | build-01下精确目标 | 条目 |
|---|---|---:|
| Assets/NTSD/Content/LoganRuntime/catalog.csv | Assets/NTSD/Content/LoganRuntime/catalog.csv |1|
| 同根decoded_dat/* | Assets/NTSD/Content/LoganRuntime/decoded_dat/* |339|
| 同根vfs/* | Assets/NTSD/Content/LoganRuntime/vfs/* |1994|
| Assets/NTSD/Sound/*（非.meta） | NTSD-H10-Budget_Data/NTSD/Sound/* |417|
| Assets/NTSD/Sprite/UIPanels/SPARK.bmp | NTSD-H10-Budget_Data/NTSD/Sprite/UIPanels/SPARK.bmp |1|

部署前按manifest重算源SHA并核所有目标不存在、目标规范化后都在新build根，拒绝symlink/reparse/path逃逸和大小写重复；用CreateNew/不可覆盖copy，复制后逐项SHA核对。源始终只读，失败保留已有新产物。Player WorkingDirectory固定build-01，使原序列化相对formal root可解析；不改GameConfig/root/controller。程序集打包的Scene/Prefab/素材不能冒充原始File IO依赖。

**完整部署尚不能认证：**旧builder全量复制Config dat/txt及Sprite bmp，只能证明其旧做法，不能证明当前Menu实际必须的raw UI路径。已列`raw-ui-candidates-not-deployed.json`，不把候选目录全部copy。须先完成READ-SCOPE-REQUESTS中R4，补准确消费者和逐文件条目；需要范围外资源时具体列出再审，不能以失败时盲补目录过关。当前manifest不可用来声称可以直接运行。

## 4. 普通真实Menu路径（三轮，同进程）

复用旧MenuFirst probe已核回调链：Menu Start按钮→prewarm完成→VS→Naruto OID2→team确认→0电脑→背景设置→Fight→App BattleRunning。通过已具名公共selection.SetSeed设1314149188；完整最终MatchConfig、background/mode/content source与新Player输入身份保存，不能仅拿seed字段推断双RNG初始化成功。三轮实际world角色/slot/seed/profile均记录；普通组不是千人或全角色技能压力。

普通路径保持App原LocalFreeRun及原序列化设置，不用stress Manual驱动，不改AI/输入/时间/音频配置。用driver.CurrentTickIndex增量观察120warm、180/1800sample；记录真实tick进度/零推进/越界和有效时段，不把Unity帧数当tick。不声称这提供新的逐tickRNG/checksum权威对照。

每轮captured driver/sound owner/World/preparation generation必须在相关等待/关闭await前固定；App.InitializeBattleAsync自身在首await前捕获driver/generation/World，但probe仍要在自己拿到ready实例时留真实owner身份，不用重入后的singleton反查旧owner。Scene尚未加载、driver尚不存在时不得伪造owner；phase明确为owner-not-yet-available。捕获窗口与SceneLoaded订阅顺序须在同文件资格验证，不手调回调。

新probe可复用已核Q10公开播放诊断（已prepared formal020/067），精确计数/voice启动/clip/非零输出留证，标为冷诊断播放，不冒称自然玩家输入或千人dispatch。稳态不额外插入每帧声音/改变规则。实际driver suppression=false、sound准备数量/加载/失败/reject及dispatch计数分别记录；普通idle若自然dispatch为0须如实标注，不能静音或伪称自然战斗播放。

普通MenuMain分支不自动ResetAll，第二/三轮如何经真实UI恢复初始slot/step及seed须R3补证，当前不得照搬第一轮步骤三次并预称PASS。

## 5. combat1000路径（三轮，另一个进程）

候选使用真实Menu/prewarm选择产生的MatchConfig为来源，创建独立诊断副本，只将players换成非null空列表，保持背景、模式、来源、seed及其它规则字段；不得修改原config对象。具体字段复制须先读R1数据定义，不能猜字段或用有副作用的反射克隆。

App.SetMatchConfig(副本)→App.LoadBattleAdditive→真实map/config/central binding/runtime/audio准备→BattleRunning。App.SetupBattleCharacters遍历空players会跳过该创建入口；**这不证明其它bootstrap不生成对象**。在调用StartRun前必须实际World.ObjectCount=0、claimedSlots=0、objectPool active=0、referencePool active=0、map prepared、audio sealed/pending0、driver/owner一致；不符立即失败，不能清除已有角色来凑空。BattleTestBootstrap suppression和ProductionStressServicesReady在Menu-owned路径是否可用须R2确认，不能盲设true使readiness永远false，也不使用旧stress CLI host。

通过既有FromRequest/AreProductionServicesReady/StartRun，不修改runner。action combat1000、inputMode ai、entityCount1000、seed1314149188、soundPresentationMode dispatch、simulationOnly=false、skipLateRendererUpdate=false、useDedicatedSimulationWorker=false。新建request逐项显式绑定当前正常配置，禁止整份复制80历史候选JSON。当前GameConfig为DataOrientedCanonical/CentralOnly；该atomic AI profile由Config派生SoA sensing/IndexedCanonical/UnifiedAuthority。formalCollectorMode=configured，所有forceRole/direct/tree/旧候选/shadow/95候选和额外phase/detail/frame计时保持OFF；forceLegacyLateTailNoOp=true保留95 OFF；postFrameTail legacy、input/frame/preFrame data-oriented按当前request安全默认。保存每个请求字段、解析Config与实际Report resolved，发现不符不强改生产。

目前Scene序列化maxCatchUpTicksPerFrame=1/maxBacklogTicks=8，候选采用request当前1/8而非盲抄历史80的2/2；这是存储字段，不授权正常Host恢复8tick追帧。正常33ms及已有2interval Host上限保持。**harness自身强制MobileExtended1050＋LooseQuadtree、Manual/paused、enableFrameChecksum=false**（Configure5434-5456/5702-5714）；必须作为诊断负载差异单列，不能声称与普通LocalFreeRun完全同配置或据此关H07。无本轮授权修改runner消除这些差异，主审须确认这些已存诊断限制的证据用途。

TrySelectLoadedCharacter将loaded IDs排序，取首个oid>0/type0/有frames和sprites的角色；这是一种角色的1000基准实体，不是1000不同角色/全角色技能压力。实际selectedOID、参与ID集合、scene/map、真实ObjectCount/active base roster/claimed slots/派生物、dispatch>0、非suppressed、实际central提交必须落盘；不能用request.entityCount单独证明1000生成。失败/容量拒绝/0GC不合格首差即止。

## 6. 两种关闭边界必须分开

Main独立补证并给出准确源码范围：TryConfigureEmptyDiagnosticWorld(2512-2540)直接CreateProductionWorld，不调用Shutdown，不能把StartRun叫重建就断言它先关闭声音；RecreateWorld(2491-2499)却会ShutdownBattleRuntime→Complete(...true)→CreateProductionWorld。CreateProductionWorld进入Preparing并重新捕获既有App声音owner。因此**runner自动CleanupInternal终态可能已经关闭本轮prepared音频**。在StartRun及等待前即捕获旧owned refs/ID/owner，不能等Active=null后才取本轮owned列表。

千人组记录：audio-prepared / runner-started / warm / sample / runner-terminal-return / owned-after-runner / App-public-close-return / Scene-unload-complete / tail-drained / owned-gone-next-frame / Menu-ready。runner终态内部RecreateWorld及其map=true只算诊断清理，不能冒称实际地图11；Active=null来自finally，不单独证明cleanup成功，必须读取真实report.harnessValidity/zeroGc/capacity/teardown/status。

随后仍调用App.TryShutdownBattleRuntimeBeforeSceneDestroy(out report)并保存真实report；它实际bootstrap.DisablePresentation/IsRuntimeMapCleared→11。仅IsComplete且真实Battle bootstrap存在时才App.UnloadBattle，并await返回Menu/scene卸载。公开方法的幂等再次调用按既有合同，不强写Stopped，不反射callback。若runner已释放owned，明确写释放发生在内部边界，不能把它归因于后面的App stage5。本轮不新增生产hook，因此不能宣称精确捕到runner内部各stage时刻；完整before/after及tail边界就是可观察范围。

每轮old owned references消失、pending/tail drained、world/slots/borrower归零才准下一轮；generic精确key/clip跨三轮同引用保留，不能把自然停播判成clip被销毁。不得为了播放断言延长/循环/重播clip。最终只回收自有probe/subscription/buffers；App/global cache由原公共流程所有，不ClearCache/GC.Collect/UnloadUnusedAssets/globalDestroy。任何未知残留保留+FAIL，不私自清理既有对象。

## 7. 候选有限执行表（待主审；本轮不执行）

明确提案为**1次构建、2次Player启动、每进程3轮：第1轮冒烟，第2/3轮正式**。不是每组3轮全正式，也不是新进程代替每轮重入。若主审希望3轮全正式，应先调整这张表，不能执行中加启动次数。

| 次序 | 进程 | 轮次 | 每轮窗口 | 后续条件 |
|---|---|---|---|---|
| Q | 原Editor | 新builder/probe窄资格（新文件内部） | source/settings/paths/flag/恢复逻辑静态及无构建资格 | 全过才允许B |
| B | 原Editor | build-01 | 唯一Windows x64 Mono Development构建，随后settings/manifest校验 | 一次失败即止，不重建 |
| P1 | ordinary | 1/2/3，同PID+CreationTime | 120+180 / 120+1800 / 120+1800 | 任一首差不开始后轮；P1全过才P2 |
| P2 | combat1000 ai | 1/2/3，同PID+CreationTime | 120+180 / 120+1800 / 120+1800 | 任一首差不开始后轮 |

warmupTicks/sampleTicks均明确，不用action=smoke（该mode把window夹到5/30且实体50）。combat action始终combat1000。config.autoStopWhenSampled=true，先由原runner终态cleanup，probe没有第二计时器强行提前终止为PASS。ordinary用真实tick增量，Sample精度与catch-up边界必须在资格中明确，不能偷偷改App驱动。普通无自然战斗输入的覆盖限制保留。

候选超时：构建45min watchdog仅记录，不kill Editor；UI冷预热480s、模式/选择入口累计120s、Battle准备180s；每轮warm/sample总600s；关闭/tail120s；每个Player总3600s。超时/预算超门/非法样本/加载失败/错误owned关系/容量或GC失败立即停止余项并走已知安全公开关闭。无法安全关闭则保存拒绝与残留，不杀不相关进程；只在明确授权的该诊断进程达到总超时且优雅Quit无响应后，是否允许终止需主审固定策略，当前不自授kill权限。无自动重试、不按更低峰挑PASS。

## 8. 外部Private Bytes采样与阶段归属

候选sampler是独立PS7进程，隐藏窗口运行。只接收自己启动Player返回的exact PID/CreationTime；Get-Process/Process.Refresh读取这些身份及PrivateMemorySize64、WorkingSet64，不输出Process全对象、CommandLine、环境或凭据。若0/负值/API错误/PID复用即invalid；不把Unity内部旧PrivateMemorySize64=0当有效数据。

100ms目标节拍：Stopwatch.GetTimestamp绝对QPC/Frequency、UTC(+08:00展示)、目标deadline/实际间隔/错过槽位记录。两内存列不相加。采样器与Player phase记录均带raw QPC/Frequency以对齐；只在确认同clock frequency和身份后归属到window，不能只按不同时钟elapsedMs拼接。样本最迟边界和不确定跨界样本同时归入更保守bucket；所有样本（含冷启动/准备/卸载）保留。峰值只叫100ms采样峰，无法证明采样间没有更高瞬时峰。

newProbe固定phase缓冲（每进程≤256条、3轮），cold metadata/owned refs容量≤2048/轮、保留3轮≤6144，超过fail-closed；不在热scope做JSON、反射、资源遍历或逐帧分配。phase跨warm/steady的写出方法须新资格证明不会污染既有0GC门；如果无法取得精确steady边界，保守把整个runner active/terminal interval作为steady2GiB域，而不是挑低样本或假装精确。不得改旧recorder/移除requireZeroGc。OS采样JSON分配属于外部sampler，不计入PlayerPrivate；probe自身成本属于Player且不扣除。

采样质量候选：所有属于验收window的private值非零且身份一致，记录实际有效数/expected slots/间隔分布和最大gap；任何不可归属或明显中断窗口不给预算PASS。具体最大gap硬阈值需主审锁定后写请求，预检未自行宣称100ms已达到。

## 9. 预算、场景及证据边界

320MiB=335544320B；steady Private≤2147483648B；transition sampled peak≤3221225472B。逐实例去重source/output/borrowed，13个nonmono不双算；decode/temp/voice与不可观测项单列。旧同输入218.313758MiB可复用，但新Player必须逐项验证部署source SHA、当前published identity和实际prepared清单一致；只复制完整目录不等于该局实际准备全catalog。

Private稳态/过渡分列WorkingSet，不按回收后/退出后数值替代在场峰值；检查三輪old owned归零、same generic复用、没有unknown owner累计。memory monotonic上升只有采样事实，不自行归因为leak或手动清缓存。真实driver输入/双RNG/checksum/pass/33ms不改；本次不能提供新的完整determinism证书。H07正常33ms/no持续backlog/dropped0及Android最终门仍原样OPEN，不因为H10运行而降门，也不把Manual千人结果算正常Host通过。

当前Development/dev PC，不是未知GPU型号/8GB基础机认证。分辨率候选1280×720 windowed，仅记录实际分辨率/AudioSettings/硬件/电源/背景负载，不改ProjectSettings/FPS/Burst。不使用batchmode/nographics/noaudio隐藏实际声画。IL2CPP/GPU/Android/AB/H07后置。

## 10. 必须先补的具名只读依赖

详READ-SCOPE-REQUESTS.md。R1 MatchConfig/PlayerSlotConfig字段身份；R2 BattleTestBootstrap.Start的Menu-owned/suppression/readiness分支及真实map owner最小段；R3 CharacterSelectionController.OnEnable/ResetAll/StartBattle、LoadingPrewarmController重入；R4当前raw UI/File IO consumer链。Driver重建/关闭区别已有主会话具名证据，不再要求泛读Driver/Q06。没有任何生产修复依据或授权；若这些读证后仍需生产变更，必须先报告准确缺口。

## 11. 所有权、十一阶段与回滚

Builder仅Editor一次回调状态，finally解绑delayCall/释放自身引用并恢复临时backend；无battle manager，不参与runtime11阶段。Probe独占自己的DontDestroyOnLoad host、256phase缓冲、bounded快照/文件句柄及SceneLoaded订阅；每轮stage1停止新诊断请求，stage2不创建worker，stage3不发OPoint，stage4沿公开owner，stage5仅观察原声音owner；stage6-10不手动回收原生产资源；stage11由App真实map证明。关闭后tail/后帧再核旧refs；最终退出时解绑自有订阅/关闭自有输出，不能在OnDestroy访问会创建服务的singleton。外部sampler只停止自己采样/释放精确Process句柄，不接管Unity对象生命周期。

回滚仅尚未创建的新文件/本Task增量及真实可归因Settings字节；任何删除/移除新文件也要新Operation与授权，不自动回退或cleanup。原10dirty/全部untracked/旧事故与失败保留。所有新报告CreateNew，运行侧runner自己的mutable报告/Temp result要显式标成runner自然覆盖域：每轮开始前备份build-01/Temp/NTSD_ProductionEntityStress.result准确前件，结束后再snapshot到run/cycle目录；项目原Temp绝不触及。每轮config.outputPath唯一，harness内部对同轮报告的多次写不伪称CreateNew原件；其terminal后才归档冻结，快照时间/哈希明记。失败版本不覆盖。

最终只有实际编译/窄资格/build/manifest/两进程有限组/Settings恢复/资源hash/ChangeLedger全部具名证据支持，才形成对应限定Player预算结论；H10父项是否可关闭仍由主审对完整覆盖决定。本轮交付止于此预检方案。

## 12. H10父项覆盖补记：构建前必须锁定CASE-MATRIX

详[H10-COVERAGE.md](H10-COVERAGE.md)。当前候选2进程各3轮表仍只有普通Naruto和单OID Manual1000，**不覆盖多角色/技能最坏集合或全部生产声音producer动态闭包**。旧全目录账本、五PCM代表及63scope证据仅按原范围复用；compat13/14与其它失败保持。

本Task不能在运行两个进程后默认停在无计划的父项缺口：在实现/构建许可前，主审应基于R1/R3真实UI数据入口及既有cue/技能证据，锁定精确角色/技能输入/事件期望CASE-MATRIX，并决定是否可配置普通轮2/3承担该矩阵，或同Task另行准确批准有限额外场景。当前没有第三进程额度、没有未读producer/新生产修改授权。采样质量及超时策略一并锁定；范围变化写入同一Task，不为每个诊断拆包。最终只按覆盖表逐门裁决，不以Android/8GB后置替代当前PC缺口。


## PRE-FLIGHT-01 addendum-01（当前补充，原文保留）

状态仍PLANNED，未执行代码/Settings/编译/测试/build/Player/sidecar。R1-R3已由主会话具名供证，精确采取方案及来源见 `artifacts/diagnostics/NTSD-H10-WINDOWS-PLAYER-PROCESS-BUDGET-20261011/preflight-01/PREFLIGHT-ADDENDUM-01.md`。新的具体用例为2/16/507三角色三轮，见同目录`CASE-MATRIX-CANDIDATE.md`；四cue当前SHA与旧inventory相等，见`case-evidence-inputs.json`。并非所有producer/角色技能动态穷举；原“全部动态覆盖”措辞按本补充纠正，不降低全catalog静态＋实际运行请求覆盖门。

仍需独立审查：`NEW-READ-DEPENDENCIES-01.md`的R4A/B、R5A/B，`OBSERVATION-SEAM-REQUEST.md`的只读事件转发入口；`EMPTY-WORLD-ENTRY-ORDER.md`说明真实MatchConfirmed后仍自动Set/Load原cfg，因此原空players回调方案不闭合，不能直接实现。不得以反射写原cfg、清场或重复Load补救。候选跨进程模板/实际UI模式引用方案及100ms/250ms/99%采样质量/无自动kill处置均尚待主审冻结。

保存的原校验：ChangeLedger exit0/PASSED1423 records/4534 warnings，3条属于本PLANNED未创建脚本，不称本Change0warning；protection-audit的4094保护/406旧H10/18源程序集Settings无漂移。最终补充审计另存，不覆盖原件。没有恢复旧目标或跳其它优化；H10父项保持OPEN且继续同Task。


## PRE-FLIGHT-01 收敛表（2026-10-11；当前单一接续点）

**PLANNED / READ_ONLY_PREPARATION_CONTINUES。** 以下整合已有方案及主审本次供证，覆盖上文旧待定/同队/seed/identity/alias/样本措辞的冲突；旧文保留历史。本段不授任何实现、第四processor写域、编译、build、Player、sampler或追加运行额度。H10父项OPEN，无其它点/Goal/队列恢复。主审已冻结静态模式来源和review03的普通/千人证据颗粒度；不是运行PASS。唯一剩余准确读请求是FINAL-PATH-BINDING-REQUEST.md，不再泛读UI/legacy/producer找copy理由。

### 当前路径证据和真实部署差量

主会话独立具名读取Candidate SHA35ac697cb8b5a43bc9ae76413f59dd3498218b6c188a06a02e00e99b01d99f9f的ImageInput/Words/Spark/KillIcon/Capture321–361/Copy407–412/GetHash414–420：body/head/small是实际配置fullpath的Ordinal集合，只有body且非requiredUI可带allowMissing。未读missing-body hash helper，不能发明缺失处理。Manager native UI只能以Candidate已捕获的精确fullpath查询SHA；不另猜raw根。未读FirstNativeResourceTable/ReadDimension/parser/feed producer/atlas/legacy正文。

| 输入域 | 本轮真实已知与冻结manifest比较 | 还不能声称 |
|---|---|---|
| body已解析原件 | H08实际C#703路径/773引用，逐项全部在当前manifest，当前SHA/长度0差；原件SHA c6ab501ea8b79481ab8afc83ba7ee2a9e72fe5066a9f4b6d9b8c1e5f9afc7154 | 该原件scope排除UI，不是head/small清单或新Player候选数据 |
| resource.dat/system.dat | 1637B/4cb8d30ca477e5e8ec0aa625813ff2c7a63e1c3af561da3822c1583dfac6818f；3091B/d848cc6d711a01d12a054e2b5adb903938d7afb182854eb39ddbbf2236c31ae2；现源/manifest同 | 未调用parser/Candidate或解码图像 |
| Spark | resource首表第43项sprite\UI\SPARK.png；system scalar99×79；formal-vfs图3600B/15d8843e0ce87ff63f46dff7170d30c23baea0f2799434b26717aadfd5ec881b已包含且现SHA同 | 与独立UIPanels/SPARK.bmp不是同一输入；两配置文件只缺一个会抛异常，不能忽略 |
| 独立BMP | UIPanels/SPARK.bmp 156752B/e11c8104824be415cd66e844dbbe49b48e1e6ea5735880669e45182c34f77306已在独立目标，现SHA/长度同 | 不用BMP替换formalPNG，不扩大旧目录copy |
| Words | resource16起六项WORDS0–5.png；当前候选formalVFS路径/manifest均无；allowUnavailable:true，必要图缺失可警告并返回null | 根解析仍待准确合同；即使确认缺失也不补资源；实际Player必须记可用/跳过及告警，不能说rawUI全可用 |
| KillIcon | Candidate只收非空且存在的feed路径，缺项null；项目模式Asset44–51为c/sk2/sk2/sk1/sk2/sk2/sk2，候选formalVFS三路径均不存在/manifest无 | 尚未把Asset值到feed数组及真实root解析链冒称已核；不补图，不逆读feed producer |
| head/small | Candidate已核会收requiredUI配置fullpath；当前没有现成实际解析全集原件 | 不能从703body、目录扫描或已知2752条manifest推导完整rawUI部署闭包 |

机器原件 `resolved-input-deployment-comparison.json`（SHA71fed1e501594b4c726fda002346d3c0c809316de6df13f6bd4a81d8615390c4）和`kill-icon-input-data-check.json`。**本轮已证明必须新增的部署文件数为0；不等于已经证明没有其它缺口。** 原2752/96956488B known-domain manifest及2334formal CSV不改，仍尚未部署。下一最小根合同：BattleContentSource23–46/49–52/73–88/95–116、Manager44/50–53；KillIcon最多数组声明14/TypeResourcePath99–104。实际属性ImageRoot，不发明VfsRoot。GameConfig实际root=Assets/NTSD/Content/LoganRuntime，配置SHA25c8e08f4d1be55ee55d0404aaf4e46d099afc6c2ba3d9040e1c5203f654d9ff。当前缺少的实际已解析head/small数据保持缺口，不调用Unity或新写解析器生成。

### 完整有限执行表（待完整实施派发；本轮执行次数全0）

| 阶段 | 固定对象/入口 | 必须满足与停止条件 |
|---|---|---|
| S 安全前置 | 原项目唯一Editor、当前实际Menu与Scene清洁状态；全路径/指纹/源目录/owner库存 | 重新核idle/nonPlay/no compile/update/test、无他人操作、Scene clean；旧状态不算本次证书。activeTarget已Windows64、API/disk backend均IL2CPP，原background/frameTiming/Burst保持；任何未知并发差异停止 |
| Q 首次编译与窄资格 | 仅拟新增builder/probe/sampler，原Editor一次具名资格组；实现前记录确切测试方法/命令 | 构建上下文/保留输出/错误输入拒绝、原raw/alias/capacity、packet值历史、tee全载荷exactly-once及所有错误分支、有效GC正控/empty/tee、phase/采样/恢复门。全部通过才B；首次失败回审，不重复同版资格找PASS。旧63scope及其它focused不能替代新tee |
| B 唯一build | build-01/NTSD-H10-Budget.exe，Mono Development Windows64，两Scene固定Menu/Battle | 精确一次BuildPipeline；旧callback全部严格门保留，仅获准后精确H10一次上下文选2334清单。非H10原1373合同保持；保留H10路径上下文缺失/已消费/丢失即拒绝，不fallback。finally恢复backend且实字节核验后，才可copy generic417到独立域；完整部署/产物SHA冻结。失败不重build/删除产物 |
| P1-O1 | 普通进程轮1，P1 Naruto OID2，另16/507中性 | seed682973786；三个公开槽team1/2/3、Confirmed/inputId、0电脑、真实VS0/1、原LocalFreeRun；120warm+180smoke；一次L→D→K→neutral，自然053持续代表；门全过并真实关闭/零残留才O2 |
| P1-O2 | 同PID轮2，P1 Gaara OID16，另2/507中性 | 相同配置/入口资格；120warm+1800formal；一次L→D→J→neutral，自然gaa/j4→043多cue代表；全过并关闭后才O3 |
| P1-O3 | 同PID轮3，P1 Kimimaro OID507，另2/16中性 | 120warm+1800formal；一次L→W→K→neutral，kim/j1 stereo代表；全组三轮通过、源/config模板完整冻结和进程收尾后才P2 |
| P2-C1 | 第二进程轮1：真实Menu/prewarm/VS读取本轮mode，P1成功模板独立clone仅players改非null空；App.Set/Load，不点Fight | suppression实际false且不写；地图prepared/id/真实boundary、World/slot/两池空、audio owner准备后原AreReady/StartRun；harness seed1314149188、combat1000 AI、最低有效type0单OID；120warm+180smoke；首差停止 |
| P2-C2/C3 | 同第二PID轮2/3，重复真实公开关闭重入 | 分别120warm+1800formal；不新开进程代替重入。原harness MobileExtended1050/LooseQuadtree/Manual/paused/checksumfalse、maxCatchUp/backlog请求1/8如实记录；normal33ms/2interval合同不改，不能称普通Host/H07通过 |
| E 结束核对 | 两进程各三轮全部原件及失败证据/Scene与源/Settings/ChangeLedger | 按scope裁决限定Windows预算；不自行关闭H10父项、追加组/进程/构建、推进其它点或恢复Goal |

额度始终**1build、2Player、每PID三轮（1冒烟+2正式）**，不是三轮全正式。普通三个目标技能只投一次，下一状态最早为实际观察到经过2tick，不承诺精确两消费tick；InputSystem正常Queue/afterUpdate，无手动Update/StepOneTick/暂停/重press。记录每个可见真实packet tick及每槽四值；漏tickUNKNOWN，关键消费无法证明则INPUT_OBSERVATION_INCONCLUSIVE并停止，不以旧Manual6/13/15tick或44声作新预期。目标声需在声明窗口自然发生且有本次真实source/clip/startSequence与播放/输出证据；不足不再换一组。

### 已冻结的声音证据域和固定容量

主会话本次明确可固定review03表：普通四目标全窗事件明细4096/轮（溢出FAIL）；千人全窗遍历所有原请求，完整counters/membership/order摘要，明细只首个非空原batch前64event及同批最终最多64source启动变化。必须带tick/phase/完整batchCount/未采数/同槽中间启动不可见；不是稳态代表、千人全事件逐声认证或完整trace。全窗生产play/reject/drop/load/build等实际计数独立保留。没有可确认实际播放样本时门未过，不换batch。

raw1000含19builtin/2mode；1050是旧不同clip数；Ordinal先原raw后缺失单属slash alias1968，原raw精确身份优先，不折叠大小写/注册生产alias。实际新prepared集合与真实接受另核，alias观察命中不代替生产会员资格。1024counter、2048index、普通target/voice4096、unknown32、phase256/process、输入转换32/process、8192tick×最多8实际槽值/process、cold voices64、owned2048/round≤6144/3round全部冷构造。超过或未知key立即令证据无效，不裁生产请求/扩容。旧703/1000/1050清单均不冒称新Player实测。

观察错误/容量/未知只设固定状态，不在callback抛新增异常；完整原List同步forward一次且顺序载荷不变，生产异常原样传播；外层冷边界停后继。诊断capacity失败不叫生产丢音/关闭回归。所有新表由probe所有，保存自有值历史；不得持可变生产List/Players当快照。GC资格/计量和新路径0GC尚未执行。

### 安全恢复门（不得用例外绕过）

- 修改前另做真实Task/Change/Operation与当前dirty备份；完整派发必须明确第四processor与唯一暂态ProjectSettings backend权限。本预检旧备份不能恢复后来用户内容。所有新文件/CreateNew，build目录已存在即拒绝，失败产物保留。
- builder一次上下文绑定精确输出/nonce/requestSHA/manifestSHA/Scenes/target/options，Prepared→CallbackEntered→Completed；finally清上下文/自己的回调并恢复API backend。API与磁盘实字节核对；只有本任务已归因的单backend差异且当前SHA等于本任务临时版本才按另记Operation恢复前件，未知并发差异停止不覆盖。不改background/frameTiming/Burst/defines/EditorBuildSettings，不SaveAssets/切target/第二Editor。
- ordinary ready后warm前capture精确Driver/owner/原override；原override只允许null或同owner，未知拒绝。sample后App关闭前同一身份条件恢复。combat StartRun同步Configure后首Update前安装，提前保留旧owned refs；runner终态可能已清音频，返回时仅核同Driver/owner/当前tee再restore，然后App真实map11。无新hook重排关闭；身份变则不覆盖他人sink，不伪造restorePASS。
- stage1原owner禁止新tick/input；probe停自己的输入和订阅，stage2无worker，3不spawn，4沿原owner，5只观察audio，6–10不手动回收，11必须App真实map而非harness内部map=true。每轮World/slot/borrower/owned归零、pending/tail/后帧Destroy通过才下一轮。generic按精确key/clip跨轮同引用；不GC.Collect/UnloadUnusedAssets/ClearCache/globalDestroy。仅解绑自己的回调/移除自己新建的keyboard/释放自己的缓存与句柄。
- sampler只精确PID+CreationTime，100ms绝对QPC，read-start/end/deadline/late/UTC齐全；Private和WS分列。private非正/身份复用/API失败无效；最大gap250ms、连续域≥99%slot覆盖、首末边界距离≤250ms，否则不发预算PASS。跨界归严格bucket，无法精确划steady则全runner active/terminal按2GiB。所有样本保留，不宣称未采瞬时峰，不扣probe成本。
- AudioClip≤335544320B；Private steady≤2147483648B；transition100ms采样峰≤3221225472B。旧218.313758MiB仅同输入对象门复用，新部署SHA/actualprepared/ownership必须吻合；不把对象预算当整进程或目标PC认证。
- build watchdog45min只记录不killEditor；UI480s/选择120s/Battle准备180s/每warm+sample600s/关闭tail120s/每进程3600s。首差、超时、预算或正确性/GC失败停止余轮/第二进程，已知captured App公开安全关闭后非零Quit；无响应保留现场，不自动kill/restart/retry/清缓存。实际退出失败仍失败，不用进程消失代替十一阶段后置条件。

### 未完成和证据复用

当前只剩准确Source/path与head/small/feed输入数据绑定、部署差量裁决，以及第四写域直接人类确认和完整实施派发；后三者不能被本表自授。已冻结模式/队伍/粒度不再另生候选分支。复用resume03的限定Editor播放/关闭/0GC、旧PCM/1050对象账本及所有失败原件，因本轮无代码/资源/Settings改动；新Player/进程预算/新tee资格均未执行，旧证据不顶替。H10父项OPEN，H07正常33ms/无持续backlog/dropped0与Android最终条件原样保留。其它可做只读准备完成后回交主会话，不转下一优化。
