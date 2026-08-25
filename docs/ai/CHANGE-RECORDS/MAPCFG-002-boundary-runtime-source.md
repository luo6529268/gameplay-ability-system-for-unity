# MAPCFG-002 — Boundary Asset 运行时数据源接线

<!-- CHANGE-RECORD
id: MAPCFG-002
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/LevelEditor/BoundaryWall.cs
code-path: Assets/NTSD/Scripts/LevelEditor/BoundaryWallManager.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleMapBoundaryRuntimeSourceEditorTests.cs
authority: USER-DIRECTED-20260825 / UNITY-NATIVE-MAP-BOUNDARY-CONFIGURATION / EXISTING-BOUNDARYWALL-SEMANTICS
evidence: UNITY-EDITOR-COMPILE-20260825-154556 / EDITMODE-850175a9e86141f680f03e2bcb26f7b5-3OF3-PASS / SELFCHECK-PASS-20260825-155347
-->

> Change ID：MAPCFG-002  
> 状态：RUNTIME_PENDING / P3 READY / P4 INTEGRATION PENDING  
> 类型：Unity 运行时配置数据源接线；不改变 battle rule  
> 创建日期：2026-08-25  
> Authority：用户明确要求，复用既有 Unity BoundaryWall / BoundaryWallManager 的多边形语义；不需要 C++ Release 规则审计。

## 用户需求与依据

- 可行走区域是任意多边形，本质是当前 `BoundaryWall` / `BoundaryWallManager` 正在做的事；
- MapId 对应 Boundary Asset，随后由地图选择加载；
- 所有脚本改动都必须有可恢复审计痕迹；
- P1 已建立并验证 `BattleMapBoundaryDefinition`、`BattleMapPresentationDefinition` 与 `BattleMapCatalog`，但未接入 runtime。

## Unity 修改前现状

- `BoundaryWallManager` 通过 `_boundaries` 中的 `BoundaryWall` 执行所有 point、rect、random sample 和 Stage bounds 查询；
- `RefreshBoundaries()` 从 Scene 收集 BoundaryWall；
- JSON export 已把每个 enabled BoundaryWall 导出成 `BoundaryData`，并把顶点展开成 world X/Y `Vector2Data`；
- `BoundaryWall` 已有同一套 point/rect/simple-polygon 实现，却没有显式从 `BoundaryData` 装载 world 顶点的运行时桥；
- P1 的 Asset 类型没有被任何 runtime 或 Scene 调用。

## 允许修改的路径和符号

- Assets/NTSD/Scripts/LevelEditor/BoundaryWall.cs：只增加 Asset world-vertex 复制桥；
- Assets/NTSD/Scripts/LevelEditor/BoundaryWallManager.cs：只增加显式 Asset source load / clear、transient carrier 生命周期和 loaded-source refresh guard；
- Assets/NTSD/Scripts/Test/Editor/BattleMapBoundaryRuntimeSourceEditorTests.cs：focused EditMode tests；
- 本 Record、Ledger、STATE、Task、Handoff 与父计划。

## 不可变合同

- 不修改任何现有几何实现、阈值或 public query 的含义；
- Asset 顶点逐值复制为 world X/Y，不量化、不重排、不裁剪、不改背景 Transform；
- runtime carrier 只在显式 load / clear 时创建或销毁，绝不在 tick 中创建；
- active Asset source 和 Scene fallback 不能混合；load failure 不得改变旧 source；
- P2 不保存 Scene、不创建 production 默认 Asset、不接 Bootstrap / GameConfig / Bg。

## 预期副作用和不可回退边界

- `BoundaryWall` 与 `BoundaryWallManager` 会多出供 P4 bootstrap 调用的显式 API；
- 成功装载一个 Asset 后，当前 manager query 改读 transient carriers，直到 caller 显式 clear；
- P2 不会自动选择 MapId，因此现有 Battle Scene 的默认运行行为不应改变；
- 若未来需要回滚，只能在明确授权下撤回 P2 的三个脚本与文档变动；当前不执行删除。

## 验收标准

1. Asset load 前后的相同几何在现有 Manager API 下给出一致结果；
2. 多边界组并集保持；
3. invalid load fail closed，旧 source 不被替换；
4. load 与 query 不反向写 Asset 或 Scene fallback wall；
5. Unity compile、focused tests、相关 existing self-check、Ledger validator、scoped diff check 有真实结果。

## 回滚方式

P2 已写入三份 task-owned C# 文件；后续若需回滚，必须先获得明确授权；不得借由删除、reset 或覆盖当前用户工作树实现回滚。

## 实际脚本改动

- `BoundaryWall.TryApplyWorldBoundaryData(BoundaryData, out string)`：先完整校验，再将 Asset 的 world X/Y 顶点转换/复制为该 wall 的 local polygon 数据；没有改动现有 point、rect、edge epsilon 或 simple-polygon 查询实现。
- `BoundaryWallManager.TryLoadBoundaryDefinition(...)`：只接受通过 P1 validation 的 Asset；为每个 `BoundaryData` 构造一个 identity-world transient carrier，全部构造成功后才原子替换 active source；失败会销毁本次 pending carriers 并保留旧 source。
- `BoundaryWallManager.ClearLoadedBoundaryDefinition()`：显式销毁 transient carriers 后才回到既有 `RefreshBoundaries()` Scene fallback；loaded 状态下的 `RefreshBoundaries()` 只刷新 carrier cache，不合并 Scene walls。
- 新增 `BattleMapBoundaryRuntimeSourceEditorTests`：覆盖 source/Asset query parity、multi-boundary union、deterministic random sample、Stage bounds、failed load retaining active source，以及 explicit clear 才恢复 Scene fallback。
- 首次 Unity compile 的 NUnit object-collection overload 问题已仅在该 focused test 中改为本地 `ContainsBoundary(...)` 循环；production source、fixture 几何和预期均未改变。
- 未改 `BattleBootstrap`、`GameConfig`、`SimulationWorld`、tick、Bg、Camera、Scene、Asset 实例、DAT、C++ 或服务器。

## 验证记录

- 只读预检已完成；
- scoped `git diff --check` 已通过；static call-site review 确认新 load / clear API 只出现在 Manager 与 focused test，未接入 `Update`、battle tick、Bootstrap、Camera 或 Bg；
- 尚未运行 Unity compile、focused test、BattleRuntimeSelfCheck 或 Play Mode；下一步必须使用已打开的 Unity Editor 编译并运行 focused tests，再如实记录结果；
- 风险：runtime carrier 的生命周期与 `MMSingleton` 的实际 Editor/Play Mode 行为尚未经 Unity 编译/测试验证；P4 MapId/Bootstrap 接线仍不在本 Change。

### 首次 Unity 编译失败 — 2026-08-25

- 已通过当前已打开的 Unity Editor 执行 force refresh / compile；
- Unity 报告 `Assets/NTSD/Scripts/Test/Editor/BattleMapBoundaryRuntimeSourceEditorTests.cs(135,65): CS1503`：当前项目使用的 NUnit 版本没有接受 `BoundaryWall` 对象的 `Does.Not.Contain(...)` 重载；
- 错误仅来自 MAPCFG-002 新增 focused test；同次 MCP script validation 对 `BoundaryWall`、`BoundaryWallManager` 和该 test 均无语法/静态诊断错误，Manager 仅有既存 `Update()` 字符串拼接提示；
- 下一步只允许把该单个集合断言换成同义、兼容当前 NUnit 的对象比较，不改变 production source、测试数据、预期或 P2 合同；修复后必须重新 Unity refresh / compile。

### 修复后 Unity compile 与 focused EditMode 结果 — 2026-08-25

- 修复后的 `BattleMapBoundaryRuntimeSourceEditorTests.cs` 仅将两处 NUnit collection constraint 替换成 test-local `ContainsBoundary(...)`；
- 已通过当前已打开的 Unity Editor 再次 force refresh / compile；`Assembly-CSharp-Editor.dll` 时间为 15:45:56，晚于 P2 test 源文件时间 15:45:00；`Assembly-CSharp.dll` 与 Editor assembly 均已更新；
- MCP script validation 对 P1/P2 六个相关脚本返回 0 errors；`BoundaryWallManager` 仅报告其既有 `Update()` 字符串拼接 warning，不是本 Change 新增诊断；
- focused EditMode job `850175a9e86141f680f03e2bcb26f7b5`：3 total、3 passed、0 failed、0 skipped：
  - `LoadedBoundaryDefinition_PreservesExistingManagerQueryResults`；
  - `FailedLoad_LeavesTheCurrentAssetSourceUntouched`；
  - `ClearLoadedBoundaryDefinition_RestoresExistingSceneFallbackOnlyWhenRequested`。
- 此结果证明 P2 数据源接线与 focused contracts 通过；尚未证明 P4 MapId/Bootstrap 接线、真实 Battle Scene 的某个地图 Asset 或 Play Mode 行为。

### 未运行项

- Play Mode：未运行。P2 没有创建默认 Asset 或把 MapId 接入 Bootstrap，不能冒充实际地图选择的 Play Mode 验收；
- P3 Editor authoring 与 P4 MapId / presentation 接线：未开始。

### Existing BattleRuntimeSelfCheck 回归 — 2026-08-25

- 通过现有 Unity Editor 菜单 `NTSD/验证/运行战斗运行时自检` 运行；不进入 Play Mode、不保存 Scene；
- 项目既有结果文件 `Temp/NTSD_BattleRuntimeSelfCheck.result` 于 15:53:47 写入 `PASS`；
- 随后的 MCP Console 查询发生连接等待，因此不以 Console 回包作为此项证据；结果文件是自检入口本身的正式 PASS 输出；
- 该回归不覆盖 P4 的 MapId / Catalog / Bootstrap 接线，也不构成真实生产地图 Asset 的 Battle Scene 或 Player 验收。

### 最终治理校验 — 2026-08-25

- `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <repository-root>` 已通过：103 条 Change Record、138 个当前 governed code diff 均有覆盖；
- scoped `git diff --check` 已通过；输出的 LF→CRLF 提示不属于 whitespace error；
- 静态 call-site audit 确认 `TryLoadBoundaryDefinition` / `ClearLoadedBoundaryDefinition` 目前只被 Manager 自身与 MAPCFG-002 focused test 调用；`Assets/NTSD/Scripts/App` 和 `Simulation` 尚无 `BattleMapBoundaryDefinition` 接线；
- P2 保持 `RUNTIME_PENDING`：实现、编译、focused contracts、existing self-check 和治理均已完成，但没有生产 MapId/Bootstrap/Scene/Player 集成，不能写为完整地图功能已验收。
