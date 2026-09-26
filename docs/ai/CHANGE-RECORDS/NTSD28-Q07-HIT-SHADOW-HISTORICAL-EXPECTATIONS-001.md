<!-- CHANGE-RECORD
id: NTSD28-Q07-HIT-SHADOW-HISTORICAL-EXPECTATIONS-001
status: FOCUSED_TEST_PASS
change-kind: HIT_SHADOW_FORMAL_EXPECTATION_CORRECTION
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
authority: formal root NTSD2.8-Logan.exe paired playable battle_world.cpp resolve_unarmored_reaction and standard/reduced hit blocks
evidence: neighboring ShadowCompare test job 7843cb4886ea46c684e0694dacfdd7d8 reached writer mask zero in all selected cases but failed old HitStateCount and fall timer assertions
-->

# NTSD28-Q07-HIT-SHADOW-HISTORICAL-EXPECTATIONS-001

Pre-change: the standard ShadowCompare test expects legacy `HitStateCount=45` instead of the formal unarmored `bdefend_accumulator=45`. The reduced test expects `HitStateCount=15` after setting it to five and applying bdefend ten, whereas the formal reduced branch adds to bdefend. Three standard falling cases expect timer zero after `fall=70` or lethal kind-nine, while the formal reaction resolver yields 80. The current tests are diagnostic-only; production X550/X1200 raw parity is independent.

Declared code path and symbols before edit: `BattleHitExecutionPlanEditorTests.cs::ShadowCompare_StandardCharacterDamageWriterEffectMatchesAuthorityState` and `ShadowCompare_AlternateCharacterDamageWriterEffectMatchesAuthorityState`, limited to these assertions and parameterized expectations. Expected side effect is correcting historical test claims, not changing gameplay. Nonbattle paths and protected assets remain unchanged. Acceptance, risks and exact-hunk rollback are in the Task Contract. New failures after these gates must be preserved and audited, not automatically changed to match Unity.

Additional declared temporary symbol before editing: `ShadowCompare_HistoricalExpectationFailureStackProbe` in the same test file. Current-assembly jobs `0575071ec8034d8e9fa3f5797fbce2e2` and `5d45bffb360640fd8c678aa9ef7da662` report the same 12/9 assertion even for a standard method with no such assertion; a short diagnostic wrapper will capture the true direct-invocation exception stack through the original Editor test runtime and then be removed.

Before the next test-code edit, the temporary two-case probe job `cbfa79f34bc046509d3519a13de938f5` revealed the actual failure at `GetHitRecordX` in both methods. Formal `append_confirmed_native_spark` and Unity `BattleNativeHitSparkWriter.Append` consume the separate CRT stream; the historical test seeds/asserts `world.Rng` instead. The same two method scopes will now seed `world.NativeRandom`, compute expected two jitters from its post-table-init CRT state, and assert NativeRandom CRT state/calls. This corrects test setup and evidence, not production RNG ownership.

Actual code change: corrected three falling test-case timer parameters to 80, replaced legacy `HitStateCount` assertions by the formal bdefend field assertions in the two methods, and changed their spark expected-coordinate setup/final RNG checks to `world.NativeRandom` CRT state after explicit seed/table initialization. A temporary `ShadowCompare_HistoricalExpectationFailureStackProbe` produced two direct NUnit stacks, then was removed. The final test file contains no probe method. No production script, DAT, image, Scene, config or nonbattle path was changed by this Change ID.

Validation: current source before correction job `be3c26b03877474da691de0c0995aabe` 20/20 FAIL on old bdefend/timer expectations. After first correction job `0575071ec8034d8e9fa3f5797fbce2e2` 20/20 FAIL at spark X; exact single-case job `5d45bffb360640fd8c678aa9ef7da662` corroborated. Direct wrapper job `cbfa79f34bc046509d3519a13de938f5` 2/2 wrapper PASS with captured underlying exception stacks. Final scoped selector `e093dd020cbf40c4888d216c5a740790` 20/20 PASS and writer-mask-zero/plan-valid retained. After probe removal, original Editor Assembly-CSharp-Editor.dll recompiled at 2026-09-26 14:02:24 local, read_console errors zero and probe test count zero. The 20 selected cases were not repeated after probe removal because their bodies did not change. Production formal raw trace, Play and GPU were not rerun for this test-only package; prior evidence remains separately scoped. Acceptance and remaining risks are in the linked report.

Result: `FOCUSED_TEST_PASS / TEST_ONLY`; Q07/BATCH-04/full goal remain open. Rollback requires review of only these two test method hunks and the three TestCase values, preserving prior user work.

Final scope checks: Change Ledger validator PASS, records 869 / current governed code files 29; `git diff --check` exit 0. Menu/Battle Scene and two configuration asset hashes remain their protected baselines, and the three recovered status documents contain zero NUL bytes. Exact hashes and commands are in the linked acceptance report.
