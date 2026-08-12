# U0 工作树与权威基线封套（2026-08-11）

> 对应总计划：`unified-battle-lockstep-ecs-server-architecture-plan.md`
> 阶段：U0
> 结论：迁移前 oracle 已固定且可重复；允许进入 U1，但不代表 U1～U9 或 1000 AI / 30 FPS 已完成。

## 1. 工作树封套

- 分支：`feat/dat-skill-flow-editor`
- 基线提交：`5cc92d63c09e41f7e879237bb75863070f027fbc`（`111`）
- 审计开始时没有未提交的 NTSD runtime 脚本修改；已有未提交内容是架构文档、旧计划标记和用户本地 `.claude/settings.local.json`，均按用户工作保留。
- U0 新增 `Assets/NTSD/Scripts/AssemblyInfo.cs`，只向 `Assembly-CSharp-Editor` 开放 internal 测试缝，不把 runtime API 改成 public。
- U0 修正三类已落后于生产结构的测试夹具：对象池 `Queue<GameObject>`、跨世界 mutation tracker 不做引用同一性比较、AI 队伍分区溢出验证正式回退与权威结果一致。

## 2. 候选实现分类

| 领域 | U0 分类 | 当前证据 | 后续阶段门 |
|---|---|---|---|
| Canonical Input / Host Policy | 候选、聚焦测试已通过 | `LocalFrameInputProviderEditorTests`、strict buffer、session、journal、checksum 共 26 项通过 | U1 仍需统一入口、同 journal 重放和单机单 tick 正式验收 |
| 中央表现与 latest-frame materialization | 候选、压力夹具可运行 | Combat1000 使用 `CentralOnly`，正式逻辑、driver、presentation 和 player-loop 守卫均为 0 B | U2 仍需把表现物化从逻辑 tick 边界正式迁出并证明 checksum 不变 |
| Data-Oriented AI | 候选、正向性能结果 | 两次 Combat1000 使用 `data-oriented-canonical`，最终 lockstep hash 一致 | U5/U6 前不能删除 Legacy oracle 或宣称 canonical writer 已迁移 |
| Role-aware collision | 候选、正式负载已覆盖 | Combat1000 生效模式为 `role`；两轮 pair/candidate peak 都是 `38,831/602` | U5 仍需完整 shadow/parity 与复杂度门禁 |
| 零 GC | 当前夹具已通过、全局门未关闭 | 两轮 180 sampled ticks 的 logic/driver/presentation/player-loop 都为 0 B，且无 collection violation | U8 仍需覆盖正式 Dispersed1000、Combat1000、生命周期和表现事件矩阵 |
| 旧实现/不安全路径 | 保留 oracle 或禁用 | Legacy 输入/AI/碰撞路径继续作为 A/B oracle；unsafe AI SoA candidate 在本基线中未启用 | 只有对应阶段 parity 和回退门通过后才允许删除 |
| 用户文档与本地配置 | 用户工作 | 未覆盖、未回退、未纳入 runtime 判断 | 持续保留 |

## 3. Authority400 固定夹具

使用 `Tools/NTSDParity/scenario.sample.json`：

- seed：`305419896`；
- mode/difficulty：`1/1`；
- roster：slot 0=`oid 2/team 1`，slot 1=`oid 1/team 2`；
- 输入 journal：6 tick，tick 2～3 左键保持，tick 4 释放；
- profile：Unity 与权威导出器均使用固定 400-slot Authority400 trace；
- T8：没有加载默认 `stage.dat`。

`dotnet build Tools/NTSDParity/NTSDParity.csproj --no-restore`：0 warning、0 error。

生成证据：

- 权威 full trace：`Temp/NTSDParity/u0-authority400-authority.jsonl`
- Unity production full trace：`Temp/NTSDParity/u0-authority400-unity.jsonl`
- Unity authority-DAT diagnostic full trace：`Temp/NTSDParity/u0-authority400-unity-authority-dat-diagnostic.jsonl`
- production compare：`Temp/NTSDParity/u0-authority400-compare-strict.json`
- diagnostic compare：`Temp/NTSDParity/u0-authority400-compare-authority-dat-diagnostic.json`

结果必须分两层解释：

1. Production compare 在 tick 0 因 battle-logic DAT manifest 不同而停止：C#=`41c088d2...0375`，Unity=`6b34e118...332a`。这是已确认的 Unity DAT 适配前置差异，不能写成逐 tick 逻辑通过。
2. Unity 改用只用于诊断的 authority DAT 后，full/full、`fixed-world-camera` 比较为 `equal-diagnostic`，6/6 tick 的 input、RNG、world、400 slots、ARest、VRest、stats、events 和 overall hash 全部相等。该结果不是 production certificate，也不替代 Unity DAT 适配资产边界。

## 4. Unity 编译与自动验证

- fresh Unity script compile：0 error。
- 输入/lockstep/回放/checksum 聚焦测试：26/26 PASS，job `7536f866186e4494ad7d96640108978d`。
- U0 失败修复聚焦回归：5/5 PASS，job `8ef9304a34134c02a670df8e76c42f10`。
- 完整 EditMode：799 项全部执行；798 项通过，1 项被 UnityMCP 自身的 disposed `NetworkStream` Error log 污染。该项随后独立复跑 1/1 PASS，job `797cdd44762549a490a08bcf9a43a41b`。因此代码断言没有遗留失败，但没有把受工具日志污染的整批运行伪报为 799/799 单次全绿。
- `BattleRuntimeSelfCheck`：PASS；结果文件 `Temp/NTSD_BattleRuntimeSelfCheck.result`。

## 5. Combat1000 可重复性能基线

固定配置：

- 1000 个真实生产 GameObject/逻辑实体；
- `MobileExtended`，1050 slots；
- 全 AI、正式 DAT、碰撞、命中、opoint、生命周期、声音和 `CentralOnly` 表现开启；
- seed=`0x4E545344`；
- `data-oriented-canonical` AI；
- role-aware formal collector；
- 30 warmup ticks + 180 sampled ticks；
- 同一 roster/workload/config 指纹。

| 运行 | Avg logic | P95 logic | Max logic | P95 Unity frame | sampled GC | final lockstep hash |
|---|---:|---:|---:|---:|---:|---|
| 第 1 轮 | 26.882 ms | 32.053 ms | 36.455 ms | 49.573 ms | 0 B/tick | `a13929d8...04e1` |
| 第 2 轮 | 27.078 ms | 32.297 ms | 35.609 ms | 48.092 ms | 0 B/tick | `a13929d8...04e1` |

两轮共同结果：

- roster fingerprint=`a905f121...8a49`；
- workload fingerprint=`78a2ecae...4bbd`；
- implementation config fingerprint=`a9f73075...5af3`；
- pair peak=`38,831`，candidate peak=`602`；
- logic、driver update、presentation 和 player-loop envelope 均为 0 B；
- 没有 Gen0/1/2 collection violation；
- teardown `restored=true`，active GameObject/world entity/claimed slot/pool active 全部恢复为 0；
- 当前保留的 1000 inactive pool capacity 只是预热缓存，不是活动实体泄漏。

第二轮报告：`Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-capacity-pressure-smoke.json`。

逻辑 tick P95 两轮都低于 33.33 ms，但可见 Unity frame P95 仍显著超过 33.33 ms，而且这只是 180-tick Editor smoke，不是 U9 的正式持续门禁。因此 U0 只固定基线，不关闭 1000 AI / 30 FPS 目标。

## 6. U0 阶段结论

U0 完成门已满足：工作树所有权已固定，Authority400 oracle、seed/roster/input journal、测试口径和 1000 AI 性能负载均可重复。已知限制被显式保留：production DAT manifest 前置差异、UnityMCP 完整测试日志污染、Unity frame P95 未达标。

下一阶段进入 U1，只晋升现有 Canonical Input 与 Host Policy 候选实现，不另写第二套输入或网络业务。
