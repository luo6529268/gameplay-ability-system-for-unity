<!-- CHANGE-RECORD
id: NTSD-OPT-H07-RENDER-BINDING-WINDOWS-068
status: CODE_WRITTEN
code-path: Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleRenderFeature.cs
authority: effective six-stage Task sections0-8 and Batch68 Task; readonly presentation observation and opt-in existing candidate only; formal336/33ms invariants unchanged
evidence: docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH68-RENDER-BINDING-WINDOWS-20261008.md
-->
# 第68批：真实千人纹理准备复用对照

当前实施阶段03已冻结（首20有效RED已完成，仍同Change/Batch）：12当前dirty源逐SHA新backups-03；before-manifest03/backup-audit03保留Source当前BFE0CA...AD9D2与Feature67原件，旧RED公共XML先备份再允许GREEN callback覆盖。Editor98492/6402本项目Menu savedclean8roots idle，不重启；不进入Play或写stress公共请求/结果。后文tests-only是前阶段历史，非当前实施授权缺口。

准确实施符号：Suite RenderTextureBindingOutputRoot/startingRenderTextureBindingWindow/RunState/SuiteState、BeginRenderTextureBindingReuse/BeginSuite（保持17参数）、BuildCurrentRequest/BuildRenderTextureBindingRequest、StartCurrentRun/Update、ShutdownAndExit/OnPlayMode/IsOwnedTerminalOrAbsent（只补本批输出owner prefix）、Apply/Complete/RestoreRenderTextureBindingReuse、ObserveRenderTextureBindingCamera及fresh纯predicate。冷owner仅bool/previousbool/RunState与last-sequence，注册endCameraRendering一次，先按flag漂移留痕，再仅采Running+warmup后同一首个有效Game camera的新Execute，不读World/Texture/lease或日志/Json热写。正常终态、catch在StopRun前、Shutdown及外部ExitingPlayMode都解除观察/恢复；no-owner不改外部flag。OFF/ON四request只输出路径不同，使用原production+calibratedDriverGC，不增加新计时采集。

Feature精确更新时间：两只读primitive long sequence/int cameraId；Execute入口cameraId清零但sequence不归零，invalid/异常不产生新序号；只在原ExecuteCommandBuffer成功、原RecordSubmission完成、body prepare>0且actual camera非空后记录camera.GetInstanceID并递增sequence。序号饱和不回绕、cameraId0拒绝；不表示GPU完成。67helper/Foot/Health/DrawMesh/lease finally与GPU生命周期不变，无资源/buffer/热扩容。RunState新增准备/复用long合计及有效camera sample primitive，仅CPU body录制数，不冒充GPU batch；首样本允许同publication/负相机ID，零覆盖可完成测量但不得伪造收益。

阶段03 CODE_WRITTEN：上述两源接线已写，17参数旧BeginSuite保持；新显式Menu/first router/four request/current mode/冷owner/相机event/normal complete/catch-beforeStopRun/shutdown/external exit/终态owner prefix均已接入。Observer只warmup后Running且actual回调camera与Feature有效camera identity相同，首有效camera固定后其它camera拒绝；不从logic polling重复读取计数，不重新排序或重放publication。旧tests不修改；GREEN/真实四短窗仍待，不能用新增predicate测试代替实际显示收益或H11完整0GC。

## 需求、原状与本次准确范围

2026-10-08用户持续优化授权；此前状态查询轮未实施。67只有局部同纹理CPU录制收益，不能据此推广或关闭H07。沿用现有生产Brute、真实1000AI工作负载与Suite，不创建第二套runner。有效完成合同以NTSD-OPTIMIZATION-BOUNDED-AUTOGOAL-20261007.md第0—8节为准，旧次数停点不能停止目标。

本阶段先test-first，只在现有Suite同文件测试类增加具名测试；在有效RED前不写候选窗口实现或Feature观察字段。读前实际Feature只有两个Last计数，不能证明新的Execute/同一camera；RuntimeDiagnostics.SubmissionCount按draw累计，不能称presenter/GPU批数。新鲜RenderPass允许重复读取同一publication，不能要求逻辑submission推进或把新逻辑tick当显示调用证据。

- Suite允许符号：既有RunState/SuiteState、BeginSuite/BuildCurrentRequest/StartCurrentRun/ObserveRunningSample/ShutdownAndExit/OnPlayModeChanged；新增显式四短窗入口及BuildRenderTextureBindingRequest、ApplyRenderTextureBindingReuse、CompleteRenderTextureBindingReuse、RestoreRenderTextureBindingReuse、IsRenderTextureBindingObservationFresh。所有权仅冷bool/RunState，正常/失败/外部Play退出先解除相机观察并恢复原候选flag，复用既有关闭顺序；不持有World、Texture、submission/native lease。退出恢复必须幂等，无owner不改外部flag。
- Feature未来允许且本阶段不实施：LastSegmentTextureBindingExecuteSequenceForDiagnostics(long)/LastSegmentTextureBindingCameraIdForDiagnostics(int)，有效body完成录制后仅记录新的序号与实际camera identity；invalid lease/failed recording不得冒充有效新调用。具体写点在有效RED后、Feature编辑前追加冻结。不重写67 AppendSegmentDrawCommands或Foot/Health/lease，不增timer/Profiler或GPU完成主张。
- tests-only写域七现有文件逐dirty副本；Feature是本阶段保护域。若后续进入实际Editor RED/实现，先再次核Editor、指纹及公共Temp输出，再追加独立before-manifest，不能用本阶段缺失Temp或旧Editor观察当新前置。

## 冻结最小test-first矩阵：20 case

在BattleOptimizationWindowsAiSuiteRequestTests增加RenderTextureBindingWindow_前缀；反射只在冷tests调用，以缺API作为未来有效断言RED，不能制造编译错误/零用例来算RED。

| 组 | case数 | 断言 |
|---|---:|---|
| Apply/Complete OFF和ON | 2 | 默认OFF、冷owner应用、具名RunState、有效观察后完成、恢复/idempotence |
| 重入和外部非默认flag | 2 | 拒绝接管，保留现owner/外部当前值 |
| OFF/ON窗口flag漂移 | 2 | Complete拒绝；显式finally恢复仍幂等 |
| 无owner恢复false/true | 2 | 不覆盖外部flag |
| 新鲜观察纯predicate | 8 | 有效新的同camera读数；重复/过期seq、错误/无效camera、空body、缺首次prepare、负reuse拒绝 |
| 四request | 4 | 0/1分散OFF/ON、2/3混战OFF/ON；仅output不同于当前生产baseline，1000AI/120+180/seed/cadence预算原值；首case同时拒绝越界-1/4 |

纯predicate签名：bool IsRenderTextureBindingObservationFresh(long previousSequence, long currentSequence, int expectedCameraId, int actualCameraId, int prepareCount, int reuseCount)。currentSequence须严格新于已接受序号、camera匹配且非零、prepareCount>0/reuseCount>=0；Unity instance identity可以为负，不能用正值假设拒绝真实相机。有效case同时检查正/负camera identity；不要求publication或logic tick发生变化，不当作GPU fence。Feature观察属性必须primitive只读public getter，tests不对其setter反射写入。

RunState未来字段：renderTextureBindingReuseEnabled、renderTextureBindingReuseFlagApplied、renderTextureBindingReuseFlagUnchanged、renderTextureBindingReuseRestored、renderTextureBindingAcceptedCameraSamples及body prepare/reuse totals；观察有有效body不要求ON出现reuse>0，零覆盖必须真实报告而非测试伪造收益。所有测试global改值都finally恢复；沿用现有Editor单作业Suite执行，旧测试不删/不弱化。

## 生命周期、预算、不变量与回滚

冷owner只管理现有默认OFF诊断flag/相机事件注册；无新的Unity对象、buffer、cache或服务。不改变11阶段/Join：先停止诊断接单/解除观察及恢复flag，再沿旧Suite shutdown；不materialize新逻辑实体。新冷测试反射/数组/Json不能纳入热0GC主张，未来相机观察只primitive加计数、不日志/Json/反射，不更改热分配/容量seal。

保持正式336、33/3ms/max2、checksum/RNG/input/pass/slot/generation、publication只读、排序/physicalsegment/failclosed。Q06仅状态/hash，方法体不读。禁止Query/Driver/Harness/GCobserver/Benchmark/其它tests、Scene/Prefab/资源/Settings/Input/Gen/Plugins/Server写域；EXT1/Mono/ATLAS/Role-aware专项不解冻。未测预算61和H11完整scope缺口不被本包关闭。

Operation NTSD-OPTIMIZATION-BATCH68-RENDER-BINDING-WINDOWS-20261008：七当前dirty源逐SHA副本；新manifest重新基线当前SDF内容和公共Temp缺失，67 UNKNOWN_CAUSE事件及所有失败/收益原件保留。回滚只本次hunk、须另获批准并比对本批副本；不得恢复HEAD或覆盖后继用户改动。

## 验收与当前证据

PLANNED，尚无本批C#修改、Editor RED/GREEN/实景收益。启动前CIM无Editor，2026-10-08T04:56:03.8653920Z新发现一个Editor，随后核为本项目新PID98492/Unity2022.3.62f3；未由本任务启动，live compile/Scene/bridge尚待。不沿用19040或6402为新前置。

唯一次必要四短窗固定120warm+180sample；实际1000AI、原production Brute、ON/OFF同request除fresh输出路径一致。真实显示/logicP95/drop/可靠logicGC、fresh同camera body准备计数/CPU DrawMesh、末snapshot/关闭门按Task。必须记录无收益/回归，默认OFF、不重采找PASS；不启动1800刷FAIL为PASS。H07正式门与H11完整0GC仍开放，Goal active。

每次脚本交付运行Validate-ChangeLedger与scoped diff；实际执行后追加命令、结果、source/保护身份。离线静态检验不能当RED、编译、focused或runtime。

CODE_WRITTEN增量：在原Suite测试类首部增加20 RenderTextureBindingWindow_ case及三个冷反射helper；没有修改class前的Suite实现、旧测试、Feature或窗口默认。Restore合同同时覆盖正常/失败后幂等调用、无owner保留外部flag；当前尚未实现本owner，更没有进入实景。新Editor已新鲜核本项目98492/6402、Menu saved clean8roots idle、编译Console0，editor-preflight-01.json留存；新tests的实际导入/编译/发现/RED待，不把写入当测试通过。

首次导入失败保留：manage_asset精确import成功仅表示导入请求；原Editor Console实际CS0234报告本批5个NonParallelizable属性在当前NUnit接口不存在，新case discovery0而旧Suite210仍可见，不能把旧程序集或compile失败当RED。仅删除本批新增的5个不可用属性，保持20case/断言/旧代码不变，沿用既有单EditMode作业；compile-import-failure-01.json保存真实错误。不更改测试框架/包/设置、不清Console；下一新鲜编译/discovery待。

首轮静态留痕：UTC2026-10-08T05:04:44.1063074Z，Validate-ChangeLedger退出0、4314历史warnings/0errors；scoped git diff --check退出2，四治理文件新增EOF空行需按本批副本做最小修正，不能称PASS。初始Suite/Feature/HEAD保持冻结身份，尚无test作业或收益。

EOF判定更正（保留前条）：逐current dirty副本与现文件read-only比较，四治理文件的尾缀均是同样的LF+CRLF；前条“本批新增/需修正”判断不成立。这是相对HEAD既存EOF差异，不能为把全树diff变绿而处理用户已有EOF。本批增量仅使用git diff --no-index --check对七备份/当前文件验证，source-static-01.json保存每条结果；不修改尾部。首次编译失败归本批且必须修复，不能与旧EOF混淆。

修复后原Editor新domain reload/新程序集：实际discovery20/20、Menu savedclean8roots idle/nonPlay/nontests；并非旧210catalog。SuiteSHA BFE0CA745229563374A92F9046A7BC67FD6890FB23676A9B27061A81C8BAD9D2，Feature仍67SHA，原实现/旧tests文本完全未变。七before-relative git --no-index --check均exit1（no-index差异状态）、只行尾转换warning，无whitespace error；全树既存EOF不处理。discovery-and-prered-01.json与唯一20case anchored red-launch-request-01.json先保存；公共callback仍不存在，沿before-manifest02的准确callback写域。接下来有效RED，尚无测试结果。

RED_CONFIRMED：唯一job7349568f6ac540dea06456ecf06a117e terminal failed/completed20，实际fresh XML20fail/0pass/0skip、duration1.345236s；缺Apply6、fresh-observation predicate8、request builder4、restore2，都是missing API断言，不是compile failure/0case。progress total9963只是catalog，不是本批执行数。XML在UTC2026-10-08T05:08:53.1340243Z精确Copy-Item到新red-results-01.xml并核source/copySHA A3E7E08A405D21A39586771E89026DC3D61D880ACEDCB65B8650CBE4855D5AD5；根start05:07:43Z/end05:07:44Z与当前job时序吻合，20fullnames均对应本批discovery。

RED后新鲜live核98492/6402 idle/nonPlay/nontests、原Menu1savedclean8roots；未清Console、未进Play、不称历史compile错误已从Console消失。当前只tests已写、新程序集实际运行；窗口/owner/predicate与Feature观察未实现，GREEN/实景待。下一必须继续同批实际接线，不能只补API让tests绿却不进入既有Suite正常/失败/退出流程。全局阶段4/6、H07/H11OPEN、Goalactive；47执行计数只是留痕，不停止或降低门。

终态文档首patch请求被工具以multiple operations target同路径拒绝，未执行文件写入；合并每路径单Update块后重新提交。原件/脚本未回退或清理。

终核UTC2026-10-08T05:12:20.2940774Z，terminal-audit-01.json：7backup同；362条中361保护同＋1精确授权callback创建/归档SHA同，0范围外变化，HEAD同。SuiteBFE0CA...AD9D2及Feature67SHA保持。实际Validate-ChangeLedger退出0/1365Records/6governed diff/4314历史warnings/0error；本批C# git diff --check退出0，七before-relative仅no-index差异exit1/CRLFwarning无whitespace error，旧EOF保留。Editor仍live，实际Menu savedclean8roots/idle；未实施窗口/GREEN/Play/千人测量，下一继续同批实现接线，不新建批次绕过真实待办。
