# NTSD28-B5-NONTYPE0-HIT-MOTION-ARM-001 — non-type0 hit-motion arm

<!-- CHANGE-RECORD
id: NTSD28-B5-NONTYPE0-HIT-MOTION-ARM-001
status: VERIFIED
change-kind: TEST_FIRST_SHARED_ARM_INTEGRATION
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5NonType0HitMotionArmEditorTests.cs
authority: NTSD 2.8-Logan arm_native_unarmored_hit_motion outside type6 skip block; EXE B1E13AE1, closure 39DDDA15.
evidence: OTHER-TARGET-GATE-AUDIT-VERIFIED / WEAPON-AND-SPECIAL-REACTION-HORIZONTAL-SEAMS-READ / TEST-FIRST-BEHAVIOR-RED7-PASS1 / COMPILE0 / FOCUSED8 / RELATED241 / NTSD28-BROAD606 / SELFCHECK-PASS-2026-09-05T11:45:00Z / SCENE-UNCHANGED
-->

> 状态：`VERIFIED / TYPE1_6_HIT_MOTION_ARM_ALIGNED`

focused test先写入：armed production 7项按预期失败，type6 strict bypass负例1项已通过；
现在进入两个seam的最小production实现。

- weapon与special/other两个owner均在reaction/hurt之后、horizontal response之前调用既有arm。
- type1..6 armed写入全部通过；type3随后按authority专用continuation清pending impulse，但arm status
  carrier保留，可证明seam已执行。
- type6同时证明status tail继续跳过而arm执行；strict stabilization production bypass保持。
- fresh compile 0 error；focused `e166e420bcb44278a564cbaba7296d70` 8/8；
  full-hit related `d212caf385584d58bf2f648013a586d6` 241/241；精确NTSD28 broad
  `b485759c256d4804a94b1fa6dd797f7a` 606/606。
- BattleRuntimeSelfCheck `2026-09-05T11:45:00Z` PASS；Scene SHA
  `0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`、203477 bytes、
  mtime unchanged。producer family闭合；resource/armor/effect等其余B5差异后置。
