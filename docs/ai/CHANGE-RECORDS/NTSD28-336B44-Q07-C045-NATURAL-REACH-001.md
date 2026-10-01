<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C045-NATURAL-REACH-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/ank_held_injury_lfr_probe.cpp
authority: selected 336B44 playable GameSession and BattleWorld catch settlement with formal OID65/OID2 DAT
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C045-NATURAL-REACH-001.md
-->

# C045 正式 OID65 自然伤害可达性

诊断脚本前创建。正式OID65 action348/357 kind3可接371/366，后继到376的injury100/cover1/recover默认0与Unity读取cover控制hold有静态分歧，但自然抓取及伤害是否发生未知。只新增声明的C++诊断及证据目录，生产Unity/内容/场景不改；运行GameSession完整tick并保存LFR、字段及反例，定位正式可达条件。若原始诊断失败保留证据并解释；后续Unity生产修复另立Task/Change。验收、边界与回滚见Task。状态PLANNED。

2026-10-01：只新增已声明C++诊断。Core/Playable g++编译exit0；当前正式DAT12组近远各120tick源码运行exit0，348/X550/580/600与357/X700为抓取→376伤害100阳性，远距/错位为阴性。348/X550源码二跑CSV/LFR各同SHA。根正式EXE已核336B44；同LFR回放三组均passed true/failureCode0，tick1～120双方18实体字段+4 RNG字段共7920逐项零差，独立CRT state/EOF排除。只关闭自然源/根可达性诊断，不证明Unity或全World，父C045/Q07开放。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C045-NATURAL-REACH-001/REPORT.md)。状态VERIFIED / SOURCE_ROOT_NATURAL_ONLY。
