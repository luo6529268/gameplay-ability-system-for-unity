# DATA-01C / BATTLE-AUDIT3-07 诊断（2026-07-18）

## 结论

首个失败不是因为 C# 的 `EmptyFrame` 语义错误，也不是 `CheckReleaseTickCpointSyncPrecedesCandidates` fixture 没有提供候选几何。`DAT-01C` 之后 Unity 的 `LF2FrameCache.GetFrameDataById` 已改成与权威 C# 相同的“合法但未定义 frame id 返回共享 `EmptyFrame`、越界返回 null”契约；但生产调用链仍有把 `GetFrameDataById(...) == null` 当作“缺帧”的旧判断。该 API 契约迁移不完整，会让合法缺帧进入 frame 写入、碰撞快照或候选路径，因而可以造成 `candidateContainsExpected=false` 这类结果。

在本 self-check 的具体 fixture 中，catcher/victim/target 使用的 frame id 均在 fixture 中明确存在：catcher `100`、victim `130/131`、target `0`。因此 fixture 的候选几何本身是有效的；失败不能用“测试目标缺少 bdy/itr”解释。最小修复应优先关闭生产侧 null-contract 迁移缺口，再重新运行该断言。

## 证据链

| 位置 | 观察 |
|---|---|
| 权威 C# `src/Data/DatModels.cs:148-197` | `GetFrameOrNull`：越界返回 null；合法但 `FrameIndex==-1` 返回共享 `EmptyFrame`；`HasFrame` 单独表示 authored frame 是否存在。 |
| Unity `Assets/NTSD/Scripts/Animation/Character/LF2FrameCache.cs:59-68` | 已复制上述语义：越界 null、合法缺帧 `EmptyFrame`、`HasFrame` 区分存在性。此改动方向正确。 |
| Unity `LF2Entity.cs:906-920` | `ImmediateFrame` 仍以 `targetFrame == null` 判定目标帧缺失。对合法缺帧，该判断现在为 false，可能把 `EmptyFrame` 写入 `Frame.D`。 |
| Unity `LF2Entity.cs:4790-4800` | `SetCpointRawFramePreserveWait` 无 `HasFrame(frameId)` 守卫，同样会把 `EmptyFrame` 写入 cpoint frame。 |
| Unity `LF2Entity.cs:4076-4079` | `GetCollisionFrameData` 只看 `Prev2D/D`；一旦前置错误写入 `EmptyFrame`，碰撞快照会得到非 null、但无 bodies/itrs 的帧。 |
| Unity `BruteForceSceneQuery.cs:334-355` | immediate query 只检查 `targetCurrentFrame == null`，不能识别合法缺帧的 `EmptyFrame`；candidate collect 同类路径依赖 `HasAnyReleaseBody`，但调用链中仍存在旧 null 语义。 |
| Unity `BattleRuntimeSelfCheck.cs:3380-3445` | BATTLE-AUDIT3-07 的 catcher/victim/target frame fixture：catcher 当前 100，victim 初始 130、cpoint vaction=131，target 当前 0；victim frame131 追加 kind0 itr，target frame0 明确追加 kind0 body，几何位置完全重叠。 |
| Unity `NTSDBattleTickSystem.cs:17-50`、`SimulationWorld.Passes.partial.cs:1011-1060,712-740` | 顺序仍是 cpoint/mismatch/held sync -> collision snapshot -> collect -> post-interaction；测试断言检查的是同 tick 候选可见性，而非旧的 post-candidate 顺序。 |

## 为什么不是旧测试顺序问题

该断言在 `RunReleaseTick(1)` 后、`SimPostInteraction` 内读取 `_candidateCache`。candidate cache 在 `CaptureCollisionFrameSnapshotsAll` 后由 `CollectCollisionCandidatesAll` 建立，并且 `EndCollisionCandidateConsumption` 要到 `ResolvePostInteractions` 完成后才清理。因此 self-check 读取时机符合 Unity 当前 pass 顺序；仅仅把断言移到别的 post pass 不能修复候选缺失。

## 最小修复建议（本轮未修改代码）

1. 在所有“跳帧/直接写帧”的生产入口先调用 `FrameCache.HasFrame(frameId)`，再读取 `GetFrameDataById`：至少覆盖 `LF2Entity.ImmediateFrame` 和 `SetCpointRawFramePreserveWait`。越界仍由 `GetFrameDataById` 的 null 语义处理。
2. 对需要“实体当前 authored frame 才能参与碰撞”的入口，增加显式 authored-frame gate（`FrameCache.HasFrame(Frame.N/Prev2)`），不要用 `Frame.D != null` 代替。优先核对 `BruteForceSceneQuery` 的 immediate/candidate collect 入口和 `GetCollisionFrameData` 的调用者。
3. 不要把 `GetFrameDataById` 改回“合法缺帧返回 null”，那会重新偏离 C# authority，并使 `HasFrame` 失去职责。
4. 先按以上契约修正后重新 fresh compile/self-check；若 BATTLE-AUDIT3-07 仍失败，再在 fixture 中打印 `victim.Frame.N`, `victim.Frame.D.frameId`, `victim.Frame.Prev2D.frameId`, `target.Frame.D.frameId`, `target.Frame.Prev2D.frameId`, `victim.Runtime.HitCandidateCount` 和 cache target slot，区分生产候选收集与 `SceneQueryHit.Target` identity 问题。

## 状态

- 生产代码：未修改（只读诊断）。
- 测试代码：未修改。
- 诊断性质：确认 `DATA-01C` API 迁移存在生产级 null-contract 风险；BATTLE-AUDIT3-07 的 fixture 几何契约有效，不能以 fixture 错误结案。
