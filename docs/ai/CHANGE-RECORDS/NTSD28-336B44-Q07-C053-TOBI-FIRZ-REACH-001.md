<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C053-TOBI-FIRZ-REACH-001
status: SUPERSEDED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/c053_tobi_firz_reach_probe.cpp
authority: selected 336B44 playable GameSession28 and formal Tobi frame512 OPoint OID251 action51 plus firz frame chain
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C053-TOBI-FIRZ-REACH-001.md
-->

# C053 Tobi自然投射物正式源/根前置探针

脚本修改前登记。当前只有静态DAT `Tobi frame512 → OID251/action51` 与受控初始OID251/action0双Uj，尚无正式运行时的生产者到投射物帧序列。唯一新增工具 `c053_tobi_firz_reach_probe.cpp`，使用正式playable完整GameSession、正式runtime内容、受控Tobi初始action510与中性输入，有限三Y×40tick。记录出生与实体逐tick，阳性才写LFR送根正式EXE。既有C053诊断/Unity生产/测试、正式源/EXE、DAT、Scene、模式和非战斗均不改。

输出只建新目录，拒绝覆盖；失败时保留日志和首差。验收为C++当前闭包编译、正式源出生/action序列与根LFR同条件可比字段；Unity原Scene和玩家普通输入另属未完成门。受控action510不能包装为自然物理键完整选招，未到action0不能依据DAT静态next臆补。回滚只审本新工具和文档，删除须文件操作审计及用户授权。

2026-10-04 首轮记录：工具已写，当前336B44源码闭包编译exit0、无stderr；三组负Y各40 tick没有OID251，三组首tick Tobi均为action212，输出为`artifacts/diagnostics/NTSD28-336B44-Q07-C053-TOBI-FIRZ-REACH-001/run-a/summary.csv`及`ticks.csv`。正式根EXE未回放（没有阳性LFR），Unity Play未运行。状态保持`CODE_WRITTEN`，不宣称功能通过。下一窄改仍只在该工具内：记录初态和首tick frame event，并加一个Y=0/action510对照；复用原编译命令写新输出，不覆盖首轮结果。

2026-10-04 补充轮：同一唯一工具记录tick0初态和每tick frame event，在Y=0额外40 tick。新`c053-tobi-firz-reach-v2.exe`编译exit0、无输出诊断；`run-b/summary.csv`四组均无OID251。负Y首tick510→212/status=held，Y=0首tick510→510/status=held且前15tick仍停510。实际代码只改`Tools/NTSD28Q07Diagnostics/c053_tobi_firz_reach_probe.cpp`的只读记录与单个Y对照；不改规则或内容。报告为`artifacts/diagnostics/NTSD28-336B44-Q07-C053-TOBI-FIRZ-REACH-001/REPORT.md`。状态`RUNTIME_PENDING`：本有限阴性已测，正式自然按键、阳性LFR/根回放、原Unity Scene均待。回滚仅针对新工具及诊断记录；未执行删除或覆盖。

2026-10-04 supersede：正式`data/data.txt`核对推翻原OID假设；本工具测试OID53/`ttobi.dat`，不是含frame512 OPoint的OID0/`tobi.dat`。当时阴性本身是真实输出，但不能裁决目标OID0。状态改`SUPERSEDED`，由`NTSD28-336B44-Q07-C053-TOBI-PHYSICAL-REACH-001`的正确OID0同版自然输入源/根证据接管；保留工具与原始输出，不执行删除。
