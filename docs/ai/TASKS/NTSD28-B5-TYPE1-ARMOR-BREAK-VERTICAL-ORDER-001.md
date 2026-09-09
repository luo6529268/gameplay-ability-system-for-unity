# Task Contract — NTSD28-B5-TYPE1-ARMOR-BREAK-VERTICAL-ORDER-001

> 状态：`VERIFIED / BREAK_VERTICAL_POSTHIT_ORDER_ALIGNED`
> 依赖：`NTSD28-B5-TYPE1-ARMOR-EXIT-AUDIT-001 / VERIFIED`

## 目标

仅对broken type1 armor unarmored fallback，把actual与HitPlan mutation顺序固定为：
horizontal→armor action/`-1→0`→vertical→attacker post-hit。

## 允许路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type1ArmorBreakVerticalOrderEditorTests.cs` 与 `.meta`
- 本Task/Change、Ledger、STATE、handoff、总表与armor manifest

## 不变量

- 非broken standard hit的最终行为不变。
- broken fallback只有在vertical最终确定为knockdown时才延后既有vertical mutation，且只提交一次。
- HitPlan顺序与actual一致；不改变selection/damage/resource/audio/spark/content。
- 不修改Scene。

## 验收

test-first捕获broken knockdown的intermediate order contract及最终actual/projection一致；随后compile、focused、B5、
HitPlan、NTSD28 broad、SelfCheck、Console、Scene与Ledger。

## 完成证据

有效red `5e7d050684b84dc8848ba9d74a8334de` 2/3预期失败；focused3、HitPlan184、B5 356、
broad821、SelfCheck/Console/Scene/diff/Ledger全部通过。下一exit audit 002。
