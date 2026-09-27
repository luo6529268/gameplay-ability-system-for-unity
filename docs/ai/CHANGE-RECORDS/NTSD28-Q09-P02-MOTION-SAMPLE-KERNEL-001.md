<!-- CHANGE-RECORD
id: NTSD28-Q09-P02-MOTION-SAMPLE-KERNEL-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationMotionSampler.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattlePresentationMotionSamplerEditorTests.cs
authority: formal Logan presentation_interpolation.cpp motion sample and user-approved D-024 view scale
evidence: artifacts/diagnostics/NTSD28-Q09-P02-MOTION-SAMPLE-KERNEL-001/TASK-CONTRACT.md
-->

# NTSD28-Q09-P02-MOTION-SAMPLE-KERNEL-001

Before: adjacent value rows are published and frozen, but no Unity code applies the formal identity, relation, source-motion and rounding gates to a display delta. The current central/Legacy renderer consumes the discrete frame. After this package: one pure presentation function returns either a formal-gated source-to-view delta or an explicit rejection status; rendering remains unchanged until its separate consumer package.

Declared paths and symbols: new `BattlePresentationMotionSampler` and `BattlePresentationMotionDelta` in the Presentation namespace; new focused Editor test class. The source rows, runtime velocity and logic checksum are read-only inputs. The native formal snapshot and interpolation call chain is the authority; the user-approved full-background D-024 scale applies only to accepted X/Z output deltas, never DAT values or source-state decisions.

Expected effects: no GameObject, Mono lifecycle, worker, resource lease, pool, snapshot version, pass order, logic coordinates, rendering command, camera, scene, menu, audio or nonbattle change. If the source coordinate carrier is unavailable, keep the row discrete. The two measured pre-pickup `0/-1` sentinel differences are reported in the Task audit and do not authorize a runtime default rewrite.

Validation: generated Runtime and Editor compile, focused original-Editor formal-gate tests, Ledger validator and diff check. A passing pure sampler does not certify visible 30/60/120 interpolation or Q09 completion. Risks are midpoint rounding, source/view double scaling, stale tick pairing and identity/relation false positives; each has a targeted test. Rollback is confined to the two newly declared script files and their metas, after exact-path review.

2026-09-27 implementation: `BattlePresentationMotionSampler.cs` adds a pure return-status sampler and view delta type. It rejects nonadjacent tick, changed handle/OID, changed six raw relations, unavailable source X/Z, and formal axis motion threshold failures. It clamps alpha, uses away-from-zero native rounding in the source domain, and scales only accepted X/Z display deltas. `BattlePresentationMotionSamplerEditorTests.cs` covers midpoint/negative rounding, scale and Y separation, alpha bounds, identity/generation, all six relation changes including encoded `0x2000` catch source, teleport versus motion-supported dash, and unavailable source. No renderer consumes this kernel yet; logic and visible presentation remain unchanged.

Status `FOCUSED_TEST_PASS / SAMPLER_ONLY`: original Editor recompiled the two new files; initial EditMode job `a918e832e57446f3afed3677bd1be7b2` passed 3/3, and after the final missing-history-tick guard revision job `435639a6cfce46498288a04dbddd62a6` again passed 3/3. After refresh, generated Runtime and Editor MSBuild each exited 0 with zero errors when run sequentially. A parallel MSBuild attempt briefly failed CS2012 because both builds shared the same `obj/Debug/Assembly-CSharp.dll`; this was build-output contention, not a source diagnostic, and the sequential rerun passed. Formal playable interpolation source test recompiled and passed. Scene files remain unchanged. Ledger validator PASSED (908 Records, 24 governed code files in the current diff) and final diff/whitespace checks passed; existing declared-but-not-current-diff warnings remain. Full command/evidence and remaining validation limits: [ACCEPTANCE](../../../artifacts/diagnostics/NTSD28-Q09-P02-MOTION-SAMPLE-KERNEL-001/ACCEPTANCE.md).
