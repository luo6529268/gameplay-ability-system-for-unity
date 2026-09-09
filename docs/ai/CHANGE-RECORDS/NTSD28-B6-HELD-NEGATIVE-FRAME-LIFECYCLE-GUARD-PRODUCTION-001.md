# NTSD28-B6-HELD-NEGATIVE-FRAME-LIFECYCLE-GUARD-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B6-HELD-NEGATIVE-FRAME-LIFECYCLE-GUARD-PRODUCTION-001
status: VERIFIED
change-kind: TEST_FIRST_HELD_LIFECYCLE_GUARD
code-path: Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6HeldNegativeFrameLifecycleGuardProductionEditorTests.cs
authority: Goal13b用户授权；playable battle_world.cpp:8556-8578
evidence: VERIFIED_HELD_LIFECYCLE_GUARD_SUBSET / RED6_FAIL_11_CONTROL_PASS / FOCUSED17_PASS / SAKURA_SAKON_DUAL_PLAY_PASS / B6_388_PASS / REFILL9_PASS / SELFCHECK_PASS / BUILDS0 / CONSOLE_SELFCHECK_DIAGNOSTICS_RETAINED / SCENE_UNCHANGED
-->

# NTSD28-B6-HELD-NEGATIVE-FRAME-LIFECYCLE-GUARD-PRODUCTION-001 — Task Contract

Goal13b用户明确授权，先独立守卫包，再收口包1，随后包2，最后一轮共享回归。IN_PROGRESS / TEST_FIRST，脚本修改前建立。

Authority为当前B1E13AE1正式EXE对应playable BattleWorld28::step_frames_range，battle_world.cpp:8556-8578。源码原文：
> A kind-2 child is updated by FUN_00417F80 from its holder's
> wpoint. Its on-hand frame commonly has wait=0,next=1000, but
> native traces prove that action remains active for the entire
> relation instead of entering ordinary lifecycle resolution.

## 原状、谓词与精确修改
Unity HandleFrameTickExit 的frameId<0或>=MaxFrameIdExclusive无条件Free，Sakon -888在C09/C20 preserved后C25误Free，用户已复核根因。
仅在该Free分支前加guard，返回true使当前late slot停止普通后续处理，不写状态，不调用Free。11/12 frameGroup分支和MaxFrameIdExclusive判定完全保留。
复用SimulationQueryAndLinkModule.HeldObjectProcessAll的入口谓词：world.IsActiveForCurrentPassInternal(entity) && entity.Runtime.LinkState < 0。既有RuntimeSlots/current-pass基础设施定义active；LinkState即Authority interaction_state的canonical signed carrier。C25 Run已有active occupant门，guard显式复用该方法，不建立第二关系规则。
该入口谓词与reciprocal-valid检查分开：负关系即held候选；holder缺失/越界/mismatch由held pass记录failure并preserve，其C25仍满足Authority interaction_state<0。不得改用ResolveActiveHolderSlotIndex>=0或强加reciprocal才能held，避免把invalid-preserve变成清理。guard不提前拦截C09/C20，也不吞诊断。非held/已release LinkState>=0仍Free。

## 授权范围
- Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs，仅HandleFrameTickExit负/越界分支guard。
- Assets/NTSD/Scripts/Test/Editor/NTSD28B6HeldNegativeFrameLifecycleGuardProductionEditorTests.cs及meta，新focused。
- 本Task/同ID Record、Ledger/STATE/对齐总表、Temp证据。
其余原Goal13授权保持；本守卫不改原72测试/probe、terminal/kind3/callback/资源/Scene/架构/上界，未启动包2。

## 验收、风险、停止、回滚
test-first真实RED：held负action完整tick仍存活；非held负action、release后负action仍Free控制组，invalid/mismatch仍诊断+preserve；补real/generic、负/upper边界及11/12不变。
focused通过后复跑原Sakura10/Sakon-888真实driver Play，均Free0/live/linkPreserved/下一child正常；原72复跑。随后串行包2与共享最终守卫/72/包2/B6(275+新增)/refill9/kind3 92/terminal80/fullSelfCheck/双build/validator/Scene。
SceneSHA D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11；Editor b1b02287/2022.3.62f3/NTSD_Battle，不第二实例。
任一旧测试或控制组变化、谓词需扩文件、清单外diff/Scene变化即停。风险为过宽guard吞普通Free，必须非held及release控制；guard过窄破坏invalid-preserve，必须invalid诊断保持。
无新持久化、queue、manager或关闭owner；保留十一阶段关闭，fixture finally清理自身handle。
回滚须用户批准，仅本包增量；不能回退Goal1-13/用户内容，不改既有terminal/kind3合同。
新fixture编写时纠正一次不存在的release方法引用，改用既有kind3 held-pass生产释放入口建立released控制组；production仍未改，不将编译引用错误当RED。
有效RED jobba9ff849a7034b818bd5432137e55de8 completed17，6条held失效失败且未capped，其余11控制无失败；真实full tick丢失handle。仅HandleFrameTickExit负/越界分支写入active+Runtime.LinkState<0 guard returntrue，11/12与上界比较未改，不触碰任何其它production。源码注释及谓词论证见事前合同。
focused GREEN job4f11fa871eb040a7af0be72b3e80100a 17/17 PASS（6原RED转绿、11控制保持绿）；ProductionCompile Editor工程0error/129warnings。下一复用原包1双Play，无修改其probe。
双Play PASS，Temp/Goal13b_Guard_DualPlay.result.json：Sakura tick6 / Sakon tick7均freeEvents0、live/linkPreserved/nextProcessed true，C09/C20 boundary保持；World4→4、logic2→2、renderer2→2，Console0。守卫17/17通过；第2步原包1 focused72重新提交，之后再做其Play复跑，共享最终门禁待后继包2。


## Goal13b共享最终验收（2026-09-10）
所有生产与probe修改完成后，共享一次B6分类jobb02fae001b4341f6b7c78e0adfae0b9b，388/388 PASS = 原275 + 包1新增72 + guard17 + 包2新增24。该单次结果内分别复核guard17、missing72、DVX24、Goal11 kind3 92、Goal12 terminal80全部Passed，不重复运行这些子集来凑计数。另held-refill job01967de39d2f46e7959426b280165b25 9/9 PASS（7+2）。证据Temp/Goal13b_Final_B6_Result.json、Goal13b_Final_Refill_Result.json。
fresh fullSelfCheck请求2026-09-09T18:56:54.9168794Z，结果mtime18:57:36UTC，实际PASS；Temp/Goal13b_Final_SelfCheck.result。
两套最终命令：dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly；dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly。各0error/22warnings（增量build），Temp/Goal13b_Final_RuntimeBuild.txt和Goal13b_Final_EditorBuild.txt。Editor项目已确认包含两个新test源。
指定Editor b1b02287 / Unity2022.3.62f3 / NTSD_Battle。Scene dirtyfalse/root13，SHA D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11。正式根EXE SHA重新确认B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033。

### Console实际结果及限制
各次scoped Play probe均未捕获Error/Exception/Assert，普通退出且World4→4、logic2→2、renderer2→2。最终fullSelfCheck之后Console保留15条error级返回：7条是注册失败/租约mismatch等负向夹具，源码与栈已确认；8条为BattleDynamicMeshBackend.cs:641的Converting invalid MinMaxAABB引擎Assert，栈回到CheckHeldPresentationGeometryContracts、CheckBattlePresentationShadowBuildContracts、CheckHitRecordPresentationLifecycleContracts。SelfCheck最终断言结果PASS，但不报告最终Console0、不把8条引擎断言说成预期或已修复。该表现路径根因未在本任务中排查；未清空Console隐藏日志。Temp/Goal13b_Final_ConsoleDetailed.json保存完整来源。
用户指定的既有测试均通过，无测试失败；上述引擎诊断不作为全应用无错误的证据。本次VERIFIED仅三个授权精确行为子集，不能扩大为完整战斗系统对齐。

### 关闭与范围
Goal13b只改原授权文件加HandleFrameTickExit guard及其新test/meta/Task/Record；未改其它C25路径、11/12分支、MaxFrameIdExclusive、callback API、generic RNG、Scene/内容/AI/schema。三包独立Record、依赖和RED均保留。validator与最终逐文件审计见本Record追加条目及Temp/Goal13b_Final_WorktreeAudit.json。无commit/push/revert/清理用户工作。
报告后停下等待用户复核与Goal14（+2F8前的联合schema方向）；不自动创建下一Task/Record。
最终审计：Tools/Validate-ChangeLedger.ps1 PASS，443 records / 382 governed code files，详见Temp/Goal13b_Final_Validator.txt；git diff --check目标production通过（仅LF/CRLF提示）。本轮基线1497→1505条status，逐文件SHA仅15条授权路径变化，无范围外变化或消失的status行；Temp/Goal13b_Final_WorktreeAudit.json。新增两test/meta/Task/Record共8条，原72测试/probe及其它既有测试均未修改。
