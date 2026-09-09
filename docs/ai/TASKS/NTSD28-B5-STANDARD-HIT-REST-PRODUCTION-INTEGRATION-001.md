# Task Contract — NTSD28-B5-STANDARD-HIT-REST-PRODUCTION-INTEGRATION-001

> 状态：`VERIFIED / PRODUCTION_CONNECTED / STANDARD_REST_ALIGNED`
> 依赖：`NTSD28-B5-STANDARD-HIT-REST-PURE-RESOLVER-001 / VERIFIED`

## 目标

把已验证的`BattleStandardHitRestResolver`统一接入`BattleDamageWriter`与
`BattleEcsHitExecutionPlan`，覆盖character、weapon、special/other、initial matching pair和type3
transform projection，使actual与shadow prediction按同一recover/definition-effect/reduction/uint8规则写入。

## 允许路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5StandardHitRestProductionIntegrationEditorTests.cs`
- 对应Unity `.meta`
- `Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- 本Task/Change、Ledger、STATE、handoff、总表与rest manifest

## 不变量

- 只接standard unarmored rest；不得改变alternate/reduced/armor rest公式或分支顺序。
- rest仍位于正式damage/reaction后的authority位置；initial matching pair仍在damage/audio/status/hit-record前early return。
- `ApplyActiveHolderFrameDelay`与type3 motion-hold release保持既有后置顺序。
- actual与HitPlan必须调用同一pure resolver；不得保留另一套等价公式。
- 不实现原生selection UI，不修改content、Scene、资源、输入、cadence或RNG。

## 验收与回滚

test-first覆盖四类actual owner、initial early、recover/definition effects/nondefault reduction/native byte vrest，并以
ShadowCompare证明HitPlan零差；再运行compile、focused、B5、HitPlan、exact broad、SelfCheck、Console、Scene、Ledger。

回滚本Change对两个生产文件、测试与SelfCheck的精确diff；保留carrier与pure resolver。

## 验收结果

- test-first red：job `dd93e4ba1c674c698588fd0590b4b71d`，7/7按预期失败。
- focused green：job `367f42a0f9204f92a796ef28460220be`，7/7通过。
- B5 + HitPlan：job `b723eefcd0ed4da89377399c8976d612`，411/411通过。
- NTSD28 broad：job `6012829e7fb24534b595488788293884`，692/692通过。
- 编译：Runtime DLL `2026-09-05T21:56:50Z`、Editor DLL `2026-09-05T21:52:45Z`；filtered Console仅7条既有rest-binding自检日志，无CS诊断。
- `BattleRuntimeSelfCheck`：`2026-09-05T22:03:59Z` PASS。
- Scene：SHA-256 `D4266C6D0A802975B50E481E1D01662DC79AEF168C6E2DE14AE382396FDA583B`、长度205625、mtime `2026-09-05T15:44:52.7794120Z`不变。
- Change Ledger validator：PASS（279 records / 240 governed code files）。

本包未修改alternate/reduced/armor、selection UI、content或Scene；下一步做standard-rest family exit audit。
