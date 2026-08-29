# WORKER-UNITY-BOUNDARY-001 — fail-closed dedicated worker eligibility for Unity-bound entities

<!-- CHANGE-RECORD
id: WORKER-UNITY-BOUNDARY-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Simulation/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleSimulationWorkerBoundaryEditorTests.cs
authority: USER-RUNTIME-FAILURE-20260829 / UNITY-MAIN-THREAD-CONTRACT
evidence: USER-STACK-LF2SPRITE-APPLYENTITYRENDERERVISIBILITY / UNITY-JOB-881E133B32AE4D3F82043DC29ECEC66D / PLAY-TICK-2860-FREE66
-->

> 创建日期：2026-08-29  
> 当前状态：`VERIFIED / SAFE-SYNC-FALLBACK`  
> 类型：Unity worker adapter 安全修复；不属于 C++ gameplay rule 修改

## Goal

修复生产 `CentralOnly + DedicatedBattleSimulationWorker` 在实体销毁时从后台线程访问 `SpriteRenderer`，触发 `EnsureRunningOnMainThread` 并使 Driver fail-closed pause 的问题。

本 Change 采用最小安全边界：当当前 world 中任何已注册 runtime entity 仍绑定 `LF2ObjectRenderer` 或 legacy `ShadowRenderer` 时，Dedicated Worker 必须判定为不具备资格并保留同步主线程 tick；只有纯逻辑实体 world 才允许启动 worker。

## Observed Facts

- 用户堆栈：`FlushPendingEntityDestroy -> FreeEntityLikeExe -> LF2Sprite.Hide -> ApplyEntityRendererVisibility -> UnityEngine.Object.op_Equality`，异常为 `EnsureRunningOnMainThread can only be called from the main thread`；
- `SimulationTickDriver` 默认 `useDedicatedSimulationWorker=true`，生产场景为 `CentralOnly`；
- `BeginBattleAllocationSeal` 在初始角色已经通过 `LF2ObjectPool` 绑定 Renderer 后才启动 worker；`SetLogicOnlyEntityMaterialization(true)` 只影响后续实体创建，不会移除既有 Unity 组件引用；
- 当前 worker focused full-tick fixture 使用无 Renderer 的 logic-only entity，因此没有覆盖生产初始实体绑定；
- 2.5D 预览实验已由用户还原，当前 source HEAD 为 `afe0d792`；本故障与该实验无关。

## Allowed Scope

- `SimulationWorld` 新增无 Unity mutation 的只读资格查询；
- `SimulationTickDriver.ResolveDedicatedSimulationWorkerIneligibilityReason` 增加 fail-closed gate 与稳定原因码；
- `BattleSimulationWorkerBoundaryEditorTests` 增加 Renderer-bound 拒绝与 pure-logic 允许的 focused fixture；
- 本 Record、Ledger、STATE 与 handoff 同步。

## Explicitly Forbidden

- 不在 worker 线程调用、比较、启停或释放任何 Unity `Object`；
- 不实现跨线程 GameObject/Renderer release queue；
- 不改变 tick/pass、输入、RNG、checksum、碰撞、对象生命周期结果或 C++ authority；
- 不修改 Scene、DAT、服务器、配置或用户当前未提交的 server-lockstep 文档；
- 不把同步 fallback 写成 Dedicated Worker 已修复或性能门已通过。

## Acceptance Criteria

1. Renderer-bound active entity 会得到稳定 ineligibility reason，worker 不启动；
2. 无 Renderer 的 logic-only world 仍通过既有 worker 资格与完整 tick 测试；
3. Unity compile 0 error，focused worker tests 通过；
4. 真实 `NTSD_Battle` Play 不再出现本堆栈，Driver 不因 worker failure 暂停；
5. Ledger validator 与 `git diff --check` 通过。

## Rollback

仅撤回本 Change 的三个代码文件和治理记录；不得影响用户当前修改的 `CODEX-CURRENT-HANDOFF.md`、server-lockstep 文档、`ServerLockstepStages/` 或 `.claude/`。

## Actual Changes / Evidence

- `SimulationWorld.HasUnityPresentationBindingsForDedicatedWorker` 按 runtime slot 扫描当前实体，只用 `ReferenceEquals` 检测 `Renderer` / `ShadowRenderer` 绑定，不调用或修改 Unity Object；
- `SimulationTickDriver.ResolveDedicatedSimulationWorkerIneligibilityReason` 在启动线程前增加稳定原因码 `unity-presentation-bindings-are-still-attached`；其他资格原因顺序和 pure-logic worker 路径保持原样；
- `FormalCentralBattleRejectsWorkerWhenEntityKeepsUnityRendererBinding` 实际构造 Renderer-bound runtime entity，验证 worker 不启动、无 failure、CentralOnly logic-only spawn boundary 保持；
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore /m:1`：exit 0、0 error，仅项目既有 warnings；这是静态交叉检查；
- Unity `refresh_unity(force/scripts/compile=request/wait_for_ready=true)` 成功恢复并 ready；
- worker 边界整类 job `881e133b32ae4d3f82043dc29ecec66d`：20/20 PASS、0 failed；既有 pure-logic full tick、input、opoint、lifecycle、publication/ack 与新增 gate 一并通过；
- 真实 `NTSD_Battle` Play：tick 1807 时 `paused=false`、worker active/in-flight 均 false、reason=`unity-presentation-bindings-are-still-attached`、worker failure=null；
- 延长至 tick 2860 后仍 `paused=false`、worker failure=null，结构写入统计 `commands=446 / spawn=73 / unregister=77 / free=66 / destroy=0`，证明本轮已实际经过 66 次 entity free，而非仅等待尚未触发回收；
- Console 对 `Dedicated simulation worker failed` 与 `EnsureRunningOnMainThread` 精确过滤均为 0；其余 error/warning 仅 MCP client-handler 退出日志，不是项目异常；
- 验证结论只覆盖：Renderer-bound 生产 world 安全停用 dedicated worker并继续同步 tick。本 Change 没有让 Unity-bound entity 支持后台 worker，也没有恢复 worker 性能门；若未来要重新启用，必须先完成主线程 presentation detach/release 所有权设计与独立 Change。
