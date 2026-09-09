# NTSD28-B1-HOST-PHYSICAL-EDGE-LATCH-001 — Host physical edge latch

<!-- CHANGE-RECORD
id: NTSD28-B1-HOST-PHYSICAL-EDGE-LATCH-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickHostPolicy.cs
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/SimulationTickHostPolicyEditorTests.cs
authority: NTSD 2.8-Logan native_function_keys.h edge transition; real Unity Play first-failure reports under Temp/NTSD28B1 show Input System frame flag not consumed by Driver ordering.
evidence: TASK-CONTRACT-CREATED / RED-CS0246-MISSING-LATCH / FALSE-TO-TRUE-EDGE-ONLY / HELD-NO-REPEAT / RELEASE-REARMS / CLEAR-REARMS / LOCAL-FREE-RUN-ONLY / UNITY-COMPILE-0 / HOST-7-OF-7-JOB-3BE40D5802984C6D99ECB5B4DC2CC286 / RELATED-31-OF-31-JOB-BB773A0D99844A97AC0AEC05A72A8021 / REAL-PLAY-SYNTHETIC-INPUT-DEVICE-PRODUCTION-PATH-PASS / OS-PHYSICAL-PENDING
-->

> 状态：`FOCUSED_TEST_PASS / UNITY_COMPILE_0 / EDGE_LATCH_7_7 / REAL_PLAY_DEVICE_PATH_PASS`

## 验收结果

- test-first先产生预期`CS0246`；实现独立F1/F2/F5 held latch后focused7/7、related31/31。
- Driver只读取`isPressed`并由自身false→true产生edge；held不重复，release/clear重装填。
- shutdown、ApplyMatchConfig、非LocalFreeRun边界清latch；pause/F2/F5 transition和Core未改。
- 真实Play的临时Input System Keyboard经production入口通过；OS用户实体键仍待人工/可获焦点环境验收。
