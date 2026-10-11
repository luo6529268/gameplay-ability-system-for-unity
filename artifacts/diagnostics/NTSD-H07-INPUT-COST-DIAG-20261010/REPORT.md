# H07-INPUT 角色输入阶段有限检测结果

Task：NTSD-H07-INPUT-COST-DIAG-20261010  
Change：NTSD-H07-INPUT-COST-DIAG-001  
结论：SCOPED_DIAGNOSIS_COMPLETED_WITH_LIMITS；ABBA_STOPPED_AFTER_OFF_GC。生产优化未实施。

## 已经确定的部分

1. **兼容动作入口存在当前配置下的条件空转。** DataOrientedCanonical、原生输入流水线拥有动作处理时，ApplyFrameInput先守卫返回、不执行旧组合技/直接动作/release；其外层仍读取17个进度字段，复制original，进入调用/计时，比较字段后才跳过提交。不是两套组合技被重复执行。
2. 新鲜千人混战180个steady tick，progress commit **0**，progress skip **180000**；每tick恰好1000 skip、0 commit，三个60tick块均相同。此范围可以提出针对性提前守卫方案，但不是所有角色/所有配置均能删除resolver。
3. ComboUpdate父段新鲜均值0.603016667ms，P95 1.015325ms，P99 1.466705ms。其子段ProgressCapture 0.114125ms、ComboDirectResolve 0.067603ms、ReleaseResolve 0.033399ms、ProgressCommit 0.120401ms。父子不相加；这些包含计时、调用、校验成本，**0.603ms不是可承诺的节省量**。
4. 真实AI决策不是“只看有没有按键”。当前RemainingAiDecision均值6.105529ms，内部IndexedCanonicalKernel 3.797896ms，其中NearestSearch 1.732797ms、SpecialSearch 0.174284ms；其它capture/validation/commit完成读取与结果发布。已有indexed查询，不能称每个AI完整遍历1000对象，也不能因有缓存目标就删除nearest搜索。
5. producer刷新每tick1000次，180tick合计180000次，复用skip0。**次数证实会执行，不能据此认定冗余**。producer后与native routing后是不同可见世代；既有producer复用USER_DEFERRED，本任务不恢复。

## 本次实际运行与证据边界

- 原Editor PID39108 / Unity2022.3.62f3 / 本地TCP6402复用，没有启动第二实例。开始、结束Menu均clean、8roots。
- 原Editor脚本刷新/编译/reload后idle，无C#编译错误；桥接断连日志保留，不能把其当脚本编译失败。execute_code的CodeDom命令长度错误原件保留；场景状态改用现成manage_scene get_active读取，没有修改桥接或编译器。
- 定向既有EditMode测试4/4 PASS：DataOrientedAiActionResolver_SkipsUnchangedProgressCommit；DataOrientedAiActionResolver_LeavesDirectFieldToNativeSecondPass；RegisteredAiActionResolver_ConsumesAndCommitsRuntimeProgressDirectly；NativeSampleTiming_DisableAndNextTickClearChildren。覆盖canonical空转、真正native动作消费、旧路径正向处理和计时关闭/清空。没有重跑44/全量10593项。
- 计划4个独立同初态combat1000窗口：OFF→ON→ON→OFF。四份冻结request除outputPath完全一致：Role、DataOrientedCanonical、1000 Sakura、seed1314149188、120warm+180sample。ON/OFF只切输入边界30..41槽；其它AI细分及外层计时保持同配置。不是全诊断关闭的生产纯耗时对照。
- **实际只完成首OFF**：120warm+180steady、300/180完整同步主线程Driver覆盖，actual flag失败0、真实fallback attack/body0且inert对账通过、输入观测有效、workloadValid true。未启动后三窗，磁盘后三份request不是运行证据。
- 新300tick与既有同fixture H06前置修复OFF逐tick完整RuntimeChecksum、输入数量、双RNG状态/次数/同步表hash共14字段全一致（300行0差异），请求除输出路径无差异。此为跨已留存基线的结果一致性，不是本次四窗ON/OFF一致性。
- 完整Driver GC前/后已知分配及空边界校准通过、invalid/reject/sequence0；steady在tick130、162各1个GC.Alloc，共2事件，**0GC资格FAIL**。raw总2000单位TimeNanoseconds，不是2000B，也不是GC停顿。没有新调用栈，不能把本轮两事件归因为此前Mono.JIT/FindEffect/95/producer。
- 首OFF非零GC触发预先声明停止门；不追加窗口、不滤事件、不改预热/门、不顺手追查分配。因此**本次没有细分计时开关扰动的可用ABBA估计**，不能跨历史12ms和当前15ms推断开关成本或性能退化。

## 当前计时读法

全部为180个steady样本的Stopwatch经过时间，含已声明诊断成本，不是独占CPU时间或Android结论。

| 范围 | 均值ms/tick | P95 | P99 | 关系 |
|---|---:|---:|---:|---|
| CharacterInput | 15.042183 | 27.387375 | 37.761139 | 整个输入阶段 |
| SnapshotBuild | 0.443754 | 0.612875 | 0.819383 | 上行子段 |
| RemainingAiDecision | 6.105529 | 10.069965 | 15.697594 | 输入内AI决策父段 |
| IndexedCanonicalKernel | 3.797896 | 5.890600 | 8.225893 | AI决策子段 |
| NearestSearch | 1.732797 | 2.771660 | 3.733839 | kernel子段 |
| ComboUpdate兼容入口 | 0.603017 | 1.015325 | 1.466705 | 输入内条件空转父段 |
| UnifiedSnapshotExecutionRowRefresh | 0.317887 | 0.655620 | 0.869782 | 后续AI可见行发布 |
| RefreshRuntimeSnapshot（后段） | 0.082057 | 0.138180 | 0.191769 | 不能冒称producer刷新成本 |

ProducerFreeze/ProducerSnapshotRefresh/ProxyApply/NativeSampledState及八个native子槽在OFF均0，意为未打开这些细分计时，**不是业务没有执行或没有成本**。兼容入口26..29槽及其它AI细分仍打开。正式整tick62.484600ms（P95 103.690690、P99 113.940059），drop590；本窗不满足H07 33ms/drop0最终门。HumanInput阶段0.256773ms，应用的玩家输入包条目0；AI1000窗口不证明玩家输入与AI各自都花12/15ms。

## 各部分如何处置

| 部分 | 诊断结论 | 下一实现策略/限制 |
|---|---|---|
| 原生拥有动作处理时的兼容resolver外壳 | 已确认本fixture全steady空转 | 优先讨论将native ownership守卫前移，减少17字段capture/copy/compare及无效调用；仅在canonical绑定有效时，保留旧/无效/未知类型路径及诊断计数语义；后续原生处理、frame velocity tail与发布不能跳过。尚未生产改码或证明收益。 |
| AI寻找目标、决策、结果提交 | 必要业务，有具体计时线索 | 不能作为纯检测删除。本轮不提出空间分区、缓存或降频新方向。 |
| 两个时点的快照/输入状态同步 | 可见边界不同，冗余未证 | 保留。producer复用继续暂缓，不能换名恢复。 |
| 逐角色细分计时 | 机制上存在额外调用；本次缺ON窗 | 当前不能量化开关扰动。旧91八native子段每角色16次timestamp的机制仍为历史结构依据，不能乘成当前毫秒收益。 |
| 完整Driver两次分配 | 本窗0GC资格未通过 | 独立挂账，不用它否认本次18万skip事实，也不自动启动另一个优化/分配修复任务。 |

没有把当前0.603ms父段包装成整tick收益。建议下一讨论只确认这一条件空转的准确提前守卫、fallback与最小回归；本Task不派发其它会话或启动生产修复。

## 关闭、保护与审计

输入边界开关恢复，组合observer解除；原十一阶段关闭完成，objects/slots/borrowers全0，Battle Scene磁盘不变，Menu恢复clean8。15562个范围外Assets/ProjectSettings/Packages文件逐字节SHA256均不变。两个Editor工具之外未修改生产代码、资源、设置或Scene。原dirty逐字节备份和hash在同Operation，旧失败证据保留。未恢复自动监测/旧Goal/34队列/H06/95或任何专项。

证据：[机器摘要](diagnosis-summary.json)、[首OFF report](windows/00-combat1000-off/report.json)、[逐tick输入观察](windows/00-combat1000-off/report.json.input-cost.json)、[完整Driver GC](windows/00-combat1000-off/report.json.logic-gc.json)、[Suite终态](windows/suite-result.json)、[定向测试](focused-result-01.json)、[保护](protected-after.json)、[冷请求比较](request-comparison.json)。最终Ledger与diff证据见validation.txt。
