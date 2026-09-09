# Task Contract — NTSD28-B5-HIT-CANDIDATE-PAIR-SNAPSHOT-CARRIER-001

> 状态：`VERIFIED / CARRIER_READY / FORMAL_PRODUCER_UNCONNECTED`
> 依赖：`NTSD28-B5-WORLD-HIT-GROUP-MODE-GATE-CARRIER-001 / VERIFIED`

## 目标

新增与Authority `HitCandidatePairSnapshot28`逐字段对应的readonly value carrier，并在不改变eligibility结果的
前提下，无损贯穿`SceneQueryHit`、candidate store/shadow、cached copy、shared runner和HitPlan observation。

## 修改边界

- 新carrier：valid + 双方OID/type/group/action/current/previous/tick state/facing + linked-holder存在/group。
- 复制边界：`ILF2SceneQuery`、`CollisionCandidateStore`、`BruteForceSceneQuery`、
  `BattleHitCandidateSequenceRunner`、`BattleEcsHitExecutionPlan`。
- tests：专属value/store roundtrip、store shadow与HitPlan snapshot identity。
- formal producer继续写`default/Valid=false`；不接truth table，不改变nearest/RNG/capacity或命中结果。
- ephemeral candidate不进入battle snapshot/checksum schema；不改Scene/content/Prefab/ProjectSettings/Authority。

## 验收

- RED证明carrier/字段不存在；实现后18-payload+valid exact roundtrip、default invalid、value equality、store copy和warm
  zero-allocation通过。
- legacy/store shadow与HitPlan observation能发现pair snapshot不一致；所有cache/rebuild path不丢snapshot。
- RoleAware、HitPlan、B5、完整Unity侧NTSD28、build、SelfCheck、Console、Scene和Ledger通过。

## 回滚

移除value carrier和各复制/比较字段即可；无persistent schema、content或Scene回滚。

## 验证结论

- RED：新test产生2个预期编译错误，证明`BattleHitCandidatePairSnapshot`不存在。
- 专属4/4；HitPlan 185/185；RoleAware前缀92/92；联合筛选198/198。
- B5 533/533（job `9373a12301ef4225a72918c205346cb5`）；完整Unity侧NTSD28 1091/1091
  （job `cf954c6807d94a64b9d7ef52c4436f4d`）。
- final build：0 error、129既有warning；SelfCheck 2026-09-06 21:03:07 +08:00 `PASS`；清空历史预期
  自检日志后Console 0 error。
- Scene `NTSD_Battle` dirty=false/root13/SHA
  `50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`；未进入Play。
- Ledger 333 records / 288 governed code files，PASS。
