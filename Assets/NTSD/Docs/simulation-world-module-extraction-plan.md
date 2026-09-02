# SimulationWorld 子模块化与 partial 移除计划

> 计划 ID：`SIMULATION-WORLD-MODULE-EXTRACTION-001`
>
> 创建日期：2026-09-01
>
> 状态：`APPROVED_BY_USER / IN_PROGRESS / ARCHITECTURE_BLUEPRINT_FROZEN / PARTIAL_DECLARATIONS_ZERO`
>
> 性质：非 Mono 战斗 Core 的架构重构；不改变战斗规则、pass 顺序、固定 30 Hz、输入时点、RNG 消费、checksum 或表现结果。

## 1. 目的

当前 `SimulationWorld` 使用多个 `partial class` 文件共享同一个 private 状态域。文件虽然按主题拆开，但编译后仍是一个超过两万行的类型，任何 partial 都可以读写其他 partial 的字段和方法，模块所有权、依赖和生命周期无法从构造函数或类型边界中确认。

本计划将 `SimulationWorld` 收敛为战斗世界聚合根和兼容 façade，由它持有明确的普通 C# 子模块引用；各子模块拥有自己的状态、缓存、算法和诊断信息。最终移除全部 `partial class SimulationWorld` 声明。

目标不是按文件机械搬代码，也不是追求类数量，而是建立以下长期边界：

```text
SimulationWorld
    负责：组合、根生命周期、固定 pass 编排、对外兼容 API

Registry / RuntimeSlots
    负责：实体注册、slot/generation、bucket、延迟结构修改

AiRuntime
    负责：AI 输入快照、感知、决策、shadow/unified state

PassPipeline
    负责：权威 pass 顺序与 pass 边界，不拥有无关业务状态

StageRuntime
    负责：stage wave、bounds、stage runtime snapshot

Presentation
    负责：immutable presentation publication 和渲染排序

FrameInput / QueryAndLinks / ObjectPoint / Snapshot / Diagnostics
    各自拥有明确能力和状态
```

## 2. 权威与不可变边界

本重构只改变 Unity 代码组织和依赖所有权，不定义或修改 gameplay 行为。

战斗行为继续以以下证据优先：

1. 用户当前明确要求。
2. `J:\QQFile\NTSD2.4\ntsd_release` release live runtime。
3. Unity 当前实现只作为重构前行为基线和回归目标。

严格保持：

- `game_tick` 对应 Unity pass 的顺序。
- 固定逻辑频率 `30 Hz`。
- input edge、组合键和 frame input 消费时点。
- entity register/unregister、slot claim/release、generation 变化时点。
- OPoint 入队、flush、materialize 和可见 tick。
- pending destroy/unregister flush 边界。
- RNG 调用次数、调用顺序和 seed。
- worker 与同步路径的逻辑结果。
- snapshot、checksum、parity trace、presentation publication 的可观察结果。
- `Running → Stopping → Stopped` 11 阶段关闭合同。

禁止借本计划实施：

- 修复或改写 Naruto、Sasuke 或其他角色规则。
- 重排 C++ release pass。
- 引入新的 ECS authority 或切换现有 fast path 默认值。
- 完成联网、回滚、重连或后台恢复。
- 修改 Scene、DAT、Prefab、URP、Input Actions、Server 或 C++。
- 同时执行完整 asmdef、目录分层或 Mono/Core 全面拆分。
- 为了减少代码行而删除 diagnostics、shadow、legacy comparison 或测试入口。

## 3. 当前基线

> 2026-09-01 实施更新：下表是重构开始前的冻结基线。当前所有
> `partial class SimulationWorld` 声明和 `SimulationWorld.*.partial.cs`
> 历史文件均已清零；Registry 与 AI 首批状态已迁入普通子模块。为先建立
> 非 partial 编译边界，尚未迁入模块的算法体被机械合并到非 partial
> `SimulationWorld.cs`，因此主文件暂时约 19,439 行。该过渡状态只满足
> “partial 清零”，不满足本计划的最终模块抽离完成定义；AI 算法主体与
> PassPipeline 仍需继续迁移。

### 3.1 文件规模

| 文件 | 行数 | 当前职责 |
|---|---:|---|
| `SimulationWorld.cs` | 1,908 | 主类、模块引用、兼容 façade、多种 writer/pass 公开面 |
| `SimulationWorld.Registry.partial.cs` | 1,672 | 构造、registry、runtime slots、reset、shutdown、结构诊断 |
| `SimulationWorld.Passes.partial.cs` | 3,138 | pass 入口、具体规则、late lifecycle、诊断 probe |
| `SimulationWorld.AiInput.partial.cs` | 5,845 | AI input snapshot、空间索引、team summary、候选产品 |
| `SimulationWorld.AiSoaShadow.partial.cs` | 2,654 | AI sensing/SoA/shadow/candidate |
| `SimulationWorld.AiDecisionShadow.partial.cs` | 4,913 | AI decision/shared/unified snapshot/shadow |
| `SimulationWorld.StageWave.partial.cs` | 794 | 已是独立 `SimulationStageWaveModule` |
| `SimulationWorld.StageRender.partial.cs` | 877 | 已是独立 `SimulationStageRenderModule` |
| `SimulationWorld.DetailTimingDiagnostics.cs` | 866 | 独立 diagnostics 类型，不是 World partial |
| 空兼容 partial 文件 | 6 | FrameInput/QueryAndLinks 迁移后的占位说明 |

相关文件合计约 `22,727` 行；真正仍属于 `partial class SimulationWorld` 的主体约 `20,130` 行。

### 3.2 已存在的正确方向

以下能力已经是普通子模块，继续复用，不重复实现：

- `SimulationEntityTraversal`
- `SimulationQueryAndLinkModule`
- `SimulationRandomWeaponDropBuffer`
- `SimulationBattleBufferModule`
- `SimulationRuntimeCapacityModule`
- `SimulationFrameInputModule`
- `SimulationObjectBucketRegistry`
- `SimulationStageWaveModule`
- `SimulationStageRenderModule`
- `BattleParitySnapshotModule`
- `BattleLogicObjectPointRuntime`
- lockstep snapshot/checksum modules
- ECS pass 和 writer 类型

`SimulationWorld.cs` 已声明“现存 partial 仅作为待迁移历史边界，不再新增”。本计划完成该迁移，而不是建立另一套并行架构。

## 4. 目标架构

### 4.1 聚合根

最终 `SimulationWorld` 是非 Mono、不可被其他模块直接拆解的聚合根：

```csharp
public class SimulationWorld
{
    private readonly SimulationRegistryModule registry;
    private readonly SimulationAiRuntime aiRuntime;
    private readonly SimulationPassPipeline passPipeline;
    private readonly SimulationStageRuntime stageRuntime;
    private readonly SimulationPresentationModule presentation;
    private readonly SimulationFrameInputModule frameInput;
    private readonly SimulationQueryAndLinkModule queryAndLinks;
    private readonly BattleLogicObjectPointRuntime objectPointRuntime;
    private readonly SimulationWorldDiagnostics diagnostics;
}
```

主类只保留：

- 构造与模块组合。
- `Runtime`、`Rng`、根 tick/context 等真正的世界根状态。
- 固定 pass 顶层入口。
- `BeginBattlePreparation/Shutdown/Reset` 的模块编排。
- 对外稳定 API 和迁移期间的兼容转发。

### 4.2 模块依赖规则

1. 子模块由 `SimulationWorld` 构造并持有，不使用可变 static singleton。
2. 子模块保持普通 C# 类型，不继承 `MonoBehaviour`。
3. 子模块不得调用创建型 `.Instance` 获取 Unity 服务。
4. 每个可变字段只能有一个 owner。
5. 模块优先接收具体、最小依赖；只有编排型模块可以临时持有 `SimulationWorld` façade。
6. 持有 `SimulationWorld` 的模块只能调用明确的 internal capability API，不能通过持续新增 `XxxForServices` 暴露整个 World。
7. hot path 不使用每 tick delegate、LINQ、反射或临时集合。
8. 模块间不形成双向状态写入。跨模块流程由 World 或上层 aggregate 编排。
9. public API 迁移采用转发 façade，调用者迁移和旧 API 删除分开实施。
10. 不在同一批次同时移动代码并重写算法。

### 4.3 生命周期合同

有状态模块按需要提供以下最小生命周期方法：

```csharp
void Reset();
void BeginBattlePreparation();
void BeginBattleShutdown();
bool TryShutdown(out string failureReason);
```

纯算法模块不实现空生命周期接口。关闭顺序仍由 `SimulationTickDriver` 和 `SimulationWorld` 按 `battle-runtime-ordered-shutdown-contract.md` 编排；子模块不能在自身 `Dispose` 中跨越阶段清理其他模块。

## 5. 模块所有权

### 5.1 SimulationRegistryModule

拥有：

- `RuntimeSlotTable`
- `RuntimeRestStore`
- `SimulationObjectBucketRegistry`
- stable id 分配
- register/unregister
- deferred unregister / slot release / destroy 边界
- structural event context 和 registry diagnostics
- registry reset/shutdown postconditions

提供：

```csharp
Register(ISimObject obj)
RequestUnregister(ISimObject obj)
TryResolve(RuntimeEntityHandle handle, out LF2Entity entity)
GetCurrentOccupant(int slot)
TryGetReadOnlySlot(int slot, out ReadOnlySlotView view)
BeginDeferredMutation()
FlushDeferredMutations()
Reset()
TryShutdownAndClear(...)
```

### 5.2 SimulationAiRuntime

AI 是一个 aggregate，不直接把三个巨型 partial 改成三个互相循环引用的类。

```text
SimulationAiRuntime
├─ SimulationAiSharedState
├─ SimulationAiInputModule
├─ SimulationAiSensingModule
└─ SimulationAiDecisionModule
```

SharedState 拥有：

- slot snapshot 和 generation/epoch
- snapshot validity / mutation version
- unified row identity
- AI 子阶段之间正式共享的只读产品

InputModule 拥有：

- AI input slot snapshot
- spatial indexes
- team HP summaries
- move-mode、nearest、candidate input products

SensingModule 拥有：

- SoA sensing rows
- role/team span
- sensing shadow comparison
- candidate sensing diagnostics

DecisionModule 拥有：

- decision snapshot/execution state
- indexed/shared/unified decision
- decision shadow comparison
- decision diagnostics 和 self-check injection state

AI 子阶段固定由 `SimulationAiRuntime` 编排，禁止子模块反向写入其他子模块 private state。

### 5.3 SimulationPassPipeline

只拥有 pass 顺序、pass scoped diagnostics 和 deferred mutation boundary。

具体规则按稳定行为域放入已有或新增 pass/module：

- `BattleCharacterInputPass`
- `BattleSerialPass`
- `BattleEarlyFrameAdvancePass`
- `BattleLateEntityLifecyclePass`
- `BattleDeathCleanupPass`
- `BattleOid5152RuntimeModule`
- `BattleRespawnModule`

不按“一方法一类”拆分；一个模块至少对应一个权威 pass 或一个具有独立状态/不变量的行为域。

### 5.4 Stage 与 Presentation

第一阶段保留已存在的 `SimulationStageWaveModule` 和 `SimulationStageRenderModule`。后续只在证据充分时把后者分为：

- `SimulationStageRuntimeModule`：bounds、walkable、stage snapshot。
- `SimulationPresentationModule`：presentation order、immutable publication、central/legacy handoff。

该拆分不得与中央渲染功能修改同时进行。

### 5.5 Diagnostics

最终统一入口：

```csharp
public SimulationWorldDiagnostics Diagnostics { get; }
```

迁移期间旧 `XxxForDiagnostics` 属性继续转发，不立即删除。Diagnostics 只读，不拥有 gameplay authority。

## 6. 实施阶段

### Phase 0：治理与基线冻结

1. 创建 Change Record、Task、Ledger、STATE 和 Handoff 条目。
2. 新增架构测试，禁止新增 `partial class SimulationWorld` 文件；迁移期间使用明确 allowlist，最终要求 0。
3. 记录当前 partial 文件、行数、允许列表和模块引用。
4. 运行编译和相关 focused baseline。
5. 记录当前完整 SelfCheck 的已知 Naruto DDA 阻塞，不能把它误判为本重构引入。

退出条件：文档和基线完整；未修改生产行为。

### Phase 1：组合根与已提取模块收口

1. 保持 `SimulationWorld` public API，确认 FrameInput、QueryAndLinks、StageWave、StageRender 只通过 module 转发。
2. 把空 compatibility partial 文件从编译实现中移除；文件删除/重命名需核对精确目标。
3. 为已有模块补齐明确 `Reset/Shutdown` 所有权，禁止主类重复持有同一状态。
4. 不改变 StageRender 的算法或 central ownership。

退出条件：已提取模块无重复实现；编译、frame input、query/link、stage、central focused 通过。

### Phase 2：Registry 与 RuntimeSlot 提取

分三个可独立验证的子批次：

1. `RuntimeSlotTable/RuntimeRestStore/ObjectBucketRegistry` 所有权转入 `SimulationRegistryModule`，World 保留转发属性。
2. register/unregister/deferred structural mutation 方法迁移。
3. reset/shutdown/structural diagnostics 迁移，World constructor 移回主文件或 composition helper。

退出条件：Registry partial 不再包含实现；slot generation、register order、deferred flush、shutdown 后置条件完全一致。

### Phase 3：AI Runtime 提取

按共享数据依赖顺序迁移：

1. 建立 `SimulationAiSharedState` 和 `SimulationAiRuntime`。
2. 迁移 AiInput private state/methods。
3. 迁移 SoA/Sensing private state/methods。
4. 迁移 Decision/Unified/Shadow private state/methods。
5. 把 World 上的 AI public/diagnostic API 改成转发。

每个子批次只做所有权移动，不改变算法表达、循环次序、RNG 或 fallback 默认值。

退出条件：三个 AI partial 无实现；AI focused、shadow parity、checksum 和 allocation 证据无差异。

### Phase 4：PassPipeline 提取

1. 建立 `SimulationPassPipeline`，先转发原 pass 方法。
2. 迁移顶层 pass 编排。
3. 将 OID 51/52、respawn、early/late lifecycle 等具体规则迁入对应 module/pass。
4. 把 self-check probe 类型移到测试或 diagnostics 支持文件，不留在 World 主类。

退出条件：Passes partial 无实现；权威 pass trace、OPoint 可见边界、input/RNG/checksum 无差异。

### Phase 5：移除 partial 和兼容 façade 收口

1. `SimulationWorld` 改为非 partial。
2. 精确删除已空的 historical partial 文件。
3. 架构测试要求 `partial class SimulationWorld` 搜索结果为 0。
4. 迁移 diagnostics callers 后删除无价值转发，但保留对外正式 API。
5. 根据真实继承使用决定是否 `sealed`；未取得零派生证据时不强制 sealed。

退出条件：源码中不存在 `partial class SimulationWorld`；主类只承担组合、生命周期、 façade 和顶层 pass 编排。

## 7. 每阶段验证矩阵

### 静态与编译

- Unity script compile 0 error。
- `git diff --check` 无 whitespace error。
- 不新增 `SimulationWorld.*.partial.cs`。
- 最终 `rg "partial class SimulationWorld" Assets/NTSD/Scripts` 返回 0。
- 不修改 `Gen/`、`Plugins/` 或用户无关改动。

### 相关 focused

- Registry/runtime slot/generation/structural lifecycle。
- FrameInput、QueryAndLinks、held object、positive link。
- AI input、AI sensing、AI decision、shadow/unified snapshot。
- pass order、frame advance、late lifecycle、OPoint。
- snapshot/checksum/restore。
- dedicated worker boundary。
- ordered shutdown。
- central presentation/materialization。

### 行为一致性

- 相同 seed、输入和 tick 的 checksum 相同。
- pass trace first-difference 为无差异。
- RNG state 和调用序列无差异。
- object count、stable id、slot、generation、frame、位置、速度、HP、link、holder、target 相同。
- worker/synchronous 结果一致。
- 每 tick allocation 不增加。

### 运行时

- 真实 `NTSD_Battle` 延迟角色生成正常。
- 两轮 Play/Stop 无 cleanup warning。
- factory/pool/boundary runtime carrier 无残留。
- Scene dirty 状态不变。

完整 `BattleRuntimeSelfCheck` 必须实际运行。若仍停在任务前已记录的 Naruto DDA 断言，只能报告阻塞；未经单独 authority 不得修改角色规则。

## 8. 风险控制

### 8.1 最大风险

- private 字段移动后初始化顺序改变。
- Reset 漏掉新 module state。
- pass 调用顺序或 deferred mutation scope 改变。
- AI snapshot epoch、cache invalidation 或 shadow comparison 时点改变。
- 通过 interface/delegate 引入 hot path 开销或 allocation。
- 模块通过 World façade 形成循环依赖，名义拆分但状态仍不清晰。
- 巨型机械 diff 覆盖用户未提交修改。

### 8.2 控制方式

- 每批次只迁一个状态所有权域。
- 先保留 World façade，再迁调用者；不同时删 API。
- 先复制调用图和测试，再移动实现。
- 不顺手格式化或重命名无关符号。
- 模块构造顺序显式写在一个 composition boundary。
- 所有 Reset/Shutdown 调用集中列出并做 focused assertion。
- hot path 使用具体引用和预分配容器；不使用 event bus。
- 每阶段独立 Change evidence，失败立即停在最近可编译阶段。

## 9. 回滚

- 每个 Phase 保持独立、可编译、可验证。
- 回滚只反向迁移当前 Phase 的 façade、字段所有权和方法，不使用 `git reset/restore/clean`。
- 保留已验证的 ordered shutdown、singleton teardown、central HP/editor preview 和所有用户未提交修改。
- 若迁移暴露 gameplay 差异，先恢复调用所有权，不修改 C++ authority 或角色资源来掩盖差异。

## 10. 完成定义

只有同时满足以下条件，才可将本计划标记完成：

1. `SimulationWorld` 不再使用 `partial`。
2. Registry、AI、PassPipeline 等状态具有单一 owner。
3. World 只负责组合、根生命周期、固定编排和对外 façade。
4. 子模块不解析 Mono singleton，不持有不受限的全局可变状态。
5. 所有相关 focused、checksum、worker、shutdown 和真实 Play 验证达到任务前基线或更高。
6. 当前已知任务外失败被明确隔离，不通过修改 gameplay 规则伪造绿色。
7. Change Ledger validator 通过，或仅被已记录的无关 Record 阻断并如实报告。

## 11. 实施蓝图的强制解释

本节开始是本计划的可执行架构合同。前文说明目标、风险和阶段；本节把每个
模块、主模块调整、依赖方向和逐批验收固定下来。后续实现若与本节冲突，必须先
更新文档和 Change Record，不能在代码中临时发明另一套边界。

### 11.1 “已拆分”的定义

一个职责只有同时满足以下条件，才可报告“已拆分”：

1. 它有独立普通 C# 类型；不得是 `partial class SimulationWorld`。
2. 生产实现位于独立 `.cs` 文件；不得把另一个顶层 module class 放在
   `SimulationWorld.cs` 末尾。
3. 该模块拥有自己的可变状态、scratch/cache 和算法。
4. `SimulationWorld` 只保留 readonly 引用、构造、生命周期编排和兼容转发。
5. World 不保留第二份同义字段、算法副本或 shadow owner。
6. 模块不解析 Mono singleton，不直接创建 GameObject，不读写 Transform 真值。
7. 对应编译、focused、checksum/pass-order 或运行时证据达到迁移前基线。

仅删除 `partial`、仅改文件名、把实现合回 World、或新增一个空 module wrapper，
都不算拆分完成。

### 11.2 当前实施事实

截至 2026-09-01 的实际状态：

| 项目 | 观察值 | 判断 |
|---|---:|---|
| `SimulationWorld.cs` | 19,045 行 | 仍是迁移债务，不能交付为最终架构 |
| `partial class SimulationWorld` | 0 | 硬性关键字债务已清零 |
| `SimulationWorld.*.partial.cs` | 0 | 历史文件已清零 |
| `SimulationRegistryModule.cs` | 1,122 行 | 核心 slot/registry 已独立，root lifecycle 余量待收口 |
| `SimulationAiRuntime` + 三个 AI module | 已存在 | 首批状态 owner 已迁，算法主体仍在 World |
| `BattleOid5152RuntimeModule.cs` | 305 行 | 物理抽离，validation compile 通过，Editor focused 待刷新后执行 |
| `BattleRespawnModule.cs` | 227 行 | 物理抽离，validation compile 通过，Editor focused 待刷新后执行 |
| StageWave/StageRender | 独立文件 | 保持现有算法，不与本次中央渲染改造混做 |

## 12. 最终依赖图

```text
SimulationTickDriver / NTSDBattleTickSystem
                    |
                    v
             SimulationWorld
        (composition + lifecycle + façade)
                    |
        +-----------+-----------+---------------------+
        |           |           |                     |
        v           v           v                     v
  Registry      PassPipeline  AiRuntime             StageRuntime
        |           |           |                     |
        |           |     +-----+------+              |
        |           |     |     |      |              |
        |           |   Input Sensing Decision        |
        |           |                                  |
        +-----------+------------------+---------------+
                    |
                    v
        Snapshot / Checksum / Presentation publication
```

依赖规则：

- 上层可以调用下层；下层不得持有或调用 `SimulationTickDriver`。
- Registry、AI、Pass 子模块之间不得互相写 private state。
- 跨模块事务由 World 或 `SimulationPassPipeline` 编排。
- 只有编排型 module 可临时持有 `SimulationWorld`；业务 module 优先接收最小依赖。
- 任何 `World.XxxForModule` capability 必须属于下文列出的 capability 类别；不得用
  `XxxForServices` 不断暴露整个 World。

## 13. 主模块 `SimulationWorld` 的最终合同

### 13.1 最终保留

`SimulationWorld` 只允许保留：

- 构造函数和模块创建顺序。
- `BattleRuntimeState Runtime`、`DeterministicRng Rng`、`SimContext Context`。
- `ILF2SceneQuery`、`INTSDItrKindService` 等世界根服务引用。
- `BeginBattlePreparation`、`BeginBattleShutdown`、`Reset`、`ClearAll` 的模块编排。
- 固定顶层 pass 入口；入口本身只能转发，不包含实体循环和规则分支。
- 对外稳定 public/internal façade；迁移结束后删除无调用者的临时 façade。
- snapshot/checksum/restore 等已经独立 module 的组合引用。

### 13.2 必须移出

- 实体 slot 遍历、register/unregister、pending destroy 算法。
- AI snapshot、空间索引、team summary、nearest、sensing、decision 算法。
- early/late frame、death/respawn、OID 51/52、interaction、random weapon 规则。
- pass 专用 scratch list、dictionary、array、mode、counter 和 mismatch state。
- self-check probe 实体、Editor-only nested test classes。
- 只属于某一模块的 diagnostics mutable state。

### 13.3 最终字段形态

```csharp
public class SimulationWorld
{
    private readonly SimulationRegistryModule registry;
    private readonly SimulationPassPipeline passPipeline;
    private readonly SimulationAiRuntime aiRuntime;
    private readonly SimulationStageWaveModule stageWave;
    private readonly SimulationStageRenderModule stageRender;
    private readonly SimulationFrameInputModule frameInput;
    private readonly SimulationQueryAndLinkModule queryAndLinks;
    private readonly BattleLogicObjectPointRuntime objectPoints;
    private readonly SimulationWorldDiagnostics diagnostics;

    public BattleRuntimeState Runtime { get; private set; }
    public DeterministicRng Rng { get; private set; }
    public SimContext Context { get; }
}
```

已有 snapshot、checksum、ECS writer 和 presentation publication 引用可以继续由
World 组合，但不得在 World 中重新实现它们的算法。

### 13.4 World capability 白名单

迁移期间 module 可调用的 World internal capability 只允许以下类别：

| 类别 | 示例 | 约束 |
|---|---|---|
| Registry read | resolve handle、active/dormant slot read | 不允许绕过 generation |
| Deferred mutation | begin/end deferred pass | 必须成对，异常路径 finally 关闭 |
| Snapshot publication | refresh one entity、invalidate row membership | 不反写 Transform |
| Root services | Runtime、Rng、SceneQuery、data catalog | 只读引用；写入由 owner API 完成 |
| ObjectPoint seam | factory/reference-pool operation | 遵守 shutdown spawn gate |
| Diagnostics hook | 明确命名的 self-check override | 不参与生产 authority |

每新增一个 capability，必须在 Change Record 中说明调用者、不可变条件和后续删除
计划。禁止新增返回整个 Registry/AI/Pass mutable object graph 的万能属性。

## 14. 子模块逐项合同

### 14.1 `SimulationRegistryModule`

**文件：** `SimulationRegistryModule.cs`

**唯一拥有：**

- `RuntimeSlotTable`、`RuntimeRestStore`、`SimulationObjectBucketRegistry`。
- stable id、slot allocation ticket、generation、profile/capacity。
- pending unregister、pending slot release、pending destroy 流程。
- registry structural event context、reject counters、witness cursor。

**提供：** register、unregister、resolve、active snapshot、deferred flush、reset、
shutdown postcondition。

**不得拥有：** AI snapshot、frame pass、Unity renderer、Stage wave。

**World 调整：** 删除 registry 算法和同义字段；保留 public façade 与 root shutdown
调用。Registry 不再以 World private list 作为第二 owner。

### 14.2 `SimulationPassPipeline`

**文件：** `SimulationPassPipeline.cs`

**唯一拥有：**

- 权威 pass 顺序和 pass-scoped diagnostics。
- deferred mutation scope 的顶层编排。
- 下列业务 pass module 的引用。

```text
SimulationPassPipeline
├─ BattleCharacterInputPass（复用已有 ECS pass/writer）
├─ BattleSerialPass
├─ BattleEarlyFrameAdvanceModule
├─ BattleLateEntityLifecycleModule
├─ BattleInteractionPipeline
├─ BattleRandomWeaponDropModule
├─ BattleOid5152RuntimeModule
└─ BattleRespawnModule
```

**不得拥有：** Registry slot table、AI row state、Stage mutable state。

**World 调整：** `PostCooldownInputAll`、`SerialTickAll`、`LateEntityUpdateAll` 等 public
入口只转发给 pipeline；World 中不再出现对应实体循环。

### 14.3 `BattleOid5152RuntimeModule`

**文件：** `BattleOid5152RuntimeModule.cs`

**拥有：** OID 7/8↔51 timer、merge/split、HP gate、partner reset 和 dormant
membership 变化。

**依赖：** Registry slot read、RuntimeCharacterConfigResolver、single-entity snapshot
refresh、AI row membership invalidation。

**不变量：** 0..19 扫描顺序、4500/900 timer、HP clamp/odd truncate、frame gate、
partner reset 字段顺序不变。

**当前状态：** 代码已物理抽离；当前 Unity 已完成 architecture `4/4`、OID `7/7`、
Respawn `4/4`，M1 合计 `15/15 PASS`。

### 14.4 `BattleRespawnModule`

**文件：** `BattleRespawnModule.cs`

**拥有：** death gate、respawn scratch、两种 respawn 分支、重生点 RNG、OID998
immediate effect。

**依赖：** active entity snapshot、active check、Runtime snapshot refresh、OPoint
factory/reference pool、diagnostic override。

**不变量：** active slot 顺序、两次 RNG 调用顺序、frame 212/219、HP/PP 写入和
OID998 task 字段不变。

**当前状态：** 代码已物理抽离；focused 待 Unity Refresh。

### 14.5 `BattleEarlyFrameAdvanceModule`

**目标文件：** `BattleEarlyFrameAdvanceModule.cs`

**迁入：** `EarlyFrameAdvanceSpecialsAll`、state500/state501 handle snapshot、验证、
resolve 和 special 执行。

**拥有：** state500/state501 handle scratch 与对应 diagnostics。

**依赖：** Registry handle resolve、single-entity refresh、边界/对象生成 capability。

**验收：** early-state focused、frame-advance snapshot、OPoint visible boundary、worker。

### 14.6 `BattleLateEntityLifecycleModule`

**目标文件：** `BattleLateEntityLifecycleModule.cs`

**迁入：** `LateEntityUpdateAll`、late state special、state9996 children、late OPoint
flush、death OPoint、cleanup、late runtime snapshot boundary。

**拥有：** late handle/scratch、no-op diagnostics、late flush counters。

**依赖：** Registry deferred mutation、ObjectPoint runtime、snapshot refresh、data
catalog；不得直接创建 Mono singleton。

**验收：** late lifecycle、OPoint、weapon depletion、worker、ordered shutdown。

### 14.7 `BattleInteractionPipeline`

**目标文件：** `BattleInteractionPipeline.cs`

**迁入：** collision candidate consume boundary、pre/post/object interaction、empty
participant proof、cpoint/mismatch/held sync proof。

**拥有：** interaction diagnostics 和 participant scratch。

**依赖：** SceneQuery、ECS hit plan/writers、Registry active view。

**验收：** collision、hit execution、cpoint/held-link、structural witness、checksum。

### 14.8 `BattleRandomWeaponDropModule`

**目标文件：** `BattleRandomWeaponDropModule.cs`

**迁入：** normal drop tick、mode2 tail、free-slot selection、spawn and cooldown reset。

**拥有：** random-weapon buffer、drop diagnostics；现有
`SimulationRandomWeaponDropBuffer` 可作为其内部存储，不再由 World 直接持有。

**依赖：** Rng、Registry slots、ObjectPoint factory、Stage bounds。

**验收：** RNG state/call order、random weapon focused、parity checksum。

### 14.9 `SimulationAiRuntime`

**文件：** `SimulationAiRuntime.cs`

**角色：** AI aggregate 和固定 Input→Sensing→Decision 子阶段编排。

**唯一拥有：** `SimulationAiSharedState` 和三个 AI module；World 只持有一个
`SimulationAiRuntime` 引用，不直接持有 AI arrays/dictionaries/modes。

**生命周期：** `Reset` 清全部 cache/epoch/mismatch；shutdown 不访问 Mono 服务。

### 14.10 `SimulationAiSharedState`

**目标文件：** `SimulationAiSharedState.cs`

**拥有：** slot identity/generation/epoch、snapshot validity、unified row identity、
mutation version 和三个 AI 阶段正式共享的只读产品。

**规则：** Input 写入并发布；Sensing/Decision 只读。失效由 Runtime aggregate 编排，
不能由 Decision 反向修改 Input private state。

### 14.11 `SimulationAiInputModule`

**文件：** `SimulationAiInputModule.cs`

**迁入：** Build/Clear slot snapshot、spatial indexes、ground-team partitions、air role、
team HP、move-mode、nearest candidate、基础 AI input preparation。

**拥有：** 当前已迁 arrays/lists/broadphase 加仍留在 World 的 ground partition、team
summary、nearest facts 和 input diagnostics。

**不得拥有：** sensing rows、decision execution state。

**验收：** move-mode、nearest、air/ground role、team partition、allocation、live-slot。

### 14.12 `SimulationAiSensingModule`

**文件：** `SimulationAiSensingModule.cs`

**迁入：** sensing rows build/validate、candidate sensing、shadow compare、snapshot
identity/generation drift、remainder boundary flags。

**拥有：** `AiSoASensingRows`、expected result、mode、epoch/validity、mismatch state。

**不得拥有：** Decision execution/mutation witness。

**验收：** sensing focused、candidate parity、identity/generation invalidation、0 allocation。

### 14.13 `SimulationAiDecisionModule`

**文件：** `SimulationAiDecisionModule.cs`

**迁入：** decision snapshot、legacy/indexed/shared/unified execution、shadow oracle、
mutation witness、decision diagnostics 和 self-check injection。

**拥有：** `AiUnifiedSnapshotExecutionState`、legacy fallback snapshot、decision modes、
oracle interval、mismatch counters。

**不得拥有：** Input spatial indexes 或 Registry slot table。

**验收：** decision fixture、known position38 baseline isolation、unified checksum、worker。

### 14.14 Stage、Snapshot、Presentation、Diagnostics

- `SimulationStageWaveModule`：只拥有 stage wave/progression/spawn buffers。
- `SimulationStageRenderModule`：保持现有 stage render/presentation ordering；本次不改
  central renderer 或 health bar。
- snapshot/checksum/restore modules：继续独立；World 只转发。
- `SimulationWorldDiagnostics`：最终汇总只读视图；mutable counter 仍由产生它的模块
  拥有。
- `SimulationWorldHooks`：仅测试/诊断 override；不得成为 production service locator。

## 15. 构造、Reset 与 Shutdown 顺序

### 15.1 构造顺序

固定顺序：

1. 创建 Registry 与 capacity/root storage。
2. 创建 root Runtime data/catalog/reference pool。
3. 创建 FrameInput、Query/Links、ObjectPoint。
4. 创建 AI Runtime。
5. 创建各业务 Pass module 与 PassPipeline。
6. 创建 Stage modules。
7. 创建 snapshot/checksum/restore modules。
8. 创建 diagnostics/hooks。
9. 初始化 Runtime、Rng、SceneQuery、Context。

构造函数不得执行 battle tick、解析 Mono singleton 或创建 GameObject。

### 15.2 Reset 顺序

```text
Stop pass publication
→ Registry clear deferred mutations
→ ObjectPoint reset
→ AI Runtime reset
→ Pass modules reset diagnostics/scratch
→ Stage reset
→ Snapshot/publication reset
→ Runtime scalar reset
```

### 15.3 Ordered shutdown 映射

继续服从 `battle-runtime-ordered-shutdown-contract.md`：

| shutdown 阶段 | World/module 动作 |
|---|---|
| 禁止 tick/input | TickDriver；World 不再接受 pass façade |
| 停 worker | TickDriver join |
| 关 spawn | ObjectPoint + Structural writer |
| unseal | allocation owner，禁止解析 singleton |
| 清 publication | Presentation/snapshot publication |
| discard OPoint | ObjectPoint runtime |
| renderer recycle | Mono presentation owner |
| logic entity clear | Registry + Pass modules |
| unbind World | TickDriver/App owner |
| pool quiesce | Mono pool owner |
| boundary cleanup | Stage/runtime boundary owner |

子模块不得在 `Dispose` 或 finalizer 中跨阶段调用其他模块。

## 16. 物理文件与代码守卫

最终必须满足：

- `SimulationWorld.cs` 只声明 `SimulationWorld` 和真正属于根 API 的小型 value/interface；
  不声明其他 behavior module class。
- 每个上文命名 module 有独立 `.cs` 和 `.meta`。
- `rg "partial class SimulationWorld"` 为 0。
- `SimulationWorld.*.partial.cs` 为 0。
- 架构测试反射确认 World 对正式 module 使用 `private readonly` 字段。
- 架构测试扫描确认 `SimulationWorld.cs` 不包含
  `BattleOid5152RuntimeModule`、`BattleRespawnModule` 等 module 声明。
- 最终 `SimulationWorld.cs` 目标不超过 2,500 行；该数值是架构报警线，不代替
  职责审阅。超过时必须在 Change Record 解释剩余根职责。
- 不以 partial、继承 God Object、source generator 万能 façade 或 `dynamic` 转发
  绕过组合边界。

## 17. 执行批次与停止条件

| 批次 | 物理改动 | 必跑验证 | 失败动作 |
|---|---|---|---|
| M0 | 文档、守卫、inventory | Editor compile、architecture | 停止，不搬算法 |
| M1 | OID5152 + Respawn | OID/respawn focused、compile | 回退本批 façade/文件 |
| M2 | EarlyFrameAdvance | early/frame snapshot/OPoint | 停止后恢复 M2 |
| M3 | LateLifecycle | late/weapon/OPoint/worker/shutdown | 停止后恢复 M3 |
| M4 | InteractionPipeline | collision/hit/cpoint/held/checksum | 停止后恢复 M4 |
| M5 | RandomWeapon + PassPipeline | RNG/parity/pass order | 停止后恢复 M5 |
| M6 | Registry remainder | slot/generation/structural/shutdown | 停止后恢复 M6 |
| M7 | AI Input | move/nearest/role/team/allocation | 停止后恢复 M7 |
| M8 | AI Sensing | sensing/candidate/shadow | 停止后恢复 M8 |
| M9 | AI Decision | decision/unified/worker/checksum | 停止后恢复 M9 |
| M10 | World cleanup | full matrix、Play/Stop、SelfCheck | 保持 IN_PROGRESS |

通用停止条件：

- 新增 compile error。
- pass trace/checksum/RNG first difference。
- slot/generation/OPoint 可见 tick 变化。
- 每 tick allocation 增加且无法证明是测试噪声。
- shutdown cleanup warning 或 Scene dirty。
- 需要修改 C++ authority、DAT、Scene、Prefab、中央渲染或角色规则才能继续。

## 18. 当前下一步

1. M1～M8 已逐批通过 Unity 门禁；M7 AI Input 为 `112/112 PASS`，M8 AI
   Sensing 最终 job `7b942f2152fb4cf8a656e61731111a8a` 为 `110/110 PASS`。
2. M9 `SimulationAiDecisionModule` 已完成：decision snapshot、legacy/indexed/shared/
   unified execution、shadow oracle、mutation witness、diagnostics 与 self-check injection
   均由 Decision 模块拥有；World 只保留跨模块 capability 和兼容 façade。
3. M9 完整组合 job `2fab77983798482dbf1985ff424d24cc` 执行 213 项，除预存
   position38 baseline 外其余 212 项通过；单项 job
   `0b2c3e88d5ae4da592911311353dc457` 1/1 精确复现同一基线，证明无新增
   decision/unified/worker/checksum/architecture 差异。
4. M10 代码清理与定向运行时已完成：runtime/editor compile0；AI 158/158、
   worker/checksum/shutdown/architecture 35/35、三项陈旧owner-path 3/3；两轮干净
   Play/Stop无目标cleanup warning且Scene不脏。`SimulationWorld.cs` 为6040行，超过
   2500报警线的剩余根职责已在Change Record解释；partial与历史partial文件均为0。
5. full EditMode job `4d26dc2aaed44165807b5da87b4714cf` 已完整执行1763项，仍被
   position38、package version、Blood/Catch static guard和并行S0 WPoint既有基线阻塞；
   fresh完整SelfCheck停在任务外central-render P4断言。按M10停止条件保持
   `IN_PROGRESS`，对应独立Change关闭这些基线并重跑前不得报告整个模块化计划完成。
