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

## 2026-09-10 用户追加授权并恢复（覆盖此前范围阻塞）
用户明确批准应用Temp/Goal16_ProposedAdditionalSelfCheckExpectations.diff；另批准有界类规则：仅本包授权文件内直接与P2/Authority矛盾的counter保持、HolderCopy镜像、PickupCount无条件递增、kind7拾取副作用四类既有期望可修。每处必须记录原行/旧值/新值/理由，不修改同断言其它语义。类外冲突及P2缺陷继续硬停。本包恢复IN_PROGRESS / TEST_FIRST；先两处已审阅期望+新focused RED，再生产接线。此前未实施历史保留。


最终状态：VERIFIED / REVIEW_HOLD，2026-09-10。140 focused、583 B6、refill9、旧pickup2、full SelfCheck、双scoped Play与C++同tick6记录117字段对照均PASS；双build0error、Scene固定SHA、保护文件与授权范围检查PASS。完整证据及8个旧期望站点见同ID Change Record最终关闭节。完成本P3后停止，等待用户复核；不自动开启其他包。
