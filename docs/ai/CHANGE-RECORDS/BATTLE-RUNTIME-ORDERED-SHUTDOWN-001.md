# BATTLE-RUNTIME-ORDERED-SHUTDOWN-001 — Ordered battle runtime shutdown transaction

<!-- CHANGE-RECORD
id: BATTLE-RUNTIME-ORDERED-SHUTDOWN-001
status: BLOCKED
code-path: Assets/NTSD/Scripts/Simulation/BattleRuntimeLifecycle.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleRuntimeAllocationGate.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleLogicObjectPointRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleStructuralWriter.cs
code-path: Assets/NTSD/Scripts/Animation/Character/LF2ObjectPointFactory.cs
code-path: Assets/NTSD/Scripts/Animation/LF2ObjectPool.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2ObjectRenderer.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs
code-path: Assets/NTSD/Scripts/App/AppManager.cs
code-path: Assets/NTSD/Scripts/App/BattleBootstrap.cs
code-path: Assets/NTSD/Scripts/App/Editor/BattleRuntimeEditorShutdownBridge.cs
code-path: Assets/NTSD/Scripts/LevelEditor/BoundaryWallManager.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleRuntimeOrderedShutdownEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: USER-APPROVED-BATTLE-RUNTIME-ORDERED-SHUTDOWN-CONTRACT-2026-09-01; Assets/NTSD/Docs/battle-runtime-ordered-shutdown-contract.md; Unity lifecycle ownership
evidence: COMPILE_0 / FOCUSED_SHUTDOWN_4_4 / WORKER_20_20 / OPOINT_8_8 / CENTRAL_13_13 / SINGLETON_2_2 / LIVE_TWO_CYCLE_CLEAN / FULL_SELFCHECK_BLOCKED_BY_NARUTO_DDA
-->

> 创建日期：2026-09-01
>
> 当前状态：`BLOCKED / CODE_WRITTEN / COMPILE_0 / FOCUSED_AND_LIVE_PASS / FULL_SELFCHECK_UNRELATED_FAILURE`

## 1. 用户要求与权威边界

- 用户明确批准按 `Assets/NTSD/Docs/battle-runtime-ordered-shutdown-contract.md` 实施完整 `Running → Stopping → Stopped` 关闭事务。
- 本 Change 只规定 Unity-native 生命周期、停止门禁、资源归还和 Scene teardown 所有权；不改变 C++ release live battle pass、固定 30 Hz、输入消费时点、OPoint 正常生成语义、碰撞/命中、HP/PP、checksum 或 Running 状态可观察结果。
- `BATTLE-SCENE-TEARDOWN-SINGLETON-001` 已验证 allocation unseal 不再自动创建 factory/pool；本 Change 在其上建立完整顺序，不覆盖或删除该历史修复。

## 2. 当前原状

- `SimulationTickDriver.OnSingletonDestroyed()` 目前只执行 allocation unseal、worker stop 和 World unbind；没有统一 lifecycle state，也没有证明 presentation、pending OPoint、renderer、logic-only entity、pool 和 boundary carrier 按顺序清零。
- App/Scene、Driver、factory、pool、renderer 和 boundary cleanup 分散在 Unity 回调与 singleton fallback 中；Unity 不保证跨 GameObject 的销毁顺序。
- dedicated worker、logic-only runtime、central publication 和 Unity object pool 已存在，因此 teardown 必须先停 worker，再关闭 spawn/publication，最后清 World/pool/map carrier。
- 工作区存在大量既有未提交改动；本 Change 必须在现状上做最小增量，不回退、移动、格式化或覆盖其他 Change 的内容。

## 3. 强制关闭顺序

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

## 4. 计划改动

1. 新增生命周期状态、shutdown result/diagnostics 和幂等重入保护；`ignorePaused` 不得绕过 `Stopping/Stopped`。
2. `SimulationTickDriver` 成为阶段 1～10 的单一 runtime transaction owner；worker stop/join 是 hard gate。
3. Core/Unity OPoint 入口在 `Stopping` 拒绝新 spawn，但保留 unregister/free/destroy；pending task 只 discard/recycle，禁止 Flush materialization。
4. allocation unseal 只使用当前 battle 已捕获或非创建引用；不得在 teardown 创建 singleton。
5. 清空 central publication/submission 和 sound/presentation cache，阻止 LateUpdate 重新发布。
6. World 仍有效时回收所有 active Renderer；然后清理 logic-only entity、registry、slot 和 pending structural state，验证后再 unbind World。
7. LF2ObjectPool 进入 quiesced：active borrower 为零、对象 inactive、拒绝新 Get、无旧 World/Entity binding；不在 shutdown 中无条件批量 Destroy。
8. App/Bootstrap 在 Driver runtime 完成后清理 boundary carrier，并且只有 hard postconditions 通过才允许标记 Stopped/正常 Scene unload。
9. 增加 focused tests 与真实 Play enter/exit/re-enter 取证；Running 状态 checksum/pass 语义保持不变。

## 5. 不变量与风险

- worker 未确认停止时，不得清 World、factory、pool 或 renderer；失败保持 `Stopping`，不恢复 gameplay，不谎报 `Stopped`。
- teardown 不得访问会创建 GameObject 的 singleton `.Instance`；允许捕获引用、`TryGetInstance()` 和显式 owner。
- shutdown 期间拒绝 Spawn/Register，但允许 Unregister/Free/Destroy/Generation release。
- 不以 `_world = null` 掩盖 World object、claimed slot 或 pending structural 残留。
- 不修改 Scene、DAT、Prefab、URP 资源、Input Actions、Server、C++、formal marker 或 30 Hz。
- 不在本 Change 中完成全项目 Mono/Core 分层、目录移动或 asmdef 拆分；只做有序关闭所必需的最小边界。

## 6. 验收标准

- Unity script compile 0 error。
- focused tests 覆盖生命周期转换、重复 shutdown、tick/input/spawn gate、worker stop、pending task discard、presentation reset、renderer/world/pool/boundary 后置条件和 teardown 无 singleton 自动创建。
- `BattleRuntimeSelfCheck` 新鲜通过，且现有相关 worker/central/OPoint/lifecycle focused tests无新增差异。
- 真实执行两轮 `enter Play → 等待延迟角色生成 → exit Play`；每轮 cleanup warning、`*_AutoCreated` teardown residual、active pool borrower、World object/slot、central submission 和 `__BoundaryAssetRuntime_*` 均为零。
- Scene dirty 状态不因验证改变。
- `Tools/Validate-ChangeLedger.ps1` 通过；若被已记录的无关 Change 阻塞，如实报告而不修改无关记录。

## 7. 回滚

- 只移除本 Change 新增的 lifecycle state、shutdown transaction、gate/discard/quiesce API 和 focused tests，恢复其调用点到改前行为。
- 保留 `BATTLE-SCENE-TEARDOWN-SINGLETON-001` 的 `TryGetInstance()` 修复、现有中央血条/Editor preview、Server/Client 对齐和所有用户未提交改动。
- 不使用 `git restore/reset/clean`；若必须回滚，使用逐文件 `apply_patch` 反向修改并重新验证。

## 8. 实际实现与验证

### 8.1 实际实现

- 新增 `BattleRuntimeLifecycleState`、固定 11 阶段 `BattleRuntimeShutdownStage`、shutdown report/diagnostics；`SimulationTickDriver` 统一拥有阶段 1～10，`AppManager/BattleBootstrap` 完成地图 carrier 阶段 11 后才进入 `Stopped`。
- `Update/LateUpdate` 仅在 `Running` 自动推进；显式 manual/diagnostic tick 兼容 `Preparing`，但 `Stopping/Stopped` 无论 `ignorePaused` 都硬拒绝。
- dedicated worker 先 stop/join；随后 Core/Mono OPoint 与 structural create 入口关闭。pending OPoint 只 discard/recycle，不再 materialize；Unregister/Free/Destroy 仍可执行。
- allocation unseal 使用 preparation/seal 阶段捕获的 owner，shutdown 不解析或创建任意全局 factory/pool；这保留并收紧 `BATTLE-SCENE-TEARDOWN-SINGLETON-001` 的非创建 teardown 合同。
- central submission、presentation publication 与 sound cache 在 World 仍有效时清空；active renderer/sprite borrower 先归还，再验证并清空 logic entity、registry、slot、structural state，最后解绑 World 并 quiesce pool。
- 新增 Editor `ExitingPlayMode` bridge；即使 Unity 开始退出 Play，也先执行同一协调事务。`BattleBootstrap.DisablePresentation()` 幂等清理 runtime map/boundary carrier。
- 新增 4 个 focused Editor tests，并把新增池状态纳入既有 SelfCheck 临时夹具的保存/恢复，避免测试状态泄漏。

实际由本 Change 写入的脚本为：

- `Assets/NTSD/Scripts/Simulation/BattleRuntimeLifecycle.cs`
- `Assets/NTSD/Scripts/Simulation/SimulationTickDriver.cs`
- `Assets/NTSD/Scripts/Simulation/SimulationWorld.cs`
- `Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs`
- `Assets/NTSD/Scripts/Simulation/BattleRuntimeAllocationGate.cs`
- `Assets/NTSD/Scripts/Simulation/BattleLogicObjectPointRuntime.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/BattleStructuralWriter.cs`
- `Assets/NTSD/Scripts/Animation/Character/LF2ObjectPointFactory.cs`
- `Assets/NTSD/Scripts/Animation/LF2ObjectPool.cs`
- `Assets/NTSD/Scripts/App/AppManager.cs`
- `Assets/NTSD/Scripts/App/BattleBootstrap.cs`
- `Assets/NTSD/Scripts/App/Editor/BattleRuntimeEditorShutdownBridge.cs`
- `Assets/NTSD/Scripts/Test/Editor/BattleRuntimeOrderedShutdownEditorTests.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`

声明范围内的 `SimulationWorld.Passes.partial.cs`、`LF2ObjectRenderer.cs`、`BattleCentralRenderSystem.cs`、`BoundaryWallManager.cs` 已复用其现有能力，本 Change 未对这些文件追加代码；工作树中的既有修改不得归因于本 Change。

### 8.2 新鲜验证证据

- Unity 2022.3.62f3 脚本 refresh/domain reload 完成；退出编译后 `read_console(types=error)` 为 0，当前 Assembly-CSharp/Editor 脚本编译无 C# error。
- Ordered shutdown focused job `79e995bbf95f46bf9cda1bbaca6af998`：`4/4 PASS`。覆盖 `Running→Stopping→Stopped`、map cleanup 失败后重试、重复 shutdown、tick gate、OPoint discard/recycle、structural reject、renderer/sprite return 与 pool quiesce。
- Dedicated worker job `7ce34263c28d467b8a4081b1e1e94b88`：`20/20 PASS`。
- W05 OPoint job `9676206742ea49e3a0acc68ab39ca7e9`：`8/8 PASS`。
- Central latest-frame materialization job `02d11c83ca0949f1a3902533eb85f049`：`13/13 PASS`。
- 目标 singleton teardown job `8f28bafa9a95458fad02e3c7e4c3c376`：`2/2 PASS`；allocation unseal/overlay destroy 均未解析或自动创建 singleton。
- 最终代码上的真实场景 `Assets/NTSD/Scene/NTSD_Battle.unity` 连续两轮 `Play → 等待 12 秒 → Stop`：每轮退出后 Console error/warning=`0`，未出现 `Some objects were not cleaned up`，未出现 `LF2ObjectPointFactory_AutoCreated` 或 `LF2ObjectPool_AutoCreated`；Scene 始终 `isDirty=false`、rootCount 前后均为 13。退出后的完整 root Hierarchy 无 `__BoundaryAssetRuntime_*`、factory/pool runtime carrier，也无临时 `Spark` root。

### 8.3 阻塞与未扩展范围

- 完整 `BattleRuntimeSelfCheck` 已在 2026-09-01 09:35、09:38、09:46 连续运行；新增池夹具空引用修正后，三次都继续运行到既有 Naruto DDA 检查，并以同一断言失败：`saw242=True, saw243=True, saw244To247=False`（`CheckNarutoDdaThrownCloneLandingGate`）。该调用链不属于本次 Unity teardown/lifecycle 变更；按用户批准范围，不顺手修改 Naruto 战斗规则。因此 full SelfCheck 不能记录 PASS，Change 暂记 `BLOCKED`，恢复条件是该既有 Naruto DDA 断言由其权威任务修复或获得明确排除批准。
- `PlayDomainReloadPoolLifecycleEditorTests` 整类运行另有既有参数化 `RestartPolicy_IsBoundedAndStateDriven(... expected 5, actual 1)` 失败；本 Change 的两个目标用例单独运行均通过，未修改 restart policy。
- `Tools/Validate-ChangeLedger.ps1` 已执行但被无关治理记录阻断：`CLIENT-CONTENT-FRAME-STRUCTURE-ALIGNMENT-001` 缺 `code-path` metadata，`S0-FORMAL-CONTENT-CLOSURE-001` 把一个 TSV 与六个 DAT 声明为 non-governed code path。本 Change 自身只有声明但未改 `BoundaryWallManager.cs` 的 warning；按约束未替其他任务改账本。
- 未修改 Scene、DAT、Prefab、URP、Input Actions、Server、C++、30 Hz 或 Running battle pass/checksum 语义。
