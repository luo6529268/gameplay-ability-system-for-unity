# CAMERA-PLATFORM-BACKGROUND-001 — PC / Mobile single-texture background implementation contract

> 状态：`FOCUSED_TEST_PASS / EDITOR-RUNTIME-WITNESS / RUNTIME_PENDING`

## Goal

将用户提供的一张 `2048 × 1152` 背景纹理配置为同纹理 Desktop / Mobile Sprite，并保持跨平台联机所需的战斗世界与逻辑视野隔离。Windows 必须全覆盖完整背景；Android/iOS 必须显示同一完整背景，并在 Game View 底部显示可调尺寸的黑色镂空。

## Scope

- 配置一个 TextureImporter 与两个完整 source Sprite rect；
- 加入只选择本地平台 Sprite 的 presentation component；
- 在该 component 上增加仅 Unity Editor 生效、可在 Game View / Editor Play Mode 查看的 Desktop / Mobile Inspector 预览切换；
- 将现有 Camera 组件维持为共享宽度 + canonical 16:9 的 size writer，并新增用户授权的 Windows full / Android bottom-gap local `Camera.rect` presentation writer；
- 在 Unity 中最小绑定到现有 `Bg (2)`；
- 编译、focused test、Desktop Play 与审计验证。

## Authority / Evidence

- 用户 2026-08-24 的最终校正：Windows 全覆盖；Android/iOS 保留完整背景 + 底部可调黑色镂空；单资源、跨平台联机隔离；
- 根 `AGENTS.md` 的 Unity presentation / lockstep 边界；
- `CAMERA-PLATFORM-BACKGROUND-001` Change Record 的主纹理、PPU、rect 和 camera 合同。

## Files Likely Involved

- `Assets/NTSD/Scripts/App/BattleCameraSafeArea.cs`
- `Assets/NTSD/Scripts/App/BattleBackgroundPlatformSelector.cs`
- `Assets/NTSD/Scripts/App/BattleBackgroundScreenPresenter.cs`
- `Assets/NTSD/Scripts/App/Editor/BattleBackgroundPlatformAssetEditor.cs`
- `Assets/NTSD/Scripts/Test/Editor/BattleBackgroundPlatformPresentationEditorTests.cs`
- `Assets/NTSD/Sprite/OtherBg/battle_background_2048x1152.png.meta`
- `Assets/NTSD/Scene/NTSD_Battle.unity`
- `docs/ai/CHANGE-LEDGER.md`、`docs/ai/STATE.md`、本 Task / Record / Handoff

## Verification

1. 用 Unity Editor 菜单配置并重新导入唯一主纹理；
2. 验证两个 Sprite 共用同一纹理、rect / PPU / bounds 正确；
3. 跑 platform resolver 与 geometry focused Editor tests；
4. 在已打开的 Unity Editor 中编译，不启动第二个 Editor；
5. Desktop Play 验证选择完整 Sprite、Camera size、scale、Console；
6. Mobile branch 通过 deterministic resolver test，Android 真机/build 由用户另行验收；
7. 运行 ledger validator 与 scoped diff check。

## Current Progress

- Task / Change Record / Ledger / State / Handoff 已在脚本前建立；
- selector、camera canonical-width公式、explicit importer menu 和 focused tests 已写；
- 初版 Mobile central rect 在短 Play 中显示为上下均镂空，已被用户明确否决，并已留存 failed job `3fadcfbc3094422fbebb83be181f7d74`；
- 所有 `2048×960` Mobile crop（含 `.4` / `.6` pivot）均已被用户最终否决；它们只能保留为历史，不可作为当前资源、测试或验收合同；
- Desktop Sprite、shared camera size、background Transform、selector映射和所有战斗/同步模块都不得改；
- 实际 `.meta` 与 focused job `711ef7d793d9427b9fb08029f8018062` 的 `9/9 PASS` 已确认 bottom edge shared；
- 当前准备改为两个完整 `2048×1152` Sprite 子资产；Desktop `Camera.rect=(0,0,1,1)`，Mobile `Camera.rect=(0,gap,1,1-gap)`，其中 `gap` 为 Camera Inspector 可调的底部镂空比例，默认以 `1/9` 起步；
- `Camera.rect` 是用户本次明确授权的唯一 Camera presentation 写入例外；Camera Transform、背景 Transform、simulation、checksum、Stage 和网络数据继续禁止；
- 既有 compile/test/screenshot 只证明旧 crop 链能运行，不证明新 viewport 布局；新实现必须重新通过 compile、focused tests、Editor Game View、scene restore、validator 和 scoped diff；
- Android/iOS Player / 真机仍待用户验收；不因 Editor 模拟而标记 `VERIFIED`。

## Code Written — 2026-08-24

- 两个 Sprite metadata 现在都要求完整 `2048×1152` source；
- selector 已发布有效本地 platform change；
- Camera 已具备 `Android 底部镂空比例` Inspector 字段、Desktop full / Mobile bottom-gap rect 算法与 canonical projection；
- focused tests 已改为完整 source + viewport 合同；
- 未运行任何 Unity 编译、菜单重导入、测试或 Play Mode，因此当前不能视为运行时完成。

## Aspect Correction — 2026-08-24

实际 Mobile Game View 已确认先前的 canonical aspect override 会把 16:9 战斗世界拉入 2:1 top viewport，人物和所有实体因此横向拉伸约 12.5%。
下一步只移除该错误的 aspect override，改用 native viewport aspect，并把 selector 的 Preview 直接接到 Camera refresh。完整 source 和原始实体宽高比会保留；Mobile 上会出现几何上不可避免的左右窄黑边，底部黑区仍可调。旧的“仅底部黑区且全宽”截图不再是可交付证据。

### Code Written

`Camera.aspect` force-write 已替换为 `ResetAspect()`；selector 已加入直接 Camera refresh reference。未完成 Unity scene reference binding、compile、focused test和实际 Game View，当前仍不可交付。

当前 Scene 已有用户未保存修改，因此 direct Camera link 改为 selector 的非序列化生命周期缓存；不会要求保存或覆盖 scene 来保持预览联动。

缓存实现已写入，待重新编译、验证和治理检查。

最新实际 witness 说明 selector-only preview 不能可靠将 Mobile rect 恢复成 Windows full rect，因此改为由 `ScenesCamera` 唯一拥有预览字段；selector只保留隐藏内部值。该修正未写代码，compile / test / Game View均待。

Camera-owned code现已写入，待重新编译、focused test与真实Desktop/Mobile Game View验证。
- 用户现已授权本次最小新增：在 selector 上公开 `Automatic / Desktop / Mobile` 编辑器预览字段；实现前后都不得改变 Player runtime 的真实 platform mapping。
- selector 与 focused test 已写入 preview resolver/anti-leak assertions；Unity compile=`error CS: 0`，focused job `3a774c11142e45c0bb9024e696dc859f`=`14/14 PASS`；
- Inspector/scene witness 已实际执行：`Bg (2)` 的 `editorPreviewMode=2` 选择 Mobile bounds，恢复 `editorPreviewMode=0` 后选择 Desktop bounds；场景正常保存且 `isDirty=false`；
- selector preview 已扩展为整个 Unity Editor（包括 Editor Play Mode）有效，Player branch 仍由 `UNITY_EDITOR` 预处理隔离；
- menu reimport 后实际 meta pivot=.4；focused `f3fa58aea9f6444f9448d556814f4834`=`14/14 PASS`；Editor Play Mobile Game View 截图已确认下方源图完整、黑色空白在底部；
- 对照用户参考图发现当前 lower-source 960 crop 缺少参考图中的部分蓝天。固定 crop 无法同时保留同等蓝天和最底部城墙；是否允许更高 rect 或纯渲染尺寸/UV 适配需要用户决定；
- 已恢复 `Automatic`、保存场景；在视觉裁决前保持 `IN_PROGRESS`，Android/iOS Player / 真机也仍待用户验收。

## 2026-08-24 最新运行时首差与停止条件

- scripts force refresh 后无 `error CS`；focused job `729aeb5c5d7d469aad6532c648ad06ea`=`17/17 PASS`；
- 但 Windows Editor Play 的 `ScenesCamera` Desktop preview screenshot 仍有底部黑区；直接将运行时 Camera rect 设为 full 也未改变该 screenshot。当前 `Camera.rect` 不能提供 Windows-full 的可靠 runtime witness；
- 用户重新确认最终目标：Windows 全屏完整背景；Android/iOS 按参考图保留可调底部黑区；战斗实体不可变形；
- 对 16:9 source，在底部留空后的 `2:1` top viewport 中，single shared camera 不可能同时做到全宽、完整 source、仅底部黑区及所有内容零形变；
- 本 Task 改为 `BLOCKED`，等待用户决定是否允许**仅背景**进入独立的纯表现合成路径（实体相机不改、背景视觉比例可适配参考图）；若不允许，则必须接受移动端左右 pillarbox。该决定之前不再修改脚本、scene、Transform、战斗/联机数据或验收口径。

## 用户恢复与当前实施范围

用户已明确回复“开始修复”，恢复本 Task。后续只实施 background-only clip-space composition：battle Camera 始终 full rect；Windows 背景全屏；Android/iOS 背景映射到顶部 `1-gap`、底部黑区可调；实体继续由 full Camera 绘制且保持比例。不得增加 scene 持久对象，不得修改 Camera/Background Transform、战斗或联机数据。实现后必须重跑 compile、focused tests、Desktop/Mobile 有角色 Game View、Console、Ledger validator 与 scoped diff。

## 2026-08-25 用户新增：Editor Live Camera Frame

当前 clip-space 路线已被后续同-world-map 需求替代；本条仅作为当前 Task 的追加实施合同：

- `BattleBackgroundPlatformPresentation` 公开一个独立的 Editor-only 实时相机取景开关；
- 开启时，Edit Mode 中 `Bg (2)` 的 Sprite、world bounds、Camera output aspect 或 Desktop/Mobile preview 改变，必须实时更新 Camera size 与 `x/y`；
- 关闭时恢复本次编辑器预览捕获的 Camera frame；
- Play runtime、PPU、Texture、Sprite pivot、Bg Transform、Stage/walkable bounds、battle/lockstep/network 完全排除；
- 同一图在 Scene View 与 Game View 的显示差异必须记录为不同观察相机的结果，不能被误判为不同资源或 Transform 缩放。

验收：focused pure eligibility/frame tests；Unity compile 0 error；在 Editor 中更换 `Bg (2)` Sprite 后 Camera Preview 更新；Play/Console 与 Ledger validator。没有这些证据前状态保持 `IN_PROGRESS`。

## 2026-08-25 当前自动证据

- `78c18d4f2b3246f99ab4b024dfc1e3f6`：`21/21 PASS`，包括实际 Editor `Update` 触发的临时 A/B Sprite replacement、相机位置/size重算及开关关闭后的 baseline restore；
- `XueYuan` 上的 foreign duplicate source owner 被独立 fixture 拒绝，production Scene 不被删除或保存；
- Unity scripts refresh 后无项目 C# error，Console error 查询仅有 MCP 连接退出日志；Ledger validator PASS。

当前只可标为 `FOCUSED_TEST_PASS / RUNTIME_PENDING`。下一层需要用户在真实 `Bg (2)` Inspector 更换 Sprite 后观察 Camera Preview，及有角色的 Desktop/Mobile Game/Player视觉验收；这些都不能用隔离测试替代。

代码现已写入：新增 transient screen presenter 与 Resource Shader；Camera rect 全平台固定full；platform gap只进入background shader；测试合同已切换。Unity验证仍全部待执行。

首次 Desktop runtime 已确认 presenter 能绘制，但纹理上下颠倒；失败截图已留痕，Shader UV Y 已修正，所有验证需重新执行。

第二次 Desktop已正向全屏；Mobile黑区却位于顶部，失败图已留痕。gap mask已按screen UV左上原点移到底部，待重新导入和Mobile验证。

新增 first-difference：source `forceRenderingOff` 同时隐藏 Scene 视图。将移除该全局writer，改由screen presenter在原背景之后、中央实体之前覆盖Game；Scene继续显示原SpriteRenderer。

该修正已写：无source visibility writer，screen presenter仅靠Transparent sorting覆盖Game，待重新验证。

用户进一步明确Scene必须显示`Bg (2)`世界Sprite，而不是clip-space overlay。全局MeshRenderer方案废止；下一步将background draw移入既有BattleRenderFeature的target-camera-only pass，并只在ScenesCamera渲染窗口临时隐藏source。

URP pass代码现已写入，既有central pass顺序未改；所有Unity与Scene/Game验证待。

## 用户恢复资源后的新范围

用户已明确取消 `BattleCameraSafeArea` 依赖，背景恢复为 `Bg (2)` 的单一世界 Sprite。当前任务改为删除前一轮平台切图、screen presenter、URP background pass 和相机写入；保留同名已挂载组件为无副作用 compatibility shell，防止 dirty Scene 在未保存前变成 Missing Script。不得移动 Camera 或背景，不得保存 Scene。验收改为原始 `Bg (2)` 同时出现在 Scene/Game、C# 0 error、静态无相机/Sprite writer、Ledger validator通过。

## 实际结果与待定项

- C# compile 0 error；Ledger validator通过；
- `Bg (2)` 在 Scene/Game 中均由单一原始 SpriteRenderer 显示，position/scale不变，`forceRenderingOff=false`；
- `BattleCameraSafeArea`、selector、presenter/importer/test compatibility types和 `BattleRenderFeature` 都不再拥有相机、Sprite或背景pass写入；
- 还原背景目前 world size=`20.48×11.52`，仓库基线 Camera size=`8.468438`，所以Game截图存在黑边。此视觉构图没有被本任务擅自改动；用户确认要以哪一个场景参数为目标前，本Task保持`RUNTIME_PENDING`。

## 用户纠正后的实施合同

用户澄清：只取消 `BattleCameraSafeArea`，不取消 Windows 全覆盖 / Android底部可调黑区。下一实现必须把背景平台表现迁到挂载于`Bg (2)`的新组件，且仅通过target-camera-only URP pass绘制。相机、背景Transform和战斗实体全部不写；SceneView仍走原SpriteRenderer。单一完整Sprite是唯一资源；Android gap 在该新组件Inspector公开。当前Scene dirty，不保存。

## Camera-only URP Pass 验证结果

- focused EditMode `75282a5423a84726a924c0fb7a87da07`：`19/19 PASS`；
- Mobile Editor Play：`Temp/battle-background-mobile-camera-only-pass-final-20260824.png`，黑区只在底部；
- Desktop Editor Play：`Temp/battle-background-desktop-camera-only-pass-final-20260824-1.png`，全屏无黑区；
- Scene View：`Temp/battle-background-mobile-bg-world-scene-final-20260824.png`，通过 `Bg (2)` world object 取景，未显示全屏 overlay；
- Play 中 source SpriteRenderer 为 `enabled=true / isVisible=true / forceRenderingOff=false`，Transform scale=`(1,1,1)`；
- 已退出 Play并恢复 `Automatic`，未保存 dirty scene。Android/iOS Player/真机和有角色最终比例复核仍待，因此状态为 `RUNTIME_PENDING`，不是 `VERIFIED`。

## 2026-08-25 用户批准的等比裁切修正

此前 Mobile 输出把完整 16:9 背景压缩到顶部 `1-gap` 区域，导致纵向拉伸。用户已确认改为：

- Desktop 全屏 `cover`，不拉伸；
- Mobile 仍保留可调的底部黑区，但顶部区域按实际输出 aspect 做 **bottom-anchored cover crop**；
- 宽顶部区域仅裁 source 顶部天空，保留地面与底部城墙；窄顶部区域仅居中裁左右；
- Camera、Bg Transform、实体 render transform 和全部 battle/lockstep 数据保持零写入。

本阶段验收需新增 source-crop 数学 focused tests，重新取得 Desktop/Mobile Game screenshot，并至少静态确认新路径没有 Camera/Transform writer。实际有角色的 Android Player 仍由用户进行最终验收。

## 2026-08-25 首差与当前阻塞：Scene / Game 必须共用 world map

等比裁切本身已通过 focused `15/15`，且 Mobile / Desktop Game 截图分别达到无拉伸+底部黑区和全覆盖；但用户在真实 Scene/Game 并排图中确认：当前 target-camera-only clip-space background pass 与 Scene 中 `Bg (2)` 的 world Sprite 构图不同，令 green walkable bounds 不能根据 Scene 地图可靠调整。

因此本 Task 曾为 **`BLOCKED / USER ARCHITECTURE DECISION`**。用户已在 2026-08-25 批准下列替代架构，Task 恢复为 **`IN_PROGRESS / WORLD-ALIGNED CAMERA FRAME`**：

1. 删除背景的 clip-space 再绘制，令 `Bg (2)` 是 Scene 和 Game 的同一个世界地图；
2. 明确 Windows / Mobile 的 static world camera framing，使游戏画面与 Scene 的同一地图坐标一致；
3. 将 Android 底部黑区改为 world render 之后的 local presentation overlay，或经用户确认的同一 Camera 固定 framing；
4. 绝不把本地视觉 frame 写入 battle/lockstep/input/checksum/Stage。

上述第 2/3 项改变了本 Task 原来“绝不写 Camera”的边界；用户已明确授权，但仅限固定视觉取景的 `orthographicSize` 与 `transform.position.x/y`。不得恢复安全区、follow、`Camera.rect`、aspect writer 或任何 gameplay/lockstep writer。

## Stop Conditions

- 需要改 battle simulation、input、checksum、Stage、Transform 或 Camera follow；
- 新的完整-source + Android bottom-gap viewport 不能在 Unity Game View 中同时保留完整图片和仅底部黑色镂空；
- 场景 asset mutation 会覆盖/保存无关用户工作；
- 真实 Android 设备验收成为完成的唯一前置。

## Out of Scope

联机实现、服务器、HFR、AI、C++ gameplay、Shader、背景重绘、手机宽屏全屏策略、资产清理和提交。
