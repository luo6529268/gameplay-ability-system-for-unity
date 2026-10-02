# 336B44 Q09：插值计时起点只读审计（2026-10-02）

状态：`SOURCE_STATIC_FIRST_DIFFERENCE_CANDIDATE / RUNTIME_PENDING`。本报告只核对当前正式 playable 源码与 Unity 生产调用链，未改运行时代码，也未取得同一自然画面的时间戳配对或正式 EXE 像素对照。

## 正式版与 Unity 的已观察调用链

- 正式版 `source/ntsd28_playable/src/main.cpp`：成功逻辑步后 `app.session.snapshot(...)`，相邻 tick 时将旧 current 移到 previous，在赋值新 current 后立即用 `clock::now()` 设置 `current_battle_snapshot_started`（约 2612–2630 行）。下一次 render 在 `render_fps > 30` 且存在 previous 时，以 `(clock::now() - current_battle_snapshot_started) / active_tick_seconds` 形成 alpha（约 2724–2743 行）；默认 render_fps 为 120（约 117 行）。
- Unity `SimulationStageRenderModule.RenderDispatchAll`：`BeginFrame` 发布当前 `BattlePresentationFrame`，随后调用 `BattleCentralRenderSystem.QueueLatestPublishedFrame`（约 383–430 行）。该方法记录 world/frame/tick 并递增 `pendingPublicationVersion`，未记录发布时间（`BattleCentralRenderSystem.cs` 约 290–318 行）。
- Unity 首次 `MaterializeLatestPublishedFrame` 调用 `ResolveDisplayAlpha`；发现发布版本变化时才把 `displayClockStartedAt` 设为**当前读取时间**（`BattleCentralRenderSystem.cs` 约 378–395、1056–1086 行）。其后每次读取用此时间计算 alpha。原 `SimulationTickDriver.LateUpdate` 在消费发布后呈现（约 491–509 行）。
- 两侧的相邻 tick、实体身份、关系、连续性阈值和 source-space `lround` 位移规则在已读 `presentation_interpolation.cpp`、`BattlePresentationMotionSampler.cs` 中对应；Unity 固定完整背景相机及 D-024 距离映射是既定例外，不能把 native 相机移动直接搬入逻辑。

## 差异候选与验证边界

若发布与首次画面读取之间有非零延迟，正式版首画面 alpha 已包含该延迟；Unity 首次读取时 alpha 从 0 开始。因此 Unity 可能让移动精灵、阴影或附着画面延迟一段 present 时间，但延迟量尚未测量，不能据静态链认定玩家实际可见首差或定义修复数值。若两个 tick 在首次读取前连续发布，另需先核 previous/current 是否只保留最后相邻的一对。

下一出口：在**原 Battle Scene** 的自然连续 tick 中，只读记录发布时刻、首次 materialize 时刻、tick/版本/alpha 与实际 body/shadow 像素；以当前 336B44 正式 EXE 的同输入、相机例外校正后的画面或确定性 sample 结果核对。若差异成立，再建立独立 Task/Change Record，在一个共用发布时间入口修正，保留旧 `ControlledDisplayClock` 诊断探针对 `displayClockStartedAt` 的反射契约或同步更新其测试；按聚焦、原 Editor 编译、Play、checksum/Scene 未变验收。不得借此修改战斗逻辑位置、DAT 或背景相机。
