# MAPCFG-003 — Boundary Asset 显式 Editor 作者工具

> 状态：FOCUSED_TEST_PASS / P4 READY / MANUAL-INSPECTOR PENDING  
> 日期：2026-08-25  
> 父计划：BATTLE-MAP-BOUNDARY-ASSET-001  
> 前置：MAPCFG-001 = FOCUSED_TEST_PASS；MAPCFG-002 = RUNTIME_PENDING

## Goal

让 Scene `BoundaryWall` 的顶点编辑和 `BattleMapBoundaryDefinition` 的实际 world X/Y 数据通过**显式、可撤销、fail-closed**的 Load / Apply 操作保持同源。

## Scope

- 为 `BoundaryWall` 增加当前多 polygon → 既有 `BoundaryData -> PolygonData -> Vector2Data` world X/Y 数据的复制接口；
- 为 `BattleMapBoundaryDefinition` 增加深复制、验证后替换其 boundary 数据的 authoring 接口；
- 为 `BoundaryWallManager` 增加 Inspector 可见的 authoring Asset 引用，以及显式 Asset→Scene / Scene→Asset 操作；
- 扩展现有 `BoundaryWallManagerEditor` 的按钮、MapId 提示和失败信息；
- 新增 focused EditMode tests，覆盖 round trip、deep copy、name/count mismatch fail-close 与 active P2 runtime source blocking。

## Authoring Contract

1. 工具仅在 Edit Mode 工作；不在 Play Mode、`Update`、tick 或 `OnValidate` 自动 Load / Apply。
2. Asset → Scene：只接受通过 `TryValidate` 的 Asset；只使用既有 Scene fallback walls；按稳定 hierarchy order 和同名 boundary group 预检，数量或名称不匹配时完全不写任何 wall。
3. Scene → Asset：只捕获 enabled Scene fallback walls 的现有 world X/Y 数据；先完整深复制/验证，再一次替换 Asset 内部数据；两者不能共享 mutable vertex list。
4. 两个方向都通过 `Undo` 记录且只标记 dirty，**不得调用 `AssetDatabase.SaveAssets`、`EditorSceneManager.SaveScene` 或自动保存**。
5. 若 Manager 正在使用 P2 transient loaded source，P3 必须 fail closed，要求先显式 clear；runtime carriers 不得充当 Scene authoring walls。

## Existing Behavior to Preserve

- BoundaryWall 的点/矩形/edge epsilon/simple-polygon 语义；
- Manager 的 union、random sample、Stage bounds 和 P2 runtime source 语义；
- `BoundaryWallEditor` 当前拖拽、插点、删点、Undo、Scene View；
- 现有 JSON export 路径和保存策略。

## Explicitly Out of Scope

- 不自动创建、删除、启用、禁用或重排用户 Scene 的 BoundaryWall；
- 不保存 Scene、Asset、ProjectSettings 或默认地图资产；
- 不接 Catalog、MapId runtime selection、Bootstrap、Bg、Camera、SimulationWorld、battle tick、C++、网络或 lockstep；
- 不添加新 polygon 规则或改现有查询算法。

## Files Likely Involved

- Assets/NTSD/Scripts/LevelEditor/BattleMapBoundaryDefinition.cs
- Assets/NTSD/Scripts/LevelEditor/BoundaryWall.cs
- Assets/NTSD/Scripts/LevelEditor/BoundaryWallManager.cs
- Assets/NTSD/Scripts/Test/Editor/BattleMapBoundaryAuthoringEditorTests.cs
- docs/ai/CHANGE-RECORDS/MAPCFG-003-explicit-boundary-authoring.md
- docs/ai/CHANGE-LEDGER.md
- docs/ai/STATE.md
- 当前计划与 Handoff

## Acceptance / Verification

1. Asset → Scene → Asset 的 world X/Y 顶点数量、顺序、坐标、group name 和 polygon name 一致；
2. Scene wall 修改后，Asset 只有在显式 Apply 后才改变；Apply 后两者不共享 mutable data；
3. Scene/Asset name-count mismatch 或 active runtime source 均 fail closed，Asset 与 Scene 不变；
4. Unity compile、focused EditMode tests、Ledger validator、scoped diff check 通过；
5. 不运行 Play Mode；真实 MapId/Catalog/Bootstrap/Player 验收留给 P4。

## Stop Conditions

- 无法在不改几何语义的前提下实现 world X/Y round trip；
- 需要自动保存、自动覆盖或自动创建/删除用户 Scene walls 才能完成；
- 需要 P4 MapId/Bootstrap、Scene 资产部署、背景或 battle gameplay 才能测试 P3；
- 发现 P3 必须扩大到 C++、网络或未批准范围。

## Current Progress

- 已只读确认：`BoundaryWall` 能把 local polygon 转为 world X/Y，Manager JSON export 已证明所需数据形状；
- 已只读确认：现有 `BoundaryWallManagerEditor` 有 default inspector、manual refresh 和 JSON export，可最小补显式按钮；
- 已修改三份 LevelEditor C# 并新增 focused Editor test；未写 Scene、Asset 实例、DAT、C++、Bootstrap、Camera、Bg 或 battle logic；
- Unity compile、P3 focused job `5e4b965f9e7b4452a5c6e236117b673a` 3/3、static no-save audit 和 scoped diff 已通过；
- cross-phase job `63182377db004cb084fc830402bbb878` 已 10/10 PASS，P3 后 existing `BattleRuntimeSelfCheck` result 于16:26:35为 PASS；
- final Ledger validator（104 Record / 139 governed diff covered）与 scoped diff 已通过；
- 下一动作：可进入 P4 configuration / integration planning；不运行 Play Mode，真实 MapId/Catalog/Bootstrap/Player 验收仍独立。
