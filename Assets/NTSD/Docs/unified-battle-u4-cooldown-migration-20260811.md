# U4 Cooldown Canonical Writer 迁移验收（2026-08-11）

> 对应总计划：`unified-battle-lockstep-ecs-server-architecture-plan.md`
> 阶段：U4 第一个切片
> 结论：cooldown canonical writer 已从逐实体对象快照路径迁移到预分配的数据化路径并晋升默认；U4 其余切片仍未完成。

## 1. 权威合同与实现边界

权威入口为 `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Simulation\GameTick.cs` 的 `RunCooldownsTick`。该 pass 位于输入之前，按活动 runtime slot 升序处理：

- `ARest > 0` 时递减一次；
- `AttackExempt > 0` 且存在 DAT 时，当前 frame 没有 itr 就清零；
- state 1001 的被持有对象在 holder 和 holder DAT 有效时，holder 当前 frame 没有 wpoint 或第一条 wpoint 的 `attacking == 0` 就清零；
- `Cd*` 不属于这个 pass，它们仍由权威输入链负责。

Unity 新实现为 `Scripts/Simulation/Ecs/BattleEcsCooldownPass.cs`。它提供三种只可在 world reset/启动边界切换的模式：

- `Legacy`：执行原 `SimulationWorld.VrestTickAll`；
- `ShadowCompare`：预先计算全部受影响字段的期望值，再执行 legacy writer 并逐槽验证；
- `DataOriented`：直接按 slot 扫描 `RuntimeRestStore` 与 runtime 的 `AttackExempt`，不再为 cooldown 对每个实体执行完整 `RefreshRuntimeSnapshot`。

正式默认已经晋升为 `DataOriented`。dormant、pending-destroy 和非活动对象沿用现有 pass membership 合同，不会被误处理。旧路径继续作为可显式启用的 oracle，没有形成双写。

## 2. 正确性与确定性证据

聚焦 EditMode：

- 初次 cooldown 聚焦：5/5 PASS，job `d4ba092d8d1d4eae952f8f1ffbc21042`；
- 默认晋升后的 cooldown、U3 ECS、lockstep checksum 交叉回归：19/19 PASS，job `e4ae2e6026cb4498b2b2db6977d7344d`；
- 覆盖默认模式与 reset 边界、ShadowCompare、普通/held state 1001、dormant/pending 跳过和 Extended1000 预热后 0 B。

完整 `BattleRuntimeSelfCheck` 在默认 `DataOriented` 模式下于 `2026-08-11 14:16:27 +08:00` fresh PASS。

Authority400 full trace：

- Unity trace：`Temp/NTSDParity/u4-cooldown-data-unity-authority-dat-diagnostic.jsonl`；
- compare：`Temp/NTSDParity/u4-cooldown-data-compare-authority-dat-diagnostic.json`；
- 6/6 tick 为 `equal-diagnostic`，`firstDifference=null`；比较器验证 input、RNG、metadata、world、400 slots、ARest、VRest、stats、events 与 overall 全域正文和 hash。

该 trace 使用用户已确认的 Unity DAT 适配边界下的 `authority-dat-diagnostic` 夹具，因此不是 production certificate，也没有把 DAT 部署差异写成战斗逻辑问题。

## 3. 1000 AI 同配置 A/B

两轮均为 1000 个真实生产 GameObject/逻辑实体、Combat1000、全 AI、30 warmup + 180 sampled ticks、`maxCatchUp=1`、相同 seed/roster/workload/implementation fingerprint、详细 phase timing 和零 GC 门禁。

| 指标 | Legacy A | DataOriented B | 变化 |
|---|---:|---:|---:|
| Cooldown 平均 | 0.972822 ms | 0.394749 ms | -59.42% |
| Cooldown P95 | 1.473430 ms | 0.500555 ms | -66.03% |
| 整体逻辑 tick 平均 | 25.789231 ms | 23.854566 ms | -7.50% |
| 整体逻辑 tick P95 | 33.104205 ms | 29.572680 ms | -10.67% |
| 整体逻辑 tick 最大值 | 50.886900 ms | 36.326400 ms | 观察值，不作为晋升阈值 |
| sampled logic GC | 0 B/tick | 0 B/tick | 均通过 |

报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.u4-cooldown-legacy-a-20260811.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u4-cooldown-data-b-20260811.json`。

A/B 的 input、RNG、metadata、world、slots、ARest、VRest、stats、events 和 overall 最终 lockstep hash 全部逐域相同；overall 均为 `a13929d82b19c54e871522a1921f658ddfa88a7e7bc8655149d76006e1c504e1`。两轮均通过 cleanup，inactive pool 的保留容量属于复用缓存，不是假装回到初始容量。

## 4. 阶段结论与下一步

cooldown 的性能收益超过计划规定的 P95 10% 默认晋升门槛，并同时满足逐域确定性、零 GC、self-check 和权威诊断 trace，因此该子项已完成。

U4 仍在进行。下一切片是基础 frame/motion/bounds：先闭合权威 C# 的调用者、被调用者、字段写入和 pass 可见边界，再选择一个最小 canonical writer 迁移；在证据完成前不会一次性替换整个 frame loop。
