# NTSD28-B0-ACTION-LATCH-BINDING-001 — action latch / WaitCounter binding

<!-- CHANGE-RECORD
id: NTSD28-B0-ACTION-LATCH-BINDING-001
status: FOCUSED_TEST_PASS
code-path: Tools/NTSD28Parity/EntityFieldContract.cs
code-path: Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
authority: NTSD 2.8-Logan FrameMachine28 action_latch/frame_counter order; Unity FrameTransistor WaitCounter and RunCommonFrameTick order; real raw first difference.
evidence: BUILD-0-WARN-0-ERROR / SELFTEST-21-OF-21 / RAW-SELFTEST-5-OF-5 / FORMAT-PASS / UNITY-COMPILE-0 / FIRST-JOINT-5-OF-6-STALE-ASSERTION / FINAL-JOINT-6-OF-6 / NONZERO-WAITCOUNTER-PROJECTION / REAL-ACTIONLATCH-DIFFERENCE-CLOSED / MATURITY-24-11-12 / GLOBAL-LEDGER-PASS / NO-PRODUCTION-WRITE-CHANGE
-->

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / UNITY-COMPILE-0 / JOINT-6-OF-6`

只修contract/projection/maturity/test。tickActionSnapshot与previousAction存在潜在映射冲突，本包排除。
回滚：恢复actionLatch MISSING/null与23/11/13；不触碰生产frame逻辑。

## 实际结果

- actionLatch绑定WaitCounter并晋级VERIFIED；maturity24/11/12。
- 工具build0/0、21/21、5/5、format PASS；contract SHA
  `225B50A17EDE245D8ABA505C8EFF52CDCDCD6E195AE916EB121F638B2C3B79D2`。
- Unity compile0。第一次联合运行唯一失败是旧测试仍断言null（实际正确输出8）；移除该旧断言后
  `82ca8ec798d940c9a856df552bd2a03d` 6/6 PASS，duration2.68846s。
- 真实raw中actionLatch差异消失：15→14 unique、32→33 equal、90→84 occurrences；首差异为
  tickActionSnapshot。生产FrameTransistor/WaitCounter写入未改。
- Ledger75 records/14 governed code files/PASS；diff check无error。
