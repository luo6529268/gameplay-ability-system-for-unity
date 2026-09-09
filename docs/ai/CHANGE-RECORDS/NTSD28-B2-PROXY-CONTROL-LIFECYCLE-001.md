# NTSD28-B2-PROXY-CONTROL-LIFECYCLE-001 — exact proxy control lifecycle core

<!-- CHANGE-RECORD
id: NTSD28-B2-PROXY-CONTROL-LIFECYCLE-001
status: FOCUSED_TEST_PASS
change-kind: SCRIPT_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Input/NTSD28InputProxyControlLifecycle.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28InputProxyControlLifecycleEditorTests.cs
authority: NTSD 2.8-Logan battle_world.cpp apply_confirmed_input_statuses, apply_native_join_and_mimic_side_effects and advance_reaction_timers FUN_0040D000/FUN_004503C5 live path.
evidence: TEST-FIRST-14-EXPECTED-CS0103 / COMPILE-PASS / FOCUSED-JOB-F1B8-13-OF-13 / RELATED-JOB-C86C-44-OF-44 / ZERO-ALLOC-4096 / FULL-SELFCHECK-PASS-20260903-065938 / CONSOLE-ERROR-0-AFTER-EXPECTED-NEGATIVE-FIXTURE-CLEAR / GLOBAL-PASS-UNCONNECTED / HIT-WRITER-UNCONNECTED
-->

> 状态：`FOCUSED_TEST_PASS / CONTROL_CORE_READY / PRODUCTION_UNCONNECTED`

## 实际修改

- confirmed mimic status payload只写counter，不擅自消费RNG或改source/enabled。
- confirmed-hit activation精确检查type0/type0、counter>0、enabled!=1，再写attacker slot与enabled=1。
- reaction tail先按body-skip/positive-HP门递减，再无条件执行expired enabled清理；保留source与block。

## 验证证据

- test-first：14个预期`CS0103`；实现后compile Error 0。
- focused：job `f1b8e93cd3a34b2980555e6d27a0ffd1`，13/13。
- control/combo/proxy/carrier：job `c86c3b07e6244f88bd9c479aa8eb4706`，44/44。
- 4096次warm lifecycle调用零托管分配。
- full SelfCheck：2026-09-03 06:59:38 `PASS`；预期负向夹具清除后Console Error 0。

## 未关闭边界

为避免把2.8尾部塞进当前未对齐pass，本包不连接production。confirmed-hit writer归B5，reaction/global tail
placement归B3；连接前不能宣称mimic proxy lifecycle在游戏中生效。
