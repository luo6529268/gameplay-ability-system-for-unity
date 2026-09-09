# BATTLE-SCENEVIEW-HIERARCHY-VISIBILITY-001 — SceneView/Hierarchy 可见性一致性

<!-- CHANGE-RECORD
id: BATTLE-SCENEVIEW-HIERARCHY-VISIBILITY-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCentralLatestFrameMaterializationEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleCentralSceneViewPixelPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleRenderingBenchmark.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleRenderingBenchmarkEditorTests.cs
authority: user requirement 2026-09-06; Unity presentation/editor isolation contract; C++ battle authority N/A because no battle result changes.
evidence: central SceneView real Play isolated; benchmark red 6ed7c384 exact, green 98c80e6f 1/1 and full 1359bba8 38/38; final SelfCheck PASS 09:06:42Z; no production GameObject HideAndDontSave remains outside test fixtures.
-->

> 创建日期：2026-09-06  
> 最后更新：2026-09-06  
> 类型：render / editor / test

## 1. 状态与范围

- 当前状态：`VERIFIED / SCENEVIEW_CENTRAL_PIXELS_ISOLATED / TRANSIENT_GAMEOBJECTS_HIERARCHY_VISIBLE`
- 所属 Work Package：用户插入的表现/编辑器可见性缺陷修复；完成后回到
  `NTSD28-B5-REMAINING-EXIT-AUDIT-006`。
- 不属于本次范围：中央 Mesh 架构、逐实体 GameObject 恢复、Edit Mode 显式预览、Game View、
  战斗逻辑、Scene/Prefab/DAT/资源、C++ authority。
- 关联 Task：`docs/ai/TASKS/BATTLE-SCENEVIEW-HIERARCHY-VISIBILITY-001.md`

## 2. Authority / 需求依据

- 用户明确要求：不允许以后再次出现“Hierarchy 没有、Scene 却显示”的此类运行时战斗内容。
- Evidence 等级：`VERIFIED`（用户需求）；`VERIFIED`（Unity 源码根因）。
- C++ release：`N/A`。本 Change 不改变战斗状态、规则、顺序或正式游戏相机表现。

## 3. Unity 原状与已确认差异

- `BattleCentralRenderSystem.CanRenderCamera(...)` 在 Editor 中返回
  `isPlaying && cameraType == CameraType.SceneView`。
- `BattleRenderFeature.BattleRenderPass` 使用 `CommandBuffer.DrawMesh(...)` 提交角色、武器、
  FootSelf、血条等中央 Mesh；这些像素没有逐对象 GameObject。
- `BattleCentralLatestFrameMaterializationEditorTests`、`BattleRuntimeSelfCheck` 与
  `BattleCentralSceneViewPixelPlayModeProbeEditor` 都把 Play Mode SceneView 收到这些像素当作旧合同。
- 当前 Edit Mode hierarchy 只读查询返回 13 个根对象，无隐藏可见战斗 GameObject；生产
  `HideAndDontSave` 命中中的 Mesh/Material 不是 Hierarchy GameObject。

## 4. 计划改动

| 文件 | 类型 / 方法 | 改前职责 | 目标职责 |
|---|---|---|---|
| `BattleCentralRenderSystem.cs` | `CanRenderCamera(...)` | world camera + Play SceneView | 只接受 exact Base world camera |
| `BattleCentralLatestFrameMaterializationEditorTests.cs` | camera acquisition test | 期望 SceneView lease 成功 | 永久断言 SceneView lease 被拒绝、world camera不受影响 |
| `BattleRuntimeSelfCheck.cs` | P4 camera filter | 接受 Play SceneView | 拒绝所有 SceneView |
| `BattleCentralSceneViewPixelPlayModeProbeEditor.cs` | Play probe | 要求 SceneView 出现中央像素 | 要求 gate/lease 拒绝且捕获为零中央像素 |
| `BattleRenderingBenchmark.cs` | transient benchmark GameObjects | `HideAndDontSave` 隐藏Hierarchy | 保持DontSave但清除HideInHierarchy |
| `BattleRenderingBenchmarkEditorTests.cs` | hierarchy visibility guard | 无对应断言 | presenter/children/camera均不可HideInHierarchy |

## 5. 不可回退边界

- 保留 `CentralOnly`、Texture2DArray、动态 Mesh、URP feature 和正式 world-camera submit。
- 不改变 33/3 ms、slot/generation、worker、logic checksum 或任何 SimulationWorld 字段。
- 保留 Edit Mode `BattleCentralEditorPreview`，因为其预览由 Hierarchy 中可见 owner 明确控制。
- benchmark 非GameObject资源仍保持 `HideAndDontSave`；不改变性能采样、相机layer或render target。

## 6. 实际改动

| 文件 | 类型 / 方法 | 实际改动 | 预期副作用 |
|---|---|---|---|
| `BattleCentralRenderSystem.cs` | `CanRenderCamera(...)` | 移除 Editor Play SceneView 例外，只接受 exact Base world camera | SceneView不再取得中央submission；Game View保持 |
| `BattleCentralLatestFrameMaterializationEditorTests.cs` | camera acquisition test | 反转materialize后的SceneView lease预期，并保留world-camera成功断言 | 防止camera gate回归 |
| `BattleRuntimeSelfCheck.cs` | P4 filter | Play/Edit Mode SceneView均要求拒绝 | broad self-check防复发 |
| `BattleCentralSceneViewPixelPlayModeProbeEditor.cs` | real Play probe | 从“要求出现中央像素”改为“要求gate/lease拒绝且像素为零” | 可在真实SceneView证明隔离 |
| `BattleRenderingBenchmark.cs` | `BattleBenchmarkTransientGameObjectPolicy` + 4 creation sites | runner/presenter/children/camera统一使用`DontSave`且不含`HideInHierarchy` | 临时对象不保存，但存在期间可在Hierarchy定位 |
| `BattleRenderingBenchmarkEditorTests.cs` | `TransientBenchmarkGameObjects_RemainVisibleInHierarchy` | 反射读取真实presenter/root/children/resource camera并断言flags | 防止未来恢复隐藏GameObject |

- 红灯证据：focused job `44017ed85c12432b8a483f091a3f1a49`，13 tests中仅目标测试失败，
  `Expected False / But was True`，准确命中旧 SceneView gate。
- benchmark 红灯：job `6ed7c38453974ac492a59534a1d59787`，准确失败于
  `NTSD Benchmark Legacy Presenter must remain visible in Hierarchy`。

## 7. 验收与证据

| 层级 | 命令 / 场景 / 输入 | 实际结果 | 状态 |
|---|---|---|---|
| 编译 | Unity MCP force refresh/domain reload + Console | 完成，C# error 0 | `PASS` |
| focused test | pre-code red `44017ed85c12432b8a483f091a3f1a49` | 目标断言 `Expected False / But was True` | `RED_CONFIRMED` |
| focused test | `473a2e0b6ea44203b4bb653b617f647c` | latest-frame/camera gate 13/13 | `PASS` |
| related test | `525b576a0bb24d50846532ab6baade7e` | camera/latest-frame/editor-preview/acceptance 30/30 | `PASS` |
| benchmark red | `6ed7c38453974ac492a59534a1d59787` | presenter `HideInHierarchy` 被准确捕获 | `RED_CONFIRMED` |
| benchmark focused | `98c80e6f0ec543c4831a3ccc8411aba3` | hierarchy visibility 1/1 | `PASS` |
| benchmark full | `1359bba886624ac187009225353290f0` | benchmark 38/38 | `PASS` |
| self-check | request/result | 初次`08:45:39Z`、最终`09:06:42Z`均`PASS`；7 intentional negative-path logs | `PASS` |
| Play Mode | SceneView Central Isolation Play Probe | world 19 commands/7 segments；gate=false、lease=false、pixels=0；counts 12/12、10/10 | `PASS` |
| Console | final clear + error/warning query | 0 entries；Play exited | `PASS` |
| Scene | before/after file SHA | Test Runner persisted pre-existing dirty in-memory scene：`D426...`→`50FD...`；未调用save/未回退用户内容 | `OBSERVED_PRESERVED` |
| static production audit | runtime C# excluding Test/Editor | 剩余`HideAndDontSave`均为Mesh/Material/Texture/Sprite/RenderTexture；无生产GameObject赋值 | `PASS` |
| Ledger | `Tools/Validate-ChangeLedger.ps1` | 最终PASS；325 records / 280 governed code files | `PASS` |

## 8. 风险、回滚与未关闭项

- 已知风险：Unity Test Runner 在第一次 focused run 时把原本已经 dirty 的 Scene in-memory状态写回磁盘；
  该结构在测试前 hierarchy 已存在，不属于本 Change，但文件SHA保护因此不能报告unchanged。
- 未关闭项：本 Change 无；完整 NTSD 2.8 对齐仍回到 B5 audit 006。
- 回滚方式：恢复六个允许脚本的本 Change 小范围 diff；不触碰用户既有脚本改动。

## 9. Git / 交接

- 修改前工作树基线：工作树已有大量用户/前序对齐改动；四个目标脚本中仅
  `BattleRuntimeSelfCheck.cs` 已有前序 diff（619/321），必须原位最小修改，不回退。
- 实际 diff 范围：六个脚本的camera gate、benchmark transient GO policy、断言和Play probe语义；无目标Scene/Prefab/内容改动。
- 提交 hash：未提交。
- `Tools/Validate-ChangeLedger.ps1`：最终PASS；325 records / 280 governed code files。
- 交接：先读本 Record 与 Task，再回到 B5 remaining audit 006。
