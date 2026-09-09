# NTSD28-B2-AI-ACCEPTED-RNG-PER-CALL-JOINT-TRACE-001 — accepted AI RNG per-call joint trace

<!-- CHANGE-RECORD
id: NTSD28-B2-AI-ACCEPTED-RNG-PER-CALL-JOINT-TRACE-001
status: FOCUSED_TEST_PASS
change-kind: CODE_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Ai/Runtime/SimulationAiDecisionModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
code-path: Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp
code-path: Tools/NTSD28Parity/B2InputRngJointRawContract.cs
code-path: Tools/NTSD28Parity/B2InputRngJointRawComparator.cs
code-path: Tools/NTSD28Parity/B2InputRngJointRawSelfTest.cs
authority: NTSD 2.8-Logan playable SimulationTickDriver28 native AI branch and NativeAi28 synchronized calls; workspace source capture linker wrapper records the committed world stream.
evidence: TASK-CONTRACT-CREATED / AUTHORITY-AI-TICK-CALLS-6-7-8 / AUTHORITY-TICK1-SITES-14-3C-1D-1E-1F-38 / TEST-FIRST-UNITY-1-CS0117 / TEST-FIRST-DOTNET-4-SCHEMA-MISMATCH / ACCEPTED-COMMIT-ONLY-OBSERVER-READY / UNITY-AI-SCENARIO-PROJECTION-READY / CPP-FULL-SOURCE-BUILD-0 / AUTHORITY-MANIFEST-C59BD8D3 / RUNNER-B2535623 / BINARY-644D8FEF / DOTNET-BUILD-0 / V3-SELFTEST-5-OF-5 / LEGACY-SELFTESTS-5-21-12-6 / UNITY-COMPILE-0 / EXPORTER-AND-AI-80-OF-80 / AI-LOCKSTEP-86-OF-86 / INPUT-COMMON-AND-STANDING-V3-EQUAL / AI-FIRST-DIFFERENCE-TICK1-SYNC-COUNTER-7-VS-1 / UNITY-TICK2-SEQUENCE-EQUAL-AUTHORITY-TICK1 / UNITY-TICK3-SEQUENCE-EQUAL-AUTHORITY-TICK2 / SELFCHECK-182546-PASS / CONSOLE-0 / FIRST-TICK-AI-READINESS-FIX-SPLIT / FORMAL-EXE-PENDING / AUTHORITY-READ-ONLY / CONFIG-DAT-SCENE-UNCHANGED
-->

> 状态：`FOCUSED_TEST_PASS / ACCEPTED-AI-TRACE-READY / REAL-FIRST-DIFFERENCE-FIRST-TICK-DELAY / BEHAVIOR-FIX-SPLIT`

## 改前事实

- authority AI probe的world stream每tick真实消费6/7/8次；v2 wrapper已经完整捕获逐次调用。
- Unity exporter忽略AI字段并强制human，首差counter7/1，不是AI算法比较结果。
- IndexedCanonical snapshot已有预分配trace数组，但生产默认不采样且没有accepted-only diagnostic publication。

## 预期改后职责

- observer存在时采集indexed trace，commit成功后按起始cursor发布；默认null不改变生产路径。
- exporter建立scenario AI并把accepted calls合并进同一completed-tick同步数组。
- v3 contract取消human-only/AI-excluded限制，继续保持diagnostic-only。

## 验证记录

- Task Contract与Change Record已在任何本包脚本修改前建立。
- Unity exporter test-first得到精确1个CS0117：`AiRngScenario`尚不存在。
- .NET v3 fixture build0，selftest 4个预期schema-mismatch、malformed case仍PASS；输出
  `Temp/NTSD28Parity/b2-ai-v3-red.json`。
- `SimulationAiDecisionModule`仅在observer非null时强制预分配trace；只有
  `CommitIndexedCanonicalDecision`成功后，才从原始cursor起点按indexed trace发布after-state。默认null、
  fallback/stale/full-oracle均不发布。
- Unity exporter支持`nativeAi/nativeComputerState1b8`并把对应角色标记为AI；同一recorder合并direct与
  accepted AI calls。v3 scope明确AI included/initialization excluded。
- C++ full source build0；source manifest `C59BD8D...F2D75`不变，runner
  `B2535623...544AC`，binary `644D8FEF...68501`。三组authority v3 raw均valid。
- .NET build0；v3 selftest5/5，legacy5/21/12/6全部PASS。Unity compile0；exporter+AI80/80 job
  `aadffe0fad824f2f955ff5c140395659`，AI+lockstep86/86 job
  `292c56f8cbf147018d7d759b8bbce700`。
- input-common和standing-attack v3仍3 ticks/6 pairs equal，comparison SHA分别
  `9A491CA1...07546C`、`14332441...F551E0`。
- AI fixture双端valid，但首差为completed tick1 synchronized counter authority7/Unity1；comparison SHA
  `9C3E968B...E43DD`。Unity tick2的6-call序列逐项等于authority tick1，Unity tick3的7-call序列逐项等于
  authority tick2，严格定位为Unity AI首次eligible晚1 tick，而不是call-site/算法值错误。
- 18:25:46 full SelfCheck PASS；7条预期失败路径error清除后Console0。
- 本包完成观测并冻结首差；production first-tick AI readiness另立包，formal EXE仍pending，B2不退出。

## 回滚说明

按Task Contract恢复v2 direct-only诊断；AI production语义不回退。
