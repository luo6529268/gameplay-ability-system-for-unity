# NTSD28-B0-OWNER-SLOT-BINDING-001 — owner slot maturity promotion

<!-- CHANGE-RECORD
id: NTSD28-B0-OWNER-SLOT-BINDING-001
status: FOCUSED_TEST_PASS
code-path: Tools/NTSD28Parity/EntityFieldContract.cs
code-path: Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
authority: NTSD 2.8-Logan EntityState28 owner_slot / native Entity+0x354 spawn root-owner copy, relation transfer, attribution and self-injury consumers; Unity NTSDEntityRuntime OwnerSlotIndex equivalent physical-slot producer/consumer chain, distinct from stable/spawner/relation-owner/holder fields.
evidence: SOURCE-CHAIN-CLOSED / EXISTING-PROJECTION-VALUE-CORRECT / BUILD-0-WARN-0-ERROR / SELFTEST-21-OF-21 / RAW-SELFTEST-5-OF-5 / FORMAT-PASS / UNITY-COMPILE-0 / JOINT-6-OF-6 / OWNERSLOT-17-DISTINCT-FROM-19-21-23 / MATURITY-38-2-7 / REAL-BASELINE-9-38-54-UNCHANGED / GLOBAL-LEDGER-85-RECORDS-14-FILES-PASS / SCOPED-DIFF-CHECK-PASS / NO-PRODUCTION-WRITE-CHANGE
-->

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / UNITY-COMPILE-0 / JOINT-6-OF-6`

既有projection已读OwnerSlotIndex；本包只晋级VERIFIED并补互异引用断言。回滚恢复candidate；不改production。

## 实际结果

- maturity38/2/7；contract SHA `992D4821D6CCB482CE2749A28EE2916D9660AB88299760179C0397AB1A53B0E5`。
- 工具build0/0、21/21、5/5、format PASS；Unity compile0；job
  `b4b2472b4c85491291a05d28bc6978cd` 6/6 PASS，17与19/21/23已分离。
- raw SHA `7EB8DF9C131B0DD7C229B9C70493B78E20C68C278E0CA4878241C4AF0B6728F9`；
  真实baseline仍为9 differences / 38 equal / 54 occurrences。
- production未改。
- Ledger validator 85 records / 14 governed code files PASS；scoped diff check PASS。
