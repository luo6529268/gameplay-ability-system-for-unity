# R1-SOURCE-006 — Unity CentralOnly presentation crosswalk 与差异登记

> 状态：COMPLETED（静态 source 审计；不代表运行时、像素或 C++ trace 已验收）。  
> Unity source 只描述当前实现；C++ Release source 才定义战斗行为 authority。  
> 本 Work Package 未修改 Unity gameplay、renderer、shader、camera、mesh 或 prefab。

## 1. Unity 现有 render handoff 主链

### 1.1 逻辑 tick 内的冻结点

`NTSDBattleTickSystem.RunPresentationAndCleanupPhase`
（`NTSDBattleTickSystem.cs:355-380`）的静态顺序是：

1. `PreFrameBounds`；
2. `CurrentWaveStage`；
3. `RenderDispatch`；
4. `FramePostProcess`；
5. `LateEntityUpdate`、tail 与结果处理。

`RenderDispatch`（494-506）在普通路径调用
`SimulationWorld.RenderDispatchAll(tickIndex, buildPresentation)`。后者
（`SimulationWorld.StageRender.partial.cs:302-350`）在需要 publication 时调用：

1. `BattlePresentation.BeginFrame(world, tickIndex)`；
2. `BattleCentralRenderSystem.QueueLatestPublishedFrame(world)`；
3. 编辑器非 PlayMode / batchmode 才立即 `PresentLatestFrame`。

所以普通 Play Mode 中的 Unity tick 先冻结逻辑 presentation frame；真正的中央 command
materialization / GPU submission 由 URP later render callback 执行。这是 Unity-native scheduling
适配，不等同于把 Transform 或渲染帧反写为 battle truth。

### 1.2 CentralOnly 的实际像素路径

`BattleRenderFeature.AddRenderPasses`（`BattleRenderFeature.cs:45-67`）仅对 world render
camera 执行：

1. 记录当前 camera / renderer 可用性；
2. `BattleCentralRenderSystem.MaterializeLatestPublishedFrameForCamera`；
3. 获取当前 submission lease；
4. enqueue `BattleRenderPass`。

`BattleRenderPass.Execute`（91-156）按 `BattleDynamicMeshBackend.SegmentCount` 的升序，
逐 segment `CommandBuffer.DrawMesh`。它不遍历 `LF2Entity`，也不读取/写回 entity
位置、frame、team、link、HP 或 velocity。

## 2. 冻结快照与可观察字段 crosswalk

| 需求字段 | Unity capture source | C++ release 对应 source | 静态结论 |
|---|---|---|---|
| slot / active / generation | `StageRender.partial.cs:385-396`、`BattlePresentationShadowBuild.cs:1977-1990` | `renderer.cpp:1303-1308` | Unity 按 runtime slot 收集并验证当前 handle；generation 是 Unity stale-handle adapter。 |
| Z sort | `BattlePresentationShadowBuild.cs:2186-2301` | `renderer.cpp:1309-1318` | CentralOnly slot-order capture 后稳定 radix Z 排序；同 Z 的初始 slot 顺序保留。 |
| frame / pic / data identity | `BattlePresentationShadowBuild.cs:1992-2008` | `renderer.cpp:581-608` | Unity snapshot 捕获 current frame、`ResolveCurrentDataObjectId` 与 `GetRenderPicIndex`；resource lookup 仍须 availability fixture。 |
| position / display Z | `BattlePresentationShadowBuild.cs:2065-2071` | `renderer.cpp:575-579` | Unity capture 使用 runtime XInt/YInt、DisplayZ、RenderOffsetX 与 ReleaseCameraX；type-3 DisplayZ mapping 由 `LF2Entity.cs:4534-4544` 提供。 |
| facing / frame delay blink | `BattlePresentationShadowBuild.cs:2070-2079`、`LF2ObjectRenderer.cs:545-567` | `renderer.cpp:572-638` | Unity 在 command build 复用 `frameDelay < 0` 的 6*(tick&1)-3 X offset 与 left/right pivot 公式。 |
| shadow | `BattlePresentationShadowBuild.cs:2496-2525` | `renderer.cpp:517-556` | hit-stop、link、state、223/224 与 ZInt shadow position 有对应 source gate；OID identity source 仍有一项差异，见 D-RENDER-004。 |
| body | `BattlePresentationShadowBuild.cs:2533-2576` | `renderer.cpp:558-685` | command 写入 Entity type、body position、pivot、UV、flip 和 source sprite descriptor。 |
| overlay / spark | `BattlePresentationShadowBuild.cs:2585-2813` | `renderer.cpp:1356-1438`、687-758 | command 顺序为 overlay 后 hit record；spark age 的写回时点有独立风险，见 D-RENDER-002。 |

## 3. 中央命令排序、mesh 与 transparent pass

### 3.1 Command 顺序

`BattlePresentationCoordinator.BuildCommands`
（`BattlePresentationShadowBuild.cs:2451-2814`）对每个已排序 entity 建立连续 command：

| 顺序 | Unity command | source order | C++ painter counterpart |
|---|---|---|---|
| 0 | Shadow | `baseOrder` | `draw_shadow` |
| 1 | Entity | `baseOrder + 1` | `draw_entity` |
| 2 | OverlayGlyph | `baseOrder + 2` | respawn / Com / slot label |
| 3 | HitRecord | `baseOrder + 3` | `draw_hit_records` |

该 order 是 CentralOnly 的 battle-presentation contract；它不得为了更少 draw call 而按 texture
全局重排。

### 3.2 Dynamic Mesh 不跨命令顺序批处理

`BattleDynamicMeshBackend.Build`（113-344）的静态行为：

- 按 `frame.CommandCount` 递增的 `commandIndex` 解析 command；
- only 相邻 command 且 resource compatible 时合并 segment；
- unresolved command 会关闭 open segment，不能跨越它 batch；
- `BattleCentralRenderSegment.FirstCommandIndex` 记录原 command 流位置；
- `BattleRenderFeature.Execute` 按 segment index 升序 draw。

因此 source 上的 segment submission 顺序保留 command stream；`OrderedChunks` 是默认 draw
mode，`StrictOrderedDraw` 是更严格的诊断模式。此结论只覆盖 C# command/segment 次序；
同一 GPU draw 内透明 primitive 的最终像素仍需未来 URP fixture。

两份中央 shader（`Assets/NTSD/Shaders/BattleCentralTransparent*.shader`）均明确
`Blend One OneMinusSrcAlpha`、`ZWrite Off`、`Cull Off`，且挂在 URP Transparent queue。
这满足“按 painter command 顺序提交”的设计前提，但不能仅凭 source 声称所有 GPU / Android
设备上的像素完全相同。

## 4. CentralOnly 的 fail-closed 行为

这是当前 Unity 渲染架构的实现事实，不是随机“不稳定”：

- `BattleCentralRenderSystem.ShouldSuppressLegacyMaterializers`
  （690-699）只要 world mode 是 `CentralOnly` 就抑制 Legacy materializer；
- `TryValidateActiveRenderer`（1607-1662）要求已注册且 active 的
  `BattleRenderFeature`、合法 central material、有效 URP world camera、以及近期 camera
  observation；
- frame / common catalog / backend / command resource 任一不满足时，
  `CommitCentralFailurePlan`（1523-1564）保留上一份有效 central submission（若存在），
  否则发布 stale / empty central plan；它不会恢复 Legacy pixels。

这解释了历史上“逻辑与音效仍在、但新对象或全部实体不显示”的故障类别。它是为了避免
Central + Legacy 双绘制的 fail-closed 策略，**不是允许无条件回退 SpriteRenderer 的授权**。
后续修复必须先判断是 resource / feature route / unresolved command / visibility gate 哪一项，
并使用现有 central diagnostic reason，不得直接改变 pixel owner。

## 5. 当前已批准、必须保护的 Unity adapter

这些并非待回退的 C++→Unity bug；它们必须在后续 R2+ 修复时保持：

| ID | 保护边界 | 当前 source 事实 | 允许的验收方式 |
|---|---|---|---|
| A-RENDER-001 | CentralOnly / Texture2DArray / dynamic Mesh / URP | `BattleCentralRenderSystem`、`BattleDynamicMeshBackend`、`BattleRenderFeature` 是 production pixels owner；Legacy 仅保留 editor diagnostic/兼容路径。 | 验证 central command 与可观察顺序；禁止回退逐实体 production SpriteRenderer。 |
| A-RENDER-002 | 1.5× 纯表现 scale 与 held attachment compensation | `NTSDRenderSpace.BattleVisualScale=1.5f`；`LF2ObjectRenderer.ComputeEntityBottomCenterPivotPixels` 与 `ComputeHeldVisualAttachmentOffsetPixels` 显式补偿 wpoint/center 相对位移。 | 角色、武器、held object 的相对挂点和阴影需 fixture；不得把 scale 改回 1 以伪造对齐。 |
| A-RENDER-003 | Unity fixed-world logic camera / presentation camera separation | `SimulationWorld.StageRender.partial.cs:828-844` 将 release-style `ReleaseCameraX` 与 per-entity `RenderOffsetX` 清零；`BattleCameraSafeArea` 仅设置 `NTSDRenderSpace.PresentationCameraOffset` 与 Unity camera display。 | 验证 camera/scene 表现不反写 runtime X/Y/Z，且一个角色的逻辑移动不错误带动其他实体或阴影。 |
| A-RENDER-004 | 容量 | C++ 400 slot 只作为 Authority400 fixture；Unity 的 MobileExtended / DesktopExtended 与 central command capacity 是已批准 production 方向。 | 验证 slot-order / lifecycle，不将 C++ 400 变成生产上限。 |

## 6. 已确认的静态差异 / 风险登记

| ID | 状态 | C++ release source | Unity source | 静态差异与最小后续 fixture |
|---|---|---|---|---|
| D-RENDER-001 | 待处理（静态确认） | `renderer.cpp:1300-1438` | `BattleCentralRenderSystem.cs:690-699, 1523-1662` | C++ 逐 active entity 直接 blit；Unity CentralOnly 有 feature/material/camera/catalog/backend ownership gate，失败时 suppress legacy 并可能保留旧 submission 或无像素。fixture：有效 entity + 依次缺 feature、缺 common resource、unresolved sprite、有效 route，记录 diagnostic reason / display tick。 |
| D-RENDER-002 | 待测试（静态时点差异） | `renderer.cpp:687-758`、`game_tick.cpp:2072-2083` | `SimulationTickDriver.cs:360-404`、`BattlePresentationShadowBuild.cs:1547-1608` | C++ 在 pre-postprocess render callback 内推进/回收 hit record；Unity 先冻结 snapshot，再在 LateUpdate 或 worker presentation acknowledgement 后 finalize。必须验证 spark age / expiry 是否影响 next tick 或只影响视觉，不得把它先验判为纯表现。 |
| D-RENDER-003 | 待处理（静态确认） | `renderer.cpp:1305-1308` | `BattlePresentationShadowBuild.cs:1833-1842, 1981-1988` | C++ render selection 只以 `active` 为起点；Unity capture 另跳过 `OidMergeDormant`、`PendingFlushDestroy`、`tick < FirstPresentationTick`、无效 handle。特别是 production `PendingFlushDestroy` 有 frame-logic writer。fixture：在 RenderDispatch 前标 pending 的 entity，比较最后可见 tick 与 slot reuse。 |
| D-RENDER-004 | 待测试（静态 identity 差异） | `renderer.cpp:527-531` | `BattlePresentationShadowBuild.cs:2496-2505` | C++ shadow special OID gate 读取 `core.char_data->oid`；Unity 当前 gate 读取 snapshot `ObjectId`，body resource 使用 `CurrentDatObjectId`。动态 data identity / transform 的 223/224 fixture 必须确认两者等价。 |
| D-RENDER-005 | 待测试（静态 extra gate） | `renderer.cpp:517-685` | `BattlePresentationShadowBuild.cs:2041-2044, 2496-2537`；`LF2Sprite.cs:314-389` | Unity snapshot 还服从 `EntityVisible` / `ShadowVisible`，而 C++ body/shadow 只见其 own gate。source 可见 Unity death/pool/legacy state writer；是否等价于 C++ active/frame transition 未闭合。fixture：Hide/HideShadow、death blink、pool reuse、hit-stop 四组。 |

## 7. 已映射但仍待运行时验收的非差异项

- C++ ZInt/slot stable sorting 与 Unity CentralOnly snapshot/radix flow 已有 source 对应；
- C++ body / shadow hit-stop threshold、modulo blink、state 3005/9997 gate、223/224 shadow
  special case已有 source 对应；
- C++ type-3 body display Z、shadow ZInt、frame-delay shake、pic offset、facing centre
  formula已有 Unity reader；
- command → adjacent compatible segment → ordered `DrawMesh` 的 C# stream 没有 source 级全局
  texture sorting。

上述均只是“已有静态 mapping”；未运行 Unity、未执行 C++ executable，不能称为视觉对齐。

## 8. 未闭合 / 不能从 source 直接回答的事项

1. 哪些 Unity `LF2Sprite.Hide/HideShadow` production caller是 C++ 同义 lifecycle，哪些是
   多余显示门；
2. `FirstPresentationTick` 当前在非 test C# source 中未发现直接 production writer；其可达性、
   是否仅为历史/dormant field，应由 SOURCE-007 记录为 UNKNOWN，而不能当作已生效的行为；
3. CentralOnly route failure 在真实 URP camera / resource loading 下的 first failure reason；
4. GPU final transparent blending、Texture2DArray slice / UV、pivot asset 结果；
5. stage/camera 的用户可见移动方式与 C++ camera scroll 的刻意 Unity 适配边界。

## 9. 后续验收数据要求

R1-SOURCE-007 应将本文件每项接入分层验收矩阵：

- **source verification**：本文件的 C++ / Unity callsite 与字段；
- **command fixture**：tick、slot、frame、pic、position、Z、sort、visibility、command/segment；
- **URP fixture**：central diagnostic reason、submission generation、display tick、draw count；
- **Play Mode**：用户验证角色、held weapon、opoint child、shadow 与具体场景 camera；
- **禁止的替代证据**：仅凭 Legacy、旧 C# self-check、Authority400 checksum 或性能报告声称 C++ release
  视觉已对齐。

