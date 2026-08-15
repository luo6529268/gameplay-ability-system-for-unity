# U6 RuntimeSlotTable 页式 SoA 第一切片

日期：2026-08-12

状态：实现与本切片验收完成；U6 整体仍在执行。

## 1. 本切片目标

U6 的最终目标是让 BattleKernel 成为唯一战斗真值，并让 `LF2Character`、`LF2Weapon` 与其他 `LF2Entity` 逐步退为 Unity shell/兼容 adapter。第一切片只处理注册表存储地基：消除 `RuntimeSlotTable` 每个地址槽的 `Entry` 引用对象和重复 claimed 状态，同时保持现有外部行为。

该切片不是 canonical ownership 切换，不把当前 `BattleEcsWorld` read-only shadow 直接晋升为正式 writer，也不删除 `LF2Entity` / `NTSDEntityRuntime` 兼容字段。

## 2. 实现边界

修改文件：`Assets/NTSD/Scripts/Simulation/RuntimeSlotTable.cs`。

原先每个物化槽由一个 `Entry` 对象保存：

- `NTSDEntityRuntime RawRuntime`；
- `LF2Entity Entity`；
- `uint Generation`；
- `bool Claimed`。

现在每个按需物化的 page 分别保存：

- `NTSDEntityRuntime[] RawRuntimes`；
- `LF2Entity[] Entities`；
- `uint[] Generations`。

claimed 状态只读取 `RuntimeSlotAllocator.IsClaimed(slot)`。这删除了 table 与 allocator 之间可能分叉的第二份 claimed 真值，也避免为每个物化槽额外创建一个 `Entry` 引用对象。

以下合同保持不变：

- 页式按需物化和 `MaterializedPageCount`；
- Authority400 与 Extended1000 容量；
- 最低空闲槽分配顺序；
- claim/release/reset 的 generation 递增；
- stale handle 失效与 occupant 引用检查；
- `GetRawRuntime`、`TryResolve`、`TryGetReadOnlyView` 等公开调用语义；
- 对象池、表现绑定、战斗 pass 顺序和 opoint 可见边界。

## 3. 验证证据

### Unity 编译与自动检查

- Unity scripts 强制刷新后 C# 编译为 0 error；
- 初次 slot/registration/lifecycle 聚焦 EditMode job `f63f06023aff474aa8b739efd176a518`：258/258 PASS；
- 最终回退后聚焦 EditMode job `1249c3dfb49d4324973b1942dde1e9cd`：258/258 PASS；
- 完整 `BattleRuntimeSelfCheck`：`2026-08-12 21:20:47` fresh PASS。

### Authority400

- Unity trace：`Temp/NTSDParity/u6-slot-page-soa-final-unity-authority-dat-diagnostic-20260812.jsonl`；
- 比较报告：`Temp/NTSDParity/u6-slot-page-soa-final-compare-authority-dat-diagnostic-20260812.json`；
- 结果：6/6 tick `equal-diagnostic`，`firstDifference=null`；
- 该结果属于 authority-DAT diagnostic，不是 production certificate。

### 1000 AI

报告：`Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-performance-smoke.json`。

- 1000 个真实生产 GameObject 与 1000 个逻辑实体；
- 30 warmup + 180 正式样本；
- average/P95/max：21.1567/25.5118/28.8231 ms/tick；
- 正式 tick average/max allocation：0/0 B；
- Gen0/1/2 collection：0/0/0；
- teardown：`restored=true`，active GameObject/world entity/claimed slot 均恢复为 0；
- 最终 lockstep overall hash：`4378ba4c0c56c3f17ea3327b91ffdf4af39e1d1c20152b5c92c4dda7d3867063`；
- 最大 backlog 7，丢弃 backlog 28。

本切片没有用非相邻运行时波动声称性能提升。报告证明保留实现维持单 tick 预算、零 GC、确定性 hash 和清理恢复；外层仍出现 backlog 丢弃，因此 U9 的稳定 30 FPS 门禁仍未关闭。

## 4. 被拒绝的第二切片候选

曾实现把 `PendingFlushDestroy` 从每个 `NTSDEntityRuntime` 迁移到世界持有、按 slot/generation 校验的连续存储，并保留旧存储诊断开关做同场景相邻 A/B。两次运行均为 1000 个真实生产 GameObject/逻辑实体、30 warmup + 180 sample，且最终 lockstep hash 相同、正式 tick 均为 0 B、Gen0/1/2 collection 均为 0：

- Legacy 字段存储：average/P95/max 21.2927/26.1168/33.8060 ms/tick；
- 世界级 canonical 存储：average/P95/max 23.2539/27.4474/34.8992 ms/tick；
- canonical 候选平均耗时回退约 9.2%。

该候选行为正确，但在当前对象兼容层中每次访问多出 store/handle 转发，未减少对应对象式热循环，属于负优化，未晋升为正式实现；相关代码、诊断开关和测试夹具均已回退。这个结果不否定 U6 的方向，而是限定后续迁移必须以整条热路径为单位，不能只把单个高频字段换一个存放位置。

## 5. 下一切片

先完成 `RuntimeSlotTable.GetRawRuntime`、`LF2Entity.EntityRuntime` 及 `BattleEcsWorld` 对应字段的完整读写盘点，确认同 tick 可见性、reset、generation 和结构播放边界，再选择一个低耦合字段组迁移为 BattleKernel canonical storage。

下一切片必须满足：

- 不把 tick-end shadow 当成同 tick 真值；
- 不新增全局可变 static；
- 不新增 partial；
- 保留可回退诊断直到行为、零 GC 和性能门槛成立；
- fresh compile、聚焦测试、完整 self-check、Authority400 和 1000 AI 回归均通过后才晋升。
