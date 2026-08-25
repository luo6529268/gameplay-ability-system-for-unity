# BATTLE-MAP-BOUNDARY-ASSET-001 — Map ID 与 BoundaryWall 配置化总合同

> 状态：PLANNED / NO CODE  
> 日期：2026-08-25  
> 规范计划：Assets/NTSD/Docs/battle-map-boundary-asset-configuration-plan.md

## Goal

将当前 BoundaryWall 和 BoundaryWallManager 已有的任意多边形可行走区域，从 Scene 固化数据改为按 MapId 选择的 Asset 数据；同时用另一份 Asset 保存同 MapId 的背景和表现资源。

本任务复用现有 polygon 行为，不创建新的战斗物理规则。

## Scope

- BattleMapBoundaryDefinition：MapId 和现有 BoundaryWall 语义的多边形世界顶点；
- BattleMapPresentationDefinition：MapId 和背景/表现资源；
- 最小 MapId 配对表；
- BoundaryWallManager 的数据源切换；
- BoundaryWall Editor 的显式 Asset 往返；
- Battle bootstrap 的地图选择。

## 绝对边界

- 不需要 C++ Release 审计作为前置；
- 不压缩 polygon 为矩形；
- 不重写 ContainsPoint、IsRectAllowed、随机采样、边缘 epsilon 或现有外接 bounds；
- 不改战斗 tick、移动、hit、opoint、AI、lockstep、服务器、Camera 或背景显示方案；
- 不在 Play Mode 自动把 Scene 保存到 Asset 或把 Asset 覆盖到 Scene。

## 现有证据

- BoundaryWall 已保存多个 polygon，并使用 Unity world X/Y 平面；
- BoundaryWallManager 已提供 IsPointWalkable、IsRectWalkable、TryGetRandomWalkablePoint 和 TryGetBattleStageRuntime；
- BoundaryWallManager 已有多个 BoundaryData / PolygonData / Vector2Data 的 JSON 世界顶点导出形状；
- 当前改动目的只是让这些已有能力按 MapId 加载。

## Phase 划分

| 阶段 | ID | Goal | 当前状态 |
|---|---|---|---|
| P1 | MAPCFG-001 | 两类 Asset 与 Map ID 配对 | IN_PROGRESS / PRE-CODE |
| P2 | MAPCFG-002 | Manager 从 Boundary Asset 加载、保持现有 API 行为 | NOT_STARTED |
| P3 | MAPCFG-003 | Editor 显式加载/应用/预览 | NOT_STARTED |
| P4 | MAPCFG-004 | Bootstrap Map ID 选择、背景资源与验收 | NOT_STARTED |

## 验收总则

1. 同一边界从 Scene 导出到 Asset 并重新加载后，点、矩形、随机点和 Stage 外接范围结果保持一致；
2. MapId A 不会加载 MapId B 的边界或背景；
3. Windows 与 Android 的本地背景显示差异不改变同一 MapId 的边界结果；
4. Editor 中最终 Apply 的数据与 Play Mode 实际加载的数据相同；
5. 每个脚本改动均有独立 Change Record、Ledger、State、Handoff 和 Validator 证据。

## 下一包

MAPCFG-001。先只新增数据类型与纯数据校验，不接入 runtime，不保存场景，不修改现有 BoundaryWall 几何算法。

MAPCFG-001 已完成 pre-code 审计和 Change Record 留痕，接下来只允许写入数据类型与 focused Editor test。
