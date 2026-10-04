# Q09 当前 336B44 正式根 EXE 实际窗口像素取证（2026-10-05）

结论：**正式根 EXE 的实际可见窗口像素已可通过只读 Windows API 获取**。本包只关闭“正式发行窗口没有可用像素取证接口”的工具阻断；尚未取得与 Unity 同初态、同逻辑 tick、同呈现插值相位的画面配对，不能据此关闭 Q09、Q12 或总目标，也不以原版背景/HUD 与项目自有表现作逐像素相等门。

权威身份：发行根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`，资源根为同目录 `resources/runtime`。两次均直接运行该发行 EXE 的真实 GUI/D3D11 路径，参数指定鸣人 OID2、鼬 OID9、P2 human、P1 X500/P2 X620、双方 Z400、正式资源根和 9000 ms GUI 报告。未使用 computer-use 工具，未合成鼠标/键盘事件；没有改发行 EXE、DAT、Unity 脚本、Scene 或配置。当前原 Unity Editor 仍为独立 PID 138072，本包没有切换其场景或进入 Play。

有效图像为 [`formal-root-printwindow-20261004T165519021Z.png`](formal-root-printwindow-20261004T165519021Z.png)，SHA-256 `B6E39A7311F2240B9CAF7BD45F5CF55C40606A308D33AF9EDF1271F1F1602027`。抓图进程先调用 `SetProcessDPIAware`，对目标 HWND 调用 `GetClientRect`，再以 `PrintWindow(hwnd, hdc, PW_CLIENTONLY | PW_RENDERFULLCONTENT)` 读取窗口客户区；返回 true。图像尺寸 1600×900，读取像素所得非黑包围盒精确为 `(0,0)-(1280,720)`，右侧 320 像素与底部 180 像素为黑色。这与发行渲染器的 1280×720 有效画布相容；图像可见两名角色、姓名牌、地面及背景，但本次未为其绑定精确 tick 或单独导出 GPU backbuffer。

对应 [GUI 原件](formal-root-gui-20261004T165519021Z.json) SHA-256 `6EFCCC4D1356E389C6CA985F3F30D3E792875997FB2062CA5C638E453C1DDAB7`：PID 57968 自行退出；窗口可见、D3D11 swap chain 已初始化、成功 Present 832 帧、snapshot 832 次、逻辑 tick 246、XAudio2/BGM 正常，`runtimeContractPassed=true`，无 fatal error。进程退出码 38、`passed=false/failureStage=manual_input` 是 GUI 总验收要求真实按键而本包没有发送或要求按键所致，不是图形初始化或运行时失败。报告结束时的 246 tick 不能当作截图采样 tick。

首次 [`formal-root-window-20261004T165322403Z.png`](formal-root-window-20261004T165322403Z.png) SHA-256 `43FC95F3B46F20FFD75FA3A91B2E30805DAF510167F36A3CC4A7DA40F75CAE50` 与 [首次 GUI 报告](formal-root-gui-20261004T165322403Z.json) 保留为诊断原件。首次抓图进程未声明 DPI awareness，`CopyFromScreen` 混用了逻辑与物理坐标，截入窗口边框、桌面边缘并截断地面，因此**不得用于角色像素比较**；其 GUI 报告的 D3D11/Present 成功只作捕获路线的前置证据。

下一画面出口只在当前 336B44 正式可达、非用户例外的具体场景出现待裁决首差时，冻结双方角色/初态/输入/tick、记录窗口标题的 tick 边界与 Unity 呈现相位，再配对角色、武器、血点或阴影局部区域；无法建立同帧条件时继续标 `PRESENT_PAIR_PENDING`，不把静止截图或 offscreen gate 图像提升为同帧一致证书。不为此重跑已关闭的 Q12 重进、C051 护甲或整张 Q09 案例表。

2026-10-05 Unity 对照入口复核：当前项目 `ProjectVersion.txt` 为 Unity 2022.3，`Packages/manifest.json` 使用 `com.coplaydev.unity-mcp` 而无 `com.unity.pipeline`；`unity status --project-path <原项目> --format json --non-interactive` 返回 `STATUS_NO_INSTANCES`，其含义是没有安装 Pipeline 的可连接 Editor，**不能据此说原 Editor 已关闭**。已有 [Q09 Game View 原件](../NTSD28-336B44-Q09-NATURAL-GAMEVIEW-TICK-001/natural-nameplate-20261002-004743-366-996e805edb434e0a93fb4ff418e6a173.json)是 Unity tick2152、两个活跃 OID2、方向 D 十 tick 的移动场景；本次正式窗口为 OID2/OID9、无输入且截图未绑定精确 tick，故不能作为同态 A/B。未安装 Pipeline、未启动第二 Unity、未改测试脚本或重跑既有 Q09 Scene。
