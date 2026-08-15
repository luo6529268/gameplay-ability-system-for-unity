# U6 CharacterInput 提交 writer 切片（2026-08-12）

> 对应总计划：`unified-battle-lockstep-ecs-server-architecture-plan.md`  
> 阶段：U6，CharacterInput canonical writer 闭合  
> 状态：IndexedCanonical AI 原子提交、共同输入状态 writer、组合动作 resolver 与 world-bound 动作事务入口已完成；整个 CharacterInput/frame/motion canonical 字段簇尚未完成。

## 1. 本切片边界

权威 C# 入口为：

- `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Input\InputRuntime.cs`；
- `PrepareAiInputBasic` 负责 AI 输入、输入历史、冷却、组合键计数、共享 AI flow 与 RNG 推进；
- `ApplyCharacterInput` 继续负责组合键消费、直接动作和速度写入。

Unity 的 `data-oriented-canonical` AI 决策此前已经由值类型 kernel 计算，但提交结果仍由
`SimulationWorld.AiDecisionShadow.partial.cs` 内的私有方法直接写回 runtime、world flow 和 RNG。
本切片把这组原子提交迁移到每个 `SimulationWorld` 独占的组合式
`BattleAiInputWriter`：

- 输入历史 0～5；
- AI 输入冷却；
- 九组组合键计数；
- previous/current key 状态；
- `Unk360/Unk3FC/Unk400`；
- AI 共享 flow；
- RNG state 与 call count。

值类型决策 kernel 保持纯计算；writer 是该决策结果发布到战斗世界的唯一组合边界。
没有新增 partial，也没有改变 slot 升序、RNG 消费点、同 tick 可见性或 pass 顺序。

第二个小切片新增组合式 `BattleCharacterInputWriter`，集中提交 AI 与 human 输入适配器共享的：

- previous/current key；
- attack/jump/defend/directional cooldown；
- defend lock；
- 九组 combo 计数。

`BattleAiInputWriter` 继续拥有 AI-only 输入历史、`Unk360/Unk3FC/Unk400`、共享 flow 与 RNG，
但共同字段委托给 `BattleCharacterInputWriter`。注册到 `SimulationWorld` 的
`NTSDInputStateModule` 也通过相同 writer 提交 full/progress transaction；未注册的独立测试或兼容
对象保留私有实例 fallback。该 fallback 不是第二份持久 canonical store，也不会参与生产 world 的写入。

第三个小切片继续把 world-bound 输入生命周期写入收口到 `BattleCharacterInputWriter`：

- AI roll previous + clear current keys；
- frame advance 前 current action/direction keys 清理；
- input edge 与 history push；
- battle-entry input reset；
- N30 history tail/gate；
- frame 110/114 的 defend lock。

已确认没有调用方的 `LF2Entity.ApplySharedRuntimeInputEvent` 与
`ForceSharedRuntimePreviousState` 私有死代码一并删除。未注册实体仍调用
`NTSDEntityRuntime` 兼容 helper；生产 writer 只使用 `registeredWorld`，不会把未注册对象误接到
`SimulationTickDriver.Instance.World`。

第四个小切片新增无实例状态的值类型 `BattleCharacterInputActionResolver`，把 human adapter 与
world-bound AI 先前各自维护的组合技和 direct-frame 算法合并为一份。正式 world 内的 AI
直接从 runtime 捕获仅包含 8 个 cooldown 与 9 个 combo 的
`BattleCharacterInputActionState`，解析后通过 `BattleCharacterInputWriter` 一次提交 progress；
不再执行 `Runtime -> NTSDInputStateModule -> Runtime` 的 progress 镜像。human 路径仍保留私有
held/previous 状态，但组合技算法也委托给同一 resolver。未注册测试对象与兼容 shell 保留原有
adapter fallback。

该切片保持权威 C# 的特殊事务语义：九组组合进度先在局部值中推进，只有 DJA 最终 fallthrough
才整体提交；提前触发技能时只保留帧跳、朝向和 cooldown 副作用。`ProcessReleaseInput` 的位置、
slot 顺序、同 tick row refresh 与 RNG 消费点均未改变。

第五个小切片新增 world-owned 组合式 `BattleCharacterActionWriter`，把组合/direct-frame 触发的
`TryCharacterDatInputFrameJump` 与后续完整 `ProcessReleaseInput` 统一收口到同一动作事务入口。
因此注册 world 内由输入触发的 frame、facing、HP/PP、`ComboCountVic`、直接动作选择和速度写入，
都先经过该 writer，再调用现有实体 adapter 的兼容实现；未注册的独立测试对象仍走兼容入口。
本切片没有复制状态、没有新增 partial，也没有改变动作 resolver、随机数、早退或 pass 顺序。

同时完成 Legacy/fail-closed 边界审计：生产默认
`DataOrientedCanonical + IndexedCanonical + UnifiedAuthority` 在 unified snapshot 发布后禁止
任何 Legacy fallback，尝试回退会触发 hard breach；只有 snapshot 发布前整批失败时，才允许
该 tick 完整使用 LegacySeparate pass。该旧路径仍是显式兼容 oracle，不会与已发布 canonical
结果发生半 tick 混写，但在最终删除旧 writer 前仍不视为 U6 完成。

## 2. 明确未关闭的边界

本切片不能扩大解释为整个输入簇已经 canonical：

- Legacy AI 决策仍作为 snapshot 发布前整批 fallback 与显式 A/B oracle 保留；发布后已 hard
  fail-closed，不允许混合回退，但旧 Legacy writer 尚未删除；
- human held/previous 状态仍由现有输入 adapter 保存；组合技/direct-frame 与 release action 已进入
  world-owned resolver/writer 事务入口，但 frame/facing/HP/PP/统计/速度的底层存储仍在实体
  adapter，尚未晋升持久 canonical SoA；
- `RefreshAiUnifiedSnapshotExecutionRowAfterCharacterInput` 必须保留，因为后序 slot 的 AI
  在同一个 tick 内必须观察到前序 slot 已完成输入与组合键后的状态；
- `LF2Character` / `NTSDEntityRuntime` 仍是兼容真值，尚未退化成 Unity shell；
- 因此 U6 与 U9 都没有因本切片关闭。

## 3. 新鲜验证

- Unity 脚本编译：0 个 C# error；
- AI kernel、SoA shadow、AI sensing candidate 与生产压力工具联合 EditMode：
  job `d974a1e780934d30b900800084e277d0`，`386/386 PASS`；
- `BattleRuntimeSelfCheck`：`2026-08-12 22:36:48 PASS`；
- 1000 AI 报告：
  `Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-performance-smoke.json`；
- 配置：1000 个真实生产 GameObject/逻辑实体，`Combat1000`，全 AI，
  `data-oriented-canonical`，30 warmup + 180 sample；
- logic average/P95/max：`21.8997 / 29.1202 / 38.0412 ms/tick`；
- 180/180 个正式 sampled tick 为 `0 B`，Gen0/1/2 collection 均为 0；
- final lockstep overall hash：
  `4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`；
- teardown：`restored=true`，active GameObject、world entity、claimed slot 与 active pool
  均恢复为 0，cleanup exception 为 0。

共同输入 writer 第二切片的新鲜证据：

- Unity 脚本编译：0 个 C# error；
- 联合 EditMode job `99c84c8511d846aebc6eefdcc30e1db2`：`393/393 PASS`；
- `BattleRuntimeSelfCheck`：`2026-08-12 22:51:36 PASS`；
- 同一固定路径重跑 1000 AI：30 warmup + 180 sample，logic average/P95/max 为
  `20.8958 / 25.3425 / 28.9198 ms/tick`；
- 正式 sampled tick 为 `0 B`，Gen0/1/2 collection 均为 0；
- final lockstep overall hash 仍为
  `4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`；
- teardown `restored=true`，cleanup exception 为 0；最大 backlog 为 7，丢弃 backlog 为 26。

该次 Editor 单样本没有精确 toggle A/B，只能证明未观察到明显回退、hash/零 GC/生命周期保持，
不能把数值差异宣称为 writer 带来的确定性能提升。

输入生命周期 writer 第三切片的新鲜证据：

- Unity 脚本编译：0 个 C# error；
- 输入 writer、live-slot、local frame provider 与 strict delayed buffer 聚焦 job
  `6dda2371888b421fb62bfda872d76f34`：`23/23 PASS`；
- 更早的输入/AI/SoA/压力工具联合 job `df71d4c9a1414b5eb780e1fb453fa0c2`：
  `448/448 PASS`；
- `BattleRuntimeSelfCheck`：`2026-08-12 23:12:54 PASS`；
- 最新 1000 AI：30 warmup + 180 sample，logic average/P95/P99/max 为
  `21.4288 / 26.4588 / 34.2308 / 37.1369 ms/tick`；
- 正式 sampled tick 为 `0 B`，Gen0/1/2 collection 均为 0，final hash 仍为
  `4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`；
- teardown `restored=true`，active GameObject/world entity/claimed slot 全部为 0，cleanup
  exception 为 0；最大 backlog 为 7，丢弃 backlog 为 24。

该 writer 切片是唯一写入边界的准备工作，不宣称降低单 tick 成本。最新 P95 低于 33.333 ms，
但 max、P99、backlog 和正式 60 秒矩阵尚未满足 U9 门禁。

组合技/direct-frame 共用 resolver 第四切片的新鲜证据：

- Unity 脚本编译：0 个 C# error；
- 输入/AI/SoA 联合 EditMode job `e2342e5439064732946c9605fab5bae1`：`188/188 PASS`；
- 新增注册 world AI 直接解析与 runtime 提交测试：`1/1 PASS`；
- `BattleRuntimeSelfCheck`：`2026-08-12 23:38:04 PASS`；
- 1000 AI、30 warmup + 180 sample、关闭详细诊断的两次复测：
  - run 1 average/P95/P99/max：`21.6286 / 25.7297 / 29.3456 / 31.1076 ms/tick`；
  - run 2 average/P95/P99/max：`21.8553 / 27.3231 / 31.3929 / 33.5564 ms/tick`；
- 两次均为正式 sampled tick `0 B`，Gen0/1/2 collection 均为 0；final lockstep overall hash 均为
  `4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`；
- 两次 teardown 均 `restored=true`，cleanup exception 为 0，active GameObject/world entity/claimed
  slot 全部恢复为 0；
- 详细诊断复测确认 `CharacterInput/AI/InputStateSyncFromRuntime` 从较早基线约
  `0.0656 ms/tick` 降为 `0`。详细诊断自身会增加计时开销，不用于总 FPS 门禁。

与第三切片旧基线 `21.4288 / 26.4588 / 34.2308 / 37.1369 ms/tick` 相比，平均值没有稳定提升，
两次平均约轻微回退 `1.5%`；P95 基本持平，P99/max 尾部改善可复现。故本切片按“移除重复状态
镜像与重复算法、目标子阶段归零、整体无显著回退”保留，但不得宣称平均帧率提升或 U9 完成。

动作事务 writer 第五切片的新鲜证据：

- Unity 脚本编译：0 个 C# error；
- 新增注册 world 动作 writer 定向测试：job `ebbe1ed671104d0880180619608436bc`，`1/1 PASS`；
- CharacterInput、AI kernel、AI snapshot/fail-closed 与生产 profile 联合 EditMode：
  job `5166dbdbf36345428d0c4ce9cd12fa06`，`73/73 PASS`；
- `BattleRuntimeSelfCheck`：`2026-08-12 23:54:47 PASS`；
- 1000 AI、30 warmup + 180 sample：average/P95/P99/max 为
  `21.5764 / 26.4189 / 28.6741 / 30.1509 ms/tick`；
- 正式 tick 为 `0 B`，Gen0/1/2 collection 均为 0；最终 lockstep overall hash 仍为
  `4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`；
- IndexedCanonical fallback、unified pre-commit fallback 与 post-commit hard breach 均为 0；
- teardown `restored=true`、cleanup exception 为 0；最大 backlog 为 7、丢弃 backlog 为 32。

该次结果与第四切片相邻波动区间一致，未观察到性能回退；writer 只建立组合所有权边界，不能
把该数值宣称为 FPS 提升。backlog 仍不满足 U9 稳态门禁。

持久输入 store 第六切片（2026-08-13）：

- 新增 world-owned `BattleCharacterInputStore`，以 runtime slot + generation 绑定输入真值；注册时从
  runtime 捕获初值，精确 generation release、world reset 与 desktop capacity growth 均在 registry
  生命周期边界执行，旧 handle 不能清除复用槽的新一代状态；
- `DataOrientedCanonical` 的 AI capture 与组合动作 progress reader 改为读取该 store；Legacy profile
  仍读取 runtime 兼容真值，因而没有把诊断 shadow 冒充 production canonical；
- AI kernel 每次会整体消费 `AiDecisionInputState`，最终实现采用连续值类型行，而不是把 64 字节输入
  拆成十多组数组后逐字段解包。这是按实际访问模式选择 AoS row，并不改变整个 ECS 的 Direct SoA
  主存储决策；human full commit 只更新 held/previous/progress 子段，不会清掉 AI history；
- AI 完整决策提交只写一次 store 行，再写一次 runtime 兼容镜像；已删除同一次事务中的重复
  `CommitFull` / `CommitProgress`。runtime 镜像仍保留，因此本切片尚未删除旧对象字段。

第六切片验证：

- Unity fresh 编译为 0 C# error；
- 输入、AI kernel、AI sensing、SoA shadow 与执行 profile 联合 EditMode job
  `cfceccbf9bd242f6aa5fabbb359c84d0`：`183/183 PASS`；
- 新增定向测试覆盖 store 成为 DataOriented capture 真值、human partial/full 提交保留 AI history，
  以及槽位 release/reuse 后旧 generation 不能清除新状态；
- 完整 `BattleRuntimeSelfCheck` 于 `2026-08-13 00:39:20` fresh PASS；
- 同配置 detailed 1000 AI 的初版 bit-packed store 为 average/P95/P99/max
  `24.2364 / 29.8481 / 33.6271 / 37.7386 ms/tick`；连续值类型输入行改为
  `23.7045 / 28.0330 / 30.5327 / 33.2456 ms/tick`，其中
  `IndexedCanonicalCapture` 从 `0.6284` 降至 `0.4545 ms/tick`；
- 迁移前同 detailed 口径为 `23.5199 / 28.3089 / 30.9806 / 32.0999 ms/tick`，所以最终 store
  相对旧口径 average 只存在约 `0.8%` 波动，P95 略好；该结果证明移除了 packed 重组回退，但不构成
  10% 性能晋升声明；
- 同负载 Legacy/DataOriented detailed A/B 的最终十域 hash 均为
  `752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，两边均为正式 tick
  `0 B`、Gen0/1/2 collection `0`、fallback/hard-breach `0`、teardown `restored=true`；Legacy
  average/P95 为 `33.7278 / 45.2979 ms`，DataOriented 最终为 `23.7045 / 28.0330 ms`；
- 关闭详细计时后的 production profile 报告
  `Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-performance-smoke.json` 为 1000 个真实生产
  GameObject/逻辑实体、30 warmup + 180 sample，average/P95/P99/max
  `21.4001 / 26.9671 / 35.0497 / 37.6365 ms/tick`，正式 tick `0 B`、Gen0/1/2 collection `0`、
  fallback/hard-breach `0`、teardown 完整恢复；maximum backlog `7`、dropped backlog `25`。

第七个输入小切片关闭了剩余 AI-only target/boundary writer：

- `Unk360/Unk3FC/Unk400` 的 AI 决策与 N30 teammate broadcast 均通过 `BattleAiInputWriter`
  写入 generation-owned store；N30 仍严格按 X、Z 顺序消费两次 RNG；
- kind14 四方向阻挡由 world-owned `BattleBoundaryWriter` 依权威方向规则发布，Character/Entity/Weapon
  mechanics 消费并清零后同步回 store；
- `ResetInputState` 只清理输入 key/progress/history，不再误删独立 target/boundary 字段；
- runtime 字段仍作为 U6 兼容镜像，未注册对象仍走原兼容实现。

第七切片 fresh 证据：

- Unity 编译 `0 C# error`；
- 输入/AI/sensing/profile 联合 EditMode job `e444a15e01cd4c61ad9237935507c814` 为 `175/175 PASS`；
- 完整 `BattleRuntimeSelfCheck` 于 `2026-08-13 01:01:12` fresh PASS；
- 1000 AI、30 warmup + 180 sample production smoke 的 average/P95/P99/max 为
  `23.3822 / 27.9603 / 31.1907 / 34.1888 ms/tick`，正式 tick `0 B`、Gen0/1/2 collection `0`、
  final hash `752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`、
  fallback/hard-breach `0`、teardown `restored=true`；maximum backlog `7`、dropped backlog `31`。

该 store 现在关闭的是共同输入与 AI-only target/boundary 的持久所有权及 generation ghost 风险。
frame/motion/HP/link 仍有 runtime writer，unified row 仍需在同 tick 按 slot 刷新，因此不得删除
runtime 兼容镜像、Legacy oracle 或 row refresh，也不得据此宣称 U6/U9 完成。

第八个 frame 前置切片进一步关闭了 CharacterInput 后的重复镜像修补：

- 权威 C# `FrameRuntime.SetFrameImmediate` 只有一个 `Entity.Frame`；Unity 所有生产 frame-id 写入现已统一
  进入 `LF2Entity.WriteCurrentFrameId`，在同一调用点写 `Frame.N` 与 `Runtime.Frame`；
- `FrameTransistor` 已直接同步 wait/next，`LF2Health` 已直接绑定 HP/MP/PP，故默认
  `RefreshRuntimeSnapshotAfterCharacterInput` 不再重复复制 12 个字段；
- 压力工具保留强制完整回拷开关作为 Legacy oracle；默认与强制旧路径交错两轮 A/B 的 final hash 完全
  一致，正式 sampled tick 均为 0 B；
- 默认两轮 average/P95 为 `20.4753/24.6792 ms`、`20.5460/24.7788 ms`；强制旧回拷两轮为
  `22.2049/27.1037 ms`、`21.3191/25.3920 ms`。该收益方向可复现，但不据短样本关闭 U9；
- fresh Unity 编译 0 error，联合 EditMode `176/176 PASS`，完整 self-check 于
  `2026-08-13 01:19:16 PASS`。

该切片仍不是持久 frame/motion canonical store：它只令兼容期两个 frame 镜像不再分叉，并删除随后
用于补救分叉的重复全量复制。下一切片必须沿同一 gateway 把 slot/generation-owned frame/motion store
接入注册、释放、复用和真实 writer，再逐步让 unified row 直接读取该 store。

本切片是写入所有权迁移，不宣称性能提升。P95/max 仍受 Editor 环境波动影响；U9 的正式
60 秒以上矩阵、P95 门禁和 Windows Player 证据尚未执行。

## 4. 下一步

生产 `IndexedCanonical`/world-bound human 路径的共同 key/cooldown/combo/history 生命周期、组合
resolver、动作事务入口、AI-only target/boundary 与持久 generation-owned 输入 store 已进入
world-owned 组合对象；Legacy 发布前整批 fallback / 发布后 hard-breach 边界也已明确。下一步迁移
frame/motion/lifecycle 的完整 writer 簇，使 unified row 能由前序 writer 增量维护；在此之前不把
U3 tick-end shadow 冒充 canonical store，也不删除同 tick row refresh。旧 Legacy writer 只在新
store 与全部 reader 闭合后删除。

### 第九切片补充：七字段 AI frame/motion projection

完整 26 字段原写点双写会让 1000 AI average 稳定回退到约 `22.6～22.7 ms/tick`，因此没有保留。最终仅把 unified AI row 实际消费的 `XInt/YInt/ZInt/Vx/Facing/Frame/HitStop` 接入 slot/generation-owned Direct-SoA projection。

fresh 179/179 EditMode、完整 self-check、两次 1000 AI 零 GC与相同 hash 均通过，两次 average/P95 为 `21.0860/25.3055 ms`、`21.4329/26.0455 ms`。其余 row 字段仍需按 writer 逐项迁移，row refresh 与 Runtime 兼容字段当前不能删除，U6/U9 仍未完成。

第十切片进一步复用同一个 input store，为 unified row 提供只包含 `InputHistoryGate/CachedTargetSlot/CoordinateTargetX` 的最小值类型 projection。该 reader 没有新建状态或写入路径；fresh EditMode `179/179 PASS`、self-check `02:16:39 PASS`，1000 AI average/P95/P99/max 为 `21.6311/25.6692/27.6213/28.7059 ms`，0 B、0 次 GC、hash 不变、teardown 完整恢复。

### 第十一至十三切片补充：unified row 剩余 Runtime reader 闭合

`RelationTeam/LinkState/KillCount/TargetSlotIndex` 已进入低频 `BattleRelationLinkStore`，`HP/HPBound/HP3/PP` 已进入 `BattleVitalStore`；二者均按原 Runtime/LF2Health 写点同步，并在 register/release/reset/grow 时由 world 管理 generation 所有权。DAT `state` 不能只由 frameId 推算，因此 `LF2FrameInfo.D` setter 现在无分配地同步 `D.state` 到既有 frame/motion store，覆盖所有直接 frame data 替换点。

最终 `TryRefreshAiUnifiedSnapshotExecutionRowAfterCharacterInput` 除身份解析外已不读取 Runtime 战斗字段。fresh Unity 编译 0 C# error、EditMode `185/185 PASS`、self-check `2026-08-13 02:43:16 PASS`；1000 AI average/P95/P99/max `22.0254/26.1165/29.5433/30.4045 ms`，正式 tick 0 B、Gen0/1/2 collection 0、hash 不变、hard breach 0、teardown 完整恢复。row refresh 本身和派生索引维护尚未消除，下一步是通过前序 writer 增量维护同一 row，并保留 Legacy oracle/fail-closed；U6/U9 仍未完成。

### 第十四切片补充：staged dirty unified row publisher

直接在四个 store 原写点立即修改已发布 row 的候选改变了同 tick 可见边界，并使 1000 AI average 回退到 `42.2450 ms/tick`，因此没有保留。最终实现使用 world-owned `BattleAiUnifiedRowPublisher`：原写点只按 slot + generation 暂存 dirty 最终值，当前实体的 CharacterInput 结束后才由 `TryCommitPending` 原子提交。这样保持原 authority 顺序，也移除了正式路径每实体重读四个 projection、复制 19 个字段的成本。

强制 full refresh 仍作为 exact oracle，并在完成 full capture 后丢弃 pending；默认关闭的 `ValidateIncrementalAiUnifiedRowForDiagnostics` 会重读四个 canonical store 的 19 个字段逐项核对，但不反写。fresh Unity 编译 0 C# error、EditMode job `33cc0f620af24c4ba48e3b7a5c4fc3cd` 为 `185/185 PASS`（增量 shadow 30 tick × 4 实体共 120 次）、self-check `2026-08-13 03:20:07 PASS`。

增量 production smoke average/P95/P99/max 为 `21.5776/25.7700/28.6098/29.9695 ms`；强制 full refresh oracle 为 `22.7332/26.9240/29.9166/33.4480 ms`。两者 battle parity hash 均为 `752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，且正式 tick 0 B、Gen0/1/2 collection 0、hard breach 0、authority success、teardown 完整恢复。capacity detail 的 row-refresh average/P95 从前序 `0.3618/0.3704 ms` 变为 `0.3527/0.3621 ms`，只作为结构迁移无明显回退的证据，不宣称显著性能提升。

本切片仍不代表 U6 完成。当时初始 unified snapshot 捕获、完整 frame/motion/lifecycle canonical world、对象 shell 热循环与 U9 长时 Player 矩阵尚未闭合；初始 19 个战斗字段读取由第十五切片继续处理。

### 第十五切片补充：初始 canonical row handle capture

正式 UnifiedAuthority 每 tick 初始 row 的 19 个战斗字段现在直接从 `BattleCharacterInputStore`、`BattleFrameMotionStore`、`BattleRelationLinkStore` 与 `BattleVitalStore` 捕获，不再从 Runtime 兼容镜像重建。最终热路径复用 registry 已验证的 `RuntimeEntityHandle`，四个 store 分别只检查 slot 范围与 generation；slot 复用后的旧 handle 会被全部拒绝。Legacy、shadow 和强制 full oracle 仍保留原 Runtime 路径。

压力报告新增 canonical initial capture exact-closure 门禁。fresh Unity 编译 0 C# error，EditMode job `1537682b724944f0ad4838ee2fe890f9` 为 `421/421 PASS`，self-check `2026-08-13 03:48:50 PASS`。最终两次 1000 AI average/P95/P99/max 为 `21.8961/25.8188/29.3556/30.4394 ms` 与 `21.7277/26.1409/29.4532/30.1781 ms`；两次均为 canonical initial capture `209000/209000`、sampled tick 0 B、Gen0/1/2 collection 0、相同 battle parity hash、hard breach 0、authority success、teardown 完整恢复。

两轮均值相对第十四切片约 +1.1%，属于 Editor 短样本波动，不宣称性能提升。剩余 U6 对象读取包括 stable identity、object id/type、boundary flags、first-ten move-mode，以及更广的 frame/motion/lifecycle 热循环；U6/U9 仍未完成。

### 第十六切片补充：boundary store 到 unified row 的延迟发布

四个方向阻挡位原本已经由 `BattleBoundaryWriter` 写入 generation-owned input store，但 UnifiedAuthority 初始捕获和 post-CharacterInput refresh 仍直接读取 Runtime bool。第十六切片把正式路径改为：input store 持有 C# 决策编码，原写点只向 `BattleAiUnifiedRowPublisher` 暂存 dirty 最终值，当前实体 CharacterInput 结束后才同时发布 decision 与 sensing 两套编码。这样保持原 authority 可见边界，也避免每实体重复读取四个 Runtime bool。

强制 full refresh 继续从 Runtime 重建并作为 exact oracle。fresh Unity 编译 0 C# error；改动直接相关测试 `78/78 PASS`，扩大聚焦 job `0e70bb9e8f8540a3a2d155e272834084` 为 `421/421 PASS`；self-check `2026-08-13 04:07:47 PASS`。1000 AI 增量路径 average/P95/P99/max 为 `22.1538/26.7865/29.1554/33.1196 ms`，强制 full oracle 为 `23.1081/27.3117/30.1937/31.8571 ms`；两者 parity/lockstep hash 相同，正式 tick 0 B、Gen0/1/2 collection 0、hard breach 0、teardown 完整恢复。

约 4.1% 的短样本差距不作为稳定性能晋升声明。本切片关闭 boundary 的正式 Runtime 热读取；input store、publisher 与 Legacy/full oracle 的所有权边界保持不变，U6/U9 仍未完成。

### 第十七切片补充：identity / object type canonical store

正式 UnifiedAuthority 初始 row 的 `StableId/ObjectId/DataObjectType` 已改从新的 generation-owned `BattleIdentityStore` 捕获。store 在 register/release/reset/grow 管理代际，并由 `LF2Entity.ObjectId`、`LF2FrameCache.Load/Clear` 与 dormant partner lifecycle stable-id 恢复在原写点同步；slot 复用后的旧 handle 会被拒绝。Legacy、shadow 与强制 full Runtime oracle 不变。

fresh Unity 编译 0 C# error；identity/input 定向测试 `22/22 PASS`，扩大聚焦 job `4105b6d6db164f47be4d66c875624645` 为 `423/423 PASS`；self-check `2026-08-13 04:32:40 PASS`。1000 AI 增量路径 average/P95/P99/max 为 `22.0894/26.2908/28.6278/29.6332 ms`，强制 full oracle 为 `23.0996/27.4190/30.4386/31.8757 ms`；两者 parity/lockstep hash 相同，正式 tick 0 B、Gen0/1/2 collection 0、canonical capture `209000/209000`、hard breach 0、teardown 完整恢复。

约 4.4% 的短样本差距不作为稳定性能晋升声明。本切片关闭初始 identity/object type 对象读取；first-ten move-mode 与完整 frame/motion/lifecycle 对象式热循环仍待 U6 后续切片处理。

### 第十八切片补充：first-ten move-mode 复用 canonical row

UnifiedAuthority 的 first-ten move-mode 初始产品现在在 canonical row 捕获完成后直接读取 row 的 DAT type/HP/X/Z，不再对同一实体二次读取对象字段。post-CharacterInput 失效检查使用当前 generation identity store 与 publisher 已提交 row，因此仍能检出 DAT 身份、生命和位置变化。Legacy、shadow、deep validator 与强制 full oracle 保留。

fresh Unity 编译 0 C# error；扩大聚焦 job `3410e3a3d4564fa588a3c4a30cf6cfdb` 为 `423/423 PASS`；self-check `2026-08-13 04:43:39 PASS`。1000 AI 增量路径 average/P95/P99/max 为 `21.8479/26.0178/27.8500/29.0220 ms`，强制 full oracle 为 `23.1703/28.1723/31.4818/32.4411 ms`；两者正式 tick 0 B、Gen0/1/2 collection 0、parity/lockstep hash 相同、hard breach 0、teardown 完整恢复。

约 5.7% 的短样本差距不作为稳定性能晋升声明。下一步转向实体边界遍历、派生索引维护及更广的 frame/motion/lifecycle 对象式热循环。
