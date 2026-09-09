# NTSD28-B5-TYPE3-ATTACKER-POST-HIT-ACTION-001 — type3 attacker post-hit action

<!-- CHANGE-RECORD
id: NTSD28-B5-TYPE3-ATTACKER-POST-HIT-ACTION-001
status: VERIFIED
change-kind: TEST_FIRST_AUTHORITY_BEHAVIOR_PORT
code-path: Assets/NTSD/Scripts/Animation/LF2FrameData.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type3AttackerPostHitActionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan apply_native_type3_post_hit_action used by unarmored/reduced attacker post-hit; EXE B1E13AE1, closure 39DDDA15.
evidence: AUDIT-VERIFIED / AUTHORITY-STATE3000-HIT_FJ-DVX / STATE3007-COVER2-3 / TEST-FIRST-94ea22161e1c4ec59ff9d68d3b977e57-9FAIL / COMPILE0 / FOCUSED-00cb663b2bb94bbbbc02b02658d2d45f-11OF11 / B5-HITPLAN-b3ad577e11ae4c47935b65968131174e-348OF348 / EXACT99-BROAD-18753cae132f4771898b7cb27bb57364-724OF724 / SELFCHECK-PASS-20260905T173452Z / FILTERED-ERROR-CS0 / EXPECTED-TEST-ERROR-LOGS7 / LEDGER266-232-PASS / SCENE-D4266C6D-UNCHANGED
-->

> 状态：`VERIFIED / TYPE3_ATTACKER_POST_HIT_ACTION_ALIGNED`

## Authority 与 Unity 原状

Authority 读取当前 frame state/cover/hit_Fj，action 0 回退 10；写 action、frame counter=0、motion.x=0，
再读取新 action frame 的 dvx 写 motion.z。Unity character、weapon、special/other、alternate 四个 actual tail
及其 hit-plan 投影固定 action10；三条路径错误读取 dvz 或漏写 Z，且没有 frame-level cover carrier。

## 计划与边界

新增 frame-level cover 与 converter 映射；建立单一 resolver/application seam，并替换四处 actual 与对应 hit-plan
投影。target type3 continuation、kind catalog、owner/control/impulse、content/Scene 均排除。

## 验收状态

- 新增 frame-level `cover` 并由 converter 与 CPoint/WeaponPoint cover 分离解析。
- 单一 resolver 严格执行 state3000 或 state3007+cover2/3、hit_Fj 仅 0 回退10、新动作帧 dvx→Z；
  selected frame 缺失保持 Z，负 hit_Fj 不会被误判成 ineligible。
- character、weapon、special/other、alternate 四条 actual 与四条 hit-plan projection 共用同一 decision。
- 移除旧 OID209/Karasu target-dependent attacker skip；target kind transform 仍留后续独立包。
- red `94ea22161e1c4ec59ff9d68d3b977e57` 9/9；fresh compile0；focused
  `00cb663b2bb94bbbbc02b02658d2d45f` 11/11；B5+hit-plan
  `b3ad577e11ae4c47935b65968131174e` 348/348；exact99 broad
  `18753cae132f4771898b7cb27bb57364` 724/724。
- SelfCheck 2026-09-05T17:34:52Z PASS；filtered `error CS`=0，Console中的7条error-type为既有
  注册回滚/绑定拒绝预期测试日志；Ledger266/232 PASS；Scene `D4266C6D...583B` unchanged。
