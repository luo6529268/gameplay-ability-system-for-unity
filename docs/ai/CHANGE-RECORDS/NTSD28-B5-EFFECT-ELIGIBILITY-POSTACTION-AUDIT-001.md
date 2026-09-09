# NTSD28-B5-EFFECT-ELIGIBILITY-POSTACTION-AUDIT-001 — effect eligibility/post-action audit

<!-- CHANGE-RECORD
id: NTSD28-B5-EFFECT-ELIGIBILITY-POSTACTION-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY_AUTHORITY_UNITY_CROSSWALK
code-path: Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterHitResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
authority: NTSD 2.8-Logan battle_world.cpp direct effect filter/action override/kind0 post-effect/type3 post-hit live path; EXE B1E13AE1, closure 39DDDA15.
evidence: KIND16-RETIREMENT-VERIFIED / DIRECT-FILTER-2-4-20-21-30-ALIGNED / EFFECT8-16-OVERRIDE-MISSING / AUTHORITY-EFFECT8-RUNTIME-CORPUS-808 / TYPE0-DIRECT-POSTACTION-MISSING / TYPE3-PARTIAL-OWNER-ROUTED / EFFECT6500-UNKNOWN / TYPE0-HITPLAN-SCALE-FIRST-DIFFERENCE-FOUND / MANIFEST-COMPLETE
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / IMPLEMENTATION_SPLIT_DEFINED`

本包只建立effect语义及production owner拆分，不写战斗代码、不修改内容或Scene。

审计确认direct filter已对齐；effect8..16 action override与unarmored type0 direct post-action缺失；type3
continuation需独立核验。另发现type0 hit-plan伤害投影仍使用raw injury，先路由独立修正。详见manifest。
