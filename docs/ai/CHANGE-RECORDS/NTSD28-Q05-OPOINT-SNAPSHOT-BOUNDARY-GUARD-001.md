<!-- CHANGE-RECORD
id: NTSD28-Q05-OPOINT-SNAPSHOT-BOUNDARY-GUARD-001
status: IN_PROGRESS
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
authority: Q03 VERSION-IDENTITY-AND-CAPTURE-CONTRACT; Q05 approved non-tick/non-structural/empty-world-owned-OPoint snapshot policy; existing ordered shutdown hard gates.
evidence: PRECHANGE_CALLGRAPH / TEST_FIRST
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
