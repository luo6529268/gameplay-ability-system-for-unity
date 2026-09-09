# Task Contract — NTSD28-B5-MULTI-BODY-CANDIDATE-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / PRODUCTION_SEAMS_FROZEN`
> 依赖：`NTSD28-B5-REMAINING-EXIT-AUDIT-003 / VERIFIED`

## 所有权结论

- `ForceBruteForce` 与loose collector最终均调用 `CollectCandidatesForPair`。
- role-aware exact路径调用 `CollectCandidatesForPairCached`；fallback仍回到 `CollectCandidatesForPair`。
- exact body rect cache按 `collisionFrame.bodies` 原顺序构建，能够保持Authority的BDY顺序。
- 每个重叠BDY必须独立调用现有 `TryRecordReleaseCandidate`；该方法已经是direct/group后续选择、20容量、
  nearest tie RNG与store/list双写的单一owner，不应复制或改写。
- immediate/direct `QueryBodyHits` 仍是“每target返回一个命中”的查询接口，不属于C11 production candidate multiplicity，
  本轮保持兼容，不把它们扩大为重复target结果。

## 实施边界

下一包只修改 `BruteForceSceneQuery.cs` 的两条production overlap枚举和新增独立Editor测试；不修改candidate
store结构、consumer、HitPlan、content或Scene。brute/loose/role-aware必须在BDY顺序、计数、20容量和RNG call count上相等。
