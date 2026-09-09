# Task Contract — NTSD28-B5-MULTI-BODY-CANDIDATE-PRODUCTION-001

> 状态：`VERIFIED / MULTI_BODY_CANDIDATE_MULTIPLICITY_ALIGNED`
> 依赖：`NTSD28-B5-MULTI-BODY-CANDIDATE-OWNER-AUDIT-001 / VERIFIED`

## 目标

使C11正式候选生成按目标BDY源顺序为每一个重叠BDY独立调用现有candidate selection/store owner，
让brute、loose与role-aware exact/fallback在candidate数量、bodyX顺序、20容量和nearest RNG次数上与Authority一致。

## 允许路径

- `Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5MultiBodyCandidateProductionEditorTests.cs` 与 `.meta`
- `Assets/NTSD/Scripts/Test/Editor/RoleAwareCollisionShadowSelfCheckTests.cs`（仅更新由全BDY扫描必然增加的精确overlap诊断计数）
- 本Task/Change、Ledger、STATE、handoff、总表与既有multi-body manifest

## 不变量

- 每个重叠BDY仍独立进入唯一 `TryRecordReleaseCandidate`；不复制selection、容量、RNG或store写逻辑。
- BDY顺序必须等于 `collisionFrame.bodies` / ordered exact body rect cache顺序。
- direct/immediate `QueryBodyHits`接口保持每target一个结果，不修改consumer、HitPlan、content或Scene。
- 不引入每tickmanaged allocation；existing role-aware cache ownership不变。

## 验收

test-first覆盖三种collector的两BDY顺序、brute/role-aware 20容量、nearest第二BDY同步RNG及direct query兼容；
随后compile、focused、RoleAware/HitPlan、B5、NTSD28 broad、SelfCheck、Console、Scene、diff与Ledger。

## 完成证据

无效fixture compile run `d2e7ce8eb01d40f4a7636ac671f83789` 不计behavior red；有效red
`c13d2e3c86454c48bf2be0063e4d622b` 为7项预期失败/8且direct protection通过。green focused
`04c83ea644284613801c9639bff3d228` 8/8；第一次RoleAware回归 `53f8def4ba3e44a98e182e7cacd85833`
为66/67，只暴露全BDY扫描后过时的精确检查计数2→3，修正后
`3062f003f8f54b3ebb150b0596521085` 67/67；HitPlan
`201046c73c634f9fa623189e8e1d7f7e` 184/184、B5
`d911f1c054134f16a41aa2dffc19a5af` 469/469、NTSD28 broad
`ddcc0fc6157d44f78bacd590e4f1a295` 934/934。05:51:00Z SelfCheck PASS，Console仅7条预期负路径，
Scene `D4266C6D...583B` unchanged，Ledger 313 Records / 272 governed files PASS。
