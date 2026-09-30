# Q09/P-21 香燐自然子体的配对 D3D11 alpha 消费

状态：`VERIFIED_SCOPED_PAIRED_RENDERER_ALPHA_CONSUMPTION`。只闭合本诊断的正式配对渲染器同快照本体像素归因；不关闭 P-21、Q09、BATCH-05 或总目标，也不是根正式 EXE 与 Unity 同场景同视口比较。

正式根 `NTSD2.8-Logan.exe` 本轮 SHA-256 为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。新增工具源 `Tools/NTSD28Q09Diagnostics/karin_alpha_warp_probe.cpp` SHA-256 为 `302155E88B3C0140F07191B3F2DD62171CBFF6C1F2096DEE72C33F6C66B2A153`。编译复用既有配对 playable `offscreen_gate` 的 core/playable/D3D11 参数与链接闭包，只换诊断 main 和输出路径；`compile-argv.txt`、空 `compile.log`、`compile-exit.txt=0` 留存，诊断 EXE SHA-256 为 `E7A650CA34C68D335C7AC364AE0A3F2F267B2149A29313A6AD01F6195ECB8847`。正式源码、EXE 和资源只读。

初始态沿用已验配对 Session：seed2833、mode0、background23，香燐 OID77/slot0/X500/Z650/action415/物理朝左，远处 OID11/slot1/X1200。无输入推进完整 Session 三次；tick3 自然生成唯一 owner0 的 OID314/slot50/action50/state9997。快照有唯一子体 sprite 命令，pic60，从正式 `vfs/c/kar/a/cha4.png` 取源矩形 `(0,0,79,79)`，正式快照本体左上 `(461,571)`、表现向右。诊断复制这一个快照，只在副本移除该命令，先后以配对 `D3D11Renderer28` WARP 1333×730 渲染两图；其间没有再推进 World tick 或修改 DAT。

两张 PNG 都是 1333×730。独立 Pillow 逐像素复算，变化 **1,433** 像素，左上原点 bbox `[475,576,534,649]`；按快照左上 `(461,571)` 投影的 `cha4.png` 首个 79×79 单元，非零 alpha 掩码也恰为 **1,433** 点，两集合完全相同。源 alpha 最大 **33/255**。对全部 1,433 个变化像素，用 `round(srcRGB×alpha/255 + filteredRGB×(1−alpha/255))` 预测完整图，三个 RGB 通道逐点**零误差**；若模拟“非零 alpha 全部当 255”，平均 RGB 误差 **122.3603**，没有一个变化像素三通道都在 2/255 内。机器结果及三输入 SHA 在 `pixel-analysis.json`（SHA-256 `BF14B21DB180818E1CDA114822DEF057C88C70C8552D97DF49B7884C739E9FA6`）。完整 PNG SHA `DD234780825C27212A362D21C38C66804BCF2580216BA9F49F6DF2B1DC9E4CC4`，去子体 PNG SHA `C243BB056FF7847318879491346306011DFC763CFC1A5F9399B3827C64F87A01`；完整图已目视检查。

第一次运行把第二个参数误给到 `runtime/vfs`，Session 明确报缺 `vfs/decoded_dat` 并 exit5；原 `run.log`/`run-exit.txt` 保留。纠正为两个参数均指正式 `resources/runtime` 后，`run-valid-root.log` 及 `run-valid-root-exit.txt=0`。重复以同名 PNG 运行被拒绝，exit3 且两 PNG SHA 未变，见 `no-overwrite-result.json`。受保护 Battle/Menu Scene SHA 分别保持 `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`、`785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`。`git -c core.safecrlf=false diff --check` 为0；账本最终校验结果见 Change Record。

已有原 Battle Scene 自然香燐 Legacy 开/关相机图证明 Unity 这一例实际使用低 alpha（`NTSD28-Q09-P21-NATURAL-ALPHA-ATTRIBUTION-20260928`）；本轮新增的是**正式配对渲染器**从同一正式源图采样后的 GPU 证据。两边背景、相机、视口及初始 World 不同，不能把 1,433 与 Unity 的 1,245 变化像素逐点比较；Unity 那 65 个简单源掩码未解释的像素也未由本包证明原因。根正式 EXE 自身 GPU、中央 Unity 出口、其它技能/角色与 P-21/Q09/R17 整体验收仍开放。未修改 Unity 生产、DAT 数值、PNG、Scene、项目相机、背景、模式 Asset 或非战斗功能，也未重跑 Unity 编译/Play。
