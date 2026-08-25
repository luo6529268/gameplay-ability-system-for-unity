# MAPCFG-004 — Map ID 启动配置与背景接线

<!-- CHANGE-RECORD
id: MAPCFG-004
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/App/BattleBootstrap.cs
code-path: Assets/NTSD/Scripts/App/AppManager.cs
code-path: Assets/NTSD/Scripts/Test/BattleTestBootstrap.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleMapStartupConfigurationEditorTests.cs
authority: USER-DIRECTED-20260825 / UNITY-NATIVE-MAP-CONFIGURATION / EXISTING-BOUNDARYWALL-SEMANTICS
evidence: PREIMPLEMENTATION-READONLY-AUDIT-STARTUP-ORDER-BACKGROUND-OWNER-AND-ASSET-INVENTORY-20260825 / CODE-WRITTEN-OPTIONAL-STARTUP-GATE-20260825
-->

> Change ID：MAPCFG-004  
> 状态：FOCUSED_TEST_PASS / RUNTIME_PENDING / DEPLOYMENT INPUT PENDING  
> 类型：Unity 启动配置接线；不改变 battle rule  
> 创建日期：2026-08-25  
> Authority：用户批准 P1–P4；只复用当前 Unity map/boundary/background interfaces，不需要 C++ Release 审计。

## 用户需求与依据

- 地图由 MapId、Boundary Asset 和 Presentation Asset 配置；
- 同一世界背景与可行走区域应在不同平台使用同一逻辑数据；
- 不能擅自修改当前 Scene、Bg、Camera、GameConfig 或选定正式 MapId；
- 所有脚本改动需要可恢复审计。

## 已观察到的前置事实

- 当前没有生产 Map Asset/Catalog 实例；
- `AppManager.InitializeBattleAsync` 与 `BattleTestBootstrap` 都在 Active Scene 后、角色创建前有启动窗口；
- `BattleBootstrap` 当前只管 presentation enable/disable；
- `BattleBackgroundPlatformPresentation` 直接以同一 world SpriteRenderer 作为 source；
- `SimulationWorld` 已从 BoundaryWallManager 的当前 source 取得 Stage runtime，故 map prepare 必须早于角色生成和 unpause。

## 既有脏文件基线（不归 MAPCFG-004）

- `AppManager.cs` 在本包开始前已存在 `R8-CENTRALSEAL-001` 的 presentation 时序 diff：`EnablePresentation()` 位于 `BeginBattleAllocationSeal()` 后、`SetPaused(false)` 前；
- `BattleTestBootstrap.cs` 在本包开始前也已存在同一 central-seal 时序与 `R8-FUNCTIONKEYMODE-001` 的 F7 保护 diff；
- MAPCFG-004 只允许分别在 `SceneManager.SetActiveScene(...)` 后、角色创建前插入 prepare gate；仅在本包确实装载了一个配置地图后才 refresh Stage snapshot。不得回退、重排或宣称拥有上述既有 diff。

## 允许修改的路径和符号

- `BattleBootstrap`：仅增加 optional map config、prepare/clear；
- `AppManager` / `BattleTestBootstrap`：仅在既有启动窗口调用 prepare 并 fail close；
- 新增 focused Editor test；
- 本 Record、Ledger、STATE、Task、Handoff 与父计划。

## 不可变合同

- 两项配置均为空时行为保持 legacy fallback，且不得触发 P4 新增的 Stage refresh；半配置或无效配置必须在 mutation 前失败；
- 有效配置必须先 resolve/validate 所有依赖，之后才一次加载 boundary、换 Bg sprite；
- 配置仅在启动前使用；不得在 tick 中查询 Catalog 或读 Asset；
- P4 不创建/save production assets/scene；
- 清理只能恢复本次 runtime mutation，不能写回 serialized source。

## 明确 deployment blocker

正式 `MapId`、Catalog、Boundary Asset、Presentation Asset 与 Bg Renderer 引用尚未由用户配置；本包只实现配置入口和测试夹具。没有这些用户选择，不得宣称真实 Battle Scene MapId 加载已完成。

## 实际脚本改动

- `Assets/NTSD/Scripts/App/BattleBootstrap.cs`
  - 新增可选的 `BattleMapCatalog`、`MapId`、`BoundaryWallManager` 与同一 world `SpriteRenderer` 引用；默认都为空，故现有 Scene 没有配置时仍走 legacy fallback。
  - 新增 `TryPrepareMapConfiguration(out string failure)`：空配置成功且零 mutation；半配置、Catalog resolve 失败、缺少 Manager/Renderer 或背景 Sprite 都在 boundary/Bg mutation 前 fail-close；有效配置先显式装载 P2 BoundaryDefinition，再仅替换同一 world Bg 的 `sprite`。
  - 新增 `ClearPreparedMapConfiguration()`：只清理由本实例成功装载的同一 BoundaryDefinition，并还原本次运行前的 Bg sprite；`DisablePresentation()` 走该清理。
  - 不写 Camera、Transform、PPU、platform overlay、tick、输入、几何算法或 serialized Asset/Scene。
- `Assets/NTSD/Scripts/App/AppManager.cs`
  - 在 Active Scene 已建立、角色创建前调用 prepare；失败抛出并阻止后续角色创建/解除暂停。
  - 仅当 `IsMapConfigurationPrepared` 为真时 refresh Stage runtime snapshot；空配置完全保留旧启动行为。
  - 保留既有 `R8-CENTRALSEAL-001` 的 `BeginBattleAllocationSeal()` → `EnablePresentation()` → `SetPaused(false)` 顺序。
- `Assets/NTSD/Scripts/Test/BattleTestBootstrap.cs`
  - 采用相同的 prepare/fail-close/仅已装载地图才 refresh Stage 的启动边界；保留既有 central-seal 与 F7 diff。
- `Assets/NTSD/Scripts/Test/Editor/BattleMapStartupConfigurationEditorTests.cs`
  - 新增内存 fixture，覆盖空配置保持原 source/Bg、有效配置装载并 clear 还原、无效 MapId 与两种半配置均在 mutation 前失败。
- 未创建或修改真实 Map Asset、Catalog、MapId、Scene、Bg 资源、GameConfig、Camera、DAT、C++ 或服务器文件。

## 验证记录

- 已完成静态 code review；发现“无配置也 refresh Stage snapshot”会违反 legacy fallback 合同，已在编译前最小修正为“仅真实 P4 map prepare 成功才 refresh”。
- Unity 当前 Editor 已正常 import；`Assembly-CSharp.dll` 与 `Assembly-CSharp-Editor.dll` 的 UTC 写入时间分别为 `2026-08-25T09:24:38.9557211Z`、`2026-08-25T09:24:39.5607986Z`，均晚于 P4 四个源文件；新测试 `.meta` 由 Unity 于 `2026-08-25T09:24:27.9763144Z` 生成。
- P4 focused EditMode job `51942ac652474e6c9ba42427a93ba44a`：4/4 PASS，覆盖空配置、有效 load/clear、无效 MapId、Catalog/MapId 两种半配置的 mutation guard。
- P1–P4 cross-phase EditMode job `50c3e1586f5145e18b6d990662b920b0`：14/14 PASS，覆盖数据模型、catalog fail-close、runtime carrier source、Scene authoring bridge 与 P4 startup prepare。
- 项目既有菜单 `NTSD/验证/运行战斗运行时自检` 已实际触发；`Temp/NTSD_BattleRuntimeSelfCheck.result` 于 `2026-08-25T09:33:25.9588182Z` 写入 `PASS`。该 self-check 证明既有 battle runtime 基线未在当前未配置地图场景中回退，不能替代真实 P4 production MapId/Play 验收。
- static audit 未发现 P4 脚本的 Asset/Scene/file persistence API，且生产 Map Asset inventory 仍为空；`Tools/Validate-ChangeLedger.ps1` 已通过，输出为 105 条 Record、141 个当前 governed code diff 全部 covered。
- 本次文档收口后的 scoped `git diff --check` 已通过；P4 新增测试也没有 trailing whitespace。Git 仅提示三个既有 tracked C# 文件 LF→CRLF 的工作树行尾预警，无 whitespace error。
- 未运行 Play Mode/Player；仍缺用户配置的正式 MapId/Catalog/Boundary Asset/Presentation Asset/Bg Renderer，故真实 Battle Scene/Player 部署验收继续 pending。
