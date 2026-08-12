# U4 FramePostProcess 迁移评估（2026-08-11）

> 对应总计划：`unified-battle-lockstep-ecs-server-architecture-plan.md`  
> 阶段：U4 第三个受控切片  
> 结论：数据路径的行为、确定性和零分配验证通过，但 P95 性能门槛失败；正式默认保持 `Legacy`，U4 继续进行。

## 1. 权威合同

唯一战斗逻辑权威为：

- `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Simulation\GameTick.cs`
- `GameTick.ApplyFramePostProcess`

该 pass 按 runtime slot 升序处理活动实体，并跳过 `FrameDelay != 0` 的实体。对其余实体：

- `HitCount > 0` 时，以 `HitCount + 1.0` 为除数，将三个累计击退分量的两倍写回 `Vx/Vy/Vz`，随后清零 `HitCount`；
- `HitCount <= 0` 时不改写速度，且负数 `HitCount` 必须保留；
- 无论是否命中，三个累计击退分量都在本 pass 结束时清零。

本轮核验发现 Unity 旧实现会无条件把负数 `HitCount` 写成零，与权威 C# 不同。该差异已在 Legacy 与 Data 两条路径中统一修正，并新增聚焦测试，避免把“性能迁移”建立在错误的旧行为上。

## 2. 受控实现

新增 `BattleEcsFramePostProcessPass`，提供仅允许在 reset 边界切换的三种模式：

- `Legacy`：调用修正后的现有 writer；
- `ShadowCompare`：先计算预期结果，再运行 Legacy 并逐字段核验；
- `DataOriented`：按固定 slot 容量顺序直接写入 runtime-backed 的 `Vx/Vy/Vz`、`HitCount` 与 `KnockbackVx/Vy/Vz`。

Data 路径不执行结构变更、不改变 entity membership、不调用 Unity API，也不进行热路径托管分配。诊断数组和 bitset 在 world 建立时预分配。

相关实现：

- `Assets/NTSD/Scripts/Simulation/Ecs/BattleEcsFramePostProcessPass.cs`
- `Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs`
- `Assets/NTSD/Scripts/Simulation/SimulationWorld.cs`
- `Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs`
- `Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs`
- `Assets/NTSD/Scripts/Test/Editor/BattleEcsFramePostProcessPassEditorTests.cs`

## 3. 正确性与确定性证据

聚焦测试覆盖：

- 默认模式与 reset 边界；
- 权威负数 `HitCount` 保留合同；
- ShadowCompare 逐字段一致；
- Data 与 Legacy 的 active/delayed/pending/dormant 分支一致；
- `Extended1000` 预热后循环为 0 B。

默认曾临时切换到 Data 用于交叉回归，EditMode job：

- job：`f7acd25de8014e03a0a55edd2e85252e`
- 结果：33/33 PASS
- 覆盖：FramePostProcess、Stage-Z、StageBounds、cooldown、U3 ECS shadow 与 lockstep checksum。

Data 模式 Authority400 full trace：

- Unity trace：`Temp/NTSDParity/u4-framepost-data-unity-authority-dat-diagnostic.jsonl`
- compare：`Temp/NTSDParity/u4-framepost-data-compare-authority-dat-diagnostic.json`
- 结果：6/6 tick `equal-diagnostic`，`firstDifference=null`。

该 trace 使用用户确认过的 Unity DAT 适配边界下的 `authority-dat-diagnostic` 夹具，因此是诊断等价证据，不是 production certificate。

最终默认恢复为 `Legacy` 后的新鲜验收：

- 指定 NTSD Unity 实例的 EditMode job：`287f7d2024184ffa856b89d619645b26`，33/33 PASS；
- `BattleRuntimeSelfCheck`：`2026-08-11 15:43:38 +08:00` fresh PASS；
- `dotnet build Assembly-CSharp.csproj --no-restore --nologo`：0 error；
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo`：0 error。

工程现有依赖版本冲突和序列化字段警告仍存在，但本切片没有新增编译错误。

## 4. 1000 AI 同配置 A/B

两轮均使用 1000 个真实生产 GameObject/逻辑实体、全 AI、30 warmup + 180 sampled ticks、`maxCatchUp=1`、相同 seed、相同 roster/workload、完整表现、phase timing 与零 GC 硬门禁。

报告：

- Legacy A：`Temp/NTSD_ProductionEntityStress.combat1000.u4-framepost-legacy-a-20260811.json`
- Data B：`Temp/NTSD_ProductionEntityStress.combat1000.u4-framepost-data-b-20260811.json`

| 指标 | Legacy A | DataOriented B | 变化 |
|---|---:|---:|---:|
| FramePostProcess 平均 | 0.525986 ms | 0.329144 ms | 改善 37.42% |
| FramePostProcess P95 | 0.650220 ms | 1.008825 ms | 恶化 55.15% |
| 整体逻辑 tick 平均 | 24.526253 ms | 48.416224 ms | B 轮系统性变慢 |
| 整体逻辑 tick P95 | 29.872105 ms | 86.082635 ms | B 轮系统性变慢 |
| 整体逻辑 tick 最大值 | 40.145700 ms | 103.570500 ms | B 轮系统性变慢 |
| sampled logic GC | 0 B/tick | 0 B/tick | 均通过 |

两轮 roster fingerprint 与 workload fingerprint 相同；input、RNG、metadata、world、slots、ARest、VRest、stats、events 和 overall 十个最终 lockstep hash 全部相同，overall 均为 `a13929d82b19c54e871522a1921f658ddfa88a7e7bc8655149d76006e1c504e1`。两轮均 `StoppedCleanly`，cleanup 状态恢复。

这组数据不能证明 Data 路径导致了整轮系统性变慢，但足以证明本轮没有获得稳定的尾延迟收益。按既定规则，不能只取平均值改善而忽略 P95 恶化。

## 5. 决策

目标 pass 的 P95 没有达到“至少改善 10%”的默认晋升门槛，反而恶化 55.15%。因此：

- 正式默认恢复并保持 `Legacy`；
- `DataOriented` 与 `ShadowCompare` 保留为可控诊断路径；
- 本切片状态为“语义验证完成，性能评估关闭，未晋升”；
- 不把 FramePostProcess 标为 U4 已迁移默认，也不把 U4 整体标为完成；
- 后续优先选择可减少对象/虚调用/重复快照或合并多次全槽扫描的更高收益切片。
