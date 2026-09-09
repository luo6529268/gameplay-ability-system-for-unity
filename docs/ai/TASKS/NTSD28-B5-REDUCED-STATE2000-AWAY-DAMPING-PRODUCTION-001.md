# Task Contract — NTSD28-B5-REDUCED-STATE2000-AWAY-DAMPING-PRODUCTION-001

> 状态：`VERIFIED / REDUCED_STATE2000_AWAY_DAMPING_ALIGNED`
> 依赖：`NTSD28-B5-REMAINING-EXIT-AUDIT-010 / VERIFIED`

## 目标

为reduced hit建立唯一state2000 away-damping判定，actual与HitPlan均使用逻辑double X和Authority精确
方向/等值语义：只有明确位于target左/右且Vx向外或为零时才把Vx/Vz除以2.5。

## 允许路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5ReducedState2000AwayDampingEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs`（仅修正旧state2000 toward夹具预期）
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（仅在需要补broad guard时）
- 本Task、Change Record、Ledger、STATE、handoff与总表

## 不变量与验收

- 非state2000、unarmored path、target response、rest、RNG、type3 action、audio/spark不变。
- attackerX>targetX且Vx>=0，或attackerX<targetX且Vx<=0时damp；相等永不damp；toward永不damp。
- 必须使用`Runtime.X`/projection double X，不得以`XInt`替代。
- 先用actual equal/fractional与HitPlan away/toward RED固定；再做最小共享判定。
- focused、HitPlan、B5、NTSD28、SelfCheck、Console/Scene、Ledger按风险闭合。

## 回滚

撤销共享判定及actual/HitPlan两个调用点和focused fixture；不回退其他B5包。
