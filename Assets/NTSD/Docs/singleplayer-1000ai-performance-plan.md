# NTSD 单机战斗运行时与 1000 AI / 30 FPS 实施计划

> 状态：当前主计划  
> 起始日期：2026-08-08  
> 当前优先级：高于服务器、真实网络、联机追帧、断线恢复和 Android 真机验证
> **2026-08-20 R0 权威迁移**：本文的性能、GC、checksum 和 Unity 内部 A/B 数据可继续作为性能回归证据，但不证明 C++ release 行为对齐。任何优化默认路径必须先满足 `J:\QQFile\NTSD2.4\ntsd_release` release live path 的 C++ trace、Unity fallback 与 Unity optimized 三向等价门禁。

> 2026-08-10 调整：在继续改变战斗热循环结构前，先完成 `future-server-lockstep-architecture.md` 中 L0～L3 的单机帧同步闭环。该调整不降低 1000 AI / 30 FPS 目标，也不提前实施真实服务器；目的是保证后续性能改动始终作用于唯一的确定性 step，并把表现成本从逻辑预算中正确分离。

## 2026-08-23：R8-WP01E current-build正式认证

国际版Unity 2022.3.62f3、当前工作树、1000 production GameObject、MobileExtended、
DataOrientedCanonical、LooseQuadtree、正式CentralOnly、每帧最多1 tick下：

| Workload | Logic Avg/P95 | Visible frame Avg/P95 | Main/Render/GPU P95 | 0B/GC/capacity | Central |
|---|---:|---:|---:|---:|---:|
| Dispersed1000 120+1800 | 16.513 / 18.575ms | 9.490 / 25.525ms | 25.286 / 0.696 / 3.841ms | 0B、0/0/0 collection、critical0 | 1 draw、SetPass4 |
| Combat1000 120+1800 | 16.313 / 19.044ms | 19.078 / 33.058ms | 26.901 / 1.015 / 2.841ms | 0B、0/0/0 collection、critical0 | 1 draw、SetPass≈4 |

两组teardown完整恢复。DesktopExtended与池/slot/generation current-build focused为299/299 PASS；fresh
Legacy/DataOriented 180-tick A/B的input、RNG、metadata、world、slots、aRest、vRest、stats、events、overall、
workload、roster 12项hash全部相等。WP01E在Editor current-build范围通过30FPS门。

Editor Profiler的完整帧仍含Editor-side allocation，但60秒窗口没有任何Gen collection，战斗tick/driver/
presentation/player-loop边界均0B；Player hard gate必须在WP01F验证。Combat visible P95距33.333ms约0.275ms，
Android真机不可从Editor外推。

## 2026-08-10：AI、中央表现与周边搜索归因结果

本轮在同一 1000 AI 生产负载、相同逻辑输入和相同最终确定性 hash 下，完成了三项独立归因。详细诊断中的逐段 `Stopwatch` 只用于比较同一诊断模式下的阶段成本，不能把它的绝对帧率当作最终 30 FPS 门禁。

- **AI 输入同步 A/B：**旧 full-sync 预热样本为 `27.179 ms/tick`，progress-only 同步为 `26.270 ms/tick`，约改善 `3.3%`。两者最终 hash 一致且 sampled GC 为 0，因此保留该修改；它是有效的小幅优化，不是容量问题的单一解法。
- **中央资源缓存 A/B：**可信资源缓存由通用字典查询改为预热的引用身份开放寻址表。同一详细负载中 `Render/PrepareFrame/LegacyCapacityGuard` 从 `4.6951` 降至 `3.9834 ms/tick`，约减少 `0.712 ms`（`-15.16%`）；`ResolveCommands` 从 `2.3745` 降至 `2.0409 ms`，`WriteQuads` 从 `1.2161` 降至 `1.0231 ms`。两轮均为 180 sampled ticks、3004 commands、`0 B` sampled GC、相同最终 hash，teardown 完整恢复。报告为 `Temp/NTSD_ProductionEntityStress.combat1000.capacity-pressure-before-central-20260810.json` 与 `Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-capacity-pressure-smoke.json`。
- **AI 周边目标搜索：**最近目标查询为 `0.6522 ms/tick`，特殊对象扫描为 `0.1687 ms/tick`，合计约 `0.821 ms/tick`，约占详细模式完整 tick 的 `2.9%`。最近目标查询 180000 次共访问 1411230 个索引行，平均每个 AI 约 `7.84` 行；特殊扫描使用预建 `SpecialSlots`，当前全角色负载的特殊 slot visit 为 0。该路径已经是 X 排序/角色/队伍索引查询，并非每个 AI 全表扫描；它不是当前第一瓶颈。

最新详细报告 `Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-capacity-pressure-smoke.json` 为 180 sampled ticks，Avg/P95=`28.023/34.365 ms`，sampled GC=`0 B/tick`，最终 hash=`b8a07be2e5ed9e94f150f4b6e0e426e6e8d23630c69e5fe05a39636e63707821`，teardown=`restored`。非详细报告 `Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-performance-smoke.json` 为 Avg/P95=`28.299/37.233 ms`。平均值进入 30 Hz 预算，但 P95 仍超过 `33.33 ms`，因此 1000 AI 稳定 30 FPS 门禁仍未关闭。

当前详细阶段排序为：`CharacterInput=5.960 ms`、`RenderDispatch=4.111 ms`、`CandidateCollect=3.582 ms`（P95=`8.450 ms`，波动最大）、`LateEntityUpdate=3.207 ms`、`FrameAdvance=1.922 ms`。下一步不再盲改 AI 搜索或中央表现，而由关闭 Deep Profile 的 Unity Profiler 实帧采样确认：

1. `CandidateCollect/ParticipantBodyItrBuild`、`DirectBroadphase`、`PairExactLoop` 的实际尖峰来源；
2. `CharacterInput/EntityInputPass` 的正式模式成本；
3. `RenderDispatch` 与 `LateEntityUpdate` 在非诊断模式下的自耗时与调用次数。

fresh 验证：Unity 脚本刷新编译成功；central/detail focused EditMode job `7a13f401bd114a24ba80e735d8f0356c` 为 `35/35 passed`；AI focused job `3b06a13e6ed343559cb25d48c221fc24` 为 `58/58 passed`；`BattleRuntimeSelfCheck` 于 `2026-08-10 21:05:56` 返回 `PASS`。T8 默认 `stage.dat` 与 Android 真机验证继续排除。

## 1. 当前目标

先完成稳定的单机战斗运行时，并让 1000 个真实生产 AI 实体在目标场景中稳定运行到 30 FPS，同时保持权威 C# 战斗逻辑的规则、字段、副作用和 pass 顺序不变。

本计划中的“1000 实体”必须满足：

- 实际存在 1000 个生产 GameObject，而不是只创建 1000 行纯数据。
- 使用生产对象池、runtime slot、DAT、输入、AI、碰撞、命中、opoint、生命周期和中央渲染路径。
- Game 与 Scene 视图能够观察到实体表现；无表现测试只能作为隔离诊断，不能代替最终验收。
- 逻辑频率固定为 30 Hz；单个逻辑 tick 内不使用真实帧耗时改变战斗结果。

## 2. 本阶段非目标

以下工作暂缓，不纳入 1000 AI / 30 FPS 完成条件：

- 真实服务器和网络传输。
- 服务器权威帧时钟、ACK、重传和网络 Jitter Buffer。
- 断线重连、服务器快照下发和联网回滚。
- 完整自研 ECS 迁移。
- T8 默认 `stage.dat` 部署。
- Android 真机验证。

未来服务器方案记录在 `Assets/NTSD/Docs/future-server-lockstep-architecture.md`，但不得反向阻塞当前单机目标。

当前执行顺序固定为：先完成 P2～P5 的单机热路径优化和 P6 验收；同时只维护未来服务器备忘中的接口、数据所有权和迁移门禁。真实服务器、transport、网络追帧、快照恢复代码均在单机目标达成后再进入 S0。

## 3. 必须保留的未来兼容边界

即使本阶段只做单机，以下已有边界继续保留：

- `SimulationConstants.SIM_DT = 1 / 30` 的固定逻辑步长。
- `FrameInputSet` 的离散输入边界。
- 稳定 player、runtime slot、stable id 和 generation 身份。
- 确定性 RNG 及其固定消费顺序。
- `Manual` 逐 tick 驱动和回放入口。
- lockstep core checksum 和双世界一致性验证。
- 表现层只读取逻辑结果，不反写逻辑真相。

这些接口是低成本的未来兼容边界，不等于本阶段要实现联机。

## 4. 当前已知基线

2026-08-08 的预算模式报告显示：

- 单个带表现 tick Avg/P95：`45.573 / 61.142 ms`。
- Unity 可见帧 Avg/P95：`75.388 / 103.443 ms`。
- 最大 `1 tick/frame` 后约为 `13.26 FPS`。
- 无预算模式允许最多 `4 ticks/frame`，可见帧 Avg/P95 为 `161.826 / 257.808 ms`。
- 两种调度的最终 lockstep core hash 一致。
- logic tick 采样为 `0 B/tick`，但 Editor frame GC 仍有显著分配。

结论：

1. 原先同一 Unity Update 最多执行 4 个完整 tick 是停顿放大器。
2. 取消 4 tick 不能解决单 tick 已超过 `33.33 ms` 的容量问题。
3. 当前剩余工作必须围绕单 tick CPU、Render Thread 和 Editor frame GC 分别归因。

### 4.1 2026-08-08 P0 实施证据

- 已移除 `LocalFreeRun` 对 `4 ticks/frame` 的强制下限；单机和压力工具默认值均为 `1 tick/frame`。
- 显式配置 `4 ticks/frame` 的吞吐诊断仍保留，归一化不会把它改回 1。
- Unity 脚本刷新与编译成功，Console 编译错误为 0。
- 聚焦 EditMode 测试任务 `4e4179df7ebe46e69c7704b747bda619`：`9/9 passed`。
- `BattleRuntimeSelfCheck`：2026-08-08 16:06:51 新鲜结果 `PASS`。
- 50 个真实生产 AI 冒烟报告：
  `Temp/NTSD_ProductionEntityStress.p0-single-tick-smoke-20260808.json`。
  结果为 `SmokePassed`，`maximumCatchUpTicksInFrame=1`、`framesWithCatchUp=0`，清理后 active GameObject、world entity 和 claimed slot 均为 0。
- 该冒烟的逻辑 tick 分配为 `0 B/tick`，但 Profiler frame GC 平均约 `204322 B/frame`；这证明 Editor frame 分配尚未关闭，不能把 P0 调度修正写成 30 FPS 已完成。

当前状态：P0 的默认时钟、显式吞吐诊断和基础报告口径已落地；Render Thread 自动化报告字段仍待补齐，P1 已完成 Idle、Move 和 Dispersed 三层短样本。

### 4.2 2026-08-08 P1 分层短样本

以下三组均使用 1000 个真实生产 GameObject、`1 tick/frame`、10 tick 预热、30 tick 采样，并在结束后恢复 active GameObject、world entity、claimed slot 和对象池 active count：

| 层级 | 报告 | 逻辑 tick Avg/P95 | Unity 帧 Avg/P95 | tick GC | 关键证据 |
|---|---|---:|---:|---:|---|
| Idle1000 | `Temp/NTSD_ProductionEntityStress.p1-idle1000-short-20260808.json` | 27.648 / 30.208 ms | 57.363 / 69.916 ms | 0 B | 无 AI、pair peak=0、candidate peak=0 |
| Move1000 | `Temp/NTSD_ProductionEntityStress.p1-move1000-short-20260808.json` | 27.901 / 28.950 ms | 64.404 / 120.037 ms | 0 B | 40,000 次 DAT `walking_speed` 速度赋值；1000/1000 实体观察到逻辑位移 |
| Dispersed1000 | `Temp/NTSD_ProductionEntityStress.p1-dispersed1000-short-20260808.json` | 48.137 / 60.587 ms | 87.883 / 129.479 ms | 0 B | 完整 AI；pair peak=23,262、candidate peak=448 |

当前证据支持：

1. 1000 实体基础 pass 和移动层的单 tick 已接近但没有稳定越过 `33.33 ms`；基础移动不是当前首要逻辑瓶颈。
2. 完整 AI 相对 Idle 平均增加约 `20.49 ms/tick`，是当前最明确的逻辑侧超预算来源。
3. 三层的 Unity 可见帧耗时均显著高于逻辑 tick，且 Profiler frame GC 平均仍约为 203–270 KB/frame；表现、Editor 诊断和外围回调必须与逻辑热循环分开治理。
4. `Move1000` 是用于隔离移动成本的诊断负载：它每 tick 从角色 DAT 读取 `walking_speed` 并写入逻辑 `Vx`，覆盖帧推进、边界、碰撞快照和表现，但不伪装成 1000 个人类 roster 输入，也不替代完整 AI 输入验收。

### 4.3 2026-08-08 P2 数据导向 AI A/B

在相同 seed、1000 个真实生产 GameObject、`1 tick/frame` 和 100 个正式采样 tick 下，对 `LegacyCanonical` 与 `DataOrientedCanonical` 进行了可回退 A/B：

| 配置 | 报告 | 逻辑 tick Avg/P95 | Unity 帧 Avg/P95 | frame GC |
|---|---|---:|---:|---:|
| LegacyCanonical | `Temp/NTSD_ProductionEntityStress.p2-ai1000-legacy-ab100-20260808.json` | 43.702 / 63.488 ms | 72.965 / 103.851 ms | 279413 B |
| DataOrientedCanonical | `Temp/NTSD_ProductionEntityStress.p2-ai1000-data-oriented-ab100-20260808.json` | 34.600 / 42.642 ms | 62.362 / 85.664 ms | 261961 B |
| DataOrientedCanonical，关闭正式表现构建 | `Temp/NTSD_ProductionEntityStress.p2-ai1000-data-simonly-ab100-20260808.json` | 27.452 / 34.325 ms | 47.037 / 73.412 ms | 206022 B |

三组运行的输入、RNG、world、slots、aRest、vRest、stats、events 和 overall 最终 hash 全部一致；overall 为 `62a09580086073b0e7faf404b53b553a745eeddb33f8ebe3080c086dbb72d499`。对象清理与驱动状态也都恢复成功。

带细分计时的短样本显示：

- `CharacterInput` 从 16.894 ms 降到 8.279 ms；
- `RemainingAiDecision` 从 9.451 ms 降到 3.333 ms；
- `RenderDispatch` 仍约 7.243 ms，其中 `BuildCommands` 约 2.651 ms、`CaptureEntities` 约 1.879 ms、`SortEntities` 约 1.095 ms；
- 正式表现构建相对无表现 A/B 增加约 7.15 ms/tick，是当前跨过平均 33.33 ms 门槛的直接差值；
- `DataOrientedCanonical` 已通过聚焦 EditMode `12/12` 与 2026-08-08 16:53:39 的新鲜 `BattleRuntimeSelfCheck PASS`，因此提升为当前默认；`LegacyCanonical` 保留为显式回退和等价 oracle。

当前结论：局部数据导向热循环方向成立，但没有证据支持把整个战斗 runtime 重写为 ECS。后续优先处理正式表现链、Unity API/引用类型热路径和 Editor frame GC；只有新的 profile 数据证明现有数据布局本身构成不可消除瓶颈时，才重新评估更大范围迁移。

## 5. 时钟和调度边界

### 5.1 OfflineLocal

- 单机性能验收时，每个 Unity Update 最多执行 1 个逻辑 tick。
- 不使用 `LocalFreeRunMinCatchUpTicks = 4` 一类强制下限。
- 积压只作为容量诊断；不得在同一可见帧连续执行 4 个完整战斗 tick 来伪装时间追平。
- 若未来需要单机偶发卡顿恢复，应增加独立的 `LocalHitchRecovery` 策略和预算，不能复用网络追帧语义。

### 5.2 ManualReplay

- 只由测试、回放或恢复流程显式推进。
- 不读取 Unity wall clock。
- 支持选择是否构建表现，但无论表现开关如何，逻辑 checksum 必须一致。

### 5.3 NetworkLockstep

- 本阶段保留类型边界但不启用。
- 未来只能按服务器帧和客户端帧的差值追帧，不能用本地 `Time.unscaledDeltaTime` 代替服务器进度。

## 6. 固定性能验证矩阵

| 场景 | 负载 | 目的 | 当前阶段门禁 |
|---|---|---|---|
| Idle1000 | 1000 个可见生产对象，不运行 AI 决策 | 隔离 GameObject、中央渲染和基础 pass | 诊断 |
| Move1000 | 按 DAT `walking_speed` 驱动 1000 个逻辑实体移动，不运行 AI 决策 | 隔离移动、帧推进、边界和表现 | 诊断 |
| Dispersed1000 | 完整 AI，实体分散 | 正常大规模生产负载 | 必须达到 30 FPS |
| Combat1000 | 完整攻击、命中、opoint、音效和生命周期 | 最终综合验收 | 必须达到 30 FPS |
| Concentrated1000 | 1000 个实体集中于小范围 | 最坏交互复杂度 | 极限报告，不预先承诺 30 FPS |

`Concentrated1000` 可能形成 `C(1000,2) = 499,500` 个无序实体对。若这些对都产生真实有效交互，空间索引无法消除真实工作量；任何候选上限、命中上限或降频都属于玩法规则变化，必须另行批准。

## 7. 正式验收口径

最终 `Dispersed1000` 和 `Combat1000` 至少满足：

- Unity Editor，关闭 Deep Profile。
- 1000 个真实生产 GameObject 全部生成成功。
- Game 与 Scene 视图能够看到实体。
- 固定 30 Hz 逻辑；正常可见帧最多一个逻辑 tick。
- 预热后连续采样不少于 60 秒。
- 平均帧率不低于 30 FPS。
- P95 主线程帧耗时不高于 `33.33 ms`。
- 报告单 tick Avg/P95/max，而不是只报告一帧中多个 tick 的总耗时。
- 稳态逻辑 tick 为 `0 B GC`，Editor frame 不发生周期性大 GC。
- 中央渲染保持低 SetPass；同时核对 Render Thread、Mesh 上传和同步等待。
- 与未优化路径的输入、RNG、slots、aRest、vRest、stats、events 和 overall checksum 一致。
- cleanup 后 active GameObject、world entity、claimed slot 和对象池 active count 恢复到基线。
- Unity compile 为 0 error，聚焦测试、完整 EditMode tests 和 `BattleRuntimeSelfCheck` 通过。

## 8. 实施批次

### P0：拆分单机时钟与压力工具口径

1. 移除单机模式强制最少 4 tick 的配置规则。
2. 单机性能验收默认 `1 tick/frame`。
3. 压力报告分别记录：
   - 单 tick CPU；
   - Unity 可见帧 CPU；
   - 本帧执行 tick 数；
   - backlog 与丢弃量；
   - Render Thread；
   - logic tick GC 与 Editor frame GC。
4. 保留旧吞吐模式作为显式诊断选项，不得作为默认值或正式 FPS 结论。

### P1：重建五层可比较基线

1. 统一 seed、角色、DAT、stage 边界和预热时间。
2. 依次运行 Idle、Move、Dispersed、Combat、Concentrated。
3. 每层只比上一层多一种负载，使用差值确定模块成本。
4. 记录 Profiler marker、JSON 报告、checksum 和 cleanup 结果。

### P2：CharacterInput

1. 重新测量 snapshot build、AI sensing、AI decision、输入提交和按键边沿消费。
2. 优先消除高频对象图遍历、重复解析、虚调用、临时容器和重复索引构建。
3. 允许对已证明的热点使用连续数组、稳定索引和局部 SoA kernel。
4. 不改变低槽先于高槽、RNG 消费、目标选择 tie-break 和同 tick 可见性。

### P3：CandidateCollect

1. 继续压缩 pair build、role-aware broadphase、exact loop、去重和排序尖峰。
2. 缓存必须用 generation-aware handle 验证，slot 复用后不得命中旧实体。
3. role/bounds 证明失败时 fail-closed 到等价路径。
4. 不删除真实有效 pair，不改变候选消费顺序。

### P4：其余基础 pass

依次处理：

1. `LateEntityUpdate`；
2. `FrameAdvance`；
3. `PreInteraction`；
4. `StageBounds`；
5. `CharacterHitConsumePostInteraction`；
6. 其他超过预算占比或产生 P95 尖峰的 pass。

### P5：表现、Render Thread 与 Editor frame GC

1. 核对 `RenderDispatch`、`BuildCommands`、Mesh 更新、材质页和 Texture2DArray 提交。
2. SetPass 已降低不等于 Render Thread 成本已关闭，必须单独测量线程等待和上传。
3. 将诊断、日志、反射、字符串、协程和 Editor-only API 从稳定热路径移出或降采样。
4. 逻辑事件不能因关闭表现而丢失；追赶/无表现模式不得重复播放历史音效和粒子。

### P6：最终验收与文档闭环

1. 对 `Dispersed1000` 和 `Combat1000` 运行正式 60 秒报告。
2. 对 `Concentrated1000` 输出极限容量和复杂度报告。
3. 运行 compile、聚焦测试、完整 EditMode、self-check 和双世界 checksum。
4. 只有所有门禁通过后才能写“1000 AI / 30 FPS 已完成”。

## 9. 每批回退原则

- 每项优化必须有开关或保留等价 oracle，直至 parity 证据充分。
- 发现输入、RNG、slot、opoint、碰撞、命中、生命周期或 checksum 分叉时立即停止晋升。
- 不用降低 AI 频率、跳过实体、限制命中或改变 DAT 数值来伪造性能达标。
- 不因本计划修改 T8、Android、主菜单、通用 UI 或无关第三方模块。
