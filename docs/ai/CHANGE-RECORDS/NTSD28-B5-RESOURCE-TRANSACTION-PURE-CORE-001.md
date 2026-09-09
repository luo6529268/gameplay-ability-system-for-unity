# NTSD28-B5-RESOURCE-TRANSACTION-PURE-CORE-001 — resource transaction pure core

<!-- CHANGE-RECORD
id: NTSD28-B5-RESOURCE-TRANSACTION-PURE-CORE-001
status: VERIFIED
change-kind: TEST_FIRST_RESOURCE_TRANSACTION_CORE
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5ResourceTransactionPureCoreEditorTests.cs
authority: NTSD 2.8-Logan apply_native_hit_resource_transfer local MP transaction; EXE B1E13AE1, closure 39DDDA15.
evidence: HIT-RESOURCE-PREREQUISITE-AUDIT-VERIFIED / TRANSACTION-ORDER-READ / TEST-FIRST-COMPILE-RED-CS0117-X6 / COMPILE0 / FOCUSED7 / RELATED256 / NTSD28-BROAD632 / SELFCHECK-PASS-2026-09-05T12:26:10Z / SCENE-UNCHANGED
-->

> 状态：`VERIFIED / PURE_TRANSACTION_READY / PRODUCTION_UNCONNECTED`

focused test已先写入，fresh compile得到6个预期`CS0117`。

- pure transaction保持local gate、双方type0 reward、suppression==1仅抑制reward、target drain、
  resource-attacker negative/positive gain和`reward→drain→gain`顺序。
- full payment与baseMax gate、consumed total写入均已覆盖；不查询world/content。
- fresh compile 0 error；focused `229178ebf8614558a98875c2944e5462` 7/7；
  related `02c45715780a4977bf3fcf6af2c91637` 256/256；精确NTSD28 broad
  `0b0291f30d524c47a3bb1fc77c31cba1` 632/632。
- BattleRuntimeSelfCheck `2026-09-05T12:26:10Z` PASS；Scene SHA
  `0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`、203477 bytes、
  mtime unchanged。production等待正式carrier/attribution。
