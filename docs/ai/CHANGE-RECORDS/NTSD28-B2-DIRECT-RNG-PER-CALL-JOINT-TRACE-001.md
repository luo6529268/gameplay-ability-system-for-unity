# NTSD28-B2-DIRECT-RNG-PER-CALL-JOINT-TRACE-001 — direct RNG per-call joint trace

<!-- CHANGE-RECORD
id: NTSD28-B2-DIRECT-RNG-PER-CALL-JOINT-TRACE-001
status: FOCUSED_TEST_PASS
change-kind: CODE_AND_TEST
code-path: Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp
code-path: Tools/NTSD28AuthorityTrace/Build-AuthoritySourceCapture.ps1
code-path: Tools/NTSD28Parity/B2InputRngJointRawContract.cs
code-path: Tools/NTSD28Parity/B2InputRngJointRawComparator.cs
code-path: Tools/NTSD28Parity/B2InputRngJointRawSelfTest.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSD28NativeRandom.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeRandomDirectCallTraceEditorTests.cs
authority: NTSD 2.8-Logan playable NativeRandom28 direct crt_next/synchronized_next symbols observed through workspace-only linker wrapping; human standing attack uses synchronized site 0x82 in input_routing.cpp.
evidence: TASK-CONTRACT-CREATED / LINKER-WRAP-TEMP-PROBE-CRT1-SYNC1-PASS / TEST-FIRST-UNITY-5-CS0246 / TEST-FIRST-DOTNET-3-SCHEMA-MISMATCH / DOTNET-BUILD-0 / B2-V2-SELFTEST-5-OF-5 / LEGACY-SELFTESTS-5-21-12-6-PASS / CPP-FULL-SOURCE-BUILD-0 / AUTHORITY-MANIFEST-C59BD8D3 / RUNNER-7A2746CE / BINARY-243075D0 / UNITY-COMPILE-0 / UNITY-FOCUSED-31-OF-31-THEN-25-OF-25 / AI-LOCKSTEP-RELATED-86-OF-86 / INPUT-COMMON-V2-3-TICKS-6-PAIRS-EQUAL / STANDING-ATTACK-V2-TICK2-SITE130-BOUND2-RESULT1-3-TICKS-6-PAIRS-EQUAL / NULL-OBSERVER-4096-ZERO-ALLOC / SELFCHECK-180840-PASS / CONSOLE-0 / AI-CURSOR-AND-FORMAL-EXE-PENDING / AUTHORITY-READ-ONLY / CONFIG-DAT-SCENE-UNCHANGED
-->

> 状态：`FOCUSED_TEST_PASS / DIRECT-PER-CALL-V2-READY / INPUT-COMMON-STANDING-EQUAL / AI-FORMAL-PENDING`

## 改前事实

- v1 B2 schema比较scalar、totalCalls与tickCallCount，但`calls`固定null。
- 临时GNU `--wrap` probe已在workspace验证可拦截两个mangled symbol，原函数结果保持可用。
- 临时3tick standing attack场景两端scalar均在completed tick2增加1次sync且lastSite130；v1 comparator全equal。

## 预期改后职责

- authority runner与Unity diagnostic exporter输出completed-tick direct-call数组。
- independent validator验证数组与scalar闭合，comparator逐字段比较。
- AI cursor/commit、formal EXE与初始化BGM bound/result继续明确排除。

## 验证记录

- Task Contract与Change Record已在任何本包脚本修改前建立。
- Unity test-first compile得到5个预期CS0246，全部来自尚不存在的observer/call-record API。
- .NET test-first切换v2 fixture后build0，selftest 3个预期`schema-mismatch`、malformed case仍PASS；输出
  `Temp/NTSD28Parity/b2-input-rng-per-call-red.json`。
- authority runner通过GNU linker wrap观察原始direct RNG函数；初始化完成后清空记录，每tick输出
  `callSite/upperBound/result/counterAfter/indexAfter/totalCalls`或CRT对应after-state。完整source build0；
  source manifest `C59BD8D...F2D75`不变，runner `7A2746CE...46DBD`，binary
  `243075D0...87C40`。
- Unity native random新增默认null的diagnostic observer；direct call result/state逻辑不变。new tests覆盖observer
  records、detach同值及4096轮null-observer direct calls零allocation。Unity compile0；首轮含world/bootstrap/
  exporter31/31，job `164bc2711344448ca65545eaed33c84b`；加zero-alloc后focused25/25，job
  `80b8050f90f349bf8c871c4c1262e997`；AI+lockstep相关86/86，job
  `cca2da07e407499bb1350ec38dca6f92`。
- .NET build0；v2 selftest5/5，legacy raw/main/domain/comparator为5/5、21/21、12/12、6/6。
- input-common v2双端valid且3 ticks/6 pairs equal，comparison SHA-256
  `E93D59E5B3770E01C0330C98AA121BE29F5B20AAE13463AEDA4137BE4DD7016D`。
- standing-attack v2双端valid；completed tick2双方唯一direct sync call均为site130、bound2、result1、
  counter/index2、totalCalls2；3 ticks/6 pairs equal，comparison SHA-256
  `F9F3B34DD09E7E5CE3A1859527507CD18AC0168DD9E6DBD3C803D42288AD205C`。
- 18:08:40 full SelfCheck PASS；7条预期失败路径error清除后Console0。
- 当前关闭的是human/direct completed-tick调用序列；AI synchronized cursor/accepted commit联合trace和formal EXE
  可观察证据仍待，B2不退出。

## 回滚说明

按Task Contract恢复v1 scalar-only诊断，不改authority或前序production修复。
