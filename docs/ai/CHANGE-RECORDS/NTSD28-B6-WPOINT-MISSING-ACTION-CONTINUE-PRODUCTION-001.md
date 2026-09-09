# NTSD28-B6-WPOINT-MISSING-ACTION-CONTINUE-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B6-WPOINT-MISSING-ACTION-CONTINUE-PRODUCTION-001
status: VERIFIED
change-kind: TEST_FIRST_MISSING_ACTION_CONTINUE_PRODUCTION
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleHeldObjectWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/Interaction/SimulationQueryAndLinkModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6WpointMissingActionContinueProductionEditorTests.cs
authority: Goal13用户授权；当前playable battle_world.cpp:7971-7994。
evidence: VERIFIED_MISSING_ACTION_SUBSET / DEPENDS_ON_HELD_LIFECYCLE_GUARD / RED_COMPLETED72_FAILED_CAPPED25 / FOCUSED72_PASS / DUAL_PLAY_TWO_RUNS_PASS / B6_388_PASS / SELFCHECK_PASS / BUILDS0 / SCENE_UNCHANGED
-->

# NTSD28-B6-WPOINT-MISSING-ACTION-CONTINUE-PRODUCTION-001 — Task Contract

Goal13 包1 / IN_PROGRESS / TEST_FIRST。用户明确授权串行两包，此包须focused及Play通过后才能启动包2。

Authority：当前正式B1E13AE1 EXE对应playable BattleWorld28::settle_held_refill_objects，battle_world.cpp:7971-7994。literal action先写，missing child frame诊断continue，declared frame无文本WPoint使用全零record；随后才写facing/hold/pose及DVX/kind3。refill耗尽先于terminal，terminal先于action。
Unity原状：real旧current-frame-null gate阻止refill及恢复；写missing action后仍写pose及投掷；generic action/pose耦合。只能移除上述多余副作用，不修改terminal/kind3已闭合算法。

## 写入清单
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleHeldObjectWriter.cs`
- `Assets/NTSD/Scripts/Simulation/Passes/Interaction/SimulationQueryAndLinkModule.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B6WpointMissingActionContinueProductionEditorTests.cs`
- 本Task/同ID Record、CHANGE-LEDGER.md、STATE.md、对齐总表及Temp产物；新test自动meta。

## 不变量、验证、回滚
瞬态UnsupportedWeaponAction不进入snapshot/checksum。耗尽→terminal→literal action→missing continue→pose/DVX/kind3。missing不Free/unlink，不改facing/hold/pose/motion/HP/ReleaseTick、不抽样；有效无文本WPoint仍正常。
focused先RED：real/generic正值/-888、current missing、missing→valid、无文本WPoint、refill current missing、type1/2/4/6 DVX和kind3、下一slot及C09/C20。Play用current Sakura/Sakon及真实driver，明确动作夹具方式。
两包最后共享B6、refill9、Goal11 92、Goal12 80、fullSelfCheck、双build0error、validator、SceneSHA D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11。instance b1b02287 / 2022.3.62f3 / NTSD_Battle，无第二Editor。
任何既有测试失败、需改terminal/kind3语义/删除callback、误Free/unlink、清单外diff或Scene变化立即停。generic nonkind3 heavy RNG、+2F8/schema全部排除。
无新queue/manager/lifecycle owner；瞬态结果栈内消费，observer只属于test，finally解除并清理owned entities。遵守十一阶段关闭。
回滚必须用户批准，仅反向本包备份对应增量与新增文件，不回退Goal1-12/用户工作。不可回退边界为既有已验收terminal/kind3/refill。
首次RED job43164ca74f864f0a8724521c3efc7e36 completed72 failed，25明细capped。action777/-888被投掷改40/0、real恢复卡在-888均为行为RED。next-slot两项新fixture误将event buffer逻辑capacity设50，slot51越界；仅修新fixture为400再跑，不把该异常算行为RED。生产未改。
修正fixture后RED jobcce4c413b93e411397f7de705f76690d completed72 failed/capped25；next-slot现行为RED为缺C09/C20诊断。生产四文件已按合同写入：瞬态UnsupportedWeaponAction；real去current-null前门、literal后判null；generic分离action与pose且SyncHeldPose原语义保持；query仅诊断+刷新+continue，无Free/unlink。COMPILE/FOCUSED/PLAY待验。
首轮新focused job30d5fe8899544ba489e1581342982ea4 failed，均只运行新fixture。发现LF2FrameCache.GetFrameDataById对范围内未声明帧返回EmptyFrame，D==null不足。沿用现有HasFrame(action)精确声明存在性（不改cache），新test的null断言同步改为HasFrame false，保留literal/全部副作用断言。此为当前包实现修正，不涉及既有测试或terminal/kind3。
HasFrame修正及Play probe编译后，focused job599246954fba4e96af7ffc965d81e54d 72/72 PASS；Editor build0error，Scene SHA不变。Play待运行，包2未启动。
Play attempts1/2新probe断言失败但cleanup4→4、pool2→2、Console0。attempt2实测Sakura51 action10/C09+C20保留、next child action20→21、noFree；唯一跨tick状态差为FrameDelay31→30（LF2Entity.TryEnterReleaseFrameAdvanceAfterDelay在C09之前）。仅新probe预期包含既有pre-C09 decrement，仍逐字段检查两次missing边界。非production失败、非旧测试失败；不修改生产。


## Goal13硬停止（2026-09-10）

状态：BLOCKED / FOCUSED_TEST_PASS / PLAY_NOT_ACCEPTED；不是VERIFIED。Goal13包2 `NTSD28-B6-WPOINT-DVX-WEAPON-HP-PRESERVATION-PRODUCTION-001` 未建立Task/Record、未修改代码、未运行focused/Play。未启动Goal14。

实测证据：
- 修正容量后的RED jobcce4c413b93e411397f7de705f76690d completed72 failed，25明细capped，不声称72全红。Temp/Goal13_Pkg1_RED_Corrected_Result.json。
- focused job599246954fba4e96af7ffc965d81e54d 72/72 PASS；Temp/Goal13_Pkg1_GREEN2_Result.json。之后仅Play probe的记录及跨tick timer预期修正，focused未再跑。
- 两次Editor项目编译0 error：ProductionCompile/ProbeCompile；均发生在最终Play probe修订前，不冒充共享最终双build。Temp/Goal13_Pkg1_ProductionCompile.txt、Goal13_Pkg1_ProbeCompile.txt。
- Play attempt1/2只暴露新probe的pre-C09 FrameDelay31→30比较错误，attempt3已修正该期望。
- 最终Temp/Goal13_Pkg1_Play.result.json：Sakura OID1/action51→child OID123/action10，tick6 slot51；C09与C20保持literal10/link=-1、facing/pose/motion/weaponHP，next slot53 action20→21且存活；Free0。
- 同一Play的Sakon OID35/action235→child OID123/action-888，tick7 slot51；C09和C20均保留literal-888/link=-1、期望状态完全相同，下一child slot53也正常；随后whole tick Free1，child不再active且relation清除。没有将该失败通过删断言掩盖。
- 各次finally普通退出；最终World4→4、logic borrower2→2、renderer borrower2→2，Console0，Scene NTSD_Battle dirtyfalse/root13。指定instance b1b02287/2022.3.62f3，无第二实例。

只读定位（未增加任何production/test修复）：
- Unity `BattleLateEntityLifecycleModule.cs:199-214`在C25 frame tick之后调用`HandleFrameTickExit`；`:941-964`无held gate，frameId<0直接`FreeEntityLikeExe()`；`LF2Entity.cs:4564-4569`转StructuralWriter.Free。这个文件不在Goal13精确授权清单。
- Authority `battle_world.cpp:8555起`的`step_frames_range`先对`interaction_state<0`返回held，不进入后面负action→pending terminal逻辑；`simulation_tick_driver.cpp:1016`逐slot frame、`:1063起`resolve pending；`battle_world.cpp:8303-8305`仅消费lifecycle_resolution_pending。
- 已观察到C20仍存活而tick结束Free1；当前probe没有捕获Free调用栈，因此上述C25具体Free归因是完整静态调用链支持的定位，不能冒称动态调用栈证据。负held和负action满足该确定分支，修正需跨已授权路径到C25 lifecycle owner。

用户硬停止条件已触发：误Free/unlink与需要清单外C25修正。停止所有实施与后继验收；不改已闭合terminal/kind3，不删callback，不修改旧测试，不继续Play。保留当前包四production文件、一个新测试/meta和治理增量，不擅自回滚。
共享最终B6、refill9、Goal11 92、Goal12 80、fullSelfCheck及双build均未运行；没有既有测试失败证据，因为该轮未启动。validator和Scene/工作树审计作为阻塞交付留痕单独执行，不代表运行验收通过。
下一步需要用户复核并明确C25负held lifecycle gate是否另包授权；在放行前禁止修复或进入包2。此Record不是新的C25实施授权。
停止交付审计：Tools/Validate-ChangeLedger.ps1 PASS，441 records / 380 governed code files；无关历史warning保留。起始1493→当前1497条status，新增test/meta/Task/Record四项。逐文件SHA对比本轮基线仅11条授权路径变化，详见Temp/Goal13_Stop_WorktreeAudit.json；生产diff见Temp/Goal13_Pkg1_Production.diff。Scene SHA D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11。未提交、未push、未回滚。


## Goal13b依赖解除 / correction
用户已授权守卫包 [NTSD28-B6-HELD-NEGATIVE-FRAME-LIFECYCLE-GUARD-PRODUCTION-001](NTSD28-B6-HELD-NEGATIVE-FRAME-LIFECYCLE-GUARD-PRODUCTION-001.md)，该包17/17及双Play通过。原BLOCKED事实保留，当前升级VERIFIED_MISSING_ACTION_SUBSET：原72复跑job7e54c1a44f4c4907a529af06ee8379dd 72/72 PASS，随后第二轮双Play PASS，Temp/Goal13b_Pkg1_FinalPlay.result.json，Sakura/Sakon均Free0/live/link/next true。生产没有追加改动。依赖守卫包，不将自身四文件宣称独立闭合。Goal13b三包共享最终回归将在包2完成后执行，当前批次尚未交付。


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
