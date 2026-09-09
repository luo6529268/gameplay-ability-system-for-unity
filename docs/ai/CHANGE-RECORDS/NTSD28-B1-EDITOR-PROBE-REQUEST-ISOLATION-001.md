# NTSD28-B1-EDITOR-PROBE-REQUEST-ISOLATION-001 — Request-gated legacy Play probes

<!-- CHANGE-RECORD
id: NTSD28-B1-EDITOR-PROBE-REQUEST-ISOLATION-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Test/Editor/BattleCentralLivenessIdentityVisibilityPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleOid5152MergeSplitPlayModeProbeEditor.cs
authority: NTSD28 B1 Host pause contract plus real Play SetPaused(false) call stack from BattleOid5152MergeSplitPlayModeProbeEditor.PollRequest before request existence check.
evidence: TASK-CONTRACT-CREATED / REAL-FIRST-FAILURE-CALLSTACK / OID5152-POLLER-SET-PAUSED-FALSE-WITHOUT-REQUEST / CENTRAL-LIVENESS-SAME-STRUCTURE / REQUEST-CHECK-MOVED-BEFORE-ALL-MUTATION / UNITY-COMPILE-0 / REAL-PLAY-F1-PAUSE-STABLE / SET-PAUSED-ONLY-BOOTSTRAP-ONCE / RELATED-31-OF-31-JOB-BB773A0D99844A97AC0AEC05A72A8021 / REQUEST-SCENARIOS-NOT-RERUN
-->

> 状态：`FOCUSED_TEST_PASS / REAL_PLAY_NO_REQUEST_PASS / REQUEST_SCENARIO_NOT_RERUN`

## 验收结果

- 调用栈证明OID5152 poller无request仍强制unpause；CentralLiveness静态同构。
- request existence check已前移到任何Editor/Driver状态写入之前；有request后的原逻辑未改。
- final真实Play pause稳定且`SetPaused`只见bootstrap一次；related31/31、compile0。
- 未创建旧R8 request重跑完整探针，因此不宣称其完整场景已重新验证。
