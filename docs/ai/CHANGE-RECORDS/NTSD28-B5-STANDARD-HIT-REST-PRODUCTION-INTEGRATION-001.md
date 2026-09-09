# NTSD28-B5-STANDARD-HIT-REST-PRODUCTION-INTEGRATION-001 — production rest integration

<!-- CHANGE-RECORD
id: NTSD28-B5-STANDARD-HIT-REST-PRODUCTION-INTEGRATION-001
status: VERIFIED
change-kind: TEST_FIRST_RUNTIME_ALIGNMENT
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5StandardHitRestProductionIntegrationEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
authority: NTSD 2.8-Logan BattleWorld28::apply_standard_hit_rest at battle_world.cpp 3854-3920 and callers 6630/6819; EXE B1E13AE1, closure 39DDDA15.
evidence: RED-7 / FOCUSED-7 / B5-HITPLAN-411 / NTSD28-BROAD-692 / SELFCHECK-PASS / SCENE-UNCHANGED / LEDGER-PASS
-->

> 状态：`VERIFIED / PRODUCTION_CONNECTED / STANDARD_REST_ALIGNED`

## 原状与差异

`BattleDamageWriter`的character/weapon/special/other/initial-pair路径和HitPlan六个standard投影仍分别写
固定`3/-3`、未缩减arest、raw vrest；因此recover、definition effect、nondefault timing reduction与native
uint8输入尚不能进入生产行为。alternate/reduced rest是另一authority函数，本包明确不碰。

## 验收状态

- test-first red：job `dd93e4ba1c674c698588fd0590b4b71d`，7/7按预期失败，实际值仍显示旧固定`3/-3`。
- focused green：job `367f42a0f9204f92a796ef28460220be`，7/7通过。
- B5 + HitPlan：job `b723eefcd0ed4da89377399c8976d612`，411/411通过。
- NTSD28 broad：job `6012829e7fb24534b595488788293884`，692/692通过。
- compile：Runtime DLL `2026-09-05T21:56:50Z`、Editor DLL `2026-09-05T21:52:45Z`；filtered Console 7条均为既有预期rest-binding自检日志，无CS诊断。
- SelfCheck：`2026-09-05T22:03:59Z` PASS。
- Scene：SHA-256 `D4266C6D0A802975B50E481E1D01662DC79AEF168C6E2DE14AE382396FDA583B`、长度205625、mtime `2026-09-05T15:44:52.7794120Z`不变。
- Ledger：validator PASS（279 records / 240 governed code files）。

## 实际实现

- `BattleDamageWriter.ApplyNativeStandardHitRest`成为character、weapon、special/other与initial matching pair唯一actual适配器，读取双方definition effect和World timing reduction后提交pure result。
- `BattleEcsHitExecutionPlan.ProjectNativeStandardHitRest`成为六个standard writer projection唯一投影适配器；ShadowCompare覆盖carrier、byte wrap与零差。
- 新增7个focused cases，覆盖四类actual owner、recover/effect suppress、initial early和HitPlan parity。
- alternate/reduced/armor的固定hold与rest公式未改；type3 hold release与active-holder mirror仍保持原后置顺序。

## 回滚

回滚两个适配helper、十个调用替换（4 actual + 6 projection）和新增测试；保留data carrier与pure resolver即可恢复改前行为。
