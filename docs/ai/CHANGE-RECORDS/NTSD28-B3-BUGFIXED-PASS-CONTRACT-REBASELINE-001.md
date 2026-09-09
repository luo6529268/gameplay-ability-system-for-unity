# NTSD28-B3-BUGFIXED-PASS-CONTRACT-REBASELINE-001 — Bug修复版pass合同重新基线

<!-- CHANGE-RECORD
id: NTSD28-B3-BUGFIXED-PASS-CONTRACT-REBASELINE-001
status: FOCUSED_TEST_PASS
change-kind: TEST_FIRST_CONTRACT_CORRECTION
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSD28BattlePassOrder.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28BattlePassOrderEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs
authority: User-promoted NTSD 2.8-Logan EXE B1E13AE1, 82-file playable closure 39DDDA15 and 75-file capture subset 07CD47A0; current SimulationTickDriver28::step live path.
evidence: TASK-CONTRACT-CREATED / CORE-SHA-4AA2CA63 / PHYSICS-NORMALIZE-NESTED / TYPE0-DROP-NONTYPE0-HIT-ORDER / STAGE-THEN-IMPULSE / SLOT-TAIL-16-STEPS / TEST-FIRST-18-CS0117 / UNITY-COMPILE-0 / FOCUSED-10-OF-10-JOB-3C2D2532 / B3-RELATED-38-OF-38-JOB-5B5F1B65 / SELFCHECK-2026-09-04T22-58-20-PASS / KNOWN-NEGATIVE-7-REVIEWED / CONSOLE-0 / OLD-52-CONTRACT-SUPERSEDED / PRODUCTION-BEHAVIOR-UNCHANGED / AUTHORITY-READ-ONLY
-->

> 状态：`FOCUSED_TEST_PASS / 57-CHECKPOINT / BUGFIXED-AUTHORITY-CONTRACT / PRODUCTION-UNCONNECTED`

## 改前事实

现有52项contract、10项focused test和B3 manifest对应旧`1277B70B...DAF75`/`C59BD8D3...2D75`身份。
用户晋升的新版权威改变了core后半和部分nested traversal，旧contract不能继续指导production migration。

## 计划

按Task Contract先更新断言取得红灯，再最小修正immutable descriptors；本包不移动任何实际Unity pass。

## 验证

- test-first：仅更新focused断言后，旧contract产生18个预期`CS0117`，覆盖新增domain/traversal/pass ID。
- 实现：contract更新为57项连续ID；physics采用2步per-slot nested transaction；hit拆为type-0与non-type-0，
  random-drop保持其间；impulse位于第二次clamp/refill/stage settlement后；slot tail扩展为16步。
- Unity脚本编译0 error。
- focused `10/10` PASS，job `3c2d2532e97b40fe8ceff0107080cce8`。
- B3 contract/actual-sequence/spark相关 `38/38` PASS，job `5b5f1b6510ea4af8af9e3ebbccb36d45`。
- 完整SelfCheck于`2026-09-04 22:58:20 +08:00` PASS，result SHA
  `2F9ACB02FAA121BB2A3621951F57B4C690655337EDEE2E5AC350BE2B3BE88EA8`；7条既有negative rest-binding
  日志复核后清空Console，error=0。
- 本包只改immutable contract/test，不修改production tick行为，因此不需要新增Play行为验收。

## 结论

旧52项合同已被本57项合同supersede。C00～C03保持不变，所以C01后的Unity实际首差仍是
`Authority CoreProducerSampleScan / Unity Cooldown`；producer/Cooldown审计可以继续。
