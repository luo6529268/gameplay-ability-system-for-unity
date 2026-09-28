<!-- CHANGE-RECORD
id: NTSD28-Q09-P02-HELD-UNSATURATED-PHASE-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07NarutoPhysicalPickupBattlePlayProbeEditor.cs
authority: formal playable presentation_interpolation.cpp and original Editor q09-held-pixel-20260928-c saturated-alpha limitation
evidence: docs/ai/TASKS/NTSD28-Q09-P02-HELD-UNSATURATED-PHASE-001.md
-->

# NTSD28-Q09-P02-HELD-UNSATURATED-PHASE-001

Before: the current natural physical pickup at held-air tick20 has a resolved OID120 central command and two camera images, but both are alpha1 with equal X/PNG; no mid-phase witness. After: a separate opt-in Editor diagnostic controls only the presentation publication clock for two same-tick camera captures, records actual alphas and command/pixel evidence, and restores every changed static clock field. The exact trigger, output, invariants, tests and rollback are in the Task.

Expected side effects: two new report PNGs and a JSON file under a new Q09 artifact directory; temporary controlled display clock and camera target during the opt-in Play only. World, production renderer, DAT, images, scenes, input settings and nonbattle paths are not modified. Generated Editor compile, one original Editor Play, checksum, ordered shutdown, protected SHA, `Tools/Validate-ChangeLedger.ps1` and `git -c core.safecrlf=false diff --check` are required before handoff. Alpha/command phase success is not target-owned pixel proof.

Code written 2026-09-28: only the declared Editor probe gained the new opt-in request/result flags and `ControlledDisplayClock`, saving private renderer publication-clock fields, setting 0.20/0.70 elapsed fractions, force-materializing each phase, then restoring clock and rematerializing in `Dispose`. Existing normal held-pixel branch remains unchanged. Generated Editor project build: 0 errors/190 warnings. Original Editor Play, four SHA and audit checks pending; status `CODE_WRITTEN`.

First Play `q09-held-unsat-20260928-a` retained FAIL: natural pickup6/held-air20 and two full-camera PNGs were produced; alpha1/1 and command X equal. Per-capture checksums equal; focus restored, ordered shutdown complete, Scene clean/hash stable, World/slots/borrowers0 and original Editor idle/nonPlay. Since `ResolveDisplayAlpha` also forces alpha1 when `PreviousMotionTickIndex` is nonadjacent, the cause is not yet established. Pre-edit correction: add exact previous/current motion tick and alpha-before-camera evidence; for the opt-in only, reset the saved display clock again from `RenderPipelineManager.beginCameraRendering` for the target camera, detach callback in `finally`, then rerun once. If adjacency or callback is missing, report the limit and do not repeatedly run the slow natural case. No production or nonbattle edit.

Second Play `q09-held-unsat-20260928-b` retained top-level FAIL with bounded positive evidence: motion19→20 and begin-render callback1/1, actual alpha0.20553→0.70514, OID120 command X -10.054014→-9.885012, both 960×540 PNG and identical per-capture World checksum. The failure was a final test-only submission assertion after the clock restorer force-materialized a new alpha1 plan; that new plan is naturally NotSubmitted. Before next code edit, capture the after-camera diagnostic before `ControlledDisplayClock.Dispose` and use that phase-specific result in the final guard, while still checking checksum after restoration. No third slow natural Play is authorized merely to turn the old raw FAIL green. Independent PNG difference is 839 total/412 within the projected weapon-union rectangle after Y-axis inversion; full other-command overlap prevents attribution. Focus/Scene/shutdown/zero borrowers and Editor idle passed.

After `-b`, only the declared Editor probe moved the final `Submitted` diagnostic capture before clock restoration, preserving the post-restoration World checksum/tick guard. Generated Editor build again 0 errors/190 warnings. No third Play was run, so the raw FAIL and phase-specific data remain exactly as observed. Status `RUNTIME_PENDING / UNSATURATED_COMMAND_WITNESS / PIXEL_OWNER_PENDING`; separate target-command isolation A/B is required to attribute the 412 in-rectangle changed pixels. Full evidence and exact non-claims are in `artifacts/diagnostics/NTSD28-Q09-P02-HELD-UNSATURATED-PHASE-001/ACCEPTANCE-20260928.md`.

Final handoff checks: original Editor PID11944 DLL newer than revised source and bridge idle/nonPlay/no compile; four protected SHA all match. `Tools/Validate-ChangeLedger.ps1` PASSED (948 records, 17 diff code files), `git -c core.safecrlf=false diff --check` exit0. The Q09 request is consumed false. No production, DAT, scene, Asset or nonbattle edit.
