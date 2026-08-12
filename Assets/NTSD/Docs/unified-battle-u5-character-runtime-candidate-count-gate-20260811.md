# U5 CharacterHit 运行时候选计数门控评估（2026-08-11）

> 对应总计划：`unified-battle-lockstep-ecs-server-architecture-plan.md`
> 结论：候选门控行为等价但属于负优化；生产默认继续使用已晋升的 Legacy candidate range 空候选证明，运行时计数门控仅保留为诊断候选。

## 1. 评估目的与边界

此前 `CharacterHitConsume` 的精确 `LF2Character` 空候选快速路径已经晋升生产默认。该路径仍会为每个角色调用一次 `TryGetCollisionCandidateRange`，本次实验尝试在同一 tick 先证明运行时 `HitCandidateCount` 与正式 Legacy 候选缓存一致，然后对角色直接读取整数计数，减少逐角色字典范围查询。

该实验不修改候选 writer、候选顺序、命中 resolver、派生角色虚调用或 pass 顺序。以下情况全部 fail closed 到原 range proof 或完整权威对象路径：

- 正式候选集合未发布、已中止或来源不是 `LegacyOracle`；
- 候选行的 slot/generation 已失效；
- 任意缓存行与运行时计数不一致；
- 全部当前 active runtime 的计数总量与缓存总量不一致；
- 派生角色、过期 runtime 快照或显式关闭门控。

## 2. 正确性与零分配验证

- `dotnet build Assembly-CSharp.csproj --no-restore -v:minimal`：0 error；
- 聚焦测试：7/7 PASS，覆盖正常空候选、候选源不可用、过期快照、计数篡改 fail closed、显式关闭、派生虚调用和预热后 `0 B`；
- 压力工具、W07 结构见证与聚焦测试联合回归：244/244 PASS，job `88e922627bc04b99856988054953b351`；
- `BattleRuntimeSelfCheck`：2026-08-11 23:49:56 fresh PASS。

## 3. 1000 AI 隔离 A/B

两组均为真实 1000 GameObject/逻辑实体、Combat1000、全 AI、`DataOrientedCanonical` AI、role collector、30 warmup + 180 sample、每 Unity Update 最多一个逻辑 tick、相同 seed、phase/detail timing、正式零 GC 门禁和最终 parity snapshot。唯一变量是 character runtime candidate-count gate。

| 指标 | Range Legacy A | Runtime Gate B | B 相对 A |
|---|---:|---:|---:|
| CharacterHitConsume average | 0.632243 ms | 0.713778 ms | 慢 12.90% |
| CharacterHitConsume P95 | 1.215255 ms | 1.272535 ms | 慢 4.71% |
| CharacterHitConsume max | 1.887700 ms | 1.708700 ms | 快 9.48% |
| 整体逻辑 tick average | 25.088918 ms | 29.586584 ms | 慢 17.93% |
| 整体逻辑 tick P95 | 36.166375 ms | 49.183535 ms | 慢 35.99% |
| sampled logic GC | 0 B/tick | 0 B/tick | 一致 |

候选组在全部 210 个 warmup/sample tick 上成功应用门控且无回退；两组最终 input、RNG、metadata、world、slots、aRest、vRest、stats、events 与 overall hash 全部一致，overall 均为 `b8a07be2e5ed9e94f150f4b6e0e426e6e8d23630c69e5fe05a39636e63707821`。两组 harness validity、cleanup 与状态恢复全部通过。

报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.u5-char-runtime-count-range-a-20260811.json`
- `Temp/NTSD_ProductionEntityStress.combat1000.u5-char-runtime-count-gate-b-20260811.json`

## 4. 决策

为了让运行时整数成为可证明的读取源，门控必须额外扫描当前 runtime slots，并校验缓存总量。该 O(N) 证明成本高于原来逐角色的 range lookup，目标 pass 已明确退化，因此不满足晋升门槛，也无需为负向候选追加三轮性能复测。

- `ForceLegacyCharacterRuntimeCandidateCountGateForDiagnostics` 生产初始值保持 `true`；
- 已晋升的 character 空候选 range proof 保持不变；
- runtime count gate 仅供隔离诊断，不作为 canonical writer 或默认读取路径；
- U5 继续处理真实存在候选时的 hit、cpoint/held/link、opoint 与结构生命周期。
