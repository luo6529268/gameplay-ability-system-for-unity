# CAMERA-PLATFORM-BACKGROUND-001 — single-texture desktop/mobile battle background presentation

<!-- CHANGE-RECORD
id: CAMERA-PLATFORM-BACKGROUND-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/App/BattleCameraSafeArea.cs
code-path: Assets/NTSD/Scripts/App/BattleBackgroundPlatformSelector.cs
code-path: Assets/NTSD/Scripts/App/BattleBackgroundPlatformPresentation.cs
code-path: Assets/NTSD/Scripts/App/BattleBackgroundBottomOverlayPresenter.cs
code-path: Assets/NTSD/Scripts/App/BattleBackgroundScreenPresenter.cs
code-path: Assets/NTSD/Scripts/App/Editor/BattleBackgroundPlatformAssetEditor.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleRenderFeature.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleBackgroundPlatformPresentationEditorTests.cs
authority: USER-DIRECTED-20260824 / UNITY-NATIVE-PRESENTATION / CROSS-PLATFORM-LOGIC-ISOLATION
evidence: PREIMPLEMENTATION-ASSET-SCENE-AUDIT-20260824
-->

> 创建日期：2026-08-24  
> 当前状态：`FOCUSED_TEST_PASS / EDITOR-RUNTIME-WITNESS / RUNTIME_PENDING`  
> 类型：Unity-native presentation / platform resource selection / camera framing

## Goal

以用户已经提供的单张 `2048 × 1152` 主背景纹理，建立 PC 与 Android/iOS 的本地表现选择：

- Windows / Editor / Standalone 显示完整 `2048 × 1152` 背景并覆盖完整 Camera viewport；
- Android / iOS 也显示完整、未裁切的 `2048 × 1152` 背景；只在最终本地 Camera presentation viewport 的**底部**预留可调黑色镂空区；
- Unity Edit Mode 可在 `Bg (2)` 的背景选择组件中显式预览 Desktop 或 Mobile，而无需切换 Build Target；
- 所有平台共用固定的逻辑战斗视野宽度和 16:9 逻辑高度；
- 背景 Sprite、Camera、屏幕比例和平台类型永不进入 `SimulationWorld`、`FrameInputSet`、checksum、Stage 边界或网络数据。

## Authority / User Decision

- 用户在 2026-08-24 明确要求执行 PC / 手机统一主背景方案；
- 这属于 Unity 表现层需求，不改变 C++ release battle authority、30 Hz tick、DAT、碰撞、输入或实体生命周期；
- `AGENTS.md` 第 5、6、7 节要求 Transform / Camera / SpriteRenderer 不得成为逻辑真相，且网络层只同步输入与校验数据。

## Observed Baseline

| 项目 | 已观察事实 |
|---|---|
| 主背景资产 | 用户已创建、尚未跟踪的 `Assets/NTSD/Sprite/OtherBg/battle_background_2048x1152.png`，实际为 `2048 × 1152`、16:9、`Format24bppRgb`。 |
| 场景绑定 | 用户已有 `Assets/NTSD/Scene/NTSD_Battle.unity` 将 `Bg (2)` 的 `SpriteRenderer` 绑定到该新资产。 |
| 当前导入状态 | 单 Sprite、PPU=100、`isReadable=1`、MipMap关闭；尚未存在 Desktop / Mobile 切片。 |
| 当前 Camera 组件 | `BattleCameraSafeArea` 在用户的 dirty Scene 中有绑定，但 `m_Enabled=0`；不得假定其已在运行。 |
| 旧资源对应 | 旧 `3011 × 1411` 图与新图中央 `2048 × 960` 区域不是可证明的像素级同一副本；移动端切片位置是用户要求的比例合同，视觉构图必须在真实 Play Mode 复核。 |

## Planned Contract

### 1. Single Texture / Two Sprite Rects

主纹理只导入一次；Sprite Editor 子资产不复制像素：

| Sprite 名称 | 像素 Rect | 平台 | 目的 |
|---|---:|---|---|
| `BattleBackground_Desktop` | `(0, 0, 2048, 1152)` | Windows、Editor、Standalone | 标准 16:9 完整覆盖。 |
| `BattleBackground_Mobile` | `(0, 0, 2048, 1152)`，pivot=`(0.5, 0.5)` | Android、iOS | 与 Desktop 使用同一完整 source rect；Android 底部镂空由 Camera presentation viewport 生成，而非裁切背景。 |

导入 PPU 设为 `68.017264`，因此两种 Sprite 的世界宽度与高度都为约 `30.1100025 × 16.9368764`。两个命名 Sprite 子资产保留，是为了维持现有平台选择器与 Inspector 预览的稳定引用；它们不复制 Texture2D 像素。

### 1.1 2026-08-24 构图纠正（已被后续用户反馈取代）

初版 Mobile 采用了 central rect `(0, 96, 2048, 960)` 与居中 pivot。实际 Editor-simulated Play
截图证明它会在**顶部和底部**各留下镂空。用户当时要求上层完整、只允许底部镂空，因此得出下一段的上对齐方案。

因此该初版 rect 不得交付。中间修正方案 `(0, 192, 2048, 960)` + pivot `(0.5, 0.4)` 保持主图上缘的
world-space 位置不变：Desktop 的 top 是 `+576 / PPU`，Mobile 的 top 是
`(1 - 0.4) × 960 / PPU = +576 / PPU`；Mobile bottom 则停在 `-384 / PPU`，仅在底部留
`192 / PPU` 的表现镂空。该中间结果之后被用户图片明确否决，因为它裁掉了背景下方地面；其历史证据保留，
但不得再作为当前交付合同。

### 1.2 2026-08-24 bottom-edge 对齐尝试（后续用户图片已取代）

用户明确的新要求是：**可以接受上层看不到，但不能接受下层看不到。** 因此 Mobile 必须保留原图下方地面，
并把唯一的 192 px 表现缺口移动到顶部。

当时使用的几何为 `(0, 0, 2048, 960)` + pivot `(0.5, 0.6)`：Desktop bottom 是 `-576 / PPU`，
Mobile bottom 是 `-0.6 × 960 / PPU = -576 / PPU`，二者完全对齐；Mobile top 则为
`(1 - 0.6) × 960 / PPU = +384 / PPU`，因此只在顶部出现 `192 / PPU` 的镂空。
这仍只改变 Sprite source rect/pivot，不写 Transform、Camera Transform 或 simulation。用户随后提供的 Game View
目标图澄清：可以裁掉的是**源图顶部**，但 Mobile slice 要在屏幕顶部对齐、镂空应留在屏幕底部；该尝试不再是当前合同。

### 1.3 2026-08-24 Game View 构图澄清（已被后续用户校正取代）

用户目标图显示的是“下方源图完整 + Game View 底部镂空”，而不是“屏幕顶部镂空”。当时错误地将其落实为
`rect=(0,0,2048,960)` + pivot `(0.5,0.4)`：

- rect 从主图底部开始，保留地面与底部城墙，裁掉的只有原图顶部 192 px；
- Mobile top 为 `(1 - 0.4) × 960 / PPU = +576 / PPU`，与 Desktop top 对齐；
- Mobile bottom 为 `-0.4 × 960 / PPU = -384 / PPU`，故在固定 16:9 Game View **底部**出现唯一的 `192 / PPU` 黑色镂空；
- Sprite source rect/pivot 是唯一可改的表现数据；不写 Background/Camera Transform、viewport、follow 或任何 simulation 状态。

### 1.4 2026-08-24 平台布局最终校正（当前合同，pre-code）

用户随后明确校正了平台语义：

- **Windows 平台**：完整 `2048 × 1152` 背景覆盖整个 presentation viewport，不存在底部镂空；
- **Android / iOS 平台**：同样显示完整 `2048 × 1152` 背景，背景的蓝天、地面和底部城墙都不得通过 source rect 裁切丢失；
- Android / iOS 的黑色镂空只位于物理显示表面的底部，比例必须通过 Camera Inspector 中公开的序列化字段调整；
- 旧的 `2048 × 960` rect、`.4` / `.6` pivot 和“以 Sprite 高度制造空白”的思路均为已否决历史，不得继续作为默认值、测试预期或交付证据。

实现只允许使用 **local presentation Camera viewport**：Desktop 使用 `Rect(0, 0, 1, 1)`；Mobile 使用
`Rect(0, bottomGapNormalized, 1, 1 - bottomGapNormalized)`。`bottomGapNormalized` 仅表示最终屏幕的底部黑色区域，
默认值先以参考图近似的 `1 / 9` 建立，并以 Inspector `Range` 公开给用户调整。Camera 必须使用该实际 viewport 的
native aspect（通过 `ResetAspect()`），而不是把 16:9 projection 强行映射到更宽的 Mobile viewport；这属于最终 raster
presentation，不改变任何 Scene Transform、battle world 或同步数据。实际 Game View 截图必须复核该布局是否满足用户图；
在未复核前不得声称像素构图已验收。

### 2. Camera Contract

`BattleCameraSafeArea` 只可由当前背景的**共享世界宽度**和固定逻辑 aspect `16 / 9` 计算正交尺寸：

```text
orthographicSize = backgroundWorldWidth / (2 × canonicalAspect)
```

它不得再以当前 Sprite 的高度决定视野。Desktop / Mobile 共享完整 Sprite 时，逻辑相机垂直范围仍相同。

用户已为本 Change 明确授权唯一例外：`BattleCameraSafeArea` 可以在 `ApplyPlatformPresentationViewport` 内写
`Camera.rect` 并重置为该 viewport 的 native aspect，目的是仅在 Android/iOS 的最终屏幕底部预留黑色镂空。该例外**不**授权
Camera Transform、follow、安全区、stage bounds、debug、screen-to-world 逻辑或任何 simulation writer。

### 3. Platform Selector Contract

新增的组件只在 `Awake` / `OnEnable` / `OnValidate` 选择本地 Sprite：

- `RuntimePlatform.Android` 与 `RuntimePlatform.IPhonePlayer` → Mobile；
- Editor、Windows、macOS、Linux 与未知平台 → Desktop；
- 不在 `Update` 中轮询，不分配，不移动/缩放任何 Transform；
- 对外公开当前有效 presentation platform，并在 Inspector preview 改变后通知绑定 Camera 刷新本地 viewport；
- 不读取或写入 Simulation、input、network、DAT、checksum 或 entity state。

### 3.1 Editor Preview Switch Contract — pre-code 2026-08-24

用户要求可在 Editor 模式直接查看电脑/手机背景效果。因此仅在 `BattleBackgroundPlatformSelector` 增加一个可序列化、
Inspector 可见的三态 preview 字段：`Automatic`、`Desktop`、`Mobile`。

- `Automatic`：继续按 `Application.platform` 解析；Editor 默认仍为 Desktop；
- `Desktop` / `Mobile`：在 **Unity Editor**（包括 Edit Mode 和 Editor Play Mode）覆盖本地 Sprite 选择，用于 Scene View / Game View 预览；
- Windows Player、Android/iOS Player 均在 `UNITY_EDITOR` 编译条件外忽略该 override，仍由真实 `RuntimePlatform` 选择；
- 预览变更只赋 `SpriteRenderer.sprite`，不写 Background/Camera Transform、camera size、simulation、input、checksum、Stage 或网络状态；
- 必须添加 focused tests，证明 Editor preview 能解析 Desktop/Mobile；Player resolver 的隔离由 `UNITY_EDITOR` 编译边界和静态审计证明。

## Allowed Files

| 路径 | 允许职责 |
|---|---|
| `Assets/NTSD/Scripts/App/BattleCameraSafeArea.cs` | 共享宽度 + canonical 16:9 size writer；在用户授权的窄范围内写入 Windows full / Android bottom-gap `Camera.rect`。不得恢复安全区、follow、Camera Transform、debug 或 simulation writer。 |
| `Assets/NTSD/Scripts/App/BattleBackgroundPlatformSelector.cs` | 无状态平台 Sprite selector，并将有效本地 presentation platform 直接通知其已绑定的 Camera presentation adapter。 |
| `Assets/NTSD/Scripts/App/Editor/BattleBackgroundPlatformAssetEditor.cs` | 显式、可重复的 Editor 菜单：配置主纹理为 Multiple Sprite、PPU、non-readable、no mipmap 和两个完整 source rect。不得自动运行或扫描/改写其他资产。 |
| `Assets/NTSD/Scripts/Test/Editor/BattleBackgroundPlatformPresentationEditorTests.cs` | platform resolver、完整 source rect、viewport 计算与单纹理 contract 的 focused tests。 |
| `Assets/NTSD/Sprite/OtherBg/battle_background_2048x1152.png.meta` | 仅由上述明确 Editor 操作更新导入/切片资料；PNG 像素不由本 Change 改写。 |
| `Assets/NTSD/Scene/NTSD_Battle.unity` | 仅通过现有 Unity Editor/MCP 将 selector 挂到 `Bg (2)`、绑定切片，并恢复本次所需的 Camera size 组件 enabled 状态；不得手写 YAML、重排或保存无关用户场景修改。 |
| `docs/ai/...` | 本 Change 的 Task、Ledger、State、Handoff 与最终证据。 |

## Explicitly Out of Scope

- C++ release、Unity battle simulation、FrameInputSet、lockstep、AI、collision、Stage、DAT、RNG、实体池和 CentralOnly renderer；
- 背景 PNG 像素重绘、透明扩展、Shader 延展、Texture2DArray 改造或多图平台资源；
- Camera follow、安全区、相机 Transform、Camera offset、调试 GUI/Gizmo；唯一例外是本 Record 1.4 / 2 节已明示的 Android bottom-gap `Camera.rect`；
- 手机物理宽屏（19.5:9 / 20:9）全屏布局策略。固定 16:9 logic view 下若存在横向额外显示区，属于后续产品布局决定，不得借本包重新引入安全区逻辑；
- 将用户已有 dirty Scene、资产或 GameConfig 的无关修改保存、清理、回退或提交。

## Actual Code Written

以下脚本已在本 Record 建立后写入。2026-08-24 已完成初版 importer/menu/binding/focused test 与短 Play；
初版 central 与中间 top-aligned Mobile 构图均被用户否决，最终已按 1.2 的下对齐合同完成重导入、编译、focused test 与 Editor-simulated Play。Android/iOS Player 或真机验收仍待用户执行：

| 文件 | 实际职责 |
|---|---|
| `Assets/NTSD/Scripts/App/BattleCameraSafeArea.cs` | 已移除“用完整 Sprite 高度/物理屏幕 aspect 取 contain max”的路径；下一步在现有 shared-width `orthographicSize` 基础上增加用户授权的 Windows full / Android bottom-gap presentation viewport writer。不会写 Transform、安全区、follow、debug 或 simulation。 |
| `Assets/NTSD/Scripts/App/BattleBackgroundPlatformSelector.cs` | 新建 `ExecuteAlways` 本地 Sprite selector。Android/iPhone 映射到 Mobile，其他/未知平台映射到 Desktop；仅在 enable/validate 时赋 `SpriteRenderer.sprite`，无 Update、无 Transform writer。 |
| `Assets/NTSD/Scripts/App/Editor/BattleBackgroundPlatformAssetEditor.cs` | 新建显式菜单 `NTSD/Presentation/Configure Platform Background Master`。只有用户/agent 执行菜单时才把用户主图配置为 Multiple Sprite，PPU=`68.017264`，无 mipmap、不可 CPU-readable、无 alpha，并写入 1.2 的底部对齐 rect/pivot。metadata 用 `SetDirty`、`WriteImportSettingsIfDirty`、`ForceUpdate` 明确持久化；不手写 `.meta`。 |
| `Assets/NTSD/Scripts/Test/Editor/BattleBackgroundPlatformPresentationEditorTests.cs` | 新建 resolver、canonical camera math、single-texture slice 和 importer memory-contract focused tests。 |

已实际运行初版菜单并完成 Scene binding：主纹理为一张 Texture2D、两个 Sprite sub-asset，`Bg (2)` 已挂 selector，
`BattleCameraSafeArea` 已启用且只写 shared-width camera size。初版 focused EditMode job `20ad2c86457c4dbba61df9c4fb06ac2b`
为 `9/9 PASS`，Desktop Play 已确认完整背景、camera size `8.468438`、background scale `(1,1,1)`。
这些证据不覆盖本节的新 Mobile bottom-aligned contract；必须在重导入、重新绑定、编译和新的 Mobile Play screenshot 后才可更新。

### Correction Failure Evidence — 2026-08-24 17:32

- 重新编译后，显式菜单确实写出 `[BattleBackgroundPlatform] Configured ...` 日志；
- 但 focused EditMode job `3fadcfbc3094422fbebb83be181f7d74` 失败于
  `MasterAsset_HasOneTextureAndTheRequiredTwoSlices`：期望 mobile rect y=`192`，实际仍为 y=`96`；
- 这证明初版 `TextureImporter.spritesheet` + `SaveAndReimport()` 在该现有 importer 上没有持久化新的 metadata；
  不是 platform resolver、camera size、Transform 或 battle runtime 的失败；
- 下一步仅允许把显式菜单的 importer write path 改为 `WriteImportSettingsIfDirty` 后的强制重导入，随后重跑同一
  focused test。不得手写 `.meta`、不得改 scene 以绕过该失败。

### Correction Resolution Evidence — 2026-08-24 17:34–17:37（中间结果，已被用户否决）

- 显式 menu 的写入路径已最小调整为 `EditorUtility.SetDirty(importer)` →
  `AssetDatabase.WriteImportSettingsIfDirty(path)` → `AssetDatabase.ImportAsset(path, ForceUpdate)`；
  只影响本主图 importer，未改 PNG 像素、其它资源或任何 gameplay；
- 重新执行 menu 后，实际 `.meta` 为 Mobile rect `(0,192,2048,960)`、`alignment: 9`、pivot `(0.5,0.4)`，
  两个子 Sprite 的既有 fileID 保持不变；
- focused EditMode job `60e60200476b4d968c0eb0be5cc4a2be`：`9/9 PASS`，其中包括单纹理、rect、PPU、
  non-readable/no-mipmap、Android/iPhone mapping、shared-width camera math 与 top-edge pivot assertions；
- Mobile editor-simulated Play：临时把 Editor 的 desktop field 指向同一资产的 Mobile sub-asset，仅为验证 resolver
  之外的画面，随后恢复 Desktop field 并由 Unity 正常保存 scene。运行时 Sprite bounds 为
  `30.1100025 × 14.1140633`、center y=`-5.2685933`，background Transform 保持
  position=`(-1.79,-6.68,0)`、scale=`(1,1,1)`，camera orthographic size 保持 `8.4684381`；
  截图 `Temp/battle-background-mobile-top-aligned-20260824.png` 显示顶部无镂空、仅底部镂空；
- Desktop Play：默认 resolver 选择完整 Sprite，bounds 为 `30.1100025 × 16.9368763`，相机仍为
  `8.4684381`、Transform 未改变；截图 `Temp/battle-background-desktop-final-20260824.png` 全覆盖，
  `BattleBackground` filtered Console error 为 0；
- 最终 Unity 状态：`NTSD_Battle` 已停在 Edit Mode 且 `isDirty=false`；filtered `error CS` 和
  `BattleBackground` Console error 均为 0；
- `Tools/Validate-ChangeLedger.ps1` 最终为 `PASSED`（99 records，126 governed code files covered）；
  scoped `git diff --check`（代码、资源 meta、本 Record/Task/Handoff/Ledger/STATE，不含 scene）通过。
  若把当前用户/Unity 已大量变更的 `NTSD_Battle.unity` 纳入 diff check，会命中 Unity 序列化的
  `m_Name: ` / `m_EditorClassIdentifier: ` trailing whitespace；不手工格式化或覆盖该用户 scene；
- `BattleRuntimeSelfCheck` 未重跑：本 Change 没有改 simulation、input、DAT、collision、stage 或 battle runtime；
  上述 focused tests 与实际纯表现 Play 是该窄范围的验证，不能被表述成任何 C++ gameplay 对齐结论；
- 该中间结果证明 selector/camera/导入路径正常，但不满足用户对下方地面的最新要求；其不再作为当前交付证据。

### Bottom-edge Alignment Evidence — 2026-08-24 17:48–17:55（后续用户图片已取代）

- 已通过唯一的显式 Unity Editor 菜单重新导入主纹理。实际 `.meta` 的 Mobile slice 为
  rect `(0,0,2048,960)`、`alignment: 9`、pivot `(0.5,0.6)`；Desktop 保持 `(0,0,2048,1152)`、pivot `(0.5,0.5)`。
  两 Sprite 继续引用同一 Texture2D，既有 sub-asset fileID 未变化；PNG 像素没有被改写；
- 几何关系已由 focused test 固化：两 Sprite 的底边同为 Desktop 的 `-576 / PPU`，Mobile 仅失去原图顶部 `192 px`；
  不存在底部裁切或 Transform 补偿；
- focused EditMode job `711ef7d793d9427b9fb08029f8018062` 为 `succeeded`，`9/9 PASS`；
- 本次末尾通过已打开 Unity Editor 强制刷新并请求编译；域重载后 Editor ready，`error CS` filtered Console 为 `0`，
  `BattleBackground` filtered error 为 `0`；
- Mobile editor-simulated Play 使用实际 Mobile Sprite：bounds=`30.1100025 × 14.1140633`、center y=`-8.0914059`；
  `Bg (2)` Transform 仍是 position=`(-1.79,-6.68,0)`、scale=`(1,1,1)`，Camera orthographic size 仍为 `8.4684381`。
  `Temp/battle-background-mobile-bottom-aligned-20260824.png` 显示唯一黑色镂空在顶部，地面和最底部城墙均完整可见；
- Desktop Sprite 与 shared-width camera formula 没有因本次 Mobile rect/pivot 修正而改变；其已有实际 Play 证据
  `Temp/battle-background-desktop-final-20260824.png` 仍显示完整 16:9 背景、scale=`(1,1,1)`、camera=`8.4684381`；
- Unity 当前 active scene 为 `Assets/NTSD/Scene/NTSD_Battle.unity`，`isDirty=false`，且 Editor/Windows 默认 resolver 仍会选取
  Desktop Sprite。该 Record 因真实 Android/iOS Player 或真机尚未运行而停在 `RUNTIME_PENDING`，不得升级为 `VERIFIED`。
- 最终治理检查：`Tools/Validate-ChangeLedger.ps1` 为 `PASSED`（99 records、126 governed code files covered）；
  scoped `git diff --check` 通过（只有既有的 LF→CRLF warning，非 whitespace error）；对本 Change 三个 production/Editor 脚本的
  static scan 未发现 `localScale`、`transform.`、`Camera.rect`、follow/safe-area/debug writer、`SimulationWorld`、
  `FrameInputSet`、checksum 或 Stage 引用。

### Editor Preview Switch Pre-code Record — 2026-08-24

- 用户在确认 bottom-aligned Mobile 画面后，要求公开一个 Inspector 属性，用于在 Editor 模式直接在 Desktop/Mobile 间切换；
- 本 Record 已在任何本次脚本变更前重新打开为 `IN_PROGRESS`；允许修改仅限 selector 与它的 focused Editor tests，
  以及本 Record / Task / Ledger / State / Handoff；
- 不改变已经验证的 sprite rect/pivot、PPU、Camera width formula、scene Transform、实际 Player runtime resolver 或任何 battle/lockstep 状态；
- 验收：Inspector 字段在 `Bg (2)` 的 selector 组件可见；Edit Mode 切换即时切 Sprite；Play Mode 不受该字段影响；
  compile、focused tests、Unity scene default restore、ledger validator 与 scoped diff 必须重新通过。

### Mobile Game View Anchor Correction Pre-code — 2026-08-24

- 用户新截图明确当前 `.6` pivot 的“顶部黑色镂空”不是目标，必须改为 `.4`，使底部黑色镂空与截图一致；
- 只允许改 `BattleBackgroundPlatformAssetEditor.MobilePivotY`、对应 focused geometry assertions、由显式 Editor 菜单写入的本主图 importer metadata，
  以及本 Change 的留痕文件；selector preview 的三态和 Player anti-leak 合同不得改变；
- 写入前 Change 状态回到 `IN_PROGRESS`；验收需要重新导入、Unity compile、focused test、Editor `Mobile` preview screenshot、
  `Automatic` restore、Console、ledger validator 与 scoped diff；Android/iOS Player 仍由用户验收。

### Mobile Game View Anchor Correction Code Written — 2026-08-24

- `BattleBackgroundPlatformAssetEditor` 的 Mobile rect 保持 `(0,0,2048,960)`，仅将 `MobilePivotY` 由 `.6` 改为 `.4`；
  将原本含糊的 `MobileTopGapHeight` 改名为 `MobilePresentationGapHeight`；
- focused geometry assertions 已从“底边相等”改为“顶边相等 + Desktop bottom distance 减 Mobile bottom distance = 192px”，
  直接锁定“lower-source 完整、Game View 底部留空”的合同；
- selector editor preview、Camera、Transform、Player resolver、scene 默认字段及任何战斗/同步路径没有修改；
- 当前为 `CODE_WRITTEN`；本主背景显式 reimport、compile、focused test、Mobile preview Game View screenshot、Automatic restore、
  console 和治理检查尚待执行。

### Editor Game View Preview Scope Correction Pre-code — 2026-08-24

- 用户目标图来自 Unity Game View，要求在 Editor 内实际看到 Desktop/Mobile 构图；此前限定 `!Application.isPlaying` 过窄；
- 允许将 selector preview gate 改为 `UNITY_EDITOR` 内无条件应用，因而 Editor Play Mode 也可预览；Player build 的
  preprocessor path 不引用 preview override，仍走真实 RuntimePlatform；
- 必须更新 focused tests：删除“Editor Play 忽略 preview”的旧断言，改为“Editor Play 仍使用 preview”；并继续保留
  `Automatic`→当前平台的测试；
- 只允许 selector/test/docs 修改；不改 Camera、Transform、simulation、input、checksum、Stage、network 或实际 Player resolver。

### Editor Game View Preview Scope Correction Code Written — 2026-08-24

- `BattleBackgroundPlatformSelector.ResolvePresentationPlatform(platform, previewMode)` 已移除 `isPlaying` gate；在 `UNITY_EDITOR`
  分支中 `Desktop` / `Mobile` preview 对 Editor Edit Mode 和 Editor Play Mode 都生效；
- Player 编译时该 `switch` 被预处理移除，方法只调用原始 `ResolvePresentationPlatform(platform)`；preview field 不改变真实 Player
  平台选择；
- focused tests 已改为 Editor 内的 `Automatic` 仍随平台、`Desktop` 能覆盖 Android、`Mobile` 能覆盖 Windows 的合同；
- 当前仍为 `CODE_WRITTEN`：要与 `.4` pivot 重导入一起重新编译、跑 tests、在 Editor Game View 实测截图、恢复 Automatic 并审计。

### Editor Preview Switch Code Written — 2026-08-24（Game View scope correction 前的历史）

- `BattleBackgroundPlatformSelector` 已新增 `BattleBackgroundEditorPreviewMode`（`Automatic` / `Desktop` / `Mobile`），
  `editorPreviewMode` 使用 `SerializeField + InspectorName("编辑器平台预览")` 显示在 `Bg (2)` 的组件 Inspector；
- `EditorPreviewMode` public property 供代码调用；Inspector 改值通过既有 `OnValidate()` 即刻重选 Sprite；
- 新 overload 只在 `UNITY_EDITOR && !isPlaying` 时使用 preview override。Play Mode 和 Player 编译路径一律回落到
  原有 `RuntimePlatform` resolver，因此预览值不会成为打包/网络/战斗状态；
- focused tests 已新增 `Automatic/Desktop/Mobile` 的 Edit Mode 行为、以及“Mobile preview + Windows playing 仍为 Desktop”与
  “Desktop preview + Android playing 仍为 Mobile”两个防泄漏断言；
- 上述为代码写入时的即时状态；后续验证结果见下一节。

### Editor Preview Switch Verification — 2026-08-24 18:16–18:19（Game View scope correction 前的历史）

- Unity scripts force refresh/compile 完成；`error CS` filtered Console=`0`；
- focused EditMode job `3a774c11142e45c0bb9024e696dc859f` 为 `succeeded`，`14/14 PASS`。其中新增的五项用例覆盖
  Automatic/Desktop/Mobile 非 Play Editor 解析，以及 Mobile preview 在 Windows Play 仍解析 Desktop、Desktop preview 在
  Android Play 仍解析 Mobile；
- UnityMCP 在 Edit Mode 对 `Bg (2)`（instance ID `54404`）实际设置 `editorPreviewMode=2`。组件资源读取到
  `EditorPreviewMode=2`，Sprite bounds 立即变为 `30.1100025 × 14.1140633`、center y=`-8.0914059`，证明实际选取了
  Mobile slice；Transform 仍为 position=`(-1.79,-6.68,0)`、scale=`(1,1,1)`；
- 随后实际恢复 `editorPreviewMode=0`（Automatic）并用 Unity 正常保存场景。组件资源读取到
  `EditorPreviewMode=0`，Sprite bounds 恢复为 `30.1100025 × 16.9368763`、center y=`-6.68`，即 Desktop 默认；
  `NTSD_Battle` active scene 最终为 `isDirty=false`；
- 验证结束时 `BattleBackground` filtered Console error=`0`。本 Change 仍为 `RUNTIME_PENDING` 的唯一原因是 Android/iOS
  Player 或真机没有运行；不能因此把纯 Editor preview 验证写成 Android 运行时验收。

### Mobile Game View Anchor Correction Verification — 2026-08-24 18:49–18:56

- 第一次显式 menu 调用发生在新脚本程序集加载前，`.meta` 仍为旧 pivot `.6`；此 stale-editor-assembly 结果未用作证据。
  随后强制 scripts refresh/compile，`error CS` filtered Console=`0`，再执行同一菜单并收到
  `[BattleBackgroundPlatform] Configured ...` 日志；实际 `.meta` 已为 rect `(0,0,2048,960)`、pivot `(0.5,0.4)`；
- focused EditMode job `f3fa58aea9f6444f9448d556814f4834` 为 `succeeded`，`14/14 PASS`，覆盖新 top-edge/bottom-gap
  几何和 Unity Editor（含 Game View）preview resolver；
- UnityMCP 在 Editor 中实际设置 `Bg (2).editorPreviewMode=2`，组件 witness 显示 `EditorPreviewMode=2`、Mobile bounds
  `30.1100025 × 14.1140633`、local center y=`+1.4114065`；现有 Transform 维持 position=`(-1.79,-4.8,0)`、
  scale=`(1,1,1)`，本 Change 没有 Transform writer；
- 在 Editor Play Mode 截取的 `Temp/battle-background-mobile-gameview-target-20260824.png` 显示 lower-source 的地面与
  最底部城墙完整，背景贴住 Game View 顶部，唯一黑色镂空在底部；
- 与用户参考图并列核对后发现：参考图还保留明显蓝天，而当前 `2048×960` lower-source crop 为保留最底部内容已裁掉更多
  源图顶部。两者不能由当前固定高度 crop 同时精确满足；是否要求保留参考图同等蓝天是 `UNKNOWN / USER DECISION REQUIRED`。
  可能的后续方案会涉及更高 Mobile rect 或纯渲染尺寸/UV 适配；用户此前禁止 background Transform 缩放，因此本 Change
  不得自行选择或实施其中任何一项；
- 停止 Play 后重新查找当前 `Bg (2)`，恢复 `editorPreviewMode=0`（Automatic），组件 witness 显示 Desktop bounds
  `30.1100025 × 16.9368763`、local center y=`0`；随后用 Unity 正常保存 `NTSD_Battle`；
- 截图期间 `BattleBackground` filtered Console error=`0`。由于上述参考图蓝天差异尚待用户裁决，本 Change 保持
  `IN_PROGRESS`；真实 Android/iOS Player/真机也仍未运行，故不得升级为 `VERIFIED`。

## Acceptance

1. 主纹理仍是一张 `2048 × 1152` RGB Texture2D，`isReadable=false`、MipMap=false；
2. 同一资产存在准确命名的 Desktop / Mobile 两个 Sprite 子资产，二者共享同一 Texture2D；
3. 两 Sprite 的世界宽度相等，PPU 和 rect 几何满足上述数值合同；
4. Desktop / Android / iOS resolver 的 platform mapping focused test 通过；
5. camera size 仅由共享宽度与 canonical 16:9 推导；Mobile Camera 使用真实 viewport aspect，静态检查确认无 Transform/follow/safe-area writer，且唯一 `Camera.rect` 写入为本 Record 明示的 Android bottom-gap presentation writer；
6. 现有 Unity Editor 编译为 0 C# error；
7. Desktop Play Mode：完整 Sprite、camera size、背景 scale `(1,1,1)`、Console 无本 Change error；
8. Mobile 分支：focused tests 与本地 resolver witness 必须证明选择 Mobile platform；Editor-simulated
   Game View screenshot 必须显示完整 source 的蓝天、地面和底部城墙，人物/武器保持原始宽高比；允许为此在顶部 viewport 左右出现必要的窄黑边，同时底部有可调黑色镂空；真实 Android build/device 验收明确标记为待用户执行，
   不伪称已完成；
9. `Tools/Validate-ChangeLedger.ps1` 与 scoped `git diff --check` 通过。

## Stop Conditions

- 需要让背景 Bounds / Camera / ScreenToWorldPoint 参与战斗逻辑或网络状态；
- 需要修改 Background Transform、Camera Transform、follow 或 safe-area；
- 需要在本 Record 明示的 Android bottom-gap `Camera.rect` 之外扩大相机 writer；
- Unity AssetImporter 无法可靠创建两个切片且不触及其他用户资产；
- 任何测试显示 Sprite 或 camera 的本地变化进入 checksum / simulation state。

## Rollback

回滚仅撤销本 Change 新增 selector、camera width-only公式、Editor 菜单、focused tests 与本 Change 写入的 `.meta` / scene component binding；不得删除用户创建的 `battle_background_2048x1152.png`、不得回退用户已有 scene / GameConfig / asset 修改。任何 Git 回退操作需用户另行明确批准。

### Windows Full / Mobile Adjustable Bottom-gap Code Written — 2026-08-24

本节是 1.4 的实际代码记录，覆盖此前所有 Mobile `2048×960` crop 合同；尚未执行 Unity 编译、重导入或运行时验证。

- `BattleBackgroundPlatformAssetEditor`：`MobileHeight` 已改为 `MasterHeight`，`MobilePivotY` 改为 `.5`，Desktop/Mobile metadata 都是
  `(0,0,2048,1152)` + centered pivot。同一 Texture2D 与两个命名 Sprite 子资产的结构不变；需通过唯一显式菜单实际写入 `.meta`。
- `BattleBackgroundPlatformSelector`：新增无分配的 `EffectivePresentationPlatform` 和
  `PresentationPlatformChanged` event；切换 Inspector 的 `Automatic / Desktop / Mobile` preview 后只在有效 platform 变化时通知
  presentation Camera。更新 tooltip，明确 Unity Editor（含 Editor Play）可预览、Player 仍使用真实平台。
- `BattleCameraSafeArea`：新增序列化 `backgroundPlatformSelector` 与 Inspector 字段
  `Android 底部镂空比例`（默认 `1/9`、范围 `0..0.5`）。`RefreshPresentationCamera` 维持 shared-width orthographicSize，
  并且仅在本 Change 授权的局部调用 `CalculatePresentationViewport`：Desktop 写 full rect，Mobile 写 bottom-gap rect；
  同时以 canonical 16:9 作为 projection aspect。没有任何 Camera Transform、Background Transform、safe-area、follow、
  debug、simulation、input、checksum、Stage 或网络 writer。
- `BattleBackgroundPlatformPresentationEditorTests`：移除了已废止的 960px/pivot-gap 断言；新增 Desktop full rect、Mobile
  default gap、gap clamp 和完整 source Sprite contract。所有这些是 code-level tests，尚未运行。

尚未验证项：Unity C# compile、显式 reimport 后的 `.meta`、focused test、Camera 与 selector 的真实 Scene 绑定、Windows 和
Mobile Editor Game View、可调 gap witness、Automatic restore、console、ledger validator、scoped diff、Android/iOS Player/真机。

### Aspect-preserving Mobile Correction — 2026-08-24（pre-code）

用户在实际 Game View 中报告“人物被压扁”。这是已观察到的实现缺陷，不是战斗 runtime 或资源问题：先前实现把
canonical `16:9` projection 写入高度只有 `1 - gap` 的 viewport。以默认 `gap=1/9` 与 16:9 Game View 为例，物理
viewport aspect 变成 `2:1`，导致所有 world Sprite 横向拉伸 `2 / (16/9) = 1.125`（约 12.5%）。

当前修正只允许：

- 移除 `BattleCameraSafeArea` 对 `Camera.aspect=canonicalBattleAspect` 的写入，改为在 rect 变更后调用 `Camera.ResetAspect()`；
- 背景与实体都使用真实 viewport aspect，因此保持相同且未变形的 world-to-pixel ratio；
- 为使完整 16:9 source 在更宽的 Mobile viewport 中不裁切，左右会出现必要的窄黑边；默认 `gap=1/9` 时每侧约为
  屏宽的 `5.56%`。这不是 Transform 缩放，不影响逻辑位置；
- 将 selector 与 Camera 的局部 presentation reference 明确接线，使 Editor 中切换 Desktop/Mobile preview 时能立刻刷新
  viewport，不依赖之前未能闭合的 event-only path；
- 不修改 Sprite source、Background/Camera Transform、逻辑 tick、实体位置、碰撞、input、checksum、Stage 或网络数据。

在用户报告前取得的 Mobile “full width + bottom gap”截图不再可作为验收证据；它已经显示了不允许的形变。修正后必须
重新编译、重跑 focused test、做 Desktop/Mobile Game View、确认人物宽高比、恢复原有 Desktop preview与默认 gap、再做治理检查。

### Aspect-preserving Mobile Correction Code Written — 2026-08-24

- `BattleCameraSafeArea.RefreshPresentationCamera` 已公开为 presentation-only refresh；仍只写它自己的 `Camera.rect`、
  `ResetAspect()` 与 shared-width orthographic size。错误的 `Camera.aspect=canonicalBattleAspect` 写入已删除；
  `ResetAspect()` 使 16:9 source 在 Mobile 的实际宽 viewport 中保持正确宽高比，而不是拉伸所有实体；
- `BattleBackgroundPlatformSelector` 新增序列化 `presentationCamera` 引用。每次本地 sprite/platform 应用都会直接请求该
  Camera refresh；保留原 event 作为既有观察接口，但 Camera 不再只依赖 event 订阅链；
- 需要通过已打开的 Unity Editor 显式绑定 `Bg (2).presentationCamera → ScenesCamera.BattleCameraSafeArea`；
  不手写 scene YAML。随后重新 compile、focused、Desktop/Mobile actual Game View、gap witness、恢复用户原有 Desktop preview / default gap、
  Console、validator 与 scoped diff。当前不可称为 runtime verified。

### Direct Camera Link Persistence Refinement — 2026-08-24（pre-code）

当前 `NTSD_Battle.unity` 在本 Change 开始前已经是 user-dirty。将 `presentationCamera` 写成序列化 Scene reference 虽可工作，
但需要保存整个 dirty scene，可能把无关用户修改一并写入。该风险不必要。

因此当前仅允许把 selector 的 direct Camera link 改为**非序列化的局部缓存**：在 `OnEnable` / `OnValidate` 的
`ApplyLocalPlatformSprite()` 中首次使用 `FindObjectOfType<BattleCameraSafeArea>()` 取得已激活的 presentation adapter，再调用其
refresh。该查询不在 Update、LateUpdate、fixed tick、input、collision 或 render command hot path 中运行；不会修改 Scene、
Transform、simulation或网络状态。已有被临时赋值的 Scene reference 不保存，编译/域重载后由该局部解析替代。

### Direct Camera Link Persistence Refinement Code Written — 2026-08-24

`BattleBackgroundPlatformSelector.presentationCamera` 已从 `[SerializeField]` 改为非序列化 local cache。`RefreshPresentationCamera()`
仅在 selector 应用 platform/sprite（OnEnable、OnValidate或用户切 preview）时，且 cache 为 null 时调用一次
`FindObjectOfType<BattleCameraSafeArea>()`；随后只刷新本机 presentation Camera。它不在战斗 tick / Update / LateUpdate / fixed tick
执行，也不写 scene。此前 UnityMCP 临时赋给该字段的 scene reference 会在此脚本域重载后自然消失；无需保存 user-dirty scene。

当前仍需 Unity compile、focused test、Desktop/Mobile Game View、gap witness、Console、ledger validator和diff；不得将旧的任何截图
升级为本修正的运行时证据。

### Camera-owned Editor Preview Correction — 2026-08-24（pre-code）

实际验证发现 selector-only preview 的跨组件通知并不可靠：Mobile 切换能产生 bottom-gap，随后 Desktop 切换却可能留下旧
`Camera.rect`，Windows screenshot 仍有黑区。这是 presentation lifecycle first-difference，不是 battle runtime 状态。

为消除两个 Inspector 字段和事件执行次序的歧义，当前合同改为：

- `BattleCameraSafeArea` 是唯一公开的 Editor preview owner，Inspector 显示 `编辑器平台预览`（`Automatic / Desktop / Mobile`）和
  `Android 底部镂空比例`；
- Camera 直接由自己的 preview 值计算 Desktop full / Mobile bottom-gap rect，并在 `OnEnable` / `OnValidate` 同步
  selector 的内部 preview；
- selector 的同名内部字段保留为 platform Sprite resource selection，但从 Inspector 隐藏，且不再持有 Camera reference、event 或
  global lookup；
- Player 编译路径仍在 `UNITY_EDITOR` 之外忽略 preview，使用真实 `RuntimePlatform`；
- 仍不写 Camera/Background Transform、battle state、input、checksum、Stage或网络数据。

这会删除前一小节新增的 event/cache 方案，因为它未通过 Windows full runtime witness。之后必须重新编译、重新运行 focused tests，并以
Camera Inspector 先后切 Desktop/Mobile 的真实 Play screenshot 验收。

### Camera-owned Editor Preview Correction Code Written — 2026-08-24

- `BattleCameraSafeArea` 已新增公开 Inspector `编辑器平台预览` / `EditorPreviewMode`；在 `OnEnable`、`OnValidate` 和 property setter
  中同步 selector 的内部 preview，并由自己的字段直接解析 `Camera.rect`；
- `BattleBackgroundPlatformSelector.editorPreviewMode` 保留序列化但使用 `HideInInspector`，避免用户在两个地方修改同一预览值；
  selector 已移除未可靠的 `PresentationPlatformChanged`、Camera reference与`FindObjectOfType` cache，重新成为只切本地 Sprite 的组件；
- 因此前临时 reference 是未保存内存状态，删除该字段不会回退或改写 user-dirty scene；旧 YAML 若存在未知字段会由 Unity 忽略，
  本 Change 不保存全场景；
- Player build 仍由 `ResolvePresentationPlatform(platform, preview)` 的 `UNITY_EDITOR` 边界忽略 preview，使用真实 RuntimePlatform；
- 尚未重新编译/测试/视觉验证，不能以旧 `Camera.rect` / screenshot 声称该修正已成功。

### Shared Camera Viewport Runtime Failure — 2026-08-24

本节记录本 Change 的最新实际验证，覆盖上节“尚未重新编译/测试/视觉验证”的待验证状态；它不是完成证据。

**已测量：**

- 当前已打开 Unity Editor 以 `refresh_unity(mode=force, scope=scripts, compile=request)` 完成脚本刷新；Console 未出现 `error CS`。仅有 MCP stdio client 关闭时写入的 `Client handler exited` 条目，它们不是 Unity C# 编译错误；
- focused EditMode job `729aeb5c5d7d469aad6532c648ad06ea` 为 `17/17 PASS`，覆盖平台 resolver、完整 source Sprite、Desktop full rect、Mobile gap 与 gap clamp；
- 在 `ScenesCamera.BattleCameraSafeArea.editorPreviewMode=Desktop` 的 Editor Play Mode 下，Game View screenshot `Temp/battle-background-windows-camera-preview-20260824-final.png` **仍有底部黑区**；随后直接把运行时 `ScenesCamera.Camera.rect` 写为 `(0,0,1,1)`，`Temp/battle-background-rect-direct-full-diagnostic-20260824.png` 画面仍不变；
- 因此，当前 shared-camera `Camera.rect` 方案既没有给出 Windows full 的可靠 runtime witness，也不能作为“角色不变形”的交付路径。场景在本 Change 前已是 dirty；验证后已停止 Play，未保存 `NTSD_Battle.unity`。

**用户再次明确的视觉合同：**

- Windows：完整背景全覆盖整个屏幕；
- Android/iOS：画面应与用户提供的参考图一致，底部黑色镂空可调；
- 角色、武器和其他战斗实体不得因为平台布局而改变宽高比。

**几何结论（已确认）：** 默认 `gap=1/9` 时，1920×1080 的上部可视区是 1920×960，即 `2:1`；完整背景和战斗世界是 `16:9`。单一 Camera 若让 16:9 世界填满 2:1 上部区，会产生 `2 / (16/9) = 1.125` 的横向形变；若保持比例，则必然出现左右 pillarbox。因而“全宽、完整 source、仅底部黑区、背景和实体都不变形”不能由同一 Camera viewport 同时满足。

**当前 blocker / 不得自行扩大：** 用户此前禁止改变背景 Transform，并曾否决单纯渲染延展。要精确复现最新 Android 参考图且让实体保持比例，后续必须获得用户对以下 presentation-only 边界的确认：是否允许仅背景使用独立的本地合成/绘制路径以适配该画面，而 battle entity camera 始终保持固定 16:9；若背景也必须保持原始比例，则 Android 必须接受左右 pillarbox。两种选择都不改变 Transform、战斗位置、tick、输入、checksum 或网络数据，但前者会改变背景的纯视觉像素比例。未确认前，禁止继续修改 gameplay、scene、Camera Transform 或背景 Transform，也不得把本 Change 标为完成。

### Background-only Composition Authorization — 2026-08-24（pre-code）

用户在看到上述边界后明确回复“开始修复”，授权按最新目标继续。当前实现合同调整为：

- `ScenesCamera` 在 Windows、Android 和 iOS 均保持 full rect 与原生 16:9 战斗投影；不得再用 platform gap 修改 battle entity camera viewport/aspect；
- 现有 `SpriteRenderer` 不改 Transform；背景由一个不保存到 scene、仅表现层使用的 clip-space quad 绘制。Windows quad 覆盖完整屏幕；Mobile quad 只覆盖屏幕顶部 `1-gap`，完整 source UV 映射到该区域，底部由 Camera clear color 保持黑色；
- 背景 quad 使用 `Background` render queue，在战斗实体之前绘制；角色、武器、阴影与其他世界对象仍由完整 `ScenesCamera` 以正常 pixel aspect 绘制，因此实体不会被压扁；
- background-only quad 的像素适配不写 `Background Transform`、`Camera Transform`、simulation、input、checksum、Stage 或网络数据；不改变跨平台逻辑坐标；
- 运行时/Editor preview 辅助对象必须使用 `HideAndDontSave`，不得保存或重写 user-dirty `NTSD_Battle.unity`；若专用 Shader 不可用，必须保留原 SpriteRenderer，不能让背景消失；
- focused tests 必须改为“Camera always full + background viewport platform/gap”；实际 Desktop/Mobile Game View 必须出现角色并证明实体宽高比不变。Android/iOS真机仍由用户后续验收。

### Background-only Composition Code Written — 2026-08-24

- 新增 `BattleBackgroundScreenPresenter`：仅创建 `HideAndDontSave` quad/Mesh/Material，使用 source Sprite 的 texture/UV；专用资源成功时临时设置 `SpriteRenderer.forceRenderingOff`，释放/失败时恢复原值；不创建或保存 Scene 组件；
- 新增 `Resources/BattleBackgroundScreen.shader`：Background queue 全屏 quad，Desktop gap=0 时显示完整 source；Mobile 在 shader 内把完整 source UV 映射到顶部 `1-gap`，底部输出黑色。该映射只作用背景像素；
- `BattleCameraSafeArea`：Camera viewport 全平台固定 full rect并 `ResetAspect()`；仍按共享背景宽度计算 canonical orthographic size；平台和gap只传入背景 presenter；OnDisable/OnDestroy释放临时对象；
- focused tests 已改为 Camera always-full、background-only viewport/gap/clamp，并新增 Resource Shader 可用性断言；
- 当前只到 `CODE_WRITTEN`；Unity import/meta、shader compile、C# compile、focused tests、Desktop/Mobile有角色Game View、fallback、Console、scene未保存确认、validator与scoped diff均待。

首次 Desktop Game View `Temp/battle-background-desktop-background-only-final-20260824.png` 证明 transient quad 已实际绘制并覆盖全屏，但纹理 Y 轴倒置；该图是 FAILED witness。Shader sourceY 已改为对 D3D/URP sprite texture 做纵向反转，其他 Camera、gap、entity 与 Transform 合同未改。修正后的 shader import/compile/focused/Game View 均需重跑。

UV Y 修正后的 Desktop screenshot `Temp/battle-background-desktop-background-only-uvfixed-20260824.png` 已显示正向完整背景并覆盖全屏。随后 Mobile screenshot `Temp/battle-background-mobile-background-only-uvfixed-20260824.png` 显示完整背景与gap，但黑区错误出现在顶部；该图是 FAILED witness。实测说明 clip-space quad 的screen UV原点在左上，故gap条件已改为 `uv.y > 1-gap`，sourceY改为在顶部 `1-gap` 内从texture top映射到bottom。Camera/entity/Transform合同未改，Mobile需重跑。

用户随后报告 Play 中 Scene 视图看不到背景。根因已静态闭合为 presenter 对 source `SpriteRenderer.forceRenderingOff=true` 的全局副作用；该属性不区分 Game/Scene camera。当前修正计划是删除所有 source visibility ownership/force-off 写入，让原 SpriteRenderer继续提供 Scene world preview；screen presenter改到 source 相同 sorting layer 的下一sorting order并进入普通 Transparent pass，先覆盖Game中的原背景，中央战斗实体仍由既有 `AfterRenderingTransparents` feature随后绘制。该改动只增加一个背景 draw，不改scene/layer/Transform/camera/battle数据；Shader失败时原SpriteRenderer自然保底。

SceneView修正代码已写：presenter不再读写`forceRenderingOff`，source SpriteRenderer始终启用；screen quad使用Transparent queue、相同sorting layer和`source.sortingOrder+1`。Dispose/失败不需恢复scene状态，fallback直接是原SpriteRenderer。需重新导入、compile、focused，并以同时包含Scene与Game的截图确认地图/实体/底部gap。

用户随后澄清：Scene 里需要看到 `Bg (2)` 世界对象本身，而不是screen quad铺满Scene视口。最新截图证明全局MeshRenderer presenter也会进入Scene camera，故上一段sorting修正仍不合格。当前允许的最小改法是：

- 删除transient GameObject/MeshRenderer；presenter只保留Mesh/Material/PropertyBlock与目标`ScenesCamera`引用；
- 接入既有 `BattleRenderFeature` 的独立 `BeforeRenderingOpaques` background pass，仅当rendering camera与目标`ScenesCamera`相同才入队；SceneView/HUDCamera不执行；
- 在SRP begin/end camera rendering期间，仅对目标Game camera临时`forceRenderingOff` source SpriteRenderer，并在该camera结束时恢复；因此Scene camera仍正常看到`Bg (2)`，Game camera只看到专用screen background；
- renderer feature不可用时绝不隐藏source，回退原SpriteRenderer；不修改scene、layer、Transform、central entity pass顺序或battle数据；
- 已取得的最新Game截图 `Temp/battle-background-mobile-bottom-gap-final-20260824.png` 已证明底部gap shader公式正确；URP camera-only改造后必须重新验证该结果与Scene world object。

### Game-camera-only URP Pass Code Written — 2026-08-24

- `BattleBackgroundScreenPresenter` 已删除transient GameObject/MeshRenderer，改为只持有`HideAndDontSave` Mesh/Material、PropertyBlock、目标Camera和source Renderer；
- presenter注册SRP begin/end camera回调，仅在目标ScenesCamera且`BattleRenderFeature`已注册时临时隐藏source，并在该camera结束后恢复；SceneView/HUDCamera从不触发；
- `BattleRenderFeature` 新增独立 `BeforeRenderingOpaques` pass，只有presenter camera匹配时入队；既有central pass仍保持`AfterRenderingTransparents`与原submission逻辑，未改实体draw顺序；
- feature缺失时presenter不会隐藏source，保持原SpriteRenderer fallback；scene/layer/Transform/battle数据均未写；
- 当前只到`CODE_WRITTEN`，fresh compile/shader、focused、Scene world Sprite、Desktop/Mobile Game、有角色比例、Console、scene未保存、validator/diff待。

### Game-camera-only URP Pass Editor Runtime Evidence — 2026-08-24 22:33–22:42

- Unity focused EditMode job `75282a5423a84726a924c0fb7a87da07`：`19/19 PASS`；覆盖 Camera 全 rect、Mobile bottom-gap/clamp、单 Texture/双完整 Sprite、无 CPU readable/mipmap、平台与 Editor preview resolver、Resources Shader 可用性；
- Mobile Editor Play Game View：`Temp/battle-background-mobile-camera-only-pass-final-20260824.png`。完整背景只映射到屏幕上部，唯一黑区位于底部；没有顶部黑区；
- Desktop Editor Play Game View：`Temp/battle-background-desktop-camera-only-pass-final-20260824-1.png`。完整背景覆盖全屏，没有底部黑区；
- Scene View：`Temp/battle-background-mobile-bg-world-scene-final-20260824.png`。截图通过 `view_target=Bg (2)` 对世界对象取景，显示的是 `Bg (2)` 自身的 world-space Sprite，不是 clip-space overlay；
- 同一 Play witness 中 `Bg (2)` 保持 position=`(-1.79,-4.8,0)`、scale=`(1,1,1)`、SpriteRenderer `enabled=true`、`isVisible=true`、`forceRenderingOff=false`；因此 source 在目标 Camera render 结束后已恢复，Scene View 没有被全局隐藏；
- 测试完成后已停止 Play，并把 Camera Inspector 的 preview 恢复为 `Automatic`；没有保存 user-dirty `NTSD_Battle.unity`；
- 当前状态只能到 `RUNTIME_PENDING`：Editor 的 Scene/Game 构图合同已闭合，但 Android/iOS Player/真机仍由用户执行；本次 MCP 自动启动场景没有出现角色，因此“角色在该最终 pass 下的视觉比例”仍需用户现有有角色 Play 场景复核，不能写成已验证。

### User-directed Resource Restore / Presentation Simplification — 2026-08-24

用户已明确说明背景资源已还原，且 `BattleCameraSafeArea` 不再需要。此决定取代本 Record 先前的
desktop/mobile slice、target-camera-only URP presenter 和 bottom-gap 交付目标。

本次允许的最小收敛如下：

- `Bg (2).SpriteRenderer.sprite` 是唯一背景资源真相；不再由平台代码切换 Sprite、裁切 Texture 或生成背景 screen quad；
- `BattleCameraSafeArea` 不再写 `Camera.rect`、`Camera.ResetAspect()`、orthographic size、Camera Transform 或任何背景资源；由于用户场景已 dirty，先保留同名组件为无副作用兼容壳，避免删除脚本导致未保存 Scene 出现 Missing Script；
- `BattleBackgroundPlatformSelector` 同样停止写 `SpriteRenderer.sprite`，保留同名无副作用兼容壳；
- 移除新增的 URP background pass、presenter、background shader、background importer tool 和与已废止平台表现合同绑定的 focused tests；
- 不保存、重排、覆盖或手写 `NTSD_Battle.unity`；用户可在确认当前场景修改后自行移除两个兼容组件并保存；
- 不修改 Camera/Background Transform、战斗 tick、输入、碰撞、checksum、Stage、实体渲染或网络数据。

验收：Unity C# compile 为 0 error；`Bg (2)` 在 Scene 和 Game 中由原始 SpriteRenderer 显示；静态审计确认
`BattleRenderFeature` 没有 background pass、`BattleCameraSafeArea`/selector 没有 Camera 或 Sprite writer；Change Ledger
validator 与 scoped diff 通过。Android/iOS 特殊底部黑区已由用户选择撤销，不再作为本包验收项。

### World-Sprite Simplification Result — 2026-08-24 23:11–23:16

- `BattleCameraSafeArea` 已缩为无字段、无生命周期逻辑的 compatibility shell；不再写 Camera rect、aspect、orthographic size、Transform、背景或任何战斗状态；
- `BattleBackgroundPlatformSelector` 已缩为无字段、无写入 compatibility shell；`Bg (2).SpriteRenderer.sprite` 不再被平台逻辑覆盖；
- `BattleRenderFeature` 已移除新增的 `BeforeRenderingOpaques` background pass，恢复其既有 central entity render 职责；
- former screen presenter、platform asset mutation menu 和 platform presentation test 文件仅保留零行为 compatibility type，以避免当前已打开 Unity Editor 的增量 compiler 在旧 source list 中报 `CS2001`；它们不创建对象、没有菜单、不会改资源、不会绘制，也没有测试；`BattleBackgroundScreen.shader` 已移除；
- Unity `refresh_unity` 后 `error CS` 为 `0`，`BattleBackground` filtered error 为 `0`；
- Editor Play witness：`Temp/battle-background-restored-world-sprite-game-20260824.png` 与 `Temp/battle-background-restored-world-sprite-scene-20260824.png`。两者都来自 `Bg (2)` 的原始 SpriteRenderer，Play 中其 `enabled=true`、`isVisible=true`、`forceRenderingOff=false`、position=`(-1.79,-4.8,0)`、scale=`(1,1,1)`；
- 当前已还原资源的 PPU=100，背景 world size=`20.48 × 11.52`；当前 Scene 基线 Camera orthographic size=`8.468438`（经 `git show HEAD:NTSD_Battle.unity` 确认该值已在仓库基线中存在）。Game witness 因此出现四周黑边；这是当前资源 world size 与已有 Camera 构图的结果，不是本次兼容壳写入造成的。本 Change 没有自行改变该 Camera 参数；
- `Tools/Validate-ChangeLedger.ps1` 已通过（101 records / 128 governed code files）。`NTSD_Battle.unity` 仍为原有 dirty 状态，Play 已停止且未保存。

本 Record 不可标为 `VERIFIED`：单 Sprite 路径本身已运行，但用户尚未确认是否需要单独调整“还原后的资源尺寸 / 现有 Camera 构图”这一视觉决定。该决定不能通过重新引入相机自动控制、跟随、平台裁切或移动背景来猜测解决。

### User Correction — Keep Platform Presentation Without BattleCameraSafeArea

用户明确纠正：`BattleCameraSafeArea` 不需要，**但** Windows 全覆盖、Android/iOS 底部可调黑区仍是必须需求。
此前“纯 world Sprite、无平台表现”的收敛被判为错误方向，不得交付。

新的最小合同：

- 只使用 `Bg (2)` 当前的一张完整 Sprite；不切图、不创建 Desktop/Mobile Sprite、不改 importer；
- 新的 `BattleBackgroundPlatformPresentation` 挂在 `Bg (2)`，公开 `Target Camera`、`Editor Preview` 和 `Android Bottom Gap`；
- Windows/desktop：在目标 Game Camera 上以完整 source 覆盖全部屏幕；Android/iOS：同一完整 source 映射到顶部 `1-gap`，底部输出黑色；
- 当前的 `BattleCameraSafeArea` 和旧 selector 不得重新获得任何行为，且从场景中移除；
- SceneView 永远绘制 `Bg (2).SpriteRenderer`；目标 Game Camera 仅在自己的 SRP render window 临时隐藏该 source 并由 target-camera-only URP pass 画背景，结束即恢复；
- 不写 Camera rect/aspect/size/Transform，不写 background Transform，不写 simulation/input/checksum/Stage/network；
- 因当前 scene dirty，不保存场景；新组件与旧组件的 Scene 变更仅在当前 Editor 内测试，由用户决定何时保存。

验收：Desktop Game 全覆盖、Mobile Game 底部可调黑区、SceneView真实`Bg (2)`、有角色时角色不变形；C# compile 0 error、static无Camera/Transform writer、Ledger validator通过。

### User-approved Aspect-correct Mobile Framing — 2026-08-25 (pre-code)

用户已确认上一轮“Android 顶部区域使用底部锚定等比裁切”的说明可以执行。该确认取代上文
“Mobile 映射同一完整 source 到顶部 `1-gap`”的压缩绘制含义；此前 `Temp/battle-background-bg-owned-mobile-20260824.png`
虽满足黑区位置，但把 16:9 source 压进约 2:1 顶部区域，属于已确认的 **FAILED** 视觉结果，不可作为交付证据。

当前实施合同如下：

- Windows / desktop：背景继续覆盖整个目标 Game Camera；在输出 aspect 与 source aspect 不同时采用等比 `cover`，绝不非等比拉伸；
- Android / iOS：底部黑区仍由 `Android 底部黑区比例` 控制；顶部可见区域用同一 source 的**底部锚定等比 cover** 填充；当顶部区域比 source 更宽时，只裁掉 source 顶部的天空，保留地面和底部城墙；
- 若极窄输出比例使 source 必须横向裁切，裁切仅在左右对称发生，仍不得拉伸或改写 source 像素；
- 背景的 source UV crop 仅是目标 Game Camera 的 transient raster presentation。`Bg (2)` 的 Sprite、Transform、Scene View、Camera rect/aspect/size/position、`NTSDRenderSpace`、实体 Mesh、战斗 tick、碰撞、输入、checksum、Stage 和网络数据均不得写入；
- 不因为这个视觉 adapter 改变逻辑坐标或跨平台联机状态。实际有角色的 Editor Play / Android Player 仍需复核角色不变形且不会进入底部黑区。

本次代码只允许：根据 source Sprite aspect、目标 Camera 输出 aspect 与底部 gap 计算一个 bottom-anchored source UV crop，传入既有 target-camera-only background pass；并增加 focused 纯计算测试。不得改 Background/Camera Transform 或引入新的 Scene 持久对象。

### First Difference — Scene World Map Versus Game Background Overlay (2026-08-25)

等比裁切实现和 focused test 已运行，但用户在真实 Scene/Game 对照中发现新的编辑工作流首差：

- Scene View 显示 `Bg (2).SpriteRenderer` 的真实 world rect 与真实 stage/green walkable bounds；
- Game View 在 `ScenesCamera` 渲染期间临时隐藏该 Sprite，并由 `BattleBackgroundScreenPresenter` 的 clip-space quad 画出同源纹理；
- 因而 Game 背景覆盖/裁切与 Scene 内的 Background、BoundaryWall 和可行走区域不在同一个坐标系，不能可靠地用 Scene 中的背景构图调整可行走区域。

这不是 Sprite pivot、DAT、碰撞或 character transform 差异，而是本 Change 为避免 Camera/Transform 写入而引入的
**background-only screen-space composition** 的结构性结果。用户的新编辑需求要求 Scene 与 Game 共享同一张 world map，
因此当前 screen-pass 架构相对于该需求为 `FAILED`，即使它已满足黑区和无拉伸的局部截图。

已观察证据：

- Unity script refresh 后 focused EditMode job `6a195ff6c776453682fc0ac3003a42e9` 为 `15/15 PASS`；
- Mobile Game screenshot：`Assets/Screenshots/Temp_battle-background-mobile-aspect-correct-bottom-anchored-20260825.png`，等比、底部黑区、角色未见非等比拉伸；
- Desktop Game screenshot：`Assets/Screenshots/Temp_battle-background-desktop-aspect-correct-20260825.png`，完整覆盖；
- Mobile Scene screenshot：`Assets/Screenshots/battle-background-mobile-aspect-correct-scene-20260825.png`，真实 `Bg (2)`、角色与 green stage bounds 可见；该图与 Game 背景构图不一致，正是首差。

**停止条件触发：** 若要满足“用 Scene 调整可行走区域时 Game 与地图同坐标”的新约束，必须废止当前 clip-space background copy，改为同一 world Sprite + 明确的 presentation camera/world-frame 合同；Android 黑区应成为 world render 之后的 local overlay 或同一 Camera 的固定 framing结果。该路线会改变本 Change 的架构边界，未经用户确认不得实施、不得再修改脚本或场景。

### User-approved World-aligned Replacement — 2026-08-25 (pre-code)

用户已明确确认采用“**同一世界背景 + 固定视觉相机取景 + Android 最终黑色覆盖层**”。因此当前 `BLOCKED` 状态解除，
本 Change 恢复为 `IN_PROGRESS`，且下列合同覆盖此前的 clip-space background copy：

1. `Bg (2).SpriteRenderer` 是 Scene 和 `ScenesCamera` Game 渲染中的唯一背景地图；不再在目标 Game Camera 临时隐藏它，也不再以 source texture 重画另一份全屏背景；
2. `BattleBackgroundPlatformPresentation` 可以只读 `Bg (2).bounds`，并仅写目标正交 Camera 的固定**视觉取景**：`orthographicSize` 与 `transform.position.x/y`。Windows 对齐 source 的 world frame；Mobile 在同一 world frame 基础上向下偏移一段等于 bottom-gap 的可见世界高度，使背景只裁源图顶部、保留下方地面/城墙；
3. Camera 使用自己的真实 output aspect、full viewport 和不变的 rotation/z；不得写 `Camera.rect`、`Camera.aspect`、follow、安全区、`NTSDRenderSpace` 或任何 battle/runtime 字段；
4. Android/iOS 的黑区由 world render 完成后、仅目标 Game Camera 执行的透明黑色 overlay 表现；它不采样、缩放、裁切或重绘 `Bg (2)`，不会在 SceneView 出现；Desktop gap 为零且不画 overlay；
5. 黑区、平台或 Camera frame 永不进入 `SimulationWorld`、input、checksum、Stage、DAT、碰撞、AI、实体 transform 真相或网络消息。相机 world position 只影响 Unity presentation frame；逻辑位置和碰撞仍保持 runtime 真相；
6. 旧 `BattleBackgroundScreenPresenter` / texture shader background pass 将被收回为无行为兼容壳或不再被调用。不得删除 dirty Scene 可能仍引用的脚本，且不得保存 `NTSD_Battle.unity`。

验收新增：同一帧 SceneView 的 `Bg (2)`、角色与 green walkable bounds 必须与 Game 中背景的相对位置一致；Windows 无黑区；Mobile 黑区只在底部且高度可调；角色、武器和阴影不发生非等比变形。需要真实场景截图和有角色 witness，不能用纯计算测试代替。

### User-directed Editor Live Camera Framing — 2026-08-25 (pre-code)

用户在检查当前同一 world `Bg (2)` 的 Scene/Game 画面后，明确要求增加一个**可见、可开关的 Editor-only 实时相机取景模式**：

- 它必须显示在 `BattleBackgroundPlatformPresentation` Inspector 中，且与现有“编辑器平台预览（Automatic / Desktop / Mobile）”独立；
- 开启时，在 Edit Mode 中监测 `Bg (2).SpriteRenderer.sprite`、其 world bounds、目标 Camera 输出 aspect 和平台预览值；其中任一发生改变时，实时重算目标正交 Camera 的 `orthographicSize` 与 `transform.position.x/y`；
- 关闭时，恢复该 Editor preview session 捕获的 Camera position / orthographic size；
- Player runtime 保持当前 world-frame 行为，不因为这个 Editor toggle 改变 platform mapping、输入、Stage、battle runtime、checksum 或联机消息；
- 不改 PPU、Texture 像素、Sprite pivot、`Bg (2)` Transform、Scene YAML 或可行走区域数据；不保存用户 dirty 的 `NTSD_Battle.unity`。

#### 已观察的“同一张图片看起来不同”解释

这不是资源不一致的证据。当前 source Sprite 是同一个 `2048 × 1152`、PPU=100 的 world Sprite（world bounds 为 `20.48 × 11.52`）：

1. Scene View 的大图由 Unity 的**自由编辑器 Scene camera**显示；它的缩放和位置由用户在 Scene 面板中的导航控制，和 `ScenesCamera` 无关。
2. Scene 内的 Camera Preview 以及 Game View 都由实际的 `ScenesCamera` 显示；它们只会看到该 Camera 的 orthographic size、position 和 output aspect 所覆盖的世界矩形。
3. 因此，同一个 world Sprite 在 Scene View、Camera Preview 与 Game View 中可呈现为不同的屏幕尺寸、位置或可见边缘；这只说明观察相机不同，不说明 PNG 被缩放、背景 Transform 被改写或战斗坐标改变。

#### 本轮最小实现与验收

- 只修改 `BattleBackgroundPlatformPresentation.cs` 与其 focused Editor tests；使用现有 `CAMERA-PLATFORM-BACKGROUND-001` Change ID。
- 新增显式 Editor live-framing Inspector 字段及纯函数/focused test，确保 Edit Mode enable、disable、Player mode 三种资格边界明确。
- `ExecuteAlways` 的 Edit Mode 更新只在该开关开启时 refresh；它不会写逻辑实体或 Stage 数据。
- 验收顺序：Unity compile 0 error；background focused EditMode tests；编辑器截图/Inspector witness（修改 Sprite 后 Camera frame 更新）；Play/Console 与 Change Ledger validator。尚未取得前不得称为 runtime VERIFIED。

#### 回滚

将 Inspector 中的 Editor live-framing 关闭，即恢复本次 Editor session 的已捕获 Camera frame；若功能本身被判错误，只移除新 toggle / update hook 并保留此前已批准的 world-background 与 final overlay 路径。不得通过恢复 screen-space background copy、修改 PPU、缩放 Transform 或写 Stage 来回滚。

### Editor Live Camera Framing Code Written — 2026-08-25

实际代码已写入，下列内容属于同一 `CAMERA-PLATFORM-BACKGROUND-001` 行为改动：

- `BattleBackgroundPlatformPresentation.editorLiveCameraFrame`：新增默认开启的序列化 Inspector 开关“编辑器实时相机取景”；
- `EditorLiveCameraFrame` property：切换时立即 refresh，关闭时沿既有 `RestoreCapturedCameraFrame()` 路径恢复当前 preview session 开始前的 Camera frame；
- `Update()`：仅在 `UNITY_EDITOR && !Application.isPlaying && editorLiveCameraFrame` 时调用现有 `RefreshPresentation()`，因此 SpriteRenderer 的 Sprite/bounds、Camera output aspect 或 Inspector preview 变化不再只依赖本组件自身的 `OnValidate()`；
- `ShouldApplyWorldCameraFrame(...)`：明确 Edit Mode disabled、Edit Mode enabled 和 Player runtime 的资格边界。Player 不会因 editor toggle=false 而停用既有运行时 world-frame；
- `RefreshPresentation()`：在 source Sprite 缺失时 fail closed，并在 Edit Mode toggle 关闭时恢复 captured Camera frame；没有写 PPU、Texture、Sprite pivot、Bg Transform、Stage或simulation；
- `BattleBackgroundPlatformPresentationEditorTests`：新增 eligibility 三分支及 replacement Sprite bounds→new Camera frame 的纯计算覆盖。

当前代码状态：`CODE_WRITTEN`。尚未取得本轮 Unity compile、focused test、实际 Inspector Sprite-change、Play Mode / Console 或 Ledger validator 证据；不得把此段写成已验收或把游戏表现差异归因为已修复。

### Focused Test First Feedback — 2026-08-25

- UnityMCP 已对当前打开的 Unity 发起 scripts force refresh / compile；domain reload 期间连接关闭属于预期重连，随后 `read_console(error)` 仅返回 MCP client-handler exit 日志，没有项目 C# error。
- focused job `dbaacc798f834bb8b78ca5b5735e8062` 运行到本类第 19 项后失败，唯一失败为新增 `WorldCameraFrame_ReplacementSpriteBoundsProduceANewCameraFrame`。
- 原因已闭合为**测试期望笔误**：replacement bounds center=`(-3,7)`、size=`(32,18)`、16:9 时完整 frame 的 `yMin=-2`；测试错误期望 `16`。现有 `ResolveWorldCameraFrame` 返回 `-2` 符合该几何合同，production / Camera runtime 无异常证据。
- 后续允许的最小改动仅是将该 test 的 expected y 从 `16` 修为 `-2`，然后重跑同一 focused class；不修改 production component、PPU、Camera、Transform、Scene 或 battle 数据。

### Ownership First Difference — 2026-08-25 (pre-code correction)

当前打开的 `NTSD_Battle` Scene 只读 hierarchy / YAML 核对发现：

- `Bg (2)`（runtime instance `97254`）含 `BattleBackgroundPlatformPresentation`，并显式引用 `ScenesCamera` 和自己的 `SpriteRenderer`；
- `XueYuan`（runtime instance `97548`）也含同一组件；其序列化 `targetCamera` / `sourceRenderer` 竟同样引用 `ScenesCamera` 和 **`Bg (2)` 的 SpriteRenderer**，而不是 XueYuan 自己的 Renderer；
- 由于该组件为 `[ExecuteAlways]`，两个有效实例会竞争更新同一 Camera 与 static overlay presenter，这能造成 Editor 取景和预览顺序不稳定。

这不授权移除、重排或保存用户 dirty Scene。用户的明确意图是组件挂在 `Bg (2)`，因此允许的最小修复是：

- `BattleBackgroundPlatformPresentation` 只在 `sourceRenderer.gameObject == gameObject` 时成为合法 owner；外部对象引用 `Bg (2)` Renderer 的 duplicate instance fail closed，释放自身 session 资源，不写 Camera；
- 新增 focused test，覆盖 own-source=true / foreign-source=false；
- 不用对象名称、tag、layer 或角色专项例外判断，不删除 `XueYuan` 上的组件，不改 PPU、Transform、Scene、Stage或battle数据。

这条 guard 是 Editor live-framing 正确性的必要收紧：它确保一个 world background 只有一个相机表现 writer。

### Ownership Guard Code Written — 2026-08-25

- `BattleBackgroundPlatformPresentation.IsValidSourceRendererOwner(...)` 已加入：只接受组件所在 `GameObject` 与 `sourceRenderer.gameObject` 相同的配置；
- `RefreshPresentation()` 现在在非 owner、source missing 或 target Camera missing 时统一 fail closed，并调用已有 `ReleasePresentation()`；不会写 Camera，也不会产生 overlay；
- 这让 `XueYuan` 当前的 foreign duplicate 在不删 Scene component 的前提下停止竞争 `ScenesCamera`；真正的 `Bg (2)` 仍满足 own-source 合同；
- focused test `SourceOwner_RejectsAForeignDuplicatePresentationComponent` 已加入，使用临时 in-memory GameObject / SpriteRenderer 并 finally 销毁，无 scene/asset side effect。

当前仍为 `CODE_WRITTEN`：owner guard 与此前 editor live framing 尚待本轮 Unity compile、focused tests、实际 Editor hierarchy/Inspector witness、Console和Ledger复核。

### Isolated Editor Update Witness — 2026-08-25 (pre-code test addition)

为验证“编辑时替换 `Bg` Sprite 后相机会即时跟随、且关闭开关后恢复”而不操作用户 dirty Scene，允许只在
`BattleBackgroundPlatformPresentationEditorTests.cs` 增加一个 isolated in-memory fixture：

- 临时创建 Camera、background GameObject、SpriteRenderer、presentation component 与两张不同 world-size Sprite；
- 用序列化 private-reference 配置把 component 指向该临时 Camera / Renderer，手动调用实际 private `Update()` 编辑器路径；
- 验证 Sprite A→B 后 Camera frame 更改为新的 source bounds，且 `EditorLiveCameraFrame=false` 后恢复 fixture 开始时捕获的 Camera frame；
- finally 销毁临时 GameObject、Sprite、Texture；不得加载/保存/修改 `NTSD_Battle.unity`、`Bg (2)`、Stage、PPU或资源；
- 此 test 是 Editor behavior witness，不是 battle 或跨平台 Player 验收。

### Isolated Editor Update Witness Code Written — 2026-08-25

`EditorLiveCameraFrame_EditorUpdateTracksSpriteReplacementAndRestoresBaseline` 已写入 focused test：

- 使用临时 Camera / background / two Sprite fixture 配置真实 component private references；
- 通过反射调用实际 private Editor `Update()`，而不是仅测试 `ResolveWorldCameraFrame` 纯函数；
- 断言 A→B replacement 后目标 Camera frame 与新的 Renderer bounds 对应，再断言关闭 `EditorLiveCameraFrame` 恢复预览开始前 position / orthographicSize；
- fixture finally 释放所有临时 Unity objects；没有访问 `NTSD_Battle`、Stage或生产资源。

当前仍为 `CODE_WRITTEN`，需重编译并运行 expanded focused class。此前 `19/19` 只覆盖 guard 写入前的版本，不能拿来证明此新 test 已通过。

### Compile and Focused Editor Evidence — 2026-08-25

- 当前打开 Unity Editor 的 scripts force refresh / compile 完成；domain reload 造成的 MCP disconnect/reconnect 已恢复，随后 Console error 查询只含 MCP `Client handler exited` 连接日志，没有项目 C# error；
- focused job `78c18d4f2b3246f99ab4b024dfc1e3f6`：`21/21 PASS`、`0 failed`、`0 skipped`，耗时 `0.724s`；
- 其中实际通过：`EditorLiveCameraFrame_EditorUpdateTracksSpriteReplacementAndRestoresBaseline`、`SourceOwner_RejectsAForeignDuplicatePresentationComponent`、Edit enable/disable eligibility、Player eligibility、source bounds camera frame与Mobile/Desktop gap矩阵；
- 当前 Editor hierarchy 仍保留用户 scene 的 `XueYuan` duplicate component，但 `ScenesCamera.position=(-1.79,-4.80000067,-10)` 与 `Bg (2).position=(-1.79,-4.8,0)` 一致；guard 会让该 foreign duplicate 不再成为 writer。没有删除组件、保存 Scene、改 PPU、Sprite、Transform、BoundaryWall或任何 battle state；
- `Tools/Validate-ChangeLedger.ps1`：`PASS`（101 records、131 governed diff files）。

当前状态升级为 `FOCUSED_TEST_PASS / EDITOR-RUNTIME-WITNESS / RUNTIME_PENDING`：隔离 Editor Sprite replacement 与相机恢复合同已运行通过；用户自己的 `Bg (2)` Inspector 实际换图观察、Desktop/Mobile有角色 Play Mode和真实 Player 平台画面尚未执行，不能写成完整 presentation 已验收。
