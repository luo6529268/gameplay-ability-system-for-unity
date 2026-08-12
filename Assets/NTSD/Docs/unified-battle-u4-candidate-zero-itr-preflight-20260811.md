# U4 CandidateCollect 零 ITR 前置快速返回评估

> 日期：2026-08-11  
> 结论：未晋升；LegacyOnly 前置路径及其快照统计代码已撤回  
> 保留范围：既有 StoreOnly 零 ITR 诊断路径保持原状

## 1. 假设与权威边界

权威 C# `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Interaction\CollisionCollect.cs` 在攻击方当前帧和前一帧均没有 ITR 时会立即结束该方向的 candidate 收集。Unity 在 CandidateCollect 之前已经独立执行 PairVRest，因此本轮评估的假设是：当同一 collision snapshot 能证明所有正式参与者都没有 release ITR 时，可以直接发布空 candidate 结果，跳过 participant、broadphase 与 exact 阶段。

实现先后评估了两版：

1. 再扫描一次全部实体并判断 ITR；
2. 在既有 `CaptureCollisionFrameSnapshotsAll` 遍历中累计 ITR 摘要，再由 CandidateCollect 做 O(1) 前置判断。

两版都只在显式压力诊断开关下启用，没有修改生产默认。

## 2. 正确性结果

同代码版本的 1000 AI A/B 均满足：

- `status = StoppedCleanly`；
- `harnessValidity = true`；
- 预热后 GC 分配 `0 B`；
- overall/world/slots 等最终 lockstep hash 完全一致；
- O(1) 版本 210 个逻辑 tick 中应用 21 次、保守回退 189 次、非法摘要 0 次。

这证明快速返回没有改变该夹具的最终战斗状态，但不能单独证明它值得进入生产路径。

## 3. 性能结果

在同一编译代码、相邻 B3→A3 顺序下：

| 指标 | A3：关闭 | B3：开启 | 开启后的变化 |
|---|---:|---:|---:|
| Logic average | 23.0366 ms | 23.4611 ms | 慢 1.84% |
| Logic P95 | 27.5112 ms | 28.2757 ms | 慢 2.78% |
| Unity frame average | 43.5835 ms | 45.2026 ms | 慢 3.71% |
| Unity frame P95 | 50.2404 ms | 51.0504 ms | 慢 1.61% |

报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-performance-smoke.json`
- `Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-zero-itr-preflight-smoke.json`

首次 B2 样本还出现过更大的 Editor 抖动；反向复跑后仍未出现正收益。根本原因是该 1000 AI 夹具只有 21/210 tick 能快速返回，而把摘要检查加入每 tick 的 collision snapshot 会让其余 189 tick 额外付费。

## 4. 决策

- 不晋升 LegacyOnly 零 ITR 前置快速返回；
- 撤回额外 collision snapshot 摘要、LegacyOnly 入口、压力菜单与专用测试；
- 恢复既有 StoreOnly 零 ITR 诊断有效性规则；
- 后续 CandidateCollect 优化必须针对高覆盖的 participant/body/itr 数据构建或 exact 访问，而不是继续增加低命中率的全局前置条件。

因此本项是“正确但性能无收益”的关闭实验，不计为 U4 已完成子项。
