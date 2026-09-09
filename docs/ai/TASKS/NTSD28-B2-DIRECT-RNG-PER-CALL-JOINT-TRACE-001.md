# Task Contract — NTSD28-B2-DIRECT-RNG-PER-CALL-JOINT-TRACE-001

> 状态：`FOCUSED_TEST_PASS / DIRECT-PER-CALL-V2-READY / INPUT-COMMON-STANDING-EQUAL / AI-FORMAL-PENDING`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2 / RNG-CALL-ORDER`  
> 建立日期：2026-09-04

## 目标

把B2 exact联合raw从只比较每tick RNG scalar/delta，升级为可比较每个completed tick内的直接native RNG
调用顺序、call-site、upper bound、result与调用后状态；新增全human standing attack场景，实际覆盖同步调用
site `0x82`。两端仍只产出diagnostic证据，不冒充formal EXE certificate。

## Authority 与诊断策略

- authority `NativeRandom28::crt_next`与`synchronized_next`是playable closure内的集中直接调用边界。
- workspace capture binary使用GNU linker `--wrap`观察这两个符号；权威源码和正式EXE保持只读，source
  manifest继续基于未修改的authority closure。
- trace window严格从`GameSession28::initialize`完成后开始，到每个completed tick结束；初始化时3000次table
  build及BGM预抽取不进入per-call数组。BGM bound/result属于B10，初始化游标继续由scalar校验。
- Unity只为`NTSD28NativeRandom.CrtNext/SynchronizedNext`提供默认null的diagnostic observer；AI的
  speculative synchronized cursor/accepted commit已有独立B2 trace合同，不在本包伪装为direct call。
- 本包场景全部是human，因此authority/Unity direct-call coverage可直接比较；AI per-call联合证据仍另包。

## 允许修改

- `Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp`
- `Tools/NTSD28AuthorityTrace/Build-AuthoritySourceCapture.ps1`
- `Tools/NTSD28AuthorityTrace/README.md`
- 新增`Tools/NTSD28AuthorityTrace/Scenarios/input-standing-attack-rng.json`
- `Tools/NTSD28Parity/B2InputRngJointRawContract.cs`
- `Tools/NTSD28Parity/B2InputRngJointRawComparator.cs`
- `Tools/NTSD28Parity/B2InputRngJointRawSelfTest.cs`
- `Tools/NTSD28Parity/README.md`
- `Assets/NTSD/Scripts/Simulation/Core/NTSD28NativeRandom.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs`
- 新增`Assets/NTSD/Scripts/Test/Editor/NTSD28NativeRandomDirectCallTraceEditorTests.cs`及`.meta`
- 本Task、Change Record、Ledger、STATE、handoff与总表

禁止修改authority、正式EXE、AI cursor/commit、战斗规则、Config/DAT、Scene/Prefab、ProjectSettings、
Packages或BGM播放/catalog。

## 不变量

- observer为null时不改变RNG state/calls/result，不产生managed allocation；restore/snapshot/checksum不包含observer。
- link wrapper必须先调用原始函数，再只读捕获after-state；不得重写或复制权威RNG算法。
- per-call数组长度必须等于tickCallCount，ordinal连续，最后after-state必须等于tick scalar。
- input-common继续全equal；standing-attack必须在completed tick2恰好出现一条site130/bound2调用并两端相等。
- schema必须明确`certificateEligible=false`和human/direct-only边界。

## 验收

- Unity test-first因缺少observer/record API编译红；.NET selftest因v2 per-call合同尚未实现而红；
- C++ full source capture build0，link wrapper probe/build实际成功；
- .NET build0及new/legacy selftests通过；Unity compile0、new/native/exporter/input相关focused通过；
- input-common与standing-attack双端raw均valid并compare equal；standing-attack tick2 call数组精确为0x82；
- SelfCheck、Console0、Ledger与diff check通过。

## 回滚

移除workspace capture linker wrapper、v2 direct-call字段/observer、新场景和测试，恢复v1 scalar-only输出；不回退
前序RNG bootstrap、defend refresh或生产RNG实现。

## 当前证据

- Unity test-first精确5个CS0246：observer interface与两类call record尚不存在。
- .NET v2 fixture test-first build0，selftest 3个`schema-mismatch`预期失败、malformed case仍PASS。
- v2已实现：workspace authority linker wrapper与Unity default-null observer输出completed-tick direct calls；
  validator闭合array count、ordinal与after-state，comparator逐数组比较。authority保持只读。
- C++ full source build0；.NET build0，v2 5/5及legacy 5/21/12/6全PASS。Unity compile0，focused
  31/31、25/25，AI+lockstep86/86；null observer 4096轮零allocation。
- input-common和standing-attack均双端valid且3 ticks/6 pairs equal；后者tick2唯一调用精确为
  site0x82/bound2/result1。18:08:40 SelfCheck PASS、Console0。
- AI cursor/accepted commit及formal EXE仍另待；本包没有B2整体退出结论。
