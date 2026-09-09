# Task Contract — NTSD28-B5-CANDIDATE-EFFECT-TYPE-PRODUCTION-FILTER-001

> 状态：`VERIFIED / CANDIDATE_AND_CONSUMER_FILTER_ALIGNED`
> 依赖：`NTSD28-B5-CANDIDATE-EFFECT-TYPE-PURE-CORE-001 / VERIFIED`

## 目标

把独立effect/type resolver接入`ItrAllowedCore`，使所有candidate collector与现有runtime defensive查询共享
同一过滤；并在shared candidate runner解析runtime ITR后再次防御性过滤，匹配Authority的二次调用。

## 允许路径

- `Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5CandidateEffectTypeProductionFilterEditorTests.cs` 与 `.meta`
- 本 Task/Change、Ledger、STATE、handoff、总表

## 不变量

- filter位于writer/disposition mutation前；拒绝不消费candidate副作用。
- 只过滤effect13..16；其他effect、kind-specific与晚阶段action override不变。
- brute/loose/role-aware均继续汇聚同一core，不复制矩阵。
- 不修改HitPlan schema、content或Scene。

## 验收

test-first覆盖13..16接受/拒绝、其他effect不限制及shared runner defensive gate顺序；随后compile、focused、
collision/HitPlan、B5、NTSD28 broad、SelfCheck、Console、Scene、diff与Ledger。

## 完成证据

有效red `e8767d08a7074e5c9d81b381d5bd5e75` 为6项预期失败/15；green focused
`70bdf534fd6341e5b43c298aaabedbb0` 15/15、HitPlan
`d1149a8995794c27b023d375ae870953` 184/184、B5
`3f3a0c18eef64e6487035f1804ac0f49` 461/461、NTSD28 broad
`de83a62ac2aa4bafbe878785fae554b4` 926/926通过；05:13:45Z SelfCheck PASS，Console仅7条预期负路径日志，
Scene `D4266C6D...583B` unchanged，Ledger 309 Records / 270 governed files PASS。
