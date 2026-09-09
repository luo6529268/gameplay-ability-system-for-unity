# NTSD28-B0-ENVIRONMENT-STATE-BINDING-CORRECTION-001 — environment state correction

<!-- CHANGE-RECORD
id: NTSD28-B0-ENVIRONMENT-STATE-BINDING-CORRECTION-001
status: FOCUSED_TEST_PASS
code-path: Tools/NTSD28Parity/EntityFieldContract.cs
code-path: Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
authority: NTSD 2.8-Logan EntityState28 environment_state_320 / native Entity+0x320 input redirect, state12/18 physics/environment damage, kind4/kind11 and attribution consumers; Unity Unk328 is separately proven OID51/52 fusion state and no current +0x320 equivalent exists.
evidence: SOURCE-CHAIN-CLOSED / EXISTING-UNK328-CANDIDATE-CORRECTED / UNITY-BINDING-MISSING / BUILD-0-WARN-0-ERROR / SELFTEST-21-OF-21 / RAW-SELFTEST-5-OF-5 / FORMAT-PASS / UNITY-COMPILE-0 / JOINT-6-OF-6 / UNK328-MINUS3-NOT-LEAKED / MATURITY-38-1-8 / REAL-BASELINE-9-38-54-CLASSIFICATION-CORRECTED / GLOBAL-LEDGER-86-RECORDS-14-FILES-PASS / SCOPED-DIFF-CHECK-PASS / PRODUCTION-IMPLEMENTATION-DEFERRED-B4-B5
-->

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / UNITY-COMPILE-0 / JOINT-6-OF-6`

本包只把错误candidate改为missing/null，并以Unk328=-3防泄漏；不新增production字段。

## 实际结果

- maturity38/1/8；contract SHA `5B2FC535E7BFBE827CF55B36D326FBCB93870A076F471D3960C0EA93DCB62B8A`。
- 工具build0/0、21/21、5/5、format PASS；Unity compile0；job
  `a86feda5f97e4e2a9f4423d0cd2ba22a` 6/6 PASS，-3未泄漏。
- raw SHA `1BEF3981B70D33214B96B96BC07EDD4E5DD02A5E0A075D141E7CF1CDE65768E6`；
  真实baseline仍9/38/54，environmentState为missing。
- production未改；实现留B4/B5。
- Ledger validator 86 records / 14 governed code files PASS；scoped diff check PASS。
