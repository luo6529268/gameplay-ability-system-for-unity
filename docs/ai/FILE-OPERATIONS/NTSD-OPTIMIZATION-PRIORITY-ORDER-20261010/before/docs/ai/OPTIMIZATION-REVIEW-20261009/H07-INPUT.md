# H07-INPUT：CharacterInput等价优化子方案

> 用户决定（2026-10-10）：87/88 producer复用 USER_DEFERRED / 生产OFF，原准入失败和ABBA GC失败保留，恢复条件见[单项记录](IMPLEMENTATION-RECORDS/H07-PRODUCER-SNAPSHOT-REUSE.md)。下一项选择[H06-ROLE-EXACT](H06-ROLE-EXACT.md)进入静态分析/方案；下方“由用户作决定”已有本次答复，不自动恢复旧候选。

本轮静态复核：当前CaptureSharedOwnedSnapshot已有unified/shared及direct-canonical分支；producer刷新后和native routing后的AI行/状态同步处于不同可见边界。不能把同名capture/sync当成同一份可直接删除的复制。91混战同窗IndexedCanonicalCapture约0.209ms、UnifiedSnapshotExecutionRowRefresh约0.245ms、NativeCanonicalAiSync约0.360ms、NativeInputStateSync约0.069ms；这些是历史嵌套计时，不相加、不当当前新鲜耗时或可节省量。IndexedCanonicalKernel约3.118ms（内含Nearest约1.507ms）也不能被上述省读整体覆盖。因此本轮不据“CharacterInput总12ms”继续提出无证微缓存；一般输入路线保留，优先核Role精确配对这块较大的已有热点。

> 最新（2026-10-10）：既有producer的新ABBA有限诊断已结束，复用仍OFF。重复ON均值波动15.518ms，未稳定复现回退也不能证明收益；首OFF可靠完整Driver稳态2event/tick130，调用点未知，其余三窗0，不能以旧窄域0覆盖。[新报告](../../../artifacts/diagnostics/NTSD-H07-PRODUCER-ABBA-DIAG-20261009/REPORT.md)。下一决定由用户作出，本方案不自动继续。

父项：H-07；与H-06、M-01共享成本背景，但不是新增第35父项。状态：ANALYSIS_REQUIRED / EXECUTION_STOPPED。建议高优先级，Role准入之后或其授权等待期间可独立只读分析。

## 本次具名准入结论（2026-10-09）

既有87/88生产者快照复用已按第二项具名授权完成当前判定：NOT_ADMITTED / DEFAULT_OFF。44+1+1测试及一次双布局四窗的正确性/完整Driver稳态0GC/容量/关闭通过；producer刷新两布局下降，但混战整tick50.428038→57.714222ms（+14.448675%），原因未闭合，未进入生产接线，不重复同版窗口。第一项Role限定准入完成，正常空配置默认Role保持。

[独立代码对照与结论](IMPLEMENTATION-RECORDS/H07-PRODUCER-SNAPSHOT-REUSE.md) · [完整报告](../../../artifacts/diagnostics/NTSD-H07-PRODUCER-SNAPSHOT-ADMISSION-20261009/REPORT.md)。下方一般CharacterInput优化仍为方案，不因本次授权自动继续；新路线/独立波动诊断/行为合同由用户决定。复用91相位归因、既有M09/M15指纹和H09影响域；保持publication只读、同tick字段失效与双RNG，不与AI降频/疏散捆绑。

## 事实与代码

91实际窗口CharacterInput约12.1—12.6ms；NativeSampledState八子阶段已测，Builtins约1.0—1.1ms不是全部输入成本。95 isolated输入12.5261→12.1755ms，仅约0.35ms局部信号，四块有一块回退。
已定位：Assets/NTSD/Scripts/Simulation/Input/NTSD28InputTwoPassModule.cs::ProcessNativeSampledState；Simulation/Ai/Runtime/SimulationAiDecisionModule.cs的RefreshCapture与AiDecisionKernel.TryEvaluate调用；Simulation/Ecs/Core/BattleAiUnifiedRowPublisher.cs；InputState.SyncFromRuntime消费口。现有SoA/DataOriented路径已存在，不能重建一套。
这些是候选调查范围，不是允许任意修改整个World/AI模块。

## 方法与有限出口

1. 先读91完整结果，画CharacterInput父子相位包含关系；同窗独占耗时才能合计。只补尚未覆盖的行捕获/感知/同步调用链，不能再重复八子阶段归因。
2. 逐字段列同tick读写时点、producer freeze、双state交换、State/HitStop等失效条件；查整块重复capture/同步。不能用每实体12微秒“感觉慢”代替成本证据。
3. 若同步/行发布主导，评估在原边界消除重复采样或更新，保留原fallback；若查询主导，再评估等价剪枝/数据访问布局。最近目标O(n²)尚未证实，tie-break/顺序/RNG保持。
4. 没有发现可解释且足够有价值的成本块则交付有据“不实施”结论，不开无证微缓存。95不自动成为必须续完的候选。

## 依赖、验证与结束

依赖M-09/M-15具名基线、H-09比较范围和现有数据发布合同；不依赖M-11全关闭、Role默认推广或95 GC修复全部完成。若所选fixture受95问题影响，则先修该fixture/生产调用点，不忽略0GC。
最小验证：受影响字段及边界focused/逐tick完整输入、state、数组索引、双RNG；有效完整Driver0GC校准；同指纹双布局必要A/B。复用原37/正式root及91归因中未受影响结果。必要时检验差异不是顺序环境噪声。
方案阶段完成：明确一个具名成本块、准确改法/写域/失效表/验证，或证据证明不值得继续。
实施阶段完成：候选实际减少声明成本，整tick不出现未解释回退，正确性/0GC/容量/关闭门通过。不承诺input<=6ms；H07<33ms最终门独立保留。
风险：按脏更新漏状态、旧source/physical坐标混读、published/scratch身份判断错误、RNG顺序变化。代码前新Task/Change及dirty备份，原canonical路径保留；回滚仅具名增量并遵守操作授权。
