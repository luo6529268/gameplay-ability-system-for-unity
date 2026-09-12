# BATTLE-CENTRAL-FOOT-MARKER-ANIMATION-001 — 中央 Foot Marker 六帧循环动画

<!-- CHANGE-RECORD
id: BATTLE-CENTRAL-FOOT-MARKER-ANIMATION-001
status: COMPILE_PASS
code-path: Assets/NTSD/Scripts/App/GameConfig.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralEditorPreview.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleFootMarkerBatchBackend.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleRenderFeature.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCentralEditorPreviewEditor.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCentralEditorPreviewEditorTests.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCentralRuntimeFootMarkerEditorTests.cs
asset-path: Assets/NTSD/Config/GameConfig/GameConfig.asset
authority: USER-REQUEST-ANIMATE-FOOT-MARKER-FRAMES-01-TO-06-2026-09-13
evidence: SIX_SOURCE_FRAMES_INSPECTED_AFTER_USER_RESIZE; ALL_128_BY_48; COMMON_ALPHA128_BBOX_X0_TO_127_Y7_TO_40; POINT_FILTER; MIPMAP_OFF; VALIDATION_REPORT_80MS_CLOCKWISE_INFINITE; RUNTIME_BUILD_0_ERRORS; EDITOR_BUILD_0_ERRORS; STATIC_ANIMATION_CONTRACT_PASS; CHANGE_LEDGER_PASS_461_RECORDS_8_GOVERNED_FILES; UNITY_FOCUSED_AND_PLAY_PENDING_NO_PIPELINE_INSTANCE
-->

> 创建日期：2026-09-13  
> 当前状态：`COMPILE_PASS / STATIC_ANIMATION_CONTRACT_PASS / UNITY_FOCUSED_PENDING / PLAY_PENDING`

## 用户要求与已观察事实

用户要求中央渲染 Foot 改为播放
`Assets/NTSD/Sprite/UIPanels/BattleHud/Foot/frame_01.png` 至 `frame_06.png` 完整帧序列。

首次检查时六帧为 1254×1254；用户随后按建议重新导出。正式实施前已再次检查当前六张
PNG、meta 和用户提供的 validation report：

- 六帧现在均为 128×48 RGBA Sprite、Point Filter、mipmap off。
- 动画为顺时针、forward infinite，六帧各 80 ms。
- 六帧 alpha≥128 的有效 bbox 完全相同，均为 top-left `x=0..127, y=7..40`；可直接使用
  full-rect UV，不再需要 source rect 配置或 importer rect 修改。
- 当前所有本地 human Foot 共用一个 Mesh、一个 texture binding 和一个 draw；必须保留该合批。

## 实施合同

1. `GameConfig` 增加六帧数组和每帧秒数；现有单帧 Sprite 保留为空数组时的 fallback。
2. 生产配置按 frame_01→frame_06 顺序绑定，帧时长 0.08 秒，使用每张 Sprite 的完整 UV。
3. Foot batch 以参考帧和公共 source rect 一次构建 UV；执行 draw 时按
   `Time.unscaledTimeAsDouble` 选择同相位 texture。动画属于表现例外，不写回战斗状态，
   不改变 pause、输入、simulation tick 或 checksum。
4. 所有 Self Foot 同步相位，从而维持一个 Foot draw；不创建 Animator、GameObject、Material
   实例或逐帧容器。
5. Editor Preview 使用同一配置，并按 Editor time 切换纹理；动画帧变化只请求 repaint，
   不重建 actor geometry。
6. 不手改六张 PNG/meta。用户重新导出后不再需要 Sprite rect 修改；源资源保持原样。

## 验收标准

- 纯函数测试覆盖 6×80 ms 的边界、循环和负时间归一化。
- backend/selector 测试覆盖帧边界和 texture 选择。
- authoring 测试证明 GameConfig 数组和帧时长进入 runtime central settings。
- 原 Self 判定、stable ground anchor、角色比例、单 Mesh/submesh、lease 和 draw 顺序测试保持。
- runtime/editor 外部编译 0 error；Unity focused/Play 如不可用则明确保留 pending。
- Change Ledger validator 通过。

## 回滚方式

仅回退本 Record 声明的脚本和 `GameConfig.asset` 动画字段；保留上一 Change 已建立的
`GameConfig.FootMarkerSprite` 静态 fallback 和 Scene GameConfig 接线。不得删除、覆盖或修改用户
提供的 Foot 目录、PNG、Aseprite、validation report 或 meta。

## 实际修改

- `GameConfig` 新增有序 `FootMarkerAnimationFrames` 和
  `FootMarkerAnimationFrameDurationSeconds`；生产资产按 frame_01→frame_06 绑定，时长 0.08 秒。
- 静态 `FootMarkerSprite` fallback 改绑 frame_01，动画数组为空时仍能显示同套新美术。
- 新增无状态 `BattleFootMarkerAnimation` selector，处理参考帧、边界、循环、负时间和坏帧回退；
  不读取或写入 simulation 状态。
- `BattleCentralEditorPreview` 将 GameConfig 动画数组/时长送入 runtime central settings；
  Editor Preview 按 `EditorApplication.timeSinceStartup` 切 texture，并仅在帧变化时请求 repaint。
- `BattleRenderFeature` 在既有 Foot draw 前按 `Time.unscaledTimeAsDouble` 解析当前 texture；Mesh、
  stable ground anchor、角色比例、材质和一个 Foot draw 均保持。
- 生产六帧都是 full-rect 128×48，现有 backend 参考帧 UV 可直接复用于其他五张同尺寸 texture；
  未修改 PNG 或 meta。
- focused tests 新增 6×80 ms 帧边界/循环/负时间断言；authoring 测试新增数组、时长及 texture
  选择断言；生产资源测试改为验证六帧顺序、尺寸、Sprite/Point/no-mipmap。

## 实际验证

- 用户重导出后复核：六帧均为 128×48；alpha≥128 bbox 均为 `x=0..127, y=7..40`；
  六份 importer 均为 Sprite single、Point、mipmap off。
- `dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly`：exit 0，
  0 error，47 warnings。
- 最终 Editor 脚本修改后运行
  `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly`：exit 0，
  0 error，104 warnings。
- PowerShell 六帧静态动画合同：PASS；验证 6 个 GUID 顺序、0.08 秒、PNG 尺寸、importer
  配置以及 RenderFeature 使用 unscaled time 解析 texture。
- `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <repo>`：exit 0，PASS，461 Records，
  当前 8 个 governed code files 均被 Record 覆盖。
- Unity CLI 对本项目仍返回 `STATUS_NO_INSTANCES`；机器上存在多个 Unity 进程，未启动第二实例。
  因此新 focused tests、真实 Play 动画、Scene in-memory dirty 和 Console 状态均未验证；不能提升为
  `FOCUSED_TEST_PASS` 或 `VERIFIED`。
