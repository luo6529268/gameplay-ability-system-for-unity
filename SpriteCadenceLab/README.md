# Sprite Cadence Lab

> **CURRENT_AUTHORITY_NOTICE（2026-09-02）：** 本文出现的 30 Hz/`1/30`、400-slot/Authority400、`ntsd_release` 或旧 `game_tick(...)` 只描述历史证据或当前 Unity 实现，不代表 NTSD 2.8-Logan 的现行权威合同。任何恢复先读 `docs/ai/CURRENT-AUTHORITY.md`；当前已观察权威基线为正常 33 ms、F5 3 ms、1000 物理 slot、双 RNG 流和新的 playable tick 入口，Unity 是否及如何迁移仍为 `REBASELINE_REQUIRED`。

这是一个独立浏览器实验，不是 Unity 项目，也不会由 Unity AssetDatabase 导入。

## 打开方式

双击 `Start-SpriteCadenceLab.cmd`。它会使用已安装的 Node.js 启动只服务本目录的本机页面，并自动在浏览器中打开默认的 `http://127.0.0.1:41731/`。若该端口已被占用，启动器会自动换到一个空闲本机端口，并将实际地址打印在命令行窗口中。

保持弹出的命令行窗口开启；测试结束后在该窗口按 `Ctrl+C` 停止。`index.html` 也可以直接打开，但优先使用启动脚本：这与 `Tools/DatSkillFlowWeb` 的本机服务方式一致，可避免浏览器对 `file:///` 下资源加载的差异。

## 观察目标

默认 fixture 对应现有 `Assets/NTSD/Config/FrameConfig/naruto_clone.dat` 的 Naruto 行走帧：`F5 → F6 → F7 → F8`。四帧均为 `wait: 3`。

`DatSkillFlowWeb/src/client/main.ts` 的播放器每 33 ms 推进一次逻辑 tick；`DatSkillFlowWeb/src/sim/frame-tick.ts` 先递增 `attacking`，再仅在 `attacking > wait` 时切换 frame。因此 `wait: 3` 不是保持 3 tick，而是保持 **4 tick**，即每张姿势约 `4 × 33 = 132 ms`，完整四姿势循环约 `528 ms`。

| 显示刷新率 | 正确显示采样 |
|---|---|
| 30 Hz | `A A A A  B B B B  C C C C  D D D D` |
| 60 Hz | 每张姿势重复 8 次 |
| 120 Hz | 每张姿势重复 16 次 |

选择“错误对照：按显示帧推进图片”后，会看到图片随显示刷新率推进，从而绕过 DAT 的 `wait`。对于此 `wait: 3` fixture，错误循环会分别快 4、8、16 倍。

页面顶部同时显示 30 Hz、60 Hz 与 120 Hz 三个预览区域，并共享同一个逻辑 tick 时钟。正确模式下三者在任何时刻都应显示同一姿势；不同之处只在于该姿势分别被重复显示 1、2、4 次。页面下方的“明细”按钮只切换采样轨迹的展开视图，不会改变三个顶部预览区的独立显示频率。

## 与未来 Unity 表现层的关系

- `frameId`、方向、source rect 与每个 DAT 帧持续的 logic tick 数必须来自战斗逻辑快照；
- 60/120 Hz 表现层只重复采样当前源图；
- 角色位置、持有武器挂点、阴影、镜头和纯视觉效果可以插值；
- Sprite 姿势本身不能由 `Update`、`LateUpdate` 或渲染刷新率推进；
- 本实验依据 `wait: 3` 的四格行走 fixture；真实接入时仍须读取当前实际 DAT frame 的 `frameId`、`pic`、`wait`、`next` 和 source rect，不能把本例的 4 tick 写死到 Unity。

## 文件

- `index.html`：交互实验；
- `Naruto_4frames_source.png`：用户提供的四格示例图片副本；
- `server.mjs` 与 `Start-SpriteCadenceLab.cmd`：只服务本目录的本机启动器；
- 本目录不包含 Unity 的 `Assets`、`ProjectSettings`、`Packages`、`Library`、`.unity` 或 `.meta` 文件。
