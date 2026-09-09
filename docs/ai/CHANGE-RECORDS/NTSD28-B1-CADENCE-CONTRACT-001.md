# NTSD28-B1-CADENCE-CONTRACT-001 — exact 33/3ms Host cadence

<!-- CHANGE-RECORD
id: NTSD28-B1-CADENCE-CONTRACT-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationConstants.cs
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickHostPolicy.cs
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/SimulationTickHostPolicyEditorTests.cs
authority: NTSD 2.8-Logan native_function_keys.h exact 33/3ms values and playable main.cpp one-step-per-host-loop/two-interval-debt-cap/timing-reset contract.
evidence: RED-COMPILE-0-JOB-BBD2211533F54ADBB8B6A7F6F282A50B-2-EXPECTED-FAILURES / EXACT-33MS-NORMAL / EXACT-3MS-FAST / TWO-ACTIVE-INTERVAL-DEBT-CAP / ONE-AUTOMATIC-TICK-PER-UPDATE / CADENCE-CHANGE-CLEARS-DEBT / SAME-MODE-PRESERVES-DEBT / UNITY-COMPILE-0 / HOSTPOLICY-4-OF-4-JOB-D112130597FF42EB93A483AFF175B3A6 / RELATED-11-OF-11-JOB-E0EE2F30AB344C9C8B5936CC6CCDA2F0 / STRESS-3-OF-3-JOB-84AA1067FB6141C3A52059BDE8322207 / B0-JOINT-9-OF-9-JOB-F5C7B46083914B5E8E1AB0F36A1ED5CF / B0-DOMAIN-SHA-UNCHANGED-D888201F168A14BF73BB8F06CC822856BDC8AC039E840A46A599218BA04BBFC8 / DOTNET-BUILD-0-0 / TOOL-6-12-21-5-PASS / FORMAT-PASS / GLOBAL-LEDGER-94-RECORDS-22-FILES-PASS / SCOPED-DIFF-CHECK-PASS / LOCAL-FREE-RUN-HOST-ONLY / MANUAL-LOCKSTEP-UNCHANGED / PHYSICS-TICK-CONVERSION-UNCHANGED / F1-F2-F5-DEFERRED / RUNTIME-PENDING
-->

> 状态：`FOCUSED_TEST_PASS / UNITY-COMPILE-0 / HOSTPOLICY-4-OF-4 / RUNTIME-PENDING`

本包只落纯cadence/Offline policy；F1/F2/F5接线另包。

## 验收结果

- red job `bbd2...a50b` compile0、2 expected failures；final host4/4 `d112...b3a6`。
- related11/11 `e0ee...a2f0`、stress3/3 `84aa...2207`、B0 joint9/9 `f5c...d5cf`。
- exact33/3ms、cap2、one/update、cadence reset已写；Manual/Lockstep/physics conversion未改。
- B0 domain SHA `D888...BFC8`不变；.NET与治理94/22通过。
- F1/F2/F5和真实LocalFreeRun/Play仍待后续包。
