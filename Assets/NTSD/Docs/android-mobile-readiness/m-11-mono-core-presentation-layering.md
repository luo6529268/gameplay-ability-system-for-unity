# M-11 Mono Host、Simulation Core 与 Presentation 分层方案

> 优先级：中；在 1000 AI worker 与中央渲染最终认证前必须完成 L1  
> 状态：`OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED / USER_HOLD`  
> 最后更新：2026-09-06  
> 专项计划：[Simulation Mono / 非 Mono 边界整理与重构计划](../simulation-mono-nonmono-boundary-refactor-plan.md)  
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

目录已区分 Host/Core/Runtime/Presentation/Lockstep，但当前 Core、Registry、Stage、Snapshot 和 Lockstep 仍引用 Renderer、MountRegistry 或具体 Mono Driver。Mono 标签本身不决定性能；本项解决依赖方向、线程安全、资源生命周期和可验证性，不直接替代 AI/Broadphase/Mesh 优化。

## 解决方案

1. 只先完成 L1：Core 不继承 Mono，不访问 GameObject、Transform、Renderer、Sprite、Texture、MountRegistry、Unity pool、Scene、资源加载器或创建型 Mono singleton。
2. Mono Host 负责 Update/Scene、输入采集、worker 启停、异步资源 Lease 和十一阶段 shutdown。
3. Core 只消费纯值输入并发布预分配 immutable presentation command/frame；handle 必须携带 generation。
4. Presentation 维护 `RuntimeEntityHandle → PresentationBinding`，负责中央目录、纹理、mount、回收与 generation-aware detach ack，不反写逻辑真值。
5. Lockstep 依赖最小 tick/frame execution port，不持有 `SimulationTickDriver`。
6. 源码依赖单向后再加 asmdef；L2 的 `Vector2/Vector3/Mathf` 全面替换延后评估。

## 实施步骤

1. B0：重新 inventory 并建立 Mono ownership、forbidden Unity Object、dependency direction、state authority guards。
2. B1/B2：Tick Host port；managed-memory Mono probes 移至 Host/Diagnostics。
3. B3/B4：建立 command/ack/binding table；Registry/World 去 Renderer/Sprite/MountRegistry。
4. B5/B6：收口 LF2Entity 兼容表现引用；分离 Core publication 与 Unity dispatch。
5. B7～B9：诊断/数学 inventory、asmdef 单向强制、最后清理 compatibility façade。
6. 每个批次独立 Task/Change；不得与 H-08、Legacy Sprite 删除或 H-06 算法切换合并。

## 验收条件

- Core/Runtime/Pass/AI/ECS/Lockstep 不声明 MonoBehaviour，也不引用禁止的 Unity Object/表现服务。
- World、Registry、Stage logic 和 Snapshot Restore 不直接操作 `entity.Renderer` 或 MountRegistry。
- Lockstep 不依赖具体 Driver；dedicated worker 只接触纯逻辑与预分配纯值数据。
- Presentation 只消费 immutable publication；旧 generation ack 不能解绑新 occupant。
- pass order、双 RNG、slot/generation、OPoint、checksum、snapshot/restore 与迁移前基线一致。
- warmup 后 `0 B/tick`、1000 AI 不低于可追溯基线，十一阶段 shutdown 与两轮重进零残留。

## 测试条件

| 测试 | 条件 | 通过标准 |
|---|---|---|
| Architecture guards | 全 Simulation 源码 | 禁止依赖为 0，allowlist 明确 |
| Lockstep/checksum | 同 seed/input/tick | 无 first difference |
| Worker guard | logic world + presentation binding 场景 | 线程资格与 fail-close 符合合同 |
| Generation/no-ghost | slot reuse、迟到 ack | 新 occupant 不被旧绑定影响 |
| Central/Legacy 对照 | actor/weapon/effect/shadow | publication 与表现不变 |
| Shutdown | 两轮 Play→Stop→re-enter | worker join、pool quiesced、零残留 |
| 性能 | 1000 AI 正式 workload | 0GC，P95/P99 不回退 |

## 证据与留痕

- 当前证据：`SimulationWorld.cs:988-989`、`SimulationRegistryModule.cs:596-624,952-953,1190-1191`、`SimulationStageWaveModule.cs:551,643`、`BattleStateSnapshotRestore.cs:470-471`、`BattleLockstepSession.cs:7-16`、`BattleManagedMemoryBoundary.cs:411-437`。
- 实施前必须修正专项计划正文残留的旧“固定 30 Hz”为当前正常 `33 ms`、F5 `3 ms` cadence 合同。
- 2026-09-06：独立方案文档建立；用户尚未明确解除专项计划 `USER_HOLD`，未修改实现。
