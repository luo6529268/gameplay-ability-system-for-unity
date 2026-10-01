# C045 OID65 自然抓取伤害原 Battle Scene 验收（2026-10-01）

**限定结论：** 当前正式 336B44 源码与根 EXE 已分别证明 OID65 两种自然入招及远距控制；原 Unity Battle Scene 同初态运行生产 `SimulationTickDriver` 三组各 35 tick，17 个实体字段和 5 个 RNG 字段共 **2310/2310** 值一致、首差 0。三次均 `CAPTURED/DONE`、退出 Play、场景干净；C045 的 `recover` 停顿与 `cover` 定位分读在这些自然抓取链中得到验证。Q07 与整个对齐目标仍开放。

唯一战斗规则权威为根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 及对应 playable live path。正式 `ank.dat` action376 为 injury100、cover1、recover默认0；[源码/根自然可达性报告](../NTSD28-336B44-Q07-C045-NATURAL-REACH-001/REPORT.md)记录了三组根同LFR 120 tick、合计7920个声明字段零差。Unity 本轮读取原项目 staged正式内容，未编辑 DAT。

| 初态，均为OID65 X500→鸣人OID2、Z400、HP/MP500、mode0、seed682973786、中性输入 | 当前源/根关键结果 | 原 Battle Scene 35 tick 对照 |
| --- | --- | --- |
| action348／目标X550 | tick1抓取、tick18 action376伤害100，抓取者hold2、目标-3 | [原始JSON](ank65-a348-x550-natural-scene-01.json)，770/770、首差0；tick18目标HP400、双方停顿2/-3 |
| action357／目标X700 | tick1抓取、tick23 action376伤害100，抓取者hold2、目标-3 | [原始JSON](ank65-a357-x700-natural-scene-01.json)，770/770、首差0；tick23目标HP400、双方停顿2/-3 |
| action348／目标X1200 | 远距不抓取、无376伤害 | [原始JSON](ank65-a348-x1200-natural-scene-01.json)，770/770、首差0；目标HP500，抓取slot始终空 |

[逐tick比较JSON](comparison-v1.json)把 source-ticks.csv 中双方 action、counter、X/Y、速度、HP、FrameDelay、伤害累计和抓取目标槽等17字段，与 Unity `SourceRuleXInt`/战斗 Runtime 字段对照；source-rng.csv 的 CRT state/calls 与同步 RNG counter/index/calls 共5字段另行对照，容差0.001。最初诊断把源空抓取槽 `-1` 归一成 `0`，造成阴性组35个假差；最终比较使用双方原始槽值，三组均0差。此修正只涉及离线比较映射，没有修改战斗代码或原始采样。

新探针只在原 Battle Scene 的 Play 克隆中把 roster 指向 OID65/2，暂停自然入场后设置声明的初态，经生产 Driver 完整推进35个逻辑 tick；不会修改序列化Scene。新脚本原Editor Tundra编译成功、0个CS错误。三组结果各自 `sceneCleanAfter=true`、`exitedPlay=true`；最终Editor idle、非Play、非编译。Battle Scene SHA `3A089236328ACAE1510F8A831B77D4895CC34028DDCDEBE542BEF0DA8EC235ED`，Menu Scene `DD6A48A37FB8CEA9CD8A1F7738964A719E007A42F0FBB54FB48BBD0B723B9DC3`，GameConfig `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`，ProjectBattleModeConfig `B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`，均与运行前一致。

本轮只覆盖所列三组初态、前35 tick和22个对照字段；未证明正式根与 Unity 全World、全部120 tick、手动物理输入、画面像素或其它 OID 的整体等价。C045 项在这一自然伤害/停顿出口可标为 `VERIFIED_SCOPED_NATURAL`，Q07/D-024及总目标仍需按新版总表逐项推进。
