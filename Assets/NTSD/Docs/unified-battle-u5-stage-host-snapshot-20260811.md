# U5 Stage 宿主快照边界报告

> 日期：2026-08-11  
> 状态：本切片已完成；U5 整体仍在执行  
> 范围：Stage 场景配置进入确定性战斗内核前的宿主快照边界

## 1. 问题与边界

修改前，同一逻辑 tick 的两次 `StageBounds` 与一次 `PreFrameBounds` 都可能各自访问 Unity 场景并解析 `StageManager`。这有两个问题：

1. Unity 场景对象被内核 pass 直接读取，宿主状态和确定性战斗数据的边界不清晰；
2. 同一 tick 重复执行三次等价场景解析，且未来回放、同进程服务器和无 GameObject 内核不能复用这条路径。

本切片不改变 C# 权威中的边界、移动或 pass 顺序。它只把 Unity-native 的场景读取前移到 `SimulationTickDriver` 的 tick 宿主入口；`StageBounds`、`PreFrameBounds` 与 ECS Stage-Z pass 只读取 `BattleStageRuntimeState`。

## 2. 实现合同

- `SimulationTickDriver.StepOneTickInternal` 在进入 managed-memory boundary 和战斗 kernel 前调用一次 `PrepareStageRuntimeSnapshotForTick(tickIndex)`；
- 相同 tick 的重复准备会复用已发布快照；
- `ClampCharacterZToStageBoundsAll`、`ApplyPreFrameBoundsAll` 和 `BattleEcsCharacterStageZPass` 不再直接解析 Unity 场景；
- 显式测试快照仍是 kernel 真值，并且不会被场景读取覆盖；
- `ForceLegacyPerPassStageRefreshForDiagnostics` 保留旧的“每 pass 读取场景”路径，仅用于 A/B 与回归定位；
- 两条路径都保持原 pass 顺序、Stage 数值、实体顺序和最终战斗状态不变。

## 3. 聚焦验证

新增 `BattleStageHostSnapshotBoundaryEditorTests`，覆盖：

1. 一个宿主快照被三次 kernel stage pass 复用；
2. 同 tick 去重、下一 tick 刷新；
3. Legacy 诊断模式跳过宿主准备并执行三次 pass 读取；
4. 显式 Stage 快照不访问 Unity 场景。

新鲜结果：

- `BattleStageHostSnapshotBoundaryEditorTests`：`4/4 passed`，job `0fe5c53cd9be429bbb62d9e13617ab8d`；
- `StageBoundsRuntimeSyncEditorTests`：`5/5 passed`，job `64579b1aac544a3198d97c7701e7ec0f`；
- `BattleEcsCharacterStageZPassEditorTests`：`4/4 passed`，job `3eae74435656489bb9deec6fcd9d8552`；
- 压力工具全类回归：`233/233 passed`，job `dcbffa65a6234192be8a575318bae20d`；
- `BattleRuntimeSelfCheck`：`2026-08-11 20:46:02 PASS`。

## 4. 1000 AI 相邻 A/B

共同配置：

- `Combat1000`，1000 个真实生产 AI；
- seed `1314149188`；
- `DataOrientedCanonical` AI；
- role-aware 正式碰撞收集器；
- 30 warmup + 180 sample；
- 每个 Unity Update 最多一个逻辑 tick；
- phase、presentation、detail timing 开启；
- 正式 tick 要求零 GC。

唯一变量是 `forceLegacyPerPassStageRefresh`。

| 指标 | Legacy A | Host snapshot B | 结论 |
|---|---:|---:|---|
| Unity 场景解析次数 | 630 | 210 | 减少 66.7% |
| Host prepare 次数 | 0 | 210 | 每 tick 一次 |
| Legacy per-pass 读取次数 | 630 | 0 | 默认路径已关闭 |
| Logic tick average | 24.6466 ms | 25.0015 ms | B 慢 1.44%，属本轮噪声/轻微回退 |
| Logic tick P95 | 32.5412 ms | 33.6139 ms | B 慢 3.30% |
| StageBounds average | 1.2995 ms | 1.2359 ms | 改善 4.90% |
| StageBounds P95 | 1.6819 ms | 1.5724 ms | 改善 6.51% |
| PreFrameBounds average | 0.8364 ms | 0.8161 ms | 改善 2.42% |
| 正式 tick GC | 0 B / 0 collections | 0 B / 0 collections | 一致 |
| 十域 lockstep overall hash | `a13929d8...04e1` | `a13929d8...04e1` | 完全一致 |
| 诊断开关恢复 | true | true | 一致 |

报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.u5-stage-host-legacy-a.json`
- `Temp/NTSD_ProductionEntityStress.combat1000.u5-stage-host-fast-b.json`

## 5. 结论

本切片的价值是确定性架构边界和重复场景访问收敛，不是当前 1000 AI 性能的主要收益来源。正式路径保留宿主快照，因为：

- 20 个 parity/lockstep 分域 hash 全部一致；
- 正式 tick 维持零 GC；
- Unity 场景读取由每 tick 三次降为一次；
- kernel pass 不再依赖 Unity 场景查询，符合后续回放和同进程服务器边界。

同时必须诚实记录：本轮总体 tick 平均和 P95 没有改善，因此不能把该切片计入 30 FPS 的主要性能增益。U5 后续仍应依据 phase timing 选择真正占时的 Interaction/Hit/生命周期路径，复杂 cpoint、held、link、object hit、aRest/vRest 和结构命令尚未完成迁移。
