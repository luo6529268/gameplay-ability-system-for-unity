# NTSD28-B0-PARTICIPANT-CLASS-BINDING-001 — participant class maturity promotion

<!-- CHANGE-RECORD
id: NTSD28-B0-PARTICIPANT-CLASS-BINDING-001
status: FOCUSED_TEST_PASS
code-path: Tools/NTSD28Parity/EntityFieldContract.cs
code-path: Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
authority: NTSD 2.8-Logan EntityState28 participant_class_344 / native Entity+0x344 spawn, OID122/123 boundary, death/blink, damage/kill stats and result classification consumers; Unity NTSDEntityRuntime Unk344 equivalent producer/consumer chain.
evidence: SOURCE-CHAIN-CLOSED / EXISTING-PROJECTION-VALUE-CORRECT / BUILD-0-WARN-0-ERROR / SELFTEST-21-OF-21 / RAW-SELFTEST-5-OF-5 / FORMAT-PASS / UNITY-COMPILE-0 / JOINT-6-OF-6 / UNK344-4-PROJECTION / MATURITY-37-3-7 / REAL-BASELINE-9-38-54-UNCHANGED / GLOBAL-LEDGER-84-RECORDS-14-FILES-PASS / SCOPED-DIFF-CHECK-PASS / NO-PRODUCTION-WRITE-CHANGE
-->

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / UNITY-COMPILE-0 / JOINT-6-OF-6`

既有projection已读Unk344；本包只晋级VERIFIED并补非零4断言。回滚恢复candidate；不改production。

## 实际结果

- maturity37/3/7；contract SHA `704A1ED376DD46B8E2992EB6A747C4A24A67E5E9E548A07D3038AD242AE88553`。
- 工具build0/0、21/21、5/5、format PASS；Unity compile0；job
  `e879e34cb57b4f4291d1b33d01d71f9e` 6/6 PASS，非零4已测。
- raw SHA `8DA9F2B65B6409200B8C20DD3971A950139DF506605D15F94261FF8A741C79FB`；
  真实baseline仍为9 differences / 38 equal / 54 occurrences。
- production未改。
- Ledger validator 84 records / 14 governed code files PASS；scoped diff check PASS。
