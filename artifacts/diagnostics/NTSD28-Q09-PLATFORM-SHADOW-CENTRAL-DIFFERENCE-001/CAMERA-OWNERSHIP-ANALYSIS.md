# Q09 平台阴影受控原相机像素归属复核（2026-09-27）

本文件只读复核同一次原 Battle Scene Play 保存的四张 1920×1080 RGBA PNG；没有再次运行 Unity、正式 EXE 或修改探针。完整 Driver tick6 的正式 OID56 平台目标 Shadow 命令，在生产中央帧中为 14 条命令之一。探针从该帧复制出全 14 条和仅移除该目标 Shadow 后的 13 条命令，并各自离屏绘制；`post-tick` 是同次运行受控白底、仅中央层的原场景生产 `Camera.Render()` 输出，`baseline` 是绘制前的相机输出。

| 图片 | SHA-256 |
|---|---|
| `platform-shadow-central-diff-01-all-central.png` | `93D15F4E453C093D18A87CE77F68267D321C5217AEB38F6CF56817933766555C` |
| `platform-shadow-central-diff-01-without-target-shadow.png` | `93C184B095AD0363A70C78B9BBBD514BCB581926A9087917D3E9AC317A096896` |
| `platform-shadow-central-diff-01-post-tick.png` | `59C85D2F01C4583CEEF1DA5479077679733207DD3F5AD3D8798190BBECA4776C` |
| `platform-shadow-central-diff-01-baseline.png` | `727556FE2B56E44015A956C19FF2153AC6A268F904E400AC96C5745A60A4AF70` |

Pillow 读取 RGBA 后，先将两张临时 `CommandBuffer` 离屏图的行序上下翻转，再与 `Camera.Render()` 的 PNG 按左上角坐标比较。这一步对齐的是两条读回路径的行序；不改变图像颜色或位置。令 `mask = max_channel(abs(flipud(all) - flipud(without))) > 2`，独立分析得到：

- `mask` 共 **372 像素**，左上角半开包围框 `[169,542,221,551)`；与原探针报告的 372 像素中央层差分一致。
- 在这 372 像素上，`post-tick` 与翻转后的 `all` **RGBA 逐通道精确相同 372/372**，与翻转后的 `without` 精确相同 **0/372**。不翻转时，`post-tick` 与原始 `all` 在同一掩码上精确相同 **0/372**。
- `baseline` 在这 372 像素上全部为白色；`post-tick` 相对 `baseline` 全部改变。目标差分不是旧白底或仅临时绘制图中存在的像素。

因此，这个正式内容、完整 Driver tick 的**受控原场景生产相机中央层输出**，可将上述 372 个像素归属到目标 Shadow 命令。原始 JSON 的 `cameraPixelStatus=PIXEL_OWNERSHIP_UNPROVEN...` 和旧 `ACCEPTANCE.md` 的“相机归属未证”描述的是未处理读回行序时的直接投影/排除算法；本复核用同次全/去目标差分与原相机图像匹配，覆盖那项保守诊断。旧算法出现的框偏差不能据此定性为生产相机投影错误。

边界：相机曾临时设为白底并只显示中央战斗层；这不是带项目背景、UI 的自然 Game view，也不是正式 EXE 同视口 A/B、LegacyOnly 生产像素或所有平台阴影情形。Q09/P-02、BATCH-05 与总目标继续开放。原始图片、JSON 与旧诊断状态均保留，不重写历史结果。
