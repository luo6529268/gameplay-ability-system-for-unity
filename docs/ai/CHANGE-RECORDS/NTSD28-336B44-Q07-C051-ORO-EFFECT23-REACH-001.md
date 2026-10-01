<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C051-ORO-EFFECT23-REACH-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/oro_effect23_reachability_probe.cpp
authority: selected 336B44 playable GameSession and formal OID20 to OID888 DAT parent-child chain
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C051-ORO-EFFECT23-REACH-001.md
-->

# C051 大蛇丸 effect23 子体正式可达性

脚本前状态：正式DAT存在OID20 frame288→289 OPoint OID888/action35→40/effect23/dvx-10，Unity已有相关相对方向分支；尚无当前336B44完整会话的实际出生/命中。仅新增本ID专用正式源码诊断，不改生产或DAT。预期副作用是新增隔离诊断源和输出；不触碰用户既有脏工作。验收、风险、回滚见Task。代码写入后立即追加实际路径/命令/结果，证据不足时不得晋升VERIFIED或C051完成。

实际代码已按唯一声明路径新增。诊断使用正式`GameSession28`完整tick和`GameSessionLfr28`，仅以指定位置/朝向构造OID20/action288与OID2对照；记录OID888出生、effect23候选/应用、目标动作/HP/X/Vx以及LFR，拒绝覆盖既有输出。当前仅`CODE_WRITTEN`，正式链接与运行结果待，不把静态候选当阳性。

首次正式链接exit0（`oro-effect23-source-v1.exe` 4,812,287 bytes；空stderr日志未生成）。X550/X700/X1200面右及X350面左各80完整tick进程exit0，子体tick4出生，effect23/dvx-10均在tick9应用，目标HP500→435；X1200意外仍命中，所以不能标作阴性。诊断追加子体X和正式水平冲量字段、允许X2000，待v2重新链接与独立输出；v1原件保留。未改Unity/DAT。

后继v2正式链接exit0；面右X550双跑CSV/LFR分别同SHA，面左X350也在tick9应用effect23，正式水平冲量右-10/左+10。X2000被正式stage钳为1330、子体追到1329仍命中，故无有效“远距不命中”样本。336B44根LFR面右和面左报告`passed:true`/failureCode0，各80tick选定8字段×80=640/640无差；左向首次根回放漏覆盖目标朝向而出现72字段差，修正两个初始朝向的v2根回放后0差，首次原件保留。只关闭正式源码/根选定字段的可达性诊断，不据此关闭C051或证明Unity；详细原始证据与限制见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C051-ORO-EFFECT23-REACH-001/REPORT.md)。
