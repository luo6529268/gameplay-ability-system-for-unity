# Task Contract — NTSD28-B5-REMAINING-EXIT-AUDIT-003

> 状态：`VERIFIED / GOVERNANCE_ONLY / MULTI_BODY_CANDIDATE_MULTIPLICITY_ROUTED / B5_EXIT_NOT_READY`
> 依赖：`NTSD28-B5-CANDIDATE-EFFECT-TYPE-EXIT-AUDIT-001 / VERIFIED`

## 下一首差

Authority对每个ITR按目标frame中的BDY顺序逐个检测，并为每一个重叠BDY产生独立几何candidate；随后每个candidate
独立进入group/direct filter、multiple/nearest选择、20容量和可能的0x85/0x86同步RNG路径。

Unity正式两条collector分别由 `HitsTarget(...)` 与 `HitsTargetCached(...)` 找到第一个重叠BDY后立即返回，
每个ITR/target至多记录一个candidate。因而多BDY同时重叠时，candidate数量、bodyX、20容量占用、nearest tie RNG
调用次数及后续消费顺序均可能不同。

## 下一步

先做 `NTSD28-B5-MULTI-BODY-CANDIDATE-OWNER-AUDIT-001`，冻结brute/role-aware两条正式collector、缓存body rect、
candidate store及immediate/direct兼容查询边界，再设计单一按BDY顺序枚举接口；不得只修改一个 `return true`。

本审计不改代码、content或Scene。证据见
`docs/ai/MANIFESTS/NTSD28-B5-MULTI-BODY-CANDIDATE-MULTIPLICITY.md`。
