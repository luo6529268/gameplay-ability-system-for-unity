# Task Contract — NTSD28-B5-ORDINARY-DEFENSE-REDUCED-HIT-PRODUCTION-INTEGRATION-001

> 状态：`VERIFIED / PRODUCTION_CONNECTED / NULL_ARMOR_DEFENSE_ALIGNED`
> 依赖：ordinary-defense、reduced-hit damage/rest pure cores 与 ITR defense carriers 均 `VERIFIED`

## 目标

以已验证 pure cores 原子替换 Unity 旧 alternate selection/damage/rest：actual 两个角色命中入口与 HitPlan
统一使用 current-state ordinary defense truth table；无 selected armor 时使用 raw `/10`、HP-only target `+0x340`
与 exact reduced rest。同步退休旧 OID37/6/52/Prev2 自检断言。

## 允许路径

- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5OrdinaryDefenseReducedHitProductionIntegrationEditorTests.cs` 与 `.meta`
- 本 Task/Change、Ledger、STATE、handoff、总表与 armor manifest

## 不变量

- selector 只认 kind0/effect<61/current state7/70/75/live HP/facing/spark/dbdefend/dvx/OID822。
- 无 selected armor reduced damage 为 raw injury `/10` 后仅 target `+0x340`；attacker weak 不参与。
- reduced rest 读取 definition effect 与 world timing reduction；不读取 ITR recover；null armor 等价 delay -1。
- actual 与 HitPlan 必须共用 pure cores；不只修一侧，不接 type1 armor model/content。
- 保留 reduced tail 的既有 weapon reaction、hit record、audio/spark 边界；这些后续按 B5/B9/B10 分项继续审计。
- 不改 Config、Scene、Prefab、Input Actions 或 snapshot schema。

## 验收

test-first 覆盖 current-state selection、旧 OID heuristic 退休、effect/spark/dbdefend/OID822、damage scale/weak exclusion、
definition-effect/timing rest，以及 actual/HitPlan ShadowCompare；随后 compile、focused、B5、HitPlan、NTSD28 broad、
SelfCheck、Console、Scene 与 Ledger。

## 验收结果

- red：job `64c185d8ea764361833501871a06986f`，3/3 按预期失败。
- focused：job `c135e2f2d75e4f3aae70f7d9680b7d10`，3/3 通过。
- B5：job `01820bd8c553461fbbb0447017ec0267`，272/272；HitPlan 最终 job
  `99358936b6df409d8c2ad01b41641446`，184/184。
- NTSD28 broad：job `b40650fe9e164dddb3118b1bc83418a7`，748/748。
- Runtime/Editor DLL `2026-09-05T23:51:38Z / 23:36:23Z`；23:53:08Z SelfCheck PASS；filtered Console
  仅 7 条预期 rest-binding 自检日志，无 CS 诊断。
- Scene SHA/长度/mtime 不变；Ledger validator PASS（286 records / 248 governed code files）。

下一 ordinary-defense/null-armor reduced-hit exit audit；type1 armor 仍受 H/B11 门约束。
