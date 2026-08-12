# U5 PreInteraction whole-pass no-op 证明报告

> 日期：2026-08-11  
> 状态：本切片已完成并保留为生产默认；U5 整体仍在执行  
> 范围：cpoint、held weapon 与 positive link 修正之前的精确空操作证明

## 1. 权威顺序

唯一战斗逻辑权威为 `J:\QQFile\NTSD2.4\ntsd_release_C#`。

`src/BattleCore/Simulation/GameTick.cs` 明确规定，帧推进和第一次 Stage-Z 修正之后，必须依次执行：

1. `InteractionRuntimePasses.RunCPoint(world)`；
2. `InteractionRuntimePasses.SyncHeldWeapons(world)`；
3. `ValidatePositiveLinks(world)`；
4. 第二次 Stage-Z 修正；
5. `InteractionRuntimePasses.RunHeldWeaponStep12(world)`；
6. 冻结 `PrevFrame2` 后才允许收集碰撞候选。

`src/BattleCore/Interaction/CPointRuntime.cs` 则定义了 cpoint kind 1/2、caught/catcher、动作切换、投掷和 held 同步的副作用。因此本切片不能合并、重排或简化真实 cpoint/held/link 行为；只能在能够证明三段调用对整个参与集合均无副作用时跳过。

## 2. Unity 实现合同

`SimulationWorld.PreInteractionTickAll` 的默认路径先执行 whole-pass proof。只有以下条件全部成立才跳过三段对象式调用：

- 当前所有有效参与者都是精确 `LF2Character`，派生角色不能进入证明；
- base runtime snapshot 与对象字段一致；
- 当前帧、前帧、cpoint kind、link、target、held stable id 和 held reference 都满足中性条件；
- slot generation 与运行时 occupant 仍对应；
- pass 执行前的注册、注销和结构命令计数没有变化；
- `SuppressPreInteractionUntilTick` 的对象不参与本 tick 证明，也不阻断其余中性对象。

任一条件失败时，整个 pass fail closed 到原有的三段对象式权威路径。`ForceLegacyPreInteractionForDiagnostics` 可以强制旧路径，用于 A/B 与回归。

## 3. 聚焦测试

`PreInteractionNoOpProofEditorTests` 共 7 项，覆盖：

1. 中性精确角色与强制 Legacy 等价；
2. cpoint kind 1/2 与陈旧 held 状态回退；
3. frame、wait、位置快照不一致时回退；
4. 派生类型保留虚调用副作用；
5. 被 suppress 的派生对象不污染其余参与者证明；
6. slot generation 复用后解析新 occupant 并回退；
7. 预热后 whole-pass proof 为 `0 B GC.Alloc`。

新鲜结果：`7/7 passed`，job `774431f2801d409b82a2cf797e6ef462`。

压力工具全类回归：`233/233 passed`，job `9e9d0b58938c4e5e8b85e53289bacd1d`。

## 4. 1000 AI 相邻 A/B

共同配置：

- `Combat1000`，1000 个真实生产 AI；
- seed `1314149188`；
- `DataOrientedCanonical` AI；
- role-aware 正式碰撞收集器；
- Stage host snapshot 默认路径；
- 30 warmup + 180 sample；
- 每个 Unity Update 最多一个逻辑 tick；
- phase、presentation、detail timing 开启；
- 正式 tick 要求零 GC。

唯一变量是 `forceLegacyPreInteraction`。

| 指标 | Legacy A | Whole-pass proof B | 改善/结论 |
|---|---:|---:|---:|
| Logic tick average | 26.5189 ms | 23.6455 ms | 10.84% |
| Logic tick P95 | 37.1088 ms | 29.5015 ms | 20.50% |
| PreInteraction average | 2.1038 ms | 1.3645 ms | 35.14% |
| PreInteraction P95 | 2.9904 ms | 2.1559 ms | 27.91% |
| whole-pass proof 成功 tick | 0 | 91 | — |
| proof participant 累计 | 0 | 91,000 | — |
| 实际对象式调用次数 | 630,000 | 357,000 | 减少 273,000 |
| proof skip 次数 | 0 | 273,000 | — |
| 正式 tick GC | 0 B / 0 collections | 0 B / 0 collections | 一致 |
| 十域 lockstep overall hash | `a13929d8...04e1` | `a13929d8...04e1` | 完全一致 |
| 20 个 parity/lockstep 分域 hash | 全部一致 | 全部一致 | 0 个差异 |
| 诊断开关恢复 | true | true | 一致 |

报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.u5-preinteraction-legacy-a.json`
- `Temp/NTSD_ProductionEntityStress.combat1000.u5-preinteraction-fast-b.json`

## 5. 结论

whole-pass proof 满足行为、确定性、零 GC 和性能晋升条件，继续作为生产默认。它没有删除复杂交互逻辑：本轮只有 91/210 个 tick 被证明为全局 no-op，其余 119 个 tick 仍完整执行 357,000 次原对象路径调用。

这关闭的是 U5 的“可证明空操作 PreInteraction”切片，不代表 cpoint、held、link 已经迁移为纯数据 writer。真实抓取、持有、投掷、链接修正和结构命令仍由现有权威对象路径处理，后续必须分别建立数据契约与差分验证。

## 6. 最终复核

- Unity `all/force` 刷新后无 C# 编译错误；Console 仅有 UnityMCP disposed-client 噪声；
- `PreInteractionNoOpProofEditorTests`：`7/7 passed`；
- `ProductionEntityStressEditorTests`：`233/233 passed`；
- 两份 1000 AI 报告均 `StoppedCleanly`、零 GC、20 个 hash 全部一致；
- `BattleRuntimeSelfCheck`：`2026-08-11 20:58:31 PASS`。
