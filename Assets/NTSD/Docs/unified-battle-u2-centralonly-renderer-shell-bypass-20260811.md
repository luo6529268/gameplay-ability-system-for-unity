# U2 CentralOnly Renderer Shell Bypass Evidence (2026-08-11)

## 结论

`CentralOnly` 已不再在每个 Unity `LateUpdate` 中遍历全部 `LF2ObjectRenderer` 并执行组件级 `SimLateTick`。中央表现命令的逻辑快照、实体可见性、阴影判定、局部偏移和排序输入均已由 `BattlePresentationCoordinator` / `BattleCentralRenderSystem` 直接消费；继续逐实体刷新 Legacy shell 不再是中央表现的生产依赖。

该旁路只作用于 `CentralOnly`：

- `LegacyOnly` 与 `CentralShadowBuild` 仍保留原来的逐 renderer late pass；
- 切换表现 backend 时会做一次性的 Legacy renderer suppression 同步；
- 新绑定到世界的 renderer 会立即同步当前 backend suppression；
- 不改变 tick、输入、碰撞、命中、opoint、生命周期或 lockstep 状态。

## 正确性证据

- Unity fresh script refresh：成功；
- 聚焦 EditMode：`246/246 PASS`；
- `BattleRuntimeSelfCheck`：`PASS`；
- Combat1000：180 个采样 tick，`0 B/tick`、Gen0/1/2 均为 0；
- 最终 overall lockstep hash：`a13929d82b19c54e871522a1921f658ddfa88a7e7bc8655149d76006e1c504e1`，与旁路前基线一致；
- world/slot 等分域 hash 与基线一致；
- cleanup `restored=true`，异常数为 0。

## 可比 CPU hierarchy

旁路前：

- Main Thread 平均：`45.6808 ms`；
- `SimulationTickDriver.LateUpdate` inclusive：`9.3253 ms`；
- `SimulationTickDriver.LateUpdate` self：`6.4431 ms`。

旁路后：

- Main Thread 平均：`40.1213 ms`；
- `SimulationTickDriver.LateUpdate` inclusive：`7.8596 ms`；
- `SimulationTickDriver.LateUpdate` self：`5.1974 ms`。

变化：

- Main Thread 平均减少 `5.5595 ms`，约 `12.2%`；
- LateUpdate inclusive 减少 `1.4657 ms`，约 `15.7%`；
- LateUpdate self 减少 `1.2457 ms`，约 `19.3%`。

正式无诊断 smoke 的采样仍受 Editor 帧时波动影响，因此不把单次可见帧均值作为独立晋升证据。保留该优化的依据是同一 CPU hierarchy 采样方式下超过 10% 的主线程改善，以及完整的确定性、零 GC 和回归验证。

## 未关闭项

该优化只移除了 CentralOnly 的重复 Unity shell 扫描，并未关闭 U9：当前同一 hierarchy 的主线程仍为 `40.1213 ms/frame`，其中 EditorLoop 约 `4.7728 ms`。下一阶段继续处理 U4 的 `CandidateCollect` 与 `LateEntityUpdate` 高频数值段；不把本文件扩张成 1000 AI / 30 FPS 已完成声明。
