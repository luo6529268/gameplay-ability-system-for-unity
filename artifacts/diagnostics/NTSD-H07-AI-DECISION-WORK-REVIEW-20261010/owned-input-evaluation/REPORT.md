# H07 owned-input evaluation — 完成有限评估，本版不采用

Task: NTSD-H07-OWNED-INPUT-EVALUATION-20261010 / COMPLETED_NO_DISCERNIBLE_GAIN
Change: NTSD-H07-OWNED-INPUT-EVALUATION-001 / VERIFIED（仅Editor诊断实现与本次有限证据，不是生产优化）
Parent: NTSD-H07-AI-DECISION-WORK-REVIEW-20261010；H07角色输入约12ms问题仍OPEN，没有新的可实施生产方案，不派发执行任务。

## 判断

既有SnapshotCopy与CanonicalStoreDirect两条路径在本次覆盖的行为中一致，但直读没有通过事前局部收益条件。停止此版，不接生产、不重复同组找PASS，不运行后继真实Sakura/完整Driver成本窗。
现有生产默认SnapshotCopy保持；不是因GC资格阻止测量。本次8窗/256测量pass均当前线程分配0B，成本组实际完成。
合成千人输入入口约8–10ms不能与旧真实Sakura的12.037463ms相减当收益：fixture、时间与程序集身份不同。

## 前提与改变

只新增H07OwnedInputEvaluationEditorTests.cs与Unity生成.meta。没有改World/Kernel/Store/Writer、生产默认、资源、Scene、ProjectSettings、编译优化模式或执行会话状态。
逐窗独立MobileExtended1050 World，先显式设置DataOrientedCanonical与SnapshotCopy/CanonicalStoreDirect，再注册1000角色。模式在ResetRuntimeState后可能保留；测试准确记录并丢弃World，不把reset误当模式还原。
实际调用NativeProducerSampleAndInputRouteAll(tick)，与生产CharacterInput阶段入口相同，包含两遍producer/routing，includeNonCharacterHitFa=true、applyFrameMotionTail=false；本fixture仅合成角色，不是Sakura完整实景。
seed1314149188，两个布局spacing120/12，各SnapshotCopy→Direct→Direct→SnapshotCopy，每窗8warm+32sample，40tick从相同初态演化。phase2、既有AI算法不改；这里World broadphase采用测试构造默认Brute，输入阶段被测AI仍UnifiedAuthority索引；不声称模拟了Role碰撞成本。全部fine timing/Profiler/其它候选关闭。
计时包住完整native输入调用及同构线程分配读取开销，校验/日志/序列化/反射在外。41字段采集为显式typed写入冷预分配数组，完整RuntimeChecksum从原API取得；附加输入hash不能替代该checksum。ABBA非自动因果证明，未猜抢占/降频/GC停顿。

## 实际验证

1. 原Editor PID39108 / Unity2022.3.62f3，原NTSD_Menu clean8。新增文件导入与最新编译/reload完成。
2. 首focused job fa5639253f7f437787dcc1fe05ef1b41 执行3项，发现夹具将per-character RunCount当per-tick，4角色预期1实际4；修正到4，旧失败原件保留，没有修改生产路径。
3. 最新focused job37494949cf914c62a09ae1ca666f968b，3/3 PASS：phase0/1/2跨tick，41字段canonical和runtime分别对照、完整checksum、legacy RNG/native9标量含generation/tablehash；冷默认/非法枚举/已注册拒绝/reset后模式保留；释放重绑旧owner/旧handle拒绝。它不证明所有AI分支、跨线程或注入post-commit异常。
4. 唯一成本job2d0f3c2d12de4b90902f77d833c636e7，1/1 PASS只表示组执行与硬正确性通过。8窗/各32sample完成；各窗40000 eligible/40000 commit/0 fallback/0 oracle；40tick全量校验，240个跨窗tick对照无差异，原数组逐41字段对照而非只hash。
5. 每窗runtime槽容量1050、1000注册；最后objects=slots=0。256个稳态native输入调用分配均0B，未把warm/全Unity或完整Driver称0GC。
6. Python独立重算raw Stopwatch ticks（frequency见JSON）的mean/线性插值P95/P99一致；独立核所有逐tick完整checksum、双RNG与输入hash一致；源码/两个程序集测前后SHA不变。

| spacing | 序号 | 实际mode | mean ms | P95 ms | P99 ms | 稳态分配 B |
|---|---:|---|---:|---:|---:|---:|
| 120 | 1 | SnapshotCopy | 8.347891 | 8.940230 | 9.097131 | 0 |
| 120 | 2 | CanonicalStoreDirect | 8.423769 | 9.503785 | 11.211890 | 0 |
| 120 | 3 | CanonicalStoreDirect | 8.380066 | 9.107245 | 9.443549 | 0 |
| 120 | 4 | SnapshotCopy | 8.616912 | 9.938140 | 10.958837 | 0 |
| 12 | 1 | SnapshotCopy | 8.157544 | 8.650225 | 9.018250 | 0 |
| 12 | 2 | CanonicalStoreDirect | 9.111237 | 12.022495 | 12.739291 | 0 |
| 12 | 3 | CanonicalStoreDirect | 10.033806 | 15.752530 | 19.534442 | 0 |
| 12 | 4 | SnapshotCopy | 8.513575 | 10.470925 | 11.582312 | 0 |

判据（测前已写Task）：每布局两个ON mean都低于两个OFF mean，且ON最大P95/P99不高于OFF最大值。两个布局均未达到；分散均值混在OFF波动中，密集两个ON在该组中更慢。不能据此证明跨场景普遍回退，也不能当作可辨优化收益。

## 保护、限制、出口

2659既有源码/Scene/资源/配置/设置保护文件0漂移；9份dirty文档备份有效；原Menu clean8/仅1Scene。成本前后3个源码/程序集身份不变。Console查询6条MCP Client handler exited（Exception分类），不是C#编译错误或本组测试失败，也不声称Console空。
旧12ms/完整Driver tick130/162分配失败与producer暂缓/compat候选OFF/95OFF分别保留，不套用旧Mono.JIT/bytes解释本组。原H07精确33ms、确定性、drop/backlog、完整Driver0GC、关闭与Android最终门没有通过或降低。
本候选评估完成；当前证据不足以形成H07生产优化实施指令。未向另一会话派任务，监测维持暂停。不要把该无收益方向再次包装成已确认优化点。
最终validator与diff结果追加见下方。

终审：Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <repo> 退出0/PASSED，保留旧路径的历史warnings；本轮diff-check退出0、新Editor文件无尾部空白。校验不等于生产优化通过。
