# U5 CharacterHitConsume 空候选快速路径报告

> 日期：2026-08-11  
> 状态：本切片已完成；U5 整体仍在执行  
> 范围：U5 的 character hit 空候选切片；不代表 U5 整体完成

## 1. 权威边界

唯一战斗逻辑权威仍为 `J:\QQFile\NTSD2.4\ntsd_release_C#`。

本切片核验了以下调用链：

- `src/BattleCore/Simulation/GameTick.cs`：碰撞快照、候选收集、character hit 与 object hit 的先后顺序；
- `src/BattleCore/Interaction/HitResolve.cs`：角色命中只消费该 tick 已发布的候选，真实候选存在时必须保持原消费顺序与副作用；
- Unity 对应入口：`SimulationWorld.PostInteractionTickAll(int)`。

优化不改变 pass 顺序，不提前消费候选，不改变候选内容，也不把 object hit 合并进 character hit。

## 2. 实现合同

`SimulationWorld.PostInteractionTickAll` 只在以下条件全部成立时跳过对象式 `SimPostInteraction` 调用和其后的冗余快照刷新：

1. 实体运行时类型精确为 `LF2Character`；
2. 当前 base runtime snapshot 已与实体字段一致；
3. 当前 scene query 可以读取该角色的候选范围；
4. 候选数量为零。

以下情况全部 fail closed 到原有权威对象路径：

- 任何派生角色类型；
- base snapshot 过期；
- scene query 或候选范围不可用；
- 候选数量非零；
- 诊断开关 `ForceLegacyEmptyCharacterHitConsumeForDiagnostics` 强制使用旧路径。

因此，真实命中、派生类虚调用、候选消费顺序和命中副作用不进入快速路径。

## 3. 聚焦测试

新增 `BattleEcsEmptyCharacterHitConsumeEditorTests`，覆盖：

1. 精确角色空候选跳过且与强制 Legacy checksum 一致；
2. 候选源不可用时回退；
3. 已发布快照过期时回退并刷新；
4. 派生角色保留虚调用；
5. 预热后的快速路径 `0 B GC.Alloc`。

初次新鲜测试任务 `b2bde62030664635a4adc2b11e876d34`：`5/5 passed`。

## 4. 1000 AI A/B 合同

所有正式样本共同配置：

- seed：`1314149188`；
- `Combat1000`，1000 个真实生产 AI；
- AI：`DataOrientedCanonical`；
- 30 warmup + 180 sample；
- 每个 Unity `Update` 最多 1 个逻辑 tick；
- role-aware 正式碰撞收集器；
- phase、presentation 和 detail timing 开启；
- 正式逻辑 tick 要求 `0 B`；
- 最终十域 lockstep overall hash：`a13929d82b19c54e871522a1921f658ddfa88a7e7bc8655149d76006e1c504e1`。

### 4.1 稳定相邻样本

| 指标 | Legacy D | Fast E | 改善 |
|---|---:|---:|---:|
| Logic tick average | 28.858307 ms | 24.595035 ms | 14.8% |
| Logic tick P95 | 42.179160 ms | 30.664775 ms | 27.3% |
| CharacterHitConsume average | 1.393227 ms | 0.606186 ms | 56.5% |
| CharacterHitConsume P95 | 2.442330 ms | 0.978475 ms | 59.9% |
| 空候选跳过次数 | 0 | 208,846 | — |
| 实际/回退执行次数 | 210,000 | 1,154 | — |
| 正式 tick GC | 0 B | 0 B | 一致 |
| 最终 hash | `a139...04e1` | `a139...04e1` | 完全一致 |
| teardown restored | true | true | 一致 |

报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.u5-empty-hit-legacy-d.json`
- `Temp/NTSD_ProductionEntityStress.combat1000.u5-empty-hit-fast-e.json`

### 4.2 被排除的噪声样本

`fast-c` 的 Logic tick average 为 `56.232809 ms`、P95 为 `86.583235 ms`。该轮不仅目标 pass，CharacterInput、FrameAdvance、PreInteraction、CandidateCollect、LateEntityUpdate 和 RenderDispatch 也同时接近翻倍，属于 Editor 全局负载污染，不用于性能晋升判断。它的 hash 与零 GC 证据仍有效。

## 5. 结论

稳定 D/E 相邻 A/B 远高于 10% 的晋升门槛，同时满足：

- 真实候选和派生类型 fail closed；
- 十域 lockstep hash 完全一致；
- 正式逻辑 tick `0 B`；
- teardown 与诊断开关恢复成功；
- 聚焦行为测试通过。

因此保留空候选快速路径为生产默认，并保留强制 Legacy 诊断开关作为 A/B oracle。该结论只覆盖 character hit 空候选切片；object hit、rest、cpoint/held/link、opoint 与复杂生命周期仍按 U5 分段验证。

## 6. 本批最终复核

- 强制 Unity `all/force` refresh 完成，Console 无 C# 编译错误；Console 中仅保留 UnityMCP disposed-client 噪声和 self-check 故意触发的注册拒绝错误日志；
- 最终空候选聚焦测试任务 `320c1a4798b74d87a9b35cba76515d26`：`5/5 passed`；
- PreInteraction 既有权威合同任务 `8894f026178948498ca8a1a33074cce7`：`7/7 passed`；
- role-aware collision 相关回归任务 `4f30c405f6be47509ef76a30e299cadc`：`9/9 passed`；
- `BattleRuntimeSelfCheck`：`2026-08-11 20:09:08 PASS`；
- Fast E 报告 teardown：`restored=true`，诊断开关恢复为原值，活动实体、world entity 和 claimed slot 均恢复为 0。
