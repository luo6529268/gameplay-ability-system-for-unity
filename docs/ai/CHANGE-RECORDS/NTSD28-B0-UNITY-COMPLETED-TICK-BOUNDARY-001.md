# NTSD28-B0-UNITY-COMPLETED-TICK-BOUNDARY-001 — Unity raw completed-tick boundary

<!-- CHANGE-RECORD
id: NTSD28-B0-UNITY-COMPLETED-TICK-BOUNDARY-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
authority: NTSD 2.8-Logan GameSession28::step/FrameMachine28 completed state; Unity NTSDBattleTickSystem NeedClearInput early return; user-directed B0 real trace.
evidence: UNITY-COMPILE-0-ERROR / JOINT-EDITMODE-6-OF-6-PASS / NEEDCLEARINPUT-FALSE-DIAGNOSTIC-BOOTSTRAP / EXACT-CHARACTER-FRAME-TICK-DELTA-2-PER-OUTPUT-TICK / FRAMECOUNTER-1-2-3 / DETERMINISTIC-RERUN / OID99-FAIL-CLOSED / GLOBAL-LEDGER-PASS / NO-PRODUCTION-TICK-CHANGE
-->

> 状态：`FOCUSED_TEST_PASS / UNITY-COMPILE-0-ERROR / EDITMODE-6-OF-6-PASS`

观察：`SimulationTickDriver.StepOneTickInternal` 无视 `NTSDBattleTickSystem.RunReleaseTick` 的
NotCompleted/full-return 区别并返回 true；旧 runner 的 `SetNeedClearInput(true)` 因而让 exporter
把 entry-clear early return 标成 completed tick。真实 raw 中 AttackingCounter 三 tick 恒0暴露了该问题。

实现只允许移除 diagnostic direct-world bootstrap 的旧 entry-clear 注入，并以 frame-tick pass
diagnostics 对每个输出 tick 做 delta=2 hard gate。

回滚：恢复 exporter 的旧 SetNeedClearInput 行并移除 hard gate/test；不改 production driver/tick。

## 实际结果

- direct-world diagnostic bootstrap 不再注入旧 runner 的 entry clear；明确设 `NeedClearInput=false`。
- 每个输出 tick 读取 `BattleEcsCharacterFrameTickPassDiagnostics` 前后值，并要求 exact-character
  delta 恰为场景两个角色；`StepOneTick=true` 不再是唯一 completed 证据。
- fresh Unity compile 0 error；联合 projection/exporter job
  `3065f714adee42fd9a75cc09d28df067` 为6/6 PASS，duration 2.5327901s。
- 输出 tick1/2/3 的两个实体均实际得到 frameCounter1/2/3；确定性、OID99拒绝、临时driver与数据
  singleton清理继续通过。
- 新 Unity raw SHA `6CCC4FAE18C821C82708C393C9DA7326DC920852A6F21899225A49CEB352AB86`；
  与 authority 重比为99 occurrences / 17 unique differences / 30 equal fields。
- 全局 Ledger 72 records / 14 governed code files / PASS；diff check无error。
