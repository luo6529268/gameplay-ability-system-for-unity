<!-- CHANGE-RECORD
id: NTSD28-Q06-HELD-RELEASE-FIELD-SELF-CHECK-ORACLE-001
status: VERIFIED
change-kind: HELD_RELEASE_FIELD_ORACLE_ONLY
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Current playable BattleWorld28::settle_held_refill_objects DVX release writes independent object_ai_excluded_group_source_slot_2f8 for type1/4/6; type2 preserves it. LF2Entity.SpawnerEntityIndex maps separate Runtime.SpawnerSlotIndex.
evidence: Fresh SelfCheck 2026-09-14T20:35:34Z fails R5-HOLD-002; source1150 and generic664 both profiles before/immediate/following zero differences, independent source296552 checks PASS. Read-only review confirms old Spawner expectation is incorrect.
-->

# Held release field self-check oracle

IN_PROGRESS / TEST_ONLY. Modify only CheckWorldLevelRealWeaponStep12Contracts and RunWorldLevelRealWeaponStep12Case. Replace type1/4/6 Spawner stamping assertions with independent +2F8 stamping; assign distinct Spawner sentinels81/84/86, preserve type2 Spawner77 and all picker assertions. Initialize +2F8 sentinel91, assert overwritten only DVX nonzero type1/4/6, otherwise preserved. Keep action/velocity/link/FrameDelay and damaged continuation checks unchanged. No production, ownership, schema, lifecycle, Scene, resources, GAS or nonbattle changes.

Original failure remains in parent release-generic-pass/self-check-203534.result. Acceptance: current compileCS0, full SelfCheck actually PASS, ledger/diff checks. This test-only correction cannot certify entire damaged-held branch; parent release replay/Play and refill remain pending. Rollback: manually reverse only these declared test changes with approval, preserve all other user/prior changes and failure evidence.

VERIFIED / TEST_ONLY：两方法精确修改经独立review通过。Unity CS0，完整SelfCheck2026-09-14T20:39:17Z PASS（同ID artifact）；Ledger590 PASS，diff check exit0。旧失败保留，不改变production或宣称damaged全事务已验。
