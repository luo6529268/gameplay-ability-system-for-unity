# Task Contract — NTSD28-B5-MULTI-BODY-CANDIDATE-EXIT-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / MULTI_BODY_CANDIDATE_FAMILY_EXIT_READY`
> 依赖：`NTSD28-B5-MULTI-BODY-CANDIDATE-PRODUCTION-001 / VERIFIED`

## 结论

- brute、loose、role-aware exact及fallback均按目标BDY源顺序逐个进入同一 `TryRecordReleaseCandidate`。
- 两个以上重叠BDY、20容量截断、nearest平局RNG及role-aware计数已有focused与既有回归证据。
- direct/immediate query保持单target兼容，没有被production重复候选语义污染。

该规则族无剩余首差，允许退出；不覆盖其他candidate selection、kind consumer、damage或全B5。
