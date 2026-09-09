# NTSD28-B2-AI-PICKUP-RNG-001 — pickup state gate and sites 0x16/0x17

<!-- CHANGE-RECORD
id: NTSD28-B2-AI-PICKUP-RNG-001
status: FOCUSED_TEST_PASS
change-kind: CODE_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Ai/Kernel/AiSensingKernel.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/Kernel/AiDecisionKernel.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28AiPickupRandomEditorTests.cs
authority: NTSD 2.8-Logan scan_threats_and_pickups pickup_state 1000/2004 and step_pickup_target sites 0x16/0x17.
evidence: TEST-FIRST-1-EXPECTED-COMPILE-ERROR-MISSING-NATIVE-PICKUP-OVERLOAD / FOCUSED-6-OF-6-JOB-FEF4B708D2C248E9B330DECB1325A36E / RELATED-AI-SENSING-297-OF-297-JOB-B27E33F7CAA84ABFBD66CA6126FB6FCB / NATIVE-PICKUP-STATE-1000-2004 / LEGACY-PICKUP-STATE-1004-2004-PRESERVED / SITES-0X16-0X17 / WARM-4096-ZERO-ALLOC / SELFCHECK-PASS-2026-09-03T09-50-31 / CONSOLE-0 / LEDGER-123-RECORDS-71-FILES / PRODUCTION-CAPTURE-UNCHANGED
-->

> 状态：`FOCUSED_TEST_PASS / PICKUP_SITES_16_17_READY / PRODUCTION_UNCONNECTED`

Authority在slot20+ pickup候选上使用state1000/2004；Unity legacy使用1004/2004。本包只为
synchronized candidate增加2.8 gate，并接左右site；不宣称完整threat/pickup选择链已闭合。

Test-first refresh取得1个预期CS1739：缺`useNative28PickupState`重载；无其他编译错误。

## 实际改动与验证

- `AiSensingKernel.TryScanSpecial`新增internal native pickup gate；既有public overload固定legacy gate。
- synchronized candidate使用state1000/2004，legacy仍使用1004/2004；decision downstream采用相同模式门。
- `MoveTowardTarget`左右远距分别以`0x16/0x17`调用，legacy仍经旧CRT。
- focused job `fef4b708d2c248e9b330decb1325a36e`：6/6；native/legacy gate、远距二选一、
  250内no-consume及4096 zero-allocation通过。
- AI sensing/decision/shadow全组首次296/297暴露旧routing visibility测试；按独立test-only包更正后，
  job `b27e33f7caa84abfbd66ca6126fb6fcb`为297/297。
- SelfCheck 09:50:31 PASS；清理预期负例后Console0；Ledger123/71通过。

## 未关闭项

完整threat/pickup selection的其他字段/分支尚未宣称对齐；production cursor capture/commit未接。
