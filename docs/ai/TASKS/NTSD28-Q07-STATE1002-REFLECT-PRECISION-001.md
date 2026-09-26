# NTSD28-Q07-STATE1002-REFLECT-PRECISION-001

Status: `FOCUSED_TEST_PASS / TEST_ONLY / FULL_SELFCHECK_PASS`. Parent BATCH-04/Q07 remains open.

Authority and trigger: after correcting the odd-`dvx=5` ordinary ground reduced knockback expectation to 2.5, the original Editor's full `BattleRuntimeSelfCheck` reaches `CheckAlternateDamageHeavyWeaponEntries` and fails its combined state1002 frame/velocity assertion. Formal `battle_world.cpp::apply_native_reduced_attacker_post_hit` writes attacker X motion as `-target.pending_hit_impulse.total.x * 0.5`, Y=-4 and Z/= -1.5. The fixture's target impulse is 2.5 and attacker initial Z=6, so the expected X=-1.25 and Z=-4 are a source-based inference; the actual Unity values have not yet been individually observed.

Exact write scope: only the combined state1002 assertion in `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`. First split its message into actual frame/Vx/Vy/Vz diagnostics while leaving the old -1.0 predicate intact, run the original Editor full SelfCheck to observe the first difference. If the formal-source inference is confirmed, change only that Vx expectation to -1.25. Do not change production battle logic, DAT values, images, scenes, config, nonbattle scripts or unrelated SelfCheck assertions.

Acceptance: preserve prior fresh full-SelfCheck FAIL, obtain actual frame and velocity values, then run one original Editor full SelfCheck after the exact expectation correction. Report any next independent failure without broadening this Task. Compile, protected scene/config hashes, ledger validator and `git diff --check` must be checked. A test correction is not Q07 or total-goal parity.

Rollback: exact assertion/message hunk only, with explicit approval before destructive Git restore; preserve existing dirty work.

Result: original Editor diagnostic RED showed only X expectation mismatch (`frame3, Vx-1.25, Vy-4, Vz-4`). Changing the historical X expectation to formal -1.25 led to one fresh full `BattleRuntimeSelfCheck` PASS. Raw evidence and limits are in same-ID `ACCEPTANCE.md`.
