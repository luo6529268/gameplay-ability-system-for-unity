# NTSD28-B3-C25G-FRAME-BODY-001 — C25g common frame body

<!-- CHANGE-RECORD
id: NTSD28-B3-C25G-FRAME-BODY-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR_ALIGNMENT
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterFrameTickPass.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25GFrameBodyEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan battle_world.cpp step_frames_range/apply_native_type3_frame_hp_drain; EXE B1E13AE1, closure 39DDDA15.
evidence: TEST-FIRST-RED4 / COMPILE0 / FOCUSED4 / RELATED85 / NTSD28-BROAD432 / SELFCHECK-PASS / SCENE-UNCHANGED / CONSOLE0
-->

> 状态：`VERIFIED / C25G_COMMON_CORE / DOWNSTREAM_BEHAVIOR_ROUTED`

## 改前事实

- exact `LF2Character`与fallback分别维护两份相近frame算法，Authority只有单一all-object body。
- Unity缺terminal low-slot dead-state14 hold；type3 state3007误走普通hit_a drain；current type2 grounded/low-Vx有Authority未见的额外早退。
- audio、collision-Y、negative transition cost和lifecycle pending已有明确后续owner，本包不跨阶段吞并。

## 计划

先加入4组Authority-directed红灯；再让ECS exact wrapper复用common core，最小修正三个独立分支，随后运行frame/late/NTSD28/SelfCheck门禁。

## 实际改动

- `LF2Entity.RunNativeC25FrameBodyForWorldPass`成为exact/fallback共享核心；`RunCommonFrameTick`只做兼容入口转发。
- ECS exact pass只保留资格检查与计数，删除第二份frame算法并调用共享核心。
- production marker下：terminal slot0..19 type0 dead state14且无revival时不推进；type3 state3007活体不扣hit_a、死体选hit_d或10并继续counter；Unity旧type2 grounded/low-Vx early return不再影响production。
- direct legacy调用仍保留旧type2 gate及非native dead-state行为，避免把测试/工具兼容入口冒充production规则。

## 验证

- RED `040d55a915a44217bec73385021e3ee7`：4/4按预期失败；GREEN `df142da97807480c953ed5b03352b4b2`：4/4。
- related `23753dd8e8d54052b6a205c4348ef87c`：frame/late/C25共85/85。
- broad `9c09b270aaa3413499b72a36935594f7`：`.*NTSD28.*` 432/432。
- 2026-09-05 10:50:29 SelfCheck PASS；清除预期负向日志后Console error=0。
- Scene SHA/length/mtime保持`0D74E174...D77`/203477/`2026-09-04T13:12:45.1526434Z`；未Play。剩余C25g行为已明确路由，不能据此宣称完整frame对齐。
