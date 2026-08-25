# MAPCFG-003 — Boundary Asset 显式 Editor 作者工具

<!-- CHANGE-RECORD
id: MAPCFG-003
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/LevelEditor/BattleMapBoundaryDefinition.cs
code-path: Assets/NTSD/Scripts/LevelEditor/BoundaryWall.cs
code-path: Assets/NTSD/Scripts/LevelEditor/BoundaryWallManager.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleMapBoundaryAuthoringEditorTests.cs
authority: USER-DIRECTED-20260825 / UNITY-NATIVE-EXPLICIT-AUTHORING / EXISTING-BOUNDARYWALL-SEMANTICS
evidence: UNITY-EDITOR-COMPILE-20260825-161924 / EDITMODE-63182377db004cb084fc830402bbb878-10OF10-PASS / SELFCHECK-PASS-20260825-162635
-->

> Change ID：MAPCFG-003  
> 状态：FOCUSED_TEST_PASS / P4 READY / MANUAL-INSPECTOR PENDING  
> 类型：Unity Editor-only authoring bridge；不改变 battle rule  
> 创建日期：2026-08-25  
> Authority：用户确认 P1–P4；复用现有 `BoundaryWall` / `BoundaryWallManager` 语义，不需要 C++ Release 审计。

## 用户需求与依据

- 不同 MapId 需要各自的任意多边形可行走区域；
- Scene View 中编辑的区域必须和将来运行时使用的 Boundary Asset 同源；
- 每次脚本变更均要有可恢复审计痕迹；
- 用户明确不希望自动覆盖、Scene 保存、背景或游戏规则的无关改动。

## Unity 修改前现状

- P1 Boundary Asset 可以验证数据，但不能从 Scene 显式 Apply / Load；
- P2 可以将 Asset 装载为 transient runtime source，但该 source 不适合直接作为 Editor authoring wall；
- `BoundaryWallEditor` 已能编辑单 wall 的 local vertices；
- `BoundaryWallManager` JSON export 已能把 enabled Scene walls 展开为 `BoundaryData` 的 world X/Y 结构；
- 现有 Manager custom inspector 已有默认字段、JSON export 和 manual refresh。

## 允许修改的路径和符号

- `BattleMapBoundaryDefinition`：只新增 editor authoring 所需的深复制/replace validation bridge；
- `BoundaryWall`：只新增 world X/Y capture bridge；
- `BoundaryWallManager`：只新增 authoring Asset 引用、preflight、explicit load/apply 和现有 custom inspector buttons；
- 新增 focused Editor test；
- 本 Record、Ledger、STATE、Task、Handoff 与父计划。

## 不可变合同

- 不重写 polygon/point/rect/Stage 算法；
- 不修改 P2 runtime carrier 的加载方式；
- Load / Apply 必须由用户明确点击或明确 API 调用触发，绝不自动执行；
- 不调用保存 API；只记录 Undo 和 dirty；
- 失败前必须 preflight，失败后 Asset 与 Scene wall 均保持不变；
- loaded runtime source 必须阻止 authoring write，直到明确 clear。

## 预期副作用和不可回退边界

- Manager Inspector 多出 authoring Boundary Asset 引用、MapId 信息和两个显式按钮；
- 用户点击按钮后才会使 Scene 或 Asset 变为 dirty，仍由用户决定保存或 Undo；
- P3 不会创建/写入真实地图 Asset 或修改当前 dirty Scene；
- 如需回滚，必须先获明确授权，禁止以删除/reset/覆盖用户工作树实现。

## 验收标准

1. world X/Y round trip 与 deep-copy contract 通过；
2. name/count mismatch 与 active runtime source fail closed；
3. custom inspector 按钮只调用显式 API，不保存；
4. Unity compile、focused test、Ledger validator、scoped diff 有真实结果。

## 实际脚本改动

- `BattleMapBoundaryDefinition.TryReplaceBoundariesFromAuthoring(...)`：在 Editor-only 编译域中对 source boundary collection 深复制、验证后才原子替换 Asset 的 list；不自动增加 revision、不保存 Asset。
- `BoundaryWall.TryCaptureWorldBoundaryData(...)`：在 Editor-only 编译域中把现有 local polygon 通过原有 Transform 展开为 `BoundaryData` world X/Y；没有改现有几何查询。
- `BoundaryWallManager`：新增 authoring Asset 引用、显式 Asset→Scene / Scene→Asset API、loaded runtime source guard、stable hierarchy+group name preflight、Undo/dirty；既有 Inspector 新增 MapId/Revision 和两枚需二次确认的按钮。
- 新增 `BattleMapBoundaryAuthoringEditorTests`：覆盖 world data round trip/deep copy、name mismatch fail-close、runtime carrier active guard。
- 未调用 `AssetDatabase.SaveAssets`、`EditorSceneManager.SaveScene`，未写 Scene/Asset 实例、Bootstrap、Camera、Bg、C++ 或 battle logic。

## 验证记录

- 只读预检完成；
- scoped `git diff --check` 已通过；static audit 证实 P3 API 仅由 Manager custom inspector 和 P3 focused test 调用，且四个 task-owned P3 files 中没有 `SaveAssets` / `SaveScene` API；
- Unity 当前 Editor 已重新编译：`Assembly-CSharp-Editor.dll` 时间为16:19:24，晚于 P3 test 源与 Unity 自动生成的 `.meta`；
- MCP validation 对 P3 四个相关脚本返回 0 error；Manager 仅报告其已有 `Update()` 字符串拼接 warning；
- focused EditMode job `5e4b965f9e7b4452a5c6e236117b673a`：3 total、3 passed、0 failed、0 skipped：
  - `ExplicitLoadAndApply_RoundTripsWorldDataWithoutSharingMutableVertices`；
  - `ExplicitLoad_FailsClosedWhenSceneBoundaryNamesDoNotMatch`；
  - `AuthoringOperations_FailClosedWhileRuntimeAssetSourceIsActive`。
- 未运行 Play Mode；P3 是 Editor-only authoring，P4 MapId/Bootstrap/production Asset integration 不在本 Change；下一步仍需重跑受影响的 P1/P2 focused tests 和治理校验。

### Cross-phase regression 与 existing self-check — 2026-08-25

- P1/P2/P3 精确 EditMode job `63182377db004cb084fc830402bbb878`：10 total、10 passed、0 failed、0 skipped；覆盖 P1 Asset/Catalog、P2 runtime source、P3 authoring 的全部当前 focused contracts；
- P3 写入后再次运行既有菜单 `NTSD/验证/运行战斗运行时自检`；项目既有 `Temp/NTSD_BattleRuntimeSelfCheck.result` 于 16:26:35 写入 `PASS`；
- 未运行 Play Mode：P3 的所有新增调用都在 `#if UNITY_EDITOR` 内，且当前没有 production Boundary Asset / Catalog / MapId bootstrap 配置。Inspector 按钮的真实用户点击与 P4 的实际地图集成必须后续单独验证。

### 最终治理校验 — 2026-08-25

- `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <repository-root>` 已通过：104 条 Change Record、139 个当前 governed code diff 都有覆盖；
- scoped `git diff --check` 已通过；输出的 LF→CRLF 提示不属于 whitespace error；
- P3 不得被写为 production MapId 加载完成；它只完成可测试的 Editor authoring bridge。
