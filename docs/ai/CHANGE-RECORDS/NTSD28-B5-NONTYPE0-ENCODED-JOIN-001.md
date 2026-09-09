# NTSD28-B5-NONTYPE0-ENCODED-JOIN-001 — non-type0 encoded status and join

<!-- CHANGE-RECORD
id: NTSD28-B5-NONTYPE0-ENCODED-JOIN-001
status: VERIFIED
change-kind: TEST_FIRST_SHARED_PRODUCER_INTEGRATION
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5NonType0EncodedJoinEditorTests.cs
authority: NTSD 2.8-Logan non-type6 damage/status/join tail in resolve_confirmed_unarmored_hit; EXE B1E13AE1, closure 39DDDA15.
evidence: OTHER-TARGET-GATE-AUDIT-VERIFIED / UNITY-WEAPON-SPECIAL-OTHER-OWNER-READ / TEST-FIRST-BEHAVIOR-RED11 / COMPILE0 / FOCUSED11 / RELATED233 / NTSD28-BROAD598 / SELFCHECK-PASS-2026-09-05T11:30:59Z / SCENE-UNCHANGED
-->

> 状态：`VERIFIED / TYPE1_5_ENCODED_JOIN_ALIGNED / TYPE6_SKIP_VERIFIED`

focused test已先写入并得到behavior red 11/11：type1..5缺encoded/join；type6另有一条越界
断言把后续weapon-specific group copy误归为join，已移除，仅保留zero-RNG/status/join carrier
边界。现在进入最小production实现。

- weapon owner仅对`normalVitalWeapon`（type1/2/4）调用既有encoded producer与side-effect helper；
  type6不调用，因此status synchronized RNG为0增量。
- special/other owner仅对type3/5调用；mimic counter写入但type gate阻止proxy enable，join正常激活。
- fresh compile 0 error；focused `c47b7c21482543288e0ea27e5f26f0c6` 11/11；
  full-hit related `7a299fb7133b466c807eaf15f8b94fe2` 233/233；精确NTSD28 broad
  `66c9e6db907d4828801608b82a86df8f` 598/598。
- BattleRuntimeSelfCheck `2026-09-05T11:30:59Z` PASS；Scene SHA
  `0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`、203477 bytes、
  mtime unchanged。non-type0 arm与其余resource/armor/effect差异后置。
