# Q09/P-08 336B44 对应 playable 源码等 HP LFR 离屏血点复核

状态：`CURRENT_PLAYABLE_PAIRED_WARP_SCOPED_PASS / P-08_OPEN`。2026-10-04 本包不修改 C++、Unity 脚本、DAT、图片或场景，只重新编译既有诊断工具并保存新输出。

## 身份与方法

- 当前正式根 `NTSD2.8-Logan.exe` SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。本工具自身不是正式根 EXE；它以旧版已登记的同一 [未改动诊断源](../../../Tools/NTSD28Q09Diagnostics/ita_equal_hp_lfr_playback_warp_probe.cpp) SHA-256 `6FC1660981687E498F1CA49FEA1E859AB754853827CC66204BEEEC08F53CF595`，重新链接当前正式发行的 `source/ntsd28_core` 与 `source/ntsd28_playable` 源码。完整 [编译参数](compile-argv.txt)、[编译日志](compile.log)和 [退出码](compile-exit.txt)保留；编译退出0。
- 输入是既有等当前/基础HP LFR，SHA-256 `ABD2B83A72E7DC7496228CE56BD2310E3AC69C67BECCC69354A7FA50416C5C55`；两资源参数都指向当前正式 `resources/runtime`。完整 [运行参数](run-argv.txt)、[日志](run.log)、[退出码](run-exit.txt)保留；运行退出0。该工具回放22个声明tick及第23次终步，验证最终headers，选取tick22快照，仅在快照副本中移除唯一血点命令做离屏WARP A/B。正式根 EXE 本身另有 [headless LFR报告](../NTSD28-336B44-Q09-P08-EQUAL-HP-ROOT-LFR-001/REPORT.md)。

## 结果与边界

- tick22 鼬站立 action0、HP10/baseHP30。当前 playable 快照有唯一 OID9 血点，阈值10、尺寸1×3、红色 `0x00ff0000`；1333×730视口位置 `(461,595)`。终步头验证 `PASS`。
- [开启标记图](ita-mark-on-336b44.png)与[移除标记图](ita-mark-off-336b44.png)的独立 [RGB 全域分析](pixel-analysis.json)恰有3个差异像素 `(461,595..597)`：开启 `(255,0,0,255)`，关闭 `(106,118,121,255)`；其余无差异。两张当前重新编译图的 SHA 与 B1E13 时保存的对应图各自逐字节相同；这是**当前源码复跑观察到的结果**，不是把旧版状态直接晋升。
- 当前原 Unity Battle Scene 第一轮相同局部初态自然Play在第22 tick也有1条1×3中央血点，1920×1080相机图投影附近观测到5个纯红输出像素。Unity 保留完整背景与不同视口是用户例外，3对5不构成已证渲染差异；第一轮 Unity 输入夹具误用Jump，修正后的同输入第二轮仍待原 Editor 编译。见 [Unity配对报告](../NTSD28-336B44-Q09-P08-ROOT-UNITY-SAME-STATE-001/REPORT.md)。

此 WARP 结果只关闭当前 playable 源码离屏标记 A/B 子门。正式根 EXE 自身的 GPU Present、原 Unity 同输入终验、完整 World 和其它 P-08 出口未覆盖，故 P-08、Q09、Q12 和总目标继续开放。
