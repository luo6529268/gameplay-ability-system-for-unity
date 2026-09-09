# NTSD28-B5-TYPE3-POST-HIT-ACTION-AUDIT-001 — type3 post-hit action audit

<!-- CHANGE-RECORD
id: NTSD28-B5-TYPE3-POST-HIT-ACTION-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY_AUTHORITY_UNITY_CROSSWALK
code-path: Assets/NTSD/Scripts/Animation/LF2FrameData.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
authority: NTSD 2.8-Logan battle_world.cpp type3 attacker post-hit and target continuation plus kind_catalog live path; EXE B1E13AE1, closure 39DDDA15.
evidence: ATTACKER-HIT_FJ-DVX-DIFFERENCE / AUTHORITY-STATE3000-902 / STATE3000-HIT_FJ-187 / TARGET-HIT_FJ-HIT_UJ-DIFFERENCE / OWNER-CONTROL-IMPULSE-CARRIERS-MISSING / KIND-CATALOG-GENERIC-PATH-MISSING / IMPLEMENTATION-SPLIT-DEFINED
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / IMPLEMENTATION_SPLIT_DEFINED`

本包只建立 type3 post-hit 的 authority/Unity crosswalk 与实施顺序，不写战斗代码、不修改内容或 Scene。

确认攻击者侧是可独立修复的现实差异；target generic continuation 依赖 owner/control/impulse 精确 carrier，
kind transform 依赖通用 kind catalog，不能继续沿用固定 20/30 和 OID 特判冒充完成。完整证据见
`docs/ai/MANIFESTS/NTSD28-B5-TYPE3-POST-HIT-ACTION.md`。

