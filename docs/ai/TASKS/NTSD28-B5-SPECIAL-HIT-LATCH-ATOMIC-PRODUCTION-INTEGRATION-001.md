# Task Contract — NTSD28-B5-SPECIAL-HIT-LATCH-ATOMIC-PRODUCTION-INTEGRATION-001

> 状态：`VERIFIED / SPECIAL_HIT_LATCH_PRODUCTION_ALIGNED / LEGACY_WEAPON_CONFIRM_PRESERVED`
> 依赖：`NTSD28-B5-SPECIAL-HIT-LATCH-OWNER-AUDIT-001 / VERIFIED`

## 目标

将已验证carrier原子接入current-authority四条type3 producer、shared pre-writer consumer和HitPlan shadow，
消除旧`HitConfirm2`身份/生命周期别名，同时保持普通weapon临时`HitConfirm2`语义不变。

## 允许路径

- `Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`
- 新增`Assets/NTSD/Scripts/Test/Editor/NTSD28B5SpecialHitLatchAtomicProductionIntegrationEditorTests.cs`与`.meta`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type3KindCatalogTransformEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type3TargetGenericContinuationEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- `Assets/NTSD/Scripts/Test/Editor/BattleCollisionHitDamagePlayModeProbeEditor.cs`
- 本Task/Change、Ledger、STATE、handoff、总表

## 不变量

- consumer gate仍位于runtime ITR替换与任何writer之前，返回whole-attacker abort；predicate改为新bool且只对target type0。
- kind9 state3005/generic、kind0 locked/generic四actual producer只写新bool，不再写旧`HitConfirm2`。
- HitPlan新增bool初值/五投影/difference；actual/shadow mask必须为0。
- 普通weapon/object的`HitConfirm2` writer、HitPlan投影、C25/C11 clear及测试必须保持。
- new latch必须跨C25/C11和tick保存，full reset/reuse清零；旧`HitConfirm2`非零不得触发current-authority gate。
- 不改schema、raw、内容、Scene、正式Authority、B6/B7/B10/H行为。

## 验收

- test-first red精确命中旧consumer/producer/HitPlan身份。
- focused atomic/type3/HitPlan/B5/NTSD28通过，SelfCheck PASS。
- 运行现有collision-hit Play probe，确认type0 whole-attacker abort与weapon临时carrier均符合各自合同。
- compile0、最终Console0、Scene unchanged/dirtyfalse、diff-check与Ledger PASS。

## 回滚

只恢复三处production行为与本包测试/文档；保留已验证`SpecialHitLatch0EB` carrier/schema/raw基础设施。

## 验证结论

- test-first job `03321ed9a5a446bfb1379f9bf1b99171`：18项执行、4项按预期失败，均为type3 actual仍把旧`HitConfirm2`从sentinel 7覆盖成1；新source-guard脚本随后显式import。
- atomic focused `8c11a02aa761419fae69e7f2d30e19d3`：4/4；type3 related `4d50a51038bc4f919d799c40f194e1c4`：18/18。
- final Editor csproj build：exit 0，0 error（129条既有warning）。
- HitPlan首次整类回归识别3个遗漏旧断言；按已冻结generic type3 producer边界更正后，job `523eb0f3f54149d0a7c81d65744892e0`：184/184，shadow difference mask为0。
- B5全组job `8339748f212749ffac6a615aa437519a`：524/524；Unity侧完整NTSD28组job `f8dbbfb723244801be65a3a8b4a3881c`：1082/1082。
- `BattleRuntimeSelfCheck`首次捕获type3旧carrier断言；更正type3/weapon分流断言后于2026-09-06 18:58（+08:00）复跑`PASS`。
- collision-hit Play探针最终`PASS`：10 candidates；weapon `HitConfirm2=1/latch=false`，type3 special `HitConfirm2=0/latch=true`，latch attacker对两个type0目标执行whole-attacker abort；基线对象/slot/pool、stats/RNG/sounds/rest/HitPlan mode全部恢复。
- Play期间还更正探针的历史`FallDamageDiv`缩放夹具为当前`IncomingDamageScale340=50`，并保留冲突旧值200，以证明当前authority伤害来源；weapon结果HP80/HPBound94/combo20、raw durability90。
- 最终Unity Console 0 error；`NTSD_Battle`已退出Play、dirty=false、root13，Scene SHA-256保持`50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`。
- Change Ledger validator：PASS，329 records / 284 governed code files。
