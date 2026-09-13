# Q05 五版本同步与子域头部检查出口

FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / TRACE_IDENTITY_PENDING。准确15脚本（5生产、9原测试、1新测试），附属WORLD-CLOCK-PHASE-FIXTURE独立记录同一测试文件中与版本无关的旧索引修正。总目标/Q05保持ACTIVE/FULL_ALIGNMENT_INCOMPLETE；这是步骤4的snapshot/checksum子出口。

当前生产版本已是entity13、aggregate21、checksum24、character shell2、base shell2。其他payload和协议版本保持。四个生产文件只有常量修改；BattleStateSnapshot.cs还修复由RED实际揭示的漏洞：过去IsValid只检查外层版本，内层entity12/character1/base1仍会被聚合恢复接受。现把TryPublish已有的完整子域schema/protocol/identity/tick条件提取为同一无副作用predicate，IsValid与TryPublish复用，恢复/会话/ring读取走既有关口，拒绝不改变原snapshot或World。原发布predicate逐字保留，未改恢复算法、payload、战斗规则或pass。

验证记录：
- 首次11 RED全失败，补旧checksum23 session replay RED1失败（曾被接受）。新测试最终23项均通过：五版本、四旧payload、旧checksum history、两profile claimed/raw+独立2F8/Spawner/输入历史/RNG恢复后两种checksum相同，以及11子域逐一污染schema/protocol/identity/tick并检查拒绝无副作用。
- 第一轮相关276=267PASS/9FAIL：5处跨行旧schema断言遗漏、1旧clock phase索引、3实际子域旧版本仍被接受；保留first-related证据并据此修复。
- 扩展相关287=286PASS/1旧clock末尾索引FAIL；该精确用例定向1/1 PASS闭合，287个不同测试全部具有通过证据。不是单次287/287绿色执行；见reconciled-test-evidence.json。29个请求类全部实际执行，没有缺失namespace类。
- clock旧夹具按正式simulation_tick_driver资源/帧/slot尾部顺序以及已退休positive-link正常33phase修正为相邻位置检查，保留总数33。没有为测试恢复退休phase；前后两次失败均保留。
- 完整BattleRuntimeSelfCheck：请求2026-09-13T12:32:12.1036639Z，结果2026-09-13T12:32:51.528638Z PASS；实际request/result已保存。最终error CS查询0，编译/Unity tests实际执行。
- 复用既有双OPoint Play probe，在当前编译版本真实NTSD_Battle tick5/对象4→4，两队列pending拒绝且任务保留、空闲capture成功并正常退出。workerWasPresent=false；不冒称新版本完整技能/worker实战/所有场景恢复回放。
- Scene isDirty=false/root14且SHA a96e11064f1bd054d9d5547fe8f02c702d971b3972d55754886d75b2dce9d28f保持；3059保护项2935不变/106既有或声明变化/18既有缺失，无新增缺失。git diff --check通过，只有既有CRLF提示。未改非战斗/Unity-GAS架构/资源/外部Server/Gen/Plugins，未提交/推送/删除素材，禁止computer-use持续遵守。

实际命令：Python -X utf8 Temp/Goal13_bridge.py refresh_unity、run_tests（test-selection.json）、get_test_job；SelfCheck请求文件机制；manage_editor play与既有NTSD28_Q05_SnapshotBoundary probe；read_console/manage_scene/get_editor_state；Validate-ChangeLedger.ps1。原生产仅常量的中间proof留作历史；最终以final-production-diff-proof.json（4常量+1复用predicate）为准，test-diff-proof覆盖九原测试。

后继仍在同一未发布窗口：NTSD28-Q05-TRACE-RAW-IDENTITY-JOINT-UPGRADE-001，trace全tag v3、raw/source wrapper v2、50字段+2F8，修复旧strategy-pending策略并绑定实际raw/decode/semantic/source/schema头与严格比较。原6MISSING保持，不凭载体绑定提升Q06 producer/AI行为。父步骤5完整同seed/input capture→restore→replay、slot/pool与Play仍待；当前新常量不能发布baseline或跳Q07正式资源迁移。

最终Change Ledger校验PASS：502 Records/20个当前累计脚本diff已覆盖。
