# NTSD28-B2-FUNCTION-KEY-ROUTE-CONTRACT-001 — 纯C# F1～F12 route契约

<!-- CHANGE-RECORD
id: NTSD28-B2-FUNCTION-KEY-ROUTE-CONTRACT-001
status: FOCUSED_TEST_PASS
change-kind: TEST_FIRST_BEHAVIOR
code-path: Assets/NTSD/Scripts/Simulation/Input/NTSD28NativeFunctionKeyRouter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeFunctionKeyRouterEditorTests.cs
authority: NTSD 2.8-Logan playable live native_function_keys.h route contract, crosswalk NTSD28-B2-FUNCTION-KEY-ROUTE-CROSSWALK-001.
evidence: TASK-CONTRACT-CREATED / TEST-FIRST-FRESH-123-CS0246-CS0103 / PURE-ROUTE-IMPLEMENTED / ENUM-VALUE-ORDER / VIRTUAL-KEY-0X70-0X7B / NINE-STAGE-PRIORITY / FULL-F1-F12-TABLE / REPEAT-BEFORE-MAINTENANCE / MAINTENANCE-BEFORE-CONTEXT / F11-F12-BYPASS / F6-F9-LOCK / F8-F9-DELAY / F10-NO-ACTION / COMPILE-0 / FOCUSED-8-OF-8-CEB90361 / RELATED-19-OF-19-0020DDD4 / WARM-4096-ZERO-ALLOC / FULL-SELFCHECK-2026-09-04-200121-PASS / EXPECTED-NEGATIVE-7-REVIEWED / CONSOLE-0 / LEDGER-154-106-PASS / PHYSICAL-AND-RUNTIME-UNCONNECTED / AUTHORITY-READ-ONLY
-->

> 状态：`FOCUSED_TEST_PASS / PURE-ROUTE-READY / PRODUCTION-UNCONNECTED / NEXT-SESSION-STATE-CARRIER`

## 改前职责

- Unity B1 Host latch直接处理F1/F2/F5；旧BattleFunctionKey latch直接处理allow-listed F7/F8/F9。
- 没有可独立测试的F1～F12 Authority route/result contract。

## 目标职责

- 新router只负责logical/virtual-key分类与拒绝结果；不读物理设备、不推进Host、不写World、不执行Session/effect。
- Editor tests冻结Authority优先级与全部命令表，后续adapter不得重新发明语义。

## 实际改动

- `NTSD28NativeFunctionKeyRouter.cs`：新增logical key、disposition、reject、Host/Session/maintenance enums，
  immutable modifiers/context/result及allocation-free router。
- `NTSD28NativeFunctionKeyRouterEditorTests.cs`：新增F1～F12完整表、virtual-key边界、repeat/maintenance/context、
  F11/F12、F10与4096次zero-allocation共8个测试。
- 没有修改现有B1 Host、旧FunctionKey latch、Runtime/Flow、Config或任何effect。

## 验证计划

1. missing type test-first compile red。
2. 实现pure router后refresh/compile。
3. 新focused tests、B1 Host相关tests、full SelfCheck、Console检查。
4. Change Ledger validator。

## 实际验证

- red：fresh 123条missing-type `CS0246/CS0103`；此前发现当前NUnit无`Assert.Multiple`后先修正测试壳，
  未把测试API不兼容算作行为red。
- compile：Unity refresh/domain reload完成，Console 0 error。
- focused：router `8/8`，job `ceb9036117174adeac78ed527543dfbb`。
- related：router + old FunctionKey + B1 Host policy `19/19`，job `0020ddd4225443b88f46c3f2dd44857f`。
- full SelfCheck：20:01:21 `PASS`；7条既有negative fixture日志审阅后清空，Console 0。
- validator：`PASSED / Records 154 / governed code files 106`。

## 未验证与边界

- 未接physical/runtime，故不需要也不能宣称F3～F12真实Play行为已对齐。
- 下一包建立Session state/event-byte carrier；production integration再后一个包。

## 回滚

删除本包两个新脚本及`.meta`；未接production，无迁移数据。
