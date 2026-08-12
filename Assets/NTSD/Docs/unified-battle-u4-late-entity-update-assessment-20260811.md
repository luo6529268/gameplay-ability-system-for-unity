# U4 LateEntityUpdate 高频数值段评估

> 日期：2026-08-11  
> 结论：U4 不新增 canonical writer；保留当前逐 slot Legacy 路径  
> 后续归属：FrameTick、opoint、销毁与逐实体结构可见性进入 U5

## 1. 权威顺序

权威 C#：

- `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Simulation\GameTick.cs`
- `RunLatePerEntityUpdatePass`
- `RunLateEntityUpdate`

每个 runtime slot 的正式顺序是：

1. state special；
2. character stats recovery；
3. `FrameTickRuntime.Tick`；
4. frame exit/free；
5. death/weapon drop；
6. opoint；
7. weapon cleanup/free；
8. N30 input、transition effects；
9. `PrevFrame` mirror。

该顺序按 slot 交错执行。较低 slot 产生、释放或改变的实体，可能影响本 pass 后续较高 slot 的可见状态，因此不能把整个 pass 预先批处理成若干全世界阶段。

## 2. 新鲜 1000 AI 测量

报告：

`Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-capacity-pressure-smoke.json`

运行结果：

- `StoppedCleanly`；
- harness valid；
- 预热后 GC `0 B`；
- 最终 lockstep overall hash：`b8a07be2e5ed9e94f150f4b6e0e426e6e8d23630c69e5fe05a39636e63707821`；
- 整体 logic average `24.1361 ms`，P95 `29.5579 ms`；
- `LateEntityUpdate` average `2.9092 ms`，P95 `3.6273 ms`。

Late 子段：

| 子段 | average | P95 |
|---|---:|---:|
| TailAndQueuedFlush | 0.7622 ms | 0.9419 ms |
| FrameTick | 0.5007 ms | 0.6464 ms |
| Recovery | 0.2290 ms | 0.3148 ms |
| OpointProcess | 0.1794 ms | 0.2338 ms |
| DeathOpoint | 0.1693 ms | 0.2045 ms |
| Cleanup | 0.1091 ms | 0.1336 ms |
| StateSpecial | 0.1068 ms | 0.1456 ms |
| 其他全部子段 | 小于 0.1 ms/项 | 小于 0.1 ms/项 |

当前 `ConsolidatedFinal` 已把 Late runtime snapshot 从历史上的多次刷新合并为每个活动实体一次：180 个采样 tick 共 180000 次，average `0.4320 ms`、P95 `0.5248 ms`。

## 3. 决策

- Recovery 是唯一明显的纯数值子段，但平均仅 `0.2290 ms`，独立新增 SoA writer、Legacy oracle、提交与回退会吞掉潜在收益；
- FrameTick 虽为数值热点，但它包含 DAT 帧推进、状态和生命周期副作用，不属于 U4 可无结构变化迁移的范围；
- Tail、opoint、death、cleanup 必须保留逐 slot 结构可见性，归入 U5；
- runtime snapshot 已完成合并，继续缩减字段需要重新证明所有消费者，不在本轮盲目裁剪；
- 本项不新增实验代码，避免复制一套低收益 canonical writer。

因此 U4 以“cooldown 与 AI 晋升；Stage-Z、FramePostProcess、Candidate 零 ITR、Late 数值段完成评估并按门槛取舍”关闭。U5 从 Interaction/Hit/Rest 和复杂生命周期继续迁移，不把本结论扩写为 U9 性能门禁完成。
