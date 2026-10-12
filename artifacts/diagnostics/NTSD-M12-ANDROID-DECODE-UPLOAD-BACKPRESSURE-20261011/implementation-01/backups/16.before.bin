# Battle Runtime Ordered Shutdown Contract

> **NTSD24_AUTHORITY_SUPERSEDED（2026-09-02）：** 本文包含 NTSD 2.4、旧 `ntsd_new.exe`/`game_tick(...)`、固定 30 Hz 或 Authority400 等旧权威假设，仅作为历史证据；不得据此定义当前战斗规则、pass、timing、slot、RNG、字段、生命周期、表现或“已对齐”状态。任何恢复先读 `docs/ai/CURRENT-AUTHORITY.md`；当前权威是 NTSD 2.8-Logan 正式 EXE 及其对应 playable 源码，旧结论一律 `REBASELINE_REQUIRED`。

> 状态：长期架构契约（当前实现需要按独立 Change Record 分阶段对齐）
>
> 生效日期：2026-09-01
>
> 适用范围：Unity NTSD battle runtime、输入、worker、OPoint、structural writer、presentation、对象池、地图 runtime carrier 及未来新增的战斗模块
>
> 用户决议：战斗关闭必须是显式、有序、幂等的 Runtime 事务；不得继续依赖 Unity `OnDestroy()` 的非确定销毁顺序逐点补漏

## 1. 目的

Unity 不保证不同 GameObject、Component、父子节点或 Scene root 之间的 `OnDisable()` / `OnDestroy()` 业务依赖顺序。战斗系统不得假设某个 manager、factory、pool、world 或 renderer 一定比另一个对象晚销毁。

本契约建立一条唯一的战斗 Runtime 关闭序列，使正常卸载、退出 Play Mode、应用退出、初始化失败、测试 teardown 和异常 Scene 销毁共享相同的状态机与依赖边界。

目标不是规定 Unity 自己按什么顺序销毁 GameObject，而是保证：

1. 在允许 Unity 开始销毁 Scene 对象之前，战斗业务已经主动停止并释放跨模块引用。
2. `OnDestroy()` 只作为幂等、非创建、顺序无关的 fallback，不承担主要关闭编排。
3. 进入 `Stopping` 后，任何模块都不得创建新的战斗对象、任务、单例、publication 或 worker work item。
4. 新模块必须声明自己的关闭阶段，不能自行绕过或重排本契约。

## 2. Authority 与边界

本契约属于 Unity-native 生命周期适配，规定停止、资源归还和 Scene teardown 的所有权。它不得改变：

- C++ release live battle pass 顺序；
- 固定 30 Hz；
- Running 状态下的输入消费、OPoint 生成、碰撞、命中或对象生命周期规则；
- 同 seed、同输入、同 tick 的 battle checksum；
- RuntimeEntityHandle、slot generation、HP/PP 或其他战斗真值；
- 中央渲染在 Running 状态下的 command/materialization/合批语义。

如果实现本契约需要改变 Running 状态的可观察战斗结果，必须停止并按 C++ authority 另建行为对齐任务；不得以“关闭流程重构”为名修改战斗规则。

## 3. 状态机

战斗 Runtime 必须具有独立于普通 Pause 的生命周期状态：

```text
Uninitialized
    ↓
Preparing
    ↓
Running
    ↓
Stopping
    ↓
Stopped
```

允许的状态转换：

| 当前状态 | 允许的下一状态 | 说明 |
|---|---|---|
| `Uninitialized` | `Preparing` | 开始创建/绑定本场战斗所需服务。 |
| `Preparing` | `Running` | 所有启动 gate 通过并完成 publication/capacity 准备。 |
| `Preparing` | `Stopping` | 初始化失败或用户取消；仍必须走有序关闭。 |
| `Running` | `Stopping` | 正常退出、Scene unload、应用退出或测试 teardown。 |
| `Stopping` | `Stopped` | 本契约所有 hard gate 与清理后置条件完成。 |
| `Stopped` | `Preparing` | 新一场战斗显式重新初始化。 |

禁止：

- `Stopping → Running`；
- `Stopped → Running` 跳过 `Preparing`；
- 把 `paused=true` 当作 `Stopping`；
- 因单个清理步骤失败而恢复接收 tick/input/spawn。

普通暂停与生命周期必须分离：

```text
Battle paused:    Lifecycle=Running,  Paused=true
Battle stopping:  Lifecycle=Stopping, Paused=true
Battle stopped:   Lifecycle=Stopped,  Paused=true
```

## 4. 唯一关闭序列

以下顺序是强制依赖序列，不是建议列表：

```text
Running
  ↓
Stopping
  ├─ 1. 禁止新的 tick 和输入
  ├─ 2. 停止并 Join dedicated worker
  ├─ 3. 关闭 OPoint / structural spawn 入口
  ├─ 4. 解除 allocation seal
  ├─ 5. 清空 presentation publication / central submission
  ├─ 6. 丢弃并回收尚未执行的 OPoint 任务
  ├─ 7. 回收角色/武器/特效 Renderer
  ├─ 8. 清理 World 中剩余的 logic-only entity 和注册表
  ├─ 9. 解绑并释放 SimulationWorld
  ├─ 10. 将 LF2ObjectPool 置为 quiesced 状态
  ├─ 11. 清理地图 runtime boundary carrier
  ↓
Stopped
  ↓
允许 Unity 销毁 Scene GameObject
```

依赖边必须始终保持：

```text
tick/input closed
    → worker joined
        → spawn intake closed
            → allocation unsealed
                → presentation released
                    → pending tasks discarded/recycled
                        → renderers returned
                            → logic-only world state cleared
                                → world unbound
                                    → pool quiesced
                                        → map carriers cleared
                                            → Stopped
                                                → Unity destroy allowed
```

任何重排必须先更新本契约、给出依赖证明、测试失败预期与用户批准；不得在普通实现任务中局部调整。

## 5. 阶段 1：禁止新的 tick 和输入

### 5.1 必须关闭的入口

进入 `Stopping` 必须发生在任何清理操作之前。随后立即阻止：

- `SimulationTickDriver.Update()` 自动 tick；
- manual/diagnostic `StepOneTick(...)`，包括 `ignorePaused=true`；
- lockstep/manual replay 继续推进；
- dedicated worker 新提交；
- 本地输入边沿捕获；
- function-key command latch；
- 新的 frame input packet 消费；
- LateUpdate 创建新的 presentation work。

`ignorePaused` 只能绕过普通暂停，不能绕过 `Stopping/Stopped`。

### 5.2 完成条件

- Lifecycle 已单向写为 `Stopping`；
- `paused=true`；
- 所有 tick API 对新请求 fail closed；
- input latch/provider 不再产生未来 tick 输入；
- 当前逻辑 tick 不再增加。

## 6. 阶段 2：停止并 Join dedicated worker

必须停止 worker、等待执行线程退出，并清理：

- submitted provider；
- tick-in-flight；
- presentation acknowledgement；
- input/output queue；
- 预分配 frame input 中的本轮引用；
- worker publication ownership。

### 6.1 Hard gate

worker 没有确认停止时，禁止进入会修改或释放 `SimulationWorld`、factory、pool 或 presentation 的后续阶段。否则可能发生 worker 与主线程并发访问已释放对象。

如果 Join 超时或 worker fault 无法确认终止：

1. Runtime 保持 `Stopping`；
2. 禁止 Scene 正常 unload gate 报告完成；
3. 记录 first-failure diagnostics；
4. 不恢复 tick/input；
5. 不在 worker 仍可能访问 World 时强行清空 World。

应用进程被外部强制终止属于不可控边界，但 fallback 仍不得创建任何新服务。

## 7. 阶段 3：关闭 OPoint / structural spawn 入口

停止阶段仍需要执行 `Unregister`、`Free`、`Destroy` 等退出写操作，因此不能简单禁用整个 structural writer。

操作权限必须区分：

| Structural 操作 | `Stopping` 时 |
|---|---:|
| Spawn / SpawnMultiple | 拒绝 |
| OPoint enqueue | 拒绝并归还 task |
| Register 新实体 | 拒绝 |
| Unregister | 允许 |
| Free | 允许 |
| Destroy | 允许 |
| Generation release | 允许 |

原则是：

```text
禁止新的生命进入系统；允许现有对象按清理路径离开系统。
```

### 7.1 完成条件

- Unity factory 与 logic-only factory 均不再接受任务；
- structural writer 不再 materialize 新实体；
- 被拒绝的 task 有明确 owner 回收，不泄漏到引用池外；
- teardown 所需 unregister/free/destroy 仍可工作。

## 8. 阶段 4：解除 allocation seal

allocation gate 只能解除本场战斗实际 seal 的实例。推荐由 gate 在 Prepare 时捕获准确依赖，Unseal 时使用捕获引用，而不是在 teardown 重新使用全局 service locator。

停止路径禁止调用会自动创建对象的 `.Instance`。只能使用：

- 预先捕获并由当前战斗拥有的引用；
- `TryGetInstance()` / `Current` 等非创建查询；
- `SimulationWorld` 自己拥有的 pool/runtime。

如果依赖已被 Unity 销毁，Unseal 跳过该对象；不得创建替代实例只为了执行一次 Unseal。

### 8.1 完成条件

- dedicated worker 已停止；
- managed-memory battle window 已关闭；
- world/runtime/factory/pool 的 capacity seal 已按现存 owner 解除；
- gate 自己的 captured dependency 在完成后清空；
- 重复 Unseal 为幂等 no-op。

## 9. 阶段 5：清空 presentation publication / central submission

必须先释放表现层对逻辑对象和资源的借用，再回收 Renderer/Entity。

需要覆盖：

- immutable `BattlePresentationFrame` publication；
- hit-record publication cycle；
- central actor/health mesh submission 与 lease；
- renderer/catalog binding；
- published sound events；
- legacy overlay/spark borrower；
- 任何保存 `RuntimeEntityHandle` 或实体引用的 presentation cache。

此阶段只清当前战斗 publication/submission，不得借机改变 Running 状态的 atlas、shader、合批或战斗规则。

### 9.1 完成条件

- 不存在可被下一帧 RenderPass 消费的旧 submission；
- central/legacy presentation 不再持有待释放实体；
- LateUpdate 在 `Stopping/Stopped` 不再重新发布；
- presentation reset 可重复调用。

## 10. 阶段 6：丢弃并回收尚未执行的 OPoint 任务

Shutdown 禁止调用 `FlushTasks()` 来执行 pending OPoint。Flush 会在停止阶段生成新角色、武器或特效，违反阶段 3。

正确策略：

1. 停止接收新任务；
2. 逐个 dequeue 尚未执行的 task；
3. 不调用 Materialize/Spawn；
4. 按原 owner 将 task 归还 `BattleLogicReferencePool`；
5. 清空 Unity factory 和 logic-only factory 两套队列；
6.记录 discarded/recycled 数字诊断。

### 10.1 完成条件

- pending task count 为 0；
- reference-pool task 租约恢复；
- entity/spawn count 不因 shutdown drain 增加；
- post-stop enqueue 被拒绝且立即回收；
- 无临时列表或 delegate 分配进入 teardown 热点。

## 11. 阶段 7：回收角色、武器、特效 Renderer

Renderer 必须在其关联 `SimulationWorld` 仍存在、structural cleanup 仍允许时归还对象池。

`LF2ObjectRenderer.ResetState()` 可能需要：

- 清 Sprite/shadow；
- 释放 catalog binding；
- 从 World unregister；
- reset logic object；
- 清 owner mount/runtime binding；
- 将 GameObject 设为 inactive。

因此不得把 World 引用在本阶段之前完全置空。

遍历 active pool objects 时必须使用 pool 预分配 scratch 或安全的两阶段遍历，不能一边枚举 `HashSet` 一边修改，也不能在 shutdown 时创建新的数组/列表。

### 11.1 完成条件

- pool active borrower count 为 0；
- 所有 pooled GameObject inactive；
- renderer sprite/shadow/catalog/owner binding 已清；
- entity 不再持有 Renderer；
- 回收过程中没有创建替代 pool/factory。

## 12. 阶段 8：清理 logic-only entity 和 World 注册表

Renderer 回收后，World 仍可能持有：

- dedicated worker 使用过的 logic-only entity；
- 没有 Renderer 的服务器/测试实体；
- pending unregister/destroy；
- runtime slot/generation；
- identity/relation/vital stores；
- collision broadphase；
- stage runtime buffer；
- mutation tracker binding。

World 必须提供专用 shutdown reset，而不是把普通 match reset 机械当作 teardown。前置条件：worker stopped、`_ticking=false`、spawn intake closed。

### 12.1 完成条件

- World object count 为 0；
- claimed runtime slot 为 0；
- pending unregister/destroy 为 0；
- logic reference leases 全部归还；
- 各 SoA/store/broadphase 不再引用实体；
- 此时 Driver 仍保留局部 World 引用用于验证后置条件。

## 13. 阶段 9：解绑并释放 SimulationWorld

只有阶段 8 完成后，才允许：

- Driver `_world = null`；
- local input provider unbind；
- battle tick system release；
- checksum/snapshot/published sound 临时状态清理；
- World owner reference release。

如果阶段 8 的 hard postcondition 未满足，不能把 `_world` 直接设为 null 来掩盖残留。

### 13.1 完成条件

- Driver 不再暴露可推进的 World；
- input/lockstep/presentation 均无法通过 Driver 重新访问旧 World；
- 重复 unbind 为幂等 no-op。

## 14. 阶段 10：将 LF2ObjectPool 置为 quiesced

`quiesced` 的含义不是无条件 `Destroy()` 所有预热对象，而是：

- active borrower 为 0；
- 所有对象 inactive；
- 不接受新的 Get/GetSprite；
- 无到旧 World/Entity/Renderer 的绑定；
- pool Update 不再执行过期扩容/回收工作；
- 可以安全等待 Scene 自然销毁，或在下一次 `Preparing` 显式重新启用。

如果 pool 是 Scene-owned，Unity 在后续 Scene unload 中销毁它；如果未来 pool 变成跨 Scene 持久服务，则必须从 quiesced 显式进入 preparing，不得隐式恢复工作。

## 15. 阶段 11：清理地图 runtime boundary carrier

地图配置属于 App/Bootstrap 场景所有权，必须在 simulation/world/pool 停止后清理：

- `BattleBootstrap.ClearPreparedMapConfiguration()`；
- `BoundaryWallManager.ClearLoadedBoundaryDefinition()`；
- 销毁 `__BoundaryAssetRuntime_*` transient carrier；
- 恢复/解绑背景表现引用；
- 清 loaded boundary cache。

Simulation 层不得反向依赖 `BattleBootstrap`。正常编排 owner 应是 App/Scene lifecycle coordinator：先关闭 Driver runtime，再关闭 Bootstrap map presentation。

### 15.1 完成条件

- loaded boundary definition 已清；
- `__BoundaryAssetRuntime_*` 数量为 0；
- Scene authoring `BoundaryWallEditor` 未被误删或修改；
- 没有 transient carrier 被保存进 Scene。

## 16. `Stopped` 与 Unity Destroy gate

只有所有 hard gate 通过后才能写入 `Stopped`。随后才允许正常流程调用：

- `SceneManager.UnloadSceneAsync`；
- 切换回菜单；
- 销毁 Scene-owned battle GameObject；
- 开始下一场战斗的 `Preparing`。

进入 `Stopped` 的最低后置条件：

```text
no tick/input
no running worker
no accepted spawn
no pending OPoint task
no active renderer borrower
no World object/claimed slot
no published presentation submission
no loaded runtime boundary carrier
no teardown-created singleton
```

## 17. 正常 owner 与 fallback owner

### 17.1 正常路径

推荐所有权：

| 层 | 职责 |
|---|---|
| `AppManager` | 将 App flow 标记为 BattleStopping，发起关闭，Stopped 后卸载 Scene。 |
| `SimulationTickDriver` | 执行阶段 1～10 的 simulation/runtime transaction。 |
| `BattleBootstrap` | 在 Driver 停止后执行阶段 11 的地图/背景/相机清理。 |
| Editor bridge | `ExitingPlayMode` 时在 Unity 销毁 Scene 对象前发起同一关闭入口。 |

### 17.2 `OnDestroy()` fallback

`OnDestroy()` 只允许：

- 调用同一个幂等 shutdown 入口；
- 检查现有引用；
- 执行非创建、顺序无关的最后清理；
- 记录数字 diagnostics。

`OnDestroy()` 禁止：

- 访问会创建 GameObject 的 singleton `.Instance`；
- 开始 worker/tick/async load；
- Flush pending spawn；
- 创建替代 factory/pool/manager；
- 假设其他组件仍存在；
- 将 `Stopping` 恢复为 `Running`。

## 18. 失败与重入策略

Shutdown 必须幂等并具备重入保护：

| 调用时状态 | 行为 |
|---|---|
| Running/Preparing | 原子进入 Stopping，由当前调用者拥有 transaction。 |
| Stopping | 返回 AlreadyStopping；不得并行执行第二份清理。 |
| Stopped | 返回 AlreadyStopped；不得重新访问已清引用。 |

错误处理原则：

1. 首个失败必须记录阶段、错误代码和关键计数；
2. 生命周期保持 `Stopping`，禁止恢复 gameplay；
3. worker 未停止属于 hard failure，不能并发清 World；
4.后续可安全、无依赖的清理可 best-effort 继续，但不得谎报 `Stopped`；
5. 正常 Scene unload gate 只在 hard postconditions 通过后开放；
6. 应用被外部强制终止时仍不得在 fallback 创建对象。

## 19. 新模块接入规则

任何新增 battle runtime 模块、manager、cache、queue、worker、renderer、pool 或 transient Scene object，必须在设计/Change Record 中回答：

1. 它在哪个生命周期状态创建？
2. 它在上述 1～11 哪个阶段停止接受新工作？
3. 它依赖哪些更早阶段完成？
4. 它持有哪些下游引用或借用？
5. 它如何 drain/discard/recycle pending work？
6. 它的 shutdown 是否幂等？
7. 它是否会调用创建型 singleton `.Instance`？如果会，为什么不在 Preparing 完成？
8. 它的完成后置条件是什么？
9. 哪个 focused test 证明它没有残留和 teardown allocation？
10. 它是否需要在 AGENTS 主序列中新增阶段？

新增模块默认插入既有阶段内部，不得随意增加顶层阶段。确需改变顶层顺序时，必须：

- 先更新本契约；
- 明确旧顺序为何不再安全；
- 列出所有依赖边；
- 提供 test-first 失败；
- 获得用户明确批准；
- 同步更新根 `AGENTS.md`。

未声明关闭阶段的模块不得接入正式战斗 Runtime。

## 20. 禁止通过 Script Execution Order 解决

不得把 Unity Script Execution Order 当作跨 GameObject teardown dependency manager。它不能可靠替代显式关闭事务，也不能覆盖 Scene unload、Domain Reload、测试 teardown 和异常对象销毁。

允许 Script Execution Order 处理正常帧内回调先后；不允许用它证明本契约的 shutdown 顺序。

## 21. 验收矩阵

实现或扩展本契约时，至少验证：

### 21.1 静态/编译

- Unity script compile 0 error；
- teardown 路径不得出现创建型 singleton `.Instance`；
- 没有新增 Scene 序列化依赖；
- Change Record 覆盖全部脚本路径。

### 21.2 Focused tests

- 生命周期合法/非法转换；
- repeated shutdown idempotence；
- Stopping 后 automatic/manual/ignorePaused tick 全拒绝；
- worker in-flight stop/join；
- post-stop OPoint/structural spawn 拒绝；
- pending task discard/recycle，不 materialize；
- presentation submission 清零；
- active renderer/pool borrower 清零；
- logic-only entity、World object、claimed slot 清零；
- boundary carrier 清零；
- factory/pool/manager 不被 teardown 自动创建；
- Running 状态的同 tick checksum 不变。

### 21.3 真实运行时

至少执行：

```text
enter Play
→ 等待战斗完整初始化和角色延迟生成
→ exit Play
→ 检查 Console/对象计数
→ 再次 enter Play
→ 验证角色、输入、血条、地图、中央渲染正常
→ 再次 exit Play
→ 再次检查零残留
```

必须确认：

- `Some objects were not cleaned up when closing the scene` 为 0；
- `*_AutoCreated` teardown residual 为 0；
- `__BoundaryAssetRuntime_*` 为 0；
- active pool objects 为 0；
- World objects/slots 为 0；
- 第二次进入战斗可正常重新 Preparing/Running；
- Scene dirty 状态不因验证改变。

## 22. 当前实现状态说明

本文定义目标契约，不声明当前所有模块已经完成接入。

截至本文建立时，已确认并修复一项局部 teardown 安全问题：allocation unseal 使用非创建查询，避免关闭 Scene 时重新生成 `LF2ObjectPointFactory_AutoCreated` 和 `LF2ObjectPool_AutoCreated`。这只证明相应漏洞已关闭，不等于上述完整有序 Shutdown 已实现。

完整实现必须使用独立 Change ID（建议 `BATTLE-RUNTIME-ORDERED-SHUTDOWN-001`），按状态机、worker、spawn gate、queue drain、renderer/world/pool cleanup、App/Bootstrap wiring 和真实 Play 验收分阶段推进。在实现与验证完成前，不得把本文状态写成“Runtime ordered shutdown 已完成”。

## 23. 维护要求

- 根 `AGENTS.md` 保存不可违反的主顺序和接入门禁；
- 本文保存详细阶段契约、失败策略和验收矩阵；
- 具体实现进度、测试 job、失败和回滚记录写入对应 Change Record/Task，不写入本契约；
- 当实现代码与本文不一致时，必须标明是代码缺口还是经批准的新契约，不能静默漂移；
- 文档修改不能替代编译、focused test、SelfCheck 或 Play Mode 证据。
