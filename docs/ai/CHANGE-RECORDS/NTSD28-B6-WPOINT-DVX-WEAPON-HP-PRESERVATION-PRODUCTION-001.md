# NTSD28-B6-WPOINT-DVX-WEAPON-HP-PRESERVATION-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B6-WPOINT-DVX-WEAPON-HP-PRESERVATION-PRODUCTION-001
status: VERIFIED
change-kind: TEST_FIRST_DVX_WEAPON_HP_PRESERVE
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6WpointDvxWeaponHpPreservationProductionEditorTests.cs
authority: Goal13及Goal13b用户授权；playable battle_world.cpp:8025-8063
evidence: VERIFIED_NONKIND3_DVX_HP_SUBSET / RED8_FAIL_16_CONTROL_PASS / FOCUSED24_PASS / DAMAGE_PICKUP_THROW_PLAY_200_199_199_199 / KIND3_CALLBACK_RETAINED / B6_388_PASS / SELFCHECK_PASS / BUILDS0 / SCENE_UNCHANGED
-->

# NTSD28-B6-WPOINT-DVX-WEAPON-HP-PRESERVATION-PRODUCTION-001 — Task Contract

Goal13b第3步，用户复用原Goal13包2授权。IN_PROGRESS / TEST_FIRST，先记录后改脚本。
Authority当前正式B1E13AE1 EXE对应playable BattleWorld28::settle_held_refill_objects battle_world.cpp:8025-8063 DVX分支只写frame/motion/relation/+2F8，不写weapon_hp。Unity ThrowHeldWeapon→OnThrownInternal→LF2Weapon.OnThrown多写definition weapon_hp。

## 范围与不变量
- Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs仅ThrowHeldWeapon末尾：non-kind3跳过OnThrownInternal，kind3仍调用；共享API及LF2Weapon.OnThrown不删不改。
- Assets/NTSD/Scripts/Test/Editor/NTSD28B6WpointDvxWeaponHpPreservationProductionEditorTests.cs及meta，新focused与scoped Play。
- 本Task/同ID Record、Ledger/STATE/对齐总表、Temp产物。
真实type1/2/4/6 nonkind3投掷保留损伤sentinel；RNG/frame/motion/relation/ReleaseTick与原来一致。generic本来无callback保持原样，heavy type2 RNG迁移不属本包。kind3 overlap仍重置definition HP并保留既有native prefix/tail。refill耗尽仍优先。
不触碰+2F8 carrier/writer/consumer、联合schema、Scene/InputActions、资源、terminal/kind3算法、C25守卫或旧tests。

## test-first / 验收
RED real四type×左右×nonkind3/kind3 overlap；sentinel7不同于definition31，断言保留/重置分别正确且callback次数、精确RNG和frame/motion/relation/ReleaseTick不变。generic四type为控制，refill122/123耗尽/未耗尽为控制。
scoped Play current-DAT weapon先经生产damage writer损伤，再生产pickup consumer拾取，再真实driver触发当前nonkind3 DVX动作；记录weaponHP三边界与清理。动作/碰撞夹具注入须如实说明，不宣称物理输入。
完成后三包共享一次：guard17、pkg1 72、pkg2、B6(275+全部新增)、refill9、kind3 92、terminal80、fullSelfCheck、双build0error、validator、SceneSHA D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11。Editor b1b02287/2022.3.62f3/NTSD_Battle，禁止第二实例。

## 风险、生命周期、回滚
callback目前只重置WeaponFlightCounter，不能机械删API，kind3仍依赖；检查callback observer证明分支区别。无新runtime manager/queue，test finally回收owned handles/observer，普通有序退出。
任何旧测试/控制组失败、清单外diff、Scene变化、需改kind3/terminal或删除callback均立即停止。不扩范围。
回滚必须用户批准，仅本包备份增量和新增fixture/治理，不回退守卫或包1。
首次RED job25c6b33f70e74235bfe731a3a710ac94 completed24，8个nonkind3 real callback预期0实际1失败，其余16控制无失败。该job仍使用调整HP断言顺序前的程序集（调整时Unity reload拒绝refresh）；保留证据，显式refresh后重跑取得HP31→expected7的直接RED，生产未改。
直接HP RED job2f01cb6ca12d49ef89a1239421dba0f0 completed24，8个real nonkind3实测HP expected7 actual31失败，其余16控制无失败、未capped。生产仅resolver ThrowHeldWeapon末尾if(wpoint.Kind==3)调用OnThrownInternal；共享API、LF2Weapon callback及generic完全未改。同新test加入scoped Play：current Gaara16/254与Kunai120，生产damage injury1→重置位置/ground64夹具→current Gaara60 ITR2 pickup consumer→真实driver tick，Act override只观察base前后HP不改结果。待编译focused/Play。
Play probe首次编译有1个不存在的pickup方法引用，已仅修新probe为Goal11已有生产入口new LF2CharacterInteractionResolver(holder).TryApplyPreInteraction(pickup, child)。不改production接口或旧测试。
focused GREEN job3d9814cd044d411d84bb65c31b26693a 24/24 PASS，8个HP保留RED转绿、kind3/refill/generic16控制保持；ProbeCompile0error/104warnings。下一scoped Play。
scoped Play PASS，Temp/Goal13b_Pkg2_Play.result.json：current Gaara16/254与Kunai120，definitionHP200，经ApplyWeaponDamage injury1降199，实际pickup consumer后199，真实driver tick6/Act前后199、整tick199；frame40，V100/-1/0，link0，callback0；World4→4、logic2→2、renderer2→2。现在三包共享最终回归；无新增production修改。


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
