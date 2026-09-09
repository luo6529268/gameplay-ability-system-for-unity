# Task Contract — NTSD28-B5-TYPE3-ATTACKER-POST-HIT-ACTION-001

> 状态：`VERIFIED / TYPE3_ATTACKER_POST_HIT_ACTION_ALIGNED`
> 依赖：`NTSD28-B5-TYPE3-POST-HIT-ACTION-AUDIT-001 / VERIFIED`

## 目标

将正式 NTSD 2.8-Logan 的攻击者 post-hit action 统一接入 Unity actual 与 hit-plan：state3000，或
state3007 且 frame-level cover=2/3 时，读取当前帧 hit_Fj（0 回退 10），清 frame counter 与 X motion，
并把新动作帧 dvx 原样写入 Z motion。

## 路径

- `Assets/NTSD/Scripts/Animation/LF2FrameData.cs`
- `Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type3AttackerPostHitActionEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`

## 不变量

- unarmored 与 reduced attacker post-hit 共用相同 resolver；state1002/state2000 的先行分支不变。
- selected frame 缺失时仍写 action/counter/X，但保持原 Z；不能用 dvz 替代 dvx。
- frame-level cover 与 CPoint/WeaponPoint cover 是三个不同字段，不能复用或覆盖。
- 不修改 target type3 ownership/action、kind catalog、effect override/direct post-effect、audio/spark 或内容文件。
- 不修改 Scene、Prefab、ProjectSettings 或 authority 目录；保留并发用户 Scene 修改。

## 验收与回滚

test-first 覆盖 converter、state3000 hit_Fj/fallback、state3007 cover gate、selected dvx/Z、missing selected frame、
standard/alternate/non-character actual 和 hit-plan parity；然后运行 fresh compile、focused、相关 hit/B5、精确 broad、
SelfCheck、Console、Scene hash 与 Ledger validator。

回滚只移除本 Change 新增的 frame cover carrier、共享 resolver、actual/hit-plan 接线与测试更新，不回退前置
effect action packages。

## 最终证据

- test-first：`94ea22161e1c4ec59ff9d68d3b977e57`，9/9 按预期失败；缺 frame cover、固定 action10、
  dvz→Z 与 state3007 gate 均被直接捕获。
- fresh compile：`Assembly-CSharp.dll` 17:26:47Z、`Assembly-CSharp-Editor.dll` 17:31:13Z；filtered `error CS`=0。
- focused：`00cb663b2bb94bbbbc02b02658d2d45f` 11/11。
- B5 + hit-plan：`b3ad577e11ae4c47935b65968131174e` 348/348。
- exact 99-class broad：`18753cae132f4771898b7cb27bb57364` 724/724；无中途 bridge poll 污染。
- SelfCheck：2026-09-05T17:34:52Z PASS。
- 最终 Console 保留 7 条既有注册回滚/绑定拒绝预期测试日志，但 filtered `error CS`=0；
  Change Ledger 266 records / 232 governed code files PASS；Scene 保持并发基线 `D4266C6D...583B`。
