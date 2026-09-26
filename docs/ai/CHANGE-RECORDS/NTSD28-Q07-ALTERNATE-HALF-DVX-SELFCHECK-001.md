<!-- CHANGE-RECORD
id: NTSD28-Q07-ALTERNATE-HALF-DVX-SELFCHECK-001
status: FOCUSED_TEST_PASS
change-kind: Q07_ALTERNATE_HALF_DVX_SELFCHECK_EXPECTATION
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: formal root NTSD2.8-Logan.exe paired playable HitResponseResolver28 ordinary ground reduced horizontal dvx/2.0 and prior NTSD28-Q07-REDUCED-HIT-HALF-DVX-PRECISION-001 acceptance
evidence: full BattleRuntimeSelfCheck stopped at stale integer dvx/2 assertion for dvx5 after production floating half-dvx fix
-->

# NTSD28-Q07-ALTERNATE-HALF-DVX-SELFCHECK-001

Pre-change: three self-check fixtures all use ordinary ground reduced kind-0 hits with odd `dvx=5` and right-facing attacker, but still assert horizontal knockback `2f` and describe an integer half. The current formal source and already-corrected Unity production writer use 2.5. The previously run full self-check failed at the first such assertion; the other two were not reached. Only the exact three stale test expectations/messages are in scope.

The Task Contract records rollback, boundaries and acceptance. Before script write this Record is `PLANNED`; no test repair or fresh run is claimed. No runtime manager, queue, worker, renderer or shutdown dependency changes.

2026-09-26 test-only edit: changed exactly the three `dvx=5` ordinary ground reduced-hit knockback expectations from `2f` to `2.5f` and replaced their integer-half wording with precise-half wording. Affected methods are `CheckAlternateDamageCoreSideEffects`, `CheckAlternateDamageCharacterEntry` and `CheckAlternateDamageSharedDatEntry`. No production, DAT, scene, config or nonbattle script was changed under this ID. Original Editor refresh and one full SelfCheck are pending. Status `CODE_WRITTEN` only.

Original Editor compiled the edit and consumed one full `BattleRuntimeSelfCheck` request. The result crossed the previous integer-half assertion and failed later in `CheckAlternateDamageHeavyWeaponEntries` at `state1002 alternate tail must update frame and reflected velocity on a real weapon`; result file preserved under the same-ID artifact. The three expectation edits are supported by formal `dvx / 2.0`, the accepted production writer and this later failure point, but the *full* SelfCheck is `FAIL`. The state1002 condition is a separate diagnostic requiring formal-source/Unity fixture audit, not a reason to change its assertion under this Task. Editor assembly is newer than source, recent log tail has zero C# errors, four protected hashes are unchanged. Ledger/diff validation pending. Status `FOCUSED_TEST_PASS` applies only to the stale odd-dvx gate.

Superseding full-suite result: `NTSD28-Q07-STATE1002-REFLECT-PRECISION-001` separately measured and corrected the later state1002 stale expectation; a fresh original Editor full `BattleRuntimeSelfCheck` then returned `PASS`. The earlier FAIL remains the accurate result at this Task's first run, not the current final suite status. The three half-dvx edits remain test-only.

Final cross-package audit: change-ledger validator and `git diff --check` both exited 0 after this and the successor test-only correction; four protected scene/config hashes stayed unchanged.
