# Task: fix C++-aligned jump velocity retention in Unity NTSD

Work in this repository. Implement and test the smallest correct code change. Do not revert or format unrelated dirty-worktree changes. You are not alone in the codebase; preserve all existing work.

## Confirmed root cause

The user explicitly authorizes the C++ project `J:\QQFile\NTSD2.4\ntsd_release` as the behavior reference for this issue because the C# port appears wrong. In C++ `src/entity/frame_advance.cpp`, frame 212 sets `vy`, and only overwrites `vx`/`vz` when an exclusive direction key is currently held. Otherwise it preserves the pre-jump horizontal velocity. C++ does not clear current input keys immediately before frame advance.

Unity currently clears current action/directional keys in `SimulationWorld.SerialTickAll` immediately before `SimTransit`, matching a bug in the C# port. This prevents the 211->212 transition from observing held direction and breaks jump momentum. The Unity frame-212 and airborne physics formulas are otherwise already aligned.

## Required implementation

1. In `Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs`, remove the two current-key clears immediately before `entity.SimTransit(tickIndex)`. Add a short comment only if it clarifies the C++ contract: current held state must remain visible through frame advance; the input phase owns rolling/clearing next-tick state.
2. In `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`, update `CheckGameTickInputClearBoundaries` GT-02 and `FrameAdvanceInputProbeEntity` so the self-check asserts that current and previous keys are preserved into `SimTransit`, while GT-01 battle-entry reset remains unchanged. Rename helpers/properties/messages as appropriate.
3. Add a focused regression assertion for frame 212 jump initialization if feasible using existing fixtures: (a) exclusive held direction applies DAT `jump_distance` / `jump_distancez`; (b) no direction preserves existing `Vx/Vz`; (c) no new input edge/cooldown/history is manufactured. Prefer exercising the real `SerialTickAll` and frame transition, not duplicating formulas. Do not alter DAT movement values or visual scaling.
4. Run `git diff --check` and `dotnet build Assembly-CSharp.csproj --no-restore /m:1 /v:minimal`. Report exact results in `.omc/research/codex-fix-cpp-jump-input-retention-20260722-summary.md`.

Do not modify rendering files, prefabs, stage.dat/T8, Generated code, or Plugins. Do not claim Play Mode verification.
