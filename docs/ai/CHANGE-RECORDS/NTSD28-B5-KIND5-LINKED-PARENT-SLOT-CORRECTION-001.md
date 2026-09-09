# NTSD28-B5-KIND5-LINKED-PARENT-SLOT-CORRECTION-001

<!-- CHANGE-RECORD
id: NTSD28-B5-KIND5-LINKED-PARENT-SLOT-CORRECTION-001
status: VERIFIED
change-kind: BEHAVIOR_CORRECTION
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Kind5LinkedParentSlotCorrectionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5HitGroupEligibilityAtomicProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan BattleWorld28 rebuild_geometric_hit_candidates pair snapshot/nearest and resolve_standard_damage_interaction kind5 read EntityState28 linked_parent_slot only; EXE B1E13AE1, closure 39DDDA15.
evidence: Focused RED was 0/5 and proved helpers, frozen pair and kind5 replacement selected HolderCopy/root instead of exact HolderStableId. Production changed only the two helper reads to HolderStableId with implicit zero. Focused is 5/5, related HitPlan/frozen/group/role-aware tests are 280/280, runtime and serialized editor builds are 0 error, and targeted NTSD_Battle Play passes 5 cases with Console 0 and unchanged Scene SHA/dirty/root. Full SelfCheck remains blocked earlier by the unrelated CPoint mode0 victim-Vz assertion. HolderCopy remains unchanged; type3 extra writer is next.
-->

> 状态：`VERIFIED / RED_0_OF_5 / FOCUSED_5_OF_5 / RELATED_280_OF_280 / TARGETED_PLAY_5_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / EXACT_LINKED_PARENT_BOUND / HOLDERCOPY_UNCHANGED / TYPE3_WRITER_NEXT`

完整范围、Authority、不变量、验收与回滚见
`docs/ai/TASKS/NTSD28-B5-KIND5-LINKED-PARENT-SLOT-CORRECTION-001.md`。

RED0/5后production仅改两个helper；focused5/5、related280/280、串行builds0 error、真实Play5与
Console/Scene通过。full SelfCheck独立CPoint阻塞；type3 extra writer与stats/schema未并入。
Change Ledger validator：`PASS / 410 records / 342 governed code files`。
