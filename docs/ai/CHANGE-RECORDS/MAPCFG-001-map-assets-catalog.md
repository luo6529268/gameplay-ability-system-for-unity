# MAPCFG-001 — Map ID、Boundary Asset 与 Presentation Asset

<!-- CHANGE-RECORD
id: MAPCFG-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/LevelEditor/BattleMapBoundaryDefinition.cs
code-path: Assets/NTSD/Scripts/LevelEditor/BattleMapPresentationDefinition.cs
code-path: Assets/NTSD/Scripts/LevelEditor/BattleMapCatalog.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleMapAssetConfigurationEditorTests.cs
authority: USER-DIRECTED-20260825 / UNITY-NATIVE-MAP-BOUNDARY-CONFIGURATION / EXISTING-BOUNDARYWALL-SEMANTICS
evidence: UNITY-COMPILE-20260825-150119 / EDITMODE-a0b70302cb314b0cbb0a6b6d3fee0457-4OF4-PASS
-->

> Change ID：MAPCFG-001  
> 状态：FOCUSED_TEST_PASS / RUNTIME_NOT_CONNECTED  
> 类型：Unity 配置数据模型与 Editor-only 验证  
> 创建日期：2026-08-25  
> Authority：用户明确要求；复用现有 Unity BoundaryWall / BoundaryWallManager 语义，不新增 C++ battle rule。

## 用户需求与依据

用户确认：

- 可行走区域是任意多边形；
- 该行为就是现有 BoundaryWall 和 BoundaryWallManager 正在处理的行为；
- 一份 Asset 使用 MapId 保存边界数据；
- 另一份 Asset 使用同一 MapId 保存地图表现资源；
- 后续通过 MapId 加载；
- 每次脚本修改必须保留可恢复审计痕迹。

本 Change 不审计或修改 C++ Release，因为它不改变战斗规则；只建立现有 Unity 多边形数据的配置载体。

## Unity 修改前现状

- BoundaryWall 在 Unity world X/Y 平面保存多个局部 polygon；
- BoundaryWallManager 提供点、矩形、随机点和 Stage 外接 bounds 查询；
- BoundaryWallManager 的 JSON 导出已经使用 BoundaryData、PolygonData、Vector2Data 保存多个边界组和 world X/Y 顶点；
- 当前没有 MapId、Boundary Asset、Presentation Asset 或按 MapId 的配对表；
- 当前没有脚本变更属于本 Change。

## 允许修改的路径和符号

- Assets/NTSD/Scripts/LevelEditor/BattleMapBoundaryDefinition.cs
- Assets/NTSD/Scripts/LevelEditor/BattleMapPresentationDefinition.cs
- Assets/NTSD/Scripts/LevelEditor/BattleMapCatalog.cs
- Assets/NTSD/Scripts/Test/Editor/BattleMapAssetConfigurationEditorTests.cs
- 对应 metadata 由现有 Unity Editor 正常导入时生成；
- 本 Record、Ledger、STATE、Task、Handoff 与父计划。

## 目标职责

### BattleMapBoundaryDefinition

- 保存稳定 MapId、显示名、revision 和现有 JSON 形状的边界 world X/Y 数据；
- 做结构完整性校验；
- 不执行任何 point-in-polygon、矩形判断、随机采样或 runtime 加载。

### BattleMapPresentationDefinition

- 保存同一 MapId、显示名、背景 Sprite 和可选表现资源；
- 不持有或修改 boundary 数据。

### BattleMapCatalog

- 在启动前或 Editor validation 时按 MapId 配对两类 Asset；
- 拒绝重复、缺失、MapId 不匹配或无效 Asset；
- 不接入 SimulationWorld、GameConfig、Scene 或 tick。

### Focused tests

- 只创建内存 ScriptableObject fixture；
- 验证数据契约和 Catalog fail-closed；
- 不启动 Play Mode、不写磁盘 Asset、不保存 Scene。

## 明确禁止

- 不修改 BoundaryWall、BoundaryWallManager、SimulationWorld、BattleBootstrap、GameConfig、Camera、Bg；
- 不改 Scene、背景资源、DAT、项目配置、C++ 或服务器；
- 不将 polygon 变成矩形；
- 不添加新的碰撞/移动/AI/hit 规则；
- 不增加每 tick AssetDatabase、Scene scan、List 或 Dictionary 分配。

## 预期副作用和不可回退边界

- 新增类型会出现在 Create Asset 菜单和 Inspector；
- P1 不创建默认 Asset，故不会改任何现有场景或运行时数据来源；
- 删除本 Change 的新 C# 文件即可恢复到当前 runtime 行为，但删除操作必须由后续明确授权执行；
- 现有 BoundaryWall JSON 类型保持原样，不迁移、不重命名。

## 验收标准

1. 合法 MapId 配对可被 Catalog 精确解析；
2. 重复 MapId、Asset MapId 不匹配、缺失 boundary、非法 polygon 顶点数或非有限世界坐标会 fail closed；
3. Presentation Asset 变化不会改动 Boundary Asset 数据；
4. Unity compile 0 error；
5. 新 focused EditMode tests 通过；
6. Tools/Validate-ChangeLedger.ps1 和 scoped diff 检查通过。

## 回滚方式

本阶段不改已有脚本、Scene、Asset 或配置。若要回滚，只需在获得明确授权后删除本 Change 新增的类型和测试文件，并移除本 Change 的 Ledger/State/Handoff 后续更正记录；当前不执行任何删除。

## 实际脚本改动

- 新增 Assets/NTSD/Scripts/LevelEditor/BattleMapBoundaryDefinition.cs：
  - BattleMapBoundaryDefinition 保存 MapId、显示名、revision 和 List<BoundaryData>；
  - 继续使用 BoundaryData、PolygonData、Vector2Data 的 world X/Y JSON 数据形状；
  - 只校验 MapId、边界组、polygon 顶点数和非有限顶点；未实现新的几何算法。
- 新增 Assets/NTSD/Scripts/LevelEditor/BattleMapPresentationDefinition.cs：
  - BattleMapPresentationDefinition 保存相同 MapId、显示名、BackgroundSprite 和可选 decorationPrefabs；
  - 不引用或写入 boundary 数据。
- 新增 Assets/NTSD/Scripts/LevelEditor/BattleMapCatalog.cs：
  - Entry 保存 MapId、BoundaryDefinition、PresentationDefinition；
  - TryValidate 和 TryResolve 在 preflight/Editor 阶段 fail closed 检查空项、重复 MapId、无效 boundary、无效 presentation 和三方 MapId 不一致；
  - 未接入 BattleBootstrap、GameConfig、SimulationWorld 或运行时 tick。
- 新增 Assets/NTSD/Scripts/Test/Editor/BattleMapAssetConfigurationEditorTests.cs：
  - 只创建内存 ScriptableObject fixture；
  - 覆盖 world X/Y 数据保留、顶点结构失败、准确配对、重复 MapId 和不匹配 MapId fail closed；
  - 不写磁盘 Asset、不启动 Play Mode、不保存 Scene。
- 未生成或手写 metadata；由当前 Unity Editor 的正常导入流程处理。

## 改前/改后职责

| 范围 | 改前 | 改后 |
|---|---|---|
| 地图边界数据 | 仅能作为 Scene BoundaryWall 或 JSON 导出数据存在。 | 可以作为独立 MapId Boundary Asset 存在，但尚未接入 runtime。 |
| 地图表现资源 | 没有与 MapId 配对的数据载体。 | 可以作为独立 MapId Presentation Asset 存在，但尚未驱动 Bg。 |
| 地图选择 | 没有可校验的 MapId 配对表。 | Catalog 可以纯数据方式 fail closed 解析配对，但尚未被 Bootstrap 调用。 |
| 几何逻辑 | BoundaryWall/Manager 已有实现。 | 未改动。 |

## 验证记录

- 尚未运行 Unity compile 或 EditMode test；
- 尚未运行 BattleRuntimeSelfCheck 或 Play Mode，因为 P1 尚未接入 runtime；
- 下一步：检查 scoped diff，使用现有 Unity Editor refresh/import，运行新的 focused EditMode tests，再运行 Ledger validator。

### 首次 Unity 编译失败 — 2026-08-25

- 已通过当前 Unity Editor 的 MCP refresh 强制导入新文件；四个新 C# metadata 已由 Unity 正常生成；
- Unity 实际编译失败：Assets/NTSD/Scripts/LevelEditor/BattleMapPresentationDefinition.cs 第 18 行，CS0246，IReadOnlyList 未找到；
- 原因：该文件引用 IReadOnlyList 但缺少 System.Collections.Generic using；
- 该错误来自本 Change 的新文件；日志未报告其他 MAPCFG-001 新文件错误；
- 下一步只允许在该文件补一个 using，不改任何类型、字段、行为或既有脚本。修复后必须重新 refresh/compile。

### 编译修复已写入 — 2026-08-25

- 已在 BattleMapPresentationDefinition.cs 顶部补充 using System.Collections.Generic；
- 没有改动任何字段、方法、Asset 数据契约、BoundaryWall 几何、Scene 或 runtime；
- 已完成真实 Unity 重新编译，见下方结果。

### Unity compile 与 focused EditMode 结果 — 2026-08-25

- 使用已打开的 Unity 2022.3.62f3 Editor，通过 MCP force refresh 的 all scope 正常导入四个新 .cs 文件；Unity 自动生成四个对应 .meta；
- 第二次真实 Unity 编译后的 Assembly-CSharp.dll 与 Assembly-CSharp-Editor.dll 时间为 15:01:19，且晚于本 Change 源文件；
- Unity Console 中没有 MAPCFG-001 的 C# compile error。error 查询仅包含 MCP client handler exited 连接关闭日志，不是项目脚本异常；
- MCP validate_script 对四个新文件均返回 0 errors / 0 warnings；
- focused EditMode job a0b70302cb314b0cbb0a6b6d3fee0457：4 total、4 passed、0 failed、0 skipped：
  - BoundaryDefinition_PreservesExistingWorldVertexExportShape；
  - BoundaryDefinition_RejectsMalformedWorldVertices；
  - Catalog_FailsClosedForDuplicateAndMismatchedMapIds；
  - Catalog_ResolvesExactMatchingPairWithoutMutatingBoundaryData。
- 先前 job 81efcb78d96e43638ff670632751df94 只运行了 NTSD 根节点并返回 total 0；它不计入验证结论。

### 留痕与差异校验 — 2026-08-25

- `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <repository-root>` 已实际通过：102 条 Change Record、135 个当前 governed code diff 均被记录覆盖；
- scoped `git diff --check` 已实际通过；
- 该 validator 通过只说明本次脚本 diff 已被 MAPCFG-001 以及既有 Change Record 正确留痕，不把 P1 提升为 runtime 或 Battle Scene 验收；
- P1 保持 `FOCUSED_TEST_PASS / RUNTIME_NOT_CONNECTED`，P2 仍需独立建立新的 Task Contract 和 Change Record 后才可改动现有 BoundaryWall / BoundaryWallManager。

### 未运行项

- BattleRuntimeSelfCheck：未运行。P1 尚未把 Asset 接入 BoundaryWallManager 或 battle runtime，现有 self-check 不会覆盖新数据源；
- Play Mode：未运行。P1 只建立内存 Asset 数据模型；实际地图加载和 Battle Scene 行为留给 P2/P4。
