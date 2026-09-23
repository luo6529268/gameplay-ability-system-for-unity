<!-- CHANGE-RECORD
id: NTSD28-Q07-NONCHARACTER-DVY-SINGLE-APPLICATION-001
status: FOCUSED_TEST_PASS
change-kind: Q07_NONCHARACTER_FRAME_MOTION_SINGLE_APPLICATION
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
authority: formal NTSD2.8-Logan.exe B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable SimulationTickDriver28/frame_motion/physics
evidence: target-present OID875/action55 complete-tick source/Unity first difference in NTSD28-Q07-HITFA7-TARGET-FULLTICK-WITNESS-001
-->

# NTSD28-Q07-NONCHARACTER-DVY-SINGLE-APPLICATION-001

Status: `FOCUSED_TEST_PASS`. Exact scope, preconditions, invariants, validation and rollback are in the matching Task; final evidence is in `artifacts/diagnostics/NTSD28-Q07-NONCHARACTER-DVY-SINGLE-APPLICATION-001/REPORT.md`.

Before: the formal source-model full tick applies action55 `dvy:1` once and reaches Vy5.2/YInt -20. Unity applies it in the global native frame-motion pass and again in the non-character native physics frame-advance path, reaching Vy6.2/YInt -19. The source and original-Editor raw captures are preserved in the diagnostic artifact. The diagnostic numeric NUnit rerun is pending after an accidental broad EditMode job finishes; do not equate broad-suite failures to this first difference.

Intended after: the existing World-level legacy frame-motion suppression also covers the native physics pass that follows global native frame motion. Preserve the native physics integration, landing, snapshots and later serial suppression; make no character, scene or content change. Verify the same target-present full-tick case and narrow adjacent regressions in the original Editor, then record actual file/symbol diff and results here before advancing status.

Actual script diff: `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs`, method `NativePhysicsAndDeadCharacterResourceNormalizeAll`, now saves the existing suppression flag, sets it during the native physics loop, and restores it in `finally`. No change to the per-slot physics, normalization or snapshot calls. The diagnostic Change `NTSD28-Q07-HITFA7-TARGET-FULLTICK-WITNESS-001` also adjusted its declared test file comparator from exact binary64 equality to `Within(1e-9)`; that test edit is not assigned to this production Change.

Validation: pre-fix original Editor exact single test job `43584c3d7f314ed0b464c5825526bb17` was 0/1 with tick1 integerY expected -20/actual -19. Post-fix job `83c62d25b9864b97b1e135bf87b0b4f4` passed 1/1. Adjacent exact test job `66c0cc2d532b46b5a606e3789d2e85b4` passed 5/5. Fresh formal paired-source runner built with 0 errors; original Editor main/B0v2/B2 kind/type3 recapture returned PASS. `NTSD28Parity` Release build was 0 warning/0 error; main raw 150/150 field occurrences equal, B2 equal, B0 shared domains equal with known RNG stream topology difference. Both Scene SHA values remain unchanged. This is focused source-model evidence only; formal EXE visible/Player, tick3 diagnostic OPoint prerequisite and Q07/R15 overall remain open.
