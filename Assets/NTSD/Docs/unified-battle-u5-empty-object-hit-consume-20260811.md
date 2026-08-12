# U5 ObjectHitConsume 空候选快速路径报告

> 日期：2026-08-11  
> 状态：本切片已完成并晋升生产默认；U5 整体仍在执行  
> 范围：只关闭可证明为空的 object hit 消费，不代表真实 object hit writer 或 U5 整体完成

## 1. 权威边界

唯一战斗逻辑权威仍为 `J:\QQFile\NTSD2.4\ntsd_release_C#`。

本切片核验的权威调用链为：

- `src/BattleCore/Simulation/GameTick.cs`：candidate collect、character hit、random weapon drop、object hit 与 candidate consumption end 的顺序；
- `src/BattleCore/Interaction/HitResolve.cs`：`ResolveLoop1/ResolveLoop2` 在消费时读取当前 DAT 类型，不能使用候选收集时缓存的旧类型；
- Unity 对应入口：`SimulationWorld.ObjectInteractionTickAll(int)`。

优化没有移动 pass，没有提前结束 candidate consumption，也没有改变 object hit 存在时的 slot 顺序、候选顺序、opoint flush 或快照刷新。

## 2. 实现合同

`SimulationWorld.ObjectInteractionTickAll` 只在以下条件全部成立时跳过整个对象式消费循环：

1. 当前 object-phase 参与者都是精确的生产 CLR 类型：`LF2Character`、`LF2SpecialAttack`、`LF2Weapon` 或 `LF2OtherObject`；
2. 当前 scene query 的候选快照仍可读；
3. 当前 generation 的每个非空攻击者候选行都能解析到活动实体；
4. 按消费时的当前 DAT 类型判断，所有非空行都只属于 character hit，不存在 object hit 候选。

以下情况全部 fail closed 到原权威对象路径：

- 派生、测试或自定义实体类型，避免跳过虚方法副作用；
- 候选快照不可用或 StoreAuthority 已 fail closed；
- runtime slot 已复用且 generation 不一致；
- 候选收集后 DAT 动态变为 object 类型；
- 任意 object hit 候选存在；
- `ForceLegacyEmptyObjectHitConsumeForDiagnostics` 强制使用旧路径。

`CollisionCandidateStore` 只额外维护固定容量的非空攻击者 slot 索引。索引在行计数从 0 变为 1 时记录，并在每次 build 开始时复用；稳定战斗窗口不分配托管内存。

## 3. 聚焦测试

新增 `BattleEcsEmptyObjectHitConsumeEditorTests`，覆盖：

1. Legacy 与 StoreAuthority 下，character 候选行不会阻止空 object pass 证明；
2. 候选收集后 DAT 动态变为 object 类型时必须执行 object hit；
3. generation/handle 使用当前活动实体，不继承旧 slot 行；
4. 候选快照不可用时保留派生类型虚调用；
5. 强制 Legacy 开关禁用证明；
6. 预热后的证明路径为 `0 B GC.Alloc`。

最终聚焦任务 `2ecc844b9df044909a2648ac00027b98`：`7/7 passed`。

压力工具回归任务 `72aa1882630948f09f6ac82c9c1b4790`：`233/233 passed`。

## 4. 1000 AI 相邻 A/B

四轮样本使用完全相同的正式配置：

- seed：`1314149188`；
- `Combat1000`，1000 个真实生产 AI；
- `DataOrientedCanonical`；
- 30 warmup + 180 sample；
- 每个 Unity `Update` 最多 1 个逻辑 tick；
- role-aware 正式收集器；
- phase、presentation、detail timing 开启；
- 正式逻辑 tick `0 B`；
- 最终 lockstep overall hash 均为 `a13929d82b19c54e871522a1921f658ddfa88a7e7bc8655149d76006e1c504e1`。

| 指标 | Legacy A | Fast B | Legacy C | Fast D |
|---|---:|---:|---:|---:|
| Logic tick average | 25.347824 ms | 26.199907 ms | 23.217024 ms | 23.620994 ms |
| Logic tick P95 | 39.358850 ms | 33.538600 ms | 28.087560 ms | 29.192885 ms |
| ObjectHitConsume average | 0.231917 ms | 0.165561 ms | 0.222246 ms | 0.162353 ms |
| ObjectHitConsume P95 | 0.315020 ms | 0.219815 ms | 0.251320 ms | 0.220825 ms |
| 空 object pass 跳过次数 | 0 | 210 | 0 | 210 |
| object 参与者实际执行次数 | 0 | 0 | 0 | 0 |
| 正式 tick GC | 0 B | 0 B | 0 B | 0 B |
| teardown restored | true | true | true | true |

两组均值合并后：

- ObjectHitConsume average：`0.227081 -> 0.163957 ms`，改善 `27.80%`；
- ObjectHitConsume P95：`0.283170 -> 0.220320 ms`，改善 `22.20%`；
- Logic tick average：`24.282424 -> 24.910450 ms`，波动为 `-2.59%`；
- Logic tick P95 在两组间方向不一致，不能宣称稳定总体收益。

报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.u5-empty-object-hit-legacy-a.json`
- `Temp/NTSD_ProductionEntityStress.combat1000.u5-empty-object-hit-fast-b.json`
- `Temp/NTSD_ProductionEntityStress.combat1000.u5-empty-object-hit-legacy-c.json`
- `Temp/NTSD_ProductionEntityStress.combat1000.u5-empty-object-hit-fast-d.json`

## 5. 结论

两组相邻 A/B 都证明目标 pass 获得约 22%～28% 的稳定局部改善，同时保持当前 DAT 语义、slot generation、防派生副作用、零 GC、最终逻辑 hash 和 teardown。该快速路径因此保留为生产默认，强制 Legacy 开关作为诊断 oracle。

本切片绝不声明 1000 AI 整体帧率已经提升：ObjectHit 在该 workload 只有约 `0.2 ms/tick`，整 tick 指标仍主要由 CharacterInput、CandidateCollect、LateEntityUpdate、RenderDispatch 等更大阶段决定。

本切片也没有迁移真实 object hit writer。存在 object 候选或无法证明安全时仍执行原对象路径。后续 U5 继续处理 cpoint/held/link、真实命中 writer、opoint 与结构生命周期。

## 6. 本批最终复核

- `dotnet build Assembly-CSharp.csproj --no-restore -v:minimal`：`0 error`；18 个既有程序集版本冲突 warning；
- Unity `all/force` refresh 完成，Console C# error：0；
- 压力工具完整回归 `72aa1882630948f09f6ac82c9c1b4790`：`233/233 passed`；
- ObjectHit + role-aware collision 聚焦回归 `d6afdde5616349d8a9927c4431c67929`：`72/72 passed`；
- `BattleRuntimeSelfCheck`：2026-08-11 22:33 `PASS`；
- 四份正式 A/B 报告均为 `StoppedCleanly`、`zeroGcGatePassed=true`、`teardown.restored=true`；
- stress 与 self-check 请求文件均已由处理器正常消费，无遗留运行请求。
