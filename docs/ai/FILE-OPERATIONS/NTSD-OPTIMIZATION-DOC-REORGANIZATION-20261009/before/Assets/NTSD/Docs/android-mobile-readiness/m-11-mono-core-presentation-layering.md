# M-11 Mono Host、Simulation Core 与 Presentation 分层方案

> 优先级：中；PERF Step B 深化按专项前置推进，不把完整L1当局部容量/资源优化或现状测量的统一阻断项。
> 状态：`OPEN / IMPLEMENTATION_NOT_STARTED / USER_HOLD / WAITING_USER_APPROVAL`
> 最后更新：2026-10-06
> 本轮共同合同与启动门：[2026-10-06复核](../battle-optimization-rebaseline-and-start-gates-20261006.md)；本轮未运行本项测试/测量，实施待用户批准。
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
5. B6前完成worker线程相关Unity API与Debug硬门；B7只纯值/数学L2，B8再asmdef，B9最后清理compat façade。沿用专项B0-B9，不提前大规模public化。
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
| Architecture guards | 全Simulation源码、逐批退出 | B0具名债务baseline且不新增；目标批次清零，不用宽泛allowlist掩盖债务 |
| Lockstep/checksum | 同 seed/input/tick | 无 first difference |
| Worker guard | logic world + presentation binding 场景 | 线程资格与 fail-close 符合合同 |
| Generation/no-ghost | slot reuse、迟到 ack | 新 occupant 不被旧绑定影响 |
| Central/Legacy 对照 | actor/weapon/effect/shadow | publication 与表现不变 |
| Shutdown | 两轮 Play→Stop→re-enter | worker join、pool quiesced、零残留 |
| 性能 | 1000 AI 正式 workload | 0GC，P95/P99 不回退 |

## 证据与留痕

- 当前证据：`SimulationWorld.cs:988-989`、`SimulationRegistryModule.cs:596-624,952-953,1190-1191`、`SimulationStageWaveModule.cs:551,643`、`BattleStateSnapshotRestore.cs:470-471`、`BattleLockstepSession.cs:7-16`、`BattleManagedMemoryBoundary.cs:411-437`。
- 专项§19.7.5已于2026-09-14落地P-1…P-5、33ms/3ms及Debug硬门正文；不是仍待落地，也不是代码已改。2026-10-06恢复前必须B0重扫当前projection/interpolation/audio/publication/关停与反向依赖，不复用旧行号。
- 2026-09-06：独立方案文档建立；用户尚未明确解除专项计划 `USER_HOLD`，未修改实现。
