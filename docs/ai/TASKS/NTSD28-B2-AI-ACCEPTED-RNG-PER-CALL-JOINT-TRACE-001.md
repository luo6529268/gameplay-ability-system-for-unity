# Task Contract — NTSD28-B2-AI-ACCEPTED-RNG-PER-CALL-JOINT-TRACE-001

> 状态：`FOCUSED_TEST_PASS / ACCEPTED-AI-TRACE-READY / REAL-FIRST-DIFFERENCE-FIRST-TICK-DELAY / BEHAVIOR-FIX-SPLIT`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2 / AI-RNG-CALL-ORDER`  
> 建立日期：2026-09-04

## 目标

在B2 v3 joint raw中加入Unity IndexedCanonical AI已接受commit的同步RNG逐次调用，并让scenario exporter
按`nativeComputerState1b8/nativeAi`建立AI角色。新增1 human + 1 AI的3tick场景，对照authority每tick
call-site/bound/result/after-state及exact input字段。

## Authority 与当前首差

- authority低slot原生AI条件：type0、difficulty≠3且`native_computer_state_1b8>0`；probe用slot1值3。
- workspace linker wrapper观测到authority tick1/2/3同步调用数6/7/8；tick1 sites为
  `0x14,0x3C,0x1D,0x1E,0x1F,0x38`，均进入真实world cursor。
- Unity exporter当前忽略两个AI scenario字段并强制`AiControlled=false/IsHuman=true`，因此同probe首差为
  tick1 synchronized counter authority7/Unity1。
- Unity AI kernel使用speculative cursor；只有`CommitIndexedCanonicalDecision`成功后才可把
  `IndexedSnapshot`的call-site/modulus/value发布为正式调用。Full oracle、stale或fallback路径不得进入raw。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Ai/Runtime/SimulationAiDecisionModule.cs`
- `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs`
- `Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp`
- 新增`Tools/NTSD28AuthorityTrace/Scenarios/input-ai-one-entity-rng.json`
- `Tools/NTSD28AuthorityTrace/README.md`
- `Tools/NTSD28Parity/B2InputRngJointRawContract.cs`
- `Tools/NTSD28Parity/B2InputRngJointRawComparator.cs`
- `Tools/NTSD28Parity/B2InputRngJointRawSelfTest.cs`
- `Tools/NTSD28Parity/README.md`
- 本Task、Change Record、Ledger、STATE、handoff与总表

禁止修改AI决策分支、call-site表、RNG算法/cursor commit、权威源码/EXE、Config/DAT、Scene/Prefab、
ProjectSettings、Packages或正式profile默认采样开关。

## 不变量

- accepted-trace observer默认为null；null时不强制trace/full oracle，不增加allocation或改变生产分支。
- observer存在时允许为diagnostic强制capture trace，但只有commit成功后按原始cursor起点发布；被拒绝、stale、
  Full oracle与fallback均不发布。
- 每个AI call的counter/index/ordinal从commit前cursor起点严格递增；最终必须闭合world tick scalar。
- v3同时继续通过input-common与standing-attack；新AI fixture如有差异必须输出新的first difference，不能
  为求相等改AI规则。

## 验收

- Unity exporter test-first精确红；.NET v3 fixture test-first schema red；
- C++/.NET/Unity compile0，v3/legacy selftests与AI focused通过；null observer allocation回归通过；
- 三个fixture双端均valid；input-common、standing保持equal，AI fixture达到equal或冻结真实下游首差；
- SelfCheck、Console0、Ledger/diff check通过；formal EXE仍单独列pending。

## 回滚

移除accepted observer、AI scenario投影与v3字段/fixture，恢复v2 human/direct capture；不回退AI生产commit、
direct RNG observer或前序B2修复。

## 当前证据

- Unity test-first精确1个CS0117（missing `AiRngScenario`）。
- .NET v3 fixture build0后4个schema-mismatch预期失败，malformed case仍PASS。
- accepted-only observer、AI scenario projection和v3 schema已实现；C++/.NET/Unity compile0。
- v3+legacy selftests 5+5/21/12/6，Unity exporter+AI80/80、AI+lockstep86/86通过。
- input-common/standing仍equal；AI raw valid但首差tick1 counter7/1。Unity后续6/7 calls分别逐项等于authority
  前一tick，证明真实缺口是AI首tick readiness晚1 tick。18:25:46 SelfCheck、Console0。
- behavior修复必须另立包；本包不改AI规则，也不声明formal EXE/B2完成。
