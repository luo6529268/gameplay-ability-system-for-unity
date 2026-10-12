# M12 阶段1/2/5与图片worker：实际关系及准确扩域提案

Task NTSD-M12-ANDROID-DECODE-UPLOAD-BACKPRESSURE-20261011；Change NTSD-M12-ANDROID-DECODE-UPLOAD-BACKPRESSURE-001。
状态：SHUTDOWN_SEAM_REVIEW_COMPLETE / AWAITING_EXACT_SHUTDOWN_SCOPE_DECISION。用户已授权按方案继续M12实施及有限验证，同时要求需要扩大关闭写域/改变合同先报告。本次命中该条件，因此尚未改生产/测试脚本或运行Unity。

## 结论及证据

| 位置 | 已观察代码事实 | 对M12的含义 |
|---|---|---|
| Simulation/Host/SimulationTickDriver.cs:1666–1680 | 写Stopping；调用CancelConfiguredContentPrewarm、CancelNativeContentPrewarm；关闭tick/input | 会使图片generation失效，但没有await图片任务 |
| 同文件:1683–1691；:1542 StopDedicatedSimulationWorker | 只Stop/Dispose _simulationWorker并检查simulationWorker的inflight/ack | 阶段2属于dedicated simulation worker；不持NativePrewarmOperation或body task句柄 |
| 同文件:1706–1710 | ReleaseCancelledNativeContentStaging后清central/world presentation并记PresentationCleared | 此时没有检查图片decode/CPU/upload等待/byte reservation是否drained |
| 同文件:1655–1663 | 再次调用时Stopping只返回AlreadyStopping/Failed/AwaitingRuntimeMapCleanup | 单纯加pending guard不能靠重调自然继续后半事务 |
| 同文件:1782–1808 | 最终只核stage10和map flag，随即Stopped | 图片工作完成度不在最终gate |
| Animation/Manager/CharacterAnimtorManager.cs:351–374 | Cancel只标Cancelled并递增generation；Release只销毁/清空Unity资源集合 | Cancellation不等于Join/任务完成/大数组引用释放 |
| 同manager:313–348、2768–2843、3015–3089 | worker经RunOnThreadPool再SwitchToMainThread，最后finally归还Semaphore；operation finally后才移除记录 | 线程返回、主线程continuation、operation完全收尾是不同边界；主线程同步等整个UniTask可能卡住其continuation |
| App/AppManager.cs:424–505 | TryShutdown同步检查后才DisablePresentation/map完成及UnloadSceneAsync | 加图片pending gate后App目前会报错并返回null，不会自动await/drain/resume |
| Driver:2484–2501 | UnbindWorld按Status!=Failed就尝试map完成；RecreateWorld只挡Failed后继续CreateProductionWorld | 新pending状态必须同时守住这些同文件入口，不能仅修主shutdown方法 |
| App/Editor/BattleRuntimeEditorShutdownBridge.cs:17–54 | ExitingPlayMode同步调用App/Driver，未完成直接return | 不能声称自然Editor退出具备新的异步drain保证；forced teardown需要单列测试/策略 |

本轮只读取上述关闭调用域和manager既有片段；未读活跃Q06。没有改变任何battle规则/33ms或十一阶段顺序。
AGENTS.md第6.1规定worker hard gate，完整关闭合同第16规定Stopped前no running worker；M12 PLAN还明确最终waiter/reserved/strong refs实际归零后才允许场景关闭。不能把这些要求改解释成仅generation失效。
同时保持准确：阶段2原名是dedicated worker，不把现有代码宣称已实现图片worker Join；本次是新M12 owner/任务准入必须接入原关闭门的需求，不泛称整个旧系统已发生实测销毁竞态。当前尚无本轮运行复现。

## 为什么原两生产文件不够

原定manager+BMPLoader能关闭准入、防旧generation Unity物化、最终归还lease，但Driver可以先到Stopped、App先卸Scene。若manager在阶段5同步等整个UniTask，会阻塞其SwitchToMainThread／Yield，存在死锁；仅抛出pending又会触发Driver失败/AlreadyStopping早退，现有App也不会恢复正常关闭。

因此不能用“取消返回就算收尾”“先清零计数后后台继续”“只改最终assert”通过门。需要让正常关闭owner能够异步等待并从原阶段继续，且失败/超时留Stopping。不会为了交付M12而放宽为只禁止晚发布。

## 供用户确认的准确扩域

建议额外允许下面关闭域；仍归同一M12 Task/Change，不启动独立全面关闭重构：

1. **Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs**
   - ShutdownBattleRuntime及最小私有phase continuation：阶段1取消，阶段2在原dedicated Stop/Join之后等待本次捕获的native图片operation drain，再继续3–10；重复请求复用同一关闭事务，不能重置diagnostics或重复释放。
   - CompleteBattleRuntimeShutdownAfterMapCleanup：实际native drain未完成不得Stopped。
   - UnbindWorld/RecreateWorld：pending不允许当成功或开始新World。
   - 不读/改tick、pass、输入/AI、Q06或worker计算算法。可复用已有AlreadyStopping作为pending，优先不增加/重编号enum。

2. **Assets/NTSD/Scripts/App/AppManager.cs**
   - TryShutdownBattleRuntimeBeforeSceneDestroy及对应新增异步等待入口；UnloadBattleInternal、TryReturnToCharacterSelectionFromBattleResult只在原owner确认drain/阶段10之后清map及真正Unload。
   - 保留现有public UnloadBattle(): AsyncOperation的同步兼容入口，不把“尚未开始卸载”伪装成已返回AsyncOperation；正常需要等待的路径使用新增明确异步请求。旧diagnostic直接调用者保持同步fail-closed，不批量迁移旧Player/探针或重开旧验收。
   - 必须明确返回选角请求的bool是“接受关闭请求”还是“卸载已启动”，不能隐式改变；建议新内部请求入口表达accepted/pending，原无pending路径行为相同，实际完成后再更新菜单状态。该等待行为由本次扩域确认，不作为已获批准事实。
   - OnApplicationQuit/OnSingletonDestroyed作为不可await fallback保留幂等取消/无晚发布，不能被当作正常异步关闭PASS；不hook阻止OS退出、不创建替代manager。

3. **Assets/NTSD/Docs/battle-runtime-ordered-shutdown-contract.md**
   - 只补M12图片prewarm owner在既有阶段1停止准入、阶段2完成本次drain、阶段5清staging的映射、pending/失败策略和强制退出边界；不重排/新增十一阶段，不豁免Stopped hard postconditions。
   - 新M12测试文件增加真实decode pending/byte waiting/late completion、shutdown pending重复请求、drain后继续、失败留Stopping、Recreate拒绝及准确fallback断言。其余测试域仍原PLAN。

BattleRuntimeEditorShutdownBridge.cs本次仅作为已读调用者，不加入写域。现有ExitingPlayMode回调不能保证继续PlayerLoop，必须如实记录其fallback限制，不阻塞主线程、不自动重进Play/保存Scene。正常App关闭完成与强制Editor/OS退出分开；不把强制退出包装为新的drain PASS，也不为本任务新增可取消PNG算法或线程框架。

原方案参数128MiB/单张例外439799644B和有限成本组保持；不以本次扩域提高内存预算或改变图像、规则、资源内容。Scene/资源/ProjectSettings/InputActions/Server/Gen/Plugins仍禁止写。

这不是要求批准全部App或Driver重构。上列符号以外若又发现必须改动，将再准确报告；不会“授权关闭就改所有调用者”。当前已通过符号检索确认若干旧probe消费UnloadBattle()的AsyncOperation，未读其方法体、未提议修改或重新运行这些旧专项。

## 扩域获准后的最小验证顺序

1. 独立记录新精确写域并备份当时dirty字节；先实现/测试关闭gate与真实pending小fixture，再实现原M12 byte gate，不让旧证据替代新owner收尾。
2. 原Editor新鲜安全/编译；既有正常无pending关闭应同步保持原结果。真实pending时停在stage1/2且World/presentation未释放，byte wait取消即时结束；允许纯decode完成但禁止新Unity对象；主线程continuation可前进；drain完成后只继续一次3–11。
3. 超时/失败留Stopping、禁止map清理/unload/newWorld；同owner两次关闭请求幂等；owner销毁/强制退出单独记录，不冒称正常关闭通过。诊断阻塞用小图和可控信号，不跑正式大图。
4. 上述门通过才原PLAN正确性/容量/像素/public入口及最多一次固定ABBA；不加全330、Player、Android或旧Goal。任何硬门失败保留证据，不靠重跑寻PASS。

## 本轮实际动作/未执行

只读：git status、rg具名关闭符号与调用者、Get-Content准确段；读取PLAN/authority/AGENTS关闭条款和文件操作合同。一次rg使用不存在的旧Test/BattleBootstrap.cs路径报找不到，已用rg --files定位真实App/BattleBootstrap.cs；没有读取或改写其方法体。
文件只写同Task的shutdown-review-01新证据及必要治理状态，逐文件前件/SHA/命令/保护记录见Operation与本目录manifest。
生产代码、测试代码、Unity命令、编译、测试、Profiler、构建、Android均0/NOT_RUN。H10 CLOSED、Player ABANDONED及其他hold不恢复。旧preflight方案/RESULT保留，不覆盖其当时事实。
