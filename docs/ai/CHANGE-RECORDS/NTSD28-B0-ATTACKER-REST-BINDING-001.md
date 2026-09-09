# NTSD28-B0-ATTACKER-REST-BINDING-001 — attacker rest maturity promotion

<!-- CHANGE-RECORD
id: NTSD28-B0-ATTACKER-REST-BINDING-001
status: FOCUSED_TEST_PASS
code-path: Tools/NTSD28Parity/EntityFieldContract.cs
code-path: Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
authority: NTSD 2.8-Logan EntityState28::attacker_rest arest/vrest writers, candidate gate, current-frame clear and motion-hold-aware decrement; Unity NTSDEntityRuntime::AttackExempt equivalent producer/consumer chain.
evidence: SOURCE-CHAIN-CLOSED / EXISTING-PROJECTION-VALUE-CORRECT / BUILD-0-WARN-0-ERROR / SELFTEST-21-OF-21 / RAW-SELFTEST-5-OF-5 / FORMAT-PASS / UNITY-COMPILE-0 / JOINT-6-OF-6 / ATTACKEXEMPT-5-PROJECTION / MATURITY-33-7-7 / REAL-BASELINE-9-38-54-UNCHANGED / GLOBAL-LEDGER-81-RECORDS-14-FILES-PASS / SCOPED-DIFF-CHECK-PASS / NO-PRODUCTION-WRITE-CHANGE
-->

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / UNITY-COMPILE-0 / JOINT-6-OF-6`

现有 projection 已读 `AttackExempt`，本包只把 source-confirmed candidate 晋级 VERIFIED，并补非零断言。
回滚恢复candidate与32/8/7；不改production。

## 实际结果

- maturity33/7/7；contract SHA `F0D6EA067FC14A3C7949AA8A5F3776F6260177623741AC04DA911E0053700FEA`。
- 工具build0/0、21/21、5/5、format PASS；Unity compile0；job
  `d43cf6e368094a6c81d2151055a9e787` 6/6 PASS，AttackExempt5已测。
- raw SHA `B9CFC67D8429527609F7C1CD7954C35B7A6219E84D5A17D2C981565BC11A7E87`；
  真实baseline仍为9 differences / 38 equal / 54 occurrences。
- production未改。
- Ledger validator 81 records / 14 governed code files PASS；scoped diff check PASS。
