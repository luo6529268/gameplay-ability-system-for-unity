# U5 正向持有链接验证迁移评估（2026-08-11）

> 对应总计划：`unified-battle-lockstep-ecs-server-architecture-plan.md`
> 结论：数据化候选保持诊断模式，生产默认继续使用 Legacy canonical writer；本次关闭“是否应直接迁移该 pass”的评估，不表示 U5 的真实 cpoint/held/link 域已经完成。

## 1. 权威合同与边界

权威入口为 `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Simulation\GameTick.cs` 的 `ValidatePositiveLinks`。该 pass 位于 `RunCPoint`、`SyncHeldWeapons` 之后，并位于第二次 stage Z clamp 与 `RunHeldWeaponStep12` 之前。合同是：

- 按 runtime slot 升序检查当前 active 且 `LinkState > 0` 的 holder；
- target slot 越界、target 非 active，或 `target.HolderIdx != holder slot` 时，只清除 holder 的 `LinkState`、`TargetIdx`、`HeldWeaponSlot`；
- 合法 reciprocal link 保持不变；
- target 的反向字段不在本 pass 修改；
- cpoint/weapon synchronization 在同一 tick 较早位置写入的链接必须立即可见，不能读取 tick-end ECS shadow 的过期值。

Unity 候选实现位于 `Scripts/Simulation/Ecs/BattleEcsPositiveLinkValidationPass.cs`，提供 `Legacy`、`ShadowCompare`、`DataOriented` 三种只能在 reset/合法 restore 边界切换的模式。候选读取当前 `RuntimeSlotTable` 和 live runtime，而不把 tick-end `BattleEcsWorld` shadow 当作 canonical truth；结构 witness 继续保留原 slot、前后字段和 reason/outcome。

## 2. 正确性与工具回归

聚焦测试覆盖：

- 默认 Legacy 与切换边界；
- ShadowCompare 的合法/失效链接一致性；
- DataOriented 与 Legacy 的 holder 清理结果一致，且 target 反向字段不被改写；
- 同 tick 新写入 link 读取 live runtime，不误读过期 ECS shadow；
- W07 结构 witness 一致；
- Extended1000 预热后执行为 `0 B`。

验证结果：

- 新 pass 聚焦测试：6/6 PASS；
- 压力工具、W07 与新 pass 联合回归：243/243 PASS，job `5b78a97bfa7c407b863eb04f81694f2c`；
- `dotnet build Assembly-CSharp.csproj --no-restore -v:minimal`：0 error，43 个既有 warning；
- `BattleRuntimeSelfCheck`：2026-08-11 23:08:08 fresh PASS。

候选没有晋升为 production canonical writer，因此本切片不把隔离候选的正确性测试扩大为 U5 Authority400 完成声明。

## 3. 1000 AI 同配置 A/B

两轮均为真实 1000 GameObject/逻辑实体、Combat1000、全 AI、DataOrientedCanonical AI、role collector、30 warmup + 180 sample、`maxCatchUpTicksPerFrame=1`、相同 seed、phase/detail timing 和正式零 GC 门禁；唯一变量是 `positiveLinkValidationMode`。

| 指标 | Legacy A | DataOriented B | 变化 |
|---|---:|---:|---:|
| HeldLinkValidation average | 0.100013 ms | 0.150752 ms | +50.73% |
| HeldLinkValidation P95 | 0.130110 ms | 0.166275 ms | +27.80% |
| HeldLinkValidation max | 0.216100 ms | 0.226900 ms | +5.00% |
| 整体逻辑 tick average | 26.757153 ms | 23.698291 ms | -11.43% |
| 整体逻辑 tick P95 | 40.614405 ms | 29.254965 ms | -27.97% |
| sampled logic GC | 0 B/tick | 0 B/tick | 一致 |

这组 workload 没有正向 link，两个模式均执行 210 次，候选的 linked participant visit 为 0；因此目标 pass 数据反映的是 1050-slot 空扫描成本。DataOriented 目标 pass 明确变慢，而整 tick 恰好反向波动，故整 tick 差值不能归因于本候选，也不能作为晋升依据。

两轮最终 input、RNG、metadata、world、slots、aRest、vRest、stats、events 与 overall hash 全部逐域一致；overall 均为 `b8a07be2e5ed9e94f150f4b6e0e426e6e8d23630c69e5fe05a39636e63707821`。两轮 cleanup/restoration 均通过。

报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.u5-positive-link-legacy-a-20260811.json`
- `Temp/NTSD_ProductionEntityStress.combat1000.u5-positive-link-data-b-20260811.json`

## 4. 决策与下一步

候选没有达到计划规定的目标 pass P95 改善门槛，反而发生稳定的局部退化，因此：

- 生产默认保持 `Legacy`；
- `ShadowCompare`/`DataOriented` 仅保留为后续 link store 设计的诊断证据，不形成双 canonical writer；
- 不用整体 tick 的自然波动掩盖目标 pass 负优化；
- 不为约 0.10 ms 的 pass 立即引入跨 writer 的 live-link 索引维护复杂度；
- U5 继续处理存在真实交互时的 held/hit/opoint 与结构生命周期，届时再由统一的 canonical link store 消除对象式扫描，而不是单独复制一条更慢的 slot loop。

