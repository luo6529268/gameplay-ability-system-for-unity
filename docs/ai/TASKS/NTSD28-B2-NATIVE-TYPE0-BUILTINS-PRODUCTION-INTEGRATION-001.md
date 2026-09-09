# Task Contract — NTSD28-B2-NATIVE-TYPE0-BUILTINS-PRODUCTION-INTEGRATION-001

> 状态：`FOCUSED_TEST_PASS / PRODUCTION_CONNECTED / RUNTIME_PENDING / JOINT_TRACE_PENDING`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2 / NATIVE-TYPE0-BUILTINS`  
> 建立日期：2026-09-04

## 目标

把已验证的NTSD 2.8 ground与air/dash/redirect built-ins按正式
`combo fields → three-button fields → direction fields → type0 built-ins`顺序接入DataOrientedCanonical
生产两遍输入链，并在同一profile中关闭后续旧`LF2CharacterActionResolver` release解析及非
`LF2Character` Character-DAT共享动作兼容解析，保证每个实体每tick只有一个动作所有者。

LegacyCanonical profile保持现有旧解析；不在本包修改built-in核心语义或environment producer。

## Authority 与当前事实

- 正式playable `input_routing.cpp:1511-1574`锁定sample/remap、edge/combo推进后依次执行combo、three、
  direction、`route_type0_builtins`，之后立即返回，不存在第二套C#/Unity release resolver。
- Unity `NTSD28InputTwoPassModule.ProcessNativeSampledState`当前只执行前三类field routing，然后projection；
  ground/air core尚未调用。
- `BattleCharacterInputActionResolver.ApplyFrameInput`虽在native profile跳过combo/direct，仍无条件调用
  `LF2Character.ProcessReleaseInput()`，会重复执行旧standing/running/jump/dash/rowing逻辑。
- 非`LF2Character`但current DAT type=Character的壳在native second pass之后仍会执行
  `RunSharedCharacterDatFrameJumpInputPhase`和`RunSharedCharacterDatStandingActionInputPhase`，同样形成旧逻辑
  二次消费。
- dead type0 suppression、current remap、edge/combo推进和exact→legacy projection已经由此前B2包完成，
  本包不改其语义。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Input/NTSD28InputTwoPassModule.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Core/BattleCharacterInputActionResolver.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`
- 新增`Assets/NTSD/Scripts/Test/Editor/NTSD28NativeType0BuiltinProductionIntegrationEditorTests.cs`及`.meta`
- 本Task、Change Record、Ledger、STATE、handoff与总表

禁止修改writer built-in核心、input producer/remap/combo算法、environment producer/raw projection、
Config/DAT、Scene/Prefab、ProjectSettings、Packages或authority。

## 不变量

- native顺序严格为combo→three→direction→ground/air built-ins→exact-to-legacy projection；ground与air域只
  能命中一个，run accumulator不得双衰减。
- native profile的后续`BattleCharacterInputActionResolver`不得再次运行legacy combo/direct/release；
  仍需保留诊断phase与canonical progress无变化提交跳过。
- native profile的共享Character-DAT壳不得再次运行旧frame-jump/standing/dash/rowing动作解析；必要的
  frame velocity tail仍按既有位置执行。
- LegacyCanonical的`LF2Character` release和共享壳兼容路径完全保留。
- action215同tick重入、state5 current-J、state4 buffer和rowing raw MP/environment gate由已闭合writer
  core定义，本包不得复制或重写。
- production pass保持allocation-free；不改变slot、AI producer/proxy、Config/DAT和表现例外。

## 验收

- test-first用完整`CharacterInputAll`证明新built-ins当前未接，并用直接resolver/共享壳证明旧release仍会
  重复执行；
- focused覆盖field→builtin顺序、action215、rowing正/负environment、LF2Character legacy suppression、
  共享壳suppression、Legacy profile保留和warm 0 B；
- compile、ground/air core、CharacterInput、B2相关、worker/AI、snapshot/checksum、SelfCheck、Console0、
  Ledger/diff通过；
- 无真实Play与C++/Unity joint trace时最多报告`PRODUCTION_CONNECTED / RUNTIME_PENDING`，不得写成B2完成。

## 回滚

移除two-pass的ground/air调用，恢复native profile下legacy release与共享壳动作解析，删除新测试；不回退
ground/air writer core或environment/data carriers。

## 当前证据

- 2026-09-04 test-first job `04e39e9d456a4c13b0947c95984c4932`：7项中2项Legacy/0B基线通过，
  5项按预期失败：native resolver仍返回true、共享壳仍跳到65、215把AnimSub重置0、field→state5落到
  legacy action90而非linked305、rowing PP保持200而未扣raw 25。红态精确覆盖三处接线缺口。
- 接线后7/7通过；审查发现dead type0仍会进入built-in并使AnimSub 17→16，新增回归job
  `94a9813952de4ecba16906664175c67d`先精确失败，再加入dead early-return后最终8/8通过。
- 最终新鲜回归：NTSD28 group167/167、CharacterInput37/37、worker/AI shadow101/101、
  snapshot/checksum/ring31/31；SelfCheck于16:36:36 PASS；预期7条错误清理后Console0。
- compile0、`git diff --check`与Ledger138/94通过；未执行真实Play与C++/Unity joint trace。
