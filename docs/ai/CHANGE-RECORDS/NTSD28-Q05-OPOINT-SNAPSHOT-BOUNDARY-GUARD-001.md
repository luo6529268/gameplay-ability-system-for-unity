<!-- CHANGE-RECORD
id: NTSD28-Q05-OPOINT-SNAPSHOT-BOUNDARY-GUARD-001
status: FOCUSED_TEST_PASS
change-kind: SNAPSHOT_BOUNDARY_GUARDS
code-path: Assets/NTSD/Scripts/Animation/LF2Tasks/LF2TaskRingBuffer.cs
code-path: Assets/NTSD/Scripts/Animation/Character/LF2ObjectPointFactory.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleStructuralWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshotRestore.cs
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Session/BattleLockstepSession.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05OpointSnapshotBoundaryEditorTests.cs
code-path: Assets/NTSD/Scripts/Simulation/Host/BattleSimulationWorkerBoundary.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Session/InProcessBattleKernelHost.cs
authority: Q03 VERSION-IDENTITY-AND-CAPTURE-CONTRACT; Q05 approved non-tick/non-structural/empty-world-owned-OPoint snapshot policy; existing ordered shutdown hard gates.
evidence: RED14 + HostRED1; final-results.xml 187/187; SelfCheck PASS; play-result.json SCOPED_PLAY_PASS; REPORT.md
-->

# Q05 OPoint与快照边界

准确9脚本。现capture没有完整tick/OPoint前置，restore只看registry _ticking且driver先StopDedicatedSimulationWorker；_ticking只覆盖局部pass。logic queue固定属于其world，实际StructuralWriter强制task.targetWorld=owner，因此不按task声明跨World过滤。renderer实际targetWorld→parent.Match→driver.World；Match会回退driver，guard用RegisteredWorldForSimulation和已知fallbackWorld复现，不创建服务。

计划新增ring只读TryPeekAt，不改enqueue/dequeue/顺序/计数；factory按explicit target、registered parent、既有host fallback观察队列。world统一boundary读取完整tick depth、registry ticking、structural playback depth、现有接受请求状态、logic队列和host observer。host observer在CreateProductionWorld绑定，在真实World解绑时清除，优先已捕获_battleObjectPointFactory；未捕获时只读TryGetInstance。Standalone World只读现有factory并用现有driver.Instance.World作fallback：已读取SingletonBehaviour源码，driver.Instance只是静态getter，不创建；LF2ObjectPointFactory.Instance会创建，guard禁止调用。MMSingleton.TryGetInstance也已确认仅返回现有引用。

NTSDBattleTickSystem以try/finally围住完整RunTick，深度为非序列化边界元数据，不改pass顺序；StructuralWriter实际Spawn/SpawnMultiple/Register/Unregister/Free/Destroy用平衡try/finally计数，existing late segment depth一起观察。worker flight/awaiting ack、Stopping/Stopped拒绝，driver restore先查guard再按原路径停止已空闲worker，不为拒绝停止/清空。复用WorldBusy失败；capture先Invalidate目标再拒绝；session所有提前失败使目标无效。Ring复用world gate，不改ring发布协议。

拒绝不Flush/丢任务/造对象/改world原状态；仅目标capture buffer无效。两World隔离、parent/fallback优先、logic固定owner、renderer多任务以及wrap-around读取均要测。没有新增manager/queue/worker；observer随当前host/World阶段9解绑，tick/structural深度始终finally归还；现有BeginShutdown接受请求false阻止Stopped World重入，不改十一阶段顶层顺序。owner线程使用，未扩大为任意多线程并发World API。

先新增实际pending反例、tick scope和structural callback、driver不停止inflight flag、session/ring、副作用与warm无分配RED，再生产；完整相关snapshot/restore/worker/structural/queue tests、SelfCheck、真实Play中仅注入和移除明确属于probe的任务，不把生产guard变成清队列。新probe与tests同一声明文件。

保持Unity/GAS/非战斗/33ms/3ms/Scene/InputActions/Gen/Plugins/外部包/资源/stage.dat USER_HOLD及例外；禁止computer-use。当前12/20/23/1/1未发布中间态，本包不改schema或trace；相关内容hash、联合版本/旧版本拒绝/回放仍后继。回滚需批准按preimages精确差量，不覆盖用户/前包工作。

## 已写

RED14/14失败，实际复现有pending和structural callback仍能capture、busy host restore会成功并reset状态、session mismatch留下有效旧buffer。9脚本已写，完整tick/六structural方法平衡scope、只读queue匹配、既有host绑定/解绑及restore前置。原逻辑body/pass顺序保持，未新增队列或schema。待compile/扩展边界与相关focused/SelfCheck/Play。

## 输入准备与完整执行入口补充（事前）

初始19+相关175回归中只有一个旧worker出生Y夹具失败，现独立核对；未改运动规则。调用链复核确认Host两个StepOneTickInternal、BattleWorldSimulationTickExecutor.Execute和InProcessBattleKernelHost.TryStepOneTick在core RunTick外仍准备stage/input并记录结果。为完整边界，将相同平衡scope围住这些既有body；新增准确两生产路径，当前合计11脚本。Host以入口时捕获的World引用配对，避免World重绑时计数错归。只增加边界元数据，不改变帧输入、worker协议、journal、checksum、调用顺序；新字段不入payload。先加Host Get/Before/After输入回调实际RED，再实施扩展，已有body归一化比对及相关kernel/worker/快照验证。

## 最终限定出口（追加）

# Q05 双 OPoint / 完整执行入口快照边界出口

状态 FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / JOINT_SCHEMA_PENDING。准确11脚本（10生产+1测试/probe），另1个worker测试夹具独立Change。总体Q05及总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE；本结果仅关闭父步骤3的boundary子条件。

World聚合capture/restore、driver/session/ring现在在完整tick、structural mutation、worker flight/awaiting ack、停止态或本World任一OPoint队列非空时拒绝。拒绝不消费队列、不创建服务、不停止worker、不改变原World或待恢复snapshot；capture目标明确失效。logic队列按固定World owner，renderer按显式target→registered parent→既有host fallback；优先driver已捕获factory。observer在既有World绑定/关闭阶段9解除，不新增manager，不改变十一阶段关闭顺序。只支持既有owner线程入口，不承诺任意并发World API。

完整范围补入Host两种StepOneTickInternal、worker Execute及现有InProcessBattleKernelHost.TryStepOneTick，使输入准备/回调及结果发布也处于边界。core tick、六structural方法和四Host/worker/kernel方法原body token连续保持；见两份body-preservation.json。修改只有平衡try/finally边界元数据，不改变规则/pass/输入/协议，未更改schema。

实际验证：
- 原14 RED全部失败；初18 GREEN通过。扩展19及相关175首次174PASS/1FAIL，worker旧Y45夹具经独立Change保留原parentY40、补parent落地0、child改5；正式physics_integrator type0接触钳制与当前OPoint公式支持，生产运动未改。
- 新Host输入反例RED1：GetFrameInput/BeforeSimTick/AfterSimTick三次都错误capture成功（expected0/actual3）；完整入口scope后通过。
- Unity桥接run_tests最终187/187 PASS，包含20个本包边界例及snapshot/restore/ring/worker/ordered shutdown/OPoint/structural/raw snapshot、InProcess authority、FormalKernelFullReturn。准确XML/job已归档，不以发现数量代替执行数。
- 完整BattleRuntimeSelfCheck：请求11:58:10.2121605Z，结果11:58:48Z之后文件PASS；实际精确mtime见SelfCheck-result-utc.txt。SelfCheck运行时一次Console桥接30s超时，之后读取成功；无重启/第二Editor。最终error CS查询0条，真实编译与测试已执行。
- 真实NTSD_Battle Play：tick5，对象4→4；已捕获renderer owner；一条probe-owned logic task和一条renderer multi task分别触发拒绝，任务reference/count保持；driver拒绝不改变worker引用；probe仅在断言后移除自己注入的任务，空闲capture成功；自动退出Play。workerWasPresent=false，因此worker运行中证据来自focused，不能宣称真实worker Play或物理技能键/全技能通过。
- Scene isDirty=false/root14，SHA a96e11064f1bd054d9d5547fe8f02c702d971b3972d55754886d75b2dce9d28f保持旧基线。3059保护项2945同/96既有或声明变化/18旧缺失，无新增缺失，相比semantic包新增9项均在本包或独立fixture声明内。

命令通过现有Temp/Goal13_bridge.py（Python -X utf8）执行refresh_unity/run_tests/get_test_job/read_console/manage_editor/manage_scene；SelfCheck用既有request/result机制。精确测试选择见test-selection.json（原14类）及final-job-start对应新增InProcessLockstepAuthoritySessionEditorTests/FormalKernelFullReturnCommitSeamEditorTests；最终XML187记录为准。未使用computer-use，未改Scene/资源/非战斗/GAS/Gen/Plugins/外部Server，未删除旧素材、提交或推送。

后继：先只读核对新增FrameSounds/profile/centerz/chp/cmp/六double及BMP/stats/armor/piece在所有实际内容hash/版本消费者的覆盖，区分源身份、值hash、运行时checksum、缓存guard和历史诊断，不凭raw+tag身份关闭其他消费者。再按Q03同窗口完成entity13/aggregate21/checksum24/character2/base2及trace v3/raw v2/source wrapper v2/50字段+2F8；最后旧版本拒绝、新capture→restore→同seed/input replay及后续Play。当前12/20/23/1/1仍INTERMEDIATE_UNPUBLISHED，禁止发布或跳Q07。

最终审计：Validate-ChangeLedger.ps1 PASS，500 Records/当前5脚本diff覆盖；其他本包脚本已在用户外部创建的HEAD f3e11239，不能把5解释为全部包scope。精确11脚本/11个原body保持检查与187结果见validation-summary.json、final-test-selection.json。
