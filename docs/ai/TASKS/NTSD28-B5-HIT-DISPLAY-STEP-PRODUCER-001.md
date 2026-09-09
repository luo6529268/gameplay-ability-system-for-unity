# Task Contract — NTSD28-B5-HIT-DISPLAY-STEP-PRODUCER-001

> 状态：`VERIFIED / TYPE0_5_DISPLAY_STEPS_ALIGNED / TYPE6_SKIP_VERIFIED`
> 依赖：`NTSD28-B5-HIT-RESOURCE-PREREQUISITE-AUDIT-001 / VERIFIED`

## 目标

实现`apply_native_hit_resource_transfer`入口处、local/F6 gate之前的positive-injury四个display
interpolation step写入，并接入target type0..5；type6保持整个tail跳过。

## Authority 合同

- `injury > 0`：`DisplayScoreStep1F4`、`DisplayDamageStep1FC`、
  `DisplayCurrentHpStep204 = injury / 10`；`DisplayEffectiveMaxHpStep20C = injury / 20`。
- `injury <= 0`不写、保留旧值。
- type0/1/2/3/4/5调用；type6不调用。
- 写入不受local/F6 gate影响，不消费RNG。

## 不变量

- 只写四个既有carrier；不实现MP reward/drain/gain。
- 不改Config/DAT/Scene/Authority/资源/snapshot schema或显示消费算法。

## 受影响路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5HitDisplayStepProducerEditorTests.cs`

## 验收

test-first；pure positive/zero/negative、type0..5 production与type6 skip；相关hit/resource/carrier、
精确NTSD28 broad、SelfCheck、Scene/Console/Ledger。

## 回滚

移除helper、三个production seam调用与focused test。

## 验证结论

- test-first fresh compile red：2个预期`CS0117`。
- fresh compile 0 error；focused `f81627c6d6eb4580806ff3635440fc69` 10/10。
- hit/resource/carrier related `be27e146083948e692371f69dd1121ba` 255/255。
- 精确NTSD28 broad `b8cff2f87b7d49749331c52ab4199326` 616/616。
- BattleRuntimeSelfCheck `2026-09-05T12:00:34Z` PASS；Scene unchanged。
