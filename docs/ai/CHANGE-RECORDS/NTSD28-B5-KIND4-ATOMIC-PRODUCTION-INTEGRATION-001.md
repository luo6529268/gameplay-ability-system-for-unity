# NTSD28-B5-KIND4-ATOMIC-PRODUCTION-INTEGRATION-001

<!-- CHANGE-RECORD
id: NTSD28-B5-KIND4-ATOMIC-PRODUCTION-INTEGRATION-001
status: VERIFIED
change-kind: TEST_FIRST_ATOMIC_PRODUCTION
code-path: Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterHitResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Kind4AtomicProductionIntegrationEditorTests.cs
authority: NTSD 2.8-Logan kind4 environment_state_320, kind4_source_count_92, catch_source_slot_90 and ordinary-damage chain; EXE B1E13AE1, closure 39DDDA15.
evidence: valid focused RED 2648c7546b294331a80e8fe0415e0ab2 = 10 completed / 8 expected failures / 2 passes; final focused 90767518358741af80413f4dcbce790d 30/30; HitPlan 184/184; RoleAwareCollision 67/67; final B5 f401315d553e466595dbba761cdb7711 504/504; final Unity-side NTSD28 automatic regression 95d3bacc68f64edcbbdb0ec3dd6470a3 1062/1062; SelfCheck PASS 2026-09-06T07:47:21Z; Console 7 intentional negative-path errors; Scene D4266C6D...583B unchanged; B6 producer excluded.
-->

> 状态：`VERIFIED / KIND4_ENVIRONMENT_CONSUMPTION_ALIGNED`

## 已完成的测试先行证据

- 首次运行暴露了source-method定位夹具错误；修正为真实的 `ProjectRuntimeItr(...)` 后重新执行。
- 有效RED：job `2648c7546b294331a80e8fe0415e0ab2`，10项完成，其中8项按预期失败、2项通过；失败分别覆盖candidate递增/16位wrap/low-fall前序，以及actual与HitPlan仍错误读取 `WeaponCount`。
- 已写第一切片：candidate逐BDY递增；actual与HitPlan改读 `EnvironmentState320`；heavy-held actual gate同源。尚未把本包写成完成，归因、成功伤害后的计数消耗与HitPlan效果快照仍待实现。

## 当前验证状态

- 生产链、归因/计数消耗和HitPlan效果快照已写；focused 26/26、HitPlan 184/184、RoleAwareCollision 67/67、B5 500/500、Unity侧 `NTSD28` 自动回归1058/1058通过。
- SelfCheck首次运行失败于 `RunEffect21RuntimeKind4PromotionAbortCase` 的旧 `WeaponCount` 断言；该断言与当前权威 `environment_state_320` 冲突，现将 `BattleRuntimeSelfCheck.cs` 纳入同一原子修正后重跑。该失败不记为生产行为回退，也不能在修正前把本包写成完成。
- SelfCheck修正后继续命中 `LF2CharacterHitResolver` / `LF2CharacterDatHitResolver` 的直接命中fallback：两者仍读取 `WeaponCount`。它们属于实际生产入口而不是单纯夹具，已扩大本包精确路径并要求direct-entry测试后再重跑全部门禁。

## 实际修改

- candidate生产在几何与early eligibility通过后、low-fall/select之前，按每个重叠BDY对 `Kind4SourceCount92` 做16位递增。
- `BruteForceSceneQuery`、HitPlan和两个direct character-hit fallback统一只读 `EnvironmentState320 > 0`；`WeaponCount`不再参与kind4转换或heavy-held gate。
- 普通伤害在计数低16位非零时，以 `(ushort)CatchSourceSlot90` 为起点走最多两层 `OwnerSlotIndex`，missing source fail-closed；type0且 `OrdinaryCreditGate2F4 == -1` 时把effective HP damage写给最终credit。
- unarmored/reduced与type1～5成功伤害各消耗一次pending count；type6、unsupported及matched-projectile early branch不消耗。
- HitPlan writer snapshot新增attacker count及选定credit的handle/score投影；保留既有三参数反射入口，actual/shadow `DifferenceMask == 0`。
- SelfCheck中两条旧 `WeaponCount` 断言更新为当前 `EnvironmentState320` 权威，并增加candidate count/abort不消耗检查。

## 最终验证

- focused：job `90767518358741af80413f4dcbce790d`，30/30。
- HitPlan：184/184；RoleAwareCollision：67/67。
- B5：job `f401315d553e466595dbba761cdb7711`，504/504。
- Unity侧 `NTSD28` 自动回归：job `95d3bacc68f64edcbbdb0ec3dd6470a3`，1062/1062。此结果只表示当前自动覆盖未回退，不表示整个NTSD 2.8已对齐。
- `BattleRuntimeSelfCheck`：2026-09-06T07:47:21Z `PASS`；Console仅7条既有故意触发的rest-binding拒绝日志。
- Scene：SHA-256 `D4266C6D0A802975B50E481E1D01662DC79AEF168C6E2DE14AE382396FDA583B`、length 205625、mtime `2026-09-05T15:44:52.7794120Z`，与基线一致。

回滚必须整体移除candidate increment、actual/HitPlan environment conversion、attribution/decrement和本包测试；carrier保留为已验证前置，不恢复WeaponCount误用。
