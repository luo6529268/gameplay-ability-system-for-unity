# HANDOFF — CAMERA-PLATFORM-BACKGROUND-001

> 日期：2026-08-24  
> 状态：`FOCUSED_TEST_PASS / EDITOR-RUNTIME-WITNESS / RUNTIME_PENDING`

## 用户目标

用户已制作一张 `2048 × 1152` 背景主图，最终校正后的要求为：

- Windows 平台使用完整背景全覆盖；
- Android / iOS 同样使用完整背景，蓝天、地面、底部城墙都不能因 source crop 丢失；Game View 只在底部保留可调尺寸的黑色镂空；
- 用户新增要求：在 Unity Edit Mode 的 `Bg (2)` 背景组件中直接切换 Desktop / Mobile 预览；
- 不使用两张重复背景资源；
- PC / 手机未来可共同联机，平台表现不可以污染 battle simulation / lockstep。

## 已确认事实

- 新资产为 `Assets/NTSD/Sprite/OtherBg/battle_background_2048x1152.png`，`2048 × 1152` RGB；
- `NTSD_Battle.unity` 的 `Bg (2)` 已由用户改为引用此资产；
- 初版 importer 已为 Multiple Sprite、PPU=`68.017264`、`isReadable=0`、no-mipmap；`Bg (2)` 已挂 selector，Camera size component 已启用；
- 旧 Desktop / Mobile Sprite 高度不同的实现已经被用户最终否决；当前两个 Sprite 都必须使用完整 source；
- 初版 `(0,96,2048,960)` central rect 的 Play screenshot 出现顶部和底部镂空，用户已明确否决；
- `(0,192,2048,960)` + pivot `(0.5,0.4)` 是已验证但被用户否决的中间结果：它裁掉了背景下方地面。
- 所有 `.4` / `.6` pivot 和 `2048×960` crop 都是历史失败尝试。当前唯一允许方案是 Windows full viewport，Mobile 用完整 source + 可调 bottom-gap presentation viewport；不得改背景或 Camera Transform。

## 允许的实施顺序

1. 将最新 Windows-full / Mobile-full-source + bottom-gap 合同写入本 Task、Change Record、Ledger、State 与 handoff；
2. 将两 Sprite metadata 都恢复为完整 `2048×1152`，在 selector 中发布有效本地 platform，并由 Camera 应用唯一获授权的 viewport rect；
3. 用已打开的 Unity Editor 重跑唯一 Texture 配置菜单，最小绑定 Camera ↔ selector；
4. compile、focused、Desktop / Mobile Editor Game View、gap 调整 witness、Automatic restore、ledger / diff；
5. Android 真机留给用户，不伪称完成。

## 已写代码与最终 Unity 证据

- `BattleCameraSafeArea` 已改为共享背景宽度 + canonical 16:9 的纯 camera-size 公式；
- `BattleBackgroundPlatformSelector` 已新增，Android/iPhone→Mobile、其他→Desktop；
- `BattleBackgroundPlatformAssetEditor` 已新增显式配置菜单；
- `BattleBackgroundPlatformPresentationEditorTests` 已新增。

初版配置菜单、bottom-edge alignment、top-screen alignment 与 Editor preview 均已执行，但所有 crop anchor 都已被用户最终否决。
下一步必须使用完整 source，绝不再以 pivot/crop 制造黑带；整个过程仍不得手写 `.meta` 或 scene YAML。

## 最新实施合同（2026-08-24，pre-code）

- `BattleBackground_Desktop` 与 `BattleBackground_Mobile` 都是同一 Texture2D 的完整 `(0,0,2048,1152)` Sprite 子资产，pivot 都为 `(0.5,0.5)`；
- Windows 的 presentation viewport 为 `Rect(0,0,1,1)`；
- Android/iOS 的 presentation viewport 为 `Rect(0,gap,1,1-gap)`，其中 `gap` 是 Camera Inspector 公开的 `Android 底部镂空比例`，默认 `1/9` 并允许用户调整；
- 该 `Camera.rect` 是用户此次明确授权的唯一 Camera writer；它只影响本机 raster layout，不改 Camera Transform、背景 Transform、战斗 world、tick、input、checksum、Stage 或网络数据；
- selector 必须在 Editor Preview 改变时通知 Camera 刷新 viewport；在 Player 中仍只按真实 platform 生效；
- 如果 Unity/URP 在 Camera rect 之外不能可靠呈现黑色区域，必须先记录并停下，不得私自恢复安全区、Transform 适配或新增未批准的画面遮罩。

## 当前首差：Mobile 实体形变（2026-08-24）

- 新的 full-source implementation 已实际 reimport，focused job `0ede4c1faced4303afdc2f8826da925e` 为 `17/17 PASS`；
- Mobile Game View `Temp/battle-background-mobile-full-source-bottom-gap-20260824-v2.png` 正确保留了蓝天、地面与底部城墙，且底部黑区出现；
- 用户随后在真实有角色的 Game View 中报告“人物被压扁”。原因已测量：默认 `gap=1/9` 时 top viewport 是 `2:1`，而 Camera 被强制
  `aspect=16/9`，因而所有 world render 输出横向拉伸 `1.125`；
- 这不是 entity transform、battle world 或联机数据的差异，但此 visual result 不可交付；
- 下一步仅移除错误的 canonical `Camera.aspect` writer，改 `ResetAspect()` 使用实际 viewport aspect，并将 selector 的有效平台直接通知其
  已绑定 Camera。完整 source 会保留，实体宽高比会恢复；左右窄黑边是避免裁切/变形的必要结果。重新验证后才可更新状态。

## Aspect 修正代码写入

- `BattleCameraSafeArea` 已删除强制 `Camera.aspect=16/9`，在本机 rect 后调用 `ResetAspect()`；
- `BattleBackgroundPlatformSelector` 已新增 `presentationCamera` direct reference，并在应用 preview/platform 时调用 Camera refresh；
- 场景接线、compile、focused tests 和新的 Desktop/Mobile Game View尚未完成；不得使用旧 screenshot 宣称该修正已验证。

当前 scene 原本已dirty，不能为了 `presentationCamera` reference 保存全场景。下一步会把该 reference 改为非序列化、仅在 selector lifecycle/Inspector preview 时解析的 local cache；它不进入战斗 hot path，且不需要 scene save。

该 local cache 已写入：selector只在 apply preview/platform 时必要地找一次 `BattleCameraSafeArea` 并刷新。下一步是重新编译与真正不变形的Desktop/Mobile Game View验证。

## 新的首差：Windows rect 未恢复

- 新的 actual screenshot 显示 Mobile native-aspect已经生成左右黑边、人物可避免横向拉伸，但 Windows preview screenshot仍保留bottom gap；
- 因为 selector preview与Camera refresh跨组件通知未可靠闭合，cache/event方案不再交付；
- 下一步由 `BattleCameraSafeArea` 公开唯一 `编辑器平台预览` 字段，并在本地同步selector；用户在 ScenesCamera Inspector切换后同一回调直接更新rect；
- 该改动还未写入，必须完成新的 compile / focused / Windows / Mobile actual visual evidence后才可交付。

## Camera-owned preview 代码写入

- `ScenesCamera` 的 `BattleCameraSafeArea` 现公开唯一 `编辑器平台预览` 字段；它在自身生命周期/Inspector设置中直接决定 full or mobile rect；
- `Bg (2)` selector的 preview字段已隐藏并被Camera同步；event/cache/direct reference均被删除；
- 下一步为compile、focused、Desktop full、Mobile bottom-gap+pillarbox、Inspector witness和治理检查。

## 最新代码写入（2026-08-24）

- `BattleBackgroundPlatformAssetEditor` 已将 Mobile 的 metadata 改为完整 `(0,0,2048,1152)` + centered pivot；须由唯一显式菜单实际重导入，不能手写 `.meta`；
- `BattleBackgroundPlatformSelector` 已新增 `EffectivePresentationPlatform` 与只在 platform 实际改变时触发的 `PresentationPlatformChanged` event；
- `BattleCameraSafeArea` 已新增 Camera Inspector 的 `Android 底部镂空比例`（默认 `1/9`、范围 `0..0.5`）、selector 引用、Desktop full / Mobile bottom-gap `Camera.rect` 计算与 canonical projection aspect；
- `BattleBackgroundPlatformPresentationEditorTests` 已替换为完整 source、Desktop full rect、Mobile gap 与 clamp contract；
- 此刻只达到 `CODE_WRITTEN`。尚未重新编译、重导入、运行 focused test、启动 Play Mode、确认底部未渲染区域为黑色、验证 gap 调整或恢复默认场景；旧 crop 的通过结果无权证明本次布局。

## 新增预览开关的严格边界（pre-code）

- 仅在 `BattleBackgroundPlatformSelector` 增加 Inspector 三态：`Automatic / Desktop / Mobile`；
- 在 Unity Editor 的 Edit Mode 与 Editor Play Mode 生效；所有 Player 在 `UNITY_EDITOR` 编译边界外忽略预览值，继续按真实平台 resolver；
- 只切 `SpriteRenderer.sprite`，不写 Background/Camera Transform、camera size、simulation、input、checksum、Stage或网络数据；
- 本次代码写入前，Change Record、Task、Ledger和State均已切回 `IN_PROGRESS`；写后需重新通过 focused test、compile、scene default restore、validator和scoped diff。

## 当前代码写入

- selector 已加入 Inspector 字段“编辑器平台预览”：`Automatic / Desktop / Mobile`；
- Editor 的非 Play Mode 中该值覆盖当前 Sprite；`OnValidate` 负责即时刷新；
- `UNITY_EDITOR && !Application.isPlaying` 是唯一 override gate，Player / Play Mode 始终使用原 `RuntimePlatform` resolver；
- focused tests 已覆盖 Edit Mode override 与 Play Mode/Android anti-leak；后续已取得 compile/test/Inspector evidence，见下节。

## 预览开关验证结果

- Unity scripts 强制刷新后 `error CS`=0；focused job `3a774c11142e45c0bb9024e696dc859f`=`14/14 PASS`；
- 在实际 `Bg (2)` Inspector/组件数据中，`editorPreviewMode=2` 立即将 Sprite bounds 改为 Mobile 的
  `30.1100025 × 14.1140633`、center y=`-8.0914059`；随后恢复为 `editorPreviewMode=0`，Desktop bounds 回到
  `30.1100025 × 16.9368763`、center y=`-6.68`；
- Background Transform 一直为 position `(-1.79,-6.68,0)`、scale `(1,1,1)`；`NTSD_Battle` 用 Unity 保存后
  `isDirty=false`；`BattleBackground` filtered Console error=0；
- 预览值不进入 Play/Player resolver 的断言已经通过；但用户否决当前 `.6` pivot 的画面，因此当前不是可交付状态。

## 当前 anchor 更正边界（pre-code）

- 将 `MobilePivotY` 从 `.6` 改为 `.4`；rect 仍为 y=`0`、height=`960`，所以源图底部地面/城墙仍完整；
- 目标是 Mobile slice top=`Desktop top`、Mobile slice bottom=`Desktop bottom + 192px`，因此 Game View 黑色镂空只在底部；
- 仅重导入本主背景并更新几何 focused test；selector preview、Camera、Transform、simulation和Player platform规则保持不变；
- 状态为 `IN_PROGRESS`，需完成实际 Mobile preview 截图与默认 Automatic restore 后才重新回到 `RUNTIME_PENDING`。

## 当前 anchor 代码写入

- `MobilePivotY` 已由 `.6` 改为 `.4`，source rect 仍为 y=`0`、height=`960`；
- focused geometry test 现断言两 Sprite top 对齐，且 Mobile 的 Game View bottom gap 精确为192px；
- 当前只到 `CODE_WRITTEN`；显式 importer menu、compile、focused、Mobile screenshot、default restore/scene、Console、validator与diff均待。

## Game View preview scope correction（pre-code）

- 用户目标图说明预览必须在 Unity Editor 的 Game View 中可用，包含 Editor Play Mode；
- 只扩展 selector 的 `UNITY_EDITOR` preview gate，Player build 继续无条件走真实 RuntimePlatform；
- 更新 focused test 后，与 `.4` pivot 导入/preview 验收一起执行；不改 battle/lockstep、Camera或Transform。

## Game View preview code written

- selector 已去掉 `isPlaying` 条件；`UNITY_EDITOR` 下 Desktop/Mobile preview 现在同时覆盖 Editor Edit/Play；
- Player build 的预处理后路径仍只调用 `ResolvePresentationPlatform(platform)`；
- focused tests 已替换为 Automatic映射与Desktop/Mobile跨平台覆盖，当前与 `.4` pivot 一起待验证。

## 当前 `.4` anchor 与 Game View 验证

- scripts 强制刷新后重跑显式菜单，实际 `.meta` 已为 y=`0`、height=`960`、pivot=`.4`；首次 stale-editor menu 仍写 `.6` 的结果已明确排除；
- focused `f3fa58aea9f6444f9448d556814f4834`=`14/14 PASS`，`error CS`=0；
- `Bg (2)` 在 Editor 设置 `editorPreviewMode=Mobile` 后，Mobile bounds 为 `30.1100025 × 14.1140633`、local center y=`+1.4114065`；
  Editor Play Mode Game View 截图 `Temp/battle-background-mobile-gameview-target-20260824.png` 已确认 lower-source 地面/城墙完整、顶部贴齐、底部黑色镂空；
- 该候选图与用户参考图仍有未裁决差异：参考图保留更多蓝天。固定 y0/h960 crop 无法同时保留该蓝天和最底部城墙；更高 rect 或纯渲染尺寸/UV 适配需要用户明确批准，且不得恢复被禁止的 background Transform 缩放；
- 退出 Play 后已恢复 `editorPreviewMode=Automatic`，Desktop bounds恢复，scene用 Unity 保存；
- 已恢复 Automatic；但上述视觉差异未决，状态为 `IN_PROGRESS`。后续不应再把历史 `.6`/顶部黑色画面作为当前目标。

## 历史首差（2026-08-24 17:32，已解决）

显式 menu 有确认 log，但修正后的 focused EditMode job `3fadcfbc3094422fbebb83be181f7d74` 报出 mobile rect
仍为 y=`96`，而非 y=`192`。因此 menu 的旧 `SaveAndReimport()` 写入路径未持久化修正；目前不允许把 mobile
top-alignment 报告为已完成。下一步只改该 Editor importer write path，使用 `WriteImportSettingsIfDirty` +
force reimport，随后重跑同一测试；禁止手写 `.meta` 或 scene YAML。

## 中间修正结果（2026-08-24 17:34–17:37，已被用户否决）

- importer write path 已换为 `SetDirty → WriteImportSettingsIfDirty → ImportAsset(ForceUpdate)`；
- 实际 `.meta`：Mobile rect `y=192`、height `960`、alignment `Custom`、pivot y=`0.4`；单 Texture、两个 Sprite、
  PPU、no-mipmap、non-readable 不变；
- focused `60e60200476b4d968c0eb0be5cc4a2be`：`9/9 PASS`；
- Mobile editor-simulated Play：顶部无镂空、底部有镂空，camera size=`8.468438`，background scale=`(1,1,1)`；
- Desktop Play：完整 16:9 覆盖，camera size=`8.468438`，background scale=`(1,1,1)`，filtered Console 0 error；
- 临时 Mobile field 已恢复为 Desktop 并用 Unity 正常保存 `NTSD_Battle` scene；
- 最终 scene `isDirty=false`，`error CS` / `BattleBackground` Console 过滤均为 0，ledger validator PASS；
- scoped `git diff --check` 已通过；不含 `NTSD_Battle.unity`，因为其现有用户/Unity YAML diff 含自动生成的空尾行，
  不得为了白空格改写用户 scene；
- 用户在其截图后明确“不接受下层看不到”，因此该结果不再可交付；它仅保留为历史证据。

## 最终 bottom-aligned 结果（2026-08-24 17:48–17:55）

- 实际 Mobile slice：`(0,0,2048,960)`、Custom pivot `(0.5,0.6)`；Desktop slice 不变；同一 Texture2D、PPU、no-mipmap、non-readable 均保持；
- `711ef7d793d9427b9fb08029f8018062`：focused EditMode `9/9 PASS`；
- Mobile editor-simulated Play 画面：`Temp/battle-background-mobile-bottom-aligned-20260824.png`。已人工确认顶部黑色镂空、下方地面与底部城墙完整；
- Mobile bounds=`30.1100025 × 14.1140633`、center y=`-8.0914059`；`Bg (2)` Transform=position `(-1.79,-6.68,0)`、scale `(1,1,1)`；Camera=`8.4684381`；
- 强制 Unity refresh/compile 后 Editor ready，`error CS` / `BattleBackground` filtered Console error 均为 `0`；active `NTSD_Battle` scene 已是 `isDirty=false`，默认 Desktop resolver 已恢复；
- Desktop Sprite/Camera 合同未在本次 Mobile correction 中修改，已有 `Temp/battle-background-desktop-final-20260824.png` Play 证据仍有效；
- `Tools/Validate-ChangeLedger.ps1` 通过（99 records、126 governed code files covered）；scoped `git diff --check` 通过，且 scoped static scan 未发现 Transform、Camera rect/follow/safe-area 或 simulation/checksum/Stage writer；
- Android/iOS Player 与真机尚未运行，故状态只到 `RUNTIME_PENDING`。后续执行者不得把这一点写成 Android 已验收或整体 `VERIFIED`。

## 禁止

- 不恢复任何 safe-area、viewport、follow、debug、Camera/Background Transform writer；
- 不让 Sprite、Camera、screen、platform 进入 SimulationWorld、FrameInputSet、checksum 或 Stage；
- 不删用户新 PNG、旧 PNG 或无关 scene / asset / config；
- 不启动第二个 Unity Editor；
- 不因为 Mobile 物理宽屏未验收就扩大为全屏适配改造。

## 最新运行时证据与恢复条件（2026-08-24）

- Unity script refresh 已完成，Console 无 `error CS`；MCP `Client handler exited` 是工具连接关闭日志，不是项目脚本错误；
- focused job `729aeb5c5d7d469aad6532c648ad06ea` 为 `17/17 PASS`；
- 但是 `ScenesCamera` 设为 Desktop preview 后的 Game View `Temp/battle-background-windows-camera-preview-20260824-final.png` 仍出现底部黑区；直接在 Play runtime 将 `Camera.rect` 设为 `(0,0,1,1)` 后的 `Temp/battle-background-rect-direct-full-diagnostic-20260824.png` 仍未改变。因此 Camera-owned single viewport 方案没有通过 Windows runtime witness；
- 用户最终要求已再次澄清为 Windows 全覆盖，Android/iOS 使用参考图构图且底部空区可调，同时人物、武器和其他实体保持比例；
- 默认 gap 下上部物理 viewport 是 2:1，而完整背景与世界为16:9。一个 shared Camera 不可能同时保持所有内容比例并让完整16:9 source 填满全宽的2:1上部区域。当前不能继续猜测“只背景能否变形”的产品决定；
- 场景本来已 dirty，Play 已停止且没有保存。恢复本包前，用户需确认：允许仅背景使用独立的纯表现合成（实体保持固定16:9），或移动端接受左右 pillarbox 以保证背景和实体均不变形。没有这一确认，不再做脚本/scene/Transform/gameplay 改动。

## 用户恢复（2026-08-24）

用户已回复“开始修复”，因此采用 background-only 独立合成：`ScenesCamera` 永远 full rect；完整背景 source 在 Windows 覆盖全屏，在 Mobile 映射到顶部 `1-gap`；底部保持黑色且 gap 可调；实体完全不参与该非等比背景像素映射。辅助 quad/material 必须 `HideAndDontSave`，原 SpriteRenderer 只有在专用 presenter 可用时才临时 `forceRenderingOff`，不保存 scene、不改 Transform、不进入 battle/lockstep。下一步是先改代码与focused test，再用实际有角色 Game View 验证。

代码已按该合同写入：新增 transient presenter、Resource Shader、Camera full-rect接线和新focused断言。当前尚无新的Unity编译/运行证据，不得把本节当作完成。

首次 Desktop screenshot 显示背景上下倒置，已判定 FAILED，不可作为验收。原因是 clip-space quad 对Sprite texture的Y采样方向；Shader已反转sourceY，待重新导入和截图。

UV修正后Desktop已正向全屏；Mobile首次图的黑区位于顶部，亦判定FAILED。screen UV原点已确认在左上，Shader gap mask已切到底部，需重跑Mobile。

用户还报告Play SceneView缺少地图；已定位为source `forceRenderingOff` 全局隐藏。下一代码修正删除该writer，保留原SpriteRenderer作Scene/fallback，screen presenter仅靠透明排序覆盖Game背景并让中央实体最后绘制。

修正已写：source不再隐藏；presenter使用Transparent queue和background sorting次序。Unity重新导入及Scene/Game联合截图待。

用户再次澄清Scene必须显示`Bg (2)`世界对象，不能显示screen overlay。全局MeshRenderer路线已判失败；恢复时改为BattleRenderFeature target-camera-only background pass，SRP camera begin/end只对ScenesCamera临时隐藏source，SceneView保持原Sprite。

该camera-only URP pass与临时source hide/restore现已写；compile/focused/Scene/Game均待，不得把旧全局MeshRenderer截图当作最终证据。

## 用户指令覆盖

用户已说明背景资源已还原且 `BattleCameraSafeArea` 不再需要。后续执行者必须收回本 handoff 中的双 Sprite、bottom-gap、screen presenter/URP pass 方案：原始 `Bg (2).SpriteRenderer` 是唯一背景资源和 Scene/Game 显示来源；两种已挂载脚本先改为无副作用 compatibility shell，不能删除导致 dirty scene Missing Script；不得保存该场景。删除无挂载的 presenter/shader/importer/test，并恢复 `BattleRenderFeature` 到没有 background pass 的既有 central-render 职责。完成后只可报告原始背景显示/compile/static治理证据，不能沿用旧平台背景验收结论。

## 已完成收敛

- `BattleCameraSafeArea` / selector：无字段、无相机或Sprite writer compatibility shell；
- `BattleRenderFeature`：无 background pass；
- former presenter/importer/test：零行为 compatibility type，避免已打开Editor的旧source list报缺失文件；Shader已移除；
- compile `error CS=0`，Ledger validator PASS；
- Scene/Game witness：`Temp/battle-background-restored-world-sprite-scene-20260824.png`、`Temp/battle-background-restored-world-sprite-game-20260824.png`；
- 不保存 dirty scene。当前照片中的黑边来自 restored PPU=100 background 的 `20.48×11.52` world size 与仓库基线 Camera size `8.468438` 的组合；后续不得猜测性改相机，等待用户决定目标构图。

## 方向纠正

用户明确保留 Windows 全覆盖和Android/iOS底部可调黑区，仅要求移除`BattleCameraSafeArea`。恢复时不得继续纯world-sprite黑边路线。应新建挂载于`Bg (2)`的背景表现组件，通过ScenesCamera-only URP pass实现完整source的desktop/full与mobile/top-gap；source Sprite只在目标Game camera窗口临时隐藏，SceneView继续显示它。不得写Camera/Transform/battle数据，且不保存dirty Scene。

## Camera-only URP Pass 最新交接证据

- focused EditMode job `75282a5423a84726a924c0fb7a87da07`：`19/19 PASS`；
- Mobile Game：`Temp/battle-background-mobile-camera-only-pass-final-20260824.png`，黑区只在底部；
- Desktop Game：`Temp/battle-background-desktop-camera-only-pass-final-20260824-1.png`，全覆盖无黑区；
- Scene：`Temp/battle-background-mobile-bg-world-scene-final-20260824.png`，取景目标是 `Bg (2)` 世界对象，不是全屏 overlay；
- Play 中 `Bg (2)` 为 scale `(1,1,1)`、SpriteRenderer `enabled/isVisible=true`、`forceRenderingOff=false`；
- 已退出 Play、恢复 Camera preview=`Automatic`，未保存 dirty scene；
- 尚缺 Android/iOS Player/真机与有角色的最终比例确认，故只能保持 `RUNTIME_PENDING`。

## 2026-08-25 当前恢复点：Mobile 等比 bottom-anchored crop

用户已批准修正当前 Mobile 背景的纵向压缩：保留 Windows 全覆盖、Android/iOS 可调底部黑区和 `Bg (2)` 的单一完整 source，但将顶部区域的采样改为 bottom-anchored aspect-correct `cover`。对通常更宽的横屏手机，这会裁掉 source 顶部天空而保留地面与底部城墙；不会拉伸背景，也不会改 Camera/Transform/logic/network。对窄输出，则只能居中裁左右以维持无拉伸。

下一执行步骤：先在 `CAMERA-PLATFORM-BACKGROUND-001` 的既有 Record 下扩展 UV crop 纯函数和 focused tests，再由现有 Unity Editor 验证 compile、Desktop/Mobile preview screenshot、SceneView `Bg (2)` 仍真实可见、以及没有 Camera/Transform writer。不得保存 dirty `NTSD_Battle.unity`，不得改 gameplay。

## 2026-08-25 已暂停：Scene/Game world-map first difference

上述 crop 已实施并验证 focused `6a195ff6c776453682fc0ac3003a42e9 = 15/15 PASS`，但用户随后提供的实际 Scene/Game 图证明当前方案不满足关卡编辑需求：Scene 是真实 `Bg (2)` world map，Game 是同纹理的 clip-space background copy，两者无法共同作为 walkable-boundary 的坐标参照。

用户已确认 world-aligned 方案，当前状态恢复为 `CAMERA-PLATFORM-BACKGROUND-001 / IN_PROGRESS / WORLD-ALIGNED CAMERA FRAME`。下一步必须删除实际 screen-background re-draw/hide path，使 `Bg (2)` 是 Scene/Game同一地图；再由背景组件以 bounds + native output aspect 计算固定 Camera `orthographicSize` 与 `position.x/y`，Mobile另加 bottom-gap 的下移；黑区改成同一 target camera 的最终透明黑色 overlay。不得保存 scene，不能恢复Camera.rect/aspect/follow/safe-area，也不能改 gameplay、Stage、input、checksum或联网状态。

## 2026-08-25 当前恢复点：Editor Live Camera Frame

用户已确认当前同-world `Bg (2)` 画面没有直接的资源或变形问题，并新增一个明确的 Editor 使用需求：

- 在 `BattleBackgroundPlatformPresentation` Inspector 添加可开关的 Edit Mode 实时 Camera 取景；
- 当 `Bg (2).SpriteRenderer.sprite`、world bounds、Camera aspect 或编辑器平台预览改变时，实时重算 Camera `orthographicSize` 和 position `x/y`；
- 关闭时恢复该 session 捕获的 camera frame；Player 逻辑继续不受该 editor toggle 影响；
- 不调整背景 PPU、PNG、Sprite pivot、Bg Transform、Stage/walkable bounds、战斗逻辑或联网数据；不保存 dirty scene。

截图中“同一张图看起来不同”的已知解释：Scene View 是独立自由编辑器相机；Scene Camera Preview / Game View 使用 `ScenesCamera` 的世界取景。相同 `2048 × 1152`、PPU100 world Sprite 在不同观察相机下出现不同的面板尺寸或裁边是正常的，不意味着图被拉伸、复制或改变。下一步只改现有背景表现组件与focused tests；之后先取得编译、focused、Sprite-change Editor witness和Console/ledger证据，再做有角色的 Play验证。

### Code Written

`BattleBackgroundPlatformPresentation` 现已新增序列化的“编辑器实时相机取景”开关、Edit Mode-only update hook、切换恢复逻辑和显式 eligibility helper；focused tests 已覆盖 Edit enable/disable、Player contract 与替换 Sprite bounds 的 frame math。当前仅为 `CODE_WRITTEN`：尚未取得 Unity compile、focused、Inspector Sprite-change 或 Play evidence。所有不改 PPU/Transform/Stage/battle/network 的边界保持不变。

### Ownership correction pending

只读 Scene hierarchy / YAML 发现 `XueYuan` 上还存在一个 `BattleBackgroundPlatformPresentation`，且其 `targetCamera` 与 `sourceRenderer` 同样指向 `ScenesCamera` / `Bg (2)`。这不是同一 Sprite 的多视图现象，而是两个 ExecuteAlways writer 竞争同一 Camera 的真实风险。下一步不改 Scene：只在组件内要求 `sourceRenderer.gameObject == gameObject`，foreign duplicate fail closed；新增 focused own/foreign source test，随后重新验证。不得用对象名称特殊处理或移除用户 Scene component。

### Ownership guard code written

组件现已按 sourceRenderer owner 收紧：`Bg (2)` 自己的 component 可以更新 Camera，`XueYuan` 的 foreign duplicate 会 fail closed；它既不删除 Scene 组件，也不写 Stage/battle/world transform。新增临时对象 focused test；compile、focused、实际 hierarchy/Inspector与Console证据待。

为闭合“Sprite 替换→实时相机更新”的实际 Editor path，focused fixture 现已扩展为临时对象 A/B Sprite replacement + private Update invocation + toggle baseline restore。该扩展尚未重新编译或执行；上一轮19/19不能代表它已通过。

### Focused evidence complete

Unity refresh / compile 后 focused job `78c18d4f2b3246f99ab4b024dfc1e3f6` 为 `21/21 PASS`：包含真实 Editor Update 的 temporary Sprite A/B replacement、Camera重算、toggle baseline restore和foreign owner rejection。Console error 仅有MCP连接退出日志，Ledger validator为PASS（101 records / 131 governed files）。当前 Scene 未保存；Bg/ScenesCamera world center一致，XueYuan duplicate仍存在但已fail closed。状态是 `FOCUSED_TEST_PASS / EDITOR-RUNTIME-WITNESS / RUNTIME_PENDING`，仍缺用户真实Inspector换图、Scene/Game有角色和Desktop/Mobile Player视觉验收。
