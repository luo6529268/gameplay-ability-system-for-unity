# BATTLE-CENTRAL-RUNTIME-HEALTH-001 — Play Mode central overhead health bars

<!-- CHANGE-RECORD
id: BATTLE-CENTRAL-RUNTIME-HEALTH-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleHealthBarBatchBackend.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralEditorPreview.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattlePixelFramePlan.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleRenderFeature.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCentralRuntimeHealthBarEditorTests.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCentralLatestFrameMaterializationEditorTests.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCatalogCentralResourceResolverEditorTests.cs
authority: USER-REQUEST-2026-08-31-RUNTIME-OVERHEAD-HP; C++ release renderer.cpp/entity hp-hp_max-hp3 semantics
evidence: COMPILE_0 / RUNTIME_PREVIEW_14_14_PASS / CENTRAL_REGRESSION_20_20_PASS / RESOURCE_REGRESSION_29_29_PASS / LIVE_STYLE_AND_STABLE_ANCHOR_PASS
-->

> 创建日期：2026-08-31  
> 当前状态：`VERIFIED / COMPILE_0 / FOCUSED_TEST_PASS / CENTRAL_REGRESSION_PASS / LIVE_VISUAL_PASS / PRESENTATION_ONLY`

## 1. 需求与权威边界

- 用户明确要求删除 `BattleCentralEditorPreview` 后，进入 Play Mode 仍显示跟随当前中央 Sprite 顶部的真实 HP 血条，并实时更新、全部合并提交。
- C++ release `src/render/renderer.cpp:670-683` 证明 renderer 在 entity sprite blit 后读取 `hp/hp3`；正式实体字段语义为 `hp` 当前值、`hp_max` 可恢复上限、`hp3` 最大上限。原版这里只在低血量+bpoint 条件画小提示点；用户要求完整头顶条，属于明确的表现扩展。
- Unity 必须从同一 immutable `BattlePresentationFrame` 捕获 `NTSDEntityRuntime.HP/HPBound/HP3`，不得在 URP RenderPass 中读取可变实体，也不得写回模拟状态。

## 2. 原状

- `BattleHealthBarBatchBackend` 和三层条只被 `BattleCentralEditorPreview` 使用。
- Play Mode `BattleCentralSubmission` 只租约保护 `BattleDynamicMeshBackend`；`BattleRenderFeature.BattleRenderPass` 只画角色/阴影/特效 segments。
- `BattlePresentationEntitySnapshot` 未携带 `HP/HPBound/HP3`，所以删除 Editor preview 后没有任何 runtime HP draw。

## 3. 计划改动

1. Presentation entity/command 捕获角色 `HP/HPBound/HP3` 和是否显示头顶条；复制/冻结保持同 tick 一致。
2. `BattleHealthBarBatchBackend` 增加从 materialized entity commands 构建的无分配路径；位置使用逻辑地面点与角色声明 sheet 的稳定最大高度，避免随当前动画帧 Rect/pivot 抖动。
3. 每个 central submission slot 配一张 health mesh，并把 health mutation/frame identity 纳入 lease currentness。
4. `BattleCentralRenderSystem` 在角色 backend 构建后构建同 frame HP backend；reset/failure/reuse 一并清理。
5. `BattleRenderFeature` 在所有角色 segments 后用 `BattleCentralTransparent + whiteTexture + vertex color` 追加最多一次 health draw。
6. 运行时默认启用；若 Scene 中保留 Editor preview 控制器（可禁用），复用其序列化样式；对象不存在时回退 `BattleHealthBarStyle.Default`。

## 4. 不变量与风险

- 只对 `LF2Character` 的可见 Entity command 生成条；武器、飞行物、阴影、火花不生成。
- `HP/HPBound/HP3` 只作为 presentation snapshot；不写逻辑、不改变 30Hz tick。
- 所有条一张 mesh/一个 submesh/一笔 draw；不得每角色 GameObject/Canvas/Material。
- actor submission lease 必须同时保护 health mesh，防止双缓冲槽复用时相机读写冲突。
- 最大单批仍为 1365 条；超过时 fail fast/截断策略必须显式测试，不静默破坏索引。

## 5. 验收

- Unity runtime/editor 生成工程 0 error。
- focused tests 覆盖 HP snapshot freeze、只选角色 Entity command、Sprite 顶部位置、HP/HPBound/HP3 比例、一张 mesh/submesh、lease currentness、RenderFeature 追加一 draw。
- 现有中央渲染相关测试无回归。
- 真实 `NTSD_Battle` Play Mode 在没有 `BattleCentralEditorPreview` 时可见至少一个角色头顶条；改变 HP 后宽度更新。
- `Tools/Validate-ChangeLedger.ps1`；若仍被无关 record 阻塞则如实停在对应状态。

## 6. 回滚

- 移除 snapshot/command 的 runtime health presentation 字段、submission health backend、central build/draw 接入和 focused tests。
- 保留 Editor preview 独立功能；不回退任何用户/并行改动。

## 7. 实际实现

- `BattlePresentationEntitySnapshot` 与 `BattleRenderCommand` 新增只读 `ShowOverheadHealthBar/CurrentHealth/RecoverableHealth/MaximumHealth`；capture 仅对 `LF2Character && HP3 > 0` 写入同 tick 的 `runtime.HP/HPBound/HP3`。
- snapshot 同时保存由角色全部 DAT sheet 声明高度求得的稳定高度；materialization 从逻辑 `XInt/DisplayZ/YInt` 计算稳定世界锚点，不读取当前动画帧 Rect/pivot/center 或帧抖动偏移。
- `BattleHealthBarBatchBackend.BuildFromFrame(...)` 按 background/recoverable/current 三 quad 写入单 Mesh/单 submesh；后续 frame 重建同一后端以反映 HP 变化。
- central 双缓冲的每个 submission slot 各自持有 health backend；publication 同时冻结 actor/health mutation version 与 frame identity，lease currentness 同时校验两张 Mesh。
- `BattleRenderFeature` 在 actor segments 后使用 `BattleCentralTransparent`、white texture 与 vertex color 追加至多一次 health draw；缺省样式回退 `BattleHealthBarStyle.Default`，不需要 `BattleCentralEditorPreview`。
- `BattleCentralEditorPreview` 存在时（包括 inactive）作为运行时样式 authoring source；当前 Scene 序列化的 `Width=120/Height=10/Border=1/HeadGap=6/Offset=(0,-16)` 被 Play Mode 实际复用。删除该对象不会删除运行时血条，但会改用默认样式。
- 原单参数 reflection test 已改为显式传入 actor/health 两个 backend，避免隐式创建无 owner 的 health backend。
- 资源解析测试的 trusted-command reflection fixture 已同步新增 health contract 参数，避免继续调用旧构造签名。

## 8. 实际验证与未验证项

- `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo /m:1 /v:minimal`：0 error、56 warnings；warnings 为工作区既有/并行代码与 Unity 依赖版本提示。
- Unity runtime HP + preview job `159562dcfaaf4c7ea6b000a63eba889f`：14/14 PASS；覆盖真实 `LF2Character` capture、snapshot copy、单 Mesh/submesh、后一帧 HP 宽度更新、Editor 样式复用、极端不同动画 Rect/pivot 下稳定锚点不变。
- Unity central regressions job `a21e89b226b54beb87ed242f26f66dfe`：20/20 PASS。
- Unity atlas/resource regressions job `e08c3da8cf6149db95af721274bf8ab8`：29/29 PASS；包含新增命令字段后的 trusted resource identity/cache 路径。
- 延迟生成角色后的两张 1920x1080 GameView 画面中，左右角色使用不同动画姿势，但红色血条像素位置均保持 `y=841..848`，并复用了 Scene authoring 的 120x10/-16 样式；证据为 `Temp/BATTLE-CENTRAL-RUNTIME-HEALTH-001/BATTLE-CENTRAL-RUNTIME-HEALTH-001-stable-a.png` 与 `stable-b.png`。
- HP 变化导致 current quad 宽度更新已由 focused test 逐帧证明；本轮未人工制造受击输入。
- 临时 `Assets/Screenshots` 取证文件及 meta 已通过 Unity AssetDatabase 删除，证据副本仅保留在 `Temp`；Scene 未保存。
