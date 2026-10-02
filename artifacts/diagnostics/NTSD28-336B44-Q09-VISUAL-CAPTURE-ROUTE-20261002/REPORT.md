# Q09 自然画面取证入口审计（2026-10-02）

状态：`READ_ONLY_ROUTE_AUDIT / Q09_OPEN`。本报告只确认可用的取证接口，不是画面一致性或正式 EXE 像素证书。本轮未启动正式程序、未进入 Unity Play、未修改脚本、DAT、场景或资源。

## 身份与检查范围

- 根目录正式 `NTSD2.8-Logan.exe` 本轮重新计算 SHA-256：`336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。
- 检查其对应 playable source 的 `source/ntsd28_playable/src/main.cpp` 选项与 GUI 接口、`d3d11_renderer.cpp` 离屏 PNG 实现、`offscreen_gate_main.cpp` 独立入口，以及当前 Unity MCP `MCPForUnity/Editor/Tools/ManageScene.cs` 截图路径。

## 观察

1. 正式 playable `main.cpp` 提供 `--gui-acceptance-report`、`--gui-acceptance-ms`、`--smoke-ms`、`--render-fps` 等入口；已检查的解析选项没有直接将正式 GUI 帧写入 PNG 的参数。GUI acceptance 输出为状态报告，不能当像素图。
2. `D3D11Renderer28::save_offscreen_png` 可从离屏纹理读回并保存 PNG；独立 `offscreen_gate_main.cpp` 以固定 `1333×730` 初始化离屏渲染并调用它。此工具走对应源码和资源，但其产物是**辅助程序画面**，不能标为根目录正式 EXE 的实测像素。
3. 现有 Unity MCP `manage_scene` 的 `screenshot` 可在 Play 下以 `captureSource=game_view`、`includeImage=true` 走合成 Game View 捕获；指定相机则走单相机捕获，不能误称为完整 Game View。自然战斗画面应选择未指定相机的合成路径，并保存唯一版本化文件名。MCP 截图能力存在不等于当前原 Battle Scene 已实测。
4. 现有 Q09 同 Z 四图只证明当前原 Battle Scene 的 CentralOnly GPU 局部遮挡；它既非自然合成 Game View，也非正式 EXE 可比视口画面。

## 后续出口

- 先以原项目 Editor、当前 Battle Scene 的自然生产 tick 抓**合成** Game View，记录逻辑 tick、发布版本、Game View 像素尺寸、相机视口、Scene 哈希与退出状态；不动场景相机配置。
- 若用离屏辅助程序做源侧对照，须写明辅助程序、当前 336B44 对应源码/资源及相同初态，结果仅为源码渲染层证据。要关闭“正式 EXE 像素”出口，仍须取得可归因于根目录正式 EXE 的实际显示帧，并按用户保留完整背景、D-024 比例映射和非例外战斗画面定义可比区域。
- Q09 仍 `IN_PROGRESS`；本审计不提升任何 P 项状态。
