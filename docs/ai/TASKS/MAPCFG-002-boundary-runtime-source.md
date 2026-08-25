# MAPCFG-002 — Boundary Asset 运行时数据源接线

> 状态：RUNTIME_PENDING / P3 READY / P4 INTEGRATION PENDING  
> 日期：2026-08-25  
> 父计划：BATTLE-MAP-BOUNDARY-ASSET-001  
> 前置：MAPCFG-001 = FOCUSED_TEST_PASS / RUNTIME_NOT_CONNECTED

## Goal

让一个已经通过 `BattleMapBoundaryDefinition.TryValidate` 的 Boundary Asset 可以被**显式**加载为 `BoundaryWallManager` 当前的运行时数据源，同时完整复用既有 `BoundaryWall` / `BoundaryWallManager` 的任意多边形查询语义。

## Scope

- 为 `BoundaryWall` 增加一个仅复制现有 `BoundaryData` world X/Y 顶点到其本地 polygon 容器的最小装载接口；
- 为 `BoundaryWallManager` 增加显式 `TryLoadBoundaryDefinition(...)` 与显式清除/恢复 Scene fallback 的接口；
- 装载成功时，Manager 只查询由该 Asset 构造的 runtime `BoundaryWall` 承载对象，不与 Scene fallback 混合；
- 装载失败时，已有 active 数据源必须保持不变；
- 增加 focused EditMode tests，比较 source wall 与 Asset-loaded manager 的 point、rect、确定性随机采样和 Stage bounds 结果；
- 运行 Unity compile、focused tests、相关 existing self-check、Ledger validator 和 scoped diff check。

## Existing Behavior to Preserve

- `BoundaryWall.ContainsPoint` / `ContainsPointWorld` 的点包含、边缘 epsilon 与 simple-polygon gate；
- `BoundaryWall.IsRectAllowed` / `ContainsRect` 的完整矩形包含规则；
- `BoundaryWallManager.IsPointWalkable`、`IsRectWalkable`、`TryGetRandomWalkablePoint`、`TryGetWalkableBounds`、`TryGetBattleStageRuntime`、`TryGetBattleStagePixelBounds` 的现有 public API 与返回语义；
- 多个 BoundaryWall 和每个 BoundaryWall 内多个 polygon 的并集语义；
- world X/Y 坐标、当前 `NTSDRenderSpace` stage 转换与既有 deterministic RNG 路径。

## Explicitly Out of Scope

- 不改 point-in-polygon、rect-in-polygon、边缘 epsilon、simple-polygon、随机采样或 Stage bounds 算法；
- 不将 polygon 改为矩形，不新增 HardBlock、Special、hole 或地图碰撞规则；
- 不接入 `BattleBootstrap`、`GameConfig`、`SimulationWorld`、tick、Camera、Bg 或平台背景表现；
- 不创建默认 Asset，不保存/改写 `NTSD_Battle.unity`，不自动把 Scene 数据写回 Asset；
- 不做 C++ Release 审计、网络、lockstep、fingerprint、服务器或联机协议工作；
- 不在 battle tick 内读取 Asset、扫描 Scene、创建对象或分配容器。

## Files Likely Involved

- Assets/NTSD/Scripts/LevelEditor/BoundaryWall.cs
- Assets/NTSD/Scripts/LevelEditor/BoundaryWallManager.cs
- Assets/NTSD/Scripts/Test/Editor/BattleMapBoundaryRuntimeSourceEditorTests.cs
- docs/ai/CHANGE-RECORDS/MAPCFG-002-boundary-runtime-source.md
- docs/ai/CHANGE-LEDGER.md
- docs/ai/STATE.md
- 当前计划与 Handoff

## Implementation Contract

1. Asset 装载是显式 bootstrap-time / caller-time 动作，不是 `Update`、`OnValidate` 或每 tick 动作。
2. 每个 `BoundaryData` 生成一个 transient runtime `BoundaryWall`；其 world X/Y 顶点逐值复制，承载对象保持 identity world transform，因此原有几何函数看到的世界顶点不变。
3. 成功装载后 Manager 缓存只包含这些 transient asset walls；`RefreshBoundaries()` 不得把 Scene walls 并入该 active source。
4. 明确清除 asset source 后，才允许 `RefreshBoundaries()` 恢复既有 Scene fallback。
5. invalid Asset 或任何构造失败必须 fail closed，且不得替换已激活的 source。
6. Asset 数据、Scene wall 和背景资源之间不发生反向写入。

## Acceptance / Verification

1. 同一组 `BoundaryData` source wall 与 Asset-loaded manager 对 inside/outside/edge point、inside/crossing rect、`TryGetBattleStageRuntime` 和同 seed 的 deterministic random sample 结果一致；
2. 多 boundary group / 多 polygon 的并集可同时工作；
3. 无效装载失败后，已加载的合法数据源、查询结果和 Asset 顶点保持不变；
4. 显式 clear 后 Scene fallback 的既有刷新路径仍可用；
5. Unity compile 0 error；focused EditMode tests 通过；相关 existing `BattleRuntimeSelfCheck` 通过或如实记录 blocker；
6. `Tools/Validate-ChangeLedger.ps1` 与 scoped `git diff --check` 通过。

## Stop Conditions

- 需要改变现有几何 API 语义才能装载 Asset；
- Asset world X/Y 顶点不能原样经现有 `BoundaryWall` 得到相同查询结果；
- 需要把 Asset lookup、Scene scan 或对象创建放入 tick；
- 需要修改 Scene、Bootstrap、背景或 battle gameplay 才能完成 P2；
- 发现 P2 必须扩大到 C++、网络或未批准的规则修改。

## Current Progress

- 只读确认：`BoundaryWallManager` 的现有 queries 都消费 `_boundaries` 中的 `BoundaryWall`，而 `BoundaryWall` 已以 world X/Y 展开顶点；
- 只读确认：现有 JSON export 正是 `BoundaryData -> PolygonData -> Vector2Data` 的多个 boundary group / 多 polygon world X/Y 形状；
- 已修改 `BoundaryWall`、`BoundaryWallManager` 并新增 focused Editor test；未改 Scene、Asset 实例、DAT、C++、Bootstrap、Camera、Bg 或 battle logic；
- Unity 二次 compile 已完成；focused EditMode job `850175a9e86141f680f03e2bcb26f7b5` 为 3/3 PASS；
- scoped `git diff --check` 与 static call-site scope review 已通过；
- existing `BattleRuntimeSelfCheck` 已通过项目既有 result 文件于 15:53:47 写出的 `PASS`；
- `Tools/Validate-ChangeLedger.ps1` 已通过（103 条 Record、138 个 governed diff covered），scoped `git diff --check` 已通过；
- 下一动作：可进入 P3 的 explicit Editor authoring；P4 的 MapId/Bootstrap 接线仍排除。
