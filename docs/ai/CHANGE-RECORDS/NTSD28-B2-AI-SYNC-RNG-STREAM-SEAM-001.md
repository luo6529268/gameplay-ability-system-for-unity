# NTSD28-B2-AI-SYNC-RNG-STREAM-SEAM-001 — call-site-aware AI synchronized RNG stream seam

<!-- CHANGE-RECORD
id: NTSD28-B2-AI-SYNC-RNG-STREAM-SEAM-001
status: FOCUSED_TEST_PASS
change-kind: CODE_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Ai/Kernel/AiDecisionRandomStream.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSD28NativeRandom.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28AiSynchronizedRandomStreamEditorTests.cs
authority: NTSD 2.8-Logan native_random.cpp synchronized_next and NTSD28-B2-AI-RNG-CALLSITE-CROSSWALK-001.
evidence: TEST-FIRST-34-EXPECTED-COMPILE-ERRORS / FOCUSED-7-OF-7-JOB-F7EF4BEE0A754DA684C765437CFEE915 / NATIVE-RNG-RELATED-18-OF-18-JOB-67CD10BB667C4C3AA4A5B97FA5E15DB7 / WORLD-KERNEL-18-OF-18-JOB-C3148397DC414D68AD969327AEDFF699 / AI-DECISION-SHADOW-172-OF-172-JOB-4FEDDA76D3DB475085E5AC68C5D63250 / LONG-SEQUENCE-5000-BIT-EXACT / WARM-4096-ZERO-ALLOC / SELFCHECK-PASS-2026-09-03T09-03-38 / CONSOLE-0 / LEDGER-119-RECORDS-64-FILES / PRODUCTION-CONSUMERS-UNCHANGED
-->

> 状态：`FOCUSED_TEST_PASS / STREAM_SEAM_READY / PRODUCTION_UNCONNECTED`

## Authority 与 Unity 原状

- Authority `synchronized_next(site,bound)`在bound<1时返回0且不推进；否则先推进counter/index，
  再用`(table[index]+counter)%bound`出值，并记录calls/last-site。
- `NTSD28NativeRandom`已有generation-gated cursor；`AiDecisionRandomStream`仍只有CRT模式和无site
  `Rand(int)`，不能安全承载40个AI live ID。

## 计划改动

- 为cursor补充无分配的raw合成值输出，不改变现有`Next`调用；
- 为AI stream增加synchronized模式、显式site API与site trace；
- 保持production kernel与旧CRT测试完全不变。

## 实际改动

- `NTSD28SynchronizedRandomCursor.Next(..., out rawValue)`在保留旧overload的同时暴露
  authority公式取模前的`table[index]+counter`，nonpositive仍不推进。
- `AiDecisionRandomStream`新增synchronized cursor constructor与显式`Rand(site,bound)`；
  记录site/bound/raw/value及带site的order hash，并能交回cursor给owner显式commit。
- 两种模式相互fail closed：同步模式拒绝无site `Rand(int)`，legacy CRT模式拒绝带site API；
  现有CRT constructor、状态推进与旧hash未变。
- 新增7个Editor tests覆盖5000步、trace、nonpositive/no-site、copy/commit、stale、legacy与zero-allocation。

## 验收记录

- test-first refresh：34个预期编译错误，全部来自新测试引用的缺失constructor、
  `Rand(site,bound)`、同步属性/cursor回收和call-site trace；无其他编译错误。
- focused job `f7ef4bee0a754da684c765437cfee915`：7/7。
- native RNG联合 job `67cd10bb667c4c3aa4a5b97fa5e15db7`：18/18。
- world-state +旧kernel job `c3148397dc414d68ad969327aedff699`：18/18。
- AI decision/character/shadow job `4fedda76d3db475085e5ac68c5d63250`：172/172。
- 完整SelfCheck：`Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`，时间09:03:38。
- SelfCheck预期负例7条已确认；清理后Console error 0。
- `Tools/Validate-ChangeLedger.ps1`通过：119 records、64 governed code files。

## 未关闭项

- `AiDecisionKernel`仍构造legacy CRT stream，所有production consumer尚未切换。
- 40个live site、69个surplus表达式、force-attack/use_ai carrier与authoritative-only commit按后续独立包处理。
