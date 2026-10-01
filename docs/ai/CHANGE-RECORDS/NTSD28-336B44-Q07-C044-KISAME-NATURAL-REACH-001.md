<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C044-KISAME-NATURAL-REACH-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/kisame_negative_decrease_lfr_probe.cpp
authority: selected 336B44 playable GameSession and formal OID17 Kisame negative-decrease DAT
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C044-KISAME-NATURAL-REACH-001.md
-->

# C044 OID17 Kisame 负decrease自然跨零可达性

正式源与根自然可达性已限定验证：[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C044-KISAME-NATURAL-REACH-001/REPORT.md)。实际只新增声明的诊断CPP；以正式 `resources/runtime` 初始化完整GameSession，action314/316近距均在tick47自然timeout1→-6且release1、pending X4/Y-3，远距无抓取。源四组160tick成功；根SHA吻合，三组LFR report PASS，20字段×160×3=9600零数值首差。初两版诊断器资源根误指向导致初始化失败，第三版编译0诊断并成功；失败输出保留。未改正式源码、DAT、Unity生产或Scene。`VERIFIED`仅指本包正式源/根自然触发，不代表 Unity 自然Scene 或C044父门关闭；回滚只审阅本新增诊断文件。
