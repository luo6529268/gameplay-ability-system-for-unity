# NTSD28-B5-HIT-GROUP-ELIGIBILITY-ATOMIC-PRODUCTION-INTEGRATION-001

<!-- CHANGE-RECORD
id: NTSD28-B5-HIT-GROUP-ELIGIBILITY-ATOMIC-PRODUCTION-INTEGRATION-001
status: VERIFIED
change-kind: CODE_AND_TEST
code-path: Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleHitCandidatePairSnapshotFactory.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleHitGroupEligibilityResolver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5HitGroupEligibilityAtomicProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan BattleWorld28::rebuild_geometric_hit_candidates, candidate_passes_native_group_filter and classify_ordinary_hit_eligibility; EXE B1E13AE1, closure 39DDDA15.
evidence: RED8; focused25 job a2ce82028e4441a38d3b5f56d5ff8625; RoleAware/store/HitPlan255 job 5983375df6b8472cb42fcce3db307950; B5 610 job 05391eee9d0941c0ad23d70e073fb156; NTSD28 1168 job eebe9af1dd1645b0b2413f864b85cd2d; build0; collision-hit Play PASS 10 candidates and cleanup; SelfCheck PASS 22:38:19 after two current-authority expectation corrections; Console0; Scene dirtyfalse/root13/SHA unchanged; Ledger335/292 PASS.
-->

> 状态：`VERIFIED / HIT_GROUP_ELIGIBILITY_PRODUCTION_ALIGNED`

## 改前事实

- formal collector在几何前仍运行旧live group/kind集合，正式candidate的pair snapshot仍为invalid。
- nearest/capacity/RNG之前尚未调用已验证pure resolver；shared/cached consumer仍重读live fields。
- world mode carrier、完整pair carrier与pure resolver已由前三个独立Change验证。

## 不变量

- resolver保持唯一七步truth-table owner；factory只采样，不判断。
- formal producer冻结后先group filter，再nearest/capacity/RNG；valid consumer不重读group/action/state/type/facing。
- 不改变effect/type、kind8、direct effect、special latch、vrest、damage与生命周期既有顺序。
- 不改content/Scene/schema/authority。

## 回滚

逆向移除factory/test及query/runner原子接线；不回退三个已验证前置包。

## 实际修改

- `BattleHitCandidatePairSnapshotFactory`从当前/previous/tick frame、OID/type/group/action/facing及world holder关系无分配采样完整pair；formal producer显式传入所属world。
- `BruteForceSceneQuery`的formal brute/loose/role-aware exact/fallback移除旧`RunsKindGroupFilters`和pre-geometry kind5 holder gate；每个重叠BDY在`TryRecordReleaseCandidate`冻结并调用pure resolver，之后才进入nearest/capacity/RNG。
- ordinary/nearest candidate、store/cached rebuild持续携带同一snapshot；direct/invalid兼容路径通过同一factory+resolver，不保留旧truth table。
- `BattleHitCandidateSequenceRunner`在runtime ITR resolution后用original kind、effective effect、world mode及frozen pair做防御复核；substituted kind5 character再用冻结holder/group/freeze-column action gate。
- `BattleRuntimeSelfCheck`把两条旧结论更正为当前合同：kind5几何可以按multi-body产生多条冻结candidate并在consumer拒绝；state18/180 group例外读取current authored state而不是Prev2。

## Test-first与验证

- RED：8个缺失factory/overload/kind5 helper编译错误。
- 初次production green 17/20；3项只因测试错误预期slot99无holder为present，修正后20/20；扩充mode/effect21/capacity/shared frozen-state后最终25/25。
- RoleAware/store/HitPlan 255/255；B5 610/610；Unity侧NTSD28 1168/1168。
- build 0 error；真实collision-hit Play probe 10 candidates全矩阵PASS且cleanup完整，随后退出Play。
- SelfCheck两次中间FAIL精确暴露旧kind5单候选与Prev2 state18口径；按已确认Authority修正后最终PASS，不是跳过检查。
- Console 0 error；Scene dirty/root/hash不变；diff check及Ledger335/292 PASS。

## 剩余边界与回滚

- background mode content非零producer仍归B8/H；persistent schema未改变。
- 本包不声明整个B5完成；下一独立exit audit。
- 回滚逆向移除factory/test及query/runner接线，并恢复SelfCheck旧断言仅用于历史比较；三个前置carrier/pure包保持。

## 2026-09-08 correction

`NTSD28-B6-LEGACY-HOLDERCOPY-MULTIPLEXED-SLOT-OWNER-AUDIT-001`保留本包的truth table、
candidate时点、RNG/capacity与frozen-consumer证据，但纠正linked-holder binding：
factory/BruteForce/HitPlan当前读legacy `HolderCopySlot`，Authority读`linked_parent_slot`。因此
“production aligned”范围已收窄为core子集，并由`NTSD28-B5-KIND5-LINKED-PARENT-SLOT-CORRECTION-001`
重开字段绑定与运行验收。
