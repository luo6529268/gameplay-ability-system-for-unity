# BATTLE-CENTRAL-RUNTIME-HEALTH-001 — Runtime central overhead HP task

> 日期：2026-08-31  
> 状态：`VERIFIED / COMPILE_0 / RUNTIME_PREVIEW_14_14_PASS / CENTRAL_20_20_PASS / LIVE_STYLE_AND_STABLE_ANCHOR_PASS / PRESENTATION_ONLY`  
> Change ID：`BATTLE-CENTRAL-RUNTIME-HEALTH-001`

## Goal

在不依赖 `BattleCentralEditorPreview` 的前提下，从 immutable battle presentation snapshot 捕获角色真实 `HP/HPBound/HP3`，按当前中央 Sprite 顶部生成一张合并 HP Mesh，并由 `BattleRenderFeature` 每相机追加一次 draw。

## Scope

- 只改 presentation snapshot、central submission/backend/RenderFeature 与 focused tests。
- 不改战斗规则、HP 写入、Scene、Input、Server、lockstep 或 C++。
- 默认只显示 `LF2Character`；HP 值与角色 command 使用同一 frozen frame，位置使用逻辑地面点加角色声明的稳定最大高度，避免动画 Rect/pivot 引起抖动。

## Acceptance

- compile 0；focused/central regression PASS；真实 Play Mode 无 Editor preview 仍能看到并更新 HP；一张 health mesh/submesh/一次 health draw；审计如实。

## Result

- runtime/preview 14/14、central regression 20/20、resource regression 29/29 PASS。
- Play Mode 实际复用 Editor authoring 的 120x10/-16 样式；不同动画姿势下血条纵向像素位置保持一致。
- 删除 preview 对象后仍显示默认样式；要让 Play Mode 继续复用 Inspector 样式，应保留该控制器（允许 inactive）。
