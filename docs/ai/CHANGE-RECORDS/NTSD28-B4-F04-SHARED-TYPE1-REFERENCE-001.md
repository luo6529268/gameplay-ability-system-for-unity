# NTSD28-B4-F04-SHARED-TYPE1-REFERENCE-001 — shared type1 reference landing

<!-- CHANGE-RECORD
id: NTSD28-B4-F04-SHARED-TYPE1-REFERENCE-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR_ALIGNMENT
code-path: Assets/NTSD/Scripts/Animation/Character/CharacterMechanics.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4SharedType1ReferenceEditorTests.cs
authority: NTSD 2.8-Logan PhysicsIntegrator28 non-character core and type1 landing predicates; EXE B1E13AE1, closure 39DDDA15.
evidence: INITIAL-RED2-INVALID-FALLBACK-TYPE5 / FIXTURE-CORRECTED-TO-TYPE1-OVERRIDE / GREEN2 / FOCUSED4 / RELATED48 / NTSD28-BROAD480 / SELFCHECK-PASS-2026-09-05T05:47:11Z / SCENE-UNCHANGED / CONSOLE0 / LEDGER-PASS
-->

> 状态：`VERIFIED / SHARED_TYPE1_REFERENCE / DERIVED_AND_OTHER_TYPES_PENDING`

回滚删除result seam/test并恢复shared type1旧bool路径；无数据迁移。

初次red job `30ac14f7db854011bfbce9cc4106e291`虽然2/2失败，但fixture实际走CLR fallback type5，
不能作为type1红灯证据，已明确作废。fixture改为override current DAT type1后，现实现job
`83453150d266427ea699b73e8f80564b` 2/2通过；新增pure result与warm 0-allocation测试待复跑。
无分配result core仅迁shared type1；旧bool/derived/其他type保持。

最终 focused job `e5b135f1c8354c29836b96a5e3800c5a` 4/4，覆盖 pure result、exact contact、
penetration landing 与 4096 次 warm 0 B；related job `223bd68ec20840a99ba825d926cdee43`
48/48，broad job `36ceb856e86c4910851544da228e7ff4` 480/480。SelfCheck
`2026-09-05T05:47:11Z` PASS，Console 0 error，Scene 基线未变化。初次无效 red 仍保留为
审计事实，但不据此声称有效 test-first red；本包凭 authority、focused、related、broad 与
SelfCheck 证据闭合。
