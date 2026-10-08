# 第68批：真实千人中央纹理准备复用率与显示收益

最终限定结论：SCOPED_WINDOWS_COMPLETED / NO_REUSE_COVERAGE / NO_CAUSAL_GAIN / NOT_ADMITTED / PERFORMANCE_FAIL。20/20 GREEN＋四actual1000AI120+180已实际完成；ON两布局各89有效camera样本、reuse0，prepare177135/177154与OFF同，CPU segment/draw平均也同。开关已实际开启且全窗不漂移，故不是把未应用误称无收益；当前helper只有相邻同texture reference与binding mode才reuse，场内覆盖为0，不猜测具体资源交替原因，不以微小计时波动称优化收益。

ON逻辑P95123.040/120.434ms、drop787/753，仍未达33ms门；显示平均254.602/249.746ms（约3.928/4.004FPS，非Stats截图同环境比较）。两组24末字段及完整checksum JSON逐SHA相同，四完整Driver有效校准steady0event；原11阶段正常关闭objects/slots/borrowers0、Battle/Menu身份与clean保持、原Menu恢复。402保护/9当前dirty副本/源冻结/HEAD保持。详[本批报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH68-RENDER-BINDING-WINDOWS-20261008/REPORT.md)。

本批不再继续cost/Driver/同四窗或1800刷PASS，不切默认、不删候选代码。H07/H11、central独立unresolved/stale与61budget/专项门均保持；下一按最新PairExactLoop53.094/55.776ms审查普通Brute逐对检查的有据减负动作，不扩到新collector或Q06排序。仍47已执行/阶段4/6、Goal active；下方所有READY/tests-only/LAUNCHED是保留历史。

最新：阶段04 WINDOWS_LAUNCHED / RUNTIME_PENDING，原Editor98492已进入Battle Play，四fresh requests已生成。原exact source/402guards与9当前dirty备份保持、validator0；尚无终态收益/逻辑达标/0GC证书。下方WINDOWS_READY是启动前留痕，不新建批次或重启同窗。

当前阶段03完成、阶段04 WINDOWS_READY：同批窗口真实接线已写，20有效RED→原Editor20/20 GREEN（job b834b93a3c444344b1bfdcd73fccab86，0skip），GREEN XML源/归档SHA 7BDC64D54862E948A93316F3F4DC0EEFFEBFD36F2EF47F87388B84CAED994AD0。此前tests-only/窗口未写的快照已被本条取代，不删除旧事实。

阶段04按before-manifest-04.json引用380现有保护及24新增保护，9当前dirty文本在backups-04逐SHA核同；两C#不再编辑、公共GREEN XML及旧RED保护。仅现有显式Batch68 Menu一次、四120+180实际1000AI/完整Driver可靠GC；输出全新windows-01下四request/report/final-checksum/logic-gc/terminal/observation、唯一suite-result和逐progress。公共stress request/result事前都缺失；不恢复旧缺失文件，既有Suite只允许将本批own结果逐窗复制到唯一terminal后再进入下一窗，所有删除/重写只限这两个精确Temp路径的本批owner结果。原Battle/Menu文件与dirty保护，禁止Save/新Editor/自动重启/Profiler/GPU capture。尚未进入Play或测量，收益/完整H11门未知。

当前增量：RED_CONFIRMED / WINDOW_IMPLEMENTATION_PENDING，唯一Change NTSD-OPT-H07-RENDER-BINDING-WINDOWS-068 / CODE_WRITTEN（tests-only）；新Editor98492/6402已实际执行20case有效RED、fresh XML归档。首次本批NonParallelizable编译失败已只移除5个不可用属性，新程序集20case发现/实际执行成功；旧Suite实现/旧tests/Feature未变。下一同批最小窗口和fresh-camera接线、GREEN后再四120+180；尚无新FPS/GC/收益，47已执行/阶段4/6/H07/H11 OPEN/Goal active。

下列READY/未开始是事前快照，不再表示当前Editor不存在；尚无本批运行时收益。

状态：READY / IMPLEMENTATION_NOT_STARTED / EDITOR_PREREQUISITE_UNAVAILABLE。67局部正确性25/25及两成本case已完成；同纹理录制均值5.2033875→3.60085ms、中位4.11735→3.25445ms，交替纹理均值约-0.93%/中位约+1.28%，不支持泛化收益。下一应检验真实资源/透明序列的复用覆盖及显示收益，而非再跑同局部cost或称H07逻辑达标。当前原Editor19040在两作业terminal之后已不在，CIM未发现任何Unity.exe，关闭原因未知；不自动启动/重启Editor。此准备不增加46已执行批数。

## 冻结范围与来源

有效六项合同0—8节/用户持续优化授权；67 [Record](../CHANGE-RECORDS/NTSD-OPT-H07-RENDER-TEXTURE-BINDING-REUSE-067.md)、[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH67-RENDER-TEXTURE-BINDING-REUSE-20261008/REPORT.md)。不修改PERF/ATLAS/EXT1/Mono的专项状态。H07上一实景65逻辑P9584.519/94.257ms/drop683/611、PairExactLoop42.118/44.256ms仍FAIL，渲染局部信号不关闭它。

本Task仅冻结下一有据最小包，不代表已实施或已获实景收益。脚本修改前另建唯一Change/Operation，逐准确current-dirty副本与保护指纹；不能复用67副本覆盖新的dirty内容。

允许准确C#仅：

1. `Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs`：复用既有Run/StartCurrentRun/ObserveRunningSample/Shutdown/OnPlayModeChanged、request与同文件Editor tests，增加唯一显式四短窗owner，持有并恢复67默认OFF静态flag。冷准备阶段注册需要的相机只读观察，完整正常/失败/外部退出路径解除观察并恢复flag；不得创建第二suite或改变11阶段关闭/清空已有World强入场。计数按实际RenderPass调用取样，不能把last缓存/逻辑tick重复读算本帧新证据。
2. `Assets/NTSD/Scripts/Animation/Rendering/BattleRenderFeature.cs`：仅当现有两个Last不足以证明调用新鲜度，最小补primitive有效Execute序号/相机身份，供同camera/本次有效body准备计数验真；事前Record冻结准确符号和更新时间。不要加计时器/Profiler采集或重写67helper/Foot/Health/lease/segment顺序。

不改Query/Driver/Harness/GC observer/Benchmark、Scene/Prefab/资源/Settings/Gen/Plugins/Input/Server或其它测试文件。Q06仍只hash/状态，不读方法体。Script前按当前代码冻结具名最小test-first矩阵（默认OFF；owner拒绝重入/非默认；flag漂移拒绝；正常/失败/外部退出幂等恢复；有效Execute序号与camera匹配；重复/过期读数拒绝；四request精确性/新输出归属），只新增和直接影响项，不重复67或全部历史。

## 唯一必要实景窗口

在同项目真实Editor确认原Scene saved/clean、非Play/测试/编译且没有已有stress owner后，现有Suite负责原Scene往返，Scene文件绝不保存/修改。无法满足时保持前置缺口，不能用孤立构建/重复显示或auto-restart冒充实景。使用现有两个生产工作负载Dispersed1000/Combat1000，同一版本/request/原生产Brute四defaults，67纹理复用OFF/ON各一次，共四120warm+180sample，输出fresh唯一68目录；组内request除outputPath相同，候选flag不写资源配置。保持实际1000activeAI、seed/输入/roster/spawn/33ms/3ms/max2/音频/worker/插值原值，复用已可靠完整logicGC校准与末snapshot/关闭门。

采集有效相机body prepare/reuse计数及其fresh Execute证明、实际body DrawMesh量、原central已提交/拒绝/解析/陈旧口径、显示帧/logic P95/drop/现有Recorder指标/末20hash与完整snapshot。Prepare+Reuse是CPU body录制数，Foot/Health另计，RuntimeDiagnostics.SubmissionCount当前按draw递增，不是presenter submission次数；不能当GPU batch或从它推SetPass。

OFF/ON保持segment/draw/材质/纹理/排序与实际publication一致；无需把GPU capture、native全项目对齐或H11旧迟发事件再跑为此Renderer只读修改的前置。现有中央指标缺口必须如实UNKNOWN，不能用source/resolved总数替代独立unresolved/stale门。完整H11 0GC与61预算UNKNOWN仍保留。

局部信号不保证真实收益。若实景复用低、无收益或回归则NOT_ADMITTED、默认OFF，不重采找PASS，转仍有据的主PairExactLoop热点。若实景收益明确且必要正确性/容量/生命周期门通过，再独立判断生产推广权限和必要证据；本Task不自动切默认。四短窗FAIL不得追加1800刷PASS；H07两正式120+1800、P95<33/drop0等阶段门仍必须真实通过后才收口。

## 当前边界

68未写代码/未跑tests/未进Play/未测量。没有Profiler、FrameDebugger、GPU capture、EXT1 M0/Instancing、bank/预算/格式/segment语义变化。67局部结果保持，六阶段4/6、H07/H11 OPEN、Goal active。原Editor不可用只限制需要Editor的后继验证，不按批数停止Goal，也不伪造完成。

终核前置更正：67最终264/266保护同，范围外字体SDF当前SHA改变及旧公共Temp结果缺失已登记UNKNOWN_CAUSE；字体当前内容保留，旧stress result及67XML都有同SHA独立原件，不自动恢复。68必须按当前dirty/保留资产重新冻结实景基线，并重新确认Editor与Scene安全；不能沿用67初始字体hash、关闭前Scene观察或消失的Temp文件作为新证据。
