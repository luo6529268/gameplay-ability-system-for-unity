<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C050-HIDAN-BDY50-REACH-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/hidan_bdy50_vertical_lfr_probe.cpp
authority: selected 336B44 playable BattleWorld28 unarmored hit and formal OID24/OID56 DAT
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C050-HIDAN-BDY50-REACH-001.md
-->

# C050 Hidan kind50 锁存帧垂直响应可达性

脚本前原状：C050 只有正式源码和 DAT 静态疑点；此前 effect8～16/caughtact -2/-3 的29个 ITR dvy 都为0。Unity prelude 识别特殊 rest，但目前未证自然普通垂直命中首差。

预定唯一代码路径是新建 `Tools/NTSD28Q07Diagnostics/hidan_bdy50_vertical_lfr_probe.cpp`；其职责是使用正式完整 GameSession 做近/远受控初态诊断与 LFR，不修改任何战斗规则或内容。预期副作用仅是新诊断文件，正式 DAT/源、Unity 项目资产、用户脏工作不覆盖。验收、边界与回滚见 Task。完成后追加实际命令、输出、限制及状态。

实际脚本按声明新增；g++正式Core/playable链接exit0，X520/X550/X1200各40tick双跑CSV/LFR同SHA。X520/X550第2tick应用dvy-5且首BDY50，正式源码垂直累计0/目标Vy0；X1200无命中。336B44根两组原LFR回放报告PASS，另以五字段×40tick各200/200比较同态。空编译日志未生成导致包装器读取错误，实际编译产物正常。原Unity/物理键未验；详细命令/输出和边界见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C050-HIDAN-BDY50-REACH-001/REPORT.md)。
