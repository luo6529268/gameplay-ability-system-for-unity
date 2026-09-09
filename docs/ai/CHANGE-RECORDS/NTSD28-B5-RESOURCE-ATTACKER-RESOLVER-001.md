# NTSD28-B5-RESOURCE-ATTACKER-RESOLVER-001 — two-hop resource attacker resolver

<!-- CHANGE-RECORD
id: NTSD28-B5-RESOURCE-ATTACKER-RESOLVER-001
status: VERIFIED
change-kind: TEST_FIRST_SLOT_ATTRIBUTION_RESOLVER
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5ResourceAttackerResolverEditorTests.cs
authority: NTSD 2.8-Logan BattleWorld28::resolve_native_hit_resource_attacker; EXE B1E13AE1, closure 39DDDA15.
evidence: RESOURCE-CARRIER-ATTRIBUTION-AUDIT-VERIFIED / OWNER-SLOT-B0-BINDING-VERIFIED / TEST-FIRST-COMPILE-RED-CS0117-X8 / COMPILE0 / FOCUSED7 / RELATED302 / NTSD28-BROAD645 / SELFCHECK-PASS-2026-09-05T13:03:42Z / SCENE-UNCHANGED
-->

> 状态：`VERIFIED / RESOLVER_READY / PRODUCTION_UNCONNECTED`

focused test已先写入，fresh compile得到8个预期`CS0117`。

- active physical slot起点、negative/self terminal、一跳/两跳、第三跳停止全部闭合。
- first/second declared owner缺失均fail closed；stable/holder/relation字段不能重定向。
- fresh compile 0 error；focused `d95507dbe5784cdfa27741b45aea8ad2` 7/7；
  related `3a2305e3ce264e45900b76fccf58daa6` 302/302；精确NTSD28 broad
  `c3b4c51aec5c42aaba4c6cd005bbfe75` 645/645。
- BattleRuntimeSelfCheck `2026-09-05T13:03:42Z` PASS；Scene SHA
  `0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`、203477 bytes、
  mtime unchanged。production仍等待world rules与B11字段。
