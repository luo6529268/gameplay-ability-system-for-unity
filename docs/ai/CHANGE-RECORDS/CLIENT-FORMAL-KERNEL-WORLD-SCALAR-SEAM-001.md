# CLIENT-FORMAL-KERNEL-WORLD-SCALAR-SEAM-001 — World Scalar Source Seam

<!-- CHANGE-RECORD
id: CLIENT-FORMAL-KERNEL-WORLD-SCALAR-SEAM-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Simulation/BattleRuntimeState.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleWorldScalarState.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleWorldScalarStateSeamEditorTests.cs
authority: User governed standing Client authorization; Server Cut E boundary audit and Task/Change; C++ release live GameWorld/game_tick/bootstrap/stage scalar owners.
evidence: USER_STANDING_AUTHORIZED / CLIENT_INTEGRATION_REQUIRED / FOCUSED_TEST_PASS / WORLD_SCALAR_SEAM_READY / GOVERNANCE_CLOSED / SOURCE_SEAM_ONLY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED
-->

> 状态：`FOCUSED_TEST_PASS / WORLD_SCALAR_SEAM_READY / GOVERNANCE_CLOSED / USER_STANDING_AUTHORIZED / CLIENT_INTEGRATION_REQUIRED / SOURCE_SEAM_ONLY / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`

## 1. 改前事实

五个BCL-only public types仍位于混合`BattleRuntimeState.cs`；Roster/bootstrap/root/content/entity等阻塞类型保持原位。

## 2. 计划与证据

按Server Task先写structural/behavior test并取得owner-file absent红灯，再只拆五个定义。Fresh job `8f3b78ebafda4def8e93d9ea52cd94d7`为4 pass/1 expected fail；唯一失败是新owner source尚不存在，无其他失败。

## 3. 实际修改与验证

- 新增`BattleWorldScalarState.cs`（GUID `e0f9b4d3565d4d3ca51d56a707df7b98`），仅迁移声明的五个类型；混合文件继续持有Roster/Results/stage campaign/root等排除内容。
- public API、字段、默认值、方法体和调用者保持不变；未改package/manifest/lock/asmdef/checksum/snapshot/recovery/battle behavior/marker。
- Unity compile `error CS=0`；focused job `03a44939601a44c0b798f3270b74cae0=5/5`；相关回归job `34cc470c4004414ab9c36b250eca6bbd=83/83`；fresh SelfCheck于20:26:09为`PASS`。
- 当前只达到source seam focused ready；shared owner、Play Mode、Android/Windows真实对局、formal marker、S0/S5 verification均未完成。
