# NTSD28-B4-F04-IDENTITY-X-EXTRAS-001 — independent identity X extras

<!-- CHANGE-RECORD
id: NTSD28-B4-F04-IDENTITY-X-EXTRAS-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR_ALIGNMENT
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Weapon.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4IdentityXExtrasEditorTests.cs
authority: NTSD 2.8-Logan physics_integrator.cpp independent type4/OID120 add and OID101 subtract operations; EXE B1E13AE1, closure 39DDDA15.
evidence: TEST-FIRST-RED-3-OF-6 / COMPILE0 / FOCUSED6 / RELATED44 / NTSD28-BROAD476 / SELFCHECK-PASS-2026-09-05T05:27:46Z / SCENE-UNCHANGED / CONSOLE0 / LEDGER-PASS
-->

> 状态：`VERIFIED / INDEPENDENT_IDENTITY_EXTRAS / FLOOR_LANDING_PENDING`

回滚删除kernel/test并恢复derived branch；不涉及数据迁移。

正确注入alias resolver后的red job `b99258aaaebd4e878c0ef0d6efab0d39`为3/6：type4+101未抵消、
real120与real101被遗漏。现已建立single kernel并由shared/derived路径共同调用。

## 验证与边界

- compile0；focused `edeca97b2e9a42c38d81b276e89ff23a` 6/6；related
  `e85ba91ae4524c76b814170221cefd9d` 44/44；broad
  `917ab25a602849ada40f7bc0d86e45be` 476/476；SelfCheck 05:27:46Z PASS。
- Scene`0D74E174...D77 / 203477 / 2026-09-04T13:12:45Z`、Console0、diff-check、Ledger
  219/197 PASS。
- non-character effective floor/contact result、landing、gravity、OID999、producer与Audio仍未关闭。
