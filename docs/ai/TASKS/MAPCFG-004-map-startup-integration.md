# MAPCFG-004 — Map ID 启动配置与背景接线

> 状态：FOCUSED_TEST_PASS / RUNTIME_PENDING / DEPLOYMENT INPUT PENDING  
> 日期：2026-08-25  
> 父计划：BATTLE-MAP-BOUNDARY-ASSET-001  
> 前置：MAPCFG-001 = FOCUSED_TEST_PASS；MAPCFG-002 = RUNTIME_PENDING；MAPCFG-003 = FOCUSED_TEST_PASS

## Goal

在不改变战斗 tick、背景平台表现或现有 Scene fallback 的前提下，让 `BattleBootstrap` 可配置一个 `BattleMapCatalog + MapId + Bg SpriteRenderer`，并在角色生成与解除暂停之前显式载入该 MapId 对应的 Boundary Asset 和 Presentation Sprite。

## Observed Deployment Input

- 当前 `Assets/NTSD` 下未发现任何实际 `BattleMapBoundaryDefinition`、`BattleMapPresentationDefinition` 或 `BattleMapCatalog` `.asset`；
- 因此本包不能自行决定正式 MapId、把当前 Scene wall 写入 Asset，或选择/覆盖背景资源；
- P4 可以完成通用启动接口与 in-memory focused tests；真实资产部署和 Play Mode 验收需要用户在 P3 Inspector 中创建/选择资产并明确 MapId 后再进行。

## Scope

- `BattleBootstrap` 增加可序列化但默认空的 Catalog、MapId、Bg Renderer 引用；
- 增加 `TryPrepareMapConfiguration(out failure)`：
  - Catalog 与 MapId 均未配置时保留 legacy Scene fallback；
  - 仅其中一项配置、Catalog resolve 失败、missing boundary manager、missing Bg Renderer 或 missing presentation sprite 时 fail closed；
  - 成功时先让 Manager 载入 BoundaryDefinition，再赋值同一 world Bg Renderer 的 sprite；
  - 不改 Camera、Transform、PPU、platform overlay 或 simulation state；
- `AppManager.InitializeBattleAsync` 和 `BattleTestBootstrap` 都在 Active Scene 建立后、角色创建前调用准备接口；失败时不得创建角色或解除暂停；
- 仅当成功装载了非空 MapId 配置时刷新已有 Stage runtime snapshot，再进行原有角色生成；空配置不新增 Stage refresh，以保持 legacy 启动行为；
- `DisablePresentation` / 卸载时显式清理 runtime boundary source 并还原本次运行前的 background sprite；
- 新增 focused EditMode tests，使用内存 ScriptableObject/Sprite/Texture fixtures，不写磁盘资产或场景。

## Existing Behavior to Preserve

- 无 Map 配置时现有 `BoundaryWallManager` Scene fallback、Bg 和 battle startup 逻辑；
- P2 explicit loaded source 不与 Scene fallback 混合；
- P3 explicit authoring API、Undo/dirty/no-save 行为；
- `BattleBackgroundPlatformPresentation` 对 world Bg 的平台 Camera/black overlay 行为；
- 所有 battle geometry、input、AI、tick、checksum、network 语义。

## Explicitly Out of Scope

- 不创建真实 Map Asset、Catalog、MapId 或默认边界数据；
- 不写 `NTSD_Battle.unity`、`GameConfig.asset`、Bg sprite、背景 Transform、Camera、PPU、Android/Windows platform 配置；
- 不在战斗中切图或换边界；
- 不接服务器、lockstep packet、fingerprint、C++、DAT 或 gameplay 修改；
- 不运行 Player build 或 Play Mode，除非用户提供实际配置后作为单独验收。

## Files Likely Involved

- Assets/NTSD/Scripts/App/BattleBootstrap.cs
- Assets/NTSD/Scripts/App/AppManager.cs
- Assets/NTSD/Scripts/Test/BattleTestBootstrap.cs
- Assets/NTSD/Scripts/Test/Editor/BattleMapStartupConfigurationEditorTests.cs
- docs/ai/CHANGE-RECORDS/MAPCFG-004-map-startup-integration.md
- docs/ai/CHANGE-LEDGER.md
- docs/ai/STATE.md
- 当前计划与 Handoff

## Acceptance / Verification

1. 无配置时 `TryPrepareMapConfiguration` 成功但不改 Bg 或 Boundary source；
2. 有效内存 Catalog+MapId 会在 Map prepare 后出现正确 `LoadedBoundaryDefinition` 和 Bg sprite；
3. 不完整/无效配置失败前不改变旧 boundary source 或 Bg sprite；
4. clear 还原原背景并恢复 Scene fallback；
5. 启动代码调用位置在 Active Scene 后、角色创建前、解除暂停前；
6. Unity compile、focused EditMode tests、Ledger validator、scoped diff check 通过；
7. 真实 Asset + Scene + Play Mode 验收留为 deployment blocker，直到用户配置 MapId/Assets。

## Stop Conditions

- 需要猜测正式 MapId、自动创建/保存 Asset、覆盖当前 Scene/Bg 或修改 GameConfig 才能继续；
- Map load 不能在角色创建前完成而需要改 tick/pass 顺序；
- 需要改变平台背景表现、几何算法、C++、网络或 battle rule；
- 用户提供新的 Change Request。

## Current Progress

- 只读确认 AppManager 和 BattleTestBootstrap 均在角色创建前存在可插入的 Active Scene 后准备窗口；
- 只读确认 Background platform presenter 继续以 `Bg (2)` 的同一 SpriteRenderer 为源，P4 只应换 sprite 引用；
- 已写 optional `BattleBootstrap` prepare/clear、两条 startup gate 和四项 in-memory focused tests；没有修改 Scene、Asset 实例、DAT、C++、Camera、Transform、PPU、platform presentation 或 battle rule。
- 空配置路径已明确不调用 P4 新增的 Stage refresh；有效配置才在角色创建前 refresh Stage snapshot。
- Unity import/compile 已完成；P4 job `51942ac652474e6c9ba42427a93ba44a` 为4/4 PASS，P1–P4 job `50c3e1586f5145e18b6d990662b920b0` 为14/14 PASS；现有 self-check result 于17:33:25写入PASS。
- `Tools/Validate-ChangeLedger.ps1` 已通过（105 Records / 141 governed code diff covered）；P4 scoped `git diff --check` 也已通过（仅既有 LF→CRLF 提示，无 whitespace error）。
- 之后 P4 仅等待用户创建/指定真实 MapId、Catalog、Boundary/Presentation Asset、Manager 和 Bg Renderer 引用，再独立进行真实 Scene/Play 验收。
