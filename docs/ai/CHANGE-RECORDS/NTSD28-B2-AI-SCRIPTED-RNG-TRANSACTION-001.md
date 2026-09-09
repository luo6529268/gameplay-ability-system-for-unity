# NTSD28-B2-AI-SCRIPTED-RNG-TRANSACTION-001 — scripted AI RNG candidate transaction

<!-- CHANGE-RECORD
id: NTSD28-B2-AI-SCRIPTED-RNG-TRANSACTION-001
status: FOCUSED_TEST_PASS
change-kind: CODE_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Ai/Snapshots/AiDecisionSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/Kernel/AiDecisionKernel.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28AiScriptedRandomTransactionEditorTests.cs
authority: NTSD 2.8-Logan NativeAi28::step_scripted_target sites 0x11/0x12 and synchronized cursor commit contract.
evidence: TEST-FIRST-19-EXPECTED-COMPILE-ERRORS / FOCUSED-6-OF-6-JOB-2E936D4CD63A47C3AAE843496D512DDF / RELATED-AI-178-OF-178-JOB-CA3F1C6AB5524A4BB73D3DD3A6C9ECC2 / SITES-0X11-0X12 / SNAPSHOT-WITNESS-CANDIDATE / EXPLICIT-COMMIT / STALE-REJECT / WARM-4096-ZERO-ALLOC / SELFCHECK-PASS-2026-09-03T09-18-37 / CONSOLE-0 / LEDGER-120-RECORDS-67-FILES / PRODUCTION-CAPTURE-UNCHANGED / OTHER-AI-BRANCHES-UNCHANGED
-->

> 状态：`FOCUSED_TEST_PASS / SCRIPTED_TRANSACTION_READY / PRODUCTION_UNCONNECTED`

## 原状

- scripted coordinate的两个Unity表达式局部同构，但仍使用无site CRT clone。
- snapshot/witness没有synchronized cursor carrier，无法将被采纳kernel的候选状态交回owner。
- production捕获仍只写legacy `RngState/RngCalls`；本包明确不切换。

## 计划

- snapshot/witness增加内部candidate cursor与reset/copy合同，并加入call-site trace数组；
- kernel只在snapshot显式携带cursor时构造synchronized stream；scripted左右site分别固定`0x11/0x12`；
- 其余未迁移`.Rand(int)`在同步模式继续fail closed，防止越界使用。

## Test-first

- Unity refresh取得19个预期编译错误，全部来自新测试所需snapshot/witness cursor carrier、
  call-site trace和transaction API；无其他编译错误。

## 实际改动

- `AiDecisionSnapshot`增加预分配call-site trace和内部candidate cursor，`ResetOwned`清空、
  `CopyOwnedFrom`按值复制；`AiDecisionWitness`可返回被kernel推进后的cursor。
- kernel在snapshot显式带cursor时才构造synchronized stream，否则保持legacy CRT；
  `MoveTowardCoordinate`左右远距分别用`0x11/0x12`，legacy mode通过同一helper保留旧CRT结果。
- `Publish`返回推进后的candidate；pre-evaluation reject返回未推进candidate。kernel不直接写owner。
- 新增6个测试覆盖左右site、near no-consume、copy/reset、显式commit、stale与4096 zero-allocation。

## 验证

- focused job `2e936d4cd63a47c3aae843496d512ddf`：6/6。
- stream + 全AI decision/character/shadow job `ca3f1c6ab5524a4bb73d3dd3a6c9ecc2`：178/178。
- 完整SelfCheck：09:18:37 `PASS`；清理预期负例后Console error 0。
- Change Ledger：120 records、67 governed code files；`git diff --check`无本包错误。

## 未关闭项

- production snapshot尚不携带NativeRandom cursor，writer也未提交candidate；真实战斗仍走旧CRT。
- 只有scripted path的`0x11/0x12`可安全使用同步模式；其他未迁移分支会按seam合同fail closed。
