# Task Contract — NTSD28-B3-OID5152-MAINTENANCE-PLACEMENT-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / NEXT-PRODUCTION-SPLIT`
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C12+C25h`
> 依赖：`NTSD28-B3-COOLDOWN-WRITER-EXTRACTION-001 / VERIFIED`

## 目标与结论

只读拆分Unity `Oid5152RuntimeMaintenance`相对Bug修复版Authority的owner与时点：

- Authority `advance_native_fusions`在C12 candidate后、type-0 hit前执行，读取本tick尚未递减的
  `input_special_timer_338`决定merge/defuse。
- `input_special_timer_338`正值递减属于C25h `advance_reaction_timers_slot`，位于当前entity frame后。
- Unity当前`RunMaintenance`在C03后遍历slot0..19，先`Unk338--`，再同slot merge/split；timer=1会同tick变0并
  提前split，且NeedClear partial也会错误推进maintenance。

下一实现必须保留旧combined direct入口供compatibility tests，同时把production拆成C12 fusion scan与C25h
per-slot timer decrement。完成后下一首差预期为`CoreFrameMotion / EarlyFrameAdvance`。

## 边界

本审计不修改脚本、Scene/Prefab/Config、资源或authority。具体实现使用
`NTSD28-B3-OID5152-PRODUCTION-SPLIT-001`，需要test-first覆盖timer=1延迟split、partial不推进、direct兼容不变、
slot/dormant identity与worker路径。
