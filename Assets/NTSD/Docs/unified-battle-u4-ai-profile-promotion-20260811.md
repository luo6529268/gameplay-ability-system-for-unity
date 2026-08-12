# U4 AI 数据化执行配置晋升验收（2026-08-11）

> 对应总计划：`unified-battle-lockstep-ecs-server-architecture-plan.md`  
> 阶段：U4 第四个受控切片  
> 结论：现有数据化 AI 感知与决策执行链通过同配置 1000 AI A/B、零 GC、确定性和回归门禁，生产默认已从 `LegacyCanonical` 晋升为 `DataOrientedCanonical`；显式 Legacy 回退仍保留，U4 整体仍未完成。

## 1. 权威合同与本轮边界

AI 战斗规则的唯一权威入口是：

- `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Input\InputRuntime.cs`
- `InputRuntime.PrepareAiInputBasic`

该调用链决定 AI 的输入历史滚动、难度派生值、移动模式、最近目标、特殊对象扫描、缓存目标、RNG 次数与顺序、按键写入和输入边沿。数据化路径只能改变读取布局与候选索引，不能改变：

- runtime slot 升序和目标 tie-break；
- `NtsdRng.Rand()` 的调用次数、顺序或条件；
- cached target、特殊 OID、team、state、frame 与距离分支；
- 最终 held/pressed/released 输入和后续 `ApplyCharacterInput` 可观察结果。

本轮没有重写 AI 规则，也没有新增 `partial` 文件或全局可变 static。它只把已经存在并经过 shadow/oracle 验证的数据化执行配置正式接到生产默认解析器：

- `BattleAiExecutionProfileResolver.Resolve` 的无配置默认值改为 `DataOrientedCanonical`；
- 命令行或 `GameConfig` 显式指定 `legacy` 时仍使用 `LegacyCanonical`；
- 直接构造、未经过生产配置接线的 `SimulationWorld` 仍可保持 Legacy，供聚焦测试和 oracle 使用；
- profile 只在启动/reset 边界解析，不在 tick 内切换。

主要现有实现：

- `Assets/NTSD/Scripts/Simulation/BattleAiExecutionProfile.cs`
- `Assets/NTSD/Scripts/Simulation/SimulationWorld.AiInput.partial.cs`
- `Assets/NTSD/Scripts/Simulation/SimulationWorld.AiSoaShadow.partial.cs`
- `Assets/NTSD/Scripts/Simulation/SimulationWorld.AiDecisionShadow.partial.cs`
- `Assets/NTSD/Scripts/Simulation/Ai/AiDecisionKernel.cs`

旧 `.partial.cs` 文件属于本轮开始前的既有实现；本轮没有扩大该结构。后续 U6 仍按总计划把对象式和旧 partial 热循环逐步收敛到组合式 BattleKernel/System。

## 2. 1000 AI 严格同配置 A/B

两轮均为 1000 个真实生产 GameObject/逻辑实体、`Combat1000`、全 AI、30 warmup + 180 sampled ticks、`maxCatchUp=1`、seed `1314149188`、完整表现、phase timing 与零 GC 硬门禁。两轮 roster 和 workload fingerprint 完全相同。

报告：

- Legacy A：`Temp/NTSD_ProductionEntityStress.combat1000.u4-ai-profile-legacy-a-20260811.json`
- Data B：`Temp/NTSD_ProductionEntityStress.combat1000.u4-ai-profile-data-b-20260811.json`

| 指标 | LegacyCanonical A | DataOrientedCanonical B | 变化 |
|---|---:|---:|---:|
| CharacterInput 平均 | 16.515596 ms | 6.486189 ms | 改善 60.73% |
| CharacterInput P95 | 26.035390 ms | 8.473325 ms | 改善 67.45% |
| 整体逻辑 tick 平均 | 35.991247 ms | 25.789926 ms | 改善 28.34% |
| 整体逻辑 tick P95 | 51.243370 ms | 35.315785 ms | 改善 31.08% |
| 整体逻辑 tick 最大值 | 56.889500 ms | 51.756600 ms | 观察值，不单独作为门槛 |
| sampled logic GC | 0 B/tick | 0 B/tick | 均通过 |

Data 轮额外满足：

- indexed canonical fallback：0；
- unified snapshot post-commit hard breach：0；
- authority success：`true`；
- cleanup、driver、logging 与活动实体状态均恢复；
- 状态为 `StoppedCleanly`。

两轮最终 input、RNG、metadata、world、slots、ARest、VRest、stats、events 和 overall 十个 lockstep 域全部相同；overall 均为：

`a13929d82b19c54e871522a1921f658ddfa88a7e7bc8655149d76006e1c504e1`

因此本轮观测到的收益不是通过 AI 降频、跳过 tick、删减目标、减少 RNG、关闭碰撞或关闭表现获得。

## 3. 编译、测试与自检

生产默认改为 Data 后的新鲜验证：

- Unity 指定 NTSD 实例完成脚本刷新与编译，0 error；
- AI/profile/SoA/decision/lockstep 聚焦 EditMode job `83a681c7e9ed4784a17cf0890fc35869`：185/185 PASS；
- `BattleRuntimeSelfCheck`：`2026-08-11 16:00:12 +08:00` fresh PASS；
- `dotnet build Assembly-CSharp.csproj --no-restore --nologo`：0 error；
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo`：0 error。

首次把两个 dotnet build 并发启动时，二者争用同一个 `obj/Debug/Assembly-CSharp.dll` 并产生一次工具层 `CS2012 Access denied`。关闭该次 MSBuild 节点并按项目规则串行重跑后，两项均为 0 error；这不是代码编译失败。工程既有依赖版本与未赋值序列化字段警告仍存在。

现有 Authority400 parity 场景的 slot 配置为 `ai:false`，不能证明 AI 决策链的跨运行时行为，所以本轮没有把该非 AI trace 冒充 AI 证据。AI 行为证据来自 1000 AI A/B 的十域 lockstep hash、零 fallback/零 hard breach、185 项聚焦测试和完整 self-check。未来增加 AI parity fixture 后仍需补充跨运行时逐 tick trace，但这不影响当前同一 Unity runtime 内的生产默认晋升结论。

## 4. 决策与下一步

CharacterInput 目标 pass 的平均和 P95 收益都显著超过总计划规定的 10% 晋升门槛，同时满足零 GC、确定性 hash、回归和 self-check，因此正式默认晋升为 `DataOrientedCanonical`。

这只关闭 U4 的 AI 感知/决策生产配置子项，不代表 U4 或 U0～U9 已完成。下一步继续按 U4 顺序处理 `CandidateCollect` 的 participant/broadphase/exact 热路径，随后处理 `LateEntityUpdate` 中无结构变化的数值段；每个切片继续执行 Legacy/Data A/B 和单 writer 门禁。
