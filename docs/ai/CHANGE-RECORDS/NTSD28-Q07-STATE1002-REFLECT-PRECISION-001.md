<!-- CHANGE-RECORD
id: NTSD28-Q07-STATE1002-REFLECT-PRECISION-001
status: FOCUSED_TEST_PASS
change-kind: Q07_STATE1002_REFLECT_PRECISION_SELFCHECK
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: formal root NTSD2.8-Logan.exe paired playable battle_world.cpp apply_native_reduced_attacker_post_hit state1002 branch
evidence: original Editor full SelfCheck after half-dvx correction fails at CheckAlternateDamageHeavyWeaponEntries combined frame/velocity assertion; formal formula predicts Vx -1.25 from target impulse 2.5
-->

# NTSD28-Q07-STATE1002-REFLECT-PRECISION-001

Pre-change: the existing combined state1002 SelfCheck still expects attacker X velocity -1.0 after an ordinary reduced `dvx=5` hit, while the now-correct target impulse is 2.5. Formal state1002 reduced post-hit reflection computes `-target.pending_hit_impulse.total.x * 0.5`, suggesting -1.25. Because the combined assertion also tests frame/Y/Z, the actual failing component must first be measured in the original Editor. Only this SelfCheck hunk is declared for diagnostic text and, if confirmed, the precise X expectation. No production or resource edit.

The Task Contract records acceptance and rollback. Status `PLANNED` before script write; the previous full SelfCheck failure remains intact as an artifact.

Diagnostic script edit: the combined assertion still expects X=-1.0, but now prints the actual frame and Vx/Vy/Vz on failure. No predicate or production behavior changed. Original Editor refresh and actual-value run pending; status `CODE_WRITTEN` diagnostic only.

Diagnostic RED: original Editor full SelfCheck after a refresh reported `frame=3, vx=-1.25, vy=-4, vz=-4` at the same combined assertion; the prior result and fresh diagnostic RED are preserved separately. Frame/Y/Z pass their stated predicates; only historical expected Vx=-1.0 is wrong. The formal reduced state1002 formula predicts -1.25 from target impulse 2.5. Changed that one expected Vx to -1.25; retained the failure-value diagnostic message. No production or resource edit. Original Editor post-correction run pending.

Post-correction original Editor full `BattleRuntimeSelfCheck` returned `PASS` on one fresh request after refresh; raw result and the prior diagnostic RED are preserved under the same-ID artifact. Editor assembly postdates the test script; recent log tail has zero C# errors. Four protected scene/config hashes unchanged. `Tools/Validate-ChangeLedger.ps1` and `git diff --check` pending final execution. Status `FOCUSED_TEST_PASS` records the test-only correction and full SelfCheck result, not Q07/natural Play/root EXE parity. See same-ID `ACCEPTANCE.md`.

Final audit: `Tools/Validate-ChangeLedger.ps1` exit0 (`Change ledger validation PASSED`), `git diff --check` exit0, four protected hashes unchanged, original Editor assembly newer than source and recent Console log tail zero C# compile errors. No additional full SelfCheck rerun after the PASS because no script behavior changed.
