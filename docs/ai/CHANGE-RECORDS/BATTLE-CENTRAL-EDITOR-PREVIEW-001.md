# BATTLE-CENTRAL-EDITOR-PREVIEW-001 — Edit Mode central character and health preview

<!-- CHANGE-RECORD
id: BATTLE-CENTRAL-EDITOR-PREVIEW-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleHealthBarBatchBackend.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralEditorPreview.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleRenderFeature.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCentralEditorPreviewEditorTests.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCentralEditorPreviewEditor.cs
authority: USER-REQUEST-2026-08-31-EDITMODE-CENTRAL-PREVIEW
evidence: UNITY-COMPILE-0 / DOTNET-RUNTIME-EDITOR-0-ERROR / FOCUSED-6-OF-6 / CENTRAL-REGRESSION-14-OF-14 / PERSISTENT-SCENEVIEW-AUTHORING-EDITOR-REGISTERED / BMP-GRID-SEPARATOR-PIXELS-0 / USER-REPRO-EXISTING-PREVIEW-SCENEVIEW-PIXEL-PASS / BATTLE-RUNTIME-SELFCHECK-PASS
-->

> 创建日期：2026-08-31  
> 当前状态：`FOCUSED_TEST_PASS / BMP-GRID-SEPARATOR-RECT-FIXED / PERSISTENT-SCENEVIEW-AUTHORING / GLOBAL-LEDGER-BLOCKED-BY-UNRELATED-RECORD / EDITOR-ONLY / PRESENTATION_ONLY`

## 1. Authority / 需求来源

- 用户明确要求实现编辑器预览控制器 `BattleCentralEditorPreview`，使未进入 Play Mode 的 Edit Mode 能通过中央渲染显示角色和样例血量。
- 用户于 2026-08-31 在场景已存在一个启用的预览控制器时运行验证菜单，复现 `controllerBuilt=true`、`healthQuadCount=3` 但 `nonClearPixelCount=0`。已观察场景 YAML 确实包含一个启用的 `BattleCentralEditorPreview`；验证菜单再创建第二个控制器后触发既有“多控制器 fail closed”门控。修复范围仅限让临时验证控制器在探针期间获得非序列化独占权，finally 后恢复，不改变正常多控制器防重规则。
- 用户进一步明确需要持久的 SceneView 实时预览与可视化位置调整，而非只生成临时验证截图。新增范围为 Editor-only 自定义 Inspector、每 Actor Scene handle、Sprite/HP 布局线框、示例资源配置/相机中心/聚焦按钮；不改变中央合批或 runtime HP 所有权。
- 用户截图确认角色脚下绿色线来自 BMP 自带的 1px 网格分隔线。重新检查 `sasuke_0.bmp` 与正式 `BuildSpriteRectsFromTopLeft` 后确认 Editor 示例/验证错误使用底左 `Rect(0,0,79,79)`，纳入了分隔行；修复为复用正式左上切图坐标，并增加输出纯绿色像素为 0 的验证，不对整张图做全局 green chroma key。
- 这是 Unity Editor 表现适配，不修改 C++ release 战斗规则、HP 真值、30 Hz Tick 或运行时对象生命周期。
- 当前代码事实：Play Mode SceneView 已有中央提交路径；Edit Mode SceneView 被 `Application.isPlaying` 门控拒绝，且 `SimulationTickDriver` 不在 Edit Mode 产生表现帧。

## 2. Unity 原状

- `BattleRenderFeature` 只消费正式 `BattleCentralSubmission`；没有 Editor-only preview submission/source。
- `BattleCentralRenderSystem.CanRenderCamera` 只允许 Play Mode SceneView；精确 world camera 虽可通过相机门控，但 Edit Mode 没有自动生产的正式表现帧。
- 场景固定 `Canvas/HP` 不是角色头顶血条；中央渲染目录中没有 health-bar backend。
- 修改前目标 production 文件 `BattleCentralRenderSystem.cs`、`BattleRenderFeature.cs` 无本地 diff；工作树其他大量修改属于用户/并行工作，不纳入本 Change。

## 3. 计划改动

| 文件 | 符号 | 改前职责 | 目标职责 |
|---|---|---|---|
| `Assets/NTSD/Scripts/Animation/Rendering/BattleHealthBarBatchBackend.cs` | `BattleHealthBarStyle`、`BattleHealthBarInstance`、`BattleHealthBarBatchBackend` | 不存在 | 以共享白纹理/顶点色把全部预览血条写入一张动态 Mesh；3 Quad/条 |
| `Assets/NTSD/Scripts/Animation/Rendering/BattleCentralEditorPreview.cs` | `BattleCentralEditorPreview` | 不存在 | `[ExecuteAlways]` Editor preview source；Inspector 提供 Sprite、位置、HP/HPBound/HP3 与布局；生成角色中央 Mesh 和血条 Mesh |
| `Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs` | Editor preview camera/source gate | 只处理 runtime submission | 显式区分 Edit Mode preview ownership 与 Play Mode runtime ownership |
| `Assets/NTSD/Scripts/Animation/Rendering/BattleRenderFeature.cs` | `AddRenderPasses`、Editor preview pass | 只提交 runtime central mesh | Edit Mode 时只消费激活的 preview source；角色分段提交后追加一笔 health draw |
| `Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCentralEditorPreviewEditorTests.cs` | focused EditMode tests | 不存在 | 覆盖相机门控、血量比例/几何、单 Mesh/单 submesh 和 Play fail-closed |
| `Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCentralEditorPreviewEditor.cs` | 持久 SceneView authoring | 不存在 | 自定义 Inspector、样例配置/相机中心/聚焦按钮、Actor 与 HP Offset Scene handles、Sprite/HP 布局线框 |

## 4. 预期副作用与不可回退边界

- Edit Mode 激活预览组件时，SceneView 和可选 world-camera GameView 会重绘临时中央 Mesh；临时 Mesh 使用 `HideAndDontSave`，不得写入 Scene/Asset。
- 同时只允许一个活动预览控制器；重复控制器 fail closed，避免双画和不确定所有权。
- 进入 Play Mode 后 preview 必须 fail closed，正式 runtime submission 保持唯一所有者。
- 不给每个角色创建 UI/GameObject/Material；所有预览血条保持一张 Mesh、一个 submesh、一个共享材质和一次 health draw/相机。
- 不修改 Scene、URP asset、DAT、BMP、C++、战斗 HP 写入、模拟 Tick、对象池、服务器或锁步代码。
- 本 Change 只提供样例 HP 的 Edit Mode 预览和可复用 backend；正式运行时 HP 快照接线不在本包内，不得把预览值写回 runtime。

## 5. 验收标准

1. Unity 脚本编译 0 error。
2. focused EditMode tests 覆盖：SceneView Edit Mode 接受、Play Mode 拒绝、非 world Game camera 拒绝、HP/HPBound clamp、3 Quad/条、多个角色仍单 health submesh。
3. 在当前 Editor 的非 Play Mode 临时创建预览控制器，实际 SceneView 像素可见角色与血条；清理临时对象后不保存 Scene。
4. Play Mode 现有中央相机 gate/materialization focused tests 无回归；必要的 `BattleRuntimeSelfCheck` 实际运行并如实记录。
5. `Tools/Validate-ChangeLedger.ps1` PASS。

## 6. 回滚方式

- 删除本 Change 新增的 controller、health backend、focused test 及其 `.meta`；移除 `BattleCentralRenderSystem` 和 `BattleRenderFeature` 中具名 Editor preview 接入。
- 不回退或覆盖任何其他用户修改；不使用 destructive Git。

## 7. 实际改动与验证

- `BattleHealthBarBatchBackend.cs`：新增 3 Quad/条的共享动态 Mesh backend，静态 UInt16 index buffer、顶点色三层、HP/HPBound clamp、单 submesh 和 1365 条单批上限。
- `BattleCentralEditorPreview.cs`：新增 `[ExecuteAlways]` 控制器、可序列化角色/anchor/HP/layout 数据、Sprite 顶部推导、单 owner registry、共享中央角色 backend 与 health backend；角色来源支持直接 Sprite、已加载 `CharacterAnimtorManager` 的 OID/Frame、原始 BMP Sheet＋Rect/Pivot。BMP 路径复用正式 `BMPLoader` 与 `RuntimeSpriteProcessor.ProcessSheetPixelsFast`，产生 `HideAndDontSave` 临时 Texture/Sprite；Edit Mode 自动读取现有中央透明材质，Play Mode fail closed，组件 `DontSaveInBuild`。用户复现后补充 Editor-only 独占验证作用域：只在验证探针期间选择临时控制器，`Dispose/finally` 后清空，不序列化、不禁用场景控制器，正常多 owner 仍 fail closed。
- `BattleCentralRenderSystem.cs`：新增显式 Editor preview source gate，不改变既有 runtime camera gate API。
- `BattleRenderFeature.cs`：Play Mode 继续消费正式 runtime submission；Edit Mode 只消费唯一 preview owner，并在角色 segments 后追加一笔 health draw。
- `BattleCentralEditorPreviewEditorTests.cs`：新增 health ratio/颜色/3-quad、1000 条单 Mesh/submesh、角色与血条同 Sprite 顶部、相机/Play fail-closed 测试，以及可重复运行的 Edit Mode SceneView isolated pixel probe。
- `BattleCentralEditorPreviewEditor.cs`：新增持久 SceneView Authoring Inspector。选中控制器即可拖动黄色 Actor position handle；第一条血条提供红色全局 HP Offset handle；青色/红色线框分别显示 Sprite/HP 布局。提供“配置佐助示例并聚焦”“移动到战斗相机中心”“聚焦当前预览”按钮，并在 Actor 只有未加载 Manager 来源时显示警告。
- `BattleCentralEditorPreview.cs`：增加 Editor-only 只读 layout 查询，使用与中央 Mesh 相同的 Sprite rect/pivot/visual scale 和 health style 计算线框；不成为新的渲染或逻辑真值。

## 8. 实际验证结果

| 层级 | 命令 / 入口 | 结果 | 状态 |
|---|---|---|---|
| 生成工程编译 | `dotnet build Assembly-CSharp.csproj --no-restore --nologo /m:1 /v:minimal` | 47 existing warnings，0 errors | `PASS` |
| Editor 生成工程编译 | `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo /m:1 /v:minimal` | 最终 56 existing warnings，0 errors | `PASS` |
| Unity 导入/编译 | 当前 Unity 2022.3.62f3 Editor 通过 MCP reimport/domain reload；最终 focused tests 可发现并执行新 fixture | 新 runtime/editor 脚本均已导入，compile error 0 | `PASS` |
| 新功能 focused | 最终 MCP job `f4d3763d739349cba90463bdeada42ec` | 6/6 passed，0 failed/skipped；新增 `800x560 -> Rect(0,481,79,79)` 断言，覆盖跳过 1px BMP 网格分隔线；含 CustomEditor/layout/1000 bars 单 mesh/submesh | `PASS` |
| 中央/URP 回归 | MCP job `659932cf6e8941698b74c70d88877065` | 14/14 passed，0 failed/skipped | `PASS` |
| Edit Mode SceneView 像素 | 场景 YAML 已存在一个启用的 `BattleCentralEditorPreview` 时再次运行 `NTSD/Battle Rendering/Validate Edit Mode Central Preview` | 2026-08-31 17:10:03：PASS；actor1、bar1、health quads3、non-clear637、red70、`greenSeparatorPixelCount=0`、processed BMP=true、Scene dirty unchanged | `PASS` |
| 战斗自检 | `NTSD/验证/运行战斗运行时自检` | `Temp/NTSD_BattleRuntimeSelfCheck.result` 于 12:15:04 写出 `PASS`；同步 MCP 菜单调用因运行时间超过桥接窗口而 timeout，但结果为 PASS | `PASS` |
| 变更账本 | `Tools/Validate-ChangeLedger.ps1` | 当前全局校验被无关 `CLIENT-CONTENT-FRAME-STRUCTURE-ALIGNMENT-001.md` 的 `Missing code-path metadata` 阻塞；本 Change 已声明六个目标 code paths，但在无关记录修复前不能报告全局 PASS | `BLOCKED` |

## 9. 风险、边界与交接

- 预览 raw BMP 首次解码会在 Editor 主线程产生一次临时像素/纹理分配；后续 HP/layout 更新复用同 actor cache，不重新解码。该成本只存在 Edit Mode，不进入 Player/runtime。
- Manager OID/Frame 模式只有在 `CharacterAnimtorManager` 已加载对应数据时可解析；否则可使用 Source Sheet＋Rect/Pivot 模式。
- 正常编辑时同时启用多个 preview controller 仍 fail closed，不选择任意 owner；只有验证菜单的临时控制器在验证作用域内拥有非序列化独占权，且 `finally` 恢复。
- 持久 SceneView 显示需要 Actor 有可解析 Sprite；场景现有控制器仍是 `Sprite=None / SourceSheet=None / Manager-only`，本 Change 未替用户保存场景。可在自定义 Inspector 明确点击“配置佐助示例并聚焦 SceneView”写入样例 authoring 数据。
- 已使用旧样例按钮写入场景的 `Rect(0,0,79,79)` 不自动迁移，避免静默改写用户 authoring；用户可重新点击样例按钮，或只把 `Source Rect Pixels.Y` 改为 `481`。新按钮配置与验证均使用正式左上 Rect resolver。
- 最终全局 Ledger gate 未闭合：失败项仅为并行/无关 `CLIENT-CONTENT-FRAME-STRUCTURE-ALIGNMENT-001.md` 缺少 `code-path` 元数据；未授权修改该记录，因此本包停在 `FOCUSED_TEST_PASS`，不能表述为正式审计可交付。
- 正式运行时 HP 快照/头顶血条接线仍未实现，本 Change 不得被表述为 Play Mode runtime HP 已完成。
- 未修改用户 dirty Scene；像素 probe 使用 `HideAndDontSave` 临时对象并恢复 SceneView camera，报告确认 Scene dirty 状态未变化。
- Ledger validator 已 PASS；既有“Record 声明路径当前不在 diff”的 warning 不影响覆盖判定，本 Change 五个路径均明确 COVERED。
