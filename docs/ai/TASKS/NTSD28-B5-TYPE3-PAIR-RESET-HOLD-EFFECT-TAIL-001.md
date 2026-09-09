# Task Contract — NTSD28-B5-TYPE3-PAIR-RESET-HOLD-EFFECT-TAIL-001

> 状态：`VERIFIED / CLOSED`
> 依赖：`NTSD28-B5-TYPE3-POST-HIT-EXIT-AUDIT-001 / VERIFIED`

## 目标

闭合type3 continuation连续尾段：matching state3005/3006 pair按双方action-latch帧hit_Uj选择动作并只清
pending impulse；无论pair是否匹配都执行attacker/negative-parent positive motion-hold取反；退役type3目标上无
2.8权威的legacy effect5000/6000/23 tail。同步HitPlan。

## 允许路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type3PairResetHoldEffectTailEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- 本Task/Change、Ledger、STATE、handoff、总表与type3 manifest

## 不变量

- pair gate仅matching 3005/3005或3006/3006；action从各自当前definition的WaitCounter帧hit_Uj读取，0回退20。
- pair reset只清Knockback XYZ并清AttackingCounter；保留Runtime XYZ、HitCount、identity和history。
- hold release始终在pair reset尝试后；negative link先复制attacker FrameDelay给active parent，再仅正值取负；
  parent缺失则fail-closed，不改attacker。
- 保留公共effect8..16 override；只退役`ApplyType3EffectTail`私有legacy支线，不改type0 direct post-effect/audio。
- 不改attacker/generic/locked-kind已验证事务、真正kind9、content/Scene。

## 验收与回滚

test-first覆盖latched hit_Uj/fallback、pending-only、ordinary与parent无条件hold、nonpositive/missing-parent、
effect5000/6000/23 no-op和HitPlan parity；之后compile、focused、B5/HitPlan、exact broad、SelfCheck、Console、
Scene与Ledger。

回滚恢复旧fixed20/velocity reset、pair内hold和legacy type3 effect helper；不回退前置type3包。

## 完成结果

- test-first job `990bb3b11b804b679656766dad07fc37`：7项中6个预期失败、1个既有
  missing-parent路径通过，证明旧fixed20、未释放hold和legacy effect tail基线。
- 实现后 focused job `a975c05b325d4189b115b316c3dbfc45`：7/7 PASS；HitPlan job
  `8fccff2ed7be4586b0ba456ccf0ecd2b`：183/183 PASS。
- 全部B5+HitPlan job `b4926e1c0a064c40b7ef24da4d4f61cb`：374/374 PASS；102个
  `NTSD28*`类 job `613297726cff432cbb4c99b4ec9b24c4`：749/749 PASS。
- final Unity runtime/editor compile完成；`BattleRuntimeSelfCheck`于`2026-09-05T20:14:00Z`
  PASS。Console仅7条自检故意触发的registration/rest-binding拒绝日志。
- Scene保持SHA-256 `D4266C6D0A802975B50E481E1D01662DC79AEF168C6E2DE14AE382396FDA583B`、
  205625 bytes及原mtime；Change Ledger 272 records / 235 governed code files PASS。
- 本包只证明声明的type3尾段和自动验证范围；下一步重新运行type3 family退出审计，不扩大为B5完成。
