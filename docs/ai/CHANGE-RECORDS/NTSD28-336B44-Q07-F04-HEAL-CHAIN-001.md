<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-F04-HEAL-CHAIN-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/f04_oid600_heal_chain_probe.cpp
authority: formal 336B44 playable OID600 to OID219 to target E4 chain and formal DAT
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-F04-HEAL-CHAIN-001.md
-->

# F04 正式道具治疗后继链完整 tick 诊断

脚本前登记。既有槽2载体包已证录制/根重建12tick，但停在OID219/action51，未证友方预赋子体、目标E4、后续治疗时点。只新增一个完整GameSession诊断文件和独立新证据目录；源初态、正式资源、LFR格式均沿用已验包，不手设 E4、HP 或速度，不修改既有载体探针。

预计副作用只在新诊断脚本及其输出。验收：当前正式源码闭包编译、受控完整tick源链逐点输出、双跑确定性（适用时）、源/本地LFR和根正式EXE选定字段对比；源内部 E4/目标槽不在根trace中，不宣称根逐字段直接见证。确切范围、不变量、后续未验项见同ID Task。回滚仅审本包新脚本/文档，旧用户未提交内容不清理。实际结果与失败需追加记录，不能用载体已PASS替代本包运行证据。

实际只新增 `Tools/NTSD28Q07Diagnostics/f04_oid600_heal_chain_probe.cpp`，从已验槽2初态延长到60tick并加槽50～53、E4与预赋目标采样；正式源码/EXE、Unity生产、DAT、Scene均未改。当前 playable 源码闭包编译 exit0/stderr0；源/本地回放初始+60tick七槽427/427声明采样无差。正式根同LFR进程exit0、passed/failure0，tick0～60七槽 active+活跃字段合计2983/2983零差。源tick14预赋目标子体HP0/aiTarget0、60tick E4始终0、无action60；根可见子体HP0及无action60，但根trace不公开E4。v1字段比较器把空槽哨值误当坐标差，572个假差保留；v2只比较空槽active后为零差。Unity `LF2Entity` 子体初始HP0和行为4 HP≤0门仅静态复核，未跑本链Unity Play；没有生产首差或新修复。完整原件和边界见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-F04-HEAL-CHAIN-001/REPORT.md)。本ID `VERIFIED` 只表示有界阴性真实复现，F04/Q07/总目标保持开放。回滚仅审本新增脚本及文档，不动其他用户文件。

同一诊断EXE/初态第二轮源码运行 exit0，CSV和LFR分别与第一轮逐SHA一致，E4仍无正值；正式根仅运行第一轮包。双跑原件见上述报告，不提升全局不可达性结论。

交付检查：`Tools/Validate-ChangeLedger.ps1` exit0/PASSED（1113 Records、56 governed code files in diff）；相关文档 `git diff --check` exit0；本包四个新文本文件 UTF-8、尾空格与末行检查通过。Unity 编译/Play 未运行，不能报告 F04 已对齐。
