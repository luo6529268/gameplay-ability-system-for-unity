# NTSD28-B1-WORKER-PACING-AUDIT-001 — Dedicated worker pacing audit

<!-- CHANGE-RECORD
id: NTSD28-B1-WORKER-PACING-AUDIT-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHostControlPlayModeProbeEditor.cs
authority: NTSD 2.8-Logan Host timing plus Unity actual production worker eligibility and single-in-flight publication contract.
evidence: TASK-CONTRACT-CREATED / SOURCE-AND-REAL-PLAY-AUDIT / SCENE-USE-WORKER-TRUE / ACTUAL-WORKER-INACTIVE / INELIGIBILITY-UNITY-PRESENTATION-BINDINGS-ARE-STILL-ATTACHED / WORKER-FAILURE-EMPTY / SUBMISSION-FAILURE-EMPTY / SINGLE-IN-FLIGHT-CANNOT-DRAIN-SECOND-INTERVAL-SAME-UPDATE / CURRENT-INLINE-PRODUCTION-PATH-REAL-PLAY-PASS / REPORT-SHA256-DAC9D1D1D8CE14E8D850B032FC16E929C09E319AD49FB1F7B8381DD11EC5C0ED / B9-REVALIDATION-TRIGGER / PRODUCTION-UNCHANGED
-->

> 状态：`FOCUSED_TEST_PASS / REAL_REASON_CAPTURED / CURRENT_PATH_INACTIVE / B9_REVALIDATION_TRIGGER`

## 验收结果

- Scene useWorker=true，但真实reason为`unity-presentation-bindings-are-still-attached`；failure/submission均空。
- 当前正式运行走inline fallback，其33/3ms已通过；worker不是当前default/optimized live path。
- source确认single-in-flight在首次submit后阻止同Update第二tick。B9若解除presentation binding，必须先
  新建B1 worker cadence实现/trace包，未通过前不得启用为正式路径。
- report SHA `DAC9D1...C0ED`；本包未修改production。
