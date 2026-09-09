# NTSD28-B0-HP-BOUND-BINDINGS-001 — HP bound maturity promotion

<!-- CHANGE-RECORD
id: NTSD28-B0-HP-BOUND-BINDINGS-001
status: FOCUSED_TEST_PASS
code-path: Tools/NTSD28Parity/EntityFieldContract.cs
code-path: Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
authority: NTSD 2.8-Logan EntityState28 effective_max_hp/base_max_hp spawn, damage, environment, recovery, fusion, queued revival and ordinary revival writer chains; Unity NTSDEntityRuntime HPBound/HP3 corresponding producer and consumer chains.
evidence: SOURCE-CHAIN-CLOSED / EXISTING-PROJECTION-VALUE-CORRECT / BUILD-0-WARN-0-ERROR / SELFTEST-21-OF-21 / RAW-SELFTEST-5-OF-5 / FORMAT-PASS / UNITY-COMPILE-0 / JOINT-6-OF-6 / HPBOUND-480-HP3-500-PROJECTION / MATURITY-35-5-7 / REAL-BASELINE-9-38-54-UNCHANGED / INCIDENTAL-FULL-1601-WITH-10-EXTERNAL-FAILURES / GLOBAL-LEDGER-82-RECORDS-14-FILES-PASS / SCOPED-DIFF-CHECK-PASS / NO-PRODUCTION-WRITE-CHANGE
-->

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / UNITY-COMPILE-0 / JOINT-6-OF-6`

既有 projection 已读 `HPBound/HP3`，本包只把 source-confirmed candidates 晋级 VERIFIED，并补
`480/500` 互异断言。回滚恢复 candidate 与 `33/7/7`；不改 production。

## 实际结果

- maturity `35/5/7`；contract SHA `112C0B23A91B9A6F470B924906D8D6583844E8B6F90A1DBB106DE33D6E582FCF`。
- 工具build 0/0、21/21、5/5、format PASS；Unity compile 0；job
  `b050dd2519714976b62eb10f7c1b05bc` 6/6 PASS，HPBound480/HP3=500已测。
- focused raw SHA `D1628540F66B2996DA53E83AA4179FFCA9ECE736B4556691EF0E24E50378E1D5`；
  真实baseline仍为9 differences / 38 equal / 54 occurrences。
- 首次未嵌套过滤参数导致全量1601项运行，10项任务外失败；目标测试无失败，后续精确6/6通过。
- production与DAT未改。
- Ledger validator 82 records / 14 governed code files PASS；scoped diff check PASS。
