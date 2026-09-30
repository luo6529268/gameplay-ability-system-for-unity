<!-- CHANGE-RECORD
id: NTSD28-Q08-LETHAL-FALL-TIMER-TEST-EXPECTATION-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
authority: formal Logan executable B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable battle_world.cpp resolve_unarmored_reaction
evidence: docs/ai/TASKS/NTSD28-Q08-LETHAL-FALL-TIMER-TEST-EXPECTATION-001.md
-->

# NTSD28-Q08-LETHAL-FALL-TIMER-TEST-EXPECTATION-001

Before edit: a direct original-Editor nested HitPlan test reports `Expected: 0 / But was: 80`. Its preceding plan-valid and difference-mask-zero assertions must have passed before the old FallCounter assertion, and the paired formal ordinary-hit path leaves the lethal type-0 reaction timer at 80. The old test expectation is inconsistent with the current formal rule; no production first difference has been established from this failure. Task contract limits the edit to one test assertion.

Expected side effect: the focused HitPlan shadow fixture accepts the formally correct lethal timer while continuing to enforce plan validity, exact writer-effect mask zero, HP and every other observable. No production behavior, DAT value, scene, resource, framework or nonbattle code changes. Rollback and validation are in the Task; previous dirty work is protected.

2026-09-29 implementation: changed only `BattleHitExecutionPlanEditorTests.ShadowCompare_LethalStandardCharacterDamageWriterEffectMatchesAuthorityState`'s `target.FallCounter` assertion from zero to 80. This updates the test oracle; production behavior is unchanged. Original-Editor compile and focused tests pending at this checkpoint.

2026-09-29 focused RED after first edit: original Editor compiled/reloaded and exact nested job `6c750ed8f32343abada03c7a7142e529` failed at the next `target.HitStateCount` assertion, expected 45/actual 0. Before a second edit, the Task was expanded to check the paired formal `bdefend_accumulator=45` through Unity `Runtime.Bdefend` and preserve historical HitStateCount 0. Same method and file only; no production edit.

2026-09-29 further source audit before editing the remaining old spark assertions: formal and Unity writers consume native CRT twice for spark Y/X, not the test's old `world.Rng`. Task scope expanded in the same method to use the established Q07 native-CRT test oracle. No production RNG edit.

2026-09-29 final scoped result: the same test method now asserts lethal `FallCounter=80`, formal `Runtime.Bdefend=45` with legacy `HitStateCount=0`, and native CRT spark coordinates/state/call count. Original Editor refresh compiled/reloaded with no observed compile error. Exact nested job `330a2116f50f41159839d8f4f1be4db8` passed 1/1; adjacent B5 producer job `e2890cf7f01a4009a4c1626f5037193e` passed 4/4, including the formerly failing `HitPlanShadowAndSources_IncludeExactKnockoutObservable`. Raw MCP results: `artifacts/diagnostics/NTSD28-Q08-LETHAL-FALL-TIMER-TEST-EXPECTATION-001/`. Editor returned idle and non-Play. Battle/Menu/GameConfig/ProjectBattleModeConfig SHA-256 match the protected baselines. This closes the stale test oracle only; Q08 SelfCheck, integrated mode Play, same-seed formal and full phase acceptance remain pending.
