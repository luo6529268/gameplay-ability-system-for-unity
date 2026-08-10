# NTSD 战斗运行时零 GC 与结构治理计划

> 建立日期：2026-08-09  
> 当前优先级：高于后续服务器实现、完整 ECS 迁移、T8 默认 `stage.dat` 与 Android 真机验收  
> 战斗逻辑权威：`J:\QQFile\NTSD2.4\ntsd_release_C#`

## 1. 目标与验收边界

本计划的首要目标是在资源加载、对象池预热和战斗世界初始化完成后，使正式战斗窗口不再产生托管堆垃圾，也不在战斗过程中触发托管垃圾回收。优化只能改变数据所有权、容器复用和执行方式，不能改变权威 C# 的 pass 顺序、slot 顺序、RNG 消费、输入边沿、碰撞候选、命中、opoint 或生命周期结果。

“零 GC”必须同时满足：

1. 预热后的普通稳态逻辑 tick 为 `0 B GC.Alloc`。
2. 命中、落地、投掷、opoint、对象销毁与复用等事件 tick 仍为 `0 B GC.Alloc`。
3. 连续 60 秒以上的正式战斗采样中，托管堆不持续爬升，GarbageCollector collection 计数不增加。
4. Editor-only 压测、计时和报告代码不得在采样窗口内制造分配；报告序列化只能在战斗采样停止后执行。
5. 1000 个真实生产 AI 的 `Dispersed1000` 与 `Combat1000` 都要通过上述门禁；只测无输入或 simulation-only 不能代替最终验收。

`GC.GetAllocatedBytesForCurrentThread()` 在当前 Unity 2022/Mono Editor 链路中已经出现漏记，不能再单独作为验收依据。正式证据使用 Unity Profiler 的 `GC.Alloc` 调用栈、ProfilerRecorder 的帧级计数、长窗口托管堆曲线和 collection 计数交叉验证。

## 2. 2026-08-09 已测量分配基线

1000 AI 稳态 290 个逻辑 tick 的调用栈采样：

- 总分配：`2,717,520 B / 13,359 events`。
- Editor 压测诊断：`1,791,312 B`。
- 正式战斗 runtime：`926,208 B`，约 `3,193.82 B/tick`、`39.7 events/tick`。
- 中央表现的 `BuildCommands`、`CaptureEntities`、`SortEntities` 和 `CaptureHitRecords` 在该样本中为 `0 B`；当前持续分配不应归因于中央 Mesh 本体。

主要已定位来源：

| 优先级 | 来源 | 当前证据 | 处理原则 |
|---|---|---:|---|
| Z0 | 压测 detail/phase `List<double>` 从 512 扩到 4096 | 约 `1.76 MB` 的 Editor 峰值 | 战斗前固定容量或固定环形缓冲；报告期再排序 |
| Z1 | 高频实体 pass 每 tick 创建 lambda/闭包/委托 | 正式 runtime 最大的持续分配族，约 `2 KB+/tick` | 改为实例拥有的无分配 slot 游标/显式循环；禁止热路径 `Action<T>` |
| Z2 | `GetBucketKeySnapshot()` 每次 `new List<int>` | `199,520 B / 3,480 calls`，约 12 次/tick | 热路径改为 slot/非分配遍历；快照只留给冷生命周期路径 |
| Z3 | `BuildRendererSnapshot` 的排序选择器委托 | `37,120 B / 290 calls` | 使用世界实例持有的 comparer，原地排序 |
| Z4 | 命中 resolver、落地参数装箱 | resolver `3,320 B`，boxing `144 B` | resolver 归属实体或世界并复用；增加强类型事件入口 |
| Z5 | `SortAndDeduplicate`、pair VRest 等条件路径 | 已定位到方法，仍需细化叶子调用栈 | 复用 scratch、比较器和候选容器；保持 authority 顺序 |
| Z6 | 对象池容量不足、字符串/日志、音频通道和 Unity API | 事件性分配，需 Combat1000 补采样 | 加载期预热；正式战斗不允许池自动扩容或格式化日志进入热路径 |

## 3. `new`、对象池与生命周期规则

战斗开始后禁止不必要的引用类型构造，但不能把规则误写成“源码中不能出现任何 `new`”：值类型局部变量和预先分配的数组元素不会必然产生托管垃圾。真正的运行期约束是：

- 禁止在正式 tick、表现刷新、命中、opoint 和对象释放路径中创建临时 class、数组、字符串、闭包、委托、LINQ 枚举结果或装箱对象。
- 所有容量根据配置在 Loading/PrepareBattle 阶段预热；正式战斗中池耗尽必须输出一次结构化容量错误并 fail closed，不能静默 `new` 扩容。
- `LF2ReferencePool` 负责纯 C# 战斗对象与任务，`LF2ObjectPool` 负责 GameObject/Renderer；Unity `ListPool<T>` 只用于能严格 `Get/Release` 的短生命周期 scratch。
- 池对象必须在 `Release` 时清空 runtime handle、generation、owner、link、holder、target、候选列表和表现绑定，避免复用旧状态。
- 诊断采样缓冲使用固定容量数组或环形缓冲，不在战斗中增长；溢出记录 dropped sample 数，不扩容。
- 需要跨 tick 保留的数据由世界或对应 module 实例持有，不从全局静态池借出后长期占用。

## 4. 容器选择规则

容器根据访问模式选择，不把 `List`、`Dictionary` 或链表机械视为好坏：

| 访问模式 | 首选 | 原因 |
|---|---|---|
| runtime slot 升序扫描、需要连续内存 | 分页数组、连续数组、bitset | cache 友好、无枚举分配、顺序稳定 |
| 最低空闲 slot 获取 | 最小堆或分层位图 | 避免全表扫描；保持确定性最低槽优先 |
| 高频追加、按索引读取、报告期排序 | 预分配数组/List 或环形缓冲 | 连续布局优于链表 |
| 已持有节点句柄的 O(1) 插入/删除 | 侵入式双向链表或自定义节点池 | 不创建 LinkedListNode；适合生命周期队列 |
| 按 key 定位且容量可预估 | 预分配 Dictionary 或 generation-aware handle table | 查找快，但禁止运行中 rehash 扩容 |
| 碰撞候选临时结果 | 每世界/每攻击者复用 scratch 或固定池 | 同 tick 结束归还，保持候选顺序 |

普通 `LinkedList<T>` 每个节点本身是引用对象，节点分散会增加 GC 与 cache miss；不能仅因为插入删除频繁就使用。若确实需要链表语义，优先使用预分配节点数组，以整数索引保存 `prev/next`，既维持 O(1) 插入删除，又避免运行期节点分配。需要数据连续性和批量扫描时，swap-remove、稠密数组加稀疏索引或 tombstone/bitset 通常更合适。

## 5. static 治理规则

`static` 本身不会触发 GC，纯函数也不会因为是静态函数而变得不严谨。需要清理的是没有明确所有权和生命周期的可变静态状态。

保留范围：

- 编译期常量、只读表、无状态数学/解析算法、运算符。
- Editor 菜单入口、测试工具和不会进入 Player 热路径的无状态方法。

必须迁移为实例所有权或显式服务的范围：

- 当前世界、当前相机、当前渲染计划、当前 generation、当前战斗配置等可变状态。
- 会跨对局残留的缓存、注册表、诊断开关和测试 override。
- 依赖全局 `Instance` 才能工作的战斗规则或随机状态。

当前优先审计对象包括 `BattleCentralRenderSystem`、`NTSDRenderSpace`、`NTSDGlobal.MPEnabled`、`NTSDEntityRuntime` 的全局 mutation epoch、中央挂载注册表及战斗路径中的测试 override。迁移目标是由 `BattleSession`、`SimulationWorld`、`BattlePresentationCoordinator` 或明确的 Unity host 实例持有；纯常量与无状态 kernel 不做无收益实例化。

## 6. 禁止 `partial` 与模块迁移

从本计划开始，NTSD 新代码不得新增 `partial` 类型或新的 `.partial.cs` 文件。现存 `partial` 共有 4 个逻辑类型：

- `SimulationWorld`
- `LF2Character`
- `LF2OtherObject`
- `LF2WeaponBase`

迁移不采用把所有内容合并成一个巨型文件，而采用 main class 持有职责 module/subclass 引用：

```text
SimulationWorld
  -> SimulationRegistry
  -> SimulationPassPipeline
  -> SimulationEntityTraversal
  -> SimulationAiRuntime
  -> SimulationStageRuntime
  -> BattlePresentationCoordinator

LF2Character
  -> CharacterInputModule
  -> CharacterDamageStateResolver
  -> CharacterInteractionResolver
  -> CharacterLateOpointModule
```

迁移顺序固定为：先提取无状态或只依赖显式参数的 module，再转移 module 自有字段，最后删除原 partial 声明。每一步都要保留主入口、pass 顺序和 checksum；不能在一个提交中同时改所有权、战斗规则与容器实现。

## 7. 执行批次

1. **Z0 门禁与诊断去污染**：固定采样容量，增加可信的 `GC.Alloc`/collection 长窗口报告，停止使用失真的单一计数作为 PASS。
2. **Z1 registry/sort**：清除 bucket 临时 List、LINQ `ToList` 和排序委托分配。
3. **Z2 pass traversal**：引入 `SimulationEntityTraversal` 实例和 struct cursor，逐个替换热路径 lambda/闭包。
4. **Z3 event path**：复用 hit resolver，增加落地强类型入口，清除 itr copy、候选去重和 VRest 的剩余条件分配。
5. **Z4 pool contract**：统计 Loading 阶段所需对象、任务、候选、音频与表现容量；正式战斗禁止自动扩容。
6. **Z5 static ownership**：先迁移可变跨对局状态，保留真正无状态的算法与常量。
7. **Z6 partial migration**：按 Registry -> PassPipeline -> AI -> Stage -> Entity subclasses 顺序拆成实例 module；每批单独 parity。
8. **Z7 最终验收**：Dispersed1000、Combat1000 连续 60 秒零分配、无 collection、30 Hz、checksum 一致、cleanup 完整，再运行 compile、focused tests、完整 EditMode 与 `BattleRuntimeSelfCheck`。

## 8. 当前状态

- 分配调用栈：已定位到“正式逻辑 tick”和“整个 Unity PlayerLoop”两个口径；两者不得混用。
- `p135-live-full-frame-gc-20260810`：1000 个正式 AI、非 simulation-only、声音 dispatch、30 个采样 tick；正式逻辑 tick 为 `0 B`，但 58 个完整 Profiler 帧平均仍为 `146,588.97 B/frame`、最大 `1,683,762 B/frame`。因此只能说明逻辑 tick 门禁通过，不能声明战斗零 GC。
- 已完成的热路径治理：slot/实体无分配遍历、battle buffer/runtime capacity 模块、VRest 连续存储、碰撞候选容量准备、AI 空间与快照容量准备、表现双帧/命令/排序容量准备、任务环形缓冲、正式声音目录与 AudioSource 预热。
- 本批新增：stage 正 ratio 波次的 `int[40]` 改为战前按 campaign 最大条目数准备的 `StageSpawnRuntimeBufferPool`；战斗封印后容量不足直接失败，不再静默分配。
- 本批新增：删除 `BattleSpriteMaterialContract` 中跨场景静态 `Dictionary<int, class>` 延迟缓存；材质合同判定改为无状态纯函数，避免首次遇到 Material 时创建 class 和扩容字典。
- `partial`：`LF2WeaponBase` 已完成普通类入口与 resolver/module 委托迁移；当前仍有 `SimulationWorld`、`LF2Character`、`LF2OtherObject` 3 个逻辑类型使用 `partial`。禁止再新增，剩余类型仍需按模块边界迁移。
- static：`MPEnabled`、pending-destroy mutation tracker、世界 self-check hooks、runtime character config resolver、材质分类缓存已经迁移或去状态化；表现会话、挂载注册表和 render-space 场景状态仍待迁移。
- 1000 AI 普通稳态门禁：`NTSD_ProductionEntityStress.dispersed1000-p147-prebattle-memory-boundary-20260810.json` 的 30 tick 预热 + 300 tick 正式窗口中，正式 tick 为 `0 B`、allocation violation 为 0、Gen0/1/2 collection 增量均为 0，cleanup 后活动对象、world entity、slot 和 active pool 均为 0。该证据仍不覆盖技能、命中、opoint、死亡复活和池耗尽等事件路径。
- T8 默认 `stage.dat` 与 Android 真机：继续排除。

## 9. static 所有权审计清单

下表按“是否有可变跨对局状态”分类，而不是按“是否写了 static”机械分类：

| 对象 | 分类 | 处理决定 | 当前状态 |
|---|---|---|---|
| `AiDecisionKernel`、`AiSensingKernel`、常量/运算符/ProfilerMarker 表 | 无状态算法或只读元数据 | 保留；实例化不会减少 GC，也会制造无意义所有权 | 保留 |
| `BattleSpriteMaterialContract` | 原本是纯合同，但混入可变静态 class 缓存 | 删除缓存，恢复无状态判定 | 已处理 |
| `LF2Entity.RuntimeCharacterConfigResolverOverride` | 可变静态测试/配置替身，会跨世界污染 | 已改为 `SimulationWorld` 持有的 `RuntimeCharacterConfigResolver`；脱离世界的自检壳显式注入 resolver | 已处理；编译 0 error、聚焦测试 6/6、self-check PASS |
| `SimulationWorld.RespawnEffectSpawnOverride`、`CharacterInputPassMutationOverrideForSelfCheck` | 可变静态测试 hook | 已迁移到每个 `SimulationWorld` 的 `SimulationWorldHooks` 实例 | 已处理 |
| `BattleCentralRenderSystem` 的 world、generation、material、pending frame | 可变全局表现会话状态 | 由明确的 `BattleRenderHost`/会话实例持有；URP feature 只保留桥接入口 | 待分批迁移 |
| `BattleCentralPresentationMountRegistry` | 可变全局挂载与 owner 注册表 | 归属当前表现会话；容量在 Loading 阶段准备 | 待迁移 |
| `NTSDRenderSpace` 的 camera/boundary/cache/测试 override | 可变场景状态 | 迁移到 scene render context；纯像素换算可保留为无状态函数 | 待迁移 |
| `NTSDGlobal.MPEnabled` | 可变当前对局规则 | 已迁移到 `BattleMatchRuntimeState`/`SimulationWorld.PpMode` | 已处理 |
| `NTSDEntityRuntime.pendingFlushDestroyMutationEpoch` | 跨世界可变诊断 epoch | 已改为每个世界的 `SimulationWorldMutationTracker`，runtime 只保留对所属 tracker 的引用 | 已处理 |
| `LF2Entity` detached fallback RNG | 原为 `static readonly` 引用，但 RNG 对象本身可变，脱离世界的对象会共享序列 | 改为每个实体预构造并持有；正式已注册实体仍只消费所属世界 RNG | 已处理；self-check 覆盖两个 detached entity 的独立序列 |
| `AudioController.Instance`、`SingletonBehaviour<T>.Instance` | Unity host 定位入口 | 不作为战斗逻辑真相；逐步改为 bootstrap 注入，host 生命周期内可保留桥接 | 待审计 |

## 10. 容量准备与池使用清单

| 数据/对象 | 战前来源 | 战斗期规则 |
|---|---|---|
| 角色、武器、opoint GameObject | roster、DAT、压力测试上限 | 只从 `LF2ObjectPool` 获取；耗尽不得 Instantiate |
| 纯 C# entity/task/resolver | runtime slot 上限、DAT 最大同时对象数 | 从 `LF2ReferencePool` 或持有模块复用；Release 必须完整重置 |
| 声音 cue/clip/AudioSource/follower | 全部已加载 DAT sound 字段 + 内建 cue 目录 | 只查预热目录、只租预建 voice；未知 cue 计数并拒绝，禁止异步加载和 coroutine |
| stage 正 ratio slot 数组 | 已加载 campaign 所有 phase 的最大 spawn 条目数 | 波次切换归还复用；每条固定 40 slot，与权威规则一致 |
| 碰撞 participant/pair/candidate | runtime slot 上限与最坏 pair 数 | 使用预分配 List/数组/树；禁止扩容、LINQ、临时 class |
| AI snapshot/spatial/team partition | runtime slot 上限；team 优化池固定 2 个 | 双缓冲/数组复用；第三队及以上走已有 fallback，不创建新 partition |
| 表现 frame/entity/hit/command/sort | runtime slot 与每实体 overlay/hit 上限 | 双帧复用、原地写入；禁止逐帧创建数组/比较器/闭包 |
| 诊断样本 | 固定采样长度 | 固定数组/环形缓冲；溢出丢样并记数，结束采样后才序列化 |

Unity `ListPool<T>` 不是跨 tick 状态容器：只有能在同一作用域严格 `Get/Release`，且容量不会在战斗中首次增长时才允许使用。普通 `LinkedList<T>` 也不是默认优化方案；它的 `LinkedListNode<T>` 是独立 class。需要稳定 O(1) 删除时，优先使用预分配节点数组和整数 `prev/next` 的侵入式索引链表。

## 11. 当前验证批次

1. Unity 刷新后脚本编译：0 个 C# error。
2. `dotnet build Assembly-CSharp.csproj --no-restore`：成功。
3. 完整 `BattleRuntimeSelfCheck`：本批修改后 PASS。
4. `p147-prebattle-memory-boundary`：1000 AI 普通稳态 30 tick 预热 + 300 tick 正式窗口，正式 tick `0 B`，allocation violation 0，Gen0/1/2 collection 增量 0，cleanup 完整。
5. 当前缺口：尚未用固定、可复现的高交互脚本覆盖命中、技能、opoint、投掷、落地、销毁/复用、死亡复活、声音饱和与池容量边界；因此尚不能声明“整个战斗过程零 GC”。
6. 最终完成条件仍是正式 1000 AI 的普通与高交互场景在预热后 `0 B/frame`、collection 计数不增加、30 Hz、checksum/parity 不变。

## 12. 逐项执行与验收矩阵

所有项目都按“先测量、再修改、再运行同口径回归”的方式处理；不把静态代码扫描本身当作零 GC 证据。

| 顺序 | 范围 | 主要风险 | 处理方式 | 完成证据 |
|---:|---|---|---|---|
| 1 | 战斗内存边界 | 加载垃圾进入战斗、Player 中途 collection | Loading/Prepare 完成后预收集；Player 正式窗口禁用 managed collection；每 tick 记录 allocation/collection 违规 | 普通 1000 AI 已通过；事件路径待补 |
| 2 | 高频稳态 tick | lambda、LINQ、快照、排序、容器扩容 | 世界持有 cursor/scratch/预分配数组；战斗封印后 fail closed | p147 普通稳态通过 |
| 3 | 事件 tick | hit/opoint/落地/投掷/死亡产生临时 resolver、数组或 task | resolver 持久化；task/entity/GameObject 全部从池取得；强类型事件参数，禁止装箱 | 待 Combat1000 逐类验证 |
| 4 | 表现与挂载 | 首次 sprite/material/handle、注册表扩容、逐帧 handle 查找 | 战前资源/命令/handle 容量准备；复用 per-frame handle cache；挂载表战斗封印 | 普通稳态已部分验证；会话所有权待迁移 |
| 5 | 声音 | 首次 cue、AudioSource、follow component、coroutine | 完整 cue catalog 与 voice pool 战前预热；战斗期未知 cue/池耗尽只记计数并拒绝 | 待声音饱和事件验证 |
| 6 | stage 与对象生命周期 | ratio buffer、spawn task、对象池耗尽、复用字段残留 | stage buffer/task/reference/GameObject 池化；封印后禁止扩容；Release 全字段重置 | 聚焦自检已覆盖部分；高交互长窗待补 |
| 7 | static 所有权 | 跨世界状态污染与无法释放的引用 | 只迁移可变会话状态；常量、只读表和无状态纯函数保留 static | 逐项进行中 |
| 8 | `partial` | 隐式共享字段、职责边界不清、难以独立预热和验证 | main class 持有普通 module/subclass；先抽无状态逻辑，再搬自有字段，最后移除旧声明 | `LF2WeaponBase` 已完成；余 3 类 |
| 9 | 容器 | rehash/扩容、`LinkedListNode` 分配、cache miss | 连续扫描用数组/List；key 查找用预分配 Dictionary；O(1) 删除且有稳定句柄时才用预分配索引链表 | 随模块逐项审计 |
| 10 | 最终门禁 | 短测漏掉偶发路径 | Dispersed1000 + Combat1000 各连续 60 秒；Profiler `GC.Alloc`、collection、堆曲线、30 Hz、checksum 和 cleanup 共同判定 | 待完成 |

Profiler 原始采样 `Temp/NTSD_1000AI_p126-gc-path.raw` 仅用于离线 Profiler 回放，大小 10,729,095,356 字节（约 10.06 GiB）；不参与项目编译或运行，可以在不再需要回看该样本后删除。删除仍由用户明确授权后执行。
