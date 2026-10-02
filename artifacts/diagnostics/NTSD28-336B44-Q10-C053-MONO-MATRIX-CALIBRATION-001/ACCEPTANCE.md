# 336B44/C053 Unity 单声道左右矩阵校准

2026-10-02。`NTSD28-336B44-Q10-C053-MONO-MATRIX-CALIBRATION-001` 的**原 Editor 临时单声道 AudioRenderer 出口** `VERIFIED`；Q10 战斗生产声像、正式 EXE 设备波形、stereo clip、完整背景下的声像相机策略及总目标仍开放。

本轮重算根正式 `NTSD2.8-Logan.exe` SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。输入矩阵取当前 playable 的 C053 12-tick/10 自然事件证据，其中相机 X 均为 0，六个不同整数目标为 64/36、75/25、65/35、63/37、78/22、66/34；正式 `020.wav` 和 `067.wav` 均是 mono。此包没有重新定义正式声像规则，也没有用旧 B1E13 目标代替新规则。

既有 Editor 校准脚本只增加独立 `RunCurrentC053` 菜单与八组案例选择，复用原采样/清理循环。历史 `Run()` 仍使用原五组目标和 `calibration-v3.json`，该旧原件 SHA-256 仍为 `024D2AD54FB348B63C2D754D591CB88FEE03E52762E94735EC271988DCC6CDDD`。生成 C# Editor 工程 `dotnet build --no-restore` exit0、0 error/284 warnings；原 Editor 刷新后 `Assembly-CSharp-Editor.dll` 时间 `05:31:46 UTC` 晚于目标脚本 `05:29:18 UTC`，MCP 状态 idle/无编译。仅据这两层报告编译，不把它当声音行为证明。

原 Editor 先读 Battle Scene `isDirty=false`，经 MCP 打开已保存 Menu、进入 Play、调用新菜单；新[原始 JSON](calibration-20261002-053649-970.json) SHA-256 `A957DFA23992010EE3FE39354EF85F23C46557FE2C4BD8309EDB7ECEC10C903B`，`status=CAPTURE_COMPLETE`、`speakerMode=Stereo`、48 kHz、八组均有 29,696～35,840 采样帧、`captureStopped=true`。结果相对于硬左参考的左右增益：

| 案例 | 正式/控制目标 L/R | Unity 实测 L/R | 最大单声道误差 |
| --- | ---: | ---: | ---: |
| 硬左参考 | 1/0 | 1.000000/0.000000 | 0 |
| Unity 居中参考 | 0.707107/0.707107 | 0.707409/0.707409 | 0.000303 |
| 当前 64/36 | 0.64/0.36 | 0.639012/0.359444 | 0.000988 |
| 当前 75/25 | 0.75/0.25 | 0.748946/0.249649 | 0.001054 |
| 当前 65/35 | 0.65/0.35 | 0.650175/0.350094 | 0.000175 |
| 当前 63/37 | 0.63/0.37 | 0.629115/0.369480 | 0.000885 |
| 当前 78/22 | 0.78/0.22 | 0.780334/0.220094 | 0.000334 |
| 当前 66/34 | 0.66/0.34 | 0.658981/0.339475 | 0.001019 |

六个当前目标最大误差 `0.00105381011962891 < 0.002` 预定门槛。此前只读 `execute_code` 状态查询因 MCP 的 Mono 编译器“文件名或扩展名太长”失败，未被算作测试结果，也没有阻断随后独立 `execute_menu_item` 和 JSON 采样。

采样后 MCP 退出 Play、Menu `isDirty=false`，重新打开 Battle 并读 `isDirty=false`、rootCount13；Menu/Battle 磁盘 SHA-256 前后分别保持 `DD6A48A37FB8CEA9CD8A1F7738964A719E007A42F0FBB54FB48BBD0B723B9DC3` / `93448372834A1BEAF2C9ACD90E2EF17E2EA487E19F601B9815A907A974D7BF60`。既有脚本的 `Finish` 路径会恢复 `Time.captureFramerate`/`runInBackground` 并停止临时声音；本次直接记录到 `captureStopped=true` 与成功退出 Play，没有额外读取这两个全局值，因此不声称独立逐值核对。`Tools/Validate-ChangeLedger.ps1` exit0/PASSED（1151 Records）；目标脚本与文档 `git diff --check` exit0。

这份结果只证明该项目当前 Unity 版本、临时 440 Hz mono 声源和无其它声音的 Menu Play 下，可以用旧校准反算式逼近当前六组目标。实际战斗 voice 还会经过 clip、`AudioItem`、音量热键、mixer/pool 和固定背景声像策略；这些须在独立生产包定向实测，不能把本结果称为 Q10 完成。
