# NTSD28-B6-CPOINT-THROW-ATOMIC-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B6-CPOINT-THROW-ATOMIC-PRODUCTION-001
status: SUPERSEDED
change-kind: TEST_FIRST
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6CpointThrowAtomicProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleGrabCpointLinkPlayModeProbeEditor.cs
authority: NTSD 2.8-Logan BattleWorld28::advance_catch_relations kind-1 throw tail; EXE B1E13AE1, closure 39DDDA15.
evidence: package opened and tests written; a provisional full-resource call compiled, then dependency re-audit proved authoritative baseMax/mode/child blockers remain; package superseded before any Unity test passed; successor removes the invalid call.
-->

> 状态：`SUPERSEDED / INVALID_FULL_RESOURCE_SCOPE / NO_VERIFICATION`

## 改前事实

- `ApplyThrow()` 对正 `ThrowInjury` 写 `victim.WeaponCount`，没有执行 Authority resource transfer，
  也没有写 environment injury/source。
- `ApplyThrow()` 在读取 depth input 前无条件 `victim.Runtime.Vz=0`，与 Authority 的 preserve-on-
  nonexclusive-input 语义相反。
- 当前 SelfCheck 与 Play probe把上述两项旧行为写成通过条件，必须由同一 Change 原子纠正。

## 预期实现

- 复用 B5 已验证 resource helper；不复制算法。
- 正 injury：resource transfer在前，environment两字段在后；WeaponCount不变。
- Vz仅在 up/down异或为真时覆盖。
- 不改其他 CPoint、hit、held、Scene或内容行为。

## 验证记录

- Unity batch RED与focused均未进入测试框架：Licensing IPC两次拒绝并以199终止，无 XML。
- `Assembly-CSharp.csproj` 与临时纳入新测试文件的 `Assembly-CSharp-Editor.csproj` 均为0 error；
  这只证明语法/引用，不证明行为。
- 编译后重读 `NTSD28-B5-RESOURCE-PRODUCTION-READINESS-AUDIT-001/002`，确认完整 MP transaction
  仍被 authoritative baseMax、selected-mode override 与 child suppression 阻塞。当前 provisional
  call 使用 `MPMax`，不能保留。
- 本包因此 `SUPERSEDED`，无绿灯、无行为完成声明。后继 Change 负责移除 invalid call并实现可精确子集。
