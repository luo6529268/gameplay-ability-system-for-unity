# Task Contract — NTSD28-B0-UNITY-COMPLETED-TICK-BOUNDARY-001

> 状态：`FOCUSED_TEST_PASS / UNITY-COMPILE-0-ERROR / EDITMODE-6-OF-6 / EXACT-FRAME-TICK-DELTA-2`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B0`  
> 建立日期：2026-09-02

## 目标

修正 2.8 Unity raw exporter 把 battle-entry `NeedClearInput` 早退外层 tick 误标为 completed battle
tick 的问题。新 authority `GameSession28::step` 公共场景首个输出已经执行 FrameMachine；Unity
direct-world diagnostic bootstrap 不应再注入旧 runner 的 entry-clear。每个被输出的 tick 必须使
`BattleEcsCharacterFrameTickPassDiagnostics.ExactCharacterCount` 恰好增加两个，否则 fail closed。

## 允许文件

- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs`
- 本 Task、同 ID Change、Ledger、STATE、handoff、总表

## 不变量

- 只修 diagnostic exporter bootstrap/completed boundary；不改 driver、tick system、NeedClearInput
  production contract、输入逻辑或战斗 pass。
- 不增加隐藏 warmup tick，不把外层 tick2重编号为completed tick1。
- 每个输出 tick 必须真实执行两个 exact character frame-tick；否则不写该 tick。
- 不处理 frameCounter 字段绑定本身、allocationEpoch、内容或其他差异。

## 验收

- Unity compile 0 error；exporter focused 3/3，联合 projection/exporter 6/6。
- 公共 Unity raw tick1/2/3 的两个实体均有 frameCounter 1/2/3（依赖进行中的独立 binding包），
  每 tick exact-character delta=2。
- 临时对象/cache清理、确定性、OID99 fail-closed继续通过。
- Ledger 与 diff check 通过。

## 当前证据

- diagnostic direct-world bootstrap 的 `NeedClearInput` 明确设为 false，不添加隐藏 warmup tick。
- 每 tick 在 `StepOneTick` 前后读取 frame-tick pass diagnostics；exact character delta 不等于2即抛错。
- fresh Unity compile 0 error；联合 job `3065f714adee42fd9a75cc09d28df067` 6/6 PASS，
  duration 2.5327901s。
- 公共 raw tick1/2/3 的两个实体 frameCounter 均为1/2/3；确定性、OID99 fail-closed、临时对象/
  cache恢复继续通过。
- 新 Unity raw SHA：`6CCC4FAE18C821C82708C393C9DA7326DC920852A6F21899225A49CEB352AB86`。
- 全局 Ledger：72 records / 14 governed code files / PASS；diff check 无 error。
