# BATTLE-CENTRAL-RUNTIME-FOOTSELF-001 Task Contract

## Objective

在正式 Play Mode 中，为绑定本地人类输入的角色显示
`Assets/NTSD/Sprite/UIPanels/FootSelf.png` 黄色脚底圆圈。运行时必须直接复用场景中
`BattleCentralEditorPreview` 已配置的 Foot Marker Sprite、最终像素尺寸、偏移、Tint 和
总开关；保留角色原有 common Shadow，并通过中央渲染系统把所有 Self marker 集中为
一个 Mesh/submesh/draw。

## Authorized scope

- 在 immutable presentation snapshot/Entity command 中增加 presentation-only
  `ShowSelfFootMarker` 标志，来源只能是现有 roster human-input binding。
- 新增专用 `BattleFootMarkerBatchBackend`；每个 marker 一个 quad，全部 marker 共享一个
  Mesh/submesh，禁止 per-actor GameObject/SpriteRenderer/material instance。
- 扩展 central submission 双缓冲、lease、capacity、clear/reset 和 RenderFeature draw，保证
  marker backend 与 entity/health backend 使用同一 captured frame 生命周期。
- 从 `BattleCentralEditorPreview` 读取运行时 authoring settings，不在 RenderFeature 或另一
  MonoBehaviour 复制 size/offset 配置。
- 将现有`NTSD_Battle.unity`中Preview的空`footMarkerSprite`精确绑定到用户提供的
  `FootSelf.png`，确保Player Build也有可追踪资源引用；不得改Scene其他字段。
- 增加 focused EditMode tests，并执行真实 Play Mode 目视/截图验证。
- 更新现有latest-frame materialization夹具的submission构造签名，保持原断言不变。

## Selection semantics

- `Self` = `SimulationWorld.IsBoundActiveHumanRosterInputEntity(entity)` 为 true 的角色。
- AI、武器、特效、logic-only entity 不显示 FootSelf。
- 多个本地 human slot 可以各自显示 FootSelf。
- Friend/Enemy 资源与关系判定不在本 Change 实现。

## Invariants

- Foot marker 只读 presentation snapshot，不写回战斗逻辑、Transform、HP、输入或 roster。
- marker ground anchor 必须与 common Shadow 使用完全相同的地面投影：
  `XInt + RenderOffsetX - CameraX, ZInt`；跳跃高度 `YInt` 不得影响圆圈位置。
- Inspector 的 Width/Height 是79像素标准角色的基准逻辑像素，不额外乘
  `BattleVisualScale`；实际宽高再乘角色资源稳定高度相对79像素的比例。
- 角色比例必须来自角色资源稳定尺寸，不得来自当前动画帧；动画切帧不能令圆圈忽大忽小。
- Inspector Offset保持未缩放的最终逻辑像素，保证用户可独立调整布局位置。
- marker renderer 必须在 actor pixels 之前提交；原 common Shadow 继续保留。
- 空配置、禁用或无本地 human 时发布当前 frame 的空 marker backend，不能保留旧帧圆圈。
- submission lease 未释放前，不得复用或修改对应 marker Mesh。
- 不修改 FootSelf PNG/meta、Prefab、shader、material、DAT、战斗规则或 C++；Scene只允许
  上述单一Sprite引用变化。

## Acceptance

1. Unity compile 0 error。
2. Authoring settings精确复用Preview设置；禁用与缺Sprite时fail-closed为空。
3. snapshot/command只对active human-bound character携带Self marker标志。
4. FootSelf与Shadow使用相同地面锚点；跳跃时留在Shadow地面位置并随角色X/Z移动。
5. 自定义尺寸按稳定角色高度相对79像素等比缩放，offset独立保持；不随动画Sprite尺寸改变。
6. 1000个marker仍为1 Mesh、1 submesh、1 draw，并由submission lease保护。
7. focused tests通过；真实Play Mode可见原Shadow、FootSelf、actor和HP，Scene dirty不变。

## Rollback

删除新增backend/tests及其meta，恢复snapshot/command、submission、central system、
RenderFeature和Preview authoring settings入口中的本Change字段与调用。不得删除或改写
用户的FootSelf资源及既有Editor Preview实现。
