<!-- CHANGE-RECORD
id: NTSD28-PREPARING-SHUTDOWN-OWNER-CAPTURE-001
status: VERIFIED
change-kind: BATTLE_PREPARATION_SERVICE_OWNERS_AND_STALE_CONTINUATION_GUARD
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/App/AppManager.cs
code-path: Assets/NTSD/Scripts/Test/BattleTestBootstrap.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28PreparingShutdownOwnerEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B11NativePublicationPlayProbeEditor.cs
authority: Active battle alignment goal; existing eleven-stage ordered shutdown contract; real Preparing Play failure and actual App/direct battle spawn ownership call chains.
evidence: RED_8_FAIL / FOCUSED_26_OF_26 / FULL_SELFCHECK_PASS / PLAY_PREPARING_2_RUNNING_2_APP_1 / ZERO_ACTUAL_BORROWERS / TWO_FRAMES_STILL_STOPPED / CS_0 / SCENE_CLEAN / LEDGER_469_34_PASS
-->

# Preparing关闭owner和过期启动续体

这是R16即时回访，只修战斗准备/关闭适配，不改变Unity/GAS框架、菜单操作、战斗规则、33ms、schema或关闭阶段顺序。Task为同ID。原始RED在E2的play-preparing-diagnostic.json，World4/slot2，cached pool缺失导致关闭失败；正常Running关闭有真实PASS，不能替代Preparing证据。

## 已追踪的所有权

AppManager.SetupBattleCharacters及BattleTestBootstrap.SetupTestCharacters直接通过LF2ObjectPool.Instance.Get借出renderer，实体ModuleBind登记至当前driver.World；Pool.ReleaseAllActiveForShutdown通过entity.RegisteredWorldForSimulation.LogicReferencePool归还逻辑实体。pool本身无World字段，不能在关闭时按全局单例名猜owner。Driver.EnterPreparingState清cached owners，ApplyMatchConfig只抓当时已有实例，BeginBattleAllocationSeal才创建服务；两个出生caller在这两者之间运行且之后有await，构成实际缺口。Factory也由同一driver在stage3关入口、stage6丢队列。

## 写前准确范围与方法

- Driver新增仅限Preparing且World已存在的PrepareBattleRuntimeServices，在首次实体materialize之前建立现有factory/pool的cached owner；首次接管拒绝已有borrower/pending队列，重复调用只接受同一owner，不在Stopping/Stopped创建服务。创建后立即登记，再调用原BeginBattlePreparation，不提前allocation seal、不改shutdown阶段。
- Driver新增非序列化preparationGeneration，EnterPreparingState递增；内部IsBattlePreparationCurrent(generation, expectedWorld)检查Preparing/同World/同世代。此为Host启动续体有效性，不进入simulation字段/schema/checksum。Shutdown令生命周期失效；重新准备即使复用World也通过新世代拒绝旧续体。
- AppManager仅InitializeBattleAsync：记录scene/driver准备身份，在首个yield和sound await之后检查；ApplyMatchConfig可能进入新世代，之后重新捕获；SetupBattleCharacters前调用service准备。关闭后旧续体直接返回，不EnablePresentation/SetPaused/state=BattleRunning，不改菜单输入/选择/导航。
- BattleTestBootstrap仅Start：在获得driver/核对profile后捕获generation/world，加载数据、sound和出生后Yield均验证身份；首次SetupTestCharacters之前调用service准备。其他测试输入与生成算法保持。
- 新Editor focused覆盖服务owner首次登记/幂等、非法生命周期无创建、已有foreign borrower/queued work拒绝、generation重新准备及停止后失效。先RED后生产代码，真实Preparing RED已存。
- 原E2 Play probe增加直接actual pool引用的borrower计数，关闭后两帧检查仍Stopped/无World/零borrower；正常Running与Preparing各两次进入退出，复用四项资源事务。测试只用自有临时对象，不保存Scene。

## 副作用、异常与回滚

仅把原本出生/封印时创建的服务提前到出生前的同一同步准备段，pool/factory依旧Unity-native原owner，无替代manager；部分准备失败仍保留已捕获owner供原shutdown回收。新方法不允许在已有实体之后猜测池归属。异步旧任务不再激活已停止战斗；正常启动参数/角色/位置/规则保持。专用worker Join及stage1..11完全不重排。精确回滚仅本Change涉及symbols及测试，保留E2的两个driver hook和用户所有已有改动；删除/回退遵守现有授权规则。

验收需Unity CS0、focused、自有实际pool计数、Preparing与Running真实Play退出重进、Scene dirtyfalse、保护hash与Ledger。任何未运行项明确pending，不以focused代替完整目标。

写前补记：Driver用非序列化准备标记保证重复登记仅同owner；首次登记同时要求World无对象/槽位且未seal。直接Bootstrap在最初delayFrames之前捕获driver身份，延迟后先guard再创建TimeWheel；profile reconciliation后重新捕获准备身份。App异常回收也核对同World/同世代，避免过期sound失败关闭后继战斗。测试先写8项（含三生命周期参数），生产未改。

写后：focused RED job819c831f2d0949779724e5b65b066e4b实际8/8因缺API失败。Driver准备世代/有效性判断、首次服务owner捕获及幂等检查已写；App InitializeBattleAsync、直接Bootstrap Start在出生前接入并保护异步续体；Play probe实际pool/factory及关闭后两帧仍Stopped检查已写。原E2 driver阶段1/5 hook保持，顶层关闭零重排；生产compile/focused/Play尚待执行。

focused GREEN job4059ac66247d42e5b22582ed97e708ad已26/26（新增8/既有ordered shutdown4/E2 atomic14）。App ApplyMatchConfig同步发生的World/世代变化在finally捕获，保证同步初始化失败仍由自身catch关闭，而过期sound异常不关闭后继World。

Play probe写前补充：新增appBootstrap请求开关，使用既有SuppressEntityCreationForProductionStress让直接入口只完成资源准备且不生成角色；等待真实ready并确认World0后，App.SetMatchConfig设置测试自有的两名已载入type0角色，再反射调用实际InitializeBattleAsync。不改生产App/menu入口，finally恢复suppress=false。此额外Running Play用来覆盖实际App启动caller；原direct Preparing/Running退出重进矩阵继续，报告注明入口与角色ID。

真实Preparing修后首轮preparing-owner-fixed-1已PASS：Preparing/World4，关闭到RuntimeMapCleared，actual pool borrower0、关闭后两帧仍Stopped/Worldnull/factory关且空，E2四项4/4、113资源零残留。完整BattleRuntimeSelfCheck本次真实PASS（selfcheck-result.txt，结果mtime晚于请求，非旧结果），Scene dirtyfalse/root14。Preparing重进、Running/实际App启动矩阵尚在执行，不提前VERIFIED。

## 本包最终出口

五次实际Play均PASS：preparing-owner-fixed-1/2、running-owner-fixed-1/2、app-owner-fixed-1（真实App InitializeBattleAsync，角色50/52）；均World4，Completed/RuntimeMapCleared、actual pool borrower0且quiesced、factory关闭/queue0、关闭后两帧仍Stopped/Worldnull。每轮E2四项4/4/113资源零残留。完整SelfCheck PASS，focused26/26，CS0、Scene dirtyfalse/root14、Ledger469/34PASS、3059保护3050不变/9声明脚本变化/零缺失。报告为artifacts/diagnostics/NTSD28-PREPARING-SHUTDOWN-OWNER-CAPTURE-001/REPORT.md。

本Record VERIFIED仅关闭本次准备/关闭适配与实际caller防过期续体，不改变其他R16后继。E2原子事务限定出口可返回关闭；下一E3 source/cache/caller审计已READY，Q02/总目标保持未完成。无音频资源修复、正式DAT部署、schema变更或非战斗操作改动。
