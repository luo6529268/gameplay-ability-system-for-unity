# NTSD28-B0-FRAME-HISTORY-BINDINGS-001 — previous/tick snapshot binding correction

<!-- CHANGE-RECORD
id: NTSD28-B0-FRAME-HISTORY-BINDINGS-001
status: FOCUSED_TEST_PASS
code-path: Tools/NTSD28Parity/EntityFieldContract.cs
code-path: Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
authority: NTSD 2.8-Logan BattleWorld28::snapshot_actions and SimulationTickDriver28 per-entity previous_action_078 tail; Unity CaptureCollisionFrameSnapshot and MirrorLatePrevFrame; real raw first difference.
evidence: TOOL-BUILD-0-WARN-0-ERROR / SELFTEST-21-OF-21 / RAW-SELFTEST-5-OF-5 / FORMAT-PASS / UNITY-COMPILE-0 / EDITOR-REQUEST-TWO-OF-TWO-PASS / REAL-RAW-DETERMINISTIC-SHA-9F5ABB4E / REAL-TICK-SNAPSHOT-DIFFERENCE-CLOSED / JOINT-6-OF-6 / NONZERO-FRAME-PREV-9-PREVFRAME2-6-SEPARATION / NO-PRODUCTION-WRITE-CHANGE
-->

> 状态：`FOCUSED_TEST_PASS / REAL-BASELINE-PASS / JOINT-6-OF-6`

旧candidate把previousAction指到PrevFrame2；新证据表明它属于Frame.Prev，而PrevFrame2属于当前missing
tickActionSnapshot。两项必须原子更正。回滚恢复旧candidate/missing与24/11/12；不改生产writer。

## 当前结果

- previousAction现读Frame.Prev，tickActionSnapshot现读Runtime.PrevFrame2；maturity26/10/11。
- 工具build0/0、21/21、5/5、format PASS；Unity fresh compile0。
- MCP bridge恢复后，focused job `16bf469ff22d48f9ad1ba1d977496949` 6/6 PASS。
- 同一Editor request通道两次真实capture均PASS，SHA均为
  `9F5ABB4EC2197834B4E5311DB8AFD8F00BACDAE8251C1CEBEBC5D19D0CF6E0D5`；真实差异14→13、
  equal33→34、occurrence84→78。
- 非零 `Frame.Prev=9 / PrevFrame2=6` 防交叉 NUnit 已实际执行通过；本包晋级 FOCUSED_TEST_PASS。
