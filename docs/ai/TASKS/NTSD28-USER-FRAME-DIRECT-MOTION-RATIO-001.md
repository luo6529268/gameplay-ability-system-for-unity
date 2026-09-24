# NTSD28-USER-FRAME-DIRECT-MOTION-RATIO-001

Status `FOCUSED_TEST_PASS / PLAY_AND_FORMAL_VISIBLE_PENDING`. Parent D-024 all-entity motion ratio and `NTSD28-UNITY-BATTLE-REALIGNMENT-001`.

Authority: user D-024 requires each battle entity's visible travel fraction to match the formal view while retaining the current fixed full-background camera and raw DAT. The paired playable `SimulationTickDriver28::step` reaches `BattleWorld28` frame motion; `battle_world.cpp` writes raw `dx/dy/dz` relative to integer position, then clears the axis velocity. Formal staged OID736 (`a/wal/wal.dat`, action120) has `dz:-2` and `dvz:550` and is spawned by character OPoints. This is a current-content reachable candidate, separate from common velocity integration and late OPoint birth.

Before: `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs::ApplyNativeFrameMotionTail` uses only DelayTimer134's 0.25/1 factor for direct `dx/dz`, then clears Vx/Vz. The scaled common X/Z integrator cannot correct that direct displacement. Existing `NTSD28Q06FrameMotionTailEditorTests` tests default-scale formal behavior only.

Declared script paths: the above `LF2Entity.cs` and `Assets/NTSD/Scripts/Test/Editor/NTSD28Q06FrameMotionTailEditorTests.cs`. No DAT, Scene, Prefab, camera, nonbattle owner or Y-height behavior changes. Keep platform-carried movement in a later independent package.

Acceptance: add one configured/default World OID736/action120 Z witness and one directional synthetic `dx` witness to the declared test, run exact RED on the unchanged producer, then scale only the direct X/Z displacement after the existing DelayTimer134 factor at the final position write. Preserve raw Vx/Vz clearing, Y, frame timing, integer rounding and default-factor behavior. Original Editor compiles; exact new and narrow adjacent default tests pass; Scene SHA/DAT status and Change Ledger are checked. A successful focused test is not all-entity D-024, full Driver Play or formal EXE visible parity.

Rollback: reviewed inverse of only the declared lines, preserving all pre-existing dirty work; no destructive Git operation.
