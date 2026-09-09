# Task Contract — NTSD28-B5-KIND4-ATOMIC-PRODUCTION-INTEGRATION-001

> 状态：`VERIFIED / KIND4_ENVIRONMENT_CONSUMPTION_ALIGNED`
> 依赖：`NTSD28-B5-KIND4-SOURCE-COUNT-CARRIER-001 / VERIFIED`

## 目标

原子闭合kind4 environment chain：每个几何有效kind4 candidate在攻击者 `EnvironmentState320 > 0` 时按16位
递增 `Kind4SourceCount92`；actual与HitPlan仅以该environment gate把kind4转kind0并按速度/朝向反转dvx；
普通伤害在计数非零时从 `CatchSourceSlot90` 低16位开始走最多两层owner归属，并在成功unarmored/reduced结算后递减一次。

## 允许路径

- `Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterHitResolver.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（仅修正本包暴露的旧 `WeaponCount` kind4断言）
- 新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28B5Kind4AtomicProductionIntegrationEditorTests.cs` 与 `.meta`
- 必要时只更新已有HitPlan/候选fixture的旧字段断言
- 本Task/Change、Ledger、STATE、handoff、总表与kind4 manifest

## 必须覆盖的测试先行矩阵

- candidate：positive environment每重叠BDY递增、0/negative不递增、`0xFFFF+1 -> 0`、即使后续low-fall reject仍已递增。
- runtime ITR：positive environment转换；zero/negative不转换；`WeaponCount`正负均不能影响；dvx方向翻转；heavy-held gate同源。
- HitPlan：与actual读取相同environment/count/catch-source并保持DifferenceMask为0。
- attribution：count非零时以 `(ushort)CatchSourceSlot90` 起点走最多两层owner；missing source fail-closed；count零从physical attacker起步。
- consume：type0普通、type1/2/3/4/5普通和reduced成功路径各递减一次；type6/unsupported/未执行伤害不递减；低16位为0不减。

## 不变量与验收

- 不改 `EnvironmentState320` producer、content、Scene、candidate容量或damage数值；不复用/双写 `WeaponCount`。
- candidate increment发生在几何/early eligibility后但low-fall与selection前；damage decrement只在正式成功结算尾部。
- 编译、focused、RoleAware/HitPlan、B5、Unity侧NTSD28自动回归、SelfCheck、Console、Scene与Ledger全部通过后才可VERIFIED。
