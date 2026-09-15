<!-- CHANGE-RECORD
id: NTSD28-Q06-REFILL-REAL-OID-SELF-CHECK-ORACLE-001
status: VERIFIED
change-kind: REFILL_TEST_ORACLE_ONLY
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Formal BattleWorld28::settle_held_refill_objects and system_battle_tables.h; native exhaustion RNG HP0x4181C9/MP0x4182C0, bound7.
evidence: Fresh SelfCheck21:00:51Z FAIL R2-SCHED-001 HP20: CheckReleaseTickRunsHeldStep12Twice uses fake drinkOid992/type_sub122. Change only method localconst drinkOid992 to formal122; same temporaryconfig/frames/HP20 and double-scan expected18 preserved. Formal system_battle_tables.h declares122/123; ordinaryOID78 negativecontrol source242 passes.
-->

# NTSD28-Q06-REFILL-REAL-OID-SELF-CHECK-ORACLE-001

IN_PROGRESS / TEST_ONLY. Fresh SelfCheck21:00:51Z FAIL R2-SCHED-001 HP20: CheckReleaseTickRunsHeldStep12Twice uses fake drinkOid992/type_sub122. Change only method localconst drinkOid992 to formal122; same temporaryconfig/frames/HP20 and double-scan expected18 preserved. Formal system_battle_tables.h declares122/123; ordinaryOID78 negativecontrol source242 passes.

No production/framework/Scene/resources or lifecycle changes. Prior failure archived under parent refill-after-fix. Acceptance current compileCS0, affected Editor regression or fullSelfCheck, ledger/diff. Rollback only exact testdiff with approval; preserve original failures and other changes.

VERIFIED / TEST_ONLY：原100回归94PASS6FAIL、SelfCheck21:00:51Z/21:05:44Z失败均保留；修订后100/100，最终generic/replay/refill标记联合11/11，新完整SelfCheck21:07:43Z PASS；当前compileCS0、Ledger592/31 PASS、diff check0，独立review通过。证据在父refill-after-fix/refill-replay-generic-pass；不宣称整个Q06。
