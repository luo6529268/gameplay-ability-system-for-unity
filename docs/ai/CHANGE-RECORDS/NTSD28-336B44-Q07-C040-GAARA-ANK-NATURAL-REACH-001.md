<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C040-GAARA-ANK-NATURAL-REACH-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/gaa_ank_held_position_lfr_probe.cpp
authority: selected 336B44 playable BattleWorld28::settle_catch_relations and formal OID16/OID65 DAT
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C040-GAARA-ANK-NATURAL-REACH-001.md
-->

# C040 我爱罗→大蛇丸自然分源候选

脚本前记录：原项目仅有 OID41→75 六例阴性诊断；本包增加 OID16→65 的正式完整 GameSession/LFR 候选，不触碰生产。正式 DAT 静态条件及可复现输入、退出/回滚、首次阳性门槛详 Task。只有结算后关系仍活跃、受害者停顿非零、目标动作异于抓取者 vaction，且两帧挂点差值非零，才允许报告 C040 正例。

实际新增 `Tools/NTSD28Q07Diagnostics/gaa_ank_held_position_lfr_probe.cpp`：复用既有完整会话逐tick/LFR诊断，仅改正式对象16/65、起手动作365及目标X准入700～1200；没有触碰正式源、Unity或DAT。待编译与近/远正反例运行。

运行出口：`g++` 编译exit0/0诊断；当前完整GameSession action365/X780、X1200各120tick双跑，近距tick1抓取、tick28持有伤害但C040完整分源0，远距无抓取；同输入逐tickCSV/RNG/LFR各SHA相同。近距伤害后目标保持action130/hold-3→-1，vaction同为130，静态130→131/132挂点差未动态触发。根EXE身份SHA复核336B44，但无阳性故按合同不做其LFR回放或UnityPlay。本包`SCOPED_NEGATIVE / RUNTIME_PENDING`，C040/Q07开放。证据：[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C040-GAARA-ANK-NATURAL-REACH-001/REPORT.md)。
