# NTSD28-B2-FIRST-TICK-CHARACTER-INPUT-AI-READINESS-001 — 首有效 tick 角色输入与 AI readiness

<!-- CHANGE-RECORD
id: NTSD28-B2-FIRST-TICK-CHARACTER-INPUT-AI-READINESS-001
status: FOCUSED_TEST_PASS
change-kind: CODE_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
authority: NTSD 2.8-Logan playable SimulationTickDriver28::step native AI/input loop executes on every valid step, including the first completed tick.
evidence: TASK-CONTRACT-CREATED / AUTHORITY-FIRST-STEP-HAS-NATIVE-AI / AI-V3-FIRST-DIFFERENCE-TICK1 / TEST-FIRST-RED-JOB-5153CB5D-EXPECTED-6-ACTUAL-0 / INVALID-TICK-GUARDS-WRITTEN / UNITY-COMPILE-0 / EXPORTER-11-OF-11-JOB-0C280F95 / BROAD-149-OF-149-JOB-520105C6 / SELFCHECK-RED-AUDIT6-01-OLD-FIRST-TICK-SKIP / SELFCHECK-REBASELINE / SELFCHECK-183800-PASS / COMMON-STANDING-V3-EQUAL / AI-V3-NATIVE-RNG-PER-CALL-EQUAL / NEXT-FIRST-DIFFERENCE-TICK1-KEY-HISTORY-NEG1-VS-0 / CONSOLE-0 / FORMAL-EXE-PENDING / AUTHORITY-READ-ONLY / CONFIG-DAT-SCENE-UNCHANGED
-->

> 状态：`FOCUSED_TEST_PASS / FIRST_TICK_READY / AI_NATIVE_RNG_JOINT_EQUAL / NEXT_FIRST_DIFFERENCE_AI_KEY_HISTORY / FORMAL_EXE_PENDING`

## 改前事实

- authority `simulation_tick_driver.cpp` 在每次 `step(...)` 的 slot loop 中直接执行符合条件的
  `NativeAi28::step_main(...)`，没有 first-tick skip。
- authority AI fixture tick 1/2/3 同步调用数为 `6/7/8`。
- Unity AI fixture tick 1为0次；tick 2/3逐次等于 authority tick 1/2。
- Unity 两个公开输入入口均用 `tickIndex <= 1` 直接返回，导致整个 input/AI pass 晚一 tick。

## 预期改后职责

- `tickIndex <= 0` 保持 no-op，`tickIndex == 1` 进入既有输入/AI完整路径。
- 不改变 phase、eligibility、decision、RNG、action resolver或 two-pass 内部顺序。
- 修正旧 SelfCheck 中把首有效 tick skip 当作正确行为的断言。

## 计划改动

| 文件 | 类型 / 方法 | 改前职责 | 目标职责 |
|---|---|---|---|
| `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs` | `AiInputAndComboAll` / `CharacterInputAll` | `tickIndex <= 1` 整体返回 | 仅拒绝非正 tick，首有效 tick 正常执行 |
| `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs` | AI RNG scenario test | 固化 Unity 延迟后的 `0/6/7` | test-first 固化 authority `6/7/8` |
| `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` | `AUDIT6-01` / AI merged fixture | 把 tick 1 skip 当作正确 | 对齐首有效 tick 执行合同 |

## 不可回退边界

- 不改 33/3 ms cadence、input phase、proxy/two-pass、slot/cursor/checksum 与 ordered shutdown。
- 不改 AI 分支、call-site、RNG 算法、accepted commit 或诊断默认开关。
- 不改 Config/DAT、Scene/Prefab、ProjectSettings、Packages、权威源码/EXE。
- 不回退此前已关闭的 B0/B1/B2 Change ID。

## 验收计划

| 层级 | 命令 / 场景 / 输入 | 预期 | 状态 |
|---|---|---|---|
| test-first | AI raw exporter focused test | job `5153cb5d...`：tick1 expected6/actual0 | `PASS` |
| 编译 | gameplay Unity refresh / console | 0 compile error | `PASS` |
| focused | exporter + input/AI/lockstep test groups | 11/11 + 149/149 | `PASS` |
| joint trace | common / standing / AI v3 | human equal；AI RNG全equal；新首差keyHistory | `PASS_WITH_NEXT_DIFFERENCE` |
| self-check | `BattleRuntimeSelfCheck` request | 18:38:00 PASS，预期日志清除后Console0 | `PASS` |
| formal EXE | 正式 EXE证书 | 独立证据 | `PENDING` |

## 实际改动与当前证据

- test-first job `5153cb5d81c64540bc3b454b834a03a8` 精确失败1项：completed tick 1期望
  `tickCallCount=6`，实际为0；其余同组测试继续完成，根因证据未被编译错误污染。
- `AiInputAndComboAll` 与 `CharacterInputAll` 的 guard 已从 `tickIndex <= 1` 收紧为
  `tickIndex <= 0`；内部 phase、eligibility、AI/RNG、two-pass顺序未改。
- Unity refresh后compile error为0；exporter focused job `0c280f95c3964e6a9d9431553dfcd315` 为11/11，
  authority `6/7/8` 断言通过。
- full SelfCheck按预期在旧 `AUDIT6-01` “tick one must not run”断言精确失败；已把同源human、AI及merged
  fixture改为“非正tick no-op、首有效tick执行”；重新编译后18:38:00 full SelfCheck PASS。
- broad exporter/input/AI/lockstep job `520105c63bbb42a7a0a3e926ec37329d` 为149/149；预期负向日志清除后
  Console error 0。
- common与standing v3仍各3 ticks/6 pairs equal。AI三tick同步 RNG 的call count、site、bound、result、
  after-state已全部跨端相等；新的首差为completed tick1 slot1 `keyHistory[0]` authority `-1` / Unity `0`。
- 本包只关闭first-tick readiness，不把新的AI exact-input差异塞进同一行为包；formal EXE与B2退出仍pending。

## 风险、回滚与未关闭项

- 风险：旧测试可能将历史首 tick skip 编码为合同；必须依据2.8 pass边界逐项重基线，不能批量放宽断言。
- 回滚：仅恢复本包 guard/test/self-check diff。
- 未关闭：AI exact input generation/key history新首差、formal EXE证书与B2阶段退出。

## Git / 交接

- 修改前工作树基线：分支 `NTSD_2.8_C++` 已有多项受治理改动；用户 `.claude/` 与既存 Scene diff 不触碰。
- 实际 diff 范围：`SimulationWorld`两个guard、raw exporter authority断言、SelfCheck三处同源首tick基线及本包文档。
- 提交 hash：无。
- validator：PASSED，Records146 / governed code files101。
