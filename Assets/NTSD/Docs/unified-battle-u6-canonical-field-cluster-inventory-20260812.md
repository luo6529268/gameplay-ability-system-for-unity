# U6 canonical 字段簇与热路径盘点（2026-08-12）

> 对应总计划：`unified-battle-lockstep-ecs-server-architecture-plan.md`  
> 阶段：U6，BattleKernel 唯一真值迁移  
> 结论：U6 尚未完成；本文件用于阻止单字段复制、重复 snapshot 和无证据 ECS 化。

## 1. 当前可复现基线

报告：`Temp/NTSD_ProductionEntityStress.u6-current-detail-baseline-20260812.json`

- 1000 个真实生产 GameObject / 逻辑实体，`Combat1000`，全 AI；
- 30 warmup + 60 sample；
- logic average/P95/max：`22.6564 / 27.6597 / 30.8269 ms/tick`；
- 正式 sampled tick：`0 B`，Gen0/1/2 collection 均为 0；
- final lockstep overall hash：`3dc903df6a711ad7385b1014651010468d469126c0253b8435d841d409d55d89`；
- maximum backlog `7`，dropped backlog `30`，因此 U9 稳态门禁尚未关闭。

主要 pass average：

| pass | average ms/tick | 当前判断 |
|---|---:|---|
| CharacterInput | 5.5775 | 最大稳定热区，但包含权威 AI 决策、组合键和同 tick slot 顺序可见性 |
| CandidateCollect | 4.2810 | participant/body/itr、exact pair 与 broadphase 共同组成，不能只复制输入字段 |
| LateEntityUpdate | 2.7706 | 多类型生命周期虚调用，单独搬一个字段不能消除对象热循环 |
| FrameAdvance | 1.8501 | 状态机、帧跳转、速度和表现事件耦合，需完整 writer 簇 |
| CollisionSnapshot | 0.6013 | 原实现只冻结碰撞帧并刷新 runtime；额外复制完整 SoA 已证明不划算 |

CharacterInput 内部已量到：

- snapshot build：约 `0.972 ms/tick`；
- entity input pass：约 `4.596 ms/tick`；
- RemainingAiDecision：约 `2.585 ms/tick`；
- IndexedCanonicalKernel：约 `1.604 ms/tick`；
- ComboUpdate：约 `0.679 ms/tick`；
- UnifiedSnapshotExecutionRowRefresh：约 `0.362 ms/tick`；
- RefreshRuntimeSnapshot：约 `0.225 ms/tick`。

## 2. 已接受的 canonical 簇

### 2.1 Runtime slot registry

已接受：

- `RuntimeSlotTable.Entry` 逐槽对象已删除；
- runtime、occupant、generation 使用页内 `RawRuntimes[]`、`Entities[]`、`Generations[]`；
- claimed 状态只由 `RuntimeSlotAllocator` 保存；
- generation、最低空闲槽、capacity profile 和 handle 失效语义保持不变。

已评估但未晋升：

- pass 遍历仍逐 slot 调用 `GetCurrentOccupant`；
- 页内 occupant 直接升序枚举保持 slot 顺序、hash 与 `0 B/tick`，但两组 180-tick A/B 只改善平均值约 `0.9%`，P95 同时回退约 `0.9%`；
- 该收益未达到“目标 pass 稳定正收益且整体无回退”的晋升条件，候选已完整回退，不在 registry 簇继续做边缘微优化。

### 2.2 CharacterInput 持久输入行

已接受：

- world-owned `BattleCharacterInputStore` 以 slot/generation 绑定 common input state，旧 handle 不能清除
  复用槽的新 generation；
- `DataOrientedCanonical` reader 从 store 取值，Legacy reader 继续走 runtime oracle；
- AI 完整 decision commit、human 公共子段 commit、history、edge、reset、defend lock 均通过同一
  writer/store 生命周期；runtime 只作为 U6 过渡兼容镜像；
- 因 AI kernel 每次整体消费完整输入状态，store 使用连续 `AiDecisionInputState[]`，避免 bit-packed
  多数组重组；这是按字段簇访问模式选择的局部 AoS row，不改变 registry、frame/motion、collision
  等主存储继续使用 Direct SoA 的方案。

实测结论：

- 初版 bit-packed store 造成 detailed average/P95 `24.2364 / 29.8481 ms`；连续行版本恢复到
  `23.7045 / 28.0330 ms`，capture 子段 `0.6284 -> 0.4545 ms`；
- 相对迁移前 detailed average/P95 `23.5199 / 28.3089 ms`，最终版本属约 `0.8%` 平均波动、P95
  略好；因此按“关闭 canonical owner/generation 风险且整体无显著回退”保留，不声明 FPS 优化；
- 183/183 聚焦回归、fresh self-check、同负载 Legacy/DataOriented hash、正式 tick 0 B 与 teardown
  均已通过。完整证据见 `unified-battle-u6-ai-input-writer-20260812.md`。

## 3. 已拒绝的候选

### 3.1 `PendingFlushDestroy` 单字段 store

- Legacy average/P95/max：`21.2927 / 26.1168 / 33.8060 ms`；
- candidate：`23.2539 / 27.4474 / 34.8992 ms`；
- hash 和 0 B 均保持，但 average 回退约 `9.2%`；
- 已完整回退。结论：单字段 SoA 不足以抵消同步和间接访问成本。

### 3.2 CollisionSnapshot -> CandidateCollect 跨 pass 完整复制

候选报告：`Temp/NTSD_ProductionEntityStress.u6-collision-snapshot-candidate-detail-20260812.json`

- baseline CandidateCollect：`4.2810 ms`；candidate：`3.9968 ms`，局部节省约 `0.284 ms`；
- baseline CollisionSnapshot：`0.6013 ms`；candidate：`1.2738 ms`，新增约 `0.673 ms`；
- 两个 pass 合计回退约 `0.389 ms/tick`，总 logic average 约回退 `1.4%`；
- 245 项碰撞/命中回归与最终 hash 保持一致，但性能不满足晋升条件；
- 候选已完整回退，回退后 Unity 编译无新增错误、聚焦测试 `245/245 PASS`、完整 self-check 于 `2026-08-12 21:59:15` fresh PASS。

结论：跨 pass 重新复制对象字段不是 canonical world；只有前序 writer 已直接维护同一存储时，下游读取才可能获得净收益。

### 3.3 Runtime slot 页内 occupant 直接枚举

同配置 A/B 均使用 1000 个真实生产 GameObject / 逻辑实体、全 AI、30 warmup + 180 sample、`data-oriented-canonical`、正式 tick `0 B`：

| 实现 | 运行 | logic average | P95 | max | final hash |
|---|---:|---:|---:|---:|---|
| Legacy 逐 slot | 1 | `22.5199 ms` | `27.3994 ms` | `32.1008 ms` | `4378ba4c...3867063` |
| Legacy 逐 slot | 2 | `22.4999 ms` | `27.6614 ms` | `32.8336 ms` | `4378ba4c...3867063` |
| 页内 direct | 1 | `22.4064 ms` | `27.7706 ms` | `35.0595 ms` | `4378ba4c...3867063` |
| 页内 direct | 2 | `22.2075 ms` | `27.8050 ms` | `31.0222 ms` | `4378ba4c...3867063` |

- 两组 Legacy 平均：average `22.5099 ms`、P95 `27.5304 ms`；
- 两组 direct 平均：average `22.3070 ms`、P95 `27.7878 ms`；
- direct 的 average 改善约 `0.9%`，但 P95 回退约 `0.9%`，且 `CandidateCollect` P95 波动更高；
- 四次运行的最终 hash 一致、正式 tick 均为 `0 B`、teardown 均 `restored=true`；
- 报告：
  - `Temp/NTSD_ProductionEntityStress.u6-page-occupied-traversal-legacy-180-20260812.json`；
  - `Temp/NTSD_ProductionEntityStress.u6-page-occupied-traversal-legacy-180-run2-20260812.json`；
  - `Temp/NTSD_ProductionEntityStress.u6-page-occupied-traversal-candidate-180-run1-20260812.json`；
  - `Temp/NTSD_ProductionEntityStress.u6-page-occupied-traversal-candidate-180-run2-20260812.json`。

结论：这是未达到 U6 晋升门槛的边缘微优化，已完整回退；继续沿该方向不会消除对象式战斗热循环。

## 4. 待迁移字段簇

| 字段簇 | 现有 owner / 重复真值 | 主要读取方 | 迁移前置条件 | 当前决策 |
|---|---|---|---|---|
| registry/occupancy/generation | allocator + paged table | 所有 pass、query、checksum | 保持 slot 升序、generation 和延迟结构变更 | 页内 occupant 枚举已 A/B 拒绝；保持已接受的 page SoA |
| AI sensing/decision/input | generation-owned input store + `NTSDEntityRuntime` 兼容镜像 + unified rows | CharacterInput、AI kernel | frame/motion、同 tick slot 可见性 writer 闭合 | common key/cooldown/combo/history、AI-only coordinate target 与 kind14 boundary 已有持久 store；IndexedCanonical 原子提交、共用 action resolver 与 release-action 事务入口已迁移；Legacy 只允许发布前整批 fallback，发布后 hard breach；frame/motion store 与 unified row 增量维护尚未闭合，不能删除 runtime 镜像或 row refresh |
| collision geometry | entity/frame/physics + role-aware 临时 rows | CollisionSnapshot、CandidateCollect | frame/position/facing/type/attack-exempt 的前序 writer 全部直写 canonical store | 拒绝额外跨 pass copy |
| lifecycle/frame/motion | entity、runtime、transition、health | FrameAdvance、LateEntityUpdate、StageBounds | frame/position/velocity/HP/link 的完整状态转换迁移 | 必须整簇迁移，不做单字段 store |
| link/held/cpoint | runtime + compatibility properties + U5 writer | PreInteraction、Held、Hit、AI | 清除 writer 外直接赋值；验证同 tick 双向关系 | 尚未闭合，不能晋升唯一 SoA |
| presentation publication | BattlePresentation snapshot/commands | RenderDispatch | 只读逻辑快照，不反写战斗真值 | 保持 U6 逻辑迁移边界 |

### 4.1 Frame 写入口前置收口（第八切片）

权威 C# `BattleCore/Frame/FrameRuntime.cs::SetFrameImmediate` 只维护一个 `Entity.Frame`，不存在 Unity
侧 `Frame.N` 与 `Runtime.Frame` 两份可独立变化的战斗真值。Unity 迁移前仍有多个 Character、Weapon、
SpecialAttack、OtherObject、opoint 和 reset 路径直接写 `Frame.N`，并依赖 CharacterInput 末尾的全量
runtime snapshot 回拷补救镜像一致性。

本切片完成以下前置收口：

- 所有生产 frame-id 写入统一进入 `LF2Entity.WriteCurrentFrameId`；
- 该入口原子写入 `Frame.N` 与 `Runtime.Frame`，不改变原调用点、早退、frame data、wait/next 或表现事件顺序；
- `FrameTransistor` 的 wait/next 与 `LF2Health` 的 HP/MP/PP 本来已经在真实 writer 处直接同步 runtime；
- 因此默认 `RefreshRuntimeSnapshotAfterCharacterInput` 退为明确的空迁移边界，不再逐角色重复复制 12 个字段；
- 压力工具保留 `forceFullCharacterInputPostRefresh`，可随时恢复完整旧回拷作为 A/B oracle；
- 这一步只关闭 frame-id 写入口和重复回拷，不把 U3 tick-end `BattleEcsWorld.Frame/Motion` shadow
  晋升为 canonical store，也不宣称整个 frame/motion/lifecycle 字段簇已迁移。

fresh 验证：

- Unity 编译 0 C# error；
- 联合 EditMode job `5c5c9d432a454ccaa76e2a889535f070`：`176/176 PASS`；
- 完整 `BattleRuntimeSelfCheck`：`2026-08-13 01:19:16 PASS`；
- 新默认路径两次 1000 AI average/P95：`20.4753/24.6792 ms`、`20.5460/24.7788 ms`；
- 强制旧回拷两次 average/P95：`22.2049/27.1037 ms`、`21.3191/25.3920 ms`；
- 四次均为 180/180 sampled tick `0 B`、Gen0/1/2 collection 0、final hash
  `752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，hard breach 0，teardown
  `restored=true`。

交错复测确认方向稳定为正，但改善幅度受 Editor 抖动影响。该切片按“移除重复镜像修补、保持同 tick
原子一致且 A/B 无行为差异”保留；U9 的长时稳态、backlog 和 Player 门禁仍未关闭。

## 5. 晋升规则

每个 U6 候选必须同时满足：

1. 唯一 canonical owner 和全部 writer 已列明；
2. C# 权威 pass 顺序、slot 顺序、RNG、生命周期与可观察行为不变；
3. 没有永久双写、tick-end shadow 充当同 tick canonical 或第二份 occupancy 真值；
4. fresh compile、聚焦测试、完整 `BattleRuntimeSelfCheck` 通过；
5. 1000 AI 同配置 A/B 的 hash 相同、sampled tick 0 B；
6. 目标 pass 有稳定正收益且整体没有显著回退，否则完整回退。

## 6. 当前执行顺序

1. registry 页内 occupant 直接枚举已完成 A/B 并按门槛拒绝；
2. `BattleAiInputWriter` 已接管 IndexedCanonical 决策的输入历史、AI-only 状态、flow、RNG 与 coordinate target 原子提交；`BattleCharacterInputWriter` 与 generation-owned `BattleCharacterInputStore` 已接管 world-bound AI/human 的 previous/current key、cooldown、defend lock、combo、edge/history、battle-entry reset、current-key clear 与 kind14 boundary；`BattleCharacterInputActionResolver` 已统一 human/AI 的组合技/direct-frame 算法，并令正式 AI 路径不再做 progress runtime/module 往返镜像；完整证据见 `unified-battle-u6-ai-input-writer-20260812.md`；
3. Legacy oracle/fail-closed 已明确为“发布前整批 fallback、发布后 hard breach”，action resolver 后的 frame、facing、HP/PP、统计、直接动作和速度也已进入 `BattleCharacterActionWriter` 组合事务入口；全部生产 frame-id 写入已通过 `WriteCurrentFrameId` 原子同步兼容镜像，CharacterInput 末尾重复全量回拷已完成交错 A/B 并删除；same-tick AI 所需 frame/motion、input、vital、relation/link projection 均已接入 generation-owned store，完整 26 字段无差别双写已因稳定负收益拒绝；当前不再有 post-CharacterInput unified row 的 Runtime 战斗字段读取，下一步令前序 writer 增量维护 unified row 并保留派生索引一致性；U3 的 tick-end `BattleEcsWorld.Input/Frame/Motion` 仍只是诊断 shadow，不能直接晋升；
4. 再处理 collision、frame/motion 与 lifecycle 整簇；
5. U6 完成后进入 U7 snapshot/restore、U8 worker、U9 1000 AI 正式矩阵。

服务器 S0、T8 默认 `stage.dat` 和 Android 真机仍不在当前执行范围。

## 7. Same-tick AI frame/motion projection（第九切片，2026-08-13）

本切片先验证了“完整 26 字段按原写点双写”的成本，而不是未经测量直接保留：

- 26 字段初版两次 1000 AI average/P95：`22.5875/26.9588 ms`、`22.7249/27.2482 ms`；
- 传值、合并整数坐标与缩短调用链后两次为：`22.2595/27.0605 ms`、`22.2906/26.3677 ms`；
- 相对第八切片两次约 `20.5/24.7 ms` 的基线仍是稳定负收益，所以完整 26 字段候选被拒绝；
- 最终只保留 unified AI row 在 CharacterInput 后实际消费的七字段：`XInt/YInt/ZInt/Vx/Facing/Frame/HitStop`；
- world-owned store 在注册时绑定 runtime slot + generation，在释放、rollback、reset 与 desktop grow 时同步生命周期；runtime 仅持有非序列化的已验证 store/slot 写入绑定；
- 旧 generation 释放后即解除绑定，旧实体继续写 Runtime 不能污染新 generation 的同槽 projection；
- 其他 frame/motion/lifecycle 字段恢复原 Runtime owner，不为尚不存在的生产 reader 支付双写成本。

fresh 验证：

- Unity 编译：0 C# error；
- EditMode job `f107df121c0a4f91a06b15a39388b75b`：`179/179 PASS`；
- `BattleRuntimeSelfCheck`：`2026-08-13 02:07:24 PASS`；
- 两次 1000 AI average/P95/P99/max：`21.0860/25.3055/27.7991/28.5208 ms`、`21.4329/26.0455/29.6440/33.2103 ms`；
- 两次均为 sampled tick `0 B`、Gen0/1/2 collection `0`、final hash `752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`、hard breach `0`、teardown `restored=true`。

该第九切片当时只关闭七字段 AI projection 的 owner/generation 风险；后续第十至十三切片已继续关闭其余 unified row reader，见下文。历史性能结论保持不变。

## 8. CharacterInput canonical projection reader（第十切片，2026-08-13）

`InputHistoryGate/CachedTargetSlot/CoordinateTargetX` 本来已经由 `BattleCharacterInputStore` 按 slot + generation 保存，但 unified row refresh 仍读取 Runtime 兼容镜像。第十切片在同一 store 上增加只包含这三个消费字段的值类型 projection reader，并令 UnifiedAuthority refresh 从该 reader 取值；未新增状态、双写或 fallback。

fresh 验证：Unity 编译 0 C# error；EditMode job `0c6f6b30b779423884423264427be29a` 为 `179/179 PASS`；完整 self-check 于 `2026-08-13 02:16:39 PASS`；1000 AI average/P95/P99/max 为 `21.6311/25.6692/27.6213/28.7059 ms`，正式 tick 0 B、Gen0/1/2 collection 0、final hash `752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`、hard breach 0、teardown `restored=true`。

因此输入 store 已覆盖 unified row 的 history gate 与 AI target 读取；HP/HP3/HPMax/PP、team、state、link、target 与 kill 的后续审计和迁移见第九节。

## 9. Unified row 剩余字段闭合（第十一至十三切片，2026-08-13）

逐字段 writer 审计确认：

- `RelationTeam/LinkState/KillCount/TargetSlotIndex` 的写入分散在初始化、opoint、持有/释放、伤害与链接验证链，但全部最终进入 `NTSDEntityRuntime` 字段；第十一切片将其改为兼容属性，并以低频 `BattleRelationLinkStore` 在原写点同步；
- `HP/HPBound/HP3/PP` 的写入由 `LF2Health`、伤害 writer、恢复、stage 与生命周期路径产生，`LF2Health` 已直接绑定 Runtime；第十二切片以 `BattleVitalStore` 捕获所有原写点，不改变伤害、恢复或资源顺序；
- `state` 不是独立可推算字段，而是当前 `LF2FrameInfo.D.state`。约 30 个生产赋值点既包含 frameId 网关后的替换，也包含直接 DAT frame data 替换；第十三切片将整数 state 同步放进 `LF2FrameInfo.D` setter，并绑定同一个 runtime/frame-motion store，避免逐调用点遗漏；
- 三个 store 都由 world 在 register/release/reset/grow 边界按 slot + generation 管理。旧 generation 解除绑定后继续写旧 Runtime，不能污染复用槽；热写测试均为 0 B；
- `TryRefreshAiUnifiedSnapshotExecutionRowAfterCharacterInput` 现在只用 runtime 引用做 store owner/slot 身份校验，row 的战斗数据均从 `BattleCharacterInputStore`、`BattleFrameMotionStore`、`BattleRelationLinkStore` 与 `BattleVitalStore` 读取，不再直接读 Runtime 战斗字段。

fresh 证据：

- relation/link：EditMode job `8a46cf71e9de4205b96a62282f91248f`，`182/182 PASS`；self-check `2026-08-13 02:30:12 PASS`；1000 AI average/P95/P99/max `21.6142/25.5854/27.8450/29.8295 ms`；
- vital：EditMode job `38fa517fd6524e62bedf4f277a454b10`，`185/185 PASS`；self-check `2026-08-13 02:36:55 PASS`；两次 1000 AI average/P95 `22.0395/26.2780 ms`、`21.7829/25.6845 ms`；
- DAT state 最终闭合：Unity 编译 0 C# error；EditMode job `f8cffb8409c241e3be0a973f684b73b7`，`185/185 PASS`；self-check `2026-08-13 02:43:16 PASS`；1000 AI average/P95/P99/max `22.0254/26.1165/29.5433/30.4045 ms`；
- 所有正式 sampled tick 均为 0 B、Gen0/1/2 collection 0、final hash `752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`、authority success、hard breach 0、teardown `restored=true`。

这关闭的是 unified row reader 的 Runtime 依赖，不是 U6 完成声明。post-CharacterInput row refresh 仍逐实体把四个 store projection 复制进 unified rows，并维护 team/HP/X 变化触发的派生索引；下一切片必须先给这个 refresh 建立等价的增量写入与 fail-closed 对照，不能直接删除。完整 collision、frame/motion/lifecycle 对象 shell 退化以及 U9 长时矩阵仍未完成。

## 10. Post-CharacterInput unified row 增量提交（第十四切片，2026-08-13）

本切片先验证了“在四个 store 原写点立即修改已发布 unified row”的候选。该候选不仅让 1000 AI average 回退到 `42.2450 ms/tick`，还把原本只在当前实体 CharacterInput 结束后可见的最终状态提前暴露给同一 pass，因此按行为边界和性能双重失败完整撤回。

最终保留实现新增 world-owned、无静态可变状态的 `BattleAiUnifiedRowPublisher`：

- 四个 generation-owned store 继续保持各自 canonical 数据所有权；它们的原写入网关只向 publisher 暂存 slot + generation、dirty mask 和最终值，不直接修改已发布 row；
- 每个实体完成 CharacterInput 后，由 `TryCommitPending` 在原有可见边界一次提交变化字段。publisher 使用旧 row 与最终 row 比较 X/team/HP/role，只有派生产品实际变化时才请求重建 role index 或 team summary；
- 没有发生相关写入的实体不再捕获四个 projection，也不复制 19 个字段，只验证当前 slot/generation；generation 不匹配在 authority 已提交后仍是 hard breach；
- 强制 full refresh oracle 仍调用完整 canonical capture，并通过 `TryDiscardPending` 丢弃本实体的暂存值，防止 oracle 真值被 pending 再覆盖；
- `ValidateIncrementalAiUnifiedRowForDiagnostics` 默认关闭。开启时，它从四个 canonical store 重读 19 个字段逐项核对提交后的 row，只验证、不反写；测试覆盖 30 tick × 4 实体共 120 次校验。

fresh 证据：

- Unity 编译 0 C# error；EditMode job `33cc0f620af24c4ba48e3b7a5c4fc3cd` 为 `185/185 PASS`；完整 self-check 于 `2026-08-13 03:20:07 PASS`；
- 增量 production smoke average/P95/P99/max 为 `21.5776/25.7700/28.6098/29.9695 ms`；强制 full refresh oracle 为 `22.7332/26.9240/29.9166/33.4480 ms`；两者 final battle parity hash 均为 `752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`；
- 两条路径均为正式 sampled tick 0 B、Gen0/1/2 collection 0、hard breach 0、authority success、teardown `restored=true`；
- capacity detail 中 `CharacterInput/AI/UnifiedSnapshotExecutionRowRefresh` average/P95 为 `0.3527/0.3621 ms`，前序基线为 `0.3618/0.3704 ms`。该差距很小，只证明本次所有权迁移没有形成稳定显著负收益，不宣称主要帧率提升。

第十四切片关闭了 post-CharacterInput 19 字段全量 projection 复制，但没有关闭整个 U6。当时初始 unified snapshot 仍读取 Runtime 战斗字段；该点由第十五切片继续处理，见下节。

## 11. UnifiedAuthority 初始 canonical row 捕获（第十五切片，2026-08-13）

每 tick 构造 UnifiedAuthority 初始 snapshot 时，旧实现仍直接从 `NTSDEntityRuntime/LF2Entity` 读取与四个 canonical store 重复的 19 个战斗字段。第十五切片新增正式 authority 专用捕获路径：

- `InputHistoryGate/CachedTargetSlot/CoordinateTargetX` 来自 `BattleCharacterInputStore`；
- `X/Y/Z/Vx/Facing/Frame/State/HitStop` 来自 `BattleFrameMotionStore`；
- `RelationTeam/LinkState/KillCount/TargetSlot` 来自 `BattleRelationLinkStore`；
- `HP/HPBound/HP3/PP` 来自 `BattleVitalStore`；
- registry 已在外层提供并验证 slot/generation，因此最终 reader 直接接收 `RuntimeEntityHandle`，不再为同一实体重复进行四次 Runtime owner 引用解析；旧 generation 在 slot 复用后会被四个 store 全部拒绝；
- Runtime/实体当前仍只用于 stable identity、object id/type、两套 boundary flag 以及 first-ten move-mode 兼容数据；Legacy、shadow 和强制 full oracle 的原捕获路径没有删除。

报告与 fresh 证据：

- `ProductionEntityStressReport` 新增 `aiUnifiedSnapshotExecutionCanonicalInitialCaptureCount`；正常 authority exact-closure 必须等于 committed pass × requested entity count，pre-commit rollback 则按实际 committed pass 计算；
- fresh Unity 编译 0 C# error；EditMode job `1537682b724944f0ad4838ee2fe890f9` 为 `421/421 PASS`；完整 self-check 于 `2026-08-13 03:48:50 PASS`；
- 最终两次 1000 AI average/P95/P99/max 为 `21.8961/25.8188/29.3556/30.4394 ms`、`21.7277/26.1409/29.4532/30.1781 ms`；两次 canonical initial capture 都是 `209000/209000`，authority success；
- 两次均为正式 sampled tick 0 B、Gen0/1/2 collection 0、final battle parity hash `752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`、hard breach 0、teardown `restored=true`。

两轮均值相对第十四切片约 +1.1%，没有形成可声明的性能收益，也未构成稳定显著回退；本切片按 canonical ownership 与 generation 安全性保留。U6 下一步仍需处理 identity/object type、boundary flags、first-ten move-mode 等初始对象读取，以及完整 frame/motion/lifecycle 热循环和对象 shell 退化；U9 长时 Player 矩阵仍未执行。

## 12. Directional boundary canonical publication（第十六切片，2026-08-13）

审计确认四个方向阻挡位的正式生产写入与消费边界已经闭合：注册到 `SimulationWorld` 的 kind14 命中统一经过 `BattleBoundaryWriter.TryApplyKind14DirectionalBlock`，Character、Entity、Weapon mechanics 消费并清零后统一调用 `SyncConsumedFlags`。其他直接字段写入只存在于无 world 兼容回退、runtime reset 和测试夹具，不能成为 UnifiedAuthority 正式路径的数据源。

本切片据此完成：

- `BattleCharacterInputAiProjection` 新增 canonical decision boundary flags；`RuntimeEntityHandle` reader 继续执行 slot + generation 校验；
- `BattleCharacterInputStore.SetBoundaryFlags` 只在值变化时向 `BattleAiUnifiedRowPublisher` 暂存 dirty boundary，不提前修改已发布 row；
- publisher 在当前实体 CharacterInput 结束后的原子提交点，同时更新 input row、published decision boundary 与 sensing boundary；
- 决策编码保持 `X+/X-/Z+/Z- = 1/2/4/8`，sensing 编码保持 `Z-/Z+/X-/X+ = 1/2/4/8`，转换为无状态纯函数，不改变权威语义；
- UnifiedAuthority 初始 snapshot 从 input store 读取 decision 编码并生成 sensing 编码，不再读取 Runtime 的四个 bool；
- 强制 full refresh 仍从 Runtime 重建两套编码并丢弃 pending，作为 exact oracle；增量诊断同时核对 store、row 和两套 published boundary 数组。

fresh 证据：

- 本地 `Assembly-CSharp.csproj` 与 Unity fresh 编译均为 0 C# error；
- 改动直接相关测试 `78/78 PASS`，扩大聚焦 job `0e70bb9e8f8540a3a2d155e272834084` 为 `421/421 PASS`；
- `BattleRuntimeSelfCheck` 于 `2026-08-13 04:07:47` PASS；
- 1000 AI 增量路径 average/P95/P99/max 为 `22.1538/26.7865/29.1554/33.1196 ms`；强制 full Runtime oracle 为 `23.1081/27.3117/30.1937/31.8571 ms`；
- 两条路径的 battle parity hash 都是 `752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，lockstep overall hash 都是 `4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`；
- 两条路径均为 180 个正式 sampled tick、0 B/tick、Gen0/1/2 collection 0、authority success、hard breach 0、teardown `restored=true`；增量路径 canonical initial capture 为 `209000/209000`。

增量相对 full oracle 的平均值约低 4.1%，但这是 Editor 短样本，只能支持“未观察到负收益”与“避免重复 Runtime 重建”的方向，不能宣称稳定性能提升。U6 剩余初始对象读取缩小为 stable identity、object id/type 与 first-ten move-mode；更广的 frame/motion/lifecycle 热循环和对象 shell 退化仍未完成。

## 13. Identity / object type canonical store（第十七切片，2026-08-13）

审计确认正式生产身份变化不只发生在 register：`ObjectId` 会在 DAT 身份切换时改变，`FrameCache.Load/Clear` 会改变当前 wrapper 与由 immutable DAT catalog 解析出的 object type，OID 51/52 dormant partner reset 还会保存并恢复 stable id。因此不能只在注册时抓一份静态数组，也不能继续让 UnifiedAuthority 每 tick 走对象图解析。

本切片完成：

- 新增 world-owned `BattleIdentityStore/BattleIdentityWriter`，以 `RuntimeEntityHandle(slot,generation)` 绑定 owner，并连续保存 `StableId/ObjectId/DataObjectType`；register、release、reset 与 grow 生命周期和其他 canonical stores 保持一致；
- `LF2Entity.ObjectId` 与 stable-id lifecycle gateway 在原写点同步 store；`LF2FrameCache` 通过无委托 observer 在 `Load/Clear` 完成后通知 owner，重新解析 DAT type；未注册对象和独立 cache 仍保持原兼容行为；
- OID 51/52 dormant partner reset 不再直接旁路写 `Runtime.StableId`，而是通过 lifecycle gateway 恢复，使 runtime 兼容镜像和 identity store 原子一致；
- UnifiedAuthority 初始 row 的 `Identity/ObjectId/DataObjectType` 从 identity handle reader 捕获；强制 full refresh、Legacy 与 shadow 仍从 Runtime/对象构造独立 oracle；增量校验同时核对 identity store；
- 测试覆盖当前 generation 捕获、slot 复用后的旧 handle 拒绝、ObjectId 原写点更新与 FrameCache DAT type 更新。

fresh 证据：

- 本地对照工具与 Unity fresh 编译均为 0 error；
- identity/input 定向测试 `22/22 PASS`；扩大聚焦 job `4105b6d6db164f47be4d66c875624645` 为 `423/423 PASS`；
- `BattleRuntimeSelfCheck` 于 `2026-08-13 04:32:40 PASS`；
- 1000 AI 增量路径 average/P95/P99/max 为 `22.0894/26.2908/28.6278/29.6332 ms`；强制 full Runtime oracle 为 `23.0996/27.4190/30.4386/31.8757 ms`；
- 两条路径均为 180 个 sampled tick、0 B/tick、Gen0/1/2 collection 0、canonical initial capture `209000/209000`、authority success、hard breach 0、teardown `restored=true`；battle parity hash 均为 `752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，lockstep hash 均为 `4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`。

增量平均值相对 full oracle 约低 4.4%，仍是 Editor 短样本，不宣称稳定性能晋升。本切片关闭的是初始 identity/object-type 对象热读取；first-ten move-mode 与更广的 frame/motion/lifecycle 对象式热循环仍未闭合，U6/U9 仍未完成。

## 14. First-ten move-mode canonical row reuse（第十八切片，2026-08-13）

UnifiedAuthority 构造每 tick 初始 row 时，first-ten move-mode 产品仍在同一 slot 上通过 `Hp(entity)`、`IsLivingCharacterDat(entity)`、`X(entity)` 与 `Z(entity)` 二次读取对象/Runtime。第十八切片把顺序调整为先完成 canonical row 捕获，再从该 row 构造 first-ten 产品；slot/generation 和 top/second 选择顺序不变。

post-CharacterInput 的失效检查不能只读取初始 row：若当前 DAT 身份发生变化，旧实现会让 first-ten 产品失效。最终实现从当前 generation 的 `BattleIdentityStore` 读取 DAT type，并从 publisher 在原可见边界提交后的 row 读取 HP/X/Z，既移除对象字段读取，也保留原失效语义。Legacy、shadow、deep validator 与强制 full Runtime oracle 仍为独立对照。

fresh 证据：

- Unity 编译 0 C# error；first-ten/authority/input 聚焦 `80/80 PASS`，扩大聚焦 job `3410e3a3d4564fa588a3c4a30cf6cfdb` 为 `423/423 PASS`；
- `BattleRuntimeSelfCheck` 于 `2026-08-13 04:43:39 PASS`；
- 1000 AI 增量路径 average/P95/P99/max 为 `21.8479/26.0178/27.8500/29.0220 ms`；强制 full Runtime oracle 为 `23.1703/28.1723/31.4818/32.4411 ms`；
- 两条路径均为 180 个 sampled tick、0 B/tick、Gen0/1/2 collection 0、canonical initial capture `209000/209000`、authority success、hard breach 0、teardown `restored=true`；battle parity 与 lockstep hash 与前序切片完全一致。

约 5.7% 的短样本差距不作为稳定性能晋升声明。至此 UnifiedAuthority 初始 row 及 first-ten 产品所需的战斗字段、boundary 与 identity/type 均从 canonical stores/row 读取；U6 剩余工作转向实体边界遍历、派生索引维护以及更广的 frame/motion/lifecycle 对象式热循环，U6/U9 仍未完成。

## 15. Active-slot 缓存遍历负实验（第十九切片候选，2026-08-13）

为验证 `CharacterInput` 的 slot 边界遍历是否值得迁移，候选实现曾在 pass 前缓存 generation-safe canonical active slots，再按 slot 升序消费。该候选没有改变输入、RNG、实体生成或释放语义，但两次 1000 AI average 分别为 `22.5868 ms` 与 `22.4815 ms`，没有优于第十八切片 `21.8479 ms` 的稳定证据。

按照本文件“目标 pass 必须有稳定正收益”的晋升门槛，候选实现、测试分支和诊断开关已完整撤回。结论不是“实体边界已经迁移”，而是“为单个 pass 增加缓存副本无法消除对象式热循环”；后续若处理实体边界，必须由多个连续 pass 共享同一个 canonical occupancy 产品，不能再为单 pass 重建第二份列表。

## 16. Exact-character FrameAdvance 尾链负实验（第二十切片候选，2026-08-13）

权威 `FrameAdvance` 链核验后，候选仅对 exact `LF2Character` + Character DAT 保留 `SimTransit`，跳过空的 `SimTU` 与其后的完整 runtime snapshot；派生角色和非 Character DAT 继续走旧链。聚焦测试与强制 Legacy A/B 证明行为、hash 与零 GC 门禁一致。

压力计数进一步证明该候选不是未命中：默认路径在 1000 实体、30 warmup + 180 sampled tick 中累计命中 `210000` 次。即便如此，默认候选与强制 Legacy 的 average/P95/P99/max 仅分别为 `21.6491/25.8994/28.0891/28.9517 ms` 和 `21.6680/25.9537/27.9025/29.0856 ms`，平均差仅约 `0.019 ms/tick`（约 `0.09%`），属于测量噪声。

因此该候选及其压力报告字段、菜单、测试和诊断开关已完整撤回。撤回后的 fresh 证据：

- `dotnet build Assembly-CSharp.csproj --no-restore`：0 error；Unity refresh ready；
- EditMode job `88cbeb995d2141b2a7d0ff5117a065eb`：`423/423 PASS`；
- `BattleRuntimeSelfCheck`：`2026-08-13 05:35:09 PASS`；
- 1000 AI average/P95/P99/max：`22.1189/26.6252/29.6743/34.9116 ms`，180/180 sampled tick、0 B、Gen0/1/2 collection 0；
- battle parity hash `752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`、lockstep hash `4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`，teardown `restored=true`。

该结果说明 `FrameAdvance` 的空虚调用和快照尾链不是当前 1000 AI 的量级瓶颈。下一候选必须来自 fresh detail timing 中占比足够大的完整字段簇或跨 pass 共享产品，而不是继续删除微小虚调用。

## 17. Collision formal slot 稠密代际映射（第二十一切片，2026-08-13）

`BruteForceSceneQuery` 的 loose 与 role-aware 正式候选链原先每 tick 清空并重建
`Dictionary<int,int>` 与 `HashSet<int>`，再在 body query、authority pair 转换、direct/sweep broadphase
中反复按 runtime slot 查询 participant ordinal。runtime slot 已是 world-owned 稠密整数域，因此本切片把正式路径改为预分配的
`slot -> ordinal` 数组和代际戳数组：开始新 tick 只递增 stamp，不清空整张表；重复 slot、缺失 slot、越界 slot
仍 fail closed，实体、participant、pair、candidate 与 RNG 顺序均未改变。旧 dictionary/hashset 路径保留为
`ForceLegacyFormalSlotMapForDiagnostics` A/B oracle，不参与默认正式路径。

同配置 1000 AI、30 warmup + 180 sample 的两轮普通性能交错 A/B：

| 运行 | 实现 | average | P95 | P99 | max | 路径命中 |
|---|---|---:|---:|---:|---:|---:|
| 1 | dense stamped | `21.7153` | `25.9680` | `29.4160` | `32.6362` | `210000` dense / `0` legacy |
| 1 | legacy hash | `21.8570` | `26.2897` | `28.7545` | `33.1086` | `0` dense / `210000` legacy |
| 2 | dense stamped | `21.7069` | `25.9153` | `27.5258` | `28.9913` | `210000` dense / `0` legacy |
| 2 | legacy hash | `21.8472` | `26.1779` | `28.4745` | `29.2453` | `0` dense / `210000` legacy |

带 nested timing 的 target-pass A/B 进一步确认收益来自目标链，而不是其他 pass 抖动：

- `CandidateCollect` average：`3.8481 -> 3.6975 ms`，减少约 `0.151 ms`（约 `3.9%`）；
- `ParticipantBodyItrBuild`：`1.0653 -> 0.9834 ms`；
- `DirectBroadphase`：`0.8666 -> 0.8065 ms`；
- detailed 总 tick：`24.2803 -> 24.2213 ms`，没有整体显著回退。

所有 A/B 均为 sampled tick `0 B`、Gen0/1/2 collection `0`，final battle parity hash
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`、lockstep hash
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063` 一致，teardown
`restored=true`。Unity EditMode `425/425 PASS`，`BattleRuntimeSelfCheck` fresh `PASS`。该切片按
“目标 pass 稳定正收益且整体无显著回退”保留；收益约 `0.14 ms/tick`，不能单独解释或解决 1000 AI
整体性能目标。

## 18. LateEntityUpdate pass-stable opoint factory（第二十二切片，2026-08-13，已保留）

fresh detailed 基线显示 `LateEntityUpdate/TailAndQueuedFlush` average 为约 `0.7975 ms/tick`。
权威 C# 要求每个 slot 的 late tail 与其结构生成在当前实体边界立即完成，不能把 1000 次 flush
合并成 tick-end flush。Unity 旧路径却在每个边界通过 `LF2ObjectPointFactory.Instance` 重新进入
`MMSingleton`；同一个 `LateEntityUpdateAll` pass 内工厂对象本身不会变化。

该切片只缓存 pass-stable factory，不改变任何结构时序：

- 每个实体原有的 flush 调用位置、次数和顺序保持不变；
- 默认路径在一个 late pass 内最多解析一次工厂，仍在每个实体边界调用同一实例的 `FlushTasks`；
- 临时 Legacy A/B 入口在测量阶段恢复逐 flush 的旧单例入口，压力报告记录 factory resolve 与 flush 次数；
- self-check 新增两实体夹具：正式路径必须为 2 次 flush / 1 次解析。

Unity 重新打开后完成 A/B 验证：

- Unity Console 为 0 个编译错误；新增 A/B 配置聚焦测试 `1/1 PASS`，压力工具整类回归
  `238/238 PASS`；完整 self-check 于 `2026-08-13 08:15:50 PASS`；
- 两轮普通 1000 AI 交错 A/B 中，正式路径 average/P95 分别为
  `22.1233/26.1083 ms`、`22.7756/28.0635 ms`，Legacy 为
  `22.4914/27.0717 ms`、`22.3140/27.3173 ms`。总 tick 方向互有波动，合并差距不足 1%，不支持
  整体 FPS 收益声明，也没有显著回退；
- 两轮开启 nested timing 的 1000 AI 交错 A/B 中，目标
  `LateEntityUpdate/TailAndQueuedFlush` 从 Legacy `0.8388`、`0.8422 ms/tick` 稳定降至正式路径
  `0.7708`、`0.7851 ms/tick`，分别改善约 8.1% 与 6.8%；完整 Late pass 也分别从
  `3.1250`、`3.1327 ms/tick` 降至 `3.0526`、`3.0847 ms/tick`；
- 每轮 warmup + sample 共 210 tick。正式路径解析工厂 `210` 次，Legacy 为 `210210` 次；两条路径
  均执行 `210000` 次 flush。全部运行的 parity hash、lockstep hash 完全一致，正式 tick 0 B、
  Gen0/1/2 collection 0、zero-GC gate PASS、teardown 完整恢复。

该切片按“目标子段两轮稳定正收益、行为与结构边界等价、总体无显著回退”晋升。晋升后删除了只为
A/B 存在的 Legacy request、菜单、runtime override 与恢复接线，正式代码只保留 pass-stable 缓存、
flush/resolve 诊断计数和两实体 self-check。清理后再次完成 Unity fresh 编译 0 C# error、压力工具整类
`237/237 PASS`、self-check `2026-08-13 08:28:47 PASS`；最终普通 1000 AI 的
average/P95/P99/max 为 `21.9488/26.3602/28.1020/29.0273 ms/tick`，工厂解析 `210` 次、flush
`210000` 次，正式 tick 0 B、Gen0/1/2 collection 0、两套 hash 不变、teardown 完整恢复。该收益约为
`0.06 ms/tick`，不能扩大为 U6 或 U9 完成。

## 19. CharacterInput canonical progress dirty commit（第二十三切片，2026-08-13，已保留）

审计确认 `BattleAiInputWriter.CommitIndexedCanonicalDecision` 会在 `ComboUpdate` 前把完整 AI 输入状态提交到
`BattleCharacterInputStore`；随后 DataOriented action resolver 从同一 generation-owned row 捕获 17 个进度字段，
执行与权威 C# 相同顺序的 combo/direct action，再把 17 字段无条件回写到 canonical store 和 Runtime 兼容镜像。
普通 AI tick 中大部分实体没有改变这些字段，因此旧链会重复写入内容完全相同的 row；这不是战斗规则需要的可见边界。

本切片完成以下最小修改：

- `BattleCharacterInputActionState.ContentEquals` 对完整 17 字段进行值比较，不只检查部分热点字段；
- DataOriented 路径记录原始 canonical row，执行原 combo/direct action 后，仅在任一字段变化时调用原完整
  `CommitProgressState`；未变化时不写 store，也不写 Runtime 兼容镜像；
- canonical capture 失败的兼容回退仍执行原完整提交，不能因缺失 store 而跳过 Runtime 写入；
- AI decision、combo/direct action、slot 升序、generation 校验、RNG、输入边沿和 Runtime 兼容结果均未改变；
- 压力报告新增 commit/skip 计数，临时强制 Legacy 无条件回写开关只用于 A/B，候选晋升后已删除；
- 聚焦测试分别覆盖“未变化必须 skip”和“变化必须完整 commit”。

两轮相同配置的 1000 AI、30 warmup + 180 sample 细分交错 A/B：

| 轮次 | 路径 | total average | P95 | P99 | max | `AI/ComboUpdate` average | `EntityInputPass` average | commit / skip |
|---|---|---:|---:|---:|---:|---:|---:|---:|
| 1 | dirty commit | `23.9809` | `28.4114` | `30.0032` | `31.1162` | `1.0611` | `5.3538` | `35371 / 173629` |
| 1 | Legacy unconditional | `24.0589` | `28.0436` | `31.3033` | `35.1214` | `1.1100` | `5.4276` | `209000 / 0` |
| 2 | dirty commit | `24.0303` | `28.5234` | `30.7447` | `34.7687` | `1.0639` | `5.3573` | `35371 / 173629` |
| 2 | Legacy unconditional | `24.0825` | `28.4681` | `31.3146` | `33.1498` | `1.1028` | `5.4007` | `209000 / 0` |

两轮均跳过 `173629/209000` 次未变化回写，约 `83.1%`。目标子段 average 稳定正向：
`AI/ComboUpdate` 约改善 `4.4%` 与 `3.5%`，`EntityInputPass` 也有小幅正收益；总 tick average 仅改善约
`0.2%～0.3%`，P95 方向存在 Editor 抖动，因此不能宣传为可见 FPS 提升。四次运行的 battle parity hash 均为
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，lockstep hash 均为
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`；正式 sampled tick 0 B、
Gen0/1/2 collection 0、zero-GC gate PASS、teardown `restored=true`。

晋升并移除临时 A/B 分支后的 fresh 门禁：

- `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 本地构建均为 0 error；Unity fresh refresh ready，
  Console 无项目 C# error；
- 聚焦 EditMode job `76b2e50bf55140ba9950e6665b4518bf`：`2/2 PASS`；压力工具整类 job
  `973f0c3c233145babbeaa0b48483e232`：`237/237 PASS`；
- `BattleRuntimeSelfCheck`：`2026-08-13 08:56:01 PASS`；
- 最终普通 1000 AI 报告
  `Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-performance-smoke.json`：180 sampled tick，
  average/P95/P99/max `22.0629/26.5426/29.7178/33.2235 ms/tick`，commit `35371`、skip `173629`，
  factory resolve `210`、flush `210000`，0 B、三代 collection 0、两套 hash 不变、teardown 完整恢复。

该切片按“完整字段簇、目标子段两轮稳定正收益、总 tick 无显著回退、确定性与零 GC 门禁通过”保留。
它仍不足以关闭 U6 或 U9；下一候选必须继续来自占比足够大的完整 frame/motion/lifecycle 字段簇或跨 pass
共享 canonical 产品，不能回到单字段同步或微小虚调用删减。

## 20. CharacterInput AI projection dirty publication（第二十四切片，2026-08-13，已保留）

`BattleCharacterInputStore.CommitFull(includeHistory: true)` 在每个 AI 实体完成正式 decision commit 后，原本都会把
`InputHistoryGate/CachedTargetSlot/CoordinateTargetX` 三字段 projection 发布给
`BattleAiUnifiedRowPublisher`，即使三字段与当前 canonical row 完全相同。publisher 随后把每个发布标成 pending，
`RefreshAiUnifiedSnapshotExecutionRowAfterCharacterInput` 再逐实体消费；因此“值未变化”仍会制造跨 pass 派生工作。

本切片保持完整 AI 输入 row、Runtime 兼容镜像、decision/combo/action、slot/generation、RNG 与 pass 顺序不变，只在
三个 projection 字段中至少一个变化时发布 pending；未变化时跳过发布。`SetInputHistoryGate`、`SetCoordinateTarget` 与
`ResetInputState` 也使用同一完整三字段比较，不能因单入口优化而漏掉其他正式 writer。压力报告保留 publish/skip 计数，
临时强制 Legacy 无条件发布开关仅用于 A/B，晋升后已删除。聚焦测试覆盖“未变化不发布”与“目标槽变化后必须发布并更新 row”。

两轮相同配置的 1000 AI、30 warmup + 180 sample 细分交错 A/B：

| 轮次 | 路径 | total average | P95 | P99 | max | `AI/UnifiedSnapshotExecutionRowRefresh` average | `EntityInputPass` average | publish / skip |
|---|---|---:|---:|---:|---:|---:|---:|---:|
| 1 | dirty publication | `24.0397` | `28.4961` | `34.8786` | `36.6328` | `0.1948` | `5.0886` | `10417 / 198583` |
| 1 | Legacy unconditional | `24.1575` | `28.1479` | `30.4066` | `31.3906` | `0.3083` | `5.3533` | `209000 / 0` |
| 2 | dirty publication | `23.9553` | `28.0868` | `30.7155` | `31.7848` | `0.2101` | `5.1233` | `10417 / 198583` |
| 2 | Legacy unconditional | `24.2431` | `30.3735` | `31.7302` | `32.5648` | `0.2907` | `5.3092` | `209000 / 0` |

两轮均跳过 `198583/209000` 次未变化发布，约 `95.0%`。目标 row refresh average 分别改善约 `36.8%` 与
`27.7%`，`EntityInputPass` 也稳定改善；总 tick average 分别改善约 `0.5%` 与 `1.2%`，P95/P99 仍受 Editor
尖峰影响，不扩大为稳定 FPS 声明。四次运行的 battle parity hash 均为
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，lockstep hash 均为
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`；正式 sampled tick 0 B、
Gen0/1/2 collection 0、zero-GC gate PASS、teardown `restored=true`。

晋升并清理临时 A/B 分支后的 fresh 门禁：

- `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 本地构建均为 0 error，Unity fresh refresh ready；
- 聚焦 EditMode job `833a9eb2301e475f8f5811db5171995d`：`3/3 PASS`；压力工具整类 job
  `c62d5f81432749e3b278bb6b17059b3f`：`237/237 PASS`；
- `BattleRuntimeSelfCheck`：`2026-08-13 09:20:55 PASS`；
- 最终普通 1000 AI 报告
  `Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-performance-smoke.json`：180 sampled tick，
  average/P95/P99/max `23.2879/28.8326/34.7102/50.2735 ms/tick`，publish `10417`、skip `198583`，
  progress commit `35371`、skip `173629`，0 B、三代 collection 0、两套 hash 不变、teardown 完整恢复。

该切片按“完整派生字段簇、两轮目标子段稳定正收益、总 tick 无显著回退、确定性与零 GC 门禁通过”保留。
最终普通样本的单个 max 尖峰不用于性能晋升，也不否认 P95 仍低于 30 Hz 预算；U6/U9 依旧未完成。下一候选继续
处理占比更大的完整 frame/motion/lifecycle 字段簇或跨 pass canonical 产品。

## 21. AI kernel 自身行值缓存负实验（第二十五切片候选，2026-08-13，已撤回）

fresh detail timing 中 `CharacterInput/AI/RemainingAiDecision/IndexedCanonicalKernel` 仍是 AI 子链热点。
候选尝试在一次 `AiDecisionKernel.TryEvaluate` 开始时把自身 slot 的 X/Y/Z、HP/PP、state/frame、
link/target、oid/facing 等 16 个只读值捕获到 value-type context；目标行扫描仍读取原 SoA，RNG、分支、
slot 顺序和提交边界均不改变。第一版按值传递扩大后的 context，确认存在结构体复制开销后又改为 `in`
只读引用；两版均通过本地编译和 AI 决策聚焦测试，最终候选采用只读引用版本进入压力测量。

相同配置 1000 AI、30 warmup + 180 sample 的相邻代码版本证据如下：

| 版本 | total average | P95 | P99 | max | `IndexedCanonicalKernel` average | `RemainingAiDecision` average |
|---|---:|---:|---:|---:|---:|---:|
| 候选前 fresh 基线 | `23.9553` | `28.0868` | `30.7155` | `31.7848` | `1.6963` | `2.8615` |
| 自身行缓存候选 1 | `24.8994` | `29.9113` | `32.9465` | `35.0412` | `2.2131` | `3.3976` |
| 自身行缓存候选 2 | `24.9812` | `28.9008` | `31.3511` | `34.2917` | `2.2668` | `3.4735` |
| 撤回后复测 1 | `30.4249` | `43.7647` | `48.2767` | `50.6851` | `1.9534` | `3.3025` |
| 撤回后复测 2 | `25.2258` | `30.2563` | `33.4044` | `37.5785` | `1.8024` | `3.0326` |

撤回后第一轮总 tick 存在明显 Editor 环境尖峰，不能用于总体性能比较；但目标 kernel 在候选两轮均为
`2.21～2.27 ms`，高于候选前 `1.70 ms`，也高于撤回后两轮 `1.80～1.95 ms`。说明为每个 AI
无条件复制完整自身字段簇的固定成本，大于本场景中省掉的 SoA 数组读取，属于可重复的目标子段负优化。

所有候选与撤回后运行均保持 battle parity hash
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`、lockstep hash
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`，sampled tick 0 B、
Gen0/1/2 collection 0、fallback 0、teardown `restored=true`。候选已完整撤回；
`AiDecisionKernel.cs` 工作区 blob 与 HEAD blob 均为 `b5abe691c4fc7b591ba895147795efbd70c33ef9`，
该文件 `git diff=0`。撤回后本地 `Assembly-CSharp.csproj` 0 error，Unity fresh refresh ready，聚焦 job
`8d86bdf60caf45a8803feea0f303aa82` 为 `36/36 PASS`。

该实验明确拒绝“无条件复制宽自身 row 来换取局部数组读取”的方案。下一候选必须减少已有工作量或复用
已经存在的 canonical 产品，不能先新增一份每 AI 固定成本再期待缓存命中抵消。

## 22. UnifiedAuthority 跨 tick 滚动 canonical row（第二十六切片，2026-08-13，已保留）

### 22.1 目标与边界

第十五至十八切片已经让 UnifiedAuthority 从 generation-owned canonical stores 构建完整 row，但每个 tick 仍会按 runtime slot 容量扫描并重新捕获全部 active row。第二十六切片复用上一 tick 已发布的同一份 canonical row：publisher 在其余 pass 中继续按 slot + generation 累积最终字段值，下一次 `CharacterInput` 开始时只提交 dirty slot，并按聚合后的 role/team 变化最多各重建一次派生索引。

以下安全边界保持不变：

- capacity、occupancy epoch 或 published generation 不一致时禁止滚动，回到完整重建；
- publisher 仍只在原 canonical writer 写入后记录最终值，不提前改变同 tick 可见边界；
- post-CharacterInput 仍按 slot 升序提交当前实体的同 tick 变化；
- first-ten move-mode 产品在滚动起点从 canonical row 重建，不读取 Transform 或表现状态；
- 强制 full rebuild 仅作为默认关闭的 A/B oracle，不改变正式默认路径；
- 结构变化、异常、过期 generation 和 publisher 失效都 fail closed，不允许混用半份滚动 row 与 Legacy row。

### 22.2 真实覆盖缺口与修复

第一版在 4 实体合成测试中通过，但真实 1000 AI 压力运行的 world/input hash 相同，RNG、slot、aRest 与 event hash 分叉。审计 canonical writer 后定位到明确缺口：`NTSDEntityRuntime.ZInt` 已通过 `BattleFrameMotionStore.CaptureChangedField` 写入权威 store，`BattleAiUnifiedRowPublisher.PublishFrameMotion(int)` 却只处理 X/Y、Frame、State 与 HitStop，没有把 `RuntimeFrameMotionField.ZInt` 写入 pending Z 并设置 `ZBit`。完整重建每 tick 重读 store，因此不会暴露；滚动路径会继续保留旧 Z，进而改变 AI 距离/决策、RNG 消费和下游事件。

修复只补齐这一 writer 契约，并把聚焦测试扩展为在两次 `CharacterInput` 之间直接修改相同角色的 `ZInt`，要求滚动与强制 full rebuild 在后续 30 tick 的决策与实体可观察状态完全一致。没有通过扩大半径、改 RNG、改 slot 顺序或强制每 tick full capture 掩盖分叉。

### 22.3 交错 A/B 与确定性证据

相同 seed、1000 生产实体、30 warmup + 180 sample 的执行顺序为 rolling 1 -> full rebuild -> rolling 2：

| 路径 | average | P95 | P99 | max | roll-forward | dirty slot | canonical initial capture |
|---|---:|---:|---:|---:|---:|---:|---:|
| rolling 1 | `24.1951` | `28.9149` | `32.7885` | `43.2552` | `208` | `136655` | `1000` |
| full rebuild | `23.9229` | `28.7023` | `31.4184` | `33.6225` | `0` | `0` | `209000` |
| rolling 2 | `23.9546` | `28.7390` | `30.4318` | `31.1594` | `208` | `136655` | `1000` |

三次运行的 final battle parity hash 均为
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，final lockstep hash 均为
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`。三次均为 sampled tick `0 B`、Gen0/1/2 collection `0`、authority success、harness validity true、teardown `restored=true`。

滚动路径把完整 canonical 捕获从 `209000` 降到 `1000`，并消除 208 个 tick 的全容量捕获边界；但两次 rolling 与中间 full rebuild 的总 tick average/P95 基本持平，不能宣称稳定 FPS 收益。该切片按“减少已存在的对象式全实体捕获、保持 canonical ownership 与确定性”等价证据保留，不以短样本速度差作为晋升理由。

### 22.4 fresh 门禁与剩余范围

- 本地 `Assembly-CSharp.csproj`：0 error；Unity force refresh 后编译 ready；
- `AiDecisionSoAShadowEditorTests`：job `18e154bdd6904c7cb595f14b9c45c2bd`，`60/60 PASS`；
- `ProductionEntityStressEditorTests`：job `b58e46e05c274ff09cc003996e83d62f`，`238/238 PASS`；
- `BattleRuntimeSelfCheck`：`2026-08-13 10:34:15 PASS`；
- 正式滚动报告：`Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-capacity-pressure-smoke.json`；
- 强制完整重建报告：`Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-full-unified-snapshot-rebuild.json`。

本切片关闭的是 UnifiedAuthority 每 tick 初始全实体 canonical row 重捕获，不代表完整 frame/motion/lifecycle canonical world、对象 shell 退化、其余 pass 对象式热循环或 U6/U9 已完成。下一步继续依据 fresh detail timing 选择完整字段簇或跨 pass canonical 产品；服务器 S0、T8 默认 `stage.dat` 与 Android 真机仍不进入当前阶段。

## 23. StageBounds 精确 Z writer（第二十七切片，2026-08-13，已保留）

### 23.1 权威契约与旧兼容路径

权威 C# `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Simulation\GameTick.cs:1961-1977`
中的 `StageBounds` 在主 tick 内调用两次。每次都按 runtime slot 升序遍历活动角色，读取 stage 的
`zmin/zmax`，把角色 `Z` 夹取到边界内，再写入 `ZInt=(int)Z`。该方法不读取或写入 Team、HP、PP、
Frame、State、关系字段，也不建立完整 Runtime 快照。

Unity 旧路径曾在 U4 writer 尚未闭合时，通过 `RefreshRuntimeSnapshot()` 兼容性地刷新宽 Runtime 镜像。
经过 U5/U6 的 canonical writer 迁移后，该宽刷新不再是权威 `StageBounds` 的职责，反而令同一 tick 的两次
边界 pass 重复读取和写入大量无关对象字段。第二十七切片据此完成以下最小修改：

- `BattleEcsCharacterStageZPass` 正式默认模式由 `Legacy` 切换为 `DataOriented`；
- exact `LF2Character` 只写 `Runtime.Z` 与 `Runtime.ZInt`，与权威 C# 字段边界相同；
- 未知派生或自定义角色类型继续调用虚拟 `RefreshRuntimeSnapshot()`，fail closed 保留兼容语义；
- 删除已经无调用者的 `LF2Entity.RefreshBaseRuntimeSnapshotForStageBounds()` 宽快照 helper；
- 聚焦测试明确把其他 canonical writer 已负责的 Frame 镜像作为前置契约，不再要求 `StageBounds` 越权修复 Frame；
- Runtime 反射等价测试忽略 `[NonSerialized]` 诊断/缓存对象，避免把非战斗真值的引用身份误判为状态差异。

### 23.2 1000 AI 两轮性能与确定性证据

相同 seed、1000 个真实生产实体、30 warmup + 180 sample 的报告如下：

| 报告 | total average | P95 | P99 | max | `StageBounds` average | `StageBounds` P95 |
|---|---:|---:|---:|---:|---:|---:|
| Legacy 基线 `data-oriented-capacity-pressure-smoke` | `23.9546` | `28.7390` | `30.4318` | `31.1594` | `1.3871` | `1.5043` |
| exact writer 候选 1 | `23.1783` | `28.1475` | `30.4370` | `39.3368` | `0.4548` | `0.6042` |
| exact writer 候选 2 | `23.2743` | `27.6578` | `30.2926` | `36.0011` | `0.4676` | `0.5810` |

两轮目标 pass average 相对基线稳定减少约 `66%～67%`；总 tick average 约改善 `3.2%` 与 `2.8%`，
P95 约改善 `2.1%` 与 `3.8%`。候选 max 高于基线但不重复，属于 Editor 短样本尖峰；晋升依据是目标 pass
两轮稳定正收益、P95 不回退以及行为等价，不使用单个 max 作为完成或否决证据。

三份报告的 final battle parity hash 均为
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，final lockstep hash 均为
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`。两轮候选均为 sampled tick
`0 B`、Gen0/1/2 collection `0`、zero-GC gate PASS、harness validity true、teardown `restored=true`。

候选报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.u6-stagez-canonical-write-candidate-1.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-stagez-canonical-write-candidate-2.json`。

### 23.3 fresh 门禁与诚实边界

- 本地 `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 串行构建：0 error；
- Unity force refresh 后编译 ready；
- 最新代码状态下聚焦 EditMode job `f62bc840a0154ffcaad65fafa0af1d11`：`9/9 PASS`；
- `BattleRuntimeSelfCheck`：`2026-08-13 11:15:58 PASS`；
- 完整 EditMode job `a02648fc844e4c0dbcd48ef0d55fdb28`：`1078/1078 PASS`。测试通过同一持久
  UnityMCP 会话启动并轮询，已排除客户端提前断开造成的 `NetworkStream` 基础设施日志污染。

因此本切片的准确结论是：StageBounds 精确 writer 的编译、自检、聚焦测试、完整 EditMode、1000 AI 行为
hash 与零 GC 门禁均已通过。早先 `1074/1075 + 1/1` 的拆分证据已被最新单次 `1078/1078` 完整证据取代。

本切片只关闭 `StageBounds` 中超出权威职责的宽 Runtime 刷新，不代表完整 frame/motion/lifecycle canonical
world、其余对象 shell 热循环、U6 或 U9 已完成。下一步继续从 fresh detail timing 的
`CharacterInput / CandidateCollect / LateEntityUpdate / FrameAdvance` 中选择能减少既有对象式工作量、且不改变
权威 pass/slot/RNG/opoint 可见边界的完整字段簇或跨 pass canonical 产品。服务器 S0、T8 默认
`stage.dat` 与 Android 真机仍不进入当前阶段。

## 24. FrameAdvance canonical writer 窄化（第二十八切片，2026-08-13，已保留）

### 24.1 权威边界与 writer 审计

权威 C# `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Simulation\GameTick.cs` 的正式帧推进按
runtime slot 升序遍历活动实体，清理当前 action/direction key，再调用 `FrameRuntimePasses.RunFrameAdvance`。
权威链没有 Unity 兼容层在每个角色尾部重建完整 Runtime 镜像的步骤。

Unity 旧 `SerialTickAll` 在相同 slot 与相同 pass 位置执行 `SimTransit()`、`SimTU()` 后，无条件调用
`RefreshRuntimeSnapshot()`。经过前序 U5/U6 writer 迁移后，正式 exact `LF2Character` 的 Frame、transition、
health、motion 与计数字段均已在各自写入点同步到 Runtime/canonical store；这次宽刷新只是在同一 tick 再读、
再写整对象。第二十八切片据此只窄化这一兼容工作：

- exact `LF2Character` 跳过尾部宽快照；
- 未知派生或自定义角色继续通过虚拟 `RefreshRuntimeSnapshot()` fail closed；
- 调用位置、slot 升序、`SimTransit -> SimTU` 顺序、早退边界与后续 pass 均不变；
- 新聚焦测试覆盖 exact 跳过、派生类型虚调用修复以及预热后 4096 次 exact 调用 0 B。

### 24.2 三轮 1000 AI 性能与确定性证据

相同 seed、1000 个真实生产实体、30 warmup + 180 sample、完整 detail timing 的报告如下：

| 报告 | total average | P95 | P99 | max | `FrameAdvance` average | `RefreshRuntimeSnapshot` average |
|---|---:|---:|---:|---:|---:|---:|
| 第二十七切片基线 | `23.2743` | `27.6578` | `30.2926` | `36.0011` | `2.5911` | `0.6151` |
| FrameAdvance 候选 1 | `22.4282` | `27.0139` | `29.5144` | `31.8868` | `1.8919` | `0.0553` |
| FrameAdvance 候选 2 | `22.9794` | `29.1838` | `31.0699` | `31.8982` | `1.9589` | `0.0574` |
| FrameAdvance 候选 3 | `23.2752` | `28.7866` | `31.2587` | `32.1691` | `1.9055` | `0.0566` |

目标子段三轮稳定减少约 `90.7%～91.0%`，完整 `FrameAdvance` 稳定减少约 `24%～27%`。候选 1、2 的总
average 低于基线，但候选 3 因其他 pass 同步变慢回到基线附近，P95 也受 Editor 噪声影响。因此晋升依据是
目标子段和完整 FrameAdvance 的三轮稳定正收益、行为等价与零 GC，不宣称本切片已经带来稳定的整体 FPS 提升。

四份报告的 final battle parity hash 均为
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，final lockstep hash 均为
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`。三轮候选均为 sampled tick
`0 B`、Gen0/1/2 collection `0`、zero-GC gate PASS、harness validity true、teardown `restored=true`。

候选报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.u6-frameadvance-canonical-writer-candidate-1.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-frameadvance-canonical-writer-candidate-2.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-frameadvance-canonical-writer-candidate-3.json`。

### 24.3 fresh 门禁与剩余边界

- 本地 `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj`：0 error；
- Unity force refresh 后编译 ready；
- `FrameAdvanceRuntimeSnapshotEditorTests` 聚焦 job `878319e0d8774628a0666b82fea37ddb`：`3/3 PASS`；
- 完整 EditMode job `a02648fc844e4c0dbcd48ef0d55fdb28`：`1078/1078 PASS`；
- `BattleRuntimeSelfCheck`：`2026-08-13 11:38:18 PASS`。

本切片只删除 exact 正式角色在 FrameAdvance 尾部已被 canonical writers 覆盖的兼容性宽快照，不代表完整
frame/motion/lifecycle canonical world、对象 shell 退化、U6 或 U9 已完成。fresh detail timing 的下一优先候选
是 `CandidateCollect/ParticipantBodyItrBuild`；`CharacterInput` 与 `LateEntityUpdate` 仍保留在后续审计清单。
服务器 S0、T8 默认 `stage.dat` 与 Android 真机仍不进入当前阶段。

## 33. CharacterInput 帧速度尾链 world-owned writer（第三十六切片，2026-08-13，已保留）

### 33.1 权威边界与兼容路径

权威 C# `BattleCore/Input/InputRuntime.cs::ApplyCharacterInput` 先执行 combo/direct/release 与状态动作，
再于正常结束和 heavy-link、frame 215、recovery-jump 早退前调用同一
`ApplyFrameVelocityTail`。尾链依次处理：

1. DVX 的 500 阈值绝对写入、朝向变换与只增强当前速度的比较；
2. DVY 的绝对写入或累加；
3. DVZ 的绝对写入，或按 Up/Down 与对应 cooldown 大小选择符号。

Unity 现将精确 `LF2Character` 的这个最终写入收口到 world-owned
`BattleCharacterActionWriter.TryApplyExactCharacterFrameVelocityTail`。只有已注册且运行时类型精确等于
`LF2Character` 才使用该快路径；未注册对象、派生角色以及装载 character DAT 的其他壳类
仍执行旧虚方法，保留 `IsFrameTick*Pressed` 与 `Dirh()` 的类型专项语义。
这是写入所有权迁移，不是完整 frame/motion store 已成为唯一真值的声明。

### 33.2 运行证据与边界

- 聚焦 job `c5555fc04c6a4ad09cdc5cc51c916cc8`：`28/28 PASS`，覆盖速度矩阵、预热后 0 B 与派生类型 fail-closed；
- 完整 EditMode job `fe030cca31614363aedf64483aab0cb6`：`1100/1100 PASS`；
- `BattleRuntimeSelfCheck`：`2026-08-13 19:46:01 PASS`；
- 本地 `Assembly-CSharp.csproj` / `Assembly-CSharp-Editor.csproj`：0 error；
- 最终 1000 AI 报告 `Temp/NTSD_ProductionEntityStress.combat1000.u6-action-writer-frame-velocity-final-20260813.json`：
  30 warmup + 300 sample，logic average/P95/P99/max
  `18.6504/23.7142/25.8144/27.4020 ms`，Unity frame average/P95
  `33.9323/39.6413 ms`；
- sampled tick average/max allocation `0/0 B`，Gen0/1/2 collection `0/0/0`，
  parity hash `7a5d8f11482c98c7293487b219e52b2cd6aa6ea545ab917f42f9541ab0d21de9`，
  lockstep hash `f26bcb9b23f0b8e2381ef09f580e47402b1c9232304b38299f33e2c110a338ef`，
  teardown `restored=true`。

相邻无该 writer 的清洁基线 logic average/P95 为 `17.9637/22.7936 ms`，两份报告 hash 一致但
短样本整体没有性能改善。因此本切片只按所有权收口与行为等价保留，不扩大为 FPS 改善、
U6 完成或 U9 达标。下一批继续以“完整字段簇/整段旧工作可被删除”为选择标准，不再对已确认只有
微小开销的单个分支反复微调。

## 31. CentralOnly 表现顺序轻量索引基数排序（第三十四切片，2026-08-13，已保留）

### 31.1 所有权与等价边界

该切片只处理 Unity 表现快照的物化成本，不改变权威 C# `GameTick.cs` 的 pass 顺序、实体真值、输入、
碰撞、命中、RNG 或生命周期。正式捕获链为：

1. `SimulationWorld.GetPresentationEntitiesNoAlloc` 按活动 runtime slot 升序发布快照；
2. `BattlePresentationCoordinator.BeginFrame` 冻结这一正式输入，但不在逻辑 tick 内创建中央命令；
3. 表现宿主调用 `CaptureAndBuild -> MaterializePresentationOrder` 生成最终透明顺序；
4. 顺序契约仍为 `(ZInt, RuntimeSlot, StableId)`。

正式生产输入中活动 runtime slot 唯一，捕获顺序已满足同 Z 下的 `(RuntimeSlot, StableId)` 次序。原实现却
用 `Array.Sort` 直接交换较宽的 `BattlePresentationEntitySnapshot`。新实现复用 world-owned 数组，只对
轻量 `int` 索引执行 4-pass、每 pass 8 bit 的稳定 radix sort；signed `ZInt` 通过最高位翻转映射到无符号
排序域，随后用两个线性 pass 重排快照并写入 `BaseOrder`。

这是有严格前置条件的正式快路径，而不是修改通用比较语义。`MaterializePresentationOrder` 会先验证输入：

- runtime slot 不得递减；
- 同一 slot 下 stable id 不得递减。

任一条件不满足时，立即回退到原 `Array.Sort + PresentationSnapshotComparer`。因此任意顺序的自定义帧、
测试帧和未知生产者仍使用原比较器；`int.MinValue/int.MaxValue`、负 Z、同 Z slot tie-break 都有聚焦测试。

### 31.2 1000 AI 性能与行为证据

相邻两轮第三十三切片报告中：

- `BeginFrame/SortEntities` average 约 `2.10 ms`，P95 约 `2.48 ms`；
- `PresentationPublish/Total` average 约 `8.42 ms`。

第三十四切片两轮报告：

| 报告 | total avg/P95/P99/max | sort avg/P95 | publish avg/P95 |
|---|---:|---:|---:|
| `u6-presentation-radix-candidate` | `20.9898/25.8899/27.8487/28.5194 ms` | `0.5241/0.5370 ms` | `6.7800/7.6110 ms` |
| `u6-presentation-radix-candidate-2` | `20.9893/26.2584/29.4533/32.6510 ms` | `0.5331/0.6165 ms` | `6.8231/7.6339 ms` |

目标排序子段 average 约减少 `75%`，P95 约减少 `77%`；publish average 约减少 `1.62 ms`。总 tick
average 只比相邻报告小约 `0.05 ms`，所以这里只确认表现排序子段收益，不宣称整体 FPS 已明显提高。
两轮 `BeginFrame/BuildCommands` average 仍约 `2.55 ms`，下一批继续审计它的高频命令构建与重复产品。

两轮均为 1000 个真实生产 AI、同一 seed、30 warmup + 180 sample、`data-oriented-canonical`：

- final parity hash：`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`；
- final lockstep hash：`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`；
- 正式逻辑 tick allocation average/max：`0/0 B`；
- Gen0/1/2 collection：`0/0/0`；
- status：`StoppedCleanly`，teardown：`restored=true`。

### 31.3 fresh 门禁与下一审计点

- 本地 `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 顺序构建：0 error；
- `BattlePresentationBeginFrameReuseEditorTests` job `8424b003e0874b0a93a85fd65cae7dd7`：`10/10 PASS`；
- 表现排序、命令与中央物化扩大聚焦 job `87dc0cd48c674f989b1868f36298fa04`：`28/28 PASS`；
- `BattleRuntimeSelfCheck`：`2026-08-13 15:13:35 PASS`；
- 完整 EditMode job `d7fb0db3187349718d9aa8d0fdd81cf7` 执行完 `1094/1094`，唯一失败是 UnityMCP
  注入的 `NetworkStream disposed` Error；对应测试独立 job `9bf689e2724848c28ecf55e4ea6fc55c`
  为 `1/1 PASS`。

下一切片优先拆分并审计 `ResolveDeferredSpriteCaptures -> BuildCommands`，以删除重复实体遍历或重复表现
解析；若无法证明行为等价或两轮目标子段没有稳定收益，则撤回候选。`CharacterInput` 与
`CandidateCollect` 仍保留为后续逻辑热区，服务器 S0、T8 默认 `stage.dat` 和 Android 真机继续排除。

## 32. CentralOnly 延迟 sprite 解析直接命令消费（第三十五切片，2026-08-13，已保留）

### 32.1 定位与失败候选

为避免继续把 `BuildCommands` 当成单一黑盒，新增了默认关闭的 presentation timing 子段：

- `BeginFrame/BuildCommands/ResolveDeferredSpriteCaptures`；
- `BeginFrame/BuildCommands/Core`。

recorder 仍由 `SimulationDiagnosticsModule` 按需创建；诊断关闭时只读取 `null`，不会调用 `Stopwatch`，
不会改变正式战斗 GC。相同 1000 AI 基线中完整阶段为 `2.6039 ms`，其中 sprite 解析与宽快照写回为
`0.7259 ms`，命令本体为 `1.8758 ms`。

首个候选把两个 for-loop 合成一个，但仍对每个实体调用 `WithResolvedSprite` 并 `SetEntity` 回写整个
`BattlePresentationEntitySnapshot`。它的目标阶段退化到 `2.7318 ms`，sprite 子段退化到 `0.7901 ms`；
虽然 total tick 因运行噪声更低，但目标子段明确负优化，因此未按总 tick 偶然波动晋升。

### 32.2 保留实现与不可变边界

保留实现仍在同一个 slot/rank 顺序循环中完成 sprite catalog resolve，但不再重建或写回冻结快照：

1. 冻结的 `BattlePresentationEntitySnapshot` 只提供逻辑与表现输入；
2. catalog resolve 的 `PixelWidth/PixelHeight/UV/Pivot/Descriptor/Identity` 保存在当前迭代局部值；
3. entity 命令直接消费这些局部值；
4. shadow、overlay、hit record、local sequence、base order 和 command 顺序不变；
5. `CommandWriter.Commit` 仍从最终 descriptor 维护 `RequiresCatalogPublicationBinding`。

因此表现宿主不再修改逻辑 publication。新增
`DeferredSpriteMaterialization_BuildsCommandWithoutMutatingFrozenSnapshot` 验证：命令得到正确 sprite key、
尺寸、UV、pivot 与 binding 标志，而冻结快照的未解析字段保持原值。该优化只改变 Unity 表现适配数据流，
不改变权威 C# 战斗 pass 或任何战斗真值。

### 32.3 两轮 1000 AI 证据

| 报告 | total avg/P95/P99/max | BuildCommands avg/P95 | sprite resolve avg/P95 | publish avg/P95 |
|---|---:|---:|---:|---:|
| `u6-buildcommands-direct-resolve-candidate` | `21.1970/26.2597/31.0765/46.7816 ms` | `2.2808/3.0402 ms` | `0.2915/0.4095 ms` | `6.6284/8.5427 ms` |
| `u6-buildcommands-direct-resolve-candidate-2` | `20.8252/25.5468/27.5242/28.8198 ms` | `2.3193/2.7762 ms` | `0.3091/0.3954 ms` | `6.7122/8.0524 ms` |

对比基线，sprite resolve average 稳定减少约 `57%～60%`，完整 `BuildCommands` average 稳定减少约
`11%～12%`。第一轮 total max 的 `46.7816 ms` 是单个 Editor 尖峰，不参与目标子段晋升判断。

两轮均为同 seed、1000 个真实生产 AI、30 warmup + 180 sample、`data-oriented-canonical`：

- parity hash：`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`；
- lockstep hash：`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`；
- logic tick、driver update、presentation managed-memory boundary：均 `0 B`、无 violation；
- Gen0/1/2 collection：`0/0/0`；
- harness/authority validity：true；status `StoppedCleanly`；teardown `restored=true`。

### 32.4 fresh 门禁与下一热点

- 本地 runtime/editor 工程顺序构建：0 error；
- 表现、catalog 与压力工具扩大聚焦 job `2f44bd76d11249b98cc4eae0b643f38c`：`287/287 PASS`；
- `BattleRuntimeSelfCheck`：`2026-08-13 15:59:33 PASS`；
- 完整 EditMode job `6814ccf3de9a4b1d977a04e3208198af` 执行 `1095/1095`，唯一失败为 MCP
  `NetworkStream disposed` 日志污染；对应独立 job `d49b31aa445b495eacc956a48e42773e`：`1/1 PASS`。

最新第二轮报告的逻辑热点为：`CharacterInput 5.4925 ms`、`CandidateCollect 3.7415 ms`、
`LateEntityUpdate 2.5017 ms`、`FrameAdvance 1.8976 ms`。Unity frame average/P95 仍为
`41.94/46.00 ms`，因此 1000 AI / 30 FPS 尚未达成；下一切片回到最大逻辑热区，并优先选择能删除现有
对象式工作或复用 canonical 产品的完整字段簇。服务器 S0、T8 默认 `stage.dat`、Android 真机继续排除。

## 30. LateEntityUpdate 最终 runtime 快照边界（第三十三切片，2026-08-13，已保留）

### 30.1 权威职责与 Unity 重复适配

权威 C# `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Simulation\GameTick.cs` 的
`RunLateEntityUpdate` 按 slot 顺序依次执行 state special、战斗恢复、`FrameTickRuntime.Tick`、特殊 frame exit、
死亡/掉落、opoint、武器清理、N30、transition effect 与 `PrevFrame` 镜像。权威实现直接修改同一个 `Entity`，
没有在 pass 尾部重新组装另一份完整对象快照。

Unity 的旧 `ConsolidatedFinal` 路径仍在每个活动实体尾部调用一次 `RefreshRuntimeSnapshot()`。对 exact
`LF2Character`，该方法复制的字段已经具备以下写入契约：

- identity/object type 由 `PublishIdentityMetadataForSimulation` 在 ObjectId 或 DAT identity 变化时同步；
- frame 与 transition 的正式写入口同时维护 `Runtime.Frame/WaitCounter/NextFrame`；
- HP/MP/PP 与边界值由绑定 `Runtime` 的 `LF2Health` 直接读写；
- position、velocity、计数、hit stop、owner/relation 等属性本身就是 Runtime 别名。

因此本切片加入 fail-closed 判定：只有 exact `LF2Character` 且 object type、frame、wait、next 等最小非别名
字段仍与 Runtime 一致时，才省略最终宽快照。未知派生类型或任何陈旧字段继续完整刷新；`LegacyThree` 诊断
模式仍强制保留三段 oracle，不改变权威 pass 顺序、早退、opoint flush、生命周期或 `PrevFrame` 可见边界。

### 30.2 两轮真实 1000 AI 结果

相同 seed、1000 个真实生产 AI、30 warmup + 180 sample、`data-oriented-canonical`、完整 phase/detail/
presentation timing：

| 报告 | total average | P95 | P99 | max | LateEntityUpdate avg | TailAndQueuedFlush avg | 最终快照调用 |
|---|---:|---:|---:|---:|---:|---:|---:|
| 相邻候选 1 | `21.8950` | `27.6595` | `31.1295` | `31.9000` | `3.120` | `0.788` | `180000` |
| 相邻候选 2 | `22.8095` | `28.6581` | `31.9332` | `39.9960` | `3.267` | `0.832` | `180000` |
| Late final candidate 1 | `21.0483` | `25.4053` | `28.6349` | `37.4828` | `2.545` | `0.331` | `0` |
| Late final candidate 2 | `21.0350` | `26.1610` | `29.1950` | `29.3290` | `2.513` | `0.324` | `0` |

目标子段两轮重复减少约 `0.46～0.51 ms/tick`，完整 LateEntityUpdate 两轮减少约 `0.58～0.75 ms/tick`；
total average/P95 也均低于两份相邻报告。两轮候选的 final battle parity hash 都是
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，lockstep hash 都是
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`；正式 sampled tick `0 B`，
Gen0/1/2 collection `0`，zero-GC gate PASS，teardown `restored=true`。

有效报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.u6-late-final-snapshot-candidate-20260813.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-late-final-snapshot-candidate-2-20260813.json`。

### 30.3 fresh 门禁与下一批

- 本地 `Assembly-CSharp.csproj`、`Assembly-CSharp-Editor.csproj` 顺序构建：0 error；
- Late 快照与边界聚焦 job `29eca881cef64c0f9b09732903e87c10`：`25/25 PASS`；
- `BattleRuntimeSelfCheck`：`2026-08-13 14:43:32 PASS`；
- 完整 EditMode job `ae76345e609e4cc0b23bcce35232b332` 执行完 `1093/1093`，唯一失败是 UnityMCP
  随机注入的 `NetworkStream disposed` Error；对应测试独立 job `16bf46b81ae148ea82f1c45069efd27f`：`1/1 PASS`。

因此本切片没有已知代码断言失败，但不能把受日志污染的完整 job 写成干净 PASS。该结果关闭的是
`LateEntityUpdate` 最终宽快照这一处重复适配，不代表整个 Late pass、U6 或 U9 完成。下一批转向中央表现链：
确认逻辑 tick 的 `CaptureEntities/SortEntities` 与渲染宿主的 `SortEntities/BuildCommands/Publish` 是否重复构造
同一帧产品，并保持表现层绝不反写战斗真值。服务器 S0、T8 默认 `stage.dat` 与 Android 真机仍排除。

## 29. IndexedCanonical 统一快照拥有期边界（第三十二切片，2026-08-13，已保留）

### 29.1 权威链与被排除的伪热点

权威 C# `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Input\InputRuntime.cs` 的
`ApplyCharacterInput` 固定执行组合技 wrapper、direct attack/defend/jump、移动与 release action；Unity
`RunCharacterInputPhaseForKnownCharacterDat -> ComboUpdate -> BattleCharacterInputActionResolver` 保持相同顺序。
临时细分计时把 `ComboUpdate` 拆成 progress capture、combo/direct、release 与 progress commit，真实工作合计约
`1.05 ms/tick`，没有可安全删除的隐藏多毫秒循环。该临时计时代码在结论产生后已经完整移除，不能把其
`Stopwatch` 开销带入正式实现。

### 29.2 统一快照拥有期优化

UnifiedAuthority 发布成功后，`AiUnifiedSnapshotExecutionState` 在同一 `CharacterInput` pass 内拥有 rows、
indexed snapshot、fallback slots 与 occupancy epoch。单个 AI 的 capture、value-only kernel 和 commit 连续同步执行；
kernel 不回调 World，也不允许在 commit 前改变 slot occupancy、generation 或 selected entity handle。因此：

- indexed snapshot capture 直接使用同一 published state 的 generation、identity 与 epoch；
- canonical input store 仍校验当前 runtime owner，非法 slot/generation 仍 fail closed；
- 非 UnifiedAuthority、shared shadow、Legacy 与测试注入的 pre-commit failure 继续走完整旧校验；
- UnifiedAuthority commit 保留 snapshot/state 引用、runtime、RNG 与 flow gate，但不再逐 AI 重读完全相同的
  indexes、epoch、self identity 与 selected handle；
- RNG 调用、slot 顺序、同 tick row refresh、fallback/hard-breach 规则与最终写入不变。

### 29.3 两轮 1000 AI 与门禁

相同 seed、1000 个真实生产 AI、30 warmup + 180 sample、完整 nested timing：

| 报告 | total average | total P95 | capture average | commit validation average |
|---|---:|---:|---:|---:|
| 临时 action-detail 基线 | `21.8364` | `26.9971` | `0.4401` | `0.1320` |
| 拥有期候选 1 | `21.8950` | `27.6595` | `0.4281` | `0.0698` |
| 拥有期候选 2 | `22.8095` | `28.6581` | `0.4206` | `0.0660` |

基线额外包含后来已删除的 `ComboUpdate` 临时分段计时，所以不能拿 total/CharacterInput 直接宣称整体收益；
可独立比较的 capture 与 commit-validation 子段两轮均稳定减少。候选 2 的其它 pass 与 Editor 外层整体变慢，
总 tick 没有稳定改善。本切片只确认删除了统一拥有期内的重复校验，不宣称 FPS 提升。

候选报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.u6-indexed-owned-boundary-candidate-20260813.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-indexed-owned-boundary-candidate-2-20260813.json`。

两轮 final battle parity hash 均为
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，lockstep hash 均为
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`；正式 sampled tick 0 B，
Gen0/1/2 collection 0，fallback 0，hard breach 0，teardown `restored=true`。fresh 门禁：

- 本地 runtime/editor 顺序构建：0 error；
- AI decision + CharacterInput 聚焦 job `0cbf017c8e624aa3bdcc0f2c482b7f24`：`85/85 PASS`；
- 完整 EditMode job `3638b05e64e64de9a2ea3ca8001a4733`：`1090/1090 PASS`；
- `BattleRuntimeSelfCheck`：`2026-08-13 14:22:38 PASS`。

本切片不关闭 U6/U9。下一批不再拆 `ComboUpdate` 微函数，转向 `LateEntityUpdate` 与 presentation publish 中
可以删除完整对象扫描、重复快照或重复命令构建的候选；服务器 S0、T8 默认 `stage.dat` 与 Android 真机仍排除。

## 27. CandidateCollect canonical geometry handle 读取负实验（第三十一切片候选，2026-08-13，已撤回）

第三十切片后先尝试让 `ParticipantBodyItrBuild` 通过已存在的
`BattleFrameMotionStore` handle reader 读取碰撞几何所需的 motion 字段，避免继续从实体对象链读取同一组值。
候选保持 authority ordinal、pair key、双向 exact collect、RNG、fallback 与异常边界不变，并通过
formal collector 聚焦测试；但真实 1000 AI 的三轮结果不支持晋升：

| 报告 | total average | P95 | `CandidateCollect` average | `ParticipantBodyItrBuild` average |
|---|---:|---:|---:|---:|
| `u6-canonical-geometry-candidate-1` | `22.3761` | `27.7844` | `3.7027` | `1.0082` |
| `u6-canonical-geometry-candidate-2` | `22.7185` | `27.5820` | `3.7640` | `1.0266` |
| `u6-canonical-geometry-candidate-3` | `22.7246` | `29.0933` | `3.8575` | `1.0632` |

相邻第三十切片基线的 total average 为 `21.9110/21.8486 ms`，`CandidateCollect` 为
`3.7537/3.7442 ms`，`ParticipantBodyItrBuild` 为 `1.025/1.010 ms`。候选没有稳定降低目标子段，
总 tick 反而稳定回退约 `2%～4%`，因此 handle overload、world/writer 转发、诊断计数与测试均已完整撤回。
三轮候选虽保持 sampled tick 0 B、三代 collection 0、两套 hash 一致和 teardown 完整恢复，但这些只证明
行为未漂移，不能抵消明确负收益。

## 28. PreFrameBounds exact character canonical writer（第三十二切片，2026-08-13，已保留）

### 28.1 权威职责与兼容边界

权威 C# `BattleCore/Simulation/GameTick.cs::ApplyPreframeBounds` 只在 runtime slot 升序遍历中应用
角色 X/Z 边界，并同步 `XInt/ZInt`；它不在该 pass 重建完整实体 snapshot。Unity 旧路径却对每个角色依次调用
两个虚方法，再无条件执行宽 `RefreshRuntimeSnapshot()`。

本切片新增 world-owned `BattleEcsCharacterPreFrameBoundsPass`：

- 正式默认 `DataOriented` 路径按 `RuntimeSlotTable` 的 slot/generation 顺序处理；
- exact `LF2Character` 直接执行与权威 C# 相同的 X/Z 条件和 `XInt/ZInt` 写入，不刷新无关字段；
- identity/object type 或 generation 不满足 exact 合同时 fail closed 到原虚方法与宽 snapshot，保留派生类型副作用；
- `Transform`、渲染状态和相机都不进入逻辑判定；
- Legacy/DataOriented 诊断模式只能在 reset boundary 配置，压力工具会记录 requested/effective、exact/fallback
  计数，并在所有 slot 释放后恢复原模式；
- 聚焦测试覆盖七组边界矩阵、派生类型 fallback、reset-boundary 门禁和 1000 个角色预热后 0 B。

### 28.2 同版本交错 A/B

相同 seed、1000 个真实生产实体、30 warmup + 180 sample、完整 detail timing：

| 轮次 | 模式 | total average | total P95 | `PreFrameBounds` average | `PreFrameBounds` P95 |
|---|---|---:|---:|---:|---:|
| 1 | Legacy | `22.7463` | `27.4700` | `0.9075` | `1.1504` |
| 1 | DataOriented | `23.6991` | `32.8930` | `0.3697` | `0.4831` |
| 2 | Legacy | `22.7792` | `28.4969` | `0.9277` | `1.0334` |
| 2 | DataOriented | `21.8951` | `26.6313` | `0.3533` | `0.4132` |

目标 pass 两轮稳定减少约 `59%～62%`。第一轮 DataOriented 的 total P95/P99/max 出现
`32.8930/44.9579/59.4141 ms` 的 Editor 全局尖峰，而第二轮 total average/P95 同时优于相邻 Legacy；
因此晋升依据是目标 pass 两轮稳定正收益、第二轮整体不回退和行为门禁，不把四轮 total 的噪声扩大为稳定 FPS 声明。

四轮最终 battle parity hash 均为
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，lockstep hash 均为
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`；正式 sampled tick 均为
0 B、Gen0/1/2 collection 0、zero-GC gate PASS、teardown `restored=true`、模式恢复成功。Data 两轮
exact write 均为 `210000`，fallback 为 0。

### 28.3 fresh 门禁与剩余边界

- 本地 `Assembly-CSharp.csproj`、`Assembly-CSharp-Editor.csproj` 均为 0 error；
- 聚焦 EditMode job `46aad3be387844d09746bf6c2ec04267` 为 `6/6 PASS`；
- 压力工具整类 job `3240b4675407415fa0f7a00acfaa33dd` 为 `240/240 PASS`；
- 完整 EditMode job `0be644482d0440a98883c66285c64376` 为 `1090/1090 PASS`；
- `BattleRuntimeSelfCheck` 于 `2026-08-13 13:36:01 PASS`。

本切片只移除 `PreFrameBounds` 中 exact 正式角色的对象式虚调用和宽 snapshot，不代表完整
frame/motion/lifecycle canonical world、其余对象 shell 热循环、U6 或 U9 已完成。下一批继续依据 fresh detail
timing 审计 `CharacterInput/EntityInputPass`、`LateEntityUpdate` 与仍有实际重复工作的 collision 子段；不再重复
第三十一切片已证伪的 canonical geometry handle 读取方案。服务器 S0、T8 默认 `stage.dat` 与 Android 真机仍排除。

## 25. CollisionSnapshot canonical writer 窄化（第二十九切片，2026-08-13，已保留）

### 25.1 权威边界与兼容回退

权威 C# `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Simulation\GameTick.cs` 的
`SnapshotPrevFrame2` 只按 runtime slot 升序遍历活动实体，并执行 `entity.PrevFrame2 = entity.Frame`。
它不在该 pass 刷新 Team、HP、motion、input、relation 或完整 Runtime 镜像。

Unity 的 `CaptureCollisionFrameSnapshot()` 已在同一写入点完成这组职责：

- `Frame.Prev2 = Frame.N`；
- `Frame.Prev2D = Frame.D`；
- `Runtime.PrevFrame2 = Frame.Prev2`。

旧 `CaptureCollisionFrameSnapshotsAll` 随后仍无条件调用 `RefreshRuntimeSnapshot()`，对 exact 正式
`LF2Character` 重读并重写整对象。这一切片只删除该权威职责以外的重复工作：

- exact `LF2Character` 在冻结 `Prev2/Prev2D/PrevFrame2` 后跳过宽快照；
- 未知派生或自定义角色继续调用虚拟 `RefreshRuntimeSnapshot()`，以兼容额外字段和 override 副作用；
- pass 位置、slot 顺序、活动性判断、后续 CandidateCollect 可见边界均不变；
- 聚焦测试覆盖 exact 字段簇、派生虚调用回退，以及预热后 4096 次 exact 分支 0 B。

### 25.2 三轮 1000 AI 性能与等价证据

相同 seed、1000 个真实生产实体、30 warmup + 180 sample、`data-oriented-canonical` 与完整 detail
timing 的有效报告如下：

| 报告 | total average | P95 | P99 | max | `CollisionSnapshot` average | `CollisionSnapshot` P95 |
|---|---:|---:|---:|---:|---:|---:|
| 第二十八切片基线 | `23.2752` | `28.7866` | `31.2587` | `32.1691` | `0.7134` | `0.8614` |
| CollisionSnapshot 候选 1 | `23.2216` | `29.8168` | `35.8186` | `36.2354` | `0.2495` | `0.3176` |
| CollisionSnapshot 候选 2 | `22.2703` | `27.4394` | `29.9383` | `36.7272` | `0.2333` | `0.2448` |
| CollisionSnapshot 候选 3 | `22.2107` | `26.3527` | `29.4321` | `31.8419` | `0.2375` | `0.2700` |

目标 pass average 三轮稳定减少约 `65.0%～67.3%`，P95 稳定减少约 `63.1%～71.6%`。候选 2、3 的
完整 tick average/P95 同时低于基线；候选 1 的 total P95/P99 受其他 pass 与 Editor 尖峰影响，因此晋升依据
仍是目标 pass 的三轮稳定正收益、行为等价与零 GC，而不是单个完整 tick 尖峰。

四份报告的 final battle parity hash 均为
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，final lockstep hash 均为
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`。三轮候选均为正式 sampled tick
`0 B`、Gen0/1/2 collection `0`、zero-GC gate PASS、teardown `restored=true`。

有效候选报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.u6-collision-snapshot-canonical-writer-candidate-1.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-collision-snapshot-canonical-writer-canonical-2.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-collision-snapshot-canonical-writer-canonical-3.json`。

测试期间曾有两份错误使用旧/不存在请求字段而回退到 `legacy` AI 的报告；它们不参与上述 A/B 表格或晋升结论。

### 25.3 fresh 门禁与剩余边界

- 本地 `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj`：0 error；
- `CollisionSnapshotRuntimeSyncEditorTests` 聚焦 job `e6633e8405a24e59b8b4d3ee2a0af412`：`3/3 PASS`；
- `BattleRuntimeSelfCheck`：`2026-08-13 12:10:21 PASS`；
- 完整 EditMode job `f443d393d36f4cb1a441ebd641c0959f` 与
  `585da834c82d457c81d8022a030f0a3e` 均执行完 `1081/1081`，但分别被 UnityMCP 随机注入的同一条
  `NetworkStream disposed` Error 污染而判失败；两项被污染测试的独立 job
  `d4a2cd5f6e2141ef9e735e186334444f`、`bd4fdb9e902f4f1ab1ef16ecf6816f78` 均为 `1/1 PASS`。

因此本切片的代码断言没有已知失败，但 fresh 完整 job 仍应诚实标记为 UnityMCP 日志基础设施阻塞，不能写成
干净的 `1081/1081 PASS`。这不回退已经由三轮真实 1000 AI、哈希、零 GC、聚焦测试和 self-check 证明的
CollisionSnapshot 局部正优化；也不代表完整 frame/motion/lifecycle canonical world、对象 shell 退化、U6 或
U9 已完成。下一优先候选恢复为 `CandidateCollect/ParticipantBodyItrBuild`；服务器 S0、T8 默认 `stage.dat` 与
Android 真机仍不进入当前阶段。

## 26. CandidateCollect 单一参与者缓冲（第三十切片，2026-08-13，已保留）

### 26.1 重复暂存边界

role-aware formal collector 原先在同一个 `CandidateCollect` pass 中维护两份内容相同的宽参与者结构：

1. 先按 authority ordinal 写入 `List<RoleAwareFormalParticipant>`；
2. broadphase 完成后把全部参与者 `CopyTo` `_roleFormalParticipantReadBuffer`；
3. pair exact loop 只读第二份数组；
4. pass 结束时再 `Array.Clear` 第二份引用数组。

这不是权威 C# 的战斗规则，也不是跨 pass 快照，只是 Unity collector 内部的重复暂存。第三十切片新增
`RoleAwareFormalParticipantBuffer`，以一个预分配数组同时承担按 slot/authority 顺序构建和 `ref readonly`
消费。`BeginBuild/CompleteBuild` 保留前一轮 count，只在新 roster 变短时清理失效尾段；同规模稳定战斗不会清空
或复制完整 1000 行。以下边界保持不变：

- authority ordinal 与 runtime slot 顺序；
- body/itr role 标记、fallback、direct/tree/sweep 选择；
- authority pair key 排序、双向 `CollectCandidatesForPair` 调用和 RNG 消费；
- occupancy epoch、generation、输入验证与异常时 brute-force fail-closed；
- 参与者减少后旧实体引用不可通过 `Count` 或 ref indexer 重新可见。

### 26.2 三轮有效 1000 AI 与无效样本排除

相同 seed、1000 个真实生产实体、30 warmup + 180 sample、`data-oriented-canonical` 和完整 detail timing：

| 报告 | total average | total P95 | CandidateCollect average | CandidateCollect P95 | ParticipantBodyItrBuild average | ParticipantBodyItrBuild P95 |
|---|---:|---:|---:|---:|---:|---:|
| 相邻基线 2 | `22.2703` | `27.4394` | `3.819` | `7.877` | `1.008` | `1.150` |
| 相邻基线 3 | `22.2107` | `26.3527` | `3.840` | `7.625` | `1.029` | `1.217` |
| 单缓冲候选 1 | `23.4850` | `30.0105` | `3.956` | `8.750` | `1.100` | `1.585` |
| 单缓冲候选 3 | `21.9110` | `26.5590` | `3.754` | `7.785` | `1.025` | `1.121` |
| 单缓冲候选 4 | `21.8490` | `26.5770` | `3.744` | `7.995` | `1.010` | `1.322` |

两轮相邻基线的 total/CandidateCollect average 中位数约为 `22.2405/3.8295 ms`，三轮有效候选中位数约为
`21.9110/3.7540 ms`。但 CandidateCollect P95 中位数约从 `7.751` 升至 `7.995 ms`，而
`ParticipantBodyItrBuild` 本身基本持平。这说明删除的成本主要位于 body/itr 构建计时段之后的整行复制，短样本
average 有小幅正向信号，但尾部没有稳定改善。保留依据是删除了可静态证明的 O(N) 重复复制和第二份引用存储，
同时通过了行为、零 GC 与生命周期门禁；不把该结果写成稳定 FPS 晋升。

有效候选报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.u6-participant-buffer-candidate-1.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-participant-buffer-candidate-3.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-participant-buffer-candidate-4.json`。

`candidate-2` 在 178/180 sample 时因 Editor runner 被销毁，状态为 `InterruptedWithResidue`、teardown
`restored=false`，其 hash 和 timing 均不参与结论。

三轮有效候选的 final battle parity hash 均为
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，final lockstep hash 均为
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`；正式 sampled tick 均为 `0 B`，
Gen0/1/2 collection 均为 `0`，teardown 均 `restored=true`。

### 26.3 fresh 门禁与下一审计点

- 本地 `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 顺序构建：0 error；
- 单缓冲聚焦 job `a52ed5b8709f441db152e15832e46eac`：`3/3 PASS`；
- formal collector 整类 job `2e90b82192f1456cabc6f3fdec0589f2`：`56/56 PASS`；
- formal dense/legacy slot-map 交叉 job `d1418163384041cca715ba1455298a98`：`2/2 PASS`；
- `BattleRuntimeSelfCheck`：`2026-08-13 12:30:49 PASS`；
- 完整 EditMode job `f099ded58b0e42bcb644f1ab288e65ac` 执行完 `1084/1084`，但唯一失败仍是 UnityMCP
  随机注入的 `NetworkStream disposed` Error 污染；被污染测试的独立 job
  `a67b46d2feab437298f171460d321a03` 为 `1/1 PASS`。

因此当前代码断言没有已知失败，但 fresh 完整 job 仍不能诚实写成干净 PASS。第三十切片不代表
`CandidateCollect` 已完全数据化，也不关闭 U6/U9。最新有效报告显示下一批应优先审计：

1. `CandidateCollect/ParticipantBodyItrBuild` 中 frame/motion 几何状态仍从实体对象链读取的部分；
2. `CharacterInput/EntityInputPass`，当前约 `5.23 ms/tick`，其中 AI decision 和 combo 更新占主要部分；
3. `LateEntityUpdate` 剩余约 `2.98 ms/tick`，但需保持 opoint、生命周期和 slot 可见边界。

服务器 S0、T8 默认 `stage.dat` 与 Android 真机仍不进入当前阶段。
## 34. ReleaseInput resolver world-owned 复用（第三十七切片，2026-08-13，已保留）

### 34.1 原问题与所有权边界

`LF2Character` 原先在构造函数中无条件 `new LF2CharacterActionResolver(this)`。因此正式
1000 AI roster 会常驻 1000 个只保存角色引用的 resolver，即使其调用始终发生在单线程、按 slot
升序的 `CharacterInput` pass 内。它不是权威 C# 的战斗状态，也不应成为每实体 canonical 数据。

本切片改为：

- `SimulationWorld` 已有的 `BattleCharacterActionWriter` 持有一份 resolver；
- `ProcessReleaseInput(character)` 在 `try/finally` 内临时绑定当前角色，退出必定清空；
- 非法重入 fail fast，防止同一 world 内共享可变绑定被静默覆盖；
- registered production character 不再创建本地 resolver；
- 未注册测试、预览或未知兼容调用在第一次需要时才懒创建实体本地 resolver；
- release action 内部的分支顺序、RNG、held/link、PP、frame 和 locomotion 写入均未改动。

### 34.2 门禁与运行证据

- 本地 `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj`：0 error；
- 聚焦测试覆盖 registered 不创建本地 resolver、双角色连续复用不串状态、4096 次预热后 0 B、
  unregistered 懒创建兼容路径；job `8409630e113c450f979c46ad536f43df`：`32/32 PASS`；
- 完整 EditMode job `ebc240ebbe664368973a08b210a64ed1`：`1104/1104 PASS`；
- `BattleRuntimeSelfCheck`：`2026-08-13 20:18:43 PASS`；
- 正式报告：
  `Temp/NTSD_ProductionEntityStress.combat1000.u6-world-release-resolver-final-20260813.json`；
- 1000/1000 真实实体，30 warmup + 300 sample；
- logic average/P95/P99/max：`18.7018/24.5418/27.7835/34.6062 ms`；
- sampled tick：`0 B`，Gen0/1/2 collection：`0/0/0`；
- parity/lockstep hash 与相邻同 workload 基线完全一致；
- teardown：`restored=true`，活动对象、world entity 与 claimed slot 均恢复为 0，cleanup exception 0。

相邻基线 average 为 `18.6504 ms`，本轮差值 `+0.0514 ms`，属于 Editor 运行噪声，不能写成
性能提升。保留依据是删除了正式 1000 roster 的 1000 份常驻 resolver 引用对象并建立明确的
world-owned 执行边界，同时通过行为、GC 与生命周期门禁。本切片不代表 ReleaseInput 已转为纯
SoA kernel，也不关闭 U6/U9；下一步继续选择能够减少正式 tick 实际遍历、虚调用、重复快照或
引用对象访问的完整字段簇。

## 35. 整段 AI CharacterInput 迁移负实验（第三十八候选，2026-08-13，已撤回）

### 35.1 候选与撤回原因

候选曾把 exact AI 的 `PrepareAiInputBasic -> ComboUpdate -> frame velocity tail` 整段入口迁入
world-owned `BattleCharacterActionWriter`，但内部仍调用相同 AI kernel、组合技、release resolver 和
canonical commit，没有删除任何遍历、快照、对象读取或写回。相邻同配置 1000 AI 结果确认它只改变
方法归属，不产生性能收益：

- 候选 logic average/P95/P99/max：`22.0234/27.5931/31.0987/32.3616 ms`；
- 候选 `CharacterInput` average：`5.8815 ms`，与迁移前约 `5.8770 ms` 等价；
- 候选 `FrameAdvance`/`CandidateCollect`/`LateEntityUpdate` average：
  `2.0173/3.7703/2.9033 ms`；
- 正式 tick `0 B`、Gen0/1/2 collection `0/0/0`，battle/lockstep hash 与基线相同，
  teardown `restored=true`。

报告保存为
`Temp/NTSD_ProductionEntityStress.combat1000.u6-full-ai-phase-relocation-negative-20260813.json`。
候选代码已完整撤回，聚焦输入 job `971c86ac40664627a914dbe4f2bd3ef5` 为 `32/32 PASS`。
本实验关闭“只把整段实体方法搬进 writer 就会提速”这一错误方向；后续切片必须实际删除重复工作或
把已证明的 Unity 适配移出逻辑热循环。

## 36. Character mechanics 纯逻辑结果（第三十九切片，2026-08-13，已保留）

### 36.1 权威边界与被删除的 Unity 适配

权威 C# 的 FrameAdvance 物理步骤只推进 X/Y/Z、Vx/Vy/Vz、边界标志、摩擦、重力与落地状态。
Unity `CharacterMechanics.Step` 此前还在每个正式角色每 tick 计算 `GroundPixelToWorld`、
`groundPlanePos`、`visualYOffset` 与 `grounded`，但全仓生产调用方只消费 `landed`、
`verticalVelocityBeforeLanding`；上述三个表现结果没有任何生产 consumer。

本切片新增值类型 `BattleMechanicsStepResult` 和 `StepBattleLogic`：

- exact `LF2Character` 与共享 character-DAT shell 的正式 FrameAdvance 只物化战斗结果；
- 位置、速度、边界清零、sprite origin、重力、落地分支与整数坐标同步顺序保持不变；
- 旧公开 `Step` 继续作为兼容/测试入口，并在纯逻辑结果之后按需物化 Unity `Vector2` 与视觉偏移；
- 没有修改 Transform、表现发布、碰撞 pass、RNG、slot/generation 或权威 C# 规则。

### 36.2 两轮 1000 AI 证据

相邻基线为第三十八负候选运行，但其 FrameAdvance 未被候选修改，可作为同版本旧 mechanics 对照。
相同 seed、1000 个真实生产 AI、30 warmup + 180 sampled tick、完整细分诊断：

| 运行 | logic average/P95 | FrameAdvance average | Transit average/P95 |
|---|---:|---:|---:|
| 旧 mechanics 相邻基线 | `22.0234/27.5931` | `2.0173` | `1.2068/1.5118` |
| 纯逻辑候选 1 | `21.7884/27.2301` | `1.5303` | `0.7405/0.9867` |
| 纯逻辑候选 2 | `21.9666/27.3381` | `1.5498` | `0.7599/0.9030` |

两轮 `Transit` average 分别下降约 `38.6%` 与 `37.0%`，整个 `FrameAdvance` 稳定减少约
`0.47 ms/tick`。两轮均为正式 tick `0 B`、Gen0/1/2 collection `0/0/0`，battle parity hash
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35` 与 lockstep hash
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063` 相同，authority success，
teardown `restored=true`。报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.u6-battle-mechanics-logic-only-candidate-1-20260813.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-battle-mechanics-logic-only-candidate-2-20260813.json`。

### 36.3 fresh 门禁与剩余边界

- 本地 `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj`：0 error；
- mechanics/frame 聚焦 job `bb3bde6c00624258b6990022d2cfe6be`：`5/5 PASS`；
- 完整 EditMode 执行 `1106/1106`，3 个失败均为未声明外部日志：2 个 MCP
  `NetworkStream disposed`，1 个 Unity 随机 mesh bounds assert；受污染 AI/benchmark 测试独立
  `2/2 PASS`，mesh/benchmark 整类独立 `42/42 PASS`；
- `BattleRuntimeSelfCheck` 于 `2026-08-13 22:13:31` fresh 写入 `PASS`。

因此该切片按“删除无消费者的 Unity 表现物化、目标子段两轮稳定正收益、行为/hash/零 GC 等价”保留。
它不代表完整 frame/motion/lifecycle canonical world 或对象 shell 已完成，也不关闭 U6/U9。下一步继续
从 `CharacterInput`、`CandidateCollect`、`LateEntityUpdate` 和表现快照中选择能够删除实际工作的字段簇；
服务器 S0、T8 默认 `stage.dat` 与 Android 真机仍排除。

## 37. 正式角色 FrameAdvance 移除旧 sprite origin 物化（第四十切片，2026-08-13，已保留）

### 37.1 权威依据与兼容边界

权威 C# `BattleCore/Frame/Physics.cs` 的角色物理步骤只推进 X/Y/Z、速度、边界、摩擦、重力、落地和帧；
没有 Unity sprite catalog 查询，也没有 `SpriteX/SpriteY/SpriteZ` 字段。Unity 的 exact registered
`LF2Character.ApplyDynamics` 此前仍会每 tick 通过当前帧图片解析 sprite 宽度，并调用
`UpdateSpriteOrigin` 物化旧碰撞/预览适配字段。生产 exact 碰撞使用 Runtime X/Y/Z 与 frame bdy/itr
中心；`PhysicsState.GetBodyVolumes/GetItrVolumes` 的剩余调用方是 Editor gizmo；central presentation、
lockstep checksum 与 battle parity snapshot 也不消费这些旧 origin 字段。

本切片因此把边界收敛为：

- registered 且运行时类型精确为 `LF2Character` 的正式路径，不再解析 sprite catalog，也不再物化
  `SpriteX/SpriteY/SpriteZ`；
- 公开兼容 `CharacterMechanics.Step` 仍在 adapter 边界物化旧 origin 和视觉结果；
- 未注册对象、未知派生角色与共享 character-DAT shell 保留原兼容行为，采用 fail-closed，而不是把
  exact 假设扩展到未知类型；
- `LF2SpecialAttack.MakePointCenter` 仍读取特殊攻击自身的 `PS.sx/sy/sz`，该路径没有被修改；
- pass 顺序、逻辑位置/速度、落地、RNG、slot/generation、碰撞候选与表现发布顺序均未改动。

聚焦测试新增了“公开兼容 Step 会物化旧 origin，而 `StepBattleLogic` 不会”的边界断言；逻辑等价测试
不再把 adapter-only 的 SpriteX/Y/Z 当作权威战斗结果。

### 37.2 两轮 1000 AI 性能与等价证据

相同 seed、1000 个真实生产 AI、30 warmup + 180 sampled tick、`data-oriented-canonical` 与完整细分诊断：

| 运行 | logic average/P95 | FrameAdvance average/P95 | Transit average/P95 |
|---|---:|---:|---:|
| 第三十九切片相邻候选 2 | `21.9666/27.3381` | `1.5498` | `0.7599/0.9030` |
| 移除旧 origin 候选 1 | `21.5163/27.8283` | `1.2468/1.6349` | `0.4872/0.6190` |
| 移除旧 origin 候选 2 | `21.5075/27.3200` | `1.2433/1.4582` | `0.4862/0.6295` |

两轮 `Transit` average 稳定下降约 `35.9%～36.0%`，`FrameAdvance` average 稳定下降约
`19.6%～19.8%`，完整 logic average 相邻下降约 `2.1%`。保留依据是目标子阶段双轮稳定正收益、
被删除工作没有生产 consumer、行为 hash 完全相同和正式窗口零 GC；不把短样本 Unity frame 波动写成
U9/FPS 验收。

报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.u6-no-legacy-sprite-origin-candidate-1-20260813.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-no-legacy-sprite-origin-candidate-2-20260813.json`。

两轮 final battle parity hash 均为
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，final lockstep hash 均为
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`；正式 sampled tick 均为
`0 B`，Gen0/1/2 collection 均为 `0/0/0`，authority success，teardown `restored=true`。

### 37.3 fresh 门禁与未关闭事项

- 本地 `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj` 顺序构建：0 error；
- `FrameAdvanceRuntimeSnapshotEditorTests` job `42d562c9abde436b9a36e0fcb82ed9db`：`6/6 PASS`；
- 完整 EditMode job `e3c9c3158b494f0497b96ff46a34c0e1` 执行 `1107/1107`，3 个失败均为 UnityMCP
  短连接注入的 `NetworkStream disposed` 外部 Error；AI/benchmark 复测 job
  `9a6f86f257464871ba81a92accfc27e9` 执行 122 项，仅剩同类日志污染；最后目标 benchmark 独立 job
  `9bee2a80ad714a26b955a76453368cea` 为 `1/1 PASS`。因此没有已知代码断言失败，但完整 job 不能诚实写成
  干净的 `1107/1107 PASS`；
- `BattleRuntimeSelfCheck` 于 `2026-08-13 22:52:14` fresh 写入 `PASS`。

第四十切片仍不代表完整 frame/motion/lifecycle canonical world、对象 shell 退化、U6 或 U9 已完成。
下一轮继续以最新报告中的 `CharacterInput`、`CandidateCollect` 与 `LateEntityUpdate` 为审计入口；S0、
T8 默认 `stage.dat` 与 Android 真机继续排除。

## 38. Collision formal participant 重复 slot 查询负实验（第四十一候选，2026-08-13，已撤回）

### 38.1 候选与等价边界

role-aware formal participant 构建原先先用 `FindEntityByRuntimeSlotForQuery(slot)` 确认当前 occupant，
随后再调用 `TryGetCurrentRuntimeHandle(slot, entity, out handle)`。后者内部已经同时校验 slot 可寻址、
claimed、generation 非零以及当前 occupant 与 `expectedEntity` 引用相等，因此候选在 loose/formal 两条
构建路径中删除了前一次查询，只保留 active-pass、dense duplicate-slot 与 generation handle 校验。

候选没有改变 participant 的 slot 顺序、frame/body/itr 读取、fallback、pair 排序、双向候选消费、RNG
或生命周期边界；聚焦 role-aware collision job `1536c9ba26284e12bc327f5d41a0beec` 为
`68/68 PASS`，本地 runtime 编译 0 error。

### 38.2 两轮 1000 AI 结果与撤回结论

相同 seed、1000 个真实生产 AI、30 warmup + 180 sampled tick、完整细分诊断：

| 运行 | logic average/P95 | CandidateCollect average/P95 | ParticipantBodyItrBuild average/P95 |
|---|---:|---:|---:|
| 第四十切片相邻有效基线 | `21.5075/27.3200` | `3.8676/8.4522` | `1.0824` |
| 单查询候选 1 | `21.6079/26.7753` | `3.8558/7.5360` | `1.0628/1.2833` |
| 单查询候选 2 | `23.7294/32.9758` | `4.1571/8.7464` | `1.1365/1.5780` |

第一轮仅有约 `0.02 ms` 的目标子段差异，第二轮方向反转；没有达到“两轮目标子段稳定正收益”的
晋升门槛。两轮 battle parity hash 均为
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，lockstep hash 均为
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`，正式 sampled tick `0 B`、
teardown `restored=true`。报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.u6-formal-slot-single-lookup-candidate-1-20260813.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-formal-slot-single-lookup-candidate-2-20260813.json`。

候选代码已完整撤回。本实验说明相邻的 slot-table 热数据读取并不是当前 CandidateCollect 的主要瓶颈；
后续不再重复尝试删除这一查询，而应减少完整 participant/frame/geometry 产品的重复构建，或转向
`CharacterInput`、`LateEntityUpdate` 与表现发布中占比更大的完整工作段。本实验不关闭 U6/U9。

## 39. CentralOnly 冻结帧轻量顺序索引（第四十二切片，2026-08-14，已保留）

### 39.1 问题、所有权与兼容契约

第三十四切片已证明 `CentralOnly` 捕获输入按 `(RuntimeSlot, StableId)` 稳定升序，可用 signed-Z radix
对轻量索引排序而无需比较整份宽快照。但旧保留版在索引排序完成后仍执行两次 O(N) 宽行工作：先按
索引把每个 `BattlePresentationEntitySnapshot` 复制到 scratch 并改写 base order，再把 scratch 全量写回
冻结帧。该工作不产生新的战斗真值，也不属于权威 C# 战斗 pass。

本切片将最终物化收敛为冻结帧内持久化的 `rank -> physical index`：

- 物理 `entities` 行继续保持捕获顺序，不再因透明排序被搬运；
- 公共 `GetEntity(rank)` 仍返回按 Z/slot/stable-id 排列的逻辑 rank，并映射 `rank * 4` base order；
- 内部命令构建显式通过 frame 解析 base order；
- `CopyFrom` 同时复制轻量顺序索引，使发布帧、冻结帧与后一份副本的 rank 语义一致；
- `PublishPresentationRenderOrderFromFrame` 继续按相同 rank 发布 handle/slot 顺序；
- 输入不满足稳定 slot 前置条件时仍 fail-closed 到原比较排序，不扩大 fast-path 假设。

聚焦测试同时用反射确认：新路径物理宽行未移动，但外部 rank/base-order 与旧路径完全一致。临时旧宽搬运
A/B 开关只存在于压力验证期间，正式最终代码已删除。

### 39.2 交错 A/B 与确定性证据

相同 seed、1000 个真实生产 AI、30 warmup + 180 sampled tick、完整 phase/presentation/detail timing，运行
顺序为旧宽 A、新索引 B、新索引 C、旧宽 D：

| 运行 | logic average/P95 | SortEntities average/P95 | PresentationPublish average/P95 |
|---|---:|---:|---:|
| 旧宽 A | `20.1126/24.3029` | `0.5354/0.5447` | `6.3666/6.5500` |
| 新索引 B | `20.1030/24.3219` | `0.0955/0.1007` | `5.8531/6.0437` |
| 新索引 C | `20.4083/24.8806` | `0.0950/0.0984` | `5.9250/6.1092` |
| 旧宽 D | `20.0980/24.4925` | `0.5394/0.5506` | `6.3879/6.6761` |

两轮平均后，`SortEntities` 目标段稳定减少约 `82.3%`，完整表现发布约减少 `7.7%`。整 tick average
处于 `20.0980～20.4083 ms` 的 Editor 噪声区间，没有稳定整体 FPS 差异，因此晋升依据只限于删除两次
宽行搬运以及目标子段双轮稳定正收益。

四轮 final battle parity hash 均为
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，final lockstep hash 均为
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`；正式 sampled tick 均为
`0 B`、Gen0/1/2 collection `0/0/0`、teardown `restored=true`。报告：

- `Temp/NTSD_ProductionEntityStress.u6-slice42-wide-a.json`；
- `Temp/NTSD_ProductionEntityStress.u6-slice42-indexed-b.json`；
- `Temp/NTSD_ProductionEntityStress.u6-slice42-indexed-c.json`；
- `Temp/NTSD_ProductionEntityStress.u6-slice42-wide-d.json`。

### 39.3 最终门禁与下一热点

- 本地 `Assembly-CSharp.csproj`、`Assembly-CSharp-Editor.csproj`：0 error；
- 最终相关回归 job `088da7280f28442cbd0d02a18ccefb72`：`277/277 PASS`；
- `BattleRuntimeSelfCheck` fresh 结果文件：`PASS`；
- 无 phase/presentation/detail timing 的正式报告
  `Temp/NTSD_ProductionEntityStress.u6-slice42-final-performance.json`：average/P95/P99/max
  `18.1067/22.3653/24.7464/25.8652 ms/tick`，折算 average/P95 为约
  `55.23/44.71 logical tick/s`；零 GC、hash 不变、teardown 完整恢复。

最新诊断下的主要完整 pass 仍为 `CharacterInput 5.8775 ms`、`CandidateCollect 3.6765 ms`、
`LateEntityUpdate 2.5053 ms`、`FrameAdvance 1.1292 ms`；表现内部 `BuildCommands 2.1689 ms` 与
`CaptureEntities 0.9185 ms` 仍是表现链的主要子段。第四十二切片不关闭 U6/U9；下一切片应优先删除
`CharacterInput/EntityInputPass` 的完整重复产品，或减少 `CandidateCollect` 的完整 participant/frame/
geometry 构建，而不是继续做相邻单查询微优化。S0、T8 默认 `stage.dat` 与 Android 真机继续排除。

## 40. CandidateCollect 排序去重与 exact requirement 单遍融合（第四十三切片，2026-08-14，已保留）

### 40.1 修改边界

正式 role-aware 碰撞路径原先先按 authority ordinal 对 pair key 排序、去重并压缩列表，随后再次遍历全部唯一 pair，
按 `first -> second`、`second -> first` 的既有方向顺序构建 exact requirement。第四十三切片把这两个相邻全量遍历
融合为一次：扫描已排序 pair 时同时跳过重复项、原地压缩唯一 pair，并按原方向顺序生成 exact requirement。

该修改没有改变 participant 构建、role 判定、fallback、pair 排序键、双向候选消费、RNG、slot/generation、命中结算或
生命周期顺序；只删除排序后第二次遍历唯一 pair 的重复工作。临时 A/B 开关已在验证后删除，正式代码只保留融合路径。

### 40.2 交错 A/B 结果

相同 seed、1000 个真实生产 AI、30 warmup + 180 sampled tick、完整 phase/detail timing，运行顺序为 Legacy A、
融合 B、融合 C、Legacy D：

| 运行 | logic average/P95 | CandidateCollect average/P95 | SortDeduplicate average/P95 |
|---|---:|---:|---:|
| Legacy A | `20.2675/24.6518` | `3.6764/7.3868` | `0.201679/0.672275` |
| 融合 B | `20.5447/24.9720` | `3.7465/8.0833` | `0.191565/0.618780` |
| 融合 C | `20.5702/24.5516` | `3.7055/7.4874` | `0.186422/0.591560` |
| Legacy D | `20.7856/25.0867` | `3.7647/7.4648` | `0.201430/0.672265` |

两轮均值中，目标 `SortDeduplicate` 从约 `0.201555 ms` 降到 `0.188994 ms`，约减少 `0.0126 ms/tick`
（`6.2%`）。完整 CandidateCollect 与总 tick 没有形成稳定可归因差异，因此本切片保留依据仅限于：删除一轮确定重复遍历、
目标子段双轮正收益、行为 hash 和零 GC 等价；不能把它描述为 FPS 修复。

A/B 报告：

- `Temp/NTSD_ProductionEntityStress.u6-slice43-legacy-a.json`
- `Temp/NTSD_ProductionEntityStress.u6-slice43-fused-b.json`
- `Temp/NTSD_ProductionEntityStress.u6-slice43-fused-c.json`
- `Temp/NTSD_ProductionEntityStress.u6-slice43-legacy-d.json`

四轮 final battle parity hash 均为
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，final lockstep hash 均为
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`；正式 sampled tick 均为
`0 B`，Gen0/1/2 collection 均为 `0/0/0`，teardown `restored=true`。

### 40.3 fresh 门禁与正式无诊断基线

- `Assembly-CSharp.csproj --no-restore /m:1`：`0 error`，保留 43 个既有 warning；
- role-aware collision 聚焦 EditMode job `33456e70009e422a8040eed0cb247eb9`：`9/9 PASS`；
- 完整 NTSD EditMode job `e38cc317d4424ff9b1ae3b7eaf1ce9a1` 执行 `1108/1108`，5 项仅因 UnityMCP 自身
  `NetworkStream disposed` 外部 Error 污染而失败；受影响类独立 job `45599944c2b14452a5829f053c0602e2`：
  `5/5 PASS`，未发现项目断言失败；
- `BattleRuntimeSelfCheck` 于 `2026-08-14 02:20:28` fresh 写入 `PASS`；
- 最终关闭 phase/presentation/detail timing 的 1000 AI 正式报告
  `Temp/NTSD_ProductionEntityStress.u6-slice43-final-performance.json`：30 warmup + 180 sampled tick，
  average/P95/P99/max 为 `17.8294/22.6115/24.5578/28.1476 ms/tick`，折算 average/P95 约
  `56.09/44.23 logical tick/s`；全部观测 tick 低于 `33.33 ms`，正式窗口 `0 B/tick`、三代 collection `0`、
  hash 不变、hard breach `0`、cleanup exception `0`、teardown 完整恢复。

第四十三切片不关闭 U6/U9。下一步回到占比更大的 `CharacterInput` 与 `LateEntityUpdate`，优先寻找可以整体删除的重复
snapshot/index/query 产品或完整重复全实体遍历，而不是继续优化单个相邻 slot 查询。S0、T8 默认 `stage.dat` 与 Android
真机继续排除。

## 41. CentralOnly 资源解析细分与 trusted identity 热缓存（第四十四切片，2026-08-14，已保留）

### 41.1 诊断结论与安全边界

默认关闭的 presentation 计时器把中央表现物化拆为 frame capture、order、command materialization、resolver
configure、逐命令资源解析/quad 写入、chunk upload 与 submission publish。1000 个真实生产 AI、30 warmup +
180 sampled tick 的基线报告 `Temp/NTSD_ProductionEntityStress.u6-central-materialization-detail.json` 显示：

| 子段 | average | P95 |
|---|---:|---:|
| `Materialize/BuildCommands` | `2.2658 ms` | `2.9553 ms` |
| `Materialize/Mesh/ResolveAndWriteCommands` | `2.8966 ms` | `4.0081 ms` |
| `Materialize/Mesh/UploadChunks` | `0.0537 ms` | `0.0801 ms` |
| `RenderDispatch/PresentationPublishTotal` | `5.4605 ms` | `7.3433 ms` |

因此当前 SetPass/批次已经收敛后，Mesh upload 不是剩余大头；表现 CPU 主要用于重建命令，以及为每条命令解析
资源并写 quad。该结论不能用于跳过命令、改变透明顺序、缓存位置或降低表现更新频率。

本切片只在 resolver 内为 `Shadow/Entity/OverlayGlyph/HitRecord` 保留四个预分配热槽。热槽只接受已经命中正式
trusted resource cache 的 identity；每次命中仍先验证 command render-state 与 logical resource kind，并用当前
command color 生成结果。catalog、common visual、material、array binding 变化，或 `Configure()` 发现已销毁的
Unity 资源时，会与正式模板/trusted cache 一起清空。自定义、无 trusted identity 与 cold command 继续走原解析链。

### 41.2 测量与门禁

详细候选报告 `Temp/NTSD_ProductionEntityStress.u6-central-hot-resource-candidate-detail.json`：

- `ResolveAndWriteCommands` average/P95：`2.7676/2.9419 ms`；
- `PresentationPublishTotal` average/P95：`5.3583/5.8052 ms`；
- logic average/P95：`21.1024/25.8328 ms`，详细基线为 `21.6732/26.6437 ms`；
- `BuildCommands` 没有被本候选优化，average 为 `2.3064 ms`，后续不能把 resolver 收益扩大到完整命令链。

关闭 phase/presentation/detail timing 后的两轮候选：

| 运行 | logic average/P95 | Unity frame average/P95 |
|---|---:|---:|
| candidate 1 | `19.1597/23.5574 ms` | `37.7396/44.0121 ms` |
| candidate 2 | `18.5654/22.8316 ms` | `33.2717/39.3927 ms` |
| 旧同 workload 基线 | `18.5834/23.1075 ms` | `35.0533/41.5899 ms` |

两轮可见帧方向存在 Editor 抖动，不能声明整体 FPS 稳定改善；保留依据仅限详细目标子段与 P95 正向、删除重复
trusted-cache 查找、以及资源失效合同闭合。全部报告的正式 tick allocation 为 `0 B`，PlayerLoop/presentation
allocation 为 0，Gen0/1/2 collection 为 0，battle parity hash
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35` 与 lockstep hash
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063` 保持不变，teardown `restored=true`。

本地 runtime/editor 编译 0 error；resolver 聚焦 EditMode job
`7634b3328afe4f42addd0ec54deb05f7` 为 `22/22 PASS`，覆盖重复 identity、当前颜色、command envelope 与已销毁资源
清除热槽。该切片不关闭 U6/U9；下一步应寻找可以删除完整 command materialization、逻辑 tick 外主线程重复工作或
其他占比足够大的完整产品，不再把相邻单次 dictionary/cache 查询当成 30 FPS 的主要解法。

## 42. Prepared entity binding 直读负实验与完整帧归因（第四十五候选，2026-08-14，已撤回）

### 42.1 候选与撤回理由

候选尝试让 `BattleCatalogCentralResourceResolver.ResolvePrepared` 对 Entity 命令直接读取已经随 snapshot 捕获的
`BattleSpriteEntry.CentralBinding`，绕过 trusted-resource hash lookup。该做法没有改变 command 顺序、颜色、材质语义、
catalog generation 或资源销毁失效边界，并先通过 resolver 聚焦测试 `22/22 PASS`；但相同 seed、1000 个真实生产 AI、
30 warmup + 180 sample 的详细 A/B 明确为负：

| 指标 | 相邻基线 | 候选 | 变化 |
|---|---:|---:|---:|
| `ResolveCommands` average | `1.3815 ms` | `1.5441 ms` | `+11.8%` |
| `ResolveAndWriteCommands` average | `2.9161 ms` | `3.1304 ms` | `+7.3%` |
| `PresentationPublishTotal` average | `5.2677 ms` | `5.4886 ms` | `+4.2%` |
| logic average/P95 | `20.4836/24.8472 ms` | `20.8102/26.0433 ms` | 方向为负 |

报告分别保存为：

- `Temp/NTSD_ProductionEntityStress.u6-prepared-entry-direct-phase-baseline.json`；
- `Temp/NTSD_ProductionEntityStress.u6-prepared-entry-direct-negative-phase.json`；
- `Temp/NTSD_ProductionEntityStress.u6-prepared-entry-direct-negative.json`。

候选代码、计数器与测试期望已完整撤回；第四十四切片已验证的四槽 trusted identity 热缓存继续保留。撤回后本地
runtime/editor 构建 0 error，resolver 聚焦 job `53f90282f6c54097bb08cced170e9480` 为 `22/22 PASS`。最终无细分诊断的
1000 AI 回归 logic average/P95/P99/max 为 `19.1309/23.7975/26.8002/27.7176 ms`，正式窗口 `0 B`、三代
collection `0`、battle/lockstep hash 不变、teardown `restored=true`。该实验关闭“从已捕获 Entry 再直接读取 binding
即可降低 resolver 成本”的方向；不能继续为单次 lookup 增加更多验证分支。

### 42.2 完整帧复测与当前低帧率解释

撤回后使用 `Run 1000 AI Completed Frame Timing Diagnostic` 重新采集 180 个 sampled tick、179 个已完成渲染帧，
报告为 `Temp/NTSD_ProductionEntityStress.combat1000.completed-frame-timing.json`：

| 域 | average | P95 | P99 |
|---|---:|---:|---:|
| logic tick | `18.3401 ms` | `22.7304 ms` | `24.7410 ms` |
| Unity frame interval | `33.9777 ms` | `44.1269 ms` | `79.7169 ms` |
| completed frame CPU | `29.7821 ms` | `36.5115 ms` | `38.3154 ms` |
| completed frame main thread | `24.9588 ms` | `31.2782 ms` | `33.0017 ms` |
| render thread | `0.6530 ms` | `0.8353 ms` | `0.9809 ms` |
| GPU | `1.9164 ms` | `3.3919 ms` | `4.5731 ms` |

本轮 `maximumCatchUpTicksInFrame=1`、`framesWithCatchUp=0`，所以不是单机错误触发四 tick 追帧；正式 tick allocation
和 Gen0/1/2 collection 均为 0，hash 不变、teardown 完整恢复。与归档基线
`Temp/NTSD_ProductionEntityStress.completed-frame-before-revert-recheck-20260814.json` 相比，main-thread P95 只从
`29.9378 ms` 增至 `31.2782 ms`，逻辑 P95 只从 `22.3245 ms` 增至 `22.7304 ms`，不足以解释 Game 视图从
30～50 FPS 降到个位数；当前可见低帧率主要由 Editor/Profiler 帧调度和逻辑 tick 外 CPU 尾延迟放大，不能归因于
GPU、render thread 或已撤回候选，也不能假设后续会自动恢复。

下一候选仍必须针对可删除的完整表现产品或逻辑 tick 外重复主线程工作做相邻 A/B；不能使用 Game 视图 Stats、单帧
Profiler 尖峰或短样本 Unity frame interval 代替 completed-frame main/render/GPU 与 Windows Player 的 U9 门禁。

## 43. Late common no-op 门禁（第四十六切片，2026-08-14，已保留）

### 43.1 删除的工作与兼容边界

权威 C# `GameTick.RunLateEntityUpdate` 在同一实体真值上按顺序执行 state-special、恢复、frame tick、死亡/opoint、
cleanup 与 tail。Unity 的虚方法兼容链此前即使对普通角色必然无事可做，也会逐实体进入以下分支。本切片只在 exact
`LF2Character` 且前置条件能够证明 no-op 时跳过调用：

- state 不是 `9995`、`4000..4999` 或 `8000..8999` 时跳过 state-special；
- 当前 tick 不落在 HP/PP 恢复周期时跳过恢复链；
- 角色 HP 仍大于 0 时跳过死亡 opoint；
- 普通角色不进入仅供飞行武器使用的 post-opoint cleanup。

未知派生类型、特殊 state、恢复周期、死亡角色、武器/特殊攻击与显式 Legacy A/B 开关全部 fail-closed 回到原链；
frame tick、opoint flush、slot 顺序、生命周期边界和最终 runtime snapshot 不变。

### 43.2 A/B 与门禁

报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.u6-late-common-noop-candidate.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-late-common-noop-legacy.json`。

相同 seed、1000 个真实生产 AI、30 warmup + 180 sample 下：

| 模式 | logic average | `LateEntityUpdate` average |
|---|---:|---:|
| candidate | `19.9376 ms` | `2.5825 ms` |
| Legacy | `20.0362 ms` | `2.6596 ms` |

目标 pass 约减少 `0.077 ms/tick`，属于低风险 no-op 删除，不是 1000 AI 帧率跃升。两轮 parity/lockstep hash 一致、
正式 tick 0 B、Gen0/1/2 collection 0、teardown 完整恢复；fresh 聚焦 job
`de4a07e46e84474687849f106098d9a0` 为 `15/15 PASS`。

## 44. CollisionSnapshot 双 roster 跨 pass 复用（第四十七切片，2026-08-14，已保留）

### 44.1 首版负实验与正式所有权

`CollisionSnapshot` 已经按 runtime slot 升序访问本 tick 的活动实体，旧 `CandidateCollect` 随后又通过 world 查询重新
构建全实体产品，并再次筛选 body/itr role。首版候选在快照 pass 保存实体与 runtime handle，并在下一 pass 对两份
roster 全量重复验证 handle；1000 AI 中 `CandidateCollect/CacheSetup` 从约 `0.2827 ms` 回退到 `0.5429 ms`，总 tick
也由 `20.4245 ms` 回退到 `21.2534 ms`。该版本已舍弃，证明不能为删除一次 world 查询而新增等量代际查找。

正式版维护两份语义不同、均复用容量的引用 roster：

- all-entity roster：保留所有有效实体，继续承担旧候选状态清空、legacy candidate list 与兼容语义；
- formal-participant roster：保留所有未 suppress 的参与者，包括没有 body/itr 或无法构建 AABB 的 inert 行，供
  role-aware formal builder 使用。后两类必须继续进入保守 fallback/narrow-phase 合同，不能在快照阶段提前删除。

跨 pass 复用只在 tick 相同且 `RuntimeSlotOccupancyEpoch` 未变化时生效；slot table 在扩容、claim、allocation、release 与
reset 时都会推进该 epoch。正式 formal builder 仍对实际参与者验证 runtime slot、实体引用、去重槽与 generation handle；
因此精简的是快照与候选之间的重复验证，不是删除碰撞入口的安全校验。若 pass 中断、tick/epoch 改变、使用 Legacy
诊断开关或 roster 合同不成立，则 fail-closed 回到 `_world.GetAllEntities`。

初版 role-only 分类曾令 full self-check 的“无 body 参与者必须进入 conservative fallback”断言失败：formal
participant 仍显示 2，但 fallback 从 1 变成 0。该分类已撤销；正式版显式保留 inert participant，并由 formal builder
决定 body/itr/fallback role。这个失败也说明聚焦的 13 项 roster/zero-itr 测试不能替代 formal collector 全矩阵与完整
self-check。

### 44.2 ABBA 结果

同一 seed `1314149188`、1000 个真实生产 AI、30 warmup + 180 sample、每渲染帧最多 1 tick：

| 运行 | logic average/P95 | `CandidateCollect` average | `CollisionSnapshot` average | CacheSetup average |
|---|---:|---:|---:|---:|
| candidate A | `20.2993/24.5710 ms` | `3.5432 ms` | `0.4762 ms` | `0.2165 ms` |
| Legacy B | `20.1337/24.3604 ms` | `3.5819 ms` | `0.4721 ms` | `0.2786 ms` |
| Legacy B repeat | `20.8226/25.3248 ms` | `3.7641 ms` | `0.4995 ms` | `0.2991 ms` |
| candidate A repeat | `20.2089/24.7555 ms` | `3.5471 ms` | `0.4800 ms` | `0.2148 ms` |

候选目标 pass 两轮都低于其交错 Legacy，目标段均值为 `3.5452 ms`，Legacy 为 `3.6730 ms`；总 tick 双轮均值
候选为 `20.2541 ms`，Legacy 为 `20.4782 ms`，但单轮 total 一正一反，不能声明稳定整体 FPS 增益。四轮 20 个最终
parity/lockstep hash 字段、workload fingerprint 完全一致，正式 tick 0 B、Gen0/1/2 collection 0、harness/authority
有效且 teardown `restored=true`。报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.u6-collision-snapshot-roster-lean-candidate.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-collision-snapshot-roster-lean-legacy.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-collision-snapshot-roster-lean-legacy-repeat.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-collision-snapshot-roster-lean-candidate-repeat.json`。

初版相关聚焦 job `328fd6bdfff34f5583aa1d9fee30fe52` 为 `13/13 PASS`，但未覆盖上述 inert conservative fallback；
修正后的 fresh runtime/editor 编译为 0 error，扩大矩阵 job `4ff0c6d5320a4c788af73b864f419fd3` 为
`69/69 PASS`，`BattleRuntimeSelfCheck` 于 `2026-08-14 15:51:12 PASS`。该切片保留的是跨 pass canonical 产品
复用和稳定目标子段收益；它没有减少本场景 1000 个均具备 collision role 的正式 participant 数量，也不关闭 U6/U9。

最终关闭全部碰撞细分诊断后的生产回归报告
`Temp/NTSD_ProductionEntityStress.combat1000.u6-collision-snapshot-roster-final.json` 为 1000 个真实生产 AI、
30 warmup + 180 sample，logic average/P95/P99/max 为
`20.8148/25.8390/29.9369/34.3540 ms/tick`；正式 tick 为 `0 B`，Gen0/1/2 collection 均为 `0`，
parity hash `752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`、lockstep hash
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`，teardown 完整恢复。该报告仅作为
关闭诊断后的新鲜回归证据，不改变 ABBA 的归因口径。

## 45. Post-frame 宽 Runtime 快照删除（第四十八切片，2026-08-14，已保留）

### 45.1 权威字段契约与适用边界

权威 C# `BattleCore/Simulation/GameTick.cs` 的 `ApplyFramePostProcess` 与 `RunEntityPostframeTail` 直接更新同一份实体
runtime 真值：速度、hit count、knockback、恢复计时器、HP 和候选 carrier；pass 尾不存在“对象 shell 再全量复制回
runtime”的第二套所有权。Unity 正式 exact `LF2Character` 的 `PhysicsState`、`LF2Health`、`HitCount`、
`KnockbackV*`、`HealTimer` 与 `CatchTimer` 已绑定 `Runtime`，所以两个 pass 尾部的完整
`RefreshRuntimeSnapshot()` 是对已经提交真值的重复宽复制。

本切片新增 `LF2Entity.RefreshRuntimeSnapshotAfterPostFrameMaintenance()`：只对 exact `LF2Character` 返回“不需要
复制”；未知派生实体仍调用虚拟 `RefreshRuntimeSnapshot()`，保持 fail-closed。两个 pass 的遍历顺序、条件、数值写入、
候选清理和生命周期边界均未改变；独立 Legacy 诊断开关可恢复原来的宽复制。

### 45.2 Fresh 验证与独立 A/B

Fresh script refresh 为 0 C# error；停止误留的 Play Mode 后，聚焦 job
`8d8a5d6a20be4480bb2d9c1cfcc9db81` 为 `16/16 PASS`，配置聚焦 job
`c065bd35c2634afb88ddbe6342e442bf` 为 `1/1 PASS`，`BattleRuntimeSelfCheck` 于
`2026-08-14 16:07:45 PASS`。聚焦测试还验证 4096 次 warmed 调用为 `0 B`。

同一 seed `1314149188`、1000 个真实生产 AI、30 warmup + 180 sample、每渲染帧最多 1 tick：

| 模式 | logic average/P95/P99/max | `FramePostProcess` average/P95 | `EntityPostFrameTail` average/P95 |
|---|---:|---:|---:|
| candidate | `19.7140/23.6160/27.0044/29.2400 ms` | `0.1581/0.1682 ms` | `0.1787/0.1993 ms` |
| Legacy | `20.3351/24.3788/27.3286/28.4246 ms` | `0.5995/0.6477 ms` | `0.6030/0.6338 ms` |

两个目标 pass 合计约减少 `0.866 ms/tick`，完整 tick average 约减少 `0.621 ms`。candidate 的 skip counter 为
`206209/210000`，Legacy 为 `0/0`。两轮 workload fingerprint、最终 parity hash 与 lockstep hash 完全一致，正式 tick
均为 `0 B`、三代 collection 均为 `0`、harness/authority 有效且 teardown `restored=true`。报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.u6-postframe-snapshot-candidate.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-postframe-snapshot-legacy.json`。

这是可归因的正收益切片，但只关闭两个 post-frame 重复数据产品，不关闭 U6/U9。

## 46. 共享 frame 的 role-aware body 模板正式 A/B（第四十九切片，2026-08-14，已保留）

### 46.1 实现边界

`BruteForceSceneQuery` 已对同一 `LF2FrameData` 的 role-aware formal body 建立 tick 内共享模板：DAT `bdy`
局部形状只解析一次，各实体仍按自己的逻辑位置、朝向、对象类型与 Z 规则物化世界 AABB。模板只复用不可变的 frame
局部数据，不缓存实体位置，不改变 participant、pair、fallback、排序、双向 exact requirement 或候选消费顺序；模板无法
安全物化时继续走原 `TryBuildRoleAwareFormalBodyAabb`，因此未知/退化输入保持 fail-closed。

本轮没有重写碰撞算法，只把已有 `ForceLegacyRoleBodyBuildForDiagnostics` 接入 1000 AI 压力工具与实现指纹，并补充请求、
应用状态和模板 build/hit/fallback 计数，使正式模板路径与强制 Legacy body 构建可以做独立 A/B。

### 46.2 A/B 结果

同一 seed `1314149188`、1000 个真实生产 AI、30 warmup + 180 sample、每渲染帧最多 1 tick：

| 模式 | logic average/P95/P99/max | `CandidateCollect` average | `ParticipantBodyItrBuild` average/P95 |
|---|---:|---:|---:|
| 共享模板 | `19.7479/24.5972/26.9161/29.2574 ms` | `3.6405 ms` | `1.0124/1.1093 ms` |
| 强制 Legacy | `19.9427/24.4388/27.3327/32.1806 ms` | `3.9221 ms` | `1.2756/1.4469 ms` |

共享模板令目标 participant build 平均减少 `0.2631 ms/tick`、P95 减少 `0.3376 ms`，完整 CandidateCollect 平均减少
`0.2816 ms/tick`，完整 tick 平均减少 `0.1948 ms`、P99 减少 `0.4166 ms`、最大值减少 `2.9232 ms`。单轮 logic
P95 反而高 `0.1583 ms`，因此只按目标子段的可归因收益保留，不宣称所有分位或 Editor 可见 FPS 稳定提高。模板路径最大
观察到 build/hit/fallback 为 `74/999/0`。

两轮 workload fingerprint、最终 parity hash
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35` 与 lockstep hash
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063` 完全一致；正式 tick `0 B`、Gen0/1/2
collection 均为 `0`、teardown `restored=true`。新增压力配置聚焦测试 job
`6a4fb1a915054b8e8b8b0bb2df25cc72` 为 `1/1 PASS`。报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.u6-role-body-template-candidate.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-role-body-template-legacy.json`。

该切片确认现有共享模板是小幅正优化，不是剩余 1000 AI 帧率问题的主因，也不关闭 U6/U9。

## 47. Late 空分支活跃检查裁剪（第五十五切片，2026-08-14，已保留）

### 47.1 完整调用与生命周期边界

`RunLateEntityUpdateAll` 的 slot 入口使用 `FindEntityByRuntimeSlotCurrent`，成功返回时已经证明实体属于当前活动
generation，因此紧随其后的 `IsActiveForCurrentPass` 是重复读取。现有 exact `LF2Character` common no-op 门禁还会
证明 state-special、recovery、death-opoint 或 post-opoint cleanup 没有调用任何对象方法；这些空分支之后的活动检查
同样不可能观察到新的生命周期变化。

正式切片只删除上述两类冗余检查：首次 current-slot 解析后的重复检查直接删除；common 阶段只有在实际调用对象方法后
才检查；opoint 只有在 `ProcessLateOpointSegment` 确实运行后才检查。frame tick、virtual/derived、死亡、恢复周期、特殊
state、实际 opoint、cleanup、tail 与 flush 路径全部保留原有检查和早退顺序。该改动不删除权威方法，不改变 slot/pass/
RNG/flush 顺序，不创建新的运行时容器，也不改变未知派生类型的 fail-closed 路径。

### 47.2 同种子 A/B 与最终回归

相同 seed、1000 个真实生产 AI、30 warmup + 180 sample：

| 模式 | logic average/P95 | `LateEntityUpdate` average/P95 |
|---|---:|---:|
| 候选 | `19.4271/23.7519 ms` | `2.4989/2.8144 ms` |
| 强制旧 common 路径 | `19.5537/24.0064 ms` | `2.6228/2.8793 ms` |

目标阶段 average/P95 分别改善约 `4.72%/2.25%`，整 tick 只改善约 `0.65%/1.06%`，因此按目标子段的可归因
收益保留，不扩大成稳定整体 FPS 提升。两轮 workload fingerprint、parity/lockstep hash 完全一致，正式 tick `0 B`、
Gen0/1/2 collection 为 `0`、teardown `restored=true`。报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice55-late-activecheck-candidate.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice55-late-activecheck-legacy.json`。

最终关闭 phase/presentation/detail 探针的生产回归
`Temp/NTSD_ProductionEntityStress.combat1000.u6-slice55-late-activecheck-final-notiming.json` 为
logic average/P95/P99/max `17.5980/22.6316/27.0556/31.0396 ms`，正式 tick `0 B`、三代 collection 均为 `0`、
hash 不变、teardown 完整恢复。主工程与 Editor 工程编译 `0 error`，聚焦 job
`cb7f149328f04c36a66d9f9bc37edbf4` 为 `31/31 PASS`，`BattleRuntimeSelfCheck` fresh `PASS`。该切片不关闭
U6/U9；下一候选仍必须优先寻找完整字段产品或对象 shell 循环，而不是继续堆叠单次 lookup 微优化。

## 48. 共享 frame 的 role-aware ITR 局部模板（第五十六切片，2026-08-14，负实验，已完整回退）

### 48.1 候选与等价边界

候选曾尝试按 `LF2FrameData` 引用缓存 ITR 的局部几何，再为各实体按逻辑位置、朝向与 Z 规则物化世界 AABB。
它保留原 ITR 索引、引用、遍历顺序、普通 X 区间保守夹紧和 `y == int.MinValue` 的未检查 X 语义，并提供强制
Legacy 构建开关；聚焦测试覆盖左右朝向、退化范围和边界值，fresh EditMode job
`e1f721469dd54b8dbf21a216d5ea4a92` 为 `2/2 PASS`。

### 48.2 同种子 A/B 与回退结论

同一 seed `1314149188`、1000 个真实生产 AI、30 warmup + 180 sample、启用相同细分计时：

| 模式 | logic average/P95 | `CandidateCollect` average/P95 | `ParticipantBodyItrBuild` average/P95 |
|---|---:|---:|---:|
| ITR 模板 | `19.8974/24.4456 ms` | `3.9053/8.3809 ms` | `1.2133/1.3894 ms` |
| 原始直接构建 | `19.4754/23.7781 ms` | `3.7219/7.5918 ms` | `1.0602/1.2178 ms` |

候选令目标 participant build 平均回退约 `14.4%`，完整 `CandidateCollect` 平均回退约 `4.9%`，完整 tick
平均回退约 `2.2%`。模板 build/hit/fallback 最大观察值为 `74/999/0`，但额外字典查询和模板物化成本高于省下的
局部几何计算，因此该抽象在当前生产数据分布下是负优化。

两轮 workload fingerprint
`28509dc1396e57e0ee35dc46024c2e4eaf4dbbd7bf191a2859a28dac2aa6d490`、parity hash
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35` 与 lockstep hash
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063` 一致；正式 tick `0 B`、Gen0/1/2
collection 均为 `0`、teardown 完整恢复。报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice56-role-itr-template-candidate.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice56-role-itr-template-legacy.json`。

候选实现、诊断开关、压力配置和临时测试已全部回退；回退后主工程与 Editor 工程编译均为 `0 error`，fresh
`NTSD_W08Regression` job `38946463643948d994650b1bdd19d92a` 为 `4/4 PASS`。后续不应重试“为 ITR 再建一套
frame 字典/局部模板”的方向；若继续处理 ITR，只允许复用正式广阶段已经生成的同 tick 产品，并必须独立 A/B。

## 49. 正式广阶段 ITR 世界矩形复用（第五十七切片，2026-08-15，已保留）

### 49.1 实现边界

第五十六批证明“另建 frame/ITR 字典”是负优化，但 role-aware 正式广阶段在构建 `RoleAwareFormalItrEntry` 时已经按
当前实体的位置、朝向与碰撞帧算出了同 tick `WorldRect`；随后 exact ITR cache 又按相同 ITR 索引重复计算一次。
本切片只把前一阶段已经生成的世界矩形随正式 entry 保存，并让 exact cache 在以下合同全部成立时复用：

- participant 的正式 ITR entry 区间连续且仍属于同一 participant；
- entry 的 `ItrIndex` 与 exact loop 当前索引一致；
- entry 保存的 `InteractionArea` 与当前引用相同。

exact loop 本身、null ITR 跳过、ITR 索引、候选顺序、双向 pair 消费、RNG、slot/generation 校验与 conservative
fallback 均未删除。任一合同不成立时立即执行原 `ItrWorldRect` 计算；因此这不是新的缓存系统，也没有增加字典、每 tick
分配或跨 tick 可变状态。独立诊断开关 `ForceLegacyFormalItrWorldRectReuseForDiagnostics` 只用于 A/B 恢复原计算。

### 49.2 同种子 A/B/B/A

同一 seed `1314149188`、1000 个真实生产 AI、30 warmup + 180 sample、相同 phase/presentation/detail 诊断：

| 运行 | logic average/P95 | `CandidateCollect` average/P95 | `PairExactLoop` average/P95 | `ParticipantBodyItrBuild` average |
|---|---:|---:|---:|---:|
| candidate A | `19.6687/23.8528 ms` | `3.7105/7.6913 ms` | `0.6513/2.4694 ms` | `1.0531 ms` |
| Legacy B | `22.3390/28.8752 ms` | `4.1544/8.9754 ms` | `0.7471/2.6960 ms` | `1.1730 ms` |
| Legacy B repeat | `19.9690/24.6686 ms` | `3.7674/8.2958 ms` | `0.6548/2.5653 ms` | `1.0564 ms` |
| candidate A repeat | `19.5378/24.1356 ms` | `3.6564/7.3618 ms` | `0.6443/2.4596 ms` | `1.0207 ms` |

两轮 candidate 的 `PairExactLoop` 平均值为约 `0.6478 ms`，两轮 Legacy 为约 `0.7010 ms`，目标子段平均改善约
`7.58%`；P95 两轮均值约由 `2.6306 ms` 降至 `2.4645 ms`，改善约 `6.31%`。candidate 在两组相邻交叉比较中
均低于 Legacy，因此按目标 exact 子段的可归因收益保留。第一轮 Legacy 的完整 tick 明显偏慢，故不把四轮完整 tick
均值差异扩大为稳定整体 FPS 收益。

四轮 workload fingerprint 均为
`28509dc1396e57e0ee35dc46024c2e4eaf4dbbd7bf191a2859a28dac2aa6d490`，parity hash 均为
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，lockstep hash 均为
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`；正式 tick `0 B`、Gen0/1/2
collection 均为 `0`，teardown 全部恢复且 cleanup exception 为 `0`。报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice57-exact-itr-reuse-candidate.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice57-exact-itr-reuse-legacy.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice57-exact-itr-reuse-legacy-repeat.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice57-exact-itr-reuse-candidate-repeat.json`。

关闭全部高频计时后的最终生产回归
`Temp/NTSD_ProductionEntityStress.combat1000.u6-slice57-exact-itr-reuse-final-notiming.json` 为
logic average/P95/P99/max `17.3797/21.6857/24.0533/24.8405 ms`，正式 tick `0 B`、三代 collection 均为
`0`、hash 不变且 teardown 完整恢复。该报告禁用了 completed-frame timing，因此只证明逻辑 tick 与零分配，不作为
显示帧率或完整 PlayerLoop 证据。

最终代码状态下主工程与 Editor 工程串行构建均为 `0 error`；单切片聚焦 job
`00d3d93e8ecd44fda4e630148b9acd5f` 为 `1/1 PASS`，扩大 formal collector job
`4fb0c72edefe48229c2de8a6304250c3` 为 `57/57 PASS`，`BattleRuntimeSelfCheck` 于
`2026-08-15 00:22:57 PASS`。该切片只删除同 tick ITR 世界矩形的重复计算，不关闭 U6/U9；下一批继续按 fresh
热点选择可删除的完整数据产品或循环，S0、T8 默认 `stage.dat` 与 Android 真机边界不变。

## 50. 朝向 canonical 值存储与单向兼容镜像（第五十八切片，2026-08-15，已保留）

### 50.1 所有权边界

权威 C# 的实体朝向是战斗 runtime 状态，不由 Unity 表现或物理镜像定义。Unity 迁移期的
`NTSDEntityRuntime.Dir` 原先直接保存字符串，同时 `PhysicsState.dir` 保留另一份兼容状态；热路径会反复比较
`"left"`/`"right"`，而 frame/motion store 已经使用 byte 表示朝向。本切片把 runtime 内部 owner 改为
`facingLeft` byte，并保留 `Dir` 字符串 facade 供现有调用方兼容；getter 只返回 interned 字面量，setter 将非
`left` 输入规范化为 `right`，并直接发布到已绑定的 generation-owned frame/motion store。

`PhysicsState.dir` 没有被改成 owner。首版曾让该属性反向写入 runtime，但完整 self-check 立即在“抓取同步必须使用
左向 `Runtime.Dir`，即使 `PS.dir` 仍是陈旧的 right”断言处失败，证明该镜像允许陈旧且不能反向覆盖战斗真值。首版
反向绑定已撤回；最终 `SwitchDir` 先写 runtime、再单向刷新 `PhysicsState.dir` 与 sprite，手工改变 PS 镜像不会改变
runtime。该失败作为所有权边界证据保留，没有修改 self-check 来迁就候选。

### 50.2 验证与 1000 AI 回归

最终状态下 runtime 与 Editor 工程编译均为 `0 error`；fresh Unity 聚焦 job
`d5f673de3fd9478cb6b58432816987f1` 为 `58/58 PASS`，其中新增测试覆盖 byte owner、非法值规范化、
runtime→PS 单向同步、陈旧 PS 不反写及 4096 次 warmed 读写 `0 B`；`BattleRuntimeSelfCheck` 于
`2026-08-15 00:45:44 PASS`，重新覆盖了首版失败的抓取朝向场景。

关闭 phase/presentation/detail 计时的生产回归
`Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-performance-smoke.json` 使用相同 seed
`1314149188`、1000 个真实生产 AI、30 warmup + 180 sample，logic average/P95/P99/max 为
`17.4333/21.4082/23.4661/33.8515 ms`。相对第五十七批最终回归的 average 约高 `0.31%`、P95 约低
`1.28%`，属于短样本噪声，不能声明新的 FPS 收益或回退。两轮最终 parity overall
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35` 完全一致；新回归正式 tick
`0 B`、Gen0/1/2 collection 均为 `0`、teardown `restored=true` 且 cleanup exception 为 `0`。

因此该切片按“单一 owner、值类型 canonical 存储、兼容 facade 不分配、完整行为不变”保留，而不是按可见性能提升
保留。它没有删除 `PhysicsState.dir` 的 Unity compatibility facade，也不关闭 U6/U9；后续继续处理仍占主要成本的
完整对象式产品或循环，S0、T8 默认 `stage.dat` 与 Android 真机边界不变。

## 51. 碰撞几何朝向读取收敛（第五十九切片，2026-08-15，已保留）

### 51.1 权威与实现边界

权威 C# `BattleCore/Interaction/CollisionCollect.cs` 的粗矩形、精确 ITR/BDY 矩形与同朝向过滤均直接读取
`Entity.Facing`；`BattleCore/Entity/Entity.cs` 又把该字段代理到 `Runtime.Transform.Facing`。因此碰撞链不能读取
允许陈旧的 Unity `PhysicsState.dir` 镜像。Unity 原实现仍在 role-aware body 物化、exact common cache、cache
失效检查、同朝向过滤、普通世界矩形与 EXE 溢出语义世界矩形六处读取 `PS.dir`，会在 Runtime 已翻转而表现镜像尚未
同步时构造错误的碰撞矩形或错误过滤 pair。

本切片将上述完整碰撞几何链统一为 Runtime 朝向。正式 role-aware 路径已经要求非空 Runtime，故直接读取
`Runtime.IsFacingLeft`；兼容几何 helper 对没有 Runtime 的未知测试/适配对象仍 fail-closed 回退到 `PS.dir`，没有
改变其位置 fallback、矩形公式、溢出语义、slot 顺序、pair 顺序、双向消费、RNG 或生命周期。`PhysicsState.dir`
继续保留为 Unity 单向兼容镜像，不被删除，也不反写战斗真值。

新增聚焦用例故意令攻击者 `Runtime` 向左而 `PS.dir` 保持向右，并使用只有左向镜像后才相交的非对称 ITR/BDY；
BruteForce 与 role-aware collector 都必须产生同一候选。原 self-check 的 kind5 朝向夹具也改为分别写 Runtime 右/左，
同时把 PS 故意设为相反方向，从而持续证明 shadow broadphase 与正式 collision collect 使用同一 canonical owner。

### 51.2 验证与性能口径

主工程与 Editor 工程串行构建均为 `0 error`；Unity 聚焦 job
`bc63d448304947ad84c1e1ab4f71a2d0` 为 `59/59 PASS`，其中新增陈旧镜像碰撞用例通过；
`BattleRuntimeSelfCheck` 于 `2026-08-15 01:08:24 PASS`。第一次测试请求因 Editor 仍在 Play Mode 而执行 0 条，
已停止 Play Mode 后重跑，不计作代码失败。

相同 seed `1314149188`、1000 个真实生产 AI、30 warmup + 180 sample 的两轮无细分计时回归为：

| 报告 | logic average/P95/P99/max |
|---|---:|
| `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice59-runtime-facing-runA.json` | `19.7072/33.7398/39.7330/40.9823 ms` |
| `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice59-runtime-facing-runB.json` | `20.4907/31.2362/37.6833/41.7351 ms` |

两轮最终 parity overall 均为
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，与第五十八批一致；正式 tick
`0 B`、Gen0/1/2 collection 均为 `0`、teardown `restored=true` 且 cleanup exception 为 `0`。两轮 average
高于第五十八批单轮、P95 又相互反向波动；本切片只把字符串镜像读取替换为 canonical bool 读取，且本 workload 的最终
hash 完全相同，因此这些 Editor 尾延迟不能归因成该切片的性能回退或收益。切片按权威正确性与单一 owner 收敛保留，
不关闭 U6/U9；下一批继续寻找能删除完整对象产品或热循环的迁移，而不是继续做零散字符串替换。

## 52. PreInteraction 跨 pass 中性证明复用（第六十切片，2026-08-15，已保留）

### 52.1 权威顺序与实现边界

权威 C# `BattleCore/Simulation/GameTick.cs` 的相关顺序是 FrameAdvance、死亡/复活清理、StageBounds、
PreInteraction。Unity 保持同一顺序。旧 `PreInteractionTickAll` 为了判断整个 pass 是否为 no-op，会重新遍历全部逻辑
slot、解析 generation handle，并逐角色检查 cpoint/link/held 中性条件；而紧邻的
`PostFrameAdvanceDeathCleanupAll` 已经按活动 runtime slot 遍历同一批实体，并在可能的 respawn 与 runtime refresh 后
得到 PreInteraction 所需的最终状态。

本切片不删除死亡清理的整数位置同步，也不删除其稳定 roster。它只在该既有遍历内同时累计中性证明，并发布一个
world-owned、同 tick 的值产品。产品记录：

- tick；
- logical capacity 与 claimed count；
- runtime-slot occupancy epoch；
- pending-destroy epoch；
- pending-unregister count；
- 已证明参与者数量与整体有效位。

StageBounds 位于发布与消费之间，但只按权威合同夹取 Z，不修改 cpoint kind、link、holder 或 held sync 条件。消费时任一
epoch/容量/队列合同变化，或遍历中遇到未知派生类型、缺失 Runtime、非中性参与者，都会 fail-closed 回到旧的完整证明与
正式 PreInteraction。正式 pass 顺序、slot 顺序、结构变化可见边界和旧诊断 oracle 均未改变，也没有每 tick 分配。

### 52.2 同种子 A/B、零 GC 与回归

相同 seed `1314145092`、1000 个真实生产 AI、30 warmup + 180 sample、相同 workload 的两轮 candidate 与两轮
Legacy：

| 模式 | logic average | logic P95 两轮均值 | `DeathCleanup` average | `PreInteraction` average | 两段合计 |
|---|---:|---:|---:|---:|---:|
| candidate 两轮均值 | `18.6477 ms` | `23.1926 ms` | `0.3635 ms` | `0.6882 ms` | `1.0517 ms` |
| Legacy 两轮均值 | `18.9709 ms` | `23.4993 ms` | `0.3623 ms` | `0.8584 ms` | `1.2208 ms` |

候选令 `PreInteraction` 平均改善约 `19.83%`，`DeathCleanup + PreInteraction` 合计改善约 `13.85%`，完整
logic tick 平均改善约 `1.70%`。候选每轮有 70 tick 命中跨 pass 证明，Legacy 为 0；四轮的 whole-pass no-op
命中数量相同，证明 A/B 只改变证明产品的取得位置，没有扩大可跳过范围。

四轮 workload fingerprint 均为
`37d80928c85a27521a131a6a4b513b9b9a875ba1e7c7aaa9e741d5066f261f63`，lockstep overall 均为
`5e12d2d7089fa6b53aa6261b8020063de9d9906f55cdc1d8e44b9f65f42b61c2`，parity overall 均为
`df2df828f6681af4c8ceb5b8e2fdd68d46e064884b95cb27af9c6bb747ab4eb5`；正式 tick 0 B、Gen0/1/2
collection 0，teardown 全部恢复。报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice60-preinteraction-crosspass-candidate-A.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice60-preinteraction-crosspass-candidate-B.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice60-preinteraction-crosspass-legacy-A.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice60-preinteraction-crosspass-legacy-B.json`。

关闭 phase/presentation/detail 高频探针后的最终回归
`Temp/NTSD_ProductionEntityStress.combat1000.u6-slice60-preinteraction-crosspass-final-notiming.json` 使用 120
warmup + 300 sample，logic average/P95/P99/max 为
`15.7364/17.7918/19.1523/19.3988 ms`，236 tick 命中跨 pass 证明，正式窗口 0 B、三代 collection 0，
最终清理完整恢复。该报告只证明逻辑 tick 明显低于 30 Hz 的 33.33 ms 预算；它关闭了 completed-frame timing，
不能代替完整 PlayerLoop、显示 FPS 或 U9 Windows Player 60 秒验收。

fresh 验证：runtime/editor 串行构建均为 0 error；PreInteraction 聚焦 job
`15a98b8e88f34f06a456e50c60933533` 为 `14/14 PASS`，包含中性命中、occupancy change 失效与非中性
fail-closed；压力工具整类 job `a748ed067f8c4365911ff3b623808da9` 为 `247/247 PASS`；
`BattleRuntimeSelfCheck` 于 `2026-08-15 01:52:12 PASS`。该切片按行为等价和目标子段稳定正收益保留，但不关闭
U6/U9，S0、T8 默认 `stage.dat` 与 Android 真机边界不变。

## 53. 普通站立无动作 release 窄快路径（第六十一候选，2026-08-15，已否决并撤回）

### 53.1 候选边界

本候选只针对运行时类型精确为 `LF2Character`、当前 state 为 Standing/Walking、没有重武器，且
Attack/Jump/Defend 均未达到 action-ready 条件的 `ProcessReleaseInput`。候选直接执行原 resolver 最终会调用的
`ApplyWalkRunFrameInternal(false)`，并保留 action-ready、派生类型、缺失 frame/PS/runtime 的原 resolver fallback；
另加默认关闭的 Legacy A/B 开关、命中计数和零分配聚焦测试。候选没有改变权威输入 pass 顺序、按键消费、组合技、
RNG、slot 顺序或生命周期。

### 53.2 同种子交叉复测与否决结论

相同 seed `1314149188`、1000 个真实生产 AI、30 warmup + 180 sample、相同 workload 的正反顺序复测结果：

| 路径 | logic average 两轮 | 两轮均值 | logic P95 两轮 | `ReleaseResolve` average 两轮 |
|---|---:|---:|---:|---:|
| candidate | `19.1095 / 18.8967 ms` | `19.0031 ms` | `23.2713 / 23.3707 ms` | `0.4177 / 0.4153 ms` |
| Legacy | `18.9367 / 19.0025 ms` | `18.9696 ms` | `23.2531 / 23.1004 ms` | `0.3823 / 0.3836 ms` |

候选整 tick 两轮均值回退约 `0.18%`，属于没有稳定收益的噪声区间；但目标 `ReleaseResolve` 子段两轮都稳定回退，
约慢 `8.7%～9.2%`。候选每轮命中 `12,760` 次，说明失败不是因为快路径未执行。四轮最终 parity overall 均为
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，正式 tick 均为 `0 B`，且 teardown
完整恢复，故结论是“行为等价但性能为负”，不是逻辑错误。

复测复用了下列两个输出路径，因此当前磁盘 JSON 保留反向复测的第二轮；第一轮完成时的指标已记录在上表：

- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice61-standing-no-action-candidate.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice61-standing-no-action-legacy.json`。

候选运行时代码、诊断接线、菜单和专用测试已用逆向最小补丁全部撤回；`rg` 确认没有
`ExactStandingNoActionRelease` 残留。撤回后 Unity 强制刷新编译为 `0 C# error`，输入聚焦 EditMode job
`e1e3c514c78145d2884ee448784eefcb` 为 `32/32 PASS`。不得再次以“跳过 resolver 的几个条件判断”作为优化方向；
后续只选择能删除完整数据产品、对象 shell 循环或跨 pass 重复遍历的候选。该负实验不关闭 U6/U9，S0、T8 默认
`stage.dat` 与 Android 真机边界不变。

## 54. 强制 role-aware sweep 诊断（第六十二候选，2026-08-15，已否决）

### 54.1 同种子对照

为确认 `CandidateCollect` 的 nested-direct / X-sweep 自动切换是否仍是主要尾延迟来源，本轮没有修改正式碰撞规则，
只使用现有诊断开关把全部 210 个采样 tick 强制为 role-aware X-sweep。对照使用相同 seed `1314149188`、
1000 个真实生产 AI、30 warmup + 180 sample，候选数、pair 数、最终 parity/lockstep hash、正式 tick 分配与
teardown 均保持一致。

| 模式 | logic average/P95/P99/max | `CandidateCollect` average/P95/P99/max | `DirectBroadphase` average | nested/sweep tick | broadphase comparisons |
|---|---:|---:|---:|---:|---:|
| 当前自动切换 | `18.9262/23.1735/26.6142/30.9398 ms` | `3.5569/7.3198/10.7432/11.8208 ms` | `0.8128 ms` | `118/92` | `1,049,994` |
| 强制 sweep | `19.6910/22.9337/24.5903/26.2139 ms` | `4.2603/7.2598/9.5584/10.9852 ms` | `1.5011 ms` | `0/210` | `664,526` |

两轮 pair 总数均为 `504071`，collision candidate 总数均为 `9588`，正式窗口均为 `0 B`、Gen0/1/2
collection 为 0。报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-capacity-pressure-smoke.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-force-role-aware-sweep.json`。

### 54.2 结论

强制 sweep 虽把比较次数降低约 `36.7%`，并略微降低 P99/max，但 `DirectBroadphase` 平均耗时增加约
`84.7%`，`CandidateCollect` 平均增加约 `19.8%`，完整逻辑 tick 平均增加约 `4.0%`。这说明比较次数不是
唯一成本：小规模活跃区间继续使用 nested-direct 比无条件构建/消费 sweep 产品更便宜。正式自动交叉阈值保持不变，
不保留任何候选代码。该实验只关闭“1000 AI 一律强制 sweep”的方向，不关闭 U6/U9；下一候选转向能够删除
完整空扫描的 generation-owned 派生索引。

## 55. 正向 Link generation-owned 派生索引（第六十三切片，2026-08-15，已保留）

### 55.1 权威合同与所有权边界

权威 C# `BattleCore/Simulation/GameTick.cs` 的 `ValidatePositiveLinks` 只处理 `LinkState > 0` 的 holder，并继续按
runtime slot 升序验证 target 是否活动、target 的反向 holder 是否仍指向当前 holder；无效时只清理当前 holder 的
`LinkState`、`TargetSlotIndex` 与 `HeldWeaponStableId`，不反向覆盖 target。Unity 旧 Legacy 实现虽然行为正确，
但即使本 tick 没有任何正向 Link，也会遍历全部 1050 个逻辑 slot。

本切片没有新增第二个 Link 真值。`NTSDEntityRuntime.LinkState` 仍是写入口，并继续即时发布到 generation-owned
`BattleRelationLinkStore`；该 store 在 bind/capture、LinkState 写入、release、reset 与 grow 时同步维护
`positiveLinkWords` 位图和数量。正式 data-oriented pass 只按位图中的 slot 升序消费，并在消费前再次校验
`RuntimeEntityHandle.Generation` 与当前 slot view；release 会先清除旧 generation 的位，slot 复用不会继承旧 Link。
当前位在校验中被清除时，迭代从 `slot + 1` 继续，因此不会跳过后续 holder。结构化 parity event、字段清理顺序与
Legacy A/B 入口均保留；没有每 tick new、字典或 LINQ。

### 55.2 同种子 A/B 与默认晋升

相同 seed `1314149188`、1000 个真实生产 AI、30 warmup + 180 sample、相同 workload 的对照：

| 模式 | logic average/P95/P99/max | `HeldLinkValidation` average/P95/P99/max | 正式 tick 分配 |
|---|---:|---:|---:|
| 派生索引 | `18.8785/22.9447/24.3108/26.5871 ms` | `0.001245/0.001700/0.002021/0.002200 ms` | `0 B` |
| Legacy 全 slot 扫描 | `19.3634/25.2833/30.7282/60.6926 ms` | `0.093405/0.096210/0.112892/0.135600 ms` | `0 B` |

目标 pass 平均耗时降低约 `98.7%`，完整 tick 平均降低约 `2.5%`。两轮 parity overall 均为
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，lockstep overall 均为
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`，Gen0/1/2 collection 均为 0，
teardown 与模式恢复均成功。报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.u6-positive-link-index-candidate.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-positive-link-index-legacy.json`。

完成 A/B 后，`BattleEcsPositiveLinkValidationPass` 的生产默认已晋升为 `DataOriented`；压力请求的未指定默认也同步
改为 data-oriented，但专用 Legacy 菜单仍会显式覆盖，以便后续回归。默认接线 fresh 报告
`Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-capacity-pressure-smoke.json` 实际解析为
`data-oriented`，1000 个实体、210 个执行 tick、30 warmup + 180 sample，logic average/P95/P99/max 为
`18.9096/23.1730/26.0301/29.7562 ms`，`HeldLinkValidation` average/P95 为
`0.001211/0.001600 ms`；零 GC 门与容量压力门均通过，最终两个 hash 与 A/B 完全一致。

聚焦 Link job `3426b75b123e4c40a840c29113e94e0c` 为 `8/8 PASS`，覆盖默认模式、same-tick 写入、
slot 顺序、generation release、结构事件与 warmed 1000-slot `0 B`；压力配置 job
`1f0f69bbaa1c48a6936650e9d1aa3191` 为 `5/5 PASS`，覆盖“未指定继承 data-oriented、明确 legacy 仍可 A/B”。
`BattleRuntimeSelfCheck` 于 `2026-08-15 03:00:21 PASS`。Unity 强制脚本刷新完成并能发现/执行上述测试；MCP stdio
客户端在域重载后仍会向 Console 写入一条自身的 disposed-object 连接错误，它不是 C# 编译诊断，也不作为战斗验收
通过项。

该切片删除了无正向 Link 时的完整逻辑 slot 空扫描，按目标 pass 的可归因显著收益保留；它不关闭 U6/U9，下一批
仍需从 fresh 热点中选择能删除完整数据产品、对象 shell 循环或跨 pass 重复遍历的候选。S0、T8 默认
`stage.dat` 与 Android 真机边界不变。

## 56. 无状态 CharacterMechanics world-owned 服务（第六十四切片，2026-08-15，已保留）

### 56.1 问题与所有权收敛

权威 C# `BattleCore/Frame/FrameAdvance.cs` 与 `BattleCore/Frame/Physics.cs` 把帧推进、角色动力学视为战斗
kernel 的确定性规则，不要求每个角色拥有一份有状态策略对象。Unity 旧实现却为每个 exact `LF2Character`
同时构造两份无状态 `CharacterMechanics`：一份来自 `LF2Entity` 的共享 character-DAT 兼容路径，另一份来自
`LF2Character` 自身字段。1000 个角色因此会创建约 2000 个语义相同、没有实体状态的托管对象；这不是稳态
tick GC，但与 U6 的 world-owned 执行所有权目标相冲突，也放大战斗进入时的对象图。

本切片令 `SimulationWorld` 持有唯一 `CharacterMechanics` 服务，已注册实体通过
`ResolveCharacterMechanics()` 解析该 world-owned 实例。未注册的测试壳、编辑器夹具和兼容对象仍可工作，但只在
首次真正调用动力学时惰性创建自己的 fallback；因此构造未注册对象本身不再分配 mechanics。没有引入静态可变
状态、每 tick `new`、字典或 LINQ，动力学规则、字段写入顺序、runtime slot 顺序与未知派生类型兼容边界均未改变。

### 56.2 验证与结论

新增聚焦测试证明：同一 world 中两个已注册角色解析到同一个 world-owned mechanics；未注册实体在首次解析前
fallback 为空，首次解析后稳定复用。fresh Unity 聚焦 job `4005ad30540c40f090fe0153fef3e944` 为
`11/11 PASS`；强制脚本刷新未发现 C# 编译错误；`BattleRuntimeSelfCheck` 于
`2026-08-15 03:30:55 PASS`。

最终无细分 timing 的 1000 AI 报告
`Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-performance-smoke.json` 为 1000 个真实生产实体、
30 warmup + 180 sample：logic average/P95/P99/max 为
`16.8144/21.2431/22.5278/23.9943 ms`，Unity 可见帧 average/P95 为
`24.4673/34.9563 ms`。driver、PlayerLoop envelope 与 presentation 的战斗窗口分配均为 `0 B`，Gen0/1/2
collection 均为 0；parity overall 为
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，lockstep overall 为
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`，teardown 完整恢复。

相邻旧实现回归为 logic average/P95 `16.8816/21.0864 ms`；当前 average 略低、P95 略高，均属于短样本噪声，
因此本切片只按减少对象所有权、启动对象数和保持行为等价保留，不声明稳态 FPS 提升。它也没有把完整
FrameAdvance 从对象 shell 迁出，故不关闭 U6/U9；下一批继续迁移 FrameAdvance 的正式 exact-character 执行
所有权，而不是重复已否决的字段读取微优化。S0、T8 默认 `stage.dat` 与 Android 真机边界不变。

## 57. exact-character FrameAdvance world-owned 编排（第六十五切片，2026-08-15，已保留）

### 57.1 权威顺序与兼容边界

权威 C# `BattleCore/Simulation/GameTick.cs` 在每个活动 slot 上先清理当前 action/directional keys，再进入
`FrameAdvance.Advance`；角色分支依次执行 throw-frame/delay 门、Link/Cpoint 门、角色动力学、state 12 空中帧
提升、燃烧空中帧 205 提升与 state 12 外 weapon-count 尾处理。旧 Unity 正式循环先对所有实体做
`entity.SimTransit()` 虚调用，再由 exact `LF2Character` 在对象方法中重新分派这条完整链。

本切片新增普通主类 `BattleEcsCharacterFrameAdvancePass`，没有新增 partial。正式 `SerialTickAll` 的 slot 顺序、
当前键清理位置、实体活动性复查、后续 `SimTU`、runtime snapshot 与 state 9998 清理边界均不变；只有 exact
`LF2Character + Character DAT` 由 world-owned pass 直接按上述权威顺序编排。未知派生角色、非角色 DAT shell 与
显式 Legacy 模式继续调用原 `SimTransit`，作为 fail-closed 兼容路径和行为 oracle。最终保留版也不再经
`LF2Character.ApplyDynamics()` 的对象分支：pass 直接构造确定性 mechanics context、调用第六十四切片的
world-owned `CharacterMechanics`、同步边界消费、分派落地事件并提交整数位置。迁移没有复制第二份状态、改变 dt、
改变 RNG 或引入每 tick 容器。

### 57.2 生产证据

聚焦测试覆盖默认模式、DataOriented/Legacy 同状态对照、未知派生回退和 mechanics 所有权，job
`81971a6424ba4caaae9b3b4240fcafe4` 为 `14/14 PASS`；停止压力 Play Mode 后，FrameAdvance、EarlyFrameAdvance 与
CollisionSnapshot 扩大聚焦 job `d6760bd21bde466c95b49bd21e205e6a` 为 `20/20 PASS`。Unity 全量刷新后 C#
编译错误为 0；动力学编排进入 pass 后的最终聚焦 job `ed91f4e232c0428096b46af2da2b8f2d` 为 `14/14 PASS`，
`BattleRuntimeSelfCheck` 于 `2026-08-15 03:52:11 PASS`。

压力报告新增只读生产计数，不改变模式。fresh
`Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-performance-smoke.json` 为 1000 个真实生产 AI、
30 warmup + 180 sample；FrameAdvance 模式为 `DataOriented`，`210000` 次实体 transit 全部命中 exact-character
路径，兼容回退为 0。logic average/P95/P99/max 为
`16.8062/21.1247/22.5522/24.1429 ms`，Unity 可见帧 average/P95 为 `24.2851/33.9971 ms`。driver、
PlayerLoop envelope 与 presentation 战斗窗口均为 `0 B`，Gen0/1/2 collection 均为 0；parity/lockstep hash
仍分别为 `752b4907...b35` 与 `4378ba4c...7063`，teardown 完整恢复。

相邻第六十四切片生产回归 logic average/P95 为 `16.8144/21.2431 ms`；本轮差异不足以单独证明稳定 FPS
收益，因此只声明正式 exact-character 执行入口和顺序所有权已经迁到 world-owned pass，并由生产命中计数与完整
确定性 hash 证明没有落回旧虚调用链。角色字段与 Frame/Transition 仍保留对象兼容镜像，完整 lifecycle canonical
world 尚未闭合，故 U6/U9 仍未完成。下一批继续从 FrameAdvance 相邻生命周期写入点收口 canonical 状态，而不是
删除 Legacy oracle。S0、T8 默认 `stage.dat` 与 Android 真机边界不变。

## 58. exact-character 周期恢复 world-owned pass（第六十六切片，2026-08-15，已保留）

### 58.1 权威合同与实现边界

权威 C# `BattleCore/Simulation/GameTick.cs` 的 `RegeneratePreCollisionStats` 在碰撞前恢复段按活动 slot 升序处理
角色：先受 `StepWait` 门控制；逻辑 tick 为 12 的倍数时处理 HP 恢复、负 `WeaponCount` 伤势恢复以及
`HPBound/ComboCountVic` 副作用；逻辑 tick 为 3 的倍数时处理 PP 恢复，并保留 oid 51/52 的上限规则。旧 Unity
实现位于每个角色的 Late 对象方法中，即使当前 tick 不满足 3/12 周期，也会进入对象分支后再判断为空操作。

本切片新增普通主类 `BattleEcsCharacterRecoveryPass`，没有新增 partial。`SimulationWorld` 在 Late recovery 原位置
持有并调用该 pass；exact `LF2Character + Character DAT + Health` 直接按上述权威顺序执行，非周期 tick 由 pass
证明为 no-op，未知派生类型、非角色 DAT、缺失 Health 与显式 Legacy 诊断仍 fail-closed 回到原虚调用。HP、PP、
`WeaponCount`、`HPBound` 与 `ComboCountVic` 继续写入原 canonical/runtime writer，不复制第二份真值；slot 顺序、
StepWait、FrameTick、death/opoint 与后续 cleanup 的可见边界未改变，也没有静态可变状态、每 tick `new`、LINQ 或
临时容器。

### 58.2 fresh 验证与性能结论

聚焦测试覆盖默认 DataOriented、与 Legacy 的周期写入等价、非周期 no-op 证明和未知派生 fallback；最终 Unity job
`b8d9d1bd67144ce3a44df5a37e08b33c` 为 `18/18 PASS`。首次测试请求因压力 Play Mode 尚未停止而执行 0 项，停止
Play Mode 后重新运行通过，不计为代码失败。fresh Unity 脚本刷新为 0 C# error，`BattleRuntimeSelfCheck` 于
`2026-08-15 04:11:42 PASS`。

关闭 phase、detail 与 presentation 高频诊断后的生产报告
`Temp/NTSD_ProductionEntityStress.combat1000.u6-slice66-character-recovery-final-20260815.json` 使用 seed
`1314149188`、1000 个真实生产 AI、30 warmup + 180 sample。`characterRecoveryMode` 为 `DataOriented`；210 个
执行 tick 共调用 pass `210000` 次，全部命中 exact-character 路径，其中 `140000` 次由周期门证明为 no-op，
compatibility fallback 为 `0`。logic average/P95/P99/max 为
`16.6533/20.6773/22.5392/23.6292 ms`，Unity frame average/P95 为 `23.4923/33.6469 ms`。逻辑 tick、driver、
presentation 与 PlayerLoop envelope 均为 `0 B`，Gen0/1/2 collection 均为 0；parity overall 为
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，lockstep overall 为
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`，teardown 完整恢复且 cleanup exception 为 0。

相邻第六十五切片无探针回归为 logic average/P95 `16.8062/21.1247 ms`。本轮没有性能回退，但差异仍不足以单独
证明稳定 FPS 收益，因此按“权威恢复职责进入 world-owned pass、14 万次确定性空对象分支被明确证明、行为与内存
边界保持一致”保留，不把它描述为 1000 AI 的关键性能突破。完整 lifecycle canonical world、对象兼容镜像退出与
U9 Windows Player 60 秒验收尚未完成，故 U6/U9 仍保持未关闭；S0、T8 默认 `stage.dat` 与 Android 真机边界不变。

## 59. exact-character FrameTick world-owned 编排（第六十七切片，2026-08-15，已保留）

### 59.1 权威顺序与兼容边界

权威 C# `BattleCore/Frame/FrameTick.cs` 与 `BattleCore/Simulation/GameTick.cs` 在 Late 实体段按活动 slot 升序执行
FrameTick：依次处理 throw-frame/delay 门、Link/Cpoint 门、frame counter 与 wait counter、state 0/14、next/wait
转移、caught-exit hit stop、frame 212 跳跃初始化、PP 显示、defend lock、frame 202 hit stop 及最终 wait-counter
同步。旧 Unity 路径由每个实体的 `SimFrameTick` 虚调用进入对象 shell，再由 exact `LF2Character` 执行上述完整链。

本切片新增普通主类 `BattleEcsCharacterFrameTickPass`，没有新增 partial。正式 Late FrameTick 原位置仍保持不变；
exact `LF2Character + Character DAT` 由 world-owned pass 按原顺序直接编排，未知派生类型、非角色 DAT shell 与
显式 Legacy 模式 fail-closed 回原 `SimFrameTick`。现有 frame/counter/runtime writer 仍是唯一真值，pass 只通过
窄 internal 边界复用 caught-exit、frame 212 与 PP-display 规则；没有静态可变会话状态、每 tick `new`、LINQ 或
临时容器。压力工具新增 `characterFrameTickMode=legacy|data-oriented`，只允许在 reset/合法 teardown 边界切换并
恢复，便于在同一代码和同一 seed 下做真实 A/B，不改变生产默认模式。

### 59.2 验证、A/B 与保留结论

聚焦测试覆盖默认模式、DataOriented/Legacy 状态等价、未知派生 fallback、32 个 warmed 角色零分配，以及压力请求
模式解析；最终 Unity job `7355546bb6dd472290a3227e1bc4eeee` 为 `23/23 PASS`。fresh 脚本刷新未发现 C# 编译
错误，`BattleRuntimeSelfCheck` 于 `2026-08-15 04:46:40 PASS`。

详细探针报告
`Temp/NTSD_ProductionEntityStress.combat1000.u6-slice67-frame-tick-detail-candidate-20260815.json` 使用 seed
`1314149188`、1000 个真实生产 AI、30 warmup + 180 sample。相对第六十六切片同口径详细基线，目标
`LateEntityUpdate/FrameTick` average 从 `0.6876 ms` 降到 `0.5667 ms`，约下降 `17.6%`；整个
`LateEntityUpdate` average 从 `2.8358 ms` 降到 `2.4709 ms`。pass 在 210 个执行 tick 中运行 `210000` 次，
全部命中 exact-character，fallback 为 0。

关闭 phase/detail/presentation 高频探针后的交错 A/B 报告为：

- Legacy A：`16.7722/21.1493/22.4482/23.8697 ms`；
- DataOriented B：`16.6170/21.2364/23.3555/24.7433 ms`；
- Legacy C：`16.9007/21.2622/23.4269/24.3126 ms`；
- DataOriented D：`16.6273/21.0954/23.1746/23.8326 ms`。

四项依次为 logic average/P95/P99/max。两轮均值中，DataOriented average 为 `16.6222 ms`，Legacy 为
`16.8364 ms`，改善约 `0.2143 ms`（`1.27%`）；P95 基本持平（`21.1659` 对 `21.2057 ms`），P99 未改善，
因此不宣称整体尾延迟或可见 FPS 突破。四份报告的正式 tick 均为 `0 B`、Gen0/1/2 collection 为 0，parity overall
均为 `752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，lockstep overall 均为
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`，teardown、模式恢复和 cleanup 均通过。

报告路径：

- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice67-frame-tick-ab-legacy-a-20260815.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice67-frame-tick-ab-data-b-20260815.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice67-frame-tick-ab-legacy-c-20260815.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice67-frame-tick-ab-data-d-20260815.json`。

本切片按“目标 pass 可归因下降、exact-character 执行所有权进入 world-owned kernel、行为/内存/恢复边界等价”保留，
但收益规模不足以关闭 U6/U9。角色对象兼容镜像、剩余 lifecycle canonical world 与 U9 Windows Player 60 秒矩阵
仍未完成；S0、T8 默认 `stage.dat` 与 Android 真机边界不变。

## 60. exact-character PostFrameTail world-owned 候选（第六十八切片，2026-08-15，性能门禁未过，默认 Legacy）

### 60.1 权威顺序与候选边界

权威 C# `BattleCore/Simulation/GameTick.cs` 的实体 post-frame tail 在原 slot 升序位置依次处理 `HealTimer`、
`CatchTimer`、state 1700 的 `HealTimer=1100`，最后清理 `HitConfirm2` 与四个瞬态 MP carrier。Unity 旧路径对每个
实体调用对象 shell。候选普通主类 `BattleEcsCharacterPostFrameTailPass` 只对 exact
`LF2Character + Character DAT + runtime` 直接执行相同写入；未知派生类型、非角色 DAT、空 runtime 与显式
Legacy 均 fail-closed 回原路径。候选没有新增 partial、静态可变会话状态、每 tick 容器、LINQ 或 managed 分配。

压力工具新增 `characterPostFrameTailMode=legacy|data-oriented`，只在合法 reset/teardown 边界切换，报告记录
requested/effective/restored 以及 run/exact/fallback 计数。由于本节性能门禁未通过，`SimulationWorld`、压力请求
默认值与空字符串解析均保持 `Legacy`；DataOriented 仅保留为显式诊断候选和后续结构实验基线，不进入生产默认。

### 60.2 ABAB、定向 timing 与否决结论

相同 seed `1314149188`、1000 个真实生产 AI、30 warmup + 180 sample、关闭 phase/detail/presentation 高频探针的
交错 ABAB 为：

- Legacy A：`16.7977/20.9585/22.6501/24.3906 ms`；
- DataOriented B：`16.9406/21.2657/23.3624/25.5175 ms`；
- Legacy C：`16.8815/21.0541/23.0719/23.9659 ms`；
- DataOriented D：`16.7148/20.7706/23.3465/24.3169 ms`。

四项依次为 logic average/P95/P99/max。两轮均值中，Legacy/DataOriented average 为
`16.8396/16.8277 ms`，Data 只改善 `0.0120 ms`（约 `0.07%`）；P95 为 `21.0063/21.0182 ms`，基本持平；
P99 为 `22.8610/23.3544 ms`，Data 没有改善。该结果不足以证明整体性能收益。

仅开启 pass timing 的独立定向对照进一步显示：Legacy 的 `EntityPostFrameTail` average/P95/P99/max 为
`0.183471/0.191405/0.246081/0.262600 ms`；DataOriented 为
`0.198651/0.206930/0.227568/0.289000 ms`。候选目标阶段 average 反而增加约 `0.01518 ms`（`8.3%`），P95 与
max 也更高；两份完整 tick average 几乎相同（`16.80835/16.80889 ms`）。因此本候选未晋升，生产默认明确恢复为
Legacy；不能以“所有权迁移”掩盖负向目标阶段数据。

六份报告的 parity overall 均为
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，lockstep overall 均为
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`；正式 tick `0 B`、Gen0/1/2 collection
均为 0，teardown 与模式恢复通过。报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice68-post-tail-ab-legacy-a-20260815.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice68-post-tail-ab-data-b-20260815.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice68-post-tail-ab-legacy-c-20260815.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice68-post-tail-ab-data-d-20260815.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice68-post-tail-detail-legacy-20260815.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice68-post-tail-detail-data-20260815.json`。

fresh Unity 刷新完成且未发现 C# 编译错误；候选/解析聚焦 job `6bc6d69649f742c09737e19eb3c23ae5` 为
`5/5 PASS`，FrameAdvance/Recovery/FrameTick/PostFrameTail 扩大聚焦 job
`96b264a08c4045f79282442f4483535b` 为 `27/27 PASS`；`BattleRuntimeSelfCheck` 于
`2026-08-15 05:19:44 PASS`。本节关闭的是该候选的生产晋升，不关闭 U6/U9；下一批必须从 fresh 热点选择能删除
完整数据产品、对象 shell 循环或跨 pass 重复遍历的候选，不能继续对亚毫秒尾部做无收益搬运。S0、T8 默认
`stage.dat` 与 Android 真机边界不变。

## 61. AI canonical owned-input 重复快照候选（第六十九切片，2026-08-15，性能门禁未过，默认 SnapshotCopy）

### 61.1 重复数据产品与等价候选

fresh 热点报告显示 `CharacterInput/EntityInputPass` 仍是最大阶段，其中 IndexedCanonical 链会先把
`BattleCharacterInputStore` 已持有的 canonical `AiDecisionInputState` 复制到 `AiDecisionSnapshot.Input`，随后
`AiDecisionKernel` 又将该值复制为本地可变输入，计算完成后通过 writer 事务提交。第六十九切片只测试删除第一份
`store -> snapshot.Input` 复制：`CanonicalStoreDirect` 直接以 `in` 参数读取 generation-owned store 行，kernel
仍创建局部值副本并维持原有 slot 升序、RNG 调用顺序、校验、提交和失败边界。低频 FullScan oracle 在采样时仍显式
捕获完整输入，因此诊断覆盖没有被候选短路。

候选新增 `AiDecisionOwnedInputMode`、store/writer 的 canonical evaluation 窄接口以及 kernel 的 owned-input 重载；
没有新增 partial、静态可变会话状态、LINQ、每 tick 容器或 managed 分配。模式只能在无活动实体的 reset 边界切换。
默认保持 `SnapshotCopy`；`CanonicalStoreDirect` 仅作为显式诊断候选。

### 61.2 ABAB、确定性与否决结论

相同 seed `1314149188`、1000 个真实生产 AI、30 warmup + 180 sample、`maxCatchUpTicksPerFrame=1`、关闭
phase/detail/presentation 高频探针的交错 ABAB 为：

- SnapshotCopy A：`16.8745/21.5046/24.1780/25.5089 ms`；
- CanonicalStoreDirect B：`16.7608/21.2717/24.4111/26.2622 ms`；
- SnapshotCopy C：`16.8996/20.8996/22.4107/27.7141 ms`；
- CanonicalStoreDirect D：`16.9907/21.2251/23.3004/24.2757 ms`。

四项依次为 logic average/P95/P99/max。两轮均值中，SnapshotCopy/Direct average 为
`16.8871/16.8757 ms`，Direct 只改善约 `0.0113 ms`（`0.07%`）；P95 为 `21.2021/21.2484 ms`，Direct 略差；
P99 为 `23.2943/23.8558 ms`，Direct 同样没有改善。候选删除的数据产品本身仅是一个结构体复制，ABAB 证明它不是
1000 AI 的有效性能杠杆，因此未晋升，生产默认恢复为 `SnapshotCopy`。

四份报告均为正式 tick `0 B`、Gen0/1/2 collection 0、IndexedCanonical fallback 0；parity overall 均为
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，lockstep overall 均为
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`。报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice69-owned-input-ab-copy-a-20260815.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice69-owned-input-ab-direct-b-20260815.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice69-owned-input-ab-copy-c-20260815.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice69-owned-input-ab-direct-d-20260815.json`。

最终默认恢复后 Unity 脚本刷新成功；AI kernel 与 unified snapshot 聚焦 job
`7fdc4bb85d1f4be28bc5bf66f0e4834a` 为 `74/74 PASS`，覆盖跨 tick 等价、失败边界、oracle、1000 行线性访问和 warmed
零分配；`BattleRuntimeSelfCheck` 于 `2026-08-15 05:48:17 PASS`。该切片只关闭 owned-input 复制候选，不关闭
U6/U9。下一批继续审计 CandidateCollect、LateEntityUpdate 与 RenderDispatch 的完整数据产品和跨 pass 重复遍历，
只接受可归因的整循环删除或数据结构收益；S0、T8 默认 `stage.dat` 与 Android 真机边界不变。

## 62. unified AI row 无 pending 空刷新跳过（第七十切片，2026-08-15，已保留）

### 62.1 重复边界与安全条件

正式 `CharacterInput` 在每个 active character 之后调用
`RefreshAiUnifiedSnapshotExecutionRowAfterCharacterInput`。第六十九切片后的 fresh 计数显示统一快照执行行每轮仍刷新
`209000` 次，而 `aiProjectionPublicationCount/SkipCount` 表明绝大多数输入投影并没有变化。进一步沿
`BattleAiUnifiedRowPublisher.TryCommitPending`、move-mode first-ten witness 和 row refresh 调用链核对后，确认以下组合
原本只会执行一组无状态变化的空检查：当前 generation 没有 pending mask、slot 不属于 first-ten move-mode 窗口、
未强制 full refresh，且没有开启逐行增量 oracle。

第七十切片为 `BattleAiUnifiedRowPublisher` 增加 generation-safe 的 `HasPendingValues` 窄查询，并在上述条件全部成立时
跳过空刷新；有 pending、first-ten、强制 full refresh、测试 mutation override 或显式增量验证均继续走原路径。
该路径不改变 active-slot 顺序、AI 输入、RNG、role/team 索引、事务提交、发布快照或下一实体可见边界，也没有新增
每 tick 容器、LINQ、委托或 managed 分配。第一次将候选设为默认后，既有增量 oracle 测试把刷新验证次数从
`120` 观察为 `91`；这证明显式验证模式不能被生产空刷新快路绕过。最终保护条件已把
`ValidateIncrementalAiUnifiedRowForDiagnostics` 排除在快路之外，诊断契约恢复，同时生产路径仍可获益。

### 62.2 交错 A/B、零分配与保留结论

相同 seed `1314149188`、1000 个真实生产 AI、30 warmup + 180 sample、单 Unity 帧最多 1 个逻辑 tick、关闭
phase/detail/presentation 高频探针的 ABAB 结果依次为 logic average/P95/P99/max：

- Legacy A：`17.1461/22.0969/24.3890/25.2847 ms`；
- Candidate B：`16.8566/21.4999/23.3373/24.7638 ms`；
- Legacy C：`16.8697/21.4320/23.2302/24.5539 ms`；
- Candidate D：`16.7495/21.1880/23.4057/24.2661 ms`。

两轮均值中 Legacy 为 `17.0079 ms`，Candidate 为 `16.8031 ms`，改善 `0.2048 ms`（约 `1.20%`）。候选两轮
都不慢于相邻旧路径，P95 均值下降，P99 没有形成可重复回退；收益规模也与 fresh 热点中
`UnifiedSnapshotExecutionRowRefresh` 约 `0.204 ms` 的理论上限一致。因此该快路保留并成为正式默认，而原完整刷新仍由
属性开关、强制 full refresh 和 oracle 模式保留为诊断对照。

四份报告均为正式 tick `0 B`、Gen0/1/2 collection 0，parity overall 均为
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，lockstep overall 均为
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`：

- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice70-no-pending-refresh-ab-legacy-a-20260815.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice70-no-pending-refresh-ab-candidate-b-20260815.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice70-no-pending-refresh-ab-legacy-c-20260815.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice70-no-pending-refresh-ab-candidate-d-20260815.json`。

最终默认配置下 Unity fresh 脚本刷新成功；聚焦 job `63ace2f715ca4dd2b608d9d4d7cbf57a` 为 `94/94 PASS`，
`BattleRuntimeSelfCheck` 于 `2026-08-15 06:12:15 PASS`，`git diff --check` 无 whitespace error。该切片只关闭
统一 AI 行的无 pending 空刷新候选，不关闭 U6/U9；完整 lifecycle canonical world、剩余多毫秒热点和 U9 Windows Player
60 秒矩阵仍未完成。S0、T8 默认 `stage.dat` 与 Android 真机边界不变。

## 63. exact-character 输入对象壳候选（第七十一切片，2026-08-15，双门禁否决）

### 63.1 权威调用链与候选边界

权威 C# `BattleCore/Simulation/GameTick.cs::ApplyCharacterInputPass` 按 runtime slot 升序调用
`CharacterLogic.ApplyInput`；后者的顺序是 runtime 同步、AI `AiInputRuntime.PrepareBasic` 或人类输入轮询、
`InputRuntime.ApplyCharacterInput`、再同步兼容字段。Unity 原路径在相同 pass 位置通过
`LF2Entity.RunCharacterInputPhaseForKnownCharacterDat` 进入对象虚调用。第七十一切片增加普通主类
`BattleEcsCharacterInputPass`，尝试对 exact `LF2Character + Character DAT` 直接编排 AI preparation、
`CharacterInputActionResolver.ApplyFrameInputFromRuntimeProgress` 与 frame velocity tail；未知派生、非角色 DAT 和
显式 Legacy 均 fail-closed 回原对象路径。没有新增 partial、静态可变状态、每 tick 容器或 managed 分配。

该候选还补齐了压力工具的 `Process Pending Request` Editor 入口，使已经由自动化写入的请求文件可以被显式接管，
不再依赖脚本域重载来翻转静态 `requestPending`。该工具修复只改变压测启动方式，不改变战斗 tick 或测试负载。

### 63.2 ABAB、确定性与否决结论

相同 seed `1314149188`、1000 个真实生产 AI、30 warmup + 180 sample、单帧最多 1 tick、关闭高频 timing 探针的
ABAB 结果依次为 logic average/P95/P99/max：

- Legacy A：`17.3493/21.5737/25.2554/29.2532 ms`；
- DataOriented B：`17.3995/23.0064/24.8935/27.0690 ms`；
- Legacy C：`16.7791/21.5129/24.2096/26.7345 ms`；
- DataOriented D：`16.9115/21.3293/22.7113/24.1692 ms`。

Legacy/DataOriented 两轮 average 均值为 `17.0642/17.1555 ms`，候选慢 `0.0913 ms`（约 `0.54%`）；P95
均值同样由 `21.5433 ms` 回退到 `22.1679 ms`。四轮正式 tick 均为 `0 B`、Gen0/1/2 collection 0，parity
overall 均为 `752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，lockstep overall 均为
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`，teardown 完整恢复：

- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice71-character-input-pass-ab-legacy-a-20260815.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice71-character-input-pass-ab-candidate-b-20260815.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice71-character-input-pass-ab-legacy-c-20260815.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice71-character-input-pass-ab-candidate-d-20260815.json`。

除性能门未过外，定向的 frame-jump/input-tail 合成用例还观察到候选与 Legacy 的状态差异（期望 `0`，候选为
`1`）。这说明 1000 AI 压力 checksum 没有覆盖该分支，不能把压力等价扩大为完整行为等价。因此生产默认明确恢复
`Legacy`，DataOriented 仅保留为不可默认启用的诊断实验；该切片同时被性能门和完整 parity 门否决，不计入 U6
性能收益。U6/U9 仍未关闭；下一候选继续从 CandidateCollect、LateEntityUpdate 与 RenderDispatch 的完整循环或
跨 pass 重复数据产品中选择。S0、T8 默认 `stage.dat` 与 Android 真机边界不变。

最终生产默认恢复为 `Legacy` 后，聚焦 EditMode job `38e5316667d046ea8b636699f6b3fdf1` 为
`35/35 PASS`（0 failed、0 skipped）；`BattleRuntimeSelfCheck` 于 `2026-08-15 06:46:36 PASS`。
`git diff --check` 未发现 whitespace error。Unity Console 中本次 self-check 留下的两条
`SimulationWorld` Error 是注册回滚与错误 generation 释放的预期 fail-closed 夹具输出；另有三条 MCP client
disposed-object 日志，属于工具连接层，不是 C# 编译错误或战斗运行时断言失败。该证据只关闭第七十一候选的拒绝闭环，
仍不把它计为 U6 完成项。

## 64. role-aware 碰撞角色产品合并候选（第七十二切片，2026-08-15，性能门禁否决并完全撤回）

### 64.1 候选边界与保持不变的权威合同

fresh 1000 AI 细分计时把 `CandidateCollect` 定位为约 `3.5375 ms/tick`，其中 participant/body/itr build、
direct broadphase、pair exact loop、cache setup 与 sort/deduplicate 分别约为 `0.9526/0.8163/0.6418/0.2655/
0.1843 ms`。第七十二候选只尝试合并该 pass 内部的一个重复数据产品：把每个 participant 已经计算出的 body/itr
四个 role flag 暂存到 participant 结构，并用 warmed 固定数组维护 exact-required role 前缀，避免第二次遍历实体来
重建同一组 role byte。候选没有改变 active slot 顺序、authority ordinal pair 排序、双向 exact 调用、RNG 调用、
candidate cap、fallback、generation/epoch 校验、几何构建或碰撞规则，也没有引入每 tick `new`、LINQ 或容器扩容。

### 64.2 同口径压力复测与否决

基线与候选都使用 seed `1314149188`、1000 个真实生产 AI、30 warmup + 180 sample、单 Unity 帧最多 1 个逻辑
tick，并开启相同 phase/detail/presentation 计时：

- 基线 `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice72-current-detail-20260815.json`：logic
  average/P95/P99/max 为 `19.0017367/23.103635/25.646932/29.6824 ms`；`CandidateCollect` average 为
  `3.5374661 ms`；participant build 为 `0.9526328 ms`；
- 候选 `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice72-collision-role-products-candidate-20260815.json`：
  logic average/P95/P99/max 为 `19.0107956/23.194025/27.408365/30.6018 ms`；`CandidateCollect` average 为
  `3.5122444 ms`；participant build 为 `0.9912617 ms`。

候选只让整个 `CandidateCollect` average 下降 `0.0252217 ms`，但目标 participant build 反而增加
`0.0386289 ms`；完整 tick average 增加 `0.0090589 ms`，P95 增加 `0.09039 ms`，P99/max 也更差。该结果说明
第二份 role-byte 产品并不是当前 1000 AI 的有效杠杆，合并后结构体写入/局部性成本抵消了省下的遍历，未通过
“目标子段和整 tick 都不能回退”的性能门禁。

两份报告的正式 tick 均为 `0 B`，Gen0/1/2 collection 均为 0；parity overall 均为
`752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，lockstep overall 均为
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`，teardown 与模式恢复完整通过。
候选代码、字段和临时存储已全部撤回，不保留隐藏开关。撤回后 Unity fresh 脚本刷新成功且 Console 0 error；
碰撞聚焦 job `fd209373a3ab4231aeb30ae632159ae5` 为 `58/58 PASS`，完整 `BattleRuntimeSelfCheck` 于
`2026-08-15 07:09:06 PASS`。该切片只关闭此负向候选，不计入 U6 收益，也不关闭 U6/U9。下一批回到 canonical
frame/motion/lifecycle store 与正式 production reader 的联合迁移审计；只有能迁移真实读取者并删除对象壳数据产品的
切片才进入实现。S0、T8 默认 `stage.dat` 与 Android 真机边界不变。

## 65. FrameAdvance 栈上值状态运动内核候选（第七十三切片，2026-08-15，性能门禁否决并完全撤回）

### 65.1 候选目的与等价边界

第七十三候选针对 exact-character `FrameAdvance` 的真实生产读取者，而不是再次尝试把 runtime getter 机械改读
`BattleFrameMotionStore`。候选在进入角色动力学时一次性捕获 X/Y/Z、Vx/Vy/Vz、YInt 与四个方向边界标志，随后在
栈上值结构中按现有顺序完成边界阻挡、平面位移、边界消费、地面摩擦、垂直位移、落地判断与重力，最后一次性提交回
`NTSDEntityRuntime`。原 `DataOriented` 对象内核保留为同一 FrameAdvance 外壳中的直接 A/B oracle；slot 顺序、
delay/link/cpoint 门、落地回调、整数坐标同步、RNG、dt、Frame/Transition 尾部和表现边界都没有改变。

候选 Unity 编译没有出现 C# error；新增的对象内核/值状态内核等价检查与现有 FrameAdvance/FrameTick/Recovery、
未知派生 fallback、warmed 0-allocation 检查共 `23/23 PASS`。因此候选先通过了静态编译、聚焦行为与内存前置门，
随后才进入 1000 AI 生产压力测量。

### 65.2 1000 AI 数据、否决和撤回验证

对照基线为
`Temp/NTSD_ProductionEntityStress.combat1000.u6-slice72-current-detail-20260815.json`，候选为
`Temp/NTSD_ProductionEntityStress.combat1000.u6-slice73-frameadvance-valuestate-candidate-20260815.json`。两者均使用
seed `1314149188`、1000 个真实生产 AI、30 warmup + 180 sample、每 Unity 帧最多 1 个逻辑 tick，并开启相同
phase/detail/presentation 诊断：

- 对象内核基线 logic average/P95/P99/max 为
  `19.0017367/23.103635/25.646932/29.6824 ms`，`FrameAdvance` average/P95/max 为
  `1.0834317/1.14388/1.7030 ms`；
- 值状态候选 logic average/P95/P99/max 为
  `19.1108328/23.801165/27.763975/30.0620 ms`，`FrameAdvance` average/P95/max 为
  `1.0798278/1.18296/1.6663 ms`。

候选只让目标阶段 average 下降 `0.0036039 ms`，低于可归因收益；目标 P95 反而增加 `0.03908 ms`，完整 tick
average/P95 分别增加 `0.1090961/0.69753 ms`，P99 也明显更差。两轮正式 tick 均为 `0 B`，Gen0/1/2 collection
均为 0，lockstep overall 均为
`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`，teardown 完整恢复。结果说明这一段仅有
约 1000 次轻量引用字段访问，栈状态的捕获与回写抵消了理论收益，不能据此继续扩大复制字段或迁移更多无真实消费者的
frame/motion 字段。

候选结构、重载、枚举值、默认模式与测试已全部撤回，不保留隐藏开关；生产默认恢复原 `DataOriented` 对象内核。
撤回后 Unity fresh 脚本刷新未观察到 C# 编译错误，FrameAdvance 聚焦 job
`1c0c8e8844d14775901c637277653c8e` 为 `22/22 PASS`，完整 `BattleRuntimeSelfCheck` 于
`2026-08-15 07:24:57 PASS`。Console 中唯一 Error 为 MCP 域重载连接对象已释放，属于工具连接层，不是编译或
战斗断言错误。该切片不计入 U6 收益，不关闭 U6/U9；下一批停止在 FrameAdvance 做字段级搬运，重新从 fresh
CharacterInput、CandidateCollect、LateEntityUpdate 与 RenderDispatch 的整循环/跨 pass 数据产品选择候选。
S0、T8 默认 `stage.dat` 与 Android 真机边界不变。

## 66. 中央表现不可变发布边界审计（第七十四切片，2026-08-15，无代码候选）

### 66.1 调用链与数据所有权结论

本切片重新审计 `RenderDispatchAll -> BeginFrame -> CaptureEntities -> CaptureHitRecords -> SortEntities ->
BuildCommands -> Publish` 的完整生产调用链。`CaptureEntities` 并不是逻辑侧已经完成排序后又无意义复制一次的产品：
它在逻辑实体仍可继续变化的边界，将当前 runtime slot 顺序、generation、逻辑位置、frame、可见性、贴图来源与表现
必要字段捕获为不可变 `BattlePresentationEntitySnapshot`，供后续主线程/渲染宿主物化 command 与 mesh。表现宿主不能直接
持有并延迟读取可变 `LF2Entity`，否则未来专用 simulation worker、渲染帧落后或同帧生命周期复用都会把“发布时状态”
变成“读取时状态”。因此该捕获是逻辑真值与表现消费之间的所有权边界，不是可以仅凭循环名称删除的重复排序。

当前代码也没有覆盖全部表现字段写入点的统一 dirty/version 合同。若只对位置、frame 或可见性做增量更新，遗漏
generation、DAT/frame source、flip、shadow、holder/layer、销毁/复用等任一写入都会产生陈旧表现；若为所有写入补齐
dirty，又会先引入新的跨模块维护合同，不能在没有 A/B 与完整 parity 证明时晋升。`CaptureHitRecords` 的独立循环约
`0.08288 ms/tick`，同样不是值得用生命周期语义风险交换的主要杠杆。

### 66.2 实测归因与处理决定

使用第七十二切片同口径详细报告
`Temp/NTSD_ProductionEntityStress.combat1000.u6-slice72-current-detail-20260815.json`，中央表现各段 average 为：

- `RenderDispatch/PresentationPublishTotal`：`5.21253 ms`；
- `Materialize/Mesh/ResolveAndWriteCommands`：`2.92792 ms`；
- `BeginFrame/BuildCommands/Core`：`1.74715 ms`；
- `BeginFrame/CaptureEntities`：`0.93838 ms`，P95 `0.999145 ms`；
- `BeginFrame/CaptureHitRecords`：`0.08288 ms`；
- 表现排序：`0.09424 ms`。

数据表明剩余主要成本在 command 解析与 mesh 写入，不在 `CaptureEntities` 本身。删除约 `0.94 ms` 的不可变发布边界
既不能解释约 `19 ms` 的逻辑 tick，也会破坏后续 worker 架构所需的双缓冲/不可变 publication 前提。因此第七十四
切片只形成“保留现有边界”的审计结论，不新增 dirty 快照、直接实体引用或隐藏实验开关，也不进行 1000 AI 复测。
该结果不计入 U6 性能收益，不关闭 U6/U9；后续中央表现优化只能针对已实测的 command resolve/mesh 写入完整产品，
或在 U8 建立正式双缓冲 publication 合同时统一处理。S0、T8 默认 `stage.dat` 与 Android 真机边界不变。
