# U5 PreInteraction 逐参与者精确过滤（2026-08-12）

> 对应总计划：`unified-battle-lockstep-ecs-server-architecture-plan.md`
> 结论：通过真实交互夹具、三轮 1000 AI A/B、零 GC 与十域 hash 验证，晋升为生产默认。

## 1. 权威边界

权威 C# `GameTick.cs` 的顺序为 `RunCPoint` → `SyncHeldWeapons` → positive-link validation。Unity `PreInteractionTickAll` 保留相同的三个升序阶段：kind1 cpoint、kind2 mismatch tail、held/cpoint sync。

此前 whole-pass no-op 证明只能在全体实体都中性时跳过整个 pass；只要一个实体处于 cpoint 或 held 状态，fallback 就会对 1000 个角色执行三轮虚调用。本切片不重写正式 writer，而是在 fallback 内对精确 `LF2Character` 分别证明单个阶段必然无副作用：

- kind1 check：collision frame 无 kind1 cpoint，或 `FrameDelay < 0`；
- kind2 mismatch tail：当前 frame 无 kind2 cpoint；
- held sync：当前 frame 不满足 kind1/Catching，且 link、target、held slot 与 managed reference 全部处于中性值。

派生角色、过期 runtime snapshot、非精确类型或任何非中性 link 全部执行原虚调用。阶段顺序、runtime slot 升序、真实写入、刷新与 deferred mutation 边界均不变。

## 2. 正确性与工具回归

- 聚焦测试 8/8 PASS；新增混合场景同时包含 kind1、kind2、正/负 stale-held 与 managed stale reference；
- 混合场景中 15 次阶段调用只跳过 10 次可证明 no-op，5 次真实写入全部保留；与强制 Legacy 的 runtime 字段、RNG 和 extended checksum 一致；
- 压力工具、W07 结构见证与聚焦测试联合回归 245/245 PASS，job `e5a0ddbfed1b49418a1a766602adc045`；
- `BattleRuntimeSelfCheck`：晋升生产默认后于 2026-08-12 00:45:56 fresh PASS；
- 热路径不引入 managed allocation。

## 3. 三轮 1000 AI A/B

每轮均为真实 1000 GameObject/逻辑实体、Combat1000、全 AI、`DataOrientedCanonical` AI、role collector、30 warmup + 180 sample、每 Unity Update 最多一个逻辑 tick、相同 seed、phase/detail timing、正式零 GC 门禁与最终 parity snapshot。第二轮按 B/A 顺序执行以抵消热机漂移。

| 轮次 | Legacy avg | Filter avg | avg 改善 | Legacy P95 | Filter P95 | P95 改善 |
|---|---:|---:|---:|---:|---:|---:|
| 1 | 1.399681 ms | 0.954997 ms | 31.77% | 2.297185 ms | 1.396175 ms | 39.22% |
| 2 | 1.430764 ms | 0.868530 ms | 39.30% | 2.403460 ms | 1.183925 ms | 50.74% |
| 3 | 1.680899 ms | 1.087311 ms | 35.31% | 3.197575 ms | 1.916060 ms | 40.08% |
| 三轮平均 | — | — | 35.46% | — | — | 43.35% |

整 tick average/P95 三轮平均改善 2.81%/4.64%。第一轮整 tick 有 2.10%/0.20% 反向波动，但第二、第三轮同方向改善，且目标 pass 三轮稳定超过 10% 晋升门槛。

候选轮在 210 个 tick 内把执行调用从 357,000 次降到 4,644 次，累计证明跳过 625,356 次；cpoint-check、mismatch-tail 与 held-sync 分别跳过 208,438、208,483、208,435 次。六份报告均为 `0 B/tick`、harness valid、cleanup/restoration 通过；最终十域 hash 全部一致，overall 均为 `b8a07be2e5ed9e94f150f4b6e0e426e6e8d23630c69e5fe05a39636e63707821`。

报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.u5-preinteraction-participant-legacy-a-20260812.json`
- `Temp/NTSD_ProductionEntityStress.combat1000.u5-preinteraction-participant-filter-b-20260812.json`
- `Temp/NTSD_ProductionEntityStress.combat1000.u5-preinteraction-participant-filter-b2-20260812.json`
- `Temp/NTSD_ProductionEntityStress.combat1000.u5-preinteraction-participant-legacy-a2-20260812.json`
- `Temp/NTSD_ProductionEntityStress.combat1000.u5-preinteraction-participant-legacy-a3-20260812.json`
- `Temp/NTSD_ProductionEntityStress.combat1000.u5-preinteraction-participant-filter-b3-20260812.json`

## 4. 晋升结论

- `ForceLegacyPreInteractionParticipantFilteringForDiagnostics` 生产初始值为 `false`；
- whole-pass no-op proof 仍先执行，失败后才进入逐 participant 精确过滤；
- Legacy 开关和独立报告字段继续保留，便于回归和后续真实 writer 迁移；
- 本切片只关闭对象式 no-op 热调用，不表示 cpoint/held/link 的 canonical writer 已经迁移到统一数据宿主。
