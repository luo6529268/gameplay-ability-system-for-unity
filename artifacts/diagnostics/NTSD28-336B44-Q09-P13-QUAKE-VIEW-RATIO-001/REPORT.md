# Q09/P-13 固定完整背景的地震位移比例

状态：`VERIFIED_SCOPED_PROJECT_BACKGROUND_VIEW_RATIO`。本报告只关闭项目自有背景在固定完整视口中的地震绘制位移比例；Q09、Q12 和总目标仍开放。

## 权威与首差

当前 336B44 playable 的 `GameSession28::step()` 发布整数背景地震偏移；`source/ntsd28_playable/src/d3d11_renderer.cpp` 将该偏移加到背景层绘制坐标。正式 1333×730 视口中韩自然抓取的 (+2,0) 占画面宽度 `2/1333`。项目按用户 D-024 决定保留自有 2048×1152 完整背景与固定相机。修改前 Unity 背景 shader 直接使用源整数 2，只占 `2/2048`，相对距离是正式画面的 65.09%；既有 1024×576 项目背景捕获右移 1 输出像素。这个首差在背景表现出口，不在地震规则状态或战斗逻辑 tick。

## 修改范围

- `Assets/NTSD/Scripts/App/BattleBackgroundPlatformPresentation.cs` 在最终 Bg shader 出口读取运行中的 `SimulationWorld.SpatialProjection`，以统一 `SourceDeltaToViewX/Z` 换算水平与屏幕高度位移；后者只用于屏幕 Y 的比例，保留向下为正到 Unity 向上为正的符号转换。原 identity 诊断路径不变。
- `Assets/NTSD/Scripts/Test/Editor/NTSD28Q09EarthquakeBackgroundEditorTests.cs` 增加聚焦断言，核 2048×1152 下的 shader 向量、源帧不变及零偏移材质恢复。
- `Assets/NTSD/Scripts/Test/Editor/NTSD28Q09HanEarthquakeBattlePlayProbeEditor.cs` 仅增加独立 opt-in 请求路径，复用既有 X520/Z400 相位配对保护，不更改旧请求或捕获逻辑。

没有修改 DAT、正式原版背景、项目地图/相机、Scene、Prefab、ProjectSettings、非战斗代码或地震规则状态。

## 验证

1. 生成 Editor 工程两次构建均 0 error；修改两脚本后 301 warning，追加测试探针后 270 warning。原项目 Unity Editor 的具名 Q09 背景 EditMode 两项 `2/2 PASS`，含新增投影测试与旧 identity GPU 测试，job `f0f478bbae75497f9bd52f869ed7f71d`。
2. 原 Unity Editor、原 `NTSD_Battle.unity`，韩/李 action0、源 X500/520/Z400、正式输入相位0，完成相对 50 tick。独立结果 [`q09-p13-quake-ratio-20261003-01.json`](../NTSD28-Q09-P13-HAN-NATURAL-BATTLE-PLAY-001/q09-p13-quake-ratio-20261003-01.json) 为 `PASS_SCOPED_PLAY`：韩相对 tick4/10 进入 action145/149，tick16 发布 (+2,0)，tick21 归零；与当前正式根 trace 的输入相位、韩动作/状态/源X/抓取目标及李动作/源X/抓取来源等八字段逐 tick `400/400` 相同。比较只覆盖这些选定源规则字段，不是全 World 同态。
3. 新 1024×576 三图的天空与地面内域共 315,580 像素：active 图是 baseline 精确右移 **2 输出像素**，通道误差0；reset 图在同域与 baseline 完全相同。连续期望输出位移 `2×2048/1333/2=1.536384`，像素取样量化后为2；shader 浮点向量另由聚焦测试精确核验。旧版项目背景同域捕获为右移1像素。证据为同目录外的 [`baseline`](../NTSD28-Q09-P13-HAN-NATURAL-BATTLE-PLAY-001/q09-p13-quake-ratio-20261003-01-baseline.png)、[`active`](../NTSD28-Q09-P13-HAN-NATURAL-BATTLE-PLAY-001/q09-p13-quake-ratio-20261003-01-active.png)、[`reset`](../NTSD28-Q09-P13-HAN-NATURAL-BATTLE-PLAY-001/q09-p13-quake-ratio-20261003-01-reset.png)。
4. Map/相机位置和尺寸未改变，捕获相机及材质已恢复；有序关闭完成，pool 借用0。Editor 已回原 `NTSD_Menu.unity`，非 Play、Scene clean。Menu、Battle、GameConfig 和 ProjectBattleModeConfig 四份保护文件 [`before`](protected-before.json)/[`after`](protected-after.json) SHA-256 全相同。

与 2026-10-02 旧 phase-pair Unity 结果逐字段复核时，50 tick 中 41 tick 的三项物理视图坐标 `hanX/leeX/leeZ` 有差异，原因是两次运行间另一独立 D-024 抓取投影修复已落地；源规则字段、动作、关系和地震发布字段未变。本报告没有把这三项物理字段计入上面的 400/400，也不将这项既有变更归于本轮背景出口。

正式版背景像素与正式 EXE GUI 并未与项目自有背景逐像素比较；正式背景会把源 Z400 钳到542，项目地图保留Z400。其它 Q09 表现、动态遮挡、音频、完整自然物理输入以及 Q12 集成验收仍需各自证据。
