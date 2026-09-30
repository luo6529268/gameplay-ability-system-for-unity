# Q09/P-20 飞段自然 430 原 Battle 场景相机像素限定验收

状态：`VERIFIED_SCOPED_ORIGINAL_WORLD_CAMERA_BODY_VISIBLE`。只关闭“原 Battle 自然 frame430 中央本体在实际 World Camera 输出可见”子门；P-20、Q09、BATCH-05 和总目标继续开放，Q07/D-024 通用碰撞域独立待用户取舍。

前置权威为正式根 `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`；匹配源码的[自然输入配对 WARP](../NTSD28-Q09-P20-HIDAN-NATURAL-WARP-001/ACCEPTANCE.md)在 tick16/action430/pic119/hid6 上同快照本体 A/B 差 1,995 像素。原 Battle 此前[目录绑定验收](../NTSD28-Q09-P20-HIDAN-BATTLE-COMMAND-001/ACCEPTANCE.md)确认自然 430/pic119/visualDataId24 实际读 Logan hid6，Unity 左下源矩形 `(722,871,360,289)`，Legacy 与中央绑定有效。本包没有重跑正式轨迹或角色矩阵。

原项目唯一 Editor PID11944，在 Battle Scene clean/非Play/idle 时通过 MCP `refresh_unity(force, all, compile=request)` 原位刷新；[生成工程编译](generated-editor-build.log)退出0、0错误/212警告，Editor DLL 时间晚于新脚本。仅提交一次物理按键请求 `hidan-frame430-scene-pixel-20260929-01`，实际[原始报告](hidan-frame430-scene-pixel-20260929-01.json)为 `CAPTURED_SCOPED_FRAME430_PIXEL`、无 `firstDifference`/error：J1–2、K9–10、L+D+J13–14 经完整 Driver 在相对 tick15/绝对20 自然到 action430/pic119，World Camera 开启。采集前后 World tick20 与战斗 checksum 不变，临时 Camera 状态已恢复。

在该自然帧，探针用原 World Camera 的真实 `Camera.Render()` 取得[白底中央画面](hidan-frame430-scene-pixel-20260929-01-scene-camera.png)，并从同一冻结命令列表只移除 actor 的唯一 Entity 本体命令，分别经中央资源 resolver/mesh backend 生成[本体有](hidan-frame430-scene-pixel-20260929-01-body-on.png)与[本体无](hidan-frame430-scene-pixel-20260929-01-body-off.png)诊断图。三图均为 1333×730；原相机图显示飞段本体、目标、阴影和战斗标记。独立[像素分析](pixel-analysis.json)确认诊断本体 A/B 差 **1,812** 像素。

**原始比较值的纠正：** 探针报告中的未配准同坐标计数是“本体有匹配0、本体无匹配1812”。该数值不能证明原相机没画本体：诊断命令缓冲图在 PNG 读回中相对 Camera 图上下翻转，且两次采样的展示位置不同。保留原始计数，不篡改报告。独立分析先翻转诊断图，再在 `dx∈[-25,25]`、`dy∈[-35,35]` 中搜索整数平移；唯一最高分为 `(dx=+8,dy=-10)`，在本体变化的1812像素中，Camera 图与本体有图精确匹配1540、与本体无图匹配56、其余216，次高分仅848。容差2与8得到相同计数；可见像素和图像本身共同证实本体确实出现在原 World Camera 输出。中央路径 `ResolveDisplayAlpha`/`DisplayMotion.ApplyToCapturedCommands` 允许相机渲染时重新采样展示位置，可能解释部分平移；本包没有记录两次具体 alpha，故不把 `(8,-10)` 归因写成已证事实，也不据此修改生产渲染。

三图 SHA-256：Camera `A6B15926E638827FFC6A126741EBF65D6AB04549E3764C1130ADFAF79F891048`，本体有 `36C90BB16141529C9C6CA2E6967B544993F7D129D149500F80865DF4C5058CC4`，本体无 `C2B6D7E33456AEAC4BF1AD40604509E5C3D178042007C5CE34A08B9D8B396B84`。退出 Play 时 ordered shutdown `Completed/Stopped`、World detached、对象/槽位/借用数 `0/0/0`，Scene clean，Battle Scene 前后 SHA-256 均 `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`；原 Editor 随后回到 Edit/idle。

这张白底图是原 Scene World Camera 的中央渲染隔离读回，**不是**包含项目背景/HUD的完整 Game View，也不是正式根 EXE 自身 GUI GPU 或两端同世界、同相机、逐像素等价证明。正式的1,995像素和 Unity 的1,812像素属于不同画面/展示映射，不能直接相减作为生产差异。P-20 人为缺图负例、其它 hidden/terminal/fallback 和 Q09 的其它表现项仍归原计划。

治理验证：`Tools/Validate-ChangeLedger.ps1` 退出0并报告 PASSED（1007 Records、35 个当前 diff 代码文件均有覆盖），[完整输出](ledger-validation.log)已留存；`git -c core.safecrlf=false diff --check` 退出0。Battle/Menu Scene、ProjectBattleModeConfig、EditorBuildSettings 四保护 SHA-256 分别为 `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`、`785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`、`B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`、`8D621A077642B5154BA305861CA57DFB549FF4FDE5BC58F3E284536A5982F01E`，均与包前值一致。未执行全量测试，因为本包只改诊断且已有唯一原场景物理 Play 与正式配对源证据。
