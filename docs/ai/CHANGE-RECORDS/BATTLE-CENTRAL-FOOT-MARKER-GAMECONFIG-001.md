# BATTLE-CENTRAL-FOOT-MARKER-GAMECONFIG-001 — Foot Marker 资源改由 GameConfig 配置

<!-- CHANGE-RECORD
id: BATTLE-CENTRAL-FOOT-MARKER-GAMECONFIG-001
status: COMPILE_PASS
code-path: Assets/NTSD/Scripts/App/GameConfig.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralEditorPreview.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCentralEditorPreviewEditor.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCentralEditorPreviewEditorTests.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCentralRuntimeFootMarkerEditorTests.cs
asset-path: Assets/NTSD/Config/GameConfig/GameConfig.asset
asset-path: Assets/NTSD/Scene/NTSD_Battle.unity
authority: USER-REQUEST-FOOT-MARKER-RESOURCE-FROM-GAMECONFIG-2026-09-13
evidence: PRECHANGE_STATIC_TRACE_HARDCODED_EDITOR_PATH_AND_SCENE_SPRITE_REFERENCE_CONFIRMED; RUNTIME_BUILD_0_ERRORS; EDITOR_BUILD_0_ERRORS; STATIC_CONFIG_CONTRACT_PASS; CHANGE_LEDGER_PASS_460_RECORDS_5_GOVERNED_FILES; UNITY_FOCUSED_AND_PLAY_PENDING_NO_PIPELINE_INSTANCE
-->

> 创建日期：2026-09-13  
> 当前状态：`COMPILE_PASS / STATIC_CONTRACT_PASS / UNITY_FOCUSED_PENDING / PLAY_PENDING`

## 需求与已观察原状

用户要求确认中央渲染 Foot 图片路径是否写死；若写死，则改为由 `GameConfig` 配置。

已观察到两条旧资源入口：

- `BattleCentralEditorPreview` 和其自定义 Inspector/离屏验证脚本包含
  `Assets/NTSD/Sprite/UIPanels/FootSelf.png` 常量路径，并通过 `AssetDatabase` 加载。
- `NTSD_Battle.unity` 的 Preview 组件另行序列化同一 Sprite GUID，Player Build 运行时从该场景字段取得资源。

因此旧实现不是纯运行时字符串加载，但资源所有权分散，且编辑器路径确实写死。

## 计划范围与不变量

1. 在 `GameConfig` 新增 Foot Marker Sprite 配置，并在生产 `GameConfig.asset` 绑定当前
   `FootSelf.png`。
2. Preview/运行时从 `GameConfig` 解析 Sprite；移除 Foot Marker 固定路径与场景内独立 Sprite 字段。
3. 保留 Preview 上已有 `drawFootMarkers`、64×24、offset、tint；不改变 Self 判定、地面锚点、
   尺寸缩放、批次顺序、材质或战斗逻辑。
4. Editor 测试和离屏验证通过 `GameConfig` Sprite 取资源，测试临时 Sprite 只经临时
   `GameConfig` 注入。

## 验收标准

- 全仓库 Foot Marker 生产代码不再包含 `Assets/NTSD/Sprite/UIPanels/FootSelf.png` 字符串。
- 生产 `GameConfig.asset` 的 Foot Marker 字段绑定现有 FootSelf GUID。
- `NTSD_Battle.unity` Preview 不再保存 `footMarkerSprite`，只保存 GameConfig 引用和样式。
- 外部 Unity C# 工程编译 0 error；相关 focused tests 在可用 Unity Editor 中通过。
- `Tools/Validate-ChangeLedger.ps1` 通过；未验证的 Play/Unity 项如实保留。

## 回滚方式

仅回退本 Change 涉及的上述脚本、`GameConfig.asset` 和 Scene 字段，并将状态记为
`ROLLED_BACK`。不得影响用户现有未跟踪的 `Assets/NTSD/Sprite/UIPanels/BattleHud/Foot/` 内容。

## 实际修改

- `GameConfig` 新增 `FootMarkerSprite`，生产 `GameConfig.asset` 绑定既有 FootSelf GUID。
- `BattleCentralEditorPreview` 移除 `DefaultFootMarkerPath` 和序列化的
  `footMarkerSprite`；`ResolveFootMarkerSprite()` 改为优先读取 Preview 指定的
  `GameConfig`，否则读取 `GameConfig.Instance`。
- Preview 的 `Reset()` 仅在 Editor 中且项目只有一个 `GameConfig` 资产时按类型发现并填入
  配置资产，不再知道 FootSelf 的具体路径。
- 自定义 Inspector 的示例配置和离屏验证改为读取 `GameConfig.FootMarkerSprite`；原尺寸、
  offset、tint、Shadow/actor/health 顺序不变。
- Scene 的 `footMarkerSprite` 字段替换为生产 `GameConfig` 引用。测试几何夹具保留
  Editor-only、非序列化 Sprite override；运行时 authoring 测试改为临时 `GameConfig` 注入，
  覆盖正式资源来源。

## 实际验证

- `dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly`：exit 0，
  0 error，47 warnings。
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly`：exit 0，
  0 error，104 warnings。
- PowerShell 静态配置合同：PASS；确认生产脚本无 FootSelf 固定路径、GameConfig GUID 绑定正确、
  Scene 不再保存 `footMarkerSprite` 且 Preview 引用生产 GameConfig。
- `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <repo>`：exit 0，PASS，460 Records，
  当前 5 个 governed code files 全部被本 Record 覆盖。
- `git diff --check`：exit 0；仅输出仓库现有行尾转换 warning。
- Unity CLI 1.0.0-beta.5 `unity status --format json --project-path <repo>`：
  `STATUS_NO_INSTANCES`。机器上同时存在多个 Unity 进程，未启动第二实例；因此 Unity focused
  tests、真实 Play 和 Scene in-memory dirty 状态均未验证，不能将状态提升为
  `FOCUSED_TEST_PASS` 或 `VERIFIED`。
