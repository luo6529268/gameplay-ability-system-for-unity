# NTSD28-B0-LIFECYCLE-PENDING-BINDING-CORRECTION-001 — lifecycle pending correction

<!-- CHANGE-RECORD
id: NTSD28-B0-LIFECYCLE-PENDING-BINDING-CORRECTION-001
status: FOCUSED_TEST_PASS
code-path: Tools/NTSD28Parity/EntityFieldContract.cs
code-path: Tools/NTSD28Parity/TraceContractSelfTest.cs
code-path: Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
authority: NTSD 2.8-Logan EntityState28 lifecycle_resolution_pending plus lifecycle_code terminal-frame producer, encoded-reset survivor and despawn consumer; Unity PendingFlushDestroy is a broader direct-release flag with no lifecycle-code/encoded-reset equivalent.
evidence: SOURCE-CHAIN-CLOSED / EXISTING-PENDINGFLUSHDESTROY-CANDIDATE-CORRECTED / UNITY-BINDING-MISSING / FIRST-SELFTEST-20-OF-21-EXPOSED-STALE-CANDIDATE-NONEMPTY-GUARD / FINAL-BUILD-0-WARN-0-ERROR / FINAL-SELFTEST-21-OF-21 / RAW-SELFTEST-5-OF-5 / FORMAT-PASS / UNITY-COMPILE-0 / JOINT-6-OF-6 / PENDINGFLUSHDESTROY-TRUE-NOT-LEAKED / MATURITY-38-0-9 / REAL-BASELINE-10-37-60-CLASSIFICATION-CORRECTED / GLOBAL-LEDGER-87-RECORDS-14-FILES-PASS / SCOPED-DIFF-CHECK-PASS / PRODUCTION-IMPLEMENTATION-DEFERRED-B7
-->

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / UNITY-COMPILE-0 / JOINT-6-OF-6`

本包只把错误candidate改为missing/null，并以PendingFlushDestroy=true防泄漏；不改production。

## 实际结果

- maturity38/0/9；contract SHA `629E209A52CA8494E5360814CE5E493D4A3C820482861F9C445A0F3F2EC1D314`。
- 首次self-test 20/21暴露旧candidate非空guard；修正后build0/0、21/21、5/5、format PASS。
- Unity compile0；job `d4bc39d450b94265b18d0e4214140bd4` 6/6，true未泄漏。
- raw SHA `A9F6A28674301F5AD1E0F29D5484F457EF4D07983940E4AEE46DBBFFFBEBE09E`；
  真实baseline为10 differences / 37 equal / 60 occurrences。
- production未改；实现留B7。
- Ledger validator 87 records / 14 governed code files PASS；scoped diff check PASS。
