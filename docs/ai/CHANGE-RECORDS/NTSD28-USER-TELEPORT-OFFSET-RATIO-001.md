<!-- CHANGE-RECORD
id: NTSD28-USER-TELEPORT-OFFSET-RATIO-001
status: FOCUSED_TEST_PASS
change-kind: USER_APPROVED_TELEPORT_RELATIVE_X_RATIO
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C05NativeTeleportProductionEditorTests.cs
authority: D-024 and paired playable BattleWorld28 resolve_native_teleport_state
evidence: artifacts/diagnostics/NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001/NON-INTEGRATOR-WRITER-AUDIT.md
-->

# NTSD28-USER-TELEPORT-OFFSET-RATIO-001

Status `FOCUSED_TEST_PASS / PLAY_AND_FORMAL_VISIBLE_PENDING`. Task: `docs/ai/TASKS/NTSD28-USER-TELEPORT-OFFSET-RATIO-001.md`.

Original Editor actual native World RED job `3d092d1ad1404e7c95c2f7cd27abc8dc`: 8 cases; four default-view cases pass, all four configured-view state400/401 right/left cases fail at the raw target-relative 120/60 X outlet (e.g. state400 right expected 115.633908477 vs actual180). The first native assertion precedes legacy assertions, so legacy still requires GREEN coverage.

Pre-change Unity: both native and legacy teleport paths copy the selected target's integer X and add raw 120/60 X offset. Under configured fixed full-background view, the relative screen fraction is shorter than formal, while common X/Z travel is scaled. The formal raw source remains unchanged; the user-approved D-024 exception only changes the final relative X placement in Unity.

Intended after: scale the horizontal target-relative offset once at final X position, keeping target absolute X, candidate selection, Y, Z+1, no-target branch, velocities, DAT, camera and nonbattle logic unchanged. Test actual native World entry and legacy branch. Risk: precise X may be fractional with integer mirror rounded by current battle convention; prove focused cases. Full Play and formal EXE visual proof remain open.

Rollback: reviewed inverse patch of declared teleport lines only, preserving other current work.

Actual change: native and legacy teleport paths multiply only the target-relative state400/401 horizontal 120/60 offset by the registered World X factor. They retain the target absolute X, Z+1, Y, candidate scan and motion reset. Precise X is double; integer mirror follows existing midpoint-to-even rounding. Original Editor compiled after refresh. Configured/default × left/right × 400/401 actual-World and legacy assertions passed 8/8 in job `6d3bce2079014495bec7ff035cdc0ca2`. Existing native teleport tests plus new cases passed 13/13 in job `e7ff4770007442ffa52e5ea4fca28f88`. Full battle tick, Scene Play, target selection under scaled X/Z, formal EXE screen comparison remain open.
