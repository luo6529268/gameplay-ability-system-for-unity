<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C040-HINATA-BEE-NATURAL-REACH-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/hinata_bee_held_position_lfr_probe.cpp
authority: selected 336B44 playable BattleWorld catch settlement and formal OID41/75 DAT
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C040-HINATA-BEE-NATURAL-REACH-001.md
-->

# C040 雏田抓奇拉比持有定位自然可达性

正式源六例有界筛选已完成：[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C040-HINATA-BEE-NATURAL-REACH-001/REPORT.md)。实际只增声明的CPP；当前playable闭包g++两次0诊断，action286/360×X550/600/1200各120tick均成功。v1用tick入口hold误报候选；v2按正式pass顺序收紧为结算后目标动作仍异于vaction且hold非零/定位CPOINT有差，此六例触发0。近距自然抓取/伤害确证、远距无抓取；本包`VERIFIED`仅指这六例有界阴性，不关闭C040/Q07，不做无触发的根或Unity晋升。未改正式源、DAT、Unity、Scene/Prefab或非战斗；回滚只审阅本新增诊断CPP。
