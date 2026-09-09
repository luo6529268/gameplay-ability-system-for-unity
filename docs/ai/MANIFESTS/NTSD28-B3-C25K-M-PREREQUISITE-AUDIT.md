# NTSD 2.8-Logan C25k/m prerequisite audit

> Change ID：`NTSD28-B3-C25K-M-PREREQUISITE-AUDIT-001`  
> 状态：`VERIFIED / GOVERNANCE_ONLY / DEPENDENCY_ORDER_CORRECTED`

## 1. terminal producer

Authority `BattleWorld28::step_frames_range`在frame machine、next999 resolution、transition costs及post-cost terminal recheck后，仅当结果为terminal code才写：

```text
lifecycle_resolution_pending = true
lifecycle_code = result.to_action
```

C25k首先检查该pending；C25o最终按原始code执行11xx/12xx reset或terminal despawn。Unity当前没有这两个等价carrier，且`RunNativeC25FrameBodyForWorldPass`仍会把一部分999提前改写为0/212，不能在C25o从当前frame反推。

## 2. previous-action readers

| reader | 时点 | C25m前是否必须保留旧值 |
|---|---|---|
| collision/hit writer与plan | C14/C16等，早于C25 | 已完成读取，不阻塞C25m |
| `SpawnLateTransitionEffects` state18/19 branch | 当前C25尾段 | 是；对应Authority C25l |
| `SpawnLateTransitionEffects` state13/200 branch | 当前mixed legacy tail | 是；必须从C25l拆出或显式接收capture |
| diagnostic/snapshot/checksum | completed tick | 应看到C25m提交后的新值 |
| `RunLateCharacterDatInputTrigger` | 当前tail | 不读取`Frame.Prev` |

## 3. 路由修正

- 下一：`C25l/m transition-history transaction`，先以旧Prev完成state18/19 branch，再提交`Frame.Prev=Frame.N`；state13/200 branch必须保留其旧值语义但不冒充C25l。
- 延后：C25k与C25g terminal producer、B7 pending/code carrier和C25o联合闭合；届时一并裁决Unity额外FrameDelay gate。
- C25n/o仍按既有B7/B10/B11/H依赖处理。
