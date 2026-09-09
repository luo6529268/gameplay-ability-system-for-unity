# NTSD28-B2-DEFEND-REENTRY-EXACT-FRAME-REFRESH-001 — exact defend re-entry refresh

<!-- CHANGE-RECORD
id: NTSD28-B2-DEFEND-REENTRY-EXACT-FRAME-REFRESH-001
status: FOCUSED_TEST_PASS
change-kind: CODE_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCharacterInputWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28DefendReentryExactFrameRefreshEditorTests.cs
authority: NTSD 2.8-Logan playable battle_world.cpp FUN_0040D000-equivalent frame-state pass refreshes Entity28+0x0C1 to 3 whenever the surviving resulting action is 110 or 114; input_routing.cpp decrements it before the following input edge pass.
evidence: TASK-CONTRACT-CREATED / B2-JOINT-FIRST-DIFFERENCE-TICK2-SLOT1-DEFEND-REENTRY-3-VS-0 / EXISTING-FRAME-110-114-GATE-AND-PASS-ORDER-CONFIRMED / WRITER-LEGACY-ONLY-GAP-CONFIRMED / TEST-FIRST-3-OF-3-EXPECTED-ASSERTION-FAILURES / WRITER-EXACT-MIRROR-WRITTEN / COMPILE-0 / FOCUSED-75-OF-75 / B2-INPUT-COMMON-EXACT-INPUT-RNG-3-TICKS-6-PAIRS-EQUAL / SELFCHECK-174738-PASS / CONSOLE-0 / AUTHORITY-READ-ONLY / CONFIG-DAT-SCENE-UNCHANGED
-->

> 状态：`FOCUSED_TEST_PASS / EXACT-REFRESH-READY / INPUT-COMMON-JOINT-EQUAL / FORMAL-PER-CALL-PENDING`

## 改前事实

- authority在frame-state pass的结果action110/114上刷新exact byte为3，下个input pass先递减。
- Unity生产frame pass与legacy-compatible path已经调用`SetDefendLock(..., 3)`。
- writer当前写SoA legacy store与`runtime.CdDefendLock`，未写
  `runtime.NativeInputProxy.DefendReentryCooldown`；joint trace因此在tick2 first-difference 3/0。

## 预期改后职责

- writer以单一事务同步SoA legacy、runtime legacy mirror和B2 exact carrier。
- frame pass、input decrement、action routing及非110/114行为不变。

## 验证记录

- Task Contract与Change Record已在任何本包脚本修改前建立。
- test-first new3/3均按预期失败，job `2036ab44931240e2a3e31aeea7254109`：legacy断言已通过，
  exact仍为预置旧值，精确证明writer mirror缺失。
- writer现以同一non-null事务写`CdDefendLock`与`NativeInputProxy.DefendReentryCooldown`；frame pass和
  action gate未改。
- Unity compile0；new/writer/combo/type0 production相关75/75 PASS，job
  `cc77e669de7647899a7cf25bec5406a0`。
- input-common B2 exact input/native dual RNG重跑valid 3 ticks/6 entity pairs，comparison
  `equal-input-rng-joint-raw`且`firstDifference=null`；Unity capture SHA-256
  `E03B8C38768B7D2A13B6511901983B94C7D9779ACA10CF8C549D60C6BA6EC313`，comparison SHA-256
  `05177FBFEF79A557F0B158485302EBA16EEAE6AC165B9E343CDCC07750EC5BEC`。
- 17:47:38 full SelfCheck PASS；7条预期失败路径error清除后Console0。
- 当前相等只覆盖source-model diagnostic input-common fixture；formal EXE与per-call RNG证据仍待，不能据此
  宣称B2整体退出。

## 回滚说明

按Task Contract只移除exact mirror和本包测试，不回退既有frame gate或前序B2包。
