# NTSD28-B0-RAW-ENTITY-DIFFERENCE-001 — repeatable raw entity first-difference

<!-- CHANGE-RECORD
id: NTSD28-B0-RAW-ENTITY-DIFFERENCE-001
status: FOCUSED_TEST_PASS
code-path: Tools/NTSD28Parity/RawEntityCaptureComparator.cs
code-path: Tools/NTSD28Parity/Program.cs
authority: NTSD28-B0-TRACE-CONTRACT-001; NTSD28-B0-ENTITY-FIELD-SCHEMA-001; validated common authority and Unity raw captures.
evidence: RELEASE-BUILD-0-WARN-0-ERROR / RAW-SELF-TEST-5-OF-5-PASS / REAL-3-TICKS-6-PAIRS-282-FIELD-OCCURRENCES / 105-DIFFERENCE-OCCURRENCES-18-UNIQUE-29-EQUAL / EXISTING-SELF-TEST-21-OF-21 / FORMAT-PASS / GLOBAL-LEDGER-PASS / NO-RUNTIME-DAT-SCENE-AUTHORITY-WRITE
-->

> 状态：`FOCUSED_TEST_PASS / BUILD-0-WARN-0-ERROR / RAW-SELF-TEST-5-OF-5`

修改前的只读 PowerShell 盘点已观察 105 个 tick-slot difference occurrences、18 个 unique fields：
14 个是既定 Unity missing=null；其余为 allocationEpoch、frame.frameCounter、vitals.baseMaxMp、
combat.environmentState。该一次性盘点不是可恢复工具，因此本包把同一检查固化为严格命令与 self-test。

回滚：删除新增 comparator，移除 Program/README 命令接线和本 ID 治理记录；不影响已有 raw 文件、
exporter、runtime 或 authority。

## 实际结果

- 新增 `compare-raw-entities` 与 `self-test-raw-entities`；authority 先走既有严格 validator，Unity
  header/tick/binding manifest/slot order/type/null policy 独立 fail-closed。
- 首次 build 报 CS1002/CS1525，原因是 switch expression 后直接放 null-forgiving；改为显式
  nullable local + null throw 后 Release build 0 warning / 0 error。
- raw self-test 5/5；现有总 self-test 21/21；format verify PASS。
- 真实公共报告稳定为：ticks3、pairs6、field occurrences282、difference occurrences105、
  unique differences18、unique equal29；首差异 allocationEpoch tick1 slot1 authority2/Unity1。
- 14 个差异分类为 `UNITY_BINDING_MISSING`；4 个非空差异为 allocationEpoch、frameCounter、
  baseMaxMp、environmentState。工具未归一化或隐藏任何一项。
- 全局 Ledger 70 records / 14 governed code files / PASS；diff check 无 error。
