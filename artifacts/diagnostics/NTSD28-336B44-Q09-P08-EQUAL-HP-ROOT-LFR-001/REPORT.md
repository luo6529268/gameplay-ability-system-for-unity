# Q09/P-08 当前正式根 EXE 等 HP LFR 复跑

2026-10-04 以当前根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`，使用当前正式 `resources/runtime`，对既有等当前/基础 HP LFR 运行 headless playback。输入 LFR SHA-256 `ABD2B83A72E7DC7496228CE56BD2310E3AC69C67BECCC69354A7FA50416C5C55`；具体参数、stdout、exit、JSON 与 24 行诊断 trace 均在本目录独立保存。

退出码 0，JSON 为 `passed=true`、声明22 tick/完成23 tick，`nativeParityClaim=false`。根 tick0 的 CRT state/calls 为 `3374725112/3000`，相当于 seed0 的预热状态；原录制源 seed2833 的同计数状态不同。当前根22 tick 的 Naruto action/X、Ita action/X/HP/baseHP 六字段与录制源 132/132 相同，24 行当前根 trace 与此前根 trace 的 JSON 逐行相同；[机械比较](field-comparison.json)保留。

根 tick8 Ita HP30→10，tick22 Ita action0、HP10。headless trace 没有血点命令或 GPU 图像，不能据此关闭 P-08 画面出口或宣称当前正式 EXE Present 像素相同。原 Unity Battle Scene 配对入口及其第一轮错误输入更正见 [同局部初态报告](../NTSD28-336B44-Q09-P08-ROOT-UNITY-SAME-STATE-001/REPORT.md)。
