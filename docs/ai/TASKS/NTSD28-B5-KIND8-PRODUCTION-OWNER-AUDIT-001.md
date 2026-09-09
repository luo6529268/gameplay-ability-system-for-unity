# Task Contract — NTSD28-B5-KIND8-PRODUCTION-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / ATOMIC_INTEGRATION_READY`
> 依赖：`NTSD28-B5-KIND8-ELIGIBILITY-PURE-CORE-001 / VERIFIED`

## 审计结论

kind8所需字段写面均已存在，无需新增runtime/snapshot carrier。下一包可原子接线，但必须同时修改三层：

1. `BruteForceSceneQuery.ItrAllowedCore`：移除kind8的type0硬限制，使用pure resolver执行candidate gate。
2. `BattleHitCandidateSequenceRunner`：在共享四壳消费点对kind8走唯一actual writer，跳过旧分散dispatch。
3. `BattleEcsHitExecutionPlan.ProjectWriterEffect`：同一resolver防御复核并投影完整条件副作用。

## 字段与时序

- group=`RelationTeam`；owner=`Runtime.OwnerSlotIndex`；mode=`SimulationWorld.BattleGameModeId`。
- current MP=`Health.PP`；heal=`HealTimer`；action=`Frame.N`与`Runtime.Frame`。
- precise coordinate=`Runtime.X/Y/Z`；`XInt/YInt/ZInt`本事务保持不变，留后续physics同步。
- shared runner在disposition后、generic consume-effects前分流kind8；成功/拒绝都继续下一个candidate，不触发普通伤害rest或全局abort。
- HitPlan现有writer snapshot已覆盖上述全部字段，DifferenceMask无需扩schema。

## 边界

旧character/shared/special/weapon kind8 direct路径不删除，避免扩大兼容面；production由shared runner唯一接管。
不改kind1/3 catch、resource/content/audio/spark或Scene。

