# Task Contract — NTSD28-B2-INPUT-RNG-JOINT-RAW-SCHEMA-001

> 状态：`FOCUSED_TEST_PASS / INPUT-RNG-OBSERVABILITY-READY / REAL-FIRST-DIFFERENCE-CAPTURED / DIAGNOSTIC_ONLY`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2 / INPUT-JOINT-TRACE`  
> 建立日期：2026-09-04

## 目标

建立独立、版本化的`ntsd28-logan-b2-input-rng-joint-raw-v1`双端诊断格式与严格validator/comparator，
从同一scenario的completed-tick边界输出并比较：input phase、native CRT与synchronized RNG初始/逐tick
scalar state、每实体exact current/previous、七键edge window、defend re-entry、combo10、proxy tail、五项
history、run accumulator、input-last-action及remap/bound控制字段。

本包只增加workspace-owned source-model runner、Unity Editor exporter和independent .NET诊断工具；不修复本包
暴露的运行差异，不修改production runtime、Config/DAT、Scene/Prefab或正式authority。

## Authority 与字段合同

- authority字段来自playable closure的`EntityState28::input`、`input_last_action_144`、
  `input_remap_state_138/input_remap_indices_13c`、`bound_state_198/input_global_record_state_20`、
  `BattleWorld28::input_update_phase_4a0b90()`和`NativeRandomState28`。
- Unity对应字段为`NTSDEntityRuntime.NativeInputProxy`、`InputHistory[1..5]`、`AnimSub`、
  `InputLastAction144`、remap/bound carriers、`SimulationWorld.InputPhase`与
  `NativeRandom.CaptureScalarState()`。
- current/previous使用canonical W/S/A/D/J/K/L mask；edge window按命名对象输出，避免authority逻辑数组
  W/S/A/D/J/K/L与native proxy内存序J/K/L/D/A/W/S混淆。
- RNG以语义role `crt/synchronized`比较，不把Unity legacy `DeterministicRng`冒充native stream；per-call log
  在本包仍为`null/missing`，不能据scalar相等宣称全B2 RNG调用顺序已闭合。
- source runner仍为`SOURCE_MODEL_DIAGNOSTIC_ONLY`，不得冒充formal EXE runtime certificate。

## 允许修改

- `Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp`
- `Tools/NTSD28AuthorityTrace/README.md`
- `Tools/NTSD28Parity/Program.cs`
- `Tools/NTSD28Parity/README.md`
- 新增`Tools/NTSD28Parity/B2InputRngJointRawContract.cs`
- 新增`Tools/NTSD28Parity/B2InputRngJointRawComparator.cs`
- 新增`Tools/NTSD28Parity/B2InputRngJointRawSelfTest.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs`
- 本Task、Change Record、Ledger、STATE、handoff与总表

禁止修改production C#、authority目录、Build closure源码、Config/DAT、Scene/Prefab、ProjectSettings或Packages。

## 不变量

- 新输出与B0 entity/domain raw并存，不更改既有v1 schema或比较语义。
- 所有数组长度、键顺序、tick连续性、producer身份、formal EXE hash和scenario identity必须fail-closed。
- authority与Unity allocation epoch只作实体配对；本schema不包含B11内容值，因此不能用内容策略掩盖输入/RNG差异。
- Unity捕获只读现有native fields，不把legacy projection补成exact，不写回world。
- RNG delta由相邻total calls计算；call totals回退、table hash格式错误或缺字段均invalid。
- comparator按`initialInputPhase → initialRng → each tick inputPhase → rng → entities`稳定顺序给首差。

## 验收

- .NET test-first先因新contract/command不存在失败；Unity test-first先因新B2输出seam不存在失败；
- authority runner重建0 error，新B2输出通过validator；Unity compile0，新B2 output focused测试通过；
- independent self-test覆盖equal、每个核心域首差、malformed与producer/scenario/tick错配；
- 同一input-common双端输出均valid，comparator给出首个真实差异或equal，不因B11 baseMaxMp阻断；
- 相关B0工具、Unity raw capture、native RNG/input、SelfCheck、Console0、Ledger/diff通过。

## 回滚

移除两个exporter的新可选输出、三个新.NET文件与Program/README命令接线，保留既有B0 raw/domain和production
输入/RNG实现不变。

## 当前证据

- test-first：.NET 12个missing type/name错误；Unity CS1501+CS0117。实现后.NET Release build0、B2
  self-test4/4；旧raw/B0 contract/B0 comparator self-test 5/5、12/12、6/6。
- authority runner首次build捕获1个`current_world`作用域错误；修正后完整playable source closure build0，
  runner SHA `31868E82...EFC88`、binary SHA `BA644BE7...DB0A8`，authority只读。
- Unity首次9项中8 pass，唯一旧预期错误已确认2tu phase1不采样current；更正后final9/9 job
  `0cad8a5a73c2468da1018448e959ee39`。相关raw/native RNG/world/input/production integration共40/40
  job `f576919498e0438c8bbc82b09d03cf4a`。
- 双端B2 raw均valid，各3tick/6entity：authority SHA `38A6DA0D...25D45D`，Unity SHA
  `30BD3102...E3C34E`。strict comparison SHA `7BEFD2CF...8FDBD`首差为
  `initialRng.synchronized.counter`：authority1、Unity0。
- authority lastCallSite `0x004021E0`是direct battle Random BGM selection；它会使后续synchronized RNG游标
  整体领先Unity 1次。production修正另包，不在诊断包伪造。
- read-only secondary exact-input comparison显示除tick2/3 slot1 `defendReentryCooldown 3/0`外全部相等；
  该frame110/114 refresh owner差异另包修复。
- B0 authority entity/domain仍valid；B0 input/slots/lifecycle equal，entity action/state/counter等37字段equal，
  首差仍是B11 baseMaxMp。17:23:28 SelfCheck PASS，7条预期负向日志清除后Console0。
