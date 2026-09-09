# NTSD28-B5-SPECIAL-HIT-LATCH-ATOMIC-PRODUCTION-INTEGRATION-001

<!-- CHANGE-RECORD
id: NTSD28-B5-SPECIAL-HIT-LATCH-ATOMIC-PRODUCTION-INTEGRATION-001
status: VERIFIED
change-kind: TEST_FIRST_ATOMIC_PRODUCTION_INTEGRATION
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5SpecialHitLatchAtomicProductionIntegrationEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type3KindCatalogTransformEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type3TargetGenericContinuationEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleCollisionHitDamagePlayModeProbeEditor.cs
authority: NTSD 2.8-Logan four special_hit_latch_0eb true writers and two pre-writer type0 consumer gates; EXE B1E13AE1, closure 39DDDA15.
evidence: red4; atomic4/type3-18/HitPlan184/B5-524/NTSD28-1082 green; SelfCheck PASS; collision-hit Play PASS; Console0; Scene unchanged.
-->

> 状态：`VERIFIED / SPECIAL_HIT_LATCH_PRODUCTION_ALIGNED / LEGACY_WEAPON_CONFIRM_PRESERVED`

必须一次性迁移actual、shared consumer与HitPlan；任何只改其中一面均不得交付。普通weapon旧
`HitConfirm2` writers/clears保持。详细矩阵见owner audit manifest。

## 实际修改

- `BattleHitCandidateSequenceRunner.TryConsumeCandidate`在frozen pair/vrest之后、runtime ITR与writer之前读取`attacker.Runtime.SpecialHitLatch0EB`；只对type0 target返回whole-attacker abort。旧`HitConfirm2`不再参与该门。
- `BattleDamageWriter`四条type3 producer只写新latch；`ApplyKind0WeaponVictimTail`三条普通weapon/object `HitConfirm2=1`保持。
- HitPlan新增`TargetSpecialHitLatch0EB`的初值、五条type3 projection及difference mask；standard object仍只有一条`TargetHitConfirm2=1`。
- focused/HitPlan/SelfCheck/Play断言改为显式区分持久special latch与临时weapon confirmation；C25/C11只清旧字段，新latch跨tick保留。
- Play探针历史weapon scale夹具从已废止的`FallDamageDiv`切到current-authority `IncomingDamageScale340`，同时保留冲突旧值作为负向证据。

## 验证证据

- test-first：`03321ed9a5a446bfb1379f9bf1b99171`，18项执行、4项预期red，均精确显示type3 actual把sentinel `HitConfirm2=7`覆盖成1。
- focused：atomic `8c11a02aa761419fae69e7f2d30e19d3` 4/4；type3 related `4d50a51038bc4f919d799c40f194e1c4` 18/18。
- final `dotnet build Assembly-CSharp-Editor.csproj --no-restore -clp:ErrorsOnly`：exit 0，0 error（129条既有warning）。
- HitPlan：首次整类3项仅为遗漏旧预期；更正后`523eb0f3f54149d0a7c81d65744892e0` 184/184，actual/shadow difference mask=0。
- broad：B5 `8339748f212749ffac6a615aa437519a` 524/524；NTSD28 `f8dbbfb723244801be65a3a8b4a3881c` 1082/1082。
- SelfCheck：首次按预期捕获type3旧carrier断言；分流更正后2026-09-06 18:58（+08:00）`PASS`。
- Play：`Temp/NTSD_R8_WP01C_04_CollisionHitDamage.result.json`最终`PASS`；10 candidates，weapon旧confirm与type3/latch/whole-attacker abort三类见证均通过，cleanup完整。
- 最终Console 0 error；`NTSD_Battle`已退出Play、dirty=false、root13，文件SHA-256保持`50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`。
- `Tools/Validate-ChangeLedger.ps1`：PASS，329 records / 284 governed code files。

## 未关闭项

- 本包只关闭special-hit latch规则族；不声明整个B5完成。
- B6/B7/B10/H、正式content与authority目录均未改；下一执行remaining exit audit 007。
