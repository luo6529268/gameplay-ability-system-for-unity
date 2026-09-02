# Simulation Mono / 非 Mono 边界整理与重构计划

> 计划标识：`SIMULATION-MONO-BOUNDARY-REFACTOR-001`
>
> 创建日期：2026-09-02
>
> 当前状态：`DOCUMENTED / IMPLEMENTATION_NOT_STARTED / USER_HOLD`
>
> 性质：架构方案与未来实施合同。本文件创建时不授权修改 C#、Scene、Prefab、
> asmdef、资源、ProjectSettings、战斗规则或运行行为。

## 1. 决策摘要

本计划用于在后续独立任务中，把 Unity/Mono 生命周期适配与确定性战斗模拟明确分层。
当前先冻结目标、边界、依赖方向、迁移批次和验收标准，不立即执行代码重构。

最终目标不是机械地把所有 `MonoBehaviour` 移到一个目录，也不是为了“纯 C#”替换所有
Unity value type，而是建立可由代码和程序集共同验证的所有权边界：

```text
Unity / Mono Host
    负责 Scene、生命周期、输入采集、主线程、资源和对象池
                    |
                    | 纯值输入 / 显式 command / lifecycle call
                    v
Deterministic Simulation Core
    负责 tick、entity state、pass、RNG、slot/generation、checksum
                    |
                    | immutable publication / presentation commands
                    v
Unity Presentation Adapter
    负责 Renderer、Sprite、Material、GameObject、Transform 和 Central Render
```

长期强制方向：

- Mono Host 可以调用 Core 的显式入口。
- Core 不得解析或调用 Mono singleton、GameObject、Transform、Renderer 或对象池。
- Core 只能发布纯值结果和命令，不直接完成 Unity 表现副作用。
- Presentation 只能消费 Core publication，并通过受限 acknowledgment/lease 返回资源状态；
  不得反写逻辑真值。
- 跨层事务由明确 lifecycle owner 编排，不依赖 Unity 跨 GameObject 的销毁顺序。

## 2. 术语与边界等级

### 2.1 Mono 层

满足任一条件即属于 Mono/Unity Host 或 Presentation 层：

- 继承 `MonoBehaviour`、`SingletonBehaviour<T>` 或其他 Unity component 基类。
- 使用 `Awake/OnEnable/Start/Update/LateUpdate/OnDisable/OnDestroy`。
- 创建、查找、销毁或持有 `GameObject`、`Component`、`Transform`。
- 访问 Scene、Camera、Renderer、Sprite、Material、Texture、Unity 对象池或资源加载器。
- 依赖主线程才能安全执行。

### 2.2 非 Mono 层

非 Mono 类型是普通 C# 类型，不继承 Unity component，也不由 Unity 生命周期直接拥有。
但“非 Mono”不自动等于“完全无 Unity 依赖”。例如普通 C# 类仍可能使用 `Vector3`、
`Mathf` 或 `Debug`。

### 2.3 两阶段纯化目标

本计划把边界分为两个等级，禁止混为一谈：

| 等级 | 强制要求 | 当前计划优先级 |
|---|---|---|
| L1：Mono 生命周期分离 | Core 不继承 Mono，不访问 GameObject/Transform/Renderer/Mono singleton | 必须完成 |
| L2：Unity assembly 独立 | Core 不引用 `UnityEngine`，坐标、数学、日志、profiling 全部使用纯值抽象 | L1 稳定后再评估 |

初次实施只强制 L1。L2 不得为了形式纯化而一次性替换全部坐标类型、制造大规模行为风险。

## 3. 权威与不可变行为

本计划只改变 Unity 代码组织和依赖方向，不定义新战斗规则。

实施时必须保持：

- C++ release live runtime 的战斗 pass 顺序和可观察结果。
- 固定逻辑频率 30 Hz。
- 输入边沿、组合键、frame input 消费时点。
- entity slot、stable id、generation、active/dormant 和销毁时点。
- OPoint 入队、flush、materialize 和 first-visible tick。
- RNG seed、调用次数和调用顺序。
- collision、hit、HP/PP、respawn、weapon、stage 等规则结果。
- snapshot、checksum、parity trace 和 replay 结果。
- `Running → Stopping → Stopped` 十一阶段有序关闭合同。
- Central Render、legacy presentation 和 Editor preview 的表现结果。

禁止借本计划：

- 修复角色招式或修改 DAT。
- 重排战斗 pass。
- 改变 30 Hz 或输入延迟语义。
- 切换 ECS/worker 默认路径。
- 新增网络、rollback、transport、database 或 Server 行为。
- 同时进行渲染性能重构、资源格式重构或 UI 改版。

## 4. 当前观察事实

### 4.1 已有正确基础

以下核心类型已经是普通 C# 类型：

| 类型 | 当前路径 | 当前判断 |
|---|---|---|
| `SimulationWorld` | `Simulation/Core/SimulationWorld.cs` | 非 Mono 聚合根 |
| `NTSDBattleTickSystem` | `Simulation/Core/NTSDBattleTickSystem.cs` | 非 Mono tick 编排 |
| AI modules | `Simulation/Ai/*` | 普通 C# 类型 |
| ECS stores/writers/passes | `Simulation/Ecs/*` | 普通 C# 类型 |
| Pass modules | `Simulation/Passes/*` | 普通 C# 类型 |
| Lockstep host/snapshot/checksum | `Simulation/Lockstep/*` | 大部分为普通 C# 类型 |
| Registry/Runtime modules | `Simulation/Runtime/*` | 大部分为普通 C# 类型 |

`SimulationWorld` partial 已清零，并已通过普通子模块引用管理主要职责。这是未来边界
整理的基础，不重新引入 partial 或第二套 World。

### 4.2 当前 Mono 类型

当前 `Simulation` 范围中至少有以下 Mono 类型：

| 类型 | 当前路径 | 目标归属 |
|---|---|---|
| `SimulationTickDriver : SingletonBehaviour<SimulationTickDriver>` | `Simulation/Host/SimulationTickDriver.cs` | 保留为 Mono Host |
| `BattleManagedMemoryFrameBeginProbe : MonoBehaviour` | `Simulation/Runtime/BattleManagedMemoryBoundary.cs` | 移到 Host/Diagnostics adapter |
| `BattleManagedMemoryFrameEndProbe : MonoBehaviour` | `Simulation/Runtime/BattleManagedMemoryBoundary.cs` | 移到 Host/Diagnostics adapter |

### 4.3 当前反向依赖

以下是未来实施必须关闭的已观察耦合，不得把它们描述为已经完成：

1. `SimulationWorld` 直接调用 `BattleCentralPresentationMountRegistry`，并传递
   `entity.Renderer`。
2. `SimulationRegistryModule` 直接读取 `entity.Renderer`。
3. `SimulationRegistryModule` 直接调用 mount registry 的 bind/reset。
4. `SimulationRegistryModule` 直接调用 `entity.Sprite.Hide()`、`HideShadow()` 和
   `SetPresentationSuppressed(...)`。
5. `BattleLockstepSession` 直接持有 `SimulationTickDriver`，使 Lockstep 层知道 Mono Host
   的具体实现。
6. managed-memory 两个 Mono probe 与非 Mono boundary 类型位于同一生产文件，并直接
   持有 `SimulationTickDriver`。
7. `NTSDBattleTickSystem` 同时编排模拟阶段和 presentation dispatch/finalization，边界
   尚未完全显式化。
8. Core/Runtime/Pass/ECS 中仍存在 `Vector2/Vector3/Mathf/Debug` 等 UnityEngine 依赖。
   它们不是 L1 的首要违规，但属于 L2 inventory。
9. `Simulation` 下没有 asmdef/asmref，当前目录边界不能阻止 Core 重新引用 Host 或
   Rendering。

### 4.4 当前依赖图

```text
SimulationTickDriver (Mono)
        |
        v
NTSDBattleTickSystem -> SimulationWorld -> Registry / Pass / AI / Stage
        |                      |                  |
        |                      |                  +-> entity.Renderer / Sprite
        |                      +-> MountRegistry
        +-> presentation dispatch/finalization

BattleLockstepSession -> SimulationTickDriver (Mono concrete type)
ManagedMemory Mono probes -> SimulationTickDriver
```

当前不是完全单向依赖；后续重构必须用端口和纯值 command 收口，而不是仅移动文件。

## 5. 目标分层

### 5.1 Simulation Core

建议最终包含：

```text
Simulation/Core
Simulation/Runtime（logic-only 部分）
Simulation/Passes
Simulation/Ai
Simulation/Ecs
Simulation/Lockstep（logic/session 部分）
Simulation/DataContracts
```

Core 可以拥有：

- `SimulationWorld`、`NTSDBattleTickSystem`。
- logic entity、runtime state、slot/generation。
- 输入帧纯值、RNG、pass、AI、collision/hit、checksum、snapshot。
- 预分配 command buffer 和 immutable publication。
- 生命周期状态机的纯值状态与后置条件。

Core 不得拥有：

- Mono callback。
- `GameObject`、`Component`、`Transform`。
- Renderer、Sprite、Material、Texture。
- Scene、Camera、Resources、Addressables。
- 创建型 singleton `.Instance`。
- Unity 对象池或 prefab。

### 5.2 Mono Host

建议最终包含：

```text
Simulation/Host
Simulation/Host/Input
Simulation/Host/Lifecycle
Simulation/Host/Diagnostics
```

Host 负责：

- Unity 生命周期接入。
- `Time.unscaledDeltaTime` 外层累计，但不改变单 tick dt。
- 输入采集并转为 `FrameInputSet`。
- 主线程/worker 启停和 join。
- ordered shutdown 顶层编排。
- Scene unload、domain reload 和 Editor play transition。
- 把 Core publication 交给 Presentation adapter。

Host 不拥有 gameplay state，不以 Transform/Renderer 作为逻辑真值。

### 5.3 Presentation Adapter

建议最终包含：

```text
Simulation/Presentation
Animation/Rendering
Animation/Rendering/Adapters
```

Presentation 负责：

- `BattleCentralPresentationMountRegistry`。
- Renderer/Sprite/GameObject mount、detach、hide、recycle。
- Central Render submission 和 legacy renderer refresh。
- immutable presentation frame 的消费。
- main-thread-only resource lease/acknowledgment。

Presentation 不得直接修改 HP、位置、速度、frame、slot、generation、link、holder、target
等逻辑真值。

### 5.4 Compatibility Shell

当前 `LF2Entity` 及相关类型可能同时暴露逻辑数据和 Renderer/Sprite 引用。迁移期间允许
保留兼容 façade，但必须满足：

- 新 Core API 使用 `RuntimeEntityHandle`、纯值 view 或 logic entity interface。
- 新模块不得继续添加 Renderer/Sprite 读取。
- Renderer 引用逐步迁移到 `PresentationBindingTable`，不新增第二份逻辑 state。
- 兼容属性只转发，不在 Core hot path 中成为正式 owner。
- 删除兼容属性必须单独 Change，不能与第一批 seam extraction 同时进行。

## 6. 强制依赖规则

### 6.1 允许方向

```text
Mono Host ----------> Core public ports
Mono Host ----------> Presentation adapters
Presentation -------> Core immutable views/publications
Core ---------------> Pure contracts / command buffers
```

### 6.2 禁止方向

```text
Core -X-> SimulationTickDriver
Core -X-> Mono singleton .Instance
Core -X-> GameObject / Transform / Renderer / Sprite
Core -X-> BattleCentralPresentationMountRegistry
Core -X-> Unity object pool / prefab / Scene
Presentation -X-> Core mutable private state
Presentation -X-> logic Transform writeback
```

### 6.3 允许的返回路径

“禁止反向调用”不等于完全没有返回值。允许的返回仅限受控纯值协议：

- command accepted/rejected。
- resource lease acquired/released。
- presentation detach acknowledgment。
- publication generation/tick acknowledgment。
- shutdown postcondition result。

返回不得携带 `GameObject`、Renderer 或任意可变 Core object graph。

## 7. 目标端口与数据合同

接口名在实施前可调整，但职责不得模糊。

### 7.1 Tick Host 端口

```csharp
public interface ISimulationTickHost
{
    SimulationWorld World { get; }
    int CurrentTick { get; }
    bool TrySubmitFrameInput(in FrameInputSet input);
    bool TryStepOneTick();
}
```

用途：让 Lockstep/Replay 依赖纯接口，不依赖 `SimulationTickDriver` 具体 Mono 类型。

### 7.2 Presentation binding command

```csharp
public readonly struct PresentationBindingCommand
{
    public RuntimeEntityHandle Handle { get; }
    public PresentationBindingOperation Operation { get; }
    public int PublicationTick { get; }
}
```

操作至少覆盖 bind、detach、suppress、restore。Core 只写 command，不传 Renderer。

### 7.3 Presentation binding table

Mono/Presentation 层维护：

```text
RuntimeEntityHandle -> PresentationBinding
```

`PresentationBinding` 可以包含 Renderer、Sprite mount、pool lease，但不得进入 Core
snapshot/checksum。

### 7.4 Presentation detach acknowledgment

```csharp
public readonly struct PresentationDetachAck
{
    public RuntimeEntityHandle Handle { get; }
    public uint Generation { get; }
    public bool Detached { get; }
}
```

ack 必须匹配 slot/generation，旧 generation 不得解除新 occupant 的表现绑定。

### 7.5 诊断端口

Core 诊断使用纯接口或 ring buffer，不直接 `Debug.Log`：

```csharp
public interface ISimulationDiagnosticsSink
{
    void Record(in SimulationDiagnosticEvent diagnosticEvent);
}
```

生产可使用 no-op sink；Mono Host 再决定 Console、Profiler 或文件输出。

## 8. 关键流程调整

### 8.1 Tick

```text
Mono Update
→ Host 累计外层时间
→ 构建/选择 FrameInputSet
→ Core.RunTick(tick, input)
→ Core 发布 immutable presentation frame
→ Host/Presentation 在主线程消费
```

Core tick 内不得读取 `Time.deltaTime`、Input、Transform 或 Renderer。

### 8.2 Entity 注册

```text
Core Registry claim slot/generation
→ 发布 BindPresentation(handle)
→ Presentation 创建/借用 renderer
→ PresentationBindingTable 绑定 handle
```

Core 注册成功不依赖 Renderer 已经生成；表现可以延迟，但 first-visible tick 合同必须保持。

### 8.3 Entity 释放

```text
Core 标记 pending unregister
→ publication 不再包含旧 entity
→ 发布 DetachPresentation(handle)
→ Presentation 验证 generation、隐藏并归还 renderer
→ 返回 detach ack
→ Registry 完成允许的 slot release/reuse 边界
```

具体 ack 是否阻塞 slot reuse 必须根据现有 generation/no-ghost 合同通过测试决定，不能凭
架构偏好改变当前可见 tick。

### 8.4 Ordered shutdown

继续服从固定十一阶段：

| 阶段 | Mono Host | Core | Presentation |
|---|---|---|---|
| 禁止 tick/input | 关闭 Update/input gate | 拒绝新 pass | 停止新 publication 消费 |
| 停 worker | stop/join | 不再执行 worker tick | 保持资源有效 |
| 关 spawn | 调用 gate | OPoint/structural reject | 无新 mount |
| unseal | 调用非创建型端口 | allocation state | 不访问 singleton |
| 清 publication | 协调 | 清纯值 publication | 清 submission |
| discard OPoint | 协调 | 丢弃 pending task | 无 materialize |
| recycle renderer | 主线程执行 | 保持 handle 可验证 | detach/recycle/ack |
| clear logic entity | 协调 | Registry 清理 | 不再持有旧 handle |
| unbind World | 解除 Host 引用 | 完成 postcondition | 不回调旧 World |
| pool quiesce | 执行 | 无 pool 依赖 | 确认无 borrower |
| boundary cleanup | Scene adapter | 清纯值 Stage state | 清 runtime carrier |

## 9. 逐模块未来调整

### 9.1 `SimulationTickDriver`

保留为 Mono lifecycle owner。未来调整：

- 实现 `ISimulationTickHost`。
- 把输入、worker、lifecycle、presentation dispatch 组织为明确 adapter 引用。
- 不把自身实例传入 Lockstep/Core module。
- 不把 `Update/LateUpdate` 方法迁入 Core。

### 9.2 `BattleLockstepSession`

- 构造参数从 `SimulationTickDriver` 改为最小 `ISimulationTickHost`/frame execution port。
- session 不查询 GameObject、Scene、Time 或 Mono singleton。
- 保持相同 tick、journal、checksum 和 input-ready 行为。

### 9.3 Managed-memory probes

- 把两个 Mono probe 移到独立 `Host/Diagnostics` 文件。
- `BattleManagedMemoryBoundary` 保留纯值计数和状态机。
- probe 仅转发 frame begin/end 观察，不拥有统计状态。
- 移动前后 allocation 计数、首次违规 tick 和 benchmark 输出必须一致。

### 9.4 `SimulationWorld`

- 移除对 `BattleCentralPresentationMountRegistry` 的直接调用。
- 移除需要 Renderer/Sprite 参数的 Core API。
- 保留纯值 publication/command buffer 的组合和生命周期编排。
- presentation façade 在调用者完成迁移后单独删除。

### 9.5 `SimulationRegistryModule`

- 不读取 `entity.Renderer` 或 `entity.Sprite`。
- bind/reset/hide/recycle 转为 `PresentationBindingCommand`。
- Registry 只拥有 slot/generation/entity membership。
- shutdown 时用 generation-aware detach command 和 postcondition，不直接操作 Unity 对象。

### 9.6 `LF2Entity` compatibility shell

- 冻结新增 Renderer/Sprite 依赖。
- 盘点所有 `Renderer`、`Sprite`、`LogicObject` 读写方。
- 新增独立 binding table 后，先迁移 Registry/World 读取者。
- 最后再评估是否删除 entity 上的兼容表现引用。
- 该阶段跨 `Animation/LF2Objects` 与 `Animation/Rendering`，必须独立 Change 和 Play 验证。

### 9.7 `NTSDBattleTickSystem`

- 保持 C++ release pass 顺序。
- 把“生成 publication”与“执行 Unity presentation”分成两个显式端口。
- Core tick 可以决定何时 publication 完成，但不直接调用 Renderer。
- CentralOnly/Legacy mode 的表现选择属于 Presentation adapter，不进入 gameplay 分支。

### 9.8 Stage modules

- `SimulationStageWaveModule` 保持 logic-only。
- `SimulationStageRenderModule` 重新审阅：纯排序/publication 可留 Core；Renderer、素材和
  Scene carrier 操作移到 Presentation/Host。
- `Vector2/Vector3` 在 L1 可暂留；Transform/Scene 引用必须移出。

### 9.9 日志与数学依赖

L1 完成后再执行：

- `Debug.Log*` 改为 diagnostics sink 或 Host logger。
- `Mathf` 可机械替换为行为等价的 `System.Math/MathF` 前，必须覆盖边界/rounding 测试。
- `Vector2/Vector3` 是否替换为 fixed/int value type，必须以 checksum 和 C++ 数值语义为
  前提，不作为美化任务。

## 10. asmdef 策略

当前禁止先创建 asmdef 强切边界。原因：

- 现有 Core 仍引用 Rendering/Animation 类型。
- 大量 `internal` API 依赖同一程序集可见性。
- 立即拆程序集会产生循环依赖，迫使扩大 public API。
- 编译修复容易演变成无证据的大规模架构改写。

只有在源码依赖方向已经单向后，才进入程序集阶段：

```text
NTSD.Simulation.Contracts
        ^
        |
NTSD.Simulation.Core
        ^
        |
+-------+----------------+
|                        |
NTSD.Simulation.Host     NTSD.Simulation.Presentation
```

强制引用：

- Contracts 不引用 Unity/Host/Presentation。
- Core 只引用 Contracts 和正式数据模型。
- Host 引用 Core/Contracts/Unity。
- Presentation 引用 Core publication/Contracts/Unity Rendering。
- Core 不引用 Host 或 Presentation asmdef。

`InternalsVisibleTo` 只能作为有期限迁移措施，必须在 Change Record 登记删除计划。

## 11. 实施批次

每批使用独立 Change ID，禁止一次性大爆炸式重构。

| 批次 | 内容 | 主要文件 | 必跑验证 |
|---|---|---|---|
| B0 | 冻结 inventory、依赖图、architecture guards | 文档+Editor tests | compile、guard baseline |
| B1 | `ISimulationTickHost`，Lockstep 去 concrete Mono | Driver/Lockstep | lockstep、checksum、input |
| B2 | Managed-memory Mono probes 移到 Host/Diagnostics | boundary/probes | benchmark、allocation、Play |
| B3 | 建立 presentation command/ack/binding table | Contracts/Presentation | generation/no-ghost/central |
| B4 | Registry/World 去 Renderer/Sprite/MountRegistry | World/Registry/adapter | structural、shutdown、Play |
| B5 | LF2Entity compatibility binding 收口 | LF2Objects/Rendering | actor/weapon/effect、pool |
| B6 | Tick publication 与 Unity dispatch 分离 | TickSystem/StageRender | pass order、worker、central |
| B7 | Debug/Math/Unity value dependency inventory 收口 | Core modules | checksum、rounding、full |
| B8 | asmdef 强制单向引用 | assembly definitions | clean compile、full tests |
| B9 | 最终 API/compat façade 清理 | 全部相关层 | full matrix、2-cycle Play |

## 12. 每批执行规则

1. 修改脚本前建立独立 Task Contract、Change Record 和 test-first guard。
2. 一批只关闭一个 seam；不同时移动文件、改算法和删除兼容 API。
3. 先加纯值 port/command，再迁调用者，最后删除旧反向调用。
4. 迁移期间双路径只能用于 shadow compare，不能双写两份 authority state。
5. 禁止通过 `FindObjectOfType`、service locator 或新 singleton 隐藏反向依赖。
6. 禁止为了解决 asmdef 编译而把大量 internal 直接改 public。
7. 每批完成后扫描禁止依赖，并运行最窄 focused，再扩大验证。
8. 任何 gameplay/checksum/pass-order first difference 立即停止，不在同批修规则。

## 13. 架构守卫

未来 B0 先建立只读/测试守卫，建议至少覆盖：

### 13.1 Mono ownership guard

- `Core/Runtime/Passes/Ai/Ecs/Lockstep` 不得声明 `MonoBehaviour`。
- 允许清单初始只包含 `Host/SimulationTickDriver.cs`。
- probe 迁移完成后 Runtime Mono allowlist 必须为0。

### 13.2 Forbidden Unity object guard

Core 禁止：

```text
GameObject
Component
Transform
Renderer
SpriteRenderer
MonoBehaviour
Object.Instantiate
Object.Destroy
FindObjectOfType
Resources.Load
```

不要简单匹配注释；守卫应使用 Roslyn/AST 或受控 token scanner。

### 13.3 Dependency direction guard

- Core namespace 不引用 Host/Rendering adapter namespace。
- Lockstep 不引用 `SimulationTickDriver` concrete type。
- Registry/World 不引用 `BattleCentralPresentationMountRegistry`。
- Core 不调用创建型 `.Instance`。

### 13.4 State authority guard

- Renderer/Transform/PresentationBinding 不进入 checksum/snapshot。
- Presentation adapter 不写逻辑 position/HP/frame/slot/generation。
- 每个 mutable state 只有一个 owner。

### 13.5 asmdef guard

仅 B8 启用：验证 assembly reference graph 无 Core→Host/Presentation 边。

## 14. 验收矩阵

每批按风险选择，下列是最终 B9 的最低矩阵：

1. Unity compile 0 error。
2. Mono ownership / forbidden dependency / assembly graph guards 全通过。
3. 固定30 Hz、pass order、RNG、slot/generation、OPoint focused 全通过。
4. AI、collision/hit、worker、checksum、snapshot/restore、lockstep 全达到迁移前基线。
5. Central Render actor/weapon/effect/shadow/health publication 不新增 ghost 或断批差异。
6. `BattleRuntimeSelfCheck` 实际执行；任务外 first-failure 单列。
7. 完整 EditMode 实际执行；不得只跑隔离 compiler。
8. 两轮真实 Play→等待延迟生成→Stop→re-enter→Stop。
9. 每轮 Scene dirty unchanged，cleanup warning为0。
10. factory/pool/boundary carrier无残留。
11. dedicated worker eligibility：纯 logic world 可运行；Unity binding 存在时 fail-closed 行为符合
    当前合同，直到 binding 完全解耦后另行批准改变。
12. Windows Mono correctness；IL2CPP 仅在模块可用时执行，不伪造通过。

## 15. 性能与线程约束

- port/command 不得导致每 tick delegate、LINQ、boxing 或临时集合分配。
- command/publication 使用预分配 buffer、stable slot/generation 和明确容量策略。
- Unity Object 永远不进入 dedicated simulation worker。
- Presentation ack 只在主线程产生，Core 只消费纯值副本。
- 不为边界整洁破坏 current 1000-active、0GC 或30 Hz预算。
- 若 async resource load 参与 binding，完成回调必须验证 world/session/generation仍有效。

## 16. 风险与回滚

| 风险 | 表现 | 控制 |
|---|---|---|
| first-visible tick变化 | entity晚一帧或ghost | 冻结publication/generation focused |
| slot reuse竞态 | 旧ack解绑新renderer | ack携带handle+generation |
| shutdown死锁 | 等待presentation ack但主线程停止 | shutdown阶段和timeout/fail-closed合同 |
| asmdef循环 | 大量public化或编译失败 | asmdef最后实施 |
| worker线程触碰Unity | exception/crash | Core command纯值守卫 |
|双authority|logic/renderer各维护一套状态|只允许shadow compare，不允许双写真值|
|性能回退|command分配/扫描增加|预分配与benchmark门禁|

回滚单位是单批 Change：恢复该批新增 port 的调用者和旧 seam，不使用破坏性 Git 命令，
不跨批回退已经验证的独立边界。

## 17. 完成定义

只有同时满足以下条件，才能报告 Mono/非 Mono 分层完成：

1. `Simulation` 中所有 Mono 类型均位于明确 Host/Presentation/Diagnostics adapter。
2. Core/Runtime/Pass/AI/ECS/Lockstep 不声明 MonoBehaviour。
3. Core 不引用 GameObject/Transform/Renderer/Sprite/Mono singleton。
4. `SimulationWorld`、`SimulationRegistryModule` 不调用 mount registry 或隐藏 Sprite。
5. Lockstep 不持有 `SimulationTickDriver` concrete type。
6. Renderer binding由独立 handle/generation table拥有。
7. Presentation只消费 immutable publication，不反写逻辑真值。
8. ordered shutdown通过纯值command/ack保持固定十一阶段。
9. 程序集引用图在 B8 后由编译器强制为单向。
10. 完整验证矩阵达到迁移前基线或更高，且没有未解释 first difference。

仅完成以下任一项不能宣称完成：

- 只移动目录。
- 只把 `MonoBehaviour` 改成普通类。
- 只增加接口但保留 concrete/Renderer 反向调用。
- 只创建 asmdef。
- 只让编译变绿。
- 只跑单个 architecture test。

## 18. 当前停止点与未来恢复方式

本文件完成后保持：

```text
SIMULATION-MONO-BOUNDARY-REFACTOR-001
DOCUMENTED
IMPLEMENTATION_NOT_STARTED
USER_HOLD
```

当前不创建实施 Change Record，不加入 active Change Ledger，不修改任何 C#。用户后续明确
批准执行时，从 B0 开始：

1. 重新读取本计划和当前 `AGENTS.md`。
2. 重新扫描现状，更新 inventory，不假设本文行号仍有效。
3. 建立 B0 Task/Change Record 和代码路径清单。
4. 先写架构守卫并取得当前基线。
5. 获得 B0 验证后，才选择 B1；不得直接跳到 asmdef 或大规模 Renderer 移除。

