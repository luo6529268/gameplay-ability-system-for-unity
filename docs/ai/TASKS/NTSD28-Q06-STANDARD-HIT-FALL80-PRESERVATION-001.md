> 2026-09-21 VERIFIED / DECLARED_SCOPE_ONLY。联合30/30、SelfCheck、代表replay与Play22/Q05关闭PASS。准确范围/未覆盖项见artifacts/diagnostics/NTSD28-Q06-STANDARD-HIT-FALL80-PRESERVATION-001/ACCEPTANCE.md。下文保留实施前计划，不代表当前尚未修改。

# NTSD28-Q06-STANDARD-HIT-FALL80-PRESERVATION-001

IN_PROGRESS / SOURCE_WITNESS_FIRST。独立于effect frame access：其四访问点修后16+6 before0/即时1/后继1，仅case15 hitReactionTimer0 expected80。当前正式resolve_unarmored_reaction生成80，resolve_confirmed_unarmored_hit写回并在尾部保留；advance_reaction_timers_slot按body/hold条件后继递减，case15 hold-3→-2时应仍80。source抓取关系另有清0，不能混用。

Unity准确差异：BattleDamageWriter.ApplyStandardCharacterDamage尾部if Fall==80 SetFall(0)；BattleEcsHitExecutionPlan.ProjectStandardCharacterDamageWriterEffect复制相同clear。type3投影另有类似表达式，不属于本包。旧SelfCheck.CheckStandardCharacterDamageAlignmentContracts的lethal/空中重反应oracle期待0是Unity自洽，不能覆盖新source证据。

当前第一写域仅Tools/NTSD28AuthorityTrace/standard_hit_fall80_preservation_witness.cpp。3代表：lethal hp1/fall0、nonlethal hp400/fall79、normal hp400/fall0，injury5/fall1、effect0/kind0、ground且显式正常反应帧180/186/220。实际geometric→resolve_ordinary_unarmored_standard_hit及following完整driver，输出完整两实体raw/descriptor/pending/HP/RNG与before/after/following。不同分支不能伪造after，源derived witness不等于正式EXE录制。

source完成后再精准声明Unityfixture、生产两符号和必要oracle断言，先RED，按3代表与相关旧回归验证，不重做B5全域。保护已有伤害/资源/accounting/音效/候选/帧访问及type3/C30计数职责；不改Kernel/schema/Scene/资源/非战斗。无新runtime模块/关闭阶段。回滚仅本ID差异并保留其余改动。父effect包等待该独立出口，Q06未完/Q07未迁移。
