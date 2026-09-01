# BATTLE-CENTRAL-EDITOR-PREVIEW-001 — Edit Mode central preview task

> 日期：2026-08-31  
> 状态：`FOCUSED_TEST_PASS / BMP-GRID-SEPARATOR-RECT-FIXED / PERSISTENT-SCENEVIEW-AUTHORING / GLOBAL-LEDGER-BLOCKED-BY-UNRELATED-RECORD / EDITOR-ONLY / PRESENTATION_ONLY`  

> 结果：Unity/Editor 生成工程均 0 error；持久 SceneView authoring 和 BMP 首帧 Rect 修复完成，最终 focused 6/6，pixel probe 为 637 non-clear/70 red/0 green-separator，Scene dirty unchanged。全局 Ledger 被无关 Change Record 缺少 `code-path` 阻塞；runtime HP 接线仍在范围外。
> Change ID：`BATTLE-CENTRAL-EDITOR-PREVIEW-001`

## Goal

在不进入 Play Mode、不推进 `SimulationTickDriver`、不修改战斗状态的前提下，提供一个可放置在编辑场景中的 `BattleCentralEditorPreview`：它用现有中央透明材质显示一组可配置 Sprite，并把这些角色的样例 HP 以一张合并 Mesh 显示在当前 Sprite 顶部。

## Scope

- 新增 `[ExecuteAlways]` 预览控制器与 Inspector 序列化数据；
- 新增可复用的 3-Quad health-bar batch backend；
- 复用 `BattleDynamicMeshBackend` 生成角色预览 Mesh；
- 通过 `BattleRenderFeature` 在 Edit Mode SceneView/可选 world-camera GameView 提交；
- Play Mode preview fail closed，正式 runtime submission 不变；
- 新增 focused EditMode tests 和 Edit Mode 像素验证入口/证据。

## Invariants

- Preview 数据永不成为战斗真值，也不写回 HP/Transform/SimulationWorld。
- Edit Mode 不启动正式战斗 Tick、对象池或 Character manager。
- 只允许一个活动 preview owner；重复 owner 不提交。
- 所有 health bars 共享 material/white texture，一个 Mesh/一个 submesh；顶点宽度表现 HP 比例，顶点色表现层级。
- 临时 native 资源可释放且不保存进 Scene/Asset。
- 当前 dirty Scene、Server/lockstep 文件和其他用户修改保持原样。

## Validation

1. Unity compile 0 error；
2. focused EditMode tests；
3. 非 Play Mode SceneView 实际像素验证；
4. 现有 central camera/materialization 回归和 `BattleRuntimeSelfCheck`；
5. Change Ledger validator。

## Rollback

按 Change Record 的精确 code paths 移除本功能；production runtime submission 不依赖 preview，因此可独立回滚。
