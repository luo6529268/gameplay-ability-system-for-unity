# NTSD-H10-FORMAL-AUDIO-SESSION-OWNERSHIP-20261010
Status: COMPLETED_SCOPED

Human authorization: direct human discussion thread01a11fe2-bfaa-72f3-a9ed-c8260f63b193 turn01a1265b-425f-7b63-9a21-f2b5634cf553 "那就先把H10完成了啊"; exact delegated handoff retained below. Scope only four authored scripts, no assets/loader/cache/Q06. New finite task, no Goal.

Precise production symbols: NTSDSoundPlayer.PreparedSoundCue (generation/path identity/OwnedSource), owner fields and readonly diagnostics; PrepareBattleCuesAsync (serialized request invalidation/drain), GetOrPrepareCue/EnsurePreparedCueLoadedAsync/LoadPreparedClipsAsync/LoadAndPlayPreparedCueAsync (local result qualification), PrepareBattlePlaybackClips (only post-publication source retirement and failure ownership release; PCM math/helper unchanged), ReleaseBattlePlaybackClips/OnDestroy plus BeginBattleSoundShutdown/CompleteBattleSoundShutdown; PlaySfx/PlayPreparedCue (closed intake guard). SimulationTickDriver.EnterPreparingState captures exact owner; ShutdownBattleRuntime stage1 stop intake/stage5 release; SetSoundPresentationSinkForDiagnostics owner binding; CompleteBattleRuntimeShutdownAfterMapCleanup clears captured owner. AppManager.TryShutdownBattleRuntimeBeforeSceneDestroy no-driver branch releases existing soundPlayer. Test H10ResidentClosureDiagnostics adds bounded production cases only.

Ownership: each NTSDSoundPlayer owns its formal cue generation, serialized cold prepare state, pending formal load counter, and list of attached exclusive sources. Max one prepare/local formal load at a time; attached list grows only cold bounded by unique prewarmed cues, mono immediately removed after complete output publication. Existing playback list bounded by prewarmed clips. No new hot capacity growth/cache/pool. Request generation invalidates on shutdown/new prepare/controller switch; stage1 intake closed without waiting, stage5 releases all attached sources/playback after voice detachment and clears formal aliases; pending I/O remains local, is released statically if stale, next cold prepare waits old owner+pending drain. OnDestroy marks disposed and uses same idempotent stages. Borrowed global/config remain untouched. All counters owned per player and reset state only on new cold prepare.

Risk: late await resurrects old generation; nonmono ownership; fallback borrowed mistaken owned; voice retains destroyed clip; concurrent prepare overlaps; failed conversion publishes partial result. Minimum validation: compile; real production aliases/PCM/borrowed/failure/closed intake; three lifecycle cycles and cancel/destroy/controller/concurrent prepare; existing narrow ordered-shutdown/sound behavior. Only after correctness use one fixed four-WAV plus nonmono group vs isolated frozen baseline, no cost ABBA. One safely isolated minimal Play permitted for deferred Destroy if necessary. Red failures and source coverage boundaries retained. Cold allocations allowed, no Driver/whole-H10/device claim.
Rollback: exact task increments from true before/ intermediate preimages only after audited authorization, preserve user changes, no HEAD restore or autonomous rollback.

## Approved handoff
继续 H10。用户最新直接要求“那就先把H10完成了啊”，沿用本讨论会话先独立检验、准确指令直接发送的约定。本指令实施 H10 当前阶段剩余工作中的 formal source 常驻所有权与按局退休，不把它写成整项 H10/设备预算全部完成，不推进其他优化点。

建议独立 Task NTSD-H10-FORMAL-AUDIO-SESSION-OWNERSHIP-20261010 / Change NTSD-H10-FORMAL-AUDIO-SESSION-OWNERSHIP-001。旧有界 PCM 转换正常生产应用保留。其旧算法窗尖峰经讨论会话独立审查已接受为计时限制，旧原件保留，不重跑。先读 CURRENT-AUTHORITY 和本目录 REPORT.md、inventory-analysis.json、review-fingerprints.json，再按实际 dirty 创建 Task/Change/Operation/准确符号与逐文件备份，更新唯一状态。

独立新证据：实际 job8b662ac5add6460fa203666cfb0b323b 为2/2。当前生产收集330定义/1000cue/978正式有效路径及72generic有效路径。真实现有空key loader + 真实生产转换，四正式WAV各3轮逐采样float位一致，源退休/输出释放12行全部通过，全局缓存未变。每轮四个源对象合计848237B，输出6706088B，是对象计数而非RSS/整局峰值。Editor原Menu clean8、87保护稳定、validator PASSED1419。空job total0不是PASS。未解码整库。

准确可写脚本（前缀Assets/NTSD/Scripts/）仅：
1. App/NTSDSoundPlayer.cs：formal单文件加载、PreparedSoundCue准确所有权、现有PCM调用后源退休、准备generation/串行drain、关闭/OnDestroy及必要只读诊断；不改PCM数学、增益/声道/voice64/重触发/事件选择。
2. Simulation/Host/SimulationTickDriver.cs：仅已有Preparing入口捕获声音owner、现有stage1关闭准备/声音接单、stage5释放该owner；必要诊断sink对应owner绑定。不得改tick/World/pass/十一阶段顺序/GC边界。
3. App/AppManager.cs：仅确有必要时在现有准备/失败/无driver关闭路径显式绑定或清理已有soundPlayer，禁止扩展菜单或battle创建逻辑。优先现有driver Preparing捕获，先证明AppManager及BattleTestBootstrap两入口都在声音await前捕获；不要只在allocation seal后捕获。
4. Test/Editor/H10ResidentClosureDiagnostics.cs：本次真实生产/生命周期/阶段内存验收；保持前述成功诊断和原件。已有H10AudioMemoryDiagnostics、NTSD28OriginalCommonAudioEditorTests、BattleRuntimeOrderedShutdownEditorTests、NTSD28PreparingShutdownOwnerEditorTests、SoundPresentationDispatchEditorTests只选窄相关方法运行，不改。
不得修改NTSDResourceLoader、LoadTask、全局cache、CharacterAnimtorManager、Scene/资源/Settings/Server/Gen/Plugins；不读Q06方法体，不创建新manager/cache/跨局lease系统。真正缺口交回具体原因，不静默增域。

生产方案：
- 只对IsFormalBattleFile && IsSingleFile的新加载调用现有LoadSingleAudioClipAsync(string.Empty, SourcePath)。空key本来就不读写全局缓存。正式源本次独占；generic/folder仍原cache借用，AudioItem.clip fallback仍借用。绝不按IsFormalBattleFile猜某个fallback资产归本owner，不销毁旧版本已在global cache中的资源。
- 源先接local，await返回后核owner仍有效、generation/当前cue身份/接单状态，再attach并登记所有权。失效结果只静态回收本次local clip，不建playback、不入dictionary/cache、不播放；finally正确减pending。返回null/缺图/格式失败维持原错误及fallback语义，不顺手修音频内容。
- mono转换成功且BattlePlaybackClips整cue发布后，退休本次owned mono source，formal cue保留播放输出。owned非mono源本身就是输出，必须活到退休；borrowed mono配置clip只销毁生成输出，不动原资产。转换失败保留既有firstOwned整cue回滚并释放本次owned input，不错误sealed成功。
- 同一player只允许一条cold正式准备。新的prepare必须使旧generation失效并在冷阶段等待其准备/独占formal加载尾部drain后才开新generation；同controller重新进入也必须换正式generation/清旧formal alias，不能复用旧root相对路径的clip。两次准备并发、准备取消、owner销毁、controller变化都要可验证。不要在热播放首次转换/加载；既有sealed拒绝语义保持。
- Stage1写Stopping后仅关闭正式准备/事件接单、使generation失效，不等待I/O；stage5先清发布声音、停止并解绑持有本局owned输出的voice，再释放已attach的formal source及playback、清formal引用与identity。不能误停仍属于菜单的generic循环。已存在AppManager.Instance是被动getter（SingletonBehaviour不自动创建），但关闭仍优先用Preparing捕获的准确owner，禁止关闭阶段查找/创建替代对象。
- Stage5完成、旧异步尾部pending、尾部drain完成分别记录。不能因旧I/O未返回把同步driver永久留在Failed/Stopping；迟到结果必须只能回收，不再发布。下一cold准备等待旧尾部。不能把stage5完成等同所有I/O结束或Unity延迟Destroy已实际释放。
- OnDestroy用同一幂等关闭；owner已Unity null时迟到local结果仍能静态回收，不能访问已毁dictionary来复活owner。保持应用退出/失败准备路径。没有新的顶层关闭阶段。

先编译与最窄实际生产RED/GREEN，必要实际失败夹具，禁止生产故障注入系统。验证：alias一次源、mono/非mono/null/缺失与配置fallback/borrowed、输出PCM位/采样率/长度/声道、原增益/重触发/voice/声音事件，冷准备后播放不得新建cue/clip或调用loader。缺失/streaming真实API失败证据复用或最窄重验；不能用反射人为字段坏值替代真实异步失败结论。

重点覆盖三轮准备→播放→stage1→stage5→drain→重入，包含一次取消实际本地文件加载、owner在await期间自然销毁、同controller同相对路径新generation、准备并发不重叠、旧回调不覆盖新cue。每轮准确记录对象身份/voice绑定/owned/stale释放，global cache内容与borrowed引用保持。自然OnDestroy不能以手动反射调用冒充；可用测试实例runInEditMode触发的真实Unity callback并明确scope。

加载阶段验证：使用已经固定的四WAV c/jira/w/e3.wav、c/kar/w/t.wav、c/dan/w/t.wav、data/001.wav，外加一个已有真实非mono/配置借用负控（先记录身份，不改资产）。基线用冻结旧正式cache加载/转换在隔离loader/owner中，对新真实生产路线；一次有界组，串行逐cue，不同组不共驻，阶段记录source、output、temporary payload、已请求Destroy但未释放、pending local load及总和，日志/readback/校验在计时外。冷加载允许分配，不套Driver0GC。逐对象Profiler值与floatpayload分栏，不能声称是进程/RSS或完整Unity解码峰值。输出不变、steady owned源不重复、退出实际owned为0、旧新generation不重叠才通过；不重复成本ABBA/不全量978解码。

如Editor即时Destroy无法覆盖Player延迟销毁，允许原单Editor一次最小隔离Play生命周期验证，限上述小音频组，原场景必须idle非用户Play/clean，先可恢复Scene setup及单例配置；用临时空场景隔离原Menu自动行为，不保存/改Scene资产、不改domain reload/Settings、不启动整局或第二实例。确认不了隔离则准确报未测边界，不以EditMode假装Play零残留。验收关注已测对象/阶段释放，不承诺未测设备峰值。

预定成功是 H10_FORMAL_SESSION_OWNERSHIP_APPLIED：正常生产接入、无新默认OFF候选；临时PCM旧优化保留、borrowed安全、实际对象常驻减少、生命周期/声音不变量通过。阶段数据有明显代价如实报告，不重跑找PASS或扩Profiler追系统。报告原→新/测试/具体节省/未测边界，保护SHA/Scene/程序集/Validate-ChangeLedger/scoped diff后停止交讨论会话审查。

H10未关闭义务仍包括保守按局声音生产者闭包与批准的整局预算。当前visual closure明确不够，保持全已加载声音集合，不擅自按玩家裁剪、不首次播放延迟转换。讨论会话继续H10评审，不将此任务称整个H10完成，也不跳H11/旧34队列。AB/设备/GPU/IL2CPP/构建期格式仍后期；H08默认ON/M03候选OFF/H07和producer暂缓/95OFF/H06不续保持。

Pre-edit call-chain correction: BattleTestBootstrap EnsureAppManager can run after Driver.Awake; EnsureRuntimeProfileFromSources may return without EnterPreparingState. Therefore declared AppManager scope additionally includes EnsureRuntimeModules immediately after creating soundPlayer, calling new Driver.BindBattleSoundPreparationOwner (Preparing-only, reject replacing captured different owner). This proves both creation orders before audio await. Same four-script contract, no bootstrap/profile/tick changes.

## 2026-10-11 final scoped closure

H10_FORMAL_SESSION_OWNERSHIP_APPLIED. VERIFIED is scoped to the declared four-file production ownership implementation; parent H10 producer closure/budget and H07/Android remain OPEN. Actual final compile PASS; initial production1/1, lifecycle7/7, failure cleanup1/1, unique resource1/1, unique actual isolated Play1/1 PASS. Existing compatibility13/14 (one known stale mono-identity test FAIL retained; static dirty-baseline attribution only). No broad test claim. Play preflight earlier FAILED before entering; installed TestFramework empty scene preflight corrected. Actual one Play source deferred destruction, stage5 voice detachment and public cold reentry wait validated; original Menu restored clean8.

Five-file output PCM/metadata exact; distinct native clip bytes7626693 to6778456 (848237B savings) and objects9 to5, both groups owned0 after release. Not RSS, whole-session peak or CPU speedup. Single stage timings including old60.4383ms retained; no rerun. Final production diff/review, all raw test XML/JSON, fingerprints, audit and actual command logs are in artifacts/diagnostics/NTSD-H10-FORMAL-AUDIO-SESSION-OWNERSHIP-20261010/REPORT.md and linked files. Early compiler/preflight/stale-test failures retained. No runtime source changes after passing lifecycle and resource group; only Play preflight fixture changed.

Reuse: unchanged bounded PCM helper and earlier chunk/real-error evidence, current original sound suite and exact outputbits; impacted owner/stale/borrowed/closure paths freshly tested. Not executed: full producer preload/battle run, late decoder failure injection, Driver0GC/H07 formal windows, device/GPU/IL2CPP/Android. Next only discussion review of remaining audio closure/budget; no automatic follow-on task.

## Final audit result (2026-10-11)

Validate-ChangeLedger.ps1: exit0 / PASSED,1420 records,26 governed code files;4263 historical warnings,0 warnings naming this Change. Full log: artifacts/diagnostics/NTSD-H10-FORMAL-AUDIO-SESSION-OWNERSHIP-20261010/change-ledger-final.log. Final-audit-v2 exit0: protected15873 unchanged; dirty preimages14 valid; intermediate backups48 valid at audit timestamp; corrected .md.meta exact; HEAD and staged diff unchanged; scoped git diff --check exit0. Runtime3 sources/runtime assembly unchanged since unique resource group; only Play preflight fixture+Editor assembly changed. No subsequent script edits. Final documentation-only preimages and hashes recorded in closeout-manifest.json after this appendix.
