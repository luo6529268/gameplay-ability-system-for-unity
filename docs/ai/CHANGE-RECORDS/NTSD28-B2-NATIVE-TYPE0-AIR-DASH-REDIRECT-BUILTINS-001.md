# NTSD28-B2-NATIVE-TYPE0-AIR-DASH-REDIRECT-BUILTINS-001 — native air/dash/redirect built-ins core

<!-- CHANGE-RECORD
id: NTSD28-B2-NATIVE-TYPE0-AIR-DASH-REDIRECT-BUILTINS-001
status: FOCUSED_TEST_PASS
change-kind: CODE_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCharacterActionWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeType0AirDashRedirectBuiltinsEditorTests.cs
authority: NTSD 2.8-Logan playable input_routing.cpp FUN_00412E80 action215/state4/state5/state85/state86/action182/action188 live path and input_routing_tests.cpp locked observations.
evidence: TASK-CONTRACT-CREATED / AUTHORITY-SOURCE-AND-TESTS-REREAD / ENVIRONMENT-CARRIER-READY / DATA-SEAMS-READY / TEST-FIRST-RED-CS1061 / COMPILE-0 / NEW-AIR-10-OF-10 / GROUND-AIR-22-OF-22 / NTSD28-GROUP-159-OF-159 / CHARACTER-INPUT-37-OF-37 / SNAPSHOT-CHECKSUM-RING-31-OF-31 / SELFCHECK-PASS-20260904-161504 / EXPECTED-ERRORS-7-THEN-CONSOLE-0 / LEDGER-137-93-PASS / PRODUCTION-INTEGRATION-DEFERRED / CONFIG-DAT-SCENE-UNCHANGED
-->

> 状态：`FOCUSED_TEST_PASS / GROUND_AIR_CORE_READY / PRODUCTION_UNCONNECTED`

## 改前事实

- ground built-ins core已完成但未生产接线；writer尚无独立air/dash/redirect入口。
- jump_attack、sky_light_throw、dash/rowing BMP fields及`EnvironmentState320` carrier已具备。
- legacy path不能作为2.8 authority；若此时直接接ground会造成air状态仍由旧实现接管的混合所有权。

## 预期改后职责

- writer可按正式live path独立路由action215、state4/5、state85/86、action182/188。
- exact current/buffer、direct write、resource、facing、counter和motion副作用由focused tests锁定。
- production two-pass与legacy skip保持不变；后续独立integration包一次接入ground+air。

## 验证记录

- Task Contract在任何本包脚本修改前建立。
- 2026-09-04 test-first：新增10个focused tests并请求Unity编译；仅因writer缺失
  `RouteNativeAirDashRedirectBuiltins`而产生CS1061，证明测试能够捕获缺失air core；生产脚本尚未修改。
- 实际实现：
  - 新入口按exact action/state域拥有action215、182/188、state4/5/85/86；不加ObjType gate，只有拥有域
    衰减一次`AnimSub`，未知state返回false。
  - action215按defend→jump→depth顺序直写，并重读`Frame.D`后同tick进入state5；不写mirror/lock/cost，
    不清counter。
  - state5实现exclusive orientation、213/214 normalization及216/217保护；forward current J触发action90
    adjusted gated MP，或linked jump/sky字段并`Vy -= 1`、counter归0。
  - state4实现严格`Y < groundY`、attack buffer、action80 clamped adjusted MP和linked exact fallback。
  - state85/86实现先定向，只有forward state85的非零action推进1。
  - rowing严格使用独立`EnvironmentState320`、current+edge jump、HP与combo gate；182/188分别读取
    frame100/108 raw MP，不adjust且不更新普通累计；按朝向/原Vx选择100/108、counter归0并应用
    rowing height/distance阈值。
  - `NativeLinkedActionField`仅增加`JumpAttack`与`SkyLightThrow`到已存在的数据carrier。
- Unity脚本编译：最新生产实现编译段0 error并完成assembly reload。
- EditMode：
  - job `125f63d94e904eb3a326cde8b2c45c07`：新air core 10/10。
  - job `fb17c3a5e96b4acead86c52159754c50`：ground+air 22/22。
  - job `dd644dc7ad884a4a98e21ba37979e8cd`：NTSD28 group 159/159。
  - job `b94ac924a9f64feabfc5f4aa128dc7cd`：CharacterInput live slot loop 37/37。
  - job `06c9d21331ca49968e18559da74d45df`：snapshot/restore/checksum/ring 31/31。
- `BattleRuntimeSelfCheck`：2026-09-04 16:15:04生成PASS；读取到7条预期负向注册/绑定错误，清理后
  Console error=0。
- `git diff --check`通过；`Tools/Validate-ChangeLedger.ps1`通过（137 records、93 governed code files）。
- 未验证/未实现：生产两遍输入接线、legacy builtin ownership切换、真实Play与C++/Unity joint trace；因此
  不报告air behavior已正式对齐。

## 回滚说明

移除本包入口/helper、linked enum两项和新测试即可；ground core、environment/data carriers及生产调用链
可保持原状，未写入Config/DAT/Scene/Prefab或authority。
