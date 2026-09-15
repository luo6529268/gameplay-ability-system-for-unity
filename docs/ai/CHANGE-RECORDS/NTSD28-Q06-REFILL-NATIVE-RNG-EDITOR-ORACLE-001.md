<!-- CHANGE-RECORD
id: NTSD28-Q06-REFILL-NATIVE-RNG-EDITOR-ORACLE-001
status: VERIFIED
change-kind: REFILL_TEST_ORACLE_ONLY
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6HeldRefillMpExhaustionProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6WpointMissingActionContinueProductionEditorTests.cs
authority: Formal BattleWorld28::settle_held_refill_objects and system_battle_tables.h; native exhaustion RNG HP0x4181C9/MP0x4182C0, bound7.
evidence: Existing refill tests predict legacy RNG and expect its increments. Source242 independent229293 PASS and Unity242 both profiles zero; fresh100 regression94PASS6FAIL. Change only exhaustion predictor to local synchronized cursor, assert native+1 correctsite and unchangedlegacy/CRT; nonexhaustednativeunchanged; preserve resource/link/frame/Vz/unsupported assertions.
-->

# NTSD28-Q06-REFILL-NATIVE-RNG-EDITOR-ORACLE-001

IN_PROGRESS / TEST_ONLY. Existing refill tests predict legacy RNG and expect its increments. Source242 independent229293 PASS and Unity242 both profiles zero; fresh100 regression94PASS6FAIL. Change only exhaustion predictor to local synchronized cursor, assert native+1 correctsite and unchangedlegacy/CRT; nonexhaustednativeunchanged; preserve resource/link/frame/Vz/unsupported assertions.

No production/framework/Scene/resources or lifecycle changes. Prior failure archived under parent refill-after-fix. Acceptance current compileCS0, affected Editor regression or fullSelfCheck, ledger/diff. Rollback only exact testdiff with approval; preserve original failures and other changes.

准确扩展（新SelfCheck21:05:44Z失败后）：仅BattleRuntimeSelfCheck.RunAudit7WeaponReleaseCase consume分支的expectedConsumeVx与consumeRngCalls断言改native cursor/site+1及legacy/CRT不变；保留所有资源/关系/速度/动作/计数/池复用断言。非consume分支不改。文件已由其它Record覆盖的OID修订独立，不混写行为。

VERIFIED / TEST_ONLY：原100回归94PASS6FAIL、SelfCheck21:00:51Z/21:05:44Z失败均保留；修订后100/100，最终generic/replay/refill标记联合11/11，新完整SelfCheck21:07:43Z PASS；当前compileCS0、Ledger592/31 PASS、diff check0，独立review通过。证据在父refill-after-fix/refill-replay-generic-pass；不宣称整个Q06。
