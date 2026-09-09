# NTSD28-B2-AI-NATIVE-HISTORY-BIND-ORDER-001 — AI native history与canonical store绑定顺序

<!-- CHANGE-RECORD
id: NTSD28-B2-AI-NATIVE-HISTORY-BIND-ORDER-001
status: FOCUSED_TEST_PASS
change-kind: CODE_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Runtime/SimulationRegistryModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan EntityInputState28 key_history initializes to five -1 values before the first sampled input edge.
evidence: TASK-CONTRACT-CREATED / AUTHORITY-INPUT-STATE-KEY-HISTORY-NEG1X5 / AI-V3-TICK1-KEY-HISTORY-NEG1-VS-0 / STORE-BIND-BEFORE-NATIVE-INITIALIZE-ROOT-CAUSE / TEST-FIRST-RED-JOB-C9956771 / SUCCESSFUL-REGISTRATION-PRE-BIND-INITIALIZE / UNITY-COMPILE-0 / EXPORTER-11-OF-11-JOB-D7FA2FFE / BROAD-183-OF-183-JOB-F6CFD497 / COMMON-STANDING-V3-EQUAL / AI-TICK1-EXACT-INPUT-EQUAL / NEXT-FIRST-DIFFERENCE-TICK2-PREVIOUS-MASK-0-VS-2 / SELFCHECK-185038-PASS / CONSOLE-0 / FORMAL-EXE-PENDING / AUTHORITY-READ-ONLY / CONFIG-DAT-SCENE-UNCHANGED
-->

> 状态：`FOCUSED_TEST_PASS / NATIVE_HISTORY_PRE_BIND_READY / AI_TICK1_EXACT_EQUAL / NEXT_FIRST_DIFFERENCE_TICK2_PREVIOUS_MASK / FORMAL_EXE_PENDING`

## 实际改动与当前证据

- test-first job `c9956771ce424234962f0be1d79a4e3d` 精确失败1项：AI tick1 expected
  `[-1,-1,-1,-1,4]`，actual `[0,0,0,0,4]`。
- `SimulationRegistryModule.RegisterCore` 仅在成功registration、取得current handle且writer尚未Bind时，
  为native profile初始化history；随后store捕获已初始化值。
- `SimulationWorld.RegisterCoreFromStructuralWriter` 删除过晚的post-bind重复初始化。
- compile0；exporter job `d7fa2ffe10974800939f0eb71d4bbd20` 为11/11，registry/slot/input/AI/lockstep
  broad job `f6cfd497b06949a4b01f1cde99b7adbc` 为183/183。
- common/standing仍各3ticks/6pairs equal；AI tick1全部exact input及RNG equal，joint首差下移到tick2 slot1
  `previousMask` authority0 / Unity2（已比较3个entity pair）。
- 18:50:38 full SelfCheck PASS；7条预期负向日志清除后Console error0。
- 本包只关闭初始化/Bind顺序；后续canonical store与native routing roundtrip另包。

## 改前事实

- authority `input_state.h`直接把五项history初始化为`-1`；tick1左edge后的末尾是4。
- Unity `RegisterCoreFromStructuralWriter`先完成registry及`CharacterInputWriter.Bind`，之后才调用
  `InitializeNativeHistory`。runtime虽变成`-1`，store仍保留绑定时的0。
- first-tick readiness修复后的AI RNG已逐次equal；当前严格首差为tick1 slot1 history首项`-1/0`。

## 预期改后职责

- 仅对成功注册且使用2.8 native input pipeline的entity，在writer Bind之前初始化history。
- 删除成功注册返回后过晚的重复初始化。
- 失败注册、Legacy、restore均保持当前边界。

## 计划改动

| 文件 | 类型 / 方法 | 改前职责 | 目标职责 |
|---|---|---|---|
| `SimulationRegistryModule.cs` | `RegisterCore` | writer先捕获全0，外层之后初始化runtime | 成功注册后先初始化native history，再Bind store |
| `SimulationWorld.cs` | `RegisterCoreFromStructuralWriter` | registry返回后补初始化 | 不再执行过晚初始化 |
| `NTSD28UnityRawCaptureEditorTests.cs` | AI scenario test | 只断言RNG `6/7/8` | test-first增加tick1 authority history断言 |

## 不可回退边界

- 不改 native history推进算法、AI kernel、RNG、phase/pass、snapshot restore。
- 不改 registration失败回滚、slot/generation、rest binding、stable id。
- 不改Config/DAT、Scene/Prefab、ProjectSettings、Packages、权威源码/EXE。

## 验收计划

| 层级 | 命令 / 场景 / 输入 | 预期 | 状态 |
|---|---|---|---|
| test-first | AI raw exporter focused | job `c9956771...` 精确 expected `-1,-1,-1,-1,4` / actual `0,0,0,0,4` | `PASS` |
| 编译 | gameplay Unity refresh | 0 error | `PASS` |
| focused | exporter + registry/input/AI/lockstep | 11/11 + 183/183 | `PASS` |
| joint | common/standing/AI v3 | human equal；AI tick1 equal，首差下移tick2 previousMask | `PASS_WITH_NEXT_DIFFERENCE` |
| self-check | full request | 18:50:38 PASS，预期日志清除后Console0 | `PASS` |

## 风险、回滚与未关闭项

- 风险：错误地在allocation commit前初始化会污染失败注册；因此初始化必须位于成功commit后的Bind窗口。
- 回滚：恢复两处registration初始化位置与测试断言。
- 未关闭：AI tick2 previousMask/store roundtrip差异、formal EXE与B2退出。

## Git / 交接

- 修改前工作树：分支`NTSD_2.8_C++`已有受治理改动；`.claude/`与Scene diff不触碰。
- 实际diff：成功registration的native history初始化移至writer Bind之前；外层过晚初始化删除；AI tick1 history断言。
- 提交：无。
- validator：PASSED，Records147 / governed code files102。
