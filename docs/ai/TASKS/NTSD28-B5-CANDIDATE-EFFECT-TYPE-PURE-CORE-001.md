# Task Contract — NTSD28-B5-CANDIDATE-EFFECT-TYPE-PURE-CORE-001

> 状态：`VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`
> 依赖：`NTSD28-B5-REMAINING-EXIT-AUDIT-002 / VERIFIED`

## 目标

建立allocation-free pure resolver，精确复刻candidate-time effect/target-type gate：13→type0、14→type3、
15→type0/3、16→type1/2/3/4/6，其他effect unrestricted。

## 允许路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleHitCandidateEffectTypeResolver.cs` 与 `.meta`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5CandidateEffectTypePureCoreEditorTests.cs` 与 `.meta`
- 本 Task/Change、Ledger、STATE、handoff、总表

## 不变量

- 本包不接candidate/runtime production，不改effect-action override matcher。
- effect16必须包含type3且排除type0/type5；其他effect不限制，包括未知值。
- 不修改content、Scene或权威目录。

## 验收

test-first红灯、完整矩阵、4096次zero-allocation；随后compile、focused、B5、NTSD28 broad、SelfCheck、
Console、Scene与Ledger。

## 完成证据

red `08b53ad3074e40888113bc7dd5a66d21` 10项预期失败/26；focused
`0fa494c910924192b380ac65cf049678` 26/26、B5
`3a1b8ed4169a4540a42c8938c5a33863` 446/446、NTSD28 broad
`0f662b4bd4b4479aac5e86f80fec39e3` 911/911通过；04:53:26Z SelfCheck PASS，Console仅7条
预期负路径日志，Scene `D4266C6D...583B` unchanged，Ledger PASS。production未接。
