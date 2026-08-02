# NTSD 自研 ECS/SoA 战斗内核与帧同步迁移计划

## 2026-08-02 — Slice 0/1 fresh 证据更新（当前口径）

本节只更新当前证据与门禁状态，并覆盖下方较早日期的“当前状态”描述；不改变本计划的权威、范围或验收合同。

### Authority400 等价诊断

- `.omc/validation/authority400-witness-slice0-final-20260802/{W01,W02,W03,W04,W07}/comparison.json` 均为 `equal-diagnostic`；对应比较 tick 数依次为 `8/2/2/1/3`。
- 五份结果均为 `certificateEligible=false`、`diagnosticComparison=true`。它们是固定夹具下的 fresh 等价诊断，不是生产证书，也不能扩写为整体迁移完成。

### Unity fresh 验证

- W06 focused EditMode：UnityMCP job `e1e1a3d8b6e74bb895fa0068333a4cf7`，`2/2 passed`。
- 导入 W06 测试前的既有全量 EditMode：UnityMCP job `e1b5a3057e7d4f7bbdd71bbb8cbe381e`，`472/472 passed`、`0 failed`、`0 skipped`。
- W06 导入后当时发现总数为 `474`，但该轮只补跑了 W06 的 `2/2`；因此没有“全量 474/474”证据。此后 full-order 运行暴露的 **W05 测试隔离失败**已完成代码修复，但 Unity LicenseRevoked modal 阻止正式 focused summary 与 full-suite 重跑；当前 full suite 仍**不得写绿**。
- `BattleRuntimeSelfCheck` fresh 结果：`2026-08-02 02:49:44 PASS`。

### Registry 日志与 W05 隔离修复边界

- `SimulationWorld.Registry.partial.cs` 的成功 Register/Unregister `Debug.Log` 已由 world-level `EnableRegistryLifecycleLoggingForDiagnostics` 保护。该开关明确默认 `false`，只有显式诊断启用时才构造插值字符串并写 Log；注册、slot/generation、structural event 与 pass 行为未改变。fresh Runtime/Editor dotnet build 均为 `0 errors`。
- `.omc/validation/UnityMCP-http-restored-editor-20260802.log` 的 `3,309,135,435 bytes`（约 `3.309 GB` 十进制）用于发现默认生命周期日志及 Unity 堆栈放大问题。它不是性能样本：`ProductionEntityStressHarness` 运行时把 `Debug.unityLogger.filterLogType` 设为 `LogType.Error`，Candidate final300 报告也记录 `runningFilterLogType=Error`，所以普通 Register/Unregister `Log` 在该基准中已被过滤。**不得把退出默认日志计为 Candidate long300 性能收益或重写其 `34.7836/50.888 ms` 结果。**
- W05 full-order 隔离失败的根因是测试间共享 `GameConfig.LF2ObjectPrefab` 污染。隔离 fixture 现保存原值、测试期间清空，并在 dispose 恢复；严格的每 renderer 两个 mount、current generation 绑定、released generation 不复活与 no-ghost command 契约均保留。fresh Editor dotnet build 为 `0 errors`。
- focused Unity 执行已到 `RunFinished`，日志中没有新的 assertion failure 信号；但随后出现 `License revoked: Your Unity Personal Version license has been revoked` 与 `EditorWindow.ShowModal`，MCP job 无法形成正式 summary，full suite 也未重跑。因此 W05 当前状态只能写为**代码修复 / Unity 回归未验收**，不能写 focused PASS 或 full-suite PASS。

### Extended1000 AI A/B/C

| 顺序 / 模式 | 样本 | Avg (ms) | P95 (ms) | 证据判断 |
|---|---:|---:|---:|---|
| 正向 Legacy | 100 | `43.848` | `60.641` | 同轮相对基线 |
| 正向 Candidate | 100 | `31.327` | `38.780` | 相对 Legacy 的 Avg/P95 分别改善 `28.56%/36.05%` |
| 正向 Candidate + Remainder | 100 | `34.181` | `42.998` | 相对 Candidate 回退 `9.11%/10.88%` |
| 反向 Candidate | 100 | `32.000` | `39.777` | 与正向 Candidate 接近 |
| 反向 Candidate + Remainder | 100 | `34.246` | `40.426` | 相对 Candidate 回退 `7.02%/1.63%` |
| 反向 Legacy | 100 | `74.494` | `163.728` | **异常系统抖动/离群样本**，不得用于稳定相对结论 |
| Candidate final300 no-detail | 300 | `34.7836` | `50.888` | `0 B/tick`；可真实运行，但未稳定达到 30 Hz |

报告为 `Temp/NTSD_ProductionEntityStress.dispersed1000.slice1-{legacy-detail100,candidate-detail100,candidate-remainder-detail100,legacy-detail100-rev,candidate-detail100-rev,candidate-remainder-detail100-rev,candidate-final300}-20260802.json`。其中最新长样本 `Temp/NTSD_ProductionEntityStress.dispersed1000.slice1-candidate-final300-20260802.json` 为 `StoppedCleanly`、`60 warmup + 300 sample`、Avg `34.7836 ms`、P95 `50.888 ms`、GC `0 B/tick`（average/maximum 均为 0），final parity overall hash=`68af82ba7cdf284d7f62e889e1cd1188e14e9c15ec48d15167cd6c8dcf210388`，teardown `restored=true`、活动对象/world/claimed slots 全部清零且 cleanup exception=`0`。该 workload **可真实运行**，但 Avg 高于 `33.33 ms` 且 P95 为 `50.888 ms`，所以仍未通过稳定 30 Hz 门禁。Remainder 在正向与反向都比 Candidate 回退，当前不得启用。

### CandidateCollect 负实验（不进生产）

#### CandidateStore authority

| 报告 | authority | Tick Avg/P95 (ms) | CandidateCollect Avg/P95 (ms) |
|---|---|---:|---:|
| `Temp/NTSD_ProductionEntityStress.dispersed1000.slice1-candidate-neighbor-detail100-20260802.json` | off | `39.1249 / 56.0624` | `4.3780 / 9.5977` |
| `Temp/NTSD_ProductionEntityStress.dispersed1000.slice1-candidate-store-only-detail100-20260802.json` | on | `43.1031 / 65.4513` | `5.3683 / 16.2869` |

- on 相对 off 的 tick Avg/P95 回退 `10.17%/16.75%`，CandidateCollect Avg/P95 回退 `22.62%/69.70%`，未过“至少改善 10%”的性能门。
- 两份报告均为 `StoppedCleanly`、GC average/maximum=`0 B/tick`，final parity 分层 hash 与 overall hash 完全一致（overall=`fa019a38aba6668b7222bf9b61b0400d2cba7b422799bbd0964506a9875450e9`）；on 报告 `requested/configured/applied=true`、`appliedTickCount=160`、legacy fallback=`0`。两份 teardown 均 `restored=true`，活动对象/world/claimed slots 清零且 cleanup exception=`0`。
- 结论：CandidateStore authority 只保留显式诊断/实验入口，生产默认继续关闭，不得将其描述为 CandidateCollect 性能优化完成。

#### Stamped role-aware ordinal map

| A/B 顺序 | 报告 | stamped | Tick Avg/P95 (ms) |
|---|---|---|---:|
| 正向 | `Temp/NTSD_ProductionEntityStress.dispersed1000.slice1-stamped-off-detail100-20260802.json` | off | `34.8805 / 46.3090` |
| 正向 | `Temp/NTSD_ProductionEntityStress.dispersed1000.slice1-stamped-on-detail100-20260802.json` | on | `38.8578 / 68.2115` |
| 反向 | `Temp/NTSD_ProductionEntityStress.dispersed1000.slice1-stamped-on-detail100-rev-20260802.json` | on | `34.0021 / 45.7062` |
| 反向 | `Temp/NTSD_ProductionEntityStress.dispersed1000.slice1-stamped-off-detail100-rev-20260802.json` | off | `32.2586 / 40.3518` |

- stamped on 在正向顺序的 tick Avg/P95 回退 `11.40%/47.30%`，在反向顺序仍回退 `5.40%/13.27%`；正反向均未过门。
- 四份报告均为 `StoppedCleanly`、GC average/maximum=`0 B/tick`、teardown `restored=true`、活动对象/world/claimed slots 清零且 cleanup exception=`0`；两份 on 报告均实际应用 `160` ticks 并在 teardown 恢复。反向 A/B 的全部 final parity hash 一致（overall=`fa019a38aba6668b7222bf9b61b0400d2cba7b422799bbd0964506a9875450e9`）。正向 A/B 的 input/RNG/metadata/world/A-rest/V-rest/stats/events hash 一致，但首份 off 是冷池运行（inactive pool capacity `10 -> 1000`），因此 slots/overall hash 与随后暖池 on 不同；不得误写成正向 overall hash 一致。
- 结论：该实验实现、专用接线与测试已删除，只有上述 JSON 作为负实验记录保留；生产不存在 stamped 开关或默认启用路径。

### 当前未关闭合同

- Candidate 的生产开关与默认值尚未关闭；当前证据不授权切换 production default。
- CandidateStore authority 性能负实验未过门，保持默认关闭；stamped ordinal map 正反向负实验未过门且实验代码已删除。
- 绝对稳定 30 Hz 尚未关闭。
- slot identity parity 尚未关闭；Authority400 的 equal-diagnostic 不能替代该合同。
- T8 默认 `stage.dat` 部署与 Android/Adreno/Mali 真机验收继续排除在当前任务之外。
- W05 根因与代码修复已关闭，但 LicenseRevoked modal 阻止正式 focused summary/full suite，Unity 回归验收仍待关闭，当前不得写绿。当前状态不是“迁移完成”或“已全面对齐”；后续完成声明仍需 fresh 全量通过、对应运行时定向验证与计划既定证书门禁。

- Status: APPROVED FOR EXECUTION
- Approved: 2026-07-27
- Scope: 战斗 runtime、1000 实体性能、确定性固定帧与帧同步基础
- Authority: `J:\QQFile\NTSD2.4\ntsd_release_C#`

## 1. 最终目标

将 Unity 当前对象式战斗 runtime 渐进迁移为共享纯 C# 的数据导向战斗内核：

1. 权威 C# 的规则、pass 顺序、字段语义和可观察行为保持不变。
2. 自研 ECS-style/SoA 承担战斗逻辑真值和高频热循环。
3. Unity 只负责输入采样、资源准备、GameObject/对象池、渲染、音频和编辑器接线。
4. 战斗内核只接受离散输入和不可变数据，不读取 Unity Transform、Time、Physics 或异步资源完成状态。
5. 固定 30 Hz、快照、重放、checksum 和严格延迟帧同步共享同一逻辑入口。
6. 1000 个全 AI 实体在受控 Windows Player 基准中达到正式性能门。

性能优化与帧同步是两个独立验收轴：

- 性能轴验证单 tick 成本、P95、GC、查询复杂度和表现提交。
- 帧同步轴验证相同输入下的确定性、快照恢复、重放和校验恢复。
- 两者共享 BattleKernel，但不得把“实现帧同步”描述为性能优化。

## 2. 明确非目标

本计划暂不包含：

- Unity Entities/DOTS 全项目改造。
- 完整客户端预测和回滚网络。
- 匹配服务、NAT 穿透、反作弊、旁观系统和房间服务。
- T8 默认 `stage.dat` 资产部署。
- Android/Adreno/Mali 真机验收；由用户后续处理。
- 主菜单、角色选择、普通 HUD 等非战斗模块重构。

## 3. 目标架构

### 3.1 BattleKernel

纯 C#、可 headless 执行，只接收：

- `FrameInputSet`
- world seed
- 不可变 DAT catalog
- 不可变 stage runtime snapshot
- profile/capacity 配置

只输出：

- 更新后的 SoA world state
- spawn/destroy/sound/presentation 事件
- tick snapshot
- 分层 checksum
- 诊断 witness

### 3.2 EntityStoreSoA

按 runtime slot 索引、按领域拆分数组：

- Identity：active、generation、stableId、oid、kind、team、owner
- Motion：X/Y/Z、Vx/Vy/Vz、facing
- Frame：frameId、state、wait、next、prevFrame
- Health：HP、MP、PP、damage/runtime stats
- Input：当前输入、边沿、历史和 AI 输出
- Links：holder、target、catching、attacker、parent
- Combat：rest、hitstop、fall、defend、candidate 状态
- Lifecycle：pending spawn/free、visibility boundary、free-slot state

不可在 SoA 内核中保存 `GameObject`、`MonoBehaviour`、`Sprite`、`Renderer` 或 Unity singleton 引用。

### 3.3 Profiles

必须区分：

- `Authority400`：用于逐 tick 权威差分和正式规则证明。
- `Extended1000`：共享规则和 pass，但容量、空间索引和表现策略允许扩展。

超过 400 槽后的结果可以称为规则保持的扩展 profile，不能声称权威 C# 已直接证明其容量语义。

### 3.4 Unity Host

Unity host 负责：

- 渲染帧输入采样并转换为离散 `FrameInputSet`
- DAT/BMP/音频等资源准备
- GameObject/presentation shell 池
- central renderer 和音频消费
- 网络 transport 接入
- 编辑器诊断和压力测试

Unity host 不得：

- 把 Transform 或 Renderer 状态写回战斗真值
- 在 tick 内用 `Time.deltaTime` 决定规则
- 因资源异步完成顺序改变碰撞、生成或 RNG
- 让 presentation stable id 影响逻辑 stable id

## 4. 不可改变的权威契约

迁移中必须逐项保留：

1. `GameTick.Run` 的 pass 顺序。
2. live ascending runtime slot 扫描。
3. 游标之后新生高槽可在同 pass 后续参与。
4. 复用游标之前低槽的实体等待下一 pass。
5. `SerialTickAll` 的逐实体 `Transit -> TU -> snapshot` 顺序。
6. character hit、random weapon drop、object hit 的消费顺序。
7. late pass 内 state/recovery/frame/collision/opoint/tail 的顺序。
8. opoint 在多个权威可见边界分段 flush，不能统一到 tick 末。
9. slot free、deferred unregister 和 generation 变化顺序。
10. world RNG 的状态、调用次数和升序消费顺序。
11. holder/target/link 的失效和恢复语义。
12. presentation observation boundary 不得写回逻辑状态。
13. late live-slot 循环中，当前实体产生的 opoint 必须在该实体结束、slot 游标继续前按权威路径完成分配和激活；高于游标的新槽因此可以在同一 late pass 后续参与。
14. allocator 必须区分权威槽域和起始搜索位置，不能使用覆盖全部槽位的单一全局最小堆。

通用 ECS 的单一帧末 EntityCommandBuffer 不适用于本项目。结构命令必须按权威 pass boundary 分段播放；需要同一 live-slot 循环可见的 opoint 使用 cursor-local immediate playback。

## 5. 当前证据基线

现有 1000 全 AI 无诊断参考：

- average：`42.807 ms/tick`
- P95：`69.274 ms`
- P99：`84.256 ms`
- logic without presentation：`38.463 ms`
- with presentation：`55.823 ms`
- steady-state allocation：`0 B/tick`

已识别热点：

- CharacterInput / EntityInputPass
- AI facts/snapshot/query rebuild
- LateEntityUpdate / FrameTick / Opoint
- CandidateCollect
- RenderDispatch 与重复 runtime snapshot
- 多 pass 全容量扫描和实体虚调用

当前 fresh `BattleRuntimeSelfCheck` 仍失败：

- LooseQuadtree role-aware fixture 在低 direct cost 下未强制 tree path。
- 复合断言同时检查 sync/rebuild/candidate/RNG，错误信息容易误导。
- Slice 0 先修正测试夹具和断言拆分，不修改 production query 行为。

## 6. Slice 0：规则封套与双运行基线

### 目标

在迁移任何写路径之前，建立可重复的权威黄金轨迹、Unity 当前轨迹和性能 workload。

### 工作内容

1. 修正 LooseQuadtree self-check fixture：
   - 需要验证 rebuild/reuse 时显式强制 role-aware tree diagnostic path。
   - sync、candidate sequence、RNG state 和 RNG call count 分开断言。
   - 不修改 `BruteForceSceneQuery` production 决策。
2. 固定 Authority400 小型场景：
   - seed
   - roster/oid/team
   - DAT fixture/version
   - 逐 tick dense input
   - 固定运行 tick 数
   - 每条权威契约对应的 fixture/witness
3. 权威 C# 侧输出逐 tick：
   - tick/input
   - entity/runtime checksum
   - RNG state/call count
   - spawn/free slot 序列
   - 关键 pass witness
4. Unity 侧复用：
   - `SimulationDriveMode.Manual`
   - `FrameInputSet`
   - `BattleParityFrameSnapshot`
   - `BattleParityTraceEditor`
5. 冻结 100/300/500/1000 性能 workload。
6. 已知 Unity 与 C# 差异单独记录，不得成为黄金答案。
7. 建立权威契约覆盖矩阵，至少覆盖：
   - 高槽同 pass 新生并参与
   - 低槽复用等待下一 pass
   - stage 槽域与 dynamic/opoint 槽域
   - pass 内 free 与延迟 unregister
   - 多段 opoint flush
   - holder/target/link 失效
   - RNG 分支和早退
   - collision/hit/tail candidate 生命周期

### 验收门

- Unity compile 0 error。
- focused LooseQuadtree test fresh PASS。
- full `BattleRuntimeSelfCheck` fresh PASS。
- 相同 fixture 可重复输出完全一致的 Unity trace。
- 权威 C# exporter 可重复输出完全一致的 authority trace。
- 首个差异 tick 可定位到具体 domain/pass。
- 受影响的 Slice 不得只依赖 smoke PASS；对应契约 witness 必须实际命中。

### 回退

Slice 0 不改变 production 战斗行为；失败时移除新增测试/导出接线即可。

## 7. Slice 1：只读 AI Sensing SoA Shadow

### 目标

先迁移收益最大、风险最低的只读 AI sensing 数据，旧 AI 仍是正式提交者。

### 工作内容

1. 建立 shadow `EntityStoreSoA`：
   - slot/generation/active
   - X/Y/Z/Vx
   - HP/team
   - frame/state/objtype
2. 将 nearest target、team summary、special OID role 和空间索引改为直接读取 SoA shadow。
3. 不再每 tick 从完整 `LF2Entity` 对象图构建多套 AI 临时快照。
4. 旧路径和新路径同时计算：
   - selected target slot
   - input bits
   - RNG state/call count
5. 默认仍由旧路径提交 AI 输入，直到 shadow parity 完成。

### 验收门

- Authority400 所有固定场景逐 AI 选择一致。
- RNG state/call count 一致。
- 逐 tick checksum 一致。
- 1000 AI 受控 A/B 的 average 与 P95 至少改善 10%，否则不切默认。
- 0 B/tick。

### 回退

保留 `LegacyAiSensing` / `SoAShadowAiSensing` / `SoAAiSensing` 三态开关。

## 8. Slice 2：纯数值 Pass 成为 SoA Canonical Writer

### 目标

让低耦合数值 pass 首先直接写 SoA，验证单一真值和无 snapshot refresh。

### 迁移顺序

1. battle flow tick/cooldown。
2. stage Z clamp，stage bounds 改为 tick 前不可变注入。
3. 无跨实体副作用的 frame postprocess 数值字段。
4. rest/cooldown 数值递减。

`entity postframe tail` 不属于本 Slice。它会读取 frame/character 数据并修改 HP、catch 和 hit candidate carriers，延后到 frame、lifecycle 与 collision ownership 全部明确后再迁移。

### 验收门

- 每个字段只能有一个 canonical writer。
- 旧 façade 只读 SoA，不得双写。
- Authority400 逐 pass witness 和最终 checksum 一致。
- presentation 开关不影响逻辑 checksum。

### 回退

按 pass 单独切回旧 writer；禁止整阶段长期双写。所有 canonical ownership 开关只能在 `ResetWorld` 或合法 snapshot restore 边界切换，禁止任意 tick 热切换。

## 9. Slice 3：Registry 与 Lifecycle 内核化

### 目标

让 slot、generation、active、pending 和 free-slot 成为 SoA 真值。

### 工作内容

1. 分页/动态 capacity。
2. range-aware 最低空闲槽分配：
   - authority/profile 定义保留域。
   - stage spawn 保持从槽 20 起的权威搜索域。
   - dynamic/opoint 保持从槽 50 起的权威搜索域。
   - 各域可以使用最小堆或分层位图，但必须返回该生成路径允许域中的最低槽。
   - 扩容只能追加高槽，不能改变既有域的搜索顺序。
3. generation-safe handle。
4. live ascending slot cursor。
5. 分段 spawn/free/unregister command buffer。
6. 去除 `RuntimeSlotTable.Entry.Entity` 与重复 `RawRuntime` 真值。
7. `LF2Entity` 暂时保留为兼容 façade/presentation shell。

### 验收门

- spawn/free slot 序列与 Authority400 一致。
- stage/dynamic/opoint 各分配路径的 slot 序列分别一致。
- 高槽同 pass 可见、低槽复用延迟语义一致。
- snapshot round-trip 后 handle/link 全部一致。
- 压力 cleanup 无 active/slot/pool 泄漏。

### 回退

registry ownership 以 feature gate 整体切换，不允许新旧 allocator 同时分配。

## 10. Slice 4：Frame/Movement/Archetype 与 Opoint

### 目标

以静态 System 替换 `SimTransit`、`SimTU`、`SimFrameTick` 等热路径虚调用。

### 工作内容

1. 按 Character/Weapon/SpecialAttack/Other 建稳定 kind 分区。
2. 每个全局可观察 pass 仍按 runtime slot 顺序提交。
3. frame catalog/DAT 访问改为不可变索引。
4. movement、ground、boundary、facing 直接读写 SoA。
5. opoint 改为 kernel spawn command。
6. 在权威 late pass 的多个边界分别 flush。
7. Sound 改为 `SoundEvent`，由 Unity host 消费。

### 验收门

- Naruto 等既有定向组合技的 frame/opoint/位置/生命周期轨迹一致。
- opoint slot、tick、frame 和 parent/holder 一致。
- RNG 调用一致。
- 不存在实体 per-frame MonoBehaviour 战斗回调。

### 回退

按 entity kind 和 pass 提供 temporary compatibility adapter；发生 parity 差异时只回退当前 vertical slice。

## 11. Slice 5：Collision、Interaction 与 HitResolve

### 目标

将碰撞候选和命中写路径迁入内核，同时避免无意义全局 pair。

### 工作内容

1. bdy/itr/cpoint 编译为不可变 frame catalog。
2. Loose Quadtree 负责常规分散实体。
3. attack ITR 查询 body index，避免 body-body、itr-itr 和无 role pair。
4. 高密度节点使用二级分割或确定性 sweep。
5. candidate 使用 slot/generation/itr index。
6. candidate 最终按权威顺序归并。
7. character hit、weapon drop、object hit 保持权威消费顺序。
8. HitResolve 直接写 SoA combat/health/link 状态。
9. 在 frame、lifecycle、collision candidate 和 hit ownership 完成后迁移 entity postframe tail。

### 重要限制

ECS 不会自动消除真实的 O(N²) 交互。若 1000 个攻击体和 body 全部重叠，仍可能出现接近百万个有效方向检查。不得为了性能私自限制有效命中；任何候选上限都属于玩法规则变化，必须另行批准。

### 验收门

- BruteForce/SoA broadphase candidate sequence 一致。
- HitResolve 事件、HP、rest、hitstop、RNG 一致。
- 分散场景不再产生全局 pair。
- 高密度场景有独立 worst-case 报告。

### 回退

保留 BruteForce、LooseShadow、SoAFormal 三种查询模式；formal parity 失败自动回退且必须记录 reason code。formal/legacy ownership 只能在 `ResetWorld` 或合法 snapshot restore 边界切换。

## 12. Slice 6：移除每实体 GameObject 热循环

### 目标

1000 个逻辑实体不再意味着 1000 个战斗 Update/LateUpdate/Renderer 回调。

### 工作内容

1. 每 tick 生成紧凑不可变 presentation command buffer。
2. central renderer 批量消费。
3. 仅屏幕内或确需旧组件的对象拥有 presentation shell。
4. GameObject pool 只管理表现，不管理逻辑真值。
5. Sound/FX/renderer event 使用 generation-safe handle。
6. 移除 `RefreshRuntimeSnapshot` 和 Late 重复快照。

### 验收门

- 关闭全部 presentation 后逻辑 checksum 不变。
- Central/Legacy 对照 workload 一致。
- 不再出现 pool 扩容后对象有声音/碰撞但不显示。
- Scene 诊断模式仍能查看实体逻辑和 presentation binding。

### 回退

Central/Legacy presentation 可独立切换；不得回退逻辑内核 ownership。

## 13. 帧同步贯穿设计

帧同步不是最后追加的模块，各 Slice 从开始就必须满足以下接口：

1. `ResetWorld(seed, immutableCatalog, stageSnapshot)`
2. `StepOneTick(FrameInputSet)`
3. `CreateSnapshot()`
4. `ApplySnapshot(snapshot)`
5. `CreateChecksum(profile)`
6. `ExportReplayInput()` / `ReplayFromSnapshot()`

Snapshot 只能在完整逻辑 tick 的正式 observation boundary 捕获。首期禁止在任意 pass 中间捕获可用于网络恢复的正式 snapshot。

Snapshot/checksum 至少覆盖：

- tick index、world seed、RNG state 和 RNG call count
- 所有逻辑实体的 slot、generation、active/pending/dormant 状态
- identity、motion、frame、health、input、links 和 combat 状态
- range-aware allocator 的各槽域空闲结构与容量
- pending spawn/free/unregister 命令及其 visibility boundary
- world cooldown/gate、stage、battle flow 和 result 状态
- 输入缓冲的消费位置
- 事件流 sequence/cursor，避免恢复后重复或遗漏 Sound/FX/Spawn 事件
- immutable catalog/stage snapshot fingerprint

必须提供恢复等价测试：

1. 世界 A 从 tick N 继续运行。
2. 世界 B 在 tick N snapshot restore。
3. A/B 接收相同后续输入。
4. 逐 tick checksum、RNG、slot 序列和事件序列保持一致。

首期网络行为：

- 严格延迟帧同步。
- 某 tick 全部玩家输入 ready 后才推进。
- 网络乱序、重复和迟到包进入 input buffer，不直接调用战斗对象。
- 周期性交换 checksum。
- checksum 不一致时请求权威快照并重同步。
- 输入和快照保留版本、tick、player、seed 和 catalog fingerprint。

后续阶段再评估：

- 客户端预测
- 回滚窗口
- 表现插值
- 断线重连与旁观

## 14. 性能测试方法

正式性能结论必须满足：

1. 同一 commit、build profile、机器和单 Unity 实例。
2. 固定 seed、input fingerprint、roster 和 workload。
3. 100/300/500/1000 阶梯。
4. 至少 30 warmup tick，至少 1200 measured tick。
5. 至少三轮交错 ABBA。
6. 比较 median、average、P95、P99、最大积压和 GC。
7. 比较前先通过 checksum、事件、碰撞、opoint 和 lifecycle parity。
8. Editor 只用于趋势回归。
9. Windows Player no-diagnostic 才是正式门。

最终 1000 全 AI simulation-only 门：

- average `<= 33.33 ms`
- P95 `<= 33.33 ms`
- steady-state `0 B/tick`
- 无持续 tick backlog
- final checksum 和固定事件轨迹一致

候选路径经过三轮 A/B 后，median 与 P95 改善不足 10%，不得切为默认。

## 15. 每阶段统一完成条件

每个 Slice 只有同时满足以下条件才能标完成：

- 代码已写。
- Unity compile 0 error。
- focused tests fresh PASS。
- full `BattleRuntimeSelfCheck` fresh PASS。
- Authority400 差分 fresh PASS。
- 目标性能 A/B 有新鲜报告。
- teardown 和资源清理通过。
- Architect 复核通过。
- 对齐文档记录与实际证据一致。

任何一项缺失只能报告“已实现但未完成验收”。

## 16. 文档与提交规则

执行过程中持续更新：

- `.omc/plans/battle-kernel-ecs-lockstep-migration-20260727.md`
- `Assets/NTSD/Docs/csharp-vs-unity-battle-alignment.md`
- `Assets/NTSD/Docs/HANDOFF-codex-battle-alignment.md`
- 性能/渲染证据继续记录到 `central-battle-render-system-plan.md`

每个 Slice 记录：

- 权威 C# 文件/类型/方法
- Unity/内核对应文件
- canonical owner
- feature gate 和回退方式
- 测试 fixture
- correctness evidence
- performance evidence
- 未关闭风险

所有 feature gate 分为两类：

- shadow/read-only gate 可以在诊断启动前配置。
- canonical writer/allocator/query ownership gate 只能在 `ResetWorld` 或合法 snapshot restore 边界切换；运行中切换必须先执行显式 canonical-state conversion，首期不支持热切换。

仓库当前存在用户未提交的 `Tools/DatSkillFlowWeb/**` 修改和未跟踪文件；本计划不得覆盖、回滚或擅自纳入这些文件。

## 17. 立即执行顺序

1. Slice 0 修复 LooseQuadtree 测试夹具。
2. fresh focused test 与 full self-check。
3. 固定 Authority400 smoke fixture。
4. 构建权威 C# tick/checksum exporter。
5. Unity trace 与权威 trace 首差异定位。

## 2026-08-01 — Slice 0 当前证据与未关闭合同

### 权威与范围

- 一般战斗逻辑唯一权威仍是 `J:\QQFile\NTSD2.4\ntsd_release_C#`。C++、反汇编和旧记录不得作为一般 authority；仅保留用户明确指定的 Naruto 防下攻与跳跃水平动量两个窄历史定向例外，且不得类推。
- T8 默认 `stage.dat` 部署与 Android/mobile rendering 不进入本 Slice；不得为 smoke 或 parity 私自补资产。

### 已获得的 Slice 0 证据（不是 certificate）

- full `BattleRuntimeSelfCheck`：**PASS**，日志 `.omc/validation/BattleRuntimeSelfCheck-combotxn2-20260801.log`。
- W01：6/6 `equal-diagnostic`，`.omc/validation/authority400-witness-root-run-W01-aftercombo2`；W02：2/2 `equal-diagnostic`，`.omc/validation/authority400-witness-root-run-W02-final`。两者都不是 production/parity certificate。
- `ProductionEntityStress` 的 simulation-only AI short smoke 在 100/300/500/1000 四档均 cleanup **PASS**：`.omc/validation/ProductionEntityStress.dispersed{N}.ai-sim-smoke-20260801.json`。
- 1000 档 baseline 为平均 tick `81.888 ms`（约 `12.21 Hz`）、平均 visible frame `394.413 ms`，未达到 30 Hz。四档结果受动态 opoint/容量影响而非单调；仅可作 baseline，不能宣称性能验收。

### 静态确认、尚未修复

- **C07 contract correction**：权威 C# `RunLateEntityUpdate` 不执行独立 collision；Unity `LateEntityUpdateAll` 额外调用 `SimEntityCollision`。C07 必须维持为 confirmed difference / pending contract correction，不能标为完成。
- **W08/C12 P0 presentation writeback**：`LF2ObjectRenderer` 的表现阶段可 release/consume forced runtime integer position 并回写 `XInt/YInt/ZInt`。必须迁移到确定性 TU 成功物理尾部；当前未修复。
- **W03/W04 v4 structural witness**：仍在实现中，未完成、不得写为已验证。
6. 冻结 100/300/500/1000 workload。
7. Slice 0 通过后进入 Slice 1 AI sensing SoA shadow。

## 2026-08-02 — StableId 与 AI decision 热路径：实现完成，Unity 验收受阻

### StableId 生命周期归属

- 逻辑 `StableId` 已改为只在 `SimulationWorld.Register` 已成功完成 runtime slot/rest admission 后分配；注册失败不会消耗 allocator，也不会留下 identity 泄漏。
- `LF2ObjectRenderer` 的 renderer identity 改用 `GetInstanceID()`，仅供表现层识别，绝不再调用 world 的逻辑 StableId allocator。
- 显式 StableId 保持可用；与活动 identity 冲突时 fail-closed，并把 auto allocator 下界推进到显式 ID 之后，避免后续自动 ID 重复。
- 已新增 `StableIdDeterminismEditorTests`，覆盖冷热对象池、`ResetWorld` checksum、失败注册不消耗 ID、同槽复用 generation 与新生命周期 identity。该测试文件已写入，但未因下述 Unity modal 获得可宣称 PASS 的新鲜结果。

### AI decision row context

- `AiDecisionRowContext` 已实现且默认关闭：每个 AI decision 只 bind 一次，正常路径为两次 gateway / 六个 identity rows；不再为同队/held 扫描预先遍历所有槽位。
- RNG 之前的 context 失败允许整实体回退 legacy；RNG 已消费后的失败为 hard failure，禁止 legacy replay，避免双重随机数消费或顺序漂移。
- 已新增 focused 与 `0 B/tick` 契约测试，并接入压力测试门禁；这些是代码/测试实现状态，尚未获得 Unity focused PASS 或性能晋升证据。

### 当前验证边界与性能基线

- 本轮根目录最终顺序 `dotnet build`：Runtime `0 error / 18 warnings`，Editor `0 error / 48 warnings`；`git diff --check` 通过。
- UnityMCP HTTP 已恢复，但现有 Editor 仍被 `License revoked: Your Unity Personal Version license has been revoked` modal 阻塞。一次 StableId focused job 结果为 `0/unknown` 后已清理；不得把它或旧结果写成当前 focused PASS。fresh `BattleRuntimeSelfCheck` 与 1000 AI 也尚未重跑。
- 最近的真实 1000 全 AI Candidate long300 仍是 `Temp/NTSD_ProductionEntityStress.dispersed1000.slice1-candidate-final300-20260802.json`：Avg `34.7836 ms`（约 `28.75 Hz`）、P95 `50.888 ms`、max `75.992 ms`、`0 B/tick`。尚未达到稳定 30 Hz 门槛。
- T8 默认 `stage.dat` 与 Android 真机验证继续排除在本轮之外。

## 2026-08-02 — Unity 恢复后的 fresh 验收与 DecisionRowContext A/B

- Unity fresh scripts refresh 完成，Console `0 error`；focused tests `202/202 PASS`（`44.908 s`）；full EditMode `483/483 PASS`（`147.809 s`）；fresh `BattleRuntimeSelfCheck` **PASS**。本节证据取代上一节的 license modal 运行时阻塞状态。
- 1000 全 AI、同 seed、`60 warmup + 300 sample` A/B：

| 顺序 / 模式 | Avg ms | P95 ms | P99 ms | Max ms |
|---|---:|---:|---:|---:|
| forward baseline | `32.878289` | `39.47168` | `52.064546` | `224.4373` |
| forward DecisionRowContext enabled | `34.418292` | `43.71326` | `56.953651` | `63.1937` |
| reverse baseline | `32.084814` | `39.6633` | `46.135898` | `63.9063` |

- 三份报告的 overall、RNG 与 slots hashes 全部相同；steady-state `0 B/tick`；teardown `restored=true`、cleanup exception=`0`。
- enabled 计数严格闭合：eligible=`359000`、applied=`359000`、bind=`359000`、gateway=`718000`、identity rows=`2154000`、fallback=`0`、hard failure=`0`。因此行为门通过。
- 性能结论：DecisionRowContext 的 Avg/P95 在 forward 与 reverse 对照下均回退，默认继续关闭，不计作性能收益。baseline Avg 已略高于 30 Hz，但 P95 约 `39.5–39.7 ms`，仍不满足稳定 30 Hz 门禁。
- 下一项按计划处理 Late snapshot `3 -> 1`；T8 默认 `stage.dat` 与 Android 真机验证继续排除。

## 2026-08-02 — Late snapshot `3 -> 1` 已验证并晋升默认

### 合同边界

- 权威 C# `GameTick` 的单实体 Late 连续链不存在中间 runtime snapshot。Unity 本轮只合并 Runtime mirror 发布，不改变 pass 顺序、flush、opoint、RNG、slot 或 lifecycle。
- 普通 active entity 的 `Consolidated` 路径只在 Tail final 发布 1 次；11xx/12xx FrameExit 仍即时 refresh；inactive/free/cleanup 不再做无效发布；transition 内部冗余发布被跳过。
- `LegacyThree` 作为显式 oracle 保留，可用于回归对照。

### 1000 AI A/B

六份报告统一前缀：`Temp/NTSD_ProductionEntityStress.dispersed1000.late-snapshot-ab-*-20260802.json`。

| 样本 | 模式 | Avg / P95（ms） | Snapshot calls |
|---|---|---|---|
| detailed100 | LegacyThree | `31.898607 / 39.055705` | FrameTick/Death/Tail=`100000/100000/100000` |
| detailed100 | Consolidated | `31.038641 / 38.034015` | FrameTick/Death/Tail=`0/0/100000` |

- detailed100 delta 为 Avg `-0.859966 ms`、P95 `-1.02169 ms`；overall=`fa019a38aba6668b7222bf9b61b0400d2cba7b422799bbd0964506a9875450e9`，RNG/slots/events 相同，`0 B/tick` 且清理完整。

| 顺序 | LegacyThree Avg/P95 | Consolidated Avg/P95 |
|---|---|---|
| long300 forward | `31.619902 / 39.4185` | `31.063709 / 37.637035` |
| long300 reverse | `34.39729 / 49.865545` | `32.93091 / 45.88688` |

- 两组 long300 overall=`68af82ba7cdf284d7f62e889e1cd1188e14e9c15ec48d15167cd6c8dcf210388`，RNG/slots/events 相同，`0 B/tick`、`restored=true`、cleanup exception=`0`。

### Promotion 证据与后续

- Architect **PASS**。ordinary world、request empty/default、Window default 与 AI smoke 均已使用 `Consolidated`；显式 `LegacyThree` 仍可选。
- fresh runtime/editor build 均为 `0 error`；干净 Unity refresh 为 `0 error`；focused `135/135 PASS`（job `d63ca30b2e7049a88d718807a6d9820d`）；full EditMode `495/495 PASS`（job `7936ba59d6a44553b3d41429821810d1`）；fresh `BattleRuntimeSelfCheck` **PASS**。
- omitted-mode smoke：`Temp/NTSD_ProductionEntityStress.smoke50.late-snapshot-default-promotion-20260802.json` 为 `SmokePassed`，requested/effective 均为 consolidated，`0 B/tick`、`restored=true`、cleanup exception=`0`。最后已退出 Play Mode、清空 Console，保持 `0 error`。
- 优化已晋升，但 1000 AI 仍未稳定 30 Hz：best forward Consolidated Avg `31.063709 ms`，P95 `37.637035 ms`；reverse P95 `45.88688 ms`。
- 下一热点为 CharacterInput Avg `9.188 ms`（其中 Remaining `5.223 ms`）、CandidateCollect Avg/P95 `3.878/9.405 ms`、Late FrameTick `2.377 ms`、FrameAdvance `2.05 ms`。等待下一 SoA slice 设计。
- T8 默认 `stage.dat` 与 Android 真机验证继续排除。
