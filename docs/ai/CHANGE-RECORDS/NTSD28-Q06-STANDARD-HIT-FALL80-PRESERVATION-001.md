<!-- CHANGE-RECORD
id: NTSD28-Q06-STANDARD-HIT-FALL80-PRESERVATION-001
status: VERIFIED
change-kind: STANDARD_CHARACTER_HIT_FALL80_SOURCE_WITNESS_FIRST
code-path: Tools/NTSD28AuthorityTrace/standard_hit_fall80_preservation_witness.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06StandardHitFall80PreservationEditorTests.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Current playable resolve_unarmored_reaction, resolve_confirmed_unarmored_hit and advance_reaction_timers_slot.
evidence: Effect16 post-fix case15 before0 immediate/following hitReactionTimer0 expected80, independent read-only audit confirms extra Unity clear and matching HitPlan projection.
-->

# NTSD28-Q06-STANDARD-HIT-FALL80-PRESERVATION-001

IN_PROGRESS / SOURCE_WITNESS_FIRST。独立于effect frame access：其四访问点修后16+6 before0/即时1/后继1，仅case15 hitReactionTimer0 expected80。当前正式resolve_unarmored_reaction生成80，resolve_confirmed_unarmored_hit写回并在尾部保留；advance_reaction_timers_slot按body/hold条件后继递减，case15 hold-3→-2时应仍80。source抓取关系另有清0，不能混用。

Unity准确差异：BattleDamageWriter.ApplyStandardCharacterDamage尾部if Fall==80 SetFall(0)；BattleEcsHitExecutionPlan.ProjectStandardCharacterDamageWriterEffect复制相同clear。type3投影另有类似表达式，不属于本包。旧SelfCheck.CheckStandardCharacterDamageAlignmentContracts的lethal/空中重反应oracle期待0是Unity自洽，不能覆盖新source证据。

当前第一写域仅Tools/NTSD28AuthorityTrace/standard_hit_fall80_preservation_witness.cpp。3代表：lethal hp1/fall0、nonlethal hp400/fall79、normal hp400/fall0，injury5/fall1、effect0/kind0、ground且显式正常反应帧180/186/220。实际geometric→resolve_ordinary_unarmored_standard_hit及following完整driver，输出完整两实体raw/descriptor/pending/HP/RNG与before/after/following。不同分支不能伪造after，源derived witness不等于正式EXE录制。

source完成后再精准声明Unityfixture、生产两符号和必要oracle断言，先RED，按3代表与相关旧回归验证，不重做B5全域。保护已有伤害/资源/accounting/音效/候选/帧访问及type3/C30计数职责；不改Kernel/schema/Scene/资源/非战斗。无新runtime模块/关闭阶段。回滚仅本ID差异并保留其余改动。父effect包等待该独立出口，Q06未完/Q07未迁移。

源3 build/双跑一致SHAcc6686dddc87fbace208641839924fbba205e399e0bfb89847ae5ebd076527b3，实际after/following timer80/80/20，HP-4/395/395，hold-3→-2，focused-fall-model.json只核对这些固定输出不冒充全模型。准确新增Unityfixture NTSD28Q06StandardHitFall80PreservationEditorTests.cs：主3源完整比较，另3代表经过真实candidate/PostInteractionTickAll，各ShadowCompare/DataOriented验证targetHP/Fall和observer mismatch；生产未改，先RED。

生产前准确扩域：仅ApplyStandardCharacterDamage尾部移除Fall80→0、ProjectStandardCharacterDamageWriterEffect对应TargetFall80→0；不改type3/C30/catch关系清零。SelfCheck.CheckStandardCharacterDamageAlignmentContracts中lethal与airborne重反应两oracle更新80及消息，其他assert保持。源case15及source3、Unity直接RED两80证明；原fixture DataOriented观察计数只应ShadowCompare非零，已修测试前置；新Shadow失败mask1<<46属于独立pendingY(12 vs10)预测问题，不掩盖、不算本项通过。旧SelfCheck0来自自洽断言，无独立source依据，新source和readonly审计要求改为80。回滚仅本Task具体2clear/2oracle，保留effect4点、impact等已有改动。


2026-09-21 联合出口：job d41e706b23d048448436ee57d9f2f0c9 30/30 PASS，实际1.777秒；effect16+smoke6 before/即时/following零差异，标准source3及vertical2真实candidate Shadow/DataOriented通过。完整SelfCheck 01:55:20 UTC PASS已存joint-pass。Ledger603/28 PASS，diffcheck通过。仍IN_PROGRESS，尚待代表Play/回放。
测试扩域预声明（生产不再修改）：既有Fall80 fixture RunCapturedHitPlan改internal并增加可选renderer参数；CreateWorld/Shutdown使用该参数，并检查对象池borrower恢复。供Effect同文件Play probe复用default3+vertical2，在ShadowCompare/renderer和DataOriented/logic两条代表路径验证，共10例；保留原EditMode调用不变。无新逻辑/资源/Scene修改，不据此证明跨World恢复。

测试扩域已实现：Effect fixture新增Authority6 replay与EffectPlay22 probe；Fall80 RunCapturedHitPlan可选renderer并逐world断言borrowers恢复。当前仅刷新编译，新增replay/Play尚未通过；不重跑既已通过完整SelfCheck。

真实Play初次12个effect代表通过；captured helper在NUnit TestContext.WriteLine处因Play没有测试context抛NullReference（两条路径）。Scene checksum/borrowers2→2保持，关闭01:58:24Z PASS。失败已存effect/representative-play-initial-test-context-failure。精确修复同测试helper日志：只在非Play时调用TestContext.WriteLine，所有assert/生产逻辑保持。修后仅重跑此Play，不重做完整SelfCheck/旧矩阵。


2026-09-21 限定VERIFIED：Standard unarmored actual and projected timer80 retained, two old SelfCheck expectations corrected from current-source evidence. Source3 SHA cc6686dddc87fbace208641839924fbba205e399e0bfb89847ae5ebd076527b3. Catch/type3 resets unchanged.
联合30/30、SelfCheck01:55:20Z、代表回放6/12ticks及Play22/关闭02:00:04Z PASS；Scene hash/dirtyfalse/root14保持。完整证据与限制见artifacts/diagnostics/NTSD28-Q06-STANDARD-HIT-FALL80-PRESERVATION-001/ACCEPTANCE.md。上文未运行/生产未改是历史检查点，由本条覆盖；不关闭整体Q06或Q07。
