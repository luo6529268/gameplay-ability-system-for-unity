# NTSD28-B2-AI-SAMPLE-PROXY-TWO-PASS-001 — production producer/proxy-routing split

<!-- CHANGE-RECORD
id: NTSD28-B2-AI-SAMPLE-PROXY-TWO-PASS-001
status: FOCUSED_TEST_PASS
change-kind: SCRIPT_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Input/NTSD28InputTwoPassModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorldHooks.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterInputPass.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28AiSampleProxyTwoPassEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/CharacterInputLiveSlotLoopEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleSimulationWorkerBoundaryEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/AiDecisionSoAShadowEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/AiSensingSoAShadowEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleComboPlayModeProbeEditor.cs
authority: NTSD 2.8-Logan simulation_tick_driver.cpp physical-slot first producer/sample/recording pass and second proxy/input-routing pass; exact +0xBE..+0xDE copy gates.
evidence: TEST-FIRST-7-EXPECTED-CS1061 / COMPILE-0 / FOCUSED-7-OF-7-8894CE87 / LIVE-SLOT-37-OF-37-A9B86158 / AI-80-OF-80-F5A6985D / RELATED-144-OF-144-895CCE1D / B2-RELATED-198-OF-198-46F30A5E / SELFCHECK-PASS-2026-09-03-075126 / PLAY-DDJ-PASS-1588-1590-1601 / PLAY-DRA-PASS-2614-2616-2627 / CONSOLE-0 / PRODUCTION-TWO-PASS / LOWER-TARGET-HIGHER-AI-SOURCE / LEGACY-AI-TO-EXACT-MIGRATION-BRIDGE-DECLARED / NATIVE-COMBO-PRODUCER-AND-JOINT-TRACE-PENDING
-->

> 状态：`FOCUSED_TEST_PASS / PRODUCTION_TWO_PASS_READY / REAL_PLAY_INPUT_ROUTE_PASS / NATIVE_COMBO_PRODUCER_AND_JOINT_TRACE_PENDING`

本包接入真实production两遍与exact proxy copy；不把legacy AI projection包装成完整native AI/combo对齐。

## 实际实现

- `SimulationWorld.CharacterInputAll`改为两个独立升序循环：第一遍完成全部producer/sample并冻结exact
  输入，第二遍先执行proxy copy再路由动作。
- `LF2Entity`与`LF2Character`拆出producer/routing seam；ECS pass只负责第二遍动作路由，避免重复AI
  producer。
- `NTSD28InputTwoPassModule`负责33-byte producer freeze、严格proxy gate、exact copy和诊断计数；低slot
  proxy可读取高slot AI的同tick producer结果，nested proxy按第二遍升序传播。
- AI执行快照在producer后发布决策可见行，在routing后只发布最终保留态；后续AI不再看到同tick前序slot
  的动作路由结果。legacy AI→exact projection明确保留为迁移桥。
- 既有SelfCheck测试替身迁到routing seam；真实Play组合键探针的合成物理状态至少保持两个tick，以跨越
  2tu采样窗口。

## 验证结果

- test-first：新module/seam缺失时7项按预期CS1061失败；实现后focused `8894ce87...` 7/7。
- 既有live-slot `a9b86158...` 37/37；AI shadow/sensing `f5a6985d...` 80/80；声明路径联合
  `895cce1d...` 144/144；B2输入/RNG联合 `46f30a5e...` 198/198。
- Dedicated worker定向 `f678...` 通过；此前失败是2tu旧夹具从错误相位直接执行tick2，已修正夹具，
  production没有为测试退回1tu。
- 完整SelfCheck在迁移两个旧combined-input替身、重基线route→AI不可见性后于
  `2026-09-03 07:51:26 +08:00` PASS。
- 真实`NTSD_Battle` Play：DDJ在tick `1588→1590→1601`、DRA在tick `2614→2616→2627`，
  两者均1/1/1次物理脉冲进入authored target frame；退出Play后未改Scene。
- 清空SelfCheck预期负例日志后Console error为0。

## 未关闭边界

- exact native combo10目前仍由legacy字段projection填充，尚未成为human/AI producer真值。
- native `hit_aj`、`hit_ad`、`hit_jd`与10个combo action reader尚未接入production routing。
- exact proxy尚缺权威/Unity同seed、同输入、同tick联合trace；RNG call-site迁移也未开始。因此本包只标
  `FOCUSED_TEST_PASS`，不标`VERIFIED`或B2 aligned。
