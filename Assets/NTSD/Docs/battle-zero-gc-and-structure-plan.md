# NTSD 战斗零 GC 与运行时结构收敛计划

更新日期：2026-08-10

## 1. 目标与边界

本计划只处理 Unity NTSD 战斗运行时的托管内存、运行时所有权和代码结构，不改变
`J:\QQFile\NTSD2.4\ntsd_release` release live path 定义的战斗规则、pass 顺序、输入、RNG、碰撞、
命中或对象生命周期结果。T8 默认 `stage.dat` 部署和 Android 真机验收继续排除。

“战斗期间不能触发 GC”拆成四个可验证合同：

1. 正式逻辑 tick：`StepOneTick` 的稳态样本必须为 `0 B`，Gen0/1/2 collection 增量为 0。
2. Unity 战斗驱动：未执行 tick 的 `SimulationTickDriver.Update` 与表现 `LateUpdate` 必须为 `0 B`。
3. 完整 PlayerLoop：Player 中从最早 `Update` 到 URP `endFrameRendering` 的托管分配必须为 `0 B`。
4. Player 战斗窗口：预热与容量封印完成后禁用托管 GC，退出战斗时恢复原 GC 模式。

Editor 的 Profiler、GameView、SceneView、IMGUI、MCP、IDE 扩展都与游戏代码运行在同一进程，
因此 Editor 完整 PlayerLoop 只能做观察，不能作为“整个 Unity Editor 进程零 GC”的硬门禁。
Player 构建没有这些 Editor 回调，完整 PlayerLoop 才是正式硬门禁。Player 中禁用 GC 并不允许
继续分配；PlayerLoop 分配门禁用于同时发现“没有 collection、但托管堆持续增长”的错误。

## 2. 当前已完成的内存边界

- `BattleManagedMemoryBoundary` 已分别记录 formal tick、driver Update、presentation LateUpdate、
  PlayerLoop envelope 和 Gen0/1/2 collection。
- `BattleManagedMemoryFrameBeginProbe` 在最早 Update 开始观察；
  `BattleManagedMemoryFrameEndProbe` 在 URP `endFrameRendering` 关闭本帧观察。
- `ProductionEntityStressZeroGcGatePolicy` 在 Player 中把 PlayerLoop envelope 纳入硬门禁；
  Editor 中保留数据但不因 Editor 自身分配误判项目失败。
- 既有 1000 AI 长样本曾取得 120 warmup + 1800 formal ticks 全部 `0 B/tick`、
  Gen0/1/2 正式区间增量为 0；这是 formal tick 证据，不自动扩大为整个 Editor 进程零 GC。

## 3. 已定位并修复的战斗帧分配风险

| 风险 | 原行为 | 当前处理 |
|---|---|---|
| 资源加载驱动空转 | `async void Update()` 每帧进入异步链；空 bucket 长期保留；每帧创建优先级快照 | 空队列同步 O(1) 返回；复用快照；删除空 bucket；全局驱动不再是 async Update |
| 资源暂停任务 | 暂停分支未移动 node，可能在同一节点无限循环 | 暂停后继续访问 previous node；聚焦测试覆盖 |
| 资源任务总表 | `tasks` 链表只插入从不读取/删除 | 删除无效总表，使用精确 `queuedTaskCount` |
| pooled mount owner 查找 | 每次启用 shadow mount 调用返回数组的 `GetComponents<T>()` | 使用实例持有、预设容量的 List 缓冲区 |
| Boundary 自动刷新 | Editor Play Mode 也可能每帧 `FindObjectsOfType` | 自动刷新仅允许非 Play 的场景编辑期；战斗使用初始化快照 |
| Enabled boundaries | `List.FindAll` 每次返回新 List | 复用实例缓冲区 |
| 世界相机兜底查找 | `Camera.allCameras` 返回新数组 | 使用固定预热相机缓冲；正式场景仍优先显式 Bind |
| Loose Quadtree 节点条目 | 每个节点持有可扩容 `List<int>` | 改为预分配数组 + 整数 next 索引链，保持插入/查询顺序 |

## 4. 容量、池与行为完整性合同

零分配不能以静默丢弃技能、命中、声音或对象生成作为代价。正式压力验收除内存门禁外，
还必须确认下列拒绝/耗尽计数在采样区间没有非预期增长：

- runtime slot / object bucket growth rejection；
- `RuntimeRestStore` VRest 写入拒绝；
- collision candidate List rent rejection；
- stage spawn buffer rent rejection；
- LF2 GameObject、SpriteRenderer、logic object、task pool fetch rejection；
- opoint task ring、input buffer、sound event buffer enqueue rejection；
- central presentation mount / owner binding rejection；
- unprepared sound cue 和 one-shot voice capacity drop。

处理原则：战斗前按平台和本局配置预热；战斗开始后封印容量。封印后不得扩容或 new，耗尽时
fail-closed 并增加数值诊断。正式验收要求诊断计数符合该场景的容量政策；不能只看 GC 为 0。

## 5. 数据结构选择规则

不把 `List`/`Dictionary` 机械替换成 `LinkedList<T>`。`LinkedList<T>` 的每个节点都是引用对象，
遍历局部性差，也会把插入/删除变成新的 GC 来源。

| 使用方式 | 首选结构 |
|---|---|
| 每 tick 顺序遍历、索引访问 | 预分配数组或封印容量的 List |
| 频繁插入/删除、需要稳定顺序、不得分配节点 | 数组槽位 + `next`/`previous`/free 整数索引 |
| 最低空闲 slot | 预分配最小堆 |
| 冷路径 key 查找 | 预设容量并在战斗期禁止增长的 Dictionary |
| 空间 broadphase | Loose Quadtree；节点与 entry 存储都必须预分配 |
| 临时收集结果 | 项目引用池、Unity ListPool，或调用方持有的复用 scratch 容器 |

## 6. static 使用规则与待迁移清单

允许保留：常量、只读值表、无状态纯函数、数学/编码 helper、ProfilerMarker 表。
需要迁移：跨战局可写、持有当前 World/Camera/Material/GameObject、带当前帧或当前 publication
状态的 static。static 方法本身不是错误，隐藏的共享可变所有权才是错误。

当前主要待迁移项：

1. `BattleCentralRenderSystem`：持有 feature、renderer、camera、material、published/pending world/frame、
   generation 和诊断状态。目标是实例 `BattleRenderSession` 由战斗 driver/mount 持有，URP feature
   仅保留最小桥接。
2. `NTSDRenderSpace`：持有 bound camera、boundary manager 与逐帧 viewport cache。目标是实例
   `BattleRenderSpaceContext`，由战斗 bootstrap 创建并注入表现模块。
3. `BattleCentralPresentationMountRegistry`：当前 static facade 内部已有实例 State；目标是把 State
   所有权交给 render session，移除进程级 facade。
4. `NTSD_ResourceLoader`、`AudioController`、`GameConfig` 等应用级 singleton：不属于 formal battle
   simulation，但需要通过 bootstrap/main class 持有引用，避免首次访问和跨场景残留。
5. `Log` 等诊断开关：可以保留进程级配置，但正式战斗热路径不得格式化字符串或增长日志缓冲。

## 7. partial 迁移规则

当前仍有 6 个 `SimulationWorld` partial 声明：主文件、Registry、Passes、AiInput、
AiDecisionShadow、AiSoaShadow。禁止增加新的 partial。

迁移不是把所有代码合并成一个巨型 `SimulationWorld.cs`，而是：

1. 为单一职责创建普通实例 module/class；
2. module 构造时接收 world 所需的最小接口或 main class 引用；
3. 状态归 module 实例所有；main class 只持有引用并保留薄转发；
4. 每迁出一块，减少 partial 文件和结构 guard allowlist；
5. 每批都比较 checksum、RNG、slot generation、事件顺序与目标 self-check。

推荐顺序：Registry/容量与生命周期 -> Passes 编排 -> AI sensing/input -> AI decision shadow/SoA。
后两项代码量和状态耦合最大，必须在前置合同稳定后串行迁移。

## 8. 验收顺序

1. C# / Unity 编译 0 error。
2. memory boundary、pool exhaustion、resource loader、quadtree 与 structure guard 聚焦测试。
3. `BattleRuntimeSelfCheck` fresh PASS。
4. 1000 AI：formal tick、完整表现、声音 dispatch、opoint/lifecycle 开启；记录 hash、GC、P95 和所有拒绝计数。
5. Desktop Player 运行完整 PlayerLoop 零分配硬门禁；Android 由用户后续真机验证。

在第 4、5 项完成前，只能说“代码边界已建立/聚焦测试通过”，不能声称整个战斗期间已经
绝对不会发生任何项目分配或 GC。
