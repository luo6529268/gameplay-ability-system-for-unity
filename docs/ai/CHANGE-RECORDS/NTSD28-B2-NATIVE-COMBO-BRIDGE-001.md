# NTSD28-B2-NATIVE-COMBO-BRIDGE-001 — exact native combo10 core and legacy projection

<!-- CHANGE-RECORD
id: NTSD28-B2-NATIVE-COMBO-BRIDGE-001
status: FOCUSED_TEST_PASS
change-kind: SCRIPT_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Input/NTSD28NativeComboStateMachine.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeComboStateMachineEditorTests.cs
authority: NTSD 2.8-Logan input_state.h and input_routing.cpp process_sampled_inputs, advance_horizontal_combo, advance_depth_combo, advance_combos and clear_combo_attempt live path.
evidence: TEST-FIRST-10-EXPECTED-CS0103 / COMPILE-PASS / FIRST-JOB-FE9D-19-OF-19 / FINAL-JOB-AAAB-31-OF-31 / FULL-SELFCHECK-PASS-20260903-065053 / CONSOLE-ERROR-0-AFTER-EXPECTED-NEGATIVE-FIXTURE-CLEAR / EXACT-COMBO10 / NATIVE-AJ-AD-JD-NOT-ALIASED / PRODUCTION-UNCONNECTED
-->

> 状态：`FOCUSED_TEST_PASS / COMBO10_CORE_READY / PRODUCTION_UNCONNECTED`

## 实际修改

- 新增allocation-free exact state machine，分别固定logical button与physical edge两套index。
- 实现native rising order、jump suppression、edge/cooldown decay、五项history与全部十个combo状态。
- 实现same-sample direction+terminal、early-terminal保留、history ordered branch chain、proxy-tail miss clear、
  clear-attempt保留S edge/previous/current/tail。
- exact→legacy projection覆盖current/previous、七edge与`hit_Fa/Fj/Ua/Uj/Da/Dj/ja`现有consumer；
  `hit_aj/ad/jd`明确不做错误别名。

## 验证证据

- test-first：10个预期`CS0103`，无其他编译错误；实现后compile Error 0。
- first focused：job `fe9d44957ce547fd98b55073b7ad4ed5`，19/19。
- final combo/proxy/carrier：job `aaabe5a774ef496e97fda7f1e0d56c0a`，31/31。
- 4096次warm process+projection零托管分配。
- full SelfCheck：2026-09-03 06:50:53 `PASS`；预期负向夹具日志清除后Console Error 0。

## 未关闭边界

新核心未被production调用；human/AI producer、proxy routing、native combo field action消费与成功后
`clear_combo_attempt`回写必须在后续包闭合。本包不能单独证明玩家组合键已按2.8运行。
