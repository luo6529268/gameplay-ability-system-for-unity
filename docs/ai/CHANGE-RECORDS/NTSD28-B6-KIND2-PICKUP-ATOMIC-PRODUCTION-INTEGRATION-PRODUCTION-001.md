# NTSD28-B6-KIND2-PICKUP-ATOMIC-PRODUCTION-INTEGRATION-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B6-KIND2-PICKUP-ATOMIC-PRODUCTION-INTEGRATION-PRODUCTION-001
status: VERIFIED
change-kind: TEST_FIRST_PICKUP_ATOMIC_INTEGRATION
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleInteractionWriter.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterInteractionResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2SpecialAttack.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6Kind2PickupAtomicProductionIntegrationEditorTests.cs
authority: 用户P3授权；B1E13AE1 playable kind2 chain；P1/P2 VERIFIED
evidence: RED_126_FAIL_14_PASS / FOCUSED_140_OF_140 / B6_583_OF_583 / REFILL_9_OF_9 / OLD_PICKUP_2_OF_2 / SELFCHECK_PASS / SCOPED_PLAY_2_PASS / SOURCE_TRACE_6_RECORDS_117_FIELDS_EQUAL / BUILDS_0_ERROR / SCENE_UNCHANGED / USER_REVIEW_ACCEPTED
-->

# NTSD28-B6-KIND2-PICKUP-ATOMIC-PRODUCTION-INTEGRATION-PRODUCTION-001 — Task Contract
IN_PROGRESS / TEST_FIRST，2026-09-10用户明确P3单包授权；P1/P2 VERIFIED是前置。此Task/Record在任何本包脚本改动之前建立。
Authority：当前正式B1E13AE1 EXE对应playable battle_world.cpp:4521-4533、5270-5309、5310-5404，game_session.cpp:4173-4182；Goal14_Triage_Report.md §1.1为定位材料，最终以live源码闭包为准。

## 范围与原状
P1 writer已具名locked规则和条件计数；P2有三态immutable有序计划，但生产未接。角色HasHeldObject/ground门过严、Special复制writer、HitPlan重复且漏owner/tail、kind7有额外写入。仅允许以下code-path及新test.meta；既有用户P1/P2未提交修改保留，不能回退。
- Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleInteractionWriter.cs
- Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterInteractionResolver.cs
- Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatInteractionResolver.cs
- Assets/NTSD/Scripts/Animation/LF2Objects/LF2SpecialAttack.cs
- Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
- Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs
- Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs
- Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
- Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
- Assets/NTSD/Scripts/Test/Editor/NTSD28B6Kind2PickupAtomicProductionIntegrationEditorTests.cs
BruteForceSceneQuery仅collector→consumer闭合夹具必要支持，Attack KeyJump/PrevJump映射不变；Runner仅kind7 unsupported disposition/trace接线。BattleHeldObjectWriter预期无改动。P2计划文件/规则政策/schema不改。

## 实施合同与不变量
- actual/shared writer与shadow均消费现有BattlePickupTransactionPlan，不重复type/关系/count/tail规则；应用之前完成全部输入捕获和locked准入，顺序执行有限操作。区分applied与relationEstablished，仅后者更新HeldWeaponReferenceInternal缓存。
- kind7保留候选分类但消费unsupported，零关系/count/action/tail副作用。
- state2004候选允许替换，消费不得再以HasHeldObject或动态ground拒绝；旧child维持既有invalid-preserve，不Free、不unlink，不做跨实体清理。
- 精确owner=attacker physical slot，不是HolderCopy/owner链；现有link/cache仅在建立关系时更新。不新增持久化字段。
- candidate身份/special-hit-latch/effect/phase/rest/Attack边沿既有门保持；same-tick多候选遵循已捕获序列。
- SelfCheck仅用户列明14590-14652/18245-18263旧kind7及计数期望；HitPlanTests仅5780-5800旧kind7期望，实际修订行和理由追加，不扩改其他旧测试。
- 无新manager/queue/worker/pool，仅现有writer事务，十一阶段shutdown不变。

## 验收
先新focused RED，再生产。P2全矩阵覆盖real/generic与两消费模式、kind7零写、tail-only不写reference、same-tick替换保旧child、候选门控制。两scoped Play选当前117-tuple域OID120与type2/state2004，真实Attack边沿collector→writer→held，同seed/input/tick Authority trace，保存JSON/freeEvents/relation/count/owner/tail。Play为生产输入链而非物理键盘。
共享B6(443+新增)，本focused及前置92/80/17/72/24/23/32、refill9、fresh fullSelfCheck；双build0error、validator、Scene SHA D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11/dirtyfalse。仅现有b1b02287 Unity2022.3.62f3/NTSD_Battle。

## 硬停止与回滚
P2语义缺陷、需要old-child Free/unlink/schema/Attack映射变更、任一既有指定测试失败、diff超授权或Scene变化立即停止报告。不为绿修改未授权夹具。回滚须用户明确批准，仅反向本包增量，保留Goal15基线；基线/各原文件备份位于Temp/Goal16_*。无commit/push。

## 2026-09-10 事前范围阻塞：未修改生产脚本
BLOCKED / PRECHANGE_SCOPE_CONFLICT / PRODUCTION_UNCHANGED_FROM_GOAL15。
实际发现：
1. BattleRuntimeSelfCheck.cs:14763-14775（CheckActualCharacterCurrentDatPickupShells）通过正式collector和PostInteractionTickAll消费type3候选，却在14770明确要求AttackingCounter保留7，14775声明完全拒绝。P2纯计划与当前用户要求都规定tail-only applied、counter0。这是确定的旧期望冲突；不是已运行的新失败，未发现P2语义缺陷。
2. 同文件14851-14865（CheckActualCharacterCurrentDatPickupShell）在14859要求写target.HolderCopySlot=pickerSlot；P2只有SetTargetOwnerSlot，没有HolderCopy操作，Authority5364-5376只写physical owner。当前runtime默认HolderCopySlotIndex=99（NTSDEntityRuntime.cs:231/1113），该夹具未覆盖默认值。此旧镜像期望也需要显式修订。
上述区域不在本轮授权SelfCheck14590-14652/18245-18263旧期望修订行内。不修改清单外断言，不保留额外写以迎合旧绿，不故意实施后制造既有SelfCheck失败。
已准备Temp/Goal16_ProposedAdditionalSelfCheckExpectations.diff，仅提案未应用：14770改0并纠正14775文字；14859改exact OwnerSlot检查并保留HolderCopy99控制。需用户明确追加这两处范围后恢复本包。
已建Task/Record/Ledger/STATE/对齐入口并冻结Goal15基线；P3 RED、production diff、旧期望实际修订、focused、Authority执行trace、scopedPlay、共享回归和双build均未运行。只读核验Unity b1b02287/2022.3.62f3/NTSD_Battle dirtyfalse/root13/Scene固定SHA；P1/P2和schema哈希保持。暂停两子代理，未落地新测试或生产脚本。这里是事前合同冲突，不把静态预测称为实测测试失败。

事前阻塞交付检查：Tools/Validate-ChangeLedger.ps1 PASS（446 records、4 governed code diff均为继承Goal15修改，本Goal脚本增量0）；git diff --check PASS。Temp/Goal16_ScopeIntegrity.json确认37个P2/schema/Scene保护文件哈希不变，新P3测试文件0。

## 2026-09-10 用户追加授权并恢复（覆盖此前范围阻塞）
用户明确批准应用Temp/Goal16_ProposedAdditionalSelfCheckExpectations.diff；另批准有界类规则：仅本包授权文件内直接与P2/Authority矛盾的counter保持、HolderCopy镜像、PickupCount无条件递增、kind7拾取副作用四类既有期望可修。每处必须记录原行/旧值/新值/理由，不修改同断言其它语义。类外冲突及P2缺陷继续硬停。本包恢复IN_PROGRESS / TEST_FIRST；先两处已审阅期望+新focused RED，再生产接线。此前未实施历史保留。

已应用追加授权站点S01（原14770/14775）：counter7→0，说明从完全拒绝改tail-only applied；其余同断言条件不变。S02（原14859，新14859-14860）：HolderCopy==pickerSlot→OwnerSlotIndex==pickerSlot并HolderCopy==99；原因Authority5364-5376精确physical owner且无镜像写，原runtime默认99。当前仅测试期望改动，生产未动，新RED待执行。

新focused已由Luna编写并主线程审阅：初稿99声明实际以Runner计数为准；修正新测试enum public参数可见性、KeyJump byte赋值、FrameWaitCounter应保留17(不是P2清零对象)、Special使用ObjectInteraction pass，以及current-DAT type/OID synthetic夹具。增加dead6 HP-1、Play probe与仅请求文件启用的Editor NUnit XML结果捕获。测试初次Unity编译2处byte错误已局部修复，最终Editor TestCompile0error/104warnings。生产仍未接线，进入真实RED。测试辅助无battle runtime manager或关闭依赖。

首轮RED job3c4663f0b00a4066b1dcb56686413ef4实测104项/90FAIL/14PASS，完整XML Temp/Goal16_RED_UnityTests.xml：6 kind7、50 owner、4 relation101、4旧relation计数、2same-tick link、24tail-counter失败；所有candidate gate/collector控制通过。随后在生产未改时补入24 supported WP literal与12采集后missing-current-frame案例，unsupported常规矩阵改为type3（latch控制仍显式type0），state2004旧child起始改为valid reciprocal，待扩展RED。新测试自审纠正不涉及P2或旧既有测试。

扩展RED job6d71e0313cba49639d258e665c7c0a37：140实际执行、126FAIL/14PASS，完整Temp/Goal16_Expanded_RED_UnityTests.xml。全部控制仍PASS，失败新增supported WP literal与采集后missing frame/valid state2004替换。P2文件未改；现在进入同包生产接线。

## 旧期望修订站点（原行号以Temp/Goal16_Before文件为准）
| 站点 | 文件/原行 | 旧值/含义 | 新值/含义 | Authority与理由 |
|---|---|---|---|---|
| S01 | SelfCheck14770/14775 | counter7，unsupported type3完全拒绝 | counter0，tail-only applied | kind2公共tail5380起；用户追加明确批准 |
| S02 | SelfCheck14859 | HolderCopy=pickerSlot | OwnerSlotIndex=pickerSlot且HolderCopy99 | 5364-5376 physical owner，无mirror写；用户追加明确批准 |
| S03 | SelfCheck14610-14615 | kind7 true、1/-1、held slot、count1 | false、0/0、held-1、count0 | kind7无pickup consumer分支；原范围批准 |
| S04 | SelfCheck14627-14634 | current OID120 kind7 true、101/-1、links/count1 | false、0/0、links-1/count0 | OID120只提升kind2；kind7 unsupported |
| S05 | SelfCheck14646-14653 | CLR OID120 kind7 true、1/-1、links/count1 | false、0/0、links-1/count0 | CLR OID不能启用已退休kind7 |
| S06 | SelfCheck18251-18253 | unsupported type Special pickup返回false/count1 | 返回true/count仍1 | 公共tail applied，不建立关系，不加count；原范围批准 |
| S07 | SelfCheck18260-18264 | Special kind7 true、4/-4、parent、count1 | false、0/0、parent-1/count0 | kind7副作用退休 |
| S08 | HitPlanTests5775/5791-5799 | kind7 observed writer1、links/HolderCopy写入、count2→3 | writer0、links初始0/-1、HolderCopy99、count2保持；方法名明确Unsupported | same runner+shadow disposition应为Unsupported，无writer观测 |
上述仅四类直接矛盾期望与对应说明/方法名修订；不修改同断言的HP/数值/动作等其它语义。新test通过P2既有计划定义预期，P2不改。

生产映射：TryApplyPickup与ProjectPickupWriterEffect各自捕获当前或projection输入后共同消费BattlePickupTransactionPlan；只把其有限operation映射到已有canonical字段，relationEstablished才更新real角色引用缓存。Character消费者去除多余held/ground再拒绝；Special复制事务退休；Generic本来已走shared writer，无需额外diff。Runner仅把kind7消费disposition设Unsupported，raw候选分类与BruteForce Attack映射不改。旧child没有任何清理操作。

生产接线脚本已实际应用（5个生产文件：shared writer/character consumer/special consumer/HitPlan/runner），S03-S08旧期望已应用，Generic resolver与BruteForce生产无改。ProductionCompile Editor0error/129warnings；当前CODE_WRITTEN等待真实focused GREEN。

首次GREEN job2bbec7c02c084018806a6b7f18cc452f：132PASS/8FAIL，均为新增generic/special missing-current-frame夹具。查明手工candidate-cache注入漏带正式PairSnapshot；target改为missing后factory live fallback返回default，既有group gate合法拒绝，尚未到writer。真实collector的对应4项已PASS。仅修新fixture：注入前调用既有BattleHitCandidatePairSnapshotFactory.Capture并assertValid，随SceneQueryHit携带冻结pair，不改任何production gate或P2；重新跑140。

修正冻结候选fixture后GREEN job5b7ac14a01124fd288431395f59947a5：140/140 PASS（完整XML Temp/Goal16_GREEN_FixedFixture_UnityTests.xml）。当前production focused通过，开始两类scoped Play，随后共享回归。Authority第一版release-DAT trace为诊断PASS但内容不同，不能作为equal；另补same normalized fixture trace。

Play首尝试字段已观察OID120 relation101/-1,count5→6,owner50,action115/counter0,heldaction24,free0,cleanup通过；probe因在C20 Act读取已EndCollisionCandidateConsumption回收的候选range而FAIL（late observation，非pickup字段失败）。保存Temp/Goal16_Play_attempt1_late_candidate_read.json。仅修新probe：在ObservedWeapon既有virtual current-type reader的消费阶段只读捕获尚有效候选range，返回base type不改变分类；保留Act记录settled/held，不改生产。

Play第二次OID120完整PASS（candidate1、Attack1/0、101/-1,count5→6,owner50,tail0,held24,free0）。type2 fixture未进入拾取，实际new weapon HP800→740、frame20→2：旧heavy在Gaara60无WPoint的C09进入action0，产生其自带injury60普通碰撞，污染新目标。不是既有测试失败或P2计划缺陷；不改该攻击规则，改从同117-tuple域选Kakuzu25/frame250（kind2、无OPoint/CPoint、WP21/y-999使旧heavy远离新目标）。同时每witness先None tick再Attack tick，避免复用同player slot的上一witness按键持续。另增加坐标及before-consumption字段供同输入native比较。

口径补充：上条Gaara重武器干扰的damage-source归因是由old C09 frame0、其ITR injury60、new HP-60及group3变化推断，未捕获伤害source slot，不能作为独立生产缺陷结论。Kakuzu250的选择直接来自同一冻结117-tuple域，不修改DAT/规则。

Play第三尝试加入None预热tick后首个witness未形成候选，所有pickup字段保持初值；未证明其具体输入门原因，不归为P2缺陷。保留attempt3证据，改每个witness使用各自未占用的独立human player slot（2/3）并以首tick的明确Attack packet进入，保持每实体native input history清零；新增collection-end只读诊断以直接记录KeyJump/PrevJump与候选数。未修改Input映射/门。只读代码审查另发现HitPlan缺Wrapper时OID fallback与actual不一致；在授权HitPlan输入映射改为同一TargetObjectId fallback，P2不变；正常loaded wrapper路径已140PASS，最终共享再验。

Play第四次collection-end观测闭合：第一类PASS，第二类tick7的collectionAttackCurrent/Previous=0/0，candidate0，target保持HP800/frame20，已排除该次普通碰撞干扰。只读查明既有正式输入采样相位：SimulationWorld.ShouldSampleHumanCurrentInput仅OneTuInput或InputPhase0；AdvanceBattleFlowTick按(nextPhase+1)&1切换，与Authority battle_world.cpp:1393及tick_driver:385/504一致。第二case位于非采样相位，故probe改在创建fixture前仅推进所需非采样空tick，使实际Attack落在下一InputPhase0；不改输入映射、相位或生产逻辑。此前对InputDelay/缓存原因的猜测不作为结论。

旧期望完整增量审计已执行Temp/Goal16_expectation_audit.py：15个连续diff块全部映射S01-S08，未出现四类以外或同断言其它语义修改；精确旧/新行号及文本保存Temp/Goal16_ExpectationSites.json。

## 最终Unity验收与范围检查（2026-09-10，Authority同输入trace仍在收口）
Play第5次实际PASS：Temp/Goal16_Play_Final.json。Gaara16/frame60→OID120/frame64于tick6，seed424242，native Attack1/0、InputPhase0、候选1；relation101/-1,count5→6,owner50,holderaction115/counter0，C20 targetaction24/position213,7,288，freeEvents0。Kakuzu25/frame250→OID150/frame20于tick8（tick7实际空推进至采样相位），旧child51的2/-2被新child52替换为2/-2；count5保持，owner50,action116/counter0，C20 action10/position203,-1,288；旧child在C09合法follow后与tick结束完全同值，invalidPreserveEvents1/freeEvents0。两例cleanup objects4/logic2/render2恢复同值。使用临时fixture与FrameInputSet，非物理键盘；不把frame-delay admission fixture称为完整自然角色招式复现。
实际共享B6 job77699144e05a4ea1bb88c25d62cc1759：583/583 PASS，其中本包140，Goal11 kind3=92，Goal12 terminal=80，Goal13b guard/missing/DVX=17/72/24，Goal15 P1/P2=23/32均在同次执行。refill jobab6d56f5b21847f4a4a57fc130df97ae为9/9 PASS；旧HitPlan kind2/kind7 joba2f3f96a6bb842768e79b9805db6fbf4为2/2 PASS。各完整XML见Temp/Goal16_Final_*_UnityTests.xml，无既有测试失败。
通过Temp/NTSD_BattleRuntimeSelfCheck.request触发真实Editor完整SelfCheck，04:53:42Z新结果PASS（Temp/Goal16_Final_SelfCheck.result）。最终Console读取7条均为既有负向registration/release/rest绑定夹具日志，compiler errors0，本次未出现MinMaxAABB；不声明Console0，亦不把历史MinMaxAABB判为已修复。
实际命令dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly：0error/22warnings；Assembly-CSharp-Editor.csproj同参数：0error/104warnings。Unity2022.3.62f3、NTSD_Battle、isDirtyfalse/root13；Scene SHA仍D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11。
Temp/Goal16_Final_Integrity.json：37个P2/schema/Scene保护哈希不变；11个前置包代码/meta/Task/Record及DECISIONS原值保持；scopeDrift空。新增P3增量为5个生产脚本、两个旧测试期望文件、新focused测试及meta、Task/Record和Ledger/STATE/对齐总表。Generic resolver及BruteForce无需改动；无old-child清理、P2计划、schema或Attack映射变更。git diff --check PASS。
当前Unity验收已闭合；C++同tick执行已实现，但第一版host trace仍使用不同HP/cache/position与frame-delay前提，不能直接称equal。正在仅Temp harness内校准fixture输入，不修改Authority源码或Unity生产。

最终旧期望行号校对：S01 14770/14775→同号；S02 14859→14859-14860；S03 14610-14615→同号；S04 14627-14634→同号；S05 14646-14653→同号；S06 18251-18253→18252-18254；S07 18260-18264→18261-18265；S08 5775/5791-5800→同号（包含方法名和对应说明）。S01-S07均BattleRuntimeSelfCheck.cs，S08为Editor/BattleHitExecutionPlanEditorTests.cs。15个连续diff块的完整旧/新文本以Temp/Goal16_ExpectationSites.json为准，上表旧/新职责与理由不变。


## 最终关闭证据（2026-09-10，以本节为当前结论）
VERIFIED / P3_ATOMIC_PICKUP_LIMITED_SCOPE / REVIEW_HOLD。已完成批准的P3单一行为包；下文不授权启动B6后继或联合schema迁移，不表示整个B6/战斗域完全对齐。

Authority同seed/input/tick trace已闭合：Temp/Goal16_Authority_Unity_Comparison.json记录两个witness、3个边界共6条记录，18个共同字段/记录，加旧child9字段，共117个标量比较全部一致，firstDifference=null。seed424242、native Attack rising、InputPhase0与实际SimulationTickDriver tick6/8一致；使用同一个source World/driver真实空推进5/7步后建立fixture，未伪写tick/phase。两case均沿真实C++ coordinator执行candidate→relation→held→pending lifecycle，另用直接phase API记录pickup后/held前中间边界。Unity走真实NTSD_Battle中的driver/collector/writer/C20；ShadowCompare/DataOriented由140 focused覆盖。无candidate注入或物理键盘声明。

最终trace校准依据：Authority battle_world.cpp:1808-1811的motion_hold_timer(+0x0B4)非零时逐步趋零并跳过物理；8578起跳过ordinary frame mutation，7991 child从parent复制，与Unity LF2Entity.TryEnterReleaseFrameAdvanceAfterDelay及FrameDelay motion gate相应。最终source fixture把holder/target/old motion_hold_timer设为1000，对齐Unity FrameDelay1000；holderHP500、target/oldHP100、target cache200/800、旧childcache800/初始frame20/counter0/x1000，holder/target位置200,0,287完全按实际Unity fixture。先前遗漏hold导致的host x+1/counter1是输入不一致，已纠正；“native无等价delay字段”的临时判断不成立，不登记为生产差异。仅Temp harness改动，Unity/Authority生产均无新增改动。

结果：OID120 tick6 after relation101/-1,count6,owner50,group3,holderaction115/counter0,targetaction24/pos213,7,288；OID150替换tick8 after2/-2,count5,owner50,group3,action116/counter0,targetaction10/pos203,-1,288。旧child51经C09后与tick尾的frame21/relation-2/parent50/owner19/group3/cache800/pos223,-1021,286一致，preserve1且free0。两端未发生pending lifecycle despawn。

trace证据边界：C++ harness编译当前playable live源码、使用冻结Direction-B normalized projection构造临时DatDocument，不是启动正式EXE。正式EXE B1E13AE1指纹已核验，但不声称完整GameSession、正式release DAT内容、整个World checksum或逐像素parity。18个共同字段中，只有relation=0时将native dormant link0归一为Unity-1；活动physical link不归一、不忽略差异。Unity独有HolderCopy99单独保持检查。中间边界为直接phase观测，最终边界来自实际完整driver tick。完整旧child/SCHEMA或其他战斗家族不在本包关闭范围。

最终证据索引：
- Temp/Goal16_Expanded_RED_UnityTests.xml：140项，126FAIL/14控制PASS。
- Temp/Goal16_Final_B6_UnityTests.xml：583PASS，包括P3 140及所有指定前置focused。
- Temp/Goal16_Final_Refill_UnityTests.xml：9PASS；Temp/Goal16_Final_OldPickup_UnityTests.xml：2PASS。
- Temp/Goal16_Final_SelfCheck.result：完整PASS；Temp/Goal16_Final_Console.json：7个既有负向夹具日志，compiler0。
- Temp/Goal16_Play_oid120_type1.json、Temp/Goal16_Play_type2_state2004_replacement.json、Temp/Goal16_Play_Final.json：双witness及cleanup。
- Temp/Goal16_AuthorityTrace_Normalized_Tick6.json、Temp/Goal16_AuthorityTrace_Normalized_Tick8.json、Temp/Goal16_Authority_Unity_Comparison.json：同tick源码对照。
- Temp/Goal16_ProductionIncremental.diff：相对Goal15基线的5文件P3生产增量；不混入既有P1规则变更。
- Temp/Goal16_ExpectationSites.json：8站点15个连续diff块的全部旧/新行号与文本；上表逐项理由。
- Temp/Goal16_Final_RuntimeBuild.txt、Temp/Goal16_Final_EditorBuild.txt：实际双build0error，22/104warnings。
- Temp/Goal16_Final_Integrity.json：37保护文件及11前置包文件不变、无scope drift、Scene SHA不变。

复现命令：现有Unity b1b02287的run_tests EditMode categoryNames=[NTSD28_B6]；两refill groupNames与两旧pickup testNames如共享节job记录；full SelfCheck通过既有Temp请求。C++执行Temp/Goal16_AuthorityTrace.ps1 -FixtureMode direction-b-normalized -ObservationTick 6 -NormalizedOutputPath Temp/Goal16_AuthorityTrace_Normalized_Tick6.json，再-SkipBuild/-ObservationTick8写Tick8；比较脚本Temp/Goal16_compare_traces.py。Unity运行时脚本未再变化，因此不重复已通过的共享测试。
本包未git add/commit/push，未变更Scene/资源/Input/schema/P2计划、未清理用户工作树；回滚须用户明确批准，按P3增量反向并保留Goal15基线。报告后停下等待用户复核。

最终交付门：Tools/Validate-ChangeLedger.ps1 已实际PASS（446 records、11 governed code diff，含继承的P1/P2）；Temp/Goal16_Final_Validator.txt保存完整结果，历史记录声明非当前diff路径的warning未扩展清理。最终git diff --check PASS，Temp/Goal16_Final_Integrity.json复核PASS。C++最终同一capture binary SHA4996C5F6358E87E30FBE83F0C2082F1C02ED620A358F40038EB6177847050CE5；Tick6 trace SHA5ED271C14940F76DABE67F8382BFB19F8D64971F1E201C769796C71740D5563A，Tick8 SHA5323CA0DEFF12075B27B2544C06CBD2A06275681FCABB8DF518C67E551956B24。此前native无hold的临时输出已被最终同输入输出取代，不能继承其旧hash/差异结论。


2026-09-10用户复核确认：本P3的REVIEW_HOLD已解除，当前状态VERIFIED / USER_REVIEW_ACCEPTED。既有验证证据和历史等待记录保留；本次仅状态同步，无P3脚本改动。
