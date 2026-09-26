<!-- CHANGE-RECORD
id: NTSD28-Q07-REDUCED-HIT-HALF-DVX-PRECISION-001
status: FOCUSED_TEST_PASS
change-kind: SHARED_REDUCED_GROUND_HALF_DVX_PRECISION
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5UnarmoredHpConsumptionProductionEditorTests.cs
authority: formal root NTSD2.8-Logan.exe and paired hit_response.cpp ordinary reduced horizontal response
evidence: OID87 X550 after resource fix formal tick19 motionX8.4/Unity8; tick20 preciseX558.4/Unity558; source divides double dvx by 2.0 and Unity divides int dvx by 2
-->

# NTSD28-Q07-REDUCED-HIT-HALF-DVX-PRECISION-001

Pre-change owner is `BattleDamageWriter.ApplyAlternateGroundKnockback`. It truncates odd `itr.dvx` via `int halfDvx = itr.dvx / 2` before writing `victim.KnockbackVx`; the paired formal reduced-ground resolver retains the 0.5 part. `BattleDamageWriter` already uses `double` motion carriers, so the intended single-expression correction has no new field or schema. The expected effect is limited to odd-`dvx` ordinary ground reduced hits, including the OID87 strict X550 scenario. Even `dvx`, airborne/effect-special branches, ordinary unarmored hit, PP/HP and input/RNG are expected unchanged.

Declared paths, test-first RED, post-fix X550/X1200 comparison, protected boundaries and exact-hunk rollback are in the Task Contract. No DAT numeric edit or special case is authorized. This is an ordinary battle-rule correction; the D-024 ratio exception remains the user's decision and must be audited as a separate coordinate layer.

Actual edit: the shared ordinary-ground reduced-hit branch now calculates `double halfDvx = itr.dvx / 2.0`; the focused Editor test asserts the pending-X increment is 3.5 for odd `dvx=7`. No object-id check or DAT adjustment was added. An initial RED test incorrectly expected the absolute accumulator 3.5; the runtime initializes it to 0.1, so the test was corrected to expect its increase and re-run before production edit. The valid RED job `f5e0867896414e778bea4aa97ae09bca` failed with expected3.6/actual3.1. Original Editor recompiled with zero Console errors and focused GREEN job `9f762614b8064ea4b0713e500a3b5423` passed 2/2 (new odd-dvx and preceding resource test).

The two strict post-fix original Editor raw requests completed PASS, with explicit domain `v2` on the final capture files. Against unchanged formal root EXE traces, `entity-comparison-motionfix-v2.json` reports X550 68 occupied rows × 21 fields =1428 comparisons, zero differences; X1200 100 rows × 21=2100 comparisons, zero differences. Formal and Unity target motion X8.4 at tick19, precise X558.4 at tick20, and precise X602.7999999999998 at tick26. Versus the post-resource pre-motion Unity rows, only target motion at tick19–26 and position at tick20–26 change; both scenarios' v2 domain rows and input/RNG rows remain equal. An earlier post-fix capture omitted the explicit v2 option and yielded v1 domain files; those are retained but not used for v2 before/after comparison. Detailed evidence: `artifacts/diagnostics/NTSD28-Q07-SASUKE-ARMOR-TARGET-FIRST-DIFF-001/MOTION-FIX-ACCEPTANCE-20260926.md`.

Result: `FOCUSED_TEST_PASS / SCOPED_RAW_PARITY`. The mapped 26-tick controlled pair is equal, but natural physical Battle Play, broader reduced-hit combinations, D-024 user ratio in the physical battle scene and full Q07 exit were not verified by this package.
