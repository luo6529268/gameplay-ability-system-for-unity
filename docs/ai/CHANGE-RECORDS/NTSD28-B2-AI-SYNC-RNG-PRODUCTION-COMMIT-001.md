# NTSD28-B2-AI-SYNC-RNG-PRODUCTION-COMMIT-001 — accepted-only production cursor commit

<!-- CHANGE-RECORD
id: NTSD28-B2-AI-SYNC-RNG-PRODUCTION-COMMIT-001
status: FOCUSED_TEST_PASS
change-kind: CODE_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSD28NativeRandom.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/Snapshots/AiDecisionSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/Runtime/SimulationAiDecisionModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleAiInputWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeRandomEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/AiDecisionSoAShadowEditorTests.cs
authority: NTSD 2.8-Logan BattleWorld synchronized RNG ownership and step_main ordered consumption; Unity accepted IndexedCanonical transaction boundary.
evidence: RED-5-OF-81 / ACCEPTED-NATIVE-COMMIT / SAME-GENERATION-STALE-REJECT / LEGACY-RNG-UNCHANGED / ORACLE-SHADOW-DISCARD / FOCUSED-81-OF-81 / ALL-AI-362-OF-362 / BROAD-234-OF-234 / SELFCHECK-2026-09-03T11:36:54-PASS / CONSOLE-0 / LEDGER-128-76-PASS / NATIVE-INPUT-JOINT-TRACE-PENDING
-->

> 状态：`FOCUSED_TEST_PASS / PRODUCTION_SYNC_COMMIT_READY / LEGACY_RNG_ISOLATED / JOINT_TRACE_PENDING`

## 改前事实

- production snapshot只捕获legacy `Rng.State/CallCount`，不设置synchronized cursor。
- writer无条件把`world.Rng`恢复为witness值；若直接启用sync，witness legacy state为0，会破坏旧stream。
- cursor只校验table identity/generation；同generation的旧cursor能覆盖已提交的新位置。
- Full oracle已有CopyOwnedFrom和不提交结构，但RNG trace比较未包含call-site数组。
- reusable snapshot Populate不会显式清除旧cursor，未来oracle复制后可能向非production capture泄漏。

## 预期改后职责

- world在每个IndexedCanonical capture时提供NativeRandom cursor；snapshot/witness传递candidate。
- commit validation检查origin freshness；writer先唯一提交NativeRandom，再发布input/flow，且不触碰legacy RNG。
- oracle/shadow/fallback零同步副作用；call-site也纳入oracle严格比较。

## 验证记录

- test-first job `929951ee02b64d2895f7207e2eefbc9d`：81 total、5 expected failed、
  76 passed；红灯分别为same-generation stale被接受、旧position38树仍被期望、accepted production
  NativeRandom未推进、Full oracle路径NativeRandom未推进、call-site差异未比较。
- 实施：cursor记录capture origin并由`CanCommitSynchronizedCursor`检查；world/module显式传递cursor；
  snapshot普通populate清旧cursor；writer先commit cursor再写input；sync accepted不动legacy RNG；
  Full oracle只复制/比较；call-site进入严格trace比较。
- 最终 focused job `8b17731394bf42709cd42ab2ca5c794d` 81/81；全AI job
  `ab5cb12975c441bbba899c34c26fea27` 362/362；B2 broad job
  `5a432d977a0a4eeea8ec69c81fb40e9b` 234/234。
- 补强DeepShadow/SharedShadow scalar不变断言后，shadow class job
  `6419df7677354cc197c0c50b9257ce29` 69/69，native RNG class job
  `b5305763fc1c4504a56cb290eb5a0dca` 12/12；warmed allocation测试继续通过。
- full SelfCheck首次暴露旧R3 legacy RNG owner断言，由独立
  `NTSD28-B2-SELFCHECK-AI-NATIVE-RNG-SEAM-001`修正；2026-09-03 11:36:54最终PASS。
- Unity compile 0；Console清理后0 error；`git diff --check`通过；Change Ledger
  128 records / 76 governed code files PASS。
- 未验证：native input producer仍使用迁移桥；B2同seed/input/tick联合trace未执行。因此状态只到
  `FOCUSED_TEST_PASS`，不得称AI或B2完全对齐。
