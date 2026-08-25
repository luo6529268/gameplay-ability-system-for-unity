# MAPCFG-001 — Map ID、Boundary Asset 与 Presentation Asset

> 状态：FOCUSED_TEST_PASS / RUNTIME_NOT_CONNECTED  
> 日期：2026-08-25  
> 父计划：BATTLE-MAP-BOUNDARY-ASSET-001

## Goal

创建按 MapId 配置现有任意多边形可行走区域和地图表现资源的基础 Asset 类型，并提供一对一 MapId 配对校验。

## Scope

- 新增 BattleMapBoundaryDefinition ScriptableObject；
- 新增 BattleMapPresentationDefinition ScriptableObject；
- 新增 BattleMapCatalog 及其配置条目；
- 新增 focused EditMode tests；
- 仅验证数据完整性、MapId 配对与 world X/Y 顶点结构。

## Existing Behavior to Preserve

- BoundaryWall 的多个 polygon 并集；
- BoundaryWallManager 的 IsPointWalkable、IsRectWalkable、TryGetRandomWalkablePoint 和 TryGetBattleStageRuntime；
- BoundaryData、PolygonData、Vector2Data 的世界 X/Y 顶点结构。

## Files Expected

- Assets/NTSD/Scripts/LevelEditor/BattleMapBoundaryDefinition.cs
- Assets/NTSD/Scripts/LevelEditor/BattleMapPresentationDefinition.cs
- Assets/NTSD/Scripts/LevelEditor/BattleMapCatalog.cs
- Assets/NTSD/Scripts/Test/Editor/BattleMapAssetConfigurationEditorTests.cs
- docs/ai/CHANGE-RECORDS/MAPCFG-001-map-assets-catalog.md
- docs/ai/CHANGE-LEDGER.md
- docs/ai/STATE.md
- 当前计划和 Handoff

## Explicitly Out of Scope

- 不修改 BoundaryWall / BoundaryWallManager 的几何算法；
- 不将 polygon 压缩为矩形；
- 不接入 BattleBootstrap、SimulationWorld、GameConfig、Scene、Bg 或 Camera；
- 不创建默认地图 Asset；
- 不执行 C++ audit、lockstep/fingerprint、服务器或新 battle physics。

## Deliverables

1. 两类 Map Asset；
2. 可验证的 MapId 配对表；
3. Asset 数据与现有 JSON 世界顶点结构兼容；
4. focused tests 覆盖合法配对、重复 MapId、MapId 不匹配、非法顶点和 presentation 不改变 boundary；
5. Change Record、Ledger、State、Handoff 与验证证据。

## Verification

1. Unity scripts compile 0 error；
2. 目标 EditMode tests 通过；
3. Tools/Validate-ChangeLedger.ps1 通过；
4. scoped git diff 检查通过。

## Stop Conditions

- 数据模型无法精确保留 BoundaryData / PolygonData / Vector2Data 的 world X/Y 形状；
- 需要改变 BoundaryWall 几何语义；
- 需要接入运行时或 Scene 才能测试 Asset 数据模型；
- 发现 P1 必须扩大到未批准的 C++、网络或 battle logic 范围。

## Current Progress

- Change Record、Ledger、STATE 和 Handoff 已在代码写入前建立；
- 已新增三个 Map Asset/Catalog 类型与一个内存 focused Editor test；
- BoundaryWall、BoundaryWallManager、Scene、runtime、Bg、Camera、GameConfig、C++、网络、lockstep 均未改；
- Unity 已正常导入、编译；focused EditMode job a0b70302cb314b0cbb0a6b6d3fee0457 为 4/4 PASS；
- Tools/Validate-ChangeLedger.ps1 已通过（102 条 Record、135 个当前 governed code diff 均有记录覆盖）；scoped git diff 检查通过；
- P1 未接入 runtime，BattleRuntimeSelfCheck 与 Play Mode 将留给 P2/P4 的实际数据源/Bootstrap 接线验收。
