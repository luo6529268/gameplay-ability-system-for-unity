# NTSD28-B2-NATIVE-DIRECT-HOLD-DIRECTION-ROUTING-001 — native direct/hold/direction routing

<!-- CHANGE-RECORD
id: NTSD28-B2-NATIVE-DIRECT-HOLD-DIRECTION-ROUTING-001
status: FOCUSED_TEST_PASS
change-kind: CODE_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCharacterActionWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Input/NTSD28InputTwoPassModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Core/BattleCharacterInputActionResolver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeDirectHoldDirectionRoutingEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/CharacterInputLiveSlotLoopEditorTests.cs
authority: NTSD 2.8-Logan input_routing.cpp try_field, route_three_button_fields and route_direction_fields in playable build closure; input_routing_tests.cpp direct/hold/depth/rejection/counter cases.
evidence: TASK-CONTRACT-CREATED / SOURCE-CHAIN-CLOSED / PREDECESSOR-COMBO-ACTION-FOCUSED-PASS / CARRIERS-READY / TEST-FIRST-RED-31-EXPECTED-CS-ERRORS / OUTSIDE-NEW-TEST-CS-ERRORS-0 / PRODUCTION-CODE-WRITTEN / UNITY-COMPILE-0 / FOCUSED-12-12 / INPUT-OWNER-37-37 / B2-RELATED-241-241 / SNAPSHOT-CHECKSUM-32-32 / WORKER-AI-SHADOW-101-101 / SELFCHECK-PASS-145549 / EXPECTED-NEGATIVE-LOGS-7-THEN-CONSOLE-0 / CONFIG-DAT-SCENE-UNCHANGED / TYPE0-BUILTINS-DEFERRED / SYNC-82-83-84-DEFERRED / JOINT-TRACE-PENDING
-->

> 状态：`FOCUSED_TEST_PASS / DIRECT_HOLD_DIRECTION_READY / PRODUCTION_CONNECTED / BUILTINS_JOINT_TRACE_PENDING`

## 改前事实

- DataOriented production second pass已完成exact combo和generic action transaction，但随后shared legacy
  resolver仍拥有`hit_a/d/j`；native hold/direction没有consumer。
- frame/parser和exact current/previous/edge carrier已存在，当前包不需要内容迁移或schema升级。
- release resolver包含type-0/common built-ins；本包必须把它保留在native fields之后。

## 预期改后职责

- `BattleCharacterActionWriter`无分配执行native three-button和direction field顺序、比较与exact cleanup。
- `NTSD28InputTwoPassModule`按combo→three-button→direction调用；随后才投影legacy。
- DataOriented shared resolver不再重复处理legacy combo/direct fields，只执行release；Legacy保持原路径。

## 验证记录

- Task Contract已建立；正式source/build/test归属与前序依赖已闭合。
- 2026-09-04 test-first：新增12项focused test，并将既有DataOriented AI direct-owner断言改为
  native second-pass owner合同。Unity编译捕获31条唯一预期错误：仅缺
  `NTSD28NativeFieldRoutingResult`、`RouteNativeThreeButtonFields`和
  `RouteNativeDirectionFields`；新测试之外的C#编译错误为0。production尚未修改。
- 2026-09-04 production已写：
  - `BattleCharacterActionWriter`新增无分配routing result、three-button和direction exact路由；
    每个field attempt重新读取当前frame，非零字段无论action是否applied都按caller规则消费字节。
  - `NTSD28InputTwoPassModule`生产顺序推进为combo→three-button→direction→legacy projection。
  - `BattleCharacterInputActionResolver`在DataOriented native owner下跳过legacy combo/direct，仍保留
    后续release处理；LegacyCanonical分支不变。
  - `git diff --check`通过；Unity compile/focused/broad/self-check尚待运行。
- 2026-09-04 final validation：
  - gameplay Unity端口6400确认`NTSD_Battle`且非Play；强制refresh在domain reload时短暂断开，
    `Editor.log`随后确认`Tundra build success`、C# error 0，并成功重连。
  - 新focused job `b05bbf15f44f466e959e3d8b4d337169`：12/12 PASS；既有
    `CharacterInputLiveSlotLoopEditorTests` job `38119d84b7b54b7aabce145317f046eb`：37/37 PASS。
  - 显式21类B2相关宽回归job `77d94b44217a41d89ee3e6553e26bdca`：241/241 PASS。
  - snapshot/checksum/restore/ring job `14c214dc37b34a168a5f402676ea1035`：28/28 PASS；
    `LockstepFrameHistoryRingEditorTests` job `13389ef6d8344400aaead849a7158569`：4/4 PASS。
  - `BattleSimulationWorkerBoundary`、`AiDecisionSoAShadow`、`AiSensingSoAShadow`邻接回归job
    `360754eb2dbb4916aa2b4f1512904062`：101/101 PASS。
  - request-file full `BattleRuntimeSelfCheck`于14:55:49写出`PASS`。Console仅有7条既有
    rest-binding负向夹具错误；清除后fresh error读取0。
  - `git diff --check`再次通过；最终Change Ledger validator结果见本记录后续项。
- final governance：`Tools/Validate-ChangeLedger.ps1`通过，133 records / 86 governed code files；
  全工作树`git diff --check`通过。`Assets/NTSD/Config`无diff；现有
  `Assets/NTSD/Scene/NTSD_Battle.unity`差异为任务开始前状态，本包未修改。
- 本包只证明direct/hold/direction生产接线及覆盖到的回归已通过；未做真实Play按键验收或
  C++/Unity joint trace，type-0 built-ins与sync82/83/84仍未实施，B2及全项目均未完成。
