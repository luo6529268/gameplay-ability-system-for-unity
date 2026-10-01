# NTSD28-336B44-Q07-C045-NATURAL-REACH-001

状态：`VERIFIED / SOURCE_ROOT_NATURAL_ONLY`。父目标NTSD28-UNITY-BATTLE-REALIGNMENT-001，BATCH-04/Q07/C045。唯一规则权威为正式336B44 EXE及对应 playable `GameSession28::step`→`BattleWorld28::settle_catch_relations`，正式 DAT OID65 `c/ank/ank.dat` 与OID2 `c/nar/nar.dat`。action348/357 kind3 抓取→371/366→…→376 injury100、cover1、recover默认0的只读入口见[审计](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C045-NATURAL-ENTRY-AUDIT-20261001/REPORT.md)。

仅新增 `Tools/NTSD28Q07Diagnostics/ank_held_injury_lfr_probe.cpp` 和本包证据；用当前Core/Playable与正式DAT近/远有界完整GameSession测试action348/357及多个目标X，逐tick记录抓取、关系、动作、HP、双方hold、动作计数、伤害pass、RNG并输出源LFR。若自然阳性再同LFR根EXE复核、之后原Unity Battle Scene首差；阴性记录实际边界并转其他正式可达首差。低层源码受控/手设关系不冒充根自然。不得改生产、DAT、Scene、非战斗、用户例外或既有脏工作。编译、双跑稳定、内容SHA和账本验证；回滚只审阅本新增诊断文件。

结果：当前源12组近远120tick均exit0，348/X550、580、600及357/X700自然抓取并分别在tick18/23动作376造成100伤害、抓取者hold2/目标-3；远距及错位阴性。348/X550源码复跑CSV/LFR同SHA。正式根三组同LFR replay PASS，声明22字段×120tick×3=7920零差；Unity/原Scene首差仍待，不关闭父C045/Q07。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C045-NATURAL-REACH-001/REPORT.md)。
