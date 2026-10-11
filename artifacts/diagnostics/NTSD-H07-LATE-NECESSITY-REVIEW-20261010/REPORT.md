# H07-LATE 必要性评审：本轮暂不实施

Task: NTSD-H07-LATE-NECESSITY-REVIEW-20261010
Status: SCOPED_REVIEW_COMPLETE / NO_JUSTIFIED_OPTIMIZATION_NOW

## 结论

目前没有足够依据把LateEntityUpdate立为新的生产优化任务。用户认为阶段耗时尚可，本轮已按源码及既有实测审查，不因阶段存在5–6ms成本就自动追加实验。初始“可能重复整理/复制快照”的假设，在已检查千人场景的被监测Late快照位置上不成立。不是证明代码已最优，也不是H07整tick/设备验收通过。
不修改生产、不运行新Unity测试/Profiler/性能组、不派执行任务、监测保持暂停。不自动挑下一点。

## 实际游戏职责与入口

NTSDBattleTickSystem.cs:543-545、787-789 → SimulationWorld.LateEntityUpdateAll:3600 → BattleLateEntityLifecycleModule.Run:48。
按当前源码，它在每个runtime slot内执行特殊状态/克隆分支、HP/MP恢复、native显示计数与后显示资源处理、动作帧事务、硬直/防御/异常状态等计时、护甲恢复/攻击间隔、opoint生成、上一动作镜像、武器破碎/生命周期退出、尾部输入处理、治疗；最后提交延迟实体变更。
它不是纯表现拷贝。画面没有移动也可能存在计时减少、恢复、持续伤害或动作等待推进，不能直接按“画面不变”跳过。

准确读到的关键位置：
- BattleLateEntityLifecycleModule.cs:95-146，已存在state-special/recovery no-op gates；:154-212负责frame事务及反应状态尾部；:233-355为opoint/cleanup/exit/tail/healing/snapshot；:358-369最终mutation提交。
- BattleEcsCharacterFrameTickPass.cs:66-93默认DataOriented，exact角色直接RunNativeC25FrameBodyForWorldPass。
- LF2Entity.cs:6594-6597在late transaction active时进入RunNativeC25FrameTransaction:6748。已核该真实分支，包括wait/next、计数、声音、状态转换及随机下一帧；不以同方法后面的legacy分支代表当前生产。
- BattleEcsCharacterRecoveryPass.cs:84-121，非恢复周期且无负环境恢复时已有ProvenNoOp，不建议重复加同一早退。

## 已有测量复用，而非本轮重测

原91四窗均120warm+180steady、细分计时开启，Late均值依次7.605826/5.945469/5.957106/5.777792ms。此前约5.8–5.9ms引用的是其中两个ON窗；它们的开关目标是其它输入候选，不能把Late跨窗差当本项收益。
原91混战ON细分：FrameTick1.453304ms、Recovery0.650679ms、TailAndQueuedFlush0.669261ms；其它已监测子段各约0.10–0.17ms。父子和相位间剩余差不能相加/直接归因；逐实体Stopwatch与未包围的native显示/资源等工作存在，未闭合独占CPU成本。

更近的既有无逐角色细分窗口（不是本轮测量）：
`artifacts/diagnostics/NTSD-H07-NATIVE-OWNED-COMPAT-SKIP-20261010/windows/00-combat1000-off/report.json`
Late均值4.914502ms、P95 10.012780ms、P99 14.469791ms、max17.4024ms，180steady；原完整Driver均值51.481118ms与GC失败分别保留。不能把历史5.8ms到4.91ms称优化收益，因为运行时间/配置/测量环境未构成因果A/B。平均成本本身不证明全局性能合格，尾部波动原因也未查明；但当前没有据此可实施的修复。

## 原“重复快照”假设核对

1. 当前Late模块默认ConsolidatedFinal，LegacyThree下FrameTick/DeathOpoint的两次额外刷新不在默认路径。
2. LF2Entity.RequiresRuntimeSnapshotAfterLateEntityUpdate:7359-7363对exact LF2Character且ObjType/EntityType/Frame/WaitCounter/NextFrame已一致时跳过尾部刷新；派生或不一致保留fallback。
3. 原91四窗及较近INPUT-SPIKE-CPU首OFF这五份已有细分报告，10个Late RefreshRuntimeSnapshot统计位置均available、callCount0、平均0。源码计数BeginLateRuntimeSnapshot:375-387与Harness:3694-3705对应；它只证明这些监测位置/对应fixture，不证明整个World无复制，或所有实体/场景均不刷新。
4. 无细分窗口中的0不能当独立快照调用证据，因为计数依赖诊断开关。因此只用上面五份有效细分报告支撑该假设排除。

## 剩余小开销与为什么不立新任务

- 尾部wrapper仍逐角色调用。当前LF2Character override → LF2CharacterLateRuntimeModule.RunLateN30InputTrigger，slot>=10已立即返回；前10slot有实际条件逻辑，不能把整个虚拟tail称空方法。既有ForceLegacyTailNoOp/CanSkipExactCharacterTail本来就存在，不把启用旧候选换名为新设计。本轮不裁决N30历史规则对齐或修改它。
- 每实体FlushTasks保持生成可见边界；当前LF2ObjectPointFactory.FlushTasks:157-173在接受请求且空队列时只读取Count并执行0次循环。若有任务，则处理调用开始时的队列长度；不能挪到整帧末尾。调用次数多不等于已证实高耗时，合并后还可能改变本tick后续slot可见实体。
- 空slot仍分别重读native显示/资源路径。没有证明可合并读或缩循环不影响动态生成/注销时点，不能以空slot数量直接承诺收益。
- frame/recovery等较大子段包含实际状态变化，没有在本轮发现可安全整段删除且尚未优化的多余工作。

仅当以后出现“明确调用/字段重复+真实高频和成本+等价边界”新证据，或本阶段被证实造成不可接受尾延迟时才重新评审；不为寻找PASS原样重复窗口。

## 执行与证据边界

主代理只读当前源码、既有6份报告与相关治理文档，Python离线抽取数字；Hume只读3份历史Change。侧审历史retirement/fixture结论没有当成当前生产或收益证明，以当前override和模块链为准。
新建本Task/报告/evidence及Operation，只给六个已有文档追加检查点、真实dirty备份六份。没有脚本改动，Change Record/编译/测试/Validate-ChangeLedger不适用本轮；没有声称fresh runtime pass。原Unity会话、Scene、资源、设置均未操作。
源码与所用报告17文件SHA前后核验、原文后缀/备份核验和声明文档git diff --check结果见audit.json与diff-check.txt。

终审通过：六份备份有效，六份文档原始字节连续完整保留（INDEX前后追加，其余五份只前置）；17已读源码/报告0漂移；最终diff-check0。未重新运行任何战斗测试或测量。
