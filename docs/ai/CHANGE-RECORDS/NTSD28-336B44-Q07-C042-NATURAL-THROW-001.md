<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C042-NATURAL-THROW-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/bee_natural_throw_lfr_probe.cpp
authority: selected 336B44 playable GameSession step and BattleWorld catch/throw pass with formal OID75 DAT
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C042-NATURAL-THROW-001.md
-->

# C042 正式内容自然投掷诊断

本 Record 在任何探针脚本修改前创建。原状：C042 共用 writer 的受控即时首差已由当前源码392例和 Unity 原 Editor RED/GREEN 修复，但根 LFR 初态无抓取关系与计数承载；正式 OID75 DAT 的 action355→358～375 仅静态可达。只允许新建 Task 声明的一个 C++ 诊断源和独立产物目录，不改生产、DAT、Scene、非战斗。

预期副作用是额外的诊断 EXE/CSV/LFR，仅在手动运行时生成；不加入正式构建，不改变玩法。验收要求近远位置完整 Driver 正反、真实关系和投掷前非零计数，再以同 LFR 跑选定根并逐 tick 比较；源码阴性应记录而非伪造阳性。回滚须审阅这个新增诊断文件，不触碰其它未提交工作。初始状态 `PLANNED`，编译/运行未执行，Q07/C042运行时出口及总目标开放。

v1脚本已写入声明的唯一路径，当前源码g++编译退出0。action355/X550第1tick抓取、第55tick投掷，投掷前被投者计数0；X530～600均同，X520/X1200无抓取。首次正式根回放误沿用旧F05参数角色36/action243，报告FAIL46，保留；修正为75/action355后同LFR正式根报告PASS/0。此链证明自然投掷可达，却不触发C042非零计数条件。下一在同脚本增加已静态确认的action378第二抓取分支，仅接受355/378，保持位置/种子/双方不变；源码阴性不强写计数。原Scene仍未运行。

v3终证：原诊断文件仅扩action355/378与160tick窗口；g++ v1/v2/v3均退出0。355近距tick55、378近距tick142自然投掷，X1200远距各阴性；投前被投者计数均0，故C042非零门未通过。两近距LFR在正式根报告PASS，离线限定1360/1360和2720/2720字段零差，独立CRT state/根EOF排除。首次旧F05参数根FAIL46与Start-Process含空格参数失败留证。原Scene、全World、非零计数根证书未验。实际修改仅声明的一份C++诊断源，未改生产/DAT/Scene/非战斗。状态`RUNTIME_PENDING`，报告见[REPORT](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C042-NATURAL-THROW-001/REPORT.md)。

收尾校验：`Tools/Validate-ChangeLedger.ps1` 退出0（1072 Records、11代码文件）；`git -c core.safecrlf=false diff --check` 退出0；四保护 SHA 与基线相同，原 Editor idle/nonPlay/noncompiling。C042父项与Q07仍开放。
