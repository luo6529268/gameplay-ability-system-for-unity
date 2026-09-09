# NTSD28-B2-AI-SYNC-RNG-CURSOR-001 — synchronized RNG cursor seam

<!-- CHANGE-RECORD
id: NTSD28-B2-AI-SYNC-RNG-CURSOR-001
status: FOCUSED_TEST_PASS
change-kind: SCRIPT_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSD28NativeRandom.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeRandomEditorTests.cs
authority: NTSD 2.8-Logan NativeRandom28 synchronized table/counter/index/calls/call-site semantics; Unity authoritative/indexed/shadow AI evaluation ownership.
evidence: TEST-FIRST-20-CS0246-CS1061 / COMPILE-0 / FOCUSED-11-OF-11-2B92FF32 / RELATED-16-OF-16-0968464E / OWNER-CURSOR-5000-BIT-EXACT / COPY-ISOLATED / EXPLICIT-COMMIT / RESET-RESTORE-STALE-REJECTED / WARM-4096-ZERO-ALLOC / SELFCHECK-PASS-2026-09-03-083512 / CONSOLE-0 / CURSOR-UNCONNECTED / CONSUMERS-UNCHANGED
-->

> 状态：`FOCUSED_TEST_PASS / SYNCHRONIZED_CURSOR_READY / CONSUMERS_UNMIGRATED`

本包为后续AI call-site迁移建立共享只读table、独立scalar cursor与版本化提交门；不迁移任何consumer。

## 实际改动

- 新增值类型`NTSD28SynchronizedRandomCursor`，共享owner当前3001-byte table引用，仅复制
  counter/index/calls/last-call-site scalar。
- `CaptureSynchronizedCursor`不改变owner；cursor copy各自独立推进。
- `TryCommitSynchronizedCursor`只接受仍指向当前table且generation一致的cursor，只提交四个scalar。
- `ResetFromSeed`与`RestoreSynchronized`均推进transient generation；同一数组原地重建后的旧cursor也会
  fail closed。
- generation只保护进程内owner提交，不写入确定性snapshot/checksum；现有`SynchronizedNext`和consumer
  未改。

## 验证

- test-first 20项预期`CS0246/CS1061`；final focused job
  `2b92ff3217c64cbeb178171ba95a90c7`为11/11。
- owner/cursor 5000步在动态call-site和bound下逐次相等；未提交owner保持不变。
- struct copy隔离、显式提交、nonpositive不推进、reset/restore stale commit拒绝均通过。
- 4096次capture/next/commit warmed managed allocation为0。
- native RNG+world state联合job `0968464eb0634a83b857dccb5f3e8e2e`为16/16。
- 完整SelfCheck于`2026-09-03 08:35:12 +08:00` PASS；清理预期负例后Console error为0。

## 未关闭

- `AiDecisionRandomStream`仍消费legacy CRT；40个native AI synchronized call-site尚未映射/迁移。
- indexed/shadow尚未使用cursor，authority-only commit边界仍待下一包。
