# H07输入尖峰CPU证据补充
Task ID: NTSD-H07-INPUT-SPIKE-CPU-DIAG-20261010
状态：SCOPED_DIAGNOSIS_COMPLETE / BOUNDED_REPAIR_READY
用户直接授权：“那还不够那就继续检验，直到可以发送给另一个会话进行修复”。当前讨论会话持续检测H07-INPUT，成熟后才派准确修复，不另开优化方向。
先复用上一raw，冷解析主线程全部可达tick的输入父/子、GC/JIT/等待样本，不读Q06体，不重捕获。
必要增量三写域：Suite冷入口/独立两窗/诊断标签；H07InputCostDiagnostics原owner组合Windows主线程CPU时间/原始周期与GC.CollectionCount，及同raw窄导出；SimulationWorld.DetailTimingDiagnostics.cs只BattleAiInputDetailDiagnostics类默认OFF可选周期provider、7段累计/计数/invalid/reset，原42槽不重编号，禁止写World生产pass/Query/AI/动作逻辑。
CPU周期只raw counts，不换ms；GetThreadTimes单位100ns、实际量化另报，不能精确反推全部waiting/频率。冷空边界校准及focused先过。
唯一新一组同combat1000/1000Sakura/Role/DataOrientedCanonical/seed1314149188：OFF周期provider→ON周期provider，各120warm+180steady；两窗均无Profiler、全输入细分ON，其它候选OFF；完整GC校准/300-180覆盖/事件不滤，明确DIAGNOSTIC_NOT_CERTIFICATE。当前非零GC不得当资格通过；本组为已知旧JIT背景下的归因，允许记录两窗非零，但任何校准/正确性/配置/容量/关闭失败阻止第二窗。这不是放宽生产0GC门。
两个Windows API只读当前线程、不调优系统/电源/亲和，不杀进程；无全局ETW session、资源或包设置改动。周期provider幂等解除，assembly reload/异常finally恢复；原11阶段关闭保持。新数组固定42与300冷预备，不热分配。
成功出口：对尖峰给出测量支持的解释/局限；只派一个与既有H07方案一致、代码/guard/影响/最小验证/回退明确的修复范围。修复实现和收益验收归执行会话，不以诊断数据承诺毫秒收益。证据不足则继续当前点的准确最小缺口而非换点/同版找PASS；不降低确定性/0GC/33ms/Android合同。
生产者复用USER_DEFERRED/OFF、H06不续、95OFF、PERF/ATLAS/Mono/EXT-1不解冻，不读活跃Q06体。
回滚只本Task三域增量，原dirty备份需用户授权才回退，禁止HEAD覆盖。

结案：REPORT.md / cpu-analysis.json / retained-raw-ticks.json / audit-summary.json / execution-handoff.txt 已生成；5/5 focused、600tick正确性、15562保护无漂移。诊断实现VERIFIED不等于生产0GC/收益通过，首窗2event保留；可实施下一有限guard候选，尚待实际派发回执。

派发完成：同report执行指令已发thread 01a1201b-7930-7c93-8c84-5bc7baada43f；新turn 01a12421-e651-7e90-802d-06895c559be9已active，监测恢复ACTIVE。上文“尚待派发”为派发前时点；此诊断Task完成，生产候选由新Task执行。
