# U4 Character Stage-Z 迁移评估（2026-08-11）

> 对应总计划：`unified-battle-lockstep-ecs-server-architecture-plan.md`  
> 阶段：U4 第二个受控切片  
> 结论：数据化实现的行为验证通过，但性能未达到默认晋升门槛；正式默认保持 `Legacy`，U4 继续进行。

## 1. 权威合同

唯一权威入口是：

- `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Simulation\GameTick.cs:107`
- `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Simulation\GameTick.cs:114`
- `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Simulation\GameTick.cs:1961`

`ClampCharactersToStageZ` 每个逻辑 tick 执行两次：第一次在 post-frame-advance 后、碰撞候选收集前，第二次在持有链接校验后、held-weapon step 12 前。它按 runtime slot 顺序扫描，只处理活动角色，将 `Z` 钳制到舞台 `zmin/zmax`，随后以 C# `(int)` 截断写入 `ZInt`。

Unity 的受控迁移实现位于：

- `Assets/NTSD/Scripts/Simulation/Ecs/BattleEcsCharacterStageZPass.cs`
- `Assets/NTSD/Scripts/Simulation/SimulationWorld.cs`
- `Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs`

它提供 `Legacy`、`ShadowCompare`、`DataOriented` 三种仅允许在 reset 边界切换的模式。Data 路径保持固定 slot 升序、活动 membership、角色筛选、`Z/ZInt` 写入和零热路径分配。为了保持现有 Unity runtime 同步合同，正式 `LF2Character` 使用非虚的基类 runtime 同步快路径；派生或自定义角色继续调用虚 `RefreshRuntimeSnapshot`，避免吞掉它们的额外字段副作用。

## 2. 验证中发现并修复的回归

第一次默认晋升后的交叉测试暴露了 slot 复用回归：替换角色在第二次钳制前修改的 `Team/HP/Frame` 没有同步到 runtime，`Runtime.Frame` 实际为 `0`，预期为 `77`。原因是最初 Data 路径误把“权威方法只写 Z/ZInt”扩大解释成“可以删除 Unity 旧路径承担的完整 runtime 同步”。

修复后，`LF2Entity.RefreshRuntimeFromEntity` 的基类字段写入被抽取为可直接调用的非虚基类同步方法：

- 精确类型 `LF2Character` 保留全部既有 runtime 字段同步，但不产生虚派发；
- 未知派生角色仍走原虚方法；
- slot 复用、重复 pass、dormant/pending 跳过和 Z 截断语义恢复一致。

这项修复不是放宽测试，而是补回 Data 路径遗漏的既有可观察合同。

## 3. 正确性证据

最终默认回退到 `Legacy` 后的交叉 EditMode job：

- job：`843fde88586f49e8a065e7b354255b6a`
- 结果：28/28 PASS
- 覆盖：Stage-Z Data/Shadow、旧 StageBounds runtime 同步、cooldown canonical writer、U3 ECS read-only shadow、lockstep checksum，以及 `Extended1000` 预热后 0 B。

完整 `BattleRuntimeSelfCheck`：

- `2026-08-11 15:00:21 +08:00` fresh PASS。

Data 模式 Authority400 full trace：

- Unity trace：`Temp/NTSDParity/u4-stagez-data-unity-authority-dat-diagnostic.jsonl`
- compare：`Temp/NTSDParity/u4-stagez-data-compare-authority-dat-diagnostic.json`
- 结果：6/6 tick `equal-diagnostic`，`firstDifference=null`。

该 trace 比较 full detail 的 input、RNG、metadata、world、400 slots、ARest、VRest、stats、events 和 overall。它使用用户确认过的 Unity DAT 适配边界下的 `authority-dat-diagnostic` 夹具，因此是诊断等价证据，不是 production certificate。

最终构建：

- `dotnet build Assembly-CSharp.csproj --no-restore`：0 error；
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore`：0 error。

现有依赖版本冲突和 nullable 等警告仍存在，但本切片没有新增编译错误。

## 4. 1000 AI 同配置 A/B

两轮均为 1000 个真实生产 GameObject/逻辑实体、Combat1000、全 AI、30 warmup + 180 sampled ticks、`maxCatchUp=1`、同 seed、同 roster/workload、完整表现、phase timing 和零 GC 硬门禁。

报告：

- Legacy A：`Temp/NTSD_ProductionEntityStress.combat1000.u4-stagez-legacy-a-20260811.json`
- Data B：`Temp/NTSD_ProductionEntityStress.combat1000.u4-stagez-data-b-20260811.json`

| 指标 | Legacy A | DataOriented B | 变化 |
|---|---:|---:|---:|
| StageBounds 平均 | 1.288447 ms | 1.252098 ms | -2.82% |
| StageBounds P95 | 1.657970 ms | 1.580670 ms | -4.66% |
| 整体逻辑 tick 平均 | 25.189040 ms | 24.749953 ms | -1.74% |
| 整体逻辑 tick P95 | 32.368390 ms | 29.642245 ms | -8.42% |
| 整体逻辑 tick 最大值 | 35.050500 ms | 35.978500 ms | 观察值；Data 未改善最大值 |
| sampled logic GC | 0 B/tick | 0 B/tick | 均通过 |

A/B 的 input、RNG、metadata、world、slots、ARest、VRest、stats、events 和 overall 最终 lockstep hash 全部逐域相同；overall 均为 `a13929d82b19c54e871522a1921f658ddfa88a7e7bc8655149d76006e1c504e1`。两轮 `StoppedCleanly`，cleanup/driver/logging 状态均恢复。

## 5. 决策

Data 路径已经证明行为等价、零 GC 且有小幅收益，但目标 pass 的 P95 只改善 `4.66%`，未达到总计划规定的 `10%` 默认晋升门槛。因此：

- `BattleEcsCharacterStageZPass` 正式默认保持 `Legacy`；
- `DataOriented` 和 `ShadowCompare` 保留为后续组合迁移、诊断及更大切片复用的受控路径；
- 不把本切片标为“已晋升”或把 U4 标为完成；
- 下一步继续选择基础 frame/motion/bounds 中更有收益、字段写入更闭合的 canonical writer，而不是为了形式上的 ECS 化强行替换低收益路径。

