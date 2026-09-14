# Simulation Mono / 非 Mono 边界整理与重构计划

> **2026-09-02 权威更新，不改变 USER_HOLD：** 后续若恢复本计划，必须先读
> `docs/ai/CURRENT-AUTHORITY.md`，并按 NTSD 2.8-Logan 的 33 ms 正常逻辑间隔、1000 物理 slot、
> 双 RNG 流和新 tick 入口重新盘点。本文及旧根规则中依赖 NTSD 2.4 或固定 30 Hz 的边界只保留为
> 历史约束，不能驱动实现；本次仍不授权修改 C#、Scene、Prefab、asmdef 或运行行为。

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
- 正常逻辑 cadence 为精确 33 ms；F5 快速 cadence 为精确 3 ms；
  LocalFreeRun wall-clock debt 最多 2 个当前 cadence interval
  （19.7-S6 强制统一口径）。
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
- 改变 33 ms 正常 cadence、3 ms 快速 cadence 或输入延迟语义。
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
// P-1（19.7，已批准落地）：Host 公共端口不暴露可变 SimulationWorld，
// 拆为最小能力端口；成员在 B1 Task Contract 中按实际调用方定型。
public interface ISimulationFrameExecutor
{
    int CurrentTick { get; }
    bool TrySubmitFrameInput(in FrameInputSet input);
    bool TryStepOneTick();
}

public interface ISimulationChecksumReader
{
    bool TryReadChecksum(int tickIndex, out SimulationChecksum checksum);
}

public interface ISimulationSnapshotPort
{
    // 受控快照 Capture / Restore
}

public interface ISimulationLifecyclePort
{
    // Start / Stop / State
}
```

用途：让 Lockstep/Replay 依赖纯接口，不依赖 `SimulationTickDriver` 具体 Mono
类型；任何端口不暴露可变 `SimulationWorld` 聚合根（World 访问留在 Host 内部
编排层）。

### 7.2 Presentation binding command

```csharp
// P-4（19.7，已批准落地）：command 携带完整 epoch 链，仅 entity generation
// 不足以区分新旧 World。
public readonly struct PresentationBindingCommand
{
    public RuntimeEntityHandle Handle { get; }
    public PresentationBindingOperation Operation { get; }
    public int PublicationTick { get; }
    public int SessionEpoch { get; }
    public int WorldGeneration { get; }
    public int PublicationSequence { get; }
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
// P-4（19.7，已批准落地）：ack 与 command 使用同一套 epoch 链。
public readonly struct PresentationDetachAck
{
    public RuntimeEntityHandle Handle { get; }
    public uint Generation { get; }
    public bool Detached { get; }
    public int SessionEpoch { get; }
    public int WorldGeneration { get; }
    public int PublicationSequence { get; }
}
```

epoch 链比较规则（P-4）：

```text
SessionEpoch 不匹配      → 整条回调/ack 丢弃
WorldGeneration 不匹配   → 不访问旧 World
EntityGeneration 不匹配  → 不解绑新 occupant
PublicationSequence 过旧 → 不覆盖新 publication
TickIndex 只用于顺序与诊断，不单独充当对象身份
```

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
Core（权威，P-2 已批准落地）：
  按权威 tick 完成 pending unregister、slot release、generation 递增；
  发布生命周期事件（handle + generation + session epoch +
  world generation + publication sequence）。

Presentation（异步，不回写 Core）：
  收到 DetachPresentation 后异步 detach 旧 binding；
  迟到 ack 只回收旧表现资源；
  epoch 链任一不匹配 → 丢弃该 ack，不触碰新 occupant。
```

Core slot 生命周期不等待表现 ack；no-ghost 合同由 binding table +
epoch/generation 校验保证（迟到回收不得解绑新 occupant），first-visible
tick 合同不变。正常模拟中 Presentation 不是模拟生命周期的裁决者；仅有序
关闭阶段可将"Presentation 已回收"作为关闭后置条件。

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

- 实现 7.1 的最小端口组合（`ISimulationFrameExecutor` 等，P-1）。
- 把输入、worker、lifecycle、presentation dispatch 组织为明确 adapter 引用。
- 不把自身实例传入 Lockstep/Core module。
- 不把 `Update/LateUpdate` 方法迁入 Core。

### 9.2 `BattleLockstepSession`

- 构造参数从 `SimulationTickDriver` 改为 7.1 的最小端口
  （`ISimulationFrameExecutor`/`ISimulationChecksumReader` 等，P-1）。
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

### 9.9 日志、线程相关 API 与数学依赖（P-3 已批准落地）

Debug 与线程相关静态 API 前移至 worker 准入范围（B0–B6），不再是"L1 后"：

- B0 记录现有 `Debug` 债务 baseline（具名，见 13.1）。
- worker 准入路径（B6 exit 前）：`Time`、`Input`、`UnityEngine.Random`、
  `Resources`、`Application`、`SystemInfo`、`Object.Instantiate/Destroy` 等
  主线程限定/进程环境型调用移除或隔离；`Debug` 调用改为 diagnostics sink
  （7.5）或证明不可达——**硬门**：生产 worker 热路径 `Debug` 调用为 0。
- **L1 完成定义：Core 对 `UnityEngine.Debug` 的依赖为 0。** `Debug` 即使
  在当前 Unity 版本可从工作线程调用，仍是 Core → Unity 服务依赖，并可能
  带来日志锁、字符串分配与不可控 IO，不允许只写成"建议收口"。
- 允许 B0 inventory 或临时诊断路径记录既有 `Debug` 债务，按 13.1 baseline
  管理并限期清除。

B7 收窄为纯值/数学收口：

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
| B1 | 7.1 最小端口落地（P-1），Lockstep 去 concrete Mono | Driver/Lockstep | lockstep、checksum、input |
| B2 | Managed-memory Mono probes 移到 Host/Diagnostics | boundary/probes | benchmark、allocation、Play |
| B3 | 建立 presentation command/ack/binding table | Contracts/Presentation | generation/no-ghost/central |
| B4 | Registry/World 去 Renderer/Sprite/MountRegistry | World/Registry/adapter | structural、shutdown、Play |
| B5 | LF2Entity compatibility binding 收口 | LF2Objects/Rendering | actor/weapon/effect、pool |
| B6 | Tick publication 与 Unity dispatch 分离；worker 准入路径线程相关 Unity API 清零（含 Debug 硬门，见 9.9） | TickSystem/StageRender | pass order、worker、central |
| B7 | 纯值/数学依赖收口（Mathf/Vector 等 L2；Debug 已前移 B0–B6） | Core modules | checksum、rounding、full |
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

### 13.1 Mono ownership guard（P-5 已批准落地）

- `Core/Runtime/Passes/Ai/Ecs/Lockstep` 不得声明 `MonoBehaviour`。
- B0 生成具名 current-debt baseline：每条债务记录文件、symbol、owner、
  目标批次与删除条件；baseline 之后不允许新增债务；
  不使用宽泛通配 allowlist 掩盖新增违规。
- B2：移除两个 Runtime Mono probe 债务，Runtime Mono debt 归零。
- B8：asmdef 强制最终依赖图。

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

### 13.6 Thread-affine / environment API guard（P-5 已批准落地）

- worker 准入路径禁止：`Time`、`Input`、`UnityEngine.Random`、`Resources`、
  `Application`、`SystemInfo`、`Object.Instantiate`、`Object.Destroy`。
- 生产 worker 热路径 `Debug` 调用必须为 0（diagnostics sink 或证明不可达）；
  L1 完成时 Core 对 `UnityEngine.Debug` 依赖为 0（见 9.9）。
- 禁止反射、service locator、`FindObjectOfType` 绕过依赖方向守卫。
- publication/command buffer 每 tick 分配守卫：预分配容量之外的新分配即失败。
- capacity overflow 守卫：buffer 满必须 fail-closed 报错，不得静默扩容。
- token scanner 不得成为永久终态：Roslyn/AST 或 asmdef 编译期强制是后续
  升级方向（S3）。

## 14. 验收矩阵

每批按风险选择，下列是最终 B9 的最低矩阵：

1. Unity compile 0 error。
2. Mono ownership / forbidden dependency / assembly graph guards 全通过。
3. 精确 33 ms 正常 cadence（F5 3 ms、debt ≤2 interval）、pass order、RNG、slot/generation、OPoint focused 全通过。
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
- Unity Object 永远不进入 dedicated simulation worker；`Time/Input/Random/
  Resources/Application/SystemInfo/Object.Instantiate/Destroy` 等主线程限定
  API 同样不进入 worker 路径（见 13.6）。
- Presentation ack 只在主线程产生，Core 只消费纯值副本；正常模拟中 Core
  slot 生命周期不等待表现 ack（见 8.3）。
- 不为边界整洁破坏 current 1000-active、0GC 或 33 ms cadence 预算。
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

## 19. 外部评审意见记录（2026-09-13，待综合评审）

> 性质：本节为外部架构评审的意见记录，仅供后续综合评审参考。本节不改变本计划
> 状态（仍为 `DOCUMENTED / IMPLEMENTATION_NOT_STARTED / USER_HOLD`），不构成任何
> 实施授权，也未修改任何 C#、Scene、Prefab、asmdef 或运行行为。未采纳前，正文
> 条款维持原文效力。
>
> 评审人：Factory Droid（应用户要求记录，供用户后续综合评审）
> 评审方式：全文通读本计划 + 抽查 4.3 节声称的反向依赖与代码现状

### 19.1 评审结论

方案方向性成立，可作为实施合同冻结。诊断、顺序、风险对冲与反自欺条款均与
代码现状和既有合同（33 ms 节奏、十一阶段关闭、no-ghost、0GC 预算）一致。

### 19.2 诊断真实性抽查（2026-09-13）

| 4.3 节声明 | 抽查结果 |
|---|---|
| SimulationWorld 直接调用 MountRegistry 并传 `entity.Renderer` | 属实（`Simulation/Core/SimulationWorld.cs` 975-976 行 `BindOwnerRuntime(entity.Renderer, ...)`） |
| BattleLockstepSession 直接持有 `SimulationTickDriver` 具体类型 | 属实（`Simulation/Lockstep/Session/BattleLockstepSession.cs` 7/16 行） |
| 两个 managed-memory Mono probe 与非 Mono 边界类型同文件并持有 Driver | 属实（`Simulation/Runtime/BattleManagedMemoryBoundary.cs` 411-437 行） |

### 19.3 认可要点

1. L1/L2 分级（2.3）避免过度纯化；L1 完成后 Core 仅剩 `Vector3/Mathf` 等线程
   安全纯值类型，14.11 的 dedicated worker eligibility 声明在技术上成立。
2. 第 3 节不可变清单与禁止搭车清单，防止重构携带行为变化。
3. asmdef 最后实施（第 10 节）并给出原因，规避 public 化失控。
4. 受控返回路径（6.3）与 generation-aware detach ack（7.4）正确对冲 slot 重用竞态。
5. 十一阶段关闭按层拆分责任（8.4），与既有合同一致。
6. 第 17 节"只做 X 不算完成"反清单与第 13 节守卫设计。

### 19.4 建议（状态：PROPOSED，待综合评审决定是否采纳）

| 编号 | 建议 | 建议落点 | 状态 |
|---|---|---|---|
| S1 | 兼容壳补"递减推进"硬守卫：每个 LF2Entity 兼容表现属性登记剩余读取者计数，B5 起每次扫描计数必须下降，归零才允许删除该兼容属性。现有 5.4 只有"不得新增"禁止项，缺"必须递减"推进项，兼容层易成为永久债 | 5.4 / 9.6 / B5 | PROPOSED |
| S2 | 第 1 节补充性能因果定位：本计划只取得 worker 化资格，完成后 Stats/逻辑帧率不会自动提高；Stats 60/120 依赖后续"worker 化 + 热点优化"独立立项，三步三份合同 | 1 | PROPOSED |
| S3 | B0 架构守卫首选轻量"受控 token scanner + 白名单测试"起步（13 节已允许该选项），Roslyn/AST 作为后期升级，避免守卫工具自研拖延 B0 | 13 / B0 | PROPOSED |
| S4 | 排期对 B3/B6 预留缓冲：detach-ack 时机与 first-visible tick 的相容性是全文唯一无法预先定量风险的区域（计划已正确推迟到测试决定）；B4 先于 B6 的顺序应保持，以缩小 B6 爆炸半径 | 11 | PROPOSED |
| S5 | 无需修改文档：18 节已内建"B0 重新盘点、不假设行号有效"；执行时必须作为恢复计划的第一件事，当前代码仍有外部改动流入，此项要当真执行 | 18 | 已内建 |
| S6 | 第 3 节"固定逻辑频率 30 Hz"与头部 2026-09-02 权威更新（33 ms 正常逻辑间隔为当前权威，旧 30 Hz 表述仅保留为历史约束）存在内部不一致，建议改为 33 ms 语义或显式引用头部说明，避免恢复实施时以旧表述驱动实现 | 3 | PROPOSED |

### 19.6 评审边界声明

- 本节由评审会话按用户要求追加，属文档维护，不触碰脚本，不属于任何
  Change Record 范围。
- 建议采纳与否由用户综合评审决定。

### 19.7 GPT6 综合评审裁定与正文修正清单（2026-09-14）

> 性质：2026-09-14 外部综合评审（GPT6）对本计划全文 + 19.4 节建议 S1–S6 的
> 裁定记录。总体裁定：**修改后采纳**（三层设计、L1/L2 分级、B0–B9、小批次
> 迁移、asmdef 最后实施整体合理）。本小节同时列出对正文的修正清单
> P-1…P-5。R2 复核（2026-09-14，同评审方）裁定 `CORRECTION_SET_APPROVED`：
> P-1～P-5 与 S1～S6 的**文档正文落地已获批准并于当日执行**（见 19.7.5）；
> 该批准仅覆盖文档修正，**不构成代码大包实施授权**——代码仍必须按
> B0→B9 顺序逐批独立立项（Task Contract + Change Record + focused test +
> 回滚边界）。

#### 19.7.1 对 19.4 节建议 S1–S6 的裁定

| 编号 | 裁定 | 理由与修改 |
|---|---|---|
| S1 | 修改后采纳 | 读取者 manifest 正确；"每次扫描必须下降"过于机械（某准备批次可能数量不变）。改为：不得增加；每个 B5 子批声明迁移目标；B5 exit 时生产读取者必须为 0；Editor/测试读取者单列 |
| S2 | 采纳 | 分层只提供线程与所有权资格，不自动提高 Stats；在第 1 节回链性能方案，并注明现有 worker 仍需资格/重叠/背压测量 |
| S3 | 修改后采纳 | token scanner 起步可行，但必须排除注释/字符串/测试夹具误报、按目录/namespace/symbol 精确配置、已知债务用具名 baseline（不用宽泛 allowlist），并把 Roslyn/asmdef 写成后续强制升级，scanner 不得成为永久终态 |
| S4 | 采纳 | B4 先于 B6 顺序正确；补充：先修正 ack 语义（见 P-2），防止 B3 把 Presentation 变成模拟生命周期裁决者 |
| S5 | 采纳（已内建） | 代码持续变化，B0 必须重新生成 inventory，不照抄 9 月 2/13 日行号 |
| S6 | 强制采纳 | 统一精确 33 ms、F5 3 ms、两 interval debt；这是权威合同，非可选文字修正 |

#### 19.7.2 新增外部建议 S7–S9

| 编号 | 内容 |
|---|---|
| S7 | `ISimulationTickHost` 不暴露 `SimulationWorld`，拆成最小 frame/checksum/snapshot/lifecycle 端口 |
| S8 | Presentation detach ack 不阻塞正常 slot release；ack 只回收表现资源，并携带 session/world/entity generation |
| S9 | worker 准入前清除 `Time/Input/Random/Resources/Application/SystemInfo/Object.Instantiate/Destroy` 等线程相关/主线程限定 Unity API；`Vector/Mathf` 可留 L2；`Debug` 线程安全但建议经诊断 sink 收口（非硬准入项） |

#### 19.7.3 正文修正清单（已批准，2026-09-14 落地正文；P-3 按 R2 附加 Debug 硬门）

| 编号 | 正文位置 | 修正内容 |
|---|---|---|
| P-1 | 7.1 | `ISimulationTickHost` 移除 `World` 属性，拆为最小 frame executor/checksum/snapshot/lifecycle 端口，不在 Host 公共端口暴露可变聚合根 |
| P-2 | 8.3 | 实体释放改为非阻塞：Core 按权威 tick 完成 unregister/slot release/generation 递增并发布含 handle+generation+session epoch 的生命周期事件；Presentation 异步 detach，迟到 ack 只回收旧表现资源，不阻塞或修改 Core slot 生命周期。仅有序关闭阶段可将"Presentation 已回收"作为后置条件 |
| P-3 | 9.9 / B6 / B7 | 线程相关静态 API（`Time/Input/Random/Resources/Application/SystemInfo` 等）清理前移至 worker 准入范围（B0–B6）；B7 收窄为纯值类型、数学舍入与可选 L2 |
| P-4 | 7.2/7.4 | command/publication/ack 增加 `SessionEpoch`/`WorldGeneration`/`PublicationSequence` 字段（仅 entity generation 不足以区分新旧 World） |
| P-5 | 13 节守卫 | B0 已知债务进 baseline manifest（具名，不用宽泛 allowlist）；新增禁止反射/service locator 绕过守卫、publication 每 tick allocation 与 capacity overflow 守卫、thread-affine Unity API 守卫 |

#### 19.7.4 关联状态

- 姊妹计划 `BATTLE-PERF-STATS120-ROADMAP-001` 已按本次评审完成 R1 修订
  （其 Step B 前置引用本清单 P-1…P-5）；
- 综合评审同时指出本计划头部"2026-09-02 权威更新"与正文第 3 节"30 Hz"
  的不一致，已由 S6/P-3 关联覆盖（S6 强制采纳，正文修正随 P 清单执行）。

#### 19.7.5 R2 复核与正文落地记录（2026-09-14）

- R2 复核裁定：`CORRECTION_SET_APPROVED`。上轮 24 项发现中 19 项已在 R1
  入正文、4 项随本次 P-1～P-5 回写、1 项（PERF render/GPU 硬门）在 PERF
  文档 R2 补齐，遗漏 0 项。
- 本次正文落地范围：7.1（P-1 端口拆分）、7.2/7.4（P-4 epoch 链 + 比较规则）、
  8.3（P-2 非阻塞 ack/slot 生命周期）、9.9 与第 11 节 B6/B7 行（P-3 线程
  API 前移 + Debug 硬门 + B7 收窄）、13.1/13.6（P-5 守卫增强）、第 3/14/15
  节（S6 33 ms 口径统一）、19.6/19.7 排版修复。
- **P-3 修改后批准的附加硬边界**：`Debug` 不得只写"建议收口"——worker
  准入路径（B6 exit 前）必须改为 diagnostics sink（7.5）或证明不可达；
  生产 worker 热路径 `Debug` 调用为 0；L1 完成定义 = Core 对
  `UnityEngine.Debug` 依赖为 0。允许 B0 baseline 记录既有 `Debug` 债务。
  PERF 文档 F9/Step B 的对应不一致由 PERF R2 同步修正。
- R2 指出的 job 状态过时已修正：`CURRENT-AUTHORITY.md` 第 3 行显示回归
  job `aa6b0c9f…` 已有终态（FAILED，58/24，XML 归档），测量排队理由更新为
  与后继活跃任务 `NTSD28-Q06` 协调。
- 状态：`CORRECTION_SET_APPROVED / BODY_UPDATE_DONE /
  IMPLEMENTATION_NOT_STARTED / USER_HOLD`。代码实施仍按 B0→B9 分批，须
  用户逐批批准；头部计划状态行保持 `DOCUMENTED / USER_HOLD` 语义。
