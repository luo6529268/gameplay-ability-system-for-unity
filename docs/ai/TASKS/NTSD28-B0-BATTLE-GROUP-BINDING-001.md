# Task Contract — NTSD28-B0-BATTLE-GROUP-BINDING-001

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / UNITY-COMPILE-0 / JOINT-6-OF-6 / REAL-BASELINE-UNCHANGED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B0`  
> 建立日期：2026-09-03

## 目标

更正 `identity.battleGroup` 的 Unity diagnostic projection：从 `NTSDEntityRuntime.Team` 改为
`NTSDEntityRuntime.RelationTeam`，并由 candidate 晋级 VERIFIED。

权威 `EntityState28::battle_group` 对应 native `Entity+0x364`，会被 join/mimic 临时覆盖，参与
同组/敌对、复活、融合、碰撞与结果分组；Unity 这些关系消费者及临时写入链使用 `RelationTeam`。

## 允许文件

- `Tools/NTSD28Parity/EntityFieldContract.cs`
- `Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs`
- 本 Task/Change、Ledger、STATE、handoff、总表

## 不变量与验收

- 只改 diagnostic contract/projection/测试；production Team/RelationTeam writer与消费者不改。
- maturity `35/5/7 -> 36/4/7`；公共neutral因Team==RelationTeam，真实差异 `9/38/54` 应保持。
- focused设置 `Team=11`、`RelationTeam=13`，断言battleGroup=13且不是11。
- 工具build0/0、21/21、5/5、format；Unity compile0、联合6/6；Ledger/diff通过。

## 回滚

恢复旧Team candidate与maturity `35/5/7`；不触碰production relation状态。

## 实际结果

- contract/projection更正为RelationTeam并晋级VERIFIED，maturity `36/4/7`；contract SHA
  `4E290FF0890FA6CEF5245F16891F8E156FAB78A64CF7FD81606FF6841E78E75A`。
- 工具build0/0、21/21、5/5、format PASS；Unity compile0；job
  `6bc4214b1894458a85cbb8bab998577c` 6/6 PASS，Team11/RelationTeam13投影为battleGroup13。
- Unity raw SHA `FDD8F748C7FE451CD487275F3042DB218D0A17F22CC848D26A28C2D92AB2E05D`；
  neutral真实差异保持9/equal38/occurrence54，首差异baseMaxMp。
- production relation/team、DAT、资源与Scene未改。
- Change Ledger validator 83 records / 14 governed code files PASS；scoped diff check PASS（仅既有LF→CRLF提示）。
