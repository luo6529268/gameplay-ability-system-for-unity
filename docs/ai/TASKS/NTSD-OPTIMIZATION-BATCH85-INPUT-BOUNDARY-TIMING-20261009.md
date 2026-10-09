# 第85批 CharacterInput 四边界耗时归因

状态：VERIFIED / ATTRIBUTION_COMPLETE / DIAGNOSTIC_ONLY / NO_DEFAULT_PROMOTION。2026-10-09 +08:00。仅本包归因完成，H07/H11未完成；[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH85-INPUT-BOUNDARY-TIMING-20261009/REPORT.md)。

需求：按有限首阶段有效合同第0—8节继续 H07 未达性能门。第84批千人 capacity-record guard 两资格均实际 skip0，不采用、不再测该候选。第83批 EntityInputPass 约10.19ms，RemainingAiDecision约4.84ms；已有互不重叠生产AI、combo、consumer refresh/row refresh合计约5.59ms，差额不是已证数学计算或可直接删除工作。四处尚无独立归因：FreezeProducerState、RefreshCharacterInputSnapshotsAfterProducer、TryApplyProxy、ProcessNativeSampledState。本包只回答这个不同且必要的问题，不重复 Role exact 六分支采样或旧 Broadphase 评估。

## 冻结范围与合同

唯一四个 C#：
- Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs：默认OFF诊断开关；RunCharacterInputPasses四既有调用的计时/调用数，不改变调用条件、内容、顺序、live-slot扫描与例外传播。
- Assets/NTSD/Scripts/Simulation/Diagnostics/SimulationWorld.DetailTimingDiagnostics.cs：既有BattleAiInputDetailPhase编号0—29不变，append ProducerFreeze30 / ProducerSnapshotRefresh31 / ProxyApply32 / NativeSampledState33，Count34及稳定名字。
- Assets/NTSD/Scripts/Test/Editor/CharacterInputLiveSlotLoopEditorTests.cs：先补12具名case，反射默认OFF、四schema、启用/native四组合、关闭清空、两profile逐tick输入域/checksum/RNG等价；旧测试/断言不动。
- Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs：独立冷入口，复用81 shared-binding/calibrated logic observer的两布局四窗；ON/OFF各一，32新case中其余20为request4/outside2/restore4/invalid-owner5/session1/coverage4。

诊断全开也不得分配/扩容、IO、log或委托新建；记录器及现有report数组按Count冷构建，多四long数组字段payload128bytes不是全RSS。不开诊断时不计四phase；全局recording关闭时也不计。新flag不参与逻辑checksum。CPU纯diag owner在原owner shutdown前恢复，外部Play退出同幂等恢复，不新增worker/pool/lease资源、不改变11stage；异常由既有finally/ShutdownAndExit恢复。没有新的战斗规则字段、资源或新算法。

正式336B44/playable closure B97保持；33ms/3ms/max2、input/RNG、原候选/透明排序/publication/segment/fail-closed/seal容量及关闭保持。不读活跃BattlePresentationShadowBuild方法体。不改Scene/Prefab/资源/Importer/settings/Gen/Plugins/Server；不切Role默认、不启用84guard、不引入Grid/Burst/EXT1/ATLAS/Mono。生产默认推广待准确人类授权，与本包独立。

## 验收及唯一窗口

先新32 RED（API/schema不存在）再GREEN，原Core live-slot/input tests及原RoleBranchOwner/PostBindingsRole request/owner相关具名回归；按实际用例/失败记录，不报catalog全跑。原Editor导入编译0CS；run_tests/get_test_job单flight，保留每次XML和终态。

窗口前4源指纹冻结；唯一菜单 NTSD/Optimization/Run Batch85 Input Boundary Timing Windows。所有request复用81/83的OID1 Sakura、1000 actualAI、seed/spawn/minimalAI/profile、shared binding、相同renderer分辨率、120warmup+180sample。四请求仅输出路径/诊断World flag有差异；collector均role、roleBranchTiming关闭、84guard关闭。窗口路径 windows-01/{00-dispersed1000-boundary-off,01-dispersed1000-boundary-on,02-combat1000-boundary-off,03-combat1000-boundary-on}/report.json。旧证据只定位，不作为新窗A/B。

四phase call counts/timings由原report动态PhaseCount出；ON每sample持续有call，OFF四call/time0；owner稳定/恢复、两对末tick300完整checksum JSON与原hash同、校准同步StepOneTick稳态180scope零事件、三残留0/原Scene clean/SHA保持。只计时归因，不把ON成本当候选收益，不以末态代替所有逐tick/native或Android/120FPS证书。测出最贵边界后才决定必要后继，不猜缓存安全。

## 留痕、恢复与状态

Change NTSD-OPT-H07-INPUT-BOUNDARY-TIMING-085；Operation同Task ID。9编辑前当前文件＋现有公共XML/result逐份Copy-Item（目标必须不存在，无Force）并核SHA，manifest在Operation。既有shared request当前ABSENT，窗口用SessionState请求，不创建/删除该磁盘用户文件。共享result按原Suite规则每次替换前SaveNew保存，actual GUID名字记录；XML每次测试覆盖前另存。只patch本包hunks，原dirty版本为恢复来源，任何恢复须独立授权与Operation，无reset/restore/delete。

主进度只原battle-optimization-progress-tracker.md；STATE/handoff/Ledger/INDEX为强制审计，保留旧事实。本包准备不算执行；测试开始才64执行，阶段4/6、H07/H11 OPEN、34父关闭0、Goal active，不因包完成标Goal完成。
