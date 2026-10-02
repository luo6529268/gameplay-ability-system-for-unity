# Q09 原 Battle Scene 合成 Game View 首张当前版截图

状态：`UNITY_DIRECT_BATTLE_COMPOSITE_CAPTURED / TICK_AND_FORMAL_PAIR_PENDING / Q09_OPEN`。截图是原项目 Unity Editor 的 Battle Scene 在 Play、直接战斗测试启动完成后取得的实际合成 Game View；它不是正式 336B44 EXE 的画面，也没有逐 tick 同初态配对。

## 取证过程与原件

- 当前原项目 Editor 为 Unity 2022.3.62f3、唯一主进程 PID 105896，进入 Play 前活动 Scene `Assets/NTSD/Scene/NTSD_Battle.unity`、`isDirty=false`，未编译/未更新资源。通过既有 Unity MCP `manage_editor` 进入 Play；Editor 日志见 `[BattleTestBootstrap] === Test bootstrap complete ===`，以及表现启用、`SimulationTickDriver resumed`。
- 随后通过既有 MCP `manage_scene` 请求 `action=screenshot, captureSource=game_view, includeImage=true, maxResolution=128`，**未指定 camera**，输出至本目录唯一文件名。该源码路径在 Play 中走 `ScreenshotUtility.CaptureComposited` / `ScreenCapture.CaptureScreenshotAsTexture`；响应的 `camera=ScenesCamera` 只是 `Camera.main` 显示标签，不表示请求了单相机捕获。128 仅是响应内联预览的最长边上限，磁盘 PNG 为原始 Game View 分辨率。
- [原始 PNG](q09-gameview-natural-336b44-20261002-01.png)：`1920×1080`、`2,339,047` 字节、SHA-256 `043A6ADF036B0779875BCBA2C541556348A1CAFC0014040E9DDE132AC27FE240`。目视可见项目全背景、两名鸣人、血条及地面阴影；这是当次 Unity 实际画面存在性的证据，不是正式版像素一致证书。
- MCP 停止 Play 后，Editor 为 `isPlaying=false/isChanging=false/idle`，活动 Battle Scene `isDirty=false`、仍为原 13 个根对象。Battle、Menu、GameConfig、ProjectBattleModeConfig 的磁盘 SHA-256 前后分别保持 `93448372834A1BEAF2C9ACD90E2EF17E2EA487E19F601B9815A907A974D7BF60`、`DD6A48A37FB8CEA9CD8A1F7738964A719E007A42F0FBB54FB48BBD0B723B9DC3`、`0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`、`B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`。这四项与 LoganRuntime 的本轮限定 Git 状态为空。

## 尚未证明

- 截图没有同时记录 `SimulationTickDriver.CurrentTickIndex`、发布版本、实体逻辑坐标、相机有效矩形及同 tick 正式源/根画面，所以不能用本图验证插值、遮挡优先级或正式 EXE 的像素。
- 直接 Battle Scene 的 `BattleTestBootstrap` 是原项目已有测试启动器；本轮未通过 Menu→Battle 正式入口，也未模拟物理按键。画面是自然运行时的合成输出，但本图单独不证明玩家输入链或整场规则。
- 后续以唯一结果路径和原项目现有诊断入口在**截图当帧**同步记录逻辑 tick、发布版本及视口，再按当前336B44正式可达场景定义源/Unity可比取景；正式 EXE 实际显示帧仍需独立证据。不得用独立 offscreen gate 图代替正式 EXE 实测。
