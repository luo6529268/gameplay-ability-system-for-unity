# NTSD28-B2-NATIVE-TYPE0-GROUND-BUILTINS-001 — native ground built-ins core

<!-- CHANGE-RECORD
id: NTSD28-B2-NATIVE-TYPE0-GROUND-BUILTINS-001
status: FOCUSED_TEST_PASS
change-kind: CODE_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCharacterActionWriter.cs
code-path: Assets/NTSD/Scripts/Animation/Character/LF2FrameCache.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeType0GroundBuiltinsEditorTests.cs
authority: NTSD 2.8-Logan input_routing.cpp movement/direct/resource/linked helpers and action110/state19-301/state0-1-2 route_type0_builtins branches in playable closure; input_routing_tests.cpp cases 1,2,11-14,29-38.
evidence: TASK-CONTRACT-CREATED / SOURCE-CHAIN-CLOSED / DATA-SEAMS-FOCUSED-PASS / TEST-FIRST-RED-37-EXPECTED-CS1061 / TEST-SYNTAX-CORRECTED-BEFORE-RED-BASELINE / CURRENT-COMPILE-SEGMENT-OUTSIDE-NEW-TEST-0 / GROUND-CORE-CODE-WRITTEN / UNITY-COMPILE-0 / FIRST-FOCUSED-10-11 / FRAME-CACHE-600-EXCLUSIVE-BLOCKS-AUTHORITY-650 / AUTH-DECODED-MAX-FRAME-856 / FRAME-CACHE-857-EXCLUSIVE-WRITTEN / FINAL-FOCUSED-12-12 / B2-BROAD-259-259 / SELFCHECK-RED-DATA01BC-OLD-600-GUARD / SELFCHECK-RED-FT02-OLD-600-GUARD / SELFCHECK-RED-LC02-OLD-600-PROBE / SELFCHECK-PASS-154650 / EXPECTED-NEGATIVE-LOGS-7-THEN-CONSOLE-0 / LEDGER-135-91 / PRODUCTION-UNCONNECTED / AIR-DASH-REDIRECT-DEFERRED / CONFIG-DAT-SCENE-UNCHANGED / JOINT-TRACE-PENDING
-->

> 状态：`FOCUSED_TEST_PASS / GROUND_CORE_READY / HIGH_FRAME_READY / PRODUCTION_UNCONNECTED / AIR_PENDING`

## 改前事实

- generic combo/direct/hold/direction已production connected，但two-pass随后仍由legacy release处理built-ins。
- Unity legacy resolver存在近似standing/running/heavy行为，但使用legacy RNG、固定动作、CLR/held helper和
  不同资源/计数副作用；不能作为2.8规则owner。
- 4组movement sequence与9个linked stats carrier/parser已通过focused，本包可只读消费。

## 预期改后职责

- action writer提供state-domain明确、无分配的native ground core和direct/resource/sequence/linked helper。
- 本包test直接调用core并冻结权威副作用；two-pass不调用它，legacy owner不变。
- air/dash/redirect core完成后，另包统一接入并一次性关闭legacy built-in owner。

## 验证记录

- Task Contract已建立。
- 新增11项focused contract；首轮1条NUnit API不兼容已先修正，第二轮最后编译段为37条唯一
  CS1061，全部只引用尚缺的`RouteNativeGroundBuiltins`，新测试之外错误0。production core尚未写。
- ground core已写入action writer：domain gate、AnimSub衰减、custom/fallback movement、normal/heavy
  state0/1/2、action110/state19/301、direct restart、clamp/gated MP、linked slot stats与sync82/83/84。
  two-pass/resolver未改，故production仍不可达；diff check通过，Unity验证待跑。
- Unity编译0 error。首轮focused job `25ea5037f7ab4bd7a423b2612d1c3bf6`为10/11；精确单跑
  `6c2319d993dc476c9c8e75bf326f86db`确认同一失败：custom sequence选择650后`Frame.D=null`。
  源码定位`LF2FrameCache.MaxFrameIdExclusive=600`静默丢弃650；formal decoded runtime全量扫描最大
  frame id为856（`c/asu/asu.dat`）。在改cache前已将该文件、857 exclusive与回滚加入Task/Record。
- `LF2FrameCache.MaxFrameIdExclusive=857`与856/857测试写入后，compile0，final focused job
  `0adb9783a9c74d0690d84cf11658a366`为12/12，B2 broad
  `919effdbf8944879a4fa8e1aefb567a4`为259/259。
- 15:39:36 full SelfCheck唯一失败是`CheckDataDefaultsFrameCacheAndCpointAliases`仍硬编码599/600；
  production/focused均未失败。修改任何self-check前已将该路径与精确重基线加入治理范围。
- SelfCheck重基线留痕：第一轮DATA-01B/C改为`MaxFrameIdExclusive-1`与exclusive边界；第二轮
  15:43:03 FT-02仍用600作out-of-range，改为constant；第三轮15:44:54 LC-02 invalid-frame probe
  仍手写600，改为constant。三处只修测试基线，production frame lifecycle逻辑未改。
- 最终编译0 error；focused复跑job `d708d38604b145608e7e460a83b9c7e3` 12/12；15:46:50
  full SelfCheck `PASS`。Console仅7条预期rest-binding负向日志，清除后fresh error0。
- `git diff --check`与Change Ledger通过（135 records / 91 governed code files）。Config/DAT、Scene、
  two-pass/resolver、air/dash/redirect和authority未由本包修改；ground core仍不可达于production。
