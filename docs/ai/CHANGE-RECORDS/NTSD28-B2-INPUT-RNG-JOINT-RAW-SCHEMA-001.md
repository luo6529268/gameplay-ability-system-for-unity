# NTSD28-B2-INPUT-RNG-JOINT-RAW-SCHEMA-001 — exact input/native RNG joint raw diagnostics

<!-- CHANGE-RECORD
id: NTSD28-B2-INPUT-RNG-JOINT-RAW-SCHEMA-001
status: FOCUSED_TEST_PASS
change-kind: TEST_AND_TOOL_ONLY
code-path: Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp
code-path: Tools/NTSD28Parity/Program.cs
code-path: Tools/NTSD28Parity/B2InputRngJointRawContract.cs
code-path: Tools/NTSD28Parity/B2InputRngJointRawComparator.cs
code-path: Tools/NTSD28Parity/B2InputRngJointRawSelfTest.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
authority: Formal NTSD2.8-Logan.exe SHA 1277B70BA030A1F33B625EEA20B43834325B280CEC555650BF43CD90A64DAF75 plus playable EntityInputState28, NativeRandomState28 and input_update_phase source closure.
evidence: TASK-CONTRACT-CREATED / DOTNET-TEST-FIRST-12-MISSING-TYPE-ERRORS / UNITY-TEST-FIRST-CS1501-CS0117 / VERSIONED-B2-INPUT-RNG-SCHEMA-FROZEN / DOTNET-BUILD-0 / B2-SELFTEST-4-OF-4 / LEGACY-SELFTESTS-5-12-6 / UNITY-FINAL-9-OF-9 / RELATED-RAW-RNG-INPUT-40-OF-40 / TWO-TU-PHASE1-CURRENT-ZERO-CONFIRMED / CPP-FIRST-BUILD-CSCOPE-ERROR-CURRENT-WORLD / CPP-BUILD-0 / RUNNER-SHA-31868E82 / BINARY-SHA-BA644BE7 / DOUBLE-VALID-3-TICKS-6-ENTITIES / DIAGNOSTIC-NATIVE-RNG-MATCH-SEED-CORRECTED / REAL-FIRST-DIFFERENCE-INITIAL-SYNCHRONIZED-COUNTER-1-VS-0 / AUTHORITY-LAST-SITE-004021E0-RANDOM-BGM / DOWNSTREAM-DEFEND-REENTRY-3-VS-0 / B0-SCHEMAS-UNCHANGED-AND-VALID / SELFCHECK-PASS-20260904-172328 / CONSOLE-0 / SOURCE-MODEL-DIAGNOSTIC-ONLY / PER-CALL-RNG-LOG-MISSING-DECLARED / PRODUCTION-CODE-UNCHANGED / AUTHORITY-READ-ONLY / CONFIG-DAT-SCENE-UNCHANGED
-->

> 状态：`FOCUSED_TEST_PASS / INPUT-RNG-OBSERVABILITY-READY / REAL-FIRST-DIFFERENCE-CAPTURED / DIAGNOSTIC_ONLY`

## 改前事实

- 既有B0 entity raw已经在input-common上关闭action/state/counter差异，但不表达exact proxy/history/combo。
- 既有B0 domain raw只表达authority dual streams与Unity legacy deterministic stream；它建立于Unity native dual
  RNG接入前，不能证明current `NTSD28NativeRandom`状态。
- 两端已具备所需scalar/carrier，缺口主要在诊断输出与独立严格比较；本包不预判它们相等。

## 预期改后职责

- source-model与Unity各自产生独立B2 input/RNG JSONL，B0输出不变。
- independent .NET validator/comparator按稳定字段顺序给出可复现首差。
- 首差后另建production修正包；本记录只闭合观测能力。

## 验证记录

- Task Contract与Change Record已在任何本包脚本修改前建立。
- .NET test-first在contract/comparator不存在时得到12个CS0246/CS0103；Unity test-first得到missing four-arg
  overload CS1501与missing schema constant CS0117。
- independent .NET contract/comparator/commands已写；Release build0，self-test4/4。
- Unity optional B2 writer已写。首轮focused job `c97ca54226d344da8e811c1ea05fa65c`为8 pass/1 fail；
  唯一失败是测试错误预期phase1立即sample current17，实际2tu phase1依法current0，phase0下一tick才为
  slot0=1/slot1=96。测试已按B2 cadence合同更正，exporter不因该失败修改。
- 首轮Unity output显示native RNG仍处于默认seed1 state/hash，authority已知scenario seed状态不同；这只是
  candidate，等待source-model B2 exporter与strict comparator正式分类。
- authority runner首次完整build在新增loop尾部因`current_world`局部作用域报1个编译错误；已把只读world
  pointer提升到本tick共同作用域并修正缩进，等待重建。该错误没有产出新binary，也未触碰authority。
- C++修正后完整build0；runner source SHA `31868E82...EFC88`、binary SHA `BA644BE7...DB0A8`。
  authority与Unity B2 raw均通过validator（各3tick/6entity）；首次strict comparison首差为
  `initialRng.crt.state` authority1758127634、Unity3878484156。
- 调用链复核确认正式Unity `SimulationTickDriver`与lockstep bootstrap都用match seed初始化NativeRandom；只有
  本诊断手工world漏调。已在Editor exporter补scenario seed并加authority CRT state/table hash断言；未伪造
  authority随后`0x004021E0`随机BGM synchronized draw，下一次首差应由comparator真实给出。
- final Unity raw job `0cad8a5a73c2468da1018448e959ee39` 9/9；相关entity raw、native RNG/world、
  LocalFrameInput、native producer与production integration job `f576919498e0438c8bbc82b09d03cf4a` 40/40。
- 双端B2 validator各通过3tick/6entity。authority raw SHA
  `38A6DA0DB36BD5D8B236363F1D2D9708ED774E911C4CEF8CD6D7229B3225D45D`，Unity raw SHA
  `30BD31024A4BADD54D13FB529D6C7B6397A3606D89D54E51E29928F9E1E3C34E`。
- strict comparison SHA `7BEFD2CFF470C284FB49C0208BB0FAEEFB3F2E77315AD12E2A51276F7F78FDBD`：
  first difference `initialRng.synchronized.counter` authority1/Unity0。authority header同时证明
  lastCallSite4202976=`0x004021E0`、index1、calls1；Unity均0。
- 不修改raw文件的字段级次级检查证明exact input除tick2/3 slot1
  `defendReentryCooldown` authority3/Unity0外全部相等；该差异位于RNG首差之后，另立production包。
- B0 authority entity/domain validator与旧self-tests仍通过；entity comparator37 equal/10 diff，首差B11
  baseMaxMp；domain input/slots/lifecycle equal。SelfCheck 17:23:28 PASS，7条预期error清除后Console0。
- production C#、Config/DAT、Scene/Prefab、ProjectSettings、Packages和authority均未由本包修改；
  formal EXE certificate与per-call RNG trace仍未获得。

## 回滚说明

按Task Contract移除新增诊断输出/工具命令；production runtime与authority不含本包改动。
