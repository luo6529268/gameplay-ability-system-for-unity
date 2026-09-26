# NTSD28-Q07-ALTERNATE-HALF-DVX-SELFCHECK-001

Status: `FOCUSED_TEST_PASS / TEST_ONLY`. Parent BATCH-04/Q07 remains open; full SelfCheck still fails later.

Authority and trigger: paired formal `HitResponseResolver28::accumulate_ordinary_horizontal` uses `dvx / 2.0` for ordinary reduced ground hits. `NTSD28-Q07-REDUCED-HIT-HALF-DVX-PRECISION-001` already corrected the Unity shared production writer and verified the selected formal root EXE/Unity raw trace. The later full `BattleRuntimeSelfCheck` stopped at `CheckAlternateDamageCoreSideEffects`: its historical assertion still expects integer `dvx/2` for `dvx=5`, conflicting with formal and current production semantics. Two neighboring self-check entry assertions use the same stale integer-half expectation.

Exact write scope: only the three odd-`dvx=5` expected knockback values and their descriptions in `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` (`CheckAlternateDamageCoreSideEffects`, `CheckAlternateDamageCharacterEntry`, `CheckAlternateDamageSharedDatEntry`). Expected value is 2.5 with the declared right-facing, grounded, ordinary reduced-hit fixtures. No production code, DAT value, image, scene, config, nonbattle or pass-order edit.

Acceptance: preserve prior full-SelfCheck RED; update the three expectations; refresh original Editor and run one full `BattleRuntimeSelfCheck` to expose any next obstruction rather than assuming whole-suite success. If a new independent failure appears, record it and stop this test-only Task at its scoped boundary. Verify Editor compile, protected scene/config hashes, `Tools/Validate-ChangeLedger.ps1` and `git diff --check`. This is a stale-test correction, not Q07 or total-goal parity.

Rollback: only these three expectation/message lines; any destructive Git restore requires explicit approval, and the dirty worktree remains protected.

Result: the original Editor's full SelfCheck crossed the old half-dvx assertion and next failed at the independent state1002 heavy-weapon tail assertion. The exact result and limits are in the same-ID `RESULT.md`. Do not record full SelfCheck PASS.
