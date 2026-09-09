# CHANGE-LEDGER-CPP-COVERAGE-001 — C/C++ authored code 审计覆盖

<!-- CHANGE-RECORD
id: CHANGE-LEDGER-CPP-COVERAGE-001
status: VERIFIED
code-path: Tools/Validate-ChangeLedger.ps1
authority: Root AGENTS.md section 13.1 truthful authored-code audit; NTSD28-B0-AUTHORITY-SOURCE-CAPTURE-001 introduces a workspace-owned C++ runner.
evidence: POWERSHELL-PARSE-PASS / CPP-ACTUAL-DIFF-COVERED / SYNTHETIC-UNRECORDED-CPP-FAIL-CLOSED / GLOBAL-LEDGER-PASS-67-RECORDS-9-GOVERNED-DIFFS / NO-UNITY-RUNTIME-RESOURCE-OR-AUTHORITY-WRITE
-->

> 状态：`VERIFIED / GLOBAL-LEDGER-PASS / CPP-NEGATIVE-PASS`

只向 `$ScriptExtensions` 增加 `.c/.cc/.cpp/.cxx/.h/.hpp`；不改变 governed roots、排除目录、
Record 语义或任何运行时行为。

实际结果：PowerShell parse 通过；当前 capture `.cpp` 被准确覆盖；模拟
`Tools/SyntheticUnrecorded.cpp` 正确报告 `Unrecorded authored script diff`；全局 validator
`67 records / 9 governed code diffs / PASS`。无 runtime/resource/authority 写入。
