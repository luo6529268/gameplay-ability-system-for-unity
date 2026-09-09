# NTSD28-B5-REMAINING-EXIT-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B5-REMAINING-EXIT-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan hit_candidates.cpp kind8 classifier and battle_world.cpp resolve_special_relation_hit; EXE B1E13AE1, closure 39DDDA15; hit_candidates.cpp is in playable build.ps1.
evidence: source-to-source audit confirms Unity hard-rejects non-character kind8 and lacks candidate/consumer selector plus full side-effect transaction; implementation split routed; no code/content/Scene changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / KIND8_FAMILY_ROUTED / B5_EXIT_NOT_READY`

完整结论见同名 Task 与 `docs/ai/MANIFESTS/NTSD28-B5-KIND8-CONTROL-RELATION.md`。下一包只建立
kind8 selector pure core；production必须在后续原子接candidate、actual与HitPlan。

