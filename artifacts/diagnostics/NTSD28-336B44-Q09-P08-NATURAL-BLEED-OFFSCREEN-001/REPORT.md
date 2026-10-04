# Q09/P-08 当前 336B44 自然血点 playable 离屏画面

状态：`DUPLICATE_VERIFICATION_NO_STATE_CHANGE`。本目录是本版对应 playable 源码血点离屏图的**重复校验**；P-08、Q09、Q12 和总目标保持开放。开始运行前漏查了总表中的 [同版、同探针、同初态既有报告](../NTSD28-336B44-Q09-P08-PLAYABLE-BLEED-RECHECK-20261004/REPORT.md)，该报告已有相同逐 tick 与三像素/PNG 哈希结果，并另有原 Unity 受控 GPU 证据。下述输出是新运行原件，但**不产生新的阶段进度或关闭证书**，保留以说明重复原因，不再重跑此案例。

正式根 EXE 在本包重新 SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。复用既有 [诊断源码](../../../Tools/NTSD28Q09Diagnostics/ita_natural_bleed_warp_probe.cpp)，源码 SHA-256 `0C14C9104659A8852FAC8F73E650148F31B6A78721BE7864D63D656338F9404E`。仅将旧版 [编译参数](../NTSD28-Q09-P08-FORMAL-NATURAL-BLEED-WARP-001/compile-argv.txt)的输出路径改到本包新目录，实际使用当前正式 `source` 的 28 个 Core C++、`game_session.cpp`、`selection_flow.cpp`、`d3d11_renderer.cpp` 和该探针；32 个 `.cpp` 路径全部存在。[本次参数](compile-argv.json)、[身份](identity.json)、[编译出口](compile.exit.txt)留存，编译 exit0、stderr 0 字节。新二进制是诊断重建，不替代根正式 EXE 的行为裁决。

正式 `resources/runtime` 下，OID2 鸣人/slot0/X500 与 OID9 鼬/slot1/X540、Z650、对立组、seed2833、mode0；鸣人仅在前两完整 tick 提交普通攻击。没有在初始化后写目标 HP 或强制动作。完整 `GameSession28::step()` 逐 tick [原始日志](run.stdout.txt)显示第 8 tick 鼬 HP180→160，第 22 tick 鼬回站立 action0、HP160、快照有唯一血点。选中命令是 slot1，阈值166、红色 `0x00FF0000`、尺寸1×3、画面左上 `(461,595)`；[运行出口](run.exit.txt)为0、stderr 空。

同一第22 tick 逻辑快照分别保留/仅移除所选血点命令，D3D11 WARP 离屏生成 1333×730 [有血点](ita-mark-on.png)与[无血点](ita-mark-off.png)。独立 [像素分析](pixel-analysis.json)得 3/972090 个输出像素改变，恰在 `(461,595)`、`(461,596)`、`(461,597)`；有血点均为不透明红 `(255,0,0,255)`，去血点均为 `(106,118,121,255)`。两张新版 PNG 各自与旧 B1E13 同探针 PNG **逐字节相同**，SHA 分别 `9E602358A7E8BD781A928E1F0581207FF9AB77EB897214A028E79D4D761799B1` 与 `9554BC3C2BBD05A43EF4976ED5B6B0A56D2E3A6F4AF95DFCF23AB596E43C21F5`。本轮首次分析误用 RGBA `ImageChops.difference().getbbox()`，未变化的 alpha 差导致空包围框；修为 RGB 定界、RGBA 像素逐点比较后取得上述结果，错误尝试未写报告数据。

这证明**当前对应 playable 源码**在此自然普通攻击样本的血点命令和 GPU 像素贡献，且与旧限定图相同；没有运行根正式 EXE 的实际 Present，没有与 Unity 原 Battle Scene 同初态、同 tick、同视口配对，也没有覆盖其它受击、遮挡或镜头条件。旧 Unity 自然血点 Play 的角色年龄/输入相位不同，不能按时间平移充当本版同状态证书。原 Editor 当前持续 `is_compiling=true`，本包没有触碰 Editor/Play。

未修改 DAT、图片原件、正式源码、探针源码、Unity 生产/测试脚本、Scene、相机、背景、配置或非战斗逻辑；正式背景只被源离屏渲染读取，没有导入 Unity。核对时 Battle/Menu Scene、GameConfig、ProjectBattleModeConfig 的 SHA 分别仍为 `D88AD2111715AB2D970A85DDDAFFAAB206DFFD3BB54B9DF071AD25901D76CDF6`、`9EAAA0B4782974D74A017C367C9D5C77326C31D4281A2D820D1CBBA76986C1BA`、`0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`、`B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`，与本轮启动时已有状态相同。全工作区 `git diff --check` 因其他正在修改的 Menu/Battle Scene 序列化空白退出1；本包关联的已跟踪进度文档定向检查退出0，没有改那两个 Scene。
