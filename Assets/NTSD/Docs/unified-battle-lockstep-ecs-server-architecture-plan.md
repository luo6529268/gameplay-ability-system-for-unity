# NTSD 统一战斗内核、帧同步、自研 ECS 与未来服务器架构方案

> 建立日期：2026-08-11
> **2026-08-24 权威修正**：本文早期使用的“C# 权威”“权威 C#”和“C# 单一真值”均为历史移植上下文，不再定义战斗规则。唯一规则 authority 为 `J:\QQFile\NTSD2.4\ntsd_release` 中参与 release 构建并运行到 `ntsd_new.exe` 的 C++ live runtime；`ntsd_release_C#` 仅用于历史意图、命名和交叉检查。下方所有保留的 C# 测试/迁移记录只说明当时的辅助验证，不可替代 C++ release trace。
> **2026-08-20 C++ authority migration（覆盖本文“C# authority”措辞）**：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live `game_tick(...)` 是 Unity 战斗行为唯一权威。U0～U9 的架构、性能、Mono/IL2CPP、worker、零 GC 与 1000 AI 结果继续有效，但它们只证明 Unity 内核的性能/确定性与当时的 C# 基线；不再自动证明 C++ release 场景行为已对齐。后续任何保持或提升 ECS/fast path 的决定必须增加 C++ trace 等价门禁。
> **2026-08-20 当前工作树最终复验（覆盖下方全部历史进度与旧 Temp 路径）**：国际版 `Unity 2022.3.62f3 (96770f904ca7)` 已对当前代码重新完成 Windows Mono Player 构建、U7 Mono/IL2CPP correctness、U8 worker/synchronous 对照和 U9 五场景正式矩阵。干净全量 EditMode job `126790e9345043bd83c1e5a81b1f38a5` 为 `1265/1265 PASS`，`BattleRuntimeSelfCheck` 最终于 `2026-08-20 12:14:11` 写入 `PASS`。U7 两份 fresh 报告均为 `Passed`，source/restored checksum 均为 `2f92a339254225de11790c2d4eb8fc51f36e7cdd6245a891d25f041ef17ac093`，replay checksum 均为 `3DEB30C4D190E5FB`，恢复 `(slot, stableId, generation)` 均为 `(3, 100, 1)`。U8 两份 300 tick 报告的 parity/lockstep 十域 hash 完全一致、0 B、中央 draw=1、cleanup 通过。U9 Idle/Move/Dispersed/Combat/Concentrated 均为 1000 个真实生产实体、300 tick 预热 + 1800 tick 正式采样；完整帧平均 `16.6681～16.7091 ms`（约 `59.85～59.99 FPS`），逻辑 P95 为 `3.9235～6.5989 ms`，SetPass=6、中央 draw=1，四条 managed-memory 边界和 Gen0/1/2 均为 0，正式容量拒绝为 0，U6 mismatch 为 0，worker `2100/2100`，teardown 全恢复。当前证据位于 `Temp/U7-Windows-IL2CPP/` 与 `Temp/U9-Windows-Player/Reports-2022.3.62f3/`。U0～U9 单机范围据此关闭；S0～S9、服务器业务、Socket、ACK、Jitter Buffer、房间、登录、重连、T8 默认 `stage.dat` 与 Android 真机均未进入。
> **2026-08-16 U7 最终关闭（覆盖下方历史阻塞记录）**：项目已由国际版 `Unity 2022.3.62f3 (96770f904ca7)` 打开，Windows Mono 与 IL2CPP Player variation 均为同一版本和 revision。真实 Windows Mono/IL2CPP correctness gate 均写出 `Passed` 报告；两者 `sourceChecksum`、`restoredChecksum`、`replayChecksum`、恢复 slot、stable id、generation 逐项相同，纯值 transfer/factory 与 snapshot -> mutate topology -> restore -> journal replay 均通过。最终聚焦 EditMode job `f0e76a50c9064e239ea1c2f438be465b` 为 `39/39 PASS`，`BattleRuntimeSelfCheck` 最终于 `2026-08-17 00:01:11` 写入 `PASS`。门禁构建对 Mono 与 IL2CPP 一致地临时关闭 Burst AOT，以隔离本机 Burst 1.8.21 损坏的 Windows hash cache；`finally` 会恢复 Burst、后端、IL2CPP 配置与 Frame Timing Stats，并保存 ProjectSettings。正常 Burst 配置下的 U9 性能证据仍单独成立。至此 U0～U9 单机计划按既定范围关闭；服务器阶段现按 S0～S9 细分且仍未开始，必须由用户再次确认后才能进入，T8 默认 `stage.dat` 与 Android 真机继续排除。
> **2026-08-16 Unity 升级后 U9 复验**：新构建的可见 Windows Mono Player 使用 1000 个真实 Combat1000 生产实体、300 tick 预热 + 1800 tick 正式采样、dedicated worker、中央渲染、零 GC 与 U6 所有权硬门禁。报告 `Temp/NTSD_U9_2022.3.62f3_combat1000.report.json` 为 `StoppedCleanly`；完整渲染 CPU average/P95 为 `19.0828/25.0888 ms`，约 `52.40 FPS`，逻辑 tick average/P95/P99/max 为 `3.6580/4.7643/6.4356/10.3933 ms`。SetPass 恒为 6、中央 draw 恒为 1；tick/driver/presentation/PlayerLoop 分配均为 0 B，Gen0/1/2 collection 均为 0，容量拒绝/丢弃为 0，worker `2100/2100` 完成，cleanup 将 1000 个活动 GameObject、1000 个 world entity 与 1000 个 claimed slot 全部恢复为 0。升级后仍满足 1000 AI / 30 FPS 正式容量目标。
> **2026-08-15 单机阶段最新状态（覆盖下方保留的历史进度流水）**：U0～U6、U8、U9 已完成实现与当前可执行验收；U7 的 snapshot/restore/replay、纯值 transfer/factory、Windows Mono Player correctness gate 已完成，但 Windows IL2CPP gate 仍保持“外部工具链待验证”，不能冒充通过。Windows IL2CPP 模块现已安装并成功完成 IL2CPP C++ 生成、MSVC 编译和 Player 构建，但当前 `2022.3.40f1c1 (0bae6c114c78)` Editor 被混装了国际版 `2022.3.40f1 (cbdda657d2f0)` IL2CPP Player variation；真实 Player 启动日志为 `Expected version: 2022.3.40f1`、`Actual version: 2022.3.40f1c1`，在加载 PlayerSettings 前退出，因而仍没有可执行 IL2CPP correctness 报告。最新 U8 Windows Player worker/synchronous 300 tick 十域 hash 完全一致，正式 tick 0 B、三代 collection 0。U9 五个 Windows Player 场景均完成 300 tick 预热 + 1800 tick 正式采样，完整渲染平均 `55.80～59.11 FPS`，最坏 Concentrated1000 的逻辑 average/P95/P99/max 为 `4.3253/5.8677/8.5765/10.4058 ms`；五场景 SetPass 均为 7、中央 draw 均为 1、四条 managed-memory 边界 0 B、容量拒绝 0、cleanup 全恢复。Authority400 fresh full/full diagnostic 为 6/6 `equal-diagnostic`、`firstDifference=null`；fresh self-check 为 `2026-08-15 21:29:11 PASS`。完整证据见 `Docs/unified-battle-u9-final-acceptance-20260815.md`。服务器阶段现按 S0～S9 细分且未开始；Windows IL2CPP、T8 默认 `stage.dat`、Android 真机不被伪装为本轮已验证项。
> **2026-08-15 23:36 fresh 门禁补充**：强制 Unity 脚本刷新后未发现 `error CS`；U7 snapshot/restore/ring/session/checksum/runtime-validation 扩大聚焦 job `992fe9182af749118696ae3e511157ff` 为 `33/33 PASS`；`BattleRuntimeSelfCheck` 结果文件于 `23:36:40` 写入 `PASS`。当前唯一未关闭项仍是匹配发行版的真实 Windows IL2CPP Player 跨运行时比较；该证据不替代 IL2CPP 报告，也不授权进入 S0。
> **2026-08-16 Editor 发行版定调**：用户明确只使用 Unity 国际版，不需要中国版 `f1c1`。当前正在运行的 `D:\Unity\HubEditor\2022.3.40f1\Editor\Unity.exe` 实际 ProductVersion 为中国版 `2022.3.40f1c1_0bae6c114c78`，项目 `ProjectVersion.txt` 也已记录为 `2022.3.40f1c1 (0bae6c114c78)`；目录名中的 `2022.3.40f1` 不能证明它是国际版。U7 后续只接受完整国际版 Editor `2022.3.40f1 (cbdda657d2f0)` 与相同 revision 的 Windows IL2CPP Player，不再寻找或安装 `0bae6c114c78` 中国版 IL2CPP 模块。切换主项目前必须关闭当前 Editor，并避免两套 Editor 同时写同一个 `Library`；不得仅手工修改 `ProjectVersion.txt` 冒充迁移完成。
> 当前状态：U0～U5 已完成，U6 正在执行。Registry page-SoA、CharacterInput generation-owned store、world-owned input/action writers、AI-only target/boundary、frame-id 原子网关与 CharacterInput 后重复 12 字段回拷删除均已完成。第九至十三切片把 unified AI row 消费的 frame/motion、input target、relation/link、vital 与 DAT state 接入 slot + generation-owned canonical stores；第十四切片新增 world-owned `BattleAiUnifiedRowPublisher`，以 staged dirty + post-CharacterInput 原子提交移除正式路径每实体 19 字段重读/复制，立即写 row 的 `42.2450 ms/tick` 负实验已撤回。第十五至十七切片依次把初始 row 的 19 个战斗字段、directional boundary 与 identity/object type 迁到 generation-owned stores。第十八切片令 UnifiedAuthority 的 first-ten move-mode 初始构建直接复用已捕获的 canonical row，并在 post-CharacterInput 失效检查中读取当前 generation identity store 与 publisher 提交后的 HP/X/Z row，不再二次读取实体字段；Legacy、shadow、deep validator 与强制 full Runtime oracle 保留。fresh Unity 编译 0 C# error、423/423 扩大聚焦测试、`2026-08-13 04:43:39` self-check PASS；1000 AI 增量路径 average/P95/P99/max 为 `21.8479/26.0178/27.8500/29.0220 ms`，强制 full oracle 为 `23.1703/28.1723/31.4818/32.4411 ms`，两者 battle parity/lockstep hash 完全一致，正式 tick 0 B、Gen0/1/2 collection 0、209000/209000 canonical capture、hard breach 0、teardown 完整恢复。该切片只按所有权与等价证据保留，不把约 5.7% 的短样本差距宣称为稳定收益。U6 尚需继续处理实体边界遍历、派生索引维护及完整 frame/motion/lifecycle 对象式热循环，U6/U9 均未完成；服务器 S0、T8 默认 `stage.dat` 与 Android 真机仍不进入当前阶段。
> 2026-08-15 最新总状态：U7 已形成可用的本地进程快照、同 runtime 实体拓扑恢复及 journal replay 闭环；新增聚合 `BattleStateSnapshotBuffer`、`LockstepSnapshotRing`、精确恢复器及明确 failure code。恢复可以重建快照时的 claimed slot、generation、实体关系、aRest/vRest、pending event，并能撤销快照后新增实体或恢复快照后被删除的实体。fresh Unity 聚焦 job `6204af59a8c64d5ea96d034f2c886a18` 为 `8/8 PASS`，其中 warm exact restore 为 `0 B`。当前实现仍通过本地快照保存的逻辑 shell 引用恢复拓扑，尚未完成跨进程纯值 entity factory、Windows IL2CPP 门禁与 U8 专用 worker；因此 U7 只标记为“本地恢复闭环已验证，跨运行时门禁未完成”，U8/U9 未完成，服务器 S0 仍不得启动。
> 2026-08-15 后续状态校正：U7 已补齐纯值 transfer/factory 门禁，并在真实 Windows Mono Player 中完成 snapshot -> mutate topology -> restore -> journal replay。报告 `Temp/U7-Windows-IL2CPP/Mono/u7-runtime-report.json` 为 `Passed`，source/restored checksum 均为 `2f92a339254225de11790c2d4eb8fc51f36e7cdd6245a891d25f041ef17ac093`，`pureValueTransferPassed=true`、`restoreReplayPassed=true`。Windows IL2CPP Player 尚不能执行的原因是当前 Unity 安装缺少 Windows IL2CPP Player 模块；这属于外部工具链阻塞，不能伪装成通过，也不授权进入服务器阶段。Burst 1.8.16 的 Windows hash cache 另有损坏头，Mono correctness gate 曾临时关闭 Burst AOT 后运行并已立即恢复项目设置；该证据不能代替正常 Burst 配置下的 U9 性能报告。
> U8 最新状态：生产 `SimulationTickDriver` 已接入专用 `DedicatedBattleSimulationWorker`、固定容量输入队列、双槽不可变 publication、主/模拟线程所有权、presentation acknowledgement/finalization 与 worker 失败停机边界，默认在满足资格时启用。1000 AI worker 报告 `Temp/NTSD_ProductionEntityStress.u8-worker-combat1000-30x300-20260815.json` 的逻辑 tick average/P95/P99/max 为 `16.7861/21.5902/24.9610/35.2916 ms`，完整渲染帧 CPU average/P95 为 `6.3849/7.5727 ms`，主线程 average `2.2232 ms`、Render Thread average `0.6136 ms`；300/300 正式 tick 为 `0 B`、三代 collection 0、容量/cleanup 通过。同步对照 `Temp/NTSD_ProductionEntityStress.u8-sync-combat1000-30x300-20260815.json` 的逻辑 average/P95/P99/max 为 `16.4258/21.0007/23.3802/24.9545 ms`，十域最终 hash 与 worker 完全一致。fresh U6/U7/U8 联合聚焦 job `b00b0ab9b234469eb1dea618ee285552` 为 `54/54 PASS`。U8 的线程和 publication 合同已经具备生产接线与 Editor 运行证据，但 U9 五场景 Windows Player 60 秒矩阵尚未完成，因此仍不得声明整个单机阶段完成。
> U6 最新状态（第六十切片）：`PostFrameAdvanceDeathCleanupAll` 在其既有活动实体遍历内发布同 tick、epoch-guarded 的 PreInteraction 全 pass no-op 证明；`PreInteractionTickAll` 命中时 O(1) 消费，证明无效时完整回退原扫描。两轮同种子 1000 AI A/B 中 `PreInteraction` average 从 `0.8584 ms` 降至 `0.6882 ms`，`DeathCleanup + PreInteraction` 合计从 `1.2208 ms` 降至 `1.0517 ms`，完整 tick average 从 `18.9709 ms` 降至 `18.6477 ms`；hash 相同、正式 tick 0 B、三代 collection 0。无高频探针的 300 tick 最终回归为 average/P95/P99/max `15.7364/17.7918/19.1523/19.3988 ms`。runtime/editor 构建 0 error，聚焦 `14/14 PASS`、压力工具整类 `247/247 PASS`、self-check `2026-08-15 01:52:12 PASS`。该切片保留，但只证明逻辑 tick 已进入 30 Hz 预算，不关闭 U6/U9，也不替代 U9 Windows Player 60 秒完整帧验收。
> U6 最新门禁：单 pass active-slot 缓存与 exact-character FrameAdvance 空尾链两个候选均已通过真实 1000 AI A/B 证明无稳定收益，并完整撤回；撤回后 `423/423 PASS`、self-check `2026-08-13 05:35:09 PASS`、1000 AI average/P95 `22.1189/26.6252 ms`、正式 tick 0 B、hash 不变、teardown 完整恢复。下一步只处理 fresh detail timing 中占比足够大的共享 canonical 产品或完整字段簇。
> U6 第四十一候选验证了 collision formal participant 的前置 occupant 查询与 generation-handle 校验合并。两轮 1000 AI 的 `ParticipantBodyItrBuild` average 为 `1.0628/1.1365 ms`，相对第四十切片基线 `1.0824 ms` 方向不稳定，未达到晋升门槛，候选已撤回；两轮 hash 不变、正式 tick 0 B、teardown 完整恢复，聚焦 role-aware collision `68/68 PASS`。该负实验关闭“删除一次相邻 slot 查询即可改善 CandidateCollect”的微优化方向，下一步继续寻找能删除完整遍历、宽快照或重复 canonical 产品的切片。
> U6 第四十二切片继续收敛中央表现顺序物化。第三十四切片虽已将宽结构体比较排序替换为轻量 signed-Z radix，但随后仍按排序索引搬运两遍完整 `BattlePresentationEntitySnapshot`。当前冻结帧改为保存 `rank -> physical index`，公开 `GetEntity(rank)`、命令 base order、发布 rank map 与 frozen `CopyFrom` 语义保持不变，不再移动宽行。旧宽搬运仅在交错 A/B 期间由压力工具临时启用，最终已删除。相同 seed、1000 个真实生产 AI、30 warmup + 180 sample 的两轮旧/两轮新中，`BeginFrame/SortEntities` average 从 `0.5354/0.5394 ms` 降至 `0.0955/0.0950 ms`，稳定减少约 `82.3%`；`PresentationPublish/Total` average 从 `6.3666/6.3879 ms` 降至 `5.8531/5.9250 ms`，约减少 `7.7%`。四轮整 tick average 位于 `20.0980～20.4083 ms`，没有稳定整体 FPS 差异，故只按目标子段晋升。四轮 parity/lockstep hash 完全一致、正式 tick 0 B、Gen0/1/2 collection 0、teardown 完整恢复。最终无 timing 探针的 1000 AI 为 average/P95/P99/max `18.1067/22.3653/24.7464/25.8652 ms`，约 `55.23 logical tick/s`，零 GC、hash 不变、teardown 完整恢复；本地 runtime/editor 编译 0 error，最终相关回归 `277/277 PASS`，`BattleRuntimeSelfCheck` fresh `PASS`。该结果仍不关闭 U6/U9；下一批继续处理 `CharacterInput/EntityInputPass`、`CandidateCollect` 和 `LateEntityUpdate` 的完整热循环。
> U6 第二十一切片已把 collision formal participant 的每 tick `Dictionary<int,int> + HashSet<int>` 映射替换为预分配稠密代际槽位表，旧哈希路径仅保留为诊断 oracle。两轮 1000 AI 普通 A/B 均稳定减少约 `0.14 ms/tick`；nested timing 中 `CandidateCollect 3.8481 -> 3.6975 ms`，A/B hash 一致、正式 tick 0 B、teardown 完整恢复。该切片已按晋升门槛保留，但不宣称它单独关闭 1000 AI 性能目标；U6 下一步继续处理 `CharacterInput / LateEntityUpdate / FrameAdvance` 的完整字段簇与对象式热循环。
> U6 第二十二切片保持逐实体 opoint flush 边界不变，把同一 `LateEntityUpdateAll` pass 内重复的 `LF2ObjectPointFactory.Instance` 解析收敛为最多一次。两轮 1000 AI 细分 A/B 的 `LateEntityUpdate/TailAndQueuedFlush` 分别为 `0.8388 -> 0.7708 ms/tick` 与 `0.8422 -> 0.7851 ms/tick`，解析次数由 `210210` 降为 `210`，flush 均保持 `210000`。普通与细分总 tick 在约 1% 范围正反波动，不宣称整体 FPS 提升；全部运行 parity/lockstep hash 一致、正式 tick 0 B、Gen0/1/2 collection 0、teardown 完整恢复。候选按目标子段两轮稳定正收益和行为等价证据保留，A/B 旧入口已删除。清理后 fresh Unity 编译 0 C# error、压力工具整类 `237/237 PASS`、self-check `2026-08-13 08:28:47 PASS`；最终 1000 AI average/P95/P99/max 为 `21.9488/26.3602/28.1020/29.0273 ms/tick`，工厂解析 `210`、flush `210000`、正式 tick 0 B、三代 collection 0、hash 不变、teardown 完整恢复。该小切片不关闭 U6/U9。
> U6 第二十三切片把 `CharacterInput` action resolver 对 canonical progress row 的无条件回写改为字段簇 dirty commit：完整 17 字段状态未变化时，不再重复写 canonical store 与 Runtime 兼容镜像；发生变化时仍一次性提交全部字段，AI decision、combo/direct action、slot/generation 与权威消费顺序不变。两轮 1000 AI 细分交错 A/B 均跳过 `173629/209000` 次回写（约 `83.1%`）；`CharacterInput/AI/ComboUpdate` average 分别由 `1.1100 -> 1.0611 ms`、`1.1028 -> 1.0639 ms`，`EntityInputPass` average 分别由 `5.4276 -> 5.3538 ms`、`5.4007 -> 5.3573 ms`。整体 average 仅改善约 `0.2%～0.3%` 且 P95 有噪声，不宣称可见 FPS 提升。A/B parity/lockstep hash 一致、正式 tick 0 B、三代 collection 0、teardown 完整恢复，因此按目标子段稳定正收益与等价证据保留，并删除临时 Legacy A/B 入口。清理后本地两套工程 0 error、聚焦 `2/2 PASS`、压力工具整类 `237/237 PASS`、self-check `2026-08-13 08:56:01 PASS`；最终普通 1000 AI average/P95/P99/max 为 `22.0629/26.5426/29.7178/33.2235 ms/tick`，commit `35371`、skip `173629`、正式 tick 0 B、Gen0/1/2 collection 0、hash 不变、teardown 完整恢复。该切片不关闭 U6/U9；下一步继续选择占比足够大的完整 frame/motion/lifecycle 字段簇或跨 pass canonical 产品。
> U6 第二十四切片把 AI decision 后 `InputHistoryGate/CachedTargetSlot/CoordinateTargetX` 三字段 projection 的无条件发布改为完整字段簇 dirty publication；三字段均未变化时不再制造 publisher pending 与 post-CharacterInput row refresh 工作，任一变化时仍通过原子发布路径提交。两轮 1000 AI 细分交错 A/B 均跳过 `198583/209000` 次无变化发布（约 `95.0%`）；`UnifiedSnapshotExecutionRowRefresh` average 分别由 `0.3083 -> 0.1948 ms`、`0.2907 -> 0.2101 ms`，`EntityInputPass` 也稳定改善，总 tick average 约改善 `0.5%` 与 `1.2%`，但 P95/P99 有 Editor 尖峰，不扩大为稳定 FPS 声明。A/B parity/lockstep hash 一致、正式 tick 0 B、三代 collection 0、teardown 完整恢复，因此按目标子段稳定正收益与行为等价证据保留，并删除临时 Legacy A/B 入口。清理后本地两套工程 0 error、聚焦 `3/3 PASS`、压力工具整类 `237/237 PASS`、self-check `2026-08-13 09:20:55 PASS`；最终普通 1000 AI average/P95/P99/max 为 `23.2879/28.8326/34.7102/50.2735 ms/tick`，publish `10417`、skip `198583`、正式 tick 0 B、Gen0/1/2 collection 0、hash 不变、teardown 完整恢复。该切片不关闭 U6/U9；下一步继续选择占比更大的完整 frame/motion/lifecycle 字段簇或跨 pass canonical 产品。
> U6 第二十五切片候选测试了 AI kernel 单次决策的 16 字段自身行值缓存；目标扫描仍保持 SoA，扩大后的 context 最终使用 `in` 只读引用以排除结构体复制。两轮 1000 AI 中 `IndexedCanonicalKernel` average 为 `2.2131`、`2.2668 ms`，均劣于候选前 `1.6963 ms`，也劣于撤回后两轮 `1.9534`、`1.8024 ms`；无条件捕获宽自身 row 的固定成本大于数组读取收益，候选按负优化完整撤回。所有运行 hash 一致、正式 tick 0 B、三代 collection 0、fallback 0、teardown 完整恢复；撤回后 `AiDecisionKernel.cs` blob 与 HEAD 完全一致、该文件 diff 为 0，本地编译 0 error、Unity fresh refresh ready、聚焦 `36/36 PASS`。本实验只关闭这一错误优化方向，不关闭 U6/U9；下一候选必须减少已有工作或复用现有 canonical 产品，不能增加每 AI 固定复制成本。
> U6 第二十六切片把 UnifiedAuthority 的上一 tick 已发布 row 保留为下一 tick 的 canonical 起点，只按 publisher 累积的 generation-safe dirty slot 原子推进；capacity、occupancy epoch 或 generation 边界变化时仍强制完整重建，Legacy 与强制 full A/B oracle 保留。首版 1000 AI 暴露 `ZInt` 已写入 FrameMotion store、但整数 publisher 漏标 `ZBit` 的真实覆盖缺口，导致 RNG/slot/rest/event hash 分叉；补齐该字段并加入跨 tick 直接 `ZInt` 变更测试后，滚动—完整重建—滚动三次压力运行的 battle parity/lockstep hash 全部恢复为基线。滚动路径在 209 次 build 中执行 208 次 roll-forward，把 canonical initial capture 从 `209000` 降到 `1000`，dirty slot 为 `136655`；两次滚动 average/P95 为 `24.1951/28.9149 ms`、`23.9546/28.7390 ms`，中间 full rebuild 为 `23.9229/28.7023 ms`，没有稳定总耗时收益，因此只按移除每 tick 全实体 canonical 捕获/对象遍历的所有权迁移保留，不宣称 FPS 提升。三次均为正式 tick 0 B、三代 collection 0、authority/harness validity true、teardown 完整恢复；fresh Unity 编译 0 error，AI 快照整类 `60/60 PASS`、压力工具整类 `238/238 PASS`，self-check `2026-08-13 10:34:15 PASS`。本切片仍不关闭 U6/U9；下一步继续处理完整 frame/motion/lifecycle canonical world 与其余对象 shell 热循环。
> U6 第二十七切片重新闭合 `StageBounds` 的权威写入边界：权威 C# `GameTick.StageBounds` 每 tick 两次只按 slot 升序读取活动角色、夹取 Z，并写回 `Z/ZInt`，不刷新 Team、HP、Frame 或完整 Runtime 快照。U6 writer 闭合后，Unity 正式默认路径因此切换为 `DataOriented`；exact `LF2Character` 只写同一对 canonical 字段，未知派生类型继续走虚调用兼容回退，旧的宽快照 helper 已删除。相同 1000 AI 基线的 `StageBounds` average/P95 为 `1.3871/1.5043 ms`，两轮候选分别为 `0.4548/0.6042 ms` 与 `0.4676/0.5810 ms`，目标 pass 稳定改善约 `66%～67%`；总 tick average 分别由基线 `23.9546 ms` 降至 `23.1783`、`23.2743 ms`，但 max 受 Editor 尖峰影响，不把单个尖峰用于晋升。三份报告 hash 完全一致、正式 sampled tick 0 B、Gen0/1/2 collection 0、teardown 完整恢复。最新代码状态下 Unity 聚焦 job `f62bc840a0154ffcaad65fafa0af1d11` 为 `9/9 PASS`，`BattleRuntimeSelfCheck` 为 `2026-08-13 11:15:58 PASS`；通过在同一持久 UnityMCP 会话内启动并轮询测试，已排除客户端提前断开造成的 `NetworkStream` 日志污染，最新完整 EditMode job `a02648fc844e4c0dbcd48ef0d55fdb28` 为 `1078/1078 PASS`。本切片不关闭 U6/U9；下一步继续审计 `CharacterInput / CandidateCollect / LateEntityUpdate / FrameAdvance` 中占比足够大的完整字段簇与对象 shell 热循环。
> U6 第二十八切片关闭了正式 `FrameAdvance` 中 exact `LF2Character` 的冗余宽 Runtime 快照：权威 C# 的正式帧推进按 slot 升序清理当前按键并执行 FrameAdvance，不包含 Unity 兼容层的整对象镜像；Unity 的 Frame、transition、health、motion 与计数字段现已由各 canonical writer 在写入点同步，因此 exact 正式角色不再在 `SimTransit + SimTU` 后重复读取整对象。未知派生角色仍调用虚拟 `RefreshRuntimeSnapshot()`，调用位置、slot 顺序和早退边界不变。相同基线下 `FrameAdvance/RefreshRuntimeSnapshot` average 从 `0.6151 ms` 稳定降至三轮 `0.0553/0.0574/0.0566 ms`，约减少 `91%`；完整 `FrameAdvance` 从 `2.5911 ms` 降至 `1.8919/1.9589/1.9055 ms`，约减少 `24%～27%`。三轮总 tick average/P95 分别为 `22.4282/27.0139`、`22.9794/29.1838`、`23.2752/28.7866 ms`；第三轮其他 pass 同步变慢令总 average 回到基线附近，因此只声明目标子段与完整 FrameAdvance 的稳定收益，不扩大成稳定整体 FPS 提升。三轮 parity/lockstep hash 完全一致、正式 sampled tick 0 B、Gen0/1/2 collection 0、teardown 完整恢复。新聚焦测试 `3/3 PASS`，完整 EditMode `1078/1078 PASS`，`BattleRuntimeSelfCheck` 为 `2026-08-13 11:38:18 PASS`，本地 runtime/editor 程序集均为 0 error。本切片仍不关闭 U6/U9；下一步优先审计 fresh detail timing 中的 `CandidateCollect/ParticipantBodyItrBuild`，同时保留 `CharacterInput` 与 `LateEntityUpdate` 为后续候选。
> U6 第二十九切片继续按权威职责窄化 `CollisionSnapshot`：权威 C# `SnapshotPrevFrame2` 只按 slot 升序把当前 frame 冻结到 `PrevFrame2`；Unity 的 `CaptureCollisionFrameSnapshot()` 已同时写入 `Frame.Prev2/Prev2D` 与 `Runtime.PrevFrame2`，因此 exact `LF2Character` 不再紧接着执行整对象 `RefreshRuntimeSnapshot()`，未知派生角色仍通过虚调用兼容回退。相同 seed、30 warmup + 180 sample 的基线 `CollisionSnapshot` average/P95 为 `0.7134/0.8614 ms`，三轮候选为 `0.2495/0.3176`、`0.2333/0.2448`、`0.2375/0.2700 ms`，目标 pass 稳定改善约 `65%～67%`；三轮总 tick average/P95 为 `23.2216/29.8168`、`22.2703/27.4394`、`22.2107/26.3527 ms`。全部有效运行均使用 `data-oriented-canonical`，parity/lockstep hash 一致、正式 tick 0 B、Gen0/1/2 collection 0、teardown 完整恢复。新增聚焦测试 `3/3 PASS`，本地 runtime/editor 编译 0 error，`BattleRuntimeSelfCheck` 为 `2026-08-13 12:10:21 PASS`。两次完整 EditMode job 均执行完 `1081/1081`，但各自被 UnityMCP 随机注入的同一条 `NetworkStream disposed` Error 污染而判失败；两项被污染测试分别单独重跑 `1/1 PASS`，因此此处诚实记录为“代码断言无已知失败、完整 job 仍受 MCP 日志基础设施阻塞”，不写成干净全量 PASS。本切片仍不关闭 U6/U9；下一步回到 `CandidateCollect/ParticipantBodyItrBuild` 的 canonical 产品与热循环审计。
> U6 第三十切片把 role-aware formal collector 的参与者暂存收敛为单一可复用值类型缓冲：原路径每 tick 先写 `List<RoleAwareFormalParticipant>`，再把约 1000 个宽结构体 `CopyTo` 第二个数组供 pair exact loop 读取，并在结束时清空第二份引用数组；新路径直接在同一连续数组中构建并以 `ref readonly` 消费，只在 roster 缩小时清理失效尾段，slot 顺序、role 标记、几何、pair 排序、双向消费、RNG 与 fail-closed 边界不变。相邻基线两轮 total/CandidateCollect average 为 `22.2703/3.819`、`22.2107/3.840 ms`；三轮有效候选为 `23.4850/3.956`、`21.9110/3.754`、`21.8490/3.744 ms`，中位数约从 `22.2405/3.8295` 降至 `21.9110/3.7540 ms`，但 CandidateCollect P95 中位数由约 `7.751` 升至 `7.995 ms`，因此只确认 O(N) 重复复制和第二份存储已删除，不宣称稳定 FPS 或尾延迟提升。三轮有效候选均为 180/180 sample、正式 tick 0 B、Gen0/1/2 collection 0、parity/lockstep hash 与基线完全一致、teardown `restored=true`；一份 178/180 且 `InterruptedWithResidue` 的 Editor 生命周期中断报告已明确排除。fresh runtime/editor 编译 0 error，聚焦 job `a52ed5b8709f441db152e15832e46eac` 为 `3/3 PASS`、formal collector job `2e90b82192f1456cabc6f3fdec0589f2` 为 `56/56 PASS`、slot-map 交叉 job `d1418163384041cca715ba1455298a98` 为 `2/2 PASS`，self-check `2026-08-13 12:30:49 PASS`。完整 EditMode job `f099ded58b0e42bcb644f1ab288e65ac` 执行完 `1084/1084`，仍只被 UnityMCP 的 `NetworkStream disposed` Error 污染；对应测试独立 job `a67b46d2feab437298f171460d321a03` 为 `1/1 PASS`。本切片不关闭 U6/U9；下一步审计 CandidateCollect 的 canonical frame/motion 几何产品，以及当前占比最大的 `CharacterInput/EntityInputPass`。
> U6 第三十二切片核对了权威 C# `InputRuntime.ApplyCharacterInput` 的组合技、direct-frame 与 release-action 顺序，并用临时细分计时证明 `ComboUpdate` 的真实工作约 `1.05 ms/tick`，不存在可直接删除的隐藏多毫秒循环；临时计时代码随后完整移除。正式候选复用 UnifiedAuthority 已发布的 immutable row 所有权：当前 AI 的 snapshot 在同步 value-only kernel 调用期间不可能改变 occupancy、generation 或 selected handle，因此 capture 使用同一 published state，commit 不再逐 AI 重复扫描同一 generation/identity/index contract。两轮 1000 AI 中 `IndexedCanonicalCommitValidation` average 从 `0.1320` 降至 `0.0698/0.0660 ms`，capture 从 `0.4401` 降至 `0.4281/0.4206 ms`；总 tick 为 `21.8950/22.8095 ms`，没有稳定 FPS 收益，故只按所有权闭合与目标子段正收益保留。两轮 parity/lockstep hash 一致、正式 tick 0 B、Gen0/1/2 collection 0、fallback/hard breach 0、teardown 完整恢复；本地 runtime/editor 编译 0 error，聚焦 `85/85 PASS`，完整 EditMode job `3638b05e64e64de9a2ea3ca8001a4733` 为 `1090/1090 PASS`，self-check `2026-08-13 14:22:38 PASS`。U6/U9 仍未完成；下一批转向 `LateEntityUpdate` 与 presentation publish 中能删除整段既有工作的候选，不把微小校验削减扩大为整体帧率结论。
> U6 第三十三切片闭合了 `LateEntityUpdate` 的最终 runtime 边界。权威 C# `RunLateEntityUpdate` 直接在同一实体真值上完成 state special、恢复、frame tick、死亡/opoint、清理与 transition，不包含 Unity 适配层的整对象回拷；Unity 的 exact `LF2Character` 字段已经由 Runtime 别名、绑定的 health/frame/transition writer 与 identity writer 在写入点同步。正式 `ConsolidatedFinal` 因此只在 exact 类型且最小非别名字段仍一致时跳过最终宽快照，未知派生类型、陈旧字段与 `LegacyThree` oracle 全部 fail-closed 保留。两轮 1000 AI 中最终快照调用由每轮 `180000` 降至 `0`，`LateEntityUpdate/TailAndQueuedFlush` average 从相邻两轮 `0.788/0.832 ms` 降至 `0.331/0.324 ms`，完整 `LateEntityUpdate` 从 `3.120/3.267 ms` 降至 `2.545/2.513 ms`；total average/P95 为 `21.0483/25.4053` 与 `21.0350/26.1610 ms`。两轮 parity/lockstep hash 完全一致、正式 tick 0 B、Gen0/1/2 collection 0、teardown 完整恢复。顺序本地编译 0 error、聚焦 `25/25 PASS`、self-check `2026-08-13 14:43:32 PASS`；完整 EditMode job 执行完 `1093/1093`，唯一失败是 UnityMCP 注入的 `NetworkStream disposed` Error，被污染测试独立重跑 `1/1 PASS`，因此诚实记录为“无已知代码断言失败，但 fresh 全量 job 仍受 MCP 日志污染”，不写成干净全量 PASS。U6/U9 仍未完成；下一批审计中央表现链 `CaptureEntities -> SortEntities -> BuildCommands -> Publish` 的重复产品与跨逻辑/渲染帧所有权。
> U6 第三十四切片先闭合了中央表现顺序的重复宽结构体排序。正式 `CentralOnly` 捕获由 `SimulationWorld.GetPresentationEntitiesNoAlloc` 按 runtime slot 升序产生，活动 slot 唯一，因此输入已经满足同 Z 下的 `(RuntimeSlot, StableId)` 次序；`MaterializePresentationOrder` 现只对轻量索引执行保留原相对次序的 4-pass signed-Z radix sort，再线性重排宽快照。若输入 slot/stable-id 次序不满足此前置条件，则 fail-closed 回退到原 `Array.Sort + PresentationSnapshotComparer`，自定义帧与测试帧的通用语义不变。相邻两轮旧实现的 `BeginFrame/SortEntities` average/P95 约为 `2.10/2.48 ms`；两轮 1000 AI 候选分别为 `0.5241/0.5370 ms` 与 `0.5331/0.6165 ms`，目标子段平均约减少 `75%`，`PresentationPublish/Total` average 由约 `8.42 ms` 降至约 `6.80 ms`。两轮 total average/P95 分别为 `20.9898/25.8899 ms` 与 `20.9893/26.2584 ms`，因此不宣称整体 FPS 已明显提升；当前 `BeginFrame/BuildCommands` 仍约 `2.55 ms`，是下一表现候选。两轮均为 1000 个真实生产 AI、180/180 sampled tick，parity hash `752b4907...b35`、lockstep hash `4378ba4c...7063` 完全一致，正式 tick average/max allocation `0/0 B`、Gen0/1/2 collection `0/0/0`、teardown `restored=true`。本地 runtime/editor 编译 0 error，聚焦 `10/10 PASS`、扩大聚焦 `28/28 PASS`，self-check `2026-08-13 15:13:35 PASS`；完整 EditMode job 执行完 `1094/1094`，唯一失败仍是 UnityMCP 注入的 `NetworkStream disposed` Error，对应测试独立重跑 `1/1 PASS`。本切片只改变表现快照物化成本，不改变权威 C# pass、战斗真值或逻辑时序，也不关闭 U6/U9。
> U6 第三十五切片继续删除中央表现命令链的重复产品。诊断先把旧 `BuildCommands` 的约 `2.60 ms` 精确拆为 `ResolveDeferredSpriteCaptures 0.7259 ms` 与命令本体 `1.8758 ms`。首个“融合但仍回写宽快照”的候选使目标阶段退化到 `2.7318 ms`，已按负优化结论舍弃；正式保留版令 `CentralOnly` 在同一实体循环中解析 sprite，并把解析结果直接写入 `BattleRenderCommand`，不再为 1000 个实体重建、写回整份冻结快照。冻结逻辑 publication 仍不可变，最终命令的 sprite key、尺寸、UV、pivot 与 catalog publication binding 契约由新测试覆盖。两轮 1000 AI 中 `BuildCommands` average/P95 由基线 `2.6039/3.3952 ms` 降至 `2.2808/3.0402 ms` 与 `2.3193/2.7762 ms`；sprite 子段由 `0.7259/0.9179 ms` 降至 `0.2915/0.4095 ms` 与 `0.3091/0.3954 ms`，average 稳定减少约 `57%～60%`；`PresentationPublish/Total` 为 `6.6284`、`6.7122 ms`。两轮 total average/P95 为 `21.1970/26.2597` 与 `20.8252/25.5468 ms`，但第一轮 max 有 `46.7816 ms` Editor 尖峰，因此仍只按目标子段晋升。两轮 parity/lockstep hash 与基线一致，逻辑 tick、driver update、presentation 三条 managed-memory 边界均 `0 B`，Gen0/1/2 collection `0/0/0`，harness/authority 有效且 teardown `restored=true`。本地 runtime/editor 顺序编译 0 error、扩大聚焦 `287/287 PASS`、self-check `2026-08-13 15:59:33 PASS`；完整 EditMode 执行 `1095/1095`，唯一失败仍为 MCP `NetworkStream disposed` 日志污染，被污染测试独立 `1/1 PASS`。当前最新逻辑热点仍为 `CharacterInput 5.4925 ms`、`CandidateCollect 3.7415 ms`、`LateEntityUpdate 2.5017 ms` 与 `FrameAdvance 1.8976 ms`；Unity frame average/P95 为 `41.94/46.00 ms`，所以 1000 AI / 30 FPS 与 U6/U9 仍未完成。
> U6 第三十六切片把权威 `InputRuntime.ApplyCharacterInput -> ApplyFrameVelocityTail` 的角色帧速度尾链收口到 world-owned `BattleCharacterActionWriter`。正式注册且精确类型为 `LF2Character` 时，writer 按权威顺序应用 DVX 阈值/方向、DVY 累加/绝对值与 DVZ 方向键冲突规则；未注册、未知派生类型与共享 character-DAT shell 继续走原虚方法兼容链，没有删除 transform 后壳类的读键副作用。该切片是输入/动作写入所有权迁移，不是一项已证明的帧率优化。新增聚焦用例覆盖权威速度矩阵、预热后 0 B 与派生类型 fail-closed；最终代码状态下本地 runtime/editor 编译 0 error，聚焦 job `c5555fc04c6a4ad09cdc5cc51c916cc8` 为 `28/28 PASS`，完整 EditMode job `fe030cca31614363aedf64483aab0cb6` 为干净的 `1100/1100 PASS`，`BattleRuntimeSelfCheck` 于 `2026-08-13 19:46:01` fresh PASS。最终 1000 AI 报告 `Temp/NTSD_ProductionEntityStress.combat1000.u6-action-writer-frame-velocity-final-20260813.json` 为 30 warmup + 300 sample，logic average/P95/P99/max `18.6504/23.7142/25.8144/27.4020 ms`，Unity frame average/P95 `33.9323/39.6413 ms`，正式 tick `0 B`、Gen0/1/2 collection `0/0/0`、parity/lockstep hash 与相邻基线完全一致、teardown `restored=true`。相邻基线 logic average/P95 为 `17.9637/22.7936 ms`，因此只声明所有权收口与行为等价，不声明性能提升；U6/U9 仍未完成。
> 战斗逻辑唯一权威：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live path；`ntsd_release_C#` 为历史辅助来源。
> 当前实现目标：先完成单机确定性闭环与 1000 AI / 30 FPS，再接入服务器
> 排除项：T8 默认 `stage.dat` 部署、Android 真机验收

## 1. 文档定位与替代关系

本文是以下工作的统一总方案：

- 固定 30 Hz 的单机与未来网络帧同步；
- NTSD 专用的自研数据导向 ECS 战斗内核；
- 1000 个真实生产 AI 的 CPU、GC、碰撞和表现性能治理；
- 中央战斗表现与逻辑真值分离；
- 未来无表现服务器复用同一战斗内核；
- 可重放、可校验、可快照恢复的确定性运行时。

`Assets/NTSD/NTSD_Lockstep_Framework_Plan.md` 是旧计划，不再作为实施依据。现有文档继续承担专项记录职责：

| 文档 | 保留职责 |
|---|---|
| `Docs/singleplayer-1000ai-performance-plan.md` | 性能基线、Profiler 归因和 1000 AI 验收证据 |
| `Docs/battle-runtime-zero-gc-architecture-plan.md` | 零 GC、池、容器、static 与 partial 治理证据 |
| `Docs/central-battle-render-system-plan.md` | 中央表现、Texture2DArray、Mesh、排序与 SetPass 证据 |
| `Docs/future-server-lockstep-architecture.md` | 历史服务器架构备忘与背景资料；不再是 S0～S9 的当前详细实施合同 |
| `Docs/server-lockstep-s0-s9-design.md` | S0～S9 的唯一详细设计合同、每阶段解决方案、故障边界与关闭证据 |
| `Docs/server-lockstep-s0-s9-progress.md` | S0～S9 的当前状态、证据、阻塞、问题台账和实际 Change ID 链接 |
| `Docs/lockstep-knowledge-base-audit.md` | `网络游戏` 知识库来源覆盖、取舍和拒绝项 |
| `.omc/plans/battle-kernel-ecs-lockstep-migration-20260727.md` | 既有 ECS shadow 实验、历史基线和未关闭合同 |
| `Docs/csharp-vs-unity-battle-alignment.md` | 历史 C# 移植辅助、命名交叉检查与 Unity 差异记录；不再定义规则 authority |
| `Docs/HANDOFF-codex-battle-alignment.md` | 当前实现进度与后续交接 |

发生冲突时按以下优先级处理：

1. 用户当前明确要求；
2. C++ release live runtime 的实际调用链和同 tick trace；
3. 本文确定的架构边界和实施顺序；
4. 各专项文档的测量证据；
5. `ntsd_release_C#` 的历史辅助交叉检查；
6. 历史计划和旧结论。

### 1.1 ECS 知识库取舍依据

本方案复核了以下本地知识库：

| 资料 | 采用内容 | 不直接照搬的内容 |
|---|---|---|
| `I:\GitHub\ZhiHu_MD\output\Unity引擎\ECS、物理与性能优化` | 连续 struct 数据、逻辑/表现延后一帧解耦、控制并行线程数、Transform 每渲染帧统一同步 | Unity Entities、Chunk、Burst 和 Job 不是本项目依赖前提 |
| `I:\GitHub\ZhiHu_MD\output\游戏架构\ECS与数据导向设计` | Entity/Component/System 分离、generation 身份、bitset 签名、World/Coordinator 所有权 | 热路径不使用 `unordered_map`/`set`/虚调用式通用组件管理 |
| `I:\GitHub\ZhiHu_MD\output\游戏开发中的 ECS 框架_323304` | Direct Array、SoA、Sparse Set、Bitset、Command Buffer、Pipeline 和结构变化成本分析 | 不把 Archetype/Chunk 当作所有项目的唯一正确方案，不因 ECS 名称默认获得性能 |

综合结论是：ECS 的实际收益来自数据布局和访问路径，不来自类型命名。NTSD 的槽位顺序和结构可见时点具有规则语义，因此采用专用混合存储，并用权威 pass boundary 约束命令播放。

### 1.2 帧同步知识库审计依据

本方案已审计 `I:\GitHub\ZhiHu_MD\output\网络游戏` 全部子目录：96 个 Markdown、24 份正文；正文去除 3 组字节级重复并合并 2 组同文变体后，共 19 个独立主题。完整来源、重复关系、采用项、适配项和拒绝项见 `Docs/lockstep-knowledge-base-audit.md`。

审计后确定的总原则是：

1. NTSD 正常战斗只同步并授权输入，结果由同一 BattleKernel 计算；
2. 服务器运行同一套 C# BattleKernel 实现，但其伤害、命中、生命周期和 pass 规则必须由 C++ release live runtime 约束，不另写第二套规则；
3. 状态同步只用于 bootstrap、快照恢复、晚加入、观战和诊断，不在正常 tick 覆盖 HP/位置；
4. 逻辑 30 Hz、网络组包频率和 Unity 渲染频率相互独立；
5. 权威帧一旦锁定不可修改，重复包只允许幂等接受；
6. Jitter Buffer、缺帧、追帧和恢复必须由显式状态机管理；
7. 预表现只能表达输入与意图，扣血、命中、opoint、控制和死亡只来自确认逻辑；
8. ECS 解决数据布局与批量处理，不自动解决确定性、错误复杂度、网络抖动或持续超预算。

知识库中的示例帧率、缓冲长度、transport、定点库和性能阈值均不是 NTSD 生产常量。任何与 C++ release live runtime 行为冲突的网络文章结论只记录为拒绝项，不进入实现。

当前工作树已经存在尚未统一验收的输入、host policy、AI、碰撞、零 GC 和表现优化。它们全部视为“候选实现”，进入第一个基线审计阶段；本文不假定这些修改已经完成，也不授权覆盖或回退用户改动。

## 2. 最终目标

最终只保留一套战斗规则实现：

```text
单机本地输入 ─┐
回放输入 ─────┼─> Canonical FrameInputSet
服务器权威帧 ─┘             │
                             v
                   Shared Battle Kernel
                   固定 30 Hz / 纯 C#
                   自研 ECS / 确定性 RNG
                             │
                 ┌───────────┼───────────┐
                 v           v           v
              Checksum   状态快照    表现观察/事件
                                         │
                              ┌──────────┴─────────┐
                              v                    v
                         Unity Client         Headless Server
                         渲染/音频/UI          房间/协议/恢复
```

必须达到的结果：

1. 单机、回放、客户端和服务器都调用同一个 `StepOneTick(FrameInputSet)` 战斗入口。
2. C++ release live runtime 的规则、pass 顺序、slot 顺序、RNG、opoint 可见边界和可观察结果保持不变。
3. 战斗真值由纯 C# 数据世界持有，不依赖 Unity Transform、Physics、Renderer、Time 或异步资源完成顺序。
4. 高频战斗循环使用连续数据、预分配容器和显式顺序，不在正式战斗窗口产生托管分配。
5. Unity 只负责输入采样、资源、GameObject 壳、中央表现、音频和编辑器接线。
6. 服务器可以在普通 .NET 或后续选定的 headless host 中复用战斗内核，不依赖表现程序集。
7. 1000 个真实生产 AI 的 `Dispersed1000` 与 `Combat1000` 达到 30 Hz / 30 FPS 正式门禁。
8. 逻辑 30 Hz 与表现 60/90/120 Hz 解耦；提高显示帧率不改变战斗规则。

## 3. 明确非目标

本文不要求：

- 使用 Unity Entities、DOTS 或 Burst；
- 把整个 Unity 项目改成 ECS；
- 当前立即实现真实网络、匹配、登录、NAT、反作弊或跨服；
- 当前立即实现客户端预测、GGPO 风格回滚或观战；
- 为性能降低 AI 频率、跳过有效碰撞、限制真实命中或修改 DAT 数值；
- 当前处理 T8 默认 `stage.dat`；
- 当前完成 Android 真机验收；
- 在没有跨运行时分叉证据时一次性把全部 `double` 改成定点数。

ECS 只进入战斗运行时和与战斗时序直接相关的表现发布边界。菜单、角色选择、普通 HUD 和通用资源工具继续使用适合 Unity 的现有结构。

## 4. 核心架构决策

### 4.1 采用 NTSD 专用混合 ECS，不采用通用 Archetype ECS

主存储采用：

```text
固定/分页 Slot 域
  + 直接索引 SoA
  + Presence/Tag Bitset
  + 少量 Sparse Set
  + 预分配 Ring/Queue/Pool
  + 确定性空间索引
```

不以 Archetype/Chunk 迁移作为主模型，原因是：

1. 权威 C# 的 runtime slot 升序遍历和复用时机属于战斗规则。
2. NTSD 的 state、frame、oid、link、holder、target 和对象生命周期变化频繁，但这些变化不应被解释为频繁添加/删除组件。
3. Archetype 结构变化会搬移实体，增加快照、排序和同 tick 可见性证明的复杂度。
4. 当前目标规模为 400 权威槽和 1000 扩展槽，直接索引数组的内存成本可控，访问路径更短。
5. 未来服务器需要纯 C# 可移植性，首期不应把共享核心绑定到 Unity Collections 或 Entities。

该方案仍然属于 ECS：Entity 是身份，Component Store 保存纯数据，System 批量处理数据；只是它是针对固定战斗域优化的 ECS，不是通用引擎级 ECS。

### 4.2 新内核使用组合，不新增 partial 和全局可变 static

新内核遵循：

```text
BattleKernel
  -> BattleWorld
  -> Entity/Data Stores
  -> BattlePipeline
  -> World-scoped Systems
  -> Command/Event Buffers
  -> Snapshot/Checksum Services
```

- `BattleKernel`/主类持有普通 module/system 实例引用。
- 新代码不新增 `partial` 类型或 `.partial.cs` 文件。
- 不使用全局可变 singleton 保存当前 World、RNG、当前帧、相机、表现 generation 或测试 override。
- 允许保留编译期常量、只读表、ProfilerMarker 和无状态纯函数 `static`。
- System 可以拥有预分配 scratch 和只属于当前世界的缓存；影响战斗结果的状态必须进入 BattleWorld 或可快照 World Resource。
- 固定 pipeline 直接调用具体 system，不在热路径使用反射、`Dictionary<Type, object>`、虚拟查询调度或每帧生成委托。

### 4.3 逻辑单线程确定性优先，表现和多房间并行

第一阶段每个 BattleWorld 内部保持单线程顺序执行。这样最容易证明：

- slot 顺序；
- RNG 调用次数；
- candidate 和 hit 消费顺序；
- opoint 同 pass 可见性；
- snapshot/checksum 一致性。

并行化顺序：

1. 先消除 GC、对象图遍历、重复快照和错误复杂度；
2. 再把 Unity 表现与 BattleKernel 分线程/分帧；
3. 最后只并行只读收集或写入互不重叠输出区间的 kernel；
4. 所有并行结果按稳定 slot/pair ordinal 确定性合并；
5. 服务器优先并行不同房间，不在同一房间内无证据并行写世界。

## 5. Entity、容量与身份

### 5.1 Entity 身份

逻辑 Entity 使用：

```text
EntityHandle
  slot        // 当前 world 内的直接数组索引
  generation  // slot 每次复用递增，拒绝过期引用
```

以下身份不得混用：

- `slot`：当前运行时位置和权威扫描顺序；
- `generation`：slot 复用安全；
- `stableId`：跨 tick 的逻辑事件和诊断身份；
- `oid`：DAT 对象类型；
- presentation handle：Unity 表现对象身份，只存在于客户端。

holder、target、owner、parent、attacker 等跨实体引用使用 generation-aware handle，不能只保存 slot 后长期信任。

### 5.2 容量 Profile

至少保留两个 profile：

| Profile | 用途 | 容量规则 |
|---|---|---|
| `Authority400` | 与权威 C# 逐 tick 对照 | 保持权威槽域、起始搜索和复用语义 |
| `Extended1000` | 移动端和 1000 AI 正式压力 | 战前封印至少 1000 active 的全部相关容量 |

桌面端不使用编译期“最多 1000”硬限制，而使用分页容量：

- 战斗准备阶段根据本局 profile 预留页；
- 房间开始后容量配置进入 session fingerprint；
- 正式 tick 内禁止托管数组自动扩容；
- 超过已封印容量时产生确定性的 capacity fault 和结构化计数，不允许静默 `new`、漏生成或不同端产生不同结果；
- 后续对局可以选择更大 profile，不需要重新编译。

“无固定编译上限”不等于无限内存。每一局仍必须有确定、可预热、可同步的容量合同。

### 5.3 Slot 分配

不能使用覆盖所有槽位的单一全局最小堆。allocator 必须知道：

- 权威槽域；
- 本次搜索起点；
- 当前 pass 游标；
- 延迟释放与可见边界；
- generation 更新时点。

底层可以组合使用分段最小堆、分层位图和分页 free list，但返回结果必须与权威 C# 的最低合法槽选择一致。

## 6. 数据存储方案

### 6.1 直接 SoA：高频、广泛存在的数据

以下领域使用按 slot 直接索引的连续数组或分页数组：

- Identity：active、generation、stableId、oid、kind、team、owner；
- Motion：X/Y/Z、Vx/Vy/Vz、facing；
- Frame：frameId、state、wait、next、prevFrame；
- Vital/Stats：HP、PP、MP、fall、defend、kill/combo/damage stats；
- Input：held、pressed、released、buffer/history、AI output；
- Links：holder、target、catching、parent、attacker；
- Lifecycle：pending spawn/free/unregister、first visible tick、dormant；
- 高频 collision/runtime flags。

优先使用普通预分配 `T[]`、分页数组和 `Span<T>`，保持纯 .NET 可移植。是否引入 unmanaged backend 必须由 Player/服务器 profile 证明，不在首期预设。

### 6.2 Bitset：存在性、标签和升序查询

Bitset 用于：

- alive/active/pending/dormant；
- character/weapon/projectile/effect；
- has body/has itr/has AI/has holder；
- dirty/presentation visible；
- pass membership。

权威顺序敏感的 pass 必须按 slot 升序扫描 bitset；不能因为 sparse set 的 swap-remove 改变执行顺序。

### 6.3 Sparse Set：真正可选且中低密度的数据

Sparse Set 只用于：

- 少量实体才拥有的数据；
- add/remove 不频繁；
- 或执行顺序与密集数组内部顺序无关的数据。

若结果影响战斗顺序，必须按 slot/ordinal 稳定化后消费，不能直接依赖 dense 数组当前排列。

### 6.4 固定池、环形缓冲和索引链

| 数据模式 | 结构 |
|---|---|
| 输入历史、表现事件、声音事件、回放帧 | 固定环形缓冲 |
| opoint、spawn/free、pass-boundary 命令 | 分段命令缓冲 |
| hit candidate、pair、排序 scratch | 每世界预分配数组/List，容量封印 |
| 高频 O(1) 插入删除并要求稳定节点 | 预分配节点数组 + 整数 `prev/next` |
| 最低合法 free slot | 分段最小堆/分层位图 |
| 冷路径 key 查找 | 战前定容 Dictionary 或开放寻址表 |

普通 `LinkedList<T>` 不作为默认方案，因为节点是分散的引用对象，遍历不连续并可能产生 GC。只有预分配索引链表才进入战斗热路径。

## 7. System 与权威 Pipeline

System 不是任意调度。`BattlePipeline` 必须固定映射权威 `GameTick.Run` 的顺序：

```text
Tick/瞬时状态开始
  -> Results 分支
  -> Cooldown 与输入边界
  -> OID 51/52 等早期维护
  -> BattleEntry clear gate
  -> CharacterInput
  -> EarlyState
  -> FrameLogic
  -> FrameAdvance
  -> PostFrameAdvance / Stage Z Bounds
  -> CPoint / Held / Link
  -> PrevFrame2 Snapshot
  -> CandidateCollect
  -> CharacterHit
  -> Random/F8 Weapon Drop
  -> ObjectHit
  -> PreFrame Bounds / Stage Advance
  -> Presentation Observation Boundary
  -> FramePostProcess
  -> Late Per Entity Update
  -> Mode2 Weapon Drop
  -> Entity PostFrame Tail
  -> Battle Results Update
```

原则：

- 不为了“ECS 纯度”把每个小函数拆成一个 System；一个 system 可以包含多个紧密相关的连续循环。
- 不为减少循环随意合并在权威 C# 中分开的 pass。
- 只有 profile 证明数据被重复读取且合并不改变观察边界时，才允许合并内部数据准备。
- 每个 system 的 canonical writer、读取集合、scratch 和命令输出必须明确。

## 8. 结构变化与 opoint 可见边界

通用 ECS 的“所有创建/销毁统一到 tick 末”不适用于 NTSD。

结构命令需要携带权威播放边界：

```text
StructuralCommand
  type
  source handle
  target/oid/data
  requested slot domain
  playback boundary
  authority ordinal
```

至少区分：

- 当前实体结束后立即可见；
- 当前 pass 分段结束可见；
- 下一 pass 可见；
- tick 结束后可见；
- 延迟 unregister/free。

当权威 late live-slot 循环要求当前实体产生的高槽 opoint 在同一 pass 后续参与时，必须使用 cursor-local immediate playback；不能强行等到全 tick 结束。

spawn、destroy、free、generation、link invalidation 和对象池 release 的顺序全部进入 checksum 和 self-check 合同。

## 9. 帧同步与宿主策略

### 9.1 唯一逻辑入口

目标接口：

```text
ResetWorld(BattleBootstrap)
StepOneTick(FrameInputSet)
CaptureStateSnapshot()
RestoreStateSnapshot()
ComputeChecksum()
Export/ReplayInputJournal()
```

`BattleBootstrap` 至少包含：

- seed；
- capacity/profile；
- immutable DAT catalog；
- stage runtime snapshot；
- player canonical order；
- catalog/stage/config fingerprint。

BattleKernel 不读取 Unity wall clock、Input API、Transform 或网络回调。

### 9.2 三种本地推进策略

| 模式 | 帧来源 | 推进规则 |
|---|---|---|
| `OfflineLocal` | 本地 canonical collector | wall clock 只决定本可见帧是否执行 0/1 tick；不执行网络追帧 |
| `ManualReplay` | journal/测试/恢复调用方 | 不读取 wall clock，调用方显式逐 tick 推进 |
| `NetworkLockstep` | 连续 ready 的服务器权威帧 | 只有落后服务器目标缓冲时，才按 ready 数量和 CPU 预算有限追帧 |

单机普通 `Update` 不再因为本地累计时间一次执行四个完整 tick。若未来需要单机卡顿恢复，必须建立独立 `LocalHitchRecovery` 策略，不能借用网络帧差语义。

### 9.3 三种频率分离

- 战斗逻辑：固定 30 Hz；
- 网络广播/组包：独立配置，可以 15 Hz 每包携带两个 30 Hz 输入帧；
- Unity 渲染：60/90/120 Hz，读取逻辑快照插值，不写回逻辑。

降低网络发包频率不能合并逻辑帧；提高渲染帧率不能多执行战斗规则。

### 9.4 三种时间与过载定义

必须严格区分：

| 时间 | 含义 | 能否改变战斗规则 |
|---|---|---|
| Logic Time | `tick * SIM_DT` 的离散规则时间 | 只能按固定 30 Hz 逐 tick 前进 |
| Wall Clock | Unity、OS 或服务器现实流逝时间 | 只供 host policy 判断调度和积压 |
| Compute Time | `StepOneTick` 实际 CPU 耗时 | 只用于预算、告警和容量判定 |

某 tick 计算超过 `33.33 ms` 不代表该 tick 的逻辑时间变长，而是本机没有跟上目标逻辑时钟。偶发慢帧可以进入 backlog 并有限恢复；持续慢帧是算法、数据、Unity API、GC 或内容容量失败，不能通过动态 dt、无限追帧或删减规则处理。

### 9.5 网络客户端的四个帧游标

`NetworkLockstep` 至少维护：

```text
latestLocalSampleFrame
highestReceivedServerFrame
highestContiguousReadyFrame
localExecutedFrame
```

- `latestLocalSampleFrame`：已采样并发送的本地未来输入目标帧；
- `highestReceivedServerFrame`：收到过的最高服务器帧，允许中间有洞；
- `highestContiguousReadyFrame`：从 `localExecutedFrame + 1` 开始连续 ready 的最高帧；
- `localExecutedFrame`：BattleKernel 已完成的最后一帧。

追帧只能依据 `highestContiguousReadyFrame`，不能依据收到过的最高帧跳过中间缺口。服务器广播序号、客户端输入序号和战斗 frame id 是不同维度，也不得混用。

### 9.6 Jitter Buffer 状态机

权威帧缓冲不是一个普通 Queue，而是以下会话状态的一部分：

```text
Priming
  -> Running
  -> WaitingForGap
  -> CatchingUp
  -> RecoveringSnapshot
  -> Running
  -> Faulted / Ending
```

- `Priming`：积累目标缓冲深度，不能刚收到第一帧就反复执行和等待；
- `Running`：维持目标缓冲，通常每个可见帧执行 0/1 tick；
- `WaitingForGap`：下一连续帧缺失，等待或补发，禁止跳过；
- `CatchingUp`：连续权威帧已经 ready 且本地落后目标缓冲，有限追帧；
- `RecoveringSnapshot`：落后超出历史窗口、预计追赶代价过高或 checksum 分叉，恢复权威快照并重放；
- `Faulted`：协议冲突、资源指纹不一致或恢复失败，显式停止，不在已知错误世界继续运行。

目标缓冲帧数允许根据会话握手时的网络配置选择，但运行中不得高频抖动调整。所有切换只影响“何时执行已有权威帧”，不改变帧内容、`SIM_DT`、RNG 和 pass 顺序。

### 9.7 有限追帧与容量失败出口

网络追帧同时受三个上限约束：

1. 本地实际落后的连续 ready 帧数；
2. 每个可见帧最多可执行的 catch-up tick 数；
3. 本次主循环允许消费的 CPU 时间预算。

中间追帧 tick 完整执行战斗规则，但可不构建 Sprite、Mesh、UI 和音频表现；最后可见 tick 发布完整表现。若 backlog 连续增长、长时间处于 catch-up 或超出 `FrameHistoryRing` 覆盖范围，必须进入快照恢复或容量失败处理，不允许进入死亡螺旋。

## 10. 输入事实源

Unity 输入回调只采集意图，不直接改变角色 runtime。

每个逻辑 tick 的输入必须形成完整、可记录的：

```text
FrameInputSet
  tick
  canonical player order
  held buttons
  pressed edges
  released edges
```

单机、回放和网络的差别只在 FrameInputSet 的授权方：

- 单机：`LocalFrameInputCollector`；
- 回放：`ReplayInputSource`；
- 未来客户端：服务器 `AuthoritativeFrameBundle`；
- 未来服务器：`AuthoritativeFrameAssembler`。

AI 不是网络输入。AI 在相同 BattleWorld、seed、输入和顺序下由内核确定性计算；客户端不能发送 AI 的位置、伤害或最终决策作为战斗真值。

### 10.1 输入必须表达完整事实

`FrameInputSet` 不能只保存“这一帧有哪些 key-down 事件”。对每个 canonical player slot，至少要能重建：

- 当前完整 held bitset；
- 本 tick pressed edges；
- 本 tick released edges；
- 量化后的方向、目标或技能附加参数；
- 是否由真实玩家输入、确定性缺失输入规则或服务器托管产生；
- 输入 schema/version 和内容 hash。

组合键窗口、按住、松开和边沿消费仍按权威 C# 顺序执行。网络层只量化和封装输入，不能为了节省字段重定义 C# 输入语义。

### 10.2 权威帧不可变合同

每个 `(sessionId, frameId, playerSlot)` 只能产生一个权威输入：

1. 首次合法输入写入尚未锁定的 `ServerInputInbox`；
2. 内容完全相同的重复包幂等接受；
3. 同一键出现不同内容时记为 protocol conflict 并拒绝，不能以后到覆盖先到；
4. 到统一 frame deadline 后，服务器按房间规则补齐并锁定完整 `FrameInputSet`；
5. 锁定、模拟或广播之后的迟到输入不得修改该帧；
6. 权威帧内容、补齐原因和输入来源写入 `FrameHistoryRing`，成为 checksum、重连和回放共同事实。

客户端对重复 `AuthoritativeFrameBundle` 采用同样规则：同帧同内容幂等去重，同帧不同内容立即报错并停止晋升连续 ready 边界。

### 10.3 缺失输入是服务器会话规则

首个同进程原型可以严格等待全部玩家输入，用于验证协议和 checksum；生产模式不能永久被最慢连接无界拖住。候选规则为：

- 在短 grace 内沿用上一帧 held，pressed/released edges 置零；
- 超过 grace 后切 neutral；
- 到固定断线阈值后切服务器 AI 托管或结束连接。

具体规则和阈值暂不写死，但最终必须由 `BattleRoomSession` 在 StartBarrier 固定，进入 session fingerprint、权威输入历史和回放。客户端不能自行决定“沿用、neutral、预测或等待”。

## 11. AI 与空间查询

ECS 数据布局只能降低遍历成本，不能自动消除错误算法复杂度。

AI 目标查询采用：

1. 每 tick 按 slot 升序建立或确定性更新空间索引；
2. 使用 Loose Quadtree 作为 2D 战斗空间的 broad query；
3. 预建角色、队伍、特殊 OID 等只读索引；
4. 每个 AI 从局部候选中按权威距离和 tie-break 选择目标；
5. query 结果按稳定 slot/ordinal 消费；
6. 空间索引是可重建派生缓存，不把节点布局当作战斗身份；
7. AI 最终 target、输入和 RNG 结果进入 checksum。

不得：

- 每个 AI 扫描全部 runtime slot；
- 按有限半径删除权威本应检查的特殊对象；
- 因候选已满提前终止会改变结果的扫描；
- 以降频或跳过 AI tick 冒充性能优化。

## 12. Collision 与交互

碰撞广阶段采用 role-aware 空间结构：

- body bounds 进入可被攻击索引；
- attack itr 查询 body；
- 不为 body-body、itr-itr 或没有有效 role 的组合制造无用 pair；
- role/bounds 无法证明时走等价 fallback；
- broadphase 只减少不可能相交的 pair，不能删除真实有效 pair；
- 最终 pair/candidate 按权威 ordinal 排序和消费；
- A 攻击 B 与 B 攻击 A 的方向检查按 C++ release live runtime 保留。

`Concentrated1000` 若确实产生 499,500 个真实有效实体对，任何 ECS 或四叉树都不能把真实输出工作量变成 O(N)。该场景用于报告极限复杂度，不在未改变玩法合同的情况下预先承诺 30 FPS。

## 13. 表现发布边界

逻辑状态快照、表现观察和正式恢复快照是三种不同数据：

1. **逻辑状态**：BattleWorld 的 canonical state；
2. **表现观察**：在权威 `prePostprocessRender` 对应时点发布的纯数据画面输入；
3. **恢复快照**：完整 tick 正式边界上的可序列化世界状态。

表现方案：

```text
BattleKernel
  -> 在准确观察边界写入预分配 BattlePresentationSnapshot
  -> 始终写入确定性的 BattleEventJournal

Unity Presentation Host
  -> 每个渲染帧读取最新已发布 snapshot
  -> 解析 sprite/material/texture page
  -> 排序并 BuildCommands 一次
  -> 更新 central mesh / UI / audio
```

约束：

- `BuildCommands`、Sprite/Material 查找和 Mesh 上传不再属于逻辑 tick 成本；
- 追帧中间 tick 可以不物化完整画面，但必须保存逻辑事件；
- 最后可见 tick 必须发布准确画面；
- 正式事件使用稳定事件键去重，避免重放、追帧或恢复重复声音和特效；
- Unity Transform 每个渲染帧最多统一刷新一次；
- 表现插值不改变 BattleWorld 的位置、速度、碰撞或 checksum；
- Scene/Game 显示问题不得反向修改逻辑状态。

### 13.1 预表现三层合同

未来网络手感优化必须分层，不能把“预测”理解为提前执行技能：

| 层 | 允许 | 禁止 |
|---|---|---|
| `InputEcho` | 本地按键/UI、瞄准提示、低承诺轻音效 | 修改角色输入缓冲之外的战斗字段 |
| `IntentPresentation` | 可撤销起手、朝向预热、有限显示位置和技能预热 | 扣 HP、正式 opoint、命中、硬直、控制、死亡 |
| `ConfirmedResult` | 消费 BattleKernel 已确认事件，播放正式命中、受击、技能、音效和 UI | 反写逻辑或把 Animator 当状态机 |

首期网络闭环只保证 `InputEcho` 与 `ConfirmedResult`；是否增加本地移动/技能意图预测，要等 U7 可恢复快照和 S2 网络仿真后决定。远端对象以确认快照插值为主，不做与本地玩家同强度的预测。

### 13.2 表现事件稳定身份

每个正式事件至少包含：

```text
sessionId
logicFrame
sourceStableId
eventSequence
eventType
payload
```

稳定键用于处理重复网络包、追帧、回放、快照恢复和未来回滚。可重放事件、仅本地即时反馈和可撤销状态表现分池管理；音频、粒子、镜头和 UI 播放游标不进入 BattleWorld，但恢复后必须用确认事件游标避免重复播放。

## 14. 零 GC 与内存边界

战斗准备完成后执行 capacity seal。正式战斗窗口禁止：

- 新建临时 class、数组、字符串和装箱对象；
- lambda/闭包、LINQ、热路径委托生成；
- List/Dictionary 自动扩容和 rehash；
- 对象池耗尽后 Instantiate 或 new；
- 每 tick 反射、类型扫描、日志格式化和报告序列化；
- 音频首次查找、首次 coroutine 或首次 follower 组件创建。

必须预热：

- entity/task/resolver/GameObject；
- candidate、pair、VRest/aRest；
- AI snapshot 和空间索引节点；
- 输入、事件、sound、opoint 队列；
- presentation snapshot、sort、command、mesh；
- sprite/material/texture page/voice。

正式声明范围：

- NTSD formal battle tick：`0 B GC.Alloc`；
- NTSD driver 与 presentation 稳态：`0 B/frame`；
- Player 战斗窗口：项目战斗链不分配，Gen0/1/2 collection 不增加；
- Editor 只做观察，不能把 Profiler、SceneView、IMGUI 或插件分配算成 Player 正式失败，也不能据此声称整个 Editor 进程永不 GC。

池或容量耗尽必须有结构化 rejection/fault 计数。零 GC 不能以静默丢技能、漏 opoint、漏声音或漏碰撞为代价。

## 15. Snapshot、Checksum 与重放

### 15.1 Checksum

分域 checksum 至少覆盖：

- tick、seed、RNG state 和 call count；
- slots、active/pending/dormant、generation、stableId；
- identity、motion、frame、vital、input、links、combat；
- allocator、rest、stats、stage、battle flow；
- pending structural commands 和 visibility boundary；
- event sequence/cursor；
- catalog/stage/profile fingerprint。

表现对象、Sprite、Mesh、Material、Transform、Camera、音频播放头和 Editor 诊断不得进入逻辑 checksum。

### 15.2 可恢复快照

生产 `BattleStateSnapshot` 必须：

- 有 schema/version；
- 使用固定宽度字段和显式字节序；
- 完整恢复 World、allocator、RNG、输入游标、命令队列和事件游标；
- 不依赖 CLR 对象地址或 Dictionary 枚举顺序；
- 不直接把进程内存块当成跨平台协议；
- 使用复用 writer/buffer，避免采样期间分配。

恢复等价测试：

```text
World A 在 tick N 后继续运行
World B 从 tick N snapshot 恢复
A/B 输入相同的 N+1...M FrameInputSet
逐 tick checksum、RNG、slot、事件和最终世界一致
```

### 15.3 历史、快照与校验的统一生命周期

三种存储不能互相替代：

| 存储 | 内容 | 用途 |
|---|---|---|
| `FrameHistoryRing` | 每一帧不可变权威 `FrameInputSet` | 补发、回放、快照后重放 |
| `SnapshotRing` | 周期性完整 `BattleStateSnapshot` | 重连、严重落后、desync 恢复、观战起点 |
| `ChecksumHistory` | 周期性 overall 与按域 hash | 主动发现分叉并定位域 |

它们必须共享 frame id、session identity、schema 和生命周期。快照覆盖到 S 时，服务器至少保留能够从 S 重放到当前目标帧的完整输入历史；不能只发“当前状态”而丢失 RNG、技能内部状态、待处理结构命令和事件游标。

客户端本地快照可以用于诊断或减少本机重建成本，但服务器不得信任客户端磁盘快照作为权威。正常对局也不得周期性下发位置、HP 或 Buff 覆盖客户端来掩盖分叉；一旦需要状态恢复，必须进入显式 `RecoveringSnapshot` 并完成恢复后 checksum。

### 15.4 跨运行时确定性

首期保留权威 C# 的数值语义。按同一 seed/journal 在以下环境对比：

- Unity Editor Mono；
- Windows IL2CPP Player；
- Android ARM64 IL2CPP（架构兼容项；真机验证由用户后续执行）；
- 未来 .NET/服务器运行时。

若确认某个 `double`/数学域发生分叉，再只迁移该字段域到整数或定点数，并重新进行 C# 行为对照。不得在没有分叉证据时整体改写移动、伤害或碰撞数值。

## 16. Unity Client Host

Unity Host 负责：

- Loading 与 BattleBootstrap；
- 本地输入采样；
- Offline/Replay/Network host policy；
- 将 FrameInputSet 提交给 BattleSession；
- 资源、对象池、中央表现、UI、音频；
- GameObject shell 与 EntityHandle 的表现映射；
- Profiler、Editor 入口和压力工具。

Unity Host 不得：

- 在 MonoBehaviour Update 中直接修改战斗字段；
- 让 Transform/Animator/Physics 成为逻辑真相；
- 因 Addressables/BMP/音频加载完成顺序改变 spawn、碰撞或 RNG；
- 在网络回调中直接推进 BattleWorld；
- 让 presentation generation 或 GameObject 创建顺序影响逻辑 stableId。

当共享核心完全移除 Unity 依赖后，可以把 BattleSession 放入单个专用 simulation worker。核心仍是单线程确定性，只是从 Unity 主线程移到明确所有权的工作线程。输入通过不可变帧队列进入，表现通过双缓冲 snapshot 发布。该步骤用于 60/120 Hz 表现稳定性，不作为首批正确性迁移的前提。

## 17. 未来服务器 Host

未来服务器使用同一个 BattleKernel：

```text
Network Receive
  -> 验证后写 ServerInputInbox

Room Worker at frame deadline N
  -> 确定性补齐缺失输入
  -> 按 canonical player order 生成 FrameInputSet(N)
  -> BattleKernel.StepOneTick
  -> 写 FrameHistory / Checksum / Snapshot
  -> 广播 AuthoritativeFrameBundle
```

原则：

- 网络线程只入队，不直接修改房间世界；
- 一个房间内部串行推进；
- 多个房间可以分配到不同 worker/process；
- 服务器没有 Sprite、GameObject、Transform、Renderer 或 Audio；
- 协议与 transport 解耦，当前不绑定 UDP/KCP/ENet/MagicOnion；
- 先做同进程 loopback，再做独立进程，再接真实网络。

服务器广播可以低于 30 包/秒，但必须携带连续完整的 30 Hz FrameInputSet。客户端只能消费服务器授权的连续帧。

### 17.1 客户端与房间状态机

客户端：

```text
Disconnected
  -> Handshaking
  -> AwaitingStartBarrier
  -> PrimingAuthoritativeFrames
  -> Running / WaitingForGap / CatchingUp
  -> RecoveringSnapshot
  -> Running
  -> Ending / Faulted
```

服务端房间：

```text
Created
  -> WaitingForPlayers
  -> StartBarrier
  -> Running
  -> Finishing
  -> Archived
```

`StartBarrier` 固定协议版本、seed、资源指纹、capacity profile、canonical player slots、input delay、缺失输入策略和起始帧。进入 `Running` 后，单个客户端不能改变这些确定性配置。

### 17.2 控制面与高频数据面

控制面处理：登录、匹配、建房、握手、资源指纹、开始屏障、重连、快照请求和结束。它可以使用 RPC、MagicOnion/StreamingHub 或其他可靠通道。

高频数据面处理：

- `ClientInputCommand`；
- `AuthoritativeFrameBundle`；
- ACK、server/client sequence；
- 最近帧冗余或可靠补发；
- checksum report/request。

权威帧是流式消息，不为每个逻辑帧创建 request id、Future、反射派发和单独响应对象。协议 ID、序列化器和 dispatcher 在构建期生成；运行期使用复用 buffer，并对包长、frame window、session、player identity 和 schema 做边界验证。

### 17.3 混合同步的硬边界

NTSD 的“混合”含义是：

- 正常战斗：输入帧同步；
- bootstrap/rejoin/desync/观战：服务器状态快照 + 快照后的输入重放；
- 表现：确认逻辑快照的插值与可撤销预表现。

它不表示客户端上报伤害、服务器另算伤害，或服务器每几秒下发 HP/位置覆盖客户端。若未来某个玩法确实需要 FPS 式服务器命中、AOI 状态裁剪或 Source 式 lag compensation，必须作为新的同步模型单独立项，不能混入共享 BattleKernel 的基础合同。

### 17.4 安全假设

- transport 加密不能证明客户端输入诚实；
- 客户端只提交受限输入意图，服务器验证 frame window、频率、身份和范围；
- 服务器同核运行产生的 checksum 是权威诊断基准；
- 两端 checksum 不同不能用客户端多数投票确定真相；
- 服务器保留输入历史、关键诊断和必要的战后回放审计；
- 透视属于纯帧同步完整信息下发的固有限制，不能用协议加密或 checksum 宣称彻底解决。

## 18. 性能目标与线程预算

### 18.1 当前第一目标：1000 AI / 30 FPS

正式 `Dispersed1000` 与 `Combat1000`：

- 1000 个真实生产 GameObject 和逻辑实体；
- 全 AI、输入、DAT、碰撞、命中、opoint、生命周期、声音和中央表现开启；
- 逻辑固定 30 Hz；
- 单机每个可见帧最多一个 tick；
- 预热后连续至少 60 秒；
- P95 完整帧不高于 33.33 ms；
- formal tick 与项目表现链稳态 0 B；
- checksum、RNG、slot、事件与 C#/旧 oracle 一致；
- cleanup 恢复完整。

`33.33 ms` 是当前 30 Hz 容量门，不是允许核心长期占满的理想预算。通过该门后还要分别记录 BattleKernel、Unity host、presentation、render thread 和系统余量；正式发布目标必须根据目标 PC/移动设备的 P95/P99 留出网络、快照、OS 调度和偶发尖峰空间。知识库中的 `<5/<10/<15 ms` 只作容量思维参考，未经过 NTSD 同口径实测前不写成硬常量。

### 18.2 未来 60/120 Hz 表现

- 60 Hz 渲染预算为 16.67 ms；
- 120 Hz 渲染预算为 8.33 ms；
- 30 Hz 逻辑 tick 即使低于 33.33 ms，若仍同步阻塞 Unity 主线程，也可能破坏 60/120 Hz；
- 因此先减少单 tick 成本，再把纯 C# BattleKernel 放到专用 worker，通过双缓冲表现快照解耦；
- 移动端是否启用专用 worker、worker 数量和热预算必须通过真机测量，不能照搬 PC。

### 18.3 优化优先级

1. 错误复杂度：AI 全表扫描、无效碰撞 pair、重复查询；
2. 每 tick 重复工作：输入 facts、runtime snapshot、BuildCommands、handle/material 解析；
3. 引用对象和 Unity API：虚调用、组件查找、Transform、协程、日志；
4. 数据布局：对象图改成连续 SoA/bitset/sparse set；
5. 安全并行化：只读 gather、稳定 merge、表现和多房间。

ECS 是第 4 项的结构基础，也帮助前 2、3 项，但不能代替空间算法和表现边界修正。

持续超过预算时的处理顺序固定为：先区分 GC/Unity API 尖峰、错误复杂度、重复工作和数据布局；再决定是否迁移到专用 simulation worker。不得把 AI 降频、跳过有效碰撞、缩短技能链或限制 DAT 结果当作“框架优化”。

## 19. 实施路线

所有阶段都必须小步、可回退。不得一次性删除旧 runtime 后再尝试补行为。

### 19.1 当前执行硬边界

以下边界由用户在 2026-08-11 明确确认。它们的优先级高于后续章节中关于未来服务器的技术展开；上下文压缩、任务交接或实现阶段切换均不得自行扩大范围：

1. 当前只执行 U0～U9，先完成单机 BattleKernel、确定性闭环、零 GC 和 1000 AI 性能目标。
2. U0～U9 只保留未来服务器必需的纯 C# 接口、数据合同和可验证边界，不实现服务器房间、连接、广播或权威业务流程。
3. 当前不选择、不接入、不预埋具体网络库；transport 类型不得进入 BattleKernel。
4. 当前不实现 ACK、Jitter Buffer、服务器房间、登录、匹配、断线重连或真实网络恢复流程。U0～U9 可以定义并测试其所需的不可变帧、快照、历史和 checksum 基础合同，但不能据此把服务器阶段标为已实现。
5. U9 完成全部验收后必须停在阶段门，由用户明确确认是否进入 S0；不得自动继续服务器实施。
6. 用户批准后，S0 首先实现同进程、内存直连的服务器与多客户端世界，不使用真实 Socket；S0 只验证权威帧、连续消费、同核模拟和 checksum，不提前绑定生产 transport。

因此，当前不存在服务器代码不是 U0～U9 的阻塞项。反过来，在 U9 之前新增服务器业务、真实网络或第二套战斗结算属于越界实现。

### U0：工作树与权威基线封套

状态：已于 2026-08-11 完成。完整证据见 `Docs/unified-battle-u0-baseline-20260811.md`。Production Authority400 trace 仍在 tick 0 记录 Unity DAT 适配导致的 manifest 前置差异；authority-DAT diagnostic full trace 为 6/6 tick `equal-diagnostic`。Combat1000 两轮最终 lockstep hash 一致、sampled logic GC 为 0 B/tick，但 Unity frame P95 仍未达到 U9 门禁。

目标：确定迁移前的可复现 oracle。

工作：

- 审查当前未提交的 L0/L1、AI、碰撞、表现和零 GC 修改；
- 将已有修改按“已验证、候选、负实验、用户工作”分类；
- 固定 seed、roster、DAT/profile、输入 journal 和性能矩阵；
- 记录 Authority400 的逐 tick checksum、RNG、slot 和事件序列；
- fresh compile、focused tests、完整 self-check 和当前 1000 AI 报告。

完成门：迁移前行为与性能基线可重复；不能把当前脏工作树默认视为已完成。

### U1：Canonical Input 与 Host Policy

目标：所有模式共享唯一 FrameInputSet 输入边界。

工作：

- Unity 输入只采集意图；
- Local provider 生成完整 held/pressed/released；
- OfflineLocal、ManualReplay、NetworkLockstep 策略独立；
- 单机普通 Update 最多一个 tick；
- 同一 journal 重放产生相同 checksum；
- 为未来网络入口固定 frame/player key、幂等重复、冲突重复和锁定后不可变测试。

当前工作树已有候选实现，必须先复验再晋升，不能重复另写第二套。

完成记录（2026-08-11）：现有 `FrameInputSet`、local provider、strict delayed input buffer、replay journal 和 `BattleLockstepSession` 已晋升为唯一输入边界；`OfflineLocalTickPolicy` 已固定为普通 Unity `Update` 最多自动推进一个逻辑 tick，积压只留待后续 `Update`，`ManualReplay` 与 `NetworkLockstep` 不消费 Unity 墙钟。23 项聚焦测试 fresh PASS，同一三帧 journal 在重建世界后逐 tick checksum 一致，完整 `BattleRuntimeSelfCheck` fresh PASS。实现与证据详见 `Docs/unified-battle-u1-input-host-policy-20260811.md`。

### U2：表现发布边界

状态：已于 2026-08-11 完成。完整边界证据见 `Docs/unified-battle-u2-presentation-host-20260811.md`。逻辑 tick 只发布纯数据，中央命令、资源解析、排序、Mesh 与音频已移到 Unity host 每个可见帧最多一次的边界；fresh 聚焦测试 262/262 PASS，完整 self-check PASS，Combat1000 最终 lockstep hash 与 U0 相同。后续又移除了 CentralOnly 每个 `LateUpdate` 对全部 Legacy renderer shell 的重复扫描：聚焦测试 246/246、自检、零 GC 与 lockstep hash 均通过，同口径 CPU hierarchy 的 Main Thread 平均从 45.6808 ms 降到 40.1213 ms，详见 `Docs/unified-battle-u2-centralonly-renderer-shell-bypass-20260811.md`。这仍只关闭 U2 架构边界与对应重复扫描，不宣称 U9 性能门禁完成。

目标：把表现构建从逻辑 tick 中移出，同时保留 C# 的观察时点。

工作：

- 预分配 BattlePresentationSnapshot；
- 逻辑边界只复制纯数据；
- BuildCommands、资源解析、排序、Mesh 和音频由 Unity host 每渲染帧处理一次；
- 中间追帧 tick 保留事件但不重复物化表现；
- 有表现/无表现运行 checksum 一致。

### U3：ECS World 与只读 Shadow

状态：已于 2026-08-11 完成。完整证据见 `Docs/unified-battle-u3-ecs-readonly-shadow-20260811.md`。固定容量 Direct SoA、bitset、sparse store、slot/generation 身份和完整 runtime fingerprint 已建立；shadow 默认关闭，Compare 模式只读且没有反写路径。fresh ECS 聚焦测试 8/8 PASS，交叉回归 14/14 PASS，完整 self-check PASS，Authority400 authority-DAT diagnostic 6/6 tick 相等，`Extended1000` 预热后 capture/validate 为 0 B。该阶段只关闭 ECS 数据地基，不宣称 U4～U9 或 1000 AI / 30 FPS 已完成。

目标：建立专用混合 ECS，但不改变 canonical writer。

工作：

- EntityHandle、capacity profile、SoA stores、bitsets、sparse stores；
- 按当前旧世界每个 tick 同步 shadow；
- 对比全部字段、slot、generation 和 query membership；
- 禁止 shadow 反写旧 runtime。

### U4：纯数值与高频 Pass 迁移

状态：已于 2026-08-11 完成。cooldown 切片已完成并晋升默认，权威合同、双实现模式、零 GC、1000 AI A/B、完整 self-check 与 Authority400 full trace 证据见 `Docs/unified-battle-u4-cooldown-migration-20260811.md`。AI 数据化感知/决策链也已通过 1000 AI 严格 A/B、十域 lockstep hash、零 GC、185 项聚焦测试和完整 self-check，`DataOrientedCanonical` 已晋升生产默认；证据见 `Docs/unified-battle-u4-ai-profile-promotion-20260811.md`。character Stage-Z 数据路径已完成行为、零 GC、Authority400 与 1000 AI A/B 验证，但目标 pass P95 只改善 4.66%，未达到 10% 晋升门槛，故正式默认保持 Legacy；完整证据见 `Docs/unified-battle-u4-stagez-migration-20260811.md`。FramePostProcess 同样完成权威合同、33 项交叉回归、Authority400、零 GC与 1000 AI A/B，但 P95 恶化 55.15%，因此默认保持 Legacy；完整证据见 `Docs/unified-battle-u4-frame-postprocess-migration-20260811.md`。CandidateCollect 的 LegacyOnly 零 ITR 前置路径虽保持哈希一致和零 GC，但相邻 A/B 中逻辑均值慢 1.84%、P95 慢 2.78%，已撤回该实验并恢复原路径；证据见 `Docs/unified-battle-u4-candidate-zero-itr-preflight-20260811.md`。LateEntityUpdate 新鲜细分测量表明完整 pass average 为 2.9092 ms，最大子段 0.7622 ms，纯数值 Recovery 仅 0.2290 ms；逐 slot 生命周期段进入 U5，不新增低收益 writer，证据见 `Docs/unified-battle-u4-late-entity-update-assessment-20260811.md`。U4 的完成表示所有计划切片均已迁移或完成数据化取舍，不表示 U5～U9 或 1000 AI / 30 FPS 已完成。

迁移建议顺序：

1. cooldown、基础 frame/motion/bounds；
2. CharacterInput facts 与 AI decision；
3. AI spatial query；
4. CandidateCollect 的 participant/broadphase/exact；
5. LateEntityUpdate 中无结构变化的数值段。

每次只允许一个 canonical writer；旧路径保留只读 oracle，逐 tick 比较后再切换默认。

### U5：Interaction、Hit、Rest 与复杂生命周期

目标：迁移结果敏感的交互域。

工作：

- cpoint、held、link；
- character/object hit；
- aRest/vRest；
- opoint 分段播放；
- spawn/destroy/free/unregister/generation；
- stage 和 battle results。

这是最高风险阶段，必须按权威 boundary 串行迁移，不能用通用 tick-end command buffer 简化。

当前进度（更新至 2026-08-12）：

- `CharacterHitConsume` 空候选精确 `LF2Character` 快速路径已经完成并晋升生产默认；派生类型、快照过期、候选源不可读或存在候选时全部 fail closed 到权威对象路径；
- 聚焦测试覆盖空候选等价、候选源不可用、过期快照刷新、派生虚调用与预热后 `0 B`；
- 同 seed、1000 AI、30 warmup + 180 sample 的稳定相邻 Legacy D / Fast E A/B 保持十域 lockstep hash 完全一致和正式 tick `0 B`，目标 pass 均值/P95 分别改善 56.5%/59.9%，逻辑 tick 均值/P95 分别改善 14.8%/27.3%；
- 完整证据见 `Docs/unified-battle-u5-empty-character-hit-consume-20260811.md`；这只关闭 U5 的 character hit 空候选切片，不代表 U5 整体完成；
- character 空候选快速路径内部的 runtime candidate-count gate 已完成 7 项聚焦测试、244 项联合回归、fresh self-check 与 1000 AI 隔离 A/B；行为、零 GC 和十域 hash 均一致，但目标 pass average/P95 分别慢 12.90%/4.71%，因此生产默认继续使用已晋升的 range proof；
- runtime count 候选只作为诊断实验保留，完整证据见 `Docs/unified-battle-u5-character-runtime-candidate-count-gate-20260811.md`；本结论不回退 character 空候选 whole-pass 优化本身；
- Stage 场景配置已经移到 `SimulationTickDriver` 的 tick 宿主边界，每 tick 发布一次 `BattleStageRuntimeState`，`StageBounds`、`PreFrameBounds` 和 ECS Stage-Z pass 只读 runtime 快照；
- 同配置 1000 AI Legacy/Host A/B 中，Unity 场景读取从 630 次降至 210 次，20 个 parity/lockstep 分域 hash 全部一致且正式 tick 维持零 GC；但总体 tick average/P95 分别慢 1.44%/3.30%，所以该切片只作为确定性边界晋升，不宣称性能收益；
- Stage 宿主快照的完整证据见 `Docs/unified-battle-u5-stage-host-snapshot-20260811.md`。复杂 writer 与结构生命周期继续按后续各批次证据处理。
- `PreInteraction` whole-pass no-op 证明已经通过 7 项聚焦测试和 1000 AI 正式 A/B；91/210 个 tick 被证明为全局无副作用，跳过 273,000 次对象调用，其余 119 个 tick fail closed 到完整权威路径；
- 该切片的 20 个 parity/lockstep 分域 hash 全部一致且正式 tick 零 GC，目标 pass average/P95 改善 35.14%/27.91%，逻辑 tick average/P95 改善 10.84%/20.50%；完整证据见 `Docs/unified-battle-u5-preinteraction-noop-proof-20260811.md`；
- whole-pass proof 只关闭可证明的空操作路径，不表示真实 cpoint、held、link writer 已迁移；存在交互时仍保留原对象路径和原顺序。
- `PreInteraction` fallback 的逐 participant 精确过滤已完成 8 项真实 kind1/kind2/stale-held 聚焦验证、245 项联合回归、fresh self-check 与三轮 1000 AI A/B；派生类型和过期快照 fail closed，真实 writer 与升序顺序保持不变；
- 三轮目标 pass average/P95 平均改善 35.46%/43.35%，整 tick average/P95 平均改善 2.81%/4.64%，六轮均为正式 tick `0 B` 且十域 hash 一致，故晋升生产默认；完整证据见 `Docs/unified-battle-u5-preinteraction-participant-filtering-20260812.md`；
- 该晋升移除的是被证明无副作用的对象调用，不能扩大为真实 cpoint/held/link canonical writer 已迁移的声明；
- Late tail no-op 候选已完成 6 项聚焦测试和 1000 AI A/B；虽然跳过 210,000 次方法调用且 hash/零 GC 一致，但 `TailAndQueuedFlush` average 慢 8.17%、Late pass average 慢 9.06%、逻辑 tick average 慢 6.29%，因此不晋升；
- 生产默认继续使用完整权威 late tail，候选只作为诊断关闭路径保留；完整证据见 `Docs/unified-battle-u5-late-tail-noop-assessment-20260811.md`。
- `ObjectHitConsume` 空候选 whole-pass 证明已通过 7 项聚焦测试、233 项压力工具回归和两组相邻 1000 AI A/B；当前 DAT 类型、slot generation、派生虚调用和不可读候选源全部 fail closed；
- 两组目标 pass average/P95 合并改善 27.80%/22.20%，正式 tick 均为 `0 B`，最终 lockstep overall hash 完全一致；整 tick average 合并波动为 -2.59%，因此只声明稳定局部收益，不声明总体帧率改善；完整证据见 `Docs/unified-battle-u5-empty-object-hit-consume-20260811.md`；
- `aRest` 的 canonical writer 已由 U4 `BattleEcsCooldownPass` 接管；`vRest` 当前已经由 `RuntimeRestStore` 按 handle/generation 保存并由权威 pair pass 消费。U5 不新增第二套 rest writer，后续只继续核验真实 hit/cpoint/opoint 对 rest 的写入与可见边界；
- 正向 link validation 已完成 live-runtime 数据候选、6 项聚焦测试、243 项联合回归、fresh self-check 与 1000 AI A/B；十域最终 hash 完全一致且正式 tick 为 `0 B`，但目标 pass average/P95 分别慢 50.73%/27.80%，因此生产默认保持 Legacy；完整证据见 `Docs/unified-battle-u5-positive-link-validation-assessment-20260811.md`；
- 该评估只拒绝当前逐 slot 数据候选，不能据此声明真实 cpoint/held/link writer 已迁移；统一 canonical link store 必须与真实 writer 的同 tick 可见性一起处理，不能读取 tick-end shadow；
- 历史 C# `GameTick -> HitResolver -> HitResolve` 闭合只保留为迁移辅助；当前 character/object hit writer 合同必须重新以 C++ release live path 的 `game_tick -> collision_collect/collision -> hit` trace 闭合。完整原子边界包含 `PrevFrame2`、slot/candidate 顺序、abort residual、preprocess、RNG、所有 kind、伤害统计、rest/link、声音/事件、opoint 与生命周期，禁止只迁移扣血片段；
- Unity 已加入默认 `Disabled`、固定容量、只读的 `BattleEcsHitExecutionPlan` 影子；显式 `ShadowCapture` 时在两个正式 pass 前冻结 attacker/candidate/itr 顺序，任何输入不可读均 fail closed，target generation 只作诊断而不新增权威判定；
- `ShadowCompare` 已在正式旧 writer 消费期间逐项核对 pass、attacker handle、candidate ordinal、target slot、itr index/fingerprint 与原始 consume 标志；Legacy range 只在该诊断模式开启时补取 attacker handle，默认路径不增加 handle 查询；多读、少读和内容不一致均 fail closed；
- 当前 pair preprocess 及预消费副作用影子已经接入四条真实 consumer 链，只在旧 `ApplyReleaseSceneQueryConsumeEffects` 前后观察；计划使用 preprocess 后的实际投影，不错误复用碰撞冻结时的原始标志；
- kind9 已闭合 kind9→kind0、attacker HP 归零；重武器已闭合 target/held link、两组 vRest、随机 frame、Vy 与 RNG state/call count。影子默认关闭、不替代 writer、不推进 RNG，预热后为 `0 B`；
- dispatch 只读观察已接入 character、DAT character、weapon 与 special attack 四条真实 consumer；独立 OID300 投影验证成功 redirect 只终止当前 attacker 的剩余候选，下一个 attacker 必须继续，伪造终止 fail closed；
- 全部权威 kind 的独立 disposition 投影已经接入并覆盖 `0/9/6/8/14/15/16/10/11/1/3/2/7`、未转换 `4/5` 与未知 kind；未转换 kind4 现按权威 no-op，kind5 替换不再倒灌触发前序 held release，weapon/special 的 kind6 现只写 hit-confirm 后返回；错误或缺失 disposition 均 fail closed；
- writer-effect 只读 oracle 已闭合 kind `6/8/14/1/3/2/7/10/11/15/16` 的精确状态变化；历史 C# 成功语义只作辅助，Unity dispatch `bool` 也不得被当成 C++ release 权威成功语义。kind1 oracle 实际检出并修正旧 character consumer 未写双方速度、朝向、抓取对位、槽位与持续时间的差异；无效/过期 kind16 link 仍须按 C++ release trace 保持原状态；
- damage `0/9` 的精确 oracle 已覆盖标准角色 HP/HPMax/统计、轻中重硬直与倒地、标准致死时 HPBound/combo/kill/damage stats/强制 fall、X/Y 击退、rest、RNG、声音和 hit-record，alternate 非致死/致死，以及标准武器类型 `1/2/4/6` 的 hit-confirm2、weapon HP、effect0/effect4 声音、随机帧、vRest 与 heavy low-fall 分支；type3 已覆盖 object-hurt、relation/holder-copy、motion 清零、rest、hit-record、state3005/3006 同步、D1 直接/活动身份替换，以及 effect `0/2/3/5/21/22/23/30/5005/5999/6033` 的 frame、主/追加声音、PP 扣减与下限；effect20 对非角色 DAT 由权威碰撞收集前置拒绝，未伪造为可达 writer 输入；
- damage oracle 实际检出并修复 `LF2WeaponBase.SetFrameDirect`、`LF2SpecialAttack.SetFrameDirect` 与公共 `DirectWriteHeldFramePreserveWaitCounter` 未同步 `Runtime.Frame` 的双帧镜像遗漏；该历史 C# 镜像不再自行定义规则，修复仅在 C++ release trace 同样确认 frame 真值和切帧时序后才能作为默认路径；
- 权威源码全量检索确认 `AbortRemainingHitPairs = true` 只有 `ApplyOid300SpecialHit` 一处，因此 abort 来源与只跳过同 attacker 的边界已经关闭；
- 命中计划在 alternate 致死补齐后聚焦测试 96/96 PASS（job `562ce635bbf64029a5b1319f45ec6dcd`）；命中计划、character/object 空候选与碰撞命中见证联合回归 112/112 PASS（job `798b5f79820c400cbe61497a1de3c186`），完整 `BattleRuntimeSelfCheck` 于 `2026-08-12 08:52:47` fresh PASS，并保留 `CharacterHit -> RandomDrop -> ObjectHit` 边界；
- OID `0xD6` 对角色命中后攻击者 HP 归零，以及 OID `0xC9` 的 `FreeEntity(attackerSlot)` 生命周期 shadow 均已在恢复后的 Unity 中完成定向运行验证。D6 用例按正式对象命中 pass 消费；C9 同时核对旧 handle 失效、slot 未占用、generation 精确递增一次、occupant 清空及攻击者 runtime slot 清为 `-1`；完整证据见 `Docs/unified-battle-u5-hit-writer-contract-20260812.md`；
- OID `5/52` 的 `HP/HPMax/HP3=10`、`PP=5` 已确认属于 opoint 创建期初始化，Unity DAT 角色命中链中错误的逐命中重置已移除；shadow 用例同时覆盖正常 HP/HPBound 扣减与 HP3/PP 保持。OID100 的角色、DAT 角色与特殊攻击链也已补齐权威早退/尾链顺序：窄速域且 `dvx == 0` 的固定 `5` 分支必须跳过 `2.5x`、最小 `10` 和 `SFX_039`，其他分支仍执行尾链；
- 标准角色 damage shadow 已补齐上一帧 `Frozen` 或碰撞快照 `PrevFrame2` 为 `Falling` 时的强制 fall80，以及 reciprocal catcher/link + `PrevFrame2.cpoint.kind == 2` 时按朝向选 `fronthurtact/backhurtact` 的被抓受击帧。测试夹具按正式边界先捕获历史/被抓帧，再切回受击时当前帧，避免在碰撞快照前手写 `PrevFrame2` 后被正式快照覆盖；OID5/52、OID100、历史帧与 cpoint 用例均已进入 118/118 Unity PASS；
- standard/alternate damage 的 `state1002/2000/3000` 攻击者尾链已加入只读投影与定向用例，覆盖 RNG 在 hit-record 前的消费顺序、相对 X 击退、state2000 速度衰减，以及 standard/alternate 对 frame10 `dvz` 的不同处理；active holder 也已从 `HolderCopy` 统计归属中拆分，单独比较负 link 下的 `FrameDelay` 传播；
- type3 标准尾链已移除只允许 character attacker 的人工限制，并按权威区分 active 非角色攻击者固定 frame20、character/held 攻击者按 effect 选择 frame20/30；active holder 的 relation/holder-copy 与 FrameDelay 传播、失效 holder 保持目标原关系、OID8/D1/D5 未满足 Karasu 身份替换时回退标准尾链均已通过。HP<=0/空中目标、上一帧 Frozen、`PrevFrame2` Falling，以及 state1002 的 RNG/反弹速度和 state3000 的 frame10/dvz 也已纳入精确比较；
- 击飞 held-pair vRest 核验实际检出生产差异：character 与 DAT-character consumer 漏写 `held->attacker=45`、`victim->held=30`，special-attack consumer 又增加了权威没有的负 link 门槛。三条正式路径现统一到共享权威 helper，shadow 使用方向独立字段核对；active-holder FrameDelay、standard/alternate/object damage 与 state1002/2000/3000 尾链也已进入 118/118 Unity PASS；
- 对象伤害继续关闭了此前无权威依据的输入排除：正式 `LF2Weapon` 补回被击飞武器持有另一实体时的双向 vRest；shadow 同时覆盖权威/伪持有关系、普通负 link 武器、`bdefend=100` 把耐久直接写 `-1`、空中重武器低 fall 随机 frame，以及 type4/6 先于 effect22/23 分支执行的动态速度击退。type3 普通尾链不再只允许三个目标 state，也不再排除 `bdefend=100`；state3005/3006 仍由专用同步投影接管；
- 命中调度的 DataOriented frozen-plan 执行已完成同 seed、30 warmup + 180 sample A/B：行为/hash/正式 tick 0 B 一致，但逻辑 tick average 仅改善 0.224%、P95 反向波动 0.403%，CharacterHit average/P95 又慢 3.75%/4.73%，未达到 10% 晋升门槛，故正式默认继续 `Disabled`；
- kind `1/3` 抓取和 kind `2/7` 拾取/link 写入现由每个 `SimulationWorld` 持有的 `BattleInteractionWriter` 统一执行；四种 LF2 consumer 只负责对象适配，不再复制该组状态写入；
- held step12 的持有帧/位置同步、受击掉落、投掷、随机掉落和 link 清理已从静态 `LF2HeldObjectRuntime` 迁到 world-owned `BattleHeldObjectWriter`；slot 升序遍历仍由 `SimulationQueryAndLinkModule` 持有，没有改变权威顺序；
- 本批 fresh 证据：0 C# error；联合 EditMode job `f96f9a461ece4cd48b2bed8f3d64abda` 209/209 PASS；完整 self-check 于 `2026-08-12 17:42:09` PASS；Authority400 full/full 为 6/6 `equal-diagnostic`、`firstDifference=null`；
- 本批 1000 AI 报告 `Temp/NTSD_ProductionEntityStress.u5-held-writer-1000ai-60-20260812.json` 为 30 warmup + 60 sample，单 tick average/P95/max 22.3180/29.1818/32.7218 ms，60/60 tick 为 0 B，lockstep overall hash 与迁移前完全相同，teardown 无残留；但最大 backlog 7、丢弃 backlog 21，故只证明当前 writer 迁移通过短样本和单 tick 预算，不关闭 U9；
- cpoint kind1/kind2/held 的 reciprocal link 校验、decrease/escape、动作选择、即时 action、位置/速度/伤害/统计同步、投掷、无效 link 回退和持续 held sync 已迁到 world-owned `BattleCpointWriter`；对象入口现在只保留兼容 adapter；
- cpoint 批次 fresh 证据：0 C# error；联合 EditMode job `07b114009a07489d951da85b13df0efb` 201/201 PASS；完整 self-check 于 `2026-08-12 18:09:43` PASS；Authority400 full/full 为 6/6 `equal-diagnostic`、`firstDifference=null`；
- 1000 AI 报告 `Temp/NTSD_ProductionEntityStress.u5-cpoint-writer-1000ai-60-20260812.json` 的 average/P95/max 为 23.0081/28.6700/32.4747 ms/tick，60/60 正式 tick 为 0 B、Gen0/1/2 collection 为 0，lockstep overall hash 与迁移前一致，所有临时开关恢复且 failure 为空；最大 backlog 7、丢弃 backlog 28，仍不关闭 U9；
- 标准角色伤害、kind16、alternate damage、武器伤害及特殊攻击/type3 对象伤害的完整原子事务均已迁到 world-owned `BattleDamageWriter`；四类 consumer 只保留对象适配，HP/HPBound、统计、帧/速度、rest/link、RNG、声音、hit record 与状态尾链仍在原候选位置同步提交；
- damage 第一批 fresh 证据：0 C# error；零分配定向 job `0e16b0e8401340d8aa53bc0c31e3e8ef` 1/1 PASS；U5 六组联合 EditMode job `c773f7ab85304d21971c4801a40fc078` 202/202 PASS；完整 self-check 于 `2026-08-12 18:40:57` PASS；Authority400 full/full 为 6/6 `equal-diagnostic`、`firstDifference=null`；
- 1000 AI 报告 `Temp/NTSD_ProductionEntityStress.u5-damage-writer-1000ai-60-20260812.json` 的 average/P95/max 为 23.3549/30.0303/35.8729 ms/tick，60/60 正式 tick 为 0 B、Gen0/1/2 collection 为 0，lockstep overall hash 与迁移前一致，teardown 无残留；最大 backlog 7、丢弃 backlog 29，且 max 超过 33.333 ms，因此仍不关闭 U9；
- W05A～W05E 的 opoint 最低槽、下一表现 tick、generation/ghost、单个/六个生成释放和 death cleanup 合同已接入 world-owned `BattleStructuralWriter`。该 writer 在当前实体即时、当前 pass 分段、下一 pass、tick end 与 deferred unregister/free 之间保留显式 boundary，没有把结构变化错误折叠成通用 tick-end command buffer；
- register/unregister、free/destroy 与 slot generation claim/release 已统一经过同一 structural writer，实际对象池物化仍由 Unity factory/adapter 执行；这只改变结构写入所有权，不改变对象池资源职责；
- 权威 `RunResultsTick`、`UpdateBattleResultsFlow` 与 `ResultsState` 已迁到 world-owned `BattleResultsWriter`/`BattleResultsRuntimeState`；活动结算态只运行输入与结算状态机，阶段、光标、队伍表、难度/stage 选择、rematch/route intent、reserve commit 与 fall-damage 分发均进入 lockstep checksum；
- U5 最终验证：Unity fresh compile 为 0 C# error；联合 EditMode job `b55c2edd04964be7b784f7bec65ab0f5` 为 220/220 PASS；退出 Play Mode 后完整 self-check 于 `2026-08-12 20:34:10` fresh PASS。压测后仍处于 Play Mode 时曾因夹具不会同步执行 `PresentLatestFrame` 得到表现探针假失败，退出 Play Mode 后通过，未发现状态泄漏；
- U5 最终 1000 AI 报告 `Temp/NTSD_ProductionEntityStress.u5-battle-results-writer-1000ai-60-20260812.json`：1000 个真实生产 GameObject/逻辑实体、30 warmup + 60 sample，average/P95/max 为 20.6265/25.7384/28.0438 ms/tick，正式 tick average/max allocation 为 0 B，Gen0/1/2 collection 均为 0，cleanup `restored=true`；最大 backlog 7、丢弃 backlog 29，因此只关闭 U5，不关闭 U9；
- 完整实现和证据见 `Docs/unified-battle-u5-hit-writer-contract-20260812.md`、`Docs/unified-battle-u5-interaction-held-writers-20260812.md`、`Docs/unified-battle-u5-cpoint-writer-20260812.md`、`Docs/unified-battle-u5-damage-writer-20260812.md`、`Docs/unified-battle-u5-structural-writer-20260812.md` 与 `Docs/unified-battle-u5-battle-results-writer-20260812.md`；
- U5 在“复杂写入所有权与权威可见边界”层面关闭。`LF2Entity` / `NTSDEntityRuntime` 字段仍是兼容真值，最终 SoA 存储、对象 shell 退化与对象式热循环移除属于 U6，不能把 U5 完成扩大为 U6/U9 已完成。

### U6：移除对象式逻辑热循环

状态：已按计划的生产所有权边界完成。generation-owned store、world-owned writer、数据化 exact-character/AI 热路径、结构索引与表现 publication 已成为正式 owner；Unity shell/未知派生类型和经 A/B 证明会回退的低频 fail-closed 兼容链保留，但不拥有战斗真值。最新 U9 五场景均启用 `requireU6ProductionOwnershipAudit`，configuration/runtime evidence 全通过，canonical mismatch、unexpected fallback 与 hard failure 为 0，正式 PlayerLoop managed-memory 边界为 0 B；因此第 1120 行所述退出门已由 U9 Player 矩阵关闭。完整收口证据见 `Docs/unified-battle-u9-final-acceptance-20260815.md`。

目标：BattleKernel 成为唯一战斗真值。

- LF2Character/LF2Weapon/LF2OtherObject 逐步退为 Unity shell/兼容 adapter；
- GameObject 不再拥有逻辑字段；
- 移除每实体 MonoBehaviour 战斗 Update；
- 保留对象池与表现绑定；
- 删除已经通过验收的旧 canonical writer，不永久维持双实现。

当前进度（更新至 2026-08-12）：

- 第一切片已移除 `RuntimeSlotTable.Entry` 逐槽对象，把 runtime、occupant 与 generation 改为页内 SoA 数组；slot claimed 状态只由 `RuntimeSlotAllocator` 保存，不再在 table 中维护第二份布尔真值；
- paging、lazy materialization、`Authority400` / `Extended1000` 容量、最低空闲槽、generation 失效、occupant 解析和公开 API 语义保持不变；本切片没有切换 BattleKernel canonical ownership，也没有删除 `LF2Entity` / `NTSDEntityRuntime` 兼容字段；
- fresh Unity C# 编译为 0 error；最终回退后 EditMode job `1249c3dfb49d4324973b1942dde1e9cd` 为 258/258 PASS；完整 self-check 于 `2026-08-12 21:20:47` PASS；
- Authority400 full/full diagnostic 对照为 6/6 `equal-diagnostic`、`firstDifference=null`，见 `Temp/NTSDParity/u6-slot-page-soa-final-compare-authority-dat-diagnostic-20260812.json`；
- 最终 1000 AI 回归 `Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-performance-smoke.json` 使用 1000 个真实生产 GameObject/逻辑实体、30 warmup + 180 sample，average/P95/max 为 21.1567/25.5118/28.8231 ms/tick；正式 tick average/max allocation 为 0 B，Gen0/1/2 collection 均为 0，teardown `restored=true`，最终 lockstep overall hash 为 `4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`；
- 该回归的最大 backlog 为 7、丢弃 backlog 为 28，因此只证明 U6 第一切片保持单 tick 预算、零 GC、确定性 hash 与生命周期恢复，不关闭 U6/U9；
- `PendingFlushDestroy` 世界级 slot/generation canonical store 候选完成同场景相邻 A/B：Legacy average/P95/max 为 21.2927/26.1168/33.8060 ms，canonical 为 23.2539/27.4474/34.8992 ms，hash/零 GC 一致但平均回退约 9.2%，故实现与诊断开关均已回退；
- CollisionSnapshot -> CandidateCollect 跨 pass 完整复制候选让 CandidateCollect average 节省约 0.284 ms，却让 CollisionSnapshot 新增约 0.673 ms，两个 pass 合计回退约 0.389 ms/tick，已完整回退；回退后 245/245 聚焦测试与 fresh self-check 通过；
- runtime slot 页内 occupant 直接枚举完成两组 Legacy 与两组 candidate 的 180-tick A/B：candidate average 改善约 0.9%，但 P95 回退约 0.9%；四组最终 hash 一致、正式 tick 0 B、cleanup 完全恢复，仍因未达到稳定正收益门槛而完整回退；
- 完整 registry 边界见 `Docs/unified-battle-u6-registry-page-soa-20260812.md`，字段簇、负实验和下一顺序见 `Docs/unified-battle-u6-canonical-field-cluster-inventory-20260812.md`；
- CharacterInput 第一组原子提交已经迁到每个 world 独占的组合式 `BattleAiInputWriter`：IndexedCanonical AI 决策的输入历史、冷却、组合计数、previous/current keys、共享 flow 与 RNG state/call count 不再由 shadow partial 私有方法直接提交；值类型 kernel、slot 顺序和 RNG 消费点保持不变；
- 本批 Unity 编译为 0 C# error；AI/SoA/压力工具联合 EditMode job `d974a1e780934d30b900800084e277d0` 为 386/386 PASS；完整 self-check 于 `2026-08-12 22:36:48` fresh PASS；
- 本批 1000 AI 报告 `Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-performance-smoke.json` 使用 1000 个真实生产 GameObject/逻辑实体、30 warmup + 180 sample，average/P95/max 为 21.8997/29.1202/38.0412 ms/tick，正式 tick 为 0 B、Gen0/1/2 collection 为 0，最终 lockstep overall hash 仍为 `4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`，teardown 完全恢复；该 writer 是所有权迁移，不宣称性能提升；
- 第二个 CharacterInput 小切片新增 `BattleCharacterInputWriter`，集中提交 AI/human adapter 共享的 previous/current key、cooldown、defend lock 与 combo 计数；`BattleAiInputWriter` 继续只拥有 AI-only history、共享 flow 与 RNG。生产 world-bound adapter 已进入共同 writer，独立兼容对象保留非 canonical 私有 fallback；
- 第二切片 Unity 编译为 0 C# error；联合 EditMode job `99c84c8511d846aebc6eefdcc30e1db2` 为 393/393 PASS；完整 self-check 于 `2026-08-12 22:51:36` fresh PASS；
- 同一路径 1000 AI 重跑 average/P95/max 为 20.8958/25.3425/28.9198 ms/tick，正式 tick 为 0 B、Gen0/1/2 collection 为 0，最终 hash 不变、teardown 完全恢复；最大 backlog 为 7、丢弃 backlog 为 26。该次没有精确 toggle A/B，只证明未观察到明显回退，不宣称确定性能提升；
- 第三个输入小切片把 world-bound AI roll/current-key clear、frame advance current-key clear、edge/history、battle-entry reset、N30 history gate/tail 与 frame 110/114 defend lock 迁到 `BattleCharacterInputWriter`；无调用方的两段共享输入私有死代码已删除，未注册对象继续保留 runtime 兼容 helper；
- 第三切片 Unity 编译为 0 C# error；输入聚焦 job `6dda2371888b421fb62bfda872d76f34` 为 23/23 PASS，较早联合 job `df71d4c9a1414b5eb780e1fb453fa0c2` 为 448/448 PASS；完整 self-check 于 `2026-08-12 23:12:54` fresh PASS；
- 最新 1000 AI 30 warmup + 180 sample 的 average/P95/P99/max 为 21.4288/26.4588/34.2308/37.1369 ms/tick，正式 tick 0 B、Gen0/1/2 collection 为 0，最终 hash 不变、teardown 完全恢复；最大 backlog 为 7、丢弃 backlog 为 24，仍不关闭 U9；
- 第四个 CharacterInput 小切片新增 `BattleCharacterInputActionResolver` 与仅包含 17 个 byte 字段的 `BattleCharacterInputActionState`。human/AI 现在共享唯一组合技/direct-frame 算法，正式 world AI 直接从 runtime 捕获 progress 并由 writer 一次提交，不再经过 `NTSDInputStateModule` progress 镜像；旧 module 内重复的 combo/direct-frame 算法已删除；
- 第四切片 Unity 编译为 0 C# error；输入/AI/SoA EditMode job `e2342e5439064732946c9605fab5bae1` 为 188/188 PASS；直接 resolver 定向测试 1/1 PASS；完整 self-check 于 `2026-08-12 23:38:04` fresh PASS；
- 同配置无详细诊断的两次 1000 AI 复测 average/P95/P99/max 分别为 21.6286/25.7297/29.3456/31.1076 与 21.8553/27.3231/31.3929/33.5564 ms/tick；正式 tick 均为 0 B、Gen0/1/2 collection 为 0，最终 hash 不变、teardown 完全恢复；详细诊断确认 `InputStateSyncFromRuntime` 从较早基线约 0.0656 ms/tick 归零；
- 相比第三切片 21.4288 ms 平均基线，平均值没有稳定提升并约轻微回退 1.5%，P95 基本持平，P99/max 尾部改善可复现。因此保留该所有权/稳定性迁移，但不宣称平均 FPS 提升；
- 第五个输入/动作小切片新增 world-owned `BattleCharacterActionWriter`，注册 world 内的组合/direct-frame 跳帧与完整 `ProcessReleaseInput` 均先进入该组合事务入口；frame、facing、HP/PP、`ComboCountVic`、直接动作和速度的既有权威顺序保持不变，未注册对象保留兼容实现；
- Legacy/fail-closed 审计确认：生产 `UnifiedAuthority` 发布后禁止 Legacy fallback 并 hard breach，只有发布前整批失败才允许完整 LegacySeparate pass，因此不会形成半 tick 混写；旧 Legacy writer 仍作为兼容 oracle 保留，待新 store/reader 全部闭合后删除；
- 第五切片 Unity 编译为 0 C# error；新增 writer 定向 job `ebbe1ed671104d0880180619608436bc` 为 1/1 PASS，输入/AI/fail-closed/profile 联合 job `5166dbdbf36345428d0c4ce9cd12fa06` 为 73/73 PASS；完整 self-check 于 `2026-08-12 23:54:47` fresh PASS；
- 最新 1000 AI 30 warmup + 180 sample 的 average/P95/P99/max 为 21.5764/26.4189/28.6741/30.1509 ms/tick，正式 tick 0 B、Gen0/1/2 collection 为 0，最终 hash 不变、fallback/hard-breach 均为 0、teardown 完全恢复；最大 backlog 7、丢弃 backlog 32，仍不关闭 U9；
- 第六个输入小切片新增 slot/generation-owned `BattleCharacterInputStore`。DataOriented common input capture 与 action progress reader 以连续值类型行作为真值，AI 完整提交整行，human 只更新 held/previous/progress 子段；旧 generation release 不会清除复用槽的新状态，runtime 继续作为 U6 兼容镜像；
- 初版 bit-packed 多数组实现使 detailed average/P95 为 `24.2364/29.8481 ms`，最终按 AI 整行访问模式改为连续 `AiDecisionInputState[]` 后恢复到 `23.7045/28.0330 ms`，`IndexedCanonicalCapture` 从 `0.6284` 降到 `0.4545 ms`。相对迁移前 detailed `23.5199/28.3089 ms` 属约 0.8% average 波动、P95 略好，故按结构风险关闭且整体无显著回退保留，不声明 10% 性能收益；
- 第六切片 fresh 证据：Unity 编译 0 C# error；联合 EditMode job `cfceccbf9bd242f6aa5fabbb359c84d0` 为 183/183 PASS；完整 self-check 于 `2026-08-13 00:39:20` PASS；同配置 Legacy/DataOriented detailed 最终十域 hash 一致、正式 tick 0 B、fallback/hard-breach 0、teardown 完整恢复；
- 无详细诊断的 production smoke 使用 1000 个真实生产 GameObject/逻辑实体、30 warmup + 180 sample，average/P95/P99/max 为 `21.4001/26.9671/35.0497/37.6365 ms`，正式 tick 0 B、Gen0/1/2 collection 0、final hash `752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`、teardown `restored=true`；maximum backlog 7、dropped backlog 25，因此仍不关闭 U9；
- 第七个输入小切片继续收口 AI-only 字段：N30 teammate broadcast 与 AI 决策的 `Unk360/Unk3FC/Unk400` coordinate target 通过 `BattleAiInputWriter` 写入持久 store；kind14 四方向阻挡通过 world-owned `BattleBoundaryWriter` 发布，Character/Entity/Weapon mechanics 消费并清零后同步同一 store。两条路径均保持 runtime 兼容镜像、既有 RNG 调用次数与方向判定顺序，未注册对象保留旧兼容实现；
- 第七切片 fresh 证据：Unity 编译 0 C# error；输入/AI/sensing/profile 联合 EditMode job `e444a15e01cd4c61ad9237935507c814` 为 175/175 PASS；完整 self-check 于 `2026-08-13 01:01:12` PASS；1000 AI production smoke 使用 30 warmup + 180 sample，average/P95/P99/max 为 `23.3822/27.9603/31.1907/34.1888 ms`，正式 tick 0 B、Gen0/1/2 collection 0、final hash `752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`、fallback/hard-breach 0、teardown `restored=true`；maximum backlog 7、dropped backlog 31。该次 Editor 样本只用于回归门禁，不据单次波动宣称性能提升；
- 第八个 frame 前置切片按权威 C# `BattleCore/Frame/FrameRuntime.cs::SetFrameImmediate` 的单一 `Entity.Frame` 合同，把 Character、Weapon、SpecialAttack、OtherObject、opoint、reset 与测试探针中的生产 `Frame.N` 写入全部收口到 `LF2Entity.WriteCurrentFrameId`；该入口在同一调用点写入 `Frame.N` 与 `Runtime.Frame`。`FrameTransistor` 已直接维护 wait/next，`LF2Health` 已直接绑定 HP/MP/PP，所以 `RefreshRuntimeSnapshotAfterCharacterInput` 不再重复复制 12 个已有直接写入口的字段；强制 Legacy A/B 仍可调用完整 `RefreshRuntimeSnapshot`；
- 第八切片 fresh 证据：Unity 编译 0 C# error；联合 EditMode job `5c5c9d432a454ccaa76e2a889535f070` 为 176/176 PASS，新增测试覆盖 Character、Weapon、SpecialAttack、OtherObject 的 frame/runtime 原子一致和 256 次对象池复用零分配；完整 self-check 于 `2026-08-13 01:19:16` PASS；
- 第八切片 1000 AI 采用相邻交错顺序运行 30 warmup + 180 sample：新默认路径两次 average/P95/P99/max 分别为 `20.4753/24.6792/26.6512/27.8427 ms` 与 `20.5460/24.7788/26.7887/28.0238 ms`；强制旧 12 字段回拷两次分别为 `22.2049/27.1037/32.6939/37.6781 ms` 与 `21.3191/25.3920/26.7819/28.5101 ms`。四次正式 tick 均为 0 B、Gen0/1/2 collection 0、final hash 均为 `752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`、hard breach 0、teardown 完整恢复；因此确认删除回拷是行为等价且稳定为正的优化，但短样本仍有 backlog/drop，不能代替 U9 60 秒以上正式矩阵；
- 第九切片首先实现 26 字段 frame/motion Direct-SoA 兼容双写。初版两次 1000 AI average/P95 为 `22.5875/26.9588 ms` 与 `22.7249/27.2482 ms`；即使将值直接随通知传递、合并整数坐标提交并缩短调用链，仍为 `22.2595/27.0605 ms` 与 `22.2906/26.3677 ms`。相对第八切片约 `20.5 ms` 的相邻基线属于稳定负收益，根因是无生产 reader 的字段也在每个原写点重复双写，因此完整 26 字段候选没有保留；
- 第九切片最终缩小为 same-tick AI row 实际读取的七字段 projection：`XInt/YInt/ZInt/Vx/Facing/Frame/HitStop`。store 由 world 注册/释放/reset/grow 管理，以 slot + generation 绑定；runtime 只保存非序列化的已验证 store/slot 绑定，释放后旧实体无法污染复用槽。AI unified authority 刷新从该 store 读取七字段，其余字段仍按现有 owner 读取，不能扩大解释为完整 frame/motion canonical world；
- 第九切片 fresh 证据：Unity 0 C# compile error；联合 EditMode job `f107df121c0a4f91a06b15a39388b75b` 为 `179/179 PASS`，覆盖七字段同步、generation ghost 与 256 轮热写零分配；完整 self-check 于 `2026-08-13 02:07:24 PASS`；两次 1000 AI 30 warmup + 180 sample 的 average/P95/P99/max 分别为 `21.0860/25.3055/27.7991/28.5208 ms` 与 `21.4329/26.0455/29.6440/33.2103 ms`。两次正式 tick 均为 0 B、Gen0/1/2 collection 0、final hash `752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`、hard breach 0、teardown `restored=true`；该结果只允许保留最小 projection，不宣称性能提升或关闭 U9；
- 第十切片复用既有 `BattleCharacterInputStore` 增加最小 AI projection reader，使 unified row 的 `InputHistoryGate/CachedTargetSlot/CoordinateTargetX` 从 generation-owned input state 读取，不再从 Runtime 镜像读取；没有新建第二份状态，也没有增加写入或改变发布后 hard-breach 规则。fresh Unity 编译 0 C# error；EditMode job `0c6f6b30b779423884423264427be29a` 为 `179/179 PASS`；完整 self-check 于 `2026-08-13 02:16:39 PASS`；1000 AI average/P95/P99/max 为 `21.6311/25.6692/27.6213/28.7059 ms`，正式 tick 0 B、Gen0/1/2 collection 0、final hash 不变、hard breach 0、teardown `restored=true`；
- 第十一切片新增低频 `BattleRelationLinkStore`，把 `RelationTeam/LinkState/KillCount/TargetSlotIndex` 的既有所有写入通过 runtime 兼容属性同步到 generation-owned Direct-SoA，并令 unified row 从该 store 读取。fresh EditMode job `8a46cf71e9de4205b96a62282f91248f` 为 `182/182 PASS`；完整 self-check 于 `2026-08-13 02:30:12 PASS`；1000 AI average/P95/P99/max 为 `21.6142/25.5854/27.8450/29.8295 ms`，0 B、0 次 collection、hash 不变、hard breach 0、teardown 完整恢复；
- 第十二切片新增 `BattleVitalStore`，把 `HP/HPBound/HP3/PP` 的 `LF2Health` 与所有直接 Runtime 写入按原写点同步到 generation-owned Direct-SoA。两次 1000 AI average/P95 分别为 `22.0395/26.2780 ms` 与 `21.7829/25.6845 ms`，相对前序样本没有形成稳定显著负收益，故保留所有权迁移但不宣称性能提升；
- 第十三切片令 `LF2FrameInfo.D` 在所有直接 DAT frame data 替换点同步整数 `state` 到既有 frame/motion store，避免逐一补约 30 个赋值点，也不创建委托或每帧对象。unified row 的最后一个实体字段读取 `entity.GetState()` 已改为 store projection；frame/motion projection 因此从七字段扩展为八字段。最终 fresh Unity 编译 0 C# error；EditMode job `f8cffb8409c241e3be0a973f684b73b7` 为 `185/185 PASS`；完整 self-check 于 `2026-08-13 02:43:16 PASS`；1000 AI average/P95/P99/max 为 `22.0254/26.1165/29.5433/30.4045 ms`，正式 tick 0 B、Gen0/1/2 collection 0、final hash `752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`、authority success、hard breach 0、teardown `restored=true`；
- 第十四切片新增 world-owned、无静态可变状态的 `BattleAiUnifiedRowPublisher`。`BattleCharacterInputStore`、`BattleFrameMotionStore`、`BattleRelationLinkStore` 与 `BattleVitalStore` 的原写入网关只暂存该 slot + generation 的 dirty 最终值；到同一个实体的 post-CharacterInput 可见边界时，`TryCommitPending` 一次提交发生变化的字段并按旧值/最终值判断 role/team 派生产品是否需要重建。没有 pending 的实体只做常数级 generation 检查，不再重读和复制全部 19 个 projection 字段；
- 立即在原写点直接修改已发布 row 的第一个候选改变了同 tick 可见边界，1000 AI average 还回退到 `42.2450 ms/tick`，已完整撤回。保留实现以 staged dirty + 边界提交恢复权威顺序；强制 full refresh 先构造完整 oracle row 再丢弃 pending，默认关闭的增量 shadow 会从四个 canonical store 重读 19 字段并逐项对照，但不写正式 row；
- 第十四切片 fresh 证据：Unity 编译 0 C# error；EditMode job `33cc0f620af24c4ba48e3b7a5c4fc3cd` 为 `185/185 PASS`，其中 30 tick × 4 实体的增量 shadow 共执行 120 次；完整 self-check 于 `2026-08-13 03:20:07 PASS`；1000 AI production smoke average/P95/P99/max 为 `21.5776/25.7700/28.6098/29.9695 ms`，正式 tick 0 B、Gen0/1/2 collection 0、final battle parity hash `752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`、authority success、hard breach 0、teardown `restored=true`；
- 强制 full refresh oracle 的 average/P95/P99/max 为 `22.7332/26.9240/29.9166/33.4480 ms`，与增量路径的 battle parity hash 完全一致且同为零 GC。capacity detail 中 `CharacterInput/AI/UnifiedSnapshotExecutionRowRefresh` 的 average/P95 为 `0.3527/0.3621 ms`，前序基线为 `0.3618/0.3704 ms`；这证明结构迁移未形成明显负收益，但样本差距很小，不宣称显著性能提升；
- 第十五切片把正式 UnifiedAuthority 每 tick 初始 row 的 19 个战斗字段改为从四个 canonical store 捕获。第一版仍以 Runtime 引用分别解析四次 owner；最终版直接复用 registry 已验证的 `RuntimeEntityHandle`，每个 store 只做 slot 范围与 generation 校验，旧 generation 在 slot 复用后会被四个 reader 全部拒绝。Legacy、shadow 和强制 full oracle 仍保留原 Runtime 捕获，作为独立对照；
- 压力报告新增 `aiUnifiedSnapshotExecutionCanonicalInitialCaptureCount`，authority exact-closure 同时要求它等于 committed pass × requested entity count；pre-commit rollback 则要求等于实际 committed pass × entity count。fresh EditMode job `1537682b724944f0ad4838ee2fe890f9` 为 `421/421 PASS`，覆盖四个 handle reader 的当前 generation 成功与复用 generation 失败；完整 self-check 于 `2026-08-13 03:48:50 PASS`；
- 第十五切片最终两次 1000 AI average/P95/P99/max 分别为 `21.8961/25.8188/29.3556/30.4394 ms` 与 `21.7277/26.1409/29.4532/30.1781 ms`。两次均为 sampled tick 0 B、Gen0/1/2 collection 0、final battle parity hash `752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`、canonical initial capture `209000/209000`、authority success、hard breach 0、teardown `restored=true`。两轮均值相对第十四切片约 +1.1%，属于 Editor 短样本波动范围，因此只保留所有权迁移，不宣称性能提升或回退；
- 第十六切片把两套 directional boundary 编码迁到 input store/publisher；第十七切片新增 `BattleIdentityStore`，按 slot + generation 持有 `StableId/ObjectId/DataObjectType`，并将 UnifiedAuthority 初始 identity/type 捕获改为 handle reader。`LF2Entity.ObjectId`、`LF2FrameCache.Load/Clear` 与 dormant partner reset 恢复都在原写点同步该 store；旧 generation 不能污染复用槽；
- 第十七切片 fresh Unity 编译 0 C# error；EditMode job `4105b6d6db164f47be4d66c875624645` 为 `423/423 PASS`；完整 self-check 于 `2026-08-13 04:32:40 PASS`。1000 AI 增量路径 average/P95/P99/max 为 `22.0894/26.2908/28.6278/29.6332 ms`，强制 full Runtime oracle 为 `23.0996/27.4190/30.4386/31.8757 ms`；两者 sampled tick 0 B、Gen0/1/2 collection 0、battle/lockstep hash 相同、canonical initial capture `209000/209000`、hard breach 0、teardown `restored=true`；
- 第十八切片把 UnifiedAuthority first-ten move-mode 初始构建从 `LF2Entity/Runtime` 二次读取改为复用同一 canonical row；post-input 失效检查从 identity store 与 publisher 已提交 row 读取当前 type/HP/X/Z，保持同 tick DAT 身份变化和运动/生命变化的旧失效语义。fresh EditMode job `3410e3a3d4564fa588a3c4a30cf6cfdb` 为 `423/423 PASS`；self-check `2026-08-13 04:43:39 PASS`；1000 AI 增量/full oracle average 为 `21.8479/23.1703 ms`，两者零 GC、hash 相同、hard breach 0、teardown 完整恢复；
- 第十九候选尝试给 `CharacterInput` 单独缓存 canonical active slots，两次 average 为 `22.5868/22.4815 ms`，没有优于第十八切片基线的稳定证据，已完整撤回。第二十候选对 exact Character 的 FrameAdvance 跳过空 `SimTU` 与重复 snapshot，1000 AI 中真实命中 `210000` 次，但默认/强制 Legacy average 仅为 `21.6491/21.6680 ms`，差约 `0.09%`，同样完整撤回；
- 两项负实验撤回后的 fresh Unity 编译 0 error；EditMode job `88cbeb995d2141b2a7d0ff5117a065eb` 为 `423/423 PASS`；self-check `2026-08-13 05:35:09 PASS`；1000 AI average/P95/P99/max 为 `22.1189/26.6252/29.6743/34.9116 ms`，正式 tick 0 B、Gen0/1/2 collection 0、battle/lockstep hash 不变、teardown `restored=true`；
- 第二十一至第三十切片继续以小切片关闭已证明的重复对象式工作：包括 StageBounds/FrameAdvance/CollisionSnapshot 的 exact writer 窄化、Late pass-stable opoint factory、CharacterInput progress/projection dirty commit、UnifiedAuthority 跨 tick rolling row，以及 CandidateCollect 单一参与者缓冲；每项的权威边界、A/B、负实验与 fresh 门禁均记录在 `Docs/unified-battle-u6-canonical-field-cluster-inventory-20260812.md`；
- 第三十一候选把 CandidateCollect 几何读取改为 `BattleFrameMotionStore` handle reader，三轮 1000 AI total average 为 `22.3761/22.7185/22.7246 ms`，没有稳定降低 `CandidateCollect` 或 `ParticipantBodyItrBuild`，相邻基线为 `21.9110/21.8486 ms`，故完整撤回；
- 第三十二切片新增 `BattleEcsCharacterPreFrameBoundsPass`。exact 正式角色按权威 C# 只写 X/Z/XInt/ZInt，派生/未知类型 fail closed 到原虚方法与宽 snapshot。两轮交错 A/B 中 `PreFrameBounds` average 从 Legacy `0.9075/0.9277 ms` 降到 DataOriented `0.3697/0.3533 ms`，两套 hash 一致、正式 tick 0 B、三代 collection 0、teardown 和诊断模式恢复成功；
- 第三十二切片 fresh 门禁：本地 runtime/editor build 0 error；聚焦 job `46aad3be387844d09746bf6c2ec04267` 为 `6/6 PASS`；压力工具整类 job `3240b4675407415fa0f7a00acfaa33dd` 为 `240/240 PASS`；完整 EditMode job `0be644482d0440a98883c66285c64376` 为 `1090/1090 PASS`；self-check `2026-08-13 13:36:01 PASS`；
- 完整证据见 `Docs/unified-battle-u6-ai-input-writer-20260812.md` 与 `Docs/unified-battle-u6-canonical-field-cluster-inventory-20260812.md`。post-CharacterInput 的 19 字段全量复制以及初始 snapshot 的战斗字段、boundary、identity/object type、first-ten move-mode 对象热读取均已移除；实体边界遍历、派生索引维护、完整 frame/motion/lifecycle canonical world 与对象 shell 热循环仍未闭合。U3 的 tick-end `BattleEcsWorld.Input` 仍只用于诊断，不能冒充 canonical store。因此不得声明 U6 或 U9 已完成。
- 第六十切片复用 `PostFrameAdvanceDeathCleanupAll` 已有活动实体遍历，在 respawn/refresh 完成后逐参与者证明 PreInteraction 中性，并发布 tick、logical capacity、claimed count、occupancy epoch、pending-destroy epoch 与 pending-unregister count 约束的只读产品；后续 StageBounds 只改 Z，不会破坏 cpoint/link/held 中性条件。`PreInteractionTickAll` 仅在所有约束仍成立时 O(1) 跳过原全槽证明，任一结构变化、未知派生类型或非中性角色均 fail-closed 到原实现；pass 顺序、slot 顺序和写入边界不变。
- 第六十切片两轮同种子 A/B 的 candidate/Legacy `PreInteraction` average 为 `0.6882/0.8584 ms`，`DeathCleanup + PreInteraction` 为 `1.0517/1.2208 ms`，完整逻辑 tick 为 `18.6477/18.9709 ms`；改善约 `19.83%/13.85%/1.70%`。四轮 workload、parity/lockstep hash 完全一致，正式 tick 0 B、Gen0/1/2 collection 0、teardown 完整恢复。关闭 phase/presentation/detail 后的 120 warmup + 300 sample 回归为 average/P95/P99/max `15.7364/17.7918/19.1523/19.3988 ms`，236 tick 使用跨 pass 证明。
- 第六十切片 fresh 门禁：runtime/editor 串行构建 0 error；PreInteraction 聚焦 job `15a98b8e88f34f06a456e50c60933533` 为 `14/14 PASS`；压力工具整类 job `a748ed067f8c4365911ff3b623808da9` 为 `247/247 PASS`；完整 self-check 于 `2026-08-15 01:52:12 PASS`。详细证据见 canonical inventory 第 52 节。该切片不关闭 U6/U9；逻辑 tick 预算通过不能替代完整 PlayerLoop、Windows Player 60 秒矩阵、snapshot/restore 与专用 worker 阶段。
- 第七十五切片重新实现并晋升了严格限于 exact `LF2Character + AI + Character DAT` 的 `BattleEcsCharacterInputPass`；human、未知派生类型、非 Character DAT 与不可读 runtime 都保持原兼容链或同义早退，不把 AI-only writer 误用到玩家输入。正式默认为 `DataOriented`，Legacy 仍仅作为 reset-boundary A/B oracle。相同 seed、专用 worker、30 warmup + 180 sample 的 Legacy/DataOriented 报告 average/P95/P99/max 分别为 `17.1991/21.1432/23.6634/24.5767 ms` 与 `17.1323/22.2910/23.2966/24.1557 ms`；两者 parity/lockstep hash 完全一致、正式 tick 0 B、三代 collection 0。该切片按所有权闭合、行为等价与无平均回退保留，不宣称可见 FPS 提升，也不把 human/派生兼容路径冒充已迁移。
- U6 当前闭环判断必须区分三类路径：已由 generation-owned store/world-owned writer 作为正式 owner 的热路径；仅保留 Unity shell/host 适配且不作为逻辑真值的兼容字段；以及经相邻 A/B 证明会回退、因而保留 Legacy 的低频/派生 fail-closed 路径。不能为了满足“全部 DataOriented”字样而重新晋升已否决的 `FramePostProcess`、`PostFrameTail`、hit frozen-plan 或其他负优化候选。剩余正式工作是完成这三类路径的可执行清单与退出门，并用 U9 Player 矩阵证明生产默认没有进入对象式 per-entity `MonoBehaviour.Update` 或每帧分配；在该审计和 U9 完成前，U6 仍不单独标完成。

### U7：生产 Snapshot、Restore 与跨运行时门禁

状态：已完成。`2026-08-20` 在国际版 `Unity 2022.3.62f3 (96770f904ca7)` 下重新生成的 Windows Mono 与 IL2CPP gate 均为 `Passed`，报告分别位于 `Temp/U7-Windows-IL2CPP/Mono/u7-runtime-report.json` 与 `Temp/U7-Windows-IL2CPP/IL2CPP/u7-runtime-report.json`。两套 Player 的 Unity 版本、平台、source/restored/replay checksum、恢复 slot、stable id、generation 完全一致；纯值 transfer/factory 与 restore + journal replay 均通过。相关回归包含在 fresh 全量 EditMode `1265/1265 PASS` 中，`BattleRuntimeSelfCheck` 于 `2026-08-20 12:14:11` fresh PASS。未来服务器 runtime 仍不在当前单机实现范围内。

> 2026-08-20 U7 fresh 关闭证据：Mono 与 IL2CPP 的 source/restored checksum 均为 `2f92a339254225de11790c2d4eb8fc51f36e7cdd6245a891d25f041ef17ac093`，replay checksum 均为 `3DEB30C4D190E5FB`，恢复 `(slot, stableId, generation)` 均为 `(3, 100, 1)`。门禁结束后 Burst 已恢复开启、Standalone 已恢复 IL2CPP、Frame Timing Stats 已恢复关闭。下方 2026-08-15 的版本混装说明仅保留为历史故障记录，不再代表当前状态。

> 2026-08-15 U7 工具链复核：Windows IL2CPP 模块已经安装，真实门禁也已完成 IL2CPP C++ 生成、MSVC 编译并产出
> `GameAssembly.dll`；阻塞不再是“模块缺失”。当前项目和 Editor 是中国版 `2022.3.40f1c1 (0bae6c114c78)`，但 Unity Hub
> 安装的四套 win32/win64 development/nondevelopment IL2CPP `UnityPlayer.dll` 全部是国际版
> `2022.3.40f1 (cbdda657d2f0)`。该 Player 启动时报告 `Expected version: 2022.3.40f1`、
> `Actual version: 2022.3.40f1c1`，在 U7 runtime bootstrap 前退出。Unity Hub 日志进一步证明：Hub 查询
> `2022.3.40f1c1` 时得到 `0 releases retrieved`，随后实际执行的安装包是
> `UnitySetup-Windows-IL2CPP-Support-for-Editor-2022.3.40f1.exe`。因此这是 Editor/Player 发行版混装，不是战斗代码失败。
> `BattleSinglePlayerRuntimeValidationBuild` 已增加每套 IL2CPP Player 精确 `ProductVersion` 预检；`2026-08-15 23:28`
> 在真实 Editor 再次执行 `NTSD/Battle Architecture/U7/Build And Run Windows IL2CPP Gate`，门禁按设计在进入
> `BuildPipeline` 前 fail-fast，并完整列出四套 `cbdda657d2f0` variation。必须安装与 `0bae6c114c78` 精确匹配的
> Windows Build Support (IL2CPP)，或用与 `cbdda657d2f0` 匹配的完整国际版 Editor 重新构建，才能关闭跨运行时
> checksum/restore/replay 门禁。用户已于 `2026-08-16` 明确只使用国际版，因此后续固定采用完整国际版 Editor
> `2022.3.40f1 (cbdda657d2f0)` 与同 revision Windows IL2CPP Player，不再寻找中国版模块；不能用替换 `UnityPlayer.dll`、
> 修改 `ProjectVersion.txt` 或版本字符串、通用 `il2cpp.exe` 或 Windows Mono 报告替代。IL2CPP Player 自身通过后，
> 同一菜单还会自动读取 Windows Mono 报告，对 Unity 版本、平台、source/restored/replay checksum、恢复 slot、stable id 和
> generation 做逐项跨运行时比较；因此后续不会把“两个 runtime 各自通过”误报成“两个 runtime 结果一致”。

- 完整 BattleStateSnapshot；
- snapshot restore + journal replay；
- FrameHistory、SnapshotRing 与 ChecksumHistory 的 frame/schema 生命周期一致；
- Editor Mono、Windows IL2CPP 与未来服务器运行时对比；
- Android ARM64 兼容合同保留，但真机结果由用户后续提供，不属于当前 Codex 验收；
- 仅对确认分叉的数值域制定定点迁移。

### U8：专用 Simulation Worker 与 60/120 Hz 表现

状态：已完成。生产接线、线程所有权、固定容量 input queue、双槽 publication、ack/finalize 与失败停机均由 fresh 全量 EditMode `1265/1265 PASS` 覆盖；`2026-08-20` Windows Player worker/synchronous 各 300 tick 正式对照的 overall/world/slots/aRest/vRest/RNG/input/stats/events/metadata parity 与 lockstep hash 全部一致。worker average/P95/P99/max 为 `4.2995/5.7216/6.3513/9.0103 ms`，同步为 `4.0190/5.2066/5.8594/8.4089 ms`；两者中央 draw=1、正式 tick 0 B、Gen0/1/2=0、U6 审计和 cleanup 均通过。报告见 `Temp/U9-Windows-Player/Reports-2022.3.62f3/u8-worker-combat1000-30x300.json` 与 `u8-sync-combat1000-30x300.json`。

- BattleKernel 移出 Unity 主线程；
- 固定所有权的输入队列与双缓冲 publication；
- 无共享可变 Unity 对象；
- 主线程只负责表现；
- 对移动端线程数和发热单独测量。

### U9：1000 AI 正式验收

状态：已完成 Windows Mono Player 正式验收。`2026-08-20` 当前代码重新构建后，Idle/Move/Dispersed/Combat/Concentrated 五场景均完成 300 tick 预热 + 1800 tick 正式采样，1000 个真实实体、AI/移动/碰撞/命中/opoint/生命周期及中央渲染均走生产路径。五场景逻辑 average/P95/P99/max 分别为 `3.4161/6.5989/12.1378/16.7198`、`2.7957/3.9235/4.9547/6.7520`、`3.9128/5.0592/5.9115/9.9178`、`3.8269/5.1214/6.3258/9.6252`、`4.2915/5.5420/7.4755/15.4023 ms`；完整帧平均约 `59.85～59.98 FPS`。SetPass 均为 6、中央 draw 均为 1、正式 tick/driver/presentation/PlayerLoop 均 0 B、Gen0/1/2 collection 0、正式容量拒绝 0、worker failure 0、cleanup 全恢复。Dispersed1000 与 Combat1000 均超过 30 FPS，单机 1000 AI / 30 FPS 容量目标关闭。fresh 报告位于 `Temp/U9-Windows-Player/Reports-2022.3.62f3/`，完整证据与边界见 `Docs/unified-battle-u9-final-acceptance-20260815.md`。

- Idle/Move/Dispersed/Combat/Concentrated 矩阵；
- Editor 趋势 + Windows Player 正式报告；
- 60 秒以上、P95、GC、SetPass、Render Thread、拒绝计数、checksum、cleanup；
- `Dispersed1000` 与 `Combat1000` 达到 30 FPS 后才关闭单机容量目标。

### S0～S9：服务器实施

状态：仅完成方案细化，代码均未开始。S0～S5 建立的是权威战斗服务器核心与独立运行边界，不等于已经完成登录、全国匹配、网关、持久化或运维后台；本节不是对服务器、Socket、ACK、Jitter Buffer、房间、登录或重连已实现的声明。

> **详细设计与后续留痕分离**：从 2026-08-24 起，本节保留冻结的上位阶段摘要和历史快照，不再作为每日实施台账或协议细节的维护位置。S0～S9 的唯一详细设计、失败处置与关闭合同见 `Docs/server-lockstep-s0-s9-design.md`；实际进度、证据、阻塞、问题和 Change ID 只追加到 `Docs/server-lockstep-s0-s9-progress.md`。不得在本文件重复写每日服务器实施流水，避免总览与执行台账漂移。

#### 外部案例评估后的保留与排除

`dudu502/LittleBee` 与后续 `littlebee_libs` 只作为服务器框架案例，不改变 C++ release live runtime 的权威战斗规则。可吸收的设计是：服务器在帧边界组装并排序命令、客户端/服务器/回放共享模拟入口、先做同进程本地服务器、使用快照加历史帧恢复，以及 Common/Client/Server 程序集分层。以下实现不得移入 NTSD：

- 用 `DateTime.Now + Thread.Sleep + while` 决定权威逻辑帧或进行无界追帧；
- 每 tick 深拷贝全部引用型 Component 并重建 `List/Dictionary` 快照；
- 用引用型 `Dictionary<Type, List<AbstractComponent>>` 取代已经验证的 slot + generation、SoA 与固定容量 ring；
- 信任客户端本地快照、把可靠 UDP 当成应用层 ACK/Jitter 合同，或用外部定点库改写已经对齐 C++ release live runtime 的战斗数值语义；
- 复制原始 LittleBee 代码。原仓库未提供明确许可证；如未来评估 MIT 的 `littlebee_libs`，也只能在许可证、归属和技术门禁全部满足后单独批准。

#### 单慢客户端不得无限阻塞全局的协议合同

- NTSD 不采用“每个权威 tick 无限等待所有玩家输入”的纯等待式 lockstep；服务器在每个 frame deadline 到达时必须锁定并广播完整权威帧，健康客户端不得因单个客户端的网络或设备卡顿无限停止；
- `StartBarrier` 固化全房间的 `InputDelayFrames`、frame deadline、连续缺失 grace、最大缺失时长和 `MissingInputPolicy`；这些参数不能由单个客户端在运行中改变，只能通过已广播、指定未来 tick 生效的 session policy transition 修改；
- 每个锁定帧必须记录每个 player slot 的输入来源：真实输入、重复幂等输入、短暂缺失填充、持续缺失 neutral、托管或断线；迟到输入永远不能改写已锁定帧；
- 缺失填充不得伪造新的 `pressed` 或 `released` 边沿。方向/持续按住是否允许短期沿用、连续缺失到 neutral/托管/对局规则的精确阈值，必须先由 C++ release live input trace 和产品规则共同确认；未确认前保持显式待决，不得凭常见网络做法写成战斗规则；
- 客户端长期落后时只允许其自身进入 grace、neutral、恢复或断线状态；恢复使用服务器 snapshot + 连续 authority frame replay，不能让其他客户端回退到该客户端过去的网络状态；
- 这条合同不覆盖服务器自身超预算：若 Battle Server 的固定 tick 不能在预算内完成，影响的是整个房间，必须由 S8 容量调度和扩容解决，不能误报为“慢客户端问题”。

#### S0：同进程权威服务器骨架

- 无 Socket、无网络库，使用显式内存 loopback；
- 同一进程创建一个服务器 BattleWorld 和至少两个客户端 BattleWorld；
- StartBarrier 固化 session identity、资源/规则 fingerprint、seed、roster 与 canonical player slot；
- StartBarrier 同时承载但不在 S0 调参的 session-wide input-delay/deadline/missing-input policy，保证后续所有客户端观察同一策略版本；
- 服务器和客户端都只能调用现有 `StepOneTick(FrameInputSet)`，不得创建第二套技能、伤害或生命周期循环；
- 每个服务器 tick 都产生完整、不可变、连续的权威帧，包括没有玩家操作的空帧；
- 验收要求：固定输入脚本下服务器与全部客户端连续 N 帧十域 checksum 一致，重复运行结果一致，单机 host policy 不受影响。

#### S1：应用层权威帧协议与组装器

- 定义 transport-agnostic 的 `InputSubmission`、`AuthoritativeFrameEnvelope`、`FrameAck`、`ServerProgress` 和稳定错误码；
- 服务器按 `(session, tick, canonical player slot)` 去重和排序，同一键第一次合法内容胜出，冲突内容进入证据而不能覆盖；
- 明确 future target tick、session-wide input delay、input deadline、缺失输入补齐原因、帧锁定、广播和历史入 ring 的原子边界；
- deadline 到达即锁定完整帧，不等待单个 player slot；`AuthoritativeFrameEnvelope` 为每个 slot 写入 `InputSource/FillReason`，迟到输入只能得到确定的接受到未来合法 target 或拒绝结果，绝不能回填历史帧；
- `MissingInputPolicy` 的短暂缺失与持续缺失状态必须分离；前者禁止伪造 pressed/released，后者必须具有 neutral、托管或按模式结束对局的明确且可审计策略；
- transport、RPC、Unity 类型和网络库对象不得进入 BattleKernel 或 `FrameInputSet`；
- 验收要求：乱序提交、重复提交、冲突提交、迟到提交、空输入、roster 变化和单 slot 缺失边界均得到确定结果；同一输入脚本和 policy version 必须生成相同 authority frame history。

#### S2：内存弱网、ACK 与 Jitter Buffer 状态机

- 在内存 transport 中可重复注入延迟、抖动、丢包、重复、乱序和暂时断流；
- 应用层维护 server/client sequence、ACK、冗余帧窗口、ready-frame 区间、缺帧请求和确认进度；
- 客户端只能连续消费已经 ready 的权威帧，不能跨洞推进，也不能用后到内容修改已锁定帧；
- 追帧必须同时受连续 ready 数量、单帧 CPU 预算和最大追帧数约束；单机 `OfflineLocal` 继续保持既定的一次外层 Update 最多自动推进一个 tick；
- 必须包含“一名客户端输入黑洞/极端抖动、其余客户端健康”的可重复场景：服务器 authority tick 和健康客户端的连续 confirmed tick 仍按 deadline 前进，只有故障客户端进入 grace/neutral/recovery；
- 验收要求：可重复弱网脚本下不丢失 deadline 前的攻击/技能边沿、不重复消费、不无限积压、不出现墙钟驱动的爆发式多 tick；单慢客户端不能造成健康客户端无限停帧。

#### S3：服务器权威快照、desync 与恢复闭环

- 服务器维护 `FrameHistoryRing`、`SnapshotRing` 和 `ChecksumHistory` 的统一 schema/session 生命周期；
- 客户端周期上报校验结果，服务器同核运行结果是权威，checksum mismatch 保存 witness 并进入明确状态机；
- 恢复包由服务器快照、快照 tick/checksum、后续连续权威帧和目标 tick 组成；客户端磁盘快照只能作为非权威缓存提示；
- 覆盖严重落后、desync、断线重连、观战加入和回放起点，但不在本阶段引入真实 Socket；
- 故障客户端恢复只能补齐其自身到当前 authority tick；其重连或 snapshot replay 不得修改健康客户端已经锁定、消费或确认的 authority frame；
- 验收要求：snapshot -> mutate/desync -> restore -> history replay 后，服务器与客户端最终十域 hash、slot/generation 和事件游标一致；单客户端恢复期间健康客户端持续推进。

#### S4：预测与回滚决策门

- A/B 测量 `ConfirmedOnly`、仅本地即时表现反馈、输入回显和有限本地玩家预测；
- 默认保持 `ConfirmedOnly`，只有输入延迟、回滚成本、错误预测率和表现收益达到预先批准阈值时才实现逻辑预测；
- 若启用预测，只允许在可恢复快照窗口内影响受控本地域，远端实体和战斗结果仍以权威帧为准；
- 音频、粒子、镜头和 UI 使用确认事件游标，不得因回滚重复播放；
- 预测不能被用作重新引入“所有人等待最慢玩家”或“迟到输入修改锁定帧”的理由；本阶段允许以“证据表明无需预测”关闭，不强制为了完整性实现 GGPO 式回滚。

#### S5：共享程序集与独立进程门禁

- 拆分 `Battle.Protocol`、`Battle.Kernel`、`Battle.ClientAdapter` 和 `Battle.ServerHost`，保持单向依赖；
- BattleKernel 和协议程序集不得引用 `UnityEngine`、GameObject、Transform、Renderer 或 transport 实现；
- 启动 headless/.NET 同核服务器进程，复用相同 schema、factory、checksum、snapshot 与 `StepOneTick`；
- Mono、IL2CPP 与服务器 runtime 使用同一固定 journal 做跨运行时对照；
- 验收要求：进程内与独立进程结果一致，序列化往返不改变排序、默认值、generation 或 checksum。

#### S6：真实 transport 选择与接入

- 只有 S0～S5 关闭后，才按移动端支持、可靠/不可靠通道、拥塞、MTU、分片、加密接线、维护状态和许可证选择 UDP/KCP/ENet/LiteNetLib 或其他实现；
- transport 只承载 S1 已冻结的应用层消息，不拥有 tick、ACK 语义、Jitter 状态或 BattleWorld；
- 先在 localhost/局域网验证，再进入公网和移动网络；
- 更换 transport 的 A/B 必须复用同一弱网脚本、权威帧记录和 checksum witness。

#### S7：真实弱网、断线重连与长时稳定性

- 在真实 transport 上注入延迟、抖动、丢包、重复、乱序、短断流、长断线和客户端进程重启；
- 验证重连身份、session/fingerprint、快照选择、历史补发、追帧预算、事件去重和失败降级；
- 覆盖前后台切换、移动网络切换、暂停恢复和长局内历史截断；
- 必须在真实公网验证单一客户端持续高延迟、输入黑洞、设备卡顿和断线时，其余健康客户端不发生无限等待；同时记录该故障客户端进入 grace/neutral/recovery 的时刻与最终恢复/结束结果；
- 正式窗口继续要求 BattleKernel/协议热路径无非预期分配、无 Gen0/1/2 collection，并保存 P50/P95/P99、带宽、恢复时间和单慢客户端影响范围。

#### S8：多房间调度、安全与可观测性

- 建立控制面 `Gateway/Auth -> Matchmaker -> Room Allocator -> Battle Server`；它只处理身份、队列、区域/延迟策略、roster、房间分配与连接令牌，绝不拥有 `StepOneTick`、伤害、命中或战斗结果；
- 初期允许单地域公网部署；全国服务再按延迟、容量和故障域扩展多个 Battle Server 区域，客户端通过稳定域名进入 Gateway，不在客户端硬编码单台 Battle Server IP；
- 明确一个 worker/process 承载多少房间，房间隔离、崩溃域、资源上限、背压和优雅停机；
- 建立 tick lag、ready depth、jitter depth、rollback/restore、checksum mismatch、带宽、快照大小和容量拒绝指标；
- 将“服务端 tick 超预算”和“单客户端缺失输入”分开监控、告警和扩容：前者影响整房间，后者只能进入该 player slot 的降级状态，不能被同一指标或同一补救策略掩盖；
- 服务器验证输入合法性、频率、玩家所有权和 session identity；客户端上报结果不能成为战斗权威；
- 回放和 checksum witness 可审计，敏感数据、日志和快照有保留/清理策略；
- 容量测试分别覆盖多房间常规负载与单房间高实体负载，不用平均值掩盖最坏房间。

#### S9：服务器阶段最终验收与发布门

- 完成确定性、弱网、恢复、长时 soak、进程崩溃、容量、协议兼容和升级/降级矩阵；
- Windows 客户端 Mono/IL2CPP 与服务器 runtime 的固定 journal、snapshot、restore/replay 和 checksum 全部一致；
- 正式矩阵必须包含一名持续慢客户端、短断流客户端、长断线客户端和服务器超预算的四类故障；前三类不得让健康客户端无限等待，最后一类必须被容量/降载/拒绝新房间策略明确检测与处置；
- 以目标部署平台的正式 Player/headless 构建报告为准，不用 Editor 或 simulation-only 数据替代；
- Android 真机仍由用户执行并提供结果，除非后续明确授权 Codex 负责；T8 默认 `stage.dat` 继续排除；
- 只有 S0～S9 的 fresh 证据齐全，才能声明服务器帧同步阶段完成。

服务器阶段的硬顺序为 `S0 -> S1 -> S2 -> S3 -> S4 -> S5 -> S6 -> S7 -> S8 -> S9`。S0～S3 先证明权威帧、应用层协议、弱网状态机和服务器恢复合同；S4 单独决定预测边界；S5 完成进程拆分后，S6 才允许选择真实 transport。任何阶段都不得反向创建第二套战斗循环，或用网络/状态覆盖掩盖 BattleKernel 分叉。

## 20. 每阶段统一验收门

每个阶段只有同时满足以下条件才能标记完成：

1. 实现边界和 canonical owner 已记录；
2. Unity 脚本编译 0 error；
3. 聚焦测试 fresh PASS；
4. 完整 `BattleRuntimeSelfCheck` fresh PASS；
5. Authority400 对照 fresh PASS；
6. 输入、RNG、slots、aRest、vRest、stats、events、overall hash 一致；
7. 目标性能 A/B 使用同一 seed、负载和采样口径；
8. 正式窗口无非预期分配、扩容或 pool rejection；
9. cleanup 后 world、GameObject、slot、pool 和 host policy 恢复；
10. 专项文档与本文状态一致。

必须区分：

- 代码已写；
- 编译通过；
- self-check 通过；
- Authority400 对照通过；
- 1000 AI 性能通过；
- Play Mode 目标场景通过；
- 对应阶段已完成。

不得用隔离编译、单个 hash、短样本或 simulation-only 结果扩大成完整完成。

## 21. 回退与故障策略

- shadow/read-only 功能可以在测试启动前切换；
- canonical writer、allocator、query 和 snapshot schema 只能在 ResetWorld/合法 restore 边界切换；
- 运行中的 BattleWorld 不支持随意热切换数据所有权；
- checksum 分叉立即停止晋升，保留 witness，不继续叠加优化；
- 任何优化若三轮同口径 A/B 的 median 与 P95 改善不足 10%，原则上不提升为默认，除非它关闭了正确性、GC 或结构性风险；
- 发现 capacity fault、pool rejection、候选丢失或事件序列变化时视为失败，不以帧率改善覆盖；
- 旧路径只保留到新路径获得充分证据，避免永久双维护。

## 22. 当前不阻塞实施、但必须后续测量的决策

以下事项现在不写死：

- 未来 transport 使用 UDP/KCP/ENet 或其他方案；
- 正式 input delay、frame deadline、缺失输入 grace/neutral/托管切换点、history 和 snapshot 周期；
- 是否实现客户端预测与回滚；
- PC 每局默认扩展容量；
- unmanaged store 或 SIMD 是否值得引入；
- 客户端专用 simulation worker 在各移动设备上的线程与热预算；
- 哪些已证明跨运行时分叉的字段迁移到定点数。

这些决策不阻塞 S0：真实 transport 延后到 S6 选择，input delay/frame deadline 在 S1～S2 用内存弱网测量，预测与回滚在 S4 单独决策。必须根据新鲜测量和确定性证据决定，不能凭通用 ECS 或网络经验直接写死。

## 23. 明确禁止的网络与恢复设计

以下做法不进入 NTSD 方案：

1. 丢失攻击或技能输入后直接忽略，不做冗余、ACK 或补发；
2. 同一玩家同一权威帧以后到内容覆盖先到内容；
3. 已锁定或已广播帧被迟到输入修改；
4. 客户端上报命中、伤害、HP 或位置作为战斗权威结果；
5. 服务器另写一套伤害/技能规则，与客户端 BattleKernel 并存；
6. 正常战斗周期性用状态包覆盖位置、HP、Buff 来掩盖 desync；
7. 用多数客户端 checksum 投票代替服务器同核运行；
8. 把传输加密当作客户端不会作弊的证明；
9. 信任客户端磁盘快照作为权威恢复状态；
10. 用动态 dt、无限 while 追帧、AI 降频或跳过战斗 pass 处理性能不足；
11. 把 FPS/Source 式 lag compensation 或 UE 状态复制直接混入基础 lockstep；
12. 在 S6 前绑定具体网络库，或让 RPC/transport 类型进入 BattleKernel；
13. 每 tick 深拷贝整个引用型 Component 世界并重建 `List/Dictionary` 作为生产快照；
14. 用 transport 的可靠通道代替应用层 sequence、ACK、Jitter、deadline、冲突和幂等合同；
15. 为套用外部帧同步框架而替换已经通过 C++ release live trace 对照和跨运行时门禁的战斗数值语义。

完整来源与理由见 `Docs/lockstep-knowledge-base-audit.md`。

## 24. 建议执行定调

建议批准以下定调后开始 U0：

1. 战斗逻辑仍以 C++ release live runtime 为唯一规则依据；C# 只用于历史移植辅助与交叉检查。
2. 使用 NTSD 专用“Direct SoA + Bitset + Sparse Set + Pool/Ring + Loose Quadtree”混合 ECS。
3. 不使用 Unity DOTS，不实现通用 Archetype ECS。
4. 新内核不新增 partial，不使用全局可变 static 保存战斗会话状态。
5. 单机、回放、客户端和服务器共享唯一 `StepOneTick(FrameInputSet)`。
6. 逻辑固定 30 Hz；网络包频率和渲染频率独立。
7. 先完整执行 U0～U9，完成单机确定性、表现边界和 1000 AI / 30 FPS；U9 验收后停下并等待用户确认是否进入 S0。
8. 按 U0～U9 小步迁移，不进行一次性重写。
9. T8 默认 `stage.dat` 与 Android 真机继续排除。
10. 正常战斗输入同步与恢复快照同步严格分开，权威帧锁定后不可变。
11. U0～U9 只保留服务器所需接口边界，不实现服务器业务、ACK、Jitter Buffer、房间、登录或重连，也不选择网络库。
12. 用户批准进入服务器阶段后按 S0～S9 推进；S0 只做无 Socket 的同进程内存 loopback，S0～S5 依次证明权威帧、应用层协议、弱网、恢复、预测边界和进程拆分，S6 才选择 transport，S7～S9 完成真实弱网、多房间和最终发布验收。

用户确认该定调后，第一个实际执行批次是 U0：审查并验证当前工作树中已经存在的候选修改，建立可重复基线，而不是继续叠加新的 ECS 代码。
> U6 2026-08-13 20:24 更新：第三十七切片把 `ProcessReleaseInput` 使用的
> `LF2CharacterActionResolver` 从“每个角色构造时常驻一份”收敛为
> `SimulationWorld.BattleCharacterActionWriter` 持有的一份可复用 resolver；每次调用只在
> `try/finally` 边界内绑定当前角色，禁止重入并在退出时清空引用。未注册到 world 的测试、预览和
> 自定义角色仍按需懒创建兼容 resolver，不改变正式 world 的 slot 顺序、输入消费、RNG、帧跳转或
> 写回位置。fresh 本地 runtime/editor 构建均为 0 error；聚焦 job
> `8409630e113c450f979c46ad536f43df` 为 `32/32 PASS`；完整 EditMode job
> `ebc240ebbe664368973a08b210a64ed1` 为 `1104/1104 PASS`；
> `BattleRuntimeSelfCheck` 于 `2026-08-13 20:18:43 PASS`。真实 1000 AI 报告
> `Temp/NTSD_ProductionEntityStress.combat1000.u6-world-release-resolver-final-20260813.json`
> 为 1000/1000 实体、30 warmup + 300 sample、average/P95/P99/max
> `18.7018/24.5418/27.7835/34.6062 ms/tick`，正式采样 tick `0 B`、
> Gen0/1/2 collection `0`，parity hash
> `7a5d8f11482c98c7293487b219e52b2cd6aa6ea545ab917f42f9541ab0d21de9`
> 与 lockstep hash
> `f26bcb9b23f0b8e2381ef09f580e47402b1c9232304b38299f33e2c110a338ef`
> 均与相邻基线一致，teardown 完整恢复。该切片按“去除 1000 份角色常驻辅助对象、统一
> world 所有权、行为等价”证据保留；average 相邻差为 `+0.0514 ms`，不宣称 FPS 收益，
> 也不关闭 U6/U9。S0、T8 默认 `stage.dat` 与 Android 真机仍排除。
>
> U6 2026-08-13 22:13 更新：第三十八候选把 exact AI 的完整 CharacterInput 入口搬入
> `BattleCharacterActionWriter`，但没有删除内部 AI kernel、combo、release 或 canonical commit；1000 AI
> average/P95 为 `22.0234/27.5931 ms`，`CharacterInput 5.8815 ms` 与基线等价，因此候选已完整撤回，
> 报告保存为 `Temp/NTSD_ProductionEntityStress.combat1000.u6-full-ai-phase-relocation-negative-20260813.json`。
> 第三十九切片随后把 Character FrameAdvance 中没有任何生产 consumer 的 Unity
> `GroundPixelToWorld/groundPlanePos/visualYOffset/grounded` 物化移出正式热循环，exact character 与共享
> character-DAT shell 只消费值类型 `BattleMechanicsStepResult`，公开兼容 `Step` 仍保留。两轮 1000 AI
> `FrameAdvance/Transit` average 从相邻 `1.2068 ms` 降至 `0.7405/0.7599 ms`，整个 FrameAdvance 从
> `2.0173 ms` 降至 `1.5303/1.5498 ms`；两轮 logic average/P95 为
> `21.7884/27.2301`、`21.9666/27.3381 ms`，正式 tick 0 B、三代 collection 0、hash 不变、teardown
> 完整恢复。fresh 本地两套工程 0 error，聚焦 `5/5 PASS`；完整 EditMode `1106/1106` 中 3 个失败均为
> MCP disposed/Unity 随机 mesh assert 日志污染，受污染目标独立 `2/2` 与 `42/42 PASS`；
> `BattleRuntimeSelfCheck` 于 `2026-08-13 22:13:31 PASS`。本切片关闭的是一处 Unity 表现计算进入逻辑
> 热循环的问题，不关闭 U6/U9；S0、T8 默认 `stage.dat` 与 Android 真机仍排除。
>
> U6 2026-08-13 22:52 更新：第四十切片继续依据权威 `BattleCore/Frame/Physics.cs` 收窄正式角色
> FrameAdvance。registered exact `LF2Character` 不再每 tick 查询 sprite catalog 或物化没有生产 consumer
> 的旧 `SpriteX/SpriteY/SpriteZ`；公开兼容 `CharacterMechanics.Step`、未注册对象、未知派生角色和共享
> character-DAT shell 仍保留 adapter 行为，特殊攻击自己的 `PS.sx/sy/sz` 路径不变。两轮相同 seed 的
> 1000 AI 报告中，`Transit` average 从相邻 `0.7599 ms` 降至 `0.4872/0.4862 ms`，整个
> `FrameAdvance` 从 `1.5498 ms` 降至 `1.2468/1.2433 ms`；logic average 为
> `21.5163/21.5075 ms`。两轮正式 tick 均为 0 B、三代 collection 0、battle/lockstep hash 相同、
> authority success、teardown 完整恢复。fresh 本地 runtime/editor 顺序构建 0 error，聚焦 job
> `42d562c9abde436b9a36e0fcb82ed9db` 为 `6/6 PASS`，`BattleRuntimeSelfCheck` 于
> `2026-08-13 22:52:14 PASS`。完整 EditMode 执行 `1107/1107`，3 项均被 UnityMCP
> `NetworkStream disposed` 外部日志污染；相关目标复测没有已知代码断言失败，但不能写成干净全量 PASS。
> 该切片仍不关闭 U6/U9；S0、T8 默认 `stage.dat` 与 Android 真机仍排除。

> U6 2026-08-14 02:22 更新：第四十三切片把 role-aware CandidateCollect 的“排序后去重压缩”与
> “按唯一 pair 构建双向 exact requirement”融合为一次已排序扫描，删除第二轮完整 pair 遍历；participant、role、
> fallback、pair 顺序、双向候选消费、RNG 与生命周期不变。Legacy A / 融合 B / 融合 C / Legacy D 的目标
> `SortDeduplicate` 双轮均值从约 `0.201555 ms` 降至 `0.188994 ms`，约减少 `6.2%`，但完整 CandidateCollect
> 和总 tick 没有稳定可归因差异，因此只按微小局部正收益保留，不宣称 FPS 修复。fresh 本地编译 0 error；聚焦碰撞
> `9/9 PASS`；完整 NTSD EditMode 执行 `1108/1108`，其中 5 项仅被 UnityMCP `NetworkStream disposed` 外部日志污染，
> 受影响类独立 `5/5 PASS`；`BattleRuntimeSelfCheck` 于 `2026-08-14 02:20:28 PASS`。最终关闭全部细粒度诊断的
> 1000 真实 AI 报告 `Temp/NTSD_ProductionEntityStress.u6-slice43-final-performance.json` 为 30 warmup + 180 sample，
> average/P95/P99/max `17.8294/22.6115/24.5578/28.1476 ms/tick`，约 `56.09/44.23 logical tick/s`，
> 全部 tick 低于 30 Hz 预算，正式窗口 0 B、三代 collection 0、hash 不变、teardown 完整恢复。下一步继续审计
> `CharacterInput` 与 `LateEntityUpdate` 的完整重复数据产品；U6/U9、S0、T8 默认 `stage.dat` 与 Android 真机边界不变。

> U6 2026-08-14 10:07 更新：中央表现链新增默认关闭的细分诊断，把约 3000 条命令的 materialize、资源解析/
> quad 写入、chunk upload 与 submission publish 分开计时。同 seed 的 1000 个真实生产 AI 中，基线
> `Materialize/Mesh/ResolveAndWriteCommands` average/P95 为 `2.8966/4.0081 ms`，而 chunk upload 仅
> `0.0537/0.0801 ms`，确认 SetPass 已降到 5 后的剩余表现 CPU 热点不是 Mesh 上传，而是逐命令解析与
> quad 写入；`Materialize/BuildCommands` 仍约 `2.2658/2.9553 ms`。本切片随后为四种正式命令类型增加仅
> 缓存已通过 catalog trusted-cache 验证之资源身份的 4 槽热缓存；颜色仍逐命令应用，render-state、逻辑资源
> kind 与 `Configure()` 资源销毁/换代边界仍 fail-closed。详细诊断下该目标段降至 `2.7676/2.9419 ms`，完整
> presentation publish 从 `5.4605/7.3433` 降至 `5.3583/5.8052 ms`；该候选只是小幅尾延迟优化，不是整体
> FPS 修复。关闭所有 timing 后两轮 logic average/P95 为 `19.1597/23.5574` 与 `18.5654/22.8316 ms`，
> 方向受 Editor 抖动影响，第二轮与旧基线 `18.5834/23.1075 ms` 基本相同。全部运行正式 tick、PlayerLoop 与
> presentation 分配均为 0，Gen0/1/2 collection 为 0，parity/lockstep hash 不变，teardown 完整恢复；本地
> runtime/editor 编译 0 error，resolver 聚焦 job `7634b3328afe4f42addd0ec54deb05f7` 为 `22/22 PASS`。
> 因此只按已证明的解析子段/P95 与资源失效安全性保留，不宣称可见帧率稳定提高；U6/U9 仍未完成，下一批继续
> 寻找能删除完整 command materialization 或逻辑 tick 外重复主线程工作、且不改变表现像素与战斗真值的候选。
>
> U6 2026-08-14 13:24 更新：第四十五候选尝试由 Entity command 的 trusted `BattleSpriteEntry` 直接读取
> `CentralBinding`。候选虽先通过 resolver 聚焦 `22/22 PASS`，但详细 A/B 中 `ResolveCommands` average 从
> `1.3815 ms` 回退到 `1.5441 ms`，`ResolveAndWriteCommands` 从 `2.9161 ms` 回退到 `3.1304 ms`，完整
> presentation 从 `5.2677 ms` 回退到 `5.4886 ms`，因此候选代码、计数器和测试期望已完整撤回。撤回后本地
> runtime/editor 构建 0 error，resolver job `53f90282f6c54097bb08cced170e9480` 为 `22/22 PASS`；无细分诊断的
> 1000 AI logic average/P95/P99/max 为 `19.1309/23.7975/26.8002/27.7176 ms`，0 B、三代 collection 0、
> hash 不变、teardown 完整恢复。
>
> 同一撤回状态的 completed-frame fresh 复测为：logic average/P95 `18.3401/22.7304 ms`，completed main-thread
> average/P95 `24.9588/31.2782 ms`，render-thread `0.6530/0.8353 ms`，GPU `1.9164/3.3919 ms`；Unity frame
> interval average/P95 则为 `33.9777/44.1269 ms`，并存在 `885.2787 ms` 的 Editor 尖峰。本轮每帧最多 1 tick、
> catch-up frame 为 0，因此当前可见低帧率不是四 tick 追帧，也不是 GPU/render-thread 或已撤回 resolver 候选造成；
> 它主要位于 Editor/Profiler 调度和逻辑 tick 外主线程尾延迟。U6 继续只接受 completed-frame 与同场景 A/B 能证明的
> 完整产品删除；U9 仍需 Windows Player 60 秒矩阵，S0、T8 默认 `stage.dat` 与 Android 真机边界不变。

> U6 2026-08-14 14:58 更新：第四十六切片对 `LateEntityUpdate` 的 common no-op 分支增加 exact-character
> fail-closed 门禁。普通 state 不再进入 state-special 虚调用；不处于 HP/PP 恢复周期时不再进入恢复链；HP 仍大于
> 0 时不再进入死亡 opoint；普通角色不再进入仅供飞行武器使用的 post-opoint cleanup。未知派生类型、特殊 state、
> 恢复周期、死亡角色、武器/特殊攻击以及诊断 Legacy oracle 均保持原链。相同 seed、1000 个真实生产 AI、30 warmup +
> 180 sample 的交错 A/B 中，候选 total average 为 `19.9376 ms`，Legacy 为 `20.0362 ms`；完整
> `LateEntityUpdate` average 为 `2.5825/2.6596 ms`。收益约 `0.077 ms/tick`，只证明删除了常见 no-op 虚调用，
> 不宣称整体 FPS 提升。两轮 parity/lockstep hash 一致、正式 tick 0 B、三代 collection 0、teardown 完整恢复；
> fresh 聚焦回归 `15/15 PASS`。本切片不关闭 U6/U9。

> U6 2026-08-14 15:38 更新：第四十七切片复用 `CollisionSnapshot` 已按 runtime slot 升序访问的实体，向
> `CandidateCollect` 发布同 tick、occupancy-epoch 约束的双 roster：全实体 roster 继续负责清空旧候选与兼容状态，
> formal-participant roster 排除 suppress 实体但保留 inert/无效 AABB 参与者，继续满足 role-aware collector 的保守
> fallback 合同。首版对两份 roster
> 逐实体重复验证 runtime handle，令目标 pass 回退，已舍弃；正式精简版只在跨 pass 边界验证 tick 与
> `RuntimeSlotOccupancyEpoch`，真正参与碰撞的实体仍由 formal builder 执行 slot/entity/handle 完整校验。相同 seed 的
> Candidate/Legacy/Legacy-repeat/Candidate-repeat 四轮中，候选 `CandidateCollect` average 为
> `3.5432/3.5471 ms`，Legacy 为 `3.5819/3.7641 ms`；候选 total average 双轮均值 `20.2541 ms`，Legacy 为
> `20.4782 ms`，但单轮总耗时一正一反，因此只按目标子段稳定正收益与删除重复 canonical roster 构建保留。
> 四轮 20 个最终 parity/lockstep hash 字段、workload fingerprint 均完全一致，正式 tick 0 B、三代 collection 0、
> teardown 完整恢复。首轮 full self-check 抓到 inert fallback 被提前过滤后已修正；最终 fresh runtime/editor 编译
> 0 error，扩大碰撞矩阵 job `4ff0c6d5320a4c788af73b864f419fd3` 为 `69/69 PASS`，
> `BattleRuntimeSelfCheck` 于 `2026-08-14 15:51:12 PASS`。本切片不关闭 U6/U9；下一批继续处理对象 shell、生命周期及
> 能删除完整数据产品的热循环，S0、T8 默认 `stage.dat` 与 Android 真机仍排除。

> U6 2026-08-14 16:07 更新：第四十八切片依据权威 C# `ApplyFramePostProcess` 与
> `RunEntityPostframeTail` 的直接 runtime 写入契约，删除 exact `LF2Character` 在两个 pass 尾部对已提交真值的完整
> `RefreshRuntimeSnapshot()`；未知派生类型与独立 Legacy A/B 开关继续 fail-closed 走原链，遍历顺序、数值计算、候选
> carrier 和生命周期不变。Fresh 聚焦 job `8d8a5d6a20be4480bb2d9c1cfcc9db81` 为 `16/16 PASS`，配置 job
> `c065bd35c2634afb88ddbe6342e442bf` 为 `1/1 PASS`，`BattleRuntimeSelfCheck` 于
> `2026-08-14 16:07:45 PASS`。相同 seed、1000 个真实生产 AI、30 warmup + 180 sample 的独立 A/B 中，candidate
> `FramePostProcess`/`EntityPostFrameTail` average 为 `0.1581/0.1787 ms`，Legacy 为
> `0.5995/0.6030 ms`；目标两段合计约减少 `0.866 ms/tick`，logic average 从 `20.3351 ms` 降至
> `19.7140 ms`。两轮 workload、parity/lockstep hash 相同，正式 tick 0 B、三代 collection 0、teardown 完整恢复。
> 报告为 `Temp/NTSD_ProductionEntityStress.combat1000.u6-postframe-snapshot-candidate.json` 与
> `Temp/NTSD_ProductionEntityStress.combat1000.u6-postframe-snapshot-legacy.json`。该切片不关闭 U6/U9；下一批继续审计
> CharacterInput、LateEntityUpdate 与对象 shell 中仍可整体删除的数据产品，S0、T8 默认 `stage.dat`、Android 真机边界不变。

> U6 2026-08-14 16:43 更新：第四十九切片没有改写碰撞规则，而是为已存在的共享 frame role-aware body 模板补齐
> 1000 AI 独立 Legacy A/B。相同 seed、30 warmup + 180 sample 下，共享模板令
> `CandidateCollect/ParticipantBodyItrBuild` average/P95 从 `1.2756/1.4469 ms` 降至
> `1.0124/1.1093 ms`，`CandidateCollect` average 从 `3.9221 ms` 降至 `3.6405 ms`；完整 tick
> average 从 `19.9427 ms` 降至 `19.7479 ms`，P99 从 `27.3327 ms` 降至 `26.9161 ms`，但单轮
> P95 高 `0.1583 ms`，所以只按目标子段可归因收益保留，不宣称稳定 FPS 跃升。两轮 workload、parity/lockstep hash
> 相同，正式 tick 0 B、三代 collection 0、teardown 完整恢复；模板 build/hit/fallback 最大观察值为 `74/999/0`。
> 压力配置聚焦 job `6a4fb1a915054b8e8b8b0bb2df25cc72` 为 `1/1 PASS`。报告为
> `Temp/NTSD_ProductionEntityStress.combat1000.u6-role-body-template-candidate.json` 与
> `Temp/NTSD_ProductionEntityStress.combat1000.u6-role-body-template-legacy.json`。该切片不关闭 U6/U9；下一批继续优先审计
> `CharacterInput/EntityInputPass`、完整 command materialization 和对象 shell 的可删除产品，S0、T8 默认 `stage.dat`、
> Android 真机仍排除。

> U6 2026-08-14 17:32 更新：第五十候选同时尝试删除 `PostFrameAdvanceDeathCleanupAll` 的稳定实体快照与
> pass 前置 `Runtime.SyncIntegerPosition()`。两次候选运行均能稳定复现相同候选 hash，logic average/P95 分别为
> `17.4383/22.0394 ms` 与 `17.1634/21.8014 ms`，但 lockstep overall 从基线
> `4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063` 变为
> `ac100b...f940c9`，RNG、slot、aRest 与事件子 hash 也随之改变，且没有可归因性能收益。候选已用逆向最小补丁完整
> 撤回；撤回后的 fresh 1000 AI 报告
> `Temp/NTSD_ProductionEntityStress.combat1000.u6-postframe-death-roster-reverted.json` 为
> average/P95/P99/max `17.4036/21.8896/23.6281/25.7688 ms`，lockstep overall 精确恢复上述基线，正式 tick
> `0 B`、三代 collection `0`、teardown 完整恢复；生命周期聚焦 job
> `7617ab8da52c4c02bbc3087ab444b0c1` 为 `8/8 PASS`。结论是 Unity 适配层的整数坐标同步与稳定 roster 仍属于当前
> 可观察行为合同，不能仅因权威 C# 没有同名方法就删除。本候选不保留任何代码，不关闭 U6/U9；下一批回到
> frame/motion store 与 runtime compatibility facade 的唯一 owner 边界，先以独立 Legacy A/B 验证，再决定是否晋升。

> U6 2026-08-14 18:44 更新：第五十一候选为 `BattleFrameMotionStore` 增加默认关闭的 runtime facade，尝试让已绑定实体的
> `XInt/YInt/ZInt/Vx/Dir/Frame/FrameState/HitStop` 直接以 generation-owned SoA store 为唯一 owner，并在 release/reset
> 时恢复 compatibility backing fields。相同 workload/config fingerprint、seed、1000 个真实生产 AI、30 warmup + 180 sample
> 的 Legacy/Candidate/Legacy-repeat/Candidate-repeat 四轮交叉复测中，Legacy average 为
> `17.8384/17.8315 ms`，Candidate 为 `18.0745/18.2388 ms`；两轮均值从 `17.8349 ms` 回退至
> `18.1567 ms`，约慢 `1.80%`。P95 两轮均值为 Legacy `22.2478 ms`、Candidate `22.2055 ms`，只有约
> `0.19%` 的噪声级反向差异，不能抵消平均耗时的稳定回退。四轮 parity overall 均为
> `752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，lockstep overall 均为
> `4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`，正式 tick 均为 `0 B`、
> Gen0/1/2 collection 均为 `0`、teardown 均完整恢复。报告为
> `Temp/NTSD_ProductionEntityStress.combat1000.u6-frame-motion-facade-legacy.json`、
> `Temp/NTSD_ProductionEntityStress.combat1000.u6-frame-motion-facade-candidate.json`、
> `Temp/NTSD_ProductionEntityStress.combat1000.u6-frame-motion-facade-legacy-repeat.json` 与
> `Temp/NTSD_ProductionEntityStress.combat1000.u6-frame-motion-facade-candidate-repeat.json`。该候选不满足“无整体性能回退”的
> 晋升门槛，facade、压力请求字段、恢复逻辑和专用测试已全部撤回；保留原有 runtime authority + generation-owned SoA 投影边界。
> 撤回后本地 runtime/editor 工程串行构建均为 0 error；fresh Unity 强制刷新后 Console 为 0 error，聚焦 job
> `ec6df1e76edb47168985485969f679a1` 为 `32/32 PASS`，`BattleRuntimeSelfCheck` 于
> `2026-08-14 18:48:52 PASS`。最终回退状态的同配置 1000 AI 报告
> `Temp/NTSD_ProductionEntityStress.combat1000.u6-frame-motion-facade-reverted-final.json` 为
> average/P95/P99/max `17.3635/21.4798/23.2571/24.9152 ms`，workload/config、parity 与 lockstep hash
> 精确恢复基线，正式窗口 `0 B`、Gen0/1/2 collection 为 `0`、teardown 全部恢复且 cleanup exception 为 `0`。
> 因此本批负实验已经完整关闭，没有候选代码残留；它不关闭 U6/U9，S0、T8 默认 `stage.dat`、Android 真机边界不变。

> U6 2026-08-14 19:10 更新：第五十二批先在第五十一候选完全撤回后的代码上采集一次默认开启全部细分计时的
> 1000 AI 热点报告 `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice52-current-hotspots.json`。30 warmup +
> 180 sample 中，logic average/P95/P99/max 为 `19.6954/23.9996/26.1995/27.8256 ms`；其中
> `CharacterInput` average 为 `5.7967 ms`，`CandidateCollect` 为 `3.7039 ms`，`LateEntityUpdate` 为
> `2.6303 ms`。`CandidateCollect` 的默认 role-aware 路由在 210 个正式 tick 中使用 nested `118` 次、sweep
> `92` 次、tree `0` 次；其 P95/P99 为 `7.9959/9.9219 ms`。随后以完全相同 seed、workload、参与者、候选、
> 声音和表现配置分别强制现有 tree、nested、sweep 后端，报告为
> `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice52-force-tree.json`、
> `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice52-force-nested.json` 与
> `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice52-force-sweep.json`。Tree 的 logic/CandidateCollect
> average 为 `19.7597/3.9272 ms`，P95 为 `25.5123/8.1680 ms`，平均与尾部均回退；全 nested 的
> average 略降至 `19.6524/3.5983 ms`，但 P95/P99 回退至 `25.1503/8.2033` 与
> `27.4753/10.9455 ms`；全 sweep 的尾部略降，但 average 回退至 `20.0447/4.3148 ms`。四轮 workload
> fingerprint 完全一致，parity overall 均为
> `752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`，lockstep overall 均为
> `4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`；正式窗口均为 `0 B`、
> Gen0/1/2 collection 均为 `0`、teardown 均完整恢复。结论是现有 mixed 路由仍是本 workload 下平均与尾延迟间
> 最稳健的生产取舍；不能把 Tree 强制设为默认，也不能仅凭全 nested 的平均值降低 crossover 而牺牲 P95/P99。
> 本批只关闭 broadphase 后端/阈值猜测，不保留生产代码修改；下一候选转向当前最大热点
> `CharacterInput/EntityInputPass` 中可整体删除的重复数据产品。U6/U9 与服务器、T8、Android 边界不变。

> U6 2026-08-14 更新（第五十三批，`CharacterInput` 句柄直读候选已否决并撤回）：审计发现 unified AI
> current snapshot 已持有 `(runtimeSlot, generation)`，因此曾在 `BattleCharacterInputStore` /
> `BattleCharacterInputWriter` 增加按句柄直读入口，并让 unified fast path 避免再次通过 runtime owner
> 解析槽位。候选先通过主工程与 Editor 工程编译 `0 error`，聚焦 EditMode job
> `f1b586c3cfb34eb691d9c4beec26e597` 为 `32/32 PASS`。随后使用与第五十二批 baseline 完全相同的
> seed、1000 AI、30 warmup、180 sample、参与者/候选/声音/表现配置运行
> `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice53-input-handle-candidate.json`。Baseline 的
> logic average/P95/P99/max 为 `19.6954/23.9996/26.1995/27.8256 ms`，候选为
> `19.4281/23.4855/25.6982/27.5283 ms`；但候选所针对的 `CharacterInput` average 实际从
> `5.7967 ms` 变为 `5.8008 ms`，`IndexedCanonicalCapture` average/P95/P99 从
> `0.4108/0.4358/0.5239 ms` 回退至 `0.4676/0.4814/0.4854 ms`，`RemainingAiDecision` 也从
> `2.7781 ms` 回退至 `2.8381 ms`。两轮 workload fingerprint、parity overall、lockstep overall
> 完全一致，正式窗口均为 `0 B`、Gen0/1/2 collection 均为 `0`、teardown 均完整恢复；因此总 logic
> 的小幅下降不能归因于这个候选，而目标子段已明确负优化。相关生产代码与测试断言已完整撤回，仅保留报告作为
> 禁止重复尝试的证据。U6/U9 仍未完成；后续继续寻找能整体删除高频数据产品或循环的实质候选，不再围绕这种
> 亚毫秒句柄校验做微优化。S0、T8 与 Android 边界不变。

> U6 2026-08-14 更新（第五十三批撤回闭环与第五十四批无探针帧基线）：第五十三候选撤回后的主工程与
> Editor 工程串行构建均为 `0 error`，正确的输入聚焦 EditMode job
> `1e352b54134f4570bbcd40c79ca34246` 为 `32/32 PASS`，`BattleRuntimeSelfCheck` fresh `PASS`。
> 同配置详细回归
> `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice53-input-handle-reverted-final.json` 的
> logic average/P95/P99/max 为 `19.8205/23.9453/25.7958/27.6590 ms`；`CharacterInput`、
> `CandidateCollect`、`LateEntityUpdate` 与 `RenderDispatch` average 分别为
> `5.8771/3.6568/2.7071/1.1246 ms`。AI 最近目标搜索仅约 `0.5923 ms`，完整 indexed canonical kernel
> 约 `1.7897 ms`；审计权威 `InputRuntime.ApplyCharacterInput` 后确认 Combo、direct action、release 与
> locomotion 都是正式规则消费，不存在可整体删除的隐藏多毫秒循环，因此不再围绕 AI 搜索或单次句柄校验做
> 微优化。
>
> 随后关闭 phase、presentation 与 detail 三类高频计时探针，只保留外层 Stopwatch 与 Unity
> `FrameTimingManager` completed-frame 证据，运行
> `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice54-current-notiming-frame-baseline.json`。1000 个真实
> 生产 AI、相同 seed、30 warmup + 180 sample 下，logic average/P95/P99/max 为
> `17.4013/21.4772/24.7339/26.0391 ms`；179 个完整渲染帧的 main-thread average/P95/P99/max 为
> `19.0253/29.2852/32.3095/33.8544 ms`，完整 CPU 为 `23.3576/33.8714/36.9172/37.9764 ms`，render-thread
> average 为 `0.6364 ms`，GPU 有效样本 average 为 `1.5260 ms`。该结果证明详细探针显著放大绝对耗时；当前
> 主线程 P95/P99 已进入 30 FPS 的 `33.33 ms` 预算，但完整 CPU P95 仍高出约 `0.54 ms`，可见 Unity frame
> P95 为 `36.5323 ms`，所以仍不能写成“稳定 30 FPS 已完成”。本轮无追帧帧、最大单帧逻辑 tick 为 1，正式
> tick `0 B`、Gen0/1/2 collection 为 `0`；parity overall
> `752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35` 与 lockstep overall
> `4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063` 精确保持，teardown
> `restored=true`。Profiler frame 的 Editor-wide 分配不等同于正式 tick 分配门禁，Windows Player 的 U9
> 60 秒矩阵仍是最终帧率与全 PlayerLoop 零 GC 依据。中央表现详细样本约 `5.32 ms` 包含逐命令计时观察开销，
> 且当前命令链已复用 viewport、sprite capture、trusted resource 与持久 Mesh；在没有同场景无探针 A/B
> 证明前，不为追逐 Editor 尾帧盲目删除 RenderCommand、Overlay、Scene 视图或 fail-closed 诊断合同。U6
> 下一批回到能删除完整对象式字段簇/循环的 canonical owner 迁移；U9、S0、T8 与 Android 边界不变。

> U6 2026-08-14 更新（第五十五批，Late 空分支活跃检查裁剪，已保留）：`RunLateEntityUpdateAll`
> 通过 `FindEntityByRuntimeSlotCurrent` 取得的实体已经保证属于当前活动 generation，旧路径却立即再次执行
> `IsActiveForCurrentPass`；同时，exact `LF2Character` 的 state-special、recovery、death-opoint 与
> post-opoint 已由现有门禁证明为 no-op 时，仍在每个空分支后重复检查活动状态。当前实现删除首次重复检查，并只在
> 对应阶段确实调用了对象方法或实际处理了 opoint 后保留生命周期检查；未知派生类型、特殊 state、恢复周期、死亡、
> 武器/特攻、frame tick、cleanup、tail、flush、slot 顺序和一切可能改变活动状态的路径继续 fail-closed，未改变权威
> C# pass 顺序或可观察行为，也未增加新的 partial、静态状态或运行时分配。
>
> 同一 seed `1314149188`、1000 个真实生产 AI、30 warmup + 180 sample、相同 workload 的独立 A/B 中，候选报告
> `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice55-late-activecheck-candidate.json` 的
> `LateEntityUpdate` average/P95 为 `2.4989/2.8144 ms`，强制旧 common 路径报告
> `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice55-late-activecheck-legacy.json` 为
> `2.6228/2.8793 ms`，目标阶段 average 下降约 `4.72%`、P95 下降约 `2.25%`；整 tick average/P95 分别为
> `19.4271/23.7519 ms` 与 `19.5537/24.0064 ms`，只改善约 `0.65%/1.06%`。两轮 workload fingerprint、
> parity overall `752b49074eccfbb15665a7565cf0bdcf24784a05569d5c935897028bf1ef7b35`、lockstep overall
> `4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063` 完全一致，正式 tick `0 B`、
> Gen0/1/2 collection 均为 `0`、teardown 均 `restored=true`。因此该切片按目标 pass 的可归因小收益保留，
> 不能宣称它单独解决 1000 AI 帧率。
>
> 关闭 phase、presentation、detail 三类高频探针后的生产回归
> `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice55-late-activecheck-final-notiming.json` 为
> logic average/P95/P99/max `17.5980/22.6316/27.0556/31.0396 ms`；179 个 accepted completed frame 的
> main-thread average/P95/P99/max 为 `20.1318/30.8649/36.7953/42.7398 ms`，完整 CPU 为
> `24.6023/35.3374/42.2010/49.2089 ms`，render-thread/GPU average 为 `0.6497/0.9306 ms`。该轮与第五十四批
> implementation config fingerprint 相同，但 workload fingerprint 因初始化批次配置不同而不同，故不与第五十四批
> 做直接整体性能归因；它只作为当前生产路径 0 B、hash 不变和清理恢复的 fresh 证据。主工程与 Editor 工程构建均
> `0 error`，Late/快照/opoint 边界聚焦 job `cb7f149328f04c36a66d9f9bc37edbf4` 为 `31/31 PASS`，
> `BattleRuntimeSelfCheck` fresh `PASS`。U6/U9 仍未关闭；下一批继续寻找能够删除完整字段产品或对象 shell 循环的
> canonical owner 迁移，S0、T8 与 Android 边界不变。

> U6 2026-08-15 更新（第五十六/五十七批）：第五十六批尝试为共享 frame 的 ITR 局部几何另建模板与字典；相同
> seed、1000 AI、30 warmup + 180 sample 下，`ParticipantBodyItrBuild` average 从 `1.0602 ms` 回退到
> `1.2133 ms`，`CandidateCollect` 从 `3.7219 ms` 回退到 `3.9053 ms`，因此候选已完整撤回，并形成“不再建立第二套
> frame/ITR 缓存”的负实验门禁。第五十七批只复用 role-aware 正式广阶段在同 tick 已经计算的 ITR `WorldRect`，
> exact loop、索引、null、引用、顺序、双向消费和 fallback 均保留；合同不成立即回到原计算。
>
> 第五十七批同种子 A/B/B/A 中，candidate 的 `PairExactLoop` 两轮平均约 `0.6478 ms`，Legacy 两轮约
> `0.7010 ms`，目标子段改善约 `7.58%`；P95 两轮均值改善约 `6.31%`。四轮 workload/parity/lockstep hash
> 完全一致，正式 tick `0 B`、Gen0/1/2 collection 为 `0`、teardown 全部恢复。关闭全部高频探针后的生产回归
> `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice57-exact-itr-reuse-final-notiming.json` 为 logic
> average/P95/P99/max `17.3797/21.6857/24.0533/24.8405 ms`；因该报告关闭 completed-frame timing，只能证明
> 逻辑 tick 已在 33.33 ms 预算内，不能冒充完整 PlayerLoop 或稳定显示 30 FPS。fresh runtime/editor 构建均为
> `0 error`，formal collector 扩大 job `4fb0c72edefe48229c2de8a6304250c3` 为 `57/57 PASS`，
> `BattleRuntimeSelfCheck` 于 `2026-08-15 00:22:57 PASS`。第五十七切片按目标子段正收益保留，但 U6/U9 仍未关闭；
> 下一批继续从 `CharacterInput`、`CandidateCollect`、`LateEntityUpdate` 和表现发布的 fresh 无探针/低扰动证据中选择
> 能删除完整数据产品或循环的候选，不以微小 lookup 替换或单帧 Editor Stats 宣称完成。S0、T8 默认 `stage.dat`、
> Android 真机边界不变。

> U6 2026-08-15 更新（第五十八批，朝向 canonical 值存储与单向镜像，已保留）：
> `NTSDEntityRuntime.Dir` 的内部 owner 已由字符串改为 byte `facingLeft`，现有字符串 API 只作为返回 interned
> `left/right` 的零分配 compatibility facade；frame/motion store 直接接收 bool/byte 朝向。首版曾令
> `PhysicsState.dir` 反向写入 runtime，但完整 self-check 在“PS.dir 陈旧时抓取同步仍必须读取 Runtime.Dir”处失败，
> 由此确认 PS 只是允许陈旧的 Unity 兼容镜像。该反向绑定已撤回，最终只允许 `SwitchDir` 执行
> runtime → PS/sprite 单向同步，未修改 self-check 以迁就实现。
>
> 最终 runtime/editor 构建为 `0 error`，聚焦 job `d5f673de3fd9478cb6b58432816987f1` 为
> `58/58 PASS`，新增朝向测试包含 4096 次 warmed 读写 `0 B`；`BattleRuntimeSelfCheck` 于
> `2026-08-15 00:45:44 PASS`。同 seed、1000 个真实生产 AI、30 warmup + 180 sample 的无细分计时回归
> `Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-performance-smoke.json` 为 logic
> average/P95/P99/max `17.4333/21.4082/23.4661/33.8515 ms`，与第五十七批 average 只差约 `0.31%`，
> 不构成可见 FPS 收益或回退；parity hash 精确一致，正式 tick `0 B`、Gen0/1/2 collection `0`、teardown
> 完整恢复。该切片按所有权收敛保留，不关闭 U6/U9；下一批继续选择能删除完整数据产品或对象 shell 循环的候选，
> S0、T8 默认 `stage.dat` 与 Android 真机边界不变。

> U6 2026-08-15 更新（第五十九批，碰撞几何朝向 canonical 读取，已保留）：权威 C# 的
> `CollisionCollect` 在粗筛、精确 ITR/BDY 几何和同朝向过滤中统一读取 `Entity.Facing`，该字段属于 Runtime；Unity
> 原实现仍有六处读取允许陈旧的 `PhysicsState.dir`。当前已把 role-aware body 物化、exact cache、cache 失效、
> same-facing、普通矩形与 EXE 溢出语义矩形整组收敛到 `Runtime.IsFacingLeft`，兼容 helper 只在 Runtime 缺失时
> fail-closed 使用旧镜像。新增测试故意令 Runtime 向左、PS 向右，验证 BruteForce 与 role-aware 都按 Runtime
> 产生同一候选；self-check 的 kind5 broadphase 夹具也以相反 PS 镜像证明 canonical owner。
>
> 最终 runtime/editor 串行构建均为 `0 error`，聚焦 job `bc63d448304947ad84c1e1ab4f71a2d0` 为
> `59/59 PASS`，`BattleRuntimeSelfCheck` 于 `2026-08-15 01:08:24 PASS`。两轮同种子 1000 AI 无细分计时
> 报告 `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice59-runtime-facing-runA.json` 与
> `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice59-runtime-facing-runB.json` 的 logic average/P95 为
> `19.7072/33.7398 ms` 与 `20.4907/31.2362 ms`；两轮 hash 与第五十八批精确一致、正式 tick `0 B`、
> Gen0/1/2 collection `0`、teardown 完整恢复。相较前一单轮的计时波动不能归因于这次 canonical bool 读取，故本批
> 只按权威正确性与所有权收敛保留，不声明 FPS 收益或回退，也不关闭 U6/U9。S0、T8 默认 `stage.dat` 与 Android
> 真机边界不变。

> U6 2026-08-15 更新（第六十一候选，普通站立无动作 release 快路径，已否决并撤回）：候选仅在 exact
> `LF2Character` 的 Standing/Walking、无重武器且 Attack/Jump/Defend 均未 action-ready 时，直接执行旧 resolver
> 最终的 walk/run frame 更新。相同 seed `1314149188`、1000 AI、30 warmup + 180 sample 的正反顺序复测中，
> candidate logic average 为 `19.1095/18.8967 ms`，Legacy 为 `18.9367/19.0025 ms`；两轮均值 candidate
> 反而约慢 `0.18%`。更关键的是目标 `ReleaseResolve` 两轮均从约 `0.383 ms` 回退到约 `0.416 ms`，尽管每轮命中
> `12,760` 次。四轮最终 parity hash 一致、正式 tick `0 B`、teardown 完整恢复，因此它是行为等价但性能为负的
> 微优化。候选代码、诊断接线、菜单与专用测试已全部撤回；撤回后 Unity 编译 `0 C# error`，输入聚焦 job
> `e1e3c514c78145d2884ee448784eefcb` 为 `32/32 PASS`。完整数据见 canonical inventory 第 53 节。后续不再围绕
> resolver 条件判断做微优化，只接受能删除完整数据产品、对象 shell 循环或跨 pass 重复遍历的候选；U6/U9、S0、
> T8 默认 `stage.dat` 与 Android 真机边界不变。

> U6 2026-08-15 更新（第六十二/六十三批）：第六十二批用相同 seed、1000 AI 与 workload 强制全部
> `CandidateCollect` 走 role-aware X-sweep；比较次数虽从 `1,049,994` 降到 `664,526`，但
> `DirectBroadphase` average 从 `0.8128 ms` 回退到 `1.5011 ms`，`CandidateCollect` average 从
> `3.5569 ms` 回退到 `4.2603 ms`，完整 tick average 从 `18.9262 ms` 回退到 `19.6910 ms`。因此保持现有
> nested/sweep 自动交叉，不保留强制 sweep 代码。
>
> 第六十三批在 generation-owned `BattleRelationLinkStore` 内维护 `LinkState > 0` 的位图派生索引；LinkState
> 写入、bind/capture、release、reset 与 grow 同步维护该索引，正式 Link 校验按 slot 升序只消费索引项，并再次校验
> generation。相同 seed 的 A/B 中，`HeldLinkValidation` average 从 Legacy `0.093405 ms` 降到
> `0.001245 ms`，约改善 `98.7%`；完整 tick average 从 `19.3634 ms` 降到 `18.8785 ms`。两轮 parity 与
> lockstep hash 完全一致，正式 tick `0 B`、Gen0/1/2 collection `0`、teardown 完整恢复，因此默认已晋升为
> `DataOriented`，同时保留显式 Legacy A/B 入口。
>
> 晋升后的默认 1000 AI 容量回归
> `Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-capacity-pressure-smoke.json` 实际模式为
> `data-oriented`，logic average/P95/P99/max 为 `18.9096/23.1730/26.0301/29.7562 ms`，零 GC 与容量门均
> 通过。Link 聚焦 job `3426b75b123e4c40a840c29113e94e0c` 为 `8/8 PASS`，压力默认接线 job
> `1f0f69bbaa1c48a6936650e9d1aa3191` 为 `5/5 PASS`，`BattleRuntimeSelfCheck` 于
> `2026-08-15 03:00:21 PASS`。完整所有权和 A/B 数据见 canonical inventory 第 55 节。该切片仍不关闭 U6/U9；
> 下一批继续从 fresh 热点选择能删除完整数据产品或循环的候选，S0、T8 默认 `stage.dat` 与 Android 边界不变。

> U6 2026-08-15 更新（第六十四切片，无状态角色 mechanics 所有权收敛，已保留）：旧 Unity exact
> `LF2Character` 会同时从基类和派生类构造两份无状态 `CharacterMechanics`，1000 个角色约产生 2000 个语义
> 相同的策略对象。当前改为 `SimulationWorld` 持有唯一 mechanics 服务；已注册实体统一解析 world-owned 实例，
> 未注册测试壳与兼容对象仅在首次实际使用时惰性创建 fallback。没有静态可变状态、每 tick 分配或规则顺序变化。
> fresh 聚焦 job `4005ad30540c40f090fe0153fef3e944` 为 `11/11 PASS`，`BattleRuntimeSelfCheck` 于
> `2026-08-15 03:30:55 PASS`。1000 AI 无细分 timing 回归的 logic average/P95/P99/max 为
> `16.8144/21.2431/22.5278/23.9943 ms`，driver、PlayerLoop 与 presentation 战斗窗口均 `0 B`，Gen0/1/2
> collection 为 0，parity/lockstep hash 与基线相同，teardown 完整恢复。相邻基线 average/P95 为
> `16.8816/21.0864 ms`，差异属于短样本噪声，因此只声明对象所有权与启动对象图收敛，不声明稳态 FPS 提升。
> 完整证据见 canonical inventory 第 56 节。U6/U9 仍未关闭；下一批进入 FrameAdvance 正式
> exact-character 执行所有权迁移，S0、T8 默认 `stage.dat` 与 Android 边界不变。

> U6 2026-08-15 更新（第六十五切片，exact-character FrameAdvance world-owned 编排，已保留）：新增普通主类
> `BattleEcsCharacterFrameAdvancePass`，正式 `SerialTickAll` 对 exact `LF2Character + Character DAT` 不再经
> `entity.SimTransit()` 虚调用二次分派，而由 world-owned pass 按权威顺序直接编排 delay/throw guard、
> Link/Cpoint、动力学、state 12/燃烧帧提升与 weapon-count 尾处理。未知派生类型、非角色 DAT shell 与显式 Legacy
> 保留原对象路径；slot 顺序、键清理、活动性复查、`SimTU` 和清理边界未变。最终版的动力学也由 pass 直接构造
> context、调用 world-owned mechanics、同步边界消费、分派落地事件并提交整数位置，不再经
> `LF2Character.ApplyDynamics()` 的对象分支。聚焦 job
> `81971a6424ba4caaae9b3b4240fcafe4` 为 `14/14 PASS`，扩大聚焦 job
> `d6760bd21bde466c95b49bd21e205e6a` 为 `20/20 PASS`，最终动力学版聚焦 job
> `ed91f4e232c0428096b46af2da2b8f2d` 为 `14/14 PASS`，self-check 于
> `2026-08-15 03:52:11 PASS`。fresh 1000 AI 生产回归中模式为 `DataOriented`，`210000/210000` 次 transit
> 命中新路径、fallback 0；logic average/P95/P99/max 为 `16.8062/21.1247/22.5522/24.1429 ms`，三条战斗
> 内存边界 `0 B`、三代 collection 0、hash 与基线一致、teardown 完整恢复。相邻回归 average/P95 为
> `16.8144/21.2431 ms`，因此不把小幅差异宣称为稳定 FPS 收益。完整证据见 canonical inventory 第 57 节；
> U6/U9 仍未关闭，下一批继续收口 FrameAdvance 相邻 lifecycle canonical 写入，S0、T8 默认 `stage.dat` 与
> Android 边界不变。

> U6 2026-08-15 更新（第六十六切片，exact-character 周期恢复 world-owned pass，已保留）：新增普通主类
> `BattleEcsCharacterRecoveryPass`，在 Late recovery 的原权威位置处理 exact `LF2Character + Character DAT +
> Health`。pass 保持 `StepWait`、12 tick HP/伤势恢复、3 tick PP 恢复、oid 51/52 上限规则以及 HPBound/
> ComboCountVic 副作用顺序；非周期 tick 可证明为 no-op，未知派生、非角色 DAT、缺失 Health 与显式 Legacy 仍
> fail-closed 回原虚调用。聚焦 job `b8d9d1bd67144ce3a44df5a37e08b33c` 为 `18/18 PASS`，Unity fresh 编译
> 0 C# error，self-check 于 `2026-08-15 04:11:42 PASS`。1000 AI、30 warmup + 180 sample 的无探针报告
> `Temp/NTSD_ProductionEntityStress.combat1000.u6-slice66-character-recovery-final-20260815.json` 中，新 pass
> `210000/210000` 次命中 exact-character，`140000` 次周期 no-op，fallback 0；logic average/P95/P99/max 为
> `16.6533/20.6773/22.5392/23.6292 ms`，四条战斗内存边界 0 B、三代 collection 0、parity/lockstep hash 与
> 第六十五切片完全一致、teardown 完整恢复。相邻回归 average/P95 为 `16.8062/21.1247 ms`，因此该切片按职责
> 所有权和行为等价保留，不宣称关键 FPS 提升。完整证据见 canonical inventory 第 58 节；U6/U9 仍未关闭，
> 下一批继续收口 lifecycle canonical 写入和对象兼容镜像，S0、T8 默认 `stage.dat` 与 Android 边界不变。

> U6 2026-08-15 更新（第六十七切片，exact-character FrameTick world-owned 编排，已保留）：新增普通主类
> `BattleEcsCharacterFrameTickPass`，在原 Late FrameTick 权威位置直接编排 exact `LF2Character + Character DAT`
> 的 delay/throw guard、Link/Cpoint、counter、state 0/14、next/wait 转移、caught-exit、frame 212、PP display、
> defend lock 与 hit-stop 顺序；未知派生、非角色 DAT shell 与显式 Legacy 仍 fail-closed 回原 `SimFrameTick`。
> 压力工具同步增加仅用于 reset-boundary A/B 的 `characterFrameTickMode`，teardown 会恢复原模式。fresh Unity 编译
> 未发现 C# error；聚焦 job `7355546bb6dd472290a3227e1bc4eeee` 为 `23/23 PASS`，self-check 于
> `2026-08-15 04:46:40 PASS`。同口径详细探针中目标 FrameTick average 从 `0.6876` 降至 `0.5667 ms`（约
> `17.6%`），LateEntityUpdate 从 `2.8358` 降至 `2.4709 ms`。两轮无探针交错 A/B 的 Legacy/DataOriented
> average 均值分别为 `16.8364/16.6222 ms`，约改善 `0.2143 ms`（`1.27%`），P95 基本持平且 P99 未改善，
> 因此只按目标 pass 与所有权的可归因收益保留，不宣称整体 FPS 或尾延迟突破。四份 A/B 均为正式 tick `0 B`、
> 三代 collection 0、parity/lockstep hash 完全一致，teardown、模式恢复与 cleanup 通过。完整证据见 canonical
> inventory 第 59 节；U6/U9 仍未关闭，下一批继续收口剩余 lifecycle canonical world 与对象兼容镜像，S0、T8
> 默认 `stage.dat` 与 Android 边界不变。

> U6 2026-08-15 更新（第六十八切片，PostFrameTail world-owned 候选，性能门禁未过，默认 Legacy）：新增普通主类
> `BattleEcsCharacterPostFrameTailPass`，按权威 C# 顺序实现 exact character 的 HealTimer、CatchTimer、state 1700
> 与瞬态命中/MP carrier 清理，并保留未知派生、非角色 DAT 与显式 Legacy 的 fail-closed 原路径。压力工具增加
> `characterPostFrameTailMode`、计数与 teardown 恢复报告。相同 seed、1000 AI、30 warmup + 180 sample 的无探针
> ABAB 中，Legacy/DataOriented 两轮 logic average 均值为 `16.8396/16.8277 ms`，只差 `0.0120 ms`；P95 基本
> 持平，Data P99 未改善。更关键的定向 timing 中，目标 `EntityPostFrameTail` average 从 Legacy `0.183471 ms`
> 回退到 Data `0.198651 ms`，慢约 `8.3%`，P95 与 max 也更高。因此候选没有晋升，生产、压力默认与空值解析均
> 保持 `Legacy`，DataOriented 只作为显式诊断候选。六轮 parity/lockstep hash 完全一致、正式 tick `0 B`、三代
> collection 0、teardown 与模式恢复通过。fresh Unity 编译未发现 C# error；聚焦 job
> `6bc6d69649f742c09737e19eb3c23ae5` 为 `5/5 PASS`，扩大聚焦 job
> `96b264a08c4045f79282442f4483535b` 为 `27/27 PASS`，self-check 于 `2026-08-15 05:19:44 PASS`。完整证据见
> canonical inventory 第 60 节。U6/U9 仍未关闭；下一批不再搬运亚毫秒生命周期尾部，而回到 fresh 多毫秒热点，
> 只接受能删除完整数据产品、对象 shell 循环或跨 pass 重复遍历的候选。S0、T8 默认 `stage.dat` 与 Android 边界不变。

> U6 2026-08-15 更新（第六十九切片，AI canonical owned-input 重复快照候选，性能门禁未过）：fresh 调用链审计确认
> IndexedCanonical 会把 `BattleCharacterInputStore` 的 canonical 输入先复制到 `AiDecisionSnapshot.Input`，kernel 再复制为
> 局部可变值。候选 `CanonicalStoreDirect` 只删除第一份结构体复制，仍保持本地值语义、slot 顺序、RNG、校验、事务提交和
> FullScan oracle。相同 seed、1000 AI、30 warmup + 180 sample 的 ABAB 中，SnapshotCopy/Direct 两轮 logic average
> 均值为 `16.8871/16.8757 ms`，只差 `0.0113 ms`（约 `0.07%`）；Direct 的 P95/P99 均值反而更高。因此该候选未晋升，
> 生产默认恢复为 `SnapshotCopy`。四份报告均为正式 tick `0 B`、三代 collection 0、fallback 0，parity/lockstep hash
> 完全一致。最终聚焦 job `7fdc4bb85d1f4be28bc5bf66f0e4834a` 为 `74/74 PASS`，self-check 于
> `2026-08-15 05:48:17 PASS`。完整证据见 canonical inventory 第 61 节。U6/U9 仍未关闭；下一批继续审计
> CandidateCollect、LateEntityUpdate 与 RenderDispatch，只接受整循环删除、跨 pass 去重或可证明的数据结构收益。
> S0、T8 默认 `stage.dat` 与 Android 边界不变。

> U6 2026-08-15 更新（第七十切片，unified AI row 无 pending 空刷新跳过，已保留）：fresh 调用链和计数确认，
> `CharacterInput` 每处理一个 active character 都进入 unified row post-input refresh，而多数 canonical input projection
> 没有变化。新快路只在 current generation 无 pending、slot 不属于 first-ten move-mode 窗口、未强制 full refresh、未启用
> 增量 oracle 时跳过空检查；任何真实写入、first-ten witness、测试 mutation override 和验证模式仍走完整路径。第一次默认
> 启用暴露了增量 oracle 计数 `120 -> 91` 的诊断回归，最终已通过排除显式 oracle 修正，未用修改断言掩盖失败。
> 相同 seed、1000 AI、30 warmup + 180 sample、max catch-up 1 的 ABAB 中，Legacy A/C logic average 为
> `17.1461/16.8697 ms`，Candidate B/D 为 `16.8566/16.7495 ms`；两轮均值改善 `0.2048 ms`（约 `1.20%`），
> P95 均值下降且 P99 未形成可重复回退。四轮正式 tick 均为 `0 B`、三代 collection 0，parity/lockstep hash 完全一致，
> teardown 完整恢复。最终 fresh 聚焦 job `63ace2f715ca4dd2b608d9d4d7cbf57a` 为 `94/94 PASS`，self-check 于
> `2026-08-15 06:12:15 PASS`。完整证据见 canonical inventory 第 62 节。该切片只删除约 `0.204 ms` 的重复边界，
> 不代表 1000 AI 总目标或 U6/U9 已完成；下一批继续从 CharacterInput、CandidateCollect、LateEntityUpdate 和
> RenderDispatch 的剩余多毫秒循环中选取可独立 A/B 的候选。S0、T8 默认 `stage.dat` 与 Android 边界不变。

> U6 2026-08-15 更新（第七十一切片，exact-character 输入对象壳候选，双门禁否决）：候选新增普通主类
> `BattleEcsCharacterInputPass`，尝试在权威 `CharacterInput` pass 位置对 exact character 直接编排 AI preparation、
> combo/release resolver 与 frame velocity tail，未知派生和非角色 DAT 保留原虚调用。相同 seed、1000 个真实生产 AI、
> 30 warmup + 180 sample 的 ABAB 中，Legacy/DataOriented 两轮 logic average 均值为 `17.0642/17.1555 ms`，
> 候选慢约 `0.54%`，P95 均值也从 `21.5433` 回退到 `22.1679 ms`。四轮均为正式 tick `0 B`、三代
> collection 0、压力 parity/lockstep hash 完全一致、teardown 完整恢复；但定向 frame-jump/input-tail 用例又发现
> 压力场景未覆盖的状态差异。因此候选同时未通过性能门与完整 parity 门，生产默认恢复 `Legacy`，不计入 U6 收益。
> 压力工具另补充 `Process Pending Request` Editor 入口，只修复外部请求触发，不改变负载。完整数据见 canonical
> inventory 第 63 节。U6/U9 仍未关闭；下一批继续从 CandidateCollect、LateEntityUpdate 与 RenderDispatch 中选择
> 能删除整循环或跨 pass 数据产品的候选，S0、T8 默认 `stage.dat` 与 Android 边界不变。
> 最终生产默认恢复后，聚焦 EditMode job `38e5316667d046ea8b636699f6b3fdf1` 为 `35/35 PASS`，
> `BattleRuntimeSelfCheck` 于 `2026-08-15 06:46:36 PASS`，`git diff --check` 无 whitespace error。
> 本轮 Console 的两条 `SimulationWorld` Error 来自 self-check 的预期 fail-closed 夹具，三条 MCP disposed-object
> 日志来自工具连接层；未观察到该候选造成的编译错误或 self-check 失败。该证据仅闭合候选拒绝，不关闭 U6/U9。

> U6 2026-08-15 更新（第七十二切片，role-aware 碰撞角色产品合并候选，性能门禁否决并完全撤回）：fresh
> 1000 AI 细分基线把 `CandidateCollect` 定位为 `3.5374661 ms/tick`。候选把 participant 的 body/itr role flags
> 合并进 participant 结构，并用 warmed 固定数组替代第二份 role-required 列表，试图删除 pass 内的重复 role byte
> 重建；authority pair 顺序、双向 exact、RNG、fallback、generation/epoch 与碰撞规则均未改变。相同 seed
> `1314149188`、30 warmup + 180 sample 的候选中，`CandidateCollect` 只下降 `0.0252217 ms`，但 participant build
> 从 `0.9526328` 回退到 `0.9912617 ms`；完整 tick average 从 `19.0017367` 回退到 `19.0107956 ms`，P95 从
> `23.103635` 回退到 `23.194025 ms`，P99/max 也更差。两轮正式 tick 均为 `0 B`、三代 collection 0，
> parity/lockstep hash 完全一致，teardown 完整恢复。候选代码、字段和临时存储已全部撤回，不保留隐藏开关；撤回后
> Unity fresh 编译 Console 0 error，碰撞聚焦 job `fd209373a3ab4231aeb30ae632159ae5` 为 `58/58 PASS`，完整
> `BattleRuntimeSelfCheck` 于 `2026-08-15 07:09:06 PASS`。完整证据见 canonical inventory 第 64 节。该负实验不
> 计入 U6 收益，不关闭 U6/U9；下一批只接受 canonical store 与真实 production reader 联合迁移、能够删除对象壳
> 数据产品或完整循环的候选。S0、T8 默认 `stage.dat` 与 Android 真机边界不变。

> U6 2026-08-15 更新（第七十三切片，FrameAdvance 栈上值状态运动内核候选，性能门禁否决并完全撤回）：候选在
> exact-character FrameAdvance 外壳内一次捕获运动字段，在栈上执行与对象内核同顺序的边界、位移、摩擦、落地和重力，
> 最后统一提交；原 `DataOriented` 对象内核作为直接 oracle。候选编译无 C# error，聚焦等价/0-allocation 检查
> `23/23 PASS`。相同 seed、1000 AI、30 warmup + 180 sample 的对照中，目标 `FrameAdvance` average 仅从
> `1.0834317` 降到 `1.0798278 ms`（只差 `0.0036039 ms`），P95 反而由 `1.14388` 增至 `1.18296 ms`；
> 完整 tick average/P95 由 `19.0017367/23.103635` 回退到 `19.1108328/23.801165 ms`，P99 也更差。两轮
> 正式 tick 均为 `0 B`、三代 collection 0，lockstep hash 一致且 teardown 完整恢复。候选代码、枚举和测试已全部
> 撤回，不保留隐藏开关；撤回后 FrameAdvance 聚焦 job `1c0c8e8844d14775901c637277653c8e` 为 `22/22 PASS`，
> 完整 `BattleRuntimeSelfCheck` 于 `2026-08-15 07:24:57 PASS`。完整证据见 canonical inventory 第 65 节。
> 该负实验不计入 U6 收益，不关闭 U6/U9；后续停止对 FrameAdvance 做字段级搬运，只接受能删除完整循环或跨 pass
> 数据产品的候选。S0、T8 默认 `stage.dat` 与 Android 真机边界不变。

> U6 2026-08-15 更新（第七十四切片，中央表现不可变发布边界审计，无代码候选）：fresh 调用链确认
> `CaptureEntities` 是逻辑可变实体到表现宿主之间的不可变 publication 边界，不是前序排序的重复结果。详细基线中
> `CaptureEntities` average/P95 为 `0.93838/0.999145 ms`，`CaptureHitRecords` 为 `0.08288 ms`，而
> command resolve/mesh 写入为 `2.92792 ms`。当前没有覆盖 generation、frame source、visible、flip、shadow、
> holder/layer、销毁与复用等全部表现写入点的完整 dirty/version 合同；直接删除捕获或只更新部分字段会使渲染读取
> 可变/陈旧 runtime，也会破坏 U8 专用 simulation worker 所需的不可变发布边界。因此本切片不修改生产代码、
> 不创建隐藏增量开关，也不重复运行没有代码差异的 1000 AI；完整审计见 canonical inventory 第 66 节。该结论不
> 计入 U6 性能收益，不关闭 U6/U9；中央表现后续只接受针对 command resolve/mesh 完整产品、并有同口径 A/B 与
> parity 证据的候选。S0、T8 默认 `stage.dat` 与 Android 真机边界不变。

> U7 2026-08-15 更新（第一切片，已消费权威输入帧历史环，已实现并通过聚焦验证）：新增普通主类
> `LockstepFrameHistoryRing`，每个固定容量 cell 在初始化时一次性拥有 `SimulationPlayerInput[]` 与可复用
> `FrameInputSet`，按严格连续 tick 复制已消费 canonical 输入并记录 schema version、session identity fingerprint
> 与 canonical input hash；环覆盖旧帧、按 tick 查询、reset 后连续窗口以及调用方输入数组隔离均不依赖每 tick
> `new`。`BattleLockstepSession` 在同一 explicit-frame transaction 成功后同时写 replay journal 与 frame history，
> reset 同步重置两者；默认 history 容量沿用 journal 容量，也可在构造时显式配置。该切片没有实现服务器、Socket、
> ACK、Jitter Buffer、房间或重连业务，也没有改变 LocalFreeRun/1000 AI 的战斗 tick 与性能。
>
> fresh Unity 聚焦 EditMode job `32557187a65a4a23b35dd3b5a703122a` 为 `22/22 PASS`，覆盖环绕淘汰、顺序/重复
> 拒绝、输入存储隔离、session 接线和预热后 1024 次记录/查询 `0 B`；完整 `BattleRuntimeSelfCheck` 于
> `2026-08-15 07:39:41 PASS`。Unity Console 未观察到项目 C# 编译错误，唯一 Error 为 UnityMCP client
> disposed-object 连接噪声。该证据只完成 U7 的 FrameHistory 第一切片；`BattleStateSnapshot`、`SnapshotRing`、
> `ChecksumHistory`、restore + journal replay、Windows/跨运行时门禁仍未实现，因此 U7/U9 继续保持未完成，S0
> 仍不得启动。

> U7 2026-08-15 更新（第二切片，权威状态 checksum 历史环，已实现并通过聚焦验证）：新增普通主类
> `LockstepChecksumHistoryRing`。该 ring 以固定容量数组保存 tick、canonical input hash、checksum algorithm
> schema 与 64 位状态 checksum，并携带 protocol schema 和 session identity fingerprint；覆盖、按 tick 查询、
> reset 与非法非连续写入均与 `FrameHistoryRing` 使用相同生命周期规则。checksum 未启用时仍写入该 tick 和 input
> hash，但以 schema `0` 明确标记“本帧没有状态 checksum”，避免两个历史窗口静默错位或用数值 `0` 冒充有效校验。
> `BattleLockstepChecksumModule` 只把原私有常量公开为程序集内 `CurrentSchemaVersion = 3`，计算字段、顺序和算法未变。
> `BattleLockstepSession` 在 explicit frame 成功提交后写入 FrameHistory 与 ChecksumHistory，reset 同步清空；默认
> checksum history 容量与 FrameHistory 一致，也可在构造时单独配置。该接线没有强制 LocalFreeRun 开启 checksum，
> 没有改变战斗 pass、表现构建、服务器边界或 1000 AI 默认压力路径。
>
> 强制 Unity 域刷新后未观察到项目 C# 编译错误；有效聚焦 EditMode job
> `78bd19abc2f1464bbb50961de1f36bf1` 为 `26/26 PASS`，覆盖 checksum 可用/不可用两种生命周期、环覆盖、schema/
> identity、session 对齐、reset、replay checksum 和预热后 1024 次记录/查询 `0 B`。此前逗号拼接筛选产生的
> `0 tests` job `e38e487972fa4c36b4686cb2e2a38de7` 明确作废，不作为证据。完整 `BattleRuntimeSelfCheck` 于
> `2026-08-15 07:47:32 PASS`；`git diff --check` 无 whitespace error。该切片只完成 ChecksumHistory，
> `BattleStateSnapshot`、`SnapshotRing`、restore + journal replay、Windows/跨运行时门禁仍未实现，因此 U7/U9
> 继续保持未完成，S0 仍不得启动。

> U7 2026-08-15 更新（第三切片，世界核心标量快照，已实现捕获但尚未提供恢复）：新增普通主类
> `BattleWorldCoreScalarSnapshotModule` 及一组只读值类型快照，覆盖 snapshot/protocol schema、session identity
> fingerprint、runtime profile/capacity/collision backend、对象与已占用槽计数、Match/Stage/Progression/Flow 的
> 标量状态、确定性 RNG state/call count、camera_x/velocity 以及 next auto stable id。`SimulationWorld` 是字段采集的
> 唯一 owner，`BattleLockstepSession.TryCaptureWorldCoreScalarSnapshot` 只在协议状态正常且 session tick 与 driver tick
> 对齐时发布快照。所有数据均为值复制；调用方后续修改 world 不会改变已取得的快照，预热后的重复捕获不产生托管分配。
>
> 该类型被有意命名为 `CoreScalarSnapshot`，而不是完整 `BattleStateSnapshot`。当前尚未覆盖 roster/results、stage
> 可变容器、runtime slot/generation/entity payload、aRest/vRest、索引、pending lifecycle/event queue，因此没有暴露
> Restore API；在这些状态簇未闭合前恢复核心标量会制造“看似成功但丢失战斗状态”的错误实现。fresh Unity Console
> 项目编译错误为 `0`；有效聚焦 EditMode job `bf9cc13afe9c433493a54963f7691f43` 为 `19/19 PASS`，覆盖不可变值、
> schema/identity、session 接线以及预热后 1024 次捕获 `0 B`；完整 `BattleRuntimeSelfCheck` 于
> `2026-08-15 07:57:32 PASS`。该切片没有接入 LocalFreeRun/1000 AI 热路径，也没有实现服务器、Socket、ACK、
> Jitter Buffer、房间或重连。U7/U9 继续保持未完成，S0 仍不得启动；下一切片先闭合可预分配的 roster/results
> 状态簇，再处理实体槽与 rest/lifecycle 重建边界。

> U7 2026-08-15 更新（第四切片，固定 roster/results 状态簇，已实现捕获但尚未提供恢复）：新增普通主类
> `BattleWorldRosterResultsSnapshotBuffer` 与 `BattleWorldRosterResultsSnapshotModule`。缓冲在构造阶段一次性分配
> roster 8 槽、结果表 `2×11`、slot label `10×12`、kill/damage 3 项以及 reserve `2×11` 的固定存储；战斗中只
> 原位复制，不返回内部数组，也不按帧创建 class。覆盖字段包括每个 roster slot 的 active/human/character/team/
> input/AI/runtime slot/stable id、Results 全部标量与固定数组/矩阵、slot labels、kill/damage stats、reserve owner
> 与 committed 矩阵，并携带 snapshot/protocol schema、identity fingerprint 和 captured tick。
>
> 捕获前会先完整验证全部固定容器的空值、长度与矩阵维度；任一契约不成立时整次返回 false，目的缓冲保持上一份
> 完整内容，不产生半写快照，也不在错误路径替换或分配源容器。`BattleLockstepSession` 仅在协议正常且 session/
> driver tick 对齐时调用 world-owned 模块。fresh Unity Console 项目编译错误为 `0`；聚焦 EditMode job
> `4466f8b0c641481ea7b35326d4b3fdc9` 为 `22/22 PASS`，覆盖字段隔离、异常源 fail-closed、不半写以及预热后
> 1024 次捕获 `0 B`；完整 `BattleRuntimeSelfCheck` 于 `2026-08-15 08:05:05 PASS`，`git diff --check` 无
> whitespace error。该切片仍不是完整 `BattleStateSnapshot`，没有 Restore，也没有接入 LocalFreeRun/1000 AI 热路径；
> stage spawn 可变容器、实体槽/payload、aRest/vRest、索引与 pending lifecycle/event queue 仍未闭合。因此 U7/U9
> 继续保持未完成，S0 仍不得启动。

> U7 2026-08-15 更新（第五切片，stage spawn 可变状态簇，已实现捕获但尚未提供恢复）：新增普通主类
> `BattleWorldStageSpawnSnapshotBuffer` 与 `BattleWorldStageSpawnSnapshotModule`。每个 spawn entry 复用现有
> `StageSpawnRuntimeBufferPool.SlotsPerSpawnEntry = 40` 契约；entry capacity 在 bootstrap 阶段从已加载 campaign 的
> 单 phase 最大 spawn 数计算，并由显式命名的 `CreateStageSpawnSnapshotBufferForBootstrap` 一次性分配。捕获阶段只复制
> runtime wave、target total、entry count、spawned total 与 `entry×40` runtime slots，不复制静态 campaign 对象图；
> campaign 身份继续由 session stage fingerprint 负责。
>
> 捕获会先验证四组列表 count 完全一致、active entry 不超过预分配 capacity、每个 slot buffer 非空且恰为 40；任一
> 条件失败时返回 false，上一份完整快照保持不变，且不会调用 List 扩容或创建替代数组。fresh Unity 未观察到项目 C#
> 编译错误；Console 中三条 Error 均为 UnityMCP disposed-object 连接噪声。聚焦 EditMode job
> `1f9b59aaf5d74adc94142eca74fec004` 为 `25/25 PASS`，覆盖 bootstrap 容量推导、值隔离、容量溢出 fail-closed、
> 不半写以及预热后 1024 次捕获 `0 B`；完整 `BattleRuntimeSelfCheck` 于 `2026-08-15 08:10:30 PASS`，
> `git diff --check` 无 whitespace error。该切片没有 Restore、没有周期性 SnapshotRing，也没有接入 LocalFreeRun/
> 1000 AI 热路径；runtime slot/generation/entity payload、aRest/vRest、索引、pending lifecycle/event queue 仍未闭合，
> 因此 U7/U9 继续保持未完成，S0 仍不得启动。

> U7 2026-08-15 更新（第六切片，runtime slot ownership/generation 状态簇，已实现捕获但尚未提供恢复）：新增普通主类
> `BattleWorldRuntimeSlotSnapshotBuffer` 与 `BattleWorldRuntimeSlotSnapshotModule`。缓冲在 bootstrap 阶段按 world 的
> logical runtime capacity 一次性分配，捕获全部槽位的 claimed bitmap 与 generation；generation 不只保存活动槽，也保存
> 已释放槽，因为回滚后若把未占用槽 generation 重置为零，旧 `RuntimeEntityHandle` 可能错误复活。每个 claimed 槽同时
> 保存实体运行时种类、stable id、object id、当前 DAT object id/type、runtime obj/entity type 与 spawn semantic，作为后续
> entity payload 恢复与对象池重绑定的身份前置条件。
>
> `RuntimeSlotAllocator` 的内部最小堆不直接序列化：审计确认按槽位升序用 claimed bitmap 重建时，已释放/跳过槽仍会进入
> 相同的最低空闲槽序列，因此决定未来分配结果的是 claimed 集合而不是堆数组布局；但 generation 必须逐槽恢复。捕获前先
> 校验 claimed count、occupant、generation、raw runtime 物化与 `entity.Runtime.SlotIndex` 契约，失败时不覆盖上一份快照。
> 审计同时确认 `RuntimeSlotTable.RawRuntime` 是独立 raw-slot 状态，并非 `LF2Entity.Runtime` 的同一引用；它将与完整
> `NTSDEntityRuntime` payload 一并进入后续状态簇，当前切片没有错误地把二者合并。
>
> fresh Unity 项目编译为 `0 error`；扩大后的 U7 聚焦 EditMode job
> `35b28dd5c776422ab9dc7c89793d2b1c` 为 `28/28 PASS`，覆盖 claimed 身份隔离、已释放槽 generation、容量不匹配
> fail-closed 与预热后 1024 次捕获 `0 B`；完整 `BattleRuntimeSelfCheck` 于 `2026-08-15 08:20:41 PASS`。
> 该切片仍没有 entity/raw-slot 完整 payload、aRest/vRest、pending lifecycle/event、派生索引重建、SnapshotRing 或 Restore；
> 也未接入 LocalFreeRun/1000 AI 热路径。因此 U7/U9 继续保持未完成，S0 仍不得启动。

> U7 2026-08-15 更新（第七切片，aRest/vRest canonical rest 状态簇，已实现捕获但尚未提供恢复）：新增普通主类
> `BattleWorldRestSnapshotBuffer` 与 `BattleWorldRestSnapshotModule`，并由 `RuntimeRestStore` 提供只读 canonical 状态复制边界。
> bootstrap 创建缓冲时先执行现有 `PrepareForBattle`，随后按 runtime profile 已准备好的实际存储模式一次性分配：
> Authority400 与 Mobile（logical capacity `<= 2048`）采用连续 dense `int[capacity × capacity]` vRest，Desktop
> 超过该阈值时采用预分配 sparse `(victimSlot, attackerSlot, value)` 三列；aRest 始终为连续 `int[capacity]`。
> 该选择镜像正式 rest store，避免在 400/1000/2048 槽常用配置中为 sparse 三列付出额外空间与遍历成本，也避免 Desktop
> 无上限模式强制分配不可控的平方矩阵。sparse 捕获按 victim slot 升序、行内 attacker slot 升序发布，顺序确定；缓冲容量或
> storage mode 不匹配时 fail-closed。rest binding token 不序列化：它属于实体与 store 的派生绑定，未来 Restore 时将由 slot/generation
> 与实体重建流程重新绑定，不能把旧 CLR 引用或旧 token 当作可恢复真值。
>
> fresh Unity 项目编译为 `0 error`；扩大后的 U7 聚焦 EditMode job
> `527d3d2c1fb74960988a25206d6feb90` 为 `31/31 PASS`，覆盖 dense canonical 值与所有权隔离、Desktop 2304 槽
> sparse 模式、容量/模式契约以及预热后 dense 256 槽 1024 次捕获 `0 B`；完整 `BattleRuntimeSelfCheck` 于
> `2026-08-15 08:28:52 PASS`。该切片没有接入 LocalFreeRun/1000 AI 热路径，也没有暴露 Restore。entity/raw-slot
> 完整 payload、pending lifecycle/event、派生索引/store binding 重建、SnapshotRing 与 restore + journal replay 仍未闭合，
> 因此 U7/U9 继续保持未完成，S0 仍不得启动。

> U7 2026-08-15 更新（第八切片，entity/runtime-slot raw payload 状态簇，已实现捕获但尚未提供恢复）：
> `NTSDEntityRuntime` 新增内部显式 canonical copy 契约，逐字段复制 identity、输入历史与边沿、位置/速度/朝向、frame/state/
> hit-stop、生命周期抑制 tick、link/holder/target、命中与统计、HP/PP/MP、边界、候选缓存、pending flush 标志与 mutation epoch；
> `[NonSerialized]` 的 world/store 引用和 store slot binding 不复制，未来 Restore 后由 world-owned 注册流程重建。
> `BattleWorldEntityRuntimeSnapshotBuffer` 在 bootstrap 阶段按 logical slot capacity 一次性预分配两套 runtime payload：一套对应
> claimed `LF2Entity.Runtime`，另一套对应 `RuntimeSlotTable.RawRuntime`。两者由独立 presence bitmap 标记，明确保留 raw4-style
> slot 状态与实体对象状态不是同一 CLR 引用的事实；未物化 raw page 不被捕获为伪对象，已物化但未 claimed 的 raw slot 仍会保存。
> 捕获前完整验证 claimed/entity/generation/raw-runtime/slot index 与所有 input-history 固定长度，验证失败时不发布新 schema/count 元数据。
>
> 正式捕获不使用反射；Editor 覆盖测试才通过反射枚举所有非 `[NonSerialized]` instance field，逐字段比较源与已复制 payload，
> 从而让今后新增 runtime 字段但遗漏 snapshot copy 成为可见测试失败。fresh Unity 编译与 Console 均为 `0 error`；扩大后的 U7
> 聚焦 EditMode job `00be8153f73f42d0a506389fb3ac9730` 为 `35/35 PASS`，覆盖 entity/raw 独立性、未 claimed
> 已物化 raw slot、源对象后续修改隔离、无效输入存储 fail-closed、容量契约以及预热后 1024 次捕获 `0 B`；完整
> `BattleRuntimeSelfCheck` 于 `2026-08-15 08:37:22 PASS`。该切片仍没有捕获 `LF2Entity`/派生类外壳状态，尚未闭合
> pending lifecycle/event、派生索引/store binding 重建、SnapshotRing 与 restore + journal replay，也未接入 LocalFreeRun/1000 AI
> 热路径；因此 U7/U9 继续保持未完成，S0 仍不得启动。

> U7 2026-08-15 更新（第九切片，所有 `LF2Entity` 的基础外壳状态簇，已实现捕获但尚未提供恢复）：
> 新增普通主类 `BattleWorldEntityBaseShellSnapshotBuffer`，按 runtime slot 捕获每个 claimed 实体的 required slot、当前 itr
> 索引、tracker-parent generation handle、当前/历史/碰撞帧编号及 DAT frame id、`FrameTransistor`、完整 `LF2EffectState`、
> 不与 runtime 别名的 `PhysicsState` 地面/朝向/摩擦/深度偏移/边界字段，以及固定十槽 hit-record 的 age、X/Z 与
> last-advance tick。DAT frame 对象、Sprite、Renderer、GameObject 和其他 Unity 表现引用不进入恢复真值；未来 Restore
> 依据资源身份与 frame id 重绑。非空 tracker-parent 必须可解析为当前 generation handle，claimed count、slot/entity/
> runtime、frame/trans/effect/physics 与 hit-record 容量任一不满足时 fail closed，且不发布新的 schema/count 元数据。
> 缓冲区在 bootstrap 预分配全部数组，正式捕获不使用反射或临时容器。
>
> fresh Unity 编译与 Console 为 `0 error`；扩大后的 U7 聚焦 EditMode job
> `77c1d5b90502409bb280c44eae7277e0` 为 `39/39 PASS`，覆盖完整基础壳字段、tracker handle、源对象后续修改隔离、
> 未注册 tracker fail-closed、容量契约以及预热后 1024 次捕获 `0 B`；完整 `BattleRuntimeSelfCheck` 于
> `2026-08-15 08:47:59 PASS`。该切片尚未捕获 `LF2LivingObject`、Character/Weapon/SpecialAttack 等派生外壳字段，
> 也未闭合 pending lifecycle/event、派生索引/store binding 重建、聚合 `BattleStateSnapshot`、`SnapshotRing` 与
> restore + journal replay；因此 U7/U9 继续保持未完成，S0 仍不得启动。

> U7 2026-08-15 更新（第十切片，`LF2LivingObject` 公共外壳状态簇，已实现捕获但尚未提供恢复）：新增普通主类
> `BattleWorldLivingShellSnapshotBuffer`，按 runtime slot 捕获 living 对象的 `Dead`、无效 frame transition 诊断计数、
> `Catching`/`Attacker` 当前 generation handle，以及 `LF2HitCountersModule` 中不与 runtime 别名的 fall/bdefend 小数恢复
> 累加器。HP/PP、fall/bdefend 整数值和输入历史已由 runtime payload 捕获；Controller、Health/ItrRest binding 与其他
> store adapter 是 Restore 后重建的宿主绑定，不作为 CLR 引用序列化。任一非空 living 引用不能解析为当前 generation、
> runtime slot/Health/ItrRest/HitCounters 不完整或容量不匹配时捕获 fail closed，且不会发布半写元数据；缓冲区只在
> bootstrap 分配，热捕获不创建 List、Dictionary、委托或反射对象。
>
> fresh Unity 脚本编译为 `0 error`；扩大后的 U7 聚焦 EditMode job
> `cd605ebd7abd4d7080e53c81db0a3aaf` 为 `43/43 PASS`，覆盖 generation handle、死亡/诊断/小数恢复状态、源对象隔离、
> 未注册引用 fail-closed、容量契约以及预热后 1024 次捕获 `0 B`；完整 `BattleRuntimeSelfCheck` 于
> `2026-08-15 08:55:13 PASS`。该切片仍未捕获 Character/Weapon/SpecialAttack 专属外壳字段，也未闭合 pending
> lifecycle/event、派生索引/store binding 重建、聚合 `BattleStateSnapshot`、`SnapshotRing` 与 restore + journal replay；
> 因此 U7/U9 继续保持未完成，S0 仍不得启动。

> U7 2026-08-15 更新（第十一切片，`LF2Character` 专属外壳状态簇，已实现捕获但尚未提供恢复）：新增普通主类
> `BattleWorldCharacterShellSnapshotBuffer`，按 runtime slot 捕获 held-object 当前 generation handle、角色 mass、死亡闪烁
> 计数、是否由 opoint 初始化以及 action-zero 保留语义。按键历史、CaughtDuration/CaughtFront 和 held slot/link 数值已由
> runtime payload 捕获；`InputState`、Controller、resolver/module 与 Unity `Transform` 是 Restore 后从 runtime/host 重建的
> adapter，不序列化为第二份对象图，held CLR 引用则只保存稳定 generation handle。任一非空 held 引用不是当前 world 的
> `LF2Entity`、角色 runtime/input/controller/hit-counter 不完整或容量不匹配时捕获 fail closed，不发布半写元数据；正式捕获
> 只写 bootstrap 预分配数组。
>
> fresh Unity 脚本编译为 `0 error`；扩大后的 U7 聚焦 EditMode job
> `6561e814a04a442f90cd1ffd3fab36fa` 为 `47/47 PASS`，覆盖 held generation handle、角色专属标量、源对象隔离、未注册 held
> 引用 fail-closed、容量契约及预热后 1024 次捕获 `0 B`；完整 `BattleRuntimeSelfCheck` 于
> `2026-08-15 09:00:51 PASS`。Weapon/SpecialAttack/OtherObject 专属外壳、pending lifecycle/event、派生索引/store
> binding 重建、聚合 `BattleStateSnapshot`、`SnapshotRing` 与 restore + journal replay 仍未闭合；因此 U7/U9 继续保持
> 未完成，S0 仍不得启动。

> U7 2026-08-15 更新（第十二切片，`LF2WeaponBase`/`LF2Weapon` 专属外壳状态簇，已实现捕获但尚未提供恢复）：
> 新增普通主类 `BattleWorldWeaponShellSnapshotBuffer`，按 runtime slot 捕获武器 late-break 副作用是否已经处理、无效 init
> task 诊断计数、本帧重力累加量、落地 clamp 前的 Vy，以及具体 `LF2Weapon` 的对象池武器类型。飞行计数、weapon count、
> weapon type、holder/link 与生命值已由 runtime payload 捕获；weapon-strength list、声音名和 Controller 是 DAT/宿主资源绑定，
> Restore 后重建，不复制引用图。武器 runtime/Health/ItrRest 不完整或容量不匹配时捕获 fail closed，并保持上一份已发布
> 元数据；正式捕获仅写 bootstrap 预分配数组。
>
> fresh Unity 脚本编译为 `0 error`；扩大后的 U7 聚焦 EditMode job
> `c08bcf866b0740669a0428fb3cbeb21d` 为 `51/51 PASS`，覆盖重力/落地/late-break/池类型、源对象隔离、无效模块
> fail-closed、容量契约及预热后 1024 次捕获 `0 B`；完整 `BattleRuntimeSelfCheck` 于
> `2026-08-15 09:05:21 PASS`。SpecialAttack/OtherObject 专属外壳、pending lifecycle/event、派生索引/store binding
> 重建、聚合 `BattleStateSnapshot`、`SnapshotRing` 与 restore + journal replay 仍未闭合；因此 U7/U9 继续保持未完成，
> S0 仍不得启动。

> U7 2026-08-15 更新（第十三切片，`LF2SpecialAttack`/`LF2OtherObject` 专属外壳状态簇，已实现捕获但尚未提供恢复）：
> 新增普通主类 `BattleWorldSpecialOtherShellSnapshotBuffer`，用显式 kind 区分两类 payload；SpecialAttack 捕获 parent 当前
> generation handle、上一 state、`NoBounce` 与无效 init task 诊断计数，OtherObject 捕获其 lifecycle module 的无效 init task
> 诊断计数。Health/ItrRest 与数值型 owner/link/运动字段由既有 runtime/rest payload 捕获；frame/lifecycle module 自身只持有
> owner adapter，不复制 CLR 引用。SpecialAttack 非空 parent 不能解析为当前 generation、任一目标类型 runtime/Health/ItrRest
> 不完整或容量不匹配时捕获 fail closed，正式捕获只写 bootstrap 预分配数组。
>
> fresh Unity 脚本编译为 `0 error`；扩大后的 U7 聚焦 EditMode job
> `eff8d216b2354dad8d2ababc3bcf4d1b` 为 `55/55 PASS`，覆盖 parent generation handle、special/other 独立字段、源对象隔离、
> 未注册 parent fail-closed、容量契约及预热后 1024 次捕获 `0 B`；完整 `BattleRuntimeSelfCheck` 于
> `2026-08-15 09:11:13 PASS`。已知实体继承层的独立可变壳字段捕获至此基本闭合，但 pending lifecycle/event、派生索引/
> store binding 重建、聚合 `BattleStateSnapshot`、`SnapshotRing` 与 restore + journal replay 仍未闭合；因此 U7/U9 继续
> 保持未完成，S0 仍不得启动。

> U7 2026-08-15 更新（第十四切片，formal-boundary pending event/lifecycle 契约，已实现捕获但尚未提供恢复）：新增普通
> 主类 `BattleWorldPendingEventSnapshotBuffer`，按原队列顺序复制 checksum 可见的 `PendingSoundEvent` cue/worldX/tick；
> cue 必须非空，容量在 bootstrap 按 world-owned battle buffer 的已准备容量一次性分配。`PendingUnregister` 与
> `PendingSlotReleasedDestroy` 不保存 CLR 引用：它们按正式 tick 边界必须已经排空，任一非空即 fail closed，防止把已释放
> runtime slot、renderer adapter 或不可解析对象伪装成可恢复 handle。旧完整快照元数据在所有失败路径保持不变。
>
> fresh Unity 脚本编译为 `0 error`；扩大后的 U7 聚焦 EditMode job
> `5c0794dc9a8940e8b1fb89a6f508b755` 为 `59/59 PASS`，覆盖声音顺序/内容与源队列隔离、lifecycle 边界不变量、容量/
> cue fail-closed 以及预热后 1024 次捕获 `0 B`；完整 `BattleRuntimeSelfCheck` 于
> `2026-08-15 09:16:31 PASS`。已知 pending event 内容与 pending lifecycle 正式边界至此闭合；派生索引/store binding
> 重建、聚合 `BattleStateSnapshot`、`SnapshotRing` 与 restore + journal replay 仍未闭合，因此 U7/U9 继续保持未完成，
> S0 仍不得启动。

> U7 2026-08-15 更新（第十五切片，聚合快照、固定容量快照环及同拓扑精确恢复）：新增普通主类
> `BattleStateSnapshotBuffer`、`LockstepSnapshotRing` 与 `BattleStateSnapshotRestoreModule`。恢复器先完整预验证 session/schema、
> runtime profile、容量、slot generation、稳定身份、DAT shell、关系 handle 与各快照域，再按 world core、runtime payload、
> rest、实体继承外壳、派生索引/store binding、pending event 的顺序提交；任何预验证失败均不修改世界。恢复后的 driver tick、
> spark frame、tick policy、输入/校验缓存及已发布声音也会同步回到快照边界。failure enum 已细分到 base/living/character/
> weapon/special-other shell、required slot、frame data、tracker parent 与 topology 等具体原因，避免以单一 `EntityShellMismatch`
> 掩盖真实故障。
>
> U7 2026-08-15 更新（第十六切片，本地进程 lifecycle topology 恢复与 journal replay）：runtime-slot 快照在同一运行时内保留
> 每个 claimed slot 的本地纯 C# entity shell 引用；恢复时先解除当前 roster/mount/bucket 绑定，再按快照的 claimed set 与
> generation 重建 slot allocator、对象桶、world 绑定和派生 stores。该路径能够移除快照后新增的 future slot，也能恢复快照后
> 被注销的 shell，并让旧 future handle 因 generation/topology 不匹配而失效。`BattleLockstepSession.TryRestoreAndReplay` 在恢复前
> 校验目标 tick 的历史输入与 checksum，恢复后按原 journal 逐 tick 重放，既不重写历史环，也会再次到达原目标 checksum。
>
> fresh Unity 聚焦 EditMode job `6204af59a8c64d5ea96d034f2c886a18` 为 `8/8 PASS`，覆盖精确状态恢复、关系/rest/pending
> 恢复、生命周期 claimed/generation 恢复、身份不匹配的无修改 fail-closed、普通 restore/replay、删除实体后的 restore/replay、
> checksum 预验证和 warm exact restore `0 B`。该证据完成的是“同一 Unity 运行时中的本地恢复与重放闭环”；快照仍包含本地
> shell 引用，不可序列化为跨进程 payload，也尚未提供基于静态战斗数据目录的纯值 entity factory。Windows IL2CPP 与跨运行时
> 门禁尚未执行，所以 U7 尚不能整体标完成；U8/U9 未完成，S0、Socket、ACK、Jitter Buffer、房间、登录和重连继续排除。
