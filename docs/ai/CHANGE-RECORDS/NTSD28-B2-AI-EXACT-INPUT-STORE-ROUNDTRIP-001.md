# NTSD28-B2-AI-EXACT-INPUT-STORE-ROUNDTRIP-001 — native exact input到AI canonical store回写

<!-- CHANGE-RECORD
id: NTSD28-B2-AI-EXACT-INPUT-STORE-ROUNDTRIP-001
status: FOCUSED_TEST_PASS
change-kind: CODE_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Stores/BattleCharacterInputStore.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCharacterInputWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Input/NTSD28InputTwoPassModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan InputRouter28::step_sampled mutates one persistent EntityInputState28 through edge/history/combo/action processing; the post-route state is the next AI tick input.
evidence: TASK-CONTRACT-CREATED / AI-V3-FIRST-DIFFERENCE-TICK2-SLOT1-KEY-HISTORY-3-AUTHORITY-4-UNITY-MINUS1 / UNITY-SPLIT-RUNTIME-AND-CANONICAL-STORE-ROOT-CAUSE / TEST-FIRST-RED-JOB-C83E0F44-EXPECTED-TWO-LEFT-HISTORY-ACTUAL-ONE / POST-ROUTE-HANDLE-SAFE-FULL-CAPTURE / UNIFIED-PROJECTION-CHANGE-DETECTION-PRESERVED / UNITY-COMPILE-0 / EXPORTER-11-OF-11-JOB-2A222175 / BROAD-200-OF-200-JOB-43DBB9C7 / COMMON-STANDING-3-TICKS-6-PAIRS-EQUAL / AI-TICKS1-2-EXACT-AND-RNG-EQUAL / AI-NEXT-FIRST-DIFFERENCE-TICK3-CURRENT-MASK-B11-CONTENT-DOWNSTREAM / SELFCHECK-20260904-192155-PASS / CONSOLE-ERROR-0 / AUTHORITY-READ-ONLY / CONFIG-DAT-SCENE-UNCHANGED
-->

> 状态：`FOCUSED_TEST_PASS / AI-TICKS1-2-EXACT-EQUAL / NEXT-FIRST-DIFFERENCE-B11-CONTENT-DOWNSTREAM-TICK3 / FORMAL_EXE_PENDING`

## 改前事实

- authority在同一`EntityInputState28`中执行`push_history()`并把结果保留给下一tick AI。
- Unity exact route已把第二个left rising edge写到`runtime.InputHistory`，但route前AI witness已经提交到
  `BattleCharacterInputStore`；本tick结束没有回写seam。
- 下一tick AI canonical commit以旧store为基线，将runtime history重新变成仅一个4。AI v3首差为tick2
  `keyHistory[3]` authority4 / Unity-1；previous/run/RNG已相等。

## 预期改后职责

- Store提供handle-safe的runtime full capture，并沿用既有projection change publication。
- Writer只暴露语义化的post-native exact AI synchronization入口。
- Two-pass在exact route、action route和legacy projection完成后，仅对AI调用该入口，再同步`InputState`。
- exact combo10/proxy tail/run accumulator仍由`NativeInputProxy`持有，不被legacy store反写。

## 实际改动与结果

- `BattleCharacterInputStore.SynchronizeCanonicalStateFromRuntime`先经`TryResolve`验证slot/generation，再从
  post-route runtime capture完整canonical row，并沿用`PublishAiProjectionIfChanged`。
- `BattleCharacterInputWriter.SynchronizeNativeExactAiStateFromRuntime`提供语义化writer seam；
  `NTSD28InputTwoPassModule.ProcessNativeSampledState`仅在native route与legacy projection完成后、仅对AI调用。
- tick2 history已与authority一致；comparator在比较完ticks1—2全部entity/input/RNG字段后，首差移到tick3
  slot1 currentMask18/3。tick2一般entity已先发生action650/9内容差异，故tick3是B11下游而非store失败。

## 计划改动

| 文件 | 类型 / 方法 | 改前职责 | 目标职责 |
|---|---|---|---|
| `BattleCharacterInputStore.cs` | 新runtime capture seam | registration时capture，运行中无post-route full回写 | handle-safe full capture并正确发布projection变化 |
| `BattleCharacterInputWriter.cs` | 新AI exact roundtrip seam | store与runtime只能显式input commit | 将post-route runtime状态同步到canonical store |
| `NTSD28InputTwoPassModule.cs` | `ProcessNativeSampledState` | exact route后只project到legacy/runtime | project完成后AI回写canonical store |
| `NTSD28UnityRawCaptureEditorTests.cs` | AI scenario | 仅断言tick2 previous/run | test-first增加tick2完整history |

## 验收计划

| 层级 | 命令 / 场景 / 输入 | 预期 | 状态 |
|---|---|---|---|
| test-first | AI raw exporter | tick2 history在现状精确失败 | `PASS / RED c83e0f44` |
| 编译 | gameplay Unity refresh | 0 error | `PASS` |
| focused | exporter/store/input/AI/lockstep/worker | 全通过 | `PASS / 11+200` |
| joint | common/standing/AI v3 | human不回归，AI首差下移/整体equal | `PASS / HUMAN EQUAL / AI TICKS1-2 EQUAL` |
| self-check | full request | PASS，Console0 | `PASS / 19:21:55` |

## 风险、回滚与未关闭项

- 风险：不带generation检查的回写可污染复用slot；全量publication可制造无意义mutation。
- 回滚：移除store/writer/two-pass seam与history断言。
- 未关闭：formal EXE与B2退出；action650/9留B11。

## Git / 交接

- 基线：分支`NTSD_2.8_C++`已有受治理改动；`.claude/`与Scene diff不触碰。
- 实际diff：input store、character input writer、two-pass module、raw exporter test与本Record/恢复文档。
- 提交：无。
- validator：`PASSED / Records 149 / governed code files 104`。
