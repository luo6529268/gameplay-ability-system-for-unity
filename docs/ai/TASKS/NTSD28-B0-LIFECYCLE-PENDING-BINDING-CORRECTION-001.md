# Task Contract — NTSD28-B0-LIFECYCLE-PENDING-BINDING-CORRECTION-001

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / UNITY-COMPILE-0 / JOINT-6-OF-6 / REAL-CLASSIFICATION-CORRECTED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B0`  
> 建立日期：2026-09-03

## 目标

纠正 `lifecycle.resolutionPending` 的错误 candidate。权威 pending 只由 terminal frame code 置位，
与 `lifecycle_code` 成对，并支持11xx/12xx encoded reset后存活；Unity `PendingFlushDestroy` 是更广的
直接释放标志，无 lifecycle code/encoded reset 等价窗口。当前应标为 MISSING/null，生产实现留B7。

## 允许文件

- `Tools/NTSD28Parity/EntityFieldContract.cs`
- `Tools/NTSD28Parity/TraceContractSelfTest.cs`
- `Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs`
- 本 Task/Change、Ledger、STATE、handoff、总表

## 不变量与验收

- 只改diagnostic contract/projection/测试；不改PendingFlushDestroy或任何lifecycle生产逻辑。
- maturity `38/1/8 -> 38/0/9`；neutral预期 `10 differences/equal37/occurrence60`。
- focused设置PendingFlushDestroy=true，断言resolutionPending=null且不泄漏true。
- binding inventory自检允许candidate=0，同时继续强制47字段唯一/strict/合法maturity且missing非空。
- 工具build0/0、21/21、5/5、format；Unity compile0、联合6/6；Ledger/diff通过。

## 回滚

恢复错误PendingFlushDestroy candidate仅用于回退本包；不得重新宣称等价。

## 实际结果

- contract改为MISSING/none，projection改为null，maturity38/0/9；contract SHA
  `629E209A52CA8494E5360814CE5E493D4A3C820482861F9C445A0F3F2EC1D314`。
- 首次通用self-test 20/21暴露“candidate必须非空”旧guard；本包扩展允许路径后删除该过期约束，
  最终工具build0/0、21/21、5/5、format PASS。
- Unity compile0；job `d4bc39d450b94265b18d0e4214140bd4` 6/6 PASS，PendingFlushDestroy=true不泄漏。
- Unity raw SHA `A9F6A28674301F5AD1E0F29D5484F457EF4D07983940E4AEE46DBBFFFBEBE09E`；
  neutral为10 differences/equal37/occurrence60，resolutionPending分类为UNITY_BINDING_MISSING。
- production lifecycle/slot release、DAT、资源与Scene未改；等价实现留B7。
- Change Ledger validator 87 records / 14 governed code files PASS；scoped diff check PASS（仅既有LF→CRLF提示）。
