# NTSD28-B2-NATIVE-ACTION-DATA-CARRIERS-001 — 2.8 native action data carriers

<!-- CHANGE-RECORD
id: NTSD28-B2-NATIVE-ACTION-DATA-CARRIERS-001
status: FOCUSED_TEST_PASS
change-kind: CODE_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldEntityRuntimeSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Animation/LF2FrameData.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs
code-path: Assets/NTSD/Scripts/Animation/LF2CharacterData.cs
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeActionDataCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleWorldEntityRuntimeSnapshotEditorTests.cs
authority: NTSD 2.8-Logan battle_world.h carrier layout/defaults, input_routing.cpp consumers, battle_world.cpp initialization/reset, and the frozen native-action routing manifest.
evidence: TASK-CONTRACT-CREATED / 20-FIELD-CROSSWALK-CLOSED / AUTHORITY-46-TEST-MATRIX / TEST-FIRST-COMPILE-RED-128-EXPECTED-CS1061 / COMPILE-0 / RUNTIME-FRAME-DEFINITION-CARRIERS-WRITTEN / DEEP-COPY-SNAPSHOT-CHECKSUM-PASS / SCHEMA-3-5-8-PASS / INITIAL-FOCUSED-87DDF9-9-OF-9 / TARGETED-9BDA00-17-OF-17 / B2-BROAD-582A49-255-OF-255 / SNAPSHOT-CHECKSUM-0FCD26-31-OF-31 / FINAL-FOCUSED-3E4F7C-10-OF-10 / ZERO-ALLOCATION / FULL-SELFCHECK-2026-09-03T13:55:53-PASS / EXPECTED-NEGATIVE-LOGS-CLEARED / CONSOLE-ERROR-0 / DIFF-CHECK-PASS / LEDGER-131-80-PASS / CONFIG-DAT-UNCHANGED / ACTION-ROUTING-UNCONNECTED / CROSS-STAGE-PRODUCERS-DEFERRED
-->

> 状态：`FOCUSED_TEST_PASS / DATA_CARRIERS_READY / PRODUCTION_UNCONNECTED`

## 改前事实

- production two-pass已能生成exact edge/history/combo，但动作层仍依赖不完整的legacy字段。
- Unity runtime缺少manifest冻结的动作锁、last action、remap、bound、资源事务与feature gate carrier。
- `LF2FrameData`与converter未保留`hp`、4个direct action和7个hold/direction字段。
- character definition未显式保留`use_ai/recmp/caughtact`。
- 现有entity runtime snapshot schema为2、aggregate为4、checksum为7。

## 预期改后职责

- runtime完整持有16个新增scalar/array carrier，并遵循input-only reset与full reset边界。
- frame/definition parser无损保留2.8动作路由依赖字段。
- canonical copy、entity/aggregate snapshot与checksum覆盖全部新增carrier，remap无alias且warm path零分配。
- 本包只建立数据面；action resolver、timer和B3/B5/B7/B8 producer保持未连接。

## 验证记录

- Task Contract已创建；20-field carrier crosswalk和authority 46-test matrix沿用已验证manifest。
- test-first新增9项focused contract；2026-09-03 Unity refresh结束后Console捕获128条预期
  `CS1061`，均为本包要求的runtime/frame/definition carrier或converter API尚不存在。Editor仍在
  `NTSD_Battle` Edit Mode且compile已结束；这是实现前红灯，不计为现有production回归。
- 已写runtime 16个新增scalar/array carrier及input-only/full reset边界、canonical deep-copy与storage
  fail-closed；entity/aggregate/checksum schema已写为`3/5/8`，checksum逐项覆盖scalar和7个remap byte。
- 已写frame `hp`、4个direct action、7个hold/direction字段与parser；definition只从BMP `use_ai`和
  `stats`块的`recmp/caughtact`读取，并接入正式character data build。
- snapshot反射完整性测试已分类新`byte[7]`并要求数组无alias；invalid storage capture也按合同
  fail closed，均已进入最终验证。
- 实现后compile 0 error。初轮focused `87ddf9dac99c4d41b72b068f7cc5d91f` 9/9；
  snapshot/input migration定向`9bda007457124db882228ba30709ebcf` 17/17；包含原B2矩阵的
  `582a493f4a5949bab7112053304fd452` 255/255；snapshot/checksum下游
  `0fcd26fe722744238af1bc3c84172c4d` 31/31。
- 增补capture storage fail-closed后重新compile 0 error，最终focused
  `3e4f7c1847f34b17afd8e0e4f97cd449` 10/10；4096次canonical copy+runtime checksum为0 B。
- 最终full SelfCheck于13:55:53写入PASS；桥接等待30秒超时但任务继续完成。7条既有预期负向
  rest-binding夹具日志已识别并清空，随后Console Error 0。
- `git diff --check`通过；Change Ledger 131 records / 80 governed code files通过；Config/DAT无
  diff。本包没有触碰任务开始前已modified的Scene，也没有连接action resolver/timer/跨阶段producer。
