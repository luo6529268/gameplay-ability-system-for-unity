# Task Contract — NTSD28-B5-REMAINING-EXIT-AUDIT-002

> 状态：`VERIFIED / GOVERNANCE_ONLY / CANDIDATE_EFFECT_TYPE_FILTER_ROUTED / B5_EXIT_NOT_READY`
> 依赖：`NTSD28-B5-KIND8-EXIT-AUDIT-001 / VERIFIED`

## 下一首差

Authority `HitCandidateBuilder28::interaction_effect_accepts_object_type(...)`在几何前对所有ITR的effect执行：

- 13仅type0；14仅type3；15仅type0或3；16仅type1/2/3/4/6；其他effect不限制。

Unity `BruteForceSceneQuery.ItrAllowedCore`没有candidate-time对应判断；现有
`BattleDamageWriter.NativeEffectActionTargetTypeMatches`属于更晚的effect-action override，覆盖8..16且effect16
不含type3，语义不同，不能复用。下一先建立独立candidate effect/type pure core，再接candidate与runtime defensive gate。

## 边界

本审计不改代码/content/Scene；不把effect-action override矩阵改写成candidate矩阵，不处理kind/effect消费副作用。

