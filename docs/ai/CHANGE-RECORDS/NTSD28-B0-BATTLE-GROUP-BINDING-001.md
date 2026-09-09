# NTSD28-B0-BATTLE-GROUP-BINDING-001 — battle group binding correction

<!-- CHANGE-RECORD
id: NTSD28-B0-BATTLE-GROUP-BINDING-001
status: FOCUSED_TEST_PASS
code-path: Tools/NTSD28Parity/EntityFieldContract.cs
code-path: Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
authority: NTSD 2.8-Logan EntityState28 battle_group / native Entity+0x364 spawn, join override/restore, relation, fusion, revival and result consumers; Unity NTSDEntityRuntime RelationTeam corresponding producer/consumer chain, distinct from Team.
evidence: SOURCE-CHAIN-CLOSED / EXISTING-TEAM-CANDIDATE-CORRECTED / BUILD-0-WARN-0-ERROR / SELFTEST-21-OF-21 / RAW-SELFTEST-5-OF-5 / FORMAT-PASS / UNITY-COMPILE-0 / JOINT-6-OF-6 / TEAM-11-RELATIONTEAM-13-PROJECTION / MATURITY-36-4-7 / REAL-BASELINE-9-38-54-UNCHANGED / GLOBAL-LEDGER-83-RECORDS-14-FILES-PASS / SCOPED-DIFF-CHECK-PASS / NO-PRODUCTION-WRITE-CHANGE
-->

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / UNITY-COMPILE-0 / JOINT-6-OF-6`

本包只把 diagnostic `battleGroup` 从Team更正到RelationTeam、晋级VERIFIED，并补11/13互异断言。
回滚恢复旧candidate；不改production。

## 实际结果

- maturity36/4/7；contract SHA `4E290FF0890FA6CEF5245F16891F8E156FAB78A64CF7FD81606FF6841E78E75A`。
- 工具build0/0、21/21、5/5、format PASS；Unity compile0；job
  `6bc4214b1894458a85cbb8bab998577c` 6/6 PASS，11/13互异投影通过。
- raw SHA `FDD8F748C7FE451CD487275F3042DB218D0A17F22CC848D26A28C2D92AB2E05D`；
  真实baseline仍为9 differences / 38 equal / 54 occurrences。
- production未改。
- Ledger validator 83 records / 14 governed code files PASS；scoped diff check PASS。
