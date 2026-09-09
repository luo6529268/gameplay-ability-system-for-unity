# NTSD28-B1-UNITY-HOST-LOOP-BRIDGE-001 — Native Host loop to Unity Present bridge

<!-- CHANGE-RECORD
id: NTSD28-B1-UNITY-HOST-LOOP-BRIDGE-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickHostPolicy.cs
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/SimulationTickHostPolicyEditorTests.cs
authority: NTSD 2.8-Logan playable main.cpp independent ~1ms Host loop, exact 33/3ms logic accumulator, separate render accumulator and two-interval debt cap.
evidence: TASK-CONTRACT-CREATED / REAL-PLAY-FAST-7.24665MS-RED / RED-JOB-7AC8D71706D94C7B9614E959416CD0E3-ONE-EXPECTED-FAILURE / NATIVE-HOST-LOOP-NOT-UNITY-PRESENT / MAX-TWO-TICKS-PER-UNITY-UPDATE / FINAL-7-OF-7-JOB-F5C9E43129E24CB29B4AC42AED60DBD7 / RELATED-31-OF-31-JOB-BB773A0D99844A97AC0AEC05A72A8021 / REAL-PLAY-NORMAL-32.76602MS / REAL-PLAY-FAST-4.4956583MS / RATIO-0.1372049 / MAX-TICK-JUMP-2 / REPORT-SHA256-07364B5362B5DCFC8D743EC9DCCAFD6F2CBBDFAA1FD94E99EBF99645FF95B95F / MANUAL-LOCKSTEP-UNCHANGED / WORKER-PATH-PENDING
-->

> 状态：`FOCUSED_TEST_PASS / REAL_PLAY_PASS / WORKER_PATH_PENDING`

## 验收结果

- test-first red 6/7证明旧policy拒绝第二interval；实现后focused7/7、related31/31。
- LocalFreeRun每Update最多排空2 interval且debt仍cap2；Manual/Lockstep不变。
- 真实Play由Fast7.24665ms改善为4.4956583ms，ratio0.1372049，Normal32.76602ms；max jump2与合同一致，最终报告SHA `07364B...B95F`。
- 当前场景worker inactive，worker单in-flight路径仍保留未验证，故不标`VERIFIED`。
