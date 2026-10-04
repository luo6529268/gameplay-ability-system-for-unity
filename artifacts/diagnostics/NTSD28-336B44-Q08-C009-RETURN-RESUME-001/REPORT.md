# Q08/C009 返组后再次单组恢复，正式源/根限定见证

2026-10-04，权威根 EXE SHA-256：`336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。仅使用当前对应 playable 构建闭包、正式 `resources/runtime`；Unity 生产、DAT、图片、Scene 和非战斗逻辑均未修改。

既有 Q08/C009 证据只覆盖第二组返场暂停。此次静态筛选 `data/data.txt` 的 type3→type0 OPoint，选 OID220/action0→OID9/action386 和 OID230/action77→OID14/action355 两例，各用一个 OID56/team1 存活者与 team2 的 type3 发射者、mode0/seed682973786、中性输入，最多20个完整 `GameSession28::step()`。新[探针](../../../Tools/NTSD28Q08Diagnostics/return_resume_lfr_probe.cpp)按既有完整 playable 28+ 源文件参数编译 exit0、无诊断；[参数](compile-argv.txt)、[编译输出](compile-output.txt)。

OID220 案正式源码逐 tick 输出在[CSV](run-v1/220/source-host-rows.csv)：tick1 结果 timer=1，OPoint 子体 OID9/team2 出生；tick2～4 `BattleFlow` 两组，timer保留1、winner=-1；tick4 世界尾部子体消失；tick5 `BattleFlow` 再判一组，timer从1到2、winner回1。这里的 `flow_groups` 是战斗结果 pass 的分类，`world_groups` 是该 tick 结束后的实体计数；二者在 tick1与tick4不同属正常 pass 顺序，不能互相替代。OID230 案在20tick内[未生成目标子体](run-v1/230/source-host-rows.csv)，只保留为有限阴性，不断言整体不可达。

OID220 阳性用同一[源 LFR](run-v1/220/source-packets.lfr)交当前正式根 EXE，`--character 56 --enemy 220 --lfr-slot0-action 0 --lfr-slot1-action 0 --p2-human`，根进程 exit0；[报告](run-v1/220/root-report.json) `passed=true/failureCode=0/declaredTicks=20`，根多跑终端 tick21，比较只取声明的 tick1～20。[正式根 trace](run-v1/220/root-trace.jsonl) 与源逐 tick 按 world tick、timer、timerAfter、winner、World存活组、OID9子体数、发射者 action 比较 [140/140 零差](run-v1/220/source-root-selected-comparison.json)。根报告 `nativeParityClaim=false`，本报告只声明所选字段和限定场景的独立回放，不声明全World同态。

原 Unity Editor 的全套 EditMode 测试已由用户取消且原 MCP 独立读到 `tests.is_running=false/current_job_id=null`；Battle Scene clean、非Play。原 Editor 仍 `is_compiling=true`，新程序集未就绪，故没有在旧程序集运行 Unity Play。本次不关闭 C009/Q08：仍须原 Battle Scene 同初态通过完整 tick 证实暂停及恢复，并保留玩家物理输入链的独立条件门。受控 OID220 初始对象并非物理按键自然选招。

[Change Ledger 校验](change-ledger-validation.txt) exit0、1232条Record、本新工具被本Change ID覆盖；已跟踪文档 `git diff --check` exit0。未执行全套EditMode重跑；用户刚取消的旧全套作业不能算本包回归。
