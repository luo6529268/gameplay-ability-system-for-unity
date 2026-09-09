# NTSD28-B3-AI-RENDER-PHASE-CONSUMERS-001 — AI render-phase consumers

<!-- CHANGE-RECORD
id: NTSD28-B3-AI-RENDER-PHASE-CONSUMERS-001
status: VERIFIED
change-kind: TEST_FIRST_CONSUMER_BINDING_MIGRATION
code-path: Assets/NTSD/Scripts/Simulation/Ai/Kernel/AiSensingKernel.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/Kernel/AiDecisionKernel.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/Runtime/SimulationAiInputModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/Runtime/SimulationAiSensingModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/Runtime/SimulationAiDecisionModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28AiRenderPhaseConsumerEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28AiHeldRandomEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/AiSensingSoACandidateEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/AiSensingSoAShadowEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/AirRoleNearestEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/LooseQuadtreeNearestEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan native_ai.cpp render_phase_008 target/abnormal/held consumers; EXE B1E13AE1, closure 39DDDA15; binding proved by NTSD28-B3-C25F-J-RENDER-PHASE-BINDING-001.
evidence: TEST-FIRST-RED4 / COMPILE0 / FOCUSED121 / ALL-AI385 / NTSD28-BROAD428 / SELFCHECK-PASS / SCENE-UNCHANGED / CONSOLE0
-->

> 状态：`VERIFIED / AI_RENDER_PHASE_CONSUMERS / PHYSICAL_Y_PRESERVED`

## 改前事实

- AoS/SoA snapshot已同时承载真实Y与HitStop。
- target role/index、abnormal dispatch、held blocker和synchronized state17仍有明确Y误读。
- legacy held state17已正确读HitStop；角色专项高度分支必须继续读Y。

## 计划

先以互异矩阵与旧held断言修正取得行为红灯，再逐consumer迁移并运行full/indexed、RNG与全AI回归。

## 实际改动

- `AiSensingKernel`、`SimulationAiSensingModule`与`SimulationAiInputModule`的normal/abnormal target role及空间索引从Y迁到HitStop；nearest facts显式携带两者。
- `AiDecisionKernel`与`SimulationAiDecisionModule`的target abnormal/C8、held line blocker和synchronized state17从Y迁到HitStop；真实高度条件保持Y。
- `SimulationWorld`只新增AI input module读取HitStop的只读seam，没有新增状态carrier。
- 新增Y/HitStop互异测试；修正held断言、SoA/legacy索引fixture、AirRole/LooseQuadtree helper与BattleRuntimeSelfCheck中3组旧Y-as-render-phase夹具。

## 验证

- RED：job `5b666972b48345b68e2e1d70a4dcc9ec`，22项中4项预期失败；首次GREEN `59c909af70144067b9ac1bcdfa035a71`，22/22。
- all-AI：首次`13a89474bc374f579e85b588898520a5`仅9个被新binding supersede的fixture失败；修正后`1bd44b4ee9c8459d8663bd042a27547a`为385/385。
- related：`47399a167ec94d1a88614d2cbeaf0607` 121/121；最终compile相关`a80ce2815906478a95f60b3bbee77075` 24/24、`8665718577b64724ba0d43259cdd68d6` 3/3。
- broad：`37f79b473c6747c7a27b44971c2e0639`，`.*NTSD28.*` 428/428 PASS。
- fresh BattleRuntimeSelfCheck于2026-09-05 10:29:46 PASS；其间的3个FAIL均准确定位旧Y fixture并已闭合。清除预期7条负向rest-binding日志后Console error=0。
- source guard确认render-specific旧Y表达式为0，OID/action/state真实高度Y断言仍在；Scene hash/length/mtime保持`0D74E174...D77`/203477/`2026-09-04T13:12:45.1526434Z`。
- 未做Play：本包是确定性AI字段消费与测试夹具迁移，不改变Scene/Input/资源；总计划仍未完成。
