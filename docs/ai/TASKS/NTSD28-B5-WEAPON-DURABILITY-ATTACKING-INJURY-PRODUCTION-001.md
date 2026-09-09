# Task Contract — NTSD28-B5-WEAPON-DURABILITY-ATTACKING-INJURY-PRODUCTION-001

> 状态：`VERIFIED / WEAPON_DURABILITY_ATTACKING_INJURY_ALIGNED`
> 依赖：`NTSD28-B5-WEAPON-DURABILITY-ATTACKING-INJURY-OWNER-AUDIT-001 / VERIFIED`

## 目标

让type1/2/4/6的actual与HitPlan weapon durability统一扣除已验证的native attacking injury，而不是raw ITR injury；保持raw bdefend=100强制破坏及HP/display/status独立语义。

## 允许路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`
- 新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28B5WeaponDurabilityAttackingInjuryProductionEditorTests.cs` 与 `.meta`
- 必要时更新已有HitPlan/weapon durability精确断言
- 本Task/Change、Ledger、STATE、handoff、总表与manifest

## 测试矩阵与不变量

- type1/2/4/6；positive definition优先、definition非正时positive mode、两者非正raw fallback、raw0。
- 低32位乘法/signed `/100`沿用既有pure resolver；不得复制算法。
- `bdefend==100`最终强制 `-1`；HP damage仍不受attacking multiplier影响。
- HitPlan shadow `DifferenceMask==0`；编译、focused、HitPlan、B5、Unity侧 `NTSD28` 自动回归、SelfCheck、Console、Scene与Ledger全通过才可VERIFIED。
