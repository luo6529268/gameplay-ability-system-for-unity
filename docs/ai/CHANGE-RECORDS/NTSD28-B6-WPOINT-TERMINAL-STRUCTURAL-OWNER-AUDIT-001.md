# NTSD28-B6-WPOINT-TERMINAL-STRUCTURAL-OWNER-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B6-WPOINT-TERMINAL-STRUCTURAL-OWNER-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan BattleWorld28::settle_held_refill_objects terminal weapon_action and BattleWorld28::despawn/clear_entity_links; playable build.ps1 closure; Unity held writer/resolver, structural writer Free/Destroy and registry lifecycle; EXE B1E13AE1, closure 39DDDA15.
evidence: post-refill/pre-pose terminal order frozen; StructuralWriter.Free selected and Destroy rejected because weapon break audio is extra; transient terminal outcome and world consumer owners defined; corrected Direction-B full held union terminal33 across23 definitions all weaponact1000, with former28/22 retained as ITR-pickup subset; lifecycle cleanup runtime dependency and focused/Play/trace matrix frozen; no code/content/Scene changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / FREE_NOT_DESTROY_OWNER / POST_REFILL_PRE_POSE_GATE / CURRENT_33_WITNESS / CORRECTED_RELATION_DOMAIN / PRODUCTION_HELD`

Authority terminal在refill/exhaustion之后、任何child frame/pose/DVX/kind3之前调用无音频的`despawn()`。
Unity必须以transient outcome把real/generic held writer的terminal请求交给
`SimulationQueryAndLinkModule`，再调用`BattleStructuralWriter.Free()`；`Destroy()`会触发真实武器破碎音效，
不是等价owner。完整ITR+OPoint held union有33条terminal WPoint、23个holder且全部weaponact1000；
原28/22保留为ITR-pickup子集。

production包必须等待`NTSD28-B6-ENTITY-LINK-LIFECYCLE-CLEANUP-PRODUCTION-001`取得Unity runtime绿灯；
否则free后仍会遗留关系/ABA。完整owner、C09/C20 trace与验收矩阵见
`docs/ai/TASKS/NTSD28-B6-WPOINT-TERMINAL-STRUCTURAL-OWNER-AUDIT-001.md`。本轮无code/content/Scene。

## RELATION-DOMAIN CORRECTION（2026-09-08）

`NTSD28-B6-HELD-RELATION-PRODUCER-DOMAIN-CORRECTION-AUDIT-001`证明OPoint kind2也是正式held producer；
Kyubi五条terminal因此current可达。owner与控制流不变，所有后续matrix使用33/23。
