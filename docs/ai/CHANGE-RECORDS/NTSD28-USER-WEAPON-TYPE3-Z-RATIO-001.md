<!-- CHANGE-RECORD
id: NTSD28-USER-WEAPON-TYPE3-Z-RATIO-001
status: FOCUSED_TEST_PASS
change-kind: USER_APPROVED_WEAPON_TYPE3_PRECISE_Z_RATIO
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Weapon.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4IdentityXExtrasEditorTests.cs
authority: D-024 and paired playable physics_integrator.cpp type3 frame_hit_j depth displacement
evidence: docs/ai/TASKS/NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001.md
-->

# NTSD28-USER-WEAPON-TYPE3-Z-RATIO-001

Status `FOCUSED_TEST_PASS / PLAY_AND_FORMAL_VISIBLE_PENDING`. Task: `docs/ai/TASKS/NTSD28-USER-WEAPON-TYPE3-Z-RATIO-001.md`.

Original Editor registered-World RED job `983bcf69755d477cb2475d6d71817c0d`: default-view type3 frame hit_j60 passed at Z260, configured-view expected Z265.780821918 but actual remained260. The precise Z assertion precedes integer and raw tracker assertions, covered in GREEN.

Pre-change: specialized LF2Weapon frame physics adds raw `hit_j - 50` to precise Z and raw Type3VisualZOffset; generic LF2Entity path already scales only precise Z. Formal source applies raw addition. D-024 user exception requires same screen-fraction adaptation at this specialized final outlet.

Intended after: one-time World depth factor on `Runtime.Z` addition only, preserving raw tracker, DAT, velocity, gravity, branch and stage/floor semantics. Test actual registered configured/default World before and after. No Scene/camera/nonbattle change.

Rollback: reviewed inverse of declared lines only, preserving other current work.

Actual code: specialized LF2Weapon type3 `hit_j - 50` displacement multiplies only the final precise-Z write by registered World depth factor, mirroring the existing generic LF2Entity path; Type3VisualZOffset remains raw. Original Editor refreshed/compiled. Configured-view RED `983bcf69755d477cb2475d6d71817c0d` (expected Z265.780821918 vs raw260) became whole focused class GREEN `3dde7fbe889e4adab9a7eeff7ca6b492` 12/12, including integer Z mirror and raw tracker. Full tick/Play and formal EXE visible proof remain open.
