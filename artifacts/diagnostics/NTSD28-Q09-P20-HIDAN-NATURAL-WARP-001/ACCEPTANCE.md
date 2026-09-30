# Q09/P-20 飞段自然 frame430 正式配对 WARP 限定验收

状态：`VERIFIED_SCOPED_FORMAL_PAIRED_GPU`。本包只证明正式源码对应的自然 frame430 存在可见本体像素；不关闭 P-20、Q09、BATCH-05 或总目标。Q07/D-024 碰撞域仍独立未决。

正式根 `NTSD2.8-Logan.exe` SHA-256 为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。正式 `source/README_SOURCE.md` 声明源码与该发行 EXE 对应。诊断仅新建 `Tools/NTSD28Q09Diagnostics/hidan_natural_frame430_warp_probe.cpp`，SHA-256 `8D242B7D0FD7EA69FB33A6749605B33D1376025DA2A8D5189838CCF6270ED89E`；复用前次正式 playable/core/D3D11 配对构建闭包的编译参数，仅替换诊断 main 和输出路径。[编译参数](compile-argv.txt)、[编译退出码](compile-exit.txt)为 0，[日志](compile.log)为空，诊断 EXE SHA-256 为 `9E873B11AEC3A34710AF22051B489859115B2961CAD97EF63CE148F638BF3205`。

完整正式 Session 从飞段 OID24/action0 使用普通输入：tick1–2 攻击、9–10 跳跃、13–14 防御+前+攻击。tick16 自然到 action430/pic119/MP150，不注入 action/MP/facing。快照中 slot0 恰有一条本体 sprite 命令，累计 sheet 范围 117–128，`hid6.png` 源矩形 `(722,0,360,289)`，屏幕位置 `(469,447)`；[诊断运行](run.log)退出码 0。复制同一快照，只从副本删除这条本体命令，两份均用 D3D11 WARP 以 1333×730 离屏渲染。

独立 Pillow [像素分析](pixel-analysis.json)：[body-on](body-on.png) 与 [body-off](body-off.png) 有 1,995 个变化像素，变化包围框 `[471,451,546,520)`，RGB 最大通道差为 241/254/246；输出画面 alpha 不变，因为背景本身不透明。两张 PNG 的 SHA-256 分别为 `6A61654EC854657A476907E5AF372310D8FE594FAB3D1894C81AF7F299333C65` 与 `DACFE031D9B7CA56AC59D92E2CB2C9FE331CE6DCEDA0A9331AEA54ED6C78A351`。由于输入是同一快照且唯一删去本体命令，这 1,995 个像素可归因于飞段该帧的本体绘制。

这不是正式根 EXE 自身窗口的 GPU 捕获，也不是 Unity 原 Battle Scene 与正式版同世界/同视口的像素等价证明。既有原 Unity 完整 Driver 的 30 tick 选定四字段 120/120 同、原 Battle 物理输入自然 frame430 和中央命令 pic119，仍须独立确认 Unity 实际 sheet 绑定与 Game View 像素；不以本包推断 P-20 其它缺图/隐藏/terminal 分支。未修改正式源码/EXE、Unity 生产或测试脚本、DAT、图片、Scene、相机、配置 Asset 或非战斗逻辑。诊断主程序在目标 PNG 已存在时拒绝覆盖；本包没有重跑同名输出。

治理检查：`Tools/Validate-ChangeLedger.ps1` 返回0并报告 PASSED（1006 Records、35 个当前 diff 代码文件有覆盖）；`git -c core.safecrlf=false diff --check` 返回0。四个保护文件的 SHA-256 与前次记录一致：Battle Scene `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`，Menu Scene `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`，`ProjectBattleModeConfig.asset` `B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`，`EditorBuildSettings.asset` `8D621A077642B5154BA305861CA57DFB549FF4FDE5BC58F3E284536A5982F01E`。本包未运行 Unity Editor/Play，因此不能据此新增 Unity 编译或运行时通过结论。
