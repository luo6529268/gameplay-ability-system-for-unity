# Task Contract — NTSD28-B5-HIT-GROUP-ELIGIBILITY-ATOMIC-PRODUCTION-INTEGRATION-001

> 状态：`VERIFIED_CORE / LINKED_HOLDER_BINDING_CORRECTION_PENDING / FAMILY_EXIT_REOPENED_BY_NTSD28-B6-LEGACY-HOLDERCOPY-MULTIPLEXED-SLOT-OWNER-AUDIT-001`
> 依赖：world mode carrier、frozen pair carrier、pure resolver均`VERIFIED`

## 目标

把Authority hit-group eligibility一次性接入正式candidate producer与共享consumer：正式collector在几何重叠后冻结完整pair，先于nearest/capacity/RNG调用同一resolver；consumer只读取valid冻结值做防御复核，invalid兼容路径统一通过live factory采样。

## 边界

- 移除formal collector在`ItrAllowedCore`中的旧`RunsKindGroupFilters`提前门，避免state190/180、kind bypass、type/facing被错误拒绝。
- brute、loose、role-aware exact/fallback必须继续汇合`TryRecordReleaseCandidate`；rejected pair不得污染nearest、20-slot容量或tie RNG。
- cached direct consume、shared runner及无snapshot direct helpers必须复用同一resolver；kind5 holder特殊consumer gate必须使用frozen holder/group字段。
- HitPlan只观察/carry已筛选snapshot身份，不复制truth table。
- 不改background content producer、damage算术、B6/B7/B8/B9/B10/H、Scene、Prefab或persistent schema。

## 验收

- test-first覆盖三collector、snapshot字段、nearest/tie RNG、capacity、低slot先命中后live group/action/facing改变、cached/shared consumer、kind5 holder与HitPlan observation。
- focused、RoleAware、HitPlan、B5、Unity侧NTSD28、build、SelfCheck、真实collision-hit Play probe、Console、Scene/hash/root及Ledger通过。

## 回滚

逆向移除factory/test并恢复query/runner接线；carrier、pure resolver与world field保留为前置已验证包。

## 实施与验证结果

- formal collector不再提前运行旧live group/kind5门；三collector在每个重叠BDY汇合`TryRecordReleaseCandidate`后冻结完整pair，并在nearest/capacity/tie RNG前调用唯一resolver。
- cached/direct adapter与shared runner统一复用snapshot；valid candidate不重读group/action/state/type/facing；kind5 substituted character gate使用冻结holder/group/action。
- RED：首次8个缺失factory/API/helper编译错误；初次green后3个holder断言揭示夹具的slot99无holder哨兵，按实际映射修正；production factory显式world查找保留。
- focused：25/25 PASS（job `a2ce82028e4441a38d3b5f56d5ff8625`）；RoleAware/store/HitPlan组合255/255 PASS（job `5983375df6b8472cb42fcce3db307950`）。
- broad：B5 610/610 PASS（job `05391eee9d0941c0ad23d70e073fb156`）；Unity侧NTSD28 1168/1168 PASS（job `eebe9af1dd1645b0b2413f864b85cd2d`）。
- build：0 error、129 warning。
- Play：`Temp/NTSD_R8_WP01C_04_CollisionHitDamage.result.json`于22:25:48写出PASS；10 candidates，damage/stat/durability/vrest/abort及cleanup全通过，随后已退出Play。
- SelfCheck：先暴露两个旧口径（kind5单候选假设、state18读Prev2）；分别改为既有multi-body与当前authored-state Authority后，22:38:19最终PASS。
- Console 0 error；`NTSD_Battle` dirty=false/root13，SHA-256仍为`50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`。
- `git diff --check`通过；Ledger 335 records / 292 governed code files PASS。

下一步：独立`NTSD28-B5-HIT-GROUP-ELIGIBILITY-EXIT-AUDIT-001`，确认该规则族无残余首差后继续B5。

## 后继字段绑定纠正（2026-09-08）

`NTSD28-B6-LEGACY-HOLDERCOPY-MULTIPLEXED-SLOT-OWNER-AUDIT-001`确认：本pure truth table、
candidate时点、nearest/capacity/RNG顺序和frozen-consumer结论保留；但
`BattleHitCandidatePairSnapshotFactory`的linked-holder采样错读`HolderCopySlot`，Authority实际读
`linked_parent_slot`（Unity `HolderStableId`）。BruteForce/HitPlan kind5使用同一错误helper，negative-link
helper还存在HolderCopy fallback。因此本包不再支持“整个family aligned”声明；后继由
`NTSD28-B5-KIND5-LINKED-PARENT-SLOT-CORRECTION-001`纠正并重跑原验收矩阵。
