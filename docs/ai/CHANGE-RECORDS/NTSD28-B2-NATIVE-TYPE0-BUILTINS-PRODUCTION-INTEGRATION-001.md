# NTSD28-B2-NATIVE-TYPE0-BUILTINS-PRODUCTION-INTEGRATION-001 — production ownership integration

<!-- CHANGE-RECORD
id: NTSD28-B2-NATIVE-TYPE0-BUILTINS-PRODUCTION-INTEGRATION-001
status: RUNTIME_PENDING
change-kind: CODE_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Input/NTSD28InputTwoPassModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Core/BattleCharacterInputActionResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeType0BuiltinProductionIntegrationEditorTests.cs
authority: NTSD 2.8-Logan playable input_routing.cpp step_sampled exact routing order and single route_type0_builtins ownership; completed ground and air core packages.
evidence: TASK-CONTRACT-CREATED / PRODUCTION-CALL-CHAIN-CLOSED / THREE-DUPLICATE-OWNERSHIP-SITES-IDENTIFIED / GROUND-AIR-CORE-READY / TEST-FIRST-RED-5-EXPECTED-FAILURES-2-BASELINES-PASS / DEAD-EARLY-RETURN-RED-17-TO-16 / COMPILE-0 / INTEGRATION-8-OF-8 / NTSD28-GROUP-167-OF-167 / CHARACTER-INPUT-37-OF-37 / WORKER-AI-SHADOW-101-OF-101 / SNAPSHOT-CHECKSUM-RING-31-OF-31 / SELFCHECK-PASS-20260904-163636 / EXPECTED-ERRORS-7-THEN-CONSOLE-0 / LEDGER-138-94-PASS / PRODUCTION-CONNECTED / RUNTIME-AND-JOINT-TRACE-PENDING / ENVIRONMENT-PRODUCER-UNCHANGED / CONFIG-DAT-SCENE-UNCHANGED
-->

> 状态：`FOCUSED_TEST_PASS / PRODUCTION_CONNECTED / RUNTIME_PENDING / JOINT_TRACE_PENDING`

## 改前事实

- native second pass stops after direction fields and projects exact state；ground/air core仅可直接测试。
- native profile后续LF2Character resolver仍执行旧release；shared Character-DAT shell仍执行旧兼容动作路径。
- 因此只增加built-in调用会留下依赖新frame state的二次旧解析，不能作为原子接线。

## 预期改后职责

- native second pass在projection前唯一调用ground或air core。
- native profile所有后续动作resolver只保留无动作的progress/diagnostic边界，不再调用legacy release。
- shared Character-DAT shell在native profile跳过旧动作解析但保留frame velocity tail；Legacy profile不变。

## 验证记录

- Task Contract在任何本包脚本修改前建立。
- 首次7项红测包含一条无效共享壳夹具（fallback type仍为Other）；改为非LF2Character但
  `ReleaseEntityType=Character`的精确壳后重跑。
- test-first job `04e39e9d456a4c13b0947c95984c4932`：Legacy profile保留与warm 0 B两项通过；其余
  5项精确失败：native LF2Character release未抑制、native shared shell跳65、action215旧路径将AnimSub
  清0、field→state5最终走legacy action90而非linked305、rowing未扣raw frame100 MP。生产脚本仍未修改。
- 实际实现：
  - `NTSD28InputTwoPassModule`在combo→three→direction之后、projection之前先尝试ground；仅ground未拥有时
    再尝试air，避免双衰减并保持正式顺序。
  - dead type0由native combo state machine清空输入后直接跳过全部field/built-in路由，仍执行exact→legacy
    projection和`InputState`同步；正式early return不再额外衰减`AnimSub`。
  - `BattleCharacterInputActionResolver`在native profile保留combo/direct与release诊断phase，但两个动作路径
    都不再执行；Legacy profile无变化。
  - 非`LF2Character`的Character-DAT壳在native profile跳过旧frame-jump/standing/dash/rowing解析，仅保留
    原位置的frame velocity tail；Legacy profile无变化。
- 接线后首轮job `f2bcde8cffbc45179f3cc433244bee73` 7/7；core+integration job
  `c25640b8d21e4b81a99f719253e97668` 29/29。
- 代码审查新增dead early-return测试；job `94a9813952de4ecba16906664175c67d`先以expected17/actual16
  精确红灯，修正后job `e0910bca3bab45f1b5e63a168aa222b4` 8/8。
- 最终新鲜EditMode：
  - job `b3c94929af8643b39f98dcad86f742a5`：NTSD28 group 167/167。
  - job `e80b4658d42e4a7fab3a92c9645cd142`：CharacterInput live slot loop 37/37。
  - job `a34e5f6ba86441de9e76404d5061aa74`：worker/AI shadow 101/101。
  - job `50e89241b09944fa83b3721b3fc9b791`：snapshot/restore/checksum/ring 31/31。
- Unity脚本编译最新段0 error并完成assembly reload。
- `BattleRuntimeSelfCheck`：2026-09-04 16:36:36生成PASS；读取7条预期负向注册/绑定error后清理，
  Console error=0。
- `git diff --check`通过；`Tools/Validate-ChangeLedger.ps1`通过（138 records、94 governed code files）。
- 未验证：真实Play的物理按键/AI场景、正式EXE与Unity同seed同tick joint trace；故状态保持RUNTIME_PENDING，
  不将production-connected扩大为B2已对齐。

## 回滚说明

按Task Contract恢复two-pass不调用built-ins、native resolver继续旧release、共享壳继续旧动作解析并删除新测试；
ground/air core与carriers不需回退，Config/DAT/Scene/Prefab/authority均未写入。
