<!-- CHANGE-RECORD
id: NTSD28-USER-PLATFORM-CARRY-RATIO-001
status: FOCUSED_TEST_PASS
change-kind: USER_APPROVED_LINKED_PLATFORM_CARRY_XZ_RATIO
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06FrameMotionTailEditorTests.cs
authority: D-024 and paired playable BattleWorld28 linked-platform frame motion
evidence: artifacts/diagnostics/NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001/NON-INTEGRATOR-WRITER-AUDIT.md
-->

# NTSD28-USER-PLATFORM-CARRY-RATIO-001

Status `FOCUSED_TEST_PASS / PLAY_AND_FORMAL_VISIBLE_PENDING`. Task: `docs/ai/TASKS/NTSD28-USER-PLATFORM-CARRY-RATIO-001.md`.

Original Editor EditMode RED job `cdc0c47e3b4b4db59c94c48381c41399`: four actual registered-platform caller cases ran. Default-view right/left passed; configured-view right/left failed at X expected 206.1455363841/193.8544636159 vs raw 204/196. The X assertion precedes Z; configured Z still needs the GREEN assertion to confirm. Scene/DAT were not edited.

Pre-change Unity: `LF2Entity.ApplyLinkedPlatformMotion` reads the registered platform's frame DV, applies native decode/facing and adds raw X/Z passenger displacement from integer position. This runs before direct frame tail and separate physics integration; when the platform's own final travel is scaled for D-024, the passenger carry can remain visually short. The formal raw source behavior is not changed; user D-024 authorizes only the viewport-fraction output exception.

Intended after: one-time World factor on the linked passenger X/Z final displacement after existing decode/facing. Preserve exact platform selection, frame fields, Delay134, Y, velocity, branch order, integer rounding and DAT. Test configured/default World via the actual registered caller. No Scene or nonbattle edit.

Rollback: reviewed inverse of only the declared diff, preserving other current work.

Actual code: `LF2Entity.ApplyLinkedPlatformMotion` multiplies the decoded passenger X/Z carry at the two final position writes by the configured World X/Z view factors, defaulting to 1. It preserves Y/reference, frame DV, facing, velocity and rounding. The actual caller test in `NTSD28Q06FrameMotionTailEditorTests.LinkedPlatformCarryUsesViewRatioAndFacing` covers default/configured view and both facing directions. Original Editor after refresh compiled and job `eee3d49ce931424d8f9427fb48ce28ad` passed 4/4; adjacent original-source linked-motion regression job `425047edee8544faa792393a2dcaee60` passed 21/21. Full Driver, Scene Play, paired EXE visible ratio and platform floor geometry remain unverified.
