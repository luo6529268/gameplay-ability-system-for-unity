# Task Contract — NTSD28-B6-HORIZONTAL-IMPULSE-FINALIZER-EXIT-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / C22_STANDALONE_EXIT_READY / PRODUCER_AND_STAGE_DEPENDENCIES_ROUTED / JOINT_TRACE_PENDING / NO_NEW_PRODUCTION_PACKAGE`
> 依赖：`NTSD28-B6-CPOINT-KIND2-HURT-ACTION-CONSUMER-OWNER-AUDIT-001 / VERIFIED`

## 目标

重新核对 Authority `finalize_horizontal_hit_impulses()` / `HitResponseResolver28::finalize_horizontal()` 与
Unity `FramePostProcessAll()` / `BattleEcsFramePostProcessPass` 的 gate、三轴公式、count-zero清理、遍历与
stage-removal边界，判断 C22 是否仍有可独立实施差异，或只剩上游 producer/前序 stage 与最终 joint trace。

只读；不修改 C#、Config、Scene、Prefab、资源、ProjectSettings 或 Authority。

## Authority C22 合同

- 位于 catch advance、catch settlement/caughtact、第二次 stage-depth clamp、第二次 held-refill 与
  `settle_ordinary_stage_bounds()`之后；已被 stage settlement移除的实体不进入 finalizer。
- 按 active slot升序扫描；`motion_hold_timer != 0`时完全跳过，保留 pending count/XYZ 与现有 motion。
- hold为0且 `contribution_count > 0` 时，三轴一律写
  `motion.axis = pending.axis * 2 / (contribution_count + 1)`；Y/Z不使用额外consumer gate，零累计会清原motion。
- count≤0 时不改三轴 motion，但仍清 pending XYZ 与 `y_resolved`；负count本身不被强制改0。
- count>0后清 pending XYZ、`y_resolved` 与 contribution count。
- type2 fall≤40等 producer可写pending XYZ但保持count0；finalizer必须丢弃这些pending值并保留旧motion。

## Unity 对应与逐项结果

| Authority | Unity | 结果 |
|---|---|---|
| `motion_hold_timer` | `Runtime.FrameDelay` | B0 binding已验证；`!=0`整实体skip。 |
| `contribution_count` | `Runtime.HitCount` | B5 carrier audit已冻结；正/零/负均有focused覆盖。 |
| pending XYZ | `Runtime.KnockbackVx/Vy/Vz` | 三轴double carrier与actual/shadow均存在。 |
| motion XYZ | `Runtime.Vx/Vy/Vz`（`PS`绑定同runtime） | writer直接写runtime真值。 |
| positive formula | `pending * 2.0 / (count + 1.0)` | 三轴公式与运算顺序一致。 |
| count≤0 | motion保持、pending三轴清零、count负值保留 | legacy/data-oriented一致。 |
| count>0 | motion三轴写入、count/pending清零 | legacy/data-oriented一致。 |
| active slot scan | `SimulationEntityTraversal` / `RuntimeSlotTable` | 升序并过滤pending unregister/destroy/dormant。 |

`BattleEcsFramePostProcessPass` 的 Legacy、ShadowCompare 与 DataOriented 三种模式共享同一合同；既有focused测试
覆盖positive count、zero、negative、nonzero hold、inactive/pending/dormant、extended capacity与warmed zero-GC。
B3 placement Play还实际观察过 pending被消费为Vx并清count/accumulator，但那只是placement witness，不替代最终
跨producer trace。

## 非 C22 本体的未关闭差异

- `BattleCpointWriter` negative-decrease release误把 native frame counter写成`HitCount`并继续throw；已路由
  `NTSD28-B6-CATCH-CONTROL-FLOW-FENCES-OWNER-AUDIT-001` 的 mixed/exact advance production。
- kind10/11/17/18 impact的 pending/instant motion、count-zero mirror和exact除法仍由
  `NTSD28-B6-NATIVE-IMPACT-10111718-OWNER-AUDIT-001` 三包关闭。
- ordinary/reduced/type3 hit producers属于B5已迁规则族，但完整same-seed joint trace仍待B12。
- C21 `PreFrameBounds`/stage settlement 的mode/dynamic/offstage removal算法归B8；C22只要求被移除实体不可进入。
  Unity active traversal会过滤已unregister/pending-destroy实体，但具体谁在C21被移除不能由C22反向定义。
- catch relation/settlement可能在finalizer前改变hold/motion/pending；这些producer差异均保留各自B6 package。

## 结论

未发现 C22 finalizer consumer 的独立首差，因此不新建重复 behavior package。状态仅为
`C22_STANDALONE_EXIT_READY`，不是整个 hit impulse、stage 或 B6完成。

最终关闭依赖：

1. 上述 catch/impact producer packages分别runtime绿灯；
2. B8 stage settlement/offstage lifecycle闭合；
3. 同tick多命中、count0、hit-stop延迟、stage removal、catch escape/throw、impact与三轴组合trace；
4. Legacy/DataOriented/ShadowCompare一致、BattleRuntimeSelfCheck、真实Play与first-difference为零。

## 不变量

- 不改 finalizer、HitCount/Knockback carrier、pass order、stage、hit/catch producer、content、Scene或Authority。
- 不把B3 placement Play或静态公式等价扩大为整个冲量系统已对齐。
- Unity licensing仍阻止fresh runtime验收；本轮没有尝试重启Unity。

## 回滚

仅移除本治理记录与摘要；没有代码、content、Scene或Authority回滚。
