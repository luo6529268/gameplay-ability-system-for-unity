# C045 正式 OID65 抓取→376 伤害自然可达性

当前唯一规则权威：根 EXE SHA-256 336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3；对应 playable GameSession28::step() / BattleWorld28::settle_catch_relations()；正式 OID65 ank.dat 与 staged 同 SHA 7BEEFED713CC85CDF2BF36399FE158B63E822038FBED09AE79901BB6B3E81AC5。只新增源诊断 Tools/NTSD28Q07Diagnostics/ank_held_injury_lfr_probe.cpp，生产、DAT、Scene未改。当前Core/Playable g++ 编译 exit0、compile-v1.txt 无诊断。

输入为 actor OID65 X500、目标鸣人OID2，双方HP/MP500、mode0/seed682973786、中性输入；仅初始 action 和目标X变化，完整 GameSession 每例120 tick。12组源码探针均 exit0：

| 初始动作与目标X | 自然关系与伤害 | 关键字段 |
| --- | --- | --- |
| action348 / X550、580、600 | tick1 kind3抓取，tick18进入376并维持关系，settlement正伤害100 | 目标HP500→400；抓取者hold2、受害者-3，抓取者动作计数1，目标动作137 |
| action348 / X650、700、1200 | 无抓取、无376伤害 | 远距阴性 |
| action357 / X700 | tick1 kind3抓取，tick23进入376并维持关系，settlement正伤害100 | 目标HP500→400；抓取者hold2、受害者-3 |
| action357 / X550、580、600、650、1200 | 无抓取、无376伤害 | 位置反例 |

action348/X550 同一当前源码再跑一次：source-ticks.csv 两次 SHA 均1252E9CA11F2133DFD7D7CC0BFA06BE095A7FE061FC6466A4DECD95384310CA3；source-packets.lfr 两次 SHA 均2EA931527F9D2FDCC7363A7CCCF31B3FEA10F297F3FB74701BA7BAC401B23936。完整源码 tick18 的关系、HP和停顿字段与正式根 trace tick18 同值；不是手设关系或timeout。

正式根用各自源LFR回放 action348/X550、action357/X700、action348/X1200，三个报告均 passed true/failureCode0/completedTicks121/traceRows122。逐tick对照 [比较结果](source-root-comparison-v1.json)：每例 tick1～120 的双方动作/计数、X/Y/Vx/Vy、HP、hold、关系、目标受伤统计共18字段，加 CRT调用数及同步RNG 共4字段，总22字段×120=2640，每例零差，三例共7920字段零差；排除正式根独立CRT内部state与额外EOF tick。根报告 nativeParityClaim=false 是内置LFR边界，本报告只主张列出的逐tick字段同态，不扩成全World或画面证书。

这把 C045 从静态字段候选推进到正式根自然伤害/停顿可达。Unity BattleCpointWriter.ApplyHeldInjury 当前按 cover1 不设置抓取者 FrameDelay=2，而正式源码按 recover默认0设置hold2；**Unity同条件首差尚未运行确认**。下一独立包在原Battle Scene同OID65近/远初态采完整tick（含双方动作/HP/hold/关系），先看首差；阳性后独立Task/Change修共用写者并作聚焦正反。C040/C043未因本包自动关闭，Q07/总目标继续开放。
