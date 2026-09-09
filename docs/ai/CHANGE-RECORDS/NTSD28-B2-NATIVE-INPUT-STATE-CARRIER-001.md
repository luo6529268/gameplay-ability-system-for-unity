# NTSD28-B2-NATIVE-INPUT-STATE-CARRIER-001 — per-entity exact native input carrier

<!-- CHANGE-RECORD
id: NTSD28-B2-NATIVE-INPUT-STATE-CARRIER-001
status: FOCUSED_TEST_PASS
change-kind: SCRIPT_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Input/NTSD28InputProxyBlock.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldEntityRuntimeSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeInputStateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleWorldEntityRuntimeSnapshotEditorTests.cs
authority: NTSD 2.8-Logan Entity input_state.h block, battle_world.h proxy control fields, snapshot/reset semantics and current Unity canonical runtime copy contracts.
evidence: TEST-FIRST-46-EXPECTED-CS1061 / COMPILE-PASS / FOCUSED-JOB-045F-5-OF-5 / RELATED-JOB-DA0D-38-OF-38 / POOLED-JOB-25B8-23-OF-23 / FULL-SELFCHECK-PASS-20260903-063420 / CONSOLE-ERROR-0-AFTER-EXPECTED-NEGATIVE-FIXTURE-CLEAR / PRODUCTION-WRITERS-EXCLUDED / LEGACY-INPUT-FIELDS-PRESERVED
-->

> 状态：`FOCUSED_TEST_PASS / CARRIER_READY / WRITERS_UNCONNECTED`

## 实际修改

- `NTSD28InputProxyBlock`新增无分配清零与canonical storage守卫。
- `NTSDEntityRuntime`新增每实例exact block、`InputProxyCounter14C`、`InputProxySourceSlot178`、
  `InputProxyEnabled17C`；`ResetInputState`只清block，完整`Reset`再清三个control。
- canonical copy深拷贝block且复制controls；entity runtime snapshot schema 1→2、aggregate snapshot 3→4、
  checksum schema 6→7，并按exact 0x21物理顺序纳入checksum。
- 全字段反射快照守卫识别并逐字段比较block，同时断言目标block不与源alias。

## 验证证据

- test-first：46个预期`CS1061`，无范围外编译错误。
- compile：实现后Unity Console编译Error 0。
- focused：job `045f4d3c0c35488eadb2d5cdbf1c7a95`，5/5。
- related：job `da0d9981526b4d5fbd09042fc87fab2b`，38/38。
- pooled/zero-allocation：job `25b807c5faa946229652fde96e470be7`，23/23。
- full SelfCheck：2026-09-03 06:34:20 `PASS`；预期负向夹具日志清除后Console Error 0。

## 未关闭边界

本包只完成状态所有权、快照与确定性覆盖；native combo桥、proxy control lifecycle、AI/input两遍pass和
authority/Unity joint trace必须由后续独立包闭合，不能据此宣称B2或战斗输入已完全对齐。
