# Task Contract — BATTLE-SCENEVIEW-HIERARCHY-VISIBILITY-001

> 状态：`VERIFIED / SCENEVIEW_ISOLATED / TRANSIENT_GAMEOBJECTS_HIERARCHY_VISIBLE`
> 来源：用户于 2026-09-06 明确要求，避免以后再次出现 Scene 面板可见、Hierarchy 面板却没有对应对象的战斗内容。

## 目标

停止 Play Mode 中 `BattleCentralRenderSystem` 向 Unity `SceneView` 相机提交没有逐对象
`GameObject` 的中央批量战斗像素；正式世界相机和 Game View 表现保持不变。

## 已确认根因

- `BattleRenderFeature.AddRenderPasses(...)` 会为每个 URP 相机尝试取得中央 submission。
- `BattleCentralRenderSystem.CanRenderCamera(...)` 当前明确接受
  `isPlaying && cameraType == CameraType.SceneView`。
- 中央 submission 使用 `CommandBuffer.DrawMesh(...)`，不是逐对象 GameObject，因此会形成
  Scene 可见、Hierarchy 无逐对象条目的表现。
- 当前 Edit Mode 场景根对象中没有隐藏的可见战斗 GameObject；`HideAndDontSave` 命中主要是
  Mesh/Material/Texture 等非 GameObject 资源以及隔离 benchmark。它们不是本次现象的生产根因。

## 允许路径

- `Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs`
- `Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCentralLatestFrameMaterializationEditorTests.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- `Assets/NTSD/Scripts/Test/Editor/BattleCentralSceneViewPixelPlayModeProbeEditor.cs`
- `Assets/NTSD/Scripts/Animation/Rendering/BattleRenderingBenchmark.cs`
- `Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleRenderingBenchmarkEditorTests.cs`
- 本 Task、Change Record、Ledger、STATE、handoff 和 NTSD 2.8 总表

## 不变量与验收

- Base world camera 仍可取得并绘制当前中央 submission。
- 其他 Game camera、Overlay camera、Play/Edit Mode SceneView 全部拒绝。
- SceneView 的拒绝必须发生在 submission lease 获取前，不能只靠清理一次现场。
- Edit Mode `BattleCentralEditorPreview` 的显式作者预览不改变；它有可见的 Hierarchy owner。
- benchmark runner、legacy presenter、render children 与 benchmark camera 如果是 GameObject，必须
  使用不含 `HideInHierarchy` 的瞬态 flags；Mesh/Material/Texture/RenderTexture 仍可
  `HideAndDontSave`。
- focused EditMode test、相关渲染测试、Unity 脚本编译、`BattleRuntimeSelfCheck`、真实 Play
  SceneView 无中央像素探针、Console、Scene SHA 与 Change Ledger 必须按风险闭合。
- 不修改 Scene、Prefab、DAT、资源、战斗逻辑、tick、输入、碰撞或权威 C++。

## 回滚

恢复 `CanRenderCamera(...)` 的 Editor Play Mode SceneView 例外，并同步恢复三处测试/探针预期；
不需要回滚任何 Scene 或内容资产。

## 最终证据

- red：job `44017ed85c12432b8a483f091a3f1a49`，旧 gate 令目标断言
  `Expected False / But was True`。
- compile：force refresh/domain reload 完成，Console C# error 0。
- green：job `473a2e0b6ea44203b4bb653b617f647c` 13/13；相关中央渲染/显式预览
  job `525b576a0bb24d50846532ab6baade7e` 30/30。
- SelfCheck：`Temp/NTSD_BattleRuntimeSelfCheck.result` 于 `2026-09-06T08:45:39Z` 为
  `PASS`；7 条输出均为既有 intentional negative-path diagnostics。
- 真实 Play：`Temp/NTSD_R8_WP01D_07_SceneViewPixels.result.json` 于
  `2026-09-06T08:48:49Z` 为 `PASS`；world frame 19 commands/7 segments，SceneView
  gate=false、lease=false、nonClearPixelCount=0，object 12→12、claimed slots 10→10。
- Play 已退出；最终清空后 Console error/warning 0。
- Change Ledger validator最终 PASS：325 records / 280 governed code files；scoped `git diff --check` PASS。
- benchmark red：job `6ed7c38453974ac492a59534a1d59787` 精确失败于
  `NTSD Benchmark Legacy Presenter` 带 `HideInHierarchy`。
- benchmark green：job `98c80e6f0ec543c4831a3ccc8411aba3` 1/1，完整 benchmark job
  `1359bba886624ac187009225353290f0` 38/38；runner/presenter/children/camera 统一走
  `BattleBenchmarkTransientGameObjectPolicy.Create`，flags=`DontSave`。
- 最终 SelfCheck 于 `2026-09-06T09:06:42Z` 再次 `PASS`。
- Scene 文件哈希检查发现 Unity Test Runner 把进入测试前已经 dirty 的 Editor Scene 状态写回磁盘：
  文件 `D426...`→`50FD...`，而测试前 in-memory hierarchy 已经是 13 roots/Canvas 6 children，
  与写回后的结构一致。本 Change 没有 Scene API save/编辑；为避免覆盖用户内容，未回退该保存结果。
