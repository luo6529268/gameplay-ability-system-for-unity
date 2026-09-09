# NTSD28-B2-NATIVE-DUAL-RNG-PRIMITIVE-001 — standalone native dual RNG primitive

<!-- CHANGE-RECORD
id: NTSD28-B2-NATIVE-DUAL-RNG-PRIMITIVE-001
status: FOCUSED_TEST_PASS
change-kind: SCRIPT_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSD28NativeRandom.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeRandomEditorTests.cs
authority: NTSD 2.8-Logan native_random.h/.cpp and native_random_tests.cpp in current-EXE build closure.
evidence: TEST-FIRST-CS0246-12 / UNITY-COMPILE-0 / FOCUSED-6-6 / RELATED-SHARED-RNG-1-1 / RELATED-BOUNDARY-RNG-3-3 / TOTAL-10-10 / AUTHORITY-SEED-682973786 / AUTHORITY-CRT-STATE-1758127634 / AUTHORITY-TABLE-HASH-A1BA1B90EA55796D / DEEP-COPY-PASS / WORLD-UNCONNECTED / EXISTING-CONSUMERS-UNCHANGED
-->

> 状态：`FOCUSED_TEST_PASS / PRIMITIVE_READY / WORLD_UNCONNECTED`

## 改前职责

Unity world只有Server-owned `DeterministicRng`，AI另有本地流；没有可表示 2.8 CRT + synchronized
table/counter/index/call-site 的独立基元。

## 预期改后职责

新增纯 managed、无 Unity API、无 world 接线的 2.8 双流值/状态 owner，供下一独立包选择性接入。

## 范围、风险与回滚

- 仅新增 production primitive 和 focused Editor test；生产消费者零改动。
- 风险集中在 uint overflow、positive modulo、3001-byte copy alias 与 FNV-1a hash。
- 回滚为移除新增文件；无现有行为需要恢复。

## 验证记录

- test-first red：12个预期 `CS0246`，只指向四个尚未实现的2.8 RNG类型。
- final Unity job `736e7f06a7c14471b6ad41170a2fc58f`：本包6/6、Server-owned shared RNG
  golden vector 1/1、既有boundary RNG 3/3，总计10/10 PASS。
- seed1的41/18467/6334、3000-call表生成、wrap/no-consume、negative restore normalization、
  deep copy、5000-call确定性通过。
- B0真实authority raw seed `682973786` 对应CRT state `1758127634`、table hash
  `A1BA1B90EA55796D`，focused test精确相等。
- Unity编译与Console均0 error。Ledger/diff最终结果在文档更新后重跑。
