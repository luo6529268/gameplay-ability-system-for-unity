# Q07/C053 正式根 trace 命中事件回读更正

状态：`VERIFIED_SCOPED_ROOT_HIT_EVENT_OBSERVATION`。仅纠正已有 C053 单 Uj 自然 producer 案例的**根程序逐 hit 可见性**，不关闭双 Uj、C053、Q07 或总目标。

重新计算正式根 EXE SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。不重新运行已保存的40tick根 LFR或原Battle Scene，而是逐行重读[正式源码CSV](../NTSD28-336B44-Q07-C053-NATURAL-PRODUCER-001/source-run-y0/ank610-y0-jira500.csv)、[根trace](../NTSD28-336B44-Q07-C053-NATURAL-PRODUCER-001/ank610-root-v2-trace.jsonl)及其[PASS报告](../NTSD28-336B44-Q07-C053-NATURAL-PRODUCER-001/ank610-root-v2-report.json)。分析输入SHA、报告状态与选定事件保存在[机器比较](comparison.json)。报告`passed=true/failureCode=0/declaredTicks=40`；终端额外tick不计入比较。

对40个声明tick逐个计数，源码 `oid808_hit_count` 与根 `events.kind=hit && attacker=51 && target=50` 为 **40/40 相等**，两边唯一阳性均在tick7。源码该行给出 `51:0:2:156`（攻击槽51、候选0、effect2、Uj156），根同tick直接导出 `attacker=51,target=50,candidate=0,status=applied,hpDamage=25,mpDamage=0`。根tick7的slot50为OID808/action156/HP475；其同tick另有独立slot50→slot0的applied/HP90事件，未混入C053目标命中计数。

旧[自然 producer 报告](../NTSD28-336B44-Q07-C053-NATURAL-PRODUCER-001/REPORT.md)说“根trace不公开逐hit”过窄：**命中是否生效、槽位、候选及伤害确实在当前根trace可观察**。但根事件没有 `effect`、`Uj` 字段；`opointActionLatch=153`也不是源码CSV的`frame.action_latch`字段，不能把两者等同或把源码`effect2/Uj156`说成根直接导出。这里的root action156/HP475是同tick最终状态，不是第二次同tick命中证明。

因此后续C053单命中根可见性门已补，真正缺的仍是**正式可达双Uj同tick鉴别条件**、原 Unity Scene 每次命中的逐 writer 动作/锁存/HP 比较、物理键自然选招及全World/退出借用。已有正式源码受控双Uj和Unity受控场景局部证据各保持自己的边界；本次不把两项拼成正式根双Uj同态。

同批既存 `source-run-y0` 七个 X 站位 CSV 各40tick也逐行复核：X580/610/640/670/700各仅tick7一条目标Uj，X550/730为零，没有同tick双Uj阳性。因此下一步需构造新的**正式可达**双命中前驱或筛其它已证可达首差，不重复这七组输入。

本包只读原件并新增Task/JSON/报告及进度更正；无C++/C#、DAT、图片、Scene、相机、配置或非战斗修改，未使用computer-use，也未重跑测试矩阵。
