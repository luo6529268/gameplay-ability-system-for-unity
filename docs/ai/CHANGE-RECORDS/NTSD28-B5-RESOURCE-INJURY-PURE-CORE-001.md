# NTSD28-B5-RESOURCE-INJURY-PURE-CORE-001 — resource injury pure core

<!-- CHANGE-RECORD
id: NTSD28-B5-RESOURCE-INJURY-PURE-CORE-001
status: VERIFIED
change-kind: TEST_FIRST_PURE_INTEGER_CORE
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5ResourceInjuryPureCoreEditorTests.cs
authority: NTSD 2.8-Logan native_hit_resource_injury28; EXE B1E13AE1, closure 39DDDA15.
evidence: HIT-RESOURCE-PREREQUISITE-AUDIT-VERIFIED / SIGNED-U32-ARITHMETIC-READ / TEST-FIRST-COMPILE-RED-CS0117-X3 / COMPILE0 / FOCUSED9 / RELATED249 / NTSD28-BROAD625 / SELFCHECK-PASS-2026-09-05T12:12:35Z / SCENE-UNCHANGED
-->

> 状态：`VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`

focused test已先写入，fresh compile得到3个预期`CS0117`。

- pure helper保持injury-double先执行、definition倍率优先、mode fallback、49/50 rounding、
  negative remainder及两处32位低位overflow语义。
- fresh compile 0 error；focused `9a7bca7442e1457aa1a0feba8114b497` 9/9；
  related `d0453fbe3ef34274bfbb661384d62d10` 249/249；精确NTSD28 broad
  `186b6bbafaeb4b6b9d16a60d0935e9e3` 625/625。
- BattleRuntimeSelfCheck `2026-09-05T12:12:35Z` PASS；Scene SHA
  `0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`、203477 bytes、
  mtime unchanged。production仍等待正式carrier。
