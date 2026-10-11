# H07角色输入成本检测
Task ID: NTSD-H07-INPUT-COST-DIAG-20261010
状态：SCOPED_DIAGNOSIS_COMPLETED_WITH_LIMITS / ABBA_STOPPED_AFTER_OFF_GC
授权：用户当前直接授权：那就先进性检测处理，确定哪些部分，这部分你来处理就行。仅H07-INPUT检测，不修改生产优化逻辑。
所属既有方案：docs/ai/OPTIMIZATION-REVIEW-20261009/H07-INPUT.md。
仅两Editor写域：Suite冷模式/request/first-tick/finish/异常关闭，H07InputCostDiagnostics.cs及Unity生成meta。生产Query/World/AI/writer不改，不读Q06体。
检测1：现有canonical兼容入口未变更进度/动作，已有正向native动作消费对照，最窄定向复验。
检测2：固定当前Role/DataOrientedCanonical/Sakura1000/seed1314149188/120+180，各独立同初态OFF→ON→ON→OFF，仅EnableCharacterInputBoundaryTimingForDiagnostics不同，AI其余细分和外层计时保持相同。整tick及CharacterInput时间按经过时间解释，诊断差不作生产收益。逐tick完整RuntimeChecksum和输入数量/双RNG状态计数同步表hash一致、正确性与配置门、真实fallback与inert区别。
复用旧91成本结构，不重跑旧双布局/八阶段归因/1800；本次ON只为同组比较，关开内波动/3x60块均记录，不机械扣底噪。
GC同步主线程完整Driver前后校准，所有120+180记录；有事件不称0GC通过，不追溯旧问题或顺手修复。原Suite安全/完整性/容量/清理门保留，失败停止后继；不无限重试。本Task是诊断，不是推广/性能证书。
输出：当前兼容跳过/提交和producer刷新计数、同组计时效应或不足、字段可见边界判定（必要/条件空转/未证冗余），准确REPORT；给出后继可修范围，不执行生产修复/派任务。
所有红线和暂缓保持。Owner/回滚/保护见同Operation。

## 实际收口

4/4 focused通过；首OFF120+180完整，180000 skip/0 commit，ComboUpdate父段0.603017ms；所有300tick与同fixture旧基线14字段0差异，配置/容量/关闭通过。完整Driver steady2event/tick130、162，因此停止后三窗。具体分配来源和细分计时扰动未获本次证据，不追查/重跑。REPORT及diagnosis-summary.json见同名artifact目录。仅当前检测收口，生产策略待讨论，无自动任务派发。
