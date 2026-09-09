# Task Contract — NTSD28-B5-KIND8-ATOMIC-PRODUCTION-INTEGRATION-001

> 状态：`VERIFIED / KIND8_CONTROL_RELATION_ALIGNED`
> 依赖：`NTSD28-B5-KIND8-PRODUCTION-OWNER-AUDIT-001 / VERIFIED`

## 目标

同包接入kind8 candidate eligibility、共享actual writer与HitPlan defensive projection，完整还原selector、
conditional heal/MP/action与dvy precise-coordinate transaction。

## 允许路径

- `Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleKind8ControlRelationWriter.cs` 与 `.meta`
- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5Kind8AtomicProductionIntegrationEditorTests.cs` 与 `.meta`
- `Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs`（仅修正kind8旧整数坐标断言）
- 本 Task/Change、Ledger、STATE、handoff、总表与kind8 manifest

## 不变量

- candidate与consumer都用同一pure eligibility；拒绝必须零mutation。
- injury/caughtact/dvx分别只在非0/非0/非999时写；action写不重置wait counter。
- dvy >2或<-1归一到0；-1不坐标同步，0=X/Z，1=Y/Z，2=X/Y/Z；只写precise double，int不写。
- shared runner保留现有preprocess、consume-effect observation、BeforeDispatch与dispatch observation结构，只替换kind8实际writer。
- 旧四壳direct kind8路径保留为compatibility-only；不改其他kind、resource/content/audio/spark/Scene。

## 验收

test-first覆盖candidate nonchar/type/relation/mode、actual rejection和完整side-effect矩阵、HitPlan等价及shared-runner
production dispatch；随后compile、focused、HitPlan、collision相关、B5、NTSD28 broad、SelfCheck、Console、Scene、diff与Ledger。

## 完成证据

有效red `db2f1ce104564c0a8cd6c28d0db55821` 14/14失败；focused
`0e3093144a2849af846fa06e1957a9d2` 14/14、HitPlan
`ece0b0bad3e04890bc0304be2073539a` 184/184、collision related
`5adea7dd6bb240018facab4d6c418872` 258/258、B5
`bc78b6630b0442ff952b58b78bcd801a` 420/420通过。首次broad被运行中MCP轮询日志污染；清噪且
不轮询重跑 `14cc72613acb477c9c48e09f80e33b86` 885/885通过。04:33:16Z SelfCheck PASS，
Console仅7条预期负路径日志，Scene `D4266C6D...583B` unchanged，Ledger PASS。
