# BATTLE-CENTRAL-EDITOR-FOOT-MARKER-PREVIEW-001 Task Contract

## Objective

在不进入 Play Mode 的情况下，让 `BattleCentralEditorPreview` 通过现有中央动态 Mesh
管线，同时预览角色既有的 `GameConfig.ShadowPrefab` 通用阴影和新增的
`Assets/NTSD/Sprite/UIPanels/FootSelf.png`。两者必须是独立层：通用阴影保留正式运行时
Sprite 尺寸/Pivot/颜色，FootSelf 仍可在 Inspector 调整最终像素尺寸和相对角色脚底中心
的像素偏移，Scene View 可显示范围并拖拽 FootSelf 偏移。

## Authorized scope

- 新增可复用的纯值 `BattleFootMarkerStyle`。
- 为 Editor Preview 增加独立 common-shadow frame/backend/resolver，资源必须来自正式
  `GameConfig.ShadowPrefab` 使用的 `Assets/NTSD/Prefabs/Common/Shadow.prefab`，不得用
  FootSelf 替代原 Shadow。
- 为 Editor Preview 增加独立 foot-marker frame/backend/resolver；复用
  `BattleDynamicMeshBackend`，所有同纹理 marker 合并为一个 segment/submesh/draw。
- 提交顺序必须为 common shadow → FootSelf marker → actor → HP。
- 增加 Scene View 黄色范围、全局 offset 手柄和 sample asset 自动配置。
- 增加 focused tests 和现有离屏验证报告字段。

## Forbidden expansion

- 本 Change 不接入正式 Play Mode 角色选择、自身玩家判定或 runtime snapshot。
- 不修改战斗规则、tick、input、HP、slot/generation、shutdown、Scene、Prefab、URP、
  material、shader、DAT、Server 或 C++。
- 不创建 per-actor GameObject、SpriteRenderer 或 Material instance。
- 不把 FootSelf 加进角色 atlas，也不修改用户提供的 PNG/meta。

## Acceptance

1. Unity compile 0 error。
2. Preview 中 common shadow、foot marker、actor、health 分别可构建；shadow 和 marker
   都位于 actor pivot 附近，且是两个独立可见层。
3. size/offset 与 Inspector/Scene handle 使用相同像素到世界换算。
4. 1000个通用阴影为一个 shadow segment/draw；1000个同纹理 FootSelf 仍为另一个
   marker segment/draw，二者不得逐角色断批。
5. 离屏 Editor Preview validation 同时包含原 Shadow 与 FootSelf，且 Scene dirty unchanged。
6. 现有 preview/health/grid-separator 回归不退化。

## Rollback

删除新增 style 文件，恢复 Preview/controller/editor/tests 中本 Change 的字段、backend、
layout、handle 和断言。不得修改或删除用户的 `FootSelf.png`。
