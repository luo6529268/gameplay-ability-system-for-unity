<!-- CHANGE-RECORD
id: NTSD28-USER-OPOINT-BIRTH-RATIO-001
status: PLANNED
change-kind: USER_APPROVED_LATE_OPOINT_BIRTH_OFFSET_RATIO
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleLogicObjectPointRuntime.cs
code-path: Assets/NTSD/Scripts/Animation/Character/LF2ObjectPointFactory.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06OpointDepthLivesEditorTests.cs
authority: D-024 user all-entity screen-travel-fraction decision and current playable ObjectSpawnPlanner28::plan_frame
evidence: artifacts/diagnostics/NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001/INITIAL-INVENTORY.md
-->

# NTSD28-USER-OPOINT-BIRTH-RATIO-001

Status `PLANNED / EDITOR_JOB_PENDING`. Created before any script edit. Exact Task Contract: `docs/ai/TASKS/NTSD28-USER-OPOINT-BIRTH-RATIO-001.md`.

Before: both production late-OPoint owners (`BattleLogicObjectPointRuntime` and component `LF2ObjectPointFactory`) add unscaled DAT/frame-center X/Z offsets to the parent's current integer position. The current D-024 core movement slice has already scaled parent travel, so a child offset can occupy a smaller fraction of the fixed Unity view than its formal counterpart. Formal child placement is parent-relative and launch motion is separate.

Intended after: scale only the late child-minus-parent X and Z birth deltas in both producers using the World factors, including native depth `+1`; preserve parent's absolute coordinate, raw launch motion and Y for the later vertical contract. Write the resulting precise and integer task positions coherently. Keep the default-factor-one branch behavior identical. No DAT, Scene, other spawn route or nonbattle change.

Expected side effects: child actual world birth coordinates, hitbox origin and later stage/collision interaction shift under the approved fixed-camera exception. No change to frame/pass timing, object slot or pooling. Validate both original Editor late-OPoint owners in focused right/left configured cases and default-scale depth/source cases after the current broad test job terminates, plus ledger/DAT/Scene checks. Real Battle Scene/EXE visual comparison and other entity spawn categories remain open. Roll back by inverse patch on the three declared script files only, after reviewing concurrent work.
