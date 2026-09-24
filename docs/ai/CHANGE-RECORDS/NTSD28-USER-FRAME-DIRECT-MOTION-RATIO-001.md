<!-- CHANGE-RECORD
id: NTSD28-USER-FRAME-DIRECT-MOTION-RATIO-001
status: FOCUSED_TEST_PASS
change-kind: USER_APPROVED_FRAME_DIRECT_XZ_MOTION_RATIO
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06FrameMotionTailEditorTests.cs
authority: D-024 and paired playable BattleWorld28 frame motion with formal OID736/action120 dz -2
evidence: artifacts/diagnostics/NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001/NON-INTEGRATOR-WRITER-AUDIT.md
-->

# NTSD28-USER-FRAME-DIRECT-MOTION-RATIO-001

Status `FOCUSED_TEST_PASS / PLAY_AND_FORMAL_VISIBLE_PENDING`. Exact Task Contract: `docs/ai/TASKS/NTSD28-USER-FRAME-DIRECT-MOTION-RATIO-001.md`.

Pre-change Unity: the direct frame `dx/dz` position writes bypass the D-024 common X/Z physics integration and clear corresponding velocity. They use only the existing DelayTimer134 0.25/1 scale. Under the configured fixed 2048×1152 view, current OID736/action120 ZInt250 and `dz:-2` yields raw Z248, while equal fraction target is precise `250 - 2×1152/730`. The formal paired source still defines raw frame behavior; D-024 is the user-approved visible-ratio exception. This is a static prediction pending original Editor RED.

Intended change: multiply only the final X and Z direct frame displacement by the World factors after DelayTimer134's existing factor. Keep Y, velocity zeroing, integer rounding, stage geometry, platform carries, DAT and nonbattle owners unchanged. Focused tests assert default scale identity and configured ratio. No Scene or content edit.

Rollback: inverse patch on only two declared scripts after scoped diff review; preserve the current dirty tree.

Exact original-Editor RED before production edit: new six-case methods compiled with Tundra success; run_tests job e35baac0da5f4b73818518c50181e2d9 selected exactly six. All three default-factor cases passed. All three configured cases failed at the predicted direct position: formal staged OID736/action120 Z expected 246.84383561643835, actual 248; synthetic dx right expected 206.14553638409603, actual 204; left expected 193.85446361590397, actual 196. This confirms the bypass at actual frame-motion method, not merely static suspicion. Next change only declared LF2Entity X/Z direct position multipliers.

Implementation and focused validation: declared LF2Entity frame-tail X and Z direct position deltas now multiply the configured World view factor after the existing DelayTimer134 0.25/1 scale; Y, Vx/Vz zeroing and native rounding remain unchanged. Original Editor Tundra build succeeded, filtered log tail had no `error CS`. Exact configured/default OID736 dz and synthetic facing dx cases passed 6/6 in job f34c9af277a44e25a500980b097fd2ab. Adjacent default-scale paired-source frame-tail regression passed 12/12 in job fbe9082bec7646e7b7859aafdf55c4ee. Scoped diff --check passed; Battle Scene SHA remains 9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0, DAT/Scene Git status empty. Full Driver/Play and formal EXE visual fraction remain pending, as do platform, teleport, weapon-piece and Y/boundary categories.
